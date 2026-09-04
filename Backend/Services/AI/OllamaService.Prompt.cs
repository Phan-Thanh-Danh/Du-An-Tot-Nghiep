using System.Net.Http;
using System.Text;
using System.Text.Json;
using Backend.Exceptions;

namespace Backend.Services.AI;

public partial class OllamaService
{
    private async Task<List<Backend.DTOs.AI.AiGeneratedQuestionDto>> GenerateQuestionsWithAiAsync(
        string subject, string? topic, int count, string difficulty, CancellationToken cancellationToken)
    {
        var question = AiOutput.Schema(new
        {
            noiDung = new { type = "string" },
            luaChon = new { type = "array", minItems = 4, maxItems = 4, items = AiOutput.Schema(new
            {
                id = new { type = "string", @enum = new[] { "A", "B", "C", "D" } }, text = new { type = "string" }
            }, "id", "text") },
            dapAnDung = new { type = "string", @enum = new[] { "A", "B", "C", "D" } },
            giaiThich = new { type = "string" }
        }, "noiDung", "luaChon", "dapAnDung", "giaiThich");
        var result = new List<Backend.DTOs.AI.AiGeneratedQuestionDto>();
        // Small batches keep 3B output complete; persist only after every batch passes validation.
        while (result.Count < count)
        {
            var batchSize = Math.Min(5, count - result.Count);
            var schema = AiOutput.Schema(new { questions = new { type = "array", minItems = batchSize, maxItems = batchSize, items = question } }, "questions");
            var answer = await CompleteAsync(
                "Soạn câu hỏi trắc nghiệm bằng tiếng Việt theo ĐÚNG môn, chủ đề, độ khó và yêu cầu người dùng. Mỗi câu có 4 đáp án khác nhau A/B/C/D, đúng 1 đáp án và giải thích chính xác. Không lặp câu trước; không đổi sang chủ đề khác. Chỉ trả JSON.",
                JsonSerializer.Serialize(new { subject, instruction = topic, difficulty, count = batchSize, previous = result.Select(x => x.NoiDung) }, AiOutput.JsonOptions),
                schema, "fast", 3000, cancellationToken);
            var batch = AiOutput.Parse<QuizBatch>(answer);
            if (batch.Questions.Count != batchSize || batch.Questions.Any(q =>
                string.IsNullOrWhiteSpace(q.NoiDung) || string.IsNullOrWhiteSpace(q.GiaiThich) ||
                q.LuaChon.Count != 4 || !q.LuaChon.Select(x => x.Id).Order().SequenceEqual(new[] { "A", "B", "C", "D" }) ||
                q.LuaChon.Any(x => string.IsNullOrWhiteSpace(x.Text)) || q.LuaChon.Select(x => x.Text.Trim()).Distinct().Count() != 4 ||
                !q.LuaChon.Any(x => x.Id == q.DapAnDung)))
                throw new ApiException(502, "AI tạo câu hỏi chưa hợp lệ; chưa lưu bộ đề. Vui lòng thử lại.");
            result.AddRange(batch.Questions);
            if (result.Select(x => x.NoiDung.Trim()).Distinct().Count() != result.Count)
                throw new ApiException(502, "AI trả câu hỏi trùng nhau; chưa lưu bộ đề. Vui lòng thử lại.");
        }
        result.ForEach(q => q.DoKho = difficulty);
        return result;
    }

    private sealed class QuizBatch
    {
        public required List<Backend.DTOs.AI.AiGeneratedQuestionDto> Questions { get; set; }
    }

    public Task<string> CompleteAsync(string systemPrompt, string userPrompt, object schema,
        string mode = "fast", int maxTokens = 2048, CancellationToken cancellationToken = default)
    {
        // Reject oversized context instead of silently dropping the user's instructions.
        if (systemPrompt.Length + userPrompt.Length > 24000)
            throw new ApiException(400, "Nội dung quá dài cho một lần xử lý. Hãy chia yêu cầu thành các bước nhỏ.");
        return _gate.ExecuteWithGateAsync(async token =>
        {
            var model = mode == "deep" ? _options.DeepModel : _options.FastModel;
            if (string.IsNullOrWhiteSpace(model)) model = _options.ChatModel;
            var payload = new
            {
                model, stream = false, think = false, format = schema,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt + "\nReturn JSON matching this schema:\n" + JsonSerializer.Serialize(schema) },
                    new { role = "user", content = userPrompt }
                },
                options = new { temperature = 0.1, num_ctx = Math.Max(8192, _options.ContextLength), num_predict = maxTokens }
            };
            try
            {
                using var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var response = await _httpClient.PostAsync("api/chat", body, token);
                if (!response.IsSuccessStatusCode)
                    throw new ApiException(response.StatusCode == System.Net.HttpStatusCode.NotFound ? 503 : 502,
                        "Mô hình AI chưa sẵn sàng. Yêu cầu chưa được áp dụng.");
                using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(token));
                var root = json.RootElement;
                if (root.TryGetProperty("done_reason", out var reason) && reason.GetString() == "length")
                    throw new ApiException(502, "AI chưa hoàn tất nội dung. Hãy chia yêu cầu thành các bước nhỏ.");
                var answer = root.GetProperty("message").GetProperty("content").GetString();
                if (string.IsNullOrWhiteSpace(answer))
                    throw new ApiException(502, "AI chưa trả về kết quả hợp lệ. Yêu cầu chưa được áp dụng.");
                return answer;
            }
            catch (HttpRequestException)
            {
                throw new ApiException(503, "Dịch vụ AI hiện không kết nối được. Yêu cầu chưa được áp dụng.");
            }
            catch (JsonException)
            {
                throw new ApiException(502, "Phản hồi AI không đúng định dạng. Yêu cầu chưa được áp dụng.");
            }
        }, cancellationToken);
    }
}
