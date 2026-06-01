<script setup>
import { ref, computed } from 'vue'
import { 
  Plus, 
  Search, 
  Filter, 
  Users, 
  UserCheck, 
  UserMinus, 
  AlertCircle,
  MoreVertical,
  Layers,
  Edit3,
  Power,
  TrendingUp,
  X,
  Check,
  BookOpen,
  Clock,
  Hash,
  User,
  Save,
  Trash2,
  Eye,
  CalendarDays
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const sections = ref([
  { id: 'LHP001', subject: 'Lập trình Java', teacher: 'Nguyễn Văn A', capacity: 45, enrolled: 45, waitlist: 12, minEnroll: 20, status: 'full', schedule: 'T2-4, 7-9h', room: 'A101' },
  { id: 'LHP002', subject: 'Cấu trúc dữ liệu', teacher: 'Trần Thị B', capacity: 45, enrolled: 38, waitlist: 0, minEnroll: 20, status: 'open', schedule: 'T3-5, 9-11h', room: 'B203' },
  { id: 'LHP003', subject: 'Lập trình Web', teacher: 'Lê Văn C', capacity: 40, enrolled: 12, waitlist: 0, minEnroll: 15, status: 'pending_cancel', schedule: 'T4-6, 13-15h', room: 'C305' },
  { id: 'LHP004', subject: 'Hệ quản trị CSDL', teacher: 'Phạm Minh D', capacity: 45, enrolled: 42, waitlist: 5, minEnroll: 20, status: 'open', schedule: 'T5-7, 15-17h', room: 'A102' },
])

const nextId = ref(5)

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const showFilters = ref(false)
const filterStatus = ref('all')

const filteredSections = computed(() => {
  let result = sections.value
  const q = searchQuery.value.toLowerCase().trim()
  if (q) {
    result = result.filter(s =>
      s.id.toLowerCase().includes(q) ||
      s.subject.toLowerCase().includes(q) ||
      s.teacher.toLowerCase().includes(q)
    )
  }
  if (filterStatus.value !== 'all') {
    result = result.filter(s => s.status === filterStatus.value)
  }
  return result
})

const statusOptions = [
  { value: 'all', label: 'Tất cả' },
  { value: 'open', label: 'Đang mở' },
  { value: 'full', label: 'Đã đầy' },
  { value: 'pending_cancel', label: 'Chờ hủy' },
  { value: 'cancelled', label: 'Đã hủy' },
]

const activeFiltersCount = computed(() => {
  let count = 0
  if (filterStatus.value !== 'all') count++
  return count
})

// ── Create Section ───────────────────────────────────────────
const showCreateModal = ref(false)
const dayOptions = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN']
const createForm = ref({
  subject: '',
  teacher: '',
  capacity: 40,
  minEnroll: 15,
  selectedDays: [],
  startTime: '07',
  endTime: '09',
  room: '',
})

const dayLabels = { T2: 'T2', T3: 'T3', T4: 'T4', T5: 'T5', T6: 'T6', T7: 'T7', CN: 'CN' }

function toggleDay(day) {
  const idx = createForm.value.selectedDays.indexOf(day)
  if (idx === -1) {
    createForm.value.selectedDays.push(day)
  } else {
    createForm.value.selectedDays.splice(idx, 1)
  }
}

function openCreateModal() {
  createForm.value = { subject: '', teacher: '', capacity: 40, minEnroll: 15, selectedDays: [], startTime: '07', endTime: '09', room: '' }
  showCreateModal.value = true
}

const scheduleDisplay = computed(() => {
  const f = createForm.value
  if (!f.selectedDays.length) return ''
  const days = f.selectedDays.sort((a, b) => {
    const order = { T2: 2, T3: 3, T4: 4, T5: 5, T6: 6, T7: 7, CN: 8 }
    return (order[a] || 0) - (order[b] || 0)
  })
  if (days.length === 1) return `${days[0]}, ${f.startTime}-${f.endTime}h`
  const first = days[0]
  const last = days[days.length - 1]
  if (days.length === 2) return `${first},${last}, ${f.startTime}-${f.endTime}h`
  if (days.every((d, i) => !i || orderDiff(days[i-1], d) === 1)) {
    return `${first}-${last.replace('T', '')}, ${f.startTime}-${f.endTime}h`
  }
  return `${days.join(',')}, ${f.startTime}-${f.endTime}h`
})

function orderDiff(a, b) {
  const order = { T2: 2, T3: 3, T4: 4, T5: 5, T6: 6, T7: 7, CN: 8 }
  return (order[b] || 0) - (order[a] || 0)
}

function createSection() {
  if (!createForm.value.selectedDays.length) return
  const newSection = {
    id: `LHP${String(nextId.value).padStart(3, '0')}`,
    subject: createForm.value.subject,
    teacher: createForm.value.teacher,
    capacity: Number(createForm.value.capacity),
    enrolled: 0,
    waitlist: 0,
    minEnroll: Number(createForm.value.minEnroll),
    status: 'open',
    schedule: scheduleDisplay.value,
    room: createForm.value.room,
  }
  sections.value.unshift(newSection)
  nextId.value++
  showCreateModal.value = false
}

// ── Edit Capacity ────────────────────────────────────────────
const showCapacityModal = ref(false)
const capacityTarget = ref(null)
const capacityEditValue = ref(0)

function openCapacityModal(sec) {
  capacityTarget.value = sec
  capacityEditValue.value = sec.capacity
  showCapacityModal.value = true
}

function saveCapacity() {
  if (!capacityTarget.value) return
  const val = Number(capacityEditValue.value)
  if (val < capacityTarget.value.enrolled) return
  capacityTarget.value.capacity = val
  showCapacityModal.value = false
  capacityTarget.value = null
}

// ── Cancel Section ───────────────────────────────────────────
const showCancelModal = ref(false)
const cancelTarget = ref(null)
const cancelReason = ref('')

function openCancelModal(sec) {
  cancelTarget.value = sec
  cancelReason.value = ''
  showCancelModal.value = true
}

function confirmCancel() {
  if (!cancelTarget.value) return
  cancelTarget.value.status = 'cancelled'
  showCancelModal.value = false
  cancelTarget.value = null
  cancelReason.value = ''
}

// ── Context Menu ─────────────────────────────────────────────
const contextTarget = ref(null)

function toggleContextMenu(sec) {
  contextTarget.value = contextTarget.value?.id === sec.id ? null : sec
}

function closeContextMenu() {
  contextTarget.value = null
}

// ── Utility ──────────────────────────────────────────────────
const getStatusBadge = (status) => {
  switch (status) {
    case 'open': return 'lg-badge-success'
    case 'full': return 'lg-badge-warning'
    case 'pending_cancel': return 'lg-badge-danger'
    case 'cancelled': return 'surface-solid text-placeholder border-default'
    default: return 'surface-solid text-placeholder'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'open': return 'Đang mở'
    case 'full': return 'Đã đầy'
    case 'pending_cancel': return 'Chờ hủy'
    case 'cancelled': return 'Đã hủy'
    default: return status
  }
}

const metrics = computed(() => {
  const total = sections.value.length
  const registered = sections.value.reduce((s, sec) => s + sec.enrolled, 0)
  const waitlisted = sections.value.reduce((s, sec) => s + sec.waitlist, 0)
  const needsAction = sections.value.filter(s => s.status === 'pending_cancel' || (s.status === 'open' && s.enrolled < s.minEnroll)).length
  return { total, registered, waitlisted, needsAction }
})
</script>

<template>
  <PageContainer 
    title="Lớp học phần" 
    subtitle="Quản lý danh sách các lớp mở trong học kỳ, theo dõi sĩ số và trạng thái lớp."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20" @click="openCreateModal">
        <Plus :size="18" /> Tạo lớp mới
      </button>
    </template>

    <div class="space-y-4" @click="closeContextMenu">
      
      <!-- ── Quick Metrics ── -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Layers :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Tổng số lớp</p>
             <p class="text-xl font-black text-heading">{{ metrics.total }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <UserCheck :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Đã đăng ký</p>
             <p class="text-xl font-black text-heading">{{ metrics.registered.toLocaleString() }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <TrendingUp :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Đang Waitlist</p>
             <p class="text-xl font-black text-heading">{{ metrics.waitlisted }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <AlertCircle :size="20" />
          </div>
          <div>
             <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Cần xử lý</p>
             <p class="text-xl font-black text-heading">{{ metrics.needsAction }}</p>
          </div>
        </div>
      </div>

      <!-- ── Search Bar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input 
            v-model="searchQuery"
            type="text" 
            placeholder="Tìm theo mã lớp, môn học hoặc giảng viên..." 
            class="w-full lg-input pl-11 pr-10 py-2.5 text-sm font-medium transition-all"
          >
          <button v-if="searchQuery" @click="searchQuery = ''" class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label transition-colors">
            <X :size="16" />
          </button>
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold relative" @click.stop="showFilters = !showFilters">
            <Filter :size="18" /> Bộ lọc
            <span v-if="activeFiltersCount > 0" class="absolute -top-1.5 -right-1.5 h-4 w-4 rounded-full bg-[var(--lg-primary)] text-white text-[9px] font-black flex items-center justify-center">{{ activeFiltersCount }}</span>
          </button>
        </div>
      </div>

      <!-- ── Filter Panel ── -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-2"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-2"
      >
        <div v-if="showFilters" class="lg-glass-strong p-4 rounded-[20px] flex flex-wrap items-center gap-4">
          <div class="flex items-center gap-2">
            <span class="text-[10px] font-black text-label uppercase tracking-widest">Trạng thái:</span>
            <div class="flex gap-1.5">
              <button
                v-for="opt in statusOptions"
                :key="opt.value"
                @click="filterStatus = opt.value"
                :class="[
                  'px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all',
                  filterStatus === opt.value
                    ? 'bg-[var(--lg-primary)] text-white shadow-md'
                    : 'surface-solid text-label hover:bg-[var(--surface-input)]'
                ]"
              >{{ opt.label }}</button>
            </div>
          </div>
          <button v-if="activeFiltersCount > 0" class="text-[11px] font-bold text-placeholder hover:text-label transition-colors ml-auto" @click="filterStatus = 'all'">
            Xóa bộ lọc
          </button>
        </div>
      </Transition>

      <!-- ── Sections Table ── -->
      <div class="lg-table-shell overflow-hidden rounded-[24px]">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Mã LHP</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Môn & Giảng viên</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Lịch học</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Đăng ký / Sức chứa</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="sec in filteredSections" :key="sec.id" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                <span class="text-xs font-black text-heading">{{ sec.id }}</span>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading leading-tight">{{ sec.subject }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-1 uppercase tracking-tighter">{{ sec.teacher }}</p>
              </td>
              <td class="px-4 py-4">
                <p class="text-xs font-semibold text-label">{{ sec.schedule }}</p>
                <p class="text-[10px] font-medium text-placeholder">{{ sec.room }}</p>
              </td>
              <td class="px-4 py-4">
                <div class="space-y-1.5">
                  <div class="flex items-center justify-between">
                    <span class="text-[11px] font-black text-label">{{ sec.enrolled }} / {{ sec.capacity }}</span>
                    <span v-if="sec.waitlist > 0" class="text-[10px] font-bold text-[var(--lg-warning)]">+{{ sec.waitlist }} waitlist</span>
                  </div>
                  <div class="h-1.5 w-32 bg-[var(--border-default)] rounded-full overflow-hidden">
                    <div 
                      :class="['h-full transition-all', sec.enrolled >= sec.capacity ? 'bg-[var(--lg-warning)]' : 'bg-[var(--lg-primary)]']"
                      :style="{ width: Math.min((sec.enrolled / sec.capacity) * 100, 100) + '%' }"
                    ></div>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border', getStatusBadge(sec.status)]">
                  {{ getStatusLabel(sec.status) }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all" title="Sửa sức chứa" @click.stop="openCapacityModal(sec)">
                    <Edit3 :size="16" />
                  </button>
                  <button v-if="sec.status !== 'cancelled'" class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] rounded-lg text-placeholder transition-all" title="Hủy lớp" @click.stop="openCancelModal(sec)">
                    <Power :size="16" />
                  </button>
                  <div class="relative">
                    <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder transition-all" @click.stop="toggleContextMenu(sec)">
                      <MoreVertical :size="16" />
                    </button>
                    <Transition
                      enter-active-class="transition-all duration-150 ease-out"
                      enter-from-class="opacity-0 scale-95"
                      enter-to-class="opacity-100 scale-100"
                      leave-active-class="transition-all duration-100 ease-in"
                      leave-from-class="opacity-100 scale-100"
                      leave-to-class="opacity-0 scale-95"
                    >
                      <div v-if="contextTarget?.id === sec.id" class="absolute right-0 top-full mt-1 z-50 w-44 lg-glass-strong rounded-xl p-1 shadow-xl shadow-slate-900/10" @click.stop>
                        <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="openCapacityModal(sec); closeContextMenu()">
                          <Edit3 :size="14" /> Sửa sức chứa
                        </button>
                        <button v-if="sec.status === 'cancelled'" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] transition-all" @click="sec.status = 'open'; closeContextMenu()">
                          <Eye :size="14" /> Mở lại lớp
                        </button>
                        <button v-if="sec.status !== 'cancelled'" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all" @click="openCancelModal(sec); closeContextMenu()">
                          <Power :size="14" /> Hủy lớp
                        </button>
                      </div>
                    </Transition>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredSections.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <BookOpen :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-black text-heading">Không tìm thấy lớp nào</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</p>
        </div>
      </div>

    </div>

    <!-- ═══════════════════════════════════════════════════════
         CREATE SECTION MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showCreateModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showCreateModal = false">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-lg lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <h3 class="text-base font-black text-heading">Tạo lớp học phần mới</h3>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showCreateModal = false">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Môn học</label>
              <input v-model="createForm.subject" type="text" placeholder="VD: Lập trình Python" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Giảng viên</label>
                <input v-model="createForm.teacher" type="text" placeholder="Họ tên GV" class="w-full lg-input px-4 py-2.5 text-sm" />
              </div>
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Phòng học</label>
                <input v-model="createForm.room" type="text" placeholder="VD: A101" class="w-full lg-input px-4 py-2.5 text-sm" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Sức chứa</label>
                <input v-model.number="createForm.capacity" type="number" min="1" class="w-full lg-input px-4 py-2.5 text-sm" />
              </div>
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Sĩ số tối thiểu</label>
                <input v-model.number="createForm.minEnroll" type="number" min="1" class="w-full lg-input px-4 py-2.5 text-sm" />
              </div>
            </div>
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Lịch học</label>
              <div class="space-y-2.5">
                <div class="flex flex-wrap gap-1.5">
                  <button
                    v-for="day in dayOptions"
                    :key="day"
                    type="button"
                    @click="toggleDay(day)"
                    :class="[
                      'px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all',
                      createForm.selectedDays.includes(day)
                        ? 'bg-[var(--lg-primary)] text-white shadow-md'
                        : 'surface-solid text-label hover:bg-[var(--surface-input)]'
                    ]"
                  >{{ dayLabels[day] }}</button>
                </div>
                <div class="flex items-center gap-2">
                  <div class="relative flex-1">
                    <Clock :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
                    <select v-model="createForm.startTime" class="w-full lg-input pl-9 pr-3 py-2 text-sm appearance-none cursor-pointer">
                      <option v-for="h in 24" :key="h" :value="String(h - 1).padStart(2, '0')">{{ String(h - 1).padStart(2, '0') }}:00</option>
                    </select>
                  </div>
                  <span class="text-[11px] font-bold text-label">→</span>
                  <div class="relative flex-1">
                    <Clock :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
                    <select v-model="createForm.endTime" class="w-full lg-input pl-9 pr-3 py-2 text-sm appearance-none cursor-pointer">
                      <option v-for="h in 24" :key="h" :value="String(h).padStart(2, '0')">{{ String(h).padStart(2, '0') }}:00</option>
                    </select>
                  </div>
                </div>
                <div v-if="createForm.selectedDays.length" class="text-[11px] font-medium text-link">
                  <CalendarDays :size="13" class="inline mr-1" />{{ scheduleDisplay }}
                </div>
              </div>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="showCreateModal = false">Hủy</button>
            <button 
              class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20"
              :disabled="!createForm.subject || !createForm.teacher || !createForm.selectedDays.length"
              :class="{ 'opacity-50 cursor-not-allowed': !createForm.subject || !createForm.teacher || !createForm.selectedDays.length }"
              @click="createSection"
            >
              <Save :size="16" /> Tạo lớp
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════
         EDIT CAPACITY MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showCapacityModal && capacityTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showCapacityModal = false">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <h3 class="text-base font-black text-heading">Điều chỉnh sức chứa</h3>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showCapacityModal = false">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div class="surface-solid p-4 rounded-2xl">
              <p class="text-sm font-black text-heading">{{ capacityTarget.subject }}</p>
              <p class="text-[11px] font-bold text-placeholder mt-0.5">{{ capacityTarget.id }} · {{ capacityTarget.teacher }}</p>
            </div>
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Sức chứa hiện tại: <span class="text-heading">{{ capacityTarget.capacity }}</span></label>
              <input v-model.number="capacityEditValue" type="number" :min="capacityTarget.enrolled" class="w-full lg-input px-4 py-2.5 text-sm" />
              <p v-if="capacityEditValue < capacityTarget.enrolled" class="text-[11px] font-bold text-[var(--lg-danger)] mt-1">Sức chứa không thể nhỏ hơn số đã đăng ký ({{ capacityTarget.enrolled }})</p>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="showCapacityModal = false">Hủy</button>
            <button 
              class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20"
              :disabled="capacityEditValue < capacityTarget.enrolled"
              :class="{ 'opacity-50 cursor-not-allowed': capacityEditValue < capacityTarget.enrolled }"
              @click="saveCapacity"
            >
              <Check :size="16" /> Lưu
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════
         CANCEL SECTION MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showCancelModal && cancelTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showCancelModal = false">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <h3 class="text-base font-black text-heading">Xác nhận hủy lớp</h3>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showCancelModal = false">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div class="surface-solid p-4 rounded-2xl flex items-center gap-3">
              <div class="h-10 w-10 rounded-xl bg-[var(--color-danger-bg)] text-[var(--lg-danger)] flex items-center justify-center flex-shrink-0">
                <AlertCircle :size="20" />
              </div>
              <div>
                <p class="text-sm font-black text-heading">{{ cancelTarget.subject }}</p>
                <p class="text-[11px] font-bold text-placeholder">{{ cancelTarget.id }} · {{ cancelTarget.teacher }} · {{ cancelTarget.enrolled }} SV</p>
              </div>
            </div>
            <div class="bg-[var(--color-warning-bg)] border border-[var(--lg-warning)]/30 rounded-2xl p-3">
              <p class="text-[11px] font-bold text-[var(--lg-warning)]">Hành động này sẽ hủy lớp và thông báo đến {{ cancelTarget.enrolled }} sinh viên đã đăng ký.</p>
            </div>
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Lý do hủy</label>
              <textarea v-model="cancelReason" rows="2" placeholder="Nhập lý do hủy lớp..." class="w-full lg-input px-4 py-2.5 text-sm resize-none"></textarea>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="showCancelModal = false">Quay lại</button>
            <button class="lg-btn-danger px-5 py-2.5 text-sm font-bold" @click="confirmCancel">
              <Trash2 :size="16" /> Xác nhận hủy
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
