namespace Backend.Models;

// Trong module học phí, HoaDon đại diện cho khoản học phí phải đóng của học sinh theo học kỳ.
// GiaoDich là sổ lịch sử công nợ, gồm dòng phát sinh học phí số âm và dòng thanh toán số dương.
// Tài khoản nhận tiền được cấu hình theo cơ sở và phải được duyệt trước khi sử dụng.
// Khi học sinh thanh toán, hệ thống tạo giao dịch QR qua provider như payOS/VietQR, lưu tài khoản
// nhận tiền đã dùng và chỉ cập nhật hóa đơn khi webhook hợp lệ được xác nhận ở backend.
public class GiaoDich
{
    public int MaGiaoDich { get; set; }

    public int MaHoaDon { get; set; }
    public int? MaTaiKhoanNhanTien { get; set; }

    public string? MaThamChieuNoiBo { get; set; }
    public string? MaThamChieuCong { get; set; }

    public decimal SoTien { get; set; }

    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;

    public string? NhaCungCapThanhToan { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }

    public string? QrPayload { get; set; }
    public string? QrUrl { get; set; }
    public string? CheckoutUrl { get; set; }

    public string? RequestPayloadJson { get; set; }
    public string? ResponsePayloadJson { get; set; }
    public string? CallbackPayloadJson { get; set; }

    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public DateTime? NgayHetHan { get; set; }
    public DateTime? NgayThanhToan { get; set; }

    public int? MaNguoiThucHien { get; set; }

    public string? ChuThich { get; set; }

    public HoaDon? HoaDon { get; set; }
    public TaiKhoanNhanTien? TaiKhoanNhanTien { get; set; }
    public NguoiDung? NguoiThucHien { get; set; }
}
