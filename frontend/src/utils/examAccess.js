export const EXAM_STATUS = {
  DRAFT: 'draft',
  SCHEDULED: 'scheduled',
  OPEN: 'open',
  CLOSED: 'closed',
  RESULT_PUBLISHED: 'result_published',
}

const statusLabels = {
  [EXAM_STATUS.DRAFT]: 'Nháp',
  [EXAM_STATUS.SCHEDULED]: 'Đã lên lịch',
  [EXAM_STATUS.OPEN]: 'Đang mở',
  [EXAM_STATUS.CLOSED]: 'Đã đóng',
  [EXAM_STATUS.RESULT_PUBLISHED]: 'Đã công bố kết quả',
}

export function getExamStatusLabel(status) {
  return statusLabels[status] || statusLabels[normalizeExamStatus(status)] || 'Không xác định'
}

export function normalizeExamStatus(status) {
  const value = String(status || '').toLowerCase()
  if (value === 'published') return EXAM_STATUS.OPEN
  if (value === 'draft') return EXAM_STATUS.DRAFT
  if (['upcoming', 'not_open', 'scheduled'].includes(value)) return EXAM_STATUS.SCHEDULED
  if (['expired', 'closed'].includes(value)) return EXAM_STATUS.CLOSED
  if (['completed', 'awaiting_result', 'result_published'].includes(value)) return EXAM_STATUS.RESULT_PUBLISHED
  return value || EXAM_STATUS.SCHEDULED
}

function isBeforeOpen(exam, now) {
  return exam.openAt && now < new Date(exam.openAt)
}

function isAfterClose(exam, now) {
  return exam.closeAt && now > new Date(exam.closeAt)
}

export function getExamAccessState(exam, studentContext = {}, now = new Date()) {
  const policy = exam.accessPolicy || {}
  const status = normalizeExamStatus(exam.status)
  const usedAttempts = Number(exam.usedAttempts ?? exam.attempts ?? 0)
  const maxAttempts = Number(exam.maxAttempts ?? 1)

  if (policy.allowedByClassSection === false) {
    return {
      canEnter: false,
      reason: 'Bạn chưa thuộc lớp học phần được phép làm đề này.',
      state: 'class_denied',
    }
  }

  if (
    studentContext.classSectionCode &&
    exam.classSectionCode &&
    studentContext.classSectionCode !== exam.classSectionCode &&
    policy.allowedByClassSection !== true
  ) {
    return {
      canEnter: false,
      reason: 'Bạn chưa thuộc lớp học phần được phép làm đề này.',
      state: 'class_denied',
    }
  }

  if (status === EXAM_STATUS.DRAFT || status === EXAM_STATUS.SCHEDULED || isBeforeOpen(exam, now)) {
    return {
      canEnter: false,
      reason: 'Đề thi chưa mở.',
      state: 'not_open',
    }
  }

  if (status === EXAM_STATUS.CLOSED || isAfterClose(exam, now)) {
    return {
      canEnter: false,
      reason: 'Đề thi đã đóng.',
      state: 'closed',
    }
  }

  if (status === EXAM_STATUS.RESULT_PUBLISHED) {
    return {
      canEnter: false,
      canViewResult: true,
      reason: 'Kết quả đã được công bố, bạn chỉ có thể xem kết quả.',
      state: 'result_published',
    }
  }

  if (Number.isFinite(maxAttempts) && usedAttempts >= maxAttempts) {
    return {
      canEnter: false,
      reason: 'Bạn đã hết số lần làm.',
      state: 'attempt_limit',
    }
  }

  return {
    canEnter: true,
    reason: '',
    state: 'allowed',
  }
}
