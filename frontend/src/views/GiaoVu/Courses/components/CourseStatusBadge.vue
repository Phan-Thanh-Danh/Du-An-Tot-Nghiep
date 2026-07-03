<script setup>
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { computed } from 'vue'

const props = defineProps({
  status: { type: String, required: true },
})

const statusMap = {
  nhap: { label: 'Nháp', variant: 'neutral' },
  da_xuat_ban: { label: 'Đã xuất bản', variant: 'success' },
  daXuatBan: { label: 'Đã xuất bản', variant: 'success' },
  luu_tru: { label: 'Lưu trữ', variant: 'default' },
  luuTru: { label: 'Lưu trữ', variant: 'default' },
}

const resolved = computed(() => statusMap[props.status] || { label: props.status, variant: 'neutral' })
</script>

<template>
  <GlassBadge :variant="resolved.variant" size="sm">
    {{ resolved.label }}
  </GlassBadge>
</template>
