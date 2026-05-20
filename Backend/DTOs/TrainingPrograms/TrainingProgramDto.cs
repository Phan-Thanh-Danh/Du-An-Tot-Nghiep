namespace Backend.DTOs.TrainingPrograms;

public class TrainingProgramDto
{
    public int MaChuongTrinh { get; set; }
    public int MaChuyenNganh { get; set; }
    public string MaCodeChuyenNganh { get; set; } = string.Empty;
    public string TenChuyenNganh { get; set; } = string.Empty;
    public int MaNganh { get; set; }
    public string TenNganh { get; set; } = string.Empty;
    public int MaKhoaTuyenSinh { get; set; }
    public string MaCodeKhoa { get; set; } = string.Empty;
    public string TenKhoa { get; set; } = string.Empty;
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
    public string? TenNguonChuongTrinh { get; set; }
    public string? GhiChuThayDoi { get; set; }
    public DateOnly? NgayHieuLuc { get; set; }
    public DateOnly? NgayHetHieuLuc { get; set; }
    public int? NguoiGuiDuyetId { get; set; }
    public string? TenNguoiGuiDuyet { get; set; }
    public DateTime? ThoiGianGuiDuyet { get; set; }
    public int? NguoiDuyetId { get; set; }
    public string? TenNguoiDuyet { get; set; }
    public DateTime? ThoiGianDuyet { get; set; }
    public string? GhiChuDuyet { get; set; }
    public int? NguoiTuChoiId { get; set; }
    public string? TenNguoiTuChoi { get; set; }
    public DateTime? ThoiGianTuChoi { get; set; }
    public string? LyDoTuChoi { get; set; }
    public bool ConHoatDong { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
