<!-- CreateExamView.vue -->
<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { 
  ArrowLeft, Save, Send, Database, Clock, 
  Settings, BookOpen, AlertCircle, Check, Plus,
  X, Search, Trash2, Award, CheckCircle2,
  Info, Users, ShieldCheck
} from 'lucide-vue-next'
import { EXAM_STATUS } from '@/utils/examAccess.js'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const router = useRouter()

const form = ref({
  name: '',
  description: '',
  subjectName: 'Lập trình Web',
  classSectionCode: 'WEB201-SD1904-B1',
  type: 'Trắc nghiệm',
  duration: 60,
  passingScore: 5.0,
  maxAttempts: 1,
  shuffle: true,
  showResult: true,
  allowEarlyLearning: false,
  status: EXAM_STATUS.SCHEDULED,
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
    { id: 1, name: 'Thi giữa kỳ: Lập trình Web', subjectName: 'Lập trình Web', classSectionCode: 'WEB201-SD1904-B1', duration: '90 phút', questions: 40, status: EXAM_STATUS.OPEN, date: '04/06/2026', openAt: '2026-06-04T08:00', closeAt: '2026-06-04T10:00', type: 'Trắc nghiệm', maxAttempts: 1, allowedStudents: 42, completedStudents: 31, pendingStudents: 11, allowEarlyLearning: false },
    { id: 2, name: 'Thi kết thúc môn: Java Basic', subjectName: 'Java Basic', classSectionCode: 'JAVA101-SD1905-B2', duration: '120 phút', questions: 50, status: EXAM_STATUS.DRAFT, date: '15/06/2026', openAt: '2026-06-15T08:00', closeAt: '2026-06-15T10:00', type: 'Hỗn hợp', maxAttempts: 1, allowedStudents: 38, completedStudents: 0, pendingStudents: 38, allowEarlyLearning: false },
    { id: 3, name: 'Quiz 2: Cấu trúc dữ liệu', subjectName: 'Cấu trúc dữ liệu', classSectionCode: 'CTDL101-SD1904-B1', duration: '15 phút', questions: 10, status: EXAM_STATUS.RESULT_PUBLISHED, date: '18/05/2026', openAt: '2026-05-18T08:00', closeAt: '2026-05-18T23:59', type: 'Trắc nghiệm', maxAttempts: 2, allowedStudents: 42, completedStudents: 42, pendingStudents: 0, allowEarlyLearning: true },
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
    subjectName: form.value.subjectName,
    classSectionCode: form.value.classSectionCode,
    duration: form.value.duration + ' phút',
    questions: selectedQuestions.value.length,
    // Phase 7: exam access is controlled by schedule/class/attempt, not password.
    status: status === 'Published' ? form.value.status : EXAM_STATUS.DRAFT,
    date: examDate,
    openAt: form.value.startTime,
    closeAt: form.value.endTime,
    type: form.value.type,
    maxAttempts: form.value.maxAttempts,
    allowedStudents: 40,
    completedStudents: 0,
    pendingStudents: 40,
    allowEarlyLearning: form.value.allowEarlyLearning,
    accessPolicy: {
      requirePassword: false,
      controlledBy: 'class_section_schedule_attempt',
    },
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
  <div class="lg-page-enter space-y-5 pb-6 max-w-7xl mx-auto">

    <!-- Toast -->
    <Transition name="toast-slide">
      <div v-if="toast.show"
           class="fixed top-4 right-6 z-50 flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-bold text-white shadow-lg border"
           :style="{ background: toast.type === 'success' ? 'var(--lg-success)' : 'var(--lg-danger)', borderColor: toast.type === 'success' ? 'color-mix(in srgb, var(--lg-success) 40%, transparent)' : 'color-mix(in srgb, var(--lg-danger) 40%, transparent)' }">
        <CheckCircle2 v-if="toast.type === 'success'" :size="18" />
        <AlertCircle v-else :size="18" />
        <p>{{ toast.message }}</p>
      </div>
    </Transition>

    <!-- Header -->
    <GlassPanel variant="flat" density="compact">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <router-link to="/teacher/exams"
            class="flex h-8 w-8 items-center justify-center rounded-lg border border-card bg-surface-input text-muted transition-colors hover:border-link hover:text-link">
            <ArrowLeft :size="17" />
          </router-link>
          <div>
            <h1 class="text-lg font-semibold text-heading tracking-tight">Tạo đề thi mới</h1>
            <p class="text-xs font-semibold text-muted mt-0.5">Cấu hình &amp; tạo đề thi tương tác</p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <GlassButton variant="secondary" size="sm" @click="saveDraft">
            <template #leading><Save :size="14" /></template>
            Lưu nháp
          </GlassButton>
          <GlassButton variant="primary" size="sm" @click="publish">
            <template #leading><Send :size="14" /></template>
            Xuất bản đề
          </GlassButton>
        </div>
      </div>
    </GlassPanel>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <!-- Main Form -->
      <div class="lg:col-span-2 space-y-5">

        <!-- Section 1: Thông tin đề thi -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2.5">
              <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--accent-primary-soft);color:var(--accent-primary)">
                <BookOpen :size="15" />
              </div>
              <h2 class="text-sm font-semibold text-heading">1. Thông tin đề thi</h2>
            </div>
          </template>
          <div class="space-y-3.5">
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Tên đề thi <span style="color:var(--color-danger-text)">*</span></label>
              <input v-model="form.name" type="text" placeholder="Ví dụ: Kiểm tra giữa kỳ môn Cấu trúc dữ liệu &amp; Giải thuật..."
                class="w-full rounded-lg border border-input bg-surface-input px-3.5 py-2.5 text-sm text-heading outline-none placeholder:text-placeholder focus:border-link transition-colors" />
            </div>
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Mô tả / Hướng dẫn làm bài</label>
              <textarea v-model="form.description" rows="3" placeholder="Nhập quy chế, lưu ý và hướng dẫn cho sinh viên trước khi làm bài..."
                class="w-full rounded-lg border border-input bg-surface-input px-3.5 py-2.5 text-sm text-body outline-none placeholder:text-placeholder focus:border-link transition-colors resize-none leading-relaxed"></textarea>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Môn học</label>
                <div class="relative">
                  <BookOpen :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted pointer-events-none" />
                  <input v-model="form.subjectName" type="text"
                    class="w-full rounded-lg border border-input bg-surface-input pl-9 pr-3.5 py-2.5 text-sm text-heading outline-none focus:border-link transition-colors" />
                </div>
              </div>
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Lớp học phần áp dụng</label>
                <div class="relative">
                  <Users :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted pointer-events-none" />
                  <input v-model="form.classSectionCode" type="text"
                    class="w-full rounded-lg border border-input bg-surface-input pl-9 pr-3.5 py-2.5 text-sm text-heading outline-none focus:border-link transition-colors" />
                </div>
              </div>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Loại đề thi</label>
                <div class="relative">
                  <select v-model="form.type"
                    class="w-full rounded-lg border border-input bg-surface-input px-3.5 py-2.5 text-sm text-heading outline-none focus:border-link transition-colors appearance-none cursor-pointer">
                    <option>Trắc nghiệm</option>
                    <option>Tự luận</option>
                    <option>Hỗn hợp</option>
                  </select>
                  <Settings :size="13" class="absolute right-3 top-1/2 -translate-y-1/2 text-muted pointer-events-none" />
                </div>
              </div>
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Thời gian làm bài (Phút)</label>
                <div class="relative">
                  <Clock :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted pointer-events-none" />
                  <input v-model.number="form.duration" type="number" min="5" max="300"
                    class="w-full rounded-lg border border-input bg-surface-input pl-9 pr-3.5 py-2.5 text-sm text-heading outline-none focus:border-link transition-colors" />
                </div>
              </div>
            </div>
          </div>
        </GlassPanel>

        <!-- Section 2: Câu hỏi trong đề thi -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center justify-between flex-wrap gap-2">
              <div class="flex items-center gap-2.5">
                <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--color-info-bg);color:var(--color-info-text)">
                  <Database :size="15" />
                </div>
                <div>
                  <h2 class="text-sm font-semibold text-heading">2. Câu hỏi trong đề thi</h2>
                  <p class="text-xs text-muted mt-0.5">Xây dựng danh sách câu hỏi trắc nghiệm hoặc tự luận.</p>
                </div>
              </div>
              <div v-if="selectedQuestions.length > 0" class="flex items-center gap-3 text-xs">
                <div class="text-right">
                  <p class="text-[10px] font-semibold text-muted uppercase tracking-wider">Số câu</p>
                  <p class="text-sm font-semibold text-heading">{{ selectedQuestions.length }} câu</p>
                </div>
                <div class="h-8 w-px bg-border-card"></div>
                <div class="text-right">
                  <p class="text-[10px] font-semibold text-muted uppercase tracking-wider">Tổng điểm</p>
                  <p class="text-sm font-semibold text-body">{{ totalScore.toFixed(2) }} đ</p>
                </div>
              </div>
            </div>
          </template>

          <!-- Question list -->
          <div v-if="selectedQuestions.length > 0" class="space-y-3 mb-3">
            <div v-for="(q, index) in selectedQuestions" :key="q.id"
              class="rounded-lg border border-card bg-surface-input p-3.5 transition-colors hover:bg-surface-card-hover">
              <div class="flex items-start justify-between gap-3">
                <div class="flex items-start gap-2.5 flex-1 min-w-0">
                  <span class="flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-xs font-bold" style="background:var(--surface-elevated);color:var(--text-muted)">
                    {{ index + 1 }}
                  </span>
                  <div class="space-y-2 flex-1 min-w-0">
                    <p class="text-sm font-semibold text-heading leading-snug">{{ q.text }}</p>
                    <div v-if="q.options && q.options.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-1.5">
                      <div v-for="(opt, oIdx) in q.options" :key="oIdx"
                        class="flex items-center gap-1.5 rounded-md border px-2.5 py-1.5 text-xs"
                        :style="oIdx === q.correctAnswer ? { background: 'var(--color-success-bg)', borderColor: 'color-mix(in srgb, var(--color-success-text) 30%, transparent)', color: 'var(--color-success-text)' } : { background: 'var(--surface-elevated)', borderColor: 'var(--border-card)', color: 'var(--text-body)' }">
                        <span class="flex h-4 w-4 shrink-0 items-center justify-center rounded text-[9px] font-bold uppercase" :style="{ background: oIdx === q.correctAnswer ? 'var(--color-success-text)' : 'var(--surface-input)', color: oIdx === q.correctAnswer ? '#fff' : 'var(--text-muted)' }">{{ String.fromCharCode(65 + oIdx) }}</span>
                        <span class="truncate">{{ opt }}</span>
                        <Check v-if="oIdx === q.correctAnswer" :size="11" class="ml-auto shrink-0" :style="{ color: 'var(--color-success-text)' }" />
                      </div>
                    </div>
                    <div class="flex items-center gap-2 pt-0.5">
                      <GlassBadge variant="info" size="sm">{{ q.category }}</GlassBadge>
                      <GlassBadge :variant="q.difficulty === 'Dễ' ? 'success' : q.difficulty === 'Khó' ? 'danger' : 'warning'" size="sm">{{ q.difficulty }}</GlassBadge>
                    </div>
                  </div>
                </div>
                <div class="flex items-center gap-2 shrink-0">
                  <div class="flex items-center gap-1 rounded-lg border border-card bg-surface-elevated px-2 py-1">
                    <Award :size="11" class="text-muted" />
                    <input type="number" step="0.05" min="0.05" :value="q.score" @input="updateQuestionScore(index, $event)"
                      class="w-9 text-xs font-bold text-center outline-none bg-transparent text-heading" />
                    <span class="text-[10px] font-semibold text-muted">đ</span>
                  </div>
                  <button @click="removeQuestion(index)" class="flex h-7 w-7 items-center justify-center rounded-md text-muted transition-colors hover:bg-surface-input" title="Xóa câu hỏi">
                    <Trash2 :size="14" style="color:var(--color-danger-text)" />
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- Add question buttons -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <div @click="openQuestionBank"
              class="flex cursor-pointer flex-col items-center justify-center gap-2 rounded-lg border-2 border-dashed border-input bg-surface-input p-3.5 text-center transition-colors hover:border-link">
              <div class="flex h-9 w-9 items-center justify-center rounded-lg border border-card bg-surface-elevated" style="color:var(--accent-primary)">
                <Database :size="17" />
              </div>
              <h4 class="text-xs font-semibold text-heading">Chọn từ Thư viện câu hỏi</h4>
              <p class="text-[10px] text-muted leading-relaxed">Tìm kiếm và sử dụng câu hỏi có sẵn từ ngân hàng đề.</p>
            </div>
            <div @click="openQuestionCreator"
              class="flex cursor-pointer flex-col items-center justify-center gap-2 rounded-lg border-2 border-dashed border-input bg-surface-input p-3.5 text-center transition-colors hover:border-link">
              <div class="flex h-9 w-9 items-center justify-center rounded-lg border border-card bg-surface-elevated" style="color:var(--accent-primary)">
                <Plus :size="17" />
              </div>
              <h4 class="text-xs font-semibold text-heading">Tự soạn câu hỏi mới</h4>
              <p class="text-[10px] text-muted leading-relaxed">Tạo câu hỏi trắc nghiệm hoặc tự luận tùy chỉnh tại đây.</p>
            </div>
          </div>
        </GlassPanel>

      </div>

      <!-- Sidebar -->
      <div class="space-y-5">

        <!-- Cấu hình đề thi -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2.5">
              <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--surface-input);color:var(--text-label)">
                <Settings :size="15" />
              </div>
              <h2 class="text-sm font-semibold text-heading">Cấu hình đề thi</h2>
            </div>
          </template>
          <div class="space-y-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm font-semibold text-body">Xáo trộn câu hỏi</p>
                <p class="text-[11px] text-muted mt-0.5">Xáo ngẫu nhiên vị trí câu hỏi</p>
              </div>
              <button type="button" @click="form.shuffle = !form.shuffle"
                :class="['relative h-5 w-9 shrink-0 rounded-full transition-colors', form.shuffle ? 'bg-link' : 'bg-surface-input']">
                <div :class="['h-4 w-4 rounded-full bg-white shadow-sm transition-transform absolute top-0.5', form.shuffle ? 'translate-x-[18px]' : 'translate-x-0.5']"></div>
              </button>
            </div>
            <div class="flex items-center justify-between pt-3 border-t border-card">
              <div>
                <p class="text-sm font-semibold text-body">Hiển thị kết quả</p>
                <p class="text-[11px] text-muted mt-0.5">Sinh viên xem điểm ngay sau nộp</p>
              </div>
              <button type="button" @click="form.showResult = !form.showResult"
                :class="['relative h-5 w-9 shrink-0 rounded-full transition-colors', form.showResult ? 'bg-link' : 'bg-surface-input']">
                <div :class="['h-4 w-4 rounded-full bg-white shadow-sm transition-transform absolute top-0.5', form.showResult ? 'translate-x-[18px]' : 'translate-x-0.5']"></div>
              </button>
            </div>
            <div class="pt-3 border-t border-card">
              <label class="mb-1 block text-xs font-semibold text-label">Điểm đạt tối thiểu (Pass)</label>
              <div class="relative">
                <Award :size="13" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted pointer-events-none" />
                <input v-model.number="form.passingScore" type="number" step="0.5" max="10" min="1"
                  class="w-full rounded-lg border border-input bg-surface-input pl-8 pr-3 py-2 text-sm text-heading outline-none focus:border-link transition-colors" />
              </div>
            </div>
          </div>
        </GlassPanel>

        <!-- Điều kiện mở đề -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2.5">
              <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--color-warning-bg);color:var(--color-warning-text)">
                <ShieldCheck :size="15" />
              </div>
              <h2 class="text-sm font-semibold text-heading">Điều kiện mở đề</h2>
            </div>
          </template>
          <div class="space-y-3.5">
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Bắt đầu mở</label>
              <input v-model="form.startTime" type="datetime-local"
                class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-sm text-heading outline-none focus:border-link transition-colors cursor-pointer" />
            </div>
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Tự động đóng</label>
              <input v-model="form.endTime" type="datetime-local"
                class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-sm text-heading outline-none focus:border-link transition-colors cursor-pointer" />
            </div>
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Số lần làm tối đa</label>
              <select v-model.number="form.maxAttempts"
                class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-sm text-heading outline-none focus:border-link transition-colors cursor-pointer">
                <option :value="1">1 lần</option>
                <option :value="2">2 lần</option>
                <option :value="3">3 lần</option>
                <option :value="999">Không giới hạn</option>
              </select>
            </div>
            <div>
              <label class="mb-1 block text-xs font-semibold text-label">Trạng thái sau khi xuất bản</label>
              <select v-model="form.status"
                class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-sm text-heading outline-none focus:border-link transition-colors cursor-pointer">
                <option :value="EXAM_STATUS.SCHEDULED">Đã lên lịch</option>
                <option :value="EXAM_STATUS.OPEN">Đang mở</option>
                <option :value="EXAM_STATUS.CLOSED">Đã đóng</option>
              </select>
            </div>
            <div class="flex items-center justify-between rounded-lg border border-card bg-surface-input p-3">
              <div>
                <p class="text-sm font-semibold text-body">Cho phép làm trước</p>
                <p class="text-[11px] text-muted mt-0.5">Sinh viên thấy cảnh báo học/làm trước.</p>
              </div>
              <button type="button" @click="form.allowEarlyLearning = !form.allowEarlyLearning"
                :class="['relative h-5 w-9 shrink-0 rounded-full transition-colors', form.allowEarlyLearning ? 'bg-link' : 'bg-surface-input']">
                <div :class="['h-4 w-4 rounded-full bg-white shadow-sm transition-transform absolute top-0.5', form.allowEarlyLearning ? 'translate-x-[18px]' : 'translate-x-0.5']"></div>
              </button>
            </div>
            <div class="flex items-start gap-2.5 rounded-lg border border-card bg-surface-input p-3">
              <Info :size="14" class="text-muted shrink-0 mt-0.5" />
              <p class="text-[11px] text-muted leading-relaxed">Không cần mật khẩu đề thi. Truy cập được kiểm soát bằng lớp học phần, thời gian mở/đóng, trạng thái đề và số lần làm.</p>
            </div>
          </div>
        </GlassPanel>

      </div>
    </div>

    <!-- Question Bank Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showBankModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm" @click.self="showBankModal = false">
          <div class="relative flex max-h-[85vh] w-full max-w-4xl flex-col overflow-hidden rounded-xl border border-card bg-surface-modal shadow-(--lg-shadow-md) animate-modal-in">
            <div class="flex items-center justify-between border-b border-card px-4 py-3">
              <div class="flex items-center gap-3">
                <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--color-info-bg);color:var(--color-info-text)">
                  <Database :size="17" />
                </div>
                <div>
                  <h3 class="text-sm font-semibold text-heading">Thư viện câu hỏi</h3>
                  <p class="text-xs text-muted mt-0.5">Chọn từ Ngân hàng đề thi môn học hiện có.</p>
                </div>
              </div>
              <button @click="showBankModal = false" class="flex h-7 w-7 items-center justify-center rounded-lg border border-card bg-surface-input text-muted transition-colors">
                <X :size="15" />
              </button>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-2.5 border-b border-card bg-surface-input p-3.5">
              <div class="relative">
                <Search :size="13" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
                <input v-model="bankSearch" type="text" placeholder="Tìm nội dung câu hỏi..."
                  class="w-full rounded-lg border border-input bg-surface-elevated pl-8 pr-3 py-2 text-xs text-heading outline-none placeholder:text-placeholder focus:border-link transition-colors" />
              </div>
              <select v-model="bankCategoryFilter"
                class="rounded-lg border border-input bg-surface-elevated px-3 py-2 text-xs text-heading outline-none focus:border-link cursor-pointer">
                <option value="">Tất cả các môn</option>
                <option>Lập trình Web</option>
                <option>Lập trình Java</option>
                <option>Cơ sở dữ liệu</option>
              </select>
              <select v-model="bankDifficultyFilter"
                class="rounded-lg border border-input bg-surface-elevated px-3 py-2 text-xs text-heading outline-none focus:border-link cursor-pointer">
                <option value="">Tất cả độ khó</option>
                <option>Dễ</option>
                <option>Trung bình</option>
                <option>Khó</option>
              </select>
            </div>
            <div class="flex-1 space-y-2 overflow-y-auto p-3.5 bg-surface-input">
              <div v-for="q in filteredBankQuestions" :key="q.id" @click="toggleQuestionSelection(q.id)"
                class="flex cursor-pointer items-start gap-3 rounded-lg border p-3.5 transition-colors"
                :style="bankCheckedIds.includes(q.id) ? { background: 'var(--color-info-bg)', borderColor: 'var(--text-link)' } : { background: 'var(--surface-elevated)', borderColor: 'var(--border-card)' }">
                <div class="flex h-4 w-4 shrink-0 mt-0.5 items-center justify-center rounded border transition-colors"
                  :style="bankCheckedIds.includes(q.id) ? { background: 'var(--text-link)', borderColor: 'var(--text-link)' } : { borderColor: 'var(--border-input)', background: 'var(--surface-elevated)' }">
                  <Check v-if="bankCheckedIds.includes(q.id)" :size="11" style="color:#fff" />
                </div>
                <div class="space-y-1.5 flex-1 min-w-0">
                  <p class="text-xs font-semibold text-heading leading-snug">{{ q.text }}</p>
                  <div v-if="q.options" class="flex flex-wrap gap-1.5">
                    <span v-for="(opt, idx) in q.options" :key="idx"
                      class="rounded border border-card bg-surface-input px-2 py-0.5 text-[10px] text-muted font-medium">
                      {{ String.fromCharCode(65 + idx) }}. {{ opt }}
                    </span>
                  </div>
                  <div class="flex items-center gap-2">
                    <GlassBadge variant="info" size="sm">{{ q.category }}</GlassBadge>
                    <GlassBadge :variant="q.difficulty === 'Dễ' ? 'success' : q.difficulty === 'Khó' ? 'danger' : 'warning'" size="sm">{{ q.difficulty }}</GlassBadge>
                    <span class="text-[10px] text-muted font-medium">{{ q.score }} đ</span>
                  </div>
                </div>
              </div>
              <div v-if="filteredBankQuestions.length === 0" class="flex flex-col items-center py-10 text-center">
                <Database :size="32" class="text-placeholder mb-2" />
                <p class="text-xs font-semibold text-muted">Không tìm thấy câu hỏi phù hợp</p>
                <p class="text-[10px] text-placeholder">Tất cả câu hỏi trong bộ lọc đã được thêm vào đề thi hoặc không khớp từ khóa.</p>
              </div>
            </div>
            <div class="flex items-center justify-end gap-2.5 border-t border-card bg-surface-input px-4 py-3">
              <GlassButton variant="secondary" size="sm" @click="showBankModal = false">Hủy</GlassButton>
              <GlassButton variant="primary" size="sm" @click="addQuestionsFromBank">
                Thêm đã chọn
                <template #trailing><span class="rounded-md px-1.5 py-0.5 text-[10px] font-bold" style="background:color-mix(in srgb, var(--text-inverse) 25%, transparent)">{{ bankCheckedIds.length }}</span></template>
              </GlassButton>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Creator Modal -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showCreatorModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm" @click.self="showCreatorModal = false">
          <div class="relative flex max-h-[90vh] w-full max-w-2xl flex-col overflow-hidden rounded-xl border border-card bg-surface-modal shadow-(--lg-shadow-md) animate-modal-in">
            <div class="flex items-center justify-between border-b border-card px-4 py-3">
              <div class="flex items-center gap-3">
                <div class="flex h-8 w-8 items-center justify-center rounded-lg" style="background:var(--color-info-bg);color:var(--color-info-text)">
                  <Plus :size="17" />
                </div>
                <div>
                  <h3 class="text-sm font-semibold text-heading">Soạn câu hỏi mới</h3>
                  <p class="text-xs text-muted mt-0.5">Soạn nội dung và cấu hình phương án chi tiết.</p>
                </div>
              </div>
              <button @click="showCreatorModal = false" class="flex h-7 w-7 items-center justify-center rounded-lg border border-card bg-surface-input text-muted transition-colors">
                <X :size="15" />
              </button>
            </div>
            <div class="flex-1 space-y-4 overflow-y-auto p-4 bg-surface-input">
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Nội dung câu hỏi <span style="color:var(--color-danger-text)">*</span></label>
                <textarea v-model="newQuestion.text" rows="3" placeholder="Nhập câu hỏi tại đây..."
                  class="w-full rounded-lg border bg-surface-elevated p-3.5 text-sm text-heading outline-none placeholder:text-placeholder focus:border-link transition-colors resize-none leading-relaxed"
                  :style="creatorErrors.text ? { borderColor: 'var(--color-danger-text)' } : { borderColor: 'var(--border-input)' }"></textarea>
                <p v-if="creatorErrors.text" class="mt-1 flex items-center gap-1 text-[11px] font-semibold" style="color:var(--color-danger-text)">
                  <AlertCircle :size="10" /> {{ creatorErrors.text }}
                </p>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-3 gap-3.5">
                <div>
                  <label class="mb-1 block text-xs font-semibold text-label">Loại câu hỏi</label>
                  <select v-model="newQuestion.type"
                    class="w-full rounded-lg border border-input bg-surface-elevated px-3 py-2.5 text-sm text-heading outline-none focus:border-link cursor-pointer">
                    <option>Trắc nghiệm</option>
                    <option>Tự luận</option>
                  </select>
                </div>
                <div>
                  <label class="mb-1 block text-xs font-semibold text-label">Chủ đề môn học</label>
                  <select v-model="newQuestion.category"
                    class="w-full rounded-lg border border-input bg-surface-elevated px-3 py-2.5 text-sm text-heading outline-none focus:border-link cursor-pointer">
                    <option>Lập trình Web</option>
                    <option>Lập trình Java</option>
                    <option>Cơ sở dữ liệu</option>
                  </select>
                </div>
                <div>
                  <label class="mb-1 block text-xs font-semibold text-label">Điểm số</label>
                  <input v-model.number="newQuestion.score" type="number" step="0.05" min="0.05"
                    class="w-full rounded-lg border border-input bg-surface-elevated px-3 py-2.5 text-sm text-heading outline-none focus:border-link transition-colors" />
                </div>
              </div>
              <div v-if="newQuestion.type === 'Trắc nghiệm'" class="space-y-2.5 pt-3 border-t border-card">
                <label class="text-xs font-semibold text-label block">Thiết lập các phương án và chọn câu trả lời đúng:</label>
                <div v-for="(opt, idx) in newQuestion.options" :key="idx" class="flex items-center gap-2.5">
                  <button type="button" @click="newQuestion.correctAnswer = idx"
                    class="flex h-5 w-5 shrink-0 items-center justify-center rounded-full border transition-colors"
                    :style="newQuestion.correctAnswer === idx ? { background: 'var(--color-success-text)', borderColor: 'var(--color-success-text)' } : { borderColor: 'var(--border-input)', background: 'var(--surface-elevated)' }">
                    <Check v-if="newQuestion.correctAnswer === idx" :size="12" style="color:#fff" />
                  </button>
                  <div class="relative flex-1">
                    <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[10px] font-semibold text-muted uppercase">{{ String.fromCharCode(65 + idx) }}</span>
                    <input v-model="newQuestion.options[idx]" type="text" :placeholder="`Phương án ${String.fromCharCode(65 + idx)}...`"
                      class="w-full rounded-lg border bg-surface-elevated pl-8 pr-3 py-2.5 text-sm text-heading outline-none placeholder:text-placeholder focus:border-link transition-colors"
                      :style="creatorErrors[`option_${idx}`] ? { borderColor: 'var(--color-danger-text)' } : { borderColor: 'var(--border-input)' }" />
                  </div>
                </div>
                <p v-if="creatorErrors.option_0 || creatorErrors.option_1 || creatorErrors.option_2 || creatorErrors.option_3"
                  class="flex items-center gap-1 text-[11px] font-semibold" style="color:var(--color-danger-text)">
                  <AlertCircle :size="10" /> Vui lòng điền đầy đủ các phương án trắc nghiệm.
                </p>
              </div>
            </div>
            <div class="flex items-center justify-end gap-2.5 border-t border-card bg-surface-input px-4 py-3">
              <GlassButton variant="secondary" size="sm" @click="showCreatorModal = false">Hủy</GlassButton>
              <GlassButton variant="primary" size="sm" @click="createQuestionInline">Lưu câu hỏi</GlassButton>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
@keyframes modal-in {
  from { opacity: 0; transform: scale(0.96) translateY(15px); }
  to   { opacity: 1; transform: scale(1) translateY(0); }
}
.animate-modal-in {
  animation: modal-in 0.35s cubic-bezier(0.16, 1, 0.3, 1) both;
}

.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from { transform: translateY(-20px) scale(0.9); opacity: 0; }
.toast-slide-leave-to   { transform: translateY(20px) scale(0.9); opacity: 0; }

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to { opacity: 0; }
</style>
