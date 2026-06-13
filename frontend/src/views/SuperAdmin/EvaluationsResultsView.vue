<script setup>
/**
 * EvaluationsResultsView.vue - Super Admin
 * Kết quả đánh giá giảng viên: bảng xếp hạng, điểm TB theo tiêu chí, biểu đồ phân bố.
 */
import { ref, computed } from 'vue'
import {
  BarChart3,
  Star,
  Search,
  Filter,
  RotateCcw,
  Eye,
  X,
  TrendingUp,
  TrendingDown,
  Award,
  Users,
  ChevronDown,
  ChevronUp,
  Download
} from 'lucide-vue-next'

// --- Mock Data ---
const semesters = ref(['Spring 2026', 'Fall 2025', 'Summer 2025'])
const selectedSemester = ref('Spring 2026')
const searchQuery = ref('')
const filterDepartment = ref('all')

const departments = ref([
  { id: 'SE', name: 'Kỹ thuật phần mềm' },
  { id: 'IA', name: 'An toàn thông tin' },
  { id: 'GD', name: 'Thiết kế đồ họa' },
  { id: 'BA', name: 'Quản trị kinh doanh' }
])

const teacherResults = ref([
  {
    id: 1,
    name: 'TS. Nguyễn Văn Minh',
    email: 'minhnv@fpt.edu.vn',
    department: 'SE',
    campus: 'Cơ sở Hòa Lạc',
    subjects: ['PRN211', 'SWD392'],
    avgScore: 4.72,
    totalResponses: 156,
    trend: 'up',
    trendValue: '+0.15',
    criteria: {
      'Nội dung giảng dạy': 4.85,
      'Phương pháp truyền đạt': 4.70,
      'Quản lý lớp học': 4.60,
      'Tài liệu và bài tập': 4.80,
      'Hỗ trợ sinh viên': 4.75,
      'Đánh giá công bằng': 4.65
    },
    isExpanded: false
  },
  {
    id: 2,
    name: 'ThS. Trần Thị Lan',
    email: 'lannt@fpt.edu.vn',
    department: 'SE',
    campus: 'Cơ sở TP.HCM',
    subjects: ['PRN221', 'SWE201c'],
    avgScore: 4.55,
    totalResponses: 128,
    trend: 'up',
    trendValue: '+0.08',
    criteria: {
      'Nội dung giảng dạy': 4.60,
      'Phương pháp truyền đạt': 4.50,
      'Quản lý lớp học': 4.45,
      'Tài liệu và bài tập': 4.70,
      'Hỗ trợ sinh viên': 4.55,
      'Đánh giá công bằng': 4.50
    },
    isExpanded: false
  },
  {
    id: 3,
    name: 'TS. Lê Hoàng Phúc',
    email: 'phuclh@fpt.edu.vn',
    department: 'IA',
    campus: 'Cơ sở Đà Nẵng',
    subjects: ['IAO201', 'NWC203'],
    avgScore: 4.38,
    totalResponses: 98,
    trend: 'down',
    trendValue: '-0.12',
    criteria: {
      'Nội dung giảng dạy': 4.50,
      'Phương pháp truyền đạt': 4.20,
      'Quản lý lớp học': 4.35,
      'Tài liệu và bài tập': 4.40,
      'Hỗ trợ sinh viên': 4.45,
      'Đánh giá công bằng': 4.40
    },
    isExpanded: false
  },
  {
    id: 4,
    name: 'ThS. Phạm Văn Hải',
    email: 'haipv@fpt.edu.vn',
    department: 'GD',
    campus: 'Cơ sở Hòa Lạc',
    subjects: ['GDM301', 'GDW401'],
    avgScore: 4.15,
    totalResponses: 85,
    trend: 'down',
    trendValue: '-0.20',
    criteria: {
      'Nội dung giảng dạy': 4.30,
      'Phương pháp truyền đạt': 4.00,
      'Quản lý lớp học': 4.10,
      'Tài liệu và bài tập': 4.25,
      'Hỗ trợ sinh viên': 4.10,
      'Đánh giá công bằng': 4.15
    },
    isExpanded: false
  },
  {
    id: 5,
    name: 'PGS.TS. Đỗ Thanh Tùng',
    email: 'tungdt@fpt.edu.vn',
    department: 'SE',
    campus: 'Cơ sở Hòa Lạc',
    subjects: ['MAD101', 'MAS291'],
    avgScore: 4.82,
    totalResponses: 210,
    trend: 'up',
    trendValue: '+0.05',
    criteria: {
      'Nội dung giảng dạy': 4.90,
      'Phương pháp truyền đạt': 4.85,
      'Quản lý lớp học': 4.75,
      'Tài liệu và bài tập': 4.88,
      'Hỗ trợ sinh viên': 4.80,
      'Đánh giá công bằng': 4.72
    },
    isExpanded: false
  },
  {
    id: 6,
    name: 'ThS. Hoàng Minh Tuấn',
    email: 'tuanhm@fpt.edu.vn',
    department: 'BA',
    campus: 'Cơ sở TP.HCM',
    subjects: ['MKT101', 'ECO201'],
    avgScore: 3.95,
    totalResponses: 72,
    trend: 'down',
    trendValue: '-0.30',
    criteria: {
      'Nội dung giảng dạy': 4.10,
      'Phương pháp truyền đạt': 3.80,
      'Quản lý lớp học': 3.90,
      'Tài liệu và bài tập': 4.00,
      'Hỗ trợ sinh viên': 3.85,
      'Đánh giá công bằng': 4.05
    },
    isExpanded: false
  }
])

// Sort by avg score descending
const sortedResults = computed(() => {
  let filtered = teacherResults.value.filter(t => {
    const matchSearch = searchQuery.value === '' ||
      t.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      t.email.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchDept = filterDepartment.value === 'all' || t.department === filterDepartment.value
    return matchSearch && matchDept
  })
  return [...filtered].sort((a, b) => b.avgScore - a.avgScore)
})

const resetFilters = () => { searchQuery.value = ''; filterDepartment.value = 'all' }

// --- KPI ---
const totalTeachers = computed(() => teacherResults.value.length)
const systemAvgScore = computed(() => {
  const sum = teacherResults.value.reduce((s, t) => s + t.avgScore, 0)
  return (sum / teacherResults.value.length).toFixed(2)
})
const totalResponses = computed(() => teacherResults.value.reduce((s, t) => s + t.totalResponses, 0))
const topTeacher = computed(() => {
  const sorted = [...teacherResults.value].sort((a, b) => b.avgScore - a.avgScore)
  return sorted[0]?.name || '-'
})

// --- Score Distribution (fake chart data rendered as bars) ---
const scoreDistribution = ref([
  { range: '4.5 - 5.0', count: 3, percent: 50 },
  { range: '4.0 - 4.4', count: 2, percent: 33 },
  { range: '3.5 - 3.9', count: 1, percent: 17 },
  { range: '3.0 - 3.4', count: 0, percent: 0 },
  { range: 'Dưới 3.0', count: 0, percent: 0 }
])

const toggleExpand = (teacher) => { teacher.isExpanded = !teacher.isExpanded }

const getScoreColor = (score) => {
  if (score >= 4.5) return 'text-emerald-600 dark:text-emerald-400'
  if (score >= 4.0) return 'text-blue-600 dark:text-blue-400'
  if (score >= 3.5) return 'text-amber-600 dark:text-amber-400'
  return 'text-rose-600 dark:text-rose-400'
}

const getScoreBgColor = (score) => {
  if (score >= 4.5) return 'bg-emerald-500'
  if (score >= 4.0) return 'bg-blue-500'
  if (score >= 3.5) return 'bg-amber-500'
  return 'bg-rose-500'
}

const getRankBadge = (idx) => {
  if (idx === 0) return { text: '🥇 #1', class: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/20' }
  if (idx === 1) return { text: '🥈 #2', class: 'bg-slate-300/20 text-slate-600 dark:text-slate-400 border-slate-300 dark:border-slate-500/20' }
  if (idx === 2) return { text: '🥉 #3', class: 'bg-amber-700/10 text-amber-700 dark:text-amber-500 border-amber-300 dark:border-amber-600/20' }
  return { text: `#${idx + 1}`, class: 'bg-surface-input text-muted border-default' }
}

const getDeptName = (code) => departments.value.find(d => d.id === code)?.name || code
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs"><div class="lg-shell-orb lg-shell-orb-primary"></div><div class="lg-shell-orb lg-shell-orb-secondary"></div></div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <BarChart3 class="w-8 h-8 text-primary" />
            Kết Quả Đánh Giá Giảng Viên
          </h1>
          <p class="text-sm text-muted mt-1">Xếp hạng và phân tích kết quả đánh giá giảng viên theo kỳ học.</p>
        </div>
        <div class="flex items-center gap-2">
          <select v-model="selectedSemester" class="px-3 lg-control text-sm">
            <option v-for="sem in semesters" :key="sem" :value="sem">{{ sem }}</option>
          </select>
        </div>
      </div>

      <!-- KPI -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500"><Users class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng GV đánh giá</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ totalTeachers }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500"><Star class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Điểm TB hệ thống</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ systemAvgScore }}/5</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500"><BarChart3 class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng phản hồi SV</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ totalResponses.toLocaleString() }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500"><Award class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">GV xuất sắc nhất</div><div class="text-sm font-bold mt-0.5 text-heading truncate">{{ topTeacher }}</div></div>
        </div>
      </div>

      <!-- Score Distribution Chart -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-default">
          <BarChart3 class="w-4.5 h-4.5 text-primary" />
          <h3 class="font-bold text-heading text-sm">Phân bố điểm đánh giá ({{ selectedSemester }})</h3>
        </div>
        <div class="space-y-2.5">
          <div v-for="dist in scoreDistribution" :key="dist.range" class="flex items-center gap-3">
            <span class="text-xs font-bold text-label w-24 text-right flex-shrink-0">{{ dist.range }}</span>
            <div class="flex-1 lg-progress-track h-6 relative">
              <div
                class="h-full rounded-full transition-all duration-500 flex items-center justify-end pr-2"
                :class="dist.percent >= 40 ? 'bg-emerald-500' : dist.percent >= 20 ? 'bg-blue-500' : 'bg-amber-500'"
                :style="{ width: Math.max(dist.percent, 2) + '%' }"
              >
                <span v-if="dist.percent >= 15" class="text-[10px] text-white font-bold">{{ dist.count }} GV</span>
              </div>
              <span v-if="dist.percent < 15" class="absolute left-2 top-1/2 -translate-y-1/2 text-[10px] text-muted font-bold">{{ dist.count }} GV</span>
            </div>
            <span class="text-xs font-bold text-muted w-10 text-right">{{ dist.percent }}%</span>
          </div>
        </div>
      </div>

      <!-- Filter -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2"><Filter class="w-4.5 h-4.5 text-primary" /><h3 class="font-bold text-heading text-sm">Bộ lọc & Tìm kiếm</h3></div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline"><RotateCcw class="w-3.5 h-3.5" /> Xóa lọc</button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
            <input v-model="searchQuery" type="text" placeholder="Tìm theo tên giảng viên, email..." class="w-full pl-9 pr-3 lg-control text-sm" />
          </div>
          <div>
            <select v-model="filterDepartment" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả bộ môn</option>
              <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Teacher Ranking List -->
      <div class="space-y-3 mb-8">
        <div v-if="sortedResults.length === 0" class="lg-glass-soft lg-card lg-density-spacious text-center">
          <BarChart3 class="w-10 h-10 text-muted mx-auto mb-3" />
          <p class="text-sm text-muted font-semibold">Không tìm thấy kết quả đánh giá nào.</p>
        </div>

        <div
          v-for="(teacher, idx) in sortedResults"
          :key="teacher.id"
          class="lg-glass-soft lg-card overflow-visible transition-all duration-200"
          :class="teacher.isExpanded ? 'ring-1 ring-primary/20' : ''"
        >
          <!-- Card Header -->
          <div class="flex items-center gap-4 px-4 py-3.5 cursor-pointer hover:bg-surface-card-hover transition-colors rounded-xl" @click="toggleExpand(teacher)">
            <!-- Rank Badge -->
            <div class="flex-shrink-0">
              <span class="text-[10px] font-extrabold px-2.5 py-1 rounded-full border" :class="getRankBadge(idx).class">
                {{ getRankBadge(idx).text }}
              </span>
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="font-bold text-heading text-sm">{{ teacher.name }}</div>
              <div class="text-[10px] text-muted flex items-center gap-2 flex-wrap mt-0.5">
                <span>{{ getDeptName(teacher.department) }}</span>
                <span>·</span>
                <span>{{ teacher.campus }}</span>
                <span>·</span>
                <span>{{ teacher.subjects.join(', ') }}</span>
              </div>
            </div>

            <!-- Score & Trend -->
            <div class="flex items-center gap-3 flex-shrink-0">
              <div class="text-right">
                <div class="text-lg font-extrabold" :class="getScoreColor(teacher.avgScore)">{{ teacher.avgScore.toFixed(2) }}</div>
                <div class="flex items-center gap-1 justify-end">
                  <component :is="teacher.trend === 'up' ? TrendingUp : TrendingDown" class="w-3 h-3" :class="teacher.trend === 'up' ? 'text-emerald-500' : 'text-rose-500'" />
                  <span class="text-[10px] font-bold" :class="teacher.trend === 'up' ? 'text-emerald-500' : 'text-rose-500'">{{ teacher.trendValue }}</span>
                </div>
              </div>
              <div class="text-xs text-muted text-right hidden sm:block">
                <div class="font-bold">{{ teacher.totalResponses }}</div>
                <div class="text-[10px]">phản hồi</div>
              </div>
              <component :is="teacher.isExpanded ? ChevronUp : ChevronDown" class="w-5 h-5 text-primary" />
            </div>
          </div>

          <!-- Expanded: Criteria Breakdown -->
          <div v-if="teacher.isExpanded" class="px-4 pb-4 pt-0">
            <div class="ml-2 pl-4 border-l-2 border-primary/20 space-y-2">
              <h5 class="text-xs font-bold text-label uppercase mb-2">Chi tiết theo tiêu chí</h5>
              <div v-for="(score, criterion) in teacher.criteria" :key="criterion" class="flex items-center gap-3">
                <span class="text-xs text-muted w-44 flex-shrink-0 truncate">{{ criterion }}</span>
                <div class="flex-1 lg-progress-track h-2.5">
                  <div class="h-full rounded-full transition-all duration-500" :class="getScoreBgColor(score)" :style="{ width: (score / 5 * 100) + '%' }"></div>
                </div>
                <span class="text-xs font-extrabold w-10 text-right" :class="getScoreColor(score)">{{ score.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>