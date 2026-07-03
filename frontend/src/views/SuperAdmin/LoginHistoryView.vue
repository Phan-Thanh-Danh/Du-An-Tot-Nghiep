<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  Search,
  Eye,
  LogOut,
  ShieldAlert,
  AlertTriangle,
  AlertCircle,
  Globe,
  X,
  Clock,
  Info,
  Sparkles,
  Bell,
  Monitor,
  Laptop,
  Smartphone,
  UserX,
  ShieldCheck
} from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'
import { apiRequest } from '@/services/apiClient'

const ENABLE_MOCK_API =
  import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const popup = usePopupStore()

// State & Filters
const searchQuery = ref('')
const selectedRole = ref('Tất cả')
const selectedStatus = ref('Tất cả')
const selectedCampus = ref('Tất cả')
const selectedTimeRange = ref('30days') // 'today' | '7days' | '30days'

const roles = ['Tất cả', 'Sinh viên', 'Giảng viên', 'Giáo vụ', 'BGH', 'Admin']
const statuses = ['Tất cả', 'Success', 'Failed', 'Suspicious']
const campuses = ['Tất cả', 'Hà Nội', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ']

const loading = ref(false)
const error = ref('')
const logins = ref([])

const mockLogins = [
  {
    id: 's_001',
    userName: 'Nguyễn Văn A',
    email: 'nguyenvana@example.com',
    role: 'Sinh viên',
    campus: 'Hà Nội',
    ip: '113.190.234.12',
    location: 'Hà Nội, Việt Nam',
    device: 'Windows 11',
    browser: 'Chrome 122.0',
    userAgent: 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36',
    time: '2026-06-04 13:45:22',
    status: 'Success',
    riskScore: 12,
    aiReason: 'Hành vi bình thường. Đăng nhập đúng giờ học tập thường lệ từ thiết bị quen thuộc.'
  },
  {
    id: 's_002',
    userName: 'Trần Thị B',
    email: 'tranthib@example.com',
    role: 'Giảng viên',
    campus: 'TP.HCM',
    ip: '27.72.90.54',
    location: 'TP.HCM, Việt Nam',
    device: 'macOS Sonoma',
    browser: 'Safari 17.2',
    userAgent: 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.2.1 Safari/605.1.15',
    time: '2026-06-04 12:30:10',
    status: 'Success',
    riskScore: 8,
    aiReason: 'Hành vi bình thường. Thiết bị và IP trùng khớp lịch sử chấm công.'
  },
  {
    id: 's_003',
    userName: 'Lê Văn C',
    email: 'levanc@example.com',
    role: 'Giáo vụ',
    campus: 'Đà Nẵng',
    ip: '103.82.143.21',
    location: 'Đà Nẵng, Việt Nam',
    device: 'iPhone 15 Pro',
    browser: 'Chrome Mobile',
    userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 17_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/122.0.6261.89 Mobile/15E148 Safari/604.1',
    time: '2026-06-04 02:15:45',
    status: 'Suspicious',
    riskScore: 78,
    aiReason: 'Đăng nhập vào ban đêm (02:15 AM). Đổi thiết bị từ Laptop sang Điện thoại di động. Vị trí địa lý cách nơi đăng nhập gần nhất 300km trong vòng 1 giờ (Impossible Travel).'
  },
  {
    id: 's_004',
    userName: 'Phạm Thị D',
    email: 'phamthid@example.com',
    role: 'BGH',
    campus: 'Hà Nội',
    ip: '45.119.212.8',
    location: 'Singapore (VPN nghi vấn)',
    device: 'Ubuntu Linux',
    browser: 'Firefox 123.0',
    userAgent: 'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:123.0) Gecko/20100101 Firefox/123.0',
    time: '2026-06-03 21:10:00',
    status: 'Failed',
    riskScore: 92,
    aiReason: 'Sai mật khẩu 5 lần liên tiếp trong 2 phút. Thiết bị sử dụng Linux OS chạy trình duyệt ẩn danh từ IP đặt tại Singapore. Nguy cơ bị Brute-force cao.'
  },
  {
    id: 's_005',
    userName: 'Hoàng Văn E',
    email: 'hoangvane@example.com',
    role: 'Sinh viên',
    campus: 'Cần Thơ',
    ip: '115.79.138.99',
    location: 'Cần Thơ, Việt Nam',
    device: 'Android 14',
    browser: 'Samsung Browser',
    userAgent: 'Mozilla/5.0 (Linux; Android 10; SAMSUNG SM-G981B) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/16.0 Chrome/92.0.4515.166 Mobile Safari/537.36',
    time: '2026-06-02 08:05:11',
    status: 'Success',
    riskScore: 15,
    aiReason: 'Hành vi bình thường. Đăng nhập qua ứng dụng di động định danh.'
  }
]

async function loadLogins() {
  loading.value = true
  error.value = ''
  try {
    const data = await apiRequest('/api/audit-logs?pageIndex=1&pageSize=100')
    logins.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch (e) {
    if (ENABLE_MOCK_API) {
      logins.value = JSON.parse(JSON.stringify(mockLogins))
      return
    }
    error.value = e?.message || 'Không thể tải lịch sử đăng nhập.'
    logins.value = []
  } finally {
    loading.value = false
  }
}

// Filtered Logins
const filteredLogins = computed(() => {
  return logins.value.filter(log => {
    const matchSearch = log.userName.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        log.email.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                        log.ip.includes(searchQuery.value.toLowerCase())
    const matchRole = selectedRole.value === 'Tất cả' || log.role === selectedRole.value
    const matchStatus = selectedStatus.value === 'Tất cả' || log.status === selectedStatus.value
    const matchCampus = selectedCampus.value === 'Tất cả' || log.campus === selectedCampus.value
    return matchSearch && matchRole && matchStatus && matchCampus
  })
})

// Statistics
const totalLogins = computed(() => logins.value.length)
const activeSessionsCount = ref(4)
const failedAttemptsCount = computed(() => logins.value.filter(l => l.status === 'Failed').length)
const suspiciousAlertsCount = computed(() => logins.value.filter(l => l.riskScore >= 70).length)

// Drawer & Modal States
const isDetailDrawerOpen = ref(false)
const selectedSession = ref(null)

const isRevokeModalOpen = ref(false)
const sessionToRevoke = ref(null)

const isLockModalOpen = ref(false)
const accountToLock = ref(null)
const lockReason = ref('')

const isAlertModalOpen = ref(false)
const newAlertConfig = ref({
  name: 'Đăng nhập từ IP nước ngoài',
  triggerType: 'risk_score', // 'risk_score' | 'ip' | 'failed_attempts'
  threshold: 75,
  channel: 'email_sms'
})

// Handlers
const openDetailDrawer = (session) => {
  selectedSession.value = session
  isDetailDrawerOpen.value = true
}

const openRevokeModal = (session) => {
  sessionToRevoke.value = session
  isRevokeModalOpen.value = true
}

const confirmRevokeSession = () => {
  popup.success(`Đã cưỡng chế đăng xuất và hủy token phiên ${sessionToRevoke.value.id} của người dùng ${sessionToRevoke.value.userName}!`)
  activeSessionsCount.value = Math.max(0, activeSessionsCount.value - 1)
  isRevokeModalOpen.value = false
}

const openLockModal = (session) => {
  accountToLock.value = session
  lockReason.value = `Phát hiện phiên đăng nhập bất thường với điểm rủi ro cao (${session.riskScore}%). IP: ${session.ip}.`
  isLockModalOpen.value = true
}

const confirmLockAccount = () => {
  if (!lockReason.value.trim()) {
    popup.warning('Vui lòng nhập lý do khóa tài khoản.')
    return
  }
  popup.success(`Tài khoản ${accountToLock.value.email} đã được KHÓA ngay lập tức. Lý do: ${lockReason.value}`)
  
  // Update local data state if match
  const idx = logins.value.findIndex(l => l.email === accountToLock.value.email)
  if (idx !== -1) {
    logins.value[idx].status = 'Failed'
    logins.value[idx].riskScore = 100
  }
  isLockModalOpen.value = false
}

const openAlertModal = () => {
  isAlertModalOpen.value = true
}

const confirmCreateAlert = () => {
  popup.info(`Đã thiết lập Luật giám sát: "${newAlertConfig.value.name}". AI sẽ tự động gửi thông báo khi phát hiện vi phạm hành vi này.`)
  isAlertModalOpen.value = false
}

// Helpers
const getStatusLabel = (status) => {
  if (status === 'Success') return 'Thành công'
  if (status === 'Failed') return 'Thất bại'
  if (status === 'Suspicious') return 'Nghi ngờ'
  return status
}

const getStatusClass = (status) => {
  if (status === 'Success') return 'bg-emerald-50 text-emerald-700 border border-emerald-200'
  if (status === 'Failed') return 'bg-rose-50 text-rose-700 border border-rose-200'
  if (status === 'Suspicious') return 'bg-amber-50 text-amber-700 border border-amber-200'
  return 'bg-slate-50 text-slate-700 border border-slate-200'
}

const getRiskClass = (score) => {
  if (score >= 80) return 'text-rose-600 bg-rose-50 border-rose-200 font-bold'
  if (score >= 50) return 'text-amber-600 bg-amber-50 border-amber-200 font-semibold'
  return 'text-emerald-600 bg-emerald-50 border-emerald-200'
}

const getDeviceIcon = (device) => {
  if (device.toLowerCase().includes('windows') || device.toLowerCase().includes('mac') || device.toLowerCase().includes('linux')) {
    return Laptop
  }
  return Smartphone
}

onMounted(() => { loadLogins() })
</script>

<template>
  <div v-if="loading" class="glass-panel rounded-2xl p-12 flex flex-col items-center justify-center mb-6">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full mb-4"></div>
    <p class="text-label text-sm">Đang tải lịch sử đăng nhập...</p>
  </div>
  <div v-else-if="error" class="glass-panel rounded-2xl p-12 flex flex-col items-center justify-center mb-6">
    <AlertCircle :size="40" class="text-rose-400 mb-3" />
    <p class="text-rose-600 font-semibold mb-2">{{ error }}</p>
    <button @click="loadLogins" class="glass-btn primary text-xs">Thử lại</button>
  </div>
  <div class="login-history-page h-full flex flex-col gap-6">

    <!-- METRIC CARDS OVERVIEW -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="glass-panel p-4 rounded-xl flex items-center justify-between shadow-sm bg-white/70">
        <div>
          <p class="text-[10px] uppercase font-bold text-label mb-1">Phiên đang hoạt động</p>
          <h3 class="text-2xl font-black text-purple-700">{{ activeSessionsCount }} <span class="text-xs font-normal text-slate-400">thiết bị</span></h3>
        </div>
        <div class="h-10 w-10 bg-purple-50 text-purple-600 rounded-lg flex items-center justify-center">
          <Monitor :size="20" />
        </div>
      </div>
      
      <div class="glass-panel p-4 rounded-xl flex items-center justify-between shadow-sm bg-white/70">
        <div>
          <p class="text-[10px] uppercase font-bold text-label mb-1">Tổng lượt truy cập</p>
          <h3 class="text-2xl font-black text-slate-700">{{ totalLogins }} <span class="text-xs font-normal text-slate-400">lần</span></h3>
        </div>
        <div class="h-10 w-10 bg-slate-50 text-slate-500 rounded-lg flex items-center justify-center">
          <Clock :size="20" />
        </div>
      </div>

      <div class="glass-panel p-4 rounded-xl flex items-center justify-between shadow-sm bg-white/70">
        <div>
          <p class="text-[10px] uppercase font-bold text-label mb-1">Lượt thất bại</p>
          <h3 class="text-2xl font-black text-rose-600">{{ failedAttemptsCount }} <span class="text-xs font-normal text-slate-400">lần</span></h3>
        </div>
        <div class="h-10 w-10 bg-rose-50 text-rose-500 rounded-lg flex items-center justify-center">
          <ShieldAlert :size="20" />
        </div>
      </div>

      <div class="glass-panel p-4 rounded-xl flex items-center justify-between shadow-sm bg-white/70">
        <div>
          <p class="text-[10px] uppercase font-bold text-purple-900 flex items-center gap-1 mb-1">
            <Sparkles :size="10" class="text-purple-600" /> Cảnh báo AI
          </p>
          <h3 class="text-2xl font-black text-amber-600">{{ suspiciousAlertsCount }} <span class="text-xs font-normal text-slate-400">nghi ngờ</span></h3>
        </div>
        <div class="h-10 w-10 bg-amber-50 text-amber-600 rounded-lg flex items-center justify-center">
          <AlertTriangle :size="20" />
        </div>
      </div>
    </div>

    <!-- FILTER BOARD -->
    <div class="flex flex-col xl:flex-row justify-between items-stretch xl:items-center gap-4 bg-slate-50/50 p-4 rounded-xl border border-slate-100/50 backdrop-blur-sm">
      <div class="flex flex-wrap items-center gap-3 flex-1">
        <!-- Search bar -->
        <div class="relative min-w-[200px] flex-1">
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm theo Tên, Email hoặc IP..." 
            class="glass-input w-full bg-white pl-9" 
          />
          <Search class="absolute left-3 top-2.5 text-placeholder" :size="16" />
        </div>

        <!-- Role select -->
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-label whitespace-nowrap">Vai trò:</span>
          <select v-model="selectedRole" class="glass-select bg-white py-1.5 text-xs">
            <option v-for="r in roles" :key="r" :value="r">{{ r }}</option>
          </select>
        </div>

        <!-- Status select -->
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-label whitespace-nowrap">Kết quả:</span>
          <select v-model="selectedStatus" class="glass-select bg-white py-1.5 text-xs">
            <option v-for="s in statuses" :key="s" :value="s">{{ getStatusLabel(s) }}</option>
          </select>
        </div>

        <!-- Campus select -->
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-label whitespace-nowrap">Cơ sở:</span>
          <select v-model="selectedCampus" class="glass-select bg-white py-1.5 text-xs">
            <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
          </select>
        </div>
        
        <!-- Time range select -->
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-label whitespace-nowrap">Thời gian:</span>
          <select v-model="selectedTimeRange" class="glass-select bg-white py-1.5 text-xs">
            <option value="today">Hôm nay</option>
            <option value="7days">7 ngày qua</option>
            <option value="30days">30 ngày qua</option>
          </select>
        </div>
      </div>

      <div class="flex gap-2 shrink-0">
        <button 
          @click="openAlertModal" 
          class="glass-btn primary !bg-purple-600 hover:!bg-purple-700 text-xs py-2 h-9"
        >
          <Bell :size="14" /> Cấu hình Cảnh báo AI
        </button>
      </div>
    </div>

    <!-- MAIN LOGS TABLE -->
    <div class="glass-panel rounded-2xl overflow-hidden shadow-lg border border-slate-100 flex-1 flex flex-col min-h-[450px]">
      <div class="table-container flex-1">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr>
              <th class="w-[200px]">Tài khoản</th>
              <th class="w-[120px]">Cơ sở</th>
              <th class="w-[120px]">Địa chỉ IP</th>
              <th class="w-[180px]">Thiết bị & Trình duyệt</th>
              <th class="w-[100px] text-center">Rủi ro (AI)</th>
              <th class="w-[120px] text-center">Trạng thái</th>
              <th class="w-[150px]">Thời gian</th>
              <th class="w-[140px] text-right">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="log in filteredLogins" 
              :key="log.id" 
              class="border-b border-slate-100 hover:bg-slate-50/50 transition-colors"
            >
              <td>
                <div class="font-bold text-heading text-xs">{{ log.userName }}</div>
                <div class="text-[10px] text-placeholder">{{ log.email }}</div>
                <div class="text-[9px] font-bold text-purple-600 uppercase tracking-wide mt-0.5">{{ log.role }}</div>
              </td>
              <td class="text-xs font-semibold text-slate-600">
                {{ log.campus }}
              </td>
              <td>
                <div class="text-xs font-mono font-medium text-heading flex items-center gap-1">
                  <Globe :size="12" class="text-slate-400" />
                  {{ log.ip }}
                </div>
                <div class="text-[9px] text-label mt-0.5" :title="log.location">
                  {{ log.location }}
                </div>
              </td>
              <td>
                <div class="text-xs text-heading flex items-center gap-1.5">
                  <component :is="getDeviceIcon(log.device)" :size="14" class="text-slate-400" />
                  <span>{{ log.device }}</span>
                </div>
                <div class="text-[10px] text-label mt-0.5">{{ log.browser }}</div>
              </td>
              <td class="text-center">
                <span 
                  class="inline-flex items-center justify-center px-2 py-0.5 rounded text-[10px] font-mono border"
                  :class="getRiskClass(log.riskScore)"
                >
                  {{ log.riskScore }}%
                </span>
              </td>
              <td class="text-center">
                <span 
                  class="px-2 py-1 rounded-full text-[10px] font-bold inline-flex items-center gap-1"
                  :class="getStatusClass(log.status)"
                >
                  <component 
                    :is="log.status === 'Success' ? ShieldCheck : (log.status === 'Failed' ? ShieldAlert : AlertTriangle)" 
                    :size="10" 
                  />
                  {{ getStatusLabel(log.status) }}
                </span>
              </td>
              <td class="text-xs text-slate-500">
                {{ log.time }}
              </td>
              <td class="text-right">
                <div class="flex items-center justify-end gap-1">
                  <button 
                    @click="openDetailDrawer(log)"
                    class="action-btn text-purple-600 hover:bg-purple-50"
                    title="Xem chi tiết phiên"
                  >
                    <Eye :size="16" />
                  </button>
                  <button 
                    @click="openRevokeModal(log)"
                    class="action-btn text-amber-600 hover:bg-amber-50"
                    title="Cưỡng chế đăng xuất (Hủy phiên)"
                  >
                    <LogOut :size="16" />
                  </button>
                  <button 
                    @click="openLockModal(log)"
                    class="action-btn text-rose-600 hover:bg-rose-50"
                    title="Khóa tài khoản lập tức"
                  >
                    <UserX :size="16" />
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="filteredLogins.length === 0">
              <td colspan="8" class="text-center py-16 text-slate-400">
                <ShieldAlert :size="32" class="mx-auto text-slate-300 mb-2" />
                Không tìm thấy bản ghi lịch sử đăng nhập nào.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer Note -->
      <div class="p-3 bg-slate-50 border-t border-slate-100 flex justify-between items-center text-[10px] text-slate-400">
        <span class="flex items-center gap-1">
          <Sparkles :size="12" class="text-purple-600" />
          Điểm rủi ro (Risk score) được phân tích theo thuật toán phát hiện bất thường Isolation Forest (AI Anomaly Detection).
        </span>
        <span>Super Admin có toàn quyền quyết định khóa/hủy phiên bảo mật.</span>
      </div>
    </div>

    <!-- TELEPORTS TO BODY -->
    <Teleport to="body">
      
      <!-- Drawer: Xem Chi Tiết Phiên Đăng Nhập -->
      <div v-if="isDetailDrawerOpen" class="drawer-overlay" @click="isDetailDrawerOpen = false"></div>
      <div class="drawer drawer-md" :class="{ 'open': isDetailDrawerOpen }">
        <div class="drawer-header bg-slate-50/50">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2">
            <Info :size="20" class="text-purple-600"/> Chi tiết Phiên Đăng nhập
          </h3>
          <button @click="isDetailDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>

        <div class="drawer-body p-6 flex flex-col gap-6" v-if="selectedSession">
          <!-- User brief -->
          <div class="flex items-center gap-3 border-b border-slate-100 pb-4">
            <div class="h-12 w-12 bg-purple-100 text-purple-700 font-black rounded-full flex items-center justify-center text-lg">
              {{ selectedSession.userName.charAt(0) }}
            </div>
            <div>
              <h4 class="font-bold text-heading">{{ selectedSession.userName }}</h4>
              <p class="text-xs text-label">{{ selectedSession.email }}</p>
              <div class="text-[9px] bg-purple-50 text-purple-700 font-bold border border-purple-200 rounded px-1.5 py-0.2 inline-block uppercase tracking-wider mt-1">
                {{ selectedSession.role }} • {{ selectedSession.campus }}
              </div>
            </div>
          </div>

          <!-- Basic Connection Information -->
          <div class="space-y-2">
            <h4 class="text-xs font-bold text-label uppercase tracking-wider">Thông số mạng & Vị trí</h4>
            <div class="bg-slate-50 p-4 rounded-xl border border-slate-100 space-y-2">
              <div class="info-row">
                <span class="info-label">Địa chỉ IP:</span>
                <span class="font-mono font-bold text-heading text-xs">{{ selectedSession.ip }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Vị trí địa lý:</span>
                <span class="font-semibold text-heading text-xs">{{ selectedSession.location }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Thời gian yêu cầu:</span>
                <span class="font-medium text-slate-500 text-xs">{{ selectedSession.time }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">ID Phiên (Session ID):</span>
                <span class="font-mono text-slate-500 text-[10px]">{{ selectedSession.id }}</span>
              </div>
            </div>
          </div>

          <!-- Device details -->
          <div class="space-y-2">
            <h4 class="text-xs font-bold text-label uppercase tracking-wider">Thiết bị & Hệ điều hành</h4>
            <div class="bg-slate-50 p-4 rounded-xl border border-slate-100 space-y-2">
              <div class="info-row">
                <span class="info-label">Hệ điều hành:</span>
                <span class="font-bold text-heading text-xs">{{ selectedSession.device }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">Trình duyệt:</span>
                <span class="font-semibold text-heading text-xs">{{ selectedSession.browser }}</span>
              </div>
              <div>
                <span class="block text-[10px] text-label mb-1">User-Agent gốc:</span>
                <div class="bg-white p-2 rounded border border-slate-200 text-[9px] font-mono text-slate-500 break-all leading-relaxed select-all">
                  {{ selectedSession.userAgent }}
                </div>
              </div>
            </div>
          </div>

          <!-- AI Isolation Forest Analysis -->
          <div class="bg-purple-50/50 border border-purple-100 rounded-xl p-4 space-y-3">
            <h4 class="text-xs font-bold text-purple-900 uppercase tracking-wider flex items-center gap-1">
              <Sparkles :size="14" class="text-purple-600" /> Đánh giá Hành vi bằng AI
            </h4>
            <div class="flex items-center justify-between bg-white p-3 rounded-lg border border-purple-100">
              <span class="text-xs text-slate-600">Độ bất thường (Risk Score):</span>
              <span class="px-2.5 py-0.5 rounded text-xs font-mono font-bold border" :class="getRiskClass(selectedSession.riskScore)">
                {{ selectedSession.riskScore }}%
              </span>
            </div>
            <div class="text-xs text-slate-600 leading-relaxed bg-white/70 p-3 rounded-lg border border-purple-50">
              <p class="font-bold text-purple-800 text-[10px] uppercase mb-1">Giải trình từ mô hình:</p>
              {{ selectedSession.aiReason }}
            </div>
          </div>
        </div>

        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex gap-2">
          <button @click="openRevokeModal(selectedSession)" class="glass-btn secondary flex-1 justify-center !text-amber-600 hover:!bg-amber-50">
            <LogOut :size="14" /> Cưỡng chế Đăng xuất
          </button>
          <button @click="isDetailDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Đóng</button>
        </div>
      </div>

      <!-- Modal: Xác nhận cưỡng chế Đăng xuất (Revoke) -->
      <div v-if="isRevokeModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-sm w-full">
          <h3 class="text-lg font-bold text-heading mb-3 flex items-center gap-2 border-b border-slate-100 pb-2">
            <AlertTriangle :size="20" class="text-amber-500" /> Cưỡng chế hủy phiên?
          </h3>
          <p class="text-xs text-slate-500 mb-4 leading-relaxed">
            Hành động này sẽ xóa token xác thực hiện tại của tài khoản 
            <strong class="text-heading">{{ sessionToRevoke?.userName }}</strong> ({{ sessionToRevoke?.email }}). 
            Hệ thống sẽ ép buộc trình duyệt của người này đăng xuất ngay lập tức.
          </p>

          <div class="flex gap-3 justify-end pt-3 border-t border-slate-100">
            <button @click="isRevokeModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button 
              @click="confirmRevokeSession" 
              class="glass-btn primary flex-1 justify-center !bg-amber-600 hover:!bg-amber-700"
            >
              Hủy phiên
            </button>
          </div>
        </div>
      </div>

      <!-- Modal: Khóa Tài Khoản khẩn cấp -->
      <div v-if="isLockModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-md w-full">
          <h3 class="text-lg font-bold text-heading mb-3 flex items-center gap-2 border-b border-slate-100 pb-2">
            <UserX :size="20" class="text-rose-500" /> Khóa tài khoản khẩn cấp
          </h3>
          <p class="text-xs text-slate-500 mb-4 leading-relaxed">
            Khóa ngay lập tức mọi quyền truy cập của tài khoản 
            <strong class="text-heading">{{ accountToLock?.userName }}</strong> ({{ accountToLock?.email }}). 
            Tất cả phiên làm việc sẽ bị vô hiệu hóa.
          </p>

          <div class="form-group mb-5">
            <label class="block text-xs font-bold text-label mb-1.5 flex items-center gap-1">
              <span>Lý do khóa tài khoản (Ghi nhận log hệ thống)</span>
              <span class="text-rose-500">*</span>
            </label>
            <textarea 
              v-model="lockReason" 
              rows="3" 
              class="glass-input w-full bg-white text-xs"
            ></textarea>
          </div>

          <div class="flex gap-3 justify-end pt-3 border-t border-slate-100">
            <button @click="isLockModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button 
              @click="confirmLockAccount" 
              class="glass-btn primary flex-1 justify-center !bg-rose-600 hover:!bg-rose-700"
              :disabled="!lockReason.trim()"
            >
              Khóa tài khoản
            </button>
          </div>
        </div>
      </div>

      <!-- Modal: Cấu hình Cảnh báo bảo mật AI (Create Alert) -->
      <div v-if="isAlertModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-md w-full">
          <h3 class="text-lg font-bold text-heading mb-3 flex items-center gap-2 border-b border-slate-100 pb-2">
            <Bell :size="20" class="text-purple-600" /> Thiết lập Luật Cảnh báo AI
          </h3>
          <p class="text-xs text-slate-500 mb-4 leading-relaxed">
            Cấu hình điều kiện để mô hình AI tự động gửi cảnh báo về Telegram/Email/SMS của quản trị viên khi phát hiện truy cập bất thường.
          </p>

          <div class="space-y-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Tên quy tắc cảnh báo</label>
              <input 
                v-model="newAlertConfig.name" 
                type="text" 
                class="glass-input w-full bg-white text-xs" 
              />
            </div>
            
            <div class="grid grid-cols-2 gap-3">
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Điều kiện kích hoạt</label>
                <select v-model="newAlertConfig.triggerType" class="glass-select w-full bg-white text-xs">
                  <option value="risk_score">Điểm rủi ro AI vượt</option>
                  <option value="failed_attempts">Số lần đăng nhập lỗi liên tiếp</option>
                  <option value="ip">Từ quốc gia ngoài danh sách</option>
                </select>
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Ngưỡng trị số</label>
                <input 
                  v-model="newAlertConfig.threshold" 
                  type="number" 
                  class="glass-input w-full bg-white text-xs" 
                />
              </div>
            </div>

            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Kênh nhận cảnh báo</label>
              <select v-model="newAlertConfig.channel" class="glass-select w-full bg-white text-xs">
                <option value="email_sms">Email & SMS của Super Admin</option>
                <option value="telegram">Webhook Telegram Channel</option>
                <option value="system">Chỉ đẩy cảnh báo vào mục Security Alert</option>
              </select>
            </div>
          </div>

          <div class="flex gap-3 justify-end pt-5 border-t border-slate-100 mt-4">
            <button @click="isAlertModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button 
              @click="confirmCreateAlert" 
              class="glass-btn primary flex-1 justify-center !bg-purple-600 hover:!bg-purple-700"
            >
              Thiết lập Cảnh báo
            </button>
          </div>
        </div>
      </div>

    </Teleport>

  </div>
</template>

<style scoped>
.glass-panel {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(12px);
}

.glass-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.glass-btn.primary {
  background: var(--text-link);
  color: white;
}
.glass-btn.primary:hover { background: #5b21b6; }

.glass-btn.secondary {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
.glass-btn.secondary:hover { background: var(--surface-input-focus); }

.glass-input, .glass-select {
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  padding: 0.5rem 0.75rem;
  border-radius: 10px;
  color: var(--text-heading);
  font-size: 0.8rem;
  outline: none;
  transition: all 0.2s;
}

.glass-input:focus, .glass-select:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
  background: var(--surface-input-focus);
}

.action-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transition: all 0.2s;
}

/* Table Styles */
.table-container {
  overflow-x: auto;
}
th {
  padding: 1rem;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-label);
  background: var(--surface-input);
  border-bottom: 1px solid var(--border-default);
}
td {
  padding: 1rem;
  vertical-align: middle;
}

/* Drawer */
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  backdrop-filter: blur(2px);
  z-index: 9990;
}
.drawer {
  position: fixed;
  top: 0;
  right: -448px; /* max-w-md: 448px */
  width: 100%;
  max-width: 400px;
  height: 100vh;
  background: var(--surface-solid);
  box-shadow: -10px 0 30px rgba(0,0,0,0.1);
  z-index: 9999;
  transition: right 0.3s ease;
  display: flex;
  flex-direction: column;
}
.drawer.open {
  right: 0 !important;
}
.drawer.drawer-md {
  max-width: 448px;
  right: -448px;
}
.drawer-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--border-default);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.drawer-body {
  flex: 1;
  overflow-y: auto;
}
.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px dashed var(--border-default);
  font-size: 0.85rem;
}
.info-label {
  color: var(--text-label);
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  backdrop-filter: blur(4px);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
</style>
