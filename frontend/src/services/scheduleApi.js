import { apiRequest } from './apiClient'

const enableMock = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const MOCK_ROWS = [
  { maTkb: 1, maKhoaHoc: 1, tieuDeKhoaHoc: 'Lập trình Java - SE1601', maDonVi: 1, tenDonVi: 'Cơ sở chính', maHocKy: 4, tenHocKy: 'Học kỳ 1 - 2025-2026', maLop: 1, tenLop: 'SE1601', maCodeLop: 'SE1601', maMonHoc: 1, maCodeMonHoc: 'CT101', tenMonHoc: 'Lập trình Java', maGiaoVien: 1, tenGiaoVien: 'TS. Nguyễn Văn An', thuTrongTuan: 2, maCaHoc: 1, tenCa: 'Ca 1', buoi: 'Sáng', gioBatDau: '07:30', gioKetThuc: '09:00', maPhong: 5, maCodePhong: 'PH301', tenPhong: 'P.301', ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', trangThai: 'da_xuat_ban' },
  { maTkb: 2, maKhoaHoc: 2, tieuDeKhoaHoc: 'CTDL & GT - SE1601', maDonVi: 1, tenDonVi: 'Cơ sở chính', maHocKy: 4, tenHocKy: 'Học kỳ 1 - 2025-2026', maLop: 1, tenLop: 'SE1601', maCodeLop: 'SE1601', maMonHoc: 2, maCodeMonHoc: 'CT201', tenMonHoc: 'CTDL & Giải thuật', maGiaoVien: 2, tenGiaoVien: 'ThS. Trần Thị Bình', thuTrongTuan: 4, maCaHoc: 3, tenCa: 'Ca 3', buoi: 'Sáng', gioBatDau: '10:50', gioKetThuc: '12:20', maPhong: 7, maCodePhong: 'LAB401', tenPhong: 'Lab 401', ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', trangThai: 'da_xuat_ban' },
  { maTkb: 3, maKhoaHoc: 3, tieuDeKhoaHoc: 'Lập trình Web - SE1602', maDonVi: 1, tenDonVi: 'Cơ sở chính', maHocKy: 4, tenHocKy: 'Học kỳ 1 - 2025-2026', maLop: 2, tenLop: 'SE1602', maCodeLop: 'SE1602', maMonHoc: 3, maCodeMonHoc: 'CT202', tenMonHoc: 'Lập trình Web', maGiaoVien: 3, tenGiaoVien: 'ThS. Lê Văn Cường', thuTrongTuan: 3, maCaHoc: 2, tenCa: 'Ca 2', buoi: 'Sáng', gioBatDau: '09:10', gioKetThuc: '10:40', maPhong: 6, maCodePhong: 'PH302', tenPhong: 'P.302', ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', trangThai: 'nhap' },
  { maTkb: 4, maKhoaHoc: 4, tieuDeKhoaHoc: 'Cơ sở dữ liệu - SA1709', maDonVi: 1, tenDonVi: 'Cơ sở chính', maHocKy: 4, tenHocKy: 'Học kỳ 1 - 2025-2026', maLop: 3, tenLop: 'SA1709', maCodeLop: 'SA1709', maMonHoc: 4, maCodeMonHoc: 'CT301', tenMonHoc: 'Cơ sở dữ liệu', maGiaoVien: 1, tenGiaoVien: 'TS. Nguyễn Văn An', thuTrongTuan: 5, maCaHoc: 4, tenCa: 'Ca 4', buoi: 'Chiều', gioBatDau: '13:00', gioKetThuc: '14:30', maPhong: 8, maCodePhong: 'PH501', tenPhong: 'P.501', ngayBatDau: '2025-09-01', ngayKetThuc: '2026-01-09', trangThai: 'nhap' },
  { maTkb: 5, maKhoaHoc: 5, tieuDeKhoaHoc: 'Mạng máy tính - SE1610', maDonVi: 1, tenDonVi: 'Cơ sở chính', maHocKy: 5, tenHocKy: 'Học kỳ 2 - 2025-2026', maLop: 4, tenLop: 'SE1610', maCodeLop: 'SE1610', maMonHoc: 5, maCodeMonHoc: 'CT302', tenMonHoc: 'Mạng máy tính', maGiaoVien: 4, tenGiaoVien: 'TS. Phạm Minh Đức', thuTrongTuan: 6, maCaHoc: 5, tenCa: 'Ca 5', buoi: 'Chiều', gioBatDau: '14:40', gioKetThuc: '16:10', maPhong: 5, maCodePhong: 'PH301', tenPhong: 'P.301', ngayBatDau: '2026-01-19', ngayKetThuc: '2026-06-06', trangThai: 'da_huy' },
]

let mockIdCounter = 100
const mockStore = [...MOCK_ROWS]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch (error) {
    if (!enableMock) throw error
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const scheduleApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai) result = result.filter(s => s.trangThai === params.TrangThai)
        if (params.MaHocKy) result = result.filter(s => s.maHocKy === Number(params.MaHocKy))
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/thoi-khoa-bieu/${id}`),
      () => mockStore.find(s => s.maTkb === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/thoi-khoa-bieu', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const newItem = {
          ...data,
          maTkb: ++mockIdCounter,
          maDonVi: 1,
          tenDonVi: 'Cơ sở chính',
          trangThai: data.trangThai || 'nhap',
        }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/thoi-khoa-bieu/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(s => s.maTkb === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  cancel(id) {
    return withFallback(
      () => apiRequest(`/api/thoi-khoa-bieu/${id}/cancel`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(s => s.maTkb === Number(id))
        if (item) item.trangThai = 'da_huy'
        return { success: true }
      }
    )
  },

  checkConflicts(data) {
    return withFallback(
      () => apiRequest('/api/thoi-khoa-bieu/check-xung-dot', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const existing = mockStore.filter(s =>
          s.thuTrongTuan === data.thuTrongTuan &&
          s.maCaHoc === data.maCaHoc &&
          s.trangThai !== 'da_huy' &&
          (!data.excludeMaTkb || s.maTkb !== data.excludeMaTkb)
        )
        const conflicts = []
        existing.forEach(s => {
          if (data.maPhong && s.maPhong === data.maPhong)
            conflicts.push({ type: 'Phòng', message: `${s.tenPhong} đã có lịch: ${s.tieuDeKhoaHoc}`, maTkb: s.maTkb })
          if (data.maGiaoVien && s.maGiaoVien === data.maGiaoVien)
            conflicts.push({ type: 'GV', message: `${s.tenGiaoVien} đã có lịch: ${s.tieuDeKhoaHoc}`, maTkb: s.maTkb })
          if (data.maLop && s.maLop === data.maLop)
            conflicts.push({ type: 'Lớp', message: `Lớp ${s.tenLop} đã có lịch: ${s.tieuDeKhoaHoc}`, maTkb: s.maTkb })
        })
        return { hasConflict: conflicts.length > 0, conflicts }
      }
    )
  },

  generateSessions(id) {
    return withFallback(
      () => apiRequest(`/api/thoi-khoa-bieu/${id}/generate-sessions`, { method: 'POST' }),
      () => ({ totalDates: 15, created: 15, skippedExisting: 0, sessions: [] })
    )
  },
}
