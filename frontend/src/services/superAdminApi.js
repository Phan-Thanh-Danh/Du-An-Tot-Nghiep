/**
 * superAdminApi.js
 * Module API tập trung cho Super Admin.
 * Mọi call API của Super Admin nên đi qua đây, không rải rác trong component.
 */
import { apiRequest } from './apiClient'

export const superAdminApi = {
  // Dashboard
  getDashboardStats() {
    return apiRequest('/api/super-admin/dashboard/stats')
  },
  getRecentActivities(limit = 10) {
    return apiRequest(`/api/super-admin/dashboard/activities?limit=${limit}`)
  },

  // Bảo mật
  getSecurityAlerts() {
    return apiRequest('/api/super-admin/security/alerts')
  },
  getLoginHistory(limit = 100) {
    return apiRequest(`/api/super-admin/login-history?limit=${limit}`)
  },

  // Hệ thống
  getSystemModules() {
    return apiRequest('/api/super-admin/system/modules')
  },

  // AI & Automation
  getAiAutomationStats() {
    return apiRequest('/api/super-admin/ai/automation-stats')
  },
  getAiJobs() {
    return apiRequest('/api/super-admin/ai/jobs')
  },
  getAiModels() {
    return apiRequest('/api/super-admin/ai/models')
  },
}
