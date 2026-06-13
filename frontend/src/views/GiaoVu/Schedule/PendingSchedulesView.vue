<script setup>
import { ref, computed, reactive } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { useAuthStore } from '@/stores/auth'
import { useSchedule } from '@/composables/useSchedule'
import {
  Clock,
  Search,
  Eye,
  Undo2,
  Calendar,
  User,
  ExternalLink,
  X,
  SlidersHorizontal,
  CheckCircle2,
  AlertTriangle,
  Send,
  Building,
  BookOpen,
  Users,
  MapPin,
  FileText,
  ChevronRight,
  RotateCcw,
  Info,
  Layers,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import FloorDropdown from '@/components/ui/FloorDropdown.vue'

// ── Mock Data ────────────────────────────────────────────────
const pendingSchedules = ref([
  {
    id: 1,
    semester: 'HK2 2025–2026',
    floor: '3',
    classCount: 45,
    scheduleCount: 320,
    conflictCount: 2,
    requester: 'Trần Thị Giáo Vụ',
    submittedDate: '12/05/2026 10:30',
    status: 'pending',
    note: '',
    classes: [
      { code: 'SE1601', subject: 'Lập trình Java', teacher: 'TS. Nguyễn Văn A', room: 'P.302', schedule: 'Thứ 2, 07:30–10:30', students: 42 },
      { code: 'SE1602', subject: 'Cấu trúc dữ liệu', teacher: 'ThS. Trần Thị B', room: 'P.105', schedule: 'Thứ 3, 13:30–15:30', students: 38 },
      { code: 'SE1603', subject: 'Web Frontend', teacher: 'TS. Lê Văn C', room: 'Lab 2', schedule: 'Thứ 4, 08:30–11:30', students: 35 },
      { code: 'SE1604', subject: 'Mạng máy tính', teacher: 'ThS. Phạm Minh Tuấn', room: 'P.401', schedule: 'Thứ 5, 13:30–16:30', students: 40 },
      { code: 'BA1501', subject: 'Kinh tế vi mô', teacher: 'TS. Hoàng Thị D', room: 'P.210', schedule: 'Thứ 6, 07:30–09:30', students: 55 },
    ]
  },
  {
    id: 2,
    semester: 'HK2 2025–2026',
    floor: '1',
    classCount: 12,
    scheduleCount: 84,
    conflictCount: 0,
    requester: 'Lê Văn Giáo Vụ',
    submittedDate: '11/05/2026 14:15',
    status: 'pending',
    note: '',
    classes: [
      { code: 'IT2001', subject: 'Nhập môn AI', teacher: 'TS. Nguyễn Minh E', room: 'P.101', schedule: 'Thứ 2, 08:30–11:30', students: 30 },
      { code: 'IT2002', subject: 'Hệ quản trị CSDL', teacher: 'ThS. Đặng Văn F', room: 'Lab 1', schedule: 'Thứ 4, 13:30–16:30', students: 28 },
    ]
  },
  {
    id: 3,
    semester: 'HK1 2025–2026',
    floor: '5',
    classCount: 38,
    scheduleCount: 266,
    conflictCount: 0,
    requester: 'Trần Thị Giáo Vụ',
    submittedDate: '20/08/2025 09:00',
    status: 'returned',
    note: 'BGH yêu cầu điều chỉnh xung đột phòng Lab 2 vào Thứ 4.',
    classes: []
  },
])

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const filterStatus = ref('all')
const filterSemester = ref('all')
const filterFloor = ref('all')
const showFilterPanel = ref(false)

const SEMESTERS = computed(() => {
  const set = new Set(pendingSchedules.value.map(s => s.semester))
  return [...set]
})

const FLOORS = ['1', '2', '3', '4', '5']

const filteredSchedules = computed(() => {
  return pendingSchedules.value.filter(item => {
    const q = searchQuery.value.toLowerCase().trim()
    const matchSearch = !q
      || item.semester.toLowerCase().includes(q)
      || item.requester.toLowerCase().includes(q)
      || item.floor.toLowerCase().includes(q)
    const matchStatus = filterStatus.value === 'all' || item.status === filterStatus.value
    const matchSemester = filterSemester.value === 'all' || item.semester === filterSemester.value
    const matchFloor = filterFloor.value === 'all' || item.floor === filterFloor.value
    return matchSearch && matchStatus && matchSemester && matchFloor
  })
})

const activeFilterCount = computed(() => {
  let count = 0
  if (filterStatus.value !== 'all') count++
  if (filterSemester.value !== 'all') count++
  if (filterFloor.value !== 'all') count++
  return count
})

function clearFilters() {
  filterStatus.value = 'all'
  filterSemester.value = 'all'
  filterFloor.value = 'all'
}

// ── Stats ────────────────────────────────────────────────────
const stats = computed(() => {
  const all = pendingSchedules.value
  return {
    total: all.length,
    pending: all.filter(s => s.status === 'pending').length,
    returned: all.filter(s => s.status === 'returned').length,
    totalClasses: all.reduce((acc, s) => acc + s.classCount, 0),
  }
})

// ── Status helpers ───────────────────────────────────────────
const STATUS_MAP = {
  pending:  { label: 'Chờ duyệt',   dot: 'bg-[var(--lg-warning)]',  badge: 'lg-badge lg-badge-warning' },
  returned: { label: 'Bị trả lại',  dot: 'bg-[var(--lg-danger)]',   badge: 'lg-badge border-[var(--color-danger-bg)] bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' },
  approved: { label: 'Đã duyệt',    dot: 'bg-[var(--lg-success)]',  badge: 'lg-badge lg-badge-success' },
}
const getStatusInfo = s => STATUS_MAP[s] || STATUS_MAP.pending

// ── Detail Modal ─────────────────────────────────────────────
const showDetailModal = ref(false)
const selectedSchedule = ref(null)
useBodyScrollLock(showDetailModal)

function openDetail(item) {
  selectedSchedule.value = item
  showDetailModal.value = true
}

function closeDetail() {
  showDetailModal.value = false
  selectedSchedule.value = null
}

// ── Withdraw (Thu hồi) ──────────────────────────────────────
const showWithdrawModal = ref(false)
const withdrawTarget = ref(null)
const isWithdrawing = ref(false)
useBodyScrollLock(showWithdrawModal)

function openWithdraw(item) {
  withdrawTarget.value = item
  showWithdrawModal.value = true
}

function closeWithdraw() {
  showWithdrawModal.value = false
  withdrawTarget.value = null
}

async function confirmWithdraw() {
  if (!withdrawTarget.value) return
  isWithdrawing.value = true
  await new Promise(r => setTimeout(r, 800))
  // Remove from list (simulate moving back to draft)
  const idx = pendingSchedules.value.findIndex(s => s.id === withdrawTarget.value.id)
  if (idx !== -1) pendingSchedules.value.splice(idx, 1)
  isWithdrawing.value = false
  closeWithdraw()
}

// ── Resubmit (Gửi lại) ──────────────────────────────────────
const isResubmitting = ref(null)

async function resubmit(item) {
  isResubmitting.value = item.id
  await new Promise(r => setTimeout(r, 800))
  item.status = 'pending'
  item.note = ''
  isResubmitting.value = null
}

// ── New Schedule Submission ─────────────────────────────────
const auth = useAuthStore()
const { events: scheduleEvents, draftEvents, updateEvent } = useSchedule()

const showNewScheduleModal = ref(false)
const newScheduleSemester = ref('HK2 2025–2026')
const newScheduleFloor = ref('all')
const isSubmitting = ref(false)
const semesterOptions = ['HK2 2025–2026', 'HK1 2025–2026', 'HK2 2024–2025']
useBodyScrollLock(showNewScheduleModal)

function openNewSchedule() {
  newScheduleSemester.value = semesterOptions[0]
  newScheduleFloor.value = 'all'
  showNewScheduleModal.value = true
}

function closeNewSchedule() {
  showNewScheduleModal.value = false
}

async function confirmNewSchedule() {
  isSubmitting.value = true
  await new Promise(r => setTimeout(r, 1200))

  const drafts = scheduleEvents.value.filter(e => e.status === 'draft')

  drafts.forEach(e => {
    updateEvent(e.id, { status: 'pending' })
  })

  const now = new Date()
  const newEntry = {
    id: Date.now(),
    semester: newScheduleSemester.value,
    floor: newScheduleFloor.value,
    classCount: drafts.length,
    scheduleCount: drafts.length,
    conflictCount: 0,
    requester: auth.displayName || 'Trần Thị Giáo Vụ',
    submittedDate: now.toLocaleString('vi-VN'),
    status: 'pending',
    note: '',
    classes: drafts.map(e => ({
      code: e.title.substring(0, 6).toUpperCase(),
      subject: e.title,
      teacher: e.teacher,
      room: e.room,
      schedule: `${now.toLocaleDateString('vi-VN')}`,
      students: 35,
    })),
  }

  pendingSchedules.value.unshift(newEntry)
  isSubmitting.value = false
  showNewScheduleModal.value = false
}
</script>

<template>
  <PageContainer
    title="Lịch chờ duyệt"
    subtitle="Danh sách các thời khóa biểu đã gửi lên Ban giám hiệu chờ phê duyệt."
  >
    <template #actions>
      <button
        id="btn-send-for-approval"
        class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2"
        title="Gửi TKB mới lên BGH"
        @click="openNewSchedule"
      >
        <Send :size="16" /> Gửi duyệt TKB mới
      </button>
    </template>

    <div class="space-y-4">

      <!-- ── Stats Row ─────────────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div
          v-for="stat in [
            { label: 'Tổng đã gửi',    value: stats.total,        color: 'text-[var(--color-info-text)]',    bg: 'bg-[var(--color-info-bg)]' },
            { label: 'Đang chờ duyệt', value: stats.pending,      color: 'text-[var(--color-warning-text)]', bg: 'bg-[var(--color-warning-bg)]' },
            { label: 'Bị trả lại',     value: stats.returned,     color: 'text-[var(--color-danger-text)]',  bg: 'bg-[var(--color-danger-bg)]' },
            { label: 'Tổng số lớp',    value: stats.totalClasses, color: 'text-[var(--color-success-text)]', bg: 'bg-[var(--color-success-bg)]' },
          ]"
          :key="stat.label"
          :class="['rounded-2xl p-4 border border-default', stat.bg]"
        >
          <p class="text-[11px] font-semibold uppercase tracking-widest text-placeholder">{{ stat.label }}</p>
          <p :class="['text-2xl font-semibold mt-1', stat.color]">{{ stat.value }}</p>
        </div>
      </div>

      <!-- ── Search & Filter Bar ───────────────────────── -->
      <div class="lg-glass-strong p-4 rounded-2xl space-y-3">
        <div class="flex flex-wrap items-center gap-3">
          <!-- Search -->
          <div class="flex-1 min-w-[260px] relative">
            <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
            <input
              id="input-search-pending"
              v-model="searchQuery"
              type="text"
               placeholder="Tìm theo học kỳ, người gửi, lầu..."
              class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium transition-all"
            />
            <button
              v-if="searchQuery"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <!-- Quick selects -->
          <select
            id="select-filter-semester"
            v-model="filterSemester"
            class="lg-input px-3 py-2.5 text-sm font-bold"
          >
            <option value="all">Tất cả học kỳ</option>
            <option v-for="s in SEMESTERS" :key="s" :value="s">{{ s }}</option>
          </select>

          <FloorDropdown
            v-model="filterFloor"
            :floors="FLOORS"
            all-label="Tất cả lầu"
            label=""
          />

          <!-- Advanced filter toggle -->
          <button
            id="btn-toggle-filter"
            :class="[
              'flex items-center gap-2 px-4 py-2.5 text-sm font-bold rounded-xl transition-all',
              showFilterPanel ? 'lg-button-primary' : 'lg-button-secondary text-body'
            ]"
            @click.stop="showFilterPanel = !showFilterPanel"
          >
            <SlidersHorizontal :size="16" />
            Bộ lọc
            <span
              v-if="activeFilterCount > 0"
              class="inline-flex items-center justify-center h-4 w-4 rounded-full bg-[var(--lg-success)] text-white text-[10px] font-semibold"
            >{{ activeFilterCount }}</span>
          </button>

          <button
            v-if="activeFilterCount > 0 || searchQuery"
            class="text-xs font-bold text-placeholder hover:text-[var(--lg-danger)] transition-colors"
            @click="clearFilters(); searchQuery = ''"
          >Xóa bộ lọc</button>
        </div>

        <!-- Advanced filter panel -->
        <Transition name="slide-down">
          <div v-if="showFilterPanel" class="pt-3 border-t border-default flex flex-wrap gap-4">
            <div>
              <p class="text-[10px] font-semibold uppercase tracking-widest text-placeholder mb-2">Trạng thái</p>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="s in ['all', 'pending', 'returned']" :key="s"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    filterStatus === s ? 'lg-button-primary' : 'lg-button-secondary text-body'
                  ]"
                  @click="filterStatus = s"
                >
                  {{ s === 'all' ? 'Tất cả' : getStatusInfo(s).label }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- ── Result count ──────────────────────────────── -->
      <div class="flex items-center justify-between px-1">
        <p class="text-sm text-label font-semibold">
          Hiển thị <span class="font-semibold text-heading">{{ filteredSchedules.length }}</span> / {{ pendingSchedules.length }} bản TKB
        </p>
      </div>

      <!-- ── Pending Cards ─────────────────────────────── -->
      <TransitionGroup name="card-list" tag="div" class="space-y-4">
        <div
          v-for="item in filteredSchedules"
          :key="item.id"
          :class="[
            'lg-card surface-card group p-4 transition-all hover:-translate-y-0.5 hover:shadow-sm cursor-default',
            item.status === 'returned' ? 'ring-1 ring-[var(--lg-danger)]/20' : ''
          ]"
        >
          <!-- Top accent bar -->
          <div
            :class="[
              'absolute top-0 left-0 right-0 h-1 rounded-t-[28px]',
              item.status === 'pending'
                ? 'bg-[var(--lg-warning)]'
                : 'bg-[var(--lg-danger)]'
            ]"
          ></div>

          <!-- Header row: status + meta -->
          <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
            <div class="flex items-center gap-4">
              <!-- Icon -->
              <div
                :class="[
                  'h-14 w-14 rounded-2xl flex items-center justify-center shrink-0 border',
                  item.status === 'pending'
                    ? 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[var(--color-warning-bg)]'
                    : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-bg)]'
                ]"
              >
                <Calendar :size="26" />
              </div>

              <div>
                <div class="flex items-center gap-3 flex-wrap">
                  <h3 class="text-lg font-semibold text-heading">TKB {{ item.semester }}</h3>
                  <span :class="['px-2.5 py-1 text-[10px] font-semibold uppercase tracking-widest', getStatusInfo(item.status).badge]">
                    {{ getStatusInfo(item.status).label }}
                  </span>
                </div>
                <div class="mt-1.5 flex flex-wrap items-center gap-x-5 gap-y-1">
                  <span class="flex items-center gap-1.5 text-xs font-bold text-label">
                    <Clock :size="13" class="text-placeholder" /> {{ item.submittedDate }}
                  </span>
                  <span class="flex items-center gap-1.5 text-xs font-bold text-label">
                    <User :size="13" class="text-placeholder" /> {{ item.requester }}
                  </span>
                  <span class="flex items-center gap-1.5 text-xs font-bold text-[var(--lg-primary)]">
                    <MapPin :size="13" /> Lầu {{ item.floor }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Right stats + actions -->
            <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-4">
              <!-- Summary numbers -->
              <div class="flex items-center gap-6 px-5 py-3 surface-input rounded-2xl border border-default">
                <div class="text-center">
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Số lớp</p>
                  <p class="text-xl font-semibold text-heading mt-0.5">{{ item.classCount }}</p>
                </div>
                <div class="w-px h-8 bg-[var(--border-default)]"></div>
                <div class="text-center">
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Số tiết</p>
                  <p class="text-xl font-semibold text-heading mt-0.5">{{ item.scheduleCount }}</p>
                </div>
                <div v-if="item.conflictCount > 0" class="w-px h-8 bg-[var(--border-default)]"></div>
                <div v-if="item.conflictCount > 0" class="text-center">
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Xung đột</p>
                  <p class="text-xl font-semibold text-[var(--lg-danger)] mt-0.5">{{ item.conflictCount }}</p>
                </div>
              </div>

              <!-- Action buttons -->
              <div class="flex items-center gap-2">
                <template v-if="item.status === 'pending'">
                  <button
                    class="lg-button-secondary px-4 py-2.5 text-sm font-bold text-body hover:text-[var(--lg-danger)]"
                    @click="openWithdraw(item)"
                    title="Thu hồi về trạng thái Draft"
                  >
                    <Undo2 :size="16" /> Thu hồi
                  </button>
                </template>
                <template v-if="item.status === 'returned'">
                  <button
                    :class="['lg-button-secondary px-4 py-2.5 text-sm font-bold text-body', isResubmitting === item.id ? 'opacity-60 pointer-events-none' : '']"
                    @click="resubmit(item)"
                    title="Gửi lại lên BGH"
                  >
                    <RotateCcw v-if="isResubmitting !== item.id" :size="16" />
                    <span v-else class="h-4 w-4 border-2 border-current border-t-transparent rounded-full animate-spin"></span>
                    {{ isResubmitting === item.id ? 'Đang gửi...' : 'Gửi lại' }}
                  </button>
                </template>
                <button
                  class="lg-button-primary px-5 py-2.5 text-sm font-bold"
                  @click="openDetail(item)"
                >
                  <Eye :size="16" /> Chi tiết
                </button>
              </div>
            </div>
          </div>

          <!-- Returned note -->
          <Transition name="slide-down">
            <div v-if="item.status === 'returned' && item.note" class="mt-4 p-4 rounded-2xl bg-[var(--color-danger-bg)] border border-default flex items-start gap-3">
              <AlertTriangle :size="18" class="text-[var(--color-danger-text)] shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-[var(--color-danger-text)] uppercase tracking-widest">Lý do trả lại</p>
                <p class="text-sm font-medium text-body mt-1 leading-relaxed">{{ item.note }}</p>
              </div>
            </div>
          </Transition>
        </div>

        <!-- Empty state -->
        <div
          v-if="filteredSchedules.length === 0"
          key="empty"
          class="flex flex-col items-center justify-center py-20 text-center"
        >
          <div class="h-16 w-16 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <FileText :size="28" class="text-placeholder" />
          </div>
          <p class="text-base font-semibold text-heading">Không tìm thấy bản TKB nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc.</p>
          <button class="mt-4 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="clearFilters(); searchQuery = ''">Xóa tất cả bộ lọc</button>
        </div>
      </TransitionGroup>

      <!-- ── Workflow Note ─────────────────────────────── -->
      <div class="lg-glass-soft p-5 rounded-2xl flex gap-4">
        <div class="h-11 w-11 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
          <Info :size="22" />
        </div>
        <div>
          <h4 class="text-sm font-semibold text-heading">Thông tin quy trình</h4>
          <p class="text-xs text-label mt-1 leading-relaxed">
            Sau khi bạn gửi duyệt, Ban giám hiệu sẽ nhận được thông báo. Bạn không thể chỉnh sửa trực tiếp lịch học khi đang trong trạng thái chờ duyệt.
            Nếu cần chỉnh sửa khẩn cấp, vui lòng sử dụng nút <strong class="text-heading">"Thu hồi"</strong> để chuyển lịch về trạng thái Draft.
            Nếu BGH trả lại, bạn có thể chỉnh sửa rồi <strong class="text-heading">"Gửi lại"</strong>.
          </p>
        </div>
      </div>
    </div>
  </PageContainer>

  <!-- ════════════════════════════════════════════════════════
       Detail Modal
  ════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showDetailModal && selectedSchedule"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeDetail"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-2xl surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <!-- Header -->
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">Chi tiết TKB {{ selectedSchedule.semester }}</h2>
                <p class="text-sm text-label mt-0.5">Lầu {{ selectedSchedule.floor }} · Gửi bởi {{ selectedSchedule.requester }}</p>
              </div>
              <button
                class="h-9 w-9 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeDetail"
              >
                <X :size="18" />
              </button>
            </div>
          </div>

          <!-- Summary stats -->
          <div class="px-6 pt-5 grid grid-cols-3 gap-3">
            <div class="surface-input rounded-xl p-3 border border-default text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Số lớp</p>
              <p class="text-2xl font-semibold text-heading mt-1">{{ selectedSchedule.classCount }}</p>
            </div>
            <div class="surface-input rounded-xl p-3 border border-default text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Số tiết</p>
              <p class="text-2xl font-semibold text-heading mt-1">{{ selectedSchedule.scheduleCount }}</p>
            </div>
            <div class="surface-input rounded-xl p-3 border border-default text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Xung đột</p>
              <p :class="['text-2xl font-semibold mt-1', selectedSchedule.conflictCount > 0 ? 'text-[var(--lg-danger)]' : 'text-[var(--lg-success)]']">
                {{ selectedSchedule.conflictCount }}
              </p>
            </div>
          </div>

          <!-- Classes list -->
          <div class="px-6 pt-5 pb-2">
            <p class="text-xs font-semibold text-label uppercase tracking-widest mb-3">Danh sách lớp học</p>

            <div v-if="selectedSchedule.classes.length === 0" class="py-8 text-center">
              <p class="text-sm text-placeholder italic">Không có dữ liệu chi tiết.</p>
            </div>

            <div v-else class="space-y-2">
              <div
                v-for="cls in selectedSchedule.classes"
                :key="cls.code"
                class="surface-input rounded-xl p-4 border border-default flex flex-col sm:flex-row sm:items-center justify-between gap-3 hover:shadow-sm transition-all"
              >
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
                    <BookOpen :size="18" />
                  </div>
                  <div>
                    <div class="flex items-center gap-2">
                      <p class="text-sm font-semibold text-heading">{{ cls.subject }}</p>
                      <span class="text-[10px] font-bold text-placeholder uppercase tracking-widest">{{ cls.code }}</span>
                    </div>
                    <div class="flex flex-wrap items-center gap-x-4 gap-y-0.5 mt-1">
                      <span class="text-xs font-medium text-label flex items-center gap-1">
                        <User :size="11" class="text-placeholder" /> {{ cls.teacher }}
                      </span>
                      <span class="text-xs font-medium text-label flex items-center gap-1">
                        <Building :size="11" class="text-placeholder" /> {{ cls.room }}
                      </span>
                      <span class="text-xs font-medium text-label flex items-center gap-1">
                        <Clock :size="11" class="text-placeholder" /> {{ cls.schedule }}
                      </span>
                    </div>
                  </div>
                </div>
                <div class="flex items-center gap-1.5 text-xs font-bold text-label shrink-0">
                  <Users :size="13" class="text-placeholder" />
                  {{ cls.students }} SV
                </div>
              </div>
            </div>
          </div>

          <!-- Modal footer -->
          <div class="px-6 pb-6 pt-4 border-t border-default flex items-center justify-between gap-3 mt-2">
            <p class="text-xs text-placeholder">
              Gửi lúc {{ selectedSchedule.submittedDate }}
            </p>
            <div class="flex items-center gap-3">
              <button
                class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
                @click="closeDetail"
              >Đóng</button>
              <button
                v-if="selectedSchedule.status === 'pending'"
                class="lg-button-secondary px-4 py-2.5 text-sm font-bold text-body hover:text-[var(--lg-danger)]"
                @click="closeDetail(); openWithdraw(selectedSchedule)"
              >
                <Undo2 :size="16" /> Thu hồi
              </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════
       Withdraw Confirmation Modal
  ════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showWithdrawModal && withdrawTarget"
        class="fixed inset-0 z-[110] flex items-center justify-center p-4"
        @click.self="closeWithdraw"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="p-6 text-center">
            <div class="h-16 w-16 rounded-3xl bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)] mx-auto mb-4">
              <Undo2 :size="28" />
            </div>
            <h3 class="text-xl font-semibold text-heading">Thu hồi TKB?</h3>
            <p class="text-sm text-label mt-2 leading-relaxed">
              Bạn sắp thu hồi <strong class="text-heading">TKB {{ withdrawTarget.semester }}</strong> (Lầu {{ withdrawTarget.floor }}).
              TKB sẽ được chuyển về trạng thái <strong class="text-heading">Draft</strong> để bạn chỉnh sửa.
            </p>
            <p class="text-xs text-placeholder mt-2">
              Ban giám hiệu sẽ nhận được thông báo thu hồi.
            </p>
          </div>
          <div class="px-6 pb-6 flex items-center justify-center gap-3">
            <button
              class="lg-button-secondary px-6 py-2.5 text-sm font-bold"
              @click="closeWithdraw"
            >Hủy</button>
            <button
              id="btn-confirm-withdraw"
              :class="[
                'px-6 py-2.5 rounded-[18px] text-sm font-bold text-white transition-all flex items-center gap-2',
                isWithdrawing
                  ? 'bg-[var(--lg-danger)] opacity-60 cursor-not-allowed'
                  : 'bg-[var(--lg-danger)] hover:opacity-90 shadow-lg shadow-[var(--lg-danger)]/20'
              ]"
              :disabled="isWithdrawing"
              @click="confirmWithdraw"
            >
              <span v-if="isWithdrawing" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Undo2 v-else :size="16" />
              {{ isWithdrawing ? 'Đang thu hồi...' : 'Xác nhận thu hồi' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════
       New Schedule Submission Modal
  ════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showNewScheduleModal"
        class="fixed inset-0 z-[120] flex items-center justify-center p-4"
        @click.self="closeNewSchedule"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <!-- Header -->
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input flex items-center justify-center">
                  <Send :size="20" class="text-link" />
                </div>
                <div>
                  <h2 class="text-lg font-semibold text-heading">Gửi duyệt TKB mới</h2>
                  <p class="text-xs text-label mt-0.5">Tạo bộ TKB gửi lên Ban giám hiệu</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="closeNewSchedule"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <!-- Body -->
          <div class="p-6 space-y-5">
            <!-- Semester -->
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Học kỳ</label>
              <select
                v-model="newScheduleSemester"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              >
                <option v-for="s in semesterOptions" :key="s" :value="s">{{ s }}</option>
              </select>
            </div>

            <!-- Floor -->
            <div>
              <FloorDropdown
                v-model="newScheduleFloor"
                :floors="FLOORS"
                all-label="Tất cả lầu"
              />
            </div>

            <!-- Draft schedules summary -->
            <div class="surface-input rounded-2xl p-4 border border-default">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)]">
                  <Layers :size="18" />
                </div>
                <div>
                  <p class="text-sm font-semibold text-heading">{{ draftEvents }} lịch học bản nháp</p>
                  <p class="text-xs text-label mt-0.5">
                    Sẽ được gửi lên BGH để phê duyệt
                  </p>
                </div>
              </div>
              <div v-if="draftEvents === 0" class="mt-3 flex items-start gap-2 p-3 rounded-xl bg-[var(--color-warning-bg)]">
                <AlertTriangle :size="14" class="text-[var(--color-warning-text)] shrink-0 mt-0.5" />
                <p class="text-xs font-medium text-[var(--color-warning-text)]">
                  Không có lịch học bản nháp nào để gửi duyệt. Vui lòng tạo lịch trước trong phần <strong class="font-semibold">Quản lý TKB</strong>.
                </p>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeNewSchedule"
            >Hủy</button>
            <button
              id="btn-confirm-send-approval"
              :class="[
                'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
                (isSubmitting || draftEvents === 0) ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="isSubmitting || draftEvents === 0"
              @click="confirmNewSchedule"
            >
              <span v-if="isSubmitting" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Send v-else :size="16" />
              {{ isSubmitting ? 'Đang gửi...' : 'Gửi duyệt' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-header {
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
}

/* Slide-down filter panel */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.25s ease;
}
.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Card list transitions */
.card-list-enter-active,
.card-list-leave-active {
  transition: all 0.35s ease;
}
.card-list-enter-from,
.card-list-leave-to {
  opacity: 0;
  transform: translateY(12px) scale(0.98);
}
.card-list-leave-active {
  position: absolute;
  width: 100%;
}

/* Modal transition */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative.w-full,
.modal-leave-to .relative.w-full {
  transform: scale(0.9) translateY(20px);
}
</style>
