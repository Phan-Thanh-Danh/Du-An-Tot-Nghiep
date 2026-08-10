<template>
  <div v-if="loading" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải dashboard...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <button @click="loadDashboard" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white">Thử lại</button>
  </div>
  <div v-else class="space-y-5 pb-10">

    <!-- ── Welcome Header ── -->
    <div class="rounded-2xl surface-card border border-card p-5 shadow-lg">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h1 class="text-lg font-semibold leading-tight tracking-tight text-heading">
            Chào buổi sáng, <span class="text-link">{{ auth.user?.fullName || auth.user?.name || 'Giảng viên' }}!</span>
          </h1>
          <p class="mt-1 text-muted text-sm">
            Hôm nay có {{ teachingSchedule.length }} ca dạy và {{ pendingGradingCount }} bài đang chờ chấm.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/teacher/schedule" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white shadow-lg hover:opacity-90 transition-all active:scale-95">
              Xem lịch dạy
            </router-link>
            <router-link to="/teacher/grading-input" class="rounded-lg bg-(--accent-primary)/10 px-4 py-2 text-xs font-bold text-link border border-(--accent-primary)/30 hover:bg-(--accent-primary)/20 transition-all">
              Chấm điểm ngay
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-20 w-20 items-center justify-center rounded-2xl surface-card border border-card">
            <BookOpen :size="32" class="text-link/60" />
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
          <div v-if="item.trend" :class="['flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold', item.isNegative ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="11" />
            <AlertCircle v-else :size="11" />
          </div>
        </div>
        <div class="mt-4">
          <p class="text-sm font-medium text-label">{{ item.label }}</p>
          <p class="mt-0.5 text-xl font-semibold text-heading">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-5">

      <!-- Left (2/3) -->
      <div class="xl:col-span-2 space-y-5">

        <!-- Today's Schedule -->
        <div class="lg-glass-soft rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-4 py-3">
            <div>
              <h2 class="text-base font-bold text-heading">Lịch dạy hôm nay</h2>
              <p class="text-xs text-muted mt-0.5">Các lớp học bạn phụ trách trong ngày</p>
            </div>
            <router-link to="/teacher/schedule" class="text-xs font-bold text-link">Xem tất cả</router-link>
          </div>
          <div class="p-3 space-y-2">
            <div v-if="teachingSchedule.length === 0" class="rounded-xl border border-card p-4 text-center text-xs text-muted">
              Không có ca dạy nào hôm nay.
            </div>
            <div v-for="item in teachingSchedule" :key="item.id"
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 rounded-xl border border-card p-3 transition-all hover:border-(--accent-primary)/30 hover:bg-(--accent-primary)/5">
              <div class="flex h-9 w-9 flex-shrink-0 flex-col items-center justify-center rounded-lg bg-(--accent-primary)/10 text-link font-bold border border-(--accent-primary)/20">
                <span class="text-[8px] font-bold uppercase tracking-tighter leading-tight">{{ item.timeLabel }}</span>
                <span class="text-[8px] font-semibold leading-tight">{{ item.period }}</span>
              </div>
              <div class="flex-1 min-w-0">
                <h3 class="text-sm font-bold text-heading truncate group-hover:text-link transition-colors">{{ item.subject }}</h3>
                <div class="flex items-center gap-2 mt-0.5">
                  <span class="text-[11px] text-label font-medium">{{ item.code }}</span>
                  <span class="h-1 w-1 rounded-full bg-(--border-default)"></span>
                  <span class="text-[11px] text-label font-medium">{{ item.room }}</span>
                </div>
              </div>
              <div class="flex items-center gap-3 mt-1 sm:mt-0">
                <GlassBadge :variant="item.status === 'completed' ? 'neutral' : 'primary'">
                  {{ item.statusLabel }}
                </GlassBadge>
              </div>
            </div>
          </div>
        </div>

        <!-- Submission Progress -->
        <div class="lg-glass-soft rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-4 py-3">
            <h2 class="text-base font-bold text-heading">Tiến độ nộp bài tập</h2>
            <span class="text-xs font-bold text-muted">{{ currentTermLabel }}</span>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-x divide-(--border-card)">
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Tỷ lệ nộp bài</h3>
                <GlassBadge :variant="submissionRate >= 60 ? 'success' : 'warning'">{{ submissionRate }}%</GlassBadge>
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
              <router-link to="/teacher/grading-input" class="mt-3 inline-block text-xs font-bold text-link">Xem chi tiết →</router-link>
            </div>
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Bài chưa chấm</h3>
                <GlassBadge :variant="pendingGradingCount > 0 ? 'warning' : 'neutral'">{{ pendingGradingCount }} bài</GlassBadge>
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
              <router-link to="/teacher/grading-input" class="mt-3 inline-block text-xs font-bold text-link">Đi chấm ngay →</router-link>
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
            <GlassBadge :variant="newSubmissionCount > 0 ? 'warning' : 'neutral'">{{ newSubmissionCount }} chờ chấm</GlassBadge>
          </div>
          <div class="space-y-2">
            <div v-if="recentSubmissions.length === 0" class="rounded-lg border border-card surface-card p-3 text-center text-[10px] text-muted">
              Chưa có bài nộp mới.
            </div>
            <div v-for="sub in recentSubmissions" :key="sub.id"
                 class="flex items-start gap-2 rounded-lg border border-card surface-card p-2.5 transition-all hover:shadow-md group">
              <div class="mt-0.5 h-8 w-8 shrink-0 rounded-lg bg-(--accent-primary)/10 flex items-center justify-center text-link">
                <User :size="14" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight">{{ sub.student }}</p>
                  <span v-if="sub.status === 'new'" class="text-[9px] font-bold text-link">NEW</span>
                </div>
                <p class="mt-0.5 text-[10px] text-label truncate">{{ sub.assignment }} · {{ sub.course }}</p>
                <p class="text-[9px] text-muted mt-0.5">{{ sub.time }}</p>
              </div>
            </div>
          </div>
          <router-link to="/teacher/grading-input" class="mt-3 block w-full rounded-lg bg-(--accent-primary)/10 py-2 text-center text-[10px] font-bold text-link hover:bg-(--accent-primary)/20 transition-colors">Xem tất cả bài nộp</router-link>
        </div>

        <!-- Teaching Stats -->
        <div class="rounded-2xl p-4 text-white overflow-hidden relative" style="background:var(--accent-primary);">
          <h3 class="text-sm font-bold">Thống kê giảng dạy</h3>
          <p class="text-xs opacity-70 mt-0.5">{{ currentTermLabel }}.</p>

          <div class="mt-4 grid grid-cols-2 gap-2">
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Lớp đang dạy</p>
              <p class="text-base font-semibold mt-0.5">{{ assignedClassCount }}</p>
            </div>
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Ca dạy tuần này</p>
              <p class="text-base font-semibold mt-0.5">{{ weeklyShiftCount }}</p>
            </div>
          </div>
        </div>

        <!-- Announcements -->
        <div class="lg-glass-soft rounded-2xl p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-sm font-bold text-heading">Thông báo</h3>
            <Bell :size="14" class="text-muted" />
          </div>
          <div class="space-y-2">
            <div v-if="notifications.length === 0" class="rounded-lg border border-card surface-card p-3 text-center text-[10px] text-muted">
              Chưa có thông báo mới.
            </div>
            <div v-for="n in notifications" :key="n.maThongBao" class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text) shrink-0">
                <Users :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-body">{{ n.tieuDe }}</p>
                <p class="text-[10px] text-label mt-0.5">{{ n.tomTat || n.tomTatNoiDung || '' }} · {{ formatDate(n.ngayTao) }}</p>
              </div>
            </div>
          </div>
          <router-link to="/teacher/notifications" class="mt-3 block w-full rounded-lg bg-(--surface-input) py-2 text-center text-[10px] font-bold text-muted hover:opacity-80 transition-colors">Tất cả thông báo</router-link>
        </div>

      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { teacherApi } from '@/services/teacherApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import {
  Users, BookOpen, ClipboardCheck, TrendingUp,
  ArrowUpRight, AlertCircle, User, Bell, Calendar
} from 'lucide-vue-next'

const auth = useAuthStore()

const loading = ref(false)
const error = ref('')
const stats = ref([])
const teachingSchedule = ref([])
const submissionStats = ref([])
const recentSubmissions = ref([])
const gradingStats = ref([])
const notifications = ref([])
const pendingGradingCount = ref(0)
const submissionRate = ref(0)
const assignedClassCount = ref(0)
const weeklyShiftCount = ref(0)
const currentTermLabel = ref('Học kỳ hiện tại')
const newSubmissionCount = ref(0)

function formatDate(value) {
  if (!value) return ''
  const d = new Date(value)
  return isNaN(d.getTime()) ? '' : d.toLocaleDateString('vi-VN')
}

async function loadDashboard() {
  loading.value = true
  error.value = ''
  try {
    const [dashboardData, summaryData, todayData, assignmentsRes, notifRes] = await Promise.all([
      teacherApi.getDashboard(),
      teacherApi.getScheduleSummary(),
      teacherApi.getTodaySchedule(),
      teacherApi.getAssignments({ pageIndex: 1, pageSize: 100 }),
      apiRequest('/api/notifications/me?pageIndex=1&pageSize=5').catch(() => null),
    ])

    const data = dashboardData
    pendingGradingCount.value = data?.pendingGrading ?? 0
    assignedClassCount.value = summaryData?.assignedClassCount ?? 0
    weeklyShiftCount.value = summaryData?.weeklyShiftCount ?? 0
    currentTermLabel.value = summaryData?.currentTerm?.tenHocKy || 'Học kỳ hiện tại'

    const sessions = Array.isArray(todayData) ? todayData : []
    teachingSchedule.value = sessions.map(s => {
      const startHour = parseInt((s.gioBatDau || '00:00').substring(0, 2), 10)
      const period = startHour >= 12 ? 'PM' : 'AM'
      return {
        id: s.maBuoiHoc,
        timeLabel: `${(s.gioBatDau || '').substring(0, 5)}`,
        period,
        subject: s.tenMonHoc,
        code: s.tenLop,
        room: s.tenPhong,
        status: s.trangThaiBuoi === 'da_huy' ? 'cancelled' : (s.trangThaiBuoi === 'da_ket_thuc' ? 'completed' : 'upcoming'),
        statusLabel: s.trangThaiBuoi === 'da_huy' ? 'Đã hủy' : (s.trangThaiBuoi === 'da_ket_thuc' ? 'Đã hoàn thành' : 'Sắp diễn ra')
      }
    })

    stats.value = [
      { id: 1, label: 'Ca dạy hôm nay', value: sessions.length, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: Calendar },
      { id: 2, label: 'Lớp đang phụ trách', value: assignedClassCount.value, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: BookOpen },
      { id: 3, label: 'Bài chờ chấm', value: pendingGradingCount.value, trend: pendingGradingCount.value > 0 ? 'cần xử lý' : '', isNegative: pendingGradingCount.value > 0, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: ClipboardCheck },
      { id: 4, label: 'Ca dạy tuần này', value: weeklyShiftCount.value, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: TrendingUp },
    ]

    recentSubmissions.value = (data?.recentSubmissions || []).map(s => ({
      id: s.submissionId || s.id,
      student: s.studentName || s.student || '',
      course: s.courseName || s.course || '',
      assignment: s.assignmentTitle || s.assignment || '',
      time: s.submittedAt
        ? new Date(s.submittedAt).toLocaleDateString('vi-VN')
        : (s.time || ''),
      status: s.status === 'moi' || s.status === 'cho_cham' ? 'new' : 'graded'
    }))
    newSubmissionCount.value = recentSubmissions.value.filter(s => s.status === 'new').length

    const assignmentsData = unwrapApiData(assignmentsRes)
    const assignments = assignmentsData?.items || (Array.isArray(assignmentsData) ? assignmentsData : [])
    let totalSubmissions = 0
    let totalStudents = 0
    let totalPending = 0
    for (const a of assignments) {
      totalSubmissions += a.submissionsCount || 0
      totalStudents += a.totalStudents || 0
      totalPending += a.pendingGrades || 0
    }
    submissionRate.value = totalStudents > 0 ? Math.round((totalSubmissions / totalStudents) * 100) : 0
    submissionStats.value = [
      { label: 'Đã nộp', value: totalSubmissions, colorClass: 'bg-(--color-success-text)' },
      { label: 'Chưa nộp', value: Math.max(totalStudents - totalSubmissions, 0), colorClass: 'bg-(--color-warning-text)' },
    ]
    gradingStats.value = [
      { label: 'Đã chấm', value: Math.max(totalSubmissions - totalPending, 0), colorClass: 'bg-(--color-success-text)' },
      { label: 'Chờ chấm', value: totalPending, colorClass: 'bg-(--color-warning-text)' },
    ]

    const notifData = notifRes ? unwrapApiData(notifRes) : null
    notifications.value = (notifData?.items || (Array.isArray(notifData) ? notifData : [])).slice(0, 5)
  } catch (e) {
    error.value = e?.message || 'Không thể tải dữ liệu dashboard.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadDashboard()
})
</script>
