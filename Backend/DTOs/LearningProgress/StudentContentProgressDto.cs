namespace Backend.DTOs.LearningProgress;

public class StudentContentProgressDto
{
    public int MaNoiDung { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public decimal PhanTramTienDo { get; set; }
    public int? ViTriVideoCuoiGiay { get; set; }
    public DateTime? LanTuongTacCuoi { get; set; }
    public DateTime? HoanThanhLuc { get; set; }
}
