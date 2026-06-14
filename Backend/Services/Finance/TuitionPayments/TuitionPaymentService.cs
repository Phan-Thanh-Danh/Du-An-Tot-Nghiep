using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Data;
using Backend.DTOs.Finance.TuitionPayments;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Finance.TuitionPayments;

public class TuitionPaymentService : ITuitionPaymentService
{
    private const string TuitionInvoiceType = "hoc_phi";
    private const string PaymentTransactionType = "thanh_toan_hoc_phi";
    private const string PendingStatus = "cho_thanh_toan";
    private const string SuccessStatus = "thanh_cong";
    private const string FailedStatus = "that_bai";
    private const string WrongAmountStatus = "sai_so_tien";
    private const string PayOsProvider = "payos";
    private const string VietQrProvider = "vietqr";
    private const string ApprovedReceivingAccountStatus = "da_duyet";
    private const string VietQrTemplateId = "Z0oL3W9";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ApplicationDbContext _context;
    private readonly IVietQrService _vietQrService;
    private readonly IPayOsService _payOsService;

    public TuitionPaymentService(
        ApplicationDbContext context,
        IVietQrService vietQrService,
        IPayOsService payOsService)
    {
        _context = context;
        _vietQrService = vietQrService;
        _payOsService = payOsService;
    }

    public async Task<IReadOnlyList<StudentTuitionInvoiceDto>> GetStudentInvoicesAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await (
            from invoice in _context.HoaDons.AsNoTracking()
            join term in _context.HocKys.AsNoTracking()
                on invoice.MaHocKy equals term.MaHocKy into termJoin
            from term in termJoin.DefaultIfEmpty()
            where invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == TuitionInvoiceType
            orderby invoice.HanThanhToan descending, invoice.MaHoaDon descending
            select new
            {
                Invoice = invoice,
                TermName = term != null ? term.TenHocKy : null
            })
            .ToListAsync(cancellationToken);

        return invoices.Select(item =>
        {
            var totalDue = CalculateTotalDue(item.Invoice);
            var remaining = CalculateRemaining(item.Invoice);

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
        var transactions = await (
            from transaction in _context.GiaoDichs.AsNoTracking()
            join invoice in _context.HoaDons.AsNoTracking()
                on transaction.MaHoaDon equals invoice.MaHoaDon
            where invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == TuitionInvoiceType
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
                x.LoaiHoaDon == TuitionInvoiceType,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy hóa đơn học phí.");
        }

        if (invoice.TrangThai == "da_huy")
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hóa đơn đã hủy, không thể thanh toán.");
        }

        var remaining = CalculateRemaining(invoice);
        if (remaining <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Hóa đơn đã được thanh toán đủ.");
        }

        var reusablePayment = await GetReusablePendingPaymentAsync(invoice.MaHoaDon, normalizedProvider, remaining, cancellationToken);
        if (reusablePayment is not null)
        {
            return ToPaymentResponse(reusablePayment);
        }

        var receivingAccount = await GetReceivingAccountAsync(invoice.MaDonVi, normalizedProvider, cancellationToken);
        var internalReference = await CreateUniqueInternalReferenceAsync(cancellationToken);
        var transferContent = $"LMS {internalReference}";

        return normalizedProvider switch
        {
            VietQrProvider => await CreateVietQrPaymentAsync(
                invoice,
                receivingAccount,
                currentUserId,
                remaining,
                internalReference,
                transferContent,
                cancellationToken),
            PayOsProvider => await CreatePayOsPaymentAsync(
                invoice,
                receivingAccount,
                currentUserId,
                remaining,
                internalReference,
                transferContent,
                cancellationToken),
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Nhà cung cấp thanh toán không hợp lệ.")
        };
    }

    public async Task<CreateTuitionPaymentResponse> GetPaymentAsync(
        int transactionId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await (
            from payment in _context.GiaoDichs.AsNoTracking()
            join invoice in _context.HoaDons.AsNoTracking()
                on payment.MaHoaDon equals invoice.MaHoaDon
            where payment.MaGiaoDich == transactionId
                && invoice.MaHocSinh == currentUserId
                && invoice.LoaiHoaDon == TuitionInvoiceType
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
            throw new ApiException(StatusCodes.Status400BadRequest, "Chữ ký webhook PayOS không hợp lệ.");
        }

        var orderCode = GetLong(data, "orderCode");
        var amount = GetDecimal(data, "amount");
        var transferDescription = GetString(data, "description");
        var isSuccessfulPayment = GetBoolean(root, "success")
            && string.Equals(GetString(root, "code"), "00", StringComparison.OrdinalIgnoreCase)
            && string.Equals(GetString(data, "code"), "00", StringComparison.OrdinalIgnoreCase);

        await using var dbTransaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var transaction = await FindPayOsTransactionAsync(orderCode, transferDescription, cancellationToken);
        if (transaction is null)
        {
            await dbTransaction.CommitAsync(cancellationToken);
            return new PayOsWebhookResultDto
            {
                Processed = false,
                TrangThai = "khong_tim_thay",
                Message = "Không tìm thấy giao dịch PayOS tương ứng."
            };
        }

        if (transaction.TrangThai == SuccessStatus)
        {
            await dbTransaction.CommitAsync(cancellationToken);
            return new PayOsWebhookResultDto
            {
                Processed = true,
                TrangThai = transaction.TrangThai,
                Message = "Giao dịch đã thành công trước đó."
            };
        }

        var now = DateTime.Now;
        transaction.CallbackPayloadJson = rawBody;
        transaction.NgayCapNhat = now;

        if (!isSuccessfulPayment)
        {
            transaction.TrangThai = FailedStatus;
            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return new PayOsWebhookResultDto
            {
                Processed = true,
                TrangThai = transaction.TrangThai,
                Message = "Webhook PayOS báo giao dịch không thành công."
            };
        }

        if (amount != transaction.SoTien)
        {
            transaction.TrangThai = WrongAmountStatus;
            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

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

        transaction.TrangThai = SuccessStatus;
        transaction.NgayThanhToan = now;
        transaction.HoaDon.DaThanhToan += transaction.SoTien;
        transaction.HoaDon.TrangThai = CalculateInvoiceStatus(transaction.HoaDon, now);
        transaction.HoaDon.NgayCapNhat = now;

        await _context.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        return new PayOsWebhookResultDto
        {
            Processed = true,
            TrangThai = transaction.TrangThai,
            Message = "Đã cập nhật thanh toán PayOS thành công."
        };
    }

    private async Task<CreateTuitionPaymentResponse> CreateVietQrPaymentAsync(
        HoaDon invoice,
        TaiKhoanNhanTien receivingAccount,
        int currentUserId,
        decimal amount,
        string internalReference,
        string transferContent,
        CancellationToken cancellationToken)
    {
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
            LoaiGiaoDich = PaymentTransactionType,
            TrangThai = PendingStatus,
            NhaCungCapThanhToan = VietQrProvider,
            NoiDungChuyenKhoan = transferContent,
            QrUrl = qrUrl,
            RequestPayloadJson = JsonSerializer.Serialize(new
            {
                provider = VietQrProvider,
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
        TaiKhoanNhanTien receivingAccount,
        int currentUserId,
        decimal amount,
        string internalReference,
        string transferContent,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var transaction = new GiaoDich
        {
            MaHoaDon = invoice.MaHoaDon,
            MaTaiKhoanNhanTien = receivingAccount.MaTaiKhoanNhanTien,
            MaThamChieuNoiBo = internalReference,
            SoTien = amount,
            LoaiGiaoDich = PaymentTransactionType,
            TrangThai = PendingStatus,
            NhaCungCapThanhToan = PayOsProvider,
            NoiDungChuyenKhoan = transferContent,
            NgayTao = now,
            NgayCapNhat = now,
            MaNguoiThucHien = currentUserId
        };

        _context.GiaoDichs.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        transaction.MaThamChieuCong = transaction.MaGiaoDich.ToString(CultureInfo.InvariantCulture);

        try
        {
            var payOsResult = await _payOsService.CreatePaymentLinkAsync(
                new PayOsCreatePaymentLinkRequest
                {
                    OrderCode = transaction.MaGiaoDich,
                    Amount = amount,
                    Description = transferContent,
                    ItemName = $"Học phí {invoice.MaHoaDonCode}"
                },
                cancellationToken);

            transaction.CheckoutUrl = payOsResult.CheckoutUrl;
            transaction.QrPayload = payOsResult.QrPayload;
            transaction.ResponsePayloadJson = payOsResult.ResponsePayloadJson;
            transaction.RequestPayloadJson = payOsResult.RequestPayloadJson;
            transaction.NgayCapNhat = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (PayOsProviderException exception)
        {
            transaction.TrangThai = FailedStatus;
            transaction.ResponsePayloadJson = exception.ResponsePayloadJson;
            transaction.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            throw new ApiException(StatusCodes.Status502BadGateway, exception.Message);
        }

        return ToPaymentResponse(transaction);
    }

    private async Task<GiaoDich?> GetReusablePendingPaymentAsync(
        int invoiceId,
        string provider,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var payment = await _context.GiaoDichs
            .AsNoTracking()
            .Where(x =>
                x.MaHoaDon == invoiceId &&
                x.NhaCungCapThanhToan == provider &&
                x.LoaiGiaoDich == PaymentTransactionType &&
                x.TrangThai == PendingStatus &&
                x.SoTien == amount)
            .OrderByDescending(x => x.NgayTao)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            return null;
        }

        if (provider == PayOsProvider && !string.IsNullOrWhiteSpace(payment.CheckoutUrl))
        {
            return payment;
        }

        if (provider == VietQrProvider && !string.IsNullOrWhiteSpace(payment.QrUrl))
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
                x.TrangThaiDuyet == ApprovedReceivingAccountStatus &&
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
        string transferDescription,
        CancellationToken cancellationToken)
    {
        var query = _context.GiaoDichs
            .Include(x => x.HoaDon)
            .Where(x => x.NhaCungCapThanhToan == PayOsProvider);

        if (orderCode.HasValue)
        {
            var orderCodeText = orderCode.Value.ToString(CultureInfo.InvariantCulture);
            var byOrderCode = await query
                .FirstOrDefaultAsync(x => x.MaThamChieuCong == orderCodeText, cancellationToken);

            if (byOrderCode is not null)
            {
                return byOrderCode;
            }
        }

        var internalReference = ExtractInternalReference(transferDescription);
        if (string.IsNullOrWhiteSpace(internalReference))
        {
            return null;
        }

        return await query.FirstOrDefaultAsync(
            x => x.MaThamChieuNoiBo == internalReference,
            cancellationToken);
    }

    private static CreateTuitionPaymentResponse ToPaymentResponse(GiaoDich transaction)
    {
        return new CreateTuitionPaymentResponse
        {
            MaGiaoDich = transaction.MaGiaoDich,
            MaHoaDon = transaction.MaHoaDon,
            Provider = transaction.NhaCungCapThanhToan ?? string.Empty,
            Amount = transaction.SoTien,
            MaThamChieuNoiBo = transaction.MaThamChieuNoiBo ?? string.Empty,
            NoiDungChuyenKhoan = transaction.NoiDungChuyenKhoan ?? string.Empty,
            QrUrl = transaction.QrUrl,
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
            PayOsProvider => PayOsProvider,
            VietQrProvider => VietQrProvider,
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ thanh toán PayOS hoặc VietQR.")
        };
    }

    private static decimal CalculateTotalDue(HoaDon invoice)
    {
        return invoice.SoTien - invoice.GiamTru;
    }

    private static decimal CalculateRemaining(HoaDon invoice)
    {
        return CalculateTotalDue(invoice) - invoice.DaThanhToan;
    }

    private static string CalculateInvoiceStatus(HoaDon invoice, DateTime now)
    {
        var totalDue = CalculateTotalDue(invoice);
        if (invoice.DaThanhToan >= totalDue)
        {
            return "da_thanh_toan";
        }

        if (invoice.DaThanhToan > 0 && invoice.DaThanhToan < totalDue)
        {
            return "thanh_toan_mot_phan";
        }

        return DateOnly.FromDateTime(now) > invoice.HanThanhToan
            ? "qua_han"
            : "chua_thanh_toan";
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
