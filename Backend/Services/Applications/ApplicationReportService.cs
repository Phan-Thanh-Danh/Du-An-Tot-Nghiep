using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Applications;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Services.Applications;

public class ApplicationReportService : IApplicationReportService
{
    private static readonly string[] RunningSlaStatuses =
    [
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview
    ];

    private static readonly string[] OrderedApplicationStatuses =
    [
        ApplicationStatuses.Draft,
        ApplicationStatuses.Submitted,
        ApplicationStatuses.InReview,
        ApplicationStatuses.NeedSupplement,
        ApplicationStatuses.Approved,
        ApplicationStatuses.Rejected,
        ApplicationStatuses.Cancelled
    ];

    private static readonly string[] OrderedProcessingStatuses =
    [
        ApplicationProcessingStatuses.NotProcessed,
        ApplicationProcessingStatuses.Pending,
        ApplicationProcessingStatuses.Recorded,
        ApplicationProcessingStatuses.Succeeded,
        ApplicationProcessingStatuses.Failed,
        ApplicationProcessingStatuses.ManualRequired
    ];

    private static readonly string[] OrderedApplicationTypes =
    [
        ApplicationTypes.Leave,
        ApplicationTypes.RetakeExam,
        ApplicationTypes.TransferSchool,
        ApplicationTypes.Certificate,
        ApplicationTypes.Other,
        ApplicationTypes.GradeAppeal,
        ApplicationTypes.AcademicPause,
        ApplicationTypes.ChangeMajor,
        ApplicationTypes.ChangeCampus,
        ApplicationTypes.Confirmation,
        ApplicationTypes.Withdrawal
    ];

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;
    private readonly ApplicationQueueOptions _options;

    public ApplicationReportService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService,
        IOptions<ApplicationQueueOptions> options)
    {
        _context = context;
        _scopeService = scopeService;
        _options = options.Value;
    }

    public async Task<ApplicationReportOverviewDto> GetOverviewAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var now = DateTime.UtcNow;
        var dueSoonDeadline = now.AddHours(_options.SlaWarningBeforeHours);
        var query = BuildFilteredQuery(normalized, actor);

        var summary = await GetSummaryAsync(query, now, dueSoonDeadline, cancellationToken);
        var statusCounts = await GetStatusCountsAsync(query, cancellationToken);
        var processingCounts = await GetProcessingCountsAsync(query, cancellationToken);
        var typeBreakdown = await GetTypeBreakdownAsync(query, cancellationToken);
        var campusBreakdown = await GetCampusBreakdownAsync(query, now, cancellationToken);
        var averageReviewHours = await GetAverageReviewHoursAsync(query, cancellationToken);

        var decidedCount = summary.Approved + summary.Rejected;
        return new ApplicationReportOverviewDto
        {
            GeneratedAtUtc = now,
            Filters = normalized.ToDto(),
            Summary = summary,
            StatusBreakdown = BuildStatusBuckets(statusCounts),
            ProcessingStatusBreakdown = BuildProcessingBuckets(processingCounts),
            TypeBreakdown = BuildTypeBuckets(typeBreakdown),
            CampusBreakdown = campusBreakdown,
            ReviewPerformance = new ApplicationReportReviewPerformanceDto
            {
                DecidedCount = decidedCount,
                ApprovalRate = CalculateRate(summary.Approved, decidedCount),
                RejectionRate = CalculateRate(summary.Rejected, decidedCount),
                AverageReviewHours = averageReviewHours
            }
        };
    }

    private IQueryable<DonTu> BuildFilteredQuery(NormalizedReportQuery parameters, ApplicationActorContext actor)
    {
        var query = _scopeService.ApplyApplicationScope(_context.DonTus.AsNoTracking(), actor);

        if (parameters.CampusId.HasValue)
        {
            query = query.Where(x => x.MaDonVi == parameters.CampusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Type))
        {
            query = query.Where(x => x.LoaiDon == parameters.Type);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status))
        {
            query = query.Where(x => x.TrangThai == parameters.Status);
        }

        if (!string.IsNullOrWhiteSpace(parameters.ProcessingStatus))
        {
            query = query.Where(x => x.TrangThaiXuLyNghiepVu == parameters.ProcessingStatus);
        }

        if (parameters.AssigneeId.HasValue)
        {
            query = query.Where(x => x.NguoiDuyetHienTai == parameters.AssigneeId.Value);
        }

        if (parameters.ProcessorId.HasValue)
        {
            query = query.Where(x => x.NguoiXuLyCuoi == parameters.ProcessorId.Value);
        }

        if (parameters.SubmittedFrom.HasValue)
        {
            query = query.Where(x => x.NgayNop.HasValue && x.NgayNop.Value >= parameters.SubmittedFrom.Value);
        }

        if (parameters.SubmittedTo.HasValue)
        {
            var submittedToEndOfDay = parameters.SubmittedTo.Value.TimeOfDay == TimeSpan.Zero
                ? parameters.SubmittedTo.Value.Date.AddDays(1)
                : parameters.SubmittedTo.Value.AddDays(1);
            query = query.Where(x => x.NgayNop.HasValue && x.NgayNop.Value < submittedToEndOfDay);
        }

        return query;
    }

    private static async Task<ApplicationReportSummaryDto> GetSummaryAsync(
        IQueryable<DonTu> query,
        DateTime now,
        DateTime dueSoonDeadline,
        CancellationToken cancellationToken)
    {
        var aggregate = await query
            .TagWith("P0-DT7 ReportSummary")
            .GroupBy(_ => 1)
            .Select(group => new
            {
                TotalApplications = group.Count(),
                PendingReview = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai)),
                WaitingForSupplement = group.Count(x => x.TrangThai == ApplicationStatuses.NeedSupplement),
                Overdue = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value < now),
                DueSoon = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value >= now &&
                                           x.HanXuLyLuc.Value <= dueSoonDeadline),
                Approved = group.Count(x => x.TrangThai == ApplicationStatuses.Approved),
                Rejected = group.Count(x => x.TrangThai == ApplicationStatuses.Rejected),
                Cancelled = group.Count(x => x.TrangThai == ApplicationStatuses.Cancelled),
                PendingProcessing = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Pending),
                ManualRequired = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.ManualRequired),
                ProcessingRecorded = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Recorded),
                ProcessingSucceeded = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Succeeded),
                ProcessingFailed = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Failed)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new ApplicationReportSummaryDto
        {
            TotalApplications = aggregate?.TotalApplications ?? 0,
            PendingReview = aggregate?.PendingReview ?? 0,
            WaitingForSupplement = aggregate?.WaitingForSupplement ?? 0,
            Overdue = aggregate?.Overdue ?? 0,
            DueSoon = aggregate?.DueSoon ?? 0,
            Approved = aggregate?.Approved ?? 0,
            Rejected = aggregate?.Rejected ?? 0,
            Cancelled = aggregate?.Cancelled ?? 0,
            PendingProcessing = aggregate?.PendingProcessing ?? 0,
            ManualRequired = aggregate?.ManualRequired ?? 0,
            ProcessingRecorded = aggregate?.ProcessingRecorded ?? 0,
            ProcessingSucceeded = aggregate?.ProcessingSucceeded ?? 0,
            ProcessingFailed = aggregate?.ProcessingFailed ?? 0
        };
    }

    private static async Task<Dictionary<string, int>> GetStatusCountsAsync(
        IQueryable<DonTu> query,
        CancellationToken cancellationToken)
    {
        return await query
            .TagWith("P0-DT7 ReportByStatus")
            .GroupBy(x => x.TrangThai)
            .Select(group => new { Code = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private static async Task<Dictionary<string, int>> GetProcessingCountsAsync(
        IQueryable<DonTu> query,
        CancellationToken cancellationToken)
    {
        return await query
            .TagWith("P0-DT7 ReportByProcessingStatus")
            .GroupBy(x => x.TrangThaiXuLyNghiepVu)
            .Select(group => new { Code = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Code, x => x.Count, StringComparer.OrdinalIgnoreCase, cancellationToken);
    }

    private static async Task<IReadOnlyList<TypeBreakdownRow>> GetTypeBreakdownAsync(
        IQueryable<DonTu> query,
        CancellationToken cancellationToken)
    {
        return await query
            .TagWith("P0-DT7 ReportByType")
            .GroupBy(x => x.LoaiDon)
            .Select(group => new TypeBreakdownRow
            {
                Code = group.Key,
                Count = group.Count(),
                Approved = group.Count(x => x.TrangThai == ApplicationStatuses.Approved),
                Rejected = group.Count(x => x.TrangThai == ApplicationStatuses.Rejected),
                PendingReview = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai)),
                ManualRequired = group.Count(x => x.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.ManualRequired)
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ApplicationReportCampusBreakdownDto>> GetCampusBreakdownAsync(
        IQueryable<DonTu> query,
        DateTime now,
        CancellationToken cancellationToken)
    {
        return await query
            .TagWith("P0-DT7 ReportByCampus")
            .GroupBy(x => new { x.MaDonVi, CampusName = x.DonVi != null ? x.DonVi.TenDonVi : string.Empty })
            .Select(group => new ApplicationReportCampusBreakdownDto
            {
                CampusId = group.Key.MaDonVi,
                CampusName = group.Key.CampusName,
                Total = group.Count(),
                PendingReview = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai)),
                Approved = group.Count(x => x.TrangThai == ApplicationStatuses.Approved),
                Rejected = group.Count(x => x.TrangThai == ApplicationStatuses.Rejected),
                Overdue = group.Count(x => RunningSlaStatuses.Contains(x.TrangThai) &&
                                           x.HanXuLyLuc.HasValue &&
                                           x.HanXuLyLuc.Value < now)
            })
            .OrderBy(x => x.CampusName)
            .ThenBy(x => x.CampusId)
            .ToListAsync(cancellationToken);
    }

    private async Task<decimal?> GetAverageReviewHoursAsync(
        IQueryable<DonTu> query,
        CancellationToken cancellationToken)
    {
        var averageMinutes = await query
            .TagWith("P0-DT7 ReportReviewDuration")
            .Where(x => (x.TrangThai == ApplicationStatuses.Approved || x.TrangThai == ApplicationStatuses.Rejected) &&
                        x.NgayNop.HasValue)
            .Select(x => new
            {
                SubmittedAt = x.NgayNop!.Value,
                DecisionAt = x.TrangThai == ApplicationStatuses.Approved
                    ? (_context.NhatKyDuyetDons
                        .Where(log => log.MaDonTu == x.MaDonTu && log.HanhDong == ApplicationActions.Approve)
                        .Max(log => (DateTime?)log.NgayTao) ?? x.NgayDuyet)
                    : (_context.NhatKyDuyetDons
                        .Where(log => log.MaDonTu == x.MaDonTu && log.HanhDong == ApplicationActions.Reject)
                        .Max(log => (DateTime?)log.NgayTao) ?? x.NgayDuyet)
            })
            .Where(x => x.DecisionAt.HasValue && x.DecisionAt.Value >= x.SubmittedAt)
            .Select(x => (double?)EF.Functions.DateDiffMinute(x.SubmittedAt, x.DecisionAt!.Value))
            .AverageAsync(cancellationToken);

        return averageMinutes.HasValue
            ? Math.Round((decimal)(averageMinutes.Value / 60d), 2, MidpointRounding.AwayFromZero)
            : null;
    }

    private static IReadOnlyList<ApplicationReportBucketDto> BuildStatusBuckets(Dictionary<string, int> counts)
    {
        return OrderedApplicationStatuses
            .Select(status => new ApplicationReportBucketDto
            {
                Code = status,
                Label = GetStatusLabel(status),
                Count = counts.GetValueOrDefault(status)
            })
            .ToList();
    }

    private static IReadOnlyList<ApplicationReportBucketDto> BuildProcessingBuckets(Dictionary<string, int> counts)
    {
        return OrderedProcessingStatuses
            .Select(status => new ApplicationReportBucketDto
            {
                Code = status,
                Label = GetProcessingStatusLabel(status),
                Count = counts.GetValueOrDefault(status)
            })
            .ToList();
    }

    private static IReadOnlyList<ApplicationReportTypeBreakdownDto> BuildTypeBuckets(IReadOnlyList<TypeBreakdownRow> rows)
    {
        var byCode = rows.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var result = OrderedApplicationTypes
            .Select(type =>
            {
                byCode.TryGetValue(type, out var row);
                return new ApplicationReportTypeBreakdownDto
                {
                    Code = type,
                    Label = ApplicationSchemaService.GetTypeLabel(type),
                    Count = row?.Count ?? 0,
                    Approved = row?.Approved ?? 0,
                    Rejected = row?.Rejected ?? 0,
                    PendingReview = row?.PendingReview ?? 0,
                    ManualRequired = row?.ManualRequired ?? 0
                };
            })
            .ToList();

        foreach (var legacy in rows.Where(row => !OrderedApplicationTypes.Contains(row.Code, StringComparer.OrdinalIgnoreCase))
                     .OrderBy(row => row.Code, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(new ApplicationReportTypeBreakdownDto
            {
                Code = legacy.Code,
                Label = ApplicationSchemaService.GetTypeLabel(legacy.Code),
                Count = legacy.Count,
                Approved = legacy.Approved,
                Rejected = legacy.Rejected,
                PendingReview = legacy.PendingReview,
                ManualRequired = legacy.ManualRequired
            });
        }

        return result;
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        return denominator <= 0
            ? 0
            : Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero);
    }

    private static NormalizedReportQuery NormalizeQuery(ApplicationReportQueryParameters parameters)
    {
        var campusId = ResolveAlias(parameters.CampusId, parameters.MaDonVi, "Cơ sở không thống nhất.");
        var assigneeId = ResolveAlias(parameters.AssigneeId, parameters.NguoiDuyetHienTai, "Người duyệt hiện tại không thống nhất.");
        var processorId = ResolveAlias(parameters.ProcessorId, parameters.NguoiXuLyCuoi, "Người xử lý cuối không thống nhất.");
        ValidatePositive(campusId, "Cơ sở không hợp lệ.");
        ValidatePositive(assigneeId, "Người duyệt hiện tại không hợp lệ.");
        ValidatePositive(processorId, "Người xử lý cuối không hợp lệ.");

        var submittedFrom = ResolveAlias(parameters.SubmittedFrom, parameters.TuNgayNop, "Từ ngày nộp không thống nhất.");
        var submittedTo = ResolveAlias(parameters.SubmittedTo, parameters.DenNgayNop, "Đến ngày nộp không thống nhất.");
        if (submittedFrom.HasValue && submittedTo.HasValue && submittedFrom > submittedTo)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Từ ngày nộp phải nhỏ hơn hoặc bằng đến ngày nộp.");
        }

        return new NormalizedReportQuery
        {
            CampusId = campusId,
            Type = NormalizeOptionalStringAlias(parameters.Type, parameters.LoaiDon, ApplicationTypes.All, "Loại đơn không thống nhất.", "Loại đơn không hợp lệ."),
            Status = NormalizeOptionalStringAlias(parameters.Status, parameters.TrangThai, ApplicationStatuses.All, "Trạng thái đơn không thống nhất.", "Trạng thái đơn không hợp lệ."),
            ProcessingStatus = NormalizeOptionalStringAlias(parameters.ProcessingStatus, parameters.TrangThaiXuLyNghiepVu, ApplicationProcessingStatuses.All, "Trạng thái xử lý nghiệp vụ không thống nhất.", "Trạng thái xử lý nghiệp vụ không hợp lệ."),
            AssigneeId = assigneeId,
            ProcessorId = processorId,
            SubmittedFrom = submittedFrom,
            SubmittedTo = submittedTo
        };
    }

    private static int? ResolveAlias(int? first, int? second, string conflictMessage)
    {
        if (first.HasValue && second.HasValue && first.Value != second.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, conflictMessage);
        }

        return first ?? second;
    }

    private static DateTime? ResolveAlias(DateTime? first, DateTime? second, string conflictMessage)
    {
        if (first.HasValue && second.HasValue && first.Value != second.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, conflictMessage);
        }

        return first ?? second;
    }

    private static void ValidatePositive(int? value, string message)
    {
        if (value is <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }
    }

    private static string? NormalizeOptionalStringAlias(
        string? first,
        string? second,
        IReadOnlySet<string> allowed,
        string conflictMessage,
        string invalidMessage)
    {
        var firstValue = string.IsNullOrWhiteSpace(first) ? null : first.Trim();
        var secondValue = string.IsNullOrWhiteSpace(second) ? null : second.Trim();
        if (firstValue is not null &&
            secondValue is not null &&
            !firstValue.Equals(secondValue, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, conflictMessage);
        }

        var value = firstValue ?? secondValue;
        if (value is null)
        {
            return null;
        }

        var canonical = allowed.FirstOrDefault(x => x.Equals(value, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, invalidMessage);
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

    private sealed class TypeBreakdownRow
    {
        public string Code { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int PendingReview { get; set; }
        public int ManualRequired { get; set; }
    }


    public async Task<List<ApplicationByTypeReportDto>> GetByTypeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);

        var grouped = await query
            .GroupBy(a => new { a.LoaiDon, a.MauDon!.TenMau })
            .Select(g => new
            {
                Type = g.Key.LoaiDon,
                TypeName = g.Key.TenMau,
                Total = g.Count(),
                Pending = g.Count(a => a.TrangThai == ApplicationStatuses.Submitted || a.TrangThai == ApplicationStatuses.InReview || a.TrangThai == ApplicationStatuses.NeedSupplement),
                Processing = g.Count(a => a.TrangThai == ApplicationStatuses.Approved && a.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Pending),
                Approved = g.Count(a => a.TrangThai == ApplicationStatuses.Approved),
                Rejected = g.Count(a => a.TrangThai == ApplicationStatuses.Rejected),
                Canceled = g.Count(a => a.TrangThai == ApplicationStatuses.Cancelled),
                Overdue = g.Count(a => a.HanXuLyLuc.HasValue && a.HanXuLyLuc < DateTime.UtcNow && RunningSlaStatuses.Contains(a.TrangThai)),
                TotalHours = g.Where(a => a.NgayDuyet.HasValue && a.NgayNop.HasValue).Sum(a => EF.Functions.DateDiffHour(a.NgayNop.Value, a.NgayDuyet.Value)),
                CountWithHours = g.Count(a => a.NgayDuyet.HasValue && a.NgayNop.HasValue)
            })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => new ApplicationByTypeReportDto
        {
            ApplicationTypeId = g.Type,
            ApplicationTypeName = g.TypeName ?? g.Type,
            Total = g.Total,
            Pending = g.Pending,
            Processing = g.Processing,
            Approved = g.Approved,
            Rejected = g.Rejected,
            Canceled = g.Canceled,
            Overdue = g.Overdue,
            AverageProcessingHours = g.CountWithHours > 0 ? (decimal)g.TotalHours / g.CountWithHours : null,
            ApprovalRate = g.Total > 0 ? Math.Round((decimal)g.Approved / g.Total * 100, 2) : 0,
            RejectionRate = g.Total > 0 ? Math.Round((decimal)g.Rejected / g.Total * 100, 2) : 0
        }).OrderByDescending(x => x.Total).ToList();
    }

    public async Task<PendingApplicationReportDto> GetPendingAsync(
        PendingApplicationReportQuery parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);
        
        query = query.Where(a => 
            a.TrangThai == ApplicationStatuses.Submitted || 
            a.TrangThai == ApplicationStatuses.InReview || 
            a.TrangThai == ApplicationStatuses.NeedSupplement ||
            (a.TrangThai == ApplicationStatuses.Approved && a.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Pending) ||
            (a.TrangThai == ApplicationStatuses.Approved && a.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Recorded));

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.NgayNop)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(a => new PendingApplicationReportItemDto
            {
                ApplicationId = a.MaDonTu,
                ApplicationCode = a.MaDonTu.ToString(),
                ApplicationType = a.LoaiDon,
                StudentCode = a.HocSinh!.MaNguoiDung.ToString(),
                StudentName = a.HocSinh.HoTen,
                Status = a.TrangThai,
                SubmittedAt = a.NgayNop,
                ReceivedAt = a.NgayCapNhat,
                AssignedToName = a.NguoiDuyetHienTaiNavigation != null ? a.NguoiDuyetHienTaiNavigation.HoTen : null,
                CurrentStep = a.TrangThaiXuLyNghiepVu,
                AgeHours = a.NgayNop.HasValue ? (decimal)EF.Functions.DateDiffHour(a.NgayNop.Value, DateTime.UtcNow) : 0,
                CampusId = a.MaDonVi,
                CampusName = a.DonVi != null ? a.DonVi.TenDonVi : null
            })
            .ToListAsync(cancellationToken);

        return new PendingApplicationReportDto
        {
            TotalCount = total,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            Items = items
        };
    }

    public async Task<OverdueApplicationReportDto> GetOverdueAsync(
        OverdueApplicationReportQuery parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);
        
        query = query.Where(a => RunningSlaStatuses.Contains(a.TrangThai));

        var now = DateTime.UtcNow;
        var defaultSlaHours = parameters.SlaHours ?? 48; // default to 48 hours if not set

        // Overdue logic: if HanXuLyLuc has value, use it. Otherwise, use NgayNop + defaultSlaHours
        var overdueQuery = query.Where(a => 
            (a.HanXuLyLuc.HasValue && a.HanXuLyLuc < now) ||
            (!a.HanXuLyLuc.HasValue && a.NgayNop.HasValue && EF.Functions.DateDiffHour(a.NgayNop.Value, now) > defaultSlaHours)
        );

        var total = await overdueQuery.CountAsync(cancellationToken);

        var items = await overdueQuery
            .OrderBy(a => a.HanXuLyLuc ?? a.NgayNop)
            .Skip((parameters.PageIndex - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(a => new OverdueApplicationReportItemDto
            {
                ApplicationId = a.MaDonTu,
                ApplicationCode = a.MaDonTu.ToString(),
                ApplicationType = a.LoaiDon,
                Status = a.TrangThai,
                SubmittedAt = a.NgayNop,
                AssignedToName = a.NguoiDuyetHienTaiNavigation != null ? a.NguoiDuyetHienTaiNavigation.HoTen : null,
                AgeHours = a.NgayNop.HasValue ? (decimal)EF.Functions.DateDiffHour(a.NgayNop.Value, now) : 0,
                OverdueHours = a.HanXuLyLuc.HasValue 
                    ? (decimal)EF.Functions.DateDiffHour(a.HanXuLyLuc.Value, now)
                    : (a.NgayNop.HasValue ? (decimal)EF.Functions.DateDiffHour(a.NgayNop.Value, now) - defaultSlaHours : 0)
            })
            .ToListAsync(cancellationToken);

        return new OverdueApplicationReportDto
        {
            TotalOverdue = total,
            DefaultSlaHours = defaultSlaHours,
            PageIndex = parameters.PageIndex,
            PageSize = parameters.PageSize,
            Items = items
        };
    }

    public async Task<ProcessingTimeReportDto> GetProcessingTimeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);
        
        // Only completed (decided)
        query = query.Where(a => a.NgayDuyet.HasValue && a.NgayNop.HasValue && 
            (a.TrangThai == ApplicationStatuses.Approved || a.TrangThai == ApplicationStatuses.Rejected));

        var stats = await query
            .Select(a => EF.Functions.DateDiffHour(a.NgayNop!.Value, a.NgayDuyet!.Value))
            .ToListAsync(cancellationToken);

        var count = stats.Count;
        decimal avg = count > 0 ? (decimal)stats.Average() : 0;
        decimal min = count > 0 ? stats.Min() : 0;
        decimal max = count > 0 ? stats.Max() : 0;
        decimal? median = null;

        if (count > 0)
        {
            var sorted = stats.OrderBy(x => x).ToList();
            if (count % 2 == 0)
            {
                median = (sorted[(count / 2) - 1] + sorted[count / 2]) / 2.0m;
            }
            else
            {
                median = sorted[count / 2];
            }
        }

        var byType = await query
            .GroupBy(a => a.LoaiDon)
            .Select(g => new ProcessingTimeGroupDto
            {
                GroupKey = g.Key,
                GroupLabel = g.Key,
                ApplicationCount = g.Count(),
                AverageProcessingHours = (decimal)g.Average(a => EF.Functions.DateDiffHour(a.NgayNop!.Value, a.NgayDuyet!.Value))
            })
            .ToListAsync(cancellationToken);

        var byAssignee = await query
            .Where(a => a.NguoiXuLyCuoi != null)
            .GroupBy(a => a.NguoiXuLyCuoiNavigation!.HoTen)
            .Select(g => new ProcessingTimeGroupDto
            {
                GroupKey = g.Key,
                GroupLabel = g.Key,
                ApplicationCount = g.Count(),
                AverageProcessingHours = (decimal)g.Average(a => EF.Functions.DateDiffHour(a.NgayNop!.Value, a.NgayDuyet!.Value))
            })
            .ToListAsync(cancellationToken);

        var byCampus = await query
            .GroupBy(a => new { a.MaDonVi, a.DonVi!.TenDonVi })
            .Select(g => new ProcessingTimeGroupDto
            {
                GroupKey = g.Key.MaDonVi.ToString(),
                GroupLabel = g.Key.TenDonVi,
                ApplicationCount = g.Count(),
                AverageProcessingHours = (decimal)g.Average(a => EF.Functions.DateDiffHour(a.NgayNop!.Value, a.NgayDuyet!.Value))
            })
            .ToListAsync(cancellationToken);

        var byMonth = await query
            .GroupBy(a => new { a.NgayDuyet!.Value.Year, a.NgayDuyet.Value.Month })
            .Select(g => new ProcessingTimeGroupDto
            {
                GroupKey = $"{g.Key.Year}-{g.Key.Month:D2}",
                GroupLabel = $"{g.Key.Year}-{g.Key.Month:D2}",
                ApplicationCount = g.Count(),
                AverageProcessingHours = (decimal)g.Average(a => EF.Functions.DateDiffHour(a.NgayNop!.Value, a.NgayDuyet!.Value))
            })
            .ToListAsync(cancellationToken);

        return new ProcessingTimeReportDto
        {
            AverageProcessingHours = avg,
            MedianProcessingHours = median,
            MinProcessingHours = min,
            MaxProcessingHours = max,
            ByType = byType.OrderByDescending(x => x.ApplicationCount).ToList(),
            ByAssignee = byAssignee.OrderByDescending(x => x.ApplicationCount).ToList(),
            ByCampus = byCampus.OrderByDescending(x => x.ApplicationCount).ToList(),
            ByMonth = byMonth.OrderBy(x => x.GroupKey).ToList()
        };
    }

    public async Task<List<ApplicationByAssigneeReportDto>> GetByAssigneeAsync(
        ApplicationReportQueryParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);
        
        var grouped = await query
            .Where(a => a.NguoiDuyetHienTai != null || a.NguoiXuLyCuoi != null)
            .GroupBy(a => new { 
                Id = a.NguoiDuyetHienTai ?? a.NguoiXuLyCuoi, 
                Name = a.NguoiDuyetHienTaiNavigation != null ? a.NguoiDuyetHienTaiNavigation.HoTen : (a.NguoiXuLyCuoiNavigation != null ? a.NguoiXuLyCuoiNavigation.HoTen : "Unknown")
            })
            .Select(g => new
            {
                AssigneeId = g.Key.Id,
                AssigneeName = g.Key.Name,
                Total = g.Count(),
                Pending = g.Count(a => a.TrangThai == ApplicationStatuses.Submitted || a.TrangThai == ApplicationStatuses.InReview || a.TrangThai == ApplicationStatuses.NeedSupplement),
                Processing = g.Count(a => a.TrangThai == ApplicationStatuses.Approved && a.TrangThaiXuLyNghiepVu == ApplicationProcessingStatuses.Pending),
                Completed = g.Count(a => a.TrangThai == ApplicationStatuses.Approved || a.TrangThai == ApplicationStatuses.Rejected),
                Approved = g.Count(a => a.TrangThai == ApplicationStatuses.Approved),
                Rejected = g.Count(a => a.TrangThai == ApplicationStatuses.Rejected),
                Overdue = g.Count(a => a.HanXuLyLuc.HasValue && a.HanXuLyLuc < DateTime.UtcNow && RunningSlaStatuses.Contains(a.TrangThai)),
                TotalHours = g.Where(a => a.NgayDuyet.HasValue && a.NgayNop.HasValue).Sum(a => EF.Functions.DateDiffHour(a.NgayNop.Value, a.NgayDuyet.Value)),
                CountWithHours = g.Count(a => a.NgayDuyet.HasValue && a.NgayNop.HasValue)
            })
            .ToListAsync(cancellationToken);

        return grouped.Select(g => new ApplicationByAssigneeReportDto
        {
            AssigneeId = g.AssigneeId,
            AssigneeName = g.AssigneeName,
            TotalAssigned = g.Total,
            Pending = g.Pending,
            Processing = g.Processing,
            Completed = g.Completed,
            Approved = g.Approved,
            Rejected = g.Rejected,
            Overdue = g.Overdue,
            AverageProcessingHours = g.CountWithHours > 0 ? (decimal)g.TotalHours / g.CountWithHours : null
        }).OrderByDescending(x => x.TotalAssigned).ToList();
    }

    public async Task<ApplicationTrendReportDto> GetTrendsAsync(
        ApplicationTrendQuery parameters,
        CancellationToken cancellationToken = default)
    {
        var actor = await _scopeService.GetCurrentActorAsync(cancellationToken);
        var normalized = NormalizeQuery(parameters);
        await _scopeService.EnsureCampusFilterAllowedAsync(actor, normalized.CampusId, cancellationToken);

        var query = BuildFilteredQuery(normalized, actor);

        string metric = parameters.Metric?.ToLower() ?? "submitted";
        string groupBy = parameters.GroupBy?.ToLower() ?? "month";

        var validMetrics = new[] { "submitted", "approved", "rejected", "canceled", "overdue", "completed" };
        if (!validMetrics.Contains(metric))
        {
            throw new ApiException(400, "Invalid metric.");
        }

        // Apply metric filters
        if (metric == "submitted") query = query.Where(a => a.NgayNop.HasValue);
        else if (metric == "approved") query = query.Where(a => a.TrangThai == ApplicationStatuses.Approved);
        else if (metric == "rejected") query = query.Where(a => a.TrangThai == ApplicationStatuses.Rejected);
        else if (metric == "canceled") query = query.Where(a => a.TrangThai == ApplicationStatuses.Cancelled);
        else if (metric == "overdue") query = query.Where(a => RunningSlaStatuses.Contains(a.TrangThai) && a.HanXuLyLuc.HasValue && a.HanXuLyLuc < DateTime.UtcNow);
        else if (metric == "completed") query = query.Where(a => a.TrangThai == ApplicationStatuses.Approved || a.TrangThai == ApplicationStatuses.Rejected);

        // Fetch dates and group in memory to avoid complex SQL date grouping compatibility
        var dates = await query.Select(a => new { a.NgayNop, a.NgayDuyet }).ToListAsync(cancellationToken);

        var grouped = dates.Select(d => new
        {
            Date = (metric == "submitted" || metric == "overdue") ? d.NgayNop : (d.NgayDuyet ?? d.NgayNop)
        }).Where(d => d.Date.HasValue).Select(d => d.Date!.Value);

        List<ApplicationTrendPointDto> points = new();

        if (groupBy == "day")
        {
            points = grouped.GroupBy(d => d.Date)
                .Select(g => new ApplicationTrendPointDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    From = g.Key,
                    To = g.Key.AddDays(1).AddTicks(-1),
                    Value = g.Count()
                }).OrderBy(x => x.From).ToList();
        }
        else if (groupBy == "month")
        {
            points = grouped.GroupBy(d => new DateTime(d.Year, d.Month, 1))
                .Select(g => new ApplicationTrendPointDto
                {
                    Label = g.Key.ToString("yyyy-MM"),
                    From = g.Key,
                    To = g.Key.AddMonths(1).AddTicks(-1),
                    Value = g.Count()
                }).OrderBy(x => x.From).ToList();
        }
        else if (groupBy == "semester")
        {
            // Group by quarters for simplicity as generic semester
            points = grouped.GroupBy(d => new DateTime(d.Year, ((d.Month - 1) / 3) * 3 + 1, 1))
                .Select(g => new ApplicationTrendPointDto
                {
                    Label = $"{g.Key.Year}-Q{((g.Key.Month - 1) / 3) + 1}",
                    From = g.Key,
                    To = g.Key.AddMonths(3).AddTicks(-1),
                    Value = g.Count()
                }).OrderBy(x => x.From).ToList();
        }

        return new ApplicationTrendReportDto
        {
            Metric = metric,
            GroupBy = groupBy,
            Points = points
        };
    }

    private sealed class NormalizedReportQuery
    {
        public int? CampusId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? ProcessingStatus { get; set; }
        public int? AssigneeId { get; set; }
        public int? ProcessorId { get; set; }
        public DateTime? SubmittedFrom { get; set; }
        public DateTime? SubmittedTo { get; set; }

        public ApplicationReportFiltersDto ToDto()
        {
            return new ApplicationReportFiltersDto
            {
                CampusId = CampusId,
                Type = Type,
                Status = Status,
                ProcessingStatus = ProcessingStatus,
                AssigneeId = AssigneeId,
                ProcessorId = ProcessorId,
                SubmittedFrom = SubmittedFrom,
                SubmittedTo = SubmittedTo
            };
        }
    }
}
