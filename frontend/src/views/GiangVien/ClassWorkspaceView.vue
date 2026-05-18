<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { 
  ArrowLeft, Play, FileText, CheckCircle2, MessageSquare, 
  Settings, Maximize, Video, Mic, MicOff, VideoOff, PhoneOff
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()

const isMicOn = ref(true)
const isCameraOn = ref(false)

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
  <div class="h-[calc(100vh-6rem)] w-full bg-slate-900 rounded-[32px] overflow-hidden flex flex-col md:flex-row relative shadow-2xl animate-fade-in border border-slate-800">
    
    <!-- Top Bar Overlay for Mobile/Small screens (Optional, here integrated into layout) -->
    
    <!-- Main Content Area (Video/Presentation) -->
    <div class="flex-1 flex flex-col relative bg-black">
       <!-- Top actions -->
       <div class="absolute top-0 left-0 right-0 p-6 flex justify-between items-center z-10 bg-gradient-to-b from-black/80 to-transparent">
          <button @click="router.push(`/teacher/classes/${route.params.id}/details`)" class="h-10 w-10 rounded-full bg-white/10 hover:bg-white/20 backdrop-blur flex items-center justify-center text-white transition-colors border border-white/10">
             <ArrowLeft :size="20" />
          </button>
          <div class="flex items-center gap-3">
             <span class="px-3 py-1.5 rounded-full bg-red-500/20 text-red-500 text-[10px] font-black uppercase tracking-widest border border-red-500/30 flex items-center gap-2">
                <span class="w-2 h-2 rounded-full bg-red-500 animate-pulse"></span> LIVE
             </span>
             <button class="h-10 w-10 rounded-full bg-white/10 hover:bg-white/20 backdrop-blur flex items-center justify-center text-white transition-colors border border-white/10">
                <Settings :size="18" />
             </button>
          </div>
       </div>

       <!-- Video Placeholder -->
       <div class="flex-1 relative flex items-center justify-center overflow-hidden">
          <div class="absolute inset-0 bg-slate-800 flex items-center justify-center">
             <div class="text-center space-y-4">
                <div class="h-24 w-24 rounded-full bg-slate-700 mx-auto flex items-center justify-center text-slate-500">
                   <Video :size="40" stroke-width="1.5" />
                </div>
                <p class="text-slate-400 font-medium text-sm">Camera đang tắt</p>
             </div>
          </div>
          <!-- Mock User Video PiP -->
          <div class="absolute bottom-6 right-6 w-48 h-32 bg-slate-700 rounded-2xl border-2 border-slate-600 overflow-hidden shadow-xl flex items-center justify-center">
             <div class="text-white font-bold text-2xl">SV</div>
          </div>
       </div>

       <!-- Bottom Controls -->
       <div class="h-24 bg-slate-900/90 backdrop-blur-md border-t border-slate-800 flex items-center justify-between px-8">
          <div class="flex items-center gap-4 text-white">
             <div>
                <h3 class="font-bold text-sm">Chương 2: Hướng đối tượng cơ bản</h3>
                <p class="text-xs text-slate-400">Đang phát • 24 sinh viên đang xem</p>
             </div>
          </div>
          <div class="flex items-center gap-3">
             <button @click="isMicOn = !isMicOn" :class="['h-12 w-12 rounded-full flex items-center justify-center transition-all', isMicOn ? 'bg-slate-800 text-white hover:bg-slate-700' : 'bg-red-500 text-white hover:bg-red-600 shadow-[0_0_15px_rgba(239,68,68,0.5)]']">
                <Mic v-if="isMicOn" :size="20" />
                <MicOff v-else :size="20" />
             </button>
             <button @click="isCameraOn = !isCameraOn" :class="['h-12 w-12 rounded-full flex items-center justify-center transition-all', isCameraOn ? 'bg-slate-800 text-white hover:bg-slate-700' : 'bg-red-500 text-white hover:bg-red-600 shadow-[0_0_15px_rgba(239,68,68,0.5)]']">
                <Video v-if="isCameraOn" :size="20" />
                <VideoOff v-else :size="20" />
             </button>
             <button class="h-12 w-12 rounded-full bg-slate-800 text-white hover:bg-slate-700 flex items-center justify-center transition-all">
                <Maximize :size="20" />
             </button>
             <div class="w-px h-8 bg-slate-700 mx-2"></div>
             <button class="h-12 px-6 rounded-full bg-red-500 text-white font-bold text-sm hover:bg-red-600 flex items-center gap-2 transition-all">
                <PhoneOff :size="18" /> Kết thúc
             </button>
          </div>
       </div>
    </div>

    <!-- Right Sidebar (Playlist / Chat) -->
    <div class="w-full md:w-80 lg:w-96 bg-slate-900 border-l border-slate-800 flex flex-col">
       <div class="p-6 border-b border-slate-800">
          <h2 class="text-white font-bold text-lg">Nội dung khóa học</h2>
          <div class="w-full bg-slate-800 h-1.5 rounded-full mt-4 overflow-hidden">
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

       <!-- Chat Placeholder -->
       <div class="h-16 border-t border-slate-800 bg-slate-900 p-3">
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
