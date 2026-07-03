import { apiRequest } from './apiClient'

const MOCK_SUBJECTS = [
  { maMonHoc: 1, maCodeMonHoc: 'CT101', tenMonHoc: 'Lập trình Cơ bản', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 2, maCodeMonHoc: 'CT201', tenMonHoc: 'Cấu trúc dữ liệu & Giải thuật', soTinChi: 4, conHoatDong: true },
  { maMonHoc: 3, maCodeMonHoc: 'CT202', tenMonHoc: 'Lập trình Web', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 4, maCodeMonHoc: 'CT301', tenMonHoc: 'Cơ sở dữ liệu', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 5, maCodeMonHoc: 'CT302', tenMonHoc: 'Mạng máy tính', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 6, maCodeMonHoc: 'CT303', tenMonHoc: 'Công nghệ phần mềm', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 7, maCodeMonHoc: 'CT401', tenMonHoc: 'Trí tuệ nhân tạo', soTinChi: 4, conHoatDong: true },
  { maMonHoc: 8, maCodeMonHoc: 'CT402', tenMonHoc: 'Học máy', soTinChi: 3, conHoatDong: false },
  { maMonHoc: 9, maCodeMonHoc: 'CT403', tenMonHoc: 'An toàn thông tin', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 10, maCodeMonHoc: 'MT101', tenMonHoc: 'Toán cao cấp', soTinChi: 4, conHoatDong: true },
  { maMonHoc: 11, maCodeMonHoc: 'MT201', tenMonHoc: 'Xác suất thống kê', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 12, maCodeMonHoc: 'NN101', tenMonHoc: 'Tiếng Anh cơ bản', soTinChi: 4, conHoatDong: true },
  { maMonHoc: 13, maCodeMonHoc: 'NN102', tenMonHoc: 'Tiếng Anh chuyên ngành', soTinChi: 3, conHoatDong: false },
  { maMonHoc: 14, maCodeMonHoc: 'TC101', tenMonHoc: 'Triết học Mác-Lênin', soTinChi: 3, conHoatDong: true },
  { maMonHoc: 15, maCodeMonHoc: 'CT304', tenMonHoc: 'Lập trình Python', soTinChi: 3, conHoatDong: true },
]

let mockIdCounter = 100
const mockStore = [...MOCK_SUBJECTS]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch {
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const subjectApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/master-data/subjects${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai === 'active') result = result.filter(s => s.conHoatDong)
        if (params.TrangThai === 'inactive') result = result.filter(s => !s.conHoatDong)
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/subjects/${id}`),
      () => mockStore.find(s => s.maMonHoc === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/master-data/subjects', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const newItem = { ...data, maMonHoc: ++mockIdCounter, conHoatDong: true }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/master-data/subjects/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(s => s.maMonHoc === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  deactivate(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/subjects/${id}/deactivate`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(s => s.maMonHoc === Number(id))
        if (item) item.conHoatDong = false
        return { success: true }
      }
    )
  },

  activate(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/subjects/${id}/activate`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(s => s.maMonHoc === Number(id))
        if (item) item.conHoatDong = true
        return { success: true }
      }
    )
  },
}
