<script setup>
import { ref } from 'vue'
import { 
  Search, Filter, Users, BookOpen, Calendar, ChevronRight, 
  MoreHorizontal, Eye, Download, GraduationCap
} from 'lucide-vue-next'

const classes = ref([
  { id: 'L01', code: 'SE1601', name: 'Lớp SE1601 - Java', subject: 'Lập trình Java', students: 45, semester: 'Spring 2026' },
  { id: 'L02', code: 'SS1402', name: 'Lớp SS1402 - Web', subject: 'Lập trình Web', students: 38, semester: 'Spring 2026' },
  { id: 'L03', code: 'SA1709', name: 'Lớp SA1709 - DB', subject: 'Cơ sở dữ liệu', students: 42, semester: 'Fall 2025' },
])

const filterSemester = ref('Spring 2026')
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Danh sách lớp học</h1>
        <p class="text-slate-500 mt-1">Quản lý và theo dõi các lớp học bạn đang phụ trách giảng dạy.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất báo cáo
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4 items-center">
      <div class="relative flex-1 w-full">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm theo mã lớp, tên lớp..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <select v-model="filterSemester" class="flex-1 md:w-48 rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none">
          <option value="Spring 2026">Spring 2026</option>
          <option value="Fall 2025">Fall 2025</option>
        </select>
        <button class="rounded-xl border border-slate-200 p-2.5 text-slate-400 hover:bg-slate-50 transition-colors">
          <Filter :size="18" />
        </button>
      </div>
    </div>

    <!-- Grid / Table -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="cls in classes" :key="cls.id" class="lg-card-glass lg-card-hover group border-slate-100 p-6 flex flex-col">
        <div class="flex justify-between items-start mb-4">
          <div class="h-12 w-12 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100/50">
            <GraduationCap :size="24" />
          </div>
          <button class="p-1.5 text-slate-300 hover:text-slate-600"><MoreHorizontal :size="20" /></button>
        </div>
        
        <div class="flex-1">
          <h3 class="text-xl font-bold text-slate-800">{{ cls.code }}</h3>
          <p class="text-sm font-semibold text-slate-500 mt-1">{{ cls.name }}</p>
          
          <div class="mt-6 space-y-3">
            <div class="flex items-center gap-3 text-sm text-slate-600">
              <BookOpen :size="16" class="text-slate-400" />
              <span>Môn: <span class="font-bold">{{ cls.subject }}</span></span>
            </div>
            <div class="flex items-center gap-3 text-sm text-slate-600">
              <Users :size="16" class="text-slate-400" />
              <span>Sĩ số: <span class="font-bold">{{ cls.students }} sinh viên</span></span>
            </div>
            <div class="flex items-center gap-3 text-sm text-slate-600">
              <Calendar :size="16" class="text-slate-400" />
              <span>Học kỳ: <span class="font-bold text-indigo-600">{{ cls.semester }}</span></span>
            </div>
          </div>
        </div>

        <div class="mt-8 pt-4 border-t border-slate-50 flex items-center justify-between">
           <router-link to="/teacher/class-progress" class="text-xs font-bold text-indigo-600 hover:underline flex items-center gap-1">
             Xem chi tiết <ChevronRight :size="14" />
           </router-link>
           <button class="rounded-xl bg-slate-900 px-4 py-2 text-xs font-bold text-white hover:bg-slate-800 transition-all flex items-center gap-2">
             <Eye :size="14" /> View class
           </button>
        </div>
      </div>
    </div>
  </div>
</template>
