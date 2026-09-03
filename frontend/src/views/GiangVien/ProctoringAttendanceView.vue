<template>
  <div class="proctor-page">
    <section class="proctor-header surface-card border-card">
      <div class="header-main">
        <div class="header-icon">
          <Monitor :size="24" />
        </div>
        <div>
          <p class="header-eyebrow">M4 Controlled Exam Environment</p>
          <h1>Điểm danh thí sinh dự thi</h1>
          <p>Xác nhận thí sinh có mặt trước khi đưa vào grid giám sát.</p>
        </div>
      </div>

      <div class="header-actions">
        <button
          type="button"
          class="ghost-action"
          @click="goBack"
        >
          <LogOut :size="16" />
          Quay lại danh sách ca
        </button>

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
      <button @click="goBack" class="btn-primary">Quay lại</button>
    </div>
    <template v-else-if="currentSession">
      <section class="attendance-shell surface-card border-card">
        <div class="section-toolbar">
          <div>
            <p class="section-eyebrow">{{ currentSession.subjectCode }} · {{ currentSession.classCode }}</p>
            <h2>Điểm danh thí sinh dự thi</h2>
            <p>{{ currentSession.examTitle }} · {{ formatSessionTime(currentSession) }} · {{ currentSession.room }}</p>
          </div>
          <div class="flex flex-wrap items-center justify-end gap-2">
            <button
              type="button"
              class="ghost-action"
              :disabled="currentStudents.length === 0"
              @click="markAllPresent"
            >
              <CheckCheck :size="16" />
              Điểm danh tất cả có mặt
            </button>
            <button
              type="button"
              class="primary-action"
              :disabled="attendanceStats.present === 0"
              @click="startMonitoring"
            >
              <LayoutGrid :size="16" />
              Bắt đầu canh thi
            </button>
          </div>
        </div>

        <div class="stats-grid compact">
          <div class="stat-card surface-input border-card">
            <span>Tổng</span>
            <strong>{{ attendanceStats.total }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Có mặt</span>
            <strong>{{ attendanceStats.present }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Vắng mặt</span>
            <strong>{{ attendanceStats.absent }}</strong>
          </div>
          <div class="stat-card surface-input border-card">
            <span>Rủi ro pre-flight</span>
            <strong>{{ attendanceStats.risk }}</strong>
          </div>
        </div>

        <div class="student-table">
          <div class="student-row table-head">
            <span>MSSV</span>
            <span>Họ tên</span>
            <span>Điểm danh</span>
            <span>Pre-flight</span>
            <span>Stream</span>
            <span>Hành động</span>
          </div>
          <div
            v-for="student in currentStudents"
            :key="student.id"
            class="student-row"
          >
            <strong>{{ student.studentCode }}</strong>
            <span>{{ student.name }}</span>
            <GlassBadge :variant="attendanceBadgeVariant(student.attendanceStatus)">
              {{ attendanceLabel(student.attendanceStatus) }}
            </GlassBadge>
            <span :class="['status-text', student.preflightStatus]">
              {{ preflightLabel(student.preflightStatus) }}
            </span>
            <span>{{ streamLabel(student.streamStatus) }}</span>
            <div class="row-actions">
              <button type="button" @click="setAttendance(student, 'present')">Có mặt</button>
              <button type="button" @click="setAttendance(student, 'absent')">Vắng mặt</button>
              <button type="button" @click="setAttendance(student, 'exempted')">Miễn thi</button>
            </div>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Monitor, Clock, LogOut, AlertCircle, CheckCheck, LayoutGrid } from 'lucide-vue-next'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { usePopupStore } from '@/stores/popup'
import { teacherApi } from '@/services/teacherApi'
import { useProctoringSession } from '@/composables/useProctoringSession'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const {
  loading,
  error,
  currentSession,
  currentStudents,
  loadSessionData,
  formatSessionTime,
  streamLabel,
  attendanceLabel,
  attendanceBadgeVariant,
  preflightLabel,
} = useProctoringSession()

const currentTime = ref('')
let clockTimer = null

const presentStudents = computed(() => {
  return currentStudents.value.filter((student) => student.attendanceStatus === 'present')
})

const attendanceStats = computed(() => {
  const total = currentStudents.value.length
  const present = currentStudents.value.filter((student) => student.attendanceStatus === 'present').length
  const exempted = currentStudents.value.filter((student) => student.attendanceStatus === 'exempted').length
  const risk = currentStudents.value.filter((student) => student.preflightStatus === 'risk').length
  return {
    total,
    present,
    absent: total - present - exempted,
    risk,
  }
})

function updateTime() {
  currentTime.value = new Date().toLocaleTimeString('vi-VN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

onMounted(() => {
  const sessionId = route.params.sessionId
  if (sessionId) {
    loadSessionData(sessionId)
  }
  updateTime()
  clockTimer = window.setInterval(updateTime, 1000)
})

onUnmounted(() => {
  if (clockTimer) window.clearInterval(clockTimer)
})

function goBack() {
  router.push({ name: 'teacher-proctoring-sessions' })
}

function setAttendance(student, status) {
  student.attendanceStatus = status
  if (status !== 'present') {
    student.streamStatus = 'waiting'
    student.examStatus = 'not_started'
  }
}

function markAllPresent() {
  currentStudents.value.forEach((student) => {
    student.attendanceStatus = 'present'
  })
}

async function startMonitoring() {
  if (!currentSession.value || attendanceStats.value.present === 0) {
    popupStore.warning('Chưa thể bắt đầu', 'Cần điểm danh ít nhất 1 thí sinh có mặt.')
    return
  }

  try {
    const attendancePayload = {
      maCaThi: currentSession.value.id,
      danhSachDiemDanh: currentStudents.value.map(s => {
        let mappedStatus = 'vang_mat';
        if (s.attendanceStatus === 'present') mappedStatus = 'co_mat';
        else if (s.attendanceStatus === 'late') mappedStatus = 'di_muon_qua_gio';
        else if (s.attendanceStatus === 'problem') mappedStatus = 'su_co';
        else mappedStatus = 'vang_mat';

        return {
          maHocSinh: s.studentId || s.id,
          trangThaiDiemDanh: mappedStatus,
          ghiChu: ''
        };
      })
    }

    await teacherApi.batchExamAttendance(attendancePayload)
    await teacherApi.startExamSession(currentSession.value.id)

    popupStore.success('Đã bắt đầu canh thi', `${presentStudents.value.length} thí sinh có mặt được đưa vào grid giám sát.`)
    router.push({ name: 'teacher-proctoring-dashboard', params: { sessionId: currentSession.value.id } })
  } catch (err) {
    popupStore.error('Lỗi', 'Không thể bắt đầu ca thi: ' + (err?.response?.data?.message || err.message || 'Lỗi không xác định'))
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

.proctor-header,
.attendance-shell {
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
.header-actions,
.section-toolbar {
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
.section-eyebrow {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.65rem;
  font-weight: 850;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.proctor-header h1,
.attendance-shell h2 {
  margin: 0.1rem 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 850;
}

.proctor-header p,
.attendance-shell p {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.78rem;
  font-weight: 600;
}

.time-chip,
.ghost-action,
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

.ghost-action,
.primary-action {
  min-height: 2.4rem;
  border: 1px solid var(--border-card);
  padding: 0 0.8rem;
  cursor: pointer;
}

.ghost-action {
  background: var(--surface-input);
  color: var(--text-label);
}

.primary-action {
  border: 0;
  background: var(--text-link);
  color: var(--text-inverse);
}

.primary-action:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.stats-grid.compact {
  margin-top: 1rem;
}

.stat-card {
  min-height: 4.2rem;
  border-radius: 16px;
  padding: 0.8rem 1rem;
}

.stat-card span {
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

.attendance-shell {
  padding: 1rem;
}

.student-table {
  margin-top: 1rem;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 16px;
}

.student-row {
  display: grid;
  grid-template-columns: 0.8fr 1.3fr 0.8fr 0.9fr 0.9fr 1.55fr;
  align-items: center;
  gap: 0.75rem;
  border-top: 1px solid var(--border-default);
  padding: 0.75rem;
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 700;
}

.student-row:first-child {
  border-top: 0;
}

.table-head {
  background: var(--surface-input);
  color: var(--text-muted);
  font-size: 0.68rem;
  font-weight: 900;
  text-transform: uppercase;
}

.row-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.row-actions button {
  min-height: 2rem;
  border: 1px solid var(--border-card);
  border-radius: 10px;
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.68rem;
  font-weight: 850;
  cursor: pointer;
  padding: 0 0.55rem;
}

.status-text.pass {
  color: var(--color-success-text);
}

.status-text.risk {
  color: var(--color-danger-text);
}

@media (max-width: 980px) {
  .proctor-header,
  .header-main,
  .header-actions,
  .section-toolbar {
    align-items: flex-start;
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .student-row {
    grid-template-columns: 1fr;
  }

  .table-head {
    display: none;
  }
}
</style>
