using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface IDisciplineAppealService
{
    Task<DisciplineAppealListItemDto> CreateDisciplineAppealAsync(
        int recordId,
        CreateDisciplineAppealRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<DisciplineAppealListItemDto>> GetDisciplineAppealsAsync(
        DisciplineAppealQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<DisciplineAppealDetailDto> GetDisciplineAppealDetailAsync(
        int appealId,
        CancellationToken cancellationToken = default);

    Task<DisciplineAppealDetailDto> ResolveDisciplineAppealAsync(
        int appealId,
        ResolveDisciplineAppealRequest request,
        CancellationToken cancellationToken = default);
}
