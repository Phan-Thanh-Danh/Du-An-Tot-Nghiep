<script setup>
import { ref } from 'vue'
import { 
  Plus, 
  Search, 
  Filter, 
  Calendar as CalendarIcon, 
  MoreVertical, 
  ChevronLeft, 
  ChevronRight,
  Download,
  Upload,
  CheckCircle2,
  AlertCircle,
  Clock
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Filters & State ──────────────────────────────────────────
const semester = ref('Spring 2026')
const campus = ref('Cơ sở chính')
const viewMode = ref('Week') // Day, Week, Month

// ── Mock Time Slots ─────────────────────────────────────────
const timeSlots = [
  '07:30', '08:30', '09:30', '10:30', '11:30', 
  '12:30', '13:30', '14:30', '15:30', '16:30', '17:30'
]

const days = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'CN']

// ── Mock Schedule Data ──────────────────────────────────────
const schedules = ref([
  {
    id: 1,
    subject: 'Lập trình Java',
    class: 'SE1601',
    teacher: 'Nguyễn Văn A',
    room: 'P.302',
    day: 'Thứ 2',
    startTime: '07:30',
    endTime: '10:30',
    status: 'published',
    color: 'blue'
  },
  {
    id: 2,
    subject: 'Cấu trúc dữ liệu',
    class: 'SE1602',
    teacher: 'Trần Thị B',
    room: 'P.105',
    day: 'Thứ 3',
    startTime: '13:30',
    endTime: '15:30',
    status: 'draft',
    color: 'indigo'
  },
  {
    id: 3,
    subject: 'Web Frontend',
    class: 'SE1603',
    teacher: 'Lê Văn C',
    room: 'Lab 2',
    day: 'Thứ 4',
    startTime: '08:30',
    endTime: '11:30',
    status: 'pending',
    color: 'orange'
  }
])

const getStatusClass = (status) => {
  switch (status) {
    case 'published': return 'bg-green-500/10 text-green-600 border-green-200'
    case 'pending': return 'bg-amber-500/10 text-amber-600 border-amber-200'
    case 'draft': return 'bg-slate-500/10 text-slate-600 border-slate-200'
    default: return 'bg-slate-500/10 text-slate-600 border-slate-200'
  }
}

const colorMap = {
  blue: 'bg-blue-50 border-blue-200 text-blue-700',
  indigo: 'bg-indigo-50 border-indigo-200 text-indigo-700',
  orange: 'bg-orange-50 border-orange-200 text-orange-700',
}

function getSchedule(day, time) {
  return schedules.value.find(s => s.day === day && s.startTime === time)
}
</script>

<template>
  <PageContainer 
    title="Quản lý thời khóa biểu" 
    subtitle="Xếp lịch học, kéo thả và kiểm tra xung đột cho học kỳ hiện tại."
  >
    <template #actions>
      <div class="flex items-center gap-3">
        <button class="lg-button-secondary px-4 py-2 text-sm font-bold">
          <Download :size="16" /> Xuất Excel
        </button>
        <button class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20">
          <Plus :size="18" /> Tạo lịch mới
        </button>
      </div>
    </template>

    <div class="space-y-6">
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-4">
          <div class="flex items-center bg-white/50 rounded-xl p-1 border border-slate-100">
            <button 
              v-for="mode in ['Day', 'Week', 'Month']" 
              :key="mode"
              @click="viewMode = mode"
              :class="[
                'px-4 py-1.5 text-xs font-bold rounded-lg transition-all',
                viewMode === mode ? 'bg-white text-blue-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'
              ]"
            >
              {{ mode }}
            </button>
          </div>

          <div class="h-6 w-px bg-slate-200"></div>

          <div class="flex items-center gap-2">
            <button class="p-2 hover:bg-white rounded-lg text-slate-500 transition-colors">
              <ChevronLeft :size="20" />
            </button>
            <span class="text-sm font-bold text-slate-700">12/05 - 18/05, 2026</span>
            <button class="p-2 hover:bg-white rounded-lg text-slate-500 transition-colors">
              <ChevronRight :size="20" />
            </button>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <select v-model="semester" class="bg-white border border-slate-100 rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-2 focus:ring-blue-500/20">
            <option>Spring 2026</option>
            <option>Fall 2025</option>
          </select>
          <select v-model="campus" class="bg-white border border-slate-100 rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-2 focus:ring-blue-500/20">
            <option>Cơ sở chính</option>
            <option>Cơ sở phụ</option>
          </select>
          <button class="lg-icon-button bg-white border border-slate-100 p-2 text-slate-500">
            <Filter :size="18" />
          </button>
        </div>
      </div>

      <!-- ── Calendar Grid ── -->
      <div class="lg-glass-strong rounded-[32px] overflow-hidden border border-slate-100 shadow-sm">
        <div class="overflow-x-auto">
          <table class="w-full border-collapse">
            <thead>
              <tr class="bg-slate-50/50">
                <th class="p-4 border-b border-r border-slate-100 w-20 text-[10px] font-black text-slate-400 uppercase tracking-widest">Thời gian</th>
                <th v-for="day in days" :key="day" class="p-4 border-b border-r border-slate-100 min-w-[160px]">
                  <div class="text-center">
                    <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">{{ day }}</p>
                    <p class="text-lg font-bold text-slate-800">1{{ days.indexOf(day) + 2 }}</p>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="time in timeSlots" :key="time" class="group">
                <td class="p-4 border-b border-r border-slate-100 text-center">
                  <span class="text-xs font-bold text-slate-500">{{ time }}</span>
                </td>
                <td v-for="day in days" :key="day" class="border-b border-r border-slate-100 p-2 relative min-h-[100px] hover:bg-blue-50/30 transition-colors cursor-pointer">
                  <!-- Event Card -->
                  <div 
                    v-if="getSchedule(day, time)" 
                    :class="[
                      'p-3 rounded-2xl border shadow-sm transition-all hover:scale-[1.02] hover:shadow-md cursor-grab active:cursor-grabbing',
                      colorMap[getSchedule(day, time).color]
                    ]"
                  >
                    <div class="flex items-start justify-between">
                      <p class="text-[10px] font-black uppercase tracking-tighter opacity-70">{{ getSchedule(day, time).class }}</p>
                      <MoreVertical :size="14" class="opacity-40 cursor-pointer" />
                    </div>
                    <p class="text-sm font-black mt-1 leading-tight">{{ getSchedule(day, time).subject }}</p>
                    <div class="mt-3 flex items-center justify-between gap-2">
                       <div class="flex flex-col gap-0.5">
                          <span class="text-[10px] font-bold opacity-60">{{ getSchedule(day, time).teacher }}</span>
                          <span class="text-[10px] font-bold opacity-60">{{ getSchedule(day, time).room }}</span>
                       </div>
                       <span :class="['px-2 py-0.5 rounded-full text-[9px] font-black uppercase tracking-widest border', getStatusClass(getSchedule(day, time).status)]">
                         {{ getSchedule(day, time).status }}
                       </span>
                    </div>
                  </div>
                  
                  <!-- Empty state hint on hover -->
                  <div class="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                    <Plus :size="16" class="text-blue-300" />
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- ── Legend & Status ── -->
      <div class="flex flex-wrap items-center justify-between gap-6 px-4">
        <div class="flex items-center gap-6">
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-green-500"></span>
            <span class="text-xs font-bold text-slate-600">Đã công bố</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-amber-500"></span>
            <span class="text-xs font-bold text-slate-600">Chờ duyệt</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-slate-400"></span>
            <span class="text-xs font-bold text-slate-600">Bản nháp</span>
          </div>
        </div>

        <div class="flex items-center gap-4 text-xs font-bold text-slate-500">
          <div class="flex items-center gap-2">
            <AlertCircle :size="14" class="text-amber-500" />
            <span>2 xung đột cần xử lý</span>
          </div>
          <div class="h-4 w-px bg-slate-200"></div>
          <div class="flex items-center gap-2">
            <CheckCircle2 :size="14" class="text-green-500" />
            <span>Dữ liệu đã được lưu</span>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>
</template>

<style scoped>
/* Custom scrollbar for calendar */
.overflow-x-auto::-webkit-scrollbar {
  height: 6px;
}
.overflow-x-auto::-webkit-scrollbar-track {
  background: transparent;
}
.overflow-x-auto::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 99px;
}
</style>
