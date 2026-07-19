<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertCircle,
  CalendarDays,
  CheckCircle2,
  Clock3,
  Eye,
  FilePenLine,
  History,
  LockKeyhole,
  Search,
  Send,
  UnlockKeyhole,
  Users,
  X,
} from 'lucide-vue-next'

import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { usePopupStore } from '@/stores/popup'
import { teacherApi } from '@/services/teacherApi'
import { formatDate, formatDateTime, formatTimeRange } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'

const popupStore = usePopupStore()

const loading = ref(false)
const error = ref('')
const sessions = ref([])
const unlockRequests = ref([])

async function loadHistory() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getAttendanceHistory()
    sessions.value = Array.isArray(data) ? data : (data?.data?.items ?? data?.data ?? data?.items ?? [])
    const unlockData = await teacherApi.getUnlockRequests()
    unlockRequests.value = Array.isArray(unlockData) ? unlockData : (unlockData?.data?.items ?? unlockData?.data ?? unlockData?.items ?? [])
  } catch (e) {
    error.value = e?.message || 'Không thể tải lịch sử điểm danh.'
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadHistory() })

const searchQuery = ref('')
const selectedCourse = ref('')
const selectedStatus = ref('')
const dateFrom = ref('')
const dateTo = ref('')
const selectedSessionId = ref('')
const isUnlockModalOpen = ref(false)
const unlockReason = ref('')
const unlockNote = ref('')
const formSubmitted = ref(false)

const sessionStatusOptions = getStatusOptions('session').filter((option) =>
  ['da_gui', 'da_khoa', 'da_huy'].includes(option.value),
)

const courseOptions = computed(() => {
  const seen = new Map()
  sessions.value.forEach((session) => {
    const key = `${session.courseCode}-${session.className}`
    if (!seen.has(key)) {
      seen.set(key, {
        value: key,
        label: `${session.courseCode} · ${session.className}`,
      })
    }
  })
  return Array.from(seen.values())
})

const filteredSessions = computed(() => {
  const query = searchQuery.value.trim().toLowerCase()
  const from = dateFrom.value ? new Date(dateFrom.value) : null
  const to = dateTo.value ? new Date(dateTo.value) : null

  return sessions.value.filter((session) => {
    const sessionDate = new Date(session.date)
    const matchesSearch =
      !query ||
      session.subject.toLowerCase().includes(query) ||
      session.courseCode.toLowerCase().includes(query) ||
      session.className.toLowerCase().includes(query) ||
      session.room.toLowerCase().includes(query)
    const matchesCourse = !selectedCourse.value || `${session.courseCode}-${session.className}` === selectedCourse.value
    const matchesStatus = !selectedStatus.value || session.status === selectedStatus.value
    const matchesFrom = !from || sessionDate >= from
    const matchesTo = !to || sessionDate <= to

    return matchesSearch && matchesCourse && matchesStatus && matchesFrom && matchesTo
  })
})

const selectedSession = computed(() =>
  sessions.value.find((session) => session.id === selectedSessionId.value) ||
  filteredSessions.value[0] ||
  null,
)

watch(
  filteredSessions,
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
  const submitted = sessions.value.filter((session) => session.status === 'da_gui').length
  const locked = sessions.value.filter((session) => session.status === 'da_khoa').length
  const pendingUnlock = unlockRequests.value.filter((request) => request.status === 'cho_duyet').length
  const onTimeRate = sessions.value.length
    ? Math.round((submitted / sessions.value.filter((session) => session.status !== 'da_huy').length) * 100)
    : 0

  return [
    { label: 'Buổi đã gửi', value: submitted, icon: CheckCircle2, variant: 'success' },
    { label: 'Buổi đã khóa', value: locked, icon: LockKeyhole, variant: 'neutral' },
    { label: 'Chờ mở khóa', value: pendingUnlock, icon: UnlockKeyhole, variant: 'warning' },
    { label: 'Gửi đúng hạn', value: `${Number.isFinite(onTimeRate) ? onTimeRate : 0}%`, icon: Clock3, variant: 'info' },
  ]
})

const detailStats = computed(() => {
  const session = selectedSession.value
  if (!session) return []

  return [
    { label: 'Tổng số', value: session.total, variant: 'neutral' },
    { label: 'Có mặt', value: session.present, variant: 'success' },
    { label: 'Đi muộn', value: session.late, variant: 'warning' },
    { label: 'Có phép', value: session.excused, variant: 'info' },
    { label: 'Vắng', value: session.absent, variant: 'danger' },
  ]
})

const selectedUnlockRequest = computed(() =>
  unlockRequests.value.find((request) => request.sessionId === selectedSession.value?.id) || null,
)

const canCreateUnlockRequest = computed(() =>
  selectedSession.value &&
  ['da_gui', 'da_khoa'].includes(selectedSession.value.status) &&
  !selectedUnlockRequest.value,
)

const unlockReasonError = computed(() =>
  formSubmitted.value && !unlockReason.value.trim() ? 'Vui lòng nhập lý do mở khóa.' : '',
)

function sessionStatusMeta(status) {
  return getStatusMeta('session', status)
}

function attendanceStatusMeta(status) {
  return getStatusMeta('attendance', status)
}

function unlockStatusMeta(status) {
  return getStatusMeta('unlockRequest', status)
}

function selectSession(sessionId) {
  selectedSessionId.value = sessionId
}

function openUnlockModal() {
  if (!selectedSession.value || !canCreateUnlockRequest.value) return
  unlockReason.value = ''
  unlockNote.value = ''
  formSubmitted.value = false
  isUnlockModalOpen.value = true
}

function closeUnlockModal() {
  isUnlockModalOpen.value = false
  formSubmitted.value = false
}

function submitUnlockRequest() {
  formSubmitted.value = true
  if (!selectedSession.value || unlockReasonError.value) return

  unlockRequests.value.unshift({
    id: `unlock-local-${Date.now()}`,
    sessionId: selectedSession.value.id,
    subject: selectedSession.value.subject,
    className: selectedSession.value.className,
    status: 'cho_duyet',
    reason: unlockReason.value.trim(),
    note: unlockNote.value.trim(),
    createdAt: new Date(),
  })

  popupStore.success('Đã tạo yêu cầu mở khóa', 'Yêu cầu demo đã được thêm vào danh sách chờ duyệt.')
  closeUnlockModal()
}

function resetFilters() {
  searchQuery.value = ''
  selectedCourse.value = ''
  selectedStatus.value = ''
  dateFrom.value = ''
  dateTo.value = ''
}
</script>

<template>
  <div v-if="loading" class="p-4">
    <SkeletonTable :rows="8" :columns="6" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton variant="secondary" @click="loadHistory">Thử lại</GlassButton>
  </div>
  <div v-else class="teacher-attendance-history-page lg-page-enter mx-auto max-w-7xl space-y-5">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <p class="eyebrow">
          <History :size="15" />
          Lịch sử điểm danh
        </p>
        <div>
          <h1>Lịch sử điểm danh</h1>
          <p>Tra cứu buổi đã gửi, xem chi tiết và tạo yêu cầu mở khóa khi cần điều chỉnh.</p>
        </div>
      </div>

      <GlassButton variant="secondary" @click="resetFilters">Xóa bộ lọc</GlassButton>
    </GlassPanel>

    <section class="summary-grid" aria-label="Tổng quan lịch sử điểm danh">
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

    <GlassPanel variant="flat" density="compact" class="filter-panel">
      <div class="filter-grid">
        <label class="control-field">
          <span>Tìm kiếm</span>
          <span class="search-control">
            <Search :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Môn, lớp, phòng" />
          </span>
        </label>

        <label class="control-field">
          <span>Môn / lớp</span>
          <select v-model="selectedCourse" class="lg-control">
            <option value="">Tất cả</option>
            <option v-for="option in courseOptions" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </label>

        <label class="control-field">
          <span>Trạng thái</span>
          <select v-model="selectedStatus" class="lg-control">
            <option value="">Tất cả</option>
            <option v-for="option in sessionStatusOptions" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </label>

        <label class="control-field">
          <span>Từ ngày</span>
          <input v-model="dateFrom" type="date" class="lg-control" />
        </label>

        <label class="control-field">
          <span>Đến ngày</span>
          <input v-model="dateTo" type="date" class="lg-control" />
        </label>
      </div>
    </GlassPanel>

    <section class="history-grid">
      <GlassPanel variant="flat" density="compact" class="history-table-panel">
        <div class="panel-heading">
          <div>
            <h2>Danh sách buổi đã điểm danh</h2>
            <p>Hiển thị {{ filteredSessions.length }} buổi theo bộ lọc hiện tại.</p>
          </div>
        </div>

        <TableShell v-if="filteredSessions.length" density="compact" class="history-table-shell">
          <table>
            <thead>
              <tr>
                <th>Ngày</th>
                <th>Môn học</th>
                <th>Lớp</th>
                <th>Ca / Phòng</th>
                <th>Trạng thái</th>
                <th>Tổng / Có mặt / Vắng</th>
                <th>Yêu cầu mở khóa</th>
                <th>Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="session in filteredSessions"
                :key="session.id"
                :class="session.id === selectedSession?.id ? 'is-selected' : ''"
              >
                <td>
                  <div class="date-cell">
                    <CalendarDays :size="15" />
                    <span>{{ formatDate(session.date) }}</span>
                  </div>
                </td>
                <td>
                  <div class="subject-cell">
                    <strong class="clamp-1">{{ session.subject }}</strong>
                    <small>{{ session.courseCode }}</small>
                  </div>
                </td>
                <td>
                  <GlassBadge variant="primary">{{ session.className }}</GlassBadge>
                </td>
                <td>
                  <span class="muted-cell clamp-1">
                    {{ session.shift.label }} · {{ formatTimeRange(session.date, session.endAt) }} · {{ session.room }}
                  </span>
                </td>
                <td>
                  <GlassBadge :variant="sessionStatusMeta(session.status).variant">
                    {{ sessionStatusMeta(session.status).label }}
                  </GlassBadge>
                </td>
                <td>
                  <span class="score-cell">
                    {{ session.total }} / {{ session.present + session.late + session.excused }} / {{ session.absent }}
                  </span>
                </td>
                <td>
                  <GlassBadge
                    v-if="unlockRequests.some((request) => request.sessionId === session.id)"
                    :variant="unlockStatusMeta(unlockRequests.find((request) => request.sessionId === session.id)?.status).variant"
                  >
                    {{ unlockStatusMeta(unlockRequests.find((request) => request.sessionId === session.id)?.status).label }}
                  </GlassBadge>
                  <span v-else class="muted-cell">Chưa có</span>
                </td>
                <td>
                  <GlassButton variant="secondary" size="sm" @click="selectSession(session.id)">
                    <template #leading>
                      <Eye :size="14" />
                    </template>
                    Chi tiết
                  </GlassButton>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>

        <div v-if="filteredSessions.length" class="history-mobile-list">
          <button
            v-for="session in filteredSessions"
            :key="session.id"
            type="button"
            :class="['history-mobile-card', session.id === selectedSession?.id ? 'is-selected' : '']"
            @click="selectSession(session.id)"
          >
            <span class="mobile-card-top">
              <span>{{ formatDate(session.date) }}</span>
              <GlassBadge :variant="sessionStatusMeta(session.status).variant">
                {{ sessionStatusMeta(session.status).label }}
              </GlassBadge>
            </span>
            <strong class="clamp-2">{{ session.subject }}</strong>
            <span class="muted-cell clamp-1">
              {{ session.className }} · {{ session.shift.label }} · {{ session.room }}
            </span>
            <span class="mobile-card-stats">
              <span>{{ session.total }} tổng</span>
              <span>{{ session.present + session.late + session.excused }} có mặt</span>
              <span>{{ session.absent }} vắng</span>
            </span>
          </button>
        </div>

        <EmptyState
          v-else
          title="Không tìm thấy buổi điểm danh"
          description="Thử đổi bộ lọc ngày, trạng thái hoặc từ khóa tìm kiếm."
        >
          <GlassButton variant="secondary" @click="resetFilters">Xóa bộ lọc</GlassButton>
        </EmptyState>
      </GlassPanel>

      <GlassPanel v-if="selectedSession" variant="flat" density="compact" class="detail-panel">
        <div class="detail-header">
          <div>
            <h2 class="clamp-2">{{ selectedSession.subject }}</h2>
            <p>
              {{ selectedSession.className }} · {{ formatDate(selectedSession.date) }} ·
              {{ selectedSession.shift.label }} · {{ selectedSession.room }}
            </p>
          </div>
          <GlassBadge :variant="sessionStatusMeta(selectedSession.status).variant" size="md">
            {{ sessionStatusMeta(selectedSession.status).label }}
          </GlassBadge>
        </div>

        <div class="detail-stats">
          <div
            v-for="item in detailStats"
            :key="item.label"
            :class="['detail-stat', item.variant]"
          >
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </div>

        <div v-if="selectedSession.students?.length" class="detail-section">
          <h3>
            <Users :size="15" />
            Sinh viên nổi bật
          </h3>
          <div class="mini-student-list">
            <div v-for="student in selectedSession.students.slice(0, 5)" :key="student.id" class="mini-student">
              <span class="avatar">{{ student.name.split(' ').pop()?.[0] }}</span>
              <span class="min-w-0">
                <strong class="clamp-1">{{ student.name }}</strong>
                <small class="clamp-1">{{ student.note || 'Không có ghi chú' }}</small>
              </span>
              <GlassBadge :variant="attendanceStatusMeta(student.status).variant">
                {{ attendanceStatusMeta(student.status).label }}
              </GlassBadge>
            </div>
          </div>
        </div>

        <div class="detail-section">
          <h3>
            <Clock3 :size="15" />
            Nhật ký xử lý
          </h3>
          <div class="timeline">
            <p>
              <CheckCircle2 :size="14" />
              Đã gửi điểm danh lúc {{ formatDateTime(selectedSession.submittedAt) }}
            </p>
            <p v-if="selectedSession.lockedAt">
              <LockKeyhole :size="14" />
              Đã khóa lúc {{ formatDateTime(selectedSession.lockedAt) }}
            </p>
            <p v-if="selectedUnlockRequest">
              <UnlockKeyhole :size="14" />
              Yêu cầu mở khóa: {{ unlockStatusMeta(selectedUnlockRequest.status).label }}
            </p>
          </div>
        </div>

        <div v-if="selectedSession.lockReason" class="lock-note">
          <LockKeyhole :size="15" />
          {{ selectedSession.lockReason }}
        </div>

        <div class="detail-actions">
          <GlassButton
            variant="primary"
            :disabled="!canCreateUnlockRequest"
            @click="openUnlockModal"
          >
            <template #leading>
              <FilePenLine :size="16" />
            </template>
            Tạo yêu cầu mở khóa
          </GlassButton>
          <p v-if="selectedUnlockRequest" class="hint-text">
            Đã có yêu cầu {{ unlockStatusMeta(selectedUnlockRequest.status).label.toLowerCase() }} cho buổi này.
          </p>
        </div>
      </GlassPanel>

      <EmptyState
        v-else
        title="Chọn một buổi học"
        description="Chi tiết điểm danh và yêu cầu mở khóa sẽ hiển thị ở đây."
      />
    </section>

    <Teleport to="body">
      <div v-if="isUnlockModalOpen" class="modal-root" role="dialog" aria-modal="true">
        <button
          type="button"
          class="modal-scrim"
          aria-label="Đóng form yêu cầu mở khóa"
          @click="closeUnlockModal"
        />

        <GlassPanel variant="readable" density="comfortable" class="unlock-modal">
          <div class="modal-header">
            <div>
              <h2>Tạo yêu cầu mở khóa</h2>
              <p v-if="selectedSession">
                {{ selectedSession.subject }} · {{ selectedSession.className }} · {{ formatDate(selectedSession.date) }}
              </p>
            </div>
            <button type="button" class="icon-button" aria-label="Đóng" @click="closeUnlockModal">
              <X :size="18" />
            </button>
          </div>

          <div v-if="unlockReasonError" class="form-error-summary">
            {{ unlockReasonError }}
          </div>

          <label class="form-field">
            <span>Lý do mở khóa <strong>*</strong></span>
            <textarea
              v-model="unlockReason"
              rows="4"
              placeholder="Nhập lý do cần mở khóa điểm danh..."
              :aria-invalid="Boolean(unlockReasonError)"
            />
            <small v-if="unlockReasonError">{{ unlockReasonError }}</small>
          </label>

          <label class="form-field">
            <span>Ghi chú bổ sung</span>
            <textarea
              v-model="unlockNote"
              rows="3"
              placeholder="Thông tin thêm cho giáo vụ nếu có..."
            />
          </label>

          <div class="modal-actions">
            <GlassButton variant="secondary" @click="closeUnlockModal">Hủy</GlassButton>
            <GlassButton variant="primary" @click="submitUnlockRequest">
              <template #leading>
                <Send :size="16" />
              </template>
              Gửi yêu cầu
            </GlassButton>
          </div>
        </GlassPanel>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.teacher-attendance-history-page {
  color: var(--text-body);
}

.page-header,
.summary-card,
.panel-heading,
.detail-header,
.date-cell,
.mini-student,
.timeline p,
.lock-note,
.modal-header,
.modal-actions {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading,
.detail-header,
.modal-header {
  justify-content: space-between;
  gap: 1rem;
}

.header-copy,
.panel-heading > div,
.detail-header > div,
.modal-header > div {
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
h2,
h3 {
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

h3 {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.875rem;
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
  min-height: 7.5rem;
  align-items: flex-start;
  gap: 0.75rem;
}

.summary-card :deep(.lg-badge) {
  align-self: flex-end;
  margin-left: auto;
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
  flex: none;
  border-radius: var(--radius-lg);
}

.summary-card p,
.detail-stat span,
.hint-text {
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

.filter-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr) minmax(0, 1fr) minmax(0, 0.82fr) minmax(0, 0.82fr);
  gap: 0.75rem;
  align-items: end;
}

.control-field,
.form-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.375rem;
}

.control-field > span,
.form-field > span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 850;
}

.form-field strong {
  color: var(--color-danger-text);
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
.lg-control:focus,
.form-field textarea:focus {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-control input,
.lg-control,
.form-field textarea {
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

.form-field textarea {
  resize: vertical;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.75rem;
  line-height: 1.5;
}

.form-field small,
.form-error-summary {
  color: var(--color-danger-text);
  font-size: 0.75rem;
  font-weight: 800;
}

.form-error-summary {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--color-danger-bg);
  padding: 0.625rem 0.75rem;
}

.history-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 0.875rem;
  align-items: start;
}

.history-table-panel,
.detail-panel {
  min-width: 0;
}

.detail-panel {
  display: grid;
  gap: 0.875rem;
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

tbody tr:hover,
tbody tr.is-selected {
  background: var(--surface-table-row-hover);
}

tbody tr.is-selected {
  box-shadow: inset 3px 0 0 var(--text-link);
}

th,
td {
  padding: 0.625rem 0.75rem;
  vertical-align: middle;
}

tbody td {
  height: 4.875rem;
  overflow: hidden;
}

.date-cell,
.timeline p,
.lock-note {
  gap: 0.45rem;
}

.date-cell {
  color: var(--text-link);
  font-size: 0.8125rem;
  font-weight: 850;
  white-space: nowrap;
}

.subject-cell strong,
.mini-student strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.84375rem;
  font-weight: 850;
}

.subject-cell small,
.mini-student small,
.muted-cell,
.score-cell {
  color: var(--text-muted);
  font-size: 0.78125rem;
  font-weight: 750;
}

.muted-cell,
.score-cell {
  white-space: nowrap;
}

.score-cell {
  color: var(--text-heading);
  font-weight: 900;
}

.history-mobile-list {
  display: none;
}

.history-mobile-card {
  display: grid;
  min-height: 9.25rem;
  gap: 0.5rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.75rem;
  text-align: left;
  color: var(--text-body);
}

.history-mobile-card.is-selected {
  border-color: var(--border-input-focus);
  box-shadow: inset 3px 0 0 var(--text-link);
}

.history-mobile-card strong {
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
  line-height: 1.35;
}

.mobile-card-top,
.mobile-card-stats {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.mobile-card-top {
  justify-content: space-between;
}

.mobile-card-top > span:first-child {
  color: var(--text-link);
  font-size: 0.8125rem;
  font-weight: 850;
}

.mobile-card-stats {
  flex-wrap: wrap;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 800;
}

.detail-stats {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 0.5rem;
}

.detail-stat {
  min-height: 4.25rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.625rem;
}

.detail-stat strong {
  display: block;
  margin-top: 0.25rem;
  color: var(--text-heading);
  font-size: 1.1rem;
  font-weight: 950;
}

.detail-stat.success {
  background: var(--color-success-bg);
}

.detail-stat.warning {
  background: var(--color-warning-bg);
}

.detail-stat.info {
  background: var(--color-info-bg);
}

.detail-stat.danger {
  background: var(--color-danger-bg);
}

.detail-section {
  display: grid;
  gap: 0.625rem;
}

.mini-student-list {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 0.5rem;
}

.mini-student {
  min-height: 3.5rem;
  gap: 0.625rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.5rem;
}

.avatar {
  width: 2rem;
  height: 2rem;
  flex: none;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 950;
}

.timeline {
  display: grid;
  gap: 0.5rem;
}

.timeline p {
  margin: 0;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.5rem 0.625rem;
  font-weight: 750;
}

.lock-note {
  margin: 0;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 0.625rem 0.75rem;
  font-weight: 800;
}

.detail-actions {
  display: grid;
  gap: 0.5rem;
}

.modal-root {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.modal-scrim {
  position: absolute;
  inset: 0;
  border: 0;
  background: color-mix(in srgb, var(--surface-app) 58%, transparent);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.unlock-modal {
  position: relative;
  z-index: 1;
  display: grid;
  width: min(34rem, 100%);
  gap: 0.875rem;
  box-shadow: var(--lg-shadow-lg);
}

.icon-button {
  display: inline-flex;
  width: 2.25rem;
  height: 2.25rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
}

.icon-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.modal-actions {
  justify-content: flex-end;
  gap: 0.5rem;
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

@media (max-width: 1180px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .detail-stats,
  .mini-student-list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 980px) {
  .filter-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .page-header,
  .panel-heading,
  .detail-header,
  .modal-header {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-grid,
  .filter-grid,
  .detail-stats,
  .mini-student-list {
    grid-template-columns: 1fr;
  }

  .summary-card {
    min-height: 7.5rem;
  }

  .history-table-shell {
    display: none;
  }

  .history-mobile-list {
    display: grid;
    gap: 0.625rem;
  }

  .modal-actions {
    display: grid;
    grid-template-columns: 1fr;
  }
}
</style>
