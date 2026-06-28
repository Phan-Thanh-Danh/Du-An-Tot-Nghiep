using Backend.Constants;
using Backend.Data;
using Backend.DTOs.RewardDiscipline;
using Backend.Models;
using Backend.Services.Audit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Backend.Services.RewardDiscipline;

public class RewardEvaluationService : IRewardEvaluationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public RewardEvaluationService(ApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
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
        var campaign = await _context.DotKhenThuongs.FindAsync(campaignId);
        if (campaign == null) throw new ArgumentException("Campaign not found");

        var q = _context.UngVienKhenThuongs
            .Where(c => c.MaDotKhenThuong == campaignId && c.TrangThai == (query.Status ?? RewardDisciplineConstants.RewardCandidateStatuses.Recommended));

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            q = q.Where(c => c.HoTenSnapshot != null && c.HoTenSnapshot.Contains(query.Keyword) || c.MssvSnapshot != null && c.MssvSnapshot.Contains(query.Keyword));
        }

        int totalCount = await q.CountAsync();
        var data = await q
            .OrderBy(c => c.XepHang)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new RewardCandidateDto
            {
                MaUngVienKhenThuong = c.MaUngVienKhenThuong,
                MaDotKhenThuong = c.MaDotKhenThuong,
                MaHocSinh = c.MaHocSinh,
                HoTenSnapshot = c.HoTenSnapshot,
                MssvSnapshot = c.MssvSnapshot,
                XepHang = c.XepHang,
                DiemXet = c.DiemXet,
                GpaHocKy = c.GpaHocKy,
                TrangThai = c.TrangThai,
                NgayTao = c.NgayTao
            })
            .ToListAsync();

        return (data, totalCount);
    }

    public async Task<(IEnumerable<ExcludedRewardCandidateDto> ExcludedCandidates, int TotalCount)> GetExcludedCandidatesAsync(int campaignId, ExcludedRewardCandidateQueryParameters query, int currentUserId)
    {
        var q = _context.UngVienKhenThuongs
            .Where(c => c.MaDotKhenThuong == campaignId && c.TrangThai == RewardDisciplineConstants.RewardCandidateStatuses.Excluded);

        if (!string.IsNullOrWhiteSpace(query.ReasonCode))
        {
            q = q.Where(c => c.LyDoLoai == query.ReasonCode);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            q = q.Where(c => c.HoTenSnapshot != null && c.HoTenSnapshot.Contains(query.Keyword) || c.MssvSnapshot != null && c.MssvSnapshot.Contains(query.Keyword));
        }

        int totalCount = await q.CountAsync();
        var data = await q
            .OrderBy(c => c.MaHocSinh)
            .Skip((query.PageIndex - 1) * query.PageSize)
            .Take(query.PageSize)
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
}
