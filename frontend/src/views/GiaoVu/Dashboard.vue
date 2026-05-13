<template>
  <div class="space-y-6 pb-10">
    
    <!-- ── Welcome Hero (Staff Style - Teal/Emerald) ── -->
    <div class="relative overflow-hidden rounded-[32px] bg-teal-900 p-8 text-white shadow-2xl shadow-teal-200">
      <div class="absolute -right-24 -top-24 h-64 w-64 rounded-full bg-teal-500/20 blur-3xl" />
      <div class="absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-emerald-500/20 blur-3xl" />
      
      <div class="relative flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-3xl md:text-4xl font-extrabold leading-tight tracking-tight">
            Chào buổi sáng, <span class="text-teal-200">Trần Thị Giáo Vụ!</span>
          </h1>
          <p class="mt-3 text-teal-100/80 text-lg">
            Hệ thống ghi nhận 3 lịch học xung đột và 28 đơn từ đang chờ xử lý. Hãy giải quyết chúng để đảm bảo tiến độ học vụ nhé!
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/staff/conflicts" class="rounded-2xl bg-white px-6 py-3 text-sm font-bold text-teal-900 shadow-lg hover:bg-teal-50 transition-all active:scale-95">
              Xử lý xung đột
            </router-link>
            <router-link to="/staff/requests" class="rounded-2xl bg-teal-700/50 backdrop-blur px-6 py-3 text-sm font-bold text-white border border-teal-400/30 hover:bg-teal-700 transition-all">
              Duyệt đơn từ
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="relative h-48 w-48 rounded-[40px] bg-gradient-to-tr from-teal-400 to-emerald-400 p-1 rotate-3 shadow-xl">
             <div class="h-full w-full rounded-[38px] bg-teal-900/40 backdrop-blur-sm flex items-center justify-center border border-white/20">
               <ShieldCheck :size="80" class="text-white/80" />
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
          <div :class="['flex h-12 w-12 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isWarning ? 'bg-red-50 text-red-600' : 'bg-teal-50 text-teal-600']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isWarning" :size="12" />
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
      
      <!-- Left: Schedule Conflicts & Registrations -->
      <div class="xl:col-span-2 space-y-6">
        
        <!-- Schedule Tasks -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <div>
              <h2 class="text-xl font-bold text-slate-800">Thời khóa biểu cần xử lý</h2>
              <p class="text-sm text-slate-400 mt-0.5">Các vấn đề phát sinh trong việc xếp lịch</p>
            </div>
            <router-link to="/staff/schedule" class="text-sm font-bold text-teal-600 hover:text-teal-700">Xem tất cả</router-link>
          </div>
          <div class="p-4 space-y-4">
            <div v-for="item in scheduleTasks" :key="item.id" 
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-4 rounded-3xl border border-slate-50 p-5 transition-all hover:border-teal-100 hover:bg-teal-50/20">
              <div :class="['flex h-16 w-16 flex-shrink-0 flex-col items-center justify-center rounded-2xl font-bold', item.alert ? 'bg-red-50 text-red-500' : 'bg-teal-50 text-teal-600']">
                 <component :is="item.alert ? AlertCircle : Clock" :size="28" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-3">
                  <h3 class="text-lg font-bold text-slate-800 truncate">{{ item.title }}</h3>
                  <span v-if="item.alert" class="rounded-full bg-red-100 px-3 py-1 text-[11px] font-bold text-red-700 uppercase tracking-wider">Khẩn cấp</span>
                </div>
                <p class="mt-1 text-sm text-slate-500">{{ item.desc }}</p>
              </div>
              <div class="flex items-center gap-2 mt-2 sm:mt-0">
                <router-link :to="item.link" class="rounded-xl bg-teal-600 px-5 py-2.5 text-xs font-bold text-white hover:bg-teal-700 shadow-md transition-all active:scale-95">Xử lý ngay</router-link>
              </div>
            </div>
          </div>
        </div>

        <!-- Class Sections Summary -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <h2 class="text-xl font-bold text-slate-800">Tình trạng lớp học phần</h2>
            <button class="rounded-xl border border-slate-100 p-2 text-slate-400 hover:bg-slate-50 transition-colors"><MoreVertical :size="18" /></button>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-x divide-slate-100">
             <div class="p-8">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="font-bold text-slate-700">Lớp sắp đầy ( >90% )</h3>
                   <span class="rounded-full bg-amber-100 px-2.5 py-0.5 text-xs font-bold text-amber-700">8 Lớp</span>
                </div>
                <div class="space-y-4">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-3">
                         <div class="h-2 w-2 rounded-full bg-amber-400"></div>
                         <span class="text-sm text-slate-600">CS101 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-sm font-bold text-slate-800">48/50</span>
                   </div>
                </div>
                <button class="mt-6 text-sm font-bold text-teal-600">Mở thêm sức chứa →</button>
             </div>
             <div class="p-8">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="font-bold text-slate-700">Waitlist cần duyệt</h3>
                   <span class="rounded-full bg-blue-100 px-2.5 py-0.5 text-xs font-bold text-blue-700">45 SV</span>
                </div>
                <div class="space-y-4">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-3">
                         <div class="h-2 w-2 rounded-full bg-blue-400"></div>
                         <span class="text-sm text-slate-600">ENG102 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-sm font-bold text-slate-800">{{ 10 + i }} SV chờ</span>
                   </div>
                </div>
                <button class="mt-6 text-sm font-bold text-teal-600">Xử lý danh sách chờ →</button>
             </div>
          </div>
        </div>

      </div>

      <!-- Right: Urgent Requests & Announcements -->
      <div class="space-y-6">
        
        <!-- Urgent Requests -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
           <div class="mb-6 flex items-center justify-between">
             <h3 class="text-lg font-bold text-slate-800">Đơn từ khẩn (Quá SLA)</h3>
             <span class="flex h-6 w-6 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white animate-pulse">3</span>
           </div>
           <div class="space-y-4">
             <div v-for="req in urgentRequests" :key="req.id" 
                  class="flex items-start gap-4 rounded-2xl border border-slate-50 bg-slate-50/30 p-4 transition-all hover:bg-white hover:shadow-md group">
               <div class="mt-1 h-10 w-10 shrink-0 rounded-xl bg-red-50 flex items-center justify-center text-red-500 border border-red-100">
                  <FileStack :size="20" />
               </div>
               <div class="flex-1 min-w-0">
                 <div class="flex justify-between items-start">
                    <p class="text-sm font-bold text-slate-800 leading-tight">{{ req.type }}</p>
                    <span class="text-[10px] font-bold text-red-600 uppercase">{{ req.time }}</span>
                 </div>
                 <p class="mt-1 text-xs text-slate-500 truncate">{{ req.studentName }} - {{ req.studentId }}</p>
               </div>
             </div>
           </div>
           <button class="mt-6 w-full rounded-xl bg-teal-600 py-3 text-xs font-bold text-white hover:bg-teal-700 shadow-md transition-all">Xử lý toàn bộ đơn</button>
        </div>

        <!-- Strategy Summary / Mini Chart -->
        <div class="rounded-[28px] border border-slate-100 bg-teal-600 p-6 text-white overflow-hidden relative">
          <div class="absolute -right-10 -bottom-10 h-32 w-32 rounded-full bg-white/10 blur-2xl" />
          <h3 class="text-lg font-bold">Thống kê Học kỳ</h3>
          <p class="text-sm text-teal-100/70 mt-1">Đã hoàn thành xếp lịch cho 85% các khoa.</p>
          
          <div class="mt-6 flex items-end gap-2 h-24">
            <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h" 
                 class="flex-1 bg-white/20 rounded-t-lg transition-all hover:bg-white/40 cursor-help" 
                 :style="{ height: h + '%' }" />
          </div>
          
          <div class="mt-6 grid grid-cols-2 gap-4">
             <div class="rounded-2xl bg-white/10 p-3 backdrop-blur-sm border border-white/10">
                <p class="text-[10px] uppercase font-bold text-teal-200 tracking-wider">Lớp học phần</p>
                <p class="text-xl font-black mt-1">1,240</p>
             </div>
             <div class="rounded-2xl bg-white/10 p-3 backdrop-blur-sm border border-white/10">
                <p class="text-[10px] uppercase font-bold text-teal-200 tracking-wider">Phòng trống</p>
                <p class="text-xl font-black mt-1">12%</p>
             </div>
          </div>
        </div>

        <!-- Announcements -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-bold text-slate-800">Thông báo mới</h3>
            <Bell :size="18" class="text-slate-400" />
          </div>
          <div class="space-y-4">
            <div class="flex gap-3">
              <div class="h-10 w-10 rounded-full bg-blue-50 flex items-center justify-center text-blue-500 shrink-0">
                <Bell :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700">Mở đăng ký kỳ Thu 2026</p>
                <p class="text-xs text-slate-500 mt-1">Hệ thống đã sẵn sàng cho đợt đăng ký tới.</p>
              </div>
            </div>
          </div>
          <button class="mt-6 w-full rounded-xl bg-slate-50 py-3 text-xs font-bold text-slate-500 hover:bg-slate-100 transition-colors">Tất cả thông báo</button>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { 
  Users, Layers, FileStack, Calendar, Clock, AlertCircle, 
  ArrowUpRight, ShieldCheck, MoreVertical, Bell, AlertTriangle 
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
.shadow-teal-200 {
  shadow-color: rgba(20, 184, 166, 0.2);
}
.transition-all {
  transition-duration: 300ms;
}
@keyframes pulse-soft {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(1.2); opacity: 0.3; }
}
.animate-pulse {
  animation: pulse-soft 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
