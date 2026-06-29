<script setup>

import { Clock, CheckCircle2, XCircle, AlertCircle, PlayCircle, Send, UserCheck, Inbox } from 'lucide-vue-next'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

defineProps({
  events: { type: Array, default: () => [] }
})

const getIcon = (type) => {
  switch(type) {
    case 'TAO_MOI': return PlayCircle
    case 'NOP_DON': return Send
    case 'TIEP_NHAN': return Inbox
    case 'PHAN_CONG': return UserCheck
    case 'YEU_CAU_BO_SUNG': return AlertCircle
    case 'NOP_LAI': return Send
    case 'DUYET': return CheckCircle2
    case 'TU_CHOI': return XCircle
    case 'XU_LY_NGHIEP_VU': return Clock
    default: return Clock
  }
}

const getColorClass = (type) => {
  switch(type) {
    case 'TAO_MOI': return 'text-[var(--text-muted)] bg-[var(--surface-hover)]'
    case 'NOP_DON': return 'text-[var(--lg-primary)] bg-[var(--lg-primary)] bg-opacity-10'
    case 'TIEP_NHAN': return 'text-[var(--color-info-text)] bg-[var(--color-info-bg)]'
    case 'DUYET': return 'text-[var(--color-success-text)] bg-[var(--color-success-bg)]'
    case 'TU_CHOI': return 'text-[var(--color-danger-text)] bg-[var(--color-danger-bg)]'
    case 'YEU_CAU_BO_SUNG': return 'text-[var(--color-warning-text)] bg-[var(--color-warning-bg)]'
    default: return 'text-[var(--lg-primary)] bg-[var(--surface-hover)]'
  }
}
</script>

<template>
  <div class="relative border-l border-[var(--border-card)] ml-4 space-y-6">
    <div v-for="(event, idx) in events" :key="event.id || idx" class="relative pl-6">
      <div class="absolute -left-[1.1rem] p-1 rounded-full flex items-center justify-center border-4 border-[var(--surface-page)]" :class="getColorClass(event.loaiSuKien)">
        <component :is="getIcon(event.loaiSuKien)" class="w-4 h-4" />
      </div>
      <div>
        <div class="text-sm font-semibold text-[var(--text-heading)]">
          {{ event.tieuDe || event.loaiSuKien }}
        </div>
        <div class="text-xs text-[var(--text-muted)] mt-1 flex items-center gap-2">
          <span>{{ dayjs(event.thoiGian).format('DD/MM/YYYY HH:mm') }}</span>
          <span>•</span>
          <span>Bởi: {{ event.nguoiThucHien || 'Hệ thống' }}</span>
        </div>
        <div v-if="event.ghiChu" class="mt-2 p-3 bg-[var(--surface-hover)] rounded-lg text-sm text-[var(--text-body)]">
          {{ event.ghiChu }}
        </div>
      </div>
    </div>
    
    <div v-if="events.length === 0" class="pl-6 text-sm text-[var(--text-muted)]">
      Chưa có lịch sử hoạt động.
    </div>
  </div>
</template>
