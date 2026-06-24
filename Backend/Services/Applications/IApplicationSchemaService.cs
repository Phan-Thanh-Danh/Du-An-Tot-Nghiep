using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationSchemaService
{
    IReadOnlyList<ApplicationTypeDto> GetTypes();
    IReadOnlyList<ApplicationStatusDto> GetStatuses();
    Task<IReadOnlyList<ApplicationTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<ApplicationTemplateDto> GetActiveTemplateByTypeAsync(string loaiDon, CancellationToken cancellationToken = default);
}
