using Backend.DTOs.Common;
using Backend.DTOs.Finance.ProgramTuitionConfigs;

namespace Backend.Services.Finance.ProgramTuitionConfigs;

public interface IProgramTuitionConfigService
{
    Task<PagedResultDto<ProgramTuitionConfigListItemDto>> GetAsync(
        ProgramTuitionConfigQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<ProgramTuitionConfigOptionsDto> GetOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<ProgramTuitionConfigDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ProgramTuitionConfigDetailDto> CreateAsync(
        CreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default);

    Task<BulkProgramTuitionConfigResultDto> BulkCreateAsync(
        BulkCreateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default);

    Task<ProgramTuitionConfigDetailDto> UpdateAsync(
        int id,
        UpdateProgramTuitionConfigRequest request,
        CancellationToken cancellationToken = default);

    Task<ProgramTuitionConfigDetailDto> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);
}
