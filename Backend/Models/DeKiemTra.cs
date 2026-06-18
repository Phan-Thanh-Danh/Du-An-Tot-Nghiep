namespace Backend.Models;

public class DeKiemTra
{
    public int MaDeKiemTra { get; set; }
    public int? MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public int ThoiGianPhut { get; set; }
    public string CauHinhDeThi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;

    // Exam module extensions
    public string? LoaiDeThi { get; set; }
    public string? HinhThucThi { get; set; }
    public decimal? TyLeTracNghiem { get; set; }
    public decimal? TyLeTuLuan { get; set; }
    public int? MaNguoiSoan { get; set; }
    public int? MaNguoiDuyet { get; set; }
    public string? TrangThaiDuyet { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
    public NguoiDung? NguoiSoan { get; set; }
    public NguoiDung? NguoiDuyet { get; set; }
}
