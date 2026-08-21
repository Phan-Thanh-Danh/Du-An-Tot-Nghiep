import { apiRequest } from '@/services/apiClient'

export const bghPersonnelApi = {
  getTeachers(params = {}) {
    const query = new URLSearchParams()
    if (params.pageIndex) query.append('pageIndex', params.pageIndex)
    if (params.pageSize) query.append('pageSize', params.pageSize)
    if (params.keyword) query.append('keyword', params.keyword)
    if (params.maDonVi) query.append('maDonVi', params.maDonVi)
    if (params.maChuyenNganh) query.append('maChuyenNganh', params.maChuyenNganh)
    if (params.maMonHoc) query.append('maMonHoc', params.maMonHoc)
    if (params.trangThai) query.append('trangThai', params.trangThai)
    if (params.maHocKy) query.append('maHocKy', params.maHocKy)
    const queryString = query.toString()
    return apiRequest(`/api/bgh/teacher-personnel${queryString ? `?${queryString}` : ''}`)
  },

  getTeacherDetail(id) {
    return apiRequest(`/api/bgh/teacher-personnel/${id}`)
  },

  getTeacherWorkload(id, semesterId = null) {
    const qs = semesterId ? `?semesterId=${semesterId}` : ''
    return apiRequest(`/api/bgh/teacher-personnel/${id}/workload${qs}`)
  },

  getTeacherSessionLogs(id, semesterId = null) {
    const qs = semesterId ? `?semesterId=${semesterId}` : ''
    return apiRequest(`/api/bgh/teacher-personnel/${id}/session-logs${qs}`)
  },

  getTeacherEvaluations(id) {
    return apiRequest(`/api/bgh/teacher-personnel/${id}/evaluations`)
  },

  createTeacher(payload) {
    return apiRequest('/api/bgh/teacher-personnel', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
  },

  updateTeacher(id, payload) {
    return apiRequest(`/api/bgh/teacher-personnel/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
  },

  toggleLockTeacher(id, reason = 'BGH thay đổi trạng thái') {
    return apiRequest(`/api/bgh/teacher-personnel/${id}/toggle-lock`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ lyDo: reason })
    })
  },

  getHierarchyTree() {
    return apiRequest('/api/bgh/teacher-personnel/hierarchy-tree')
  },

  importExcel(file, options = {}) {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('dryRun', String(options.dryRun ?? true))
    if (options.defaultMaDonVi) {
      formData.append('defaultMaDonVi', String(options.defaultMaDonVi))
    }
    return apiRequest('/api/bgh/teacher-personnel/import-excel', {
      method: 'POST',
      body: formData
    })
  }
}
