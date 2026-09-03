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

  /**
   * Lấy dữ liệu phân tích và đề xuất hành động AI cho Dashboard (cached IMemoryCache 30 phút)
   * @param {Object} [params]
   * @param {boolean} [params.forceRefresh] - Bỏ qua cache để phân tích lại
   * @returns {Promise<{role: string, executiveSummary: string, actionItems: Array<{title: string, description: string, severity: string, actionPrompt: string}>, generatedAt: string, cached: boolean}>}
   */
  async getDashboardInsight(params = {}) {
    const query = new URLSearchParams()
    if (params.forceRefresh) query.append('forceRefresh', 'true')
    const qs = query.toString() ? `?${query.toString()}` : ''
    const res = await apiRequest(`/api/ai/dashboard-insight${qs}`)
    return unwrapApiData(res)
  },

  /**
   * AI Action: Tự động sinh đề kiểm tra và lưu trực tiếp vào CSDL môn học
   * @param {Object} payload
   * @param {number} payload.maMonHoc
   * @param {string} payload.tieuDe
   * @param {string} [payload.chuDe]
   * @param {number} [payload.soLuongCauHoi]
   * @param {number} [payload.thoiGianPhut]
   * @param {string} [payload.doKho]
   * @returns {Promise<{success: boolean, maDeKiemTra: number, tieuDe: string, tongSoCau: number, actionUrl: string}>}
   */
  async generateQuiz(payload) {
    const res = await apiRequest('/api/ai/actions/generate-quiz', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return unwrapApiData(res)
  },

  /**
   * BGH AI Orchestrator: Tạo báo cáo phân tích chiến lược tổng hợp
   * @param {Object} payload
   * @param {string} payload.reportType - "gpa" | "at_risk" | "pass_fail" | "teacher_eval"
   * @param {number} [payload.semesterId]
   * @param {number} [payload.departmentId]
   * @param {string} [payload.mode] - "fast" | "deep"
   * @param {boolean} [payload.useRag]
   * @param {boolean} [payload.forceRefresh]
   */
  async generateBghReport(payload) {
    const res = await apiRequest('/api/ai/bgh/report', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return unwrapApiData(res)
  },

  async getGpaAnalytics(params = {}) {
    const query = new URLSearchParams(params).toString()
    const res = await apiRequest(`/api/ai/analytics/gpa${query ? `?${query}` : ''}`)
    return unwrapApiData(res)
  },

  async getAtRiskAnalytics(params = {}) {
    const query = new URLSearchParams(params).toString()
    const res = await apiRequest(`/api/ai/analytics/at-risk${query ? `?${query}` : ''}`)
    return unwrapApiData(res)
  },

  async getPassFailAnalytics(params = {}) {
    const query = new URLSearchParams(params).toString()
    const res = await apiRequest(`/api/ai/analytics/pass-fail${query ? `?${query}` : ''}`)
    return unwrapApiData(res)
  },

  async getTeacherEvaluationAnalytics(params = {}) {
    const query = new URLSearchParams(params).toString()
    const res = await apiRequest(`/api/ai/analytics/teacher-eval${query ? `?${query}` : ''}`)
    return unwrapApiData(res)
  },

  /**
   * Lấy dữ liệu phân tích khen thưởng và Top 3 GPA năm học
   */
  async getAwardsAnalytics(params = {}) {
    const query = new URLSearchParams(params).toString()
    const res = await apiRequest(`/api/ai/analytics/awards${query ? `?${query}` : ''}`)
    return unwrapApiData(res)
  },

  /**
   * Lấy dữ liệu phân tích cơ sở vật chất, tòa nhà và phòng học
   */
  async getFacilitiesAnalytics() {
    const res = await apiRequest('/api/ai/analytics/facilities')
    return unwrapApiData(res)
  },

  /**
   * AI Trợ lý chỉnh sửa mẫu bằng khen / giấy khen bằng ngôn ngữ tự nhiên
   * @param {Object} payload
   * @param {number} payload.templateId
   * @param {string} payload.instruction - Yêu cầu sửa (vd: viền vàng hoàng gia, tăng cỡ họ tên)
   * @param {string} payload.currentHtml
   * @param {string} payload.currentCss
   * @returns {Promise<{templateId: number, updatedHtml: string, updatedCss: string, explanation: string, changesSummary: string[]}>}
   */
  async editCertificateTemplate(payload) {
    const res = await apiRequest('/api/ai/certificate-templates/ai-edit', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return unwrapApiData(res)
  },
}
