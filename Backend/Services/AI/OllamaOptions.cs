namespace Backend.Services.AI;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "qwen3.5:9b-q4_K_M";
    public string EmbeddingModel { get; set; } = "qwen3-embedding:0.6b";
    public int ContextLength { get; set; } = 4096;
    public int MaxOutputTokens { get; set; } = 2048;
    public int TimeoutSeconds { get; set; } = 180;
    public int MaxConcurrentChatRequests { get; set; } = 1;
    public int MaxQueueSize { get; set; } = 10;
}
