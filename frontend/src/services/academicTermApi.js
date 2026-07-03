import { apiRequest } from './apiClient'

const MOCK_TERMS = [
  { maHocKy: 1, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeHocKy: 'HK1-2024-2025', tenHocKy: 'Học kỳ 1', namHoc: '2024-2025', thuTuTrongNam: 1, ngayBatDau: '2024-09-02', ngayKetThuc: '2025-01-10', ngayKetThucBlock5: null, daKhoa: true, soTinChiToiDa: 24, hanRutMon: '2024-10-15' },
  { maHocKy: 2, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeHocKy: 'HK2-2024-2025', tenHocKy: 'Học kỳ 2', namHoc: '2024-2025', thuTuTrongNam: 2, ngayBatDau: '2025-01-20', ngayKetThuc: '2025-06-07', ngayKetThucBlock5: null, daKhoa: true, soTinChiToiDa: 24, hanRutMon: '2025-03-01' },
  { maHocKy: 3, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeHocKy: 'HK3-2024-2025', tenHocKy: 'Học kỳ hè', namHoc: '2024-2025', thuTuTrongNam: 3, ngayBatDau: '2025-06-16', ngayKetThuc: '2025-08-08', ngayKetThucBlock5: null, daKhoa: true, soTinChiToiDa: 16, hanRutMon: '2025-07-01' },
  { maHocKy: 4, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeHocKy: 'HK1-2025-2026', tenHocKy: 'Học kỳ 1', namHoc: '2025-2026', thuTuTrongNam: 1, ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', ngayKetThucBlock5: null, daKhoa: false, soTinChiToiDa: 24, hanRutMon: '2025-10-15' },
  { maHocKy: 5, maDonVi: 1, tenDonVi: 'Cơ sở chính', maCodeHocKy: 'HK2-2025-2026', tenHocKy: 'Học kỳ 2', namHoc: '2025-2026', thuTuTrongNam: 2, ngayBatDau: '2026-01-19', ngayKetThuc: '2026-06-06', ngayKetThucBlock5: null, daKhoa: false, soTinChiToiDa: 24, hanRutMon: '2026-03-01' },
  { maHocKy: 6, maDonVi: 2, tenDonVi: 'Cơ sở phụ', maCodeHocKy: 'CS2-HK1-2025-2026', tenHocKy: 'Học kỳ 1', namHoc: '2025-2026', thuTuTrongNam: 1, ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', ngayKetThucBlock5: null, daKhoa: false, soTinChiToiDa: 20, hanRutMon: '2025-10-15' },
]

let mockIdCounter = 100
const mockStore = [...MOCK_TERMS]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch {
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const academicTermApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/master-data/academic-terms${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai === 'locked') result = result.filter(t => t.daKhoa)
        if (params.TrangThai === 'unlocked') result = result.filter(t => !t.daKhoa)
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/academic-terms/${id}`),
      () => mockStore.find(t => t.maHocKy === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/master-data/academic-terms', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const newItem = { ...data, maHocKy: ++mockIdCounter, daKhoa: false }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/master-data/academic-terms/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(t => t.maHocKy === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  lock(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/academic-terms/${id}/lock`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(t => t.maHocKy === Number(id))
        if (item) item.daKhoa = true
        return { success: true }
      }
    )
  },

  unlock(id) {
    return withFallback(
      () => apiRequest(`/api/master-data/academic-terms/${id}/unlock`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(t => t.maHocKy === Number(id))
        if (item) item.daKhoa = false
        return { success: true }
      }
    )
  },
}
