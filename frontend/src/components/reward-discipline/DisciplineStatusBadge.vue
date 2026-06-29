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
    case 'pending_approval': return 'warning'
    case 'approved': return 'info'
    case 'active': return 'danger'
    case 'expired': return 'secondary'
    case 'removed': return 'success'
    case 'cancelled': return 'secondary'
    case 'rejected': return 'secondary'
    case 'pending': return 'warning'
    case 'accepted': return 'success'
    default: return 'primary'
  }
})

const displayLabel = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'draft': return 'Nháp'
    case 'pending_approval': return 'Chờ duyệt'
    case 'approved': return 'Đã duyệt'
    case 'active': return 'Đang hiệu lực'
    case 'expired': return 'Hết hiệu lực'
    case 'removed': return 'Đã gỡ'
    case 'cancelled': return 'Đã hủy'
    case 'rejected': return 'Từ chối'
    case 'pending': return 'Chờ xử lý'
    case 'accepted': return 'Đã chấp nhận'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
