import { apiRequest } from './apiClient'
import { mockFloors } from './mockFacilitiesData'

const enableMock = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch (error) {
    if (!enableMock) throw error
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const floorApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/master-data/floors${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockFloors]
        if (params.MaToaNha) result = result.filter(f => f.maToaNha === Number(params.MaToaNha))
        if (params.MaDonVi) result = result.filter(f => f.maDonVi === Number(params.MaDonVi))
        return result
      }
    )
  },

  getByBuilding(buildingId) {
    return withFallback(
      () => apiRequest(`/api/master-data/buildings/${buildingId}/floors`),
      () => mockFloors.filter(f => f.maToaNha === Number(buildingId))
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/floors/${id}`),
      () => mockFloors.find(f => f.maTang === Number(id)) || mockFloors[0]
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/master-data/floors', { method: 'POST', body: JSON.stringify(data) }),
      () => ({ success: true, maTang: Date.now() })
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/master-data/floors/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => ({ success: true })
    )
  },

  delete(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/floors/${id}`, { method: 'DELETE' }),
      () => ({ success: true })
    )
  },
}
