export const LEARNING_ACCESS = {
  OFFICIAL: 'official',
  EARLY_AVAILABLE: 'early_available',
  EARLY_COMPLETED: 'early_completed',
  LOCKED_PREREQUISITE: 'locked_prerequisite',
  FUTURE_LOCKED: 'future_locked',
  COMPLETED: 'completed',
}

export function canStartLearning(item) {
  return [LEARNING_ACCESS.OFFICIAL, LEARNING_ACCESS.EARLY_AVAILABLE].includes(item?.accessStatus)
}

export function canViewLearningResult(item) {
  return [LEARNING_ACCESS.COMPLETED, LEARNING_ACCESS.EARLY_COMPLETED].includes(item?.accessStatus)
}

export function isLocked(item) {
  return [LEARNING_ACCESS.LOCKED_PREREQUISITE, LEARNING_ACCESS.FUTURE_LOCKED].includes(item?.accessStatus)
}

export function needsEarlyLearningConfirm(item) {
  return item?.accessStatus === LEARNING_ACCESS.EARLY_AVAILABLE
}

export function getLockedReason(item) {
  if (item?.lockedReason) return item.lockedReason
  if (item?.accessStatus === LEARNING_ACCESS.FUTURE_LOCKED) {
    return `Nội dung này sẽ mở ở ${item.semesterName || 'kỳ học tới'} - ${item.blockName || 'block tới'}.`
  }
  if (item?.accessStatus === LEARNING_ACCESS.LOCKED_PREREQUISITE) {
    return 'Bạn chưa đủ điều kiện mở bài này.'
  }
  return ''
}
