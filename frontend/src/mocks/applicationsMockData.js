export const applicationsMockData = {
  templates: [
    { id: 't1', code: 'DT-01', name: 'Đơn xin nghỉ học tạm thời', type: 'hoc_vu', processingDays: 3, formSchema: [] },
    { id: 't2', code: 'DT-02', name: 'Đơn phúc khảo điểm', type: 'khao_thi', processingDays: 5, formSchema: [] }
  ],
  myApplications: [
    { id: 'a1', templateName: 'Đơn xin nghỉ học tạm thời', status: 'dang_xem_xet', submittedAt: '2025-10-01T10:00:00Z', updatedAt: '2025-10-02T10:00:00Z' },
    { id: 'a2', templateName: 'Đơn phúc khảo điểm', status: 'da_duyet', submittedAt: '2025-09-15T10:00:00Z', updatedAt: '2025-09-18T10:00:00Z' }
  ],
  adminApplications: [
    { id: 'a1', studentName: 'Nguyễn Văn A', studentCode: 'SE150001', templateName: 'Đơn xin nghỉ học tạm thời', status: 'dang_xem_xet', submittedAt: '2025-10-01T10:00:00Z', assignee: null },
    { id: 'a3', studentName: 'Trần Thị B', studentCode: 'SE150002', templateName: 'Đơn phúc khảo điểm', status: 'nhap', submittedAt: '2025-10-05T10:00:00Z', assignee: 'Staff_1' }
  ],
  summary: {
    total: 2,
    pending: 1,
    approved: 1,
    rejected: 0,
    requiresSupplement: 0
  }
}
