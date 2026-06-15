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

export async function createTuitionPayment(invoiceId, provider = 'payos') {
  return unwrap(
    await apiRequest(`/api/student/tuition/invoices/${invoiceId}/payments`, {
      method: 'POST',
      body: JSON.stringify({ provider }),
    }),
  )
}

export async function getTuitionPaymentStatus(transactionId) {
  return unwrap(
    await apiRequest(`/api/student/tuition/payments/${transactionId}`, {
      method: 'GET',
    }),
  )
}

export const tuitionService = {
  getStudentTuitionInvoices,
  getStudentTuitionTransactions,
  createTuitionPayment,
  getTuitionPaymentStatus,
}
