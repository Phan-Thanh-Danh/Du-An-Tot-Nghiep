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

// ── Pending Tasks ──────────────────────────────────────────
const pendingTasks = [
  { id: 1, task: 'Chấm điểm tiểu luận nhóm lớp L01', deadline: 'Hôm nay', urgent: true },
  { id: 2, task: 'Cập nhật tài liệu tuần 12 môn OOP', deadline: 'Thứ 4', urgent: false },
  { id: 3, task: 'Xác nhận danh sách thực tập', deadline: 'Thứ 6', urgent: false },
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
    <div class="relative overflow-hidden rounded-[32px] bg-indigo-900 p-8 text-white shadow-2xl shadow-indigo-200">
      <div class="absolute -right-24 -top-24 h-64 w-64 rounded-full bg-indigo-500/20 blur-3xl" />
      <div class="absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-purple-500/20 blur-3xl" />
      
      <div class="relative flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-3xl md:text-4xl font-extrabold leading-tight tracking-tight">
            Chào buổi tối, <span class="text-indigo-200">TS. Nguyễn Minh Khoa!</span>
          </h1>
          <p class="mt-3 text-indigo-100/80 text-lg">
            Bạn có 2 lớp học vào chiều nay và 24 bài tập đang chờ chấm điểm. Hãy bắt đầu một ngày làm việc hiệu quả nhé!
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <button class="rounded-2xl bg-white px-6 py-3 text-sm font-bold text-indigo-900 shadow-lg hover:bg-indigo-50 transition-all active:scale-95">
              Chấm bài ngay
            </button>
            <button class="rounded-2xl bg-indigo-700/50 backdrop-blur px-6 py-3 text-sm font-bold text-white border border-indigo-400/30 hover:bg-indigo-700 transition-all">
              Tạo thông báo
            </button>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="relative h-48 w-48 rounded-[40px] bg-gradient-to-tr from-indigo-400 to-purple-400 p-1 rotate-3 shadow-xl">
             <div class="h-full w-full rounded-[38px] bg-indigo-900/40 backdrop-blur-sm flex items-center justify-center border border-white/20">
               <GraduationCap :size="80" class="text-white/80" />
             </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      <div v-for="item in stats" :key="item.id" 
           class="group relative overflow-hidden rounded-[24px] border border-white bg-white p-6 shadow-sm transition-all hover:shadow-xl hover:-translate-y-1">
        <div class="flex items-center justify-between">
          <div :class="['flex h-12 w-12 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', colorMap[item.color]]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.urgent ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-600']">
            {{ item.growth }}
            <ArrowUpRight v-if="!item.urgent" :size="12" />
            <AlertCircle v-else :size="12" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-medium text-slate-500">{{ item.label }}</p>
          <p class="mt-1 text-3xl font-black text-slate-800">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <!-- Left: Schedule & Activity -->
      <div class="xl:col-span-2 space-y-6">
        
        <!-- Today's Classes -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <div>
              <h2 class="text-xl font-bold text-slate-800">Lịch giảng dạy hôm nay</h2>
              <p class="text-sm text-slate-400 mt-0.5">Thứ 3, 12 tháng 05 năm 2026</p>
            </div>
            <button class="text-sm font-bold text-indigo-600 hover:text-indigo-700">Xem toàn bộ lịch</button>
          </div>
          <div class="p-4 space-y-4">
            <div v-for="cls in teachingSchedule" :key="cls.id" 
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-4 rounded-3xl border border-slate-50 p-5 transition-all hover:border-indigo-100 hover:bg-indigo-50/20">
              <div :class="['flex h-16 w-16 flex-shrink-0 flex-col items-center justify-center rounded-2xl font-bold', cls.status === 'completed' ? 'bg-slate-50 text-slate-400' : 'bg-indigo-600 text-white shadow-lg shadow-indigo-100']">
                 <span class="text-xs font-normal">Tiết</span>
                 <span class="text-xl">{{ cls.id === 1 ? '1-3' : (cls.id === 2 ? '7-9' : '10-12') }}</span>
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-bold text-slate-800 truncate">{{ cls.subject }}</h3>
                  <span :class="['rounded-full px-3 py-1 text-[11px] font-bold uppercase tracking-wider', getStatusBadge(cls.status)]">
                    {{ cls.status === 'completed' ? 'Đã dạy' : 'Sắp diễn ra' }}
                  </span>
                </div>
                <div class="mt-2 flex flex-wrap items-center gap-x-5 gap-y-2 text-sm text-slate-500">
                  <div class="flex items-center gap-1.5"><Clock :size="16" class="text-slate-400" /> {{ cls.time }}</div>
                  <div class="flex items-center gap-1.5"><Calendar :size="16" class="text-slate-400" /> {{ cls.room }}</div>
                  <div class="flex items-center gap-1.5"><Users :size="16" class="text-slate-400" /> {{ cls.students }} SV</div>
                </div>
              </div>
              <div class="flex items-center gap-2 mt-2 sm:mt-0">
                <button class="rounded-xl bg-slate-100 px-4 py-2 text-xs font-bold text-slate-600 hover:bg-slate-200 transition-colors">Chi tiết</button>
                <button v-if="cls.status !== 'completed'" class="rounded-xl bg-indigo-600 px-4 py-2 text-xs font-bold text-white hover:bg-indigo-700 shadow-md transition-all active:scale-95">Điểm danh</button>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Submissions -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <h2 class="text-xl font-bold text-slate-800">Hoạt động sinh viên mới nhất</h2>
            <div class="flex items-center gap-2">
               <div class="relative hidden sm:block">
                  <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
                  <input type="text" placeholder="Tìm kiếm bài nộp..." class="rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300 transition-all w-48" />
               </div>
               <button class="rounded-xl border border-slate-100 p-2 text-slate-400 hover:bg-slate-50 transition-colors"><MoreVertical :size="18" /></button>
            </div>
          </div>
          <div class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
                  <th class="px-8 py-4">Sinh viên</th>
                  <th class="px-6 py-4">Khóa học / Bài tập</th>
                  <th class="px-6 py-4">Thời gian nộp</th>
                  <th class="px-6 py-4">Trạng thái</th>
                  <th class="px-8 py-4"></th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-50">
                <tr v-for="sub in recentSubmissions" :key="sub.id" class="group hover:bg-slate-50/80 transition-colors">
                  <td class="px-8 py-4">
                    <div class="flex items-center gap-3">
                      <div class="h-9 w-9 rounded-full bg-gradient-to-br from-indigo-100 to-purple-100 flex items-center justify-center text-indigo-600 font-bold text-xs">
                        {{ sub.student.split(' ').pop()[0] }}
                      </div>
                      <span class="text-sm font-semibold text-slate-700">{{ sub.student }}</span>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <p class="text-sm font-medium text-slate-800">{{ sub.assignment }}</p>
                    <p class="text-[11px] text-slate-400 mt-0.5">{{ sub.course }}</p>
                  </td>
                  <td class="px-6 py-4 text-sm text-slate-500">{{ sub.time }}</td>
                  <td class="px-6 py-4">
                    <span :class="['rounded-full px-3 py-1 text-[11px] font-bold', getStatusBadge(sub.status)]">
                      {{ sub.status === 'new' ? 'Chưa chấm' : 'Đã chấm' }}
                    </span>
                  </td>
                  <td class="px-8 py-4 text-right">
                    <button class="opacity-0 group-hover:opacity-100 rounded-lg p-2 text-indigo-600 hover:bg-indigo-50 transition-all">
                      <ChevronRight :size="20" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

      </div>

      <!-- Right: Tasks & Announcements -->
      <div class="space-y-6">
        
        <!-- Tasks -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
           <div class="mb-6 flex items-center justify-between">
             <h3 class="text-lg font-bold text-slate-800">Nhiệm vụ cần làm</h3>
             <button class="text-xs font-bold text-indigo-600">Thêm mới</button>
           </div>
           <div class="space-y-4">
             <div v-for="task in pendingTasks" :key="task.id" 
                  class="flex items-start gap-4 rounded-2xl border border-slate-50 bg-slate-50/30 p-4 transition-all hover:bg-white hover:shadow-md group">
               <div :class="['mt-1 h-5 w-5 rounded-md border-2 flex items-center justify-center transition-colors', task.urgent ? 'border-red-400 bg-red-50' : 'border-slate-200 bg-white group-hover:border-indigo-400']">
                  <div v-if="task.urgent" class="h-2 w-2 rounded-full bg-red-400" />
               </div>
               <div class="flex-1 min-w-0">
                 <p :class="['text-sm font-semibold leading-tight', task.urgent ? 'text-red-700' : 'text-slate-700']">{{ task.task }}</p>
                 <div class="mt-1.5 flex items-center gap-3 text-[11px] text-slate-400">
                    <span class="flex items-center gap-1"><Clock :size="12" /> {{ task.deadline }}</span>
                    <span v-if="task.urgent" class="flex items-center gap-1 text-red-500 font-bold uppercase"><AlertCircle :size="12" /> Gấp</span>
                 </div>
               </div>
             </div>
           </div>
        </div>

        <!-- Course Summary / Mini Chart (Simulated) -->
        <div class="rounded-[28px] border border-slate-100 bg-indigo-600 p-6 text-white overflow-hidden relative">
          <div class="absolute -right-10 -bottom-10 h-32 w-32 rounded-full bg-white/10 blur-2xl" />
          <h3 class="text-lg font-bold">Thống kê khóa học</h3>
          <p class="text-sm text-indigo-100/70 mt-1">Học kỳ này bạn đang dạy 6 khóa học với sự tăng trưởng ổn định.</p>
          
          <div class="mt-6 flex items-end gap-2 h-24">
            <div v-for="h in [40, 70, 50, 90, 60, 85, 45]" :key="h" 
                 class="flex-1 bg-white/20 rounded-t-lg transition-all hover:bg-white/40 cursor-help" 
                 :style="{ height: h + '%' }" 
                 :title="h + '%'" />
          </div>
          
          <div class="mt-6 grid grid-cols-2 gap-4">
             <div class="rounded-2xl bg-white/10 p-3 backdrop-blur-sm border border-white/10">
                <p class="text-[10px] uppercase font-bold text-indigo-200 tracking-wider">Hoàn thành</p>
                <p class="text-xl font-black mt-1">74%</p>
             </div>
             <div class="rounded-2xl bg-white/10 p-3 backdrop-blur-sm border border-white/10">
                <p class="text-[10px] uppercase font-bold text-indigo-200 tracking-wider">Hài lòng</p>
                <p class="text-xl font-black mt-1">4.8/5</p>
             </div>
          </div>
        </div>

        <!-- Notifications -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-bold text-slate-800">Thông báo mới</h3>
            <Bell :size="18" class="text-slate-400" />
          </div>
          <div class="space-y-4">
            <div class="flex gap-3">
              <div class="h-10 w-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-500 shrink-0">
                <MessageSquare :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700">Thông báo từ Phòng Đào tạo</p>
                <p class="text-xs text-slate-500 mt-1">Cập nhật lịch thi cuối kỳ cho các lớp CNTT...</p>
                <p class="text-[10px] text-slate-400 mt-1.5">2 giờ trước</p>
              </div>
            </div>
            <div class="flex gap-3">
              <div class="h-10 w-10 rounded-full bg-orange-50 flex items-center justify-center text-orange-500 shrink-0">
                <AlertCircle :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700">Cảnh báo hệ thống</p>
                <p class="text-xs text-slate-500 mt-1">Bảo trì hệ thống vào lúc 23:00 tối nay.</p>
                <p class="text-[10px] text-slate-400 mt-1.5">5 giờ trước</p>
              </div>
            </div>
          </div>
          <button class="mt-6 w-full rounded-xl bg-slate-50 py-3 text-xs font-bold text-slate-500 hover:bg-slate-100 transition-colors">Tất cả thông báo</button>
        </div>

      </div>

    </div>

  </div>
</template>

<style scoped>
.shadow-indigo-200 {
  shadow-color: rgba(79, 70, 229, 0.2);
}

/* Custom transitions */
.transition-all {
  transition-duration: 300ms;
}

/* Hide scrollbar for tables on mobile if needed */
.overflow-x-auto {
  scrollbar-width: thin;
}

@keyframes pulse-soft {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(1.2); opacity: 0.3; }
}

.animate-pulse {
  animation: pulse-soft 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
