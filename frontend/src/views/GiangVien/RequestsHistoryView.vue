<script setup>
import { ref } from 'vue'
import { 
  Search, Calendar, Filter, CheckCircle, XCircle, 
  ArrowUpDown, Download, History, User 
} from 'lucide-vue-next'

const history = ref([
  { id: 1, date: '12/05/2026', student: 'Nguyễn Văn A', type: 'Xin vắng học', result: 'Approved' },
  { id: 2, date: '10/05/2026', student: 'Trần Thị B', type: 'Phúc khảo điểm', result: 'Rejected' },
  { id: 3, date: '08/05/2026', student: 'Lê Hoàng C', type: 'Xin học bù', result: 'Approved' },
  { id: 4, date: '05/05/2026', student: 'Phạm Minh D', type: 'Xin nghỉ phép', result: 'Approved' },
])
</script>

<template>
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Lịch sử xử lý đơn</h1>
        <p class="text-slate-500 mt-1">Tra cứu lại các yêu cầu của sinh viên đã được giải quyết.</p>
      </div>
      <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
        <Download :size="18" /> Xuất dữ liệu
      </button>
    </div>

    <!-- Filters -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4 items-center">
      <div class="relative flex-1 w-full">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm sinh viên hoặc loại đơn..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
      </div>
      <div class="flex gap-2 w-full md:w-auto">
         <select class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none">
            <option>Tất cả trạng thái</option>
            <option>Approved</option>
            <option>Rejected</option>
         </select>
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
                 <div class="flex items-center gap-2">Ngày xử lý <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-6 py-5">Sinh viên</th>
              <th class="px-6 py-5">Loại đơn</th>
              <th class="px-8 py-5 text-right">Kết quả</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in history" :key="item.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5 text-sm font-bold text-slate-500">{{ item.date }}</td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-3">
                   <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 text-[10px] font-bold">
                      {{ item.student.split(' ').pop()[0] }}
                   </div>
                   <span class="text-sm font-bold text-slate-800">{{ item.student }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                 <span class="text-sm font-medium text-slate-600">{{ item.type }}</span>
              </td>
              <td class="px-8 py-5 text-right">
                <div :class="['inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-[10px] font-black uppercase tracking-wider', item.result === 'Approved' ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600']">
                  <CheckCircle v-if="item.result === 'Approved'" :size="12" />
                  <XCircle v-else :size="12" />
                  {{ item.result }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Stats Footer -->
      <div class="p-6 bg-slate-50/50 border-t border-slate-50 flex items-center gap-8">
         <div class="flex items-center gap-2">
            <History :size="16" class="text-indigo-500" />
            <span class="text-xs font-bold text-slate-500 uppercase tracking-widest">Thống kê tháng này:</span>
         </div>
         <div class="flex gap-6">
            <span class="text-xs font-bold text-slate-700">Đã duyệt: <span class="text-emerald-600">12</span></span>
            <span class="text-xs font-bold text-slate-700">Đã từ chối: <span class="text-rose-600">3</span></span>
         </div>
      </div>
    </div>
  </div>
</template>
