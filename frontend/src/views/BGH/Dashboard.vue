<template>
  <div class="space-y-4 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="rounded-[24px] bg-indigo-900 p-5 text-white shadow-lg shadow-indigo-200">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-lg md:text-2xl font-extrabold leading-tight tracking-tight">
            Chào buổi sáng, <span class="text-indigo-200">Nguyễn Văn Hiệu Trưởng!</span>
          </h1>
          <p class="mt-2 text-indigo-100/80 text-sm">
            GPA trung bình tăng 0.12, tỷ lệ Pass đạt 92.5%. Có 2 bản thảo TKB đang chờ phê duyệt.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/bgh/schedule/pending" class="rounded-lg bg-white px-3 py-2 text-xs font-bold text-indigo-900 shadow-lg hover:bg-indigo-50 transition-all active:scale-95">
              Duyệt TKB ngay
            </router-link>
            <router-link to="/bgh/academic/reports" class="rounded-lg bg-indigo-700/50 backdrop-blur px-3 py-2 text-xs font-bold text-white border border-indigo-400/30 hover:bg-indigo-700 transition-all">
              Báo cáo GPA
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-20 w-20 items-center justify-center rounded-2xl bg-white/10 backdrop-blur border border-white/20">
            <GraduationCap :size="32" class="text-white/80" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group relative overflow-hidden rounded-[24px] border border-white bg-white p-4 shadow-sm transition-all hover:shadow-xl hover:-translate-y-1">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
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
          <p class="mt-1 text-xl font-black text-slate-800">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      
      <!-- Left: Academic Performance & Teacher Evaluations -->
      <div class="xl:col-span-2 space-y-4">
        
        <!-- Teacher Evaluations Ranking -->
        <div class="rounded-2xl border border-slate-100 bg-white shadow-sm overflow-hidden">
          <div class="flex items-center justify-between border-b border-slate-100 px-4 py-4">
            <div>
              <h2 class="text-lg font-bold text-slate-800">Ranking Giảng viên</h2>
              <p class="text-xs text-slate-400 mt-0.5">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-xs font-bold text-indigo-600 hover:text-indigo-700">Tất cả xếp hạng</router-link>
          </div>
          <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
             <div v-for="teacher in topTeachers" :key="teacher.id" 
                  class="group flex items-center gap-4 rounded-3xl border border-slate-50 p-4 transition-all hover:border-indigo-100 hover:bg-indigo-50/20">
               <div class="h-10 w-10 rounded-2xl bg-indigo-100 text-indigo-700 flex items-center justify-center font-bold shadow-sm">{{ teacher.initials }}</div>
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

        <!-- Trend Chart -->
        <div class="rounded-2xl border border-slate-100 bg-white shadow-sm overflow-hidden p-4">
           <div class="flex items-center justify-between mb-4">
              <h2 class="text-base font-bold text-slate-800">Tỷ lệ Pass / Fail</h2>
              <select class="rounded-lg border border-slate-100 bg-slate-50 px-3 py-1.5 text-[10px] font-bold outline-none">
                 <option>Kỳ Spring 2026</option>
                 <option>Kỳ Fall 2025</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-32 gap-4 border-b border-slate-100 pb-2">
              <div v-for="(h, i) in [45, 65, 80, 55, 90, 75, 88]" :key="i" class="flex-1 flex flex-col items-center gap-2">
                 <div class="w-full bg-indigo-600 rounded-t-xl transition-all hover:bg-indigo-700" :style="{ height: h + '%' }" />
                 <span class="text-[10px] font-bold text-slate-400">{{ ['CNTT', 'KT', 'NN', 'DL', 'TK', 'YT', 'GD'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Pending Approvals & Risk Alerts -->
      <div class="space-y-4">
        
        <!-- Pending TKB Approvals -->
        <div class="rounded-2xl border border-slate-100 bg-white shadow-sm p-4">
           <div class="mb-3 flex items-center justify-between">
              <h3 class="text-base font-bold text-slate-800">TKB Chờ Duyệt</h3>
              <span class="rounded-full bg-indigo-100 px-2 py-0.5 text-[10px] font-bold text-indigo-700">2 Mới</span>
           </div>
           <div class="space-y-3">
             <div v-for="i in 2" :key="i" 
                  class="p-3 rounded-xl border border-slate-50 bg-slate-50/30 transition-all hover:bg-white hover:shadow-md cursor-pointer">
               <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-slate-800 leading-tight">Khoa {{ i === 1 ? 'CNTT' : 'Kinh Tế' }} - Spring</p>
                  <span class="text-[9px] font-bold text-indigo-600">NEW</span>
               </div>
               <p class="mt-0.5 text-[10px] text-slate-500">{{ i === 1 ? '142' : '86' }} lớp • {{ i === 1 ? '3' : '0' }} xung đột</p>
               <button class="mt-2 w-full text-center text-[10px] font-bold text-indigo-600">Xem ngay →</button>
             </div>
           </div>
        </div>

        <!-- AI Risk Alerts -->
        <div class="rounded-2xl border border-slate-100 bg-indigo-900 p-4 text-white overflow-hidden relative">
          <div class="flex items-center gap-2">
             <h3 class="text-base font-bold">Cảnh báo rủi ro</h3>
          </div>
          <p class="text-xs text-indigo-100/70 mt-1">AI phát hiện 124 sinh viên có rủi ro rớt môn cao do vắng học.</p>
          
          <div class="mt-4 space-y-3">
             <div v-for="sv in riskStudents" :key="sv.id" class="flex items-center justify-between border-b border-white/10 pb-2">
                <div>
                   <p class="text-xs font-bold">{{ sv.name }}</p>
                   <p class="text-[9px] text-indigo-300">{{ sv.class }}</p>
                </div>
                <span class="text-[9px] font-bold bg-rose-500 px-2 py-0.5 rounded-full">{{ sv.reason }}</span>
             </div>
          </div>
          <button class="mt-4 w-full text-center text-[10px] font-bold text-indigo-200 hover:text-white">Xem toàn bộ báo cáo rủi ro</button>
        </div>

        <!-- Strategy Announcements -->
        <div class="rounded-2xl border border-slate-100 bg-white shadow-sm p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-base font-bold text-slate-800">Thông báo</h3>
            <Bell :size="16" class="text-slate-400" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-500 shrink-0">
                <ShieldCheck :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-slate-700">Audit kết quả đào tạo 2025</p>
                <p class="text-[10px] text-slate-500 mt-0.5">Phòng Thanh tra sẽ thực hiện kiểm tra vào tuần tới.</p>
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
