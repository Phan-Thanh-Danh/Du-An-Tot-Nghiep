<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import SessionStatusBadge from './SessionStatusBadge.vue'
import { MapPin, User, Clock, AlertTriangle } from 'lucide-vue-next'
import dayjs from 'dayjs'

defineProps({
  session: { type: Object, required: true }
})
</script>

<template>
  <GlassPanel padding="normal" class="h-full flex flex-col hover:border-[var(--lg-primary)] transition-colors" :class="{'opacity-60 grayscale': session.trangThaiBuoi === 'da_huy'}">
    <div class="flex justify-between items-start mb-2">
      <h3 class="font-bold text-[var(--text-heading)] line-clamp-1 pr-2">{{ session.tenMon }}</h3>
      <SessionStatusBadge :status="session.trangThaiBuoi" />
    </div>
    
    <div class="text-xs font-semibold text-[var(--lg-primary)] mb-3">{{ session.maTkb }} - Lớp: {{ session.lop }}</div>
    
    <div v-if="session.loaiThayDoi !== 'none'" class="mb-3 text-xs p-2 bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] rounded flex items-center gap-1">
      <AlertTriangle class="w-3 h-3" />
      <span v-if="session.loaiThayDoi === 'doi_phong'">Đổi phòng: {{ session.lyDoThayDoi }}</span>
      <span v-else-if="session.loaiThayDoi === 'day_thay'">Dạy thay: {{ session.giaoVienDayThay }}</span>
      <span v-else-if="session.loaiThayDoi === 'huy_buoi'">Hủy: {{ session.lyDoThayDoi }}</span>
      <span v-else>{{ session.loaiThayDoi }}</span>
    </div>

    <div class="mt-auto space-y-2 text-sm text-[var(--text-body)]">
      <div class="flex items-center gap-2">
        <Clock class="w-4 h-4 text-[var(--text-muted)]" />
        <span>{{ dayjs(session.ngayHoc).format('DD/MM/YYYY') }} ({{ session.caHoc }})</span>
      </div>
      <div class="flex items-center gap-2">
        <MapPin class="w-4 h-4 text-[var(--text-muted)]" />
        <span>{{ session.phong }}</span>
      </div>
      <div class="flex items-center gap-2">
        <User class="w-4 h-4 text-[var(--text-muted)]" />
        <span>{{ session.giaoVienDayThay || session.giaoVien }}</span>
      </div>
    </div>
  </GlassPanel>
</template>
