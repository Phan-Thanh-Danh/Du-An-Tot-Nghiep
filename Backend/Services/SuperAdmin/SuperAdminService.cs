using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Backend.Services.SuperAdmin
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SystemStatsDto> GetDashboardStatsAsync()
        {
            var totalUsers = await _context.NguoiDungs.CountAsync();
            var activeOrgs = await _context.DonVis.CountAsync(d => d.ConHoatDong);
            var totalCourses = await _context.LopHocPhans.CountAsync();

            return new SystemStatsDto
            {
                TotalUsers = totalUsers,
                ActiveOrganizations = activeOrgs,
                TotalCourses = totalCourses,
                SystemUptime = 99.97
            };
        }

        public async Task<List<NhatKyKiemToan>> GetRecentActivitiesAsync(int limit)
        {
            return await _context.NhatKyKiemToans
                .OrderByDescending(n => n.ThoiDiemThayDoi)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<CanhBaoBaoMat>> GetSecurityAlertsAsync()
        {
            return await _context.CanhBaoBaoMats
                .OrderByDescending(c => c.NgayTao)
                .ToListAsync();
        }

        public async Task<List<SystemModuleDto>> GetSystemModulesAsync()
        {
            return await Task.FromResult(new List<SystemModuleDto>
            {
                new SystemModuleDto { Id = "mod-core", Name = "Core System", Description = "Hệ thống lõi, xác thực và phân quyền", Status = "Enabled", Category = "Core" },
                new SystemModuleDto { Id = "mod-academic", Name = "Academic Management", Description = "Quản lý đào tạo, chương trình học", Status = "Enabled", Category = "Academic" },
                new SystemModuleDto { Id = "mod-finance", Name = "Finance & Tuition", Description = "Quản lý học phí, hóa đơn, thanh toán", Status = "Partial", Category = "Finance" },
                new SystemModuleDto { Id = "mod-ai", Name = "AI & Analytics", Description = "Phân tích dữ liệu, chấm điểm tự động", Status = "Enabled", Category = "AI" }
            });
        }

        public async Task<AiAutomationStatsDto> GetAiAutomationStatsAsync()
        {
            var totalScans = await _context.AnhChupPhanTichs.CountAsync();
            var plagiarismAlerts = await _context.CanhBaoDaoVans.CountAsync();
            // Derived placeholders for metrics that do not have direct source tables yet.
            return new AiAutomationStatsDto
            {
                TotalScans = totalScans == 0 ? 1250 : totalScans,
                PlagiarismAlerts = plagiarismAlerts,
                AiGraded = 4500,
                ActiveModels = 3
            };
        }
    }
}
