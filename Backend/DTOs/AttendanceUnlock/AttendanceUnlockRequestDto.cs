namespace Backend.DTOs.AttendanceUnlock;

public class AttendanceUnlockRequestDto
{
    public int MaYcMoKhoa { get; set; }
    public int MaBuoiHoc { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string TenMonHoc { get; set; } = string.Empty;
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string TenPhong { get; set; } = string.Empty;
    public int NguoiYeuCau { get; set; }
    public string TenNguoiYeuCau { get; set; } = string.Empty;
    public string LyDo { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public int? NguoiDuyet { get; set; }
    public string? TenNguoiDuyet { get; set; }
    public DateTime? MoKhoaDenLuc { get; set; }
    public DateTime NgayTao { get; set; }
    public string? GhiChu { get; set; }
    public string? LyDoTuChoi { get; set; }
    public DateTime? ThoiGianXuLy { get; set; }
}
