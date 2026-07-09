import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    const normalizedKey = key === 'PageSize' ? 'pageSize' : key === 'PageIndex' ? 'pageIndex' : key
    const normalizedValue = normalizedKey === 'pageSize' ? Math.min(Number(value) || 20, 100) : value
    query.set(normalizedKey, normalizedValue)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

export const academicTermApi = {
  async list(params = {}) {
    return unwrapList(await apiRequest(`/api/master-data/academic-terms${buildQuery(params)}`))
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
