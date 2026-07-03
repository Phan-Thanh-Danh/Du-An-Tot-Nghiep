import { apiRequest } from './apiClient'

const enableMock = import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const mockCourses = [
  { maKhoaHoc: 1, maDonVi: 1, tenDonVi: 'Cơ sở chính', maMonHoc: 1, tenMonHoc: 'Lập trình Web', maMonHocCode: 'WEB101', maGiaoVien: 1, tenGiaoVien: 'Nguyễn Văn An', maHocKy: 1, tenHocKy: 'HK1 2025-2026', maLop: 1, tenLop: 'CNTT01', tieuDe: 'Lập trình Web - CNTT01', moTa: 'Môn học căn bản về phát triển web', trangThai: 'da_xuat_ban', urlAnhBia: null, ngayTao: '2025-09-01T00:00:00' },
  { maKhoaHoc: 2, maDonVi: 1, tenDonVi: 'Cơ sở chính', maMonHoc: 2, tenMonHoc: 'Cơ sở dữ liệu', maMonHocCode: 'CSDL201', maGiaoVien: 2, tenGiaoVien: 'Trần Thị Bình', maHocKy: 1, tenHocKy: 'HK1 2025-2026', maLop: 2, tenLop: 'CNTT02', tieuDe: 'Cơ sở dữ liệu - CNTT02', moTa: null, trangThai: 'da_xuat_ban', urlAnhBia: null, ngayTao: '2025-09-02T00:00:00' },
  { maKhoaHoc: 3, maDonVi: 2, tenDonVi: 'Cơ sở 2', maMonHoc: 3, tenMonHoc: 'Cấu trúc dữ liệu', maMonHocCode: 'CTDL301', maGiaoVien: 3, tenGiaoVien: 'Lê Văn Cường', maHocKy: 2, tenHocKy: 'HK2 2025-2026', maLop: 3, tenLop: 'KTPM01', tieuDe: 'Cấu trúc dữ liệu - KTPM01', moTa: 'Giải thuật và cấu trúc dữ liệu nâng cao', trangThai: 'nhap', urlAnhBia: null, ngayTao: '2025-10-01T00:00:00' },
  { maKhoaHoc: 4, maDonVi: 1, tenDonVi: 'Cơ sở chính', maMonHoc: 4, tenMonHoc: 'Hệ điều hành', maMonHocCode: 'HDH401', maGiaoVien: 4, tenGiaoVien: 'Phạm Thị Dung', maHocKy: 1, tenHocKy: 'HK1 2025-2026', maLop: 4, tenLop: 'ATTT01', tieuDe: 'Hệ điều hành - ATTT01', moTa: null, trangThai: 'nhap', urlAnhBia: null, ngayTao: '2025-09-15T00:00:00' },
  { maKhoaHoc: 5, maDonVi: 2, tenDonVi: 'Cơ sở 2', maMonHoc: 1, tenMonHoc: 'Lập trình Web', maMonHocCode: 'WEB101', maGiaoVien: 1, tenGiaoVien: 'Nguyễn Văn An', maHocKy: 2, tenHocKy: 'HK2 2025-2026', maLop: 5, tenLop: 'CNTT03', tieuDe: 'Lập trình Web - CNTT03', moTa: 'Phát triển ứng dụng web với Vue.js', trangThai: 'da_xuat_ban', urlAnhBia: null, ngayTao: '2025-10-05T00:00:00' },
  { maKhoaHoc: 6, maDonVi: 1, tenDonVi: 'Cơ sở chính', maMonHoc: 5, tenMonHoc: 'Mạng máy tính', maMonHocCode: 'MMT501', maGiaoVien: 5, tenGiaoVien: 'Hoàng Văn Em', maHocKy: 1, tenHocKy: 'HK1 2025-2026', maLop: 1, tenLop: 'CNTT01', tieuDe: 'Mạng máy tính - CNTT01', moTa: null, trangThai: 'luu_tru', urlAnhBia: null, ngayTao: '2025-08-20T00:00:00' },
  { maKhoaHoc: 7, maDonVi: 1, tenDonVi: 'Cơ sở chính', maMonHoc: 6, tenMonHoc: 'Trí tuệ nhân tạo', maMonHocCode: 'TTNT601', maGiaoVien: 6, tenGiaoVien: 'Ngô Thị Phương', maHocKy: null, tenHocKy: null, maLop: 6, tenLop: 'KHMT01', tieuDe: 'Trí tuệ nhân tạo - KHMT01', moTa: 'Nhập môn AI và Machine Learning', trangThai: 'nhap', urlAnhBia: null, ngayTao: '2025-11-01T00:00:00' },
  { maKhoaHoc: 8, maDonVi: 2, tenDonVi: 'Cơ sở 2', maMonHoc: 2, tenMonHoc: 'Cơ sở dữ liệu', maMonHocCode: 'CSDL201', maGiaoVien: 2, tenGiaoVien: 'Trần Thị Bình', maHocKy: null, tenHocKy: null, maLop: 7, tenLop: 'HTTT01', tieuDe: 'Cơ sở dữ liệu - HTTT01', moTa: null, trangThai: 'nhap', urlAnhBia: null, ngayTao: '2025-11-05T00:00:00' },
]

const mockDropdowns = {
  academicTerms: [
    { value: 1, label: 'HK1 2025-2026' },
    { value: 2, label: 'HK2 2025-2026' },
    { value: 3, label: 'HK1 2026-2027' },
  ],
  subjects: [
    { value: 1, label: 'Lập trình Web (WEB101)' },
    { value: 2, label: 'Cơ sở dữ liệu (CSDL201)' },
    { value: 3, label: 'Cấu trúc dữ liệu (CTDL301)' },
    { value: 4, label: 'Hệ điều hành (HDH401)' },
    { value: 5, label: 'Mạng máy tính (MMT501)' },
    { value: 6, label: 'Trí tuệ nhân tạo (TTNT601)' },
  ],
  teachers: [
    { value: 1, label: 'Nguyễn Văn An' },
    { value: 2, label: 'Trần Thị Bình' },
    { value: 3, label: 'Lê Văn Cường' },
    { value: 4, label: 'Phạm Thị Dung' },
    { value: 5, label: 'Hoàng Văn Em' },
    { value: 6, label: 'Ngô Thị Phương' },
  ],
  classes: [
    { value: 1, label: 'CNTT01' },
    { value: 2, label: 'CNTT02' },
    { value: 3, label: 'KTPM01' },
    { value: 4, label: 'ATTT01' },
    { value: 5, label: 'CNTT03' },
    { value: 6, label: 'KHMT01' },
    { value: 7, label: 'HTTT01' },
  ],
}

function filterMockCourses(params) {
  let result = [...mockCourses]
  if (params.keyword) {
    const kw = params.keyword.toLowerCase()
    result = result.filter(c => c.tieuDe.toLowerCase().includes(kw) || c.tenMonHoc.toLowerCase().includes(kw) || c.tenLop.toLowerCase().includes(kw) || c.tenGiaoVien.toLowerCase().includes(kw))
  }
  if (params.maHocKy) result = result.filter(c => c.maHocKy === params.maHocKy)
  if (params.maMonHoc) result = result.filter(c => c.maMonHoc === params.maMonHoc)
  if (params.maGiaoVien) result = result.filter(c => c.maGiaoVien === params.maGiaoVien)
  if (params.maLop) result = result.filter(c => c.maLop === params.maLop)
  if (params.trangThai && params.trangThai !== 'all') result = result.filter(c => c.trangThai === params.trangThai)

  if (params.sortBy) {
    const dir = params.sortDir === 'desc' ? -1 : 1
    result.sort((a, b) => {
      const va = a[params.sortBy] ?? ''
      const vb = b[params.sortBy] ?? ''
      if (typeof va === 'string') return va.localeCompare(vb) * dir
      return (va - vb) * dir
    })
  }

  const pageIndex = params.pageIndex || 1
  const pageSize = params.pageSize || 20
  const totalItems = result.length
  const totalPages = Math.ceil(totalItems / pageSize)
  const start = (pageIndex - 1) * pageSize
  const items = result.slice(start, start + pageSize)

  return { success: true, message: 'Thành công', data: { items, pageIndex, pageSize, totalItems, totalPages } }
}

function delay(ms = 300) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

async function mockCall(fn) {
  await delay()
  return fn()
}

export const courseApi = {
  async getCourses(params = {}) {
    if (enableMock) return mockCall(() => filterMockCourses(params))
    const query = buildQuery(params)
    const res = await apiRequest(`/api/courses${query}`)
    return res
  },

  async getCourseDetail(id) {
    if (enableMock) return mockCall(() => {
      const course = mockCourses.find(c => c.maKhoaHoc === id)
      if (!course) throw new Error('Không tìm thấy khóa học')
      const chuongs = [
        {
          maChuong: 1, tenChuong: 'Chương 1: Giới thiệu tổng quan',
          moTa: 'Giới thiệu về môn học, mục tiêu và yêu cầu', soTiet: 5,
          baiHocs: [
            { maBaiHoc: 1, tenBaiHoc: 'Bài 1: Giới thiệu chung', loaiBai: 'ly_thuyet' },
            { maBaiHoc: 2, tenBaiHoc: 'Bài 2: Mục tiêu và chuẩn đầu ra', loaiBai: 'ly_thuyet' },
          ],
        },
        {
          maChuong: 2, tenChuong: 'Chương 2: Kiến thức nền tảng',
          moTa: 'Các khái niệm cơ bản và kiến thức nền tảng', soTiet: 10,
          baiHocs: [
            { maBaiHoc: 3, tenBaiHoc: 'Bài 3: Khái niệm cơ bản', loaiBai: 'ly_thuyet' },
            { maBaiHoc: 4, tenBaiHoc: 'Bài 4: Thực hành cơ bản', loaiBai: 'thuc_hanh' },
            { maBaiHoc: 5, tenBaiHoc: 'Bài 5: Bài tập ứng dụng', loaiBai: 'thuc_hanh' },
          ],
        },
        {
          maChuong: 3, tenChuong: 'Chương 3: Chuyên sâu',
          moTa: 'Kiến thức chuyên sâu về môn học', soTiet: 8,
          baiHocs: [
            { maBaiHoc: 6, tenBaiHoc: 'Bài 6: Lý thuyết chuyên sâu', loaiBai: 'ly_thuyet' },
            { maBaiHoc: 7, tenBaiHoc: 'Bài 7: Thực hành chuyên sâu', loaiBai: 'thuc_hanh' },
          ],
        },
      ]
      return { success: true, message: 'Thành công', data: { ...course, chuongs } }
    })
    return apiRequest(`/api/courses/${id}`)
  },

  async createCourse(payload) {
    if (enableMock) return mockCall(() => {
      const newCourse = {
        maKhoaHoc: mockCourses.length + 1,
        maDonVi: payload.maDonVi || 1,
        tenDonVi: 'Cơ sở chính',
        maMonHoc: payload.maMonHoc,
        tenMonHoc: mockDropdowns.subjects.find(s => s.value === payload.maMonHoc)?.label || '',
        maMonHocCode: '',
        maGiaoVien: payload.maGiaoVien,
        tenGiaoVien: mockDropdowns.teachers.find(t => t.value === payload.maGiaoVien)?.label || '',
        maHocKy: payload.maHocKy || null,
        tenHocKy: mockDropdowns.academicTerms.find(t => t.value === payload.maHocKy)?.label || null,
        maLop: payload.maLop,
        tenLop: mockDropdowns.classes.find(c => c.value === payload.maLop)?.label || '',
        tieuDe: payload.tieuDe || '',
        moTa: payload.moTa || null,
        trangThai: payload.trangThai || 'nhap',
        urlAnhBia: payload.urlAnhBia || null,
        ngayTao: new Date().toISOString(),
      }
      mockCourses.unshift(newCourse)
      return { success: true, message: 'Tạo khóa học thành công', data: newCourse }
    })
    return apiRequest('/api/courses', { method: 'POST', body: JSON.stringify(payload) })
  },

  async bulkAssign(payload) {
    if (enableMock) return mockCall(() => {
      const created = []
      const skipped = []
      payload.maLopIds.forEach((maLop, idx) => {
        const exists = mockCourses.find(c => c.maMonHoc === payload.maMonHoc && c.maHocKy === payload.maHocKy && c.maLop === maLop)
        if (exists) {
          skipped.push({ maLop, tenLop: mockDropdowns.classes.find(c => c.value === maLop)?.label || '', lyDo: 'Lớp đã có khóa học cho môn/học kỳ này' })
        } else {
          const newCourse = {
            maKhoaHoc: mockCourses.length + 1 + idx,
            maDonVi: payload.maDonVi || 1,
            tenDonVi: 'Cơ sở chính',
            maMonHoc: payload.maMonHoc,
            tenMonHoc: mockDropdowns.subjects.find(s => s.value === payload.maMonHoc)?.label || '',
            maMonHocCode: '',
            maGiaoVien: payload.maGiaoVien,
            tenGiaoVien: mockDropdowns.teachers.find(t => t.value === payload.maGiaoVien)?.label || '',
            maHocKy: payload.maHocKy || null,
            tenHocKy: mockDropdowns.academicTerms.find(t => t.value === payload.maHocKy)?.label || null,
            maLop: maLop,
            tenLop: mockDropdowns.classes.find(c => c.value === maLop)?.label || '',
            tieuDe: payload.tieuDe || '',
            moTa: payload.moTa || null,
            trangThai: payload.trangThai || 'nhap',
            urlAnhBia: payload.urlAnhBia || null,
            ngayTao: new Date().toISOString(),
          }
          created.push(newCourse)
        }
      })
      mockCourses.unshift(...created)
      return { success: true, message: `Tạo thành công ${created.length}/${payload.maLopIds.length} lớp`, data: { created, skipped } }
    })
    return apiRequest('/api/courses/bulk-assign', { method: 'POST', body: JSON.stringify(payload) })
  },

  async updateCourse(id, payload) {
    if (enableMock) return mockCall(() => {
      const idx = mockCourses.findIndex(c => c.maKhoaHoc === id)
      if (idx === -1) throw new Error('Không tìm thấy khóa học')
      mockCourses[idx] = { ...mockCourses[idx], ...payload }
      return { success: true, message: 'Cập nhật khóa học thành công', data: mockCourses[idx] }
    })
    return apiRequest(`/api/courses/${id}`, { method: 'PUT', body: JSON.stringify(payload) })
  },

  async archiveCourse(id) {
    if (enableMock) return mockCall(() => {
      const idx = mockCourses.findIndex(c => c.maKhoaHoc === id)
      if (idx === -1) throw new Error('Không tìm thấy khóa học')
      mockCourses[idx].trangThai = 'luu_tru'
      return { success: true, message: 'Lưu trữ khóa học thành công', data: mockCourses[idx] }
    })
    return apiRequest(`/api/courses/${id}`, { method: 'DELETE' })
  },

  async cloneCourse(id) {
    if (enableMock) return mockCall(() => {
      const source = mockCourses.find(c => c.maKhoaHoc === id)
      if (!source) throw new Error('Không tìm thấy khóa học')
      const newCourse = {
        ...source,
        maKhoaHoc: mockCourses.length + 1,
        tieuDe: `${source.tieuDe} (Bản sao)`,
        ngayTao: new Date().toISOString(),
        trangThai: 'nhap',
      }
      mockCourses.unshift(newCourse)
      return { success: true, message: 'Nhân bản khóa học thành công', data: newCourse }
    })
    return apiRequest(`/api/courses/${id}/clone`, { method: 'POST' })
  },

  async batchArchive(ids) {
    if (enableMock) return mockCall(() => {
      let count = 0
      ids.forEach(id => {
        const idx = mockCourses.findIndex(c => c.maKhoaHoc === id)
        if (idx !== -1) { mockCourses[idx].trangThai = 'luu_tru'; count++ }
      })
      return { success: true, message: `Đã lưu trữ ${count}/${ids.length} khóa học`, data: { count } }
    })
    return apiRequest('/api/courses/batch-archive', { method: 'POST', body: JSON.stringify({ ids }) })
  },

  async batchPublish(ids) {
    if (enableMock) return mockCall(() => {
      let count = 0
      ids.forEach(id => {
        const idx = mockCourses.findIndex(c => c.maKhoaHoc === id)
        if (idx !== -1) { mockCourses[idx].trangThai = 'da_xuat_ban'; count++ }
      })
      return { success: true, message: `Đã xuất bản ${count}/${ids.length} khóa học`, data: { count } }
    })
    return apiRequest('/api/courses/batch-publish', { method: 'POST', body: JSON.stringify({ ids }) })
  },
}

function buildQuery(params) {
  const entries = Object.entries(params).filter(([, v]) => v !== undefined && v !== null && v !== '')
  if (!entries.length) return ''
  return '?' + entries.map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v)}`).join('&')
}
