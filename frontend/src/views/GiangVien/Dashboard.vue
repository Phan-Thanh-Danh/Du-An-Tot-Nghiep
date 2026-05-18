<template>
  <div class="space-y-6 pb-10">
    
    <!-- ── Welcome Hero (GiangVien Style - matching BGH) ── -->
    <div class="relative overflow-hidden rounded-[32px] bg-blue-900 p-8 text-white shadow-2xl shadow-blue-200 animate-fade-in">
      <div class="absolute -right-24 -top-24 h-64 w-64 rounded-full bg-blue-500/20 blur-3xl transform-gpu pointer-events-none will-change-transform" />
      <div class="absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-cyan-500/20 blur-3xl transform-gpu pointer-events-none will-change-transform" />
      
      <div class="relative flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-3xl md:text-4xl font-extrabold leading-tight tracking-tight">
            Chào buổi sáng, <span class="text-blue-200">TS. Nguyễn Minh Khoa!</span>
          </h1>
          <p class="mt-3 text-blue-100/80 text-lg">
            Hôm nay là Thứ 2, ngày 12 tháng 5. Bạn có 2 ca dạy và 24 bài tập đang chờ chấm điểm. Chúc bạn một ngày làm việc hiệu quả.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/teacher/schedule" class="rounded-2xl bg-white px-6 py-3 text-sm font-bold text-blue-900 shadow-lg hover:bg-blue-50 transition-all active:scale-95">
              Xem lịch dạy
            </router-link>
            <router-link to="/teacher/grading" class="rounded-2xl bg-blue-700/50 backdrop-blur px-6 py-3 text-sm font-bold text-white border border-blue-400/30 hover:bg-blue-700 transition-all">
              Chấm điểm ngay
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="relative h-48 w-48 rounded-[40px] bg-gradient-to-tr from-blue-400 to-cyan-400 p-1 rotate-3 shadow-xl">
             <div class="h-full w-full rounded-[38px] bg-blue-900/40 backdrop-blur-sm flex items-center justify-center border border-white/20">
               <BookOpen :size="80" class="text-white/80" />
             </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 animate-fade-in delay-100">
      <div v-for="item in stats" :key="item.id" 
           class="group relative overflow-hidden rounded-[24px] border border-white bg-white p-6 shadow-sm transition-all hover:shadow-xl hover:-translate-y-1">
        <div class="flex items-center justify-between">
          <div :class="['flex h-12 w-12 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isNegative ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-600']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="12" />
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
      
      <!-- Left: Schedule & Chart -->
      <div class="xl:col-span-2 space-y-6 animate-fade-in delay-200">
        
        <!-- Today's Schedule -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <div>
              <h2 class="text-xl font-bold text-slate-800">Lịch dạy hôm nay</h2>
              <p class="text-sm text-slate-400 mt-0.5">Các lớp học bạn phụ trách trong ngày</p>
            </div>
            <router-link to="/teacher/schedule" class="text-sm font-bold text-blue-600 hover:text-blue-700">Xem tất cả</router-link>
          </div>
          <div class="p-6 space-y-4">
             <div v-for="item in teachingSchedule" :key="item.id" 
                  class="group flex flex-col sm:flex-row sm:items-center justify-between p-4 rounded-2xl border border-slate-50 bg-slate-50/30 transition-all hover:bg-white hover:shadow-md cursor-pointer hover:border-blue-100">
               <div class="flex items-center gap-4">
                 <div class="h-12 w-12 rounded-2xl bg-blue-100 text-blue-700 flex flex-col items-center justify-center border border-blue-200">
                   <span class="text-[10px] font-bold uppercase tracking-tighter">{{ item.time.split(' ')[0] }}</span>
                   <span class="text-[10px] font-black">AM</span>
                 </div>
                 <div>
                   <h3 class="font-bold text-slate-800 leading-snug group-hover:text-blue-700 transition-colors">{{ item.subject }}</h3>
                   <div class="flex items-center gap-3 mt-1">
                     <span class="text-xs text-slate-500 font-medium">{{ item.code }}</span>
                     <span class="h-1 w-1 rounded-full bg-slate-300"></span>
                     <span class="text-xs text-slate-500 font-medium">{{ item.room }}</span>
                   </div>
                 </div>
               </div>
               <div class="flex items-center justify-between sm:justify-end gap-4 border-t sm:border-0 pt-3 sm:pt-0 mt-3 sm:mt-0">
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

        <!-- Simulated Trend Chart -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden p-8">
           <div class="flex items-center justify-between mb-8">
              <h2 class="text-xl font-bold text-slate-800">Tiến độ nộp bài tập (Tuần 12)</h2>
              <select class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2 text-xs font-bold outline-none">
                 <option>Tất cả các lớp</option>
                 <option>CTDL101_L01</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-40 gap-4 border-b border-slate-100 pb-2">
              <div v-for="(h, i) in [85, 65, 90, 55, 95]" :key="i" class="flex-1 flex flex-col items-center gap-2">
                 <div class="w-full bg-blue-600 rounded-t-xl transition-all hover:bg-blue-700" :style="{ height: h + '%' }" />
                 <span class="text-[10px] font-bold text-slate-400">{{ ['Lab 1', 'Lab 2', 'Asm 1', 'Quiz 1', 'Lab 3'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Submissions & Reminders -->
      <div class="space-y-6 animate-fade-in delay-300">
        
        <!-- Recent Submissions -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
           <div class="mb-6 flex items-center justify-between">
             <h3 class="text-lg font-bold text-slate-800">Bài Nộp Mới</h3>
             <span class="rounded-full bg-orange-100 px-2.5 py-0.5 text-xs font-bold text-orange-700">6 Gấp</span>
           </div>
           <div class="space-y-4">
             <div v-for="sub in recentSubmissions" :key="sub.id" 
                  class="p-4 rounded-2xl border border-slate-50 bg-slate-50/30 transition-all hover:bg-white hover:shadow-md cursor-pointer">
               <div class="flex justify-between items-start">
                  <div class="flex items-center gap-2">
                    <div class="h-6 w-6 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center shrink-0">
                      <User :size="12" />
                    </div>
                    <p class="text-sm font-bold text-slate-800 leading-tight">{{ sub.student }}</p>
                  </div>
                  <span v-if="sub.status === 'new'" class="text-[10px] font-bold text-blue-600 animate-pulse">NEW</span>
               </div>
               <p class="mt-2 text-xs font-medium text-slate-700">{{ sub.assignment }}</p>
               <div class="mt-2 flex items-center justify-between">
                 <p class="text-[10px] text-slate-500">{{ sub.course }} • {{ sub.time }}</p>
                 <button class="text-[10px] font-bold text-blue-600 hover:underline">Chấm ngay →</button>
               </div>
             </div>
           </div>
           <button class="mt-4 w-full text-center text-xs font-bold text-blue-600 hover:text-blue-700 bg-blue-50/50 py-2 rounded-xl">Xem tất cả bài nộp</button>
        </div>

        <!-- Reminders / Urgent Actions -->
        <div class="rounded-[28px] border border-slate-100 bg-blue-900 p-6 text-white overflow-hidden relative">
          <div class="absolute -right-10 -bottom-10 h-32 w-32 rounded-full bg-white/10 blur-2xl transform-gpu pointer-events-none will-change-transform" />
          <div class="flex items-center gap-2">
             <h3 class="text-lg font-bold">Nhắc nhở giáo vụ</h3>
             <AlertCircle class="w-4 h-4 text-orange-300" />
          </div>
          <p class="text-sm text-blue-100/70 mt-2">Hạn cuối nhập điểm giữa kỳ cho các lớp Block 1 là ngày 15/05. Vui lòng hoàn tất đúng hạn.</p>
          
          <button class="mt-6 w-full text-center text-xs font-bold text-blue-900 bg-white py-2 rounded-xl shadow-lg hover:bg-blue-50 transition-all">Chi tiết công văn</button>
        </div>

        <!-- Announcements -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-bold text-slate-800">Thông báo</h3>
            <Bell :size="18" class="text-slate-400" />
          </div>
          <div class="space-y-4">
            <div class="flex gap-3">
              <div class="h-10 w-10 rounded-full bg-green-50 flex items-center justify-center text-green-500 shrink-0">
                <Users :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700">Họp bộ môn thường kỳ</p>
                <p class="text-xs text-slate-500 mt-1">14:00 Thứ 6, ngày 16/05 tại Phòng họp 2.</p>
              </div>
            </div>
          </div>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { 
  Users, BookOpen, ClipboardCheck, Clock, TrendingUp, Calendar, Search, Bell, 
  ArrowUpRight, GraduationCap, MessageSquare, AlertCircle, User
} from 'lucide-vue-next'

// ── KPI Stats ──
const stats = [
  { id: 1, label: 'Tổng sinh viên', value: '452', trend: '+12%', isNegative: false, bgColor: 'bg-blue-50', iconColor: 'text-blue-600', icon: Users },
  { id: 2, label: 'Lớp đang dạy', value: '8', trend: 'Học kỳ 2', isNegative: false, bgColor: 'bg-blue-50', iconColor: 'text-blue-600', icon: BookOpen },
  { id: 3, label: 'Bài chờ chấm', value: '24', trend: '6 bài gấp', isNegative: true, bgColor: 'bg-orange-50', iconColor: 'text-orange-600', icon: ClipboardCheck },
  { id: 4, label: 'Hiệu suất lớp', value: '82%', trend: '+5%', isNegative: false, bgColor: 'bg-green-50', iconColor: 'text-green-600', icon: TrendingUp },
]

// ── Today's Teaching Schedule ──
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

// ── Recent Activity / Assignments ──
const recentSubmissions = [
  { id: 1, student: 'Lê Văn B', course: 'Lập trình Web', assignment: 'Bài tập 2: CSS Flexbox', time: '10 phút trước', status: 'new' },
  { id: 2, student: 'Trần Thị C', course: 'OOP', assignment: 'Lab 4: Polymorphism', time: '45 phút trước', status: 'new' },
  { id: 3, student: 'Nguyễn Văn A', course: 'CTDL&GT', assignment: 'Tiểu luận giữa kỳ', time: '2 giờ trước', status: 'graded' },
  { id: 4, student: 'Phạm Minh D', course: 'HQTCSDL', assignment: 'Truy vấn SQL nâng cao', time: 'Hôm qua', status: 'graded' },
]

// ── Helpers ──
const getStatusBadge = (status) => {
  switch (status) {
    case 'completed': return 'bg-slate-100 text-slate-500'
    case 'upcoming': return 'bg-blue-100 text-blue-700'
    case 'new': return 'bg-blue-100 text-blue-700'
    case 'graded': return 'bg-green-100 text-green-700'
    default: return 'bg-slate-100 text-slate-500'
  }
}
</script>

<style scoped>
.shadow-blue-200 {
  shadow-color: rgba(79, 70, 229, 0.2);
}
.transition-all {
  transition-duration: 400ms;
  transition-timing-function: cubic-bezier(0.16, 1, 0.3, 1);
}
@keyframes pulse-soft {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(1.2); opacity: 0.3; }
}
.animate-pulse {
  animation: pulse-soft 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
  will-change: opacity, transform;
}
.delay-100 { animation-delay: 100ms; }
.delay-200 { animation-delay: 200ms; }
.delay-300 { animation-delay: 300ms; }
</style>

