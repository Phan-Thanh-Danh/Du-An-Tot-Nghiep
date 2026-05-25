namespace Backend.DTOs.TrainingProgramSubjects;

public class TrainingProgramSubjectDto
{
    public int MaChuongTrinhMonHoc { get; set; }
    public int MaChuongTrinh { get; set; }
    public string MaCodeChuongTrinh { get; set; } = string.Empty;
    public string TenChuongTrinh { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int HocKyDuKien { get; set; }
    public int SemesterNumber { get; set; }
    public int SoTinChi { get; set; }
    public string LoaiMonHoc { get; set; } = string.Empty;
    public bool BatBuoc { get; set; }
    public int ThuTu { get; set; }
    public string? GhiChu { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
