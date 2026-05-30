namespace Backend.DTOs.Finance.ProgramTuitionConfigs;

public class ProgramTuitionConfigDetailDto
{
    public int Id { get; set; }
    public int MaCauHinhHocPhi => Id;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int MaChuongTrinhDaoTao { get; set; }
    public string MaCodeChuongTrinh { get; set; } = string.Empty;
    public string TenChuongTrinh { get; set; } = string.Empty;
    public int MaHocKy { get; set; }
    public string MaCodeHocKy { get; set; } = string.Empty;
    public string TenHocKy { get; set; } = string.Empty;
    public int NamHocTrongChuongTrinh { get; set; }
    public int HocKyTrongNam { get; set; }
    public int SoThuTuHocKy { get; set; }
    public string LoaiCachTinhHocPhi { get; set; } = string.Empty;
    public decimal SoTienHocPhi { get; set; }
    public decimal TienHocLieu { get; set; }
    public decimal TongTienDuKien { get; set; }
    public bool ConHoatDong { get; set; }
    public bool CoDuocSua { get; set; }
    public string? LyDoKhongDuocSua { get; set; }
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
