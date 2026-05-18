<template>
  <div class="h-[calc(100vh-120px)] flex flex-col gap-6 pb-2 animate-fade-in">
    
    <!-- ── TOP BAR: Exam Info (Premium Dark Header) ── -->
    <div class="relative overflow-hidden rounded-[28px] bg-blue-900 p-6 text-white shadow-xl shadow-blue-200/50 flex flex-wrap items-center justify-between gap-4">
      <div class="absolute -right-10 -top-10 h-40 w-40 rounded-full bg-blue-500/30 blur-2xl transform-gpu pointer-events-none will-change-transform" />
      <div class="absolute -bottom-10 -left-10 h-40 w-40 rounded-full bg-cyan-500/30 blur-2xl transform-gpu pointer-events-none will-change-transform" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="relative flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-md border border-white/20 shadow-inner">
           <Monitor :size="28" class="text-blue-200" />
           <span class="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-rose-500 text-[10px] text-white animate-pulse">
             <div class="h-2 w-2 rounded-full bg-white"></div>
           </span>
        </div>
        <div>
           <h1 class="text-xl md:text-2xl font-black text-white uppercase tracking-tight">{{ examInfo.name }}</h1>
           <div class="flex flex-wrap items-center gap-4 mt-2">
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-white/10 backdrop-blur-md border border-white/10 text-xs font-bold text-blue-100">
                <Users :size="14" class="text-blue-300" /> Tổng: {{ examInfo.totalStudents }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500/20 backdrop-blur-md border border-emerald-500/30 text-xs font-bold text-emerald-300">
                <div class="h-2 w-2 rounded-full bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.8)]"></div> Đang thi: {{ examInfo.testing }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-rose-500/20 backdrop-blur-md border border-rose-500/30 text-xs font-bold text-rose-300">
                <div class="h-2 w-2 rounded-full bg-rose-400 shadow-[0_0_8px_rgba(251,113,133,0.8)]"></div> Mất kết nối: {{ examInfo.disconnected }}
              </div>
           </div>
        </div>
      </div>
      
      <div class="relative z-10 flex items-center gap-4 bg-black/40 backdrop-blur-xl border border-white/10 px-6 py-3 rounded-2xl shadow-2xl">
         <Clock :size="24" class="text-rose-400 animate-pulse" />
         <div class="flex flex-col items-end">
            <span class="text-[10px] font-bold text-blue-300 uppercase tracking-widest">Thời gian còn lại</span>
            <span class="text-3xl font-black font-mono leading-none text-white tracking-wider drop-shadow-[0_0_8px_rgba(255,255,255,0.5)]">{{ examInfo.timeRemaining }}</span>
         </div>
      </div>
    </div>

    <!-- ── MAIN CONTENT ── -->
    <div class="flex-1 flex gap-6 min-h-0">
      
      <!-- LEFT: Student List -->
      <aside class="w-[340px] flex flex-col rounded-[28px] bg-white border border-slate-100 shadow-sm overflow-hidden delay-100 animate-fade-in">
        <div class="p-5 border-b border-slate-100 bg-slate-50/80 flex items-center justify-between backdrop-blur-sm">
           <h2 class="font-bold text-slate-800 text-sm flex items-center gap-2">
             <Users :size="16" class="text-blue-600" /> Danh sách sinh viên
           </h2>
           <button class="p-2 rounded-xl bg-white border border-slate-200 hover:border-blue-300 hover:bg-blue-50 text-slate-500 hover:text-blue-600 transition-all shadow-sm">
             <RefreshCw :size="14" />
           </button>
        </div>
        <div class="flex-1 overflow-y-auto p-3 space-y-2 custom-scrollbar">
           <div v-for="sv in students" :key="sv.id" 
                @click="selectStudent(sv)"
                :class="[
                  'group flex items-center gap-4 p-3.5 rounded-[20px] cursor-pointer transition-all duration-300',
                  selectedStudent?.id === sv.id 
                    ? 'bg-gradient-to-r from-blue-600 to-blue-800 text-white shadow-lg shadow-blue-200 transform scale-[1.02]' 
                    : 'bg-white border border-slate-100 hover:border-blue-200 hover:shadow-md hover:bg-blue-50/30'
                ]"
           >
              <div class="relative flex-shrink-0">
                 <div :class="['h-11 w-11 rounded-2xl flex items-center justify-center font-bold text-sm shadow-sm transition-colors', 
                              selectedStudent?.id === sv.id ? 'bg-white/20 text-white' : 'bg-slate-100 text-slate-600 group-hover:bg-white group-hover:text-blue-600']">
                   {{ sv.name.split(' ').pop()[0] }}
                 </div>
                 <div :class="['absolute -bottom-1 -right-1 h-3.5 w-3.5 rounded-full border-2', 
                              selectedStudent?.id === sv.id ? 'border-blue-700' : 'border-white', getStatusColor(sv.status)]"></div>
              </div>
              <div class="flex-1 min-w-0">
                 <div class="flex items-center justify-between">
                    <p :class="['text-sm font-bold truncate', selectedStudent?.id === sv.id ? 'text-white' : 'text-slate-800']">{{ sv.name }}</p>
                    <span v-if="sv.violations > 0" class="flex h-5 w-5 items-center justify-center rounded-full bg-rose-500 text-[10px] font-bold text-white shadow-sm shadow-rose-200 animate-pulse-soft">
                       {{ sv.violations }}
                    </span>
                 </div>
                 <p :class="['text-[10px] uppercase tracking-widest mt-0.5 font-semibold', selectedStudent?.id === sv.id ? 'text-blue-200' : 'text-slate-400']">
                   {{ sv.status }} • {{ sv.id }}
                 </p>
              </div>
           </div>
        </div>
      </aside>

      <!-- RIGHT: Student Detail -->
      <main class="flex-1 flex flex-col rounded-[28px] bg-white border border-slate-100 shadow-sm overflow-hidden delay-200 animate-fade-in relative">
        <div v-if="selectedStudent" class="flex flex-col h-full">
           <!-- Detail Header -->
           <div class="p-6 border-b border-slate-100 bg-white flex items-center justify-between">
              <div class="flex items-center gap-5">
                 <div class="relative h-16 w-16 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600 border border-blue-100 shadow-sm">
                    <User :size="28" />
                    <div :class="['absolute -bottom-1.5 -right-1.5 h-4 w-4 rounded-full border-2 border-white shadow-sm', getStatusColor(selectedStudent.status)]"></div>
                 </div>
                 <div>
                    <h2 class="text-xl font-black text-slate-800">{{ selectedStudent.name }}</h2>
                    <p class="text-xs text-slate-500 font-bold tracking-widest uppercase mt-1">{{ selectedStudent.id }} • Khoa Công nghệ thông tin</p>
                 </div>
              </div>
              <div class="flex gap-6">
                 <div class="text-right px-6 border-r border-slate-100">
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Thời gian làm</p>
                    <p class="text-2xl font-mono font-black text-slate-800">00:44:12</p>
                 </div>
                 <div class="text-right pl-4">
                    <p class="text-[10px] font-black text-rose-500 uppercase tracking-widest">Số lần vi phạm</p>
                    <p class="text-2xl font-black text-rose-600">{{ selectedStudent.violations }}</p>
                 </div>
              </div>
           </div>

           <!-- Monitoring Area -->
           <div class="flex-1 flex overflow-hidden bg-slate-50/50">
              <!-- Logs -->
              <div class="flex-1 flex flex-col p-6 overflow-hidden">
                 <div class="flex items-center gap-2 mb-4">
                    <Terminal :size="18" class="text-slate-400" />
                    <span class="font-black text-sm text-slate-700 uppercase tracking-widest">Nhật ký hành vi</span>
                 </div>
                 <div class="flex-1 bg-slate-900 rounded-[24px] p-5 border border-slate-800 shadow-inner flex flex-col font-mono text-xs overflow-hidden relative group">
                    <div class="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-blue-900/20 via-slate-900 to-slate-900 pointer-events-none"></div>
                    <div class="flex-1 overflow-y-auto space-y-2 custom-scrollbar-dark relative z-10">
                       <div v-for="(log, i) in logs" :key="i" class="flex gap-4 opacity-80 hover:opacity-100 hover:bg-white/5 p-1.5 rounded-lg transition-all cursor-default">
                          <span class="text-slate-500 w-20 shrink-0 select-none">[{{ log.time }}]</span>
                          <span :class="log.type === 'warning' ? 'text-amber-400' : 'text-emerald-400'">
                            <span class="mr-2">{{ log.type === 'warning' ? '⚠️' : '▶' }}</span>
                            {{ log.msg }}
                          </span>
                       </div>
                       <div class="animate-pulse text-emerald-500/50 p-1.5">_</div>
                    </div>
                 </div>
              </div>

              <!-- Actions -->
              <div class="w-80 border-l border-slate-100 bg-white p-6 space-y-4 overflow-y-auto">
                 <h3 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-4">Hành động</h3>
                 
                 <button class="group w-full flex flex-col items-center justify-center rounded-[20px] bg-gradient-to-b from-amber-400 to-amber-500 p-4 text-white font-bold shadow-lg shadow-amber-200 hover:-translate-y-1 hover:shadow-xl hover:shadow-amber-200 transition-all border border-amber-300">
                    <ShieldAlert :size="24" class="mb-2 group-hover:scale-110 transition-transform" />
                    <span class="text-sm">Cảnh cáo sinh viên</span>
                 </button>

                 <button class="group w-full flex flex-col items-center justify-center rounded-[20px] bg-gradient-to-b from-blue-500 to-blue-600 p-4 text-white font-bold shadow-lg shadow-blue-200 hover:-translate-y-1 hover:shadow-xl hover:shadow-blue-200 transition-all border border-blue-400">
                    <MessageCircle :size="24" class="mb-2 group-hover:scale-110 transition-transform" />
                    <span class="text-sm">Gửi tin nhắn riêng</span>
                 </button>

                 <div class="pt-6 mt-6 border-t border-slate-100 space-y-3">
                    <button class="w-full flex items-center justify-between rounded-[16px] bg-slate-100 p-4 text-slate-700 font-bold hover:bg-slate-200 transition-all border border-slate-200">
                       <span class="text-sm">Khóa bài tạm thời</span>
                       <Lock :size="18" class="text-slate-400" />
                    </button>
                    <button class="w-full flex items-center justify-between rounded-[16px] bg-rose-50 p-4 text-rose-700 font-bold hover:bg-rose-100 transition-all border border-rose-200 shadow-sm shadow-rose-100/50 hover:shadow-md">
                       <span class="text-sm">Đình chỉ thi</span>
                       <XCircle :size="18" class="text-rose-500" />
                    </button>
                 </div>
              </div>
           </div>

           <!-- Send Quick Msg -->
           <div class="p-5 bg-white border-t border-slate-100 flex gap-4">
              <div class="relative flex-1">
                 <input type="text" placeholder="Nhập tin nhắn để gửi nhanh đến sinh viên này..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-5 pr-12 py-3.5 text-sm outline-none focus:border-blue-500 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all" />
              </div>
              <button class="rounded-[16px] bg-blue-600 px-8 py-3.5 text-white font-bold hover:bg-blue-700 flex items-center gap-2 shadow-lg shadow-blue-200 hover:-translate-y-0.5 transition-all">
                 <Send :size="18" /> Gửi
              </button>
           </div>
        </div>

        <div v-else class="h-full flex flex-col items-center justify-center text-center opacity-40 bg-slate-50/50">
           <div class="h-32 w-32 rounded-full bg-slate-200 flex items-center justify-center mb-6 shadow-inner">
             <Monitor :size="48" class="text-slate-400" />
           </div>
           <h3 class="font-black text-xl text-slate-600 mb-2">Chưa chọn sinh viên</h3>
           <p class="font-medium text-slate-500 text-sm">Chọn một sinh viên từ danh sách bên trái để bắt đầu giám sát</p>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { 
  Monitor, Wifi, WifiOff, AlertTriangle, RefreshCw, 
  Clock, ShieldAlert, MessageCircle, Lock, XCircle, 
  ChevronRight, Terminal, User, Send, Users 
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
  { time: '09:30:12', msg: 'Bắt đầu làm bài', type: 'info' },
  { time: '09:35:45', msg: 'Hệ thống ghi nhận hành động: Chuyển Tab (Lần 1)', type: 'warning' },
  { time: '09:42:10', msg: 'Hệ thống ghi nhận hành động: Chuyển Tab (Lần 2)', type: 'warning' },
  { time: '09:45:00', msg: 'Phục hồi trạng thái: Đang làm bài ổn định', type: 'info' },
])

function selectStudent(sv) {
  selectedStudent.value = sv
}

function getStatusColor(status) {
  switch (status) {
    case 'Testing': return 'bg-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.6)]'
    case 'Submitted': return 'bg-blue-500 shadow-[0_0_10px_rgba(59,130,246,0.6)]'
    case 'Disconnected': return 'bg-rose-500 shadow-[0_0_10px_rgba(244,63,94,0.6)]'
    default: return 'bg-slate-500'
  }
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar-dark::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar-dark::-webkit-scrollbar-thumb {
  background: rgba(255,255,255,0.15);
  border-radius: 10px;
}
.custom-scrollbar-dark::-webkit-scrollbar-thumb:hover {
  background: rgba(255,255,255,0.25);
}

@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.5s cubic-bezier(0.16, 1, 0.3, 1) both;
  will-change: opacity, transform;
}
.delay-100 { animation-delay: 100ms; }
.delay-200 { animation-delay: 200ms; }

@keyframes pulse-soft {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.8; transform: scale(1.05); }
}
.animate-pulse-soft {
  animation: pulse-soft 2s infinite ease-in-out;
}
</style>
