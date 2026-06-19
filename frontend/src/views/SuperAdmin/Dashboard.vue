<script setup>
/**
 * Dashboard.vue - Super Admin
 * Trang tổng quan hệ thống cho Super Admin.
 * Hiển thị KPI toàn trường, cảnh báo hệ thống, hoạt động gần đây.
 */
import { ref } from 'vue'
import {
  Users, Building2, GraduationCap, ShieldAlert,
  TrendingUp, TrendingDown, Activity, Server,
  CheckCircle, AlertTriangle, Clock, Zap,
} from 'lucide-vue-next'

const systemStats = ref([
  {
    id: 'total-users',
    label: 'Tổng người dùng',
    value: '12,847',
    change: '+3.2%',
    trend: 'up',
    icon: 'Users',
    color: 'violet',
    sub: '245 tài khoản mới tháng này',
  },
  {
    id: 'active-orgs',
    label: 'Đơn vị hoạt động',
    value: '38',
    change: '+2',
    trend: 'up',
    icon: 'Building2',
    color: 'blue',
    sub: '6 khoa, 32 phòng ban',
  },
  {
    id: 'total-courses',
    label: 'Môn học đang mở',
    value: '1,204',
    change: '+8.1%',
    trend: 'up',
    icon: 'GraduationCap',
    color: 'emerald',
    sub: 'Học kỳ 2 · 2024–2025',
  },
  {
    id: 'system-health',
    label: 'Uptime hệ thống',
    value: '99.97%',
    change: '-0.01%',
    trend: 'down',
    icon: 'Server',
    color: 'amber',
    sub: 'Hoạt động ổn định',
  },
])

const recentActivities = ref([
  { id: 1, type: 'user', icon: 'UserPlus', color: 'blue', text: 'Tài khoản GV Nguyễn Văn A được kích hoạt', time: '2 phút trước' },
  { id: 2, type: 'security', icon: 'ShieldAlert', color: 'red', text: 'Phát hiện đăng nhập bất thường từ IP 192.168.1.50', time: '15 phút trước' },
  { id: 3, type: 'config', icon: 'Settings2', color: 'violet', text: 'Cấu hình học kỳ 2/2025 được cập nhật', time: '1 giờ trước' },
  { id: 4, type: 'backup', icon: 'DatabaseBackup', color: 'emerald', text: 'Backup tự động lúc 02:00 hoàn tất thành công', time: '6 giờ trước' },
  { id: 5, type: 'org', icon: 'Building2', color: 'blue', text: 'Khoa Công nghệ Thông tin cập nhật cơ cấu tổ chức', time: 'Hôm qua' },
])

const colorMap = {
  violet: { bg: 'bg-violet-50 dark:bg-violet-600/20', text: 'text-violet-600 dark:text-violet-400', ring: 'ring-violet-100 dark:ring-violet-500/20' },
  blue: { bg: 'bg-blue-50 dark:bg-blue-600/20', text: 'text-blue-600 dark:text-blue-400', ring: 'ring-blue-100 dark:ring-blue-500/20' },
  emerald: { bg: 'bg-emerald-50 dark:bg-emerald-600/20', text: 'text-emerald-600 dark:text-emerald-400', ring: 'ring-emerald-100 dark:ring-emerald-500/20' },
  amber: { bg: 'bg-amber-50 dark:bg-amber-600/20', text: 'text-amber-600 dark:text-amber-400', ring: 'ring-amber-100 dark:ring-amber-500/20' },
  red: { bg: 'bg-red-50 dark:bg-red-600/20', text: 'text-red-600 dark:text-red-400', ring: 'ring-red-100 dark:ring-red-500/20' },
}

import * as LucideIcons from 'lucide-vue-next'
function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Circle
}
</script>

<template>
  <div class="space-y-6 pb-6">
    <!-- Welcome Banner -->
    <div class="lg-glass-strong rounded-[24px] p-6 lg:p-8">
      <div class="flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <div class="mb-2 inline-flex items-center gap-2">
            <span class="rounded-full bg-[var(--lg-primary)]/10 text-[var(--lg-primary)] border border-[var(--lg-primary)]/20 px-3 py-1 text-[10px] font-bold uppercase tracking-widest shadow-sm">Super Admin Portal</span>
          </div>
          <h2 class="text-2xl md:text-3xl font-bold tracking-tight text-heading">Chào mừng trở lại! 👋</h2>
          <p class="mt-2 text-sm text-muted leading-relaxed">Hệ thống đang hoạt động bình thường · {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</p>
        </div>
        <div class="hidden lg:flex items-center gap-3">
          <div class="flex items-center gap-2 rounded-[16px] lg-solid-soft px-4 py-2.5 text-sm font-bold text-heading shadow-sm border border-default">
            <div class="relative flex h-2.5 w-2.5">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-[var(--color-success-text)] opacity-75"></span>
              <span class="relative inline-flex rounded-full h-2.5 w-2.5 bg-[var(--color-success-text)]"></span>
            </div>
            Hệ thống ổn định
          </div>
        </div>
      </div>
    </div>

    <!-- KPI Stats Grid -->
    <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <div
        v-for="stat in systemStats"
        :key="stat.id"
        class="group lg-glass-soft rounded-[20px] p-5 shadow-[var(--lg-shadow-sm)] transition-all duration-300 hover:shadow-[var(--lg-shadow-lg)] hover:-translate-y-1"
      >
        <div class="flex items-start justify-between">
          <div>
            <p class="text-sm font-semibold text-muted tracking-tight">{{ stat.label }}</p>
            <p class="mt-1.5 text-3xl font-extrabold text-heading tracking-tight">{{ stat.value }}</p>
          </div>
          <div :class="['flex h-12 w-12 items-center justify-center rounded-xl shadow-sm border border-white/20 dark:border-white/5 transition-transform group-hover:scale-110', colorMap[stat.color]?.bg, colorMap[stat.color]?.text]">
            <component :is="getIcon(stat.icon)" :size="24" stroke-width="2" />
          </div>
        </div>
        <div class="mt-4 flex items-center gap-2">
          <span :class="['flex items-center gap-1 rounded-full px-2 py-0.5 text-[11px] font-bold shadow-sm border border-white/10 dark:border-white/5', stat.trend === 'up' ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]' : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]']">
            <component :is="stat.trend === 'up' ? TrendingUp : TrendingDown" :size="12" stroke-width="2.5" />
            {{ stat.change }}
          </span>
          <span class="text-[11px] font-medium text-muted truncate">{{ stat.sub }}</span>
        </div>
      </div>
    </div>

    <!-- Content Grid: Activity + Quick Actions -->
    <div class="grid grid-cols-1 gap-5 lg:grid-cols-3">

      <!-- Recent Activity (chiếm 2 cột) -->
      <div class="lg:col-span-2 lg-glass rounded-[24px] p-6 shadow-[var(--lg-shadow-sm)]">
        <div class="mb-5 flex items-center justify-between border-b border-card pb-4">
          <h3 class="text-base font-bold text-heading">Hoạt động gần đây</h3>
          <router-link to="/super-admin/audit/logs" class="text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Xem tất cả</router-link>
        </div>
        <div class="space-y-3">
          <div
            v-for="activity in recentActivities"
            :key="activity.id"
            class="group flex items-start gap-4 rounded-[16px] lg-solid-soft p-4 transition-all hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 border border-default cursor-pointer"
          >
            <div :class="['flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-[12px] shadow-sm border border-white/20 dark:border-white/5', colorMap[activity.color]?.bg, colorMap[activity.color]?.text]">
              <component :is="getIcon(activity.icon)" :size="18" stroke-width="2.5" />
            </div>
            <div class="min-w-0 flex-1 pt-0.5">
              <p class="text-sm font-bold text-heading leading-snug group-hover:text-link transition-colors">{{ activity.text }}</p>
              <p class="mt-1 flex items-center gap-1.5 text-[11px] font-medium text-muted">
                <Clock :size="12" />
                {{ activity.time }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="lg-glass rounded-[24px] p-6 shadow-[var(--lg-shadow-sm)] flex flex-col">
        <h3 class="mb-5 text-base font-bold text-heading border-b border-card pb-4">Thao tác nhanh</h3>
        <div class="space-y-3 flex-1">
          <router-link
            v-for="action in [
              { label: 'Import tài khoản', icon: 'FileSpreadsheet', to: '/super-admin/users/import', color: 'violet' },
              { label: 'Thiết lập cơ sở mới', icon: 'PlusCircle', to: '/super-admin/organizations/form', color: 'blue' },
              { label: 'Cấu hình học kỳ', icon: 'CalendarRange', to: '/super-admin/training/semesters', color: 'emerald' },
              { label: 'Xem Audit Logs', icon: 'ScrollText', to: '/super-admin/audit/logs', color: 'amber' },
              { label: 'Cấu hình AI & Job', icon: 'Cpu', to: '/super-admin/system/ai-automation', color: 'red' },
            ]"
            :key="action.label"
            :to="action.to"
            class="group flex items-center gap-3 rounded-[16px] border border-default lg-solid-soft px-4 py-3 text-sm font-bold text-heading shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-[var(--lg-shadow-sm)] hover:border-violet-200 dark:hover:border-violet-900/50 hover:bg-[var(--surface-card-hover)]"
          >
            <div :class="['flex h-8 w-8 items-center justify-center rounded-[10px] shadow-inner', colorMap[action.color]?.bg, colorMap[action.color]?.text]">
              <component :is="getIcon(action.icon)" :size="16" class="flex-shrink-0" stroke-width="2.5" />
            </div>
            <span class="group-hover:text-violet-600 dark:group-hover:text-violet-400 transition-colors">{{ action.label }}</span>
          </router-link>
        </div>

        <!-- System status -->
        <div class="mt-5 rounded-[16px] border border-[var(--color-success-text)]/20 bg-[var(--color-success-bg)]/50 p-4 shadow-sm backdrop-blur-md">
          <div class="flex items-center gap-2.5">
            <div class="relative flex h-2.5 w-2.5">
              <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-[var(--color-success-text)] opacity-75"></span>
              <span class="relative inline-flex rounded-full h-2.5 w-2.5 bg-[var(--color-success-text)]"></span>
            </div>
            <p class="text-[12px] font-bold text-[var(--color-success-text)]">Hệ thống hoạt động bình thường</p>
          </div>
          <p class="mt-1.5 text-[11px] font-medium text-[var(--color-success-text)]/80">Tất cả dịch vụ đang chạy ổn định</p>
        </div>
      </div>

    </div>
  </div>
</template>
