<script setup>
import { ref } from 'vue'
import { 
  Search, Edit3, Save, Download, Filter, 
  ChevronRight, ArrowUpDown, CheckCircle 
} from 'lucide-vue-next'

const gradesData = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', assignment: 8.5, exam: 7.5, total: 7.9, isEditing: false },
  { id: 'SV16002', name: 'Trần Thị B', assignment: 7.0, exam: 8.0, total: 7.6, isEditing: false },
  { id: 'SV16003', name: 'Lê Hoàng C', assignment: 9.5, exam: 9.0, total: 9.2, isEditing: false },
  { id: 'SV16004', name: 'Phạm Minh D', assignment: 5.0, exam: 4.5, total: 4.7, isEditing: false },
])

function toggleEdit(sv) {
  sv.isEditing = !sv.isEditing
}

function calculateTotal(sv) {
  // Mock formula: 40% assignment, 60% exam
  sv.total = (sv.assignment * 0.4 + sv.exam * 0.6).toFixed(1)
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Bảng điểm lớp học</h1>
        <p class="text-slate-500 mt-1">Quản lý điểm thành phần và điểm thi kết thúc môn của lớp SE1601.</p>
      </div>
      <div class="flex gap-2">
        <button class="lg-button-secondary py-2.5 px-4 text-xs font-bold">
          <Download :size="18" /> Xuất bảng điểm
        </button>
        <button class="lg-button-primary py-2.5 px-6 text-xs font-bold" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
          <Save :size="18" /> Lưu toàn bộ
        </button>
      </div>
    </div>

    <!-- Filters & Search -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4 items-center">
      <div class="relative flex-1 w-full">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm sinh viên bằng tên hoặc MSSV..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
      </div>
      <div class="flex items-center gap-3 w-full md:w-auto">
        <button class="flex items-center gap-2 rounded-xl border border-slate-200 px-4 py-2.5 text-xs font-bold text-slate-600 hover:bg-slate-50 transition-colors">
          <Filter :size="16" /> Lọc nâng cao
        </button>
      </div>
    </div>

    <!-- Grades Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Sinh viên</th>
              <th class="px-6 py-5">
                 <div class="flex items-center gap-2">Assignment (40%) <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-6 py-5">
                 <div class="flex items-center gap-2">Điểm thi (60%) <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-6 py-5">
                 <div class="flex items-center gap-2 text-indigo-600">Điểm tổng kết <ArrowUpDown :size="12" /></div>
              </th>
              <th class="px-8 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sv in gradesData" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-600 font-bold text-xs">
                    {{ sv.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800">{{ sv.name }}</p>
                    <p class="text-[10px] text-slate-400">{{ sv.id }}</p>
                  </div>
                </div>
              </td>
              
              <!-- Assignment Grade -->
              <td class="px-6 py-5">
                <input 
                  v-if="sv.isEditing" 
                  type="number" 
                  v-model="sv.assignment" 
                  @input="calculateTotal(sv)"
                  class="w-20 rounded-lg border border-indigo-200 bg-white px-2 py-1.5 text-sm font-bold outline-none focus:ring-4 focus:ring-indigo-50"
                  max="10" min="0" step="0.1"
                />
                <span v-else class="text-sm font-bold text-slate-700">{{ sv.assignment }}</span>
              </td>

              <!-- Exam Grade -->
              <td class="px-6 py-5">
                <input 
                  v-if="sv.isEditing" 
                  type="number" 
                  v-model="sv.exam" 
                  @input="calculateTotal(sv)"
                  class="w-20 rounded-lg border border-indigo-200 bg-white px-2 py-1.5 text-sm font-bold outline-none focus:ring-4 focus:ring-indigo-50"
                  max="10" min="0" step="0.1"
                />
                <span v-else class="text-sm font-bold text-slate-700">{{ sv.exam }}</span>
              </td>

              <!-- Total Grade -->
              <td class="px-6 py-5">
                <span :class="['text-base font-black', sv.total < 5 ? 'text-rose-500' : 'text-indigo-600']">
                  {{ sv.total }}
                </span>
              </td>

              <td class="px-8 py-5 text-right">
                <button 
                  @click="toggleEdit(sv)"
                  :class="[
                    'inline-flex items-center gap-2 rounded-xl px-4 py-2 text-xs font-bold transition-all',
                    sv.isEditing 
                      ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-100 hover:bg-emerald-700' 
                      : 'bg-slate-100 text-slate-600 hover:bg-indigo-50 hover:text-indigo-600'
                  ]"
                >
                  <component :is="sv.isEditing ? CheckCircle : Edit3" :size="14" />
                  {{ sv.isEditing ? 'Xong' : 'Sửa điểm' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Note Section -->
    <div class="lg-alert lg-alert-info">
       <p class="font-bold">Hướng dẫn nhập điểm</p>
       <ul class="mt-1 text-xs space-y-1 list-disc list-inside opacity-80">
          <li>Điểm tổng kết được tính tự động dựa trên trọng số môn học.</li>
          <li>Sau khi sửa điểm, hãy nhấn "Lưu toàn bộ" để cập nhật vào hệ thống chính thức.</li>
          <li>Các trường hợp điểm dưới 5.0 sẽ được đánh dấu đỏ tự động.</li>
       </ul>
    </div>
  </div>
</template>
