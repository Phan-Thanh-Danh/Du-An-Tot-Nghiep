<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  ArrowLeft, Users, Calendar, BookOpen, 
  TrendingUp, FileText, Bell, Clock, ChevronRight,
  GraduationCap
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()

// Mock data
const classData = ref({
  id: route.params.id || 'SE1601',
  name: 'Lập trình Java Cơ bản',
  subject: 'Công nghệ thông tin',
  semester: 'Spring 2026',
  studentsCount: 45,
  upcomingSessions: 3,
  pendingAssignments: 12,
  averageGpa: 7.8
})

const recentActivities = ref([
  { id: 1, title: 'Đã nộp bài tập Lab 3', time: '2 giờ trước', type: 'assignment' },
  { id: 2, title: 'Sinh viên Nguyễn Văn A xin nghỉ', time: '5 giờ trước', type: 'leave' },
  { id: 3, title: 'Cập nhật tài liệu Chương 4', time: '1 ngày trước', type: 'material' },
])
</script>

<template>
  <div class="space-y-6 pb-10 max-w-7xl mx-auto animate-fade-in">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-6 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <div class="absolute -right-20 -bottom-20 w-64 h-64 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-50 to-transparent rounded-full pointer-events-none"></div>
      
      <div class="relative z-10 flex items-center gap-5">
        <button @click="router.push('/teacher/classes')" class="h-12 w-12 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-500 hover:text-blue-600 hover:bg-blue-50 border border-slate-100 transition-all">
           <ArrowLeft :size="24" stroke-width="2.5" />
        </button>
        <div>
          <div class="flex items-center gap-2 mb-1">
             <span class="px-2.5 py-1 rounded-lg bg-blue-100 text-blue-700 text-[10px] font-black uppercase tracking-wider">{{ classData.id }}</span>
             <span class="px-2.5 py-1 rounded-lg bg-slate-100 text-slate-600 text-[10px] font-black uppercase tracking-wider">{{ classData.semester }}</span>
          </div>
          <h1 class="text-2xl font-black text-slate-800 tracking-tight">{{ classData.name }}</h1>
        </div>
      </div>
      
      <div class="relative z-10 flex gap-3">
         <router-link :to="`/teacher/classes/${classData.id}/workspace`" class="rounded-[20px] bg-slate-900 px-6 py-3 text-sm font-bold text-white hover:bg-slate-800 transition-all shadow-md">
            View Class Workspace
         </router-link>
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
       <!-- Card 1 -->
       <div class="bg-white p-6 rounded-[28px] border border-slate-100 shadow-sm relative overflow-hidden group hover:border-blue-200 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center">
                <Users :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">Sĩ số</p>
          </div>
          <h3 class="text-3xl font-black text-slate-800">{{ classData.studentsCount }}</h3>
          <p class="text-xs font-bold text-slate-500 mt-1">Sinh viên đăng ký</p>
       </div>
       
       <!-- Card 2 -->
       <div class="bg-white p-6 rounded-[28px] border border-slate-100 shadow-sm relative overflow-hidden group hover:border-blue-200 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-cyan-50 text-cyan-600 flex items-center justify-center">
                <Calendar :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">Lịch học</p>
          </div>
          <h3 class="text-3xl font-black text-slate-800">{{ classData.upcomingSessions }}</h3>
          <p class="text-xs font-bold text-slate-500 mt-1">Buổi học sắp tới</p>
       </div>

       <!-- Card 3 -->
       <div class="bg-white p-6 rounded-[28px] border border-slate-100 shadow-sm relative overflow-hidden group hover:border-blue-200 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center">
                <FileText :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">Bài tập</p>
          </div>
          <h3 class="text-3xl font-black text-slate-800">{{ classData.pendingAssignments }}</h3>
          <p class="text-xs font-bold text-slate-500 mt-1">Bài nộp chờ chấm</p>
       </div>

       <!-- Card 4 -->
       <div class="bg-white p-6 rounded-[28px] border border-slate-100 shadow-sm relative overflow-hidden group hover:border-blue-200 transition-colors">
          <div class="flex justify-between items-center mb-4">
             <div class="h-10 w-10 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
                <TrendingUp :size="20" stroke-width="2.5" />
             </div>
             <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">GPA</p>
          </div>
          <h3 class="text-3xl font-black text-slate-800">{{ classData.averageGpa }}</h3>
          <p class="text-xs font-bold text-slate-500 mt-1">Trung bình lớp</p>
       </div>
    </div>

    <!-- Main Content Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
       <!-- Left: Quick Links & Actions -->
       <div class="lg:col-span-2 space-y-6">
          <div class="bg-white rounded-[32px] border border-slate-100 p-8 shadow-sm">
             <h2 class="text-lg font-black text-slate-800 mb-6 flex items-center gap-2">
                <BookOpen :size="20" class="text-blue-500" /> Phân hệ quản lý lớp
             </h2>
             <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <router-link to="/teacher/class-progress" class="group flex items-center justify-between p-5 rounded-[24px] border border-slate-100 bg-slate-50 hover:bg-white hover:border-blue-200 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-12 w-12 rounded-2xl bg-blue-100 text-blue-600 flex items-center justify-center group-hover:scale-110 transition-transform">
                         <GraduationCap :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-bold text-slate-800">Tiến độ học tập</h4>
                         <p class="text-[11px] font-semibold text-slate-400">Theo dõi tiến độ sinh viên</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-slate-300 group-hover:text-blue-500 group-hover:translate-x-1 transition-all" />
                </router-link>
                
                <router-link to="/teacher/class-attendance" class="group flex items-center justify-between p-5 rounded-[24px] border border-slate-100 bg-slate-50 hover:bg-white hover:border-blue-200 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-12 w-12 rounded-2xl bg-cyan-100 text-cyan-600 flex items-center justify-center group-hover:scale-110 transition-transform">
                         <Clock :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-bold text-slate-800">Điểm danh</h4>
                         <p class="text-[11px] font-semibold text-slate-400">Quản lý chuyên cần</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-slate-300 group-hover:text-cyan-500 group-hover:translate-x-1 transition-all" />
                </router-link>

                <router-link to="/teacher/class-grades" class="group flex items-center justify-between p-5 rounded-[24px] border border-slate-100 bg-slate-50 hover:bg-white hover:border-blue-200 hover:shadow-md transition-all">
                   <div class="flex items-center gap-4">
                      <div class="h-12 w-12 rounded-2xl bg-indigo-100 text-indigo-600 flex items-center justify-center group-hover:scale-110 transition-transform">
                         <TrendingUp :size="20" />
                      </div>
                      <div>
                         <h4 class="text-sm font-bold text-slate-800">Bảng điểm</h4>
                         <p class="text-[11px] font-semibold text-slate-400">Nhập và xuất điểm</p>
                      </div>
                   </div>
                   <ChevronRight :size="18" class="text-slate-300 group-hover:text-indigo-500 group-hover:translate-x-1 transition-all" />
                </router-link>
             </div>
          </div>
       </div>

       <!-- Right: Recent Activities -->
       <div class="lg:col-span-1">
          <div class="bg-white rounded-[32px] border border-slate-100 p-8 shadow-sm h-full">
             <div class="flex justify-between items-center mb-6">
                <h2 class="text-lg font-black text-slate-800 flex items-center gap-2">
                   <Bell :size="20" class="text-amber-500" /> Hoạt động mới
                </h2>
                <button class="text-[10px] font-bold text-blue-600 uppercase tracking-widest hover:underline">Xem tất cả</button>
             </div>
             
             <div class="space-y-6 relative before:absolute before:inset-0 before:ml-5 before:-translate-x-px md:before:mx-auto md:before:translate-x-0 before:h-full before:w-0.5 before:bg-gradient-to-b before:from-transparent before:via-slate-200 before:to-transparent">
                <div v-for="act in recentActivities" :key="act.id" class="relative flex items-center justify-between md:justify-normal md:odd:flex-row-reverse group is-active">
                   <div class="flex items-center justify-center w-10 h-10 rounded-full border-4 border-white bg-slate-100 text-slate-500 shrink-0 md:order-1 md:group-odd:-translate-x-1/2 md:group-even:translate-x-1/2 shadow-sm z-10">
                      <FileText v-if="act.type === 'assignment'" :size="14" class="text-blue-500" />
                      <Clock v-if="act.type === 'leave'" :size="14" class="text-amber-500" />
                      <BookOpen v-if="act.type === 'material'" :size="14" class="text-emerald-500" />
                   </div>
                   <div class="w-[calc(100%-4rem)] md:w-[calc(50%-2.5rem)] bg-slate-50 p-4 rounded-2xl border border-slate-100 shadow-sm">
                      <div class="flex items-center justify-between mb-1">
                         <span class="text-[10px] font-black text-slate-400 uppercase tracking-wider">{{ act.time }}</span>
                      </div>
                      <h4 class="text-sm font-bold text-slate-800">{{ act.title }}</h4>
                   </div>
                </div>
             </div>
          </div>
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
