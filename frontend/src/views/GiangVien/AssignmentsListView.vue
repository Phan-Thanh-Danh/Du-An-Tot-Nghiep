<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  ChevronRight,
  ChevronLeft,
  Users,
  Search,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  FileText,
  Filter,
  Download,
  Edit3,
  X
} from 'lucide-vue-next'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { teacherApi } from '@/services/teacherApi'

// State management
const currentStep = ref(1) // 1: Courses, 2: Assignments, 3: Students
const loading = ref(false)
const error = ref('')

// Data arrays
const courses = ref([])
const assignments = ref([])
const students = ref([])

// Selections
const selectedCourse = ref(null)
const selectedAssignment = ref(null)

// Search
const searchQuery = ref('')

// Grading Modal State
const showGradingModal = ref(false)
const gradingStudent = ref(null)
const gradingSubmitting = ref(false)
const gradingForm = ref({
  score: null,
  feedback: '',
  publish: true
})

function openGradingModal(student) {
  gradingStudent.value = student
  gradingForm.value = {
    score: student.score ?? student.Score ?? null,
    feedback: student.feedback ?? student.Feedback ?? '',
    publish: true
  }
  showGradingModal.value = true
}

function closeGradingModal() {
  showGradingModal.value = false
  gradingStudent.value = null
}

async function submitGrade() {
  if (gradingForm.value.score == null) return;
  gradingSubmitting.value = true;
  try {
    const submissionId = gradingStudent.value.submissionId ?? gradingStudent.value.SubmissionId;
    await teacherApi.gradeSubmission(submissionId, {
      score: gradingForm.value.score,
      feedback: gradingForm.value.feedback,
      publish: gradingForm.value.publish
    });
    // Reload student list
    await loadStudents(selectedAssignment.value);
    closeGradingModal();
  } catch (err) {
    console.error('Grade failed', err);
    alert('Không thể chấm điểm. Vui lòng thử lại.');
  } finally {
    gradingSubmitting.value = false;
  }
}

// Data loading
async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    const res = await teacherApi.getTeacherCourses()
    // handle nested data
    courses.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    error.value = 'Không thể tải danh sách lớp học.'
  } finally {
    loading.value = false
  }
}

async function loadAssignments(course) {
  selectedCourse.value = course
  currentStep.value = 2
  loading.value = true
  error.value = ''
  searchQuery.value = '' // reset search
  
  try {
    const res = await teacherApi.getTeacherCourseAssignments(course.courseId ?? course.CourseId)
    assignments.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    error.value = 'Không thể tải danh sách bài tập.'
  } finally {
    loading.value = false
  }
}

async function loadStudents(assignment) {
  selectedAssignment.value = assignment
  currentStep.value = 3
  loading.value = true
  error.value = ''
  searchQuery.value = '' // reset search
  
  try {
    const courseId = selectedCourse.value.courseId ?? selectedCourse.value.CourseId
    const assignmentId = assignment.maBaiTap ?? assignment.MaBaiTap ?? assignment.id ?? assignment.Id
    
    const res = await teacherApi.getCourseAssignmentStudentStatus(courseId, assignmentId)
    students.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    error.value = 'Không thể tải danh sách sinh viên.'
  } finally {
    loading.value = false
  }
}

// Navigation
function goBackToCourses() {
  currentStep.value = 1
  selectedCourse.value = null
  selectedAssignment.value = null
  searchQuery.value = ''
}

function goBackToAssignments() {
  currentStep.value = 2
  selectedAssignment.value = null
  searchQuery.value = ''
}

const downloadingAll = ref(false)

const downloadAll = async () => {
  if (!selectedCourse.value || !selectedAssignment.value) return
  const courseId = selectedCourse.value.courseId ?? selectedCourse.value.CourseId
  const assignmentId = selectedAssignment.value.maBaiTap ?? selectedAssignment.value.MaBaiTap ?? selectedAssignment.value.id ?? selectedAssignment.value.Id

  downloadingAll.value = true
  try {
    const blob = await teacherApi.downloadAllSubmissions(courseId, assignmentId)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const courseName = selectedCourse.value.courseName ?? selectedCourse.value.CourseName ?? selectedCourse.value.tenMonHoc ?? 'KhoaHoc'
    const className = selectedCourse.value.className ?? selectedCourse.value.ClassName ?? selectedCourse.value.tenLop ?? 'Lop'
    const safeCourseName = courseName.replace(/[\/\\:*?"<>|]/g, '').replace(/\s+/g, '_')
    const safeClassName = className.replace(/[\/\\:*?"<>|]/g, '').replace(/\s+/g, '_')
    a.download = `${safeCourseName}_${safeClassName}.zip`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
  } catch (err) {
    alert(err.message || 'Lỗi tải file')
  } finally {
    downloadingAll.value = false
  }
}

// Formatting
function formatDate(dateString) {
  if (!dateString) return 'Không có'
  const date = new Date(dateString)
  if (isNaN(date)) return dateString
  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function getStatusBadgeClass(status) {
  if (status === 'Đã nộp') return 'badge-info'
  if (status === 'Đã chấm') return 'badge-success'
  if (status === 'Chưa nộp bài') return 'badge-danger'
  return 'badge-neutral'
}

onMounted(() => {
  loadCourses()
})

const filteredCourses = computed(() => {
  if (!searchQuery.value) return courses.value
  const lower = searchQuery.value.toLowerCase()
  return courses.value.filter(c => 
    (c.courseName ?? c.CourseName ?? '').toLowerCase().includes(lower) ||
    (c.className ?? c.ClassName ?? '').toLowerCase().includes(lower)
  )
})

const filteredAssignments = computed(() => {
  if (!searchQuery.value) return assignments.value
  const lower = searchQuery.value.toLowerCase()
  return assignments.value.filter(a => 
    (a.tieuDe ?? a.TieuDe ?? a.name ?? a.Name ?? '').toLowerCase().includes(lower)
  )
})

const filteredStudents = computed(() => {
  if (!searchQuery.value) return students.value
  const lower = searchQuery.value.toLowerCase()
  return students.value.filter(s => 
    (s.studentName ?? s.StudentName ?? '').toLowerCase().includes(lower) ||
    (s.studentId ?? s.StudentId ?? '').toLowerCase().includes(lower)
  )
})
</script>

<template>
  <div v-if="loading && currentStep === 1" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải dữ liệu...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadCourses">Thử lại</GlassButton>
  </div>
  <div v-else class="courses-page">
    
    <!-- Header -->
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <BookOpen :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Teacher Assignments</div>
          <h1 class="page-title">Bài tập & Đồ án</h1>
          <p class="page-subtitle">
            Quản lý bài tập, đồ án và đánh giá sinh viên
          </p>
        </div>
      </div>
    </GlassPanel>

    <!-- Context bar (Search) -->
    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="flex-1">
        <button v-if="currentStep === 2" @click="goBackToCourses" class="back-btn">
          <ChevronLeft :size="16" /> Quay lại danh sách khóa học
        </button>
        <button v-else-if="currentStep === 3" @click="goBackToAssignments" class="back-btn">
          <ChevronLeft :size="16" /> Quay lại danh sách bài tập
        </button>
        <div v-else class="text-sm font-medium text-muted">
          Đang hiển thị {{ filteredCourses.length }} lớp học
        </div>
      </div>
      
      <div class="filters">
        <label class="search-field">
          <Search :size="15" />
          <input 
            v-model="searchQuery" 
            type="text" 
            :placeholder="currentStep === 1 ? 'Tìm tên khóa học hoặc lớp...' : (currentStep === 2 ? 'Tìm bài tập...' : 'Tìm mã hoặc tên SV...')" 
          />
        </label>
      </div>
    </GlassPanel>

    <!-- Content Area -->
    <div class="courses-content-area mt-4">
      <!-- Step 1: Courses -->
      <div v-if="currentStep === 1">
        <div class="panel-heading mb-4 px-1">
          <div>
            <h2>Chọn Khóa học</h2>
            <p>Vui lòng chọn một lớp học để xem bài tập</p>
          </div>
          <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
        </div>
        
        <div v-if="filteredCourses.length === 0" class="text-center p-12 surface-card border-card rounded-2xl">
          <BookOpen :size="48" class="mx-auto mb-4 text-slate-300" />
          <p class="text-body font-medium">Không tìm thấy khóa học nào</p>
        </div>
        <div v-else class="courses-grid">
          <TeacherClassCard
            v-for="course in filteredCourses"
            :key="course.courseId ?? course.CourseId"
            :title="course.courseName ?? course.CourseName"
            :subtitle="course.className ?? course.ClassName"
            :studentsCount="course.studentCount ?? course.StudentCount"
            @click="loadAssignments(course)"
            class="cursor-pointer"
          />
        </div>
      </div>

      <!-- Step 2: Assignments -->
      <div v-if="currentStep === 2">
        <div class="panel-heading mb-4 px-1">
          <div>
            <h2>Bài tập: {{ selectedCourse?.courseName ?? selectedCourse?.CourseName }}</h2>
            <p>Lớp: {{ selectedCourse?.className ?? selectedCourse?.ClassName }}</p>
          </div>
          <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
        </div>

        <div v-if="loading" class="flex justify-center p-12">
          <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
        </div>
        <div v-else-if="filteredAssignments.length === 0" class="text-center p-12 surface-card border-card rounded-2xl">
          <FileText :size="48" class="mx-auto mb-4 text-slate-300" />
          <p class="text-body font-medium">Khóa học này chưa có bài tập nào được giao.</p>
        </div>
        <div v-else class="assignments-list">
          <div 
            v-for="asm in filteredAssignments" 
            :key="asm.maBaiTap ?? asm.MaBaiTap ?? asm.id ?? asm.Id"
            class="assignment-item surface-card border-card cursor-pointer"
            @click="loadStudents(asm)"
          >
            <div class="asm-icon">
              <FileText :size="24" class="text-blue-500" />
            </div>
            <div class="asm-content">
              <h3 class="font-medium text-heading text-lg">{{ asm.tieuDe ?? asm.TieuDe ?? asm.name ?? asm.Name }}</h3>
              <p class="text-sm text-body mt-1 line-clamp-2">{{ asm.moTa ?? asm.MoTa ?? asm.description ?? asm.Description }}</p>
              <div class="flex gap-4 mt-3 text-sm text-body">
                <span class="flex items-center gap-1">
                  <CheckCircle2 :size="14" class="text-green-500" /> Hạn nộp: {{ formatDate(asm.hanNop ?? asm.HanNop ?? asm.deadline ?? asm.Deadline) }}
                </span>
              </div>
            </div>
            <div class="asm-action">
              <ChevronRight :size="20" class="text-slate-400" />
            </div>
          </div>
        </div>
      </div>

      <!-- Step 3: Students & Submissions -->
      <div v-if="currentStep === 3">
        <div class="panel-heading mb-4 px-1 flex flex-col md:flex-row md:justify-between md:items-start gap-4">
          <div>
            <h2 class="flex items-center gap-3">
              {{ selectedAssignment?.tieuDe ?? selectedAssignment?.TieuDe ?? selectedAssignment?.name ?? selectedAssignment?.Name }}
            </h2>
            <p>Tình trạng nộp bài của sinh viên lớp {{ selectedCourse?.className ?? selectedCourse?.ClassName }}</p>
          </div>
          <div class="flex items-center gap-3">
            <GlassButton variant="primary" size="sm" @click="downloadAll" :disabled="downloadingAll" class="flex items-center gap-2">
              <Download :size="16" />
              <span v-if="downloadingAll">Đang tải...</span>
              <span v-else>Tải tất cả bài nộp</span>
            </GlassButton>
            <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
          </div>
        </div>

        <div v-if="loading" class="flex justify-center p-12">
          <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
        </div>
        <div v-else class="surface-card border-card rounded-2xl overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="border-b border-card bg-black/5 dark:bg-white/5">
                  <th class="p-4 font-semibold text-sm text-heading w-16">STT</th>
                  <th class="p-4 font-semibold text-sm text-heading w-32">Mã SV</th>
                  <th class="p-4 font-semibold text-sm text-heading">Họ tên</th>
                  <th class="p-4 font-semibold text-sm text-heading w-48">Trạng thái</th>
                  <th class="p-4 font-semibold text-sm text-heading w-48">Thời gian nộp</th>
                  <th class="p-4 font-semibold text-sm text-heading w-24">Điểm</th>
                  <th class="p-4 font-semibold text-sm text-heading w-32">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr 
                  v-for="(student, index) in filteredStudents" 
                  :key="student.studentId ?? student.StudentId"
                  class="border-b border-card hover:bg-black/5 dark:hover:bg-white/5 transition-colors"
                >
                  <td class="p-4 text-sm text-body">{{ index + 1 }}</td>
                  <td class="p-4 text-sm font-medium text-heading">{{ student.studentId ?? student.StudentId }}</td>
                  <td class="p-4 text-sm font-medium text-heading">{{ student.studentName ?? student.StudentName }}</td>
                  <td class="p-4 text-sm">
                    <span class="badge" :class="getStatusBadgeClass(student.status ?? student.Status)">
                      {{ student.status ?? student.Status }}
                    </span>
                  </td>
                  <td class="p-4 text-sm text-body">{{ formatDate(student.submittedAt ?? student.SubmittedAt) }}</td>
                  <td class="p-4 text-sm font-semibold text-heading">{{ student.score ?? student.Score ?? '-' }}</td>
                  <td class="p-4 text-sm">
                    <div class="flex items-center gap-2" v-if="student.submissionId ?? student.SubmissionId">
                      <a :href="student.fileUrl ?? student.FileUrl" target="_blank" class="p-2 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded-lg transition-colors" title="Tải bài tập">
                        <Download :size="16" />
                      </a>
                      <button @click="openGradingModal(student)" class="p-2 text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 rounded-lg transition-colors" title="Chấm điểm & Nhận xét">
                        <Edit3 :size="16" />
                      </button>
                    </div>
                    <span v-else class="text-muted text-xs">Chưa có bài</span>
                  </td>
                </tr>
                <tr v-if="filteredStudents.length === 0">
                  <td colspan="6" class="p-8 text-center text-body">
                    Không tìm thấy sinh viên nào.
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Grading Modal -->
    <div v-if="showGradingModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <GlassPanel variant="surface" density="normal" class="w-full max-w-md shadow-2xl">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-bold text-heading">Chấm điểm: {{ gradingStudent?.studentName ?? gradingStudent?.StudentName }}</h3>
          <button @click="closeGradingModal" class="text-muted hover:text-heading">
            <X :size="20" />
          </button>
        </div>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-body mb-1">Điểm số</label>
            <input type="number" step="0.5" min="0" max="10" v-model="gradingForm.score" class="w-full p-2 border border-input rounded-md bg-transparent text-heading focus:outline-none focus:border-blue-500" />
          </div>
          <div>
            <label class="block text-sm font-medium text-body mb-1">Nhận xét</label>
            <textarea v-model="gradingForm.feedback" rows="3" class="w-full p-2 border border-input rounded-md bg-transparent text-heading focus:outline-none focus:border-blue-500"></textarea>
          </div>
        </div>
        <div class="flex justify-end gap-3 mt-6">
          <GlassButton variant="secondary" @click="closeGradingModal">Hủy</GlassButton>
          <GlassButton variant="primary" @click="submitGrade" :disabled="gradingSubmitting || gradingForm.score == null">Lưu điểm</GlassButton>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.courses-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.context-bar,
.header-main,
.header-actions,
.filters,
.panel-heading {
  display: flex;
  align-items: center;
}

.page-header,
.context-bar,
.panel-heading {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.header-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.eyebrow,
.page-subtitle,
.panel-heading p {
  color: var(--text-muted);
}

.eyebrow {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  color: var(--text-heading);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  font-weight: 900;
}

.page-subtitle {
  margin: 0.25rem 0 0;
  max-width: 42rem;
  font-size: 0.875rem;
  line-height: 1.5;
}

.header-actions,
.filters {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.625rem;
}

.context-bar {
  align-items: stretch;
}

.search-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  min-width: min(18rem, 100%);
  height: 2.25rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.75rem;
}

.search-field input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.search-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.panel-heading h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
}

.panel-heading p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
  padding: 0.5rem 0;
}

.back-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
  color: var(--text-link, #3b82f6);
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
}
.back-btn:hover {
  text-decoration: underline;
}

.assignments-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.assignment-item {
  display: flex;
  align-items: center;
  padding: 1.25rem;
  border-radius: 1rem;
  transition: all 0.2s ease;
  border-width: 1px;
}
.assignment-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05);
  border-color: var(--sidebar-accent, #3b82f6);
}
.dark .assignment-item:hover {
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
  border-color: var(--sidebar-accent-dark, #60a5fa);
}
.asm-icon {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  border-radius: 0.75rem;
  background: rgba(59, 130, 246, 0.1);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 1.25rem;
}
.asm-content {
  flex-grow: 1;
}
.asm-action {
  flex-shrink: 0;
  padding-left: 1rem;
}

/* Badges */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  white-space: nowrap;
}
.badge-info {
  background: var(--color-info-bg, #eff6ff);
  color: var(--color-info-text, #2563eb);
}
.dark .badge-info {
  background: rgba(59, 130, 246, 0.2);
  color: #60a5fa;
}
.badge-success {
  background: var(--color-success-bg, #f0fdf4);
  color: var(--color-success-text, #16a34a);
}
.dark .badge-success {
  background: rgba(34, 197, 94, 0.2);
  color: #4ade80;
}
.badge-danger {
  background: var(--color-danger-bg, #fef2f2);
  color: var(--color-danger-text, #dc2626);
}
.dark .badge-danger {
  background: rgba(239, 68, 68, 0.2);
  color: #f87171;
}
.badge-neutral {
  background: rgba(0, 0, 0, 0.05);
  color: var(--text-body);
}
.dark .badge-neutral {
  background: rgba(255, 255, 255, 0.1);
  color: #cbd5e1;
}

@media (max-width: 1024px) {
  .page-header,
  .context-bar {
    align-items: flex-start;
    flex-direction: column;
  }

  .filters,
  .search-field {
    width: 100%;
  }
}
</style>
