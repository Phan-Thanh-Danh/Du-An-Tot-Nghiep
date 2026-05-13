<script setup>
import { ref } from 'vue'
import { 
  Search, Award, Clock, Download, Filter, 
  ExternalLink, ChevronRight, User 
} from 'lucide-vue-next'

const examResults = ref([
  { id: 1, studentId: 'SV16001', name: 'Nguyễn Văn A', score: 9.5, timeSpent: '45 min', date: '25/05/2026' },
  { id: 2, studentId: 'SV16002', name: 'Trần Thị B', score: 7.2, timeSpent: '82 min', date: '25/05/2026' },
  { id: 3, studentId: 'SV16003', name: 'Lê Hoàng C', score: 8.8, timeSpent: '60 min', date: '25/05/2026' },
  { id: 4, studentId: 'SV16004', name: 'Phạm Minh D', score: 4.5, timeSpent: '89 min', date: '25/05/2026' },
])
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Kết quả bài thi</h1>
        <p class="text-slate-500 mt-1">Danh sách điểm số và thống kê kết quả của kỳ thi vừa qua.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất kết quả
        </button>
      </div>
    </div>

    <!-- Filters -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4">
      <div class="relative flex-1">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm sinh viên bằng tên hoặc MSSV..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300 transition-all" />
      </div>
      <div class="flex gap-2">
         <select class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none">
            <option>Tất cả kỳ thi</option>
            <option>Thi giữa kỳ</option>
            <option>Thi cuối kỳ</option>
         </select>
         <button class="rounded-xl border border-slate-200 p-2.5 text-slate-400 hover:bg-slate-50 transition-colors">
            <Filter :size="18" />
         </button>
      </div>
    </div>

    <!-- Results Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="overflow-x-auto text-slate-800">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Sinh viên</th>
              <th class="px-6 py-5">Điểm số</th>
              <th class="px-6 py-5">Thời gian làm bài</th>
              <th class="px-6 py-5">Ngày thi</th>
              <th class="px-8 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="res in examResults" :key="res.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs">
                    {{ res.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800">{{ res.name }}</p>
                    <p class="text-[10px] text-slate-400 uppercase tracking-tighter">{{ res.studentId }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2">
                   <div class="h-8 w-8 rounded-lg bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100">
                      <Award :size="16" />
                   </div>
                   <span :class="['text-base font-black', res.score < 5 ? 'text-rose-500' : 'text-slate-800']">{{ res.score }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2 text-sm text-slate-500">
                   <Clock :size="14" class="text-slate-300" />
                   {{ res.timeSpent }}
                </div>
              </td>
              <td class="px-6 py-5 text-sm text-slate-500">{{ res.date }}</td>
              <td class="px-8 py-5 text-right">
                <button class="inline-flex items-center gap-2 rounded-xl bg-slate-100 px-4 py-2 text-[11px] font-black uppercase text-slate-500 hover:bg-indigo-600 hover:text-white transition-all shadow-sm">
                   Chi tiết <ChevronRight :size="14" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
