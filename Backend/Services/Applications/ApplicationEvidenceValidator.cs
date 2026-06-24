using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationEvidenceValidator : IApplicationEvidenceValidator
{
    private readonly ApplicationDbContext _context;

    public ApplicationEvidenceValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ValidateAsync(
        DonTu application,
        MauDonTu template,
        ApplicationFormDataValidationResult formData,
        CancellationToken cancellationToken = default)
    {
        var attachments = await _context.TepDinhKemDonTus.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && !x.DaXoa)
            .ToListAsync(cancellationToken);

        if (attachments.Count > template.SoTepToiDa)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số lượng tệp minh chứng vượt quá giới hạn.");
        }

        if (attachments.Any(x => x.KichThuocByte > template.DungLuongTepToiDaByte))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dung lượng một tệp minh chứng vượt quá giới hạn.");
        }

        if (attachments.Sum(x => x.KichThuocByte) > template.TongDungLuongToiDaByte)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tổng dung lượng minh chứng vượt quá giới hạn.");
        }

        if (attachments.Any(x => !ApplicationEvidenceConstants.AllowedContentTypes.Contains(x.ContentType)))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Định dạng tệp minh chứng không được hỗ trợ.");
        }

        var hasEvidence = attachments.Count > 0 || !string.IsNullOrWhiteSpace(application.UrlBangChung);
        if ((template.BatBuocMinhChung || formData.RequiresEvidence) && !hasEvidence)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Đơn này yêu cầu minh chứng trước khi nộp.");
        }
    }
}
