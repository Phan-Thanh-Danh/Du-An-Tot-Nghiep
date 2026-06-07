<script setup>
import { ref, computed } from 'vue'
import { 
  Search, 
  Filter, 
  FileText, 
  Clock, 
  User, 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  UserPlus, 
  ArrowRight,
  MoreVertical,
  Flag,
  Timer,
  X,
  Eye
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const requests = ref([
  { id: 'DON-001', student: 'Nguyễn Văn A', type: 'Chuyển lớp', title: 'Xin chuyển từ L01 sang L02', date: '12/05/2026', reviewer: 'Phạm Minh D', sla: '2h 15m', status: 'under_review', priority: 'high' },
  { id: 'DON-002', student: 'Lê Thị B', type: 'Nghỉ học tạm thời', title: 'Xin bảo lưu kết quả học tập', date: '11/05/2026', reviewer: 'Chưa phân công', sla: '12h 40m', status: 'submitted', priority: 'medium' },
  { id: 'DON-003', student: 'Trần Văn C', type: 'Cấp giấy xác nhận', title: 'Xin giấy xác nhận SV làm thẻ ngân hàng', date: '13/05/2026', reviewer: 'Nguyễn Bích L', sla: '45m', status: 'under_review', priority: 'low' },
  { id: 'DON-004', student: 'Hoàng Thị D', type: 'Thi lại', title: 'Đơn xin thi lại môn Java', date: '10/05/2026', reviewer: 'Trần Văn K', sla: 'QUÁ HẠN', status: 'under_review', priority: 'high' },
])

const getStatusBadge = (status) => {
  switch (status) {
    case 'submitted': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20'
    case 'under_review': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[var(--color-warning-text)]/20'
    default: return 'surface-solid text-muted border-default'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'submitted': return 'Chờ xử lý'
    case 'under_review': return 'Đang xử lý'
    default: return status
  }
}

const getPriorityColor = (priority) => {
  switch (priority) {
    case 'high': return 'text-[var(--color-danger-text)]'
    case 'medium': return 'text-[var(--color-warning-text)]'
    case 'low': return 'text-[var(--color-success-text)]'
    default: return 'text-muted'
  }
}

// ── Search ───────────────────────────────────────────────────
const searchQuery = ref('')
const searchTriggered = ref('')
const showFilters = ref(false)

const filterStatus = ref('all')
const filterPriority = ref('all')

const activeFilterCount = computed(() => {
  let count = 0
  if (filterStatus.value !== 'all') count++
  if (filterPriority.value !== 'all') count++
  return count
})

function doSearch() {
  searchTriggered.value = searchQuery.value
}

function clearSearch() {
  searchQuery.value = ''
  searchTriggered.value = ''
}

const filteredRequests = computed(() => {
  let result = requests.value
  const q = searchTriggered.value.toLowerCase().trim()
  if (q) {
    result = result.filter(r =>
      r.id.toLowerCase().includes(q) ||
      r.student.toLowerCase().includes(q) ||
      r.title.toLowerCase().includes(q) ||
      r.type.toLowerCase().includes(q)
    )
  }
  if (filterStatus.value !== 'all') {
    result = result.filter(r => r.status === filterStatus.value)
  }
  if (filterPriority.value !== 'all') {
    result = result.filter(r => r.priority === filterPriority.value)
  }
  return result
})

function clearAllFilters() {
  filterStatus.value = 'all'
  filterPriority.value = 'all'
}

// ── Context Menu ─────────────────────────────────────────────
const contextTarget = ref(null)

function toggleContextMenu(req) {
  contextTarget.value = contextTarget.value?.id === req.id ? null : req
}

function closeContextMenu() {
  contextTarget.value = null
}
</script>

<template>
  <PageContainer 
    title="Đơn từ cần xử lý" 
    subtitle="Quản lý và phê duyệt các yêu cầu hành chính học vụ từ sinh viên."
  >
    <div class="space-y-4" @click="closeContextMenu">
      
      <!-- ── Toolbar ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative flex items-center gap-2">
          <div class="relative flex-1">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
            <input 
              v-model="searchQuery"
              type="text" 
              placeholder="Tìm theo mã đơn, sinh viên hoặc nội dung..." 
              class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
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
            <Filter :size="18" /> Lọc nâng cao
            <span v-if="activeFilterCount > 0" class="absolute -top-1.5 -right-1.5 h-4 w-4 rounded-full bg-[var(--lg-primary)] text-white text-[9px] font-semibold flex items-center justify-center">{{ activeFilterCount }}</span>
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
        <div v-if="showFilters" class="surface-card border border-card p-4 rounded-2xl space-y-3">
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-semibold text-label uppercase tracking-widest min-w-[70px]">Trạng thái:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="s in ['all','submitted','under_review']" :key="s"
                @click="filterStatus = s"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterStatus === s ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ { all: 'Tất cả', submitted: 'Chờ xử lý', under_review: 'Đang xử lý' }[s] }}</button>
            </div>
          </div>
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-semibold text-label uppercase tracking-widest min-w-[70px]">Ưu tiên:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="p in ['all','high','medium','low']" :key="p"
                @click="filterPriority = p"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterPriority === p ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ { all: 'Tất cả', high: 'Cao', medium: 'Trung bình', low: 'Thấp' }[p] }}</button>
            </div>
          </div>
          <div v-if="activeFilterCount > 0" class="pt-2 border-t border-default flex justify-end">
            <button class="text-[11px] font-bold text-placeholder hover:text-label transition-colors flex items-center gap-1" @click="clearAllFilters">
              <X :size="13" /> Xóa tất cả bộ lọc
            </button>
          </div>
        </div>
      </Transition>

      <!-- ── Requests Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default w-10">#</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default">Sinh viên & Loại đơn</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default">Người xử lý</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default">SLA còn lại</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="req in filteredRequests" :key="req.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                 <Flag :size="16" :class="getPriorityColor(req.priority)" />
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl surface-solid flex items-center justify-center text-placeholder shrink-0">
                    <FileText :size="18" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ req.type }}</p>
                    <p class="text-[11px] font-bold text-label mt-0.5">{{ req.student }} • {{ req.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <User :size="14" class="text-placeholder" />
                    <span :class="['text-xs font-bold', req.reviewer === 'Chưa phân công' ? 'text-[var(--color-warning-text)]' : 'text-label']">
                    {{ req.reviewer }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <Timer :size="14" :class="req.sla === 'QUÁ HẠN' ? 'text-[var(--color-danger-text)]' : 'text-placeholder'" />
                  <span :class="['text-xs font-semibold uppercase tracking-tighter', req.sla === 'QUÁ HẠN' ? 'text-[var(--color-danger-text)]' : 'text-label']">
                    {{ req.sla }}
                  </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border', getStatusBadge(req.status)]">
                  {{ getStatusLabel(req.status) }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <router-link :to="`/staff/requests/${req.id}`" class="p-2 lg-button-ghost rounded-lg" title="Xem chi tiết">
                    <ArrowRight :size="18" />
                  </router-link>
                  <div class="relative">
                    <button class="p-2 lg-button-ghost rounded-lg" @click.stop="toggleContextMenu(req)">
                      <MoreVertical :size="18" />
                    </button>
                    <Transition
                      enter-active-class="transition-all duration-150 ease-out"
                      enter-from-class="opacity-0 scale-95"
                      enter-to-class="opacity-100 scale-100"
                      leave-active-class="transition-all duration-100 ease-in"
                      leave-from-class="opacity-100 scale-100"
                      leave-to-class="opacity-0 scale-95"
                    >
                      <div v-if="contextTarget?.id === req.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-sm" @click.stop>
                        <router-link :to="`/staff/requests/${req.id}`" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="closeContextMenu()">
                          <Eye :size="14" /> Xem chi tiết
                        </router-link>
                      </div>
                    </Transition>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredRequests.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <FileText :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-semibold text-heading">Không có đơn từ nào</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm hoặc bộ lọc</p>
        </div>
      </div>

      <!-- ── SLA Legend ── -->
      <div class="surface-card border border-card p-5 rounded-2xl">
        <div class="flex items-start gap-4">
          <div class="h-10 w-10 rounded-2xl bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)] shrink-0 shadow-sm border border-[var(--color-danger-text)]/20">
             <AlertCircle :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Quy tắc xử lý đơn (SLA)</h4>
            <p class="text-xs text-body mt-1.5 leading-relaxed">
              Tất cả các đơn từ đều có thời hạn xử lý (SLA) quy định theo từng loại. Các đơn <strong>QUÁ HẠN</strong> sẽ được hệ thống tự động đẩy lên mức ưu tiên cao nhất và thông báo cho Trưởng phòng Giáo vụ.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>
