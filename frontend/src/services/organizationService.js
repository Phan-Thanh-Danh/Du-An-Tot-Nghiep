import { apiRequest } from './apiClient'

export const organizationApi = {
  // Lấy tất cả đơn vị
  getAll() {
    return apiRequest('/api/organizations', {
      method: 'GET',
    })
  },

  // Lấy cây tổ chức
  getTree() {
    return apiRequest('/api/organizations/tree', {
      method: 'GET',
    })
  },

  // Lấy chi tiết đơn vị
  getById(id) {
    return apiRequest(`/api/organizations/${id}`, {
      method: 'GET',
    })
  },

  // Lấy cây con của một đơn vị
  getSubtree(id) {
    return apiRequest(`/api/organizations/${id}/subtree`, {
      method: 'GET',
    })
  },

  // Tạo mới đơn vị (Yêu cầu SuperAdmin)
  create(payload) {
    return apiRequest('/api/organizations', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  // Cập nhật đơn vị (Yêu cầu SuperAdmin)
  update(id, payload) {
    return apiRequest(`/api/organizations/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  // Xóa mềm đơn vị (Yêu cầu SuperAdmin)
  delete(id) {
    return apiRequest(`/api/organizations/${id}`, {
      method: 'DELETE',
    })
  },

  // Xóa cứng đơn vị (Yêu cầu SuperAdmin)
  hardDelete(id) {
    return apiRequest(`/api/organizations/${id}/hard-delete`, {
      method: 'DELETE',
    })
  },
}
