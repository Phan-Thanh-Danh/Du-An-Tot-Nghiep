import re

with open('Backend/Constants/NotificationConstants.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Add to SpecializedTypes in Types class
if 'public const string GeneralTargeted =' not in content:
    types_addition = """
        public const string GeneralTargeted = "general_targeted";"""
    content = content.replace('public const string System = "system";', 'public const string System = "system";' + types_addition)
    content = content.replace('RewardDiscipline\n        };', 'RewardDiscipline,\n            GeneralTargeted\n        };')

# Add EventCodes
if 'public static class EventCodes' not in content:
    event_codes = """
    public static class EventCodes
    {
        public const string TUITION_NOTICE = "TUITION_NOTICE";
        public const string ACADEMIC_NOTICE = "ACADEMIC_NOTICE";
        public const string URGENT_NOTICE = "URGENT_NOTICE";
        public const string MAINTENANCE_NOTICE = "MAINTENANCE_NOTICE";
    }

    public static class NotificationPriorities"""
    content = content.replace('public static class NotificationPriorities', event_codes)

# Add AuditActions to NotificationTemplateAuditActions or create SpecializedNotificationAuditActions
if 'public static class SpecializedNotificationAuditActions' not in content:
    audit_actions = """
    public static class SpecializedNotificationAuditActions
    {
        public const string SEND_TUITION_NOTIFICATION = "SEND_TUITION_NOTIFICATION";
        public const string SEND_ACADEMIC_NOTIFICATION = "SEND_ACADEMIC_NOTIFICATION";
        public const string SEND_URGENT_NOTIFICATION = "SEND_URGENT_NOTIFICATION";
        public const string SEND_MAINTENANCE_NOTIFICATION = "SEND_MAINTENANCE_NOTIFICATION";
        public const string PREVIEW_SPECIALIZED_NOTIFICATION_RECIPIENTS = "PREVIEW_SPECIALIZED_NOTIFICATION_RECIPIENTS";
    }

    public static class NotificationChannels"""
    content = content.replace('public static class NotificationChannels', audit_actions)

with open('Backend/Constants/NotificationConstants.cs', 'w', encoding='utf-8') as f:
    f.write(content)
