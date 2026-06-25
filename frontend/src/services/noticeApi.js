import { apiRequest } from './apiClient'

export const noticeApi = {
  send(payload) {
    return apiRequest('/api/admin/notifications', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getHistory(params = {}) {
    const query = new URLSearchParams(params).toString()
    return apiRequest(`/api/admin/notifications${query ? '?' + query : ''}`)
  },

  getDetail(id) {
    return apiRequest(`/api/admin/notifications/${id}`)
  },

}
