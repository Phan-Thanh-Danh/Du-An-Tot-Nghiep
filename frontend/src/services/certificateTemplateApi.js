import { apiRequest, unwrapApiData } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.append(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

const BASE = '/api/admin/certificate-templates'

export const certificateTemplateApi = {
  async getTemplates(params = {}) {
    return unwrapApiData(await apiRequest(`${BASE}${buildQuery(params)}`))
  },

  async getTemplate(id) {
    return unwrapApiData(await apiRequest(`${BASE}/${id}`))
  },

  async createTemplate(payload) {
    return unwrapApiData(await apiRequest(BASE, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  async updateTemplate(id, payload) {
    return unwrapApiData(await apiRequest(`${BASE}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }))
  },

  async disableTemplate(id) {
    return unwrapApiData(await apiRequest(`${BASE}/${id}`, {
      method: 'DELETE',
    }))
  },

  async previewTemplate(id, payload = {}) {
    return unwrapApiData(await apiRequest(`${BASE}/${id}/preview`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  async uploadRewardCertificatePdf(campaignId, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/reward-campaigns/${campaignId}/certificates/upload`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
}
