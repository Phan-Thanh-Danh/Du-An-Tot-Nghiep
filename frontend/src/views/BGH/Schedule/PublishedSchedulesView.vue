<script setup>
import { ref } from 'vue'
import { 
  Calendar as CalendarIcon, 
  Search, 
  Filter, 
  Clock, 
  MapPin, 
  User, 
  Eye, 
  History, 
  ExternalLink,
  ChevronLeft,
  ChevronRight,
  Download
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const viewMode = ref('Week')
const schedules = ref([
  { id: 1, subject: 'Lập trình Mobile (React Native)', class: 'SE1601', teacher: 'Nguyễn Văn A', room: 'P.302', time: '07:30 - 10:30', day: 'Thứ 2', type: 'offline', status: 'published' },
  { id: 2, subject: 'Cơ sở dữ liệu NoSQL', class: 'SE1602', teacher: 'Trần Thị B', room: 'MS Teams', time: '13:30 - 15:30', day: 'Thứ 3', type: 'online', link: 'https://teams.microsoft.com/l/meetup-join/...', status: 'published' },
  { id: 3, subject: 'Web Frontend Advanced', class: 'SE1603', teacher: 'Lê Văn C', room: 'Lab 2', time: '08:30 - 11:30', day: 'Thứ 4', type: 'offline', status: 'published' },
])
</script>

<template>
  <PageContainer 
    title="Thời khóa biểu đã duyệt" 
    subtitle="Xem và theo dõi các bộ TKB đã được công bố chính thức cho sinh viên và giảng viên."
  >
    <template #actions>
      <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
         <Download :size="18" /> Xuất dữ liệu
      </button>
    </template>

    <div class="space-y-6">
      
      <!-- ── Calendar Toolbar ── -->
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
           <div class="relative">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm lớp, môn..." class="bg-white border border-slate-200 rounded-xl pl-9 pr-4 py-2 text-xs font-bold outline-none">
           </div>
           <button class="lg-icon-button bg-white border border-slate-200 p-2 text-slate-500">
              <Filter :size="18" />
           </button>
        </div>
      </div>

      <!-- ── Schedule List (Read-only for BGH) ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lớp & Môn học</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thời gian & Phòng</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Hình thức</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sch in schedules" :key="sch.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div :class="['h-9 w-9 rounded-xl flex items-center justify-center font-black text-[10px] uppercase shadow-sm', sch.type === 'online' ? 'bg-indigo-50 text-indigo-600 border border-indigo-100' : 'bg-blue-50 text-blue-600 border border-blue-100']">
                    {{ sch.class.slice(0, 2) }}
                  </div>
                  <div>
                    <p class="text-sm font-black text-slate-800 leading-tight">{{ sch.subject }}</p>
                    <p class="text-[11px] font-bold text-slate-400 mt-0.5">{{ sch.class }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                   <div class="h-6 w-6 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 overflow-hidden">
                      <User :size="14" />
                   </div>
                   <span class="text-xs font-bold text-slate-600">{{ sch.teacher }}</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex flex-col gap-1">
                   <div class="flex items-center gap-1.5 text-xs font-bold text-slate-600">
                      <Clock :size="12" class="text-slate-300" /> {{ sch.day }}, {{ sch.time }}
                   </div>
                   <div class="flex items-center gap-1.5 text-[10px] font-bold text-slate-400">
                      <MapPin :size="12" class="text-slate-300" /> {{ sch.room }}
                   </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2 py-0.5 rounded-lg text-[9px] font-black uppercase tracking-widest border', sch.type === 'online' ? 'bg-indigo-50 text-indigo-600 border-indigo-100' : 'bg-blue-50 text-blue-600 border-blue-100']">
                  {{ sch.type }}
                </span>
                <div v-if="sch.type === 'online'" class="mt-1 flex items-center gap-1 text-[9px] font-black text-indigo-500 uppercase cursor-pointer hover:underline">
                   <ExternalLink :size="10" /> Join Link
                </div>
              </td>
              <td class="px-6 py-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all" title="Xem chi tiết">
                    <Eye :size="18" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all" title="Lịch sử duyệt">
                    <History :size="18" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

    </div>
  </PageContainer>
</template>
