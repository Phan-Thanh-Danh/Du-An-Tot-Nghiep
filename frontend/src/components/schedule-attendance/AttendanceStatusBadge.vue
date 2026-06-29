<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  status: { type: String, default: '' },
})

const badgeVariant = computed(() => {
  if (!props.status) return 'secondary'
  const s = props.status.toLowerCase()
  switch (s) {
    case 'co_mat': return 'success'
    case 'di_muon': return 'warning'
    case 'co_phep': return 'info'
    case 'vang': return 'danger'
    default: return 'secondary'
  }
})

const displayLabel = computed(() => {
  if (!props.status) return 'Chưa điểm danh'
  const s = props.status.toLowerCase()
  switch (s) {
    case 'co_mat': return 'Có mặt'
    case 'di_muon': return 'Đi muộn'
    case 'co_phep': return 'Có phép'
    case 'vang': return 'Vắng mặt'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
