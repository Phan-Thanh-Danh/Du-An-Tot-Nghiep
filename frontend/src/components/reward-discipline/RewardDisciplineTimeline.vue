<script setup>
import { Clock, CheckCircle2, XCircle, FileText } from 'lucide-vue-next'
import dayjs from 'dayjs'

defineProps({
  events: { type: Array, default: () => [] }
})

const getIcon = (type) => {
  switch(type) {
    case 'TAO_MOI': return FileText
    case 'DUYET': return CheckCircle2
    case 'TU_CHOI': return XCircle
    default: return Clock
  }
}
</script>

<template>
  <div class="relative border-l border-(--border-card) ml-4 space-y-6">
    <div v-for="(event, idx) in events" :key="idx" class="relative pl-6">
      <div class="absolute -left-[1.1rem] p-1 rounded-full flex items-center justify-center border-4 border-(--surface-page) text-(--lg-primary) bg-(--surface-hover)">
        <component :is="getIcon(event.loaiSuKien)" class="w-4 h-4" />
      </div>
      <div>
        <div class="text-sm font-semibold text-(--text-heading)">
          {{ event.tieuDe || event.loaiSuKien }}
        </div>
        <div class="text-xs text-(--text-muted) mt-1 flex items-center gap-2">
          <span>{{ dayjs(event.thoiGian).format('DD/MM/YYYY HH:mm') }}</span>
        </div>
        <div v-if="event.ghiChu" class="mt-2 p-3 bg-(--surface-hover) rounded-lg text-sm text-(--text-body)">
          {{ event.ghiChu }}
        </div>
      </div>
    </div>
    
    <div v-if="events.length === 0" class="pl-6 text-sm text-(--text-muted)">
      Chưa có lịch sử.
    </div>
  </div>
</template>
