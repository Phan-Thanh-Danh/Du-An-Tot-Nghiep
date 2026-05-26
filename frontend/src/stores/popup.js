import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

let nextId = 0

export const usePopupStore = defineStore('popup', () => {
  const notifications = ref([])

  const hasNotifications = computed(() => notifications.value.length > 0)

  function show({ type = 'info', title, message, duration = 4000 } = {}) {
    const id = ++nextId
    notifications.value.push({ id, type, title, message })
    if (duration > 0) {
      setTimeout(() => dismiss(id), duration)
    }
    return id
  }

  function success(title, message, duration) {
    return show({ type: 'success', title, message, duration })
  }

  function error(title, message, duration) {
    return show({ type: 'error', title, message, duration })
  }

  function warning(title, message, duration) {
    return show({ type: 'warning', title, message, duration })
  }

  function info(title, message, duration) {
    return show({ type: 'info', title, message, duration })
  }

  function dismiss(id) {
    const idx = notifications.value.findIndex((n) => n.id === id)
    if (idx !== -1) notifications.value.splice(idx, 1)
  }

  function clear() {
    notifications.value.splice(0)
  }

  return {
    notifications,
    hasNotifications,
    show,
    success,
    error,
    warning,
    info,
    dismiss,
    clear,
  }
})
