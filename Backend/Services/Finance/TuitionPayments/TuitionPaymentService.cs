using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Finance.TuitionPayments;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Finance;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Finance.TuitionPayments;

public class TuitionPaymentService : ITuitionPaymentService
{
    private const string VietQrTemplateId = "ZdyCQ5H";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ApplicationDbContext _context;
    private readonly IVietQrService _vietQrService;
    private readonly IPayOsService _payOsService;
    private readonly ILogger<TuitionPaymentService> _logger;

    public TuitionPaymentService(
        ApplicationDbContext context,
        IVietQrService vietQrService,
        IPayOsService payOsService,
        ILogger<TuitionPaymentService> logger)
    {
        _context = context;
        _vietQrService = vietQrService;
        _payOsService = payOsService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<StudentTuitionInvoiceDto>> GetStudentInvoicesAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await TrySyncPendingPayOsPaymentsForStudentAsync(currentUserId, cancellationToken);

        var invoices = await (
            from invoice in _context.HoaDons.AsNoTracking()
            join term in _context.HocKys.AsNoTracking()
                on invoice.MaHocKy equals term.MaHocKy into termJoin
            from term in termJoin.DefaultIfEmpty()
            where invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition
            orderby invoice.HanThanhToan descending, invoice.MaHoaDon descending
            select new
            {
                Invoice = invoice,
                TermName = term != null ? term.TenHocKy : null
            })
            .ToListAsync(cancellationToken);

        return invoices.Select(item =>
        {
            var totalDue = InvoiceFinanceHelper.CalculateAmountDue(item.Invoice);
            var remaining = InvoiceFinanceHelper.CalculateRemainingAmount(item.Invoice);

            return new StudentTuitionInvoiceDto
            {
                MaHoaDon = item.Invoice.MaHoaDon,
                MaHoaDonCode = item.Invoice.MaHoaDonCode,
                HocKy = string.IsNullOrWhiteSpace(item.TermName) ? "Chưa xác định học kỳ" : item.TermName,
                SoTien = item.Invoice.SoTien,
                GiamTru = item.Invoice.GiamTru,
                DaThanhToan = item.Invoice.DaThanhToan,
                SoTienPhaiDong = totalDue,
                ConPhaiDong = remaining,
                TrangThai = item.Invoice.TrangThai,
                HanThanhToan = item.Invoice.HanThanhToan
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<StudentTuitionTransactionDto>> GetStudentTransactionsAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await TrySyncPendingPayOsPaymentsForStudentAsync(currentUserId, cancellationToken);

        var transactions = await (
            from transaction in _context.GiaoDichs.AsNoTracking()
            join invoice in _context.HoaDons.AsNoTracking()
                on transaction.MaHoaDon equals invoice.MaHoaDon
            where invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition
            orderby transaction.NgayTao descending, transaction.MaGiaoDich descending
            select new StudentTuitionTransactionDto
            {
                MaGiaoDich = transaction.MaGiaoDich,
                MaThamChieuNoiBo = transaction.MaThamChieuNoiBo,
                SoTien = transaction.SoTien,
                LoaiGiaoDich = transaction.LoaiGiaoDich,
                TrangThai = transaction.TrangThai,
                NhaCungCapThanhToan = transaction.NhaCungCapThanhToan,
                NoiDungChuyenKhoan = transaction.NoiDungChuyenKhoan,
                NgayTao = transaction.NgayTao,
                NgayThanhToan = transaction.NgayThanhToan
            })
            .ToListAsync(cancellationToken);

        return transactions;
    }

    public async Task<CreateTuitionPaymentResponse> CreatePaymentAsync(
        int invoiceId,
        int currentUserId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var normalizedProvider = NormalizeProvider(provider);
        var invoice = await _context.HoaDons
            .FirstOrDefaultAsync(x =>
                x.MaHoaDon == invoiceId &&
                x.MaHocSinh == currentUserId &&
                x.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hóa đơn học phí.");
        }

        if (invoice.TrangThai == FinanceConstants.InvoiceStatuses.Canceled)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hóa đơn đã hủy, không thể thanh toán.");
        }

        var remaining = InvoiceFinanceHelper.CalculateRemainingAmount(invoice);
        if (remaining <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hóa đơn đã được thanh toán đủ.");
        }

        var reusablePayment = await GetReusablePendingPaymentAsync(
            invoice.MaHoaDon,
            invoice.MaDonVi,
            normalizedProvider,
            remaining,
            cancellationToken);
        if (reusablePayment is not null)
        {
            return ToPaymentResponse(reusablePayment);
        }

        return normalizedProvider switch
        {
            FinanceConstants.PaymentProviders.VietQr => await CreateVietQrPaymentAsync(
                invoice,
                await GetReceivingAccountAsync(invoice.MaDonVi, normalizedProvider, cancellationToken),
                currentUserId,
                remaining,
                await CreateUniqueInternalReferenceAsync(cancellationToken),
                cancellationToken),
            FinanceConstants.PaymentProviders.PayOs => await CreatePayOsPaymentAsync(
                invoice,
                currentUserId,
                remaining,
                cancellationToken),
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Nhà cung cấp thanh toán không hợp lệ.")
        };
    }

    public async Task<CreateTuitionPaymentResponse> GetPaymentAsync(
        int transactionId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var existingPayment = await (
            from payment in _context.GiaoDichs.AsNoTracking()
            join invoice in _context.HoaDons.AsNoTracking()
                on payment.MaHoaDon equals invoice.MaHoaDon
            where payment.MaGiaoDich == transactionId
                && invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition
            select new
            {
                payment.MaGiaoDich,
                payment.NhaCungCapThanhToan
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPayment is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy giao dịch thanh toán.");
        }

        await TrySyncPayOsPaymentAsync(existingPayment.MaGiaoDich, cancellationToken);

        var transaction = await (
            from payment in _context.GiaoDichs.AsNoTracking()
            join invoice in _context.HoaDons.AsNoTracking()
                on payment.MaHoaDon equals invoice.MaHoaDon
            where payment.MaGiaoDich == transactionId
                && invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition
            select payment)
            .FirstOrDefaultAsync(cancellationToken);

        if (transaction is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy giao dịch thanh toán.");
        }

        return ToPaymentResponse(transaction);
    }

    public async Task<PayOsWebhookResultDto> HandlePayOsWebhookAsync(
        string rawBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Webhook PayOS không có nội dung.");
        }

        using var document = ParseWebhookJson(rawBody);
        var root = document.RootElement;

        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Webhook PayOS thiếu dữ liệu giao dịch.");
        }

        var signature = GetString(root, "signature");
        if (!_payOsService.VerifyWebhookSignature(data, signature))
        {
            _logger.LogWarning(
                "PayOS webhook rejected because signature is invalid. Signature={Signature}, Body={RawBody}",
                signature,
                rawBody);
            throw new ApiException(StatusCodes.Status400BadRequest, "Chữ ký webhook PayOS không hợp lệ.");
        }

        var orderCode = GetLong(data, "orderCode");
        var amount = GetDecimal(data, "amount");
        var transferDescription = GetString(data, "description");
        var paymentLinkId = GetString(data, "paymentLinkId");
        var reference = GetString(data, "reference");
        var isSuccessfulPayment = GetBoolean(root, "success")
            && string.Equals(GetString(root, "code"), "00", StringComparison.OrdinalIgnoreCase)
            && string.Equals(GetString(data, "code"), "00", StringComparison.OrdinalIgnoreCase);

        _logger.LogInformation(
            "PayOS webhook received. OrderCode={OrderCode}, PaymentLinkId={PaymentLinkId}, Reference={Reference}, Amount={Amount}, Success={Success}",
            orderCode,
            paymentLinkId,
            reference,
            amount,
            isSuccessfulPayment);

        return await _context.ExecuteInTransactionAsync(
            IsolationLevel.Serializable,
            async () =>
            {
                var transaction = await FindPayOsTransactionAsync(
                    orderCode,
                    paymentLinkId,
                    reference,
                    transferDescription,
                    cancellationToken);
                if (transaction is null)
                {
                    _logger.LogWarning(
                        "PayOS webhook transaction not found. OrderCode={OrderCode}, PaymentLinkId={PaymentLinkId}, Reference={Reference}, Description={Description}",
                        orderCode,
                        paymentLinkId,
                        reference,
                        transferDescription);
                    return new PayOsWebhookResultDto
                    {
                        Processed = false,
                        TrangThai = "khong_tim_thay",
                        Message = "Không tìm thấy giao dịch PayOS tương ứng."
                    };
                }

                if (transaction.TrangThai == FinanceConstants.TransactionStatuses.Success)
                {
                    _logger.LogInformation(
                        "PayOS webhook ignored because transaction already succeeded. MaGiaoDich={MaGiaoDich}, OrderCode={OrderCode}",
                        transaction.MaGiaoDich,
                        orderCode);
                    return new PayOsWebhookResultDto
                    {
                        Processed = true,
                        TrangThai = transaction.TrangThai,
                        Message = "Giao dịch đã thành công trước đó."
                    };
                }

                var now = DateTime.Now;
                transaction.CallbackPayloadJson = rawBody;
                transaction.MaThamChieuCong = FirstNonEmpty(paymentLinkId, reference, transaction.MaThamChieuCong);
                transaction.NgayCapNhat = now;

                if (!isSuccessfulPayment)
                {
                    _logger.LogWarning(
                        "PayOS webhook reports unsuccessful payment. MaGiaoDich={MaGiaoDich}, OldStatus={OldStatus}, OrderCode={OrderCode}",
                        transaction.MaGiaoDich,
                        transaction.TrangThai,
                        orderCode);
                    transaction.TrangThai = FinanceConstants.TransactionStatuses.Failed;
                    await _context.SaveChangesAsync(cancellationToken);

                    return new PayOsWebhookResultDto
                    {
                        Processed = true,
                        TrangThai = transaction.TrangThai,
                        Message = "Webhook PayOS báo giao dịch không thành công."
                    };
                }

                if (amount != transaction.SoTien)
                {
                    _logger.LogWarning(
                        "PayOS webhook amount mismatch. MaGiaoDich={MaGiaoDich}, Expected={ExpectedAmount}, Actual={ActualAmount}",
                        transaction.MaGiaoDich,
                        transaction.SoTien,
                        amount);
                    transaction.TrangThai = FinanceConstants.TransactionStatuses.WrongAmount;
                    await _context.SaveChangesAsync(cancellationToken);

                    return new PayOsWebhookResultDto
                    {
                        Processed = true,
                        TrangThai = transaction.TrangThai,
                        Message = "Số tiền PayOS báo về không khớp giao dịch."
                    };
                }

                if (transaction.HoaDon is null)
                {
                    throw new ApiException(StatusCodes.Status400BadRequest, "Giao dịch PayOS không gắn với hóa đơn hợp lệ.");
                }

                var oldPaymentStatus = transaction.TrangThai;
                var oldInvoicePaid = transaction.HoaDon.DaThanhToan;
                var oldInvoiceStatus = transaction.HoaDon.TrangThai;
                MarkPaymentSuccess(transaction, now);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "PayOS webhook processed. MaGiaoDich={MaGiaoDich}, PaymentStatus={OldPaymentStatus}->{NewPaymentStatus}, HoaDon={MaHoaDon}, Paid={OldPaid}->{NewPaid}, InvoiceStatus={OldInvoiceStatus}->{NewInvoiceStatus}",
                    transaction.MaGiaoDich,
                    oldPaymentStatus,
                    transaction.TrangThai,
                    transaction.MaHoaDon,
                    oldInvoicePaid,
                    transaction.HoaDon.DaThanhToan,
                    oldInvoiceStatus,
                    transaction.HoaDon.TrangThai);

                return new PayOsWebhookResultDto
                {
                    Processed = true,
                    TrangThai = transaction.TrangThai,
                    Message = "Đã cập nhật thanh toán PayOS thành công."
                };
            },
            cancellationToken);
    }

    private async Task TrySyncPendingPayOsPaymentsForStudentAsync(
        int currentUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var pendingPaymentIds = await (
                from payment in _context.GiaoDichs.AsNoTracking()
                join invoice in _context.HoaDons.AsNoTracking()
                    on payment.MaHoaDon equals invoice.MaHoaDon
                where invoice.MaHocSinh == currentUserId
                    && invoice.LoaiHoaDon == FinanceConstants.InvoiceTypes.Tuition
                    && payment.NhaCungCapThanhToan == FinanceConstants.PaymentProviders.PayOs
                    && (payment.TrangThai == FinanceConstants.TransactionStatuses.PendingPayment
                        || payment.TrangThai == FinanceConstants.TransactionStatuses.Processing)
                orderby payment.NgayTao descending, payment.MaGiaoDich descending
                select payment.MaGiaoDich)
                .Take(10)
                .ToListAsync(cancellationToken);

            foreach (var paymentId in pendingPaymentIds)
            {
                await TrySyncPayOsPaymentAsync(paymentId, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Không thể đồng bộ giao dịch PayOS đang chờ cho sinh viên {CurrentUserId}. Tiếp tục trả dữ liệu hóa đơn hiện có.",
                currentUserId);
        }
    }

    private async Task TrySyncPayOsPaymentAsync(
        int transactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await SyncPayOsPaymentAsync(transactionId, cancellationToken);
        }
        catch (PayOsProviderException)
        {
            // Nếu PayOS tạm thời không phản hồi, trả trạng thái hiện tại để FE tiếp tục poll.
        }
    }

    private async Task SyncPayOsPaymentAsync(
        int transactionId,
        CancellationToken cancellationToken)
    {
        var paymentSnapshot = await _context.GiaoDichs
            .AsNoTracking()
            .Where(x =>
                x.MaGiaoDich == transactionId &&
                x.NhaCungCapThanhToan == FinanceConstants.PaymentProviders.PayOs)
            .Select(x => new
            {
                x.MaGiaoDich,
                x.MaThamChieuCong,
                x.TrangThai
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (paymentSnapshot is null ||
            paymentSnapshot.TrangThai == FinanceConstants.TransactionStatuses.Success)
        {
            return;
        }

        var orderCode = ResolvePayOsOrderCode(paymentSnapshot.MaThamChieuCong, paymentSnapshot.MaGiaoDich);
        var payOsStatus = await _payOsService.GetPaymentLinkAsync(orderCode, cancellationToken);

        await _context.ExecuteInTransactionAsync(
            IsolationLevel.Serializable,
            async () =>
            {
                var transaction = await _context.GiaoDichs
                    .Include(x => x.HoaDon)
                    .FirstOrDefaultAsync(x =>
                        x.MaGiaoDich == transactionId &&
                        x.NhaCungCapThanhToan == FinanceConstants.PaymentProviders.PayOs,
                        cancellationToken);

                if (transaction is null ||
                    transaction.TrangThai == FinanceConstants.TransactionStatuses.Success)
                {
                    return;
                }

                var now = DateTime.Now;
                transaction.CallbackPayloadJson = payOsStatus.ResponsePayloadJson;
                transaction.NgayCapNhat = now;

                if (IsPayOsPaid(payOsStatus))
                {
                    var paidAmount = payOsStatus.AmountPaid > 0 ? payOsStatus.AmountPaid : payOsStatus.Amount;
                    if (paidAmount != transaction.SoTien)
                    {
                        transaction.TrangThai = FinanceConstants.TransactionStatuses.WrongAmount;
                        await _context.SaveChangesAsync(cancellationToken);
                        return;
                    }

                    MarkPaymentSuccess(transaction, payOsStatus.PaidAt ?? now);
                    await _context.SaveChangesAsync(cancellationToken);
                    return;
                }

                if (IsPayOsCanceled(payOsStatus.Status))
                {
                    transaction.TrangThai = FinanceConstants.TransactionStatuses.Canceled;
                }
                else if (IsPayOsExpired(payOsStatus.Status))
                {
                    transaction.TrangThai = FinanceConstants.TransactionStatuses.Expired;
                }

                await _context.SaveChangesAsync(cancellationToken);
            },
            cancellationToken);
    }

    private static long ResolvePayOsOrderCode(string? publicReference, int transactionId)
    {
        return long.TryParse(publicReference, NumberStyles.Integer, CultureInfo.InvariantCulture, out var orderCode)
            ? orderCode
            : transactionId;
    }

    private static bool IsPayOsPaid(PayOsPaymentLinkStatusResult status)
    {
        return string.Equals(status.Status, "PAID", StringComparison.OrdinalIgnoreCase)
            || (status.Amount > 0 && status.AmountPaid >= status.Amount)
            || (status.AmountPaid > 0 && status.AmountRemaining <= 0);
    }

    private static bool IsPayOsCanceled(string status)
    {
        return string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPayOsExpired(string status)
    {
        return string.Equals(status, "EXPIRED", StringComparison.OrdinalIgnoreCase);
    }

    private static void MarkPaymentSuccess(GiaoDich transaction, DateTime paidAt)
    {
        if (transaction.TrangThai == FinanceConstants.TransactionStatuses.Success)
        {
            return;
        }

        if (transaction.HoaDon is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giao dịch PayOS không gắn với hóa đơn hợp lệ.");
        }

        transaction.TrangThai = FinanceConstants.TransactionStatuses.Success;
        transaction.NgayThanhToan = paidAt;
        transaction.HoaDon.DaThanhToan += transaction.SoTien;
        transaction.HoaDon.TrangThai = InvoiceFinanceHelper.RecalculateInvoiceStatus(
            transaction.HoaDon,
            DateOnly.FromDateTime(paidAt));
        transaction.HoaDon.NgayCapNhat = paidAt;
    }

    private async Task<CreateTuitionPaymentResponse> CreateVietQrPaymentAsync(
        HoaDon invoice,
        TaiKhoanNhanTien receivingAccount,
        int currentUserId,
        decimal amount,
        string internalReference,
        CancellationToken cancellationToken)
    {
        var transferContent = $"LMS {internalReference}";
        var qrUrl = _vietQrService.CreateQrUrl(
            receivingAccount.MaNganHang,
            receivingAccount.SoTaiKhoan,
            receivingAccount.TenChuTaiKhoan,
            amount,
            transferContent,
            VietQrTemplateId);

        var now = DateTime.UtcNow;
        var transaction = new GiaoDich
        {
            MaHoaDon = invoice.MaHoaDon,
            MaTaiKhoanNhanTien = receivingAccount.MaTaiKhoanNhanTien,
            MaThamChieuNoiBo = internalReference,
            SoTien = amount,
            LoaiGiaoDich = FinanceConstants.TransactionTypes.TuitionPayment,
            TrangThai = FinanceConstants.TransactionStatuses.PendingPayment,
            NhaCungCapThanhToan = FinanceConstants.PaymentProviders.VietQr,
            NoiDungChuyenKhoan = transferContent,
            QrUrl = qrUrl,
            RequestPayloadJson = JsonSerializer.Serialize(new
            {
                provider = FinanceConstants.PaymentProviders.VietQr,
                bankCode = receivingAccount.MaNganHang,
                accountNo = receivingAccount.SoTaiKhoan,
                accountName = receivingAccount.TenChuTaiKhoan,
                amount,
                transferContent,
                templateId = VietQrTemplateId
            }, JsonOptions),
            ResponsePayloadJson = JsonSerializer.Serialize(new { qrUrl }, JsonOptions),
            NgayTao = now,
            NgayCapNhat = now,
            MaNguoiThucHien = currentUserId
        };

        _context.GiaoDichs.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return ToPaymentResponse(transaction);
    }

    private async Task<CreateTuitionPaymentResponse> CreatePayOsPaymentAsync(
        HoaDon invoice,
        int currentUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var transaction = new GiaoDich
        {
            MaHoaDon = invoice.MaHoaDon,
            SoTien = amount,
            LoaiGiaoDich = FinanceConstants.TransactionTypes.TuitionPayment,
            TrangThai = FinanceConstants.TransactionStatuses.PendingPayment,
            NhaCungCapThanhToan = FinanceConstants.PaymentProviders.PayOs,
            NgayTao = now,
            NgayCapNhat = now,
            MaNguoiThucHien = currentUserId
        };

        _context.GiaoDichs.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        var orderCode = transaction.MaGiaoDich;
        var orderCodeText = orderCode.ToString(CultureInfo.InvariantCulture);
        var transferContent = $"LMS {orderCodeText}";

        transaction.MaThamChieuNoiBo = orderCodeText;
        transaction.NoiDungChuyenKhoan = transferContent;
        transaction.NgayCapNhat = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            var payOsResult = await _payOsService.CreatePaymentLinkAsync(
                new PayOsCreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = amount,
                    Description = transferContent,
                    ItemName = $"Học phí {invoice.MaHoaDonCode}"
                },
                cancellationToken);

            transaction.CheckoutUrl = payOsResult.CheckoutUrl;
            transaction.QrPayload = payOsResult.QrPayload;
            transaction.QrUrl = null;
            transaction.MaThamChieuCong = FirstNonEmpty(payOsResult.PaymentLinkId, orderCodeText);
            transaction.ResponsePayloadJson = payOsResult.ResponsePayloadJson;
            transaction.RequestPayloadJson = payOsResult.RequestPayloadJson;
            transaction.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (PayOsProviderException exception)
        {
            transaction.TrangThai = FinanceConstants.TransactionStatuses.Failed;
            transaction.ResponsePayloadJson = exception.ResponsePayloadJson;
            transaction.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            throw new ApiException(StatusCodes.Status502BadGateway, exception.Message);
        }

        return ToPaymentResponse(transaction);
    }

    private async Task<GiaoDich?> GetReusablePendingPaymentAsync(
        int invoiceId,
        int organizationId,
        string provider,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var payment = await _context.GiaoDichs
            .Where(x =>
                x.MaHoaDon == invoiceId &&
                x.NhaCungCapThanhToan == provider &&
                x.LoaiGiaoDich == FinanceConstants.TransactionTypes.TuitionPayment &&
                x.TrangThai == FinanceConstants.TransactionStatuses.PendingPayment &&
                x.SoTien == amount)
            .OrderByDescending(x => x.NgayTao)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            return null;
        }

        if (provider == FinanceConstants.PaymentProviders.PayOs
            && !string.IsNullOrWhiteSpace(payment.CheckoutUrl)
            && !string.IsNullOrWhiteSpace(payment.QrPayload)
            && IsPositiveLong(payment.MaThamChieuNoiBo))
        {
            return payment;
        }

        if (provider == FinanceConstants.PaymentProviders.VietQr && !string.IsNullOrWhiteSpace(payment.QrUrl))
        {
            return payment;
        }

        return null;
    }

    private async Task<TaiKhoanNhanTien> GetReceivingAccountAsync(
        int organizationId,
        string provider,
        CancellationToken cancellationToken)
    {
        var account = await _context.TaiKhoanNhanTiens
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.MaDonVi == organizationId &&
                x.NhaCungCapThanhToan == provider &&
                x.TrangThaiDuyet == FinanceConstants.PaymentAccountApprovalStatuses.Approved &&
                x.LaMacDinh &&
                x.ConHoatDong,
                cancellationToken);

        if (account is null)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Chưa có tài khoản nhận tiền hợp lệ cho nhà cung cấp {provider}.");
        }

        return account;
    }

    private async Task<string> CreateUniqueInternalReferenceAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var reference = $"HP{DateTime.UtcNow:yyMMddHHmmss}{RandomNumberGenerator.GetInt32(1000, 9999)}";
            var exists = await _context.GiaoDichs
                .AsNoTracking()
                .AnyAsync(x => x.MaThamChieuNoiBo == reference, cancellationToken);

            if (!exists)
            {
                return reference;
            }
        }

        throw new ApiException(StatusCodes.Status500InternalServerError, "Không tạo được mã tham chiếu thanh toán.");
    }

    private async Task<GiaoDich?> FindPayOsTransactionAsync(
        long? orderCode,
        string paymentLinkId,
        string reference,
        string transferDescription,
        CancellationToken cancellationToken)
    {
        var query = _context.GiaoDichs
            .Include(x => x.HoaDon)
            .Where(x => x.NhaCungCapThanhToan == FinanceConstants.PaymentProviders.PayOs);

        if (orderCode.HasValue)
        {
            var orderCodeText = orderCode.Value.ToString(CultureInfo.InvariantCulture);
            var byOrderCode = await query
                .FirstOrDefaultAsync(
                    x => x.MaThamChieuNoiBo == orderCodeText
                        || x.MaThamChieuCong == orderCodeText
                        || x.MaGiaoDich == orderCode.Value,
                    cancellationToken);

            if (byOrderCode is not null)
            {
                return byOrderCode;
            }
        }

        var publicReference = FirstNonEmpty(paymentLinkId, reference);
        if (!string.IsNullOrWhiteSpace(publicReference))
        {
            var byPublicReference = await query.FirstOrDefaultAsync(
                x => x.MaThamChieuCong == publicReference,
                cancellationToken);

            if (byPublicReference is not null)
            {
                return byPublicReference;
            }
        }

        var internalReference = ExtractInternalReference(transferDescription);
        if (string.IsNullOrWhiteSpace(internalReference))
        {
            return null;
        }

        return await query.FirstOrDefaultAsync(
            x => x.MaThamChieuNoiBo == internalReference
                || x.MaThamChieuCong == internalReference,
            cancellationToken);
    }

    private static CreateTuitionPaymentResponse ToPaymentResponse(GiaoDich transaction)
    {
        var provider = transaction.NhaCungCapThanhToan ?? string.Empty;

        return new CreateTuitionPaymentResponse
        {
            MaGiaoDich = transaction.MaGiaoDich,
            MaHoaDon = transaction.MaHoaDon,
            Provider = provider,
            Amount = transaction.SoTien,
            MaThamChieuNoiBo = transaction.MaThamChieuNoiBo ?? string.Empty,
            NoiDungChuyenKhoan = transaction.NoiDungChuyenKhoan ?? string.Empty,
            QrUrl = provider == FinanceConstants.PaymentProviders.PayOs ? null : transaction.QrUrl,
            CheckoutUrl = transaction.CheckoutUrl,
            QrPayload = transaction.QrPayload,
            TrangThai = transaction.TrangThai
        };
    }

    private static string NormalizeProvider(string provider)
    {
        var normalized = (provider ?? string.Empty).Trim().ToLowerInvariant();

        return normalized switch
        {
            FinanceConstants.PaymentProviders.PayOs => FinanceConstants.PaymentProviders.PayOs,
            FinanceConstants.PaymentProviders.VietQr => FinanceConstants.PaymentProviders.VietQr,
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ thanh toán PayOS hoặc VietQR.")
        };
    }

    private static JsonDocument ParseWebhookJson(string rawBody)
    {
        try
        {
            return JsonDocument.Parse(rawBody);
        }
        catch (JsonException exception)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Webhook PayOS không phải JSON hợp lệ: {exception.Message}");
        }
    }

    private static string ExtractInternalReference(string transferDescription)
    {
        if (string.IsNullOrWhiteSpace(transferDescription))
        {
            return string.Empty;
        }

        var normalized = transferDescription.Trim();
        return normalized.StartsWith("LMS ", StringComparison.OrdinalIgnoreCase)
            ? normalized[4..].Trim()
            : normalized;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static bool IsPositiveLong(string? value)
    {
        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            && parsed > 0;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : string.Empty;
    }

    private static bool GetBoolean(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.True;
    }

    private static long? GetLong(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
        {
            return number;
        }

        return long.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    private static decimal GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return 0m;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
        {
            return number;
        }

        return decimal.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;
    }
}
