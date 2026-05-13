<script setup>
import { ref } from 'vue'
import { 
  Monitor, Wifi, WifiOff, AlertTriangle, RefreshCw, 
  Clock, ShieldAlert, MessageCircle, Lock, XCircle, 
  ChevronRight, Terminal, User, Send 
} from 'lucide-vue-next'

const examInfo = {
  name: 'Thi kết thúc môn: Lập trình Web (Spring 2026)',
  totalStudents: 45,
  testing: 38,
  submitted: 4,
  disconnected: 3,
  timeRemaining: '45:12'
}

const students = ref([
  { id: 'SV001', name: 'Nguyễn Văn A', status: 'Testing', violations: 2, warnings: ['Tab switch', 'Tab switch'] },
  { id: 'SV002', name: 'Trần Thị B', status: 'Testing', violations: 0, warnings: [] },
  { id: 'SV003', name: 'Lê Hoàng C', status: 'Submitted', violations: 0, warnings: [] },
  { id: 'SV004', name: 'Phạm Minh D', status: 'Disconnected', violations: 1, warnings: ['Lost connection'] },
  { id: 'SV005', name: 'Hoàng Văn E', status: 'Testing', violations: 5, warnings: ['Tab switch', 'Reload', 'Tab switch', 'Tab switch', 'Tab switch'] },
])

const selectedStudent = ref(students.value[0])

const logs = ref([
  { time: '09:30:12', msg: 'Bắt đầu làm bài' },
  { time: '09:35:45', msg: 'Chuyển Tab (Lần 1)', type: 'warning' },
  { time: '09:42:10', msg: 'Chuyển Tab (Lần 2)', type: 'warning' },
  { time: '09:45:00', msg: 'Đang làm bài ổn định' },
])

function selectStudent(sv) {
  selectedStudent.value = sv
}

function getStatusColor(status) {
  switch (status) {
    case 'Testing': return 'bg-emerald-500'
    case 'Submitted': return 'bg-blue-500'
    case 'Disconnected': return 'bg-rose-500'
    default: return 'bg-slate-500'
  }
}
</script>

<template>
  <div class="h-[calc(100vh-120px)] flex flex-col gap-4 pb-2">
    
    <!-- ── TOP BAR: Exam Info ── -->
    <div class="lg-card-glass p-4 border-slate-100 flex flex-wrap items-center justify-between gap-4">
      <div class="flex items-center gap-4">
        <div class="h-12 w-12 rounded-2xl bg-rose-600 flex items-center justify-center text-white shadow-lg animate-pulse">
           <Monitor :size="24" />
        </div>
        <div>
           <h1 class="text-lg font-black text-slate-800 uppercase tracking-tight">{{ examInfo.name }}</h1>
           <div class="flex items-center gap-4 mt-1">
              <span class="flex items-center gap-1.5 text-xs font-bold text-slate-500">
                <Users :size="14" /> Tổng: {{ examInfo.totalStudents }}
              </span>
              <span class="flex items-center gap-1.5 text-xs font-bold text-emerald-600">
                <div class="h-2 w-2 rounded-full bg-emerald-500"></div> Đang thi: {{ examInfo.testing }}
              </span>
              <span class="flex items-center gap-1.5 text-xs font-bold text-rose-600">
                <div class="h-2 w-2 rounded-full bg-rose-500"></div> Mất kết nối: {{ examInfo.disconnected }}
              </span>
           </div>
        </div>
      </div>
      
      <div class="flex items-center gap-6 bg-slate-900 px-6 py-3 rounded-2xl text-white shadow-xl">
         <Clock :size="20" class="text-rose-400" />
         <div class="flex flex-col">
            <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Thời gian còn lại</span>
            <span class="text-2xl font-black font-mono leading-none">{{ examInfo.timeRemaining }}</span>
         </div>
      </div>
    </div>

    <!-- ── MAIN CONTENT ── -->
    <div class="flex-1 flex gap-4 min-h-0">
      
      <!-- LEFT: Student List -->
      <aside class="w-80 flex flex-col lg-card-glass p-0 overflow-hidden border-slate-100">
        <div class="p-4 border-b border-slate-100 bg-slate-50/50 flex items-center justify-between">
           <h2 class="font-bold text-slate-800 text-sm">Danh sách sinh viên</h2>
           <button class="p-1.5 rounded-lg hover:bg-slate-200 text-slate-400 transition-colors"><RefreshCw :size="16" /></button>
        </div>
        <div class="flex-1 overflow-y-auto p-2 space-y-1 custom-scrollbar">
           <div v-for="sv in students" :key="sv.id" 
                @click="selectStudent(sv)"
                :class="[
                  'group flex items-center gap-3 p-3 rounded-xl cursor-pointer transition-all border',
                  selectedStudent?.id === sv.id 
                    ? 'bg-slate-900 text-white border-slate-900 shadow-lg scale-[1.02]' 
                    : 'bg-white border-slate-50 hover:border-slate-200'
                ]"
           >
              <div class="relative flex-shrink-0">
                 <div class="h-9 w-9 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs group-hover:bg-indigo-100">
                   {{ sv.name.split(' ').pop()[0] }}
                 </div>
                 <div :class="['absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full border-2 border-white', getStatusColor(sv.status)]"></div>
              </div>
              <div class="flex-1 min-w-0">
                 <div class="flex items-center justify-between">
                    <p class="text-xs font-bold truncate">{{ sv.name }}</p>
                    <span v-if="sv.violations > 0" class="flex h-4 w-4 items-center justify-center rounded-full bg-rose-500 text-[10px] font-bold text-white animate-bounce">
                       {{ sv.violations }}
                    </span>
                 </div>
                 <p :class="['text-[9px] uppercase tracking-tighter', selectedStudent?.id === sv.id ? 'text-slate-400' : 'text-slate-400']">
                   {{ sv.status }} • {{ sv.id }}
                 </p>
              </div>
           </div>
        </div>
      </aside>

      <!-- RIGHT: Student Detail -->
      <main class="flex-1 flex flex-col lg-card-glass p-0 overflow-hidden border-slate-100 relative">
        <div v-if="selectedStudent" class="flex flex-col h-full">
           <!-- Detail Header -->
           <div class="p-6 border-b border-slate-100 bg-white flex items-center justify-between">
              <div class="flex items-center gap-4">
                 <div class="h-16 w-16 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100 shadow-sm">
                    <User :size="32" />
                 </div>
                 <div>
                    <h2 class="text-xl font-black text-slate-800">{{ selectedStudent.name }}</h2>
                    <p class="text-xs text-slate-400 font-bold tracking-widest uppercase">{{ selectedStudent.id }} • Khoa Công nghệ thông tin</p>
                 </div>
              </div>
              <div class="flex gap-4">
                 <div class="text-center px-6 border-r border-slate-100">
                    <p class="text-[10px] font-black text-slate-400 uppercase">Thời gian làm</p>
                    <p class="text-lg font-mono font-black text-slate-800">00:44:12</p>
                 </div>
                 <div class="text-center px-6">
                    <p class="text-[10px] font-black text-rose-500 uppercase">Số lần vi phạm</p>
                    <p class="text-lg font-black text-rose-600">{{ selectedStudent.violations }}</p>
                 </div>
              </div>
           </div>

           <!-- Monitoring Area -->
           <div class="flex-1 flex overflow-hidden">
              <!-- Logs -->
              <div class="flex-1 flex flex-col p-6 bg-slate-900 text-emerald-400 font-mono text-xs overflow-hidden">
                 <div class="flex items-center gap-2 mb-4 border-b border-white/10 pb-4 text-white">
                    <Terminal :size="16" />
                    <span class="font-bold uppercase tracking-widest">Behavior Log / Nhật ký hành vi</span>
                 </div>
                 <div class="flex-1 overflow-y-auto space-y-2 custom-scrollbar-dark">
                    <div v-for="(log, i) in logs" :key="i" class="flex gap-4 opacity-80 hover:opacity-100 transition-opacity">
                       <span class="text-slate-500">{{ log.time }}</span>
                       <span :class="log.type === 'warning' ? 'text-amber-400 font-bold' : 'text-emerald-400'">
                         {{ log.type === 'warning' ? '⚠️ ' : '✓ ' }} {{ log.msg }}
                       </span>
                    </div>
                    <div class="animate-pulse">_</div>
                 </div>
              </div>

              <!-- Actions -->
              <div class="w-80 border-l border-slate-100 bg-slate-50/30 p-6 space-y-4">
                 <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-6">Điều khiển / Actions</h3>
                 
                 <button class="w-full flex items-center justify-between rounded-2xl bg-amber-500 p-4 text-white font-bold shadow-lg shadow-amber-100 hover:scale-105 transition-all">
                    <div class="flex items-center gap-3">
                       <ShieldAlert :size="20" />
                       <span class="text-sm">Cảnh cáo SV</span>
                    </div>
                    <ChevronRight :size="16" />
                 </button>

                 <button class="w-full flex items-center justify-between rounded-2xl bg-indigo-600 p-4 text-white font-bold shadow-lg shadow-indigo-100 hover:scale-105 transition-all">
                    <div class="flex items-center gap-3">
                       <MessageCircle :size="20" />
                       <span class="text-sm">Gửi Message</span>
                    </div>
                    <ChevronRight :size="16" />
                 </button>

                 <div class="pt-6 mt-6 border-t border-slate-200 space-y-4">
                    <button class="w-full flex items-center gap-3 rounded-2xl bg-slate-900 p-4 text-white font-bold hover:bg-slate-800 transition-all">
                       <Lock :size="20" class="text-rose-400" />
                       <span class="text-sm">Khóa bài tạm thời</span>
                    </button>
                    <button class="w-full flex items-center gap-3 rounded-2xl bg-rose-600 p-4 text-white font-bold hover:bg-rose-700 transition-all shadow-lg shadow-rose-100">
                       <XCircle :size="20" />
                       <span class="text-sm">Hủy bài thi ( Đình chỉ )</span>
                    </button>
                 </div>
              </div>
           </div>

           <!-- Send Quick Msg -->
           <div class="p-4 bg-white border-t border-slate-100 flex gap-3">
              <input type="text" placeholder="Nhập tin nhắn nhanh cho sinh viên..." class="flex-1 rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
              <button class="rounded-xl bg-indigo-600 px-6 py-2.5 text-white font-bold hover:bg-indigo-700 flex items-center gap-2">
                 <Send :size="18" /> Gửi
              </button>
           </div>
        </div>

        <div v-else class="h-full flex flex-col items-center justify-center text-center opacity-30">
           <Monitor :size="80" class="text-slate-300 mb-4" />
           <p class="font-bold text-slate-400">Chọn sinh viên từ danh sách để bắt đầu giám sát</p>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 3px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar-dark::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar-dark::-webkit-scrollbar-thumb {
  background: rgba(255,255,255,0.1);
  border-radius: 10px;
}
</style>
