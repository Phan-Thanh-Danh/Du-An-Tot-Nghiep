namespace Backend.Constants;

public static class ApplicationTypes
{
    public const string Leave = "nghi_phep";
    public const string RetakeExam = "thi_lai";
    public const string TransferSchool = "chuyen_truong";
    public const string Certificate = "cap_chung_chi";
    public const string Other = "khac";
    public const string GradeAppeal = "phuc_tra_diem";
    public const string AcademicPause = "bao_luu";
    public const string ChangeMajor = "chuyen_nganh";
    public const string ChangeCampus = "chuyen_co_so";
    public const string Confirmation = "xac_nhan";
    public const string Withdrawal = "rut_hoc";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Leave,
        RetakeExam,
        TransferSchool,
        Certificate,
        Other,
        GradeAppeal,
        AcademicPause,
        ChangeMajor,
        ChangeCampus,
        Confirmation,
        Withdrawal
    };
}

public static class ApplicationStatuses
{
    public const string Draft = "nhap";
    public const string Submitted = "da_nop";
    public const string InReview = "dang_xem_xet";
    public const string NeedSupplement = "yeu_cau_bo_sung";
    public const string Approved = "da_duyet";
    public const string Rejected = "tu_choi";
    public const string Cancelled = "da_huy";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Submitted,
        InReview,
        NeedSupplement,
        Approved,
        Rejected,
        Cancelled
    };
}

public static class ApplicationProcessingStatuses
{
    public const string NotProcessed = "chua_xu_ly";
    public const string Pending = "cho_xu_ly";
    public const string Recorded = "da_ghi_nhan";
    public const string Succeeded = "xu_ly_thanh_cong";
    public const string Failed = "xu_ly_that_bai";
    public const string ManualRequired = "can_xu_ly_thu_cong";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        NotProcessed,
        Pending,
        Recorded,
        Succeeded,
        Failed,
        ManualRequired
    };
}

public static class ApplicationActions
{
    public const string CreateDraft = "tao_nhap";
    public const string Update = "cap_nhat";
    public const string Submit = "nop";
    public const string Resubmit = "nop_lai";
    public const string Assign = "phan_cong";
    public const string Reassign = "phan_cong_lai";
    public const string Receive = "tiep_nhan";
    public const string RequestSupplement = "yeu_cau_bo_sung";
    public const string Supplement = "bo_sung";
    public const string Approve = "phe_duyet";
    public const string Reject = "tu_choi";
    public const string Escalate = "leo_thang";
    public const string Cancel = "huy";
    public const string BusinessProcess = "xu_ly_nghiep_vu";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        CreateDraft,
        Update,
        Submit,
        Resubmit,
        Assign,
        Reassign,
        Receive,
        RequestSupplement,
        Supplement,
        Approve,
        Reject,
        Escalate,
        Cancel,
        BusinessProcess
    };
}

public static class ApplicationFieldTypes
{
    public const string Text = "text";
    public const string Textarea = "textarea";
    public const string Number = "number";
    public const string Date = "date";
    public const string DateTime = "datetime";
    public const string Select = "select";
    public const string Multiselect = "multiselect";
    public const string Boolean = "boolean";
    public const string RelatedEntity = "related_entity";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Text,
        Textarea,
        Number,
        Date,
        DateTime,
        Select,
        Multiselect,
        Boolean,
        RelatedEntity
    };
}

public static class ApplicationRelatedEntities
{
    public const string AcademicTerm = "hoc_ky";
    public const string Subject = "mon_hoc";
    public const string Grade = "diem_so";
    public const string Organization = "don_vi";
    public const string Major = "nganh";
    public const string Specialization = "chuyen_nganh";
    public const string Course = "khoa_hoc";
    public const string ClassSession = "buoi_hoc";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        AcademicTerm,
        Subject,
        Grade,
        Organization,
        Major,
        Specialization,
        Course,
        ClassSession
    };
}

public static class ApplicationEvidenceConstants
{
    public const int MaxFiles = 5;
    public const long MaxFileSizeBytes = 10L * 1024 * 1024;
    public const long MaxTotalSizeBytes = 25L * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    };
}
