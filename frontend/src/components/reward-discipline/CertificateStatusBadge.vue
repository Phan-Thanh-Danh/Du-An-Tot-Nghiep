<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  status: { type: String, required: true },
})

const badgeVariant = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'pending': return 'warning'
    case 'generated': return 'success'
    case 'failed': return 'danger'
    case 'downloaded': return 'info'
    default: return 'primary'
  }
})

const displayLabel = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'pending': return 'Chờ sinh'
    case 'generated': return 'Đã sinh'
    case 'failed': return 'Lỗi'
    case 'downloaded': return 'Đã tải'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
