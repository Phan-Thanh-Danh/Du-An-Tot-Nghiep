<script setup>
/**
 * SecurityAlertsView.vue - Super Admin
 * Giao diện giám sát & xử lý cảnh báo bảo mật được hệ thống phát hiện tự động.
 * Tích hợp AI Risk Score, Alert Detail Drawer, Confirm Lock Modal và Workflow cập nhật trạng thái.
 */
import { ref, computed } from 'vue'
import {
  Shield,
  ShieldAlert,
  AlertTriangle,
  Search,
  Filter,
  Calendar,
  RotateCcw,
  Eye,
  Lock,
  Unlock,
  CheckCircle,
  X,
  Activity,
  User,
  Cpu,
  Globe,
  Database
} from 'lucide-vue-next'

// --- Mock Data cho Security Alerts ---
const alertsMock = ref([
  {
    id: 'SEC-001',
    userEmail: 'giangvien_lam@fpt.edu.vn',
    userRole: 'Teacher',
    avatar: '',
    campusId: 'HN',
    alertType: 'IMPOSSIBLE_TRAVEL', // IMPOSSIBLE_TRAVEL, ANOMALOUS_IP, SUSPICIOUS_DEVICE, BRUTE_FORCE
    riskScore: 0.88, // AI calculated risk score
    ipAddress: '103.21.140.22',
    deviceDetail: 'Chrome 125, MacOS 14.5',
    location: 'Singapore (SG)',
    detectedAt: '2026-06-22 12:45:00',
    status: 'Open', // Open, Investigating, Resolved
    isUserLocked: false,
    aiExplanation: 'Phát hiện đăng nhập bất thường cách xa địa điểm đăng nhập trước đó 1,200 km (Hà Nội, VN) chỉ sau 15 phút. Hệ thống nghi ngờ tài khoản bị chiếm đoạt session token.',
    historyLogs: [
      { action: 'Detected', time: '2026-06-22 12:45:00', actor: 'AI (Isolation Forest)', note: 'Điểm rủi ro vượt ngưỡng 0.70 (0.88). Tự động tạo cảnh báo và gửi email thông báo cho Super Admin.' }
    ]
  },
  {
    id: 'SEC-002',
    userEmail: 'sinhvien_namhe17@fpt.edu.vn',
    userRole: 'Student',
    avatar: '',
    campusId: 'HCM',
    alertType: 'BRUTE_FORCE',
    riskScore: 0.72,
    ipAddress: '171.244.10.89',
    deviceDetail: 'Firefox 126, Windows 11',
    location: 'TP. Hồ Chí Minh (VN)',
    detectedAt: '2026-06-22 11:30:12',
    status: 'Investigating',
    isUserLocked: false,
    aiExplanation: 'Phát hiện 8 lần thử đăng nhập thất bại liên tiếp trong vòng 45 giây trước khi đăng nhập thành công. Đây là hành vi thử mật khẩu phổ biến.',
    historyLogs: [
      { action: 'Detected', time: '2026-06-22 11:30:12', actor: 'AI (Isolation Forest)', note: 'Điểm rủi ro đạt 0.72. Tự động khởi tạo cảnh báo rủi ro.' },
      { action: 'Status_Updated', time: '2026-06-22 11:45:00', actor: 'super_admin@fpt.edu.vn', note: 'Chuyển sang trạng thái Investigating. Đang liên hệ sinh viên xác minh có quên mật khẩu không.' }
    ]
  },
  {
    id: 'SEC-003',
    userEmail: 'giaovu_binh@fpt.edu.vn',
    userRole: 'Staff',
    avatar: '',
    campusId: 'DN',
    alertType: 'ANOMALOUS_IP',
    riskScore: 0.94,
    ipAddress: '198.51.100.4',
    deviceDetail: 'Python-requests/2.31',
    location: 'California (US)',
    detectedAt: '2026-06-21 16:20:00',
    status: 'Open',
    isUserLocked: true,
    aiExplanation: 'Truy cập API Endpoint với User-Agent không phải trình duyệt (Python script) từ địa chỉ IP thuộc nhà mạng đám mây (AWS). Điểm rủi ro cực kỳ cao vì đây là tài khoản Giáo vụ nắm giữ dữ liệu điểm danh môn học.',
    historyLogs: [
      { action: 'Detected', time: '2026-06-21 16:20:00', actor: 'AI (Isolation Forest)', note: 'Điểm rủi ro đạt 0.94 do IP hosting và User-Agent lạ.' },
      { action: 'Account_Locked', time: '2026-06-21 16:30:00', actor: 'super_admin@fpt.edu.vn', note: 'Khóa tài khoản khẩn cấp đề phòng bị dò quét hoặc rò rỉ API key.' }
    ]
  },
  {
    id: 'SEC-004',
    userEmail: 'ketoan_lan@fpt.edu.vn',
    userRole: 'Staff',
    avatar: '',
    campusId: 'HN',
    alertType: 'SUSPICIOUS_DEVICE',
    riskScore: 0.35,
    ipAddress: '14.162.245.10',
    deviceDetail: 'Safari 17.4, iPadOS 17.4',
    location: 'Hà Nội (VN)',
    detectedAt: '2026-06-21 10:15:30',
    status: 'Resolved',
    isUserLocked: false,
    aiExplanation: 'Đăng nhập từ thiết bị iPad mới chưa từng được ghi nhận trong lịch sử đăng nhập của nhân sự Kế toán này. Điểm rủi ro thấp (0.35) vì IP nằm trong dải quen thuộc của văn phòng.',
    historyLogs: [
      { action: 'Detected', time: '2026-06-21 10:15:30', actor: 'AI (Isolation Forest)', note: 'Thiết bị mới phát sinh. Điểm rủi ro 0.35.' },
      { action: 'Status_Updated', time: '2026-06-21 14:00:00', actor: 'super_admin@fpt.edu.vn', note: 'Đã xác minh trực tiếp. Kế toán Lan dùng iPad cá nhân để duyệt hóa đơn tại cơ quan. Đánh dấu Resolved.' }
    ]
  },
  {
    id: 'SEC-005',
    userEmail: 'sinhvien_hieuhe16@fpt.edu.vn',
    userRole: 'Student',
    avatar: '',
    campusId: 'HCM',
    alertType: 'ANOMALOUS_IP',
    riskScore: 0.52,
    ipAddress: '115.79.136.21',
    deviceDetail: 'Chrome Mobile 125, Android 13',
    location: 'Vũng Tàu (VN)',
    detectedAt: '2026-06-20 09:22:15',
    status: 'Resolved',
    isUserLocked: false,
    aiExplanation: 'Đăng nhập từ địa chỉ IP thuộc nhà mạng di động khác thường lệ. Điểm rủi ro ở mức trung bình (0.52) do thay đổi tỉnh thành đăng nhập nhanh chóng.',
    historyLogs: [
      { action: 'Detected', time: '2026-06-20 09:22:15', actor: 'AI (Isolation Forest)', note: 'Thay đổi vị trí địa lý. Điểm rủi ro 0.52.' },
      { action: 'Status_Updated', time: '2026-06-20 10:30:00', actor: 'super_admin@fpt.edu.vn', note: 'Sinh viên đi du lịch và đăng nhập học tập bằng 4G Viettel. Đánh dấu Resolved.' }
    ]
  }
])

// --- State Bộ lọc ---
const searchUser = ref('')
const selectedRiskLevel = ref('all') // 'all', 'high' (>0.7), 'mid' (0.4-0.7), 'low' (<0.4)
const selectedStatus = ref('all') // 'all', 'Open', 'Investigating', 'Resolved'
const filterStartDate = ref('')
const filterEndDate = ref('')

const filteredAlerts = computed(() => {
  return alertsMock.value.filter(alert => {
    // Lọc theo userEmail
    const matchUser = !searchUser.value || alert.userEmail.toLowerCase().includes(searchUser.value.toLowerCase())

    // Lọc theo điểm rủi ro Risk Score
    let matchRisk = true
    if (selectedRiskLevel.value === 'high') {
      matchRisk = alert.riskScore >= 0.7
    } else if (selectedRiskLevel.value === 'mid') {
      matchRisk = alert.riskScore >= 0.4 && alert.riskScore < 0.7
    } else if (selectedRiskLevel.value === 'low') {
      matchRisk = alert.riskScore < 0.4
    }

    // Lọc theo trạng thái
    const matchStatus = selectedStatus.value === 'all' || alert.status === selectedStatus.value

    // Lọc theo thời gian
    let matchTime = true
    if (filterStartDate.value) {
      matchTime = matchTime && new Date(alert.detectedAt) >= new Date(filterStartDate.value)
    }
    if (filterEndDate.value) {
      const endDateLimit = new Date(filterEndDate.value)
      endDateLimit.setHours(23, 59, 59, 999)
      matchTime = matchTime && new Date(alert.detectedAt) <= endDateLimit
    }

    return matchUser && matchRisk && matchStatus && matchTime
  })
})

const resetFilters = () => {
  searchUser.value = ''
  selectedRiskLevel.value = 'all'
  selectedStatus.value = 'all'
  filterStartDate.value = ''
  filterEndDate.value = ''
}

// --- KPI Metrics ---
const totalAlertsCount = computed(() => alertsMock.value.length)
const openAlertsCount = computed(() => alertsMock.value.filter(a => a.status === 'Open').length)
const avgRiskScore = computed(() => {
  if (alertsMock.value.length === 0) return 0
  const sum = alertsMock.value.reduce((acc, a) => acc + a.riskScore, 0)
  return parseFloat((sum / alertsMock.value.length).toFixed(2))
})
const lockedAccountsCount = computed(() => alertsMock.value.filter(a => a.isUserLocked).length)

// --- State Alert Detail Drawer ---
const isDrawerOpen = ref(false)
const activeAlert = ref(null)
const drawerStatus = ref('')
const drawerActionNote = ref('')

const openAlertDrawer = (alert) => {
  activeAlert.value = alert
  drawerStatus.value = alert.status
  drawerActionNote.value = ''
  isDrawerOpen.value = true
}

// --- State Confirm Lock Modal ---
const isLockModalOpen = ref(false)
const lockReason = ref('')

const openLockModal = () => {
  lockReason.value = ''
  isLockModalOpen.value = true
}

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

// --- Xử lý Cập nhật trạng thái workflow ---
const updateWorkflowStatus = () => {
  if (!activeAlert.value) return

  const oldStatus = activeAlert.value.status
  const newStatus = drawerStatus.value
  activeAlert.value.status = newStatus

  // Lưu vết lịch sử xử lý
  const timeString = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  activeAlert.value.historyLogs.push({
    action: 'Status_Updated',
    time: timeString,
    actor: 'super_admin@fpt.edu.vn',
    note: `Chuyển trạng thái từ ${oldStatus} sang ${newStatus}. Ghi chú: ${drawerActionNote.value.trim() || 'Không có ghi chú.'}`
  })

  triggerToast(`Đã chuyển trạng thái cảnh báo sang: ${newStatus}`, 'success')
  drawerActionNote.value = ''
}



// --- Xử lý khóa tài khoản ---
const confirmLockAccount = () => {
  if (!activeAlert.value) return
  if (!lockReason.value.trim()) {
    triggerToast('Vui lòng nhập lý do khóa tài khoản!', 'error')
    return
  }

  activeAlert.value.isUserLocked = true
  
  const timeString = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  activeAlert.value.historyLogs.push({
    action: 'Account_Locked',
    time: timeString,
    actor: 'super_admin@fpt.edu.vn',
    note: `Khóa tài khoản người dùng. Lý do: ${lockReason.value}`
  })

  // Đổi trạng thái cảnh báo sang Investigating nếu đang là Open
  if (activeAlert.value.status === 'Open') {
    activeAlert.value.status = 'Investigating'
    drawerStatus.value = 'Investigating'
  }

  isLockModalOpen.value = false
  triggerToast(`Đã khóa tài khoản ${activeAlert.value.userEmail} thành công!`, 'success')
}

// --- Xử lý mở khóa tài khoản ---
const unlockAccount = () => {
  if (!activeAlert.value) return

  activeAlert.value.isUserLocked = false

  const timeString = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  activeAlert.value.historyLogs.push({
    action: 'Account_Unlocked',
    time: timeString,
    actor: 'super_admin@fpt.edu.vn',
    note: 'Mở khóa tài khoản thủ công sau khi xác minh danh tính người dùng an toàn.'
  })

  triggerToast(`Đã mở khóa tài khoản ${activeAlert.value.userEmail}!`, 'info')
}

// --- Helpers phân biệt giao diện ---
const getAlertTypeBadge = (type) => {
  switch (type) {
    case 'IMPOSSIBLE_TRAVEL': return 'bg-rose-500/10 text-rose-500 border-rose-300'
    case 'BRUTE_FORCE': return 'bg-amber-500/10 text-amber-500 border-amber-300'
    case 'ANOMALOUS_IP': return 'bg-violet-500/10 text-violet-500 border-violet-300'
    case 'SUSPICIOUS_DEVICE': return 'bg-sky-500/10 text-sky-500 border-sky-300'
    default: return 'bg-slate-500/10 text-slate-500 border-slate-300'
  }
}

const getAlertTypeName = (type) => {
  switch (type) {
    case 'IMPOSSIBLE_TRAVEL': return 'Vị trí bất thường (Impossible Travel)'
    case 'BRUTE_FORCE': return 'Dò mật khẩu (Brute Force)'
    case 'ANOMALOUS_IP': return 'IP bất thường (Anomalous IP)'
    case 'SUSPICIOUS_DEVICE': return 'Thiết bị lạ (Suspicious Device)'
    default: return type
  }
}

const getRiskProgressColor = (score) => {
  if (score >= 0.7) return 'bg-rose-500'
  if (score >= 0.4) return 'bg-amber-500'
  return 'bg-emerald-500'
}

const getRiskTextColor = (score) => {
  if (score >= 0.7) return 'text-rose-500 font-extrabold'
  if (score >= 0.4) return 'text-amber-500 font-bold'
  return 'text-emerald-500 font-semibold'
}

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'Open': return 'bg-rose-500/10 text-rose-500 border-rose-300'
    case 'Investigating': return 'bg-amber-500/10 text-amber-500 border-amber-300'
    case 'Resolved': return 'bg-emerald-500/10 text-emerald-500 border-emerald-300'
    default: return 'bg-slate-500/10 text-slate-500 border-slate-300'
  }
}

const getStatusName = (status) => {
  switch (status) {
    case 'Open': return 'Mới phát hiện'
    case 'Investigating': return 'Đang điều tra'
    case 'Resolved': return 'Đã xử lý'
    default: return status
  }
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
      <Shield v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
          <ShieldAlert class="w-8 h-8 text-rose-500" />
          Giám Sát Cảnh Báo Bảo Mật (Security Alerts)
        </h1>
        <p class="text-sm text-muted mt-1">
          Quản lý tập trung các cảnh báo xâm nhập tài khoản được phát hiện bởi thuật toán AI Isolation Forest. Xem bối cảnh kỹ thuật và can thiệp bảo vệ an toàn hệ thống.
        </p>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- Tổng số cảnh báo -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Activity class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tổng số cảnh báo</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ totalAlertsCount }} alerts</div>
          </div>
        </div>

        <!-- Cảnh báo chưa xử lý -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <AlertTriangle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Chưa xử lý (Open)</div>
            <div class="text-xl font-bold mt-0.5 text-rose-500 font-extrabold">{{ openAlertsCount }} alerts</div>
          </div>
        </div>

        <!-- Điểm rủi ro trung bình -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Shield class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Risk Score TB (AI)</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ avgRiskScore }}</div>
          </div>
        </div>

        <!-- Tài khoản đã khóa -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <Lock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Đã khóa tài khoản</div>
            <div class="text-xl font-bold mt-0.5 text-rose-600 dark:text-rose-400 font-extrabold">{{ lockedAccountsCount }} user(s)</div>
          </div>
        </div>
      </div>

      <!-- Công cụ Tìm kiếm & Bộ Lọc Nâng Cao -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Tìm kiếm & Lọc cảnh báo rủi ro</h3>
          </div>
          <button 
            @click="resetFilters" 
            class="text-xs text-link font-bold flex items-center gap-1 hover:underline"
          >
            <RotateCcw class="w-3.5 h-3.5" />
            Reset bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-5 gap-3">
          <!-- Tìm kiếm theo Đối tượng -->
          <div class="sm:col-span-2">
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Đối tượng người dùng (User Email)</label>
            <div class="relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input 
                type="text" 
                v-model="searchUser" 
                placeholder="Nhập email sinh viên/giảng viên..."
                class="w-full pl-9 pr-3 lg-control text-sm"
              />
            </div>
          </div>

          <!-- Lọc mức độ rủi ro -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Mức độ rủi ro (Risk Score)</label>
            <select v-model="selectedRiskLevel" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả mức độ</option>
              <option value="high">Rất cao (Risk >= 0.70)</option>
              <option value="mid">Trung bình (0.40 - 0.69)</option>
              <option value="low">Thấp (dưới 0.40)</option>
            </select>
          </div>

          <!-- Lọc trạng thái -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Trạng thái xử lý</label>
            <select v-model="selectedStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Open">Mới phát hiện (Open)</option>
              <option value="Investigating">Đang điều tra</option>
              <option value="Resolved">Đã giải quyết (Resolved)</option>
            </select>
          </div>

          <!-- Lọc theo thời gian -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Từ ngày</label>
            <div class="relative">
              <Calendar class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input type="date" v-model="filterStartDate" class="w-full pl-9 pr-3 lg-control text-xs" />
            </div>
          </div>
        </div>
      </div>

      <!-- Bảng Alert Table -->
      <div class="lg-table-shell overflow-x-auto w-full max-w-full mb-8">
        <table class="min-w-[1100px] w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Người dùng (Campus)</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Loại cảnh báo</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase w-32 whitespace-nowrap">Chỉ số rủi ro (Risk)</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">IP & Vị trí</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Thiết bị</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap">Trạng thái</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Phát sinh lúc</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap w-16">Xem</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredAlerts.length === 0">
              <td colspan="8" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <ShieldAlert class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy cảnh báo bảo mật nào phù hợp.</span>
                </div>
              </td>
            </tr>

            <tr v-for="alert in filteredAlerts" :key="alert.id" class="transition-colors hover:bg-surface-table-row-hover">
              <!-- Người dùng -->
              <td class="px-4 py-4 whitespace-nowrap">
                <div class="flex items-center gap-2.5">
                  <span class="w-8 h-8 rounded-full bg-slate-500/10 flex items-center justify-center border border-default text-primary flex-shrink-0 font-bold text-xs">
                    {{ alert.userEmail.charAt(0).toUpperCase() }}
                  </span>
                  <div>
                    <div class="flex items-center gap-1.5">
                      <span class="font-extrabold text-heading text-xs truncate max-w-[170px]" :title="alert.userEmail">
                        {{ alert.userEmail }}
                      </span>
                      <span v-if="alert.isUserLocked" class="text-[9px] bg-rose-500/10 text-rose-500 px-1.5 py-0.5 rounded border border-rose-300 font-extrabold flex items-center gap-0.5 whitespace-nowrap">
                        <Lock class="w-2.5 h-2.5" /> Khóa
                      </span>
                    </div>
                    <div class="text-[10px] text-muted mt-0.5 flex items-center gap-1.5">
                      <span>Vai trò: {{ alert.userRole }}</span>
                      <span>•</span>
                      <span class="px-1.5 py-0.2 bg-surface-input border border-default rounded text-[9px] font-bold">{{ alert.campusId }}</span>
                    </div>
                  </div>
                </div>
              </td>

              <!-- Loại cảnh báo -->
              <td class="px-4 py-4 whitespace-nowrap">
                <span 
                  class="px-2 py-0.5 rounded border text-[10px] font-extrabold tracking-wide whitespace-nowrap"
                  :class="getAlertTypeBadge(alert.alertType)"
                >
                  {{ getAlertTypeName(alert.alertType) }}
                </span>
              </td>

              <!-- Risk Score -->
              <td class="px-4 py-4">
                <div class="flex flex-col gap-1 w-28">
                  <div class="flex justify-between items-center text-[10px]">
                    <span :class="getRiskTextColor(alert.riskScore)">Score: {{ alert.riskScore }}</span>
                  </div>
                  <div class="lg-progress-track h-1.5 w-full">
                    <div 
                      class="lg-progress-fill" 
                      :class="getRiskProgressColor(alert.riskScore)"
                      :style="{ width: `${alert.riskScore * 100}%` }"
                    ></div>
                  </div>
                </div>
              </td>

              <!-- IP & Location -->
              <td class="px-4 py-4 whitespace-nowrap">
                <div class="font-mono text-xs text-heading font-semibold">{{ alert.ipAddress }}</div>
                <div class="text-[10px] text-muted flex items-center gap-1 mt-0.5">
                  <Globe class="w-3 h-3 text-slate-400" />
                  {{ alert.location }}
                </div>
              </td>

              <!-- Device detail -->
              <td class="px-4 py-4">
                <div class="text-xs text-muted leading-tight truncate max-w-[150px]" :title="alert.deviceDetail">
                  {{ alert.deviceDetail }}
                </div>
              </td>

              <!-- Trạng thái -->
              <td class="px-4 py-4 text-center whitespace-nowrap">
                <span 
                  class="px-2.5 py-0.5 rounded-full border text-[10px] font-extrabold tracking-wide"
                  :class="getStatusBadgeClass(alert.status)"
                >
                  {{ getStatusName(alert.status) }}
                </span>
              </td>

              <!-- Phát sinh lúc -->
              <td class="px-4 py-4 text-xs font-semibold text-muted whitespace-nowrap">
                {{ alert.detectedAt }}
              </td>

              <!-- Thao tác -->
              <td class="px-4 py-4 text-center whitespace-nowrap w-16">
                <div class="flex items-center justify-center">
                  <button
                    @click="openAlertDrawer(alert)"
                    class="lg-btn-secondary p-1.5 rounded-lg flex items-center justify-center"
                    title="Xem chi tiết kỹ thuật"
                  >
                    <Eye class="w-4 h-4" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Alert Detail Drawer (Ngăn kéo chi tiết cảnh báo) -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[100] flex justify-end bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-2xl h-screen bg-surface-modal lg-glass-strong border-l border-default shadow-2xl flex flex-col animate-in slide-in-from-right duration-300">
        <!-- Drawer Header -->
        <div class="p-5 border-b border-default flex items-center justify-between">
          <div class="space-y-1">
            <h2 class="text-lg font-extrabold text-heading flex items-center gap-2.5">
              <ShieldAlert class="w-5.5 h-5.5 text-rose-500" />
              Chi Tiết Cảnh Báo Bảo Mật
            </h2>
            <p class="text-xs text-muted">Báo cáo bối cảnh kỹ thuật phát sinh mối đe dọa bảo mật.</p>
          </div>

          <button 
            @click="isDrawerOpen = false"
            class="lg-icon-button p-2 bg-surface-card rounded-lg border border-default text-muted hover:text-heading"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Drawer Body -->
        <div class="flex-1 overflow-y-auto p-5 space-y-5">
          <!-- Banner Cảnh Báo Chung -->
          <div 
            class="p-4 rounded-xl border flex items-start gap-3"
            :class="activeAlert?.riskScore >= 0.7 ? 'bg-rose-500/10 border-rose-300 text-rose-700 dark:text-rose-400' : 'bg-amber-500/10 border-amber-300 text-amber-700 dark:text-amber-400'"
          >
            <AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5" />
            <div>
              <h4 class="font-extrabold text-sm uppercase">Cảnh báo: {{ getAlertTypeName(activeAlert?.alertType) }}</h4>
              <p class="text-xs mt-1 leading-relaxed">
                Được hệ thống phát hiện vào lúc **{{ activeAlert?.detectedAt }}** từ IP **{{ activeAlert?.ipAddress }}** ({{ activeAlert?.location }}). Điểm chỉ số rủi ro tính toán: **{{ activeAlert?.riskScore }}**.
              </p>
            </div>
          </div>

          <!-- Thông tin kỹ thuật & Người dùng -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Thẻ người dùng -->
            <div class="p-4 rounded-xl bg-surface-card border border-default space-y-2.5">
              <h3 class="text-xs font-bold text-muted uppercase tracking-wider flex items-center gap-1.5">
                <User class="w-4 h-4 text-primary" />
                Đối tượng cảnh báo
              </h3>
              <div class="space-y-1 text-xs">
                <div><span class="text-muted">Tài khoản:</span> <strong class="text-heading font-extrabold">{{ activeAlert?.userEmail }}</strong></div>
                <div><span class="text-muted">Vai trò:</span> <span class="text-heading font-semibold">{{ activeAlert?.userRole }}</span></div>
                <div><span class="text-muted">Cơ sở (Campus):</span> <span class="px-1.5 py-0.2 bg-surface-input border border-default rounded text-[9px] font-bold">{{ activeAlert?.campusId }}</span></div>
                <div class="pt-1 flex items-center gap-1.5">
                  <span class="text-muted">Trạng thái tài khoản:</span> 
                  <span 
                    class="font-extrabold text-[10px] px-2 py-0.5 rounded border"
                    :class="activeAlert?.isUserLocked ? 'bg-rose-500/10 text-rose-500 border-rose-300' : 'bg-emerald-500/10 text-emerald-500 border-emerald-300'"
                  >
                    {{ activeAlert?.isUserLocked ? 'Bị Khóa (Locked)' : 'Hoạt Động (Active)' }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Thẻ Dữ liệu kết nối -->
            <div class="p-4 rounded-xl bg-surface-card border border-default space-y-2.5">
              <h3 class="text-xs font-bold text-muted uppercase tracking-wider flex items-center gap-1.5">
                <Cpu class="w-4 h-4 text-primary" />
                Dữ liệu kỹ thuật
              </h3>
              <div class="space-y-1 text-xs">
                <div><span class="text-muted">Địa chỉ IP:</span> <code class="text-heading font-mono font-bold">{{ activeAlert?.ipAddress }}</code></div>
                <div><span class="text-muted">Địa lý:</span> <span class="text-heading font-semibold">{{ activeAlert?.location }}</span></div>
                <div class="truncate" :title="activeAlert?.deviceDetail"><span class="text-muted">Thiết bị:</span> <span class="text-heading font-semibold">{{ activeAlert?.deviceDetail }}</span></div>
                <div><span class="text-muted">ID Cảnh báo:</span> <span class="text-heading font-semibold">{{ activeAlert?.id }}</span></div>
              </div>
            </div>
          </div>

          <!-- Phân tích của AI (Isolation Forest) -->
          <div class="p-4 rounded-xl bg-violet-500/5 border border-violet-500/10 space-y-2.5">
            <h3 class="text-xs font-bold text-violet-600 dark:text-violet-400 uppercase tracking-wider flex items-center gap-1.5">
              <Database class="w-4 h-4" />
              AI Security Analysis (Isolation Forest Engine)
            </h3>
            <p class="text-xs text-heading leading-relaxed font-medium">
              {{ activeAlert?.aiExplanation }}
            </p>
            <div class="p-3 bg-violet-500/10 rounded-lg border border-violet-300/30 text-[10px] leading-relaxed text-slate-700 dark:text-slate-300 space-y-1">
              <span class="font-bold flex items-center gap-1 text-violet-600 dark:text-violet-400">
                <Shield class="w-3.5 h-3.5" /> Quy tắc Nghiệp vụ Bảo Mật Kép:
              </span>
              <span>1. AI phân tích điểm bất thường dựa trên hành vi đăng nhập lịch sử. Nếu rủi ro vượt <strong>0.70</strong>, hệ thống tự kích hoạt cảnh báo này.</span>
              <br/>
              <span>2. AI chỉ đề xuất rủi ro chứ không tự động thực hiện hành động khóa tài khoản để tránh false-positives. <strong>Super Admin là người quyết định cuối cùng</strong>.</span>
            </div>
          </div>

          <!-- Workflow Status & Actions Form -->
          <div class="p-4 rounded-xl bg-surface-card border border-default space-y-4">
            <h3 class="text-xs font-bold text-muted uppercase tracking-wider">Cập nhật quy trình điều phối cảnh báo</h3>
            
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <!-- Chọn trạng thái workflow -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Chuyển trạng thái</label>
                <select v-model="drawerStatus" class="w-full px-3 lg-control text-sm">
                  <option value="Open">Mới phát hiện (Open)</option>
                  <option value="Investigating">Đang điều tra (Investigating)</option>
                  <option value="Resolved">Đã xử lý xong (Resolved)</option>
                </select>
              </div>

              <!-- Thao tác tài khoản trực tiếp -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Hành động bảo mật</label>
                <div class="flex gap-2">
                  <button
                    v-if="!activeAlert?.isUserLocked"
                    @click="openLockModal"
                    class="lg-btn-danger text-xs px-4 py-2 flex-1 flex items-center justify-center gap-1.5"
                  >
                    <Lock class="w-4 h-4" />
                    Khóa tài khoản
                  </button>
                  <button
                    v-else
                    @click="unlockAccount"
                    class="lg-btn-success text-xs px-4 py-2 flex-1 flex items-center justify-center gap-1.5"
                  >
                    <Unlock class="w-4 h-4" />
                    Mở khóa tài khoản
                  </button>
                </div>
              </div>
            </div>

            <!-- Ghi chú hành động -->
            <div>
              <label class="block text-xs font-bold text-label mb-1.5 uppercase">Ghi chú hành động / Nội dung xử lý</label>
              <textarea
                v-model="drawerActionNote"
                rows="2"
                placeholder="Nhập ghi chú xử lý để phục vụ kiểm toán bảo mật..."
                class="w-full px-3 py-2 lg-control text-xs leading-relaxed"
              ></textarea>
            </div>

            <!-- Cập nhật button -->
            <div class="flex justify-end pt-1">
              <button
                @click="updateWorkflowStatus"
                class="lg-btn-primary text-xs px-4 py-2 flex items-center gap-1.5"
              >
                <CheckCircle class="w-4 h-4" />
                Cập nhật Quy trình
              </button>
            </div>
          </div>

          <!-- Nhật ký biến động (History logs) -->
          <div class="space-y-2.5">
            <h3 class="text-xs font-bold text-muted uppercase tracking-wider flex items-center gap-1.5">
              <Activity class="w-4 h-4 text-primary" />
              Lịch sử ghi vết xử lý cảnh báo
            </h3>
            <div class="space-y-2">
              <div 
                v-for="(log, idx) in activeAlert?.historyLogs" 
                :key="idx"
                class="p-3 rounded-lg bg-surface-card border border-default text-xs leading-relaxed"
              >
                <div class="flex items-center justify-between text-muted font-bold text-[10px] mb-1">
                  <span class="flex items-center gap-1 text-primary">
                    <User class="w-3 h-3" />
                    {{ log.actor }}
                  </span>
                  <span>{{ log.time }}</span>
                </div>
                <div>
                  <span class="font-extrabold text-heading">Hành động:</span>
                  <span class="px-2 py-0.2 ml-1 rounded bg-surface-input border border-default text-[9px] font-bold">
                    {{ log.action }}
                  </span>
                </div>
                <p class="text-muted mt-1 italic font-semibold">
                  {{ log.note }}
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Drawer Footer -->
        <div class="p-5 border-t border-default flex items-center justify-end bg-surface-table">
          <button
            @click="isDrawerOpen = false"
            class="lg-btn-secondary px-5 py-2 text-xs font-bold"
          >
            Đóng Panel
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Lock Account Modal (Modal xác nhận khóa tài khoản) -->
    <div 
      v-if="isLockModalOpen" 
      class="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <!-- Close button -->
        <button 
          @click="isLockModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
          <div class="w-10 h-10 rounded-full bg-rose-500/10 flex items-center justify-center text-rose-500 flex-shrink-0">
            <Lock class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Xác Nhận Khóa Tài Khoản</h3>
            <p class="text-xs text-muted mt-0.5">Hành động can thiệp khẩn cấp bảo vệ tài khoản khỏi xâm nhập trái phép.</p>
          </div>
        </div>

        <!-- Nội dung Form xác nhận -->
        <div class="space-y-4 mb-5">
          <div class="p-3.5 rounded-lg bg-rose-500/5 border border-rose-500/10 text-xs flex items-start gap-2 text-rose-600 dark:text-rose-400">
            <AlertTriangle class="w-4.5 h-4.5 flex-shrink-0 mt-0.5" />
            <p class="leading-relaxed font-semibold">
              Tài khoản **{{ activeAlert?.userEmail }}** sẽ bị vô hiệu hóa đăng nhập trên toàn hệ thống ngay lập tức. Các session token hiện tại sẽ bị hủy bỏ.
            </p>
          </div>

          <!-- Lý do bắt buộc -->
          <div>
            <label class="block text-xs font-bold text-label mb-2 uppercase">Lý do khóa tài khoản (Bắt buộc)</label>
            <textarea
              v-model="lockReason"
              rows="3"
              placeholder="Nhập lý do chi tiết để làm dữ liệu lưu vết audit log..."
              class="w-full px-3 py-2 lg-control text-xs leading-relaxed"
            ></textarea>
            <span v-if="!lockReason.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Bắt buộc nhập lý do phục vụ lưu vết</span>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="isLockModalOpen = false"
            class="lg-btn-secondary px-4 py-2 text-sm font-bold"
          >
            Hủy
          </button>
          <button
            @click="confirmLockAccount"
            :disabled="!lockReason.trim()"
            class="lg-btn-danger text-xs px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Lock class="w-4 h-4" />
            Đồng ý khóa tài khoản
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
