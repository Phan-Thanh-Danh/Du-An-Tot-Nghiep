<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import {
  Activity,
  AlertCircle,
  ArrowLeft,
  BookMarked,
  BookOpen,
  Clock,
  Filter,
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
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const students = ref([])
const overallProgress = ref(0)
const completedLessons = ref(0)
const totalLessons = ref(0)
const activeStudents = ref(0)
const courseName = ref('')
const className = ref('')

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
        grade: item.grade ?? item.Grade ?? null
      })
    }
  }
  return items
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
      chartData.value = []
      courseName.value = ''
      className.value = ''
      return
    }

    currentCourseId.value = Number(courseId)
    const raw = await teacherApi.getTeacherCourseProgress(courseId)
    const data = raw?.data?.data ?? raw?.data ?? raw?.Data ?? raw
    
    currentClassId.value = data?.classId ?? data?.maLop ?? 0
    students.value = (data?.students || []).map(s => ({
      id: s.maSinhVien ?? s.id,
      name: s.tenSinhVien ?? s.name ?? '',
      email: s.email ?? '',
      progress: s.tienDo ?? s.progress ?? 0,
      gpa: s.diemTB ?? s.gpa ?? 0,
      absent: s.soBuoiVang ?? s.absent ?? 0,
      status: s.trangThai ?? s.status ?? 'good',
      lastActive: s.lanTuongTacCuoi ?? s.lastActive ?? null,
    }))
    overallProgress.value = data?.tienDoChung ?? data?.overallProgress ?? 0
    completedLessons.value = data?.soBaiHoanThanh ?? data?.completedLessons ?? 0
    totalLessons.value = data?.tongSoBai ?? data?.totalLessons ?? 0
    activeStudents.value = data?.siSo ?? data?.activeStudents ?? students.value.length
    chartData.value = (data?.phanBo ?? data?.chartData ?? []).map(item => ({
      range: item.nhan ?? item.range ?? '',
      value: item.soLuong ?? item.value ?? 0,
      height: item.chieuCao ?? item.height ?? 0,
    }))
    courseName.value = data?.tenKhoaHoc ?? data?.courseName ?? ''
    className.value = data?.tenLop ?? data?.className ?? ''
  } catch (e) {
    error.value = e?.message || 'Không thể tải tiến độ khóa học.'
  } finally {
    loading.value = false
  }
  setTimeout(() => { animateProgress.value = true }, 100)
}

const summaryStats = computed(() => {
  const completed = students.value.filter((student) => (student.status === 'excellent' || student.progress >= 90)).length
  const studying = students.value.filter((student) => student.progress >= 60 && student.progress < 90).length
  const delayed = students.value.filter((student) => student.status === 'warning').length
  const risk = students.value.filter((student) => student.status === 'danger').length

  return [
    { label: 'Sĩ số', value: activeStudents.value, tone: 'primary' },
    { label: 'Tiến độ TB', value: `${overallProgress.value}%`, tone: 'success' },
    { label: 'Hoàn thành', value: completed, tone: 'success' },
    { label: 'Đang học', value: studying, tone: 'info' },
    { label: 'Chậm tiến độ', value: delayed, tone: 'warning' },
    { label: 'Nguy cơ', value: risk, tone: 'danger' },
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
  document.body.style.overflow = 'hidden' // prevent body scroll
  await loadStudentGradeDetail(studentId)
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  document.body.style.overflow = ''
  setTimeout(() => {
    if (!isDrawerOpen.value) {
      selectedStudent.value = null
      studentGradeDetail.value = null
    }
  }, 300)
}
</script>

<template>
  <div v-if="loading" class="p-4">
    <SkeletonTable :rows="6" :columns="5" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton variant="secondary" @click="loadProgress">Thử lại</GlassButton>
  </div>
  <div v-else class="class-progress-page lg-page-enter">
    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-main">
        <router-link to="/teacher/class-progress" class="back-link" aria-label="Quay lại danh sách lớp">
          <ArrowLeft :size="18" />
        </router-link>

        <div class="header-copy">
          <div class="context-tags">
            <GlassBadge variant="primary" v-if="className">{{ className }}</GlassBadge>
            <GlassBadge variant="success">Đang diễn ra</GlassBadge>
          </div>
          <h1>{{ courseName || 'Tiến độ khóa học' }}</h1>
          <p>Theo dõi tiến độ học, bài hoàn thành và sinh viên có nguy cơ chậm tiến độ của khóa học này.</p>
        </div>
      </div>

      <div class="header-actions">
        <GlassButton variant="secondary" size="sm" @click="$router.push('/teacher/lessons')">
          <template #leading>
            <BookMarked :size="16" />
          </template>
          Giáo trình
        </GlassButton>
        <GlassButton variant="primary" size="sm" @click="sendReminder">
          <template #leading>
            <Mail :size="16" />
          </template>
          Gửi nhắc nhở
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="context-panel">
      <div class="summary-strip">
        <div v-for="item in summaryStats" :key="item.label" :class="['summary-pill', item.tone]">
          <span>{{ item.label }}</span>
          <strong>{{ item.value }}</strong>
        </div>
      </div>

      <div class="filters">
        <label class="select-shell">
          <Target :size="16" />
          <select>
            <option>Tất cả trạng thái</option>
            <option>Hoàn thành tốt</option>
            <option>Đang học</option>
            <option>Chậm tiến độ</option>
            <option>Nguy cơ</option>
          </select>
        </label>
        <label class="input-shell">
          <Search :size="16" />
          <input type="text" placeholder="Tìm sinh viên, MSSV..." />
        </label>
      </div>
    </GlassPanel>

    <div class="progress-grid">
      <GlassPanel variant="flat" density="compact" class="overall-panel">
        <div class="panel-title">
          <div>
            <h2>
              <Activity :size="17" />
              Tiến độ chung
            </h2>
            <p>{{ completedLessons }}/{{ totalLessons }} bài học đã hoàn thành.</p>
          </div>
          <GlassBadge variant="primary">{{ overallProgress }}%</GlassBadge>
        </div>

        <div class="overall-meter">
          <div class="meter-value">{{ overallProgress }}%</div>
          <div class="progress-track large" aria-hidden="true">
            <span :style="{ width: animateProgress ? `${overallProgress}%` : '0%' }" />
          </div>
          <div class="meter-meta">
            <span>Đã học: {{ completedLessons }}</span>
            <span>Còn lại: {{ totalLessons - completedLessons }}</span>
          </div>
        </div>
      </GlassPanel>

      <GlassPanel variant="flat" density="compact" class="chart-panel">
        <div class="panel-title">
          <div>
            <h2>
              <Target :size="17" />
              Phân bố tiến độ
            </h2>
            <p>Mức độ hoàn thành bài học của sinh viên trong lớp.</p>
          </div>
        </div>

        <div class="bar-chart" aria-label="Phân bố tiến độ sinh viên">
          <div v-for="(item, i) in chartData" :key="i" class="bar-item">
            <strong>{{ item.value }}</strong>
            <div class="bar-track">
              <span :style="{ height: animateProgress ? `${item.height}%` : '0%' }" />
            </div>
            <small>{{ item.range }}</small>
          </div>
        </div>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" density="compact" class="students-panel">
      <div class="panel-title">
        <div>
          <h2>
            <Users :size="17" />
            Danh sách sinh viên
          </h2>
          <p>Quản lý và theo dõi chi tiết từng sinh viên trong lớp.</p>
        </div>
        <GlassButton variant="secondary" size="sm">
          <template #leading>
            <Filter :size="15" />
          </template>
          Bộ lọc
        </GlassButton>
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
            <tr v-for="sv in students" :key="sv.id" class="hover:surface-card-hover transition-colors">
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
              <td class="py-2 px-2.5 text-center whitespace-nowrap text-xs text-heading font-semibold">{{ Math.round((sv.progress / 100) * totalLessons) }}/{{ totalLessons }}</td>
              <td class="py-2 px-2.5 text-center whitespace-nowrap text-xs">
                <GlassBadge :variant="sv.gpa >= 8 ? 'success' : sv.gpa < 6 ? 'warning' : 'info'" size="sm">
                  {{ sv.gpa >= 8 ? 'Tốt' : sv.gpa < 6 ? 'Cần hỗ trợ' : 'Đạt' }}
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
              <div v-if="activeTab === 'profile'" class="drawer-stack">
                <div class="profile-grid">
                  <div class="profile-stat">
                    <span>Điểm TB</span>
                    <strong>{{ selectedStudent.gpa }}</strong>
                  </div>
                  <div class="profile-stat danger">
                    <span>Vắng mặt</span>
                    <strong>{{ selectedStudent.absent }} buổi</strong>
                  </div>
                </div>

                <div class="detail-section">
                  <h3>Mức độ hoàn thành</h3>
                  <div class="progress-track large" aria-hidden="true">
                    <span :style="{ width: `${selectedStudent.progress}%` }" />
                  </div>
                  <div class="meter-meta">
                    <span>Tiến độ khóa học</span>
                    <strong>{{ selectedStudent.progress }}%</strong>
                  </div>
                </div>
              </div>

              <div v-if="activeTab === 'assignments'" class="drawer-stack">
                <div v-if="studentDetailLoading" class="py-6 text-center text-xs text-muted">
                  Đang tải danh sách bài nộp...
                </div>
                <template v-else-if="studentAssignmentItems.length > 0">
                  <article v-for="item in studentAssignmentItems" :key="item.id" class="assignment-row">
                    <span class="row-icon">
                      <BookOpen :size="16" />
                    </span>
                    <div class="flex-1 min-w-0">
                      <h3 class="truncate">{{ item.title }}</h3>
                      <p>{{ item.groupName }}</p>
                    </div>
                    <strong :class="item.grade !== null ? 'text-heading font-bold' : 'text-muted italic text-xs'">
                      {{ item.grade !== null ? item.grade : 'Chưa nộp / Chưa chấm' }}
                    </strong>
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
