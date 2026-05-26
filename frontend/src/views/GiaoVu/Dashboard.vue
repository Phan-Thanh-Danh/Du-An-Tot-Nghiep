<template>
  <div class="space-y-6 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="relative overflow-hidden rounded-[32px] lg-glass-card p-8">
      <div class="pointer-events-none absolute -right-24 -top-24 h-64 w-64 rounded-full bg-teal-400/20 dark:bg-teal-500/15 blur-[80px]" />
      <div class="pointer-events-none absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-emerald-400/20 dark:bg-emerald-500/15 blur-[80px]" />
      
      <div class="relative flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <div class="inline-flex items-center gap-1.5 rounded-full bg-teal-100/80 dark:bg-teal-500/20 px-3 py-1 text-[11px] font-semibold text-teal-700 dark:text-teal-300 backdrop-blur-xl border border-teal-200/50 dark:border-teal-500/20 mb-4">
            <Sparkles :size="10" />
            Học kỳ Spring 2026
          </div>
          <h1 class="text-3xl md:text-4xl font-extrabold leading-tight tracking-tight text-slate-900 dark:text-white">
            Chào buổi sáng, <span class="lg-text-gradient">Trần Thị Giáo Vụ!</span>
          </h1>
          <p class="mt-3 text-base text-slate-500 dark:text-slate-400">
            Hệ thống ghi nhận 3 lịch học xung đột và 28 đơn từ đang chờ xử lý.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/staff/conflicts" class="lg-button-primary h-11 rounded-2xl px-6 text-sm font-bold shadow-lg shadow-teal-500/20 dark:shadow-teal-500/10">
              <AlertTriangle :size="16" />
              Xử lý xung đột
            </router-link>
            <router-link to="/staff/requests" class="lg-button-secondary h-11 rounded-2xl px-6 text-sm font-bold">
              <FileStack :size="16" />
              Duyệt đơn từ
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="relative h-48 w-48 rounded-[40px] bg-gradient-to-tr from-teal-400/40 to-emerald-400/40 dark:from-teal-500/30 dark:to-emerald-500/30 p-1 rotate-3 shadow-xl border border-white/30 dark:border-white/10 backdrop-blur-2xl">
             <div class="h-full w-full rounded-[38px] bg-white/40 dark:bg-black/40 backdrop-blur-2xl flex items-center justify-center border border-white/50 dark:border-white/10">
               <ShieldCheck :size="80" class="text-teal-600/60 dark:text-teal-400/60" />
             </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
      <div v-for="item in stats" :key="item.id" 
           class="group lg-glass-card-hover p-6">
        <div class="flex items-center justify-between">
          <div :class="['flex h-12 w-12 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isWarning ? 'bg-red-100/80 dark:bg-red-500/20 text-red-600 dark:text-red-400' : 'bg-teal-100/80 dark:bg-teal-500/20 text-teal-600 dark:text-teal-400']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isWarning" :size="12" />
            <AlertCircle v-else :size="12" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-medium text-slate-500 dark:text-slate-400">{{ item.label }}</p>
          <p class="mt-1 text-3xl font-black text-slate-800 dark:text-white">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <!-- Left: Schedule Conflicts & Registrations -->
      <div class="xl:col-span-2 space-y-6">
        
        <!-- Schedule Tasks -->
        <div class="lg-glass-card overflow-hidden">
          <div class="flex items-center justify-between border-b border-white/40 dark:border-white/10 px-8 py-5">
            <div>
              <h2 class="text-xl font-bold text-slate-800 dark:text-white">Thời khóa biểu cần xử lý</h2>
              <p class="text-sm text-slate-400 dark:text-slate-500 mt-0.5">Các vấn đề phát sinh trong việc xếp lịch</p>
            </div>
            <router-link to="/staff/schedule" class="text-sm font-bold text-teal-600 dark:text-teal-400 hover:text-teal-700 dark:hover:text-teal-300">Xem tất cả</router-link>
          </div>
          <div class="p-4 space-y-4">
            <div v-for="item in scheduleTasks" :key="item.id" 
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-4 rounded-3xl p-5 transition-all lg-glass-card-hover">
              <div :class="['flex h-16 w-16 flex-shrink-0 flex-col items-center justify-center rounded-2xl font-bold', item.alert ? 'bg-red-100/80 dark:bg-red-500/20 text-red-500 dark:text-red-400' : 'bg-teal-100/80 dark:bg-teal-500/20 text-teal-600 dark:text-teal-400']">
                 <component :is="item.alert ? AlertCircle : Clock" :size="28" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-bold text-slate-800 dark:text-white truncate">{{ item.title }}</h3>
                  <span v-if="item.alert" class="rounded-full bg-red-100/80 dark:bg-red-500/20 px-3 py-1 text-[11px] font-bold text-red-700 dark:text-red-400 uppercase tracking-wider">Khẩn cấp</span>
                </div>
                <p class="mt-1 text-sm text-slate-500 dark:text-slate-400">{{ item.desc }}</p>
              </div>
              <div class="flex items-center gap-2 mt-2 sm:mt-0">
                <router-link :to="item.link" class="lg-button-primary h-10 rounded-xl px-5 text-xs font-bold shadow-lg">Xử lý ngay</router-link>
              </div>
            </div>
          </div>
        </div>

        <!-- Class Sections Summary -->
        <div class="lg-glass-card overflow-hidden">
          <div class="flex items-center justify-between border-b border-white/40 dark:border-white/10 px-8 py-5">
            <h2 class="text-xl font-bold text-slate-800 dark:text-white">Tình trạng lớp học phần</h2>
            <button class="rounded-xl border border-white/40 dark:border-white/10 p-2 text-slate-400 hover:bg-white/50 dark:hover:bg-white/5 transition-colors"><MoreVertical :size="18" /></button>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-x divide-white/30 dark:divide-white/10">
             <div class="p-8">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="font-bold text-slate-700 dark:text-slate-300">Lớp sắp đầy ( &gt;90% )</h3>
                   <span class="rounded-full bg-amber-100/80 dark:bg-amber-500/20 px-2.5 py-0.5 text-xs font-bold text-amber-700 dark:text-amber-400">8 Lớp</span>
                </div>
                <div class="space-y-4">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-3">
                         <div class="h-2 w-2 rounded-full bg-amber-400"></div>
                         <span class="text-sm text-slate-600 dark:text-slate-400">CS101 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-sm font-bold text-slate-800 dark:text-white">48/50</span>
                   </div>
                </div>
                <button class="mt-6 text-sm font-bold text-teal-600 dark:text-teal-400">Mở thêm sức chứa →</button>
             </div>
             <div class="p-8">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="font-bold text-slate-700 dark:text-slate-300">Waitlist cần duyệt</h3>
                   <span class="rounded-full bg-blue-100/80 dark:bg-blue-500/20 px-2.5 py-0.5 text-xs font-bold text-blue-700 dark:text-blue-400">45 SV</span>
                </div>
                <div class="space-y-4">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-3">
                         <div class="h-2 w-2 rounded-full bg-blue-400"></div>
                         <span class="text-sm text-slate-600 dark:text-slate-400">ENG102 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-sm font-bold text-slate-800 dark:text-white">{{ 10 + i }} SV chờ</span>
                   </div>
                </div>
                <button class="mt-6 text-sm font-bold text-teal-600 dark:text-teal-400">Xử lý danh sách chờ →</button>
             </div>
          </div>
        </div>

      </div>

      <!-- Right: Urgent Requests & Announcements -->
      <div class="space-y-6">
        
        <!-- Urgent Requests -->
        <div class="lg-glass-card p-6">
           <div class="mb-6 flex items-center justify-between">
             <h3 class="text-lg font-bold text-slate-800 dark:text-white">Đơn từ khẩn (Quá SLA)</h3>
             <span class="flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white animate-pulse">3</span>
           </div>
           <div class="space-y-4">
             <div v-for="req in urgentRequests" :key="req.id" 
                  class="flex items-start gap-4 rounded-2xl p-4 transition-all lg-glass-card-hover">
               <div class="mt-1 h-10 w-10 shrink-0 rounded-xl bg-red-100/80 dark:bg-red-500/20 flex items-center justify-center text-red-500 dark:text-red-400 border border-red-200/50 dark:border-red-500/20">
                  <FileStack :size="20" />
               </div>
               <div class="flex-1 min-w-0">
                 <div class="flex justify-between items-start">
                    <p class="text-sm font-bold text-slate-800 dark:text-white leading-tight">{{ req.type }}</p>
                    <span class="text-[10px] font-bold text-red-600 dark:text-red-400 uppercase">{{ req.time }}</span>
                 </div>
                 <p class="mt-1 text-xs text-slate-500 dark:text-slate-400 truncate">{{ req.studentName }} - {{ req.studentId }}</p>
               </div>
             </div>
           </div>
           <button class="mt-6 w-full lg-button-primary h-11 rounded-xl text-xs font-bold">Xử lý toàn bộ đơn</button>
        </div>

        <!-- Strategy Summary / Mini Chart -->
        <div class="lg-glass-card-hover p-6 relative overflow-hidden">
          <div class="pointer-events-none absolute -right-10 -bottom-10 h-32 w-32 rounded-full bg-teal-400/20 dark:bg-teal-500/15 blur-[64px]" />
          <div class="relative">
            <h3 class="text-lg font-bold text-slate-800 dark:text-white">Thống kê Học kỳ</h3>
            <p class="text-sm text-slate-500 dark:text-slate-400 mt-1">Đã hoàn thành xếp lịch cho 85% các khoa.</p>
            
            <div class="mt-6 flex items-end gap-2 h-24">
              <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h" 
                   class="flex-1 bg-teal-400/30 dark:bg-teal-500/25 rounded-t-lg transition-all hover:bg-teal-400/50 dark:hover:bg-teal-500/40 cursor-help" 
                   :style="{ height: h + '%' }" />
            </div>
            
            <div class="mt-6 grid grid-cols-2 gap-4">
               <div class="rounded-2xl bg-white/40 dark:bg-white/5 p-3 backdrop-blur-xl border border-white/50 dark:border-white/10">
                  <p class="text-[10px] uppercase font-bold text-slate-500 dark:text-slate-400 tracking-wider">Lớp học phần</p>
                  <p class="text-xl font-black mt-1 text-slate-800 dark:text-white">1,240</p>
               </div>
               <div class="rounded-2xl bg-white/40 dark:bg-white/5 p-3 backdrop-blur-xl border border-white/50 dark:border-white/10">
                  <p class="text-[10px] uppercase font-bold text-slate-500 dark:text-slate-400 tracking-wider">Phòng trống</p>
                  <p class="text-xl font-black mt-1 text-slate-800 dark:text-white">12%</p>
               </div>
            </div>
          </div>
        </div>

        <!-- Announcements -->
        <div class="lg-glass-card p-6">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-bold text-slate-800 dark:text-white">Thông báo mới</h3>
            <Bell :size="18" class="text-slate-400 dark:text-slate-500" />
          </div>
          <div class="space-y-4">
            <div class="flex gap-3 p-3 rounded-2xl lg-glass-card-hover">
              <div class="h-10 w-10 rounded-full bg-blue-100/80 dark:bg-blue-500/20 flex items-center justify-center text-blue-500 dark:text-blue-400 shrink-0">
                <Bell :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700 dark:text-slate-300">Mở đăng ký kỳ Thu 2026</p>
                <p class="text-xs text-slate-500 dark:text-slate-400 mt-1">Hệ thống đã sẵn sàng cho đợt đăng ký tới.</p>
              </div>
            </div>
          </div>
          <button class="mt-6 w-full lg-button-secondary h-11 rounded-xl text-xs font-bold">Tất cả thông báo</button>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { 
  Users, Layers, FileStack, Calendar, Clock, AlertCircle, 
  ArrowUpRight, ShieldCheck, MoreVertical, Bell, AlertTriangle,
  Sparkles
} from 'lucide-vue-next'

// KPI Stats
const stats = [
  { id: 1, label: 'Lịch học hôm nay', value: '142', trend: '+12', bgColor: 'bg-blue-50', iconColor: 'text-blue-600', icon: Calendar },
  { id: 2, label: 'Xung đột lịch', value: '3', trend: 'Cần xử lý', isWarning: true, bgColor: 'bg-red-50', iconColor: 'text-red-600', icon: AlertTriangle },
  { id: 3, label: 'Lớp đang mở', value: '86', trend: '8 lớp đầy', isWarning: true, bgColor: 'bg-amber-50', iconColor: 'text-amber-600', icon: Layers },
  { id: 4, label: 'Đơn từ chờ duyệt', value: '28', trend: '3 quá hạn', isWarning: true, bgColor: 'bg-teal-50', iconColor: 'text-teal-600', icon: FileStack },
]

const scheduleTasks = [
  { id: 1, title: '2 Lịch trùng phòng học', desc: 'Phòng A201 (Tiết 1-3) đang có 2 lớp xếp chồng.', alert: true, link: '/staff/conflicts' },
  { id: 2, title: 'TKB Khoa CNTT đang chờ duyệt', desc: 'Đã submit ngày hôm qua, chưa có phản hồi từ BGH.', alert: false, link: '/staff/schedule' },
  { id: 3, title: 'Giảng viên nghỉ đột xuất', desc: 'TS. Nguyễn Văn A xin nghỉ chiều nay. Cần dạy thay.', alert: true, link: '/staff/assignments' },
]

const urgentRequests = [
  { id: 101, type: 'Xin chuyển lớp', studentName: 'Trần Bình', studentId: 'SE150212', time: '-2 NGÀY' },
  { id: 102, type: 'Xin thi lại', studentName: 'Lê Hoàng', studentId: 'SS140023', time: '-1 NGÀY' },
  { id: 103, type: 'Xin giấy xác nhận SV', studentName: 'Phạm Thu', studentId: 'SA160199', time: 'TRỄ 4H' },
]
</script>

<style scoped>
.transition-all {
  transition-duration: 300ms;
}
</style>
