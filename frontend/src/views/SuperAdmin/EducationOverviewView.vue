<script setup>
/**
 * EducationOverviewView.vue - Super Admin
 * Tổng quan đào tạo toàn hệ thống — KPI, phân phối điểm, xu hướng GPA,
 * môn pass/fail cao. Module M18 – Analytics & Reporting Dashboard.
 */
import { ref, computed } from 'vue'
import {
  Users, GraduationCap, BookOpen, TrendingUp, TrendingDown,
  Award, BarChart3, PieChart, Filter, RotateCcw, ArrowUp,
  ArrowDown, Minus, CheckCircle, XCircle, Activity, School
} from 'lucide-vue-next'

// --- Mock Data ---
const semesters = ref(['Spring 2026', 'Fall 2025', 'Summer 2025'])
const campuses = ref(['Tất cả', 'Hà Nội', 'Hòa Lạc', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ'])

const filterSemester = ref('Spring 2026')
const filterCampus = ref('Tất cả')

const kpiData = computed(() => ([
  { label: 'Tổng sinh viên', value: '12.450', icon: Users, color: 'text-blue-500', bg: 'bg-blue-500/10', delta: '+320', deltaUp: true },
  { label: 'Tổng giảng viên', value: '385', icon: GraduationCap, color: 'text-violet-500', bg: 'bg-violet-500/10', delta: '+12', deltaUp: true },
  { label: 'Tổng lớp học phần', value: '640', icon: BookOpen, color: 'text-cyan-500', bg: 'bg-cyan-500/10', delta: '+45', deltaUp: true },
  { label: 'GPA trung bình', value: '7.28', icon: Award, color: 'text-amber-500', bg: 'bg-amber-500/10', delta: '+0.15', deltaUp: true },
  { label: 'Tỷ lệ Pass', value: '82.5%', icon: CheckCircle, color: 'text-emerald-500', bg: 'bg-emerald-500/10', delta: '+2.3%', deltaUp: true },
  { label: 'Hoàn thành KH', value: '76.3%', icon: Activity, color: 'text-rose-500', bg: 'bg-rose-500/10', delta: '-1.2%', deltaUp: false },
]))

const gradeDistribution = ref([
  { grade: 'A (8.5-10)', count: 2180, pct: 17.5, color: 'bg-emerald-500' },
  { grade: 'B (7.0-8.4)', count: 3860, pct: 31.0, color: 'bg-cyan-500' },
  { grade: 'C (5.5-6.9)', count: 3110, pct: 25.0, color: 'bg-amber-500' },
  { grade: 'D (4.0-5.4)', count: 2050, pct: 16.5, color: 'bg-orange-500' },
  { grade: 'F (<4.0)', count: 1250, pct: 10.0, color: 'bg-rose-500' },
])

const gpaTrends = ref([
  { semester: 'Summer 2025', gpa: 7.05, passRate: 79.8, students: 11200 },
  { semester: 'Fall 2025', gpa: 7.13, passRate: 80.2, students: 11980 },
  { semester: 'Spring 2026', gpa: 7.28, passRate: 82.5, students: 12450 },
])

const topFailSubjects = ref([
  { code: 'PRN211', name: 'Lập trình C# (.NET)', enrolled: 580, failed: 165, failRate: 28.4 },
  { code: 'MAD101', name: 'Toán rời rạc', enrolled: 720, failed: 180, failRate: 25.0 },
  { code: 'CSD201', name: 'Cấu trúc dữ liệu & Giải thuật', enrolled: 640, failed: 147, failRate: 23.0 },
  { code: 'OSG202', name: 'Hệ điều hành', enrolled: 410, failed: 86, failRate: 21.0 },
  { code: 'MAS291', name: 'Xác suất thống kê', enrolled: 550, failed: 105, failRate: 19.1 },
])

const topPassSubjects = ref([
  { code: 'SWE201c', name: 'Nhập môn kỹ nghệ phần mềm', enrolled: 480, passed: 465, passRate: 96.9 },
  { code: 'SSL101c', name: 'Kỹ năng học tập', enrolled: 800, passed: 768, passRate: 96.0 },
  { code: 'JPD113', name: 'Tiếng Nhật 1', enrolled: 350, passed: 332, passRate: 94.9 },
  { code: 'CEA201', name: 'Kỹ năng giao tiếp', enrolled: 620, passed: 583, passRate: 94.0 },
  { code: 'PRF192', name: 'Lập trình cơ bản', enrolled: 900, passed: 837, passRate: 93.0 },
])

const resetFilters = () => {
  filterSemester.value = 'Spring 2026'
  filterCampus.value = 'Tất cả'
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <div class="lg-shell-content space-y-6">
      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
        <div
          v-for="kpi in kpiData"
          :key="kpi.label"
          class="lg-glass-soft lg-card p-4 flex flex-col gap-3"
        >
          <div class="flex items-center justify-between">
            <div class="p-2 rounded-xl" :class="kpi.bg">
              <component :is="kpi.icon" :size="20" :class="kpi.color" />
            </div>
            <span
              class="text-xs font-semibold flex items-center gap-0.5 px-1.5 py-0.5 rounded-full"
              :class="kpi.deltaUp ? 'text-emerald-600 bg-emerald-500/10' : 'text-rose-600 bg-rose-500/10'"
            >
              <component :is="kpi.deltaUp ? ArrowUp : ArrowDown" :size="12" />
              {{ kpi.delta }}
            </span>
          </div>
          <div>
            <p class="text-2xl font-bold text-heading">{{ kpi.value }}</p>
            <p class="text-xs text-muted mt-0.5">{{ kpi.label }}</p>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="lg-glass-soft lg-card p-4 flex flex-wrap items-center gap-3">
        <component :is="Filter" :size="18" class="text-muted" />
        <select v-model="filterSemester" class="lg-control text-sm min-w-[160px]">
          <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
        </select>
        <select v-model="filterCampus" class="lg-control text-sm min-w-[140px]">
          <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
        </select>
        <button @click="resetFilters" class="lg-btn-secondary text-xs flex items-center gap-1 px-3 py-1.5">
          <component :is="RotateCcw" :size="14" /> Đặt lại
        </button>
      </div>

      <!-- Charts Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Grade Distribution -->
        <div class="lg-glass-soft lg-card p-5">
          <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
            <component :is="BarChart3" :size="18" class="text-primary" /> Phân phối điểm toàn hệ thống
          </h3>
          <div class="space-y-3">
            <div v-for="g in gradeDistribution" :key="g.grade" class="flex items-center gap-3">
              <span class="text-xs text-label w-[90px] shrink-0 font-medium">{{ g.grade }}</span>
              <div class="flex-1 h-7 rounded-lg bg-black/5 dark:bg-white/5 overflow-hidden relative">
                <div
                  class="h-full rounded-lg transition-all duration-700 ease-out flex items-center pl-3"
                  :class="g.color"
                  :style="{ width: g.pct + '%' }"
                >
                  <span class="text-xs font-bold text-white drop-shadow">{{ g.pct }}%</span>
                </div>
              </div>
              <span class="text-xs text-muted w-[60px] text-right">{{ g.count.toLocaleString() }} SV</span>
            </div>
          </div>
        </div>

        <!-- GPA Trends -->
        <div class="lg-glass-soft lg-card p-5">
          <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
            <component :is="TrendingUp" :size="18" class="text-primary" /> Xu hướng GPA (3 kỳ gần nhất)
          </h3>
          <div class="space-y-3">
            <div v-for="(t, idx) in gpaTrends" :key="t.semester" class="lg-glass-soft rounded-xl p-4 flex items-center justify-between">
              <div>
                <p class="text-sm font-semibold text-heading">{{ t.semester }}</p>
                <p class="text-xs text-muted">{{ t.students.toLocaleString() }} sinh viên</p>
              </div>
              <div class="flex items-center gap-6">
                <div class="text-center">
                  <p class="text-lg font-bold text-heading">{{ t.gpa }}</p>
                  <p class="text-[10px] text-muted uppercase tracking-wider">GPA</p>
                </div>
                <div class="text-center">
                  <p class="text-lg font-bold text-emerald-500">{{ t.passRate }}%</p>
                  <p class="text-[10px] text-muted uppercase tracking-wider">Pass Rate</p>
                </div>
                <div v-if="idx > 0" class="flex items-center gap-1">
                  <component
                    :is="t.gpa > gpaTrends[idx-1].gpa ? TrendingUp : TrendingDown"
                    :size="16"
                    :class="t.gpa > gpaTrends[idx-1].gpa ? 'text-emerald-500' : 'text-rose-500'"
                  />
                  <span
                    class="text-xs font-semibold"
                    :class="t.gpa > gpaTrends[idx-1].gpa ? 'text-emerald-500' : 'text-rose-500'"
                  >
                    {{ t.gpa > gpaTrends[idx-1].gpa ? '+' : '' }}{{ (t.gpa - gpaTrends[idx-1].gpa).toFixed(2) }}
                  </span>
                </div>
                <div v-else class="w-[50px]"></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Tables Row -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Top Fail Subjects -->
        <div class="lg-glass-soft lg-card p-5">
          <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
            <component :is="XCircle" :size="18" class="text-rose-500" /> Top 5 Môn Tỷ Lệ Fail Cao
          </h3>
          <div class="lg-table-shell lg-density-normal">
            <table class="w-full text-sm">
              <thead>
                <tr>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Mã môn</th>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Tên môn</th>
                  <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">SV fail</th>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[160px]">Tỷ lệ fail</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="s in topFailSubjects" :key="s.code" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                  <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ s.code }}</td>
                  <td class="px-3 py-2.5 text-body">{{ s.name }}</td>
                  <td class="px-3 py-2.5 text-right text-heading font-semibold">{{ s.failed }}</td>
                  <td class="px-3 py-2.5">
                    <div class="flex items-center gap-2">
                      <div class="flex-1 h-2 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                        <div class="h-full rounded-full bg-rose-500 transition-all duration-500" :style="{ width: s.failRate + '%' }"></div>
                      </div>
                      <span class="text-xs font-semibold text-rose-500 w-[42px] text-right">{{ s.failRate }}%</span>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Top Pass Subjects -->
        <div class="lg-glass-soft lg-card p-5">
          <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
            <component :is="CheckCircle" :size="18" class="text-emerald-500" /> Top 5 Môn Tỷ Lệ Pass Cao
          </h3>
          <div class="lg-table-shell lg-density-normal">
            <table class="w-full text-sm">
              <thead>
                <tr>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Mã môn</th>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Tên môn</th>
                  <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">SV pass</th>
                  <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[160px]">Tỷ lệ pass</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="s in topPassSubjects" :key="s.code" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                  <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ s.code }}</td>
                  <td class="px-3 py-2.5 text-body">{{ s.name }}</td>
                  <td class="px-3 py-2.5 text-right text-heading font-semibold">{{ s.passed }}</td>
                  <td class="px-3 py-2.5">
                    <div class="flex items-center gap-2">
                      <div class="flex-1 h-2 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                        <div class="h-full rounded-full bg-emerald-500 transition-all duration-500" :style="{ width: s.passRate + '%' }"></div>
                      </div>
                      <span class="text-xs font-semibold text-emerald-500 w-[42px] text-right">{{ s.passRate }}%</span>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Data Timestamp -->
      <div class="text-center">
        <p class="text-xs text-muted">
          Dữ liệu cập nhật lần cuối: <span class="font-semibold">12/06/2026 02:00</span> (CRON hàng đêm)
        </p>
      </div>
    </div>
  </div>
</template>
