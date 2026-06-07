<script setup>
import { ref } from 'vue'
import { 
  Search, Filter, Users, BookOpen, Calendar, ChevronRight, 
  MoreHorizontal, Eye, Download, GraduationCap
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const classes = ref([
  { id: 'L01', code: 'SE1601', name: 'Lớp SE1601 - Java', subject: 'Lập trình Java', students: 45, semester: 'Spring 2026' },
  { id: 'L02', code: 'SS1402', name: 'Lớp SS1402 - Web', subject: 'Lập trình Web', students: 38, semester: 'Spring 2026' },
  { id: 'L03', code: 'SA1709', name: 'Lớp SA1709 - DB', subject: 'Cơ sở dữ liệu', students: 42, semester: 'Fall 2025' },
])

const filterSemester = ref('Spring 2026')
</script>

<template>
  <div class="space-y-4 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-xl font-bold text-heading tracking-tight">Danh sách lớp học</h1>
        <p class="text-muted mt-1">Quản lý và theo dõi các lớp học bạn đang phụ trách giảng dạy.</p>
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
        <input type="text" placeholder="Tìm theo mã lớp, tên lớp..." class="lg-control w-full pl-11 pr-4" />
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <select v-model="filterSemester" class="lg-control flex-1 md:w-48">
          <option value="Spring 2026">Spring 2026</option>
          <option value="Fall 2025">Fall 2025</option>
        </select>
        <button class="lg-icon-button h-10 w-10 rounded-xl border border-card surface-card text-muted hover:text-heading hover:bg-[var(--accent-primary)]/10 transition-all">
          <Filter :size="18" />
        </button>
      </div>
    </div>

    <!-- Grid / Table -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div v-for="cls in classes" :key="cls.id" class="lg-glass-soft lg-card-hover rounded-2xl p-4 flex flex-col">
        <div class="flex justify-between items-start mb-4">
          <div class="h-10 w-10 rounded-2xl bg-[var(--accent-primary)]/10 flex items-center justify-center text-link border border-[var(--accent-primary)]/20">
            <GraduationCap :size="24" />
          </div>
          <button class="p-1.5 text-muted hover:text-heading"><MoreHorizontal :size="20" /></button>
        </div>
        
        <div class="flex-1">
          <h3 class="text-xl font-bold text-heading">{{ cls.code }}</h3>
          <p class="text-sm font-semibold text-label mt-1">{{ cls.name }}</p>
          
          <div class="mt-6 space-y-3">
            <div class="flex items-center gap-3 text-sm text-body">
              <BookOpen :size="16" class="text-muted" />
              <span>Môn: <span class="font-bold">{{ cls.subject }}</span></span>
            </div>
            <div class="flex items-center gap-3 text-sm text-body">
              <Users :size="16" class="text-muted" />
              <span>Sĩ số: <span class="font-bold">{{ cls.students }} sinh viên</span></span>
            </div>
            <div class="flex items-center gap-3 text-sm text-body">
              <Calendar :size="16" class="text-muted" />
              <span>Học kỳ: <GlassBadge variant="primary">{{ cls.semester }}</GlassBadge></span>
            </div>
          </div>
        </div>

        <div class="mt-8 pt-4 border-t border-card flex items-center justify-between">
           <router-link :to="'/teacher/classes/' + cls.code + '/details'" class="text-xs font-bold text-link hover:underline flex items-center gap-1">
             Xem chi tiết <ChevronRight :size="14" />
           </router-link>
           <router-link :to="'/teacher/classes/' + cls.code + '/workspace'" class="rounded-xl bg-[var(--accent-primary)] px-4 py-2 text-xs font-bold text-inverse hover:opacity-90 transition-all flex items-center gap-2">
             <Eye :size="14" /> View class
           </router-link>
        </div>
      </div>
    </div>
  </div>
</template>
