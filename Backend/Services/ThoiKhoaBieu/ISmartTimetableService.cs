using Backend.DTOs.SmartTimetable;
using Backend.DTOs.SmartTimetable.Suggestions;

namespace Backend.Services.ThoiKhoaBieu;

public interface ISmartTimetableService
{
    Task<CourseSlotSuggestionResultDto> SuggestSlotsAsync(
        SuggestScheduleSlotsRequest request,
        CancellationToken cancellationToken = default);
        
    Task<BatchSlotSuggestionResultDto> SuggestSlotsBatchAsync(
        SuggestScheduleSlotsBatchRequest request,
        CancellationToken cancellationToken = default);
    Task<ScheduleDraftDto> GenerateAsync(
        GenerateTimetableRequest request,
        CancellationToken cancellationToken = default);

    Task<ScheduleDraftDto> GetDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default);

    Task<List<ScheduleDraftDto>> ListDraftsAsync(
        int maDonVi,
        int maHocKy,
        CancellationToken cancellationToken = default);

    Task<TimetablePublishResultDto> PublishAsync(
        PublishTimetableRequest request,
        CancellationToken cancellationToken = default);

    Task<ConflictCheckBatchResultDto> CheckConflictsAsync(
        ConflictCheckBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteDraftAsync(
        Guid draftId,
        CancellationToken cancellationToken = default);
}
