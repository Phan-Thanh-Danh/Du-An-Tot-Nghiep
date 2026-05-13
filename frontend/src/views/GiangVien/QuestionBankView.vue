<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, Filter, MoreHorizontal, Edit2, 
  Trash2, Database, HelpCircle, Layers, Shield 
} from 'lucide-vue-next'

const questions = ref([
  { id: 1, content: 'HTML là viết tắt của từ gì?', type: 'Multiple Choice', difficulty: 'Easy', category: 'Web Development' },
  { id: 2, content: 'Sự khác biệt giữa let và var trong JS?', type: 'Essay', difficulty: 'Medium', category: 'JavaScript' },
  { id: 3, content: 'Giải thuật sắp xếp nhanh (Quick Sort) có độ phức tạp trung bình là bao nhiêu?', type: 'Multiple Choice', difficulty: 'Hard', category: 'Algorithms' },
])

const difficultyColors = {
  'Easy': 'bg-emerald-50 text-emerald-600',
  'Medium': 'bg-amber-50 text-amber-600',
  'Hard': 'bg-rose-50 text-rose-600',
}
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-slate-800 tracking-tight">Ngân hàng câu hỏi</h1>
        <p class="text-slate-500 mt-1">Quản lý kho câu hỏi trắc nghiệm và tự luận cho các kỳ thi.</p>
      </div>
      <button class="lg-button-primary py-3 px-6" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
        <Plus :size="20" /> Thêm câu hỏi mới
      </button>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
       <div v-for="item in [{label: 'Tổng số', val: '1,240', icon: Database, color: 'text-indigo-600'}, {label: 'Dễ', val: '450', icon: Shield, color: 'text-emerald-600'}, {label: 'Trung bình', val: '620', icon: Shield, color: 'text-amber-600'}, {label: 'Khó', val: '170', icon: Shield, color: 'text-rose-600'}]" :key="item.label"
            class="lg-card-glass p-4 border-slate-100 flex items-center gap-4">
          <div :class="['h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center', item.color]">
             <component :is="item.icon" :size="20" />
          </div>
          <div>
             <p class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">{{ item.label }}</p>
             <p class="text-xl font-black text-slate-800">{{ item.val }}</p>
          </div>
       </div>
    </div>

    <!-- Filters -->
    <div class="rounded-[24px] border border-slate-100 bg-white p-4 shadow-sm flex flex-col md:flex-row gap-4">
      <div class="relative flex-1">
        <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
        <input type="text" placeholder="Tìm nội dung câu hỏi..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-11 pr-4 py-2.5 text-sm outline-none focus:border-indigo-300" />
      </div>
      <div class="flex gap-2">
         <select class="rounded-xl border border-slate-100 bg-slate-50 px-4 py-2.5 text-sm font-medium outline-none">
            <option>Tất cả độ khó</option>
            <option>Dễ</option>
            <option>Trung bình</option>
            <option>Khó</option>
         </select>
         <button class="rounded-xl border border-slate-200 p-2.5 text-slate-400 hover:bg-slate-50 transition-colors">
            <Filter :size="18" />
         </button>
      </div>
    </div>

    <!-- Table -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="overflow-x-auto text-slate-800">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-5">Nội dung câu hỏi</th>
              <th class="px-6 py-5">Loại</th>
              <th class="px-6 py-5">Độ khó</th>
              <th class="px-6 py-5">Danh mục</th>
              <th class="px-8 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="q in questions" :key="q.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-5 max-w-md">
                <p class="text-sm font-bold text-slate-700 line-clamp-2">{{ q.content }}</p>
              </td>
              <td class="px-6 py-5">
                <span class="inline-flex items-center gap-1.5 text-xs font-semibold text-slate-500">
                  <HelpCircle :size="14" /> {{ q.type }}
                </span>
              </td>
              <td class="px-6 py-5">
                <span :class="['rounded-full px-3 py-1 text-[10px] font-black uppercase tracking-tight', difficultyColors[q.difficulty]]">
                  {{ q.difficulty }}
                </span>
              </td>
              <td class="px-6 py-5">
                <span class="inline-flex items-center gap-1.5 rounded-lg bg-indigo-50 px-2.5 py-1 text-[10px] font-bold text-indigo-600">
                  <Layers :size="12" /> {{ q.category }}
                </span>
              </td>
              <td class="px-8 py-5 text-right">
                <div class="flex items-center justify-end gap-1">
                   <button class="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-all"><Edit2 :size="16" /></button>
                   <button class="p-2 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-all"><Trash2 :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
