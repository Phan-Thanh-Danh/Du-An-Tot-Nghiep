using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = AuthRoles.Principal + "," + AuthRoles.AcademicStaff)]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ReportsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("courses")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetCourseReports()
    {
        var activeCourses = await _db.KhoaHocs
            .GroupBy(k => k.TrangThai)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync();

        var totalCourses = await _db.KhoaHocs.CountAsync();

        var data = new
        {
            TotalCourses = totalCourses,
            ByStatus = activeCourses
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAttendanceReports()
    {
        var attendanceStats = await _db.DiemDanhs
            .GroupBy(d => d.TrangThai)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync();

        var data = new
        {
            Stats = attendanceStats
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetAssignmentReports()
    {
        var totalSubmissions = await _db.BaiNops.CountAsync();
        var gradedSubmissions = await _db.BaiNops.CountAsync(b => b.DiemSo != null);

        var data = new
        {
            TotalSubmissions = totalSubmissions,
            GradedSubmissions = gradedSubmissions,
            UngradedSubmissions = totalSubmissions - gradedSubmissions
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("grades")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetGradeReports()
    {
        var passed = await _db.DiemSos.CountAsync(d => d.TrangThai == "passed");
        var failed = await _db.DiemSos.CountAsync(d => d.TrangThai == "failed");

        var data = new
        {
            PassedCount = passed,
            FailedCount = failed,
            TotalGrades = passed + failed
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("registrations")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetRegistrationReports()
    {
        var registrations = await _db.DangKyHocPhans
            .GroupBy(d => d.TrangThai)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync();

        var data = new
        {
            Stats = registrations
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("teacher-workload")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetTeacherWorkloadReports()
    {
        var workload = await _db.KhoaHocs
            .Where(k => k.GiaoVien != null)
            .GroupBy(k => new { k.MaGiaoVien, Name = k.GiaoVien!.HoTen })
            .Select(g => new { TeacherId = g.Key.MaGiaoVien, Name = g.Key.Name, CourseCount = g.Count() })
            .OrderByDescending(x => x.CourseCount)
            .Take(20)
            .AsNoTracking()
            .ToListAsync();

        var data = new
        {
            TopTeachers = workload
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }

    [HttpGet("rooms")]
    public async Task<ActionResult<ApiResponseDto<object>>> GetRoomReports()
    {
        var rooms = await _db.PhongHocs
            .GroupBy(p => p.LoaiPhong)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .AsNoTracking()
            .ToListAsync();

        var totalRooms = await _db.PhongHocs.CountAsync();

        var data = new
        {
            TotalRooms = totalRooms,
            ByType = rooms
        };

        return Ok(ApiResponseDto<object>.Ok(data));
    }
}
