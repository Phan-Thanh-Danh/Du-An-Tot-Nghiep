using Backend.Models;
using Backend.Data;
using Backend.Services.Export;
using Backend.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/super-admin/exports")]
    [Authorize] // Should require SuperAdmin role, using Authorize for now
    public class DataExportController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ExportQueueService _exportQueueService;

        public DataExportController(ApplicationDbContext db, ExportQueueService exportQueueService)
        {
            _db = db;
            _exportQueueService = exportQueueService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateExportRequest([FromBody] ExportRequestDto dto, CancellationToken cancellationToken)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            
            var request = new YeuCauXuatDuLieu
            {
                MaYeuCau = $"RPT-{DateTime.Now.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                LoaiBaoCao = dto.Type,
                TenBaoCao = GetTypeName(dto.Type),
                HocKy = dto.Semester,
                CapDonVi = dto.Campus,
                DinhDang = dto.Format,
                NguoiYeuCau = currentUser?.UserId.ToString() ?? "Unknown",
                ThoiGianYeuCau = DateTime.UtcNow,
                TrangThai = "queued"
            };

            _db.YeuCauXuatDuLieus.Add(request);
            await _db.SaveChangesAsync(cancellationToken);

            // Enqueue
            await _exportQueueService.EnqueueExportAsync(request.MaYeuCau);

            return Ok(new { success = true, message = "Đã đưa vào hàng đợi", data = request });
        }

        [HttpGet]
        public async Task<IActionResult> GetExportHistory(CancellationToken cancellationToken)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userIdStr = currentUser?.UserId.ToString() ?? "Unknown";
            bool isSuperAdmin = currentUser?.Role == "SuperAdmin" || currentUser?.Role == "Admin";

            var history = await _db.YeuCauXuatDuLieus
                .Where(r => isSuperAdmin || r.NguoiYeuCau == userIdStr)
                .OrderByDescending(r => r.ThoiGianYeuCau)
                .Take(50)
                .ToListAsync(cancellationToken);

            return Ok(new { success = true, data = history });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadExportFile(string id, CancellationToken cancellationToken)
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userIdStr = currentUser?.UserId.ToString() ?? "Unknown";
            bool isSuperAdmin = currentUser?.Role == "SuperAdmin" || currentUser?.Role == "Admin";

            var request = await _db.YeuCauXuatDuLieus.FirstOrDefaultAsync(r => r.MaYeuCau == id, cancellationToken);
            
            if (request == null)
            {
                return NotFound(new { success = false, message = "Yêu cầu xuất báo cáo không tồn tại" });
            }

            if (!isSuperAdmin && request.NguoiYeuCau != userIdStr)
            {
                return Forbid();
            }

            string physicalPath = string.Empty;
            if (!string.IsNullOrEmpty(request.DuongDanFile))
            {
                physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", request.DuongDanFile.TrimStart('/'));
            }

            // Auto-regenerate on-the-fly if physical file is missing from disk (e.g. after container recreate)
            if (string.IsNullOrEmpty(physicalPath) || !System.IO.File.Exists(physicalPath))
            {
                physicalPath = await _exportQueueService.GenerateAndSaveReportDirectlyAsync(request, cancellationToken);
            }

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(new { success = false, message = "Không thể khởi tạo tệp báo cáo" });
            }

            var contentType = request.DinhDang == "excel" 
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" 
                : "application/pdf";
            var fileName = Path.GetFileName(physicalPath);

            return PhysicalFile(physicalPath, contentType, fileName);
        }

        private string GetTypeName(string type)
        {
            return type switch
            {
                "gradebook" => "Bảng điểm toàn kỳ",
                "attendance" => "Chuyên cần",
                "teacher_eval" => "Đánh giá giảng viên",
                "finance" => "Tài chính",
                "awards" => "Khen thưởng & Kỷ luật",
                _ => "Báo cáo tùy chỉnh"
            };
        }
    }

    public class ExportRequestDto
    {
        public string Type { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public string Format { get; set; } = "excel";
    }
}
