<script setup>
/**
 * DataExportView.vue - Super Admin
 * Export dữ liệu báo cáo — Form tạo yêu cầu, lịch sử export,
 * lịch export định kỳ. Module M18 Luồng 2.
 */
import { ref, computed, onMounted } from 'vue'
import {
  FileDown, FileSpreadsheet, FileText, Clock, CheckCircle, XCircle,
  Loader2, Plus, RotateCcw, Filter, Download, RefreshCw, Calendar,
  Play, Pause, Trash2, AlertTriangle, Info, Settings
} from 'lucide-vue-next'
import { superAdminApi } from '@/services/superAdminApi'
import { academicTermApi } from '@/services/academicTermApi'
import { organizationApi } from '@/services/organizationService'

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => { showToast.value = false }, 4000)
}

// --- KPI ---
const kpiData = computed(() => {
  const total = exportHistory.value.length
  const completed = exportHistory.value.filter(i => i.status === 'completed').length
  const processing = exportHistory.value.filter(i => i.status === 'processing' || i.status === 'pending').length
  const failed = exportHistory.value.filter(i => i.status === 'failed').length
  return [
    { label: 'Đã xuất thành công', value: completed.toString(), subValue: `Tổng ${total} yêu cầu`, icon: FileDown, color: 'text-blue-500', bg: 'bg-blue-500/10' },
    { label: 'Đang xử lý', value: processing.toString(), icon: Loader2, color: 'text-amber-500', bg: 'bg-amber-500/10' },
    { label: 'Lỗi / Cần thử lại', value: failed.toString(), icon: XCircle, color: 'text-rose-500', bg: 'bg-rose-500/10' },
  ]
})

// --- Export Form ---
const reportTypes = ref([
  { value: 'gradebook', label: 'Bảng điểm toàn kỳ', icon: FileSpreadsheet },
  { value: 'attendance', label: 'Chuyên cần', icon: Calendar },
  { value: 'teacher_eval', label: 'Đánh giá giảng viên', icon: FileText },
  { value: 'finance', label: 'Tài chính', icon: FileDown },
  { value: 'awards', label: 'Khen thưởng & Kỷ luật', icon: FileDown },
])
const semesters = ref([])
const campuses = ref([])

const formReportType = ref('gradebook')
const formSemester = ref('')
const formCampus = ref('')
const formFormat = ref('excel')
const formScheduleEnabled = ref(false)
const formFrequency = ref('weekly')

const handleSubmitExport = async () => {
  const typeName = reportTypes.value.find(t => t.value === formReportType.value)?.label || ''
  
  try {
    const payload = {
      type: formReportType.value,
      semester: formSemester.value,
      campus: formCampus.value,
      format: formFormat.value
    }
    
    const response = await superAdminApi.createExportRequest(payload)
    if (response.success) {
      triggerToast(`Yêu cầu xuất "${typeName}" đã được gửi thành công.`)
      // Auto reload after short delay
      setTimeout(loadHistory, 1000)
    } else {
      triggerToast(response.message || 'Lỗi khi tạo yêu cầu', 'error')
    }
  } catch (err) {
    triggerToast('Đã có lỗi xảy ra', 'error')
  }
}

// --- Export History ---
const exportHistory = ref([])
const isHistoryLoading = ref(false)

const loadHistory = async () => {
  try {
    isHistoryLoading.value = true
    const response = await superAdminApi.getExportHistory()
    if (response.success) {
      exportHistory.value = response.data.map(item => ({
        id: item.maYeuCau,
        type: item.loaiBaoCao,
        typeName: item.tenBaoCao,
        campus: item.capDonVi,
        semester: item.hocKy,
        format: item.dinhDang,
        status: item.trangThai,
        requestedAt: new Date(item.thoiGianYeuCau).toLocaleString('vi-VN'),
        fileUrl: item.trangThai === 'completed' ? superAdminApi.getExportDownloadUrl(item.maYeuCau) : null
      }))
    }
  } catch (err) {
    console.error(err)
  } finally {
    isHistoryLoading.value = false
  }
}

const loadFilters = async () => {
  try {
    // 1. Load academic terms
    try {
      const termRes = await academicTermApi.list({ pageSize: 100 })
      const termList = Array.isArray(termRes) ? termRes : (termRes?.items || termRes?.data || [])
      if (termList.length > 0) {
        semesters.value = termList.map(t => ({
          id: t.maHocKy ?? t.MaHocKy ?? t.id,
          name: t.tenHocKy ?? t.TenHocKy ?? t.name ?? t.maCodeHocKy ?? `Học kỳ ${t.maHocKy}`
        }))
      } else {
        semesters.value = [
          { id: 'Spring 2026', name: 'Spring 2026' },
          { id: 'Fall 2025', name: 'Fall 2025' },
          { id: 'Summer 2025', name: 'Summer 2025' }
        ]
      }
    } catch (e) {
      console.warn('Could not load terms, fallback to defaults:', e)
      semesters.value = [
        { id: 'Spring 2026', name: 'Spring 2026' },
        { id: 'Fall 2025', name: 'Fall 2025' },
        { id: 'Summer 2025', name: 'Summer 2025' }
      ]
    }
    if (semesters.value.length > 0 && !formSemester.value) {
      formSemester.value = semesters.value[0].name
    }

    // 2. Load campuses / organizations
    try {
      const orgRes = await organizationApi.getAll()
      const orgList = Array.isArray(orgRes) ? orgRes : (orgRes?.data || orgRes?.Data || [])
      const campusItems = orgList.filter(o => {
        const level = (o.organizationLevel || o.OrganizationLevel || '').toLowerCase()
        const type = (o.type || o.Type || '').toLowerCase()
        return level === 'campus' || type === 'co_so' || (!level && !type)
      })
      const listToUse = campusItems.length > 0 ? campusItems : orgList
      
      const mapped = listToUse.map(c => ({
        id: c.id ?? c.Id ?? c.maDonVi,
        name: c.name ?? c.Name ?? c.tenDonVi ?? `Cơ sở ${c.id}`
      }))

      campuses.value = [
        { id: 'all', name: 'Toàn hệ thống' },
        ...mapped.filter(m => m.name !== 'Toàn hệ thống')
      ]
    } catch (e) {
      console.warn('Could not load organizations, fallback to defaults:', e)
      campuses.value = [
        { id: 'all', name: 'Toàn hệ thống' },
        { id: 'hn', name: 'Hà Nội' },
        { id: 'hl', name: 'Hòa Lạc' },
        { id: 'hcm', name: 'TP.HCM' },
        { id: 'dn', name: 'Đà Nẵng' },
        { id: 'ct', name: 'Cần Thơ' }
      ]
    }
    if (campuses.value.length > 0 && !formCampus.value) {
      formCampus.value = campuses.value[0].name
    }
  } catch (err) {
    console.error('Lỗi tải bộ lọc', err)
  }
}

// Polling every 5 seconds
let pollInterval
onMounted(() => {
  loadHistory()
  loadFilters()
  pollInterval = setInterval(() => {
    // Only reload if there are pending items
    if (exportHistory.value.some(r => r.status === 'queued' || r.status === 'processing')) {
      loadHistory()
    }
  }, 5000)
})

// --- Scheduled Exports ---
const scheduledExports = ref([
  { id: 'SCH-001', name: 'Báo cáo điểm hàng tháng', type: 'gradebook', frequency: 'Hàng tháng', nextRun: '01/07/2026 02:00', status: 'active' },
  { id: 'SCH-002', name: 'Chuyên cần hàng tuần', type: 'attendance', frequency: 'Hàng tuần', nextRun: '17/06/2026 02:00', status: 'active' },
  { id: 'SCH-003', name: 'Tổng hợp tài chính cuối kỳ', type: 'finance', frequency: 'Hàng tháng', nextRun: '01/07/2026 02:00', status: 'paused' },
])

const toggleSchedule = (sch) => {
  sch.status = sch.status === 'active' ? 'paused' : 'active'
  triggerToast(`Lịch "${sch.name}" đã ${sch.status === 'active' ? 'bật' : 'tạm dừng'}.`, sch.status === 'active' ? 'success' : 'info')
}

const retryExport = async (exp) => {
  try {
    triggerToast(`Đang thử lại xuất "${exp.typeName}"...`)
    const payload = {
      type: exp.type,
      semester: exp.semester,
      campus: exp.campus,
      format: exp.format
    }
    const response = await superAdminApi.createExportRequest(payload)
    if (response.success) {
      triggerToast(`Yêu cầu thử lại đã được gửi vào hàng đợi.`)
      setTimeout(loadHistory, 1000)
    } else {
      triggerToast(response.message || 'Lỗi khi thử lại xuất file', 'error')
    }
  } catch (err) {
    triggerToast('Lỗi khi thử lại xuất file', 'error')
  }
}

const downloadingId = ref(null)
const handleDownload = async (exp) => {
  try {
    downloadingId.value = exp.id
    triggerToast(`Đang chuẩn bị tải "${exp.typeName}"...`, 'info')
    const ext = exp.format === 'pdf' ? 'pdf' : 'xlsx'
    const fileName = `${(exp.typeName || 'BaoCao').replace(/\s+/g, '_')}_${exp.id}.${ext}`
    await superAdminApi.downloadExportFile(exp.id, fileName)
    triggerToast(`Tải file thành công!`, 'success')
  } catch (err) {
    console.error(err)
    triggerToast(err.message || 'Lỗi khi tải file', 'error')
  } finally {
    downloadingId.value = null
  }
}

// --- Helpers ---
const getStatusBadge = (status) => {
  switch (status) {
    case 'queued': return 'bg-slate-500/10 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-500/20'
    case 'processing': return 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border border-amber-200 dark:border-amber-500/20'
    case 'completed': return 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-500/20'
    case 'failed': return 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-200 dark:border-rose-500/20'
    default: return ''
  }
}

const getStatusIcon = (status) => {
  switch (status) {
    case 'queued': return Clock
    case 'processing': return Loader2
    case 'completed': return CheckCircle
    case 'failed': return XCircle
    default: return Clock
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'queued': return 'Đang chờ'
    case 'processing': return 'Đang xử lý'
    case 'completed': return 'Hoàn thành'
    case 'failed': return 'Lỗi'
    default: return status
  }
}

const getFormatIcon = (format) => format === 'excel' ? FileSpreadsheet : FileText
const getFormatLabel = (format) => format === 'excel' ? 'Excel' : 'PDF'
const getFormatColor = (format) => format === 'excel' ? 'text-emerald-500' : 'text-rose-500'
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast -->
    <div
      v-if="showToast"
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-50 dark:bg-emerald-900/30 border-emerald-200 dark:border-emerald-500/30 text-emerald-700 dark:text-emerald-300': toastType === 'success',
        'bg-sky-50 dark:bg-sky-900/30 border-sky-200 dark:border-sky-500/30 text-sky-700 dark:text-sky-300': toastType === 'info',
        'bg-rose-50 dark:bg-rose-900/30 border-rose-200 dark:border-rose-500/30 text-rose-700 dark:text-rose-300': toastType === 'error',
      }"
    >
      <component :is="toastType === 'success' ? CheckCircle : toastType === 'info' ? Info : XCircle" :size="18" />
      <span class="text-sm font-medium">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content space-y-6">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div v-for="kpi in kpiData" :key="kpi.label" class="lg-glass-soft lg-card p-4 flex items-start gap-4">
          <div class="p-2.5 rounded-xl shrink-0" :class="kpi.bg">
            <component :is="kpi.icon" :size="22" :class="kpi.color" />
          </div>
          <div>
            <p class="text-2xl font-bold text-heading leading-tight">{{ kpi.value }}</p>
            <p v-if="kpi.subValue" class="text-xs text-muted">{{ kpi.subValue }}</p>
            <p class="text-xs text-muted mt-0.5">{{ kpi.label }}</p>
          </div>
        </div>
      </div>

      <!-- Export Form -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="Plus" :size="18" class="text-primary" /> Tạo Yêu Cầu Xuất Báo Cáo
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <!-- Report Type -->
          <div>
            <label class="text-xs text-label font-medium mb-1.5 block">Loại báo cáo</label>
            <select v-model="formReportType" class="lg-control text-sm w-full">
              <option v-for="t in reportTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
            </select>
          </div>
          <!-- Semester -->
          <div>
            <label class="text-xs text-label font-medium mb-1.5 block">Học kỳ</label>
            <select v-model="formSemester" class="lg-control text-sm w-full">
              <option v-for="s in semesters" :key="s.id || s.name" :value="s.name">{{ s.name }}</option>
            </select>
          </div>
          <!-- Campus -->
          <div>
            <label class="text-xs text-label font-medium mb-1.5 block">Cơ sở (campus scope)</label>
            <select v-model="formCampus" class="lg-control text-sm w-full">
              <option v-for="c in campuses" :key="c.id || c.name" :value="c.name">{{ c.name }}</option>
            </select>
          </div>
          <!-- Format -->
          <div>
            <label class="text-xs text-label font-medium mb-1.5 block">Định dạng</label>
            <div class="flex gap-3">
              <label class="flex items-center gap-2 cursor-pointer lg-glass-soft rounded-lg px-3 py-2 border transition" :class="formFormat === 'excel' ? 'border-emerald-400 bg-emerald-500/5' : 'border-default'">
                <input type="radio" v-model="formFormat" value="excel" class="hidden" />
                <component :is="FileSpreadsheet" :size="16" class="text-emerald-500" />
                <span class="text-sm font-medium text-heading">Excel</span>
              </label>
              <label class="flex items-center gap-2 cursor-pointer lg-glass-soft rounded-lg px-3 py-2 border transition" :class="formFormat === 'pdf' ? 'border-rose-400 bg-rose-500/5' : 'border-default'">
                <input type="radio" v-model="formFormat" value="pdf" class="hidden" />
                <component :is="FileText" :size="16" class="text-rose-500" />
                <span class="text-sm font-medium text-heading">PDF</span>
              </label>
            </div>
          </div>
          <!-- Schedule Toggle -->
          <div>
            <label class="text-xs text-label font-medium mb-1.5 block">Lên lịch định kỳ</label>
            <div class="flex items-center gap-3">
              <button
                @click="formScheduleEnabled = !formScheduleEnabled"
                class="relative w-11 h-6 rounded-full transition-colors duration-200 focus:outline-none"
                :class="formScheduleEnabled ? 'bg-blue-500' : 'bg-black/10 dark:bg-white/10'"
              >
                <span
                  class="absolute top-0.5 left-0.5 w-5 h-5 rounded-full bg-white shadow transition-transform duration-200"
                  :class="formScheduleEnabled ? 'translate-x-5' : ''"
                ></span>
              </button>
              <select v-if="formScheduleEnabled" v-model="formFrequency" class="lg-control text-sm min-w-[140px]">
                <option value="weekly">Hàng tuần</option>
                <option value="monthly">Hàng tháng</option>
              </select>
              <span v-else class="text-xs text-muted">Tắt</span>
            </div>
          </div>
          <!-- Submit -->
          <div class="flex items-end">
            <button @click="handleSubmitExport" class="lg-btn-primary text-sm flex items-center gap-2 px-5 py-2.5 w-full justify-center">
              <component :is="FileDown" :size="16" /> Yêu cầu xuất
            </button>
          </div>
        </div>
        <!-- Info -->
        <div class="mt-3 flex items-start gap-2 px-3 py-2 rounded-lg bg-sky-500/5 border border-sky-200 dark:border-sky-500/20">
          <component :is="Info" :size="14" class="text-sky-500 shrink-0 mt-0.5" />
          <p class="text-xs text-sky-700 dark:text-sky-300">
            Báo cáo lớn (> 5.000 dòng) sẽ được xử lý bất đồng bộ qua Hangfire. Bạn sẽ nhận thông báo khi file sẵn sàng. Link tải có hiệu lực 10 phút.
          </p>
        </div>
      </div>

      <!-- Export History -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="Clock" :size="18" class="text-primary" /> Lịch Sử Export
        </h3>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">ID</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Loại</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Cơ sở</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Học kỳ</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Định dạng</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Trạng thái</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Thời gian</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default w-[100px]">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="exp in exportHistory" :key="exp.id" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ exp.id }}</td>
                <td class="px-3 py-2.5 text-body text-xs">{{ exp.typeName }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ exp.campus }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ exp.semester }}</td>
                <td class="px-3 py-2.5 text-center">
                  <span class="inline-flex items-center gap-1">
                    <component :is="getFormatIcon(exp.format)" :size="14" :class="getFormatColor(exp.format)" />
                    <span class="text-xs font-medium" :class="getFormatColor(exp.format)">{{ getFormatLabel(exp.format) }}</span>
                  </span>
                </td>
                <td class="px-3 py-2.5 text-center">
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold" :class="getStatusBadge(exp.status)">
                    <component :is="getStatusIcon(exp.status)" :size="11" :class="exp.status === 'processing' ? 'animate-spin' : ''" />
                    {{ getStatusLabel(exp.status) }}
                  </span>
                </td>
                <td class="px-3 py-2.5 text-xs text-muted">{{ exp.requestedAt }}</td>
                <td class="px-3 py-2.5 text-center">
                  <button
                    v-if="exp.status === 'completed'"
                    @click="handleDownload(exp)"
                    :disabled="downloadingId === exp.id"
                    class="flex items-center justify-center gap-1.5 px-3 py-1.5 text-xs font-medium rounded-lg text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-500/10 transition-colors disabled:opacity-50 mx-auto"
                  >
                    <Loader2 v-if="downloadingId === exp.id" :size="14" class="animate-spin" />
                    <Download v-else :size="14" />
                    Tải về
                  </button>
                  <button v-else-if="exp.status === 'failed'" @click="retryExport(exp)" class="text-xs text-amber-500 font-medium inline-flex items-center gap-1 hover:underline">
                    <component :is="RefreshCw" :size="13" /> Thử lại
                  </button>
                  <span v-else class="text-xs text-muted">—</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Scheduled Exports -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="Settings" :size="18" class="text-primary" /> Lịch Export Định Kỳ
        </h3>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Tên</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Loại</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Tần suất</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Lần chạy tiếp</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Trạng thái</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default w-[100px]">Bật/Tắt</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sch in scheduledExports" :key="sch.id" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-semibold text-heading text-xs">{{ sch.name }}</td>
                <td class="px-3 py-2.5 text-body text-xs capitalize">{{ sch.type }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ sch.frequency }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ sch.nextRun }}</td>
                <td class="px-3 py-2.5 text-center">
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold" :class="sch.status === 'active' ? 'bg-emerald-500/10 text-emerald-600 border border-emerald-200 dark:border-emerald-500/20' : 'bg-slate-500/10 text-slate-500 border border-slate-200 dark:border-slate-500/20'">
                    <component :is="sch.status === 'active' ? Play : Pause" :size="11" />
                    {{ sch.status === 'active' ? 'Hoạt động' : 'Tạm dừng' }}
                  </span>
                </td>
                <td class="px-3 py-2.5 text-center">
                  <button
                    @click="toggleSchedule(sch)"
                    class="relative w-9 h-5 rounded-full transition-colors duration-200 focus:outline-none mx-auto"
                    :class="sch.status === 'active' ? 'bg-emerald-500' : 'bg-black/10 dark:bg-white/10'"
                  >
                    <span
                      class="absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white shadow transition-transform duration-200"
                      :class="sch.status === 'active' ? 'translate-x-4' : ''"
                    ></span>
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Audit Notice -->
      <div class="flex items-start gap-2 px-4 py-3 rounded-lg bg-amber-500/5 border border-amber-200 dark:border-amber-500/20">
        <component :is="AlertTriangle" :size="14" class="text-amber-500 shrink-0 mt-0.5" />
        <p class="text-xs text-amber-700 dark:text-amber-300">
          Mọi thao tác export dữ liệu nhạy cảm (điểm, tài chính) đều được ghi Audit Log theo quy định campus scope. File tải về có SAS URL hết hạn sau 10 phút.
        </p>
      </div>
    </div>
  </div>
</template>
