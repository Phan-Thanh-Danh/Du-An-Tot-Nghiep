<script setup>
import { computed, ref, watch } from 'vue'
import {
  AlertTriangle,
  CalendarDays,
  CheckCircle2,
  Clock3,
  LockKeyhole,
  MapPin,
  Search,
  Send,
  ShieldCheck,
  UnlockKeyhole,
} from 'lucide-vue-next'

import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { getTeacherTodaySessions } from '@/mocks/scheduleAttendanceMockData'
import { usePopupStore } from '@/stores/popup'
import { formatTimeRange, formatWeekdayDate } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'

const popupStore = usePopupStore()

const loading = ref(false)
const error = ref('')
const searchQuery = ref('')
const statusFilter = ref('')
const selectedSessionId = ref('')
const isSubmitDialogOpen = ref(false)

const sessions = ref(getTeacherTodaySessions())

const attendanceOptions = getStatusOptions('attendance').filter((option) =>
  ['co_mat', 'di_muon', 'co_phep', 'vang'].includes(option.value),
)

const selectedSession = computed(() =>
  sessions.value.find((session) => session.id === selectedSessionId.value) || sessions.value[0] || null,
)

watch(
  sessions,
  (items) => {
    if (!items.length) {
      selectedSessionId.value = ''
      return
    }

    if (!selectedSessionId.value || !items.some((session) => session.id === selectedSessionId.value)) {
      selectedSessionId.value = items[0].id
    }
  },
  { immediate: true },
)

const summaryCards = computed(() => {
  const total = sessions.value.length
  const active = sessions.value.filter((session) => session.status === 'dang_diem_danh').length
  const submitted = sessions.value.filter((session) => session.status === 'da_gui').length
  const needsAction = sessions.value.filter((session) =>
    ['chua_mo', 'da_khoa'].includes(session.status),
  ).length

  return [
    { label: 'Lớp hôm nay', value: total, icon: CalendarDays, variant: 'primary' },
    { label: 'Đang mở', value: active, icon: Clock3, variant: 'info' },
    { label: 'Đã gửi', value: submitted, icon: CheckCircle2, variant: 'success' },
    { label: 'Cần xử lý', value: needsAction, icon: AlertTriangle, variant: 'warning' },
  ]
})

const selectedStats = computed(() => {
  const students = selectedSession.value?.students || []
  const total = students.length
  const present = students.filter((student) => student.status === 'co_mat').length
  const late = students.filter((student) => student.status === 'di_muon').length
  const excused = students.filter((student) => student.status === 'co_phep').length
  const absent = students.filter((student) => student.status === 'vang').length
  const undecided = students.filter((student) => student.status === 'chua_diem_danh').length

  return { total, present, late, excused, absent, undecided }
})

const filteredStudents = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  return (selectedSession.value?.students || []).filter((student) => {
    const matchesSearch =
      !query ||
      student.name.toLowerCase().includes(query) ||
      student.studentCode.toLowerCase().includes(query) ||
      student.className.toLowerCase().includes(query)
    const matchesStatus = !statusFilter.value || student.status === statusFilter.value
    return matchesSearch && matchesStatus
  })
})

const canEditSelected = computed(() => selectedSession.value?.status === 'dang_diem_danh')
const canOpenSelected = computed(() => selectedSession.value?.status === 'chua_mo')
const canSubmitSelected = computed(() => selectedSession.value?.status === 'dang_diem_danh')
const canRequestUnlock = computed(() => ['da_gui', 'da_khoa'].includes(selectedSession.value?.status))

function sessionStatusMeta(status) {
  return getStatusMeta('session', status)
}

function attendanceStatusMeta(status) {
  return getStatusMeta('attendance', status)
}

function selectSession(sessionId) {
  selectedSessionId.value = sessionId
  searchQuery.value = ''
  statusFilter.value = ''
}

function updateStudentStatus(studentId, status) {
  if (!canEditSelected.value) return
  const student = selectedSession.value?.students.find((item) => item.id === studentId)
  if (!student) return
  student.status = status
  if (status === 'co_mat' && student.note === 'Cần xác nhận lý do vắng') student.note = ''
}

function openAttendance() {
  if (!selectedSession.value || !canOpenSelected.value) return
  selectedSession.value.status = 'dang_diem_danh'
  selectedSession.value.students.forEach((student) => {
    if (student.status === 'chua_diem_danh') student.status = 'co_mat'
  })
  popupStore.success('Đã mở điểm danh', `Buổi ${selectedSession.value.className} đã sẵn sàng cập nhật.`)
}

function markAllCoMat() {
  if (!canEditSelected.value || !selectedSession.value) return
  selectedSession.value.students.forEach((student) => {
    student.status = 'co_mat'
    if (student.note === 'Cần xác nhận lý do vắng') student.note = ''
  })
  popupStore.success('Đã cập nhật nhanh', 'Tất cả sinh viên trong buổi đã được đánh dấu có mặt.')
}

function markPendingVang() {
  if (!canEditSelected.value || !selectedSession.value) return
  selectedSession.value.students.forEach((student) => {
    if (student.status === 'chua_diem_danh') {
      student.status = 'vang'
      student.note = 'Chưa chọn trạng thái khi chốt nhanh'
    }
  })
  popupStore.warning('Đã xử lý nhanh', 'Các sinh viên chưa chọn trạng thái đã được đánh dấu vắng.')
}

function submitSelectedAttendance() {
  if (!selectedSession.value || !canSubmitSelected.value) return
  selectedSession.value.status = 'da_gui'
  isSubmitDialogOpen.value = false
  popupStore.success('Đã gửi điểm danh', `Điểm danh lớp ${selectedSession.value.className} đã được gửi.`)
}

function requestUnlock() {
  if (!selectedSession.value || !canRequestUnlock.value) return
  popupStore.info('Đã tạo yêu cầu mẫu', 'Yêu cầu mở khóa sẽ được xử lý ở màn lịch sử điểm danh.')
}

function retryLoad() {
  loading.value = true
  error.value = ''
  window.setTimeout(() => {
    sessions.value = getTeacherTodaySessions()
    loading.value = false
  }, 350)
}
</script>

<template>
  <div class="teacher-attendance-page lg-page-enter mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <CalendarDays :size="15" />
          {{ formatWeekdayDate(new Date()) }}
        </p>
        <div>
          <h1>Điểm danh hôm nay</h1>
          <p>Chọn buổi học, cập nhật trạng thái sinh viên và gửi điểm danh trong một màn thao tác nhanh.</p>
        </div>
      </div>

      <GlassBadge :variant="selectedSession ? sessionStatusMeta(selectedSession.status).variant : 'neutral'" size="md">
        {{ selectedSession ? sessionStatusMeta(selectedSession.status).label : 'Chưa có buổi học' }}
      </GlassBadge>
    </GlassPanel>

    <section class="summary-grid" aria-label="Tổng quan điểm danh hôm nay">
      <GlassPanel
        v-for="card in summaryCards"
        :key="card.label"
        variant="flat"
        density="compact"
        class="summary-card"
      >
        <div class="summary-icon">
          <component :is="card.icon" :size="18" />
        </div>
        <div class="min-w-0">
          <p>{{ card.label }}</p>
          <strong>{{ card.value }}</strong>
        </div>
        <GlassBadge :variant="card.variant">{{ card.label }}</GlassBadge>
      </GlassPanel>
    </section>

    <GlassPanel v-if="error" variant="flat" density="compact" class="error-panel">
      <div>
        <h2>Không tải được dữ liệu điểm danh</h2>
        <p>{{ error }}</p>
      </div>
      <GlassButton variant="secondary" @click="retryLoad">Thử lại</GlassButton>
    </GlassPanel>

    <LoadingSkeleton v-else-if="loading" :lines="6" />

    <EmptyState
      v-else-if="!sessions.length"
      title="Không có lớp hôm nay"
      description="Lịch giảng dạy hôm nay chưa có buổi học cần điểm danh."
    >
      <GlassButton variant="secondary" @click="retryLoad">Tải lại lịch</GlassButton>
    </EmptyState>

    <section v-else class="main-grid">
      <GlassPanel variant="flat" density="compact" class="session-list-panel">
        <div class="panel-heading">
          <div>
            <h2>Buổi học hôm nay</h2>
            <p>{{ sessions.length }} buổi theo lịch giảng dạy.</p>
          </div>
          <GlassBadge variant="info">Mock UI</GlassBadge>
        </div>

        <div class="session-list">
          <button
            v-for="session in sessions"
            :key="session.id"
            type="button"
            :class="['session-card', session.id === selectedSession?.id ? 'is-active' : '']"
            @click="selectSession(session.id)"
          >
            <span class="session-card-top">
              <span class="session-time">{{ session.shift.label }} · {{ formatTimeRange(session.startAt, session.endAt) }}</span>
              <GlassBadge :variant="sessionStatusMeta(session.status).variant">
                {{ sessionStatusMeta(session.status).label }}
              </GlassBadge>
            </span>
            <span class="session-title clamp-2">{{ session.subject }}</span>
            <span class="session-meta">
              <span>{{ session.className }}</span>
              <span>
                <MapPin :size="13" />
                {{ session.room }}
              </span>
            </span>
            <span v-if="session.changeStatus" class="session-change">
              <GlassBadge :variant="sessionStatusMeta(session.changeStatus).variant">
                {{ sessionStatusMeta(session.changeStatus).label }}
              </GlassBadge>
              <span class="clamp-1">{{ session.changeNote }}</span>
            </span>
          </button>
        </div>
      </GlassPanel>

      <div class="workspace">
        <GlassPanel v-if="selectedSession" variant="flat" density="compact" class="session-summary-panel">
          <div class="selected-info">
            <div>
              <h2 class="clamp-1">{{ selectedSession.subject }}</h2>
              <p>
                {{ selectedSession.className }} · {{ selectedSession.shift.label }} ·
                {{ formatTimeRange(selectedSession.startAt, selectedSession.endAt) }} · {{ selectedSession.room }}
              </p>
            </div>
            <GlassBadge :variant="sessionStatusMeta(selectedSession.status).variant" size="md">
              {{ sessionStatusMeta(selectedSession.status).label }}
            </GlassBadge>
          </div>

          <div class="stats-grid">
            <div class="stat-item">
              <span>Tổng số</span>
              <strong>{{ selectedStats.total }}</strong>
            </div>
            <div class="stat-item success">
              <span>Có mặt</span>
              <strong>{{ selectedStats.present }}</strong>
            </div>
            <div class="stat-item warning">
              <span>Đi muộn</span>
              <strong>{{ selectedStats.late }}</strong>
            </div>
            <div class="stat-item info">
              <span>Có phép</span>
              <strong>{{ selectedStats.excused }}</strong>
            </div>
            <div class="stat-item danger">
              <span>Vắng</span>
              <strong>{{ selectedStats.absent }}</strong>
            </div>
            <div class="stat-item">
              <span>Chưa chọn</span>
              <strong>{{ selectedStats.undecided }}</strong>
            </div>
          </div>

          <p v-if="selectedSession.status === 'da_khoa'" class="lock-note">
            <LockKeyhole :size="15" />
            {{ selectedSession.lockedReason || 'Buổi học đã khóa, không thể chỉnh sửa trực tiếp.' }}
          </p>

          <div class="summary-actions">
            <GlassButton v-if="canOpenSelected" variant="primary" @click="openAttendance">
              <template #leading>
                <ShieldCheck :size="16" />
              </template>
              Mở điểm danh
            </GlassButton>
            <GlassButton
              v-if="canSubmitSelected"
              variant="success"
              @click="isSubmitDialogOpen = true"
            >
              <template #leading>
                <Send :size="16" />
              </template>
              Gửi điểm danh
            </GlassButton>
            <GlassButton
              v-if="canRequestUnlock"
              variant="secondary"
              :disabled="selectedSession.status === 'da_gui'"
              @click="requestUnlock"
            >
              <template #leading>
                <UnlockKeyhole :size="16" />
              </template>
              Yêu cầu mở khóa
            </GlassButton>
          </div>
        </GlassPanel>

        <GlassPanel v-if="selectedSession" variant="flat" density="compact" class="attendance-table-panel">
          <div class="toolbar-grid">
            <label class="control-field">
              <span>Tìm sinh viên</span>
              <span class="search-control">
                <Search :size="15" />
                <input v-model="searchQuery" type="text" placeholder="Tên, MSSV hoặc lớp" />
              </span>
            </label>
            <label class="control-field">
              <span>Trạng thái</span>
              <select v-model="statusFilter" class="lg-control">
                <option value="">Tất cả</option>
                <option v-for="option in attendanceOptions" :key="option.value" :value="option.value">
                  {{ option.label }}
                </option>
              </select>
            </label>
            <div class="toolbar-actions">
              <GlassButton variant="secondary" :disabled="!canEditSelected" @click="markAllCoMat">
                Tất cả có mặt
              </GlassButton>
              <GlassButton variant="ghost" :disabled="!canEditSelected" @click="markPendingVang">
                Chưa chọn là vắng
              </GlassButton>
            </div>
          </div>

          <TableShell v-if="filteredStudents.length" density="compact">
            <table>
              <thead>
                <tr>
                  <th class="col-index">STT</th>
                  <th>Sinh viên</th>
                  <th>Mã SV</th>
                  <th>Lớp</th>
                  <th>Trạng thái</th>
                  <th>Ghi chú</th>
                  <th>Cập nhật</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(student, index) in filteredStudents" :key="student.id">
                  <td class="col-index">{{ index + 1 }}</td>
                  <td>
                    <div class="student-cell">
                      <span class="avatar">{{ student.name.split(' ').pop()?.[0] }}</span>
                      <strong class="clamp-1">{{ student.name }}</strong>
                    </div>
                  </td>
                  <td class="muted-cell">{{ student.studentCode }}</td>
                  <td>
                    <GlassBadge variant="primary">{{ student.className }}</GlassBadge>
                  </td>
                  <td>
                    <GlassBadge :variant="attendanceStatusMeta(student.status).variant">
                      {{ attendanceStatusMeta(student.status).label }}
                    </GlassBadge>
                  </td>
                  <td>
                    <span class="note-text clamp-1" :title="student.note">{{ student.note || 'Không có ghi chú' }}</span>
                  </td>
                  <td>
                    <div class="status-segment" :aria-disabled="!canEditSelected">
                      <button
                        v-for="option in attendanceOptions"
                        :key="option.value"
                        type="button"
                        :disabled="!canEditSelected"
                        :class="['segment-button', student.status === option.value ? 'is-active' : '']"
                        @click="updateStudentStatus(student.id, option.value)"
                      >
                        {{ option.label }}
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>

          <EmptyState
            v-else
            title="Không tìm thấy sinh viên"
            description="Thử đổi từ khóa hoặc bộ lọc trạng thái điểm danh."
          />
        </GlassPanel>

        <EmptyState
          v-else
          title="Chọn một buổi học"
          description="Danh sách sinh viên và công cụ điểm danh sẽ xuất hiện sau khi chọn buổi học bên trái."
        />
      </div>
    </section>

    <ConfirmActionDialog
      v-model="isSubmitDialogOpen"
      title="Gửi điểm danh buổi học?"
      :message="`Sau khi gửi, buổi ${selectedSession?.className || ''} sẽ chuyển sang trạng thái đã gửi và không thể cập nhật trực tiếp.`"
      confirm-label="Gửi điểm danh"
      cancel-label="Kiểm tra lại"
      variant="success"
      @confirm="submitSelectedAttendance"
    />
  </div>
</template>

<style scoped>
.teacher-attendance-page {
  color: var(--text-body);
}

.page-header,
.panel-heading,
.selected-info,
.summary-actions,
.session-card-top,
.session-meta,
.session-change,
.lock-note,
.student-cell,
.toolbar-actions {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading,
.selected-info {
  justify-content: space-between;
  gap: 1rem;
}

.header-copy,
.panel-heading > div,
.selected-info > div {
  min-width: 0;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.625rem;
  font-size: 0.71875rem;
  font-weight: 850;
}

h1,
h2 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
  letter-spacing: 0;
}

h1 {
  margin-top: 0.45rem;
  font-size: 1.5rem;
  line-height: 1.15;
}

h2 {
  font-size: 1rem;
}

p {
  margin: 0.25rem 0 0;
  color: var(--text-muted);
  font-size: 0.84375rem;
  line-height: 1.55;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
  align-items: stretch;
}

.summary-card {
  display: grid;
  min-height: 7.5rem;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 0.75rem;
  align-content: space-between;
}

.summary-card :deep(.lg-badge) {
  grid-column: 1 / -1;
  width: fit-content;
}

.summary-icon,
.avatar {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.summary-icon {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-lg);
}

.summary-card p,
.stat-item span {
  margin: 0;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 750;
}

.summary-card strong {
  display: block;
  margin-top: 0.25rem;
  color: var(--text-heading);
  font-size: 1.45rem;
  font-weight: 950;
}

.main-grid {
  display: grid;
  grid-template-columns: 360px minmax(0, 1fr);
  gap: 0.875rem;
  align-items: start;
}

.session-list-panel,
.workspace,
.attendance-table-panel {
  min-width: 0;
}

.session-list {
  display: grid;
  gap: 0.625rem;
  margin-top: 0.875rem;
  max-height: 43rem;
  overflow-y: auto;
  padding-right: 0.125rem;
}

.session-card {
  display: flex;
  min-height: 10rem;
  width: 100%;
  flex-direction: column;
  justify-content: space-between;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.75rem;
  text-align: left;
  color: var(--text-body);
  transition:
    transform 180ms ease,
    border-color 180ms ease,
    background 180ms ease;
}

.session-card:hover,
.session-card.is-active {
  transform: translateY(-1px);
  border-color: var(--border-input-focus);
  background: var(--surface-card-hover);
}

.session-card.is-active {
  box-shadow: inset 3px 0 0 var(--text-link), var(--lg-shadow-sm);
}

.session-card-top,
.session-meta,
.session-change {
  gap: 0.5rem;
}

.session-card-top {
  justify-content: space-between;
}

.session-time,
.muted-cell,
.note-text {
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 750;
}

.session-title {
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
  line-height: 1.35;
}

.session-meta {
  justify-content: space-between;
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 800;
}

.session-change {
  min-width: 0;
  color: var(--text-muted);
  font-size: 0.75rem;
}

.workspace {
  display: grid;
  gap: 0.875rem;
}

.session-summary-panel {
  position: sticky;
  top: 0.75rem;
  z-index: 2;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 0.5rem;
  margin-top: 0.875rem;
}

.stat-item {
  min-height: 4.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.625rem;
}

.stat-item strong {
  display: block;
  margin-top: 0.25rem;
  color: var(--text-heading);
  font-size: 1.15rem;
  font-weight: 950;
}

.stat-item.success {
  background: var(--color-success-bg);
}

.stat-item.warning {
  background: var(--color-warning-bg);
}

.stat-item.info {
  background: var(--color-info-bg);
}

.stat-item.danger {
  background: var(--color-danger-bg);
}

.lock-note {
  gap: 0.45rem;
  margin-top: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 0.625rem 0.75rem;
  font-weight: 800;
}

.summary-actions {
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 0.875rem;
}

.toolbar-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.25fr) minmax(0, 0.75fr) auto;
  gap: 0.75rem;
  align-items: end;
  margin-bottom: 0.875rem;
}

.control-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.375rem;
}

.control-field > span:first-child {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 850;
}

.search-control,
.lg-control {
  min-height: var(--control-height-lg);
}

.search-control {
  display: flex;
  align-items: center;
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.75rem;
  color: var(--text-placeholder);
}

.search-control:focus-within,
.lg-control:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-control input,
.lg-control {
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 750;
}

.lg-control {
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.75rem;
}

.toolbar-actions {
  gap: 0.5rem;
  justify-content: flex-end;
}

table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
}

thead tr {
  border-bottom: 1px solid var(--border-table);
  background: var(--surface-table-header);
}

tbody tr {
  border-bottom: 1px solid var(--border-table);
  background: var(--surface-table);
}

tbody tr:hover {
  background: var(--surface-table-row-hover);
}

th,
td {
  padding: 0.625rem 0.75rem;
  vertical-align: middle;
}

.col-index {
  width: 3.25rem;
  text-align: center;
}

.student-cell {
  min-width: 0;
  gap: 0.625rem;
}

.avatar {
  width: 2rem;
  height: 2rem;
  flex: none;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 950;
}

.student-cell strong {
  min-width: 0;
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 850;
}

.status-segment {
  display: inline-flex;
  max-width: 100%;
  overflow: hidden;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
}

.segment-button {
  min-height: var(--control-height-sm);
  border: 0;
  border-right: 1px solid var(--border-input);
  background: transparent;
  color: var(--text-label);
  padding: 0 0.5rem;
  font-size: 0.71875rem;
  font-weight: 850;
  white-space: nowrap;
}

.segment-button:last-child {
  border-right: 0;
}

.segment-button:hover:not(:disabled),
.segment-button.is-active {
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.segment-button:disabled {
  cursor: not-allowed;
  opacity: 0.58;
}

.clamp-1,
.clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
}

.clamp-1 {
  -webkit-line-clamp: 1;
}

.clamp-2 {
  -webkit-line-clamp: 2;
}

.error-panel {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

@media (max-width: 1180px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .main-grid {
    grid-template-columns: 1fr;
  }

  .session-list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
    max-height: none;
    overflow: visible;
  }

  .session-summary-panel {
    position: static;
  }
}

@media (max-width: 920px) {
  .toolbar-grid {
    grid-template-columns: 1fr;
  }

  .toolbar-actions {
    justify-content: flex-start;
  }

  .stats-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .page-header,
  .selected-info,
  .panel-heading,
  .error-panel {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid,
  .session-list,
  .stats-grid {
    grid-template-columns: 1fr;
  }

  .summary-card,
  .session-card {
    min-height: 7.75rem;
  }

  .toolbar-actions {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
