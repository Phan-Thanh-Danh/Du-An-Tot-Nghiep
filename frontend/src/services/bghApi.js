import { apiRequest } from './apiClient'

export const bghApi = {
  getDashboard() {
    return apiRequest('/api/bgh/dashboard')
  },

  getUsers(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/admin/users${qs ? '?' + qs : ''}`)
  },

  getOrganizations() {
    return apiRequest('/api/organizations')
  },

  getOrganizationsTree() {
    return apiRequest('/api/organizations/tree')
  },

  getRoles() {
    return apiRequest('/api/admin/rbac/roles')
  },

  getAuditLogs(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/audit-logs${qs ? '?' + qs : ''}`)
  },

  getAcademicTerms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/master-data/academic-terms${qs ? '?' + qs : ''}`)
  },

  getSubjects(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/master-data/subjects${qs ? '?' + qs : ''}`)
  },

  getPrograms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/master-data/training-programs${qs ? '?' + qs : ''}`)
  },

  getReportOverview() {
    return apiRequest('/api/admin/applications/reports/overview')
  },

  getPendingSchedules(params = {}) {
    const query = new URLSearchParams()
    if (params.status) query.append('status', params.status)
    const qs = query.toString()
    return apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`)
  },

  getEvaluations(params = {}) {
    const query = new URLSearchParams()
    if (params.teacherId) query.append('teacherId', params.teacherId)
    const qs = query.toString()
    return apiRequest(`/api/bgh/evaluations${qs ? '?' + qs : ''}`)
  },
}
