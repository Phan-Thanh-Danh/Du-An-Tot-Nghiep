<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, Users, Calendar, BookOpen, TrendingUp, FileText, Bell, Clock, ChevronRight, GraduationCap, AlertCircle } from 'lucide-vue-next'
import FormSkeleton from '@/components/common/skeleton/FormSkeleton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref('')
const classData = ref(null)
const recentActivities = ref([])

function mapClassToDetail(classDetail) {
  const courses = classDetail.courses || []
  const firstCourse = courses[0] || {}
  return {
    id: classDetail.classId ?? classDetail.id,
    name: classDetail.className ?? classDetail.name ?? '',
    subject: firstCourse.subjectCode ?? firstCourse.subject ?? '',
    semester: firstCourse.semester ?? '',
    studentsCount: firstCourse.studentCount ?? classDetail.studentCount ?? 0,
    upcomingSessions: 0,
    pendingAssignments: 0,
    averageGpa: 0,
    courses,
  }
}

async function loadClass() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getTeacherClassDetail(route.params.id)
    classData.value = data ? mapClassToDetail(data) : null
  } catch (e) {
    error.value = e?.message || 'Không thể tải thông tin lớp học.'
    classData.value = null
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadClass() })
</script>

<template>
  <div v-if="loading" class="p-4">
    <FormSkeleton :fields="8" />
  </div>
  <div v-else-if="error" class="flex flex-col items-center justify-center min-h-[300px] gap-4">
    <AlertCircle :size="40" class="text-rose-400" />
    <p class="text-rose-600 font-semibold">{{ error }}</p>
    <button @click="loadClass" class="rounded-lg bg-(--accent-primary) px-4 py-2 text-xs font-bold text-white">Thử lại</button>
  </div>
  <div v-else class="space-y-4 pb-10 max-w-7xl mx-auto animate-fade-in">
    <!-- Header -->
    <div class="surface-card border border-card rounded-2xl p-4">
      <div class="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex items-center gap-5">
          <button @click="router.push('/teacher/classes')" class="h-10 w-10 rounded-2xl surface-card border border-card flex items-center justify-center text-muted hover:text-link hover:bg-(--accent-primary)/10 transition-all">
             <ArrowLeft :size="24" stroke-width="2.5" />
          </button>
          <div>
            <div class="flex items-center gap-2 mb-1">
               <GlassBadge variant="primary">{{ classData?.id }}</GlassBadge>
               <GlassBadge variant="neutral">{{ classData?.semester }}</GlassBadge>
            </div>
            <h1 class="text-xl font-semibold text-heading tracking-tight">{{ classData?.name }}</h1>
          </div>
        </div>

        <div class="flex gap-3">
           <router-link :to="'/teacher/classes/' + classData?.id + '/workspace'" class="rounded-xl bg-(--accent-primary) px-4 py-3 text-sm font-semibold text-inverse hover:opacity-90 transition-all shadow-md">
              View Class Workspace
           </router-link>
        </div>
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
       <!-- Card 1 -->
       <div class="surface-card border border-card rounded-2xl p-4 group hover:border-link/30 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-(--accent-primary)/10 text-link flex items-center justify-center">
                <Users :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-semibold text-muted uppercase tracking-widest">Sĩ số</p>
          </div>
          <h3 class="text-xl font-semibold text-heading">{{ classData?.studentsCount }}</h3>
          <p class="text-xs font-semibold text-label mt-1">Sinh viên đăng ký</p>
       </div>
       
       <!-- Card 2 -->
       <div class="surface-card border border-card rounded-2xl p-4 group hover:border-link/30 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center">
                <Calendar :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-semibold text-muted uppercase tracking-widest">Lịch học</p>
          </div>
          <h3 class="text-xl font-semibold text-heading">{{ classData?.upcomingSessions }}</h3>
<p class="text-xs font-semibold text-label mt-1">Buổi học sắp tới</p>
        </div>
        <!-- Card 3 -->
        <div class="surface-card border border-card rounded-2xl p-4 group hover:border-link/30 transition-colors">
           <div class="flex justify-between items-center mb-4">
              <div class="h-10 w-10 rounded-2xl bg-(--color-warning-bg) text-(--color-warning-text) flex items-center justify-center">
                 <FileText :size="20" stroke-width="2.5" />
              </div>
              <p class="text-[11px] font-semibold text-muted uppercase tracking-widest">Bài tập</p>
           </div>
           <h3 class="text-xl font-semibold text-heading">{{ classData?.pendingAssignments }}</h3>
           <p class="text-xs font-semibold text-label mt-1">Bài nộp chờ chấm</p>
       </div>

       <!-- Card 4 -->
       <div class="surface-card border border-card rounded-2xl p-4 group hover:border-link/30 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center">
                <TrendingUp :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-semibold text-muted uppercase tracking-widest">GPA</p>
          </div>
          <h3 class="text-xl font-semibold text-heading">{{ classData?.averageGpa }}</h3>
          <p class="text-xs font-semibold text-label mt-1">Trung bình lớp</p>
       </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
       <!-- Left: Quick Links & Actions -->
       <div class="lg:col-span-2 space-y-4">
          <GlassPanel variant="soft" density="normal">
             <template #header>
               <h2 class="text-lg font-semibold text-heading flex items-center gap-2">
                  <BookOpen :size="20" class="text-link" /> Phân hệ quản lý lớp
               </h2>
             </template>
             <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <router-link to="/teacher/class-progress" class="group flex items-center justify-between p-5 rounded-2xl border border-card surface-card hover:surface-elevated hover:border-link/30 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-10 w-10 rounded-2xl bg-(--accent-primary)/10 text-link flex items-center justify-center group-hover:scale-110 transition-transform">
                         <GraduationCap :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-semibold text-heading">Tiến độ học tập</h4>
                         <p class="text-[11px] font-semibold text-muted">Theo dõi tiến độ sinh viên</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-muted group-hover:text-link group-hover:translate-x-1 transition-all" />
                </router-link>
                
                <router-link to="/teacher/class-attendance" class="group flex items-center justify-between p-5 rounded-2xl border border-card surface-card hover:surface-elevated hover:border-link/30 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center group-hover:scale-110 transition-transform">
                         <Clock :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-semibold text-heading">Điểm danh</h4>
                         <p class="text-[11px] font-semibold text-muted">Quản lý chuyên cần</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-muted group-hover:text-link group-hover:translate-x-1 transition-all" />
                </router-link>

                <router-link to="/teacher/class-grades" class="group flex items-center justify-between p-5 rounded-2xl border border-card surface-card hover:surface-elevated hover:border-link/30 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-10 w-10 rounded-2xl bg-(--accent-violet-soft) text-(--accent-violet) flex items-center justify-center group-hover:scale-110 transition-transform">
                         <TrendingUp :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-semibold text-heading">Bảng điểm</h4>
                         <p class="text-[11px] font-semibold text-muted">Nhập và xuất điểm</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-muted group-hover:text-link group-hover:translate-x-1 transition-all" />
                </router-link>
             </div>
          </GlassPanel>
       </div>

       <!-- Right: Recent Activities -->
       <div class="lg:col-span-1">
          <GlassPanel variant="soft" density="normal" class="h-full">
             <template #header>
               <div class="flex justify-between items-center">
                  <h2 class="text-lg font-semibold text-heading flex items-center gap-2">
                     <Bell :size="20" class="text-(--color-warning-text)" /> Hoạt động mới
                  </h2>
                  <button class="text-[10px] font-semibold text-link uppercase tracking-widest hover:underline">Xem tất cả</button>
               </div>
             </template>
             
             <div class="space-y-4">
                <div v-for="act in recentActivities" :key="act.id" class="relative flex items-center gap-3">
                   <div class="flex items-center justify-center w-10 h-10 rounded-full border-2 border-card surface-card text-muted shrink-0 shadow-sm z-10">
                      <FileText v-if="act.type === 'assignment'" :size="14" class="text-link" />
                      <Clock v-if="act.type === 'leave'" :size="14" class="text-(--color-warning-text)" />
                      <BookOpen v-if="act.type === 'material'" :size="14" class="text-(--color-success-text)" />
                   </div>
                   <div class="flex-1 surface-card p-4 rounded-2xl border border-card shadow-sm">
                      <div class="flex items-center justify-between mb-1">
                         <span class="text-[10px] font-semibold text-muted uppercase tracking-wider">{{ act.time }}</span>
                      </div>
                      <h4 class="text-sm font-semibold text-heading">{{ act.title }}</h4>
                   </div>
                </div>
             </div>
          </GlassPanel>
       </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.4s ease-out forwards;
}
</style>
