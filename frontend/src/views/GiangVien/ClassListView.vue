<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  Search, Filter, Users, BookOpen, Calendar, ChevronRight,
  Eye, Download, GraduationCap, AlertCircle, Inbox
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const classes = ref([])
const searchQuery = ref('')
const filterSemester = ref('')

function mapCourseToClass(course) {
  const sCount = course.studentCount ?? course.StudentCount ?? course.siSo ?? course.SiSo ?? 0
  const cId = course.classId ?? course.ClassId ?? course.maKhoaHoc ?? course.MaKhoaHoc ?? course.id ?? course.Id
  const cName = course.className ?? course.ClassName ?? course.tenLop ?? course.TenLop ?? course.tieuDe ?? course.TieuDe ?? course.name ?? 'Lớp học'
  const cSubj = course.subjectName ?? course.SubjectName ?? course.tenMonHoc ?? course.TenMonHoc ?? course.subject ?? (course.courseCount || course.CourseCount ? `${course.courseCount || course.CourseCount} môn học` : 'Môn học')
  const cSem = course.semester ?? course.Semester ?? course.tenHocKy ?? course.TenHocKy ?? 'Học kỳ 1 năm 2026'

  return {
    id: cId,
    code: cName.split(' - ')[0] || cName,
    name: cName,
    subject: cSubj,
    students: sCount,
    semester: cSem,
  }
}

async function loadClasses() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getClasses({ semesterId: filterSemester.value || undefined })
    const extracted = data?.data?.items ?? data?.items ?? data?.data ?? data
    const items = Array.isArray(extracted) ? extracted : []
    classes.value = items.map(mapCourseToClass)
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách lớp.'
    classes.value = []
  } finally {
    loading.value = false
  }
}

const availableSemesters = computed(() => {
  const set = new Set(classes.value.map(c => c.semester).filter(Boolean))
  return Array.from(set)
})

const filteredClasses = computed(() => {
  let list = classes.value
  if (filterSemester.value) {
    list = list.filter(c => c.semester === filterSemester.value)
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(c =>
      c.code.toLowerCase().includes(q) ||
      c.name.toLowerCase().includes(q) ||
      c.subject.toLowerCase().includes(q)
    )
  }
  return list
})

function exportReport() {
  const headers = ['ID', 'Mã Lớp', 'Tên Lớp', 'Môn Học', 'Sĩ Số', 'Học Kỳ']
  const rows = filteredClasses.value.map(c => [
    c.id, c.code, `"${c.name}"`, `"${c.subject}"`, c.students, c.semester
  ])
  const csvContent = 'data:text/csv;charset=utf-8,\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n')
  const encodedUri = encodeURI(csvContent)
  const link = document.createElement('a')
  link.setAttribute('href', encodedUri)
  link.setAttribute('download', `Danh_sach_lop_giang_vien_${new Date().toISOString().slice(0, 10)}.csv`)
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
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
          <h1 class="text-xl font-bold text-heading tracking-tight">Danh sách lớp học</h1>
          <p class="text-muted mt-1">Quản lý và theo dõi các lớp học bạn đang phụ trách giảng dạy.</p>
        </div>
        <div class="flex gap-2">
          <GlassButton variant="secondary" size="sm" @click="exportReport">
            <Download :size="18" /> Xuất báo cáo
          </GlassButton>
        </div>
      </div>

      <!-- Filters -->
      <div class="lg-glass-soft rounded-2xl p-4 flex flex-col md:flex-row gap-4 items-center">
        <div class="relative flex-1 w-full">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
          <input v-model="searchQuery" type="text" placeholder="Tìm theo mã lớp, tên môn..." class="lg-control w-full pl-11 pr-4" />
        </div>
        <div class="flex items-center gap-3 w-full md:w-auto">
          <LmsSelect v-model="filterSemester" class="w-48">
            <option value="">Tất cả học kỳ</option>
            <option v-for="sem in availableSemesters" :key="sem" :value="sem">{{ sem }}</option>
          </LmsSelect>
        </div>
      </div>

      <!-- Grid / Table -->
      <div v-if="filteredClasses.length === 0" class="py-12 text-center surface-card border border-card rounded-2xl">
        <Inbox :size="40" class="mx-auto text-muted/50 mb-2" />
        <p class="text-heading font-semibold text-sm">Không tìm thấy lớp học nào</p>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="cls in filteredClasses" :key="cls.id" class="lg-glass-soft lg-card-hover rounded-2xl p-4 flex flex-col justify-between">
          <div>
            <div class="flex justify-between items-start mb-4">
              <div class="h-10 w-10 rounded-2xl bg-(--accent-primary)/10 flex items-center justify-center text-link border border-(--accent-primary)/20">
                <GraduationCap :size="24" />
              </div>
              <GlassBadge variant="primary" size="sm">{{ cls.semester }}</GlassBadge>
            </div>

            <div>
              <h3 class="text-xl font-bold text-heading">{{ cls.code }}</h3>
              <p class="text-sm font-semibold text-label mt-1">{{ cls.name }}</p>

              <div class="mt-4 space-y-2.5">
                <div class="flex items-center gap-3 text-xs text-body">
                  <BookOpen :size="15" class="text-muted" />
                  <span>Môn: <span class="font-bold text-heading">{{ cls.subject }}</span></span>
                </div>
                <div class="flex items-center gap-3 text-xs text-body">
                  <Users :size="15" class="text-muted" />
                  <span>Sĩ số: <span class="font-bold text-heading">{{ cls.students }} sinh viên</span></span>
                </div>
              </div>
            </div>
          </div>

          <div class="mt-6 pt-4 border-t border-card">
             <router-link :to="'/teacher/classes/' + cls.id + '/workspace'" class="w-full rounded-xl bg-(--accent-primary) py-2.5 text-xs font-bold text-white hover:opacity-90 transition-all flex items-center justify-center gap-2 shadow-sm">
                <Eye :size="15" /> Xem chi tiết lớp học
             </router-link>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
