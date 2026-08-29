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

import LmsSelect from '@/components/LmsSelect.vue'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const error = ref('')
const courses = ref([])
const searchQuery = ref('')
const filterSemester = ref('')

const availableSemesters = computed(() => {
  const set = new Set(courses.value.map(c => c.semester).filter(Boolean))
  return Array.from(set)
})

const semesterOptions = computed(() => [
  { value: '', label: 'Tất cả học kỳ' },
  ...availableSemesters.value.map(s => ({ value: s, label: s }))
])

const filteredCourses = computed(() => {
  let list = courses.value
  if (filterSemester.value) {
    list = list.filter(c => (c.semester || '') === filterSemester.value)
  }
  if (searchQuery.value && searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(c => {
      const code = (c.code || '').toLowerCase()
      const name = (c.name || '').toLowerCase()
      const classesStr = (c.classes || []).join(' ').toLowerCase()
      const semester = (c.semester || '').toLowerCase()
      return code.includes(q) || name.includes(q) || classesStr.includes(q) || semester.includes(q)
    })
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

    const subjectMap = new Map()
    rawItems.forEach(item => {
      const code = item.subjectCode || item.SubjectCode || item.code || item.Code || ''
      const name = item.subjectName || item.SubjectName || item.courseName || item.CourseName || item.name || ''
      const id = item.subjectId || item.SubjectId || item.courseId || item.CourseId || item.id || item.Id

      let classList = []
      if (Array.isArray(item.classes || item.Classes)) {
        classList = item.classes || item.Classes
      } else if (item.className || item.ClassName) {
        classList = (item.className || item.ClassName).split(',').map(s => s.trim()).filter(Boolean)
      }

      const classCount = item.classCount ?? item.ClassCount ?? classList.length
      const students = item.studentCount ?? item.StudentCount ?? item.siSo ?? 0
      const semester = item.semester || item.Semester || item.tenHocKy || 'Học kỳ 1 năm 2026'
      const lessonCount = item.lessonCount ?? item.LessonCount ?? item.lessonsCount ?? item.LessonsCount ?? 0

      const key = `${code}_${semester}`
      if (!subjectMap.has(key)) {
        subjectMap.set(key, {
          id: id,
          code: code,
          name: name,
          classes: [...classList],
          classCount: classCount || classList.length || 1,
          students: students,
          lessonsCount: lessonCount,
          semester: semester,
        })
      } else {
        const existing = subjectMap.get(key)
        classList.forEach(cn => {
          if (cn && !existing.classes.includes(cn)) {
            existing.classes.push(cn)
          }
        })
        existing.classCount = existing.classes.length
        existing.students += students
        if (lessonCount > 0) existing.lessonsCount = lessonCount
      }
    })

    courses.value = Array.from(subjectMap.values()).map(s => {
      const actualClassCount = s.classCount || s.classes.length || 1
      return {
        ...s,
        displaySubtitle: s.classes.length ? `Lớp: ${s.classes.join(', ')}` : 'Chưa có lớp',
        displayStudentCount: `${s.students} (${actualClassCount} lớp)`,
      }
    })
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
        <LmsSelect
          v-model="filterSemester"
          :options="semesterOptions"
          class="w-56"
          placeholder="Chọn học kỳ"
        />
        <button
          @click="searchQuery = ''; filterSemester = ''"
          title="Lọc / Đặt lại bộ lọc"
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
        :title="`${cls.code} - ${cls.name}`"
        :subtitle="cls.displaySubtitle"
        :semester="cls.semester"
        :studentsCount="cls.displayStudentCount"
        :lessonsCount="cls.lessonsCount ?? 0"
      >
        <template #action>
          <GlassButton
            variant="primary"
            size="sm"
            class="w-full justify-center text-xs font-bold"
            @click="goToDetail(cls.code || cls.id)"
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
