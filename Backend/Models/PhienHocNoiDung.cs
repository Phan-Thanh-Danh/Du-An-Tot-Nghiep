namespace Backend.Models;

public class PhienHocNoiDung
{
    public long MaPhienHoc { get; set; }
    public Guid SessionToken { get; set; }
    public int MaHocSinh { get; set; }
    public int MaNoiDung { get; set; }
    public DateTime BatDauLuc { get; set; }
    public DateTime? NhipTimCuoiLuc { get; set; }
    public DateTime? KetThucLuc { get; set; }
    public int SoGiayHoatDongDaXacNhan { get; set; }
    public int? ViTriVideoCuoiGiay { get; set; }
    public decimal? PhanTramCuonLonNhat { get; set; }
    public int SoThuTuNhipTimCuoi { get; set; }
    public string TrangThai { get; set; } = "dang_hoat_dong"; // dang_hoat_dong, da_ket_thuc, het_han, bi_thay_the
    public string? UserAgentHash { get; set; }
    public string? DiaChiIpHash { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? HocSinh { get; set; }
    public BaiHocNoiDung? NoiDung { get; set; }
}
