<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  CalendarRange,
  ChevronRight,
  Clock,
  FileSignature,
  Search,
  UserCheck,
  X,
} from 'lucide-vue-next'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { formatDate, formatTimeRange, toDateInputValue } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'
import { studentApi } from '@/services/studentApi'

const router = useRouter()

const loading = ref(false)
const error = ref('')
const attendanceHistory = ref([])
const subjectStats = ref([])

const selectedSubject = ref('all')
const selectedStatus = ref('all')
const searchTerm = ref('')
const dateFrom = ref('')
const dateTo = ref('')
const selectedRecord = ref(null)
const detailOpen = ref(false)

useBodyScrollLock(detailOpen)

const subjects = computed(() => [...new Set(attendanceHistory.value.map((item) => item.subject))])
const attendanceStatusOptions = computed(() => getStatusOptions('attendance'))

const filteredHistory = computed(() => {
  const keyword = searchTerm.value.trim().toLowerCase()

  return attendanceHistory.value.filter((item) => {
    const matchesSubject = selectedSubject.value === 'all' || item.subject === selectedSubject.value
    const matchesStatus = selectedStatus.value === 'all' || item.status === selectedStatus.value
    const matchesFrom = !dateFrom.value || toDateInputValue(item.attendedAt) >= dateFrom.value
    const matchesTo = !dateTo.value || toDateInputValue(item.attendedAt) <= dateTo.value
    const searchable = `${item.subject} ${item.teacher} ${item.room} ${item.note}`.toLowerCase()
    const matchesSearch = !keyword || searchable.includes(keyword)

    return matchesSubject && matchesStatus && matchesFrom && matchesTo && matchesSearch
  })
})

const kpis = computed(() => {
  const rows = attendanceHistory.value
  const counted = rows.filter((item) => item.status !== 'chua_diem_danh')
  const positive = counted.filter((item) => ['co_mat', 'di_muon', 'co_phep'].includes(item.status))
  const rate = counted.length ? Math.round((positive.length / counted.length) * 100) : 100

  return [
    { label: 'Tỷ lệ chuyên cần', value: `${rate}%`, hint: 'Tính trên các buổi đã chốt' },
    { label: 'Vắng', value: rows.filter((item) => item.status === 'vang').length, hint: 'Buổi cần lưu ý' },
    { label: 'Đi muộn', value: rows.filter((item) => item.status === 'di_muon').length, hint: 'Lần đi muộn' },
    { label: 'Có phép', value: rows.filter((item) => item.status === 'co_phep').length, hint: 'Đã được ghi nhận' },
  ]
})

function statusMeta(status) {
  return getStatusMeta('attendance', status)
}

function openDetail(record) {
  selectedRecord.value = record
  detailOpen.value = true
}

function closeDetail() {
  detailOpen.value = false
}

function goToRequests() {
  router.push('/student/requests')
}

onMounted(async () => {
  loading.value = true
  error.value = ''
  try {
    const data = await studentApi.getAttendance()
    attendanceHistory.value = data.history || []
    subjectStats.value = data.subjectStats || []
  } catch (e) {
    error.value = e?.message || 'Không thể tải dữ liệu.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="lg-page-enter mx-auto max-w-7xl space-y-5">
    <div class="kpi-grid">
      <GlassPanel v-for="kpi in kpis" :key="kpi.label" variant="soft" density="compact" class="kpi-card">
        <div>
          <p class="ui-label text-label">{{ kpi.label }}</p>
          <div class="mt-2 text-2xl font-semibold text-heading">{{ kpi.value }}</div>
        </div>
        <p class="clamp-2 text-sm text-muted">{{ kpi.hint }}</p>
      </GlassPanel>
    </div>

    <GlassPanel variant="readable" density="comfortable">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
        <div>
          <p class="ui-label text-label">Tiến độ theo môn</p>
          <h2 class="ui-section-title text-heading">Tỷ lệ chuyên cần học kỳ</h2>
          <p class="ui-body text-muted">Các môn có tỷ lệ thấp cần được theo dõi và giải trình kịp thời.</p>
        </div>
        <GlassButton variant="secondary" size="sm" @click="goToRequests">
          <template #leading><FileSignature :size="15" /></template>
          Đến đơn từ
        </GlassButton>
      </div>

      <div class="progress-grid mt-4">
        <div v-for="subject in subjectStats" :key="subject.courseId" class="subject-progress">
          <div class="mb-3 flex min-w-0 items-start justify-between gap-3">
            <div class="min-w-0">
              <p class="clamp-2 text-sm font-semibold text-heading">{{ subject.subject }}</p>
              <p class="text-xs text-muted">{{ subject.absent }} buổi vắng / {{ subject.total }} buổi đã chốt</p>
            </div>
            <GlassBadge :variant="subject.rate >= 80 ? 'success' : 'warning'" class="shrink-0">
              {{ subject.rate }}%
            </GlassBadge>
          </div>
          <ProgressBar :value="subject.rate" :max="100" :variant="subject.rate >= 80 ? 'green' : 'amber'" />
        </div>
      </div>
    </GlassPanel>

    <GlassPanel variant="readable" density="comfortable">
      <div class="filter-grid">
        <GlassInput v-model="searchTerm" placeholder="Tìm theo môn, phòng, giảng viên, ghi chú">
          <template #prefix><Search :size="15" class="text-muted" aria-hidden="true" /></template>
        </GlassInput>

        <label class="control-field">
          <span class="lg-label">Môn học</span>
          <select v-model="selectedSubject" class="lg-control w-full">
            <option value="all">Tất cả môn học</option>
            <option v-for="subject in subjects" :key="subject" :value="subject">{{ subject }}</option>
          </select>
        </label>

        <label class="control-field">
          <span class="lg-label">Trạng thái</span>
          <select v-model="selectedStatus" class="lg-control w-full">
            <option value="all">Tất cả trạng thái</option>
            <option v-for="status in attendanceStatusOptions" :key="status.value" :value="status.value">
              {{ status.label }}
            </option>
          </select>
        </label>

        <label class="control-field">
          <span class="lg-label">Từ ngày</span>
          <input v-model="dateFrom" type="date" class="lg-control w-full" />
        </label>

        <label class="control-field">
          <span class="lg-label">Đến ngày</span>
          <input v-model="dateTo" type="date" class="lg-control w-full" />
        </label>
      </div>
    </GlassPanel>

    <EmptyState
      v-if="filteredHistory.length === 0"
      title="Không có dữ liệu điểm danh"
      description="Thử đổi bộ lọc môn học, trạng thái hoặc khoảng ngày."
    />

    <GlassPanel v-else variant="surface" density="none">
      <TableShell density="compact">
        <table>
          <thead>
            <tr>
              <th class="text-left">Ngày học</th>
              <th class="text-left">Môn học</th>
              <th class="text-left">Ca / phòng</th>
              <th class="text-left">Trạng thái</th>
              <th class="text-left">Ghi chú</th>
              <th class="text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filteredHistory" :key="item.id">
              <td>{{ formatDate(item.attendedAt) }}</td>
              <td>
                <div class="font-semibold text-heading">{{ item.subject }}</div>
                <div class="text-xs text-muted">{{ item.courseCode }}</div>
              </td>
              <td>
                <div>{{ item.shift.label }} · {{ formatTimeRange(item.shift.start, item.shift.end) }}</div>
                <div class="text-xs text-muted">{{ item.room }}</div>
              </td>
              <td>
                <GlassBadge :variant="statusMeta(item.status).variant">
                  {{ statusMeta(item.status).label }}
                </GlassBadge>
              </td>
              <td class="max-w-[18rem] text-muted">
                <span class="clamp-2">{{ item.note }}</span>
              </td>
              <td class="text-right">
                <GlassButton variant="ghost" size="sm" @click="openDetail(item)">
                  Chi tiết
                  <template #trailing><ChevronRight :size="14" /></template>
                </GlassButton>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>

    <div class="attendance-mobile-list">
      <GlassPanel
        v-for="item in filteredHistory"
        :key="`mobile-${item.id}`"
        variant="surface"
        density="compact"
        as="button"
        class="mobile-card text-left"
        @click="openDetail(item)"
      >
        <div class="flex items-start justify-between gap-3">
          <div>
            <p class="text-sm font-semibold text-heading">{{ item.subject }}</p>
            <p class="mt-1 text-xs text-muted">{{ formatDate(item.attendedAt) }} · {{ item.shift.label }}</p>
          </div>
          <GlassBadge :variant="statusMeta(item.status).variant">
            {{ statusMeta(item.status).label }}
          </GlassBadge>
        </div>
        <p class="mt-3 text-sm text-muted">{{ item.note }}</p>
      </GlassPanel>
    </div>

    <Teleport to="body">
      <Transition name="drawer">
        <div v-if="detailOpen" class="detail-overlay" @click.self="closeDetail">
          <GlassPanel
            v-if="selectedRecord"
            variant="readable"
            density="comfortable"
            class="detail-panel"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <GlassBadge :variant="statusMeta(selectedRecord.status).variant" size="md">
                  {{ statusMeta(selectedRecord.status).label }}
                </GlassBadge>
                <h2 class="mt-3 text-lg font-semibold text-heading">{{ selectedRecord.subject }}</h2>
                <p class="text-sm text-muted">{{ selectedRecord.courseCode }}</p>
              </div>
              <button
                type="button"
                class="lg-icon-button flex h-9 w-9 items-center justify-center"
                aria-label="Đóng chi tiết điểm danh"
                @click="closeDetail"
              >
                <X :size="17" aria-hidden="true" />
              </button>
            </div>

            <div class="mt-5 space-y-3">
              <div class="detail-row">
                <CalendarRange :size="16" aria-hidden="true" />
                <span>{{ formatDate(selectedRecord.attendedAt) }}</span>
              </div>
              <div class="detail-row">
                <Clock :size="16" aria-hidden="true" />
                <span>{{ selectedRecord.shift.label }} · {{ formatTimeRange(selectedRecord.shift.start, selectedRecord.shift.end) }}</span>
              </div>
              <div class="detail-row">
                <UserCheck :size="16" aria-hidden="true" />
                <span>{{ selectedRecord.teacher }}</span>
              </div>
              <GlassPanel variant="soft" density="compact">
                <p class="ui-label text-label">Ghi chú</p>
                <p class="mt-1 text-sm text-body">{{ selectedRecord.note }}</p>
              </GlassPanel>
              <GlassButton
                v-if="['vang', 'di_muon', 'chua_diem_danh'].includes(selectedRecord.status)"
                variant="primary"
                block
                @click="goToRequests"
              >
                <template #leading><FileSignature :size="16" /></template>
                Tạo đơn giải trình
              </GlassButton>
            </div>
          </GlassPanel>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  grid-auto-rows: 1fr;
  gap: 0.75rem;
  align-items: stretch;
}

.kpi-card {
  display: flex;
  min-height: 7.25rem;
  flex-direction: column;
  justify-content: space-between;
}

.progress-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  grid-auto-rows: 1fr;
  gap: 0.75rem;
  align-items: stretch;
}

.subject-progress {
  display: flex;
  min-height: 7.5rem;
  flex-direction: column;
  justify-content: space-between;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.875rem;
}

.filter-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr) minmax(0, 1fr) minmax(0, 0.82fr) minmax(0, 0.82fr);
  gap: 0.75rem;
  align-items: end;
}

.filter-grid :deep(.lg-input),
.filter-grid .lg-control {
  min-height: var(--control-height-lg);
}

.control-field {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.375rem;
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
  padding: 0.75rem 0.875rem;
  vertical-align: middle;
}

tbody td {
  min-height: 3.75rem;
}

.attendance-mobile-list {
  display: none;
}

.mobile-card {
  width: 100%;
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

.detail-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: flex;
  justify-content: flex-end;
  background: color-mix(in srgb, var(--surface-app) 55%, transparent);
  padding: 1rem;
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.detail-panel {
  width: min(100%, 28rem);
  height: 100%;
  overflow-y: auto;
  box-shadow: var(--lg-shadow-lg);
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 0.625rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.75rem;
  color: var(--text-body);
  font-size: 0.875rem;
}

.detail-row svg {
  color: var(--text-muted);
  flex-shrink: 0;
}

.drawer-enter-active,
.drawer-leave-active {
  transition: opacity 180ms ease;
}

.drawer-enter-from,
.drawer-leave-to {
  opacity: 0;
}

@media (max-width: 860px) {
  .filter-grid,
  .progress-grid {
    grid-template-columns: 1fr;
  }

  .lg-table-shell {
    display: none;
  }

  .attendance-mobile-list {
    display: grid;
    gap: 0.75rem;
  }
}

@media (min-width: 640px) {
  .kpi-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (min-width: 1280px) {
  .kpi-grid {
    grid-template-columns: repeat(4, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .detail-overlay {
    padding: 0;
  }

  .detail-panel {
    width: 100%;
    border-radius: 0;
  }
}
</style>
