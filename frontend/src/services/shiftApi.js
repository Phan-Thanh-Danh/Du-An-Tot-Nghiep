import { apiRequest } from './apiClient'

const MOCK_SHIFTS = [
  { maCaHoc: 1, tenCa: 'Tiết 1-3', buoi: 'Sáng', gioBatDau: '07:00', gioKetThuc: '09:30', thuTu: 1, conHoatDong: true },
  { maCaHoc: 2, tenCa: 'Tiết 4-6', buoi: 'Sáng', gioBatDau: '09:45', gioKetThuc: '12:15', thuTu: 2, conHoatDong: true },
  { maCaHoc: 3, tenCa: 'Tiết 7-9', buoi: 'Chiều', gioBatDau: '13:00', gioKetThuc: '15:30', thuTu: 3, conHoatDong: true },
  { maCaHoc: 4, tenCa: 'Tiết 10-12', buoi: 'Chiều', gioBatDau: '15:45', gioKetThuc: '18:15', thuTu: 4, conHoatDong: true },
  { maCaHoc: 5, tenCa: 'Tiết 13-15', buoi: 'Tối', gioBatDau: '18:30', gioKetThuc: '21:00', thuTu: 5, conHoatDong: true },
  { maCaHoc: 6, tenCa: 'Tiết 1-2', buoi: 'Sáng', gioBatDau: '07:00', gioKetThuc: '08:30', thuTu: 1, conHoatDong: false },
  { maCaHoc: 7, tenCa: 'Tiết 3-5', buoi: 'Sáng', gioBatDau: '08:45', gioKetThuc: '11:15', thuTu: 2, conHoatDong: false },
]

let mockIdCounter = 100
const mockStore = [...MOCK_SHIFTS]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch {
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const shiftApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/ca-hoc${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai === 'active') result = result.filter(s => s.conHoatDong)
        if (params.TrangThai === 'inactive') result = result.filter(s => !s.conHoatDong)
        if (params.Buoi) result = result.filter(s => s.buoi === params.Buoi)
        result.sort((a, b) => a.thuTu - b.thuTu)
        return result
      }
    )
  },

  getActive() {
    return withFallback(
      () => apiRequest('/api/ca-hoc/active'),
      () => mockStore.filter(s => s.conHoatDong)
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/ca-hoc/${id}`),
      () => mockStore.find(s => s.maCaHoc === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/ca-hoc', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const newShift = { ...data, maCaHoc: ++mockIdCounter }
        mockStore.push(newShift)
        return newShift
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/ca-hoc/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(s => s.maCaHoc === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  toggleActive(id) {
    return withFallback(
      () => apiRequest(`/api/ca-hoc/${id}/toggle-active`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(s => s.maCaHoc === Number(id))
        if (item) item.conHoatDong = !item.conHoatDong
        return { success: true }
      }
    )
  },
}
