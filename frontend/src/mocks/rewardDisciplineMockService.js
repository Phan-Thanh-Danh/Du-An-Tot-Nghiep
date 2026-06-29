import { rewardDisciplineMockData } from './rewardDisciplineMockData'

const delay = (ms = 300) => new Promise(resolve => setTimeout(resolve, ms))

export const rewardDisciplineMockService = {
  // ── Student: Rewards ──────────────────────────────────────────────
  async getMyRewards(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentRewards, totalCount: rewardDisciplineMockData.studentRewards.length }
  },
  async getMyRewardDetail(id) {
    await delay()
    return rewardDisciplineMockData.studentRewards.find(x => x.id === id) || null
  },
  async downloadCertificateMock(_id) {
    await delay(500)
    return { url: '/mocks/cert-001.pdf' }
  },

  // ── Student: Discipline ───────────────────────────────────────────
  async getMyDisciplineRecords(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentDiscipline, totalCount: rewardDisciplineMockData.studentDiscipline.length }
  },
  async getMyDisciplineDetail(id) {
    await delay()
    return rewardDisciplineMockData.studentDiscipline.find(x => x.id === id) || null
  },
  async submitDisciplineAppeal(id, _payload) {
    await delay()
    const record = rewardDisciplineMockData.studentDiscipline.find(x => x.id === id)
    if(record) record.appealStatus = 'pending'
    return { success: true }
  },
  async getMyAppeals(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentAppeals, totalCount: rewardDisciplineMockData.studentAppeals.length }
  },

  // ── Admin: Overview ───────────────────────────────────────────────
  async getRewardDisciplineOverview(_query = {}) {
    await delay()
    return rewardDisciplineMockData.reports
  },

  // ── Admin: Reward Campaigns ───────────────────────────────────────
  async getRewardCampaigns(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.rewardCampaigns, totalCount: rewardDisciplineMockData.rewardCampaigns.length }
  },
  async getRewardCampaignDetail(id) {
    await delay()
    return rewardDisciplineMockData.rewardCampaigns.find(x => x.id === id) || null
  },
  async getRewardCandidates(campaignId, _query = {}) {
    await delay()
    const cands = rewardDisciplineMockData.candidates.filter(x => x.campaignId === campaignId)
    return { items: cands, totalCount: cands.length }
  },
  async approveCandidate(_campaignId, _candidateId) {
    await delay()
    return { success: true }
  },
  async rejectCandidate(_campaignId, _candidateId, _payload) {
    await delay()
    return { success: true }
  },
  async generateCertificates(_campaignId) {
    await delay(1000)
    return { success: true, count: 120 }
  },

  // ── Admin: Discipline Records ─────────────────────────────────────
  async getDisciplineRecords(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.disciplineRecords, totalCount: rewardDisciplineMockData.disciplineRecords.length }
  },
  async getDisciplineRecordDetail(id) {
    await delay()
    return rewardDisciplineMockData.disciplineRecords.find(x => x.id === id) || null
  },
  async createDisciplineRecord(_payload) {
    await delay()
    return { success: true, id: 'dl-new' }
  },
  async updateDisciplineRecord(_id, _payload) {
    await delay()
    return { success: true }
  },

  // ── Admin: Discipline Appeals ─────────────────────────────────────
  async getDisciplineAppeals(_query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.adminAppeals, totalCount: rewardDisciplineMockData.adminAppeals.length }
  },
  async getDisciplineAppealDetail(id) {
    await delay()
    return rewardDisciplineMockData.adminAppeals.find(x => x.id === id) || null
  },
  async resolveDisciplineAppeal(_id, _payload) {
    await delay()
    return { success: true }
  }
}
