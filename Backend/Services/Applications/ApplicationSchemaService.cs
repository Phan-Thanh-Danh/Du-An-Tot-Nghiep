using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Applications;

public class ApplicationSchemaService : IApplicationSchemaService
{
    private static readonly IReadOnlyDictionary<string, string> TypeLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ApplicationTypes.Leave] = "Đơn nghỉ phép",
            [ApplicationTypes.RetakeExam] = "Đơn thi lại",
            [ApplicationTypes.TransferSchool] = "Đơn chuyển trường",
            [ApplicationTypes.Certificate] = "Đơn cấp chứng chỉ",
            [ApplicationTypes.Other] = "Đơn khác",
            [ApplicationTypes.GradeAppeal] = "Đơn phúc tra điểm",
            [ApplicationTypes.AcademicPause] = "Đơn bảo lưu",
            [ApplicationTypes.ChangeMajor] = "Đơn chuyển ngành",
            [ApplicationTypes.ChangeCampus] = "Đơn chuyển cơ sở",
            [ApplicationTypes.Confirmation] = "Đơn xác nhận",
            [ApplicationTypes.Withdrawal] = "Đơn xin rút học bạ"
        };

    private static readonly IReadOnlyDictionary<string, string> StatusLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ApplicationStatuses.Draft] = "Nháp",
            [ApplicationStatuses.Submitted] = "Đã nộp",
            [ApplicationStatuses.InReview] = "Đang xem xét",
            [ApplicationStatuses.NeedSupplement] = "Yêu cầu bổ sung",
            [ApplicationStatuses.Approved] = "Đã duyệt",
            [ApplicationStatuses.Rejected] = "Từ chối",
            [ApplicationStatuses.Cancelled] = "Đã hủy"
        };

    private readonly ApplicationDbContext _context;
    private readonly IApplicationStateMachine _stateMachine;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationTemplateValidator _templateValidator;

    public ApplicationSchemaService(
        ApplicationDbContext context,
        IApplicationStateMachine stateMachine,
        IHttpContextAccessor httpContextAccessor,
        IAuditLogService auditLogService,
        IApplicationTemplateValidator templateValidator)
    {
        _context = context;
        _stateMachine = stateMachine;
        _httpContextAccessor = httpContextAccessor;
        _auditLogService = auditLogService;
        _templateValidator = templateValidator;
    }

    public IReadOnlyList<ApplicationTypeDto> GetTypes()
    {
        return ApplicationTypes.All
            .Select(type => new ApplicationTypeDto
            {
                LoaiDon = type,
                TenHienThi = GetTypeLabel(type)
            })
            .OrderBy(x => x.LoaiDon)
            .ToList();
    }

    public IReadOnlyList<ApplicationStatusDto> GetStatuses()
    {
        return ApplicationStatuses.All
            .Select(status => new ApplicationStatusDto
            {
                TrangThai = status,
                TenHienThi = GetStatusLabel(status),
                LaTrangThaiKetThuc = _stateMachine.IsTerminal(status),
                TrangThaiTiepTheo = _stateMachine.GetAllowedTargets(status)
            })
            .OrderBy(x => x.TrangThai)
            .ToList();
    }

    public async Task<IReadOnlyList<ApplicationTemplateDto>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _context.MauDonTus
            .AsNoTracking()
            .Where(x => x.DangHoatDong)
            .OrderBy(x => x.LoaiDon)
            .ThenByDescending(x => x.PhienBan)
            .ToListAsync(cancellationToken);

        return templates.Select(ToDto).ToList();
    }

    public async Task<ApplicationTemplateDto> GetActiveTemplateByTypeAsync(
        string loaiDon,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeType(loaiDon);
        var template = await _context.MauDonTus
            .AsNoTracking()
            .Where(x => x.DangHoatDong && x.LoaiDon == normalizedType)
            .OrderByDescending(x => x.PhienBan)
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu đơn đang hoạt động.");
        }

        return ToDto(template);
    }

    public async Task<IReadOnlyList<ApplicationTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _context.MauDonTus
            .AsNoTracking()
            .OrderBy(x => x.LoaiDon)
            .ThenByDescending(x => x.PhienBan)
            .ToListAsync(cancellationToken);

        return templates.Select(ToDto).ToList();
    }

    public async Task<ApplicationTemplateDto> CreateTemplateAsync(
        CreateApplicationTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var loaiDon = NormalizeType(request.LoaiDon);

        var exists = await _context.MauDonTus
            .AnyAsync(x => x.LoaiDon == loaiDon, cancellationToken);
        if (exists)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn này đã có mẫu, chỉ được cập nhật mẫu hiện có.");
        }

        if (string.IsNullOrWhiteSpace(request.TenMau))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên mẫu đơn không được rỗng.");
        }

        _templateValidator.Validate(request.CauHinhJson);

        var now = DateTime.UtcNow;
        var entity = new MauDonTu
        {
            LoaiDon = loaiDon,
            TenMau = request.TenMau.Trim(),
            PhienBan = 1,
            CauHinhJson = request.CauHinhJson.Trim(),
            BatBuocMinhChung = request.BatBuocMinhChung,
            SoTepToiDa = request.SoTepToiDa,
            DungLuongTepToiDaByte = request.DungLuongTepToiDaByte,
            TongDungLuongToiDaByte = request.TongDungLuongToiDaByte,
            SlaGio = request.SlaGio,
            DangHoatDong = true,
            NgayTao = now,
            NgayCapNhat = now
        };

        _context.MauDonTus.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await _auditLogService.LogAsync(
            "MauDonTu",
            entity.MaMauDon.ToString(),
            "CREATE_APPLICATION_TEMPLATE",
            null,
            dto,
            currentUser.UserId,
            currentUser.CampusId,
            $"Tạo mẫu đơn '{dto.TenMau}' (loại {loaiDon}).",
            cancellationToken);

        return dto;
    }

    public async Task<ApplicationTemplateDto> UpdateTemplateAsync(
        string loaiDon,
        UpdateApplicationTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetCurrentUser();
        var normalizedType = NormalizeType(loaiDon);

        var entity = await _context.MauDonTus
            .Where(x => x.LoaiDon == normalizedType)
            .OrderByDescending(x => x.PhienBan)
            .FirstOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy mẫu đơn.");
        }

        if (string.IsNullOrWhiteSpace(request.TenMau))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tên mẫu đơn không được rỗng.");
        }

        var oldSnapshot = ToDto(entity);

        var jsonChanged = !string.Equals(
            request.CauHinhJson?.Trim(),
            entity.CauHinhJson,
            StringComparison.Ordinal);
        if (jsonChanged)
        {
            _templateValidator.Validate(request.CauHinhJson);
            entity.PhienBan += 1;
        }

        entity.TenMau = request.TenMau.Trim();
        entity.CauHinhJson = request.CauHinhJson.Trim();
        entity.BatBuocMinhChung = request.BatBuocMinhChung;
        entity.SoTepToiDa = request.SoTepToiDa;
        entity.DungLuongTepToiDaByte = request.DungLuongTepToiDaByte;
        entity.TongDungLuongToiDaByte = request.TongDungLuongToiDaByte;
        entity.SlaGio = request.SlaGio;
        entity.DangHoatDong = request.DangHoatDong;
        entity.NgayCapNhat = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var dto = ToDto(entity);
        await _auditLogService.LogAsync(
            "MauDonTu",
            entity.MaMauDon.ToString(),
            "UPDATE_APPLICATION_TEMPLATE",
            oldSnapshot,
            dto,
            currentUser.UserId,
            currentUser.CampusId,
            $"Cập nhật mẫu đơn '{dto.TenMau}' (loại {normalizedType}, phiên bản {dto.PhienBan}).",
            cancellationToken);

        return dto;
    }

    public static string GetTypeLabel(string type)
    {
        return TypeLabels.TryGetValue(type, out var label) ? label : type;
    }

    private static string GetStatusLabel(string status)
    {
        return StatusLabels.TryGetValue(status, out var label) ? label : status;
    }

    private static string NormalizeType(string loaiDon)
    {
        if (string.IsNullOrWhiteSpace(loaiDon))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn không hợp lệ.");
        }

        var trimmed = loaiDon.Trim();
        var canonical = ApplicationTypes.All.FirstOrDefault(type =>
            type.Equals(trimmed, StringComparison.OrdinalIgnoreCase));

        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn không hợp lệ.");
        }

        return canonical;
    }

    private static ApplicationTemplateDto ToDto(MauDonTu template)
    {
        return new ApplicationTemplateDto
        {
            MaMauDon = template.MaMauDon,
            LoaiDon = template.LoaiDon,
            TenLoaiDon = GetTypeLabel(template.LoaiDon),
            TenMau = template.TenMau,
            PhienBan = template.PhienBan,
            CauHinhJson = template.CauHinhJson,
            BatBuocMinhChung = template.BatBuocMinhChung,
            SoTepToiDa = template.SoTepToiDa,
            DungLuongTepToiDaByte = template.DungLuongTepToiDaByte,
            TongDungLuongToiDaByte = template.TongDungLuongToiDaByte,
            SlaGio = template.SlaGio,
            DangHoatDong = template.DangHoatDong,
            NgayTao = template.NgayTao,
            NgayCapNhat = template.NgayCapNhat
        };
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
}
