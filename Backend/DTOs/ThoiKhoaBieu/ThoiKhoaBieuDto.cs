namespace Backend.DTOs.ThoiKhoaBieu;

public class ThoiKhoaBieuDto
{
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public string TieuDeKhoaHoc { get; set; } = string.Empty;
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public int? MaHocKy { get; set; }
    public string? TenHocKy { get; set; }
    public int MaLop { get; set; }
    public string TenLop { get; set; } = string.Empty;
    public string MaCodeLop { get; set; } = string.Empty;
    public int MaMonHoc { get; set; }
    public string MaCodeMonHoc { get; set; } = string.Empty;
    public string TenMonHoc { get; set; } = string.Empty;
    public int MaGiaoVien { get; set; }
    public string TenGiaoVien { get; set; } = string.Empty;
    public int ThuTrongTuan { get; set; }
    public int MaCaHoc { get; set; }
    public string TenCa { get; set; } = string.Empty;
    public string Buoi { get; set; } = string.Empty;
    public string GioBatDau { get; set; } = string.Empty;
    public string GioKetThuc { get; set; } = string.Empty;
    public int MaPhong { get; set; }
    public string MaCodePhong { get; set; } = string.Empty;
    public string TenPhong { get; set; } = string.Empty;
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
}
