<script setup>
import { ref, computed } from 'vue'
import { 
  Search, 
  Filter, 
  CheckCircle2, 
  XCircle, 
  Bell, 

  MoreVertical,
  ArrowLeftRight,
  X,
  BookOpen,
  Check,
  AlertCircle,
  Trash2,
  Loader2,
  Layers,
  UserCheck,
  UserX
} from 'lucide-vue-next'
import PageContainer from '../../../components/SinhVien/PageContainer.vue'

const waitlist = ref([
  { id: 1, position: 1, student: 'Lê Thị Mai', studentCode: 'SV002', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 08:30', status: 'waiting' },
  { id: 2, position: 2, student: 'Trần Văn Hoàng', studentCode: 'SV005', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 09:15', status: 'waiting' },
  { id: 3, position: 3, student: 'Nguyễn Bích Liên', studentCode: 'SV008', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 10:00', status: 'confirmed' },
  { id: 4, position: 4, student: 'Hoàng Minh Tuấn', studentCode: 'SV012', section: 'LHP001', subject: 'Lập trình Java', time: '16/01/2026 11:20', status: 'expired' },
])

const sections = ['LHP001', 'LHP002', 'LHP003', 'LHP004']
const subjectMap = { LHP001: 'Lập trình Java', LHP002: 'Cấu trúc dữ liệu', LHP003: 'Lập trình Web', LHP004: 'Hệ quản trị CSDL' }

const searchQuery = ref('')
const searchTriggered = ref('')
const showFilters = ref(false)
const filterSection = ref('all')

const activeFilterCount = computed(() => filterSection.value !== 'all' ? 1 : 0)

function doSearch() {
  searchTriggered.value = searchQuery.value
}

function clearSearch() {
  searchQuery.value = ''
  searchTriggered.value = ''
}

function clearAllFilters() {
  filterSection.value = 'all'
}

const filteredWaitlist = computed(() => {
  let result = waitlist.value
  const q = searchTriggered.value.toLowerCase().trim()
  if (q) {
    result = result.filter(item =>
      item.student.toLowerCase().includes(q) ||
      item.studentCode.toLowerCase().includes(q) ||
      item.section.toLowerCase().includes(q) ||
      item.subject.toLowerCase().includes(q)
    )
  }
  if (filterSection.value !== 'all') {
    result = result.filter(item => item.section === filterSection.value)
  }
  return result
})

const getStatusBadge = (status) => {
  switch (status) {
    case 'waiting': return 'lg-badge-warning'
    case 'confirmed': return 'lg-badge-success'
    case 'expired': return 'lg-badge-danger'
    case 'removed': return 'text-placeholder surface-solid'
    default: return 'surface-solid text-placeholder'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'waiting': return 'Đang chờ'
    case 'confirmed': return 'Đã xác nhận'
    case 'expired': return 'Hết hạn'
    case 'removed': return 'Đã loại'
    default: return status
  }
}

const contextTarget = ref(null)

function toggleContextMenu(item) {
  contextTarget.value = contextTarget.value?.id === item.id ? null : item
}

function closeContextMenu() {
  contextTarget.value = null
}

const showConfirmModal = ref(false)
const confirmTarget = ref(null)
const confirming = ref(false)
const confirmDone = ref(false)

function openConfirmModal(item) {
  confirmTarget.value = item
  confirming.value = false
  confirmDone.value = false
  showConfirmModal.value = true
}

function doConfirm() {
  if (!confirmTarget.value) return
  confirming.value = true
  setTimeout(() => {
    const idx = waitlist.value.findIndex(w => w.id === confirmTarget.value.id)
    if (idx !== -1) {
      waitlist.value[idx] = { ...waitlist.value[idx], status: 'confirmed' }
    }
    confirming.value = false
    confirmDone.value = true
    setTimeout(() => { showConfirmModal.value = false; confirmTarget.value = null }, 1000)
  }, 600)
}

function closeConfirmModal() {
  showConfirmModal.value = false
  confirmTarget.value = null
}

const showRemoveModal = ref(false)
const removeTarget = ref(null)
const removeReason = ref('')

function openRemoveModal(item) {
  removeTarget.value = item
  removeReason.value = ''
  showRemoveModal.value = true
}

function doRemove() {
  if (!removeTarget.value) return
  const idx = waitlist.value.findIndex(w => w.id === removeTarget.value.id)
  if (idx !== -1) {
    waitlist.value[idx] = { ...waitlist.value[idx], status: 'removed' }
  }
  showRemoveModal.value = false
  removeTarget.value = null
  removeReason.value = ''
}

function closeRemoveModal() {
  showRemoveModal.value = false
  removeTarget.value = null
  removeReason.value = ''
}

const showTransferModal = ref(false)
const transferTarget = ref(null)
const transferSection = ref('')
const transferError = ref('')

function openTransferModal(item) {
  transferTarget.value = item
  transferSection.value = ''
  transferError.value = ''
  showTransferModal.value = true
}

function confirmTransfer() {
  if (!transferTarget.value || !transferSection.value) return
  const dup = waitlist.value.find(w =>
    w.studentCode === transferTarget.value.studentCode &&
    w.section === transferSection.value &&
    w.status !== 'removed'
  )
  if (dup) {
    transferError.value = `Sinh viên đã trong hàng chờ lớp ${transferSection.value}`
    return
  }
  transferTarget.value.section = transferSection.value
  transferTarget.value.subject = subjectMap[transferSection.value] || transferSection.value
  closeTransferModal()
}

function closeTransferModal() {
  showTransferModal.value = false
  transferTarget.value = null
  transferSection.value = ''
  transferError.value = ''
}
</script>

<template>
  <PageContainer 
    title="Danh sách hàng chờ (Waitlist)" 
    subtitle="Quản lý thứ tự đăng ký khi lớp học phần đã đầy sức chứa."
  >
    <div class="space-y-4" @click="closeContextMenu">

      <div class="lg-glass-strong p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative flex items-center gap-2">
          <div class="relative flex-1">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
            <input 
              v-model="searchQuery"
              type="text" 
              placeholder="Tìm theo SV hoặc Lớp học phần..." 
              class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium transition-all"
              @keyup.enter="doSearch"
            >
          </div>
          <button class="lg-button-primary px-4 py-2.5 text-sm font-bold" @click="doSearch">
            <Search :size="16" /> Tìm
          </button>
          <button v-if="searchTriggered" class="lg-button-secondary px-3 py-2.5 text-sm font-bold" @click="clearSearch" title="Xóa tìm kiếm">
            <X :size="16" />
          </button>
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold relative" @click.stop="showFilters = !showFilters">
            <Filter :size="18" /> Lọc lớp
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
            <span class="text-[10px] font-semibold text-label uppercase tracking-widest min-w-[70px]">Lớp HP:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="s in ['all', ...sections]" :key="s"
                @click="filterSection = s"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterSection === s ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ s === 'all' ? 'Tất cả' : s }}</button>
            </div>
          </div>
          <div v-if="activeFilterCount > 0" class="pt-2 border-t border-default flex justify-end">
            <button class="text-[11px] font-bold text-placeholder hover:text-label transition-colors flex items-center gap-1" @click="clearAllFilters">
              <X :size="13" /> Xóa tất cả bộ lọc
            </button>
          </div>
        </div>
      </Transition>

      <div class="lg-table-shell overflow-hidden rounded-2xl">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">STT</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Sinh viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Lớp & Môn</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian vào</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="item in filteredWaitlist" :key="item.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <span class="text-sm font-semibold text-placeholder">#{{ item.position }}</span>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading">{{ item.student }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-0.5">{{ item.studentCode }}</p>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading leading-tight">{{ item.subject }}</p>
                <div class="flex items-center gap-2 mt-1">
                  <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ item.section }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span class="text-xs font-medium text-label">{{ item.time }}</span>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border', getStatusBadge(item.status)]">
                  {{ getStatusLabel(item.status) }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button v-if="item.status === 'waiting'" class="p-2 hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] rounded-lg text-placeholder transition-all" title="Xác nhận vào lớp" @click.stop="openConfirmModal(item)">
                    <CheckCircle2 :size="16" />
                  </button>
                  <button v-if="item.status === 'waiting'" class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all" title="Chuyển lớp khác" @click.stop="openTransferModal(item)">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button v-if="item.status === 'waiting'" class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] rounded-lg text-placeholder transition-all" title="Loại bỏ" @click.stop="openRemoveModal(item)">
                    <XCircle :size="16" />
                  </button>
                  <div class="relative">
                    <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder transition-all" @click.stop="toggleContextMenu(item)">
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
                      <div v-if="contextTarget?.id === item.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-sm" @click.stop>
                        <button v-if="item.status === 'waiting'" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] transition-all" @click="openConfirmModal(item); closeContextMenu()">
                          <UserCheck :size="14" /> Xác nhận vào lớp
                        </button>
                        <button v-if="item.status === 'waiting'" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="openTransferModal(item); closeContextMenu()">
                          <ArrowLeftRight :size="14" /> Chuyển lớp khác
                        </button>
                        <button v-if="item.status === 'waiting'" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all" @click="openRemoveModal(item); closeContextMenu()">
                          <UserX :size="14" /> Loại khỏi hàng chờ
                        </button>
                      </div>
                    </Transition>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredWaitlist.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <BookOpen :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-semibold text-heading">Không có sinh viên nào trong hàng chờ</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</p>
        </div>
      </div>

      <div class="lg-card-glass p-4 border border-[var(--color-warning-bg)]/30">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--lg-warning)] shrink-0">
             <Bell :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-semibold text-heading">Quy trình tự động</h4>
            <p class="text-xs text-label mt-2 leading-relaxed">
              Hệ thống xử lý hàng chờ theo nguyên tắc <strong>FIFO (First In First Out)</strong>. Khi một sinh viên hủy môn hoặc Giáo vụ tăng sức chứa lớp, hệ thống sẽ tự động gửi thông báo cho sinh viên đầu hàng chờ. Sinh viên có <strong>24 giờ</strong> để xác nhận đăng ký trước khi lượt bị chuyển cho người tiếp theo.
            </p>
          </div>
        </div>
      </div>

    </div>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showConfirmModal && confirmTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeConfirmModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div v-if="confirmDone" class="flex flex-col items-center py-8 text-center">
            <div class="h-16 w-16 rounded-full bg-[var(--color-success-bg)] text-[var(--lg-success)] flex items-center justify-center mb-4">
              <Check :size="32" />
            </div>
            <h3 class="text-base font-semibold text-heading">Xác nhận thành công!</h3>
            <p class="text-[12px] font-medium text-label mt-1">{{ confirmTarget.student }} đã được thêm vào lớp {{ confirmTarget.section }}</p>
          </div>
          <template v-else>
            <div class="flex items-center justify-between mb-5">
              <div class="flex items-center gap-2">
                <div class="h-8 w-8 rounded-lg bg-[var(--color-success-bg)] text-[var(--lg-success)] flex items-center justify-center">
                  <UserCheck :size="18" />
                </div>
                <h3 class="text-base font-semibold text-heading">Xác nhận vào lớp</h3>
              </div>
              <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeConfirmModal">
                <X :size="18" />
              </button>
            </div>
            <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-4">
              <div class="h-10 w-10 rounded-full bg-[var(--color-success-bg)] text-[var(--color-success-text)] text-xs font-semibold flex items-center justify-center flex-shrink-0 border border-default">
                {{ confirmTarget.studentCode.slice(-2) }}
              </div>
              <div>
                <p class="text-sm font-semibold text-heading">{{ confirmTarget.student }}</p>
                <div class="flex items-center gap-2 mt-0.5">
                  <span class="text-[11px] font-bold text-placeholder">{{ confirmTarget.studentCode }}</span>
                  <span class="w-1 h-1 rounded-full bg-placeholder" />
                  <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ confirmTarget.section }}</span>
                </div>
              </div>
            </div>
            <div class="bg-[var(--color-success-bg)] border border-[var(--lg-success)]/30 rounded-2xl p-3 mb-4">
              <p class="text-[11px] font-bold text-[var(--lg-success)]">Xác nhận sinh viên <strong>{{ confirmTarget.student }}</strong> vào lớp <strong>{{ confirmTarget.subject }}</strong> từ hàng chờ.</p>
            </div>
            <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
              <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeConfirmModal">Hủy</button>
              <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2 bg-[var(--lg-success)] hover:opacity-90" @click="doConfirm" :disabled="confirming">
                <Loader2 v-if="confirming" :size="16" class="animate-spin" />
                <CheckCircle2 v-else :size="16" />
                {{ confirming ? 'Đang xử lý...' : 'Xác nhận' }}
              </button>
            </div>
          </template>
        </div>
      </div>
    </Transition>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showRemoveModal && removeTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeRemoveModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-danger-bg)] text-[var(--lg-danger)] flex items-center justify-center">
                <UserX :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Loại khỏi hàng chờ</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeRemoveModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-4">
            <div class="h-10 w-10 rounded-full bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] text-xs font-semibold flex items-center justify-center flex-shrink-0 border border-default">
              {{ removeTarget.studentCode.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-semibold text-heading">{{ removeTarget.student }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[11px] font-bold text-placeholder">{{ removeTarget.studentCode }}</span>
                <span class="w-1 h-1 rounded-full bg-placeholder" />
                <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ removeTarget.section }}</span>
              </div>
            </div>
          </div>
          <div class="bg-[var(--color-danger-bg)] border border-[var(--lg-danger)]/30 rounded-2xl p-3 mb-4">
            <p class="text-[11px] font-bold text-[var(--lg-danger)]">Sinh viên <strong>{{ removeTarget.student }}</strong> sẽ bị loại khỏi hàng chờ của lớp <strong>{{ removeTarget.subject }}</strong>.</p>
          </div>
          <div>
            <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Lý do loại (không bắt buộc)</label>
            <textarea v-model="removeReason" rows="2" placeholder="Nhập lý do loại khỏi hàng chờ..." class="w-full lg-input px-4 py-2.5 text-sm resize-none"></textarea>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeRemoveModal">Quay lại</button>
            <button class="lg-btn-danger px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="doRemove">
              <Trash2 :size="16" /> Xác nhận loại
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showTransferModal && transferTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeTransferModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <ArrowLeftRight :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Chuyển lớp hàng chờ</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeTransferModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-5">
            <div class="h-10 w-10 rounded-full bg-[var(--color-info-bg)] text-[var(--color-info-text)] text-xs font-semibold flex items-center justify-center flex-shrink-0 border border-default">
              {{ transferTarget.studentCode.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-semibold text-heading">{{ transferTarget.student }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[11px] font-bold text-placeholder">{{ transferTarget.studentCode }}</span>
                <span class="w-1 h-1 rounded-full bg-placeholder" />
                <span class="text-[10px] font-semibold text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ transferTarget.section }}</span>
              </div>
            </div>
          </div>
          <div>
            <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Chuyển đến lớp</label>
            <select v-model="transferSection" class="w-full lg-input px-4 py-2.5 text-sm appearance-none cursor-pointer">
              <option value="" disabled>Chọn lớp mới...</option>
              <option v-for="sec in sections.filter(s => s !== transferTarget.section)" :key="sec" :value="sec">{{ sec }} - {{ subjectMap[sec] }}</option>
            </select>
            <div v-if="transferSection" class="mt-2 flex items-center gap-2 text-[11px] font-medium text-label">
              <Layers :size="14" class="text-placeholder" />
              {{ subjectMap[transferSection] || transferSection }}
            </div>
          </div>
          <div v-if="transferError" class="mt-3 flex items-center gap-2 bg-[var(--color-danger-bg)] border border-[var(--lg-danger)]/30 rounded-2xl p-3">
            <AlertCircle :size="16" class="text-[var(--lg-danger)] flex-shrink-0" />
            <p class="text-[11px] font-bold text-[var(--lg-danger)]">{{ transferError }}</p>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeTransferModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" :disabled="!transferSection" :class="{ 'opacity-50 cursor-not-allowed': !transferSection }" @click="confirmTransfer">
              <ArrowLeftRight :size="16" /> Chuyển
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
