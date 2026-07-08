import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const entries = Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== '')
  if (!entries.length) return ''
  return `?${entries.map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value)}`).join('&')}`
}

export const courseApi = {
  getCourses(params = {}) {
    return apiRequest(`/api/courses${buildQuery(params)}`)
  },

  getCourseDetail(id) {
    return apiRequest(`/api/courses/${id}`)
  },

  createCourse(payload) {
    return apiRequest('/api/courses', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  bulkAssign(payload) {
    return apiRequest('/api/courses/bulk-assign', {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  updateCourse(id, payload) {
    return apiRequest(`/api/courses/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    })
  },

  archiveCourse(id) {
    return apiRequest(`/api/courses/${id}`, { method: 'DELETE' })
  },

  cloneCourse(id, payload = {}) {
    return apiRequest(`/api/courses/${id}/clone`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  batchArchive(ids) {
    return apiRequest('/api/courses/batch-archive', {
      method: 'POST',
      body: JSON.stringify({ ids }),
    })
  },

  batchPublish(ids) {
    return apiRequest('/api/courses/batch-publish', {
      method: 'POST',
      body: JSON.stringify({ ids }),
    })
  },
}
