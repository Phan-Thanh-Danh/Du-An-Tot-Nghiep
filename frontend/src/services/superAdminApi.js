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

  // Export Data
  createExportRequest(payload) {
    return apiRequest('/api/super-admin/exports', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  getExportHistory() {
    return apiRequest('/api/super-admin/exports')
  },
  getExportDownloadUrl(requestId) {
    return `/api/super-admin/exports/download/${requestId}`
  },
  async downloadExportFile(requestId, filename) {
    const { getStoredAccessToken } = await import('./apiClient')
    const token = getStoredAccessToken()
    const url = `/api/super-admin/exports/download/${requestId}`
    const fullUrl = import.meta.env.VITE_API_BASE_URL ? `${import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '')}${url}` : url

    const response = await fetch(fullUrl, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    })

    if (!response.ok) {
      throw new Error(`Tải file thất bại (HTTP ${response.status})`)
    }

    const blob = await response.blob()
    const downloadUrl = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = downloadUrl
    a.download = filename || `Export_${requestId}.xlsx`
    document.body.appendChild(a)
    a.click()
    a.remove()
    window.URL.revokeObjectURL(downloadUrl)
  }
}
