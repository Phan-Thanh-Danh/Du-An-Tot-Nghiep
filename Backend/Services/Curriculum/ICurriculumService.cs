using Backend.DTOs.Curriculum;

namespace Backend.Services.Curriculum;

public interface ICurriculumService
{
    // ─── Chapters ────────────────────────────────────────────
    Task<List<ChuongDto>> GetChaptersBySubjectAsync(int subjectId, CancellationToken ct = default);
    Task<ChuongDto> GetChapterByIdAsync(int chapterId, CancellationToken ct = default);
    Task<ChuongDto> CreateChapterAsync(ChuongCreateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task<ChuongDto> UpdateChapterAsync(int chapterId, ChuongUpdateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task DeleteChapterAsync(int chapterId, int userId, int? campusId, CancellationToken ct = default);

    // ─── Lessons ─────────────────────────────────────────────
    Task<List<BaiHocDto>> GetLessonsByChapterAsync(int chapterId, CancellationToken ct = default);
    Task<BaiHocDto> GetLessonByIdAsync(int lessonId, CancellationToken ct = default);
    Task<BaiHocDto> CreateLessonAsync(BaiHocCreateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task<BaiHocDto> UpdateLessonAsync(int lessonId, BaiHocUpdateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task DeleteLessonAsync(int lessonId, int userId, int? campusId, CancellationToken ct = default);

    // ─── Content Blocks ──────────────────────────────────────
    Task<List<BaiHocNoiDungDto>> GetContentByLessonAsync(int lessonId, CancellationToken ct = default);
    Task<BaiHocNoiDungDto> GetContentByIdAsync(int contentId, CancellationToken ct = default);
    Task<BaiHocNoiDungDto> CreateContentAsync(BaiHocNoiDungCreateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task<BaiHocNoiDungDto> UpdateContentAsync(int contentId, BaiHocNoiDungUpdateDto dto, int userId, int? campusId, CancellationToken ct = default);
    Task DeleteContentAsync(int contentId, int userId, int? campusId, CancellationToken ct = default);

    // ─── Reorder ─────────────────────────────────────────────
    Task ReorderChaptersAsync(int subjectId, ReorderRequestDto dto, CancellationToken ct = default);
    Task ReorderLessonsAsync(int chapterId, ReorderRequestDto dto, CancellationToken ct = default);
    Task ReorderContentAsync(int lessonId, ReorderRequestDto dto, CancellationToken ct = default);
}
