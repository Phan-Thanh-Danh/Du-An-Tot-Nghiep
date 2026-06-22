namespace Backend.Models;

public class TienDoNoiDungHocTap
{
    public int MaTienDoNoiDung { get; set; }
    public int MaHocSinh { get; set; }
    public int MaNoiDung { get; set; }
    public string LoaiNoiDung { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "chua_bat_dau"; // chua_bat_dau, dang_hoc, hoan_thanh
    public decimal PhanTramTienDo { get; set; }
    public int SoGiayDaXacNhan { get; set; }
    public int? ViTriVideoCuoiGiay { get; set; }
    public decimal? PhanTramCuonLonNhat { get; set; }
    public int? ChiSoMucCuoi { get; set; }
    public int? SoMucDaXem { get; set; }
    public int? TongSoMuc { get; set; }
    public DateTime? BatDauLuc { get; set; }
    public DateTime? LanTuongTacCuoi { get; set; }
    public DateTime? HoanThanhLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public string? ChiTietTienDoJson { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public BaiHocNoiDung? NoiDung { get; set; }
}
