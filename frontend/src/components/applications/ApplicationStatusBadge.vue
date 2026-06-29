<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  status: { type: String, required: true },
})

const badgeVariant = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'nhap': return 'secondary'
    case 'da_nop': return 'info'
    case 'dang_xem_xet': return 'warning'
    case 'yeu_cau_bo_sung': return 'warning'
    case 'da_duyet': return 'success'
    case 'tu_choi': return 'danger'
    case 'da_huy': return 'secondary'
    case 'xu_ly_thanh_cong': return 'success'
    case 'xu_ly_that_bai': return 'danger'
    case 'chua_xu_ly': return 'secondary'
    case 'cho_xu_ly': return 'info'
    case 'can_xu_ly_thu_cong': return 'warning'
    case 'da_ghi_nhan': return 'success'
    default: return 'primary'
  }
})

const displayLabel = computed(() => {
  const s = props.status.toLowerCase()
  switch (s) {
    case 'nhap': return 'Nháp'
    case 'da_nop': return 'Đã nộp'
    case 'dang_xem_xet': return 'Đang xem xét'
    case 'yeu_cau_bo_sung': return 'Cần bổ sung'
    case 'da_duyet': return 'Đã duyệt'
    case 'tu_choi': return 'Từ chối'
    case 'da_huy': return 'Đã hủy'
    case 'xu_ly_thanh_cong': return 'Xử lý thành công'
    case 'xu_ly_that_bai': return 'Xử lý thất bại'
    case 'chua_xu_ly': return 'Chưa xử lý'
    case 'cho_xu_ly': return 'Chờ xử lý'
    case 'can_xu_ly_thu_cong': return 'Cần xử lý thủ công'
    case 'da_ghi_nhan': return 'Đã ghi nhận'
    default: return props.status
  }
})
</script>

<template>
  <GlassBadge :variant="badgeVariant">{{ displayLabel }}</GlassBadge>
</template>
