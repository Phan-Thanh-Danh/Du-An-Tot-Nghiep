using Backend.DTOs.Applications;

namespace Backend.Services.Applications;

public interface IApplicationSchemaService
{
    IReadOnlyList<ApplicationTypeDto> GetTypes();
    IReadOnlyList<ApplicationStatusDto> GetStatuses();
    Task<IReadOnlyList<ApplicationTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<ApplicationTemplateDto> GetActiveTemplateByTypeAsync(string loaiDon, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<ApplicationTemplateDto> CreateTemplateAsync(
        CreateApplicationTemplateRequest request,
        CancellationToken cancellationToken = default);
    Task<ApplicationTemplateDto> UpdateTemplateAsync(
        string loaiDon,
        UpdateApplicationTemplateRequest request,
        CancellationToken cancellationToken = default);
    Task<ApplicationTemplateDto> DeleteTemplateAsync(string loaiDon, CancellationToken cancellationToken = default);
}
