<!-- ExamsView.vue -->
<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Plus, Search, FileEdit, Clock, HelpCircle, 
  Send, CheckCircle2,
  FileSignature, Calendar, Settings, ShieldCheck
} from 'lucide-vue-next'
import { EXAM_STATUS, getExamStatusLabel, normalizeExamStatus } from '@/utils/examAccess.js'

const defaultExams = [
  {
    id: 1,
    name: 'Thi giữa kỳ: Lập trình Web',
    subjectName: 'Lập trình Web',
    classSectionCode: 'WEB201-SD1904-B1',
    duration: '90 phút',
    questions: 40,
    status: EXAM_STATUS.OPEN,
    date: '04/06/2026',
    openAt: '2026-06-04T08:00',
    closeAt: '2026-06-04T10:00',
    type: 'Trắc nghiệm',
    maxAttempts: 1,
    allowedStudents: 42,
    completedStudents: 31,
    pendingStudents: 11,
    allowEarlyLearning: false,
  },
  {
    id: 2,
    name: 'Thi kết thúc môn: Java Basic',
    subjectName: 'Java Basic',
    classSectionCode: 'JAVA101-SD1905-B2',
    duration: '120 phút',
    questions: 50,
    status: EXAM_STATUS.DRAFT,
    date: '15/06/2026',
    openAt: '2026-06-15T08:00',
    closeAt: '2026-06-15T10:00',
    type: 'Hỗn hợp',
    maxAttempts: 1,
    allowedStudents: 38,
    completedStudents: 0,
    pendingStudents: 38,
    allowEarlyLearning: false,
  },
  {
    id: 3,
    name: 'Quiz 2: Cấu trúc dữ liệu',
    subjectName: 'Cấu trúc dữ liệu',
    classSectionCode: 'CTDL101-SD1904-B1',
    duration: '15 phút',
    questions: 10,
    status: EXAM_STATUS.RESULT_PUBLISHED,
    date: '18/05/2026',
    openAt: '2026-05-18T08:00',
    closeAt: '2026-05-18T23:59',
    type: 'Trắc nghiệm',
    maxAttempts: 2,
    allowedStudents: 42,
    completedStudents: 42,
    pendingStudents: 0,
    allowEarlyLearning: true,
  },
]

const exams = ref([])
const searchQuery = ref('')

onMounted(() => {
  const stored = localStorage.getItem('teacher_exams')
  if (stored) {
    exams.value = JSON.parse(stored).map(normalizeTeacherExam)
  } else {
    exams.value = defaultExams.map(normalizeTeacherExam)
    localStorage.setItem('teacher_exams', JSON.stringify(defaultExams))
  }
})

const filteredExams = computed(() => {
  return exams.value.filter(ex => 
    [ex.name, ex.subjectName, ex.classSectionCode].some((value) =>
      String(value || '').toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  )
})

const isConfigModalOpen = ref(false)
const configuringExam = ref(null)

const openConfigModal = (exam) => {
  configuringExam.value = normalizeTeacherExam({ ...exam, shuffle: true, passScore: exam.passScore || 5 })
  isConfigModalOpen.value = true
}

const saveConfig = () => {
  const idx = exams.value.findIndex(e => e.id === configuringExam.value.id)
  if (idx !== -1) {
    exams.value[idx] = normalizeTeacherExam({
      ...exams.value[idx],
      ...configuringExam.value,
      pendingStudents: Math.max(
        Number(configuringExam.value.allowedStudents || 0) - Number(configuringExam.value.completedStudents || 0),
        0
      ),
    })
    localStorage.setItem('teacher_exams', JSON.stringify(exams.value))
  }
  isConfigModalOpen.value = false
}

const publishExam = (examId) => {
  const idx = exams.value.findIndex(e => e.id === examId)
  if (idx !== -1) {
    exams.value[idx].status = EXAM_STATUS.SCHEDULED
    localStorage.setItem('teacher_exams', JSON.stringify(exams.value))
  }
}

const getStatusStyle = (status) => {
  const normalized = normalizeExamStatus(status)
  if (normalized === EXAM_STATUS.OPEN) return 'bg-emerald-50 text-emerald-600 border-emerald-100'
  if (normalized === EXAM_STATUS.RESULT_PUBLISHED) return 'bg-blue-50 text-blue-600 border-blue-100'
  if (normalized === EXAM_STATUS.CLOSED) return 'bg-slate-100 text-slate-600 border-slate-200'
  if (normalized === EXAM_STATUS.SCHEDULED) return 'bg-cyan-50 text-cyan-600 border-cyan-100'
  return 'bg-amber-50 text-amber-600 border-amber-100'
}

function normalizeTeacherExam(exam) {
  const maxAttempts = exam.maxAttempts || 1
  const allowedStudents = exam.allowedStudents || 40
  const completedStudents = exam.completedStudents || 0
  return {
    ...exam,
    subjectName: exam.subjectName || exam.type || 'Môn học demo',
    classSectionCode: exam.classSectionCode || 'SD1904-DEMO',
    openAt: exam.openAt || '',
    closeAt: exam.closeAt || '',
    maxAttempts,
    allowedStudents,
    completedStudents,
    pendingStudents: exam.pendingStudents ?? Math.max(allowedStudents - completedStudents, 0),
    allowEarlyLearning: Boolean(exam.allowEarlyLearning),
    status: normalizeExamStatus(exam.status),
    accessPolicy: {
      requirePassword: false,
      controlledBy: 'class_section_schedule_attempt',
    },
  }
}

function formatTime(value) {
  if (!value) return 'Chưa đặt'
  const date = new Date(value)
  return date.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}
</script>

<template>
  <div class="teacher-exams-page space-y-8 pb-10 text-slate-800">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-gradient-to-br from-blue-500 to-cyan-600 flex items-center justify-center text-white shadow-md shadow-blue-200">
           <FileSignature :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Quản lý Đề thi</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Thiết kế và cấu hình các bộ đề thi trắc nghiệm & tự luận.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <router-link to="/teacher/exams/create" class="flex items-center gap-2 rounded-2xl bg-gradient-to-br from-blue-600 to-cyan-600 px-4 py-4 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:shadow-xl hover:-translate-y-0.5 transition-all">
            <Plus :size="18" /> Tạo đề thi mới
         </router-link>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="flex items-center justify-between gap-4">
       <div class="flex items-center gap-2">
          <h2 class="text-lg font-black text-slate-800">Danh sách đề thi</h2>
          <span class="rounded-full bg-blue-100 px-2 py-0.5 text-xs font-black text-blue-700">{{ filteredExams.length }}</span>
       </div>
       <div class="relative w-64">
          <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input v-model="searchQuery" type="text" placeholder="Tìm đề, môn, lớp..." class="w-full rounded-[16px] border border-slate-200 bg-white pl-11 pr-4 py-3 text-sm font-medium outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-colors shadow-sm" />
       </div>
    </div>

    <!-- Main List -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div v-for="(exam, index) in filteredExams" :key="exam.id" 
           class="group rounded-2xl bg-white border border-slate-100 p-5 shadow-sm hover:shadow-xl hover:border-blue-200 transition-all flex flex-col animate-fade-in-up"
           :style="{ animationDelay: `${index * 100}ms` }">
        <div class="flex justify-between items-start mb-4">
           <div class="h-10 w-10 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-colors shadow-sm">
              <FileEdit :size="24" />
           </div>
           <div :class="['rounded-xl border px-3 py-1.5 text-[10px] font-black uppercase tracking-widest', getStatusStyle(exam.status)]">
              {{ getExamStatusLabel(exam.status) }}
           </div>
        </div>
        
        <div class="flex-1">
           <div class="inline-block rounded-lg bg-slate-100 px-2 py-1 text-[10px] font-black text-slate-500 uppercase tracking-widest mb-3">
              {{ exam.type }}
           </div>
           <h3 class="text-xl font-black text-slate-900 leading-tight group-hover:text-blue-700 transition-colors line-clamp-2 min-h-[56px]">{{ exam.name }}</h3>
           <p class="mt-2 text-xs font-bold text-slate-500">{{ exam.subjectName }} · {{ exam.classSectionCode }}</p>
           
           <div class="mt-6 flex flex-col gap-3">
              <div class="flex items-center gap-3">
                 <div class="flex items-center justify-center w-8 h-8 rounded-full bg-slate-50 text-slate-400">
                    <Calendar :size="14" />
                 </div>
                 <div>
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400">Thời gian mở đề</p>
                    <p class="text-sm font-bold text-slate-700">{{ formatTime(exam.openAt) }} - {{ formatTime(exam.closeAt) }}</p>
                 </div>
              </div>
              <div class="grid grid-cols-3 gap-2 text-center">
                <div class="rounded-xl bg-slate-50 border border-slate-100 px-2 py-2">
                  <p class="text-[9px] font-black uppercase tracking-widest text-slate-400">Được làm</p>
                  <p class="text-sm font-black text-slate-800">{{ exam.allowedStudents }}</p>
                </div>
                <div class="rounded-xl bg-emerald-50 border border-emerald-100 px-2 py-2">
                  <p class="text-[9px] font-black uppercase tracking-widest text-emerald-500">Đã làm</p>
                  <p class="text-sm font-black text-emerald-700">{{ exam.completedStudents }}</p>
                </div>
                <div class="rounded-xl bg-amber-50 border border-amber-100 px-2 py-2">
                  <p class="text-[9px] font-black uppercase tracking-widest text-amber-500">Chưa</p>
                  <p class="text-sm font-black text-amber-700">{{ exam.pendingStudents }}</p>
                </div>
              </div>
              <div class="flex items-center gap-4 mt-2 pt-4 border-t border-slate-50">
                 <div class="flex items-center gap-2">
                    <Clock :size="14" class="text-blue-400" />
                    <span class="text-sm font-bold text-slate-600">{{ exam.duration }}</span>
                 </div>
                 <div class="w-1 h-1 rounded-full bg-slate-300"></div>
                 <div class="flex items-center gap-2">
                    <HelpCircle :size="14" class="text-blue-400" />
                    <span class="text-sm font-bold text-slate-600">{{ exam.questions }} câu hỏi</span>
                 </div>
              </div>
              <div class="flex items-center gap-2 rounded-xl border border-blue-100 bg-blue-50 px-3 py-2 text-[10px] font-bold text-blue-700">
                <ShieldCheck :size="13" />
                Không cần mật khẩu đề thi. Truy cập theo lớp, lịch mở đề và số lần làm.
              </div>
           </div>
        </div>

        <div class="mt-8 pt-6 border-t border-slate-100 flex gap-3">
           <button @click="openConfigModal(exam)" class="flex-1 rounded-2xl bg-white border border-slate-200 py-3 text-sm font-bold text-slate-500 hover:bg-slate-50 hover:text-blue-600 transition-colors shadow-sm">
              Cấu hình
           </button>
           <button v-if="normalizeExamStatus(exam.status) === EXAM_STATUS.DRAFT" @click="publishExam(exam.id)" class="flex-1 rounded-2xl bg-blue-600 py-3 text-sm font-bold text-white shadow-lg shadow-blue-200 hover:bg-blue-700 transition-colors flex items-center justify-center gap-2">
              <Send :size="16" /> Lên lịch
           </button>
           <button v-else class="flex-1 rounded-2xl bg-emerald-50 py-3 text-sm font-bold text-emerald-600 flex items-center justify-center gap-2 border border-emerald-100">
              <CheckCircle2 :size="16" /> Theo dõi
           </button>
        </div>
      </div>
    </div>
  <!-- Configure Exam Modal -->
  <Teleport to="body">
    <div v-if="isConfigModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" @click="isConfigModalOpen = false"></div>
      <div class="teacher-exam-modal relative bg-white rounded-3xl shadow-2xl w-full max-w-2xl overflow-hidden animate-fade-in-up">
        <div class="p-4 border-b border-slate-100 bg-slate-50/50 flex items-center justify-between">
          <div>
             <h3 class="text-xl font-bold text-slate-800">Cấu hình Đề thi</h3>
             <p class="text-sm text-slate-500 mt-1">Phase 7: quyền vào đề được kiểm soát bằng lớp học phần, lịch mở đề và số lần làm.</p>
          </div>
          <div class="h-10 w-10 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600 shadow-sm border border-blue-100/50">
             <Settings :size="24" />
          </div>
        </div>
        
        <div class="p-4 space-y-4 max-h-[60vh] overflow-y-auto">
           <div class="space-y-4">
             <h4 class="text-sm font-bold text-slate-800 uppercase tracking-wider flex items-center gap-2"><FileEdit :size="16" class="text-blue-500" /> Thông tin cơ bản</h4>
             <div>
               <label class="block text-sm font-bold text-slate-700 mb-1.5">Tên đề thi</label>
               <input v-model="configuringExam.name" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
             </div>
             <div class="grid grid-cols-2 gap-4">
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Môn học</label>
                 <input v-model="configuringExam.subjectName" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Lớp học phần áp dụng</label>
                 <input v-model="configuringExam.classSectionCode" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Thời gian làm bài</label>
                 <input v-model="configuringExam.duration" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
               <div>
                 <label class="block text-sm font-bold text-slate-700 mb-1.5">Ngày diễn ra</label>
                 <input v-model="configuringExam.date" type="text" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
               </div>
             </div>
           </div>

           <div class="space-y-4 pt-4 border-t border-slate-100">
             <h4 class="text-sm font-bold text-slate-800 uppercase tracking-wider flex items-center gap-2"><Settings :size="16" class="text-amber-500" /> Điều kiện mở đề</h4>
             
             <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Thời gian mở</label>
                  <input v-model="configuringExam.openAt" type="datetime-local" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Thời gian đóng</label>
                  <input v-model="configuringExam.closeAt" type="datetime-local" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Số lần làm bài tối đa</label>
                  <select v-model="configuringExam.maxAttempts" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all">
                    <option :value="1">1 lần</option>
                    <option :value="2">2 lần</option>
                    <option :value="3">3 lần</option>
                    <option :value="999">Không giới hạn</option>
                  </select>
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Trạng thái</label>
                  <select v-model="configuringExam.status" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all">
                    <option :value="EXAM_STATUS.DRAFT">Nháp</option>
                    <option :value="EXAM_STATUS.SCHEDULED">Đã lên lịch</option>
                    <option :value="EXAM_STATUS.OPEN">Đang mở</option>
                    <option :value="EXAM_STATUS.CLOSED">Đã đóng</option>
                    <option :value="EXAM_STATUS.RESULT_PUBLISHED">Đã công bố kết quả</option>
                  </select>
                </div>
             </div>

             <div class="grid grid-cols-3 gap-3">
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">SV được phép</label>
                  <input v-model.number="configuringExam.allowedStudents" type="number" min="0" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Đã làm</label>
                  <input v-model.number="configuringExam.completedStudents" type="number" min="0" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
                <div>
                  <label class="block text-sm font-bold text-slate-700 mb-1.5">Chưa làm</label>
                  <input v-model.number="configuringExam.pendingStudents" type="number" min="0" class="w-full rounded-xl border border-slate-200 px-4 py-3 outline-none focus:border-blue-400 focus:ring-4 focus:ring-blue-50 transition-all" />
                </div>
             </div>
             
             <div class="flex items-center justify-between p-4 rounded-xl border border-slate-200 bg-slate-50 mt-2">
                <div>
                   <p class="text-sm font-bold text-slate-800">Cho phép làm/học trước</p>
                   <p class="text-xs font-medium text-slate-500 mt-0.5">Sinh viên thuộc lớp được phép thấy cảnh báo trước khi làm trước lộ trình.</p>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" v-model="configuringExam.allowEarlyLearning" class="sr-only peer">
                  <div class="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-500"></div>
                </label>
             </div>
             <div class="rounded-xl border border-blue-100 bg-blue-50 p-3 text-xs font-bold leading-relaxed text-blue-700">
               Không có trường mật khẩu/access code cho đề thi. Backend sau này cần enforce classSectionIds, openAt, closeAt, maxAttempts và status.
             </div>
           </div>
        </div>
        
        <div class="p-4 border-t border-slate-100 bg-slate-50/50 flex justify-end gap-3">
          <button @click="isConfigModalOpen = false" class="px-5 py-2.5 rounded-xl font-bold text-slate-500 hover:bg-slate-200 transition-colors">Đóng</button>
          <button @click="saveConfig" class="px-5 py-2.5 rounded-xl font-bold text-white bg-blue-600 hover:bg-blue-700 shadow-md shadow-blue-200 transition-all hover:-translate-y-0.5">Lưu cấu hình</button>
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
  animation: fade-in-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.teacher-exams-page {
  color: var(--text-body);
}

:global(.dark) .teacher-exams-page :deep(.bg-white),
:global(.dark) .teacher-exam-modal {
  background: var(--surface-card-strong);
  color: var(--text-body);
}

:global(.dark) .teacher-exams-page :deep(.bg-slate-50),
:global(.dark) .teacher-exams-page :deep(.bg-slate-50\/50),
:global(.dark) .teacher-exam-modal :deep(.bg-slate-50),
:global(.dark) .teacher-exam-modal :deep(.bg-slate-50\/50) {
  background: var(--surface-input);
}

:global(.dark) .teacher-exams-page :deep(.border-slate-100),
:global(.dark) .teacher-exams-page :deep(.border-slate-200),
:global(.dark) .teacher-exam-modal :deep(.border-slate-100),
:global(.dark) .teacher-exam-modal :deep(.border-slate-200) {
  border-color: var(--border-card);
}

:global(.dark) .teacher-exams-page :deep(.text-slate-900),
:global(.dark) .teacher-exams-page :deep(.text-slate-800),
:global(.dark) .teacher-exams-page :deep(.text-slate-700),
:global(.dark) .teacher-exam-modal :deep(.text-slate-900),
:global(.dark) .teacher-exam-modal :deep(.text-slate-800),
:global(.dark) .teacher-exam-modal :deep(.text-slate-700) {
  color: var(--text-heading);
}

:global(.dark) .teacher-exams-page :deep(.text-slate-600),
:global(.dark) .teacher-exams-page :deep(.text-slate-500),
:global(.dark) .teacher-exam-modal :deep(.text-slate-600),
:global(.dark) .teacher-exam-modal :deep(.text-slate-500) {
  color: var(--text-label);
}

:global(.dark) .teacher-exams-page :deep(input),
:global(.dark) .teacher-exam-modal :deep(input),
:global(.dark) .teacher-exam-modal :deep(select) {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
</style>
