using Backend.Models;
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
        public async Task<ActionResult<List<NhatKyKiemToan>>> GetRecentActivities([FromQuery] int limit = 10)
        {
            var activities = await _superAdminService.GetRecentActivitiesAsync(limit);
            return Ok(activities);
        }

        [HttpGet("security/alerts")]
        public async Task<ActionResult<List<CanhBaoBaoMat>>> GetSecurityAlerts()
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
        public ActionResult<List<object>> GetAiModels()
        {
            // Placeholder for real models
            return Ok(new List<object>());
        }

        [HttpGet("ai/jobs")]
        public ActionResult<List<object>> GetAiJobs()
        {
            // Placeholder for real jobs
            return Ok(new List<object>());
        }
    }
}
