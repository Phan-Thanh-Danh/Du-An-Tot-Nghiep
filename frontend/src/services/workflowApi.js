import { apiRequest, unwrapApiData } from './apiClient'

export const workflowApi = {
  async getWorkflows() {
    return unwrapApiData(await apiRequest('/api/admin/applications/workflow'))
  },

  async updateWorkflow(id, payload) {
    return unwrapApiData(await apiRequest(`/api/admin/applications/workflow/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    }))
  }
}
