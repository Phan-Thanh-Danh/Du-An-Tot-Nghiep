using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.DTOs.Courses;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class CourseService : ICourseService
{
    private const string DraftStatus = "nhap";
    private const string PublishedStatus = "da_xuat_ban";
    private const string ArchivedStatus = "luu_tru";

    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        DraftStatus,
        PublishedStatus,
        ArchivedStatus
    };

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;

    public CourseService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<KhoaHocDto>> GetAsync(
        KhoaHocQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var query = ApplyReadScope(CreateCourseQuery(), currentUser, allowedOrganizationIds);

        if (parameters.MaDonVi.HasValue)
        {
            if (!allowedOrganizationIds.Contains(parameters.MaDonVi.Value))
            {
                throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem khóa học của cơ sở này.");
            }

            query = query.Where(x => x.Course.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.MaMonHoc.HasValue)
        {
            query = query.Where(x => x.Course.MaMonHoc == parameters.MaMonHoc.Value);
        }

        if (parameters.MaGiaoVien.HasValue)
        {
            query = query.Where(x => x.Course.MaGiaoVien == parameters.MaGiaoVien.Value);
        }

        if (parameters.MaHocKy.HasValue)
        {
            query = query.Where(x => x.Course.MaHocKy == parameters.MaHocKy.Value);
        }

        if (parameters.MaLop.HasValue)
        {
            query = query.Where(x => x.Course.MaLop == parameters.MaLop.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.Course.TrangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Keyword))
        {
            var keyword = parameters.Keyword.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Course.TieuDe.ToLower().Contains(keyword) ||
                (x.Course.MoTa != null && x.Course.MoTa.ToLower().Contains(keyword)) ||
                x.Subject.MaCodeMonHoc.ToLower().Contains(keyword) ||
                x.Subject.TenMonHoc.ToLower().Contains(keyword) ||
                x.Teacher.HoTen.ToLower().Contains(keyword) ||
                x.Class.MaCodeLop.ToLower().Contains(keyword) ||
                x.Class.TenLop.ToLower().Contains(keyword) ||
                x.Organization.TenDonVi.ToLower().Contains(keyword) ||
                (x.Term != null && x.Term.TenHocKy.ToLower().Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Organization.TenDonVi)
            .ThenBy(x => x.Term == null ? string.Empty : x.Term.TenHocKy)
            .ThenBy(x => x.Subject.TenMonHoc)
            .ThenBy(x => x.Class.TenLop)
            .ThenBy(x => x.Teacher.HoTen)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => ToDto(x.Course, x.Organization, x.Subject, x.Teacher, x.Term, x.Class))
            .ToListAsync(cancellationToken);

        return new PagedResultDto<KhoaHocDto>
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<KhoaHocDetailDto> GetByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        var result = await ApplyReadScope(CreateCourseQuery(), currentUser, allowedOrganizationIds)
            .FirstOrDefaultAsync(x => x.Course.MaKhoaHoc == courseId, cancellationToken);

        if (result is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa học.");
        }

        var dto = ToDetailDto(result.Course, result.Organization, result.Subject, result.Teacher, result.Term, result.Class);
        dto.Chuongs = await GetCourseChaptersAsync(result.Course.MaMonHoc, cancellationToken);
        return dto;
    }

    public async Task<KhoaHocDto> CreateAsync(
        CreateKhoaHocRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var status = NormalizeStatus(string.IsNullOrWhiteSpace(request.TrangThai) ? DraftStatus : request.TrangThai);
        var organization = await ValidateOrganizationAsync(request.MaDonVi, currentUser, cancellationToken);
        var subject = await ValidateSubjectAsync(request.MaMonHoc, cancellationToken);
        var teacher = await ValidateTeacherAsync(request.MaGiaoVien, organization.MaDonVi, cancellationToken);
        var term = await ValidateTermAsync(request.MaHocKy, organization.MaDonVi, cancellationToken);
        var classEntity = await ValidateClassAsync(request.MaLop, organization.MaDonVi, cancellationToken);

        await ValidateUniqueCourseAsync(
            organization.MaDonVi,
            subject.MaMonHoc,
            term?.MaHocKy,
            classEntity.MaLop,
            null,
            cancellationToken);

        var title = NormalizeOptionalText(request.TieuDe)
            ?? BuildCourseTitle(subject, classEntity, term, teacher);

        var course = new KhoaHoc
        {
            MaDonVi = organization.MaDonVi,
            MaMonHoc = subject.MaMonHoc,
            MaGiaoVien = teacher.MaNguoiDung,
            MaHocKy = term?.MaHocKy,
            MaLop = classEntity.MaLop,
            MaLopHocPhan = null,
            TieuDe = title,
            MoTa = NormalizeOptionalText(request.MoTa),
            TrangThai = status,
            UrlAnhBia = NormalizeOptionalText(request.UrlAnhBia),
            NgayTao = DateTime.UtcNow
        };

        _context.KhoaHocs.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(course.MaKhoaHoc, cancellationToken);
        await WriteAuditAsync(
            "CREATE_KHOA_HOC",
            course,
            null,
            newSnapshot,
            currentUser,
            "Tạo khóa học.",
            cancellationToken);

        return ToDto(course, organization, subject, teacher, term, classEntity);
    }

    public async Task<BulkAssignCoursesResultDto> BulkAssignAsync(
        BulkAssignCoursesRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var classIds = NormalizeIds(request.MaLopIds, "Danh sách lớp không được để trống.");
        if (classIds.Count > 5)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được phân phối tối đa 5 lớp mỗi lần.");
        }

        var status = NormalizeStatus(string.IsNullOrWhiteSpace(request.TrangThai) ? DraftStatus : request.TrangThai);
        var subject = await ValidateSubjectAsync(request.MaMonHoc, cancellationToken);
        var teacher = await ValidateTeacherInManagedScopeAsync(request.MaGiaoVien, currentUser, cancellationToken);
        var organization = await ValidateOrganizationAsync(teacher.MaDonVi, currentUser, cancellationToken);
        var term = await ValidateTermAsync(request.MaHocKy, organization.MaDonVi, cancellationToken);

        var classes = await _context.LopHanhChinhs
            .AsNoTracking()
            .Where(x => classIds.Contains(x.MaLop))
            .ToListAsync(cancellationToken);

        var result = new BulkAssignCoursesResultDto();
        var now = DateTime.UtcNow;
        var coursesToCreate = new List<(KhoaHoc Course, LopHanhChinh ClassEntity)>();

        foreach (var classId in classIds)
        {
            var classEntity = classes.FirstOrDefault(x => x.MaLop == classId);
            if (classEntity is null || !classEntity.ConHoatDong)
            {
                result.Skipped.Add(new BulkAssignCourseSkippedDto
                {
                    MaLop = classId,
                    LyDo = "Lớp hành chính không tồn tại hoặc không hoạt động."
                });
                continue;
            }

            if (classEntity.MaDonVi != organization.MaDonVi)
            {
                result.Skipped.Add(new BulkAssignCourseSkippedDto
                {
                    MaLop = classEntity.MaLop,
                    TenLop = classEntity.TenLop,
                    LyDo = "Lớp hành chính không thuộc cùng cơ sở với giảng viên."
                });
                continue;
            }

            var exists = await CourseExistsAsync(
                organization.MaDonVi,
                subject.MaMonHoc,
                term?.MaHocKy,
                classEntity.MaLop,
                null,
                cancellationToken);

            if (exists)
            {
                result.Skipped.Add(new BulkAssignCourseSkippedDto
                {
                    MaLop = classEntity.MaLop,
                    TenLop = classEntity.TenLop,
                    LyDo = "Lớp đã có khóa học cho môn học này trong học kỳ đã chọn."
                });
                continue;
            }

            var title = NormalizeOptionalText(request.TieuDe)
                ?? BuildCourseTitle(subject, classEntity, term, teacher);

            coursesToCreate.Add((new KhoaHoc
            {
                MaDonVi = organization.MaDonVi,
                MaMonHoc = subject.MaMonHoc,
                MaGiaoVien = teacher.MaNguoiDung,
                MaHocKy = term?.MaHocKy,
                MaLop = classEntity.MaLop,
                MaLopHocPhan = null,
                TieuDe = title,
                MoTa = NormalizeOptionalText(request.MoTa),
                TrangThai = status,
                UrlAnhBia = NormalizeOptionalText(request.UrlAnhBia),
                NgayTao = now
            }, classEntity));
        }

        if (coursesToCreate.Count == 0)
        {
            return result;
        }

        _context.KhoaHocs.AddRange(coursesToCreate.Select(x => x.Course));
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var item in coursesToCreate)
        {
            result.Created.Add(ToDto(item.Course, organization, subject, teacher, term, item.ClassEntity));

            var newSnapshot = await GetAuditSnapshotAsync(item.Course.MaKhoaHoc, cancellationToken);
            await WriteAuditAsync(
                "BULK_ASSIGN_KHOA_HOC",
                item.Course,
                null,
                newSnapshot,
                currentUser,
                "Phân phối khóa học hàng loạt.",
                cancellationToken);
        }

        return result;
    }

    public async Task<KhoaHocDto> CloneAsync(
        int courseId,
        CloneCourseRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var source = await GetManagedCourseAsync(courseId, currentUser, cancellationToken);
        var subject = await ValidateSubjectAsync(source.MaMonHoc, cancellationToken);
        var teacher = await ValidateTeacherAsync(request.MaGiaoVien ?? source.MaGiaoVien, source.MaDonVi, cancellationToken);
        var term = await ValidateTermAsync(request.MaHocKy ?? source.MaHocKy, source.MaDonVi, cancellationToken);
        var classEntity = await ValidateClassAsync(request.MaLop ?? source.MaLop, source.MaDonVi, cancellationToken);
        var organization = await _context.DonVis.AsNoTracking().FirstAsync(x => x.MaDonVi == source.MaDonVi, cancellationToken);

        await ValidateUniqueCourseAsync(
            source.MaDonVi,
            source.MaMonHoc,
            term?.MaHocKy,
            classEntity.MaLop,
            null,
            cancellationToken);

        var title = NormalizeOptionalText(request.TieuDe) ?? $"{source.TieuDe} (Bản sao)";
        var clone = new KhoaHoc
        {
            MaDonVi = source.MaDonVi,
            MaMonHoc = source.MaMonHoc,
            MaGiaoVien = teacher.MaNguoiDung,
            MaHocKy = term?.MaHocKy,
            MaLop = classEntity.MaLop,
            MaLopHocPhan = null,
            TieuDe = title,
            MoTa = NormalizeOptionalText(request.MoTa) ?? source.MoTa,
            TrangThai = DraftStatus,
            UrlAnhBia = NormalizeOptionalText(request.UrlAnhBia) ?? source.UrlAnhBia,
            NgayTao = DateTime.UtcNow
        };

        _context.KhoaHocs.Add(clone);
        await _context.SaveChangesAsync(cancellationToken);

        var sourceSnapshot = await GetAuditSnapshotAsync(source.MaKhoaHoc, cancellationToken);
        var newSnapshot = await GetAuditSnapshotAsync(clone.MaKhoaHoc, cancellationToken);
        await WriteAuditAsync(
            "CLONE_KHOA_HOC",
            clone,
            sourceSnapshot,
            newSnapshot,
            currentUser,
            "Nhân bản khóa học.",
            cancellationToken);

        return ToDto(clone, organization, subject, teacher, term, classEntity);
    }

    public async Task<KhoaHocDto> UpdateAsync(
        int courseId,
        UpdateKhoaHocRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var course = await GetManagedCourseAsync(courseId, currentUser, cancellationToken);
        var oldSnapshot = await GetAuditSnapshotAsync(courseId, cancellationToken);
        var oldTeacherId = course.MaGiaoVien;
        var oldClassId = course.MaLop;
        var oldStatus = course.TrangThai;

        var teacher = await ValidateTeacherAsync(request.MaGiaoVien, course.MaDonVi, cancellationToken);
        var term = await ValidateTermAsync(request.MaHocKy, course.MaDonVi, cancellationToken);
        var classEntity = await ValidateClassAsync(request.MaLop, course.MaDonVi, cancellationToken);
        var title = NormalizeRequiredText(request.TieuDe, "Tiêu đề khóa học");
        var status = NormalizeStatus(request.TrangThai);

        await ValidateUniqueCourseAsync(
            course.MaDonVi,
            course.MaMonHoc,
            term?.MaHocKy,
            classEntity.MaLop,
            courseId,
            cancellationToken);

        course.MaGiaoVien = teacher.MaNguoiDung;
        course.MaHocKy = term?.MaHocKy;
        course.MaLop = classEntity.MaLop;
        course.TieuDe = title;
        course.MoTa = NormalizeOptionalText(request.MoTa);
        course.TrangThai = status;
        course.UrlAnhBia = NormalizeOptionalText(request.UrlAnhBia);

        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(courseId, cancellationToken);
        await WriteAuditAsync(
            "UPDATE_KHOA_HOC",
            course,
            oldSnapshot,
            newSnapshot,
            currentUser,
            "Cập nhật khóa học.",
            cancellationToken);

        if (oldTeacherId != course.MaGiaoVien)
        {
            await WriteAuditAsync(
                "CHANGE_KHOA_HOC_TEACHER",
                course,
                new { MaGiaoVien = oldTeacherId },
                new { MaGiaoVien = course.MaGiaoVien },
                currentUser,
                "Đổi giảng viên phụ trách khóa học.",
                cancellationToken);
        }

        if (oldClassId != course.MaLop)
        {
            await WriteAuditAsync(
                "CHANGE_KHOA_HOC_CLASS",
                course,
                new { MaLop = oldClassId },
                new { MaLop = course.MaLop },
                currentUser,
                "Đổi lớp hành chính của khóa học.",
                cancellationToken);
        }

        if (!string.Equals(oldStatus, course.TrangThai, StringComparison.OrdinalIgnoreCase))
        {
            await WriteAuditAsync(
                "CHANGE_KHOA_HOC_STATUS",
                course,
                new { TrangThai = oldStatus },
                new { TrangThai = course.TrangThai },
                currentUser,
                "Đổi trạng thái khóa học.",
                cancellationToken);
        }

        var subject = await _context.DanhMucMonHocs.AsNoTracking().FirstAsync(x => x.MaMonHoc == course.MaMonHoc, cancellationToken);
        var organization = await _context.DonVis.AsNoTracking().FirstAsync(x => x.MaDonVi == course.MaDonVi, cancellationToken);
        return ToDto(course, organization, subject, teacher, term, classEntity);
    }

    public async Task DeleteAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var course = await GetManagedCourseAsync(courseId, currentUser, cancellationToken);
        var oldSnapshot = await GetAuditSnapshotAsync(courseId, cancellationToken);
        course.TrangThai = ArchivedStatus;

        await _context.SaveChangesAsync(cancellationToken);

        var newSnapshot = await GetAuditSnapshotAsync(courseId, cancellationToken);
        await WriteAuditAsync(
            "DELETE_KHOA_HOC",
            course,
            oldSnapshot,
            newSnapshot,
            currentUser,
            "Lưu trữ khóa học.",
            cancellationToken);
    }

    public async Task<BatchCourseActionResultDto> BatchArchiveAsync(
        BatchCourseActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var ids = NormalizeIds(request.Ids, "Danh sách khóa học không được để trống.");
        var result = new BatchCourseActionResultDto();

        foreach (var id in ids)
        {
            try
            {
                var course = await GetManagedCourseAsync(id, currentUser, cancellationToken);
                var oldSnapshot = await GetAuditSnapshotAsync(id, cancellationToken);
                course.TrangThai = ArchivedStatus;

                await _context.SaveChangesAsync(cancellationToken);

                var newSnapshot = await GetAuditSnapshotAsync(id, cancellationToken);
                await WriteAuditAsync(
                    "BATCH_ARCHIVE_KHOA_HOC",
                    course,
                    oldSnapshot,
                    newSnapshot,
                    currentUser,
                    "Lưu trữ khóa học hàng loạt.",
                    cancellationToken);

                result.SuccessIds.Add(id);
            }
            catch (ApiException ex)
            {
                result.Failed.Add(new BatchCourseActionFailureDto { Id = id, LyDo = ex.Message });
            }
        }

        return result;
    }

    public async Task<BatchCourseActionResultDto> BatchPublishAsync(
        BatchCourseActionRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        EnsureCanManageCourses(currentUser);

        var ids = NormalizeIds(request.Ids, "Danh sách khóa học không được để trống.");
        var result = new BatchCourseActionResultDto();

        foreach (var id in ids)
        {
            try
            {
                var course = await GetManagedCourseAsync(id, currentUser, cancellationToken);
                if (string.Equals(course.TrangThai, ArchivedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    result.Failed.Add(new BatchCourseActionFailureDto
                    {
                        Id = id,
                        LyDo = "Không thể xuất bản khóa học đã lưu trữ."
                    });
                    continue;
                }

                var oldSnapshot = await GetAuditSnapshotAsync(id, cancellationToken);
                course.TrangThai = PublishedStatus;

                await _context.SaveChangesAsync(cancellationToken);

                var newSnapshot = await GetAuditSnapshotAsync(id, cancellationToken);
                await WriteAuditAsync(
                    "BATCH_PUBLISH_KHOA_HOC",
                    course,
                    oldSnapshot,
                    newSnapshot,
                    currentUser,
                    "Xuất bản khóa học hàng loạt.",
                    cancellationToken);

                result.SuccessIds.Add(id);
            }
            catch (ApiException ex)
            {
                result.Failed.Add(new BatchCourseActionFailureDto { Id = id, LyDo = ex.Message });
            }
        }

        return result;
    }

    private IQueryable<CourseQueryResult> CreateCourseQuery()
    {
        return
            from course in _context.KhoaHocs.AsNoTracking()
            join organization in _context.DonVis.AsNoTracking()
                on course.MaDonVi equals organization.MaDonVi
            join subject in _context.DanhMucMonHocs.AsNoTracking()
                on course.MaMonHoc equals subject.MaMonHoc
            join teacher in _context.NguoiDungs.AsNoTracking()
                on course.MaGiaoVien equals teacher.MaNguoiDung
            join classEntity in _context.LopHanhChinhs.AsNoTracking()
                on course.MaLop equals classEntity.MaLop
            join term in _context.HocKys.AsNoTracking()
                on course.MaHocKy equals term.MaHocKy into termJoin
            from term in termJoin.DefaultIfEmpty()
            select new CourseQueryResult
            {
                Course = course,
                Organization = organization,
                Subject = subject,
                Teacher = teacher,
                Term = term,
                Class = classEntity
            };
    }

    private IQueryable<CourseQueryResult> ApplyReadScope(
        IQueryable<CourseQueryResult> query,
        CurrentUserContext currentUser,
        HashSet<int> allowedOrganizationIds)
    {
        query = query.Where(x => allowedOrganizationIds.Contains(x.Course.MaDonVi));

        if (currentUser.Role == AuthRoles.Teacher)
        {
            query = query.Where(x => x.Course.MaGiaoVien == currentUser.UserId);
        }

        return query;
    }

    private async Task<KhoaHoc> GetManagedCourseAsync(
        int courseId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var course = await _context.KhoaHocs.FirstOrDefaultAsync(x => x.MaKhoaHoc == courseId, cancellationToken);
        if (course is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy khóa học.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, course.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý khóa học của cơ sở này.");
        }

        return course;
    }

    private async Task<DonVi> ValidateOrganizationAsync(
        int organizationId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var organization = await _context.DonVis
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDonVi == organizationId, cancellationToken);

        if (organization is null || !organization.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở không tồn tại hoặc không hoạt động.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, organizationId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý khóa học của cơ sở này.");
        }

        return organization;
    }

    private async Task<DanhMucMonHoc> ValidateSubjectAsync(int subjectId, CancellationToken cancellationToken)
    {
        var subject = await _context.DanhMucMonHocs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaMonHoc == subjectId, cancellationToken);

        if (subject is null || !subject.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Môn học không tồn tại hoặc không hoạt động.");
        }

        return subject;
    }

    private async Task<NguoiDung> ValidateTeacherAsync(
        int teacherId,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var teacher = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == teacherId, cancellationToken);

        if (teacher is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giảng viên không tồn tại.");
        }

        if (!await HasTeacherRoleAsync(teacher, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Người dùng được chọn không phải giảng viên.");
        }

        if (teacher.TrangThai == UserStatuses.DbLocked)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giảng viên đang bị khóa.");
        }

        if (teacher.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giảng viên không thuộc cơ sở đã chọn.");
        }

        return teacher;
    }

    private async Task<NguoiDung> ValidateTeacherInManagedScopeAsync(
        int teacherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        var teacher = await _context.NguoiDungs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == teacherId, cancellationToken);

        if (teacher is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giảng viên không tồn tại.");
        }

        if (!await HasTeacherRoleAsync(teacher, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Người dùng được chọn không phải giảng viên.");
        }

        if (teacher.TrangThai == UserStatuses.DbLocked)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Giảng viên đang bị khóa.");
        }

        if (!await CanAccessOrganizationAsync(currentUser, teacher.MaDonVi, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền phân phối khóa học cho cơ sở của giảng viên này.");
        }

        return teacher;
    }

    private async Task<bool> HasTeacherRoleAsync(NguoiDung teacher, CancellationToken cancellationToken)
    {
        var teacherRoleCode = AuthRoles.ToDatabaseCode(AuthRoles.Teacher);
        if (teacher.VaiTroChinh.Equals(teacherRoleCode, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return await _context.PhanQuyenNguoiDungs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaNguoiDung == teacher.MaNguoiDung &&
                x.VaiTro != null &&
                x.VaiTro.MaCodeVaiTro == teacherRoleCode,
                cancellationToken);
    }

    private async Task<HocKy?> ValidateTermAsync(
        int? termId,
        int organizationId,
        CancellationToken cancellationToken)
    {
        if (!termId.HasValue)
        {
            return null;
        }

        var term = await _context.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHocKy == termId.Value, cancellationToken);

        if (term is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không tồn tại.");
        }

        if (term.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không thuộc cơ sở đã chọn.");
        }

        return term;
    }

    private async Task<LopHanhChinh> ValidateClassAsync(
        int classId,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var classEntity = await _context.LopHanhChinhs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaLop == classId, cancellationToken);

        if (classEntity is null || !classEntity.ConHoatDong)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lớp hành chính không tồn tại hoặc không hoạt động.");
        }

        if (classEntity.MaDonVi != organizationId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lớp hành chính không thuộc cơ sở đã chọn.");
        }

        return classEntity;
    }

    private async Task ValidateUniqueCourseAsync(
        int organizationId,
        int subjectId,
        int? termId,
        int classId,
        int? excludedCourseId,
        CancellationToken cancellationToken)
    {
        if (await CourseExistsAsync(organizationId, subjectId, termId, classId, excludedCourseId, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Lớp này đã có khóa học cho môn học này trong học kỳ đã chọn.");
        }
    }

    private async Task<bool> CourseExistsAsync(
        int organizationId,
        int subjectId,
        int? termId,
        int classId,
        int? excludedCourseId,
        CancellationToken cancellationToken)
    {
        return await _context.KhoaHocs
            .AsNoTracking()
            .AnyAsync(x =>
                x.MaDonVi == organizationId &&
                x.MaMonHoc == subjectId &&
                x.MaHocKy == termId &&
                x.MaLop == classId &&
                (!excludedCourseId.HasValue || x.MaKhoaHoc != excludedCourseId.Value),
                cancellationToken);
    }

    private async Task<IReadOnlyList<KhoaHocChuongDto>> GetCourseChaptersAsync(
        int subjectId,
        CancellationToken cancellationToken)
    {
        var chapters = await _context.Chuongs
            .AsNoTracking()
            .Where(x => x.MaMonHoc == subjectId)
            .OrderBy(x => x.ThuTu)
            .ThenBy(x => x.MaChuong)
            .Select(x => new KhoaHocChuongDto
            {
                MaChuong = x.MaChuong,
                MaMonHoc = x.MaMonHoc,
                TieuDe = x.TieuDe,
                ThuTu = x.ThuTu,
                DaAn = x.DaAn
            })
            .ToListAsync(cancellationToken);

        var chapterIds = chapters.Select(x => x.MaChuong).ToList();
        if (chapterIds.Count == 0)
        {
            return chapters;
        }

        var lessons = await _context.BaiHocs
            .AsNoTracking()
            .Where(x => chapterIds.Contains(x.MaChuong))
            .OrderBy(x => x.ThuTu)
            .ThenBy(x => x.MaBaiHoc)
            .Select(x => new KhoaHocBaiHocDto
            {
                MaBaiHoc = x.MaBaiHoc,
                MaChuong = x.MaChuong,
                TieuDe = x.TieuDe,
                LoaiBaiHoc = x.LoaiBaiHoc,
                UrlTapTin = x.UrlTapTin,
                ThoiLuongGiay = x.ThoiLuongGiay,
                NoiDungVanBan = x.NoiDungVanBan,
                TomTatAi = x.TomTatAi,
                ThuTu = x.ThuTu,
                DaAn = x.DaAn
            })
            .ToListAsync(cancellationToken);

        var lessonsByChapter = lessons
            .GroupBy(x => x.MaChuong)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<KhoaHocBaiHocDto>)x.ToList());

        foreach (var chapter in chapters)
        {
            chapter.BaiHocs = lessonsByChapter.TryGetValue(chapter.MaChuong, out var chapterLessons)
                ? chapterLessons
                : [];
        }

        return chapters;
    }

    private async Task<object?> GetAuditSnapshotAsync(int courseId, CancellationToken cancellationToken)
    {
        return await CreateCourseQuery()
            .Where(x => x.Course.MaKhoaHoc == courseId)
            .Select(x => new
            {
                x.Course.MaKhoaHoc,
                x.Course.MaDonVi,
                TenDonVi = x.Organization.TenDonVi,
                x.Course.MaMonHoc,
                x.Subject.MaCodeMonHoc,
                x.Subject.TenMonHoc,
                x.Course.MaGiaoVien,
                TenGiaoVien = x.Teacher.HoTen,
                x.Course.MaHocKy,
                TenHocKy = x.Term == null ? null : x.Term.TenHocKy,
                x.Course.MaLop,
                x.Class.TenLop,
                x.Course.MaLopHocPhan,
                x.Course.TieuDe,
                x.Course.MoTa,
                x.Course.TrangThai,
                x.Course.UrlAnhBia,
                x.Course.NgayTao
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task WriteAuditAsync(
        string action,
        KhoaHoc course,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description,
        CancellationToken cancellationToken)
    {
        await _auditLogService.LogAsync(
            "KhoaHoc",
            course.MaKhoaHoc.ToString(),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            course.MaDonVi,
            description,
            cancellationToken);
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        return currentUser;
    }

    private static void EnsureCanManageCourses(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin or AuthRoles.AcademicStaff))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền quản lý khóa học.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return await _context.DonVis
                .AsNoTracking()
                .Select(x => x.MaDonVi)
                .ToHashSetAsync(cancellationToken);
        }

        if (currentUser.Role is AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin)
        {
            var organizations = await _context.DonVis
                .AsNoTracking()
                .Select(x => new { x.MaDonVi, x.MaDonViCha })
                .ToListAsync(cancellationToken);

            var allowedIds = new HashSet<int> { currentUser.CampusId };
            var queue = new Queue<int>();
            queue.Enqueue(currentUser.CampusId);

            while (queue.Count > 0)
            {
                var parentId = queue.Dequeue();
                foreach (var child in organizations.Where(x => x.MaDonViCha == parentId))
                {
                    if (allowedIds.Add(child.MaDonVi))
                    {
                        queue.Enqueue(child.MaDonVi);
                    }
                }
            }

            return allowedIds;
        }

        return new HashSet<int> { currentUser.CampusId };
    }

    private async Task<bool> CanAccessOrganizationAsync(
        CurrentUserContext currentUser,
        int organizationId,
        CancellationToken cancellationToken)
    {
        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        return allowedOrganizationIds.Contains(organizationId);
    }

    private static string NormalizeStatus(string value)
    {
        var status = NormalizeRequiredText(value, "Trạng thái").ToLowerInvariant();
        if (!ValidStatuses.Contains(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái khóa học không hợp lệ.");
        }

        return status;
    }

    private static string NormalizeRequiredText(string value, string fieldName)
    {
        var normalizedValue = value.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalizedValue;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static List<int> NormalizeIds(IEnumerable<int>? ids, string emptyMessage)
    {
        var normalizedIds = ids?
            .Where(x => x > 0)
            .Distinct()
            .ToList() ?? [];

        if (normalizedIds.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, emptyMessage);
        }

        return normalizedIds;
    }

    private static string BuildCourseTitle(
        DanhMucMonHoc subject,
        LopHanhChinh classEntity,
        HocKy? term,
        NguoiDung teacher)
    {
        var termName = term?.TenHocKy ?? "Chưa chọn học kỳ";
        return $"{subject.TenMonHoc} - {classEntity.TenLop} - {termName} - {teacher.HoTen}";
    }

    private static KhoaHocDto ToDto(
        KhoaHoc course,
        DonVi organization,
        DanhMucMonHoc subject,
        NguoiDung teacher,
        HocKy? term,
        LopHanhChinh classEntity)
    {
        return new KhoaHocDto
        {
            MaKhoaHoc = course.MaKhoaHoc,
            MaDonVi = course.MaDonVi,
            TenDonVi = organization.TenDonVi,
            MaMonHoc = course.MaMonHoc,
            TenMonHoc = subject.TenMonHoc,
            MaMonHocCode = subject.MaCodeMonHoc,
            MaGiaoVien = course.MaGiaoVien,
            TenGiaoVien = teacher.HoTen,
            MaHocKy = course.MaHocKy,
            TenHocKy = term?.TenHocKy,
            MaLop = course.MaLop,
            TenLop = classEntity.TenLop,
            TieuDe = course.TieuDe,
            MoTa = course.MoTa,
            TrangThai = course.TrangThai,
            UrlAnhBia = course.UrlAnhBia,
            NgayTao = course.NgayTao
        };
    }

    private static KhoaHocDetailDto ToDetailDto(
        KhoaHoc course,
        DonVi organization,
        DanhMucMonHoc subject,
        NguoiDung teacher,
        HocKy? term,
        LopHanhChinh classEntity)
    {
        var dto = ToDto(course, organization, subject, teacher, term, classEntity);
        return new KhoaHocDetailDto
        {
            MaKhoaHoc = dto.MaKhoaHoc,
            MaDonVi = dto.MaDonVi,
            TenDonVi = dto.TenDonVi,
            MaMonHoc = dto.MaMonHoc,
            TenMonHoc = dto.TenMonHoc,
            MaMonHocCode = dto.MaMonHocCode,
            MaGiaoVien = dto.MaGiaoVien,
            TenGiaoVien = dto.TenGiaoVien,
            MaHocKy = dto.MaHocKy,
            TenHocKy = dto.TenHocKy,
            MaLop = dto.MaLop,
            TenLop = dto.TenLop,
            TieuDe = dto.TieuDe,
            MoTa = dto.MoTa,
            TrangThai = dto.TrangThai,
            UrlAnhBia = dto.UrlAnhBia,
            NgayTao = dto.NgayTao
        };
    }

    private sealed class CourseQueryResult
    {
        public KhoaHoc Course { get; init; } = null!;
        public DonVi Organization { get; init; } = null!;
        public DanhMucMonHoc Subject { get; init; } = null!;
        public NguoiDung Teacher { get; init; } = null!;
        public HocKy? Term { get; init; }
        public LopHanhChinh Class { get; init; } = null!;
    }
}
