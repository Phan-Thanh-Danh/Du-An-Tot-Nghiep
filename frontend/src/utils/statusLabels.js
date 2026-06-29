const DEFAULT_STATUS = {
  label: 'Không xác định',
  variant: 'neutral',
}

const statusMaps = {
  applications: {
    nhap: { label: 'Bản nháp', variant: 'neutral' },
    da_nop: { label: 'Đã nộp', variant: 'info' },
    dang_xem_xet: { label: 'Đang xem xét', variant: 'warning' },
    yeu_cau_bo_sung: { label: 'Yêu cầu bổ sung', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    da_huy: { label: 'Đã hủy', variant: 'neutral' },
    da_tiep_nhan: { label: 'Đã tiếp nhận', variant: 'info' },
    dang_xu_ly: { label: 'Đang xử lý', variant: 'warning' },
    hoan_thanh: { label: 'Hoàn thành', variant: 'success' },
    can_xu_ly_thu_cong: { label: 'Cần xử lý thủ công', variant: 'warning' },
    da_xu_ly_tu_dong: { label: 'Đã xử lý tự động', variant: 'success' },
    loi_xu_ly: { label: 'Lỗi xử lý', variant: 'danger' },
  },

  attendance: {
    co_mat: { label: 'Có mặt', variant: 'success' },
    di_muon: { label: 'Đi muộn', variant: 'warning' },
    co_phep: { label: 'Có phép', variant: 'info' },
    vang: { label: 'Vắng', variant: 'danger' },
    chua_diem_danh: { label: 'Chưa điểm danh', variant: 'neutral' },
  },

  session: {
    du_kien: { label: 'Dự kiến', variant: 'neutral' },
    chua_mo: { label: 'Chưa mở', variant: 'neutral' },
    dang_diem_danh: { label: 'Đang điểm danh', variant: 'info' },
    da_gui: { label: 'Đã gửi', variant: 'success' },
    da_khoa: { label: 'Đã khóa', variant: 'neutral' },
    huy: { label: 'Đã hủy', variant: 'danger' },
    da_huy: { label: 'Đã hủy', variant: 'danger' },
    doi_phong: { label: 'Đổi phòng', variant: 'warning' },
    doi_ca: { label: 'Đổi ca', variant: 'warning' },
    day_thay: { label: 'Dạy thay', variant: 'info' },
    binh_thuong: { label: 'Bình thường', variant: 'success' },
  },

  timetable: {
    nhap: { label: 'Bản nháp', variant: 'neutral' },
    da_xuat_ban: { label: 'Đã xuất bản', variant: 'success' },
    da_huy: { label: 'Đã hủy', variant: 'danger' },
  },

  notificationPriority: {
    thap: { label: 'Thấp', variant: 'neutral' },
    binh_thuong: { label: 'Bình thường', variant: 'info' },
    cao: { label: 'Cao', variant: 'warning' },
    khan_cap: { label: 'Khẩn cấp', variant: 'danger' },
    low: { label: 'Thấp', variant: 'neutral' },
    normal: { label: 'Bình thường', variant: 'info' },
    high: { label: 'Cao', variant: 'warning' },
    urgent: { label: 'Khẩn cấp', variant: 'danger' },
  },

  notificationCategory: {
    hoc_vu: { label: 'Học vụ', variant: 'info' },
    tai_chinh: { label: 'Tài chính', variant: 'warning' },
    lich_hoc: { label: 'Lịch học', variant: 'primary' },
    diem_danh: { label: 'Điểm danh', variant: 'violet' },
    he_thong: { label: 'Hệ thống', variant: 'neutral' },
    khen_thuong: { label: 'Khen thưởng', variant: 'success' },
    ky_luat: { label: 'Kỷ luật', variant: 'danger' },
    academic: { label: 'Học vụ', variant: 'info' },
    finance: { label: 'Tài chính', variant: 'warning' },
    schedule: { label: 'Lịch học', variant: 'primary' },
    attendance: { label: 'Điểm danh', variant: 'violet' },
    system: { label: 'Hệ thống', variant: 'neutral' },
  },

  reward: {
    nhap: { label: 'Bản nháp', variant: 'neutral' },
    dang_mo: { label: 'Đang mở', variant: 'info' },
    da_chot: { label: 'Đã chốt', variant: 'success' },
    cho_duyet: { label: 'Chờ duyệt', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    da_huy: { label: 'Đã hủy', variant: 'neutral' },
    da_phat_hanh: { label: 'Đã phát hành', variant: 'success' },
    chua_sinh_pdf: { label: 'Chưa sinh PDF', variant: 'warning' },
    da_sinh_pdf: { label: 'Đã sinh PDF', variant: 'success' },
  },

  certificate: {
    chua_sinh_pdf: { label: 'Chưa sinh PDF', variant: 'warning' },
    dang_sinh_pdf: { label: 'Đang sinh PDF', variant: 'info' },
    da_sinh_pdf: { label: 'Đã sinh PDF', variant: 'success' },
    loi_pdf: { label: 'Lỗi PDF', variant: 'danger' },
    da_phat_hanh: { label: 'Đã phát hành', variant: 'success' },
  },

  discipline: {
    nhap: { label: 'Bản nháp', variant: 'neutral' },
    dang_xu_ly: { label: 'Đang xử lý', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'danger' },
    da_go_hieu_luc: { label: 'Đã gỡ hiệu lực', variant: 'neutral' },
    da_huy: { label: 'Đã hủy', variant: 'neutral' },
    con_hieu_luc: { label: 'Còn hiệu lực', variant: 'danger' },
    het_hieu_luc: { label: 'Hết hiệu lực', variant: 'neutral' },
  },

  appeal: {
    cho_duyet: { label: 'Chờ duyệt', variant: 'warning' },
    dang_xem_xet: { label: 'Đang xem xét', variant: 'info' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    het_han: { label: 'Hết hạn', variant: 'neutral' },
  },

  unlockRequest: {
    cho_duyet: { label: 'Chờ duyệt', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    het_han: { label: 'Hết hạn', variant: 'neutral' },
  },
}

const aliases = {
  application: 'applications',
  app: 'applications',
  attendanceStatus: 'attendance',
  sessionStatus: 'session',
  schedule: 'session',
  notification_category: 'notificationCategory',
  notification_priority: 'notificationPriority',
  rewards: 'reward',
  disciplineRecord: 'discipline',
  disciplineAppeal: 'appeal',
}

export function normalizeStatusKey(status) {
  if (status === null || status === undefined) return ''
  return String(status).trim().toLowerCase()
}

export function getStatusMeta(group, status, fallback = DEFAULT_STATUS) {
  const groupKey = aliases[group] || group
  const map = statusMaps[groupKey] || {}
  const key = normalizeStatusKey(status)
  return map[key] || {
    ...fallback,
    label: key ? fallback.label : 'Chưa có trạng thái',
  }
}

export function getStatusLabel(group, status) {
  return getStatusMeta(group, status).label
}

export function getStatusVariant(group, status) {
  return getStatusMeta(group, status).variant
}

export function getStatusOptions(group) {
  const groupKey = aliases[group] || group
  const map = statusMaps[groupKey] || {}
  return Object.entries(map).map(([value, meta]) => ({
    value,
    label: meta.label,
    variant: meta.variant,
  }))
}

export { statusMaps }
