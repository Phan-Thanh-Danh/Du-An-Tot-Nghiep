using Backend.DTOs.QuizAttempts;
using Backend.Models;
using Backend.Services.QuizGrading;
using NUnit.Framework;

namespace Backend.ApiTests;

public class QuizGradingServiceTests
{
    [Test]
    public void GradeObjectiveQuestions_ExactSingleChoiceAnswer_AwardsFullScore()
    {
        var service = new QuizGradingService();
        var questions = new List<CauHoiDeKiemTra>
        {
            new()
            {
                MaCauHoi = 10,
                DiemSo = 2,
                CauHoi = new CauHoi
                {
                    MaCauHoi = 10,
                    LoaiCauHoi = "trac_nghiem",
                    KieuLuaChon = "chon_mot",
                    DapAnDung = "[\"A\"]"
                }
            }
        };

        var result = service.GradeObjectiveQuestions(
            questions,
            new List<QuizAttemptAnswerDto>
            {
                new() { MaCauHoi = 10, SelectedOptionIds = new List<string> { "A" } }
            },
            includeAnswerKeys: true);

        Assert.Multiple(() =>
        {
            Assert.That(result.DiemTracNghiem, Is.EqualTo(2));
            Assert.That(result.SoCauDung, Is.EqualTo(1));
            Assert.That(result.SoCauSai, Is.EqualTo(0));
            Assert.That(result.ChiTiet.Single().DapAnDung, Is.EquivalentTo(new[] { "A" }));
        });
    }

    [Test]
    public void GradeObjectiveQuestions_MultipleChoiceRequiresExactSet()
    {
        var service = new QuizGradingService();
        var questions = new List<CauHoiDeKiemTra>
        {
            new()
            {
                MaCauHoi = 11,
                DiemSo = 3,
                CauHoi = new CauHoi
                {
                    MaCauHoi = 11,
                    LoaiCauHoi = "trac_nghiem",
                    KieuLuaChon = "chon_nhieu",
                    DapAnDung = "[\"A\",\"C\"]"
                }
            }
        };

        var partial = service.GradeObjectiveQuestions(
            questions,
            new List<QuizAttemptAnswerDto>
            {
                new() { MaCauHoi = 11, SelectedOptionIds = new List<string> { "A" } }
            },
            includeAnswerKeys: false);

        var exact = service.GradeObjectiveQuestions(
            questions,
            new List<QuizAttemptAnswerDto>
            {
                new() { MaCauHoi = 11, SelectedOptionIds = new List<string> { "C", "A" } }
            },
            includeAnswerKeys: false);

        Assert.Multiple(() =>
        {
            Assert.That(partial.DiemTracNghiem, Is.EqualTo(0));
            Assert.That(partial.SoCauSai, Is.EqualTo(1));
            Assert.That(exact.DiemTracNghiem, Is.EqualTo(3));
            Assert.That(exact.SoCauDung, Is.EqualTo(1));
        });
    }

    [Test]
    public void ParseAnswersJson_SupportsWrappedAnswersPayload()
    {
        var service = new QuizGradingService();

        var answers = service.ParseAnswersJson("""
        {
          "answers": [
            { "maCauHoi": 1, "selectedOptionIds": ["B"] },
            { "maCauHoi": 2, "essayText": "Bai lam tu luan" }
          ]
        }
        """);

        Assert.Multiple(() =>
        {
            Assert.That(answers, Has.Count.EqualTo(2));
            Assert.That(answers[0].SelectedOptionIds, Is.EquivalentTo(new[] { "B" }));
            Assert.That(answers[1].EssayText, Is.EqualTo("Bai lam tu luan"));
        });
    }
}
