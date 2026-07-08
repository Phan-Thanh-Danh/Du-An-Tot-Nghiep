import { apiRequest } from '@/services/apiClient'


// Dữ liệu mock học phí fallback khi backend không khả dụng
const mockInvoices = [
  {
    maHoaDon: 1,
    maHoaDonCode: 'HD-2026-001',
    hocKy: 'HK2 2025-2026',
    soTien: 18500000,
    giamTru: 0,
    daThanhToan: 12000000,
    soTienPhaiDong: 18500000,
    conPhaiDong: 6500000,
    trangThai: 'thanh_toan_mot_phan',
    hanThanhToan: '2026-05-25'
  }
]

const mockTransactions = [
  {
    maGiaoDich: 101,
    maThamChieuNoiBo: 'GD-2026-001',
    ngayTao: '2026-05-10T10:30:00',
    soTien: 12000000,
    nhaCungCapThanhToan: 'payos',
    trangThai: 'thanh_cong'
  }
]

function unwrap(response) {
  return response?.data ?? response
}

export async function getStudentTuitionInvoices() {
  try {
    return unwrap(await apiRequest('/api/student/tuition/invoices', { method: 'GET' }))
  } catch (e) {
    throw e
  }
}

export async function getStudentTuitionTransactions() {
  try {
    return unwrap(await apiRequest('/api/student/tuition/transactions', { method: 'GET' }))
  } catch (e) {
    throw e
  }
}

export async function createTuitionPayment(invoiceId, provider) {
  try {
    return unwrap(
      await apiRequest(`/api/student/tuition/invoices/${invoiceId}/payments`, {
        method: 'POST',
        body: JSON.stringify({ provider }),
      }),
    )
  } catch (e) {
    throw e
  }
}

export const tuitionService = {
  getStudentTuitionInvoices,
  getStudentTuitionTransactions,
  createTuitionPayment,
  getTuitionPaymentStatus,
}
