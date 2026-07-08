import { apiRequest } from './apiClient'

export const floorApi = {
  list(params = {}) {
    const query = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
    const qs = query.toString()
    return apiRequest(`/api/master-data/floors${qs ? '?' + qs : ''}`)
  },

  getByBuilding(buildingId) {
    return apiRequest(`/api/master-data/buildings/${buildingId}/floors`)
  },

  get(id) {
    return apiRequest(`/api/master-data/floors/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/floors', { method: 'POST', body: JSON.stringify(data) })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/floors/${id}`, { method: 'PUT', body: JSON.stringify(data) })
  },

  delete(id) {
    return apiRequest(`/api/master-data/floors/${id}`, { method: 'DELETE' })
  },
}
