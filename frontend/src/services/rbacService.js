import { apiRequest } from './apiClient'

export const rbacApi = {
  getRoles() {
    return apiRequest('/api/admin/rbac/roles', { method: 'GET' })
  },

  getRoleById(id) {
    return apiRequest(`/api/admin/rbac/roles/${id}`, { method: 'GET' })
  },

  createRole(payload) {
    return apiRequest('/api/admin/rbac/roles', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateRole(id, payload) {
    return apiRequest(`/api/admin/rbac/roles/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  deleteRole(id) {
    return apiRequest(`/api/admin/rbac/roles/${id}`, { method: 'DELETE' })
  },

  getUserRoles(userId) {
    return apiRequest(`/api/admin/rbac/users/${userId}/roles`, { method: 'GET' })
  },

  assignUserRoles(userId, payload) {
    return apiRequest(`/api/admin/rbac/users/${userId}/roles`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },
}
