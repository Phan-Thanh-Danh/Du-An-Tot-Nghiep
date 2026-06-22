using Backend.Data;
using Backend.DTOs.QuizAttempts;
using Backend.DTOs.QuizManagement;
using Backend.Exceptions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.QuizRuntime;

public class QuizAvailabilityService : IQuizAvailabilityService
{
    private readonly ApplicationDbContext _db;

    public QuizAvailabilityService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DeKiemTra> SynchronizeQuizStatusAsync(int quizId, DateTime utcNow, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId, ct);
        if (quiz == null)
        {
            throw new ApiException(404, "Không tìm thấy quiz");
        }

        var nextStatus = ResolvePublishedStatus(quiz, utcNow);
        if (nextStatus != quiz.TrangThai)
        {
            quiz.TrangThai = nextStatus;
            quiz.NgayCapNhat = utcNow;
            await _db.SaveChangesAsync(ct);
        }

        return quiz;
    }

    public async Task<int> SynchronizeScheduledQuizzesAsync(DateTime utcNow, CancellationToken ct)
    {
        var candidates = await _db.DeKiemTras
            .Where(x => x.TrangThai == "da_len_lich" || x.TrangThai == "dang_mo")
            .ToListAsync(ct);

        var changed = 0;
        foreach (var quiz in candidates)
        {
            var nextStatus = ResolvePublishedStatus(quiz, utcNow);
            if (nextStatus != quiz.TrangThai)
            {
                quiz.TrangThai = nextStatus;
                quiz.NgayCapNhat = utcNow;
                changed++;
            }
        }

        if (changed > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return changed;
    }

    public async Task<QuizAvailabilityDto> GetAvailabilityAsync(int quizId, int studentId, CancellationToken ct)
    {
        var quiz = await SynchronizeQuizStatusAsync(quizId, DateTime.UtcNow, ct);
        var config = QuizConfigurationDto.Parse(quiz.CauHinhDeThi);
        var attempts = await _db.PhienThiHocSinhs
            .Where(x => x.MaDeKiemTra == quizId && x.MaHocSinh == studentId && x.MaCaThi == null)
            .OrderBy(x => x.LanThu)
            .ToListAsync(ct);

        var activeAttempt = attempts.LastOrDefault(x => x.TrangThaiLuong == "dang_hoat_dong" && x.NopLuc == null);
        var completedAttempts = attempts.Where(x => x.TrangThaiLuong == "da_dung").ToList();
        var completedCount = completedAttempts.Count;
        var maxAttempts = config.KhongGioiHanSoLan ? null : config.SoLanLamToiDa;
        var canAttempt = quiz.TrangThai == "dang_mo";
        string? reason = null;

        if (!canAttempt)
        {
            reason = quiz.TrangThai switch
            {
                "da_len_lich" => "Quiz chưa mở",
                "da_dong" => "Quiz đã đóng",
                "nhap" => "Quiz chưa được xuất bản",
                _ => "Quiz không ở trạng thái có thể làm"
            };
        }
        else if (!config.KhongGioiHanSoLan && completedCount >= config.SoLanLamToiDa && activeAttempt == null)
        {
            canAttempt = false;
            reason = "Đã hết số lượt làm quiz";
        }

        return new QuizAvailabilityDto
        {
            MaDeKiemTra = quiz.MaDeKiemTra,
            TrangThai = quiz.TrangThai,
            CoTheLam = canAttempt,
            LyDoKhongTheLam = reason,
            SoLanDaLam = completedCount,
            SoLanLamToiDa = maxAttempts,
            KhongGioiHanSoLan = config.KhongGioiHanSoLan,
            MoLuc = config.MoLuc,
            DongLuc = config.DongLuc,
            HanNopDangHoatDong = activeAttempt?.HanNopLuc,
            KetQuaHienTai = BuildCurrentResultSummary(completedAttempts, config)
        };
    }

    public string ResolvePublishedStatus(DeKiemTra quiz, DateTime utcNow)
    {
        if (quiz.TrangThai != "da_len_lich" && quiz.TrangThai != "dang_mo")
        {
            return quiz.TrangThai;
        }

        var config = QuizConfigurationDto.Parse(quiz.CauHinhDeThi);
        if (config.DongLuc.HasValue && config.DongLuc.Value <= utcNow)
        {
            return "da_dong";
        }

        if (config.MoLuc.HasValue && config.MoLuc.Value > utcNow)
        {
            return "da_len_lich";
        }

        return "dang_mo";
    }

    private static QuizResultSummaryDto? BuildCurrentResultSummary(
        IReadOnlyList<PhienThiHocSinh> completedAttempts,
        QuizConfigurationDto config)
    {
        var scoredAttempts = completedAttempts
            .Where(x => x.DiemCuoiCung.HasValue || x.DiemTuDong.HasValue)
            .ToList();

        if (scoredAttempts.Count == 0)
        {
            return null;
        }

        var selected = config.CachTinhDiemCuoi switch
        {
            "lan_cuoi" => scoredAttempts.OrderByDescending(x => x.LanThu).First(),
            "trung_binh" => null,
            _ => scoredAttempts.OrderByDescending(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0).ThenByDescending(x => x.LanThu).First()
        };

        var finalScore = config.CachTinhDiemCuoi == "trung_binh"
            ? scoredAttempts.Average(x => x.DiemCuoiCung ?? x.DiemTuDong ?? 0)
            : selected!.DiemCuoiCung ?? selected.DiemTuDong;

        return new QuizResultSummaryDto
        {
            DiemCuoiCung = Math.Round(finalScore ?? 0, 2),
            KetQuaDat = config.CachTinhDat == "theo_diem"
                ? finalScore >= config.DiemDat
                : (selected?.SoCauDung ?? scoredAttempts.Max(x => x.SoCauDung ?? 0)) >= config.SoCauDungToiThieu,
            SoCauDung = selected?.SoCauDung ?? scoredAttempts.Max(x => x.SoCauDung ?? 0)
        };
    }
}
