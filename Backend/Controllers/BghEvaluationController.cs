using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/bgh")]
[Authorize]
public class BghEvaluationController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public BghEvaluationController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("evaluations")]
    public async Task<ActionResult<ApiResponseDto<BghEvaluationDto>>> GetEvaluations()
    {
        var totalTeachers = await _db.NguoiDungs.CountAsync(u => u.VaiTroChinh == "giao_vien");

        // TODO: Replace with actual evaluation data when evaluation module schema is finalized.
        // DanhGiaGiaoVien entity exists with DiemSo, MaGiaoVien, MaHocKy fields.
        // Example:
        //   var avgRating = await _db.DanhGiaGiaoViens.AverageAsync(g => (double?)g.DiemSo) ?? 0;
        //   var rankings  = await _db.DanhGiaGiaoViens
        //       .GroupBy(g => g.MaGiaoVien)
        //       .Select(g => new TeacherRankingDto { ... })
        //       .ToListAsync();

        var data = new BghEvaluationDto
        {
            Rankings = [],
            Overview = new EvaluationOverviewDto
            {
                TotalTeachers = totalTeachers,
                AvgRating = 0
            }
        };

        return Ok(ApiResponseDto<BghEvaluationDto>.Ok(data));
    }
}

public class BghEvaluationDto
{
    public List<object> Rankings { get; set; } = [];
    public EvaluationOverviewDto Overview { get; set; } = new();
}

public class EvaluationOverviewDto
{
    public int TotalTeachers { get; set; }
    public double AvgRating { get; set; }
}
