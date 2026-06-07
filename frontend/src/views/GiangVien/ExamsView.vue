<!-- ExamsView.vue -->
<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Plus, Search, FileEdit, Clock, HelpCircle, 
  Send, CheckCircle2,
  FileSignature, Calendar, Settings, ShieldCheck
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
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

const glassBadgeVariant = (status) => {
  const normalized = normalizeExamStatus(status)
  if (normalized === EXAM_STATUS.OPEN) return 'success'
  if (normalized === EXAM_STATUS.RESULT_PUBLISHED) return 'info'
  if (normalized === EXAM_STATUS.CLOSED) return 'neutral'
  if (normalized === EXAM_STATUS.SCHEDULED) return 'primary'
  return 'warning'
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
  <div class="space-y-6 pb-10">
    <!-- ── Header ── -->
    <GlassPanel variant="flat" class="flex flex-col md:flex-row md:items-center justify-between gap-4 p-5 relative">
      <div class="flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-[var(--accent-primary)]/10 flex items-center justify-center text-link shadow-sm">
           <FileSignature :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-semibold text-heading tracking-tight">Quản lý Đề thi</h1>
          <p class="text-sm font-medium text-muted mt-1">Thiết kế và cấu hình các bộ đề thi trắc nghiệm & tự luận.</p>
        </div>
      </div>
      <div class="flex gap-3">
         <router-link to="/teacher/exams/create">
           <GlassButton variant="primary" size="md"><Plus :size="16" /> Tạo đề thi mới</GlassButton>
         </router-link>
      </div>
    </GlassPanel>

    <!-- Toolbar -->
    <div class="flex items-center justify-between gap-4">
       <div class="flex items-center gap-2">
          <h2 class="text-lg font-semibold text-heading">Danh sách đề thi</h2>
          <GlassBadge variant="primary">{{ filteredExams.length }}</GlassBadge>
       </div>
       <div class="relative w-64">
          <Search :size="16" class="absolute left-4 top-1/2 -translate-y-1/2 text-muted" />
          <input v-model="searchQuery" type="text" placeholder="Tìm đề, môn, lớp..." class="w-full rounded-xl border border-input bg-surface-input pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:border-[var(--accent-primary)] transition-colors" />
       </div>
    </div>

    <!-- Main List -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div v-for="(exam, index) in filteredExams" :key="exam.id" 
           class="group rounded-2xl surface-card border border-card p-5 shadow-sm hover:shadow-xl hover:border-[var(--accent-primary)]/30 transition-all flex flex-col animate-fade-in-up"
           :style="{ animationDelay: `${index * 100}ms` }">
        <div class="flex justify-between items-start mb-4">
           <div class="h-10 w-10 rounded-2xl bg-[var(--accent-primary)]/5 flex items-center justify-center text-muted group-hover:bg-[var(--accent-primary)]/10 group-hover:text-link transition-colors shadow-sm">
              <FileEdit :size="24" />
           </div>
           <GlassBadge :variant="glassBadgeVariant(exam.status)">
              {{ getExamStatusLabel(exam.status) }}
           </GlassBadge>
        </div>
        
        <div class="flex-1">
           <div class="inline-block rounded-lg bg-[var(--surface-input)] px-2 py-1 text-[10px] font-semibold text-muted uppercase tracking-widest mb-3">
              {{ exam.type }}
           </div>
           <h3 class="text-xl font-semibold text-heading leading-tight group-hover:text-link transition-colors line-clamp-2 min-h-[56px]">{{ exam.name }}</h3>
           <p class="mt-2 text-xs font-semibold text-muted">{{ exam.subjectName }} · {{ exam.classSectionCode }}</p>
           
           <div class="mt-6 flex flex-col gap-3">
              <div class="flex items-center gap-3">
                 <div class="flex items-center justify-center w-8 h-8 rounded-full bg-[var(--accent-primary)]/5 text-muted">
                    <Calendar :size="14" />
                 </div>
                 <div>
                    <p class="text-[10px] font-semibold uppercase tracking-widest text-muted">Thời gian mở đề</p>
                    <p class="text-sm font-bold text-heading">{{ formatTime(exam.openAt) }} - {{ formatTime(exam.closeAt) }}</p>
                 </div>
              </div>
              <div class="grid grid-cols-3 gap-2 text-center">
                <div class="rounded-xl bg-[var(--surface-input)] border border-card px-2 py-2">
                  <p class="text-[9px] font-semibold uppercase tracking-widest text-muted">Được làm</p>
                  <p class="text-sm font-semibold text-heading">{{ exam.allowedStudents }}</p>
                </div>
                <div class="rounded-xl bg-[var(--color-success-bg)] border border-[var(--color-success-bg)] px-2 py-2">
                  <p class="text-[9px] font-semibold uppercase tracking-widest text-[var(--color-success-text)]">Đã làm</p>
                  <p class="text-sm font-semibold" style="color:var(--color-success-text)">{{ exam.completedStudents }}</p>
                </div>
                <div class="rounded-xl bg-[var(--color-warning-bg)] border border-[var(--color-warning-bg)] px-2 py-2">
                  <p class="text-[9px] font-semibold uppercase tracking-widest text-[var(--color-warning-text)]">Chưa</p>
                  <p class="text-sm font-semibold" style="color:var(--color-warning-text)">{{ exam.pendingStudents }}</p>
                </div>
              </div>
              <div class="flex items-center gap-4 mt-2 pt-4 border-t border-card">
                 <div class="flex items-center gap-2">
                    <Clock :size="14" class="text-link" />
                    <span class="text-sm font-bold text-body">{{ exam.duration }}</span>
                 </div>
                 <div class="w-1 h-1 rounded-full bg-[var(--border-default)]"></div>
                 <div class="flex items-center gap-2">
                    <HelpCircle :size="14" class="text-link" />
                    <span class="text-sm font-bold text-body">{{ exam.questions }} câu hỏi</span>
                 </div>
              </div>
              <div class="flex items-center gap-2 rounded-xl bg-[var(--color-info-bg)] px-3 py-2 text-[10px] font-bold" style="color:var(--color-info-text);border:1px solid var(--color-info-bg)">
                <ShieldCheck :size="13" />
                Không cần mật khẩu đề thi. Truy cập theo lớp, lịch mở đề và số lần làm.
              </div>
           </div>
        </div>

        <div class="mt-8 pt-6 border-t border-card flex gap-3">
           <GlassButton variant="secondary" size="sm" class="flex-1" @click="openConfigModal(exam)">
              Cấu hình
           </GlassButton>
           <GlassButton v-if="normalizeExamStatus(exam.status) === EXAM_STATUS.DRAFT" variant="primary" size="sm" class="flex-1" @click="publishExam(exam.id)">
              <Send :size="14" /> Lên lịch
           </GlassButton>
           <GlassButton v-else variant="success" size="sm" class="flex-1">
              <CheckCircle2 :size="14" /> Theo dõi
           </GlassButton>
        </div>
      </div>
    </div>
  <!-- Configure Exam Modal -->
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="isConfigModalOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-[var(--overlay-bg)] backdrop-blur-sm" @click="isConfigModalOpen = false"></div>
        <div class="relative surface-card rounded-2xl shadow-[var(--lg-shadow-md)] w-full max-w-2xl overflow-hidden animate-modal-in">
          <div class="p-4 border-b border-card bg-[var(--surface-input)] flex items-center justify-between">
            <div>
               <h3 class="text-lg font-semibold text-heading">Cấu hình Đề thi</h3>
               <p class="text-sm text-muted mt-1">Quyền vào đề được kiểm soát bằng lớp học phần, lịch mở đề và số lần làm.</p>
            </div>
            <div class="h-10 w-10 rounded-xl bg-[var(--accent-primary)]/10 flex items-center justify-center text-link shadow-sm">
               <Settings :size="24" />
            </div>
           </div>

           <div class="p-4 space-y-4 max-h-[60vh] overflow-y-auto">
             <div class="space-y-4">
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wider flex items-center gap-2"><FileEdit :size="16" style="color:var(--accent-primary)" /> Thông tin cơ bản</h4>
               <div>
                 <label class="block text-sm font-semibold text-body mb-1.5">Tên đề thi</label>
                 <input v-model="configuringExam.name" type="text" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
               </div>
               <div class="grid grid-cols-2 gap-4">
                 <div>
                   <label class="block text-sm font-semibold text-body mb-1.5">Môn học</label>
                   <input v-model="configuringExam.subjectName" type="text" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
                 </div>
                 <div>
<label class="block text-sm font-semibold text-body mb-1.5">Lớp học phần áp dụng</label>
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Thời gian làm bài</label>
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Ngày diễn ra</label>
                   <input v-model="configuringExam.date" type="text" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
                 </div>
               </div>
             </div>

             <div class="space-y-4 pt-4 border-t border-card">
                <h4 class="text-sm font-semibold text-heading uppercase tracking-wider flex items-center gap-2"><Settings :size="16" style="color:var(--color-warning-text)" /> Điều kiện mở đề</h4>

                <div class="grid grid-cols-2 gap-4">
                  <div>
<label class="block text-sm font-semibold text-body mb-1.5">Thời gian mở</label>
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Thời gian đóng</label>
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Số lần làm bài tối đa</label>
                    <select v-model="configuringExam.maxAttempts" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors">
                      <option :value="1">1 lần</option>
                      <option :value="2">2 lần</option>
                      <option :value="3">3 lần</option>
                      <option :value="999">Không giới hạn</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Trạng thái</label>
                    <select v-model="configuringExam.status" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors">
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
<label class="block text-sm font-semibold text-body mb-1.5">SV được phép</label>
                     <input v-model.number="configuringExam.allowedStudents" type="number" min="0" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Đã làm</label>
                     <input v-model.number="configuringExam.completedStudents" type="number" min="0" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
                  </div>
                  <div>
                    <label class="block text-sm font-semibold text-body mb-1.5">Chưa làm</label>
                    <input v-model.number="configuringExam.pendingStudents" type="number" min="0" class="w-full rounded-xl border border-input bg-surface-input px-4 py-2.5 text-sm outline-none focus:border-[var(--accent-primary)] transition-colors" />
               </div>
                </div>

                <div class="flex items-center justify-between p-4 rounded-xl border border-card bg-[var(--surface-input)] mt-2">
                  <div>
                     <p class="text-sm font-semibold text-heading">Cho phép làm/học trước</p>
                     <p class="text-xs font-medium text-muted mt-0.5">Sinh viên thuộc lớp được phép thấy cảnh báo trước khi làm trước lộ trình.</p>
                  </div>
                  <label class="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" v-model="configuringExam.allowEarlyLearning" class="sr-only peer">
                    <div class="w-11 h-6 bg-[var(--border-input)] peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-[var(--border-input)] after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-link"></div>
                  </label>
               </div>
               <div class="rounded-xl border border-[var(--color-info-bg)] bg-[var(--color-info-bg)] p-3 text-xs font-bold leading-relaxed" style="color:var(--color-info-text)">
                 Không có trường mật khẩu/access code cho đề thi. Backend sau này cần enforce classSectionIds, openAt, closeAt, maxAttempts và status.
               </div>
              </div>
           </div>

           <div class="p-4 border-t border-card bg-[var(--surface-input)] flex justify-end gap-3">
            <GlassButton variant="ghost" @click="isConfigModalOpen = false">Đóng</GlassButton>
            <GlassButton variant="primary" @click="saveConfig">Lưu cấu hình</GlassButton>
          </div>
        </div>
      </div>
    </Transition>
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

@keyframes modal-in {
  from { opacity: 0; transform: scale(0.96) translateY(15px); }
  to   { opacity: 1; transform: scale(1) translateY(0); }
}
.animate-modal-in {
  animation: modal-in 0.35s cubic-bezier(0.16, 1, 0.3, 1) both;
}

.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
