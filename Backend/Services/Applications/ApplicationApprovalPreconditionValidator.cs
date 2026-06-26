using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationApprovalPreconditionValidator : IApplicationApprovalPreconditionValidator
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationTemplateValidator _templateValidator;
    private readonly IApplicationFormDataValidator _formDataValidator;
    private readonly IApplicationReferenceValidator _referenceValidator;
    private readonly IApplicationEvidenceValidator _evidenceValidator;
    private readonly IReadOnlyDictionary<string, IApplicationSubmissionRule> _submissionRules;

    public ApplicationApprovalPreconditionValidator(
        ApplicationDbContext context,
        IApplicationTemplateValidator templateValidator,
        IApplicationFormDataValidator formDataValidator,
        IApplicationReferenceValidator referenceValidator,
        IApplicationEvidenceValidator evidenceValidator,
        IEnumerable<IApplicationSubmissionRule> submissionRules)
    {
        _context = context;
        _templateValidator = templateValidator;
        _formDataValidator = formDataValidator;
        _referenceValidator = referenceValidator;
        _evidenceValidator = evidenceValidator;
        _submissionRules = submissionRules.ToDictionary(x => x.SupportedType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task ValidateAsync(DonTu application, CancellationToken cancellationToken = default)
    {
        var student = await _context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == application.MaHocSinh, cancellationToken);
        if (student is null ||
            student.TrangThai != UserStatuses.DbActive ||
            student.MaDonVi != application.MaDonVi)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Sinh viên của đơn không còn hợp lệ.");
        }

        var template = await ResolveTemplateForApprovalAsync(application, cancellationToken);
        _templateValidator.Validate(template.CauHinhJson);
        var formData = _formDataValidator.Validate(template, application.DuLieuBieuMau, ApplicationFormValidationMode.Submit);
        await _referenceValidator.ValidateAsync(student, formData, cancellationToken);
        await GetSubmissionRule(application.LoaiDon).ValidateAsync(new ApplicationSubmissionRuleContext
        {
            Application = application,
            Student = student,
            FormData = formData
        }, cancellationToken);
        await _evidenceValidator.ValidateAsync(application, template, formData, cancellationToken);

        application.MaMauDon = template.MaMauDon;
        application.DuLieuBieuMau = formData.NormalizedJson;
    }

    private async Task<MauDonTu> ResolveTemplateForApprovalAsync(
        DonTu application,
        CancellationToken cancellationToken)
    {
        MauDonTu? template = null;
        if (application.MaMauDon.HasValue)
        {
            template = await _context.MauDonTus
                .FirstOrDefaultAsync(x => x.MaMauDon == application.MaMauDon.Value, cancellationToken);
            if (template is null || !string.Equals(template.LoaiDon, application.LoaiDon, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException(StatusCodes.Status409Conflict, "Mẫu đơn gắn với hồ sơ không còn an toàn để phê duyệt.");
            }

            return template;
        }

        template = await _context.MauDonTus
            .Where(x => x.LoaiDon == application.LoaiDon && x.DangHoatDong)
            .OrderByDescending(x => x.PhienBan)
            .FirstOrDefaultAsync(cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Không tìm thấy mẫu đơn phù hợp cho đơn legacy.");
        }

        return template;
    }

    private IApplicationSubmissionRule GetSubmissionRule(string type)
    {
        if (_submissionRules.TryGetValue(type, out var rule))
        {
            return rule;
        }

        throw new ApiException(
            StatusCodes.Status500InternalServerError,
            "Chưa cấu hình quy tắc nghiệp vụ cho loại đơn.");
    }
}
