<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Award, CheckCircle, CreditCard, Calendar,
  AlertTriangle, ChevronRight, Clock, MapPin,
  User, Sparkles, ChevronDown, Check, ShieldAlert,
  GraduationCap, Bell
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { getStoredActiveChildId, setActiveChildId } from '@/components/PhuHuynh/data/parentState.js'

const router = useRouter()

const emptyChild = {
  id: null,
  name: 'Chưa có học sinh liên kết',
  studentId: '',
  class: '',
  avatarInitials: '-',
  gpa: 0,
  attendanceRate: 0,
  tuitionDebt: 0,
  isOverdue: false,
  schedule: [],
  alerts: [],
}

const activeChildId = ref(getStoredActiveChildId())
const dropdownOpen = ref(false)
const rawChildren = ref([])
const childDetails = ref({})
const childSchedules = ref({})
const childAlerts = ref({})
const childTuition = ref({})

const children = computed(() =>
  rawChildren.value.map(c => ({
    id: c.id,
    name: c.name,
    studentId: c.email || `ID ${c.id}`,
    class: c.className || 'Chưa có lớp',
    avatarInitials: getInitials(c.name),
    gpa: childDetails.value[c.id]?.gpa || 0,
    attendanceRate: 0,
    tuitionDebt: childTuition.value[c.id]?.totalDue || 0,
    isOverdue: false,
    schedule: (childSchedules.value[c.id] || []).map((s, idx) => ({
      id: idx + 1,
      subject: s.subject || 'Lịch học',
      room: s.room || '-',
      teacher: s.teacher || '-',
      time: s.time || '-',
      status: idx === 0 ? 'upcoming' : 'finished',
    })),
    alerts: (childAlerts.value[c.id] || []).map((a, idx) => ({
      id: idx + 1,
      type: a.severity || 'info',
      message: a.message || 'Cảnh báo hệ thống',
      time: 'Từ hệ thống',
    })),
  }))
)

const currentChild = computed(() =>
  children.value.find(c => c.id === activeChildId.value) || children.value[0] || emptyChild
)

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  loadChildDetails(id)
}

function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

function nav(path) { router.push(path) }

function getInitials(name = '') {
  return name.split(' ').filter(Boolean).slice(-2).map(part => part.charAt(0)).join('').toUpperCase() || '-'
}

async function loadChildDetails(childId) {
  if (!childId) return
  const [detailRes, scheduleRes, alertsRes, tuitionRes] = await Promise.allSettled([
    parentApi.getChildDetail(childId),
    parentApi.getChildSchedule(childId),
    parentApi.getChildAlerts(childId),
    parentApi.getChildTuition(childId),
  ])
  if (detailRes.status === 'fulfilled') childDetails.value[childId] = detailRes.value?.data || {}
  if (scheduleRes.status === 'fulfilled') childSchedules.value[childId] = scheduleRes.value?.data || []
  if (alertsRes.status === 'fulfilled') childAlerts.value[childId] = alertsRes.value?.data?.alerts || []
  if (tuitionRes.status === 'fulfilled') childTuition.value[childId] = tuitionRes.value?.data || {}
}

async function loadDashboard() {
  const dashboardRes = await parentApi.getDashboard()
  rawChildren.value = dashboardRes?.data?.children || []
  const firstChild = rawChildren.value.find(child => child.id === activeChildId.value) || rawChildren.value[0]
  if (firstChild) {
    activeChildId.value = firstChild.id
    setActiveChildId(firstChild.id)
    await loadChildDetails(firstChild.id)
  }
}

onMounted(loadDashboard)
</script>

<template>
  <div class="pb-6 min-h-screen bg-(--surface-page)">

    <!-- ── HERO / CHILD SELECTOR ── -->
    <div class="relative px-4 py-6 overflow-hidden bg-gradient-to-br from-(--lg-primary-dark) via-(--lg-primary) to-orange-400 text-white rounded-b-[32px] shadow-(--lg-shadow-md) z-10">
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
            <div v-if="dropdownOpen" class="absolute left-0 top-[calc(100%+0.5rem)] w-[240px] bg-(--surface-card) rounded-[20px] shadow-(--lg-shadow-lg) border border-card p-1.5 z-50">
              <button
                v-for="c in children" :key="c.id"
                class="flex items-center gap-3 w-full p-2.5 rounded-[14px] transition-colors"
                :class="c.id === activeChildId ? 'bg-(--surface-card-hover)' : 'hover:bg-(--surface-card-hover)'"
                @click="selectChild(c.id)"
              >
                <div class="w-8 h-8 rounded-full bg-gradient-to-br from-(--lg-primary) to-orange-600 flex items-center justify-center text-[10px] font-bold text-white shadow-sm shrink-0">
                  {{ c.avatarInitials }}
                </div>
                <div class="flex-1 text-left min-w-0">
                  <p class="text-[13px] font-bold text-heading truncate">{{ c.name }}</p>
                  <p class="text-[10px] text-muted truncate">{{ c.studentId }} · {{ c.class }}</p>
                </div>
                <Check v-if="c.id === activeChildId" :size="16" class="text-(--lg-primary) shrink-0" />
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
        class="flex items-center gap-3 p-3.5 bg-(--color-danger-bg)/80 border border-(--color-danger-text)/30 rounded-[20px] shadow-(--lg-shadow-md) backdrop-blur-xl active:scale-[0.98] transition-transform"
        @click="nav('/parent/finance/tuition')"
      >
        <div class="w-8 h-8 rounded-[12px] bg-(--color-danger-text)/20 text-(--color-danger-text) flex items-center justify-center shrink-0">
          <AlertTriangle :size="16" stroke-width="2.5" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-[12px] font-bold text-(--color-danger-text)">Học phí quá hạn!</p>
          <p class="text-[11px] font-medium text-(--color-danger-text) mt-0.5 truncate">
            Còn nợ <strong>{{ formatCurrency(currentChild.tuitionDebt) }}</strong>
          </p>
        </div>
        <ChevronRight :size="16" class="text-(--color-danger-text) shrink-0" />
      </div>

      <!-- ── METRIC CARDS ── -->
      <div class="grid grid-cols-2 gap-3">

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-(--lg-shadow-sm) active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/grades')">
          <div class="w-9 h-9 rounded-[12px] bg-(--color-warning-bg)/80 text-(--color-warning-text) flex items-center justify-center mb-2 shrink-0 border border-(--color-warning-text)/20">
            <Award :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold text-heading tracking-tight leading-none mb-1">{{ currentChild.gpa }}<span class="text-[11px] font-semibold text-muted ml-0.5">/10</span></p>
          <p class="text-[11px] font-bold text-muted mb-2">Điểm GPA</p>
          <span class="text-[10px] font-bold text-(--color-warning-text) mt-auto inline-flex items-center gap-1">Bảng điểm <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-(--lg-shadow-sm) active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/attendance')">
          <div class="w-9 h-9 rounded-[12px] bg-(--color-success-bg)/80 text-(--color-success-text) flex items-center justify-center mb-2 shrink-0 border border-(--color-success-text)/20">
            <CheckCircle :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold text-heading tracking-tight leading-none mb-1">{{ currentChild.attendanceRate }}<span class="text-[11px] font-semibold text-muted ml-0.5">%</span></p>
          <p class="text-[11px] font-bold text-muted mb-2">Chuyên cần</p>
          <span class="text-[10px] font-bold text-(--color-success-text) mt-auto inline-flex items-center gap-1">Nhật ký <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-(--lg-shadow-sm) active:scale-95 transition-transform flex flex-col" @click="nav('/parent/finance/tuition')">
          <div class="w-9 h-9 rounded-[12px] flex items-center justify-center mb-2 shrink-0 border" :class="currentChild.tuitionDebt > 0 ? 'bg-(--color-warning-bg)/80 text-(--color-warning-text) border-(--color-warning-text)/20' : 'bg-(--color-success-bg)/80 text-(--color-success-text) border-(--color-success-text)/20'">
            <CreditCard :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[13px] font-bold tracking-tight leading-tight mb-1" :class="currentChild.tuitionDebt > 0 ? 'text-heading' : 'text-(--color-success-text)'">
            {{ currentChild.tuitionDebt > 0 ? formatCurrency(currentChild.tuitionDebt) : 'Đã đóng' }}
          </p>
          <p class="text-[11px] font-bold text-muted mb-2">Học phí</p>
          <span class="text-[10px] font-bold mt-auto inline-flex items-center gap-1" :class="currentChild.tuitionDebt > 0 ? 'text-(--color-warning-text)' : 'text-(--color-success-text)'">Chi tiết <ChevronRight :size="10" /></span>
        </div>

        <div class="lg-glass-soft p-3.5 rounded-[20px] shadow-(--lg-shadow-sm) active:scale-95 transition-transform flex flex-col" @click="nav('/parent/learning/alerts')">
          <div class="w-9 h-9 rounded-[12px] flex items-center justify-center mb-2 shrink-0 border" :class="currentChild.alerts.length > 0 ? 'bg-(--color-danger-bg)/80 text-(--color-danger-text) border-(--color-danger-text)/20' : 'bg-(--color-success-bg)/80 text-(--color-success-text) border-(--color-success-text)/20'">
            <AlertTriangle :size="18" stroke-width="2.5" />
          </div>
          <p class="text-[22px] font-extrabold tracking-tight leading-none mb-1" :class="currentChild.alerts.length > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)'">
            {{ currentChild.alerts.length }}<span class="text-[11px] font-semibold text-muted ml-0.5">tin</span>
          </p>
          <p class="text-[11px] font-bold text-muted mb-2">Cảnh báo</p>
          <span class="text-[10px] font-bold mt-auto inline-flex items-center gap-1" :class="currentChild.alerts.length > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)'">Xem ngay <ChevronRight :size="10" /></span>
        </div>

      </div>

      <!-- ── TODAY SCHEDULE ── -->
      <div>
        <div class="flex items-center justify-between mb-3 px-1">
          <div class="text-[13px] font-bold text-heading flex items-center gap-1.5">
            <Calendar :size="16" class="text-(--lg-primary)" />
            Lịch học hôm nay
          </div>
          <button class="text-[11px] font-bold text-(--lg-primary)" @click="nav('/parent/learning/schedule')">Xem tất cả</button>
        </div>

        <div class="lg-glass rounded-[24px] p-2 border border-card shadow-(--lg-shadow-sm)">
          <div v-if="currentChild.schedule.length === 0" class="p-6 text-center text-[12px] font-medium text-muted">
            Hôm nay không có lịch học
          </div>
          <div class="space-y-1">
            <div
              v-for="s in currentChild.schedule" :key="s.id"
              class="flex items-center gap-3 p-3 rounded-[16px] transition-colors"
              :class="s.status === 'active' ? 'bg-(--color-warning-bg)/40 border border-(--color-warning-text)/20' : 'lg-solid-soft'"
            >
              <!-- Status dot -->
              <div class="w-2.5 h-2.5 rounded-full shrink-0" :class="{
                'bg-(--color-success-text)': s.status === 'finished',
                'bg-(--color-warning-text) ring-4 ring-(--color-warning-text)/30 animate-pulse': s.status === 'active',
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
            <ShieldAlert :size="16" class="text-(--color-danger-text)" />
            Cảnh báo học tập
          </div>
          <button class="text-[11px] font-bold text-(--color-danger-text)" @click="nav('/parent/learning/alerts')">Xem tất cả</button>
        </div>
        <div class="lg-glass rounded-[24px] p-2 space-y-1 border border-card shadow-(--lg-shadow-sm)">
          <div
            v-for="a in currentChild.alerts.slice(0, 3)" :key="a.id"
            class="flex gap-3 p-3 rounded-[16px] bg-(--color-danger-bg)/40 border border-(--color-danger-text)/20"
          >
            <AlertTriangle :size="16" class="text-(--color-danger-text) shrink-0 mt-0.5" />
            <div class="flex-1 min-w-0">
              <p class="text-[12px] font-bold text-heading leading-snug">{{ a.message }}</p>
              <p class="text-[10px] font-medium text-(--color-danger-text) mt-1">{{ a.time }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- ── QUICK ACTIONS ── -->
      <div>
        <div class="flex items-center mb-3 px-1">
          <div class="text-[13px] font-bold text-heading flex items-center gap-1.5">
            <Sparkles :size="16" class="text-(--lg-primary)" />
            Truy cập nhanh
          </div>
        </div>
        <div class="grid grid-cols-3 gap-3">
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/learning/grades')">
            <div class="w-10 h-10 rounded-[12px] bg-(--color-warning-bg)/80 text-(--color-warning-text) flex items-center justify-center shrink-0 shadow-sm border border-(--color-warning-text)/20">
               <Award :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Bảng điểm</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/learning/attendance')">
            <div class="w-10 h-10 rounded-[12px] bg-(--color-success-bg)/80 text-(--color-success-text) flex items-center justify-center shrink-0 shadow-sm border border-(--color-success-text)/20">
               <CheckCircle :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Điểm danh</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/learning/schedule')">
            <div class="w-10 h-10 rounded-[12px] bg-(--color-info-bg)/80 text-(--color-info-text) flex items-center justify-center shrink-0 shadow-sm border border-(--color-info-text)/20">
               <Calendar :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">TKB</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/finance/tuition')">
            <div class="w-10 h-10 rounded-[12px] bg-cyan-50 dark:bg-cyan-950/30 text-cyan-600 flex items-center justify-center shrink-0 shadow-sm border border-cyan-200/50">
               <CreditCard :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Học phí</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/notifications/history')">
            <div class="w-10 h-10 rounded-[12px] bg-amber-50 dark:bg-amber-950/30 text-amber-600 flex items-center justify-center shrink-0 shadow-sm border border-amber-200/50">
               <Bell :size="20" stroke-width="2.5" />
            </div>
            <span class="text-[11px] font-bold text-heading">Thông báo</span>
          </button>
          <button class="lg-glass p-3 rounded-[20px] flex flex-col items-center gap-2 active:scale-95 transition-transform border border-card shadow-(--lg-shadow-sm)" @click="nav('/parent/profile/info')">
            <div class="w-10 h-10 rounded-[12px] bg-(--surface-input) text-muted flex items-center justify-center shrink-0 shadow-sm border border-card">
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
