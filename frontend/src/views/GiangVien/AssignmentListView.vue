<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { 
  BookOpen, 
  AlertCircle, 
  Search, 
  ChevronLeft, 
  FileText, 
  CheckCircle2, 
  ChevronRight, 
  Plus, 
  Edit3, 
  Trash2, 
  X, 
  Save,
  Users
} from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const error = ref('')
const assignments = ref([])
const searchQuery = ref('')
const selectedCourse = ref(null)

const courseId = route.params.courseId

// Create / Edit Assignment Modal State
const showAssignmentModal = ref(false)
const editingAssignment = ref(null)
const assignmentSubmitting = ref(false)
const assignmentForm = ref({
  title: '',
  description: '',
  dueAt: '',
  maxAttempts: 3,
  maxScore: 10,
  gradingGuide: '',
  status: 'da_xuat_ban'
})

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    // Tải thông tin khóa học để hiển thị Tên Khóa học & Tên Lớp
    const coursesRes = await teacherApi.getTeacherCourses()
    const allCourses = coursesRes?.data ?? coursesRes?.Data ?? coursesRes ?? []
    selectedCourse.value = allCourses.find(c => String(c.courseId ?? c.CourseId) === String(courseId)) || null

    // Tải danh sách bài tập
    const res = await teacherApi.getTeacherCourseAssignments(courseId)
    assignments.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    console.error('Failed to load assignments', err)
    error.value = 'Không thể tải danh sách bài tập của khóa học.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadData()
})

const filteredAssignments = computed(() => {
  if (!searchQuery.value) return assignments.value
  const lower = searchQuery.value.toLowerCase()
  return assignments.value.filter(a => 
    (a.title ?? a.Title ?? a.tieuDe ?? a.TieuDe ?? a.name ?? a.Name ?? '').toLowerCase().includes(lower) ||
    (a.description ?? a.Description ?? a.moTa ?? a.MoTa ?? '').toLowerCase().includes(lower)
  )
})

function goToSubmissions(asm) {
  const assignmentId = asm.id ?? asm.Id ?? asm.maBaiTap ?? asm.MaBaiTap
  router.push(`/teacher/assignments/${courseId}/${assignmentId}`)
}

function goBack() {
  router.push('/teacher/assignments')
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

function openCreateModal() {
  editingAssignment.value = null
  // Default deadline: 7 days from now at 23:59
  const nextWeek = new Date()
  nextWeek.setDate(nextWeek.getDate() + 7)
  nextWeek.setHours(23, 59, 0, 0)
  const tzOffset = nextWeek.getTimezoneOffset() * 60000
  const localISOTime = new Date(nextWeek.getTime() - tzOffset).toISOString().slice(0, 16)

  assignmentForm.value = {
    title: '',
    description: '',
    dueAt: localISOTime,
    maxAttempts: 3,
    maxScore: 10,
    gradingGuide: '',
    status: 'da_xuat_ban'
  }
  showAssignmentModal.value = true
}

function openEditModal(asm, event) {
  if (event) event.stopPropagation()
  editingAssignment.value = asm

  const due = asm.deadline || asm.Deadline || asm.hanNop || asm.HanNop
  let formattedDue = ''
  if (due) {
    const d = new Date(due)
    const tzOffset = d.getTimezoneOffset() * 60000
    formattedDue = new Date(d.getTime() - tzOffset).toISOString().slice(0, 16)
  }

  assignmentForm.value = {
    title: asm.title || asm.Title || asm.tieuDe || asm.TieuDe || '',
    description: asm.description || asm.Description || asm.moTa || asm.MoTa || '',
    dueAt: formattedDue,
    maxAttempts: asm.maxAttempts || asm.MaxAttempts || asm.soLanNopToiDa || 3,
    maxScore: asm.maxScore || asm.MaxScore || 10,
    gradingGuide: asm.gradingGuide || asm.GradingGuide || asm.huongDanChamDiem || '',
    status: asm.status || asm.Status || asm.trangThai || 'da_xuat_ban'
  }
  showAssignmentModal.value = true
}

function closeAssignmentModal() {
  showAssignmentModal.value = false
  editingAssignment.value = null
}

async function submitAssignment() {
  if (!assignmentForm.value.title.trim()) {
    alert('Vui lòng nhập tiêu đề bài tập.')
    return
  }
  if (!assignmentForm.value.dueAt) {
    alert('Vui lòng chọn hạn nộp bài tập.')
    return
  }

  assignmentSubmitting.value = true
  try {
    const payload = {
      courseId: parseInt(courseId),
      title: assignmentForm.value.title.trim(),
      description: assignmentForm.value.description,
      dueAt: new Date(assignmentForm.value.dueAt).toISOString(),
      maxAttempts: assignmentForm.value.maxAttempts,
      maxScore: assignmentForm.value.maxScore,
      gradingGuide: assignmentForm.value.gradingGuide,
      status: assignmentForm.value.status
    }

    if (editingAssignment.value) {
      const asmId = editingAssignment.value.id ?? editingAssignment.value.Id ?? editingAssignment.value.maBaiTap ?? editingAssignment.value.MaBaiTap
      await teacherApi.updateAssignment(asmId, payload)
    } else {
      await teacherApi.createAssignment(payload)
    }

    await loadData()
    closeAssignmentModal()
  } catch (err) {
    console.error('Save assignment failed', err)
    alert(err?.message || 'Không thể lưu bài tập. Vui lòng kiểm tra lại.')
  } finally {
    assignmentSubmitting.value = false
  }
}

async function handleDeleteAssignment(asm, event) {
  if (event) event.stopPropagation()
  const asmId = asm.id ?? asm.Id ?? asm.maBaiTap ?? asm.MaBaiTap
  const title = asm.title ?? asm.Title ?? asm.tieuDe ?? asm.TieuDe ?? 'Bài tập'
  
  if (!confirm(`Bạn có chắc chắn muốn xóa bài tập "${title}" không?`)) return

  try {
    await teacherApi.deleteAssignment(asmId)
    await loadData()
  } catch (err) {
    alert(err?.message || 'Không thể xóa bài tập.')
  }
}
</script>

<template>
  <div v-if="loading && assignments.length === 0" class="flex items-center justify-center min-h-[300px]">
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
            Khóa học: {{ selectedCourse?.courseName ?? selectedCourse?.CourseName ?? selectedCourse?.title ?? 'Đang tải...' }} ({{ selectedCourse?.className ?? selectedCourse?.ClassName ?? 'Tất cả lớp' }})
          </p>
        </div>
      </div>
      <div class="header-actions">
        <button
          type="button"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs shadow-md transition-all cursor-pointer"
          @click="openCreateModal"
        >
          <Plus :size="15" /> Tạo bài tập mới
        </button>
      </div>
    </GlassPanel>

    <!-- Context bar (Search) -->
    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="flex-1">
        <button @click="goBack" class="back-btn">
          <ChevronLeft :size="16" /> Quay lại danh sách khóa học
        </button>
      </div>
      
      <div class="filters">
        <label class="search-field">
          <Search :size="15" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm bài tập..." 
          />
        </label>
      </div>
    </GlassPanel>

    <!-- Content Area -->
    <div class="courses-content-area mt-4">
      <div class="panel-heading mb-4 px-1">
        <div>
          <h2>Bài tập: {{ selectedCourse?.courseName ?? selectedCourse?.CourseName ?? selectedCourse?.title ?? 'Khóa học' }}</h2>
          <p>Lớp: {{ selectedCourse?.className ?? selectedCourse?.ClassName ?? 'Tất cả lớp' }} · {{ filteredAssignments.length }} bài tập</p>
        </div>
        <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
      </div>

      <div v-if="filteredAssignments.length === 0" class="text-center p-12 surface-card border-card rounded-2xl">
        <FileText :size="48" class="mx-auto mb-4 text-slate-300" />
        <p class="text-body font-medium">Khóa học này chưa có bài tập nào được giao.</p>
        <button
          type="button"
          class="mt-4 inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold text-xs transition-all shadow-xs"
          @click="openCreateModal"
        >
          <Plus :size="14" /> Tạo bài tập đầu tiên
        </button>
      </div>
      <div v-else class="assignments-list">
        <div 
          v-for="asm in filteredAssignments" 
          :key="asm.id ?? asm.Id ?? asm.maBaiTap ?? asm.MaBaiTap"
          class="assignment-item surface-card border-card cursor-pointer group"
          @click="goToSubmissions(asm)"
        >
          <div class="asm-icon">
            <FileText :size="24" class="text-blue-500" />
          </div>
          <div class="asm-content flex-1">
            <div class="flex items-center justify-between gap-2">
              <h3 class="font-medium text-heading text-lg group-hover:text-blue-600 transition-colors">
                {{ asm.title ?? asm.Title ?? asm.tieuDe ?? asm.TieuDe ?? asm.name ?? asm.Name ?? 'Bài tập' }}
              </h3>
              <!-- Action buttons for edit / delete -->
              <div class="flex items-center gap-1 opacity-80 group-hover:opacity-100 transition-opacity" @click.stop>
                <button
                  type="button"
                  title="Chỉnh sửa bài tập"
                  class="p-1.5 rounded-lg hover:bg-surface-input text-slate-500 hover:text-blue-600 transition-colors"
                  @click="openEditModal(asm, $event)"
                >
                  <Edit3 :size="15" />
                </button>
                <button
                  type="button"
                  title="Xóa bài tập"
                  class="p-1.5 rounded-lg hover:bg-rose-50 dark:hover:bg-rose-950/30 text-slate-400 hover:text-rose-600 transition-colors"
                  @click="handleDeleteAssignment(asm, $event)"
                >
                  <Trash2 :size="15" />
                </button>
              </div>
            </div>
            <p class="text-sm text-body mt-1 line-clamp-2">{{ asm.description ?? asm.Description ?? asm.moTa ?? asm.MoTa }}</p>
            <div class="flex flex-wrap items-center justify-between gap-3 mt-3 pt-3 border-t border-card/50 text-sm text-body">
              <div class="flex flex-wrap items-center gap-4">
                <span class="flex items-center gap-1 text-xs">
                  <CheckCircle2 :size="14" class="text-emerald-500" /> Hạn nộp: {{ formatDate(asm.deadline ?? asm.Deadline ?? asm.hanNop ?? asm.HanNop) }}
                </span>
                <span v-if="asm.submissionsCount !== undefined" class="text-xs text-muted">
                  Đã nộp: <strong class="text-heading">{{ asm.submissionsCount }}</strong> / {{ asm.totalStudents ?? 0 }}
                </span>
                <span v-if="asm.pendingGrades !== undefined && asm.pendingGrades > 0" class="text-xs text-amber-500 font-semibold">
                  Chờ chấm: {{ asm.pendingGrades }}
                </span>
              </div>

              <!-- Button to view submissions & grade -->
              <button
                type="button"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold bg-blue-600/10 text-blue-600 hover:bg-blue-600 hover:text-white dark:bg-blue-400/10 dark:text-blue-400 dark:hover:bg-blue-600 dark:hover:text-white transition-all ml-auto cursor-pointer"
                @click.stop="goToSubmissions(asm)"
              >
                <Users :size="13" /> Xem bài nộp & Chấm điểm
              </button>
            </div>
          </div>
          <div class="asm-action">
            <ChevronRight :size="20" class="text-slate-400 group-hover:text-blue-600 transition-colors" />
          </div>
        </div>
      </div>
    </div>

    <!-- Create / Edit Assignment Modal -->
    <div
      v-if="showAssignmentModal"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-xs"
    >
      <div class="w-full max-w-lg rounded-2xl surface-card border border-card p-6 shadow-2xl space-y-4">
        <div class="flex items-center justify-between border-b border-card pb-3">
          <div>
            <h3 class="text-base font-bold text-heading">
              {{ editingAssignment ? 'Chỉnh sửa bài tập' : 'Tạo bài tập mới cho lớp' }}
            </h3>
            <p class="text-xs text-muted">{{ selectedCourse?.courseName || 'Khóa học' }}</p>
          </div>
          <button
            type="button"
            class="p-1 rounded-lg hover:bg-surface-input text-muted"
            @click="closeAssignmentModal"
          >
            <X :size="18" />
          </button>
        </div>

        <form class="space-y-3.5 text-xs" @submit.prevent="submitAssignment">
          <div>
            <label class="block font-semibold text-heading mb-1">Tiêu đề bài tập *</label>
            <input
              v-model="assignmentForm.title"
              type="text"
              required
              placeholder="VD: Bài tập thực hành 01 - Cấu trúc dữ liệu mảng"
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-heading font-medium text-sm focus:outline-hidden focus:border-blue-500"
            />
          </div>

          <div>
            <label class="block font-semibold text-heading mb-1">Mô tả & Hướng dẫn làm bài</label>
            <textarea
              v-model="assignmentForm.description"
              rows="3"
              placeholder="Nhập yêu cầu đề bài, tiêu chí đánh giá và hướng dẫn nộp file..."
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-heading focus:outline-hidden focus:border-blue-500"
            ></textarea>
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="block font-semibold text-heading mb-1">Hạn nộp (Deadline) *</label>
              <input
                v-model="assignmentForm.dueAt"
                type="datetime-local"
                required
                class="w-full px-3 py-2 rounded-xl surface-input border border-card text-heading focus:outline-hidden focus:border-blue-500"
              />
            </div>
            <div>
              <label class="block font-semibold text-heading mb-1">Số lần nộp tối đa</label>
              <input
                v-model.number="assignmentForm.maxAttempts"
                type="number"
                min="1"
                max="20"
                class="w-full px-3 py-2 rounded-xl surface-input border border-card text-heading focus:outline-hidden focus:border-blue-500"
              />
            </div>
          </div>

          <div>
            <label class="block font-semibold text-heading mb-1">Hướng dẫn chấm điểm (nội bộ)</label>
            <input
              v-model="assignmentForm.gradingGuide"
              type="text"
              placeholder="Ghi chú thang điểm và barem chấm..."
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-heading focus:outline-hidden focus:border-blue-500"
            />
          </div>

          <div class="flex items-center justify-end gap-2 pt-3 border-t border-card">
            <button
              type="button"
              class="px-4 py-2 rounded-xl text-xs font-semibold hover:bg-surface-input text-muted"
              @click="closeAssignmentModal"
            >
              Hủy
            </button>
            <button
              type="submit"
              class="px-4 py-2 rounded-xl text-xs font-bold bg-blue-600 hover:bg-blue-700 text-white shadow-xs inline-flex items-center gap-1.5"
              :disabled="assignmentSubmitting"
            >
              <Save :size="14" />
              {{ assignmentSubmitting ? 'Đang lưu...' : (editingAssignment ? 'Cập nhật' : 'Tạo bài tập') }}
            </button>
          </div>
        </form>
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
