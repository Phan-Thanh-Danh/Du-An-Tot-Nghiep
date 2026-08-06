using Backend.Services.SuperAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [Route("api/super-admin")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ISuperAdminService _superAdminService;

        public SuperAdminController(ISuperAdminService superAdminService)
        {
            _superAdminService = superAdminService;
        }

        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<SystemStatsDto>> GetDashboardStats()
        {
            var stats = await _superAdminService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        [HttpGet("dashboard/activities")]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities([FromQuery] int limit = 10)
        {
            var activities = await _superAdminService.GetRecentActivitiesAsync(limit);
            return Ok(activities);
        }

        [HttpGet("security/alerts")]
        public async Task<ActionResult> GetSecurityAlerts()
        {
            var alerts = await _superAdminService.GetSecurityAlertsAsync();
            return Ok(alerts);
        }

        [HttpGet("system/modules")]
        public async Task<ActionResult<List<SystemModuleDto>>> GetSystemModules()
        {
            var modules = await _superAdminService.GetSystemModulesAsync();
            return Ok(modules);
        }

        [HttpGet("ai/automation-stats")]
        public async Task<ActionResult<AiAutomationStatsDto>> GetAiAutomationStats()
        {
            var stats = await _superAdminService.GetAiAutomationStatsAsync();
            return Ok(stats);
        }

        [HttpGet("ai/models")]
        public async Task<ActionResult<List<AiModelDto>>> GetAiModels()
        {
            var models = await _superAdminService.GetAiModelsAsync();
            return Ok(models);
        }

        [HttpGet("ai/jobs")]
        public async Task<ActionResult<List<AiJobDto>>> GetAiJobs()
        {
            var jobs = await _superAdminService.GetAiJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("login-history")]
        public async Task<ActionResult<List<LoginHistoryDto>>> GetLoginHistory([FromQuery] int limit = 100)
        {
            var history = await _superAdminService.GetLoginHistoryAsync(limit);
            return Ok(history);
        }

        [HttpPost("ai-alerts/config")]
        public async Task<ActionResult> CreateAiAlertConfig([FromBody] CreateAiAlertConfigRequest request)
        {
            await _superAdminService.CreateAiAlertConfigAsync(request);
            return Ok(new { message = "Cấu hình cảnh báo AI thành công" });
        }

        [HttpGet("active-sessions-count")]
        public async Task<ActionResult<int>> GetActiveSessionsCount()
        {
            var count = await _superAdminService.GetActiveSessionsCountAsync();
            return Ok(count);
        }
    }
}
