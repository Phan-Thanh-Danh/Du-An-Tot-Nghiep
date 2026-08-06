<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  ClipboardCheck, Eye, Search, Loader2, AlertCircle, FileText, ChevronLeft, ChevronRight
} from 'lucide-vue-next'
import { applicationsApi } from '@/services/applicationsApi'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const loading = ref(true)
const apiError = ref('')
const items = ref([])
const totalItems = ref(0)
const pageIndex = ref(1)
const pageSize = ref(20)
const totalPages = ref(0)
const searchQuery = ref('')
const statusFilter = ref('')
const slaFilter = ref('')
let searchTimer = null

const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'da_nop', label: 'Chờ tiếp nhận' },
  { value: 'dang_xem_xet', label: 'Đang xem xét' },
  { value: 'yeu_cau_bo_sung', label: 'Yêu cầu bổ sung' },
  { value: 'da_duyet', label: 'Đã duyệt' },
  { value: 'tu_choi', label: 'Từ chối' },
  { value: 'da_huy', label: 'Đã hủy' },
  { value: 'nhap', label: 'Nháp' },
]

const SLA_OPTIONS = [
  { value: '', label: 'Mọi hạn xử lý' },
  { value: 'on_track', label: 'Đúng hạn' },
  { value: 'due_soon', label: 'Sắp hết hạn' },
  { value: 'overdue', label: 'Quá hạn' },
]

const getStatusMeta = (status) => {
  const map = {
    da_nop: { label: 'Chờ tiếp nhận', variant: 'warning' },
    dang_xem_xet: { label: 'Đang xem xét', variant: 'info' },
    yeu_cau_bo_sung: { label: 'Yêu cầu bổ sung', variant: 'warning' },
    da_duyet: { label: 'Đã duyệt', variant: 'success' },
    tu_choi: { label: 'Từ chối', variant: 'danger' },
    da_huy: { label: 'Đã hủy', variant: 'neutral' },
    nhap: { label: 'Nháp', variant: 'neutral' },
  }
  return map[status] || { label: status, variant: 'neutral' }
}

const getSlaMeta = (sla) => {
  const status = sla?.status ?? 'none'
  const map = {
    on_track: { label: 'Đúng hạn', variant: 'success' },
    due_soon: { label: 'Sắp hết hạn', variant: 'warning' },
    overdue: { label: 'Quá hạn', variant: 'danger' },
    paused: { label: 'Tạm dừng', variant: 'neutral' },
    none: { label: 'Không giới hạn', variant: 'neutral' },
  }
  const meta = map[status] || { label: status, variant: 'neutral' }
  if (status === 'due_soon' && sla?.remainingMinutes != null) {
    meta.label += ` (${formatMinutes(sla.remainingMinutes)})`
  }
  return meta
}

function formatMinutes(totalMinutes) {
  if (totalMinutes < 60) return `${totalMinutes}p`
  const h = Math.floor(totalMinutes / 60)
  const m = totalMinutes % 60
  return m > 0 ? `${h}h${m}m` : `${h}h`
}

function formatDate(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const pad = (n) => String(n).padStart(2, '0')
  return `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`
}

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await applicationsApi.getAdminApplications({
      search: searchQuery.value || undefined,
      status: statusFilter.value || undefined,
      slaStatus: slaFilter.value || undefined,
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
    })
    items.value = res?.items ?? []
    totalItems.value = res?.totalItems ?? 0
    totalPages.value = res?.totalPages ?? 0
  } catch (e) {
    console.error(e)
    apiError.value = e?.message || 'Không thể tải danh sách đơn.'
    items.value = []
  } finally {
    loading.value = false
  }
}

function onSearchInput() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    pageIndex.value = 1
    loadData()
  }, 400)
}

function onFilterChange() {
  pageIndex.value = 1
  loadData()
}

function goToPage(index) {
  if (index < 1 || index > totalPages.value || index === pageIndex.value) return
  pageIndex.value = index
  loadData()
}

const pagingRange = computed(() => {
  const pages = []
  const total = totalPages.value || 1
  const current = pageIndex.value
  const start = Math.max(1, Math.min(current - 2, total - 4))
  const end = Math.min(total, start + 4)
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})

onMounted(() => { loadData() })
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-7xl mx-auto w-full">
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <ClipboardCheck class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Đơn cần xử lý</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Tiếp nhận và xử lý các đơn từ sinh viên.</p>
      </div>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="surface-card border border-(--border-card) rounded-2xl flex flex-col min-h-0 flex-1 shadow-sm overflow-hidden">
        <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 bg-(--surface-input)">
          <div class="relative flex-1 min-w-[200px] max-w-sm">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="15" />
            <input v-model="searchQuery" @input="onSearchInput" type="text" placeholder="Tra cứu mã đơn, tên SV..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
          <select v-model="statusFilter" @change="onFilterChange" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="opt in STATUS_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
          <select v-model="slaFilter" @change="onFilterChange" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="opt in SLA_OPTIONS" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>

        <div class="flex-1 overflow-x-auto">
          <table class="w-full text-left text-sm whitespace-nowrap">
            <thead class="bg-(--surface-input) border-b border-(--border-default) text-(--text-muted)">
              <tr>
                <th class="px-5 py-4 font-semibold w-24">Mã đơn</th>
                <th class="px-5 py-4 font-semibold">Sinh viên & Loại đơn</th>
                <th class="px-5 py-4 font-semibold">Trạng thái</th>
                <th class="px-5 py-4 font-semibold">Hạn xử lý</th>
                <th class="px-5 py-4 font-semibold">Ngày nộp</th>
                <th class="px-5 py-4 font-semibold text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-(--border-default)">
              <tr v-for="item in items" :key="item.maDonTu" class="hover:bg-(--surface-hover) transition-colors">
                <td class="px-5 py-4 font-mono text-xs font-bold text-(--text-muted)">ĐT-{{ String(item.maDonTu).padStart(4, '0') }}</td>
                <td class="px-5 py-4">
                  <p class="font-bold text-(--text-heading)">{{ item.hocSinh?.hoTen || '—' }}</p>
                  <p class="text-xs text-(--text-muted) mt-0.5">
                    <span class="text-(--lg-primary)">{{ item.tenLoaiDon || item.loaiDon }}</span>
                    <span v-if="item.tieuDe"> — {{ item.tieuDe }}</span>
                  </p>
                </td>
                <td class="px-5 py-4">
                  <div class="flex flex-col gap-1">
                    <GlassBadge :variant="getStatusMeta(item.trangThai).variant" size="sm">{{ getStatusMeta(item.trangThai).label }}</GlassBadge>
                    <span v-if="item.tenTrangThaiXuLyNghiepVu" class="text-[11px] text-(--text-muted)">{{ item.tenTrangThaiXuLyNghiepVu }}</span>
                  </div>
                </td>
                <td class="px-5 py-4">
                  <GlassBadge :variant="getSlaMeta(item.sla).variant" size="sm">{{ getSlaMeta(item.sla).label }}</GlassBadge>
                </td>
                <td class="px-5 py-4 text-(--text-muted)">{{ formatDate(item.ngayNop) }}</td>
                <td class="px-5 py-4 text-right">
                  <router-link :to="`/staff/requests/${item.maDonTu}`">
                    <GlassButton variant="secondary" size="xs">
                      <Eye :size="14" class="mr-1" /> Chi tiết
                    </GlassButton>
                  </router-link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="items.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <FileText :size="40" class="text-(--text-muted) mb-3" />
          <p class="text-base font-semibold text-(--text-heading)">Không có đơn nào</p>
          <p class="text-sm text-(--text-muted) mt-1">Không tìm thấy đơn phù hợp với bộ lọc hiện tại.</p>
        </div>

        <div class="border-t border-(--border-default) p-4 flex justify-between items-center bg-(--surface-input)">
          <p class="text-sm text-(--text-muted)">Hiển thị {{ items.length }} / {{ totalItems }} kết quả</p>
          <div class="flex items-center gap-1">
            <button @click="goToPage(pageIndex - 1)" :disabled="pageIndex <= 1" class="p-1.5 rounded-lg border border-(--border-input) text-(--text-muted) hover:bg-(--surface-hover) disabled:opacity-40 disabled:pointer-events-none">
              <ChevronLeft :size="16" />
            </button>
            <button
              v-for="page in pagingRange"
              :key="page"
              @click="goToPage(page)"
              class="px-3 py-1.5 rounded-lg text-sm font-semibold transition-colors"
              :class="page === pageIndex ? 'bg-(--lg-primary) text-white' : 'border border-(--border-input) text-(--text-muted) hover:bg-(--surface-hover)'"
            >
              {{ page }}
            </button>
            <button @click="goToPage(pageIndex + 1)" :disabled="pageIndex >= totalPages" class="p-1.5 rounded-lg border border-(--border-input) text-(--text-muted) hover:bg-(--surface-hover) disabled:opacity-40 disabled:pointer-events-none">
              <ChevronRight :size="16" />
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
