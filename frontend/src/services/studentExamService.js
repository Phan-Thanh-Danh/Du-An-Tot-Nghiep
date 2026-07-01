import { apiRequest } from '@/services/apiClient'

function unwrapApiData(response) {
  return response?.data ?? response?.Data ?? response
}

export async function getStudentExams() {
  const response = await apiRequest('/api/exam/student/list')
  return unwrapApiData(response) || []
}
