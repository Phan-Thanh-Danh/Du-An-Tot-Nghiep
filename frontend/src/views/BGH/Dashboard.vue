<template>
  <div class="space-y-4 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="rounded-2xl surface-card border border-card p-4">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h2 class="text-lg md:text-2xl font-semibold leading-tight tracking-tight text-heading">
            Tổng quan Ban giám hiệu
          </h2>
          <p class="mt-2 text-muted text-sm">
            GPA trung bình tăng 0.12, tỷ lệ Pass đạt 92.5%. Có 2 bản thảo TKB đang chờ phê duyệt.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/bgh/schedule/pending" class="lg-button-primary rounded-lg px-3 py-2 text-xs font-bold transition-all active:scale-95">
              Duyệt TKB ngay
            </router-link>
            <router-link to="/bgh/academic/reports" class="lg-button-secondary rounded-lg px-3 py-2 text-xs font-bold transition-all">
              Báo cáo GPA
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-(--color-info-bg)/70 border border-(--color-info-text)/20">
            <GraduationCap :size="22" class="text-(--color-info-text)/70" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isNegative ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="12" />
            <AlertCircle v-else :size="12" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-medium text-muted">{{ item.label }}</p>
          <p class="mt-1 text-xl font-semibold text-heading">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      
      <!-- Left: Academic Performance & Teacher Evaluations -->
      <div class="xl:col-span-2 space-y-4">
        
        <!-- Teacher Evaluations Ranking -->
        <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden">
          <div class="flex items-center justify-between px-4 py-4">
            <div>
              <h2 class="text-lg font-bold text-heading">Ranking Giảng viên</h2>
              <p class="text-xs text-muted mt-0.5">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-xs font-bold text-link hover:underline">Tất cả xếp hạng</router-link>
          </div>
          <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
             <div v-for="teacher in topTeachers" :key="teacher.id" 
                  class="group flex items-center gap-4 rounded-2xl border border-default p-4 transition-all hover:border-(--border-input-focus)">
               <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-bold shadow-sm">{{ teacher.initials }}</div>
               <div class="flex-1 min-w-0">
                 <h3 class="font-bold text-heading truncate">{{ teacher.name }}</h3>
                 <p class="text-xs text-muted">{{ teacher.department }}</p>
               </div>
               <div class="text-right">
                 <div class="flex items-center justify-end gap-1 text-sm font-semibold text-heading">
                    <Star class="w-3.5 h-3.5 text-(--color-warning-text)" fill="currentColor" /> {{ teacher.rating }}
                 </div>
                 <p class="text-[10px] text-muted">{{ teacher.reviews }} lượt</p>
               </div>
             </div>
          </div>
        </div>

        <!-- Trend Chart -->
        <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden p-4">
           <div class="flex items-center justify-between mb-4">
              <h2 class="text-base font-bold text-heading">Tỷ lệ Pass / Fail</h2>
              <select class="rounded-lg border border-input surface-input px-3 py-1.5 text-[10px] font-bold outline-none">
                 <option>Kỳ Spring 2026</option>
                 <option>Kỳ Fall 2025</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-32 gap-4 pb-2">
              <div v-for="(h, i) in [45, 65, 80, 55, 90, 75, 88]" :key="i" class="flex-1 flex flex-col items-center gap-2">
                 <div class="w-full bg-(--lg-primary) rounded-t-xl transition-all hover:opacity-80" :style="{ height: h + '%' }" />
                 <span class="text-[10px] font-bold text-muted">{{ ['CNTT', 'KT', 'NN', 'DL', 'TK', 'YT', 'GD'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Pending Approvals & Risk Alerts -->
      <div class="space-y-4">
        
        <!-- Pending TKB Approvals -->
        <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
           <div class="mb-3 flex items-center justify-between">
              <h3 class="text-base font-bold text-heading">TKB Chờ Duyệt</h3>
              <span class="rounded-full bg-(--color-info-bg) px-2 py-0.5 text-[10px] font-bold text-(--color-info-text)">2 Mới</span>
           </div>
           <div class="space-y-3">
             <div v-for="i in 2" :key="i" 
                  class="p-3 rounded-xl border border-default surface-solid transition-all hover:bg-(--surface-input) cursor-pointer">
               <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight">Khoa {{ i === 1 ? 'CNTT' : 'Kinh Tế' }} - Spring</p>
                  <span class="text-[9px] font-bold text-link">NEW</span>
               </div>
               <p class="mt-0.5 text-[10px] text-muted">{{ i === 1 ? '142' : '86' }} lớp • {{ i === 1 ? '3' : '0' }} xung đột</p>
               <button class="mt-2 w-full text-center text-[10px] font-bold text-link">Xem ngay →</button>
             </div>
           </div>
        </div>

        <!-- AI Risk Alerts -->
        <div class="rounded-2xl border border-(--color-danger-text)/20 bg-(--color-danger-bg) p-4 overflow-hidden relative">
          <div class="flex items-center gap-2">
             <h3 class="text-base font-bold text-heading">Cảnh báo rủi ro</h3>
          </div>
          <p class="text-xs text-body mt-1">AI phát hiện 124 sinh viên có rủi ro rớt môn cao do vắng học.</p>
          
          <div class="mt-4 space-y-3">
             <div v-for="sv in riskStudents" :key="sv.id" class="flex items-center justify-between border-b border-(--color-danger-text)/20 pb-2">
                <div>
                   <p class="text-xs font-bold text-heading">{{ sv.name }}</p>
                   <p class="text-[9px] text-muted">{{ sv.class }}</p>
                </div>
                <span class="text-[9px] font-bold bg-(--surface-card) text-(--color-danger-text) px-2 py-0.5 rounded-full">{{ sv.reason }}</span>
             </div>
          </div>
          <button class="mt-4 w-full text-center text-[10px] font-bold text-(--color-danger-text) hover:underline">Xem toàn bộ báo cáo rủi ro</button>
        </div>

        <!-- Strategy Announcements -->
        <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="16" class="text-muted" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) shrink-0">
                <ShieldCheck :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-heading">Audit kết quả đào tạo 2025</p>
                <p class="text-[10px] text-muted mt-0.5">Phòng Thanh tra sẽ thực hiện kiểm tra vào tuần tới.</p>
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
  { id: 1, label: 'GPA Trung Bình', value: '7.84', trend: '↑ 0.12', isNegative: false, bgColor: 'bg-(--color-info-bg)', iconColor: 'text-(--color-info-text)', icon: BarChart2 },
  { id: 2, label: 'Tỷ lệ Pass', value: '92.5%', trend: 'Mục tiêu đạt', isNegative: false, bgColor: 'bg-(--color-success-bg)', iconColor: 'text-(--color-success-text)', icon: PieChart },
  { id: 3, label: 'Đánh giá GV', value: '4.5/5', trend: '85% phản hồi', isNegative: false, bgColor: 'bg-(--color-warning-bg)', iconColor: 'text-(--color-warning-text)', icon: Star },
  { id: 4, label: 'SV Nguy cơ rớt', value: '124', trend: 'Cảnh báo AI', isNegative: true, bgColor: 'bg-(--color-danger-bg)', iconColor: 'text-(--color-danger-text)', icon: UserMinus },
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
