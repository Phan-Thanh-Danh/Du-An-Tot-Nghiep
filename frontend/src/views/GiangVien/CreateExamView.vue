<!-- CreateExamView.vue -->
<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { 
  ArrowLeft, Save, Send, Database, Clock, 
  Settings, BookOpen, AlertCircle, Check, Plus,
  X, Search, Trash2, Award, CheckCircle2,
  Info
} from 'lucide-vue-next'

const router = useRouter()

const form = ref({
  name: '',
  description: '',
  type: 'Trắc nghiệm',
  duration: 60,
  passingScore: 5.0,
  shuffle: true,
  showResult: true,
  startTime: '',
  endTime: ''
})

const selectedQuestions = ref([])
const showBankModal = ref(false)
const showCreatorModal = ref(false)

const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

function triggerToast(msg, type = 'success') {
  toast.value.message = msg
  toast.value.type = type
  toast.value.show = true
  setTimeout(() => {
    toast.value.show = false
  }, 4000)
}

// Mock Question Bank
const mockQuestionBank = ref([
  { id: 'Q1', text: 'Thẻ HTML nào được dùng để tạo một liên kết?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Lập trình Web', options: ['<a>', '<link>', '<href>', '<url>'], correctAnswer: 0, score: 0.25 },
  { id: 'Q2', text: 'Trong CSS, thuộc tính nào dùng để căn giữa văn bản?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Lập trình Web', options: ['align: center', 'text-align: center', 'valign: center', 'margin: auto'], correctAnswer: 1, score: 0.25 },
  { id: 'Q3', text: 'Khái niệm "Reactive" trong VueJS dùng để làm gì?', type: 'Trắc nghiệm', difficulty: 'Trung bình', category: 'Lập trình Web', options: ['Tạo biến thông thường', 'Tự động đồng bộ và cập nhật UI khi dữ liệu thay đổi', 'Đọc ghi dữ liệu từ API', 'Quản lý routing của trang'], correctAnswer: 1, score: 0.5 },
  { id: 'Q4', text: 'Để định dạng flexbox container, ta dùng thuộc tính CSS nào?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Lập trình Web', options: ['display: block', 'display: grid', 'display: flex', 'float: left'], correctAnswer: 2, score: 0.25 },
  { id: 'Q5', text: 'Trong Java, từ khóa nào kế thừa một lớp khác?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Lập trình Java', options: ['implements', 'extends', 'inherits', 'import'], correctAnswer: 1, score: 0.25 },
  { id: 'Q6', text: 'Đâu không phải là một kiểu dữ liệu nguyên thủy (primitive) trong Java?', type: 'Trắc nghiệm', difficulty: 'Trung bình', category: 'Lập trình Java', options: ['int', 'double', 'char', 'String'], correctAnswer: 3, score: 0.5 },
  { id: 'Q7', text: 'Lớp cha cao nhất của mọi lớp trong Java là lớp nào?', type: 'Trắc nghiệm', difficulty: 'Trung bình', category: 'Lập trình Java', options: ['Object', 'Class', 'System', 'Main'], correctAnswer: 0, score: 0.5 },
  { id: 'Q8', text: 'Khóa chính (Primary Key) trong cơ sở dữ liệu dùng để làm gì?', type: 'Trắc nghiệm', difficulty: 'Dễ', category: 'Cơ sở dữ liệu', options: ['Để mã hóa bảng dữ liệu', 'Định danh duy nhất cho mỗi bản ghi', 'Liên kết dữ liệu giữa nhiều bảng', 'Để sắp xếp các dòng dữ liệu'], correctAnswer: 1, score: 0.25 },
  { id: 'Q9', text: 'Câu lệnh SQL nào dùng để loại bỏ các hàng trùng lặp?', type: 'Trắc nghiệm', difficulty: 'Trung bình', category: 'Cơ sở dữ liệu', options: ['SELECT UNIQUE', 'SELECT DISTINCT', 'SELECT DIFFERENT', 'SELECT SINGLE'], correctAnswer: 1, score: 0.5 }
])

// Question bank filters
const bankSearch = ref('')
const bankCategoryFilter = ref('')
const bankDifficultyFilter = ref('')
const bankCheckedIds = ref([])

const filteredBankQuestions = computed(() => {
  return mockQuestionBank.value.filter(q => {
    const matchSearch = q.text.toLowerCase().includes(bankSearch.value.toLowerCase())
    const matchCat = !bankCategoryFilter.value || q.category === bankCategoryFilter.value
    const matchDiff = !bankDifficultyFilter.value || q.difficulty === bankDifficultyFilter.value
    // Exclude already added questions to avoid duplicates
    const notAddedYet = !selectedQuestions.value.some(sq => sq.id === q.id)
    return matchSearch && matchCat && matchDiff && notAddedYet
  })
})

function openQuestionBank() {
  bankCheckedIds.value = []
  showBankModal.value = true
}

function toggleQuestionSelection(id) {
  if (bankCheckedIds.value.includes(id)) {
    bankCheckedIds.value = bankCheckedIds.value.filter(item => item !== id)
  } else {
    bankCheckedIds.value.push(id)
  }
}

function addQuestionsFromBank() {
  if (bankCheckedIds.value.length === 0) {
    showBankModal.value = false
    return
  }
  
  bankCheckedIds.value.forEach(id => {
    const question = mockQuestionBank.value.find(q => q.id === id)
    if (question) {
      selectedQuestions.value.push(JSON.parse(JSON.stringify(question)))
    }
  })
  
  triggerToast(`Đã thêm thành công ${bankCheckedIds.value.length} câu hỏi!`, 'success')
  showBankModal.value = false
}

// Inline Creator State
const initialNewQuestion = {
  text: '',
  type: 'Trắc nghiệm',
  difficulty: 'Dễ',
  category: 'Lập trình Web',
  options: ['', '', '', ''],
  correctAnswer: 0,
  score: 0.25
}
const newQuestion = ref(JSON.parse(JSON.stringify(initialNewQuestion)))
const creatorErrors = ref({})

function openQuestionCreator() {
  newQuestion.value = JSON.parse(JSON.stringify(initialNewQuestion))
  creatorErrors.value = {}
  showCreatorModal.value = true
}

function validateNewQuestion() {
  creatorErrors.value = {}
  let isValid = true
  if (!newQuestion.value.text.trim()) {
    creatorErrors.value.text = 'Vui lòng nhập nội dung câu hỏi'
    isValid = false
  }
  if (newQuestion.value.type === 'Trắc nghiệm') {
    newQuestion.value.options.forEach((opt, idx) => {
      if (!opt.trim()) {
        creatorErrors.value[`option_${idx}`] = `Vui lòng nhập phương án ${String.fromCharCode(65 + idx)}`
        isValid = false
      }
    })
  }
  if (newQuestion.value.score <= 0) {
    creatorErrors.value.score = 'Điểm số phải lớn hơn 0'
    isValid = false
  }
  return isValid
}

function createQuestionInline() {
  if (!validateNewQuestion()) {
    return
  }
  
  const created = {
    id: 'CQ_' + Date.now(),
    text: newQuestion.value.text,
    type: newQuestion.value.type,
    difficulty: newQuestion.value.difficulty,
    category: newQuestion.value.category,
    options: newQuestion.value.type === 'Trắc nghiệm' ? [...newQuestion.value.options] : [],
    correctAnswer: newQuestion.value.type === 'Trắc nghiệm' ? newQuestion.value.correctAnswer : null,
    score: newQuestion.value.score
  }
  
  selectedQuestions.value.push(created)
  triggerToast('Đã tạo và thêm câu hỏi mới thành công!', 'success')
  showCreatorModal.value = false
}

function removeQuestion(index) {
  selectedQuestions.value.splice(index, 1)
}

function updateQuestionScore(index, event) {
  const val = parseFloat(event.target.value)
  if (!isNaN(val) && val > 0) {
    selectedQuestions.value[index].score = val
  }
}

// Computed total values
const totalScore = computed(() => {
  return selectedQuestions.value.reduce((acc, q) => acc + (q.score || 0), 0)
})

function saveDraft() {
  if (!form.value.name.trim()) {
    triggerToast('Vui lòng nhập tên đề thi!', 'error')
    return
  }
  
  saveToLocalStorage('Draft')
}

function publish() {
  if (!form.value.name.trim()) {
    triggerToast('Vui lòng nhập tên đề thi!', 'error')
    return
  }
  
  if (selectedQuestions.value.length === 0) {
    triggerToast('Đề thi phải có ít nhất 1 câu hỏi!', 'error')
    return
  }
  
  saveToLocalStorage('Published')
}

function saveToLocalStorage(status) {
  const defaultExams = [
    { id: 1, name: 'Thi giữa kỳ: Lập trình Web', duration: '90 phút', questions: 40, status: 'Published', date: '25/05/2026', type: 'Trắc nghiệm' },
    { id: 2, name: 'Thi kết thúc môn: Java Basic', duration: '120 phút', questions: 50, status: 'Draft', date: '15/06/2026', type: 'Hỗn hợp' },
    { id: 3, name: 'Quiz 2: Cấu trúc dữ liệu', duration: '15 phút', questions: 10, status: 'Published', date: '18/05/2026', type: 'Trắc nghiệm' },
  ]
  
  const stored = localStorage.getItem('teacher_exams')
  const examList = stored ? JSON.parse(stored) : defaultExams
  
  const examId = examList.length + 1
  
  // Format Date
  let examDate = '25/05/2026'
  if (form.value.startTime) {
    const parts = form.value.startTime.split('T')[0].split('-')
    examDate = `${parts[2]}/${parts[1]}/${parts[0]}`
  }
  
  const newExam = {
    id: examId,
    name: form.value.name,
    duration: form.value.duration + ' phút',
    questions: selectedQuestions.value.length,
    status: status,
    date: examDate,
    type: form.value.type
  }
  
  examList.unshift(newExam)
  localStorage.setItem('teacher_exams', JSON.stringify(examList))
  
  triggerToast(`Đã ${status === 'Published' ? 'xuất bản' : 'lưu nháp'} đề thi thành công!`, 'success')
  setTimeout(() => {
    router.push('/teacher/exams')
  }, 1000)
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800 max-w-7xl mx-auto relative">
    
    <!-- Toast Component -->
    <Transition name="toast-slide">
      <div v-if="toast.show" 
           :class="['fixed top-6 right-6 z-[100] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl border backdrop-blur-md transition-all duration-300', 
                    toast.type === 'success' ? 'bg-emerald-500/90 border-emerald-400 text-white' : 'bg-rose-500/90 border-rose-400 text-white']">
        <CheckCircle2 v-if="toast.type === 'success'" :size="20" />
        <AlertCircle v-else :size="20" />
        <p class="text-sm font-bold tracking-wide">{{ toast.message }}</p>
      </div>
    </Transition>

    <!-- ── Header ── -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between bg-white p-6 rounded-[32px] border border-slate-100 shadow-sm sticky top-6 z-10 gap-4">
      <div class="flex items-center gap-4">
        <router-link to="/teacher/exams" class="h-12 w-12 rounded-2xl bg-slate-50 border border-slate-200 flex items-center justify-center text-slate-500 hover:bg-white hover:text-blue-600 hover:border-blue-200 transition-all shadow-sm active:scale-95">
           <ArrowLeft :size="20" />
        </router-link>
        <div>
          <h1 class="text-xl font-black text-slate-900 tracking-tight">Tạo đề thi mới</h1>
          <p class="text-xs font-bold text-slate-400 mt-1 uppercase tracking-widest">Cấu hình & Tạo đề thi tương tác</p>
        </div>
      </div>
      <div class="flex items-center gap-3 self-end sm:self-center">
         <button @click="saveDraft" class="flex items-center gap-2 rounded-2xl bg-white px-5 py-3 border border-slate-200 shadow-sm hover:bg-slate-50 hover:text-blue-600 transition-all font-bold text-sm text-slate-700 active:scale-95">
            <Save :size="16" /> Lưu nháp
         </button>
         <button @click="publish" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-cyan-600 px-6 py-3 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all active:scale-95">
            <Send :size="16" /> Xuất bản đề
         </button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
       <!-- Main Form Area -->
       <div class="lg:col-span-2 space-y-8">
          
          <!-- Thông tin cơ bản -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
                   <BookOpen :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Thông tin cơ bản</h2>
             </div>

             <div class="space-y-6">
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Tên đề thi *</label>
                   <input v-model="form.name" type="text" placeholder="Ví dụ: Kiểm tra giữa kỳ môn Cấu trúc dữ liệu & Giải thuật..." class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-4 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
                </div>
                
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Mô tả / Hướng dẫn làm bài</label>
                   <textarea v-model="form.description" rows="3" placeholder="Nhập quy chế, lưu ý và hướng dẫn cho sinh viên trước khi làm bài..." class="w-full rounded-[16px] border border-slate-200 bg-white px-5 py-4 text-sm font-medium text-slate-850 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm resize-none leading-relaxed"></textarea>
                </div>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                   <div>
                      <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Loại đề thi</label>
                      <div class="relative">
                        <select v-model="form.type" class="w-full rounded-[16px] border border-slate-200 bg-slate-50 px-5 py-4 text-sm font-bold text-slate-650 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm appearance-none cursor-pointer">
                           <option>Trắc nghiệm</option>
                           <option>Tự luận</option>
                           <option>Hỗn hợp</option>
                        </select>
                        <Settings :size="14" class="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                      </div>
                   </div>
                   <div>
                      <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Thời gian làm bài (Phút)</label>
                      <div class="relative">
                         <Clock :size="16" class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" />
                         <input v-model.number="form.duration" type="number" min="5" max="300" class="w-full rounded-[16px] border border-slate-200 bg-white pl-12 pr-5 py-4 text-sm font-bold text-slate-800 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
                      </div>
                   </div>
                </div>
             </div>
          </div>

          <!-- Nội dung & Câu hỏi đề thi -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center justify-between mb-6 pb-6 border-b border-slate-50">
                <div class="flex items-center gap-3">
                   <div class="h-10 w-10 rounded-xl bg-cyan-50 text-cyan-600 flex items-center justify-center">
                      <Database :size="18" />
                   </div>
                   <div>
                     <h2 class="text-lg font-black text-slate-900">Câu hỏi trong đề thi</h2>
                     <p class="text-xs font-semibold text-slate-400 mt-0.5">Xây dựng danh sách câu hỏi trắc nghiệm hoặc tự luận.</p>
                   </div>
                </div>
                <!-- Dynamic Indicators -->
                <div v-if="selectedQuestions.length > 0" class="flex gap-4">
                  <div class="text-right">
                    <p class="text-[9px] font-black uppercase tracking-widest text-slate-400">Số câu</p>
                    <p class="text-xs font-bold text-slate-800">{{ selectedQuestions.length }} câu</p>
                  </div>
                  <div class="w-px bg-slate-100"></div>
                  <div class="text-right">
                    <p class="text-[9px] font-black uppercase tracking-widest text-slate-400">Tổng điểm</p>
                    <p class="text-xs font-black text-indigo-650">{{ totalScore.toFixed(2) }} đ</p>
                  </div>
                </div>
             </div>

             <!-- Questions list inside exam -->
             <div v-if="selectedQuestions.length > 0" class="space-y-4 mb-6">
               <div 
                 v-for="(q, index) in selectedQuestions" 
                 :key="q.id"
                 class="group/item rounded-2xl border border-slate-100 p-5 bg-slate-50/30 hover:bg-white hover:border-blue-200 transition-all shadow-sm"
               >
                 <div class="flex justify-between items-start gap-4">
                   <div class="flex items-start gap-3.5 flex-1">
                     <span class="h-8 w-8 rounded-lg bg-slate-150/60 font-black text-xs text-slate-500 flex items-center justify-center shrink-0">
                       {{ index + 1 }}
                     </span>
                     <div class="space-y-3 flex-1 min-w-0">
                       <p class="text-sm font-bold text-slate-850 leading-snug">{{ q.text }}</p>
                       
                       <!-- Options rendering -->
                       <div v-if="q.options && q.options.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs">
                         <div 
                           v-for="(opt, oIdx) in q.options" 
                           :key="oIdx"
                           :class="['p-2.5 rounded-xl border flex items-center gap-2', 
                                    oIdx === q.correctAnswer ? 'bg-emerald-50/40 border-emerald-200 text-emerald-800 font-semibold' : 'bg-white border-slate-100 text-slate-600']"
                         >
                           <span class="h-5 w-5 rounded bg-slate-100 text-[10px] font-black text-slate-400 flex items-center justify-center uppercase shrink-0">
                             {{ String.fromCharCode(65 + oIdx) }}
                           </span>
                           <span class="truncate">{{ opt }}</span>
                           <Check v-if="oIdx === q.correctAnswer" :size="12" class="text-emerald-500 ml-auto shrink-0" />
                         </div>
                       </div>

                       <!-- Meta data -->
                       <div class="flex items-center gap-3 pt-1">
                         <span class="rounded bg-indigo-50 px-2 py-0.5 text-[9px] font-black text-indigo-600 uppercase tracking-wide">
                           {{ q.category }}
                         </span>
                         <span :class="['rounded px-2 py-0.5 text-[9px] font-black uppercase tracking-wide',
                                       q.difficulty === 'Dễ' ? 'bg-emerald-50 text-emerald-600' : q.difficulty === 'Khó' ? 'bg-rose-50 text-rose-600' : 'bg-amber-50 text-amber-600']">
                           {{ q.difficulty }}
                         </span>
                       </div>
                     </div>
                   </div>

                   <!-- Actions right side -->
                   <div class="flex items-center gap-3 shrink-0">
                     <!-- Score Input -->
                     <div class="flex items-center gap-1.5 bg-white border border-slate-200 rounded-xl px-2.5 py-1.5 shadow-sm">
                       <Award :size="12" class="text-slate-450" />
                       <input 
                         type="number" 
                         step="0.05"
                         min="0.05"
                         :value="q.score"
                         @input="updateQuestionScore(index, $event)"
                         class="w-10 text-xs font-bold text-center outline-none text-slate-800"
                       />
                       <span class="text-[9px] font-bold text-slate-400">điểm</span>
                     </div>

                     <!-- Delete button -->
                     <button 
                       @click="removeQuestion(index)"
                       class="p-2 text-slate-350 hover:text-rose-500 rounded-xl hover:bg-rose-50 transition-colors"
                       title="Xóa câu hỏi"
                     >
                       <Trash2 :size="16" />
                     </button>
                   </div>
                 </div>
               </div>
             </div>

             <!-- Control action cards -->
             <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
               <!-- Select from Bank -->
               <div 
                 @click="openQuestionBank"
                 class="border-2 border-dashed border-slate-200 rounded-3xl p-6 bg-slate-50/20 hover:bg-indigo-50/25 hover:border-indigo-400 text-center cursor-pointer transition-all duration-300 flex flex-col items-center justify-center group"
               >
                 <div class="h-12 w-12 rounded-2xl bg-white text-indigo-600 flex items-center justify-center mb-3 shadow-sm border border-slate-100 group-hover:scale-110 transition-transform">
                   <Database :size="20" />
                 </div>
                 <h4 class="text-xs font-black text-slate-750">Chọn từ Thư viện câu hỏi</h4>
                 <p class="text-[10px] font-semibold text-slate-400 mt-1 max-w-[200px] leading-relaxed">Tìm kiếm và sử dụng câu hỏi có sẵn từ ngân hàng đề.</p>
               </div>

               <!-- Add custom question inline -->
               <div 
                 @click="openQuestionCreator"
                 class="border-2 border-dashed border-slate-200 rounded-3xl p-6 bg-slate-50/20 hover:bg-cyan-50/25 hover:border-cyan-400 text-center cursor-pointer transition-all duration-300 flex flex-col items-center justify-center group"
               >
                 <div class="h-12 w-12 rounded-2xl bg-white text-cyan-600 flex items-center justify-center mb-3 shadow-sm border border-slate-100 group-hover:scale-110 transition-transform">
                   <Plus :size="20" />
                 </div>
                 <h4 class="text-xs font-black text-slate-750">Tự soạn câu hỏi mới</h4>
                 <p class="text-[10px] font-semibold text-slate-400 mt-1 max-w-[200px] leading-relaxed">Tạo câu hỏi trắc nghiệm hoặc tự luận tùy chỉnh tại đây.</p>
               </div>
             </div>
          </div>

       </div>

       <!-- Sidebar Area -->
       <div class="lg:col-span-1 space-y-8">
          
          <!-- Cấu hình thi nâng cao -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-slate-100 text-slate-650 flex items-center justify-center border border-slate-150/40">
                   <Settings :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Cấu hình đề thi</h2>
             </div>

             <div class="space-y-5">
                <!-- Toggle Item -->
                <div class="flex items-center justify-between">
                   <div>
                      <p class="text-sm font-bold text-slate-800">Xáo trộn câu hỏi</p>
                      <p class="text-[10px] font-semibold text-slate-400 mt-0.5">Xáo ngẫu nhiên vị trí câu hỏi</p>
                   </div>
                   <button 
                     type="button"
                     @click="form.shuffle = !form.shuffle" 
                     :class="['w-11 h-6 rounded-full transition-colors relative focus:outline-none shrink-0', 
                              form.shuffle ? 'bg-indigo-650' : 'bg-slate-200']"
                   >
                     <div :class="['h-5 w-5 rounded-full bg-white shadow-sm transition-transform absolute top-0.5 left-0.5', 
                                  form.shuffle ? 'translate-x-5' : 'translate-x-0']"></div>
                   </button>
                </div>

                <!-- Toggle Item -->
                <div class="flex items-center justify-between pt-4 border-t border-slate-50">
                   <div>
                      <p class="text-sm font-bold text-slate-800">Hiển thị kết quả</p>
                      <p class="text-[10px] font-semibold text-slate-400 mt-0.5">Sinh viên xem điểm ngay sau nộp</p>
                   </div>
                   <button 
                     type="button"
                     @click="form.showResult = !form.showResult" 
                     :class="['w-11 h-6 rounded-full transition-colors relative focus:outline-none shrink-0', 
                              form.showResult ? 'bg-indigo-650' : 'bg-slate-200']"
                   >
                     <div :class="['h-5 w-5 rounded-full bg-white shadow-sm transition-transform absolute top-0.5 left-0.5', 
                                  form.showResult ? 'translate-x-5' : 'translate-x-0']"></div>
                   </button>
                </div>

                <!-- Score setting -->
                <div class="pt-4 border-t border-slate-50">
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Điểm đạt tối thiểu (Pass)</label>
                   <div class="relative">
                     <Award :size="14" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
                     <input v-model.number="form.passingScore" type="number" step="0.5" max="10" min="1" class="w-full rounded-[16px] border border-slate-200 bg-white pl-10 pr-4 py-3 text-sm font-bold text-slate-800 outline-none focus:border-indigo-400 transition-colors shadow-sm" />
                   </div>
                </div>
             </div>
          </div>

          <!-- Lịch thi -->
          <div class="rounded-[32px] bg-white border border-slate-100 p-8 shadow-sm">
             <div class="flex items-center gap-3 mb-6 pb-6 border-b border-slate-50">
                <div class="h-10 w-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center border border-amber-100/50">
                   <Clock :size="18" />
                </div>
                <h2 class="text-lg font-black text-slate-900">Thời gian mở đề</h2>
             </div>

             <div class="space-y-4">
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Bắt đầu mở</label>
                   <input v-model="form.startTime" type="datetime-local" class="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-xs font-bold text-slate-650 outline-none focus:border-amber-400 transition-colors shadow-sm cursor-pointer" />
                </div>
                <div>
                   <label class="block text-[11px] font-black uppercase tracking-widest text-slate-400 mb-2">Tự động đóng</label>
                   <input v-model="form.endTime" type="datetime-local" class="w-full rounded-[16px] border border-slate-200 bg-white px-4 py-3 text-xs font-bold text-slate-650 outline-none focus:border-amber-400 transition-colors shadow-sm cursor-pointer" />
                </div>
                
                <div class="mt-4 p-4 rounded-2xl bg-slate-50 border border-slate-100 flex items-start gap-3">
                   <Info :size="16" class="text-slate-400 shrink-0 mt-0.5" />
                   <p class="text-[10px] font-bold text-slate-450 leading-relaxed">Nếu bỏ trống thời gian, đề thi sẽ được mở không thời hạn cho đến khi bạn khóa thủ công.</p>
                </div>
             </div>
          </div>
       </div>
    </div>

    <!-- Question Bank Selector Modal -->
    <div v-if="showBankModal" class="fixed inset-0 z-[80] flex items-center justify-center p-4">
      <div @click="showBankModal = false" class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity duration-300"></div>
      
      <div class="relative w-full max-w-4xl bg-white rounded-[32px] shadow-2xl border border-slate-100 flex flex-col max-h-[85vh] overflow-hidden transform transition-all duration-300 scale-100 animate-modal-in">
        
        <!-- Header -->
        <div class="p-6 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-xl bg-indigo-50 text-indigo-600 flex items-center justify-center border border-indigo-100/50">
               <Database :size="20" />
            </div>
            <div>
               <h3 class="text-lg font-black text-slate-800">Thư viện câu hỏi</h3>
               <p class="text-xs font-semibold text-slate-400 mt-0.5">Chọn từ Ngân hàng đề thi môn học hiện có.</p>
            </div>
          </div>
          <button @click="showBankModal = false" class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-450 hover:bg-rose-50 hover:text-rose-500 hover:rotate-90 transition-all duration-300">
             <X :size="16" />
          </button>
        </div>

        <!-- Toolbar Filters -->
        <div class="p-5 border-b border-slate-50 bg-white grid grid-cols-1 md:grid-cols-3 gap-3">
          <div class="relative">
            <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
            <input 
              v-model="bankSearch"
              type="text" 
              placeholder="Tìm nội dung câu hỏi..." 
              class="w-full rounded-xl border border-slate-200 bg-slate-50/50 pl-9 pr-4 py-2.5 text-xs font-semibold outline-none focus:border-indigo-400 focus:bg-white transition-all text-slate-800"
            />
          </div>
          <select 
            v-model="bankCategoryFilter"
            class="rounded-xl border border-slate-200 bg-slate-50/50 px-3 py-2.5 text-xs font-bold outline-none cursor-pointer focus:border-indigo-400 focus:bg-white text-slate-650"
          >
            <option value="">Tất cả các môn</option>
            <option>Lập trình Web</option>
            <option>Lập trình Java</option>
            <option>Cơ sở dữ liệu</option>
          </select>
          <select 
            v-model="bankDifficultyFilter"
            class="rounded-xl border border-slate-200 bg-slate-50/50 px-3 py-2.5 text-xs font-bold outline-none cursor-pointer focus:border-indigo-400 focus:bg-white text-slate-650"
          >
            <option value="">Tất cả độ khó</option>
            <option>Dễ</option>
            <option>Trung bình</option>
            <option>Khó</option>
          </select>
        </div>

        <!-- Questions List Body -->
        <div class="overflow-y-auto p-6 bg-slate-50/30 flex-1 space-y-3.5">
          <div 
            v-for="q in filteredBankQuestions" 
            :key="q.id"
            @click="toggleQuestionSelection(q.id)"
            :class="['rounded-2xl border p-4 cursor-pointer transition-all duration-200 flex items-start gap-4', 
                     bankCheckedIds.includes(q.id) ? 'bg-indigo-50/30 border-indigo-350 shadow-sm' : 'bg-white border-slate-100 hover:border-slate-300']"
          >
            <!-- Checkbox wrapper -->
            <div class="flex items-center shrink-0 mt-0.5">
              <div :class="['h-5 w-5 rounded border flex items-center justify-center transition-all', 
                            bankCheckedIds.includes(q.id) ? 'bg-indigo-600 border-indigo-600 text-white' : 'border-slate-300 bg-white']">
                <Check v-if="bankCheckedIds.includes(q.id)" :size="12" />
              </div>
            </div>
            
            <div class="space-y-2 flex-1 min-w-0">
              <p class="text-xs font-bold text-slate-800 leading-snug">{{ q.text }}</p>
              
              <!-- Option tags preview -->
              <div v-if="q.options" class="flex flex-wrap gap-2 text-[10px] text-slate-500 font-semibold">
                <span v-for="(opt, idx) in q.options" :key="idx" class="px-2 py-1 bg-slate-50 border border-slate-100 rounded-lg">
                  {{ String.fromCharCode(65 + idx) }}. {{ opt }}
                </span>
              </div>
              
              <!-- Badge information -->
              <div class="flex items-center gap-3">
                <span class="rounded bg-indigo-50 px-2 py-0.5 text-[8px] font-black text-indigo-600 uppercase tracking-wide">{{ q.category }}</span>
                <span :class="['rounded px-2 py-0.5 text-[8px] font-black uppercase tracking-wide', 
                              q.difficulty === 'Dễ' ? 'bg-emerald-50 text-emerald-600' : q.difficulty === 'Khó' ? 'bg-rose-50 text-rose-600' : 'bg-amber-50 text-amber-600']">
                  {{ q.difficulty }}
                </span>
                <span class="text-[9px] font-semibold text-slate-400">{{ q.score }} đ</span>
              </div>
            </div>
          </div>
          
          <!-- Empty state -->
          <div v-if="filteredBankQuestions.length === 0" class="text-center py-12 text-slate-350 flex flex-col items-center">
            <Database :size="40" class="text-slate-200 mb-2" />
            <p class="text-xs font-bold">Không tìm thấy câu hỏi phù hợp</p>
            <p class="text-[10px]">Tất cả câu hỏi trong bộ lọc đã được thêm vào đề thi hoặc không khớp từ khóa.</p>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-5 border-t border-slate-100 flex justify-end gap-3 bg-white">
          <button @click="showBankModal = false" class="rounded-xl border border-slate-200 px-4 py-2.5 text-xs font-bold text-slate-500 hover:bg-slate-50 transition-colors">
            Hủy
          </button>
          <button 
            @click="addQuestionsFromBank"
            class="rounded-xl bg-indigo-600 px-5 py-2.5 text-xs font-bold text-white shadow-md shadow-indigo-100 hover:bg-indigo-700 transition-colors flex items-center gap-1.5"
          >
            Thêm đã chọn <span class="bg-indigo-700/50 rounded-md px-1.5 py-0.5 text-[9px] font-black">{{ bankCheckedIds.length }}</span>
          </button>
        </div>

      </div>
    </div>

    <!-- Inline Creator Modal -->
    <div v-if="showCreatorModal" class="fixed inset-0 z-[80] flex items-center justify-center p-4">
      <div @click="showCreatorModal = false" class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity duration-300"></div>
      
      <div class="relative w-full max-w-2xl bg-white rounded-[32px] shadow-2xl border border-slate-100 flex flex-col max-h-[90vh] overflow-hidden transform transition-all duration-300 scale-100 animate-modal-in">
        
        <!-- Header -->
        <div class="p-6 border-b border-slate-100 flex justify-between items-center bg-slate-50/50">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-xl bg-cyan-50 text-cyan-600 flex items-center justify-center border border-cyan-100/50">
               <Plus :size="20" />
            </div>
            <div>
               <h3 class="text-lg font-black text-slate-800">Soạn câu hỏi mới</h3>
               <p class="text-xs font-semibold text-slate-400 mt-0.5">Soạn nội dung và cấu hình phương án chi tiết.</p>
            </div>
          </div>
          <button @click="showCreatorModal = false" class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-slate-450 hover:bg-rose-50 hover:text-rose-500 hover:rotate-90 transition-all duration-300">
             <X :size="16" />
          </button>
        </div>

        <!-- Body Form -->
        <div class="overflow-y-auto p-6 md:p-8 space-y-5 bg-slate-50/20 flex-1">
          <!-- Question text -->
          <div class="space-y-2">
            <label class="text-xs font-bold text-slate-700">Nội dung câu hỏi *</label>
            <textarea 
              v-model="newQuestion.text"
              rows="3"
              placeholder="Nhập câu hỏi tại đây..."
              :class="['w-full rounded-2xl border bg-white p-4 text-xs font-semibold outline-none focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850 resize-none leading-relaxed', 
                       creatorErrors.text ? 'border-rose-400' : 'border-slate-200']"
            ></textarea>
            <p v-if="creatorErrors.text" class="text-[10px] font-bold text-rose-500 flex items-center gap-1"><AlertCircle :size="10" /> {{ creatorErrors.text }}</p>
          </div>

          <!-- Type & Category & Score row -->
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Loại câu hỏi</label>
              <select v-model="newQuestion.type" class="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-xs font-bold outline-none cursor-pointer text-slate-700">
                <option>Trắc nghiệm</option>
                <option>Tự luận</option>
              </select>
            </div>
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Chủ đề môn học</label>
              <select v-model="newQuestion.category" class="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-xs font-bold outline-none cursor-pointer text-slate-700">
                <option>Lập trình Web</option>
                <option>Lập trình Java</option>
                <option>Cơ sở dữ liệu</option>
              </select>
            </div>
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Điểm số</label>
              <input v-model.number="newQuestion.score" type="number" step="0.05" min="0.05" class="w-full rounded-2xl border border-slate-200 bg-white px-4 py-3 text-xs font-bold outline-none text-slate-800" />
            </div>
          </div>

          <!-- Options Builder (Trắc nghiệm only) -->
          <div v-if="newQuestion.type === 'Trắc nghiệm'" class="space-y-3 pt-3 border-t border-slate-100">
            <label class="text-xs font-bold text-slate-700 block mb-1">Thiết lập các phương án và chọn câu trả lời đúng:</label>
            
            <div 
              v-for="(opt, idx) in newQuestion.options" 
              :key="idx"
              class="flex items-center gap-3"
            >
              <!-- Select Correct Answer -->
              <button 
                type="button"
                @click="newQuestion.correctAnswer = idx"
                :class="['h-6 w-6 rounded-full border flex items-center justify-center shrink-0 transition-colors', 
                         newQuestion.correctAnswer === idx ? 'bg-emerald-500 border-emerald-500 text-white' : 'border-slate-300 hover:border-slate-400 bg-white']"
                title="Đặt làm đáp án đúng"
              >
                <Check v-if="newQuestion.correctAnswer === idx" :size="14" />
              </button>
              
              <!-- Input text -->
              <div class="flex-1 relative">
                <span class="absolute left-4 top-1/2 -translate-y-1/2 text-[10px] font-black text-slate-400 uppercase">
                  {{ String.fromCharCode(65 + idx) }}
                </span>
                <input 
                  v-model="newQuestion.options[idx]"
                  type="text" 
                  :placeholder="`Phương án ${String.fromCharCode(65 + idx)}...`"
                  :class="['w-full rounded-xl border bg-white pl-9 pr-4 py-3 text-xs font-semibold outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all text-slate-800', 
                           creatorErrors[`option_${idx}`] ? 'border-rose-400' : 'border-slate-200']"
                />
              </div>
            </div>
            <!-- Display option errors if any -->
            <p v-if="creatorErrors.option_0 || creatorErrors.option_1 || creatorErrors.option_2 || creatorErrors.option_3" class="text-[10px] font-bold text-rose-500 flex items-center gap-1"><AlertCircle :size="10" /> Vui lòng điền đầy đủ các phương án trắc nghiệm.</p>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-5 border-t border-slate-100 flex justify-end gap-3 bg-white">
          <button @click="showCreatorModal = false" class="rounded-xl border border-slate-200 px-4 py-2.5 text-xs font-bold text-slate-500 hover:bg-slate-50 transition-colors">
            Hủy
          </button>
          <button 
            @click="createQuestionInline"
            class="rounded-xl bg-cyan-600 px-5 py-2.5 text-xs font-bold text-white shadow-md shadow-cyan-100 hover:bg-cyan-700 transition-colors flex items-center gap-1.5"
          >
            Lưu câu hỏi
          </button>
        </div>

      </div>
    </div>

  </div>
</template>

<style scoped>
/* Modal slide/fade animations */
@keyframes modal-in {
  from {
    opacity: 0;
    transform: scale(0.96) translateY(15px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}
.animate-modal-in {
  animation: modal-in 0.35s cubic-bezier(0.16, 1, 0.3, 1) both;
}

/* Toast animations */
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from {
  transform: translateY(-20px) scale(0.9);
  opacity: 0;
}
.toast-slide-leave-to {
  transform: translateY(20px) scale(0.9);
  opacity: 0;
}

.text-slate-850 {
  color: #1e293b;
}
.text-slate-750 {
  color: #334155;
}
.text-slate-650 {
  color: #475569;
}
.text-slate-450 {
  color: #94a3b8;
}
.text-slate-350 {
  color: #cbd5e1;
}
.text-slate-150\/60 {
  background-color: rgba(226, 232, 240, 0.6);
}
.text-indigo-650 {
  color: #4f46e5;
}
.bg-indigo-650 {
  background-color: #4f46e5;
}
</style>
