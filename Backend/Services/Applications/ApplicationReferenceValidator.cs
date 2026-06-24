using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationReferenceValidator : IApplicationReferenceValidator
{
    private readonly ApplicationDbContext _context;

    public ApplicationReferenceValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ValidateAsync(
        NguoiDung student,
        ApplicationFormDataValidationResult formData,
        CancellationToken cancellationToken = default)
    {
        foreach (var reference in formData.RelatedEntities)
        {
            switch (reference.RelatedEntity)
            {
                case ApplicationRelatedEntities.AcademicTerm:
                    await EnsureAcademicTermAsync(reference.Id, student.MaDonVi, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Subject:
                    await EnsureSubjectAsync(reference.Id, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Grade:
                    await EnsureGradeAsync(reference.Id, student, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Organization:
                    await EnsureOrganizationAsync(reference.Id, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Major:
                    await EnsureMajorAsync(reference.Id, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Specialization:
                    await EnsureSpecializationAsync(reference.Id, student.MaDonVi, cancellationToken);
                    break;
                case ApplicationRelatedEntities.Course:
                    await EnsureCourseAccessAsync(reference.Id, student, cancellationToken);
                    break;
                case ApplicationRelatedEntities.ClassSession:
                    await EnsureSessionAccessAsync(reference.Id, student, cancellationToken);
                    break;
                default:
                    throw new ApiException(StatusCodes.Status400BadRequest, $"Related entity '{reference.RelatedEntity}' không được hỗ trợ.");
            }
        }

        await CrossCheckScoreAsync(student, formData, cancellationToken);
    }

    private async Task EnsureAcademicTermAsync(int id, int organizationId, CancellationToken cancellationToken)
    {
        var exists = await _context.HocKys.AsNoTracking()
            .AnyAsync(x => x.MaHocKy == id && x.MaDonVi == organizationId, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học kỳ không hợp lệ.");
        }
    }

    private async Task EnsureSubjectAsync(int id, CancellationToken cancellationToken)
    {
        var exists = await _context.DanhMucMonHocs.AsNoTracking()
            .AnyAsync(x => x.MaMonHoc == id && x.ConHoatDong, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Môn học không hợp lệ.");
        }
    }

    private async Task EnsureGradeAsync(int id, NguoiDung student, CancellationToken cancellationToken)
    {
        var exists = await _context.DiemSos.AsNoTracking()
            .AnyAsync(x => x.MaDiemSo == id && x.MaHocSinh == student.MaNguoiDung && x.MaDonVi == student.MaDonVi, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không hợp lệ.");
        }
    }

    private async Task EnsureOrganizationAsync(int id, CancellationToken cancellationToken)
    {
        var exists = await _context.DonVis.AsNoTracking()
            .AnyAsync(x => x.MaDonVi == id && x.ConHoatDong, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn vị không hợp lệ.");
        }
    }

    private async Task EnsureMajorAsync(int id, CancellationToken cancellationToken)
    {
        var exists = await _context.NganhDaoTaos.AsNoTracking()
            .AnyAsync(x => x.MaNganh == id && x.ConHoatDong, cancellationToken);
        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ngành đào tạo không hợp lệ.");
        }
    }

    private async Task EnsureSpecializationAsync(int id, int organizationId, CancellationToken cancellationToken)
    {
        var exists = await (
            from specialization in _context.ChuyenNganhs.AsNoTracking()
            join major in _context.NganhDaoTaos.AsNoTracking()
                on specialization.MaNganh equals major.MaNganh
            join campus in _context.ChuyenNganhTheoCoSos.AsNoTracking()
                on specialization.MaChuyenNganh equals campus.MaChuyenNganh
            where specialization.MaChuyenNganh == id &&
                  specialization.ConHoatDong &&
                  major.ConHoatDong &&
                  campus.MaDonVi == organizationId &&
                  campus.ConHoatDong
            select specialization.MaChuyenNganh).AnyAsync(cancellationToken);

        if (!exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chuyên ngành không hợp lệ.");
        }
    }

    private async Task EnsureCourseAccessAsync(int id, NguoiDung student, CancellationToken cancellationToken)
    {
        var course = await _context.KhoaHocs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaKhoaHoc == id, cancellationToken);
        if (course is null || !await CanAccessCourseAsync(course, student, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khóa học không hợp lệ hoặc không thuộc sinh viên.");
        }
    }

    private async Task EnsureSessionAccessAsync(int id, NguoiDung student, CancellationToken cancellationToken)
    {
        var result = await (
            from session in _context.BuoiHocs.AsNoTracking()
            join course in _context.KhoaHocs.AsNoTracking()
                on session.MaKhoaHoc equals course.MaKhoaHoc
            where session.MaBuoiHoc == id
            select new { Session = session, Course = course })
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null || !await CanAccessCourseAsync(result.Course, student, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Buổi học không hợp lệ hoặc không thuộc sinh viên.");
        }
    }

    private async Task<bool> CanAccessCourseAsync(KhoaHoc course, NguoiDung student, CancellationToken cancellationToken)
    {
        if (student.MaLop.HasValue && course.MaLop == student.MaLop.Value)
        {
            return true;
        }

        if (!course.MaLopHocPhan.HasValue)
        {
            return false;
        }

        return await _context.DangKyHocPhans.AsNoTracking()
            .AnyAsync(x =>
                x.MaHocSinh == student.MaNguoiDung &&
                x.MaLopHocPhan == course.MaLopHocPhan.Value &&
                x.TrangThai == "da_dang_ky",
                cancellationToken);
    }

    private async Task CrossCheckScoreAsync(
        NguoiDung student,
        ApplicationFormDataValidationResult formData,
        CancellationToken cancellationToken)
    {
        if (!formData.Values.TryGetInt("ma_diem_so", out var scoreId))
        {
            return;
        }

        var score = await _context.DiemSos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDiemSo == scoreId && x.MaHocSinh == student.MaNguoiDung, cancellationToken);
        if (score is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không hợp lệ.");
        }

        if (formData.Values.TryGetInt("ma_mon_hoc", out var subjectId) && score.MaMonHoc != subjectId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không khớp môn học.");
        }

        if (formData.Values.TryGetInt("ma_hoc_ky", out var termId) && score.MaHocKy != termId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không khớp học kỳ.");
        }
    }
}
