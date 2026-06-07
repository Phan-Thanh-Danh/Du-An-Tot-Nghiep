<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { 
  Search, 
  Filter, 
  TrendingUp, 
  TrendingDown, 
  AlertTriangle, 
  Building,
  Layers,
  Users,
  Edit3,
  Save,
  X,
  Check,
  BookOpen,
  UserCheck
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const popupStore = usePopupStore()

const sections = ref([
  { id: 'LHP001', subject: 'Lập trình Java', teacher: 'Nguyễn Văn A', capacity: 45, enrolled: 45, waitlist: 12, minEnroll: 20, status: 'full', schedule: 'T2-4, 7-9h', room: 'A101', roomCapacity: 60 },
  { id: 'LHP002', subject: 'Cấu trúc dữ liệu', teacher: 'Trần Thị B', capacity: 45, enrolled: 38, waitlist: 0, minEnroll: 20, status: 'open', schedule: 'T3-5, 9-11h', room: 'B203', roomCapacity: 50 },
  { id: 'LHP003', subject: 'Lập trình Web', teacher: 'Lê Văn C', capacity: 40, enrolled: 12, waitlist: 0, minEnroll: 15, status: 'pending_cancel', schedule: 'T4-6, 13-15h', room: 'C305', roomCapacity: 60 },
  { id: 'LHP004', subject: 'Hệ quản trị CSDL', teacher: 'Phạm Minh D', capacity: 45, enrolled: 42, waitlist: 5, minEnroll: 20, status: 'open', schedule: 'T5-7, 15-17h', room: 'A102', roomCapacity: 50 },
])

const searchQuery = ref('')
const showFilters = ref(false)
const filterStatus = ref('all')

const activeFilterCount = computed(() => filterStatus.value !== 'all' ? 1 : 0)

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

function clearAllFilters() {
  filterStatus.value = 'all'
}

const statusOptions = [
  { value: 'all', label: 'Tất cả' },
  { value: 'open', label: 'Đang mở' },
  { value: 'full', label: 'Đã đầy' },
  { value: 'pending_cancel', label: 'Chờ hủy' },
]

const getStatusBadge = (status) => {
  switch (status) {
    case 'open': return 'lg-badge-success'
    case 'full': return 'lg-badge-warning'
    case 'pending_cancel': return 'lg-badge-danger'
    default: return 'surface-solid text-placeholder border-default'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'open': return 'Đang mở'
    case 'full': return 'Đã đầy'
    case 'pending_cancel': return 'Chờ hủy'
    default: return status
  }
}

const selectedSection = ref(null)
const newCapacity = ref(0)
const reason = ref('')
const isProcessing = ref(false)
const showSectionDetail = ref(false)

function selectSection(sec) {
  selectedSection.value = sec
  newCapacity.value = sec.capacity
  reason.value = ''
  showSectionDetail.value = true
}

function closeDetail() {
  showSectionDetail.value = false
  selectedSection.value = null
}

const waitlistImpact = computed(() => {
  if (!selectedSection.value) return 0
  const diff = newCapacity.value - selectedSection.value.capacity
  return diff > 0 ? Math.min(diff, selectedSection.value.waitlist) : 0
})

function handleAdjust() {
  if (!selectedSection.value) return
  if (newCapacity.value < selectedSection.value.enrolled) {
    popupStore.error('Lỗi', 'Sức chứa không thể nhỏ hơn số đã đăng ký.')
    return
  }
  isProcessing.value = true
  setTimeout(() => {
    selectedSection.value.capacity = Number(newCapacity.value)
    isProcessing.value = false
    popupStore.success('Đã cập nhật', `Sức chứa lớp ${selectedSection.value.id} đã được điều chỉnh thành ${selectedSection.value.capacity}.`)
  }, 600)
}

const showQuickModal = ref(false)
const quickTarget = ref(null)
const quickValue = ref(0)

function openQuickModal(sec) {
  quickTarget.value = sec
  quickValue.value = sec.capacity
  showQuickModal.value = true
}

function saveQuick() {
  if (!quickTarget.value) return
  if (quickValue.value < quickTarget.value.enrolled) return
  quickTarget.value.capacity = Number(quickValue.value)
  showQuickModal.value = false
  quickTarget.value = null
}

function closeQuickModal() {
  showQuickModal.value = false
  quickTarget.value = null
}

const metrics = computed(() => {
  const total = sections.value.length
  const enrolled = sections.value.reduce((s, sec) => s + sec.enrolled, 0)
  const waitlisted = sections.value.reduce((s, sec) => s + sec.waitlist, 0)
  const totalCapacity = sections.value.reduce((s, sec) => s + sec.capacity, 0)
  return { total, enrolled, waitlisted, totalCapacity }
})
</script>

<template>
  <PageContainer 
    title="Điều chỉnh sức chứa" 
    subtitle="Tăng hoặc giảm số lượng chỗ trống của lớp học phần."
  >
    <div class="space-y-4">

      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Layers :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Tổng lớp</p>
            <p class="text-xl font-semibold text-heading">{{ metrics.total }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Users :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Tổng sức chứa</p>
            <p class="text-xl font-semibold text-heading">{{ metrics.totalCapacity }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <UserCheck :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Đã đăng ký</p>
            <p class="text-xl font-semibold text-heading">{{ metrics.enrolled.toLocaleString() }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <TrendingUp :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Đang Waitlist</p>
            <p class="text-xl font-semibold text-heading">{{ metrics.waitlisted }}</p>
          </div>
        </div>
      </div>

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
            <span v-if="activeFilterCount > 0" class="absolute -top-1.5 -right-1.5 h-4 w-4 rounded-full bg-[var(--lg-primary)] text-white text-[9px] font-semibold flex items-center justify-center">{{ activeFilterCount }}</span>
          </button>
        </div>
      </div>

      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-2"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-2"
      >
        <div v-if="showFilters" class="lg-glass-strong p-5 rounded-[20px] space-y-3">
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-semibold text-label uppercase tracking-widest min-w-[70px]">Trạng thái:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="opt in statusOptions" :key="opt.value"
                @click="filterStatus = opt.value"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterStatus === opt.value ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ opt.label }}</button>
            </div>
          </div>
          <div v-if="activeFilterCount > 0" class="pt-2 border-t border-default flex justify-end">
            <button class="text-[11px] font-bold text-placeholder hover:text-label transition-colors flex items-center gap-1" @click="clearAllFilters">
              <X :size="13" /> Xóa tất cả bộ lọc
            </button>
          </div>
        </div>
      </Transition>

      <div class="lg-table-shell overflow-hidden rounded-[24px]">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Lớp HP</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Môn học</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Sức chứa</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Đã ĐK</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Chờ</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="sec in filteredSections" :key="sec.id"
              :class="['group hover:bg-[var(--surface-input)] transition-colors cursor-pointer', selectedSection?.id === sec.id ? 'bg-[var(--color-info-bg)]' : '']"
              @click="selectSection(sec)"
            >
              <td class="px-4 py-4">
                <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ sec.id }}</span>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading">{{ sec.subject }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-0.5">{{ sec.teacher }}</p>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <span class="text-sm font-semibold text-heading">{{ sec.capacity }}</span>
                  <span class="text-[10px] font-medium text-placeholder">SV</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1.5">
                  <div class="h-2 w-16 rounded-full surface-solid overflow-hidden">
                    <div class="h-full rounded-full transition-all"
                      :class="sec.enrolled >= sec.capacity ? 'bg-[var(--lg-warning)]' : 'bg-[var(--lg-primary)]'"
                      :style="{ width: Math.min((sec.enrolled / Math.max(sec.capacity, 1)) * 100, 100) + '%' }"
                    />
                  </div>
                  <span class="text-[11px] font-bold text-label">{{ sec.enrolled }}/{{ sec.capacity }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span v-if="sec.waitlist > 0" class="text-xs font-semibold text-[var(--lg-warning)]">{{ sec.waitlist }}</span>
                <span v-else class="text-xs font-medium text-placeholder">0</span>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border', getStatusBadge(sec.status)]">
                  {{ getStatusLabel(sec.status) }}
                </span>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1" @click.stop>
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all" title="Điều chỉnh sức chứa" @click="openQuickModal(sec)">
                    <Edit3 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] rounded-lg text-placeholder transition-all" title="Xem chi tiết & điều chỉnh" @click="selectSection(sec)">
                    <TrendingUp :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredSections.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <BookOpen :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-semibold text-heading">Không có lớp học phần nào</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</p>
        </div>
      </div>

      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-200 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 translate-y-4"
      >
        <div v-if="showSectionDetail && selectedSection" class="lg-card-glass p-6 rounded-[24px]">
          <div class="flex items-center justify-between mb-6">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Layers :size="20" />
              </div>
              <div>
                <h3 class="text-base font-semibold text-heading">{{ selectedSection.subject }}</h3>
                <div class="flex items-center gap-2 mt-0.5">
                  <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ selectedSection.id }}</span>
                  <span class="text-[11px] font-bold text-placeholder">{{ selectedSection.teacher }}</span>
                  <span class="w-1 h-1 rounded-full bg-placeholder" />
                  <span class="text-[11px] font-bold text-placeholder">{{ selectedSection.room }}</span>
                </div>
              </div>
            </div>
            <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder transition-all" @click="closeDetail">
              <X :size="18" />
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div class="surface-solid rounded-2xl p-4 text-center">
              <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest mb-2">Sức chứa hiện tại</p>
              <p class="text-2xl font-semibold text-heading">{{ selectedSection.capacity }}</p>
              <p class="text-[11px] font-bold text-label mt-1">SV</p>
            </div>
            <div class="surface-solid rounded-2xl p-4 text-center">
              <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest mb-2">Đã đăng ký</p>
              <p class="text-2xl font-semibold text-heading">{{ selectedSection.enrolled }}</p>
              <p class="text-[11px] font-bold text-label mt-1">SV</p>
            </div>
            <div class="surface-solid rounded-2xl p-4 text-center border border-[var(--color-warning-bg)]/30">
              <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest mb-2">Đang đợi (Waitlist)</p>
              <p class="text-2xl font-semibold text-[var(--lg-warning)]">{{ selectedSection.waitlist }}</p>
              <p class="text-[11px] font-bold text-[var(--lg-warning)] mt-1">SV trong hàng chờ</p>
            </div>
          </div>

          <div class="border-t border-default pt-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
              <div class="space-y-4">
                <div class="space-y-2">
                  <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Sức chứa mới</label>
                  <div class="flex items-center gap-4">
                    <input 
                      v-model="newCapacity" 
                      type="number" 
                      :min="selectedSection.enrolled"
                      class="flex-1 lg-input px-4 py-3 text-lg font-semibold text-heading transition-all"
                    >
                    <div class="flex flex-col gap-1">
                      <button @click="newCapacity++" class="p-1 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder"><TrendingUp :size="16" /></button>
                      <button @click="newCapacity--" class="p-1 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder"><TrendingDown :size="16" /></button>
                    </div>
                  </div>
                  <p v-if="newCapacity < selectedSection.enrolled" class="text-[11px] font-bold text-[var(--lg-danger)] mt-1">
                    Sức chứa không thể nhỏ hơn số đã đăng ký ({{ selectedSection.enrolled }})
                  </p>
                </div>
                <div class="space-y-2">
                  <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Lý do điều chỉnh</label>
                  <textarea 
                    v-model="reason" 
                    placeholder="Nhập lý do (Ví dụ: Theo nhu cầu SV, Đổi phòng lớn hơn...)"
                    class="w-full lg-input px-4 py-3 text-sm font-medium h-24 resize-none"
                  ></textarea>
                </div>
              </div>
              <div class="surface-solid rounded-[24px] p-4 border border-default space-y-4">
                <h4 class="text-xs font-semibold text-label uppercase tracking-widest">Xem trước ảnh hưởng</h4>
                <div class="space-y-4">
                  <div class="flex items-center justify-between p-3 surface-card rounded-xl border border-default">
                    <div class="flex items-center gap-3">
                      <Building :size="18" class="text-placeholder" />
                      <span class="text-xs font-bold text-label">Giới hạn phòng học</span>
                    </div>
                    <span class="text-xs font-semibold text-heading">{{ selectedSection.roomCapacity }} SV</span>
                  </div>
                  <div class="flex items-center justify-between p-3 surface-card rounded-xl border border-default">
                    <div class="flex items-center gap-3">
                      <TrendingUp :size="18" class="text-[var(--lg-success)]" />
                      <span class="text-xs font-bold text-label">Đẩy từ Waitlist</span>
                    </div>
                    <span class="text-xs font-semibold text-[var(--lg-success)]">{{ waitlistImpact > 0 ? '+' + waitlistImpact : 0 }} SV</span>
                  </div>
                  <div class="flex items-center justify-between p-3 surface-card rounded-xl border border-default">
                    <div class="flex items-center gap-3">
                      <Users :size="18" class="text-link" />
                      <span class="text-xs font-bold text-label">Chênh lệch</span>
                    </div>
                    <span class="text-xs font-semibold" :class="newCapacity >= selectedSection.capacity ? 'text-[var(--lg-success)]' : 'text-[var(--lg-danger)]'">
                      {{ newCapacity >= selectedSection.capacity ? '+' : '' }}{{ newCapacity - selectedSection.capacity }}
                    </span>
                  </div>
                  <div v-if="newCapacity > selectedSection.roomCapacity" class="p-4 bg-[var(--color-danger-bg)] rounded-xl border border-[var(--color-danger-bg)]/50 flex gap-3">
                    <AlertTriangle :size="18" class="text-[var(--lg-danger)] shrink-0" />
                    <p class="text-[11px] font-bold text-[var(--lg-danger)]">Sức chứa mới vượt quá giới hạn của phòng học hiện tại. Vui lòng đổi phòng sau khi cập nhật.</p>
                  </div>
                </div>
              </div>
            </div>
            <div class="mt-6 flex items-center justify-end gap-4 border-t border-default pt-6">
              <button class="px-4 py-2.5 text-sm font-bold text-label hover:text-heading transition-colors" @click="closeDetail">Hủy bỏ</button>
              <button 
                @click="handleAdjust"
                :disabled="isProcessing || newCapacity < selectedSection.enrolled"
                class="lg-button-primary px-5 py-2.5 text-sm font-bold disabled:opacity-50"
              >
                <Save :size="16" />
                {{ isProcessing ? 'Đang xử lý...' : 'Áp dụng thay đổi' }}
              </button>
            </div>
          </div>
        </div>
      </Transition>

    </div>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showQuickModal && quickTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeQuickModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Edit3 :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Điều chỉnh sức chứa</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeQuickModal">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div class="surface-solid p-4 rounded-2xl">
              <p class="text-sm font-semibold text-heading">{{ quickTarget.subject }}</p>
              <div class="flex items-center gap-2 mt-1">
                <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ quickTarget.id }}</span>
                <span class="text-[11px] font-bold text-placeholder">{{ quickTarget.teacher }}</span>
              </div>
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Sức chứa hiện tại: <span class="text-heading">{{ quickTarget.capacity }}</span></label>
              <input v-model.number="quickValue" type="number" :min="quickTarget.enrolled" class="w-full lg-input px-4 py-2.5 text-sm" />
              <p v-if="quickValue < quickTarget.enrolled" class="text-[11px] font-bold text-[var(--lg-danger)] mt-1">Sức chứa không thể nhỏ hơn số đã đăng ký ({{ quickTarget.enrolled }})</p>
              <div v-if="quickValue > quickTarget.capacity && quickTarget.waitlist > 0" class="mt-2 flex items-center gap-2 text-[11px] font-medium text-label">
                <TrendingUp :size="14" class="text-[var(--lg-success)]" />
                <span>Sẽ đẩy <strong class="text-heading">{{ Math.min(quickValue - quickTarget.capacity, quickTarget.waitlist) }}</strong> SV từ waitlist</span>
              </div>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeQuickModal">Hủy</button>
            <button 
              class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2"
              :disabled="quickValue < quickTarget.enrolled"
              :class="{ 'opacity-50 cursor-not-allowed': quickValue < quickTarget.enrolled }"
              @click="saveQuick"
            >
              <Check :size="16" /> Lưu
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
