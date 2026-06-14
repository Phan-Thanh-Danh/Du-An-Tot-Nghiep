using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Finance.TuitionPayments;

public class CreateTuitionPaymentRequest
{
    [Required(ErrorMessage = "Nhà cung cấp thanh toán không được để trống.")]
    public string Provider { get; set; } = string.Empty;
}

public class CreateTuitionPaymentResponse
{
    public int MaGiaoDich { get; set; }
    public int MaHoaDon { get; set; }
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string MaThamChieuNoiBo { get; set; } = string.Empty;
    public string NoiDungChuyenKhoan { get; set; } = string.Empty;
    public string? QrUrl { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrPayload { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

public class StudentTuitionInvoiceDto
{
    public int MaHoaDon { get; set; }
    public string MaHoaDonCode { get; set; } = string.Empty;
    public string HocKy { get; set; } = string.Empty;
    public decimal SoTien { get; set; }
    public decimal GiamTru { get; set; }
    public decimal DaThanhToan { get; set; }
    public decimal SoTienPhaiDong { get; set; }
    public decimal ConPhaiDong { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateOnly HanThanhToan { get; set; }
}

public class StudentTuitionTransactionDto
{
    public int MaGiaoDich { get; set; }
    public string? MaThamChieuNoiBo { get; set; }
    public decimal SoTien { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? NhaCungCapThanhToan { get; set; }
    public string? NoiDungChuyenKhoan { get; set; }
    public DateTime NgayTao { get; set; }
    public DateTime? NgayThanhToan { get; set; }
}

public class PayOsWebhookResultDto
{
    public bool Processed { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
