<template>
  <div class="space-y-4 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="lg-glass-strong rounded-[24px] p-6 lg:p-8">
      <div class="flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-2xl md:text-3xl font-bold tracking-tight text-heading">
            Tổng quan giáo vụ
          </h1>
          <p class="mt-2 text-sm text-muted">
            Hệ thống ghi nhận <strong class="text-[var(--color-danger-text)]">3 lịch học xung đột</strong> và 28 đơn từ đang chờ xử lý.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/staff/conflicts" class="lg-btn-danger px-5 py-2.5 text-sm font-bold transition-all hover:-translate-y-0.5">
              <AlertTriangle :size="16" />
              Xử lý xung đột
            </router-link>
            <router-link to="/staff/requests" class="lg-btn-secondary px-5 py-2.5 text-sm font-bold transition-all hover:-translate-y-0.5">
              <FileStack :size="16" />
              Duyệt đơn từ
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block relative group">
          <div class="absolute -inset-1 rounded-3xl bg-[var(--color-info-text)] opacity-10 blur-xl group-hover:opacity-20 transition-opacity"></div>
          <div class="relative flex h-24 w-24 items-center justify-center rounded-[24px] lg-glass border border-[var(--color-info-text)]/20 shadow-xl">
            <ShieldCheck :size="42" class="text-[var(--color-info-text)]" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group lg-glass-soft rounded-[20px] p-5 transition-all duration-300 hover:shadow-[var(--lg-shadow-lg)] hover:-translate-y-1">
        <div class="flex items-center justify-between">
          <div :class="['flex h-12 w-12 items-center justify-center rounded-xl transition-transform group-hover:scale-110 shadow-sm border border-white/20 dark:border-white/5', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold shadow-sm', item.isWarning ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isWarning" :size="12" stroke-width="2.5" />
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
      
      <!-- Left: Schedule Conflicts & Registrations -->
      <div class="xl:col-span-2 space-y-5">
        
        <!-- Schedule Tasks -->
        <div class="lg-glass rounded-[24px] overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-5 py-4 bg-[var(--surface-table-header)]/40">
            <div>
              <h2 class="text-base font-bold text-heading">Thời khóa biểu cần xử lý</h2>
              <p class="text-xs text-muted mt-0.5">Các vấn đề phát sinh trong việc xếp lịch</p>
            </div>
            <router-link to="/staff/schedule" class="text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Xem tất cả</router-link>
          </div>
          <div class="p-4 space-y-3">
            <div v-for="item in scheduleTasks" :key="item.id" 
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-4 rounded-[16px] p-4 transition-all lg-solid-soft hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5">
              <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-[12px] font-bold shadow-inner', item.alert ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]']">
                 <component :is="item.alert ? AlertCircle : Clock" :size="20" stroke-width="2.5" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2">
                  <h3 class="text-sm font-bold text-heading truncate">{{ item.title }}</h3>
                  <span v-if="item.alert" class="rounded-full bg-[var(--color-danger-bg)]/80 px-2.5 py-0.5 text-[10px] font-bold text-[var(--color-danger-text)] uppercase tracking-wider">Khẩn cấp</span>
                </div>
                <p class="mt-1 text-xs text-muted leading-relaxed">{{ item.desc }}</p>
              </div>
              <div class="flex items-center gap-2 mt-3 sm:mt-0">
                <router-link :to="item.link" class="lg-button-primary h-9 rounded-[10px] px-4 text-[11px] font-bold shadow-[var(--lg-shadow-sm)]">Xử lý ngay</router-link>
              </div>
            </div>
          </div>
        </div>

        <!-- Class Sections Summary -->
        <div class="lg-glass rounded-[24px] overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-5 py-4 bg-[var(--surface-table-header)]/40">
            <h2 class="text-base font-bold text-heading">Tình trạng lớp học phần</h2>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-y md:divide-y-0 md:divide-x divide-card">
             <div class="p-5">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="text-xs font-bold text-heading">Lớp sắp đầy (&gt;90%)</h3>
                   <span class="rounded-full bg-[var(--color-warning-bg)] px-2.5 py-1 text-[10px] font-bold text-[var(--color-warning-text)] shadow-sm">8 Lớp</span>
                </div>
                <div class="space-y-3">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-2.5">
                         <div class="h-2 w-2 rounded-full bg-[var(--color-warning-text)] shadow-[0_0_8px_var(--color-warning-text)]"></div>
                         <span class="text-xs text-body font-medium">CS101 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-xs font-bold text-heading tabular-nums">48/50</span>
                   </div>
                </div>
                <button class="mt-5 flex items-center text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Mở thêm sức chứa <ArrowUpRight :size="14" class="ml-1"/></button>
             </div>
             <div class="p-5">
                <div class="flex items-center justify-between mb-4">
                   <h3 class="text-xs font-bold text-heading">Waitlist cần duyệt</h3>
                   <span class="rounded-full bg-[var(--color-info-bg)] px-2.5 py-1 text-[10px] font-bold text-[var(--color-info-text)] shadow-sm">45 SV</span>
                </div>
                <div class="space-y-3">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-2.5">
                         <div class="h-2 w-2 rounded-full bg-[var(--color-info-text)] shadow-[0_0_8px_var(--color-info-text)]"></div>
                         <span class="text-xs text-body font-medium">ENG102 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-xs font-bold text-heading tabular-nums">{{ 10 + i }} SV chờ</span>
                   </div>
                </div>
                <button class="mt-5 flex items-center text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Xử lý danh sách chờ <ArrowUpRight :size="14" class="ml-1"/></button>
             </div>
          </div>
        </div>

      </div>

      <!-- Right sidebar -->
      <div class="space-y-5">
        
        <!-- Urgent Requests -->
        <div class="lg-glass rounded-[24px] p-5">
           <div class="mb-4 flex items-center justify-between">
             <h3 class="text-base font-bold text-heading">Đơn từ khẩn</h3>
             <span class="flex h-6 w-6 items-center justify-center rounded-full bg-[var(--color-danger-bg)] text-[11px] font-bold text-[var(--color-danger-text)] shadow-sm">3</span>
           </div>
           <div class="space-y-3">
             <div v-for="req in urgentRequests" :key="req.id" 
                  class="group flex items-start gap-3 rounded-[16px] p-3 transition-all lg-solid-soft hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 cursor-pointer">
               <div class="h-9 w-9 shrink-0 rounded-xl bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)]">
                  <FileStack :size="18" stroke-width="2.5" />
               </div>
               <div class="flex-1 min-w-0 pt-0.5">
                 <div class="flex justify-between items-start">
                    <p class="text-xs font-bold text-heading leading-tight truncate pr-2 group-hover:text-link transition-colors">{{ req.type }}</p>
                    <span class="text-[9px] font-bold text-[var(--color-danger-text)] uppercase whitespace-nowrap">{{ req.time }}</span>
                 </div>
                 <p class="mt-1 text-[10px] font-medium text-muted truncate">{{ req.studentName }}</p>
               </div>
             </div>
           </div>
           <button class="mt-4 w-full lg-btn-primary h-10 rounded-[12px] text-xs font-bold shadow-[var(--lg-shadow-md)]">Xử lý toàn bộ đơn</button>
        </div>

        <!-- Strategy Summary / Mini Chart -->
        <div class="lg-glass-soft rounded-[24px] p-5">
            <h3 class="text-base font-bold text-heading">Thống kê Học kỳ</h3>
            <p class="text-xs text-muted mt-1 leading-relaxed">Đã hoàn thành xếp lịch cho 85% các khoa.</p>
            
            <div class="mt-5 flex items-end gap-2 h-24">
              <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h" 
                   class="flex-1 bg-gradient-to-t from-[var(--color-info-text)]/60 to-[var(--color-info-text)] rounded-t-lg transition-all hover:opacity-80 hover:scale-y-105 cursor-help"
                   :style="{ height: h + '%' }" />
            </div>
            
            <div class="mt-5 grid grid-cols-2 gap-3">
               <div class="rounded-[16px] lg-solid-soft p-3 text-center">
                  <p class="text-[9px] uppercase font-bold text-muted tracking-wider">Lớp học phần</p>
                  <p class="text-lg font-bold mt-1 text-heading">1,240</p>
               </div>
               <div class="rounded-[16px] lg-solid-soft p-3 text-center">
                  <p class="text-[9px] uppercase font-bold text-muted tracking-wider">Phòng trống</p>
                  <p class="text-lg font-bold mt-1 text-heading">12%</p>
               </div>
            </div>
        </div>

        <!-- Announcements -->
        <div class="lg-glass-soft rounded-[24px] p-5">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="16" class="text-muted" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-3 p-3 rounded-[16px] lg-solid-soft transition-colors hover:bg-[var(--surface-card-hover)] cursor-pointer">
              <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
                <Bell :size="18" stroke-width="2.5" />
              </div>
              <div>
                <p class="text-xs font-bold text-heading">Mở đăng ký kỳ Thu 2026</p>
                <p class="text-[11px] font-medium text-muted mt-1">Hệ thống đã sẵn sàng cho đợt đăng ký tới.</p>
              </div>
            </div>
          </div>
          <button class="mt-4 w-full lg-btn-subtle h-10 rounded-[12px] text-xs font-bold">Tất cả thông báo</button>
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
  { id: 1, label: 'Lịch học hôm nay', value: '142', trend: '+12', bgColor: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]', icon: Calendar },
  { id: 2, label: 'Xung đột lịch', value: '3', trend: 'Cần xử lý', isWarning: true, bgColor: 'bg-[var(--color-danger-bg)]', iconColor: 'text-[var(--color-danger-text)]', icon: AlertTriangle },
  { id: 3, label: 'Lớp đang mở', value: '86', trend: '8 lớp đầy', isWarning: true, bgColor: 'bg-[var(--color-warning-bg)]', iconColor: 'text-[var(--color-warning-text)]', icon: Layers },
  { id: 4, label: 'Đơn từ chờ duyệt', value: '28', trend: '3 quá hạn', isWarning: true, bgColor: 'bg-[var(--color-info-bg)]', iconColor: 'text-[var(--color-info-text)]', icon: FileStack },
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
