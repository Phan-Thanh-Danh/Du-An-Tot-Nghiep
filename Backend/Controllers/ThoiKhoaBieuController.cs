using Backend.DTOs.Common;
using Backend.DTOs.BuoiHoc;
using Backend.DTOs.SmartTimetable;
using Backend.DTOs.SmartTimetable.Suggestions;
using Backend.DTOs.ThoiKhoaBieu;
using Backend.Services.BuoiHoc;
using Backend.Services.ThoiKhoaBieu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/thoi-khoa-bieu")]
[Authorize(Policy = "AcademicOperations")]
public class ThoiKhoaBieuController : ControllerBase
{
    private readonly IThoiKhoaBieuService _thoiKhoaBieuService;
    private readonly IScheduleConflictService _scheduleConflictService;
    private readonly IBuoiHocService _buoiHocService;
    private readonly ISmartTimetableService _smartTimetableService;

    public ThoiKhoaBieuController(
        IThoiKhoaBieuService thoiKhoaBieuService,
        IScheduleConflictService scheduleConflictService,
        IBuoiHocService buoiHocService,
        ISmartTimetableService smartTimetableService)
    {
        _thoiKhoaBieuService = thoiKhoaBieuService;
        _scheduleConflictService = scheduleConflictService;
        _buoiHocService = buoiHocService;
        _smartTimetableService = smartTimetableService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ThoiKhoaBieuDto>>>> Get(
        [FromQuery] ThoiKhoaBieuQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var schedules = await _thoiKhoaBieuService.GetAsync(parameters, cancellationToken);
        return Ok(ApiResponseDto<PagedResultDto<ThoiKhoaBieuDto>>.Ok(schedules));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule));
    }

    [HttpPost("check-xung-dot")]
    public async Task<ActionResult<ApiResponseDto<ScheduleConflictResultDto>>> CheckConflicts(
        CheckScheduleConflictRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _scheduleConflictService.CheckConflictsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<ScheduleConflictResultDto>.Ok(
            result,
            "Kiểm tra xung đột thời khóa biểu thành công"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Create(
        CreateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _thoiKhoaBieuService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = schedule.MaTkb },
                ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Tạo thời khóa biểu thành công"));
        }
        catch (ScheduleConflictException exception)
        {
            return ToConflictResponse(exception);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Update(
        int id,
        UpdateThoiKhoaBieuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _thoiKhoaBieuService.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Cập nhật thời khóa biểu thành công"));
        }
        catch (ScheduleConflictException exception)
        {
            return ToConflictResponse(exception);
        }
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<ActionResult<ApiResponseDto<ThoiKhoaBieuDetailDto>>> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var schedule = await _thoiKhoaBieuService.CancelAsync(id, cancellationToken);
        return Ok(ApiResponseDto<ThoiKhoaBieuDetailDto>.Ok(schedule, "Hủy thời khóa biểu thành công"));
    }

    [HttpPost("{id:int}/generate-sessions")]
    public async Task<ActionResult<ApiResponseDto<GenerateSessionsResultDto>>> GenerateSessions(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _buoiHocService.GenerateSessionsAsync(id, cancellationToken);
        return Ok(ApiResponseDto<GenerateSessionsResultDto>.Ok(result, "Sinh buổi học thành công"));
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ApiResponseDto<ScheduleDraftDto>>> Generate(
        GenerateTimetableRequest request,
        CancellationToken cancellationToken)
    {
        var draft = await _smartTimetableService.GenerateAsync(request, cancellationToken);
        return Ok(ApiResponseDto<ScheduleDraftDto>.Ok(draft, "Sinh thời khóa biểu thông minh thành công."));
    }

    [HttpGet("drafts/{draftId:guid}")]
    public async Task<ActionResult<ApiResponseDto<ScheduleDraftDto>>> GetDraft(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var draft = await _smartTimetableService.GetDraftAsync(draftId, cancellationToken);
        return Ok(ApiResponseDto<ScheduleDraftDto>.Ok(draft));
    }

    [HttpGet("drafts")]
    public async Task<ActionResult<ApiResponseDto<List<ScheduleDraftDto>>>> ListDrafts(
        [FromQuery] int maDonVi,
        [FromQuery] int maHocKy,
        CancellationToken cancellationToken)
    {
        var drafts = await _smartTimetableService.ListDraftsAsync(maDonVi, maHocKy, cancellationToken);
        return Ok(ApiResponseDto<List<ScheduleDraftDto>>.Ok(drafts));
    }

    [HttpPost("publish")]
    public async Task<ActionResult<ApiResponseDto<TimetablePublishResultDto>>> Publish(
        PublishTimetableRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _smartTimetableService.PublishAsync(request, cancellationToken);
        return Ok(ApiResponseDto<TimetablePublishResultDto>.Ok(result, result.Success ? "Xuất bản thời khóa biểu thành công." : "Xuất bản có lỗi."));
    }

    [HttpPost("check-xung-dot-batch")]
    public async Task<ActionResult<ApiResponseDto<ConflictCheckBatchResultDto>>> CheckConflictsBatch(
        ConflictCheckBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _smartTimetableService.CheckConflictsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<ConflictCheckBatchResultDto>.Ok(result));
    }

    [HttpDelete("drafts/{draftId:guid}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteDraft(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        var deleted = await _smartTimetableService.DeleteDraftAsync(draftId, cancellationToken);
        if (!deleted)
            return NotFound(ApiResponseDto.Fail("Không tìm thấy bản nháp."));
        return Ok(ApiResponseDto<bool>.Ok(true, "Xóa bản nháp thành công."));
    }

    [HttpPost("suggest-slots")]
    public async Task<ActionResult<ApiResponseDto<CourseSlotSuggestionResultDto>>> SuggestSlots(
        SuggestScheduleSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _smartTimetableService.SuggestSlotsAsync(request, cancellationToken);
        return Ok(ApiResponseDto<CourseSlotSuggestionResultDto>.Ok(result, "Lấy danh sách gợi ý thành công."));
    }

    [HttpPost("suggest-slots-batch")]
    public async Task<ActionResult<ApiResponseDto<BatchSlotSuggestionResultDto>>> SuggestSlotsBatch(
        SuggestScheduleSlotsBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _smartTimetableService.SuggestSlotsBatchAsync(request, cancellationToken);
        return Ok(ApiResponseDto<BatchSlotSuggestionResultDto>.Ok(result, "Tính toán gợi ý hàng loạt thành công."));
    }

    [HttpGet("khoa-hoc/{maKhoaHoc}/tien-do-buoi")]
    [ProducesResponseType(typeof(ApiResponseDto<TienDoBuoiHocDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTienDoBuoiHoc(
        [FromRoute] int maKhoaHoc,
        CancellationToken cancellationToken)
    {
        var result = await _thoiKhoaBieuService.GetTienDoBuoiHocAsync(maKhoaHoc, cancellationToken);
        return Ok(ApiResponseDto<TienDoBuoiHocDto>.Ok(result));
    }

    private ConflictObjectResult ToConflictResponse(ScheduleConflictException exception)
    {
        return Conflict(new ApiResponseDto<ScheduleConflictResultDto>
        {
            Success = false,
            Message = "Thời khóa biểu bị xung đột.",
            Data = exception.Result,
            Errors = exception.Result.Conflicts
                .Select(x => x.Message)
                .Distinct()
                .ToList()
        });
    }
}
