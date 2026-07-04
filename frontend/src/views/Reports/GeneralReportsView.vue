<script setup>
import { ref, onMounted } from 'vue'
import { apiRequest } from '@/services/apiClient'
import PageContainer from '@/components/ui/PageContainer.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { BarChart3, Users, BookOpen, GraduationCap, Building } from 'lucide-vue-next'

const loading = ref(true)
const reportData = ref({
  courses: null,
  attendance: null,
  assignments: null,
  grades: null,
  registrations: null,
  teacherWorkload: null,
  rooms: null,
})

const fetchReports = async () => {
  try {
    loading.value = true
    const [courses, attendance, assignments, grades, registrations, teacherWorkload, rooms] = await Promise.all([
      apiRequest('/api/reports/courses', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/attendance', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/assignments', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/grades', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/registrations', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/teacher-workload', { method: 'GET' }).catch(() => ({ data: null })),
      apiRequest('/api/reports/rooms', { method: 'GET' }).catch(() => ({ data: null })),
    ])

    reportData.value = {
      courses: courses?.data || courses?.Data,
      attendance: attendance?.data || attendance?.Data,
      assignments: assignments?.data || assignments?.Data,
      grades: grades?.data || grades?.Data,
      registrations: registrations?.data || registrations?.Data,
      teacherWorkload: teacherWorkload?.data || teacherWorkload?.Data,
      rooms: rooms?.data || rooms?.Data,
    }
  } catch (error) {
    console.error('Failed to load reports:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchReports()
})
</script>

<template>
  <PageContainer title="Báo cáo tổng hợp" description="Số liệu thống kê hoạt động đào tạo từ hệ thống">
    <div v-if="loading" class="flex justify-center p-10">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
    </div>
    
    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      
      <!-- Khóa học -->
      <GlassPanel class="p-6 flex flex-col gap-4">
        <div class="flex items-center gap-3">
          <BookOpen class="w-6 h-6 text-blue-600" />
          <h3 class="text-lg font-semibold text-heading">Khóa học</h3>
        </div>
        <div v-if="reportData.courses" class="space-y-3 text-body">
          <div class="flex justify-between border-b border-border-default pb-2">
            <span>Tổng số khóa học</span>
            <strong class="text-blue-600">{{ reportData.courses.totalCourses || reportData.courses.TotalCourses }}</strong>
          </div>
          <div v-for="stat in (reportData.courses.byStatus || reportData.courses.ByStatus || [])" :key="stat.status || stat.Status" class="flex justify-between">
            <span class="capitalize">{{ (stat.status || stat.Status || 'Không xác định').replace('_', ' ') }}</span>
            <GlassBadge variant="primary">{{ stat.count || stat.Count }}</GlassBadge>
          </div>
        </div>
      </GlassPanel>

      <!-- Điểm danh -->
      <GlassPanel class="p-6 flex flex-col gap-4">
        <div class="flex items-center gap-3">
          <Users class="w-6 h-6 text-teal-600" />
          <h3 class="text-lg font-semibold text-heading">Điểm danh</h3>
        </div>
        <div v-if="reportData.attendance" class="space-y-3 text-body">
          <div v-for="stat in (reportData.attendance.stats || reportData.attendance.Stats || [])" :key="stat.status || stat.Status" class="flex justify-between">
            <span class="capitalize">{{ (stat.status || stat.Status || 'Không xác định').replace('_', ' ') }}</span>
            <GlassBadge variant="success">{{ stat.count || stat.Count }}</GlassBadge>
          </div>
        </div>
      </GlassPanel>

      <!-- Bài tập -->
      <GlassPanel class="p-6 flex flex-col gap-4">
        <div class="flex items-center gap-3">
          <BarChart3 class="w-6 h-6 text-amber-600" />
          <h3 class="text-lg font-semibold text-heading">Bài tập & Nộp bài</h3>
        </div>
        <div v-if="reportData.assignments" class="space-y-3 text-body">
          <div class="flex justify-between">
            <span>Tổng bài nộp</span>
            <strong>{{ reportData.assignments.totalSubmissions || reportData.assignments.TotalSubmissions }}</strong>
          </div>
          <div class="flex justify-between">
            <span>Đã chấm điểm</span>
            <GlassBadge variant="success">{{ reportData.assignments.gradedSubmissions || reportData.assignments.GradedSubmissions }}</GlassBadge>
          </div>
          <div class="flex justify-between">
            <span>Chờ chấm điểm</span>
            <GlassBadge variant="warning">{{ reportData.assignments.ungradedSubmissions || reportData.assignments.UngradedSubmissions }}</GlassBadge>
          </div>
        </div>
      </GlassPanel>

      <!-- Điểm số -->
      <GlassPanel class="p-6 flex flex-col gap-4">
        <div class="flex items-center gap-3">
          <GraduationCap class="w-6 h-6 text-violet-600" />
          <h3 class="text-lg font-semibold text-heading">Điểm số</h3>
        </div>
        <div v-if="reportData.grades" class="space-y-3 text-body">
          <div class="flex justify-between">
            <span>Đạt (Passed)</span>
            <GlassBadge variant="success">{{ reportData.grades.passedCount || reportData.grades.PassedCount }}</GlassBadge>
          </div>
          <div class="flex justify-between">
            <span>Không đạt (Failed)</span>
            <GlassBadge variant="danger">{{ reportData.grades.failedCount || reportData.grades.FailedCount }}</GlassBadge>
          </div>
        </div>
      </GlassPanel>

      <!-- Giảng viên -->
      <GlassPanel class="p-6 flex flex-col gap-4 col-span-1 md:col-span-2 lg:col-span-1">
        <div class="flex items-center gap-3">
          <Users class="w-6 h-6 text-indigo-600" />
          <h3 class="text-lg font-semibold text-heading">Tải giảng viên (Top)</h3>
        </div>
        <div v-if="reportData.teacherWorkload" class="text-body max-h-48 overflow-y-auto pr-2 custom-scrollbar">
          <TableShell>
            <thead>
              <tr>
                <th class="text-left text-label">Giảng viên</th>
                <th class="text-right text-label">Khóa học</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="teacher in (reportData.teacherWorkload.topTeachers || reportData.teacherWorkload.TopTeachers || [])" :key="teacher.teacherId || teacher.TeacherId">
                <td class="py-2">{{ teacher.name || teacher.Name }}</td>
                <td class="py-2 text-right font-medium">{{ teacher.courseCount || teacher.CourseCount }}</td>
              </tr>
            </tbody>
          </TableShell>
        </div>
      </GlassPanel>

      <!-- Phòng học -->
      <GlassPanel class="p-6 flex flex-col gap-4">
        <div class="flex items-center gap-3">
          <Building class="w-6 h-6 text-slate-600" />
          <h3 class="text-lg font-semibold text-heading">Phòng học</h3>
        </div>
        <div v-if="reportData.rooms" class="space-y-3 text-body">
          <div class="flex justify-between border-b border-border-default pb-2">
            <span>Tổng số phòng</span>
            <strong>{{ reportData.rooms.totalRooms || reportData.rooms.TotalRooms }}</strong>
          </div>
          <div v-for="stat in (reportData.rooms.byType || reportData.rooms.ByType || [])" :key="stat.type || stat.Type" class="flex justify-between">
            <span class="capitalize">{{ (stat.type || stat.Type || 'Không xác định') }}</span>
            <GlassBadge variant="neutral">{{ stat.count || stat.Count }}</GlassBadge>
          </div>
        </div>
      </GlassPanel>

    </div>
  </PageContainer>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: var(--border-default);
  border-radius: 4px;
}
</style>
