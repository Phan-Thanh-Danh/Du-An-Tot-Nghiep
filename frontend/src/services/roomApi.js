import { apiRequest } from './apiClient'

export const roomApi = {
  list(params = {}) {
    const query = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
    const qs = query.toString()
    return apiRequest(`/api/master-data/rooms${qs ? '?' + qs : ''}`)
  },

  getByFloor(floorId) {
    return apiRequest(`/api/master-data/floors/${floorId}/rooms`)
  },

  get(id) {
    return apiRequest(`/api/master-data/rooms/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/rooms', { method: 'POST', body: JSON.stringify(data) })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/rooms/${id}`, { method: 'PUT', body: JSON.stringify(data) })
  },

  delete(id) {
    return apiRequest(`/api/master-data/rooms/${id}`, { method: 'DELETE' })
  },
}
