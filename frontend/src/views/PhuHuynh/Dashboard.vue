<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  AlertTriangle,
  Award,
  Calendar,
  CheckCircle,
  CreditCard,
  ChevronRight,
  User,
  GraduationCap,
  Clock,
  MapPin,
  DollarSign,
  ArrowUpRight,
  MessageSquare,
  ShieldAlert,
  Sparkles,
  ChevronDown,
  Check
} from 'lucide-vue-next'

const router = useRouter()

import { childrenData } from '@/components/PhuHuynh/data/parentData.js'

const activeChildId = ref(Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const children = computed(() => {
  return childrenData.map(c => ({
    ...c,
    gpaTrend: c.gpaTrendText,
    tuitionDebt: c.balanceTuition,
    tuitionStatus: c.balanceTuition > 0 ? (c.isOverdue ? 'Quá hạn đóng' : 'Chờ thanh toán') : 'Đã hoàn thành',
    schedule: c.id === 1 ? [
      { id: 1, subject: 'Cấu trúc dữ liệu & Giải thuật', room: 'Phòng 201 - Nhà Beta', teacher: 'TS. Trần Thị B', time: '07:30 - 09:30', status: 'finished' },
      { id: 2, subject: 'Toán cao cấp', room: 'Phòng 105 - Nhà Gamma', teacher: 'ThS. Lê Văn C', time: '09:30 - 11:30', status: 'active' },
      { id: 3, subject: 'Lập trình Web nâng cao', room: 'Phòng 402 - Nhà Alpha', teacher: 'ThS. Nguyễn Văn A', time: '12:30 - 14:30', status: 'upcoming' },
    ] : [
      { id: 1, subject: 'An toàn thông tin mạng', room: 'Phòng 301 - Nhà Alpha', teacher: 'TS. Nguyễn Hoàng G', time: '07:30 - 09:30', status: 'finished' },
      { id: 2, subject: 'Phát triển ứng dụng di động', room: 'Phòng Lab 2 - Nhà Beta', teacher: 'ThS. Vũ Thị H', time: '09:30 - 11:30', status: 'upcoming' },
    ],
    alerts: c.warnings.filter(w => !w.confirmed).map(w => ({
      id: w.id,
      type: w.type,
      message: `${w.reason} (${w.subject})`,
      time: w.date
    })),
    gradesProgress: c.id === 1 ? [
      { semester: 'Kỳ 1 - Block 1', gpa: 7.8 },
      { semester: 'Kỳ 1 - Block 2', gpa: 8.0 },
      { semester: 'Kỳ 2 - Block 1', gpa: 8.2 },
      { semester: 'Kỳ 2 - Block 2', gpa: 8.4 },
    ] : [
      { semester: 'Kỳ 1 - Block 1', gpa: 8.9 },
      { semester: 'Kỳ 1 - Block 2', gpa: 9.0 },
      { semester: 'Kỳ 2 - Block 1', gpa: 9.0 },
      { semester: 'Kỳ 2 - Block 2', gpa: 9.1 },
    ]
  }))
})

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0]
})

function selectChild(id) {
  activeChildId.value = id
  localStorage.setItem('parent_active_student_id', id)
  dropdownOpen.value = false
}

// ── Định dạng tiền tệ ──
function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

// ── Hệ thống thông báo chung ──
const systemNotifications = ref([
  { id: 1, title: 'Đăng ký học kỳ phụ', content: 'Cổng đăng ký môn học học kỳ phụ sẽ mở từ ngày 15/06/2026. Phụ huynh lưu ý lịch trình học tập của con.', time: '2 giờ trước', isNew: true },
  { id: 2, title: 'Lịch nghỉ lễ sắp tới', content: 'Nhà trường thông báo lịch nghỉ học các lớp buổi tối và cuối tuần dịp lễ tổng kết năm học.', time: '1 ngày trước', isNew: false },
  { id: 3, title: 'Khảo sát giảng dạy', content: 'Kính mong phụ huynh nhắc nhở con hoàn thành khảo sát chất lượng giảng dạy trên cổng thông tin trước ngày 12/06.', time: '3 ngày trước', isNew: false }
])

function navigateTo(path) {
  router.push(path)
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── CHỌN HỌC SINH (CHILD SELECTOR) ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-5 p-5 lg-glass-soft rounded-[24px]">
      <div class="flex items-center gap-4">
        <div class="h-12 w-12 flex items-center justify-center rounded-[14px] bg-orange-100 dark:bg-orange-950/30 text-orange-600 shadow-sm border border-orange-200/50 dark:border-orange-900/50">
          <GraduationCap :size="24" stroke-width="2.5" />
        </div>
        <div>
          <h2 class="text-base font-bold text-heading tracking-tight">Hồ sơ theo dõi con em</h2>
          <p class="text-xs font-medium text-muted mt-0.5">Chọn tài khoản của con để xem báo cáo học tập & học phí</p>
        </div>
      </div>

      <!-- Custom Dropdown Selector -->
      <div class="relative min-w-[260px]">
        <button
          type="button"
          class="lg-input flex w-full items-center justify-between gap-3 rounded-[16px] px-4 py-3 text-sm font-bold text-heading shadow-[var(--lg-shadow-sm)] transition-all hover:bg-[var(--surface-input)] focus:outline-none focus:ring-2 focus:ring-orange-500/30"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2.5">
            <div class="h-7 w-7 flex items-center justify-center rounded-full bg-orange-600 text-[11px] font-bold text-white shadow-sm">
              {{ currentChild.avatarInitials }}
            </div>
            <span>{{ currentChild.name }} ({{ currentChild.class }})</span>
          </div>
          <ChevronDown :size="16" class="text-muted transition-transform duration-300" :class="dropdownOpen ? 'rotate-180' : ''" />
        </button>

        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 translate-y-2 scale-95"
          enter-to-class="opacity-100 translate-y-0 scale-100"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0 scale-100"
          leave-to-class="opacity-0 translate-y-2 scale-95"
        >
          <div
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-[16px] border border-card p-1.5 shadow-[var(--lg-shadow-lg)] backdrop-blur-xl"
          >
            <button
              v-for="child in children"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-xl px-3 py-2.5 text-left text-sm font-medium transition hover:bg-[var(--surface-card-hover)] hover:text-orange-600 focus:outline-none"
              @click="selectChild(child.id)"
            >
              <div class="flex items-center gap-3">
                <div class="h-7 w-7 flex items-center justify-center rounded-full bg-orange-100 dark:bg-orange-950/40 text-[10px] font-bold text-orange-600 border border-orange-200 dark:border-orange-900/50">
                  {{ child.avatarInitials }}
                </div>
                <div>
                  <span class="block font-bold text-heading leading-tight">{{ child.name }}</span>
                  <span class="block text-[11px] font-medium text-muted mt-0.5">{{ child.studentId }} | {{ child.class }}</span>
                </div>
              </div>
              <Check v-if="child.id === activeChildId" :size="16" class="text-orange-600" stroke-width="3" />
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- Cảnh báo nợ học phí quá hạn ở Dashboard -->
    <div
      v-if="currentChild.isOverdue"
      class="p-4 rounded-xl border border-red-200 dark:border-red-950/20 bg-red-50/50 dark:bg-red-950/10 flex gap-3 animate-pulse cursor-pointer"
      @click="navigateTo('/parent/finance/tuition')"
    >
      <AlertTriangle :size="20" class="text-red-500 flex-shrink-0 mt-0.5" />
      <div class="text-xs text-body space-y-1">
        <p class="font-extrabold text-red-600 dark:text-red-400">CẢNH BÁO QUÁ HẠN THANH TOÁN HỌC PHÍ</p>
        <p class="text-slate-600 dark:text-slate-400 leading-relaxed font-semibold">
          Học phí của con đang bị trễ hạn (Hạn chót: <strong>{{ currentChild.deadlineTuition }}</strong>). 
          Số tiền còn nợ: <strong class="text-red-600 dark:text-red-400">{{ formatCurrency(currentChild.tuitionDebt) }}</strong>.
          Vui lòng bấm vào đây để thanh toán trực tuyến ngay.
        </p>
      </div>
    </div>

    <!-- ── KEY CARDS GRID (5 MAIN METRICS) ── -->
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-3 sm:gap-4">
      
      <!-- Card 1: Điểm học tập -->
      <div class="lg-glass-soft hover:scale-[1.02] hover:-translate-y-1 transition-all duration-300 flex flex-col p-5 relative overflow-hidden group rounded-[20px] shadow-[var(--lg-shadow-sm)] hover:shadow-[var(--lg-shadow-lg)]">
        <div class="absolute -right-6 -bottom-6 opacity-5 text-orange-600 group-hover:scale-110 transition-transform duration-500">
          <Award :size="90" />
        </div>
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm font-bold text-muted">Điểm học tập</span>
          <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm border border-orange-200/30">
            <Award :size="20" stroke-width="2" />
          </div>
        </div>
        <div class="mt-auto">
          <div class="flex items-baseline gap-1.5">
            <span class="text-3xl font-extrabold text-heading tracking-tight">{{ currentChild.gpa }}</span>
            <span class="text-xs font-semibold text-muted">/10</span>
          </div>
          <p class="text-[11px] font-bold text-emerald-600 flex items-center gap-1.5 mt-2 bg-emerald-50 dark:bg-emerald-950/30 px-2 py-0.5 rounded-full w-fit">
            <Sparkles :size="12" /> {{ currentChild.gpaTrend }}
          </p>
        </div>
        <div class="border-t border-card mt-4 pt-3 text-right">
          <button @click="navigateTo('/parent/learning/grades')" class="text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2 inline-flex items-center gap-1">
            Chi tiết bảng điểm <ChevronRight :size="12" />
          </button>
        </div>
      </div>

      <!-- Card 2: Chuyên cần -->
      <div class="lg-glass-soft hover:scale-[1.02] hover:-translate-y-1 transition-all duration-300 flex flex-col p-5 relative overflow-hidden group rounded-[20px] shadow-[var(--lg-shadow-sm)] hover:shadow-[var(--lg-shadow-lg)]">
        <div class="absolute -right-6 -bottom-6 opacity-5 text-orange-600 group-hover:scale-110 transition-transform duration-500">
          <CheckCircle :size="90" />
        </div>
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm font-bold text-muted">Chuyên cần</span>
          <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm border border-orange-200/30">
            <CheckCircle :size="20" stroke-width="2" />
          </div>
        </div>
        <div class="mt-auto">
          <div class="flex items-baseline gap-1">
            <span class="text-3xl font-extrabold text-heading tracking-tight">{{ currentChild.attendanceRate }}%</span>
          </div>
          <p class="text-[11px] font-semibold text-muted mt-2">
            Vắng: <span class="text-orange-600 font-bold px-1">{{ currentChild.absences }} buổi</span>
          </p>
        </div>
        <div class="border-t border-card mt-4 pt-3 text-right">
          <button @click="navigateTo('/parent/learning/attendance')" class="text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2 inline-flex items-center gap-1">
            Nhật ký điểm danh <ChevronRight :size="12" />
          </button>
        </div>
      </div>

      <!-- Card 3: Công nợ học phí -->
      <div class="lg-glass-soft hover:scale-[1.02] hover:-translate-y-1 transition-all duration-300 flex flex-col p-5 relative overflow-hidden group rounded-[20px] shadow-[var(--lg-shadow-sm)] hover:shadow-[var(--lg-shadow-lg)]">
        <div class="absolute -right-6 -bottom-6 opacity-5 text-orange-600 group-hover:scale-110 transition-transform duration-500">
          <CreditCard :size="90" />
        </div>
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm font-bold text-muted">Công nợ học phí</span>
          <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm border border-orange-200/30">
            <CreditCard :size="20" stroke-width="2" />
          </div>
        </div>
        <div class="mt-auto">
          <div class="flex items-baseline gap-1">
            <span class="text-2xl font-extrabold text-heading tracking-tight truncate">{{ formatCurrency(currentChild.tuitionDebt) }}</span>
          </div>
          <p class="text-[11px] font-bold mt-2" :class="currentChild.tuitionDebt > 0 ? 'text-orange-600 bg-orange-50 dark:bg-orange-950/30 px-2 py-0.5 rounded-full w-fit' : 'text-emerald-600 bg-emerald-50 dark:bg-emerald-950/30 px-2 py-0.5 rounded-full w-fit'">
            {{ currentChild.tuitionStatus }}
          </p>
        </div>
        <div class="border-t border-card mt-4 pt-3 text-right">
          <button @click="navigateTo('/parent/finance/tuition')" class="text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2 inline-flex items-center gap-1">
            Thanh toán học phí <ChevronRight :size="12" />
          </button>
        </div>
      </div>

      <!-- Card 4: Cảnh báo mới -->
      <div class="lg-glass-soft hover:scale-[1.02] hover:-translate-y-1 transition-all duration-300 flex flex-col p-5 relative overflow-hidden group rounded-[20px] shadow-[var(--lg-shadow-sm)] hover:shadow-[var(--lg-shadow-lg)]">
        <div class="absolute -right-6 -bottom-6 opacity-5 text-orange-600 group-hover:scale-110 transition-transform duration-500">
          <AlertTriangle :size="90" />
        </div>
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm font-bold text-muted">Cảnh báo mới</span>
          <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm border border-orange-200/30">
            <AlertTriangle :size="20" stroke-width="2" />
          </div>
        </div>
        <div class="mt-auto">
          <div class="flex items-baseline gap-1.5">
            <span class="text-3xl font-extrabold text-heading tracking-tight" :class="currentChild.alerts.length > 0 ? 'text-red-500' : 'text-emerald-600'">
              {{ currentChild.alerts.length }}
            </span>
            <span class="text-xs font-semibold text-muted">tin mới</span>
          </div>
          <p class="text-[11px] font-semibold text-muted mt-2 truncate">
            {{ currentChild.alerts[0]?.message || 'Không có cảnh báo học tập' }}
          </p>
        </div>
        <div class="border-t border-card mt-4 pt-3 text-right">
          <button @click="navigateTo('/parent/learning/alerts')" class="text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2 inline-flex items-center gap-1">
            Xem tất cả cảnh báo <ChevronRight :size="12" />
          </button>
        </div>
      </div>

      <!-- Card 5: Lớp học hôm nay -->
      <div class="lg-glass-soft hover:scale-[1.02] hover:-translate-y-1 transition-all duration-300 flex flex-col p-5 relative overflow-hidden group rounded-[20px] shadow-[var(--lg-shadow-sm)] hover:shadow-[var(--lg-shadow-lg)]">
        <div class="absolute -right-6 -bottom-6 opacity-5 text-orange-600 group-hover:scale-110 transition-transform duration-500">
          <Calendar :size="90" />
        </div>
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm font-bold text-muted">Lịch học hôm nay</span>
          <div class="h-10 w-10 flex items-center justify-center rounded-xl bg-orange-50 dark:bg-orange-950/20 text-orange-600 shadow-sm border border-orange-200/30">
            <Calendar :size="20" stroke-width="2" />
          </div>
        </div>
        <div class="mt-auto">
          <div class="flex items-baseline gap-1.5">
            <span class="text-3xl font-extrabold text-heading tracking-tight">{{ currentChild.schedule.length }}</span>
            <span class="text-xs font-semibold text-muted">ca học</span>
          </div>
          <p class="text-[11px] font-semibold text-muted mt-2">
            Đã hoàn thành: <strong class="text-emerald-600">{{ currentChild.schedule.filter(s => s.status === 'finished').length }} ca</strong>
          </p>
        </div>
        <div class="border-t border-card mt-4 pt-3 text-right">
          <button @click="navigateTo('/parent/learning/schedule')" class="text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2 inline-flex items-center gap-1">
            Thời khóa biểu con <ChevronRight :size="12" />
          </button>
        </div>
      </div>

    </div>

    <!-- ── SECOND ROW: TODAY SCHEDULE & ACADEMIC PROGRESS CHART ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4 lg:gap-6">
      
      <!-- Cột lịch học chi tiết (2/3 chiều rộng trên desktop) -->
      <div class="lg-glass flex flex-col p-6 lg:col-span-2 rounded-[24px]">
        <div class="flex items-center justify-between border-b border-card pb-5 mb-5">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <Clock :size="18" class="text-orange-600" />
            Lịch trình học tập hôm nay
          </h3>
          <span class="text-xs font-bold text-muted bg-[var(--surface-input)] px-3 py-1.5 rounded-xl border border-card shadow-inner">Ngày 10 tháng 06 năm 2026</span>
        </div>
        
        <div class="flex-1 space-y-4">
          <div v-if="currentChild.schedule.length === 0" class="text-center py-12 text-muted font-medium">
            Hôm nay con không có lịch học lên lớp.
          </div>
          <div
            v-for="session in currentChild.schedule"
            :key="session.id"
            class="group flex items-start gap-4 p-4 rounded-[16px] lg-solid-soft transition-all hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 border border-default cursor-pointer"
            :class="session.status === 'active' ? 'bg-orange-50/50 border-orange-200 dark:bg-orange-950/10 dark:border-orange-900/30 ring-1 ring-orange-500/20' : ''"
          >
            <!-- Trạng thái icon -->
            <div class="flex-shrink-0 mt-0.5">
              <div v-if="session.status === 'finished'" class="h-8 w-8 rounded-[10px] bg-emerald-100 dark:bg-emerald-950/40 text-emerald-600 flex items-center justify-center shadow-sm">
                <Check :size="14" stroke-width="3" />
              </div>
              <div v-else-if="session.status === 'active'" class="h-8 w-8 rounded-[10px] bg-orange-100 dark:bg-orange-950/40 text-orange-600 flex items-center justify-center animate-pulse shadow-sm">
                <Clock :size="14" stroke-width="2.5" />
              </div>
              <div v-else class="h-8 w-8 rounded-[10px] bg-[var(--surface-input)] border border-card text-muted flex items-center justify-center">
                <Clock :size="14" />
              </div>
            </div>

            <!-- Nội dung môn học -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-3">
                <h4 class="text-sm font-bold text-heading truncate group-hover:text-orange-600 transition-colors">{{ session.subject }}</h4>
                <span
                  class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider shadow-sm"
                  :class="
                    session.status === 'finished' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/40 dark:text-emerald-400' :
                    session.status === 'active' ? 'bg-orange-100 text-orange-700 animate-pulse dark:bg-orange-900/40 dark:text-orange-400' :
                    'bg-[var(--surface-input)] text-muted border border-card'
                  "
                >
                  {{ session.status === 'finished' ? 'Đã học' : session.status === 'active' ? 'Đang diễn ra' : 'Sắp tới' }}
                </span>
              </div>
              
              <div class="flex flex-wrap gap-x-5 gap-y-2 mt-3 text-xs font-medium text-body">
                <span class="flex items-center gap-1.5">
                  <Clock :size="14" class="text-muted" /> {{ session.time }}
                </span>
                <span class="flex items-center gap-1.5">
                  <MapPin :size="14" class="text-muted" /> {{ session.room }}
                </span>
                <span class="flex items-center gap-1.5">
                  <User :size="14" class="text-muted" /> {{ session.teacher }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Biểu đồ tiến trình học tập (1/3 chiều rộng trên desktop) -->
      <div class="lg-glass flex flex-col p-6 rounded-[24px]">
        <div class="border-b border-card pb-5 mb-5">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <Award :size="18" class="text-orange-600" />
            Biểu đồ tiến độ GPA của con
          </h3>
        </div>

        <div class="flex-1 flex flex-col justify-between">
          <!-- Custom SVG Line Chart -->
          <div class="relative w-full h-44 flex items-end justify-between px-2 pt-4">
            <!-- Grid Lines -->
            <div class="absolute inset-0 flex flex-col justify-between pointer-events-none opacity-10">
              <div class="w-full border-t border-slate-400"></div>
              <div class="w-full border-t border-slate-400"></div>
              <div class="w-full border-t border-slate-400"></div>
              <div class="w-full border-t border-slate-400"></div>
            </div>

            <!-- Visual Line and Nodes via SVG -->
            <svg class="absolute inset-0 w-full h-full drop-shadow-md" viewBox="0 0 100 40" preserveAspectRatio="none">
              <!-- Path Line -->
              <path
                :d="`M 10,${40 - (currentChild.gradesProgress[0].gpa * 3.5)}
                     L 35,${40 - (currentChild.gradesProgress[1].gpa * 3.5)}
                     L 65,${40 - (currentChild.gradesProgress[2].gpa * 3.5)}
                     L 90,${40 - (currentChild.gradesProgress[3].gpa * 3.5)}`"
                fill="none"
                stroke="#ea580c"
                stroke-width="2.5"
                stroke-linecap="round"
                stroke-linejoin="round"
              />
              <!-- Gradient Area -->
              <path
                :d="`M 10,40
                     L 10,${40 - (currentChild.gradesProgress[0].gpa * 3.5)}
                     L 35,${40 - (currentChild.gradesProgress[1].gpa * 3.5)}
                     L 65,${40 - (currentChild.gradesProgress[2].gpa * 3.5)}
                     L 90,${40 - (currentChild.gradesProgress[3].gpa * 3.5)}
                     L 90,40 Z`"
                fill="url(#orange-gradient)"
                opacity="0.2"
              />
              <defs>
                <linearGradient id="orange-gradient" x1="0%" y1="0%" x2="0%" y2="100%">
                  <stop offset="0%" stop-color="#ea580c" />
                  <stop offset="100%" stop-color="#ea580c" stop-opacity="0" />
                </linearGradient>
              </defs>

              <!-- Circle Dots -->
              <circle :cx="10" :cy="40 - (currentChild.gradesProgress[0].gpa * 3.5)" r="1.5" fill="#ea580c" />
              <circle :cx="35" :cy="40 - (currentChild.gradesProgress[1].gpa * 3.5)" r="1.5" fill="#ea580c" />
              <circle :cx="65" :cy="40 - (currentChild.gradesProgress[2].gpa * 3.5)" r="1.5" fill="#ea580c" />
              <circle :cx="90" :cy="40 - (currentChild.gradesProgress[3].gpa * 3.5)" r="2.5" fill="#ea580c" stroke="white" stroke-width="0.5" />
            </svg>

            <!-- Chart Value Labels -->
            <div
              v-for="(point, idx) in currentChild.gradesProgress"
              :key="idx"
              class="relative z-10 flex flex-col items-center justify-end h-full w-1/4 group"
            >
              <span class="text-[11px] font-bold text-orange-600 bg-white/95 dark:bg-slate-900/95 px-1.5 py-0.5 rounded-md border border-card shadow-[var(--lg-shadow-sm)] mb-1.5 group-hover:-translate-y-1 transition-transform">
                {{ point.gpa }}
              </span>
              <span class="text-[9px] font-bold text-muted text-center w-full truncate px-1 mt-auto">
                {{ point.semester.split(' - ')[1] || point.semester }}
              </span>
            </div>
          </div>
          
          <div class="mt-6 p-4 rounded-xl lg-solid-soft flex items-start gap-3 border border-default">
            <Sparkles :size="20" class="text-orange-600 flex-shrink-0 mt-0.5" />
            <p class="text-xs font-medium text-body leading-relaxed">
              Điểm số trung bình tích lũy GPA hiện tại đạt mức <strong class="text-orange-600 font-bold">{{ currentChild.gpa }} / 10</strong>. Con em tiếp tục duy trì mức học lực khá giỏi và tiến bộ đều đặn.
            </p>
          </div>
        </div>
      </div>

    </div>

    <!-- ── THIRD ROW: DANGER ALERTS & SYSTEM NOTIFICATIONS ── -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4 lg:gap-6">
      
      <!-- Box Cảnh báo từ giáo viên / hệ thống -->
      <div class="lg-glass p-6 flex flex-col rounded-[24px]">
        <div class="flex items-center justify-between border-b border-card pb-5 mb-5">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <ShieldAlert :size="18" class="text-red-500" />
            Cảnh báo học tập & rèn luyện gần đây
          </h3>
          <span class="px-2.5 py-1 rounded-full text-[10px] font-bold bg-red-100 text-red-700 shadow-sm">Cần lưu ý</span>
        </div>

        <div class="flex-1 space-y-4">
          <div v-if="currentChild.alerts.length === 0" class="flex flex-col items-center justify-center py-12 text-center bg-[var(--surface-input)] rounded-2xl border border-card border-dashed">
            <CheckCircle :size="36" class="text-emerald-500 mb-3" />
            <p class="text-sm font-bold text-heading">Không có cảnh báo học tập nào</p>
            <p class="text-xs font-medium text-body mt-1">Con đang học tập và tham gia đầy đủ các lớp học phần.</p>
          </div>
          <div
            v-for="alert in currentChild.alerts"
            :key="alert.id"
            class="flex items-start gap-4 p-4 rounded-[16px] border border-red-200/60 dark:border-red-950/30 bg-red-50/60 dark:bg-red-950/10 shadow-sm hover:shadow-md transition-shadow cursor-pointer"
          >
            <div class="h-10 w-10 shrink-0 rounded-[12px] bg-red-100 dark:bg-red-900/30 text-red-500 flex items-center justify-center">
              <AlertTriangle :size="18" stroke-width="2.5" />
            </div>
            <div class="flex-1 min-w-0 pt-0.5">
              <p class="text-sm font-bold text-slate-800 dark:text-slate-200 leading-snug">
                {{ alert.message }}
              </p>
              <span class="text-[11px] font-medium text-muted mt-1.5 block">{{ alert.time }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Tin tức / Thông báo từ nhà trường -->
      <div class="lg-glass p-6 flex flex-col rounded-[24px]">
        <div class="flex items-center justify-between border-b border-card pb-5 mb-5">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <MessageSquare :size="18" class="text-orange-600" />
            Thông tin từ trường đại học / cao đẳng
          </h3>
          <button @click="navigateTo('/parent/notifications/history')" class="text-xs font-bold text-orange-600 hover:text-orange-700 hover:underline decoration-2 underline-offset-2">Xem tất cả</button>
        </div>

        <div class="flex-1 space-y-3">
          <div
            v-for="notif in systemNotifications"
            :key="notif.id"
            class="group flex gap-4 relative p-4 rounded-[16px] lg-solid-soft transition-all hover:bg-[var(--surface-card-hover)] cursor-pointer border border-default"
          >
            <div class="flex-shrink-0 mt-1">
              <span class="relative flex h-2.5 w-2.5">
                <span v-if="notif.isNew" class="animate-ping absolute inline-flex h-full w-full rounded-full bg-orange-400 opacity-75"></span>
                <span class="relative inline-flex rounded-full h-2.5 w-2.5" :class="notif.isNew ? 'bg-orange-600' : 'bg-slate-300 dark:bg-slate-700'"></span>
              </span>
            </div>
            <div class="flex-1 min-w-0">
              <h4 class="text-sm font-bold text-heading flex items-center gap-2 leading-snug group-hover:text-orange-600 transition-colors">
                {{ notif.title }}
                <span v-if="notif.isNew" class="px-2 py-0.5 bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-400 text-[9px] font-bold rounded-md shadow-sm">Mới</span>
              </h4>
              <p class="text-xs font-medium text-body mt-1.5 leading-relaxed line-clamp-2">
                {{ notif.content }}
              </p>
              <span class="text-[10px] font-bold text-muted mt-2 block">{{ notif.time }}</span>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.border-card {
  border-color: var(--border-card);
}
.text-heading {
  color: var(--text-heading);
}
.text-body {
  color: var(--text-body);
}
.text-label {
  color: var(--text-label);
}
.text-muted {
  color: var(--text-muted);
}
</style>
