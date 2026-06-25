using System.Text.Json;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.Applications;

public class ApplicationAdminQueueService : IApplicationAdminQueueService
{
    private static readonly string[] DefaultQueueStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview
    ];

    private static readonly string[] ActiveQueueStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview,
        ApplicationStatuses.NeedSupplement
    ];

    private static readonly string[] AssignableRoles =
    [
        AuthRoles.SuperAdmin,
        AuthRoles.Admin,
        AuthRoles.CampusAdmin,
        AuthRoles.SubCampusAdmin,
        AuthRoles.AcademicStaff
    ];

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly ApplicationQueueOptions _options;

    public ApplicationAdminQueueService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IOptions<ApplicationQueueOptions> options)
    {
        _context = context;
        _scopeService = scopeService;
        _options = options.Value;
    }

    public async Task<AdminApplicationQueueResponseDto> GetQueueAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, parameters.MaDonVi, cancellationToken);

        var query = BuildFilteredQuery(parameters, actor);
        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await ApplyDefaultOrdering(query)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new ApplicationProjection
            {
                MaDonTu = x.MaDonTu,
                MaDonVi = x.MaDonVi,
                MaHocSinh = x.MaHocSinh,
                MaMauDon = x.MaMauDon,
                LoaiDon = x.LoaiDon,
                TieuDe = x.TieuDe,
                TrangThai = x.TrangThai,
                TrangThaiXuLyNghiepVu = x.TrangThaiXuLyNghiepVu,
                NguoiDuyetHienTai = x.NguoiDuyetHienTai,
                NguoiXuLyCuoi = x.NguoiXuLyCuoi,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat,
                NgayNop = x.NgayNop,
                HanXuLyLuc = x.HanXuLyLuc,
                RowVersion = x.RowVersion,
                StudentName = x.HocSinh != null ? x.HocSinh.HoTen : string.Empty,
                StudentEmail = x.HocSinh != null ? x.HocSinh.Email : string.Empty,
                CampusName = x.DonVi != null ? x.DonVi.TenDonVi : string.Empty,
                AssigneeName = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.HoTen : null,
                AssigneeEmail = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.Email : null,
                AssigneeRole = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.VaiTroChinh : null
            })
            .ToListAsync(cancellationToken);

        return new AdminApplicationQueueResponseDto
        {
            Items = rows.Select(row => ToQueueItemDto(row)).ToList(),
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminApplicationQueueSummaryDto> GetQueueSummaryAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        NormalizeQuery(parameters, allowPaging: false);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, parameters.MaDonVi, cancellationToken);

        var query = BuildFilteredQuery(parameters, actor);
        var rows = await query
            .Select(x => new { x.TrangThai, x.NguoiDuyetHienTai, x.HanXuLyLuc })
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        return new AdminApplicationQueueSummaryDto
        {
            TotalActive = rows.Count,
            Submitted = rows.Count(x => x.TrangThai == ApplicationStatuses.Submitted),
            InReview = rows.Count(x => x.TrangThai == ApplicationStatuses.InReview),
            NeedSupplement = rows.Count(x => x.TrangThai == ApplicationStatuses.NeedSupplement),
            Unassigned = rows.Count(x => x.NguoiDuyetHienTai is null),
            Overdue = rows.Count(x => GetSlaStatus(x.TrangThai, x.HanXuLyLuc, now) == "overdue"),
            DueSoon = rows.Count(x => GetSlaStatus(x.TrangThai, x.HanXuLyLuc, now) == "due_soon")
        };
    }

    public async Task<AdminApplicationDetailDto> GetDetailAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var row = await _scopeService.ApplyApplicationScope(_context.DonTus.AsNoTracking(), actor)
            .Where(x => x.MaDonTu == applicationId)
            .Select(x => new ApplicationProjection
            {
                MaDonTu = x.MaDonTu,
                MaDonVi = x.MaDonVi,
                MaHocSinh = x.MaHocSinh,
                MaMauDon = x.MaMauDon,
                LoaiDon = x.LoaiDon,
                TieuDe = x.TieuDe,
                TrangThai = x.TrangThai,
                TrangThaiXuLyNghiepVu = x.TrangThaiXuLyNghiepVu,
                NguoiDuyetHienTai = x.NguoiDuyetHienTai,
                NguoiXuLyCuoi = x.NguoiXuLyCuoi,
                DuLieuBieuMau = x.DuLieuBieuMau,
                NoiDungYeuCauBoSung = x.NoiDungYeuCauBoSung,
                LyDoTuChoi = x.LyDoTuChoi,
                NgayTao = x.NgayTao,
                NgayCapNhat = x.NgayCapNhat,
                NgayNop = x.NgayNop,
                HanXuLyLuc = x.HanXuLyLuc,
                RowVersion = x.RowVersion,
                StudentName = x.HocSinh != null ? x.HocSinh.HoTen : string.Empty,
                StudentEmail = x.HocSinh != null ? x.HocSinh.Email : string.Empty,
                CampusName = x.DonVi != null ? x.DonVi.TenDonVi : string.Empty,
                TemplateVersion = x.MauDon != null ? x.MauDon.PhienBan : null,
                AssigneeName = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.HoTen : null,
                AssigneeEmail = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.Email : null,
                AssigneeRole = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.VaiTroChinh : null,
                LastProcessorName = x.NguoiXuLyCuoiNavigation != null ? x.NguoiXuLyCuoiNavigation.HoTen : null,
                LastProcessorEmail = x.NguoiXuLyCuoiNavigation != null ? x.NguoiXuLyCuoiNavigation.Email : null,
                LastProcessorRole = x.NguoiXuLyCuoiNavigation != null ? x.NguoiXuLyCuoiNavigation.VaiTroChinh : null
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đơn từ.");
        }

        var attachments = await _context.TepDinhKemDonTus.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId && !x.DaXoa)
            .OrderBy(x => x.NgayTao)
            .ThenBy(x => x.MaTep)
            .Select(x => new AdminApplicationAttachmentDto
            {
                MaTep = x.MaTep,
                TenFileGoc = x.TenFileGoc,
                ContentType = x.ContentType,
                KichThuocByte = x.KichThuocByte,
                NgayTao = x.NgayTao
            })
            .ToListAsync(cancellationToken);

        var timeline = await _context.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .OrderBy(x => x.NgayTao)
            .ThenBy(x => x.MaNkDuyet)
            .Select(x => new AdminApplicationTimelineDto
            {
                MaNkDuyet = x.MaNkDuyet,
                NguonThucHien = x.NguonThucHien,
                HanhDong = x.HanhDong,
                TrangThaiCu = x.TrangThaiCu,
                TrangThaiMoi = x.TrangThaiMoi,
                GhiChuCongKhai = x.GhiChuCongKhai,
                GhiChuNoiBo = x.GhiChuNoiBo,
                SnapshotJson = x.SnapshotJson,
                HienThiChoHocSinh = x.HienThiChoHocSinh,
                NgayTao = x.NgayTao,
                NguoiThucHien = x.NguoiDuyet == null ? null : new AdminApplicationPersonDto
                {
                    MaNguoiDung = x.NguoiDuyet.MaNguoiDung,
                    HoTen = x.NguoiDuyet.HoTen,
                    Email = x.NguoiDuyet.Email,
                    VaiTro = AuthRoles.FromDatabaseCode(x.NguoiDuyet.VaiTroChinh)
                }
            })
            .ToListAsync(cancellationToken);

        var detail = ToDetailDto(row, actor);
        detail.Attachments = attachments;
        detail.Timeline = timeline;
        return detail;
    }

    public async Task<AdminApplicationAssigneeResponseDto> GetAssigneesAsync(
        AdminApplicationAssigneeQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        NormalizeAssigneeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, parameters.MaDonVi, cancellationToken);

        var query = _scopeService.ApplyUserScope(_context.NguoiDungs.AsNoTracking(), actor)
            .Where(x => x.TrangThai == UserStatuses.DbActive)
            .Where(x => AssignableRoles.Select(AuthRoles.ToDatabaseCode).Contains(x.VaiTroChinh));

        if (parameters.MaDonVi.HasValue)
        {
            query = query.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(x => x.HoTen.Contains(search) || x.Email.Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.HoTen)
            .ThenBy(x => x.Email)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(x => new AdminApplicationAssigneeDto
            {
                MaNguoiDung = x.MaNguoiDung,
                HoTen = x.HoTen,
                Email = x.Email,
                VaiTro = AuthRoles.FromDatabaseCode(x.VaiTroChinh),
                MaDonVi = x.MaDonVi,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : string.Empty
            })
            .ToListAsync(cancellationToken);

        return new AdminApplicationAssigneeResponseDto
        {
            Items = items,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<DonTu> BuildFilteredQuery(
        AdminApplicationQueryParameters parameters,
        ApplicationActorContext actor)
    {
        var query = _scopeService.ApplyApplicationScope(_context.DonTus.AsNoTracking(), actor);
        if (string.IsNullOrWhiteSpace(parameters.TrangThai))
        {
            query = query.Where(x => DefaultQueueStatuses.Contains(x.TrangThai));
        }
        else
        {
            var status = NormalizeStatus(parameters.TrangThai);
            query = query.Where(x => x.TrangThai == status);
        }

        if (parameters.MaDonVi.HasValue)
        {
            query = query.Where(x => x.MaDonVi == parameters.MaDonVi.Value);
        }

        if (parameters.MaHocSinh.HasValue)
        {
            query = query.Where(x => x.MaHocSinh == parameters.MaHocSinh.Value);
        }

        if (parameters.NguoiDuyetHienTai.HasValue)
        {
            query = query.Where(x => x.NguoiDuyetHienTai == parameters.NguoiDuyetHienTai.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.LoaiDon))
        {
            var type = NormalizeType(parameters.LoaiDon);
            query = query.Where(x => x.LoaiDon == type);
        }

        if (parameters.TuNgayNop.HasValue)
        {
            query = query.Where(x => x.NgayNop >= parameters.TuNgayNop.Value);
        }

        if (parameters.DenNgayNop.HasValue)
        {
            query = query.Where(x => x.NgayNop <= parameters.DenNgayNop.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignmentState))
        {
            var assignmentState = parameters.AssignmentState.Trim().ToLowerInvariant();
            query = assignmentState switch
            {
                "unassigned" => query.Where(x => x.NguoiDuyetHienTai == null),
                "assigned" => query.Where(x => x.NguoiDuyetHienTai != null),
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái phân công không hợp lệ.")
            };
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();
            query = query.Where(x => x.TieuDe.Contains(search) ||
                                     x.LoaiDon.Contains(search) ||
                                     x.HocSinh!.HoTen.Contains(search) ||
                                     x.HocSinh.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(parameters.SlaStatus))
        {
            query = ApplySlaStatusFilter(query, parameters.SlaStatus);
        }

        return query;
    }

    private IQueryable<DonTu> ApplyDefaultOrdering(IQueryable<DonTu> query)
    {
        var now = DateTime.UtcNow;
        var dueSoonDeadline = now.AddHours(_options.SlaWarningBeforeHours);
        return query
            .OrderByDescending(x => ActiveQueueStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value < now)
            .ThenByDescending(x => ActiveQueueStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value >= now && x.HanXuLyLuc.Value <= dueSoonDeadline)
            .ThenByDescending(x => x.NguoiDuyetHienTai == null)
            .ThenBy(x => x.HanXuLyLuc)
            .ThenBy(x => x.NgayNop)
            .ThenBy(x => x.MaDonTu);
    }

    private IQueryable<DonTu> ApplySlaStatusFilter(IQueryable<DonTu> query, string slaStatus)
    {
        var status = slaStatus.Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;
        var dueSoonDeadline = now.AddHours(_options.SlaWarningBeforeHours);
        return status switch
        {
            "none" => query.Where(x => !ActiveQueueStatuses.Contains(x.TrangThai) || !x.HanXuLyLuc.HasValue),
            "paused" => query.Where(x => x.TrangThai == ApplicationStatuses.NeedSupplement),
            "overdue" => query.Where(x => x.TrangThai != ApplicationStatuses.NeedSupplement &&
                                          ActiveQueueStatuses.Contains(x.TrangThai) &&
                                          x.HanXuLyLuc.HasValue &&
                                          x.HanXuLyLuc.Value < now),
            "due_soon" => query.Where(x => x.TrangThai != ApplicationStatuses.NeedSupplement &&
                                           ActiveQueueStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value >= now &&
                                           x.HanXuLyLuc.Value <= dueSoonDeadline),
            "on_track" => query.Where(x => x.TrangThai != ApplicationStatuses.NeedSupplement &&
                                           ActiveQueueStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value > dueSoonDeadline),
            _ => throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái SLA không hợp lệ.")
        };
    }

    private AdminApplicationQueueItemDto ToQueueItemDto(ApplicationProjection row)
    {
        return new AdminApplicationQueueItemDto
        {
            MaDonTu = row.MaDonTu,
            LoaiDon = row.LoaiDon,
            TenLoaiDon = ApplicationSchemaService.GetTypeLabel(row.LoaiDon),
            TieuDe = row.TieuDe,
            TrangThai = row.TrangThai,
            TenTrangThai = GetStatusLabel(row.TrangThai),
            HocSinh = new AdminApplicationPersonDto
            {
                MaNguoiDung = row.MaHocSinh,
                HoTen = row.StudentName,
                Email = row.StudentEmail,
                VaiTro = AuthRoles.Student
            },
            DonVi = new AdminApplicationCampusDto
            {
                MaDonVi = row.MaDonVi,
                TenDonVi = row.CampusName
            },
            NguoiDuyetHienTai = row.NguoiDuyetHienTai.HasValue
                ? new AdminApplicationPersonDto
                {
                    MaNguoiDung = row.NguoiDuyetHienTai.Value,
                    HoTen = row.AssigneeName ?? string.Empty,
                    Email = row.AssigneeEmail ?? string.Empty,
                    VaiTro = AuthRoles.FromDatabaseCode(row.AssigneeRole ?? string.Empty)
                }
                : null,
            NgayTao = row.NgayTao,
            NgayCapNhat = row.NgayCapNhat,
            NgayNop = row.NgayNop,
            HanXuLyLuc = row.HanXuLyLuc,
            Sla = BuildSla(row.TrangThai, row.HanXuLyLuc),
            RowVersion = Convert.ToBase64String(row.RowVersion)
        };
    }

    private AdminApplicationDetailDto ToDetailDto(ApplicationProjection row, ApplicationActorContext actor)
    {
        var item = ToQueueItemDto(row);
        var (form, valid) = ParseFormJson(row.DuLieuBieuMau);
        return new AdminApplicationDetailDto
        {
            MaDonTu = item.MaDonTu,
            LoaiDon = item.LoaiDon,
            TenLoaiDon = item.TenLoaiDon,
            TieuDe = item.TieuDe,
            TrangThai = item.TrangThai,
            TenTrangThai = item.TenTrangThai,
            HocSinh = item.HocSinh,
            DonVi = item.DonVi,
            NguoiDuyetHienTai = item.NguoiDuyetHienTai,
            NgayTao = item.NgayTao,
            NgayCapNhat = item.NgayCapNhat,
            NgayNop = item.NgayNop,
            HanXuLyLuc = item.HanXuLyLuc,
            Sla = item.Sla,
            RowVersion = item.RowVersion,
            MaMauDon = row.MaMauDon,
            PhienBanMau = row.TemplateVersion,
            TrangThaiXuLyNghiepVu = row.TrangThaiXuLyNghiepVu,
            DuLieuBieuMau = form,
            DuLieuBieuMauHopLe = valid,
            NoiDungYeuCauBoSung = row.NoiDungYeuCauBoSung,
            LyDoTuChoi = row.LyDoTuChoi,
            NguoiXuLyCuoi = row.NguoiXuLyCuoi.HasValue
                ? new AdminApplicationPersonDto
                {
                    MaNguoiDung = row.NguoiXuLyCuoi.Value,
                    HoTen = row.LastProcessorName ?? string.Empty,
                    Email = row.LastProcessorEmail ?? string.Empty,
                    VaiTro = AuthRoles.FromDatabaseCode(row.LastProcessorRole ?? string.Empty)
                }
                : null,
            AllowedActions = BuildAllowedActions(row, actor)
        };
    }

    private AdminApplicationAllowedActionsDto BuildAllowedActions(
        ApplicationProjection row,
        ApplicationActorContext actor)
    {
        var canReceive = actor.Role is AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin or AuthRoles.AcademicStaff &&
                         row.TrangThai == ApplicationStatuses.Submitted &&
                         row.NguoiDuyetHienTai is null;
        var canAssign = actor.Role is AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin or AuthRoles.SubCampusAdmin &&
                        row.TrangThai is ApplicationStatuses.Submitted or ApplicationStatuses.InReview or ApplicationStatuses.NeedSupplement;
        return new AdminApplicationAllowedActionsDto
        {
            CanReceive = canReceive,
            CanAssign = canAssign && row.NguoiDuyetHienTai is null,
            CanReassign = canAssign && row.NguoiDuyetHienTai is not null,
            CanDownloadEvidence = true
        };
    }

    private AdminApplicationSlaDto BuildSla(string status, DateTime? deadline)
    {
        var now = DateTime.UtcNow;
        return new AdminApplicationSlaDto
        {
            Status = GetSlaStatus(status, deadline, now),
            RemainingMinutes = deadline.HasValue ? (int)Math.Floor((deadline.Value - now).TotalMinutes) : null
        };
    }

    private string GetSlaStatus(string status, DateTime? deadline, DateTime now)
    {
        if (status == ApplicationStatuses.NeedSupplement)
        {
            return "paused";
        }

        if (!ActiveQueueStatuses.Contains(status) || !deadline.HasValue)
        {
            return "none";
        }

        if (deadline.Value < now)
        {
            return "overdue";
        }

        return deadline.Value <= now.AddHours(_options.SlaWarningBeforeHours)
            ? "due_soon"
            : "on_track";
    }

    private static (JsonElement Form, bool Valid) ParseFormJson(string? json)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return (document.RootElement.Clone(), true);
        }
        catch (JsonException)
        {
            using var document = JsonDocument.Parse("{}");
            return (document.RootElement.Clone(), false);
        }
    }

    private static void NormalizeQuery(AdminApplicationQueryParameters parameters, bool allowPaging = true)
    {
        if (allowPaging)
        {
            if (parameters.PageIndex < 1)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "PageIndex phải lớn hơn hoặc bằng 1.");
            }

            if (parameters.PageSize is < 1 or > 100)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "PageSize phải từ 1 đến 100.");
            }
        }

        if (parameters.TuNgayNop.HasValue && parameters.DenNgayNop.HasValue && parameters.TuNgayNop > parameters.DenNgayNop)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ ngày nộp phải nhỏ hơn hoặc bằng đến ngày nộp.");
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search) && parameters.Search.Trim().Length > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ khóa tìm kiếm vượt quá độ dài cho phép.");
        }
    }

    private static void NormalizeAssigneeQuery(AdminApplicationAssigneeQueryParameters parameters)
    {
        if (parameters.PageIndex < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageIndex phải lớn hơn hoặc bằng 1.");
        }

        if (parameters.PageSize is < 1 or > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageSize phải từ 1 đến 100.");
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search) && parameters.Search.Trim().Length > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ khóa tìm kiếm vượt quá độ dài cho phép.");
        }
    }

    private static string NormalizeType(string type)
    {
        var canonical = ApplicationTypes.All.FirstOrDefault(x => x.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Loại đơn không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizeStatus(string status)
    {
        var canonical = ApplicationStatuses.All.FirstOrDefault(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái đơn không hợp lệ.");
        }

        return canonical;
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

    private sealed class ApplicationProjection
    {
        public int MaDonTu { get; set; }
        public int MaDonVi { get; set; }
        public int MaHocSinh { get; set; }
        public int? MaMauDon { get; set; }
        public string LoaiDon { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string TrangThaiXuLyNghiepVu { get; set; } = string.Empty;
        public int? NguoiDuyetHienTai { get; set; }
        public int? NguoiXuLyCuoi { get; set; }
        public string? DuLieuBieuMau { get; set; }
        public string? NoiDungYeuCauBoSung { get; set; }
        public string? LyDoTuChoi { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public DateTime? NgayNop { get; set; }
        public DateTime? HanXuLyLuc { get; set; }
        public byte[] RowVersion { get; set; } = [];
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string CampusName { get; set; } = string.Empty;
        public int? TemplateVersion { get; set; }
        public string? AssigneeName { get; set; }
        public string? AssigneeEmail { get; set; }
        public string? AssigneeRole { get; set; }
        public string? LastProcessorName { get; set; }
        public string? LastProcessorEmail { get; set; }
        public string? LastProcessorRole { get; set; }
    }
}
