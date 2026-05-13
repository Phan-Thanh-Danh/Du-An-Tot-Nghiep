<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, FileEdit, Clock, HelpCircle, 
  ChevronRight, MoreVertical, Send, CheckCircle2 
} from 'lucide-vue-next'

const exams = ref([
  { id: 1, name: 'Thi giữa kỳ: Lập trình Web', duration: '90 min', questions: 40, status: 'Published', date: '25/05/2026' },
  { id: 2, name: 'Thi kết thúc môn: Java Basic', duration: '120 min', questions: 50, status: 'Draft', date: '15/06/2026' },
  { id: 3, name: 'Quiz 2: Cấu trúc dữ liệu', duration: '15 min', questions: 10, status: 'Published', date: '18/05/2026' },
])

const getStatusStyle = (status) => {
  return status === 'Published' ? 'bg-emerald-50 text-emerald-600 border-emerald-100' : 'bg-amber-50 text-amber-600 border-amber-100'
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Quản lý Đề thi</h1>
        <p class="text-slate-500 mt-1">Thiết kế và cấu hình các bộ đề thi trắc nghiệm & tự luận.</p>
      </div>
      <button class="lg-button-primary py-3 px-6" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
        <Plus :size="20" /> Tạo đề thi mới
      </button>
    </div>

    <!-- Main List -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 text-slate-800">
      <div v-for="exam in exams" :key="exam.id" class="lg-card-glass lg-card-hover group border-slate-100 p-6 flex flex-col">
        <div class="flex justify-between items-start mb-6">
           <div class="h-12 w-12 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 shadow-sm">
              <FileEdit :size="24" />
           </div>
           <div :class="['rounded-full border px-3 py-1 text-[10px] font-black uppercase tracking-wider', getStatusStyle(exam.status)]">
              {{ exam.status }}
           </div>
        </div>
        
        <div class="flex-1">
           <h3 class="text-xl font-bold text-slate-800 leading-tight group-hover:text-indigo-600 transition-colors">{{ exam.name }}</h3>
           <p class="text-xs text-slate-400 mt-2 font-medium">Dự kiến: {{ exam.date }}</p>
           
           <div class="mt-6 flex items-center gap-6">
              <div class="flex flex-col">
                 <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Thời gian</span>
                 <div class="flex items-center gap-1.5 mt-1">
                    <Clock :size="14" class="text-indigo-500" />
                    <span class="text-sm font-black">{{ exam.duration }}</span>
                 </div>
              </div>
              <div class="flex flex-col border-l border-slate-100 pl-6">
                 <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Số câu</span>
                 <div class="flex items-center gap-1.5 mt-1">
                    <HelpCircle :size="14" class="text-indigo-500" />
                    <span class="text-sm font-black">{{ exam.questions }} câu</span>
                 </div>
              </div>
           </div>
        </div>

        <div class="mt-8 pt-4 border-t border-slate-50 flex gap-2">
           <button class="flex-1 rounded-xl bg-slate-50 py-2.5 text-xs font-bold text-slate-600 hover:bg-slate-100 transition-all">Chỉnh sửa</button>
           <button v-if="exam.status === 'Draft'" class="flex-1 rounded-xl bg-indigo-600 py-2.5 text-xs font-bold text-white shadow-lg shadow-indigo-100 hover:bg-indigo-700 transition-all flex items-center justify-center gap-2">
              <Send :size="14" /> Publish
           </button>
           <button v-else class="flex-1 rounded-xl bg-emerald-50 py-2.5 text-xs font-bold text-emerald-600 flex items-center justify-center gap-2">
              <CheckCircle2 :size="14" /> Đã đăng
           </button>
        </div>
      </div>
    </div>
  </div>
</template>
