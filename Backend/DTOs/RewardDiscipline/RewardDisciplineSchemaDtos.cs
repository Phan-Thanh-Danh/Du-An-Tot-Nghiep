namespace Backend.DTOs.RewardDiscipline;

public record RewardDisciplineOptionDto(string Value, string Label);

public class RewardDisciplineSchemaOptionsDto
{
    public IReadOnlyList<RewardDisciplineOptionDto> RewardCampaignTypes { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> RewardCampaignStatuses { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> CertificateTemplateTypes { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> PaperOrientations { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> RewardTypes { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> RewardStatuses { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> DisciplineLevels { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> DisciplineStatuses { get; init; } = [];
    public IReadOnlyList<RewardDisciplineOptionDto> DisciplineActions { get; init; } = [];
}
