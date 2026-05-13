<script setup>
import { ref } from 'vue'
import { 
  Search, Calendar, Filter, Download, 
  ChevronRight, ArrowUpDown, Clock, Users 
} from 'lucide-vue-next'

const attendanceHistory = ref([
  { id: 1, date: '12/05/2026', className: 'SE1601 - Java', absences: 3, time: '07:30', room: 'A201' },
  { id: 2, date: '11/05/2026', className: 'SS1402 - Web', absences: 5, time: '12:30', room: 'B305' },
  { id: 3, date: '10/05/2026', className: 'SE1601 - Java', absences: 1, time: '07:30', room: 'A201' },
  { id: 4, date: '08/05/2026', className: 'SA1709 - DB', absences: 0, time: '09:45', room: 'C102' },
])
</script>

<template>
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Lịch sử điểm danh</h1>
        <p class="text-slate-500 mt-1">Xem lại nhật ký điểm danh của các buổi học đã diễn ra.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất báo cáo
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4 items-center">
      <div class="relative flex-1 w-full">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm theo tên lớp..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <input type="date" class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none" />
        <button class="rounded-xl border border-slate-200 p-2.5 text-slate-400 hover:bg-slate-50 transition-colors">
          <Filter :size="18" />
        </button>
      </div>
    </div>

    <!-- History Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">
                 <div class="flex items-center gap-2">Ngày học <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-6 py-5">Lớp học</th>
              <th class="px-6 py-5">Thời gian</th>
              <th class="px-6 py-5 text-center">Số SV vắng</th>
              <th class="px-8 py-5 text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in attendanceHistory" :key="item.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                   <div class="h-9 w-9 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-indigo-50 group-hover:text-indigo-600 transition-all">
                      <Calendar :size="18" />
                   </div>
                   <span class="text-sm font-bold text-slate-700">{{ item.date }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <p class="text-sm font-bold text-slate-800">{{ item.className }}</p>
                <p class="text-[10px] text-slate-400 font-bold uppercase tracking-widest mt-0.5">Phòng: {{ item.room }}</p>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2 text-sm text-slate-500 font-medium">
                   <Clock :size="14" /> {{ item.time }}
                </div>
              </td>
              <td class="px-6 py-5">
                 <div class="flex flex-col items-center">
                    <span :class="['text-sm font-black', item.absences > 3 ? 'text-rose-500' : 'text-slate-800']">
                       {{ item.absences }}
                    </span>
                    <div class="flex gap-0.5 mt-1">
                       <div v-for="i in 5" :key="i" :class="['h-1 w-2 rounded-full', i <= item.absences ? 'bg-rose-400' : 'bg-slate-100']"></div>
                    </div>
                 </div>
              </td>
              <td class="px-8 py-5 text-right">
                <button class="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-all">
                   <ChevronRight :size="18" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="bg-slate-50/50 px-8 py-4 border-t border-slate-50 text-[10px] font-bold text-slate-400 uppercase tracking-widest flex justify-between">
         <div class="flex items-center gap-2">
            <Users :size="12" /> Tổng số buổi đã điểm danh: {{ attendanceHistory.length }}
         </div>
         <span>LMS Academic Management System</span>
      </div>
    </div>
  </div>
</template>
