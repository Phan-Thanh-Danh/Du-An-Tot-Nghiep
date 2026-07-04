<script setup>
import { ref, onMounted } from 'vue'
import {
  PieChart,
  BarChart,
  Users,
  AlertTriangle,
  CheckCircle2,
  BookOpen,
  Clock,
  TrendingUp,
} from 'lucide-vue-next'
import reportsApi from '@/services/reportsApi'

const courseReport = ref(null)
const teacherLoadReport = ref(null)
const attendanceReport = ref(null)
const isLoading = ref(true)

onMounted(async () => {
  try {
    const [resCourses, resTeachers, resAttendance] = await Promise.all([
      reportsApi.getCourseReport(),
      reportsApi.getTeacherLoadReport(),
      reportsApi.getAttendanceReport()
    ])
    
    courseReport.value = resCourses.data
    teacherLoadReport.value = resTeachers.data
    attendanceReport.value = resAttendance.data
  } catch (error) {
    console.error('Lỗi khi tải báo cáo:', error)
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h2 class="text-xl font-bold text-heading">Báo cáo & Thống kê Học vụ</h2>
        <p class="text-sm text-muted mt-1">Dữ liệu tổng quan về khóa học, giảng viên và chuyên cần của sinh viên.</p>
      </div>
      <div class="flex gap-2">
        <button class="flex items-center gap-2 rounded-(--radius-md) border border-card bg-(--surface-card) px-3 py-1.5 text-sm font-medium text-label hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors">
          <TrendingUp :size="16" />
          Xuất báo cáo
        </button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="isLoading" class="flex justify-center items-center h-64">
      <div class="w-8 h-8 border-4 border-(--sidebar-accent) border-t-transparent rounded-full animate-spin"></div>
    </div>

    <!-- Content -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      
      <!-- BÁO CÁO KHÓA HỌC -->
      <div class="surface-card rounded-(--radius-lg) border border-card p-5 lg-glass-soft shadow-sm">
        <div class="flex items-center gap-3 mb-5">
          <div class="p-2 bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400 rounded-lg">
            <BookOpen :size="20" />
          </div>
          <div>
            <h3 class="font-bold text-heading text-lg">Báo cáo Khóa học</h3>
            <p class="text-xs text-muted">Tổng số: {{ courseReport?.totalCourses }} khóa học</p>
          </div>
        </div>
        
        <div class="space-y-4">
          <h4 class="text-sm font-semibold text-label border-b border-card pb-2">Phân bố theo trạng thái</h4>
          <div class="space-y-3">
            <div v-for="(item, idx) in courseReport?.statusDistribution" :key="idx" class="flex items-center justify-between">
              <span class="text-sm text-body flex items-center gap-2">
                <span class="w-2 h-2 rounded-full" :class="item.label === 'Đang diễn ra' ? 'bg-emerald-500' : 'bg-slate-400'"></span>
                {{ item.label }}
              </span>
              <span class="text-sm font-medium text-heading">{{ item.value }}</span>
            </div>
          </div>
          
          <h4 class="text-sm font-semibold text-label border-b border-card pb-2 mt-6">Khóa học theo học kỳ</h4>
          <div class="space-y-3">
            <div v-for="(item, idx) in courseReport?.semesterDistribution" :key="idx" class="flex items-center gap-4">
              <span class="text-sm text-body w-20 truncate">{{ item.label }}</span>
              <div class="flex-1 h-2 bg-slate-100 dark:bg-slate-800 rounded-full overflow-hidden">
                <div class="h-full bg-indigo-500 rounded-full" :style="{ width: (item.value / courseReport?.totalCourses * 100) + '%' }"></div>
              </div>
              <span class="text-sm font-medium text-heading w-8 text-right">{{ item.value }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <!-- CHUYÊN CẦN -->
      <div class="surface-card rounded-(--radius-lg) border border-card p-5 lg-glass-soft shadow-sm">
        <div class="flex items-center gap-3 mb-5">
          <div class="p-2 bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400 rounded-lg">
            <CheckCircle2 :size="20" />
          </div>
          <div>
            <h3 class="font-bold text-heading text-lg">Tỷ lệ Chuyên cần</h3>
            <p class="text-xs text-muted">Trung bình: {{ attendanceReport?.averageAttendanceRate }}%</p>
          </div>
        </div>
        
        <div class="space-y-4 max-h-[300px] overflow-y-auto pr-2 scrollbar-thin">
          <div v-for="(item, idx) in attendanceReport?.classAttendance" :key="idx" 
               class="p-3 rounded-lg border border-card bg-slate-50/50 dark:bg-slate-800/30 flex items-center justify-between">
            <div class="flex-1 min-w-0 pr-4">
              <p class="text-sm font-medium text-heading truncate">{{ item.courseName }}</p>
              <div class="flex items-center gap-2 mt-1.5">
                <div class="flex-1 h-1.5 bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
                  <div class="h-full rounded-full" 
                       :class="item.attendanceRate >= 80 ? 'bg-emerald-500' : (item.attendanceRate >= 50 ? 'bg-amber-500' : 'bg-red-500')"
                       :style="{ width: item.attendanceRate + '%' }"></div>
                </div>
                <span class="text-xs font-semibold" 
                      :class="item.attendanceRate >= 80 ? 'text-emerald-600 dark:text-emerald-400' : 'text-amber-600 dark:text-amber-400'">
                  {{ item.attendanceRate }}%
                </span>
              </div>
            </div>
            
            <div v-if="item.warningCount > 0" class="flex flex-col items-center flex-shrink-0 px-2 border-l border-card">
              <span class="flex items-center gap-1 text-xs font-medium text-red-600 dark:text-red-400">
                <AlertTriangle :size="12" />
                {{ item.warningCount }} vắng
              </span>
            </div>
          </div>
        </div>
      </div>
      
      <!-- TẢI GIẢNG VIÊN -->
      <div class="surface-card rounded-(--radius-lg) border border-card p-5 lg-glass-soft shadow-sm lg:col-span-2">
        <div class="flex items-center gap-3 mb-5">
          <div class="p-2 bg-amber-100 text-amber-600 dark:bg-amber-900/30 dark:text-amber-400 rounded-lg">
            <Users :size="20" />
          </div>
          <div>
            <h3 class="font-bold text-heading text-lg">Tải Giảng Viên</h3>
            <p class="text-xs text-muted">Tổng số GV đang phụ trách: {{ teacherLoadReport?.totalTeachers }}</p>
          </div>
        </div>
        
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm text-body">
            <thead class="text-xs uppercase bg-slate-50 dark:bg-slate-800/50 text-label border-b border-card">
              <tr>
                <th scope="col" class="px-4 py-3 font-semibold rounded-tl-lg">Giảng viên</th>
                <th scope="col" class="px-4 py-3 font-semibold">Khóa học phụ trách</th>
                <th scope="col" class="px-4 py-3 font-semibold">Số buổi dạy</th>
                <th scope="col" class="px-4 py-3 font-semibold rounded-tr-lg">Tổng giờ quy đổi (ước tính)</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-card">
              <tr v-for="t in teacherLoadReport?.teacherLoads.slice(0, 10)" :key="t.teacherId" 
                  class="hover:bg-slate-50/50 dark:hover:bg-slate-800/30 transition-colors">
                <td class="px-4 py-3 font-medium text-heading">{{ t.teacherName }}</td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center justify-center bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400 text-xs font-medium px-2 py-0.5 rounded-full">
                    {{ t.totalCourses }} lớp
                  </span>
                </td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center justify-center bg-slate-100 text-slate-700 dark:bg-slate-700 dark:text-slate-300 text-xs font-medium px-2 py-0.5 rounded-full">
                    {{ t.totalSessions }} buổi
                  </span>
                </td>
                <td class="px-4 py-3 font-semibold text-emerald-600 dark:text-emerald-400">
                  {{ t.totalHours }} giờ
                </td>
              </tr>
              <tr v-if="teacherLoadReport?.teacherLoads.length === 0">
                <td colspan="4" class="px-4 py-8 text-center text-muted">Không có dữ liệu giảng viên</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  </div>
</template>
