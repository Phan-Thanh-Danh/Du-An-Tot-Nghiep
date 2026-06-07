namespace Backend.Models;

// Trong MVP, điểm danh được quản lý theo từng BuoiHoc. Khi mở điểm danh, hệ thống tạo danh sách DiemDanh
// mặc định tất cả học sinh là vắng mặt; giảng viên chuyển sang có mặt, đi muộn hoặc có phép khi cần.
// Mỗi buổi học có 15 phút đầu giờ để điểm danh, sau khi gửi có 10 phút chỉnh sửa, rồi hệ thống tự khóa.
// Sau khi khóa, muốn sửa phải gửi yêu cầu mở khóa cho Admin/Giáo vụ duyệt.
public class BuoiHoc
{
    public int MaBuoiHoc { get; set; }
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public DateOnly NgayHoc { get; set; }
    public int MaCaHoc { get; set; }
    public int MaPhong { get; set; }
    public int MaGiaoVien { get; set; }
    public int? MaGiaoVienDayThay { get; set; }
    public string TrangThaiBuoi { get; set; } = string.Empty;
    public string? LoaiThayDoi { get; set; }
    public string? LyDoThayDoi { get; set; }
    public string? GhiChu { get; set; }
    public DateTime? KhoaLuc { get; set; }
    public DateTime? DiemDanhBatDauLuc { get; set; }
    public DateTime? DiemDanhHanGuiLuc { get; set; }
    public DateTime? DiemDanhDaGuiLuc { get; set; }
    public DateTime? DiemDanhHanChinhSuaLuc { get; set; }
    public DateTime? DiemDanhKhoaLuc { get; set; }
    public string TrangThaiDiemDanh { get; set; } = "chua_mo";
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public ThoiKhoaBieu? Tkb { get; set; }
    public KhoaHoc? KhoaHoc { get; set; }
    public CaHoc? CaHoc { get; set; }
    public PhongHoc? Phong { get; set; }
    public NguoiDung? GiaoVien { get; set; }
    public NguoiDung? GiaoVienDayThay { get; set; }
}
