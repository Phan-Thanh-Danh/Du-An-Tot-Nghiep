using Backend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Backend.Services.SuperAdmin
{
    public class SystemStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveOrganizations { get; set; }
        public int TotalCourses { get; set; }
        public double SystemUptime { get; set; }
        // Thay đổi so với kỳ trước (30 ngày)
        public string TotalUsersChange { get; set; } = "+0";
        public string ActiveOrgsChange { get; set; } = "+0";
        public string TotalCoursesChange { get; set; } = "+0%";
        public string SystemUptimeTrend { get; set; } = "up";
        public string TotalUsersTrend { get; set; } = "up";
        public string ActiveOrgsTrend { get; set; } = "up";
        public string TotalCoursesTrend { get; set; } = "up";
    }

    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string HanhDong { get; set; } = string.Empty;
        public string LoaiDoiTuong { get; set; } = string.Empty;
        public string MaDoiTuong { get; set; } = string.Empty;
        public string? NguoiThucHien { get; set; }
        public string? DiaChiIp { get; set; }
        public string? MoTa { get; set; }
        public DateTime ThoiDiemThayDoi { get; set; }
    }

    public class LoginHistoryDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public string Ip { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public DateTime LoginTime { get; set; }
        public string? SessionId { get; set; }
    }

    public class AiJobDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string CronDescription { get; set; } = string.Empty;
        public string LastRun { get; set; } = string.Empty;
        public string LastRunResult { get; set; } = "Success";
        public string Duration { get; set; } = "0s";
        public string NextRun { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
    }

    public class AiModelDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Enabled";
        public string ApiService { get; set; } = string.Empty;
        public string LastAccuracy { get; set; } = "N/A";
        public string Latency { get; set; } = "N/A";
    }

    public class AiAutomationStatsDto
    {
        public int TotalScans { get; set; }
        public int PlagiarismAlerts { get; set; }
        public int AiGraded { get; set; }
        public int ActiveModels { get; set; }
    }

    public class SystemModuleDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
    }

    public class CreateAiAlertConfigRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TriggerType { get; set; } = string.Empty;
        public int Threshold { get; set; }
        public string Channel { get; set; } = string.Empty;
    }

    public interface ISuperAdminService
    {
        Task<SystemStatsDto> GetDashboardStatsAsync();
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int limit);
        Task<List<CanhBaoBaoMat>> GetSecurityAlertsAsync();
        Task<List<SystemModuleDto>> GetSystemModulesAsync();
        Task<AiAutomationStatsDto> GetAiAutomationStatsAsync();
        Task<List<LoginHistoryDto>> GetLoginHistoryAsync(int limit);
        Task<List<AiJobDto>> GetAiJobsAsync();
        Task<List<AiModelDto>> GetAiModelsAsync();
        
        Task CreateAiAlertConfigAsync(CreateAiAlertConfigRequest request);
        Task<int> GetActiveSessionsCountAsync();
    }
}

