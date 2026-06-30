<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ApplicationStatusBadge from './ApplicationStatusBadge.vue'
import { FileText, Clock, AlertTriangle } from 'lucide-vue-next'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

defineProps({
  application: { type: Object, required: true }
})
</script>

<template>
  <GlassPanel 
    padding="compact" 
    class="cursor-pointer hover:border-(--lg-primary) transition-colors group h-full flex flex-col"
  >
    <div class="flex justify-between items-start mb-2">
      <div class="flex items-center gap-2">
        <div class="p-2 bg-(--surface-hover) rounded-lg group-hover:bg-(--lg-primary) group-hover:bg-opacity-10 group-hover:text-(--lg-primary) transition-colors">
          <FileText class="w-4 h-4" />
        </div>
        <div>
          <h4 class="font-semibold text-sm text-(--text-heading) line-clamp-1">{{ application.tieuDe || 'Đơn từ' }}</h4>
          <span class="text-xs text-(--text-muted)">{{ application.maDon }}</span>
        </div>
      </div>
      <ApplicationStatusBadge :status="application.trangThai" />
    </div>

    <div class="mt-auto pt-3 border-t border-(--border-card) flex justify-between items-center text-xs">
      <div class="flex items-center gap-1 text-(--text-muted)">
        <Clock class="w-3 h-3" />
        {{ dayjs(application.ngayTao).fromNow() }}
      </div>
      
      <div v-if="application.trangThai === 'YEU_CAU_BO_SUNG'" class="flex items-center gap-1 text-(--color-warning-text)">
        <AlertTriangle class="w-3 h-3" />
        Cần xử lý ngay
      </div>
    </div>
  </GlassPanel>
</template>
