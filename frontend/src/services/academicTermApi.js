import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

export const academicTermApi = {
  list(params = {}) {
    return apiRequest(`/api/master-data/academic-terms${buildQuery(params)}`)
  },

  get(id) {
    return apiRequest(`/api/master-data/academic-terms/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/academic-terms', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/academic-terms/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  },

  lock(id) {
    return apiRequest(`/api/master-data/academic-terms/${id}/lock`, { method: 'PATCH' })
  },

  unlock(id) {
    return apiRequest(`/api/master-data/academic-terms/${id}/unlock`, { method: 'PATCH' })
  },
}
