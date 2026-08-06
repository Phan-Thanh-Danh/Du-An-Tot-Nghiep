<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { academicTermApi } from '@/services/academicTermApi'
import { organizationApi } from '@/services/organizationService'
import LmsSelect from '@/components/LmsSelect.vue'
import {
  Search,
  Plus,
  Edit2,
  Lock,
  Unlock,
  Trash2,
  Calendar,
  AlertCircle,
  X,
  MapPin,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  RefreshCw
} from 'lucide-vue-next'

const popup = usePopupStore()

// State & Filters
const searchQuery = ref('')
const selectedCampus = ref('Tất cả')
const selectedStatus = ref('Tất cả')

// Pagination
const pageIndex = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const totalPages = ref(1)

const availableOrganizations = ref([])

const campusFilterOptions = computed(() => [
  { value: 'Tất cả', label: 'Tất cả cơ sở' },
  ...availableOrganizations.value.map(c => ({ value: c.id, label: c.name }))
])

const statusFilterOptions = [
  { value: 'Tất cả', label: 'Tất cả trạng thái' },
  { value: 'dang_mo', label: 'Đang mở' },
  { value: 'da_khoa', label: 'Đã khóa' },
  { value: 'da_ket_thuc', label: 'Đã kết thúc' }
]

const pageSizeOptions = [
  { value: 10, label: '10 / trang' },
  { value: 25, label: '25 / trang' },
  { value: 50, label: '50 / trang' },
  { value: 100, label: '100 / trang' }
]

const formCampusOptions = computed(() => 
  availableOrganizations.value.map(c => ({ value: c.id, label: c.name }))
)

const loading = ref(false)
const error = ref('')
const terms = ref([])
const allTermsCache = ref([]) // For client side search if API doesn't support

// Helper to flatten organization tree
function flattenOrganizations(tree) {
  const result = []
  function traverse(nodes, depth = 0) {
    if (!nodes) return
    const list = Array.isArray(nodes) ? nodes : [nodes]
    for (const node of list) {
      if (!node) continue
      const id = node.id ?? node.Id ?? node.maDonVi
      const name = node.name ?? node.Name ?? node.tenDonVi ?? ''
      const prefix = depth > 0 ? '— '.repeat(depth) : ''
      if (id && name) {
        result.push({ id, name, displayName: `${prefix}${name}` })
      }
      const children = node.children ?? node.Children ?? node.cacDonViCon
      if (children && children.length > 0) {
        traverse(children, depth + 1)
      }
    }
  }
  traverse(tree)
  return result
}

async function loadOrganizations() {
  try {
    const data = await organizationApi.getTree()
    const rawTree = Array.isArray(data) ? data : (data?.items ?? data?.data ?? (data ? [data] : []))
    availableOrganizations.value = flattenOrganizations(rawTree)
    
    if (availableOrganizations.value.length === 0) {
      const allData = await organizationApi.getAll()
      const list = Array.isArray(allData) ? allData : (allData?.data ?? allData?.items ?? [])
      availableOrganizations.value = list.map(o => ({
        id: o.id || o.maDonVi,
        name: o.name || o.tenDonVi,
        displayName: o.name || o.tenDonVi
      }))
    }
  } catch (e) {
    console.error('Không thể nạp danh sách cơ sở:', e)
  }
}

async function loadTerms() {
  loading.value = true
  error.value = ''
  try {
    const res = await academicTermApi.list({ pageSize: 1000 })
    let list = Array.isArray(res) ? res : (res?.items ?? [])
    allTermsCache.value = list
    
    applyFilters()
  } catch (e) {
    error.value = e?.response?.data?.message || e?.message || 'Không thể tải danh sách học kỳ.'
    terms.value = []
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  let filtered = [...allTermsCache.value]
  
  // Search
  const kw = searchQuery.value.trim().toLowerCase()
  if (kw) {
    filtered = filtered.filter(t => 
      (t.tenHocKy || t.TenHocKy || '').toLowerCase().includes(kw) ||
      (t.maCodeHocKy || t.MaCodeHocKy || '').toLowerCase().includes(kw)
    )
  }
  
  // Campus
  if (selectedCampus.value !== 'Tất cả') {
    const orgId = Number(selectedCampus.value)
    filtered = filtered.filter(t => (t.maDonVi || t.MaDonVi) === orgId)
  }
  
  // Status
  if (selectedStatus.value !== 'Tất cả') {
    filtered = filtered.filter(t => getStatusObj(t).code === selectedStatus.value)
  }
  
  // Pagination
  totalItems.value = filtered.length
  totalPages.value = Math.max(1, Math.ceil(totalItems.value / pageSize.value))
  if (pageIndex.value > totalPages.value) pageIndex.value = 1
  
  const start = (pageIndex.value - 1) * pageSize.value
  terms.value = filtered.slice(start, start + pageSize.value)
}

watch([selectedCampus, selectedStatus, pageSize, searchQuery], () => {
  pageIndex.value = 1
  applyFilters()
})

const getStatusObj = (term) => {
   const now = new Date()
   const isLocked = term.daKhoa || term.DaKhoa
   const endDateStr = term.ngayKetThuc || term.NgayKetThuc
   const endDate = endDateStr ? new Date(endDateStr) : null
   
   if (isLocked) return { code: 'da_khoa', class: 'bg-rose-500/15 text-rose-700 dark:text-rose-300 border-rose-300 border', label: 'Đã khóa', icon: Lock }
   if (endDate && now > endDate) return { code: 'da_ket_thuc', class: 'bg-slate-500/15 text-slate-700 dark:text-slate-300 border-slate-300 border', label: 'Đã kết thúc', icon: AlertCircle }
   return { code: 'dang_mo', class: 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-300 border', label: 'Đang mở', icon: CheckCircle2 }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '--'
  try {
    return new Date(dateStr).toLocaleDateString('vi-VN')
  } catch {
    return dateStr
  }
}

// Modals & Drawers
const isDrawerOpen = ref(false)
const drawerMode = ref('create') // 'create' or 'edit'
const termForm = ref({
  id: 0,
  maCodeHocKy: '',
  tenHocKy: '',
  namHoc: new Date().getFullYear(),
  ngayBatDau: '',
  ngayKetThuc: '',
  maDonVi: null
})

const isConfirmModalOpen = ref(false)
const confirmAction = ref('') // 'lock', 'unlock', 'delete'
const actionTerm = ref(null)

// Format cho input date (YYYY-MM-DD)
const toInputDate = (dateStr) => {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toISOString().split('T')[0]
}

const openCreateDrawer = () => {
  drawerMode.value = 'create'
  termForm.value = {
    id: 0,
    maCodeHocKy: '',
    tenHocKy: '',
    namHoc: new Date().getFullYear(),
    ngayBatDau: '',
    ngayKetThuc: '',
    maDonVi: availableOrganizations.value[0]?.id || null
  }
  isDrawerOpen.value = true
}

const openEditDrawer = (term) => {
  drawerMode.value = 'edit'
  termForm.value = {
    id: term.maHocKy || term.MaHocKy,
    maCodeHocKy: term.maCodeHocKy || term.MaCodeHocKy || '',
    tenHocKy: term.tenHocKy || term.TenHocKy || '',
    namHoc: term.namHoc || term.NamHoc || new Date().getFullYear(),
    ngayBatDau: toInputDate(term.ngayBatDau || term.NgayBatDau),
    ngayKetThuc: toInputDate(term.ngayKetThuc || term.NgayKetThuc),
    maDonVi: term.maDonVi || term.MaDonVi || null
  }
  isDrawerOpen.value = true
}

const submitForm = async () => {
  if (!termForm.value.maCodeHocKy || !termForm.value.tenHocKy || !termForm.value.ngayBatDau || !termForm.value.ngayKetThuc) {
    popup.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ các thông tin bắt buộc.')
    return
  }
  
  if (new Date(termForm.value.ngayBatDau) >= new Date(termForm.value.ngayKetThuc)) {
    popup.warning('Lỗi thời gian', 'Ngày kết thúc phải lớn hơn ngày bắt đầu.')
    return
  }

  const payload = {
    maCodeHocKy: termForm.value.maCodeHocKy.trim(),
    tenHocKy: termForm.value.tenHocKy.trim(),
    namHoc: Number(termForm.value.namHoc),
    ngayBatDau: new Date(termForm.value.ngayBatDau).toISOString(),
    ngayKetThuc: new Date(termForm.value.ngayKetThuc).toISOString(),
    maDonVi: Number(termForm.value.maDonVi)
  }

  try {
    if (drawerMode.value === 'create') {
      await academicTermApi.create(payload)
      popup.success('Thành công', 'Đã thêm mới học kỳ.')
    } else {
      await academicTermApi.update(termForm.value.id, payload)
      popup.success('Thành công', 'Đã cập nhật thông tin học kỳ.')
    }
    isDrawerOpen.value = false
    await loadTerms()
  } catch (e) {
    popup.error('Lỗi', e?.response?.data?.message || e?.message || 'Không thể lưu học kỳ.')
  }
}

const openConfirm = (action, term) => {
  confirmAction.value = action
  actionTerm.value = term
  isConfirmModalOpen.value = true
}

const executeConfirm = async () => {
  const id = actionTerm.value.maHocKy || actionTerm.value.MaHocKy
  try {
    if (confirmAction.value === 'lock') {
      await academicTermApi.lock(id)
      popup.success('Đã khóa', 'Học kỳ đã được khóa thành công.')
    } else if (confirmAction.value === 'unlock') {
      await academicTermApi.unlock(id)
      popup.success('Đã mở khóa', 'Học kỳ đã được mở khóa.')
    } else if (confirmAction.value === 'delete') {
      await academicTermApi.remove(id)
      popup.success('Đã xóa', 'Học kỳ đã được xóa khỏi hệ thống.')
    }
    isConfirmModalOpen.value = false
    await loadTerms()
  } catch (e) {
    popup.error('Lỗi', e?.response?.data?.message || e?.message || 'Thao tác thất bại.')
  }
}

onMounted(async () => {
  await loadOrganizations()
  await loadTerms()
})
</script>

<template>
  <div class="semesters-page">
    <!-- Header -->
    <header class="page-header mb-6">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-heading flex items-center gap-2">
            Quản lý học kỳ
            <span v-if="totalItems > 0" class="text-xs px-2.5 py-0.5 rounded-full bg-blue-500/10 text-blue-600 dark:text-blue-400 font-semibold border border-blue-200 dark:border-blue-800">
              {{ totalItems }} học kỳ
            </span>
          </h1>
          <p class="text-sm text-label mt-1">Cấu hình thời gian đào tạo, khóa và quản lý các học kỳ toàn hệ thống.</p>
        </div>
        <div class="flex items-center gap-3">
          <button @click="loadTerms" class="glass-btn secondary shadow-sm inline-flex items-center gap-2" title="Tải lại dữ liệu">
            <RefreshCw :size="16" :class="{ 'animate-spin': loading }" /> Tải lại
          </button>
          <button @click="openCreateDrawer" class="glass-btn primary shadow-sm inline-flex items-center gap-2">
            <Plus :size="16" /> Thêm học kỳ
          </button>
        </div>
      </div>
    </header>

    <!-- Controls (Search & Filters) -->
    <div class="controls-panel glass-panel relative z-20 mb-6 p-4 rounded-2xl flex flex-col lg:flex-row gap-4 items-center justify-between">
      <div class="search-box relative w-full lg:w-80">
        <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm kiếm theo Tên, Mã..." 
          class="glass-input w-full pl-10"
        />
      </div>
      
      <div class="filters flex flex-wrap items-center gap-3 w-full lg:w-auto">
        <div class="w-64">
          <LmsSelect v-model="selectedCampus" :options="campusFilterOptions" placeholder="Cơ sở" />
        </div>
        <div class="w-48">
          <LmsSelect v-model="selectedStatus" :options="statusFilterOptions" placeholder="Trạng thái" />
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="glass-panel rounded-2xl p-6 text-center text-label">
      <RefreshCw :size="32" class="animate-spin mx-auto mb-2 text-blue-500" />
      <p class="text-sm">Đang nạp dữ liệu...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="glass-panel rounded-2xl p-12 flex flex-col items-center justify-center">
      <AlertCircle :size="40" class="text-rose-400 mb-3" />
      <p class="text-rose-600 dark:text-rose-400 font-semibold mb-2">{{ error }}</p>
      <button @click="loadTerms" class="glass-btn primary text-xs">Thử lại</button>
    </div>

    <!-- Data Table -->
    <div v-else class="table-container glass-panel rounded-2xl overflow-hidden">
      <table class="w-full text-left border-collapse">
        <thead class="bg-[var(--surface-muted)] text-xs font-semibold uppercase text-label">
          <tr>
            <th class="px-4 py-3">Mã học kỳ</th>
            <th class="px-4 py-3">Tên học kỳ & Năm</th>
            <th class="px-4 py-3">Cơ sở</th>
            <th class="px-4 py-3">Thời gian</th>
            <th class="px-4 py-3 text-center">Trạng thái</th>
            <th class="px-4 py-3 text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-card)]">
          <tr v-if="terms.length === 0">
            <td colspan="6" class="text-center py-10 text-placeholder">Không có học kỳ nào.</td>
          </tr>
          <tr v-for="term in terms" :key="term.maHocKy || term.MaHocKy" class="hover:bg-slate-500/5 transition border-t border-slate-500/10">
            <td class="px-4 py-3 font-semibold text-heading">{{ term.maCodeHocKy || term.MaCodeHocKy }}</td>
            <td class="px-4 py-3">
              <div class="font-bold text-heading">{{ term.tenHocKy || term.TenHocKy }}</div>
              <div class="text-xs text-label">Năm học: {{ term.namHoc || term.NamHoc }}</div>
            </td>
            <td class="px-4 py-3 text-sm text-body flex items-center gap-1">
              <MapPin :size="14" class="text-placeholder"/>
              {{ term.tenDonVi || term.TenDonVi || 'Hệ thống' }}
            </td>
            <td class="px-4 py-3 text-sm text-body">
              <div class="flex items-center gap-1"><Calendar :size="14" class="text-placeholder"/> {{ formatDate(term.ngayBatDau || term.NgayBatDau) }} - {{ formatDate(term.ngayKetThuc || term.NgayKetThuc) }}</div>
            </td>
            <td class="px-4 py-3 text-center">
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold" :class="getStatusObj(term).class">
                <component :is="getStatusObj(term).icon" :size="12" />
                {{ getStatusObj(term).label }}
              </div>
            </td>
            <td class="px-4 py-3 text-right">
              <div class="flex items-center justify-end gap-2">
                <button @click="openEditDrawer(term)" class="action-btn text-teal-600 hover:bg-teal-500/10" title="Chỉnh sửa">
                  <Edit2 :size="16" />
                </button>
                <button v-if="!(term.daKhoa || term.DaKhoa)" @click="openConfirm('lock', term)" class="action-btn text-amber-600 hover:bg-amber-500/10" title="Khóa học kỳ">
                  <Lock :size="16" />
                </button>
                <button v-else @click="openConfirm('unlock', term)" class="action-btn text-emerald-600 hover:bg-emerald-500/10" title="Mở khóa học kỳ">
                  <Unlock :size="16" />
                </button>
                <button @click="openConfirm('delete', term)" class="action-btn text-rose-600 hover:bg-rose-500/10" title="Xóa học kỳ">
                  <Trash2 :size="16" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div class="pagination-bar p-4 border-t border-slate-500/10 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div class="text-xs text-label">
          Trang <strong>{{ pageIndex }}</strong> / <strong>{{ totalPages }}</strong> — Tổng số <strong>{{ totalItems }}</strong> học kỳ
        </div>
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-2 text-xs text-label">
            <span>Hiển thị:</span>
            <div class="w-32">
              <LmsSelect v-model="pageSize" :options="pageSizeOptions" />
            </div>
          </div>
          <div class="flex gap-1">
            <button 
              @click="pageIndex > 1 && (pageIndex--, applyFilters())" 
              :disabled="pageIndex <= 1"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex <= 1 }"
            >
              <ChevronLeft :size="14" /> Trước
            </button>
            <button 
              @click="pageIndex < totalPages && (pageIndex++, applyFilters())" 
              :disabled="pageIndex >= totalPages"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex >= totalPages }"
            >
              Sau <ChevronRight :size="14" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Thêm/Sửa Học kỳ (Popup) -->
    <Teleport to="body">
      <div v-if="isDrawerOpen" class="modal-overlay" @click.self="isDrawerOpen = false">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between pb-4 border-b border-slate-500/10 mb-4">
            <h3 class="font-bold text-heading text-lg">{{ drawerMode === 'create' ? 'Thêm mới Học kỳ' : 'Cập nhật Học kỳ' }}</h3>
            <button @click="isDrawerOpen = false" class="text-label hover:text-heading p-1 rounded-lg hover:bg-slate-500/10"><X :size="20" /></button>
          </div>

          <div class="flex flex-col gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Cơ sở <span class="text-rose-500">*</span></label>
              <LmsSelect v-model="termForm.maDonVi" :options="formCampusOptions" />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Mã học kỳ <span class="text-rose-500">*</span></label>
                <input v-model="termForm.maCodeHocKy" type="text" class="glass-input w-full" placeholder="VD: HK1_2028" />
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Năm học <span class="text-rose-500">*</span></label>
                <input v-model="termForm.namHoc" type="number" class="glass-input w-full" placeholder="VD: 2028" />
              </div>
            </div>

            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Tên học kỳ <span class="text-rose-500">*</span></label>
              <input v-model="termForm.tenHocKy" type="text" class="glass-input w-full" placeholder="VD: Học kỳ 1 năm 2028" />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Ngày bắt đầu <span class="text-rose-500">*</span></label>
                <input v-model="termForm.ngayBatDau" type="date" class="glass-input w-full" />
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Ngày kết thúc <span class="text-rose-500">*</span></label>
                <input v-model="termForm.ngayKetThuc" type="date" class="glass-input w-full" />
              </div>
            </div>

            <div class="mt-6 flex gap-3 pt-2">
              <button @click="isDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
              <button @click="submitForm" class="glass-btn primary flex-1 justify-center">Lưu thông tin</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Xác nhận (Khóa/Mở Khóa/Xóa) -->
      <div v-if="isConfirmModalOpen" class="modal-overlay" @click.self="isConfirmModalOpen = false">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-sm w-full">
          <div class="flex items-center justify-center w-12 h-12 rounded-full mb-4 mx-auto"
               :class="confirmAction === 'delete' ? 'bg-rose-500/15 text-rose-500' : 'bg-amber-500/15 text-amber-500'">
            <Trash2 v-if="confirmAction === 'delete'" :size="24" />
            <AlertCircle v-else :size="24" />
          </div>
          
          <h3 class="text-lg font-bold text-center text-heading mb-1">Xác nhận thao tác</h3>
          
          <p class="text-sm text-center text-label mb-6">
            Bạn có chắc chắn muốn 
            <strong v-if="confirmAction === 'lock'">Khóa</strong>
            <strong v-else-if="confirmAction === 'unlock'">Mở khóa</strong>
            <strong v-else class="text-rose-500">Xóa vĩnh viễn</strong> 
            học kỳ <strong>{{ actionTerm?.tenHocKy || actionTerm?.TenHocKy }}</strong> không?
            
            <template v-if="confirmAction === 'delete'">
              <br/><br/>
              <span class="text-xs text-rose-500">Lưu ý: Chỉ xóa được nếu học kỳ đã khóa và chưa có dữ liệu điểm/lớp.</span>
            </template>
          </p>
          
          <div class="flex gap-3">
            <button @click="isConfirmModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button @click="executeConfirm" class="glass-btn flex-1 justify-center"
                    :class="confirmAction === 'delete' ? 'danger' : 'primary'">
              Xác nhận
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
</style>
