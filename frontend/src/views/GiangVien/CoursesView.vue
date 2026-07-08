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
const subjects = ['CNTT', 'Kinh tế', 'Ngôn ngữ', 'Thiết kế']

const filterSemester = ref('Tất cả')
const filterSubject = ref('Tất cả')
const searchQuery = ref('')

function mapCourse(c) {
  return {
    id: c.maKhoaHoc ?? c.id,
    name: c.tieuDe ?? c.name ?? '',
    subject: c.tenMonHoc ?? c.subject ?? '',
    lessons: c.soBaiHoc ?? c.lessons ?? 0,
    status: c.trangThai === 'published' ? 'Published' : c.trangThai === 'draft' ? 'Draft' : 'Archived',
    semester: c.tenHocKy ?? c.semester ?? '',
  }
}

async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getClasses()
    const items = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
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
    const matchSubject = filterSubject.value === 'Tất cả' || c.subject === filterSubject.value
    const matchSearch = !query || c.name.toLowerCase().includes(query)
    return matchSemester && matchSubject && matchSearch
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
  filterSubject.value = 'Tất cả'
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
  if (course.status === 'Draft') return 38
  if (course.status === 'Archived') return 100
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

        <label class="select-field">
          <Layers :size="15" />
          <select v-model="filterSubject">
            <option value="Tất cả">Tất cả môn</option>
            <option v-for="subj in subjects" :key="subj" :value="subj">{{ subj }}</option>
          </select>
        </label>
      </div>
    </GlassPanel>

    <GlassPanel variant="surface" density="none" class="courses-table-panel">
      <template #header>
        <div class="panel-heading">
          <div>
            <h2>Danh sách khóa học</h2>
            <p>Hiển thị {{ filteredCourses.length }} / {{ courses.length }} khóa học</p>
          </div>
          <GlassBadge variant="primary" size="sm">LMS Academic</GlassBadge>
        </div>
      </template>

      <TableShell v-if="filteredCourses.length" density="compact" sticky-header>
        <table>
          <thead>
            <tr>
              <th>Khóa học</th>
              <th>Môn</th>
              <th>Bài học</th>
              <th>Tiến độ</th>
              <th>Trạng thái</th>
              <th class="text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="course in filteredCourses" :key="course.id">
              <td>
                <div class="course-cell">
                  <span class="course-icon">
                    <BookOpen :size="17" />
                  </span>
                  <span class="min-w-0">
                    <strong>{{ course.name }}</strong>
                    <small>
                      <Calendar :size="12" />
                      {{ course.semester }}
                    </small>
                  </span>
                </div>
              </td>
              <td>
                <span class="subject-chip">
                  <Layers :size="13" />
                  {{ course.subject }}
                </span>
              </td>
              <td>
                <span class="lesson-count">{{ course.lessons }}</span>
                <span class="muted"> bài học</span>
              </td>
              <td>
                <div class="progress-cell">
                  <div class="progress-line">
                    <span :style="{ width: `${getCourseProgress(course)}%` }" />
                  </div>
                  <strong>{{ getCourseProgress(course) }}%</strong>
                </div>
              </td>
              <td>
                <GlassBadge :variant="getStatusVariant(course.status)" size="sm">
                  <CheckCircle2 v-if="course.status === 'Published'" :size="11" />
                  <Clock v-else-if="course.status === 'Draft'" :size="11" />
                  <AlertCircle v-else :size="11" />
                  {{ getStatusText(course.status) }}
                </GlassBadge>
              </td>
              <td class="text-right">
                <GlassButton
                  size="sm"
                  variant="ghost"
                  title="Xem chi tiết"
                  @click="$router.push('/teacher/classes')"
                >
                  <template #leading>
                    <ExternalLink :size="14" />
                  </template>
                  Vào lớp
                </GlassButton>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>

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
    </GlassPanel>
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

table {
  border-collapse: collapse;
  width: 100%;
}

th {
  background: var(--surface-input);
  text-align: left;
  white-space: nowrap;
}

td {
  border-top: 1px solid var(--border-card);
  vertical-align: middle;
}

tbody tr {
  transition: background 160ms ease;
}

tbody tr:hover {
  background: var(--surface-input);
}

.course-cell {
  gap: 0.75rem;
  min-width: 18rem;
}

.course-icon {
  width: 2.25rem;
  height: 2.25rem;
  border-radius: var(--radius-md);
}

.course-cell strong {
  display: block;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.875rem;
  font-weight: 900;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.course-cell small {
  gap: 0.25rem;
  margin-top: 0.25rem;
  font-size: 0.75rem;
  font-weight: 650;
}

.subject-chip {
  width: max-content;
  gap: 0.375rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-body);
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
  font-weight: 800;
}

.lesson-count {
  color: var(--text-heading);
  font-weight: 900;
}

.progress-cell {
  gap: 0.5rem;
  min-width: 8rem;
}

.progress-cell strong {
  color: var(--text-heading);
  font-size: 0.75rem;
  font-weight: 900;
}

.progress-line {
  flex: 1;
  min-width: 4rem;
  height: 0.5rem;
  overflow: hidden;
  border-radius: 999px;
  background: var(--surface-input);
  border: 1px solid var(--border-card);
}

.progress-line span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--text-link);
}

.text-right {
  text-align: right;
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
