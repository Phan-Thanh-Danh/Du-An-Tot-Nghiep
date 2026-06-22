using System.Data;
using System.Text.Json;
using Backend.Data;
using Backend.DTOs.QuestionBank;
using Backend.DTOs.QuizAttempts;
using Backend.DTOs.QuizManagement;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services.Audit;
using Backend.Services.QuizGrading;
using Backend.Services.QuizRuntime;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.QuizAttempts;

public class QuizAttemptService : IQuizAttemptService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApplicationDbContext _db;
    private readonly IQuizAvailabilityService _availabilityService;
    private readonly IQuizGradingService _gradingService;
    private readonly IAuditLogService _auditLogService;

    public QuizAttemptService(
        ApplicationDbContext db,
        IQuizAvailabilityService availabilityService,
        IQuizGradingService gradingService,
        IAuditLogService auditLogService)
    {
        _db = db;
        _availabilityService = availabilityService;
        _gradingService = gradingService;
        _auditLogService = auditLogService;
    }

    public Task<QuizAvailabilityDto> GetAvailabilityAsync(int quizId, int studentId, CancellationToken ct)
    {
        return _availabilityService.GetAvailabilityAsync(quizId, studentId, ct);
    }

    public async Task<StartQuizAttemptResponse> StartAsync(int quizId, int studentId, CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

        var quiz = await _availabilityService.SynchronizeQuizStatusAsync(quizId, DateTime.UtcNow, ct);
        await EnsureLessonQuizAsync(quiz, ct);

        var config = QuizConfigurationDto.Parse(quiz.CauHinhDeThi);
        if (quiz.TrangThai != "dang_mo")
        {
            throw new ApiException(409, "Quiz chưa mở hoặc đã đóng");
        }

        var existingActive = await _db.PhienThiHocSinhs
            .Where(x => x.MaDeKiemTra == quizId && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "dang_hoat_dong" && x.NopLuc == null)
            .OrderByDescending(x => x.LanThu)
            .FirstOrDefaultAsync(ct);

        if (existingActive != null)
        {
            existingActive = await AutoSubmitIfExpiredAsync(existingActive, config, ct);
            if (existingActive.TrangThaiLuong == "dang_hoat_dong")
            {
                await transaction.CommitAsync(ct);
                return await BuildStartResponseAsync(existingActive.MaPhienThi, ct);
            }
        }

        var completedCount = await _db.PhienThiHocSinhs
            .CountAsync(x => x.MaDeKiemTra == quizId && x.MaHocSinh == studentId && x.MaCaThi == null && x.TrangThaiLuong == "da_dung", ct);

        if (!config.KhongGioiHanSoLan && completedCount >= config.SoLanLamToiDa)
        {
            throw new ApiException(409, "Đã hết số lượt làm quiz");
        }

        var questions = await LoadQuestionsAsync(quizId, ct);
        if (questions.Count == 0)
        {
            throw new ApiException(409, "Quiz chưa có câu hỏi");
        }

        var now = DateTime.UtcNow;
        var attemptNumber = await _db.PhienThiHocSinhs
            .Where(x => x.MaDeKiemTra == quizId && x.MaHocSinh == studentId && x.MaCaThi == null)
            .Select(x => (int?)x.LanThu)
            .MaxAsync(ct) ?? 0;

        var dueAt = now.AddMinutes(quiz.ThoiGianPhut);
        if (config.DongLuc.HasValue && config.DongLuc.Value < dueAt)
        {
            dueAt = config.DongLuc.Value;
        }

        var snapshot = BuildSnapshot(questions, config);
        var attempt = new PhienThiHocSinh
        {
            MaDeKiemTra = quizId,
            MaHocSinh = studentId,
            MaCaThi = null,
            LanThu = attemptNumber + 1,
            BatDauLuc = now,
            HanNopLuc = dueAt,
            TrangThaiLuong = "dang_hoat_dong",
            TrangThaiKyTen = "chua_ky",
            TrangThaiCongBo = "chua_co_diem",
            DeThiSnapshotJson = JsonSerializer.Serialize(snapshot, JsonOptions),
            NgayCapNhat = now
        };

        _db.PhienThiHocSinhs.Add(attempt);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        await _auditLogService.LogAsync(
            "PhienThiHocSinh",
            attempt.MaPhienThi.ToString(),
            "START_LESSON_QUIZ_ATTEMPT",
            null,
            new { attempt.MaDeKiemTra, attempt.LanThu },
            studentId,
            null,
            "Học sinh bắt đầu lượt làm quiz bài học",
            ct);

        return await BuildStartResponseAsync(attempt.MaPhienThi, ct);
    }

    public async Task SaveAnswersAsync(int attemptId, SaveQuizAnswersRequest request, int studentId, CancellationToken ct)
    {
        var attempt = await GetOwnedLessonAttemptAsync(attemptId, studentId, ct);
        var quizId = attempt.MaDeKiemTra;
        var config = QuizConfigurationDto.Parse(attempt.DeKiemTra!.CauHinhDeThi);
        attempt = await AutoSubmitIfExpiredAsync(attempt, config, ct);
        if (attempt.TrangThaiLuong != "dang_hoat_dong")
        {
            throw new ApiException(409, "Lượt làm đã kết thúc");
        }

        ValidateAnswers(request.Answers, quizId);
        attempt.SaoLuuCucBo = JsonSerializer.Serialize(request, JsonOptions);
        attempt.NgayCapNhat = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<QuizAttemptResultDto> SubmitAsync(int attemptId, SubmitQuizAttemptRequest request, int studentId, CancellationToken ct)
    {
        var attempt = await GetOwnedLessonAttemptAsync(attemptId, studentId, ct);
        var quizId = attempt.MaDeKiemTra;
        var config = QuizConfigurationDto.Parse(attempt.DeKiemTra!.CauHinhDeThi);
        attempt = await AutoSubmitIfExpiredAsync(attempt, config, ct);
        if (attempt.TrangThaiLuong != "dang_hoat_dong")
        {
            return await BuildResultAsync(attempt, config, config.HienKetQuaSauKhiNop, config.HienDapAnDungSauKhiNop, ct);
        }

        ValidateAnswers(request.Answers, quizId);
        await FinalizeAttemptAsync(attempt, request.Answers, config, DateTime.UtcNow, ct);
        await _db.SaveChangesAsync(ct);

        await _auditLogService.LogAsync(
            "PhienThiHocSinh",
            attempt.MaPhienThi.ToString(),
            "SUBMIT_LESSON_QUIZ_ATTEMPT",
            null,
            new { attempt.MaDeKiemTra, attempt.LanThu, attempt.DiemTuDong, attempt.DiemCuoiCung, attempt.SoCauDung, attempt.KetQuaDat },
            studentId,
            null,
            "Học sinh nộp bài quiz bài học",
            ct);

        return await BuildResultAsync(attempt, config, config.HienKetQuaSauKhiNop, config.HienDapAnDungSauKhiNop, ct);
    }

    public async Task<QuizAttemptHistoryDto> GetHistoryAsync(int quizId, int studentId, CancellationToken ct)
    {
        var quiz = await _db.DeKiemTras.FirstOrDefaultAsync(x => x.MaDeKiemTra == quizId, ct);
        if (quiz == null)
        {
            throw new ApiException(404, "Không tìm thấy quiz");
        }

        await EnsureLessonQuizAsync(quiz, ct);
        var config = QuizConfigurationDto.Parse(quiz.CauHinhDeThi);
        var attempts = await _db.PhienThiHocSinhs
            .Where(x => x.MaDeKiemTra == quizId && x.MaHocSinh == studentId && x.MaCaThi == null)
            .OrderBy(x => x.LanThu)
            .ToListAsync(ct);

        var completed = attempts.Where(x => x.TrangThaiLuong == "da_dung").ToList();
        return new QuizAttemptHistoryDto
        {
            MaDeKiemTra = quizId,
            LanLam = attempts.Select(x => new QuizAttemptHistoryItemDto
            {
                MaPhienThi = x.MaPhienThi,
                LanThu = x.LanThu,
                BatDauLuc = x.BatDauLuc,
                NopLuc = x.NopLuc,
                TrangThaiLuong = x.TrangThaiLuong,
                DiemTuDong = x.DiemTuDong,
                DiemCuoiCung = x.DiemCuoiCung,
                SoCauDung = x.SoCauDung,
                KetQuaDat = x.KetQuaDat
            }).ToList(),
            KetQuaCuoi = BuildFinalSummary(completed, config)
        };
    }

    private async Task<PhienThiHocSinh> GetOwnedLessonAttemptAsync(int attemptId, int studentId, CancellationToken ct)
    {
        var attempt = await _db.PhienThiHocSinhs
            .Include(x => x.DeKiemTra)
            .FirstOrDefaultAsync(x => x.MaPhienThi == attemptId && x.MaHocSinh == studentId && x.MaCaThi == null, ct);

        if (attempt == null)
        {
            throw new ApiException(404, "Không tìm thấy lượt làm quiz");
        }

        await EnsureLessonQuizAsync(attempt.DeKiemTra!, ct);
        return attempt;
    }

    private async Task EnsureLessonQuizAsync(DeKiemTra quiz, CancellationToken ct)
    {
        var isLessonContent = await _db.BaiHocNoiDungs
            .AnyAsync(x => x.MaDeKiemTra == quiz.MaDeKiemTra && x.LoaiNoiDung == "quiz", ct);

        if (!isLessonContent)
        {
            throw new ApiException(400, "Quiz này không phải quiz gắn trong bài học");
        }

        if (!string.IsNullOrWhiteSpace(quiz.LoaiDeThi) && quiz.LoaiDeThi != "quiz_bai_hoc")
        {
            throw new ApiException(400, "Loại đề không phù hợp với quiz bài học");
        }
    }

    private async Task<PhienThiHocSinh> AutoSubmitIfExpiredAsync(PhienThiHocSinh attempt, QuizConfigurationDto config, CancellationToken ct)
    {
        if (attempt.TrangThaiLuong != "dang_hoat_dong" || attempt.HanNopLuc == null || attempt.HanNopLuc > DateTime.UtcNow)
        {
            return attempt;
        }

        var answers = _gradingService.ParseAnswersJson(attempt.SaoLuuCucBo);
        await FinalizeAttemptAsync(attempt, answers, config, attempt.HanNopLuc.Value, ct);
        await _db.SaveChangesAsync(ct);
        return attempt;
    }

    private async Task FinalizeAttemptAsync(
        PhienThiHocSinh attempt,
        IReadOnlyList<QuizAttemptAnswerDto> answers,
        QuizConfigurationDto config,
        DateTime submittedAt,
        CancellationToken ct)
    {
        var questions = await LoadQuestionsAsync(attempt.MaDeKiemTra, ct);
        var grading = _gradingService.GradeObjectiveQuestions(questions, answers, false);
        var finalScore = grading.CoCauTuLuan ? (decimal?)null : grading.DiemTracNghiem;

        attempt.CauTraLoiJson = JsonSerializer.Serialize(new SaveQuizAnswersRequest { Answers = answers.ToList() }, JsonOptions);
        attempt.SaoLuuCucBo = attempt.CauTraLoiJson;
        attempt.NopLuc = submittedAt;
        attempt.TrangThaiLuong = "da_dung";
        attempt.DiemTuDong = grading.DiemTracNghiem;
        attempt.DiemCuoiCung = finalScore;
        attempt.SoCauDung = grading.SoCauDung;
        attempt.KetQuaDat = ResolvePass(config, finalScore, grading.SoCauDung);
        attempt.TrangThaiCongBo = grading.CoCauTuLuan ? "chua_co_diem" : "da_cham_xong";
        attempt.NgayCapNhat = DateTime.UtcNow;
    }

    private async Task<QuizAttemptResultDto> BuildResultAsync(
        PhienThiHocSinh attempt,
        QuizConfigurationDto config,
        bool includeResult,
        bool includeAnswerKeys,
        CancellationToken ct)
    {
        var questions = await LoadQuestionsAsync(attempt.MaDeKiemTra, ct);
        var answers = _gradingService.ParseAnswersJson(attempt.CauTraLoiJson);
        var grading = _gradingService.GradeObjectiveQuestions(questions, answers, includeResult && includeAnswerKeys);

        return new QuizAttemptResultDto
        {
            MaPhienThi = attempt.MaPhienThi,
            MaDeKiemTra = attempt.MaDeKiemTra,
            LanThu = attempt.LanThu,
            TrangThaiLuong = attempt.TrangThaiLuong,
            NopLuc = attempt.NopLuc,
            DiemTuDong = includeResult ? attempt.DiemTuDong : null,
            DiemCuoiCung = includeResult ? attempt.DiemCuoiCung : null,
            SoCauDung = includeResult ? attempt.SoCauDung : null,
            TongSoCau = questions.Count,
            KetQuaDat = includeResult ? attempt.KetQuaDat : null,
            CoCauTuLuanChoCham = grading.CoCauTuLuan && attempt.DiemCuoiCung == null,
            HienKetQua = includeResult,
            HienDapAnDung = includeResult && includeAnswerKeys,
            ChiTiet = includeResult ? grading.ChiTiet : null
        };
    }

    private async Task<StartQuizAttemptResponse> BuildStartResponseAsync(int attemptId, CancellationToken ct)
    {
        var attempt = await _db.PhienThiHocSinhs
            .Include(x => x.DeKiemTra)
            .FirstAsync(x => x.MaPhienThi == attemptId, ct);

        var questions = await LoadQuestionsAsync(attempt.MaDeKiemTra, ct);
        var snapshot = ParseSnapshot(attempt.DeThiSnapshotJson);
        var orderedQuestions = ApplySnapshot(questions, snapshot);

        return new StartQuizAttemptResponse
        {
            MaPhienThi = attempt.MaPhienThi,
            MaDeKiemTra = attempt.MaDeKiemTra,
            LanThu = attempt.LanThu,
            BatDauLuc = attempt.BatDauLuc,
            HanNopLuc = attempt.HanNopLuc,
            ThoiGianPhut = attempt.DeKiemTra!.ThoiGianPhut,
            TieuDe = attempt.DeKiemTra.TieuDe,
            CauHoi = orderedQuestions
        };
    }

    private async Task<List<CauHoiDeKiemTra>> LoadQuestionsAsync(int quizId, CancellationToken ct)
    {
        return await _db.CauHoiDeKiemTras
            .Include(x => x.CauHoi)
            .Where(x => x.MaDeKiemTra == quizId)
            .OrderBy(x => x.ThuTu)
            .ToListAsync(ct);
    }

    private static QuizAttemptSnapshotDto BuildSnapshot(IReadOnlyList<CauHoiDeKiemTra> questions, QuizConfigurationDto config)
    {
        var orderedQuestions = config.XaoTronCauHoi
            ? questions.OrderBy(_ => Guid.NewGuid()).ToList()
            : questions.OrderBy(x => x.ThuTu ?? int.MaxValue).ThenBy(x => x.MaCauHoi).ToList();

        var snapshot = new QuizAttemptSnapshotDto();
        for (var i = 0; i < orderedQuestions.Count; i++)
        {
            var relation = orderedQuestions[i];
            var choices = ParseChoices(relation.CauHoi?.LuaChon);
            var choiceOrder = config.XaoTronDapAn
                ? choices.OrderBy(_ => Guid.NewGuid()).Select(x => x.Id).ToList()
                : choices.Select(x => x.Id).ToList();

            snapshot.Questions.Add(new QuizQuestionSnapshotDto
            {
                MaCauHoi = relation.MaCauHoi,
                ThuTu = i + 1,
                ChoiceOrder = choiceOrder
            });
        }

        return snapshot;
    }

    private static IReadOnlyList<QuizAttemptQuestionDto> ApplySnapshot(
        IReadOnlyList<CauHoiDeKiemTra> questions,
        QuizAttemptSnapshotDto snapshot)
    {
        var byQuestionId = questions.ToDictionary(x => x.MaCauHoi);
        return snapshot.Questions
            .OrderBy(x => x.ThuTu)
            .Where(x => byQuestionId.ContainsKey(x.MaCauHoi))
            .Select(x =>
            {
                var relation = byQuestionId[x.MaCauHoi];
                var choices = ParseChoices(relation.CauHoi?.LuaChon);
                var orderedChoices = OrderChoices(choices, x.ChoiceOrder);
                return new QuizAttemptQuestionDto
                {
                    MaCauHoi = relation.MaCauHoi,
                    LoaiCauHoi = relation.CauHoi!.LoaiCauHoi,
                    NoiDung = relation.CauHoi.NoiDung,
                    KieuLuaChon = relation.CauHoi.KieuLuaChon,
                    LuaChon = orderedChoices.Count > 0
                        ? JsonSerializer.SerializeToElement(orderedChoices, JsonOptions)
                        : null,
                    DiemSo = relation.DiemSo,
                    ThuTu = x.ThuTu
                };
            })
            .ToList();
    }

    private static List<QuestionChoiceDto> ParseChoices(string? choicesJson)
    {
        if (string.IsNullOrWhiteSpace(choicesJson))
        {
            return new List<QuestionChoiceDto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<QuestionChoiceDto>>(choicesJson, JsonOptions) ?? new List<QuestionChoiceDto>();
        }
        catch (JsonException)
        {
            return new List<QuestionChoiceDto>();
        }
    }

    private static List<QuestionChoiceDto> OrderChoices(List<QuestionChoiceDto> choices, IReadOnlyList<string> order)
    {
        if (order.Count == 0)
        {
            return choices;
        }

        var orderMap = order.Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index, StringComparer.OrdinalIgnoreCase);

        return choices
            .OrderBy(x => orderMap.TryGetValue(x.Id, out var index) ? index : int.MaxValue)
            .ThenBy(x => x.Id)
            .ToList();
    }

    private static QuizAttemptSnapshotDto ParseSnapshot(string? snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            return new QuizAttemptSnapshotDto();
        }

        try
        {
            return JsonSerializer.Deserialize<QuizAttemptSnapshotDto>(snapshotJson, JsonOptions) ?? new QuizAttemptSnapshotDto();
        }
        catch (JsonException)
        {
            return new QuizAttemptSnapshotDto();
        }
    }

    private static void ValidateAnswers(IReadOnlyList<QuizAttemptAnswerDto> answers, int quizId)
    {
        if (answers.Select(x => x.MaCauHoi).Distinct().Count() != answers.Count)
        {
            throw new ApiException(400, "Danh sách câu trả lời bị trùng câu hỏi");
        }

        if (answers.Any(x => x.MaCauHoi <= 0))
        {
            throw new ApiException(400, "Mã câu hỏi trong câu trả lời không hợp lệ");
        }
    }

    private static bool? ResolvePass(QuizConfigurationDto config, decimal? finalScore, int correctCount)
    {
        if (config.CachTinhDat == "theo_so_cau_dung")
        {
            return correctCount >= config.SoCauDungToiThieu;
        }

        return finalScore.HasValue ? finalScore.Value >= config.DiemDat : null;
    }

    private static QuizResultSummaryDto? BuildFinalSummary(IReadOnlyList<PhienThiHocSinh> completedAttempts, QuizConfigurationDto config)
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
            : selected!.DiemCuoiCung ?? selected.DiemTuDong ?? 0;

        var correctCount = selected?.SoCauDung ?? scoredAttempts.Max(x => x.SoCauDung ?? 0);
        return new QuizResultSummaryDto
        {
            DiemCuoiCung = Math.Round(finalScore, 2),
            KetQuaDat = config.CachTinhDat == "theo_diem"
                ? finalScore >= config.DiemDat
                : correctCount >= config.SoCauDungToiThieu,
            SoCauDung = correctCount
        };
    }
}
