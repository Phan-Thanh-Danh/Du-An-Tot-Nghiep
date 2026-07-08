import { apiRequest } from '@/services/apiClient'

function unwrap(response) {
  return response?.data ?? response
}

export async function getStudentTuitionInvoices() {
  return unwrap(await apiRequest('/api/student/tuition/invoices', { method: 'GET' }))
}

export async function getStudentTuitionTransactions() {
  return unwrap(await apiRequest('/api/student/tuition/transactions', { method: 'GET' }))
}

export async function createTuitionPayment(invoiceId, provider) {
  return unwrap(
    await apiRequest(`/api/student/tuition/invoices/${invoiceId}/payments`, {
      method: 'POST',
      body: JSON.stringify({ provider }),
    }),
  )
}

export async function getTuitionPaymentStatus(paymentId) {
  return unwrap(await apiRequest(`/api/student/tuition/payments/${paymentId}`, { method: 'GET' }))
}

export const tuitionService = {
  getStudentTuitionInvoices,
  getStudentTuitionTransactions,
  createTuitionPayment,
  getTuitionPaymentStatus,
}
