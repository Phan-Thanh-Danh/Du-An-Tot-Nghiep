<script setup>
import { ref } from 'vue'
import { 
  Plus, Search, Filter, MoreHorizontal, Edit2, 
  Trash2, Database, HelpCircle, Layers,
  BookOpen, Target, AlignLeft, CheckSquare, ChevronRight
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'

const questions = ref([
  { id: 1, content: 'HTML là viết tắt của từ gì?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Web Development', usageCount: 12 },
  { id: 2, content: 'Sự khác biệt giữa let và var trong JavaScript là gì?', type: 'Tự luận', difficulty: 'Trung bình', category: 'JavaScript', usageCount: 5 },
  { id: 3, content: 'Giải thuật sắp xếp nhanh (Quick Sort) có độ phức tạp trung bình là bao nhiêu?', type: 'Trắc nghiệm', difficulty: 'Khó', category: 'Algorithms', usageCount: 8 },
  { id: 4, content: 'Trình bày các đặc tính của lập trình hướng đối tượng (OOP).', type: 'Tự luận', difficulty: 'Trung bình', category: 'Software Engineering', usageCount: 15 },
])

const difficultyVariants = {
  'Dễ': 'success',
  'Trung bình': 'warning',
  'Khó': 'danger',
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
  <div class="space-y-8 pb-10">
    <!-- ── Header ── -->
    <div class="lg-glass-soft rounded-2xl p-5 relative overflow-hidden">
      <div class="relative z-10 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div class="flex items-center gap-5">
          <div class="h-10 w-10 rounded-2xl bg-[var(--accent-primary)] flex items-center justify-center text-inverse shadow-md">
             <BookOpen :size="20" />
          </div>
          <div>
            <h1 class="text-xl md:text-xl font-semibold text-heading tracking-tight">Thư viện câu hỏi</h1>
            <p class="text-sm font-medium text-muted mt-1">Quản lý kho tài nguyên câu hỏi trắc nghiệm và tự luận.</p>
          </div>
        </div>
        <div class="flex gap-3">
           <router-link to="/teacher/questions/create">
             <GlassButton variant="primary" size="sm">
               <Plus :size="18" /> Thêm câu hỏi mới
             </GlassButton>
           </router-link>
        </div>
      </div>
    </div>

    <!-- Quick Stats & Filters -->
    <div class="flex flex-col xl:flex-row gap-4">
       <!-- Stats -->
       <div class="grid grid-cols-2 md:grid-cols-4 xl:w-1/2 gap-4">
          <div class="lg-glass-soft rounded-2xl p-5 col-span-2 sm:col-span-1">
             <div class="h-10 w-10 rounded-xl bg-[var(--accent-primary)]/10 flex items-center justify-center text-link mb-3">
                <Database :size="20" />
             </div>
             <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-1">Tổng câu hỏi</p>
             <p class="text-xl font-semibold text-heading">1,240</p>
          </div>
          <div class="lg-glass-soft rounded-2xl p-5">
             <div class="h-10 w-10 rounded-xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-1">Mức Dễ</p>
             <p class="text-xl font-semibold text-heading">450</p>
          </div>
          <div class="lg-glass-soft rounded-2xl p-5">
             <div class="h-10 w-10 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)] mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-1">Mức Trung bình</p>
             <p class="text-xl font-semibold text-heading">620</p>
          </div>
          <div class="lg-glass-soft rounded-2xl p-5">
             <div class="h-10 w-10 rounded-xl bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)] mb-3">
                <Target :size="20" />
             </div>
             <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-1">Mức Khó</p>
             <p class="text-xl font-semibold text-heading">170</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 lg-glass-soft rounded-2xl p-4 flex flex-col justify-center">
          <p class="text-sm font-semibold text-heading mb-4 flex items-center gap-2"><Filter :size="16" class="text-link" /> Bộ lọc tìm kiếm</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
              <input type="text" placeholder="Tìm kiếm nội dung câu hỏi..." class="lg-control w-full pl-11 pr-4" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
               <select class="lg-control w-full appearance-none cursor-pointer">
                  <option>Tất cả độ khó</option>
                  <option>Mức Dễ</option>
                  <option>Mức Trung bình</option>
                  <option>Mức Khó</option>
               </select>
               <ChevronRight :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-muted rotate-90 pointer-events-none" />
            </div>
          </div>
       </div>
    </div>

    <!-- Questions Table -->
    <TableShell>
      <table>
        <thead>
          <tr>
            <th class="px-5 py-5 text-[11px] font-semibold uppercase tracking-widest text-muted">Nội dung câu hỏi</th>
            <th class="px-4 py-5 text-[11px] font-semibold uppercase tracking-widest text-muted">Loại & Phân loại</th>
            <th class="px-4 py-5 text-[11px] font-semibold uppercase tracking-widest text-muted">Độ khó</th>
            <th class="px-5 py-5 text-[11px] font-semibold uppercase tracking-widest text-muted text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-table)]">
          <tr v-for="q in questions" :key="q.id" class="group hover:bg-[var(--surface-table-row-hover)] transition-colors">
            <td class="px-5 py-4">
               <div class="max-w-lg">
                  <p class="text-sm font-semibold text-heading leading-relaxed group-hover:text-link transition-colors">{{ q.content }}</p>
                  <div class="flex items-center gap-3 mt-2">
                     <span class="text-[10px] font-medium text-muted flex items-center gap-1">
                        <Database :size="12" /> Đã sử dụng {{ q.usageCount }} lần
                     </span>
                  </div>
               </div>
            </td>
            <td class="px-4 py-4">
               <div class="flex flex-col gap-2">
                  <span class="inline-flex items-center gap-1.5 text-xs font-semibold text-body surface-card px-2.5 py-1.5 rounded-lg border border-card w-max">
                     <component :is="typeIcons[q.type]" :size="14" class="text-muted" />
                     {{ q.type }}
                  </span>
                  <span class="inline-flex items-center gap-1.5 text-[10px] font-semibold uppercase tracking-widest text-link w-max">
                     <Layers :size="12" /> {{ q.category }}
                  </span>
               </div>
            </td>
            <td class="px-4 py-4">
              <GlassBadge :variant="difficultyVariants[q.difficulty]">
                {{ q.difficulty }}
              </GlassBadge>
            </td>
            <td class="px-5 py-4 text-right">
              <div class="flex items-center justify-end gap-2">
                 <button @click="openEditModal(q)" class="lg-icon-button h-9 w-9 rounded-xl border border-card surface-card text-muted hover:text-link hover:border-link/30 hover:bg-[var(--accent-primary)]/10 transition-all" title="Chỉnh sửa">
                    <Edit2 :size="16" />
                 </button>
                 <button @click="confirmDelete(q.id)" class="lg-icon-button h-9 w-9 rounded-xl border border-card surface-card text-muted hover:text-[var(--color-danger-text)] hover:border-[var(--color-danger-bg)] hover:bg-[var(--color-danger-bg)] transition-all" title="Xóa">
                    <Trash2 :size="16" />
                 </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      
      <!-- Footer -->
      <div class="surface-table-header px-5 py-5 border-t border-table flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-medium text-muted tracking-wider">
            <Database :size="14" class="text-muted" /> Đang hiển thị {{ questions.length }} câu hỏi
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-[var(--accent-primary)] rounded-full"></div>
            <div class="h-1 w-2 bg-[var(--accent-primary)]/30 rounded-full"></div>
            <div class="h-1 w-2 bg-[var(--accent-primary)]/30 rounded-full"></div>
         </div>
      </div>
    </TableShell>

    <!-- Edit Modal -->
    <Teleport to="body">
      <div v-if="isEditModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[var(--text-heading)]/40 backdrop-blur-sm" @click="isEditModalOpen = false"></div>
        <div class="relative surface-modal rounded-3xl shadow-2xl w-full max-w-lg overflow-hidden animate-fade-in-up">
          <div class="p-4 border-b border-card surface-table-header">
            <h3 class="text-xl font-semibold text-heading">Chỉnh sửa Câu hỏi</h3>
            <p class="text-sm text-muted mt-1">Cập nhật nội dung và thuộc tính phân loại.</p>
          </div>
          <div class="p-4 space-y-5">
            <div>
              <label class="block text-sm font-semibold text-label mb-1.5">Nội dung câu hỏi</label>
              <textarea v-model="editingQuestion.content" rows="3" class="lg-control w-full resize-none"></textarea>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-semibold text-label mb-1.5">Phân loại (Môn học)</label>
                <select v-model="editingQuestion.category" class="lg-control w-full">
                  <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-semibold text-label mb-1.5">Loại câu hỏi</label>
                <select v-model="editingQuestion.type" class="lg-control w-full">
                  <option v-for="t in types" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>
            </div>
            <div>
               <label class="block text-sm font-semibold text-label mb-1.5">Độ khó</label>
               <div class="flex gap-3">
                  <button v-for="d in difficulties" :key="d" @click="editingQuestion.difficulty = d" :class="['flex-1 py-2.5 rounded-xl text-sm font-semibold border transition-colors', editingQuestion.difficulty === d ? (d==='Dễ' ? 'bg-[var(--color-success-bg)] border-[var(--color-success-text)]/30 text-[var(--color-success-text)]' : d==='Trung bình' ? 'bg-[var(--color-warning-bg)] border-[var(--color-warning-text)]/30 text-[var(--color-warning-text)]' : 'bg-[var(--color-danger-bg)] border-[var(--color-danger-text)]/30 text-[var(--color-danger-text)]') : 'border-card surface-card text-muted hover:text-heading hover:bg-[var(--accent-primary)]/5']">
                     {{ d }}
                  </button>
               </div>
            </div>
          </div>
          <div class="p-4 border-t border-card surface-table-header flex justify-end gap-3">
<button @click="isEditModalOpen = false" class="px-5 py-2.5 rounded-xl font-semibold text-muted hover:text-heading hover:bg-[var(--accent-primary)]/5 transition-colors">Hủy</button>
            <button @click="saveQuestion" class="px-5 py-2.5 rounded-xl font-semibold text-inverse bg-[var(--accent-primary)] hover:opacity-90 shadow-md transition-all hover:-translate-y-0.5">Lưu thay đổi</button>
          </div>
        </div>
      </div>
    </Teleport>

    <!-- Delete Confirm Modal -->
    <Teleport to="body">
      <div v-if="isDeleteModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[var(--text-heading)]/40 backdrop-blur-sm" @click="isDeleteModalOpen = false"></div>
        <div class="relative surface-modal rounded-3xl shadow-2xl w-full max-w-sm overflow-hidden animate-fade-in-up text-center p-8">
          <div class="mx-auto w-16 h-16 rounded-full bg-[var(--color-danger-bg)] flex items-center justify-center text-[var(--color-danger-text)] mb-4">
            <Trash2 :size="32" stroke-width="2.5" />
          </div>
          <h3 class="text-xl font-semibold text-heading mb-2">Xóa câu hỏi?</h3>
          <p class="text-muted text-sm mb-8 font-medium">Bạn có chắc chắn muốn xóa câu hỏi này khỏi thư viện? Hành động này không thể hoàn tác.</p>
          <div class="flex gap-3">
<button @click="isDeleteModalOpen = false" class="flex-1 px-4 py-3 rounded-xl font-semibold text-body surface-card border border-card hover:bg-[var(--accent-primary)]/5 transition-colors">Hủy</button>
            <button @click="deleteQuestion" class="flex-1 px-4 py-3 rounded-xl font-semibold text-inverse bg-[var(--color-danger-text)] hover:opacity-90 shadow-md transition-all hover:-translate-y-0.5">Xóa ngay</button>
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
