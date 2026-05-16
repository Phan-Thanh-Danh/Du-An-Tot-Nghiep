using Backend.DTOs.Common;
using Backend.DTOs.CourseSyllabuses;

namespace Backend.Services.CourseSyllabuses;

public interface ICourseSyllabusService
{
    Task<PagedResultDto<CourseSyllabusDto>> GetAsync(CourseSyllabusQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> GetByIdAsync(int syllabusId, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> CreateAsync(CreateCourseSyllabusRequest request, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> UpdateAsync(int syllabusId, UpdateCourseSyllabusRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int syllabusId, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> ActivateAsync(int syllabusId, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> DeactivateAsync(int syllabusId, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> ApproveAsync(int syllabusId, CancellationToken cancellationToken = default);
    Task<CourseSyllabusDto> ArchiveAsync(int syllabusId, CancellationToken cancellationToken = default);
}
