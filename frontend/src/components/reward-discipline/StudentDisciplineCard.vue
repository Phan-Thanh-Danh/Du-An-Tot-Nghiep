<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import DisciplineStatusBadge from './DisciplineStatusBadge.vue'
import { AlertTriangle, Clock } from 'lucide-vue-next'
import dayjs from 'dayjs'

defineProps({
  record: { type: Object, required: true }
})
</script>

<template>
  <GlassPanel padding="normal" class="h-full flex flex-col hover:border-[var(--color-danger-border)] transition-colors border-l-4 border-l-[var(--color-danger-border)]">
    <div class="flex justify-between items-start mb-3">
      <h3 class="font-semibold text-[var(--text-heading)] line-clamp-1 pr-2">{{ record.tieuDe }}</h3>
      <DisciplineStatusBadge :status="record.trangThai" />
    </div>
    
    <div class="flex items-center gap-2 mb-3">
      <span class="px-2 py-0.5 text-xs font-medium bg-[var(--surface-hover)] rounded">{{ record.mucDoKyLuat }}</span>
    </div>

    <div class="mt-auto space-y-2 text-sm text-[var(--text-body)]">
      <div class="flex justify-between items-center gap-2">
        <span class="text-[var(--text-muted)]">Ngày vi phạm:</span>
        <span class="font-medium text-right">{{ dayjs(record.ngayViPham).format('DD/MM/YYYY') }}</span>
      </div>
      <div class="flex justify-between items-center gap-2" v-if="record.thoiHanHieuLuc">
        <span class="text-[var(--text-muted)]">Hết hiệu lực:</span>
        <span class="font-medium flex items-center gap-1">
          <Clock class="w-3 h-3" /> {{ dayjs(record.thoiHanHieuLuc).format('DD/MM/YYYY') }}
        </span>
      </div>
    </div>
  </GlassPanel>
</template>
