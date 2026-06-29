using Backend.Constants;
using Backend.Data;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Applications;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.RewardDiscipline;

public class RewardDisciplineReportService : IRewardDisciplineReportService
{
    private const int MaxPageSize = 100;
    private const int MaxTopStudentLimit = 50;
    private const int AppealSlaHours = 72;

    private static readonly string[] TrendMetrics =
    [
        "rewards",
        "issued_rewards",
        "certificates_generated",
        "discipline_records",
        "active_discipline",
        "discipline_appeals"
    ];

    private static readonly string[] TrendGroups = ["day", "month", "semester"];
    private static readonly string[] ReportGroups = ["day", "month", "semester", "campus"];
    private static readonly string[] TopStudentModes = ["reward", "discipline", "balanced"];

    private readonly ApplicationDbContext _context;
    private readonly IApplicationCampusScopeService _scopeService;

    public RewardDisciplineReportService(
        ApplicationDbContext context,
        IApplicationCampusScopeService scopeService)
    {
        _context = context;
        _scopeService = scopeService;
    }

    public async Task<RewardDisciplineOverviewReportDto> GetOverviewAsync(
        RewardDisciplineReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var filter = await NormalizeCommonQueryAsync(actor, query, allowGroupBy: true, cancellationToken);

        var campaigns = BuildCampaignQuery(actor, filter);
        var rewards = BuildRewardQuery(actor, filter);
        var records = BuildDisciplineQuery(actor, filter);
        var appeals = BuildAppealQuery(actor, filter);

        var rewardStatusRows = await rewards
            .TagWith("RD-DL Report Overview RewardStatus")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var disciplineStatusRows = await records
            .TagWith("RD-DL Report Overview DisciplineStatus")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var appealStatusRows = await appeals
            .TagWith("RD-DL Report Overview AppealStatus")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var campaignCount = await campaigns.CountAsync(cancellationToken);
        var rewardCount = await rewards.CountAsync(cancellationToken);
        var issuedCount = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Issued, cancellationToken);
        var certGenerated = await rewards.CountAsync(x => x.UrlPdfBangKhen != null && x.UrlPdfBangKhen != string.Empty, cancellationToken);
        var certFailed = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfFailed, cancellationToken);
        var activeDiscipline = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active, cancellationToken);
        var expiredDiscipline = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Expired, cancellationToken);
        var removedDiscipline = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Removed, cancellationToken);
        var appealCount = await appeals.CountAsync(cancellationToken);
        var pendingAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Pending, cancellationToken);
        var acceptedAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Accepted, cancellationToken);
        var rejectedAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Rejected, cancellationToken);

        var latestRewardEvents = await rewards
            .TagWith("RD-DL Report Overview LatestRewards")
            .OrderByDescending(x => x.NgayCapNhat ?? x.NgaySinhPdf ?? x.CapLuc)
            .ThenByDescending(x => x.MaKhenThuong)
            .Take(5)
            .Select(x => new LatestRewardDisciplineEventDto
            {
                Type = "reward",
                Id = x.MaKhenThuong,
                Title = x.DanhHieuSnapshot ?? x.LoaiKhenThuong,
                Status = x.TrangThai,
                MaDonVi = x.MaDonVi,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null,
                OccurredAt = x.NgayCapNhat ?? x.NgaySinhPdf ?? x.CapLuc
            })
            .ToListAsync(cancellationToken);

        var latestDisciplineEvents = await records
            .TagWith("RD-DL Report Overview LatestDiscipline")
            .OrderByDescending(x => x.NgayCapNhat ?? x.NgayDuyet ?? x.NgayTao)
            .ThenByDescending(x => x.MaKyLuat)
            .Take(5)
            .Select(x => new LatestRewardDisciplineEventDto
            {
                Type = "discipline",
                Id = x.MaKyLuat,
                Title = x.TieuDe,
                Status = x.TrangThai,
                MaDonVi = x.MaDonVi,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null,
                OccurredAt = x.NgayCapNhat ?? x.NgayDuyet ?? x.NgayTao
            })
            .ToListAsync(cancellationToken);

        return new RewardDisciplineOverviewReportDto
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Filters = filter.ToDto(),
            TotalRewardCampaigns = campaignCount,
            TotalRewards = rewardCount,
            TotalIssuedRewards = issuedCount,
            TotalCertificateGenerated = certGenerated,
            TotalCertificateFailed = certFailed,
            TotalActiveDisciplineRecords = activeDiscipline,
            TotalExpiredDisciplineRecords = expiredDiscipline,
            TotalRemovedDisciplineRecords = removedDiscipline,
            TotalDisciplineAppeals = appealCount,
            PendingDisciplineAppeals = pendingAppeals,
            ApprovedDisciplineAppeals = acceptedAppeals,
            RejectedDisciplineAppeals = rejectedAppeals,
            RewardByStatus = BuildStatusBuckets(RewardDisciplineConstants.RewardStatuses.All, rewardStatusRows),
            DisciplineByStatus = BuildStatusBuckets(RewardDisciplineConstants.DisciplineStatuses.All, disciplineStatusRows),
            AppealByStatus = BuildStatusBuckets(RewardDisciplineConstants.DisciplineAppealStatuses.All, appealStatusRows),
            LatestRewardEvents = latestRewardEvents,
            LatestDisciplineEvents = latestDisciplineEvents
        };
    }

    public async Task<RewardReportDto> GetRewardsAsync(
        RewardReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var filter = await NormalizeCommonQueryAsync(actor, query, allowGroupBy: false, cancellationToken);
        var loaiDot = NormalizeOptional(query.LoaiDot, RewardDisciplineConstants.RewardCampaignTypes.All, "Loại đợt khen thưởng không hợp lệ.");
        var loaiKhenThuong = NormalizeOptional(query.LoaiKhenThuong, RewardDisciplineConstants.RewardTypes.All, "Loại khen thưởng không hợp lệ.");
        var trangThai = NormalizeOptional(query.TrangThai, RewardDisciplineConstants.RewardStatuses.All, "Trạng thái khen thưởng không hợp lệ.");

        var campaigns = BuildCampaignQuery(actor, filter);
        if (loaiDot is not null)
        {
            campaigns = campaigns.Where(x => x.LoaiDot == loaiDot);
        }

        var rewards = BuildRewardQuery(actor, filter);
        if (loaiKhenThuong is not null)
        {
            rewards = rewards.Where(x => x.LoaiKhenThuong == loaiKhenThuong);
        }
        if (trangThai is not null)
        {
            rewards = rewards.Where(x => x.TrangThai == trangThai);
        }

        var campaignStatusRows = await campaigns
            .TagWith("RD-DL Report Rewards CampaignStatus")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var rewardStatusRows = await rewards
            .TagWith("RD-DL Report Rewards RewardStatus")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        var rewardCount = await rewards.CountAsync(cancellationToken);
        var campaignCount = await campaigns.CountAsync(cancellationToken);

        return new RewardReportDto
        {
            Filters = filter.ToDto(),
            TotalCampaigns = campaignCount,
            TotalCandidates = await BuildCandidateQuery(actor, filter).CountAsync(cancellationToken),
            TotalApprovedRewards = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Approved, cancellationToken),
            TotalIssuedRewards = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Issued, cancellationToken),
            TotalCanceledRewards = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Cancelled || x.DaHuy, cancellationToken),
            TotalRestoredRewards = await rewards.CountAsync(x => !x.DaHuy && x.LyDoHuy == null && x.NgayHuy == null && x.GhiChuVongDoi != null && x.GhiChuVongDoi.Contains("khôi phục"), cancellationToken),
            AverageRewardsPerCampaign = campaignCount == 0 ? 0 : Math.Round(rewardCount / (decimal)campaignCount, 2, MidpointRounding.AwayFromZero),
            CampaignsByStatus = BuildStatusBuckets(RewardDisciplineConstants.RewardCampaignStatuses.All, campaignStatusRows),
            RewardsByType = await GroupRewardsByTypeAsync(rewards, cancellationToken),
            RewardsBySemester = await GroupRewardsBySemesterAsync(rewards, cancellationToken),
            RewardsByCampus = await GroupRewardsByCampusAsync(rewards, cancellationToken),
            RewardsByStatus = BuildStatusBuckets(RewardDisciplineConstants.RewardStatuses.All, rewardStatusRows),
            TopRewardedStudents = await BuildTopStudentsAsync(actor, filter, "reward", 10, cancellationToken)
        };
    }

    public async Task<DisciplineReportDto> GetDisciplineAsync(
        DisciplineReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var filter = await NormalizeCommonQueryAsync(actor, query, allowGroupBy: false, cancellationToken);
        var level = NormalizeOptional(query.MucDoKyLuat, RewardDisciplineConstants.DisciplineLevels.All, "Mức độ kỷ luật không hợp lệ.");
        var action = NormalizeOptional(query.HinhThucXuLy, RewardDisciplineConstants.DisciplineActions.All, "Hình thức xử lý không hợp lệ.");
        var status = NormalizeOptional(query.TrangThai, RewardDisciplineConstants.DisciplineStatuses.All, "Trạng thái kỷ luật không hợp lệ.");

        var records = BuildDisciplineQuery(actor, filter);
        if (level is not null) records = records.Where(x => x.MucDoViPham == level);
        if (action is not null) records = records.Where(x => x.HinhThucXuLy == action);
        if (status is not null) records = records.Where(x => x.TrangThai == status);

        var statusRows = await records
            .TagWith("RD-DL Report Discipline Status")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return new DisciplineReportDto
        {
            Filters = filter.ToDto(),
            TotalDisciplineRecords = await records.CountAsync(cancellationToken),
            ActiveRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active, cancellationToken),
            ApprovedRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Approved, cancellationToken),
            RejectedRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Rejected, cancellationToken),
            ExpiredRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Expired, cancellationToken),
            RemovedEffectRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Removed || x.DaGoKyLuat, cancellationToken),
            VoidedRecords = await records.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Cancelled, cancellationToken),
            AverageActiveDurationDays = await GetAverageDisciplineDurationDaysAsync(records, cancellationToken),
            RecordsBySeverity = await GroupDisciplineBySeverityAsync(records, cancellationToken),
            RecordsByHandlingMethod = await GroupDisciplineByHandlingMethodAsync(records, cancellationToken),
            RecordsByStatus = BuildStatusBuckets(RewardDisciplineConstants.DisciplineStatuses.All, statusRows),
            RecordsBySemester = await GroupDisciplineBySemesterAsync(records, cancellationToken),
            RecordsByCampus = await GroupDisciplineByCampusAsync(records, cancellationToken),
            RepeatDisciplineStudents = await BuildTopStudentsAsync(actor, filter, "discipline", 10, cancellationToken)
        };
    }

    public async Task<CertificateReportDto> GetCertificatesAsync(
        CertificateReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var filter = await NormalizeCommonQueryAsync(actor, query, allowGroupBy: false, cancellationToken);
        var status = NormalizeOptional(query.TrangThai, RewardDisciplineConstants.RewardStatuses.All, "Trạng thái PDF bằng khen không hợp lệ.");

        var rewards = BuildRewardQuery(actor, filter)
            .Where(x => x.LoaiKhenThuong == RewardDisciplineConstants.RewardTypes.Top100Semester);
        if (query.MaDotKhenThuong.HasValue)
        {
            ValidatePositive(query.MaDotKhenThuong, "Đợt khen thưởng không hợp lệ.");
            rewards = rewards.Where(x => x.MaDotKhenThuong == query.MaDotKhenThuong.Value);
        }
        if (query.MaMauBangKhen.HasValue)
        {
            ValidatePositive(query.MaMauBangKhen, "Mẫu bằng khen không hợp lệ.");
            rewards = rewards.Where(x => x.MaMauBangKhen == query.MaMauBangKhen.Value);
        }
        if (status is not null)
        {
            rewards = rewards.Where(x => x.TrangThai == status);
        }

        var total = await rewards.CountAsync(cancellationToken);
        var generated = await rewards.CountAsync(x => x.UrlPdfBangKhen != null && x.UrlPdfBangKhen != string.Empty, cancellationToken);
        var failed = await rewards.CountAsync(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfFailed, cancellationToken);
        var statusRows = await rewards
            .TagWith("RD-DL Report Certificates Status")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return new CertificateReportDto
        {
            Filters = filter.ToDto(),
            TotalRewardsEligibleForCertificate = total,
            TotalCertificatesGenerated = generated,
            TotalCertificatesFailed = failed,
            TotalDownloadedByStudents = null,
            GenerationFailureRate = CalculateRate(failed, total),
            CertificatesByTemplate = await GroupCertificatesByTemplateAsync(rewards, cancellationToken),
            CertificatesByCampaign = await GroupCertificatesByCampaignAsync(rewards, cancellationToken),
            CertificatesByStatus = BuildStatusBuckets(RewardDisciplineConstants.RewardStatuses.All, statusRows),
            RecentFailedCertificates = await GetRecentFailedCertificatesAsync(rewards, cancellationToken)
        };
    }

    public async Task<DisciplineAppealReportDto> GetAppealsAsync(
        DisciplineAppealReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var filter = await NormalizeCommonQueryAsync(actor, query, allowGroupBy: false, cancellationToken);
        var status = NormalizeOptional(query.TrangThai, RewardDisciplineConstants.DisciplineAppealStatuses.All, "Trạng thái khiếu nại không hợp lệ.");

        var appeals = BuildAppealQuery(actor, filter);
        if (status is not null)
        {
            appeals = appeals.Where(x => x.TrangThai == status);
        }

        var statusRows = await appeals
            .TagWith("RD-DL Report Appeals Status")
            .GroupBy(x => x.TrangThai)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);

        return new DisciplineAppealReportDto
        {
            Filters = filter.ToDto(),
            TotalAppeals = await appeals.CountAsync(cancellationToken),
            PendingAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Pending, cancellationToken),
            AcceptedAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Accepted, cancellationToken),
            RejectedAppeals = await appeals.CountAsync(x => x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Rejected, cancellationToken),
            AverageResolutionTimeHours = await GetAverageAppealResolutionHoursAsync(appeals, cancellationToken),
            OverdueAppeals = await appeals.CountAsync(x =>
                x.TrangThai == RewardDisciplineConstants.DisciplineAppealStatuses.Pending &&
                x.NgayTao < DateTime.UtcNow.AddHours(-AppealSlaHours),
                cancellationToken),
            SlaHours = AppealSlaHours,
            AppealsByStatus = BuildStatusBuckets(RewardDisciplineConstants.DisciplineAppealStatuses.All, statusRows),
            AppealsByDisciplineSeverity = await GroupAppealsBySeverityAsync(appeals, cancellationToken),
            AppealsBySemester = await GroupAppealsBySemesterAsync(appeals, cancellationToken),
            AppealsByCampus = await GroupAppealsByCampusAsync(appeals, cancellationToken)
        };
    }

    public async Task<RewardDisciplineTrendReportDto> GetTrendsAsync(
        RewardDisciplineTrendQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var common = new RewardDisciplineReportQuery
        {
            MaDonVi = query.MaDonVi,
            MaHocKy = query.MaHocKy,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            GroupBy = query.GroupBy
        };
        var filter = await NormalizeCommonQueryAsync(actor, common, allowGroupBy: true, cancellationToken);
        var metric = NormalizeRequired(query.Metric, TrendMetrics, "Metric báo cáo không hợp lệ.");
        var groupBy = NormalizeRequired(query.GroupBy, TrendGroups, "groupBy báo cáo xu hướng không hợp lệ.");

        if (groupBy == "day" && filter.FromDate.HasValue && filter.ToDate.HasValue &&
            (filter.ToDate.Value.Date - filter.FromDate.Value.Date).TotalDays > 370)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Khoảng ngày không được vượt quá 370 ngày khi groupBy=day.");
        }

        var points = metric switch
        {
            "rewards" => await BuildRewardTrendAsync(BuildRewardQuery(actor, filter), groupBy, x => true, cancellationToken),
            "issued_rewards" => await BuildRewardTrendAsync(BuildRewardQuery(actor, filter), groupBy, x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.Issued, cancellationToken),
            "certificates_generated" => await BuildRewardTrendAsync(BuildRewardQuery(actor, filter), groupBy, x => x.UrlPdfBangKhen != null && x.UrlPdfBangKhen != string.Empty, cancellationToken),
            "discipline_records" => await BuildDisciplineTrendAsync(BuildDisciplineQuery(actor, filter), groupBy, x => true, cancellationToken),
            "active_discipline" => await BuildDisciplineTrendAsync(BuildDisciplineQuery(actor, filter), groupBy, x => x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active, cancellationToken),
            "discipline_appeals" => await BuildAppealTrendAsync(BuildAppealQuery(actor, filter), groupBy, cancellationToken),
            _ => []
        };

        filter.GroupBy = groupBy;
        return new RewardDisciplineTrendReportDto
        {
            Metric = metric,
            GroupBy = groupBy,
            Filters = filter.ToDto(),
            Points = points
        };
    }

    public async Task<IReadOnlyList<TopStudentReportItemDto>> GetTopStudentsAsync(
        TopStudentReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await GetActorAsync(cancellationToken);
        var common = new RewardDisciplineReportQuery
        {
            MaDonVi = query.MaDonVi,
            MaHocKy = query.MaHocKy
        };
        var filter = await NormalizeCommonQueryAsync(actor, common, allowGroupBy: false, cancellationToken);
        var mode = NormalizeRequired(query.Mode, TopStudentModes, "Mode báo cáo sinh viên không hợp lệ.");
        var limit = Math.Clamp(query.Limit <= 0 ? 10 : query.Limit, 1, MaxTopStudentLimit);

        return await BuildTopStudentsAsync(actor, filter, mode, limit, cancellationToken);
    }

    private async Task<ApplicationActorContext> GetActorAsync(CancellationToken cancellationToken)
    {
        return await _scopeService.GetCurrentActorAsync(cancellationToken);
    }

    private async Task<NormalizedReportFilter> NormalizeCommonQueryAsync(
        ApplicationActorContext actor,
        RewardDisciplineReportQuery query,
        bool allowGroupBy,
        CancellationToken cancellationToken)
    {
        ValidatePositive(query.MaDonVi, "Cơ sở không hợp lệ.");
        ValidatePositive(query.MaHocKy, "Học kỳ không hợp lệ.");

        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value.Date > query.ToDate.Value.Date)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "fromDate phải nhỏ hơn hoặc bằng toDate.");
        }

        await _scopeService.EnsureCampusFilterAllowedAsync(actor, query.MaDonVi, cancellationToken);
        var groupBy = string.IsNullOrWhiteSpace(query.GroupBy)
            ? null
            : NormalizeRequired(query.GroupBy, allowGroupBy ? ReportGroups : TrendGroups, "groupBy không hợp lệ.");

        return new NormalizedReportFilter
        {
            MaDonVi = query.MaDonVi,
            MaHocKy = query.MaHocKy,
            FromDate = query.FromDate?.Date,
            ToDate = query.ToDate?.Date,
            GroupBy = groupBy,
            PageIndex = Math.Max(1, query.PageIndex),
            PageSize = Math.Clamp(query.PageSize <= 0 ? 20 : query.PageSize, 1, MaxPageSize)
        };
    }

    private IQueryable<DotKhenThuong> BuildCampaignQuery(ApplicationActorContext actor, NormalizedReportFilter filter)
    {
        var query = _context.DotKhenThuongs.AsNoTracking();
        if (!actor.IsGlobal && actor.AllowedCampusIds is not null)
        {
            query = query.Where(x => x.MaDonVi.HasValue && actor.AllowedCampusIds.Contains(x.MaDonVi.Value));
        }
        if (filter.MaDonVi.HasValue) query = query.Where(x => x.MaDonVi == filter.MaDonVi.Value);
        if (filter.MaHocKy.HasValue) query = query.Where(x => x.MaHocKy == filter.MaHocKy.Value);
        if (filter.FromDate.HasValue) query = query.Where(x => x.NgayTao >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(x => x.NgayTao < filter.ToDate.Value.AddDays(1));
        return query;
    }

    private IQueryable<UngVienKhenThuong> BuildCandidateQuery(ApplicationActorContext actor, NormalizedReportFilter filter)
    {
        var query = _context.UngVienKhenThuongs.AsNoTracking();
        if (!actor.IsGlobal && actor.AllowedCampusIds is not null)
        {
            query = query.Where(x => x.MaDonVi.HasValue && actor.AllowedCampusIds.Contains(x.MaDonVi.Value));
        }
        if (filter.MaDonVi.HasValue) query = query.Where(x => x.MaDonVi == filter.MaDonVi.Value);
        if (filter.MaHocKy.HasValue) query = query.Where(x => x.MaHocKy == filter.MaHocKy.Value);
        if (filter.FromDate.HasValue) query = query.Where(x => x.NgayTao >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(x => x.NgayTao < filter.ToDate.Value.AddDays(1));
        return query;
    }

    private IQueryable<KhenThuong> BuildRewardQuery(ApplicationActorContext actor, NormalizedReportFilter filter)
    {
        var query = _context.KhenThuongs.AsNoTracking();
        if (!actor.IsGlobal && actor.AllowedCampusIds is not null)
        {
            query = query.Where(x => actor.AllowedCampusIds.Contains(x.MaDonVi));
        }
        if (filter.MaDonVi.HasValue) query = query.Where(x => x.MaDonVi == filter.MaDonVi.Value);
        if (filter.MaHocKy.HasValue) query = query.Where(x => x.MaHocKy == filter.MaHocKy.Value);
        if (filter.FromDate.HasValue) query = query.Where(x => x.CapLuc >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(x => x.CapLuc < filter.ToDate.Value.AddDays(1));
        return query;
    }

    private IQueryable<HoSoKyLuat> BuildDisciplineQuery(ApplicationActorContext actor, NormalizedReportFilter filter)
    {
        var query = _context.HoSoKyLuats.AsNoTracking();
        if (!actor.IsGlobal && actor.AllowedCampusIds is not null)
        {
            query = query.Where(x => actor.AllowedCampusIds.Contains(x.MaDonVi));
        }
        if (filter.MaDonVi.HasValue) query = query.Where(x => x.MaDonVi == filter.MaDonVi.Value);
        if (filter.MaHocKy.HasValue) query = query.Where(x => x.MaHocKy == filter.MaHocKy.Value);
        if (filter.FromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(filter.FromDate.Value);
            query = query.Where(x => x.NgayViPham >= from);
        }
        if (filter.ToDate.HasValue)
        {
            var to = DateOnly.FromDateTime(filter.ToDate.Value);
            query = query.Where(x => x.NgayViPham <= to);
        }
        return query;
    }

    private IQueryable<KhieuNaiKyLuat> BuildAppealQuery(ApplicationActorContext actor, NormalizedReportFilter filter)
    {
        var query = _context.KhieuNaiKyLuats.AsNoTracking();
        if (!actor.IsGlobal && actor.AllowedCampusIds is not null)
        {
            query = query.Where(x => x.MaDonVi.HasValue && actor.AllowedCampusIds.Contains(x.MaDonVi.Value));
        }
        if (filter.MaDonVi.HasValue) query = query.Where(x => x.MaDonVi == filter.MaDonVi.Value);
        if (filter.MaHocKy.HasValue) query = query.Where(x => x.HoSoKyLuat != null && x.HoSoKyLuat.MaHocKy == filter.MaHocKy.Value);
        if (filter.FromDate.HasValue) query = query.Where(x => x.NgayTao >= filter.FromDate.Value);
        if (filter.ToDate.HasValue) query = query.Where(x => x.NgayTao < filter.ToDate.Value.AddDays(1));
        return query;
    }

    private static IReadOnlyList<StatusCountDto> BuildStatusBuckets(IReadOnlySet<string> statuses, IReadOnlyList<CountRow> rows)
    {
        var counts = rows.ToDictionary(x => x.Key, x => x.Count, StringComparer.OrdinalIgnoreCase);
        return statuses
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new StatusCountDto
            {
                Status = x,
                Label = x,
                Count = counts.GetValueOrDefault(x)
            })
            .ToList();
    }

    private static IReadOnlyList<GroupCountDto> ToGroupCounts(IEnumerable<CountRow> rows)
    {
        return rows
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Label ?? x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => new GroupCountDto
            {
                Key = x.Key,
                Label = string.IsNullOrWhiteSpace(x.Label) ? x.Key : x.Label!,
                Count = x.Count
            })
            .ToList();
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        return denominator <= 0
            ? 0
            : Math.Round(numerator * 100m / denominator, 2, MidpointRounding.AwayFromZero);
    }

    private static string? NormalizeOptional(string? value, IReadOnlySet<string> allowed, string invalidMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        var canonical = allowed.FirstOrDefault(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, invalidMessage);
        }

        return canonical;
    }

    private static string NormalizeRequired(string? value, IEnumerable<string> allowed, string invalidMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, invalidMessage);
        }

        var trimmed = value.Trim();
        var canonical = allowed.FirstOrDefault(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, invalidMessage);
        }

        return canonical;
    }

    private static void ValidatePositive(int? value, string message)
    {
        if (value is <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, message);
        }
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupRewardsByTypeAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => x.LoaiKhenThuong)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupRewardsBySemesterAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaHocKy, Name = x.HocKy != null ? x.HocKy.TenHocKy : "" })
            .Select(x => new CountRow { Key = x.Key.MaHocKy.ToString(), Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupRewardsByCampusAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaDonVi, Name = x.DonVi != null ? x.DonVi.TenDonVi : "" })
            .Select(x => new CountRow { Key = x.Key.MaDonVi.ToString(), Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupDisciplineBySeverityAsync(IQueryable<HoSoKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => x.MucDoViPham)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupDisciplineByHandlingMethodAsync(IQueryable<HoSoKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => x.HinhThucXuLy)
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupDisciplineBySemesterAsync(IQueryable<HoSoKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaHocKy, Name = x.HocKy != null ? x.HocKy.TenHocKy : "" })
            .Select(x => new CountRow { Key = x.Key.MaHocKy.HasValue ? x.Key.MaHocKy.Value.ToString() : "none", Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupDisciplineByCampusAsync(IQueryable<HoSoKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaDonVi, Name = x.DonVi != null ? x.DonVi.TenDonVi : "" })
            .Select(x => new CountRow { Key = x.Key.MaDonVi.ToString(), Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<decimal?> GetAverageDisciplineDurationDaysAsync(IQueryable<HoSoKyLuat> query, CancellationToken cancellationToken)
    {
        var average = await query
            .Where(x => x.NgayApDung.HasValue && (x.NgayGoKyLuat.HasValue || x.NgayCapNhat.HasValue))
            .Select(x => (double?)EF.Functions.DateDiffDay(x.NgayApDung!.Value, (x.NgayGoKyLuat ?? x.NgayCapNhat)!.Value))
            .AverageAsync(cancellationToken);
        return average.HasValue
            ? Math.Round((decimal)average.Value, 2, MidpointRounding.AwayFromZero)
            : null;
    }

    private async Task<decimal?> GetAverageAppealResolutionHoursAsync(IQueryable<KhieuNaiKyLuat> query, CancellationToken cancellationToken)
    {
        var average = await query
            .Where(x => x.NgayXuLy.HasValue && x.NgayXuLy.Value >= x.NgayTao)
            .Select(x => (double?)EF.Functions.DateDiffMinute(x.NgayTao, x.NgayXuLy!.Value))
            .AverageAsync(cancellationToken);
        return average.HasValue
            ? Math.Round((decimal)(average.Value / 60d), 2, MidpointRounding.AwayFromZero)
            : null;
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupCertificatesByTemplateAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaMauBangKhen, Name = x.MauBangKhen != null ? x.MauBangKhen.TenMau : "" })
            .Select(x => new CountRow { Key = x.Key.MaMauBangKhen.HasValue ? x.Key.MaMauBangKhen.Value.ToString() : "none", Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupCertificatesByCampaignAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaDotKhenThuong, Name = x.DotKhenThuong != null ? x.DotKhenThuong.TenDot : "" })
            .Select(x => new CountRow { Key = x.Key.MaDotKhenThuong.HasValue ? x.Key.MaDotKhenThuong.Value.ToString() : "none", Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<RecentFailedCertificateDto>> GetRecentFailedCertificatesAsync(IQueryable<KhenThuong> query, CancellationToken cancellationToken)
    {
        return await query
            .Where(x => x.TrangThai == RewardDisciplineConstants.RewardStatuses.PdfFailed)
            .OrderByDescending(x => x.NgaySinhPdf ?? x.NgayCapNhat ?? x.CapLuc)
            .ThenByDescending(x => x.MaKhenThuong)
            .Take(10)
            .Select(x => new RecentFailedCertificateDto
            {
                MaKhenThuong = x.MaKhenThuong,
                MaDotKhenThuong = x.MaDotKhenThuong,
                TenDot = x.DotKhenThuong != null ? x.DotKhenThuong.TenDot : null,
                MaHocSinh = x.MaHocSinh,
                HoTen = x.HoTenSnapshot,
                Mssv = x.MssvSnapshot,
                SafeError = x.LoiSinhPdf == null ? "" : x.LoiSinhPdf.Substring(0, x.LoiSinhPdf.Length > 300 ? 300 : x.LoiSinhPdf.Length),
                NgaySinhPdf = x.NgaySinhPdf
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupAppealsBySeverityAsync(IQueryable<KhieuNaiKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => x.HoSoKyLuat != null ? x.HoSoKyLuat.MucDoViPham : "unknown")
            .Select(x => new CountRow { Key = x.Key, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupAppealsBySemesterAsync(IQueryable<KhieuNaiKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.HoSoKyLuat!.MaHocKy, Name = x.HoSoKyLuat!.HocKy != null ? x.HoSoKyLuat.HocKy.TenHocKy : "" })
            .Select(x => new CountRow { Key = x.Key.MaHocKy.HasValue ? x.Key.MaHocKy.Value.ToString() : "none", Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<GroupCountDto>> GroupAppealsByCampusAsync(IQueryable<KhieuNaiKyLuat> query, CancellationToken cancellationToken)
    {
        var rows = await query.GroupBy(x => new { x.MaDonVi, Name = x.DonVi != null ? x.DonVi.TenDonVi : "" })
            .Select(x => new CountRow { Key = x.Key.MaDonVi.HasValue ? x.Key.MaDonVi.Value.ToString() : "none", Label = x.Key.Name, Count = x.Count() })
            .ToListAsync(cancellationToken);
        return ToGroupCounts(rows);
    }

    private async Task<IReadOnlyList<TopStudentReportItemDto>> BuildTopStudentsAsync(
        ApplicationActorContext actor,
        NormalizedReportFilter filter,
        string mode,
        int limit,
        CancellationToken cancellationToken)
    {
        var rewards = BuildRewardQuery(actor, filter);
        var disciplines = BuildDisciplineQuery(actor, filter);

        var rewardRows = await rewards
            .GroupBy(x => x.MaHocSinh)
            .Select(x => new StudentRewardAgg
            {
                StudentId = x.Key,
                RewardCount = x.Count(),
                LatestRewardTitle = x.OrderByDescending(r => r.CapLuc).Select(r => r.DanhHieuSnapshot ?? r.LoaiKhenThuong).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var disciplineRows = await disciplines
            .GroupBy(x => x.MaHocSinh)
            .Select(x => new StudentDisciplineAgg
            {
                StudentId = x.Key,
                DisciplineCount = x.Count(),
                ActiveDisciplineCount = x.Count(r => r.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active),
                LatestDisciplineStatus = x.OrderByDescending(r => r.NgayTao).Select(r => r.TrangThai).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var studentIds = rewardRows.Select(x => x.StudentId)
            .Concat(disciplineRows.Select(x => x.StudentId))
            .Distinct()
            .ToList();

        var users = await _context.NguoiDungs.AsNoTracking()
            .Where(x => studentIds.Contains(x.MaNguoiDung))
            .Select(x => new
            {
                x.MaNguoiDung,
                x.HoTen,
                x.Email,
                x.MaDonVi,
                TenDonVi = x.DonVi != null ? x.DonVi.TenDonVi : null
            })
            .ToListAsync(cancellationToken);

        var rewardByStudent = rewardRows.ToDictionary(x => x.StudentId);
        var disciplineByStudent = disciplineRows.ToDictionary(x => x.StudentId);

        var items = users.Select(user =>
        {
            rewardByStudent.TryGetValue(user.MaNguoiDung, out var reward);
            disciplineByStudent.TryGetValue(user.MaNguoiDung, out var discipline);
            var rewardCount = reward?.RewardCount ?? 0;
            var disciplineCount = discipline?.DisciplineCount ?? 0;
            var activeDisciplineCount = discipline?.ActiveDisciplineCount ?? 0;
            return new TopStudentReportItemDto
            {
                StudentId = user.MaNguoiDung,
                StudentCode = user.Email,
                FullName = user.HoTen,
                MaDonVi = user.MaDonVi,
                TenDonVi = user.TenDonVi,
                RewardCount = rewardCount,
                DisciplineCount = disciplineCount,
                ActiveDisciplineCount = activeDisciplineCount,
                Score = rewardCount * 10m - disciplineCount * 5m - activeDisciplineCount * 5m,
                LatestRewardTitle = reward?.LatestRewardTitle,
                LatestDisciplineStatus = discipline?.LatestDisciplineStatus
            };
        });

        items = mode switch
        {
            "reward" => items.OrderByDescending(x => x.RewardCount).ThenBy(x => x.FullName),
            "discipline" => items.OrderByDescending(x => x.DisciplineCount).ThenByDescending(x => x.ActiveDisciplineCount).ThenBy(x => x.FullName),
            _ => items.OrderByDescending(x => x.Score).ThenBy(x => x.FullName)
        };

        return items.Take(limit).ToList();
    }

    private async Task<IReadOnlyList<TrendPointDto>> BuildRewardTrendAsync(
        IQueryable<KhenThuong> query,
        string groupBy,
        System.Linq.Expressions.Expression<Func<KhenThuong, bool>> predicate,
        CancellationToken cancellationToken)
    {
        query = query.Where(predicate);
        if (groupBy == "semester")
        {
            var semesterRows = await query.GroupBy(x => new { x.MaHocKy, Name = x.HocKy != null ? x.HocKy.TenHocKy : "" })
                .Select(x => new { x.Key.MaHocKy, x.Key.Name, Count = x.Count() })
                .OrderBy(x => x.MaHocKy)
                .ToListAsync(cancellationToken);
            return semesterRows.Select(x => new TrendPointDto { Label = string.IsNullOrWhiteSpace(x.Name) ? x.MaHocKy.ToString() : x.Name, Value = x.Count }).ToList();
        }

        var rows = await query
            .GroupBy(x => new { x.CapLuc.Year, x.CapLuc.Month, Day = groupBy == "day" ? x.CapLuc.Day : 1 })
            .Select(x => new { x.Key.Year, x.Key.Month, x.Key.Day, Count = x.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);
        return rows.Select(x => ToTrendPoint(x.Year, x.Month, x.Day, groupBy, x.Count)).ToList();
    }

    private async Task<IReadOnlyList<TrendPointDto>> BuildDisciplineTrendAsync(
        IQueryable<HoSoKyLuat> query,
        string groupBy,
        System.Linq.Expressions.Expression<Func<HoSoKyLuat, bool>> predicate,
        CancellationToken cancellationToken)
    {
        query = query.Where(predicate);
        if (groupBy == "semester")
        {
            var semesterRows = await query.GroupBy(x => new { x.MaHocKy, Name = x.HocKy != null ? x.HocKy.TenHocKy : "" })
                .Select(x => new { x.Key.MaHocKy, x.Key.Name, Count = x.Count() })
                .OrderBy(x => x.MaHocKy)
                .ToListAsync(cancellationToken);
            return semesterRows.Select(x => new TrendPointDto { Label = x.MaHocKy.HasValue ? (string.IsNullOrWhiteSpace(x.Name) ? x.MaHocKy.Value.ToString() : x.Name) : "Không có học kỳ", Value = x.Count }).ToList();
        }

        var rows = await query
            .GroupBy(x => new { x.NgayViPham.Year, x.NgayViPham.Month, Day = groupBy == "day" ? x.NgayViPham.Day : 1 })
            .Select(x => new { x.Key.Year, x.Key.Month, x.Key.Day, Count = x.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);
        return rows.Select(x => ToTrendPoint(x.Year, x.Month, x.Day, groupBy, x.Count)).ToList();
    }

    private async Task<IReadOnlyList<TrendPointDto>> BuildAppealTrendAsync(
        IQueryable<KhieuNaiKyLuat> query,
        string groupBy,
        CancellationToken cancellationToken)
    {
        if (groupBy == "semester")
        {
            var semesterRows = await query.GroupBy(x => new { x.HoSoKyLuat!.MaHocKy, Name = x.HoSoKyLuat!.HocKy != null ? x.HoSoKyLuat.HocKy.TenHocKy : "" })
                .Select(x => new { x.Key.MaHocKy, x.Key.Name, Count = x.Count() })
                .OrderBy(x => x.MaHocKy)
                .ToListAsync(cancellationToken);
            return semesterRows.Select(x => new TrendPointDto { Label = x.MaHocKy.HasValue ? (string.IsNullOrWhiteSpace(x.Name) ? x.MaHocKy.Value.ToString() : x.Name) : "Không có học kỳ", Value = x.Count }).ToList();
        }

        var rows = await query
            .GroupBy(x => new { x.NgayTao.Year, x.NgayTao.Month, Day = groupBy == "day" ? x.NgayTao.Day : 1 })
            .Select(x => new { x.Key.Year, x.Key.Month, x.Key.Day, Count = x.Count() })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
            .ToListAsync(cancellationToken);
        return rows.Select(x => ToTrendPoint(x.Year, x.Month, x.Day, groupBy, x.Count)).ToList();
    }

    private static TrendPointDto ToTrendPoint(int year, int month, int day, string groupBy, int count)
    {
        var from = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        var to = groupBy == "day"
            ? from.AddDays(1).AddTicks(-1)
            : from.AddMonths(1).AddTicks(-1);
        return new TrendPointDto
        {
            Label = groupBy == "day" ? from.ToString("yyyy-MM-dd") : from.ToString("yyyy-MM"),
            From = from,
            To = to,
            Value = count
        };
    }

    private sealed class NormalizedReportFilter
    {
        public int? MaDonVi { get; set; }
        public int? MaHocKy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? GroupBy { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public ReportFilterDto ToDto() => new()
        {
            MaDonVi = MaDonVi,
            MaHocKy = MaHocKy,
            FromDate = FromDate,
            ToDate = ToDate,
            GroupBy = GroupBy
        };
    }

    private sealed class CountRow
    {
        public string Key { get; set; } = string.Empty;
        public string? Label { get; set; }
        public int Count { get; set; }
    }

    private sealed class StudentRewardAgg
    {
        public int StudentId { get; set; }
        public int RewardCount { get; set; }
        public string? LatestRewardTitle { get; set; }
    }

    private sealed class StudentDisciplineAgg
    {
        public int StudentId { get; set; }
        public int DisciplineCount { get; set; }
        public int ActiveDisciplineCount { get; set; }
        public string? LatestDisciplineStatus { get; set; }
    }
}
