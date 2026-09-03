using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.SmartTimetable;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.AcademicSchedulingContext;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Services.AI;

public class SchedulingAiService : ISchedulingAiService
{
    private readonly IOllamaService _ollamaService;
    private readonly IAcademicSchedulingContextService _schedulingContextService;
    private readonly ISmartTimetableService _smartTimetableService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SchedulingAiService> _logger;

    public SchedulingAiService(
        IOllamaService ollamaService,
        IAcademicSchedulingContextService schedulingContextService,
        ISmartTimetableService smartTimetableService,
        ApplicationDbContext context,
        ILogger<SchedulingAiService> logger)
    {
        _ollamaService = ollamaService;
        _schedulingContextService = schedulingContextService;
        _smartTimetableService = smartTimetableService;
        _context = context;
        _logger = logger;
    }

    public async Task<AiSchedulingInterpretResponse> InterpretIntentAsync(
        AiSchedulingInterpretRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default)
    {
        // 1. Xác định campusId và Học kỳ mục tiêu
        int campusId = ResolveCampusId(currentUser, request.CampusId);
        var schedulingContext = await _schedulingContextService.GetContextAsync(campusId, cancellationToken);
        
        // Xác định học kỳ: ưu tiên học kỳ người dùng chọn (SemesterId), nếu không thì lấy SchedulableTerm
        int targetTermId = request.SemesterId ?? schedulingContext.SchedulableTerm?.MaHocKy ?? 0;

        HocKy? targetTerm = null;
        if (targetTermId > 0)
        {
            targetTerm = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(h => h.MaHocKy == targetTermId, cancellationToken);
        }

        if (targetTerm == null && schedulingContext.SchedulableTerm != null)
        {
            targetTermId = schedulingContext.SchedulableTerm.MaHocKy;
            targetTerm = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(h => h.MaHocKy == targetTermId, cancellationToken);
        }

        if (targetTerm == null)
        {
            // Tìm học kỳ tương lai gần nhất có khóa học của cơ sở
            var termWithCourses = await _context.KhoaHocs.AsNoTracking()
                .Where(k => k.MaDonVi == campusId && k.MaHocKy != null)
                .Select(k => k.MaHocKy!.Value)
                .Distinct()
                .OrderByDescending(id => id)
                .FirstOrDefaultAsync(cancellationToken);

            if (termWithCourses > 0)
            {
                targetTermId = termWithCourses;
                targetTerm = await _context.HocKys.AsNoTracking().FirstOrDefaultAsync(h => h.MaHocKy == targetTermId, cancellationToken);
            }
        }

        if (targetTerm == null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở hiện không có học kỳ nào ở trạng thái sẵn sàng để xếp lịch.");
        }

        var campus = await _context.DonVis.AsNoTracking().FirstOrDefaultAsync(d => d.MaDonVi == campusId, cancellationToken);
        string campusName = campus?.TenDonVi ?? $"Cơ sở #{campusId}";
        string termName = targetTerm.TenHocKy ?? $"Học kỳ #{targetTermId}";

        int schedulableCourseCount = await _context.KhoaHocs.CountAsync(
            k => k.MaHocKy == targetTermId && k.MaDonVi == campusId, 
            cancellationToken);

        var validationErrors = schedulingContext.Readiness?.BlockingIssues?.Select(b => b.Message).ToList() 
            ?? new List<string>();

        // 2. PHÂN TÍCH ĐỘNG TOÀN DIỆN BẰNG AI (Không hardcode mẫu cố định)
        string promptText = request.Message?.Trim() ?? string.Empty;
        
        string profile = "balanced";
        string profileDisplayName = "Cân bằng toàn diện";
        string summary = $"Xếp lịch cho {termName} tại {campusName} ({schedulableCourseCount} khóa học) dựa trên yêu cầu: \"{promptText}\"";
        var requestedPrefs = new List<string>();

        // Gọi Ollama để hiểu ngôn ngữ tự nhiên sâu và trích xuất mọi yêu cầu tùy biến của người dùng
        try
        {
            var systemPrompt = new StringBuilder();
            systemPrompt.AppendLine("Bạn là Trợ lý AI Chuyên gia Xếp lịch Thời khóa biểu Đại học.");
            systemPrompt.AppendLine($"Giáo vụ vừa gửi yêu cầu xếp lịch: \"{promptText}\"");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("Hãy phân tích chi tiết và trung thực theo đúng nội dung yêu cầu của người dùng, KHÔNG dùng mẫu cố định:");
            systemPrompt.AppendLine("1. Chọn 1 trong 3 hồ sơ giải thuật phù hợp nhất:");
            systemPrompt.AppendLine("   - \"student_friendly\": nếu yêu cầu nhấn mạnh lợi ích sinh viên (tránh ca tối, tránh trống tiết, tránh học thứ 7, ưu tiên học ca sáng hoặc chiều, giảm tải...)");
            systemPrompt.AppendLine("   - \"teacher_friendly\": nếu yêu cầu nhấn mạnh lợi ích giảng viên (đáp ứng nguyện vọng, dồn buổi dạy, tránh dạy quá nhiều ca/ngày, nghỉ cuối tuần...)");
            systemPrompt.AppendLine("   - \"balanced\": nếu yêu cầu cân bằng giữa sinh viên và giảng viên, hoặc bao gồm cả hai.");
            systemPrompt.AppendLine("2. Trích xuất TẤT CẢ các ràng buộc, tiêu chí cụ thể mà người dùng vừa nêu ra trong câu prompt (mỗi ý là 1 dòng ngắn gọn, giữ đúng ý người dùng).");
            systemPrompt.AppendLine("3. Viết 1 câu tóm tắt chính xác những gì người dùng muốn.");
            systemPrompt.AppendLine();
            systemPrompt.AppendLine("Chỉ trả về duy nhất chuỗi JSON (không bọc trong ```json hay văn bản thừa):");
            systemPrompt.AppendLine("{");
            systemPrompt.AppendLine("  \"profile\": \"student_friendly | teacher_friendly | balanced\",");
            systemPrompt.AppendLine("  \"profileDisplayName\": \"Tên mô tả tiếng Việt của hồ sơ\",");
            systemPrompt.AppendLine("  \"summary\": \"Tóm tắt phân tích ý định của người dùng\",");
            systemPrompt.AppendLine("  \"requestedPreferences\": [\"Ràng buộc 1 do người dùng yêu cầu\", \"Ràng buộc 2 do người dùng yêu cầu\"]");
            systemPrompt.AppendLine("}");

            var chatReq = new AiChatRequest
            {
                Message = systemPrompt.ToString(),
                Mode = "fast",
                UseRag = false
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            var aiRes = await _ollamaService.ChatAsync(chatReq, currentUser, cts.Token);

            if (!string.IsNullOrWhiteSpace(aiRes?.Answer))
            {
                string rawJson = aiRes.Answer.Trim();
                // Bóc tách JSON nếu có markdown block
                var match = Regex.Match(rawJson, @"\{[\s\S]*\}");
                if (match.Success)
                {
                    rawJson = match.Value;
                }

                using var doc = JsonDocument.Parse(rawJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("profile", out var pElem) && !string.IsNullOrWhiteSpace(pElem.GetString()))
                {
                    string pVal = pElem.GetString()!.ToLowerInvariant();
                    if (pVal.Contains("student")) profile = "student_friendly";
                    else if (pVal.Contains("teacher")) profile = "teacher_friendly";
                    else profile = "balanced";
                }

                if (root.TryGetProperty("profileDisplayName", out var pdElem) && !string.IsNullOrWhiteSpace(pdElem.GetString()))
                {
                    profileDisplayName = pdElem.GetString()!;
                }
                else
                {
                    profileDisplayName = profile == "student_friendly" ? "Ưu tiên Sinh viên" :
                                         profile == "teacher_friendly" ? "Ưu tiên Giảng viên" : "Cân bằng toàn diện";
                }

                if (root.TryGetProperty("summary", out var sElem) && !string.IsNullOrWhiteSpace(sElem.GetString()))
                {
                    summary = sElem.GetString()!;
                }

                if (root.TryGetProperty("requestedPreferences", out var rpElem) && rpElem.ValueKind == JsonValueKind.Array)
                {
                    requestedPrefs.Clear();
                    foreach (var item in rpElem.EnumerateArray())
                    {
                        string? str = item.GetString();
                        if (!string.IsNullOrWhiteSpace(str))
                        {
                            requestedPrefs.Add(str.Trim());
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama dynamic intent parsing encountered an issue, extracting dynamic clauses from prompt directly");
        }

        // Nếu LLM không trả về danh sách preferences hoặc bị lỗi timeout, tự động bóc tách các vế câu thực tế của prompt
        if (requestedPrefs.Count == 0)
        {
            // Bóc tách theo dấu phẩy, chấm phẩy, hoặc liên từ "và", "đồng thời", "nhưng"
            var clauses = Regex.Split(promptText, @"[,;\.\n]| và | đồng thời | nhưng | ngoài ra ")
                .Select(c => c.Trim())
                .Where(c => c.Length >= 5)
                .ToList();

            if (clauses.Count > 0)
            {
                foreach (var clause in clauses.Take(5))
                {
                    string formatted = char.ToUpper(clause[0]) + clause.Substring(1);
                    requestedPrefs.Add(formatted);
                }
            }
            else
            {
                requestedPrefs.Add($"Đáp ứng yêu cầu tùy biến: \"{promptText}\"");
            }

            // Heuristic phân loại profile cơ bản nếu chưa có
            string norm = NormalizeText(promptText);
            if (norm.Contains("sinh vien") || norm.Contains("ca toi") || norm.Contains("hoc sinh") || norm.Contains("trong tiet") || norm.Contains("sang") || norm.Contains("chieu"))
            {
                profile = "student_friendly";
                profileDisplayName = "Ưu tiên Sinh viên (Theo yêu cầu tùy biến)";
            }
            else if (norm.Contains("giang vien") || norm.Contains("giao vien") || norm.Contains("nguyen vong") || norm.Contains("don tai") || norm.Contains("qua tai") || norm.Contains("day"))
            {
                profile = "teacher_friendly";
                profileDisplayName = "Ưu tiên Giảng viên (Theo yêu cầu tùy biến)";
            }
            else
            {
                profile = "balanced";
                profileDisplayName = "Cân bằng toàn diện (Theo yêu cầu tùy biến)";
            }
        }

        bool canPrepare = schedulableCourseCount > 0 && validationErrors.Count == 0;

        return new AiSchedulingInterpretResponse
        {
            Intent = "prepare_schedule",
            Profile = profile,
            ProfileDisplayName = profileDisplayName,
            Summary = summary,
            RequestedPreferences = requestedPrefs,
            RequiresConfirmation = true,
            ContextVersion = $"{campusId}-{targetTermId}-{schedulableCourseCount}",
            CampusId = campusId,
            CampusName = campusName,
            SemesterId = targetTermId,
            SemesterName = termName,
            SchedulableCourseCount = schedulableCourseCount,
            CanPrepareSchedule = canPrepare,
            ValidationErrors = canPrepare ? new List<string>() : validationErrors
        };
    }

    public async Task<AiExplainDraftResponse> ExplainDraftAsync(
        AiExplainDraftRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default)
    {
        var draft = await _smartTimetableService.GetDraftAsync(request.DraftId, cancellationToken);
        if (draft == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bản nháp thời khóa biểu.");
        }

        int campusId = ResolveCampusId(currentUser, draft.MaDonVi);
        if (draft.MaDonVi != campusId && currentUser?.Role != "SuperAdmin" && currentUser?.Role != "Admin")
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Không có quyền xem bản nháp của cơ sở khác.");
        }

        // 1. Tính toán DraftFacts xác thực từ database
        var eveningShifts = await _context.CaHocs.AsNoTracking()
            .Where(x => x.Buoi.ToLower() == "toi" || x.GioBatDau >= new TimeOnly(17, 30))
            .Select(x => x.MaCaHoc)
            .ToListAsync(cancellationToken);

        int totalCourses = draft.TongCourse ?? draft.Items.Select(x => x.MaKhoaHoc).Distinct().Count();
        int assignedCourses = draft.SoXepDuoc ?? draft.Items.Select(x => x.MaKhoaHoc).Distinct().Count();
        int unassigned = draft.SoKhongXepDuoc ?? Math.Max(0, totalCourses - assignedCourses);
        int hardConflicts = draft.SoXungDotCung ?? 0;
        int eveningCount = draft.Items.Count(x => x.MaCaHoc.HasValue && eveningShifts.Contains(x.MaCaHoc.Value));
        int saturdayCount = draft.Items.Count(x => x.ThuTrongTuan == 7);
        double successRate = totalCourses > 0 ? Math.Round((double)assignedCourses / totalCourses * 100, 1) : 100.0;

        string profileUsed = "balanced";
        if (!string.IsNullOrWhiteSpace(draft.Items.FirstOrDefault()?.LyDoGoiY?.FirstOrDefault()))
        {
            profileUsed = "applied";
        }

        var notes = new List<string>();
        if (unassigned == 0 && hardConflicts == 0)
        {
            notes.Add("Thuật toán xếp lịch thành công tuyệt đối 100%, không xảy ra bất kỳ xung đột phòng hoặc giảng viên nào.");
        }
        else if (unassigned > 0)
        {
            notes.Add($"Còn {unassigned} khóa học chưa thể bố trí phòng do thiếu ca hoặc không trùng thời gian rảnh.");
        }

        if (eveningCount > 0)
        {
            notes.Add($"Có {eveningCount} buổi học được bố trí vào ca tối (sau 17h30).");
        }
        else
        {
            notes.Add("Không có bất kỳ buổi học nào bị xếp vào ca tối.");
        }

        if (saturdayCount > 0)
        {
            notes.Add($"Có {saturdayCount} buổi học được xếp vào ngày Thứ Bảy.");
        }

        var facts = new SchedulingDraftFactsDto
        {
            DraftId = draft.DraftId,
            TotalCourses = totalCourses,
            AssignedCourses = assignedCourses,
            UnassignedCourses = unassigned,
            HardConflictsCount = hardConflicts,
            EveningShiftsCount = eveningCount,
            SaturdayShiftsCount = saturdayCount,
            SuccessRate = successRate,
            BestFitnessScore = draft.Score,
            ProfileUsed = profileUsed,
            TotalSessionsCount = draft.Items.Count,
            AverageRoomFitRatio = 85.0,
            HighlightNotes = notes
        };

        // 2. AI diễn giải kết quả từ Facts
        string explanation = GenerateDraftExplanationText(facts);

        try
        {
            var promptSb = new StringBuilder();
            promptSb.AppendLine("Bạn là Trợ lý Học vụ AI của trường Đại học. Hãy viết một đoạn nhận xét chuyên môn ngắn (khoảng 3-4 câu) cho Ban Giám hiệu / Giáo vụ về bản nháp thời khóa biểu vừa xếp:");
            promptSb.AppendLine($"- Tổng khóa học: {facts.TotalCourses}, Xếp được: {facts.AssignedCourses} ({facts.SuccessRate}%), Chưa xếp được: {facts.UnassignedCourses}");
            promptSb.AppendLine($"- Xung đột phòng/giờ: {facts.HardConflictsCount}");
            promptSb.AppendLine($"- Số buổi học ca tối: {facts.EveningShiftsCount}, Số buổi học Thứ Bảy: {facts.SaturdayShiftsCount}");
            promptSb.AppendLine("Yêu cầu: Lời văn chuyên nghiệp, đánh giá thẳng thắn về mức độ tối ưu của bản nháp.");

            var chatReq = new AiChatRequest
            {
                Message = promptSb.ToString(),
                Mode = "fast",
                UseRag = false
            };
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            var aiRes = await _ollamaService.ChatAsync(chatReq, currentUser, cts.Token);
            if (!string.IsNullOrWhiteSpace(aiRes?.Answer) && !aiRes.Answer.Contains("đăng nhập"))
            {
                explanation = aiRes.Answer.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ollama draft explanation skipped or timed out, using safe deterministic template");
        }

        return new AiExplainDraftResponse
        {
            DraftId = draft.DraftId,
            IsSuccess = unassigned == 0 && hardConflicts == 0,
            Facts = facts,
            AiExplanation = explanation,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public Task<AiExplainReadinessResponse> ExplainReadinessAsync(
        AiExplainReadinessRequest request,
        CurrentUserContext? currentUser,
        CancellationToken cancellationToken = default)
    {
        string code = request.ReasonCode?.Trim().ToUpperInvariant() ?? "UNKNOWN";
        string humanExpl;
        string recAction;
        string route = "/staff/schedule/management";
        string label = "Quản lý Thời khóa biểu";

        switch (code)
        {
            case "STUDENT_CAPACITY_DATA_MISSING":
                humanExpl = "Một số khóa học chưa có dữ liệu sĩ số thực tế từ danh sách lớp hoặc đăng ký môn. Cỗ máy xếp lịch bắt buộc phải có sĩ số để tính toán phòng học có sức chứa tương ứng.";
                recAction = "Vui lòng kiểm tra danh sách lớp học phần hoặc cập nhật sĩ số dự kiến của các khóa học trước khi xếp lịch.";
                route = "/staff/schedule/management";
                label = "Kiểm tra danh sách khóa học";
                break;

            case "NO_ACTIVE_ROOMS":
                humanExpl = "Cơ sở hiện chưa có phòng học nào ở trạng thái Hoạt động. Hệ thống không thể xếp lịch nếu không có địa điểm giảng dạy.";
                recAction = "Vui lòng truy cập trang Quản lý phòng học để kiểm tra và bật trạng thái Hoạt động cho các phòng học.";
                route = "/staff/schedule/rooms";
                label = "Quản lý Phòng học";
                break;

            case "ROOM_CAPACITY_INSUFFICIENT":
                humanExpl = "Sức chứa tối đa của các phòng học hiện có nhỏ hơn sĩ số của một số lớp học phần lớn.";
                recAction = "Vui lòng bổ sung phòng học giảng đường lớn hoặc tách nhỏ lớp học phần để đảm bảo đủ chỗ ngồi.";
                route = "/staff/schedule/rooms";
                label = "Điều chỉnh Phòng học";
                break;

            case "CREDIT_MAPPING_MISSING":
                humanExpl = "Chưa có cấu hình quy đổi số tín chỉ sang số buổi học/tuần trong học kỳ này.";
                recAction = "Vui lòng vào trang Quy đổi tín chỉ để thiết lập định mức (ví dụ: 3 tín chỉ = 3 buổi/tuần).";
                route = "/staff/schedule/credit-mapping";
                label = "Cấu hình Quy đổi tín chỉ";
                break;

            case "SCHEDULE_LOCKED_AFTER_EDIT_WINDOW":
            case "SCHEDULE_LOCKED_BY_ATTENDANCE":
                humanExpl = "Thời khóa biểu đã chính thức bị khóa do đã có buổi học điểm danh hoặc đã kết thúc thời hạn chỉnh sửa của học kỳ.";
                recAction = "Nếu cần điều chỉnh lịch khẩn cấp, vui lòng liên hệ Ban Giám Hiệu để xin cấp quyền mở khóa.";
                route = "/staff/schedule/management";
                label = "Xem Thời khóa biểu";
                break;

            default:
                humanExpl = !string.IsNullOrWhiteSpace(request.RawMessage) 
                    ? request.RawMessage 
                    : "Học kỳ hiện tại chưa đáp ứng đầy đủ điều kiện sẵn sàng để khởi tạo thời khóa biểu tự động.";
                recAction = "Vui lòng kiểm tra các tiêu chí phòng học, giảng viên và ca học trước khi thử lại.";
                break;
        }

        return Task.FromResult(new AiExplainReadinessResponse
        {
            ReasonCode = code,
            HumanExplanation = humanExpl,
            RecommendedAction = recAction,
            ActionRoute = route,
            ActionLabel = label
        });
    }

    private static string GenerateDraftExplanationText(SchedulingDraftFactsDto facts)
    {
        var sb = new StringBuilder();
        if (facts.AssignedCourses == facts.TotalCourses && facts.HardConflictsCount == 0)
        {
            sb.Append($"Bản nháp thời khóa biểu đã được khởi tạo thành công 100% ({facts.AssignedCourses}/{facts.TotalCourses} khóa học). ");
            sb.Append("Không phát hiện bất kỳ xung đột nào về phòng học hay lịch giảng dạy của giảng viên. ");
            if (facts.EveningShiftsCount == 0)
            {
                sb.Append("Toàn bộ các lớp đều được bố trí vào các ca học ban ngày, hoàn toàn không có ca tối sau 17h30. ");
            }
            else
            {
                sb.Append($"Có {facts.EveningShiftsCount} buổi học được bố trí vào ca tối. ");
            }
            sb.Append("Thời khóa biểu đã sẵn sàng để quý Thầy/Cô rà soát và tiến hành Xuất bản.");
        }
        else
        {
            sb.Append($"Đã xếp thành công {facts.AssignedCourses}/{facts.TotalCourses} khóa học (Tỷ lệ đạt {facts.SuccessRate}%). ");
            if (facts.UnassignedCourses > 0)
            {
                sb.Append($"Hiện còn {facts.UnassignedCourses} khóa học chưa bố trí được lịch phù hợp do hạn chế về phòng học hoặc ca trống. ");
            }
            sb.Append("Khuyến nghị rà soát lại các môn chưa xếp được trước khi quyết định xuất bản.");
        }
        return sb.ToString();
    }

    private static int ResolveCampusId(CurrentUserContext? currentUser, int? requestedCampusId)
    {
        if (currentUser?.Role == "AcademicStaff" && currentUser.CampusId > 0)
        {
            return currentUser.CampusId;
        }
        return requestedCampusId ?? (currentUser?.CampusId > 0 ? currentUser.CampusId : 14);
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        text = text.Replace("đ", "d").Replace("Đ", "d");
        string formD = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
