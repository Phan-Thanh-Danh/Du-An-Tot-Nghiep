namespace Backend.Models;

public class HoSoKyLuat
{
    public int MaKyLuat { get; set; }
    public int MaHocSinh { get; set; }
    public int MaDonVi { get; set; }
    public int? MaHocKy { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string LoaiKyLuat { get; set; } = string.Empty;
    public string MucDoViPham { get; set; } = string.Empty;
    public string HinhThucXuLy { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string MoTa { get; set; } = string.Empty;
    public DateOnly NgayViPham { get; set; }
    public string? CanCuXuLy { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public string? LyDoHuy { get; set; }
    public int? NguoiHuy { get; set; }
    public DateTime? NgayHuy { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
    public int NguoiTao { get; set; }
    public DateTime NgayTao { get; set; }
    public int? NguoiDuyet { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? LyDoTuChoi { get; set; }
    public string? ChungTuJson { get; set; }
    public bool DaGoKyLuat { get; set; }
    public string? LyDoGoKyLuat { get; set; }
    public int? NguoiGoKyLuat { get; set; }
    public DateTime? NgayGoKyLuat { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? LoaiDoiTuongLienKet { get; set; }
    public int? MaDoiTuongLienKet { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public DonVi? DonVi { get; set; }
    public HocKy? HocKy { get; set; }
    public NguoiDung? NguoiTaoNavigation { get; set; }
    public NguoiDung? NguoiDuyetNavigation { get; set; }
    public NguoiDung? NguoiGoKyLuatNavigation { get; set; }
    public NguoiDung? NguoiHuyNavigation { get; set; }
}
