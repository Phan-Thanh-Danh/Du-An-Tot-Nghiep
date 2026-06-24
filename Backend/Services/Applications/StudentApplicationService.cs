using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.DTOs.Auth;
using Backend.DTOs.Common;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class StudentApplicationService : IStudentApplicationService
{
    private const int MaxTitleLength = 255;
    private const int MaxCancelReasonLength = 1000;
    private static readonly string[] CancelableStatuses =
    [
        ApplicationStatuses.Draft,
        ApplicationStatuses.Submitted,
        ApplicationStatuses.NeedSupplement
    ];

    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationTemplateValidator _templateValidator;
    private readonly IApplicationFormDataValidator _formDataValidator;
    private readonly IApplicationReferenceValidator _referenceValidator;
    private readonly IApplicationEvidenceValidator _evidenceValidator;
    private readonly IApplicationStateMachine _stateMachine;
    private readonly IReadOnlyDictionary<string, IApplicationSubmissionRule> _submissionRules;

    public StudentApplicationService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IApplicationTemplateValidator templateValidator,
        IApplicationFormDataValidator formDataValidator,
        IApplicationReferenceValidator referenceValidator,
        IApplicationEvidenceValidator evidenceValidator,
        IApplicationStateMachine stateMachine,
        IEnumerable<IApplicationSubmissionRule> submissionRules)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _templateValidator = templateValidator;
        _formDataValidator = formDataValidator;
        _referenceValidator = referenceValidator;
        _evidenceValidator = evidenceValidator;
        _stateMachine = stateMachine;
        _submissionRules = submissionRules.ToDictionary(x => x.SupportedType, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<StudentApplicationDetailDto> CreateAsync(
        CreateStudentApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var type = NormalizeType(request.LoaiDon);
        var template = await GetActiveTemplateAsync(type, cancellationToken);
        _templateValidator.Validate(template.CauHinhJson);

        var formJson = JsonElementToJsonOrEmpty(request.DuLieuBieuMau);
        var formData = _formDataValidator.Validate(template, formJson, ApplicationFormValidationMode.Draft);
        var title = NormalizeTitleOrGenerate(request.TieuDe, type);
        var now = DateTime.UtcNow;
        var application = new DonTu
        {
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = student.MaDonVi,
            MaMauDon = template.MaMauDon,
            LoaiDon = type,
            TieuDe = title,
            TrangThai = ApplicationStatuses.Draft,
            TrangThaiXuLyNghiepVu = ApplicationProcessingStatuses.NotProcessed,
            DuLieuBieuMau = formData.NormalizedJson,
            NgayTao = now,
            NgayCapNhat = now
        };

        await _context.ExecuteInTransactionAsync(async () =>
        {
            _context.DonTus.Add(application);
            await _context.SaveChangesAsync(cancellationToken);

            _context.NhatKyDuyetDons.Add(CreateLog(
                application,
                student.MaNguoiDung,
                ApplicationActions.CreateDraft,
                null,
                ApplicationStatuses.Draft,
                "Tạo đơn nháp.",
                true,
                null));

            await _context.SaveChangesAsync(cancellationToken);
        }, cancellationToken);

        return await GetOwnDetailAsync(application.MaDonTu, cancellationToken);
    }

    public async Task<PagedResultDto<StudentApplicationListItemDto>> GetOwnAsync(
        StudentApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        NormalizeQuery(parameters);
        var query = _context.DonTus.AsNoTracking()
            .Include(x => x.MauDon)
            .Where(x => x.MaHocSinh == student.MaNguoiDung);

        if (!string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.TrangThai == status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiDon))
        {
            var type = NormalizeType(parameters.LoaiDon);
            query = query.Where(x => x.LoaiDon == type);
        }

        if (parameters.TuNgay.HasValue)
        {
            query = query.Where(x => x.NgayTao >= parameters.TuNgay.Value);
        }

        if (parameters.DenNgay.HasValue)
        {
            query = query.Where(x => x.NgayTao <= parameters.DenNgay.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(x => x.TieuDe.Contains(search) || x.LoaiDon.Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.NgayCapNhat)
            .ThenByDescending(x => x.MaDonTu)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<StudentApplicationListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<StudentApplicationDetailDto> GetOwnDetailAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        return await GetDetailForStudentAsync(applicationId, student.MaNguoiDung, cancellationToken);
    }

    public async Task<StudentApplicationDetailDto> UpdateAsync(
        int applicationId,
        UpdateStudentApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var application = await GetOwnedApplicationTrackedAsync(applicationId, student.MaNguoiDung, cancellationToken);
        EnsureRowVersion(application, request.RowVersion);

        if (application.TrangThai is not (ApplicationStatuses.Draft or ApplicationStatuses.NeedSupplement))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được cập nhật đơn nháp hoặc đơn đang yêu cầu bổ sung.");
        }

        var template = await ResolveTemplateForMutationAsync(application, cancellationToken);
        var formData = _formDataValidator.Validate(
            template,
            JsonElementToJson(request.DuLieuBieuMau),
            ApplicationFormValidationMode.Draft);

        var newTitle = application.TieuDe;
        if (request.TieuDe is not null)
        {
            newTitle = NormalizeRequiredTitle(request.TieuDe);
        }

        var changedKeys = GetChangedKeys(application.DuLieuBieuMau, formData.NormalizedJson, formData.ProvidedFieldKeys);
        if (newTitle == application.TieuDe && formData.NormalizedJson == (application.DuLieuBieuMau ?? "{}") && application.MaMauDon == template.MaMauDon)
        {
            return await GetDetailForStudentAsync(application.MaDonTu, student.MaNguoiDung, cancellationToken);
        }

        var action = application.TrangThai == ApplicationStatuses.Draft
            ? ApplicationActions.Update
            : ApplicationActions.Supplement;
        var now = DateTime.UtcNow;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(async () =>
            {
                application.TieuDe = newTitle;
                application.DuLieuBieuMau = formData.NormalizedJson;
                application.MaMauDon = template.MaMauDon;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, request.RowVersion);
                _context.NhatKyDuyetDons.Add(CreateLog(
                    application,
                    student.MaNguoiDung,
                    action,
                    application.TrangThai,
                    application.TrangThai,
                    null,
                    false,
                    new { changedFields = changedKeys }));

                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        });

        return await GetDetailForStudentAsync(application.MaDonTu, student.MaNguoiDung, cancellationToken);
    }

    public Task<StudentApplicationDetailDto> SubmitAsync(
        int applicationId,
        SubmitStudentApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SubmitInternalAsync(applicationId, request.RowVersion, false, cancellationToken);
    }

    public Task<StudentApplicationDetailDto> ResubmitAsync(
        int applicationId,
        ResubmitStudentApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        return SubmitInternalAsync(applicationId, request.RowVersion, true, cancellationToken);
    }

    public async Task<StudentApplicationDetailDto> CancelAsync(
        int applicationId,
        CancelStudentApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var application = await GetOwnedApplicationTrackedAsync(applicationId, student.MaNguoiDung, cancellationToken);
        EnsureRowVersion(application, request.RowVersion);

        if (!CancelableStatuses.Contains(application.TrangThai))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái hiện tại không cho phép hủy đơn.");
        }

        _stateMachine.EnsureTransitionAllowed(application.TrangThai, ApplicationStatuses.Cancelled);
        var reason = NormalizeOptionalText(request.LyDo, MaxCancelReasonLength);
        var oldStatus = application.TrangThai;
        var now = DateTime.UtcNow;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(async () =>
            {
                application.TrangThai = ApplicationStatuses.Cancelled;
                application.NgayCapNhat = now;
                SetOriginalRowVersion(application, request.RowVersion);
                _context.NhatKyDuyetDons.Add(CreateLog(
                    application,
                    student.MaNguoiDung,
                    ApplicationActions.Cancel,
                    oldStatus,
                    ApplicationStatuses.Cancelled,
                    reason,
                    true,
                    null));

                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        });

        return await GetDetailForStudentAsync(application.MaDonTu, student.MaNguoiDung, cancellationToken);
    }

    private async Task<StudentApplicationDetailDto> SubmitInternalAsync(
        int applicationId,
        string rowVersion,
        bool isResubmit,
        CancellationToken cancellationToken)
    {
        var student = await GetCurrentStudentAsync(cancellationToken);
        var application = await GetOwnedApplicationTrackedAsync(applicationId, student.MaNguoiDung, cancellationToken);
        EnsureRowVersion(application, rowVersion);
        var expectedStatus = isResubmit ? ApplicationStatuses.NeedSupplement : ApplicationStatuses.Draft;
        var targetStatus = isResubmit ? ApplicationStatuses.InReview : ApplicationStatuses.Submitted;
        if (application.TrangThai != expectedStatus)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái hiện tại không hợp lệ để nộp đơn.");
        }

        var template = await ResolveTemplateForMutationAsync(application, cancellationToken);
        var formData = _formDataValidator.Validate(template, application.DuLieuBieuMau, ApplicationFormValidationMode.Submit);

        var oldStatus = application.TrangThai;
        var now = DateTime.UtcNow;

        await ExecuteConcurrencyAwareAsync(async () =>
        {
            await _context.ExecuteInTransactionAsync(IsolationLevel.Serializable, async () =>
            {
                var ruleContext = new ApplicationSubmissionRuleContext
                {
                    Application = application,
                    Student = student,
                    FormData = formData
                };
                await AcquireDuplicateBusinessLockAsync(ruleContext, cancellationToken);
                await _referenceValidator.ValidateAsync(student, formData, cancellationToken);
                await ValidateTypeRuleAsync(ruleContext, cancellationToken);
                await _evidenceValidator.ValidateAsync(application, template, formData, cancellationToken);
                SetOriginalRowVersion(application, rowVersion);
                application.MaMauDon = template.MaMauDon;
                application.DuLieuBieuMau = formData.NormalizedJson;
                application.TrangThai = targetStatus;
                application.NgayCapNhat = now;
                application.HanXuLyLuc = template.SlaGio.HasValue ? now.AddHours(template.SlaGio.Value) : null;
                if (isResubmit)
                {
                    application.NoiDungYeuCauBoSung = null;
                }
                else
                {
                    application.NgayNop = now;
                }

                _context.NhatKyDuyetDons.Add(CreateLog(
                    application,
                    student.MaNguoiDung,
                    isResubmit ? ApplicationActions.Resubmit : ApplicationActions.Submit,
                    oldStatus,
                    targetStatus,
                    isResubmit ? "Nộp lại đơn sau khi bổ sung." : "Nộp đơn.",
                    true,
                    null));

                await _context.SaveChangesAsync(cancellationToken);
            }, cancellationToken);
        });

        return await GetDetailForStudentAsync(application.MaDonTu, student.MaNguoiDung, cancellationToken);
    }

    private async Task ValidateTypeRuleAsync(
        ApplicationSubmissionRuleContext context,
        CancellationToken cancellationToken)
    {
        if (_submissionRules.TryGetValue(context.Application.LoaiDon, out var rule))
        {
            await rule.ValidateAsync(context, cancellationToken);
        }
    }

    private async Task AcquireDuplicateBusinessLockAsync(
        ApplicationSubmissionRuleContext context,
        CancellationToken cancellationToken)
    {
        if (!_submissionRules.TryGetValue(context.Application.LoaiDon, out var rule))
        {
            return;
        }

        var businessKey = rule.BuildDuplicateLockKey(context);
        if (string.IsNullOrWhiteSpace(businessKey))
        {
            return;
        }

        var resource = "ApplicationDuplicate:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(businessKey)));
        var result = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        var resourceParameter = new SqlParameter("@resource", SqlDbType.NVarChar, 255)
        {
            Value = resource
        };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC @result = sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000",
            [result, resourceParameter],
            cancellationToken);

        if (result.Value is not int code || code < 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đơn cùng nghiệp vụ đang được xử lý đồng thời. Vui lòng thử lại.");
        }
    }

    private async Task<NguoiDung> GetCurrentStudentAsync(CancellationToken cancellationToken)
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Token xác thực không hợp lệ.");
        }

        var student = await _context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == currentUser.UserId, cancellationToken);
        if (student is null ||
            student.VaiTroChinh != AuthRoles.ToDatabaseCode(AuthRoles.Student) ||
            student.TrangThai != UserStatuses.DbActive)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ sinh viên đang hoạt động được sử dụng chức năng đơn từ.");
        }

        return student;
    }

    private async Task<DonTu> GetOwnedApplicationTrackedAsync(
        int applicationId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var application = await _context.DonTus
            .FirstOrDefaultAsync(x => x.MaDonTu == applicationId && x.MaHocSinh == studentId, cancellationToken);
        if (application is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        return application;
    }

    private async Task<StudentApplicationDetailDto> GetDetailForStudentAsync(
        int applicationId,
        int studentId,
        CancellationToken cancellationToken)
    {
        var application = await _context.DonTus.AsNoTracking()
            .Include(x => x.MauDon)
            .FirstOrDefaultAsync(x => x.MaDonTu == applicationId && x.MaHocSinh == studentId, cancellationToken);
        if (application is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        var attachments = await _context.TepDinhKemDonTus.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && !x.DaXoa)
            .OrderBy(x => x.NgayTao)
            .ThenBy(x => x.MaTep)
            .Select(x => new StudentApplicationAttachmentDto
            {
                MaTep = x.MaTep,
                TenFileGoc = x.TenFileGoc,
                ContentType = x.ContentType,
                KichThuocByte = x.KichThuocByte,
                NgayTao = x.NgayTao
            })
            .ToListAsync(cancellationToken);

        var timeline = await _context.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == application.MaDonTu && x.HienThiChoHocSinh)
            .OrderBy(x => x.NgayTao)
            .ThenBy(x => x.MaNkDuyet)
            .Select(x => new StudentApplicationTimelineDto
            {
                HanhDong = x.HanhDong,
                TrangThaiCu = x.TrangThaiCu,
                TrangThaiMoi = x.TrangThaiMoi,
                GhiChuCongKhai = x.GhiChuCongKhai,
                NguonThucHien = x.NguonThucHien,
                NgayTao = x.NgayTao
            })
            .ToListAsync(cancellationToken);

        var detail = ToDetailDto(application);
        detail.Attachments = attachments;
        detail.Timeline = timeline;
        return detail;
    }

    private async Task<MauDonTu> GetActiveTemplateAsync(string type, CancellationToken cancellationToken)
    {
        var template = await _context.MauDonTus
            .FirstOrDefaultAsync(x => x.LoaiDon == type && x.DangHoatDong, cancellationToken);
        if (template is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không tìm thấy mẫu đơn đang hoạt động.");
        }

        return template;
    }

    private async Task<MauDonTu> ResolveTemplateForMutationAsync(DonTu application, CancellationToken cancellationToken)
    {
        MauDonTu? template = null;
        if (application.MaMauDon.HasValue)
        {
            template = await _context.MauDonTus
                .FirstOrDefaultAsync(x => x.MaMauDon == application.MaMauDon.Value, cancellationToken);
        }

        if (template is null)
        {
            template = await _context.MauDonTus
                .FirstOrDefaultAsync(x => x.LoaiDon == application.LoaiDon && x.DangHoatDong, cancellationToken);
            if (template is null)
            {
                throw new ApiException(StatusCodes.Status409Conflict, "Không tìm thấy mẫu đơn phù hợp cho đơn legacy.");
            }

            application.MaMauDon = template.MaMauDon;
        }

        _templateValidator.Validate(template.CauHinhJson);
        return template;
    }

    private static StudentApplicationListItemDto ToListItemDto(DonTu application)
    {
        return new StudentApplicationListItemDto
        {
            MaDonTu = application.MaDonTu,
            LoaiDon = application.LoaiDon,
            TenLoaiDon = ApplicationSchemaService.GetTypeLabel(application.LoaiDon),
            TieuDe = application.TieuDe,
            TrangThai = application.TrangThai,
            TenTrangThai = GetStatusLabel(application.TrangThai),
            PhienBanMau = application.MauDon?.PhienBan,
            NgayTao = application.NgayTao,
            NgayCapNhat = application.NgayCapNhat,
            NgayNop = application.NgayNop,
            HanXuLyLuc = application.HanXuLyLuc,
            CanEdit = application.TrangThai is ApplicationStatuses.Draft or ApplicationStatuses.NeedSupplement,
            CanSubmit = application.TrangThai == ApplicationStatuses.Draft,
            CanResubmit = application.TrangThai == ApplicationStatuses.NeedSupplement,
            CanCancel = CancelableStatuses.Contains(application.TrangThai)
        };
    }

    private static StudentApplicationDetailDto ToDetailDto(DonTu application)
    {
        var item = ToListItemDto(application);
        return new StudentApplicationDetailDto
        {
            MaDonTu = item.MaDonTu,
            LoaiDon = item.LoaiDon,
            TenLoaiDon = item.TenLoaiDon,
            TieuDe = item.TieuDe,
            TrangThai = item.TrangThai,
            TenTrangThai = item.TenTrangThai,
            PhienBanMau = item.PhienBanMau,
            NgayTao = item.NgayTao,
            NgayCapNhat = item.NgayCapNhat,
            NgayNop = item.NgayNop,
            HanXuLyLuc = item.HanXuLyLuc,
            CanEdit = item.CanEdit,
            CanSubmit = item.CanSubmit,
            CanResubmit = item.CanResubmit,
            CanCancel = item.CanCancel,
            MaMauDon = application.MaMauDon,
            DuLieuBieuMau = ParseJsonElement(application.DuLieuBieuMau),
            Template = application.MauDon is null ? null : new StudentApplicationTemplateDto
            {
                MaMauDon = application.MauDon.MaMauDon,
                TenMau = application.MauDon.TenMau,
                PhienBan = application.MauDon.PhienBan,
                CauHinhJson = application.MauDon.CauHinhJson,
                BatBuocMinhChung = application.MauDon.BatBuocMinhChung,
                SoTepToiDa = application.MauDon.SoTepToiDa,
                DungLuongTepToiDaByte = application.MauDon.DungLuongTepToiDaByte,
                TongDungLuongToiDaByte = application.MauDon.TongDungLuongToiDaByte,
                SlaGio = application.MauDon.SlaGio
            },
            RowVersion = Convert.ToBase64String(application.RowVersion)
        };
    }

    private static NhatKyDuyetDon CreateLog(
        DonTu application,
        int userId,
        string action,
        string? oldStatus,
        string? newStatus,
        string? publicNote,
        bool showToStudent,
        object? snapshot)
    {
        return new NhatKyDuyetDon
        {
            MaDonTu = application.MaDonTu,
            MaNguoiDuyet = userId,
            NguonThucHien = "user",
            HanhDong = action,
            TrangThaiCu = oldStatus,
            TrangThaiMoi = newStatus,
            GhiChuCongKhai = publicNote,
            GhiChuNoiBo = null,
            SnapshotJson = snapshot is null ? null : JsonSerializer.Serialize(snapshot),
            HienThiChoHocSinh = showToStudent,
            NgayTao = DateTime.UtcNow
        };
    }

    private static void NormalizeQuery(StudentApplicationQueryParameters parameters)
    {
        if (parameters.PageIndex < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageIndex phải lớn hơn hoặc bằng 1.");
        }

        if (parameters.PageSize is < 1 or > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageSize phải từ 1 đến 100.");
        }

        if (parameters.TuNgay.HasValue && parameters.DenNgay.HasValue && parameters.TuNgay > parameters.DenNgay)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ ngày phải nhỏ hơn hoặc bằng đến ngày.");
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            parameters.Search = parameters.Search.Trim();
            if (parameters.Search.Length > 100)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Từ khóa tìm kiếm vượt quá độ dài cho phép.");
            }
        }
    }

    private static string NormalizeType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn không hợp lệ.");
        }

        var canonical = ApplicationTypes.All.FirstOrDefault(x => x.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizeStatus(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái đơn không hợp lệ.");
        }

        var canonical = ApplicationStatuses.All.FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái đơn không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizeTitleOrGenerate(string? title, string type)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            return NormalizeRequiredTitle(title);
        }

        return $"{ApplicationSchemaService.GetTypeLabel(type)} - {DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static string NormalizeRequiredTitle(string title)
    {
        var normalized = title.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tiêu đề không được để trống.");
        }

        if (normalized.Length > MaxTitleLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tiêu đề vượt quá độ dài cho phép.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var normalized = text.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Nội dung vượt quá độ dài cho phép.");
        }

        return normalized;
    }

    private static string JsonElementToJsonOrEmpty(JsonElement? element)
    {
        if (!element.HasValue || element.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return "{}";
        }

        return element.Value.GetRawText();
    }

    private static string JsonElementToJson(JsonElement element)
    {
        return element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? "{}"
            : element.GetRawText();
    }

    private static JsonElement ParseJsonElement(string? json)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
        return document.RootElement.Clone();
    }

    private static string GetStatusLabel(string status)
    {
        return status switch
        {
            ApplicationStatuses.Draft => "Nháp",
            ApplicationStatuses.Submitted => "Đã nộp",
            ApplicationStatuses.InReview => "Đang xem xét",
            ApplicationStatuses.NeedSupplement => "Yêu cầu bổ sung",
            ApplicationStatuses.Approved => "Đã duyệt",
            ApplicationStatuses.Rejected => "Từ chối",
            ApplicationStatuses.Cancelled => "Đã hủy",
            _ => status
        };
    }

    private static IReadOnlyList<string> GetChangedKeys(string? oldJson, string newJson, IReadOnlyList<string> fallbackKeys)
    {
        try
        {
            using var oldDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(oldJson) ? "{}" : oldJson);
            using var newDoc = JsonDocument.Parse(newJson);
            var oldKeys = oldDoc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetRawText(), StringComparer.OrdinalIgnoreCase);
            var newKeys = newDoc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.GetRawText(), StringComparer.OrdinalIgnoreCase);
            return oldKeys.Keys.Union(newKeys.Keys, StringComparer.OrdinalIgnoreCase)
                .Where(key => !oldKeys.TryGetValue(key, out var oldValue) ||
                              !newKeys.TryGetValue(key, out var newValue) ||
                              oldValue != newValue)
                .OrderBy(x => x)
                .ToList();
        }
        catch (JsonException)
        {
            return fallbackKeys;
        }
    }

    private static void EnsureRowVersion(DonTu application, string rowVersion)
    {
        var decoded = DecodeRowVersion(rowVersion);
        if (!application.RowVersion.SequenceEqual(decoded))
        {
            throw ConcurrencyException();
        }
    }

    private void SetOriginalRowVersion(DonTu application, string rowVersion)
    {
        var decoded = DecodeRowVersion(rowVersion);
        _context.Entry(application).Property(x => x.RowVersion).OriginalValue = decoded;
    }

    private static byte[] DecodeRowVersion(string rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }

        try
        {
            var decoded = Convert.FromBase64String(rowVersion);
            if (decoded.Length != 8)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
            }

            return decoded;
        }
        catch (FormatException)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "RowVersion không hợp lệ.");
        }
    }

    private static ApiException ConcurrencyException()
    {
        return new ApiException(
            StatusCodes.Status409Conflict,
            "Đơn đã được thay đổi bởi một thao tác khác. Vui lòng tải lại dữ liệu.");
    }

    private static async Task ExecuteConcurrencyAwareAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw ConcurrencyException();
        }
        catch (SqlException exception) when (exception.Number is 1205 or 3960 or 3961 or 3962 or 3963)
        {
            throw ConcurrencyException();
        }
    }
}
