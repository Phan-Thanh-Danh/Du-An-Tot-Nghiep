using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "Teacher")]
public class TeacherExamResultsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeacherExamResultsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("exam-results")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetExamResults()
    {
        try
        {
            var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserContext;
            var userId = currentUser!.UserId;

            var assignments = await _context.PhanCongGiamThis
                .Where(pcg => pcg.MaGiamThi == userId)
                .Select(pcg => new
                {
                    pcg.MaCaThi,
                    TenCaThi = pcg.CaThi != null ? pcg.CaThi.TenCaThi : "",
                    MaMonHoc = pcg.CaThi != null && pcg.CaThi.LichThiTong != null ? pcg.CaThi.LichThiTong.MaMonHoc : (int?)null,
                    MaHocKy = pcg.CaThi != null && pcg.CaThi.LichThiTong != null && pcg.CaThi.LichThiTong.KyThi != null ? pcg.CaThi.LichThiTong.KyThi.MaHocKy : (int?)null,
                    SubjectName = pcg.CaThi != null && pcg.CaThi.LichThiTong != null && pcg.CaThi.LichThiTong.MonHoc != null ? pcg.CaThi.LichThiTong.MonHoc.TenMonHoc : "",
                    MaCodeMonHoc = pcg.CaThi != null && pcg.CaThi.LichThiTong != null && pcg.CaThi.LichThiTong.MonHoc != null ? pcg.CaThi.LichThiTong.MonHoc.MaCodeMonHoc : "",
                    TotalStudents = pcg.CaThi != null ? pcg.CaThi.ThiSinhCaThis.Count : 0,
                    SubmittedCount = pcg.CaThi != null ? pcg.CaThi.ThiSinhCaThis.Count(ts => ts.TrangThaiDuThi != "cho_thi") : 0
                })
                .ToListAsync();

            var monHocIds = assignments.Where(a => a.MaMonHoc.HasValue).Select(a => a.MaMonHoc!.Value).Distinct().ToList();
            var hocKyIds = assignments.Where(a => a.MaHocKy.HasValue).Select(a => a.MaHocKy!.Value).Distinct().ToList();

            var classNames = new Dictionary<int, string>();
            if (monHocIds.Count > 0)
            {
                classNames = await _context.KhoaHocs
                    .Where(k => k.MaGiaoVien == userId && monHocIds.Contains(k.MaMonHoc) && k.Lop != null)
                    .Select(k => new { k.MaMonHoc, TenLop = k.Lop!.TenLop })
                    .Distinct()
                    .ToDictionaryAsync(k => k.MaMonHoc, k => k.TenLop);
            }

            var avgScores = new Dictionary<(int, int), decimal>();
            if (monHocIds.Count > 0 && hocKyIds.Count > 0)
            {
                avgScores = await _context.DiemSos
                    .Where(ds => monHocIds.Contains(ds.MaMonHoc) && hocKyIds.Contains(ds.MaHocKy))
                    .GroupBy(ds => new { ds.MaMonHoc, ds.MaHocKy })
                    .Select(g => new { g.Key.MaMonHoc, g.Key.MaHocKy, Avg = g.Average(ds => ds.GpaMonHoc) })
                    .ToDictionaryAsync(k => (k.MaMonHoc, k.MaHocKy), k => k.Avg);
            }

            var result = assignments.Select(a => new
            {
                ExamId = a.MaCaThi,
                ExamTitle = a.TenCaThi,
                Subject = a.SubjectName,
                SubjectCode = a.MaCodeMonHoc,
                ClassName = a.MaMonHoc.HasValue && classNames.ContainsKey(a.MaMonHoc.Value) ? classNames[a.MaMonHoc.Value] : "",
                TotalStudents = a.TotalStudents,
                SubmittedCount = a.SubmittedCount,
                AvgScore = a.MaMonHoc.HasValue && a.MaHocKy.HasValue && avgScores.ContainsKey((a.MaMonHoc.Value, a.MaHocKy.Value))
                    ? avgScores[(a.MaMonHoc.Value, a.MaHocKy.Value)]
                    : 0m
            }).ToList();

            return Ok(ApiResponseDto<object>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.Fail("Lỗi khi tải kết quả thi: " + ex.Message));
        }
    }
}
