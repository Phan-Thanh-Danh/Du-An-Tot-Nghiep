<script setup>
import { ref, onMounted } from 'vue'
import { 
  Search, Users, CheckCircle, XCircle, BarChart3, 
  Download, Calendar, Filter, ArrowLeft, ArrowUpRight, TrendingUp, AlertCircle, UserCheck, CheckCircle2
} from 'lucide-vue-next'

const attendanceData = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', present: 12, absent: 1, percent: 92, status: 'good' },
  { id: 'SV16002', name: 'Trần Thị B', present: 10, absent: 3, percent: 76, status: 'warning' },
  { id: 'SV16003', name: 'Lê Hoàng C', present: 13, absent: 0, percent: 100, status: 'excellent' },
  { id: 'SV16004', name: 'Phạm Minh D', present: 8, absent: 5, percent: 61, status: 'danger' },
  { id: 'SV16005', name: 'Hoàng Hữu E', present: 13, absent: 0, percent: 100, status: 'excellent' },
  { id: 'SV16006', name: 'Vũ Thị F', present: 11, absent: 2, percent: 85, status: 'good' },
])

const totalSessions = 13
const avgAttendance = 82

const getStatusBadge = (status) => {
  const badges = {
    excellent: 'text-emerald-600 bg-emerald-50 border-emerald-100/50',
    good: 'text-blue-600 bg-blue-50 border-blue-100/50',
    warning: 'text-amber-600 bg-amber-50 border-amber-100/50',
    danger: 'text-rose-600 bg-rose-50 border-rose-100/50'
  }
  return badges[status] || badges.good
}

const getStatusText = (status) => {
  const texts = {
    excellent: 'Xuất sắc',
    good: 'Ổn định',
    warning: 'Cảnh báo',
    danger: 'Nguy hiểm'
  }
  return texts[status] || 'Ổn định'
}

const animateProgress = ref(false)
onMounted(() => {
  setTimeout(() => {
    animateProgress.value = true
  }, 100)
})
</script>

<template>
  <div class="space-y-8 pb-12 animate-fade-in text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <router-link to="/teacher/classes" class="h-10 w-10 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-500 flex items-center justify-center text-white shadow-md shadow-blue-200 hover:scale-105 transition-transform duration-300">
           <ArrowLeft :size="28" stroke-width="2.5" />
        </router-link>
        <div>
          <div class="flex items-center gap-3 mb-1.5">
            <span class="px-3 py-1 rounded-xl bg-blue-50 text-blue-600 text-[10px] font-black uppercase tracking-widest border border-blue-100/50">SE1601</span>
            <span class="px-3 py-1 rounded-xl bg-slate-50 text-slate-500 text-[10px] font-black uppercase tracking-widest border border-slate-200/50">HK 2 - 2026</span>
          </div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Chuyên cần lớp</h1>
        </div>
      </div>

      <div class="relative z-10 flex gap-3">
         <button class="flex items-center gap-2 rounded-2xl bg-white px-4 py-3 border border-slate-200 shadow-sm hover:bg-blue-50 hover:border-blue-200 hover:text-blue-600 transition-colors font-bold text-sm text-slate-700">
            <Download :size="18" /> Xuất báo cáo
         </button>
      </div>
    </div>

    <!-- Attendance Summary KPIs -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-50 to-cyan-50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-blue-50 text-blue-600 border border-blue-100/50 flex items-center justify-center shadow-inner group-hover:scale-110 transition-transform duration-300">
              <Calendar :size="28" stroke-width="2" />
           </div>
           <div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng buổi học</p>
              <div class="flex items-baseline gap-2">
                <p class="text-xl font-black text-slate-800">{{ totalSessions }}</p>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider bg-slate-50 px-2 py-1 rounded-lg border border-slate-100">Buổi</span>
              </div>
           </div>
        </div>
      </div>
      
      <div class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-cyan-50 to-blue-50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-cyan-50 text-cyan-600 border border-cyan-100/50 flex items-center justify-center shadow-inner group-hover:scale-110 transition-transform duration-300">
              <UserCheck :size="28" stroke-width="2" />
           </div>
           <div>
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Trung bình vắng</p>
              <div class="flex items-baseline gap-2">
                <p class="text-xl font-black text-slate-800">2.2</p>
                <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider bg-slate-50 px-2 py-1 rounded-lg border border-slate-100">Buổi</span>
              </div>
           </div>
        </div>
      </div>

      <div class="bg-white p-4 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-shadow relative overflow-hidden group">
        <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-50 to-cyan-50 rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
        <div class="flex items-center gap-5">
           <div class="h-10 w-10 rounded-2xl bg-blue-50 text-blue-600 border border-blue-100/50 flex items-center justify-center shadow-inner group-hover:scale-110 transition-transform duration-300">
              <BarChart3 :size="28" stroke-width="2" />
           </div>
           <div class="flex-1">
              <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tỉ lệ chuyên cần</p>
              <div class="flex items-baseline justify-between gap-2">
                <p class="text-xl font-black text-slate-800">{{ avgAttendance }}%</p>
              </div>
           </div>
        </div>
      </div>
    </div>

    <!-- Attendance Table -->
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col">
      <div class="p-4 md:p-5 border-b border-slate-100/80 flex flex-col md:flex-row md:items-center justify-between gap-4 bg-slate-50/30">
        <div class="flex items-center gap-4">
           <div>
             <h2 class="text-xl font-black text-slate-800">Chi tiết chuyên cần</h2>
             <p class="text-sm font-medium text-slate-500 mt-1">Danh sách theo dõi điểm danh từng sinh viên</p>
           </div>
        </div>
        <div class="flex flex-col sm:flex-row items-center gap-3">
          <select class="w-full sm:w-auto rounded-[20px] border border-slate-200 bg-white px-5 py-3.5 text-sm font-bold text-slate-600 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm appearance-none cursor-pointer">
             <option>Tháng 5/2026</option>
             <option>Tháng 4/2026</option>
             <option>Tháng 3/2026</option>
          </select>
          <div class="relative w-full md:w-72">
            <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
            <input type="text" placeholder="Tìm sinh viên, mã SV..." class="w-full rounded-[20px] border border-slate-200 bg-white pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
          </div>
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50 border-b border-slate-100">
              <th class="px-5 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Sinh viên</th>
              <th class="px-4 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Có mặt</th>
              <th class="px-4 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Vắng</th>
              <th class="px-4 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Tỷ lệ tham gia</th>
              <th class="px-5 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sv in attendanceData" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-5 py-5">
                <div class="flex items-center gap-4">
                  <div class="h-10 w-10 rounded-2xl bg-slate-50 border border-slate-100 flex items-center justify-center text-slate-500 font-black text-sm group-hover:bg-blue-100 group-hover:text-blue-600 group-hover:border-blue-200 transition-colors shadow-sm">
                    {{ sv.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-900 group-hover:text-blue-700 transition-colors">{{ sv.name }}</p>
                    <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ sv.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-2">
                   <div class="w-7 h-7 rounded-lg bg-blue-50 text-blue-500 flex items-center justify-center border border-blue-100/50">
                     <CheckCircle2 :size="14" />
                   </div>
                   <span class="text-sm font-bold text-slate-700">{{ sv.present }} / {{ totalSessions }}</span>
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-2">
                   <div class="w-7 h-7 rounded-lg flex items-center justify-center border" :class="sv.absent > 3 ? 'bg-rose-50 text-rose-500 border-rose-100/50' : 'bg-slate-50 text-slate-400 border-slate-100'">
                     <XCircle :size="14" />
                   </div>
                   <span :class="['text-sm font-bold', sv.absent > 3 ? 'text-rose-500' : 'text-slate-600']">{{ sv.absent }} buổi</span>
                </div>
              </td>
              <td class="px-4 py-5 w-56">
                <div class="flex items-center gap-3">
                  <div class="flex-1 h-2 bg-slate-100 rounded-full overflow-hidden shadow-inner">
                    <!-- Unified blue gradient for all progress bars -->
                    <div class="h-full rounded-full bg-gradient-to-r from-blue-400 to-cyan-400 transition-all duration-1000 ease-out" 
                         :style="{ width: animateProgress ? sv.percent + '%' : '0%' }"></div>
                  </div>
                  <span class="text-[11px] font-black text-slate-500 w-9">{{ sv.percent }}%</span>
                </div>
              </td>
              <td class="px-5 py-5 text-right">
                <div class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-[10px] font-black uppercase tracking-wider shadow-sm" :class="getStatusBadge(sv.status)">
                  <AlertCircle v-if="sv.status === 'danger' || sv.status === 'warning'" :size="12" />
                  <CheckCircle2 v-else :size="12" />
                  {{ getStatusText(sv.status) }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer Pagination -->
      <div class="bg-slate-50/80 px-5 py-5 border-t border-slate-100 flex items-center justify-between text-xs text-slate-500">
        <span class="font-bold uppercase tracking-widest text-[10px]">Hiển thị 1-{{ attendanceData.length }} trong số 42 sinh viên</span>
        <div class="flex gap-1.5">
          <button class="px-4 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 font-bold disabled:opacity-50 transition-colors">Trước</button>
          <button class="px-4 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 font-black shadow-sm">1</button>
          <button class="px-4 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 font-bold transition-colors">2</button>
          <button class="px-4 py-2 rounded-xl border border-slate-200 bg-white hover:bg-slate-50 font-bold transition-colors">Sau</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.4s ease-out forwards;
}
</style>
