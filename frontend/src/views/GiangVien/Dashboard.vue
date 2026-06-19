<template>
  <div class="space-y-5 pb-10">

    <!-- ── Welcome Header ── -->
    <div class="lg-glass-strong rounded-[24px] p-6 lg:p-8">
      <div class="flex flex-col md:flex-row items-center justify-between gap-6">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-2xl md:text-3xl font-bold tracking-tight text-heading">
            Chào buổi sáng, <span class="text-[var(--accent-primary)]">TS. Nguyễn Minh Khoa!</span>
          </h1>
          <p class="mt-2 text-muted text-sm">
            Hôm nay có 2 ca dạy và 24 bài đang chờ chấm.
          </p>
          <div class="mt-6 flex flex-wrap justify-center md:justify-start gap-3">
            <router-link to="/teacher/schedule" class="lg-btn-primary px-5 py-2.5 text-sm font-bold shadow-[var(--lg-shadow-md)]">
              Xem lịch dạy
            </router-link>
            <router-link to="/teacher/grading" class="lg-btn-secondary px-5 py-2.5 text-sm font-bold">
              Chấm điểm ngay
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block relative group">
          <div class="absolute -inset-1 rounded-3xl bg-[var(--accent-primary)] opacity-10 blur-xl group-hover:opacity-20 transition-opacity"></div>
          <div class="relative flex h-24 w-24 items-center justify-center rounded-[24px] lg-glass border border-[var(--accent-primary)]/20 shadow-xl">
            <BookOpen :size="42" class="text-[var(--accent-primary)]" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid (compact, like GiaoVu) ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id"
           class="lg-glass-soft group relative overflow-hidden rounded-[20px] p-5 transition-all duration-300 hover:shadow-[var(--lg-shadow-lg)] hover:-translate-y-1">
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

      <!-- Left (2/3) -->
      <div class="xl:col-span-2 space-y-5">

        <!-- Today's Schedule -->
        <div class="lg-glass rounded-[24px] overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-5 py-4 bg-[var(--surface-table-header)]/40">
            <div>
              <h2 class="text-base font-bold text-heading">Lịch dạy hôm nay</h2>
              <p class="text-xs text-muted mt-0.5">Các lớp học bạn phụ trách trong ngày</p>
            </div>
            <router-link to="/teacher/schedule" class="text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Xem tất cả</router-link>
          </div>
          <div class="p-4 space-y-3">
            <div v-for="item in teachingSchedule" :key="item.id"
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-4 rounded-[16px] lg-solid-soft p-4 transition-all hover:border-[var(--accent-primary)]/30 hover:bg-[var(--accent-primary-soft)] hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 border border-default">
              <div class="flex h-11 w-11 flex-shrink-0 flex-col items-center justify-center rounded-[12px] bg-[var(--accent-primary)]/10 text-link font-bold shadow-inner">
                <span class="text-[9px] font-bold uppercase tracking-tighter leading-tight">{{ item.time.split(' ')[0] }}</span>
                <span class="text-[9px] font-semibold leading-tight">AM</span>
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="text-sm font-bold text-heading truncate group-hover:text-link transition-colors">{{ item.subject }}</h3>
                <div class="flex items-center gap-2 mt-1">
                  <span class="text-[11px] text-label font-medium">{{ item.code }}</span>
                  <span class="h-1 w-1 rounded-full bg-[var(--border-default)]"></span>
                  <span class="text-[11px] text-label font-medium">{{ item.room }}</span>
                </div>
              </div>
              <div class="flex items-center gap-3 mt-3 sm:mt-0">
                <div class="flex items-center gap-1.5 text-[11px] font-semibold text-body">
                  <Users :size="14" class="text-muted" /> {{ item.students }} SV
                </div>
                <GlassBadge :variant="item.status === 'completed' ? 'neutral' : 'primary'" class="px-2.5 py-1">
                  {{ item.status === 'completed' ? 'Đã hoàn thành' : 'Sắp diễn ra' }}
                </GlassBadge>
              </div>
            </div>
          </div>
        </div>

        <!-- Submission Progress -->
        <div class="lg-glass rounded-[24px] overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-5 py-4 bg-[var(--surface-table-header)]/40">
            <h2 class="text-base font-bold text-heading">Tiến độ nộp bài tập (Tuần 12)</h2>
            <select class="lg-input rounded-xl px-3 py-1.5 text-xs font-bold w-40">
              <option>Tất cả các lớp</option>
              <option>CTDL101_L01</option>
            </select>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-y md:divide-y-0 md:divide-x divide-[var(--border-card)]">
            <div class="p-5">
              <div class="flex items-center justify-between mb-4">
                <h3 class="text-xs font-bold text-body">Tỷ lệ nộp bài</h3>
                <GlassBadge variant="success" class="px-2.5 py-1 text-[10px]">85%</GlassBadge>
              </div>
              <div class="space-y-3">
                <div v-for="(item, i) in submissionStats" :key="i" class="flex items-center justify-between">
                  <div class="flex items-center gap-2.5">
                    <div class="h-2 w-2 rounded-full shadow-sm" :class="item.colorClass"></div>
                    <span class="text-xs text-body font-medium">{{ item.label }}</span>
                  </div>
                  <span class="text-xs font-bold text-heading tabular-nums">{{ item.value }}</span>
                </div>
              </div>
              <button class="mt-5 flex items-center text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Xem chi tiết <ArrowUpRight :size="14" class="ml-1"/></button>
            </div>
            <div class="p-5">
              <div class="flex items-center justify-between mb-4">
                <h3 class="text-xs font-bold text-body">Bài chưa chấm</h3>
                <GlassBadge variant="warning" class="px-2.5 py-1 text-[10px]">24 bài</GlassBadge>
              </div>
              <div class="space-y-3">
                <div v-for="(item, i) in gradingStats" :key="i" class="flex items-center justify-between">
                  <div class="flex items-center gap-2.5">
                    <div class="h-2 w-2 rounded-full shadow-sm" :class="item.colorClass"></div>
                    <span class="text-xs text-body font-medium">{{ item.label }}</span>
                  </div>
                  <span class="text-xs font-bold text-heading tabular-nums">{{ item.value }}</span>
                </div>
              </div>
              <button class="mt-5 flex items-center text-xs font-bold text-link hover:underline decoration-2 underline-offset-2">Đi chấm ngay <ArrowUpRight :size="14" class="ml-1"/></button>
            </div>
          </div>
        </div>

      </div>

      <!-- Right (1/3) -->
      <div class="space-y-5">

        <!-- Recent Submissions -->
        <div class="lg-glass rounded-[24px] p-5">
          <div class="mb-4 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Bài nộp mới</h3>
            <span class="flex h-6 w-6 items-center justify-center rounded-full bg-[var(--color-warning-bg)] text-[11px] font-bold text-[var(--color-warning-text)] shadow-sm">6</span>
          </div>
          <div class="space-y-3">
            <div v-for="sub in recentSubmissions" :key="sub.id"
                 class="group flex items-start gap-3 rounded-[16px] lg-solid-soft p-3 transition-all hover:shadow-[var(--lg-shadow-sm)] hover:-translate-y-0.5 border border-default cursor-pointer">
              <div class="h-9 w-9 shrink-0 rounded-xl bg-[var(--accent-primary-soft)] flex items-center justify-center text-link">
                <User :size="16" stroke-width="2.5" />
              </div>
              <div class="flex-1 min-w-0 pt-0.5">
                <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight truncate pr-2 group-hover:text-link transition-colors">{{ sub.student }}</p>
                  <span v-if="sub.status === 'new'" class="text-[9px] font-bold text-link">NEW</span>
                </div>
                <p class="mt-1 text-[10px] font-medium text-label truncate">{{ sub.assignment }} · {{ sub.course }}</p>
                <p class="text-[9px] text-muted mt-1">{{ sub.time }}</p>
              </div>
            </div>
          </div>
          <button class="mt-4 w-full lg-btn-subtle h-10 rounded-[12px] text-xs font-bold">Xem tất cả bài nộp</button>
        </div>

        <!-- Teaching Stats -->
        <div class="rounded-[24px] p-5 text-white overflow-hidden relative shadow-[var(--lg-shadow-md)]" style="background: linear-gradient(135deg, var(--lg-primary-dark), var(--lg-primary) 52%, var(--lg-cyan));">
          <h3 class="text-base font-bold">Thống kê giảng dạy</h3>
          <p class="text-xs opacity-80 mt-1 leading-relaxed">Hiệu suất các lớp trong học kỳ.</p>

          <div class="mt-5 flex items-end gap-2 h-24">
            <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h"
                 class="flex-1 bg-white/25 rounded-t-lg transition-all hover:bg-white/40 hover:scale-y-105 cursor-help"
                 :style="{ height: h + '%' }" />
          </div>

          <div class="mt-5 grid grid-cols-2 gap-3">
            <div class="rounded-[16px] bg-white/10 p-3 backdrop-blur-md border border-white/20 text-center shadow-inner">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Lớp đang dạy</p>
              <p class="text-lg font-bold mt-1">8</p>
            </div>
            <div class="rounded-[16px] bg-white/10 p-3 backdrop-blur-md border border-white/20 text-center shadow-inner">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Hiệu suất</p>
              <p class="text-lg font-bold mt-1">82%</p>
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
              <div class="h-9 w-9 rounded-xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] shrink-0">
                <Users :size="18" stroke-width="2.5" />
              </div>
              <div>
                <p class="text-xs font-bold text-heading">Họp bộ môn thường kỳ</p>
                <p class="text-[11px] font-medium text-label mt-1">14:00 Thứ 6, 16/05 tại Phòng họp 2.</p>
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
import GlassBadge from '@/components/ui/GlassBadge.vue'
import {
  Users, BookOpen, ClipboardCheck, TrendingUp,
  ArrowUpRight, AlertCircle, User, Bell
} from 'lucide-vue-next'

const stats = [
  { id: 1, label: 'Tổng sinh viên', value: '452', trend: '+12%', isNegative: false, bgColor: 'bg-[var(--accent-primary-soft)]', iconColor: 'text-[var(--text-link)]', icon: Users },
  { id: 2, label: 'Lớp đang dạy', value: '8', trend: 'Học kỳ 2', isNegative: false, bgColor: 'bg-[var(--accent-primary-soft)]', iconColor: 'text-[var(--text-link)]', icon: BookOpen },
  { id: 3, label: 'Bài chờ chấm', value: '24', trend: '6 bài gấp', isNegative: true, bgColor: 'bg-[var(--accent-primary-soft)]', iconColor: 'text-[var(--text-link)]', icon: ClipboardCheck },
  { id: 4, label: 'Hiệu suất lớp', value: '82%', trend: '+5%', isNegative: false, bgColor: 'bg-[var(--accent-primary-soft)]', iconColor: 'text-[var(--text-link)]', icon: TrendingUp },
]

const teachingSchedule = [
  { id: 1, subject: 'Cấu trúc dữ liệu & Giải thuật', code: 'CTDL101_L01', time: '07:30 - 09:30', room: 'Phòng 302 - Cơ sở chính', students: 45, status: 'completed' },
  { id: 2, subject: 'Lập trình hướng đối tượng', code: 'OOP202_L03', time: '13:30 - 15:30', room: 'Phòng 105 - Cơ sở chính', students: 38, status: 'upcoming' },
  { id: 3, subject: 'Hệ quản trị CSDL', code: 'DBM301_L02', time: '15:45 - 17:45', room: 'Lab 2 - Cơ sở phụ', students: 42, status: 'upcoming' },
]

const recentSubmissions = [
  { id: 1, student: 'Lê Văn B', course: 'Lập trình Web', assignment: 'Bài tập 2: CSS Flexbox', time: '10 phút trước', status: 'new' },
  { id: 2, student: 'Trần Thị C', course: 'OOP', assignment: 'Lab 4: Polymorphism', time: '45 phút trước', status: 'new' },
  { id: 3, student: 'Nguyễn Văn A', course: 'CTDL&GT', assignment: 'Tiểu luận giữa kỳ', time: '2 giờ trước', status: 'graded' },
  { id: 4, student: 'Phạm Minh D', course: 'HQTCSDL', assignment: 'Truy vấn SQL nâng cao', time: 'Hôm qua', status: 'graded' },
]

const submissionStats = [
  { label: 'CTDL&GT - L01', value: '42/45', colorClass: 'bg-emerald-400' },
  { label: 'OOP - L03', value: '35/38', colorClass: 'bg-blue-400' },
  { label: 'HQTCSDL - L02', value: '40/42', colorClass: 'bg-cyan-400' },
]

const gradingStats = [
  { label: 'CTDL&GT - Bài tập 2', value: '12 bài', colorClass: 'bg-orange-400' },
  { label: 'OOP - Lab 4', value: '8 bài', colorClass: 'bg-amber-400' },
  { label: 'HQTCSDL - SQL', value: '4 bài', colorClass: 'bg-red-400' },
]
</script>
