<script setup>
import { ref, onMounted } from 'vue'
import { rewardDisciplineApi } from '@/services/rewardDisciplineApi'
import { unwrapApiData } from '@/services/apiClient'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { Trophy, AlertTriangle, Users, FileText } from 'lucide-vue-next'

const stats = ref({
  totalRewards: 0,
  totalDiscipline: 0,
  pendingAppeals: 0,
  certificatesGenerated: 0,
  topRewardStudents: [],
})
const loading = ref(false)
const error = ref('')

const fetchStats = async () => {
  loading.value = true
  error.value = ''
  try {
    const [overviewRes, topStudentsRes] = await Promise.all([
      rewardDisciplineApi.getOverview(),
      rewardDisciplineApi.getTopStudents({ mode: 'reward', limit: 3 }),
    ])
    const overview = unwrapApiData(overviewRes) || {}
    const topStudents = unwrapApiData(topStudentsRes) || []
    stats.value = {
      totalRewards: overview.totalRewards ?? overview.TotalRewards ?? 0,
      totalDiscipline: overview.totalActiveDisciplineRecords ?? overview.TotalActiveDisciplineRecords ?? 0,
      pendingAppeals: overview.pendingDisciplineAppeals ?? overview.PendingDisciplineAppeals ?? 0,
      certificatesGenerated: overview.totalCertificateGenerated ?? overview.TotalCertificateGenerated ?? 0,
      topRewardStudents: (Array.isArray(topStudents) ? topStudents : []).map((student) => ({
        studentId: student.studentCode ?? student.StudentCode ?? student.studentId ?? student.StudentId,
        name: student.fullName ?? student.FullName ?? 'Sinh viên',
        score: student.score ?? student.Score ?? 0,
      })),
    }
  } catch (err) {
    error.value = err?.message || 'Không thể tải tổng quan khen thưởng - kỷ luật.'
  } finally {
    loading.value = false
  }
}
onMounted(() => fetchStats())
</script>

<template>
  <div class="p-6 h-full bg-(--surface-page) overflow-y-auto custom-scrollbar">
    <h1 class="text-2xl font-bold text-(--text-heading) mb-6">Tổng quan Khen thưởng - Kỷ luật</h1>
    
    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-4 gap-4 animate-pulse mb-6">
      <GlassPanel v-for="i in 4" :key="i" class="h-24"></GlassPanel>
    </div>
    
    <div v-else-if="error" class="rounded-lg border border-(--border-default) bg-(--surface-card) p-4 text-sm text-(--color-danger-text) mb-6">
      {{ error }}
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-amber-500">
        <div class="p-3 bg-amber-500/10 rounded-full text-amber-500"><Trophy class="w-6 h-6" /></div>
        <div>
          <div class="text-xs text-(--text-muted) font-semibold mb-1 uppercase">Tổng khen thưởng</div>
          <div class="text-2xl font-bold text-(--text-heading)">{{ stats.totalRewards }}</div>
        </div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-(--color-danger-border)">
        <div class="p-3 bg-(--color-danger-bg) rounded-full text-(--color-danger-text)"><AlertTriangle class="w-6 h-6" /></div>
        <div>
          <div class="text-xs text-(--text-muted) font-semibold mb-1 uppercase">Tổng kỷ luật</div>
          <div class="text-2xl font-bold text-(--text-heading)">{{ stats.totalDiscipline }}</div>
        </div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-(--color-warning-border)">
        <div class="p-3 bg-(--color-warning-bg) rounded-full text-(--color-warning-text)"><FileText class="w-6 h-6" /></div>
        <div>
          <div class="text-xs text-(--text-muted) font-semibold mb-1 uppercase">Khiếu nại chờ xử lý</div>
          <div class="text-2xl font-bold text-(--text-heading)">{{ stats.pendingAppeals }}</div>
        </div>
      </GlassPanel>
      <GlassPanel padding="normal" class="flex items-center gap-4 border-l-4 border-(--color-success-border)">
        <div class="p-3 bg-(--color-success-bg) rounded-full text-(--color-success-text)"><Users class="w-6 h-6" /></div>
        <div>
          <div class="text-xs text-(--text-muted) font-semibold mb-1 uppercase">Bằng khen đã cấp</div>
          <div class="text-2xl font-bold text-(--text-heading)">{{ stats.certificatesGenerated }}</div>
        </div>
      </GlassPanel>
    </div>

    <!-- Bảng vàng (Mock) -->
    <h2 class="text-xl font-bold text-(--text-heading) mb-4 mt-8">Top Sinh Viên Nổi Bật</h2>
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <GlassPanel v-for="st in stats.topRewardStudents" :key="st.studentId" padding="normal" class="flex items-center gap-3 hover:border-amber-500/50 transition-colors">
        <div class="w-10 h-10 bg-amber-500/20 text-amber-600 rounded-full flex items-center justify-center font-bold">{{ st.name.charAt(0) }}</div>
        <div>
          <div class="font-semibold text-(--text-heading)">{{ st.name }}</div>
          <div class="text-xs text-(--text-muted)">{{ st.studentId }} - Điểm thành tích: {{ st.score }}</div>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>
