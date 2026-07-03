using Backend.DTOs.Common;
using Backend.DTOs.Courses;

namespace Backend.Services.Courses;

public interface ICourseService
{
    Task<PagedResultDto<KhoaHocDto>> GetAsync(KhoaHocQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<KhoaHocDetailDto> GetByIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task<KhoaHocDto> CreateAsync(CreateKhoaHocRequest request, CancellationToken cancellationToken = default);
    Task<BulkAssignCoursesResultDto> BulkAssignAsync(BulkAssignCoursesRequest request, CancellationToken cancellationToken = default);
    Task<KhoaHocDto> CloneAsync(int courseId, CloneCourseRequest request, CancellationToken cancellationToken = default);
    Task<KhoaHocDto> UpdateAsync(int courseId, UpdateKhoaHocRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int courseId, CancellationToken cancellationToken = default);
    Task<BatchCourseActionResultDto> BatchArchiveAsync(BatchCourseActionRequest request, CancellationToken cancellationToken = default);
    Task<BatchCourseActionResultDto> BatchPublishAsync(BatchCourseActionRequest request, CancellationToken cancellationToken = default);
}
