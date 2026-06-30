<script setup>
import { computed } from 'vue'
import NotificationInbox from '@/components/notifications/NotificationInbox.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const roleContext = computed(() => {
  const role = authStore.user?.role?.toLowerCase() || 'student'
  if (role.includes('teacher') || role.includes('giangvien')) return 'teacher'
  if (role.includes('bgh') || role.includes('principal')) return 'bgh'
  return 'student'
})
</script>

<template>
  <NotificationInbox :roleContext="roleContext" />
</template>
