import { apiRequest } from './apiClient'

function unwrap(response) {
  return response?.data ?? response?.Data ?? response
}

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.set(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
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
    return apiRequest(`/api/thoi-khoa-bieu${buildQuery(params)}`).then(normalizeAssignmentList)
  },

  get(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`).then(res => normalizeAssignment(unwrap(res)))
  },

  create() {
    return unavailable()
  },

  update() {
    return unavailable()
  },

  remove() {
    return unavailable()
  },

  assignTeacher() {
    return unavailable()
  },

  getTeachers(params = {}) {
    const apiParams = { PageSize: 100, Role: 'Teacher', Keyword: params.Keyword }
    return apiRequest(`/api/admin/users${buildQuery(apiParams)}`).then(normalizeTeacherList)
  },

  getDonViOptions() {
    return []
  },
}
