<script setup>
/**
 * AttendanceReportView.vue - Super Admin
 * Báo cáo chuyên cần — Heatmap CSS, SV cảnh báo vắng,
 * tỷ lệ chuyên cần theo cơ sở. Module M18 + M5 data.
 */
import { ref, computed } from 'vue'
import {
  CalendarCheck2, AlertTriangle, Users, Clock, Filter, RotateCcw,
  ArrowUp, ArrowDown, MapPin, UserX, Eye, Info, Calendar
} from 'lucide-vue-next'

// --- Filters ---
const semesters = ref(['Spring 2026', 'Fall 2025', 'Summer 2025'])
const campuses = ref(['Tất cả', 'Hà Nội', 'Hòa Lạc', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ'])
const weeks = ref(['Tất cả', ...Array.from({ length: 16 }, (_, i) => `Tuần ${i + 1}`)])

const filterSemester = ref('Spring 2026')
const filterCampus = ref('Tất cả')
const filterWeek = ref('Tất cả')

const resetFilters = () => {
  filterSemester.value = 'Spring 2026'
  filterCampus.value = 'Tất cả'
  filterWeek.value = 'Tất cả'
}

// --- KPI ---
const kpiData = computed(() => ([
  { label: 'Tỷ lệ CC toàn trường', value: '91.2%', icon: CalendarCheck2, color: 'text-emerald-500', bg: 'bg-emerald-500/10', delta: '+1.5%', deltaUp: true },
  { label: 'Session chưa xác nhận', value: '23', icon: Clock, color: 'text-amber-500', bg: 'bg-amber-500/10', delta: '-5', deltaUp: true },
  { label: 'SV vượt 80% quỹ vắng', value: '45', icon: UserX, color: 'text-rose-500', bg: 'bg-rose-500/10', delta: '+8', deltaUp: false },
  { label: 'Campus vắng cao nhất', value: 'Đà Nẵng', subValue: '86.1%', icon: MapPin, color: 'text-violet-500', bg: 'bg-violet-500/10' },
]))

// --- Heatmap Data (16 weeks × 5 days = Mon-Fri) ---
const dayNames = ['T2', 'T3', 'T4', 'T5', 'T6']
const heatmapData = ref(
  Array.from({ length: 16 }, (_, weekIdx) =>
    Array.from({ length: 5 }, () => {
      const base = 85 + Math.random() * 12
      return Math.min(Math.round(base * 10) / 10, 100)
    })
  )
)

const getHeatColor = (val) => {
  if (val >= 95) return 'bg-emerald-500'
  if (val >= 90) return 'bg-emerald-400'
  if (val >= 85) return 'bg-cyan-400'
  if (val >= 80) return 'bg-amber-400'
  if (val >= 75) return 'bg-orange-400'
  return 'bg-rose-500'
}

const getHeatOpacity = (val) => {
  if (val >= 95) return 'opacity-100'
  if (val >= 90) return 'opacity-90'
  if (val >= 85) return 'opacity-80'
  return 'opacity-70'
}

// --- Tooltip ---
const hoveredCell = ref(null)
const hoverInfo = ref('')
const showHeatTooltip = (week, day, val) => {
  hoveredCell.value = `${week}-${day}`
  hoverInfo.value = `Tuần ${week + 1}, ${dayNames[day]}: ${val}% đi học`
}
const hideHeatTooltip = () => {
  hoveredCell.value = null
}

// --- Campus Table ---
const campusAttendance = ref([
  { campus: 'Hà Nội', sessions: 4200, absences: 315, rate: 92.5, delta: +1.8 },
  { campus: 'Hòa Lạc', sessions: 6100, absences: 488, rate: 92.0, delta: +1.2 },
  { campus: 'TP.HCM', sessions: 3800, absences: 342, rate: 91.0, delta: +0.5 },
  { campus: 'Cần Thơ', sessions: 900, absences: 90, rate: 90.0, delta: -0.3 },
  { campus: 'Đà Nẵng', sessions: 1600, absences: 222, rate: 86.1, delta: -2.1 },
])

// --- Warning Students ---
const warningStudents = ref([
  { id: 'HE170301', name: 'Nguyễn Quốc Bảo', campus: 'Đà Nẵng', subject: 'PRN211', absent: 7, total: 30, pctUsed: 100, level: '100%' },
  { id: 'HE170112', name: 'Lê Hoàng Long', campus: 'Đà Nẵng', subject: 'SWD392', absent: 5, total: 20, pctUsed: 100, level: '100%' },
  { id: 'HE170045', name: 'Trần Thị Bích', campus: 'Hòa Lạc', subject: 'MAD101', absent: 6, total: 30, pctUsed: 85.7, level: '80%' },
  { id: 'HE170201', name: 'Phạm Minh Tuấn', campus: 'TP.HCM', subject: 'CSD201', absent: 5, total: 28, pctUsed: 83.3, level: '80%' },
  { id: 'HE170078', name: 'Nguyễn Thị Hạnh', campus: 'Hà Nội', subject: 'OSG202', absent: 4, total: 24, pctUsed: 66.7, level: '60%' },
  { id: 'HE170155', name: 'Võ Đình Khoa', campus: 'Hòa Lạc', subject: 'PRN221', absent: 4, total: 26, pctUsed: 64.1, level: '60%' },
  { id: 'HE170310', name: 'Đỗ Thúy Ngân', campus: 'TP.HCM', subject: 'SWE201c', absent: 3, total: 20, pctUsed: 62.5, level: '60%' },
])

const getLevelBadge = (level) => {
  switch (level) {
    case '100%': return 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-200 dark:border-rose-500/20'
    case '80%': return 'bg-orange-500/10 text-orange-600 dark:text-orange-400 border border-orange-200 dark:border-orange-500/20'
    case '60%': return 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border border-amber-200 dark:border-amber-500/20'
    default: return ''
  }
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <div class="lg-shell-content space-y-6">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div v-for="kpi in kpiData" :key="kpi.label" class="lg-glass-soft lg-card p-4 flex items-start gap-4">
          <div class="p-2.5 rounded-xl shrink-0" :class="kpi.bg">
            <component :is="kpi.icon" :size="22" :class="kpi.color" />
          </div>
          <div class="flex-1">
            <div class="flex items-center justify-between">
              <p class="text-2xl font-bold text-heading leading-tight">{{ kpi.value }}</p>
              <span v-if="kpi.delta" class="text-xs font-semibold flex items-center gap-0.5 px-1.5 py-0.5 rounded-full" :class="kpi.deltaUp ? 'text-emerald-600 bg-emerald-500/10' : 'text-rose-600 bg-rose-500/10'">
                <component :is="kpi.deltaUp ? ArrowUp : ArrowDown" :size="11" />
                {{ kpi.delta }}
              </span>
            </div>
            <p v-if="kpi.subValue" class="text-xs text-muted">{{ kpi.subValue }}</p>
            <p class="text-xs text-muted mt-0.5">{{ kpi.label }}</p>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="lg-glass-soft lg-card p-4 flex flex-wrap items-center gap-3">
        <component :is="Filter" :size="18" class="text-muted" />
        <select v-model="filterSemester" class="lg-control text-sm min-w-[150px]">
          <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
        </select>
        <select v-model="filterCampus" class="lg-control text-sm min-w-[130px]">
          <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
        </select>
        <select v-model="filterWeek" class="lg-control text-sm min-w-[120px]">
          <option v-for="w in weeks" :key="w" :value="w">{{ w }}</option>
        </select>
        <button @click="resetFilters" class="lg-btn-secondary text-xs flex items-center gap-1 px-3 py-1.5">
          <component :is="RotateCcw" :size="14" /> Đặt lại
        </button>
      </div>

      <!-- Heatmap -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-1 flex items-center gap-2">
          <component :is="Calendar" :size="18" class="text-primary" /> Heatmap Chuyên Cần (16 tuần)
        </h3>
        <p class="text-xs text-muted mb-4 flex items-center gap-1">
          <component :is="Info" :size="12" /> Màu càng đậm = tỷ lệ đi học càng cao. Hover để xem chi tiết.
        </p>

        <!-- Legend -->
        <div class="flex items-center gap-3 mb-4 text-[10px] text-muted">
          <span>Thấp</span>
          <div class="flex gap-0.5">
            <div class="w-4 h-4 rounded bg-rose-500 opacity-70"></div>
            <div class="w-4 h-4 rounded bg-orange-400 opacity-80"></div>
            <div class="w-4 h-4 rounded bg-amber-400 opacity-80"></div>
            <div class="w-4 h-4 rounded bg-cyan-400 opacity-90"></div>
            <div class="w-4 h-4 rounded bg-emerald-400 opacity-90"></div>
            <div class="w-4 h-4 rounded bg-emerald-500"></div>
          </div>
          <span>Cao</span>
        </div>

        <!-- Heatmap Grid -->
        <div class="overflow-x-auto">
          <div class="inline-block">
            <!-- Header -->
            <div class="flex items-center mb-1">
              <div class="w-12 shrink-0"></div>
              <div v-for="d in dayNames" :key="d" class="w-10 text-center text-[10px] text-muted font-medium">{{ d }}</div>
            </div>
            <!-- Rows -->
            <div v-for="(week, wIdx) in heatmapData" :key="wIdx" class="flex items-center mb-1">
              <div class="w-12 shrink-0 text-[10px] text-muted font-medium">T{{ wIdx + 1 }}</div>
              <div v-for="(val, dIdx) in week" :key="dIdx" class="w-10 flex justify-center">
                <div
                  class="w-8 h-8 rounded-md cursor-pointer transition-all duration-200 hover:scale-110 hover:ring-2 hover:ring-white/50 relative group"
                  :class="[getHeatColor(val), getHeatOpacity(val)]"
                  @mouseenter="showHeatTooltip(wIdx, dIdx, val)"
                  @mouseleave="hideHeatTooltip()"
                >
                  <!-- Tooltip -->
                  <div
                    v-if="hoveredCell === `${wIdx}-${dIdx}`"
                    class="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 px-2 py-1 rounded-lg bg-slate-900 text-white text-[10px] whitespace-nowrap z-50 shadow-lg pointer-events-none"
                  >
                    {{ hoverInfo }}
                    <div class="absolute top-full left-1/2 -translate-x-1/2 w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent border-t-slate-900"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Campus Attendance Table -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="MapPin" :size="18" class="text-primary" /> Chuyên Cần Theo Cơ Sở
        </h3>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Cơ sở</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Tổng buổi</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Vắng</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[200px]">Tỷ lệ CC</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Δ Kỳ trước</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in campusAttendance" :key="c.campus" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-semibold text-heading">{{ c.campus }}</td>
                <td class="px-3 py-2.5 text-right text-muted">{{ c.sessions.toLocaleString() }}</td>
                <td class="px-3 py-2.5 text-right text-rose-500 font-semibold">{{ c.absences }}</td>
                <td class="px-3 py-2.5">
                  <div class="flex items-center gap-2">
                    <div class="flex-1 h-2.5 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                      <div class="h-full rounded-full transition-all duration-500" :class="c.rate >= 90 ? 'bg-emerald-500' : c.rate >= 85 ? 'bg-amber-500' : 'bg-rose-500'" :style="{ width: c.rate + '%' }"></div>
                    </div>
                    <span class="text-xs font-semibold w-[42px] text-right" :class="c.rate >= 90 ? 'text-emerald-600' : c.rate >= 85 ? 'text-amber-600' : 'text-rose-600'">{{ c.rate }}%</span>
                  </div>
                </td>
                <td class="px-3 py-2.5 text-right">
                  <span class="inline-flex items-center gap-0.5 text-xs font-semibold" :class="c.delta >= 0 ? 'text-emerald-500' : 'text-rose-500'">
                    <component :is="c.delta >= 0 ? ArrowUp : ArrowDown" :size="12" />
                    {{ c.delta >= 0 ? '+' : '' }}{{ c.delta.toFixed(1) }}%
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Warning Students -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="UserX" :size="18" class="text-rose-500" /> Sinh Viên Cảnh Báo Vắng
        </h3>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">MSSV</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Họ tên</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Cơ sở</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Môn</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Vắng/Tổng</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[150px]">% Quỹ vắng</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Mức</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in warningStudents" :key="s.id + s.subject" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ s.id }}</td>
                <td class="px-3 py-2.5 text-body">{{ s.name }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ s.campus }}</td>
                <td class="px-3 py-2.5 font-mono text-xs text-heading font-semibold">{{ s.subject }}</td>
                <td class="px-3 py-2.5 text-center text-heading font-semibold">{{ s.absent }}/{{ s.total }}</td>
                <td class="px-3 py-2.5">
                  <div class="flex items-center gap-2">
                    <div class="flex-1 h-2 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                      <div class="h-full rounded-full transition-all" :class="s.pctUsed >= 100 ? 'bg-rose-500' : s.pctUsed >= 80 ? 'bg-orange-500' : 'bg-amber-500'" :style="{ width: Math.min(s.pctUsed, 100) + '%' }"></div>
                    </div>
                    <span class="text-xs font-semibold w-[38px] text-right" :class="s.pctUsed >= 100 ? 'text-rose-500' : s.pctUsed >= 80 ? 'text-orange-500' : 'text-amber-500'">{{ s.pctUsed.toFixed(0) }}%</span>
                  </div>
                </td>
                <td class="px-3 py-2.5 text-center">
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold" :class="getLevelBadge(s.level)">
                    <component :is="AlertTriangle" :size="11" />
                    {{ s.level }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>
