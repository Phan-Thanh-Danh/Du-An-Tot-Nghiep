<script setup>
import AttendanceHealthPanel from '@/components/SinhVien/dashboard/AttendanceHealthPanel.vue'
import CourseProgressPanel from '@/components/SinhVien/dashboard/CourseProgressPanel.vue'
import FocusAiCard from '@/components/SinhVien/dashboard/FocusAiCard.vue'
import KpiCard from '@/components/SinhVien/dashboard/KpiCard.vue'
import NotificationsPanel from '@/components/SinhVien/dashboard/NotificationsPanel.vue'
import RecentGradesPanel from '@/components/SinhVien/dashboard/RecentGradesPanel.vue'
import TodaySchedulePanel from '@/components/SinhVien/dashboard/TodaySchedulePanel.vue'
import TuitionMiniPanel from '@/components/SinhVien/dashboard/TuitionMiniPanel.vue'
import UpcomingAssignmentsPanel from '@/components/SinhVien/dashboard/UpcomingAssignmentsPanel.vue'
import WelcomeHero from '@/components/SinhVien/dashboard/WelcomeHero.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { studentDashboardMock } from '@/data/studentDashboard.mock'

defineOptions({
  name: 'StudentDashboard',
})

const dashboard = studentDashboardMock
</script>

<template>
  <div class="space-y-4 pb-6">
    <!-- Row 1: Hero Today Summary + Today Focus AI -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-7">
        <WelcomeHero
          :student="dashboard.student"
          :summary="dashboard.focusSummary"
          :week-progress="dashboard.weekProgress"
        />
      </section>
      <section class="xl:col-span-5">
        <FocusAiCard />
      </section>
    </div>

    <!-- Row 2: KPI grid (Compact cards) -->
    <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <KpiCard v-for="item in dashboard.kpis" :key="item.id" :item="item" />
    </div>

    <!-- Row 3: Primary Content (Courses + Assignments) -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-7">
        <CourseProgressPanel :courses="dashboard.courses" />
      </section>
      <section class="xl:col-span-5">
        <UpcomingAssignmentsPanel :assignments="dashboard.assignments" />
      </section>
    </div>

    <!-- Row 4: Secondary Content (Schedule + Grades + Notifications) -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-4">
        <TodaySchedulePanel :schedule="dashboard.schedule" />
      </section>
      <section class="xl:col-span-4">
        <RecentGradesPanel :grades="dashboard.grades" />
      </section>
      <section class="xl:col-span-4">
        <NotificationsPanel :notifications="dashboard.notifications" />
      </section>
    </div>

    <!-- Row 5: Health & Utilities (Attendance + Tuition) -->
    <div class="grid gap-4 xl:grid-cols-12">
      <section class="xl:col-span-6">
        <AttendanceHealthPanel :attendance="dashboard.attendance" />
      </section>
      <section class="xl:col-span-6">
        <TuitionMiniPanel :tuition="dashboard.tuition" :registration="dashboard.registration" />
      </section>
    </div>
  </div>
</template>
