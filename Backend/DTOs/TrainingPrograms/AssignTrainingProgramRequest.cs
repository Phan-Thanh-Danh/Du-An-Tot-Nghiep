namespace Backend.DTOs.TrainingPrograms;

public class AssignTrainingProgramRequest
{
    public List<int> MaKhoaTuyenSinhIds { get; set; } = [];
    public List<int> MaDonViIds { get; set; } = [];
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
}
