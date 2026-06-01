<script setup>
import { ref, computed } from 'vue'
import { 
  Search, 
  Filter, 
  Download, 
  UserPlus, 
  ArrowLeftRight, 
  Eye, 
  Trash2, 
  CheckCircle2, 
  XCircle,
  MoreVertical,
  History,
  X,
  Save,
  FileSpreadsheet,
  FileText,
  Loader2,
  Check,
  AlertCircle,
  User,
  BookOpen,
  Hash,
  Calendar,
  Layers
} from 'lucide-vue-next'
import PageContainer from '../../../components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const enrollments = ref([
  { id: 1, student: 'Nguyễn Văn Nam', studentCode: 'SV001', section: 'LHP001', subject: 'Lập trình Java', status: 'enrolled', type: 'new', prereq: 'pass', date: '15/01/2026' },
  { id: 2, student: 'Lê Thị Mai', studentCode: 'SV002', section: 'LHP001', subject: 'Lập trình Java', status: 'waitlist', type: 'new', prereq: 'pass', date: '16/01/2026' },
  { id: 3, student: 'Trần Minh Tâm', studentCode: 'SV003', section: 'LHP002', subject: 'Cấu trúc dữ liệu', status: 'enrolled', type: 'retake', prereq: 'pass', date: '15/01/2026' },
  { id: 4, student: 'Phạm Hoàng Anh', studentCode: 'SV004', section: 'LHP003', subject: 'Lập trình Web', status: 'dropped', type: 'new', prereq: 'fail', date: '17/01/2026' },
])

const sections = ['LHP001', 'LHP002', 'LHP003', 'LHP004']
const subjectMap = { LHP001: 'Lập trình Java', LHP002: 'Cấu trúc dữ liệu', LHP003: 'Lập trình Web', LHP004: 'Hệ quản trị CSDL' }

const nextId = ref(5)

// ── Search ───────────────────────────────────────────────────
const searchQuery = ref('')
const searchTriggered = ref('')
const showFilters = ref(false)

const filterStatus = ref('all')
const filterType = ref('all')
const filterPrereq = ref('all')

const activeFilterCount = computed(() => {
  let count = 0
  if (filterStatus.value !== 'all') count++
  if (filterType.value !== 'all') count++
  if (filterPrereq.value !== 'all') count++
  return count
})

function doSearch() {
  searchTriggered.value = searchQuery.value
}

function clearSearch() {
  searchQuery.value = ''
  searchTriggered.value = ''
}

const filteredEnrollments = computed(() => {
  let result = enrollments.value
  const q = searchTriggered.value.toLowerCase().trim()
  if (q) {
    result = result.filter(e =>
      e.student.toLowerCase().includes(q) ||
      e.studentCode.toLowerCase().includes(q) ||
      e.subject.toLowerCase().includes(q) ||
      e.section.toLowerCase().includes(q)
    )
  }
  if (filterStatus.value !== 'all') {
    result = result.filter(e => e.status === filterStatus.value)
  }
  if (filterType.value !== 'all') {
    result = result.filter(e => e.type === filterType.value)
  }
  if (filterPrereq.value !== 'all') {
    result = result.filter(e => e.prereq === filterPrereq.value)
  }
  return result
})

function clearAllFilters() {
  filterStatus.value = 'all'
  filterType.value = 'all'
  filterPrereq.value = 'all'
}

// ── Context Menu ─────────────────────────────────────────────
const contextTarget = ref(null)

function toggleContextMenu(en) {
  contextTarget.value = contextTarget.value?.id === en.id ? null : en
}

function closeContextMenu() {
  contextTarget.value = null
}

// ── Manual Enrollment ────────────────────────────────────────
const showEnrollModal = ref(false)
const enrollForm = ref({
  student: '',
  studentCode: '',
  section: '',
})

const enrollError = ref('')
const enrollSuccess = ref(false)

function openEnrollModal() {
  enrollForm.value = { student: '', studentCode: '', section: '' }
  enrollError.value = ''
  enrollSuccess.value = false
  showEnrollModal.value = true
}

function submitEnrollment() {
  if (!enrollForm.value.student || !enrollForm.value.studentCode || !enrollForm.value.section) {
    enrollError.value = 'Vui lòng điền đầy đủ thông tin'
    return
  }
  const dup = enrollments.value.find(e => e.studentCode === enrollForm.value.studentCode && e.section === enrollForm.value.section)
  if (dup) {
    enrollError.value = `Sinh viên ${enrollForm.value.studentCode} đã đăng ký lớp ${enrollForm.value.section}`
    return
  }
  const newEnroll = {
    id: nextId.value++,
    student: enrollForm.value.student,
    studentCode: enrollForm.value.studentCode,
    section: enrollForm.value.section,
    subject: subjectMap[enrollForm.value.section] || enrollForm.value.section,
    status: 'enrolled',
    type: 'new',
    prereq: 'pass',
    date: new Date().toLocaleDateString('vi-VN'),
  }
  enrollments.value.unshift(newEnroll)
  enrollSuccess.value = true
  enrollError.value = ''
  setTimeout(() => { showEnrollModal.value = false }, 1200)
}

// ── Export ───────────────────────────────────────────────────
const showExportModal = ref(false)
const exportFormat = ref('excel')
const exporting = ref(false)
const exportDone = ref(false)

function openExportModal() {
  exportFormat.value = 'excel'
  exportDone.value = false
  showExportModal.value = true
}

function doExport() {
  exporting.value = true
  const data = filteredEnrollments.value.map(e => ({
    'Sinh viên': e.student,
    'Mã SV': e.studentCode,
    'Lớp': e.section,
    'Môn học': e.subject,
    'Trạng thái': e.status,
    'Loại': e.type,
    'Điều kiện': e.prereq === 'pass' ? 'Hợp lệ' : 'Thiếu ĐK',
    'Ngày ĐK': e.date,
  }))
  const headers = Object.keys(data[0] || {})
  if (exportFormat.value === 'csv') {
    const csv = [headers.join(','), ...data.map(r => headers.map(h => `"${r[h]}"`).join(','))].join('\n')
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `danh-sach-dang-ky-${Date.now()}.csv`; a.click()
    URL.revokeObjectURL(url)
  } else {
    const wsCols = headers.map(h => ({ h }))
    const wsData = [headers, ...data.map(r => headers.map(h => r[h]))]
    const workbook = { SheetNames: ['Sheet1'], Sheets: { Sheet1: { '!ref': 'A1:' + String.fromCharCode(64 + headers.length) + wsData.length, '!cols': wsCols.map(() => ({ wch: 22 })) } } }
    // Manual XML-based XLSX generation since direct xlsx may not be available at runtime
    let xml = '<?xml version="1.0" encoding="UTF-8"?><?mso-application progid="Excel.Sheet"?><Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">'
    xml += '<Styles><Style ss:ID="h"><Font ss:Bold="1" ss:Size="11"/></Style></Styles><Worksheet ss:Name="Sheet1"><Table>'
    wsData.forEach((row, i) => {
      xml += '<Row>'
      row.forEach(cell => {
        xml += i === 0 ? `<Cell ss:StyleID="h"><Data ss:Type="String">${cell}</Data></Cell>` : `<Cell><Data ss:Type="String">${cell}</Data></Cell>`
      })
      xml += '</Row>'
    })
    xml += '</Table></Worksheet></Workbook>'
    const blob = new Blob([xml], { type: 'application/vnd.ms-excel' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `danh-sach-dang-ky-${Date.now()}.xls`; a.click()
    URL.revokeObjectURL(url)
  }
  setTimeout(() => {
    exporting.value = false
    exportDone.value = true
    setTimeout(() => { showExportModal.value = false }, 1000)
  }, 600)
}

// ── Transfer Class ───────────────────────────────────────────
const showTransferModal = ref(false)
const transferTarget = ref(null)
const transferSection = ref('')
const transferError = ref('')

function openTransferModal(en) {
  transferTarget.value = en
  transferSection.value = ''
  transferError.value = ''
  showTransferModal.value = true
}

function confirmTransfer() {
  if (!transferTarget.value || !transferSection.value) return
  const dup = enrollments.value.find(e =>
    e.studentCode === transferTarget.value.studentCode &&
    e.section === transferSection.value
  )
  if (dup) {
    transferError.value = `Sinh viên đã đăng ký lớp ${transferSection.value}`
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

// ── Drop Enrollment ──────────────────────────────────────────
const showDropModal = ref(false)
const dropTarget = ref(null)
const dropReason = ref('')

function openDropModal(en) {
  dropTarget.value = en
  dropReason.value = ''
  showDropModal.value = true
}

function confirmDrop() {
  if (!dropTarget.value) return
  dropTarget.value.status = 'dropped'
  showDropModal.value = false
  dropTarget.value = null
  dropReason.value = ''
}

function closeDropModal() {
  showDropModal.value = false
  dropTarget.value = null
  dropReason.value = ''
}

const getStatusBadge = (status) => {
  switch (status) {
    case 'enrolled': return 'lg-badge-success'
    case 'waitlist': return 'lg-badge-warning'
    case 'dropped': return 'lg-badge-danger'
    default: return 'surface-solid text-placeholder'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'enrolled': return 'Đã ĐK'
    case 'waitlist': return 'Chờ'
    case 'dropped': return 'Đã hủy'
    default: return status
  }
}
</script>

<template>
  <PageContainer 
    title="Danh sách đăng ký" 
    subtitle="Theo dõi và xử lý các yêu cầu đăng ký môn học của sinh viên."
  >
    <template #actions>
      <div class="flex items-center gap-3">
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold" @click="openExportModal">
          <Download :size="18" /> Export
        </button>
        <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20" @click="openEnrollModal">
          <UserPlus :size="18" /> Ghép HS thủ công
        </button>
      </div>
    </template>

    <div class="space-y-4" @click="closeContextMenu">
      
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[280px] relative flex items-center gap-2">
          <div class="relative flex-1">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
            <input 
              v-model="searchQuery"
              type="text" 
              placeholder="Tìm theo SV, Mã SV hoặc Môn học..." 
              class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium transition-all"
              @keyup.enter="doSearch"
            >
          </div>
          <button class="lg-button-primary px-4 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20" @click="doSearch">
            <Search :size="16" /> Tìm
          </button>
          <button v-if="searchTriggered" class="lg-button-secondary px-3 py-2.5 text-sm font-bold" @click="clearSearch" title="Xóa tìm kiếm">
            <X :size="16" />
          </button>
        </div>
        <div class="flex items-center gap-3">
          <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold relative" @click.stop="showFilters = !showFilters">
            <Filter :size="18" /> Lọc nâng cao
            <span v-if="activeFilterCount > 0" class="absolute -top-1.5 -right-1.5 h-4 w-4 rounded-full bg-[var(--lg-primary)] text-white text-[9px] font-black flex items-center justify-center">{{ activeFilterCount }}</span>
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
        <div v-if="showFilters" class="lg-glass-strong p-5 rounded-[20px] space-y-3">
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-black text-label uppercase tracking-widest min-w-[70px]">Trạng thái:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="s in ['all','enrolled','waitlist','dropped']" :key="s"
                @click="filterStatus = s"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterStatus === s ? 'bg-[var(--lg-primary)] text-white shadow-md' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ { all: 'Tất cả', enrolled: 'Đã ĐK', waitlist: 'Chờ', dropped: 'Đã hủy' }[s] }}</button>
            </div>
          </div>
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-black text-label uppercase tracking-widest min-w-[70px]">Loại ĐK:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="t in ['all','new','retake']" :key="t"
                @click="filterType = t"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterType === t ? 'bg-[var(--lg-primary)] text-white shadow-md' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ { all: 'Tất cả', new: 'Mới', retake: 'Học lại' }[t] }}</button>
            </div>
          </div>
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-[10px] font-black text-label uppercase tracking-widest min-w-[70px]">Điều kiện:</span>
            <div class="flex gap-1.5 flex-wrap">
              <button v-for="p in ['all','pass','fail']" :key="p"
                @click="filterPrereq = p"
                :class="['px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all', filterPrereq === p ? 'bg-[var(--lg-primary)] text-white shadow-md' : 'surface-solid text-label hover:bg-[var(--surface-input)]']"
              >{{ { all: 'Tất cả', pass: 'Hợp lệ', fail: 'Thiếu ĐK' }[p] }}</button>
            </div>
          </div>
          <div v-if="activeFilterCount > 0" class="pt-2 border-t border-default flex justify-end">
            <button class="text-[11px] font-bold text-placeholder hover:text-label transition-colors flex items-center gap-1" @click="clearAllFilters">
              <X :size="13" /> Xóa tất cả bộ lọc
            </button>
          </div>
        </div>
      </Transition>

      <!-- ── Enrollment Table ── -->
      <div class="lg-table-shell overflow-hidden rounded-[24px]">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Sinh viên</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Lớp & Môn</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Prereq</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="en in filteredEnrollments" :key="en.id" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading">{{ en.student }}</p>
                <p class="text-[11px] font-bold text-placeholder mt-0.5">{{ en.studentCode }}</p>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading leading-tight">{{ en.subject }}</p>
                <div class="flex items-center gap-2 mt-1">
                  <span class="text-[10px] font-black text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ en.section }}</span>
                  <span v-if="en.type === 'retake'" class="text-[9px] font-bold text-[var(--lg-warning)] border border-default px-1.5 py-0.5 rounded uppercase tracking-tighter">Học lại</span>
                </div>
              </td>
              <td class="px-4 py-4">
                 <div v-if="en.prereq === 'pass'" class="text-[var(--lg-success)] flex items-center gap-1.5">
                    <CheckCircle2 :size="16" /> <span class="text-[10px] font-black uppercase tracking-widest">Hợp lệ</span>
                 </div>
                 <div v-else class="text-[var(--lg-danger)] flex items-center gap-1.5">
                    <XCircle :size="16" /> <span class="text-[10px] font-black uppercase tracking-widest">Thiếu ĐK</span>
                 </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest border', getStatusBadge(en.status)]">
                  {{ getStatusLabel(en.status) }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-link rounded-lg text-placeholder transition-all" title="Chuyển lớp" @click.stop="openTransferModal(en)">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] rounded-lg text-placeholder transition-all" title="Hủy đăng ký" @click.stop="openDropModal(en)">
                    <Trash2 :size="16" />
                  </button>
                  <div class="relative">
                    <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder transition-all" @click.stop="toggleContextMenu(en)">
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
                      <div v-if="contextTarget?.id === en.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-xl shadow-slate-900/10" @click.stop>
                        <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="openTransferModal(en); closeContextMenu()">
                          <ArrowLeftRight :size="14" /> Chuyển lớp
                        </button>
                        <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all">
                          <Eye :size="14" /> Xem chi tiết
                        </button>
                        <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all" @click="openDropModal(en); closeContextMenu()">
                          <Trash2 :size="14" /> Hủy đăng ký
                        </button>
                      </div>
                    </Transition>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredEnrollments.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <BookOpen :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-black text-heading">Không có đăng ký nào</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm</p>
        </div>
      </div>

    </div>

    <!-- ═══════════════════════════════════════════════════════
         EXPORT MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showExportModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showExportModal = false">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-sm lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <h3 class="text-base font-black text-heading">Xuất danh sách</h3>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showExportModal = false">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-3">
            <div class="text-[12px] font-medium text-label mb-2">Chọn định dạng xuất:</div>
            <button @click="exportFormat = 'excel'"
              :class="['w-full flex items-center gap-3 p-3 rounded-2xl border transition-all', exportFormat === 'excel' ? 'border-[var(--lg-primary)] bg-[var(--color-info-bg)]' : 'border-default surface-solid hover:bg-[var(--surface-input)]']">
              <div class="h-9 w-9 rounded-xl bg-[var(--lg-primary)]/10 text-[var(--lg-primary)] flex items-center justify-center">
                <FileSpreadsheet :size="20" />
              </div>
              <div class="text-left">
                <p class="text-sm font-bold text-heading">Microsoft Excel (.xls)</p>
                <p class="text-[10px] font-medium text-placeholder">Xuất file Excel tương thích</p>
              </div>
            </button>
            <button @click="exportFormat = 'csv'"
              :class="['w-full flex items-center gap-3 p-3 rounded-2xl border transition-all', exportFormat === 'csv' ? 'border-[var(--lg-primary)] bg-[var(--color-info-bg)]' : 'border-default surface-solid hover:bg-[var(--surface-input)]']">
              <div class="h-9 w-9 rounded-xl bg-[var(--lg-success)]/10 text-[var(--lg-success)] flex items-center justify-center">
                <FileText :size="20" />
              </div>
              <div class="text-left">
                <p class="text-sm font-bold text-heading">CSV (.csv)</p>
                <p class="text-[10px] font-medium text-placeholder">Dữ liệu phân cách dấu phẩy</p>
              </div>
            </button>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="showExportModal = false">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 min-w-[120px] flex items-center justify-center gap-2" @click="doExport" :disabled="exporting">
              <Loader2 v-if="exporting" :size="16" class="animate-spin" />
              <Download v-else :size="16" />
              {{ exporting ? 'Đang xuất...' : exportDone ? 'Đã xuất ✓' : 'Xuất ngay' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════
         MANUAL ENROLLMENT MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showEnrollModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showEnrollModal = false">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <!-- Success state -->
          <div v-if="enrollSuccess" class="flex flex-col items-center py-8 text-center">
            <div class="h-16 w-16 rounded-full bg-[var(--color-success-bg)] text-[var(--lg-success)] flex items-center justify-center mb-4">
              <Check :size="32" />
            </div>
            <h3 class="text-base font-black text-heading">Ghép thủ công thành công!</h3>
            <p class="text-[12px] font-medium text-label mt-1">{{ enrollForm.student }} đã được thêm vào {{ enrollForm.section }}</p>
          </div>
          <!-- Form -->
          <template v-else>
            <div class="flex items-center justify-between mb-5">
              <div class="flex items-center gap-2">
                <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                  <UserPlus :size="18" />
                </div>
                <h3 class="text-base font-black text-heading">Ghép HS thủ công</h3>
              </div>
              <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showEnrollModal = false">
                <X :size="18" />
              </button>
            </div>
            <div class="space-y-4">
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Họ tên sinh viên</label>
                <div class="relative">
                  <User :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
                  <input v-model="enrollForm.student" type="text" placeholder="VD: Nguyễn Văn A" class="w-full lg-input pl-10 pr-4 py-2.5 text-sm" />
                </div>
              </div>
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Mã số sinh viên</label>
                <div class="relative">
                  <Hash :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
                  <input v-model="enrollForm.studentCode" type="text" placeholder="VD: SV001" class="w-full lg-input pl-10 pr-4 py-2.5 text-sm" />
                </div>
              </div>
              <div>
                <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Lớp học phần</label>
                <div class="relative">
                  <BookOpen :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
                  <select v-model="enrollForm.section" class="w-full lg-input pl-10 pr-4 py-2.5 text-sm appearance-none cursor-pointer">
                    <option value="" disabled>Chọn lớp...</option>
                    <option v-for="sec in sections" :key="sec" :value="sec">{{ sec }} - {{ subjectMap[sec] }}</option>
                  </select>
                </div>
              </div>
              <div v-if="enrollError" class="flex items-center gap-2 bg-[var(--color-danger-bg)] border border-[var(--lg-danger)]/30 rounded-2xl p-3">
                <AlertCircle :size="16" class="text-[var(--lg-danger)] flex-shrink-0" />
                <p class="text-[11px] font-bold text-[var(--lg-danger)]">{{ enrollError }}</p>
              </div>
            </div>
            <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
              <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="showEnrollModal = false">Hủy</button>
              <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 flex items-center gap-2" @click="submitEnrollment">
                <Save :size="16" /> Ghép
              </button>
            </div>
          </template>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════
         DROP ENROLLMENT MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showDropModal && dropTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDropModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-danger-bg)] text-[var(--lg-danger)] flex items-center justify-center">
                <Trash2 :size="18" />
              </div>
              <h3 class="text-base font-black text-heading">Hủy đăng ký</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeDropModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-4">
            <div class="h-10 w-10 rounded-full bg-gradient-to-br from-rose-500 to-red-600 text-white text-xs font-black flex items-center justify-center flex-shrink-0">
              {{ dropTarget.studentCode.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-black text-heading">{{ dropTarget.student }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[11px] font-bold text-placeholder">{{ dropTarget.studentCode }}</span>
                <span class="w-1 h-1 rounded-full bg-placeholder" />
                <span class="text-[10px] font-black text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ dropTarget.section }}</span>
              </div>
            </div>
          </div>
          <div class="bg-[var(--color-danger-bg)] border border-[var(--lg-danger)]/30 rounded-2xl p-3 mb-4">
            <p class="text-[11px] font-bold text-[var(--lg-danger)]">Sinh viên sẽ bị hủy đăng ký khỏi lớp <strong>{{ dropTarget.subject }}</strong>. Thao tác này có thể được thực hiện lại nếu cần.</p>
          </div>
          <div>
            <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Lý do hủy (không bắt buộc)</label>
            <textarea v-model="dropReason" rows="2" placeholder="Nhập lý do hủy đăng ký..." class="w-full lg-input px-4 py-2.5 text-sm resize-none"></textarea>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDropModal">Quay lại</button>
            <button class="lg-btn-danger px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="confirmDrop">
              <Trash2 :size="16" /> Xác nhận hủy
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══════════════════════════════════════════════════════
         TRANSFER CLASS MODAL
         ═══════════════════════════════════════════════════════ -->
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
        <div class="relative w-full max-w-md lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <ArrowLeftRight :size="18" />
              </div>
              <h3 class="text-base font-black text-heading">Chuyển lớp</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeTransferModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-5">
            <div class="h-10 w-10 rounded-full bg-gradient-to-br from-blue-500 to-cyan-600 text-white text-xs font-black flex items-center justify-center flex-shrink-0">
              {{ transferTarget.studentCode.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-black text-heading">{{ transferTarget.student }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[11px] font-bold text-placeholder">{{ transferTarget.studentCode }}</span>
                <span class="w-1 h-1 rounded-full bg-placeholder" />
                <span class="text-[10px] font-black text-link bg-[var(--color-info-bg)] px-1.5 py-0.5 rounded">{{ transferTarget.section }}</span>
              </div>
            </div>
          </div>
          <div>
            <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Chuyển đến lớp</label>
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
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 flex items-center gap-2" :disabled="!transferSection" :class="{ 'opacity-50 cursor-not-allowed': !transferSection }" @click="confirmTransfer">
              <ArrowLeftRight :size="16" /> Chuyển
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
