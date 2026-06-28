using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Auth;
using Backend.DTOs.RewardDiscipline;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Services.RewardDiscipline;

public class RewardEvaluationService : IRewardEvaluationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RewardEvaluationService(ApplicationDbContext context, IAuditLogService auditLogService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _auditLogService = auditLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RewardEvaluationResultDto> EvaluateCampaignAsync(int campaignId, EvaluateRewardCampaignRequest request)
    {
        var campaign = await _context.DotKhenThuongs
            .Include(c => c.DonVi)
            .Include(c => c.HocKy)
            .FirstOrDefaultAsync(c => c.MaDotKhenThuong == campaignId);

        if (campaign == null)
            throw new Backend.Exceptions.ApiException(StatusCodes.Status404NotFound, "Đợt khen thưởng không tồn tại.");

        if (campaign.LoaiDot != RewardDisciplineConstants.RewardCampaignTypes.Top100Semester)
            throw new Backend.Exceptions.ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ xét duyệt đợt Top 100 Học Kỳ.");

        if (campaign.TrangThai == RewardDisciplineConstants.RewardCampaignStatuses.Approved ||
            campaign.TrangThai == RewardDisciplineConstants.RewardCampaignStatuses.Published ||
            campaign.TrangThai == RewardDisciplineConstants.RewardCampaignStatuses.Cancelled)
        {
            throw new Backend.Exceptions.ApiException(StatusCodes.Status400BadRequest, "Không thể xét duyệt đợt khen thưởng đã duyệt, đã công bố hoặc đã hủy.");
        }

        bool hasExisting = await _context.UngVienKhenThuongs.AnyAsync(c => c.MaDotKhenThuong == campaignId);
        if (hasExisting && !request.ForceReevaluate)
        {
            throw new Backend.Exceptions.ApiException(StatusCodes.Status409Conflict, "Đợt khen thưởng đã có danh sách xét duyệt. Vui lòng bật forceReevaluate để xét lại.");
        }

        // Parse Criteria
        int maxCandidates = campaign.SoLuongToiDa;
        decimal minGpa = 0m;
        bool excludeActiveDiscipline = true;
        bool requireActiveStudent = true;

        var criteriaJson = string.IsNullOrWhiteSpace(request.OverrideCriteriaJson)
            ? campaign.TieuChiXetJson
            : request.OverrideCriteriaJson;

        if (!string.IsNullOrWhiteSpace(criteriaJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(criteriaJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("minGpa", out var gpaProp) && gpaProp.TryGetDecimal(out var gpaVal)) minGpa = gpaVal;
                if (root.TryGetProperty("maxCandidates", out var maxProp) && maxProp.TryGetInt32(out var maxVal)) maxCandidates = maxVal;
                if (root.TryGetProperty("excludeActiveDiscipline", out var excDiscProp)) excludeActiveDiscipline = excDiscProp.GetBoolean();
                if (root.TryGetProperty("requireActiveStudent", out var reqActiveProp)) requireActiveStudent = reqActiveProp.GetBoolean();
            }
            catch { /* Ignore parsing errors and use defaults */ }
        }

        var candidatesToSave = new List<UngVienKhenThuong>();
        var excludedCandidates = new List<UngVienKhenThuong>();

        // 1. Get Base Students
        var studentQuery = _context.NguoiDungs
            .Where(u => u.VaiTroChinh == "sinh_vien" || u.VaiTroChinh == "hoc_sinh");

        if (campaign.MaDonVi.HasValue)
        {
            studentQuery = studentQuery.Where(u => u.MaDonVi == campaign.MaDonVi.Value);
        }

        var students = await studentQuery
            .Select(u => new
            {
                u.MaNguoiDung,
                u.HoTen,
                u.Email, // Email acting as MSSV
                u.TrangThai,
                u.MaDonVi
            })
            .ToListAsync();

        // 2. Load Grades for the term
        var grades = await _context.DiemSos
            .Include(d => d.MonHoc)
            .Where(d => d.MaHocKy == campaign.MaHocKy && students.Select(s => s.MaNguoiDung).Contains(d.MaHocSinh))
            .ToListAsync();

        var studentGrades = grades.GroupBy(g => g.MaHocSinh).ToDictionary(g => g.Key, g => g.ToList());

        // 3. Load Active Disciplines
        var disciplineStudentIds = new HashSet<int>();
        if (excludeActiveDiscipline)
        {
            disciplineStudentIds = await _context.HoSoKyLuats
                .Where(k => (k.MaHocKy == campaign.MaHocKy || k.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active)
                            && k.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Removed
                            && k.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Cancelled
                            && k.TrangThai != RewardDisciplineConstants.DisciplineStatuses.Rejected)
                .Select(k => k.MaHocSinh)
                .Distinct()
                .ToHashSetAsync();
        }

        // 4. Evaluate each student
        var validCandidates = new List<UngVienKhenThuong>();

        foreach (var student in students)
        {
            var isExcluded = false;
            string? excludeReason = null;

            if (requireActiveStudent && student.TrangThai != "hoat_dong")
            {
                isExcluded = true;
                excludeReason = RewardDisciplineConstants.CandidateExclusionReasons.InactiveStudent;
            }
            else if (disciplineStudentIds.Contains(student.MaNguoiDung))
            {
                isExcluded = true;
                excludeReason = RewardDisciplineConstants.CandidateExclusionReasons.ActiveDiscipline;
            }
            else if (!studentGrades.TryGetValue(student.MaNguoiDung, out var sg) || !sg.Any())
            {
                isExcluded = true;
                excludeReason = RewardDisciplineConstants.CandidateExclusionReasons.MissingGrades;
            }
            else
            {
                // Calculate GPA
                decimal totalPoints = 0;
                int totalCredits = 0;
                foreach (var g in sg)
                {
                    int credits = g.MonHoc?.SoTinChi ?? 0;
                    totalPoints += g.GpaMonHoc * credits;
                    totalCredits += credits;
                }

                if (totalCredits == 0)
                {
                    isExcluded = true;
                    excludeReason = RewardDisciplineConstants.CandidateExclusionReasons.MissingGrades;
                }
                else
                {
                    decimal gpa = Math.Round(totalPoints / totalCredits, 2);
                    if (gpa < minGpa)
                    {
                        isExcluded = true;
                        excludeReason = RewardDisciplineConstants.CandidateExclusionReasons.LowGpa;
                    }
                    else
                    {
                        // Eligible
                        validCandidates.Add(new UngVienKhenThuong
                        {
                            MaDotKhenThuong = campaignId,
                            MaHocSinh = student.MaNguoiDung,
                            MaDonVi = student.MaDonVi,
                            MaHocKy = campaign.MaHocKy,
                            DiemXet = gpa,
                            GpaHocKy = gpa,
                            TongTinChi = totalCredits,
                            TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.Recommended,
                            HoTenSnapshot = student.HoTen,
                            MssvSnapshot = student.Email,
                            TenHocKySnapshot = campaign.HocKy?.TenHocKy,
                            TieuChiSnapshotJson = criteriaJson
                        });
                    }
                }
            }

            if (isExcluded && request.IncludeExcluded)
            {
                excludedCandidates.Add(new UngVienKhenThuong
                {
                    MaDotKhenThuong = campaignId,
                    MaHocSinh = student.MaNguoiDung,
                    MaDonVi = student.MaDonVi,
                    MaHocKy = campaign.MaHocKy,
                    TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.Excluded,
                    LyDoLoai = excludeReason,
                    HoTenSnapshot = student.HoTen,
                    MssvSnapshot = student.Email,
                    TenHocKySnapshot = campaign.HocKy?.TenHocKy
                });
            }
        }

        // Rank valid candidates
        var rankedCandidates = validCandidates
            .OrderByDescending(c => c.DiemXet)
            .ThenByDescending(c => c.TongTinChi)
            .ThenBy(c => c.MaHocSinh)
            .ToList();

        // Apply max candidates
        int rank = 1;
        foreach (var c in rankedCandidates)
        {
            if (rank <= maxCandidates)
            {
                c.XepHang = rank;
                candidatesToSave.Add(c);
            }
            else
            {
                c.XepHang = null;
                c.TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.Excluded;
                c.LyDoLoai = RewardDisciplineConstants.CandidateExclusionReasons.OutOfScope;
                if (request.IncludeExcluded)
                {
                    candidatesToSave.Add(c);
                }
            }
            rank++;
        }

        if (request.IncludeExcluded)
        {
            candidatesToSave.AddRange(excludedCandidates);
        }

        var resultDto = new RewardEvaluationResultDto
        {
            MaDotKhenThuong = campaignId,
            CandidateCount = candidatesToSave.Count(c => c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended),
            ExcludedCount = candidatesToSave.Count(c => c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Excluded),
            IsDryRun = request.DryRun,
            StatusAfterEvaluation = campaign.TrangThai
        };

        if (!request.DryRun)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (hasExisting)
                    {
                        var existing = await _context.UngVienKhenThuongs.Where(u => u.MaDotKhenThuong == campaignId).ToListAsync();
                        _context.UngVienKhenThuongs.RemoveRange(existing);
                        await _context.SaveChangesAsync();
                    }

                    _context.UngVienKhenThuongs.AddRange(candidatesToSave);
                    
                    campaign.TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Evaluating;
                    
                    await _context.SaveChangesAsync();
                    
                    string action = hasExisting ? "REEVALUATE_REWARD_CAMPAIGN" : "EVALUATE_REWARD_CAMPAIGN";
                    await _auditLogService.LogAsync(
                        "DotKhenThuong", 
                        campaignId.ToString(), 
                        action, 
                        null, 
                        new { Candidates = resultDto.CandidateCount }, 
                        null, 
                        campaign.MaDonVi, 
                        $"Evaluated top 100. Candidates: {resultDto.CandidateCount}");

                    await transaction.CommitAsync();
                    resultDto.StatusAfterEvaluation = campaign.TrangThai;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        return resultDto;
    }

    public async Task<(IEnumerable<RewardCandidateDto> Candidates, int TotalCount)> GetCandidatesAsync(int campaignId, RewardCandidateQueryParameters query, int currentUserId)
    {
        var currentUser = GetCurrentUser();
        EnsureCanRead(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        var status = string.IsNullOrWhiteSpace(query.Status)
            ? RewardDisciplineConstants.RewardCandidateStatuses.Recommended
            : NormalizeCandidateStatus(query.Status);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var candidatesQuery = _context.UngVienKhenThuongs
            .AsNoTracking()
            .Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong && c.TrangThai == status);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToLowerInvariant();
            candidatesQuery = candidatesQuery.Where(c =>
                (c.HoTenSnapshot != null && c.HoTenSnapshot.ToLower().Contains(keyword)) ||
                (c.MssvSnapshot != null && c.MssvSnapshot.ToLower().Contains(keyword)));
        }

        var totalCount = await candidatesQuery.CountAsync();
        var rows = await candidatesQuery
            .OrderBy(c => c.XepHang == null)
            .ThenBy(c => c.XepHang)
            .ThenByDescending(c => c.DiemXet)
            .ThenBy(c => c.MaHocSinh)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var data = rows.Select(ToCandidateDto).ToList();

        return (data, totalCount);
    }

    public async Task<(IEnumerable<ExcludedRewardCandidateDto> ExcludedCandidates, int TotalCount)> GetExcludedCandidatesAsync(int campaignId, ExcludedRewardCandidateQueryParameters query, int currentUserId)
    {
        var currentUser = GetCurrentUser();
        EnsureCanRead(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        var pageIndex = Math.Max(1, query.PageIndex);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var candidatesQuery = _context.UngVienKhenThuongs
            .AsNoTracking()
            .Where(c => c.MaDotKhenThuong == campaign.MaDotKhenThuong && c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Excluded);

        if (!string.IsNullOrWhiteSpace(query.ReasonCode))
        {
            var reasonCode = query.ReasonCode.Trim();
            candidatesQuery = candidatesQuery.Where(c => c.LyDoLoai == reasonCode);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim().ToLowerInvariant();
            candidatesQuery = candidatesQuery.Where(c =>
                (c.HoTenSnapshot != null && c.HoTenSnapshot.ToLower().Contains(keyword)) ||
                (c.MssvSnapshot != null && c.MssvSnapshot.ToLower().Contains(keyword)));
        }

        var totalCount = await candidatesQuery.CountAsync();
        var data = await candidatesQuery
            .OrderBy(c => c.MaHocSinh)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ExcludedRewardCandidateDto
            {
                MaHocSinh = c.MaHocSinh,
                HoTenSnapshot = c.HoTenSnapshot,
                MssvSnapshot = c.MssvSnapshot,
                DiemXet = c.DiemXet,
                LyDoLoai = c.LyDoLoai,
                LyDoLoaiJson = c.LyDoLoaiJson
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<RewardApprovalSummaryDto> GetApprovalSummaryAsync(int campaignId)
    {
        var currentUser = GetCurrentUser();
        EnsureCanRead(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        return await BuildApprovalSummaryAsync(campaign);
    }

    public async Task<RewardCandidateDto> AdjustCandidateAsync(int campaignId, int candidateId, AdjustCandidateRequest request)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        EnsureMutableCampaign(campaign);

        var candidate = await _context.UngVienKhenThuongs
            .FirstOrDefaultAsync(c => c.MaDotKhenThuong == campaignId && c.MaUngVienKhenThuong == candidateId);
        if (candidate is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy ứng viên khen thưởng.");
        }

        var oldSnapshot = CreateCandidateAuditSnapshot(candidate);
        var newStatus = string.IsNullOrWhiteSpace(request.TrangThai)
            ? candidate.TrangThai
            : NormalizeAdjustableCandidateStatus(request.TrangThai);
        var newRank = request.XepHang ?? candidate.XepHang;
        var newScore = request.DiemXet ?? candidate.DiemXet;
        if (newRank.HasValue && newRank.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Xếp hạng phải lớn hơn 0.");
        }

        if (newScore < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Điểm xét phải lớn hơn hoặc bằng 0.");
        }

        var hasChange =
            !candidate.TrangThai.Equals(newStatus, StringComparison.OrdinalIgnoreCase) ||
            candidate.XepHang != newRank ||
            candidate.DiemXet != newScore;
        if (hasChange && string.IsNullOrWhiteSpace(request.LyDoDieuChinh))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Lý do điều chỉnh là bắt buộc khi thay đổi trạng thái, xếp hạng hoặc điểm xét.");
        }

        candidate.TrangThai = newStatus;
        candidate.XepHang = newRank;
        candidate.DiemXet = newScore;
        candidate.GhiChuDieuChinh = NormalizeOptionalText(request.GhiChuDieuChinh ?? request.LyDoDieuChinh);
        candidate.NguoiDieuChinh = currentUser.UserId;
        candidate.NgayDieuChinh = DateTime.UtcNow;
        candidate.NgayCapNhat = DateTime.UtcNow;
        candidate.LyDoLoai = newStatus == RewardDisciplineConstants.RewardCandidateStatuses.Excluded
            ? NormalizeOptionalText(request.LyDoDieuChinh) ?? RewardDisciplineConstants.CandidateExclusionReasons.Other
            : candidate.LyDoLoai;

        await _context.SaveChangesAsync();
        await LogRewardAuditAsync(
            campaign,
            RewardDisciplineConstants.RewardAuditActions.AdjustRewardCandidate,
            oldSnapshot,
            CreateCandidateAuditSnapshot(candidate),
            currentUser,
            "Điều chỉnh ứng viên khen thưởng Top 100.");

        return ToCandidateDto(candidate);
    }

    public async Task<RewardCandidateDto> ManualAddCandidateAsync(int campaignId, ManualAddCandidateRequest request)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        EnsureMutableCampaign(campaign);

        var reason = NormalizeRequiredText(request.LyDoThemBatBuoc, "Lý do thêm thủ công");
        if (request.XepHang.HasValue && request.XepHang.Value <= 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Xếp hạng phải lớn hơn 0.");
        }

        if (request.DiemXet.HasValue && request.DiemXet.Value < 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Điểm xét phải lớn hơn hoặc bằng 0.");
        }

        var student = await _context.NguoiDungs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaNguoiDung == request.MaHocSinh);
        if (student is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học sinh/sinh viên không tồn tại.");
        }

        if (!IsStudent(student) || !student.TrangThai.Equals(UserStatuses.DbActive, StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Tài khoản được thêm phải là học sinh/sinh viên đang hoạt động.");
        }

        if (campaign.MaDonVi.HasValue && student.MaDonVi != campaign.MaDonVi.Value)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học sinh/sinh viên không thuộc cơ sở của đợt khen thưởng.");
        }

        var duplicateCandidate = await _context.UngVienKhenThuongs
            .AnyAsync(x => x.MaDotKhenThuong == campaignId && x.MaHocSinh == request.MaHocSinh);
        if (duplicateCandidate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Học sinh/sinh viên đã có trong danh sách ứng viên.");
        }

        var duplicateReward = await _context.KhenThuongs
            .AnyAsync(x => x.MaDotKhenThuong == campaignId && x.MaHocSinh == request.MaHocSinh && !x.DaHuy);
        if (duplicateReward)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Học sinh/sinh viên đã có quyết định khen thưởng cho đợt này.");
        }

        var hasActiveDiscipline = await _context.HoSoKyLuats.AsNoTracking()
            .AnyAsync(x =>
                x.MaHocSinh == request.MaHocSinh &&
                x.TrangThai == RewardDisciplineConstants.DisciplineStatuses.Active &&
                !x.DaGoKyLuat);
        if (hasActiveDiscipline)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Học sinh/sinh viên đang có kỷ luật còn hiệu lực.");
        }

        var candidate = new UngVienKhenThuong
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            MaHocSinh = student.MaNguoiDung,
            MaDonVi = student.MaDonVi,
            MaHocKy = campaign.MaHocKy,
            XepHang = request.XepHang,
            DiemXet = request.DiemXet ?? 0m,
            GpaHocKy = null,
            TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded,
            HoTenSnapshot = student.HoTen,
            MssvSnapshot = student.Email,
            TenHocKySnapshot = campaign.HocKy?.TenHocKy,
            TieuChiSnapshotJson = campaign.TieuChiXetJson,
            GhiChuDieuChinh = reason,
            NguoiDieuChinh = currentUser.UserId,
            NgayDieuChinh = DateTime.UtcNow,
            NguoiTao = currentUser.UserId,
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };

        _context.UngVienKhenThuongs.Add(candidate);
        await _context.SaveChangesAsync();
        await LogRewardAuditAsync(
            campaign,
            RewardDisciplineConstants.RewardAuditActions.ManualAddRewardCandidate,
            null,
            CreateCandidateAuditSnapshot(candidate),
            currentUser,
            "Thêm thủ công ứng viên Top 100.");

        return ToCandidateDto(candidate);
    }

    public async Task<RewardApprovalSummaryDto> ReorderCandidatesAsync(int campaignId, ReorderCandidatesRequest request)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        EnsureMutableCampaign(campaign);

        if (request.Items.Count == 0)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Danh sách sắp xếp không được để trống.");
        }

        if (request.Items.Any(x => x.XepHang <= 0))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Xếp hạng phải lớn hơn 0.");
        }

        if (request.Items.Select(x => x.XepHang).Distinct().Count() != request.Items.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Xếp hạng không được trùng.");
        }

        if (request.Items.Select(x => x.CandidateId).Distinct().Count() != request.Items.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Ứng viên trong danh sách sắp xếp không được trùng.");
        }

        var candidateIds = request.Items.Select(x => x.CandidateId).ToHashSet();
        var candidates = await _context.UngVienKhenThuongs
            .Where(x => x.MaDotKhenThuong == campaignId && candidateIds.Contains(x.MaUngVienKhenThuong))
            .ToListAsync();
        if (candidates.Count != request.Items.Count)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Có ứng viên không thuộc đợt khen thưởng.");
        }

        if (candidates.Any(x => !RewardDisciplineConstants.RewardCandidateStatuses.Selected.Contains(x.TrangThai)))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ được sắp xếp ứng viên đang được đề xuất hoặc thêm thủ công.");
        }

        var oldSnapshot = candidates
            .Select(x => new { x.MaUngVienKhenThuong, x.XepHang })
            .ToList();
        var rankByCandidateId = request.Items.ToDictionary(x => x.CandidateId, x => x.XepHang);
        var now = DateTime.UtcNow;
        foreach (var candidate in candidates)
        {
            candidate.XepHang = rankByCandidateId[candidate.MaUngVienKhenThuong];
            candidate.NguoiDieuChinh = currentUser.UserId;
            candidate.NgayDieuChinh = now;
            candidate.NgayCapNhat = now;
        }

        await _context.SaveChangesAsync();
        await LogRewardAuditAsync(
            campaign,
            RewardDisciplineConstants.RewardAuditActions.ReorderRewardCandidates,
            oldSnapshot,
            new { Count = candidates.Count, Ranks = request.Items.Take(10) },
            currentUser,
            "Sắp xếp lại thứ hạng ứng viên Top 100.");

        return await BuildApprovalSummaryAsync(campaign);
    }

    public async Task<RewardApprovalSummaryDto> SubmitForApprovalAsync(int campaignId)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        if (campaign.TrangThai != RewardDisciplineConstants.RewardCampaignStatuses.Evaluating)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được trình duyệt đợt khen thưởng đang xét.");
        }

        await ValidateApprovalSelectionAsync(campaign);
        var oldSnapshot = CreateCampaignAuditSnapshot(campaign);
        campaign.TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval;
        await _context.SaveChangesAsync();
        await LogRewardAuditAsync(
            campaign,
            RewardDisciplineConstants.RewardAuditActions.SubmitRewardCampaignForApproval,
            oldSnapshot,
            CreateCampaignAuditSnapshot(campaign),
            currentUser,
            "Trình duyệt danh sách khen thưởng Top 100.");

        return await BuildApprovalSummaryAsync(campaign);
    }

    public async Task<ApproveRewardCampaignResultDto> ApproveCampaignAsync(int campaignId)
    {
        var currentUser = GetCurrentUser();
        EnsureSuperAdmin(currentUser);
        var campaign = await LoadCampaignInScopeAsync(campaignId, currentUser, CancellationToken.None);
        if (campaign.TrangThai is not (RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval or RewardDisciplineConstants.RewardCampaignStatuses.Evaluating))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được duyệt đợt khen thưởng đang xét hoặc chờ duyệt.");
        }

        await ValidateApprovalSelectionAsync(campaign);
        var hasRewards = await _context.KhenThuongs
            .AnyAsync(x => x.MaDotKhenThuong == campaignId && !x.DaHuy);
        if (hasRewards)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Đợt khen thưởng đã tạo quyết định khen thưởng.");
        }

        var selectedCandidates = await _context.UngVienKhenThuongs
            .Where(x =>
                x.MaDotKhenThuong == campaignId &&
                (x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended ||
                 x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded))
            .OrderBy(x => x.XepHang == null)
            .ThenBy(x => x.XepHang)
            .ThenByDescending(x => x.DiemXet)
            .ThenBy(x => x.MaHocSinh)
            .ToListAsync();

        var duplicateStudentIds = selectedCandidates
            .GroupBy(x => x.MaHocSinh)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
        if (duplicateStudentIds.Count > 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Danh sách duyệt có học sinh/sinh viên bị trùng.");
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        var rewardsCreated = 0;
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var now = DateTime.UtcNow;
            foreach (var candidate in selectedCandidates)
            {
                if (!candidate.MaDonVi.HasValue)
                {
                    throw new ApiException(StatusCodes.Status409Conflict, "Ứng viên được duyệt thiếu thông tin cơ sở.");
                }

                _context.KhenThuongs.Add(new KhenThuong
                {
                    MaDonVi = candidate.MaDonVi.Value,
                    MaHocSinh = candidate.MaHocSinh,
                    MaHocKy = candidate.MaHocKy,
                    MaDotKhenThuong = campaign.MaDotKhenThuong,
                    MaMauBangKhen = campaign.MaMauBangKhen,
                    LoaiKhenThuong = RewardDisciplineConstants.RewardTypes.Top100Semester,
                    TrangThai = RewardDisciplineConstants.RewardStatuses.Approved,
                    GpaDatDuoc = candidate.GpaHocKy,
                    DiemXet = candidate.DiemXet,
                    XepHang = candidate.XepHang,
                    UrlChungTu = string.Empty,
                    HoTenSnapshot = Truncate(candidate.HoTenSnapshot, 200),
                    MssvSnapshot = Truncate(candidate.MssvSnapshot, 50),
                    TenHocKySnapshot = Truncate(candidate.TenHocKySnapshot, 200),
                    DanhHieuSnapshot = "Top 100 học kỳ",
                    CapLuc = now,
                    NgayCapNhat = now,
                    NguoiCap = currentUser.UserId,
                    NguoiDuyet = currentUser.UserId,
                    DaHuy = false
                });
                candidate.TrangThai = RewardDisciplineConstants.RewardCandidateStatuses.ApprovedForReward;
                candidate.NgayCapNhat = now;
            }

            var oldSnapshot = CreateCampaignAuditSnapshot(campaign);
            campaign.TrangThai = RewardDisciplineConstants.RewardCampaignStatuses.Approved;
            campaign.NguoiDuyet = currentUser.UserId;
            campaign.NgayDuyet = now;
            await _context.SaveChangesAsync();
            rewardsCreated = selectedCandidates.Count;

            await LogRewardAuditAsync(
                campaign,
                RewardDisciplineConstants.RewardAuditActions.ApproveRewardCampaign,
                oldSnapshot,
                new
                {
                    campaign.MaDotKhenThuong,
                    campaign.TrangThai,
                    RewardsCreatedCount = rewardsCreated
                },
                currentUser,
                "Duyệt danh sách và tạo quyết định khen thưởng Top 100.");
            await transaction.CommitAsync();
        });

        return new ApproveRewardCampaignResultDto
        {
            CampaignId = campaign.MaDotKhenThuong,
            RewardsCreatedCount = rewardsCreated,
            Status = campaign.TrangThai
        };
    }

    private async Task<RewardApprovalSummaryDto> BuildApprovalSummaryAsync(DotKhenThuong campaign)
    {
        var counts = await _context.UngVienKhenThuongs
            .AsNoTracking()
            .Where(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong)
            .GroupBy(x => x.TrangThai)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync();
        var rewardsCreated = await _context.KhenThuongs
            .AsNoTracking()
            .CountAsync(x => x.MaDotKhenThuong == campaign.MaDotKhenThuong && !x.DaHuy);
        var countByStatus = counts.ToDictionary(x => x.Status, x => x.Count, StringComparer.OrdinalIgnoreCase);
        var selectedCount =
            countByStatus.GetValueOrDefault(RewardDisciplineConstants.RewardCandidateStatuses.Recommended) +
            countByStatus.GetValueOrDefault(RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded);
        var warnings = new List<string>();
        if (selectedCount == 0)
        {
            warnings.Add("Chưa có ứng viên được chọn để duyệt.");
        }

        if (selectedCount > campaign.SoLuongToiDa)
        {
            warnings.Add("Số ứng viên được chọn vượt quá số lượng tối đa.");
        }

        if (rewardsCreated > 0)
        {
            warnings.Add("Đợt khen thưởng đã có quyết định khen thưởng.");
        }

        return new RewardApprovalSummaryDto
        {
            MaDotKhenThuong = campaign.MaDotKhenThuong,
            TenDot = campaign.TenDot,
            MaHocKy = campaign.MaHocKy,
            TenHocKy = campaign.HocKy?.TenHocKy,
            MaDonVi = campaign.MaDonVi,
            TenDonVi = campaign.DonVi?.TenDonVi,
            TrangThai = campaign.TrangThai,
            SoLuongToiDa = campaign.SoLuongToiDa,
            TotalCandidates = counts.Sum(x => x.Count),
            SelectedCount = selectedCount,
            ExcludedCount = countByStatus.GetValueOrDefault(RewardDisciplineConstants.RewardCandidateStatuses.Excluded),
            ReserveCount = countByStatus.GetValueOrDefault(RewardDisciplineConstants.RewardCandidateStatuses.Reserve),
            ApprovedCandidateCount = countByStatus.GetValueOrDefault(RewardDisciplineConstants.RewardCandidateStatuses.ApprovedForReward),
            RewardsCreatedCount = rewardsCreated,
            CanAdjust = campaign.TrangThai is RewardDisciplineConstants.RewardCampaignStatuses.Evaluating or RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval,
            CanApprove = campaign.TrangThai is RewardDisciplineConstants.RewardCampaignStatuses.Evaluating or RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval,
            Warnings = warnings
        };
    }

    private async Task ValidateApprovalSelectionAsync(DotKhenThuong campaign)
    {
        var selectedCount = await _context.UngVienKhenThuongs
            .CountAsync(x =>
                x.MaDotKhenThuong == campaign.MaDotKhenThuong &&
                (x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended ||
                 x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded));
        if (selectedCount == 0)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chưa có ứng viên được chọn để duyệt.");
        }

        if (selectedCount > campaign.SoLuongToiDa)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Số ứng viên được chọn vượt quá số lượng tối đa.");
        }

        var hasDuplicate = await _context.UngVienKhenThuongs
            .Where(x =>
                x.MaDotKhenThuong == campaign.MaDotKhenThuong &&
                (x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Recommended ||
                 x.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.ManuallyAdded))
            .GroupBy(x => x.MaHocSinh)
            .AnyAsync(x => x.Count() > 1);
        if (hasDuplicate)
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Danh sách duyệt có học sinh/sinh viên bị trùng.");
        }
    }

    private async Task<DotKhenThuong> LoadCampaignInScopeAsync(int campaignId, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        var campaign = await _context.DotKhenThuongs
            .Include(x => x.HocKy)
            .Include(x => x.DonVi)
            .FirstOrDefaultAsync(x => x.MaDotKhenThuong == campaignId, cancellationToken);
        if (campaign is null)
        {
            throw new ApiException(StatusCodes.Status404NotFound, "Không tìm thấy đợt khen thưởng.");
        }

        if (campaign.LoaiDot != RewardDisciplineConstants.RewardCampaignTypes.Top100Semester)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Chỉ hỗ trợ đợt khen thưởng Top 100 học kỳ.");
        }

        await EnsureCampaignReadableAsync(campaign, currentUser, cancellationToken);
        return campaign;
    }

    private async Task EnsureCampaignReadableAsync(DotKhenThuong campaign, CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == AuthRoles.SuperAdmin)
        {
            return;
        }

        if (!campaign.MaDonVi.HasValue)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng toàn hệ thống.");
        }

        if (currentUser.Role == AuthRoles.Admin)
        {
            return;
        }

        var allowedOrganizationIds = await GetAllowedOrganizationIdsAsync(currentUser, cancellationToken);
        if (!allowedOrganizationIds.Contains(campaign.MaDonVi.Value))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem đợt khen thưởng của đơn vị này.");
        }
    }

    private async Task<HashSet<int>> GetAllowedOrganizationIdsAsync(CurrentUserContext currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role is AuthRoles.SuperAdmin or AuthRoles.Admin)
        {
            return await _context.DonVis.AsNoTracking().Select(x => x.MaDonVi).ToHashSetAsync(cancellationToken);
        }

        var organizations = await _context.DonVis.AsNoTracking()
            .Select(x => new { x.MaDonVi, x.MaDonViCha })
            .ToListAsync(cancellationToken);
        var allowedIds = new HashSet<int> { currentUser.CampusId };
        var queue = new Queue<int>();
        queue.Enqueue(currentUser.CampusId);

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();
            foreach (var child in organizations.Where(x => x.MaDonViCha == parentId))
            {
                if (allowedIds.Add(child.MaDonVi))
                {
                    queue.Enqueue(child.MaDonVi);
                }
            }
        }

        return allowedIds;
    }

    private static void EnsureMutableCampaign(DotKhenThuong campaign)
    {
        if (campaign.TrangThai is not (RewardDisciplineConstants.RewardCampaignStatuses.Evaluating or RewardDisciplineConstants.RewardCampaignStatuses.PendingApproval))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "Chỉ được điều chỉnh danh sách khi đợt khen thưởng đang xét hoặc chờ duyệt.");
        }
    }

    private static string NormalizeCandidateStatus(string value)
    {
        var normalized = value.Trim();
        var canonical = RewardDisciplineConstants.RewardCandidateStatuses.All
            .FirstOrDefault(x => x.Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (canonical is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Trạng thái ứng viên không hợp lệ.");
        }

        return canonical;
    }

    private static string NormalizeAdjustableCandidateStatus(string value)
    {
        var status = NormalizeCandidateStatus(value);
        if (status == RewardDisciplineConstants.RewardCandidateStatuses.ApprovedForReward)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Không được điều chỉnh trực tiếp sang trạng thái đã duyệt khen thưởng.");
        }

        return status;
    }

    private CurrentUserContext GetCurrentUser()
    {
        var currentUser = _httpContextAccessor.HttpContext?.Items["CurrentUser"] as CurrentUserContext;
        if (currentUser is null)
        {
            throw new ApiException(StatusCodes.Status401Unauthorized, "Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        return currentUser;
    }

    private static void EnsureCanRead(CurrentUserContext currentUser)
    {
        if (currentUser.Role is not (AuthRoles.SuperAdmin or AuthRoles.Admin or AuthRoles.CampusAdmin))
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Bạn không có quyền xem danh sách khen thưởng.");
        }
    }

    private static void EnsureSuperAdmin(CurrentUserContext currentUser)
    {
        if (currentUser.Role != AuthRoles.SuperAdmin)
        {
            throw new ApiException(StatusCodes.Status403Forbidden, "Chỉ SuperAdmin được thực hiện thao tác này.");
        }
    }

    private static bool IsStudent(NguoiDung user)
    {
        return user.VaiTroChinh.Equals(AuthRoles.ToDatabaseCode(AuthRoles.Student), StringComparison.OrdinalIgnoreCase) ||
               user.VaiTroChinh.Equals("sinh_vien", StringComparison.OrdinalIgnoreCase) ||
               user.VaiTroChinh.Equals("hoc_sinh", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRequiredText(string? value, string fieldName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, $"{fieldName} không được để trống.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private async Task LogRewardAuditAsync(
        DotKhenThuong campaign,
        string action,
        object? oldValue,
        object? newValue,
        CurrentUserContext currentUser,
        string description)
    {
        await _auditLogService.LogAsync(
            "DotKhenThuong",
            campaign.MaDotKhenThuong.ToString(),
            action,
            oldValue,
            newValue,
            currentUser.UserId,
            campaign.MaDonVi,
            description);
    }

    private static object CreateCampaignAuditSnapshot(DotKhenThuong campaign)
    {
        return new
        {
            campaign.MaDotKhenThuong,
            campaign.MaHocKy,
            campaign.MaDonVi,
            campaign.TenDot,
            campaign.LoaiDot,
            campaign.SoLuongToiDa,
            campaign.TrangThai,
            campaign.NguoiDuyet,
            campaign.NgayDuyet
        };
    }

    private static object CreateCandidateAuditSnapshot(UngVienKhenThuong candidate)
    {
        return new
        {
            candidate.MaUngVienKhenThuong,
            candidate.MaDotKhenThuong,
            candidate.MaHocSinh,
            candidate.XepHang,
            candidate.DiemXet,
            candidate.TrangThai,
            candidate.LyDoLoai,
            candidate.GhiChuDieuChinh,
            candidate.NguoiDieuChinh,
            candidate.NgayDieuChinh
        };
    }

    private static RewardCandidateDto ToCandidateDto(UngVienKhenThuong candidate)
    {
        return new RewardCandidateDto
        {
            MaUngVienKhenThuong = candidate.MaUngVienKhenThuong,
            MaDotKhenThuong = candidate.MaDotKhenThuong,
            MaHocSinh = candidate.MaHocSinh,
            HoTenSnapshot = candidate.HoTenSnapshot,
            MssvSnapshot = candidate.MssvSnapshot,
            XepHang = candidate.XepHang,
            DiemXet = candidate.DiemXet,
            GpaHocKy = candidate.GpaHocKy,
            TrangThai = candidate.TrangThai,
            LyDoLoai = candidate.LyDoLoai,
            GhiChuDieuChinh = candidate.GhiChuDieuChinh,
            NguoiDieuChinh = candidate.NguoiDieuChinh,
            NgayDieuChinh = candidate.NgayDieuChinh,
            NgayTao = candidate.NgayTao
        };
    }
}
