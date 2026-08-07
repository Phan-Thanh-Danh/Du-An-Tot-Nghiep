import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

function unwrapPagedResult(response) {
  const body = response?.data ?? response?.Data ?? response
  if (!body) return { items: [], totalItems: 0, pageIndex: 1, pageSize: 20 }
  return {
    items: Array.isArray(body.items)
      ? body.items
      : Array.isArray(body.Items)
      ? body.Items
      : [],
    totalItems: body.totalItems ?? body.TotalItems ?? 0,
    pageIndex: body.pageIndex ?? body.PageIndex ?? 1,
    pageSize: body.pageSize ?? body.PageSize ?? 20,
  }
}

export const cohortApi = {
  list(params = {}) {
    return apiRequest(`/api/master-data/cohorts${buildQuery(params)}`)
  },

  get(id) {
    return apiRequest(`/api/master-data/cohorts/${id}`)
  },

  create(data) {
    return apiRequest('/api/master-data/cohorts', {
      method: 'POST',
      body: JSON.stringify(data),
    })
  },

  update(id, data) {
    return apiRequest(`/api/master-data/cohorts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    })
  },

  activate(id) {
    return apiRequest(`/api/master-data/cohorts/${id}/activate`, {
      method: 'PATCH',
    })
  },

  deactivate(id) {
    return apiRequest(`/api/master-data/cohorts/${id}/deactivate`, {
      method: 'PATCH',
    })
  },

  remove(id) {
    return apiRequest(`/api/master-data/cohorts/${id}`, {
      method: 'DELETE',
    })
  },

  unwrapPagedResult,
}
