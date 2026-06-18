using Backend.Constants;
using Backend.DTOs.Common;
using Backend.DTOs.Finance.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/finance/schema")]
[Authorize(Roles = FinanceConstants.FinanceAuthorizationRoles.SchemaReaders)]
public class FinanceSchemaController : ControllerBase
{
    [HttpGet("options")]
    public ActionResult<ApiResponseDto<FinanceSchemaOptionsDto>> GetOptions()
    {
        return Ok(ApiResponseDto<FinanceSchemaOptionsDto>.Ok(
            CreateOptions(),
            "Lấy cấu hình schema tài chính thành công."));
    }

    [HttpGet("statuses")]
    public ActionResult<ApiResponseDto<FinanceSchemaStatusesDto>> GetStatuses()
    {
        return Ok(ApiResponseDto<FinanceSchemaStatusesDto>.Ok(
            new FinanceSchemaStatusesDto
            {
                InvoiceStatuses = InvoiceStatuses,
                TransactionStatuses = TransactionStatuses,
                PaymentAccountApprovalStatuses = PaymentAccountApprovalStatuses,
                RefundRequestStatuses = RefundRequestStatuses
            },
            "Lấy danh sách trạng thái tài chính thành công."));
    }

    private static FinanceSchemaOptionsDto CreateOptions()
    {
        return new FinanceSchemaOptionsDto
        {
            InvoiceTypes = InvoiceTypes,
            InvoiceStatuses = InvoiceStatuses,
            TransactionTypes = TransactionTypes,
            TransactionStatuses = TransactionStatuses,
            PaymentProviders = PaymentProviders,
            PaymentAccountApprovalStatuses = PaymentAccountApprovalStatuses,
            TuitionCalculationTypes = TuitionCalculationTypes,
            RefundTypes = RefundTypes,
            RefundRequestStatuses = RefundRequestStatuses,
            Formulas = Formulas,
            InvoiceStatusRules = InvoiceStatusRules
        };
    }

    private static readonly FinanceSchemaOptionDto[] InvoiceTypes =
    [
        new(FinanceConstants.InvoiceTypes.Tuition, "Học phí"),
        new(FinanceConstants.InvoiceTypes.Fee, "Lệ phí"),
        new(FinanceConstants.InvoiceTypes.Material, "Tài liệu"),
        new(FinanceConstants.InvoiceTypes.Other, "Khác")
    ];

    private static readonly FinanceSchemaOptionDto[] InvoiceStatuses =
    [
        new(FinanceConstants.InvoiceStatuses.Unpaid, "Chưa thanh toán"),
        new(FinanceConstants.InvoiceStatuses.PartiallyPaid, "Thanh toán một phần"),
        new(FinanceConstants.InvoiceStatuses.Paid, "Đã thanh toán"),
        new(FinanceConstants.InvoiceStatuses.Overdue, "Quá hạn"),
        new(FinanceConstants.InvoiceStatuses.Canceled, "Đã hủy")
    ];

    private static readonly FinanceSchemaOptionDto[] TransactionTypes =
    [
        new(FinanceConstants.TransactionTypes.TuitionCharge, "Phát sinh học phí"),
        new(FinanceConstants.TransactionTypes.TuitionPayment, "Thanh toán học phí"),
        new(FinanceConstants.TransactionTypes.DebtAdjustment, "Điều chỉnh công nợ"),
        new(FinanceConstants.TransactionTypes.Refund, "Hoàn tiền"),
        new(FinanceConstants.TransactionTypes.InvoiceCancellation, "Hủy hóa đơn")
    ];

    private static readonly FinanceSchemaOptionDto[] TransactionStatuses =
    [
        new(FinanceConstants.TransactionStatuses.Created, "Phát sinh"),
        new(FinanceConstants.TransactionStatuses.PendingPayment, "Chờ thanh toán"),
        new(FinanceConstants.TransactionStatuses.Processing, "Đang xử lý"),
        new(FinanceConstants.TransactionStatuses.Success, "Thành công"),
        new(FinanceConstants.TransactionStatuses.Failed, "Thất bại"),
        new(FinanceConstants.TransactionStatuses.Expired, "Hết hạn"),
        new(FinanceConstants.TransactionStatuses.Canceled, "Đã hủy"),
        new(FinanceConstants.TransactionStatuses.WrongAmount, "Sai số tiền"),
        new(FinanceConstants.TransactionStatuses.ManualReview, "Chờ xử lý thủ công")
    ];

    private static readonly FinanceSchemaOptionDto[] PaymentProviders =
    [
        new(FinanceConstants.PaymentProviders.VietQr, "VietQR"),
        new(FinanceConstants.PaymentProviders.PayOs, "PayOS")
    ];

    private static readonly FinanceSchemaOptionDto[] PaymentAccountApprovalStatuses =
    [
        new(FinanceConstants.PaymentAccountApprovalStatuses.Draft, "Nháp"),
        new(FinanceConstants.PaymentAccountApprovalStatuses.PendingApproval, "Chờ duyệt"),
        new(FinanceConstants.PaymentAccountApprovalStatuses.Approved, "Đã duyệt"),
        new(FinanceConstants.PaymentAccountApprovalStatuses.Rejected, "Từ chối"),
        new(FinanceConstants.PaymentAccountApprovalStatuses.Inactive, "Ngừng hoạt động")
    ];

    private static readonly FinanceSchemaOptionDto[] TuitionCalculationTypes =
    [
        new(FinanceConstants.TuitionCalculationTypes.FixedPerTerm, "Cố định theo học kỳ"),
        new(FinanceConstants.TuitionCalculationTypes.PerCredit, "Theo tín chỉ"),
        new(FinanceConstants.TuitionCalculationTypes.PerSubject, "Theo môn học")
    ];

    private static readonly FinanceSchemaOptionDto[] RefundTypes =
    [
        new(FinanceConstants.RefundTypes.Full, "Toàn phần"),
        new(FinanceConstants.RefundTypes.Partial, "Một phần"),
        new(FinanceConstants.RefundTypes.Credit, "Ghi có")
    ];

    private static readonly FinanceSchemaOptionDto[] RefundRequestStatuses =
    [
        new(FinanceConstants.RefundRequestStatuses.PendingApproval, "Chờ duyệt"),
        new(FinanceConstants.RefundRequestStatuses.Approved, "Đã duyệt"),
        new(FinanceConstants.RefundRequestStatuses.Rejected, "Từ chối"),
        new(FinanceConstants.RefundRequestStatuses.Processed, "Đã xử lý")
    ];

    private static readonly FinanceFormulaDto[] Formulas =
    [
        new(
            "tongTienDuKien",
            "SoTienHocPhi + TienHocLieu",
            "Tổng tiền dự kiến của cấu hình học phí chương trình."),
        new(
            "hoaDonSoTien",
            "HoaDon.SoTien = TongTienDuKien",
            "Số tiền hóa đơn học phí chính lấy từ tổng tiền dự kiến."),
        new(
            "soTienPhaiDong",
            "MAX(0, HoaDon.SoTien - HoaDon.GiamTru)",
            "Số tiền học sinh cần đóng sau giảm trừ."),
        new(
            "daThanhToan",
            "SUM(GiaoDich.SoTien) WHERE LoaiGiaoDich = 'thanh_toan_hoc_phi' AND TrangThai = 'thanh_cong'",
            "Tổng giao dịch thanh toán thành công đã áp dụng vào hóa đơn."),
        new(
            "conPhaiDong",
            "MAX(0, SoTienPhaiDong - HoaDon.DaThanhToan)",
            "Số tiền còn phải đóng.")
    ];

    private static readonly string[] InvoiceStatusRules =
    [
        "Hóa đơn bị hủy luôn có trạng thái da_huy.",
        "DaThanhToan >= SoTienPhaiDong thì trạng thái là da_thanh_toan.",
        "DaThanhToan = 0 và chưa quá hạn thì trạng thái là chua_thanh_toan.",
        "DaThanhToan = 0 và đã quá hạn thì trạng thái là qua_han.",
        "0 < DaThanhToan < SoTienPhaiDong và chưa quá hạn thì trạng thái là thanh_toan_mot_phan.",
        "0 < DaThanhToan < SoTienPhaiDong và đã quá hạn thì trạng thái là qua_han."
    ];
}
