namespace Backend.Constants;

public static class FinanceConstants
{
    public static class TuitionCalculationTypes
    {
        public const string FixedPerTerm = "co_dinh_theo_hoc_ky";
        public const string PerCredit = "theo_tin_chi";
        public const string PerSubject = "theo_mon_hoc";

        public static readonly string[] All = [FixedPerTerm, PerCredit, PerSubject];
    }

    public static class InvoiceTypes
    {
        public const string Tuition = "hoc_phi";
        public const string Fee = "le_phi";
        public const string Material = "tai_lieu";
        public const string Other = "khac";

        public static readonly string[] All = [Tuition, Fee, Material, Other];
    }

    public static class InvoiceStatuses
    {
        public const string Unpaid = "chua_thanh_toan";
        public const string PartiallyPaid = "thanh_toan_mot_phan";
        public const string Paid = "da_thanh_toan";
        public const string Overdue = "qua_han";
        public const string Canceled = "da_huy";

        public static readonly string[] All = [Unpaid, PartiallyPaid, Paid, Overdue, Canceled];
    }

    public static class TransactionTypes
    {
        public const string TuitionCharge = "phat_sinh_hoc_phi";
        public const string TuitionPayment = "thanh_toan_hoc_phi";
        public const string DebtAdjustment = "dieu_chinh_cong_no";
        public const string Refund = "hoan_tien";
        public const string InvoiceCancellation = "huy_hoa_don";

        public static readonly string[] All =
        [
            TuitionCharge,
            TuitionPayment,
            DebtAdjustment,
            Refund,
            InvoiceCancellation
        ];
    }

    public static class TransactionStatuses
    {
        public const string Created = "phat_sinh";
        public const string PendingPayment = "cho_thanh_toan";
        public const string Processing = "dang_xu_ly";
        public const string Success = "thanh_cong";
        public const string Failed = "that_bai";
        public const string Expired = "het_han";
        public const string Canceled = "da_huy";
        public const string WrongAmount = "sai_so_tien";
        public const string ManualReview = "cho_xu_ly_thu_cong";

        public static readonly string[] All =
        [
            Created,
            PendingPayment,
            Processing,
            Success,
            Failed,
            Expired,
            Canceled,
            WrongAmount,
            ManualReview
        ];
    }

    public static class PaymentProviders
    {
        public const string VietQr = "vietqr";
        public const string PayOs = "payos";

        public static readonly string[] All = [VietQr, PayOs];
    }

    public static class PaymentAccountApprovalStatuses
    {
        public const string Draft = "nhap";
        public const string PendingApproval = "cho_duyet";
        public const string Approved = "da_duyet";
        public const string Rejected = "tu_choi";
        public const string Inactive = "ngung_hoat_dong";

        public static readonly string[] All =
        [
            Draft,
            PendingApproval,
            Approved,
            Rejected,
            Inactive
        ];
    }

    public static class RefundTypes
    {
        public const string Full = "toan_phan";
        public const string Partial = "mot_phan";
        public const string Credit = "ghi_co";

        public static readonly string[] All = [Full, Partial, Credit];
    }

    public static class RefundRequestStatuses
    {
        public const string PendingApproval = "cho_duyet";
        public const string Approved = "da_duyet";
        public const string Rejected = "tu_choi";
        public const string Processed = "da_xu_ly";

        public static readonly string[] All =
        [
            PendingApproval,
            Approved,
            Rejected,
            Processed
        ];
    }

    public static class FinanceAuthorizationRoles
    {
        public const string SchemaReaders =
            AuthRoles.SuperAdmin + "," +
            AuthRoles.FinanceAdmin + "," +
            AuthRoles.CampusChiefAccountant + "," +
            AuthRoles.CampusAccountant;

        public const string TuitionConfigReaders = SchemaReaders;

        public const string TuitionConfigManagers =
            AuthRoles.SuperAdmin + "," +
            AuthRoles.FinanceAdmin;
    }
}
