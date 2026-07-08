import { apiRequest } from './apiClient'

export const parentApi = {
  getDashboard() {
    return apiRequest('/api/parent/dashboard', { method: 'GET' })
  },

  getChildren() {
    return apiRequest('/api/parent/children', { method: 'GET' })
  },

  getChildDetail(childId) {
    return apiRequest(`/api/parent/children/${childId}`, { method: 'GET' })
  },

  getChildGrades(childId) {
    return apiRequest(`/api/parent/children/${childId}/grades`, { method: 'GET' })
  },

  getChildSchedule(childId) {
    return apiRequest(`/api/parent/children/${childId}/schedule`, { method: 'GET' })
  },

  getChildAttendance(childId) {
    return apiRequest(`/api/parent/children/${childId}/attendance`, { method: 'GET' })
  },

  getChildAlerts(childId) {
    return apiRequest(`/api/parent/children/${childId}/alerts`, { method: 'GET' })
  },

  getChildTuition(childId) {
    return apiRequest(`/api/parent/children/${childId}/tuition`, { method: 'GET' })
  },

  getChildTransactions(childId) {
    return apiRequest(`/api/parent/children/${childId}/transactions`, { method: 'GET' })
  },

  getChildInvoices(childId) {
    return apiRequest(`/api/parent/children/${childId}/invoices`, { method: 'GET' })
  },

  makePayment(payload) {
    return apiRequest('/api/parent/payment', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getNotifications() {
    return apiRequest('/api/parent/notifications', { method: 'GET' })
  },

  getNotificationHistory() {
    return apiRequest('/api/parent/notifications/history', { method: 'GET' })
  },

  getProfile() {
    return apiRequest('/api/parent/profile', { method: 'GET' })
  },

  getAccessRights() {
    return apiRequest('/api/parent/access-rights', { method: 'GET' })
  },
}
