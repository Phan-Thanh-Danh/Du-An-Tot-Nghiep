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
