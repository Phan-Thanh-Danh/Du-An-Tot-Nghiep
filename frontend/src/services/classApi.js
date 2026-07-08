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

function normalizeClass(item) {
  return {
    ...item,
    maKhoa: item.maDonVi,
    tenKhoa: item.tenDonVi,
    maKhoaHoc: item.namNhapHoc,
    khoaHoc: item.namNhapHoc ? `K${String(item.namNhapHoc).slice(-2)}` : '',
    siSo: item.siSo ?? 0,
    siSoToiDa: item.siSoToiDa ?? 0,
    maGiaoVien: item.maGiaoVienChuNhiem,
    tenGiaoVien: item.tenGiaoVienChuNhiem,
    namNhapHoc: item.namNhapHoc ? String(item.namNhapHoc) : '',
    trangThai: item.conHoatDong === false ? 'da_bi_huy' : 'dang_hoc',
  }
}

function normalizeClassList(response) {
  const data = unwrap(response)
  const items = Array.isArray(data) ? data : data?.items || []
  return items.map(normalizeClass)
}

function toClassPayload(data) {
  return {
    maDonVi: Number(data.maDonVi || data.maKhoa || 1),
    maCodeLop: data.maCodeLop,
    tenLop: data.tenLop,
    maGiaoVienChuNhiem: data.maGiaoVien || null,
    maChuongTrinh: data.maChuongTrinh || null,
    namNhapHoc: data.namNhapHoc ? Number(data.namNhapHoc) : null,
    conHoatDong: data.trangThai !== 'da_bi_huy',
  }
}

function unavailable() {
  throw new Error('Chức năng đang phát triển')
}

export const classApi = {
  list(params = {}) {
    const apiParams = {
      PageSize: params.PageSize || 100,
      Keyword: params.Search,
      MaDonVi: params.MaKhoa,
      ConHoatDong: params.TrangThai === 'da_bi_huy' ? false : undefined,
    }
    return apiRequest(`/api/admin/classes${buildQuery(apiParams)}`).then(normalizeClassList)
  },

  get(id) {
    return apiRequest(`/api/admin/classes/${id}`).then(res => normalizeClass(unwrap(res)))
  },

  create(data) {
    return apiRequest('/api/admin/classes', {
      method: 'POST',
      body: JSON.stringify(toClassPayload(data)),
    }).then(res => normalizeClass(unwrap(res)))
  },

  update(id, data) {
    return apiRequest(`/api/admin/classes/${id}`, {
      method: 'PUT',
      body: JSON.stringify(toClassPayload(data)),
    }).then(res => normalizeClass(unwrap(res)))
  },

  delete(id) {
    return apiRequest(`/api/admin/classes/${id}`, { method: 'DELETE' })
  },

  getStudents() {
    return unavailable()
  },
}
