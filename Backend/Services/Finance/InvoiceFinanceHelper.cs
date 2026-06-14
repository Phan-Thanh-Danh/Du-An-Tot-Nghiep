using Backend.Constants;
using Backend.Models;

namespace Backend.Services.Finance;

public static class InvoiceFinanceHelper
{
    public static decimal CalculateProgramTuitionTotal(decimal tuitionAmount, decimal materialAmount)
    {
        return tuitionAmount + materialAmount;
    }

    public static decimal CalculateAmountDue(HoaDon invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return Math.Max(0m, invoice.SoTien - invoice.GiamTru);
    }

    public static decimal CalculateRemainingAmount(HoaDon invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return Math.Max(0m, CalculateAmountDue(invoice) - invoice.DaThanhToan);
    }

    public static string RecalculateInvoiceStatus(HoaDon invoice, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        if (invoice.TrangThai == FinanceConstants.InvoiceStatuses.Canceled || invoice.NgayHuy.HasValue)
        {
            return FinanceConstants.InvoiceStatuses.Canceled;
        }

        var amountDue = CalculateAmountDue(invoice);
        if (invoice.DaThanhToan >= amountDue)
        {
            return FinanceConstants.InvoiceStatuses.Paid;
        }

        if (invoice.DaThanhToan <= 0)
        {
            return today > invoice.HanThanhToan
                ? FinanceConstants.InvoiceStatuses.Overdue
                : FinanceConstants.InvoiceStatuses.Unpaid;
        }

        return today > invoice.HanThanhToan
            ? FinanceConstants.InvoiceStatuses.Overdue
            : FinanceConstants.InvoiceStatuses.PartiallyPaid;
    }
}
