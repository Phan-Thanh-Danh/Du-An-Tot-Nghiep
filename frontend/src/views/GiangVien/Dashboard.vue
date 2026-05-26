<template>
  <div class="space-y-5 pb-10">

    <!-- ── Welcome Hero ── -->
    <div class="rounded-[24px] bg-blue-900 p-5 text-white shadow-lg shadow-blue-200/30">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-lg font-extrabold leading-tight tracking-tight">
            Chào buổi sáng, <span class="text-blue-200">TS. Nguyễn Minh Khoa!</span>
          </h1>
          <p class="mt-1 text-blue-100/80 text-sm">
            Hôm nay có 2 ca dạy và 24 bài đang chờ chấm.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/teacher/schedule" class="rounded-lg bg-white px-4 py-2 text-xs font-bold text-blue-900 shadow-lg hover:bg-blue-50 transition-all active:scale-95">
              Xem lịch dạy
            </router-link>
            <router-link to="/teacher/grading" class="rounded-lg bg-blue-700/50 backdrop-blur px-4 py-2 text-xs font-bold text-white border border-blue-400/30 hover:bg-blue-700 transition-all">
              Chấm điểm ngay
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-20 w-20 items-center justify-center rounded-2xl bg-white/10 backdrop-blur border border-white/20">
            <BookOpen :size="32" class="text-white/80" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Stats Grid (compact, like GiaoVu) ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id"
           class="lg-glass-soft group relative overflow-hidden rounded-[20px] p-5 transition-all hover:shadow-lg hover:-translate-y-0.5">
        <div class="flex items-center justify-between">
          <div :class="['flex h-11 w-11 items-center justify-center rounded-xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="22" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold', item.isNegative ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-600']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="11" />
            <AlertCircle v-else :size="11" />
          </div>
        </div>
        <div class="mt-4">
          <p class="text-sm font-medium text-slate-500 dark:text-slate-400">{{ item.label }}</p>
          <p class="mt-0.5 text-xl font-black text-heading">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-5">

      <!-- Left (2/3) -->
      <div class="xl:col-span-2 space-y-5">

        <!-- Today's Schedule -->
        <div class="lg-glass-soft rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-white/45 dark:border-white/10 px-4 py-3">
            <div>
              <h2 class="text-base font-bold text-heading">Lịch dạy hôm nay</h2>
              <p class="text-xs text-slate-400 dark:text-slate-500 mt-0.5">Các lớp học bạn phụ trách trong ngày</p>
            </div>
            <router-link to="/teacher/schedule" class="text-xs font-bold text-link">Xem tất cả</router-link>
          </div>
          <div class="p-3 space-y-2">
            <div v-for="item in teachingSchedule" :key="item.id"
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 rounded-xl border border-white/40 dark:border-white/10 p-3 transition-all hover:border-blue-100 dark:hover:border-blue-500/30 hover:bg-blue-50/20 dark:hover:bg-blue-500/10">
              <div class="flex h-9 w-9 flex-shrink-0 flex-col items-center justify-center rounded-lg bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300 font-bold border border-blue-200 dark:border-blue-500/30">
                <span class="text-[8px] font-bold uppercase tracking-tighter leading-tight">{{ item.time.split(' ')[0] }}</span>
                <span class="text-[8px] font-black leading-tight">AM</span>
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="text-sm font-bold text-heading truncate group-hover:text-blue-700 dark:group-hover:text-blue-300 transition-colors">{{ item.subject }}</h3>
                <div class="flex items-center gap-2 mt-0.5">
                  <span class="text-[11px] text-label font-medium">{{ item.code }}</span>
                  <span class="h-1 w-1 rounded-full bg-slate-300 dark:bg-slate-600"></span>
                  <span class="text-[11px] text-label font-medium">{{ item.room }}</span>
                </div>
              </div>
              <div class="flex items-center gap-3 mt-1 sm:mt-0">
                <div class="flex items-center gap-1 text-[11px] font-semibold text-body">
                  <Users :size="12" class="text-slate-400 dark:text-slate-500" /> {{ item.students }} SV
                </div>
                <span :class="['px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wider', item.status === 'completed' ? 'bg-slate-100 dark:bg-slate-700/40 text-slate-500 dark:text-slate-400' : 'bg-blue-100 dark:bg-blue-900/40 text-blue-700 dark:text-blue-300']">
                  {{ item.status === 'completed' ? 'Đã hoàn thành' : 'Sắp diễn ra' }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <!-- Submission Progress -->
        <div class="lg-glass-soft rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-white/45 dark:border-white/10 px-4 py-3">
            <h2 class="text-base font-bold text-heading">Tiến độ nộp bài tập (Tuần 12)</h2>
            <select class="rounded-lg border border-white/40 dark:border-white/10 bg-surface-input px-2 py-1 text-[10px] font-bold text-body outline-none placeholder:text-placeholder">
              <option>Tất cả các lớp</option>
              <option>CTDL101_L01</option>
            </select>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-x divide-white/40 dark:divide-white/10">
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Tỷ lệ nộp bài</h3>
                <span class="rounded-full bg-green-100 dark:bg-green-900/40 px-2 py-0.5 text-[10px] font-bold text-green-700 dark:text-green-300">85%</span>
              </div>
              <div class="space-y-3">
                <div v-for="(item, i) in submissionStats" :key="i" class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <div class="h-1.5 w-1.5 rounded-full" :class="item.colorClass"></div>
                    <span class="text-xs text-body">{{ item.label }}</span>
                  </div>
                  <span class="text-xs font-bold text-heading">{{ item.value }}</span>
                </div>
              </div>
              <button class="mt-3 text-xs font-bold text-link">Xem chi tiết →</button>
            </div>
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Bài chưa chấm</h3>
                <span class="rounded-full bg-orange-100 dark:bg-orange-900/40 px-2 py-0.5 text-[10px] font-bold text-orange-700 dark:text-orange-300">24 bài</span>
              </div>
              <div class="space-y-3">
                <div v-for="(item, i) in gradingStats" :key="i" class="flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <div class="h-1.5 w-1.5 rounded-full" :class="item.colorClass"></div>
                    <span class="text-xs text-body">{{ item.label }}</span>
                  </div>
                  <span class="text-xs font-bold text-heading">{{ item.value }}</span>
                </div>
              </div>
              <button class="mt-3 text-xs font-bold text-link">Đi chấm ngay →</button>
            </div>
          </div>
        </div>

      </div>

      <!-- Right (1/3) -->
      <div class="space-y-5">

        <!-- Recent Submissions -->
        <div class="lg-glass-soft rounded-2xl p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-bold text-heading">Bài nộp mới</h3>
            <span class="rounded-full bg-orange-100 dark:bg-orange-900/40 px-2 py-0.5 text-[9px] font-bold text-orange-700 dark:text-orange-300">6 gấp</span>
          </div>
          <div class="space-y-2">
            <div v-for="sub in recentSubmissions" :key="sub.id"
                 class="flex items-start gap-2 rounded-lg border border-white/40 dark:border-white/10 bg-white/30 dark:bg-slate-700/30 p-2.5 transition-all hover:bg-white/60 dark:hover:bg-slate-700/50 hover:shadow-md group">
              <div class="mt-0.5 h-8 w-8 shrink-0 rounded-lg bg-blue-50 dark:bg-blue-900/40 flex items-center justify-center text-blue-600 dark:text-blue-300">
                <User :size="14" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight">{{ sub.student }}</p>
                  <span v-if="sub.status === 'new'" class="text-[9px] font-bold text-link">NEW</span>
                </div>
                <p class="mt-0.5 text-[10px] text-label truncate">{{ sub.assignment }} · {{ sub.course }}</p>
                <p class="text-[9px] text-slate-400 dark:text-slate-500 mt-0.5">{{ sub.time }}</p>
              </div>
            </div>
          </div>
          <button class="mt-3 w-full rounded-lg bg-blue-50 dark:bg-blue-900/30 py-2 text-[10px] font-bold text-blue-700 dark:text-blue-300 hover:bg-blue-100 dark:hover:bg-blue-900/50 transition-colors">Xem tất cả bài nộp</button>
        </div>

        <!-- Teaching Stats -->
        <div class="rounded-2xl bg-blue-600 dark:bg-blue-800 p-4 text-white overflow-hidden relative">
          <h3 class="text-sm font-bold">Thống kê giảng dạy</h3>
          <p class="text-xs text-blue-100/70 mt-0.5">Hiệu suất các lớp trong học kỳ.</p>

          <div class="mt-4 flex items-end gap-2 h-16">
            <div v-for="h in [60, 40, 80, 50, 70, 95, 65]" :key="h"
                 class="flex-1 bg-white/20 rounded-t-lg transition-all hover:bg-white/40 cursor-help"
                 :style="{ height: h + '%' }" />
          </div>

          <div class="mt-4 grid grid-cols-2 gap-2">
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold text-blue-200 tracking-wider">Lớp đang dạy</p>
              <p class="text-base font-black mt-0.5">8</p>
            </div>
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold text-blue-200 tracking-wider">Hiệu suất</p>
              <p class="text-base font-black mt-0.5">82%</p>
            </div>
          </div>
        </div>

        <!-- Announcements -->
        <div class="lg-glass-soft rounded-2xl p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-bold text-heading">Thông báo</h3>
            <Bell :size="14" class="text-slate-400 dark:text-slate-500" />
          </div>
          <div class="space-y-2">
            <div class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-green-50 dark:bg-green-900/40 flex items-center justify-center text-green-500 dark:text-green-300 shrink-0">
                <Users :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-body">Họp bộ môn thường kỳ</p>
                <p class="text-[10px] text-label mt-0.5">14:00 Thứ 6, ngày 16/05 tại Phòng họp 2.</p>
              </div>
            </div>
          </div>
          <button class="mt-3 w-full rounded-lg bg-slate-50 dark:bg-slate-700/30 py-2 text-[10px] font-bold text-slate-500 dark:text-slate-400 hover:bg-slate-100 dark:hover:bg-slate-700/50 transition-colors">Tất cả thông báo</button>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import {
  Users, BookOpen, ClipboardCheck, TrendingUp,
  ArrowUpRight, AlertCircle, User, Bell
} from 'lucide-vue-next'

const stats = [
  { id: 1, label: 'Tổng sinh viên', value: '452', trend: '+12%', isNegative: false, bgColor: 'bg-blue-50', iconColor: 'text-blue-600', icon: Users },
  { id: 2, label: 'Lớp đang dạy', value: '8', trend: 'Học kỳ 2', isNegative: false, bgColor: 'bg-blue-50', iconColor: 'text-blue-600', icon: BookOpen },
  { id: 3, label: 'Bài chờ chấm', value: '24', trend: '6 bài gấp', isNegative: true, bgColor: 'bg-orange-50', iconColor: 'text-orange-600', icon: ClipboardCheck },
  { id: 4, label: 'Hiệu suất lớp', value: '82%', trend: '+5%', isNegative: false, bgColor: 'bg-green-50', iconColor: 'text-green-600', icon: TrendingUp },
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

<style scoped>
.transition-all {
  transition-duration: 300ms;
}
</style>
