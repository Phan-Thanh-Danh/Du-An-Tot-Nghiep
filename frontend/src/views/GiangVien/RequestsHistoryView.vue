<script setup>
import { ref } from 'vue'
import { 
  Search, Calendar, Filter, CheckCircle, XCircle, 
  ArrowUpDown, Download, History, User, Clock, CheckSquare, XSquare
} from 'lucide-vue-next'

const history = ref([
  { id: 1, date: '12/05/2026', time: '14:30', student: 'Nguyễn Văn A', type: 'Xin vắng học', result: 'Approved' },
  { id: 2, date: '10/05/2026', time: '09:15', student: 'Trần Thị B', type: 'Phúc khảo điểm', result: 'Rejected' },
  { id: 3, date: '08/05/2026', time: '11:45', student: 'Lê Hoàng C', type: 'Xin học bù', result: 'Approved' },
  { id: 4, date: '05/05/2026', time: '16:20', student: 'Phạm Minh D', type: 'Xin nghỉ phép', result: 'Approved' },
])

const getStatusBadge = (status) => {
  return status === 'Approved' 
    ? 'bg-emerald-100 text-emerald-700 border-emerald-200'
    : 'bg-rose-100 text-rose-700 border-rose-200'
}

const getStatusText = (status) => {
  return status === 'Approved' ? 'Đã duyệt' : 'Từ chối'
}

const getStatusIcon = (status) => {
  return status === 'Approved' ? CheckCircle : XCircle
}

const getAvatarGradient = (id) => {
  const gradients = [
    'from-blue-100 to-blue-100 text-blue-700',
    'from-cyan-100 to-pink-100 text-cyan-700',
    'from-emerald-100 to-teal-100 text-emerald-700',
    'from-orange-100 to-amber-100 text-orange-700'
  ]
  return gradients[id % 4]
}
</script>

<template>
  <div class="space-y-8 pb-12 animate-fade-in">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-white p-4 rounded-[24px] shadow-sm border border-slate-100/60 backdrop-blur-xl relative overflow-hidden">
      <!-- Decorative background glow -->
      <div class="absolute -top-10 -right-10 w-40 h-40 bg-blue-500/10 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute -bottom-10 -left-10 w-40 h-40 bg-cyan-500/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="flex items-center gap-5 relative z-10">
        <div class="p-3 rounded-2xl bg-blue-50 text-blue-600 border border-blue-100 shadow-inner">
          <History :size="24" stroke-width="2" />
        </div>
        <div>
          <h1 class="text-xl font-black text-slate-800 tracking-tight">Lịch sử xử lý đơn</h1>
          <p class="text-sm text-slate-500 mt-1">Tra cứu lại các yêu cầu của sinh viên đã được giải quyết.</p>
        </div>
      </div>

      <div class="flex items-center gap-3 relative z-10">
        <button class="group relative px-5 py-2.5 rounded-2xl bg-white border border-slate-200 text-slate-700 font-bold text-sm hover:border-blue-300 hover:text-blue-600 hover:bg-blue-50 hover:shadow-lg hover:shadow-blue-500/10 transition-all duration-300 active:scale-95 flex items-center gap-2 overflow-hidden">
          <Download :size="18" class="group-hover:-translate-y-0.5 transition-transform duration-300" />
          <span class="relative z-10">Xuất báo cáo</span>
        </button>
      </div>
    </div>

    <!-- Stats & Filters Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-4 gap-4">
      <div class="lg:col-span-1 flex flex-col gap-4">
        <!-- Main Stats Card -->
        <div class="bg-gradient-to-br from-blue-600 to-cyan-600 p-4 md:p-8 rounded-2xl shadow-xl shadow-blue-500/20 text-white relative overflow-hidden group">
           <div class="absolute top-0 right-0 w-48 h-48 bg-white/10 rounded-full blur-3xl pointer-events-none group-hover:scale-110 transition-transform duration-700"></div>
           <div class="absolute -bottom-10 -left-10 w-32 h-32 bg-black/10 rounded-full blur-2xl pointer-events-none"></div>
           
           <div class="relative z-10 flex flex-col gap-4">
              <div>
                 <h3 class="text-[11px] font-black uppercase tracking-widest text-blue-200 mb-1">Thống kê tháng này</h3>
                 <p class="text-xl font-black">15 <span class="text-sm font-medium text-blue-200 ml-1">đơn</span></p>
              </div>

              <div class="space-y-4">
                 <div class="flex items-center justify-between p-3 rounded-2xl bg-white/10 border border-white/10 backdrop-blur-sm">
                    <div class="flex items-center gap-3">
                       <div class="p-2 rounded-xl bg-emerald-500/20 text-emerald-300 shadow-inner">
                         <CheckSquare :size="18" />
                       </div>
                       <span class="text-sm font-bold">Đã duyệt</span>
                    </div>
                    <span class="text-xl font-black">12</span>
                 </div>
                 
                 <div class="flex items-center justify-between p-3 rounded-2xl bg-white/10 border border-white/10 backdrop-blur-sm">
                    <div class="flex items-center gap-3">
                       <div class="p-2 rounded-xl bg-rose-500/20 text-rose-300 shadow-inner">
                         <XSquare :size="18" />
                       </div>
                       <span class="text-sm font-bold">Từ chối</span>
                    </div>
                    <span class="text-xl font-black">3</span>
                 </div>
              </div>
           </div>
        </div>
        
        <!-- Filter Summary Card (Optional/Extra aesthetic) -->
        <div class="bg-white p-4 rounded-[24px] border border-slate-100 shadow-sm">
           <h3 class="text-[11px] font-black uppercase tracking-widest text-slate-400 mb-4">Loại đơn phổ biến</h3>
           <div class="space-y-3">
              <div class="flex items-center justify-between">
                 <span class="text-sm font-semibold text-slate-600">Xin vắng học</span>
                 <span class="text-sm font-black text-slate-800">45%</span>
              </div>
              <div class="w-full bg-slate-100 rounded-full h-1.5 mb-2">
                 <div class="bg-blue-500 h-1.5 rounded-full" style="width: 45%"></div>
              </div>
              
              <div class="flex items-center justify-between mt-3">
                 <span class="text-sm font-semibold text-slate-600">Phúc khảo</span>
                 <span class="text-sm font-black text-slate-800">30%</span>
              </div>
              <div class="w-full bg-slate-100 rounded-full h-1.5 mb-2">
                 <div class="bg-cyan-500 h-1.5 rounded-full" style="width: 30%"></div>
              </div>
           </div>
        </div>
      </div>

      <div class="lg:col-span-3">
        <!-- History Table -->
        <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex flex-col h-full">
          <div class="p-4 border-b border-slate-100/80 flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-slate-50/30">
            <div class="flex items-center gap-3 w-full sm:w-auto">
              <select class="w-full sm:w-40 rounded-2xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-600 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm cursor-pointer hover:bg-slate-50">
                 <option>Tất cả trạng thái</option>
                 <option>Đã phê duyệt</option>
                 <option>Đã từ chối</option>
              </select>
              <button class="p-2.5 rounded-2xl border border-slate-200 bg-white text-slate-400 hover:text-blue-600 hover:border-blue-200 hover:bg-blue-50 transition-all shadow-sm">
                 <Filter :size="18" />
              </button>
            </div>
            
            <div class="relative w-full sm:w-72">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm sinh viên hoặc loại đơn..." class="w-full rounded-2xl border border-slate-200 bg-white pl-11 pr-4 py-2.5 text-sm outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
            </div>
          </div>
          
          <div class="overflow-x-auto">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-slate-50/80 text-[11px] font-bold uppercase tracking-widest text-slate-500">
                  <th class="px-5 py-5 border-b border-slate-100">
                     <div class="flex items-center gap-2 cursor-pointer hover:text-blue-600 transition-colors">Ngày xử lý <ArrowUpDown :size="14" /></div>
                  </th>
                  <th class="px-4 py-5 border-b border-slate-100">Sinh viên</th>
                  <th class="px-4 py-5 border-b border-slate-100">Loại đơn</th>
                  <th class="px-5 py-5 border-b border-slate-100 text-right">Kết quả</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100/80 bg-white">
                <tr v-for="item in history" :key="item.id" class="group hover:bg-slate-50/50 transition-colors duration-200 cursor-pointer">
                  <td class="px-5 py-5">
                    <div class="flex flex-col">
                      <span class="text-sm font-bold text-slate-700">{{ item.date }}</span>
                      <span class="text-[11px] font-medium text-slate-400 flex items-center gap-1 mt-0.5"><Clock :size="10" /> {{ item.time }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-5">
                    <div class="flex items-center gap-4">
                       <div class="h-10 w-10 rounded-[14px] bg-gradient-to-br flex items-center justify-center text-sm font-black shadow-inner group-hover:scale-105 transition-transform" :class="getAvatarGradient(item.id)">
                          {{ item.student.split(' ').pop()[0] }}
                       </div>
                       <span class="text-sm font-bold text-slate-800 group-hover:text-blue-600 transition-colors">{{ item.student }}</span>
                    </div>
                  </td>
                  <td class="px-4 py-5">
                     <span class="text-sm font-medium text-slate-600 bg-slate-50 px-3 py-1.5 rounded-xl border border-slate-100 group-hover:border-blue-100 group-hover:bg-blue-50/50 transition-colors">{{ item.type }}</span>
                  </td>
                  <td class="px-5 py-5 text-right">
                    <div class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl border text-[10px] font-bold uppercase tracking-wider" :class="getStatusBadge(item.result)">
                      <component :is="getStatusIcon(item.result)" :size="14" />
                      {{ getStatusText(item.result) }}
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <div class="p-4 border-t border-slate-100/80 bg-slate-50/50 flex flex-col sm:flex-row items-center justify-between text-xs text-slate-500 gap-4">
            <span>Hiển thị 1-{{ history.length }} trong số 15 kết quả</span>
            <div class="flex gap-1">
              <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50 disabled:opacity-50">Trước</button>
              <button class="px-3 py-1.5 rounded-lg border border-blue-200 bg-blue-50 text-blue-600 font-bold">1</button>
              <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50">2</button>
              <button class="px-3 py-1.5 rounded-lg border border-slate-200 bg-white hover:bg-slate-50">Sau</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
