namespace Backend.DTOs.Applications;

public class WorkflowConfigDto
{
    public int Id { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string? Sla { get; set; }
    public int Steps { get; set; }
    public List<WorkflowStepDto> WfSteps { get; set; } = new();
}

public class WorkflowStepDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? Type { get; set; }
    public string? Sla { get; set; }
    public int Order { get; set; }
}

public class UpdateWorkflowRequest
{
    public bool Active { get; set; }
}
