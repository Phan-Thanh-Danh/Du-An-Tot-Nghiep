<script setup>
import { ref, computed } from 'vue'
import * as L from 'lucide-vue-next'
import {
  mockAssignment, mockSubmissionRules, mockAttachments,
  mockSubmissions, mockPlagiarismResult, mockFeedback,
} from '@/data/assignmentDetail.mock.js'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const icon = (n) => L[n] || L.Circle

// ── Deadline countdown (mock) ──────────────────────────────
const deadlinePassed = false
const deadlineUrgent = true
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
const statusBadgeVariant = (s) => ({
  submitted: 'primary',
  graded: 'success',
  pending: 'info',
  late: 'warning',
  overdue: 'danger',
  checking: 'warning',
}[s] || 'neutral')

const statusPillStyle = (s) => {
  const map = {
    submitted: { bg: 'var(--accent-primary-soft)', color: 'var(--accent-primary)' },
    graded: { bg: 'var(--color-success-bg)', color: 'var(--color-success-text)' },
    pending: { bg: 'var(--color-info-bg)', color: 'var(--color-info-text)' },
    late: { bg: 'var(--color-warning-bg)', color: 'var(--color-warning-text)' },
    overdue: { bg: 'var(--color-danger-bg)', color: 'var(--color-danger-text)' },
    checking: { bg: 'var(--color-warning-bg)', color: 'var(--color-warning-text)' },
  }
  return map[s] || { bg: 'var(--surface-input)', color: 'var(--text-muted)' }
}

const plagStyle = {
  safe: { bar: 'var(--color-success-bg)', variant: 'success', label: 'An toàn' },
  warning: { bar: 'var(--color-warning-bg)', variant: 'warning', label: 'Cần chú ý' },
  danger: { bar: 'var(--color-danger-bg)', variant: 'danger', label: 'Cảnh báo cao' },
  unchecked: { bar: 'var(--color-info-bg)', variant: 'info', label: 'Chưa kiểm tra' },
}
const ps = plagStyle[mockPlagiarismResult.status] || plagStyle.unchecked
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-6">

    <!-- Toast -->
    <Transition enter-active-class="transition-all duration-300" enter-from-class="opacity-0 translate-y-2" leave-active-class="transition-all duration-200" leave-to-class="opacity-0">
      <div v-if="showToast" class="fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-semibold text-white shadow-lg" style="background:var(--lg-success)">
        <component :is="icon('CheckCircle2')" :size="18" /> Nộp bài thành công!
      </div>
    </Transition>

    <!-- Header -->
    <GlassPanel variant="flat" density="compact">
      <div class="flex flex-wrap items-start justify-between gap-3">
        <div class="flex-1 min-w-0">
          <div class="flex items-center gap-2 text-label text-xs font-semibold mb-1.5">
            <component :is="icon('ClipboardList')" :size="13" />
            {{ mockAssignment.courseCode }} · {{ mockAssignment.class }}
          </div>
          <h1 class="text-heading text-lg font-semibold leading-tight">{{ mockAssignment.title }}</h1>
          <p class="mt-0.5 text-muted text-sm">{{ mockAssignment.teacher }} · Hạn nộp: {{ mockAssignment.deadlineDisplay }}</p>
        </div>
        <div class="flex flex-wrap items-center gap-2 shrink-0">
          <GlassBadge :variant="statusBadgeVariant(mockAssignment.status)" size="md">
            {{ mockAssignment.statusLabel }}
          </GlassBadge>
          <router-link to="/student/assignments" class="inline-flex items-center gap-1.5 rounded-lg border border-card px-2.5 py-1.5 text-xs font-semibold text-link transition-colors hover:bg-surface-input">
            <component :is="icon('ArrowLeft')" :size="13" /> Danh sách
          </router-link>
        </div>
      </div>
    </GlassPanel>

    <!-- Stats -->
    <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <GlassPanel variant="flat" density="compact">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--accent-primary-soft);color:var(--accent-primary)">
            <component :is="icon('ClipboardCheck')" :size="13" />
          </div>
          <span class="text-label text-xs font-semibold">Trạng thái</span>
        </div>
        <GlassBadge :variant="statusBadgeVariant(mockAssignment.status)" size="sm">
          {{ mockAssignment.statusLabel }}
        </GlassBadge>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-warning-bg);color:var(--color-warning-text)">
            <component :is="icon('Clock')" :size="13" />
          </div>
          <span class="text-label text-xs font-semibold">Deadline</span>
        </div>
        <p class="text-sm font-semibold text-heading">{{ mockAssignment.deadlineDisplay }}</p>
        <p class="text-xs mt-0.5 font-medium" :style="{ color: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--text-muted)' }">{{ deadlineText }}</p>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-info-bg);color:var(--color-info-text)">
            <component :is="icon('RefreshCw')" :size="13" />
          </div>
          <span class="text-label text-xs font-semibold">Lượt nộp</span>
        </div>
        <p class="text-lg font-semibold text-heading">{{ rules.currentAttempt }}<span class="text-sm text-muted">/{{ rules.maxAttempts }}</span></p>
        <p class="text-xs text-muted mt-0.5">Còn {{ attemptsLeft }} lượt</p>
      </GlassPanel>
      <GlassPanel variant="flat" density="compact">
        <div class="flex items-center gap-2 mb-2">
          <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-success-bg);color:var(--color-success-text)">
            <component :is="icon('Star')" :size="13" />
          </div>
          <span class="text-label text-xs font-semibold">Điểm số</span>
        </div>
        <p v-if="mockFeedback.graded" class="text-lg font-semibold" style="color:var(--color-success-text)">{{ mockFeedback.score }}<span class="text-sm text-muted">/{{ mockFeedback.maxScore }}</span></p>
        <p v-else class="text-sm font-semibold text-muted">Chờ chấm</p>
      </GlassPanel>
    </div>

    <!-- Main Content -->
    <div class="grid gap-4 xl:grid-cols-[minmax(0,1fr)_320px]">

      <!-- LEFT -->
      <div class="space-y-4">

        <!-- Assignment Requirements -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2">
              <component :is="icon('BookOpen')" :size="15" style="color:var(--accent-primary)" />
              <h2 class="text-sm font-semibold text-heading">Yêu cầu bài tập</h2>
            </div>
          </template>
          <p class="text-sm leading-7 text-body whitespace-pre-line">{{ mockAssignment.description }}</p>
          <div v-if="mockAttachments.length" class="mt-4 pt-4 border-t border-card">
            <p class="text-xs font-semibold text-muted mb-3 flex items-center gap-1.5">
              <component :is="icon('Paperclip')" :size="13" /> File đính kèm
            </p>
            <div class="space-y-1.5">
              <div v-for="a in mockAttachments" :key="a.id" class="flex items-center gap-3 rounded-lg border border-card bg-surface-input px-3 py-2 hover:bg-surface-card-hover transition-colors cursor-pointer">
                <component :is="icon('FileText')" :size="14" style="color:var(--accent-primary)" class="shrink-0" />
                <span class="flex-1 text-sm font-medium text-body">{{ a.name }}</span>
                <span class="text-xs text-muted">{{ a.size }}</span>
                <component :is="icon('Download')" :size="13" class="text-muted" />
              </div>
            </div>
          </div>
        </GlassPanel>

        <!-- Upload -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <component :is="icon('Upload')" :size="15" style="color:var(--accent-primary)" />
                <h2 class="text-sm font-semibold text-heading">Nộp bài</h2>
              </div>
              <GlassBadge v-if="attemptsLeft === 0" variant="danger" size="sm">Hết lượt nộp</GlassBadge>
              <GlassBadge v-else variant="primary" size="sm">Có thể nộp · Còn {{ attemptsLeft }} lượt</GlassBadge>
            </div>
          </template>

          <div class="space-y-3">
            <!-- Drag drop -->
            <label @dragover.prevent="isDragging = true" @dragleave="isDragging = false" @drop.prevent="onDrop"
              :class="['flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed p-4 transition-colors cursor-pointer', isDragging ? 'border-link' : 'border-input bg-surface-input hover:border-link']"
              :style="isDragging ? { background: 'var(--accent-primary-soft)', borderColor: 'var(--text-link)' } : {}">
              <input type="file" class="hidden" multiple @change="onDrop" :accept="rules.allowedFormats.join(',')" />
              <component :is="icon('UploadCloud')" :size="26" :class="isDragging ? 'text-link' : 'text-placeholder'" />
              <div class="text-center">
                <p class="text-xs font-semibold text-body">Kéo thả file vào đây hoặc bấm để chọn file</p>
                <p class="text-xs text-muted mt-1">Định dạng: {{ rules.allowedFormats.join(', ') }} · Tối đa {{ rules.maxSizeMB }} MB</p>
              </div>
            </label>

            <!-- File list -->
            <div v-if="selectedFiles.length" class="space-y-1.5">
              <div v-for="f in selectedFiles" :key="f.name"
                class="flex items-center gap-2.5 rounded-lg border border-card px-3 py-2 text-sm"
                :style="{ background: f.status === 'valid' ? 'var(--color-success-bg)' : 'var(--color-danger-bg)' }">
                <component :is="icon(f.status === 'valid' ? 'CheckCircle2' : 'XCircle')" :size="14" :style="{ color: f.status === 'valid' ? 'var(--color-success-text)' : 'var(--color-danger-text)' }" />
                <span class="flex-1 font-medium truncate text-body">{{ f.name }}</span>
                <span class="text-xs text-muted shrink-0">{{ f.size }}</span>
                <span v-if="f.status === 'invalid'" class="text-xs shrink-0" style="color:var(--color-danger-text)">Sai định dạng</span>
                <span v-if="f.status === 'toolarge'" class="text-xs shrink-0" style="color:var(--color-danger-text)">Quá dung lượng</span>
                <button @click="removeFile(f.name)" class="remove-file-button ml-1 text-muted transition-colors"><component :is="icon('X')" :size="13" /></button>
              </div>
            </div>

            <GlassButton variant="primary" :disabled="!canSubmit" block @click="doSubmit">
              <template #leading><component :is="icon('Send')" :size="15" /></template>
              {{ submitted ? 'Đã nộp bài' : 'Nộp bài' }}
            </GlassButton>
          </div>
        </GlassPanel>

        <!-- Submission History -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2">
              <component :is="icon('History')" :size="15" class="text-muted" />
              <h2 class="text-sm font-semibold text-heading">Lịch sử nộp bài</h2>
            </div>
          </template>
          <div class="divide-y divide-border-card">
            <div v-for="s in [...submissions].reverse()" :key="s.id"
              class="py-3 transition-colors"
              :style="s.isLatest ? { margin: '0 -0.75rem', padding: '0.75rem', borderRadius: '0.5rem', background: 'var(--accent-primary-soft)' } : {}">
              <div class="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <div class="flex items-center gap-1.5 mb-1 flex-wrap">
                    <span class="text-sm font-semibold text-heading">Lần {{ s.attempt }}</span>
                    <GlassBadge v-if="s.isLatest" variant="primary" size="sm">Mới nhất</GlassBadge>
                    <GlassBadge :variant="statusBadgeVariant(s.status)" size="sm">{{ s.statusLabel }}</GlassBadge>
                    <span class="text-[11px] font-semibold" :style="{ color: s.onTime ? 'var(--color-success-text)' : 'var(--color-warning-text)' }">{{ s.timeLabel }}</span>
                  </div>
                  <p class="text-xs text-muted">{{ s.submittedAt }}</p>
                  <div class="flex items-center gap-1.5 mt-1.5">
                    <component :is="icon('FileArchive')" :size="12" class="text-muted" />
                    <span class="text-xs text-body font-medium">{{ s.file }}</span>
                    <span class="text-xs text-muted">· {{ s.fileSize }}</span>
                  </div>
                  <p v-if="s.note" class="text-xs text-muted mt-0.5">{{ s.note }}</p>
                </div>
                <GlassButton variant="ghost" size="sm">
                  <template #leading><component :is="icon('Download')" :size="13" /></template>
                  Tải xuống
                </GlassButton>
              </div>
            </div>
            <div v-if="!submissions.length" class="py-6 text-center text-sm text-muted">Chưa có lần nộp nào.</div>
          </div>
        </GlassPanel>
      </div>

      <!-- RIGHT SIDEBAR -->
      <div class="space-y-4">

        <!-- Deadline Reminder -->
        <GlassPanel variant="flat" :style="{
          borderColor: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--color-success-text)',
          background: deadlinePassed ? 'var(--color-danger-bg)' : deadlineUrgent ? 'var(--color-warning-bg)' : 'var(--color-success-bg)'
        }">
          <div class="flex items-center gap-2 mb-2">
            <component :is="icon('AlarmClock')" :size="15" :style="{ color: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--color-success-text)' }" />
            <span class="text-sm font-semibold" :style="{ color: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--color-success-text)' }">Nhắc nhở deadline</span>
          </div>
          <p class="text-base font-semibold" :style="{ color: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--color-success-text)' }">{{ deadlineText }}</p>
          <p class="text-xs mt-1 text-muted">Hạn nộp: {{ mockAssignment.deadlineDisplay }}</p>
        </GlassPanel>

        <!-- Submission Rules -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2">
              <component :is="icon('ShieldCheck')" :size="15" style="color:var(--accent-primary)" />
              <h3 class="text-sm font-semibold text-heading">Điều kiện nộp bài</h3>
            </div>
          </template>
          <div class="space-y-3">
            <div>
              <p class="text-xs font-semibold text-muted mb-1.5">Định dạng cho phép</p>
              <div class="flex flex-wrap gap-1.5">
                <span v-for="fmt in rules.allowedFormats" :key="fmt" class="inline-block rounded px-2 py-0.5 text-xs font-bold" style="background:var(--accent-primary-soft);color:var(--accent-primary)">{{ fmt }}</span>
              </div>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">Dung lượng tối đa</span>
              <span class="font-semibold text-heading">{{ rules.maxSizeMB }} MB</span>
            </div>
            <div class="flex items-center justify-between text-sm">
              <span class="text-muted">Số lần nộp</span>
              <span class="font-semibold text-heading">{{ rules.currentAttempt }}/{{ rules.maxAttempts }}</span>
            </div>
            <p class="text-xs text-muted leading-5 pt-3 border-t border-card">{{ rules.note }}</p>
          </div>
        </GlassPanel>

        <!-- Plagiarism -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2">
              <component :is="icon('ScanSearch')" :size="15" style="color:var(--color-warning-text)" />
              <h3 class="text-sm font-semibold text-heading">Kiểm tra đạo văn</h3>
            </div>
          </template>
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <GlassBadge :variant="ps.variant" size="sm">{{ ps.label }}</GlassBadge>
              <span class="text-lg font-semibold text-heading">{{ mockPlagiarismResult.percentage }}%</span>
            </div>
            <div class="h-1.5 overflow-hidden rounded-full bg-surface-input">
              <div class="h-full rounded-full" :style="{ background: ps.bar, width: mockPlagiarismResult.percentage + '%' }" />
            </div>
            <p class="text-xs text-muted">{{ mockPlagiarismResult.detail }}</p>
            <p class="text-xs text-muted">Kiểm tra lúc: {{ mockPlagiarismResult.checkedAt }}</p>
          </div>
        </GlassPanel>

        <!-- Feedback -->
        <GlassPanel variant="flat">
          <template #header>
            <div class="flex items-center gap-2">
              <component :is="icon('MessageSquare')" :size="15" style="color:var(--color-success-text)" />
              <h3 class="text-sm font-semibold text-heading">Kết quả & Phản hồi</h3>
            </div>
          </template>
          <div v-if="mockFeedback.graded" class="space-y-3">
            <div class="flex items-center gap-3 rounded-xl p-3 border border-card" style="background:var(--color-success-bg)">
              <div class="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-white font-bold text-base" style="background:var(--lg-success)">{{ mockFeedback.score }}</div>
              <div>
                <p class="text-sm font-semibold text-heading">Điểm: {{ mockFeedback.score }}/{{ mockFeedback.maxScore }}</p>
                <p class="text-xs text-muted">Chấm ngày {{ mockFeedback.gradedAt }} bởi {{ mockFeedback.gradedBy }}</p>
              </div>
            </div>
            <div class="space-y-1.5">
              <p class="text-xs font-semibold text-muted">Tiêu chí chấm điểm</p>
              <div v-for="r in mockFeedback.rubric" :key="r.label" class="flex items-center justify-between text-xs">
                <span class="text-body">{{ r.label }}</span>
                <span class="font-semibold" :style="{ color: r.score === r.max ? 'var(--color-success-text)' : 'var(--color-warning-text)' }">{{ r.score }}/{{ r.max }}</span>
              </div>
            </div>
            <div class="rounded-lg border border-card bg-surface-input p-3">
              <p class="text-xs font-semibold text-muted mb-1">Nhận xét giảng viên</p>
              <p class="text-sm text-body leading-6">{{ mockFeedback.comment }}</p>
            </div>
            <div class="rounded-lg border border-card p-3" style="background:var(--color-info-bg)">
              <div class="flex items-center gap-1.5 mb-1">
                <component :is="icon('Sparkles')" :size="12" style="color:var(--color-info-text)" />
                <p class="text-xs font-semibold" style="color:var(--color-info-text)">AI Gợi ý</p>
              </div>
              <p class="text-xs leading-5" style="color:var(--color-info-text)">{{ mockFeedback.aiSuggestion }}</p>
            </div>
          </div>
          <div v-else class="text-center py-4">
            <component :is="icon('Hourglass')" :size="24" class="mx-auto text-muted mb-2" />
            <p class="text-sm font-semibold text-body">Đang chờ giảng viên chấm</p>
            <p class="text-xs text-muted mt-1">Kết quả sẽ được thông báo qua email.</p>
          </div>
        </GlassPanel>

      </div>
    </div>
  </div>
</template>

<style scoped>
.remove-file-button:hover {
  color: var(--color-danger-text);
}
</style>
