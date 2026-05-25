<template>
  <div class="h-[calc(100vh-120px)] flex flex-col gap-6 pb-2 animate-fade-in relative">
    
    <!-- ── TOP BAR: CCTV System Info (Premium Dark Header) ── -->
    <div class="relative overflow-hidden rounded-[28px] bg-slate-900 p-6 text-white shadow-xl flex flex-wrap items-center justify-between gap-4 flex-shrink-0 border border-slate-800">
      <div class="absolute -right-10 -top-10 h-40 w-40 rounded-full bg-blue-500/20 blur-2xl pointer-events-none" />
      <div class="absolute -bottom-10 -left-10 h-40 w-40 rounded-full bg-rose-500/20 blur-2xl pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="relative flex h-14 w-14 items-center justify-center rounded-2xl bg-white/5 border border-white/10 shadow-inner backdrop-blur-md">
           <Video class="text-blue-400" :size="28" />
           <span class="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-emerald-500 animate-pulse">
             <div class="h-2 w-2 rounded-full bg-white"></div>
           </span>
        </div>
        <div>
           <h1 class="text-xl md:text-2xl font-black text-white uppercase tracking-tight">Hệ Thống Giám Sát Camera Lớp Học</h1>
           <div class="flex flex-wrap items-center gap-4 mt-2">
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-white/5 border border-white/10 text-xs font-bold text-slate-300">
                <School :size="14" class="text-blue-400" /> Tổng số phòng: {{ systemStats.total }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-xs font-bold text-emerald-400">
                <div class="h-2 w-2 rounded-full bg-emerald-400 shadow-[0_0_8px_rgba(52,211,153,0.8)]"></div> Đang học: {{ systemStats.active }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-500/10 border border-amber-500/20 text-xs font-bold text-amber-400">
                <div class="h-2 w-2 rounded-full bg-amber-400 shadow-[0_0_8px_rgba(245,158,11,0.8)]"></div> Đang thi: {{ systemStats.testing }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-slate-500/10 border border-slate-500/20 text-xs font-bold text-slate-400">
                <div class="h-2 w-2 rounded-full bg-slate-400 shadow-[0_0_8px_rgba(148,163,184,0.8)]"></div> Phòng trống: {{ systemStats.idle }}
              </div>
              <div class="flex items-center gap-1.5 px-3 py-1 rounded-full bg-rose-500/10 border border-rose-500/20 text-xs font-bold text-rose-400">
                <div class="h-2 w-2 rounded-full bg-rose-400 shadow-[0_0_8px_rgba(251,113,133,0.8)]"></div> Mất kết nối: {{ systemStats.disconnected }}
              </div>
           </div>
        </div>
      </div>
      
      <div class="relative z-10 flex items-center gap-4 bg-black/40 border border-white/10 px-6 py-3 rounded-2xl shadow-2xl backdrop-blur-xl">
         <Clock :size="24" class="text-blue-400 animate-pulse" />
         <div class="flex flex-col items-end">
            <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Thời gian thực tế</span>
            <span class="text-3xl font-black font-mono leading-none text-white tracking-wider drop-shadow-[0_0_8px_rgba(255,255,255,0.5)]">{{ currentTime }}</span>
         </div>
      </div>
    </div>

    <!-- ── CCTV GRID ── -->
    <div class="flex-1 overflow-y-auto custom-scrollbar pr-2 min-h-0">
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-4 gap-6 pb-6">
        <div v-for="(room, idx) in classrooms" :key="room.id" 
             class="group relative rounded-2xl bg-black border border-slate-800 overflow-hidden aspect-video shadow-lg cursor-pointer hover:ring-2 hover:ring-blue-500 hover:shadow-blue-500/20 transition-all flex flex-col justify-center items-center"
             @click="openZoom(idx)">
          
          <!-- Simulated Camera Feed based on Room Status -->
          <div class="absolute inset-0 w-full h-full pointer-events-none group-hover:scale-[1.02] transition-transform duration-700">
            <!-- Grid lines background -->
            <div class="absolute inset-0 bg-[linear-gradient(to_right,#1e293b_1px,transparent_1px),linear-gradient(to_bottom,#1e293b_1px,transparent_1px)] bg-[size:16px_16px] opacity-25"></div>
            
            <!-- If disconnected, render static lines and warning -->
            <div v-if="room.status === 'Disconnected'" class="absolute inset-0 bg-slate-900/90 flex flex-col items-center justify-center z-10">
               <WifiOff :size="36" class="text-rose-500 mb-2 animate-bounce-slow" />
               <span class="text-xs font-black text-rose-500 uppercase tracking-widest bg-black/50 px-3 py-1 rounded-full border border-rose-500/20">NO SIGNAL</span>
               <!-- Static noise screen effect -->
               <div class="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0)_50%,rgba(0,0,0,0.15)_50%)] bg-[length:100%_4px] pointer-events-none"></div>
            </div>

            <!-- If idle, render school building placeholder -->
            <div v-else-if="room.status === 'Idle'" class="absolute inset-0 bg-slate-950 flex flex-col items-center justify-center z-10">
               <School :size="40" class="text-slate-600 mb-2" />
               <span class="text-xs font-bold text-slate-500 uppercase tracking-wider">PHÒNG TRỐNG</span>
            </div>

            <!-- If active/testing, render real camera image feed -->
            <div v-else class="w-full h-full relative">
               <img :src="room.status === 'Testing' ? cctvExamImg : cctvActiveImg" 
                    alt="Classroom Camera Feed" 
                    class="w-full h-full object-cover opacity-60 filter brightness-90 contrast-125 saturate-50" />
               
               <!-- Camera vignette and scanline overlays to make it look realistic -->
               <div class="absolute inset-0 bg-[radial-gradient(circle_at_center,transparent_30%,rgba(0,0,0,0.6)_100%)] pointer-events-none"></div>
               <div class="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0)_50%,rgba(0,0,0,0.12)_50%)] bg-[length:100%_6px] opacity-40 pointer-events-none"></div>

               <!-- Live HUD -->
               <div class="absolute top-11 left-3 right-3 flex justify-between text-[9px] font-mono text-emerald-400 bg-black/50 px-2 py-0.5 rounded border border-white/5">
                  <span class="flex items-center gap-1.5">
                     <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                     LIVE • CAM_01
                  </span>
                  <span>1080P @ 30FPS</span>
               </div>
            </div>

            <!-- Standard scanline overlay -->
            <div class="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0)_50%,rgba(0,0,0,0.06)_50%)] bg-[length:100%_4px] pointer-events-none"></div>
          </div>

          <!-- Overlay Info (Top) -->
          <div class="absolute top-0 left-0 right-0 p-3.5 flex items-start justify-between bg-gradient-to-b from-black/90 via-black/60 to-transparent z-20">
             <div>
                <div class="flex items-center gap-2">
                   <p class="text-sm font-black text-white drop-shadow-md">{{ room.roomName }}</p>
                   <span class="px-1.5 py-0.5 rounded bg-slate-800 text-[9px] font-bold text-slate-300 border border-slate-700">{{ room.type }}</span>
                </div>
                <p v-if="room.status !== 'Idle'" class="text-[10px] text-slate-300 font-medium truncate max-w-[200px] mt-0.5">
                   Lớp: {{ room.className }} • {{ room.subject }}
                </p>
                <p v-else class="text-[10px] text-slate-400 italic mt-0.5">Không có lịch học</p>
             </div>
             <div class="flex items-center gap-1.5">
                <span v-if="room.status !== 'Idle' && room.status !== 'Disconnected'" class="text-[10px] font-mono font-bold bg-black/60 border border-slate-700 text-slate-300 px-2 py-0.5 rounded-full">
                  {{ room.studentsCount }} SV
                </span>
                <div :class="['h-2 w-2 rounded-full shadow-[0_0_5px_currentColor]', getStatusColor(room.status)]" :title="getStatusLabel(room.status)"></div>
             </div>
          </div>

          <!-- Warnings Badge (Bottom Left) -->
          <div v-if="room.warnings.length > 0 && room.status !== 'Disconnected'" class="absolute bottom-3 left-3 z-20 flex items-center gap-1 bg-amber-500/20 text-amber-400 border border-amber-500/30 rounded-lg px-2.5 py-1 text-[10px] font-bold shadow-[0_0_8px_rgba(245,158,11,0.2)] animate-pulse-soft">
             <AlertTriangle :size="12" />
             {{ room.warnings.length }} Cảnh báo AI
          </div>
          
          <div class="absolute bottom-3 right-3 z-20 opacity-0 group-hover:opacity-100 transition-opacity bg-black/60 backdrop-blur border border-white/10 text-white rounded-lg p-2 shadow-xl">
             <Maximize2 :size="14" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── ZOOM MODAL (FULL SCREEN OVERLAY) ── -->
    <Teleport to="body">
      <Transition name="fade-scale">
        <div v-if="isZoomOpen" class="fixed inset-0 z-[100] flex bg-slate-950/95 backdrop-blur-xl">
          
          <!-- Left: Big Camera View -->
          <div class="flex-1 flex flex-col p-6 relative">
             <!-- Modal Header -->
             <div class="flex items-center justify-between mb-4">
                <div class="flex items-center gap-4">
                   <button @click="closeZoom" class="h-10 w-10 flex items-center justify-center rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors border border-white/10">
                      <ArrowLeft :size="20" />
                   </button>
                   <div>
                      <h2 class="text-xl font-black text-white flex items-center gap-3">
                         {{ zoomedClassroom.roomName }}
                         <span class="px-2 py-0.5 rounded bg-slate-800 text-xs font-bold text-slate-300 border border-slate-700">{{ zoomedClassroom.type }}</span>
                         <div :class="['h-3 w-3 rounded-full shadow-[0_0_10px_currentColor]', getStatusColor(zoomedClassroom.status)]"></div>
                      </h2>
                      <p class="text-sm text-slate-400 mt-1">
                         <span v-if="zoomedClassroom.status !== 'Idle'">
                           Lớp: <strong class="text-white">{{ zoomedClassroom.className }}</strong> • Môn học: <strong class="text-white">{{ zoomedClassroom.subject }}</strong> • GV: <strong class="text-white">{{ zoomedClassroom.lecturer }}</strong>
                         </span>
                         <span v-else class="italic text-slate-500">Phòng học hiện đang trống và thiết bị đang ở trạng thái nghỉ.</span>
                      </p>
                   </div>
                </div>
                
                <!-- Status & Alerts Badge -->
                <div class="flex items-center gap-3">
                   <div v-if="zoomedClassroom.warnings.length > 0 && zoomedClassroom.status !== 'Disconnected'" class="flex items-center gap-2 bg-amber-500/20 text-amber-400 px-4 py-2 rounded-full border border-amber-500/30">
                      <AlertTriangle :size="16" />
                      <span class="text-xs font-bold uppercase tracking-widest">{{ zoomedClassroom.warnings.length }} Cảnh báo AI</span>
                   </div>
                   <div class="bg-slate-800 border border-slate-700 text-slate-300 px-4 py-2 rounded-full text-xs font-bold font-mono">
                      Sĩ số: {{ zoomedClassroom.studentsCount }}
                   </div>
                </div>
             </div>

             <!-- Big Camera Feed -->
             <div class="flex-1 bg-black rounded-[32px] border border-slate-800 shadow-2xl relative overflow-hidden flex flex-col group">
                <!-- Camera Switcher Tabs -->
                <div class="absolute top-4 left-4 z-20 flex gap-2 bg-black/60 backdrop-blur border border-white/10 p-1 rounded-xl">
                   <button v-for="angle in ['CAM-01', 'CAM-02', 'PC-GV']" :key="angle"
                           @click="activeAngle = angle"
                           :class="[
                             'px-3 py-1.5 rounded-lg text-xs font-bold transition-all',
                             activeAngle === angle 
                               ? 'bg-blue-600 text-white shadow' 
                               : 'text-slate-400 hover:text-white'
                           ]">
                      {{ angle === 'CAM-01' ? 'Góc trước (CAM-01)' : angle === 'CAM-02' ? 'Góc sau (CAM-02)' : 'Màn hình GV' }}
                   </button>
                </div>

                <!-- Simulation Feed Render -->
                <div class="flex-1 flex items-center justify-center relative">
                   
                   <!-- If Disconnected -->
                   <div v-if="zoomedClassroom.status === 'Disconnected'" class="absolute inset-0 flex flex-col items-center justify-center bg-slate-950 z-10">
                      <div class="flex flex-col items-center animate-pulse">
                         <WifiOff :size="64" class="text-rose-500 mb-4" />
                         <span class="text-xl font-black text-rose-500 uppercase tracking-widest">Mất kết nối camera</span>
                         <p class="text-slate-500 text-xs font-mono mt-2">NO VIDEO SIGNAL RECEIVED FROM CHANNEL {{ activeAngle }}</p>
                      </div>
                      <div class="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0)_50%,rgba(0,0,0,0.15)_50%)] bg-[length:100%_4px] pointer-events-none"></div>
                   </div>

                   <!-- If Idle -->
                   <div v-else-if="zoomedClassroom.status === 'Idle'" class="absolute inset-0 flex flex-col items-center justify-center bg-slate-950 z-10">
                      <School :size="80" class="text-slate-700 mb-4" />
                      <span class="text-lg font-bold text-slate-500 uppercase tracking-wider">PHÒNG TRỐNG - THIẾT BỊ TẠM NGHỈ</span>
                      <p class="text-slate-600 text-xs font-mono mt-2">STANDBY MODE ACTIVE • PRESS BROADCAST TO ACTIVATE OVERRIDE</p>
                   </div>

                   <!-- Camera Visual Simulation -->
                   <div v-else class="w-full h-full relative flex items-center justify-center">
                       <!-- Render real camera image for CAM-01 and CAM-02 -->
                       <img v-if="activeAngle === 'CAM-01'" 
                            :src="zoomedClassroom.status === 'Testing' ? cctvExamImg : cctvActiveImg" 
                            class="w-full h-full object-cover opacity-70 filter brightness-90 contrast-125 saturate-50" />
                       
                       <img v-else-if="activeAngle === 'CAM-02'" 
                            :src="zoomedClassroom.status === 'Testing' ? cctvExamImg : cctvActiveImg" 
                            class="w-full h-full object-cover opacity-70 filter brightness-90 contrast-125 saturate-50 scale-x-[-1]" /> <!-- flipped horizontally to simulate back angle -->
                       
                       <!-- For Lecturer PC Screen, we keep the monitor layout because it represents a screen output, not a physical CCTV angle. That is very professional! -->
                       <div v-else class="w-[80%] max-w-lg aspect-video bg-slate-950 border border-slate-800 rounded-xl p-4 flex flex-col justify-between z-10 shadow-2xl relative">
                          <div class="border-b border-slate-800 pb-2 flex justify-between items-center">
                             <span class="text-[9px] font-mono text-slate-400">Giảng Viên PC: 192.168.4.120</span>
                             <span class="h-2 w-2 rounded-full bg-emerald-500"></span>
                          </div>
                          <div class="flex-1 flex flex-col justify-center items-center gap-2">
                             <Monitor :size="40" class="text-blue-500" />
                             <span class="text-[10px] font-mono text-slate-300">Đang truyền tải slide bài học</span>
                          </div>
                          <div class="text-[8px] font-mono text-slate-500 text-right">LMS Teacher App v2.4</div>
                       </div>

                       <!-- Camera vignette and scanline overlays to make it look realistic -->
                       <div v-if="activeAngle !== 'PC-GV'" class="absolute inset-0 bg-[radial-gradient(circle_at_center,transparent_30%,rgba(0,0,0,0.6)_100%)] pointer-events-none"></div>
                       <div v-if="activeAngle !== 'PC-GV'" class="absolute inset-0 bg-[linear-gradient(rgba(255,255,255,0)_50%,rgba(0,0,0,0.12)_50%)] bg-[length:100%_6px] opacity-40 pointer-events-none"></div>
                   </div>

                   <!-- Standard HUD Overlay Info -->
                   <div class="absolute bottom-6 right-6 flex gap-2 z-20">
                      <div class="px-4 py-2 bg-black/75 backdrop-blur rounded-xl text-white font-mono text-xs border border-white/10 flex items-center gap-2">
                        <span class="h-2.5 w-2.5 rounded-full bg-rose-500 animate-pulse"></span>
                        REC • {{ activeAngle }}
                      </div>
                      <div class="px-4 py-2 bg-black/75 backdrop-blur rounded-xl text-white font-mono text-xs border border-white/10">
                        1080P @ 30FPS
                      </div>
                   </div>

                   <!-- Navigation Controls -->
                   <button @click.stop="prevClassroom" class="absolute left-6 h-14 w-14 rounded-full bg-black/40 hover:bg-black/80 backdrop-blur border border-white/10 text-white flex items-center justify-center transition-all opacity-0 group-hover:opacity-100 hover:scale-110 z-20">
                      <ChevronLeft :size="32" />
                   </button>
                   <button @click.stop="nextClassroom" class="absolute right-6 h-14 w-14 rounded-full bg-black/40 hover:bg-black/80 backdrop-blur border border-white/10 text-white flex items-center justify-center transition-all opacity-0 group-hover:opacity-100 hover:scale-110 z-20">
                      <ChevronRight :size="32" />
                   </button>
                </div>
             </div>
          </div>

          <!-- Right: Details & Actions Sidebar -->
          <div class="w-[400px] border-l border-slate-800 bg-slate-900 flex flex-col shadow-2xl relative">
             <!-- Logs -->
             <div class="flex-1 flex flex-col p-6 overflow-hidden border-b border-slate-800">
                <div class="flex items-center justify-between mb-4">
                   <div class="flex items-center gap-2">
                      <Terminal :size="18" class="text-blue-400" />
                      <span class="font-black text-sm text-slate-300 uppercase tracking-widest text-[11px]">Nhật ký giám sát AI & Thiết bị</span>
                   </div>
                   <button class="text-slate-500 hover:text-white transition-colors" title="Làm mới"><RefreshCw :size="14" /></button>
                </div>
                <div class="flex-1 overflow-y-auto space-y-3 custom-scrollbar-dark pr-2">
                   <div v-for="(log, i) in currentLogs" :key="i" class="flex gap-3 text-xs font-mono">
                      <span class="text-slate-500 shrink-0">[{{ log.time }}]</span>
                      <span :class="log.type === 'error' ? 'text-rose-400 font-bold' : log.type === 'warning' ? 'text-amber-400' : 'text-emerald-400'">
                        {{ log.type === 'error' ? '❌' : log.type === 'warning' ? '⚠️' : '▶' }} {{ log.msg }}
                      </span>
                   </div>
                   <div class="animate-pulse text-emerald-500/50">_</div>
                </div>
             </div>

             <!-- Actions -->
             <div class="p-6 space-y-4">
                <h3 class="text-xs font-black text-slate-500 uppercase tracking-widest mb-2">Hành động của Giám thị / Giáo vụ</h3>
                
                <button @click="contactLecturer"
                        class="w-full flex items-center justify-center gap-3 rounded-[16px] bg-amber-500/10 text-amber-400 p-4 font-bold border border-amber-500/20 hover:bg-amber-500 hover:text-black transition-all">
                   <ShieldAlert :size="20" /> Liên hệ Giảng viên
                </button>

                <!-- Speaker Broadcast Announcement -->
                <div class="relative">
                   <input type="text" 
                          v-model="broadcastText" 
                          @keyup.enter="sendBroadcast"
                          placeholder="Phát âm thanh thông báo đến phòng..." 
                          class="w-full rounded-[16px] border border-slate-700 bg-black/50 pl-5 pr-12 py-4 text-sm text-white placeholder-slate-500 outline-none focus:border-blue-500 focus:bg-slate-800 transition-all" />
                   <button @click="sendBroadcast" class="absolute right-2 top-2 bottom-2 rounded-[12px] bg-blue-600 px-4 text-white hover:bg-blue-500 transition-colors">
                      <Volume2 :size="16" />
                   </button>
                </div>

                <div class="pt-2 flex gap-3">
                   <button @click="reportIncident" class="flex-1 flex items-center justify-center gap-2 rounded-[16px] bg-slate-800 p-4 text-slate-300 font-bold hover:bg-slate-700 transition-all border border-slate-700 text-xs">
                      <XCircle :size="16" class="text-rose-500" /> Báo sự cố thiết bị
                   </button>
                   <button @click="takeSnapshot" class="flex-1 flex items-center justify-center gap-2 rounded-[16px] bg-blue-500/10 p-4 text-blue-400 font-bold hover:bg-blue-600 hover:text-white transition-all border border-blue-500/20 text-xs">
                      <Camera :size="16" /> Chụp màn hình
                   </button>
                </div>
             </div>
          </div>
          
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { 
  Monitor, WifiOff, AlertTriangle, RefreshCw, 
  Clock, ShieldAlert, XCircle, 
  ChevronRight, ChevronLeft, Terminal,
  Maximize2, ArrowLeft, Video, School, Volume2, Camera
} from 'lucide-vue-next'

import cctvActiveImg from '@/assets/cctv_active.png'
import cctvExamImg from '@/assets/cctv_exam.png'

const currentTime = ref('')
let timer = null

onMounted(() => {
  updateTime()
  timer = setInterval(updateTime, 1000)
})

onUnmounted(() => {
  if (timer) clearInterval(timer)
})

function updateTime() {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

const classrooms = ref([
  { 
    id: 'PM401', 
    roomName: 'Phòng Máy 401', 
    className: 'D19_PM01', 
    subject: 'Lập trình Web (Thi học kỳ)', 
    lecturer: 'ThS. Nguyễn Minh Khoa', 
    studentsCount: '38/40', 
    status: 'Testing', 
    violations: 0, 
    warnings: [],
    type: 'Phòng Máy'
  },
  { 
    id: 'H201', 
    roomName: 'Phòng Học H201', 
    className: 'D20_KH02', 
    subject: 'Mạng máy tính (Lý thuyết)', 
    lecturer: 'TS. Lê Hoàng C', 
    studentsCount: '35/35', 
    status: 'Active', 
    violations: 1, 
    warnings: ['Phát hiện tiếng ồn vượt mức (65dB)'],
    type: 'Phòng Lý thuyết'
  },
  { 
    id: 'PM402', 
    roomName: 'Phòng Máy 402', 
    className: 'D19_PM02', 
    subject: 'Phát triển ứng dụng di động (Thi)', 
    lecturer: 'ThS. Phạm Minh D', 
    studentsCount: '28/30', 
    status: 'Testing', 
    violations: 2, 
    warnings: ['Không phát hiện giảng viên đứng lớp', 'Màn hình máy chiếu phụ mất kết nối'],
    type: 'Phòng Máy'
  },
  { 
    id: 'H305', 
    roomName: 'Phòng Học H305', 
    className: 'Chưa có lớp học', 
    subject: 'Trống', 
    lecturer: 'Không có', 
    studentsCount: '0/0', 
    status: 'Idle', 
    violations: 0, 
    warnings: [],
    type: 'Phòng Lý thuyết'
  },
  { 
    id: 'PM303', 
    roomName: 'Phòng Máy 303', 
    className: 'D21_PM03', 
    subject: 'Cơ sở dữ liệu (Thực hành)', 
    lecturer: 'ThS. Hoàng Văn E', 
    studentsCount: '42/45', 
    status: 'Disconnected', 
    violations: 1, 
    warnings: ['Mất tín hiệu camera chính (Cam-01)'],
    type: 'Phòng Máy'
  },
  { 
    id: 'H102', 
    roomName: 'Phòng Học H102', 
    className: 'D20_PM04', 
    subject: 'Cấu trúc dữ liệu và giải thuật', 
    lecturer: 'ThS. Đặng Mai G', 
    studentsCount: '39/40', 
    status: 'Active', 
    violations: 0, 
    warnings: [],
    type: 'Phòng Lý thuyết'
  },
  { 
    id: 'PM405', 
    roomName: 'Phòng Máy 405', 
    className: 'D19_KH03', 
    subject: 'An toàn thông tin', 
    lecturer: 'ThS. Võ Minh H', 
    studentsCount: '24/25', 
    status: 'Active', 
    violations: 0, 
    warnings: [],
    type: 'Phòng Máy'
  },
  { 
    id: 'H401', 
    roomName: 'Phòng Học H401', 
    className: 'Chưa có lớp học', 
    subject: 'Trống', 
    lecturer: 'Không có', 
    studentsCount: '0/0', 
    status: 'Idle', 
    violations: 0, 
    warnings: [],
    type: 'Phòng Lý thuyết'
  }
])

const isZoomOpen = ref(false)
const zoomedIndex = ref(0)
const activeAngle = ref('CAM-01')
const broadcastText = ref('')

const zoomedClassroom = computed(() => {
  return classrooms.value[zoomedIndex.value] || classrooms.value[0]
})

const systemStats = computed(() => {
  const total = classrooms.value.length
  const testing = classrooms.value.filter(c => c.status === 'Testing').length
  const active = classrooms.value.filter(c => c.status === 'Active').length
  const idle = classrooms.value.filter(c => c.status === 'Idle').length
  const disconnected = classrooms.value.filter(c => c.status === 'Disconnected').length
  const warnings = classrooms.value.filter(c => c.violations > 0 || c.warnings.length > 0).length
  return { total, testing, active, idle, disconnected, warnings }
})

const getLogsForClassroom = (classroom) => {
  if (!classroom) return []
  const logsList = [
    { time: '12:30:15', msg: `Hệ thống kết nối camera tại ${classroom.roomName} thành công.`, type: 'info' }
  ]
  
  if (classroom.status === 'Disconnected') {
    logsList.push({ time: '12:35:44', msg: 'Cảnh báo: Mất tín hiệu truyền tải video từ Camera chính.', type: 'error' })
    logsList.push({ time: '12:40:00', msg: 'Yêu cầu kiểm tra kỹ thuật tự động được kích hoạt.', type: 'warning' })
  } else if (classroom.status === 'Idle') {
    logsList.push({ time: '12:31:00', msg: 'Không ghi nhận lịch học/thi trong ca học hiện tại.', type: 'info' })
    logsList.push({ time: '12:32:00', msg: 'Thiết bị camera chuyển sang chế độ Standby tiết kiệm điện.', type: 'info' })
  } else {
    logsList.push({ time: '12:35:10', msg: `Giảng viên ${classroom.lecturer} đã đăng nhập máy trạm giảng dạy.`, type: 'info' })
    logsList.push({ time: '12:40:30', msg: `Nhận diện số lượng sinh viên tại chỗ: ${classroom.studentsCount}`, type: 'info' })
    
    if (classroom.status === 'Testing') {
      logsList.push({ time: '13:00:00', msg: `Kích hoạt chế độ giám sát phòng thi nghiêm ngặt.`, type: 'info' })
    }
    
    if (classroom.warnings.length > 0) {
      classroom.warnings.forEach((w, idx) => {
        logsList.push({ time: `13:${15 + idx * 5}:22`, msg: `CẢNH BÁO AI: ${w}`, type: 'warning' })
      })
    } else {
      logsList.push({ time: '13:20:00', msg: 'Hành vi phòng học ổn định. Không phát hiện bất thường.', type: 'info' })
    }
  }
  
  return logsList
}

const currentLogs = computed(() => {
  return getLogsForClassroom(zoomedClassroom.value)
})

function getStatusColor(status) {
  switch (status) {
    case 'Active': return 'text-emerald-500 bg-emerald-500'
    case 'Testing': return 'text-amber-500 bg-amber-500'
    case 'Idle': return 'text-slate-500 bg-slate-500'
    case 'Disconnected': return 'text-rose-500 bg-rose-500'
    default: return 'text-slate-500 bg-slate-500'
  }
}

function getStatusLabel(status) {
  switch (status) {
    case 'Active': return 'Đang học'
    case 'Testing': return 'Đang thi'
    case 'Idle': return 'Phòng trống'
    case 'Disconnected': return 'Mất kết nối'
    default: return 'Không xác định'
  }
}

function openZoom(index) {
  zoomedIndex.value = index
  activeAngle.value = 'CAM-01'
  isZoomOpen.value = true
  document.body.style.overflow = 'hidden'
}

function closeZoom() {
  isZoomOpen.value = false
  document.body.style.overflow = ''
}

function nextClassroom() {
  if (zoomedIndex.value < classrooms.value.length - 1) {
    zoomedIndex.value++
  } else {
    zoomedIndex.value = 0
  }
}

function prevClassroom() {
  if (zoomedIndex.value > 0) {
    zoomedIndex.value--
  } else {
    zoomedIndex.value = classrooms.value.length - 1
  }
}

function contactLecturer() {
  alert(`Đã gửi thông báo yêu cầu hỗ trợ đến giảng viên ${zoomedClassroom.value.lecturer} tại ${zoomedClassroom.value.roomName}!`)
}

function sendBroadcast() {
  if (!broadcastText.value.trim()) return
  alert(`Đã phát thông báo giọng nói AI đến loa ${zoomedClassroom.value.roomName}: "${broadcastText.value}"`)
  broadcastText.value = ''
}

function reportIncident() {
  alert(`Đã tạo phiếu yêu cầu kỹ thuật hỗ trợ camera tại ${zoomedClassroom.value.roomName}. Mã sự cố: TKT-${zoomedClassroom.value.id}-${Math.floor(Math.random() * 900 + 100)}`)
}

function takeSnapshot() {
  alert(`Đã chụp ảnh màn hình từ camera ${activeAngle.value} của phòng ${zoomedClassroom.value.roomName} và lưu vào kho tài liệu giám sát.`)
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(148, 163, 184, 0.5);
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

@keyframes pulse-soft {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.8; transform: scale(1.03); }
}
.animate-pulse-soft {
  animation: pulse-soft 2s infinite ease-in-out;
}

@keyframes bounce-slow {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-6px); }
}
.animate-bounce-slow {
  animation: bounce-slow 3s infinite ease-in-out;
}

/* Modal Transition */
.fade-scale-enter-active,
.fade-scale-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.fade-scale-enter-from,
.fade-scale-leave-to {
  opacity: 0;
  transform: scale(0.96) translateY(20px);
}
</style>
