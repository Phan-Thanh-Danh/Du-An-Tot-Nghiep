using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IDisciplineRecordService
{
    Task<PagedResultDto<DisciplineRecordListItemDto>> GetDisciplineRecordsAsync(
        DisciplineRecordQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DisciplineRecordDetailDto> GetDisciplineRecordDetailAsync(
        int recordId,
        CancellationToken cancellationToken = default);

    Task<DisciplineRecordResultDto> CreateDisciplineRecordAsync(
        CreateDisciplineRecordRequest request,
        CancellationToken cancellationToken = default);

    Task<DisciplineRecordResultDto> UpdateDisciplineRecordAsync(
        int recordId,
        UpdateDisciplineRecordRequest request,
        CancellationToken cancellationToken = default);

    Task<DisciplineRecordResultDto> SubmitDisciplineRecordAsync(
        int recordId,
        CancellationToken cancellationToken = default);

    Task<DisciplineRecordResultDto> CancelDisciplineRecordAsync(
        int recordId,
        CancelDisciplineRecordRequest request,
        CancellationToken cancellationToken = default);
}
