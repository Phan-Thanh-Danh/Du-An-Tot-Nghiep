namespace Backend.Constants;

public static class NotificationConstants
{
    public static class Types
    {
        public const string General = "thong_bao_chung";
        public const string Tuition = "hoc_phi";
        public const string Maintenance = "bao_tri";
        public const string Facility = "co_so_vat_chat";
        public const string Academic = "hoc_vu";
        public const string Urgent = "khan_cap";
        public const string System = "system";
        public const string Manual = "manual";
        public const string ScheduleChanged = "schedule_changed";
        public const string SessionCancelled = "session_cancelled";
        public const string AttendanceUnlockApproved = "attendance_unlock_approved";
        public const string AttendanceUnlockRejected = "attendance_unlock_rejected";
        public const string RewardDiscipline = Academic;

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            General,
            Tuition,
            Maintenance,
            Facility,
            Academic,
            Urgent,
            System,
            Manual,
            ScheduleChanged,
            SessionCancelled,
            AttendanceUnlockApproved,
            AttendanceUnlockRejected,
            RewardDiscipline
        };
    }

    public static class Levels
    {
        public const string Information = "thong_tin";
        public const string ImportantVietnamese = "quan_trong";
        public const string Urgent = "khan_cap";
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Important = "important";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Information,
            ImportantVietnamese,
            Urgent,
            Info,
            Warning,
            Important
        };
    }

    public static class Scopes
    {
        public const string AllSystem = "toan_he_thong";
        public const string Organization = "don_vi";
        public const string Class = "lop_hanh_chinh";
        public const string Role = "vai_tro";
        public const string User = "nguoi_dung";
        public const string Course = "khoa_hoc";
        public const string LegacyUsers = "users";
        public const string LegacyClass = "class";
        public const string LegacyCourse = "course";
        public const string LegacyCampus = "campus";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AllSystem,
            Organization,
            Class,
            Role,
            User,
            Course,
            LegacyUsers,
            LegacyClass,
            LegacyCourse,
            LegacyCampus
        };
    }

    public static class Statuses
    {
        public const string Draft = "nhap";
        public const string Sent = "da_gui";
        public const string Cancelled = "da_huy";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Draft,
            Sent,
            Cancelled
        };
    }

    public static class NotificationTemplateAuditActions
    {
        public const string Create = "CREATE_NOTIFICATION_TEMPLATE";
        public const string Update = "UPDATE_NOTIFICATION_TEMPLATE";
        public const string Activate = "ACTIVATE_NOTIFICATION_TEMPLATE";
        public const string Deactivate = "DEACTIVATE_NOTIFICATION_TEMPLATE";
        public const string DeleteOrDisable = "DELETE_OR_DISABLE_NOTIFICATION_TEMPLATE";
        public const string Preview = "PREVIEW_NOTIFICATION_TEMPLATE";
    }

    public static class NotificationPriorities
    {
        public const string Low = "low";
        public const string Normal = "normal";
        public const string High = "high";
        public const string Urgent = "urgent";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Low,
            Normal,
            High,
            Urgent
        };
    }

    public static class NotificationChannels
    {
        public const string InApp = "in_app";
        public const string Email = "email";
        public const string Sms = "sms";
        public const string ThongBaoDay = "thong_bao_day";

        public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            InApp,
            Email,
            Sms,
            ThongBaoDay
        };
    }
}
