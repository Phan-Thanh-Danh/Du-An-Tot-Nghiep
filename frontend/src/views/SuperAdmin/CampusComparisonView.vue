<script setup>
/**
 * CampusComparisonView.vue - Super Admin
 * So sánh hiệu năng giữa các campus — bảng tổng hợp, grouped bar chart CSS,
 * ranking huy chương. Module M18 section 6.1 "So Sánh Campus".
 */
import { ref, computed } from 'vue'
import {
  GitCompare, Award, Filter, RotateCcw, Trophy,
  Users, GraduationCap, CalendarCheck2, DollarSign, Star,
  CheckCircle, ArrowUp, ArrowDown, X, Check
} from 'lucide-vue-next'

// --- Filters ---
const semesters = ref(['Spring 2026', 'Fall 2025', 'Summer 2025'])
const filterSemester = ref('Spring 2026')

// --- Campus Selection ---
const allCampuses = ref([
  { id: 'hanoi', name: 'Hà Nội', selected: true },
  { id: 'hoalac', name: 'Hòa Lạc', selected: true },
  { id: 'hcm', name: 'TP.HCM', selected: true },
  { id: 'danang', name: 'Đà Nẵng', selected: true },
])

const selectedCampuses = computed(() => allCampuses.value.filter(c => c.selected))

const toggleCampus = (campus) => {
  if (campus.selected && selectedCampuses.value.length <= 2) return
  campus.selected = !campus.selected
}

// --- Campus Data ---
const campusData = ref({
  hanoi: { name: 'Hà Nội', students: 3200, gpa: 7.45, passRate: 85.2, attendanceRate: 92.5, revenue: 48.5, teacherScore: 4.2, color: 'bg-blue-500', textColor: 'text-blue-500', lightBg: 'bg-blue-500/20' },
  hoalac: { name: 'Hòa Lạc', students: 4500, gpa: 7.32, passRate: 83.1, attendanceRate: 92.0, revenue: 67.8, teacherScore: 4.0, color: 'bg-violet-500', textColor: 'text-violet-500', lightBg: 'bg-violet-500/20' },
  hcm: { name: 'TP.HCM', students: 3100, gpa: 7.18, passRate: 80.5, attendanceRate: 91.0, revenue: 46.2, teacherScore: 3.9, color: 'bg-cyan-500', textColor: 'text-cyan-500', lightBg: 'bg-cyan-500/20' },
  danang: { name: 'Đà Nẵng', students: 1200, gpa: 7.05, passRate: 78.2, attendanceRate: 86.1, revenue: 17.5, teacherScore: 3.7, color: 'bg-amber-500', textColor: 'text-amber-500', lightBg: 'bg-amber-500/20' },
})

const selectedData = computed(() => {
  return selectedCampuses.value.map(c => ({
    ...campusData.value[c.id],
    id: c.id,
  }))
})

// --- Comparison Metrics ---
const metrics = ref([
  { key: 'students', label: 'Tổng sinh viên', format: 'number', icon: Users },
  { key: 'gpa', label: 'GPA trung bình', format: 'decimal', icon: GraduationCap },
  { key: 'passRate', label: 'Tỷ lệ Pass (%)', format: 'pct', icon: CheckCircle },
  { key: 'attendanceRate', label: 'Chuyên cần (%)', format: 'pct', icon: CalendarCheck2 },
  { key: 'revenue', label: 'Doanh thu (tỷ VNĐ)', format: 'currency', icon: DollarSign },
  { key: 'teacherScore', label: 'Điểm đánh giá GV', format: 'decimal', icon: Star },
])

const formatValue = (val, format) => {
  switch (format) {
    case 'number': return val.toLocaleString()
    case 'decimal': return val.toFixed(2)
    case 'pct': return val.toFixed(1) + '%'
    case 'currency': return val.toFixed(1)
    default: return val
  }
}

const getBestWorst = (metricKey) => {
  const data = selectedData.value
  if (data.length === 0) return { bestId: null, worstId: null }
  let best = data[0], worst = data[0]
  data.forEach(d => {
    if (d[metricKey] > best[metricKey]) best = d
    if (d[metricKey] < worst[metricKey]) worst = d
  })
  return { bestId: best.id, worstId: data.length > 1 ? worst.id : null }
}

// --- Bar Chart Helpers ---
const getBarWidth = (val, metricKey) => {
  const maxVal = Math.max(...selectedData.value.map(d => d[metricKey]))
  return maxVal === 0 ? 0 : (val / maxVal) * 100
}

// --- Rankings ---
const rankingMetrics = ['gpa', 'passRate', 'attendanceRate', 'teacherScore']
const rankingLabels = { gpa: 'GPA TB', passRate: 'Tỷ lệ Pass', attendanceRate: 'Chuyên cần', teacherScore: 'Điểm GV' }

const getRanking = (metricKey) => {
  return [...selectedData.value].sort((a, b) => b[metricKey] - a[metricKey])
}

const getMedal = (idx) => {
  switch (idx) {
    case 0: return '🥇'
    case 1: return '🥈'
    case 2: return '🥉'
    default: return `#${idx + 1}`
  }
}

const resetFilters = () => {
  filterSemester.value = 'Spring 2026'
  allCampuses.value.forEach(c => c.selected = true)
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <div class="lg-shell-content space-y-6">
      <!-- Campus Selector + Filters -->
      <div class="lg-glass-soft lg-card p-4 space-y-3">
        <div class="flex flex-wrap items-center gap-3">
          <component :is="Filter" :size="18" class="text-muted" />
          <select v-model="filterSemester" class="lg-control text-sm min-w-[150px]">
            <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
          </select>
          <button @click="resetFilters" class="lg-btn-secondary text-xs flex items-center gap-1 px-3 py-1.5">
            <component :is="RotateCcw" :size="14" /> Đặt lại
          </button>
        </div>
        <div class="flex flex-wrap items-center gap-2">
          <span class="text-xs text-muted mr-1">Chọn campus:</span>
          <button
            v-for="campus in allCampuses"
            :key="campus.id"
            @click="toggleCampus(campus)"
            class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-medium border transition-all duration-200"
            :class="campus.selected
              ? `${campusData[campus.id].color} text-white border-transparent shadow-sm`
              : 'bg-black/5 dark:bg-white/5 text-muted border-default hover:border-slate-300'"
          >
            <component :is="campus.selected ? Check : X" :size="12" />
            {{ campus.name }}
          </button>
          <span class="text-[10px] text-muted ml-1">(chọn tối thiểu 2)</span>
        </div>
      </div>

      <!-- Comparison Table -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="GitCompare" :size="18" class="text-primary" /> Bảng So Sánh Tổng Hợp
        </h3>
        <div class="lg-table-shell lg-density-normal overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[180px]">Chỉ số</th>
                <th
                  v-for="c in selectedData"
                  :key="c.id"
                  class="text-center text-label font-medium px-3 py-2.5 border-b border-default min-w-[130px]"
                >
                  <span class="inline-flex items-center gap-1.5">
                    <span class="w-2.5 h-2.5 rounded-full" :class="c.color"></span>
                    {{ c.name }}
                  </span>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="m in metrics" :key="m.key" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-3 text-body flex items-center gap-2">
                  <component :is="m.icon" :size="15" class="text-muted shrink-0" />
                  <span class="font-medium">{{ m.label }}</span>
                </td>
                <td
                  v-for="c in selectedData"
                  :key="c.id + m.key"
                  class="px-3 py-3 text-center font-semibold"
                  :class="{
                    'bg-emerald-500/5 text-emerald-600 dark:text-emerald-400': getBestWorst(m.key).bestId === c.id && selectedData.length > 1,
                    'bg-rose-500/5 text-rose-500': getBestWorst(m.key).worstId === c.id && selectedData.length > 1,
                    'text-heading': getBestWorst(m.key).bestId !== c.id && getBestWorst(m.key).worstId !== c.id,
                  }"
                >
                  {{ formatValue(c[m.key], m.format) }}
                  <span v-if="getBestWorst(m.key).bestId === c.id && selectedData.length > 1" class="text-[10px] ml-0.5">⬆</span>
                  <span v-if="getBestWorst(m.key).worstId === c.id && selectedData.length > 1" class="text-[10px] ml-0.5">⬇</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Grouped Bar Charts -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div v-for="m in metrics.slice(1)" :key="'chart-' + m.key" class="lg-glass-soft lg-card p-5">
          <h3 class="text-sm font-semibold text-heading mb-4 flex items-center gap-2">
            <component :is="m.icon" :size="16" class="text-primary" /> {{ m.label }}
          </h3>
          <div class="space-y-2.5">
            <div v-for="c in selectedData" :key="c.id + m.key" class="flex items-center gap-3">
              <span class="text-xs text-label w-[70px] shrink-0 truncate">{{ c.name }}</span>
              <div class="flex-1 h-6 rounded-lg bg-black/5 dark:bg-white/5 overflow-hidden relative">
                <div
                  class="h-full rounded-lg transition-all duration-700 ease-out flex items-center pl-2"
                  :class="c.color"
                  :style="{ width: getBarWidth(c[m.key], m.key) + '%' }"
                >
                  <span class="text-[10px] font-bold text-white drop-shadow" v-if="getBarWidth(c[m.key], m.key) > 15">
                    {{ formatValue(c[m.key], m.format) }}
                  </span>
                </div>
                <span v-if="getBarWidth(c[m.key], m.key) <= 15" class="absolute left-2 top-1/2 -translate-y-1/2 text-[10px] font-bold text-heading">
                  {{ formatValue(c[m.key], m.format) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Rankings -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="Trophy" :size="18" class="text-amber-500" /> Bảng Xếp Hạng Campus
        </h3>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <div v-for="mk in rankingMetrics" :key="mk" class="lg-glass-soft rounded-xl p-4">
            <h4 class="text-xs text-muted font-medium uppercase tracking-wider mb-3 text-center">{{ rankingLabels[mk] }}</h4>
            <div class="space-y-2">
              <div
                v-for="(c, idx) in getRanking(mk)"
                :key="c.id"
                class="flex items-center gap-2 px-2 py-1.5 rounded-lg transition-all"
                :class="idx === 0 ? 'bg-amber-500/10' : ''"
              >
                <span class="text-base w-6 text-center">{{ getMedal(idx) }}</span>
                <span class="w-2.5 h-2.5 rounded-full shrink-0" :class="c.color"></span>
                <span class="text-xs font-medium text-heading flex-1">{{ c.name }}</span>
                <span class="text-xs font-bold" :class="c.textColor">
                  {{ mk === 'students' ? c[mk].toLocaleString() : mk.includes('Rate') ? c[mk].toFixed(1) + '%' : c[mk].toFixed(2) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Data Timestamp -->
      <div class="text-center">
        <p class="text-xs text-muted">
          Dữ liệu snapshot: <span class="font-semibold">12/06/2026 02:00</span> — Cập nhật mỗi đêm bởi Hangfire CRON
        </p>
      </div>
    </div>
  </div>
</template>
