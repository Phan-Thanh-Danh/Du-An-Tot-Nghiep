import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

export const scheduleApi = {
  list(params = {}) {
    return apiRequest(`/api/thoi-khoa-bieu${buildQuery(params)}`)
  },

  get(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`)
  },

  create(data) {
    return apiRequest('/api/thoi-khoa-bieu', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  update(id, data) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  },

  cancel(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}/cancel`, { method: 'PATCH' })
  },

  checkConflicts(data) {
    return apiRequest('/api/thoi-khoa-bieu/check-xung-dot', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  checkConflictsBatch(data = {}) {
    return apiRequest('/api/thoi-khoa-bieu/check-xung-dot-batch', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  listDrafts(params = {}) {
    return apiRequest(`/api/thoi-khoa-bieu/drafts${buildQuery(params)}`)
  },

  generateSessions(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}/generate-sessions`, { method: 'POST' })
  },

  generateDraft(data) {
    return apiRequest('/api/thoi-khoa-bieu/generate', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  getDraft(id) {
    return apiRequest(`/api/thoi-khoa-bieu/drafts/${id}`)
  },

  publishDraft(data) {
    return apiRequest('/api/thoi-khoa-bieu/publish', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  deleteDraft(id) {
    return apiRequest(`/api/thoi-khoa-bieu/drafts/${id}`, { method: 'DELETE' })
  },
}
