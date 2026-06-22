<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  Award, CheckCircle, CreditCard, Calendar,
  AlertTriangle, ChevronRight, Clock, MapPin,
  User, Sparkles, ChevronDown, Check, ShieldAlert,
  GraduationCap, Bell
} from 'lucide-vue-next'
import { childrenData } from '@/components/PhuHuynh/data/parentData.js'

const router = useRouter()

const activeChildId = ref(Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const children = computed(() =>
  childrenData.map(c => ({
    ...c,
    tuitionDebt: c.balanceTuition,
    tuitionStatus: c.balanceTuition > 0 ? (c.isOverdue ? 'Quá hạn' : 'Chờ thanh toán') : 'Đã hoàn thành',
    schedule: c.id === 1 ? [
      { id: 1, subject: 'Cấu trúc dữ liệu & Giải thuật', room: 'P.201 - Nhà Beta', teacher: 'TS. Trần Thị B', time: '07:30–09:30', status: 'finished' },
      { id: 2, subject: 'Toán cao cấp', room: 'P.105 - Nhà Gamma', teacher: 'ThS. Lê Văn C', time: '09:30–11:30', status: 'active' },
      { id: 3, subject: 'Lập trình Web nâng cao', room: 'P.402 - Nhà Alpha', teacher: 'ThS. Nguyễn Văn A', time: '12:30–14:30', status: 'upcoming' },
    ] : [
      { id: 1, subject: 'An toàn thông tin mạng', room: 'P.301 - Nhà Alpha', teacher: 'TS. Nguyễn Hoàng G', time: '07:30–09:30', status: 'finished' },
      { id: 2, subject: 'Phát triển ứng dụng di động', room: 'Lab 2 - Nhà Beta', teacher: 'ThS. Vũ Thị H', time: '09:30–11:30', status: 'upcoming' },
    ],
    alerts: c.warnings.filter(w => !w.confirmed).map(w => ({
      id: w.id, type: w.type,
      message: `${w.reason} (${w.subject})`,
      time: w.date
    })),
  }))
)

const currentChild = computed(() =>
  children.value.find(c => c.id === activeChildId.value) || children.value[0]
)

function selectChild(id) {
  activeChildId.value = id
  localStorage.setItem('parent_active_student_id', id)
  dropdownOpen.value = false
}

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

function nav(path) { router.push(path) }
</script>

<template>
  <div class="pb-6 min-h-screen bg-[var(--surface-page)]">

    <!-- ── HERO / CHILD SELECTOR ── -->
    <div class="relative px-4 py-6 overflow-hidden bg-gradient-to-br from-[var(--lg-primary-dark)] via-[var(--lg-primary)] to-orange-400 text-white rounded-b-[32px] shadow-[var(--lg-shadow-md)] z-10">
      <div class="absolute inset-0 opacity-10 bg-[url('data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cg fill=\'none\' fill-rule=\'evenodd\'%3E%3Cg fill=\'%23ffffff\' fill-opacity=\'1\'%3E%3Cpath d=\'M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E')]" />
      
      <div class="relative z-10">
        <p class="text-[13px] font-medium opacity-90 mb-1">Xin chào, Phụ huynh 👋</p>
        <h2 class="text-[24px] font-extrabold tracking-tight mb-1">{{ currentChild.name }}</h2>
        <p class="text-[12px] opacity-80 mb-4">{{ currentChild.studentId }} · {{ currentChild.class }}</p>

        <!-- Child switcher -->
        <div class="relative">
          <button class="inline-flex items-center gap-2 bg-white/20 border border-white/30 backdrop-blur-md text-white rounded-[20px] px-3.5 py-1.5 text-[12px] font-bold active:scale-95 transition-transform" @click="dropdownOpen = !dropdownOpen">
            <GraduationCap :size="14" />
            <span>Đổi hồ sơ con</span>
            <ChevronDown :size="14" :class="dropdownOpen ? 'rotate-180' : ''" class="transition-transform duration-300" />
          </button>

          <!-- Dropdown -->
          <Transition
            enter-active-class="transition-all duration-200 ease-out"
            enter-from-class="opacity-0 translate-y-2 scale-95"
            enter-to-class="opacity-100 translate-y-0 scale-100"
            leave-active-class="transition-all duration-150 ease-in"
            leave-from-class="opacity-100 translate-y-0 scale-100"
            leave-to-class="opacity-0 translate-y-2 scale-95"
          >
            <div v-if="dropdownOpen" class="absolute left-0 top-[calc(100%+0.5rem)] w-[240px] bg-[var(--surface-card)] rounded-[20px] shadow-[var(--lg-shadow-lg)] border border-card p-1.5 z-50">
              <button
                v-for="c in children" :key="c.id"
                class="flex items-center gap-3 w-full p-2.5 rounded-[14px] transition-colors"
                :class="c.id === activeChildId ? 'bg-[var(--surface-card-hover)]' : 'hover:bg-[var(--surface-card-hover)]'"
                @click="selectChild(c.id)"
              >
                <div class="w-8 h-8 rounded-full bg-gradient-to-br from-[var(--lg-primary)] to-orange-600 flex items-center justify-center text-[10px] font-bold text-white shadow-sm shrink-0">
                  {{ c.avatarInitials }}
                </div>
                <div class="flex-1 text-left min-w-0">
                  <p class="text-[13px] font-bold text-heading truncate">{{ c.name }}</p>
                  <p class="text-[10px] text-muted truncate">{{ c.studentId }} · {{ c.class }}</p>
                </div>
                <Check v-if="c.id === activeChildId" :size="16" class="text-[var(--lg-primary)] shrink-0" />
              </button>
            </div>
          </Transition>
        </div>
      </div>
    </div>

    <div class="px-4 mt-[-16px] relative z-20 space-y-4">
      
      <!-- ── OVERDUE ALERT ── -->
      <div
        v-if="currentChild.isOverdue"
        class="flex items-center gap-3 p-3.5 bg-[var(--color-danger-bg)]/80 border border-[var(--color-danger-text)]/30 rounded-[20px] shadow-[var(--lg-shadow-md)] backdrop-blur-xl active:scale-[0.98] transition-transform"
        @click="nav('/parent/finance/tuition')"
      >
        <div class="w-8 h-8 rounded-[12px] bg-[var(--color-danger-text)]/20 text-[var(--color-danger-text)] flex items-center justify-center shrink-0">
          <AlertTriangle :size="16" stroke-width="2.5" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-[12px] font-bold text-[var(--color-danger-text)]">Học phí quá hạn!</p>
          <p class="text-[11px] font-medium text-[var(--color-danger-text)] mt-0.5 truncate">
            Còn nợ <strong>{{ formatCurrency(currentChild.tuitionDebt) }}</strong>
          </p>
        </div>
        <ChevronRight :size="16" class="text-[var(--color-danger-text)] shrink-0" />
      </div>

      <!-- ── METRIC CARDS ── -->
      <div class="grid grid-cols-2 gap-3">

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-[var(--lg-shadow-sm)] active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/grades')">
          <div class="w-9 h-9 rounded-[12px] bg-[var(--color-warning-bg)]/80 text-[var(--color-warning-text)] flex items-center justify-center mb-2 shrink-0 border border-[var(--color-warning-text)]/20">
            <Award :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold text-heading tracking-tight leading-none mb-1">{{ currentChild.gpa }}<span class="text-[11px] font-semibold text-muted ml-0.5">/10</span></p>
          <p class="text-[11px] font-bold text-muted mb-2">Điểm GPA</p>
          <span class="text-[10px] font-bold text-[var(--color-warning-text)] mt-auto inline-flex items-center gap-1">Bảng điểm <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-[var(--lg-shadow-sm)] active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/attendance')">
          <div class="w-9 h-9 rounded-[12px] bg-[var(--color-success-bg)]/80 text-[var(--color-success-text)] flex items-center justify-center mb-2 shrink-0 border border-[var(--color-success-text)]/20">
            <CheckCircle :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold text-heading tracking-tight leading-none mb-1">{{ currentChild.attendanceRate }}<span class="text-[11px] font-semibold text-muted ml-0.5">%</span></p>
          <p class="text-[11px] font-bold text-muted mb-2">Chuyên cần</p>
          <span class="text-[10px] font-bold text-[var(--color-success-text)] mt-auto inline-flex items-center gap-1">Nhật ký <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-[var(--lg-shadow-sm)] active:scale-95 transition-transform flex flex-col" @click="nav('/parent/finance/tuition')">
          <div class="w-9 h-9 rounded-[12px] flex items-center justify-center mb-2 shrink-0 border" :class="currentChild.tuitionDebt > 0 ? 'bg-[var(--color-warning-bg)]/80 text-[var(--color-warning-text)] border-[var(--color-warning-text)]/20' : 'bg-[var(--color-success-bg)]/80 text-[var(--color-success-text)] border-[var(--color-success-text)]/20'">
            <CreditCard :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[13px] font-bold tracking-tight leading-tight mb-1" :class="currentChild.tuitionDebt > 0 ? 'text-heading' : 'text-[var(--color-success-text)]'">
            {{ currentChild.tuitionDebt > 0 ? formatCurrency(currentChild.tuitionDebt) : 'Đã đóng' }}
          </p>
          <p class="text-[11px] font-bold text-muted mb-2">Học phí</p>
          <span class="text-[10px] font-bold mt-auto inline-flex items-center gap-1" :class="currentChild.tuitionDebt > 0 ? 'text-[var(--color-warning-text)]' : 'text-[var(--color-success-text)]'">Chi tiết <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-[var(--lg-shadow-sm)] active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/alerts')">
          <div class="w-9 h-9 rounded-[12px] flex items-center justify-center mb-2 shrink-0 border" :class="currentChild.alerts.length > 0 ? 'bg-[var(--color-danger-bg)]/80 text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20' : 'bg-[var(--color-success-bg)]/80 text-[var(--color-success-text)] border-[var(--color-success-text)]/20'">
            <AlertTriangle :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold tracking-tight leading-none mb-1" :class="currentChild.alerts.length > 0 ? 'text-[var(--color-danger-text)]' : 'text-[var(--color-success-text)]'">
            {{ currentChild.alerts.length }}<span class="text-[11px] font-semibold text-muted ml-0.5">tin</span>
          </p>
          <p class="text-[11px] font-bold text-muted mb-2">Cảnh báo</p>
          <span class="text-[10px] font-bold mt-auto inline-flex items-center gap-1" :class="currentChild.alerts.length > 0 ? 'text-[var(--color-danger-text)]' : 'text-[var(--color-success-text)]'">Xem ngay <ChevronRight :size="10" /></span>
        </div>

      </div>

      <!-- ── TODAY SCHEDULE ── -->
      <div>
        <div class="flex items-center justify-between mb-3 px-1">
          <div class="text-[13px] font-bold text-heading flex items-center gap-1.5">
            <Calendar :size="16" class="text-[var(--lg-primary)]" />
            Lịch học hôm nay
          </div>
          <button class="text-[11px] font-bold text-[var(--lg-primary)]" @click="nav('/parent/learning/schedule')">Xem tất cả</button>
        </div>

        <div class="lg-glass rounded-[24px] p-2 border border-card shadow-[var(--lg-shadow-sm)]">
          <div v-if="currentChild.schedule.length === 0" class="p-6 text-center text-[12px] font-medium text-muted">
            Hôm nay không có lịch học
          </div>
          <div class="space-y-1">
            <div
              v-for="s in currentChild.schedule" :key="s.id"
              class="flex items-center gap-3 p-3 rounded-[16px] transition-colors"
              :class="s.status === 'active' ? 'bg-[var(--color-warning-bg)]/40 border border-[var(--color-warning-text)]/20' : 'lg-solid-soft'"
            >
              <!-- Status dot -->
              <div class="w-2.5 h-2.5 rounded-full shrink-0" :class="{
                'bg-[var(--color-success-text)]': s.status === 'finished',
                'bg-[var(--color-warning-text)] ring-4 ring-[var(--color-warning-text)]/30 animate-pulse': s.status === 'active',
                'bg-slate-300 dark:bg-slate-600': s.status === 'upcoming'
              }" />

              <div class="flex-1 min-w-0">
                <div class="text-[13px] font-bold text-heading truncate">{{ s.subject }}</div>
                <div class="flex items-center gap-3 mt-1 text-[11px] font-medium text-muted">
                  <span class="flex items-center gap-1"><Clock :size="10" /> {{ s.time }}</span>
                  <span class="flex items-center gap-1 truncate"><MapPin :size="10" /> {{ s.room }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- ── LEARNING ALERTS ── -->
      <div v-if="currentChild.alerts.length > 0">
        <div class="flex items-center justify-between mb-3 px-1">
          <div class="text-[13px] font-bold text-heading flex items-center gap-1.5">
            <ShieldAlert :size="16" class="text-[var(--color-danger-text)]" />
            Cảnh báo học tập
          </div>
          <button class="text-[11px] font-bold text-[var(--color-danger-text)]" @click="nav('/parent/learning/alerts')">Xem tất cả</button>
        </div>
        <div class="lg-glass rounded-[24px] p-2 space-y-1 border border-card shadow-[var(--lg-shadow-sm)]">
          <div
            v-for="a in currentChild.alerts.slice(0, 3)" :key="a.id"
            class="flex gap-3 p-3 rounded-[16px] bg-[var(--color-danger-bg)]/40 border border-[var(--color-danger-text)]/20"
          >
            <AlertTriangle :size="16" class="text-[var(--color-danger-text)] shrink-0 mt-0.5" />
            <div class="flex-1 min-w-0">
              <p class="text-[12px] font-bold text-heading leading-snug">{{ a.message }}</p>
              <p class="text-[10px] font-medium text-[var(--color-danger-text)] mt-1">{{ a.time }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- ── QUICK ACTIONS ── -->
      <div>
        <div class="flex items-center mb-3 px-1">
          <div class="text-[13px] font-bold text-heading flex items-center gap-1.5">
            <Sparkles :size="16" class="text-[var(--lg-primary)]" />
            Truy cập nhanh
          </div>
        </div>
        <div class="grid grid-cols-3 gap-3">
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/learning/grades')">
            <div class="w-10 h-10 rounded-[12px] bg-[var(--color-warning-bg)]/80 text-[var(--color-warning-text)] flex items-center justify-center shrink-0 shadow-sm border border-[var(--color-warning-text)]/20">
               <Award :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Bảng điểm</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/learning/attendance')">
            <div class="w-10 h-10 rounded-[12px] bg-[var(--color-success-bg)]/80 text-[var(--color-success-text)] flex items-center justify-center shrink-0 shadow-sm border border-[var(--color-success-text)]/20">
               <CheckCircle :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Điểm danh</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/learning/schedule')">
            <div class="w-10 h-10 rounded-[12px] bg-[var(--color-info-bg)]/80 text-[var(--color-info-text)] flex items-center justify-center shrink-0 shadow-sm border border-[var(--color-info-text)]/20">
               <Calendar :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">TKB</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/finance/tuition')">
            <div class="w-10 h-10 rounded-[12px] bg-cyan-50 dark:bg-cyan-950/30 text-cyan-600 flex items-center justify-center shrink-0 shadow-sm border border-cyan-200/50">
               <CreditCard :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Học phí</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/notifications/history')">
            <div class="w-10 h-10 rounded-[12px] bg-amber-50 dark:bg-amber-950/30 text-amber-600 flex items-center justify-center shrink-0 shadow-sm border border-amber-200/50">
               <Bell :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Thông báo</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-[var(--lg-shadow-sm)]" @click="nav('/parent/profile/info')">
            <div class="w-10 h-10 rounded-[12px] bg-[var(--surface-input)] text-muted flex items-center justify-center shrink-0 shadow-sm border border-card">
               <User :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Hồ sơ</span>
          </button>
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
/* No longer need custom CSS as we use standard Tailwind and Liquid Glass classes */
</style>
