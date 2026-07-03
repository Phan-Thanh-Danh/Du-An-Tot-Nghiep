import { apiRequest } from './apiClient'
import { mockBuildings } from './mockFacilitiesData'

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

export const buildingApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/master-data/buildings${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockBuildings]
        if (params.MaDonVi) result = result.filter(b => b.maDonVi === Number(params.MaDonVi))
        if (params.Keyword) {
          const kw = params.Keyword.toLowerCase()
          result = result.filter(b => b.tenToaNha.toLowerCase().includes(kw) || b.maCodeToaNha.toLowerCase().includes(kw))
        }
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/buildings/${id}`),
      () => mockBuildings.find(b => b.maToaNha === Number(id)) || mockBuildings[0]
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/master-data/buildings', { method: 'POST', body: JSON.stringify(data) }),
      () => ({ success: true, maToaNha: Date.now() })
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/master-data/buildings/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => ({ success: true })
    )
  },

  delete(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/buildings/${id}`, { method: 'DELETE' }),
      () => ({ success: true })
    )
  },
}
