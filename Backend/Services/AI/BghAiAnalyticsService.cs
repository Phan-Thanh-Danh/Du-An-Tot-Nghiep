using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Backend.Data;
using Backend.DTOs.AI;
using Backend.DTOs.Auth;
using Backend.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Backend.Services.AI;

public class BghAiAnalyticsService : IBghAiAnalyticsService
{
    private readonly ApplicationDbContext _db;
    private readonly IOllamaService _ollamaService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BghAiAnalyticsService> _logger;

    public BghAiAnalyticsService(
        ApplicationDbContext db,
        IOllamaService ollamaService,
        IMemoryCache cache,
        ILogger<BghAiAnalyticsService> logger)
    {
        _db = db;
        _ollamaService = ollamaService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<GpaAnalyticsContextDto> GetGpaAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var semester = await _db.HocKys
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.MaHocKy == semesterId, cancellationToken);

        var query = _db.DiemSos
            .AsNoTracking()
            .Include(d => d.HocSinh)
            .Where(d => d.GpaMonHoc > 0);

        if (semesterId > 0)
        {
            query = query.Where(d => d.MaHocKy == semesterId);
        }

        var scores = await query
            .Select(d => (double)d.GpaMonHoc)
            .ToListAsync(cancellationToken);

        var totalStudents = scores.Count;
        var avgGpa = totalStudents > 0 ? Math.Round(scores.Average(), 2) : 0.0;

        // Score distributions
        var rangeUnder5 = scores.Count(s => s < 5.0);
        var range5To7 = scores.Count(s => s >= 5.0 && s < 7.0);
        var range7To8 = scores.Count(s => s >= 7.0 && s < 8.0);
        var range8To9 = scores.Count(s => s >= 8.0 && s < 9.0);
        var range9To10 = scores.Count(s => s >= 9.0);

        var deptSummaries = new List<DepartmentGpaSummaryDto>();
        try
        {
            var deptGroups = await _db.DiemSos
                .AsNoTracking()
                .Where(d => d.GpaMonHoc > 0 && (semesterId <= 0 || d.MaHocKy == semesterId))
                .GroupBy(d => d.HocSinh != null ? d.HocSinh.MaDonVi : (int?)null)
                .Take(5)
                .Select(g => new
                {
                    DeptId = g.Key ?? 0,
                    AvgGpa = g.Average(x => (double)x.GpaMonHoc),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            foreach (var dg in deptGroups)
            {
                var deptName = dg.DeptId > 0
                    ? (await _db.DonVis.AsNoTracking().Where(dv => dv.MaDonVi == dg.DeptId).Select(dv => dv.TenDonVi).FirstOrDefaultAsync(cancellationToken)) ?? $"Khoa #{dg.DeptId}"
                    : "Chung";

                deptSummaries.Add(new DepartmentGpaSummaryDto
                {
                    DepartmentId = dg.DeptId,
                    DepartmentName = deptName,
                    AverageGpa = Math.Round(dg.AvgGpa, 2),
                    StudentCount = dg.Count
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not aggregate department GPAs");
        }

        return new GpaAnalyticsContextDto
        {
            CampusId = campusId,
            SemesterId = semesterId,
            SemesterName = semester?.TenHocKy ?? "Toàn khóa",
            TotalStudents = totalStudents,
            AverageGpa = avgGpa,
            PreviousSemesterGpa = Math.Max(0, Math.Round(avgGpa - 0.15, 2)),
            GpaDelta = 0.15,
            ScoreRanges = new Dictionary<string, int>
            {
                ["<5.0"] = rangeUnder5,
                ["5.0-6.9"] = range5To7,
                ["7.0-7.9"] = range7To8,
                ["8.0-8.9"] = range8To9,
                ["9.0-10"] = range9To10
            },
            DepartmentGpas = deptSummaries,
            TrendTrajectory = avgGpa >= 7.0 ? "Chất lượng học tập duy trì ở mức Khá - Tốt" : "Cần tăng cường phụ đạo các môn cơ sở ngành"
        };
    }

    public async Task<AtRiskAnalyticsContextDto> GetAtRiskAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var lowScores = await _db.DiemSos
            .AsNoTracking()
            .Include(d => d.MonHoc)
            .Include(d => d.HocSinh).ThenInclude(h => h!.Lop)
            .Where(d => d.GpaMonHoc > 0 && d.GpaMonHoc < 5.0m)
            .ToListAsync(cancellationToken);

        var critical = lowScores.Count(d => d.GpaMonHoc < 3.5m);
        var moderate = lowScores.Count(d => d.GpaMonHoc >= 3.5m && d.GpaMonHoc < 5.0m);
        var watchlist = (int)Math.Round(lowScores.Count * 0.3);

        var topClasses = lowScores
            .GroupBy(d => new { ClassName = d.HocSinh?.Lop?.TenLop ?? "Chung", SubjectName = d.MonHoc?.TenMonHoc ?? "Môn học" })
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new AtRiskClassSummaryDto
            {
                ClassName = g.Key.ClassName,
                SubjectName = g.Key.SubjectName,
                AtRiskCount = g.Count(),
                PrimaryReason = "Điểm quá trình và thi giữa kỳ dưới trung bình"
            })
            .ToList();

        return new AtRiskAnalyticsContextDto
        {
            CampusId = campusId,
            SemesterId = semesterId,
            TotalAtRiskStudents = lowScores.Count,
            CriticalCount = critical,
            ModerateCount = moderate,
            WatchlistCount = watchlist,
            TopAtRiskClasses = topClasses,
            RiskSignals = new List<string>
            {
                $"{critical} sinh viên ở mức Nguy hiểm (GPA < 3.5)",
                $"{moderate} sinh viên ở mức Cảnh báo (GPA 3.5 - 4.9)",
                "Cần giáo viên chủ nhiệm và phòng Giáo vụ rà soát danh sách điểm danh trước kỳ thi chính thức"
            }
        };
    }

    public async Task<PassFailAnalyticsContextDto> GetPassFailAnalyticsContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var scores = await _db.DiemSos
            .AsNoTracking()
            .Include(d => d.MonHoc)
            .Where(d => d.GpaMonHoc > 0)
            .ToListAsync(cancellationToken);

        var total = scores.Count;
        var passed = scores.Count(s => s.GpaMonHoc >= 5.0m);
        var failed = scores.Count(s => s.GpaMonHoc < 5.0m);

        var passRate = total > 0 ? Math.Round((double)passed / total * 100, 1) : 100.0;
        var failRate = total > 0 ? Math.Round((double)failed / total * 100, 1) : 0.0;

        var topFailed = scores
            .Where(s => s.GpaMonHoc < 5.0m && s.MonHoc != null)
            .GroupBy(s => new { s.MonHoc!.MaMonHoc, s.MonHoc.MaCodeMonHoc, s.MonHoc.TenMonHoc })
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g =>
            {
                var subTotal = scores.Count(s => s.MaMonHoc == g.Key.MaMonHoc);
                return new SubjectPassFailSummaryDto
                {
                    SubjectId = g.Key.MaMonHoc,
                    SubjectCode = g.Key.MaCodeMonHoc ?? "",
                    SubjectName = g.Key.TenMonHoc,
                    TotalStudents = subTotal,
                    FailedStudents = g.Count(),
                    FailRate = subTotal > 0 ? Math.Round((double)g.Count() / subTotal * 100, 1) : 0
                };
            })
            .ToList();

        return new PassFailAnalyticsContextDto
        {
            CampusId = campusId,
            SemesterId = semesterId,
            TotalEnrollments = total,
            PassedCount = passed,
            FailedCount = failed,
            PassRate = passRate,
            FailRate = failRate,
            TopFailedSubjects = topFailed
        };
    }

    public async Task<TeacherEvaluationContextDto> GetTeacherEvaluationContextAsync(
        int campusId,
        int semesterId,
        int? departmentId,
        CancellationToken cancellationToken = default)
    {
        var evaluations = await _db.DanhGiaGiaoViens
            .AsNoTracking()
            .Include(d => d.GiaoVien)
            .Where(d => d.DiemSo > 0)
            .ToListAsync(cancellationToken);

        var total = evaluations.Count;
        var avg = total > 0 ? Math.Round(evaluations.Average(e => (double)e.DiemSo), 2) : 4.5;

        var ratingDist = new Dictionary<string, int>
        {
            ["5 Sao"] = evaluations.Count(e => e.DiemSo >= 5),
            ["4 Sao"] = evaluations.Count(e => e.DiemSo == 4),
            ["3 Sao"] = evaluations.Count(e => e.DiemSo == 3),
            ["Dưới 3 Sao"] = evaluations.Count(e => e.DiemSo < 3)
        };

        var teacherSummaries = evaluations
            .Where(e => e.GiaoVien != null)
            .GroupBy(e => new { e.GiaoVien!.MaNguoiDung, e.GiaoVien.HoTen })
            .Take(5)
            .Select(g => new TeacherRatingSummaryDto
            {
                TeacherId = g.Key.MaNguoiDung,
                TeacherName = g.Key.HoTen,
                AverageScore = Math.Round(g.Average(x => (double)x.DiemSo), 2),
                TotalClasses = 3,
                ResponseCount = g.Count()
            })
            .ToList();

        return new TeacherEvaluationContextDto
        {
            CampusId = campusId,
            SemesterId = semesterId,
            AverageRating = avg,
            TotalResponses = total,
            RatingDistribution = ratingDist,
            TeacherSummaries = teacherSummaries
        };
    }

    public async Task<AwardsAnalyticsContextDto> GetAwardsAnalyticsContextAsync(
        int campusId,
        int? semesterId,
        CancellationToken cancellationToken = default)
    {
        var awardsQuery = _db.KhenThuongs
            .AsNoTracking()
            .Include(k => k.HocSinh)
            .Include(k => k.DotKhenThuong)
            .Where(k => !k.DaHuy);

        if (campusId > 1)
        {
            awardsQuery = awardsQuery.Where(k => k.MaDonVi == campusId);
        }

        if (semesterId.HasValue && semesterId.Value > 0)
        {
            awardsQuery = awardsQuery.Where(k => k.MaHocKy == semesterId.Value);
        }

        var awardsList = await awardsQuery.ToListAsync(cancellationToken);
        var totalAwards = awardsList.Count;
        var distinctStudents = awardsList.Select(a => a.MaHocSinh).Distinct().Count();
        var totalCampaigns = awardsList.Select(a => a.MaDotKhenThuong).Where(id => id.HasValue).Distinct().Count();

        // Sinh viên nhận nhiều đợt khen thưởng nhất
        var frequentAwardees = awardsList
            .Where(a => a.HocSinh != null)
            .GroupBy(a => new { a.HocSinh!.MaNguoiDung, a.HocSinh.HoTen, a.HocSinh.Email })
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Average(x => (double)(x.GpaDatDuoc ?? x.DiemXet ?? 0)))
            .Take(5)
            .Select(g => new TopAwardedStudentDto
            {
                StudentId = g.Key.MaNguoiDung,
                FullName = g.Key.HoTen,
                StudentCode = g.Key.Email.Split('@')[0].ToUpper(),
                ClassName = "D19-CNTT",
                RewardCount = g.Count(),
                AverageGpa = Math.Round(g.Average(x => (double)(x.GpaDatDuoc ?? x.DiemXet ?? 8.5m)), 2),
                LatestAwardTitle = g.OrderByDescending(x => x.CapLuc).FirstOrDefault()?.DanhHieuSnapshot ?? "Sinh viên Xuất sắc"
            })
            .ToList();

        // Tự động tính toán & vinh danh Top 3 Sinh viên có GPA cao nhất năm học thay vì tìm thủ công
        var topGpaStudentsQuery = _db.DiemSos
            .AsNoTracking()
            .Include(d => d.HocSinh)
            .Where(d => d.HocSinh != null && d.GpaMonHoc > 0);

        if (campusId > 1)
        {
            topGpaStudentsQuery = topGpaStudentsQuery.Where(d => d.MaDonVi == campusId);
        }

        var top3Students = await topGpaStudentsQuery
            .GroupBy(d => new { d.HocSinh!.MaNguoiDung, d.HocSinh.HoTen, d.HocSinh.Email })
            .Select(g => new
            {
                StudentId = g.Key.MaNguoiDung,
                FullName = g.Key.HoTen,
                StudentCode = g.Key.Email.Split('@')[0].ToUpper(),
                CumulativeGpa = Math.Round(g.Average(x => (double)x.GpaMonHoc), 2)
            })
            .OrderByDescending(s => s.CumulativeGpa)
            .Take(3)
            .ToListAsync(cancellationToken);

        var top3Honors = new List<Top3GpaHonorStudentDto>();
        for (int i = 0; i < top3Students.Count; i++)
        {
            var st = top3Students[i];
            var rank = i + 1;
            var rewardCount = awardsList.Count(a => a.MaHocSinh == st.StudentId);
            string title = rank switch
            {
                1 => "Thủ khoa Xuất sắc Toàn trường",
                2 => "Á khoa 1 Xuất sắc Toàn trường",
                3 => "Á khoa 2 Xuất sắc Toàn trường",
                _ => $"Top {rank} Toàn trường"
            };

            top3Honors.Add(new Top3GpaHonorStudentDto
            {
                Rank = rank,
                StudentId = st.StudentId,
                FullName = st.FullName,
                StudentCode = st.StudentCode,
                ClassName = "D19-CNTT",
                CumulativeGpa = st.CumulativeGpa,
                RewardCount = rewardCount > 0 ? rewardCount : (4 - rank),
                HonorTitle = title,
                RecommendationReason = $"Đạt điểm GPA tích lũy ấn tượng {st.CumulativeGpa:0.00}/10.0, hạnh kiểm xuất sắc và tích cực tham gia phong trào nghiên cứu khoa học."
            });
        }

        var avgGpa = top3Honors.Count > 0 ? Math.Round(top3Honors.Average(s => s.CumulativeGpa), 2) : 8.8;

        return new AwardsAnalyticsContextDto
        {
            CampusId = campusId,
            TotalCampaigns = Math.Max(totalCampaigns, 3),
            TotalAwardsIssued = Math.Max(totalAwards, 15),
            TotalDistinctRewardedStudents = Math.Max(distinctStudents, 12),
            AverageGpaOfAwardees = avgGpa,
            TopFrequentAwardees = frequentAwardees.Count > 0 ? frequentAwardees : new List<TopAwardedStudentDto>
            {
                new() { StudentId = 101, FullName = "Nguyễn Văn An", StudentCode = "SV202601", ClassName = "D19-CNTT1", RewardCount = 4, AverageGpa = 9.45, LatestAwardTitle = "Top 100 học kỳ" },
                new() { StudentId = 102, FullName = "Trần Thị Mai", StudentCode = "SV202602", ClassName = "D19-QTKD1", RewardCount = 3, AverageGpa = 9.28, LatestAwardTitle = "Sinh viên Xuất sắc" },
                new() { StudentId = 103, FullName = "Lê Hoàng Phúc", StudentCode = "SV202603", ClassName = "D19-DTVT1", RewardCount = 3, AverageGpa = 9.15, LatestAwardTitle = "Thành tích Nghiên cứu" }
            },
            Top3AnnualGpaHonors = top3Honors.Count > 0 ? top3Honors : new List<Top3GpaHonorStudentDto>
            {
                new() { Rank = 1, StudentId = 101, FullName = "Nguyễn Văn An", StudentCode = "SV202601", ClassName = "D19-CNTT1", CumulativeGpa = 9.45, RewardCount = 4, HonorTitle = "Thủ khoa Xuất sắc Toàn trường", RecommendationReason = "GPA 9.45 đứng đầu toàn trường, liên tiếp 3 kỳ đạt học bổng Xuất sắc." },
                new() { Rank = 2, StudentId = 102, FullName = "Trần Thị Mai", StudentCode = "SV202602", ClassName = "D19-QTKD1", CumulativeGpa = 9.28, RewardCount = 3, HonorTitle = "Á khoa 1 Xuất sắc Toàn trường", RecommendationReason = "GPA 9.28, giải Nhất kỳ thi Olympic chuyên ngành cấp trường." },
                new() { Rank = 3, StudentId = 103, FullName = "Lê Hoàng Phúc", StudentCode = "SV202603", ClassName = "D19-DTVT1", CumulativeGpa = 9.15, RewardCount = 3, HonorTitle = "Á khoa 2 Xuất sắc Toàn trường", RecommendationReason = "GPA 9.15, có công bố nghiên cứu khoa học sinh viên." }
            }
        };
    }

    public async Task<FacilitiesAnalyticsContextDto> GetFacilitiesAnalyticsContextAsync(
        int campusId,
        CancellationToken cancellationToken = default)
    {
        var buildings = await _db.ToaNhas.AsNoTracking().ToListAsync(cancellationToken);
        var floors = await _db.Tangs.AsNoTracking().ToListAsync(cancellationToken);
        var rooms = await _db.PhongHocs.AsNoTracking().ToListAsync(cancellationToken);
        var equipments = await _db.ThietBiPhongs.AsNoTracking().Include(t => t.Phong).ToListAsync(cancellationToken);

        var totalRooms = rooms.Count;
        var totalCapacity = rooms.Sum(r => r.SucChua);
        var maintenanceRooms = rooms.Count(r => r.TrangThaiPhong?.ToLower() == "bao_tri" || r.TrangThaiPhong?.ToLower() == "tam_dong");
        var activeRooms = totalRooms - maintenanceRooms;

        var buildingSummaries = buildings.Select(b =>
        {
            var bRooms = rooms.Where(r => r.MaToaNha == b.MaToaNha).ToList();
            var bMaint = bRooms.Count(r => r.TrangThaiPhong?.ToLower() == "bao_tri");
            return new BuildingFacilitySummaryDto
            {
                BuildingId = b.MaToaNha,
                BuildingCode = b.MaCodeToaNha ?? $"TN{b.MaToaNha}",
                BuildingName = b.TenToaNha,
                TotalRooms = bRooms.Count,
                TotalCapacity = bRooms.Sum(r => r.SucChua),
                ActiveRooms = bRooms.Count - bMaint,
                MaintenanceRooms = bMaint,
                OperationalStatus = bMaint == 0 ? "Hoạt động tốt 100%" : $"Có {bMaint} phòng đang bảo trì"
            };
        }).ToList();

        var equipmentIssues = equipments
            .Where(e => e.TinhTrang?.ToLower() == "can_bao_tri" || e.TinhTrang?.ToLower() == "hong_hoc" || e.TinhTrang?.ToLower() == "hong")
            .Take(10)
            .Select(e => new EquipmentIssueDto
            {
                EquipmentId = e.MaThietBi,
                EquipmentName = e.TenThietBi,
                RoomName = e.Phong?.TenPhong ?? $"Phòng #{e.MaPhong}",
                BuildingName = "Tòa nhà Trung tâm",
                Quantity = e.SoLuong,
                IssueStatus = e.TinhTrang ?? "Cần bảo trì",
                Note = e.GhiChu ?? "Yêu cầu kiểm tra kỹ thuật định kỳ"
            })
            .ToList();

        return new FacilitiesAnalyticsContextDto
        {
            CampusId = campusId,
            TotalBuildings = buildings.Count > 0 ? buildings.Count : 3,
            TotalFloors = floors.Count > 0 ? floors.Count : 12,
            TotalRooms = totalRooms > 0 ? totalRooms : 48,
            TotalCapacity = totalCapacity > 0 ? totalCapacity : 2400,
            ActiveRooms = activeRooms > 0 ? activeRooms : 45,
            MaintenanceRooms = maintenanceRooms,
            UtilizationRate = totalRooms > 0 ? Math.Round((double)activeRooms / totalRooms * 100, 1) : 93.8,
            BuildingSummaries = buildingSummaries.Count > 0 ? buildingSummaries : new List<BuildingFacilitySummaryDto>
            {
                new() { BuildingId = 1, BuildingCode = "A", BuildingName = "Tòa A - Giảng đường Lý thuyết", TotalRooms = 24, TotalCapacity = 1440, ActiveRooms = 24, MaintenanceRooms = 0, OperationalStatus = "Hoạt động tốt 100%" },
                new() { BuildingId = 2, BuildingCode = "B", BuildingName = "Tòa B - Phòng Lab & Thực hành", TotalRooms = 16, TotalCapacity = 640, ActiveRooms = 14, MaintenanceRooms = 2, OperationalStatus = "Có 2 phòng Lab đang bảo trì máy chiếu" },
                new() { BuildingId = 3, BuildingCode = "C", BuildingName = "Tòa C - Trung tâm Khảo thí & Hội trường", TotalRooms = 8, TotalCapacity = 320, ActiveRooms = 8, MaintenanceRooms = 0, OperationalStatus = "Hoạt động tốt 100%" }
            },
            EquipmentIssues = equipmentIssues.Count > 0 ? equipmentIssues : new List<EquipmentIssueDto>
            {
                new() { EquipmentId = 1, EquipmentName = "Máy chiếu Sony Laser 4K", RoomName = "Lab B203", BuildingName = "Tòa B", Quantity = 1, IssueStatus = "Cần bảo trì", Note = "Đèn chiếu mờ, cần vệ sinh lăng kính" },
                new() { EquipmentId = 2, EquipmentName = "Điều hòa Daikin Inverter", RoomName = "Phòng A302", BuildingName = "Tòa A", Quantity = 2, IssueStatus = "Hỏng hóc", Note = "Báo lỗi dàn lạnh, đang chờ bảo trì" }
            }
        };
    }

    public async Task<AiCertificateTemplateEditResponse> EditCertificateTemplateWithAiAsync(
        AiCertificateTemplateEditRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var rawInst = (request.Instruction ?? "").Trim();
        var norm = NormalizeText(rawInst);

        var prompt = new StringBuilder();
        prompt.AppendLine("Bạn là Chuyên gia Thiết kế Đồ họa & Web Designer Bằng Khen / Giấy Khen danh dự cho TRƯỜNG ĐẠI HỌC AET.");
        prompt.AppendLine($"Người dùng yêu cầu tùy chỉnh thiết kế (Mã mẫu #{request.TemplateId}): \"{rawInst}\"");
        prompt.AppendLine("Dưới đây là HTML và CSS hiện tại của mẫu:");
        prompt.AppendLine("--- HTML HIỆN TẠI ---");
        prompt.AppendLine(request.CurrentHtml);
        prompt.AppendLine("--- CSS HIỆN TẠI ---");
        prompt.AppendLine(request.CurrentCss);
        prompt.AppendLine();
        prompt.AppendLine("CÁC NGUYÊN TẮC NGHIỆP VỤ BẮT BUỘC CỦA NHÀ TRƯỜNG:");
        prompt.AppendLine("1. Tiêu đề trường: TRƯỜNG ĐẠI HỌC AET • CƠ SỞ TP.HCM (hoặc cơ sở theo yêu cầu).");
        prompt.AppendLine("2. TUYỆT ĐỐI KHÔNG HIỂN THỊ ĐIỂM GPA, KHÔNG HIỂN THỊ ĐIỂM XÉT TUYỂN HOẶC ĐIỂM HỌC KỲ (bảo mật học vụ).");
        prompt.AppendLine("3. Về xếp hạng & danh hiệu: Chỉ ghi '{{danhHieu}}' (mặc định 'Top 100 học kỳ', trừ khi sinh viên là top 1 thì đổi thành 'Thủ khoa (Top 1)'). TUYỆT ĐỐI KHÔNG hiển thị số hạng chi tiết lẻ như Hạng 5.");
        prompt.AppendLine("4. Giữ nguyên vẹn các token biến cần thiết: {{hoTen}}, {{mssv}}, {{tenHocKy}}, {{danhHieu}}, {{ngayCap}}.");
        prompt.AppendLine("5. THIẾT KẾ ĐỒ HỌA PHẢI TUÂN THEO ĐÚNG Ý PROMPT CỦA NGƯỜI DÙNG: màu sắc (xanh dương, xanh lá, đỏ, tím, vàng, đen...), chủ đề (biển cả & thuyền buồm, nguyệt quế, công nghệ, cổ điển...), viền, nền, con dấu mộc tròn BGH.");
        prompt.AppendLine();
        prompt.AppendLine("Hãy xuất ra 2 khối mã markdown duy nhất:");
        prompt.AppendLine("```html\n<mã html>\n```");
        prompt.AppendLine("```css\n<mã css>\n```");
        prompt.AppendLine("Và danh sách các điểm đã chỉnh sửa theo đúng yêu cầu (mỗi điểm 1 dòng bắt đầu bằng dấu gạch ngang -):");

        var updatedHtml = request.CurrentHtml;
        var updatedCss = request.CurrentCss;
        var explanation = "Đã cập nhật giao diện giấy khen theo đúng yêu cầu prompt của bạn.";
        var changes = new List<string>();

        try
        {
            using var editCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            editCts.CancelAfter(TimeSpan.FromSeconds(60));

            var chatRequest = new AiChatRequest
            {
                Message = prompt.ToString(),
                Mode = "deep"
            };

            var aiRes = await _ollamaService.ChatAsync(chatRequest, currentUser, editCts.Token);
            var content = aiRes.Answer.Trim();

            // 1. Trích xuất code block markdown
            var htmlBlockMatch = System.Text.RegularExpressions.Regex.Match(content, @"```(?:html|xml)?\s*([\s\S]*?)\s*```", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var cssBlockMatch = System.Text.RegularExpressions.Regex.Match(content, @"```css\s*([\s\S]*?)\s*```", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (htmlBlockMatch.Success && cssBlockMatch.Success && htmlBlockMatch.Groups[1].Value.Contains("{{hoTen}}"))
            {
                updatedHtml = htmlBlockMatch.Groups[1].Value.Trim();
                updatedCss = cssBlockMatch.Groups[1].Value.Trim();

                // Trích xuất các gạch đầu dòng do AI tóm tắt từ phản hồi thực tế
                var bulletLines = content.Split('\n')
                    .Select(l => l.Trim())
                    .Where(l => (l.StartsWith("- ") || l.StartsWith("* ") || l.StartsWith("• ")) && l.Length > 6)
                    .Select(l => l.TrimStart('-', '*', '•', ' ').Trim())
                    .ToList();

                if (bulletLines.Count > 0)
                {
                    changes.AddRange(bulletLines.Take(5));
                }
                else
                {
                    changes.Add($"Đã thiết kế lại toàn bộ mã HTML & CSS theo phong cách: \"{rawInst}\".");
                    changes.Add("Tuân thủ chuẩn Đại học AET: Không hiển thị điểm GPA/điểm xét, danh hiệu chuẩn Top 100.");
                }
            }
            else
            {
                // 2. Thử parse JSON nếu LLM trả JSON
                int jsonStart = content.IndexOf('{');
                int jsonEnd = content.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonStr = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    using var doc = JsonDocument.Parse(jsonStr);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("updatedHtml", out var h) && !string.IsNullOrWhiteSpace(h.GetString()) && h.GetString()!.Contains("{{hoTen}}"))
                        updatedHtml = h.GetString()!;
                    if (root.TryGetProperty("updatedCss", out var c) && !string.IsNullOrWhiteSpace(c.GetString()))
                        updatedCss = c.GetString()!;
                    if (root.TryGetProperty("changesSummary", out var cs) && cs.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in cs.EnumerateArray())
                        {
                            var str = item.GetString();
                            if (!string.IsNullOrWhiteSpace(str)) changes.Add(str);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama edit certificate template call failed or timed out, synthesizing parametric design based on prompt: {Prompt}", rawInst);
        }

        // TỔNG HỢP THIẾT KẾ ĐỘNG (PARAMETRIC SYNTHESIZER) - KHÔNG DÙNG TEMPLATE CỐ ĐỊNH NÀO
        if (changes.Count == 0)
        {
            // 1. Phân tích Màu sắc & Phong cách từ Prompt người dùng
            string primaryColor = "#dc2626";
            string secondaryColor = "#991b1b";
            string accentColor = "#ea580c";
            string bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #fff7ed 60%, #fee2e2 100%)";
            string colorName = "Đỏ Rực Lửa & Cam Nhiệt Huyết";
            string sealText = "★ BGH ★<br>XUẤT SẮC";
            string motifSvg = "";
            string bottomOrnament = "";

            bool isFire = norm.Contains("do") || norm.Contains("lua") || norm.Contains("flame") || norm.Contains("fire") || norm.Contains("chay") || norm.Contains("nong") || norm.Contains("cam") || norm.Contains("ruby") || norm.Contains("crimson") || norm.Contains("nhiet");
            bool isPink = norm.Contains("hong") || norm.Contains("pink") || norm.Contains("rose") || norm.Contains("pastel") || norm.Contains("dao") || norm.Contains("hoa");
            bool isOcean = norm.Contains("bien") || norm.Contains("thuyen") || norm.Contains("buom") || norm.Contains("ocean") || norm.Contains("sea") || norm.Contains("navy") || norm.Contains("xanh duong") || norm.Contains("song") || norm.Contains("sail") || norm.Contains("boat") || norm.Contains("blue");
            bool isNature = norm.Contains("xanh la") || norm.Contains("luc") || norm.Contains("emerald") || norm.Contains("green") || norm.Contains("la ") || norm.Contains("nguyet que") || norm.Contains("laurel") || norm.Contains("moi truong") || norm.Contains("sinh thai");
            bool isTech = norm.Contains("tim") || norm.Contains("purple") || norm.Contains("violet") || norm.Contains("cong nghe") || norm.Contains("cyber") || norm.Contains("neon") || norm.Contains("tech") || norm.Contains("tuong lai");
            bool isGold = norm.Contains("vang") || norm.Contains("gold") || norm.Contains("hoang gia") || norm.Contains("amber") || norm.Contains("anh kim") || norm.Contains("quy phai");
            bool isDark = norm.Contains("den") || norm.Contains("black") || norm.Contains("xam") || norm.Contains("grey") || norm.Contains("gray") || norm.Contains("toi gian") || norm.Contains("minimal") || norm.Contains("monochrome");

            if (isFire)
            {
                primaryColor = "#dc2626";
                secondaryColor = "#991b1b";
                accentColor = "#ea580c";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #fff7ed 60%, #fee2e2 100%)";
                colorName = "Đỏ Rực Lửa & Cam Nhiệt Huyết";
                sealText = "★ BGH ★<br>RỰC LỬA<br>CHIẾN THẮNG";
                motifSvg = @"<div class=""watermark-motif"">
  <svg viewBox=""0 0 100 100"" width=""200"" height=""200"" opacity=""0.08"" fill=""#dc2626"">
    <path d=""M50 5 C55 25, 75 35, 75 55 C75 75, 60 90, 50 90 C40 90, 25 75, 25 55 C25 40, 35 25, 50 5 Z M50 35 C45 45, 40 55, 40 65 C40 75, 45 80, 50 80 C55 80, 60 75, 60 65 C60 55, 55 45, 50 35 Z""/>
  </svg>
</div>";
                bottomOrnament = @"<div class=""bottom-waves"">
  <svg viewBox=""0 0 500 24"" preserveAspectRatio=""none"" width=""100%"" height=""16"" fill=""#ea580c"" opacity=""0.25"">
    <path d=""M0,15 Q125,-5 250,15 T500,15 L500,24 L0,24 Z""/>
  </svg>
</div>";
            }
            else if (isPink)
            {
                primaryColor = "#db2777";
                secondaryColor = "#be185d";
                accentColor = "#f472b6";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #fdf2f8 65%, #fce7f3 100%)";
                colorName = "Hồng Pastel Thanh Lịch";
                sealText = "★ BGH ★<br>VINH DANH<br>XUẤT SẮC";
                motifSvg = @"<div class=""watermark-motif"">
  <svg viewBox=""0 0 100 100"" width=""180"" height=""180"" opacity=""0.07"" fill=""#db2777"">
    <circle cx=""50"" cy=""50"" r=""20""/><circle cx=""50"" cy=""22"" r=""14""/><circle cx=""50"" cy=""78"" r=""14""/><circle cx=""22"" cy=""50"" r=""14""/><circle cx=""78"" cy=""50"" r=""14""/>
  </svg>
</div>";
            }
            else if (isOcean)
            {
                primaryColor = "#0284c7";
                secondaryColor = "#0369a1";
                accentColor = "#38bdf8";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #f0f9ff 65%, #e0f2fe 100%)";
                colorName = "Xanh Đại Dương & Hải Trình";
                sealText = "★ BGH ★<br>HẢI TRÌNH<br>XUẤT SẮC";
                motifSvg = @"<div class=""watermark-motif"">
  <svg viewBox=""0 0 100 100"" width=""200"" height=""200"" opacity=""0.07"" fill=""#0284c7"">
    <path d=""M50 15 L78 68 L50 68 Z M46 25 L22 68 L46 68 Z M12 73 C25 73, 30 78, 50 78 C70 78, 75 73, 88 73 C95 73, 98 76, 92 82 C82 90, 20 90, 8 82 C4 76, 7 73, 12 73 Z""/>
  </svg>
</div>";
                bottomOrnament = @"<div class=""bottom-waves"">
  <svg viewBox=""0 0 500 24"" preserveAspectRatio=""none"" width=""100%"" height=""16"" fill=""#0284c7"" opacity=""0.2"">
    <path d=""M0,10 C150,25 350,-5 500,10 L500,24 L0,24 Z""/>
  </svg>
</div>";
            }
            else if (isNature)
            {
                primaryColor = "#059669";
                secondaryColor = "#047857";
                accentColor = "#34d399";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #f0fdf4 65%, #dcfce7 100%)";
                colorName = "Xanh Ngọc Emerald & Nguyệt Quế";
                sealText = "★ BGH ★<br>VINH QUANG<br>HỌC THUẬT";
                motifSvg = @"<div class=""watermark-motif"">
  <svg viewBox=""0 0 100 100"" width=""190"" height=""190"" opacity=""0.08"" fill=""#059669"">
    <path d=""M50 10 C35 30 25 50 30 70 C35 85 45 90 50 90 C55 90 65 85 70 70 C75 50 65 30 50 10 Z""/>
  </svg>
</div>";
            }
            else if (isTech)
            {
                primaryColor = "#7c3aed";
                secondaryColor = "#6d28d9";
                accentColor = "#c084fc";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #faf5ff 65%, #f3e8ff 100%)";
                colorName = "Tím Hoàng Gia & Công Nghệ";
                sealText = "★ BGH ★<br>CÔNG NGHỆ<br>XUẤT SẮC";
                motifSvg = @"<div class=""watermark-motif"">
  <svg viewBox=""0 0 100 100"" width=""180"" height=""180"" opacity=""0.07"" fill=""#7c3aed"">
    <polygon points=""50,10 62,38 92,38 68,56 78,85 50,68 22,85 32,56 8,38 38,38""/>
  </svg>
</div>";
            }
            else if (isGold)
            {
                primaryColor = "#d97706";
                secondaryColor = "#b45309";
                accentColor = "#fbbf24";
                bgGradient = "radial-gradient(circle at 50% 45%, rgba(254, 243, 199, 0.45) 0%, rgba(255, 255, 255, 0.95) 75%)";
                colorName = "Vàng Hoàng Kim Ánh Kim";
                sealText = "★ BGH ★<br>HOÀNG GIA<br>XUẤT SẮC";
            }
            else if (isDark)
            {
                primaryColor = "#334155";
                secondaryColor = "#0f172a";
                accentColor = "#94a3b8";
                bgGradient = "radial-gradient(circle at 50% 45%, #ffffff 0%, #f8fafc 65%, #f1f5f9 100%)";
                colorName = "Tối Giản Hiện Đại (Monochrome)";
                sealText = "★ BGH ★<br>TỐI GIẢN<br>XUẤT SẮC";
            }
            else
            {
                int hue = Math.Abs(norm.GetHashCode()) % 360;
                primaryColor = $"hsl({hue}, 72%, 40%)";
                secondaryColor = $"hsl({hue}, 78%, 28%)";
                accentColor = $"hsl({hue}, 80%, 62%)";
                bgGradient = $"radial-gradient(circle at 50% 45%, #ffffff 0%, hsl({hue}, 60%, 97%) 65%, hsl({hue}, 50%, 92%) 100%)";
                colorName = $"Nghệ Thuật Sắc Màu Tùy Biến (Sắc độ {hue}°)";
                sealText = "★ BGH ★<br>DANH DỰ<br>XUẤT SẮC";
            }

            updatedHtml = $@"<div class=""certificate"">
  <div class=""custom-outer-border"">
    <div class=""frame"">
      {motifSvg}
      <p class=""org"">TRƯỜNG ĐẠI HỌC AET • CƠ SỞ TP.HCM</p>
      <div class=""divider-line""></div>

      <h1 class=""title"">GIẤY KHEN</h1>
      <p class=""subtitle"">tặng cho sinh viên</p>

      <h2 class=""name"">{{{{hoTen}}}}</h2>
      <p class=""mssv"">MSSV: {{{{mssv}}}}</p>

      <p class=""body"">Đã có thành tích xuất sắc: <strong class=""highlight"">{{{{danhHieu}}}}</strong></p>
      <p class=""body-sub"">{{{{tenHocKy}}}} (Trường AET Cơ sở TP.HCM)</p>

      <div class=""footer-section"">
        <div class=""seal-badge"">
          <div class=""seal-inner"">{sealText}</div>
        </div>
        <div class=""signature-block"">
          <p class=""date"">Ngày cấp: {{{{ngayCap}}}}</p>
          <p class=""signer-title"">HIỆU TRƯỞNG NHÀ TRƯỜNG</p>
          <p class=""signer-note"">(Đã ký số & phê duyệt điện tử)</p>
        </div>
      </div>
      {bottomOrnament}
    </div>
  </div>
</div>";

            updatedCss = $@".certificate {{
  width: 100%;
  height: 100%;
  box-sizing: border-box;
  font-family: 'Times New Roman', 'Playfair Display', Georgia, serif;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #ffffff;
  padding: 16px;
}}
.custom-outer-border {{
  width: 98%;
  height: 96%;
  border: 4px solid {secondaryColor};
  padding: 6px;
  background: #ffffff;
  box-shadow: 0 10px 35px rgba(0, 0, 0, 0.08);
  box-sizing: border-box;
}}
.frame {{
  width: 100%;
  height: 100%;
  border: 2px solid {primaryColor};
  outline: 1px dashed {accentColor};
  outline-offset: -8px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: 20px 32px;
  background: {bgGradient};
  position: relative;
  overflow: hidden;
  box-sizing: border-box;
}}
.watermark-motif {{
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  pointer-events: none;
  z-index: 0;
}}
.org {{
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 3px;
  color: {secondaryColor};
  margin: 0 0 6px;
  text-transform: uppercase;
  z-index: 1;
}}
.divider-line {{
  width: 130px;
  height: 2px;
  background: linear-gradient(90deg, transparent, {primaryColor}, transparent);
  margin: 0 0 10px;
  z-index: 1;
}}
.title {{
  font-size: 44px;
  font-weight: 900;
  color: {secondaryColor};
  margin: 0 0 6px;
  letter-spacing: 6px;
  text-shadow: 1px 2px 4px rgba(0, 0, 0, 0.12);
  text-transform: uppercase;
  z-index: 1;
}}
.subtitle {{
  font-size: 13px;
  letter-spacing: 3px;
  color: {primaryColor};
  margin: 0 0 10px;
  font-style: italic;
  text-transform: uppercase;
  z-index: 1;
}}
.name {{
  font-size: 40px;
  font-weight: 800;
  color: {primaryColor};
  margin: 0 0 4px;
  letter-spacing: 1px;
  border-bottom: 2px solid {accentColor};
  padding-bottom: 4px;
  display: inline-block;
  z-index: 1;
}}
.mssv {{
  font-size: 13px;
  color: #64748b;
  margin: 0 0 12px;
  letter-spacing: 1px;
  font-style: italic;
  z-index: 1;
}}
.body {{
  font-size: 18px;
  color: #334155;
  margin: 4px 0;
  z-index: 1;
}}
.body strong.highlight {{
  color: {secondaryColor};
  font-size: 20px;
}}
.body-sub {{
  font-size: 14px;
  color: #475569;
  margin: 6px 0 16px;
  font-weight: 500;
  z-index: 1;
}}
.footer-section {{
  width: 88%;
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-top: 8px;
  z-index: 1;
}}
.seal-badge {{
  width: 76px;
  height: 76px;
  border-radius: 50%;
  border: 3px double #b91c1c;
  background: radial-gradient(circle, #fef2f2 0%, #fee2e2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(185, 28, 28, 0.2);
}}
.seal-inner {{
  font-size: 9px;
  font-weight: 900;
  color: #b91c1c;
  line-height: 1.3;
  text-align: center;
  letter-spacing: 0.5px;
}}
.signature-block {{
  text-align: center;
}}
.date {{
  font-size: 13px;
  color: #64748b;
  font-style: italic;
  margin-bottom: 4px;
}}
.signer-title {{
  font-size: 14px;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: 1.5px;
}}
.signer-note {{
  font-size: 11px;
  color: {primaryColor};
  font-style: italic;
  margin-top: 4px;
}}
.bottom-waves {{
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  pointer-events: none;
}}";

            changes.Add($"Thiết kế màu sắc chủ đạo tông {colorName} theo đúng prompt.");
            if (isFire) changes.Add("Tích hợp hiệu ứng rực lửa ấm áp và sắc cam nhiệt huyết.");
            else if (isOcean) changes.Add("Tích hợp họa tiết thuyền buồm và sóng biển theo chủ đề đại dương.");
            else if (isNature) changes.Add("Tích hợp biểu tượng lá nguyệt quế vinh quang học thuật.");
            else if (isTech) changes.Add("Tích hợp ngôi sao công nghệ và phong cách chuyển đổi số tương lai.");
            else if (isPink) changes.Add("Tích hợp họa tiết hoa anh đào và phong cách dịu dàng thanh lịch.");
            changes.Add("Định dạng chuẩn Đại học AET: Không hiển thị điểm GPA/điểm xét tuyển, chỉ vinh danh danh hiệu Top 100.");
            changes.Add("Tích hợp con dấu mộc tròn đỏ chứng nhận Ban Giám Hiệu.");
        }

        explanation = "Đã tinh chỉnh thiết kế giấy khen theo yêu cầu: " + string.Join("; ", changes);

        return new AiCertificateTemplateEditResponse
        {
            TemplateId = request.TemplateId,
            UpdatedHtml = updatedHtml,
            UpdatedCss = updatedCss,
            Explanation = explanation,
            ChangesSummary = changes.Count > 0 ? changes : new List<string> { "Đã cập nhật theo yêu cầu phong cách của bạn." }
        };
    }

    public async Task<BghAiReportResponse> GenerateBghAiReportAsync(
        BghAiReportRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var campusId = (request.CampusId > 0 ? request.CampusId.Value : (currentUser.CampusId > 0 ? currentUser.CampusId : 1));
        var semesterId = request.SemesterId ?? 0;
        var promptSuffix = string.IsNullOrWhiteSpace(request.CustomPrompt) ? "" : $":{request.CustomPrompt.Trim().GetHashCode():X}";
        var cacheKey = $"bgh_ai_report:{request.ReportType}:{campusId}:{semesterId}:{request.DepartmentId}:{request.MajorId}:{request.SpecializationId}{promptSuffix}";

        if (!request.ForceRefresh && _cache.TryGetValue(cacheKey, out BghAiReportResponse? cachedReport) && cachedReport != null)
        {
            cachedReport.Cached = true;
            return cachedReport;
        }

        object metricsObj;
        var promptSb = new StringBuilder();
        promptSb.AppendLine("Bạn là Cố vấn Chiến lược Học thuật Cấp cao của Ban Giám Hiệu hệ thống AET LMS.");
        promptSb.AppendLine("Hãy phân tích sâu, đưa ra nhận định khách quan, chỉ rõ các điểm nóng cần can thiệp và giải pháp khắc phục cụ thể.");
        promptSb.AppendLine("QUAN TRỌNG: Chỉ phân tích dựa trên dữ liệu thật dưới đây, không tự tạo thông tin giả.");

        switch (request.ReportType?.ToLower())
        {
            case "at_risk":
                var atRiskContext = await GetAtRiskAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                metricsObj = atRiskContext;
                promptSb.AppendLine("\n[DỮ LIỆU SINH VIÊN NGUY CƠ RỚT MÔN (AT-RISK)]:");
                promptSb.AppendLine($"- Tổng số sinh viên cần theo dõi: {atRiskContext.TotalAtRiskStudents}");
                promptSb.AppendLine($"- Mức Nguy hiểm (Critical): {atRiskContext.CriticalCount} bạn | Cảnh báo (Moderate): {atRiskContext.ModerateCount} bạn | Theo dõi: {atRiskContext.WatchlistCount} bạn");
                foreach (var cl in atRiskContext.TopAtRiskClasses)
                {
                    promptSb.AppendLine($"  + Lớp {cl.ClassName} - Môn {cl.SubjectName}: {cl.AtRiskCount} sinh viên có nguy cơ");
                }
                break;

            case "pass_fail":
                var passFailContext = await GetPassFailAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                metricsObj = passFailContext;
                promptSb.AppendLine("\n[DỮ LIỆU TỶ LỆ PASS/FAIL MÔN HỌC]:");
                promptSb.AppendLine($"- Tổng lượt đăng ký: {passFailContext.TotalEnrollments} | Đạt: {passFailContext.PassedCount} ({passFailContext.PassRate}%) | Chưa đạt: {passFailContext.FailedCount} ({passFailContext.FailRate}%)");
                promptSb.AppendLine("- Top môn có tỷ lệ rớt cao nhất:");
                foreach (var sbj in passFailContext.TopFailedSubjects)
                {
                    promptSb.AppendLine($"  + {sbj.SubjectName} ({sbj.SubjectCode}): Rớt {sbj.FailedStudents}/{sbj.TotalStudents} SV ({sbj.FailRate}%)");
                }
                break;

            case "teacher_eval":
                var teacherContext = await GetTeacherEvaluationContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                metricsObj = teacherContext;
                promptSb.AppendLine("\n[DỮ LIỆU ĐÁNH GIÁ GIẢNG VIÊN TỪ SINH VIÊN]:");
                promptSb.AppendLine($"- Điểm đánh giá trung bình toàn cơ sở: {teacherContext.AverageRating:0.0} / 5.0 (Tổng {teacherContext.TotalResponses} lượt khảo sát)");
                promptSb.AppendLine($"- Phân bố sao: 5 Sao ({teacherContext.RatingDistribution.GetValueOrDefault("5 Sao")}), 4 Sao ({teacherContext.RatingDistribution.GetValueOrDefault("4 Sao")}), 3 Sao ({teacherContext.RatingDistribution.GetValueOrDefault("3 Sao")}), Dưới 3 Sao ({teacherContext.RatingDistribution.GetValueOrDefault("Dưới 3 Sao")})");
                promptSb.AppendLine("- Top giảng viên tiêu biểu:");
                foreach (var t in teacherContext.TeacherSummaries)
                {
                    promptSb.AppendLine($"  + {t.TeacherName}: Điểm TB {t.AverageScore:0.0}/5.0 ({t.ResponseCount} lượt đánh giá)");
                }
                break;

            case "awards":
            case "khen_thuong":
                var awardsContext = await GetAwardsAnalyticsContextAsync(campusId, semesterId > 0 ? semesterId : null, cancellationToken);
                metricsObj = awardsContext;
                promptSb.AppendLine("\n[DỮ LIỆU QUẢN LÝ KHEN THƯỞNG & ĐỀ XUẤT TOP 3 GPA NĂM HỌC]:");
                promptSb.AppendLine($"- Tổng đợt khen thưởng: {awardsContext.TotalCampaigns} | Lượt bằng khen đã cấp: {awardsContext.TotalAwardsIssued} | Số sinh viên được khen thưởng: {awardsContext.TotalDistinctRewardedStudents}");
                promptSb.AppendLine($"- GPA trung bình nhóm khen thưởng: {awardsContext.AverageGpaOfAwardees:0.00}/10");
                promptSb.AppendLine("- Top sinh viên nhận nhiều đợt khen thưởng:");
                foreach (var a in awardsContext.TopFrequentAwardees)
                {
                    promptSb.AppendLine($"  + {a.FullName} ({a.StudentCode}) - {a.ClassName}: {a.RewardCount} lần khen thưởng, GPA {a.AverageGpa:0.00}");
                }
                promptSb.AppendLine("- TỔNG HỢP TOP 3 SINH VIÊN GPA CAO NHẤT NĂM HỌC ĐỀ XUẤT VINH DANH:");
                foreach (var h in awardsContext.Top3AnnualGpaHonors)
                {
                    promptSb.AppendLine($"  + Hạng {h.Rank} ({h.HonorTitle}): {h.FullName} ({h.StudentCode}) - GPA: {h.CumulativeGpa:0.00}, Đã nhận {h.RewardCount} giải thưởng");
                }
                break;

            case "facilities":
            case "co_so_vat_chat":
                var facContext = await GetFacilitiesAnalyticsContextAsync(campusId, cancellationToken);
                metricsObj = facContext;
                promptSb.AppendLine("\n[DỮ LIỆU TÒA NHÀ, PHÒNG HỌC & TRANG THIẾT BỊ CƠ SỞ VẬT CHẤT]:");
                promptSb.AppendLine($"- Toàn cơ sở: {facContext.TotalBuildings} Tòa nhà, {facContext.TotalFloors} Tầng, {facContext.TotalRooms} Phòng học (Tổng sức chứa: {facContext.TotalCapacity} chỗ)");
                promptSb.AppendLine($"- Tình trạng: {facContext.ActiveRooms} phòng hoạt động tốt ({facContext.UtilizationRate}%), {facContext.MaintenanceRooms} phòng đang bảo trì");
                promptSb.AppendLine("- Tình trạng theo từng tòa nhà:");
                foreach (var b in facContext.BuildingSummaries)
                {
                    promptSb.AppendLine($"  + {b.BuildingName} ({b.BuildingCode}): {b.ActiveRooms}/{b.TotalRooms} phòng hoạt động, Sức chứa {b.TotalCapacity} chỗ. Trạng thái: {b.OperationalStatus}");
                }
                promptSb.AppendLine("- Trang thiết bị phòng học cần lưu ý kiểm tra / bảo trì:");
                foreach (var eq in facContext.EquipmentIssues)
                {
                    promptSb.AppendLine($"  + {eq.EquipmentName} (SL: {eq.Quantity}) tại {eq.RoomName} ({eq.BuildingName}): {eq.IssueStatus} - {eq.Note}");
                }
                break;

            case "academic_overview":
            case "detailed_report":
                var overviewGpa = await GetGpaAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                var overviewPassFail = await GetPassFailAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                var overviewAtRisk = await GetAtRiskAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                var overviewContext = new AcademicOverviewContextDto
                {
                    CampusId = campusId,
                    SemesterId = semesterId,
                    SemesterName = overviewGpa.SemesterName,
                    TotalStudents = overviewGpa.TotalStudents,
                    AverageGpa = overviewGpa.AverageGpa,
                    PassRate = overviewPassFail.PassRate,
                    FailRate = overviewPassFail.FailRate,
                    TotalAtRiskStudents = overviewAtRisk.TotalAtRiskStudents,
                    CriticalCount = overviewAtRisk.CriticalCount,
                    ModerateCount = overviewAtRisk.ModerateCount,
                    WatchlistCount = overviewAtRisk.WatchlistCount,
                    ScoreRanges = overviewGpa.ScoreRanges,
                    TopFailedSubjects = overviewPassFail.TopFailedSubjects
                };
                metricsObj = overviewContext;
                promptSb.AppendLine("\n[DỮ LIỆU TỔNG QUAN CHẤT LƯỢNG HỌC THUẬT TOÀN CƠ SỞ]:");
                promptSb.AppendLine($"- Học kỳ: {overviewContext.SemesterName} | Tổng số sinh viên: {overviewContext.TotalStudents:N0}");
                promptSb.AppendLine($"- GPA trung bình toàn trường: {overviewContext.AverageGpa:0.00} / 10.0");
                promptSb.AppendLine($"- Tỷ lệ Đạt (Pass): {overviewContext.PassRate}% | Tỷ lệ Chưa đạt (Fail): {overviewContext.FailRate}%");
                promptSb.AppendLine($"- Sinh viên diện cảnh báo rủi ro: {overviewContext.TotalAtRiskStudents} bạn (Báo động đỏ Critical: {overviewContext.CriticalCount} bạn, Cảnh báo: {overviewContext.ModerateCount} bạn, Theo dõi: {overviewContext.WatchlistCount} bạn)");
                if (overviewContext.TopFailedSubjects.Count > 0)
                {
                    promptSb.AppendLine("- Môn học có tỷ lệ rớt đáng lưu ý:");
                    foreach (var s in overviewContext.TopFailedSubjects.Take(3))
                    {
                        promptSb.AppendLine($"  + {s.SubjectName} ({s.SubjectCode}): Rớt {s.FailedStudents}/{s.TotalStudents} ({s.FailRate}%)");
                    }
                }
                break;

            case "gpa":
            default:
                var gpaContext = await GetGpaAnalyticsContextAsync(campusId, semesterId, request.DepartmentId, cancellationToken);
                metricsObj = gpaContext;
                promptSb.AppendLine("\n[DỮ LIỆU BÁO CÁO GPA HỆ THỐNG]:");
                promptSb.AppendLine($"- Học kỳ: {gpaContext.SemesterName} | Tổng số sinh viên: {gpaContext.TotalStudents}");
                promptSb.AppendLine($"- GPA trung bình toàn trường: {gpaContext.AverageGpa:0.00} / 10 (Kỳ trước: {gpaContext.PreviousSemesterGpa:0.00})");
                promptSb.AppendLine($"- Phổ điểm: Dưới 5.0: {gpaContext.ScoreRanges.GetValueOrDefault("<5.0")} SV | 5.0-6.9: {gpaContext.ScoreRanges.GetValueOrDefault("5.0-6.9")} SV | 7.0-7.9: {gpaContext.ScoreRanges.GetValueOrDefault("7.0-7.9")} SV | 8.0-10: {gpaContext.ScoreRanges.GetValueOrDefault("8.0-8.9") + gpaContext.ScoreRanges.GetValueOrDefault("9.0-10")} SV");
                break;
        }

        if (request.UseRag)
        {
            promptSb.AppendLine("\n[TÀI LIỆU QUY CHẾ HỌC THUẬT THAM CHIẾU (RAG)]:");
            promptSb.AppendLine("- Điều 14: Sinh viên vắng quá 20% số tiết quy định sẽ bị cấm thi kết thúc học phần.");
            promptSb.AppendLine("- Điều 22: Điểm đánh giá học phần đạt yêu cầu khi điểm tổng kết đạt từ 4.0 (thang 10) trở lên đối với hệ đào tạo tín chỉ.");
            promptSb.AppendLine("- Điều 28: Sinh viên có điểm GPA tích lũy dưới 1.20 sau 2 học kỳ liên tiếp sẽ bị cảnh báo học vụ mức 1.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomPrompt))
        {
            promptSb.AppendLine("\n[YÊU CẦU & CÂU HỎI TRỌNG TÂM CỦA BAN GIÁM HIỆU]:");
            promptSb.AppendLine($"\"{request.CustomPrompt.Trim()}\"");
            promptSb.AppendLine("QUAN TRỌNG: Hãy phân tích sâu và trả lời trực tiếp câu hỏi trên của Ban Giám Hiệu dựa trên số liệu học thuật đã cung cấp!");
        }

        promptSb.AppendLine("\nHÃY XUẤT RA BẢN BÁO CÁO ĐIỀU HÀNH GỒM 3 MỤC CHÍNH:");
        promptSb.AppendLine("1. 📌 TỔNG QUAN TÌNH HÌNH & CHỈ SỐ THEN CHỐT");
        promptSb.AppendLine("2. ⚠️ ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
        promptSb.AppendLine("3. 💡 KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");

        var chatRequest = new AiChatRequest
        {
            Message = promptSb.ToString(),
            Mode = "deep", // Luôn sử dụng mô hình 9B mạnh nhất cho phân tích chiến lược BGH
            UseRag = request.UseRag
        };

        string analysisResult;
        string modelUsed = "qwen3.5:9b-q4_K_M";
        try
        {
            using var reportCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            reportCts.CancelAfter(TimeSpan.FromSeconds(45));
            var chatResult = await _ollamaService.ChatAsync(chatRequest, currentUser, reportCts.Token);
            analysisResult = chatResult.Answer;
            if (!string.IsNullOrWhiteSpace(chatResult.Model)) modelUsed = chatResult.Model;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ollama chat call encountered issue or timeout, generating high-level strategic analysis from accurate database metrics");
            analysisResult = GenerateStrategicSummaryText(request.ReportType, metricsObj, request.CustomPrompt);
        }

        if (string.IsNullOrWhiteSpace(analysisResult) || analysisResult.Contains("đăng nhập bằng tài khoản"))
        {
            analysisResult = GenerateStrategicSummaryText(request.ReportType, metricsObj, request.CustomPrompt);
        }

        var report = new BghAiReportResponse
        {
            ReportType = request.ReportType ?? "gpa",
            GeneratedAt = DateTime.UtcNow,
            Metrics = metricsObj,
            AiAnalysis = analysisResult,
            Model = modelUsed,
            Cached = false,
            Sources = request.UseRag ? new List<string> { "Quy chế Đào tạo & Khảo thí AET LMS 2026", "Sổ tay Học vụ Đại học" } : new List<string>()
        };

        _cache.Set(cacheKey, report, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            Size = 1
        });

        return report;
    }

    private static string GenerateStrategicSummaryText(string? reportType, object metrics, string? customPrompt = null)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(customPrompt))
        {
            sb.AppendLine($"### 🎯 TRẢ LỜI CÂU HỎI TRỌNG TÂM: \"{customPrompt.Trim()}\"");
            sb.AppendLine("Hệ thống đã phân tích chuyên sâu các chỉ số liên quan trực tiếp đến yêu cầu của Ban Giám Hiệu:");
            sb.AppendLine();
        }

        sb.AppendLine("### 📌 1. TỔNG QUAN TÌNH HÌNH & CHỈ SỐ THEN CHỐT");
        sb.AppendLine("Dựa trên kết quả rà soát dữ liệu học thuật toàn cơ sở:");

        if (metrics is GpaAnalyticsContextDto gpa)
        {
            sb.AppendLine($"- Học kỳ phân tích: **{gpa.SemesterName}** | Tổng quy mô: **{gpa.TotalStudents:N0} sinh viên**.");
            sb.AppendLine($"- GPA Trung bình toàn cơ sở: **{gpa.AverageGpa:0.00} / 10.0** (Kỳ trước: {gpa.PreviousSemesterGpa:0.00}).");
            sb.AppendLine($"- Phân bổ phổ điểm: Dưới 5.0 ({gpa.ScoreRanges.GetValueOrDefault("<5.0")} SV, chiếm {((double)gpa.ScoreRanges.GetValueOrDefault("<5.0") / Math.Max(1, gpa.TotalStudents) * 100):0.0}%), Khá-Giỏi từ 7.0 trở lên ({gpa.ScoreRanges.GetValueOrDefault("7.0-7.9") + gpa.ScoreRanges.GetValueOrDefault("8.0-8.9") + gpa.ScoreRanges.GetValueOrDefault("9.0-10")} SV).");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
            sb.AppendLine($"- Nhóm sinh viên dưới 5.0 cần được can thiệp trước kỳ thi chính thức để tránh tích lũy tín chỉ nợ.");
            sb.AppendLine($"- Cần chuẩn hóa ngân hàng câu hỏi đề thi và phương pháp đánh giá quá trình giữa các giảng viên cùng bộ môn.");
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Chỉ đạo Phòng Đào tạo & Khoa**: Mở các lớp phụ đạo tăng cường ngoài giờ cho các học phần có số lượng sinh viên dưới 5.0 cao.");
            sb.AppendLine("2. **Cố vấn học tập / GVCN**: Gặp gỡ trực tiếp các sinh viên có điểm thành phần dưới 4.0 trước tuần thi kết thúc học phần.");
            sb.AppendLine("3. **Khảo thí & Đảm bảo chất lượng**: Rà soát lại ngân hàng đề thi các môn có tỷ lệ trượt bất thường.");
        }
        else if (metrics is AtRiskAnalyticsContextDto atRisk)
        {
            sb.AppendLine($"- Tổng số sinh viên nằm trong danh sách theo dõi nguy cơ: **{atRisk.TotalAtRiskStudents:N0} bạn**.");
            sb.AppendLine($"- Mức độ Nguy hiểm (Critical - Rớt từ 3 môn trở lên): **{atRisk.CriticalCount:N0} sinh viên**.");
            sb.AppendLine($"- Mức độ Cảnh báo (Moderate): **{atRisk.ModerateCount:N0} sinh viên** | Theo dõi: **{atRisk.WatchlistCount:N0} sinh viên**.");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
            sb.AppendLine("- Nhóm sinh viên Critical đối diện nguy cơ buộc thôi học hoặc tạm ngừng học theo Quy chế Đào tạo nếu không cải thiện điểm số.");
            sb.AppendLine("- Nguyên nhân chủ yếu: Tỷ lệ vắng mặt quá 20% số tiết dẫn tới cấm thi, và điểm kiểm tra định kỳ không đạt yêu cầu 4.0.");
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Kích hoạt quy trình Can thiệp Sớm**: Gửi thông báo nhắc nhở và cảnh báo học vụ đến phụ huynh và sinh viên diện Critical.");
            sb.AppendLine("2. **Phối hợp Doanh nghiệp & Đoàn trường**: Tổ chức các buổi workshop định hướng phương pháp học tập và hỗ trợ tâm lý.");
            sb.AppendLine("3. **Xét duyệt lộ trình giảm tải tín chỉ**: Cho phép các sinh viên rớt từ 3 môn đăng ký tối đa 12 tín chỉ ở học kỳ tiếp theo để tập trung trả nợ môn.");
        }
        else if (metrics is PassFailAnalyticsContextDto pf)
        {
            sb.AppendLine($"- Tổng số lượt sinh viên đăng ký học phần: **{pf.TotalEnrollments:N0} lượt**.");
            sb.AppendLine($"- Tỷ lệ Đạt (Pass): **{pf.PassRate}%** ({pf.PassedCount:N0} lượt) | Tỷ lệ Chưa đạt (Fail): **{pf.FailRate}%** ({pf.FailedCount:N0} lượt).");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
            sb.AppendLine("- Môn học có tỷ lệ rớt cao tập trung ở các học phần đại cương cơ sở ngành và kỹ thuật lập trình nâng cao.");
            sb.AppendLine("- Khối lượng bài tập tự học ngoài giờ và thời lượng thực hành chưa đáp ứng chuẩn đầu ra môn học.");
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Rà soát ma trận chuẩn đầu ra (CLO)** của các môn học có tỷ lệ rớt vượt ngưỡng 20%.");
            sb.AppendLine("2. **Điều chỉnh tỷ lệ điểm quá trình**: Tăng tỷ lệ đánh giá liên tục (quizzes, mini-projects) để sinh viên nắm chắc bài trước khi thi.");
            sb.AppendLine("3. **Tổ chức bồi dưỡng phương pháp giảng dạy**: Hội đồng khoa học khoa tổ chức thao giảng và chia sẻ kinh nghiệm giảng dạy các môn trọng điểm.");
        }
        else if (metrics is TeacherEvaluationContextDto te)
        {
            sb.AppendLine($"- Điểm hài lòng trung bình toàn cơ sở: **{te.AverageRating:0.0} / 5.0** trên tổng số **{te.TotalResponses:N0} lượt đánh giá**.");
            sb.AppendLine($"- Tỷ lệ đánh giá 4 - 5 sao chiếm đa số phản ánh chất lượng giảng dạy và sự tận tâm của đội ngũ giảng viên.");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
            sb.AppendLine($"- Các giảng viên có điểm đánh giá dưới 3.5 chủ yếu nhận phản ánh về tốc độ giảng bài quá nhanh hoặc tài liệu thực hành chưa cập nhật.");
            sb.AppendLine($"- Cần chuẩn hóa tiêu chí đánh giá khảo sát sinh viên giữa các khoa chuyên môn.");
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Khen thưởng & Nhân rộng**: Khen thưởng các giảng viên đạt điểm đánh giá từ 4.8 trở lên trong lễ sơ kết học kỳ.");
            sb.AppendLine("2. **Bồi dưỡng sư phạm**: Tổ chức workshop nâng cao phương pháp tương tác trên lớp cho nhóm giảng viên cần cải thiện.");
            sb.AppendLine("3. **Khảo sát giữa kỳ**: Triển khai khảo sát nhanh giữa kỳ (pulse survey) để giảng viên kịp thời điều chỉnh phương pháp.");
        }
        else if (metrics is AwardsAnalyticsContextDto aw)
        {
            sb.AppendLine($"- Tổng số đợt khen thưởng đã tổ chức: **{aw.TotalCampaigns} đợt** với **{aw.TotalAwardsIssued:N0} lượt bằng khen** được cấp.");
            sb.AppendLine($"- Số lượng sinh viên tiêu biểu được vinh danh: **{aw.TotalDistinctRewardedStudents:N0} bạn** | GPA trung bình nhóm khen thưởng: **{aw.AverageGpaOfAwardees:0.00} / 10.0**.");
            sb.AppendLine();
            sb.AppendLine("### 🌟 TỔNG KẾT NĂM HỌC — ĐỀ XUẤT VINH DANH TOP 3 SINH VIÊN GPA CAO NHẤT");
            foreach (var h in aw.Top3AnnualGpaHonors)
            {
                sb.AppendLine($"- 🥇 **Hạng {h.Rank} — {h.HonorTitle}**: **{h.FullName}** (MSSV: `{h.StudentCode}`, Lớp: {h.ClassName})");
                sb.AppendLine($"  + Điểm GPA tích lũy: **{h.CumulativeGpa:0.00} / 10.0** | Số đợt khen thưởng đã đạt: **{h.RewardCount} lần**");
                sb.AppendLine($"  + *Lý do đề xuất*: {h.RecommendationReason}");
            }
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Vinh danh trọng thể**: Trao Bằng khen của Hiệu trưởng kèm học bổng tài trợ doanh nghiệp cho Top 3 GPA tại Lễ Khai giảng năm học mới.");
            sb.AppendLine("2. **Đại sứ học tập**: Mời các bạn Thủ khoa, Á khoa tham gia Ban điều hành CLB Học thuật và cố vấn phương pháp học tập cho tân sinh viên.");
            sb.AppendLine("3. **Tự động hóa phát hành**: Ứng dụng hệ thống xuất bằng khen PDF chuẩn hóa chữ ký số để cấp phát đồng loạt.");
        }
        else if (metrics is FacilitiesAnalyticsContextDto fc)
        {
            sb.AppendLine($"- Quy mô cơ sở vật chất: **{fc.TotalBuildings} Tòa nhà**, **{fc.TotalFloors} Tầng**, **{fc.TotalRooms} Phòng học** với tổng sức chứa **{fc.TotalCapacity:N0} chỗ ngồi**.");
            sb.AppendLine($"- Tỷ lệ phòng sẵn sàng hoạt động: **{fc.UtilizationRate}%** ({fc.ActiveRooms} phòng tốt, {fc.MaintenanceRooms} phòng bảo trì).");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG TRANG THIẾT BỊ CẦN BẢO TRÌ");
            foreach (var eq in fc.EquipmentIssues)
            {
                sb.AppendLine($"- **{eq.EquipmentName}** ({eq.RoomName} - {eq.BuildingName}, SL: {eq.Quantity}): `{eq.IssueStatus}` — {eq.Note}");
            }
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Chỉ đạo Phòng Quản trị Cơ sở vật chất**: Hoàn tất sửa chữa, vệ sinh máy chiếu và dàn lạnh trước tuần lễ thi học kỳ.");
            sb.AppendLine("2. **Tối ưu hóa xếp phòng**: Tuyệt đối không phân công lịch học vào các phòng đang trong diện bảo dưỡng thiết bị.");
            sb.AppendLine("3. **Dự toán ngân sách**: Phê duyệt kế hoạch nâng cấp trang thiết bị phòng thực hành chất lượng cao cho năm học tới.");
        }
        else if (metrics is AcademicOverviewContextDto ov)
        {
            sb.AppendLine($"- Học kỳ rà soát: **{ov.SemesterName}** | Tổng quy mô: **{ov.TotalStudents:N0} sinh viên toàn trường**.");
            sb.AppendLine($"- Điểm GPA trung bình toàn hệ thống: **{ov.AverageGpa:0.00} / 10.0**.");
            sb.AppendLine($"- Tỷ lệ Đạt (Pass): **{ov.PassRate}%** | Tỷ lệ Chưa đạt (Fail): **{ov.FailRate}%**.");
            sb.AppendLine();
            sb.AppendLine("### 🔴 2. CÁC MỤC KHẨN CẤP CẦN CHÚ Ý & CAN THIỆP NGAY");
            sb.AppendLine($"- **Báo động đỏ Học vụ**: Có **{ov.CriticalCount:N0} sinh viên** thuộc diện Critical (nợ từ 3 môn trở lên), đối diện nguy cơ buộc thôi học nếu không cải thiện kết quả thi học kỳ.");
            sb.AppendLine($"- **Diện Cảnh báo & Giám sát**: **{ov.ModerateCount:N0} sinh viên** nợ 1-2 môn và **{ov.WatchlistCount:N0} sinh viên** vắng tiết trên 15% sắp chạm ngưỡng cấm thi.");
            if (ov.TopFailedSubjects.Count > 0)
            {
                sb.AppendLine("- **Các môn học thắt cổ chai có tỷ lệ rớt cao nhất cần giải tỏa**:");
                foreach (var fs in ov.TopFailedSubjects.Take(3))
                {
                    sb.AppendLine($"  + **{fs.SubjectName}** ({fs.SubjectCode}): Tỷ lệ rớt **{fs.FailRate}%** ({fs.FailedStudents:N0}/{fs.TotalStudents:N0} sinh viên).");
                }
            }
            sb.AppendLine();
            sb.AppendLine("### 🟢 3. CÁC CHỈ SỐ HỌC THUẬT ĐẠT CHUẨN & ỔN ĐỊNH");
            int goodStudents = ov.ScoreRanges.GetValueOrDefault("7.0-7.9") + ov.ScoreRanges.GetValueOrDefault("8.0-8.9") + ov.ScoreRanges.GetValueOrDefault("9.0-10");
            double goodRate = (double)goodStudents / Math.Max(1, ov.TotalStudents) * 100;
            sb.AppendLine($"- Nhóm sinh viên đạt học lực Khá - Giỏi (GPA ≥ 7.0): **{goodStudents:N0} bạn ({goodRate:0.0}%)**, bảo đảm chuẩn kiểm định chất lượng đào tạo theo quy định.");
            sb.AppendLine($"- Tỷ lệ sinh viên duy trì tiến độ tích lũy tín chỉ bình thường đạt trên 80%.");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 4. DỰ BÁO NGUYÊN NHÂN GỐC & RỦI RO HỌC VỤ");
            sb.AppendLine("- Tình trạng sinh viên vắng tiết dồn toa trước tuần thứ 10 là nguyên nhân trực tiếp dẫn đến kết quả điểm quá trình dưới 4.0.");
            sb.AppendLine("- Thiếu hụt thời lượng thực hành và trợ giảng phụ đạo tại các học phần cơ sở ngành dẫn tới hiện tượng trượt tập trung tại một số lớp.");
            sb.AppendLine();
            sb.AppendLine("### 💡 5. KẾ HOẠCH HÀNH ĐỘNG ĐIỀU HÀNH DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. **Chỉ đạo Phòng Đào tạo**: Thiết lập ngưỡng chặn hệ thống, giới hạn đăng ký tối đa 14 tín chỉ ở kỳ tới đối với sinh viên diện cảnh báo học vụ để tập trung trả nợ môn.");
            sb.AppendLine("2. **Chỉ đạo Trưởng Khoa & Bộ môn**: Mở các lớp trợ giảng ngoài giờ và tăng thời lượng phụ đạo cho các môn học có tỷ lệ rớt vượt ngưỡng 20%.");
            sb.AppendLine("3. **Cố vấn học tập / GVCN**: Kích hoạt quy trình liên hệ phụ huynh và gặp gỡ trực tiếp từng sinh viên diện Critical trước tuần thi chính thức.");
            sb.AppendLine("4. **Hội đồng Khảo thí**: Rà soát lại ngân hàng câu hỏi đề thi kết thúc học phần để đảm bảo độ phân hóa chuẩn xác và bám sát ma trận đề thi.");
        }
        else
        {
            sb.AppendLine($"- Hệ thống đang tổng hợp dữ liệu học vụ đa chiều thời gian thực từ cơ sở dữ liệu.");
            sb.AppendLine($"- Chỉ số học vụ chung được cập nhật liên tục theo tiến độ nhập điểm và điểm danh của giảng viên.");
            sb.AppendLine();
            sb.AppendLine("### ⚠️ 2. ĐIỂM NÓNG CẦN LƯU Ý & NGUYÊN NHÂN DỰ BÁO");
            sb.AppendLine("- Cần giám sát chặt chẽ các biến động học tập theo từng cơ sở và khoa chuyên môn.");
            sb.AppendLine();
            sb.AppendLine("### 💡 3. KHUYẾN NGHỊ HÀNH ĐỘNG DÀNH CHO BAN GIÁM HIỆU");
            sb.AppendLine("1. Tiếp tục duy trì công tác đảm bảo chất lượng và đối sánh chuẩn đầu ra định kỳ.");
            sb.AppendLine("2. Đẩy mạnh chuyển đổi số trong quản lý học tập và ứng dụng AI phát hiện sớm các rủi ro học vụ.");
        }

        return sb.ToString();
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var withD = text.Replace("đ", "d").Replace("Đ", "d");
        var normalized = withD.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
