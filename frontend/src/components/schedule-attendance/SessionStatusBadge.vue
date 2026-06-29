<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  status: { type: String, required: true },
})

const badgeVariant = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'sap_dien_ra': return 'info'
    case 'dang_dien_ra': return 'warning'
    case 'da_hoan_thanh': return 'success'
    case 'da_huy': return 'danger'
    default: return 'primary'
  }
})

const displayLabel = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'sap_dien_ra': return 'Sắp diễn ra'
    case 'dang_dien_ra': return 'Đang diễn ra'
    case 'da_hoan_thanh': return 'Đã hoàn thành'
    case 'da_huy': return 'Đã hủy'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
