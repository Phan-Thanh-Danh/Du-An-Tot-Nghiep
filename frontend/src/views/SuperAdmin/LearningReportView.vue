<script setup>
/**
 * LearningReportView.vue - Super Admin
 * Báo cáo học tập — GPA theo cơ sở, SV nguy cơ rớt (AI prediction),
 * chi tiết môn học. Module M18 + M6 data.
 */
import { ref, computed } from 'vue'
import {
  LineChart, AlertTriangle, Users, BookOpen, Filter, RotateCcw,
  Search, TrendingUp, TrendingDown, ArrowUp, ArrowDown, Brain,
  GraduationCap, Eye, ChevronDown, ChevronUp, Info, Shield
} from 'lucide-vue-next'

// --- Filters ---
const semesters = ref(['Spring 2026', 'Fall 2025', 'Summer 2025'])
const campuses = ref(['Tất cả', 'Hà Nội', 'Hòa Lạc', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ'])
const departments = ref(['Tất cả', 'Phần mềm', 'An toàn thông tin', 'Thiết kế đồ họa', 'Quản trị kinh doanh', 'Khoa học cơ bản'])

const filterSemester = ref('Spring 2026')
const filterCampus = ref('Tất cả')
const filterDepartment = ref('Tất cả')
const searchQuery = ref('')

const resetFilters = () => {
  filterSemester.value = 'Spring 2026'
  filterCampus.value = 'Tất cả'
  filterDepartment.value = 'Tất cả'
  searchQuery.value = ''
}

// --- KPI ---
const kpiData = computed(() => ([
  { label: 'GPA TB Hệ Thống', value: '7.28', icon: GraduationCap, color: 'text-blue-500', bg: 'bg-blue-500/10' },
  { label: 'SV Nguy Cơ Rớt', value: '156', icon: AlertTriangle, color: 'text-rose-500', bg: 'bg-rose-500/10' },
  { label: 'Môn Fail Cao Nhất', value: 'PRN211', subValue: '28.4%', icon: BookOpen, color: 'text-amber-500', bg: 'bg-amber-500/10' },
  { label: 'Tổng SV Đang Học', value: '12.450', icon: Users, color: 'text-violet-500', bg: 'bg-violet-500/10' },
]))

// --- GPA by Campus ---
const campusGPA = ref([
  { campus: 'Hà Nội', students: 3200, gpa: 7.45, passRate: 85.2, failRate: 14.8, delta: +0.18 },
  { campus: 'Hòa Lạc', students: 4500, gpa: 7.32, passRate: 83.1, failRate: 16.9, delta: +0.12 },
  { campus: 'TP.HCM', students: 3100, gpa: 7.18, passRate: 80.5, failRate: 19.5, delta: +0.08 },
  { campus: 'Đà Nẵng', students: 1200, gpa: 7.05, passRate: 78.2, failRate: 21.8, delta: -0.05 },
  { campus: 'Cần Thơ', students: 450, gpa: 7.12, passRate: 79.8, failRate: 20.2, delta: +0.03 },
])

// --- At-risk Students (AI prediction) ---
const atRiskStudents = ref([
  { id: 'HE170045', name: 'Trần Thị Bích', campus: 'Hòa Lạc', subject: 'PRN211', currentGpa: 3.2, risk: 'Cao', probability: 89 },
  { id: 'HE170112', name: 'Lê Hoàng Long', campus: 'Đà Nẵng', subject: 'MAD101', currentGpa: 3.8, risk: 'Cao', probability: 82 },
  { id: 'HE170201', name: 'Phạm Minh Tuấn', campus: 'TP.HCM', subject: 'CSD201', currentGpa: 4.1, risk: 'Cao', probability: 78 },
  { id: 'HE170078', name: 'Nguyễn Thị Hạnh', campus: 'Hà Nội', subject: 'OSG202', currentGpa: 4.5, risk: 'Trung bình', probability: 62 },
  { id: 'HE170155', name: 'Võ Đình Khoa', campus: 'Hòa Lạc', subject: 'MAS291', currentGpa: 4.3, risk: 'Trung bình', probability: 58 },
  { id: 'HE170310', name: 'Đỗ Thúy Ngân', campus: 'TP.HCM', subject: 'PRN211', currentGpa: 4.6, risk: 'Trung bình', probability: 55 },
  { id: 'HE170089', name: 'Bùi Văn Sơn', campus: 'Hà Nội', subject: 'MAD101', currentGpa: 4.8, risk: 'Thấp', probability: 42 },
  { id: 'HE170422', name: 'Hồ Quốc Việt', campus: 'Đà Nẵng', subject: 'CSD201', currentGpa: 4.9, risk: 'Thấp', probability: 38 },
])

// --- Subject Details ---
const subjectDetails = ref([
  { code: 'PRN211', name: 'Lập trình C# (.NET)', enrolled: 580, pass: 415, fail: 165, failRate: 28.4, avgGpa: 5.82 },
  { code: 'MAD101', name: 'Toán rời rạc', enrolled: 720, pass: 540, fail: 180, failRate: 25.0, avgGpa: 6.15 },
  { code: 'CSD201', name: 'Cấu trúc dữ liệu & GT', enrolled: 640, pass: 493, fail: 147, failRate: 23.0, avgGpa: 6.28 },
  { code: 'OSG202', name: 'Hệ điều hành', enrolled: 410, pass: 324, fail: 86, failRate: 21.0, avgGpa: 6.45 },
  { code: 'MAS291', name: 'Xác suất thống kê', enrolled: 550, pass: 445, fail: 105, failRate: 19.1, avgGpa: 6.52 },
  { code: 'SWD392', name: 'Thiết kế & Kiến trúc PM', enrolled: 420, pass: 357, fail: 63, failRate: 15.0, avgGpa: 7.10 },
  { code: 'SWE201c', name: 'Nhập môn kỹ nghệ PM', enrolled: 480, pass: 465, fail: 15, failRate: 3.1, avgGpa: 7.85 },
])

const filteredSubjects = computed(() => {
  if (!searchQuery.value) return subjectDetails.value
  const q = searchQuery.value.toLowerCase()
  return subjectDetails.value.filter(s =>
    s.code.toLowerCase().includes(q) || s.name.toLowerCase().includes(q)
  )
})

// --- Expand/Collapse Risk Table ---
const showAllRisk = ref(false)
const displayedRisk = computed(() => showAllRisk.value ? atRiskStudents.value : atRiskStudents.value.slice(0, 5))

const getRiskBadge = (risk) => {
  switch (risk) {
    case 'Cao': return 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-200 dark:border-rose-500/20'
    case 'Trung bình': return 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border border-amber-200 dark:border-amber-500/20'
    case 'Thấp': return 'bg-sky-500/10 text-sky-600 dark:text-sky-400 border border-sky-200 dark:border-sky-500/20'
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
          <div>
            <p class="text-2xl font-bold text-heading leading-tight">{{ kpi.value }}</p>
            <p v-if="kpi.subValue" class="text-xs text-rose-500 font-semibold">{{ kpi.subValue }}</p>
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
        <select v-model="filterDepartment" class="lg-control text-sm min-w-[160px]">
          <option v-for="d in departments" :key="d" :value="d">{{ d }}</option>
        </select>
        <button @click="resetFilters" class="lg-btn-secondary text-xs flex items-center gap-1 px-3 py-1.5">
          <component :is="RotateCcw" :size="14" /> Đặt lại
        </button>
      </div>

      <!-- GPA by Campus -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-4 flex items-center gap-2">
          <component :is="LineChart" :size="18" class="text-primary" /> GPA theo Cơ sở
        </h3>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Cơ sở</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Tổng SV</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">GPA TB</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Xu hướng</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[200px]">Tỷ lệ Pass</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Fail</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="c in campusGPA" :key="c.campus" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-semibold text-heading">{{ c.campus }}</td>
                <td class="px-3 py-2.5 text-right text-body">{{ c.students.toLocaleString() }}</td>
                <td class="px-3 py-2.5 text-right font-bold text-heading">{{ c.gpa.toFixed(2) }}</td>
                <td class="px-3 py-2.5 text-right">
                  <span class="inline-flex items-center gap-0.5 text-xs font-semibold" :class="c.delta >= 0 ? 'text-emerald-500' : 'text-rose-500'">
                    <component :is="c.delta >= 0 ? ArrowUp : ArrowDown" :size="12" />
                    {{ c.delta >= 0 ? '+' : '' }}{{ c.delta.toFixed(2) }}
                  </span>
                </td>
                <td class="px-3 py-2.5">
                  <div class="flex items-center gap-2">
                    <div class="flex-1 h-2.5 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                      <div class="h-full rounded-full bg-emerald-500 transition-all duration-500" :style="{ width: c.passRate + '%' }"></div>
                    </div>
                    <span class="text-xs font-semibold text-emerald-600 w-[42px] text-right">{{ c.passRate }}%</span>
                  </div>
                </td>
                <td class="px-3 py-2.5 text-right text-rose-500 font-semibold text-xs">{{ c.failRate }}%</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- At-Risk Students (AI) -->
      <div class="lg-glass-soft lg-card p-5">
        <h3 class="text-base font-semibold text-heading mb-1 flex items-center gap-2">
          <component :is="Brain" :size="18" class="text-rose-500" /> Sinh viên Nguy Cơ Rớt Môn
          <span class="lg-badge lg-badge-danger text-[10px] ml-1">AI Prediction</span>
        </h3>
        <p class="text-xs text-muted mb-4 flex items-center gap-1">
          <component :is="Info" :size="12" /> Dự đoán bằng RandomForest dựa trên điểm hiện tại, điểm danh và tiến độ lab (M6)
        </p>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">MSSV</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Họ tên</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Cơ sở</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Môn</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">GPA hiện tại</th>
                <th class="text-center text-label font-medium px-3 py-2.5 border-b border-default">Mức rủi ro</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Xác suất</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in displayedRisk" :key="s.id" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ s.id }}</td>
                <td class="px-3 py-2.5 text-body">{{ s.name }}</td>
                <td class="px-3 py-2.5 text-muted text-xs">{{ s.campus }}</td>
                <td class="px-3 py-2.5 font-mono text-xs text-heading font-semibold">{{ s.subject }}</td>
                <td class="px-3 py-2.5 text-right font-bold" :class="s.currentGpa < 4.0 ? 'text-rose-500' : 'text-amber-500'">{{ s.currentGpa.toFixed(1) }}</td>
                <td class="px-3 py-2.5 text-center">
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[11px] font-semibold" :class="getRiskBadge(s.risk)">
                    <component :is="AlertTriangle" :size="11" />
                    {{ s.risk }}
                  </span>
                </td>
                <td class="px-3 py-2.5 text-right">
                  <div class="flex items-center justify-end gap-2">
                    <div class="w-16 h-1.5 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                      <div class="h-full rounded-full transition-all duration-500" :class="s.probability >= 70 ? 'bg-rose-500' : s.probability >= 50 ? 'bg-amber-500' : 'bg-sky-500'" :style="{ width: s.probability + '%' }"></div>
                    </div>
                    <span class="text-xs font-semibold text-heading">{{ s.probability }}%</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="atRiskStudents.length > 5" class="mt-3 text-center">
          <button @click="showAllRisk = !showAllRisk" class="text-xs text-link font-medium inline-flex items-center gap-1 hover:underline">
            <component :is="showAllRisk ? ChevronUp : ChevronDown" :size="14" />
            {{ showAllRisk ? 'Thu gọn' : `Xem tất cả (${atRiskStudents.length})` }}
          </button>
        </div>
      </div>

      <!-- Subject Details -->
      <div class="lg-glass-soft lg-card p-5">
        <div class="flex items-center justify-between mb-4 flex-wrap gap-3">
          <h3 class="text-base font-semibold text-heading flex items-center gap-2">
            <component :is="BookOpen" :size="18" class="text-primary" /> Chi Tiết Theo Môn Học
          </h3>
          <div class="relative">
            <component :is="Search" :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
            <input v-model="searchQuery" placeholder="Tìm mã/tên môn..." class="lg-control text-sm pl-9 pr-3 py-1.5 w-[220px]" />
          </div>
        </div>
        <div class="lg-table-shell lg-density-normal">
          <table class="w-full text-sm">
            <thead>
              <tr>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Mã</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default">Tên môn</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Đăng ký</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Pass</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">Fail</th>
                <th class="text-left text-label font-medium px-3 py-2.5 border-b border-default w-[140px]">% Fail</th>
                <th class="text-right text-label font-medium px-3 py-2.5 border-b border-default">GPA TB</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in filteredSubjects" :key="s.code" class="hover:bg-black/[0.02] dark:hover:bg-white/[0.02] transition">
                <td class="px-3 py-2.5 font-mono text-xs font-semibold text-heading">{{ s.code }}</td>
                <td class="px-3 py-2.5 text-body">{{ s.name }}</td>
                <td class="px-3 py-2.5 text-right text-muted">{{ s.enrolled }}</td>
                <td class="px-3 py-2.5 text-right text-emerald-500 font-semibold">{{ s.pass }}</td>
                <td class="px-3 py-2.5 text-right text-rose-500 font-semibold">{{ s.fail }}</td>
                <td class="px-3 py-2.5">
                  <div class="flex items-center gap-2">
                    <div class="flex-1 h-2 rounded-full bg-black/5 dark:bg-white/5 overflow-hidden">
                      <div class="h-full rounded-full transition-all" :class="s.failRate > 20 ? 'bg-rose-500' : s.failRate > 10 ? 'bg-amber-500' : 'bg-emerald-500'" :style="{ width: Math.min(s.failRate * 2, 100) + '%' }"></div>
                    </div>
                    <span class="text-xs font-semibold w-[38px] text-right" :class="s.failRate > 20 ? 'text-rose-500' : s.failRate > 10 ? 'text-amber-500' : 'text-emerald-500'">{{ s.failRate }}%</span>
                  </div>
                </td>
                <td class="px-3 py-2.5 text-right font-bold text-heading">{{ s.avgGpa.toFixed(2) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <!-- Empty State -->
        <div v-if="filteredSubjects.length === 0" class="py-12 text-center">
          <component :is="BookOpen" :size="40" class="mx-auto text-muted opacity-40 mb-3" />
          <p class="text-sm text-muted">Không tìm thấy môn học phù hợp.</p>
        </div>
      </div>
    </div>
  </div>
</template>
