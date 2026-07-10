<script setup>
import { computed, ref, onMounted } from 'vue'
import {
  BookOpen,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Clock,
  LayoutGrid,
  List,
  PlayCircle,
  Search,
  Users,
  AlertCircle,
  Loader2
} from 'lucide-vue-next'
import LmsBadge from '@/components/LmsBadge.vue'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'

import { studentApi } from '@/services/studentApi'

const rawCourses = ref([])
const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  try {
    loading.value = true
    const res = await studentApi.getCourses()
    const isSuccess = res.success === true || res.Success === true
    if (isSuccess) {
      rawCourses.value = res.data || res.Data || []
    } else {
      error.value = res.message || res.Message || 'Lỗi khi tải danh sách khóa học'
    }
  } catch (err) {
    console.error('Failed to load courses', err)
    error.value = 'Lỗi kết nối đến máy chủ.'
  } finally {
    loading.value = false
  }
})

const courses = computed(() => {
  if (!rawCourses.value) return []
  return rawCourses.value.map((course) => {
    let status = 'learning'
    const progress = course.progress || course.Progress || 0
    const courseStatus = course.status || course.Status || ''
    
    if (progress === 100 || courseStatus === 'Hoàn thành' || courseStatus === 'completed') {
      status = 'completed'
    } else if (courseStatus === 'Sắp tới' || courseStatus === 'upcoming') {
      status = 'upcoming'
    } else {
      status = 'learning' // Đang học
    }

    return {
      id: course.code || course.Code || (course.id || course.Id || '').toUpperCase(),
      name: course.name || course.Name,
      instructor: course.lecturer || course.Lecturer || 'Giảng viên phụ trách',
      credits: course.credits || course.Credits || 3,
      progress: progress,
      totalSessions: course.total || course.Total || 15,
      completedSessions: course.completed || course.Completed || 0,
      status: status,
      lastAccessed: courseStatus || 'Chưa bắt đầu',
    }
  })
})

const viewMode = ref('grid')
const searchQuery = ref('')
const selectedFilter = ref('all')

const statusMeta = {
  learning: { label: 'Đang học', badge: 'info', className: 'status-learning' },
  completed: { label: 'Hoàn thành', badge: 'success', className: 'status-completed' },
  upcoming: { label: 'Sắp tới', badge: 'warning', className: 'status-upcoming' },
}

const filteredCourses = computed(() => {
  const keyword = searchQuery.value.trim().toLowerCase()

  return courses.value.filter((course) => {
    const matchesKeyword = !keyword ||
      course.name.toLowerCase().includes(keyword) ||
      course.id.toLowerCase().includes(keyword) ||
      course.instructor.toLowerCase().includes(keyword)
    const matchesStatus = selectedFilter.value === 'all' || course.status === selectedFilter.value

    return matchesKeyword && matchesStatus
  })
})

const averageProgress = computed(() => {
  if (!courses.value.length) return 0
  const total = courses.value.reduce((sum, course) => sum + course.progress, 0)
  return Math.round(total / courses.value.length)
})

const courseSummary = computed(() => [
  {
    label: 'Đang học',
    value: `${courses.value.filter((course) => course.status === 'learning').length} môn`,
    icon: BookOpen,
    tone: 'info',
  },
  {
    label: 'Hoàn thành',
    value: `${courses.value.filter((course) => course.status === 'completed').length} môn`,
    icon: CheckCircle2,
    tone: 'success',
  },
  {
    label: 'Sắp tới',
    value: `${courses.value.filter((course) => course.status === 'upcoming').length} môn`,
    icon: Clock,
    tone: 'warning',
  },
  {
    label: 'Tiến độ TB',
    value: `${averageProgress.value}%`,
    icon: PlayCircle,
    tone: 'primary',
  },
])
</script>

<template>
  <div class="courses-page">
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>

    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle class="h-12 w-12 text-red-500 mb-4" />
      <h3 class="text-lg font-semibold text-slate-800">Không thể tải dữ liệu</h3>
      <p class="text-slate-500 max-w-md">{{ error }}</p>
    </div>

    <template v-else>
    <section class="courses-header">
      <div class="title-block">
        <span class="eyebrow">
          <BookOpen :size="14" />
          Học tập
        </span>
        <div>
          <h1>Khóa học</h1>
          <p>Theo dõi môn đang học, tiến độ buổi học và truy cập nhanh vào nội dung.</p>
        </div>
      </div>

      <div class="summary-strip" aria-label="Tổng quan khóa học">
        <article
          v-for="item in courseSummary"
          :key="item.label"
          class="summary-pill"
          :class="`summary-${item.tone}`"
        >
          <component :is="item.icon" :size="15" />
          <div>
            <strong>{{ item.value }}</strong>
            <span>{{ item.label }}</span>
          </div>
        </article>
      </div>
    </section>

    <section class="filter-panel" aria-label="Bộ lọc khóa học">
      <div class="search-field">
        <Search :size="16" />
        <input
          v-model="searchQuery"
          type="search"
          placeholder="Tìm theo môn, mã môn, giảng viên..."
          aria-label="Tìm kiếm khóa học"
        >
      </div>

      <label class="select-field">
        <select v-model="selectedFilter" aria-label="Lọc khóa học">
          <option value="all">Tất cả khóa học</option>
          <option value="learning">Đang học</option>
          <option value="completed">Đã hoàn thành</option>
          <option value="upcoming">Sắp diễn ra</option>
        </select>
        <ChevronDown :size="15" />
      </label>

      <div class="view-toggle" aria-label="Chế độ hiển thị">
        <button
          type="button"
          :class="{ active: viewMode === 'grid' }"
          title="Lưới"
          aria-label="Hiển thị dạng lưới"
          @click="viewMode = 'grid'"
        >
          <LayoutGrid :size="17" />
        </button>
        <button
          type="button"
          :class="{ active: viewMode === 'list' }"
          title="Danh sách"
          aria-label="Hiển thị dạng danh sách"
          @click="viewMode = 'list'"
        >
          <List :size="17" />
        </button>
      </div>
    </section>

    <section
      v-if="filteredCourses.length && viewMode === 'grid'"
      class="course-grid"
      aria-label="Danh sách khóa học dạng lưới"
    >
      <router-link
        v-for="course in filteredCourses"
        :key="course.id"
        :to="`/student/courses/${course.id}`"
        class="course-card"
      >
        <header class="course-card-header">
          <span class="subject-code">{{ course.id }}</span>
          <LmsBadge :variant="statusMeta[course.status].badge" size="sm">
            {{ statusMeta[course.status].label }}
          </LmsBadge>
        </header>

        <div class="course-main">
          <h2>{{ course.name }}</h2>
          <p>
            <Users :size="13" />
            {{ course.instructor }}
          </p>
        </div>

        <dl class="course-facts">
          <div>
            <dt>Tín chỉ</dt>
            <dd>{{ course.credits }}</dd>
          </div>
          <div>
            <dt>Buổi học</dt>
            <dd>{{ course.completedSessions }}/{{ course.totalSessions }}</dd>
          </div>
          <div>
            <dt>Truy cập</dt>
            <dd>{{ course.lastAccessed }}</dd>
          </div>
        </dl>

        <div class="progress-block">
          <div class="progress-copy">
            <span>Tiến độ</span>
            <strong>{{ course.progress }}%</strong>
          </div>
          <div class="progress-track">
            <div class="progress-fill" :style="{ width: `${course.progress}%` }" />
          </div>
        </div>

        <footer>
          <span :class="['status-dot', statusMeta[course.status].className]" />
          <span>{{ statusMeta[course.status].label }}</span>
          <strong>
            Tiếp tục
            <ChevronRight :size="14" />
          </strong>
        </footer>
      </router-link>
    </section>

    <section
      v-else-if="filteredCourses.length"
      class="course-list"
      aria-label="Danh sách khóa học dạng danh sách"
    >
      <router-link
        v-for="course in filteredCourses"
        :key="course.id"
        :to="`/student/courses/${course.id}`"
        class="course-row"
      >
        <div class="course-row-code">
          <span>{{ course.id }}</span>
          <small>{{ course.credits }} tín chỉ</small>
        </div>

        <div class="course-row-main">
          <h2>{{ course.name }}</h2>
          <p>{{ course.instructor }} · Truy cập {{ course.lastAccessed }}</p>
        </div>

        <div class="row-progress">
          <div class="progress-copy">
            <span>{{ course.completedSessions }}/{{ course.totalSessions }} buổi</span>
            <strong>{{ course.progress }}%</strong>
          </div>
          <div class="progress-track">
            <div class="progress-fill" :style="{ width: `${course.progress}%` }" />
          </div>
        </div>

        <span class="row-status" :class="statusMeta[course.status].className">
          {{ statusMeta[course.status].label }}
        </span>

        <ChevronRight class="row-arrow" :size="16" />
      </router-link>
    </section>

    <section v-else class="empty-state">
      <BookOpen :size="34" />
      <h2>Không tìm thấy khóa học</h2>
      <p>Thử đổi từ khóa hoặc bộ lọc trạng thái.</p>
    </section>
    </template>
  </div>
</template>

<style scoped>
.courses-page {
  display: grid;
  gap: 1rem;
  padding-bottom: 2.5rem;
}

.courses-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 1rem;
}

.title-block {
  display: grid;
  gap: 0.45rem;
  min-width: 0;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.title-block h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.35rem;
  font-weight: 900;
  line-height: 1.15;
}

.title-block p {
  margin: 0.2rem 0 0;
  color: var(--text-body);
  font-size: 0.82rem;
}

.summary-strip {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 0.45rem;
}

.summary-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.45rem;
  min-height: 2.45rem;
  border: 1px solid var(--border-card);
  border-radius: 16px;
  background: var(--surface-card);
  color: var(--text-link);
  padding: 0.45rem 0.65rem;
  box-shadow: var(--lg-shadow-sm);
}

.summary-pill div {
  display: grid;
  gap: 0.05rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 0.9rem;
  font-weight: 900;
  line-height: 1;
}

.summary-pill span {
  color: var(--text-placeholder);
  font-size: 0.66rem;
  font-weight: 850;
  text-transform: uppercase;
}

.summary-info { box-shadow: inset 3px 0 0 var(--color-info-text), var(--lg-shadow-sm); }
.summary-success { box-shadow: inset 3px 0 0 var(--color-success-text), var(--lg-shadow-sm); }
.summary-warning { box-shadow: inset 3px 0 0 var(--color-warning-text), var(--lg-shadow-sm); }
.summary-primary { box-shadow: inset 3px 0 0 var(--text-link), var(--lg-shadow-sm); }

.filter-panel {
  display: grid;
  grid-template-columns: minmax(14rem, 1fr) minmax(12rem, 0.28fr) auto;
  gap: 0.55rem;
  border: 1px solid var(--border-card);
  border-radius: 20px;
  background: var(--surface-card);
  padding: 0.65rem;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
}

.search-field,
.select-field,
.view-toggle {
  display: flex;
  align-items: center;
  min-height: 2.4rem;
  border: 1px solid var(--border-input);
  border-radius: 14px;
  background: var(--surface-input);
  color: var(--text-placeholder);
}

.search-field,
.select-field {
  gap: 0.45rem;
  padding: 0 0.7rem;
}

.search-field input,
.select-field select {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-label);
  font-size: 0.82rem;
  font-weight: 680;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.select-field select {
  cursor: pointer;
  appearance: none;
}

.view-toggle {
  gap: 0.2rem;
  padding: 0.2rem;
}

.view-toggle button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2rem;
  height: 2rem;
  border: 0;
  border-radius: 11px;
  background: transparent;
  color: var(--text-placeholder);
  cursor: pointer;
}

.view-toggle button.active {
  background: var(--surface-card-strong);
  color: var(--text-link);
  box-shadow: var(--lg-shadow-sm);
}

.course-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(16rem, 1fr));
  gap: 0.8rem;
}

.course-card,
.course-row,
.empty-state {
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(calc(var(--glass-blur) - 4px)) saturate(130%);
}

.course-card {
  display: grid;
  gap: 0.75rem;
  min-height: 16.4rem;
  border-radius: 20px;
  padding: 0.85rem;
  color: inherit;
  text-decoration: none;
  transition: transform 180ms ease, border-color 180ms ease, box-shadow 180ms ease;
}

.course-card:hover,
.course-row:hover {
  transform: translateY(-2px);
  border-color: var(--border-input-focus);
  box-shadow: var(--lg-shadow-md);
}

.course-card-header,
.course-card footer,
.progress-copy {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.6rem;
}

.subject-code {
  color: var(--text-link);
  font-size: 0.74rem;
  font-weight: 950;
}

.course-main {
  display: grid;
  gap: 0.45rem;
}

.course-main h2,
.course-row-main h2 {
  display: -webkit-box;
  margin: 0;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  color: var(--text-heading);
  font-size: 0.98rem;
  font-weight: 900;
  line-height: 1.28;
}

.course-main p,
.course-row-main p {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin: 0;
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 720;
}

.course-facts {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.45rem;
  margin: 0;
}

.course-facts div {
  min-width: 0;
  border: 1px solid var(--border-card);
  border-radius: 13px;
  background: var(--surface-input);
  padding: 0.45rem 0.5rem;
}

.course-facts dt {
  color: var(--text-placeholder);
  font-size: 0.62rem;
  font-weight: 850;
  text-transform: uppercase;
}

.course-facts dd {
  margin: 0.16rem 0 0;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.72rem;
  font-weight: 850;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.progress-block {
  display: grid;
  gap: 0.35rem;
}

.progress-copy span {
  color: var(--text-placeholder);
  font-size: 0.68rem;
  font-weight: 850;
  text-transform: uppercase;
}

.progress-copy strong {
  color: var(--text-heading);
  font-size: 0.78rem;
  font-weight: 900;
}

.progress-track {
  height: 0.48rem;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--accent-primary), var(--accent-cyan));
}

.course-card footer {
  margin-top: auto;
  border-top: 1px solid var(--border-card);
  padding-top: 0.65rem;
  color: var(--text-label);
  font-size: 0.72rem;
  font-weight: 800;
}

.course-card footer strong {
  display: inline-flex;
  align-items: center;
  gap: 0.2rem;
  margin-left: auto;
  color: var(--text-link);
}

.status-dot {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 999px;
  background: var(--text-placeholder);
}

.status-learning { color: var(--color-info-text); background: var(--color-info-bg); }
.status-completed { color: var(--color-success-text); background: var(--color-success-bg); }
.status-upcoming { color: var(--color-warning-text); background: var(--color-warning-bg); }

.status-dot.status-learning { background: var(--color-info-text); }
.status-dot.status-completed { background: var(--color-success-text); }
.status-dot.status-upcoming { background: var(--color-warning-text); }

.course-list {
  display: grid;
  gap: 0.55rem;
}

.course-row {
  display: grid;
  grid-template-columns: minmax(5.5rem, 0.14fr) minmax(0, 1fr) minmax(10rem, 0.24fr) auto auto;
  align-items: center;
  gap: 0.8rem;
  border-radius: 18px;
  padding: 0.7rem;
  color: inherit;
  text-decoration: none;
  transition: transform 180ms ease, border-color 180ms ease, box-shadow 180ms ease;
}

.course-row-code,
.course-row-main {
  min-width: 0;
}

.course-row-code {
  display: grid;
  gap: 0.12rem;
}

.course-row-code span {
  color: var(--text-link);
  font-size: 0.78rem;
  font-weight: 950;
}

.course-row-code small,
.row-arrow {
  color: var(--text-placeholder);
}

.course-row-main h2 {
  -webkit-line-clamp: 1;
  font-size: 0.9rem;
}

.course-row-main p {
  display: block;
  margin-top: 0.18rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.row-progress {
  display: grid;
  gap: 0.3rem;
}

.row-status {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 1.55rem;
  border-radius: 999px;
  padding: 0 0.55rem;
  font-size: 0.66rem;
  font-weight: 850;
  white-space: nowrap;
}

.empty-state {
  display: grid;
  place-items: center;
  min-height: 14rem;
  border-radius: 20px;
  padding: 1rem;
  color: var(--text-placeholder);
  text-align: center;
}

.empty-state h2 {
  margin: 0.45rem 0 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 880;
}

.empty-state p {
  margin: 0.25rem 0 0;
  color: var(--text-label);
  font-size: 0.82rem;
}

@media (max-width: 1024px) {
  .courses-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .summary-strip {
    justify-content: flex-start;
  }

  .filter-panel {
    grid-template-columns: 1fr auto;
  }

  .search-field {
    grid-column: 1 / -1;
  }

  .course-row {
    grid-template-columns: minmax(0, 1fr) auto;
  }

  .course-row-code,
  .row-progress,
  .row-status {
    grid-column: 1 / -1;
  }

  .course-row-code {
    display: flex;
    align-items: center;
    gap: 0.45rem;
  }
}

@media (max-width: 640px) {
  .filter-panel,
  .course-grid {
    grid-template-columns: 1fr;
  }

  .view-toggle {
    width: fit-content;
  }

  .summary-pill {
    flex: 1 1 calc(50% - 0.5rem);
  }

  .course-facts {
    grid-template-columns: 1fr;
  }
}
</style>
