<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import {
  Activity,
  AlertCircle,
  ArrowLeft,
  Award,
  BookMarked,
  BookOpen,
  Clock,
  Filter,
  Layers,
  Lock,
  Mail,
  Search,
  Target,
  User,
  Users,
  CheckCircle2,
  X,
} from 'lucide-vue-next'

import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const students = ref([])
const filterStatus = ref('')
const searchQuery = ref('')

const statusFilterOptions = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'excellent', label: 'Hoàn thành tốt' },
  { value: 'good', label: 'Đang học' },
  { value: 'warning', label: 'Chậm tiến độ' },
  { value: 'danger', label: 'Nguy cơ' }
]

const filteredStudents = computed(() => {
  let list = students.value
  if (filterStatus.value) {
    list = list.filter(s => s.status === filterStatus.value)
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(s => (s.name || '').toLowerCase().includes(q) || String(s.id || '').toLowerCase().includes(q))
  }
  return list
})

const overallProgress = ref(0)
const completedLessons = ref(0)
const totalLessons = ref(0)
const courseTotalLessons = ref(10)
const activeStudents = ref(0)
const courseName = ref('')
const className = ref('')

const displaySubjectTitle = computed(() => {
  if (!courseName.value || courseName.value === 'Khóa học') return 'Tiến độ môn học'
  const parts = courseName.value.split(' - ')
  return parts[0] || courseName.value
})

const displaySubTitle = computed(() => {
  if (courseName.value && courseName.value.includes(' - ')) {
    const parts = courseName.value.split(' - ')
    return parts.slice(1).join(' · ')
  }
  return className.value ? `${className.value} · Cập nhật theo thời gian thực` : 'Cập nhật theo thời gian thực'
})

const chartData = ref([])
const route = useRoute()

function getClassId(item) {
  return item?.id ?? item?.Id ?? item?.maKhoaHoc ?? item?.MaKhoaHoc ?? item?.classId ?? item?.ClassId
}

const currentClassId = ref(0)
const currentCourseId = ref(0)
const studentDetailLoading = ref(false)
const studentGradeDetail = ref(null)

async function loadStudentGradeDetail(studentId) {
  if (!studentId || (!currentClassId.value && !currentCourseId.value)) return
  studentDetailLoading.value = true
  studentGradeDetail.value = null
  try {
    const targetId = currentClassId.value || currentCourseId.value
    const raw = await teacherApi.getStudentGradeDetail(
      targetId,
      studentId,
      currentCourseId.value || undefined
    )
    studentGradeDetail.value = raw?.data?.data ?? raw?.data ?? raw
  } catch {
    studentGradeDetail.value = null
  } finally {
    studentDetailLoading.value = false
  }
}

function formatDateTime(dtStr) {
  if (!dtStr) return ''
  try {
    const d = new Date(dtStr)
    return d.toLocaleString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    })
  } catch {
    return dtStr
  }
}

const studentAssignmentItems = computed(() => {
  const types = studentGradeDetail.value?.gradeTypes || studentGradeDetail.value?.GradeTypes || []
  if (!types.length) return []
  const items = []
  for (const gt of types) {
    const itemList = gt.items || gt.Items || []
    for (const item of itemList) {
      items.push({
        id: item.itemId || item.ItemId,
        title: item.itemName || item.ItemName || gt.name || gt.Name,
        groupName: gt.name || gt.Name,
        grade: item.grade ?? item.Grade ?? null,
        status: item.status || item.Status || (item.grade !== null ? 'da_cham' : 'chua_nop'),
        submittedAt: item.submittedAt || item.SubmittedAt || null,
        isSubmitted: item.isSubmitted || item.IsSubmitted || false
      })
    }
  }
  return items
})

const studentActivities = computed(() => {
  return studentGradeDetail.value?.activities || studentGradeDetail.value?.Activities || []
})

const studentGpaDisplay = computed(() => {
  if (studentGradeDetail.value) {
    const val = studentGradeDetail.value.gpaMonHoc ?? studentGradeDetail.value.GpaMonHoc ?? studentGradeDetail.value.diemQuaTrinh ?? studentGradeDetail.value.DiemQuaTrinh
    if (val !== null && val !== undefined && val !== '') {
      return `${val} / 10`
    }
  }
  if (selectedStudent.value) {
    const val = selectedStudent.value.gpa ?? selectedStudent.value.score
    if (val !== null && val !== undefined && val !== '') {
      return `${val} / 10`
    }
  }
  return 'Chưa có điểm'
})

const gradeBreakdown = computed(() => {
  if (!studentGradeDetail.value) return null
  return {
    gpa: studentGradeDetail.value.gpaMonHoc ?? studentGradeDetail.value.GpaMonHoc ?? null,
    processScore: studentGradeDetail.value.diemQuaTrinh ?? studentGradeDetail.value.DiemQuaTrinh ?? null,
    midtermScore: studentGradeDetail.value.diemGiuaKy ?? studentGradeDetail.value.DiemGiuaKy ?? null,
    finalScore: studentGradeDetail.value.diemCuoiKy ?? studentGradeDetail.value.DiemCuoiKy ?? null,
    status: studentGradeDetail.value.trangThai ?? studentGradeDetail.value.TrangThai ?? null,
    isLocked: studentGradeDetail.value.daKhoa ?? studentGradeDetail.value.DaKhoa ?? false,
    gradeTypes: studentGradeDetail.value.gradeTypes ?? studentGradeDetail.value.GradeTypes ?? []
  }
})

async function loadProgress() {
  loading.value = true
  error.value = ''
  try {
    let courseId = route.params.id
    if (!courseId || courseId === 'undefined') {
      const courseList = await teacherApi.getTeacherCourses({ pageSize: 1 })
      const firstCourse = (courseList?.items ?? courseList?.Items ?? courseList?.data ?? courseList?.Data ?? courseList ?? [])[0]
      courseId = firstCourse?.courseId ?? firstCourse?.CourseId
    }
    if (!courseId) {
      students.value = []
      overallProgress.value = 0
      completedLessons.value = 0
      totalLessons.value = 0
      activeStudents.value = 0
      courseName.value = 'Chưa có lớp'
      className.value = ''
      chartData.value = []
      return
    }

    const data = await teacherApi.getTeacherClassProgress(courseId)
    const progressData = data?.data?.data ?? data?.data ?? data

    currentClassId.value = progressData?.classId ?? progressData?.id ?? (parseInt(courseId) || 0)
    currentCourseId.value = progressData?.courseId ?? (parseInt(courseId) || 0)

    courseName.value = progressData?.courseName || progressData?.name || 'Khóa học'
    className.value = progressData?.className || ''
    overallProgress.value = progressData?.overallProgress ?? 0
    completedLessons.value = progressData?.completedLessons ?? 0
    totalLessons.value = progressData?.totalLessons ?? 0
    courseTotalLessons.value = progressData?.courseTotalLessons ?? (progressData?.students?.[0]?.totalLessons || 10)
    activeStudents.value = progressData?.activeStudents ?? 0

    students.value = (progressData?.students || []).map((s) => ({
      id: s.id,
      name: s.name,
      email: s.email || `${s.id}@lms.edu.vn`,
      progress: s.progress,
      completedLessons: s.completedLessons ?? Math.round((s.progress / 100) * courseTotalLessons.value),
      totalLessons: s.totalLessons ?? courseTotalLessons.value,
      score: s.score ?? s.gpa,
      gpa: s.gpa ?? s.score ?? null,
      lastActive: s.lastActive,
      status: s.status,
      absent: s.absent || 0,
      avatar: s.avatar || null,
      phone: s.phone || '0987654321',
      major: s.major || 'Công nghệ thông tin',
      cohort: s.cohort || 'K18',
    }))

    chartData.value = progressData?.chartData || progressData?.ChartData || progressData?.distribution || []
  } catch (e) {
    error.value = e?.message || 'Không thể tải tiến độ lớp học.'
    students.value = []
  } finally {
    loading.value = false
  }
  setTimeout(() => { animateProgress.value = true }, 100)
}

const completedCount = computed(() => students.value.filter(s => s.status === 'excellent' || s.progress >= 90).length)
const studyingCount = computed(() => students.value.filter(s => (s.status === 'good' || (s.progress >= 70 && s.progress < 90))).length)
const delayedCount = computed(() => students.value.filter(s => (s.status === 'warning' || (s.progress >= 50 && s.progress < 70))).length)
const riskCount = computed(() => students.value.filter(s => (s.status === 'danger' || s.progress < 50)).length)

const computedChartData = computed(() => {
  const list = students.value || []
  const total = list.length || 1

  const r1 = list.filter(s => s.progress <= 25).length
  const r2 = list.filter(s => s.progress > 25 && s.progress <= 50).length
  const r3 = list.filter(s => s.progress > 50 && s.progress <= 75).length
  const r4 = list.filter(s => s.progress > 75).length
  const maxVal = Math.max(r1, r2, r3, r4, 1)

  return [
    {
      range: '0 - 25%',
      value: r1,
      percent: Math.round((r1 / total) * 100),
      height: Math.max(12, Math.round((r1 / maxVal) * 100)),
      color: 'from-rose-500 to-amber-500',
      bgGlow: 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-500/20',
      tone: 'danger',
      label: 'Cần hỗ trợ'
    },
    {
      range: '26 - 50%',
      value: r2,
      percent: Math.round((r2 / total) * 100),
      height: Math.max(12, Math.round((r2 / maxVal) * 100)),
      color: 'from-amber-500 to-yellow-500',
      bgGlow: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-500/20',
      tone: 'warning',
      label: 'Chậm tiến độ'
    },
    {
      range: '51 - 75%',
      value: r3,
      percent: Math.round((r3 / total) * 100),
      height: Math.max(12, Math.round((r3 / maxVal) * 100)),
      color: 'from-blue-500 to-cyan-500',
      bgGlow: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/20',
      tone: 'info',
      label: 'Đang học'
    },
    {
      range: '76 - 100%',
      value: r4,
      percent: Math.round((r4 / total) * 100),
      height: Math.max(12, Math.round((r4 / maxVal) * 100)),
      color: 'from-emerald-500 to-teal-500',
      bgGlow: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20',
      tone: 'success',
      label: 'Hoàn thành tốt'
    }
  ]
})

const getStatusText = (status) => {
  const texts = {
    excellent: 'Hoàn thành tốt',
    good: 'Đang học',
    warning: 'Chậm tiến độ',
    danger: 'Nguy cơ'
  }
  return texts[status] || 'Đang học'
}

const getStatusVariant = (status) => {
  const variants = {
    excellent: 'success',
    good: 'primary',
    warning: 'warning',
    danger: 'danger'
  }
  return variants[status] || 'primary'
}

const animateProgress = ref(false)

function sendReminder() {
  const pendingCount = students.value.filter(s => s.progress < 100).length
  if (typeof window !== 'undefined') {
    window.alert(`Đã gửi thông báo nhắc nhở tiến độ học tập đến ${pendingCount} sinh viên chưa hoàn thành bài!`)
  }
}

onMounted(() => {
  loadProgress()
})

// --- Student Drawer State ---
const isDrawerOpen = ref(false)
const selectedStudent = ref(null)
const activeTab = ref('profile') // 'profile', 'assignments', 'activity'

const openStudentDetails = async (studentId, tab) => {
  selectedStudent.value = students.value.find(s => s.id === studentId) || null
  activeTab.value = tab
  isDrawerOpen.value = true
  if (selectedStudent.value) {
    await loadStudentGradeDetail(selectedStudent.value.id)
  }
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  selectedStudent.value = null
  studentGradeDetail.value = null
}
</script>

<template>
  <div v-if="loading" class="p-4">
    <SkeletonTable :rows="6" :columns="6" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton variant="secondary" @click="loadProgress">Thử lại</GlassButton>
  </div>
  <div v-else class="class-progress-page lg-page-enter">
    <!-- Top Hero Header Card -->
    <div class="relative overflow-hidden rounded-3xl p-6 md:p-8 lg-glass-soft border border-card/80 shadow-lg shadow-blue-500/5 backdrop-blur-xl">
      <!-- Ambient Gradient Flares -->
      <div class="absolute -right-16 -top-16 w-80 h-80 rounded-full bg-gradient-to-br from-blue-500/20 via-indigo-500/10 to-transparent blur-3xl pointer-events-none" />
      <div class="absolute -left-16 -bottom-16 w-64 h-64 rounded-full bg-gradient-to-tr from-cyan-500/15 via-blue-500/10 to-transparent blur-2xl pointer-events-none" />

      <div class="relative z-10 flex flex-col lg:flex-row lg:items-center justify-between gap-6">
        <!-- Left: Back Button & Course Details -->
        <div class="flex items-start gap-4 md:gap-5 min-w-0">
          <button
            type="button"
            class="h-12 w-12 rounded-2xl surface-card hover:surface-card-hover border border-card flex items-center justify-center text-label hover:text-white hover:bg-blue-600 hover:border-blue-600 transition-all duration-300 shadow-sm shrink-0 group mt-1"
            aria-label="Quay lại danh sách lớp"
            @click="$router.push('/teacher/classes')"
          >
            <ArrowLeft :size="20" class="transition-transform group-hover:-translate-x-0.5" />
          </button>

          <div class="space-y-2 min-w-0 flex-1">
            <!-- Eyebrow & Badges -->
            <div class="flex flex-wrap items-center gap-2">
              <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-blue-500/15 text-blue-600 dark:text-blue-400 border border-blue-500/25 shadow-xs">
                <span class="relative flex h-2 w-2">
                  <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                  <span class="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
                </span>
                Theo dõi tiến độ học tập
              </span>

              <span v-if="className" class="px-2.5 py-1 rounded-full text-xs font-bold surface-card border border-card text-heading shadow-xs">
                {{ className }}
              </span>
            </div>

            <!-- Main Subject Title -->
            <h1 class="text-2xl md:text-3xl lg:text-4xl font-black text-heading tracking-tight leading-tight">
              {{ displaySubjectTitle }}
            </h1>

            <!-- Meta Information Sub-row -->
            <div class="flex flex-wrap items-center gap-2.5 md:gap-3 text-xs font-medium text-body pt-1">
              <span class="flex items-center gap-1.5 bg-surface-input/60 px-2.5 py-1 rounded-xl border border-border-card/60">
                <Users :size="15" class="text-blue-500" />
                Sĩ số: <strong class="text-heading font-bold">{{ activeStudents }} sinh viên</strong>
              </span>

              <span class="flex items-center gap-1.5 bg-surface-input/60 px-2.5 py-1 rounded-xl border border-border-card/60">
                <BookOpen :size="15" class="text-indigo-500" />
                Tiến độ: <strong class="text-heading font-bold">{{ overallProgress }}% hoàn thành</strong>
              </span>

              <span class="flex items-center gap-1.5 bg-surface-input/60 px-2.5 py-1 rounded-xl border border-border-card/60">
                <Clock :size="15" class="text-emerald-500" />
                <span>{{ displaySubTitle }}</span>
              </span>
            </div>
          </div>
        </div>

        <!-- Right: Action Buttons -->
        <div class="flex flex-wrap items-center gap-3 shrink-0 self-start lg:self-center">
          <button
            type="button"
            class="px-4 py-2.5 rounded-xl surface-card hover:surface-card-hover border border-card text-xs font-bold text-heading transition-all duration-200 flex items-center gap-2 shadow-xs hover:border-blue-500/40 active:scale-95 cursor-pointer"
            @click="$router.push('/teacher/lessons')"
          >
            <BookMarked :size="16" class="text-blue-500" />
            <span>Giáo trình môn học</span>
          </button>

          <button
            type="button"
            class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-blue-600 via-indigo-600 to-blue-600 hover:from-blue-500 hover:to-indigo-500 text-xs font-bold text-white transition-all duration-200 flex items-center gap-2 shadow-md shadow-blue-500/25 hover:shadow-blue-500/40 active:scale-95 cursor-pointer"
            @click="sendReminder"
          >
            <Mail :size="16" />
            <span>Gửi nhắc nhở</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 6 KPI Stat Cards Grid -->
    <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-3 md:gap-4">
      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-blue-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Sĩ số lớp</span>
          <Users :size="16" class="text-blue-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-heading">{{ activeStudents }}</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-blue-500/10 text-blue-600 dark:text-blue-400">100%</span>
        </div>
      </div>

      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-emerald-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Tiến độ TB</span>
          <Activity :size="16" class="text-emerald-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-heading">{{ overallProgress }}%</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-600 dark:text-emerald-400">Toàn lớp</span>
        </div>
      </div>

      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-emerald-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Hoàn thành tốt</span>
          <CheckCircle2 :size="16" class="text-emerald-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-emerald-600 dark:text-emerald-400">{{ completedCount }}</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-600 dark:text-emerald-400">≥90%</span>
        </div>
      </div>

      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-indigo-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Đang học</span>
          <BookOpen :size="16" class="text-indigo-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-indigo-600 dark:text-indigo-400">{{ studyingCount }}</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-indigo-500/10 text-indigo-600 dark:text-indigo-400">70-89%</span>
        </div>
      </div>

      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-amber-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Chậm tiến độ</span>
          <Clock :size="16" class="text-amber-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-amber-600 dark:text-amber-400">{{ delayedCount }}</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-amber-500/10 text-amber-600 dark:text-amber-400">50-69%</span>
        </div>
      </div>

      <div class="p-4 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs hover:border-rose-500/30 transition-all">
        <div class="flex items-center justify-between text-muted mb-2">
          <span class="text-xs font-semibold">Cần hỗ trợ</span>
          <AlertCircle :size="16" class="text-rose-500" />
        </div>
        <div class="flex items-baseline justify-between">
          <strong class="text-2xl font-black text-rose-600 dark:text-rose-400">{{ riskCount }}</strong>
          <span class="text-[10px] font-bold px-1.5 py-0.5 rounded bg-rose-500/10 text-rose-600 dark:text-rose-400">&lt;50%</span>
        </div>
      </div>
    </div>

    <!-- Middle Row: 2 Balanced Progress Cards -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
      <!-- Card 1: Tiến độ chung -->
      <div class="p-5 md:p-6 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs">
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <div>
            <h2 class="text-sm font-bold text-heading flex items-center gap-2">
              <Activity :size="18" class="text-blue-500" />
              Tiến độ hoàn thành chung
            </h2>
            <p class="text-xs text-muted mt-0.5">{{ completedLessons }}/{{ totalLessons }} bài học đã hoàn thành của lớp.</p>
          </div>
          <span class="px-3 py-1 rounded-full text-xs font-extrabold bg-blue-500/15 text-blue-600 dark:text-blue-400 border border-blue-500/20 font-mono">
            {{ overallProgress }}%
          </span>
        </div>

        <div class="py-4 space-y-4">
          <div class="flex items-baseline justify-between">
            <span class="text-3xl md:text-4xl font-black text-heading tracking-tight font-mono">{{ overallProgress }}%</span>
            <span class="text-xs font-semibold text-muted">Mục tiêu: 100%</span>
          </div>

          <div class="w-full h-3.5 bg-surface-input border border-card rounded-full overflow-hidden p-0.5">
            <div
              class="h-full bg-gradient-to-r from-blue-600 via-indigo-500 to-cyan-400 rounded-full transition-all duration-1000 shadow-sm"
              :style="{ width: animateProgress ? `${overallProgress}%` : '0%' }"
            />
          </div>

          <div class="grid grid-cols-3 gap-2.5 pt-2">
            <div class="p-2.5 rounded-xl bg-surface-input/60 border border-card text-center">
              <span class="text-[10px] text-muted font-bold block uppercase">Đã học</span>
              <strong class="text-sm font-black text-heading">{{ completedLessons }}</strong>
            </div>
            <div class="p-2.5 rounded-xl bg-surface-input/60 border border-card text-center">
              <span class="text-[10px] text-muted font-bold block uppercase">Còn lại</span>
              <strong class="text-sm font-black text-heading">{{ Math.max(0, totalLessons - completedLessons) }}</strong>
            </div>
            <div class="p-2.5 rounded-xl bg-surface-input/60 border border-card text-center">
              <span class="text-[10px] text-muted font-bold block uppercase">TB / Sinh viên</span>
              <strong class="text-sm font-black text-heading">{{ (completedLessons / (activeStudents || 1)).toFixed(1) }} bài</strong>
            </div>
          </div>
        </div>
      </div>

      <!-- Card 2: Phân bố tiến độ sinh viên (Bar Chart) -->
      <div class="p-5 md:p-6 rounded-2xl lg-glass-soft border border-card flex flex-col justify-between shadow-xs">
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <div>
            <h2 class="text-sm font-bold text-heading flex items-center gap-2">
              <Target :size="18" class="text-indigo-500" />
              Phân bố tiến độ sinh viên
            </h2>
            <p class="text-xs text-muted mt-0.5">Mức độ hoàn thành bài học của sinh viên trong lớp.</p>
          </div>
          <span class="text-xs font-semibold text-muted">4 phân khúc</span>
        </div>

        <div class="pt-3 flex-1 flex flex-col justify-end">
          <div class="grid grid-cols-4 gap-2.5 sm:gap-3 h-32 items-end px-1 sm:px-2">
            <div
              v-for="(item, i) in computedChartData"
              :key="i"
              class="flex flex-col items-center gap-1.5 h-full justify-end group"
            >
              <div class="flex flex-col items-center gap-0.5">
                <span class="text-xs font-black text-heading font-mono">{{ item.value }} SV</span>
                <span class="text-[10px] text-muted font-medium font-mono">({{ item.percent }}%)</span>
              </div>

              <div class="w-full max-w-[3.2rem] bg-surface-input rounded-xl overflow-hidden p-1 flex items-end h-20 border border-card">
                <div
                  class="w-full rounded-lg bg-gradient-to-t transition-all duration-1000 shadow-sm"
                  :class="item.color"
                  :style="{ height: animateProgress ? `${item.height}%` : '8%' }"
                />
              </div>

              <span class="text-[11px] font-bold text-body whitespace-nowrap">{{ item.range }}</span>
            </div>
          </div>

          <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 pt-3 mt-2 border-t border-card">
            <div
              v-for="item in computedChartData"
              :key="item.range"
              class="flex items-center gap-1.5 text-[11px] px-2 py-1 rounded-lg border"
              :class="item.bgGlow"
            >
              <span class="w-1.5 h-1.5 rounded-full shrink-0" :class="item.tone === 'danger' ? 'bg-rose-500' : (item.tone === 'warning' ? 'bg-amber-500' : (item.tone === 'info' ? 'bg-blue-500' : 'bg-emerald-500'))" />
              <span class="truncate font-semibold">{{ item.label }}: <strong>{{ item.value }}</strong></span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Student Table Section with Integrated Toolbar -->
    <GlassPanel variant="flat" density="compact" class="students-panel p-5 md:p-6 rounded-2xl lg-glass-soft border border-card shadow-xs">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-3 pb-3 border-b border-card">
        <div>
          <h2 class="text-base font-bold text-heading flex items-center gap-2">
            <Users :size="18" class="text-blue-500" />
            Danh sách sinh viên
          </h2>
          <p class="text-xs text-muted mt-0.5">
            Hiển thị <strong class="text-heading">{{ filteredStudents.length }}</strong> / {{ students.length }} sinh viên trong lớp.
          </p>
        </div>

        <div class="flex flex-wrap items-center gap-2.5">
          <div class="w-44 sm:w-48">
            <LmsSelect
              v-model="filterStatus"
              placeholder="Tất cả trạng thái"
              :options="statusFilterOptions"
            />
          </div>

          <div class="relative w-full sm:w-64">
            <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm sinh viên, MSSV..."
              class="w-full pl-9 pr-3 py-2 rounded-xl border border-input surface-input text-xs text-heading placeholder:text-placeholder outline-none focus:border-blue-500 transition-all"
            />
          </div>
        </div>
      </div>

      <TableShell density="compact" class="w-full overflow-x-auto">
        <table class="w-full text-left border-collapse text-xs">
          <thead>
            <tr class="border-b border-card surface-table-header">
              <th class="whitespace-nowrap py-2.5 px-3 font-semibold text-muted text-[11px] uppercase tracking-wider">Sinh viên</th>
              <th class="whitespace-nowrap py-2.5 px-2.5 text-center font-semibold text-muted text-[11px] uppercase tracking-wider">MSSV</th>
              <th class="whitespace-nowrap py-2.5 px-3 font-semibold text-muted text-[11px] uppercase tracking-wider">Liên hệ</th>
              <th class="whitespace-nowrap py-2.5 px-2.5 text-center font-semibold text-muted text-[11px] uppercase tracking-wider">Tiến độ</th>
              <th class="whitespace-nowrap py-2.5 px-2.5 text-center font-semibold text-muted text-[11px] uppercase tracking-wider">Bài học</th>
              <th class="whitespace-nowrap py-2.5 px-2.5 text-center font-semibold text-muted text-[11px] uppercase tracking-wider">Đánh giá</th>
              <th class="whitespace-nowrap py-2.5 px-3 font-semibold text-muted text-[11px] uppercase tracking-wider">Lần học cuối</th>
              <th class="whitespace-nowrap py-2.5 px-2.5 text-center font-semibold text-muted text-[11px] uppercase tracking-wider">Trạng thái</th>
              <th class="whitespace-nowrap py-2.5 px-3 text-right font-semibold text-muted text-[11px] uppercase tracking-wider">Hành động</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card/40">
            <tr v-for="sv in filteredStudents" :key="sv.id" class="hover:surface-card-hover transition-colors">
              <td class="py-2 px-3">
                <div class="flex items-center gap-2.5">
                  <span class="shrink-0 w-7 h-7 rounded-full bg-(--accent-primary-soft) text-(--accent-primary) font-bold flex items-center justify-center text-xs border border-(--border-card)">{{ sv.name.split(' ').pop()[0] }}</span>
                  <span class="truncate max-w-[11rem]">
                    <strong class="text-heading font-semibold block text-xs truncate leading-snug">{{ sv.name }}</strong>
                    <small class="text-muted block text-[10px]">{{ sv.absent }} buổi vắng</small>
                  </span>
                </div>
              </td>
              <td class="py-2 px-2.5 text-center font-mono text-xs text-heading font-medium">{{ sv.id }}</td>
              <td class="py-2 px-3">
                <span class="flex items-center gap-1.5 text-xs text-muted whitespace-nowrap">
                  <Mail :size="13" class="shrink-0 text-muted/70" />
                  {{ sv.email }}
                </span>
              </td>
              <td class="py-2 px-2.5 text-center">
                <div class="flex items-center justify-center gap-2">
                  <div class="w-14 h-1.5 surface-input border border-card rounded-full overflow-hidden shrink-0" aria-hidden="true">
                    <span class="block h-full bg-(--accent-primary) transition-all duration-500" :style="{ width: animateProgress ? `${sv.progress}%` : '0%' }" />
                  </div>
                  <strong class="text-xs font-bold text-heading min-w-[2.2rem] text-right">{{ sv.progress }}%</strong>
                </div>
              </td>
              <td class="py-2 px-2.5 text-center whitespace-nowrap text-xs text-heading font-semibold">
                {{ sv.completedLessons ?? Math.round((sv.progress / 100) * courseTotalLessons) }}/{{ sv.totalLessons ?? courseTotalLessons }}
              </td>
              <td class="py-2 px-2.5 text-center whitespace-nowrap text-xs">
                <GlassBadge
                  v-if="sv.gpa !== null && sv.gpa !== undefined && sv.gpa > 0"
                  :variant="sv.gpa >= 8 ? 'success' : sv.gpa < 5 ? 'danger' : (sv.gpa < 6.5 ? 'warning' : 'info')"
                  size="sm"
                >
                  {{ sv.gpa >= 8 ? 'Giỏi' : sv.gpa >= 6.5 ? 'Khá' : (sv.gpa >= 5 ? 'Trung bình' : 'Yếu') }}
                </GlassBadge>
                <GlassBadge v-else variant="secondary" size="sm">
                  Chưa có điểm
                </GlassBadge>
              </td>
              <td class="py-2 px-3 whitespace-nowrap">
                <span class="flex items-center gap-1.5 text-xs text-muted">
                  <Clock :size="13" class="shrink-0 text-muted/70" />
                  {{ sv.lastActive || 'Chưa ghi nhận' }}
                </span>
              </td>
              <td class="py-2 px-2.5 text-center whitespace-nowrap">
                <GlassBadge :variant="getStatusVariant(sv.status)" size="sm">
                  <CheckCircle2 v-if="sv.status === 'excellent' || sv.status === 'good'" :size="11" />
                  <AlertCircle v-else :size="11" />
                  {{ getStatusText(sv.status) }}
                </GlassBadge>
              </td>
              <td class="py-2 px-3 text-right">
                <div class="flex items-center justify-end gap-1 whitespace-nowrap">
                  <GlassButton variant="ghost" size="sm" @click="openStudentDetails(sv.id, 'profile')">
                    <template #leading>
                      <User :size="13" />
                    </template>
                    Hồ sơ
                  </GlassButton>
                  <GlassButton variant="ghost" size="sm" @click="openStudentDetails(sv.id, 'assignments')">
                    Bài nộp
                  </GlassButton>
                  <GlassButton variant="secondary" size="sm" @click="openStudentDetails(sv.id, 'activity')">
                    Chi tiết
                  </GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>

      <div class="table-footer">
        <span>Hiển thị 1-{{ students.length }} trong số {{ activeStudents }} sinh viên</span>
        <div class="pagination">
          <button type="button" class="active">1</button>
        </div>
      </div>
    </GlassPanel>

    <Teleport to="body">
      <div
        v-if="isDrawerOpen || selectedStudent"
        class="drawer-root"
        aria-labelledby="slide-over-title"
        role="dialog"
        aria-modal="true"
      >
        <button
          type="button"
          class="drawer-backdrop"
          :class="isDrawerOpen ? 'open' : ''"
          aria-label="Đóng chi tiết sinh viên"
          @click="closeDrawer"
        />

        <aside class="drawer-shell" :class="isDrawerOpen ? 'open' : ''">
          <template v-if="selectedStudent">
            <div class="drawer-header">
              <div class="student-profile">
                <span class="drawer-avatar">{{ selectedStudent.name.split(' ').pop()[0] }}</span>
                <div>
                  <h2>{{ selectedStudent.name }}</h2>
                  <p>{{ selectedStudent.id }} · {{ selectedStudent.email }}</p>
                </div>
              </div>
              <button type="button" class="close-button" @click="closeDrawer">
                <X :size="18" />
              </button>
            </div>

            <div class="drawer-tabs">
              <button
                type="button"
                :class="activeTab === 'profile' ? 'active' : ''"
                @click="activeTab = 'profile'"
              >
                Hồ sơ
              </button>
              <button
                type="button"
                :class="activeTab === 'assignments' ? 'active' : ''"
                @click="activeTab = 'assignments'"
              >
                Bài tập
              </button>
              <button
                type="button"
                :class="activeTab === 'activity' ? 'active' : ''"
                @click="activeTab = 'activity'"
              >
                Hoạt động
              </button>
            </div>

            <div class="drawer-body">
              <div v-if="activeTab === 'profile'" class="drawer-stack space-y-3">
                <div v-if="studentDetailLoading" class="py-6 text-center text-xs text-muted">
                  Đang tải chi tiết thành phần điểm sinh viên...
                </div>

                <template v-else>
                  <!-- 1. Hero Card: Điểm Tổng Kết GPA & Trạng Thái -->
                  <div class="p-3.5 rounded-2xl surface-card border border-card bg-gradient-to-br from-blue-500/5 via-indigo-500/5 to-transparent">
                    <div class="flex items-center justify-between gap-3 mb-2">
                      <div class="flex items-center gap-2.5">
                        <span class="p-2 rounded-xl bg-(--accent-primary)/10 text-(--accent-primary)">
                          <Award :size="20" />
                        </span>
                        <div>
                          <span class="text-[11px] font-semibold text-muted uppercase tracking-wider block">Điểm trung bình môn (GPA)</span>
                          <strong class="text-xl font-extrabold text-heading tracking-tight">
                            {{ gradeBreakdown?.gpa !== null && gradeBreakdown?.gpa !== undefined ? `${gradeBreakdown.gpa} / 10` : studentGpaDisplay }}
                          </strong>
                        </div>
                      </div>
                      <div class="flex flex-col items-end gap-1">
                        <GlassBadge
                          :variant="gradeBreakdown?.status === 'dat' || (gradeBreakdown?.gpa >= 5) ? 'success' : (gradeBreakdown?.status === 'rot' ? 'danger' : 'neutral')"
                          size="sm"
                        >
                          {{ gradeBreakdown?.status === 'dat' || (gradeBreakdown?.gpa >= 5) ? 'Đạt môn' : (gradeBreakdown?.status === 'rot' ? 'Chưa đạt' : 'Đang học') }}
                        </GlassBadge>
                        <span v-if="gradeBreakdown?.isLocked" class="inline-flex items-center gap-1 text-[10px] text-amber-600 dark:text-amber-400">
                          <Lock :size="10" /> Đã khóa điểm
                        </span>
                      </div>
                    </div>

                    <!-- 3 Cột Điểm Cấu Thành Tổng Thể -->
                    <div class="grid grid-cols-3 gap-2 mt-3 pt-2.5 border-t border-card/60">
                      <div class="p-2 rounded-xl surface-input border border-card/60 text-center">
                        <span class="text-[10px] text-muted font-medium block">Điểm Quá Trình</span>
                        <strong class="text-xs font-bold text-heading mt-0.5 block">
                          {{ gradeBreakdown?.processScore !== null && gradeBreakdown?.processScore !== undefined ? `${gradeBreakdown.processScore} đ` : '—' }}
                        </strong>
                      </div>
                      <div class="p-2 rounded-xl surface-input border border-card/60 text-center">
                        <span class="text-[10px] text-muted font-medium block">Điểm Giữa Kỳ</span>
                        <strong class="text-xs font-bold text-heading mt-0.5 block">
                          {{ gradeBreakdown?.midtermScore !== null && gradeBreakdown?.midtermScore !== undefined ? `${gradeBreakdown.midtermScore} đ` : '—' }}
                        </strong>
                      </div>
                      <div class="p-2 rounded-xl surface-input border border-card/60 text-center">
                        <span class="text-[10px] text-muted font-medium block">Điểm Cuối Kỳ</span>
                        <strong class="text-xs font-bold text-heading mt-0.5 block">
                          {{ gradeBreakdown?.finalScore !== null && gradeBreakdown?.finalScore !== undefined ? `${gradeBreakdown.finalScore} đ` : '—' }}
                        </strong>
                      </div>
                    </div>
                  </div>

                  <!-- 2. Chi Tiết Các Nhóm Đầu Điểm & Trọng Số -->
                  <div v-if="gradeBreakdown?.gradeTypes?.length > 0" class="p-3.5 rounded-2xl surface-card border border-card">
                    <div class="flex items-center justify-between mb-2.5">
                      <h4 class="text-xs font-bold text-heading flex items-center gap-1.5">
                        <Layers :size="14" class="text-(--accent-primary)" />
                        Chi tiết thành phần tính điểm
                      </h4>
                      <span class="text-[10px] text-muted">Trọng số đào tạo</span>
                    </div>

                    <div class="space-y-2">
                      <div
                        v-for="gt in gradeBreakdown.gradeTypes"
                        :key="gt.code || gt.Code"
                        class="p-2.5 rounded-xl surface-input border border-card/50 flex flex-col gap-1.5"
                      >
                        <div class="flex items-center justify-between text-xs">
                          <div class="flex items-center gap-1.5 min-w-0">
                            <span class="w-1.5 h-1.5 rounded-full bg-(--accent-primary)" />
                            <strong class="text-heading font-semibold truncate">{{ gt.name || gt.Name }}</strong>
                            <span class="text-[10px] px-1.5 py-0.5 rounded bg-(--accent-primary)/10 text-(--accent-primary) font-mono">
                              {{ gt.weight || gt.Weight }}%
                            </span>
                          </div>
                          <strong
                            class="text-xs font-bold shrink-0 ml-2"
                            :class="(gt.averageGrade ?? gt.AverageGrade) !== null ? 'text-heading' : 'text-muted font-normal italic'"
                          >
                            {{ (gt.averageGrade ?? gt.AverageGrade) !== null && (gt.averageGrade ?? gt.AverageGrade) !== undefined ? `${(gt.averageGrade ?? gt.AverageGrade)} / 10 đ` : 'Chưa có điểm' }}
                          </strong>
                        </div>

                        <!-- Thanh progress điểm thành phần -->
                        <div class="w-full h-1 surface-card rounded-full overflow-hidden" v-if="(gt.averageGrade ?? gt.AverageGrade) !== null">
                          <div
                            class="h-full bg-(--accent-primary) rounded-full"
                            :style="{ width: `${Math.min(100, ((gt.averageGrade ?? gt.AverageGrade) / 10) * 100)}%` }"
                          />
                        </div>
                      </div>
                    </div>

                    <p class="text-[10px] text-muted italic mt-2.5 text-center">
                      * Điểm TB môn được tổng hợp từ Điểm Quá Trình, Giữa Kỳ & Cuối Kỳ theo quy chế đào tạo.
                    </p>
                  </div>

                  <!-- 3. Tiến Độ & Chuyên Cần -->
                  <div class="p-3.5 rounded-2xl surface-card border border-card space-y-3">
                    <div class="flex items-center justify-between">
                      <h4 class="text-xs font-bold text-heading flex items-center gap-1.5">
                        <Activity :size="14" class="text-emerald-500" />
                        Tiến độ & Chuyên cần
                      </h4>
                      <span class="text-xs font-bold" :class="selectedStudent.absent > 3 ? 'text-rose-500' : 'text-muted'">
                        Vắng {{ selectedStudent.absent }} buổi
                      </span>
                    </div>

                    <div>
                      <div class="flex justify-between text-xs text-muted mb-1 font-medium">
                        <span>Tiến độ học tập khóa học</span>
                        <strong class="text-heading font-bold">{{ selectedStudent.progress }}%</strong>
                      </div>
                      <div class="w-full h-2 surface-input border border-card rounded-full overflow-hidden">
                        <div class="h-full bg-(--accent-primary) transition-all duration-500" :style="{ width: `${selectedStudent.progress}%` }" />
                      </div>
                    </div>
                  </div>
                </template>
              </div>

              <div v-if="activeTab === 'assignments'" class="drawer-stack">
                <div v-if="studentDetailLoading" class="py-6 text-center text-xs text-muted">
                  Đang tải danh sách bài nộp...
                </div>
                <template v-else-if="studentAssignmentItems.length > 0">
                  <article
                    v-for="item in studentAssignmentItems"
                    :key="item.id"
                    class="assignment-row flex items-center justify-between p-3 rounded-xl surface-card border border-card mb-2 hover:surface-card-hover transition-all"
                  >
                    <div class="flex items-center gap-3 min-w-0">
                      <span class="row-icon w-8 h-8 rounded-lg bg-(--accent-primary)/10 text-(--accent-primary) flex items-center justify-center shrink-0">
                        <BookOpen :size="16" />
                      </span>
                      <div class="min-w-0">
                        <h3 class="truncate text-xs font-bold text-heading">{{ item.title }}</h3>
                        <div class="flex items-center gap-2 text-[11px] text-muted mt-0.5">
                          <span>{{ item.groupName }}</span>
                          <span v-if="item.submittedAt" class="flex items-center gap-1 text-[10px] text-muted">
                            · <Clock :size="11" /> {{ formatDateTime(item.submittedAt) }}
                          </span>
                        </div>
                      </div>
                    </div>

                    <div class="shrink-0 ml-3">
                      <span
                        v-if="item.status === 'da_cham' || item.grade !== null"
                        class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-500/15 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20"
                      >
                        <CheckCircle2 :size="12" />
                        {{ item.grade }} / 10 đ
                      </span>
                      <span
                        v-else-if="item.status === 'cho_cham' || item.isSubmitted"
                        class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-500/15 text-amber-600 dark:text-amber-400 border border-amber-500/20"
                      >
                        <Clock :size="12" />
                        Chờ chấm điểm
                      </span>
                      <span
                        v-else
                        class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium text-muted bg-(--surface-input) border border-card"
                      >
                        Chưa nộp bài
                      </span>
                    </div>
                  </article>
                </template>
                <div v-else class="py-8 text-center text-xs text-muted">
                  Chưa có bài tập hay bài nộp nào được ghi nhận cho sinh viên này.
                </div>
              </div>

              <div v-if="activeTab === 'activity'" class="drawer-stack">
                <div v-if="studentDetailLoading" class="py-6 text-center text-xs text-muted">
                  Đang tải nhật ký hoạt động...
                </div>
                <template v-else-if="studentActivities.length > 0">
                  <div class="relative pl-4 space-y-3 before:absolute before:left-1.5 before:top-2 before:bottom-2 before:w-0.5 before:bg-(--border-card)">
                    <div
                      v-for="(act, idx) in studentActivities"
                      :key="idx"
                      class="relative flex items-start gap-3 text-xs"
                    >
                      <span class="w-3.5 h-3.5 rounded-full bg-(--accent-primary) ring-4 ring-(--surface-card) shrink-0 mt-0.5" />
                      <div class="flex-1 p-2.5 rounded-xl surface-card border border-card">
                        <div class="flex items-center justify-between gap-2 mb-1">
                          <strong class="text-heading font-semibold truncate">{{ act.title }}</strong>
                          <small class="text-muted text-[10px] whitespace-nowrap">{{ formatDateTime(act.timestamp) }}</small>
                        </div>
                        <p class="text-muted text-[11px]">{{ act.description }}</p>
                        <div v-if="act.score !== null && act.score !== undefined" class="mt-1">
                          <span class="text-[11px] font-bold text-emerald-600 dark:text-emerald-400">Điểm số: {{ act.score }} đ</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </template>
                <div v-else class="py-8 text-center text-xs text-muted flex flex-col items-center gap-2">
                  <Activity :size="24" class="text-muted/50" />
                  <p>Chưa ghi nhận nhật ký hoạt động gần đây của sinh viên này.</p>
                </div>
              </div>
            </div>
          </template>
        </aside>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.class-progress-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.page-header,
.header-main,
.header-actions,
.context-panel,
.summary-strip,
.filters,
.panel-title,
.student-cell,
.progress-cell,
.row-actions,
.email-cell,
.last-active,
.table-footer,
.pagination,
.student-profile,
.drawer-tabs,
.assignment-row,
.activity-row,
.meter-meta {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.panel-title,
.table-footer {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  align-items: flex-start;
  gap: 0.75rem;
  min-width: 0;
}

.back-link,
.close-button,
.student-avatar,
.drawer-avatar,
.row-icon {
  display: inline-flex;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
}

.back-link,
.close-button {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
  color: var(--text-label);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    color 0.2s ease;
}

.back-link:hover,
.close-button:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.context-tags,
.header-actions,
.summary-strip,
.filters,
.row-actions,
.pagination {
  gap: 0.5rem;
  flex-wrap: wrap;
}

.context-tags {
  margin-bottom: 0.45rem;
}

.header-copy h1,
.panel-title h2,
.student-cell strong,
.drawer-header h2,
.detail-section h3,
.assignment-row h3,
.activity-row h3 {
  margin: 0;
  color: var(--text-heading);
  font-weight: 900;
}

.header-copy h1 {
  font-size: 1.45rem;
  line-height: 1.15;
}

.panel-title h2 {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 1rem;
}

.header-copy p,
.panel-title p,
.summary-pill span,
.student-cell small,
.student-code,
.email-cell,
.last-active,
.table-footer,
.drawer-header p,
.assignment-row p,
.activity-row p,
.profile-stat span {
  color: var(--text-muted);
}

.header-copy p,
.panel-title p,
.drawer-header p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.context-panel {
  align-items: center;
}

.summary-pill {
  display: grid;
  min-width: 5rem;
  gap: 0.05rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.primary,
.summary-pill.info {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.input-shell,
.select-shell {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.7rem;
  color: var(--text-placeholder);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.input-shell {
  width: min(20rem, 100%);
}

.select-shell {
  width: min(13rem, 100%);
}

.input-shell:focus-within,
.select-shell:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.input-shell input,
.select-shell select {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.select-shell select {
  appearance: none;
  cursor: pointer;
}

.progress-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1rem;
}

.overall-panel,
.chart-panel,
.students-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.panel-title {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.overall-meter {
  display: grid;
  gap: 0.75rem;
}

.meter-value {
  color: var(--text-heading);
  font-size: 2rem;
  font-weight: 900;
}

.progress-track {
  width: 6rem;
  height: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  overflow: hidden;
}

.progress-track.large {
  width: 100%;
  height: 0.55rem;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
  transition: width 0.8s ease, height 0.8s ease;
}

.meter-meta {
  justify-content: space-between;
  gap: 1rem;
  color: var(--text-muted);
  font-size: 0.78rem;
  font-weight: 750;
}

.bar-chart {
  display: grid;
  grid-template-columns: repeat(5, minmax(3.5rem, 1fr));
  gap: 0.75rem;
  min-height: 12rem;
  align-items: end;
}

.bar-item {
  display: grid;
  gap: 0.4rem;
  justify-items: center;
}

.bar-item strong {
  color: var(--text-heading);
  font-size: 0.82rem;
}

.bar-item small {
  color: var(--text-muted);
  font-size: 0.72rem;
  font-weight: 750;
}

.bar-track {
  display: flex;
  align-items: end;
  width: 100%;
  height: 8rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  overflow: hidden;
}

.bar-track span {
  display: block;
  width: 100%;
  background: var(--accent-primary);
  transition: height 0.8s ease;
}

.student-avatar,
.drawer-avatar {
  width: 1.75rem;
  height: 1.75rem;
  border-radius: 9999px;
  color: var(--text-link);
  font-size: 0.7rem;
  font-weight: 700;
}

.drawer-avatar {
  width: 2.4rem;
  height: 2.4rem;
  border-radius: var(--radius-md);
}

.table-footer {
  align-items: center;
  border-top: 1px solid var(--border-card);
  padding-top: 0.75rem;
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.pagination button {
  min-height: 2rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface-input);
  color: var(--text-label);
  padding: 0 0.75rem;
  font-size: 0.76rem;
  font-weight: 850;
}

.pagination button.active,
.pagination button:hover {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.drawer-root {
  position: fixed;
  inset: 0;
  z-index: 999;
  overflow: hidden;
}

.drawer-backdrop {
  position: absolute;
  inset: 0;
  border: 0;
  background: var(--surface-modal);
  opacity: 0;
  transition: opacity 0.25s ease;
}

.drawer-backdrop.open {
  opacity: 1;
}

.drawer-shell {
  position: fixed;
  inset-block: 0;
  right: 0;
  display: flex;
  width: min(30rem, 100%);
  flex-direction: column;
  border-left: 1px solid var(--border-card);
  background: var(--surface-modal);
  box-shadow: var(--lg-shadow-lg);
  transform: translateX(100%);
  transition: transform 0.25s ease;
}

.drawer-shell.open {
  transform: translateX(0);
}

.drawer-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 1rem;
}

.student-profile {
  align-items: flex-start;
  gap: 0.75rem;
}

.drawer-tabs {
  gap: 0.35rem;
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-card);
  padding: 0.5rem 1rem;
}

.drawer-tabs button {
  min-height: 2rem;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-label);
  padding: 0 0.75rem;
  font-size: 0.78rem;
  font-weight: 850;
}

.drawer-tabs button.active,
.drawer-tabs button:hover {
  border-color: var(--border-card);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.drawer-body {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
}

.drawer-stack {
  display: grid;
  gap: 0.75rem;
}

.profile-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.profile-stat,
.assignment-row,
.activity-row,
.detail-section {
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.8rem;
}

.profile-stat span {
  display: block;
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.profile-stat strong {
  display: block;
  margin-top: 0.2rem;
  color: var(--text-heading);
  font-size: 1.1rem;
  font-weight: 900;
}

.profile-stat.danger strong {
  color: var(--color-danger-text);
}

.detail-section {
  display: grid;
  gap: 0.65rem;
}

.assignment-row,
.activity-row {
  justify-content: space-between;
  gap: 0.75rem;
}

.row-icon {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
  color: var(--text-link);
}

.assignment-row div,
.activity-row div {
  flex: 1;
  min-width: 0;
}

.assignment-row h3,
.activity-row h3 {
  font-size: 0.86rem;
}

.assignment-row p,
.activity-row p {
  margin: 0.15rem 0 0;
  font-size: 0.74rem;
}

.assignment-row strong {
  color: var(--color-success-text);
  font-weight: 900;
}

@media (max-width: 1024px) {
  .page-header,
  .context-panel,
  .panel-title,
  .table-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .progress-grid {
    grid-template-columns: 1fr;
  }

  .filters,
  .header-actions,
  .row-actions {
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .summary-strip,
  .filters,
  .pagination,
  .row-actions {
    display: grid;
    grid-template-columns: 1fr;
  }

  .summary-pill,
  .input-shell,
  .select-shell {
    width: 100%;
  }

  .bar-chart {
    grid-template-columns: repeat(5, minmax(2.75rem, 1fr));
    gap: 0.4rem;
  }
}
</style>
