namespace Backend.Models;

public class DonTu
{
    public int MaDonTu { get; set; }
    public int MaDonVi { get; set; }
    public int MaHocSinh { get; set; }
    public int? MaMauDon { get; set; }
    public string LoaiDon { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string TrangThaiXuLyNghiepVu { get; set; } = string.Empty;
    public int? NguoiDuyetHienTai { get; set; }
    public int? NguoiXuLyCuoi { get; set; }
    public string? DuLieuBieuMau { get; set; }
    public string? UrlBangChung { get; set; }
    public string? LyDoTuChoi { get; set; }
    public string? NoiDungYeuCauBoSung { get; set; }
    public string? KetQuaXuLyJson { get; set; }
    public string? NhatKyTuDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public DateTime? NgayNop { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public DateTime? HanXuLyLuc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public DonVi? DonVi { get; set; }
    public NguoiDung? HocSinh { get; set; }
    public MauDonTu? MauDon { get; set; }
    public NguoiDung? NguoiDuyetHienTaiNavigation { get; set; }
    public NguoiDung? NguoiXuLyCuoiNavigation { get; set; }
}
