import { apiRequest } from '@/services/apiClient'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

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
    if (!ENABLE_MOCK_API) throw e
    console.warn('Failed to fetch tuition invoices, using DEV mock data:', e)
    return mockInvoices
  }
}

export async function getStudentTuitionTransactions() {
  try {
    return unwrap(await apiRequest('/api/student/tuition/transactions', { method: 'GET' }))
  } catch (e) {
    if (!ENABLE_MOCK_API) throw e
    console.warn('Failed to fetch tuition transactions, using DEV mock data:', e)
    return mockTransactions
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
    if (!ENABLE_MOCK_API) throw e
    console.warn('Failed to create payment, using DEV mock response:', e)
    if (provider === 'payos') {
      return {
        checkoutUrl: 'https://pay.payos.vn/web/mock-payment-link-2026'
      }
    } else {
      return {
        qrUrl: 'https://api.vietqr.io/image/970415-113366668888-hPaN4A.jpg?accountName=TRUONG%20DAI%20HOC%20EDULMS&amount=6500000&addInfo=Thanh%20toan%20hoc%20phi%20HK2%202025-2026',
        amount: 6500000,
        noiDungChuyenKhoan: 'SV2026001 NGUYEN VAN AN DONG HOC PHI'
      }
    }
  }
}

export async function getTuitionPaymentStatus(transactionId) {
  try {
    return unwrap(
      await apiRequest(`/api/student/tuition/payments/${transactionId}`, {
        method: 'GET',
      }),
    )
  } catch (e) {
    if (!ENABLE_MOCK_API) throw e
    console.warn('Failed to fetch payment status, using DEV mock status:', e)
    return { trangThai: 'thanh_cong' }
  }
}

export const tuitionService = {
  getStudentTuitionInvoices,
  getStudentTuitionTransactions,
  createTuitionPayment,
  getTuitionPaymentStatus,
}
