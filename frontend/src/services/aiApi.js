/**
 * aiApi.js
 * Module API tập trung cho dịch vụ AI Local (Ollama + Qwen)
 * Mọi yêu cầu AI phải đi qua Backend ASP.NET Core với JWT authentication.
 */
import { apiRequest, unwrapApiData } from './apiClient'

export const aiApi = {
  /**
   * Kiểm tra trạng thái máy chủ AI Ollama và các model local
   * @returns {Promise<{available: boolean, chatModel: string, chatModelAvailable: boolean, embeddingModel: string, embeddingModelAvailable: boolean, latencyMs: number, queueLength: number}>}
   */
  async checkHealth() {
    const res = await apiRequest('/api/ai/health')
    return unwrapApiData(res)
  },

  /**
   * Gửi tin nhắn / câu hỏi tới AI
   * @param {Object} payload
   * @param {string} payload.message - Nội dung câu hỏi (bắt buộc)
   * @param {string} [payload.conversationId] - Mã phiên hội thoại (tùy chọn)
   * @param {number} [payload.courseId] - Mã khóa học đang xem (tùy chọn)
   * @param {number} [payload.lessonId] - Mã bài học đang xem (tùy chọn)
   * @returns {Promise<{answer: string, conversationId: string, model: string, sources: string[]}>}
   */
  async chat(payload) {
    const res = await apiRequest('/api/ai/chat', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return unwrapApiData(res)
  },

  /**
   * Kiểm thử tính năng Vector Embedding (1024 dimensions)
   * @param {string} text - Đoạn văn bản cần tạo vector
   * @returns {Promise<{model: string, dimensions: number, success: boolean}>}
   */
  async testEmbedding(text) {
    const res = await apiRequest('/api/ai/embedding-test', {
      method: 'POST',
      body: JSON.stringify({ text }),
    })
    return unwrapApiData(res)
  },
}
