namespace Backend.DTOs.TeacherPersonnel;

public class TeacherPersonnelListDto
{
    public int MaNguoiDung { get; set; }
    public string MaGiangVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SoDienThoai { get; set; }
    public int MaDonVi { get; set; }
    public string TenDonVi { get; set; } = string.Empty;
    public string TrangThai { get; set; } = "hoat_dong"; // hoat_dong, bi_khoa, tam_nghi
    public string ChuyenNganhChinh { get; set; } = string.Empty;
    public int SoMonDuocPhepDay { get; set; }
    public int SoLopHocKyHienTai { get; set; }
    public int SoCaMoiTuan { get; set; }
    public decimal DiemDanhGiaTrungBinh { get; set; }
    public DateTime NgayTao { get; set; }
}

public class TeacherPersonnelQueryParameters
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Keyword { get; set; }
    public int? MaDonVi { get; set; }
    public int? MaChuyenNganh { get; set; }
    public int? MaMonHoc { get; set; }
    public string? TrangThai { get; set; }
    public int? MaHocKy { get; set; }
}
