import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

export const shiftApi = {
  list(params = {}) {
    return apiRequest(`/api/ca-hoc${buildQuery(params)}`)
  },

  getActive() {
    return apiRequest('/api/ca-hoc/active')
  },

  get(id) {
    return apiRequest(`/api/ca-hoc/${id}`)
  },

  create(data) {
    return apiRequest('/api/ca-hoc', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  update(id, data) {
    return apiRequest(`/api/ca-hoc/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  },

  toggleActive(id) {
    return apiRequest(`/api/ca-hoc/${id}/toggle-active`, { method: 'PATCH' })
  },
}
