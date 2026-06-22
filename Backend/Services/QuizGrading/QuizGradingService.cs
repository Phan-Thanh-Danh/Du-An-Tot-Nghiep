using System.Text.Json;
using Backend.DTOs.QuizAttempts;
using Backend.Models;

namespace Backend.Services.QuizGrading;

public class QuizGradingService : IQuizGradingService
{
    public QuizGradingResultDto GradeObjectiveQuestions(
        IReadOnlyList<CauHoiDeKiemTra> questions,
        IReadOnlyList<QuizAttemptAnswerDto> answers,
        bool includeAnswerKeys)
    {
        var answersByQuestion = answers
            .GroupBy(x => x.MaCauHoi)
            .ToDictionary(x => x.Key, x => x.Last());

        var details = new List<QuizQuestionGradingDetailDto>();
        decimal objectiveScore = 0;
        var correctCount = 0;
        var wrongCount = 0;
        var unansweredCount = 0;
        var hasEssay = false;

        foreach (var relation in questions.OrderBy(x => x.ThuTu ?? int.MaxValue).ThenBy(x => x.MaCauHoi))
        {
            var question = relation.CauHoi;
            if (question == null)
            {
                continue;
            }

            if (question.LoaiCauHoi == "tu_luan")
            {
                hasEssay = true;
                details.Add(new QuizQuestionGradingDetailDto
                {
                    MaCauHoi = relation.MaCauHoi,
                    DiemToiDa = relation.DiemSo,
                    DiemDatDuoc = null,
                    Dung = null,
                    ChuaTraLoi = !answersByQuestion.TryGetValue(relation.MaCauHoi, out var essayAnswer)
                        || string.IsNullOrWhiteSpace(essayAnswer.EssayText)
                });
                continue;
            }

            var correctAnswers = ParseStringList(question.DapAnDung);
            answersByQuestion.TryGetValue(relation.MaCauHoi, out var answer);
            var selectedAnswers = NormalizeAnswerIds(answer?.SelectedOptionIds);
            var isUnanswered = selectedAnswers.Count == 0;
            var isCorrect = !isUnanswered && selectedAnswers.SetEquals(correctAnswers);
            var earnedScore = isCorrect ? relation.DiemSo : 0m;

            if (isUnanswered)
            {
                unansweredCount++;
            }
            else if (isCorrect)
            {
                correctCount++;
                objectiveScore += earnedScore;
            }
            else
            {
                wrongCount++;
            }

            details.Add(new QuizQuestionGradingDetailDto
            {
                MaCauHoi = relation.MaCauHoi,
                DiemToiDa = relation.DiemSo,
                DiemDatDuoc = earnedScore,
                Dung = isCorrect,
                ChuaTraLoi = isUnanswered,
                DapAnDung = includeAnswerKeys ? correctAnswers.OrderBy(x => x).ToList() : null,
                GiaiThichDapAn = includeAnswerKeys ? question.GiaiThichDapAn : null
            });
        }

        return new QuizGradingResultDto
        {
            DiemTracNghiem = objectiveScore,
            SoCauDung = correctCount,
            SoCauSai = wrongCount,
            SoCauChuaTraLoi = unansweredCount,
            TongSoCau = questions.Count,
            CoCauTuLuan = hasEssay,
            ChiTiet = details
        };
    }

    public IReadOnlyList<QuizAttemptAnswerDto> ParseAnswersJson(string? answersJson)
    {
        if (string.IsNullOrWhiteSpace(answersJson))
        {
            return Array.Empty<QuizAttemptAnswerDto>();
        }

        try
        {
            using var document = JsonDocument.Parse(answersJson);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("answers", out var answersElement))
            {
                return ParseAnswerArray(answersElement);
            }

            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Answers", out var pascalAnswersElement))
            {
                return ParseAnswerArray(pascalAnswersElement);
            }

            if (root.ValueKind == JsonValueKind.Array)
            {
                return ParseAnswerArray(root);
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                return ParseDictionaryAnswers(root);
            }
        }
        catch (JsonException)
        {
            return Array.Empty<QuizAttemptAnswerDto>();
        }

        return Array.Empty<QuizAttemptAnswerDto>();
    }

    private static IReadOnlyList<QuizAttemptAnswerDto> ParseAnswerArray(JsonElement arrayElement)
    {
        if (arrayElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<QuizAttemptAnswerDto>();
        }

        var answers = new List<QuizAttemptAnswerDto>();
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var questionId = TryGetInt(item, "maCauHoi") ?? TryGetInt(item, "MaCauHoi") ?? TryGetInt(item, "questionId");
            if (!questionId.HasValue)
            {
                continue;
            }

            var selected = TryGetStringList(item, "selectedOptionIds")
                ?? TryGetStringList(item, "SelectedOptionIds")
                ?? TryGetStringList(item, "dapAn")
                ?? TryGetStringList(item, "answer")
                ?? new List<string>();

            answers.Add(new QuizAttemptAnswerDto
            {
                MaCauHoi = questionId.Value,
                SelectedOptionIds = selected,
                EssayText = TryGetString(item, "essayText") ?? TryGetString(item, "EssayText") ?? TryGetString(item, "noiDungTuLuan")
            });
        }

        return answers;
    }

    private static IReadOnlyList<QuizAttemptAnswerDto> ParseDictionaryAnswers(JsonElement root)
    {
        var answers = new List<QuizAttemptAnswerDto>();
        foreach (var property in root.EnumerateObject())
        {
            if (!int.TryParse(property.Name, out var questionId))
            {
                continue;
            }

            answers.Add(new QuizAttemptAnswerDto
            {
                MaCauHoi = questionId,
                SelectedOptionIds = ReadStringList(property.Value)
            });
        }

        return answers;
    }

    private static HashSet<string> ParseStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return NormalizeAnswerIds(ReadStringList(document.RootElement));
        }
        catch (JsonException)
        {
            return NormalizeAnswerIds(new[] { json });
        }
    }

    private static List<string>? TryGetStringList(JsonElement item, string propertyName)
    {
        return item.TryGetProperty(propertyName, out var value) ? ReadStringList(value) : null;
    }

    private static List<string> ReadStringList(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            return value.EnumerateArray()
                .Select(ReadScalarString)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        var scalar = ReadScalarString(value);
        return string.IsNullOrWhiteSpace(scalar) ? new List<string>() : new List<string> { scalar! };
    }

    private static string? ReadScalarString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.Object when value.TryGetProperty("id", out var id) => id.GetString(),
            JsonValueKind.Object when value.TryGetProperty("Id", out var id) => id.GetString(),
            JsonValueKind.Object when value.TryGetProperty("optionId", out var id) => id.GetString(),
            JsonValueKind.Object when value.TryGetProperty("key", out var id) => id.GetString(),
            _ => null
        };
    }

    private static int? TryGetInt(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var number) => number,
            JsonValueKind.String when int.TryParse(value.GetString(), out var number) => number,
            _ => null
        };
    }

    private static string? TryGetString(JsonElement item, string propertyName)
    {
        return item.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static HashSet<string> NormalizeAnswerIds(IEnumerable<string>? values)
    {
        return values?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
