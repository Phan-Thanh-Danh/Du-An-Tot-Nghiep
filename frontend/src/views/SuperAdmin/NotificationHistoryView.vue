<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { AlertCircle, Bell, ChevronLeft, ChevronRight, Eye, Filter, Loader2, RefreshCw, Search, X } from 'lucide-vue-next'
import { notificationsApi } from '@/services/notificationsApi'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { formatDateTime } from '@/utils/dateFormat'

// ── State ────────────────────────────────────────────────────────────
const loading = ref(false)
const error = ref('')
const notifications = ref([])

// Pagination
const currentPage = ref(1)
const pageSize = ref(20)
const totalItems = ref(0)
const totalPages = computed(() =>
  pageSize.value <= 0 ? 1 : Math.max(1, Math.ceil(totalItems.value / pageSize.value))
)

// Filters
const keyword = ref('')
const filterLoai = ref('')
const filterTrangThai = ref('')
const filterMucDo = ref('')
const filterNgayTu = ref('')
const filterNgayDen = ref('')
const showFilters = ref(false)

// Detail modal
const selectedNotification = ref(null)
const detailLoading = ref(false)

// ── Constants (mirror backend NotificationConstants) ─────────────────
const LOAI_OPTIONS = [
  { value: '', label: 'Tất cả loại' },
  { value: 'thong_bao_chung', label: 'Thông báo chung' },
  { value: 'hoc_vu', label: 'Học vụ' },
  { value: 'hoc_phi', label: 'Học phí' },
  { value: 'bao_tri', label: 'Bảo trì' },
  { value: 'co_so_vat_chat', label: 'Cơ sở vật chất' },
  { value: 'khan_cap', label: 'Khẩn cấp' },
  { value: 'manual', label: 'Thủ công' },
  { value: 'schedule_changed', label: 'Đổi lịch' },
  { value: 'system', label: 'Hệ thống' },
]

const TRANG_THAI_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'da_gui', label: 'Đã gửi' },
  { value: 'nhap', label: 'Nháp' },
  { value: 'da_huy', label: 'Đã huỷ' },
]

const MUC_DO_OPTIONS = [
  { value: '', label: 'Tất cả mức độ' },
  { value: 'thong_tin', label: 'Thông tin' },
  { value: 'info', label: 'Info' },
  { value: 'quan_trong', label: 'Quan trọng' },
  { value: 'important', label: 'Important' },
  { value: 'warning', label: 'Warning' },
  { value: 'khan_cap', label: 'Khẩn cấp' },
]

// ── Helpers ──────────────────────────────────────────────────────────
function getMucDoBadgeVariant(mucDo) {
  if (!mucDo) return 'neutral'
  const m = mucDo.toLowerCase()
  if (m.includes('khan_cap') || m === 'urgent') return 'danger'
  if (m.includes('quan_trong') || m === 'important' || m === 'warning') return 'warning'
  if (m === 'thong_tin' || m === 'info') return 'info'
  return 'neutral'
}

function getTrangThaiBadgeVariant(trangThai) {
  if (!trangThai) return 'neutral'
  const t = trangThai.toLowerCase()
  if (t === 'da_gui') return 'success'
  if (t === 'da_huy') return 'danger'
  if (t === 'nhap') return 'warning'
  return 'neutral'
}

function getTrangThaiLabel(trangThai) {
  const found = TRANG_THAI_OPTIONS.find(o => o.value === trangThai)
  return found?.label || trangThai || '-'
}

function getMucDoLabel(mucDo) {
  const found = MUC_DO_OPTIONS.find(o => o.value === mucDo)
  return found?.label || mucDo || '-'
}

function getPhamViLabel(phamVi) {
  const map = {
    toan_he_thong: 'Toàn hệ thống',
    don_vi: 'Đơn vị',
    vai_tro: 'Vai trò',
    nguoi_dung: 'Cá nhân',
    lop_hanh_chinh: 'Lớp hành chính',
    khoa_hoc: 'Khóa học',
  }
  return map[phamVi] || phamVi || '-'
}

const hasActiveFilters = computed(() =>
  filterLoai.value || filterTrangThai.value || filterMucDo.value ||
  filterNgayTu.value || filterNgayDen.value
)

// ── Data Loading ─────────────────────────────────────────────────────
async function loadNotifications() {
  loading.value = true
  error.value = ''
  try {
    const params = {
      pageIndex: currentPage.value,   // FIX: was "pageNumber" which BE ignores
      pageSize: pageSize.value,
    }
    if (keyword.value.trim()) params.keyword = keyword.value.trim()
    if (filterLoai.value) params.loaiThongBao = filterLoai.value
    if (filterTrangThai.value) params.trangThai = filterTrangThai.value
    if (filterMucDo.value) params.mucDo = filterMucDo.value
    if (filterNgayTu.value) params.ngayTu = filterNgayTu.value
    if (filterNgayDen.value) params.ngayDen = filterNgayDen.value

    const data = await notificationsApi.getAdminNotifications(params)
    // BE wraps in PagedResultDto: { items, pageIndex, pageSize, totalItems }
    notifications.value = data?.items || []
    totalItems.value = data?.totalItems ?? 0
    currentPage.value = data?.pageIndex ?? currentPage.value
  } catch (err) {
    error.value = err?.message || 'Không tải được lịch sử thông báo'
    notifications.value = []
    totalItems.value = 0
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  keyword.value = ''
  filterLoai.value = ''
  filterTrangThai.value = ''
  filterMucDo.value = ''
  filterNgayTu.value = ''
  filterNgayDen.value = ''
  currentPage.value = 1
  loadNotifications()
}

function applyFilters() {
  currentPage.value = 1
  loadNotifications()
}

function prevPage() {
  if (currentPage.value > 1) {
    currentPage.value--
    loadNotifications()
  }
}

function nextPage() {
  if (currentPage.value < totalPages.value) {
    currentPage.value++
    loadNotifications()
  }
}

// ── Detail View ──────────────────────────────────────────────────────
async function openDetail(item) {
  selectedNotification.value = item
  detailLoading.value = true
  try {
    const detail = await notificationsApi.getAdminNotificationDetail(item.maThongBao)
    if (detail) {
      selectedNotification.value = { ...item, ...detail }
    }
  } catch {
    // silently keep existing item data
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  selectedNotification.value = null
}

// ── Watchers & Init ──────────────────────────────────────────────────
// Server-side keyword search (debounced via watch delay)
let keywordTimer = null
watch(keyword, () => {
  clearTimeout(keywordTimer)
  keywordTimer = setTimeout(() => {
    currentPage.value = 1
    loadNotifications()
  }, 400)
})

onMounted(loadNotifications)
</script>

<template>
  <div class="space-y-4 pb-10">

    <!-- Header -->
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h2 class="text-xl font-bold text-heading">Lịch sử thông báo</h2>
        <p class="text-xs text-body mt-0.5">
          {{ totalItems > 0 ? `${totalItems} thông báo trong hệ thống` : 'Dữ liệu từ API quản trị thông báo' }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          class="inline-flex items-center gap-1.5 rounded-xl border border-default surface-card px-3 py-2 text-xs font-semibold text-heading hover:bg-(--surface-input) transition-colors"
          :class="{ 'border-(--lg-primary) text-(--lg-primary)': showFilters || hasActiveFilters }"
          @click="showFilters = !showFilters"
        >
          <Filter :size="14" />
          Bộ lọc
          <span
            v-if="hasActiveFilters"
            class="ml-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-(--lg-primary) text-[10px] text-white font-bold"
          >
            {{ [filterLoai, filterTrangThai, filterMucDo, filterNgayTu, filterNgayDen].filter(Boolean).length }}
          </span>
        </button>
        <button
          class="inline-flex items-center gap-1.5 rounded-xl border border-default surface-card px-3 py-2 text-xs font-semibold text-heading hover:bg-(--surface-input) transition-colors"
          :disabled="loading"
          @click="loadNotifications"
        >
          <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
          Tải lại
        </button>
      </div>
    </div>

    <!-- Search + Filter Bar -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm space-y-3">
      <!-- Search -->
      <div class="relative max-w-md">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-body" :size="16" />
        <input
          v-model="keyword"
          class="w-full rounded-xl border border-input bg-(--surface-input) py-2 pl-9 pr-3 text-sm text-body outline-none focus:border-(--lg-primary)"
          placeholder="Tìm tiêu đề, nội dung, trạng thái..."
          type="text"
        />
      </div>

      <!-- Advanced Filters (toggle) -->
      <div v-if="showFilters" class="grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
        <!-- Loại thông báo -->
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Loại thông báo</label>
          <select
            v-model="filterLoai"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          >
            <option v-for="o in LOAI_OPTIONS" :key="o.value" :value="o.value">{{ o.label }}</option>
          </select>
        </div>

        <!-- Trạng thái -->
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Trạng thái</label>
          <select
            v-model="filterTrangThai"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          >
            <option v-for="o in TRANG_THAI_OPTIONS" :key="o.value" :value="o.value">{{ o.label }}</option>
          </select>
        </div>

        <!-- Mức độ -->
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Mức độ</label>
          <select
            v-model="filterMucDo"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          >
            <option v-for="o in MUC_DO_OPTIONS" :key="o.value" :value="o.value">{{ o.label }}</option>
          </select>
        </div>

        <!-- Từ ngày -->
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Từ ngày</label>
          <input
            v-model="filterNgayTu"
            type="date"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          />
        </div>

        <!-- Đến ngày -->
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Đến ngày</label>
          <input
            v-model="filterNgayDen"
            type="date"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          />
        </div>
      </div>

      <!-- Reset filters -->
      <div v-if="hasActiveFilters" class="flex items-center gap-2">
        <button
          class="inline-flex items-center gap-1.5 rounded-xl bg-(--color-danger-bg) px-3 py-1.5 text-xs font-semibold text-(--color-danger-text) hover:opacity-80 transition-opacity"
          @click="resetFilters"
        >
          <X :size="12" /> Xoá bộ lọc
        </button>
        <span class="text-xs text-body">Đang lọc theo {{ [filterLoai, filterTrangThai, filterMucDo, filterNgayTu, filterNgayDen].filter(Boolean).length }} điều kiện</span>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-16 text-body">
      <Loader2 class="mr-2 animate-spin" :size="20" />
      Đang tải lịch sử thông báo...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <AlertCircle class="text-(--color-danger-text)" :size="28" />
      <p class="text-sm font-semibold text-(--color-danger-text)">{{ error }}</p>
      <button
        class="text-xs text-(--lg-primary) hover:underline"
        @click="loadNotifications"
      >Thử lại</button>
    </div>

    <!-- Empty -->
    <div v-else-if="notifications.length === 0" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <Bell class="text-body" :size="32" />
      <p class="text-sm font-semibold text-heading">Chưa có thông báo phù hợp.</p>
      <p v-if="hasActiveFilters" class="max-w-md text-xs text-body">
        Thử xoá bộ lọc để xem tất cả thông báo.
      </p>
      <button v-if="hasActiveFilters" class="text-xs text-(--lg-primary) hover:underline" @click="resetFilters">Xoá bộ lọc</button>
    </div>

    <!-- Table -->
    <div v-else class="overflow-hidden rounded-2xl border border-card surface-card shadow-sm">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="bg-(--surface-input) border-b border-card">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">Tiêu đề</th>
              <th class="px-4 py-3 font-bold text-heading">Loại</th>
              <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
              <th class="px-4 py-3 font-bold text-heading">Mức độ</th>
              <th class="px-4 py-3 font-bold text-heading">Phạm vi</th>
              <th class="px-4 py-3 font-bold text-heading">Người tạo</th>
              <th class="px-4 py-3 font-bold text-heading text-right">Người nhận</th>
              <th class="px-4 py-3 font-bold text-heading text-right">Đã đọc</th>
              <th class="px-4 py-3 font-bold text-heading">Ngày gửi</th>
              <th class="px-4 py-3 font-bold text-heading"></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="item in notifications"
              :key="item.maThongBao"
              class="border-t border-default hover:bg-(--surface-input) transition-colors"
            >
              <!-- Tiêu đề -->
              <td class="px-4 py-3 max-w-[240px]">
                <p class="font-semibold text-heading truncate" :title="item.tieuDe">{{ item.tieuDe || '-' }}</p>
                <p v-if="item.tomTat || item.tomTatNoiDung" class="text-xs text-body truncate mt-0.5" :title="item.tomTat || item.tomTatNoiDung">
                  {{ item.tomTat || item.tomTatNoiDung }}
                </p>
              </td>

              <!-- Loại -->
              <td class="px-4 py-3 text-body text-xs whitespace-nowrap">{{ item.loaiThongBao || '-' }}</td>

              <!-- Trạng thái -->
              <td class="px-4 py-3">
                <GlassBadge :variant="getTrangThaiBadgeVariant(item.trangThai)">
                  {{ getTrangThaiLabel(item.trangThai) }}
                </GlassBadge>
              </td>

              <!-- Mức độ -->
              <td class="px-4 py-3">
                <GlassBadge :variant="getMucDoBadgeVariant(item.mucDo)">
                  {{ getMucDoLabel(item.mucDo) }}
                </GlassBadge>
              </td>

              <!-- Phạm vi -->
              <td class="px-4 py-3 text-body text-xs whitespace-nowrap">{{ getPhamViLabel(item.phamViGui) }}</td>

              <!-- Người tạo -->
              <td class="px-4 py-3 text-body text-xs whitespace-nowrap">{{ item.tenNguoiTao || 'Hệ thống' }}</td>

              <!-- Người nhận -->
              <td class="px-4 py-3 text-right">
                <span class="text-sm font-semibold text-heading">{{ item.recipientCount ?? '-' }}</span>
              </td>

              <!-- Đã đọc -->
              <td class="px-4 py-3 text-right">
                <span v-if="item.recipientCount > 0" class="text-xs text-body">
                  {{ item.readCount ?? 0 }}
                  <span class="text-body/50">/{{ item.recipientCount }}</span>
                </span>
                <span v-else class="text-xs text-body">-</span>
              </td>

              <!-- Ngày gửi — FIX: use guiLuc || ngayTao (was ngayGui/createdAt/sentAt which don't exist in DTO) -->
              <td class="px-4 py-3 text-body text-xs whitespace-nowrap">
                {{ formatDateTime(item.guiLuc || item.ngayTao, '-') }}
              </td>

              <!-- Detail -->
              <td class="px-4 py-3">
                <button
                  class="inline-flex items-center gap-1 rounded-lg border border-default px-2 py-1.5 text-xs text-body hover:text-heading hover:bg-(--surface-input) transition-colors"
                  @click="openDetail(item)"
                >
                  <Eye :size="13" /> Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="flex items-center justify-between gap-4 border-t border-card px-4 py-3">
        <p class="text-xs text-body">
          Trang <span class="font-semibold text-heading">{{ currentPage }}</span>
          /{{ totalPages }}
          &middot; {{ totalItems }} thông báo
        </p>
        <div class="flex items-center gap-2">
          <button
            class="inline-flex items-center gap-1 rounded-xl border border-default px-3 py-1.5 text-xs font-semibold text-heading disabled:opacity-40 disabled:cursor-not-allowed hover:bg-(--surface-input) transition-colors"
            :disabled="currentPage <= 1 || loading"
            @click="prevPage"
          >
            <ChevronLeft :size="14" /> Trước
          </button>
          <button
            class="inline-flex items-center gap-1 rounded-xl border border-default px-3 py-1.5 text-xs font-semibold text-heading disabled:opacity-40 disabled:cursor-not-allowed hover:bg-(--surface-input) transition-colors"
            :disabled="currentPage >= totalPages || loading"
            @click="nextPage"
          >
            Sau <ChevronRight :size="14" />
          </button>
        </div>
      </div>
    </div>

    <!-- Detail Modal -->
    <Teleport to="body">
      <div
        v-if="selectedNotification"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        @click.self="closeDetail"
      >
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" @click="closeDetail" />
        <div class="relative w-full max-w-2xl rounded-3xl border border-card surface-card shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
          <!-- Modal Header -->
          <div class="flex items-start justify-between gap-4 border-b border-card px-6 py-4">
            <div class="min-w-0">
              <h3 class="text-lg font-bold text-heading truncate">{{ selectedNotification.tieuDe }}</h3>
              <div class="flex flex-wrap gap-2 mt-1.5">
                <GlassBadge :variant="getTrangThaiBadgeVariant(selectedNotification.trangThai)">
                  {{ getTrangThaiLabel(selectedNotification.trangThai) }}
                </GlassBadge>
                <GlassBadge :variant="getMucDoBadgeVariant(selectedNotification.mucDo)">
                  {{ getMucDoLabel(selectedNotification.mucDo) }}
                </GlassBadge>
              </div>
            </div>
            <button
              class="flex-shrink-0 rounded-xl border border-default p-1.5 text-body hover:bg-(--surface-input) transition-colors"
              @click="closeDetail"
            >
              <X :size="16" />
            </button>
          </div>

          <!-- Modal Body -->
          <div class="overflow-y-auto flex-1 px-6 py-4 space-y-4">
            <div v-if="detailLoading" class="flex items-center justify-center py-8 text-body">
              <Loader2 class="mr-2 animate-spin" :size="18" /> Đang tải chi tiết...
            </div>

            <template v-else>
              <!-- Meta grid -->
              <div class="grid gap-3 sm:grid-cols-2">
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Loại thông báo</p>
                  <p class="text-sm font-semibold text-heading">{{ selectedNotification.loaiThongBao || '-' }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Phạm vi gửi</p>
                  <p class="text-sm font-semibold text-heading">{{ getPhamViLabel(selectedNotification.phamViGui) }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Người tạo</p>
                  <p class="text-sm font-semibold text-heading">{{ selectedNotification.tenNguoiTao || 'Hệ thống' }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Đơn vị</p>
                  <p class="text-sm font-semibold text-heading">{{ selectedNotification.tenDonVi || '-' }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Ngày tạo</p>
                  <p class="text-sm font-semibold text-heading">{{ formatDateTime(selectedNotification.ngayTao, '-') }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Ngày gửi</p>
                  <p class="text-sm font-semibold text-heading">{{ formatDateTime(selectedNotification.guiLuc, '-') }}</p>
                </div>
              </div>

              <!-- Statistics -->
              <div class="rounded-2xl border border-card bg-(--surface-input) p-4">
                <p class="text-xs font-bold text-label mb-3">Thống kê đọc</p>
                <div class="grid grid-cols-3 gap-3 text-center">
                  <div>
                    <p class="text-2xl font-bold text-heading">{{ selectedNotification.recipientCount ?? 0 }}</p>
                    <p class="text-xs text-body">Người nhận</p>
                  </div>
                  <div>
                    <p class="text-2xl font-bold text-(--color-success-text)">{{ selectedNotification.readCount ?? 0 }}</p>
                    <p class="text-xs text-body">Đã đọc</p>
                  </div>
                  <div>
                    <p class="text-2xl font-bold text-(--color-warning-text)">
                      {{ (selectedNotification.recipientCount ?? 0) - (selectedNotification.readCount ?? 0) }}
                    </p>
                    <p class="text-xs text-body">Chưa đọc</p>
                  </div>
                </div>
              </div>

              <!-- Tóm tắt nội dung -->
              <div v-if="selectedNotification.tomTat || selectedNotification.tomTatNoiDung" class="rounded-2xl border border-card bg-(--surface-input) p-4">
                <p class="text-xs font-bold text-label mb-2">Tóm tắt nội dung</p>
                <p class="text-sm text-body leading-relaxed">
                  {{ selectedNotification.tomTat || selectedNotification.tomTatNoiDung }}
                </p>
              </div>
            </template>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>
