<script setup>
import { ref, computed } from 'vue'
import { 
  Search, 
  Download, 
  CheckCircle2, 
  XCircle, 
  Eye, 
  FileCheck,
  Calendar,
  ArrowUpRight,
  X,
  MoreVertical,
  Loader2,
  FileSpreadsheet,
  FileText
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

// ── Mock Data ────────────────────────────────────────────────
const history = ref([
  { id: 'DON-998', student: 'Vũ Thị E', type: 'Cấp giấy xác nhận', result: 'approved', processedBy: 'Hệ thống tự động', date: '05/05/2026', note: 'Đã sinh PDF và gửi email cho SV.' },
  { id: 'DON-995', student: 'Phạm Văn F', type: 'Thi lại', result: 'rejected', processedBy: 'Nguyễn Bích L', date: '04/05/2026', note: 'Không đủ điều kiện dự thi (vắng quá 20%).' },
  { id: 'DON-990', student: 'Lê Hoàng G', type: 'Chuyển lớp', result: 'approved', processedBy: 'Phạm Minh D', date: '01/05/2026', note: 'Đã cập nhật enrollment sang lớp L04.' },
])

const getResultBadge = (result) => {
  switch (result) {
    case 'approved': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
    case 'rejected': return 'bg-rose-50 text-rose-600 border-rose-100'
    default: return 'bg-slate-50 text-slate-500 border-slate-100'
  }
}

const today = computed(() => new Date().toISOString().split('T')[0])

// ── Search ───────────────────────────────────────────────────
const searchQuery = ref('')
const searchTriggered = ref('')

function doSearch() {
  searchTriggered.value = searchQuery.value
}

function clearSearch() {
  searchQuery.value = ''
  searchTriggered.value = ''
}

// ── Date Filter ──────────────────────────────────────────────
const showDatePicker = ref(false)
const startDate = ref('')
const endDate = ref('')

function openDatePicker() {
  showDatePicker.value = true
}

function applyDateFilter() {
  showDatePicker.value = false
}

function clearDateFilter() {
  startDate.value = ''
  endDate.value = ''
}

function closeDatePicker() {
  showDatePicker.value = false
}

const hasDateFilter = computed(() => startDate.value || endDate.value)

// ── Filtered Data ────────────────────────────────────────────
const filteredHistory = computed(() => {
  let result = history.value
  const q = searchTriggered.value.toLowerCase().trim()
  if (q) {
    result = result.filter(h =>
      h.id.toLowerCase().includes(q) ||
      h.student.toLowerCase().includes(q)
    )
  }
  if (hasDateFilter.value) {
    result = result.filter(h => {
      const [d, m, y] = h.date.split('/')
      const hDate = new Date(+y, +m - 1, +d)
      if (startDate.value) {
        const [sy, sm, sd] = startDate.value.split('-')
        if (hDate < new Date(+sy, +sm - 1, +sd)) return false
      }
      if (endDate.value) {
        const [ey, em, ed] = endDate.value.split('-')
        if (hDate > new Date(+ey, +em - 1, +ed)) return false
      }
      return true
    })
  }
  return result
})

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

function getResultLabel(result) {
  return result === 'approved' ? 'Phê duyệt' : 'Từ chối'
}

function doExport() {
  exporting.value = true
  const data = filteredHistory.value.map(h => ({
    'Mã đơn': h.id,
    'Sinh viên': h.student,
    'Loại đơn': h.type,
    'Kết quả': getResultLabel(h.result),
    'Người xử lý': h.processedBy,
    'Ngày xử lý': h.date,
    'Ghi chú': h.note,
  }))
  const headers = Object.keys(data[0] || {})
  if (exportFormat.value === 'csv') {
    const csv = [headers.join(','), ...data.map(r => headers.map(h => `"${r[h]}"`).join(','))].join('\n')
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `lich-su-don-tu-${Date.now()}.csv`; a.click()
    URL.revokeObjectURL(url)
  } else {
    let xml = '<?xml version="1.0" encoding="UTF-8"?><?mso-application progid="Excel.Sheet"?><Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">'
    xml += '<Styles><Style ss:ID="h"><Font ss:Bold="1" ss:Size="11"/></Style></Styles><Worksheet ss:Name="Sheet1"><Table>'
    xml += '<Row>' + headers.map(h => `<Cell ss:StyleID="h"><Data ss:Type="String">${h}</Data></Cell>`).join('') + '</Row>'
    data.forEach(r => {
      xml += '<Row>' + headers.map(h => `<Cell><Data ss:Type="String">${r[h]}</Data></Cell>`).join('') + '</Row>'
    })
    xml += '</Table></Worksheet></Workbook>'
    const blob = new Blob([xml], { type: 'application/vnd.ms-excel' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url; a.download = `lich-su-don-tu-${Date.now()}.xls`; a.click()
    URL.revokeObjectURL(url)
  }
  setTimeout(() => {
    exporting.value = false
    exportDone.value = true
    popupStore.success('Xuất báo cáo', `Đã tải xuống ${filteredHistory.value.length} đơn (${exportFormat.value === 'excel' ? 'Excel' : 'CSV'}). Kiểm tra thư mục Downloads.`)
    setTimeout(() => { showExportModal.value = false }, 1000)
  }, 400)
}

// ── Detail Modal ─────────────────────────────────────────────
const showDetailModal = ref(false)
const detailTarget = ref(null)

function openDetailModal(h) {
  detailTarget.value = h
  showDetailModal.value = true
  closeContextMenu()
}

function closeDetailModal() {
  showDetailModal.value = false
  detailTarget.value = null
}

// ── Context Menu ─────────────────────────────────────────────
const contextTarget = ref(null)

function toggleContextMenu(h) {
  contextTarget.value = contextTarget.value?.id === h.id ? null : h
}

function closeContextMenu() {
  contextTarget.value = null
}


</script>

<template>
  <PageContainer 
    title="Lịch sử xử lý đơn từ" 
    subtitle="Tra cứu và kiểm tra lại kết quả các đơn từ đã được giải quyết."
  >
    <div class="space-y-4">
      
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1 flex-wrap">
           <div class="relative max-w-sm w-full flex-1 min-w-[240px]">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
               <input 
                 v-model="searchQuery"
                 type="text" 
                 placeholder="Mã đơn hoặc tên sinh viên..." 
                 class="w-full lg-input pl-11 pr-4 py-2.5 text-sm font-medium"
                 @keyup.enter="doSearch"
               >
           </div>
          <div class="flex items-center gap-2">
            <button class="lg-button-primary px-4 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20" @click="doSearch">
              <Search :size="16" /> Tìm
            </button>
            <button v-if="searchTriggered" class="lg-button-secondary px-3 py-2.5 text-sm font-bold" @click="clearSearch" title="Xóa tìm kiếm">
              <X :size="16" />
            </button>
            <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2 relative" @click="openDatePicker">
              <Calendar :size="18" /> Chọn thời gian
              <span v-if="hasDateFilter" class="h-2 w-2 rounded-full bg-[var(--lg-primary)] absolute top-2 right-2" />
            </button>
          </div>
        </div>
        <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold" @click="openExportModal">
          <Download :size="18" /> Xuất báo cáo
        </button>
      </div>

      <!-- ── History Table ── -->
      <div class="lg-table-shell overflow-hidden" @click="closeContextMenu">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Mã đơn</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Sinh viên & Loại đơn</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Kết quả</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Người xử lý</th>
               <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="h in filteredHistory" :key="h.id" class="group hover:bg-white/10 transition-colors">
              <td class="px-4 py-4">
                <span class="text-xs font-black text-label uppercase tracking-tighter">{{ h.id }}</span>
              </td>
              <td class="px-4 py-4">
                <div>
                  <p class="text-sm font-black text-heading leading-tight">{{ h.type }}</p>
                  <p class="text-[11px] font-bold text-label mt-0.5">{{ h.student }} • {{ h.date }}</p>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                   <div :class="['h-6 w-6 rounded-full flex items-center justify-center border', getResultBadge(h.result)]">
                      <CheckCircle2 v-if="h.result === 'approved'" :size="14" />
                      <XCircle v-else :size="14" />
                   </div>
                    <span :class="['text-[10px] font-black uppercase tracking-widest', h.result === 'approved' ? 'text-success' : 'text-danger']">
                      {{ h.result === 'approved' ? 'Phê duyệt' : 'Từ chối' }}
                   </span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                    <div class="h-6 w-6 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                      <ArrowUpRight :size="12" />
                   </div>
                    <span class="text-xs font-bold text-label">{{ h.processedBy }}</span>
                </div>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button class="p-2 lg-button-ghost rounded-lg" title="Xem kết quả" @click.stop="openDetailModal(h)">
                    <Eye :size="18" />
                  </button>
                  <div class="relative">
                    <button class="p-2 lg-button-ghost rounded-lg" @click.stop="toggleContextMenu(h)">
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
                      <div v-if="contextTarget?.id === h.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-xl shadow-slate-900/10" @click.stop>
                        <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-bold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="openDetailModal(h); closeContextMenu()">
                          <Eye :size="14" /> Xem chi tiết
                        </button>
                      </div>
                    </Transition>
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredHistory.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-16 w-16 rounded-2xl surface-solid flex items-center justify-center mb-4">
            <FileCheck :size="28" class="text-placeholder" />
          </div>
          <p class="text-sm font-black text-heading">Không có đơn từ nào</p>
          <p class="text-xs font-medium text-placeholder mt-1">Thử thay đổi từ khóa tìm kiếm hoặc thời gian</p>
        </div>
      </div>

    </div>

    <!-- ═══════════════════════════════════════════════════════
         DATE PICKER MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showDatePicker" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDatePicker">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-sm lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Calendar :size="18" />
              </div>
              <h3 class="text-base font-black text-heading">Chọn thời gian</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeDatePicker">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Từ ngày</label>
               <input type="date" v-model="startDate" :min="today" :max="endDate || undefined" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-black text-label uppercase tracking-widest mb-1.5 block">Đến ngày</label>
              <input type="date" v-model="endDate" :min="startDate || today" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
          </div>
          <div class="flex items-center justify-between gap-3 mt-6 pt-4 border-t border-default">
            <button v-if="hasDateFilter" class="text-[11px] font-bold text-placeholder hover:text-label transition-colors flex items-center gap-1" @click="clearDateFilter">
              <X :size="13" /> Xóa bộ lọc
            </button>
            <div class="flex items-center gap-3 ml-auto">
              <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDatePicker">Hủy</button>
              <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20" @click="applyDateFilter">Áp dụng</button>
            </div>
          </div>
        </div>
      </div>
    </Transition>

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
            <h3 class="text-base font-black text-heading">Xuất báo cáo</h3>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="showExportModal = false">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-3">
            <p class="text-[12px] font-medium text-label mb-2">Chọn định dạng xuất:</p>
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
         DETAIL MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showDetailModal && detailTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDetailModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-lg lg-glass-strong rounded-[28px] p-6 shadow-2xl">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Eye :size="18" />
              </div>
              <h3 class="text-base font-black text-heading">Chi tiết đơn từ</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeDetailModal">
              <X :size="18" />
            </button>
          </div>

          <div class="space-y-4">
            <div class="surface-solid p-4 rounded-2xl">
              <div class="flex items-center justify-between mb-3">
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest">Mã đơn</span>
                <span class="text-xs font-black text-label">{{ detailTarget.id }}</span>
              </div>
              <div class="flex items-center justify-between mb-3">
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest">Sinh viên</span>
                <span class="text-sm font-black text-heading">{{ detailTarget.student }}</span>
              </div>
              <div class="flex items-center justify-between mb-3">
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest">Loại đơn</span>
                <span class="text-sm font-bold text-label">{{ detailTarget.type }}</span>
              </div>
              <div class="flex items-center justify-between">
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest">Ngày xử lý</span>
                <span class="text-xs font-bold text-label">{{ detailTarget.date }}</span>
              </div>
            </div>

            <div class="surface-solid p-4 rounded-2xl space-y-3">
              <div>
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest block mb-1">Kết quả</span>
                <div class="flex items-center gap-2">
                  <div :class="['h-8 w-8 rounded-full flex items-center justify-center border', getResultBadge(detailTarget.result)]">
                    <CheckCircle2 v-if="detailTarget.result === 'approved'" :size="18" />
                    <XCircle v-else :size="18" />
                  </div>
                  <span :class="['text-sm font-black', detailTarget.result === 'approved' ? 'text-[var(--lg-success)]' : 'text-[var(--lg-danger)]']">
                    {{ detailTarget.result === 'approved' ? 'Phê duyệt' : 'Từ chối' }}
                  </span>
                </div>
              </div>
              <div>
                <span class="text-[10px] font-black text-placeholder uppercase tracking-widest block mb-1">Người xử lý</span>
                <div class="flex items-center gap-2">
                  <div class="h-7 w-7 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                    <ArrowUpRight :size="14" />
                  </div>
                  <span class="text-sm font-bold text-label">{{ detailTarget.processedBy }}</span>
                </div>
              </div>
            </div>

            <div class="surface-solid p-4 rounded-2xl">
              <span class="text-[10px] font-black text-placeholder uppercase tracking-widest block mb-2">Ghi chú</span>
              <p class="text-sm font-medium text-label leading-relaxed">{{ detailTarget.note }}</p>
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDetailModal">Đóng</button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
