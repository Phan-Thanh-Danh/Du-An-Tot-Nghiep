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
    return apiRequest(`/api/bgh/users${qs ? '?' + qs : ''}`)
  },

  getOrganizations() {
    return apiRequest('/api/organizations')
  },

  getOrganizationsTree() {
    return apiRequest('/api/organizations/tree')
  },

  getRoles() {
    return apiRequest('/api/bgh/rbac/roles')
  },

  getAuditLogs(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return apiRequest(`/api/bgh/audit-logs${qs ? '?' + qs : ''}`)
  },

  getAcademicTerms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/bgh/master-data/academic-terms${qs ? '?' + qs : ''}`)
  },

  getSubjects(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/bgh/master-data/subjects${qs ? '?' + qs : ''}`)
  },

  getPrograms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return apiRequest(`/api/bgh/master-data/training-programs${qs ? '?' + qs : ''}`)
  },

  getAcademicOverview() {
    return apiRequest('/api/bgh/academic/overview')
  },

  getGpaReports() {
    return apiRequest('/api/bgh/academic/gpa')
  },

  getAtRiskStudents() {
    return apiRequest('/api/bgh/academic/at-risk')
  },

  getAcademicReports() {
    return apiRequest('/api/bgh/academic/reports')
  },

  getPassFailRates() {
    return apiRequest('/api/bgh/academic/pass-fail')
  },

  getScheduleChanges() {
    return apiRequest('/api/bgh/schedule/changes')
  },

  getPendingSchedules(params = {}) {
    const query = new URLSearchParams()
    if (params.status) query.append('status', params.status)
    const qs = query.toString()
    return apiRequest(`/api/bgh/schedules${qs ? '?' + qs : ''}`)
  },

  getEvaluations(params = {}) {
    const query = new URLSearchParams()
    if (params.teacherId) query.append('teacherId', params.teacherId)
    const qs = query.toString()
    return apiRequest(`/api/bgh/evaluations${qs ? '?' + qs : ''}`)
  },

  getEvaluationRanking() {
    return apiRequest('/api/bgh/evaluations/ranking')
  },

  getEvaluationDetail(teacherId) {
    return apiRequest(`/api/bgh/evaluations/${teacherId}`)
  },

  getEvaluationOverview() {
    return apiRequest('/api/bgh/evaluations/overview')
  },

  getEvaluationAiAnalysis() {
    return apiRequest('/api/bgh/evaluations/ai-analysis')
  },
}
