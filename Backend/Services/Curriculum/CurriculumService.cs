using Backend.Data;
using Backend.DTOs.Curriculum;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Curriculum;

public class CurriculumService : ICurriculumService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _audit;
    private readonly HtmlSanitizer _sanitizer;

    public CurriculumService(ApplicationDbContext context, IAuditLogService audit)
    {
        _context = context;
        _audit = audit;
        _sanitizer = new HtmlSanitizer();
        // Allow safe code display elements
        _sanitizer.AllowedTags.Add("pre");
        _sanitizer.AllowedTags.Add("code");
        _sanitizer.AllowedAttributes.Add("class");
    }

    // ═════════════════════════════════════════════════════════
    //  CHAPTERS
    // ═════════════════════════════════════════════════════════

    public async Task<List<ChuongDto>> GetChaptersBySubjectAsync(
        int subjectId,
        CancellationToken ct = default
    )
    {
        var subject =
            await _context
                .DanhMucMonHocs.AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaMonHoc == subjectId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học.");

        var chapters = await _context
            .Chuongs.AsNoTracking()
            .Include(c => c.MonHoc)
            .Include(c => c.BaiHocs.OrderBy(b => b.ThuTu))
                .ThenInclude(b => b.BaiHocNoiDungs.OrderBy(n => n.ThuTu))
            .Where(c => c.MaMonHoc == subjectId)
            .OrderBy(c => c.ThuTu)
            .ToListAsync(ct);

        return chapters.Select(MapChapter).ToList();
    }

    public async Task<ChuongDto> GetChapterByIdAsync(int chapterId, CancellationToken ct = default)
    {
        var chapter =
            await _context
                .Chuongs.AsNoTracking()
                .Include(c => c.MonHoc)
                .Include(c => c.BaiHocs.OrderBy(b => b.ThuTu))
                    .ThenInclude(b => b.BaiHocNoiDungs.OrderBy(n => n.ThuTu))
                .FirstOrDefaultAsync(c => c.MaChuong == chapterId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương.");

        return MapChapter(chapter);
    }

    public async Task<ChuongDto> CreateChapterAsync(
        ChuongCreateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        await ValidateSubjectExistsAsync(dto.MaMonHoc, ct);

        var maxOrder =
            await _context
                .Chuongs.Where(c => c.MaMonHoc == dto.MaMonHoc)
                .MaxAsync(c => (int?)c.ThuTu, ct)
            ?? 0;

        var entity = new Chuong
        {
            MaMonHoc = dto.MaMonHoc,
            TieuDe = dto.TieuDe.Trim(),
            ThuTu = dto.ThuTu ?? maxOrder + 1,
            DaAn = false,
            NgayTao = DateTime.UtcNow,
        };

        _context.Chuongs.Add(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Chuong",
            entity.MaChuong.ToString(),
            "Create",
            null,
            entity,
            userId,
            campusId,
            $"Tạo chương: {entity.TieuDe}",
            ct
        );

        return await GetChapterByIdAsync(entity.MaChuong, ct);
    }

    public async Task<ChuongDto> UpdateChapterAsync(
        int chapterId,
        ChuongUpdateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context.Chuongs.FirstOrDefaultAsync(c => c.MaChuong == chapterId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương.");

        var oldValue = new { entity.TieuDe, entity.DaAn };

        entity.TieuDe = dto.TieuDe.Trim();
        if (dto.DaAn.HasValue)
            entity.DaAn = dto.DaAn.Value;
        entity.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Chuong",
            chapterId.ToString(),
            "Update",
            oldValue,
            new { entity.TieuDe, entity.DaAn },
            userId,
            campusId,
            $"Cập nhật chương: {entity.TieuDe}",
            ct
        );

        return await GetChapterByIdAsync(chapterId, ct);
    }

    public async Task DeleteChapterAsync(
        int chapterId,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context
                .Chuongs.Include(c => c.BaiHocs)
                .FirstOrDefaultAsync(c => c.MaChuong == chapterId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương.");

        if (entity.BaiHocs.Count > 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                $"Không thể xóa chương đang có {entity.BaiHocs.Count} bài học. Hãy xóa hết bài học trước."
            );
        }

        _context.Chuongs.Remove(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "Chuong",
            chapterId.ToString(),
            "Delete",
            new { entity.TieuDe, entity.MaMonHoc },
            null,
            userId,
            campusId,
            $"Xóa chương: {entity.TieuDe}",
            ct
        );
    }

    // ═════════════════════════════════════════════════════════
    //  LESSONS
    // ═════════════════════════════════════════════════════════

    public async Task<List<BaiHocDto>> GetLessonsByChapterAsync(
        int chapterId,
        CancellationToken ct = default
    )
    {
        await ValidateChapterExistsAsync(chapterId, ct);

        return await _context
            .BaiHocs.AsNoTracking()
            .Where(b => b.MaChuong == chapterId)
            .OrderBy(b => b.ThuTu)
            .Select(b => new BaiHocDto
            {
                MaBaiHoc = b.MaBaiHoc,
                MaChuong = b.MaChuong,
                TieuDe = b.TieuDe,
                LoaiBaiHoc = b.LoaiBaiHoc,
                UrlTapTin = b.UrlTapTin,
                ThoiLuongGiay = b.ThoiLuongGiay,
                ThuTu = b.ThuTu,
                DaAn = b.DaAn,
                TrangThai = b.TrangThai,
                NgayTao = b.NgayTao,
                NgayCapNhat = b.NgayCapNhat,
                SoNoiDung = b.BaiHocNoiDungs.Count,
            })
            .ToListAsync(ct);
    }

    public async Task<BaiHocDto> GetLessonByIdAsync(int lessonId, CancellationToken ct = default)
    {
        var lesson =
            await _context
                .BaiHocs.AsNoTracking()
                .Include(b => b.BaiHocNoiDungs.OrderBy(n => n.ThuTu))
                .FirstOrDefaultAsync(b => b.MaBaiHoc == lessonId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bài học.");

        return MapLesson(lesson);
    }

    public async Task<BaiHocDto> CreateLessonAsync(
        BaiHocCreateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        await ValidateChapterExistsAsync(dto.MaChuong, ct);

        var maxOrder =
            await _context
                .BaiHocs.Where(b => b.MaChuong == dto.MaChuong)
                .MaxAsync(b => (int?)b.ThuTu, ct)
            ?? 0;

        var entity = new BaiHoc
        {
            MaChuong = dto.MaChuong,
            TieuDe = dto.TieuDe.Trim(),
            LoaiBaiHoc = dto.LoaiBaiHoc,
            ThuTu = dto.ThuTu ?? maxOrder + 1,
            DaAn = false,
            TrangThai = "nhap",
            NgayTao = DateTime.UtcNow,
        };

        _context.BaiHocs.Add(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHoc",
            entity.MaBaiHoc.ToString(),
            "Create",
            null,
            entity,
            userId,
            campusId,
            $"Tạo bài học: {entity.TieuDe}",
            ct
        );

        return await GetLessonByIdAsync(entity.MaBaiHoc, ct);
    }

    public async Task<BaiHocDto> UpdateLessonAsync(
        int lessonId,
        BaiHocUpdateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context.BaiHocs.FirstOrDefaultAsync(b => b.MaBaiHoc == lessonId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bài học.");

        var oldValue = new
        {
            entity.TieuDe,
            entity.LoaiBaiHoc,
            entity.TrangThai,
            entity.DaAn,
        };

        if (dto.TieuDe is not null)
            entity.TieuDe = dto.TieuDe.Trim();
        if (dto.LoaiBaiHoc is not null)
            entity.LoaiBaiHoc = dto.LoaiBaiHoc;
        if (dto.NoiDungVanBan is not null)
            entity.NoiDungVanBan = dto.NoiDungVanBan;
        if (dto.DieuKienMoKhoa is not null)
            entity.DieuKienMoKhoa = dto.DieuKienMoKhoa;
        if (dto.ThuTu.HasValue)
            entity.ThuTu = dto.ThuTu.Value;
        if (dto.DaAn.HasValue)
            entity.DaAn = dto.DaAn.Value;
        if (dto.TrangThai is not null)
            entity.TrangThai = dto.TrangThai;
        entity.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHoc",
            lessonId.ToString(),
            "Update",
            oldValue,
            new
            {
                entity.TieuDe,
                entity.LoaiBaiHoc,
                entity.TrangThai,
                entity.DaAn,
            },
            userId,
            campusId,
            $"Cập nhật bài học: {entity.TieuDe}",
            ct
        );

        return await GetLessonByIdAsync(lessonId, ct);
    }

    public async Task DeleteLessonAsync(
        int lessonId,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context
                .BaiHocs.Include(b => b.BaiHocNoiDungs)
                .FirstOrDefaultAsync(b => b.MaBaiHoc == lessonId, ct)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bài học.");

        // Cascade delete will remove BaiHocNoiDungs
        _context.BaiHocs.Remove(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHoc",
            lessonId.ToString(),
            "Delete",
            new { entity.TieuDe, entity.MaChuong },
            null,
            userId,
            campusId,
            $"Xóa bài học: {entity.TieuDe}",
            ct
        );
    }

    // ═════════════════════════════════════════════════════════
    //  CONTENT BLOCKS
    // ═════════════════════════════════════════════════════════

    public async Task<List<BaiHocNoiDungDto>> GetContentByLessonAsync(
        int lessonId,
        CancellationToken ct = default
    )
    {
        await ValidateLessonExistsAsync(lessonId, ct);

        return await _context
            .BaiHocNoiDungs.AsNoTracking()
            .Where(n => n.MaBaiHoc == lessonId)
            .OrderBy(n => n.ThuTu)
            .Select(n => MapContentProjection(n))
            .ToListAsync(ct);
    }

    public async Task<BaiHocNoiDungDto> GetContentByIdAsync(
        int contentId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context
                .BaiHocNoiDungs.AsNoTracking()
                .FirstOrDefaultAsync(n => n.MaNoiDung == contentId, ct)
            ?? throw new ApiException(
                StatusCodes.Status404NotFound,
                "Không tìm thấy nội dung bài học."
            );

        return MapContent(entity);
    }

    public async Task<BaiHocNoiDungDto> CreateContentAsync(
        BaiHocNoiDungCreateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        await ValidateLessonExistsAsync(dto.MaBaiHoc, ct);

        if (dto.LoaiNoiDung == "quiz")
        {
            if (!dto.MaDeKiemTra.HasValue)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Nội dung loại quiz bắt buộc phải có mã đề kiểm tra."
                );
            }
            await ValidateQuizExistsAndMatchesSubjectAsync(dto.MaDeKiemTra.Value, dto.MaBaiHoc, ct);
        }

        var maxOrder =
            await _context
                .BaiHocNoiDungs.Where(n => n.MaBaiHoc == dto.MaBaiHoc)
                .MaxAsync(n => (int?)n.ThuTu, ct)
            ?? 0;

        var entity = new BaiHocNoiDung
        {
            MaBaiHoc = dto.MaBaiHoc,
            LoaiNoiDung = dto.LoaiNoiDung,
            NoiDungHtml = SanitizeHtml(dto.NoiDungHtml),
            NoiDungJson = dto.NoiDungJson,
            UrlTapTin = dto.UrlTapTin,
            StorageKey = dto.StorageKey,
            KichThuocByte = dto.KichThuocByte,
            ThoiLuongGiay = dto.ThoiLuongGiay,
            MaDeKiemTra = dto.LoaiNoiDung == "quiz" ? dto.MaDeKiemTra : null,
            TrangThai = "nhap",
            ThuTu = dto.ThuTu ?? maxOrder + 1,
            NgayTao = DateTime.UtcNow,
        };

        _context.BaiHocNoiDungs.Add(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHocNoiDung",
            entity.MaNoiDung.ToString(),
            "Create",
            null,
            new { entity.LoaiNoiDung, entity.MaBaiHoc },
            userId,
            campusId,
            $"Tạo nội dung bài học (loại: {entity.LoaiNoiDung})",
            ct
        );

        return MapContent(entity);
    }

    public async Task<BaiHocNoiDungDto> UpdateContentAsync(
        int contentId,
        BaiHocNoiDungUpdateDto dto,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context.BaiHocNoiDungs.FirstOrDefaultAsync(n => n.MaNoiDung == contentId, ct)
            ?? throw new ApiException(
                StatusCodes.Status404NotFound,
                "Không tìm thấy nội dung bài học."
            );

        // Optimistic concurrency check
        if (
            dto.NgayCapNhat.HasValue
            && entity.NgayCapNhat.HasValue
            && dto.NgayCapNhat.Value != entity.NgayCapNhat.Value
        )
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "Nội dung đã bị người khác thay đổi. Vui lòng tải lại trang và thử lại."
            );
        }

        var oldValue = new { entity.LoaiNoiDung, entity.TrangThai };

        if (dto.NoiDungHtml is not null)
            entity.NoiDungHtml = SanitizeHtml(dto.NoiDungHtml);
        if (dto.NoiDungJson is not null)
            entity.NoiDungJson = dto.NoiDungJson;
        if (dto.UrlTapTin is not null)
            entity.UrlTapTin = dto.UrlTapTin;
        if (dto.StorageKey is not null)
            entity.StorageKey = dto.StorageKey;
        if (dto.KichThuocByte.HasValue)
            entity.KichThuocByte = dto.KichThuocByte;
        if (dto.ThoiLuongGiay.HasValue)
            entity.ThoiLuongGiay = dto.ThoiLuongGiay;
        if (dto.MaDeKiemTra.HasValue)
        {
            if (entity.LoaiNoiDung != "quiz")
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Chỉ nội dung loại quiz mới được gắn đề kiểm tra."
                );
            }
            await ValidateQuizExistsAndMatchesSubjectAsync(
                dto.MaDeKiemTra.Value,
                entity.MaBaiHoc,
                ct
            );
            entity.MaDeKiemTra = dto.MaDeKiemTra;
        }
        if (dto.TrangThai is not null)
            entity.TrangThai = dto.TrangThai;
        entity.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHocNoiDung",
            contentId.ToString(),
            "Update",
            oldValue,
            new { entity.LoaiNoiDung, entity.TrangThai },
            userId,
            campusId,
            $"Cập nhật nội dung bài học (loại: {entity.LoaiNoiDung})",
            ct
        );

        return MapContent(entity);
    }

    public async Task DeleteContentAsync(
        int contentId,
        int userId,
        int? campusId,
        CancellationToken ct = default
    )
    {
        var entity =
            await _context.BaiHocNoiDungs.FirstOrDefaultAsync(n => n.MaNoiDung == contentId, ct)
            ?? throw new ApiException(
                StatusCodes.Status404NotFound,
                "Không tìm thấy nội dung bài học."
            );

        _context.BaiHocNoiDungs.Remove(entity);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            "BaiHocNoiDung",
            contentId.ToString(),
            "Delete",
            new
            {
                entity.LoaiNoiDung,
                entity.MaBaiHoc,
                entity.StorageKey,
            },
            null,
            userId,
            campusId,
            $"Xóa nội dung bài học (loại: {entity.LoaiNoiDung})",
            ct
        );
    }

    // ═════════════════════════════════════════════════════════
    //  REORDER
    // ═════════════════════════════════════════════════════════

    public async Task ReorderChaptersAsync(
        int subjectId,
        ReorderRequestDto dto,
        CancellationToken ct = default
    )
    {
        var chapters = await _context.Chuongs.Where(c => c.MaMonHoc == subjectId).ToListAsync(ct);

        foreach (var item in dto.Items)
        {
            var chapter = chapters.FirstOrDefault(c => c.MaChuong == item.Id);
            if (chapter is not null)
                chapter.ThuTu = item.ThuTu;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ReorderLessonsAsync(
        int chapterId,
        ReorderRequestDto dto,
        CancellationToken ct = default
    )
    {
        var lessons = await _context.BaiHocs.Where(b => b.MaChuong == chapterId).ToListAsync(ct);

        foreach (var item in dto.Items)
        {
            var lesson = lessons.FirstOrDefault(b => b.MaBaiHoc == item.Id);
            if (lesson is not null)
                lesson.ThuTu = item.ThuTu;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task ReorderContentAsync(
        int lessonId,
        ReorderRequestDto dto,
        CancellationToken ct = default
    )
    {
        var contents = await _context
            .BaiHocNoiDungs.Where(n => n.MaBaiHoc == lessonId)
            .ToListAsync(ct);

        foreach (var item in dto.Items)
        {
            var content = contents.FirstOrDefault(n => n.MaNoiDung == item.Id);
            if (content is not null)
                content.ThuTu = item.ThuTu;
        }

        await _context.SaveChangesAsync(ct);
    }

    // ═════════════════════════════════════════════════════════
    //  HELPERS
    // ═════════════════════════════════════════════════════════

    private string? SanitizeHtml(string? html)
    {
        return html is null ? null : _sanitizer.Sanitize(html);
    }

    private async Task ValidateSubjectExistsAsync(int subjectId, CancellationToken ct)
    {
        var exists = await _context.DanhMucMonHocs.AnyAsync(s => s.MaMonHoc == subjectId, ct);
        if (!exists)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy môn học.");
    }

    private async Task ValidateChapterExistsAsync(int chapterId, CancellationToken ct)
    {
        var exists = await _context.Chuongs.AnyAsync(c => c.MaChuong == chapterId, ct);
        if (!exists)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy chương.");
    }

    private async Task ValidateLessonExistsAsync(int lessonId, CancellationToken ct)
    {
        var exists = await _context.BaiHocs.AnyAsync(b => b.MaBaiHoc == lessonId, ct);
        if (!exists)
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy bài học.");
    }

    private async Task ValidateQuizExistsAndMatchesSubjectAsync(
        int quizId,
        int lessonId,
        CancellationToken ct
    )
    {
        var lesson = await _context
            .BaiHocs.Include(b => b.Chuong)
            .FirstOrDefaultAsync(b => b.MaBaiHoc == lessonId, ct);

        if (lesson?.Chuong == null)
            return;

        var quiz = await _context.DeKiemTras.FirstOrDefaultAsync(q => q.MaDeKiemTra == quizId, ct);

        if (quiz == null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đề kiểm tra.");
        }

        if (quiz.MaMonHoc != lesson.Chuong.MaMonHoc)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "Đề kiểm tra không thuộc môn học của bài học này."
            );
        }
    }

    private static ChuongDto MapChapter(Chuong c) =>
        new()
        {
            MaChuong = c.MaChuong,
            MaMonHoc = c.MaMonHoc,
            TenMonHoc = c.MonHoc?.TenMonHoc ?? string.Empty,
            TieuDe = c.TieuDe,
            ThuTu = c.ThuTu,
            DaAn = c.DaAn,
            NgayTao = c.NgayTao,
            NgayCapNhat = c.NgayCapNhat,
            SoBaiHoc = c.BaiHocs.Count,
            BaiHocs = c.BaiHocs.Select(MapLesson).ToList(),
        };

    private static BaiHocDto MapLesson(BaiHoc b) =>
        new()
        {
            MaBaiHoc = b.MaBaiHoc,
            MaChuong = b.MaChuong,
            TieuDe = b.TieuDe,
            LoaiBaiHoc = b.LoaiBaiHoc,
            UrlTapTin = b.UrlTapTin,
            ThoiLuongGiay = b.ThoiLuongGiay,
            NoiDungVanBan = b.NoiDungVanBan,
            DieuKienMoKhoa = b.DieuKienMoKhoa,
            TomTatAi = b.TomTatAi,
            ThuTu = b.ThuTu,
            DaAn = b.DaAn,
            TrangThai = b.TrangThai,
            NgayTao = b.NgayTao,
            NgayCapNhat = b.NgayCapNhat,
            SoNoiDung = b.BaiHocNoiDungs.Count,
            NoiDungs = b.BaiHocNoiDungs.Select(MapContent).ToList(),
        };

    private static BaiHocNoiDungDto MapContent(BaiHocNoiDung n) =>
        new()
        {
            MaNoiDung = n.MaNoiDung,
            MaBaiHoc = n.MaBaiHoc,
            LoaiNoiDung = n.LoaiNoiDung,
            NoiDungHtml = n.NoiDungHtml,
            NoiDungJson = n.NoiDungJson,
            UrlTapTin = n.UrlTapTin,
            StorageKey = n.StorageKey,
            KichThuocByte = n.KichThuocByte,
            ThoiLuongGiay = n.ThoiLuongGiay,
            MaDeKiemTra = n.MaDeKiemTra,
            TrangThai = n.TrangThai,
            ThuTu = n.ThuTu,
            NgayTao = n.NgayTao,
            NgayCapNhat = n.NgayCapNhat,
        };

    private static BaiHocNoiDungDto MapContentProjection(BaiHocNoiDung n) => MapContent(n);
}
