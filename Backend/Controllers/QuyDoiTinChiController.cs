using Backend.DTOs.Common;
using Backend.DTOs.QuyDoiTinChis;
using Backend.Services.QuyDoiTinChis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/quy-doi-tin-chi")]
[Authorize(Policy = "AcademicScheduleConfig")]
public class QuyDoiTinChiController : ControllerBase
{
    private readonly IQuyDoiTinChiService _service;

    public QuyDoiTinChiController(IQuyDoiTinChiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IReadOnlyList<QuyDoiTinChiDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(ApiResponseDto<IReadOnlyList<QuyDoiTinChiDto>>.Ok(items));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<QuyDoiTinChiDto>>> Create(CreateQuyDoiTinChiRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<QuyDoiTinChiDto>.Ok(item, "Tạo cấu hình quy đổi tín chỉ thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<QuyDoiTinChiDto>>> Update(int id, UpdateQuyDoiTinChiRequest request, CancellationToken cancellationToken)
    {
        var item = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponseDto<QuyDoiTinChiDto>.Ok(item, "Cập nhật cấu hình quy đổi tín chỉ thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponseDto<object>.Ok(null, "Xóa cấu hình quy đổi tín chỉ thành công."));
    }
}
