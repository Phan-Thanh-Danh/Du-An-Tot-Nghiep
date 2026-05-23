<script setup>
import { ref, computed } from 'vue'
import * as L from 'lucide-vue-next'
import {
  mockAssignment, mockSubmissionRules, mockAttachments,
  mockSubmissions, mockPlagiarismResult, mockFeedback,
} from '@/data/assignmentDetail.mock.js'

const icon = (n) => L[n] || L.Circle

// ── Deadline countdown (mock) ──────────────────────────────
const deadlinePassed = false // mock: chưa quá hạn
const deadlineUrgent = true  // mock: còn < 24h
const deadlineText = 'Còn 10 ngày 23 giờ'

// ── File upload state ──────────────────────────────────────
const isDragging = ref(false)
const selectedFiles = ref([])
const submitted = ref(false)
const showToast = ref(false)

const rules = mockSubmissionRules
const attemptsLeft = rules.maxAttempts - rules.currentAttempt

function validateFile(file) {
  const ext = '.' + file.name.split('.').pop().toLowerCase()
  if (!rules.allowedFormats.includes(ext)) return 'invalid'
  if (file.size > rules.maxSizeMB * 1024 * 1024) return 'toolarge'
  return 'valid'
}

function onDrop(e) {
  isDragging.value = false
  const files = Array.from(e.dataTransfer?.files || e.target?.files || [])
  files.forEach(f => {
    if (!selectedFiles.value.find(sf => sf.name === f.name)) {
      selectedFiles.value.push({ file: f, name: f.name, size: (f.size / 1024).toFixed(0) + ' KB', status: validateFile(f) })
    }
  })
}

function removeFile(name) { selectedFiles.value = selectedFiles.value.filter(f => f.name !== name) }

const canSubmit = computed(() =>
  !submitted.value &&
  !deadlinePassed &&
  attemptsLeft > 0 &&
  selectedFiles.value.length > 0 &&
  selectedFiles.value.every(f => f.status === 'valid')
)

const submissions = ref([...mockSubmissions])

function doSubmit() {
  if (!canSubmit.value) return
  submissions.value.forEach(s => s.isLatest = false)
  submissions.value.push({
    id: 's' + Date.now(), attempt: rules.currentAttempt + 1,
    submittedAt: new Date().toLocaleString('vi-VN'), status: 'checking',
    statusLabel: 'Đang kiểm tra', onTime: true, timeLabel: 'Đúng hạn',
    file: selectedFiles.value[0]?.name || 'file.zip',
    fileSize: selectedFiles.value[0]?.size || '–', note: '', isLatest: true,
  })
  selectedFiles.value = []
  submitted.value = true
  showToast.value = true
  setTimeout(() => showToast.value = false, 3500)
}

// ── Style helpers ──────────────────────────────────────────
const statusStyle = {
  submitted: 'bg-blue-100 text-blue-700',
  graded: 'bg-green-100 text-green-700',
  pending: 'bg-slate-100 text-slate-600',
  late: 'bg-orange-100 text-orange-700',
  overdue: 'bg-red-100 text-red-700',
  checking: 'bg-amber-100 text-amber-700',
}
const plagStyle = {
  safe: { bar: 'from-green-500 to-teal-400', badge: 'bg-green-100 text-green-700', label: 'An toàn' },
  warning: { bar: 'from-amber-500 to-orange-400', badge: 'bg-amber-100 text-amber-700', label: 'Cần chú ý' },
  danger: { bar: 'from-red-500 to-rose-400', badge: 'bg-red-100 text-red-700', label: 'Cảnh báo cao' },
  unchecked: { bar: 'from-slate-300 to-slate-400', badge: 'bg-slate-100 text-slate-500', label: 'Chưa kiểm tra' },
}
const ps = plagStyle[mockPlagiarismResult.status] || plagStyle.unchecked
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-6">

    <!-- Toast -->
    <Transition enter-active-class="transition-all duration-300" enter-from-class="opacity-0 translate-y-2" leave-active-class="transition-all duration-200" leave-to-class="opacity-0">
      <div v-if="showToast" class="fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-2xl bg-green-600 px-5 py-3.5 text-sm font-semibold text-white shadow-xl">
        <component :is="icon('CheckCircle2')" :size="18" /> Nộp bài thành công!
      </div>
    </Transition>

    <!-- HERO -->
    <div class="relative overflow-hidden rounded-[24px] bg-gradient-to-br from-violet-700 via-violet-600 to-blue-500 p-6 text-white shadow-lg">
      <div class="pointer-events-none absolute right-0 top-0 h-56 w-56 rounded-full bg-white/10 blur-3xl" />
      <div class="relative flex flex-wrap items-start justify-between gap-4">
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 text-violet-200 text-xs font-semibold mb-2">
            <component :is="icon('ClipboardList')" :size="14" />
            {{ mockAssignment.courseCode }} · {{ mockAssignment.class }}
          </div>
          <h1 class="text-xl font-bold leading-tight">{{ mockAssignment.title }}</h1>
          <p class="mt-1 text-violet-200 text-sm">{{ mockAssignment.teacher }} · Hạn nộp: {{ mockAssignment.deadlineDisplay }}</p>
        </div>
        <div class="flex flex-wrap gap-2 shrink-0">
          <span :class="['rounded-xl px-3 py-1.5 text-xs font-semibold', statusStyle[mockAssignment.status] || 'bg-white/20 text-white']">
            {{ mockAssignment.statusLabel }}
          </span>
          <router-link to="/student/assignments" class="inline-flex items-center gap-1.5 rounded-xl bg-white/20 px-3 py-1.5 text-xs font-semibold hover:bg-white/30 transition-colors">
            <component :is="icon('ArrowLeft')" :size="13" /> Danh sách bài tập
          </router-link>
        </div>
      </div>
    </div>

    <!-- STATS -->
    <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <div class="rounded-[20px] border border-white/60 bg-white/80 p-4 shadow-sm backdrop-blur-md">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-blue-50 text-blue-600"><component :is="icon('ClipboardCheck')" :size="15" /></div>
          <span class="text-xs font-semibold text-slate-500">Trạng thái</span>
        </div>
        <span :class="['rounded-full px-2.5 py-1 text-xs font-bold', statusStyle[mockAssignment.status]]">{{ mockAssignment.statusLabel }}</span>
      </div>
      <div class="rounded-[20px] border border-white/60 bg-white/80 p-4 shadow-sm backdrop-blur-md">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-orange-50 text-orange-600"><component :is="icon('Clock')" :size="15" /></div>
          <span class="text-xs font-semibold text-slate-500">Deadline</span>
        </div>
        <p class="text-sm font-bold text-slate-900">{{ mockAssignment.deadlineDisplay }}</p>
        <p :class="['text-xs mt-0.5 font-medium', deadlinePassed ? 'text-red-500' : deadlineUrgent ? 'text-orange-500' : 'text-slate-400']">{{ deadlineText }}</p>
      </div>
      <div class="rounded-[20px] border border-white/60 bg-white/80 p-4 shadow-sm backdrop-blur-md">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-violet-50 text-violet-600"><component :is="icon('RefreshCw')" :size="15" /></div>
          <span class="text-xs font-semibold text-slate-500">Lượt nộp</span>
        </div>
        <p class="text-2xl font-bold text-slate-900">{{ rules.currentAttempt }}<span class="text-sm text-slate-400">/{{ rules.maxAttempts }}</span></p>
        <p class="text-xs text-slate-400 mt-0.5">Còn {{ attemptsLeft }} lượt</p>
      </div>
      <div class="rounded-[20px] border border-white/60 bg-white/80 p-4 shadow-sm backdrop-blur-md">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-8 w-8 items-center justify-center rounded-xl bg-green-50 text-green-600"><component :is="icon('Star')" :size="15" /></div>
          <span class="text-xs font-semibold text-slate-500">Điểm số</span>
        </div>
        <p v-if="mockFeedback.graded" class="text-2xl font-bold text-green-600">{{ mockFeedback.score }}<span class="text-sm text-slate-400">/{{ mockFeedback.maxScore }}</span></p>
        <p v-else class="text-sm font-semibold text-slate-500">Chờ chấm</p>
      </div>
    </div>

    <!-- MAIN CONTENT -->
    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">

      <!-- LEFT -->
      <div class="space-y-4">

        <!-- Yêu cầu bài tập -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 bg-white/60 px-5 py-4 flex items-center gap-2">
            <component :is="icon('BookOpen')" :size="16" class="text-violet-600" />
            <h2 class="text-base font-bold text-slate-900">Yêu cầu bài tập</h2>
          </div>
          <div class="px-5 py-4">
            <p class="text-sm leading-7 text-slate-700 whitespace-pre-line">{{ mockAssignment.description }}</p>
          </div>
          <div v-if="mockAttachments.length" class="border-t border-slate-100 px-5 py-4">
            <p class="text-xs font-semibold text-slate-500 mb-3 flex items-center gap-1.5"><component :is="icon('Paperclip')" :size="13" /> File đính kèm</p>
            <div class="space-y-2">
              <div v-for="a in mockAttachments" :key="a.id" class="flex items-center gap-3 rounded-xl border border-slate-100 bg-slate-50 px-3 py-2.5 hover:bg-slate-100 transition-colors cursor-pointer">
                <component :is="icon('FileText')" :size="16" class="text-violet-500 shrink-0" />
                <span class="flex-1 text-sm font-medium text-slate-700">{{ a.name }}</span>
                <span class="text-xs text-slate-400">{{ a.size }}</span>
                <component :is="icon('Download')" :size="14" class="text-slate-400" />
              </div>
            </div>
          </div>
        </div>

        <!-- Upload Box -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 bg-white/60 px-5 py-4 flex items-center justify-between">
            <div class="flex items-center gap-2">
              <component :is="icon('Upload')" :size="16" class="text-blue-600" />
              <h2 class="text-base font-bold text-slate-900">Nộp bài</h2>
            </div>
            <span v-if="attemptsLeft === 0" class="rounded-full bg-red-100 px-2.5 py-1 text-xs font-semibold text-red-600">Hết lượt nộp</span>
            <span v-else class="rounded-full bg-blue-100 px-2.5 py-1 text-xs font-semibold text-blue-700">Có thể nộp · Còn {{ attemptsLeft }} lượt</span>
          </div>
          <div class="px-5 py-4 space-y-4">
            <!-- Drag drop -->
            <label @dragover.prevent="isDragging = true" @dragleave="isDragging = false" @drop.prevent="onDrop"
              :class="['flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-dashed p-8 transition-colors cursor-pointer', isDragging ? 'border-blue-400 bg-blue-50' : 'border-slate-200 bg-slate-50 hover:border-blue-300 hover:bg-blue-50/40']">
              <input type="file" class="hidden" multiple @change="onDrop" :accept="rules.allowedFormats.join(',')" />
              <component :is="icon('UploadCloud')" :size="36" :class="isDragging ? 'text-blue-500' : 'text-slate-300'" />
              <div class="text-center">
                <p class="text-sm font-semibold text-slate-600">Kéo thả file vào đây hoặc bấm để chọn file</p>
                <p class="text-xs text-slate-400 mt-1">Định dạng: {{ rules.allowedFormats.join(', ') }} · Tối đa {{ rules.maxSizeMB }} MB</p>
              </div>
            </label>

            <!-- File list -->
            <div v-if="selectedFiles.length" class="space-y-2">
              <div v-for="f in selectedFiles" :key="f.name"
                :class="['flex items-center gap-3 rounded-xl border px-3 py-2.5 text-sm', f.status === 'valid' ? 'border-green-200 bg-green-50' : 'border-red-200 bg-red-50']">
                <component :is="icon(f.status === 'valid' ? 'CheckCircle2' : 'XCircle')" :size="16" :class="f.status === 'valid' ? 'text-green-500' : 'text-red-500'" />
                <span class="flex-1 font-medium truncate" :class="f.status === 'valid' ? 'text-slate-700' : 'text-red-700'">{{ f.name }}</span>
                <span class="text-xs text-slate-400 shrink-0">{{ f.size }}</span>
                <span v-if="f.status === 'invalid'" class="text-xs text-red-500 shrink-0">Sai định dạng</span>
                <span v-if="f.status === 'toolarge'" class="text-xs text-red-500 shrink-0">Quá dung lượng</span>
                <button @click="removeFile(f.name)" class="ml-1 text-slate-400 hover:text-red-500 transition-colors"><component :is="icon('X')" :size="14" /></button>
              </div>
            </div>

            <button @click="doSubmit" :disabled="!canSubmit"
              :class="['inline-flex w-full items-center justify-center gap-2 rounded-xl py-3 text-sm font-bold transition-all shadow-sm', canSubmit ? 'bg-blue-600 text-white hover:bg-blue-700 shadow-blue-500/20' : 'bg-slate-200 text-slate-400 cursor-not-allowed']">
              <component :is="icon('Send')" :size="16" />
              {{ submitted ? 'Đã nộp bài' : 'Nộp bài' }}
            </button>
          </div>
        </div>

        <!-- Submission History -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 bg-white/60 px-5 py-4 flex items-center gap-2">
            <component :is="icon('History')" :size="16" class="text-slate-500" />
            <h2 class="text-base font-bold text-slate-900">Lịch sử nộp bài</h2>
          </div>
          <div class="divide-y divide-slate-100">
            <div v-for="s in [...submissions].reverse()" :key="s.id"
              :class="['px-5 py-4 transition-colors', s.isLatest ? 'bg-blue-50/50' : '']">
              <div class="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <div class="flex items-center gap-2 mb-1">
                    <span class="text-sm font-bold text-slate-900">Lần {{ s.attempt }}</span>
                    <span v-if="s.isLatest" class="rounded-full bg-blue-100 px-2 py-0.5 text-[11px] font-semibold text-blue-700">Mới nhất</span>
                    <span :class="['rounded-full px-2 py-0.5 text-[11px] font-semibold', statusStyle[s.status] || 'bg-slate-100 text-slate-500']">{{ s.statusLabel }}</span>
                    <span :class="['text-[11px] font-semibold', s.onTime ? 'text-green-600' : 'text-orange-600']">{{ s.timeLabel }}</span>
                  </div>
                  <p class="text-xs text-slate-500">{{ s.submittedAt }}</p>
                  <div class="flex items-center gap-2 mt-2">
                    <component :is="icon('FileArchive')" :size="13" class="text-slate-400" />
                    <span class="text-xs text-slate-600 font-medium">{{ s.file }}</span>
                    <span class="text-xs text-slate-400">· {{ s.fileSize }}</span>
                  </div>
                  <p v-if="s.note" class="text-xs text-slate-400 mt-1">{{ s.note }}</p>
                </div>
                <button class="inline-flex items-center gap-1.5 rounded-xl border border-slate-200 bg-white px-3 py-1.5 text-xs font-semibold text-slate-600 hover:bg-slate-50 transition-colors">
                  <component :is="icon('Download')" :size="13" /> Tải xuống
                </button>
              </div>
            </div>
            <div v-if="!submissions.length" class="px-5 py-8 text-center text-sm text-slate-400">Chưa có lần nộp nào.</div>
          </div>
        </div>
      </div>

      <!-- RIGHT SIDEBAR -->
      <div class="space-y-4">

        <!-- Deadline Reminder -->
        <div :class="['rounded-[24px] border p-4 shadow-sm', deadlinePassed ? 'border-red-200 bg-red-50' : deadlineUrgent ? 'border-orange-200 bg-orange-50' : 'border-green-200 bg-green-50']">
          <div class="flex items-center gap-2 mb-2">
            <component :is="icon('AlarmClock')" :size="16" :class="deadlinePassed ? 'text-red-500' : deadlineUrgent ? 'text-orange-500' : 'text-green-600'" />
            <span class="text-sm font-bold" :class="deadlinePassed ? 'text-red-700' : deadlineUrgent ? 'text-orange-700' : 'text-green-700'">Nhắc nhở deadline</span>
          </div>
          <p class="text-lg font-bold" :class="deadlinePassed ? 'text-red-700' : deadlineUrgent ? 'text-orange-700' : 'text-green-700'">{{ deadlineText }}</p>
          <p class="text-xs mt-1" :class="deadlinePassed ? 'text-red-500' : 'text-slate-500'">Hạn nộp: {{ mockAssignment.deadlineDisplay }}</p>
        </div>

        <!-- Submission Rules -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 px-5 py-4 flex items-center gap-2">
            <component :is="icon('ShieldCheck')" :size="16" class="text-blue-500" />
            <h3 class="text-sm font-bold text-slate-900">Điều kiện nộp bài</h3>
          </div>
          <div class="px-5 py-4 space-y-3">
            <div>
              <p class="text-xs font-semibold text-slate-500 mb-2">Định dạng cho phép</p>
              <div class="flex flex-wrap gap-1.5">
                <span v-for="fmt in rules.allowedFormats" :key="fmt" class="rounded-full bg-blue-100 px-2.5 py-1 text-xs font-bold text-blue-700">{{ fmt }}</span>
              </div>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-slate-500">Dung lượng tối đa</span>
              <span class="font-bold text-slate-900">{{ rules.maxSizeMB }} MB</span>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-slate-500">Số lần nộp</span>
              <span class="font-bold text-slate-900">{{ rules.currentAttempt }}/{{ rules.maxAttempts }}</span>
            </div>
            <p class="text-xs text-slate-400 leading-5 border-t border-slate-100 pt-3">{{ rules.note }}</p>
          </div>
        </div>

        <!-- Plagiarism -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 px-5 py-4 flex items-center gap-2">
            <component :is="icon('ScanSearch')" :size="16" class="text-amber-500" />
            <h3 class="text-sm font-bold text-slate-900">Kiểm tra đạo văn</h3>
          </div>
          <div class="px-5 py-4">
            <div class="flex items-center justify-between mb-3">
              <span :class="['rounded-full px-2.5 py-1 text-xs font-bold', ps.badge]">{{ ps.label }}</span>
              <span class="text-2xl font-bold text-slate-900">{{ mockPlagiarismResult.percentage }}%</span>
            </div>
            <div class="h-2 overflow-hidden rounded-full bg-slate-200 mb-3">
              <div :class="['h-full rounded-full bg-gradient-to-r', ps.bar]" :style="{ width: mockPlagiarismResult.percentage + '%' }" />
            </div>
            <p class="text-xs text-slate-500">{{ mockPlagiarismResult.detail }}</p>
            <p class="text-xs text-slate-400 mt-1">Kiểm tra lúc: {{ mockPlagiarismResult.checkedAt }}</p>
          </div>
        </div>

        <!-- Feedback -->
        <div class="rounded-[24px] border border-white/60 bg-white/80 shadow-sm backdrop-blur-md overflow-hidden">
          <div class="border-b border-slate-100 px-5 py-4 flex items-center gap-2">
            <component :is="icon('MessageSquare')" :size="16" class="text-green-600" />
            <h3 class="text-sm font-bold text-slate-900">Kết quả & Phản hồi</h3>
          </div>
          <div v-if="mockFeedback.graded" class="px-5 py-4 space-y-4">
            <div class="flex items-center gap-3 rounded-2xl bg-green-50 border border-green-100 p-3">
              <div class="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-green-600 text-white font-bold text-lg">{{ mockFeedback.score }}</div>
              <div>
                <p class="text-sm font-bold text-slate-900">Điểm: {{ mockFeedback.score }}/{{ mockFeedback.maxScore }}</p>
                <p class="text-xs text-slate-500">Chấm ngày {{ mockFeedback.gradedAt }} bởi {{ mockFeedback.gradedBy }}</p>
              </div>
            </div>
            <!-- Rubric -->
            <div class="space-y-2">
              <p class="text-xs font-semibold text-slate-500">Tiêu chí chấm điểm</p>
              <div v-for="r in mockFeedback.rubric" :key="r.label" class="flex items-center justify-between text-xs">
                <span class="text-slate-600">{{ r.label }}</span>
                <span :class="['font-bold', r.score === r.max ? 'text-green-600' : 'text-orange-500']">{{ r.score }}/{{ r.max }}</span>
              </div>
            </div>
            <!-- Teacher comment -->
            <div class="rounded-xl border border-slate-100 bg-slate-50 p-3">
              <p class="text-xs font-semibold text-slate-500 mb-1.5">Nhận xét giảng viên</p>
              <p class="text-sm text-slate-700 leading-6">{{ mockFeedback.comment }}</p>
            </div>
            <!-- AI suggestion -->
            <div class="rounded-xl border border-violet-100 bg-violet-50 p-3">
              <div class="flex items-center gap-1.5 mb-1.5">
                <component :is="icon('Sparkles')" :size="13" class="text-violet-500" />
                <p class="text-xs font-semibold text-violet-600">AI Gợi ý</p>
              </div>
              <p class="text-xs text-violet-700 leading-5">{{ mockFeedback.aiSuggestion }}</p>
            </div>
          </div>
          <div v-else class="px-5 py-6 text-center">
            <component :is="icon('Hourglass')" :size="28" class="mx-auto text-slate-300 mb-2" />
            <p class="text-sm font-semibold text-slate-600">Đang chờ giảng viên chấm</p>
            <p class="text-xs text-slate-400 mt-1">Kết quả sẽ được thông báo qua email.</p>
          </div>
        </div>

      </div>
    </div>
  </div>
</template>
