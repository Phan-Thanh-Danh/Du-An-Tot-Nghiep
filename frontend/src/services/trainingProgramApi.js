import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value === null || value === undefined || value === '') return
    query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

export const trainingProgramApi = {
  list(params = {}) {
    return apiRequest(`/api/master-data/training-programs${buildQuery(params)}`)
  },

  getById(id) {
    return apiRequest(`/api/master-data/training-programs/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/training-programs', {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/training-programs/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data)
    })
  },

  clone(id, data) {
    return apiRequest(`/api/master-data/training-programs/${id}/clone`, {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },

  delete(id) {
    return apiRequest(`/api/master-data/training-programs/${id}`, {
      method: 'DELETE'
    })
  },

  activate(id) {
    return apiRequest(`/api/master-data/training-programs/${id}/activate`, {
      method: 'PATCH'
    })
  },

  deactivate(id) {
    return apiRequest(`/api/master-data/training-programs/${id}/deactivate`, {
      method: 'PATCH'
    })
  },

  archive(id) {
    return apiRequest(`/api/master-data/training-programs/${id}/archive`, {
      method: 'PATCH'
    })
  },

  getCurriculum(id) {
    return apiRequest(`/api/master-data/training-programs/${id}/curriculum`)
  },

  addSubject(id, data) {
    return apiRequest(`/api/master-data/training-programs/${id}/subjects`, {
      method: 'POST',
      body: JSON.stringify(data)
    })
  },

  updateSubject(id, subjectId, data) {
    return apiRequest(`/api/master-data/training-programs/${id}/subjects/${subjectId}`, {
      method: 'PUT',
      body: JSON.stringify(data)
    })
  },

  removeSubject(id, subjectId) {
    return apiRequest(`/api/master-data/training-programs/${id}/subjects/${subjectId}`, {
      method: 'DELETE'
    })
  },

  compare(sourceId, targetId) {
    return apiRequest(`/api/master-data/training-programs/compare?sourceId=${sourceId}&targetId=${targetId}`)
  },

  assign(id, data) {
    return apiRequest(`/api/master-data/training-programs/${id}/assign`, {
      method: 'POST',
      body: JSON.stringify(data)
    })
  }
}
