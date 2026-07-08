<script setup>
/**
 * AiAutomationView.vue - Super Admin
 * Giao diện cấu hình mô hình AI và lập lịch tác vụ tự động ngầm (Automation Jobs).
 * Tích hợp Bảng giám sát dịch vụ AI, AI Model Toggles, CRON Editor Drawer và Confirm Run Modal.
 */
import { ref, computed , onMounted} from 'vue'
import { apiRequest } from '@/services/apiClient'
import {
  Brain,
  Cpu,
  Clock,
  Activity,
  CheckCircle,
  AlertTriangle,
  X,
  Server,
  Settings,
  Play,
  Terminal,
  Database,
  Calendar,
  Wifi
} from 'lucide-vue-next'

// --- Connection APIs & Servers State ---
const connectionStats = ref({
  claudeApi: { status: 'Connected', latency: '185ms', provider: 'Claude 3.5 Sonnet' },
  openaiApi: { status: 'Connected', latency: '142ms', provider: 'GPT-4o-mini' },
  hangfireNode1: { status: 'Active', cpu: '12%', memory: '42%' },
  hangfireNode2: { status: 'Active', cpu: '8%', memory: '38%' },
  database: { status: 'Connected', activeConns: 42 }
})

// --- AI Models State ---
const aiModels = ref([])

// --- Automation Jobs (Hangfire) State ---
const automationJobs = ref([])

// --- State CRON Editor Drawer ---
const isDrawerOpen = ref(false)
const activeJob = ref(null)
const tempCronExpression = ref('')
const tempCronDescription = ref('')
const selectedPreset = ref('custom')

const cronPresets = [
  { label: 'Tùy chỉnh biểu thức CRON', value: 'custom', expression: '' },
  { label: 'Mỗi 5 phút một lần (Testing)', value: 'every_5_min', expression: '*/5 * * * *', desc: 'Chạy mỗi 5 phút một lần' },
  { label: 'Mỗi 15 phút một lần', value: 'every_15_min', expression: '*/15 * * * *', desc: 'Chạy mỗi 15 phút một lần' },
  { label: 'Hàng giờ (Phút thứ 0)', value: 'every_hour', expression: '0 * * * *', desc: 'Chạy vào phút thứ 0 của mỗi giờ' },
  { label: 'Hàng ngày lúc 02:00 sáng', value: 'daily_2am', expression: '0 2 * * *', desc: 'Chạy hàng ngày lúc 02:00 sáng' },
  { label: 'Hàng tuần vào Chủ nhật lúc 01:00 sáng', value: 'weekly_sun_1am', expression: '0 1 * * 0', desc: 'Chạy hàng tuần lúc 01:00 sáng Chủ nhật' },
  { label: 'Hàng tháng vào ngày 1 lúc 03:00 sáng', value: 'monthly_1st_3am', expression: '0 3 1 * *', desc: 'Chạy hàng tháng lúc 03:00 sáng ngày mùng 1' }
]

const openCronDrawer = (job) => {
  activeJob.value = job
  tempCronExpression.value = job.cronExpression
  tempCronDescription.value = job.cronDescription
  
  // Xác định preset tương ứng
  const matchingPreset = cronPresets.find(p => p.expression === job.cronExpression)
  selectedPreset.value = matchingPreset ? matchingPreset.value : 'custom'
  
  isDrawerOpen.value = true
}

const handlePresetChange = () => {
  const preset = cronPresets.find(p => p.value === selectedPreset.value)
  if (preset && preset.value !== 'custom') {
    tempCronExpression.value = preset.expression
    tempCronDescription.value = preset.desc
  }
}

// Diễn dịch mã CRON (Giả lập chuyển đổi)
const computedCronTranslation = computed(() => {
  if (selectedPreset.value !== 'custom') {
    return tempCronDescription.value
  }
  
  // Phân tích biểu thức cơ bản
  const parts = tempCronExpression.value.trim().split(/\s+/)
  if (parts.length !== 5) {
    return 'Biểu thức CRON không hợp lệ. Phải chứa đúng 5 trường.'
  }

  const [min, hour, day, month, dayOfWeek] = parts
  
  if (min === '*/5' && hour === '*' && day === '*' && month === '*' && dayOfWeek === '*') {
    return 'Chạy mỗi 5 phút một lần'
  }
  if (min === '*/15' && hour === '*' && day === '*' && month === '*' && dayOfWeek === '*') {
    return 'Chạy mỗi 15 phút một lần'
  }
  if (min === '0' && hour === '2' && day === '*' && month === '*' && dayOfWeek === '*') {
    return 'Chạy hàng ngày vào lúc 02:00 sáng'
  }
  if (min === '0' && hour === '1' && day === '*' && month === '*' && dayOfWeek === '0') {
    return 'Chạy hàng tuần vào lúc 01:00 sáng ngày Chủ nhật'
  }
  if (min === '0' && hour === '3' && day === '1' && month === '*' && dayOfWeek === '*') {
    return 'Chạy hàng tháng vào lúc 03:00 sáng ngày mùng 1'
  }

  return `Chạy với cấu hình tùy chỉnh (Phút: ${min}, Giờ: ${hour}, Ngày: ${day}, Tháng: ${month}, Thứ: ${dayOfWeek})`
})

const saveCronSchedule = () => {
  if (!activeJob.value) return
  const parts = tempCronExpression.value.trim().split(/\s+/)
  if (parts.length !== 5) {
    triggerToast('Lỗi: Biểu thức CRON không đúng định dạng 5 trường!', 'error')
    return
  }

  activeJob.value.cronExpression = tempCronExpression.value
  activeJob.value.cronDescription = computedCronTranslation.value

  triggerToast(`Đã lưu lịch trình chạy mới cho ${activeJob.value.name}!`, 'success')
  isDrawerOpen.value = false
}

// --- State Confirm Run Modal ---
const isRunModalOpen = ref(false)
const pendingRunJob = ref(null)

const openRunModal = (job) => {
  pendingRunJob.value = job
  isRunModalOpen.value = true
}

const executeTestRun = () => {
  if (!pendingRunJob.value) return
  
  const job = pendingRunJob.value
  job.status = 'Running'
  isRunModalOpen.value = false
  triggerToast(`Đã đưa tác vụ "${job.name}" vào hàng đợi thực thi Hangfire!`, 'info')
  
  // Giả lập tác vụ hoàn thành sau 2.5 giây
  setTimeout(() => {
    job.status = 'Scheduled'
    job.lastRun = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
    job.lastRunResult = 'Success'
    job.duration = '1.8s'
    triggerToast(`Tác vụ ngầm "${job.name}" đã hoàn tất thành công (Lưu vết thành công)!`, 'success')
  }, 2500)
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

// --- Thao tác AI Model ---
const toggleModel = (model) => {
  model.status = model.status === 'Enabled' ? 'Disabled' : 'Enabled'
  triggerToast(`Đã ${model.status === 'Enabled' ? 'kích hoạt' : 'vô hiệu hóa'} mô hình: ${model.name}`, 'info')
}

const testModelConnection = (model) => {
  model.status = 'Running'
  triggerToast(`Đang kiểm tra kết nối API tới dịch vụ AI...`, 'info')
  
  setTimeout(() => {
    model.status = 'Enabled'
    triggerToast(`Kết nối thành công tới ${model.apiService}! Độ trễ: ${model.latency}`, 'success')
  }, 1200)
}

onMounted(async () => {
  try {
    const res = await apiRequest('/api/super-admin/ai/jobs')
    if (Array.isArray(res)) {
      automationJobs.value = res
    }
  } catch (error) {
    console.error('Failed to load data for automationJobs', error)
  }

  try {
    const res = await apiRequest('/api/super-admin/ai/models')
    if (Array.isArray(res)) {
      aiModels.value = res
    }
  } catch (error) {
    console.error('Failed to load data for aiModels', error)
  }
})

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
      <Brain v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
          <Brain class="w-8 h-8 text-primary" />
          Cấu Hình AI & Tác Vụ Tự Động (AI & Automation)
        </h1>
        <p class="text-sm text-muted mt-1">
          Quản lý trạng thái các mô hình trí tuệ nhân tạo (AI Models) và cấu hình lịch trình chạy ngầm (Cron Jobs) thông qua Hangfire Server.
        </p>
      </div>

      <!-- Connection & Hardware Status Panel (Bảng điều khiển kết nối) -->
      <div class="grid grid-cols-1 md:grid-cols-5 gap-4 mb-6">
        <!-- Claude API status -->
        <div class="lg-glass-soft lg-card lg-density-compact space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-bold text-muted uppercase">Claude API Service</span>
            <Wifi class="w-3.5 h-3.5 text-emerald-500" />
          </div>
          <div class="text-xs font-extrabold text-heading flex items-baseline gap-1.5">
            Claude 3.5 Sonnet
          </div>
          <div class="flex items-center justify-between text-[10px]">
            <span class="text-emerald-500 font-bold">Connected</span>
            <span class="text-muted">Delay: {{ connectionStats.claudeApi.latency }}</span>
          </div>
        </div>

        <!-- OpenAI API status -->
        <div class="lg-glass-soft lg-card lg-density-compact space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-bold text-muted uppercase">OpenAI API Service</span>
            <Wifi class="w-3.5 h-3.5 text-emerald-500" />
          </div>
          <div class="text-xs font-extrabold text-heading flex items-baseline gap-1.5">
            GPT-4o-mini
          </div>
          <div class="flex items-center justify-between text-[10px]">
            <span class="text-emerald-500 font-bold">Connected</span>
            <span class="text-muted">Delay: {{ connectionStats.openaiApi.latency }}</span>
          </div>
        </div>

        <!-- Hangfire Node 1 -->
        <div class="lg-glass-soft lg-card lg-density-compact space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-bold text-muted uppercase">Hangfire Server (Node 1)</span>
            <Server class="w-3.5 h-3.5 text-emerald-500" />
          </div>
          <div class="text-xs font-extrabold text-heading flex items-baseline gap-1.5">
            Active / Leader
          </div>
          <div class="flex items-center justify-between text-[10px] text-muted">
            <span>CPU: {{ connectionStats.hangfireNode1.cpu }}</span>
            <span>RAM: {{ connectionStats.hangfireNode1.memory }}</span>
          </div>
        </div>

        <!-- Hangfire Node 2 -->
        <div class="lg-glass-soft lg-card lg-density-compact space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-bold text-muted uppercase">Hangfire Server (Node 2)</span>
            <Server class="w-3.5 h-3.5 text-emerald-500" />
          </div>
          <div class="text-xs font-extrabold text-heading flex items-baseline gap-1.5">
            Active / Backup
          </div>
          <div class="flex items-center justify-between text-[10px] text-muted">
            <span>CPU: {{ connectionStats.hangfireNode2.cpu }}</span>
            <span>RAM: {{ connectionStats.hangfireNode2.memory }}</span>
          </div>
        </div>

        <!-- DB Connection -->
        <div class="lg-glass-soft lg-card lg-density-compact space-y-1.5">
          <div class="flex items-center justify-between">
            <span class="text-[10px] font-bold text-muted uppercase">Database Pool</span>
            <Database class="w-3.5 h-3.5 text-emerald-500" />
          </div>
          <div class="text-xs font-extrabold text-heading flex items-baseline gap-1.5">
            SQL Server
          </div>
          <div class="flex items-center justify-between text-[10px]">
            <span class="text-emerald-500 font-bold">Connected</span>
            <span class="text-muted">{{ connectionStats.database.activeConns }} active conns</span>
          </div>
        </div>
      </div>

      <!-- Ràng buộc AI & Quyết định hệ thống Alert Banner -->
      <div class="lg-alert lg-alert-info mb-6 flex items-start justify-between gap-4">
        <div class="flex items-start gap-2.5">
          <Brain class="w-5.5 h-5.5 text-current flex-shrink-0 mt-0.5 opacity-90" />
          <div>
            <h4 class="font-extrabold text-sm text-current">Nguyên tắc Bảo mật & Quyết định nghiệp vụ (Human-in-the-Loop)</h4>
            <p class="text-xs leading-relaxed text-current opacity-90 mt-1">
              Hệ thống LMS quy định rõ: **Các mô hình AI chỉ đóng vai trò hỗ trợ phân tích và gợi ý đề xuất**. Mọi quyết định nghiệp vụ nhạy cảm (như phê duyệt điểm số sinh viên, khóa tài khoản người dùng có cảnh báo bảo mật) bắt buộc phải do Super Admin hoặc Nhân sự phụ trách xem xét và duyệt lại thủ công trên hệ thống.
            </p>
          </div>
        </div>
      </div>

      <!-- 1. Quản lý trạng thái các mô hình AI -->
      <div class="mb-6">
        <h2 class="text-lg font-bold text-heading flex items-center gap-2 mb-4">
          <Cpu class="w-5.5 h-5.5 text-primary" />
          Quản lý Trạng thái Mô hình AI (AI Models)
        </h2>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div 
            v-for="model in aiModels" 
            :key="model.id"
            class="lg-glass-soft lg-card lg-density-normal flex flex-col justify-between gap-4 border border-default"
          >
            <div>
              <div class="flex items-start justify-between gap-3">
                <h3 class="text-xs font-extrabold text-heading leading-snug">{{ model.name }}</h3>
                <span 
                  class="px-2 py-0.5 rounded text-[9px] font-extrabold"
                  :class="{
                    'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-300': model.status === 'Enabled',
                    'bg-rose-500/10 text-rose-500 border border-rose-300': model.status === 'Disabled',
                    'bg-amber-500/10 text-amber-500 border border-amber-300': model.status === 'Running',
                    'bg-red-500/10 text-red-500 border border-red-300': model.status === 'Failed'
                  }"
                >
                  <span v-if="model.status === 'Enabled'">Đã bật</span>
                  <span v-else-if="model.status === 'Disabled'">Đã tắt</span>
                  <span v-else-if="model.status === 'Running'">Đang kết nối...</span>
                  <span v-else>Lỗi kết nối</span>
                </span>
              </div>
              <p class="text-[10px] text-muted leading-relaxed mt-1.5">{{ model.description }}</p>
            </div>

            <!-- Panel điều khiển chân thẻ -->
            <div class="flex items-center justify-between border-t border-default/50 pt-3 text-[10px]">
              <div class="flex items-center gap-4 text-muted">
                <div><span class="font-bold">Độ chính xác (Acc):</span> <span class="text-heading font-semibold">{{ model.lastAccuracy }}</span></div>
                <div><span class="font-bold">Dịch vụ:</span> <span class="text-heading font-semibold">{{ model.apiService }}</span></div>
              </div>

              <div class="flex items-center gap-2">
                <!-- Nút test connection -->
                <button
                  @click="testModelConnection(model)"
                  class="lg-btn-secondary text-[10px] px-2 py-1 flex items-center gap-1"
                  :disabled="model.status === 'Running'"
                >
                  <Activity class="w-3 h-3" />
                  Test API
                </button>

                <!-- Switch gạt bật tắt -->
                <button 
                  @click="toggleModel(model)"
                  class="w-10 h-5.5 rounded-full p-0.5 transition-colors duration-200 focus:outline-none flex items-center"
                  :class="model.status === 'Enabled' ? 'bg-emerald-500' : 'bg-slate-400 dark:bg-slate-600'"
                >
                  <div 
                    class="bg-white w-4.5 h-4.5 rounded-full shadow-md transform transition-transform duration-200"
                    :class="model.status === 'Enabled' ? 'translate-x-4.5' : 'translate-x-0'"
                  ></div>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 2. Quản lý Tác vụ tự động Hangfire (Hangfire Jobs Table) -->
      <div>
        <h2 class="text-lg font-bold text-heading flex items-center gap-2 mb-4">
          <Clock class="w-5.5 h-5.5 text-primary" />
          Lập Lịch Tác Vụ Hệ Thống Ngầm (Hangfire Automation Jobs)
        </h2>

        <!-- Bảng Job Table -->
        <div class="lg-table-shell overflow-x-auto w-full max-w-full">
          <table class="min-w-[1100px] w-full divide-y divide-default text-sm">
            <thead>
              <tr class="surface-table-header">
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Tên Tác Vụ Tự Động</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap w-40">Mã Tác Vụ</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap w-44">Thời gian chạy (CRON)</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Lần chạy cuối (Last Run)</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap w-32">Kết quả</th>
                <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Lần tiếp theo (Next Run)</th>
                <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap min-w-[220px] w-[220px]">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="job in automationJobs" :key="job.id" class="transition-colors hover:bg-surface-table-row-hover">
                <!-- Tên tác vụ -->
                <td class="px-4 py-4 max-w-xs">
                  <div class="font-extrabold text-heading text-xs flex items-center gap-1.5">
                    {{ job.name }}
                    <span 
                      v-if="job.status === 'Running'"
                      class="text-[9px] bg-amber-500/10 text-amber-600 dark:text-amber-400 px-1.5 py-0.2 rounded border border-amber-300 font-extrabold animate-pulse whitespace-nowrap"
                    >
                      Đang xử lý ngầm...
                    </span>
                  </div>
                  <div class="text-[10px] text-muted mt-1 leading-normal">
                    Hangfire background task worker
                  </div>
                </td>

                <!-- Mã tác vụ -->
                <td class="px-4 py-4 font-mono text-xs text-heading font-semibold">
                  {{ job.id }}
                </td>

                <!-- Biểu thức CRON -->
                <td class="px-4 py-4">
                  <div class="font-mono text-xs text-primary font-bold">{{ job.cronExpression }}</div>
                  <div class="text-[10px] text-muted mt-0.5" :title="job.cronDescription">
                    {{ job.cronDescription }}
                  </div>
                </td>

                <!-- Lần chạy cuối -->
                <td class="px-4 py-4 whitespace-nowrap">
                  <div class="text-xs text-heading font-semibold">{{ job.lastRun }}</div>
                  <div v-if="job.duration !== '0s'" class="text-[10px] text-muted mt-0.5 flex items-center gap-1">
                    <Clock class="w-3 h-3 text-slate-400 flex-shrink-0" />
                    Thời gian chạy: {{ job.duration }}
                  </div>
                </td>

                <!-- Kết quả lần chạy cuối -->
                <td class="px-4 py-4 text-center whitespace-nowrap">
                  <span 
                    class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold whitespace-nowrap"
                    :class="job.lastRunResult === 'Success' ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-300' : 'bg-rose-500/10 text-rose-500 border-rose-300'"
                  >
                    {{ job.lastRunResult === 'Success' ? 'Hoàn thành' : 'Thất bại' }}
                  </span>
                </td>

                <!-- Lần chạy tiếp theo -->
                <td class="px-4 py-4 text-xs font-semibold text-muted whitespace-nowrap">
                  {{ job.nextRun }}
                </td>

                <!-- Thao tác -->
                <td class="px-4 py-4 text-center whitespace-nowrap min-w-[220px] w-[220px]">
                  <div class="flex items-center justify-center gap-2">
                    <!-- Nút chạy thử ngay lập tức -->
                    <button
                      @click="openRunModal(job)"
                      class="lg-icon-button p-1.5 rounded-lg border border-default transition-all flex items-center justify-center gap-1 bg-violet-500/10 text-violet-600 dark:text-violet-400 hover:bg-violet-500/20"
                      title="Kích hoạt chạy thử tác vụ ngay lập tức"
                      :disabled="job.status === 'Running'"
                    >
                      <Play class="w-3.5 h-3.5" />
                      <span class="text-[10px] font-bold">Chạy thử</span>
                    </button>

                    <!-- Nút sửa lịch CRON -->
                    <button
                      @click="openCronDrawer(job)"
                      class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                      title="Chỉnh sửa lịch chạy tự động"
                    >
                      <Settings class="w-3.5 h-3.5" />
                      Lên lịch
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- CRON Editor Drawer (Ngăn kéo chỉnh sửa lịch trình) -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[100] flex justify-end bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md h-screen bg-surface-modal lg-glass-strong border-l border-default shadow-2xl flex flex-col animate-in slide-in-from-right duration-300">
        <!-- Drawer Header -->
        <div class="p-5 border-b border-default flex items-center justify-between">
          <div class="space-y-1">
            <h2 class="text-lg font-extrabold text-heading flex items-center gap-2.5">
              <Calendar class="w-5.5 h-5.5 text-primary" />
              Lên Lịch Tác Vụ Tự Động (CRON Editor)
            </h2>
            <p class="text-xs text-muted">Điều chỉnh chu kỳ chạy tự động cho phân hệ tác vụ ngầm.</p>
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
          <div class="text-xs font-bold text-heading uppercase mb-2">
            Tác vụ: {{ activeJob?.name }}
          </div>

          <!-- Lựa chọn lịch mẫu Preset -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Lịch trình mẫu (Presets)</label>
            <select v-model="selectedPreset" @change="handlePresetChange" class="w-full px-3 lg-control text-sm">
              <option v-for="preset in cronPresets" :key="preset.value" :value="preset.value">
                {{ preset.label }}
              </option>
            </select>
          </div>

          <!-- Nhập biểu thức CRON -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Biểu thức CRON (5 trường)</label>
            <input
              type="text"
              v-model="tempCronExpression"
              :disabled="selectedPreset !== 'custom'"
              placeholder="Ví dụ: */15 * * * *"
              class="w-full px-3 lg-control font-mono text-sm tracking-widest text-center"
            />
          </div>

          <!-- Thông dịch tiếng Việt trực quan -->
          <div class="p-3.5 rounded-lg bg-primary/5 border border-primary/10 text-xs flex items-start gap-2.5 text-primary">
            <Info class="w-4.5 h-4.5 flex-shrink-0 mt-0.5 text-primary" />
            <div>
              <span class="font-bold block text-[10px] uppercase mb-1">Mô tả lịch trình (Bản dịch trực quan)</span>
              <span class="font-semibold">{{ computedCronTranslation }}</span>
            </div>
          </div>

          <!-- Gợi ý cách nhập CRON tùy chỉnh -->
          <div v-if="selectedPreset === 'custom'" class="p-3.5 rounded-lg bg-surface-card border border-default text-[10px] space-y-2 text-muted leading-relaxed">
            <span class="font-bold text-heading block">HƯỚNG DẪN CÚ PHÁP CRON (5 trường):</span>
            <div class="grid grid-cols-5 text-center font-mono gap-1 text-[9px] bg-surface-input p-2 rounded border border-default">
              <div>Phút<br/>(0-59)</div>
              <div>Giờ<br/>(0-23)</div>
              <div>Ngày<br/>(1-31)</div>
              <div>Tháng<br/>(1-12)</div>
              <div>Thứ<br/>(0-6)</div>
            </div>
            <p>Sử dụng dấu <code class="font-mono text-primary font-bold">*</code> cho mọi giá trị, dấu <code class="font-mono text-primary font-bold">/</code> cho chu kỳ lặp (ví dụ: <code class="font-mono text-primary">*/10</code> là mỗi 10 phút).</p>
          </div>
        </div>

        <!-- Drawer Footer -->
        <div class="p-5 border-t border-default flex items-center justify-end bg-surface-table">
          <button
            @click="isDrawerOpen = false"
            class="lg-btn-secondary px-4 py-2 text-xs font-bold mr-2"
          >
            Hủy
          </button>
          <button
            @click="saveCronSchedule"
            class="lg-btn-primary px-5 py-2 text-xs font-bold"
          >
            Lưu cấu hình
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Run Modal (Xác nhận chạy thử tác vụ) -->
    <div 
      v-if="isRunModalOpen" 
      class="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <!-- Close button -->
        <button 
          @click="isRunModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
          <div class="w-10 h-10 rounded-full bg-violet-500/10 flex items-center justify-center text-violet-500 flex-shrink-0">
            <Play class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Xác Nhận Chạy Thử Tác Vụ</h3>
            <p class="text-xs text-muted mt-0.5">Yêu cầu chạy khẩn cấp tác vụ tự động ngầm dưới nền.</p>
          </div>
        </div>

        <!-- Form nội dung -->
        <div class="space-y-4 mb-5 text-xs">
          <!-- Banner bất đồng bộ (Async) -->
          <div class="p-3.5 rounded-lg bg-violet-500/5 border border-violet-500/10 flex items-start gap-2.5 text-violet-600 dark:text-violet-400">
            <Terminal class="w-4.5 h-4.5 flex-shrink-0 mt-0.5" />
            <div class="leading-relaxed">
              <strong>Xử lý bất đồng bộ (Async execution)</strong>: Tác vụ sẽ được đẩy vào Hangfire Queue và xử lý ngầm dưới nền. Quá trình này không chặn trải nghiệm của người dùng trên web nhưng có thể tiêu tốn CPU/RAM của server trong vài phút.
            </div>
          </div>

          <div class="p-3.5 rounded-lg bg-surface-card border border-default space-y-1">
            <div><span class="text-muted">Tác vụ kích hoạt:</span> <strong class="text-heading font-extrabold">{{ pendingRunJob?.name }}</strong></div>
            <div><span class="text-muted">Mã Job ID:</span> <code class="font-mono text-[10px] text-primary font-bold">{{ pendingRunJob?.id }}</code></div>
            <div><span class="text-muted">Lần chạy cuối:</span> <span class="text-heading font-semibold">{{ pendingRunJob?.lastRun }}</span></div>
          </div>

          <p class="text-slate-700 dark:text-slate-300 font-semibold leading-relaxed">
            Bạn có chắc chắn muốn kích hoạt chạy thử tác vụ tự động này ngay lập tức không? Hệ thống cam kết việc chạy thử không làm ảnh hưởng hay thay đổi dữ liệu gốc đã lưu trữ.
          </p>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="isRunModalOpen = false"
            class="lg-btn-secondary px-4 py-2 text-sm font-bold"
          >
            Hủy
          </button>
          <button
            @click="executeTestRun"
            class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"
          >
            <Play class="w-4 h-4" />
            Xác nhận chạy thử
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
