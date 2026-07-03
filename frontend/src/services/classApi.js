import { apiRequest } from './apiClient'

const MOCK_CLASSES = [
  { maLop: 1, maCodeLop: 'SE1601', tenLop: 'Kỹ thuật phần mềm 1601', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2024, khoaHoc: 'K16', siSo: 45, siSoToiDa: 50, maGiaoVien: 1, tenGiaoVien: 'TS. Nguyễn Văn An', namNhapHoc: '2024', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 2, maCodeLop: 'SE1602', tenLop: 'Kỹ thuật phần mềm 1602', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2024, khoaHoc: 'K16', siSo: 42, siSoToiDa: 50, maGiaoVien: 3, tenGiaoVien: 'ThS. Lê Văn Cường', namNhapHoc: '2024', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 3, maCodeLop: 'SA1709', tenLop: 'Quản trị doanh nghiệp 1709', maKhoa: 2, tenKhoa: 'Quản trị kinh doanh', maKhoaHoc: 2024, khoaHoc: 'K17', siSo: 38, siSoToiDa: 45, maGiaoVien: 4, tenGiaoVien: 'TS. Phạm Minh Đức', namNhapHoc: '2024', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 4, maCodeLop: 'SE1610', tenLop: 'Kỹ thuật phần mềm 1610', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2024, khoaHoc: 'K16', siSo: 35, siSoToiDa: 50, maGiaoVien: 2, tenGiaoVien: 'ThS. Trần Thị Bình', namNhapHoc: '2024', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 5, maCodeLop: 'SE1501', tenLop: 'Kỹ thuật phần mềm 1501', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2023, khoaHoc: 'K15', siSo: 40, siSoToiDa: 50, maGiaoVien: 1, tenGiaoVien: 'TS. Nguyễn Văn An', namNhapHoc: '2023', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 6, maCodeLop: 'SA1601', tenLop: 'Quản trị doanh nghiệp 1601', maKhoa: 2, tenKhoa: 'Quản trị kinh doanh', maKhoaHoc: 2023, khoaHoc: 'K16', siSo: 50, siSoToiDa: 50, maGiaoVien: 5, tenGiaoVien: 'ThS. Hoàng Thị Lan', namNhapHoc: '2023', trangThai: 'dang_hoc', ghiChu: '' },
  { maLop: 7, maCodeLop: 'SE1701', tenLop: 'Kỹ thuật phần mềm 1701', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2025, khoaHoc: 'K17', siSo: 30, siSoToiDa: 50, maGiaoVien: 6, tenGiaoVien: 'TS. Lê Minh Tuấn', namNhapHoc: '2025', trangThai: 'moi', ghiChu: 'Lớp mới năm 2025' },
  { maLop: 8, maCodeLop: 'SE1702', tenLop: 'Kỹ thuật phần mềm 1702', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2025, khoaHoc: 'K17', siSo: 0, siSoToiDa: 50, maGiaoVien: null, tenGiaoVien: '', namNhapHoc: '2025', trangThai: 'moi', ghiChu: 'Chờ phân công GVCN' },
  { maLop: 9, maCodeLop: 'SA1801', tenLop: 'Quản trị doanh nghiệp 1801', maKhoa: 2, tenKhoa: 'Quản trị kinh doanh', maKhoaHoc: 2025, khoaHoc: 'K18', siSo: 28, siSoToiDa: 45, maGiaoVien: 7, tenGiaoVien: 'ThS. Phạm Thị Hoa', namNhapHoc: '2025', trangThai: 'moi', ghiChu: '' },
  { maLop: 10, maCodeLop: 'SE1801', tenLop: 'Kỹ thuật phần mềm 1801', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2026, khoaHoc: 'K18', siSo: 0, siSoToiDa: 50, maGiaoVien: null, tenGiaoVien: '', namNhapHoc: '2026', trangThai: 'moi', ghiChu: 'Dự kiến khai giảng 09/2026' },
  { maLop: 11, maCodeLop: 'SE1401', tenLop: 'Kỹ thuật phần mềm 1401', maKhoa: 1, tenKhoa: 'Công nghệ thông tin', maKhoaHoc: 2022, khoaHoc: 'K14', siSo: 44, siSoToiDa: 50, maGiaoVien: 8, tenGiaoVien: 'TS. Đặng Văn Hùng', namNhapHoc: '2022', trangThai: 'da_tot_nghiep', ghiChu: 'Đã tốt nghiệp' },
  { maLop: 12, maCodeLop: 'SA1501', tenLop: 'Quản trị doanh nghiệp 1501', maKhoa: 2, tenKhoa: 'Quản trị kinh doanh', maKhoaHoc: 2023, khoaHoc: 'K15', siSo: 46, siSoToiDa: 50, maGiaoVien: 9, tenGiaoVien: 'TS. Nguyễn Thị Mai', namNhapHoc: '2023', trangThai: 'da_tot_nghiep', ghiChu: 'Đã tốt nghiệp' },
]

let mockIdCounter = 100
const mockStore = [...MOCK_CLASSES]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch {
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

export const classApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/lop${qs ? '?' + qs : ''}`)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai) result = result.filter(c => c.trangThai === params.TrangThai)
        if (params.MaKhoa) result = result.filter(c => c.maKhoa === Number(params.MaKhoa))
        if (params.Search) {
          const q = params.Search.toLowerCase()
          result = result.filter(c => c.maCodeLop.toLowerCase().includes(q) || c.tenLop.toLowerCase().includes(q))
        }
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/lop/${id}`),
      () => mockStore.find(c => c.maLop === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/lop', { method: 'POST', body: JSON.stringify(data) }),
      () => {
        const newItem = { ...data, maLop: ++mockIdCounter, trangThai: data.trangThai || 'moi' }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => apiRequest(`/api/lop/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(c => c.maLop === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  delete(id) {
    return withFallback(
      () => apiRequest(`/api/lop/${id}`, { method: 'DELETE' }),
      () => {
        const idx = mockStore.findIndex(c => c.maLop === Number(id))
        if (idx !== -1) mockStore.splice(idx, 1)
        return { success: true }
      }
    )
  },

  getStudents(classId) {
    return withFallback(
      () => apiRequest(`/api/lop/${classId}/students`),
      () => {
        const count = mockStore.find(c => c.maLop === Number(classId))?.siSo || 0
        return Array.from({ length: Math.min(count, 5) }, (_, i) => ({
          maSinhVien: `SV${String(classId).padStart(3, '0')}${String(i + 1).padStart(2, '0')}`,
          hoTen: `Sinh viên ${i + 1}`,
          email: `sv${i + 1}@lms.edu.vn`,
          trangThai: 'dang_hoc',
        }))
      }
    )
  },
}
