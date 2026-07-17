<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  Search, Filter, Users, BookOpen, Calendar, ChevronRight,
  MoreHorizontal, Eye, Download, GraduationCap, AlertCircle
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const classes = ref([])
const filterSemester = ref('')
const route = useRoute()

function mapCourseToClass(course) {
  return {
    id: course.courseId || course.CourseId,
    code: course.subjectCode || course.SubjectCode || '',
    name: course.courseName || course.CourseName || '',
    subject: `Lớp ${course.className || course.ClassName || ''}`,
    students: course.studentCount || course.StudentCount || 0,
    semester: course.semester || course.Semester || 'N/A',
  }
}

async function loadClasses() {
  loading.value = true
  error.value = ''
  try {
    const classId = route.query.classId
    const data = await teacherApi.getTeacherCourses({ 
      semesterId: filterSemester.value || undefined,
      classId: classId || undefined
    })
    const unwrapped = data?.data ?? data?.Data ?? data
    const extracted = Array.isArray(unwrapped) ? unwrapped : (unwrapped?.items ?? unwrapped?.Items ?? [])
    const items = Array.isArray(extracted) ? extracted : []
    classes.value = items.map(mapCourseToClass)
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách khóa học.'
    classes.value = []
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadClasses() })
</script>

<template>
  <div class="space-y-4 pb-10">
    <div v-if="loading" class="flex items-center justify-center min-h-[200px]">
      <div class="animate-spin w-8 h-8 border-2 border-blue-600 border-t-transparent rounded-full"></div>
      <span class="ml-3 text-muted text-sm">Đang tải danh sách lớp...</span>
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[200px] gap-4">
      <AlertCircle :size="40" class="text-rose-400" />
      <p class="text-rose-600 font-semibold">{{ error }}</p>
      <button @click="loadClasses" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white">Thử lại</button>
    </div>
    <template v-else>
      <!-- Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-bold text-heading tracking-tight">Tiến độ học tập các lớp</h1>
          <p class="text-muted mt-1">Chọn lớp học để theo dõi chi tiết tiến độ học tập của sinh viên.</p>
        </div>
        <div class="flex gap-2">
          <GlassButton variant="secondary" size="sm">
            <Download :size="18" /> Xuất báo cáo
          </GlassButton>
        </div>
      </div>

      <!-- Filters -->
      <div class="lg-glass-soft rounded-2xl p-4 flex flex-col md:flex-row gap-4 items-center">
        <div class="relative flex-1 w-full">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
          <input type="text" placeholder="Tìm theo mã khóa học, tên khóa học..." class="lg-control w-full pl-11 pr-4" />
        </div>
        <div class="flex items-center gap-3 w-full md:w-auto">
          <select v-model="filterSemester" class="lg-control flex-1 md:w-48">
            <option value="Spring 2026">Spring 2026</option>
            <option value="Fall 2025">Fall 2025</option>
          </select>
          <button class="lg-icon-button h-10 w-10 rounded-xl border border-card surface-card text-muted hover:text-heading hover:bg-(--accent-primary)/10 transition-all">
            <Filter :size="18" />
          </button>
        </div>
      </div>

      <!-- Grid / Table -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <TeacherClassCard
          v-for="cls in classes"
          :key="cls.id"
          :title="cls.code"
          :subtitle="cls.name"
          :semester="cls.semester"
          :studentsCount="cls.students"
        >
          <template #action>
            <router-link
              :to="'/teacher/class-progress/' + cls.id"
              class="w-full flex justify-center items-center gap-2 group-hover:bg-(--accent-primary) group-hover:text-inverse transition-all bg-slate-100 px-4 py-2 rounded-xl text-xs font-bold"
            >
              Xem tiến độ khóa học
              <Eye :size="14" />
            </router-link>
          </template>
        </TeacherClassCard>
      </div>
    </template>
  </div>
</template>
