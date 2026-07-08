import { apiRequest } from './apiClient'

const vaiTroOptions = ['GiangVien', 'AcademicStaff', 'SinhVien', 'Principal', 'SuperAdmin', 'PhuHuynh']

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

function toUiRole(role) {
  const map = {
    Teacher: 'GiangVien',
    Student: 'SinhVien',
    Parent: 'PhuHuynh',
    AcademicStaff: 'AcademicStaff',
    Principal: 'Principal',
    SuperAdmin: 'SuperAdmin',
    Admin: 'SuperAdmin',
  }
  return map[role] || role
}

function toApiRole(role) {
  const map = {
    GiangVien: 'Teacher',
    SinhVien: 'Student',
    PhuHuynh: 'Parent',
  }
  return map[role] || role
}

function normalizeAccount(item) {
  const role = toUiRole(item.tenVaiTro || item.vaiTro || item.role)
  const status = item.trangThai || ''
  return {
    ...item,
    maTaiKhoan: item.maNguoiDung ?? item.maTaiKhoan,
    tenDangNhap: item.tenDangNhap || item.email?.split('@')[0] || '',
    hoTen: item.hoTen || '',
    email: item.email || '',
    vaiTro: role,
    donVi: item.tenDonVi || item.donVi || '',
    kichHoat: status !== 'bi_khoa' && status !== 'inactive' && status !== 'locked',
    ngayTao: item.ngayTao,
  }
}

function normalizeAccountList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(normalizeAccount)
}

function unavailable() {
  throw new Error('Chức năng đang phát triển')
}

export const accountApi = {
  list(params = {}) {
    const apiParams = {
      PageSize: params.PageSize || 100,
      Keyword: params.Search,
      Role: params.VaiTro ? toApiRole(params.VaiTro) : undefined,
      TrangThai: params.KichHoat === 'false' || params.KichHoat === false ? 'bi_khoa' : undefined,
    }
    return apiRequest(`/api/admin/users${buildQuery(apiParams)}`).then(normalizeAccountList)
  },

  get(id) {
    return apiRequest(`/api/admin/users/${id}`).then(res => normalizeAccount(unwrap(res)))
  },

  create() {
    return unavailable()
  },

  update() {
    return unavailable()
  },

  toggleActive(account) {
    const id = typeof account === 'object' ? account.maTaiKhoan : account
    const isActive = typeof account === 'object' ? account.kichHoat : true
    return apiRequest(`/api/admin/users/${id}/${isActive ? 'lock' : 'unlock'}`, { method: 'PATCH' })
  },

  resetPassword() {
    return unavailable()
  },

  getVaiTroOptions() {
    return vaiTroOptions
  },
}
