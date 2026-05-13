<script setup>
import { ref } from 'vue'
import { 
  Search, Users, CheckCircle, XCircle, BarChart3, 
  Download, Calendar, Filter 
} from 'lucide-vue-next'

const attendanceData = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', present: 12, absent: 1, percent: 92 },
  { id: 'SV16002', name: 'Trần Thị B', present: 10, absent: 3, percent: 76 },
  { id: 'SV16003', name: 'Lê Hoàng C', present: 13, absent: 0, percent: 100 },
  { id: 'SV16004', name: 'Phạm Minh D', present: 8, absent: 5, percent: 61 },
])

const totalSessions = 13
const avgAttendance = 82
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Chuyên cần lớp</h1>
        <p class="text-slate-500 mt-1">Theo dõi tỷ lệ tham gia lớp học của sinh viên lớp SE1601.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất Excel
        </button>
      </div>
    </div>

    <!-- Attendance Summary KPIs -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
         <div class="h-14 w-14 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center border border-blue-100">
            <Calendar :size="28" />
         </div>
         <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Tổng buổi học</p>
            <p class="text-3xl font-black text-slate-800">{{ totalSessions }}</p>
         </div>
      </div>
      
      <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
         <div class="h-14 w-14 rounded-2xl bg-rose-50 text-rose-600 flex items-center justify-center border border-rose-100">
            <XCircle :size="28" />
         </div>
         <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Trung bình vắng</p>
            <p class="text-3xl font-black text-slate-800">2.2 <span class="text-sm font-bold text-slate-400">/ buổi</span></p>
         </div>
      </div>

      <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
         <div class="h-14 w-14 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center border border-emerald-100">
            <BarChart3 :size="28" />
         </div>
         <div>
            <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Tỉ lệ chuyên cần</p>
            <p class="text-3xl font-black text-slate-800">{{ avgAttendance }}%</p>
         </div>
      </div>
    </div>

    <!-- Attendance Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="p-6 border-b border-slate-50 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex items-center gap-4">
           <h2 class="text-xl font-bold text-slate-800">Chi tiết chuyên cần</h2>
           <div class="flex items-center gap-2">
              <select class="rounded-xl border border-slate-100 bg-slate-50 px-3 py-1.5 text-xs font-bold outline-none">
                 <option>Tháng 5/2026</option>
                 <option>Tháng 4/2026</option>
              </select>
           </div>
        </div>
        <div class="relative w-full md:w-72">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input type="text" placeholder="Tìm sinh viên..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Sinh viên</th>
              <th class="px-6 py-5">Có mặt (Buổi)</th>
              <th class="px-6 py-5">Vắng (Buổi)</th>
              <th class="px-6 py-5">Phần trăm (%)</th>
              <th class="px-8 py-5 text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sv in attendanceData" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs">
                    {{ sv.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800">{{ sv.name }}</p>
                    <p class="text-[10px] text-slate-400">{{ sv.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2">
                   <CheckCircle :size="14" class="text-emerald-500" />
                   <span class="text-sm font-bold text-slate-700">{{ sv.present }} / {{ totalSessions }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2">
                   <XCircle :size="14" :class="sv.absent > 3 ? 'text-rose-500' : 'text-slate-300'" />
                   <span :class="['text-sm font-bold', sv.absent > 3 ? 'text-rose-500' : 'text-slate-700']">{{ sv.absent }} buổi</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-3">
                  <div class="flex-1 h-1.5 w-20 bg-slate-100 rounded-full overflow-hidden">
                    <div :class="['h-full rounded-full', sv.percent < 80 ? 'bg-amber-500' : 'bg-emerald-500']" :style="{ width: sv.percent + '%' }"></div>
                  </div>
                  <span class="text-xs font-bold text-slate-700">{{ sv.percent }}%</span>
                </div>
              </td>
              <td class="px-8 py-5 text-right">
                <span :class="['rounded-full px-3 py-1 text-[10px] font-bold uppercase tracking-wider', sv.percent < 80 ? 'bg-amber-50 text-amber-600' : 'bg-emerald-50 text-emerald-600']">
                  {{ sv.percent < 80 ? 'Cảnh báo' : 'Ổn định' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
