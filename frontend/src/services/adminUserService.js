import { apiRequest } from './apiClient'

export const adminUserApi = {
  getUsers(params) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.role) query.append('role', params.role)
    if (params.trangThai) query.append('trangThai', params.trangThai)
    if (params.maDonVi) query.append('maDonVi', params.maDonVi)

    return apiRequest(`/api/admin/users?${query.toString()}`, { method: 'GET' })
  },

  getById(id) {
    return apiRequest(`/api/admin/users/${id}`, { method: 'GET' })
  },

  create(payload) {
    return apiRequest('/api/admin/users', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  update(id, payload) {
    return apiRequest(`/api/admin/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  lock(id) {
    return apiRequest(`/api/admin/users/${id}/lock`, { method: 'PATCH' })
  },

  unlock(id) {
    return apiRequest(`/api/admin/users/${id}/unlock`, { method: 'PATCH' })
  },

  resetPassword(id, payload) {
    return apiRequest(`/api/admin/users/${id}/reset-password`, {
      method: 'PATCH',
      body: JSON.stringify(payload),
    })
  },
}
