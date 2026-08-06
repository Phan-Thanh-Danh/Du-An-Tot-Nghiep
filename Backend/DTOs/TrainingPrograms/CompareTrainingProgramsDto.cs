namespace Backend.DTOs.TrainingPrograms;

public class CompareTrainingProgramsDto
{
    public TrainingProgramDto SourceProgram { get; set; } = null!;
    public TrainingProgramDto TargetProgram { get; set; } = null!;
    
    public List<CurriculumSubjectDiffDto> Differences { get; set; } = [];
    public int TotalAdded { get; set; }
    public int TotalRemoved { get; set; }
    public int TotalModified { get; set; }
    public int TotalShifted { get; set; }
    public int TotalUnchanged { get; set; }
}

public class CurriculumSubjectDiffDto
{
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int? SourceHocKy { get; set; }
    public int? TargetHocKy { get; set; }
    public int? SourceTinChi { get; set; }
    public int? TargetTinChi { get; set; }
    public string? SourceLoaiMon { get; set; }
    public string? TargetLoaiMon { get; set; }
    public string DiffType { get; set; } = string.Empty; // "added", "removed", "shifted", "modified", "unchanged"
}
