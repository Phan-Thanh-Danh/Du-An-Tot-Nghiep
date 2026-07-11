import { apiRequest } from './apiClient'

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

export const creditMappingApi = {
  async list() {
    return unwrapList(await apiRequest('/api/quy-doi-tin-chi'))
  },

  create(data) {
    return apiRequest('/api/quy-doi-tin-chi', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  update(id, data) {
    return apiRequest(`/api/quy-doi-tin-chi/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  },

  delete(id) {
    return apiRequest(`/api/quy-doi-tin-chi/${id}`, { method: 'DELETE' })
  },
}
