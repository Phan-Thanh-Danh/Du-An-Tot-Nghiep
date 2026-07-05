using Backend.DTOs.SmartTimetable;

namespace Backend.Services.ThoiKhoaBieu;

public interface ISmartTimetableService
{
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
