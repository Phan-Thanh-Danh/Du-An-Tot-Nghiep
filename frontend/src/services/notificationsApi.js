import { apiRequest } from './apiClient'

// Helper to convert object to query string
const buildQuery = (params) => {
  if (!params) return ''
  const searchParams = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      searchParams.append(key, value)
    }
  })
  const qs = searchParams.toString()
  return qs ? `?${qs}` : ''
}

export const notificationsApi = {
  // ── User Notifications ──────────────────────────────────────────────
  async getMyNotifications(query = {}) {
    const response = await apiRequest(`/api/notifications${buildQuery(query)}`)
    return response.data // PagedResultDto<NotificationDto>
  },
  async getNotificationDetail(id) {
    const response = await apiRequest(`/api/notifications/${id}`)
    return response.data // NotificationDetailDto
  },
  async getUnreadCount() {
    const response = await apiRequest('/api/notifications/unread-count')
    return response.data // UnreadCountDto
  },
  async markAsRead(id) {
    const response = await apiRequest(`/api/notifications/${id}/read`, {
      method: 'PATCH',
    })
    return response.data // NotificationDto
  },
  async markAllAsRead() {
    const response = await apiRequest('/api/notifications/read-all', {
      method: 'PATCH',
    })
    return response.data // UnreadCountDto
  },
  async hideNotification(id) {
    const response = await apiRequest(`/api/notifications/${id}`, {
      method: 'DELETE',
    })
    return response.success
  },

  // ── Admin Notifications ─────────────────────────────────────────────
  async getAdminNotifications(query = {}) {
    const response = await apiRequest(`/api/admin/notifications${buildQuery(query)}`)
    return response.data // PagedResultDto<AdminNotificationDto>
  },
  async getAdminNotificationDetail(id) {
    const response = await apiRequest(`/api/admin/notifications/${id}`)
    return response.data // AdminNotificationDto
  },
  async previewRecipients(payload) {
    const response = await apiRequest('/api/admin/notifications/preview-recipients', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // NotificationRecipientPreviewResultDto
  },
  async createNotification(payload) {
    const response = await apiRequest('/api/admin/notifications', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // AdminNotificationDto
  },
  async cancelNotification(id) {
    const response = await apiRequest(`/api/admin/notifications/${id}/cancel`, {
      method: 'PATCH',
    })
    return response.data // AdminNotificationDto
  },
  async getNotificationRecipients(id, query = {}) {
    const response = await apiRequest(`/api/admin/notifications/${id}/recipients${buildQuery(query)}`)
    return response.data // PagedResultDto<AdminNotificationRecipientDto>
  },
  async getNotificationStatistics(id) {
    const response = await apiRequest(`/api/admin/notifications/${id}/statistics`)
    return response.data // NotificationStatisticsDto
  },

  // ── Admin Notification Templates ────────────────────────────────────
  async getNotificationTemplates(query = {}) {
    const response = await apiRequest(`/api/admin/notification-templates${buildQuery(query)}`)
    return response.data // PagedResultDto<NotificationTemplateListItemDto>
  },
  async getNotificationTemplateDetail(id) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}`)
    return response.data // NotificationTemplateDetailDto
  },
  async createNotificationTemplate(payload) {
    const response = await apiRequest('/api/admin/notification-templates', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // NotificationTemplateDetailDto
  },
  async updateNotificationTemplate(id, payload) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
    return response.data // NotificationTemplateDetailDto
  },
  async activateTemplate(id) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}/activate`, {
      method: 'POST',
    })
    return response.success
  },
  async deactivateTemplate(id, payload = {}) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}/deactivate`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.success
  },
  async deleteTemplate(id) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}`, {
      method: 'DELETE',
    })
    return response.success
  },
  async previewTemplate(id, payload) {
    const response = await apiRequest(`/api/admin/notification-templates/${id}/preview`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // PreviewNotificationTemplateResultDto
  },

  // ── Admin Specialized Notifications ─────────────────────────────────
  async getSpecializedCategories() {
    const response = await apiRequest('/api/admin/specialized-notifications/categories')
    return response.data // SpecializedNotificationCategoryDto
  },
  async previewSpecializedRecipients(payload) {
    const response = await apiRequest('/api/admin/specialized-notifications/preview-recipients', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // PreviewSpecializedRecipientsResultDto
  },
  async sendTuitionNotification(payload) {
    const response = await apiRequest('/api/admin/specialized-notifications/tuition', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // SpecializedNotificationSendResultDto
  },
  async sendAcademicNotification(payload) {
    const response = await apiRequest('/api/admin/specialized-notifications/academic', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // SpecializedNotificationSendResultDto
  },
  async sendUrgentNotification(payload) {
    const response = await apiRequest('/api/admin/specialized-notifications/urgent', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // SpecializedNotificationSendResultDto
  },
  async sendMaintenanceNotification(payload) {
    const response = await apiRequest('/api/admin/specialized-notifications/maintenance', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data // SpecializedNotificationSendResultDto
  },
}
