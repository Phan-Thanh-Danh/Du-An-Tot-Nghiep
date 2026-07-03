<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { courseApi } from '@/services/courseApi'
import { exportToExcel } from '@/services/exportService'
import {
  Search, Plus, BookOpen, Clock, CheckCircle, Archive,
  Eye, Pencil, Trash2, Loader2, AlertCircle, ChevronLeft, ChevronRight,
  ArrowUpDown, ArrowUp, ArrowDown, FileDown, RefreshCw, RotateCcw,
  Copy, CheckSquare, Square, X,
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import CourseStatusBadge from './components/CourseStatusBadge.vue'
import BulkAssignCourseDrawer from './components/BulkAssignCourseDrawer.vue'
import BulkAssignResultDialog from './components/BulkAssignResultDialog.vue'
import CourseDetailDrawer from './components/CourseDetailDrawer.vue'
import EditCourseDrawer from './components/EditCourseDrawer.vue'

const popupStore = usePopupStore()
const loading = ref(false)
const error = ref('')
const courses = ref([])
const pagination = ref({ pageIndex: 1, pageSize: 20, totalItems: 0, totalPages: 0 })

const filters = ref({
  keyword: '',
  maHocKy: null,
  maMonHoc: null,
  maGiaoVien: null,
  maLop: null,
  trangThai: 'all',
})

const sortBy = ref('ngayTao')
const sortDir = ref('desc')

const academicTerms = ref([])
const subjects = ref([])
const teachers = ref([])
const classes = ref([])

const loadingDropdowns = ref(false)

const selectedCourse = ref(null)
const showDetailDrawer = ref(false)
const showEditDrawer = ref(false)
const showBulkAssignDrawer = ref(false)
const showResultDialog = ref(false)
const showConfirmArchive = ref(false)
const archiving = ref(false)

const coursesForStats = ref([])

const pageSizeOptions = [10, 20, 50, 100]

const kpiStats = computed(() => {
  const all = coursesForStats.value.length ? coursesForStats.value : courses.value
  return {
    total: all.length,
    daXuatBan: all.filter(c => c.trangThai === 'da_xuat_ban').length,
    nhap: all.filter(c => c.trangThai === 'nhap').length,
    luuTru: all.filter(c => c.trangThai === 'luu_tru').length,
  }
})

const hasActiveFilters = computed(() => {
  return filters.value.keyword
    || filters.value.maHocKy
    || filters.value.maMonHoc
    || filters.value.maGiaoVien
    || filters.value.maLop
    || filters.value.trangThai !== 'all'
})

// ── Selection ──
const selectedIds = ref(new Set())

const allSelected = computed(() => {
  return courses.value.length > 0 && courses.value.every(c => selectedIds.value.has(c.maKhoaHoc))
})

function toggleSelectAll() {
  if (allSelected.value) {
    selectedIds.value = new Set()
  } else {
    selectedIds.value = new Set(courses.value.map(c => c.maKhoaHoc))
  }
}

function toggleSelect(id) {
  const next = new Set(selectedIds.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  selectedIds.value = next
}

// ── Batch actions ──
const batchProcessing = ref(false)

async function handleBatchArchive() {
  if (selectedIds.value.size === 0) return
  batchProcessing.value = true
  try {
    await courseApi.batchArchive([...selectedIds.value])
    popupStore.success('Lưu trữ hàng loạt', `Đã lưu trữ ${selectedIds.value.size} khóa học.`)
    selectedIds.value = new Set()
    loadCourses()
  } catch (err) {
    popupStore.error('Thất bại', err.message || 'Không thể lưu trữ hàng loạt.')
  } finally {
    batchProcessing.value = false
  }
}

async function handleBatchPublish() {
  if (selectedIds.value.size === 0) return
  batchProcessing.value = true
  try {
    await courseApi.batchPublish([...selectedIds.value])
    popupStore.success('Xuất bản hàng loạt', `Đã xuất bản ${selectedIds.value.size} khóa học.`)
    selectedIds.value = new Set()
    loadCourses()
  } catch (err) {
    popupStore.error('Thất bại', err.message || 'Không thể xuất bản hàng loạt.')
  } finally {
    batchProcessing.value = false
  }
}

function clearSelection() {
  selectedIds.value = new Set()
}

// ── Sort ──
function toggleSort(field) {
  if (sortBy.value === field) {
    if (sortDir.value === 'asc') {
      sortDir.value = 'desc'
    } else if (sortDir.value === 'desc') {
      sortBy.value = null
      sortDir.value = null
    }
  } else {
    sortBy.value = field
    sortDir.value = 'asc'
  }
  loadCourses()
}

function getSortIcon(field) {
  if (sortBy.value !== field) return ArrowUpDown
  return sortDir.value === 'asc' ? ArrowUp : ArrowDown
}

// ── Data ──
async function loadDropdowns() {
  loadingDropdowns.value = true
  try {
    academicTerms.value = [
      { value: 1, label: 'HK1 2025-2026' },
      { value: 2, label: 'HK2 2025-2026' },
    ]
    subjects.value = [
      { value: 1, label: 'Lập trình Web (WEB101)' },
      { value: 2, label: 'Cơ sở dữ liệu (CSDL201)' },
      { value: 3, label: 'Cấu trúc dữ liệu (CTDL301)' },
    ]
    teachers.value = [
      { value: 1, label: 'Nguyễn Văn An' },
      { value: 2, label: 'Trần Thị Bình' },
      { value: 3, label: 'Lê Văn Cường' },
    ]
    classes.value = [
      { value: 1, label: 'CNTT01' },
      { value: 2, label: 'CNTT02' },
      { value: 3, label: 'KTPM01' },
    ]
  } finally {
    loadingDropdowns.value = false
  }
}

async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    const params = {
      ...filters.value,
      sortBy: sortBy.value || undefined,
      sortDir: sortDir.value || undefined,
      pageIndex: pagination.value.pageIndex,
      pageSize: pagination.value.pageSize,
    }
    if (params.trangThai === 'all') params.trangThai = undefined
    const res = await courseApi.getCourses(params)
    const data = res.data || res
    courses.value = data.items || []
    pagination.value = {
      pageIndex: data.pageIndex || 1,
      pageSize: data.pageSize || 20,
      totalItems: data.totalItems || 0,
      totalPages: data.totalPages || 0,
    }
    if (import.meta.env.VITE_ENABLE_MOCK_API === 'true') {
      const allRes = await courseApi.getCourses({ ...filters.value, trangThai: undefined, pageSize: 200 })
      const allData = allRes.data || allRes
      coursesForStats.value = allData.items || []
    }
  } catch (err) {
    error.value = err.message || 'Không thể tải danh sách khóa học'
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  pagination.value.pageIndex = 1
  selectedIds.value = new Set()
  loadCourses()
}

function clearFilters() {
  filters.value = { keyword: '', maHocKy: null, maMonHoc: null, maGiaoVien: null, maLop: null, trangThai: 'all' }
  applyFilters()
}

function goToPage(page) {
  if (page < 1 || page > pagination.value.totalPages) return
  pagination.value.pageIndex = page
  loadCourses()
}

function changePageSize(size) {
  pagination.value.pageSize = size
  pagination.value.pageIndex = 1
  loadCourses()
}

const displayPages = computed(() => {
  const total = pagination.value.totalPages
  const current = pagination.value.pageIndex
  const pages = []
  const maxShow = 5
  let start = Math.max(1, current - Math.floor(maxShow / 2))
  let end = Math.min(total, start + maxShow - 1)
  if (end - start + 1 < maxShow) {
    start = Math.max(1, end - maxShow + 1)
  }
  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})

// ── Row actions ──
function openDetail(course) {
  selectedCourse.value = course
  showDetailDrawer.value = true
}

function openEdit(course) {
  selectedCourse.value = course
  showEditDrawer.value = true
}

function confirmArchive(course) {
  selectedCourse.value = course
  showConfirmArchive.value = true
}

async function handleArchive() {
  if (!selectedCourse.value) return
  archiving.value = true
  try {
    await courseApi.archiveCourse(selectedCourse.value.maKhoaHoc)
    popupStore.success('Lưu trữ thành công', 'Khóa học đã được chuyển sang trạng thái lưu trữ.')
    showConfirmArchive.value = false
    selectedCourse.value = null
    loadCourses()
  } catch (err) {
    popupStore.error('Lưu trữ thất bại', err.message || 'Không thể lưu trữ khóa học')
  } finally {
    archiving.value = false
  }
}

async function handleClone(course) {
  try {
    await courseApi.cloneCourse(course.maKhoaHoc)
    popupStore.success('Nhân bản thành công', `Đã tạo bản sao của "${course.tieuDe}".`)
    loadCourses()
  } catch (err) {
    popupStore.error('Nhân bản thất bại', err.message || 'Không thể nhân bản khóa học.')
  }
}

// ── Export ──
function handleExport() {
  const data = coursesForStats.value.length ? coursesForStats.value : courses.value
  if (!data.length) {
    popupStore.info('Không có dữ liệu', 'Không có khóa học nào để xuất Excel.')
    return
  }
  const rows = data.map(c => ({
    'Mã KH': `#${c.maKhoaHoc}`,
    'Tiêu đề': c.tieuDe || '',
    'Môn học': c.tenMonHoc || '',
    'Giảng viên': c.tenGiaoVien || '',
    'Lớp': c.tenLop || '',
    'Học kỳ': c.tenHocKy || '',
    'Trạng thái': c.trangThai === 'da_xuat_ban' ? 'Đã xuất bản' : c.trangThai === 'nhap' ? 'Nháp' : 'Lưu trữ',
    'Ngày tạo': c.ngayTao ? new Date(c.ngayTao).toLocaleDateString('vi-VN') : '',
    'Mô tả': c.moTa || '',
  }))
  exportToExcel(rows, `KhoaHoc_${new Date().toISOString().slice(0, 10)}.xlsx`, 'Khóa học')
  popupStore.success('Xuất Excel thành công', `Đã xuất ${rows.length} khóa học.`)
}

function openBulkAssign() {
  showBulkAssignDrawer.value = true
}

const bulkResult = ref(null)

function onBulkAssignDone(result) {
  showBulkAssignDrawer.value = false
  bulkResult.value = result
  showResultDialog.value = true
}

function onResultDialogClose() {
  showResultDialog.value = false
  bulkResult.value = null
  loadCourses()
}

onMounted(() => {
  loadDropdowns()
  loadCourses()
})
</script>

<template>
  <div class="space-y-4">
    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng số khóa học</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ kpiStats.total }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <BookOpen :size="20" class="text-(--color-info-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Đã xuất bản</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ kpiStats.daXuatBan }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) flex items-center justify-center">
            <CheckCircle :size="20" class="text-(--color-success-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Nháp</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ kpiStats.nhap }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-warning-bg) flex items-center justify-center">
            <Clock :size="20" class="text-(--color-warning-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Lưu trữ</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ kpiStats.luuTru }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--surface-input) flex items-center justify-center">
            <Archive :size="20" class="text-muted" />
          </div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="p-4 border-b border-default bg-(--surface-input)">
        <div class="flex flex-wrap items-center gap-3">
          <div class="relative flex-1 min-w-[200px]">
            <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
            <input v-model="filters.keyword" placeholder="Tìm kiếm khóa học..."
              class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)"
              @keydown.enter="applyFilters">
          </div>
          <LmsSelect v-model="filters.maHocKy" :options="academicTerms" placeholder="Học kỳ" class="w-40"
            :disabled="loadingDropdowns" @update:model-value="applyFilters" />
          <LmsSelect v-model="filters.maMonHoc" :options="subjects" placeholder="Môn học" class="w-40"
            :disabled="loadingDropdowns" @update:model-value="applyFilters" />
          <LmsSelect v-model="filters.maGiaoVien" :options="teachers" placeholder="Giảng viên" class="w-40"
            :disabled="loadingDropdowns" @update:model-value="applyFilters" />
          <LmsSelect v-model="filters.maLop" :options="classes" placeholder="Lớp" class="w-36"
            :disabled="loadingDropdowns" @update:model-value="applyFilters" />
          <select v-model="filters.trangThai" @change="applyFilters"
            class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="all">Tất cả trạng thái</option>
            <option value="nhap">Nháp</option>
            <option value="da_xuat_ban">Đã xuất bản</option>
            <option value="luu_tru">Lưu trữ</option>
          </select>

          <button v-if="hasActiveFilters" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <RotateCcw :size="14" /> Xóa lọc
          </button>

          <div class="flex items-center gap-1 ml-auto">
            <button @click="handleExport" title="Xuất Excel"
              class="h-10 w-10 rounded-xl flex items-center justify-center text-muted hover:text-heading hover:bg-(--surface-card) transition-colors">
              <FileDown :size="16" />
            </button>
            <button @click="loadCourses" title="Làm mới"
              class="h-10 w-10 rounded-xl flex items-center justify-center text-muted hover:text-heading hover:bg-(--surface-card) transition-colors">
              <RefreshCw :size="15" />
            </button>
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openBulkAssign">
              <Plus :size="15" class="mr-1" /> Tạo khóa học
            </GlassButton>
          </div>
        </div>
      </div>

      <!-- Batch action bar -->
      <div v-if="selectedIds.size > 0"
        class="px-4 py-2.5 bg-(--color-info-bg) border-b border-default flex items-center justify-between">
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-(--color-info-text)">Đã chọn {{ selectedIds.size }} khóa học</span>
          <button @click="clearSelection"
            class="text-xs text-muted hover:text-heading flex items-center gap-1">
            <X :size="12" /> Bỏ chọn
          </button>
        </div>
        <div class="flex items-center gap-2">
          <GlassButton variant="primary" size="sm" :loading="batchProcessing" @click="handleBatchPublish">
            <CheckCircle :size="13" class="mr-1" /> Xuất bản
          </GlassButton>
          <GlassButton variant="danger" size="sm" :loading="batchProcessing" @click="handleBatchArchive">
            <Archive :size="13" class="mr-1" /> Lưu trữ
          </GlassButton>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="p-6">
        <LoadingSkeleton :lines="6" />
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <AlertCircle :size="32" class="text-(--color-danger-text)" />
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="loadCourses">
            Thử lại
          </GlassButton>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="courses.length === 0" class="p-6">
        <EmptyState title="Chưa có khóa học nào"
          description="Chưa có khóa học nào trong cơ sở/học kỳ này. Hãy tạo khóa học đầu tiên.">
          <GlassButton variant="primary" @click="openBulkAssign">
            <Plus :size="15" class="mr-1" /> Tạo khóa học đầu tiên
          </GlassButton>
        </EmptyState>
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-4 py-4 w-10">
                <button @click="toggleSelectAll" class="flex items-center justify-center w-full">
                  <CheckSquare v-if="allSelected" :size="16" class="text-(--lg-primary)" />
                  <Square v-else :size="16" class="text-muted" />
                </button>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('maKhoaHoc')">
                <div class="flex items-center gap-1">
                  Mã KH
                  <component :is="getSortIcon('maKhoaHoc')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('tieuDe')">
                <div class="flex items-center gap-1">
                  Tiêu đề
                  <component :is="getSortIcon('tieuDe')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('tenMonHoc')">
                <div class="flex items-center gap-1">
                  Môn học
                  <component :is="getSortIcon('tenMonHoc')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('tenGiaoVien')">
                <div class="flex items-center gap-1">
                  Giảng viên
                  <component :is="getSortIcon('tenGiaoVien')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('tenLop')">
                <div class="flex items-center gap-1">
                  Lớp
                  <component :is="getSortIcon('tenLop')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('tenHocKy')">
                <div class="flex items-center gap-1">
                  Học kỳ
                  <component :is="getSortIcon('tenHocKy')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('trangThai')">
                <div class="flex items-center gap-1">
                  Trạng thái
                  <component :is="getSortIcon('trangThai')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold cursor-pointer select-none hover:text-heading transition-colors"
                @click="toggleSort('ngayTao')">
                <div class="flex items-center gap-1">
                  Ngày tạo
                  <component :is="getSortIcon('ngayTao')" :size="12" class="shrink-0" />
                </div>
              </th>
              <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="course in courses" :key="course.maKhoaHoc"
              class="hover:bg-(--surface-hover) transition-colors cursor-pointer"
              :class="{ 'bg-(--color-info-bg)/40': selectedIds.has(course.maKhoaHoc) }"
              @click="openDetail(course)">
              <td class="px-4 py-3.5" @click.stop>
                <button @click="toggleSelect(course.maKhoaHoc)" class="flex items-center justify-center w-full">
                  <CheckSquare v-if="selectedIds.has(course.maKhoaHoc)" :size="16" class="text-(--lg-primary)" />
                  <Square v-else :size="16" class="text-muted" />
                </button>
              </td>
              <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">#{{ course.maKhoaHoc }}</td>
              <td class="px-3 py-3.5 font-bold text-heading max-w-[200px] truncate">{{ course.tieuDe || '—' }}</td>
              <td class="px-3 py-3.5 text-body">{{ course.tenMonHoc || '—' }}</td>
              <td class="px-3 py-3.5 text-body">{{ course.tenGiaoVien || '—' }}</td>
              <td class="px-3 py-3.5">
                <GlassBadge variant="primary" size="sm">{{ course.tenLop }}</GlassBadge>
              </td>
              <td class="px-3 py-3.5 text-body">{{ course.tenHocKy || '—' }}</td>
              <td class="px-3 py-3.5">
                <CourseStatusBadge :status="course.trangThai" />
              </td>
              <td class="px-3 py-3.5 text-muted text-xs">{{ course.ngayTao ? new Date(course.ngayTao).toLocaleDateString('vi-VN') : '—' }}</td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors"
                    title="Xem chi tiết" @click.stop="openDetail(course)">
                    <Eye :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors"
                    title="Sửa" @click.stop="openEdit(course)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-secondary-soft) flex items-center justify-center text-muted hover:text-(--color-info-text) transition-colors"
                    title="Nhân bản" @click.stop="handleClone(course)">
                    <Copy :size="14" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors"
                    title="Lưu trữ" @click.stop="confirmArchive(course)">
                    <Trash2 :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="!loading && !error && courses.length > 0"
        class="border-t border-default p-4 bg-(--surface-input) flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <p class="text-xs text-muted">
            Hiển thị {{ ((pagination.pageIndex - 1) * pagination.pageSize) + 1 }}
            - {{ Math.min(pagination.pageIndex * pagination.pageSize, pagination.totalItems) }}
            / {{ pagination.totalItems }}
          </p>
          <div class="flex items-center gap-1.5">
            <label class="text-[10px] font-semibold text-muted uppercase">Hiển thị</label>
            <select v-model="pagination.pageSize" @change="changePageSize(pagination.pageSize)"
              class="h-7 px-2 bg-(--surface-card) border border-(--border-input) rounded-lg text-xs outline-none">
              <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
            </select>
          </div>
        </div>
        <div class="flex items-center gap-1">
          <GlassButton variant="subtle" size="sm" :disabled="pagination.pageIndex <= 1" @click="goToPage(pagination.pageIndex - 1)">
            <ChevronLeft :size="15" />
          </GlassButton>
          <template v-for="page in displayPages" :key="page">
            <button v-if="page === pagination.pageIndex"
              class="h-8 w-8 rounded-lg bg-(--lg-primary) text-white text-xs font-bold shadow-sm">
              {{ page }}
            </button>
            <button v-else @click="goToPage(page)"
              class="h-8 w-8 rounded-lg text-muted text-xs font-bold hover:bg-(--surface-card) hover:text-heading transition-colors">
              {{ page }}
            </button>
          </template>
          <GlassButton variant="subtle" size="sm" :disabled="pagination.pageIndex >= pagination.totalPages" @click="goToPage(pagination.pageIndex + 1)">
            <ChevronRight :size="15" />
          </GlassButton>
        </div>
      </div>
    </div>

    <!-- Drawers & Dialogs -->
    <EditCourseDrawer
      v-if="showEditDrawer && selectedCourse"
      :course="selectedCourse"
      :academic-terms="academicTerms"
      :teachers="teachers"
      :classes="classes"
      @close="showEditDrawer = false"
      @done="loadCourses"
    />
    <CourseDetailDrawer
      v-if="showDetailDrawer && selectedCourse"
      :course="selectedCourse"
      @close="showDetailDrawer = false"
    />
    <BulkAssignCourseDrawer
      v-if="showBulkAssignDrawer"
      :academic-terms="academicTerms"
      :subjects="subjects"
      :teachers="teachers"
      :classes="classes"
      @close="showBulkAssignDrawer = false"
      @done="onBulkAssignDone"
    />
    <ConfirmActionDialog
      v-model="showConfirmArchive"
      title="Lưu trữ khóa học"
      :message="`Bạn có chắc muốn lưu trữ khóa học &quot;${selectedCourse?.tieuDe || ''}&quot;?`"
      confirm-label="Lưu trữ"
      cancel-label="Hủy"
      variant="danger"
      :loading="archiving"
      @confirm="handleArchive"
      @cancel="showConfirmArchive = false"
    />
    <BulkAssignResultDialog
      v-if="showResultDialog && bulkResult"
      :result="bulkResult"
      @close="onResultDialogClose"
    />
  </div>
</template>
