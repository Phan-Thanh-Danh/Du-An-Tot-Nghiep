import { apiRequest } from './apiClient'

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

export const applicationsApi = {
  // ── Schema/Template ──────────────────────────────────────────────
  async getApplicationSchemaOptions() {
    const response = await apiRequest('/api/application-schema/options')
    return response.data
  },
  async getApplicationTemplates(query = {}) {
    const response = await apiRequest(`/api/application-schema/templates${buildQuery(query)}`)
    return response.data
  },
  async getApplicationTemplateDetail(id) {
    const response = await apiRequest(`/api/application-schema/templates/${id}`)
    return response.data
  },

  // ── Student ──────────────────────────────────────────────────────
  async getMyApplications(query = {}) {
    const response = await apiRequest(`/api/student/applications${buildQuery(query)}`)
    return response.data
  },
  async getMyApplicationDetail(id) {
    const response = await apiRequest(`/api/student/applications/${id}`)
    return response.data
  },
  async createDraft(payload) {
    const response = await apiRequest('/api/student/applications/drafts', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async updateDraft(id, payload) {
    const response = await apiRequest(`/api/student/applications/drafts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async submitApplication(id, payload = {}) {
    const response = await apiRequest(`/api/student/applications/${id}/submit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async resubmitApplication(id, payload = {}) {
    const response = await apiRequest(`/api/student/applications/${id}/resubmit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async cancelApplication(id, payload = {}) {
    const response = await apiRequest(`/api/student/applications/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async uploadEvidence(applicationId, file, metadata = {}) {
    const formData = new FormData()
    formData.append('file', file)
    if (metadata.description) formData.append('description', metadata.description)

    const token = localStorage.getItem('lms_access_token') || sessionStorage.getItem('lms_access_token')
    const res = await fetch(`${import.meta.env.VITE_API_BASE_URL || ''}/api/student/applications/${applicationId}/evidence`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` },
      body: formData
    })
    const data = await res.json()
    if (!res.ok) throw new Error(data?.message || 'Upload failed')
    return data.data
  },
  async downloadEvidence(applicationId, evidenceId) {
    // Return URL for download or handle blob
    return `/api/student/applications/${applicationId}/evidence/${evidenceId}/download`
  },
  async deleteEvidence(applicationId, evidenceId) {
    const response = await apiRequest(`/api/student/applications/${applicationId}/evidence/${evidenceId}`, {
      method: 'DELETE',
    })
    return response.success
  },

  // ── Admin Queue & Assignment ─────────────────────────────────────
  async getAdminApplications(query = {}) {
    const response = await apiRequest(`/api/admin/applications${buildQuery(query)}`)
    return response.data
  },
  async getAdminApplicationSummary(query = {}) {
    const response = await apiRequest(`/api/admin/applications/summary${buildQuery(query)}`)
    return response.data
  },
  async getAdminApplicationDetail(id) {
    const response = await apiRequest(`/api/admin/applications/${id}`)
    return response.data
  },
  async getAssignableUsers(query = {}) {
    const response = await apiRequest(`/api/admin/applications/assignable-users${buildQuery(query)}`)
    return response.data
  },
  async receiveApplication(id, payload = {}) {
    const response = await apiRequest(`/api/admin/applications/${id}/receive`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async assignApplication(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/assign`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async reassignApplication(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/reassign`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async downloadAdminEvidence(applicationId, evidenceId) {
    return `/api/admin/applications/${applicationId}/evidence/${evidenceId}/download`
  },

  // ── Admin Decision ───────────────────────────────────────────────
  async requestSupplement(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/request-supplement`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async approveApplication(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/approve`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async rejectApplication(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/reject`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },

  // ── Post-Approval Processing ─────────────────────────────────────
  async processApprovedApplication(id, payload = {}) {
    const response = await apiRequest(`/api/admin/applications/${id}/process`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },
  async recordProcessingResult(id, payload) {
    const response = await apiRequest(`/api/admin/applications/${id}/record-result`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
    return response.data
  },

  // ── Reports ──────────────────────────────────────────────────────
  async getApplicationReportOverview(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/overview${buildQuery(query)}`)
    return response.data
  },
  async getApplicationReportByType(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/by-type${buildQuery(query)}`)
    return response.data
  },
  async getPendingApplicationReport(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/pending${buildQuery(query)}`)
    return response.data
  },
  async getOverdueApplicationReport(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/overdue${buildQuery(query)}`)
    return response.data
  },
  async getProcessingTimeReport(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/processing-time${buildQuery(query)}`)
    return response.data
  },
  async getByAssigneeReport(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/by-assignee${buildQuery(query)}`)
    return response.data
  },
  async getApplicationTrends(query = {}) {
    const response = await apiRequest(`/api/admin/application-reports/trends${buildQuery(query)}`)
    return response.data
  }
}
