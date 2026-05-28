namespace Backend.DTOs.TrainingProgramTerms;

public class TrainingProgramTermDto
{
    public int MaChuongTrinhHocKy { get; set; }
    public int MaChuongTrinh { get; set; }
    public string TenChuongTrinh { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string MaCodeHocKy { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public string NamHoc { get; set; } = string.Empty;
    public int ThuTuTrongNam { get; set; }
    public int ThuTuHocKy { get; set; }
    public DateOnly NgayBatDau { get; set; }
    public DateOnly NgayKetThuc { get; set; }
}
