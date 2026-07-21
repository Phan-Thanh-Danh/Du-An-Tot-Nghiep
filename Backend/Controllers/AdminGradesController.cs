using Backend.Data;
using Backend.Constants;
using Backend.Services.Grading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Exceptions;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/grades")]
[Authorize(Roles = AuthRoles.SuperAdmin)]
public class AdminGradesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IGradeAggregationService _gradeAggregationService;
    private readonly ILogger<AdminGradesController> _logger;

    public AdminGradesController(ApplicationDbContext db, IGradeAggregationService gradeAggregationService, ILogger<AdminGradesController> logger)
    {
        _db = db;
        _gradeAggregationService = gradeAggregationService;
        _logger = logger;
    }

    /// <summary>
    /// Recalculates grades for all students across all subjects and terms that already have a grade record.
    /// This is an administrative endpoint used to force a recalculation after configuration changes.
    /// </summary>
    [HttpPost("recalculate-all")]
    public async Task<IActionResult> RecalculateAllGrades(CancellationToken ct)
    {
        // Tìm tất cả các học sinh đang học trong các lớp có mở môn học (KhoaHoc)
        var allGrades = await _db.NguoiDungs
            .Where(n => n.VaiTroChinh == "hoc_sinh" && n.MaLop != null)
            .Join(_db.KhoaHocs,
                student => student.MaLop,
                course => course.MaLop,
                (student, course) => new { MaHocSinh = student.MaNguoiDung, course.MaMonHoc, course.MaHocKy })
            .Where(x => x.MaHocKy != null)
            .Select(x => new { x.MaHocSinh, x.MaMonHoc, MaHocKy = x.MaHocKy.Value })
            .Distinct()
            .ToListAsync(ct);

        int success = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var g in allGrades)
        {
            try
            {
                // We use CancellationToken.None here to avoid cancelling the whole loop if the request is aborted
                await _gradeAggregationService.CalculateGradeAsync(g.MaHocSinh, g.MaMonHoc, g.MaHocKy, CancellationToken.None);
                success++;
            }
            catch (ApiException ex) when (ex.StatusCode == 400)
            {
                // Typically missing configuration
                _logger.LogWarning(ex, "Skipped recalculation for Student {StudentId}, Subject {SubjectId}, Term {TermId}: {Message}", 
                    g.MaHocSinh, g.MaMonHoc, g.MaHocKy, ex.Message);
                skipped++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recalculating grade for Student {StudentId}, Subject {SubjectId}, Term {TermId}", 
                    g.MaHocSinh, g.MaMonHoc, g.MaHocKy);
                errors++;
            }
        }

        return Ok(new
        {
            success = true,
            message = "Hoàn tất quá trình tính toán lại điểm",
            data = new
            {
                total = allGrades.Count,
                success,
                skipped,
                errors
            }
        });
    }
}
