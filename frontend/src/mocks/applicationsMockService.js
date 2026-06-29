import { applicationsMockData } from './applicationsMockData'

const delay = (ms = 300) => new Promise(resolve => setTimeout(resolve, ms))

export const applicationsApi = { // Kept name as applicationsApi for seamless replacement
  async getApplicationSchemaOptions() { await delay(); return [] },
  async getApplicationTemplates() { await delay(); return { items: applicationsMockData.templates } },
  async getApplicationTemplateDetail() { await delay(); return applicationsMockData.templates[0] },
  async getMyApplications() { await delay(); return { items: applicationsMockData.myApplications, total: applicationsMockData.myApplications.length } },
  async getMyApplicationDetail(id) { await delay(); return applicationsMockData.myApplications.find(x => x.id === id) || applicationsMockData.myApplications[0] },
  async createDraft() { await delay(); return { success: true } },
  async updateDraft() { await delay(); return { success: true } },
  async submitApplication() { await delay(); return { success: true } },
  async resubmitApplication() { await delay(); return { success: true } },
  async cancelApplication() { await delay(); return { success: true } },
  
  async getAdminApplications() { await delay(); return { items: applicationsMockData.adminApplications, total: applicationsMockData.adminApplications.length } },
  async getAdminApplicationSummary() { await delay(); return applicationsMockData.summary },
  async getAdminApplicationDetail(id) { await delay(); return applicationsMockData.adminApplications.find(x => x.id === id) || applicationsMockData.adminApplications[0] },
  async getAssignableUsers() { await delay(); return { items: [{ id: 'staff1', name: 'Nguyễn Văn Staff' }] } },
  async receiveApplication() { await delay(); return { success: true } },
  async assignApplication() { await delay(); return { success: true } },
  async requestSupplement() { await delay(); return { success: true } },
  async approveApplication() { await delay(); return { success: true } },
  async rejectApplication() { await delay(); return { success: true } },
  async getApplicationReportOverview() { await delay(); return { total: 100, pending: 20 } },
  async getApplicationReportByType() { await delay(); return { items: [] } },
  async getPendingApplicationReport() { await delay(); return { items: [] } },
}
