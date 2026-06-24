using System.Globalization;
using Backend.Constants;
using Backend.Data;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public abstract class ApplicationSubmissionRuleBase : IApplicationSubmissionRule
{
    protected static readonly string[] ActiveSubmittedStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview,
        ApplicationStatuses.NeedSupplement
    ];

    protected readonly ApplicationDbContext Context;

    protected ApplicationSubmissionRuleBase(ApplicationDbContext context)
    {
        Context = context;
    }

    public abstract string SupportedType { get; }
    public virtual string? BuildDuplicateLockKey(ApplicationSubmissionRuleContext context) => null;
    public abstract Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default);

    protected Task<bool> HasActiveSameTypeAsync(DonTu application, CancellationToken cancellationToken)
    {
        return Context.DonTus.AsNoTracking().AnyAsync(x =>
            x.MaDonTu != application.MaDonTu &&
            x.MaHocSinh == application.MaHocSinh &&
            x.LoaiDon == application.LoaiDon &&
            ActiveSubmittedStatuses.Contains(x.TrangThai),
            cancellationToken);
    }

    protected async Task EnsureNoActiveSameTypeAsync(DonTu application, string message, CancellationToken cancellationToken)
    {
        if (await HasActiveSameTypeAsync(application, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, message);
        }
    }

    protected static DateOnly RequireDate(ApplicationFormDataValidationResult formData, string key)
    {
        if (!formData.Values.TryGetString(key, out var text) ||
            !DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"Field '{key}' không hợp lệ.");
        }

        return date;
    }
}

public class LeaveApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public LeaveApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.Leave;

    public override Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        var start = RequireDate(context.FormData, "ngay_bat_dau");
        var end = RequireDate(context.FormData, "ngay_ket_thuc");
        var days = end.DayNumber - start.DayNumber + 1;
        if (days is < 1 or > 30)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khoảng ngày nghỉ phép phải từ 1 đến 30 ngày.");
        }

        return Task.CompletedTask;
    }
}

public class RetakeExamApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public RetakeExamApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.RetakeExam;

    public override string BuildDuplicateLockKey(ApplicationSubmissionRuleContext context)
    {
        if (!context.FormData.Values.TryGetInt("ma_mon_hoc", out var subjectId) ||
            !context.FormData.Values.TryGetInt("ma_hoc_ky", out var termId))
        {
            return $"{context.Student.MaNguoiDung}|{SupportedType}|invalid";
        }

        return $"{context.Student.MaNguoiDung}|{SupportedType}|{subjectId}|{termId}";
    }

    public override async Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        if (!context.FormData.Values.TryGetInt("ma_mon_hoc", out var subjectId) ||
            !context.FormData.Values.TryGetInt("ma_hoc_ky", out var termId))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thiếu môn học hoặc học kỳ đăng ký thi lại.");
        }

        var hasScore = await Context.DiemSos.AsNoTracking().AnyAsync(x =>
            x.MaHocSinh == context.Student.MaNguoiDung &&
            x.MaMonHoc == subjectId &&
            x.MaHocKy == termId,
            cancellationToken);
        if (!hasScore)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Sinh viên chưa có dữ liệu học môn này trong học kỳ đã chọn.");
        }

        var duplicate = await Context.DonTus.AsNoTracking().AnyAsync(x =>
            x.MaDonTu != context.Application.MaDonTu &&
            x.MaHocSinh == context.Application.MaHocSinh &&
            x.LoaiDon == ApplicationTypes.RetakeExam &&
            ActiveSubmittedStatuses.Contains(x.TrangThai) &&
            x.DuLieuBieuMau != null &&
            x.DuLieuBieuMau.Contains($"\"ma_mon_hoc\":{subjectId}") &&
            x.DuLieuBieuMau.Contains($"\"ma_hoc_ky\":{termId}"),
            cancellationToken);
        if (duplicate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đã tồn tại đơn thi lại đang xử lý cho môn học và học kỳ này.");
        }
    }
}

public class GradeAppealApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public GradeAppealApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.GradeAppeal;

    public override string BuildDuplicateLockKey(ApplicationSubmissionRuleContext context)
    {
        if (!context.FormData.Values.TryGetInt("ma_diem_so", out var scoreId) ||
            !context.FormData.Values.TryGetString("cot_diem", out var scoreColumn))
        {
            return $"{context.Student.MaNguoiDung}|{SupportedType}|invalid";
        }

        return $"{context.Student.MaNguoiDung}|{SupportedType}|{scoreId}|{scoreColumn.Trim().ToLowerInvariant()}";
    }

    public override async Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        if (!context.FormData.Values.TryGetInt("ma_diem_so", out var scoreId))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thiếu dòng điểm cần phúc tra.");
        }

        if (!context.FormData.Values.TryGetString("cot_diem", out var scoreColumn))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thiếu cột điểm cần phúc tra.");
        }

        var score = await Context.DiemSos.AsNoTracking().FirstOrDefaultAsync(x =>
            x.MaDiemSo == scoreId &&
            x.MaHocSinh == context.Student.MaNguoiDung,
            cancellationToken);
        if (score is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không hợp lệ.");
        }

        if (context.FormData.Values.TryGetInt("ma_mon_hoc", out var subjectId) && score.MaMonHoc != subjectId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không khớp môn học.");
        }

        if (context.FormData.Values.TryGetInt("ma_hoc_ky", out var termId) && score.MaHocKy != termId)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Dòng điểm không khớp học kỳ.");
        }

        var duplicate = await Context.DonTus.AsNoTracking().AnyAsync(x =>
            x.MaDonTu != context.Application.MaDonTu &&
            x.MaHocSinh == context.Application.MaHocSinh &&
            x.LoaiDon == ApplicationTypes.GradeAppeal &&
            ActiveSubmittedStatuses.Contains(x.TrangThai) &&
            x.DuLieuBieuMau != null &&
            x.DuLieuBieuMau.Contains($"\"ma_diem_so\":{scoreId}") &&
            x.DuLieuBieuMau.Contains($"\"cot_diem\":\"{scoreColumn}\""),
            cancellationToken);
        if (duplicate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đã tồn tại đơn phúc tra đang xử lý cho dòng điểm này.");
        }
    }
}

public class AcademicPauseApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public AcademicPauseApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.AcademicPause;

    public override string BuildDuplicateLockKey(ApplicationSubmissionRuleContext context)
    {
        return $"{context.Student.MaNguoiDung}|{SupportedType}";
    }

    public override async Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        if (!context.FormData.Values.TryGetDecimal("thoi_luong_du_kien", out var months) ||
            months < 1 ||
            months > 24)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Thời lượng bảo lưu phải từ 1 đến 24 tháng.");
        }

        await EnsureNoActiveSameTypeAsync(context.Application, "Đã tồn tại đơn bảo lưu đang xử lý.", cancellationToken);
    }
}

public class ChangeCampusApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public ChangeCampusApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.ChangeCampus;

    public override string BuildDuplicateLockKey(ApplicationSubmissionRuleContext context)
    {
        return $"{context.Student.MaNguoiDung}|{SupportedType}";
    }

    public override async Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        if (!context.FormData.Values.TryGetInt("ma_don_vi_hien_tai", out var currentCampusId) ||
            currentCampusId != context.Student.MaDonVi)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở hiện tại không khớp với sinh viên.");
        }

        if (!context.FormData.Values.TryGetInt("ma_don_vi_mong_muon", out var targetCampusId) ||
            targetCampusId == context.Student.MaDonVi)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở mong muốn phải khác cơ sở hiện tại.");
        }

        var targetActive = await Context.DonVis.AsNoTracking()
            .AnyAsync(x => x.MaDonVi == targetCampusId && x.ConHoatDong, cancellationToken);
        if (!targetActive)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Cơ sở mong muốn không hợp lệ.");
        }

        await EnsureNoActiveSameTypeAsync(context.Application, "Đã tồn tại đơn chuyển cơ sở đang xử lý.", cancellationToken);
    }
}

public class ConfirmationApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public ConfirmationApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.Confirmation;

    public override Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        if (!context.FormData.Values.TryGetDecimal("so_ban", out var copies) ||
            copies < 1 ||
            copies > 5 ||
            decimal.Truncate(copies) != copies)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Số bản xác nhận phải từ 1 đến 5.");
        }

        return Task.CompletedTask;
    }
}

public class CertificateApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public CertificateApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.Certificate;

    public override Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class OtherApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public OtherApplicationSubmissionRule(ApplicationDbContext context) : base(context) { }
    public override string SupportedType => ApplicationTypes.Other;

    public override Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class SingleActiveSameTypeApplicationSubmissionRule : ApplicationSubmissionRuleBase
{
    public SingleActiveSameTypeApplicationSubmissionRule(ApplicationDbContext context, string supportedType) : base(context)
    {
        SupportedType = supportedType;
    }

    public override string SupportedType { get; }

    public override string BuildDuplicateLockKey(ApplicationSubmissionRuleContext context)
    {
        return $"{context.Student.MaNguoiDung}|{SupportedType}";
    }

    public override Task ValidateAsync(ApplicationSubmissionRuleContext context, CancellationToken cancellationToken = default)
    {
        return EnsureNoActiveSameTypeAsync(
            context.Application,
            "Đã tồn tại đơn cùng loại đang xử lý.",
            cancellationToken);
    }
}

public class WithdrawalApplicationSubmissionRule : SingleActiveSameTypeApplicationSubmissionRule
{
    public WithdrawalApplicationSubmissionRule(ApplicationDbContext context)
        : base(context, ApplicationTypes.Withdrawal)
    {
    }
}

public class TransferSchoolApplicationSubmissionRule : SingleActiveSameTypeApplicationSubmissionRule
{
    public TransferSchoolApplicationSubmissionRule(ApplicationDbContext context)
        : base(context, ApplicationTypes.TransferSchool)
    {
    }
}

public class ChangeMajorApplicationSubmissionRule : SingleActiveSameTypeApplicationSubmissionRule
{
    public ChangeMajorApplicationSubmissionRule(ApplicationDbContext context)
        : base(context, ApplicationTypes.ChangeMajor)
    {
    }
}
