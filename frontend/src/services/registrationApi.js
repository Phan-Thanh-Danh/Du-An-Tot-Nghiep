import { apiRequest, unwrapApiData } from './apiClient'

function withQuery(path, params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.append(key, value)
  })
  const qs = query.toString()
  return `${path}${qs ? `?${qs}` : ''}`
}

export const registrationApi = {
  async getPeriods() {
    return unwrapApiData(await apiRequest('/api/admin/registration-periods'))
  },

  async createPeriod(payload) {
    return unwrapApiData(await apiRequest('/api/admin/registration-periods', {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  async updatePeriod(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/registration-periods/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }))
  },

  async openPeriod(id) {
    return unwrapApiData(await apiRequest(`/api/admin/registration-periods/${id}/open`, {
      method: 'POST',
      body: JSON.stringify({}),
    }))
  },

  async closePeriod(id) {
    return unwrapApiData(await apiRequest(`/api/admin/registration-periods/${id}/close`, {
      method: 'POST',
      body: JSON.stringify({}),
    }))
  },

  async deletePeriod(id) {
    return unwrapApiData(await apiRequest(`/api/admin/registration-periods/${id}`, {
      method: 'DELETE',
    }))
  },

  async getRegistrationResults(periodId = null) {
    const path = periodId
      ? `/api/admin/registration-periods/${periodId}/registrations`
      : '/api/admin/registrations'
    return unwrapApiData(await apiRequest(path))
  },

  async getCapacity(params = {}) {
    return unwrapApiData(await apiRequest(withQuery('/api/admin/course-sections/capacity', params)))
  },

  async updateCapacity(sectionId, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/course-sections/${sectionId}/capacity`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }))
  },

  async cancelSection(sectionId, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/course-sections/${sectionId}/cancel`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  async reopenSection(sectionId, payload = {}) {
    return unwrapApiData(await apiRequest(`/api/admin/course-sections/${sectionId}/reopen`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }))
  },

  async getAvailableSections() {
    return unwrapApiData(await apiRequest('/api/student/registrations/available'))
  },

  async getMyRegistrations() {
    return unwrapApiData(await apiRequest('/api/student/registrations'))
  },

  async enroll(sectionId) {
    return unwrapApiData(await apiRequest('/api/student/registrations', {
      method: 'POST',
      body: JSON.stringify({ maLopHocPhan: sectionId }),
    }))
  },

  async withdraw(registrationId) {
    return unwrapApiData(await apiRequest(`/api/student/registrations/${registrationId}/withdraw`, {
      method: 'POST',
      body: JSON.stringify({}),
    }))
  },
}
