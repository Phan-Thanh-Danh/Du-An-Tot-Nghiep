<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { 
  ArrowLeft, Save, Plus, Trash2, HelpCircle, 
  Layers, Shield, FileText, CheckCircle2
} from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'

const router = useRouter()
const popupStore = usePopupStore()

const form = ref({
  type: 'Trắc nghiệm',
  content: '',
  difficulty: 'Trung bình',
  category: 'Web Development',
  explanation: '',
})

const options = ref([
  { id: 1, text: '', isCorrect: true },
  { id: 2, text: '', isCorrect: false },
  { id: 3, text: '', isCorrect: false },
  { id: 4, text: '', isCorrect: false },
])

function addOption() {
  options.value.push({ id: Date.now(), text: '', isCorrect: false })
}

function removeOption(index) {
  if (options.value.length > 2) {
    options.value.splice(index, 1)
  }
}

function setCorrectAnswer(index) {
  options.value.forEach((opt, i) => {
    opt.isCorrect = i === index
  })
}

function saveQuestion() {
  popupStore.success('Đã lưu câu hỏi', 'Câu hỏi đã được lưu vào ngân hàng đề.')
  router.push('/teacher/questions')
}
</script>

<template>
  <div class="space-y-8 pb-10 max-w-5xl mx-auto">
    <!-- ── Header ── -->
    <div class="flex items-center justify-between lg-glass-soft rounded-2xl p-4 sticky top-4 z-10">
      <div class="flex items-center gap-4">
        <router-link to="/teacher/questions" class="h-10 w-10 rounded-2xl surface-card border border-card flex items-center justify-center text-muted hover:text-link hover:border-link/30 hover:bg-(--accent-primary)/10 transition-all">
           <ArrowLeft :size="20" />
        </router-link>
        <div>
          <h1 class="text-xl font-semibold text-heading tracking-tight">Thêm câu hỏi mới</h1>
          <p class="text-xs font-medium text-muted mt-1 tracking-wider">Biên soạn nội dung</p>
        </div>
      </div>
      <div class="flex items-center gap-3">
         <button @click="router.push('/teacher/questions')" class="flex items-center gap-2 rounded-2xl surface-card border border-card px-4 py-3 hover:bg-(--color-danger-bg) hover:text-(--color-danger-text) hover:border-(--color-danger-text)/30 transition-colors font-semibold text-sm text-body">
            Hủy bỏ
         </button>
         <button @click="saveQuestion" class="flex items-center gap-2 rounded-2xl bg-(--accent-primary) px-4 py-3 text-sm font-semibold text-inverse shadow-md hover:shadow-lg hover:-translate-y-0.5 transition-all">
            <Save :size="16" /> Lưu câu hỏi
         </button>
      </div>
    </div>

    <!-- ── Main Content ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <!-- Cột Trái (Nội dung & Đáp án) -->
      <div class="lg:col-span-2 space-y-8">
         <div class="lg-glass-soft rounded-2xl p-5 space-y-4">
            <div class="flex items-center gap-3 mb-2 pb-6 border-b border-card">
               <div class="h-10 w-10 rounded-xl bg-(--accent-primary)/10 text-link flex items-center justify-center">
                  <FileText :size="18" />
               </div>
               <h2 class="text-lg font-semibold text-heading">Nội dung câu hỏi</h2>
            </div>

            <!-- Loại câu hỏi -->
            <div class="flex gap-4">
               <label 
                  @click="form.type = 'Trắc nghiệm'"
                  :class="['flex-1 flex flex-col items-center justify-center p-4 rounded-2xl border-2 cursor-pointer transition-all', form.type === 'Trắc nghiệm' ? 'border-(--accent-primary) bg-(--accent-primary)/10' : 'border-card hover:border-default hover:bg-(--accent-primary)/5']">
                  <div :class="['h-4 w-4 rounded-full border-2 mb-2', form.type === 'Trắc nghiệm' ? 'border-4 border-(--accent-primary)' : 'border-muted']"></div>
                  <span :class="['text-sm font-semibold', form.type === 'Trắc nghiệm' ? 'text-(--accent-primary)' : 'text-body']">Trắc nghiệm</span>
               </label>
               <label 
                  @click="form.type = 'Tự luận'"
                  :class="['flex-1 flex flex-col items-center justify-center p-4 rounded-2xl border-2 cursor-pointer transition-all', form.type === 'Tự luận' ? 'border-(--accent-primary) bg-(--accent-primary)/10' : 'border-card hover:border-default hover:bg-(--accent-primary)/5']">
                  <div :class="['h-4 w-4 rounded-full border-2 mb-2', form.type === 'Tự luận' ? 'border-4 border-(--accent-primary)' : 'border-muted']"></div>
                  <span :class="['text-sm font-semibold', form.type === 'Tự luận' ? 'text-(--accent-primary)' : 'text-body']">Tự luận</span>
               </label>
            </div>

            <!-- Editor Câu hỏi -->
            <div>
               <label class="block text-[11px] font-semibold uppercase tracking-widest text-muted mb-2">Đề bài *</label>
               <textarea v-model="form.content" rows="4" placeholder="Nhập câu hỏi ở đây..." class="lg-control w-full resize-none leading-relaxed"></textarea>
            </div>
         </div>

         <!-- Đáp án (Chỉ hiện nếu là Trắc nghiệm) -->
         <div v-if="form.type === 'Trắc nghiệm'" class="lg-glass-soft rounded-2xl p-5">
            <div class="flex items-center justify-between mb-4 pb-6 border-b border-card">
               <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-xl bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center">
                     <CheckCircle2 :size="18" />
                  </div>
                  <h2 class="text-lg font-semibold text-heading">Các phương án đáp án</h2>
               </div>
            </div>

            <div class="space-y-4">
               <div v-for="(opt, index) in options" :key="opt.id" class="flex gap-3 items-center group">
                  <button @click="setCorrectAnswer(index)" 
                          :class="['flex-shrink-0 h-8 w-8 rounded-full border-2 flex items-center justify-center transition-colors', opt.isCorrect ? 'border-(--color-success-text) bg-(--color-success-text) text-inverse' : 'border-default text-transparent hover:border-(--color-success-text)/50 hover:bg-(--color-success-bg)']">
                     <CheckCircle2 :size="14" />
                  </button>
                  <div class="flex-1 relative">
                     <span class="absolute left-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-muted pointer-events-none">{{ String.fromCharCode(65 + index) }}.</span>
                     <input v-model="opt.text" type="text" placeholder="Nhập nội dung đáp án..." 
                            :class="['lg-control w-full pl-10', opt.isCorrect ? 'border-(--color-success-text)/50 focus:border-(--color-success-text)' : '']" />
                  </div>
                  <button @click="removeOption(index)" class="flex-shrink-0 h-10 w-10 rounded-xl surface-card border border-card flex items-center justify-center text-muted hover:bg-(--color-danger-bg) hover:text-(--color-danger-text) hover:border-(--color-danger-text)/30 transition-all opacity-0 group-hover:opacity-100 focus:opacity-100">
                     <Trash2 :size="18" />
                  </button>
               </div>
               
               <button @click="addOption" class="mt-4 w-full rounded-xl border-2 border-dashed border-card py-4 text-sm font-semibold text-muted hover:border-link/30 hover:bg-(--accent-primary)/5 hover:text-link transition-colors flex items-center justify-center gap-2">
                  <Plus :size="18" /> Thêm lựa chọn
               </button>
            </div>
         </div>
         
         <div class="lg-glass-soft rounded-2xl p-5 space-y-4">
            <div>
               <label class="block text-[11px] font-semibold uppercase tracking-widest text-muted mb-2">Giải thích đáp án (Tùy chọn)</label>
               <textarea v-model="form.explanation" rows="3" placeholder="Sẽ hiển thị cho sinh viên sau khi kiểm tra xong..." class="lg-control w-full resize-none"></textarea>
            </div>
         </div>
      </div>

      <!-- Cột Phải (Phân loại & Thuộc tính) -->
      <div class="lg:col-span-1 space-y-8">
         <div class="lg-glass-soft rounded-2xl p-5">
            <div class="flex items-center gap-3 mb-4 pb-6 border-b border-card">
               <div class="h-10 w-10 rounded-xl bg-(--color-warning-bg) text-(--color-warning-text) flex items-center justify-center">
                  <Shield :size="18" />
               </div>
               <h2 class="text-lg font-semibold text-heading">Phân loại</h2>
            </div>

            <div class="space-y-4">
               <div>
                  <label class="block text-[11px] font-semibold uppercase tracking-widest text-muted mb-2">Độ khó</label>
                  <select v-model="form.difficulty" class="lg-control w-full appearance-none cursor-pointer">
                     <option>Dễ</option>
                     <option>Trung bình</option>
                     <option>Khó</option>
                  </select>
               </div>
               
               <div>
                  <label class="block text-[11px] font-semibold uppercase tracking-widest text-muted mb-2">Môn học / Danh mục</label>
                  <select v-model="form.category" class="lg-control w-full appearance-none cursor-pointer">
                     <option>Web Development</option>
                     <option>JavaScript</option>
                     <option>Algorithms</option>
                     <option>Software Engineering</option>
                  </select>
               </div>
            </div>
         </div>
         
         <div class="lg-glass-soft rounded-2xl p-5 bg-(--accent-primary)/5 border-(--accent-primary)/20">
            <div class="flex items-start gap-3">
               <HelpCircle :size="20" class="text-link shrink-0 mt-0.5" />
               <div>
                  <h4 class="text-sm font-semibold text-heading">Mẹo tạo câu hỏi</h4>
                  <ul class="mt-2 space-y-2 text-xs font-medium text-body leading-relaxed list-disc list-inside">
                     <li>Chọn 1 phương án làm đáp án đúng bằng cách click vào hình tròn bên cạnh.</li>
                     <li>Có thể thêm đến 6 lựa chọn cho một câu trắc nghiệm.</li>
                     <li>Viết giải thích rõ ràng giúp sinh viên dễ hiểu khi xem lại kết quả.</li>
                  </ul>
               </div>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>
