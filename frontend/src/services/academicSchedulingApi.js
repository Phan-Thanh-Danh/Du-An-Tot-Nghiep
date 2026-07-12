import { apiRequest, unwrapApiData } from './apiClient'

const academicSchedulingApi = {
  async getContext() {
    return unwrapApiData(await apiRequest('/api/academic-scheduling/context'))
  },
  async getMajors() {
    const data = unwrapApiData(await apiRequest('/api/master-data/majors'))
    return data.items || data.Items || data
  },
  async getSpecializations(maNganh) {
    if (!maNganh) return []
    const data = unwrapApiData(await apiRequest(`/api/master-data/specializations?maNganh=${maNganh}`))
    return data.items || data.Items || data
  },
  async getClassesBySpecialization(maChuyenNganh) {
    if (!maChuyenNganh) return []
    const data = unwrapApiData(await apiRequest(`/api/lop-hanh-chinh?maChuyenNganh=${maChuyenNganh}&conHoatDong=true`))
    return data.items || data.Items || data
  }
}

export default academicSchedulingApi
