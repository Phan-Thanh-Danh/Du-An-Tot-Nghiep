using System;
using System.Collections.Generic;

namespace Backend.DTOs.AI;

public class AiDashboardInsightDto
{
    public string Role { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<AiInsightActionItem> ActionItems { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool Cached { get; set; }
}

public class AiInsightActionItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "info"; // info | warning | danger | success
    public string ActionPrompt { get; set; } = string.Empty;
}
