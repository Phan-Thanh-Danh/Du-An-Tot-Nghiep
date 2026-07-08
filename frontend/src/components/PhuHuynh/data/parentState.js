export function getStoredActiveChildId() {
  return Number(localStorage.getItem('parent_active_student_id')) || null
}

export function setActiveChildId(id) {
  if (id == null) return
  localStorage.setItem('parent_active_student_id', String(id))
}
