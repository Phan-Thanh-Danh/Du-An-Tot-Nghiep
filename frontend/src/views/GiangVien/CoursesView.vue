<script setup>
import { ref, computed } from 'vue'
import {
  Plus,
  Search,
  Filter,
  MoreHorizontal,
  Edit2,
  Send,
  Trash2,
  ExternalLink,
  BookOpen,
  Calendar,
  Layers,
  CheckCircle2,
  Clock,
  AlertCircle
} from 'lucide-vue-next'

// ── Mock Data ──────────────────────────────────────────────
const courses = ref([
  { id: 1, name: 'Lập trình Web nâng cao', subject: 'CNTT', lessons: 24, status: 'Published', semester: 'Spring 2026' },
  { id: 2, name: 'Cấu trúc dữ liệu & Giải thuật', subject: 'CNTT', lessons: 18, status: 'Published', semester: 'Spring 2026' },
  { id: 3, name: 'Cơ sở dữ liệu', subject: 'CNTT', lessons: 15, status: 'Draft', semester: 'Spring 2026' },
  { id: 4, name: 'Lập trình hướng đối tượng (Java)', subject: 'CNTT', lessons: 22, status: 'Published', semester: 'Fall 2025' },
  { id: 5, name: 'Trí tuệ nhân tạo cơ bản', subject: 'CNTT', lessons: 12, status: 'Archived', semester: 'Fall 2025' },
])

const semesters = ['Spring 2026', 'Fall 2025', 'Summer 2025']
const subjects = ['CNTT', 'Kinh tế', 'Ngôn ngữ', 'Thiết kế']

const filterSemester = ref('Spring 2026')
const filterSubject = ref('Tất cả')
const searchQuery = ref('')

// ── Computed ──────────────────────────────────────────────
const filteredCourses = computed(() => {
  return courses.value.filter(c => {
    const matchSemester = filterSemester.value === 'Tất cả' || c.semester === filterSemester.value
    const matchSubject = filterSubject.value === 'Tất cả' || c.subject === filterSubject.value
    const matchSearch = c.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    return matchSemester && matchSubject && matchSearch
  })
})

const getStatusStyles = (status) => {
  switch (status) {
    case 'Published': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
    case 'Draft': return 'bg-amber-50 text-amber-600 border-amber-100'
    case 'Archived': return 'bg-slate-50 text-slate-500 border-slate-100'
    default: return 'bg-slate-50 text-slate-500 border-slate-100'
  }
}

// ── Actions ───────────────────────────────────────────────
function createCourse() {
  alert('Chức năng tạo khóa học mới')
}

function editCourse(id) {
  console.log('Edit course:', id)
}

function publishCourse(id) {
  const course = courses.value.find(c => c.id === id)
  if (course) {
    course.status = 'Published'
  }
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Quản lý Khóa học</h1>
        <p class="text-slate-500 mt-1">Tạo, chỉnh sửa và quản lý nội dung các khóa học bạn phụ trách.</p>
      </div>
      <button @click="createCourse" class="lg-button-primary" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
        <Plus :size="20" />
        Tạo khóa học mới
      </button>
    </div>

    <!-- Filters Bar -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm">
      <div class="flex flex-col lg:flex-row gap-4">
        <!-- Search -->
        <div class="relative flex-1">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input 
            v-model="searchQuery"
            type="text" 
            placeholder="Tìm tên khóa học..." 
            class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300 transition-all"
          />
        </div>
        
        <!-- Dropdowns -->
        <div class="flex flex-wrap gap-3">
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-slate-400 uppercase tracking-wider">Học kỳ</span>
            <select v-model="filterSemester" class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none focus:border-indigo-300">
              <option value="Tất cả">Tất cả</option>
              <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
            </select>
          </div>
          
          <div class="flex items-center gap-2">
            <span class="text-xs font-bold text-slate-400 uppercase tracking-wider">Môn học</span>
            <select v-model="filterSubject" class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none focus:border-indigo-300">
              <option value="Tất cả">Tất cả</option>
              <option v-for="subj in subjects" :key="subj" :value="subj">{{ subj }}</option>
            </select>
          </div>

          <button class="rounded-xl border border-slate-200 p-2.5 text-slate-400 hover:bg-slate-50 transition-colors">
            <Filter :size="18" />
          </button>
        </div>
      </div>
    </div>

    <!-- Courses Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Tên khóa học</th>
              <th class="px-6 py-5">Môn học</th>
              <th class="px-6 py-5">Số bài học</th>
              <th class="px-6 py-5">Trạng thái</th>
              <th class="px-8 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="course in filteredCourses" :key="course.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-4">
                  <div class="h-11 w-11 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 shadow-sm border border-indigo-100/50 group-hover:scale-110 transition-transform">
                    <BookOpen :size="22" />
                  </div>
                  <div>
                    <p class="text-[15px] font-bold text-slate-800">{{ course.name }}</p>
                    <p class="text-xs text-slate-400 mt-0.5 flex items-center gap-1">
                      <Calendar :size="12" /> {{ course.semester }}
                    </p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="inline-flex items-center gap-1.5 rounded-lg bg-slate-100 px-2.5 py-1 text-xs font-bold text-slate-600">
                  <Layers :size="14" />
                  {{ course.subject }}
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2">
                  <span class="text-sm font-black text-slate-700">{{ course.lessons }}</span>
                  <span class="text-xs text-slate-400 font-medium">bài giảng</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div :class="['inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-[11px] font-extrabold uppercase tracking-tight', getStatusStyles(course.status)]">
                  <CheckCircle2 v-if="course.status === 'Published'" :size="12" />
                  <Clock v-else-if="course.status === 'Draft'" :size="12" />
                  <AlertCircle v-else :size="12" />
                  {{ course.status }}
                </div>
              </td>
              <td class="px-8 py-5 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <button 
                    @click="editCourse(course.id)"
                    class="rounded-xl p-2.5 text-slate-400 hover:bg-indigo-50 hover:text-indigo-600 transition-all"
                    title="Chỉnh sửa"
                  >
                    <Edit2 :size="18" />
                  </button>
                  
                  <button 
                    v-if="course.status === 'Draft'"
                    @click="publishCourse(course.id)"
                    class="rounded-xl p-2.5 text-emerald-400 hover:bg-emerald-50 hover:text-emerald-600 transition-all"
                    title="Công bố"
                  >
                    <Send :size="18" />
                  </button>

                  <button 
                    class="rounded-xl p-2.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600 transition-all"
                    title="Xem chi tiết"
                  >
                    <ExternalLink :size="18" />
                  </button>

                  <button 
                    class="rounded-xl p-2.5 text-slate-400 hover:bg-rose-50 hover:text-rose-600 transition-all"
                    title="Xóa"
                  >
                    <Trash2 :size="18" />
                  </button>
                </div>
              </td>
            </tr>
            
            <!-- Empty State -->
            <tr v-if="filteredCourses.length === 0">
              <td colspan="5" class="px-8 py-20 text-center">
                <div class="flex flex-col items-center">
                  <div class="h-20 w-20 rounded-full bg-slate-50 flex items-center justify-center text-slate-300 mb-4 border border-dashed border-slate-200">
                    <Search :size="40" />
                  </div>
                  <h3 class="text-lg font-bold text-slate-700">Không tìm thấy khóa học</h3>
                  <p class="text-slate-400 text-sm mt-1">Thử thay đổi bộ lọc hoặc từ khóa tìm kiếm của bạn.</p>
                  <button @click="searchQuery = ''; filterSemester = 'Tất cả'; filterSubject = 'Tất cả'" class="mt-4 text-sm font-bold text-indigo-600">Đặt lại bộ lọc</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer Info -->
      <div class="bg-slate-50/50 px-8 py-4 border-t border-slate-50 text-[11px] font-bold text-slate-400 uppercase tracking-widest flex justify-between">
        <span>Hiển thị {{ filteredCourses.length }} / {{ courses.length }} khóa học</span>
        <span>Hệ thống LMS Academic Management</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.transition-all {
  transition-duration: 300ms;
}
</style>
