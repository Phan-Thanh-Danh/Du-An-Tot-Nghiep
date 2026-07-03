import { apiRequest } from './apiClient'

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
  } catch {
    return typeof mockFallback === 'function' ? mockFallback() : mockFallback
  }
}

const vaiTroOptions = ['GiangVien', 'AcademicStaff', 'SinhVien', 'Principal', 'SuperAdmin', 'PhuHuynh']

export const accountApi = {
  list(params = {}) {
    return withFallback(
      () => {
        const query = new URLSearchParams()
        Object.entries(params).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') query.set(k, v) })
        const qs = query.toString()
        return apiRequest(`/api/admin/accounts${qs ? '?' + qs : ''}`)
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
      () => apiRequest(`/api/admin/accounts/${id}`),
      () => mockStore.find(a => a.maTaiKhoan === Number(id)) || null
    )
  },

  create(data) {
    return withFallback(
      () => apiRequest('/api/admin/accounts', { method: 'POST', body: JSON.stringify(data) }),
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
      () => apiRequest(`/api/admin/accounts/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
      () => {
        const idx = mockStore.findIndex(a => a.maTaiKhoan === Number(id))
        if (idx !== -1) mockStore[idx] = { ...mockStore[idx], ...data }
        return { success: true }
      }
    )
  },

  toggleActive(id) {
    return withFallback(
      () => apiRequest(`/api/admin/accounts/${id}/toggle-active`, { method: 'PATCH' }),
      () => {
        const item = mockStore.find(a => a.maTaiKhoan === Number(id))
        if (item) item.kichHoat = !item.kichHoat
        return item || null
      }
    )
  },

  resetPassword(id) {
    return withFallback(
      () => apiRequest(`/api/admin/accounts/${id}/reset-password`, { method: 'PATCH' }),
      () => ({ success: true, newPassword: 'Abc@123456' })
    )
  },

  getVaiTroOptions() {
    return vaiTroOptions
  },
}
