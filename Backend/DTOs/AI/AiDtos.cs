using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AI;

public class AiChatRequest
{
    [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
    [MaxLength(2000, ErrorMessage = "Câu hỏi không được vượt quá 2000 ký tự.")]
    public string Message { get; set; } = string.Empty;

    public string? ConversationId { get; set; }
    [MaxLength(8)]
    public List<AiConversationTurn> History { get; set; } = new();
    public int? CourseId { get; set; }
    public int? LessonId { get; set; }

    /// <summary>
    /// Chế độ suy luận: "fast" (qwen2.5:3b) hoặc "deep" (qwen3.5:9b-q4_K_M)
    /// </summary>
    public string Mode { get; set; } = "fast";

    /// <summary>
    /// Kích hoạt tìm kiếm văn bản quy chế học vụ qua RAG Vector Embedding
    /// </summary>
    public bool UseRag { get; set; } = false;
}

public class AiConversationTurn
{
    [Required, RegularExpression("^(user|assistant)$")]
    public string Role { get; set; } = "user";
    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public class AiChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public string? Thinking { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();

    /// <summary>
    /// Hành động thực thi (nếu AI kích hoạt tạo đề, phân tích,...)
    /// </summary>
    public AiChatActionDto? Action { get; set; }
}

public class AiChatActionDto
{
    public string ActionType { get; set; } = string.Empty; // "create_quiz", "view_report", "schedule_alert", "create_ticket", "download_quiz"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "completed"; // "completed", "pending"
    public string? ActionUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class AiHealthResponse
{
    public bool Available { get; set; }
    public string ChatModel { get; set; } = string.Empty;
    public bool ChatModelAvailable { get; set; }
    public string EmbeddingModel { get; set; } = string.Empty;
    public bool EmbeddingModelAvailable { get; set; }
    public long LatencyMs { get; set; }
    public int QueueLength { get; set; }
}

public class AiEmbeddingTestRequest
{
    [Required(ErrorMessage = "Đoạn văn bản không được để trống.")]
    [MaxLength(4000, ErrorMessage = "Đoạn văn bản không được vượt quá 4000 ký tự.")]
    public string Text { get; set; } = string.Empty;
}

public class AiEmbeddingTestResponse
{
    public string Model { get; set; } = string.Empty;
    public int Dimensions { get; set; }
    public bool Success { get; set; }
}
