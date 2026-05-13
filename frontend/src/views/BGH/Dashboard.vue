<template>
  <div class="space-y-6 pb-10">
    
    <!-- ── Welcome Hero (BGH Style - Indigo/Purple) ── -->
    <div class="relative overflow-hidden rounded-[32px] bg-indigo-900 p-8 text-white shadow-2xl shadow-indigo-200">
      <div class="absolute -right-24 -top-24 h-64 w-64 rounded-full bg-indigo-500/20 blur-3xl" />
      <div class="absolute -bottom-24 -left-24 h-64 w-64 rounded-full bg-purple-500/20 blur-3xl" />
      
      <div class="relative flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-3xl md:text-4xl font-extrabold leading-tight tracking-tight">
            Chào buổi sáng, <span class="text-indigo-200">Nguyễn Văn Hiệu Trưởng!</span>
          </h1>
          <p class="mt-3 text-indigo-100/80 text-lg">
            Học kỳ này ghi nhận GPA trung bình tăng 0.12 điểm và tỷ lệ Pass đạt 92.5%. Bạn có 2 bản thảo TKB đang chờ phê duyệt.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/bgh/schedule/pending" class="rounded-2xl bg-white px-6 py-3 text-sm font-bold text-indigo-900 shadow-lg hover:bg-indigo-50 transition-all active:scale-95">
              Duyệt TKB ngay
            </router-link>
            <router-link to="/bgh/academic/reports" class="rounded-2xl bg-indigo-700/50 backdrop-blur px-6 py-3 text-sm font-bold text-white border border-indigo-400/30 hover:bg-indigo-700 transition-all">
              Báo cáo GPA
            </router-link>
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

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
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
      
      <!-- Left: Academic Performance & Teacher Evaluations -->
      <div class="xl:col-span-2 space-y-6">
        
        <!-- Teacher Evaluations Ranking -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-8 py-5">
            <div>
              <h2 class="text-xl font-bold text-slate-800">Ranking Giảng viên</h2>
              <p class="text-sm text-slate-400 mt-0.5">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-sm font-bold text-indigo-600 hover:text-indigo-700">Tất cả xếp hạng</router-link>
          </div>
          <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
             <div v-for="teacher in topTeachers" :key="teacher.id" 
                  class="group flex items-center gap-4 rounded-3xl border border-slate-50 p-4 transition-all hover:border-indigo-100 hover:bg-indigo-50/20">
               <div class="h-12 w-12 rounded-2xl bg-indigo-100 text-indigo-700 flex items-center justify-center font-bold shadow-sm">{{ teacher.initials }}</div>
               <div class="flex-1 min-w-0">
                 <h3 class="font-bold text-slate-800 truncate">{{ teacher.name }}</h3>
                 <p class="text-xs text-slate-500">{{ teacher.department }}</p>
               </div>
               <div class="text-right">
                 <div class="flex items-center justify-end gap-1 text-sm font-black text-slate-800">
                    <Star class="w-3.5 h-3.5 text-amber-500" fill="currentColor" /> {{ teacher.rating }}
                 </div>
                 <p class="text-[10px] text-slate-400">{{ teacher.reviews }} lượt</p>
               </div>
             </div>
          </div>
        </div>

        <!-- Simulated Trend Chart -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden p-8">
           <div class="flex items-center justify-between mb-8">
              <h2 class="text-xl font-bold text-slate-800">Tỷ lệ Pass / Fail toàn trường</h2>
              <select class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2 text-xs font-bold outline-none">
                 <option>Kỳ Spring 2026</option>
                 <option>Kỳ Fall 2025</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-40 gap-4 border-b border-slate-100 pb-2">
              <div v-for="(h, i) in [45, 65, 80, 55, 90, 75, 88]" :key="i" class="flex-1 flex flex-col items-center gap-2">
                 <div class="w-full bg-indigo-600 rounded-t-xl transition-all hover:bg-indigo-700" :style="{ height: h + '%' }" />
                 <span class="text-[10px] font-bold text-slate-400">{{ ['CNTT', 'KT', 'NN', 'DL', 'TK', 'YT', 'GD'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Pending Approvals & Risk Alerts -->
      <div class="space-y-6">
        
        <!-- Pending TKB Approvals -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
           <div class="mb-6 flex items-center justify-between">
             <h3 class="text-lg font-bold text-slate-800">TKB Chờ Duyệt</h3>
             <span class="rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-bold text-indigo-700">2 Mới</span>
           </div>
           <div class="space-y-4">
             <div v-for="i in 2" :key="i" 
                  class="p-4 rounded-2xl border border-slate-50 bg-slate-50/30 transition-all hover:bg-white hover:shadow-md cursor-pointer">
               <div class="flex justify-between items-start">
                  <p class="text-sm font-bold text-slate-800 leading-tight">Khoa {{ i === 1 ? 'CNTT' : 'Kinh Tế' }} - Spring</p>
                  <span class="text-[10px] font-bold text-indigo-600">NEW</span>
               </div>
               <p class="mt-1 text-xs text-slate-500">{{ i === 1 ? '142' : '86' }} lớp • {{ i === 1 ? '3' : '0' }} xung đột</p>
               <button class="mt-3 w-full text-center text-xs font-bold text-indigo-600">Xem ngay →</button>
             </div>
           </div>
        </div>

        <!-- AI Risk Alerts -->
        <div class="rounded-[28px] border border-slate-100 bg-indigo-900 p-6 text-white overflow-hidden relative">
          <div class="absolute -right-10 -bottom-10 h-32 w-32 rounded-full bg-white/10 blur-2xl" />
          <div class="flex items-center gap-2">
             <h3 class="text-lg font-bold">Cảnh báo rủi ro</h3>
             <Sparkles class="w-4 h-4 text-indigo-300" />
          </div>
          <p class="text-sm text-indigo-100/70 mt-1">AI phát hiện 124 sinh viên có rủi ro rớt môn cao do vắng học.</p>
          
          <div class="mt-6 space-y-4">
             <div v-for="sv in riskStudents" :key="sv.id" class="flex items-center justify-between border-b border-white/10 pb-3">
                <div>
                   <p class="text-xs font-bold">{{ sv.name }}</p>
                   <p class="text-[10px] text-indigo-300">{{ sv.class }}</p>
                </div>
                <span class="text-[10px] font-bold bg-rose-500 px-2 py-0.5 rounded-full">{{ sv.reason }}</span>
             </div>
          </div>
          <button class="mt-6 w-full text-center text-xs font-bold text-indigo-200 hover:text-white">Xem toàn bộ báo cáo rủi ro</button>
        </div>

        <!-- Strategy Announcements -->
        <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm p-6">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-lg font-bold text-slate-800">Thông báo điều hành</h3>
            <Bell :size="18" class="text-slate-400" />
          </div>
          <div class="space-y-4">
            <div class="flex gap-3">
              <div class="h-10 w-10 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-500 shrink-0">
                <ShieldCheck :size="18" />
              </div>
              <div>
                <p class="text-sm font-bold text-slate-700">Audit kết quả đào tạo 2025</p>
                <p class="text-xs text-slate-500 mt-1">Phòng Thanh tra sẽ thực hiện kiểm tra vào tuần tới.</p>
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
  BarChart2, PieChart, Star, AlertTriangle, GraduationCap, 
  TrendingUp, Clock, User, UserMinus, Sparkles, ArrowUpRight, Bell, ShieldCheck
} from 'lucide-vue-next'

// KPI Stats
const stats = [
  { id: 1, label: 'GPA Trung Bình', value: '7.84', trend: '↑ 0.12', isNegative: false, bgColor: 'bg-indigo-50', iconColor: 'text-indigo-600', icon: BarChart2 },
  { id: 2, label: 'Tỷ lệ Pass', value: '92.5%', trend: 'Mục tiêu đạt', isNegative: false, bgColor: 'bg-green-50', iconColor: 'text-green-600', icon: PieChart },
  { id: 3, label: 'Đánh giá GV', value: '4.5/5', trend: '85% phản hồi', isNegative: false, bgColor: 'bg-purple-50', iconColor: 'text-purple-600', icon: Star },
  { id: 4, label: 'SV Nguy cơ rớt', value: '124', trend: 'Cảnh báo AI', isNegative: true, bgColor: 'bg-red-50', iconColor: 'text-red-600', icon: UserMinus },
]

const topTeachers = [
  { id: 1, name: 'TS. Nguyễn Khắc A', initials: 'NA', department: 'Khoa CNTT', rating: '4.9', reviews: 145 },
  { id: 2, name: 'ThS. Trần Thị B', initials: 'TB', department: 'Khoa Kinh Tế', rating: '4.8', reviews: 210 },
]

const riskStudents = [
  { id: 'SE1601', name: 'Lê Hoàng Phát', class: 'SE1601', reason: 'Vắng > 20%' },
  { id: 'SS1402', name: 'Trần Bích Thủy', class: 'SS1402', reason: 'GPA < 4.0' },
]
</script>

<style scoped>
.shadow-indigo-200 {
  shadow-color: rgba(79, 70, 229, 0.2);
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
