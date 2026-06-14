namespace Backend.DTOs.Finance.Schema;

public record FinanceSchemaOptionDto(string Value, string Label, string? Description = null);

public record FinanceFormulaDto(string Key, string Expression, string Description);

public class FinanceSchemaOptionsDto
{
    public IReadOnlyList<FinanceSchemaOptionDto> InvoiceTypes { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> InvoiceStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> TransactionTypes { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> TransactionStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> PaymentProviders { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> PaymentAccountApprovalStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> TuitionCalculationTypes { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> RefundTypes { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> RefundRequestStatuses { get; set; } = [];
    public IReadOnlyList<FinanceFormulaDto> Formulas { get; set; } = [];
    public IReadOnlyList<string> InvoiceStatusRules { get; set; } = [];
}

public class FinanceSchemaStatusesDto
{
    public IReadOnlyList<FinanceSchemaOptionDto> InvoiceStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> TransactionStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> PaymentAccountApprovalStatuses { get; set; } = [];
    public IReadOnlyList<FinanceSchemaOptionDto> RefundRequestStatuses { get; set; } = [];
}
