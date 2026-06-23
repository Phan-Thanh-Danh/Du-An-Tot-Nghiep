<script setup>
/**
 * AuditLogsView.vue - Super Admin
 * Giao diện giám sát lịch sử thay đổi dữ liệu nhạy cảm toàn hệ thống.
 * Tích hợp JSON Diff Drawer so sánh dữ liệu cũ/mới, gắn cờ rà soát (Flagged),
 * xuất dữ liệu bảo mật kép và bộ lọc đa chiều theo phân hệ.
 */
import { ref, computed } from 'vue'
import {
  History,
  Shield,
  Filter,
  Eye,
  Flag,
  Download,
  Search,
  Calendar,
  X,
  CheckCircle,
  AlertTriangle,
  Info,
  ShieldAlert,
  Database,
  User,
  RotateCcw
} from 'lucide-vue-next'

// --- Mock Data cho Audit Logs ---
const auditLogsMock = ref([
  {
    id: 'AUD-001',
    entityType: 'StudentGrade',
    entityId: 'GRD-8821',
    action: 'Update', // Create, Update, Lock, Approve, Export
    changedBy: 'giangvien_lam@fpt.edu.vn',
    changedAt: '2026-06-22 10:45:12',
    campusId: 'HN',
    module: 'Grade', // Grade, Finance, RBAC, Attendance
    isFlagged: true,
    reason: 'Sửa điểm nhầm cột điểm thực hành Lab 3 môn Thiết kế phần mềm',
    oldValue: {
      studentId: 'HE170001',
      studentName: 'Nguyễn Văn An',
      courseCode: 'SWE302',
      gradeType: 'Lab_3',
      gradeWeight: 10,
      score: 5.5,
      status: 'Incomplete',
      updatedBy: 'giangvien_lam@fpt.edu.vn'
    },
    newValue: {
      studentId: 'HE170001',
      studentName: 'Nguyễn Văn An',
      courseCode: 'SWE302',
      gradeType: 'Lab_3',
      gradeWeight: 10,
      score: 8.5,
      status: 'Complete',
      updatedBy: 'giangvien_lam@fpt.edu.vn'
    }
  },
  {
    id: 'AUD-002',
    entityType: 'TuitionInvoice',
    entityId: 'INV-40291',
    action: 'Approve',
    changedBy: 'ketoan_lan@fpt.edu.vn',
    changedAt: '2026-06-22 09:30:15',
    campusId: 'HN',
    module: 'Finance',
    isFlagged: false,
    reason: 'Phê duyệt miễn giảm 20% học phí kỳ Spring 2026 diện học bổng',
    oldValue: {
      invoiceId: 'INV-40291',
      studentId: 'HE170092',
      originalAmount: 4800000,
      discountAmount: 0,
      payableAmount: 4800000,
      status: 'Pending',
      approvedBy: null
    },
    newValue: {
      invoiceId: 'INV-40291',
      studentId: 'HE170092',
      originalAmount: 4800000,
      discountAmount: 960000,
      payableAmount: 3840000,
      status: 'Approved',
      approvedBy: 'ketoan_lan@fpt.edu.vn'
    }
  },
  {
    id: 'AUD-003',
    entityType: 'UserRole',
    entityId: 'USR-0092',
    action: 'Update',
    changedBy: 'admin_huan@fpt.edu.vn',
    changedAt: '2026-06-21 16:20:00',
    campusId: 'HCM',
    module: 'RBAC',
    isFlagged: true,
    reason: 'Cấp quyền Khảo thí cho nhân sự phòng Đào tạo',
    oldValue: {
      userId: 'USR-0092',
      userName: 'Trần Văn Tiến',
      role: 'Staff',
      permissions: ['Read_Schedule', 'Read_Students'],
      isActive: true
    },
    newValue: {
      userId: 'USR-0092',
      userName: 'Trần Văn Tiến',
      role: 'Staff',
      permissions: ['Read_Schedule', 'Read_Students', 'Manage_Exams', 'Publish_Grades'],
      isActive: true
    }
  },
  {
    id: 'AUD-004',
    entityType: 'AttendanceRecord',
    entityId: 'ATT-19028',
    action: 'Lock',
    changedBy: 'giaovu_binh@fpt.edu.vn',
    changedAt: '2026-06-21 12:00:00',
    campusId: 'DN',
    module: 'Attendance',
    isFlagged: false,
    reason: 'Đóng băng dữ liệu điểm danh môn Java Web kỳ Spring 2026 block 1',
    oldValue: {
      classCode: 'PRO192_SE1702',
      sessionDate: '2026-06-20',
      totalSlots: 30,
      lockedStatus: 'Open',
      lockedAt: null
    },
    newValue: {
      classCode: 'PRO192_SE1702',
      sessionDate: '2026-06-20',
      totalSlots: 30,
      lockedStatus: 'Locked',
      lockedAt: '2026-06-21 12:00:00'
    }
  },
  {
    id: 'AUD-005',
    entityType: 'StudentGrade',
    entityId: 'GRD-9011',
    action: 'Create',
    changedBy: 'giangvien_mai@fpt.edu.vn',
    changedAt: '2026-06-21 10:15:30',
    campusId: 'HCM',
    module: 'Grade',
    isFlagged: false,
    reason: 'Khởi tạo bảng điểm giữa kỳ môn Cơ sở dữ liệu',
    oldValue: null,
    newValue: {
      courseCode: 'DBI202',
      classCode: 'DBI202_SE1701',
      examiner: 'giangvien_mai@fpt.edu.vn',
      status: 'Draft',
      totalStudents: 32
    }
  }
])

// --- State Bộ lọc ---
const searchUser = ref('')
const selectedCampus = ref('all')
const selectedModule = ref('all') // 'all', 'Grade', 'Finance', 'RBAC', 'Attendance'
const filterStartDate = ref('')
const filterEndDate = ref('')

const campuses = [
  { id: 'all', name: 'Tất cả cơ sở' },
  { id: 'HN', name: 'Campus Hà Nội' },
  { id: 'HCM', name: 'Campus TP. Hồ Chí Minh' },
  { id: 'DN', name: 'Campus Đà Nẵng' }
]

const modules = [
  { code: 'all', name: 'Tất cả Module' },
  { code: 'Grade', name: 'Điểm số (Grades)' },
  { code: 'Finance', name: 'Tài chính (Finance)' },
  { code: 'RBAC', name: 'Phân quyền (RBAC)' },
  { code: 'Attendance', name: 'Điểm danh (Attendance)' }
]

const filteredLogs = computed(() => {
  return auditLogsMock.value.filter(log => {
    // Lọc theo changedBy
    const matchUser = !searchUser.value || log.changedBy.toLowerCase().includes(searchUser.value.toLowerCase())
    
    // Lọc theo Campus
    const matchCampus = selectedCampus.value === 'all' || log.campusId === selectedCampus.value
    
    // Lọc theo Module
    const matchModule = selectedModule.value === 'all' || log.module === selectedModule.value
    
    // Lọc theo Thời gian
    let matchTime = true
    if (filterStartDate.value) {
      matchTime = matchTime && new Date(log.changedAt) >= new Date(filterStartDate.value)
    }
    if (filterEndDate.value) {
      // Đặt cuối ngày cho End Date
      const endDateLimit = new Date(filterEndDate.value)
      endDateLimit.setHours(23, 59, 59, 999)
      matchTime = matchTime && new Date(log.changedAt) <= endDateLimit
    }

    return matchUser && matchCampus && matchModule && matchTime
  })
})

const resetFilters = () => {
  searchUser.value = ''
  selectedCampus.value = 'all'
  selectedModule.value = 'all'
  filterStartDate.value = ''
  filterEndDate.value = ''
}

// --- KPI Metrics ---
const totalLogsCount = computed(() => auditLogsMock.value.length)
const flaggedLogsCount = computed(() => auditLogsMock.value.filter(l => l.isFlagged).length)
const todayLogsCount = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return auditLogsMock.value.filter(l => l.changedAt.startsWith(today)).length
})

// --- State JSON Diff Drawer ---
const isDrawerOpen = ref(false)
const activeLog = ref(null)

const openDiffDrawer = (log) => {
  activeLog.value = log
  isDrawerOpen.value = true
}

// --- State Export Modal ---
const isExportModalOpen = ref(false)
const exportFormat = ref('Excel') // Excel, PDF, CSV
const exportReason = ref('')

// --- Toast States ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success') // 'success' | 'error' | 'info'

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 4000)
}

// Thao tác gắn cờ rà soát
const toggleFlag = (log) => {
  log.isFlagged = !log.isFlagged
  if (log.isFlagged) {
    triggerToast(`Đã gắn cờ rà soát bản ghi ${log.id}`, 'info')
  } else {
    triggerToast(`Đã gỡ cờ rà soát bản ghi ${log.id}`, 'success')
  }
}

// Xuất báo cáo
const handleExport = () => {
  if (!exportReason.value.trim()) {
    triggerToast('Vui lòng nhập lý do xuất dữ liệu để phục vụ kiểm toán!', 'error')
    return
  }

  // Giả lập ghi log cho hành động xuất dữ liệu (Bảo mật kép)
  const timeString = new Date().toLocaleString('vi-VN')
  const exportLog = {
    id: `AUD-0${auditLogsMock.value.length + 1}`,
    entityType: 'AuditLog',
    entityId: 'ALL',
    action: 'Export',
    changedBy: 'super_admin@fpt.edu.vn',
    changedAt: timeString.replace(/\//g, '-'),
    campusId: 'HN',
    module: 'RBAC',
    isFlagged: false,
    reason: `Xuất báo cáo định dạng ${exportFormat.value}. Lý do: ${exportReason.value}`,
    oldValue: { status: 'Normal' },
    newValue: { status: 'Exported', format: exportFormat.value, reason: exportReason.value }
  }

  auditLogsMock.value.unshift(exportLog)
  isExportModalOpen.value = false
  triggerToast(`Xuất dữ liệu audit dạng ${exportFormat.value} thành công!`, 'success')
  exportReason.value = ''
}

// Phân biệt màu sắc thao tác
const getActionBadgeClass = (action) => {
  switch (action) {
    case 'Create': return 'bg-emerald-500/10 text-emerald-500 border-emerald-300'
    case 'Update': return 'bg-amber-500/10 text-amber-500 border-amber-300'
    case 'Lock': return 'bg-rose-500/10 text-rose-500 border-rose-300'
    case 'Approve': return 'bg-sky-500/10 text-sky-500 border-sky-300'
    case 'Export': return 'bg-violet-500/10 text-violet-500 border-violet-300'
    default: return 'bg-slate-500/10 text-slate-500 border-slate-300'
  }
}

// Phân biệt text hiển thị của action
const getActionName = (action) => {
  switch (action) {
    case 'Create': return 'Tạo mới'
    case 'Update': return 'Cập nhật'
    case 'Lock': return 'Khóa'
    case 'Approve': return 'Phê duyệt'
    case 'Export': return 'Xuất dữ liệu'
    default: return action
  }
}

// Rút gọn giá trị hiển thị trên bảng
const getSummaryDiff = (log) => {
  if (log.action === 'Create') {
    return `Khởi tạo thực thể ID: ${log.entityId}`
  }
  if (log.action === 'Export') {
    return log.reason
  }
  if (!log.oldValue || !log.newValue) {
    return 'Thay đổi thông tin thực thể'
  }

  // Phân tích các trường có sự thay đổi để hiển thị nhanh
  const changes = []
  Object.keys(log.newValue).forEach(key => {
    if (JSON.stringify(log.oldValue[key]) !== JSON.stringify(log.newValue[key])) {
      changes.push(`${key}: ${log.oldValue[key] || 'Trống'} → ${log.newValue[key]}`)
    }
  })
  return changes.length > 0 ? changes.join(', ') : 'Cập nhật thông tin thực thể'
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <History class="w-8 h-8 text-primary" />
            Nhật Ký Hệ Thống (Audit Log)
          </h1>
          <p class="text-sm text-muted mt-1">
            Theo dõi vết lịch sử biến động của các thực thể nhạy cảm (Điểm số, Tài chính, Phân quyền, Điểm danh) hỗ trợ rà soát kiểm toán bảo mật.
          </p>
        </div>

        <!-- Export Action Button -->
        <div>
          <button
            @click="isExportModalOpen = true"
            class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2"
          >
            <Download class="w-4.5 h-4.5" />
            Xuất Báo Cáo Audit
          </button>
        </div>
      </div>

      <!-- Security Status & Roles Banner -->
      <div class="lg-alert lg-alert-info mb-6 flex items-start justify-between gap-4">
        <div class="flex items-start gap-2.5">
          <Shield class="w-5.5 h-5.5 text-sky-500 flex-shrink-0 mt-0.5" />
          <div>
            <h4 class="font-extrabold text-sm text-sky-700 dark:text-sky-400">Thông tin Phân quyền Giám sát</h4>
            <p class="text-xs leading-relaxed text-slate-700 dark:text-slate-300 mt-1">
              Bạn đang đăng nhập với quyền **Super Admin (Full Access)**. Hệ thống hiển thị nhật ký toàn bộ hệ thống đa cơ sở (Campus Hà Nội, Campus TP. HCM, Campus Đà Nẵng). Mọi hành động xem vết và xuất file của bạn đều được ghi lại để phục vụ bảo mật kép.
            </p>
          </div>
        </div>
        <span class="text-[10px] font-extrabold text-sky-600 dark:text-sky-400 bg-sky-500/10 px-2.5 py-1 rounded border border-sky-300/40 whitespace-nowrap">
          Quyền: FULL_ACCESS
        </span>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Database class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tổng số bản ghi</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ totalLogsCount }} logs</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <Flag class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Gắn cờ rà soát (Flagged)</div>
            <div class="text-xl font-bold mt-0.5 text-heading text-rose-500 font-extrabold">{{ flaggedLogsCount }} logs</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <CheckCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Biến động hôm nay</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ todayLogsCount }} bản ghi</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <ShieldAlert class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Trạng thái giám sát</div>
            <div class="text-xl font-bold mt-0.5 text-heading">HEALTH_OK</div>
          </div>
        </div>
      </div>

      <!-- Entity Tabs Phân loại nhanh (Module Tabs) -->
      <div class="flex gap-2 border-b border-default mb-6 pb-2 text-xs font-bold overflow-x-auto whitespace-nowrap">
        <button
          v-for="mod in modules"
          :key="mod.code"
          @click="selectedModule = mod.code"
          class="px-4 py-2 border-b-2 transition-colors flex items-center gap-1.5"
          :class="selectedModule === mod.code ? 'border-primary text-primary font-extrabold' : 'border-transparent text-muted hover:text-heading'"
        >
          {{ mod.name }}
        </button>
      </div>

      <!-- Khung Bộ Lọc & Tìm Kiếm Nâng Cao -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Tìm kiếm & Lọc log nâng cao</h3>
          </div>
          <button 
            @click="resetFilters" 
            class="text-xs text-link font-bold flex items-center gap-1 hover:underline"
          >
            <RotateCcw class="w-3.5 h-3.5" />
            Reset bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <!-- Tìm kiếm theo Người dùng -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Người thực hiện (changed_by)</label>
            <div class="relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input 
                type="text" 
                v-model="searchUser" 
                placeholder="Nhập email nhân viên..."
                class="w-full pl-9 pr-3 lg-control text-sm"
              />
            </div>
          </div>

          <!-- Lọc Campus -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Chọn Cơ sở (Campus)</label>
            <select v-model="selectedCampus" class="w-full px-3 lg-control text-sm">
              <option v-for="c in campuses" :key="c.id" :value="c.id">{{ c.name }}</option>
            </select>
          </div>

          <!-- Từ ngày -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Từ ngày</label>
            <div class="relative">
              <Calendar class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input type="date" v-model="filterStartDate" class="w-full pl-9 pr-3 lg-control text-xs" />
            </div>
          </div>

          <!-- Đến ngày -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Đến ngày</label>
            <div class="relative">
              <Calendar class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input type="date" v-model="filterEndDate" class="w-full pl-9 pr-3 lg-control text-xs" />
            </div>
          </div>
        </div>
      </div>

      <!-- Bảng Audit Table -->
      <div class="lg-table-shell overflow-x-auto w-full max-w-full mb-8">
        <table class="min-w-[1100px] w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase w-10 whitespace-nowrap">Cờ</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Thời gian</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Người thực hiện</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap">Cơ sở</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Thực thể (ID)</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap">Hành động</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Nội dung tóm tắt</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap min-w-[140px] w-[140px]">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredLogs.length === 0">
              <td colspan="8" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <Database class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy bản ghi Audit Log nào.</span>
                </div>
              </td>
            </tr>

            <tr v-for="log in filteredLogs" :key="log.id" class="transition-colors hover:bg-surface-card-hover/20">
              <!-- Cờ rà soát -->
              <td class="px-4 py-4 text-center">
                <button 
                  @click="toggleFlag(log)"
                  class="p-1 rounded-full transition-all duration-150"
                  :class="log.isFlagged ? 'text-rose-500 hover:text-rose-600 scale-110' : 'text-slate-400 hover:text-rose-400'"
                  :title="log.isFlagged ? 'Đang gắn cờ rà soát - Click để gỡ' : 'Click để gắn cờ rà soát'"
                >
                  <Flag class="w-4.5 h-4.5" :class="log.isFlagged ? 'fill-current' : ''" />
                </button>
              </td>

              <!-- Thời gian -->
              <td class="px-4 py-4 text-xs font-semibold text-muted whitespace-nowrap">
                {{ log.changedAt }}
              </td>

              <!-- Người thực hiện -->
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <span class="w-7 h-7 rounded-full bg-slate-500/10 flex items-center justify-center border border-default text-primary flex-shrink-0">
                    <User class="w-3.5 h-3.5" />
                  </span>
                  <div class="text-xs font-extrabold text-heading truncate max-w-[180px]">{{ log.changedBy }}</div>
                </div>
              </td>

              <!-- Campus -->
              <td class="px-4 py-4 text-center">
                <span class="px-2 py-0.5 rounded bg-surface-input border border-default text-[10px] font-bold">
                  {{ log.campusId }}
                </span>
              </td>

              <!-- Thực thể (Entity) -->
              <td class="px-4 py-4 whitespace-nowrap">
                <div class="font-extrabold text-heading text-xs">{{ log.entityType }}</div>
                <div class="text-[10px] text-muted mt-0.5">ID: {{ log.entityId }}</div>
              </td>

              <!-- Hành động -->
              <td class="px-4 py-4 text-center">
                <span 
                  class="px-2.5 py-0.5 rounded-full border text-[10px] font-extrabold tracking-wider"
                  :class="getActionBadgeClass(log.action)"
                >
                  {{ getActionName(log.action) }}
                </span>
              </td>

              <!-- Nội dung tóm tắt -->
              <td class="px-4 py-4 max-w-xs">
                <p class="text-xs font-semibold text-muted leading-relaxed truncate" :title="getSummaryDiff(log)">
                  {{ getSummaryDiff(log) }}
                </p>
                <p v-if="log.reason" class="text-[10px] text-rose-500/85 mt-0.5 italic leading-normal truncate" :title="log.reason">
                  Lý do: {{ log.reason }}
                </p>
              </td>

              <!-- Action Xem chi tiết -->
              <td class="px-4 py-4 text-center whitespace-nowrap min-w-[140px] w-[140px]">
                <button
                  @click="openDiffDrawer(log)"
                  class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1.5 mx-auto"
                >
                  <Eye class="w-3.5 h-3.5" />
                  Xem JSON Diff
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </div>

    <!-- JSON Diff Drawer (Drawer so sánh JSON Cũ/Mới) -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[100] flex justify-end bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-4xl h-screen bg-surface-modal lg-glass-strong border-l border-default shadow-2xl flex flex-col animate-in slide-in-from-right duration-300">
        
        <!-- Drawer Header -->
        <div class="p-5 border-b border-default flex items-center justify-between">
          <div class="space-y-1">
            <h2 class="text-lg font-extrabold text-heading flex items-center gap-2.5">
              <History class="w-5.5 h-5.5 text-primary" />
              So Sánh Biến Động Dữ Liệu (JSON Diff)
            </h2>
            <p class="text-xs text-muted">So sánh trực quan giá trị thực thể trước và sau khi thay đổi dữ liệu.</p>
          </div>

          <button 
            @click="isDrawerOpen = false"
            class="lg-icon-button p-2 bg-surface-card rounded-lg border border-default text-muted hover:text-heading"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Drawer Body -->
        <div class="flex-1 overflow-y-auto p-5 space-y-4">
          <!-- Metadata Summary card -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3 p-4 rounded-xl bg-surface-card border border-default text-xs leading-relaxed">
            <div class="space-y-1">
              <div><span class="font-bold text-muted mr-1">Bản ghi:</span> <span class="font-bold text-heading">{{ activeLog?.id }}</span></div>
              <div><span class="font-bold text-muted mr-1">Thời gian:</span> <span class="font-bold text-heading">{{ activeLog?.changedAt }}</span></div>
            </div>
            <div class="space-y-1">
              <div><span class="font-bold text-muted mr-1">Người sửa:</span> <span class="font-bold text-primary">{{ activeLog?.changedBy }}</span></div>
              <div><span class="font-bold text-muted mr-1">Cơ sở:</span> <span class="font-bold text-heading">{{ activeLog?.campusId }}</span></div>
            </div>
            <div class="space-y-1">
              <div><span class="font-bold text-muted mr-1">Thực thể:</span> <span class="font-bold text-heading">{{ activeLog?.entityType }} (ID: {{ activeLog?.entityId }})</span></div>
              <div><span class="font-bold text-muted mr-1">Hành động:</span> <span class="font-bold text-heading">{{ getActionName(activeLog?.action) }}</span></div>
            </div>
            <div class="col-span-1 sm:col-span-3 pt-2 border-t border-default/50 text-rose-500 font-semibold italic" v-if="activeLog?.reason">
              Lý do thay đổi: {{ activeLog?.reason }}
            </div>
          </div>

          <!-- JSON Diff Blocks Grid -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 h-[calc(100%-120px)] min-h-[300px]">
            <!-- Old Value (Cột trái - Màu đỏ nhạt) -->
            <div class="flex flex-col h-full">
              <div class="bg-rose-500/10 rounded-t-xl px-4 py-2 border border-rose-300/30 flex items-center justify-between text-xs text-rose-600 dark:text-rose-400 font-bold">
                <span>DỮ LIỆU CŨ (PREVIOUS VALUE)</span>
                <span v-if="!activeLog?.oldValue" class="text-[9px] bg-rose-500/10 px-1.5 py-0.5 rounded uppercase">Mới tạo</span>
              </div>
              <div class="flex-1 bg-rose-500/5 dark:bg-rose-950/10 p-4 border border-rose-300/20 border-t-0 rounded-b-xl overflow-auto font-mono text-[11px] leading-relaxed text-rose-700 dark:text-rose-400 max-h-[450px]">
                <pre v-if="activeLog?.oldValue">{{ JSON.stringify(activeLog.oldValue, null, 2) }}</pre>
                <div v-else class="text-muted italic flex items-center justify-center h-full">
                  Không có dữ liệu (Thực thể được tạo mới)
                </div>
              </div>
            </div>

            <!-- New Value (Cột phải - Màu xanh lá nhạt) -->
            <div class="flex flex-col h-full">
              <div class="bg-emerald-500/10 rounded-t-xl px-4 py-2 border border-emerald-300/30 flex items-center justify-between text-xs text-emerald-600 dark:text-emerald-400 font-bold">
                <span>DỮ LIỆU MỚI (NEW VALUE)</span>
              </div>
              <div class="flex-1 bg-emerald-500/5 dark:bg-emerald-950/10 p-4 border border-emerald-300/20 border-t-0 rounded-b-xl overflow-auto font-mono text-[11px] leading-relaxed text-emerald-700 dark:text-emerald-400 max-h-[450px]">
                <pre v-if="activeLog?.newValue">{{ JSON.stringify(activeLog.newValue, null, 2) }}</pre>
                <div v-else class="text-muted italic flex items-center justify-center h-full">
                  Không có dữ liệu
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Drawer Footer -->
        <div class="p-5 border-t border-default flex items-center justify-end bg-surface-table">
          <button
            @click="isDrawerOpen = false"
            class="lg-btn-primary px-5 py-2 text-xs font-bold"
          >
            Đóng bảng so sánh
          </button>
        </div>

      </div>
    </div>

    <!-- Export Confirm Modal (Xác nhận xuất báo cáo) -->
    <div 
      v-if="isExportModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <!-- Close button -->
        <button 
          @click="isExportModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
          <div class="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
            <Download class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Xác Nhận Xuất Báo Cáo Audit</h3>
            <p class="text-xs text-muted mt-0.5">Yêu cầu xác nhận kiểm toán trước khi trích xuất dữ liệu nhạy cảm ra khỏi hệ thống.</p>
          </div>
        </div>

        <!-- Nội dung Form xác nhận -->
        <div class="space-y-4 mb-5">
          <div class="p-3.5 rounded-lg bg-rose-500/5 border border-rose-500/10 text-xs flex items-start gap-2 text-rose-600 dark:text-rose-400">
            <ShieldAlert class="w-4.5 h-4.5 flex-shrink-0 mt-0.5" />
            <p class="leading-relaxed">
              **Lưu ý bảo mật**: Hoạt động xuất audit log sẽ tự động được hệ thống ghi vết bảo mật kép thành một bản ghi log mới. Vui lòng ghi rõ lý do kiểm toán.
            </p>
          </div>

          <!-- Chọn định dạng -->
          <div>
            <label class="block text-xs font-bold text-label mb-2 uppercase">Định dạng xuất báo cáo</label>
            <div class="flex gap-4 text-xs font-bold">
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="radio" value="Excel" v-model="exportFormat" class="text-primary focus:ring-primary" />
                Excel (.xlsx)
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="radio" value="PDF" v-model="exportFormat" class="text-primary focus:ring-primary" />
                PDF Document
              </label>
              <label class="flex items-center gap-2 cursor-pointer">
                <input type="radio" value="CSV" v-model="exportFormat" class="text-primary focus:ring-primary" />
                CSV Thô
              </label>
            </div>
          </div>

          <!-- Lý do bắt buộc -->
          <div>
            <label class="block text-xs font-bold text-label mb-2 uppercase">Lý do xuất dữ liệu (Bắt buộc)</label>
            <textarea
              v-model="exportReason"
              rows="3"
              placeholder="Nhập lý do chi tiết phục vụ lưu vết audit log..."
              class="w-full px-3 py-2 lg-control text-xs leading-relaxed"
            ></textarea>
            <span v-if="!exportReason.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Bắt buộc nhập lý do phục vụ lưu vết</span>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="isExportModalOpen = false"
            class="lg-btn-secondary px-4 py-2 text-sm font-bold"
          >
            Hủy
          </button>
          <button
            @click="handleExport"
            :disabled="!exportReason.trim()"
            class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Download class="w-4 h-4" />
            Xác nhận xuất file
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
pre {
  margin: 0;
  white-space: pre-wrap;
  word-break: break-all;
}
input[type="radio"], input[type="checkbox"] {
  width: 1rem;
  height: 1rem;
  border: 1px solid var(--border-input);
  background-color: var(--surface-input);
}
</style>
