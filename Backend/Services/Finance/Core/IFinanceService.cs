using Backend.DTOs.Common;
using Backend.DTOs.Finance;

namespace Backend.Services.Finance.Core;

public interface IFinanceService
{
    Task<PagedResultDto<InvoiceListItemDto>> GetInvoicesAsync(
        InvoiceQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto> GetInvoiceByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<TransactionListItemDto>> GetTransactionsAsync(
        TransactionQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<List<PaymentAccountDto>> GetPaymentAccountsAsync(
        int? maDonVi = null,
        CancellationToken cancellationToken = default);
}
