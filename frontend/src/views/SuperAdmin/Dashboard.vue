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
  <div class="space-y-6">
    <!-- Welcome Banner -->
    <div class="relative overflow-hidden rounded-2xl border border-violet-100/60 dark:border-violet-500/20 bg-gradient-to-br from-violet-600 via-violet-500 to-purple-600 p-6 text-white shadow-lg shadow-violet-500/20">
      <div class="pointer-events-none absolute inset-0 overflow-hidden">
        <div class="absolute -right-16 -top-16 h-48 w-48 rounded-full bg-white/10 blur-2xl" />
        <div class="absolute -bottom-8 left-1/3 h-32 w-32 rounded-full bg-purple-300/20 blur-2xl" />
      </div>
      <div class="relative flex items-center justify-between">
        <div>
          <div class="mb-1 flex items-center gap-2">
            <span class="rounded-full bg-white/20 px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-widest">Super Admin Portal</span>
          </div>
          <h2 class="text-2xl font-bold">Chào mừng trở lại! 👋</h2>
          <p class="mt-1 text-sm text-violet-100">Hệ thống đang hoạt động bình thường · {{ new Date().toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</p>
        </div>
        <div class="hidden lg:flex items-center gap-2">
          <div class="flex items-center gap-1.5 rounded-xl bg-white/15 px-3 py-2 text-sm font-semibold backdrop-blur-sm">
            <Activity :size="14" />
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
        class="lg-glass-soft rounded-2xl border border-card p-4 shadow-sm transition-all hover:shadow-md hover:-translate-y-0.5"
      >
        <div class="flex items-start justify-between">
          <div>
            <p class="text-xs font-semibold text-muted">{{ stat.label }}</p>
            <p class="mt-1.5 text-2xl font-bold text-heading">{{ stat.value }}</p>
          </div>
          <div :class="['flex h-10 w-10 items-center justify-center rounded-xl ring-1', colorMap[stat.color]?.bg, colorMap[stat.color]?.ring]">
            <component :is="getIcon(stat.icon)" :size="18" :class="colorMap[stat.color]?.text" />
          </div>
        </div>
        <div class="mt-3 flex items-center gap-1.5">
          <span :class="['flex items-center gap-0.5 text-[11px] font-bold', stat.trend === 'up' ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-500 dark:text-red-400']">
            <component :is="stat.trend === 'up' ? TrendingUp : TrendingDown" :size="11" />
            {{ stat.change }}
          </span>
          <span class="text-[10px] text-placeholder">{{ stat.sub }}</span>
        </div>
      </div>
    </div>

    <!-- Content Grid: Activity + Quick Actions -->
    <div class="grid grid-cols-1 gap-4 lg:grid-cols-3">

      <!-- Recent Activity (chiếm 2 cột) -->
      <div class="lg:col-span-2 lg-glass-soft rounded-2xl border border-card p-4 shadow-sm">
        <div class="mb-4 flex items-center justify-between">
          <h3 class="text-sm font-bold text-heading">Hoạt động gần đây</h3>
          <router-link to="/super-admin/audit/logs" class="text-[11px] font-bold text-violet-600 dark:text-violet-400 hover:underline">Xem tất cả</router-link>
        </div>
        <div class="space-y-3">
          <div
            v-for="activity in recentActivities"
            :key="activity.id"
            class="flex items-start gap-3 rounded-xl p-2.5 transition-colors hover:bg-[var(--surface-card-hover)]"
          >
            <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full', colorMap[activity.color]?.bg]">
              <component :is="getIcon(activity.icon)" :size="14" :class="colorMap[activity.color]?.text" />
            </div>
            <div class="min-w-0 flex-1">
              <p class="text-[12px] font-semibold text-body leading-snug">{{ activity.text }}</p>
              <p class="mt-0.5 flex items-center gap-1 text-[10px] text-muted">
                <Clock :size="9" />
                {{ activity.time }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="lg-glass-soft rounded-2xl border border-card p-4 shadow-sm">
        <h3 class="mb-4 text-sm font-bold text-heading">Thao tác nhanh</h3>
        <div class="space-y-2">
          <router-link
            v-for="action in [
              { label: 'Import tài khoản', icon: 'FileSpreadsheet', to: '/super-admin/users?action=import', color: 'violet' },
              { label: 'Thiết lập cơ sở mới', icon: 'PlusCircle', to: '/super-admin/organizations?action=create', color: 'blue' },
              { label: 'Cấu hình học kỳ', icon: 'CalendarRange', to: '/super-admin/training/semesters', color: 'emerald' },
              { label: 'Xem Audit Logs', icon: 'ScrollText', to: '/super-admin/audit/logs', color: 'amber' },
              { label: 'Cấu hình AI & Job', icon: 'Cpu', to: '/super-admin/system/ai-automation', color: 'red' },
            ]"
            :key="action.label"
            :to="action.to"
            class="flex items-center gap-3 rounded-xl border border-transparent px-3 py-2.5 text-[12px] font-semibold text-label transition-all hover:border-violet-100 dark:hover:border-violet-500/20 hover:bg-violet-50/60 dark:hover:bg-violet-600/10 hover:text-violet-700 dark:hover:text-violet-300"
          >
            <component :is="getIcon(action.icon)" :size="14" class="flex-shrink-0 text-placeholder" />
            {{ action.label }}
          </router-link>
        </div>

        <!-- System status -->
        <div class="mt-4 rounded-xl border border-emerald-100 dark:border-emerald-500/20 bg-emerald-50/60 dark:bg-emerald-600/10 p-3">
          <div class="flex items-center gap-2">
            <div class="h-2 w-2 rounded-full bg-emerald-500 shadow-[0_0_6px_rgba(16,185,129,0.5)] animate-pulse" />
            <p class="text-[11px] font-bold text-emerald-700 dark:text-emerald-400">Hệ thống hoạt động bình thường</p>
          </div>
          <p class="mt-1 text-[10px] text-emerald-600/70 dark:text-emerald-400/60">Tất cả dịch vụ đang chạy ổn định</p>
        </div>
      </div>

    </div>
  </div>
</template>
