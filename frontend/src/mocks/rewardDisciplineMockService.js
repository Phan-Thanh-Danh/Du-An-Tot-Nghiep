import { rewardDisciplineMockData } from './rewardDisciplineMockData'

const delay = (ms = 300) => new Promise(resolve => setTimeout(resolve, ms))

export const rewardDisciplineMockService = {
  // ── Student: Rewards ──────────────────────────────────────────────
  async getMyRewards(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentRewards, totalCount: rewardDisciplineMockData.studentRewards.length }
  },
  async getMyRewardDetail(id) {
    await delay()
    return rewardDisciplineMockData.studentRewards.find(x => x.id === id) || null
  },
  async downloadCertificateMock(id) {
    await delay(500)
    return { url: '/mocks/cert-001.pdf' }
  },

  // ── Student: Discipline ───────────────────────────────────────────
  async getMyDisciplineRecords(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentDiscipline, totalCount: rewardDisciplineMockData.studentDiscipline.length }
  },
  async getMyDisciplineDetail(id) {
    await delay()
    return rewardDisciplineMockData.studentDiscipline.find(x => x.id === id) || null
  },
  async submitDisciplineAppeal(id, payload) {
    await delay()
    const record = rewardDisciplineMockData.studentDiscipline.find(x => x.id === id)
    if(record) record.appealStatus = 'pending'
    return { success: true }
  },
  async getMyAppeals(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.studentAppeals, totalCount: rewardDisciplineMockData.studentAppeals.length }
  },

  // ── Admin: Overview ───────────────────────────────────────────────
  async getRewardDisciplineOverview(query = {}) {
    await delay()
    return rewardDisciplineMockData.reports
  },

  // ── Admin: Reward Campaigns ───────────────────────────────────────
  async getRewardCampaigns(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.rewardCampaigns, totalCount: rewardDisciplineMockData.rewardCampaigns.length }
  },
  async getRewardCampaignDetail(id) {
    await delay()
    return rewardDisciplineMockData.rewardCampaigns.find(x => x.id === id) || null
  },
  async getRewardCandidates(campaignId, query = {}) {
    await delay()
    const cands = rewardDisciplineMockData.candidates.filter(x => x.campaignId === campaignId)
    return { items: cands, totalCount: cands.length }
  },
  async approveCandidate(campaignId, candidateId) {
    await delay()
    return { success: true }
  },
  async rejectCandidate(campaignId, candidateId, payload) {
    await delay()
    return { success: true }
  },
  async generateCertificates(campaignId) {
    await delay(1000)
    return { success: true, count: 120 }
  },

  // ── Admin: Discipline Records ─────────────────────────────────────
  async getDisciplineRecords(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.disciplineRecords, totalCount: rewardDisciplineMockData.disciplineRecords.length }
  },
  async getDisciplineRecordDetail(id) {
    await delay()
    return rewardDisciplineMockData.disciplineRecords.find(x => x.id === id) || null
  },
  async createDisciplineRecord(payload) {
    await delay()
    return { success: true, id: 'dl-new' }
  },
  async updateDisciplineRecord(id, payload) {
    await delay()
    return { success: true }
  },

  // ── Admin: Discipline Appeals ─────────────────────────────────────
  async getDisciplineAppeals(query = {}) {
    await delay()
    return { items: rewardDisciplineMockData.adminAppeals, totalCount: rewardDisciplineMockData.adminAppeals.length }
  },
  async getDisciplineAppealDetail(id) {
    await delay()
    return rewardDisciplineMockData.adminAppeals.find(x => x.id === id) || null
  },
  async resolveDisciplineAppeal(id, payload) {
    await delay()
    return { success: true }
  }
}
