using Backend.Constants;
using Backend.DTOs.Auth;
using Backend.DTOs.Organizations;
using Backend.Exceptions;
using Backend.Services.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrganizationResponseDto>>> GetAll()
    {
        var organizations = await _organizationService.GetAllAsync();
        return Ok(organizations);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<OrganizationTreeDto>>> GetTree()
    {
        var organizations = await _organizationService.GetTreeAsync();
        return Ok(organizations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganizationResponseDto>> GetById(int id)
    {
        var organization = await _organizationService.GetByIdAsync(id);
        return Ok(organization);
    }

    [HttpPost]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<OrganizationResponseDto>> Create(OrganizationCreateDto request)
    {
        var organization = await _organizationService.CreateAsync(request, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = organization.Id }, organization);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<ActionResult<OrganizationResponseDto>> Update(int id, OrganizationUpdateDto request)
    {
        var organization = await _organizationService.UpdateAsync(id, request, GetCurrentUserId());
        return Ok(organization);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _organizationService.SoftDeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id:int}/hard-delete")]
    [Authorize(Roles = AuthRoles.SuperAdmin)]
    public async Task<IActionResult> HardDelete(int id)
    {
        await _organizationService.HardDeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }

    [HttpGet("{id:int}/subtree")]
    public async Task<ActionResult<OrganizationTreeDto>> GetSubtree(int id)
    {
        var organization = await _organizationService.GetSubtreeAsync(id);
        return Ok(organization);
    }

    private int GetCurrentUserId()
    {
        if (HttpContext.Items["CurrentUser"] is CurrentUserContext currentUser)
        {
            return currentUser.UserId;
        }

        throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
    }
}
