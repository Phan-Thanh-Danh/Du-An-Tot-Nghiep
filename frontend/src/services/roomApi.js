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

export const roomApi = {
  async list(params = {}) {
    const query = new URLSearchParams()
    appendParams(query, params)
    const qs = query.toString()
    return unwrapList(await apiRequest(`/api/master-data/rooms${qs ? '?' + qs : ''}`))
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
