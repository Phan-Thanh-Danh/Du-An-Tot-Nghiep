import { apiRequest } from './apiClient'
import { organizationApi } from './organizationService'

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

const STATUS_LABELS = {
  nhap: 'Bản nháp',
  da_xuat_ban: 'Đã xuất bản',
  da_huy: 'Đã hủy',
}

function normalizeAssignment(item) {
  const status = item.trangThai || 'nhap'
  return {
    ...item,
    maPhanCong: item.maTkb ?? item.maPhanCong,
    maLopHocPhan: item.maKhoaHoc ?? item.maLopHocPhan,
    tenLop: item.tenLop || item.maCodeLop || '',
    monHoc: item.tenMonHoc || item.tieuDeKhoaHoc || item.monHoc || '',
    maGiangVien: item.maGiaoVien || null,
    giangVien: item.tenGiaoVien || 'Chưa phân công',
    trangThai: status,
    trangThaiLabel: STATUS_LABELS[status] || status,
    lichDay: item.thuTrongTuan ? `Thứ ${item.thuTrongTuan} · ${item.tenCa || ''}` : item.lichDay,
    phong: item.maCodePhong || item.tenPhong || item.phong,
    donVi: item.tenDonVi || '',
  }
}

function normalizeAssignmentList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(normalizeAssignment)
}

function normalizeCourseList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(item => ({
    maKhoaHoc: item.maKhoaHoc,
    tenLop: item.tenLop || item.maCodeLop || '',
    monHoc: item.tenMonHoc || item.tieuDe || '',
    maLop: item.maLop,
    maMonHoc: item.maMonHoc,
    maHocKy: item.maHocKy,
    maDonVi: item.maDonVi,
    tenDonVi: item.tenDonVi || '',
    maGiangVien: item.maGiaoVien || null,
    giangVien: item.tenGiaoVien || 'Chưa phân công',
    tieuDe: item.tieuDe || '',
    trangThai: item.trangThai || '',
  }))
}

function normalizeTeacherCandidates(response) {
  const data = unwrap(response)
  const candidates = Array.isArray(data) ? data : data?.candidates || []
  return candidates.map(item => ({
    maGiangVien: item.maGiaoVien,
    hoTen: item.hoTen,
    email: item.email,
    chuyenNganh: item.chuyenNganh,
    donVi: '',
    soTietDaDay: item.currentWeeklyShiftCount ?? item.currentClassCount ?? 0,
    tietToiDa: item.weeklyShiftLimit ?? 0,
    isEligible: item.isEligible !== false,
    warnings: item.warnings || [],
    reasons: item.reasons || [],
  }))
}

export const assignmentApi = {
  list(params = {}) {
    const apiParams = {
      PageIndex: 1,
      PageSize: 100,
      TrangThai: params.TrangThai,
      MaKhoaHoc: params.MaKhoaHoc,
      MaLop: params.MaLop,
      MaGiaoVien: params.MaGiaoVien,
      ThuTrongTuan: params.ThuTrongTuan,
    }
    return apiRequest(`/api/thoi-khoa-bieu${buildQuery(apiParams)}`).then(normalizeAssignmentList)
  },

  get(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`).then(res => normalizeAssignment(unwrap(res)))
  },

  create(payload) {
    return apiRequest('/api/thoi-khoa-bieu', {
      method: 'POST',
      body: JSON.stringify(payload),
    }).then(res => normalizeAssignment(unwrap(res)))
  },

  update(id, payload) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    }).then(res => normalizeAssignment(unwrap(res)))
  },

  remove(id) {
    return apiRequest(`/api/thoi-khoa-bieu/${id}/cancel`, {
      method: 'PATCH',
    })
  },

  async assignTeacher(maTkb, maGiaoVien, courseInfo) {
    const detail = unwrap(await apiRequest(`/api/courses/${courseInfo.maKhoaHoc}`))
    return apiRequest(`/api/courses/${courseInfo.maKhoaHoc}`, {
      method: 'PUT',
      body: JSON.stringify({
        MaGiaoVien: maGiaoVien,
        MaHocKy: detail.maHocKy ?? courseInfo.maHocKy ?? null,
        MaLop: detail.maLop ?? courseInfo.maLop,
        TieuDe: detail.tieuDe || courseInfo.tieuDe || courseInfo.monHoc,
        MoTa: detail.moTa ?? null,
        TrangThai: detail.trangThai || courseInfo.trangThai || 'nhap',
        UrlAnhBia: detail.urlAnhBia ?? null,
      }),
    }).then(res => unwrap(res))
  },

  getTeachers(courseInfo = {}) {
    if (!courseInfo.maMonHoc || !courseInfo.maHocKy || !courseInfo.maLop) return Promise.resolve([])
    return apiRequest('/api/courses/assignment-suggestions', {
      method: 'POST',
      body: JSON.stringify({
        MaHocKy: courseInfo.maHocKy,
        MaMonHoc: courseInfo.maMonHoc,
        MaLopIds: [courseInfo.maLop],
        CandidateLimit: 20,
      }),
    }).then(normalizeTeacherCandidates)
  },

  getCourses(params = {}) {
    const apiParams = { PageIndex: 1, PageSize: 100, ...params }
    return apiRequest(`/api/courses${buildQuery(apiParams)}`).then(normalizeCourseList)
  },

  getCaHocs() {
    return apiRequest('/api/ca-hoc/active').then(res => {
      const data = unwrap(res)
      return Array.isArray(data) ? data : []
    })
  },

  getRooms() {
    return apiRequest('/api/master-data/rooms?PageIndex=1&PageSize=100').then(res => {
      const data = unwrap(res)
      const items = Array.isArray(data) ? data : data?.items || []
      return items.map(item => ({
        maPhong: item.maPhong,
        maCodePhong: item.maCodePhong,
        tenPhong: item.tenPhong,
        sucChua: item.sucChua || 0,
        loaiPhong: item.loaiPhong || '',
      }))
    })
  },

  getDonViOptions() {
    return organizationApi.getAll().then(list =>
      Array.from(new Set((Array.isArray(list) ? list : []).map(item => item.name ?? item.tenDonVi).filter(Boolean))),
    )
  },
}
