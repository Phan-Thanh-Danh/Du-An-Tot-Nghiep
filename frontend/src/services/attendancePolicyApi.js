import { apiRequest } from '@/services/apiClient'

function unwrapApiData(response) {
  return response?.data ?? response?.Data ?? response
}

export const attendancePolicyApi = {
  getCurrentPolicy() {
    return apiRequest('/api/attendance-policy').then(unwrapApiData)
  },
  getPolicyHistory() {
    return apiRequest('/api/attendance-policy/history').then(unwrapApiData)
  },
  updatePolicy(payload) {
    return apiRequest('/api/attendance-policy', {
      method: 'PUT',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },

  // ===== Yêu cầu mở khóa điểm danh (admin) =====
  getUnlockRequests(params = {}) {
    const qs = new URLSearchParams(params).toString()
    return apiRequest(`/api/admin/attendance/unlock-requests${qs ? '?' + qs : ''}`).then(unwrapApiData)
  },
  approveUnlockRequest(id, payload) {
    return apiRequest(`/api/admin/attendance/unlock-requests/${id}/approve`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
  rejectUnlockRequest(id, payload) {
    return apiRequest(`/api/admin/attendance/unlock-requests/${id}/reject`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(unwrapApiData)
  },
}
