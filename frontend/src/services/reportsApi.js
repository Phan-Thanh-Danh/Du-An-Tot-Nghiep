import { apiRequest } from './apiClient'

export default {
  getCourseReport() {
    return apiRequest('/api/reports/courses')
  },
  getTeacherLoadReport() {
    return apiRequest('/api/reports/teacher-load')
  },
  getAttendanceReport() {
    return apiRequest('/api/reports/attendance')
  }
}
