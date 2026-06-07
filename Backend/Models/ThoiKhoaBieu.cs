namespace Backend.Models;

// Trong MVP, KhoaHoc là sự kết hợp giữa Lớp hành chính, Môn học, Giảng viên, Học kỳ và Cơ sở.
// Thời khóa biểu là lịch cố định hằng tuần của một KhoaHoc, gồm Thứ trong tuần, Ca học và Phòng học.
// Hệ thống không nhập giờ học tự do trong TKB mà chọn từ danh mục CaHoc.
// Sau khi xuất bản TKB, hệ thống sinh các BuoiHoc cụ thể theo từng ngày để phục vụ xem lịch và điểm danh.
// Các phát sinh như dạy thay, đổi phòng, dời ca hoặc hủy buổi được xử lý trên từng BuoiHoc, không làm sai lịch cố định toàn kỳ.
public class ThoiKhoaBieu
{
    public int MaTkb { get; set; }
    public int MaKhoaHoc { get; set; }
    public int MaPhong { get; set; }
    public int MaCaHoc { get; set; }
    public int ThuTrongTuan { get; set; }
    public DateOnly? NgayBatDau { get; set; }
    public DateOnly? NgayKetThuc { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public KhoaHoc? KhoaHoc { get; set; }
    public PhongHoc? Phong { get; set; }
    public CaHoc? CaHoc { get; set; }
}
