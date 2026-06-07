<template>
  <div class="space-y-4 pb-10">
    
    <!-- ── Welcome Hero ── -->
    <div class="rounded-2xl surface-card border border-card p-5">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-lg md:text-2xl font-extrabold leading-tight tracking-tight text-heading">
            Tổng quan giáo vụ
          </h1>
          <p class="mt-2 text-sm text-muted">
            Hệ thống ghi nhận 3 lịch học xung đột và 28 đơn từ đang chờ xử lý.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/staff/conflicts" class="lg-button-primary h-9 rounded-xl px-3 text-xs font-bold">
              <AlertTriangle :size="14" />
              Xử lý xung đột
            </router-link>
            <router-link to="/staff/requests" class="lg-button-secondary h-9 rounded-xl px-3 text-xs font-bold">
              <FileStack :size="14" />
              Duyệt đơn từ
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-[var(--color-info-bg)] border border-[var(--color-info-text)]/20">
            <ShieldCheck :size="30" class="text-[var(--color-info-text)]" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group surface-card border border-card rounded-2xl p-4">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-colors', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isWarning ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isWarning" :size="12" />
            <AlertCircle v-else :size="12" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-medium text-muted">{{ item.label }}</p>
          <p class="mt-1 text-xl font-black text-heading">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      
      <!-- Left: Schedule Conflicts & Registrations -->
      <div class="xl:col-span-2 space-y-4">
        
        <!-- Schedule Tasks -->
        <div class="surface-card border border-card rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-default px-4 py-4">
            <div>
              <h2 class="text-lg font-bold text-heading">Thời khóa biểu cần xử lý</h2>
              <p class="text-xs text-muted mt-0.5">Các vấn đề phát sinh trong việc xếp lịch</p>
            </div>
            <router-link to="/staff/schedule" class="text-xs font-bold text-link hover:underline">Xem tất cả</router-link>
          </div>
          <div class="p-3 space-y-3">
            <div v-for="item in scheduleTasks" :key="item.id" 
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 rounded-2xl p-4 transition-all surface-solid border border-default">
              <div :class="['flex h-9 w-9 flex-shrink-0 items-center justify-center rounded-xl font-bold', item.alert ? 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]' : 'bg-[var(--color-info-bg)] text-[var(--color-info-text)]']">
                 <component :is="item.alert ? AlertCircle : Clock" :size="20" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2">
                  <h3 class="text-base font-bold text-heading truncate">{{ item.title }}</h3>
                  <span v-if="item.alert" class="rounded-full bg-[var(--color-danger-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-danger-text)] uppercase tracking-wider">Khẩn cấp</span>
                </div>
                <p class="mt-0.5 text-xs text-muted">{{ item.desc }}</p>
              </div>
              <div class="flex items-center gap-2 mt-2 sm:mt-0">
                <router-link :to="item.link" class="lg-button-primary h-9 rounded-lg px-4 text-[10px] font-bold">Xử lý ngay</router-link>
              </div>
            </div>
          </div>
        </div>

        <!-- Class Sections Summary -->
        <div class="surface-card border border-card rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-default px-4 py-4">
            <h2 class="text-lg font-bold text-heading">Tình trạng lớp học phần</h2>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 md:divide-x divide-default">
             <div class="p-4">
                <div class="flex items-center justify-between mb-3">
                   <h3 class="text-xs font-bold text-heading">Lớp sắp đầy (&gt;90%)</h3>
                   <span class="rounded-full bg-[var(--color-warning-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-warning-text)]">8 Lớp</span>
                </div>
                <div class="space-y-3">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-2">
                         <div class="h-1.5 w-1.5 rounded-full bg-[var(--color-warning-text)]"></div>
                         <span class="text-xs text-body">CS101 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-xs font-bold text-heading">48/50</span>
                   </div>
                </div>
                <button class="mt-4 text-xs font-bold text-link">Mở thêm sức chứa →</button>
             </div>
             <div class="p-4">
                <div class="flex items-center justify-between mb-3">
                   <h3 class="text-xs font-bold text-heading">Waitlist cần duyệt</h3>
                   <span class="rounded-full bg-[var(--color-info-bg)] px-2 py-0.5 text-[10px] font-bold text-[var(--color-info-text)]">45 SV</span>
                </div>
                <div class="space-y-3">
                   <div v-for="i in 3" :key="i" class="flex items-center justify-between">
                      <div class="flex items-center gap-2">
                         <div class="h-1.5 w-1.5 rounded-full bg-[var(--color-info-text)]"></div>
                         <span class="text-xs text-body">ENG102 - Lớp L0{{i}}</span>
                      </div>
                      <span class="text-xs font-bold text-heading">{{ 10 + i }} SV chờ</span>
                   </div>
                </div>
                <button class="mt-4 text-xs font-bold text-link">Xử lý danh sách chờ →</button>
             </div>
          </div>
        </div>

      </div>

      <!-- Right sidebar -->
      <div class="space-y-4">
        
        <!-- Urgent Requests -->
        <div class="surface-card border border-card rounded-2xl p-4">
           <div class="mb-3 flex items-center justify-between">
             <h3 class="text-base font-bold text-heading">Đơn từ khẩn</h3>
             <span class="flex h-5 w-5 items-center justify-center rounded-full bg-[var(--color-danger-bg)] text-[9px] font-bold text-[var(--color-danger-text)]">3</span>
           </div>
           <div class="space-y-3">
             <div v-for="req in urgentRequests" :key="req.id" 
                  class="flex items-start gap-3 rounded-xl p-3 transition-all surface-solid border border-default">
               <div class="mt-0.5 h-8 w-8 shrink-0 rounded-lg bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)]">
                  <FileStack :size="16" />
               </div>
               <div class="flex-1 min-w-0">
                 <div class="flex justify-between items-start">
                    <p class="text-xs font-bold text-heading leading-tight">{{ req.type }}</p>
                    <span class="text-[9px] font-bold text-[var(--color-danger-text)] uppercase">{{ req.time }}</span>
                 </div>
                 <p class="mt-0.5 text-[10px] text-muted truncate">{{ req.studentName }}</p>
               </div>
             </div>
           </div>
           <button class="mt-4 w-full lg-button-primary h-9 rounded-lg text-[10px] font-bold">Xử lý toàn bộ đơn</button>
        </div>

        <!-- Strategy Summary / Mini Chart -->
        <div class="surface-card border border-card rounded-2xl p-4">
            <h3 class="text-base font-bold text-heading">Thống kê Học kỳ</h3>
            <p class="text-xs text-muted mt-1">Đã hoàn thành xếp lịch cho 85% các khoa.</p>
            
            <div class="mt-4 flex items-end gap-2 h-20">
              <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h" 
                   class="flex-1 bg-[var(--color-info-bg)] rounded-t-lg transition-all hover:bg-[var(--surface-input)] cursor-help"
                   :style="{ height: h + '%' }" />
            </div>
            
            <div class="mt-4 grid grid-cols-2 gap-3">
               <div class="rounded-xl surface-solid p-2.5 border border-default">
                  <p class="text-[9px] uppercase font-bold text-muted tracking-wider">Lớp học phần</p>
                  <p class="text-base font-black mt-0.5 text-heading">1,240</p>
               </div>
               <div class="rounded-xl surface-solid p-2.5 border border-default">
                  <p class="text-[9px] uppercase font-bold text-muted tracking-wider">Phòng trống</p>
                  <p class="text-base font-black mt-0.5 text-heading">12%</p>
               </div>
            </div>
        </div>

        <!-- Announcements -->
        <div class="surface-card border border-card rounded-2xl p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="14" class="text-muted" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-2 p-2 rounded-xl surface-solid border border-default">
              <div class="h-8 w-8 rounded-full bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
                <Bell :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-heading">Mở đăng ký kỳ Thu 2026</p>
                <p class="text-[10px] text-muted mt-0.5">Hệ thống đã sẵn sàng cho đợt đăng ký tới.</p>
              </div>
            </div>
          </div>
          <button class="mt-4 w-full lg-button-secondary h-9 rounded-lg text-[10px] font-bold">Tất cả thông báo</button>
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
