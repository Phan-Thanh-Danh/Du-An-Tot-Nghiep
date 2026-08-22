<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  Code2,
  Filter,
  Loader2,
  RefreshCw,
  Search,
  Shield,
  X,
} from 'lucide-vue-next'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { formatDateTime } from '@/utils/dateFormat'

// ── State ─────────────────────────────────────────────────────────────
const loading = ref(false)
const error = ref('')
const rows = ref([])

// Pagination
const currentPage = ref(1)
const pageSize = ref(20)
const totalItems = ref(0)
const totalPages = computed(() =>
  pageSize.value <= 0 ? 1 : Math.max(1, Math.ceil(totalItems.value / pageSize.value))
)

// Filters
const keyword = ref('')
const filterEntityType = ref('')
const filterAction = ref('')
const filterFromDate = ref('')
const filterToDate = ref('')
const showFilters = ref(false)

// Detail modal
const selectedLog = ref(null)
const detailLoading = ref(false)

// ── Helpers ───────────────────────────────────────────────────────────
const hasActiveFilters = computed(() =>
  keyword.value || filterEntityType.value || filterAction.value ||
  filterFromDate.value || filterToDate.value
)

function getActionBadgeVariant(action) {
  if (!action) return 'neutral'
  const a = action.toUpperCase()
  if (a.includes('DELETE') || a.includes('VOID') || a.includes('CANCEL') || a.includes('HUY')) return 'danger'
  if (a.includes('CREATE') || a.includes('INSERT')) return 'success'
  if (a.includes('UPDATE') || a.includes('EDIT') || a.includes('UPSERT')) return 'warning'
  if (a.includes('LOGIN') || a.includes('LOGOUT') || a.includes('AUTH')) return 'info'
  return 'neutral'
}

function truncate(str, max = 60) {
  if (!str) return '-'
  return str.length <= max ? str : str.slice(0, max) + '…'
}

function formatJson(raw) {
  if (!raw) return null
  try {
    const parsed = typeof raw === 'string' ? JSON.parse(raw) : raw
    return JSON.stringify(parsed, null, 2)
  } catch {
    return raw
  }
}

// ── Data Loading ──────────────────────────────────────────────────────
async function loadLogs() {
  loading.value = true
  error.value = ''
  try {
    const params = new URLSearchParams()
    params.set('pageNumber', String(currentPage.value))
    params.set('pageSize', String(pageSize.value))
    if (keyword.value.trim()) params.set('keyword', keyword.value.trim())
    if (filterEntityType.value) params.set('entityType', filterEntityType.value)
    if (filterAction.value) params.set('action', filterAction.value)
    if (filterFromDate.value) params.set('fromDate', filterFromDate.value)
    if (filterToDate.value) params.set('toDate', filterToDate.value)

    const response = await apiRequest(`/api/audit-logs?${params.toString()}`)
    const data = unwrapApiData(response) // { items, pageIndex, pageSize, totalItems }

    rows.value = Array.isArray(data?.items) ? data.items : []
    totalItems.value = data?.totalItems ?? 0
    currentPage.value = data?.pageIndex ?? currentPage.value
  } catch (err) {
    error.value = err?.message || 'Không tải được audit log.'
    rows.value = []
    totalItems.value = 0
  } finally {
    loading.value = false
  }
}

function resetFilters() {
  keyword.value = ''
  filterEntityType.value = ''
  filterAction.value = ''
  filterFromDate.value = ''
  filterToDate.value = ''
  currentPage.value = 1
  loadLogs()
}

function applyFilters() {
  currentPage.value = 1
  loadLogs()
}

function prevPage() {
  if (currentPage.value > 1) { currentPage.value--; loadLogs() }
}

function nextPage() {
  if (currentPage.value < totalPages.value) { currentPage.value++; loadLogs() }
}

// ── Detail View ───────────────────────────────────────────────────────
async function openDetail(row) {
  selectedLog.value = row
  detailLoading.value = true
  try {
    const response = await apiRequest(`/api/audit-logs/${row.id}`)
    const detail = unwrapApiData(response)
    if (detail) {
      selectedLog.value = { ...row, ...detail }
    }
  } catch {
    // silently keep existing row data
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  selectedLog.value = null
}

// ── Watchers & Init ───────────────────────────────────────────────────
let keywordTimer = null
watch(keyword, () => {
  clearTimeout(keywordTimer)
  keywordTimer = setTimeout(() => {
    currentPage.value = 1
    loadLogs()
  }, 400)
})

onMounted(loadLogs)
</script>

<template>
  <div class="space-y-4 pb-10">

    <!-- Header -->
    <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <div class="flex items-center gap-2 mb-0.5">
          <Shield class="text-(--lg-primary)" :size="20" />
          <h1 class="text-2xl font-bold text-heading">Nhật ký hệ thống</h1>
        </div>
        <p class="text-sm text-body">
          {{ totalItems > 0 ? `${totalItems.toLocaleString('vi-VN')} bản ghi audit log` : 'Dữ liệu thật từ GET /api/audit-logs' }}
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
            {{ [filterEntityType, filterAction, filterFromDate, filterToDate].filter(Boolean).length + (keyword ? 1 : 0) }}
          </span>
        </button>
        <button
          class="inline-flex items-center gap-1.5 rounded-xl border border-default surface-card px-3 py-2 text-xs font-semibold text-heading hover:bg-(--surface-input) transition-colors"
          :disabled="loading"
          @click="loadLogs"
        >
          <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
          Tải lại
        </button>
      </div>
    </div>

    <!-- Search + Filters -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm space-y-3">
      <!-- Keyword search -->
      <div class="relative max-w-lg">
        <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-body" :size="16" />
        <input
          v-model="keyword"
          type="text"
          class="w-full rounded-xl border border-input bg-(--surface-input) py-2 pl-9 pr-3 text-sm text-body outline-none focus:border-(--lg-primary)"
          placeholder="Tìm theo action, loại đối tượng, mô tả, IP..."
        />
      </div>

      <!-- Advanced filters -->
      <div v-if="showFilters" class="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Loại đối tượng</label>
          <input
            v-model="filterEntityType"
            type="text"
            placeholder="vd: User, ThongBao..."
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @blur="applyFilters"
            @keyup.enter="applyFilters"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Hành động</label>
          <input
            v-model="filterAction"
            type="text"
            placeholder="vd: UPDATE_USER, CREATE..."
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @blur="applyFilters"
            @keyup.enter="applyFilters"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Từ ngày</label>
          <input
            v-model="filterFromDate"
            type="datetime-local"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          />
        </div>
        <div class="flex flex-col gap-1">
          <label class="text-xs font-semibold text-label">Đến ngày</label>
          <input
            v-model="filterToDate"
            type="datetime-local"
            class="h-9 rounded-xl border border-input bg-(--surface-input) px-3 text-sm text-body outline-none focus:border-(--lg-primary)"
            @change="applyFilters"
          />
        </div>
      </div>

      <!-- Active filter chips -->
      <div v-if="hasActiveFilters" class="flex flex-wrap items-center gap-2">
        <button
          class="inline-flex items-center gap-1.5 rounded-xl bg-(--color-danger-bg) px-3 py-1.5 text-xs font-semibold text-(--color-danger-text) hover:opacity-80 transition-opacity"
          @click="resetFilters"
        >
          <X :size="12" /> Xoá bộ lọc
        </button>
        <span v-if="keyword" class="rounded-full bg-(--surface-input) border border-default px-2.5 py-1 text-xs text-body">
          Từ khoá: <strong>{{ keyword }}</strong>
        </span>
        <span v-if="filterEntityType" class="rounded-full bg-(--surface-input) border border-default px-2.5 py-1 text-xs text-body">
          Loại: <strong>{{ filterEntityType }}</strong>
        </span>
        <span v-if="filterAction" class="rounded-full bg-(--surface-input) border border-default px-2.5 py-1 text-xs text-body">
          Action: <strong>{{ filterAction }}</strong>
        </span>
        <span v-if="filterFromDate" class="rounded-full bg-(--surface-input) border border-default px-2.5 py-1 text-xs text-body">
          Từ: <strong>{{ filterFromDate.replace('T', ' ') }}</strong>
        </span>
        <span v-if="filterToDate" class="rounded-full bg-(--surface-input) border border-default px-2.5 py-1 text-xs text-body">
          Đến: <strong>{{ filterToDate.replace('T', ' ') }}</strong>
        </span>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center py-16 text-body">
      <Loader2 class="mr-2 animate-spin" :size="20" />
      Đang tải audit log...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <AlertCircle class="text-(--color-danger-text)" :size="28" />
      <p class="text-sm font-semibold text-(--color-danger-text)">{{ error }}</p>
      <button class="text-xs text-(--lg-primary) hover:underline" @click="loadLogs">Thử lại</button>
    </div>

    <!-- Empty -->
    <div v-else-if="rows.length === 0" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <Shield class="text-body" :size="32" />
      <p class="text-sm font-semibold text-heading">Không có audit log phù hợp.</p>
      <button v-if="hasActiveFilters" class="text-xs text-(--lg-primary) hover:underline" @click="resetFilters">Xoá bộ lọc</button>
    </div>

    <!-- Table -->
    <div v-else class="overflow-hidden rounded-2xl border border-card surface-card shadow-sm">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm">
          <thead class="bg-(--surface-input) border-b border-card">
            <tr>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">Thời gian</th>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">Người thao tác</th>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">Hành động</th>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">Đối tượng</th>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">Đơn vị</th>
              <th class="px-4 py-3 font-bold text-heading whitespace-nowrap">IP</th>
              <th class="px-4 py-3 font-bold text-heading">Mô tả</th>
              <th class="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in rows"
              :key="row.id"
              class="border-t border-default hover:bg-(--surface-input) transition-colors"
            >
              <!-- Thời gian -->
              <td class="px-4 py-3 text-xs text-body whitespace-nowrap font-mono">
                {{ formatDateTime(row.changedAt, '-') }}
              </td>

              <!-- Người thao tác -->
              <td class="px-4 py-3 whitespace-nowrap">
                <span class="text-sm font-medium text-heading">
                  {{ row.changedByName || 'Hệ thống' }}
                </span>
                <span v-if="row.changedBy" class="block text-xs text-body">#{{ row.changedBy }}</span>
              </td>

              <!-- Hành động -->
              <td class="px-4 py-3 whitespace-nowrap">
                <GlassBadge :variant="getActionBadgeVariant(row.action)" size="sm">
                  {{ row.action || '-' }}
                </GlassBadge>
              </td>

              <!-- Đối tượng -->
              <td class="px-4 py-3 whitespace-nowrap">
                <span class="text-sm text-heading font-medium">{{ row.entityType || '-' }}</span>
                <span v-if="row.entityId" class="block text-xs text-body font-mono">#{{ row.entityId }}</span>
              </td>

              <!-- Đơn vị -->
              <td class="px-4 py-3 text-xs text-body whitespace-nowrap">
                {{ row.tenDonVi || (row.maDonVi ? `Đơn vị #${row.maDonVi}` : '-') }}
              </td>

              <!-- IP -->
              <td class="px-4 py-3 text-xs text-body font-mono whitespace-nowrap">
                {{ row.ipAddress || '-' }}
              </td>

              <!-- Mô tả -->
              <td class="px-4 py-3 text-sm text-body max-w-[260px]">
                <span :title="row.description">{{ truncate(row.description, 70) }}</span>
              </td>

              <!-- Detail button -->
              <td class="px-4 py-3">
                <button
                  class="inline-flex items-center gap-1 rounded-lg border border-default px-2 py-1.5 text-xs text-body hover:text-heading hover:bg-(--surface-input) transition-colors whitespace-nowrap"
                  @click="openDetail(row)"
                >
                  <Code2 :size="13" /> Chi tiết
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="flex items-center justify-between gap-4 border-t border-card px-4 py-3">
        <p class="text-xs text-body">
          Trang <span class="font-semibold text-heading">{{ currentPage }}</span>/{{ totalPages }}
          &middot; {{ totalItems.toLocaleString('vi-VN') }} bản ghi
        </p>
        <div class="flex items-center gap-2">
          <select
            v-model.number="pageSize"
            class="h-8 rounded-lg border border-input bg-(--surface-input) px-2 text-xs text-body outline-none focus:border-(--lg-primary)"
            @change="() => { currentPage = 1; loadLogs() }"
          >
            <option :value="20">20/trang</option>
            <option :value="50">50/trang</option>
            <option :value="100">100/trang</option>
          </select>
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
        v-if="selectedLog"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        @click.self="closeDetail"
      >
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" @click="closeDetail" />
        <div class="relative w-full max-w-2xl rounded-3xl border border-card surface-card shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">

          <!-- Modal Header -->
          <div class="flex items-start justify-between gap-4 border-b border-card px-6 py-4">
            <div class="min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <GlassBadge :variant="getActionBadgeVariant(selectedLog.action)">
                  {{ selectedLog.action || '-' }}
                </GlassBadge>
              </div>
              <p class="text-sm text-body">{{ selectedLog.entityType }} #{{ selectedLog.entityId }}</p>
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
                  <p class="text-xs text-label mb-0.5">Thời gian</p>
                  <p class="text-sm font-semibold text-heading font-mono">{{ formatDateTime(selectedLog.changedAt, '-') }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Người thao tác</p>
                  <p class="text-sm font-semibold text-heading">
                    {{ selectedLog.changedByName || 'Hệ thống' }}
                    <span v-if="selectedLog.changedBy" class="text-xs text-body font-normal ml-1">(#{{ selectedLog.changedBy }})</span>
                  </p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">Đơn vị</p>
                  <p class="text-sm font-semibold text-heading">{{ selectedLog.tenDonVi || '-' }}</p>
                </div>
                <div class="rounded-2xl border border-card bg-(--surface-input) p-3">
                  <p class="text-xs text-label mb-0.5">IP Address</p>
                  <p class="text-sm font-semibold text-heading font-mono">{{ selectedLog.ipAddress || '-' }}</p>
                </div>
                <div v-if="selectedLog.traceId" class="rounded-2xl border border-card bg-(--surface-input) p-3 sm:col-span-2">
                  <p class="text-xs text-label mb-0.5">Trace ID</p>
                  <p class="text-xs font-mono text-body break-all">{{ selectedLog.traceId }}</p>
                </div>
                <div v-if="selectedLog.userAgent" class="rounded-2xl border border-card bg-(--surface-input) p-3 sm:col-span-2">
                  <p class="text-xs text-label mb-0.5">User Agent</p>
                  <p class="text-xs text-body break-all">{{ selectedLog.userAgent }}</p>
                </div>
              </div>

              <!-- Mô tả -->
              <div v-if="selectedLog.description" class="rounded-2xl border border-card bg-(--surface-input) p-4">
                <p class="text-xs font-bold text-label mb-1.5">Mô tả</p>
                <p class="text-sm text-body leading-relaxed">{{ selectedLog.description }}</p>
              </div>

              <!-- Old Value -->
              <div v-if="selectedLog.oldValue" class="rounded-2xl border border-(--color-warning-text)/30 bg-(--color-warning-bg) p-4">
                <p class="text-xs font-bold text-(--color-warning-text) mb-2">Giá trị trước (Old Value)</p>
                <pre class="text-xs text-body bg-(--surface-card) rounded-xl p-3 overflow-x-auto leading-relaxed">{{ formatJson(selectedLog.oldValue) }}</pre>
              </div>

              <!-- New Value -->
              <div v-if="selectedLog.newValue" class="rounded-2xl border border-(--color-success-text)/30 bg-(--color-success-bg) p-4">
                <p class="text-xs font-bold text-(--color-success-text) mb-2">Giá trị sau (New Value)</p>
                <pre class="text-xs text-body bg-(--surface-card) rounded-xl p-3 overflow-x-auto leading-relaxed">{{ formatJson(selectedLog.newValue) }}</pre>
              </div>

              <!-- No diff notice -->
              <div v-if="!selectedLog.oldValue && !selectedLog.newValue" class="text-center py-4 text-xs text-body">
                Không có diff dữ liệu cho bản ghi này.
              </div>
            </template>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>
