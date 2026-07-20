using System.Text.Json;
using Backend.Data;
using Backend.DTOs.QuizManagement;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Grading;

public class GradeAggregationService : IGradeAggregationService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<GradeAggregationService> _logger;

    public GradeAggregationService(ApplicationDbContext db, ILogger<GradeAggregationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CalculateGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
    {
        var configs = await _db.CauHinhDauDiemQuaTrinhs
            .Include(x => x.LoaiDauDiem)
            .Where(x => x.MaMonHoc == subjectId && x.MaHocKy == termId)
            .ToListAsync(ct);

        if (!configs.Any())
        {
            throw new ApiException(400, "Môn học chưa cấu hình đầu điểm quá trình.");
        }

        var totalWeight = configs.Sum(x => x.TrongSoNoiBo);
        if (totalWeight != 100m)
        {
            _logger.LogWarning("Tổng trọng số nội bộ của môn {SubjectId} học kỳ {TermId} không bằng 100 ({TotalWeight}).", subjectId, termId, totalWeight);
            throw new ApiException(400, $"Tổng trọng số đầu điểm quá trình phải là 100% (hiện tại: {totalWeight}%). Vui lòng cấu hình lại.");
        }

        decimal processGradeSum = 0;

        foreach (var config in configs)
        {
            decimal typeGrade = 0;
            var loaiCode = config.LoaiDauDiem?.MaCode;

            if (loaiCode == "chuyen_can")
            {
                typeGrade = await CalculateAttendanceGradeAsync(studentId, subjectId, termId, ct);
            }
            else if (loaiCode == "lab" || loaiCode == "assignment")
            {
                typeGrade = await CalculateAssignmentGradeAsync(studentId, subjectId, config, ct);
            }
            else if (loaiCode == "quiz" || loaiCode == "progress_test")
            {
                typeGrade = await CalculateQuizGradeAsync(studentId, subjectId, termId, loaiCode, config, ct);
            }

            processGradeSum += typeGrade * config.TrongSoNoiBo;
        }

        decimal finalProcessGrade = processGradeSum / 100m;

        // Formula 5
        var subjectConfig = await _db.CauHinhDiemMonHocs
            .FirstOrDefaultAsync(x => x.MaMonHoc == subjectId && x.MaHocKy == termId, ct);

        if (subjectConfig == null)
        {
            throw new ApiException(400, "Môn học chưa cấu hình điểm (trọng số, ngưỡng đạt).");
        }

        var diemRecord = await _db.DiemSos
            .FirstOrDefaultAsync(x => x.MaHocSinh == studentId && x.MaMonHoc == subjectId && x.MaHocKy == termId, ct);

        bool isNew = false;
        if (diemRecord == null)
        {
            var hocSinh = await _db.NguoiDungs.FindAsync(new object[] { studentId }, ct);
            diemRecord = new DiemSo
            {
                MaDonVi = hocSinh?.MaDonVi ?? 1,
                MaHocSinh = studentId,
                MaMonHoc = subjectId,
                MaHocKy = termId,
                TrangThai = "draft",
                DaKhoa = false,
                NamNhapHoc = hocSinh?.NamNhapHoc ?? DateTime.UtcNow.Year
            };
            isNew = true;
        }

        diemRecord.DiemQuaTrinh = Math.Round(finalProcessGrade, 2);

        decimal gk = diemRecord.DiemGiuaKy ?? 0m;
        decimal ck = diemRecord.DiemCuoiKy ?? 0m;
        decimal pt = diemRecord.DiemQuaTrinh ?? 0m;

        decimal gpa = (pt * subjectConfig.TrongSoQuaTrinh + gk * subjectConfig.TrongSoGiuaKy + ck * subjectConfig.TrongSoCuoiKy) / 100m;
        diemRecord.GpaMonHoc = Math.Round(gpa, 2);

        diemRecord.TrangThai = diemRecord.GpaMonHoc >= subjectConfig.NguongDat ? "Đạt" : "Rớt";

        if (isNew)
        {
            _db.DiemSos.Add(diemRecord);
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<decimal> CalculateAttendanceGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct)
    {
        // Công thức 3: Điểm Chuyên cần = 10 * (Số buổi có mặt / Tổng số buổi học của môn)
        var khoahocs = await _db.KhoaHocs
            .Where(k => k.MaMonHoc == subjectId && k.MaHocKy == termId)
            .Select(k => k.MaKhoaHoc)
            .ToListAsync(ct);

        if (!khoahocs.Any()) return 0m;

        var totalSessions = await _db.BuoiHocs
            .CountAsync(b => khoahocs.Contains(b.MaKhoaHoc) && b.TrangThaiBuoi == "da_dien_ra", ct); // only count sessions that have already occurred

        if (totalSessions == 0) return 0m;

        var presentCount = await _db.DiemDanhs
            .CountAsync(d => khoahocs.Contains(d.BuoiHoc!.MaKhoaHoc) 
                && d.MaHocSinh == studentId 
                && (d.TrangThai == "co_mat" || d.TrangThai == "di_muon"), ct);

        decimal grade = 10m * presentCount / totalSessions;
        return grade;
    }

    private async Task<decimal> CalculateAssignmentGradeAsync(int studentId, int subjectId, CauHinhDauDiemQuaTrinh config, CancellationToken ct)
    {
        // Công thức 2: Điểm(loại L) = SUM(điểm từng bài loại L, thiếu = 0) / SoLuongCot(loại L)
        var baiTaps = await _db.BaiTaps
            .Where(b => b.MaMonHoc == subjectId && b.MaCauHinhDauDiem == config.MaCauHinhDauDiem)
            .Select(b => b.MaBaiTap)
            .ToListAsync(ct);

        if (!baiTaps.Any()) return 0m;

        var submissions = await _db.BaiNops
            .Where(b => baiTaps.Contains(b.MaBaiTap) && b.MaHocSinh == studentId)
            .GroupBy(b => b.MaBaiTap)
            .Select(g => g.OrderByDescending(x => x.ThoiDiemNop).FirstOrDefault()) // Lấy bài nộp mới nhất
            .ToListAsync(ct);

        decimal sumGrade = 0;
        foreach (var sub in submissions)
        {
            if (sub != null && sub.DiemSo.HasValue)
            {
                sumGrade += sub.DiemSo.Value;
            }
        }

        return sumGrade / config.SoLuongCot;
    }

    private async Task<decimal> CalculateQuizGradeAsync(int studentId, int subjectId, int termId, string loaiCode, CauHinhDauDiemQuaTrinh config, CancellationToken ct)
    {
        // LoaiDeThi for quiz is "quiz_bai_hoc", for progress_test is "progress_test"
        string expectedLoaiDeThi = loaiCode == "quiz" ? "quiz_bai_hoc" : "progress_test";

        var deKiemTras = await _db.DeKiemTras
            .Where(d => d.MaMonHoc == subjectId && d.MaHocKy == termId && d.LoaiDeThi == expectedLoaiDeThi)
            .Select(d => new { d.MaDeKiemTra, d.CauHinhDeThi })
            .ToListAsync(ct);

        if (!deKiemTras.Any()) return 0m;

        decimal sumGrade = 0;

        foreach (var deKiemTra in deKiemTras)
        {
            var attempts = await _db.PhienThiHocSinhs
                .Where(x => x.MaDeKiemTra == deKiemTra.MaDeKiemTra && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "da_dung")
                .ToListAsync(ct);

            var scoredAttempts = attempts.Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue).ToList();
            if (scoredAttempts.Any())
            {
                var quizConfig = QuizConfigurationDto.Parse(deKiemTra.CauHinhDeThi);
                decimal testScore = 0;

                switch (quizConfig.CachTinhDiemCuoi)
                {
                    case "lan_cuoi":
                        var last = scoredAttempts.OrderByDescending(x => x.LanThu).First();
                        testScore = last.DiemCuoiCung ?? last.DiemTuDong ?? 0;
                        break;
                    case "trung_binh":
                        testScore = scoredAttempts.Average(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                        break;
                    default: // cao nhat
                        testScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                        break;
                }
                
                sumGrade += testScore;
            }
        }

        return sumGrade / config.SoLuongCot;
    }

    public async Task CalculateFallbackGradeAsync(int studentId, int subjectId, int termId, decimal? manualAssignmentGrade, decimal? manualExamGrade, CancellationToken ct = default)
    {
        var diemRecord = await _db.DiemSos
            .FirstOrDefaultAsync(x => x.MaHocSinh == studentId && x.MaMonHoc == subjectId && x.MaHocKy == termId, ct);

        if (diemRecord != null)
        {
            diemRecord.GpaMonHoc = ((manualAssignmentGrade ?? 0) * 0.4m) + ((manualExamGrade ?? 0) * 0.6m);
            await _db.SaveChangesAsync(ct);
        }
    }

    // ===== Phase 3: Read-only methods for display =====

    public async Task<decimal?> GetAttendanceGradeAsync(int studentId, int subjectId, int termId, CancellationToken ct = default)
    {
        var khoahocs = await _db.KhoaHocs
            .Where(k => k.MaMonHoc == subjectId && k.MaHocKy == termId)
            .Select(k => k.MaKhoaHoc)
            .ToListAsync(ct);

        if (!khoahocs.Any()) return null;

        var totalSessions = await _db.BuoiHocs
            .CountAsync(b => khoahocs.Contains(b.MaKhoaHoc) && b.TrangThaiBuoi == "da_dien_ra", ct);

        if (totalSessions == 0) return null;

        var presentCount = await _db.DiemDanhs
            .CountAsync(d => khoahocs.Contains(d.BuoiHoc!.MaKhoaHoc)
                && d.MaHocSinh == studentId
                && (d.TrangThai == "co_mat" || d.TrangThai == "di_muon"), ct);

        return 10m * presentCount / totalSessions;
    }

    public async Task<decimal?> GetAssignmentTypeGradeAsync(int studentId, int subjectId, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default)
    {
        var baiTaps = await _db.BaiTaps
            .Where(b => b.MaMonHoc == subjectId && b.MaCauHinhDauDiem == config.MaCauHinhDauDiem)
            .Select(b => b.MaBaiTap)
            .ToListAsync(ct);

        if (!baiTaps.Any() || config.SoLuongCot == 0) return null;

        var submissions = await _db.BaiNops
            .Where(b => baiTaps.Contains(b.MaBaiTap) && b.MaHocSinh == studentId)
            .GroupBy(b => b.MaBaiTap)
            .Select(g => g.OrderByDescending(x => x.ThoiDiemNop).FirstOrDefault())
            .ToListAsync(ct);

        decimal sumGrade = 0;
        foreach (var sub in submissions)
        {
            if (sub != null && sub.DiemSo.HasValue)
            {
                sumGrade += sub.DiemSo.Value;
            }
        }

        return sumGrade / config.SoLuongCot;
    }

    public async Task<decimal?> GetQuizTypeGradeAsync(int studentId, int subjectId, int termId, string loaiCode, CauHinhDauDiemQuaTrinh config, CancellationToken ct = default)
    {
        string expectedLoaiDeThi = loaiCode == "quiz" ? "quiz_bai_hoc" : "progress_test";

        var deKiemTras = await _db.DeKiemTras
            .Where(d => d.MaMonHoc == subjectId && d.MaHocKy == termId && d.LoaiDeThi == expectedLoaiDeThi)
            .Select(d => new { d.MaDeKiemTra, d.CauHinhDeThi })
            .ToListAsync(ct);

        if (!deKiemTras.Any() || config.SoLuongCot == 0) return null;

        decimal sumGrade = 0;

        foreach (var deKiemTra in deKiemTras)
        {
            var attempts = await _db.PhienThiHocSinhs
                .Where(x => x.MaDeKiemTra == deKiemTra.MaDeKiemTra && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "da_dung")
                .ToListAsync(ct);

            var scoredAttempts = attempts.Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue).ToList();
            if (scoredAttempts.Any())
            {
                var quizConfig = QuizConfigurationDto.Parse(deKiemTra.CauHinhDeThi);
                decimal testScore = 0;

                switch (quizConfig.CachTinhDiemCuoi)
                {
                    case "lan_cuoi":
                        var last = scoredAttempts.OrderByDescending(x => x.LanThu).First();
                        testScore = last.DiemCuoiCung ?? last.DiemTuDong ?? 0;
                        break;
                    case "trung_binh":
                        testScore = scoredAttempts.Average(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                        break;
                    default:
                        testScore = scoredAttempts.Max(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0);
                        break;
                }

                sumGrade += testScore;
            }
        }

        return sumGrade / config.SoLuongCot;
    }
}
