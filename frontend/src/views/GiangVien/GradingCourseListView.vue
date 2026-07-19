<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { BookOpen, AlertCircle, Search } from 'lucide-vue-next'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const loading = ref(false)
const error = ref('')
const courses = ref([])
const searchQuery = ref('')

async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    const res = await teacherApi.getTeacherCourses()
    courses.value = res?.data ?? res?.Data ?? res ?? []
  } catch (err) {
    error.value = 'Không thể tải danh sách lớp học.'
  } finally {
    loading.value = false
  }
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

function goToGrading(course) {
  const courseId = course.courseId ?? course.CourseId
  router.push(`/teacher/grading-input/${courseId}`)
}
</script>

<template>
  <div v-if="loading" class="flex items-center justify-center min-h-[300px]">
    <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
    <span class="ml-3 text-muted text-sm">Đang tải dữ liệu...</span>
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <GlassButton size="sm" variant="secondary" @click="loadCourses">Thử lại</GlassButton>
  </div>
  <div v-else class="courses-page lg-page-enter">
    
    <!-- Header -->
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <span class="header-icon">
          <BookOpen :size="20" />
        </span>
        <div class="min-w-0">
          <div class="eyebrow">Teacher Grading</div>
          <h1 class="page-title">Nhập điểm kết thúc môn</h1>
          <p class="page-subtitle">
            Quản lý và nhập điểm cho các lớp đang giảng dạy
          </p>
        </div>
      </div>
    </GlassPanel>

    <!-- Context bar (Search) -->
    <GlassPanel variant="surface" density="compact" class="context-bar" :clip="false">
      <div class="flex-1">
        <div class="text-sm font-medium text-muted">
          Đang hiển thị {{ filteredCourses.length }} lớp học
        </div>
      </div>
      
      <div class="filters">
        <label class="search-field">
          <Search :size="15" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm tên khóa học hoặc lớp..."
          />
        </label>
      </div>
    </GlassPanel>

    <!-- Content Area -->
    <div class="courses-content-area mt-4">
      <div class="panel-heading mb-4 px-1">
        <div>
          <h2>Chọn Khóa học</h2>
          <p>Vui lòng chọn một lớp học để tiến hành nhập điểm</p>
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
          @click="goToGrading(course)"
          class="cursor-pointer"
        />
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
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-lg);
  background: var(--lg-primary);
  color: white;
  flex: none;
}

.page-title {
  font-size: 1.5rem;
  font-weight: 800;
  margin: 0;
  color: var(--text-heading);
}

.page-subtitle {
  margin: 0;
  color: var(--text-secondary);
}

.eyebrow {
  font-size: 0.75rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--lg-primary);
  margin-bottom: 0.25rem;
}

.search-field {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  padding: 0.5rem 0.75rem;
  width: 280px;
}

.search-field:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
}

.search-field input {
  background: transparent;
  border: none;
  outline: none;
  width: 100%;
  color: var(--text-body);
  font-size: 0.875rem;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.25rem;
}

@media (max-width: 768px) {
  .page-header,
  .context-bar {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filters {
    width: 100%;
  }
  
  .search-field {
    width: 100%;
  }
  
  .courses-grid {
    grid-template-columns: 1fr;
  }
}
</style>
