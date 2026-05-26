<script setup>
import { ref, onMounted } from 'vue'
import { 
  ArrowLeft, Search, User, Mail, Activity, Target, 
  BookOpen, Clock, ChevronRight, MoreVertical,
  CheckCircle2, AlertCircle, TrendingUp, Users, BookMarked, Filter
} from 'lucide-vue-next'

const students = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', email: 'anv@student.edu.vn', progress: 85, gpa: 8.2, absent: 1, status: 'good' },
  { id: 'SV16002', name: 'Trần Thị B', email: 'btt@student.edu.vn', progress: 70, gpa: 7.5, absent: 3, status: 'warning' },
  { id: 'SV16003', name: 'Lê Hoàng C', email: 'clh@student.edu.vn', progress: 95, gpa: 9.0, absent: 0, status: 'excellent' },
  { id: 'SV16004', name: 'Phạm Minh D', email: 'dpm@student.edu.vn', progress: 45, gpa: 5.4, absent: 5, status: 'danger' },
  { id: 'SV16005', name: 'Hoàng Hữu E', email: 'ehh@student.edu.vn', progress: 100, gpa: 9.5, absent: 0, status: 'excellent' },
  { id: 'SV16006', name: 'Vũ Thị F', email: 'fvt@student.edu.vn', progress: 60, gpa: 6.8, absent: 2, status: 'warning' },
])

const overallProgress = 74
const completedLessons = 18
const totalLessons = 24
const activeStudents = 42

const chartData = [
  { range: '< 20%', value: 5, height: 15 },
  { range: '20-40%', value: 8, height: 25 },
  { range: '40-60%', value: 12, height: 45 },
  { range: '60-80%', value: 18, height: 75 },
  { range: '80-100%', value: 25, height: 100 }
]

const getStatusColor = (status) => {
  return 'from-blue-400 to-cyan-400'
}

const getStatusBadge = (status) => {
  const badges = {
    excellent: 'bg-emerald-50 text-emerald-600 border-emerald-100/50 shadow-sm',
    good: 'bg-blue-50 text-blue-600 border-blue-100/50 shadow-sm',
    warning: 'bg-amber-50 text-amber-600 border-amber-100/50 shadow-sm',
    danger: 'bg-rose-50 text-rose-600 border-rose-100/50 shadow-sm'
  }
  return badges[status] || badges.good
}

const animateProgress = ref(false)
onMounted(() => {
  setTimeout(() => {
    animateProgress.value = true
  }, 100)
})

// --- Student Drawer State ---
const isDrawerOpen = ref(false)
const selectedStudent = ref(null)
const activeTab = ref('profile') // 'profile', 'assignments', 'activity'

const openStudentDetails = (studentId, tab) => {
  selectedStudent.value = students.value.find(s => s.id === studentId) || null
  activeTab.value = tab
  isDrawerOpen.value = true
  document.body.style.overflow = 'hidden' // prevent body scroll
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  document.body.style.overflow = ''
  setTimeout(() => {
    if (!isDrawerOpen.value) selectedStudent.value = null
  }, 300)
}
</script>

<template>
  <div class="space-y-8 pb-12 animate-fade-in">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <router-link to="/teacher/class-progress" class="h-10 w-10 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 flex items-center justify-center text-white shadow-md shadow-blue-200 hover:scale-105 transition-transform duration-300">
           <ArrowLeft :size="28" stroke-width="2.5" />
        </router-link>
        <div>
          <div class="flex items-center gap-3 mb-1.5">
            <span class="px-3 py-1 rounded-xl bg-blue-50 text-blue-600 text-[10px] font-black uppercase tracking-widest border border-blue-100/50">SE1601</span>
            <span class="flex items-center gap-1.5 text-emerald-600 bg-emerald-50 px-3 py-1 rounded-xl border border-emerald-100/50 text-[10px] font-black uppercase tracking-widest">
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
              Đang diễn ra
            </span>
          </div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Chi tiết & Tiến độ</h1>
        </div>
      </div>
      
      <div class="relative z-10 flex items-center gap-3">
        <button class="flex items-center gap-2 rounded-2xl bg-white px-5 py-3 border border-slate-200 shadow-sm hover:bg-blue-50 hover:border-blue-200 hover:text-blue-600 transition-colors font-bold text-sm text-slate-700">
           <BookMarked :size="18" /> Giáo trình
        </button>
        <button class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 px-4 py-3 text-sm font-bold text-white shadow-md shadow-blue-200 hover:shadow-lg transition-all hover:-translate-y-0.5 active:translate-y-0">
           <Mail :size="18" /> Gửi thông báo
        </button>
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="bg-white p-4 rounded-[24px] border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-50 to-blue-100/50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-2xl bg-blue-100 text-blue-600 flex items-center justify-center shadow-inner">
            <Users :size="24" stroke-width="2.5" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Sĩ số lớp</p>
            <h4 class="text-xl font-black text-slate-800">{{ activeStudents }}</h4>
          </div>
        </div>
        <p class="text-xs text-slate-500 flex items-center gap-1"><span class="text-emerald-500 font-medium">100%</span> sinh viên đang học</p>
      </div>
      
      <div class="bg-white p-4 rounded-[24px] border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-cyan-50 to-cyan-100/50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-2xl bg-cyan-100 text-cyan-600 flex items-center justify-center shadow-inner">
            <BookOpen :size="24" stroke-width="2.5" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Bài giảng</p>
            <h4 class="text-xl font-black text-slate-800">{{ completedLessons }}<span class="text-sm text-slate-400 font-medium">/{{ totalLessons }}</span></h4>
          </div>
        </div>
        <div class="w-full h-1.5 bg-slate-100 rounded-full overflow-hidden mt-2">
          <div class="h-full bg-cyan-500 rounded-full" :style="{ width: (completedLessons/totalLessons*100) + '%' }"></div>
        </div>
      </div>

      <div class="bg-white p-4 rounded-[24px] border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-emerald-50 to-emerald-100/50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-2xl bg-emerald-100 text-emerald-600 flex items-center justify-center shadow-inner">
            <TrendingUp :size="24" stroke-width="2.5" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Điểm TB Lớp</p>
            <h4 class="text-xl font-black text-slate-800">7.8</h4>
          </div>
        </div>
        <p class="text-xs text-slate-500 flex items-center gap-1"><span class="text-emerald-500 font-medium">+0.4</span> so với tháng trước</p>
      </div>

      <div class="bg-white p-4 rounded-[24px] border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-rose-50 to-rose-100/50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-4 mb-4">
          <div class="w-12 h-12 rounded-2xl bg-rose-100 text-rose-600 flex items-center justify-center shadow-inner">
            <AlertCircle :size="24" stroke-width="2.5" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-wider">Cảnh báo</p>
            <h4 class="text-xl font-black text-slate-800">2</h4>
          </div>
        </div>
        <p class="text-xs text-slate-500">Sinh viên có nguy cơ</p>
      </div>
    </div>

    <!-- Main Charts Section -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-4">
      <!-- Overall Progress Card -->
      <div class="relative bg-gradient-to-br from-blue-600 to-cyan-500 rounded-2xl p-5 text-white shadow-xl shadow-blue-500/20 overflow-hidden group">
        <div class="absolute top-0 right-0 w-64 h-64 bg-white/10 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2 pointer-events-none group-hover:scale-110 transition-transform duration-700"></div>
        <div class="absolute bottom-0 left-0 w-64 h-64 bg-black/10 rounded-full blur-3xl translate-y-1/2 -translate-x-1/2 pointer-events-none"></div>
        
        <div class="relative z-10 flex flex-col h-full justify-between">
          <div>
            <div class="flex justify-between items-center mb-8">
               <div class="flex items-center gap-3">
                 <div class="p-2.5 bg-white/20 rounded-2xl backdrop-blur-md border border-white/20 shadow-inner">
                   <Activity :size="24" class="text-white" />
                 </div>
                 <h3 class="font-bold text-white text-lg">Tiến độ chung</h3>
               </div>
            </div>
            
            <div class="flex flex-col items-center justify-center my-8">
              <div class="relative w-48 h-48 flex items-center justify-center">
                <!-- Circular Progress SVG -->
                <svg class="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
                  <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" class="text-white/20" stroke-width="8"></circle>
                  <circle cx="50" cy="50" r="45" fill="none" stroke="currentColor" class="text-white drop-shadow-lg transition-all duration-1500 ease-out" stroke-width="8" stroke-linecap="round" :stroke-dasharray="2 * Math.PI * 45" :stroke-dashoffset="animateProgress ? 2 * Math.PI * 45 * (1 - overallProgress / 100) : 2 * Math.PI * 45"></circle>
                </svg>
                <div class="absolute flex flex-col items-center justify-center text-center">
                  <span class="text-5xl font-black text-white drop-shadow-md">{{ overallProgress }}%</span>
                  <span class="text-[11px] font-bold uppercase tracking-widest text-cyan-100 mt-1">Hoàn thành</span>
                </div>
              </div>
            </div>
          </div>
          
          <div class="bg-white/10 backdrop-blur-md rounded-2xl p-4 border border-white/20 flex items-center justify-between mt-auto shadow-inner">
            <div class="text-center flex-1">
              <p class="text-[10px] text-cyan-100 font-black uppercase tracking-widest mb-1">Đã học</p>
              <p class="text-xl font-black">{{ completedLessons }}</p>
            </div>
            <div class="w-px h-10 bg-white/20"></div>
            <div class="text-center flex-1">
              <p class="text-[10px] text-cyan-100 font-black uppercase tracking-widest mb-1">Còn lại</p>
              <p class="text-xl font-black">{{ totalLessons - completedLessons }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Chart Card -->
      <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm lg:col-span-2 flex flex-col relative overflow-hidden group">
        <div class="flex justify-between items-start mb-8">
           <div>
             <h3 class="text-lg font-bold text-slate-800 flex items-center gap-2">
               <Target :size="20" class="text-blue-500" />
               Phân bố tiến độ sinh viên
             </h3>
             <p class="text-sm text-slate-500 mt-1">Biểu đồ thể hiện mức độ hoàn thành bài học của lớp</p>
           </div>
           <div class="p-2 bg-slate-50 rounded-lg text-slate-400">
             <MoreVertical :size="20" />
           </div>
        </div>
        
        <div class="flex-1 flex items-end justify-between gap-4 mt-auto">
           <div v-for="(item, i) in chartData" :key="i" class="flex-1 flex flex-col items-center group/bar cursor-pointer">
              <!-- Tooltip -->
              <div class="opacity-0 group-hover/bar:opacity-100 transition-opacity duration-200 mb-3 bg-slate-800 text-white text-xs font-bold py-1.5 px-3 rounded-lg relative whitespace-nowrap shadow-lg translate-y-2 group-hover/bar:translate-y-0">
                {{ item.value }} Sinh viên
                <div class="absolute -bottom-1 left-1/2 -translate-x-1/2 w-2 h-2 bg-slate-800 rotate-45"></div>
              </div>
              
              <!-- Bar -->
              <div class="w-full relative flex items-end justify-center rounded-t-xl overflow-hidden bg-slate-100 h-48 transition-all duration-300 group-hover/bar:bg-blue-50">
                <div class="w-full bg-gradient-to-t from-blue-600 to-blue-400 rounded-t-xl transition-all duration-1000 ease-out shadow-[0_0_15px_rgba(99,102,241,0.2)]" 
                     :style="{ height: animateProgress ? item.height + '%' : '0%' }">
                   <!-- Highlight on hover -->
                   <div class="absolute inset-0 bg-white/20 opacity-0 group-hover/bar:opacity-100 transition-opacity"></div>
                </div>
              </div>
              
              <!-- Label -->
              <div class="mt-4 text-xs font-bold text-slate-400 group-hover/bar:text-blue-600 transition-colors">
                {{ item.range }}
              </div>
           </div>
        </div>
      </div>
    </div>

    <!-- Students Table Section -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col">
      <div class="p-4 md:p-8 border-b border-slate-100/80 flex flex-col md:flex-row md:items-center justify-between gap-4 bg-slate-50/30">
        <div>
          <h2 class="text-xl font-bold text-slate-800">Danh sách sinh viên</h2>
          <p class="text-sm text-slate-500 mt-1">Quản lý và theo dõi chi tiết từng sinh viên trong lớp</p>
        </div>
        <div class="flex items-center gap-3">
          <div class="relative w-full md:w-72">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
            <input type="text" placeholder="Tìm kiếm sinh viên, mã SV..." class="w-full rounded-2xl border border-slate-200 bg-white pl-11 pr-4 py-3 text-sm outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
          </div>
          <button class="p-3 rounded-2xl border border-slate-200 bg-white text-slate-500 hover:bg-slate-50 transition-colors shadow-sm">
             <Filter :size="20" />
          </button>
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/80 text-[11px] font-bold uppercase tracking-widest text-slate-500">
              <th class="px-5 py-5 border-b border-slate-100">Sinh viên</th>
              <th class="px-4 py-5 border-b border-slate-100">Liên hệ</th>
              <th class="px-4 py-5 border-b border-slate-100">Tiến độ</th>
              <th class="px-4 py-5 border-b border-slate-100">Điểm TB</th>
              <th class="px-4 py-5 border-b border-slate-100">Trạng thái</th>
              <th class="px-5 py-5 border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100/80 bg-white">
            <tr v-for="sv in students" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors duration-200 cursor-pointer">
              <td class="px-5 py-5">
                <div class="flex items-center gap-4">
                  <div class="relative">
                    <div class="h-11 w-11 rounded-2xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-500 font-bold text-sm shadow-sm group-hover:bg-blue-100 group-hover:text-blue-600 group-hover:border-blue-200 transition-colors">
                      {{ sv.name.split(' ').pop()[0] }}
                    </div>
                    <div class="absolute -bottom-1 -right-1 w-3.5 h-3.5 rounded-full border-2 border-white" :class="sv.absent > 3 ? 'bg-rose-500' : 'bg-emerald-500'"></div>
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800 group-hover:text-blue-600 transition-colors">{{ sv.name }}</p>
                    <p class="text-[11px] text-slate-400 font-medium mt-0.5">{{ sv.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-2 text-sm text-slate-500 group-hover:text-slate-700 transition-colors">
                  <Mail :size="14" class="text-slate-400" />
                  {{ sv.email }}
                </div>
              </td>
              <td class="px-4 py-5 w-48">
                <div class="flex items-center gap-3">
                  <div class="flex-1 h-2 bg-slate-100 rounded-full overflow-hidden shadow-inner">
                    <div class="h-full rounded-full bg-gradient-to-r transition-all duration-1000 ease-out" 
                         :class="getStatusColor(sv.status)"
                         :style="{ width: animateProgress ? sv.progress + '%' : '0%' }"></div>
                  </div>
                  <span class="text-xs font-bold text-slate-700 w-8">{{ sv.progress }}%</span>
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-2">
                  <span class="text-sm font-black" :class="sv.gpa < 5 ? 'text-rose-500' : 'text-slate-800'">{{ sv.gpa }}</span>
                  <TrendingUp v-if="sv.gpa >= 8" :size="14" class="text-emerald-500" />
                  <AlertCircle v-else-if="sv.gpa < 5" :size="14" class="text-rose-500" />
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-[10px] font-black uppercase tracking-wider" :class="getStatusBadge(sv.status)">
                  <CheckCircle2 v-if="sv.status === 'excellent' || sv.status === 'good'" :size="12" />
                  <AlertCircle v-else :size="12" />
                  {{ sv.status === 'excellent' ? 'Xuất sắc' : sv.status === 'good' ? 'Khá tốt' : sv.status === 'warning' ? 'Cảnh báo' : 'Nguy hiểm' }}
                </div>
              </td>
              <td class="px-5 py-5">
                <div class="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300 translate-x-4 group-hover:translate-x-0">
                  <button @click="openStudentDetails(sv.id, 'profile')" class="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-xl transition-all shadow-sm border border-transparent hover:border-blue-100" title="Hồ sơ">
                    <User :size="16" />
                  </button>
                  <button @click="openStudentDetails(sv.id, 'assignments')" class="p-2 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-xl transition-all shadow-sm border border-transparent hover:border-emerald-100" title="Bài nộp">
                    <BookOpen :size="16" />
                  </button>
                  <button @click="openStudentDetails(sv.id, 'activity')" class="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-xl transition-all shadow-sm border border-transparent hover:border-blue-100" title="Chi tiết điểm">
                    <Activity :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="p-4 border-t border-slate-100/80 bg-slate-50/50 flex flex-col sm:flex-row items-center justify-between text-xs text-slate-500 gap-4">
        <span>Hiển thị 1-{{ students.length }} trong số {{ activeStudents }} sinh viên</span>
        <div class="flex gap-1">
          <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-50">Trước</button>
          <button class="px-3 py-1.5 rounded-lg border border-blue-200 bg-blue-50 text-blue-600 font-bold">1</button>
          <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50">2</button>
          <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50">3</button>
          <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50">Sau</button>
        </div>
      </div>
    </div>

    <!-- Student Slide-over Drawer -->
    <Teleport to="body">
      <div v-if="isDrawerOpen || selectedStudent" class="fixed inset-0 z-[999] overflow-hidden" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
        <div class="absolute inset-0 overflow-hidden">
          <div 
            class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm transition-opacity duration-300"
            :class="isDrawerOpen ? 'opacity-100' : 'opacity-0'"
            @click="closeDrawer"
          ></div>

          <div class="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
            <div 
              class="pointer-events-auto w-screen max-w-md transform transition duration-300 ease-in-out"
              :class="isDrawerOpen ? 'translate-x-0' : 'translate-x-full'"
            >
              <div class="flex h-full flex-col overflow-y-auto bg-white shadow-2xl rounded-l-[32px] border-l border-slate-100 relative">
                 
                 <template v-if="selectedStudent">
                   <!-- Drawer Header -->
                   <div class="p-5 pb-6 border-b border-slate-100 bg-slate-50/50 relative overflow-hidden">
                      <div class="absolute -top-10 -right-10 w-32 h-32 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-100 to-transparent rounded-full pointer-events-none"></div>
                      
                      <div class="flex justify-between items-start mb-4 relative z-10">
                         <div class="h-10 w-10 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 flex items-center justify-center text-white text-xl font-bold shadow-md shadow-blue-200">
                            {{ selectedStudent.name.split(' ').pop()[0] }}
                         </div>
                         <button @click="closeDrawer" class="p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-full transition-colors">
                           <ArrowLeft :size="20" class="rotate-180" />
                         </button>
                      </div>
                      <div class="relative z-10">
                         <h2 class="text-xl font-black text-slate-800">{{ selectedStudent.name }}</h2>
                         <p class="text-sm font-medium text-slate-500 mt-1">{{ selectedStudent.id }} • {{ selectedStudent.email }}</p>
                      </div>
                      
                      <!-- Tabs -->
                      <div class="flex items-center gap-4 mt-8 border-b border-slate-200">
                         <button @click="activeTab = 'profile'" :class="['pb-3 text-sm font-bold border-b-2 transition-all', activeTab === 'profile' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-400 hover:text-slate-600']">Hồ sơ</button>
                         <button @click="activeTab = 'assignments'" :class="['pb-3 text-sm font-bold border-b-2 transition-all', activeTab === 'assignments' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-400 hover:text-slate-600']">Bài tập</button>
                         <button @click="activeTab = 'activity'" :class="['pb-3 text-sm font-bold border-b-2 transition-all', activeTab === 'activity' ? 'border-blue-600 text-blue-600' : 'border-transparent text-slate-400 hover:text-slate-600']">Hoạt động</button>
                      </div>
                   </div>

                   <!-- Drawer Content -->
                   <div class="p-5 flex-1 bg-white">
                      
                      <!-- Profile Tab -->
                      <div v-if="activeTab === 'profile'" class="space-y-8 animate-fade-in">
                         <div class="grid grid-cols-2 gap-4">
                            <div class="p-4 rounded-2xl bg-slate-50 border border-slate-100">
                               <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Điểm TB (GPA)</p>
                               <p class="text-xl font-black text-slate-800">{{ selectedStudent.gpa }}</p>
                            </div>
                            <div class="p-4 rounded-2xl bg-slate-50 border border-slate-100">
                               <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Vắng mặt</p>
                               <p class="text-xl font-black text-rose-500">{{ selectedStudent.absent }} <span class="text-sm text-slate-500 font-medium">buổi</span></p>
                            </div>
                         </div>
                         
                         <div>
                            <h4 class="text-sm font-bold text-slate-800 mb-4">Mức độ hoàn thành</h4>
                            <div class="h-2.5 w-full bg-slate-100 rounded-full overflow-hidden">
                               <div class="h-full bg-gradient-to-r from-blue-400 to-cyan-400 rounded-full" :style="{ width: selectedStudent.progress + '%' }"></div>
                            </div>
                            <div class="flex justify-between items-center mt-2">
                               <span class="text-xs text-slate-500">Tiến độ khóa học</span>
                               <span class="text-xs font-bold text-slate-700">{{ selectedStudent.progress }}%</span>
                            </div>
                         </div>
                      </div>

                      <!-- Assignments Tab -->
                      <div v-if="activeTab === 'assignments'" class="space-y-4 animate-fade-in">
                         <div v-for="i in 3" :key="i" class="p-4 rounded-2xl border border-slate-100 flex items-center justify-between group hover:border-blue-200 transition-colors cursor-pointer">
                            <div class="flex items-center gap-3">
                               <div class="h-10 w-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
                                  <BookOpen :size="16" />
                               </div>
                               <div>
                                  <p class="text-sm font-bold text-slate-800">Lab {{ i }}: Thực hành Java</p>
                                  <p class="text-xs text-slate-500 mt-0.5">Nộp lúc 10:45 AM, Hôm qua</p>
                               </div>
                            </div>
                            <div class="text-right">
                               <span class="text-sm font-black text-emerald-600">9.{{ 5 - i }}</span>
                            </div>
                         </div>
                      </div>

                      <!-- Activity Tab -->
                      <div v-if="activeTab === 'activity'" class="space-y-4 relative before:absolute before:inset-0 before:ml-5 before:-translate-x-px md:before:mx-auto md:before:translate-x-0 before:h-full before:w-0.5 before:bg-gradient-to-b before:from-transparent before:via-slate-200 before:to-transparent animate-fade-in">
                         <div v-for="(act, idx) in ['Xem video bài giảng Chương 2', 'Bình luận trên diễn đàn lớp', 'Hoàn thành Quiz 1']" :key="idx" class="relative flex items-center justify-between md:justify-normal md:odd:flex-row-reverse group is-active">
                            <div class="flex items-center justify-center w-10 h-10 rounded-full border-4 border-white bg-slate-100 text-slate-500 shrink-0 md:order-1 md:group-odd:-translate-x-1/2 md:group-even:translate-x-1/2 shadow-sm z-10">
                               <Activity :size="14" class="text-blue-500" />
                            </div>
                            <div class="w-[calc(100%-4rem)] md:w-[calc(50%-2.5rem)] bg-slate-50 p-4 rounded-2xl border border-slate-100 shadow-sm">
                               <p class="text-[10px] font-black text-slate-400 uppercase tracking-wider mb-1">{{ idx + 1 }} ngày trước</p>
                               <h4 class="text-sm font-bold text-slate-800">{{ act }}</h4>
                            </div>
                         </div>
                      </div>

                   </div>
                 </template>

              </div>
            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
