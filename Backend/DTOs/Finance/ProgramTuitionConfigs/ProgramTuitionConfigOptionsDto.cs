namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public record ProgramTuitionConfigOptionDto(int Id, string Label, string? Code = null, int? ParentId = null);

public record ProgramTuitionCalculationTypeOptionDto(string Value, string Label);

public class ProgramTuitionConfigOptionsDto
{
    public IReadOnlyList<ProgramTuitionConfigOptionDto> Organizations { get; set; } = [];
    public IReadOnlyList<ProgramTuitionConfigOptionDto> TrainingPrograms { get; set; } = [];
    public IReadOnlyList<ProgramTuitionConfigOptionDto> AcademicTerms { get; set; } = [];
    public IReadOnlyList<ProgramTuitionCalculationTypeOptionDto> CalculationTypes { get; set; } = [];
}
