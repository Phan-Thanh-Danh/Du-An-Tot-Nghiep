<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  ArrowLeft, Play, FileText, CheckCircle2, MessageSquare, 
  Settings, Maximize, Minimize, Video, Mic, MicOff, VideoOff, PhoneOff,
  UserCheck, UserX
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()

const isMicOn = ref(true)
const isCameraOn = ref(false)
const activeTab = ref('content')
const isAttendanceExpanded = ref(false)

const students = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', present: true },
  { id: 'SV16002', name: 'Trần Thị B', present: true },
  { id: 'SV16003', name: 'Lê Hoàng C', present: false },
  { id: 'SV16004', name: 'Phạm Minh D', present: true },
  { id: 'SV16005', name: 'Hoàng Hữu E', present: true },
  { id: 'SV16006', name: 'Vũ Thị F', present: false },
])

const modules = ref([
  {
    id: 1,
    title: 'Chương 1: Tổng quan về Java',
    duration: '45 phút',
    status: 'completed',
    type: 'video'
  },
  {
    id: 2,
    title: 'Chương 2: Hướng đối tượng cơ bản',
    duration: '1h 20 phút',
    status: 'playing',
    type: 'video'
  },
  {
    id: 3,
    title: 'Bài tập thực hành: OOP',
    duration: 'Giới hạn 2 ngày',
    status: 'locked',
    type: 'exercise'
  }
])
</script>

<template>
  <div class="min-h-[88vh] w-full bg-slate-900 rounded-2xl overflow-hidden flex flex-col md:flex-row relative shadow-2xl animate-fade-in border border-slate-800">
    
    <!-- Top Bar Overlay for Mobile/Small screens (Optional, here integrated into layout) -->
    
    <!-- Main Content Area (Google Meet) -->
    <div v-show="!isAttendanceExpanded" class="flex-1 flex flex-col relative bg-slate-900 transition-all duration-300">
       <!-- Top actions -->
       <div class="absolute top-0 left-0 right-0 p-4 flex justify-between items-center z-10">
          <button @click="router.push(`/teacher/classes/${route.params.id}/details`)" class="h-10 w-10 rounded-full bg-white/10 hover:bg-white/20 backdrop-blur flex items-center justify-center text-white transition-colors border border-white/10">
             <ArrowLeft :size="20" />
          </button>
          <div class="flex items-center gap-3">
             <span class="px-3 py-1.5 rounded-full bg-emerald-500/20 text-emerald-400 text-[10px] font-black uppercase tracking-widest border border-emerald-500/30 flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span> ONLINE
             </span>
             <button class="h-10 w-10 rounded-full bg-white/10 hover:bg-white/20 backdrop-blur flex items-center justify-center text-white transition-colors border border-white/10">
                <Settings :size="18" />
             </button>
          </div>
       </div>

       <!-- Google Meet Integration -->
       <div class="flex-1 relative flex items-center justify-center overflow-hidden bg-slate-900/50">
          <div class="text-center space-y-4 max-w-md p-5 bg-slate-800/80 rounded-2xl border border-slate-700 shadow-2xl backdrop-blur-sm">
             <div class="h-24 w-24 rounded-[24px] bg-gradient-to-br from-emerald-400 to-green-600 mx-auto flex items-center justify-center text-white shadow-lg shadow-green-500/20">
                <Video :size="48" stroke-width="2" />
             </div>
             
             <div>
               <h3 class="text-xl font-black text-white tracking-tight">Lớp học trực tuyến</h3>
               <p class="text-slate-400 font-medium mt-2">Buổi học hiện đang được tổ chức qua Google Meet. Nhấn vào nút bên dưới để tham gia phòng học.</p>
             </div>

             <div class="pt-4 border-t border-slate-700">
               <a href="https://meet.google.com/new" target="_blank" rel="noopener noreferrer" class="inline-flex items-center justify-center gap-3 w-full bg-emerald-500 hover:bg-emerald-600 text-white px-5 py-4 rounded-2xl font-bold text-lg transition-all shadow-lg hover:shadow-emerald-500/30 hover:-translate-y-1 active:translate-y-0">
                  <Video :size="24" /> Tham gia Google Meet
               </a>
             </div>
          </div>
       </div>
    </div>

    <!-- Right Sidebar (Playlist / Chat / Attendance) -->
    <div :class="['bg-slate-900 border-l border-slate-800 flex flex-col transition-all duration-300', isAttendanceExpanded ? 'w-full' : 'w-full md:w-[400px] lg:w-[450px]']">
       <div class="flex border-b border-slate-800">
          <button @click="activeTab = 'content'" :class="['flex-1 py-4 text-sm font-bold border-b-2 transition-colors', activeTab === 'content' ? 'border-blue-500 text-blue-400' : 'border-transparent text-slate-400 hover:text-slate-200']">
             Nội dung
          </button>
          <button @click="activeTab = 'attendance'" :class="['flex-1 py-4 text-sm font-bold border-b-2 transition-colors', activeTab === 'attendance' ? 'border-emerald-500 text-emerald-400' : 'border-transparent text-slate-400 hover:text-slate-200']">
             Điểm danh
          </button>
       </div>

       <!-- Tab: Content -->
       <div v-if="activeTab === 'content'" class="flex-1 flex flex-col min-h-0">
          <div class="p-4 pb-2">
             <div class="w-full bg-slate-800 h-1.5 rounded-full overflow-hidden">
                <div class="bg-blue-500 h-full w-1/3"></div>
             </div>
             <p class="text-xs text-slate-400 mt-2">Hoàn thành 1/3 nội dung</p>
          </div>

          <div class="flex-1 overflow-y-auto p-4 space-y-2 scrollbar-thin">
             <div v-for="mod in modules" :key="mod.id" 
                :class="[
                  'p-4 rounded-2xl border transition-all cursor-pointer flex gap-4',
                  mod.status === 'playing' ? 'bg-blue-600/10 border-blue-500/30' : 'bg-slate-800/50 border-transparent hover:bg-slate-800'
                ]">
                <div :class="['mt-0.5 h-6 w-6 rounded-full flex items-center justify-center shrink-0', 
                   mod.status === 'completed' ? 'bg-emerald-500/20 text-emerald-400' : 
                   mod.status === 'playing' ? 'bg-blue-500 text-white' : 'bg-slate-700 text-slate-500']">
                   <CheckCircle2 v-if="mod.status === 'completed'" :size="14" />
                   <Play v-else-if="mod.status === 'playing'" :size="12" class="ml-0.5" />
                   <FileText v-else :size="12" />
                </div>
                <div>
                   <h4 :class="['text-sm font-bold', mod.status === 'playing' ? 'text-blue-400' : 'text-slate-200']">{{ mod.title }}</h4>
                   <p class="text-xs text-slate-500 mt-1">{{ mod.duration }}</p>
                </div>
             </div>
          </div>
       </div>

       <!-- Tab: Attendance -->
       <div v-if="activeTab === 'attendance'" class="flex-1 flex flex-col min-h-0">
          <div class="p-4 flex items-center justify-between border-b border-slate-800 bg-slate-800/20">
             <div class="flex gap-4">
                <span class="text-xs font-bold text-emerald-400">Có mặt: {{ students.filter(s => s.present).length }}</span>
                <span class="text-xs font-bold text-rose-400">Vắng: {{ students.filter(s => !s.present).length }}</span>
             </div>
             <button @click="isAttendanceExpanded = !isAttendanceExpanded" class="h-8 w-8 rounded-lg bg-slate-800 flex items-center justify-center text-slate-400 hover:text-white hover:bg-slate-700 transition-colors shadow-sm" :title="isAttendanceExpanded ? 'Thu nhỏ' : 'Phóng to thành trang riêng'">
                <Minimize v-if="isAttendanceExpanded" :size="16" />
                <Maximize v-else :size="16" />
             </button>
          </div>
          <div class="flex-1 overflow-y-auto p-4 scrollbar-thin">
             <div class="flex flex-col gap-3">
                <div v-for="sv in students" :key="sv.id" :class="['flex justify-between rounded-2xl border border-slate-800 bg-slate-800/50 hover:bg-slate-800 transition-colors', isAttendanceExpanded ? 'flex-col sm:flex-row sm:items-center p-4 gap-4' : 'items-center p-3 gap-3']">
                   <div class="flex items-center gap-3">
                      <div :class="['rounded-full bg-slate-700 text-slate-300 flex items-center justify-center font-bold border border-slate-600 shadow-inner shrink-0', isAttendanceExpanded ? 'h-10 w-10 text-lg' : 'h-9 w-9 text-xs']">
                         {{ sv.name.split(' ').pop()[0] }}
                      </div>
                      <div>
                         <p :class="['font-bold text-slate-200', isAttendanceExpanded ? 'text-base' : 'text-sm']">{{ sv.name }}</p>
                         <p :class="['text-slate-500 font-bold uppercase tracking-wider', isAttendanceExpanded ? 'text-xs mt-0.5' : 'text-[10px]']">{{ sv.id }}</p>
                      </div>
                   </div>
                   <div :class="['flex items-center', isAttendanceExpanded ? 'gap-3' : 'gap-2']">
                      <button @click="sv.present = true" :class="['flex items-center justify-center rounded-xl transition-all border font-bold', 
                           isAttendanceExpanded ? 'h-12 px-4 text-sm flex-1 sm:flex-none font-black' : 'h-9 w-9 text-xs', 
                           sv.present ? 'bg-emerald-500/20 text-emerald-400 border-emerald-500/50 shadow-[0_0_15px_rgba(16,185,129,0.15)]' : 'bg-slate-800/50 text-slate-500 border-slate-700/50 hover:bg-slate-700 hover:text-slate-300']">
                         <span v-if="isAttendanceExpanded">CÓ MẶT</span>
                         <UserCheck v-else :size="16" />
                      </button>
                      <button @click="sv.present = false" :class="['flex items-center justify-center rounded-xl transition-all border font-bold', 
                           isAttendanceExpanded ? 'h-12 px-4 text-sm flex-1 sm:flex-none font-black' : 'h-9 w-9 text-xs', 
                           !sv.present ? 'bg-rose-500/20 text-rose-400 border-rose-500/50 shadow-[0_0_15px_rgba(244,63,94,0.15)]' : 'bg-slate-800/50 text-slate-500 border-slate-700/50 hover:bg-slate-700 hover:text-slate-300']">
                         <span v-if="isAttendanceExpanded">VẮNG MẶT</span>
                         <UserX v-else :size="16" />
                      </button>
                   </div>
                </div>
             </div>
          </div>
          <div class="p-5 border-t border-slate-800 bg-slate-900 flex justify-end">
             <button class="rounded-xl bg-emerald-500 text-white font-bold text-sm px-5 py-3.5 hover:bg-emerald-600 transition-colors shadow-lg shadow-emerald-500/20 flex items-center justify-center gap-2 w-full md:w-auto">
                <UserCheck :size="18" /> Lưu điểm danh
             </button>
          </div>
       </div>

       <!-- Chat Placeholder -->
       <div v-if="!isAttendanceExpanded" class="h-16 border-t border-slate-800 bg-slate-900 p-3">
          <div class="w-full h-full bg-slate-800 rounded-xl flex items-center px-4 gap-3 border border-slate-700 cursor-pointer hover:border-slate-600 transition-colors">
             <MessageSquare :size="18" class="text-slate-400" />
             <span class="text-sm font-medium text-slate-400">Thảo luận lớp học...</span>
          </div>
       </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.3s ease-out forwards;
}

.scrollbar-thin::-webkit-scrollbar {
  width: 4px;
}
.scrollbar-thin::-webkit-scrollbar-track {
  background: transparent;
}
.scrollbar-thin::-webkit-scrollbar-thumb {
  background: #334155;
  border-radius: 999px;
}
</style>
