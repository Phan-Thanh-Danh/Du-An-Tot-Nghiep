namespace Backend.Models;

// Trong phạm vi MVP, KhoaHoc là một môn học được mở cho một lớp hành chính trong một học kỳ và do một giảng viên phụ trách.
// LopHocPhan tạm thời chưa dùng cho khóa học MVP và được giữ nullable để mở rộng đăng ký học phần sau này.
public class KhoaHoc
{
    public int MaKhoaHoc { get; set; }
    public int MaDonVi { get; set; }
    public int MaGiaoVien { get; set; }
    public int MaMonHoc { get; set; }
    public int? MaHocKy { get; set; }
    public int MaLop { get; set; }
    public int? MaLopHocPhan { get; set; }
    public string TieuDe { get; set; } = string.Empty;
    public string? MoTa { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string? UrlAnhBia { get; set; }
    public DateTime NgayTao { get; set; }

    public DonVi? DonVi { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public DanhMucMonHoc? MonHoc { get; set; }
    public HocKy? HocKy { get; set; }
    public LopHanhChinh? Lop { get; set; }
    public LopHocPhan? LopHocPhan { get; set; }
}
