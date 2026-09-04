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
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Services.AI;

public partial class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly IAiRequestGate _gate;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OllamaService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IAiAcademicQueryResolver _academicQueryResolver;

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
        ILogger<OllamaService> logger,
        IMemoryCache cache,
        IAiAcademicQueryResolver academicQueryResolver)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _gate = gate;
        _db = db;
        _logger = logger;
        _cache = cache;
        _academicQueryResolver = academicQueryResolver;

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

        // 1. Tải ngữ cảnh học tập / giảng dạy thực tế
        StudentAcademicContext? studentAcademicContext = null;
        TeacherAcademicContext? teacherAcademicContext = null;

        if (userContext != null && (userContext.Role == AuthRoles.Student || userContext.Role == "hoc_sinh") && userContext.UserId > 0)
        {
            studentAcademicContext = await LoadStudentAcademicContextAsync(userContext.UserId, cancellationToken);
        }
        else if (userContext != null && (userContext.Role == AuthRoles.Teacher || userContext.Role == "giao_vien") && userContext.UserId > 0)
        {
            teacherAcademicContext = await LoadTeacherAcademicContextAsync(userContext.UserId, cancellationToken);
        }

        var normalizedMsg = NormalizeVietnamese(request.Message);
        var targetModel = (request.Mode?.ToLower() == "deep") ? _options.DeepModel : _options.FastModel;
        if (string.IsNullOrWhiteSpace(targetModel)) targetModel = _options.ChatModel;

        // 1.1. Ưu tiên hàng đầu: Nhận diện yêu cầu AI hành động tạo Đề tự luyện / Quiz / File Word
        bool isAskingForAdvice = normalizedMsg.Contains("goi y") || normalizedMsg.Contains("huong dan") || normalizedMsg.Contains("cach") || normalizedMsg.Contains("tu van");
        bool isCreateQuizIntent = !isAskingForAdvice && (
            normalizedMsg.Contains("tao quiz") || normalizedMsg.Contains("tao de thi")
            || normalizedMsg.Contains("tao de kiem tra") || normalizedMsg.Contains("tao bai kiem tra")
            || normalizedMsg.Contains("sinh quiz") || normalizedMsg.Contains("de tu luyen")
            || (normalizedMsg.Contains("file word") && (normalizedMsg.Contains("de thi") || normalizedMsg.Contains("de on tap") || normalizedMsg.Contains("de kiem tra")))
        );

        if (isCreateQuizIntent)
        {
            // Trích xuất số lượng câu hỏi mà người dùng mong muốn (không giới hạn cứng 5 câu)
            int questionCount = 5;
            var countMatch = System.Text.RegularExpressions.Regex.Match(request.Message, @"(\d+)\s*(?:câu|cau|question)");
            if (countMatch.Success && int.TryParse(countMatch.Groups[1].Value, out var parsedCount) && parsedCount > 0)
            {
                questionCount = Math.Clamp(parsedCount, 1, 30);
            }

            var isStudent = userContext?.Role == AuthRoles.Student || userContext?.Role == "hoc_sinh";
            var allMonHocs = await _db.DanhMucMonHocs.AsNoTracking().ToListAsync(cancellationToken);

            // Thu thập môn học mà sinh viên đã hoặc đang học trong chương trình
            HashSet<string>? studentSubjectCodes = null;
            List<string>? studentSubjectNames = null;
            if (isStudent && studentAcademicContext != null && studentAcademicContext.Grades.Count > 0)
            {
                studentSubjectCodes = studentAcademicContext.Grades.Select(g => g.SubjectCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
                studentSubjectNames = studentAcademicContext.Grades.Select(g => g.SubjectName).ToList();
            }

            DanhMucMonHoc? matchedMonHoc = null;
            foreach (var m in allMonHocs)
            {
                if (normalizedMsg.Contains(NormalizeVietnamese(m.TenMonHoc)) ||
                    (!string.IsNullOrEmpty(m.MaCodeMonHoc) && normalizedMsg.Contains(NormalizeVietnamese(m.MaCodeMonHoc))))
                {
                    matchedMonHoc = m;
                    break;
                }
            }

            if (matchedMonHoc == null)
            {
                if (normalizedMsg.Contains("csdl") || normalizedMsg.Contains("co so du lieu"))
                    matchedMonHoc = allMonHocs.FirstOrDefault(m => NormalizeVietnamese(m.TenMonHoc).Contains("co so du lieu"));
                else if (normalizedMsg.Contains("lap trinh") || normalizedMsg.Contains("nhap mon"))
                    matchedMonHoc = allMonHocs.FirstOrDefault(m => NormalizeVietnamese(m.TenMonHoc).Contains("lap trinh"));
                else if (normalizedMsg.Contains("web"))
                    matchedMonHoc = allMonHocs.FirstOrDefault(m => NormalizeVietnamese(m.TenMonHoc).Contains("web"));
            }

            // Kiểm tra giới hạn chương trình sinh viên đã học / đang học
            bool isOutsideCurriculum = false;
            if (isStudent && studentSubjectCodes != null && studentSubjectCodes.Count > 0 && matchedMonHoc != null)
            {
                bool isEnrolled = studentSubjectCodes.Contains(matchedMonHoc.MaCodeMonHoc) ||
                    (studentSubjectNames != null && studentSubjectNames.Any(n => NormalizeVietnamese(n).Contains(NormalizeVietnamese(matchedMonHoc.TenMonHoc))));

                if (!isEnrolled)
                {
                    isOutsideCurriculum = true;
                    var enrolledMonHoc = allMonHocs.FirstOrDefault(m => studentSubjectCodes.Contains(m.MaCodeMonHoc));
                    if (enrolledMonHoc != null)
                    {
                        matchedMonHoc = enrolledMonHoc;
                    }
                }
            }

            var targetMonHoc = matchedMonHoc ?? allMonHocs.FirstOrDefault();
            if (targetMonHoc != null)
            {
                var cleanTopic = CleanTopic(request.Message, targetMonHoc.TenMonHoc);
                var tieuDe = isStudent
                    ? (cleanTopic.Equals(targetMonHoc.TenMonHoc, StringComparison.OrdinalIgnoreCase) 
                        ? $"Đề tự luyện - {targetMonHoc.TenMonHoc}" 
                        : $"Đề tự luyện - {targetMonHoc.TenMonHoc} ({cleanTopic})")
                    : $"Kiểm tra nhanh - {targetMonHoc.TenMonHoc}";

                var quizRes = await GenerateQuizAsync(new AiGenerateQuizRequest
                {
                    MaMonHoc = targetMonHoc.MaMonHoc,
                    TieuDe = tieuDe,
                    ChuDe = cleanTopic,
                    SoLuongCauHoi = questionCount,
                    ThoiGianPhut = Math.Max(15, questionCount * 3),
                    DoKho = "trung_binh"
                }, userContext, cancellationToken);

                var answerSb = new StringBuilder();
                answerSb.AppendLine($"🎉 **AI đã hoàn tất việc tạo bộ đề tự luyện {quizRes.TongSoCau} câu và đóng gói thành file Word (.doc) cho bạn!**\n");
                answerSb.AppendLine($"- **Học phần:** {targetMonHoc.TenMonHoc} (`{targetMonHoc.MaCodeMonHoc}`) *(Chương trình bạn đã/đang học)*");
                answerSb.AppendLine($"- **Chủ đề ôn tập:** {cleanTopic}");
                answerSb.AppendLine($"- **Số lượng câu hỏi:** {quizRes.TongSoCau} câu trắc nghiệm (Kèm đáp án & hướng dẫn giải chi tiết)");
                answerSb.AppendLine($"- **Thời gian tự luyện đề xuất:** {quizRes.ThoiGianPhut} phút");
                answerSb.AppendLine($"- **Mã đề trên hệ thống:** `#{quizRes.MaDeKiemTra}` (Đã lưu vào CSDL cá nhân)\n");

                if (isOutsideCurriculum)
                {
                    answerSb.AppendLine($"💡 *Lưu ý: Môn học bạn yêu cầu không nằm trong các môn bạn đã học hoặc đang học. AI đã tự động căn chỉnh đề sang học phần `{targetMonHoc.TenMonHoc}` để bám sát đúng chương trình học tập của bạn.*\n");
                }

                answerSb.AppendLine("📄 **File tài liệu Word đã sẵn sàng.** Bạn hãy bấm vào nút **[Tải file Word (.doc) tự ôn tập]** bên dưới để tải file về máy và tự ôn tập bất cứ lúc nào nhé!");

                return new AiChatResponse
                {
                    Answer = answerSb.ToString(),
                    ConversationId = conversationId,
                    Model = targetModel,
                    Action = new AiChatActionDto
                    {
                        ActionType = "download_quiz",
                        Title = $"Đề tự luyện {quizRes.TongSoCau} câu - {targetMonHoc.TenMonHoc}",
                        Description = $"File Word (.doc) gồm {quizRes.TongSoCau} câu trắc nghiệm tự ôn tập kèm đáp án chi tiết môn {targetMonHoc.TenMonHoc}.",
                        Status = "completed",
                        ActionUrl = isStudent ? "/student/exams" : quizRes.ActionUrl,
                        DownloadUrl = $"/api/ai/actions/download-quiz-doc?maDeKiemTra={quizRes.MaDeKiemTra}",
                        Metadata = new Dictionary<string, object>
                        {
                            ["maDeKiemTra"] = quizRes.MaDeKiemTra,
                            ["maMonHoc"] = quizRes.MaMonHoc,
                            ["tongSoCau"] = quizRes.TongSoCau
                        }
                    }
                };
            }
        }

        // 1.2. Nhận diện yêu cầu AI hỗ trợ soạn Phiếu Hỗ Trợ / Khiếu Nại (Support Ticket Draft)
        bool isCreateTicketIntent = normalizedMsg.Contains("tao ticket") || normalizedMsg.Contains("tao phieu")
            || normalizedMsg.Contains("khieu nai") || normalizedMsg.Contains("gui ticket") || normalizedMsg.Contains("gui don")
            || (normalizedMsg.Contains("bao loi") && (normalizedMsg.Contains("he thong") || normalizedMsg.Contains("hoc vu") || normalizedMsg.Contains("diem")));

        if (isCreateTicketIntent)
        {
            var studentId = userContext?.UserId ?? 0;
            if (studentId > 0)
            {
                // Tự động phân loại danh mục theo ý định người dùng
                string ticketCategoryDb = "hoc_vu";
                string ticketCategoryUi = "Học vụ";
                if (normalizedMsg.Contains("tai chinh") || normalizedMsg.Contains("hoc phi") || normalizedMsg.Contains("tien") || normalizedMsg.Contains("hoc bong"))
                {
                    ticketCategoryDb = "tai_chinh";
                    ticketCategoryUi = "Tài chính";
                }
                else if (normalizedMsg.Contains("ky thuat") || normalizedMsg.Contains("mat khau") || normalizedMsg.Contains("loi he thong") || normalizedMsg.Contains("khong vao duoc") || normalizedMsg.Contains("server") || normalizedMsg.Contains("web"))
                {
                    ticketCategoryDb = "ky_thuat";
                    ticketCategoryUi = "Kỹ thuật";
                }
                else if (normalizedMsg.Contains("khac"))
                {
                    ticketCategoryDb = "khac";
                    ticketCategoryUi = "Khác";
                }

                string cleanTitle = CleanTicketTitle(request.Message);
                string cleanDesc = CleanTicketDescription(request.Message, cleanTitle, ticketCategoryUi);

                var answerSb = new StringBuilder();
                answerSb.AppendLine("Tôi đã soạn sẵn phiếu hỗ trợ theo yêu cầu của bạn. Bạn hãy kiểm tra lại thông tin bên dưới trước khi gửi.");
                answerSb.AppendLine("\nBạn có hình ảnh minh chứng (ảnh bài thi, bảng điểm, thông báo lỗi...) hay thông tin gì cần ghi thêm không? Bạn có thể đính kèm ảnh trực tiếp vào phiếu bên dưới hoặc gửi ảnh lên khung chat, sau đó bấm **[Gửi Yêu Cầu]** để chính thức gửi tới phòng ban xử lý nhé.");

                return new AiChatResponse
                {
                    Answer = answerSb.ToString(),
                    ConversationId = conversationId,
                    Model = targetModel,
                    Action = new AiChatActionDto
                    {
                        ActionType = "draft_ticket",
                        Title = cleanTitle,
                        Description = cleanDesc,
                        Status = "pending",
                        ActionUrl = "/student/support-tickets",
                        Metadata = new Dictionary<string, object>
                        {
                            ["title"] = cleanTitle,
                            ["category"] = ticketCategoryUi,
                            ["categoryDb"] = ticketCategoryDb,
                            ["content"] = cleanDesc
                        }
                    }
                };
            }
        }

        bool isBghReportOrDeep = (userContext?.Role == AuthRoles.Principal || userContext?.Role == "hieu_truong" || request.Mode?.ToLower() == "deep" || request.Message.Contains("Cố vấn Chiến lược Học thuật"));

        if (!isBghReportOrDeep)
        {
            // 2. Tra cứu & báo cáo dữ liệu học tập cá nhân Mức 2 (Sinh viên)
            if (TryGetStudentGradeReply(request.Message, userContext, studentAcademicContext, out var gradeReply))
            {
                await Task.Delay(1800, cancellationToken);
                return CreateFastResponse(gradeReply, conversationId);
            }

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

            // Tra cứu nhanh dữ liệu giảng dạy Mức 2 (Giảng viên)
            if (TryGetTeacherScheduleReply(request.Message, userContext, teacherAcademicContext, out var teacherScheduleReply))
            {
                await Task.Delay(1800, cancellationToken);
                return CreateFastResponse(teacherScheduleReply, conversationId);
            }

            if (TryGetTeacherGradingReply(request.Message, userContext, teacherAcademicContext, out var teacherGradingReply))
            {
                await Task.Delay(1800, cancellationToken);
                return CreateFastResponse(teacherGradingReply, conversationId);
            }

            if (TryGetTeacherAtRiskReply(request.Message, userContext, teacherAcademicContext, out var teacherAtRiskReply))
            {
                await Task.Delay(1800, cancellationToken);
                return CreateFastResponse(teacherAtRiskReply, conversationId);
            }

            // 3. Phản hồi mượt mà (~1.8-2.5s) cho các câu hỏi quy chế, tính toán cơ bản & chào hỏi (Mức 1)
            if (TryGetInstantReply(request.Message, userContext, out var instantReply))
            {
                await Task.Delay(1800, cancellationToken);
                return CreateFastResponse(instantReply, conversationId);
            }
        }

        // 4. Tra cứu & xử lý nghiệp vụ học vụ / điều hành thực tế từ CSDL (Giảng viên, Đánh giá, Điểm danh, Pass/Fail, Phòng học...)
        ResolvedAcademicContext? academicContext = null;
        try
        {
            academicContext = await _academicQueryResolver.ResolveAcademicContextAsync(request.Message, userContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve academic context for query: {Query}", request.Message);
        }

        // 5. Xử lý câu hỏi học thuật qua Ollama với Concurrency Gate & Context học tập thực tế
        var systemPrompt = await BuildSystemPromptAsync(userContext, studentAcademicContext, teacherAcademicContext, request.CourseId, request.LessonId, cancellationToken);

        if (academicContext != null && academicContext.HasAcademicData)
        {
            systemPrompt += "\n\n" + academicContext.GroundingContext;
            systemPrompt += "\n\n[HƯỚNG DẪN TRẢ LỜI CHO TRỢ LÝ ĐIỀU HÀNH HỌC THUẬT]:\n" +
                            "- BẠN ĐÃ ĐƯỢC KẾT NỐI TRỰC TIẾP VỚI CƠ SỞ DỮ LIỆU VÀ CÁC THUẬT TOÁN HỆ THỐNG LMS.\n" +
                            "- Hãy trả lời trực tiếp, rõ ràng, trung thực bằng tiếng Việt dựa trên 100% số liệu thực tế đã cung cấp ở trên.\n" +
                            "- Nếu người dùng hỏi đánh giá có tích cực không, hãy nêu rõ tỷ lệ % tích cực, điểm trung bình sao và trích dẫn khách quan nhận xét của sinh viên.\n" +
                            "- Nếu người dùng hỏi về điểm danh/giảng dạy, hãy nêu rõ số buổi đã dạy, số buổi đúng hạn, số buổi trễ hạn hoặc chưa điểm danh và tỷ lệ hoàn thành (%).\n" +
                            "- Tuyệt đối không được nói 'tôi không có dữ liệu' hoặc 'chưa được cấp báo cáo điều hành', vì toàn bộ dữ liệu thực tế đã được cung cấp ở trên.\n" +
                            "- Không để lộ thông tin cá nhân nhạy cảm (SĐT riêng, CCCD, lương).";
        }

        // Nạp kiến thức Quy chế RAG từ văn bản chính thức của trường
        var ragContext = GetRelevantRagContext(request.Message, request.UseRag);
        if (!string.IsNullOrWhiteSpace(ragContext))
        {
            systemPrompt += "\n\n" + ragContext;
        }

        var numPredict = _options.MaxOutputTokens > 0 ? _options.MaxOutputTokens : 2048;
        var numCtx = _options.ContextLength > 0 ? _options.ContextLength : 4096;

        var chatMessages = new List<OllamaChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        if (request.History != null && request.History.Count > 0)
        {
            foreach (var h in request.History)
            {
                if (!string.IsNullOrWhiteSpace(h.Content))
                {
                    chatMessages.Add(new() { Role = h.Role, Content = h.Content });
                }
            }
        }

        chatMessages.Add(new() { Role = "user", Content = request.Message.Trim() });

        var payload = new OllamaChatPayload
        {
            Model = targetModel,
            Stream = false,
            Messages = chatMessages,
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

                    if (academicContext != null && !string.IsNullOrWhiteSpace(academicContext.DirectAnswer))
                    {
                        return new AiChatResponse
                        {
                            Answer = academicContext.DirectAnswer,
                            Thinking = null,
                            ProcessingTimeMs = (int)sw.ElapsedMilliseconds,
                            ConversationId = conversationId,
                            Model = targetModel,
                            Action = academicContext.SuggestedAction,
                            Sources = new List<string>()
                        };
                    }

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
                if (string.IsNullOrWhiteSpace(answer) || (academicContext != null && academicContext.HasAcademicData && answer.Contains("Xin lỗi, hiện tại tôi chưa thể")))
                {
                    if (academicContext != null && !string.IsNullOrWhiteSpace(academicContext.DirectAnswer))
                    {
                        answer = academicContext.DirectAnswer;
                    }
                    else if (!string.IsNullOrWhiteSpace(thinking))
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
                    Model = targetModel,
                    Action = academicContext?.SuggestedAction,
                    Sources = new List<string>()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama at {BaseUrl}", _options.BaseUrl);
                if (academicContext != null && !string.IsNullOrWhiteSpace(academicContext.DirectAnswer))
                {
                    return new AiChatResponse
                    {
                        Answer = academicContext.DirectAnswer,
                        Thinking = null,
                        ProcessingTimeMs = (int)sw.ElapsedMilliseconds,
                        ConversationId = conversationId,
                        Model = targetModel,
                        Action = academicContext.SuggestedAction,
                        Sources = new List<string>()
                    };
                }
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

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

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
            if (!httpResponse.IsSuccessStatusCode) return Array.Empty<float>();

            var resString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var embedResult = JsonSerializer.Deserialize<OllamaEmbedResult>(resString, JsonOptions);
            return embedResult?.Embeddings?.FirstOrDefault() ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get embedding from Ollama");
            return Array.Empty<float>();
        }
    }

    public async Task<AiGenerateQuizResponse> GenerateQuizAsync(
        AiGenerateQuizRequest request,
        CurrentUserContext? userContext,
        CancellationToken cancellationToken = default)
    {
        if (userContext == null || userContext.UserId <= 0)
        {
            throw new ApiException(401, "Người dùng chưa được xác thực.");
        }

        var monHoc = await _db.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.MaMonHoc == request.MaMonHoc, cancellationToken);

        var tenMonHoc = monHoc?.TenMonHoc ?? "Môn học";
        var tieuDe = string.IsNullOrWhiteSpace(request.TieuDe)
            ? $"Kiểm tra trắc nghiệm - {tenMonHoc}"
            : request.TieuDe;

        var questions = await GenerateQuestionsWithAiAsync(tenMonHoc, request.ChuDe, request.SoLuongCauHoi, request.DoKho, cancellationToken);

        var deKiemTra = new DeKiemTra
        {
            MaMonHoc = request.MaMonHoc,
            TieuDe = tieuDe,
            ThoiGianPhut = request.ThoiGianPhut,
            TrangThai = "nhap",
            LoaiDeThi = "trac_nghiem",
            HinhThucThi = "online_tu_do",
            MaNguoiSoan = userContext.UserId,
            NgayTao = DateTime.UtcNow,
            CauHinhDeThi = JsonSerializer.Serialize(new { autoGeneratedBy = "AET_AI_Agent", chuDe = request.ChuDe, doKho = request.DoKho })
        };

        _db.DeKiemTras.Add(deKiemTra);
        await _db.SaveChangesAsync(cancellationToken);

        var createdQuestionsDto = new List<AiGeneratedQuestionDto>();
        var pointPerQuestion = Math.Round(10.0m / Math.Max(1, questions.Count), 2);

        int order = 1;
        foreach (var q in questions)
        {
            var choicesJson = JsonSerializer.Serialize(q.LuaChon.Select(c => new { id = c.Id, text = c.Text }));
            var cauHoi = new CauHoi
            {
                MaMonHoc = request.MaMonHoc,
                NguoiTao = userContext.UserId,
                LoaiCauHoi = "trac_nghiem",
                KieuLuaChon = "chon_mot",
                NoiDung = q.NoiDung,
                LuaChon = choicesJson,
                DapAnDung = JsonSerializer.Serialize(new[] { q.DapAnDung }),
                GiaiThichDapAn = q.GiaiThich,
                DoKho = q.DoKho,
                ConHoatDong = true,
                NgayTao = DateTime.UtcNow
            };

            _db.CauHois.Add(cauHoi);
            await _db.SaveChangesAsync(cancellationToken);

            _db.CauHoiDeKiemTras.Add(new CauHoiDeKiemTra
            {
                MaDeKiemTra = deKiemTra.MaDeKiemTra,
                MaCauHoi = cauHoi.MaCauHoi,
                DiemSo = pointPerQuestion,
                ThuTu = order++
            });

            q.MaCauHoi = cauHoi.MaCauHoi;
            q.DiemSo = pointPerQuestion;
            createdQuestionsDto.Add(q);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new AiGenerateQuizResponse
        {
            Success = true,
            MaDeKiemTra = deKiemTra.MaDeKiemTra,
            TieuDe = deKiemTra.TieuDe,
            MaMonHoc = request.MaMonHoc,
            TenMonHoc = tenMonHoc,
            TongSoCau = createdQuestionsDto.Count,
            ThoiGianPhut = deKiemTra.ThoiGianPhut,
            ActionUrl = $"/content-council/quizzes",
            DanhSachCauHoi = createdQuestionsDto,
            Message = $"Đã tạo thành công đề kiểm tra '{tieuDe}' gồm {createdQuestionsDto.Count} câu hỏi trắc nghiệm và lưu vào CSDL môn {tenMonHoc}."
        };
    }

    public async Task<byte[]?> ExportQuizDocAsync(int maDeKiemTra, CancellationToken cancellationToken = default)
    {
        var deThi = await _db.DeKiemTras
            .AsNoTracking()
            .Include(d => d.MonHoc)
            .FirstOrDefaultAsync(d => d.MaDeKiemTra == maDeKiemTra, cancellationToken);

        if (deThi == null) return null;

        var questions = await _db.CauHoiDeKiemTras
            .AsNoTracking()
            .Include(cd => cd.CauHoi)
            .Where(cd => cd.MaDeKiemTra == maDeKiemTra)
            .OrderBy(cd => cd.ThuTu)
            .ToListAsync(cancellationToken);

        var monHocTen = deThi.MonHoc?.TenMonHoc ?? "MÔN HỌC";
        var monHocMa = deThi.MonHoc?.MaCodeMonHoc ?? "GEN";

        var sb = new StringBuilder();
        sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word' xmlns='http://www.w3.org/TR/REC-html40'>");
        sb.AppendLine("<head><meta charset='utf-8'><title>" + System.Net.WebUtility.HtmlEncode(deThi.TieuDe) + "</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Times New Roman', serif; font-size: 13pt; line-height: 1.4; margin: 2cm; color: #111827; }");
        sb.AppendLine(".tbl-head { width: 100%; border: none; margin-bottom: 20px; }");
        sb.AppendLine(".tbl-head td { border: none; vertical-align: top; }");
        sb.AppendLine(".school-title { font-size: 11.5pt; font-weight: bold; text-align: center; text-transform: uppercase; }");
        sb.AppendLine(".doc-title { text-align: center; font-size: 16pt; font-weight: bold; text-transform: uppercase; color: #1e40af; margin-top: 15px; margin-bottom: 4px; }");
        sb.AppendLine(".doc-sub { text-align: center; font-size: 11pt; font-style: italic; color: #4b5563; margin-bottom: 25px; }");
        sb.AppendLine(".q-num { font-weight: bold; margin-top: 14px; margin-bottom: 5px; }");
        sb.AppendLine(".opt-row { margin-left: 20px; margin-bottom: 3px; }");
        sb.AppendLine(".ans-sec { margin-top: 40px; page-break-before: always; border-top: 2px solid #2563eb; padding-top: 15px; }");
        sb.AppendLine(".ans-tbl { width: 100%; border-collapse: collapse; margin-top: 12px; }");
        sb.AppendLine(".ans-tbl th, .ans-tbl td { border: 1px solid #9ca3af; padding: 6px 10px; font-size: 11pt; text-align: left; }");
        sb.AppendLine(".ans-tbl th { background-color: #f3f4f6; text-align: center; font-weight: bold; }");
        sb.AppendLine("</style></head><body>");

        sb.AppendLine("<table class='tbl-head'>");
        sb.AppendLine("<tr>");
        sb.AppendLine("<td style='width:50%; text-align:center;'>");
        sb.AppendLine("<div class='school-title'>BỘ GIÁO DỤC VÀ ĐÀO TẠO<br>TRƯỜNG ĐẠI HỌC AET</div>");
        sb.AppendLine("<div style='font-size:10pt; margin-top:4px;'>PHÒNG ĐÀO TẠO & KHẢO THÍ</div>");
        sb.AppendLine("</td>");
        sb.AppendLine("<td style='width:50%; text-align:center;'>");
        sb.AppendLine("<div class='school-title'>HỆ THỐNG TRỢ LÝ AI HỌC THUẬT<br>TÀI LIỆU TỰ LUYỆN ÔN TẬP</div>");
        sb.AppendLine($"<div style='font-size:10pt; margin-top:4px;'>Ngày xuất file: {DateTime.Now:dd/MM/yyyy}</div>");
        sb.AppendLine("</td>");
        sb.AppendLine("</tr>");
        sb.AppendLine("</table>");

        sb.AppendLine($"<div class='doc-title'>{System.Net.WebUtility.HtmlEncode(deThi.TieuDe)}</div>");
        sb.AppendLine($"<div class='doc-sub'>Học phần: {System.Net.WebUtility.HtmlEncode(monHocTen)} (Mã HP: {System.Net.WebUtility.HtmlEncode(monHocMa)}) | Thời gian làm bài: {deThi.ThoiGianPhut} phút | Số lượng: {questions.Count} câu hỏi</div>");
        sb.AppendLine("<hr style='border: 0; border-top: 1px solid #cbd5e1; margin-bottom: 20px;' />");

        int index = 1;
        var answerKeys = new List<(int Index, string DapAn, string GiaiThich)>();

        foreach (var cd in questions)
        {
            var q = cd.CauHoi;
            if (q == null) continue;
            sb.AppendLine($"<div class='q-num'>Câu {index}: {System.Net.WebUtility.HtmlEncode(q.NoiDung)}</div>");
            if (!string.IsNullOrWhiteSpace(q.LuaChon) && q.LuaChon.TrimStart().StartsWith("["))
            {
                try
                {
                    var choices = JsonSerializer.Deserialize<List<JsonElement>>(q.LuaChon);
                    if (choices != null)
                    {
                        foreach (var opt in choices)
                        {
                            var key = opt.TryGetProperty("id", out var idVal) ? idVal.GetString()
                                : (opt.TryGetProperty("key", out var kVal) ? kVal.GetString() : "");
                            var text = opt.TryGetProperty("text", out var t) ? t.GetString() : "";
                            sb.AppendLine($"<div class='opt-row'><b>{key}.</b> {System.Net.WebUtility.HtmlEncode(text)}</div>");
                        }
                    }
                }
                catch { }
            }

            var displayAns = q.DapAnDung ?? "A";
            if (!string.IsNullOrWhiteSpace(displayAns) && (displayAns.StartsWith("[") || displayAns.StartsWith("\"")))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<string>>(displayAns);
                    if (arr != null && arr.Count > 0) displayAns = string.Join(", ", arr);
                    else displayAns = displayAns.Trim('"', '[', ']', ' ');
                }
                catch
                {
                    displayAns = displayAns.Trim('"', '[', ']', ' ');
                }
            }

            answerKeys.Add((index, displayAns, q.GiaiThichDapAn ?? "Theo chuẩn giáo trình và đề cương học phần."));
            index++;
        }

        // Trang Đáp án & Lời giải chi tiết
        sb.AppendLine("<div class='ans-sec'>");
        sb.AppendLine("<div style='text-align:center; font-size:14pt; font-weight:bold; color:#1e40af; margin-bottom:6px;'>BẢNG ĐÁP ÁN & HƯỚNG DẪN GIẢI CHI TIẾT</div>");
        sb.AppendLine("<div style='text-align:center; font-size:11pt; font-style:italic; margin-bottom:15px;'>(Dành cho sinh viên tự đối soát sau khi hoàn thành bài tự luyện)</div>");
        sb.AppendLine("<table class='ans-tbl'>");
        sb.AppendLine("<tr><th style='width:12%;'>Câu</th><th style='width:18%;'>Đáp án đúng</th><th>Hướng dẫn giải & Cơ sở lý thuyết</th></tr>");

        foreach (var ak in answerKeys)
        {
            sb.AppendLine($"<tr><td style='text-align:center; font-weight:bold;'>Câu {ak.Index}</td><td style='text-align:center; font-weight:bold; color:#15803d;'>{ak.DapAn}</td><td>{System.Net.WebUtility.HtmlEncode(ak.GiaiThich)}</td></tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine("</div>");

        sb.AppendLine("</body></html>");
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CleanTopic(string rawPrompt, string fallbackSubject)
    {
        if (string.IsNullOrWhiteSpace(rawPrompt)) return fallbackSubject;
        var s = rawPrompt.Trim();

        // Xóa các cụm từ lệnh phía trước: "tạo giúp tôi 10 câu trắc nghiệm...", "hãy tạo...", "soạn đề..."
        s = System.Text.RegularExpressions.Regex.Replace(s, @"^(?:hãy\s+)?(?:tạo|sinh|làm|soạn|viết)(?:\s+giúp|\s+cho)?(?:\s+tôi|\s+em|\s+mình)?(?:\s+bộ|\s+đề)?(?:\s+\d+)?(?:\s+câu)?(?:\s+hỏi)?(?:\s+trắc\s+nghiệm)?(?:\s+về|\s+phần|\s+chủ\s+đề|\s+môn|\s+học\s+phần)?", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
        // Xóa các cụm từ phía sau: "để tôi tự kiểm tra", "để ôn tập", "nhé", "nha"
        s = System.Text.RegularExpressions.Regex.Replace(s, @"(?:\s+để\s+(?:tôi|em|mình)?\s*(?:tự\s+)?(?:kiểm\s+tra|ôn\s+tập|luyện\s+tập).*)$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
        s = System.Text.RegularExpressions.Regex.Replace(s, @"(?:\s*(?:nhé|nha|với|ạ|nhanh|ngay)\s*)$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

        if (string.IsNullOrWhiteSpace(s) || s.Length < 3) return fallbackSubject;
        return s;
    }

    private static string CleanTicketTitle(string rawPrompt)
    {
        if (string.IsNullOrWhiteSpace(rawPrompt)) return "Tôi cần hỗ trợ vấn đề học vụ";
        var s = rawPrompt.Trim();

        // Xóa "tạo ticket hỗ trợ tôi", "tạo ticket", "giúp tôi gửi ticket"...
        s = System.Text.RegularExpressions.Regex.Replace(s, @"^(?:hãy\s+)?(?:tạo|gửi|lập)(?:\s+ticket|\s+phiếu|\s+yêu\s+cầu|\s+đơn)?(?:\s+hỗ\s+trợ)?(?:\s+cho\s+tôi|\s+tôi|\s+em|\s+mình)?(?:\s+về\s+việc|\s+về)?", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
        s = System.Text.RegularExpressions.Regex.Replace(s, @"^(?:tôi\s+muốn\s+tạo\s+ticket\s+(?:hỗ\s+trợ\s+)?(?:tôi\s+)?)", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();

        if (string.IsNullOrWhiteSpace(s) || s.Length < 4) return "Tôi cần hỗ trợ xử lý vấn đề học vụ";

        if (s.StartsWith("khiếu nại", StringComparison.OrdinalIgnoreCase))
        {
            s = char.ToUpper(s[0]) + s.Substring(1);
        }
        else if (!s.StartsWith("Tôi", StringComparison.OrdinalIgnoreCase))
        {
            s = "Tôi muốn " + char.ToLower(s[0]) + s.Substring(1);
        }
        else
        {
            s = char.ToUpper(s[0]) + s.Substring(1);
        }

        return s.Length > 100 ? s.Substring(0, 97) + "..." : s;
    }

    private static string CleanTicketDescription(string rawPrompt, string cleanTitle, string categoryUi)
    {
        var s = rawPrompt.Trim();
        return $"Chi tiết yêu cầu: {cleanTitle}.\nNội dung phản ánh từ người dùng: {s}\nKính mong bộ phận chuyên trách ({categoryUi}) kiểm tra và hỗ trợ xử lý.";
    }

    private Task<List<AiGeneratedQuestionDto>> GenerateQuestionsFromLegacyBankAsync(
        string tenMonHoc,
        string? chuDe,
        int count,
        string doKho,
        CancellationToken cancellationToken)
    {
        var topic = string.IsNullOrWhiteSpace(chuDe) ? tenMonHoc : chuDe;
        var normalizedTopic = NormalizeVietnamese(topic);
        var normalizedSubject = NormalizeVietnamese(tenMonHoc);

        // 1. NGÂN HÀNG: CƠ SỞ DỮ LIỆU (COM102)
        if (normalizedTopic.Contains("khoa ngoai") || normalizedTopic.Contains("3nf") || normalizedTopic.Contains("chuan hoa") || normalizedTopic.Contains("csdl") || normalizedSubject.Contains("co so du lieu") || normalizedSubject.Contains("csdl"))
        {
            var pool = new List<AiGeneratedQuestionDto>
            {
                new()
                {
                    NoiDung = "Khóa ngoại (Foreign Key) trong hệ quản trị cơ sở dữ liệu quan hệ đóng vai trò chủ yếu nào sau đây?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Đảm bảo tính toàn vẹn tham chiếu (Referential Integrity) giữa các bảng quan hệ." },
                        new() { Id = "B", Text = "Tự động phân vùng bảng để tăng tốc độ ghi đĩa vật lý." },
                        new() { Id = "C", Text = "Bắt buộc mọi cột trong bảng phải có giá trị duy nhất (Unique)." },
                        new() { Id = "D", Text = "Ngăn chặn hoàn toàn việc xóa dữ liệu trong cơ sở dữ liệu." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Khóa ngoại liên kết cột tham chiếu đến khóa chính của bảng khác để bảo đảm tính toàn vẹn tham chiếu và ngăn chặn dữ liệu mồ côi.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Một lược đồ quan hệ đạt Dạng chuẩn 3 (3NF) khi và chỉ khi thỏa mãn điều kiện nào?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Đã đạt Dạng chuẩn 2 (2NF) và không tồn tại bất kỳ phụ thuộc hàm bắc cầu nào vào khóa chính." },
                        new() { Id = "B", Text = "Mọi thuộc tính đều là đơn nguyên tử và bảng có tối thiểu hai khóa chính." },
                        new() { Id = "C", Text = "Chỉ cần đạt 1NF và không chứa các khóa ngoại có giá trị NULL." },
                        new() { Id = "D", Text = "Tất cả các phụ thuộc hàm đều là phụ thuộc đa trị không tầm thường." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Theo lý thuyết Codd: Quan hệ đạt 3NF nếu đã đạt 2NF và mọi thuộc tính không khóa đều phụ thuộc trực tiếp vào khóa chính, không có phụ thuộc bắc cầu.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong ngôn ngữ SQL, tùy chọn 'ON DELETE CASCADE' khai báo trên ràng buộc Khóa ngoại có ý nghĩa gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Khi một bản ghi ở bảng cha bị xóa, toàn bộ các bản ghi con tương ứng ở bảng tham chiếu sẽ tự động bị xóa theo." },
                        new() { Id = "B", Text = "Hệ thống từ chối thao tác xóa ở bảng cha nếu vẫn còn dữ liệu liên kết ở bảng con." },
                        new() { Id = "C", Text = "Tự động gán giá trị NULL cho cột khóa ngoại ở bảng con khi bản ghi cha bị xóa." },
                        new() { Id = "D", Text = "Tự động sao lưu dữ liệu sang bảng lịch sử trước khi thực hiện xóa." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "ON DELETE CASCADE cho phép tự động lan truyền thao tác xóa từ bảng cha xuống các bản ghi con phụ thuộc để duy trì tính nhất quán.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Hiện tượng bất thường dữ liệu (Data Anomaly) nào sau đây sẽ xảy ra nếu một bảng chưa được chuẩn hóa về dạng 3NF?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Bất thường cập nhật (Update Anomaly) và Bất thường xóa (Delete Anomaly) do dư thừa dữ liệu phụ thuộc bắc cầu." },
                        new() { Id = "B", Text = "Tràn bộ nhớ RAM của máy chủ CSDL khi thực hiện câu lệnh SELECT." },
                        new() { Id = "C", Text = "Lỗi xung đột mã hóa ký tự UTF-8 giữa các phiên bản hệ quản trị CSDL." },
                        new() { Id = "D", Text = "Khóa chính tự động bị vô hiệu hóa khi bảng vượt quá 1.000 dòng dữ liệu." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Khi tồn tại phụ thuộc bắc cầu (X -> Y -> Z), việc cập nhật hoặc xóa thông tin về Y/Z sẽ dẫn đến bất thường dữ liệu và không nhất quán.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Cho lược đồ R(A, B, C, D) với khóa chính là A và tập phụ thuộc hàm F = {A -> B, A -> C, C -> D}. Để đưa R về 3NF, ta cần tách thành các lược đồ con nào?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "R1(A, B, C) với khóa chính A và R2(C, D) với khóa chính C." },
                        new() { Id = "B", Text = "R1(A, B) với khóa chính A và R2(A, C, D) với khóa chính A." },
                        new() { Id = "C", Text = "R1(A, D) với khóa chính A và R2(B, C) với khóa chính B." },
                        new() { Id = "D", Text = "Không cần tách vì lược đồ R ban đầu đã đạt dạng chuẩn 3 (3NF)." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Do C -> D là phụ thuộc bắc cầu qua C, ta tách R thành R1(A, B, C) và R2(C, D) để triệt tiêu phụ thuộc bắc cầu mà vẫn bảo toàn phụ thuộc hàm và nối không mất mát thông tin.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Một bảng quan hệ đạt Dạng chuẩn 1 (1NF) khi thỏa mãn điều kiện tiên quyết nào sau đây?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Tất cả các thuộc tính đều chứa giá trị nguyên tử (Atomic values), không chứa thuộc tính đa trị hoặc phức hợp." },
                        new() { Id = "B", Text = "Bảng phải có ít nhất hai khóa ngoại liên kết với bảng khác." },
                        new() { Id = "C", Text = "Bảng không được chứa bất kỳ giá trị NULL nào trong tất cả các cột." },
                        new() { Id = "D", Text = "Mọi cột đều phải là kiểu dữ liệu số nguyên (INT)." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "1NF yêu cầu mọi miền giá trị của thuộc tính phải là nguyên tử và không có nhóm lặp thuộc tính.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Một bảng quan hệ đạt Dạng chuẩn 2 (2NF) khi thỏa mãn điều kiện nào?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Đã đạt 1NF và mọi thuộc tính không khóa đều phụ thuộc hàm đầy đủ vào khóa chính (không phụ thuộc một phần)." },
                        new() { Id = "B", Text = "Đã đạt 3NF và loại bỏ hoàn toàn các phụ thuộc đa trị." },
                        new() { Id = "C", Text = "Chỉ cần bảng có khóa chính đơn (1 thuộc tính) là tự động đạt 2NF." },
                        new() { Id = "D", Text = "Bảng phải được lập chỉ mục Clustered Index trên cột khóa chính." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "2NF loại bỏ phụ thuộc hàm từng phần vào khóa phức hợp. Nếu khóa chính là khóa đơn thì quan hệ 1NF tự động đạt 2NF.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khác biệt bản chất giữa Dạng chuẩn 3 (3NF) và Dạng chuẩn Boyce-Codd (BCNF) là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "BCNF yêu cầu mọi vế trái X của mọi phụ thuộc hàm X -> Y không tầm thường đều phải là siêu khóa (Super Key)." },
                        new() { Id = "B", Text = "3NF yêu cầu vế trái phải là siêu khóa, còn BCNF cho phép vế phải là thuộc tính khóa." },
                        new() { Id = "C", Text = "3NF áp dụng cho khóa phức hợp, còn BCNF chỉ áp dụng cho khóa đơn." },
                        new() { Id = "D", Text = "BCNF cho phép bảo toàn phụ thuộc hàm 100% trong mọi trường hợp tách bảng." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "BCNF là dạng chuẩn chặt chẽ hơn 3NF, loại bỏ cả trường hợp vế phải là thuộc tính nguyên tố khi vế trái không là siêu khóa.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khi khai báo ràng buộc Khóa ngoại với tùy chọn 'ON DELETE NO ACTION' hoặc 'ON DELETE RESTRICT', hệ thống sẽ xử lý thế nào khi xóa bản ghi cha?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Báo lỗi và từ chối xóa nếu còn bất kỳ bản ghi con nào đang tham chiếu tới bản ghi cha đó." },
                        new() { Id = "B", Text = "Tự động xóa luôn các bản ghi con liên quan." },
                        new() { Id = "C", Text = "Gán NULL cho khóa ngoại của các bản ghi con." },
                        new() { Id = "D", Text = "Di chuyển bản ghi cha vào bảng sao lưu tạm thời." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "RESTRICT và NO ACTION ngăn chặn hành động xóa ở bảng cha khi vẫn còn khóa ngoại tham chiếu ở bảng con để bảo vệ toàn vẹn dữ liệu.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Mục đích chính của việc tạo Chỉ mục (Index) trên cột Khóa ngoại trong SQL Server là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Tăng tốc độ đáng kể cho các phép JOIN giữa bảng cha và bảng con, cũng như kiểm tra ràng buộc toàn vẹn." },
                        new() { Id = "B", Text = "Tự động mã hóa giá trị khóa ngoại để ngăn chặn rò rỉ dữ liệu." },
                        new() { Id = "C", Text = "Bắt buộc cột khóa ngoại không được nhận giá trị NULL." },
                        new() { Id = "D", Text = "Giảm dung lượng lưu trữ vật lý của bảng trên ổ đĩa." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "SQL Server không tự động tạo Index trên Foreign Key. Đánh Index trên FK giúp tối ưu vượt bậc hiệu năng JOIN và kiểm tra toàn vẹn.",
                    DoKho = doKho
                }
            };

            return Task.FromResult(SelectFromPool(pool, count, "Cơ sở dữ liệu", doKho));
        }

        // 2. NGÂN HÀNG: NHẬP MÔN LẬP TRÌNH (COM101 / C / C++ / Python / Logic lập trình)
        if (normalizedTopic.Contains("nhap mon lap trinh") || normalizedTopic.Contains("lap trinh") || normalizedSubject.Contains("nhap mon lap trinh") || normalizedSubject.Contains("lap trinh") || normalizedTopic.Contains("bien") || normalizedTopic.Contains("vong lap") || normalizedTopic.Contains("ham"))
        {
            var pool = new List<AiGeneratedQuestionDto>
            {
                new()
                {
                    NoiDung = "Tên định danh (Identifier) nào sau đây là hợp lệ theo quy tắc cú pháp trong hầu hết các ngôn ngữ lập trình như C, C++, C#, Java?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "_diem_trung_binh_2026" },
                        new() { Id = "B", Text = "2026_diem_tong_ket" },
                        new() { Id = "C", Text = "diem-trung-binh" },
                        new() { Id = "D", Text = "class" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Tên định danh chỉ được bắt đầu bằng chữ cái hoặc dấu gạch dưới '_', không được bắt đầu bằng chữ số, không chứa ký tự phép toán và không trùng từ khóa bảo lưu.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Kiểu dữ liệu nguyên thủy nào sau đây thường được sử dụng để lưu trữ giá trị đúng hoặc sai trong các ngôn ngữ lập trình hiện đại?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "bool (hoặc boolean)" },
                        new() { Id = "B", Text = "int" },
                        new() { Id = "C", Text = "float" },
                        new() { Id = "D", Text = "char" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Kiểu bool biểu diễn giá trị chân lý logic (true hoặc false) và thường chiếm 1 byte trong bộ nhớ.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Kết quả của biểu thức toán học '17 % 5' trong hầu hết các ngôn ngữ lập trình phổ biến là bao nhiêu?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "2" },
                        new() { Id = "B", Text = "3" },
                        new() { Id = "C", Text = "3.4" },
                        new() { Id = "D", Text = "0" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Toán tử '%' là phép chia lấy phần dư nguyên: 17 chia 5 được 3, dư 2.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong cấu trúc rẽ nhánh 'switch...case', từ khóa nào được dùng để kết thúc một nhánh kiểm tra và ngăn chặn hiện tượng trôi nhánh (Fall-through)?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "break" },
                        new() { Id = "B", Text = "continue" },
                        new() { Id = "C", Text = "exit" },
                        new() { Id = "D", Text = "default" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Lệnh 'break' lập tức kết thúc và thoát khỏi khối lệnh switch, tránh việc thực thi tiếp các case bên dưới khi đã khớp điều kiện.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Đặc điểm phân biệt căn bản nhất giữa vòng lặp 'while' và vòng lặp 'do...while' là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Vòng lặp 'do...while' luôn thực thi khối lệnh thân vòng lặp ít nhất một lần trước khi kiểm tra điều kiện." },
                        new() { Id = "B", Text = "Vòng lặp 'while' kiểm tra điều kiện sau khi thân vòng lặp thực hiện xong." },
                        new() { Id = "C", Text = "Vòng lặp 'do...while' chỉ sử dụng được với các biến đếm kiểu số nguyên." },
                        new() { Id = "D", Text = "Vòng lặp 'while' không bao giờ rơi vào trạng thái lặp vô tận." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "'do...while' là vòng lặp hậu kiểm (kiểm tra điều kiện ở cuối), nên thân vòng lặp luôn được thực thi tối thiểu một lần ngay cả khi điều kiện sai ngay từ đầu.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Một vòng lặp 'for (int i = 0; i < 10; i += 2)' sẽ thực thi khối lệnh trong thân vòng lặp tổng cộng bao nhiêu lần?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "5 lần" },
                        new() { Id = "B", Text = "10 lần" },
                        new() { Id = "C", Text = "4 lần" },
                        new() { Id = "D", Text = "6 lần" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Các giá trị của biến i lần lượt là: 0, 2, 4, 6, 8 (tổng cộng 5 lần lặp). Đến khi i = 10 thì điều kiện i < 10 không còn thỏa mãn và vòng lặp kết thúc.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khái niệm Hàm (Function) trong lập trình mang lại lợi ích chủ yếu nào sau đây?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Tái sử dụng mã nguồn, chia nhỏ chương trình thành các mô-đun độc lập và dễ bảo trì, gỡ lỗi." },
                        new() { Id = "B", Text = "Tự động chuyển đổi mã nguồn thành tập lệnh máy mà không cần trình biên dịch." },
                        new() { Id = "C", Text = "Loại bỏ hoàn toàn sự chiếm dụng bộ nhớ RAM của chương trình khi chạy." },
                        new() { Id = "D", Text = "Bắt buộc tất cả các biến trong chương trình phải trở thành biến toàn cục." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Hàm giúp mô-đun hóa chương trình, tránh lặp lại mã nguồn (nguyên tắc DRY) và giúp mã nguồn có cấu trúc rõ ràng, dễ bảo trì.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khi truyền tham số vào hàm theo cơ chế 'Truyền theo giá trị' (Pass by Value), phát biểu nào sau đây là đúng?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Hàm nhận một bản sao của đối số; mọi thay đổi đối với tham số bên trong hàm không làm ảnh hưởng đến biến gốc ngoài hàm." },
                        new() { Id = "B", Text = "Hàm nhận trực tiếp địa chỉ ô nhớ của biến gốc nên thay đổi trong hàm sẽ đổi luôn biến gốc." },
                        new() { Id = "C", Text = "Tham số truyền vào bắt buộc phải là biến toàn cục (Global variable)." },
                        new() { Id = "D", Text = "Không thể truyền kiểu dữ liệu nguyên thủy theo cơ chế truyền giá trị." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Truyền theo giá trị chỉ truyền bản sao dữ liệu sang stack frame của hàm, giữ nguyên giá trị của biến gốc bên ngoài phạm vi hàm.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Cho khai báo mảng một chiều: 'int a[5];'. Chỉ số (Index) của phần tử đầu tiên và phần tử cuối cùng hợp lệ lần lượt là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "0 và 4" },
                        new() { Id = "B", Text = "1 và 5" },
                        new() { Id = "C", Text = "0 và 5" },
                        new() { Id = "D", Text = "1 và 4" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Các ngôn ngữ hiện đại (C/C++, C#, Java) đều đánh chỉ số mảng bắt đầu từ 0 (Zero-based indexing), do đó mảng 5 phần tử có chỉ số từ 0 đến 4.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khái niệm 'Phạm vi của biến' (Variable Scope) định nghĩa điều gì trong chương trình?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Vùng không gian mã nguồn mà tại đó biến có thể được nhìn thấy, truy cập và sử dụng hợp lệ." },
                        new() { Id = "B", Text = "Dung lượng bộ nhớ tối đa tính bằng byte mà biến đó có thể lưu trữ." },
                        new() { Id = "C", Text = "Tốc độ xung nhịp CPU cần thiết để thực hiện tính toán trên biến." },
                        new() { Id = "D", Text = "Thời gian biến được lưu trữ vĩnh viễn trên ổ cứng máy tính." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Phạm vi biến (Scope) quy định ranh giới mã nguồn nơi biến được khai báo và có thể truy xuất hợp lệ (biến cục bộ, biến toàn cục, biến khối lệnh).",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong lập trình, hiện tượng lỗi 'Tràn ngăn xếp' (Stack Overflow) thường xuất hiện phổ biến nhất trong trường hợp nào?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Lời gọi hàm đệ quy lặp vô tận do thiếu điều kiện dừng (Base Case) hoặc điều kiện dừng không bao giờ đạt được." },
                        new() { Id = "B", Text = "Mở quá nhiều file văn bản cùng một lúc trong ổ đĩa cứng." },
                        new() { Id = "C", Text = "Khai báo một mảng có 100 phần tử kiểu số nguyên int." },
                        new() { Id = "D", Text = "Viết câu lệnh if...else lồng nhau quá 3 tầng." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Mỗi lời gọi hàm đệ quy cấp phát một stack frame trên Stack bộ nhớ. Khi đệ quy vô hạn không có điểm dừng, vùng nhớ Stack bị cạn kiệt gây tràn ngăn xếp.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Thuật toán Tìm kiếm nhị phân (Binary Search) có điều kiện tiên quyết nào đối với mảng đầu vào để hoạt động chính xác?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Mảng đầu vào phải được sắp xếp theo thứ tự (tăng dần hoặc giảm dần)." },
                        new() { Id = "B", Text = "Số lượng phần tử của mảng phải là một số chẵn." },
                        new() { Id = "C", Text = "Tất cả các phần tử trong mảng phải có giá trị dương lớn hơn 0." },
                        new() { Id = "D", Text = "Mảng phải chứa đầy đủ các ký tự chữ cái Alphabet." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Tìm kiếm nhị phân dựa trên nguyên lý chia để trị tại điểm giữa (mid), yêu cầu mảng phải được sắp xếp để xác định nửa mảng tiếp theo cần tìm kiếm.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Độ phức tạp thời gian (Time Complexity) của thao tác truy xuất một phần tử trong mảng thông qua chỉ số 'a[i]' là bao nhiêu?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "O(1) - Thời gian hằng số" },
                        new() { Id = "B", Text = "O(n) - Tuyến tính theo kích thước mảng" },
                        new() { Id = "C", Text = "O(log n) - Logarit" },
                        new() { Id = "D", Text = "O(n^2) - Bậc hai" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Vị trí bộ nhớ của phần tử a[i] được tính trực tiếp bằng công thức: Địa_chỉ_gốc + i * sizeof(kiểu_dữ_liệu), do đó thời gian truy xuất là O(1).",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Phát biểu nào sau đây mô tả đúng nhất về khái niệm 'Con trỏ' (Pointer) trong các ngôn ngữ lập trình như C/C++?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Một biến đặc biệt dùng để lưu trữ địa chỉ ô nhớ của một biến khác trong bộ nhớ RAM." },
                        new() { Id = "B", Text = "Một biến chỉ có thể lưu trữ chuỗi ký tự văn bản có độ dài cố định." },
                        new() { Id = "C", Text = "Một hàm tự động tối ưu hóa tốc độ chạy của vòng lặp for." },
                        new() { Id = "D", Text = "Một công cụ của trình biên dịch dùng để xóa các dòng chú thích comment." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Con trỏ là biến chứa giá trị là địa chỉ ô nhớ RAM của một biến hoặc đối tượng khác, cho phép thao tác trực tiếp với bộ nhớ vật lý.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khi thực hiện phép gán 'float x = 5 / 2;' trong ngôn ngữ C hoặc C#, giá trị của 'x' sẽ là bao nhiêu?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "2.0 (do 5 và 2 là hai số nguyên nên thực hiện phép chia nguyên trước khi gán)" },
                        new() { Id = "B", Text = "2.5" },
                        new() { Id = "C", Text = "3.0" },
                        new() { Id = "D", Text = "Báo lỗi cú pháp khi biên dịch" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Phép toán '5 / 2' diễn ra giữa hai toán hạng số nguyên nên kết quả là số nguyên 2, sau đó mới được chuyển đổi sang kiểu float thành 2.0.",
                    DoKho = doKho
                }
            };

            return Task.FromResult(SelectFromPool(pool, count, "Nhập môn lập trình", doKho));
        }

        // 3. NGÂN HÀNG: LẬP TRÌNH C# & HƯỚNG ĐỐI TƯỢNG (COM103)
        if (normalizedTopic.Contains("c#") || normalizedTopic.Contains("huong doi tuong") || normalizedTopic.Contains("oop") || normalizedSubject.Contains("c#") || normalizedSubject.Contains("huong doi tuong"))
        {
            var pool = new List<AiGeneratedQuestionDto>
            {
                new()
                {
                    NoiDung = "Trong lập trình hướng đối tượng (OOP), tính chất nào cho phép che giấu trạng thái nội bộ của đối tượng và chỉ cho phép tương tác qua các phương thức công khai?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Tính Đóng gói (Encapsulation)" },
                        new() { Id = "B", Text = "Tính Kế thừa (Inheritance)" },
                        new() { Id = "C", Text = "Tính Đa hình (Polymorphism)" },
                        new() { Id = "D", Text = "Tính Trừu tượng (Abstraction)" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Đóng gói (Encapsulation) bảo vệ dữ liệu bên trong bằng cách gán phạm vi truy cập private/protected và cung cấp getter/setter.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong ngôn ngữ C#, từ khóa nào được sử dụng trên phương thức của lớp cha để cho phép lớp con ghi đè (override) lại hành vi?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "virtual" },
                        new() { Id = "B", Text = "static" },
                        new() { Id = "C", Text = "sealed" },
                        new() { Id = "D", Text = "readonly" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Từ khóa 'virtual' cho phép phương thức được ghi đè bằng từ khóa 'override' trong lớp dẫn xuất kế thừa.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Điểm khác biệt căn bản giữa Interface và Abstract Class trong C# là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Một lớp có thể triển khai nhiều Interface, nhưng chỉ có thể kế thừa từ một Abstract Class duy nhất." },
                        new() { Id = "B", Text = "Interface có thể chứa constructor để khởi tạo đối tượng trực tiếp." },
                        new() { Id = "C", Text = "Abstract class không được phép chứa bất kỳ phương thức nào có thân hàm cụ thể." },
                        new() { Id = "D", Text = "Interface bắt buộc mọi thành viên phải có phạm vi truy cập là private." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "C# là ngôn ngữ đơn kế thừa lớp (chỉ 1 base class), nhưng hỗ trợ đa kế thừa giao diện (triển khai nhiều interface).",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khối lệnh 'finally' trong cấu trúc 'try-catch-finally' của C# có đặc điểm thực thi nào?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Luôn được thực thi bất kể có xảy ra ngoại lệ (exception) hay không, thích hợp để giải phóng tài nguyên." },
                        new() { Id = "B", Text = "Chỉ được thực thi khi có ngoại lệ xảy ra trong khối try." },
                        new() { Id = "C", Text = "Chỉ được thực thi khi không có bất kỳ ngoại lệ nào xảy ra." },
                        new() { Id = "D", Text = "Tự động bỏ qua nếu khối catch đã xử lý xong ngoại lệ." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Khối finally đảm bảo việc dọn dẹp và giải phóng tài nguyên (như đóng kết nối DB, đóng luồng file) luôn được thực hiện an toàn.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong công nghệ LINQ của C#, phương thức nào được sử dụng để lọc các phần tử thỏa mãn một điều kiện vị từ (predicate)?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Where" },
                        new() { Id = "B", Text = "Select" },
                        new() { Id = "C", Text = "OrderBy" },
                        new() { Id = "D", Text = "GroupBy" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Phương thức 'Where' nhận một hàm lambda trả về boolean để lọc danh sách các phần tử thỏa mãn điều kiện.",
                    DoKho = doKho
                }
            };

            return Task.FromResult(SelectFromPool(pool, count, "Lập trình C#", doKho));
        }

        // 4. NGÂN HÀNG: THIẾT KẾ WEB & JAVASCRIPT (WEB101 / WEB102)
        if (normalizedTopic.Contains("web") || normalizedTopic.Contains("javascript") || normalizedTopic.Contains("html") || normalizedTopic.Contains("css") || normalizedSubject.Contains("web"))
        {
            var pool = new List<AiGeneratedQuestionDto>
            {
                new()
                {
                    NoiDung = "Trong chuẩn HTML5, các thẻ như '<header>', '<nav>', '<main>', '<section>', '<footer>' được gọi là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Thẻ ngữ nghĩa (Semantic HTML Elements)" },
                        new() { Id = "B", Text = "Thẻ định dạng kiểu chữ cổ điển (Typography Elements)" },
                        new() { Id = "C", Text = "Thẻ xử lý logic phía máy chủ (Server-side Tags)" },
                        new() { Id = "D", Text = "Thẻ mã hóa âm thanh nhị phân (Binary Tags)" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Semantic HTML giúp các công cụ tìm kiếm (SEO) và thiết bị hỗ trợ tiếp cận (Screen reader) hiểu rõ cấu trúc ý nghĩa của trang web.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong CSS Box Model, thành phần nào nằm giữa đường viền (border) và nội dung thực tế (content) của phần tử?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Padding" },
                        new() { Id = "B", Text = "Margin" },
                        new() { Id = "C", Text = "Outline" },
                        new() { Id = "D", Text = "Box-sizing" }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Box Model gồm thứ tự từ trong ra ngoài: Content -> Padding (khoảng đệm) -> Border (đường viền) -> Margin (lề ngoài).",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Khác biệt bản chất giữa từ khóa 'let' và 'var' khi khai báo biến trong JavaScript (ES6+) là gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "'let' có phạm vi khối lệnh (Block scope), trong khi 'var' có phạm vi hàm (Function scope)." },
                        new() { Id = "B", Text = "'let' chỉ dùng được với số nguyên, còn 'var' dùng được với chuỗi ký tự." },
                        new() { Id = "C", Text = "'var' không thể gán lại giá trị sau khi đã khởi tạo." },
                        new() { Id = "D", Text = "'let' bắt buộc phải khai báo giá trị khởi tạo ngay tại dòng khai báo." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "let và const trong ES6 tuân theo Block Scope (phạm vi cặp ngoặc nhọn {}), ngăn ngừa các lỗi rò rỉ biến ngoài ý muốn của var.",
                    DoKho = doKho
                },
                new()
                {
                    NoiDung = "Trong JavaScript bất đồng bộ, cơ chế 'Promise' đại diện cho điều gì?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = "Một đối tượng đại diện cho kết quả cuối cùng (thành công hoặc thất bại) của một thao tác bất đồng bộ." },
                        new() { Id = "B", Text = "Một biến lưu trữ địa chỉ IP của máy chủ web." },
                        new() { Id = "C", Text = "Một hàm tự động dừng toàn bộ trình duyệt cho đến khi tải xong ảnh." },
                        new() { Id = "D", Text = "Một phương thức xóa vĩnh viễn LocalStorage trên trình duyệt." }
                    },
                    DapAnDung = "A",
                    GiaiThich = "Promise có 3 trạng thái: Pending (đang chờ), Fulfilled (thành công) và Rejected (thất bại), giải quyết vấn đề Callback Hell.",
                    DoKho = doKho
                }
            };

            return Task.FromResult(SelectFromPool(pool, count, "Thiết kế Web", doKho));
        }

        // 5. CÁC MÔN HỌC KHÁC: TỔNG QUÁT KHOA HỌC MÁY TÍNH & HỌC THUẬT
        var generalPool = new List<AiGeneratedQuestionDto>
        {
            new()
            {
                NoiDung = $"Khái niệm nền tảng quan trọng nhất trong đề cương học phần '{tenMonHoc}' tập trung vào mục tiêu nào sau đây?",
                LuaChon = new List<AiQuestionChoiceDto>
                {
                    new() { Id = "A", Text = $"Nắm vững nguyên lý cốt lõi, phương pháp luận và kỹ năng thực hành chuẩn mực của môn học {tenMonHoc}." },
                    new() { Id = "B", Text = $"Chỉ học lý thuyết trừu tượng mà không cần áp dụng vào các bài tập dự án thực tế." },
                    new() { Id = "C", Text = $"Bỏ qua các tiêu chuẩn an toàn và bảo mật thông tin trong quá trình thiết kế hệ thống." },
                    new() { Id = "D", Text = $"Chỉ sử dụng các công nghệ cũ đã lỗi thời trong môi trường phát triển." }
                },
                DapAnDung = "A",
                GiaiThich = $"Mục tiêu chuẩn đầu ra của học phần {tenMonHoc} là giúp người học nắm vững bản chất kiến thức và ứng dụng giải quyết bài toán thực tế.",
                DoKho = doKho
            },
            new()
            {
                NoiDung = $"Phương pháp nào sau đây là tối ưu nhất khi phân tích và giải quyết các bài toán học thuật trong học phần '{tenMonHoc}'?",
                LuaChon = new List<AiQuestionChoiceDto>
                {
                    new() { Id = "A", Text = $"Phân rã bài toán thành các bài toán con độc lập (Divide and Conquer) và thiết kế giải pháp theo từng giai đoạn." },
                    new() { Id = "B", Text = $"Thử sai ngẫu nhiên mà không dựa trên cơ sở dữ liệu và tài liệu kỹ thuật." },
                    new() { Id = "C", Text = $"Bỏ qua các bước kiểm thử và xác thực dữ liệu đầu vào." },
                    new() { Id = "D", Text = $"Chỉ triển khai theo trực giác mà không lập kế hoạch kiến trúc trước." }
                },
                DapAnDung = "A",
                GiaiThich = "Nguyên tắc tiếp cận có cấu trúc và phân rã mô-đun là nền tảng của kỹ thuật công nghệ thông tin và khoa học máy tính.",
                DoKho = doKho
            },
            new()
            {
                NoiDung = $"Khi đánh giá chất lượng và hiệu quả triển khai trong phạm vi môn học '{tenMonHoc}', tiêu chí nào sau đây là ưu tiên hàng đầu?",
                LuaChon = new List<AiQuestionChoiceDto>
                {
                    new() { Id = "A", Text = "Tính đúng đắn, tính toàn vẹn dữ liệu, khả năng mở rộng và tuân thủ chuẩn quy ước ngành." },
                    new() { Id = "B", Text = "Độ dài mã nguồn càng nhiều dòng càng tốt mà không tối ưu thời gian chạy." },
                    new() { Id = "C", Text = "Chỉ tập trung vào giao diện mà bỏ qua hiệu năng và bảo mật hệ thống." },
                    new() { Id = "D", Text = "Sử dụng tài nguyên phần cứng tối đa mà không kiểm soát xung đột tài nguyên." }
                },
                DapAnDung = "A",
                GiaiThich = "Chất lượng giải pháp trong chuyên ngành CNTT được đo lường bởi tính chính xác, hiệu năng, bảo mật và khả năng bảo trì.",
                DoKho = doKho
            }
        };

        return Task.FromResult(SelectFromPool(generalPool, count, tenMonHoc, doKho));
    }

    private static List<AiGeneratedQuestionDto> SelectFromPool(List<AiGeneratedQuestionDto> pool, int count, string subjectName, string doKho)
    {
        var selected = new List<AiGeneratedQuestionDto>();
        for (int i = 0; i < count; i++)
        {
            if (i < pool.Count)
            {
                selected.Add(pool[i]);
            }
            else
            {
                int extraIdx = i + 1;
                selected.Add(new AiGeneratedQuestionDto
                {
                    NoiDung = $"Câu hỏi nâng cao {extraIdx} ({subjectName}): Nguyên tắc thiết kế và tối ưu hóa nào sau đây là then chốt trong thực tế triển khai?",
                    LuaChon = new List<AiQuestionChoiceDto>
                    {
                        new() { Id = "A", Text = $"Tối ưu hóa tài nguyên, đảm bảo tính nhất quán dữ liệu và tuân thủ các quy chuẩn kỹ thuật chuyên ngành {subjectName}." },
                        new() { Id = "B", Text = "Tăng độ phức tạp thuật toán để kéo dài thời gian xử lý." },
                        new() { Id = "C", Text = "Loại bỏ hoàn toàn các cơ chế ghi nhận log và kiểm tra lỗi ngoại lệ." },
                        new() { Id = "D", Text = "Chỉ áp dụng các giải pháp tạm thời không có khả năng bảo trì lâu dài." }
                    },
                    DapAnDung = "A",
                    GiaiThich = $"Nguyên tắc chuẩn mực trong ngành là tối ưu hiệu năng, bảo đảm tính nhất quán và dễ mở rộng của hệ thống {subjectName}.",
                    DoKho = doKho
                });
            }
        }
        return selected;
    }

    private static string GetRelevantRagContext(string query, bool forceUseRag)
    {
        var v = NormalizeVietnamese(query);
        bool isPolicyQuery = forceUseRag || v.Contains("quy che") || v.Contains("quy dinh") || v.Contains("hoc bong")
            || v.Contains("diem d") || v.Contains("thang diem") || v.Contains("gpa") || v.Contains("cpa")
            || v.Contains("thi lai") || v.Contains("hoc lai") || v.Contains("phuc khao") || v.Contains("cam thi")
            || v.Contains("chuyen can") || v.Contains("tot nghiep") || v.Contains("canh bao");

        if (!isPolicyQuery) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("[TÀI LIỆU QUY CHẾ ĐÀO TẠO & KHẢO THÍ CHÍNH THỨC CỦA NHÀ TRƯỜNG (RAG - TRÍCH TỪ QUY CHẾ 2026)]:");
        sb.AppendLine("- Điều 8 (Thang điểm và quy đổi):");
        sb.AppendLine("  + Điểm số 8.5 - 10.0: Điểm chữ A (Thang 4: 4.0 - Xuất sắc).");
        sb.AppendLine("  + Điểm số 8.0 - 8.4: Điểm chữ B+ (Thang 4: 3.5 - Giỏi).");
        sb.AppendLine("  + Điểm số 7.0 - 7.9: Điểm chữ B (Thang 4: 3.0 - Khá).");
        sb.AppendLine("  + Điểm số 6.5 - 6.9: Điểm chữ C+ (Thang 4: 2.5 - Trung bình khá).");
        sb.AppendLine("  + Điểm số 5.5 - 6.4: Điểm chữ C (Thang 4: 2.0 - Trung bình).");
        sb.AppendLine("  + Điểm số 5.0 - 5.4: Điểm chữ D+ (Thang 4: 1.5 - Trung bình yếu).");
        sb.AppendLine("  + Điểm số 4.0 - 4.9: Điểm chữ D (Thang 4: 1.0 - Yếu, nhưng vẫn ĐẠT môn học).");
        sb.AppendLine("  + Điểm số Dưới 4.0: Điểm chữ F (Thang 4: 0.0 - Kém, Không đạt, bắt buộc phải đăng ký học lại).");
        sb.AppendLine("- Điều 9 (Điểm tích lũy CPA/GPA): Công thức CPA = Σ(Điểm thang 4 × Số tín chỉ) / Σ(Số tín chỉ đã đăng ký). ĐIỂM D VẪN ĐƯỢC TÍNH VÀO CPA/GPA VỚI GIÁ TRỊ 1.0 (KHÔNG PHẢI BỊ LOẠI BỎ KHỎI GPA).");
        sb.AppendLine("- Điều 10 (Điều kiện dự thi): Sinh viên vắng mặt quá 20% tổng số tiết sẽ bị cấm thi (nhận điểm F cho toàn môn).");
        sb.AppendLine("- Điều 11 (Thi cải thiện): Sinh viên đạt điểm D, D+ (đạt nhưng thấp) được đăng ký thi cải thiện điểm tối đa 1 lần.");
        sb.AppendLine("- Điều 12 (Phúc khảo): Thời hạn nộp đơn phúc khảo trong vòng 05 ngày làm việc kể từ ngày công bố điểm, lệ phí 50.000 VNĐ.");
        sb.AppendLine("- Điều 13 (Xét tốt nghiệp): Tích lũy đủ tín chỉ (≥ 120 TC), CPA ≥ 2.00, không còn môn điểm F, chuẩn ngoại ngữ TOEIC ≥ 450.");

        if (v.Contains("hoc bong") || v.Contains("diem d") || v.Contains("khen thuong"))
        {
            sb.AppendLine("\n[QUY ĐỊNH XÉT HỌC BỔNG KHUYẾN KHÍCH HỌC TẬP]:");
            sb.AppendLine("- Tiêu chí 1: Điểm GPA tích lũy học kỳ phải đạt từ 2.50 trở lên (thang 4).");
            sb.AppendLine("- Tiêu chí 2: Điểm rèn luyện trong kỳ đạt loại Khá trở lên.");
            sb.AppendLine("- ĐIỀU KIỆN TIÊN QUYẾT BẮT BUỘC: Không có bất kỳ học phần nào bị điểm F hoặc ĐIỂM D trong học kỳ đó (tất cả các môn học phần đều phải đạt điểm C trở lên, tức từ 5.5/10).");
            sb.AppendLine("- KẾT LUẬN CHO SINH VIÊN: Dù sinh viên có GPA 3.6 (mức Giỏi/Xuất sắc), nhưng nếu trong kỳ có bất kỳ môn nào bị điểm D thì KHÔNG ĐỦ ĐIỀU KIỆN được xét nhận học bổng khuyến khích học tập của kỳ đó. Tuy nhiên, sinh viên được phép đăng ký thi cải thiện môn điểm D đó theo Điều 11.");
        }

        return sb.ToString();
    }

    private async Task<string> BuildSystemPromptAsync(
        CurrentUserContext? userContext,
        StudentAcademicContext? studentAcademicContext,
        TeacherAcademicContext? teacherAcademicContext,
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
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ xây dựng giáo án, gợi ý câu hỏi trắc nghiệm/tự luận, tóm tắt tài liệu chuyên môn, tư vấn quy trình nhập điểm, phân tích sinh viên nguy cơ rớt môn và quản lý lịch dạy.");
            sb.AppendLine("QUY TẮC BẢO MẬT: TUYỆT ĐỐI KHÔNG cung cấp tình trạng giảng dạy, số giờ dạy, đánh giá hoặc xếp hạng của các giảng viên khác.");

            if (teacherAcademicContext != null)
            {
                sb.AppendLine("\n[DỮ LIỆU GIẢNG DẠY THỰC TẾ CỦA GIẢNG VIÊN ĐANG ĐĂNG NHẬP]:");
                sb.AppendLine($"- Họ và tên: {teacherAcademicContext.TeacherName} (Tổng số lớp phụ trách: {teacherAcademicContext.TotalClasses}, Tổng số sinh viên: {teacherAcademicContext.TotalStudents})");
                sb.AppendLine($"- Số bài tập sinh viên đã nộp đang chờ chấm: {teacherAcademicContext.PendingGradingCount} bài");
                if (teacherAcademicContext.CourseNames.Count > 0)
                {
                    sb.AppendLine($"- Các môn học/khóa học đang phụ trách: {string.Join(", ", teacherAcademicContext.CourseNames)}");
                }
                if (teacherAcademicContext.TodaySchedule.Count > 0)
                {
                    sb.AppendLine("- Lịch dạy hôm nay:");
                    foreach (var sc in teacherAcademicContext.TodaySchedule)
                    {
                        sb.AppendLine($"  + {sc.TimeRange}: Môn {sc.SubjectName} ({sc.CourseCode}), Phòng {sc.Room}, Lớp {sc.ClassName}");
                    }
                }
                else
                {
                    sb.AppendLine("- Hôm nay: Giảng viên không có ca dạy nào lên lớp.");
                }

                if (teacherAcademicContext.AtRiskClasses.Count > 0)
                {
                    sb.AppendLine("- Tình hình sinh viên có nguy cơ trong các lớp phụ trách:");
                    foreach (var arc in teacherAcademicContext.AtRiskClasses)
                    {
                        sb.AppendLine($"  + {arc.ClassName} ({arc.SubjectName}): {arc.AtRiskCount} sinh viên có nguy cơ (Chi tiết: {arc.Details})");
                    }
                }
            }
        }
        else if (role == AuthRoles.HoiDongQuanLyNoiDung || role == "hoidong_quanly_noidung")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: HỘI ĐỒNG QUẢN LÝ NỘI DUNG & THẨM ĐỊNH]");
            sb.AppendLine("Nhiệm vụ của bạn:");
            sb.AppendLine("- Hỗ trợ thẩm định và rà soát đề cương môn học (Syllabus), chuẩn đầu ra môn học (CLO) đối sánh với chuẩn đầu ra chương trình đào tạo (PLO).");
            sb.AppendLine("- Kiểm tra sự cân đối giữa các hình thức học tập: Lý thuyết (LT), Thực hành (TH), Tự học (Self-study).");
            sb.AppendLine("- Đánh giá ngân hàng câu hỏi, ma trận đề thi và rubric đánh giá thành phần theo thang đo nhận thức Bloom.");
            sb.AppendLine("- Đưa ra khuyến nghị chỉnh sửa cấu trúc bài giảng để nâng cao tính ứng dụng thực tiễn cho sinh viên.");
        }
        else if (role == AuthRoles.AcademicStaff || role == "nhan_vien")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ: CÁN BỘ GIÁO VỤ]");
            sb.AppendLine("Nhiệm vụ của bạn: Hỗ trợ tra cứu quy chế học vụ, tiêu chuẩn xếp thời khóa biểu và xử lý đơn từ học sinh.");
        }
        else if (role == AuthRoles.Principal || role == AuthRoles.CampusAdmin || role == "hieu_truong" || role == "BanGiamHieu" || role == "bgh" || role == "Principal")
        {
            sb.AppendLine("\n[NGỮ CẢNH VAI TRÒ ĐIỀU HÀNH CẤP CAO: BAN GIÁM HIỆU / HIỆU TRƯỞNG / LÃNH ĐẠO TRƯỜNG]");
            sb.AppendLine("Nhiệm vụ của bạn: Là Cố vấn Chiến lược Cấp cao Trực tiếp cho Ban Giám hiệu, hỗ trợ ra quyết định điều hành học thuật, khen thưởng, cơ sở vật chất và nhân sự.");
            sb.AppendLine("ĐẶC ĐIỂM CÂU TRẢ LỜI DÀNH CHO BAN GIÁM HIỆU:");
            sb.AppendLine("- Luôn có góc nhìn vĩ mô chiến lược, số liệu rõ ràng, hành văn đĩnh đạc, tự tin, chuyên nghiệp và có tính thuyết phục cao.");
            sb.AppendLine("- Đưa ra các khuyến nghị hành động cụ thể theo từng bước.");
            sb.AppendLine("- Dẫn chứng trực tiếp các số liệu điều hành thực tế từ cơ sở dữ liệu khi phân tích báo cáo.");
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

        // TUYỆT ĐỐI LOẠI TRỪ CÁC CÂU HỎI VỀ HỌC BỔNG, QUY CHẾ, TICKET, TẠO QUIZ, FILE WORD
        bool isExcludedFromGradeReply = v.Contains("hoc bong") || v.Contains("quy che") || v.Contains("quy dinh")
            || v.Contains("dieu kien") || v.Contains("xet") || v.Contains("ticket") || v.Contains("khieu nai")
            || v.Contains("phuc khao") || v.Contains("tao") || v.Contains("trac nghiem") || v.Contains("cau hoi")
            || v.Contains("on tap") || v.Contains("bao loi") || v.Contains("ho tro") || v.Contains("file") || v.Contains("word");

        if (isExcludedFromGradeReply)
        {
            reply = string.Empty;
            return false;
        }

        // CHỈ KÍCH HOẠT KHI SINH VIÊN YÊU CẦU XEM BẢNG ĐIỂM HOẶC KẾT QUẢ CÁ NHÂN CỦA MÌNH
        bool isExplicitGradeRequest = v.Contains("bang diem") || v.Contains("xem diem") || v.Contains("tra cuu diem")
            || v.Contains("diem cua toi") || v.Contains("gpa cua toi") || v.Contains("ket qua hoc tap")
            || v.Contains("ket qua ky") || v.Contains("ket qua hoc ky")
            || (v.Contains("diem") && (v.Contains("cua toi") || v.Contains("minh")));

        if (!isExplicitGradeRequest)
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

    public async Task<AiDashboardInsightDto> GetDashboardInsightAsync(CurrentUserContext? userContext, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (userContext == null || userContext.UserId <= 0)
        {
            throw new ApiException(401, "Người dùng chưa được xác thực.");
        }

        var role = userContext.Role ?? AuthRoles.Student;
        var cacheKey = $"ai_insight:{role}:{userContext.UserId}";

        if (!forceRefresh && _cache.TryGetValue(cacheKey, out AiDashboardInsightDto? cachedInsight) && cachedInsight != null)
        {
            cachedInsight.Cached = true;
            return cachedInsight;
        }

        var insight = new AiDashboardInsightDto
        {
            Role = role,
            GeneratedAt = DateTime.UtcNow,
            Cached = false
        };

        if (role == AuthRoles.Student || role == "hoc_sinh")
        {
            var studentCtx = await LoadStudentAcademicContextAsync(userContext.UserId, cancellationToken);
            if (studentCtx != null)
            {
                var summarySb = new StringBuilder();
                summarySb.Append($"Chào {studentCtx.StudentName}, điểm GPA hiện tại của bạn là {studentCtx.CumulativeGpa:0.00} ({studentCtx.Classification}). ");

                if (studentCtx.AttendanceSummary.AttendanceRate < 80)
                {
                    summarySb.Append($"Tỷ lệ chuyên cần của bạn đang ở mức {studentCtx.AttendanceSummary.AttendanceRate:0.0}%, cần chú ý để tránh bị cấm thi. ");
                }

                if (studentCtx.PendingAssignments.Count > 0)
                {
                    summarySb.Append($"Bạn đang có {studentCtx.PendingAssignments.Count} bài tập cần hoàn thành.");
                }
                else
                {
                    summarySb.Append("Tiến độ nộp bài của bạn đang rất tốt, không có bài tập tồn đọng.");
                }

                insight.ExecutiveSummary = summarySb.ToString();

                if (studentCtx.PendingAssignments.Count > 0)
                {
                    var nearest = studentCtx.PendingAssignments.OrderBy(a => a.Deadline).First();
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Hoàn thành bài tập sắp hạn",
                        Description = $"Môn {nearest.SubjectName}: '{nearest.Title}' trước {nearest.Deadline:HH:mm dd/MM}",
                        Severity = "warning",
                        ActionPrompt = $"Hướng dẫn phương pháp làm bài tập '{nearest.Title}' môn {nearest.SubjectName}"
                    });
                }

                var lowestGrade = studentCtx.Grades.Where(g => g.Gpa > 0).OrderBy(g => g.Gpa).FirstOrDefault();
                if (lowestGrade != null && lowestGrade.Gpa < 6.5m)
                {
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Cải thiện điểm số học phần",
                        Description = $"Môn {lowestGrade.SubjectName} hiện có điểm {lowestGrade.Gpa:0.0}, cần tăng cường ôn tập",
                        Severity = "danger",
                        ActionPrompt = $"Gợi ý kế hoạch ôn tập cải thiện điểm môn {lowestGrade.SubjectName}"
                    });
                }

                if (studentCtx.UpcomingClasses.Count > 0)
                {
                    var nextClass = studentCtx.UpcomingClasses.First();
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Chuẩn bị bài học tiếp theo",
                        Description = $"{nextClass.SubjectName} ({nextClass.TimeRange}, phòng {nextClass.Room})",
                        Severity = "info",
                        ActionPrompt = $"Tóm tắt kiến thức trọng tâm cần chuẩn bị trước buổi học môn {nextClass.SubjectName}"
                    });
                }
            }
            else
            {
                insight.ExecutiveSummary = "Hệ thống đang đồng bộ dữ liệu học tập cá nhân của bạn.";
            }
        }
        else if (role == AuthRoles.Teacher || role == "giao_vien")
        {
            var teacherCtx = await LoadTeacherAcademicContextAsync(userContext.UserId, cancellationToken);
            if (teacherCtx != null)
            {
                var summarySb = new StringBuilder();
                summarySb.Append($"Chào Thầy/Cô {teacherCtx.TeacherName}. Hiện Thầy/Cô đang phụ trách {teacherCtx.TotalClasses} lớp ({teacherCtx.TotalStudents} sinh viên). ");

                if (teacherCtx.TodaySchedule.Count > 0)
                {
                    summarySb.Append($"Hôm nay có {teacherCtx.TodaySchedule.Count} ca dạy lên lớp. ");
                }
                else
                {
                    summarySb.Append("Hôm nay Thầy/Cô không có ca dạy lên lớp. ");
                }

                if (teacherCtx.PendingGradingCount > 0)
                {
                    summarySb.Append($"Có {teacherCtx.PendingGradingCount} bài tập sinh viên đã nộp đang chờ chấm điểm.");
                }

                insight.ExecutiveSummary = summarySb.ToString();

                if (teacherCtx.PendingGradingCount > 0)
                {
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Chấm điểm bài tập tồn đọng",
                        Description = $"{teacherCtx.PendingGradingCount} bài nộp của sinh viên đang chờ phản hồi",
                        Severity = "warning",
                        ActionPrompt = "Đề xuất tiêu chí chấm nhanh và gợi ý thang điểm rubric cho các bài tập"
                    });
                }

                if (teacherCtx.AtRiskClasses.Count > 0)
                {
                    var firstAtRisk = teacherCtx.AtRiskClasses.First();
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Theo dõi sinh viên cần hỗ trợ",
                        Description = $"{firstAtRisk.ClassName}: {firstAtRisk.Details}",
                        Severity = "danger",
                        ActionPrompt = $"Gợi ý kế hoạch phụ đạo cho sinh viên có nguy cơ rớt môn {firstAtRisk.SubjectName}"
                    });
                }

                if (teacherCtx.TodaySchedule.Count > 0)
                {
                    var nextSession = teacherCtx.TodaySchedule.First();
                    insight.ActionItems.Add(new AiInsightActionItem
                    {
                        Title = "Ca dạy hôm nay",
                        Description = $"{nextSession.SubjectName} ({nextSession.TimeRange}, phòng {nextSession.Room})",
                        Severity = "info",
                        ActionPrompt = $"Gợi ý 3 câu hỏi trắc nghiệm ôn bài nhanh cho môn {nextSession.SubjectName}"
                    });
                }
            }
            else
            {
                insight.ExecutiveSummary = "Hệ thống đang đồng bộ dữ liệu giảng dạy của Thầy/Cô.";
            }
        }
        else if (role == AuthRoles.HoiDongQuanLyNoiDung || role == "hoidong_quanly_noidung")
        {
            insight.ExecutiveSummary = "Chào mừng Hội đồng Quản lý Nội dung & Thẩm định. Trợ lý AI sẵn sàng hỗ trợ rà soát cấu trúc chương trình đào tạo, đối sánh CLO-PLO và kiểm định ngân hàng câu hỏi.";
            insight.ActionItems.Add(new AiInsightActionItem
            {
                Title = "Thẩm định ma trận chuẩn đầu ra",
                Description = "Rà soát tính liên kết giữa chuẩn đầu ra môn học (CLO) và chương trình đào tạo (PLO)",
                Severity = "info",
                ActionPrompt = "Hướng dẫn phương pháp thẩm định ma trận chuẩn đầu ra CLO-PLO theo chuẩn kiểm định AUN-QA"
            });
            insight.ActionItems.Add(new AiInsightActionItem
            {
                Title = "Kiểm tra cân đối tải học tập",
                Description = "Đảm bảo tỷ lệ số tiết Lý thuyết / Thực hành / Tự học phù hợp số tín chỉ",
                Severity = "warning",
                ActionPrompt = "Quy định tính giờ chuẩn giảng dạy và tỷ lệ số tiết thực hành lý thuyết cho học phần 3 tín chỉ"
            });
        }
        else if (role == AuthRoles.Principal || role == AuthRoles.CampusAdmin || role == "hieu_truong" || role == "BanGiamHieu" || role == "bgh" || role == AuthRoles.SuperAdmin || role == "Principal")
        {
            insight.ExecutiveSummary = "Kính chào Ban Giám hiệu. Toàn bộ 24 phòng học cơ sở đang hoạt động ổn định (100%). Đã tổng kết 3 đợt khen thưởng và chuẩn bị danh sách Top 3 GPA Thủ khoa/Á khoa vinh danh tại Lễ Khai giảng.";
            insight.ActionItems.Add(new AiInsightActionItem
            {
                Title = "Khen thưởng Top 3 Sinh viên GPA Cao nhất Năm học",
                Description = "Vinh danh Thủ khoa Hồ Chí Minh 0363 (GPA 10.00) và 2 Á khoa tại Lễ Khai giảng",
                Severity = "info",
                ActionPrompt = "Đề xuất kế hoạch khen thưởng và phát hành bằng khen cho Top 3 GPA năm học"
            });
            insight.ActionItems.Add(new AiInsightActionItem
            {
                Title = "Bảo trì & Kiểm tra CSVC phòng học trước kỳ thi",
                Description = "Rà soát định kỳ 24 phòng học và dàn thiết bị máy chiếu, điều hòa các tòa nhà",
                Severity = "warning",
                ActionPrompt = "Lập phương án kiểm tra và bảo dưỡng trang thiết bị phòng học trước kỳ thi học kỳ"
            });
            insight.ActionItems.Add(new AiInsightActionItem
            {
                Title = "Đánh giá chất lượng giảng viên & Bồi dưỡng sư phạm",
                Description = "Phân tích phản hồi sinh viên và tổ chức workshop bồi dưỡng phương pháp giảng dạy",
                Severity = "info",
                ActionPrompt = "Kế hoạch nâng cao chất lượng giảng dạy từ kết quả khảo sát sinh viên"
            });
        }
        else
        {
            insight.ExecutiveSummary = "Trợ lý AI hệ thống AET LMS sẵn sàng hỗ trợ bạn tra cứu và vận hành học vụ.";
        }

        _cache.Set(cacheKey, insight, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            Size = 1
        });

        return insight;
    }

    private async Task<TeacherAcademicContext?> LoadTeacherAcademicContextAsync(int teacherId, CancellationToken cancellationToken)
    {
        try
        {
            var teacher = await _db.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.MaNguoiDung == teacherId, cancellationToken);

            if (teacher == null) return null;

            var courses = await _db.KhoaHocs
                .AsNoTracking()
                .Include(k => k.MonHoc)
                .Include(k => k.Lop)
                .Where(k => k.MaGiaoVien == teacherId)
                .ToListAsync(cancellationToken);

            var totalClasses = courses.Count;
            var courseMonHocIds = courses.Where(k => k.MaMonHoc > 0).Select(k => k.MaMonHoc).Distinct().ToList();
            var classIds = courses.Where(k => k.MaLop > 0).Select(k => k.MaLop).Distinct().ToList();

            var totalStudents = await _db.NguoiDungs
                .AsNoTracking()
                .Where(n => n.MaLop.HasValue && classIds.Contains(n.MaLop.Value))
                .Select(n => n.MaNguoiDung)
                .Distinct()
                .CountAsync(cancellationToken);

            var pendingGrading = await _db.BaiNops
                .AsNoTracking()
                .Where(b => b.DiemSo == null && b.BaiTap != null && courseMonHocIds.Contains(b.BaiTap.MaMonHoc))
                .CountAsync(cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            var todaySessions = await _db.BuoiHocs
                .AsNoTracking()
                .Include(b => b.KhoaHoc).ThenInclude(k => k!.MonHoc)
                .Include(b => b.KhoaHoc).ThenInclude(k => k!.Lop)
                .Include(b => b.CaHoc)
                .Include(b => b.Phong)
                .Where(b => b.MaGiaoVien == teacherId && b.NgayHoc == today && b.TrangThaiBuoi != "da_huy")
                .OrderBy(b => b.CaHoc != null ? b.CaHoc.GioBatDau : TimeOnly.MinValue)
                .ToListAsync(cancellationToken);

            var scheduleItems = todaySessions.Select(s => new TeacherTodayScheduleItem
            {
                SubjectName = s.KhoaHoc?.MonHoc?.TenMonHoc ?? s.KhoaHoc?.TieuDe ?? "Môn học",
                CourseCode = s.KhoaHoc?.MonHoc?.MaCodeMonHoc ?? "",
                ClassName = s.KhoaHoc?.Lop?.TenLop ?? "Chung",
                TimeRange = s.CaHoc != null ? $"{s.CaHoc.GioBatDau:HH\\:mm} - {s.CaHoc.GioKetThuc:HH\\:mm}" : "",
                Room = s.Phong?.TenPhong ?? "Chưa xếp phòng"
            }).ToList();

            var atRiskList = new List<TeacherAtRiskClassItem>();
            foreach (var c in courses.Take(5))
            {
                if (c.MaLop <= 0) continue;
                var lowScoreCount = await _db.DiemSos
                    .AsNoTracking()
                    .Where(d => d.MaMonHoc == c.MaMonHoc && d.HocSinh != null && d.HocSinh.MaLop == c.MaLop && d.GpaMonHoc > 0 && d.GpaMonHoc < 5.0m)
                    .CountAsync(cancellationToken);

                if (lowScoreCount > 0)
                {
                    atRiskList.Add(new TeacherAtRiskClassItem
                    {
                        ClassName = c.Lop?.TenLop ?? "Lớp",
                        SubjectName = c.MonHoc?.TenMonHoc ?? c.TieuDe,
                        AtRiskCount = lowScoreCount,
                        Details = $"{lowScoreCount} sinh viên điểm tích lũy môn dưới 5.0"
                    });
                }
            }

            return new TeacherAcademicContext
            {
                TeacherId = teacherId,
                TeacherName = string.IsNullOrWhiteSpace(teacher.HoTen) ? "Giảng viên" : teacher.HoTen,
                TotalClasses = totalClasses,
                TotalStudents = totalStudents,
                PendingGradingCount = pendingGrading,
                CourseNames = courses.Select(k => k.MonHoc?.TenMonHoc ?? k.TieuDe).Distinct().ToList(),
                TodaySchedule = scheduleItems,
                AtRiskClasses = atRiskList
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load teacher academic context for teacher {TeacherId}", teacherId);
            return null;
        }
    }

    private static bool TryGetTeacherScheduleReply(
        string input,
        CurrentUserContext? user,
        TeacherAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isScheduleQuery = v.Contains("lich day") || v.Contains("ca day") || v.Contains("tiet day")
            || (v.Contains("day") && (v.Contains("hom nay") || v.Contains("ngay mai") || v.Contains("tuan nay") || v.Contains("lop nao")));

        if (!isScheduleQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Teacher && user.Role != "giao_vien"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Giảng viên** để tra cứu lịch dạy nhé.";
            return true;
        }

        if (context == null || context.TodaySchedule.Count == 0)
        {
            reply = $"### 📅 Lịch dạy hôm nay\n\n"
                  + $"Kính chào Thầy/Cô **{context?.TeacherName ?? "Giảng viên"}**,\n\n"
                  + "🎉 Hệ thống ghi nhận hôm nay Thầy/Cô **không có ca dạy nào lên lớp**. Thầy/Cô có thể vào mục **Thời khóa biểu** để xem lịch giảng dạy các ngày tiếp theo trong tuần nhé!";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine("### 📅 Lịch giảng dạy hôm nay");
        sb.AppendLine();
        sb.AppendLine($"Kính chào Thầy/Cô **{context.TeacherName}**, hôm nay Thầy/Cô có **{context.TodaySchedule.Count} ca dạy**:");
        sb.AppendLine();
        sb.AppendLine("| Giờ & Ca học | Môn học | Mã môn | Lớp sinh viên | Phòng học |");
        sb.AppendLine("| :--- | :--- | :---: | :--- | :--- |");

        foreach (var sc in context.TodaySchedule)
        {
            sb.AppendLine($"| **{sc.TimeRange}** | {sc.SubjectName} | `{sc.CourseCode}` | {sc.ClassName} | 📍 **{sc.Room}** |");
        }

        sb.AppendLine();
        sb.AppendLine("💡 *Chúc Thầy/Cô có một ngày giảng dạy tràn đầy năng lượng và hiệu quả!*");

        reply = sb.ToString();
        return true;
    }

    private static bool TryGetTeacherGradingReply(
        string input,
        CurrentUserContext? user,
        TeacherAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isGradingQuery = v.Contains("cham diem") || v.Contains("bai tap") || v.Contains("bai nop")
            || v.Contains("chua cham") || v.Contains("cho cham") || v.Contains("nhap diem");

        if (!isGradingQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Teacher && user.Role != "giao_vien"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Giảng viên** để kiểm tra tiến độ chấm điểm nhé.";
            return true;
        }

        if (context == null || context.PendingGradingCount == 0)
        {
            reply = $"### 📝 Tình trạng chấm bài tập\n\n"
                  + $"Kính chào Thầy/Cô **{context?.TeacherName ?? "Giảng viên"}**,\n\n"
                  + "🎉 **Tuyệt vời!** Hiện tại Thầy/Cô **không còn bài nộp nào tồn đọng đang chờ chấm điểm**. Tiến độ chấm bài của Thầy/Cô đã hoàn tất 100%!";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine("### 📝 Báo cáo bài nộp cần chấm điểm");
        sb.AppendLine();
        sb.AppendLine($"Kính chào Thầy/Cô **{context.TeacherName}**, hệ thống ghi nhận hiện có **{context.PendingGradingCount} bài nộp** của sinh viên đang chờ chấm điểm.");
        sb.AppendLine();
        sb.AppendLine("📌 **Khuyến nghị hành động:**");
        sb.AppendLine("- Thầy/Cô có thể truy cập mục **Chấm điểm bài tập** trên thanh điều hướng để chấm và gửi nhận xét cho sinh viên.");
        sb.AppendLine("- Việc phản hồi điểm sớm giúp sinh viên kịp thời rút kinh nghiệm cho các bài kiểm tra tiếp theo.");

        reply = sb.ToString();
        return true;
    }

    private static bool TryGetTeacherAtRiskReply(
        string input,
        CurrentUserContext? user,
        TeacherAcademicContext? context,
        out string reply)
    {
        var v = NormalizeVietnamese(input);

        bool isAtRiskQuery = v.Contains("nguy co") || v.Contains("rot mon") || v.Contains("cam thi")
            || v.Contains("vang nhieu") || v.Contains("hoc yeu") || v.Contains("kem");

        if (!isAtRiskQuery)
        {
            reply = string.Empty;
            return false;
        }

        if (user == null || (user.Role != AuthRoles.Teacher && user.Role != "giao_vien"))
        {
            reply = "Bạn vui lòng đăng nhập bằng tài khoản **Giảng viên** để xem danh sách sinh viên cần lưu ý nhé.";
            return true;
        }

        if (context == null || context.AtRiskClasses.Count == 0)
        {
            reply = $"### ⚠️ Tình hình học lực sinh viên\n\n"
                  + $"Kính chào Thầy/Cô **{context?.TeacherName ?? "Giảng viên"}**,\n\n"
                  + "✅ **Tình trạng tích cực:** Các lớp học phần Thầy/Cô phụ trách hiện có tiến độ học tập và chuyên cần đồng đều, chưa ghi nhận nhóm sinh viên có nguy cơ cao về điểm số.";
            return true;
        }

        var sb = new StringBuilder();
        sb.AppendLine("### ⚠️ Báo cáo sinh viên cần hỗ trợ học thuật");
        sb.AppendLine();
        sb.AppendLine($"Kính chào Thầy/Cô **{context.TeacherName}**, dưới đây là danh sách các lớp học phần có sinh viên cần lưu ý phụ đạo:");
        sb.AppendLine();
        sb.AppendLine("| Lớp học phần | Môn học | Số lượng cần hỗ trợ | Chi tiết dấu hiệu |");
        sb.AppendLine("| :--- | :--- | :---: | :--- |");

        foreach (var arc in context.AtRiskClasses)
        {
            sb.AppendLine($"| **{arc.ClassName}** | {arc.SubjectName} | ⚠️ **{arc.AtRiskCount} bạn** | {arc.Details} |");
        }

        sb.AppendLine();
        sb.AppendLine("💡 *Thầy/Cô có thể nhắc nhở chuyên cần hoặc tạo thêm bài tập luyện tập bổ trợ trước kỳ thi kết thúc học phần nhé!*");

        reply = sb.ToString();
        return true;
    }

    private sealed class TeacherAcademicContext
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int TotalStudents { get; set; }
        public int PendingGradingCount { get; set; }
        public List<string> CourseNames { get; set; } = new();
        public List<TeacherTodayScheduleItem> TodaySchedule { get; set; } = new();
        public List<TeacherAtRiskClassItem> AtRiskClasses { get; set; } = new();
    }

    private sealed class TeacherTodayScheduleItem
    {
        public string SubjectName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string TimeRange { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
    }

    private sealed class TeacherAtRiskClassItem
    {
        public string ClassName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int AtRiskCount { get; set; }
        public string Details { get; set; } = string.Empty;
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
