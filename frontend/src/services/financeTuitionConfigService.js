import { apiRequest } from '@/services/apiClient'

const BASE_PATH = '/api/finance/program-tuition-configs'

function unwrap(response) {
  return response?.data ?? response
}

function buildQuery(params = {}) {
  const query = new URLSearchParams()

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    query.set(key, value)
  })

  const queryString = query.toString()
  return queryString ? `?${queryString}` : ''
}

export async function getTuitionConfigs(params = {}) {
  return unwrap(
    await apiRequest(`${BASE_PATH}${buildQuery(params)}`, {
      method: 'GET',
    }),
  )
}

export async function getTuitionConfigOptions() {
  return unwrap(
    await apiRequest(`${BASE_PATH}/options`, {
      method: 'GET',
    }),
  )
}

export async function getTuitionConfig(id) {
  return unwrap(
    await apiRequest(`${BASE_PATH}/${id}`, {
      method: 'GET',
    }),
  )
}

export async function createTuitionConfig(payload) {
  return unwrap(
    await apiRequest(BASE_PATH, {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  )
}

export async function bulkCreateTuitionConfigs(payload) {
  return unwrap(
    await apiRequest(`${BASE_PATH}/bulk`, {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
  )
}

export async function updateTuitionConfig(id, payload) {
  return unwrap(
    await apiRequest(`${BASE_PATH}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }),
  )
}

export async function deactivateTuitionConfig(id) {
  return unwrap(
    await apiRequest(`${BASE_PATH}/${id}/deactivate`, {
      method: 'PATCH',
    }),
  )
}

export const financeTuitionConfigService = {
  getTuitionConfigs,
  getTuitionConfigOptions,
  getTuitionConfig,
  createTuitionConfig,
  bulkCreateTuitionConfigs,
  updateTuitionConfig,
  deactivateTuitionConfig,
}
