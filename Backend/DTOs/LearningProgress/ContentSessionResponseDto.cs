namespace Backend.DTOs.LearningProgress;

public class ContentSessionResponseDto
{
    public Guid SessionToken { get; set; }
    public string TrangThaiHienTai { get; set; } = string.Empty;
    public decimal PhanTramTienDo { get; set; }
    public int HeartbeatExpectedSeconds { get; set; }
    public int? ViTriVideoBatDau { get; set; }
}
