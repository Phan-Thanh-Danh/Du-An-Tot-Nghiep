<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  status: { type: String, required: true },
})

const badgeVariant = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'draft': return 'secondary'
    case 'evaluating': return 'info'
    case 'pending_approval': return 'warning'
    case 'pending': return 'warning'
    case 'approved': return 'success'
    case 'issued': return 'primary'
    case 'cancelled': return 'secondary'
    case 'eligible': return 'info'
    case 'excluded': return 'danger'
    case 'rejected': return 'danger'
    default: return 'primary'
  }
})

const displayLabel = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'draft': return 'Nháp'
    case 'evaluating': return 'Đang xét'
    case 'pending_approval': return 'Chờ duyệt'
    case 'pending': return 'Chờ duyệt'
    case 'approved': return 'Đã duyệt'
    case 'issued': return 'Đã cấp'
    case 'cancelled': return 'Đã hủy'
    case 'eligible': return 'Đủ điều kiện'
    case 'excluded': return 'Bị loại'
    case 'rejected': return 'Từ chối'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
