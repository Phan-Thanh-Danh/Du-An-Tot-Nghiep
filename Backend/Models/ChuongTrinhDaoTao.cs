namespace Backend.Models;

public class ChuongTrinhDaoTao
{
    public int MaChuongTrinh { get; set; }
    public int MaChuyenNganh { get; set; }
    public int MaKhoaTuyenSinh { get; set; }
    public string MaCodeChuongTrinh { get; set; } = string.Empty;
    public string TenChuongTrinh { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int SoHocKy { get; set; }
    public int ThoiGianDaoTaoThang { get; set; }
    public int? TongTinChiYeuCau { get; set; }
    public int? SoTinChiToiThieuMoiKy { get; set; }
    public int? SoTinChiToiDaMoiKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public int? NguonChuongTrinhId { get; set; }
    public string? GhiChuThayDoi { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
    public int? NguoiGuiDuyetId { get; set; }
    public DateTime? ThoiGianGuiDuyet { get; set; }
    public int? NguoiDuyetId { get; set; }
    public DateTime? ThoiGianDuyet { get; set; }
    public string? GhiChuDuyet { get; set; }
    public int? NguoiTuChoiId { get; set; }
    public DateTime? ThoiGianTuChoi { get; set; }
    public string? LyDoTuChoi { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public ChuyenNganh? ChuyenNganh { get; set; }
    public KhoaTuyenSinh? KhoaTuyenSinh { get; set; }
    public ChuongTrinhDaoTao? NguonChuongTrinh { get; set; }
    public ICollection<ChuongTrinhDaoTao> ChuongTrinhPhienBanCon { get; set; } = new List<ChuongTrinhDaoTao>();
    public ICollection<LopHanhChinh> LopHanhChinhs { get; set; } = new List<LopHanhChinh>();
}
