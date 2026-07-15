<script setup>
import { computed, ref, watch, onMounted } from 'vue'
import dayjs from 'dayjs'
import {
  CalendarDays,
  ChevronLeft,
  ChevronRight,
  Clock,
  MapPin,
  Search,
  User,
  X,
} from 'lucide-vue-next'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import { formatDate, formatTimeRange } from '@/utils/dateFormat'
import { getStatusMeta, getStatusOptions } from '@/utils/statusLabels'
import { studentApi } from '@/services/studentApi'

const loading = ref(false)
const error = ref('')
const scheduleSessions = ref([])
const anchorDate = ref(dayjs().toDate())
const selectedSubject = ref('all')
const selectedStatus = ref('all')
const searchTerm = ref('')
const selectedSession = ref(null)
const detailOpen = ref(false)

useBodyScrollLock(detailOpen)

const weekStart = computed(() => {
  const current = dayjs(anchorDate.value)
  return current.startOf('day').subtract(current.day() === 0 ? 6 : current.day() - 1, 'day')
})
const weekDays = computed(() => Array.from({ length: 6 }, (_, index) => weekStart.value.add(index, 'day')))
const weekLabel = computed(() => `${formatDate(weekDays.value[0])} - ${formatDate(weekDays.value[5])}`)
const subjects = computed(() => [...new Set(scheduleSessions.value.map((session) => session.subject))])
const statusOptions = computed(() => getStatusOptions('session'))

const filteredSessions = computed(() => {
  const keyword = searchTerm.value.trim().toLowerCase()

  return scheduleSessions.value.filter((session) => {
    const matchesSubject = selectedSubject.value === 'all' || session.subject === selectedSubject.value
    const matchesStatus = selectedStatus.value === 'all' || session.status === selectedStatus.value
    const searchable = `${session.subject} ${session.room} ${session.teacher} ${session.courseCode}`.toLowerCase()
    const matchesSearch = !keyword || searchable.includes(keyword)
    return matchesSubject && matchesStatus && matchesSearch
  })
})

const todaySessions = computed(() =>
  filteredSessions.value
    .filter((session) => dayjs(session.startAt).isSame(dayjs(), 'day') && session.status !== 'huy')
    .sort((left, right) => dayjs(left.startAt).valueOf() - dayjs(right.startAt).valueOf()),
)

const nextSession = computed(() =>
  filteredSessions.value
    .filter((session) => dayjs(session.startAt).isAfter(dayjs()) && session.status !== 'huy')
    .sort((left, right) => dayjs(left.startAt).valueOf() - dayjs(right.startAt).valueOf())[0],
)

const changedCount = computed(() =>
  filteredSessions.value.filter((session) => ['doi_phong', 'doi_ca', 'day_thay', 'huy'].includes(session.status)).length,
)

const visibleSessionCount = computed(() => filteredSessions.value.length)

const shifts = [
  { id: 'ca1', label: 'Ca 1', start: '07:30:00', end: '09:00:00' },
  { id: 'ca2', label: 'Ca 2', start: '09:05:00', end: '12:00:00' },
  { id: 'ca3', label: 'Ca 3', start: '13:00:00', end: '14:30:00' },
  { id: 'ca4', label: 'Ca 4', start: '14:35:00', end: '16:05:00' },
  { id: 'ca5', label: 'Ca 5', start: '16:10:00', end: '17:40:00' },
]

function sessionsForCell(day, shiftStart) {
  const targetPrefix = shiftStart.substring(0, 8) // use the full time string
  return filteredSessions.value
    .filter(
      (session) =>
        dayjs(session.startAt).isSame(day, 'day') &&
        session.shift?.start === targetPrefix
    )
    .sort((left, right) => dayjs(left.startAt).valueOf() - dayjs(right.startAt).valueOf())
}

function goPreviousWeek() {
  anchorDate.value = dayjs(anchorDate.value).subtract(7, 'day').toDate()
}

function goNextWeek() {
  anchorDate.value = dayjs(anchorDate.value).add(7, 'day').toDate()
}

function goToday() {
  anchorDate.value = dayjs().toDate()
}

function openDetail(session) {
  selectedSession.value = session
  detailOpen.value = true
}

function closeDetail() {
  detailOpen.value = false
}

function statusMeta(status) {
  return getStatusMeta('session', status)
}

async function loadSessions(date) {
  loading.value = true
  error.value = ''
  try {
    const data = await studentApi.getSchedule({ anchorDate: date })
    scheduleSessions.value = data.sessions || []
  } catch (e) {
    error.value = e?.message || 'Không thể tải dữ liệu.'
  } finally {
    loading.value = false
  }
}

onMounted(() => loadSessions(anchorDate.value))

watch(anchorDate, (newDate) => {
  loadSessions(newDate)
})
</script>

<template>
  <div class="lg-page-enter mx-auto max-w-7xl space-y-5">
    <div class="summary-grid">
      <GlassPanel variant="soft" density="comfortable" class="summary-card">
        <div class="flex min-w-0 items-start gap-3">
          <div class="summary-icon">
            <CalendarDays :size="20" aria-hidden="true" />
          </div>
          <div class="min-w-0">
            <p class="ui-label text-label">Hôm nay</p>
            <h2 class="ui-section-title text-heading">{{ todaySessions.length }} buổi học</h2>
            <p class="clamp-2 ui-body text-muted">
              {{ todaySessions[0]?.subject || 'Không có buổi học trong hôm nay.' }}
            </p>
          </div>
        </div>
      </GlassPanel>

      <GlassPanel variant="soft" density="comfortable" class="summary-card">
        <div class="min-w-0">
          <p class="ui-label text-label">Buổi tiếp theo</p>
          <h2 class="clamp-1 ui-section-title text-heading">
            {{ nextSession?.subject || 'Chưa có lịch sắp tới' }}
          </h2>
          <p class="clamp-2 ui-body text-muted">
            <span v-if="nextSession">
              {{ formatDate(nextSession.startAt) }} ·
              {{ formatTimeRange(nextSession.shift.start, nextSession.shift.end) }} ·
              {{ nextSession.room }}
            </span>
            <span v-else>Lịch tuần này chưa có buổi học sắp tới.</span>
          </p>
        </div>
      </GlassPanel>

      <GlassPanel variant="soft" density="comfortable" class="summary-card">
        <div class="min-w-0">
          <p class="ui-label text-label">Thay đổi trong tuần</p>
          <h2 class="ui-section-title text-heading">{{ changedCount }} thông báo</h2>
          <p class="clamp-2 ui-body text-muted">
            Theo dõi đổi phòng, đổi ca, dạy thay hoặc hủy buổi học.
          </p>
        </div>
      </GlassPanel>
    </div>

    <GlassPanel variant="readable" density="comfortable">
      <div class="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
        <div class="min-w-0">
          <p class="ui-label text-label">Tuần đang xem</p>
          <h2 class="ui-section-title text-heading">{{ weekLabel }}</h2>
        </div>

        <div class="flex flex-wrap items-center gap-2 xl:justify-end">
          <GlassButton variant="secondary" size="sm" aria-label="Tuần trước" @click="goPreviousWeek">
            <template #leading><ChevronLeft :size="15" /></template>
            Tuần trước
          </GlassButton>
          <GlassButton variant="primary" size="sm" @click="goToday">Hôm nay</GlassButton>
          <GlassButton variant="secondary" size="sm" aria-label="Tuần sau" @click="goNextWeek">
            Tuần sau
            <template #trailing><ChevronRight :size="15" /></template>
          </GlassButton>
        </div>
      </div>

      <div class="filter-grid mt-4">
        <GlassInput v-model="searchTerm" placeholder="Tìm theo môn, phòng, giảng viên">
          <template #prefix><Search :size="15" class="text-muted" aria-hidden="true" /></template>
        </GlassInput>

        <label class="control-field">
          <span class="lg-label">Môn học</span>
          <select v-model="selectedSubject" class="lg-control w-full">
            <option value="all">Tất cả môn học</option>
            <option v-for="subject in subjects" :key="subject" :value="subject">
              {{ subject }}
            </option>
          </select>
        </label>

        <label class="control-field">
          <span class="lg-label">Trạng thái / thay đổi</span>
          <select v-model="selectedStatus" class="lg-control w-full">
            <option value="all">Tất cả trạng thái</option>
            <option v-for="status in statusOptions" :key="status.value" :value="status.value">
              {{ status.label }}
            </option>
          </select>
        </label>
      </div>
    </GlassPanel>

    <GlassPanel v-if="loading" variant="readable" density="comfortable">
      <LoadingSkeleton :lines="8" />
    </GlassPanel>

    <EmptyState
      v-else-if="visibleSessionCount === 0"
      title="Không có lịch học phù hợp"
      description="Thử đổi bộ lọc môn học, trạng thái hoặc quay về tuần hiện tại."
    />

    <div v-else class="flex-1 min-w-0 overflow-x-auto pb-4">
      <div class="surface-card border border-(--border-card) rounded-2xl overflow-hidden shadow-sm" style="min-width: 768px">
        <!-- Day headers -->
        <div class="grid border-b border-(--border-default)" :style="`grid-template-columns: 80px repeat(${weekDays.length}, 1fr)`">
          <div class="p-2 text-center text-xs font-semibold text-(--text-muted) bg-(--surface-solid) border-r border-(--border-default) flex items-center justify-center">
            Ca / Thứ
          </div>
          <div
            v-for="day in weekDays" :key="day.format('YYYY-MM-DD')"
            class="p-2.5 text-center text-xs bg-(--surface-solid) border-r last:border-r-0 border-(--border-default)"
          >
            <div :class="day.isSame(dayjs(), 'day') ? 'text-(--text-link) font-bold' : 'text-(--text-heading) font-semibold'">
              Thứ {{ day.day() === 0 ? 'CN' : day.day() + 1 }}
            </div>
          </div>
        </div>

        <!-- Shift rows -->
        <div v-for="shift in shifts" :key="shift.id" class="grid border-b last:border-b-0 border-(--border-default)" :style="`grid-template-columns: 80px repeat(${weekDays.length}, 1fr)`">
          <!-- Shift label -->
          <div class="p-2 flex flex-col items-center justify-center text-center border-r border-(--border-default) bg-(--surface-solid) select-none">
            <span class="text-xs font-bold text-(--text-heading)">{{ shift.label }}</span>
            <span class="text-[10px] text-(--text-muted) leading-tight mt-0.5">{{ shift.start }}<br>{{ shift.end }}</span>
          </div>

          <!-- Day cells -->
          <div
            v-for="day in weekDays" :key="day.format('YYYY-MM-DD')"
            class="border-r last:border-r-0 border-(--border-default) p-1 min-h-[100px] transition-colors relative hover:bg-(--surface-hover)"
          >
            <!-- Empty state placeholder -->
            <div
              v-if="sessionsForCell(day, shift.start).length === 0"
              class="absolute inset-0 m-1 rounded-lg border border-dashed border-(--border-default) flex items-center justify-center opacity-0 hover:opacity-100 transition-all z-0"
            >
              <span class="text-[10px] text-(--text-muted) font-medium">Trống</span>
            </div>

            <!-- Session cards -->
            <button
              v-for="session in sessionsForCell(day, shift.start)" :key="session.id"
              type="button"
              class="relative z-10 mb-1 last:mb-0 w-full text-left rounded-lg border px-2 py-1.5 transition-all hover:shadow-md hover:-translate-y-0.5 border-(--border-default) bg-(--surface-card)"
              @click="openDetail(session)"
            >
              <div class="flex items-start gap-1.5">
                <span class="mt-1 shrink-0 w-1.5 h-1.5 rounded-full" :class="statusMeta(session.status).variant === 'success' ? 'bg-emerald-500' : (statusMeta(session.status).variant === 'danger' ? 'bg-red-500' : 'bg-blue-500')"></span>
                <div class="min-w-0 flex-1">
                  <div class="text-xs font-bold truncate leading-tight text-(--text-heading)" :title="session.subject">
                    {{ session.subject }}
                  </div>
                  <div class="text-[10px] truncate leading-tight text-(--text-muted) mt-0.5">
                    {{ session.room }} · {{ session.substituteTeacher || session.teacher }}
                  </div>
                </div>
              </div>
            </button>
          </div>
        </div>
      </div>
    </div>

    <Teleport to="body">
      <Transition name="drawer">
        <div v-if="detailOpen" class="detail-overlay" @click.self="closeDetail">
          <GlassPanel
            v-if="selectedSession"
            variant="readable"
            density="comfortable"
            class="detail-panel"
          >
            <div class="flex items-start justify-between gap-3">
              <div>
                <GlassBadge :variant="statusMeta(selectedSession.status).variant" size="md">
                  {{ statusMeta(selectedSession.status).label }}
                </GlassBadge>
                <h2 class="mt-3 text-lg font-semibold text-heading">{{ selectedSession.subject }}</h2>
                <p class="text-sm text-muted">{{ selectedSession.courseCode }} · {{ selectedSession.className }}</p>
              </div>
              <button
                type="button"
                class="lg-icon-button flex h-9 w-9 items-center justify-center"
                aria-label="Đóng chi tiết buổi học"
                @click="closeDetail"
              >
                <X :size="17" aria-hidden="true" />
              </button>
            </div>

            <div class="mt-5 space-y-3">
              <div class="detail-row">
                <Clock :size="16" aria-hidden="true" />
                <span>{{ formatDate(selectedSession.startAt) }} · {{ formatTimeRange(selectedSession.shift.start, selectedSession.shift.end) }}</span>
              </div>
              <div class="detail-row">
                <MapPin :size="16" aria-hidden="true" />
                <span>{{ selectedSession.room }}</span>
              </div>
              <div class="detail-row">
                <User :size="16" aria-hidden="true" />
                <span>{{ selectedSession.substituteTeacher || selectedSession.teacher }}</span>
              </div>

              <GlassPanel v-if="selectedSession.reason" variant="soft" density="compact">
                <p class="ui-label text-label">Lý do thay đổi</p>
                <p class="mt-1 text-sm text-body">{{ selectedSession.reason }}</p>
              </GlassPanel>
            </div>
          </GlassPanel>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  grid-auto-rows: 1fr;
  gap: 0.75rem;
  align-items: stretch;
}

.summary-card {
  min-height: 8.25rem;
}

.summary-icon {
  display: flex;
  width: 2.75rem;
  height: 2.75rem;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--color-info-bg);
  color: var(--color-info-text);
}

.filter-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr) minmax(0, 1fr);
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

.schedule-grid {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
  grid-auto-rows: 1fr;
  gap: 0.75rem;
  align-items: stretch;
}

.day-column {
  display: flex;
  min-height: 24rem;
  flex-direction: column;
}

.day-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  min-height: 3.25rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.625rem;
}

.day-header.is-today strong {
  color: var(--text-link);
}

.day-body {
  display: flex;
  min-height: 0;
  flex: 1;
  flex-direction: column;
  gap: 0.5rem;
  overflow-y: auto;
  padding-top: 0.75rem;
}

.session-card {
  display: flex;
  height: 10.75rem;
  min-height: 9.75rem;
  width: 100%;
  flex-direction: column;
  justify-content: space-between;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.75rem;
  color: var(--text-body);
  box-shadow: var(--lg-shadow-sm);
  transition:
    transform 180ms ease,
    border-color 180ms ease,
    background 180ms ease;
}

.session-card:hover {
  transform: translateY(-1px);
  border-color: var(--border-default);
  background: var(--surface-card-hover);
}

.session-time {
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 600;
}

.session-title {
  display: -webkit-box;
  margin-top: 0.25rem;
  overflow: hidden;
  color: var(--text-heading);
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  font-size: 0.875rem;
  font-weight: 700;
  line-height: 1.35;
}

.session-meta {
  display: grid;
  min-height: 2.25rem;
  gap: 0.35rem;
  margin-top: 0.75rem;
  color: var(--text-muted);
  font-size: 0.75rem;
  text-align: left;
}

.session-meta span {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-day {
  display: flex;
  min-height: 9.75rem;
  align-items: center;
  justify-content: center;
  border: 1px dashed var(--border-card);
  border-radius: var(--radius-lg);
  padding: 0.875rem;
  color: var(--text-muted);
  font-size: 0.8125rem;
  text-align: center;
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

@media (max-width: 1180px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }

  .filter-grid {
    grid-template-columns: 1fr;
  }

  .schedule-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .schedule-grid {
    grid-template-columns: 1fr;
  }

  .day-column {
    min-height: auto;
  }

  .detail-overlay {
    padding: 0;
  }

  .detail-panel {
    width: 100%;
    border-radius: 0;
  }
}
</style>
