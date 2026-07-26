<template>
  <div class="proctor-page">
    <section class="proctor-header surface-card border-card">
      <div class="header-main">
        <div class="header-icon">
          <Monitor :size="24" />
        </div>
        <div>
          <p class="header-eyebrow">M4 Controlled Exam Environment</p>
          <h1>Ca canh thi được phân công</h1>
          <p>Chọn ca thi/lớp thi để điểm danh và mở màn hình giám thị realtime.</p>
        </div>
      </div>

      <div class="header-actions">
        <div class="time-chip">
          <Clock :size="18" />
          <div>
            <span>Thời gian thực</span>
            <strong>{{ currentTime }}</strong>
          </div>
        </div>
      </div>
    </section>

    <div v-if="loading" class="p-4">
      <ListSkeleton :rows="4" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[400px] gap-4">
      <AlertCircle :size="48" class="text-rose-400" />
      <p class="text-rose-600 font-semibold">{{ error }}</p>
      <button @click="loadSessions" class="btn-primary">Thử lại</button>
    </div>
    <template v-else>
      <section class="stats-grid">
        <div class="stat-card surface-card border-card">
          <span>Tổng ca</span>
          <strong>{{ sessionStats.total }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đang điểm danh</span>
          <strong>{{ sessionStats.attendance }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đang canh thi</span>
          <strong>{{ sessionStats.monitoring }}</strong>
        </div>
        <div class="stat-card surface-card border-card">
          <span>Đã kết thúc</span>
          <strong>{{ sessionStats.ended }}</strong>
        </div>
      </section>

      <section class="session-grid">
        <article
          v-for="session in assignedExamSessions"
          :key="session.id"
          class="session-card surface-card border-card"
        >
          <div class="session-card-top">
            <div>
              <p class="session-code">{{ session.subjectCode }} · {{ session.classCode }}</p>
              <h2>{{ session.examTitle }}</h2>
              <p>{{ formatSessionTime(session) }} · {{ session.room }}</p>
            </div>
            <GlassBadge :variant="sessionBadgeVariant(session.status)">
              {{ sessionStatusLabel(session.status) }}
            </GlassBadge>
          </div>

          <div class="session-metrics">
            <div>
              <span>Tổng thí sinh</span>
              <strong>{{ session.totalStudents }}</strong>
            </div>
            <div>
              <span>Đã điểm danh</span>
              <strong>-</strong>
            </div>
            <div>
              <span>Đang thi</span>
              <strong>-</strong>
            </div>
            <div>
              <span>Đã nộp</span>
              <strong>-</strong>
            </div>
            <div>
              <span>Vi phạm</span>
              <strong>-</strong>
            </div>
          </div>

          <button
            type="button"
            class="primary-action"
            :class="{ muted: session.status === 'ended' }"
            @click="openSession(session)"
          >
            <PlayCircle v-if="session.status !== 'ended'" :size="16" />
            <FileCheck2 v-else :size="16" />
            {{ sessionActionLabel(session) }}
          </button>
        </article>
      </section>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { Monitor, Clock, AlertCircle, PlayCircle, FileCheck2 } from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const loading = ref(false)
const error = ref('')
const assignedExamSessions = ref([])
const currentTime = ref('')
let clockTimer = null

const sessionStats = computed(() => {
  return {
    total: assignedExamSessions.value.length,
    attendance: assignedExamSessions.value.filter((session) => session.status === 'attendance').length,
    monitoring: assignedExamSessions.value.filter((session) => session.status === 'monitoring').length,
    ended: assignedExamSessions.value.filter((session) => session.status === 'ended').length,
  }
})

function updateTime() {
  currentTime.value = new Date().toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

async function loadSessions() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getExams()
    const rawSessions = Array.isArray(data) ? data : (data?.data?.items ?? data?.data ?? data?.items ?? [])
    assignedExamSessions.value = rawSessions
  } catch (e) {
    error.value = e?.message || 'Không thể tải ca thi.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadSessions()
  updateTime()
  clockTimer = window.setInterval(updateTime, 1000)
})

onUnmounted(() => {
  if (clockTimer) window.clearInterval(clockTimer)
})

function formatSessionTime(session) {
  const start = new Date(session.startTime)
  const end = new Date(session.endTime)
  return `${start.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`
}

function sessionStatusLabel(status) {
  if (status === 'scheduled') return 'Sắp diễn ra'
  if (status === 'attendance') return 'Đang điểm danh'
  if (status === 'monitoring') return 'Đang canh thi'
  return 'Đã kết thúc'
}

function sessionBadgeVariant(status) {
  if (status === 'monitoring') return 'success'
  if (status === 'attendance') return 'warning'
  if (status === 'ended') return 'neutral'
  return 'primary'
}

function sessionActionLabel(session) {
  if (session.status === 'ended') return 'Xem biên bản'
  if (session.status === 'monitoring') return 'Mở dashboard giám thị'
  return 'Vào điểm danh'
}

function openSession(session) {
  if (session.status === 'monitoring') {
    router.push({ name: 'teacher-proctoring-dashboard', params: { sessionId: session.id } })
  } else {
    router.push({ name: 'teacher-proctoring-attendance', params: { sessionId: session.id } })
  }
}
</script>

<style scoped>
.proctor-page {
  display: flex;
  min-height: calc(100vh - 132px);
  flex-direction: column;
  gap: 1rem;
  color: var(--text-body);
}

.border-card {
  border: 1px solid var(--border-card);
}

.proctor-header {
  border-radius: 18px;
  box-shadow: var(--lg-shadow-sm);
}

.proctor-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem;
}

.header-main,
.header-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.header-icon {
  display: grid;
  width: 2.8rem;
  height: 2.8rem;
  place-items: center;
  border-radius: 14px;
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.header-eyebrow,
.session-code {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.65rem;
  font-weight: 850;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.proctor-header h1,
.session-card h2 {
  margin: 0.1rem 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 850;
}

.proctor-header p,
.session-card p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.78rem;
  font-weight: 600;
}

.time-chip,
.primary-action {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 850;
}

.time-chip {
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.65rem 0.8rem;
}

.time-chip span {
  display: block;
  color: var(--text-muted);
  font-size: 0.58rem;
  text-transform: uppercase;
}

.time-chip strong {
  display: block;
  color: var(--text-heading);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
}

.primary-action {
  min-height: 2.4rem;
  border: 0;
  padding: 0 0.8rem;
  cursor: pointer;
  background: var(--text-link);
  color: var(--text-inverse);
}

.primary-action.muted {
  background: var(--surface-input);
  color: var(--text-label);
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.stat-card {
  min-height: 4.2rem;
  border-radius: 16px;
  padding: 0.8rem 1rem;
}

.stat-card span,
.session-metrics span {
  color: var(--text-muted);
  font-size: 0.68rem;
  font-weight: 800;
}

.stat-card strong {
  display: block;
  color: var(--text-heading);
  font-size: 1.35rem;
  font-weight: 900;
}

.session-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1rem;
}

.session-card {
  border-radius: 18px;
  padding: 1rem;
  box-shadow: var(--lg-shadow-sm);
}

.session-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.session-metrics {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 0.45rem;
  margin: 1rem 0;
}

.session-metrics div {
  border: 1px solid var(--border-default);
  border-radius: 12px;
  background: var(--surface-input);
  padding: 0.55rem;
}

.session-metrics strong {
  display: block;
  color: var(--text-heading);
  font-weight: 900;
}

@media (max-width: 980px) {
  .proctor-header,
  .header-main,
  .header-actions {
    align-items: flex-start;
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .session-metrics {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
