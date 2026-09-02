using Backend.DTOs.Finance;

namespace Backend.Services.Finance;

public interface IFinanceService
{
    Task<(List<HoaDonDto> Items, int Total)> GetInvoicesAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default);

    Task<HoaDonDetailDto?> GetInvoiceDetailAsync(
        int maHoaDon, int maDonVi,
        CancellationToken cancellationToken = default);

    Task<FinanceMonitorDto> GetMonitorAsync(
        int maDonVi, DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<(List<GiaoDichDto> Items, int Total)> GetTransactionsAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto> CreateInvoiceAsync(
        int maDonVi, CreateInvoiceRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateInvoiceStatusAsync(
        int maHoaDon, int maDonVi, UpdateInvoiceStatusRequest request,
        int maNguoiThucHien,
        CancellationToken cancellationToken = default);

    Task<RefundRequestDto> CreateRefundRequestAsync(
        int maDonVi, CreateRefundRequest request, int maNguoiTao,
        CancellationToken cancellationToken = default);

    Task<(List<RefundRequestDto> Items, int Total)> GetRefundRequestsAsync(
        int maDonVi, int pageIndex, int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> ApproveRefundRequestAsync(
        int maHoanPhi, int maDonVi, ApproveRefundRequest request,
        int maNguoiDuyet,
        CancellationToken cancellationToken = default);
}
