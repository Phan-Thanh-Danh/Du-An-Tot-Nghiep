<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  BookOpen, Search, Filter, Eye, Download, GraduationCap, AlertCircle, Inbox, Lock, ArrowRight
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import { teacherApi } from '@/services/teacherApi'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const error = ref('')
const courses = ref([])
const searchQuery = ref('')
const filterSemester = ref('')

function mapCourse(course) {
  return {
    id: course.courseId || course.CourseId || course.id || course.Id,
    code: course.subjectCode || course.SubjectCode || course.code || '',
    name: course.courseName || course.CourseName || course.name || '',
    className: course.className || course.ClassName || '',
    subject: course.subjectName || course.SubjectName || course.name || '',
    students: course.studentCount || course.StudentCount || 0,
    lessonsCount: course.lessonCount || course.LessonCount || 12,
    semester: course.semester || course.Semester || 'Spring 2026',
  }
}

const availableSemesters = computed(() => {
  const set = new Set(courses.value.map(c => c.semester).filter(Boolean))
  return Array.from(set)
})

const filteredCourses = computed(() => {
  let list = courses.value
  if (filterSemester.value) {
    list = list.filter(c => c.semester === filterSemester.value)
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(c =>
      c.code.toLowerCase().includes(q) ||
      c.name.toLowerCase().includes(q) ||
      c.subject.toLowerCase().includes(q) ||
      c.className.toLowerCase().includes(q)
    )
  }
  return list
})

async function loadCourses() {
  loading.value = true
  error.value = ''
  try {
    let rawItems = []
    if (typeof teacherApi.getTeacherSubjects === 'function') {
      const res = await teacherApi.getTeacherSubjects()
      const unwrapped = res?.data ?? res?.Data ?? res
      rawItems = Array.isArray(unwrapped) ? unwrapped : (unwrapped?.items ?? unwrapped?.Items ?? [])
    }
    if (!rawItems || rawItems.length === 0) {
      const res = await teacherApi.getTeacherCourses()
      const unwrapped = res?.data ?? res?.Data ?? res
      rawItems = Array.isArray(unwrapped) ? unwrapped : (unwrapped?.items ?? unwrapped?.Items ?? [])
    }

    // Group by Subject Code / Subject Name to display DISTINCT MÔN HỌC!
    const subjectMap = new Map()
    rawItems.forEach(item => {
      const code = item.subjectCode || item.SubjectCode || item.code || item.Code || 'MH01'
      const name = item.subjectName || item.SubjectName || item.courseName || item.CourseName || item.name || 'Môn học'
      const id = item.subjectId || item.SubjectId || item.courseId || item.CourseId || item.id || item.Id
      const className = item.className || item.ClassName || ''
      const students = item.studentCount || item.StudentCount || 0
      const semester = item.semester || item.Semester || 'Spring 2026'

      if (!subjectMap.has(code)) {
        subjectMap.set(code, {
          id: id,
          code: code,
          name: name,
          classes: className ? [className] : [],
          students: students,
          semester: semester,
        })
      } else {
        const existing = subjectMap.get(code)
        if (className && !existing.classes.includes(className)) {
          existing.classes.push(className)
        }
        existing.students += students
      }
    })

    courses.value = Array.from(subjectMap.values()).map(s => ({
      ...s,
      displaySubtitle: `${s.name}${s.classes.length ? ` · Giảng dạy: ${s.classes.join(', ')}` : ''}`
    }))
  } catch (err) {
    console.error('Error loading teacher subjects:', err)
    error.value = err?.message || 'Không thể tải danh sách môn học.'
    courses.value = []
  } finally {
    loading.value = false
  }
}

function goToDetail(courseId) {
  router.push(`/teacher/lessons/${courseId}`)
}

onMounted(() => {
  loadCourses()
})
</script>

<template>
  <div class="space-y-4 pb-10">
    <!-- Header -->
    <GlassPanel variant="soft" density="compact" class="flex flex-col md:flex-row md:items-center justify-between gap-4" :clip="false">
      <div class="flex items-center gap-3">
        <span class="w-10 h-10 rounded-2xl bg-(--accent-primary-soft) text-(--accent-primary) flex items-center justify-center border border-card shadow-xs">
          <BookOpen :size="20" />
        </span>
        <div>
          <div class="flex items-center gap-2">
            <h1 class="text-xl font-bold text-heading tracking-tight">Bài học & Học liệu giảng dạy</h1>
            <GlassBadge variant="info" size="sm">
              <Lock :size="11" /> Chỉ xem
            </GlassBadge>
          </div>
          <p class="text-muted text-xs mt-0.5">
            Danh sách các môn học bạn đang giảng dạy. Chọn môn học để xem nội dung bài giảng & học liệu do Hội đồng biên soạn.
          </p>
        </div>
      </div>
    </GlassPanel>

    <!-- Filters -->
    <div class="lg-glass-soft rounded-2xl p-4 flex flex-col md:flex-row gap-4 items-center">
      <div class="relative flex-1 w-full">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm môn học theo tên, mã môn, mã lớp..."
          class="lg-control w-full pl-11 pr-4"
        />
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <select v-model="filterSemester" class="lg-control flex-1 md:w-48">
          <option value="">Tất cả học kỳ</option>
          <option v-for="sem in availableSemesters" :key="sem" :value="sem">{{ sem }}</option>
        </select>
        <button
          @click="searchQuery = ''; filterSemester = ''"
          title="Xóa bộ lọc"
          class="lg-icon-button h-10 w-10 rounded-xl border border-card surface-card text-muted hover:text-heading hover:bg-(--accent-primary)/10 transition-all flex items-center justify-center shrink-0"
        >
          <Filter :size="18" />
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex items-center justify-center min-h-[250px]">
      <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
      <span class="ml-3 text-muted text-sm">Đang tải danh sách môn học...</span>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[250px] gap-4">
      <AlertCircle :size="40" class="text-rose-400" />
      <p class="text-rose-600 font-semibold">{{ error }}</p>
      <button @click="loadCourses" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white">Thử lại</button>
    </div>

    <!-- Empty search -->
    <div v-else-if="filteredCourses.length === 0" class="flex flex-col items-center justify-center py-12 text-center surface-card border border-card rounded-2xl gap-3">
      <Inbox :size="40" class="text-muted/50" />
      <p class="text-heading font-semibold text-sm">Không tìm thấy môn học nào</p>
      <p class="text-muted text-xs">Thử tìm kiếm lại với từ khóa khác</p>
      <button
        @click="searchQuery = ''; filterSemester = ''"
        class="mt-1 px-3 py-1.5 rounded-lg bg-(--accent-primary-soft) text-(--accent-primary) text-xs font-bold hover:opacity-90 transition-all"
      >
        Xóa bộ lọc
      </button>
    </div>

    <!-- Course Grid -->
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <TeacherClassCard
        v-for="cls in filteredCourses"
        :key="cls.id"
        :title="cls.code"
        :subtitle="cls.displaySubtitle || cls.name"
        :semester="cls.semester"
        :studentsCount="cls.students"
      >
        <template #action>
          <GlassButton
            variant="primary"
            size="sm"
            class="w-full justify-center text-xs font-bold"
            @click="goToDetail(cls.id)"
          >
            <template #leading>
              <BookOpen :size="14" />
            </template>
            Xem nội dung môn học
            <template #trailing>
              <ArrowRight :size="14" />
            </template>
          </GlassButton>
        </template>
      </TeacherClassCard>
    </div>
  </div>
</template>
