using Backend.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = $"{AuthRoles.Admin},{AuthRoles.SuperAdmin},{AuthRoles.CampusAdmin}")]
public class AccountManagementExampleController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAccounts()
    {
        return Ok(new { message = "This endpoint is protected by role-based authorization." });
    }
}
