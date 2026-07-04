import { apiRequest } from './apiClient'

function buildQuery(params = {}) {
  const query = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') query.append(key, value)
  })
  const qs = query.toString()
  return qs ? `?${qs}` : ''
}

export const rewardDisciplineApi = {
  getRewardCampaigns(params = {}) {
    return apiRequest(`/api/admin/reward-campaigns${buildQuery(params)}`)
  },

  getRewardCampaignCandidates(campaignId, params = {}) {
    return apiRequest(`/api/admin/reward-campaigns/${campaignId}/candidates${buildQuery(params)}`)
  },

  approveRewardCampaign(campaignId) {
    return apiRequest(`/api/admin/reward-campaigns/${campaignId}/approve`, {
      method: 'POST',
    })
  },

  generateRewardCertificates(campaignId, payload = {}) {
    return apiRequest(`/api/admin/reward-campaigns/${campaignId}/certificates/generate`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getDisciplineRecords(params = {}) {
    return apiRequest(`/api/admin/discipline-records${buildQuery(params)}`)
  },

  removeDisciplineEffect(recordId, payload) {
    return apiRequest(`/api/admin/discipline-records/${recordId}/remove-effect`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getDisciplineAppeals(params = {}) {
    return apiRequest(`/api/admin/discipline-appeals${buildQuery(params)}`)
  },

  resolveDisciplineAppeal(appealId, payload) {
    return apiRequest(`/api/admin/discipline-appeals/${appealId}/resolve`, {
      method: 'POST',
      body: JSON.stringify(payload),
    })
  },

  getOverview(params = {}) {
    return apiRequest(`/api/admin/reward-discipline/reports/overview${buildQuery(params)}`)
  },

  getTopStudents(params = {}) {
    return apiRequest(`/api/admin/reward-discipline/reports/top-students${buildQuery(params)}`)
  },
}
