import { apiRequest, unwrapApiData } from './apiClient'

const APPLICATION_SCHEMA_BASE = '/api/applications/schema'

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
    return unwrapApiData(await apiRequest(`${APPLICATION_SCHEMA_BASE}/options`))
  },
  async getApplicationTemplates(query = {}) {
    return unwrapApiData(await apiRequest(`${APPLICATION_SCHEMA_BASE}/templates${buildQuery(query)}`))
  },
  async getApplicationTemplateDetail(id) {
    return unwrapApiData(await apiRequest(`${APPLICATION_SCHEMA_BASE}/templates/${id}`))
  },

  // ── Student ──────────────────────────────────────────────────────
  async getMyApplications(query = {}) {
    return unwrapApiData(await apiRequest(`/api/student/applications${buildQuery(query)}`))
  },
  async getMyApplicationDetail(id) {
    return unwrapApiData(await apiRequest(`/api/student/applications/${id}`))
  },
  async createDraft(payload) {
    return unwrapApiData(await apiRequest('/api/student/applications/drafts', {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async updateDraft(id, payload) {
    return unwrapApiData(await apiRequest(`/api/student/applications/drafts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }))
  },
  async submitApplication(id, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/student/applications/${id}/submit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async resubmitApplication(id, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/student/applications/${id}/resubmit`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async cancelApplication(id, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/student/applications/${id}/cancel`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async uploadEvidence(applicationId, file, metadata = {}) {
    const formData = new FormData()
    formData.append('file', file)
    if (metadata.description) formData.append('description', metadata.description)

    return unwrapApiData(await apiRequest(`/api/student/applications/${applicationId}/evidence`, {
      method: 'POST',
      body: formData,
    }))
  },
  async downloadEvidence(applicationId, evidenceId) {
    // Return URL for download or handle blob
    return `/api/student/applications/${applicationId}/evidence/${evidenceId}/download`
  },
  async deleteEvidence(applicationId, evidenceId) {
    const response = await apiRequest(`/api/student/applications/${applicationId}/evidence/${evidenceId}`, {
      method: 'DELETE',
    })
    return response?.success ?? response?.Success ?? true
  },

  // ── Admin Queue & Assignment ─────────────────────────────────────
  async getAdminApplications(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/applications${buildQuery(query)}`))
  },
  async getAdminApplicationSummary(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/summary${buildQuery(query)}`))
  },
  async getAdminApplicationDetail(id) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}`))
  },
  async getAssignableUsers(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/assignable-users${buildQuery(query)}`))
  },
  async receiveApplication(id, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/receive`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async assignApplication(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/assign`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async reassignApplication(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/reassign`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async downloadAdminEvidence(applicationId, evidenceId) {
    return `/api/admin/applications/${applicationId}/evidence/${evidenceId}/download`
  },

  // ── Admin Decision ───────────────────────────────────────────────
  async requestSupplement(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/request-supplement`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async approveApplication(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/approve`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async rejectApplication(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/reject`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  // ── Post-Approval Processing ─────────────────────────────────────
  async processApprovedApplication(id, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/process`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },
  async recordProcessingResult(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/${id}/record-result`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  // ── Reports ──────────────────────────────────────────────────────
  async getApplicationReportOverview(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/overview${buildQuery(query)}`))
  },
  async getApplicationReportByType(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/by-type${buildQuery(query)}`))
  },
  async getPendingApplicationReport(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/pending${buildQuery(query)}`))
  },
  async getOverdueApplicationReport(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/overdue${buildQuery(query)}`))
  },
  async getProcessingTimeReport(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/processing-time${buildQuery(query)}`))
  },
  async getByAssigneeReport(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/by-assignee${buildQuery(query)}`))
  },
  async getApplicationTrends(query = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/application-reports/trends${buildQuery(query)}`))
  },
}
