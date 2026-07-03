import { apiRequest } from './apiClient'

const enableMock = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const MOCK_ACCOUNTS = [
  { maTaiKhoan: 1, tenDangNhap: 'nguyen.van.a', hoTen: 'TS. Nguyễn Văn An', email: 'an.nv@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2023-08-01' },
  { maTaiKhoan: 2, tenDangNhap: 'tran.thi.binh', hoTen: 'ThS. Trần Thị Bình', email: 'binh.tt@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2023-08-01' },
  { maTaiKhoan: 3, tenDangNhap: 'le.van.cuong', hoTen: 'ThS. Lê Văn Cường', email: 'cuong.lv@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2023-09-15' },
  { maTaiKhoan: 4, tenDangNhap: 'pham.minh.duc', hoTen: 'TS. Phạm Minh Đức', email: 'duc.pm@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Quản trị kinh doanh', kichHoat: true, ngayTao: '2023-08-01' },
  { maTaiKhoan: 5, tenDangNhap: 'hoang.thi.lan', hoTen: 'ThS. Hoàng Thị Lan', email: 'lan.ht@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Quản trị kinh doanh', kichHoat: true, ngayTao: '2024-01-10' },
  { maTaiKhoan: 6, tenDangNhap: 'le.minh.tuan', hoTen: 'TS. Lê Minh Tuấn', email: 'tuan.lm@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Công nghệ thông tin', kichHoat: false, ngayTao: '2024-02-20' },
  { maTaiKhoan: 7, tenDangNhap: 'pham.thi.hoa', hoTen: 'ThS. Phạm Thị Hoa', email: 'hoa.pt@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Quản trị kinh doanh', kichHoat: true, ngayTao: '2024-03-05' },
  { maTaiKhoan: 8, tenDangNhap: 'dang.van.hung', hoTen: 'TS. Đặng Văn Hùng', email: 'hung.dv@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2023-08-01' },
  { maTaiKhoan: 9, tenDangNhap: 'nguyen.thi.mai', hoTen: 'TS. Nguyễn Thị Mai', email: 'mai.nt@lms.edu.vn', vaiTro: 'GiangVien', donVi: 'Quản trị kinh doanh', kichHoat: true, ngayTao: '2023-08-01' },
  { maTaiKhoan: 10, tenDangNhap: 'tran.gia.vu', hoTen: 'Trần Thị Giáo Vụ', email: 'giaovu@lms.edu.vn', vaiTro: 'AcademicStaff', donVi: 'Phòng Đào tạo', kichHoat: true, ngayTao: '2023-07-01' },
  { maTaiKhoan: 11, tenDangNhap: 'sv001.se1601', hoTen: 'Nguyễn Văn Sinh Viên', email: 'sv001@lms.edu.vn', vaiTro: 'SinhVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2024-09-01' },
  { maTaiKhoan: 12, tenDangNhap: 'sv002.se1601', hoTen: 'Trần Thị Học Sinh', email: 'sv002@lms.edu.vn', vaiTro: 'SinhVien', donVi: 'Công nghệ thông tin', kichHoat: true, ngayTao: '2024-09-01' },
]

let mockIdCounter = 200
const mockStore = [...MOCK_ACCOUNTS]

async function withFallback(apiCall, mockFallback) {
  try {
    const res = await apiCall()
    return res
  } catch (error) {
    if (!enableMock) throw error
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

const vaiTroOptions = ['GiangVien', 'AcademicStaff', 'SinhVien', 'Principal', 'SuperAdmin', 'PhuHuynh']

function unwrap(response) {
  return response?.data ?? response?.Data ?? response
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
    return withFallback(
      () => {
        const query = new URLSearchParams()
        const apiParams = {
          PageSize: params.PageSize || 100,
          Keyword: params.Search,
          Role: params.VaiTro ? toApiRole(params.VaiTro) : undefined,
          TrangThai: params.KichHoat === 'false' || params.KichHoat === false ? 'bi_khoa' : undefined,
        }
        Object.entries(apiParams).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/admin/users${qs ? '?' + qs : ''}`).then(normalizeAccountList)
      },
      () => {
        let result = [...mockStore]
        if (params.VaiTro) result = result.filter(a => a.vaiTro === params.VaiTro)
        if (params.KichHoat !== undefined && params.KichHoat !== '') {
          const val = params.KichHoat === 'true' || params.KichHoat === true
          result = result.filter(a => a.kichHoat === val)
        }
        if (params.Search) {
          const q = params.Search.toLowerCase()
          result = result.filter(a =>
            a.hoTen.toLowerCase().includes(q) ||
            a.tenDangNhap.toLowerCase().includes(q) ||
            a.email.toLowerCase().includes(q)
          )
        }
        return result
      }
    )
  },

  get(id) {
    return withFallback(
      () => apiRequest(`/api/admin/users/${id}`).then(res => normalizeAccount(unwrap(res))),
      () => mockStore.find(a => a.maTaiKhoan === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => unavailable(),
      () => {
        const newItem = {
          ...data,
          maTaiKhoan: ++mockIdCounter,
          kichHoat: data.kichHoat !== undefined ? data.kichHoat : true,
          ngayTao: new Date().toISOString().split('T')[0],
        }
        mockStore.push(newItem)
        return newItem
      }
    )
  },

  update(id, data) {
    return withFallback(
      () => unavailable(),
      () => {
        const idx = mockStore.findIndex(a => a.maTaiKhoan === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  toggleActive(account) {
    const id = typeof account === 'object' ? account.maTaiKhoan : account
    const isActive = typeof account === 'object' ? account.kichHoat : true
    return withFallback(
      () => apiRequest(`/api/admin/users/${id}/${isActive ? 'lock' : 'unlock'}`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(a => a.maTaiKhoan === Number(id))
        if (item) item.kichHoat = !item.kichHoat
        return item || null
      }
    )
  },

  resetPassword(_id) {
    return withFallback(
      () => unavailable(),
      () => ({ success: true, newPassword: 'Abc@123456' })
    )
  },

  getVaiTroOptions() {
    return vaiTroOptions
  },
}
