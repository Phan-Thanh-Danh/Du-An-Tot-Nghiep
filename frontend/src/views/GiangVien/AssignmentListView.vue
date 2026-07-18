<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { BookOpen, AlertCircle, Search, ChevronLeft, FileText, CheckCircle2, ChevronRight } from 'lucide-vue-next'
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
    error.value = 'Không thể tải danh sách bài tập.'
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
    (a.tieuDe ?? a.TieuDe ?? a.name ?? a.Name ?? '').toLowerCase().includes(lower)
  )
})

function goToSubmissions(asm) {
  const assignmentId = asm.maBaiTap ?? asm.MaBaiTap ?? asm.id ?? asm.Id
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
            Quản lý bài tập, đồ án và đánh giá sinh viên
          </p>
        </div>
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
          <h2>Bài tập: {{ selectedCourse?.courseName ?? selectedCourse?.CourseName ?? 'Chưa xác định' }}</h2>
          <p>Lớp: {{ selectedCourse?.className ?? selectedCourse?.ClassName ?? 'Chưa xác định' }}</p>
        </div>
        <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
      </div>

      <div v-if="filteredAssignments.length === 0" class="text-center p-12 surface-card border-card rounded-2xl">
        <FileText :size="48" class="mx-auto mb-4 text-slate-300" />
        <p class="text-body font-medium">Khóa học này chưa có bài tập nào được giao.</p>
      </div>
      <div v-else class="assignments-list">
        <div 
          v-for="asm in filteredAssignments" 
          :key="asm.maBaiTap ?? asm.MaBaiTap ?? asm.id ?? asm.Id"
          class="assignment-item surface-card border-card cursor-pointer"
          @click="goToSubmissions(asm)"
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
