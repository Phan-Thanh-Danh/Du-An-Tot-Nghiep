<script setup>
import { ref } from 'vue'
import { 
  Search, Calendar, Filter, Download, 
  ChevronRight, ArrowUpDown, Clock, Users,
  CheckCircle2, AlertTriangle, BookOpen
} from 'lucide-vue-next'

const attendanceHistory = ref([
  { id: 1, date: '12/05/2026', className: 'SE1601 - Java', absences: 3, total: 30, time: '07:30', room: 'A201' },
  { id: 2, date: '11/05/2026', className: 'SS1402 - Web', absences: 5, total: 32, time: '12:30', room: 'B305' },
  { id: 3, date: '10/05/2026', className: 'SE1601 - Java', absences: 1, total: 30, time: '07:30', room: 'A201' },
  { id: 4, date: '08/05/2026', className: 'SA1709 - DB', absences: 0, total: 25, time: '09:45', room: 'C102' },
])
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-6 bg-white p-8 rounded-[32px] border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      <div class="absolute -right-32 -bottom-32 h-96 w-96 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-emerald-50 to-transparent rounded-full pointer-events-none" />
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white shadow-md shadow-emerald-200">
           <Calendar :size="32" />
        </div>
        <div>
          <h1 class="text-2xl md:text-3xl font-black text-slate-900 tracking-tight">Lịch sử điểm danh</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Xem lại nhật ký điểm danh của các buổi học đã diễn ra.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <button class="flex items-center gap-2 rounded-2xl bg-white px-5 py-3 border border-slate-200 shadow-sm hover:bg-slate-50 hover:text-blue-600 transition-colors font-bold text-sm text-slate-700">
            <Download :size="18" /> Xuất báo cáo
         </button>
      </div>
    </div>

    <!-- Quick Stats & Filters -->
    <div class="flex flex-col xl:flex-row gap-6">
       <!-- Stats -->
       <div class="grid grid-cols-2 md:grid-cols-4 xl:w-1/2 gap-4">
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 mb-3">
                <BookOpen :size="20" />
             </div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Tổng số buổi</p>
             <p class="text-2xl font-black text-slate-800">24</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-emerald-50 flex items-center justify-center text-emerald-600 mb-3">
                <CheckCircle2 :size="20" />
             </div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Đi học đúng</p>
             <p class="text-2xl font-black text-slate-800">92%</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm col-span-2 flex flex-col justify-center">
             <div class="flex items-center gap-3 mb-2">
                 <div class="h-10 w-10 rounded-xl bg-rose-50 flex items-center justify-center text-rose-600">
                    <AlertTriangle :size="20" />
                 </div>
                 <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Cần lưu ý</p>
             </div>
             <p class="text-sm font-bold text-slate-800 mt-1">Lớp SS1402 có tỷ lệ vắng cao (15%)</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 rounded-[32px] bg-white border border-slate-100 p-6 shadow-sm flex flex-col justify-center">
          <p class="text-sm font-bold text-slate-800 mb-4 flex items-center gap-2"><Filter :size="16" class="text-blue-500" /> Bộ lọc dữ liệu</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm kiếm lớp học, mã phòng..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
              <Calendar :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="date" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
            </div>
          </div>
       </div>
    </div>

    <!-- History Table -->
    <div class="rounded-[32px] border border-slate-100 bg-white shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50 border-b border-slate-100">
              <th class="px-8 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">
                 <div class="flex items-center gap-2 hover:text-blue-600 cursor-pointer transition-colors w-max">Ngày học <ArrowUpDown :size="14" /></div>
              </th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Lớp học</th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Thời gian</th>
              <th class="px-6 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-center">Tình trạng</th>
              <th class="px-8 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in attendanceHistory" :key="item.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-4">
                   <div class="h-12 w-12 rounded-2xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-500 group-hover:bg-blue-50 group-hover:text-blue-600 group-hover:border-blue-100 transition-colors shadow-sm">
                      <Calendar :size="20" />
                   </div>
                   <div>
                      <p class="text-sm font-bold text-slate-900">{{ item.date }}</p>
                      <p class="text-xs font-semibold text-slate-400 mt-0.5">Học kỳ Fall</p>
                   </div>
                </div>
              </td>
              <td class="px-6 py-5">
                <p class="text-sm font-bold text-slate-900 group-hover:text-blue-700 transition-colors">{{ item.className }}</p>
                <div class="flex items-center gap-1.5 mt-1">
                   <span class="rounded bg-slate-100 px-1.5 py-0.5 text-[10px] font-bold text-slate-500 uppercase">Phòng</span>
                   <span class="text-xs font-bold text-slate-600">{{ item.room }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2 text-sm text-slate-600 font-bold bg-slate-50 px-3 py-1.5 rounded-xl w-max border border-slate-100">
                   <Clock :size="14" class="text-blue-500" /> {{ item.time }}
                </div>
              </td>
              <td class="px-6 py-5 text-center">
                 <div v-if="item.absences === 0" class="inline-flex items-center gap-1.5 rounded-full bg-emerald-50 px-3 py-1.5 border border-emerald-100 text-emerald-700">
                    <CheckCircle2 :size="14" />
                    <span class="text-xs font-bold">Đầy đủ ({{item.total}})</span>
                 </div>
                 <div v-else class="inline-flex items-center gap-1.5 rounded-full bg-rose-50 px-3 py-1.5 border border-rose-100 text-rose-700">
                    <Users :size="14" />
                    <span class="text-xs font-bold">Vắng {{ item.absences }}/{{item.total}}</span>
                 </div>
              </td>
              <td class="px-8 py-5 text-right">
                <button class="h-10 w-10 inline-flex items-center justify-center rounded-xl border border-slate-200 bg-white text-slate-400 hover:text-blue-600 hover:border-blue-200 hover:bg-blue-50 transition-colors shadow-sm group-hover:shadow-md">
                   <ChevronRight :size="20" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="bg-slate-50/80 px-8 py-5 border-t border-slate-100 flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-widest">
            <Users :size="14" class="text-slate-400" /> Hiển thị {{ attendanceHistory.length }} bản ghi
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-blue-500 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
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
.animate-fade-in-up {
  animation: fade-in-up 0.3s ease-out forwards;
}
</style>
