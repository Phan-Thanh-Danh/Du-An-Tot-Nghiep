<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import StudentScheduleCard from './StudentScheduleCard.vue' // Reusing visual card
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { CheckSquare } from 'lucide-vue-next'

const router = useRouter()
const sessions = ref([])
const loading = ref(false)

const fetchSchedule = async () => {
  loading.value = true
  try {
    const res = await scheduleAttendanceMockService.getTeacherTodayClasses()
    sessions.value = res || []
  } finally {
    loading.value = false
  }
}
onMounted(() => fetchSchedule())

const goToAttendance = (id) => {
  router.push({ name: 'TeacherAttendanceDetail', params: { id } })
}
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <ScheduleMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Lịch giảng dạy hôm nay</h1>
    
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-3 gap-6 animate-pulse">
      <GlassPanel v-for="i in 3" :key="i" class="h-48"></GlassPanel>
    </div>
    
    <div v-else-if="sessions.length === 0" class="text-center py-12 bg-[var(--surface-card)] rounded-xl border border-[var(--border-card)]">
      <div class="text-[var(--text-muted)]">Bạn không có giờ dạy nào trong hôm nay.</div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="s in sessions" :key="s.id" class="relative">
        <StudentScheduleCard :session="s" />
        <div class="absolute bottom-4 right-4" v-if="s.trangThaiBuoi !== 'da_huy'">
          <GlassButton variant="primary" size="sm" @click="goToAttendance(s.maBuoiHoc)">
            <template #leading><CheckSquare class="w-4 h-4" /></template> Điểm danh
          </GlassButton>
        </div>
      </div>
    </div>
  </div>
</template>
