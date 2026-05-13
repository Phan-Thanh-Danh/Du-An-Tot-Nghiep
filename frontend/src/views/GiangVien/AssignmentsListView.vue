<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, ClipboardList, Users, Clock, 
  ChevronRight, MoreVertical, FileText, Send 
} from 'lucide-vue-next'

const assignments = ref([
  { id: 1, name: 'Assignment 1: HTML/CSS Basic', className: 'SE1601', deadline: '20/05/2026', submissions: '42/45', status: 'Active' },
  { id: 2, name: 'Assignment 2: JavaScript DOM', className: 'SE1601', deadline: '28/05/2026', submissions: '12/45', status: 'Active' },
  { id: 3, name: 'Lab 1: UI Design with Figma', className: 'SS1402', deadline: '15/05/2026', submissions: '38/38', status: 'Completed' },
])

function createAssignment() {
  alert('Chức năng tạo bài tập mới')
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Danh sách bài tập</h1>
        <p class="text-slate-500 mt-1">Quản lý các bài tập về nhà, bài Lab và Assignment cho các lớp.</p>
      </div>
      <button @click="createAssignment" class="lg-button-primary py-3 px-6" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
        <Plus :size="20" /> Tạo bài tập mới
      </button>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
       <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
          <div class="h-12 w-12 rounded-2xl bg-indigo-50 text-indigo-600 flex items-center justify-center">
             <ClipboardList :size="24" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Đang mở</p>
             <p class="text-2xl font-black text-slate-800">12 bài tập</p>
          </div>
       </div>
       <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
          <div class="h-12 w-12 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center">
             <Send :size="24" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Tổng bài nộp</p>
             <p class="text-2xl font-black text-slate-800">458 bài</p>
          </div>
       </div>
       <div class="lg-card-glass p-6 border-slate-100 flex items-center gap-5">
          <div class="h-12 w-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
             <Clock :size="24" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Cần chấm điểm</p>
             <p class="text-2xl font-black text-slate-800">24 bài</p>
          </div>
       </div>
    </div>

    <!-- Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="p-6 border-b border-slate-50 flex flex-col md:flex-row md:items-center justify-between gap-4 text-slate-800">
        <h2 class="text-xl font-bold">Chi tiết bài tập</h2>
        <div class="relative w-full md:w-72">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input type="text" placeholder="Tìm tên bài tập..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none" />
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Tên bài tập</th>
              <th class="px-6 py-5">Lớp</th>
              <th class="px-6 py-5">Hạn nộp</th>
              <th class="px-6 py-5">Số bài nộp</th>
              <th class="px-8 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50 text-slate-800">
            <tr v-for="asm in assignments" :key="asm.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-indigo-50 group-hover:text-indigo-600 transition-all">
                    <FileText :size="20" />
                  </div>
                  <p class="text-sm font-bold">{{ asm.name }}</p>
                </div>
              </td>
              <td class="px-6 py-5">
                <span class="rounded-lg bg-indigo-50 px-2.5 py-1 text-[11px] font-black text-indigo-600 uppercase tracking-wider">
                  {{ asm.className }}
                </span>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-2 text-sm">
                   <Clock :size="14" class="text-slate-300" />
                   <span class="font-medium">{{ asm.deadline }}</span>
                </div>
              </td>
              <td class="px-6 py-5">
                <div class="flex items-center gap-3">
                   <span class="text-sm font-black">{{ asm.submissions }}</span>
                   <div class="h-1.5 w-16 bg-slate-100 rounded-full overflow-hidden">
                      <div class="h-full bg-indigo-500" style="width: 80%"></div>
                   </div>
                </div>
              </td>
              <td class="px-8 py-5 text-right">
                <router-link to="/teacher/grading" class="inline-flex items-center gap-2 rounded-xl bg-slate-100 px-4 py-2 text-xs font-bold text-slate-600 hover:bg-indigo-600 hover:text-white transition-all">
                  Chấm điểm <ChevronRight :size="14" />
                </router-link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
