import { apiRequest } from './apiClient'

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

export const blockApi = {
  async getByTerm(maHocKy) {
    return unwrapList(await apiRequest(`/api/blocks?maHocKy=${maHocKy}`))
  },

  update(id, data) {
    return apiRequest(`/api/blocks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  }
}
