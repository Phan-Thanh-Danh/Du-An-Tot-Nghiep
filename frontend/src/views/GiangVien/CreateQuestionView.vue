<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { 
  ArrowLeft, Save, Plus, Trash2, HelpCircle, 
  Layers, Shield, FileText, CheckCircle2
} from 'lucide-vue-next'

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
  <div class="space-y-8 pb-10 text-slate-800 max-w-5xl mx-auto">
    <!-- ── Header ── -->
    <div class="flex items-center justify-between bg-white p-4 rounded-2xl border border-slate-100 shadow-sm sticky top-4 z-10">
      <div class="flex items-center gap-4">
        <router-link to="/teacher/questions" class="h-10 w-10 rounded-2xl bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-white hover:text-blue-600 hover:border-blue-200 transition-colors shadow-sm">
           <ArrowLeft :size="20" />
        </router-link>
        <div>
          <h1 class="text-xl font-black text-slate-900 tracking-tight">Thêm câu hỏi mới</h1>
          <p class="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Biên soạn nội dung</p>
        </div>
      </div>
      <div class="flex items-center gap-3">
         <button @click="router.push('/teacher/questions')" class="flex items-center gap-2 rounded-2xl bg-white px-4 py-3 border border-slate-200 shadow-sm hover:bg-rose-50 hover:text-rose-600 hover:border-rose-200 transition-colors font-bold text-sm text-slate-700">
            Hủy bỏ
         </button>
         <button @click="saveQuestion" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-blue-600 px-4 py-3 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all">
            <Save :size="16" /> Lưu câu hỏi
         </button>
      </div>
    </div>

    <!-- ── Main Content ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <!-- Cột Trái (Nội dung & Đáp án) -->
      <div class="lg:col-span-2 space-y-8">
         <div class="rounded-2xl bg-white border border-slate-100 p-5 shadow-sm space-y-4">
            <div class="flex items-center gap-3 mb-2 pb-6 border-b border-slate-50">
               <div class="h-10 w-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
                  <FileText :size="18" />
               </div>
               <h2 class="text-lg font-black text-slate-900">Nội dung câu hỏi</h2>
            </div>

            <!-- Loại câu hỏi -->
            <div class="flex gap-4">
               <label 
                  @click="form.type = 'Trắc nghiệm'"
                  :class="['flex-1 flex flex-col items-center justify-center p-4 rounded-[20px] border-2 cursor-pointer transition-all', form.type === 'Trắc nghiệm' ? 'border-blue-500 bg-blue-50' : 'border-slate-100 hover:border-slate-200 hover:bg-slate-50']">
                  <div :class="['h-4 w-4 rounded-full border-2 mb-2', form.type === 'Trắc nghiệm' ? 'border-4 border-blue-600' : 'border-slate-300']"></div>
                  <span :class="['text-sm font-bold', form.type === 'Trắc nghiệm' ? 'text-blue-700' : 'text-slate-600']">Trắc nghiệm</span>
               </label>
               <label 
                  @click="form.type = 'Tự luận'"
                  :class="['flex-1 flex flex-col items-center justify-center p-4 rounded-[20px] border-2 cursor-pointer transition-all', form.type === 'Tự luận' ? 'border-blue-500 bg-blue-50' : 'border-slate-100 hover:border-slate-200 hover:bg-slate-50']">
                  <div :class="['h-4 w-4 rounded-full border-2 mb-2', form.type === 'Tự luận' ? 'border-4 border-blue-600' : 'border-slate-300']"></div>
                  <span :class="['text-sm font-bold', form.type === 'Tự luận' ? 'text-blue-700' : 'text-slate-600']">Tự luận</span>
               </label>
            </div>

            <!-- Editor Câu hỏi -->
            <div>
               <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Đề bài *</label>
               <textarea v-model="form.content" rows="4" placeholder="Nhập câu hỏi ở đây..." class="w-full rounded-[24px] border border-slate-200 bg-slate-50 p-4 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm resize-none leading-relaxed"></textarea>
            </div>
         </div>

         <!-- Đáp án (Chỉ hiện nếu là Trắc nghiệm) -->
         <div v-if="form.type === 'Trắc nghiệm'" class="rounded-2xl bg-white border border-slate-100 p-5 shadow-sm">
            <div class="flex items-center justify-between mb-4 pb-6 border-b border-slate-50">
               <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
                     <CheckCircle2 :size="18" />
                  </div>
                  <h2 class="text-lg font-black text-slate-900">Các phương án đáp án</h2>
               </div>
            </div>

            <div class="space-y-4">
               <div v-for="(opt, index) in options" :key="opt.id" class="flex gap-3 items-center group">
                  <button @click="setCorrectAnswer(index)" 
                          :class="['flex-shrink-0 h-8 w-8 rounded-full border-2 flex items-center justify-center transition-colors', opt.isCorrect ? 'border-emerald-500 bg-emerald-500 text-white' : 'border-slate-200 text-transparent hover:border-emerald-200 hover:bg-emerald-50']">
                     <CheckCircle2 :size="14" />
                  </button>
                  <div class="flex-1 relative">
                     <span class="absolute left-4 top-1/2 -translate-y-1/2 text-sm font-black text-slate-300 pointer-events-none">{{ String.fromCharCode(65 + index) }}.</span>
                     <input v-model="opt.text" type="text" placeholder="Nhập nội dung đáp án..." 
                            :class="['w-full rounded-[16px] border bg-white pl-10 pr-4 py-3.5 text-sm font-bold outline-none transition-colors shadow-sm', opt.isCorrect ? 'border-emerald-300 focus:ring-4 focus:ring-emerald-50 text-emerald-900' : 'border-slate-200 focus:border-blue-400 focus:ring-4 focus:ring-blue-50 text-slate-700']" />
                  </div>
                  <button @click="removeOption(index)" class="flex-shrink-0 h-10 w-10 rounded-[16px] bg-white border border-slate-200 flex items-center justify-center text-slate-400 hover:bg-rose-50 hover:text-rose-500 hover:border-rose-200 transition-colors shadow-sm opacity-0 group-hover:opacity-100 focus:opacity-100">
                     <Trash2 :size="18" />
                  </button>
               </div>
               
               <button @click="addOption" class="mt-4 w-full rounded-[16px] border-2 border-dashed border-slate-200 py-4 text-sm font-bold text-slate-500 hover:border-blue-300 hover:bg-blue-50 hover:text-blue-600 transition-colors flex items-center justify-center gap-2">
                  <Plus :size="18" /> Thêm lựa chọn
               </button>
            </div>
         </div>
         
         <div class="rounded-2xl bg-white border border-slate-100 p-5 shadow-sm space-y-4">
            <div>
               <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Giải thích đáp án (Tùy chọn)</label>
               <textarea v-model="form.explanation" rows="3" placeholder="Sẽ hiển thị cho sinh viên sau khi kiểm tra xong..." class="w-full rounded-[24px] border border-slate-200 bg-white p-5 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm resize-none"></textarea>
            </div>
         </div>
      </div>

      <!-- Cột Phải (Phân loại & Thuộc tính) -->
      <div class="lg:col-span-1 space-y-8">
         <div class="rounded-2xl bg-white border border-slate-100 p-5 shadow-sm">
            <div class="flex items-center gap-3 mb-4 pb-6 border-b border-slate-50">
               <div class="h-10 w-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
                  <Shield :size="18" />
               </div>
               <h2 class="text-lg font-black text-slate-900">Phân loại</h2>
            </div>

            <div class="space-y-4">
               <div>
                  <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Độ khó</label>
                  <select v-model="form.difficulty" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 px-5 py-4 text-sm font-bold text-slate-700 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm appearance-none cursor-pointer">
                     <option>Dễ</option>
                     <option>Trung bình</option>
                     <option>Khó</option>
                  </select>
               </div>
               
               <div>
                  <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Môn học / Danh mục</label>
                  <select v-model="form.category" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 px-5 py-4 text-sm font-bold text-slate-700 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm appearance-none cursor-pointer">
                     <option>Web Development</option>
                     <option>JavaScript</option>
                     <option>Algorithms</option>
                     <option>Software Engineering</option>
                  </select>
               </div>
            </div>
         </div>
         
         <div class="rounded-2xl bg-white border border-slate-100 p-5 shadow-sm bg-gradient-to-br from-blue-50 to-blue-50 border-none">
            <div class="flex items-start gap-3">
               <HelpCircle :size="20" class="text-blue-500 shrink-0 mt-0.5" />
               <div>
                  <h4 class="text-sm font-bold text-slate-800">Mẹo tạo câu hỏi</h4>
                  <ul class="mt-2 space-y-2 text-xs font-medium text-slate-600 leading-relaxed list-disc list-inside">
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

<style scoped>
</style>
