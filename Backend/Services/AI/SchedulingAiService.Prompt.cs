using System.Text.Json;
using System.Text.RegularExpressions;
using Backend.Constants;
using Backend.DTOs.AcademicSchedulingContext;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.AI;

public partial class SchedulingAiService
{
    public async Task<AiSchedulingInterpretResponse> InterpretIntentAsync(AiSchedulingInterpretRequest request,
        CurrentUserContext? currentUser, CancellationToken cancellationToken = default)
    {
        if (currentUser == null) throw new ApiException(401, "Cần đăng nhập để tra cứu lịch.");
        if (currentUser.Role is not (AuthRoles.AcademicStaff or AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin))
            throw new ApiException(403, "Bạn không có quyền sử dụng trợ lý xếp lịch.");
        var campusId = currentUser.Role == AuthRoles.SuperAdmin ? request.CampusId ?? currentUser.CampusId : currentUser.CampusId;
        if (campusId <= 0 || (request.CampusId.HasValue && request.CampusId != campusId))
            throw new ApiException(403, "Không có quyền truy cập cơ sở này.");
        if (string.IsNullOrWhiteSpace(request.Message)) throw new ApiException(400, "Hãy nhập câu hỏi hoặc yêu cầu.");

        var context = await _schedulingContextService.GetContextAsync(campusId, cancellationToken);
        var termId = request.SemesterId ?? context.SchedulableTerm?.MaHocKy ?? context.CurrentTerm?.MaHocKy;
        var term = await _context.HocKys.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == termId && x.MaDonVi == campusId, cancellationToken);
        if (term == null) throw new ApiException(400, "Hãy chọn học kỳ thuộc cơ sở của bạn để tra cứu.");
        var campusName = await _context.DonVis.Where(x => x.MaDonVi == campusId).Select(x => x.TenDonVi).FirstOrDefaultAsync(cancellationToken) ?? "";

        // Query courses
        var courses = await _context.KhoaHocs.AsNoTracking()
            .Where(x => x.MaHocKy == term.MaHocKy && x.MaDonVi == campusId && x.TrangThai != "luu_tru")
            .Select(x => new { x.MaKhoaHoc, x.TieuDe })
            .ToListAsync(cancellationToken);

        // Query shifts
        var shifts = await _context.CaHocs.AsNoTracking().ToListAsync(cancellationToken);
        var shiftMap = shifts.ToDictionary(x => x.MaCaHoc);
        var eveningIds = shifts.Where(IsEvening).Select(x => x.MaCaHoc).ToHashSet();
        var morningIds = shifts.Where(x => NormalizeText(x.Buoi + " " + x.TenCa).Contains("sang") || x.GioBatDau < new TimeOnly(12, 0)).Select(x => x.MaCaHoc).ToHashSet();
        var afternoonIds = shifts.Where(x => NormalizeText(x.Buoi + " " + x.TenCa).Contains("chieu") || (x.GioBatDau >= new TimeOnly(12, 0) && x.GioBatDau < new TimeOnly(17, 30))).Select(x => x.MaCaHoc).ToHashSet();
        var courseMap = courses.ToDictionary(x => x.MaKhoaHoc, x => x.TieuDe);

        // Query active rooms
        var activeRooms = await _context.PhongHocs.AsNoTracking()
            .Where(x => x.MaDonVi == campusId && x.TrangThaiPhong == "hoat_dong")
            .Select(x => new { x.MaPhong, x.TenPhong, x.SucChua })
            .ToListAsync(cancellationToken);

        // Query active teachers count
        var teacherCount = await _context.NguoiDungs.AsNoTracking()
            .Where(x => x.MaDonVi == campusId && x.TrangThai == "hoat_dong" && (x.VaiTroChinh == "giang_vien" || x.VaiTroChinh == "Teacher" || x.VaiTroChinh == "giao_vien"))
            .CountAsync(cancellationToken);

        // Schedule rows
        var rows = new List<SchedulePromptRow>();
        var source = "published";
        if (request.DraftId.HasValue)
        {
            var job = await _context.ScheduleGenerationJobs.AsNoTracking().FirstOrDefaultAsync(x =>
                x.DraftId == request.DraftId && x.MaDonVi == campusId && x.MaHocKy == term.MaHocKy, cancellationToken);
            if (job == null) throw new ApiException(404, "Không tìm thấy bản nháp trong học kỳ và cơ sở này.");
            source = "draft";
            rows = await _context.ScheduleDraftItems.AsNoTracking()
                .Where(x => x.MaJob == job.MaJob && x.TrangThai == "xep_duoc" && x.MaCaHoc.HasValue)
                .Select(x => new SchedulePromptRow
                {
                    CourseId = x.MaKhoaHoc,
                    ShiftId = x.MaCaHoc!.Value,
                    Day = x.ThuTrongTuan ?? 0,
                    RoomId = x.MaPhong ?? 0
                })
                .ToListAsync(cancellationToken);
        }
        else
        {
            rows = await _context.ThoiKhoaBieus.AsNoTracking()
                .Where(x => x.KhoaHoc != null && x.KhoaHoc.MaDonVi == campusId && x.KhoaHoc.MaHocKy == term.MaHocKy && x.TrangThai == "da_xuat_ban")
                .Select(x => new SchedulePromptRow
                {
                    CourseId = x.MaKhoaHoc,
                    ShiftId = x.MaCaHoc,
                    Day = x.ThuTrongTuan,
                    RoomId = x.MaPhong
                })
                .ToListAsync(cancellationToken);
        }

        // Enrich schedule details in memory
        var roomMap = activeRooms.ToDictionary(x => x.MaPhong, x => x.TenPhong);
        foreach (var r in rows)
        {
            r.CourseTitle = courseMap.GetValueOrDefault(r.CourseId, "Khóa học");
            r.SubjectName = r.CourseTitle;
            var s = shiftMap.GetValueOrDefault(r.ShiftId);
            r.ShiftName = s?.TenCa ?? "";
            r.StartTime = s != null ? s.GioBatDau.ToString("HH:mm") : "";
            r.EndTime = s != null ? s.GioKetThuc.ToString("HH:mm") : "";
            r.RoomName = roomMap.GetValueOrDefault(r.RoomId, "Phòng học");
        }

        var drafts = await _context.ScheduleGenerationJobs.AsNoTracking()
            .Where(x => x.MaDonVi == campusId && x.MaHocKy == term.MaHocKy && x.TrangThai == "draft")
            .OrderByDescending(x => x.NgayTao)
            .Select(x => new { x.DraftId, x.NgayTao, x.TongCourse, x.SoXepDuoc, x.SoKhongXepDuoc })
            .Take(5).ToListAsync(cancellationToken);

        var issues = context.Readiness?.BlockingIssues.Select(x => x.Message).ToList() ?? new();
        var isSchedulableTerm = context.SchedulableTerm?.MaHocKy == term.MaHocKy;
        var canPrepare = context.CanPrepareSchedule && isSchedulableTerm && courses.Count > 0;
        var totalWeeklySessions = rows.Count;
        var distinctCoursesCount = rows.Select(x => x.CourseId).Distinct().Count();
        var morningWeeklySessions = rows.Count(x => morningIds.Contains(x.ShiftId));
        var afternoonWeeklySessions = rows.Count(x => afternoonIds.Contains(x.ShiftId));
        var eveningWeeklySessions = rows.Count(x => eveningIds.Contains(x.ShiftId));
        var saturdayWeeklySessions = rows.Count(x => x.Day == 7);

        var dayMap = new Dictionary<int, string>
        {
            { 2, "Thứ 2" }, { 3, "Thứ 3" }, { 4, "Thứ 4" },
            { 5, "Thứ 5" }, { 6, "Thứ 6" }, { 7, "Thứ 7" }, { 1, "Chủ Nhật" }
        };

        var dayDistribution = rows.GroupBy(x => x.Day)
            .OrderBy(g => g.Key)
            .Select(g => new { Day = dayMap.GetValueOrDefault(g.Key, $"Thứ {g.Key}"), Count = g.Count() })
            .ToList();

        var usedRooms = rows.Where(x => !string.IsNullOrWhiteSpace(x.RoomName))
            .Select(x => x.RoomName).Distinct().Take(15).ToList();

        var normalizedMessage = NormalizeText(request.Message);
        if (IsEveningScheduleQuestion(normalizedMessage))
        {
            return BuildEveningQueryResponse(request, campusId, campusName, term, totalWeeklySessions,
                eveningWeeklySessions, courses.Count, rows, eveningIds, courseMap, shiftMap, source);
        }

        if (IsEveningRemovalRequest(normalizedMessage, request.History))
        {
            var validationErrors = canPrepare
                ? new List<string>()
                : BuildPrepareValidationErrors(context, term, issues);
            return new AiSchedulingInterpretResponse
            {
                Intent = "prepare_schedule",
                Summary = "Tôi sẽ tạo bản nháp thời khóa biểu mới và loại toàn bộ ca tối khỏi danh sách ca có thể xếp. Hãy bấm xác nhận để bắt đầu tạo bản nháp.",
                ExcludeEvening = true,
                RequestedPreferences = ["Loại toàn bộ ca tối"],
                UnsupportedPreferences = [],
                RequiresConfirmation = true,
                CanPrepareSchedule = canPrepare && validationErrors.Count == 0,
                CampusId = campusId,
                CampusName = campusName,
                SemesterId = term.MaHocKy,
                SemesterName = term.TenHocKy,
                SchedulableCourseCount = courses.Count,
                ValidationErrors = validationErrors
            };
        }

        var schema = AiOutput.Schema(new
        {
            intent = new { type = "string", @enum = new[] { "query_schedule", "query_readiness", "prepare_schedule", "clarify", "unsupported" } },
            summary = new { type = "string", description = "Câu trả lời giải đáp đầy đủ chi tiết cho người dùng dựa trên facts, tuyệt đối không lặp lại câu hỏi của người dùng" },
            excludeEvening = new { type = "boolean" },
            requestedPreferences = new { type = "array", items = new { type = "string" } },
            unsupportedPreferences = new { type = "array", items = new { type = "string" } }
        }, "intent", "summary", "excludeEvening", "requestedPreferences", "unsupportedPreferences");

        const string system = """
            Bạn là trợ lý AI chuyên môn về Xếp lịch và Quản lý Thời khóa biểu của trường học.
            Hãy đọc kỹ câu hỏi/yêu cầu mới nhất cùng lịch sử hội thoại và bộ dữ liệu thực tế (facts) để trả lời chính xác, thông minh bằng tiếng Việt chuyên nghiệp.

            QUY TẮC BẮT BUỘC VỀ TRƯỜNG "summary":
            - Trường "summary" CHÍNH LÀ NỘI DUNG CÂU TRẢ LỜI GIẢI ĐÁP HOÀN CHỈNH gửi cho người dùng.
            - TUYỆT ĐỐI KHÔNG lặp lại câu hỏi của người dùng. TUYỆT ĐỐI KHÔNG đặt câu hỏi ngược lại cho người dùng.
            - Phải đưa ra câu trả lời trực tiếp kèm các số liệu thực tế cụ thể từ facts (số môn học, số phòng học, số ca học, số giảng viên).

            BẠN PHẢI PHÂN BIỆT RÕ CÁC Ý ĐỊNH (INTENT):
            1. "query_readiness" (Kiểm tra điều kiện xếp lịch):
               - Chọn ý định này khi người dùng hỏi học kỳ đã đủ điều kiện xếp lịch chưa, có sẵn sàng xếp lịch không, cần chuẩn bị gì, có thiếu phòng học, giảng viên hay vướng mắc gì không (ví dụ: "hiện đã đủ điều kiện xếp lịch chưa?", "kỳ này đã xếp lịch được chưa?", "kiểm tra xem có thiếu phòng học hay giảng viên gì không").
               - Trả lời dựa trên facts.readiness:
                 * Nếu canPrepareSchedule = true: Khẳng định rõ ràng học kỳ ĐÃ ĐỦ ĐIỀU KIỆN để xếp lịch. Liệt kê các số liệu đã sẵn sàng: số lượng khóa học cần xếp (schedulableCourseCount), số phòng học hoạt động (activeRoomCount), số ca học (activeShiftCount), số giảng viên (activeTeacherCount), và không có vướng mắc nào cản trở. Nhắc người dùng có thể gửi yêu cầu xếp lịch hoặc bấm nút tạo bản nháp.
                 * Nếu canPrepareSchedule = false: Nêu rõ học kỳ CHƯA ĐỦ ĐIỀU KIỆN xếp lịch. Giải thích cụ thể các nguyên nhân cản trở từ blockingIssues hoặc reasonMessage (ví dụ: chưa đến thời gian xếp lịch, học kỳ đang bị khóa, thiếu phòng học, chưa có môn học...). Hướng dẫn các bước khắc phục.
               - KHÔNG tự động đề cập đến ca tối khi trả lời câu hỏi về điều kiện xếp lịch.

            2. "query_schedule" (Tra cứu thời khóa biểu đã công bố hoặc bản nháp):
               - Chọn ý định này khi người dùng hỏi thông tin, thống kê, kiểm tra về thời khóa biểu (ví dụ: "lịch đã công bố có bao nhiêu môn học và phân bổ thế nào?", "lịch có ca tối nào không?", "thứ 7 có lớp nào học không?", "danh sách phòng học đã xếp").
               - Trả lời dựa trên facts.scheduleSummary:
                 * Nếu hasSchedule = false: Báo rõ học kỳ này hiện chưa có thời khóa biểu công bố chính thức (hoặc bản nháp chưa có dữ liệu).
                 * Nếu hasSchedule = true: Phân tích chi tiết theo đúng câu hỏi: nêu tổng số môn học đã xếp (distinctScheduledCoursesCount), tổng số buổi học/tuần (totalWeeklySessions), phân bổ ca sáng/chiều/tối, phân bổ các ngày trong tuần (dayDistribution), phòng học sử dụng (usedRooms) và ví dụ từ samples.
                 * Trả lời chính xác số ca tối dựa trên facts.scheduleSummary.eveningWeeklySessions. Nếu bằng 0 thì nêu rõ không có ca tối nào. Chỉ nói về ca tối NẾU người dùng hỏi về ca tối hoặc khi tổng kết toàn diện về phân bổ ca học. Tuyệt đối KHÔNG tự động lái mọi câu hỏi về chủ đề ca tối.

            3. "prepare_schedule" (Yêu cầu khởi tạo hoặc xếp lại lịch):
               - CHỈ chọn intent này khi người dùng ĐƯA RA YÊU CẦU THỰC HIỆN XẾP LỊCH hoặc TẠO BẢN NHÁP (ví dụ: "hãy xếp lịch", "xếp lịch giúp tôi", "bỏ ca tối đi và xếp lại", "tạo bản nháp mới").
               - Nếu người dùng yêu cầu loại ca tối (ví dụ: "bỏ ca tối", "không xếp ca tối"), đặt excludeEvening = true và ghi nhận vào requestedPreferences.
               - Tóm tắt phương án xếp lịch, số môn học sẽ xếp và thông báo cần người dùng bấm nút xác nhận để bắt đầu chạy thuật toán.
               - Nếu người dùng chỉ đang HỎI ("lịch có ca tối không?", "đã đủ điều kiện chưa?"), ĐÂY LÀ CÂU HỎI (query), TUYỆT ĐỐI KHÔNG chọn prepare_schedule.

            4. "clarify": Khi câu hỏi quá ngắn hoặc mơ hồ, đề nghị người dùng làm rõ.
            5. "unsupported": Khi yêu cầu vượt quá khả năng hệ thống hoặc vi phạm quy chế (ví dụ: yêu cầu tự ý đổi giáo viên, tự sửa DB, xóa dữ liệu). Ghi lý do vào unsupportedPreferences.

            NGUYÊN TẮC:
            - Trả lời đúng trọng tâm câu hỏi, dùng văn phong giáo vụ chuẩn mực, mạch lạc.
            - Căn cứ 100% vào số liệu trong facts, không tự bịa đặt hay suy đoán số liệu.
            """;

        var payload = JsonSerializer.Serialize(new
        {
            message = request.Message.Trim(),
            history = request.History.TakeLast(8),
            facts = new
            {
                campusName,
                semesterName = term.TenHocKy,
                semesterId = term.MaHocKy,
                source,
                readiness = new
                {
                    canPrepareSchedule = canPrepare,
                    isSchedulableTerm,
                    schedulableTermName = context.SchedulableTerm?.TenHocKy,
                    currentTermName = context.CurrentTerm?.TenHocKy,
                    reasonMessage = context.ReasonMessage,
                    blockingIssues = issues,
                    schedulableCourseCount = courses.Count,
                    activeRoomCount = activeRooms.Count,
                    activeShiftCount = shifts.Count(x => x.ConHoatDong),
                    morningShiftsCount = shifts.Count(x => x.ConHoatDong && morningIds.Contains(x.MaCaHoc)),
                    afternoonShiftsCount = shifts.Count(x => x.ConHoatDong && afternoonIds.Contains(x.MaCaHoc)),
                    eveningShiftsCount = shifts.Count(x => x.ConHoatDong && eveningIds.Contains(x.MaCaHoc)),
                    activeTeacherCount = teacherCount,
                    readinessChecklist = context.Readiness?.Items?.Select(i => new { i.Code, i.Status, i.Message }).ToList() ?? new()
                },
                scheduleSummary = new
                {
                    sourceName = source == "draft" ? "Bản nháp được chọn" : "Thời khóa biểu đã công bố",
                    hasSchedule = rows.Count > 0,
                    totalWeeklySessions,
                    distinctScheduledCoursesCount = distinctCoursesCount,
                    totalCoursesInSemester = courses.Count,
                    morningWeeklySessions,
                    afternoonWeeklySessions,
                    eveningWeeklySessions,
                    saturdayWeeklySessions,
                    dayDistribution,
                    usedRooms,
                    samples = rows.Take(30).Select(x => new
                    {
                        course = !string.IsNullOrWhiteSpace(x.SubjectName) ? x.SubjectName : x.CourseTitle,
                        className = x.ClassName,
                        teacher = x.TeacherName,
                        room = x.RoomName,
                        day = dayMap.GetValueOrDefault(x.Day, $"Thứ {x.Day}"),
                        shift = x.ShiftName,
                        time = $"{x.StartTime}-{x.EndTime}"
                    })
                },
                availableDrafts = drafts
            }
        }, AiOutput.JsonOptions);

        SchedulePromptAnswer parsed;
        try
        {
            parsed = AiOutput.Parse<SchedulePromptAnswer>(await _ollamaService.CompleteAsync(system, payload, schema, "fast", 900, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama CompleteAsync failed or timed out. Falling back to structured academic answer.");
            parsed = BuildFallbackAnswer(normalizedMessage, canPrepare, term, courses.Count, activeRooms.Count,
                shifts.Count(x => x.ConHoatDong), teacherCount, issues, context.ReasonMessage ?? "", totalWeeklySessions,
                distinctCoursesCount, morningWeeklySessions, afternoonWeeklySessions, eveningWeeklySessions, source);
        }

        parsed.RequestedPreferences ??= new();
        parsed.UnsupportedPreferences ??= new();

        // If user asked a question ending with '?' or 'không'/'chưa', reclassify prepare_schedule to appropriate query
        if (parsed.Intent == "prepare_schedule" && Regex.IsMatch(normalizedMessage, @"\?|\b(khong|chua)\s*[.!]*$"))
        {
            if (Regex.IsMatch(normalizedMessage, @"\b(du dieu kien|san sang|xep duoc chua|co the xep|dieu kien)\b", RegexOptions.IgnoreCase))
            {
                parsed.Intent = "query_readiness";
            }
            else
            {
                parsed.Intent = "query_schedule";
            }
        }

        // Guard against model echoing the user's question back as summary instead of answering
        var isEchoQuestion = parsed.Summary.Trim().EndsWith("?") && (parsed.Intent == "query_readiness" || parsed.Intent == "query_schedule");

        if (parsed.Intent is not ("query_schedule" or "query_readiness" or "prepare_schedule" or "clarify" or "unsupported") 
            || string.IsNullOrWhiteSpace(parsed.Summary)
            || isEchoQuestion)
        {
            parsed = BuildFallbackAnswer(normalizedMessage, canPrepare, term, courses.Count, activeRooms.Count,
                shifts.Count(x => x.ConHoatDong), teacherCount, issues, context.ReasonMessage ?? "", totalWeeklySessions,
                distinctCoursesCount, morningWeeklySessions, afternoonWeeklySessions, eveningWeeklySessions, source);
        }

        var prepare = parsed.Intent == "prepare_schedule";
        var prepareValidationErrors = new List<string>();
        if (prepare && !canPrepare)
            prepareValidationErrors = BuildPrepareValidationErrors(context, term, issues);

        return new AiSchedulingInterpretResponse
        {
            Intent = parsed.Intent,
            Summary = parsed.Summary,
            ExcludeEvening = prepare && parsed.ExcludeEvening,
            RequestedPreferences = prepare ? parsed.RequestedPreferences : new(),
            UnsupportedPreferences = parsed.UnsupportedPreferences,
            RequiresConfirmation = prepare,
            CanPrepareSchedule = canPrepare && (prepare ? parsed.UnsupportedPreferences.Count == 0 : true),
            CampusId = campusId,
            CampusName = campusName,
            SemesterId = term.MaHocKy,
            SemesterName = term.TenHocKy,
            SchedulableCourseCount = courses.Count,
            ValidationErrors = prepare ? prepareValidationErrors.Distinct().ToList() : new()
        };
    }

    internal static bool IsEvening(Backend.Models.CaHoc shift) =>
        NormalizeText(shift.Buoi + " " + shift.TenCa).Contains("toi") || shift.GioBatDau >= new TimeOnly(17, 30);

    private static bool IsEveningScheduleQuestion(string normalized)
    {
        if (!normalized.Contains("ca toi", StringComparison.Ordinal)) return false;
        var strippedBo = Regex.Replace(normalized, @"\b(cong bo|toan bo|tien bo|bo mon|can bo)\b", " ", RegexOptions.IgnoreCase);
        if (Regex.IsMatch(strippedBo, @"\b(bo ca toi|loai ca toi|tranh ca toi|khong xep ca toi|khong dung ca toi)\b", RegexOptions.IgnoreCase)) return false;
        if (Regex.IsMatch(strippedBo, @"\b(bo|loai|tranh|cam|khong xep|khong dung)\b", RegexOptions.IgnoreCase)
            && !Regex.IsMatch(strippedBo, @"\b(co|con|ton tai|da xep|xep duoc)\b", RegexOptions.IgnoreCase)
            && !strippedBo.Contains("?")) return false;

        return normalized.Contains("?", StringComparison.Ordinal)
               || Regex.IsMatch(normalized, @"\b(co|con|ton tai|da xep)\b.*\b(khong|chua)\b", RegexOptions.IgnoreCase)
               || Regex.IsMatch(normalized, @"\b(khong|chua)\s*[.!?]*$", RegexOptions.IgnoreCase);
    }

    private static bool IsEveningRemovalRequest(string normalized, IEnumerable<AiConversationTurn>? history)
    {
        // Tra cứu/hỏi (chứa ?, 'có...không', kết thúc bằng 'không/chưa') thì không phải là câu lệnh yêu cầu bỏ ca tối
        var isQuestion = normalized.Contains("?", StringComparison.Ordinal)
                         || Regex.IsMatch(normalized, @"\b(co|con|ton tai|da xep)\b.*\b(khong|chua)\b", RegexOptions.IgnoreCase)
                         || Regex.IsMatch(normalized, @"\b(khong|chua)\s*[.!?]*$", RegexOptions.IgnoreCase);
        if (isQuestion) return false;

        var strippedBo = Regex.Replace(normalized, @"\b(cong bo|toan bo|tien bo|bo mon|can bo)\b", " ", RegexOptions.IgnoreCase);
        var mentionsEvening = strippedBo.Contains("ca toi", StringComparison.Ordinal)
                              || (history?.Any(x => NormalizeText(x.Content).Contains("ca toi", StringComparison.Ordinal)) ?? false);
        return mentionsEvening && Regex.IsMatch(strippedBo, @"\b(bo|loai|tranh|cam|khong xep|khong dung)\b", RegexOptions.IgnoreCase);
    }

    private static AiSchedulingInterpretResponse BuildEveningQueryResponse(AiSchedulingInterpretRequest request,
        int campusId, string campusName, HocKy term, int totalWeeklySessions, int eveningWeeklySessions,
        int courseCount, IReadOnlyCollection<SchedulePromptRow> rows, ISet<int> eveningIds,
        IReadOnlyDictionary<int, string> courseMap, IReadOnlyDictionary<int, Backend.Models.CaHoc> shiftMap,
        string source)
    {
        var sourceLabel = source == "draft" ? "bản nháp được chọn" : "lịch đã công bố";
        string summary;
        if (totalWeeklySessions == 0)
        {
            summary = $"Chưa có {sourceLabel} cho {term.TenHocKy}, nên chưa thể kết luận có ca tối hay không.";
        }
        else if (eveningWeeklySessions == 0)
        {
            summary = $"Không có ca tối trong {sourceLabel} của {term.TenHocKy}. Tổng số ca học mỗi tuần đang kiểm tra là {totalWeeklySessions}.";
        }
        else
        {
            var sample = rows.Where(x => eveningIds.Contains(x.ShiftId))
                .Take(3)
                .Select(x =>
                {
                    var shift = shiftMap.GetValueOrDefault(x.ShiftId);
                    var course = courseMap.GetValueOrDefault(x.CourseId, "Khóa học");
                    return $"{course} Thứ {x.Day} {shift?.TenCa ?? "ca tối"} {shift?.GioBatDau.ToString("HH:mm")}";
                })
                .ToList();
            summary = $"Có {eveningWeeklySessions} ca học mỗi tuần rơi vào ca tối trong {sourceLabel} của {term.TenHocKy}.";
            if (sample.Count > 0) summary += " Ví dụ: " + string.Join("; ", sample) + ".";
        }

        return new AiSchedulingInterpretResponse
        {
            Intent = "query_schedule",
            Summary = summary,
            ExcludeEvening = false,
            RequestedPreferences = [],
            UnsupportedPreferences = [],
            RequiresConfirmation = false,
            CanPrepareSchedule = false,
            CampusId = campusId,
            CampusName = campusName,
            SemesterId = term.MaHocKy,
            SemesterName = term.TenHocKy,
            SchedulableCourseCount = courseCount,
            ValidationErrors = []
        };
    }

    private static SchedulePromptAnswer BuildFallbackAnswer(
        string normalizedMessage,
        bool canPrepare,
        HocKy term,
        int courseCount,
        int activeRoomCount,
        int activeShiftCount,
        int teacherCount,
        List<string> issues,
        string reasonMessage,
        int totalWeeklySessions,
        int distinctCoursesCount,
        int morningSessions,
        int afternoonSessions,
        int eveningSessions,
        string source)
    {
        var isReadinessQuery = Regex.IsMatch(normalizedMessage, @"\b(du dieu kien|san sang|xep duoc chua|co the xep|dieu kien|thieu phong|thieu giang vien|kiem tra xem|co thieu)\b", RegexOptions.IgnoreCase);
        var isScheduleQuery = Regex.IsMatch(normalizedMessage, @"\b(bao nhieu|mon hoc|lop|phan bo|ca toi|ca sang|ca chieu|thu 7|lich|thoi khoa bieu)\b", RegexOptions.IgnoreCase);
        var isPrepareRequest = Regex.IsMatch(normalizedMessage, @"\b(xep lich|tao ban nhap|chuan bi lich|khoi tao|tien hanh xep)\b", RegexOptions.IgnoreCase);

        if (isReadinessQuery)
        {
            if (canPrepare)
            {
                return new SchedulePromptAnswer
                {
                    Intent = "query_readiness",
                    Summary = $"Học kỳ {term.TenHocKy} đã đủ điều kiện sẵn sàng để xếp lịch thời khóa biểu: Đã có {courseCount} khóa học cần xếp, {activeRoomCount} phòng học hoạt động, {activeShiftCount} ca học và {teacherCount} giảng viên đã được phân công đầy đủ. Không có vướng mắc nào cản trở. Quý Thầy/Cô có thể tạo bản nháp ngay.",
                    ExcludeEvening = false,
                    RequestedPreferences = new(),
                    UnsupportedPreferences = new()
                };
            }
            else
            {
                var reasonText = issues.Count > 0 ? string.Join("; ", issues) : (string.IsNullOrWhiteSpace(reasonMessage) ? "Chưa đến thời điểm xếp lịch hoặc học kỳ đang bị khóa" : reasonMessage);
                return new SchedulePromptAnswer
                {
                    Intent = "query_readiness",
                    Summary = $"Học kỳ {term.TenHocKy} hiện chưa đủ điều kiện để xếp lịch do: {reasonText}. Vui lòng kiểm tra lại cấu hình học vụ hoặc liên hệ Ban Giám Hiệu.",
                    ExcludeEvening = false,
                    RequestedPreferences = new(),
                    UnsupportedPreferences = new()
                };
            }
        }

        if (isPrepareRequest)
        {
            var excludeEvening = normalizedMessage.Contains("bo ca toi") || normalizedMessage.Contains("loai ca toi");
            return new SchedulePromptAnswer
            {
                Intent = "prepare_schedule",
                Summary = $"Tôi sẽ lập kế hoạch tạo bản nháp thời khóa biểu mới cho học kỳ {term.TenHocKy} với {courseCount} khóa học{(excludeEvening ? " (loại bỏ toàn bộ ca tối)" : "")}. Vui lòng bấm xác nhận để bắt đầu chạy thuật toán xếp lịch.",
                ExcludeEvening = excludeEvening,
                RequestedPreferences = excludeEvening ? new List<string> { "Loại toàn bộ ca tối" } : new(),
                UnsupportedPreferences = new()
            };
        }

        if (isScheduleQuery || totalWeeklySessions > 0)
        {
            var sourceName = source == "draft" ? "Bản nháp đang chọn" : "Thời khóa biểu đã công bố";
            if (totalWeeklySessions == 0)
            {
                return new SchedulePromptAnswer
                {
                    Intent = "query_schedule",
                    Summary = $"{sourceName} của {term.TenHocKy} hiện chưa có dữ liệu buổi học nào được xếp.",
                    ExcludeEvening = false,
                    RequestedPreferences = new(),
                    UnsupportedPreferences = new()
                };
            }

            return new SchedulePromptAnswer
            {
                Intent = "query_schedule",
                Summary = $"{sourceName} của {term.TenHocKy} có {distinctCoursesCount} môn học với tổng cộng {totalWeeklySessions} ca học mỗi tuần. Phân bổ gồm: {morningSessions} ca sáng, {afternoonSessions} ca chiều và {eveningSessions} ca tối. Phân bổ từ Thứ 2 đến Thứ 7.",
                ExcludeEvening = false,
                RequestedPreferences = new(),
                UnsupportedPreferences = new()
            };
        }

        return new SchedulePromptAnswer
        {
            Intent = "clarify",
            Summary = $"Trợ lý xếp lịch sẵn sàng hỗ trợ bạn kiểm tra điều kiện xếp lịch, tra cứu thời khóa biểu hoặc tạo bản nháp mới cho {term.TenHocKy}. Vui lòng cho biết yêu cầu cụ thể của bạn.",
            ExcludeEvening = false,
            RequestedPreferences = new(),
            UnsupportedPreferences = new()
        };
    }

    private static List<string> BuildPrepareValidationErrors(AcademicSchedulingContextDto context, HocKy term, IEnumerable<string> readinessIssues)
    {
        var errors = readinessIssues.ToList();
        errors.Add(context.SchedulableTerm?.MaHocKy != term.MaHocKy
            ? $"Chỉ được chuẩn bị lịch cho {context.SchedulableTerm?.TenHocKy ?? "học kỳ hợp lệ theo context"}."
            : context.ReasonMessage ?? "Học kỳ chưa sẵn sàng xếp lịch.");
        return errors.Distinct().ToList();
    }

    private sealed class SchedulePromptRow
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int ShiftId { get; set; }
        public string ShiftName { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int Day { get; set; }
    }

    private sealed class SchedulePromptAnswer
    {
        public string Intent { get; set; } = "clarify";
        public string Summary { get; set; } = string.Empty;
        public bool ExcludeEvening { get; set; }
        public List<string> RequestedPreferences { get; set; } = new();
        public List<string> UnsupportedPreferences { get; set; } = new();
    }
}
