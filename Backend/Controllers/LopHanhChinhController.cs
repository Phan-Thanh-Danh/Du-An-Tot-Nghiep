using Backend.DTOs.Common;
using Backend.DTOs.LopHanhChinhs;
using Backend.Services.LopHanhChinhs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/lop-hanh-chinh")]
[Authorize(Policy = "AcademicOperations")]
public class LopHanhChinhController : ControllerBase
{
    private readonly ILopHanhChinhService _lopHanhChinhService;

    public LopHanhChinhController(ILopHanhChinhService lopHanhChinhService)
    {
        _lopHanhChinhService = lopHanhChinhService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<LopHanhChinhDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChuyenNganh(
        [FromQuery] int maChuyenNganh,
        [FromQuery] bool conHoatDong = true,
        CancellationToken cancellationToken = default)
    {
        var items = await _lopHanhChinhService.GetByChuyenNganhAsync(maChuyenNganh, conHoatDong, cancellationToken);
        return Ok(ApiResponseDto<IEnumerable<LopHanhChinhDto>>.Ok(items));
    }
}
