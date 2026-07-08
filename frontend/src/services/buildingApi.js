import { apiRequest } from './apiClient'

export const buildingApi = {
  list(params = {}) {
    const query = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
    const qs = query.toString()
    return apiRequest(`/api/master-data/buildings${qs ? '?' + qs : ''}`)
  },

  get(id) {
    return apiRequest(`/api/master-data/buildings/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/buildings', { method: 'POST', body: JSON.stringify(data) })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/buildings/${id}`, { method: 'PUT', body: JSON.stringify(data) })
  },

  delete(id) {
    return apiRequest(`/api/master-data/buildings/${id}`, { method: 'DELETE' })
  },
}
