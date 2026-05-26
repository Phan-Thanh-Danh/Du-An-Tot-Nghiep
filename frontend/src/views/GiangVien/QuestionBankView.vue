<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, Filter, MoreHorizontal, Edit2, 
  Trash2, Database, HelpCircle, Layers, Shield, 
  BookOpen, Target, AlignLeft, CheckSquare, ChevronRight
} from 'lucide-vue-next'

const questions = ref([
  { id: 1, content: 'HTML là viết tắt của từ gì?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Web Development', usageCount: 12 },
  { id: 2, content: 'Sự khác biệt giữa let và var trong JavaScript là gì?', type: 'Tự luận', difficulty: 'Trung bình', category: 'JavaScript', usageCount: 5 },
  { id: 3, content: 'Giải thuật sắp xếp nhanh (Quick Sort) có độ phức tạp trung bình là bao nhiêu?', type: 'Trắc nghiệm', difficulty: 'Khó', category: 'Algorithms', usageCount: 8 },
  { id: 4, content: 'Trình bày các đặc tính của lập trình hướng đối tượng (OOP).', type: 'Tự luận', difficulty: 'Trung bình', category: 'Software Engineering', usageCount: 15 },
])

const difficultyColors = {
  'Dễ': 'bg-emerald-50 text-emerald-600 border-emerald-100',
  'Trung bình': 'bg-amber-50 text-amber-600 border-amber-100',
  'Khó': 'bg-rose-50 text-rose-600 border-rose-100',
}

const typeIcons = {
  'Trắc nghiệm': CheckSquare,
  'Tự luận': AlignLeft,
}

// --- Modal State & Actions ---
const isEditModalOpen = ref(false)
const isDeleteModalOpen = ref(false)
const editingQuestion = ref({ id: null, content: '', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Web Development' })
const deletingQuestionId = ref(null)

const categories = ['Web Development', 'JavaScript', 'Algorithms', 'Software Engineering', 'Database', 'General']
const types = ['Trắc nghiệm', 'Tự luận']
const difficulties = ['Dễ', 'Trung bình', 'Khó']

function openEditModal(question) {
  editingQuestion.value = { ...question }
  isEditModalOpen.value = true
}

function saveQuestion() {
  const idx = questions.value.findIndex(q => q.id === editingQuestion.value.id)
  if (idx !== -1) {
    questions.value[idx] = { ...questions.value[idx], ...editingQuestion.value }
  }
  isEditModalOpen.value = false
}

function confirmDelete(id) {
  deletingQuestionId.value = id
  isDeleteModalOpen.value = true
}

function deleteQuestion() {
  if (deletingQuestionId.value) {
    questions.value = questions.value.filter(q => q.id !== deletingQuestionId.value)
  }
  isDeleteModalOpen.value = false
  deletingQuestionId.value = null
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-gradient-to-br from-sky-500 to-blue-600 flex items-center justify-center text-white shadow-md shadow-sky-200">
           <BookOpen :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Thư viện câu hỏi</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Quản lý kho tài nguyên câu hỏi trắc nghiệm và tự luận.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <router-link to="/teacher/questions/create" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-blue-600 px-4 py-4 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all">
            <Plus :size="18" /> Thêm câu hỏi mới
         </router-link>
      </div>
    </div>

    <!-- Quick Stats & Filters -->
    <div class="flex flex-col xl:flex-row gap-4">
       <!-- Stats -->
       <div class="grid grid-cols-2 md:grid-cols-4 xl:w-1/2 gap-4">
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm col-span-2 sm:col-span-1">
             <div class="h-10 w-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 mb-3">
                <Database :size="20" />
             </div>
             <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Tổng câu hỏi</p>
             <p class="text-xl font-black text-slate-800">1,240</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-emerald-50 flex items-center justify-center text-emerald-600 mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Mức Dễ</p>
             <p class="text-xl font-black text-slate-800">450</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-amber-50 flex items-center justify-center text-amber-600 mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Mức Trung bình</p>
             <p class="text-xl font-black text-slate-800">620</p>
          </div>
          <div class="rounded-[24px] bg-white border border-slate-100 p-5 shadow-sm">
             <div class="h-10 w-10 rounded-xl bg-rose-50 flex items-center justify-center text-rose-600 mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Mức Khó</p>
             <p class="text-xl font-black text-slate-800">170</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 rounded-2xl bg-white border border-slate-100 p-4 shadow-sm flex flex-col justify-center">
          <p class="text-sm font-bold text-slate-800 mb-4 flex items-center gap-2"><Filter :size="16" class="text-blue-500" /> Bộ lọc tìm kiếm</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
              <input type="text" placeholder="Tìm kiếm nội dung câu hỏi..." class="w-full rounded-[16px] border border-slate-200 bg-slate-50 pl-11 pr-4 py-3.5 text-sm font-medium outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
               <select class="w-full rounded-[16px] border border-slate-200 bg-slate-50 px-4 py-3.5 text-sm font-bold text-slate-600 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors appearance-none cursor-pointer">
                  <option>Tất cả độ khó</option>
                  <option>Mức Dễ</option>
                  <option>Mức Trung bình</option>
                  <option>Mức Khó</option>
               </select>
               <ChevronRight :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 rotate-90 pointer-events-none" />
            </div>
          </div>
       </div>
    </div>

    <!-- Questions Table -->
    <div class="rounded-2xl border border-slate-100 bg-white shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50 border-b border-slate-100">
              <th class="px-5 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Nội dung câu hỏi</th>
              <th class="px-4 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Loại & Phân loại</th>
              <th class="px-4 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400">Độ khó</th>
              <th class="px-5 py-5 text-[11px] font-black uppercase tracking-widest text-slate-400 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="q in questions" :key="q.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-5 py-4">
                 <div class="max-w-lg">
                    <p class="text-sm font-bold text-slate-800 leading-relaxed group-hover:text-blue-700 transition-colors">{{ q.content }}</p>
                    <div class="flex items-center gap-3 mt-2">
                       <span class="text-[10px] font-bold text-slate-400 flex items-center gap-1">
                          <Database :size="12" /> Đã sử dụng {{ q.usageCount }} lần
                       </span>
                    </div>
                 </div>
              </td>
              <td class="px-4 py-4">
                 <div class="flex flex-col gap-2">
                    <span class="inline-flex items-center gap-1.5 text-xs font-bold text-slate-600 bg-slate-50 px-2.5 py-1.5 rounded-lg border border-slate-100 w-max">
                       <component :is="typeIcons[q.type]" :size="14" class="text-slate-400" />
                       {{ q.type }}
                    </span>
                    <span class="inline-flex items-center gap-1.5 text-[10px] font-black uppercase tracking-widest text-blue-600 w-max">
                       <Layers :size="12" /> {{ q.category }}
                    </span>
                 </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['inline-flex items-center px-3 py-1.5 rounded-xl text-[10px] font-black uppercase tracking-widest border', difficultyColors[q.difficulty]]">
                  {{ q.difficulty }}
                </span>
              </td>
              <td class="px-5 py-4 text-right">
                <div class="flex items-center justify-end gap-2">
                   <button @click="openEditModal(q)" class="h-9 w-9 rounded-xl flex items-center justify-center text-slate-400 bg-white border border-slate-200 hover:text-blue-600 hover:border-blue-200 hover:bg-blue-50 transition-colors shadow-sm" title="Chỉnh sửa">
                      <Edit2 :size="16" />
                   </button>
                   <button @click="confirmDelete(q.id)" class="h-9 w-9 rounded-xl flex items-center justify-center text-slate-400 bg-white border border-slate-200 hover:text-rose-600 hover:border-rose-200 hover:bg-rose-50 transition-colors shadow-sm" title="Xóa">
                      <Trash2 :size="16" />
                   </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="bg-slate-50/80 px-5 py-5 border-t border-slate-100 flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-bold text-slate-500 uppercase tracking-widest">
            <Database :size="14" class="text-slate-400" /> Đang hiển thị {{ questions.length }} câu hỏi
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-blue-500 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
            <div class="h-1 w-2 bg-blue-200 rounded-full"></div>
         </div>
      </div>
    </div>

    <!-- Edit Modal -->
    <Teleport to="body">
      <div v-if="isEditModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="isEditModalOpen = false"></div>
        <div class="relative bg-white rounded-3xl shadow-2xl w-full max-w-lg overflow-hidden animate-fade-in-up">
          <div class="p-4 border-b border-slate-100 bg-slate-50/50">
            <h3 class="text-xl font-bold text-slate-800">Chỉnh sửa Câu hỏi</h3>
            <p class="text-sm text-slate-500 mt-1">Cập nhật nội dung và thuộc tính phân loại.</p>
          </div>
          <div class="p-4 space-y-5">
            <div>
              <label class="block text-sm font-bold text-slate-700 mb-1.5">Nội dung câu hỏi</label>
              <textarea v-model="editingQuestion.content" rows="3" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all resize-none"></textarea>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-bold text-slate-700 mb-1.5">Phân loại (Môn học)</label>
                <select v-model="editingQuestion.category" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all">
                  <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-bold text-slate-700 mb-1.5">Loại câu hỏi</label>
                <select v-model="editingQuestion.type" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all">
                  <option v-for="t in types" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>
            </div>
            <div>
               <label class="block text-sm font-bold text-slate-700 mb-1.5">Độ khó</label>
               <div class="flex gap-3">
                  <button v-for="d in difficulties" :key="d" @click="editingQuestion.difficulty = d" :class="['flex-1 py-2.5 rounded-xl text-sm font-bold border transition-colors', editingQuestion.difficulty === d ? (d==='Dễ' ? 'bg-emerald-50 border-emerald-200 text-emerald-600' : d==='Trung bình' ? 'bg-amber-50 border-amber-200 text-amber-600' : 'bg-rose-50 border-rose-200 text-rose-600') : 'bg-white border-slate-200 text-slate-500 hover:bg-slate-50']">
                     {{ d }}
                  </button>
               </div>
            </div>
          </div>
          <div class="p-4 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3">
            <button @click="isEditModalOpen = false" class="px-5 py-2.5 rounded-xl font-bold text-slate-500 hover:bg-slate-200 transition-colors">Hủy</button>
            <button @click="saveQuestion" class="px-5 py-2.5 rounded-xl font-bold text-white bg-blue-600 hover:bg-blue-700 shadow-md shadow-blue-200 transition-all hover:-translate-y-0.5">Lưu thay đổi</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete Confirm Modal -->
    <Teleport to="body">
      <div v-if="isDeleteModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="isDeleteModalOpen = false"></div>
        <div class="relative bg-white rounded-3xl shadow-2xl w-full max-w-sm overflow-hidden animate-fade-in-up text-center p-8">
          <div class="mx-auto w-16 h-16 rounded-full bg-rose-100 flex items-center justify-center text-rose-500 mb-4">
            <Trash2 :size="32" stroke-width="2.5" />
          </div>
          <h3 class="text-xl font-black text-slate-800 mb-2">Xóa câu hỏi?</h3>
          <p class="text-slate-500 text-sm mb-8 font-medium">Bạn có chắc chắn muốn xóa câu hỏi này khỏi thư viện? Hành động này không thể hoàn tác.</p>
          <div class="flex gap-3">
            <button @click="isDeleteModalOpen = false" class="flex-1 px-4 py-3 rounded-xl font-bold text-slate-600 bg-slate-100 hover:bg-slate-200 transition-colors">Hủy</button>
            <button @click="deleteQuestion" class="flex-1 px-4 py-3 rounded-xl font-bold text-white bg-rose-500 hover:bg-rose-600 shadow-md shadow-rose-200 transition-all hover:-translate-y-0.5">Xóa ngay</button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fade-in-up 0.3s ease-out forwards;
}
</style>
