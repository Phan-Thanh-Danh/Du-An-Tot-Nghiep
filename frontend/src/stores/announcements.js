import { defineStore } from 'pinia'
import { ref } from 'vue'

const DISMISSED_KEY = 'lms_announcements_dismissed'

function loadDismissed() {
  try {
    const raw = localStorage.getItem(DISMISSED_KEY)
    return raw ? JSON.parse(raw) : []
  } catch {
    return []
  }
}

function persistDismissed(ids) {
  localStorage.setItem(DISMISSED_KEY, JSON.stringify(ids))
}

export const useAnnouncementsStore = defineStore('announcements', () => {
  const dismissedIds = ref(loadDismissed())

  const announcements = ref([
    {
      id: 'maintenance-1',
      type: 'info',
      message: 'Hệ thống sẽ bảo trì định kỳ vào Chủ nhật (31/05) từ 02:00 - 04:00.',
      dismissable: true,
    },
  ])

  const visibleAnnouncements = ref([...announcements.value])

  function dismiss(id) {
    if (!dismissedIds.value.includes(id)) {
      dismissedIds.value.push(id)
      persistDismissed(dismissedIds.value)
    }
    visibleAnnouncements.value = announcements.value.filter(
      a => dismissedIds.value.includes(a.id) === false
    )
  }

  function addAnnouncement(ann) {
    announcements.value.push(ann)
    visibleAnnouncements.value = announcements.value.filter(
      a => dismissedIds.value.includes(a.id) === false
    )
  }

  visibleAnnouncements.value = announcements.value.filter(
    a => dismissedIds.value.includes(a.id) === false
  )

  return { announcements, visibleAnnouncements, dismiss, addAnnouncement }
})
