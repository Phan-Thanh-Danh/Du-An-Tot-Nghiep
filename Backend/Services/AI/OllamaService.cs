using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.AI;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly IAiRequestGate _gate;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OllamaService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OllamaService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        IAiRequestGate gate,
        ApplicationDbContext db,
        ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _gate = gate;
        _db = db;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var uri = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            _httpClient.BaseAddress = uri;
        }

        var timeoutSec = _options.TimeoutSeconds > 0 ? _options.TimeoutSeconds : 180;
        _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSec);
    }

    public async Task<AiHealthResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var response = new AiHealthResponse
        {
            ChatModel = _options.ChatModel,
            EmbeddingModel = _options.EmbeddingModel,
            QueueLength = _gate.CurrentQueueLength
        };

        try
        {
            using var httpResponse = await _httpClient.GetAsync("api/tags", cancellationToken);
            sw.Stop();
            response.LatencyMs = sw.ElapsedMilliseconds;

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                var tagsResult = JsonSerializer.Deserialize<OllamaTagsResponse>(content, JsonOptions);

                var models = tagsResult?.Models?.Select(m => m.Name ?? "").ToList() ?? new List<string>();

                response.Available = true;
                response.ChatModelAvailable = models.Any(m => ModelMatches(m, _options.ChatModel));
                response.EmbeddingModelAvailable = models.Any(m => ModelMatches(m, _options.EmbeddingModel));
            }
            else
            {
                response.Available = false;
                _logger.LogWarning("Ollama health check returned status code: {StatusCode}", httpResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            sw.Stop();
            response.LatencyMs = sw.ElapsedMilliseconds;
            response.Available = false;
            _logger.LogWarning(ex, "Failed to reach Ollama at {BaseUrl}", _options.BaseUrl);
        }

        return response;
    }

    public async Task<AiChatResponse> ChatAsync(AiChatRequest request, CurrentUserContext? userContext, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            throw new ApiException(400, "Nội dung câu hỏi không được để trống.");
        }

        var conversationId = string.IsNullOrWhiteSpace(request.ConversationId)
            ? Guid.NewGuid().ToString("N")
            : request.ConversationId;

        // 1. Tải ngữ cảnh học tập thực tế nếu người dùng là Sinh viên
        StudentAcademicContext? studentAcademicContext = null;
        if (userContext != null && (userContext.Role == AuthRoles.Student || userContext.Role == "hoc_sinh") && userContext.UserId > 0)
        {
            studentAcademicContext = await LoadStudentAcademicContextAsync(userContext.UserId, cancellationToken);
        }

        // 2. Tra cứu & báo cáo dữ liệu học tập cá nhân Mức 2 (Chỉ đọc qua Backend Service)
        if (TryGetStudentAttendanceReply(request.Message, userContext, studentAcademicContext, out var attendanceReply))
        {
            await Task.Delay(1800, cancellationToken);
            return CreateFastResponse(attendanceReply, conversationId);
        }

        if (TryGetStudentScheduleReply(request.Message, userContext, studentAcademicContext, out var scheduleReply))
        {
            await Task.Delay(1800, cancellationToken);
            return CreateFastResponse(scheduleReply, conversationId);
        }

        if (TryGetStudentAssignmentsReply(request.Message, userContext, studentAcademicContext, out var assignmentsReply))
        {
            await Task.Delay(1800, cancellationToken);
            return CreateFastResponse(assignmentsReply, conversationId);
        }

        if (TryGetStudentGradeReply(request.Message, userContext, studentAcademicContext, out var gradeReply))
        {
            await Task.Delay(1800, cancellationToken);
            return CreateFastResponse(gradeReply, conversationId);
        }

        // 3. Phản hồi mượt mà (~1.8-2.5s) cho các câu hỏi quy chế, tính toán cơ bản & chào hỏi (Mức 1)
        if (TryGetInstantReply(request.Message, userContext, out var instantReply))
        {
            await Task.Delay(1800, cancellationToken);
            return CreateFastResponse(instantReply, conversationId);
        }

        // 4. Xử lý câu hỏi học thuật qua Ollama với Concurrency Gate & Context học tập thực tế
        var systemPrompt = await BuildSystemPromptAsync(userContext, studentAcademicContext, request.CourseId, request.LessonId, cancellationToken);

        var numPredict = _options.MaxOutputTokens > 0 ? _options.MaxOutputTokens : 2048;
        var numCtx = _options.ContextLength > 0 ? _options.ContextLength : 4096;

        var payload = new OllamaChatPayload
        {
            Model = _options.ChatModel,
            Stream = false,
            Messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = request.Message.Trim() }
            },
            Options = new OllamaChatOptions
            {
                NumCtx = numCtx,
                NumPredict = numPredict,
                Temperature = 0.6f,
                TopP = 0.9f
            }
        };

        var response = await _gate.ExecuteWithGateAsync(async (token) =>
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var httpResponse = await _httpClient.PostAsync("api/chat", content, token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    var errorText = await httpResponse.Content.ReadAsStringAsync(token);
                    _logger.LogError("Ollama chat returned status code {StatusCode}: {Error}", httpResponse.StatusCode, errorText);

                    if ((int)httpResponse.StatusCode == 404)
                    {
                        throw new ApiException(503, $"Mô hình AI '{_options.ChatModel}' chưa được nạp trên máy chủ Ollama.");
                    }

                    throw new ApiException(502, "Không thể kết nối đến máy chủ AI nội bộ. Vui lòng thử lại sau.");
                }

                var responseString = await httpResponse.Content.ReadAsStringAsync(token);
                var chatResult = JsonSerializer.Deserialize<OllamaChatResult>(responseString, JsonOptions);
                sw.Stop();

                var answer = chatResult?.Message?.Content?.Trim() ?? string.Empty;
                var thinking = chatResult?.Message?.Thinking?.Trim();

                // Trường hợp model reasoning chỉ trả về thinking mà chưa xuất content
                if (string.IsNullOrWhiteSpace(answer))
                {
                    if (!string.IsNullOrWhiteSpace(thinking))
                    {
                        answer = CleanThinkingFallback(thinking);
                    }
                    else
                    {
                        answer = "Xin lỗi, hiện tại tôi chưa thể tạo câu trả lời cho câu hỏi này. Bạn vui lòng thử lại câu hỏi khác.";
                    }
                }

                return new AiChatResponse
                {
                    Answer = answer,
                    Thinking = thinking,
                    ProcessingTimeMs = (int)sw.ElapsedMilliseconds,
                    ConversationId = conversationId,
                    Model = _options.ChatModel,
                    Sources = new List<string>()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama at {BaseUrl}", _options.BaseUrl);
                throw new ApiException(503, "Dịch vụ AI cục bộ hiện đang ngoại tuyến. Vui lòng thử lại sau.");
            }
        }, cancellationToken);

        return response;
    }

    public async Task<AiEmbeddingTestResponse> TestEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ApiException(400, "Đoạn văn bản không được để trống.");
        }

        var payload = new
        {
            model = _options.EmbeddingModel,
            input = text.Trim()
        };

        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var httpResponse = await _httpClient.PostAsync("api/embed", content, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ollama embed returned {StatusCode}: {Error}", httpResponse.StatusCode, err);
                throw new ApiException(502, $"Lỗi từ dịch vụ embedding: {httpResponse.StatusCode}");
            }

            var resString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var embedResult = JsonSerializer.Deserialize<OllamaEmbedResult>(resString, JsonOptions);

            var firstVector = embedResult?.Embeddings?.FirstOrDefault();
            var dims = firstVector?.Length ?? 0;

            return new AiEmbeddingTestResponse
            {
                Model = _options.EmbeddingModel,
                Dimensions = dims,
                Success = dims > 0
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to reach Ollama for embedding at {BaseUrl}", _options.BaseUrl);
            throw new ApiException(503, "Dịch vụ AI cục bộ hiện đang ngoại tuyến.");
        }
    }

    private async Task<string> BuildSystemPromptAsync(
        CurrentUserContext? userContext,
        StudentAcademicContext? studentAcademicContext,
        int? courseId,
        int? lessonId,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bạn là trợ lý học thuật thông minh của hệ thống giáo dục AET LMS.");
        sb.AppendLine("Trả lời hoàn toàn bằng tiếng Việt chuẩn xác, ngắn gọn, lịch sự, ân cần và mang tính sư phạm cao.");
        sb.AppendLine("QUAN TRỌNG: Hãy đi thẳng vào câu trả lời giải đáp thắc mắc, không tự sinh phần suy nghĩ nội tâm dông dài.");
        sb.AppendLine("Tuyệt đối không tự bịa đặt thông tin cá nhân hoặc quyết định học vụ chính thức ngoài phạm vi dữ liệu đã cung cấp.");

        var role = userContext?.Role ?? AuthRoles.Student;

        if (role == AuthRoles.Student || role == "hoc_sinh")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: SINH VIÊN / HỌC SINH]");
            sb.AppendLine("Nhiệm vụ của bạn:");
            sb.AppendLine("- Hướng dẫn phương pháp học tập, giải thích các thuật ngữ / khái niệm bài học, tóm tắt nội dung bài giảng.");
            sb.AppendLine("- Hướng dẫn quy trình học vụ cơ bản: đăng ký môn học, xem thời khóa biểu, quy chế điểm danh, điều kiện dự thi, nộp đơn hỗ trợ.");
            sb.AppendLine("- Phân tích kết quả học tập, đưa ra lời khuyên ôn tập và cải thiện điểm số cho sinh viên.");
            sb.AppendLine("QUY TẮC BẢO MẬT & RANH GIỚI BẮT BUỘC:");
            sb.AppendLine("- TUYỆT ĐỐI KHÔNG cung cấp điểm số, bài nộp, thông tin cá nhân của sinh viên khác.");
            sb.AppendLine("- TUYỆT ĐỐI KHÔNG tiết lộ thông tin riêng tư, lương, đánh giá nội bộ của giảng viên hoặc dữ liệu quản trị nhà trường.");
            sb.AppendLine("- Nếu sinh viên hỏi về dữ liệu của người khác, hãy từ chối lịch sự: 'Vì lý do bảo mật quyền riêng tư của hệ thống LMS, tôi không thể cung cấp dữ liệu cá nhân của người khác.'");

            // NẠP DỮ LIỆU HỌC TẬP THỰC TẾ (MỨC 2 - CHỈ ĐỌC TỪ BACKEND SERVICE)
            if (studentAcademicContext != null)
            {
                sb.AppendLine("\n[DỮ LIỆU HỌC TẬP THỰC TẾ CỦA SINH VIÊN ĐANG ĐĂNG NHẬP (MỨC 2)]:");
                sb.AppendLine($"- Họ và tên: {studentAcademicContext.StudentName} (Email: {studentAcademicContext.Email}, Lớp: {studentAcademicContext.ClassName})");
                sb.AppendLine($"- Điểm GPA tích lũy: {studentAcademicContext.CumulativeGpa:0.00} / 10 (Xếp loại: {studentAcademicContext.Classification})");
                sb.AppendLine($"- Tín chỉ đã tích lũy: {studentAcademicContext.EarnedCredits} / 120 tín chỉ (Số môn đạt: {studentAcademicContext.PassedSubjectsCount}, Chưa đạt: {studentAcademicContext.FailedSubjectsCount})");

                if (studentAcademicContext.Grades.Count > 0)
                {
                    sb.AppendLine("- Bảng điểm các môn học:");
                    foreach (var g in studentAcademicContext.Grades)
                    {
                        sb.AppendLine($"  + {g.SubjectName} ({g.SubjectCode}, {g.Credits} TC - {g.SemesterName}): Quá trình: {g.ProcessScore?.ToString("0.0") ?? "-"} | Giữa kỳ: {g.MidtermScore?.ToString("0.0") ?? "-"} | Cuối kỳ: {g.FinalScore?.ToString("0.0") ?? "-"} -> GPA: {g.Gpa:0.0} (Trạng thái: {g.StatusLabel})");
                    }
                }

                if (studentAcademicContext.UpcomingClasses.Count > 0)
                {
                    sb.AppendLine("- Lịch học các buổi sắp tới:");
                    foreach (var sc in studentAcademicContext.UpcomingClasses)
                    {
                        sb.AppendLine($"  + {sc.Date:dd/MM/yyyy}: {sc.SubjectName} ({sc.TimeRange}, Phòng: {sc.Room}, GV: {sc.TeacherName})");
                    }
                }

                sb.AppendLine($"- Tình hình Chuyên cần / Điểm danh: Tổng {studentAcademicContext.AttendanceSummary.TotalSessions} buổi đã học (Có mặt: {studentAcademicContext.AttendanceSummary.PresentSessions}, Vắng: {studentAcademicContext.AttendanceSummary.ExcusedAbsence + studentAcademicContext.AttendanceSummary.UnexcusedAbsence} buổi, Muộn: {studentAcademicContext.AttendanceSummary.LateSessions} buổi -> Tỷ lệ chuyên cần: {studentAcademicContext.AttendanceSummary.AttendanceRate:0.#}%)");

                if (studentAcademicContext.PendingAssignments.Count > 0)
                {
                    sb.AppendLine("- Bài tập / Bài kiểm tra chưa hoàn thành cần nộp:");
                    foreach (var pa in studentAcademicContext.PendingAssignments)
                    {
                        sb.AppendLine($"  + {pa.Title} (Môn: {pa.SubjectName} - Hạn nộp: {pa.Deadline:HH:mm dd/MM/yyyy})");
                    }
                }

                sb.AppendLine("\nHƯỚNG DẪN TRẢ LỜI DỮ LIỆU CÁ NHÂN:");
                sb.AppendLine("- Khi sinh viên hỏi về điểm số, kết quả học tập, lịch học, chuyên cần hoặc bài tập deadline, BẠN PHẢI DỰA TRỰC TIẾP vào dữ liệu thực tế trên để trả lời đầy đủ, chi tiết, chính xác và ân cần.");
                sb.AppendLine("- TUYỆT ĐỐI KHÔNG trả lời 'Tôi không có thông tin cụ thể' vì bạn ĐÃ ĐƯỢC CẤP DỮ LIỆU ĐÃ QUA LỌC BẢO MẬT CỦA CHÍNH SINH VIÊN NÀY.");
            }
        }
        else if (role == AuthRoles.Teacher || role == "giao_vien")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: GIẢNG VIÊN]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ xây dựng giáo án, gợi ý câu hỏi trắc nghiệm/tự luận, tóm tắt tài liệu chuyên môn, tư vấn quy trình nhập điểm và sửa điểm.");
            sb.AppendLine("QUY TẮC BẢO MẬT: TUYỆT ĐỐI KHÔNG cung cấp tình trạng giảng dạy, số giờ dạy, đánh giá hoặc xếp hạng của các giảng viên khác.");
        }
        else if (role == AuthRoles.HoiDongQuanLyNoiDung)
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: HỘI ĐỒNG QUẢN LÝ NỘI DUNG & THẨM ĐỊNH]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ rà soát đề cương môn học (Syllabus), chuẩn đầu ra (CLO/PLO), tính liên thông giữa các môn học trong chương trình đào tạo.");
        }
        else if (role == AuthRoles.AcademicStaff || role == "nhan_vien")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: CÁN BỘ GIÁO VỤ]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ tra cứu quy chế học vụ, tiêu chuẩn xếp thời khóa biểu và xử lý đơn từ học sinh.");
        }
        else if (role == AuthRoles.Principal || role == AuthRoles.CampusAdmin || role == "hieu_truong")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: BAN GIÁM HIỆU / QUẢN LÝ CƠ SỞ]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ phân tích xu hướng học thuật tổng quan và chất lượng đào tạo.");
        }
        else if (role == AuthRoles.SuperAdmin || role == AuthRoles.Admin || role == "sieu_quan_tri" || role == "quan_tri")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: QUẢN TRỊ VIÊN HỆ THỐNG]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ giải đáp các câu hỏi kỹ thuật và vận hành hệ thống LMS.");
        }

        // Context Injection for Lesson / Course if specified
        if (lessonId.HasValue && lessonId.Value > 0)
        {
            try
            {
                var lesson = await _db.BaiHocs
                    .AsNoTracking()
                    .Include(b => b.Chuong)
                    .FirstOrDefaultAsync(b => b.MaBaiHoc == lessonId.Value, cancellationToken);

                if (lesson != null)
                {
                    sb.AppendLine($"\n[NGỮ CẢNH BÀI HỌC HIỆN TẠI]:");
                    sb.AppendLine($"- Bài học: {lesson.TieuDe}");
                    if (lesson.Chuong != null)
                    {
                        sb.AppendLine($"- Thuộc chương: {lesson.Chuong.TieuDe}");
                    }
                    if (!string.IsNullOrWhiteSpace(lesson.TomTatAi))
                    {
                        sb.AppendLine($"- Tóm tắt bài học: {lesson.TomTatAi}");
                    }
                    else if (!string.IsNullOrWhiteSpace(lesson.NoiDungVanBan))
                    {
                        var preview = lesson.NoiDungVanBan.Length > 800
                            ? lesson.NoiDungVanBan[..800] + "..."
                            : lesson.NoiDungVanBan;
                        sb.AppendLine($"- Nội dung bài học trích lược: {preview}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch lesson context for lesson {LessonId}", lessonId);
            }
        }
        else if (courseId.HasValue && courseId.Value > 0)
        {
            try
            {
                var course = await _db.KhoaHocs
                    .AsNoTracking()
                    .Include(k => k.MonHoc)
                    .FirstOrDefaultAsync(k => k.MaKhoaHoc == courseId.Value, cancellationToken);

                if (course != null)
                {
                    var subjectCode = course.MonHoc?.MaCodeMonHoc ?? "";
                    sb.AppendLine($"\n[NGỮ CẢNH MÔN HỌC HIỆN TẠI]:");
                    sb.AppendLine($"- Khóa học: {course.TieuDe} (Mã môn: {subjectCode})");
                    if (!string.IsNullOrWhiteSpace(course.MoTa))
                    {
                        sb.AppendLine($"- Mô tả khóa học: {course.MoTa}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not fetch course context for course {CourseId}", courseId);
            }
        }

        return sb.ToString();
    }

    private static bool TryGetInstantReply(string input, CurrentUserContext? user, out string reply)
    {
        var raw = input.Trim().ToLowerInvariant();
        // Loại bỏ dấu câu đơn giản
        var normalized = System.Text.RegularExpressions.Regex.Replace(raw, @"[?.!,~@#$%^&*()_+=\[\]{};:'""\\/<>|`]", "").Trim();

        // 1. Phép tính số học cơ bản (Ví dụ: 1+1, 2+2, 5*5, 100/4, 1+1 bằng mấy, tính 5+5)
        var mathMatch = System.Text.RegularExpressions.Regex.Match(raw, @"(?:tính|kết quả|bằng mấy|\?|\=)?\s*(\d+(?:\.\d+)?)\s*([\+\-\*\/xX]|cộng|trừ|nhân|chia)\s*(\d+(?:\.\d+)?)\s*(?:bằng mấy|\=|\?)?");
        if (mathMatch.Success)
        {
            if (double.TryParse(mathMatch.Groups[1].Value, out var n1) && double.TryParse(mathMatch.Groups[3].Value, out var n2))
            {
                var op = mathMatch.Groups[2].Value.ToLowerInvariant();
                double result = 0;
                bool valid = true;
                string opSymbol = "+";

                if (op is "+" or "cộng") { result = n1 + n2; opSymbol = "+"; }
                else if (op is "-" or "trừ") { result = n1 - n2; opSymbol = "-"; }
                else if (op is "*" or "x" or "nhân") { result = n1 * n2; opSymbol = "×"; }
                else if (op is "/" or "chia")
                {
                    if (Math.Abs(n2) < 0.000001)
                    {
                        reply = "Phép chia cho 0 không xác định trong toán học.";
                        return true;
                    }
                    result = n1 / n2;
                    opSymbol = "÷";
                }
                else { valid = false; }

                if (valid)
                {
                    reply = $"Kết quả phép tính: **{n1} {opSymbol} {n2} = {result}**";
                    return true;
                }
            }
        }

        // 2. Chào hỏi cơ bản
        var greetings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "xin chao", "chao", "hi", "hello", "alo", "chao ban", "chao bot", "chao em", "chao anh", "chao thay",
            "chao co", "xin chao bro", "chao bro", "hey", "helo", "hi bot", "hello bot", "xin chao ban", "chao ae",
            "xin chào", "chào", "chào bạn", "chào bot", "chào em", "chào anh", "chào thầy", "chào cô", "xin chào bro",
            "chào bro", "xin chào bạn", "chào ae", "alo bot", "hi bro", "hello bro", "helo bro", "chào bạn nhé",
            "xin chào bạn nhé", "chào buổi sáng", "chào buổi tối", "chúc buổi sáng", "good morning", "good evening"
        };

        if (greetings.Contains(normalized) || greetings.Contains(raw))
        {
            var role = user?.Role ?? "Student";
            if (role == AuthRoles.Student || role == "hoc_sinh")
            {
                reply = "Xin chào! Chào bạn. Tôi là Trợ lý Học thuật của hệ thống AET LMS. Rất vui được hỗ trợ bạn hôm nay. Bạn cần tư vấn về phương pháp học tập, tra cứu môn học, bài giảng hay các quy trình học vụ trên hệ thống LMS nhé?";
            }
            else if (role == AuthRoles.Teacher || role == "giao_vien")
            {
                reply = "Xin chào Thầy/Cô! Tôi là Trợ lý AI hệ thống AET LMS. Rất sẵn lòng hỗ trợ Thầy/Cô về soạn giáo án, ngân hàng câu hỏi kiểm tra hoặc tra cứu quy trình học vụ.";
            }
            else
            {
                reply = "Xin chào! Tôi là Trợ lý AI của hệ thống AET LMS. Tôi có thể hỗ trợ gì cho bạn hôm nay?";
            }
            return true;
        }

        // 3. Giới thiệu danh tính
        var identityQueries = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ban la ai", "bạn là ai", "who are you", "gioi thieu ve ban", "giới thiệu về bạn", "ten ban la gi", "tên bạn là gì"
        };
        if (identityQueries.Contains(normalized))
        {
            reply = "Tôi là Trợ lý Học thuật AI của hệ thống giáo dục AET LMS. Tôi hỗ trợ sinh viên và giảng viên tra cứu bài học, ôn luyện kiến thức, giải đáp quy chế học vụ và tối ưu trải nghiệm học tập.";
            return true;
        }

        // 4. Quy trình phúc khảo điểm số
        if (normalized.Contains("phuc khao") || normalized.Contains("phúc khảo") || (normalized.Contains("cham lai") && normalized.Contains("diem")))
        {
            reply = "### 📋 Quy trình phúc khảo điểm số trên hệ thống AET LMS:\n\n"
                  + "1. **Thời hạn nộp đơn:** Sinh viên gửi đơn phúc khảo trực tuyến trên LMS trong vòng **03 - 05 ngày làm việc** kể từ ngày công bố điểm thi chính thức.\n"
                  + "2. **Tiếp nhận & Xử lý:** Phòng Giáo vụ tiếp nhận, kiểm tra tính hợp lệ và chuyển bài thi đến Hội đồng chấm/Giảng viên chấm phúc khảo.\n"
                  + "3. **Thời gian trả kết quả:** Kết quả chấm phúc khảo được cập nhật vào bảng điểm cá nhân trên LMS trong vòng **07 ngày làm việc**.\n"
                  + "4. **Cách thức nộp đơn:** Bạn truy cập mục **Đơn từ / Dịch vụ trực tuyến** > Chọn loại đơn **'Đơn xin phúc khảo bài thi'** > Chọn môn học, nhập lý do và gửi duyệt.";
            return true;
        }

        // 5. Công thức tính điểm trung bình học kỳ & GPA / CPA
        if (normalized.Contains("tinh diem trung binh") || normalized.Contains("tính điểm trung bình")
            || normalized.Contains("cong thuc gpa") || normalized.Contains("công thức gpa")
            || normalized.Contains("tinh gpa") || normalized.Contains("tính gpa")
            || (normalized.Contains("diem trung binh") && normalized.Contains("hoc ky"))
            || (normalized.Contains("tinh diem") && normalized.Contains("hoc ky")))
        {
            reply = "### 📊 Công thức tính điểm trung bình học kỳ (GPA) & tích lũy (CPA):\n\n"
                  + "Điểm trung bình được tính theo **trọng số tín chỉ** của từng môn học:\n\n"
                  + "$$\\text{GPA} = \\frac{\\sum (\\text{Điểm chữ quy đổi thang 4}_i \\times \\text{Số tín chỉ}_i)}{\\sum \\text{Số tín chỉ}}$$\n\n"
                  + "#### 📌 Bảng quy đổi điểm sang Thang 4:\n"
                  + "- **A (8.5 - 10.0):** 4.0 điểm *(Xuất sắc / Giỏi)*\n"
                  + "- **B+ (7.8 - 8.4):** 3.5 điểm *(Khá giỏi)*\n"
                  + "- **B (7.0 - 7.7):** 3.0 điểm *(Khá)*\n"
                  + "- **C+ (6.3 - 6.9):** 2.5 điểm *(Trung bình khá)*\n"
                  + "- **C (5.5 - 6.2):** 2.0 điểm *(Trung bình)*\n"
                  + "- **D+ (4.8 - 5.4):** 1.5 điểm *(Trung bình yếu)*\n"
                  + "- **D (4.0 - 4.7):** 1.0 điểm *(Yếu - Đạt)*\n"
                  + "- **F (< 4.0):** 0.0 điểm *(Kém - Không đạt / Học lại)*\n\n"
                  + "*(Lưu ý: Điểm môn học gồm Điểm Quá trình + Chuyên cần + Điểm Thi kết thúc học phần theo tỷ lệ trọng số cấu hình của từng môn).*";
            return true;
        }

        // 6. Quy chế điểm danh & vắng học
        if (normalized.Contains("diem danh") || normalized.Contains("điểm danh")
            || normalized.Contains("vang toi da") || normalized.Contains("vắng tối đa")
            || normalized.Contains("nghi hoc toi da") || normalized.Contains("nghỉ học tối đa"))
        {
            reply = "### ⏱️ Quy định về Điểm danh & Chuyên cần tại AET LMS:\n\n"
                  + "1. **Tỷ lệ vắng tối đa cho phép:** Sinh viên không được vắng quá **20% tổng số giờ học/tiết học** của môn học (tương đương tối đa 3-4 buổi tùy số tín chỉ môn).\n"
                  + "2. **Hậu quả khi vắng quá quy định:** Sinh viên vắng quá 20% sẽ bị **cấm thi kết thúc học phần** (đạt điểm F môn học) và phải đăng ký học lại.\n"
                  + "3. **Điểm danh muộn / Có phép:** Đi học muộn quá 15 phút tính là 0.5 buổi vắng. Nghỉ ốm có giấy xác nhận y tế cần nộp đơn xin phép trên LMS trong vòng 48h để được xem xét.";
            return true;
        }

        // 7. Quy chế thi lại, học lại, tốt nghiệp
        if (normalized.Contains("tot nghiep") || normalized.Contains("tốt nghiệp")
            || normalized.Contains("bao luu") || normalized.Contains("bảo lưu")
            || normalized.Contains("rut mon") || normalized.Contains("rút môn")
            || normalized.Contains("thi lai") || normalized.Contains("thi lại"))
        {
            reply = "### 🎓 Quy định Học vụ chung trên AET LMS:\n\n"
                  + "- **Điều kiện tốt nghiệp:** Tích lũy đủ số tín chỉ theo chương trình đào tạo, GPA tích lũy $\\ge$ 2.00 (thang 4), hoàn thành chuẩn đầu ra ngoại ngữ/tin học và không trong thời gian kỷ luật.\n"
                  + "- **Đăng ký thi lại / Học lại:** Sinh viên rớt môn (điểm F) có thể nộp đơn thi lại (nếu có ca thi mở) hoặc đăng ký học lại vào học kỳ tiếp theo.\n"
                  + "- **Rút học phần / Bảo lưu:** Nộp đơn trực tuyến trong vòng 2 tuần đầu học kỳ để được hoàn phí/bảo lưu kết quả học tập.";
            return true;
        }

        reply = string.Empty;
        return false;
    }

    private static string CleanThinkingFallback(string thinking)
    {
        var lastQuotes = System.Text.RegularExpressions.Regex.Matches(thinking, @"[""']([^""']{15,})[""']");
        if (lastQuotes.Count > 0)
        {
            var bestMatch = lastQuotes[^1].Groups[1].Value.Trim();
            if (!bestMatch.StartsWith("Thinking", StringComparison.OrdinalIgnoreCase))
            {
                return bestMatch;
            }
        }

        return "Tôi đã tiếp nhận câu hỏi của bạn. Xin vui lòng cung cấp thêm chi tiết môn học hoặc bài giảng để tôi hỗ trợ giải đáp chính xác nhất nhé.";
    }

    private static bool ModelMatches(string actualModelName, string targetModelName)
    {
        if (string.IsNullOrWhiteSpace(actualModelName) || string.IsNullOrWhiteSpace(targetModelName))
            return false;

        var cleanActual = actualModelName.Split(':')[0].Trim();
        var cleanTarget = targetModelName.Split(':')[0].Trim();

        return actualModelName.Equals(targetModelName, StringComparison.OrdinalIgnoreCase)
            || actualModelName.StartsWith(targetModelName, StringComparison.OrdinalIgnoreCase)
            || cleanActual.Equals(cleanTarget, StringComparison.OrdinalIgnoreCase);
    }

    private AiChatResponse CreateFastResponse(string answer, string conversationId)
    {
        return new AiChatResponse
        {
            Answer = answer,
            Thinking = null,
            ProcessingTimeMs = 1950,
            ConversationId = conversationId,
            Model = _options.ChatModel,
            Sources = new List<string>()
        };
    }

    private async Task<StudentAcademicContext?> LoadStudentAcademicContextAsync(int studentId, CancellationToken cancellationToken)
    {
        try
        {
            var student = await _db.NguoiDungs
                .AsNoTracking()
                .Include(u => u.Lop)
                .FirstOrDefaultAsync(u => u.MaNguoiDung == studentId, cancellationToken);

            if (student == null) return null;

            // 1. Điểm số cá nhân
            var scores = await _db.DiemSos
                .AsNoTracking()
                .Include(d => d.MonHoc)
                .Include(d => d.HocKy)
                .Where(d => d.MaHocSinh == studentId)
                .OrderByDescending(d => d.MaHocKy)
                .ThenBy(d => d.MonHoc != null ? d.MonHoc.TenMonHoc : "")
                .ToListAsync(cancellationToken);

            var items = scores.Select(d => new StudentGradeItem
            {
                SubjectCode = d.MonHoc?.MaCodeMonHoc ?? "",
                SubjectName = d.MonHoc?.TenMonHoc ?? "Môn học",
                Credits = d.MonHoc?.SoTinChi ?? 0,
                SemesterName = d.HocKy != null ? $"{d.HocKy.TenHocKy} {d.HocKy.NamHoc}" : "Chưa xác định",
                ProcessScore = d.DiemQuaTrinh,
                MidtermScore = d.DiemGiuaKy,
                FinalScore = d.DiemCuoiKy,
                Gpa = d.GpaMonHoc,
                Status = d.TrangThai,
                StatusLabel = d.TrangThai == "dat" ? "Đạt" : d.TrangThai == "khong_dat" ? "Chưa đạt" : "Đang học",
                Note = d.LyDoRot
            }).ToList();

            var gpaValues = scores.Where(d => d.GpaMonHoc > 0).Select(d => (double)d.GpaMonHoc).ToList();
            var cumulativeGpa = gpaValues.Any() ? Math.Round(gpaValues.Average(), 2) : 0;
            var earnedCredits = items.Where(x => x.Status == "dat").Sum(x => x.Credits);
            var passedCount = items.Count(x => x.Status == "dat");
            var failedCount = items.Count(x => x.Status == "khong_dat");

            string classification;
            if (cumulativeGpa >= 8.5 || (cumulativeGpa >= 3.6 && cumulativeGpa <= 4.0)) classification = "Xuất sắc";
            else if (cumulativeGpa >= 7.8 || (cumulativeGpa >= 3.2 && cumulativeGpa < 3.6)) classification = "Giỏi";
            else if (cumulativeGpa >= 6.5 || (cumulativeGpa >= 2.5 && cumulativeGpa < 3.2)) classification = "Khá";
            else if (cumulativeGpa >= 5.0 || (cumulativeGpa >= 2.0 && cumulativeGpa < 2.5)) classification = "Trung bình";
            else classification = "Yếu";

            // 2. Lịch học cá nhân (Hôm nay & 7 ngày tới)
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var nextWeek = today.AddDays(7);

            var upcomingSessions = await _db.BuoiHocs
                .AsNoTracking()
                .Include(b => b.KhoaHoc)
                    .ThenInclude(k => k!.MonHoc)
                .Include(b => b.CaHoc)
                .Include(b => b.Phong)
                .Include(b => b.GiaoVien)
                .Where(b => b.NgayHoc >= today && b.NgayHoc <= nextWeek
                    && b.TrangThaiBuoi != "da_huy"
                    && b.KhoaHoc != null
                    && (student.MaLop != null && b.KhoaHoc.MaLop == student.MaLop))
                .OrderBy(b => b.NgayHoc)
                .ThenBy(b => b.CaHoc != null ? b.CaHoc.GioBatDau : TimeOnly.MinValue)
                .Take(7)
                .ToListAsync(cancellationToken);

            var scheduleItems = upcomingSessions.Select(b => new StudentScheduleItem
            {
                Date = b.NgayHoc,
                SubjectName = b.KhoaHoc?.MonHoc?.TenMonHoc ?? b.KhoaHoc?.TieuDe ?? "Buổi học",
                ShiftName = b.CaHoc?.TenCa ?? "Ca học",
                TimeRange = b.CaHoc != null ? $"{b.CaHoc.GioBatDau:HH\\:mm} - {b.CaHoc.GioKetThuc:HH\\:mm}" : "",
                Room = b.Phong?.TenPhong ?? "Chưa xếp phòng",
                TeacherName = b.GiaoVien?.HoTen ?? "Giảng viên"
            }).ToList();

            // 3. Chuyên cần / Điểm danh cá nhân
            var attendanceRecords = await _db.DiemDanhs
                .AsNoTracking()
                .Where(d => d.MaHocSinh == studentId)
                .ToListAsync(cancellationToken);

            var totalSessions = attendanceRecords.Count;
            var presentSessions = attendanceRecords.Count(d => d.TrangThai == "co_mat");
            var excusedAbsence = attendanceRecords.Count(d => d.TrangThai == "vang_co_phep");
            var unexcusedAbsence = attendanceRecords.Count(d => d.TrangThai == "vang_khong_phep");
            var lateSessions = attendanceRecords.Count(d => d.TrangThai == "muon");

            var attendanceSummary = new StudentAttendanceSummary
            {
                TotalSessions = totalSessions,
                PresentSessions = presentSessions,
                ExcusedAbsence = excusedAbsence,
                UnexcusedAbsence = unexcusedAbsence,
                LateSessions = lateSessions,
                AttendanceRate = totalSessions > 0 ? Math.Round((double)presentSessions / totalSessions * 100, 1) : 100.0
            };

            // 4. Bài tập / Nhiệm vụ chưa nộp sắp đến hạn
            var now = DateTime.UtcNow.AddHours(7);
            var monHocIds = scores.Select(s => s.MaMonHoc).Distinct().ToList();

            var pendingAssignments = await _db.BaiTaps
                .AsNoTracking()
                .Include(bt => bt.MonHoc)
                .Where(bt => bt.HanNop >= now
                    && bt.TrangThai == "da_xuat_ban"
                    && (monHocIds.Contains(bt.MaMonHoc) || monHocIds.Count == 0)
                    && !_db.BaiNops.Any(bn => bn.MaBaiTap == bt.MaBaiTap && bn.MaHocSinh == studentId))
                .OrderBy(bt => bt.HanNop)
                .Take(5)
                .Select(bt => new StudentPendingAssignmentItem
                {
                    Title = bt.TieuDe,
                    SubjectName = bt.MonHoc != null ? bt.MonHoc.TenMonHoc : "Môn học",
                    Deadline = bt.HanNop
                })
                .ToListAsync(cancellationToken);

            return new StudentAcademicContext
            {
                StudentId = studentId,
                StudentName = string.IsNullOrWhiteSpace(student.HoTen) ? "Sinh viên" : student.HoTen,
                Email = student.Email,
                ClassName = student.Lop?.TenLop ?? "Chưa phân lớp",
                CumulativeGpa = cumulativeGpa,
                EarnedCredits = earnedCredits,
                PassedSubjectsCount = passedCount,
                FailedSubjectsCount = failedCount,
                Classification = classification,
                Grades = items,
                UpcomingClasses = scheduleItems,
                AttendanceSummary = attendanceSummary,
                PendingAssignments = pendingAssignments
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load student academic context for student {StudentId}", studentId);
            return null;
        }
    }

    private static string NormalizeVietnamese(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var raw = input.Trim().ToLowerInvariant();
        var normalizedString = raw.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizedString.Length);
        foreach (var c in normalizedString)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        var cleaned = sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
        return System.Text.RegularExpressions.Regex.Replace(cleaned, @"[?.!,~@#$%^&*()_+=\[\]{};:'""\\/<>|`]", " ").Trim();
    }

    private static bool TryGetStudentGradeReply(
        string input,
        CurrentUserContext? user,
        StudentAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isGradeQuery = (v.Contains("diem") && !v.Contains("diem danh")) || v.Contains("bang diem") || v.Contains("gpa")
            || v.Contains("ket qua hoc tap") || v.Contains("ket qua ky") || v.Contains("ket qua hoc ky")
            || v.Contains("xem diem") || v.Contains("qua mon") || v.Contains("rot mon") || v.Contains("tong ket");

        bool isAnalyticalQuery = v.Contains("cao nhat") || v.Contains("thap nhat") || v.Contains("khuyen")
            || v.Contains("tu van") || v.Contains("co nen") || v.Contains("lam sao de") || v.Contains("tai sao");

        if (!isGradeQuery || isAnalyticalQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Student && user.Role != "hoc_sinh"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Sinh viên** để tra cứu bảng điểm và kết quả học tập cá nhân của mình nhé.";
            return true;
        }

        if (context == null)
        {
            reply = "Không tìm thấy dữ liệu học tập cá nhân trên hệ thống. Vui lòng kiểm tra lại tài khoản hoặc liên hệ phòng Giáo vụ để được hỗ trợ.";
            return true;
        }

        if (context.Grades.Count == 0)
        {
            reply = $"### 📊 Kết quả học tập cá nhân\n\n"
                  + $"Chào bạn **{context.StudentName}** *(Lớp: {context.ClassName} - Email: `{context.Email}`)*,\n\n"
                  + "Hiện tại hệ thống ghi nhận bạn **chưa có điểm số chính thức nào được công bố** cho các học kỳ gần đây.\n\n"
                  + "📌 **Gợi ý dành cho bạn:**\n"
                  + "- Bạn có thể vào mục **Khóa học của tôi** hoặc **Lịch học** trên thanh điều hướng để theo dõi tiến độ bài giảng.\n"
                  + "- Điểm quá trình và điểm thi kết thúc học phần sẽ được cập nhật tự động khi Giảng viên hoàn tất chấm bài.\n"
                  + "- Nếu bạn cần kiểm tra tình trạng môn học, hãy gửi yêu cầu hỗ trợ hoặc liên hệ phòng **Giáo vụ** nhé!";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"### 📊 Bảng điểm & Kết quả học tập cá nhân");
        sb.AppendLine();
        sb.AppendLine($"Chào bạn **{context.StudentName}** *(Lớp: **{context.ClassName}** - Email: `{context.Email}`)*,");
        sb.AppendLine();
        sb.AppendLine("Dưới đây là kết quả học tập thực tế của bạn trên hệ thống AET LMS:");
        sb.AppendLine();
        sb.AppendLine("#### 📌 Tổng quan kết quả:");
        sb.AppendLine($"- **Điểm trung bình tích lũy (GPA):** **{context.CumulativeGpa:0.00}** *(Xếp loại: **{context.Classification}**)*");
        sb.AppendLine($"- **Tổng tín chỉ tích lũy:** **{context.EarnedCredits} / 120 tín chỉ**");
        sb.AppendLine($"- **Số môn đạt:** **{context.PassedSubjectsCount} môn** | **Số môn chưa đạt:** **{context.FailedSubjectsCount} môn**");
        sb.AppendLine();
        sb.AppendLine("#### 📝 Bảng điểm chi tiết các môn học:");
        sb.AppendLine();
        sb.AppendLine("| Môn học | Mã môn | Tín chỉ | Học kỳ | Điểm QT | Điểm GK | Điểm CK | GPA môn | Trạng thái |");
        sb.AppendLine("| :--- | :--- | :---: | :--- | :---: | :---: | :---: | :---: | :---: |");

        foreach (var g in context.Grades)
        {
            var qt = g.ProcessScore.HasValue ? g.ProcessScore.Value.ToString("0.0") : "-";
            var gk = g.MidtermScore.HasValue ? g.MidtermScore.Value.ToString("0.0") : "-";
            var ck = g.FinalScore.HasValue ? g.FinalScore.Value.ToString("0.0") : "-";
            var statusBadge = g.Status == "dat" ? "✅ Đạt" : g.Status == "khong_dat" ? "❌ Chưa đạt" : "⏳ Đang học";

            sb.AppendLine($"| **{g.SubjectName}** | `{g.SubjectCode}` | {g.Credits} | {g.SemesterName} | {qt} | {gk} | {ck} | **{g.Gpa:0.0}** | {statusBadge} |");
        }

        sb.AppendLine();
        sb.AppendLine("#### 💡 Lời khuyên học thuật:");
        if (context.Classification is "Xuất sắc" or "Giỏi")
        {
            sb.AppendLine("- 🎉 **Chúc mừng bạn!** Bạn đang duy trì phong độ học tập rất xuất sắc. Hãy tiếp tục phát huy ở các kỳ học tới.");
        }
        else if (context.FailedSubjectsCount > 0)
        {
            sb.AppendLine("- ⚠️ **Lưu ý:** Bạn có môn chưa đạt. Bạn có thể nộp đơn **Đăng ký thi lại** hoặc **Học lại** trong mục Dịch vụ trực tuyến để cải thiện điểm nhé.");
        }
        else
        {
            sb.AppendLine("- 💪 Hãy tiếp tục ôn tập đều đặn các môn học và tham gia đầy đủ các buổi học để giữ vững kết quả này nhé!");
        }

        reply = sb.ToString();
        return true;
    }

    private static bool TryGetStudentScheduleReply(
        string input,
        CurrentUserContext? user,
        StudentAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isScheduleQuery = v.Contains("lich hoc") || v.Contains("thoi khoa bieu")
            || (v.Contains("hoc") && (v.Contains("hom nay") || v.Contains("ngay mai") || v.Contains("tuan nay") || v.Contains("tiep theo") || v.Contains("gio")));

        if (!isScheduleQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Student && user.Role != "hoc_sinh"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Sinh viên** để tra cứu thời khóa biểu cá nhân của mình nhé.";
            return true;
        }

        if (context == null || context.UpcomingClasses.Count == 0)
        {
            reply = "### 📅 Thời khóa biểu & Lịch học cá nhân\n\n"
                  + $"Chào bạn **{context?.StudentName ?? "Sinh viên"}**,\n\n"
                  + "Hiện tại hệ thống **chưa ghi nhận buổi học nào trong 7 ngày tới** của lớp bạn. Bạn có thể kiểm tra danh sách khóa học đã đăng ký hoặc theo dõi thông báo từ Giảng viên nhé!";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"### 📅 Lịch học cá nhân sắp tới (7 ngày tới)");
        sb.AppendLine();
        sb.AppendLine($"Chào bạn **{context.StudentName}** *(Lớp: **{context.ClassName}**)*,");
        sb.AppendLine();
        sb.AppendLine("| Ngày học | Môn học | Ca học & Giờ | Phòng học | Giảng viên |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var sc in context.UpcomingClasses)
        {
            sb.AppendLine($"| **{sc.Date:dd/MM/yyyy}** | {sc.SubjectName} | {sc.ShiftName} ({sc.TimeRange}) | 📍 {sc.Room} | 👨‍🏫 {sc.TeacherName} |");
        }

        sb.AppendLine();
        sb.AppendLine("💡 *Bạn nhớ đến lớp trước giờ bắt đầu 10-15 phút để đảm bảo điểm danh đầy đủ nhé!*");

        reply = sb.ToString();
        return true;
    }

    private static bool TryGetStudentAttendanceReply(
        string input,
        CurrentUserContext? user,
        StudentAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isAttendanceQuery = v.Contains("chuyen can") || v.Contains("diem danh") || v.Contains("vang")
            || v.Contains("cam thi") || v.Contains("di muon") || v.Contains("nghi hoc");

        if (!isAttendanceQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Student && user.Role != "hoc_sinh"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Sinh viên** để tra cứu tình hình chuyên cần và điểm danh của mình nhé.";
            return true;
        }

        var att = context?.AttendanceSummary ?? new StudentAttendanceSummary();

        var sb = new StringBuilder();
        sb.AppendLine($"### ⏱️ Tình hình Điểm danh & Chuyên cần cá nhân");
        sb.AppendLine();
        sb.AppendLine($"Chào bạn **{context?.StudentName ?? "Sinh viên"}** *(Lớp: **{context?.ClassName ?? "N/A"}**)*,");
        sb.AppendLine();
        sb.AppendLine("#### 📌 Báo cáo chuyên cần:");
        sb.AppendLine($"- **Tổng số buổi học đã ghi nhận:** **{att.TotalSessions} buổi**");
        sb.AppendLine($"- **Số buổi có mặt:** **{att.PresentSessions} buổi** (✅)");
        sb.AppendLine($"- **Số buổi đi muộn:** **{att.LateSessions} buổi** (⏳)");
        sb.AppendLine($"- **Số buổi vắng có phép:** **{att.ExcusedAbsence} buổi** (📝)");
        sb.AppendLine($"- **Số buổi vắng không phép:** **{att.UnexcusedAbsence} buổi** (❌)");
        sb.AppendLine($"- **Tỷ lệ chuyên cần đạt:** **{att.AttendanceRate:0.0}%**");
        sb.AppendLine();

        var totalAbsent = att.ExcusedAbsence + att.UnexcusedAbsence;
        if (att.AttendanceRate < 80.0 && att.TotalSessions > 5)
        {
            sb.AppendLine("⚠️ **CẢNH BÁO NGUY CƠ CẤM THI:** Tỷ lệ vắng của bạn đang ở mức cao. Theo quy chế AET LMS, sinh viên vắng quá 20% tổng số tiết sẽ bị cấm thi kết thúc học phần. Hãy chú ý đi học đầy đủ các buổi còn lại!");
        }
        else
        {
            sb.AppendLine("✅ **Tình trạng tốt:** Bạn đang duy trì tỷ lệ chuyên cần an toàn. Hãy tiếp tục duy trì nhé!");
        }

        reply = sb.ToString();
        return true;
    }

    private static bool TryGetStudentAssignmentsReply(
        string input,
        CurrentUserContext? user,
        StudentAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isAssignmentQuery = v.Contains("bai tap") || v.Contains("deadline") || v.Contains("bai kiem tra")
            || (v.Contains("kiem tra") && (v.Contains("chua") || v.Contains("con") || v.Contains("den han") || v.Contains("nop")))
            || (v.Contains("nop") && (v.Contains("chua") || v.Contains("con") || v.Contains("bai")));

        if (!isAssignmentQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Student && user.Role != "hoc_sinh"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Sinh viên** để tra cứu bài tập và deadline của mình nhé.";
            return true;
        }

        if (context == null || context.PendingAssignments.Count == 0)
        {
            reply = "### 📝 Bài tập & Deadline cá nhân\n\n"
                  + $"Chào bạn **{context?.StudentName ?? "Sinh viên"}**,\n\n"
                  + "🎉 **Tuyệt vời!** Hiện tại bạn **không có bài tập nào chưa nộp hoặc sắp đến hạn**. Hãy tiếp tục theo dõi các thông báo bài tập mới từ Giảng viên nhé!";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"### 📝 Danh sách bài tập chưa hoàn thành (Sắp đến hạn)");
        sb.AppendLine();
        sb.AppendLine($"Chào bạn **{context.StudentName}**, bạn có **{context.PendingAssignments.Count} bài tập** cần lưu ý hoàn thành:");
        sb.AppendLine();
        sb.AppendLine("| Môn học | Tên bài tập | Hạn nộp | Thời gian còn lại |");
        sb.AppendLine("| :--- | :--- | :--- | :--- |");

        var now = DateTime.UtcNow.AddHours(7);
        foreach (var pa in context.PendingAssignments)
        {
            var timeLeft = pa.Deadline - now;
            var timeStr = timeLeft.TotalHours > 24
                ? $"Còn {(int)timeLeft.TotalDays} ngày {timeLeft.Hours} giờ"
                : $"⚠️ Còn {timeLeft.Hours} giờ {timeLeft.Minutes} phút";

            sb.AppendLine($"| **{pa.SubjectName}** | {pa.Title} | ⏰ {pa.Deadline:HH:mm dd/MM/yyyy} | {timeStr} |");
        }

        sb.AppendLine();
        sb.AppendLine("💡 *Bạn hãy vào mục **Khóa học của tôi** để làm và nộp bài trước hạn quy định nhé!*");

        reply = sb.ToString();
        return true;
    }

    private sealed class StudentAcademicContext
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public double CumulativeGpa { get; set; }
        public int EarnedCredits { get; set; }
        public int PassedSubjectsCount { get; set; }
        public int FailedSubjectsCount { get; set; }
        public string Classification { get; set; } = string.Empty;
        public List<StudentGradeItem> Grades { get; set; } = new();
        public List<StudentScheduleItem> UpcomingClasses { get; set; } = new();
        public StudentAttendanceSummary AttendanceSummary { get; set; } = new();
        public List<StudentPendingAssignmentItem> PendingAssignments { get; set; } = new();
    }

    private sealed class StudentGradeItem
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public decimal? ProcessScore { get; set; }
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal Gpa { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    private sealed class StudentScheduleItem
    {
        public DateOnly Date { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
    }

    private sealed class StudentAttendanceSummary
    {
        public int TotalSessions { get; set; }
        public int PresentSessions { get; set; }
        public int ExcusedAbsence { get; set; }
        public int UnexcusedAbsence { get; set; }
        public int LateSessions { get; set; }
        public double AttendanceRate { get; set; } = 100.0;
    }

    private sealed class StudentPendingAssignmentItem
    {
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
    }

    private sealed class OllamaTagsResponse
    {
        public List<OllamaModelItem>? Models { get; set; }
    }

    private sealed class OllamaModelItem
    {
        public string? Name { get; set; }
        public string? Model { get; set; }
    }

    private sealed class OllamaChatPayload
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OllamaChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("options")]
        public OllamaChatOptions? Options { get; set; }
    }

    private sealed class OllamaChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; }
    }

    private sealed class OllamaChatOptions
    {
        [JsonPropertyName("num_ctx")]
        public int NumCtx { get; set; }

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }
    }

    private sealed class OllamaChatResult
    {
        public string? Model { get; set; }
        public OllamaChatMessage? Message { get; set; }
        public bool Done { get; set; }
    }

    private sealed class OllamaEmbedResult
    {
        public string? Model { get; set; }
        public float[][]? Embeddings { get; set; }
    }
}
