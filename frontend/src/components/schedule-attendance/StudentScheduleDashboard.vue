<script setup>
import { ref, onMounted } from 'vue'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import StudentScheduleCard from './StudentScheduleCard.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const sessions = ref([])
const loading = ref(false)

const fetchSchedule = async () => {
  loading.value = true
  try {
    const res = await scheduleAttendanceMockService.getStudentTodaySchedule()
    sessions.value = res || []
  } finally {
    loading.value = false
  }
}
onMounted(() => fetchSchedule())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <ScheduleMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Thời khóa biểu hôm nay</h1>
    
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-6 animate-pulse">
      <GlassPanel v-for="i in 3" :key="i" class="h-40"></GlassPanel>
    </div>
    
    <div v-else-if="sessions.length === 0" class="text-center py-12 bg-[var(--surface-card)] rounded-xl border border-[var(--border-card)]">
      <div class="text-[var(--text-muted)]">Bạn không có ca học nào trong hôm nay.</div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <StudentScheduleCard v-for="s in sessions" :key="s.id" :session="s" />
    </div>
  </div>
</template>
