<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Nhật ký kiểm toán</h2>
        <p class="text-xs text-muted mt-1">Theo dõi lịch sử thay đổi và thao tác trên hệ thống</p>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-4 items-end">
      <div class="flex-1 min-w-[200px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Từ khóa</label>
        <input v-model="filters.keyword" placeholder="Mô tả, người thay đổi..." class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
      </div>
      <div class="w-full sm:w-44">
        <label class="block text-xs font-bold text-heading mb-1.5">Đối tượng</label>
        <select v-model="filters.loaiDoiTuong" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả</option>
          <option v-for="t in entityTypes" :key="t" :value="t">{{ t }}</option>
        </select>
      </div>
      <div class="w-full sm-w-36">
        <label class="block text-xs font-bold text-heading mb-1.5">Hành động</label>
        <select v-model="filters.hanhDong" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả</option>
          <option value="CREATE">Tạo mới</option>
          <option value="UPDATE">Cập nhật</option>
          <option value="DELETE">Xóa</option>
          <option value="LOCK">Khóa</option>
          <option value="UNLOCK">Mở khóa</option>
          <option value="LOGIN">Đăng nhập</option>
          <option value="HTTP_GET">Xem</option>
        </select>
      </div>
      <div class="w-full sm:w-36">
        <label class="block text-xs font-bold text-heading mb-1.5">Từ ngày</label>
        <input v-model="filters.fromDate" type="date" :max="filters.toDate || undefined" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
      </div>
      <div class="w-full sm:w-36">
        <label class="block text-xs font-bold text-heading mb-1.5">Đến ngày</label>
        <input v-model="filters.toDate" type="date" :min="filters.fromDate || undefined" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
      </div>
      <button @click="applyFilter" class="px-4 py-2 bg-(--surface-input) border border-input hover:bg-(--surface-input-hover) text-heading text-sm font-bold rounded-lg transition-colors h-10">Lọc</button>
    </div>

    <div class="flex-1 surface-card border border-card rounded-2xl shadow-sm flex flex-col overflow-hidden">
      <div class="flex-1 overflow-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="sticky top-0 bg-(--surface-card) z-10 backdrop-blur-[12px]">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">ID</th>
              <th class="px-4 py-3 font-bold text-heading">Đơn vị</th>
              <th class="px-4 py-3 font-bold text-heading">Đối tượng</th>
              <th class="px-4 py-3 font-bold text-heading">Mã ĐT</th>
              <th class="px-4 py-3 font-bold text-heading">Hành động</th>
              <th class="px-4 py-3 font-bold text-heading">Người thay đổi</th>
              <th class="px-4 py-3 font-bold text-heading">Thời điểm</th>
              <th class="px-4 py-3 font-bold text-heading">IP</th>
              <th class="px-4 py-3 font-bold text-heading"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredLogs.length === 0" class="bg-transparent">
              <td colspan="9" class="py-12 text-center text-muted">
                <History :size="32" class="mx-auto mb-2 opacity-50" />
                <p>Không tìm thấy nhật ký nào.</p>
              </td>
            </tr>
            <tr v-for="log in paginatedLogs" :key="log.maKiemToan" class="hover:bg-(--surface-input)/50 transition-colors">
              <td class="px-4 py-3 font-medium">{{ log.maKiemToan }}</td>
              <td class="px-4 py-3 text-xs">{{ log.tenDonVi }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ log.loaiDoiTuong }}</span>
              </td>
              <td class="px-4 py-3 text-muted">{{ log.maDoiTuong }}</td>
              <td class="px-4 py-3">
                <span :class="actionBadge(log.hanhDong)" class="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider">
                  <component :is="actionIcon(log.hanhDong)" :size="12" />
                  {{ log.hanhDong }}
                </span>
              </td>
              <td class="px-4 py-3 font-semibold text-heading">{{ log.tenNguoiThayDoi }}</td>
              <td class="px-4 py-3 text-xs text-muted">{{ log.thoiDiemThayDoi }}</td>
              <td class="px-4 py-3 text-[10px] text-muted font-mono">{{ log.diaChiIp || '—' }}</td>
              <td class="px-4 py-3 text-right">
                <button @click="viewDetail(log)" class="p-1.5 text-muted hover:text-(--lg-primary) hover:bg-(--lg-primary)/10 rounded-lg transition-colors" title="Xem chi tiết">
                  <Eye :size="16" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="p-4 bg-(--surface-card) flex items-center justify-between text-sm">
        <span class="text-muted">
          Hiển thị {{ displayedCount }} / {{ filteredLogs.length }} bản ghi
        </span>
        <div class="flex items-center gap-2">
          <button @click="prevPage" :disabled="currentPage === 1" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang trước</button>
          <span class="px-2 font-bold text-heading">Trang {{ currentPage }} / {{ totalPages }}</span>
          <button @click="nextPage" :disabled="currentPage >= totalPages" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang sau</button>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex flex-col p-4">
      <SkeletonTable :rows="8" :columns="6" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div v-if="detailModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm" @click.self="detailModal = false">
      <div class="w-full max-w-lg surface-card rounded-2xl shadow-2xl border border-default overflow-hidden">
        <div class="p-4 flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">Chi tiết nhật ký #{{ detailLog.maKiemToan }}</h3>
          <button @click="detailModal = false" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted">
            <X :size="20" />
          </button>
        </div>
        <div class="p-6 space-y-3 text-sm">
          <div class="grid grid-cols-2 gap-3">
            <div><span class="text-muted">Đối tượng:</span><p class="font-bold text-heading">{{ detailLog.loaiDoiTuong }}</p></div>
            <div><span class="text-muted">Mã đối tượng:</span><p class="font-bold text-heading">{{ detailLog.maDoiTuong }}</p></div>
            <div><span class="text-muted">Hành động:</span><p class="font-bold text-heading">{{ detailLog.hanhDong }}</p></div>
            <div><span class="text-muted">Đơn vị:</span><p class="font-bold text-heading">{{ detailLog.tenDonVi }}</p></div>
            <div><span class="text-muted">Người thay đổi:</span><p class="font-bold text-heading">{{ detailLog.tenNguoiThayDoi }}</p></div>
            <div><span class="text-muted">Thời điểm:</span><p class="font-bold text-heading">{{ detailLog.thoiDiemThayDoi }}</p></div>
            <div><span class="text-muted">IP:</span><p class="font-bold text-heading font-mono text-xs">{{ detailLog.diaChiIp || '—' }}</p></div>
            <div><span class="text-muted">User Agent:</span><p class="font-bold text-heading text-xs truncate">{{ detailLog.userAgent || '—' }}</p></div>
          </div>
          <div class="pt-3">
            <span class="text-muted">Mô tả:</span>
            <p class="mt-1 text-body bg-(--surface-input) p-3 rounded-lg text-sm">{{ detailLog.moTa }}</p>
          </div>
          <div v-if="detailLog.traceId" class="pt-2">
            <span class="text-muted">Trace ID:</span>
            <p class="font-mono text-xs text-body bg-(--surface-input) p-2 rounded-lg mt-1">{{ detailLog.traceId }}</p>
          </div>
        </div>
        <div class="p-4 flex justify-end">
          <button @click="detailModal = false" class="px-4 py-2 border border-input rounded-lg text-sm font-bold text-body hover:bg-(--surface-input) transition-colors">Đóng</button>
        </div>
      </div>
    </div>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch, onMounted } from 'vue'
import {
  Activity, Search, Filter, Calendar, AlertCircle, ArrowLeft,
  ChevronDown, ArrowUpRight, ArrowDownRight, RefreshCw, Loader2, Download,
  History, Eye, X, Plus, Lock, Unlock, LogIn, Edit3, Trash2
} from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const auditLogs = ref([])
const auditLogsRef = auditLogs

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getAuditLogs()
    auditLogs.value = unwrapApiData(res) || []
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu nhật ký'
  } finally {
    loading.value = false
  }
}

const pageSize = 15
const currentPage = ref(1)
const detailModal = ref(false)
const detailLog = ref(null)

const entityTypes = ['User', 'Organization', 'Role', 'AcademicTerm', 'TrainingProgram', 'Course', 'Building', 'Floor', 'Room', 'HttpRequest']

const filters = reactive({
  keyword: '',
  loaiDoiTuong: '',
  hanhDong: '',
  fromDate: '',
  toDate: '',
})

watch(() => filters.fromDate, (val) => {
  if (val && filters.toDate && val > filters.toDate) {
    filters.toDate = ''
  }
})

const filteredLogs = computed(() => {
  return auditLogs.value.filter(log => {
    if (filters.keyword && !log.moTa?.toLowerCase().includes(filters.keyword.toLowerCase()) && !log.tenNguoiThayDoi?.toLowerCase().includes(filters.keyword.toLowerCase())) return false
    if (filters.loaiDoiTuong && log.loaiDoiTuong !== filters.loaiDoiTuong) return false
    if (filters.hanhDong && log.hanhDong !== filters.hanhDong) return false
    return true
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredLogs.value.length / pageSize)))

const paginatedLogs = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredLogs.value.slice(start, start + pageSize)
})

const displayedCount = computed(() => paginatedLogs.value.length)

function applyFilter() {
  currentPage.value = 1
}

function prevPage() {
  if (currentPage.value > 1) currentPage.value--
}

function nextPage() {
  if (currentPage.value < totalPages.value) currentPage.value++
}

function viewDetail(log) {
  detailLog.value = log
  detailModal.value = true
}

function actionBadge(action) {
  switch (action) {
    case 'CREATE': case 'HTTP_POST': return 'bg-(--color-success-bg) text-(--color-success-text)'
    case 'UPDATE': case 'HTTP_PUT': case 'HTTP_PATCH': return 'bg-(--color-info-bg) text-(--color-info-text)'
    case 'DELETE': case 'HTTP_DELETE': return 'bg-(--color-danger-bg) text-(--color-danger-text)'
    case 'LOCK': return 'bg-(--color-warning-bg) text-(--color-warning-text)'
    case 'UNLOCK': return 'bg-(--color-info-bg) text-(--color-info-text)'
    case 'LOGIN': return 'bg-(--color-info-bg) text-(--color-info-text)'
    case 'HTTP_GET': return 'bg-(--surface-input) text-muted'
    default: return 'bg-(--surface-input) text-muted'
  }
}

function actionIcon(action) {
  switch (action) {
    case 'CREATE': case 'HTTP_POST': return Plus
    case 'UPDATE': case 'HTTP_PUT': case 'HTTP_PATCH': return Edit3
    case 'DELETE': case 'HTTP_DELETE': return Trash2
    case 'LOCK': return Lock
    case 'UNLOCK': return Unlock
    case 'LOGIN': return LogIn
    case 'HTTP_GET': return Search
    default: return Edit3
  }
}

onMounted(() => { loadData() })
</script>
