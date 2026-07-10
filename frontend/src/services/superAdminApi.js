import { apiRequest } from './apiClient'

export const superAdminApi = {
  getSecurityAlerts() {
    return apiRequest('/api/super-admin/security/alerts')
  },
  getSystemModules() {
    return apiRequest('/api/super-admin/system/modules')
  }
}
