<script setup>
import { ref } from 'vue'
import {
  BookOpen, ClipboardList, CalendarDays, CreditCard,
  AlertTriangle, Clock, MapPin,
  ChevronRight, CheckCircle2, Bell, X,
  Star, UserCheck, BarChart2, Sparkles, Award, TrendingUp,
} from 'lucide-vue-next'
import LmsAlert from '@/components/LmsAlert.vue'
import LmsCard from '@/components/LmsCard.vue'
import LmsBadge from '@/components/LmsBadge.vue'
import { useAuthStore } from '@/stores/auth'

defineOptions({
  name: 'StudentDashboard',
})

const authStore = useAuthStore()

// ── KPI Cards ──────────────────────────────────────────────
const kpiCards = [
  {
    id: 'courses', label: 'Khóa học đang học', value: '6', unit: 'môn',
    icon: BookOpen, color: 'blue', sub: '+1 so với kỳ trước', route: '/student/courses',
  },
  {
    id: 'assignments', label: 'Bài tập chưa nộp', value: '3', unit: 'bài',
    icon: ClipboardList, color: 'orange', sub: '1 bài hết hạn HÔM NAY', urgent: true, route: '/student/assignments',
  },
  {
    id: 'schedule', label: 'Tiết học hôm nay', value: '2', unit: 'tiết',
    icon: CalendarDays, color: 'green', sub: 'Tiếp theo: 13:30 — Toán rời rạc', route: '/student/schedule',
  },
  {
    id: 'tuition', label: 'Học phí kỳ 2', value: 'Đã nộp', unit: '',
    icon: CreditCard, color: 'teal', sub: 'Hạn: 30/06/2025', route: '/student/tuition',
  },
  {
    id: 'gpa', label: 'GPA tích lũy', value: '3.42', unit: '/4.0',
    icon: BarChart2, color: 'indigo', sub: 'Xếp loại: Giỏi', route: '/student/grades',
  },
  {
    id: 'attendance', label: 'Tỷ lệ điểm danh', value: '92', unit: '%',
    icon: UserCheck, color: 'purple', sub: '3 buổi vắng trong kỳ', route: '/student/attendance',
  },
]

// ── Lịch học hôm nay ──────────────────────────────────────
const todayClasses = [
  { id: 1, subject: 'Cấu trúc dữ liệu & Giải thuật', code: 'CTDL101', timeStart: '07:30', timeEnd: '09:30', room: 'P.302', campus: 'CS1', lecturer: 'TS. Nguyễn Minh Khoa', type: 'Lý thuyết', status: 'done' },
  { id: 2, subject: 'Toán rời rạc', code: 'TRR201', timeStart: '13:30', timeEnd: '15:30', room: 'P.105', campus: 'CS1', lecturer: 'ThS. Trần Thu Hà', type: 'Lý thuyết', status: 'upcoming' },
]

// ── Deadline & Tasks ───────────────────────────────────────
const deadlines = [
  { id: 1, type: 'assignment', label: 'Nộp bài tập CTDL - Tuần 10', subject: 'CTDL101', due: 'Hôm nay · 23:59', urgent: true, done: false, route: '/student/assignments' },
  { id: 2, type: 'exam', label: 'Thi giữa kỳ: Toán rời rạc', subject: 'TRR201', due: 'Thứ 6 · 09/05 · 07:30', urgent: false, done: false, route: '/student/exams' },
  { id: 3, type: 'assignment', label: 'Bài tập lớn: Lập trình Web', subject: 'LTW301', due: 'Thứ 2 · 12/05 · 23:59', urgent: false, done: false, route: '/student/assignments' },
  { id: 4, type: 'registration', label: 'Đăng ký môn học kỳ 3', subject: 'Phòng đào tạo', due: 'Đến 15/05/2025', urgent: false, done: false, route: '/student/registrations' },
  { id: 5, type: 'assignment', label: 'Báo cáo thực hành CSDL', subject: 'HQTCSDL401', due: 'Thứ 5 · 15/05 · 23:59', urgent: false, done: false, route: '/student/assignments' },
]

// ── Tiến độ khóa học ──────────────────────────────────────
const courseProgress = [
  { name: 'Cấu trúc dữ liệu & Giải thuật', code: 'CTDL101', progress: 72, completed: 9, total: 12, color: 'blue', grade: 8.2 },
  { name: 'Toán rời rạc', code: 'TRR201', progress: 58, completed: 7, total: 12, color: 'indigo', grade: null },
  { name: 'Lập trình Web', code: 'LTW301', progress: 85, completed: 10, total: 12, color: 'green', grade: 9.0 },
  { name: 'Hệ quản trị CSDL', code: 'HQTCSDL401', progress: 40, completed: 5, total: 12, color: 'orange', grade: null },
  { name: 'Mạng máy tính', code: 'MMT501', progress: 65, completed: 8, total: 12, color: 'teal', grade: 7.5 },
  { name: 'Anh văn chuyên ngành', code: 'AVN601', progress: 50, completed: 6, total: 12, color: 'purple', grade: null },
]

// ── Thông báo hệ thống ─────────────────────────────────────
const systemNotices = [
  { id: 1, type: 'warning', icon: AlertTriangle, title: 'Học phí nhắc nhở', body: 'Học phí kỳ 2/2024-2025 đã đến hạn thanh toán ngày 15/05/2025.', time: '1 giờ trước', read: false, color: 'yellow' },
  { id: 2, type: 'grade', icon: Star, title: 'Điểm đã được công bố', body: 'Điểm giữa kỳ môn Lập trình Web đã được đăng: 9.0/10', time: '3 giờ trước', read: false, color: 'green' },
  { id: 3, type: 'schedule', icon: CalendarDays, title: 'Lịch học thay đổi', body: 'Môn Vật lý tuần tới chuyển từ P101 sang P205.', time: 'Hôm qua', read: true, color: 'blue' },
]

const recentGrades = [
  { id: 1, subject: 'Lập trình Web', score: '9.0', type: 'Giữa kỳ', trend: '+0.4', color: 'green' },
  { id: 2, subject: 'Cấu trúc dữ liệu', score: '8.2', type: 'Bài tập', trend: '+0.2', color: 'blue' },
  { id: 3, subject: 'Mạng máy tính', score: '7.5', type: 'Quiz', trend: 'ổn định', color: 'teal' },
]

// ── Drawer thông báo chi tiết ─────────────────────────────
const drawerOpen = ref(false)
const selectedNotice = ref(null)
function openNoticeDrawer(n) { selectedNotice.value = n; drawerOpen.value = true }
function closeDrawer() { drawerOpen.value = false }

// ── Color helpers ─────────────────────────────────────────
const colorMap = {
  blue:   { bg: 'bg-blue-50',   text: 'text-blue-600',   icon: 'bg-blue-100',   bar: 'bg-blue-500',   badge: 'bg-blue-100 text-blue-700'   },
  orange: { bg: 'bg-orange-50', text: 'text-orange-600', icon: 'bg-orange-100', bar: 'bg-orange-500', badge: 'bg-orange-100 text-orange-700' },
  green:  { bg: 'bg-green-50',  text: 'text-green-600',  icon: 'bg-green-100',  bar: 'bg-green-500',  badge: 'bg-green-100 text-green-700'  },
  teal:   { bg: 'bg-teal-50',   text: 'text-teal-600',   icon: 'bg-teal-100',   bar: 'bg-teal-500',   badge: 'bg-teal-100 text-teal-700'   },
  indigo: { bg: 'bg-indigo-50', text: 'text-indigo-600', icon: 'bg-indigo-100', bar: 'bg-indigo-500', badge: 'bg-indigo-100 text-indigo-700'},
  purple: { bg: 'bg-purple-50', text: 'text-purple-600', icon: 'bg-purple-100', bar: 'bg-purple-500', badge: 'bg-purple-100 text-purple-700'},
  yellow: { bg: 'bg-yellow-50', text: 'text-yellow-600', icon: 'bg-yellow-100', bar: 'bg-yellow-500', badge: 'bg-yellow-100 text-yellow-700'},
}
const c = (color, type) => colorMap[color]?.[type] || ''

const typeLabel = { assignment: 'Bài tập', exam: 'Thi', registration: 'Đăng ký' }
const typeColor = { assignment: 'orange', exam: 'blue', registration: 'purple' }
</script>

<template>
  <div class="lg-page-enter space-y-6">
    <LmsCard variant="glass" class="relative overflow-hidden" padding="1.75rem">
      <div class="pointer-events-none absolute -right-16 -top-24 h-72 w-72 rounded-full bg-cyan-300/30 blur-3xl" />
      <div class="pointer-events-none absolute -bottom-28 left-1/3 h-72 w-72 rounded-full bg-violet-300/20 blur-3xl" />

      <div class="relative grid gap-6 lg:grid-cols-[1fr_320px] lg:items-center">
        <div>
          <div class="inline-flex items-center gap-2 rounded-full border border-white/55 bg-white/55 px-3 py-1 text-xs font-bold uppercase tracking-[0.08em] text-teal-700 backdrop-blur-xl">
            <Sparkles :size="14" />
            Student cockpit
          </div>
          <h2 class="mt-4 text-2xl font-extrabold leading-tight tracking-[-0.02em] text-slate-950 sm:text-3xl">
            Chào mừng trở lại, {{ authStore.displayName || 'Sinh viên' }}
          </h2>
          <p class="mt-3 max-w-2xl text-sm leading-6 text-slate-600">
            Hôm nay bạn có 2 tiết học, 1 bài tập đến hạn và 3 thông báo học vụ mới. Các mục quan trọng đã được gom lại để bạn xử lý nhanh hơn.
          </p>
          <div class="mt-5 flex flex-wrap gap-2">
            <router-link to="/student/assignments" class="lg-button-primary px-4 py-2.5 text-sm font-semibold">
              Xem việc cần làm
              <ChevronRight :size="16" />
            </router-link>
            <router-link to="/student/schedule" class="lg-button-secondary px-4 py-2.5 text-sm font-semibold">
              Lịch hôm nay
            </router-link>
          </div>
        </div>

        <div class="grid gap-3 sm:grid-cols-3 lg:grid-cols-1">
          <div class="lg-nav rounded-2xl p-4">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-blue-600 text-white shadow-lg shadow-blue-500/20">
                <Award :size="20" />
              </div>
              <div>
                <p class="text-xs font-semibold text-slate-500">GPA tích lũy</p>
                <p class="text-xl font-extrabold text-slate-950">3.42/4.0</p>
              </div>
            </div>
          </div>
          <div class="lg-nav rounded-2xl p-4">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-teal-600 text-white shadow-lg shadow-teal-500/20">
                <TrendingUp :size="20" />
              </div>
              <div>
                <p class="text-xs font-semibold text-slate-500">Tiến độ kỳ này</p>
                <p class="text-xl font-extrabold text-slate-950">68%</p>
              </div>
            </div>
          </div>
          <div class="lg-nav rounded-2xl p-4">
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-2xl bg-violet-600 text-white shadow-lg shadow-violet-500/20">
                <Bell :size="20" />
              </div>
              <div>
                <p class="text-xs font-semibold text-slate-500">Thông báo mới</p>
                <p class="text-xl font-extrabold text-slate-950">{{ systemNotices.filter(n => !n.read).length }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </LmsCard>

    <!-- Alert: Deadline Today -->
    <LmsAlert
      v-if="deadlines.some(d => d.urgent && !d.done)"
      type="warning"
      :closeable="true"
    >
      <span>
        Bạn có <strong>{{ deadlines.filter(d => d.urgent && !d.done).length }} bài tập</strong> hết hạn hôm nay!
        <router-link to="/student/assignments" class="ml-2 inline-flex items-center gap-1 font-semibold text-yellow-700 hover:text-yellow-800 underline">
          Xem ngay <ChevronRight :size="14" />
        </router-link>
      </span>
    </LmsAlert>

    <!-- KPI Cards -->
    <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-3">
      <router-link
        v-for="card in kpiCards" :key="card.id"
        :to="card.route"
        class="group lg-card-hover lg-glass-soft relative rounded-[24px] p-4"
        :class="{ 'ring-2 ring-yellow-300': card.urgent }"
      >
        <div class="relative space-y-2">
          <div :class="['inline-flex h-8 w-8 items-center justify-center rounded-lg', c(card.color,'icon')]">
            <component :is="card.icon" :size="16" :class="c(card.color,'text')" stroke-width="2" />
          </div>
          <div class="flex items-baseline gap-1">
            <span :class="['text-xl font-bold text-slate-800', card.urgent ? 'text-yellow-600' : '']">{{ card.value }}</span>
            <span v-if="card.unit" class="text-xs text-slate-500">{{ card.unit }}</span>
          </div>
          <p class="text-[12px] text-slate-600 leading-tight font-medium">{{ card.label }}</p>
          <p :class="['text-[11px] leading-tight', card.urgent ? 'text-yellow-600 font-semibold' : 'text-slate-500']">{{ card.sub }}</p>
        </div>
        <ChevronRight :size="14" class="absolute right-3 bottom-3 text-slate-300 group-hover:text-blue-500 transition-colors" />
      </router-link>
    </div>

    <!-- Row: Schedule + Deadlines -->
    <div class="grid grid-cols-1 lg:grid-cols-5 gap-4">

      <!-- Lịch học hôm nay -->
      <LmsCard variant="glass" class="lg:col-span-3 flex flex-col">
        <div class="border-b border-white/40 pb-3.5 mb-4">
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-[15px] font-semibold text-slate-800">Lịch học hôm nay</h2>
              <p class="text-[11px] text-slate-500 mt-0.5">Chủ nhật, 04 tháng 05 năm 2025</p>
            </div>
            <router-link to="/student/schedule" class="flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-700 lg-focus-ring rounded px-1">
              Xem đầy đủ <ChevronRight :size="12" />
            </router-link>
          </div>
        </div>
        <div class="space-y-3 flex-1">
          <!-- Empty state -->
          <div v-if="todayClasses.length === 0" class="flex flex-col items-center py-10 text-slate-400">
            <CalendarDays :size="36" stroke-width="1.2" />
            <p class="mt-2 text-sm">Không có lịch học hôm nay</p>
          </div>

          <div
            v-for="cls in todayClasses" :key="cls.id"
            :class="['flex gap-3 rounded-lg border p-3.5 transition-all', cls.status === 'done' ? 'border-slate-200 bg-slate-50 opacity-60' : 'border-blue-200 bg-blue-50/60 hover:border-blue-300']"
          >
            <!-- Time -->
            <div class="flex-shrink-0 w-[56px] text-center">
              <div :class="['rounded-lg px-1.5 py-1.5 text-[11px] leading-snug font-semibold', cls.status === 'done' ? 'bg-slate-200 text-slate-600' : 'bg-blue-200 text-blue-800']">
                {{ cls.timeStart }}<br /><span class="font-normal">{{ cls.timeEnd }}</span>
              </div>
            </div>
            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-2">
                <p class="text-[13px] font-semibold text-slate-800 leading-tight truncate">{{ cls.subject }}</p>
                <LmsBadge :variant="cls.status === 'done' ? 'warning' : 'success'" size="sm">
                  {{ cls.status === 'done' ? 'Đã học' : 'Sắp tới' }}
                </LmsBadge>
              </div>
              <p class="text-[11px] text-slate-600 mt-0.5">{{ cls.code }} · {{ cls.type }}</p>
              <div class="mt-1.5 flex flex-wrap gap-x-3 text-[11px] text-slate-600">
                <span class="flex items-center gap-1"><MapPin :size="10" />{{ cls.room }} – {{ cls.campus }}</span>
                <span class="flex items-center gap-1"><Clock :size="10" />{{ cls.lecturer }}</span>
              </div>
            </div>
          </div>
        </div>
      </LmsCard>

      <!-- Deadline & Nhiệm vụ -->
      <LmsCard variant="glass" class="lg:col-span-2 flex flex-col">
        <div class="border-b border-white/40 pb-3.5 mb-3 flex items-center justify-between">
          <h2 class="text-[15px] font-semibold text-slate-800">Sắp hết hạn</h2>
          <LmsBadge variant="warning" size="sm">
            {{ deadlines.filter(d => d.urgent).length }} gấp
          </LmsBadge>
        </div>
        <div class="flex-1 space-y-1.5 overflow-y-auto max-h-[300px]">
          <router-link
            v-for="d in deadlines" :key="d.id"
            :to="d.route"
            :class="['flex items-start gap-3 rounded-lg p-3 transition-colors group', d.urgent ? 'bg-yellow-50/60 border border-yellow-200/50 hover:border-yellow-300' : 'hover:bg-slate-50 border border-transparent']"
          >
            <div class="mt-0.5 flex-shrink-0">
              <span :class="['flex h-2 w-2 rounded-full', d.urgent ? 'bg-red-500 animate-pulse' : 'bg-slate-300']" />
            </div>
            <div class="flex-1 min-w-0">
              <p :class="['text-[13px] font-medium leading-snug truncate', d.urgent ? 'text-red-700' : 'text-slate-700']">{{ d.label }}</p>
              <p class="text-[11px] text-slate-400 mt-0.5">{{ d.subject }}</p>
              <p :class="['mt-0.5 text-[11px] font-semibold', d.urgent ? 'text-red-500' : 'text-slate-400']">{{ d.due }}</p>
            </div>
            <span :class="['flex-shrink-0 rounded-full px-2 py-0.5 text-[10px] font-semibold', c(typeColor[d.type], 'badge')]">
              {{ typeLabel[d.type] }}
            </span>
          </router-link>
        </div>
        <div class="border-t border-slate-100 px-5 py-2.5">
          <router-link to="/student/assignments" class="text-xs font-medium text-blue-600 hover:text-blue-700">
            Xem tất cả bài tập →
          </router-link>
        </div>
      </LmsCard>
    </div>

    <!-- ══ ROW: Tiến độ + Thông báo ══ -->
    <div class="grid grid-cols-1 gap-4 xl:grid-cols-4">

      <!-- Tiến độ học tập -->
      <LmsCard variant="solid" padding="0" class="xl:col-span-2">
        <div class="flex items-center justify-between border-b border-white/55 bg-white/45 px-5 py-3.5">
          <div>
            <h2 class="text-[15px] font-semibold text-slate-800">Tiến độ học tập</h2>
            <p class="text-[11px] text-slate-400 mt-0.5">Học kỳ 2 · 2024 – 2025</p>
          </div>
          <router-link to="/student/courses" class="flex items-center gap-1 text-xs font-medium text-blue-600 hover:text-blue-700">
            Xem khóa học <ChevronRight :size="12" />
          </router-link>
        </div>
        <div class="p-5 grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div v-for="course in courseProgress" :key="course.code">
            <div class="flex items-center justify-between mb-1.5">
              <div class="min-w-0 pr-2">
                <p class="text-[12px] font-semibold text-slate-700 truncate leading-tight">{{ course.name }}</p>
                <p class="text-[11px] text-slate-400">{{ course.code }}</p>
              </div>
              <div class="flex-shrink-0 text-right">
                <span :class="['text-[12px] font-bold', c(course.color, 'text')]">{{ course.progress }}%</span>
                <p v-if="course.grade" class="text-[10px] text-green-600 font-semibold">{{ course.grade }}/10</p>
                <p v-else class="text-[10px] text-slate-400">Chưa có điểm</p>
              </div>
            </div>
            <div class="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
              <div :class="['h-full rounded-full transition-all duration-700', c(course.color, 'bar')]" :style="{ width: course.progress + '%' }" />
            </div>
            <div class="mt-1 flex items-center gap-1 text-[10px] text-slate-400">
              <CheckCircle2 :size="10" class="text-green-400" />
              {{ course.completed }}/{{ course.total }} buổi hoàn thành
            </div>
          </div>
        </div>
      </LmsCard>

      <LmsCard variant="glass" padding="0" class="flex flex-col">
        <div class="flex items-center justify-between border-b border-white/50 px-5 py-3.5">
          <div>
            <h2 class="text-[15px] font-semibold text-slate-800">Điểm mới nhất</h2>
            <p class="mt-0.5 text-[11px] text-slate-500">Theo dõi kết quả vừa công bố</p>
          </div>
          <Award :size="16" class="text-blue-500" />
        </div>
        <div class="space-y-2 p-4">
          <router-link
            v-for="grade in recentGrades"
            :key="grade.id"
            to="/student/grades"
            class="lg-nav flex items-center justify-between gap-3 rounded-2xl p-3 transition-all duration-200 hover:-translate-y-0.5"
          >
            <div class="min-w-0">
              <p class="truncate text-[13px] font-bold text-slate-900">{{ grade.subject }}</p>
              <p class="mt-0.5 text-[11px] text-slate-500">{{ grade.type }}</p>
            </div>
            <div class="text-right">
              <p :class="['text-lg font-extrabold', c(grade.color, 'text')]">{{ grade.score }}</p>
              <p class="text-[10px] font-semibold text-slate-400">{{ grade.trend }}</p>
            </div>
          </router-link>
        </div>
      </LmsCard>

      <!-- Thông báo hệ thống -->
      <LmsCard variant="solid" padding="0" class="flex flex-col">
        <div class="flex items-center justify-between border-b border-white/55 bg-white/45 px-5 py-3.5">
          <div class="flex items-center gap-2">
            <h2 class="text-[15px] font-semibold text-slate-800">Thông báo</h2>
            <span v-if="systemNotices.filter(n => !n.read).length" class="rounded-full bg-blue-600 px-1.5 py-0.5 text-[10px] font-bold text-white">
              {{ systemNotices.filter(n => !n.read).length }}
            </span>
          </div>
          <Bell :size="15" class="text-slate-400" />
        </div>
        <div class="flex-1 divide-y divide-slate-50 overflow-y-auto">
          <div
            v-for="n in systemNotices" :key="n.id"
            :class="['flex gap-3 px-4 py-3.5 cursor-pointer hover:bg-slate-50 transition-colors', !n.read ? 'bg-blue-50/30' : '']"
            @click="openNoticeDrawer(n)"
          >
            <div :class="['flex-shrink-0 flex h-8 w-8 items-center justify-center rounded-full', c(n.color, 'icon')]">
              <component :is="n.icon" :size="14" :class="c(n.color, 'text')" />
            </div>
            <div class="min-w-0 flex-1">
              <div class="flex items-start justify-between gap-1">
                <p :class="['text-[12px] leading-snug', !n.read ? 'font-semibold text-slate-800' : 'font-medium text-slate-700']">{{ n.title }}</p>
                <span v-if="!n.read" class="mt-1 h-1.5 w-1.5 flex-shrink-0 rounded-full bg-blue-500" />
              </div>
              <p class="mt-0.5 text-[11px] text-slate-500 line-clamp-1">{{ n.body }}</p>
              <p class="mt-0.5 text-[10px] text-slate-400">{{ n.time }}</p>
            </div>
          </div>
        </div>
        <div class="border-t border-slate-100 px-5 py-2.5">
          <button class="text-xs font-medium text-blue-600 hover:text-blue-700">Xem tất cả thông báo →</button>
        </div>
      </LmsCard>
    </div>

    <!-- ══ DRAWER: Chi tiết thông báo ══ -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-200"
      leave-from-class="opacity-100" leave-to-class="opacity-0"
    >
      <div v-if="drawerOpen" class="fixed inset-0 z-50 bg-slate-900/40 backdrop-blur-sm" @click="closeDrawer" />
    </Transition>
    <Transition
      enter-active-class="transition-transform duration-300 ease-out"
      enter-from-class="translate-x-full" enter-to-class="translate-x-0"
      leave-active-class="transition-transform duration-200 ease-in"
      leave-from-class="translate-x-0" leave-to-class="translate-x-full"
    >
      <div v-if="drawerOpen && selectedNotice" class="lg-solid fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-6 py-4">
          <h3 class="text-base font-semibold text-slate-800">Chi tiết thông báo</h3>
          <button
            class="rounded-lg p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600 focus:outline-none focus:ring-4 focus:ring-blue-500/20"
            aria-label="Đóng thông báo"
            @click="closeDrawer"
          >
            <X :size="18" />
          </button>
        </div>
        <div class="flex-1 overflow-y-auto p-6">
          <div :class="['inline-flex h-12 w-12 items-center justify-center rounded-2xl mb-4', c(selectedNotice.color, 'icon')]">
            <component :is="selectedNotice.icon" :size="22" :class="c(selectedNotice.color, 'text')" />
          </div>
          <h4 class="text-lg font-bold text-slate-800">{{ selectedNotice.title }}</h4>
          <p class="mt-1 text-xs text-slate-400">{{ selectedNotice.time }}</p>
          <p class="mt-4 text-sm text-slate-600 leading-relaxed">{{ selectedNotice.body }}</p>
          <div class="mt-6 rounded-xl border border-slate-100 bg-slate-50 p-4">
            <p class="text-xs text-slate-500">Nếu cần hỗ trợ thêm, vui lòng liên hệ phòng đào tạo hoặc tạo ticket hỗ trợ.</p>
          </div>
        </div>
        <div class="border-t border-slate-100 px-6 py-4 flex gap-3">
          <router-link to="/student/support-tickets" class="flex-1 rounded-xl border border-slate-200 py-2.5 text-center text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-50 focus:outline-none focus:ring-4 focus:ring-blue-500/20">
            Tạo ticket hỗ trợ
          </router-link>
          <button
            class="flex-1 rounded-xl bg-blue-900 py-2.5 text-sm font-semibold text-white shadow-lg shadow-blue-900/20 transition-colors hover:bg-blue-800 focus:outline-none focus:ring-4 focus:ring-blue-500/25"
            @click="closeDrawer"
          >
            Đã hiểu
          </button>
        </div>
      </div>
    </Transition>

  </div>
</template>
