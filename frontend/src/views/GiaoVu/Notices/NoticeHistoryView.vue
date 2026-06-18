<script setup>
import { ref, computed } from 'vue'
import { usePopupStore } from '@/stores/popup'
import {
  Search,
  Filter,
  Eye,
  RotateCcw,
  Mail,
  Smartphone,
  Bell,
  Users,
  CheckCircle2,
  Clock,
  MoreVertical,
  ChevronRight,
  Trash2,
  Copy,
  X
} from 'lucide-vue-next'
const popupStore = usePopupStore()

// ── Mock Data ────────────────────────────────────────────────
const allNotices = ref([
  { id: 'NT-001', title: 'Thay đổi lịch học môn Java Lab 2', author: 'Phạm Minh D', target: 'Lớp L01, L02', channels: ['in-app', 'email'], date: '12/05 08:30', status: 'sent', recipients: 124 },
  { id: 'NT-002', title: 'Thông báo kết quả học bổng kỳ Fall', author: 'Nguyễn Bích L', target: 'Sinh viên đạt học bổng', channels: ['email', 'push'], date: '10/05 15:45', status: 'sent', recipients: 45 },
  { id: 'NT-003', title: 'Cảnh báo quá hạn đăng ký môn học', author: 'Hệ thống', target: 'SV chưa đăng ký', channels: ['push'], date: '08/05 09:00', status: 'failed', recipients: 312 },
  { id: 'NT-004', title: 'Lịch thi học kỳ Spring 2026', author: 'Trần Văn K', target: 'Toàn bộ SV Campus HCM', channels: ['in-app', 'email', 'push'], date: '15/05 08:00', status: 'scheduled', recipients: 4500 },
  { id: 'NT-005', title: 'Gia hạn đăng ký môn học kỳ hè', author: 'Nguyễn Bích L', target: 'SV đủ điều kiện', channels: ['in-app', 'email'], date: '14/05 14:20', status: 'sent', recipients: 230 },
  { id: 'NT-006', title: 'Nhắc nhở đánh giá giảng viên', author: 'Hệ thống', target: 'Toàn bộ SV', channels: ['in-app', 'push'], date: '11/05 10:00', status: 'failed', recipients: 5200 },
  { id: 'NT-007', title: 'Lễ tốt nghiệp đợt tháng 6', author: 'Trần Văn K', target: 'SV tốt nghiệp', channels: ['email'], date: '20/05 07:30', status: 'scheduled', recipients: 180 },
  { id: 'NT-008', title: 'Thông báo nghỉ lễ 30/4 - 1/5', author: 'Phạm Minh D', target: 'Toàn bộ CB-GV-SV', channels: ['in-app', 'email', 'push'], date: '25/04 16:00', status: 'sent', recipients: 8500 },
])

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const channelFilter = ref(null)

const channelOptions = [
  { value: null, label: 'Tất cả kênh' },
  { value: 'in-app', label: 'In-app' },
  { value: 'email', label: 'Email' },
  { value: 'push', label: 'Push' },
]

const showChannelDropdown = ref(false)

function selectChannel(ch) {
  channelFilter.value = ch
  showChannelDropdown.value = false
  currentPage.value = 1
}

function clearChannelFilter() {
  channelFilter.value = null
  showChannelDropdown.value = false
  currentPage.value = 1
}

const hasActiveFilter = computed(() => channelFilter.value !== null || searchQuery.value.trim() !== '')

const filteredNotices = computed(() => {
  let result = allNotices.value

  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    result = result.filter(
      nt => nt.title.toLowerCase().includes(q) || nt.author.toLowerCase().includes(q),
    )
  }

  if (channelFilter.value) {
    result = result.filter(nt => nt.channels.includes(channelFilter.value))
  }

  return result
})

function clearSearch() {
  searchQuery.value = ''
  currentPage.value = 1
}

// ── Context Menu ─────────────────────────────────────────────
const contextTarget = ref(null)

function toggleContextMenu(nt) {
  contextTarget.value = contextTarget.value?.id === nt.id ? null : nt
}

function closeContextMenu() {
  contextTarget.value = null
}

// ── Pagination ───────────────────────────────────────────────
const currentPage = ref(1)
const perPage = ref(5)

const totalPages = computed(() => Math.ceil(filteredNotices.value.length / perPage.value))

const pagedNotices = computed(() => {
  const start = (currentPage.value - 1) * perPage.value
  return filteredNotices.value.slice(start, start + perPage.value)
})

function goToPage(page) {
  if (page < 1 || page > totalPages.value) return
  currentPage.value = page
}

// ── Actions ──────────────────────────────────────────────────
function retryNotice(nt) {
  const idx = allNotices.value.findIndex(n => n.id === nt.id)
  if (idx !== -1) {
    allNotices.value[idx] = { ...allNotices.value[idx], status: 'sent' }
  }
  closeContextMenu()
  popupStore.success('Đã thử lại', `Thông báo ${nt.id} đang được gửi lại.`)
}

function viewDetail(nt) {
  closeContextMenu()
  popupStore.info('Chi tiết thông báo', `${nt.title}\nNgười gửi: ${nt.author}\nĐối tượng: ${nt.target}\nKênh: ${nt.channels.join(', ')}\nTrạng thái: ${nt.status}`)
}

function copyNotice(nt) {
  const newNotice = {
    ...nt,
    id: `NT-${String(allNotices.value.length + 1).padStart(3, '0')}`,
    title: `${nt.title} (bản sao)`,
    date: new Date().toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' }),
    status: 'draft',
  }
  allNotices.value.unshift(newNotice)
  closeContextMenu()
  popupStore.success('Đã sao chép', `Thông báo ${newNotice.id} đã được tạo từ bản sao.`)
}

function deleteNotice(nt) {
  const idx = allNotices.value.findIndex(n => n.id === nt.id)
  if (idx !== -1) {
    allNotices.value.splice(idx, 1)
  }
  closeContextMenu()
  popupStore.warning('Đã xóa', `Thông báo ${nt.id} đã được xóa.`)
}

// ── Display helpers ──────────────────────────────────────────
const getStatusBadge = (status) => {
  switch (status) {
    case 'sent': return 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[var(--color-success-text)]/20'
    case 'failed': return 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20'
    case 'scheduled': return 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20'
    case 'draft': return 'bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] border-[var(--color-warning-text)]/20'
    default: return 'surface-solid text-muted border-default'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'sent': return 'sent'
    case 'failed': return 'failed'
    case 'scheduled': return 'scheduled'
    case 'draft': return 'draft'
    default: return status
  }
}

const getChannelIcon = (ch) => {
  switch (ch) {
    case 'in-app': return Bell
    case 'email': return Mail
    case 'push': return Smartphone
    default: return Bell
  }
}
</script>

<template>
  <PageContainer 
    title="Lịch sử thông báo" 
    subtitle="Xem lại danh sách và trạng thái gửi của các thông báo học vụ đã phát hành."
  >
    <div class="space-y-4" @click="closeContextMenu">
      
      <!-- ── Filters ── -->
      <div class="surface-card border border-card p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative flex items-center gap-2">
          <div class="relative flex-1">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
            <input 
              v-model="searchQuery"
              type="text" 
              placeholder="Tìm theo tiêu đề hoặc người gửi..." 
              class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
            >
            <button 
              v-if="searchQuery.trim()" 
              @click="clearSearch"
              class="absolute right-3 top-1/2 -translate-y-1/2 p-0.5 rounded-full hover:surface-solid transition-all"
            >
              <X :size="14" class="text-placeholder" />
            </button>
          </div>
        </div>
        <div class="relative">
          <button 
            class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2"
            @click.stop="showChannelDropdown = !showChannelDropdown; closeContextMenu()"
          >
            <Filter :size="18" /> Lọc theo kênh
            <span v-if="channelFilter" class="h-4 w-4 rounded-full bg-[var(--lg-primary)] text-white text-[9px] font-semibold flex items-center justify-center">1</span>
          </button>
          <Transition
            enter-active-class="transition-all duration-150 ease-out"
            enter-from-class="opacity-0 scale-95 -translate-y-2"
            enter-to-class="opacity-100 scale-100 translate-y-0"
            leave-active-class="transition-all duration-100 ease-in"
            leave-from-class="opacity-100 scale-100 translate-y-0"
            leave-to-class="opacity-0 scale-95 -translate-y-2"
          >
            <div 
              v-if="showChannelDropdown" 
              class="absolute right-0 top-full mt-2 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-sm"
              @click.stop
            >
              <button 
                v-for="ch in channelOptions" 
                :key="ch.value"
                @click="selectChannel(ch.value)"
                :class="['w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold transition-all', channelFilter === ch.value ? 'bg-[var(--color-info-bg)] text-link' : 'text-label hover:bg-[var(--surface-input)]']"
              >
                <CheckCircle2 v-if="channelFilter === ch.value" :size="14" class="shrink-0" />
                <div v-else :class="['h-3.5 w-3.5 rounded-full border-2', channelFilter === ch.value ? 'border-link' : 'border-default']"></div>
                {{ ch.label }}
              </button>
              <div v-if="channelFilter" class="border-t border-default mt-1 pt-1">
                <button @click="clearChannelFilter" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all">
                  <X :size="14" /> Bỏ lọc
                </button>
              </div>
            </div>
          </Transition>
        </div>
      </div>

      <!-- ── Active Filters Bar ── -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-2"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-2"
      >
        <div v-if="hasActiveFilter" class="flex items-center gap-2 flex-wrap">
          <span class="text-[10px] font-semibold text-label uppercase tracking-widest">Bộ lọc đang áp dụng:</span>
          <button v-if="searchQuery.trim()" @click="clearSearch" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-[var(--color-info-bg)] text-link text-[11px] font-semibold border border-[var(--color-info-text)]/20 hover:bg-[var(--color-info-bg)]/80 transition-all">
            <Search :size="12" /> "{{ searchQuery.trim() }}" <X :size="12" />
          </button>
          <button v-if="channelFilter" @click="clearChannelFilter" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-[var(--color-info-bg)] text-link text-[11px] font-semibold border border-[var(--color-info-text)]/20 hover:bg-[var(--color-info-bg)]/80 transition-all">
            <Filter :size="12" /> Kênh: {{ channelOptions.find(c => c.value === channelFilter)?.label }} <X :size="12" />
          </button>
          <span class="text-[11px] font-medium text-placeholder">{{ filteredNotices.length }} kết quả</span>
        </div>
      </Transition>

      <!-- ── History Grid ── -->
      <div class="space-y-3">
        <div 
          v-for="nt in pagedNotices" 
          :key="nt.id"
          class="surface-card border border-card rounded-2xl p-4 group hover:border-[var(--border-input-focus)] transition-all"
        >
          <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div class="min-w-0 flex-1">
              <div class="flex items-center gap-2 mb-2">
                <span class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">{{ nt.id }}</span>
                <div :class="['px-2.5 py-1 rounded-lg text-[10px] font-semibold uppercase tracking-widest border shadow-sm', getStatusBadge(nt.status)]">
                  {{ getStatusLabel(nt.status) }}
                </div>
              </div>

              <h3 class="text-base font-semibold text-heading leading-snug group-hover:text-link transition-colors">
                {{ nt.title }}
              </h3>

              <div class="mt-3 flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-x-4">
                <div class="flex items-center gap-2 text-xs font-bold text-label">
                  <Users :size="14" /> {{ nt.target }}
                </div>
                <div class="flex items-center gap-2 text-xs font-bold text-label">
                  <Clock :size="14" /> {{ nt.date }} • {{ nt.author }}
                </div>
              </div>
            </div>

            <div class="flex items-center justify-between gap-4 lg:min-w-[320px]">
              <div class="flex items-center gap-2">
                <div 
                  v-for="ch in nt.channels" 
                  :key="ch"
                  class="h-7 w-7 rounded-lg surface-solid flex items-center justify-center text-placeholder border-default"
                  :title="ch"
                >
                  <component :is="getChannelIcon(ch)" :size="14" />
                </div>
                <span class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">{{ nt.recipients }} recipients</span>
              </div>
              
              <div class="flex items-center gap-2">
                <button 
                  v-if="nt.status === 'failed'" 
                  @click.stop="retryNotice(nt)"
                  class="p-2 lg-button-ghost text-[var(--color-danger-text)] rounded-lg transition-colors" 
                  title="Thử lại"
                >
                  <RotateCcw :size="16" />
                </button>
                <button 
                  @click.stop="viewDetail(nt)"
                  class="p-2 lg-button-ghost text-link rounded-lg transition-colors flex items-center gap-1 text-xs font-semibold uppercase tracking-widest"
                >
                  Detail <ChevronRight :size="14" />
                </button>
                <div class="relative">
                  <button 
                    @click.stop="toggleContextMenu(nt)"
                    class="p-2 lg-button-ghost text-placeholder hover:text-heading rounded-lg transition-colors"
                  >
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
                    <div 
                      v-if="contextTarget?.id === nt.id" 
                      class="absolute right-0 top-full mt-1 z-50 w-44 lg-glass-strong rounded-xl p-1 shadow-sm"
                      @click.stop
                    >
                      <button 
                        @click="viewDetail(nt)"
                        class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all"
                      >
                        <Eye :size="14" /> Xem chi tiết
                      </button>
                      <button 
                        @click="copyNotice(nt)"
                        class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all"
                      >
                        <Copy :size="14" /> Sao chép
                      </button>
                      <button 
                        v-if="nt.status === 'failed'"
                        @click="retryNotice(nt)"
                        class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] transition-all"
                      >
                        <RotateCcw :size="14" /> Gửi lại
                      </button>
                      <div class="border-t border-default my-1"></div>
                      <button 
                        @click="deleteNotice(nt)"
                        class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all"
                      >
                        <Trash2 :size="14" /> Xóa
                      </button>
                    </div>
                  </Transition>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── Pagination ── -->
      <div v-if="totalPages > 1" class="flex items-center justify-between gap-4 pt-2">
        <span class="text-[11px] font-semibold text-label">
          Trang {{ currentPage }} / {{ totalPages }} ({{ filteredNotices.length }} thông báo)
        </span>
        <div class="flex items-center gap-1.5">
          <button 
            @click="goToPage(currentPage - 1)" 
            :disabled="currentPage <= 1"
            class="px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all disabled:opacity-30 disabled:cursor-default surface-solid text-label hover:bg-[var(--surface-input)]"
          >
            Trước
          </button>
          <button 
            v-for="p in totalPages" 
            :key="p"
            @click="goToPage(p)"
            :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', currentPage === p ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
          >
            {{ p }}
          </button>
          <button 
            @click="goToPage(currentPage + 1)" 
            :disabled="currentPage >= totalPages"
            class="px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all disabled:opacity-30 disabled:cursor-default surface-solid text-label hover:bg-[var(--surface-input)]"
          >
            Sau
          </button>
        </div>
      </div>

      <!-- ── Empty State ── -->
      <div v-if="filteredNotices.length === 0" class="py-24 text-center">
        <div class="h-20 w-20 surface-solid rounded-3xl flex items-center justify-center text-placeholder mx-auto mb-4">
          <Bell :size="40" />
        </div>
        <h3 class="text-xl font-semibold text-heading tracking-tight">Chưa có thông báo nào</h3>
        <p class="text-sm text-label mt-2 max-w-xs mx-auto">
          {{ hasActiveFilter ? 'Không tìm thấy thông báo phù hợp. Hãy thử thay đổi từ khóa hoặc bộ lọc.' : 'Hãy bắt đầu bằng việc tạo một thông báo học vụ mới cho sinh viên hoặc giảng viên.' }}
        </p>
        <button v-if="hasActiveFilter" @click="clearSearch; clearChannelFilter();" class="mt-6 lg-button-secondary px-5 py-2.5 text-sm font-bold">
          <X :size="16" /> Xóa tất cả bộ lọc
        </button>
      </div>

    </div>
  </PageContainer>
</template>
