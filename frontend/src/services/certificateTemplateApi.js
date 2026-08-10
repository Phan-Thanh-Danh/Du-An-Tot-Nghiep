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
  getTemplates(params = {}) {
    return unwrapApiData(apiRequest(`${BASE}${buildQuery(params)}`))
  },

  getTemplate(id) {
    return unwrapApiData(apiRequest(`${BASE}/${id}`))
  },

  createTemplate(payload) {
    return unwrapApiData(apiRequest(BASE, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  updateTemplate(id, payload) {
    return unwrapApiData(apiRequest(`${BASE}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }))
  },

  disableTemplate(id) {
    return unwrapApiData(apiRequest(`${BASE}/${id}`, {
      method: 'DELETE',
    }))
  },

  previewTemplate(id, payload = {}) {
    return unwrapApiData(apiRequest(`${BASE}/${id}/preview`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  uploadRewardCertificatePdf(campaignId, payload) {
    return unwrapApiData(apiRequest(`/api/admin/reward-campaigns/${campaignId}/certificates/upload`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
}
