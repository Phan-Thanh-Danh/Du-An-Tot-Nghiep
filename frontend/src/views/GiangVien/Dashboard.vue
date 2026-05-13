<script setup>
import { ref } from 'vue'
import {
  Users,
  BookOpen,
  ClipboardCheck,
  Clock,
  ChevronRight,
  TrendingUp,
  MoreVertical,
  Calendar,
  Search,
  Bell,
  ArrowUpRight,
  GraduationCap,
  MessageSquare,
  AlertCircle
} from 'lucide-vue-next'

defineOptions({
  name: 'TeacherDashboard',
})

// ── KPI Stats ──────────────────────────────────────────────
const stats = [
  { id: 'total-students', label: 'Tổng sinh viên', value: '452', growth: '+12%', icon: Users, color: 'blue' },
  { id: 'active-classes', label: 'Lớp đang dạy', value: '8', growth: 'Học kỳ 2', icon: BookOpen, color: 'indigo' },
  { id: 'pending-grading', label: 'Bài chờ chấm', value: '24', growth: '6 bài gấp', icon: ClipboardCheck, color: 'orange', urgent: true },
  { id: 'avg-performance', label: 'Hiệu suất lớp', value: '82%', growth: '+5%', icon: TrendingUp, color: 'green' },
]

// ── Today's Teaching Schedule ──────────────────────────────
const teachingSchedule = [
  {
    id: 1,
    subject: 'Cấu trúc dữ liệu & Giải thuật',
    code: 'CTDL101_L01',
    time: '07:30 - 09:30',
    room: 'Phòng 302 - Cơ sở chính',
    students: 45,
    status: 'completed'
  },
  {
    id: 2,
    subject: 'Lập trình hướng đối tượng',
    code: 'OOP202_L03',
    time: '13:30 - 15:30',
    room: 'Phòng 105 - Cơ sở chính',
    students: 38,
    status: 'upcoming'
  },
  {
    id: 3,
    subject: 'Hệ quản trị CSDL',
    code: 'DBM301_L02',
    time: '15:45 - 17:45',
    room: 'Lab 2 - Cơ sở phụ',
    students: 42,
    status: 'upcoming'
  }
]

// ── Recent Activity / Assignments ──────────────────────────
const recentSubmissions = [
  { id: 1, student: 'Lê Văn B', course: 'Lập trình Web', assignment: 'Bài tập 2: CSS Flexbox', time: '10 phút trước', status: 'new' },
  { id: 2, student: 'Trần Thị C', course: 'OOP', assignment: 'Lab 4: Polymorphism', time: '45 phút trước', status: 'new' },
  { id: 3, student: 'Nguyễn Văn A', course: 'CTDL&GT', assignment: 'Tiểu luận giữa kỳ', time: '2 giờ trước', status: 'graded' },
  { id: 4, student: 'Phạm Minh D', course: 'HQTCSDL', assignment: 'Truy vấn SQL nâng cao', time: 'Hôm qua', status: 'graded' },
]

// ── Helpers ──────────────────────────────────────────────
const colorMap = {
  blue:   'bg-blue-50 text-blue-600 border-blue-100',
  indigo: 'bg-indigo-50 text-indigo-600 border-indigo-100',
  orange: 'bg-orange-50 text-orange-600 border-orange-100',
  green:  'bg-green-50 text-green-600 border-green-100',
}

const getStatusBadge = (status) => {
  switch (status) {
    case 'completed': return 'bg-slate-100 text-slate-500'
    case 'upcoming': return 'bg-indigo-100 text-indigo-700'
    case 'new': return 'bg-blue-100 text-blue-700'
    case 'graded': return 'bg-green-100 text-green-700'
    default: return 'bg-slate-100 text-slate-500'
  }
}
</script>

<template>
  <div class="space-y-6 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="lg-glass-strong relative overflow-hidden rounded-[32px] bg-white px-6 py-8 shadow-sm">
      <div class="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-slate-900">Chào buổi sáng, TS. Nguyễn Minh Khoa! 👋</h1>
          <p class="mt-1 text-slate-500">Hôm nay là Thứ 2, ngày 12 tháng 5 năm 2026. Chúc bạn một ngày làm việc hiệu quả.</p>
          <div class="mt-4 flex items-center gap-4">
            <div class="flex items-center gap-1.5 text-sm font-medium text-slate-600">
              <Calendar :size="16" class="text-indigo-500" /> Học kỳ 2, 2023-2024
            </div>
            <div class="h-4 w-px bg-slate-200"></div>
            <div class="flex items-center gap-1.5 text-sm font-medium text-slate-600">
              <Clock :size="16" class="text-orange-500" /> Tuần học thứ 12
            </div>
          </div>
        </div>
        <div class="shrink-0">
          <router-link to="/teacher/schedule" class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-indigo-100">Xem lịch dạy</router-link>
        </div>
      </div>
      <!-- Background decoration -->
      <div class="absolute -right-20 -top-20 h-64 w-64 rounded-full bg-indigo-50/50 blur-3xl"></div>
    </div>

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <div v-for="stat in stats" :key="stat.id" class="lg-card-glass group p-5 transition-all hover:scale-[1.02]">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-xl border', colorMap[stat.color]]">
            <component :is="stat.icon" :size="20" />
          </div>
          <span :class="['text-xs font-bold', stat.urgent ? 'text-orange-600' : 'text-green-600']">{{ stat.growth }}</span>
        </div>
        <div class="mt-3">
          <p class="text-2xl font-bold text-slate-900">{{ stat.value }}</p>
          <p class="text-xs font-medium text-slate-500">{{ stat.label }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Grid ── -->
    <div class="grid grid-cols-1 gap-6 xl:grid-cols-3">
      <!-- Teaching Schedule -->
      <div class="xl:col-span-2 space-y-4">
        <div class="flex items-center justify-between px-2">
          <h2 class="text-lg font-bold text-slate-800 flex items-center gap-2">
            <BookOpen :size="20" class="text-indigo-600" /> Lịch dạy hôm nay
          </h2>
          <router-link to="/teacher/schedule" class="text-xs font-semibold text-indigo-600 hover:underline">Xem tất cả</router-link>
        </div>

        <div class="space-y-3">
          <div v-for="item in teachingSchedule" :key="item.id" class="lg-card-glass flex flex-col sm:flex-row sm:items-center justify-between p-5 gap-4">
            <div class="flex items-center gap-4">
              <div class="h-12 w-12 rounded-2xl bg-slate-50 flex flex-col items-center justify-center border border-slate-100">
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-tighter">{{ item.time.split(' ')[0] }}</span>
                <span class="text-xs font-black text-slate-700">AM</span>
              </div>
              <div>
                <h3 class="font-bold text-slate-800 leading-snug">{{ item.subject }}</h3>
                <div class="flex items-center gap-3 mt-1">
                  <span class="text-xs text-slate-500 font-medium">{{ item.code }}</span>
                  <span class="h-1 w-1 rounded-full bg-slate-300"></span>
                  <span class="text-xs text-slate-500 font-medium">{{ item.room }}</span>
                </div>
              </div>
            </div>
            <div class="flex items-center justify-between sm:justify-end gap-4 border-t sm:border-0 pt-3 sm:pt-0">
               <div class="flex items-center gap-1.5 text-xs font-semibold text-slate-600">
                 <Users :size="14" class="text-slate-400" /> {{ item.students }} SV
               </div>
               <span :class="['px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider', getStatusBadge(item.status)]">
                 {{ item.status === 'completed' ? 'Đã hoàn thành' : 'Sắp diễn ra' }}
               </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Recent Activities & Tasks -->
      <div class="space-y-6">
        <!-- Submissions -->
        <div class="lg-glass-card p-5">
          <h3 class="font-bold text-slate-800 mb-4 flex items-center gap-2">
            <ClipboardCheck :size="18" class="text-orange-600" /> Bài nộp mới nhất
          </h3>
          <div class="space-y-4">
            <div v-for="sub in recentSubmissions" :key="sub.id" class="flex gap-3 group cursor-pointer">
              <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center shrink-0 group-hover:bg-indigo-100 transition-colors">
                <User class="h-4 w-4 text-slate-400 group-hover:text-indigo-600" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-bold text-slate-800 truncate">{{ sub.student }}</p>
                <p class="text-[11px] text-slate-500 truncate">{{ sub.assignment }}</p>
                <div class="flex items-center justify-between mt-1">
                  <span class="text-[10px] text-slate-400">{{ sub.time }}</span>
                  <span v-if="sub.status === 'new'" class="h-1.5 w-1.5 rounded-full bg-blue-500 animate-pulse"></span>
                </div>
              </div>
            </div>
          </div>
          <button class="w-full mt-5 py-2 text-xs font-bold text-indigo-600 bg-indigo-50 rounded-xl hover:bg-indigo-100 transition-colors">Xem tất cả bài nộp</button>
        </div>

        <!-- Quick Tips/Reminders -->
        <div class="lg-glass-strong bg-gradient-to-br from-indigo-600 to-indigo-800 p-5 rounded-[24px] text-white shadow-lg shadow-indigo-200">
           <h3 class="font-bold mb-2 flex items-center gap-2">
             <AlertCircle :size="18" /> Nhắc nhở giáo vụ
           </h3>
           <p class="text-xs text-indigo-100 leading-relaxed">Hạn cuối nhập điểm giữa kỳ cho các lớp Block 1 là ngày 15/05. Vui lòng hoàn tất đúng hạn.</p>
           <button class="mt-4 text-xs font-bold bg-white/20 hover:bg-white/30 px-3 py-1.5 rounded-lg transition-all">Chi tiết công văn</button>
        </div>
      </div>
    </div>
  </div>
</template>
