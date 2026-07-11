using Backend.DTOs.Blocks;
using Backend.DTOs.Common;
using Backend.Services.Blocks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/blocks")]
[Authorize(Policy = "AcademicScheduleConfig")]
public class BlockController : ControllerBase
{
    private readonly IBlockService _blockService;

    public BlockController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<BlockDto>>>> Get([FromQuery] int maHocKy, CancellationToken cancellationToken)
    {
        var blocks = await _blockService.GetByTermIdAsync(maHocKy, cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<BlockDto>>.Ok(blocks));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<BlockDto>>> Update(int id, UpdateBlockRequest request, CancellationToken cancellationToken)
    {
        var block = await _blockService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<BlockDto>.Ok(block, "Cập nhật ngày Block thành công."));
    }
}
