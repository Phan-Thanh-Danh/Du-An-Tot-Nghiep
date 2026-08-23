using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.AI;

public class AiChatRequest
{
    [Required(ErrorMessage = "Nội dung câu hỏi không được để trống.")]
    [MaxLength(2000, ErrorMessage = "Câu hỏi không được vượt quá 2000 ký tự.")]
    public string Message { get; set; } = string.Empty;

    public string? ConversationId { get; set; }
    public int? CourseId { get; set; }
    public int? LessonId { get; set; }
}

public class AiChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public string? Thinking { get; set; }
    public long? ProcessingTimeMs { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<string> Sources { get; set; } = new();
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
