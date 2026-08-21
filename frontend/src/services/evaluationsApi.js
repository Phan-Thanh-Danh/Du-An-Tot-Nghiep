import { apiRequest } from './apiClient'

const BASE = '/api/admin/evaluations'

function unwrap(response) {
  return response?.data ?? response?.Data ?? response
}

export const evaluationsApi = {
  async getConfig() {
    return unwrap(await apiRequest(`${BASE}/config`))
  },

  async saveConfig(payload) {
    return unwrap(
      await apiRequest(`${BASE}/config`, {
        method: 'PUT',
        body: JSON.stringify(payload),
      }),
    )
  },

  getSummary() {
    return apiRequest(`${BASE}/summary`)
  },

  async getQuestions() {
    const data = unwrap(await apiRequest(`${BASE}/questions`))
    return Array.isArray(data) ? data : []
  },

  createQuestion(payload) {
    return apiRequest(`${BASE}/questions`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateQuestion(id, payload) {
    return apiRequest(`${BASE}/questions/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  toggleActive(id) {
    return apiRequest(`${BASE}/questions/${id}/toggle-active`, { method: 'POST' })
  },

  deleteQuestion(id) {
    return apiRequest(`${BASE}/questions/${id}`, { method: 'DELETE' })
  },
}
