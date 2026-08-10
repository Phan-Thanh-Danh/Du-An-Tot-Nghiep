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
            Hôm nay có {{ teachingSchedule.length }} ca dạy và {{ (stats[2]?.value || 0) }} bài đang chờ chấm.
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/teacher/schedule" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white shadow-lg hover:opacity-90 transition-all active:scale-95">
              Xem lịch dạy
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

    <!-- ── Stats Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id"
           class="lg-glass-soft group relative overflow-hidden rounded-[20px] p-5 transition-all hover:shadow-lg hover:-translate-y-0.5">
        <div class="flex items-center justify-between">
          <div :class="['flex h-11 w-11 items-center justify-center rounded-xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="22" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold', item.isNegative ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            {{ item.trend || 'Ổn định' }}
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
            <div v-if="teachingSchedule.length === 0" class="py-8 text-center text-muted text-xs">
              Hôm nay bạn không có ca dạy nào.
            </div>
            <div v-for="item in teachingSchedule" :key="item.id"
                 class="group flex flex-col sm:flex-row items-start sm:items-center gap-3 rounded-xl border border-card p-3 transition-all hover:border-(--accent-primary)/30 hover:bg-(--accent-primary)/5">
              <div class="flex h-9 w-9 flex-shrink-0 flex-col items-center justify-center rounded-lg bg-(--accent-primary)/10 text-link font-bold border border-(--accent-primary)/20">
                <span class="text-[8px] font-bold uppercase tracking-tighter leading-tight">{{ item.time.split(' ')[0] }}</span>
                <span class="text-[8px] font-semibold leading-tight">CA</span>
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
                  {{ item.status === 'completed' ? 'Đã hoàn thành' : 'Sắp diễn ra' }}
                </GlassBadge>
              </div>
            </div>
          </div>
        </div>

        <!-- Submission Progress -->
        <div class="lg-glass-soft rounded-2xl overflow-hidden">
          <div class="flex items-center justify-between border-b border-card px-4 py-3">
            <h2 class="text-base font-bold text-heading">Tiến độ nộp bài tập (Tuần này)</h2>
            <router-link to="/teacher/submissions" class="text-xs font-bold text-link">Quản lý bài tập</router-link>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 divide-x divide-(--border-card)">
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Tỷ lệ nộp bài</h3>
                <GlassBadge variant="success">85%</GlassBadge>
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
            </div>
            <div class="p-3">
              <div class="flex items-center justify-between mb-3">
                <h3 class="text-xs font-bold text-body">Bài chưa chấm</h3>
                <GlassBadge variant="warning">{{ stats[2]?.value || 0 }} bài</GlassBadge>
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
            <GlassBadge variant="warning">{{ recentSubmissions.length }} bài</GlassBadge>
          </div>
          <div class="space-y-2">
            <div v-if="recentSubmissions.length === 0" class="py-4 text-center text-xs text-muted">Chưa có bài nộp mới</div>
            <div v-for="sub in recentSubmissions" :key="sub.id"
                 @click="$router.push('/teacher/submissions')"
                 class="flex items-start gap-2 rounded-lg border border-card surface-card p-2.5 transition-all hover:shadow-md cursor-pointer group">
              <div class="mt-0.5 h-8 w-8 shrink-0 rounded-lg bg-(--accent-primary)/10 flex items-center justify-center text-link">
                <User :size="14" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight group-hover:text-link transition-colors">{{ sub.student }}</p>
                  <span v-if="sub.status === 'new'" class="text-[9px] font-bold text-link">NEW</span>
                </div>
                <p class="mt-0.5 text-[10px] text-label truncate">{{ sub.assignment }} · {{ sub.course }}</p>
                <p class="text-[9px] text-muted mt-0.5">{{ sub.time }}</p>
              </div>
            </div>
          </div>
          <button @click="$router.push('/teacher/submissions')" class="mt-3 w-full rounded-lg bg-(--accent-primary)/10 py-2 text-[10px] font-bold text-link hover:bg-(--accent-primary)/20 transition-colors">Xem tất cả bài nộp</button>
        </div>

        <!-- Teaching Stats & GPA Performance Bar Chart -->
        <div class="rounded-2xl p-4 text-white overflow-hidden relative shadow-lg" style="background: linear-gradient(135deg, #1e40af 0%, #3b82f6 100%);">
          <div class="flex justify-between items-start">
            <div>
              <h3 class="text-sm font-bold">Thống kê giảng dạy</h3>
              <p class="text-xs opacity-80 mt-0.5">Điểm GPA trung bình từng lớp học phần</p>
            </div>
            <span class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-white/20 border border-white/30 backdrop-blur-xs">
              {{ overallGpa.level }}
            </span>
          </div>

          <!-- Rating Scale & Bar Chart Grid -->
          <div class="mt-4 flex items-stretch gap-3">
            <!-- Left Level Indicator -->
            <div class="flex flex-col justify-between text-[9px] font-bold opacity-80 py-1 shrink-0">
              <span class="text-emerald-300">Cao (≥8)</span>
              <span class="text-sky-200">Khá (6.5)</span>
              <span class="text-amber-200">TB (5.0)</span>
              <span class="text-rose-200">Kém (&lt;5)</span>
            </div>

            <!-- Bar Chart Area -->
            <div class="flex-1 flex items-end justify-between gap-2 h-24 border-b border-white/20 pb-1">
              <div v-for="cls in classGpaList" :key="cls.code"
                   class="relative flex-1 group flex flex-col items-center h-full justify-end cursor-pointer">
                <!-- Bar Container -->
                <div class="w-full rounded-t-md transition-all duration-300 group-hover:brightness-125 relative"
                     :class="cls.gpa >= 8.0 ? 'bg-emerald-400' : (cls.gpa >= 6.5 ? 'bg-sky-300' : (cls.gpa >= 5.0 ? 'bg-amber-300' : 'bg-rose-400'))"
                     :style="{ height: `${(cls.gpa / 10) * 100}%` }">
                </div>
                <span class="text-[8px] font-mono font-bold mt-1 opacity-90 truncate max-w-full" :title="cls.code">{{ cls.code.split('_')[0] }}</span>

                <!-- Floating Hover Tooltip -->
                <div class="absolute bottom-full mb-2 left-1/2 -translate-x-1/2 hidden group-hover:block z-50 bg-slate-900 text-white text-[10px] rounded-lg px-2.5 py-1.5 shadow-xl whitespace-nowrap border border-slate-700 pointer-events-none">
                  <p class="font-bold text-amber-400">{{ cls.name }}</p>
                  <p class="text-[9px] text-slate-300 mt-0.5">GPA: <span class="font-bold text-white">{{ cls.gpa }} ★</span> ({{ cls.level }})</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Bottom Stat Badges -->
          <div class="mt-4 grid grid-cols-2 gap-2">
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Lớp đang dạy</p>
              <p class="text-base font-bold mt-0.5">{{ classGpaList.length }} Lớp</p>
            </div>
            <div class="rounded-lg bg-white/10 p-2 backdrop-blur-sm border border-white/10">
              <p class="text-[9px] uppercase font-bold opacity-80 tracking-wider">Hiệu suất GPA</p>
              <p class="text-base font-bold mt-0.5 text-amber-300">{{ overallGpa.score }} / 10</p>
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
            <div class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text) shrink-0">
                <Users :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-body">Họp bộ môn thường kỳ</p>
                <p class="text-[10px] text-label mt-0.5">14:00 Thứ 6, ngày 16/05 tại Phòng họp 2.</p>
              </div>
            </div>
          </div>
          <button @click="showAnnouncementsModal = true" class="mt-3 w-full rounded-lg bg-(--surface-input) py-2 text-[10px] font-bold text-heading hover:opacity-80 transition-colors">Tất cả thông báo</button>
        </div>

      </div>

    </div>

    <!-- Announcements Modal -->
    <div v-if="showAnnouncementsModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-xs">
      <div class="w-full max-w-md surface-card rounded-2xl shadow-xl border border-default p-5 space-y-4">
        <div class="flex justify-between items-center border-b border-default pb-3">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <Bell :size="18" class="text-link" /> Thông Báo Giảng Viên
          </h3>
          <button @click="showAnnouncementsModal = false" class="text-muted hover:text-heading font-bold text-sm">✕</button>
        </div>
        <div class="space-y-3 max-h-60 overflow-y-auto pr-1">
          <div class="p-3 rounded-xl bg-(--surface-input) border border-default space-y-1">
            <p class="text-xs font-bold text-heading">Họp bộ môn Công nghệ thông tin</p>
            <p class="text-[11px] text-body">14:00 Thứ 6 tới tại Phòng họp 2. Yêu cầu nạp đề cương bài giảng.</p>
            <p class="text-[9px] text-muted">Hôm nay, 08:30</p>
          </div>
          <div class="p-3 rounded-xl bg-(--surface-input) border border-default space-y-1">
            <p class="text-xs font-bold text-heading">Hạn nộp điểm giữa kỳ Block 1</p>
            <p class="text-[11px] text-body">Nhắc nhở giảng viên cập nhật sổ điểm trước 23:59 ngày Chủ Nhật.</p>
            <p class="text-[9px] text-muted">Hôm qua, 16:20</p>
          </div>
        </div>
        <div class="flex justify-end pt-2">
          <button @click="showAnnouncementsModal = false" class="px-4 py-1.5 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Đóng</button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { teacherApi } from '@/services/teacherApi'
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
const submissionStats = ref([
  { label: 'Đã nộp đúng hạn', value: '72 sinh viên', colorClass: 'bg-emerald-500' },
  { label: 'Nộp trễ hạn', value: '13 sinh viên', colorClass: 'bg-amber-500' },
  { label: 'Chưa nộp bài', value: '15 sinh viên', colorClass: 'bg-rose-500' },
])
const recentSubmissions = ref([])
const gradingStats = ref([
  { label: 'Bài tập lớn', value: '14 bài', colorClass: 'bg-blue-500' },
  { label: 'Bài Lab thực hành', value: '10 bài', colorClass: 'bg-indigo-500' },
])
const showAnnouncementsModal = ref(false)

const classGpaList = ref([
  { code: 'CTDL101_L01', name: 'Cấu trúc dữ liệu L01', gpa: 8.2, level: 'Cao' },
  { code: 'CSDL102_L02', name: 'Cơ sở dữ liệu L02', gpa: 7.5, level: 'Khá' },
  { code: 'WEB201_L01', name: 'Lập trình Web L01', gpa: 6.2, level: 'Trung bình' },
  { code: 'MOB101_L03', name: 'Lập trình Mobile L03', gpa: 4.8, level: 'Kém' },
  { code: 'PRJ301_L01', name: 'Dự án mẫu L01', gpa: 8.7, level: 'Cao' },
  { code: 'NET104_L02', name: 'Lập trình C# L02', gpa: 7.1, level: 'Khá' },
])

const overallGpa = computed(() => {
  if (!classGpaList.value.length) return { score: '0.0', level: 'Chưa có' }
  const avg = classGpaList.value.reduce((acc, cur) => acc + cur.gpa, 0) / classGpaList.value.length
  let level = 'Kém'
  if (avg >= 8.0) level = 'Cao'
  else if (avg >= 6.5) level = 'Khá'
  else if (avg >= 5.0) level = 'Trung bình'
  return { score: avg.toFixed(1), level }
})

async function loadDashboard() {
  loading.value = true
  error.value = ''
  try {
    const [dashboardData, summaryData, todayData] = await Promise.all([
      teacherApi.getDashboard().catch(() => null),
      teacherApi.getScheduleSummary().catch(() => null),
      teacherApi.getTodaySchedule().catch(() => [])
    ])
    
    const data = dashboardData || {}
    const sessions = Array.isArray(todayData) ? todayData : []
    
    teachingSchedule.value = sessions.map(s => ({
      id: s.maBuoiHoc,
      time: `${(s.gioBatDau || '').substring(0,5)} - ${(s.gioKetThuc || '').substring(0,5)}`,
      subject: s.tenMonHoc || s.subjectName || 'Bài giảng',
      code: s.tenLop || s.classCode || 'Lớp học',
      room: s.tenPhong || s.roomName || 'Phòng học',
      status: s.trangThaiBuoi === 'da_huy' ? 'cancelled' : (s.trangThaiBuoi === 'da_ket_thuc' ? 'completed' : 'upcoming')
    }))
    
    stats.value = [
      { id: 1, label: 'Ca dạy hôm nay', value: sessions.length, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: Calendar },
      { id: 2, label: 'Lớp đang phụ trách', value: summaryData?.assignedClassCount ?? classGpaList.value.length, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: BookOpen },
      { id: 3, label: 'Bài chờ chấm', value: data?.pendingGrading ?? 0, trend: '', isNegative: true, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: ClipboardCheck },
      { id: 4, label: 'Ca dạy tuần này', value: summaryData?.weeklyShiftCount ?? 0, trend: '', isNegative: false, bgColor: 'bg-(--accent-primary-soft)', iconColor: 'text-(--text-link)', icon: TrendingUp },
    ]
    
    recentSubmissions.value = (data?.recentSubmissions || []).map(s => ({
      id: s.submissionId || s.id,
      student: s.studentName || s.student || '',
      course: s.courseName || s.course || '',
      assignment: s.assignmentTitle || s.assignment || '',
      time: s.submittedAt 
        ? new Date(s.submittedAt).toLocaleDateString('vi-VN') 
        : (s.time || ''),
      status: s.status === 'moi' || s.status === 'cho_cham' || s.status === 'new' ? 'new' : 'graded'
    }))
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
