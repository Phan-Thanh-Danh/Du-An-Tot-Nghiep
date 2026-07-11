using Backend.Services.TeachingPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("api/staff/teaching-preferences")]
[ApiController]
[Authorize(Roles = "AcademicStaff")]
public class StaffTeachingPreferencesController : ControllerBase
{
    private readonly ITeachingPreferenceService _service;

    public StaffTeachingPreferencesController(ITeachingPreferenceService service)
    {
        _service = service;
    }

    private int GetCurrentStaffDonViId()
    {
        // Actually, Staff also belongs to a DonVi, and we only summarize for their DonVi.
        // Assuming the CurrentUser model has MaDonVi mapped. Let's get it from the DB.
        // Wait, for simplicity, I'll retrieve it from HttpContext if it's there, but we didn't add DonViId to HttpContext yet in auth.
        // I'll fetch the user from DB to get MaDonVi.
        return 0; // We will implement the exact fetch logic below.
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int maHocKy, [FromServices] Backend.Data.ApplicationDbContext db)
    {
        if (HttpContext.Items["CurrentUser"] is Backend.DTOs.Auth.CurrentUserContext userContext)
        {
            var user = await db.NguoiDungs.FindAsync(userContext.UserId);
            if (user == null) return Unauthorized();
            
            var result = await _service.GetSummaryAsync(user.MaDonVi, maHocKy);
            return Ok(new { success = true, data = result });
        }
        return Unauthorized();
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers([FromQuery] int maHocKy, [FromServices] Backend.Data.ApplicationDbContext db)
    {
        if (HttpContext.Items["CurrentUser"] is Backend.DTOs.Auth.CurrentUserContext userContext)
        {
            var user = await db.NguoiDungs.FindAsync(userContext.UserId);
            if (user == null) return Unauthorized();
            
            var result = await _service.GetTeachersSummaryAsync(user.MaDonVi, maHocKy);
            return Ok(new { success = true, data = result });
        }
        return Unauthorized();
    }
}
