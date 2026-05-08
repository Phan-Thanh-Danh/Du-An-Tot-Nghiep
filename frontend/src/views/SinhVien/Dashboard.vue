<script setup>
import { ref } from 'vue'
import {
  BookOpen, ClipboardList, CalendarDays, CreditCard,
  AlertTriangle, Clock, MapPin,
  ChevronRight, CheckCircle2, Bell, X,
  Star, UserCheck, BarChart2,
} from 'lucide-vue-next'

defineOptions({
  name: 'StudentDashboard',
})

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
  <div class="space-y-6">

    <!-- ══ ALERT: Deadline hôm nay ══ -->
    <div
      v-if="deadlines.some(d => d.urgent && !d.done)"
      class="flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 px-4 py-3"
    >
      <AlertTriangle :size="18" class="flex-shrink-0 text-red-500" />
      <p class="flex-1 text-sm font-medium text-red-700">
        Bạn có <strong>{{ deadlines.filter(d => d.urgent && !d.done).length }} bài tập</strong> hết hạn hôm nay!
      </p>
      <router-link to="/student/assignments" class="flex-shrink-0 rounded-lg bg-red-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-red-700 transition-colors">
        Xem ngay
      </router-link>
    </div>

    <!-- ══ KPI CARDS ══ -->
    <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-3">
      <router-link
        v-for="card in kpiCards" :key="card.id"
        :to="card.route"
        :class="['group relative overflow-hidden rounded-2xl border bg-white p-4 shadow-sm transition-all hover:shadow-md hover:-translate-y-0.5 cursor-pointer', card.urgent ? 'border-orange-200' : 'border-slate-100']"
      >
        <div :class="['absolute right-0 top-0 h-16 w-16 rounded-bl-full opacity-40', c(card.color,'bg')]" />
        <div class="relative space-y-2">
          <div :class="['inline-flex h-8 w-8 items-center justify-center rounded-lg', c(card.color,'icon')]">
            <component :is="card.icon" :size="16" :class="c(card.color,'text')" stroke-width="2" />
          </div>
          <div class="flex items-baseline gap-1">
            <span :class="['text-xl font-bold text-slate-800', card.urgent ? 'text-orange-600' : '']">{{ card.value }}</span>
            <span v-if="card.unit" class="text-xs text-slate-500">{{ card.unit }}</span>
          </div>
          <p class="text-[12px] text-slate-500 leading-tight">{{ card.label }}</p>
          <p :class="['text-[11px] leading-tight', card.urgent ? 'text-orange-500 font-semibold' : 'text-slate-400']">{{ card.sub }}</p>
        </div>
        <ChevronRight :size="14" class="absolute right-3 bottom-3 text-slate-300 group-hover:text-blue-400 transition-colors" />
      </router-link>
    </div>

    <!-- ══ ROW: Lịch hôm nay + Deadline ══ -->
    <div class="grid grid-cols-1 lg:grid-cols-5 gap-4">

      <!-- Lịch học hôm nay -->
      <div class="lg:col-span-3 rounded-2xl border border-slate-100 bg-white shadow-sm">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-3.5">
          <div>
            <h2 class="text-[15px] font-semibold text-slate-800">Lịch học hôm nay</h2>
            <p class="text-[11px] text-slate-400 mt-0.5">Chủ nhật, 04 tháng 05 năm 2025</p>
          </div>
          <router-link to="/student/schedule" class="flex items-center gap-1 text-xs font-medium text-blue-600 hover:text-blue-700">
            Xem đầy đủ <ChevronRight :size="12" />
          </router-link>
        </div>
        <div class="p-4 space-y-3">
          <!-- Empty state -->
          <div v-if="todayClasses.length === 0" class="flex flex-col items-center py-10 text-slate-400">
            <CalendarDays :size="36" stroke-width="1.2" />
            <p class="mt-2 text-sm">Không có lịch học hôm nay 🎉</p>
          </div>

          <div
            v-for="cls in todayClasses" :key="cls.id"
            :class="['flex gap-3 rounded-xl border p-3.5 transition-all', cls.status === 'done' ? 'border-slate-100 opacity-60' : 'border-blue-100 bg-blue-50/30 hover:border-blue-200']"
          >
            <!-- Time -->
            <div class="flex-shrink-0 w-[56px] text-center">
              <div :class="['rounded-lg px-1.5 py-1.5 text-[11px] leading-snug font-semibold', cls.status === 'done' ? 'bg-slate-100 text-slate-500' : 'bg-blue-100 text-blue-700']">
                {{ cls.timeStart }}<br /><span class="font-normal">{{ cls.timeEnd }}</span>
              </div>
            </div>
            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-start justify-between gap-2">
                <p class="text-[13px] font-semibold text-slate-800 leading-tight truncate">{{ cls.subject }}</p>
                <span :class="['flex-shrink-0 rounded-full px-2 py-0.5 text-[10px] font-semibold', cls.status === 'done' ? 'bg-slate-100 text-slate-500' : 'bg-green-100 text-green-700']">
                  {{ cls.status === 'done' ? 'Đã học' : 'Sắp tới' }}
                </span>
              </div>
              <p class="text-[11px] text-slate-500 mt-0.5">{{ cls.code }} · {{ cls.type }}</p>
              <div class="mt-1.5 flex flex-wrap gap-x-3 text-[11px] text-slate-500">
                <span class="flex items-center gap-1"><MapPin :size="10" />{{ cls.room }} – {{ cls.campus }}</span>
                <span class="flex items-center gap-1"><Clock :size="10" />{{ cls.lecturer }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Deadline & Nhiệm vụ -->
      <div class="lg:col-span-2 rounded-2xl border border-slate-100 bg-white shadow-sm flex flex-col">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-3.5">
          <h2 class="text-[15px] font-semibold text-slate-800">Sắp hết hạn</h2>
          <span class="rounded-full bg-red-100 px-2 py-0.5 text-[10px] font-bold text-red-600">
            {{ deadlines.filter(d => d.urgent).length }} gấp
          </span>
        </div>
        <div class="flex-1 p-3 space-y-1.5 overflow-y-auto max-h-[300px]">
          <router-link
            v-for="d in deadlines" :key="d.id"
            :to="d.route"
            :class="['flex items-start gap-3 rounded-xl p-3 transition-colors hover:bg-slate-50 group', d.urgent ? 'bg-red-50/50' : '']"
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
      </div>
    </div>

    <!-- ══ ROW: Tiến độ + Thông báo ══ -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">

      <!-- Tiến độ học tập -->
      <div class="lg:col-span-2 rounded-2xl border border-slate-100 bg-white shadow-sm">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-3.5">
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
      </div>

      <!-- Thông báo hệ thống -->
      <div class="rounded-2xl border border-slate-100 bg-white shadow-sm flex flex-col">
        <div class="flex items-center justify-between border-b border-slate-100 px-5 py-3.5">
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
      </div>
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
      <div v-if="drawerOpen && selectedNotice" class="fixed inset-y-0 right-0 z-50 flex w-full max-w-md flex-col bg-white shadow-2xl">
        <div class="flex items-center justify-between border-b border-slate-100 px-6 py-4">
          <h3 class="text-base font-semibold text-slate-800">Chi tiết thông báo</h3>
          <button @click="closeDrawer" class="rounded-lg p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600">
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
          <router-link to="/student/support-tickets" class="flex-1 rounded-lg border border-slate-200 py-2.5 text-center text-sm font-medium text-slate-600 hover:bg-slate-50 transition-colors">
            Tạo ticket hỗ trợ
          </router-link>
          <button @click="closeDrawer" class="flex-1 rounded-lg bg-blue-600 py-2.5 text-sm font-medium text-white hover:bg-blue-700 transition-colors">
            Đã hiểu
          </button>
        </div>
      </div>
    </Transition>

  </div>
</template>
