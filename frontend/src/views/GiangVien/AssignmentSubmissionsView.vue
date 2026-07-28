<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { BookOpen, AlertCircle, Search, ChevronLeft, Download, Edit3, X, Award, Check, User, Clock } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const error = ref('')
const students = ref([])
const searchQuery = ref('')
const selectedCourse = ref(null)
const selectedAssignment = ref(null)

const courseId = route.params.courseId
const assignmentId = route.params.assignmentId

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
    await loadData(false); // don't show full loading spinner again
    closeGradingModal();
  } catch (err) {
    console.error('Grade failed', err);
    alert('Không thể chấm điểm. Vui lòng thử lại.');
  } finally {
    gradingSubmitting.value = false;
  }
}

async function loadData(showLoading = true) {
  if (showLoading) loading.value = true
  error.value = ''
  try {
    // Lấy tên khóa học
    const coursesRes = await teacherApi.getTeacherCourses()
    const allCourses = coursesRes?.data ?? coursesRes?.Data ?? coursesRes ?? []
    selectedCourse.value = allCourses.find(c => String(c.courseId ?? c.CourseId) === String(courseId)) || null

    // Lấy tên bài tập
    const asmRes = await teacherApi.getTeacherCourseAssignments(courseId)
    const allAsm = asmRes?.data ?? asmRes?.Data ?? asmRes ?? []
    selectedAssignment.value = allAsm.find(a => String(a.maBaiTap ?? a.MaBaiTap ?? a.id ?? a.Id) === String(assignmentId)) || null

    // Lấy danh sách nộp bài
    const res = await teacherApi.getCourseAssignmentStudentStatus(courseId, assignmentId)
    students.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    error.value = 'Không thể tải danh sách sinh viên.'
  } finally {
    if (showLoading) loading.value = false
  }
}

onMounted(() => {
  loadData()
})

const filteredStudents = computed(() => {
  if (!searchQuery.value) return students.value
  const lower = searchQuery.value.toLowerCase()
  return students.value.filter(s => 
    (s.studentName ?? s.StudentName ?? '').toLowerCase().includes(lower) ||
    (s.studentId ?? s.StudentId ?? '').toLowerCase().includes(lower)
  )
})

function goBack() {
  router.push(`/teacher/assignments/${courseId}`)
}

const downloadingAll = ref(false)

const downloadAll = async () => {
  if (!selectedCourse.value || !selectedAssignment.value) return

  downloadingAll.value = true
  try {
    const blob = await teacherApi.downloadAllSubmissions(courseId, assignmentId)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const courseName = selectedCourse.value.courseName ?? selectedCourse.value.CourseName ?? selectedCourse.value.tenMonHoc ?? 'KhoaHoc'
    const className = selectedCourse.value.className ?? selectedCourse.value.ClassName ?? selectedCourse.value.tenLop ?? 'Lop'
    const safeCourseName = courseName.replace(/[/\\:*?"<>|]/g, '').replace(/\s+/g, '_')
    const safeClassName = className.replace(/[/\\:*?"<>|]/g, '').replace(/\s+/g, '_')
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

function downloadFile(student) {
  const url = student.fileUrl ?? student.FileUrl
  if (!url || !url.trim()) {
    alert('Bài nộp này chưa có file đính kèm trên hệ thống R2!')
    return
  }
  window.open(url, '_blank')
}

function getStatusBadgeClass(status) {
  if (status === 'Đã nộp') return 'badge-info'
  if (status === 'Đã chấm') return 'badge-success'
  if (status === 'Chưa nộp bài') return 'badge-danger'
  return 'badge-neutral'
}
</script>

<template>
  <div v-if="loading && students.length === 0" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải dữ liệu...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadData">Thử lại</GlassButton>
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
        <button @click="goBack" class="back-btn">
          <ChevronLeft :size="16" /> Quay lại danh sách bài tập
        </button>
      </div>
      
      <div class="filters">
        <label class="search-field">
          <Search :size="15" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm mã hoặc tên SV..." 
          />
        </label>
      </div>
    </GlassPanel>

    <!-- Content Area -->
    <div class="courses-content-area mt-4">
      <div class="panel-heading mb-4 px-1 flex flex-col md:flex-row md:justify-between md:items-start gap-4">
        <div>
          <h2 class="flex items-center gap-3">
            {{ selectedAssignment?.tieuDe ?? selectedAssignment?.TieuDe ?? selectedAssignment?.name ?? selectedAssignment?.Name ?? 'Bài tập' }}
          </h2>
          <p>Tình trạng nộp bài của sinh viên lớp {{ selectedCourse?.className ?? selectedCourse?.ClassName ?? 'Chưa xác định' }}</p>
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

      <div class="surface-card border-card rounded-2xl overflow-hidden">
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
                    <button
                      type="button"
                      @click="downloadFile(student)"
                      class="p-2 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded-lg transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
                      :disabled="!(student.fileUrl || student.FileUrl)"
                      :title="(student.fileUrl || student.FileUrl) ? 'Tải bài nộp' : 'Chưa có file bài nộp R2'"
                    >
                      <Download :size="16" />
                    </button>
                    <button @click="openGradingModal(student)" class="p-2 text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 rounded-lg transition-colors" title="Chấm điểm & Nhận xét">
                      <Edit3 :size="16" />
                    </button>
                  </div>
                  <span v-else class="text-xs text-muted">Chưa có bài</span>
                </td>
              </tr>
              <tr v-if="filteredStudents.length === 0">
                <td colspan="7" class="p-8 text-center text-body">
                  Không tìm thấy sinh viên nào.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Grading Modal -->
    <div v-if="showGradingModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-md transition-all animate-fade-in">
      <div class="w-full max-w-lg lg-glass-strong rounded-3xl p-6 shadow-2xl border border-card/60 space-y-5 relative">
        <!-- Header -->
        <div class="flex justify-between items-center pb-3 border-b border-card">
          <div class="flex items-center gap-3">
            <span class="w-10 h-10 rounded-2xl bg-(--accent-primary-soft) text-(--accent-primary) flex items-center justify-center border border-card shadow-xs">
              <Award :size="20" />
            </span>
            <div>
              <h3 class="text-base font-bold text-heading leading-tight">Chấm điểm & Nhận xét</h3>
              <p class="text-xs text-muted">Đánh giá kết quả bài nộp của sinh viên</p>
            </div>
          </div>
          <button
            @click="closeGradingModal"
            class="w-8 h-8 rounded-full flex items-center justify-center text-muted hover:text-heading hover:bg-black/5 dark:hover:bg-white/10 transition-all"
          >
            <X :size="18" />
          </button>
        </div>

        <!-- Student Info Banner -->
        <div class="p-3.5 rounded-2xl surface-card border border-card flex items-center justify-between gap-3">
          <div class="flex items-center gap-3 min-w-0">
            <span class="w-9 h-9 rounded-full bg-(--accent-primary-soft) text-(--accent-primary) font-bold flex items-center justify-center text-sm shrink-0 border border-card">
              {{ (gradingStudent?.studentName ?? gradingStudent?.StudentName ?? 'SV').split(' ').pop()[0] }}
            </span>
            <div class="min-w-0">
              <h4 class="text-xs font-bold text-heading truncate">{{ gradingStudent?.studentName ?? gradingStudent?.StudentName }}</h4>
              <p class="text-[11px] text-muted font-mono">MSSV: {{ gradingStudent?.studentId ?? gradingStudent?.StudentId }}</p>
            </div>
          </div>
          <GlassBadge variant="info" size="sm" class="shrink-0">
            <Clock :size="11" />
            {{ formatDate(gradingStudent?.submittedAt ?? gradingStudent?.SubmittedAt) }}
          </GlassBadge>
        </div>

        <!-- Form Body -->
        <div class="space-y-4">
          <!-- Score Input & Presets -->
          <div>
            <div class="flex justify-between items-center mb-1.5">
              <label class="text-xs font-bold text-heading">Điểm số (Thang điểm 10)</label>
              <span class="text-xs font-bold text-(--accent-primary)">
                {{ gradingForm.score !== null && gradingForm.score !== '' ? `${gradingForm.score} / 10` : 'Chưa nhập điểm' }}
              </span>
            </div>
            <div class="relative">
              <input
                type="number"
                step="0.1"
                min="0"
                max="10"
                v-model="gradingForm.score"
                placeholder="Nhập điểm số (0 - 10)"
                class="lg-control w-full px-4 text-center font-bold text-lg tracking-wide text-heading"
              />
            </div>
            <!-- Score Presets -->
            <div class="flex items-center gap-1.5 mt-2 overflow-x-auto pb-1">
              <span class="text-[11px] text-muted mr-1 font-medium">Gợi ý:</span>
              <button
                v-for="preset in [10, 9, 8.5, 7.5, 6, 5]"
                :key="preset"
                type="button"
                @click="gradingForm.score = preset"
                class="px-2.5 py-1 text-xs font-semibold rounded-lg border border-card surface-card text-heading hover:bg-(--accent-primary-soft) hover:text-(--accent-primary) hover:border-(--accent-primary)/30 transition-all shrink-0"
              >
                {{ preset }}
              </button>
            </div>
          </div>

          <!-- Feedback Input -->
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Nhận xét & Gợi ý cải thiện</label>
            <textarea
              v-model="gradingForm.feedback"
              rows="3"
              placeholder="Nhập nhận xét chi tiết bài làm cho sinh viên (tùy chọn)..."
              class="lg-control w-full p-3 text-xs text-heading resize-none leading-relaxed"
            ></textarea>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex justify-end items-center gap-2.5 pt-3 border-t border-card">
          <GlassButton variant="ghost" size="md" @click="closeGradingModal">
            Hủy bỏ
          </GlassButton>
          <GlassButton
            variant="primary"
            size="md"
            @click="submitGrade"
            :disabled="gradingSubmitting || gradingForm.score === null || gradingForm.score === ''"
          >
            <template #leading>
              <Check :size="16" />
            </template>
            <span v-if="gradingSubmitting">Đang lưu...</span>
            <span v-else>Lưu điểm số</span>
          </GlassButton>
        </div>
      </div>
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
