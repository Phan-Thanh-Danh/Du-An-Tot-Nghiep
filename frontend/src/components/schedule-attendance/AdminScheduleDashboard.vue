<script setup>
import { ref, onMounted } from 'vue'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const summary = ref({ shifts: 0, schedules: 0, sessions: 0, unlocks: 0 })
const loading = ref(false)

const loadData = async () => {
  loading.value = true
  try {
    const shifts = await scheduleAttendanceMockService.getShifts()
    const sch = await scheduleAttendanceMockService.getSchedules()
    const sess = await scheduleAttendanceMockService.getSessions()
    const unl = await scheduleAttendanceMockService.getAttendanceUnlockRequests()
    summary.value = {
      shifts: shifts.items?.length || 0,
      schedules: sch.items?.length || 0,
      sessions: sess.items?.length || 0,
      unlocks: unl.items?.filter(x => x.trangThai === 'cho_duyet').length || 0
    }
  } finally {
    loading.value = false
  }
}
onMounted(() => loadData())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <ScheduleMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Dashboard Thời khóa biểu</h1>
    
    <div v-if="loading" class="grid grid-cols-2 md:grid-cols-4 gap-6 animate-pulse">
      <GlassPanel v-for="i in 4" :key="i" class="h-24"></GlassPanel>
    </div>
    <div v-else class="grid grid-cols-2 md:grid-cols-4 gap-6">
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--lg-primary)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Ca học active</div>
        <div class="text-3xl font-bold text-[var(--lg-primary)]">{{ summary.shifts }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-success-border)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">TKB xuất bản</div>
        <div class="text-3xl font-bold text-[var(--color-success-text)]">{{ summary.schedules }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-warning-border)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Buổi học (Tổng)</div>
        <div class="text-3xl font-bold text-[var(--color-warning-text)]">{{ summary.sessions }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-danger-border)] relative">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Yêu cầu mở khóa</div>
        <div class="text-3xl font-bold text-[var(--color-danger-text)]">{{ summary.unlocks }}</div>
        <div v-if="summary.unlocks > 0" class="absolute top-2 right-2 w-3 h-3 bg-[var(--color-danger-text)] rounded-full animate-pulse"></div>
      </GlassPanel>
    </div>
  </div>
</template>
