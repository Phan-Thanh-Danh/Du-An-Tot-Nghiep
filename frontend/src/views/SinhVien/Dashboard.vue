<script setup>
import { ref, onMounted, defineAsyncComponent } from 'vue'
import FocusAiCard from '@/components/SinhVien/dashboard/FocusAiCard.vue'
import KpiCard from '@/components/SinhVien/dashboard/KpiCard.vue'
import WelcomeHero from '@/components/SinhVien/dashboard/WelcomeHero.vue'
import { studentDashboardMock } from '@/data/studentData.mock.js'
import { studentApi } from '@/services/studentApi'
import { AlertCircle, Loader2 } from 'lucide-vue-next'

// Lazy load below-the-fold components to prioritize initial render and prevent jank
const CourseProgressPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/CourseProgressPanel.vue'))
const UpcomingAssignmentsPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/UpcomingAssignmentsPanel.vue'))
const TodaySchedulePanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/TodaySchedulePanel.vue'))
const RecentGradesPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/RecentGradesPanel.vue'))
const NotificationsPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/NotificationsPanel.vue'))
const AttendanceHealthPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/AttendanceHealthPanel.vue'))
const TuitionMiniPanel = defineAsyncComponent(() => import('@/components/SinhVien/dashboard/TuitionMiniPanel.vue'))

defineOptions({
  name: 'StudentDashboard',
})

const dashboard = ref(studentDashboardMock)
const loading = ref(true)
const error = ref(null)

onMounted(async () => {
  try {
    loading.value = true
    const res = await studentApi.getDashboard()
    console.log('Dashboard API Response:', res)
    const isSuccess = res.success === true || res.Success === true
    
    if (isSuccess) {
      dashboard.value = res.data || res.Data
    } else {
      error.value = res.message || res.Message || 'Lỗi khi tải dữ liệu dashboard'
    }
  } catch (err) {
    console.error('Failed to load student dashboard', err)
    error.value = 'Lỗi kết nối đến máy chủ.'
    // Fallback disabled to test live data
    // dashboard.value = studentDashboardMock
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-5">
    <div v-if="loading" class="flex flex-col items-center justify-center py-20">
      <Loader2 class="h-10 w-10 animate-spin text-blue-500 mb-4" />
      <p class="text-slate-500">Đang tải dữ liệu tổng quan...</p>
    </div>

    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle class="h-12 w-12 text-red-500 mb-4" />
      <h3 class="text-lg font-semibold text-slate-800">Không thể tải dữ liệu</h3>
      <p class="text-slate-500 max-w-md">{{ error }}</p>
    </div>

    <template v-else>
      <!-- Row 1: Hero Chào mừng và Chuyên cần & Cảnh báo (tỉ lệ 10-6) -->
      <div class="grid gap-4 xl:grid-cols-[repeat(16,minmax(0,1fr))]">
        <section class="xl:col-span-10 flex flex-col">
          <WelcomeHero
            :student="dashboard.student"
            :summary="dashboard.focusSummary"
            :week-progress="dashboard.weekProgress"
            class="h-full"
          />
        </section>
        <section class="xl:col-span-6 flex flex-col">
          <AttendanceHealthPanel :attendance="dashboard.attendance" class="h-full" />
        </section>
      </div>

      <!-- Row 2: Thống kê tổng hợp (KPI Cards - 5 sao) -->
      <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KpiCard v-for="item in dashboard.kpis" :key="item.id" :item="item" class="h-full" />
      </div>

      <!-- Row 3: Các hoạt động khẩn cấp (Lịch học hôm nay, Bài tập sắp hạn, Thông báo mới - 5 sao) -->
      <div class="grid gap-4 xl:grid-cols-12">
        <section class="xl:col-span-4 flex flex-col">
          <TodaySchedulePanel :schedule="dashboard.schedule" class="h-full" />
        </section>
        <section class="xl:col-span-4 flex flex-col">
          <UpcomingAssignmentsPanel :assignments="dashboard.assignments" class="h-full" />
        </section>
        <section class="xl:col-span-4 flex flex-col">
          <NotificationsPanel :notifications="dashboard.notifications" class="h-full" />
        </section>
      </div>

      <!-- Row 4: Tiến trình học tập & Điểm số (Tiến độ khóa học, Điểm gần đây - 4 sao) -->
      <div class="grid gap-4 xl:grid-cols-12">
        <section class="xl:col-span-8 flex flex-col">
          <CourseProgressPanel :courses="dashboard.courses" class="h-full" />
        </section>
        <section class="xl:col-span-4 flex flex-col">
          <RecentGradesPanel :grades="dashboard.grades" class="h-full" />
        </section>
      </div>

      <!-- Row 5: Tiện ích & Hỗ trợ (Học phí, AI Assistant - 2 sao) -->
      <div class="grid gap-4 xl:grid-cols-12">
        <section class="xl:col-span-6 flex flex-col">
          <TuitionMiniPanel :tuition="dashboard.tuition" :registration="dashboard.registration" class="h-full" />
        </section>
        <section class="xl:col-span-6 flex flex-col">
          <FocusAiCard class="h-full" />
        </section>
      </div>
    </template>
  </div>
</template>
