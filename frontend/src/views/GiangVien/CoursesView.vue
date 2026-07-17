<script setup>
import { computed, ref, onMounted } from 'vue'
import {
  AlertCircle,
  BookOpen,
  Calendar,
  CheckCircle2,
  Clock,
  ExternalLink,
  Filter,
  Layers,
  Search,
} from 'lucide-vue-next'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const courses = ref([])

const semesters = ['Spring 2026', 'Fall 2025', 'Summer 2025']

const filterSemester = ref('Tất cả')
const searchQuery = ref('')

function mapCourse(c) {
  return {
    id: c.maKhoaHoc ?? c.id,
    name: c.tieuDe ?? c.name ?? '',
    subject: c.tenMonHoc ?? c.subject ?? '',
    lessons: c.soBaiHoc ?? c.lessons ?? 0,
    status: c.trangThai === 'published' ? 'Published' : c.trangThai === 'draft' ? 'Draft' : 'Archived',
    semester: c.tenHocKy ?? c.semester ?? '',
    progress: c.tienDo ?? c.progress,
  }
}

async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getClasses()
    const extracted = data?.data?.items ?? data?.items ?? data?.data ?? data
    const items = Array.isArray(extracted) ? extracted : []
    courses.value = items.map(mapCourse)
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách khóa học.'
    courses.value = []
  } finally {
    loading.value = false
  }
}

const filteredCourses = computed(() => {
  const query = searchQuery.value.toLowerCase()
  return courses.value.filter(c => {
    const matchSemester = filterSemester.value === 'Tất cả' || c.semester === filterSemester.value
    const matchSearch = !query || c.name.toLowerCase().includes(query)
    return matchSemester && matchSearch
  })
})

const courseStats = computed(() => [
  { label: 'Tổng khóa học', value: courses.value.length, variant: 'neutral' },
  { label: 'Đang dạy', value: courses.value.filter(c => c.status === 'Published').length, variant: 'success' },
  { label: 'Bản nháp', value: courses.value.filter(c => c.status === 'Draft').length, variant: 'warning' },
  { label: 'Đã lưu trữ', value: courses.value.filter(c => c.status === 'Archived').length, variant: 'neutral' },
])

function resetFilters() {
  searchQuery.value = ''
  filterSemester.value = 'Tất cả'
}

function getStatusText(status) {
  switch (status) {
    case 'Published': return 'Đang dạy'
    case 'Draft': return 'Bản nháp'
    case 'Archived': return 'Đã kết thúc'
    default: return status
  }
}

function getStatusVariant(status) {
  switch (status) {
    case 'Published': return 'success'
    case 'Draft': return 'warning'
    case 'Archived': return 'neutral'
    default: return 'neutral'
  }
}

function getCourseProgress(course) {
  if (course.progress !== undefined && course.progress !== null) {
    return course.progress;
  }
  if (course.status === 'Draft') return 0;
  if (course.status === 'Archived') return 100;
  if (course.lessons === 0) return 0;
  return Math.min(92, 58 + course.lessons)
}

onMounted(() => { loadCourses() })
</script>

<template>
  <div v-if="loading" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải khóa học...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadCourses">Thử lại</GlassButton>
  </div>
  <div v-else class="courses-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <BookOpen :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Teacher courses</div>
          <h1 class="page-title">Khóa học của tôi</h1>
          <p class="page-subtitle">
            Quản lý danh sách khóa học, học kỳ và trạng thái nội dung bạn đang phụ trách.
          </p>
        </div>
      </div>

      <div class="header-actions">
        <GlassBadge variant="info" size="md">{{ filterSemester }}</GlassBadge>
        <GlassButton size="sm" variant="secondary" @click="resetFilters">
          <template #leading>
            <Filter :size="14" />
          </template>
          Đặt lại lọc
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="mini-stats">
        <div v-for="item in courseStats" :key="item.label" class="mini-stat">
          <span class="stat-label">{{ item.label }}</span>
          <div class="stat-value-line">
            <strong>{{ item.value }}</strong>
            <GlassBadge :variant="item.variant" size="sm">{{ item.label }}</GlassBadge>
          </div>
        </div>
      </div>

      <div class="filters">
        <label class="search-field">
          <Search :size="15" />
          <input v-model="searchQuery" type="text" placeholder="Tìm tên khóa học..." />
        </label>

        <label class="select-field">
          <Calendar :size="15" />
          <select v-model="filterSemester">
            <option value="Tất cả">Tất cả học kỳ</option>
            <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
          </select>
        </label>
      </div>
    </GlassPanel>

    <div class="courses-content-area mt-4">
      <div class="panel-heading mb-4 px-1">
        <div>
          <h2>Danh sách khóa học</h2>
          <p>Hiển thị {{ filteredCourses.length }} / {{ courses.length }} khóa học</p>
        </div>
        <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
      </div>

      <div v-if="filteredCourses.length" class="courses-grid">
        <div v-for="course in filteredCourses" :key="course.id" class="course-card group">
          <div class="course-card-header">
            <div class="course-icon-wrapper">
              <BookOpen :size="24" class="text-link" />
            </div>
            <GlassBadge :variant="getStatusVariant(course.status)" size="sm" class="status-badge">
              <CheckCircle2 v-if="course.status === 'Published'" :size="12" />
              <Clock v-else-if="course.status === 'Draft'" :size="12" />
              <AlertCircle v-else :size="12" />
              {{ getStatusText(course.status) }}
            </GlassBadge>
          </div>
          
          <div class="course-card-body">
            <h3 class="course-title" :title="course.name">{{ course.name }}</h3>
            <div class="course-meta">
              <span class="meta-item">
                <Calendar :size="14" />
                {{ course.semester }}
              </span>
              <span class="meta-item">
                <Layers :size="14" />
                {{ course.subject }}
              </span>
            </div>
            
            <div class="course-progress">
              <div class="progress-header">
                <span class="text-xs font-semibold text-muted">{{ course.lessons }} bài học</span>
                <span class="text-xs font-bold text-link">{{ getCourseProgress(course) }}%</span>
              </div>
              <div class="progress-track">
                <div class="progress-fill" :style="{ width: `${getCourseProgress(course)}%` }"></div>
              </div>
            </div>
          </div>
          
          <div class="course-card-footer">
            <GlassButton
              size="md"
              variant="primary"
              class="w-full flex justify-center items-center gap-2 group-hover:bg-(--accent-primary) group-hover:text-inverse transition-all"
              @click="$router.push(`/teacher/class-progress/${course.id}`)"
            >
              Vào không gian khóa học
              <ExternalLink :size="16" />
            </GlassButton>
          </div>
        </div>
      </div>

      <EmptyState
        v-else
        title="Không tìm thấy khóa học"
        description="Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm của bạn."
      >
        <template #icon>
          <Search :size="22" />
        </template>
        <GlassButton size="sm" variant="secondary" @click="resetFilters">Đặt lại bộ lọc</GlassButton>
      </EmptyState>
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
.panel-heading,
.stat-value-line,
.course-cell,
.course-cell small,
.subject-chip,
.progress-cell {
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

.header-icon,
.course-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: 0 0 auto;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.header-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.eyebrow,
.page-subtitle,
.panel-heading p,
.stat-label,
.muted,
.course-cell small {
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

.mini-stats {
  display: grid;
  grid-template-columns: repeat(4, minmax(7rem, 1fr));
  gap: 0.625rem;
  flex: 1;
}

.mini-stat {
  min-width: 0;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
}

.stat-label {
  display: block;
  font-size: 0.6875rem;
  font-weight: 700;
}

.stat-value-line {
  justify-content: space-between;
  gap: 0.5rem;
  margin-top: 0.375rem;
}

.stat-value-line strong {
  color: var(--text-heading);
  font-size: 1.125rem;
}

.search-field,
.select-field {
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

.select-field {
  min-width: 11rem;
}

.search-field input,
.select-field select {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.8125rem;
  font-weight: 600;
}

.select-field select {
  appearance: none;
  cursor: pointer;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.search-field:focus-within,
.select-field:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.courses-table-panel {
  min-width: 0;
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

.courses-table-panel {
  min-width: 0;
  background: transparent !important;
  border: none !important;
  box-shadow: none !important;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.5rem;
  padding: 0.5rem 0;
}

.course-card {
  display: flex;
  flex-direction: column;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
}

.course-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 20px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -4px rgba(0, 0, 0, 0.04);
  border-color: rgba(37, 99, 235, 0.3); /* blue-600 with opacity */
}

.course-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.25rem 1.25rem 0;
}

.course-icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  transition: all 0.3s ease;
}

.course-card:hover .course-icon-wrapper {
  background: rgba(37, 99, 235, 0.1);
  border-color: rgba(37, 99, 235, 0.2);
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.course-card-body {
  padding: 1.25rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.course-title {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
  font-weight: 800;
  color: var(--text-heading);
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.course-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-muted);
  background: var(--surface-input);
  padding: 0.25rem 0.625rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
}

.course-progress {
  margin-top: auto;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.progress-track {
  width: 100%;
  height: 0.375rem;
  background: var(--surface-input);
  border-radius: 999px;
  overflow: hidden;
  border: 1px solid var(--border-card);
}

.progress-fill {
  height: 100%;
  background: var(--text-link);
  border-radius: inherit;
  transition: width 0.8s cubic-bezier(0.4, 0, 0.2, 1);
}

.course-card-footer {
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--border-card);
  background: var(--surface-sidebar);
}

@media (max-width: 1024px) {
  .page-header,
  .context-bar {
    align-items: flex-start;
    flex-direction: column;
  }

  .mini-stats {
    width: 100%;
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filters,
  .search-field {
    width: 100%;
  }

  .select-field {
    flex: 1;
  }
}

@media (max-width: 640px) {
  .mini-stats {
    grid-template-columns: 1fr;
  }

  .header-actions,
  .select-field {
    width: 100%;
  }
}
</style>
