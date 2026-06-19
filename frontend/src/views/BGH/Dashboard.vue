<template>
  <div class="space-y-4 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="lg-glass-strong rounded-[24px] p-6 lg:p-8">
      <div class="flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-2xl md:text-3xl font-bold tracking-tight text-heading">
            Tổng quan Ban giám hiệu
          </h1>
          <p class="mt-2 text-muted text-sm leading-relaxed">
            GPA trung bình <strong class="text-[var(--color-success-text)]">tăng 0.12</strong>, tỷ lệ Pass đạt <strong class="text-[var(--color-success-text)]">92.5%</strong>. Có 2 bản thảo TKB đang chờ phê duyệt.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/bgh/schedule/pending" class="lg-btn-primary px-5 py-2.5 text-sm font-bold shadow-[var(--lg-shadow-md)] hover:-translate-y-0.5 transition-all">
              Duyệt TKB ngay
            </router-link>
            <router-link to="/bgh/academic/reports" class="lg-btn-secondary px-5 py-2.5 text-sm font-bold hover:-translate-y-0.5 transition-all">
              Báo cáo GPA
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block relative group">
          <div class="absolute -inset-1 rounded-3xl bg-[var(--color-info-text)] opacity-10 blur-xl group-hover:opacity-20 transition-opacity"></div>
          <div class="relative flex h-24 w-24 items-center justify-center rounded-[24px] lg-glass border border-[var(--color-info-text)]/20 shadow-xl">
            <GraduationCap :size="42" class="text-[var(--color-info-text)]" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group lg-glass-soft rounded-[20px] p-5 transition-all duration-300 hover:shadow-[var(--lg-shadow-lg)] hover:-translate-y-1">
        <div class="flex items-center justify-between">
          <div :class="['flex h-12 w-12 items-center justify-center rounded-xl transition-transform group-hover:scale-110 shadow-sm border border-white/20 dark:border-white/5', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold shadow-sm', item.isNegative ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="12" stroke-width="2.5" />
            <AlertCircle v-else :size="12" stroke-width="2.5" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-semibold text-muted tracking-tight">{{ item.label }}</p>
          <p class="mt-1 text-2xl font-bold text-heading tracking-tight">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-5">
      
      <!-- Left: Academic Performance & Teacher Evaluations -->
      <div class="xl:col-span-2 space-y-5">
        
        <!-- Teacher Evaluations Ranking -->
        <div class="lg-glass rounded-[24px] overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-5 py-4 bg-[var(--surface-table-header)]/40">
            <div>
              <h2 class="text-base font-bold text-heading">Ranking Giảng viên</h2>
              <p class="text-xs text-muted mt-0.5">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Tất cả xếp hạng</router-link>
          </div>
          <div class="p-5 grid grid-cols-1 md:grid-cols-2 gap-4">
             <div v-for="teacher in topTeachers" :key="teacher.id" 
                  class="group flex items-center gap-4 rounded-[16px] lg-solid-soft p-4 transition-all hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 border border-default cursor-pointer">
               <div class="h-11 w-11 rounded-[12px] bg-[var(--color-info-bg)] text-[var(--color-info-text)] flex items-center justify-center font-bold shadow-inner text-sm">{{ teacher.initials }}</div>
               <div class="flex-1 min-w-0">
                 <h3 class="font-bold text-heading truncate group-hover:text-link transition-colors">{{ teacher.name }}</h3>
                 <p class="text-xs text-muted mt-0.5">{{ teacher.department }}</p>
               </div>
               <div class="text-right">
                 <div class="flex items-center justify-end gap-1.5 text-sm font-bold text-heading">
                    <Star class="w-4 h-4 text-[var(--color-warning-text)]" fill="currentColor" /> {{ teacher.rating }}
                 </div>
                 <p class="text-[10px] text-muted mt-0.5">{{ teacher.reviews }} lượt</p>
               </div>
             </div>
          </div>
        </div>

        <!-- Trend Chart -->
        <div class="lg-glass rounded-[24px] p-5">
           <div class="flex items-center justify-between mb-5">
              <h2 class="text-base font-bold text-heading">Tỷ lệ Pass / Fail</h2>
              <select class="lg-input rounded-xl px-3 py-1.5 text-[11px] font-bold w-40">
                 <option>Kỳ Spring 2026</option>
                 <option>Kỳ Fall 2025</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-36 gap-3 border-b border-card pb-3">
              <div v-for="(h, i) in [45, 65, 80, 55, 90, 75, 88]" :key="i" class="flex-1 flex flex-col items-center gap-2 group">
                 <div class="w-full bg-gradient-to-t from-[var(--lg-primary-dark)] to-[var(--lg-primary)] rounded-t-xl transition-all group-hover:opacity-80 group-hover:scale-y-105 cursor-help" :style="{ height: h + '%' }" />
                 <span class="text-[11px] font-bold text-muted group-hover:text-heading transition-colors">{{ ['CNTT', 'KT', 'NN', 'DL', 'TK', 'YT', 'GD'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Pending Approvals & Risk Alerts -->
      <div class="space-y-5">
        
        <!-- Pending TKB Approvals -->
        <div class="lg-glass rounded-[24px] p-5">
           <div class="mb-4 flex items-center justify-between">
              <h3 class="text-base font-bold text-heading">TKB Chờ Duyệt</h3>
              <span class="rounded-full bg-[var(--color-info-bg)] px-2.5 py-1 text-[10px] font-bold text-[var(--color-info-text)] shadow-sm">2 Mới</span>
           </div>
           <div class="space-y-3">
             <div v-for="i in 2" :key="i" 
                  class="group p-4 rounded-[16px] lg-solid-soft transition-all hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 cursor-pointer border border-default">
               <div class="flex justify-between items-start">
                  <p class="text-sm font-bold text-heading leading-tight group-hover:text-link transition-colors">Khoa {{ i === 1 ? 'CNTT' : 'Kinh Tế' }} - Spring</p>
                  <span class="text-[9px] font-bold text-link">NEW</span>
               </div>
               <p class="mt-1.5 text-[11px] font-medium text-muted">{{ i === 1 ? '142' : '86' }} lớp • {{ i === 1 ? '3' : '0' }} xung đột</p>
               <button class="mt-3 w-full lg-btn-subtle h-9 rounded-[10px] text-[11px] font-bold">Xem chi tiết</button>
             </div>
           </div>
        </div>

        <!-- AI Risk Alerts -->
        <div class="rounded-[24px] border border-[var(--color-danger-text)]/20 bg-[var(--color-danger-bg)]/50 p-5 overflow-hidden relative backdrop-blur-card shadow-[var(--lg-shadow-md)]">
          <div class="flex items-center gap-2">
             <h3 class="text-base font-bold text-[var(--color-danger-text)]">Cảnh báo rủi ro</h3>
          </div>
          <p class="text-xs text-body mt-1.5 font-medium leading-relaxed">AI phát hiện 124 sinh viên có rủi ro rớt môn cao do vắng học.</p>
          
          <div class="mt-5 space-y-3">
             <div v-for="sv in riskStudents" :key="sv.id" class="flex items-center justify-between border-b border-[var(--color-danger-text)]/10 pb-3 last:border-0 last:pb-0">
                <div>
                   <p class="text-[13px] font-bold text-heading">{{ sv.name }}</p>
                   <p class="text-[10px] font-medium text-muted mt-0.5">{{ sv.class }}</p>
                </div>
                <span class="text-[10px] font-bold bg-white dark:bg-slate-900 shadow-sm text-[var(--color-danger-text)] px-2.5 py-1 rounded-full border border-[var(--color-danger-text)]/20">{{ sv.reason }}</span>
             </div>
          </div>
          <button class="mt-4 w-full text-center text-[11px] font-bold text-[var(--color-danger-text)] hover:underline decoration-2 underline-offset-2">Xem toàn bộ báo cáo rủi ro</button>
        </div>

        <!-- Strategy Announcements -->
        <div class="lg-glass-soft rounded-[24px] p-5">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="16" class="text-muted" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-3 p-3 rounded-[16px] lg-solid-soft transition-colors hover:bg-[var(--surface-card-hover)] cursor-pointer">
              <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0 shadow-sm border border-[var(--color-info-text)]/10">
                <ShieldCheck :size="16" stroke-width="2.5" />
              </div>
              <div>
                <p class="text-[13px] font-bold text-heading leading-tight">Audit kết quả đào tạo 2025</p>
                <p class="text-[11px] font-medium text-muted mt-1 leading-relaxed">Phòng Thanh tra sẽ thực hiện kiểm tra vào tuần tới.</p>
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
  { id: 1, label: 'GPA Trung Bình', value: '7.84', trend: '↑ 0.12', isNegative: false, bgColor: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]', icon: BarChart2 },
  { id: 2, label: 'Tỷ lệ Pass', value: '92.5%', trend: 'Mục tiêu đạt', isNegative: false, bgColor: 'bg-[var(--color-success-bg)]', iconColor: 'text-[var(--color-success-text)]', icon: PieChart },
  { id: 3, label: 'Đánh giá GV', value: '4.5/5', trend: '85% phản hồi', isNegative: false, bgColor: 'bg-[var(--color-warning-bg)]', iconColor: 'text-[var(--color-warning-text)]', icon: Star },
  { id: 4, label: 'SV Nguy cơ rớt', value: '124', trend: 'Cảnh báo AI', isNegative: true, bgColor: 'bg-[var(--color-danger-bg)]', iconColor: 'text-[var(--color-danger-text)]', icon: UserMinus },
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
