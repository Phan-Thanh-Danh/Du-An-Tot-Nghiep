using Backend.DTOs.Common;
using Backend.DTOs.RewardDiscipline;

namespace Backend.Services.RewardDiscipline;

public interface ICertificateTemplateService
{
    Task<PagedResultDto<CertificateTemplateDto>> GetAsync(
        CertificateTemplateQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<CertificateTemplateDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CertificateTemplateDto> CreateAsync(
        CreateCertificateTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<CertificateTemplateDto> UpdateAsync(
        int id,
        UpdateCertificateTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<CertificateTemplateDto> DisableAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CertificateTemplatePreviewDto> PreviewAsync(
        int id,
        CertificateTemplatePreviewRequest request,
        CancellationToken cancellationToken = default);
}
