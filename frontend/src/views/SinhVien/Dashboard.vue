<script setup>
import { defineAsyncComponent } from 'vue'
import FocusAiCard from '@/components/SinhVien/dashboard/FocusAiCard.vue'
import KpiCard from '@/components/SinhVien/dashboard/KpiCard.vue'
import WelcomeHero from '@/components/SinhVien/dashboard/WelcomeHero.vue'
import { studentDashboardMock } from '@/data/studentData.mock.js'

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

const dashboard = studentDashboardMock
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-5">
    <!-- Row 1: Hero + Focus AI -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-8 flex flex-col">
        <WelcomeHero
          :student="dashboard.student"
          :summary="dashboard.focusSummary"
          :week-progress="dashboard.weekProgress"
          class="h-full"
        />
      </section>
      <section class="xl:col-span-4 flex flex-col">
        <FocusAiCard class="h-full" />
      </section>
    </div>

    <!-- Row 2: Primary Content — Course Progress + Deadlines -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-7 flex flex-col">
        <CourseProgressPanel :courses="dashboard.courses" class="h-full" />
      </section>
      <section class="xl:col-span-5 flex flex-col">
        <UpcomingAssignmentsPanel :assignments="dashboard.assignments" class="h-full" />
      </section>
    </div>

    <!-- Row 3: Secondary Content — Schedule + Grades + Notifications -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-4 flex flex-col">
        <TodaySchedulePanel :schedule="dashboard.schedule" class="h-full" />
      </section>
      <section class="xl:col-span-4 flex flex-col">
        <RecentGradesPanel :grades="dashboard.grades" class="h-full" />
      </section>
      <section class="xl:col-span-4 flex flex-col">
        <NotificationsPanel :notifications="dashboard.notifications" class="h-full" />
      </section>
    </div>

    <!-- Row 4: KPI grid (Compact cards — moved below primary content) -->
    <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
      <KpiCard v-for="item in dashboard.kpis" :key="item.id" :item="item" class="h-full" />
    </div>

    <!-- Row 5: Health & Utilities (Attendance + Tuition) -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-6 flex flex-col">
        <AttendanceHealthPanel :attendance="dashboard.attendance" class="h-full" />
      </section>
      <section class="xl:col-span-6 flex flex-col">
        <TuitionMiniPanel :tuition="dashboard.tuition" :registration="dashboard.registration" class="h-full" />
      </section>
    </div>
  </div>
</template>
