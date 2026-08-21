import { apiRequest } from '@/services/apiClient'

function unwrapApiData(response) {
  return response?.data ?? response?.Data ?? response
}

export const passFailRuleApi = {
  getRules(params = {}) {
    const qs = new URLSearchParams(params).toString()
    return apiRequest(`/api/pass-fail-rules${qs ? '?' + qs : ''}`).then(unwrapApiData)
  },
  getRule(id) {
    return apiRequest(`/api/pass-fail-rules/${id}`).then(unwrapApiData)
  },
  createRule(payload) {
    return apiRequest('/api/pass-fail-rules', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
  updateRule(id, payload) {
    return apiRequest(`/api/pass-fail-rules/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
}
