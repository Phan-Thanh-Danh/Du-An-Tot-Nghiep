import { apiRequest, unwrapApiData } from './apiClient'

const academicSchedulingApi = {
  async getContext() {
    return unwrapApiData(await apiRequest('/api/academic-scheduling/context'))
  }
}

export default academicSchedulingApi
