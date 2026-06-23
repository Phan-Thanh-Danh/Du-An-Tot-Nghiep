using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;
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
            [ApplicationTypes.Withdrawal] = "Đơn rút học"
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

    public ApplicationSchemaService(
        ApplicationDbContext context,
        IApplicationStateMachine stateMachine)
    {
        _context = context;
        _stateMachine = stateMachine;
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
            SlaGio = template.SlaGio
        };
    }
}
