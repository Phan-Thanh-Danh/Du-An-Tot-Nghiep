<script setup>
import { ref } from 'vue'
import { 
  Search, Award, CheckCircle2, XCircle, 
  Download, Filter, ArrowUpDown, TrendingUp 
} from 'lucide-vue-next'

const gradebook = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', gpa: 8.2, status: 'Pass', credits: 3 },
  { id: 'SV16002', name: 'Trần Thị B', gpa: 7.5, status: 'Pass', credits: 3 },
  { id: 'SV16003', name: 'Lê Hoàng C', gpa: 9.0, status: 'Pass', credits: 3 },
  { id: 'SV16004', name: 'Phạm Minh D', gpa: 4.2, status: 'Fail', credits: 3 },
  { id: 'SV16005', name: 'Hoàng Văn E', gpa: 6.8, status: 'Pass', credits: 3 },
])

const avgGPA = 7.14
const passRate = 80
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Bảng điểm lớp học</h1>
        <p class="text-slate-500 mt-1">Tổng hợp kết quả học tập và trạng thái hoàn thành môn học của lớp SE1601.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất bảng điểm
        </button>
      </div>
    </div>

    <!-- Quick Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
       <div class="lg-card-glass p-6 bg-indigo-900 text-white border-none">
          <div class="flex justify-between items-center mb-4">
             <p class="text-xs font-bold text-indigo-200 uppercase tracking-widest">GPA Trung bình lớp</p>
             <TrendingUp :size="20" class="text-indigo-300" />
          </div>
          <div class="flex items-baseline gap-2">
             <span class="text-4xl font-black">{{ avgGPA }}</span>
             <span class="text-indigo-300 text-sm font-bold">/ 10.0</span>
          </div>
          <div class="mt-6 h-1.5 w-full bg-white/10 rounded-full overflow-hidden">
             <div class="h-full bg-indigo-400" :style="{ width: (avgGPA * 10) + '%' }"></div>
          </div>
       </div>

       <div class="lg-card-glass p-6 bg-emerald-900 text-white border-none">
          <div class="flex justify-between items-center mb-4">
             <p class="text-xs font-bold text-emerald-200 uppercase tracking-widest">Tỉ lệ đạt (Pass Rate)</p>
             <CheckCircle2 :size="20" class="text-emerald-300" />
          </div>
          <div class="flex items-baseline gap-2">
             <span class="text-4xl font-black">{{ passRate }}%</span>
             <span class="text-emerald-300 text-sm font-bold">Hoàn thành môn</span>
          </div>
          <div class="mt-6 h-1.5 w-full bg-white/10 rounded-full overflow-hidden">
             <div class="h-full bg-emerald-400" :style="{ width: passRate + '%' }"></div>
          </div>
       </div>
    </div>

    <!-- Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="p-6 border-b border-slate-50 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <h2 class="text-xl font-bold text-slate-800">Bảng kết quả chi tiết</h2>
        <div class="relative w-full md:w-72">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input type="text" placeholder="Tìm sinh viên..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
        </div>
      </div>
      
      <div class="overflow-x-auto text-slate-800">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Sinh viên</th>
              <th class="px-6 py-5">Mã số SV</th>
              <th class="px-6 py-5">
                 <div class="flex items-center gap-2 text-indigo-600 font-black">GPA <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-6 py-5 text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sv in gradebook" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-400 font-bold text-xs">
                    {{ sv.name.split(' ').pop()[0] }}
                  </div>
                  <p class="text-sm font-bold text-slate-800">{{ sv.name }}</p>
                </div>
              </td>
              <td class="px-6 py-5 text-sm font-medium text-slate-500">{{ sv.id }}</td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2">
                   <div class="h-8 w-8 rounded-lg bg-indigo-50 flex items-center justify-center text-indigo-600 border border-indigo-100">
                      <Award :size="16" />
                   </div>
                   <span :class="['text-base font-black', sv.gpa < 5 ? 'text-rose-500' : 'text-slate-800']">{{ sv.gpa }}</span>
                </div>
              </td>
              <td class="px-8 py-5 text-right">
                <div :class="['inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-[10px] font-black uppercase tracking-wider', sv.status === 'Pass' ? 'bg-emerald-50 text-emerald-600 border border-emerald-100' : 'bg-rose-50 text-rose-600 border border-rose-100']">
                  <CheckCircle2 v-if="sv.status === 'Pass'" :size="12" />
                  <XCircle v-else :size="12" />
                  {{ sv.status }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
