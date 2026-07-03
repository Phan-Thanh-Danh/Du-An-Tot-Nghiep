import { apiRequest } from './apiClient'

const enableMock = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const MOCK_ASSIGNMENTS = [
  { maPhanCong: 1, maLopHocPhan: 101, tenLop: 'SE1601', monHoc: 'Lập trình Java', maGiangVien: 1, giangVien: 'TS. Nguyễn Minh Khoa', soBuoi: 3, trangThai: 'assigned', donVi: 'CNTT', lichDay: 'T2 (Ca 1-2), T4 (Ca 3)', phong: 'Lab 102', siSo: 30 },
  { maPhanCong: 2, maLopHocPhan: 102, tenLop: 'SE1602', monHoc: 'Cấu trúc dữ liệu', maGiangVien: 2, giangVien: 'ThS. Trần Thị Lan', soBuoi: 2, trangThai: 'assigned', donVi: 'CNTT', lichDay: 'T3 (Ca 4-5)', phong: 'P.305', siSo: 35 },
  { maPhanCong: 3, maLopHocPhan: 103, tenLop: 'SE1603', monHoc: 'Lập trình Web', maGiangVien: null, giangVien: 'Chưa phân công', soBuoi: 3, trangThai: 'unassigned', donVi: 'CNTT', lichDay: 'T5 (Ca 1-3)', phong: 'Lab 201', siSo: 28 },
  { maPhanCong: 4, maLopHocPhan: 104, tenLop: 'SE1604', monHoc: 'Hệ quản trị CSDL', maGiangVien: 3, giangVien: 'ThS. Lê Văn Dũng', soBuoi: 3, trangThai: 'assigned', donVi: 'CNTT', lichDay: 'T6 (Ca 4-6)', phong: 'Lab 304', siSo: 32 },
  { maPhanCong: 5, maLopHocPhan: 105, tenLop: 'AI1601', monHoc: 'Học máy', maGiangVien: null, giangVien: 'Chưa phân công', soBuoi: 3, trangThai: 'unassigned', donVi: 'CNTT', lichDay: 'T4 (Ca 2-4)', phong: 'Lab 101', siSo: 25 },
  { maPhanCong: 6, maLopHocPhan: 106, tenLop: 'AI1602', monHoc: 'Xử lý ảnh', maGiangVien: 4, giangVien: 'PGS. Phạm Văn Hùng', soBuoi: 2, trangThai: 'assigned', donVi: 'CNTT', lichDay: 'T3 (Ca 1-2)', phong: 'Lab 203', siSo: 20 },
  { maPhanCong: 7, maLopHocPhan: 107, tenLop: 'MK1601', monHoc: 'Marketing căn bản', maGiangVien: 5, giangVien: 'TS. Lê Thị Mai', soBuoi: 2, trangThai: 'assigned', donVi: 'KinhTe', lichDay: 'T5 (Ca 4-5)', phong: 'P.401', siSo: 40 },
  { maPhanCong: 8, maLopHocPhan: 108, tenLop: 'QT1601', monHoc: 'Quản trị học', maGiangVien: null, giangVien: 'Chưa phân công', soBuoi: 3, trangThai: 'unassigned', donVi: 'KinhTe', lichDay: 'T2 (Ca 4-6)', phong: 'P.402', siSo: 35 },
]

const TEACHERS = [
  { maGiangVien: 1, hoTen: 'TS. Nguyễn Minh Khoa', donVi: 'CNTT', soTietDaDay: 12, tietToiDa: 30 },
  { maGiangVien: 2, hoTen: 'ThS. Trần Thị Lan', donVi: 'CNTT', soTietDaDay: 8, tietToiDa: 24 },
  { maGiangVien: 3, hoTen: 'ThS. Lê Văn Dũng', donVi: 'CNTT', soTietDaDay: 15, tietToiDa: 30 },
  { maGiangVien: 4, hoTen: 'PGS. Phạm Văn Hùng', donVi: 'CNTT', soTietDaDay: 6, tietToiDa: 20 },
  { maGiangVien: 5, hoTen: 'TS. Lê Thị Mai', donVi: 'KinhTe', soTietDaDay: 10, tietToiDa: 24 },
  { maGiangVien: 6, hoTen: 'ThS. Nguyễn Văn An', donVi: 'KinhTe', soTietDaDay: 4, tietToiDa: 20 },
  { maGiangVien: 7, hoTen: 'TS. Trần Văn Bình', donVi: 'CNTT', soTietDaDay: 18, tietToiDa: 30 },
  { maGiangVien: 8, hoTen: 'ThS. Hoàng Thị Hương', donVi: 'KinhTe', soTietDaDay: 0, tietToiDa: 20 },
]

let mockIdCounter = 100
const mockStore = MOCK_ASSIGNMENTS.map(a => ({ ...a }))

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch (error) {
    if (!enableMock) throw error
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

function unwrap(response) {
  return response?.data ?? response?.Data ?? response
}

function normalizeAssignment(item) {
  return {
    ...item,
    maPhanCong: item.maTkb ?? item.maPhanCong,
    maLopHocPhan: item.maKhoaHoc ?? item.maLopHocPhan,
    tenLop: item.tenLop || item.maCodeLop || '',
    monHoc: item.tenMonHoc || item.monHoc || item.tieuDeKhoaHoc || '',
    maGiangVien: item.maGiaoVien ?? item.maGiangVien,
    giangVien: item.tenGiaoVien || item.giangVien || 'Chưa phân công',
    trangThai: item.maGiaoVien ? 'assigned' : 'unassigned',
    lichDay: item.thuTrongTuan ? `Thứ ${item.thuTrongTuan} (${item.tenCa || ''})` : item.lichDay,
    phong: item.tenPhong || item.maCodePhong || item.phong,
    siSo: item.siSo || 0,
  }
}

function normalizeAssignmentList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(normalizeAssignment)
}

function normalizeTeacherList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(item => ({
    maGiangVien: item.maNguoiDung,
    hoTen: item.hoTen,
    donVi: item.tenDonVi || '',
    soTietDaDay: 0,
    tietToiDa: 0,
  }))
}

function unavailable() {
  throw new Error('Chức năng đang phát triển')
}

export const assignmentApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/thoi-khoa-bieu${qs ? '?' + qs : ''}`).then(normalizeAssignmentList)
      },
      () => {
        let result = [...mockStore]
        if (params.TrangThai === 'assigned') result = result.filter(a => a.trangThai === 'assigned')
        if (params.TrangThai === 'unassigned') result = result.filter(a => a.trangThai === 'unassigned')
        if (params.DonVi) result = result.filter(a => a.donVi === params.DonVi)
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/thoi-khoa-bieu/${id}`).then(res => normalizeAssignment(unwrap(res))),
      () => mockStore.find(a => a.maPhanCong === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => unavailable(),
      () => {
        const newItem = { ...data, maPhanCong: ++mockIdCounter }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => unavailable(),
      () => {
        const idx = mockStore.findIndex(a => a.maPhanCong === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  remove(id) {
    return withFallback(
      () => unavailable(),
      () => {
        const idx = mockStore.findIndex(a => a.maPhanCong === Number(id))
        if (idx !== -1) mockStore.splice(idx, 1)
        return { success: true }
      }
    )
  },

  assignTeacher(id, maGiangVien) {
    return withFallback(
      () => unavailable(),
      () => {
        const item = mockStore.find(a => a.maPhanCong === Number(id))
        if (item) {
          const teacher = TEACHERS.find(t => t.maGiangVien === Number(maGiangVien))
          item.maGiangVien = Number(maGiangVien)
          item.giangVien = teacher ? teacher.hoTen : 'Đã phân công'
          item.trangThai = 'assigned'
        }
        return { success: true }
      }
    )
  },

  getTeachers(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        const apiParams = { PageSize: 100, Role: 'Teacher', Keyword: params.Keyword }
        Object.entries(apiParams).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/admin/users${qs ? '?' + qs : ''}`).then(normalizeTeacherList)
      },
      () => {
        let result = [...TEACHERS]
        if (params.DonVi) result = result.filter(t => t.donVi === params.DonVi)
        return result
      }
    )
  },

  getDonViOptions() {
    return [...new Set(mockStore.map(a => a.donVi).filter(Boolean))]
  },
}
