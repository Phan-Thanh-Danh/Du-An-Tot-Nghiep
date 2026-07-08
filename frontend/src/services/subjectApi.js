import { apiRequest } from './apiClient'

export const subjectApi = {
  list(params = {}) {
    const query = new URLSearchParams()
    Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
    const qs = query.toString()
    return apiRequest(`/api/master-data/subjects${qs ? '?' + qs : ''}`)
  },
  get(id) {
    return apiRequest(`/api/master-data/subjects/${id}`)
  },
  create(data) {
    return apiRequest('/api/master-data/subjects', { method: 'POST', body: JSON.stringify(data) })
  },
  update(id, data) {
    return apiRequest(`/api/master-data/subjects/${id}`, { method: 'PUT', body: JSON.stringify(data) })
  },
  deactivate(id) {
    return apiRequest(`/api/master-data/subjects/${id}/deactivate`, { method: 'PATCH' })
  },
  activate(id) {
    return apiRequest(`/api/master-data/subjects/${id}/activate`, { method: 'PATCH' })
  },
}
