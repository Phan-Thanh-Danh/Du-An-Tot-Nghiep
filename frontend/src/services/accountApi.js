import { apiRequest } from './apiClient'
import { organizationApi } from './organizationService'
import { classApi } from './classApi'

const dbCodeToUi = {
  giao_vien: 'GiangVien',
  nhan_vien: 'AcademicStaff',
  hoc_sinh: 'SinhVien',
  hieu_truong: 'Principal',
  sieu_quan_tri: 'SuperAdmin',
  phu_huynh: 'PhuHuynh',
  quan_tri: 'Admin',
  quan_tri_co_so: 'CampusAdmin',
  quan_tri_co_so_con: 'SubCampusAdmin',
  chu_tich: 'Chairman',
}

const uiCodeToApi = {
  GiangVien: 'Teacher',
  SinhVien: 'Student',
  PhuHuynh: 'Parent',
  Admin: 'Admin',
  CampusAdmin: 'CampusAdmin',
  Chairman: 'Chairman',
}

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

function toUiRole(value) {
  if (dbCodeToUi[value]) return dbCodeToUi[value]
  const map = {
    Teacher: 'GiangVien',
    Student: 'SinhVien',
    Parent: 'PhuHuynh',
    AcademicStaff: 'AcademicStaff',
    Principal: 'Principal',
    SuperAdmin: 'SuperAdmin',
    Admin: 'Admin',
    CampusAdmin: 'CampusAdmin',
    SubCampusAdmin: 'SubCampusAdmin',
    Chairman: 'Chairman',
  }
  return map[value] || value
}

function toApiRole(role) {
  return uiCodeToApi[role] || role
}

function normalizeAccount(item) {
  const status = item.trangThai || ''
  const role = toUiRole(item.maCodeVaiTro || item.tenVaiTro || item.vaiTro || item.role)
  return {
    ...item,
    maTaiKhoan: item.maNguoiDung ?? item.maTaiKhoan,
    tenDangNhap: item.tenDangNhap || item.email?.split('@')[0] || '',
    hoTen: item.hoTen || '',
    email: item.email || '',
    vaiTro: role,
    donVi: item.tenDonVi || item.donVi || '',
    lopHanhChinh: item.tenLopHanhChinh || item.lopHanhChinh || '',
    kichHoat: status !== 'Locked' && status !== 'bi_khoa' && status !== 'locked',
    ngayTao: item.ngayTao,
  }
}

function normalizeAccountList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(normalizeAccount)
}

export const accountApi = {
  getMe() {
    return apiRequest('/api/account/me').then(unwrap)
  },

  updateProfile(payload) {
    return apiRequest('/api/account/profile', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        email: payload.email,
        hoTen: payload.hoTen,
        soDienThoai: payload.soDienThoai ?? null,
      }),
    }).then(unwrap)
  },

  changePassword(payload) {
    return apiRequest('/api/account/change-password', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        currentPassword: payload.currentPassword,
        newPassword: payload.newPassword,
        confirmPassword: payload.confirmPassword,
      }),
    })
  },

  list(params = {}) {
    const apiParams = {
      PageIndex: params.PageIndex || 1,
      PageSize: params.PageSize || 100,
      Keyword: params.Search,
      Role: params.VaiTro ? toApiRole(params.VaiTro) : undefined,
      TrangThai: params.KichHoat === 'false' || params.KichHoat === false ? 'Locked' : undefined,
    }
    return apiRequest(`/api/admin/users${buildQuery(apiParams)}`).then(normalizeAccountList)
  },

  get(id) {
    return apiRequest(`/api/admin/users/${id}`).then(res => normalizeAccount(unwrap(res)))
  },

  create(payload) {
    const body = {
      hoTen: payload.hoTen,
      email: payload.email,
      soDienThoai: payload.soDienThoai || null,
      matKhau: payload.matKhau,
      maVaiTro: Number(payload.maVaiTro),
      maDonVi: Number(payload.maDonVi),
      maLopHanhChinh: payload.maLopHanhChinh ? Number(payload.maLopHanhChinh) : null,
    }
    return apiRequest('/api/admin/users', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }).then(res => normalizeAccount(unwrap(res)))
  },

  update(id, payload) {
    const body = {
      hoTen: payload.hoTen,
      email: payload.email,
      soDienThoai: payload.soDienThoai || null,
      maVaiTro: Number(payload.maVaiTro),
      maDonVi: Number(payload.maDonVi),
      maLopHanhChinh: payload.maLopHanhChinh ? Number(payload.maLopHanhChinh) : null,
    }
    return apiRequest(`/api/admin/users/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }).then(res => normalizeAccount(unwrap(res)))
  },

  toggleActive(account) {
    const id = typeof account === 'object' ? account.maTaiKhoan : account
    const isActive = typeof account === 'object' ? account.kichHoat : true
    return apiRequest(`/api/admin/users/${id}/${isActive ? 'lock' : 'unlock'}`, { method: 'PATCH' })
  },

  resetPassword(id, newPassword) {
    return apiRequest(`/api/admin/users/${id}/reset-password`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ newPassword }),
    })
  },

  getRoles() {
    return apiRequest('/api/admin/users/roles').then(res => {
      const roles = unwrap(res)
      return Array.isArray(roles) ? roles : []
    })
  },

  getOrganizations() {
    return organizationApi.getAll().then(list =>
      (Array.isArray(list) ? list : []).map(item => ({
        maDonVi: item.id ?? item.maDonVi,
        tenDonVi: item.name ?? item.tenDonVi,
        isActive: item.isActive !== false && item.conHoatDong !== false,
      })),
    )
  },

  getClasses(params = {}) {
    return classApi.list({ PageSize: 100, ...params }).then(list => (Array.isArray(list) ? list : []))
  },

  getVaiTroOptions() {
    return ['GiangVien', 'AcademicStaff', 'SinhVien', 'Principal', 'SuperAdmin', 'PhuHuynh']
  },
}
