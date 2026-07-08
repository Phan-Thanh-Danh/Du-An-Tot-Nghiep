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

    public interface ISuperAdminService
    {
        Task<SystemStatsDto> GetDashboardStatsAsync();
        Task<List<NhatKyKiemToan>> GetRecentActivitiesAsync(int limit);
        Task<List<CanhBaoBaoMat>> GetSecurityAlertsAsync();
        Task<List<SystemModuleDto>> GetSystemModulesAsync();
        Task<AiAutomationStatsDto> GetAiAutomationStatsAsync();
    }
}
