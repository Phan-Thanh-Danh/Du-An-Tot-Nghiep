import { apiRequest } from './apiClient'
import { mockRooms } from './mockFacilitiesData'

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

export const roomApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/master-data/rooms${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockRooms]
        if (params.BuildingId) result = result.filter(r => r.maToaNha === Number(params.BuildingId))
        if (params.FloorId) result = result.filter(r => r.maTang === Number(params.FloorId))
        if (params.MaDonVi) result = result.filter(r => r.maDonVi === Number(params.MaDonVi))
        if (params.LoaiPhong) result = result.filter(r => r.loaiPhong === params.LoaiPhong)
        if (params.TrangThaiPhong) result = result.filter(r => r.trangThaiPhong === params.TrangThaiPhong)
        return result
      }
    )
  },

  getByFloor(floorId) {
    return withFallback(
      () => apiRequest(`/api/master-data/floors/${floorId}/rooms`),
      () => mockRooms.filter(r => r.maTang === Number(floorId))
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/rooms/${id}`),
      () => mockRooms.find(r => r.maPhong === Number(id)) || mockRooms[0]
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/master-data/rooms', { method: 'POST', body: JSON.stringify(data) }),
      () => ({ success: true, maPhong: Date.now() })
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/master-data/rooms/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => ({ success: true })
    )
  },

  delete(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/rooms/${id}`, { method: 'DELETE' }),
      () => ({ success: true })
    )
  },
}
