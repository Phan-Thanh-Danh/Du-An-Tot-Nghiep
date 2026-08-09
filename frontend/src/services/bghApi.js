import { bghDataClient } from '@/components/BGH/performance/bghDataClient'
import { apiRequest } from '@/services/apiClient'

const SHORT = { freshMs: 15_000, staleMs: 60_000 }
const REPORT = { freshMs: 60_000, staleMs: 300_000 }
const MASTER = { freshMs: 300_000, staleMs: 900_000 }

function get(path, policy = SHORT, options = {}) {
  return bghDataClient.get(path, { ...policy, ...options })
}

export const bghApi = {
  getDashboard() {
    return get('/api/bgh/dashboard')
  },

  getUsers(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.role) query.append('role', params.role)
    if (params.status) query.append('status', params.status)
    const qs = query.toString()
    return get(`/api/bgh/users${qs ? '?' + qs : ''}`)
  },

  getOrganizations() {
    return get('/api/organizations', MASTER)
  },

  getOrganizationsTree() {
    return get('/api/organizations/tree', MASTER)
  },

  getRoles() {
    return get('/api/bgh/rbac/roles', MASTER)
  },

  getAuditLogs(params = {}) {
    const query = new URLSearchParams()
    const filterKeys = ['pageIndex', 'pageSize', 'keyword', 'entityType', 'action', 'fromDate', 'toDate']
    filterKeys.forEach((key) => {
      if (params[key] !== undefined && params[key] !== null && params[key] !== '') query.append(key, params[key])
    })
    const qs = query.toString()
    return get(`/api/bgh/audit-logs${qs ? '?' + qs : ''}`)
  },

  getAcademicTerms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return get(`/api/bgh/master-data/academic-terms${qs ? '?' + qs : ''}`, MASTER)
  },

  getCohorts() {
    return get('/api/bgh/master-data/cohorts', MASTER)
  },

  getBuildings() {
    return get('/api/bgh/master-data/buildings', MASTER)
  },

  getFloors() {
    return get('/api/bgh/master-data/floors', MASTER)
  },

  getRooms() {
    return get('/api/bgh/master-data/rooms', MASTER)
  },

  getSubjects(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return get(`/api/bgh/master-data/subjects${qs ? '?' + qs : ''}`, MASTER)
  },

  getPrograms(params = {}) {
    const query = new URLSearchParams()
    if (params.keyword) query.append('keyword', params.keyword)
    const qs = query.toString()
    return get(`/api/bgh/master-data/training-programs${qs ? '?' + qs : ''}`, MASTER)
  },

  getProgramCurriculum(programId) {
    return get(`/api/bgh/master-data/training-programs/${programId}/curriculum`, MASTER)
  },

  getAcademicOverview(params = {}) {
    const query = new URLSearchParams()
    if (params.campusId) query.append('campusId', params.campusId)
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.specializationId) query.append('specializationId', params.specializationId)
    const qs = query.toString()
    return get(`/api/bgh/academic/overview${qs ? '?' + qs : ''}`, REPORT)
  },

  getGpaReports(params = {}) {
    const query = new URLSearchParams()
    if (params.campusId) query.append('campusId', params.campusId)
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.specializationId) query.append('specializationId', params.specializationId)
    const qs = query.toString()
    return get(`/api/bgh/academic/gpa${qs ? '?' + qs : ''}`, REPORT)
  },

  getAtRiskStudents(params = {}, options = {}) {
    const query = new URLSearchParams()
    const filterKeys = ['pageIndex', 'pageSize', 'studentId', 'semesterId', 'keyword']
    filterKeys.forEach((key) => {
      if (params[key] !== undefined && params[key] !== null && params[key] !== '') query.append(key, params[key])
    })
    const qs = query.toString()
    return get(`/api/bgh/academic/at-risk${qs ? '?' + qs : ''}`, SHORT, options)
  },

  getAtRiskStudentHistory(studentId) {
    return get(`/api/bgh/academic/at-risk/${studentId}/history`, REPORT)
  },

  getAcademicReports(params = {}) {
    const query = new URLSearchParams()
    if (params.campusId) query.append('campusId', params.campusId)
    if (params.semesterId) query.append('semesterId', params.semesterId)
    if (params.reportType) query.append('reportType', params.reportType)
    const qs = query.toString()
    return get(`/api/bgh/academic/reports${qs ? '?' + qs : ''}`, REPORT)
  },

  getPassFailRates(params = {}) {
    const query = new URLSearchParams()
    const filterKeys = ['majorId', 'specializationId', 'programSubjectId', 'semesterId']
    filterKeys.forEach((key) => {
      if (params[key] !== undefined && params[key] !== null && params[key] !== '') {
        query.append(key, params[key])
      }
    })
    const qs = query.toString()
    return get(`/api/bgh/academic/pass-fail${qs ? '?' + qs : ''}`, REPORT)
  },

  getPassFailFilterOptions(params = {}) {
    const query = new URLSearchParams()
    const filterKeys = ['majorId', 'specializationId', 'programSubjectId']
    filterKeys.forEach((key) => {
      if (params[key] !== undefined && params[key] !== null && params[key] !== '') {
        query.append(key, params[key])
      }
    })
    const qs = query.toString()
    return get(`/api/bgh/academic/pass-fail/filters${qs ? '?' + qs : ''}`, MASTER)
  },

  getScheduleChanges() {
    return get('/api/bgh/schedule/changes', SHORT)
  },

  getPendingSchedules(params = {}) {
    const query = new URLSearchParams()
    if (params.status) query.append('status', params.status)
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    const qs = query.toString()
    return get(`/api/bgh/schedules${qs ? '?' + qs : ''}`, SHORT)
  },

  async approveSchedule(scheduleId) {
    const response = await apiRequest(`/api/bgh/schedules/${scheduleId}/approve`, { method: 'POST' })
    bghDataClient.invalidate('/api/bgh/schedules')
    bghDataClient.invalidate('/api/bgh/dashboard')
    return response
  },

  async rejectSchedule(scheduleId) {
    const response = await apiRequest(`/api/bgh/schedules/${scheduleId}/reject`, { method: 'POST' })
    bghDataClient.invalidate('/api/bgh/schedules')
    bghDataClient.invalidate('/api/bgh/dashboard')
    return response
  },

  async resolveScheduleConflict(conflictId) {
    return apiRequest(`/api/bgh/schedule/conflicts/${conflictId}/resolve`, { method: 'POST' })
  },

  async approveScheduleChange(changeId) {
    const response = await apiRequest(`/api/bgh/schedule/changes/${changeId}/approve`, { method: 'POST' })
    bghDataClient.invalidate('/api/bgh/schedule/changes')
    return response
  },

  async rejectScheduleChange(changeId) {
    const response = await apiRequest(`/api/bgh/schedule/changes/${changeId}/reject`, { method: 'POST' })
    bghDataClient.invalidate('/api/bgh/schedule/changes')
    return response
  },

  getEvaluations(params = {}) {
    const query = new URLSearchParams()
    if (params.teacherId) query.append('teacherId', params.teacherId)
    const qs = query.toString()
    return get(`/api/bgh/evaluations${qs ? '?' + qs : ''}`, REPORT)
  },

  getEvaluationRanking() {
    return get('/api/bgh/evaluations/ranking', REPORT)
  },

  getEvaluationDetail(teacherId) {
    return get(`/api/bgh/evaluations/${teacherId}`, REPORT)
  },

  getEvaluationOverview() {
    return get('/api/bgh/evaluations/overview', REPORT)
  },

  getEvaluationAiAnalysis() {
    return get('/api/bgh/evaluations/ai-analysis', REPORT)
  },

  prefetch(path, policy = SHORT) {
    return bghDataClient.prefetch(path, policy)
  },

  invalidate(pathPrefix = '') {
    bghDataClient.invalidate(pathPrefix)
  },

  abortScope(scope) {
    bghDataClient.abortScope(scope)
  },

  clearCache() {
    bghDataClient.clear()
  },

  getPerformanceMetrics() {
    return bghDataClient.getMetrics()
  },
}
