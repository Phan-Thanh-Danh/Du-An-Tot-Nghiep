namespace Backend.Constants;

public static class RewardDisciplineConstants
{
    public static class RewardCampaignTypes
    {
        public const string Top100Semester = "TOP_100_HOC_KY";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Top100Semester
        };
    }

    public static class RewardCampaignStatuses
    {
        public const string Draft = "nhap";
        public const string Evaluating = "dang_xet";
        public const string PendingApproval = "cho_duyet";
        public const string Approved = "da_duyet";
        public const string Published = "da_cong_bo";
        public const string Cancelled = "da_huy";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            Evaluating,
            PendingApproval,
            Approved,
            Published,
            Cancelled
        };
    }

    public static class CertificateTemplateTypes
    {
        public const string Top100Semester = "TOP_100_HOC_KY";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Top100Semester
        };
    }

    public static class PaperOrientations
    {
        public const string A4Landscape = "A4_NGANG";
        public const string A4Portrait = "A4_DOC";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            A4Landscape,
            A4Portrait
        };
    }

    public static class RewardTypes
    {
        public const string Top100Semester = "TOP_100_HOC_KY";
        public const string Other = "KHAC";
        public const string AcademicLegacy = "hoc_luc";
        public const string SpecialLegacy = "dac_biet";
        public const string CompetitionLegacy = "thi_dau";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Top100Semester,
            Other,
            AcademicLegacy,
            SpecialLegacy,
            CompetitionLegacy
        };
    }

    public static class RewardStatuses
    {
        public const string Draft = "nhap";
        public const string PendingApproval = "cho_duyet";
        public const string Approved = "da_duyet";
        public const string Issued = "da_cap";
        public const string PdfGenerated = "da_sinh_pdf";
        public const string PdfFailed = "loi_sinh_pdf";
        public const string Cancelled = "da_huy";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            PendingApproval,
            Approved,
            Issued,
            PdfGenerated,
            PdfFailed,
            Cancelled
        };
    }

    public static class DisciplineLevels
    {
        public const string Minor = "nhe";
        public const string Moderate = "trung_binh";
        public const string Severe = "nghiem_trong";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Minor,
            Moderate,
            Severe
        };
    }

    public static class DisciplineStatuses
    {
        public const string Draft = "nhap";
        public const string PendingApproval = "cho_duyet";
        public const string Approved = "da_duyet";
        public const string Rejected = "tu_choi";
        public const string Active = "dang_hieu_luc";
        public const string Expired = "het_hieu_luc";
        public const string Removed = "da_go_hieu_luc";
        public const string Cancelled = "da_huy";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            PendingApproval,
            Approved,
            Rejected,
            Active,
            Expired,
            Removed,
            Cancelled
        };
    }

    public static class DisciplineActions
    {
        public const string Reminder = "nhac_nho";
        public const string Reprimand = "khien_trach";
        public const string Warning = "canh_cao";
        public const string Suspension = "dinh_chi";
        public const string Other = "khac";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Reminder,
            Reprimand,
            Warning,
            Suspension,
            Other
        };
    }

    public static class RewardCandidateStatuses
    {
        public const string Recommended = "duoc_de_xuat";
        public const string Excluded = "bi_loai";
        public const string Reserve = "du_phong";
        public const string ManuallyAdded = "them_thu_cong";
        public const string ApprovedForReward = "da_duyet_kt";
        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Recommended,
            Excluded,
            Reserve,
            ManuallyAdded,
            ApprovedForReward
        };

        public static readonly IReadOnlySet<string> Selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Recommended,
            ManuallyAdded
        };
    }

    public static class CandidateExclusionReasons
    {
        public const string InactiveStudent = "INACTIVE_STUDENT";
        public const string OutOfScope = "OUT_OF_SCOPE";
        public const string MissingGrades = "MISSING_GRADES";
        public const string LowGpa = "LOW_GPA";
        public const string ActiveDiscipline = "ACTIVE_DISCIPLINE";
        public const string UnpaidTuition = "UNPAID_TUITION";
        public const string Other = "OTHER";
    }

    public static class RewardAuditActions
    {
        public const string AdjustRewardCandidate = "ADJUST_REWARD_CANDIDATE";
        public const string ManualAddRewardCandidate = "MANUAL_ADD_REWARD_CANDIDATE";
        public const string ReorderRewardCandidates = "REORDER_REWARD_CANDIDATES";
        public const string SubmitRewardCampaignForApproval = "SUBMIT_REWARD_CAMPAIGN_FOR_APPROVAL";
        public const string ApproveRewardCampaign = "APPROVE_REWARD_CAMPAIGN";
        public const string GenerateRewardCertificates = "GENERATE_REWARD_CERTIFICATES";
        public const string RegenerateRewardCertificates = "REGENERATE_REWARD_CERTIFICATES";
        public const string RewardCertificateGenerationFailed = "REWARD_CERTIFICATE_GENERATION_FAILED";
    }
}
