<script setup>
/**
 * AttendanceReportView.vue - Super Admin
 * Báo cáo chuyên cần — Heatmap CSS, SV cảnh báo vắng,
 * tỷ lệ chuyên cần theo cơ sở. Module M18 + M5 data.
 */
import { ref, computed } from 'vue'
import {
  CalendarCheck2, AlertTriangle, Clock, Filter, RotateCcw,
  ArrowUp, ArrowDown, MapPin, UserX, Info, Calendar,
  TrendingUp, Award, Zap, BarChart3, Target
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
  Array.from({ length: 16 }, () =>
    Array.from({ length: 5 }, () => {
      const base = 85 + Math.random() * 12
      return Math.min(Math.round(base * 10) / 10, 100)
    })
  )
)

// --- Heatmap Summary Stats (GitHub-style) ---
const heatmapSummary = computed(() => {
  const allCells = heatmapData.value.flat()
  const total = allCells.length
  const avg = (allCells.reduce((a, b) => a + b, 0) / total).toFixed(1)
  const highDays = allCells.filter(v => v >= 95).length
  const lowDays = allCells.filter(v => v < 85).length
  const maxVal = Math.max(...allCells)
  const minVal = Math.min(...allCells)

  // Weekly averages for mini trend bar
  const weeklyAvgs = heatmapData.value.map(week => {
    const weekAvg = week.reduce((a, b) => a + b, 0) / week.length
    return Math.round(weekAvg * 10) / 10
  })

  // Current streak of weeks with avg >= 90
  let streak = 0
  for (let i = weeklyAvgs.length - 1; i >= 0; i--) {
    if (weeklyAvgs[i] >= 90) streak++
    else break
  }

  // Day averages
  const dayAvgs = dayNames.map((_, dIdx) => {
    const dayVals = heatmapData.value.map(week => week[dIdx])
    return (dayVals.reduce((a, b) => a + b, 0) / dayVals.length).toFixed(1)
  })

  return { total, avg, highDays, lowDays, maxVal, minVal, weeklyAvgs, streak, dayAvgs }
})

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

// --- Tooltip & Analysis ---
const hoveredCell = ref(null)
const hoverInfo = ref('')
const hoveredData = ref(null)
const selectedCell = ref(null)

const activeData = computed(() => selectedCell.value || hoveredData.value)

const selectCell = (week, day, val) => {
  // If clicked the already selected cell, deselect it
  if (selectedCell.value && selectedCell.value.week === week + 1 && selectedCell.value.day === dayNames[day]) {
    selectedCell.value = null
    return
  }

  const totalStudents = 1250
  const attended = Math.round(totalStudents * val / 100)
  const absent = totalStudents - attended
  let status = 'Tốt'
  let statusColor = 'text-emerald-500 bg-emerald-500/10 border border-emerald-500/20'
  if (val < 85) {
    status = 'Cảnh báo vắng nhiều'
    statusColor = 'text-rose-500 bg-rose-500/10 border border-rose-500/20'
  } else if (val < 90) {
    status = 'Trung bình'
    statusColor = 'text-amber-500 bg-amber-500/10 border border-amber-500/20'
  }

  selectedCell.value = {
    week: week + 1,
    day: dayNames[day],
    rate: val,
    total: totalStudents,
    attended,
    absent,
    status,
    statusColor
  }
}

const showHeatTooltip = (week, day, val) => {
  hoveredCell.value = `${week}-${day}`
  hoverInfo.value = `Tuần ${week + 1}, ${dayNames[day]}: ${val}% đi học`
  
  const totalStudents = 1250
  const attended = Math.round(totalStudents * val / 100)
  const absent = totalStudents - attended
  let status = 'Tốt'
  let statusColor = 'text-emerald-500 bg-emerald-500/10 border border-emerald-500/20'
  if (val < 85) {
    status = 'Cảnh báo vắng nhiều'
    statusColor = 'text-rose-500 bg-rose-500/10 border border-rose-500/20'
  } else if (val < 90) {
    status = 'Trung bình'
    statusColor = 'text-amber-500 bg-amber-500/10 border border-amber-500/20'
  }
  
  hoveredData.value = {
    week: week + 1,
    day: dayNames[day],
    rate: val,
    total: totalStudents,
    attended,
    absent,
    status,
    statusColor
  }
}

const hideHeatTooltip = () => {
  hoveredCell.value = null
  hoveredData.value = null
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

      <!-- Heatmap & Analysis Section -->
      <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
        <!-- Heatmap Grid (Col span 8) -->
        <div class="lg:col-span-8 lg-glass-soft lg-card p-5 flex flex-col justify-between lg:h-[530px]">
          <div>
            <h3 class="text-base font-semibold text-heading mb-1 flex items-center gap-2">
              <component :is="Calendar" :size="18" class="text-primary" /> Heatmap Chuyên Cần (16 tuần)
            </h3>
            <p class="text-xs text-muted mb-3 flex items-center gap-1">
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
            <div class="overflow-x-auto pb-2">
              <div class="inline-block min-w-full pt-7">
                <!-- Header: Weeks 1 to 16 -->
                <div class="flex items-center justify-between w-full mb-2">
                  <div class="w-16 shrink-0 text-[11px] text-label font-semibold">Ngày / Tuần</div>
                  <div class="flex-1 flex items-center justify-between">
                    <div v-for="w in 16" :key="w" class="flex-1 text-center text-[10px] text-muted font-semibold">T{{ w }}</div>
                  </div>
                </div>
                <!-- Rows: Monday to Friday -->
                <div v-for="(dayName, dIdx) in dayNames" :key="dayName" class="flex items-center justify-between w-full mb-2">
                  <div class="w-16 shrink-0 text-xs font-semibold text-heading">{{ dayName }}</div>
                  <div class="flex-1 flex items-center justify-between">
                    <div v-for="wIdx in 16" :key="wIdx" class="flex-1 flex justify-center">
                      <div
                        class="w-8 h-8 rounded-md cursor-pointer transition-all duration-200 hover:scale-110 hover:ring-2 hover:ring-primary hover:z-30 relative group"
                        :class="[
                          getHeatColor(heatmapData[wIdx - 1][dIdx]), 
                          getHeatOpacity(heatmapData[wIdx - 1][dIdx]),
                          selectedCell && selectedCell.week === wIdx && selectedCell.day === dayNames[dIdx] ? 'ring-2 ring-primary scale-110 shadow-lg border border-heading z-10 animate-selected-cell' : ''
                        ]"
                        @mouseenter="showHeatTooltip(wIdx - 1, dIdx, heatmapData[wIdx - 1][dIdx])"
                        @mouseleave="hideHeatTooltip()"
                        @click="selectCell(wIdx - 1, dIdx, heatmapData[wIdx - 1][dIdx])"
                      >
                        <!-- Tooltip -->
                        <div
                          v-if="hoveredCell === `${wIdx - 1}-${dIdx}`"
                          class="absolute bottom-full mb-2 px-2 py-1 rounded-lg bg-slate-900 text-white text-[10px] whitespace-nowrap z-50 shadow-lg pointer-events-none"
                          :class="[
                            wIdx <= 2 ? 'left-0' : '',
                            wIdx >= 15 ? 'right-0' : '',
                            wIdx > 2 && wIdx < 15 ? 'left-1/2 -translate-x-1/2' : ''
                          ]"
                        >
                          {{ hoverInfo }}
                          <div 
                            class="absolute top-full w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent border-t-slate-900"
                            :class="[
                              wIdx <= 2 ? 'left-3' : '',
                              wIdx >= 15 ? 'right-3' : '',
                              wIdx > 2 && wIdx < 15 ? 'left-1/2 -translate-x-1/2' : ''
                            ]"
                          ></div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- GitHub-style Contribution Summary -->
          <div class="mt-4 pt-4 border-t border-default">
            <!-- Quick Stats Row -->
            <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mb-4">
              <div class="flex items-center gap-2 p-2 rounded-lg bg-emerald-500/5 dark:bg-emerald-500/10">
                <component :is="Target" :size="14" class="text-emerald-500 shrink-0" />
                <div>
                  <p class="text-[11px] text-muted leading-tight">TB chung</p>
                  <p class="text-sm font-bold text-emerald-600 dark:text-emerald-400">{{ heatmapSummary.avg }}%</p>
                </div>
              </div>
              <div class="flex items-center gap-2 p-2 rounded-lg bg-cyan-500/5 dark:bg-cyan-500/10">
                <component :is="Zap" :size="14" class="text-cyan-500 shrink-0" />
                <div>
                  <p class="text-[11px] text-muted leading-tight">Ngày cao (≥95%)</p>
                  <p class="text-sm font-bold text-cyan-600 dark:text-cyan-400">{{ heatmapSummary.highDays }}/{{ heatmapSummary.total }}</p>
                </div>
              </div>
              <div class="flex items-center gap-2 p-2 rounded-lg bg-rose-500/5 dark:bg-rose-500/10">
                <component :is="AlertTriangle" :size="14" class="text-rose-500 shrink-0" />
                <div>
                  <p class="text-[11px] text-muted leading-tight">Ngày thấp (&lt; 85%)</p>
                  <p class="text-sm font-bold text-rose-600 dark:text-rose-400">{{ heatmapSummary.lowDays }}</p>
                </div>
              </div>
              <div class="flex items-center gap-2 p-2 rounded-lg bg-violet-500/5 dark:bg-violet-500/10">
                <component :is="Award" :size="14" class="text-violet-500 shrink-0" />
                <div>
                  <p class="text-[11px] text-muted leading-tight">Streak ≥90%</p>
                  <p class="text-sm font-bold text-violet-600 dark:text-violet-400">{{ heatmapSummary.streak }} tuần</p>
                </div>
              </div>
            </div>

            <!-- Weekly Trend Mini Bar -->
            <div>
              <p class="text-[11px] text-muted mb-2 flex items-center gap-1">
                <component :is="TrendingUp" :size="12" /> Xu hướng theo tuần
              </p>
              <div class="flex items-end gap-1 h-10">
                <div
                  v-for="(avg, idx) in heatmapSummary.weeklyAvgs"
                  :key="idx"
                  class="flex-1 rounded-t-sm transition-all duration-300 cursor-pointer hover:opacity-80 relative group"
                  :class="avg >= 93 ? 'bg-emerald-500' : avg >= 90 ? 'bg-emerald-400' : avg >= 87 ? 'bg-cyan-400' : avg >= 84 ? 'bg-amber-400' : 'bg-rose-400'"
                  :style="{ height: Math.max((avg - 80) * 2, 4) + 'px' }"
                >
                  <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1 px-1.5 py-0.5 rounded bg-slate-900 text-white text-[9px] whitespace-nowrap z-50 shadow-lg pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity">
                    T{{ idx + 1 }}: {{ avg }}%
                  </div>
                </div>
              </div>
              <div class="flex justify-between text-[9px] text-muted mt-1">
                <span>T1</span>
                <span>T8</span>
                <span>T16</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Detail Insights Column (Col span 4) -->
        <div class="lg:col-span-4 lg-glass-soft lg-card p-4 flex flex-col lg:h-[530px]">
          <!-- Card Header (Fixed) -->
          <div class="shrink-0 pb-2 border-b border-default mb-3">
            <h3 class="text-sm font-semibold text-heading flex items-center gap-2">
              <component :is="BarChart3" :size="16" class="text-primary" /> Phân Tích Chuyên Cần
            </h3>
          </div>

          <!-- Card Body (Scrollable) -->
          <div class="flex-1 overflow-y-auto pr-1 space-y-4 lg-scrollbar my-1">
            <!-- Hover / Clicked Data Active -->
            <div v-if="activeData" class="space-y-4 animate-fade-in">
              <div>
                <div class="flex items-center justify-between mb-2 px-2 py-1.5 rounded-lg bg-primary/5">
                  <span class="text-[11px] text-muted">{{ selectedCell ? 'Đang khóa' : 'Đang xem' }}</span>
                  <div class="flex items-center gap-1.5">
                    <span class="text-xs font-bold text-primary">Tuần {{ activeData.week }} — {{ activeData.day }}</span>
                    <button v-if="selectedCell" @click="selectedCell = null" class="text-[10px] text-rose-500 hover:text-rose-600 font-semibold ml-1.5 underline cursor-pointer select-none">
                      Hủy khóa
                    </button>
                  </div>
                </div>

                <!-- Indicators Grid -->
                <div class="grid grid-cols-2 gap-3 my-2">
                  <!-- Circular progress -->
                  <div class="flex items-center gap-2 p-2 rounded-xl bg-black/[0.02] dark:bg-white/[0.03] border border-default">
                    <div class="relative w-12 h-12 shrink-0 flex items-center justify-center">
                      <svg class="w-12 h-12 transform -rotate-90">
                        <circle cx="24" cy="24" r="20" stroke="currentColor" stroke-width="3.5" class="text-black/5 dark:text-white/5" fill="transparent" />
                        <circle :key="activeData.week + '-' + activeData.day + '-' + activeData.rate" cx="24" cy="24" r="20" stroke="currentColor" stroke-width="3.5" :class="activeData.rate >= 90 ? 'text-emerald-500' : activeData.rate >= 80 ? 'text-amber-500' : 'text-rose-500'" fill="transparent" :stroke-dasharray="125.6" :stroke-dashoffset="125.6 - (125.6 * activeData.rate) / 100" class="transition-all duration-300 animate-circle-small" />
                      </svg>
                      <span class="absolute text-[10px] font-bold text-heading">{{ activeData.rate }}%</span>
                    </div>
                    <div class="flex-1 min-w-0">
                      <span class="text-[10px] text-muted block truncate font-medium">Tỷ lệ đi học</span>
                      <span class="text-[9px] font-semibold px-1 py-0.5 rounded inline-block mt-0.5" :class="activeData.statusColor">
                        {{ activeData.status }}
                      </span>
                    </div>
                  </div>

                  <!-- Absentee details -->
                  <div class="flex items-center gap-2 p-2 rounded-xl bg-black/[0.02] dark:bg-white/[0.03] border border-default">
                    <div class="relative w-12 h-12 shrink-0 flex items-center justify-center text-rose-500 bg-rose-500/10 rounded-full">
                      <component :is="UserX" :size="20" />
                    </div>
                    <div class="flex-1 min-w-0">
                      <span class="text-[10px] text-muted block truncate font-medium">Vắng mặt</span>
                      <span class="text-[11px] font-bold text-rose-500 block leading-none">{{ activeData.absent }} SV</span>
                      <span class="text-[9px] text-muted block mt-0.5">Có mặt: {{ activeData.attended }}</span>
                    </div>
                  </div>
                </div>

                <!-- General metrics -->
                <div class="space-y-1.5 border-t border-default pt-2 text-[11px]">
                  <div class="flex justify-between items-center">
                    <span class="text-muted">Sĩ số dự kiến</span>
                    <span class="font-bold text-heading">{{ activeData.total.toLocaleString() }} SV</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted">Có mặt</span>
                    <span class="font-semibold text-emerald-500">{{ activeData.attended.toLocaleString() }} SV</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted">Vắng mặt</span>
                    <span class="font-semibold text-rose-500">{{ activeData.absent.toLocaleString() }} SV</span>
                  </div>
                </div>

                <!-- Attendance distribution bar -->
                <div class="mt-2 pt-2 border-t border-default">
                  <div class="flex items-center gap-1.5 text-[10px] text-muted mb-1">
                    <span>Phân bổ chuyên cần</span>
                  </div>
                  <div :key="activeData.week + '-' + activeData.day + '-' + activeData.rate" class="flex h-3 rounded-full overflow-hidden bg-black/5 dark:bg-white/5">
                    <div class="bg-emerald-500 transition-all duration-300 rounded-l-full animate-progress" :style="{ width: activeData.rate + '%' }"></div>
                    <div class="bg-rose-400 transition-all duration-300 rounded-r-full animate-progress" :style="{ width: (100 - activeData.rate) + '%' }"></div>
                  </div>
                  <div class="flex justify-between text-[9px] text-muted mt-0.5">
                    <span class="text-emerald-500">Có mặt {{ activeData.rate }}%</span>
                    <span class="text-rose-400">Vắng {{ (100 - activeData.rate).toFixed(1) }}%</span>
                  </div>
                </div>
              </div>

              <!-- Detailed Slot Info -->
              <div class="border-t border-default pt-3">
                <p class="text-[11px] font-semibold text-heading mb-2 flex items-center gap-1.5">
                  <component :is="Info" :size="13" class="text-primary" /> Chi tiết theo ca học (Slot)
                </p>
                <div :key="activeData.week + '-' + activeData.day" class="space-y-2">
                  <div class="p-2 rounded-lg bg-black/[0.02] dark:bg-white/[0.03] border border-default flex flex-col gap-1 text-[10px]">
                    <div class="flex justify-between">
                      <span class="text-muted font-medium">Ca học Sáng (Slot 1-3)</span>
                      <span class="font-semibold text-heading">92.8% đi học</span>
                    </div>
                    <div class="w-full bg-black/5 dark:bg-white/5 h-1.5 rounded-full overflow-hidden">
                      <div class="bg-emerald-500 h-full rounded-full animate-progress" :style="{ width: '92.8%' }"></div>
                    </div>
                  </div>
                  <div class="p-2 rounded-lg bg-black/[0.02] dark:bg-white/[0.03] border border-default flex flex-col gap-1 text-[10px]">
                    <div class="flex justify-between">
                      <span class="text-muted font-medium">Ca học Chiều (Slot 4-6)</span>
                      <span class="font-semibold text-heading">89.6% đi học</span>
                    </div>
                    <div class="w-full bg-black/5 dark:bg-white/5 h-1.5 rounded-full overflow-hidden">
                      <div class="bg-amber-500 h-full rounded-full animate-progress" :style="{ width: '89.6%' }"></div>
                    </div>
                  </div>
                  <div class="p-2 rounded-lg bg-primary/5 border border-primary/10 flex items-start gap-2">
                    <div class="w-1.5 h-1.5 rounded-full bg-primary mt-1 shrink-0"></div>
                    <p class="text-[10px] leading-relaxed text-body text-muted">
                      Học viên vắng chủ yếu tập trung vào các lớp thuộc khối ngành CNTT chuyên ngành Kỹ thuật phần mềm.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Default Stats (when not hovered) -->
            <div v-else class="space-y-4 animate-fade-in">
              <div>
                <div class="flex items-center justify-between mb-2 px-2 py-1.5 rounded-lg bg-primary/5">
                  <span class="text-[11px] text-muted">Phạm vi</span>
                  <span class="text-xs font-bold text-primary">Spring 2026</span>
                </div>

                <!-- Indicators Grid -->
                <div class="grid grid-cols-2 gap-3 my-2">
                  <!-- Circular progress -->
                  <div class="flex items-center gap-2 p-2 rounded-xl bg-black/[0.02] dark:bg-white/[0.03] border border-default">
                    <div class="relative w-12 h-12 shrink-0 flex items-center justify-center">
                      <svg class="w-12 h-12 transform -rotate-90">
                        <circle cx="24" cy="24" r="20" stroke="currentColor" stroke-width="3.5" class="text-black/5 dark:text-white/5" fill="transparent" />
                        <circle :key="filterSemester + '-' + filterCampus" cx="24" cy="24" r="20" stroke="currentColor" stroke-width="3.5" class="text-emerald-500 transition-all duration-300 animate-circle-small" fill="transparent" :stroke-dasharray="125.6" :stroke-dashoffset="125.6 - (125.6 * 91.2) / 100" />
                      </svg>
                      <span class="absolute text-[10px] font-bold text-heading">91.2%</span>
                    </div>
                    <div class="flex-1 min-w-0">
                      <span class="text-[10px] text-muted block truncate font-medium">Trung bình kỳ</span>
                      <span class="text-[9px] font-bold text-emerald-500 bg-emerald-500/10 px-1 py-0.5 rounded inline-block mt-0.5">
                        +1.5%
                      </span>
                    </div>
                  </div>

                  <!-- Semester Target progress -->
                  <div class="flex items-center gap-2 p-2 rounded-xl bg-black/[0.02] dark:bg-white/[0.03] border border-default">
                    <div class="relative w-12 h-12 shrink-0 flex items-center justify-center text-primary bg-primary/10 rounded-full">
                      <component :is="Target" :size="20" />
                    </div>
                    <div class="flex-1 min-w-0">
                      <span class="text-[10px] text-muted block truncate font-medium">Mục tiêu năm</span>
                      <span class="text-[11px] font-bold text-heading block leading-none">95.0%</span>
                      <span class="text-[9px] text-muted block mt-0.5">Còn thiếu 3.8%</span>
                    </div>
                  </div>
                </div>

                <!-- Day of week distribution -->
                <div class="border-t border-default pt-2 mt-1">
                  <p class="text-[11px] text-muted mb-1.5">Phân bổ theo thứ</p>
                  <div class="space-y-1">
                    <div v-for="(dayName, dIdx) in dayNames" :key="dayName" class="flex items-center gap-2">
                      <span class="text-[10px] font-semibold text-heading w-5">{{ dayName }}</span>
                      <div class="flex-1 h-2 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                        <div
                          :key="filterSemester + '-' + filterCampus + '-' + Number(heatmapSummary.dayAvgs[dIdx])"
                          class="h-full rounded-full transition-all duration-300 animate-progress"
                          :class="Number(heatmapSummary.dayAvgs[dIdx]) >= 92 ? 'bg-emerald-500' : Number(heatmapSummary.dayAvgs[dIdx]) >= 89 ? 'bg-cyan-400' : 'bg-amber-400'"
                          :style="{ width: ((Number(heatmapSummary.dayAvgs[dIdx]) - 80) * 5) + '%' }"
                        ></div>
                      </div>
                      <span class="text-[10px] font-bold w-10 text-right" :class="Number(heatmapSummary.dayAvgs[dIdx]) >= 92 ? 'text-emerald-500' : Number(heatmapSummary.dayAvgs[dIdx]) >= 89 ? 'text-cyan-500' : 'text-amber-500'">{{ heatmapSummary.dayAvgs[dIdx] }}%</span>
                    </div>
                  </div>
                </div>

                <!-- Key metrics -->
                <div class="space-y-1.5 border-t border-default pt-2 mt-2 text-[11px]">
                  <div class="flex justify-between items-center">
                    <span class="text-muted flex items-center gap-1"><component :is="ArrowUp" :size="10" class="text-emerald-500" /> Ngày tốt nhất</span>
                    <span class="font-bold text-emerald-500">Thứ 3 (93.4%)</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted flex items-center gap-1"><component :is="Award" :size="10" class="text-cyan-500" /> Tuần tốt nhất</span>
                    <span class="font-bold text-heading">Tuần 10 (96.8%)</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted flex items-center gap-1"><component :is="ArrowDown" :size="10" class="text-rose-500" /> Campus yếu nhất</span>
                    <span class="font-bold text-rose-500">Đà Nẵng (86.1%)</span>
                  </div>
                  <div class="flex justify-between items-center">
                    <span class="text-muted flex items-center gap-1"><component :is="TrendingUp" :size="10" class="text-violet-500" /> Biên độ</span>
                    <span class="font-bold text-heading">{{ heatmapSummary.minVal }}% — {{ heatmapSummary.maxVal }}%</span>
                  </div>
                </div>
              </div>

              <!-- Recommendations & Suggestions -->
              <div class="border-t border-default pt-3">
                <p class="text-[11px] font-semibold text-heading mb-2 flex items-center gap-1.5">
                  <component :is="AlertTriangle" :size="13" class="text-amber-500" /> Cảnh báo & Đề xuất học vụ
                </p>
                <div class="space-y-2">
                  <div class="p-2 rounded-lg bg-rose-500/5 dark:bg-rose-500/10 border border-rose-500/10 flex items-start gap-2">
                    <div class="w-1.5 h-1.5 rounded-full bg-rose-500 mt-1 shrink-0"></div>
                    <p class="text-[10px] leading-relaxed text-body text-muted">
                      <span class="font-bold text-rose-500 dark:text-rose-400">Đà Nẵng</span> có tỷ lệ chuyên cần (86.1%) thấp hơn mức sàn 90%. Đề xuất thanh tra chất lượng điểm danh.
                    </p>
                  </div>
                  <div class="p-2 rounded-lg bg-amber-500/5 dark:bg-amber-500/10 border border-amber-500/10 flex items-start gap-2">
                    <div class="w-1.5 h-1.5 rounded-full bg-amber-500 mt-1 shrink-0"></div>
                    <p class="text-[10px] leading-relaxed text-body text-muted">
                      <span class="font-bold text-amber-500 dark:text-amber-400">45 Sinh viên</span> sắp vượt quá 20% tổng quỹ vắng cho phép. Đề xuất gửi thông báo SMS tự động.
                    </p>
                  </div>
                  <div class="p-2 rounded-lg bg-primary/5 border border-primary/10 flex items-start gap-2">
                    <div class="w-1.5 h-1.5 rounded-full bg-primary mt-1 shrink-0"></div>
                    <p class="text-[10px] leading-relaxed text-body text-muted">
                      Chuyên cần giảm trung bình <span class="font-bold text-primary">2.4%</span> vào Thứ 6. Khuyên tăng tần suất điểm danh đột xuất.
                  </p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Card Footer (Fixed) -->
          <div class="shrink-0 mt-2 pt-2 border-t border-default text-[10px] text-center text-muted select-none">
            💡 Hover ô heatmap để xem chi tiết
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
                      <div :key="filterSemester + '-' + filterCampus + '-' + c.rate" class="h-full rounded-full transition-all duration-500 animate-progress" :class="c.rate >= 90 ? 'bg-emerald-500' : c.rate >= 85 ? 'bg-amber-500' : 'bg-rose-500'" :style="{ width: c.rate + '%' }"></div>
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
                      <div :key="filterSemester + '-' + filterCampus + '-' + s.pctUsed" class="h-full rounded-full transition-all animate-progress" :class="s.pctUsed >= 100 ? 'bg-rose-500' : s.pctUsed >= 80 ? 'bg-orange-500' : 'bg-amber-500'" :style="{ width: Math.min(s.pctUsed, 100) + '%' }"></div>
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

<style scoped>
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fadeIn 0.28s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes pulseSelected {
  0%, 100% { transform: scale(1.1); filter: brightness(1.05); }
  50% { transform: scale(1.16); filter: brightness(1.18); }
}
.animate-selected-cell {
  animation: pulseSelected 2s infinite ease-in-out;
}

/* Progress bar growing animation */
@keyframes growProgress {
  from { width: 0; }
}
.animate-progress {
  animation: growProgress 1s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

/* Circle charts filling animation */
@keyframes circleFillSmall {
  from { stroke-dashoffset: 125.6; }
}
.animate-circle-small {
  animation: circleFillSmall 1.2s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

/* Custom premium scrollbar for entire side panel */
.lg-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.lg-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.lg-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.08);
  border-radius: 4px;
}
.dark .lg-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.08);
}
.lg-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.16);
}
.dark .lg-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.16);
}
</style>
