import { apiRequest } from './apiClient'

function appendParams(query, params = {}) {
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    const normalizedKey = key === 'PageSize' ? 'pageSize' : key === 'PageIndex' ? 'pageIndex' : key
    const normalizedValue = normalizedKey === 'pageSize' ? Math.min(Number(value) || 20, 100) : value
    query.set(normalizedKey, normalizedValue)
  })
}

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

export const subjectApi = {
  async list(params = {}) {
    const query = new URLSearchParams()
    appendParams(query, params)
    const qs = query.toString()
    return unwrapList(await apiRequest(`/api/master-data/subjects${qs ? '?' + qs : ''}`))
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
