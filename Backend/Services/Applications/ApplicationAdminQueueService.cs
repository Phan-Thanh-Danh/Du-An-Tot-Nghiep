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

    private static readonly string[] RunningSlaStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview
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
    private readonly IApplicationDecisionPermissionEvaluator _permissionEvaluator;
    private readonly IApplicationProcessingPermissionEvaluator _processingPermissionEvaluator;
    private readonly ApplicationQueueOptions _options;

    public ApplicationAdminQueueService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IApplicationDecisionPermissionEvaluator permissionEvaluator,
        IApplicationProcessingPermissionEvaluator processingPermissionEvaluator,
        IOptions<ApplicationQueueOptions> options)
    {
        _context = context;
        _scopeService = scopeService;
        _permissionEvaluator = permissionEvaluator;
        _processingPermissionEvaluator = processingPermissionEvaluator;
        _options = options.Value;
    }

    public async Task<AdminApplicationQueueResponseDto> GetQueueAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters, actor);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);
        var totalItems = await query.CountAsync(cancellationToken);
        var rows = await ApplyOrdering(query, normalized)
            .Skip((normalized.PageIndex - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
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
                AssigneeRole = x.NguoiDuyetHienTaiNavigation != null ? x.NguoiDuyetHienTaiNavigation.VaiTroChinh : null,
                AttachmentCount = _context.TepDinhKemDonTus.Count(attachment => attachment.MaDonTu == x.MaDonTu && !attachment.DaXoa)
            })
            .ToListAsync(cancellationToken);

        return new AdminApplicationQueueResponseDto
        {
            Items = rows.Select(row => ToQueueItemDto(row)).ToList(),
            PageIndex = normalized.PageIndex,
            PageSize = normalized.PageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AdminApplicationQueueSummaryDto> GetQueueSummaryAsync(
        AdminApplicationQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters, actor, allowPaging: false);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var now = DateTime.UtcNow;
        var dueSoonDeadline = now.AddHours(_options.SlaWarningBeforeHours);
        var query = BuildSummaryQuery(normalized, actor)
            .TagWith("P0-DT4.1 QueueSummaryAggregate");
        var aggregate = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalActive = group.Count(),
                Submitted = group.Count(x => x.TrangThai == ApplicationStatuses.Submitted),
                InReview = group.Count(x => x.TrangThai == ApplicationStatuses.InReview),
                NeedSupplement = group.Count(x => x.TrangThai == ApplicationStatuses.NeedSupplement),
                Unassigned = group.Count(x => x.NguoiDuyetHienTai == null),
                Assigned = group.Count(x => x.NguoiDuyetHienTai != null),
                AssignedToMe = group.Count(x => x.NguoiDuyetHienTai == actor.User.MaNguoiDung),
                Overdue = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value < now),
                DueSoon = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value >= now &&
                                           x.HanXuLyLuc.Value <= dueSoonDeadline)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new AdminApplicationQueueSummaryDto
        {
            Active = aggregate?.TotalActive ?? 0,
            TotalActive = aggregate?.TotalActive ?? 0,
            Submitted = aggregate?.Submitted ?? 0,
            InReview = aggregate?.InReview ?? 0,
            NeedSupplement = aggregate?.NeedSupplement ?? 0,
            WaitingForSupplement = aggregate?.NeedSupplement ?? 0,
            Unassigned = aggregate?.Unassigned ?? 0,
            Assigned = aggregate?.Assigned ?? 0,
            AssignedToMe = aggregate?.AssignedToMe ?? 0,
            Overdue = aggregate?.Overdue ?? 0,
            DueSoon = aggregate?.DueSoon ?? 0
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

        var timelineRows = await _context.NhatKyDuyetDons.AsNoTracking()
            .Where(x => x.MaDonTu == applicationId)
            .OrderBy(x => x.NgayTao)
            .ThenBy(x => x.MaNkDuyet)
            .Select(x => new
            {
                x.MaNkDuyet,
                x.NguonThucHien,
                x.HanhDong,
                x.TrangThaiCu,
                x.TrangThaiMoi,
                x.GhiChuCongKhai,
                x.GhiChuNoiBo,
                x.SnapshotJson,
                x.HienThiChoHocSinh,
                x.NgayTao,
                NguoiThucHien = x.NguoiDuyet == null ? null : new AdminApplicationPersonDto
                {
                    MaNguoiDung = x.NguoiDuyet.MaNguoiDung,
                    HoTen = x.NguoiDuyet.HoTen,
                    Email = x.NguoiDuyet.Email,
                    VaiTro = AuthRoles.FromDatabaseCode(x.NguoiDuyet.VaiTroChinh)
                }
            })
            .ToListAsync(cancellationToken);
        var timeline = timelineRows
            .Select(x => new AdminApplicationTimelineDto
            {
                MaNkDuyet = x.MaNkDuyet,
                NguonThucHien = x.NguonThucHien,
                HanhDong = x.HanhDong,
                TrangThaiCu = x.TrangThaiCu,
                TrangThaiMoi = x.TrangThaiMoi,
                GhiChuCongKhai = x.GhiChuCongKhai,
                GhiChuNoiBo = x.GhiChuNoiBo,
                Metadata = ApplicationTimelineMetadataSanitizer.Sanitize(x.SnapshotJson),
                HienThiChoHocSinh = x.HienThiChoHocSinh,
                NgayTao = x.NgayTao,
                NguoiThucHien = x.NguoiThucHien
            })
            .ToList();

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
        var normalized = NormalizeAssigneeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = _scopeService.ApplyUserScope(_context.NguoiDungs.AsNoTracking(), actor)
            .Where(x => x.TrangThai == UserStatuses.DbActive)
            .Where(x => AssignableRoles.Select(AuthRoles.ToDatabaseCode).Contains(x.VaiTroChinh));

        if (normalized.CampusId.HasValue)
        {
            query = query.Where(x => x.MaDonVi == normalized.CampusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(normalized.Search))
        {
            var search = normalized.Search;
            query = query.Where(x => x.HoTen.Contains(search) || x.Email.Contains(search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.HoTen)
            .ThenBy(x => x.Email)
            .Skip((normalized.PageIndex - 1) * normalized.PageSize)
            .Take(normalized.PageSize)
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
            PageIndex = normalized.PageIndex,
            PageSize = normalized.PageSize,
            TotalItems = totalItems
        };
    }

    private IQueryable<DonTu> BuildFilteredQuery(
        NormalizedQueueQuery parameters,
        ApplicationActorContext actor)
    {
        var query = _scopeService.ApplyApplicationScope(_context.DonTus.AsNoTracking(), actor);
        if (string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(x => DefaultQueueStatuses.Contains(x.TrangThai));
        }
        else
        {
            query = query.Where(x => x.TrangThai == parameters.Status);
        }

        if (parameters.CampusId.HasValue)
        {
            query = query.Where(x => x.MaDonVi == parameters.CampusId.Value);
        }

        if (parameters.StudentId.HasValue)
        {
            query = query.Where(x => x.MaHocSinh == parameters.StudentId.Value);
        }

        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(x => x.NguoiDuyetHienTai == parameters.AssigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type))
        {
            query = query.Where(x => x.LoaiDon == parameters.Type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProcessingStatus))
        {
            query = query.Where(x => x.TrangThaiXuLyNghiepVu == parameters.ProcessingStatus);
        }

        if (parameters.SubmittedFrom.HasValue)
        {
            query = query.Where(x => x.NgayNop >= parameters.SubmittedFrom.Value);
        }

        if (parameters.SubmittedTo.HasValue)
        {
            query = query.Where(x => x.NgayNop <= parameters.SubmittedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignmentState) && parameters.AssignmentState != "all")
        {
            query = parameters.AssignmentState switch
            {
                "unassigned" => query.Where(x => x.NguoiDuyetHienTai == null),
                "assigned" => query.Where(x => x.NguoiDuyetHienTai != null),
                "mine" => query.Where(x => x.NguoiDuyetHienTai == actor.User.MaNguoiDung),
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái phân công không hợp lệ.")
            };
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search;
            if (int.TryParse(search, out var applicationId))
            {
                query = query.Where(x => x.MaDonTu == applicationId ||
                                         x.TieuDe.Contains(search) ||
                                         x.HocSinh!.HoTen.Contains(search) ||
                                         x.HocSinh.Email.Contains(search));
            }
            else
            {
                query = query.Where(x => x.TieuDe.Contains(search) ||
                                     x.LoaiDon.Contains(search) ||
                                     x.HocSinh!.HoTen.Contains(search) ||
                                     x.HocSinh.Email.Contains(search));
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.SlaStatus) && parameters.SlaStatus != "all")
        {
            query = ApplySlaStatusFilter(query, parameters.SlaStatus);
        }

        return query;
    }

    private IQueryable<DonTu> BuildSummaryQuery(
        NormalizedQueueQuery parameters,
        ApplicationActorContext actor)
    {
        var query = _scopeService.ApplyApplicationScope(_context.DonTus.AsNoTracking(), actor);
        query = string.IsNullOrWhiteSpace(parameters.Status)
            ? query.Where(x => ActiveQueueStatuses.Contains(x.TrangThai))
            : query.Where(x => x.TrangThai == parameters.Status);

        if (parameters.CampusId.HasValue)
        {
            query = query.Where(x => x.MaDonVi == parameters.CampusId.Value);
        }

        if (parameters.StudentId.HasValue)
        {
            query = query.Where(x => x.MaHocSinh == parameters.StudentId.Value);
        }

        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(x => x.NguoiDuyetHienTai == parameters.AssigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type))
        {
            query = query.Where(x => x.LoaiDon == parameters.Type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProcessingStatus))
        {
            query = query.Where(x => x.TrangThaiXuLyNghiepVu == parameters.ProcessingStatus);
        }

        if (parameters.SubmittedFrom.HasValue)
        {
            query = query.Where(x => x.NgayNop >= parameters.SubmittedFrom.Value);
        }

        if (parameters.SubmittedTo.HasValue)
        {
            query = query.Where(x => x.NgayNop <= parameters.SubmittedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.AssignmentState) && parameters.AssignmentState != "all")
        {
            query = parameters.AssignmentState switch
            {
                "unassigned" => query.Where(x => x.NguoiDuyetHienTai == null),
                "assigned" => query.Where(x => x.NguoiDuyetHienTai != null),
                "mine" => query.Where(x => x.NguoiDuyetHienTai == actor.User.MaNguoiDung),
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái phân công không hợp lệ.")
            };
        }

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search;
            if (int.TryParse(search, out var applicationId))
            {
                query = query.Where(x => x.MaDonTu == applicationId ||
                                         x.TieuDe.Contains(search) ||
                                         x.HocSinh!.HoTen.Contains(search) ||
                                         x.HocSinh.Email.Contains(search));
            }
            else
            {
                query = query.Where(x => x.TieuDe.Contains(search) ||
                                         x.LoaiDon.Contains(search) ||
                                         x.HocSinh!.HoTen.Contains(search) ||
                                         x.HocSinh.Email.Contains(search));
            }
        }

        if (!string.IsNullOrWhiteSpace(parameters.SlaStatus) && parameters.SlaStatus != "all")
        {
            query = ApplySlaStatusFilter(query, parameters.SlaStatus);
        }

        return query;
    }

    private IQueryable<DonTu> ApplyOrdering(IQueryable<DonTu> query, NormalizedQueueQuery parameters)
    {
        if (parameters.SortBy != "sla")
        {
            var descending = parameters.SortDirection == "desc";
            return parameters.SortBy switch
            {
                "submittedAt" => descending
                    ? query.OrderByDescending(x => x.NgayNop).ThenBy(x => x.MaDonTu)
                    : query.OrderBy(x => x.NgayNop).ThenBy(x => x.MaDonTu),
                "updatedAt" => descending
                    ? query.OrderByDescending(x => x.NgayCapNhat).ThenBy(x => x.MaDonTu)
                    : query.OrderBy(x => x.NgayCapNhat).ThenBy(x => x.MaDonTu),
                "studentName" => descending
                    ? query.OrderByDescending(x => x.HocSinh!.HoTen).ThenBy(x => x.MaDonTu)
                    : query.OrderBy(x => x.HocSinh!.HoTen).ThenBy(x => x.MaDonTu),
                _ => throw new ApiException(StatusCodes.Status400BadRequest, "Trường sắp xếp không hợp lệ.")
            };
        }

        var now = DateTime.UtcNow;
        var dueSoonDeadline = now.AddHours(_options.SlaWarningBeforeHours);
        var ordered = query
            .OrderByDescending(x => RunningSlaStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value < now)
            .ThenByDescending(x => RunningSlaStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value >= now && x.HanXuLyLuc.Value <= dueSoonDeadline)
            .ThenByDescending(x => x.NguoiDuyetHienTai == null)
            .ThenBy(x => x.HanXuLyLuc)
            .ThenBy(x => x.NgayNop)
            .ThenBy(x => x.MaDonTu);
        return parameters.SortDirection == "desc"
            ? query
                .OrderBy(x => RunningSlaStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value < now)
                .ThenBy(x => RunningSlaStatuses.Contains(x.TrangThai) && x.HanXuLyLuc.HasValue && x.HanXuLyLuc.Value >= now && x.HanXuLyLuc.Value <= dueSoonDeadline)
                .ThenBy(x => x.NguoiDuyetHienTai == null)
                .ThenByDescending(x => x.HanXuLyLuc)
                .ThenByDescending(x => x.NgayNop)
                .ThenBy(x => x.MaDonTu)
            : ordered;
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
            "overdue" => query.Where(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                          x.HanXuLyLuc.HasValue &&
                                          x.HanXuLyLuc.Value < now),
            "due_soon" => query.Where(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value >= now &&
                                           x.HanXuLyLuc.Value <= dueSoonDeadline),
            "on_track" => query.Where(x => RunningSlaStatuses.Contains(x.TrangThai) &&
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
            TrangThaiXuLyNghiepVu = row.TrangThaiXuLyNghiepVu,
            TenTrangThaiXuLyNghiepVu = GetProcessingStatusLabel(row.TrangThaiXuLyNghiepVu),
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
            AttachmentCount = row.AttachmentCount,
            RowVersion = Convert.ToBase64String(row.RowVersion)
        };
    }

    private AdminApplicationDetailDto ToDetailDto(ApplicationProjection row, ApplicationActorContext actor)
    {
        var item = ToQueueItemDto(row);
        var (form, valid) = ParseFormJson(row.DuLieuBieuMau);
        var detail = new AdminApplicationDetailDto
        {
            MaDonTu = item.MaDonTu,
            LoaiDon = item.LoaiDon,
            TenLoaiDon = item.TenLoaiDon,
            TieuDe = item.TieuDe,
            TrangThai = item.TrangThai,
            TenTrangThai = item.TenTrangThai,
            TrangThaiXuLyNghiepVu = item.TrangThaiXuLyNghiepVu,
            TenTrangThaiXuLyNghiepVu = item.TenTrangThaiXuLyNghiepVu,
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
            AllowedActions = _permissionEvaluator.BuildAllowedActions(new DonTu
            {
                MaDonTu = row.MaDonTu,
                MaDonVi = row.MaDonVi,
                TrangThai = row.TrangThai,
                TrangThaiXuLyNghiepVu = row.TrangThaiXuLyNghiepVu,
                NguoiDuyetHienTai = row.NguoiDuyetHienTai
            }, actor)
        };
        _processingPermissionEvaluator.ApplyAllowedActions(detail.AllowedActions, new DonTu
        {
            MaDonTu = row.MaDonTu,
            MaDonVi = row.MaDonVi,
            TrangThai = row.TrangThai,
            TrangThaiXuLyNghiepVu = row.TrangThaiXuLyNghiepVu
        }, actor);
        return detail;
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

    private static NormalizedQueueQuery NormalizeQuery(
        AdminApplicationQueryParameters parameters,
        ApplicationActorContext actor,
        bool allowPaging = true)
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

        var submittedFrom = parameters.SubmittedFrom ?? parameters.TuNgayNop;
        var submittedTo = parameters.SubmittedTo ?? parameters.DenNgayNop;
        if (submittedFrom.HasValue && submittedTo.HasValue && submittedFrom > submittedTo)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ ngày nộp phải nhỏ hơn hoặc bằng đến ngày nộp.");
        }

        var search = parameters.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search) && search.Length > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ khóa tìm kiếm vượt quá độ dài cho phép.");
        }

        var assignmentState = string.IsNullOrWhiteSpace(parameters.AssignmentState)
            ? "all"
            : parameters.AssignmentState.Trim().ToLowerInvariant();
        if (assignmentState is not ("all" or "unassigned" or "assigned" or "mine"))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái phân công không hợp lệ.");
        }

        var slaStatus = string.IsNullOrWhiteSpace(parameters.SlaStatus)
            ? "all"
            : parameters.SlaStatus.Trim().ToLowerInvariant();
        if (slaStatus is not ("all" or "none" or "on_track" or "due_soon" or "overdue" or "paused"))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái SLA không hợp lệ.");
        }

        var sortBy = string.IsNullOrWhiteSpace(parameters.SortBy)
            ? "sla"
            : parameters.SortBy.Trim();
        if (sortBy is not ("sla" or "submittedAt" or "updatedAt" or "studentName"))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trường sắp xếp không hợp lệ.");
        }

        var sortDirection = string.IsNullOrWhiteSpace(parameters.SortDirection)
            ? "asc"
            : parameters.SortDirection.Trim().ToLowerInvariant();
        if (sortDirection is not ("asc" or "desc"))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chiều sắp xếp không hợp lệ.");
        }

        return new NormalizedQueueQuery
        {
            CampusId = parameters.CampusId ?? parameters.MaDonVi,
            StudentId = parameters.StudentId ?? parameters.MaHocSinh,
            AssigneeId = parameters.AssigneeId ?? parameters.NguoiDuyetHienTai,
            Type = string.IsNullOrWhiteSpace(parameters.Type ?? parameters.LoaiDon)
                ? null
                : NormalizeType((parameters.Type ?? parameters.LoaiDon)!),
            Status = string.IsNullOrWhiteSpace(parameters.Status ?? parameters.TrangThai)
                ? null
                : NormalizeStatus((parameters.Status ?? parameters.TrangThai)!),
            ProcessingStatus = NormalizeProcessingStatusAlias(parameters.ProcessingStatus, parameters.TrangThaiXuLyNghiepVu),
            AssignmentState = assignmentState,
            SlaStatus = slaStatus,
            SubmittedFrom = submittedFrom,
            SubmittedTo = submittedTo,
            Search = string.IsNullOrWhiteSpace(search) ? null : search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            ActorId = actor.User.MaNguoiDung
        };
    }

    private static NormalizedAssigneeQuery NormalizeAssigneeQuery(AdminApplicationAssigneeQueryParameters parameters)
    {
        if (parameters.PageIndex < 1)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageIndex phải lớn hơn hoặc bằng 1.");
        }

        if (parameters.PageSize is < 1 or > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "PageSize phải từ 1 đến 100.");
        }

        var search = parameters.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search) && search.Length > 100)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ khóa tìm kiếm vượt quá độ dài cho phép.");
        }

        return new NormalizedAssigneeQuery
        {
            CampusId = parameters.CampusId ?? parameters.MaDonVi,
            Search = string.IsNullOrWhiteSpace(search) ? null : search,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize
        };
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

    private static string? NormalizeProcessingStatusAlias(string? processingStatus, string? vietnameseProcessingStatus)
    {
        var first = string.IsNullOrWhiteSpace(processingStatus) ? null : processingStatus.Trim();
        var second = string.IsNullOrWhiteSpace(vietnameseProcessingStatus) ? null : vietnameseProcessingStatus.Trim();
        if (first is not null &&
            second is not null &&
            !first.Equals(second, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái xử lý nghiệp vụ không thống nhất.");
        }

        var value = first ?? second;
        if (value is null)
        {
            return null;
        }

        var canonical = ApplicationProcessingStatuses.All.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái xử lý nghiệp vụ không hợp lệ.");
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

    private static string GetProcessingStatusLabel(string status)
    {
        return status switch
        {
            ApplicationProcessingStatuses.NotProcessed => "Chưa xử lý",
            ApplicationProcessingStatuses.Pending => "Chờ xử lý",
            ApplicationProcessingStatuses.Recorded => "Đã ghi nhận",
            ApplicationProcessingStatuses.Succeeded => "Xử lý thành công",
            ApplicationProcessingStatuses.Failed => "Xử lý thất bại",
            ApplicationProcessingStatuses.ManualRequired => "Cần xử lý thủ công",
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
        public int AttachmentCount { get; set; }
    }

    private sealed class NormalizedQueueQuery
    {
        public int? CampusId { get; set; }
        public int? StudentId { get; set; }
        public int? AssigneeId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? ProcessingStatus { get; set; }
        public string AssignmentState { get; set; } = "all";
        public string SlaStatus { get; set; } = "all";
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sla";
        public string SortDirection { get; set; } = "asc";
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int ActorId { get; set; }
    }

    private sealed class NormalizedAssigneeQuery
    {
        public int? CampusId { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
