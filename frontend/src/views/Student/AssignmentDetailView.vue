<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import * as L from 'lucide-vue-next'
import FormSkeleton from '@/components/common/skeleton/FormSkeleton.vue'
import { studentApi } from '@/services/studentApi.js'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const route = useRoute()
const assignmentId = route.params.assignmentId

const icon = (n) => L[n] || L.Circle

// ── State ──────────────────────────────────────
const loading = ref(true)
const submitting = ref(false)
const isDragging = ref(false)
const selectedFiles = ref([])
const showToast = ref(false)
const toastMessage = ref('')
const loadError = ref('')

const assignment = ref({
  courseCode: '',
  class: '',
  title: '',
  teacher: '',
  deadlineDisplay: '',
  status: '',
  statusLabel: '',
  description: '',
  rules: {
    allowedFormats: [],
    maxSizeMB: 0,
    maxAttempts: 0,
    currentAttempt: 0,
    note: ''
  },
  submissions: []
})

// ── Deadline countdown ───────────────────────────────────────────────────
const deadlinePassed = computed(() => assignment.value.status === 'overdue')
const deadlineUrgent = computed(() => assignment.value.status === 'late' || assignment.value.status === 'pending')
const deadlineText = computed(() => deadlinePassed.value ? 'Đã quá hạn' : 'Sắp đến hạn')
const scoreText = computed(() => {
  if (assignment.value.score === null || assignment.value.score === undefined) return 'Chờ chấm'
  return `${Number(assignment.value.score).toFixed(1)}/10`
})

// ── Methods ──────────────────────────────────────

const cleanAllowedFormats = computed(() => {
  if (!assignment.value.rules?.allowedFormats) return []
  const rawString = assignment.value.rules.allowedFormats.join(',')
  // Loại bỏ các dấu ngoặc vuông, ngoặc kép, khoảng trắng và tách bằng dấu phẩy
  return rawString.replace(/[[\]"\s]/g, '').split(',').filter(f => f.startsWith('.'))
})

async function fetchDetail() {
  loading.value = true
  loadError.value = ''
  try {
    const res = await studentApi.getAssignmentDetail(assignmentId)
    if (res.success && res.data) {
      assignment.value = res.data
    }
  } catch (err) {
    console.error('Error fetching assignment detail', err)
    loadError.value = err?.message || 'Không thể tải chi tiết bài tập.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchDetail()
})

const attemptsLeft = computed(() => {
  if (!assignment.value.rules.maxAttempts) return 0
  return assignment.value.rules.maxAttempts - assignment.value.rules.currentAttempt
})

function validateFile(file) {
  const ext = '.' + file.name.split('.').pop().toLowerCase()
  if (!cleanAllowedFormats.value.includes(ext)) return 'invalid'
  if (file.size > assignment.value.rules.maxSizeMB * 1024 * 1024) return 'toolarge'
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

function removeFile(name) { 
  selectedFiles.value = selectedFiles.value.filter(f => f.name !== name) 
}

const canSubmit = computed(() =>
  !submitting.value &&
  !deadlinePassed.value &&
  attemptsLeft.value > 0 &&
  selectedFiles.value.length > 0 &&
  selectedFiles.value.every(f => f.status === 'valid')
)

async function doSubmit() {
  if (!canSubmit.value) return
  submitting.value = true
  
  const formData = new FormData()
  // Currently, the backend accepts one file for simplicity in demo
  if (selectedFiles.value.length > 0) {
    formData.append('file', selectedFiles.value[0].file)
  }

  try {
    const res = await studentApi.submitAssignment(assignmentId, formData)
    if (res.success) {
      toastMessage.value = 'Nộp bài thành công!'
      showToast.value = true
      selectedFiles.value = []
      setTimeout(() => showToast.value = false, 3500)
      await fetchDetail() // Reload details
    }
  } catch (err) {
    console.error('Submission failed', err)
    toastMessage.value = err.message || 'Có lỗi xảy ra khi nộp bài.'
    showToast.value = true
    setTimeout(() => showToast.value = false, 3500)
  } finally {
    submitting.value = false
  }
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

</script>

<template>
  <div class="lg-page-enter space-y-4 pb-6">

    <div v-if="loading" class="p-4">
      <FormSkeleton :fields="8" />
    </div>
    <div v-else-if="loadError" class="rounded-lg border border-card px-4 py-6 text-center text-sm font-semibold" style="background:var(--color-danger-bg);color:var(--color-danger-text)">
      {{ loadError }}
    </div>

    <template v-else>
      <!-- Toast -->
      <Transition enter-active-class="transition-all duration-300" enter-from-class="opacity-0 translate-y-2" leave-active-class="transition-all duration-200" leave-to-class="opacity-0">
        <div v-if="showToast" class="fixed bottom-6 right-6 z-50 flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-semibold text-white shadow-lg" style="background:var(--lg-success)">
          <component :is="icon('CheckCircle2')" :size="18" /> {{ toastMessage }}
        </div>
      </Transition>

      <!-- Header -->
      <GlassPanel variant="flat" density="compact">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 text-label text-xs font-semibold mb-1.5">
              <component :is="icon('ClipboardList')" :size="13" />
              {{ assignment.courseCode }} · {{ assignment.class }}
            </div>
            <h1 class="text-heading text-lg font-semibold leading-tight">{{ assignment.title }}</h1>
            <p class="mt-0.5 text-muted text-sm">{{ assignment.teacher }} · Hạn nộp: {{ assignment.deadlineDisplay }}</p>
          </div>
          <div class="flex flex-wrap items-center gap-2 shrink-0">
            <GlassBadge :variant="statusBadgeVariant(assignment.status)" size="md">
              {{ assignment.statusLabel }}
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
          <GlassBadge :variant="statusBadgeVariant(assignment.status)" size="sm">
            {{ assignment.statusLabel }}
          </GlassBadge>
        </GlassPanel>
        <GlassPanel variant="flat" density="compact">
          <div class="flex items-center gap-2 mb-2">
            <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-warning-bg);color:var(--color-warning-text)">
              <component :is="icon('Clock')" :size="13" />
            </div>
            <span class="text-label text-xs font-semibold">Deadline</span>
          </div>
          <p class="text-sm font-semibold text-heading">{{ assignment.deadlineDisplay }}</p>
          <p class="text-xs mt-0.5 font-medium" :style="{ color: deadlinePassed ? 'var(--color-danger-text)' : deadlineUrgent ? 'var(--color-warning-text)' : 'var(--text-muted)' }">{{ deadlineText }}</p>
        </GlassPanel>
        <GlassPanel variant="flat" density="compact">
          <div class="flex items-center gap-2 mb-2">
            <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-info-bg);color:var(--color-info-text)">
              <component :is="icon('RefreshCw')" :size="13" />
            </div>
            <span class="text-label text-xs font-semibold">Lượt nộp</span>
          </div>
          <p class="text-lg font-semibold text-heading">{{ assignment.rules.currentAttempt }}<span class="text-sm text-muted">/{{ assignment.rules.maxAttempts }}</span></p>
          <p class="text-xs text-muted mt-0.5">Còn {{ attemptsLeft }} lượt</p>
        </GlassPanel>
        <GlassPanel variant="flat" density="compact">
          <div class="flex items-center gap-2 mb-2">
            <div class="flex h-7 w-7 items-center justify-center rounded-lg" style="background:var(--color-success-bg);color:var(--color-success-text)">
              <component :is="icon('Star')" :size="13" />
            </div>
            <span class="text-label text-xs font-semibold">Điểm số</span>
          </div>
          <p class="text-sm font-semibold text-heading">{{ scoreText }}</p>
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
            <p class="text-sm leading-7 text-body whitespace-pre-line">{{ assignment.description }}</p>
          </GlassPanel>

          <GlassPanel v-if="assignment.feedback" variant="flat">
            <template #header>
              <div class="flex items-center gap-2">
                <component :is="icon('MessageSquare')" :size="15" style="color:var(--accent-primary)" />
                <h2 class="text-sm font-semibold text-heading">Nhận xét giảng viên</h2>
              </div>
            </template>
            <p class="text-sm leading-7 text-body whitespace-pre-line">{{ assignment.feedback }}</p>
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
                <input type="file" class="hidden" @change="onDrop" :accept="cleanAllowedFormats.join(',')" />
                <component :is="icon('UploadCloud')" :size="26" :class="isDragging ? 'text-link' : 'text-placeholder'" />
                <div class="text-center">
                  <p class="text-xs font-semibold text-body">Kéo thả file vào đây hoặc bấm để chọn file</p>
                  <p class="text-xs text-muted mt-1">Định dạng: {{ cleanAllowedFormats.join(', ') }} · Tối đa {{ assignment.rules.maxSizeMB }} MB</p>
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
                {{ submitting ? 'Đang nộp bài...' : 'Nộp bài' }}
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
              <div v-for="s in assignment.submissions" :key="s.id"
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
                  <GlassButton v-if="s.fileUrl" variant="ghost" size="sm" @click="() => window.open(s.fileUrl, '_blank')">
                    <template #leading><component :is="icon('Download')" :size="13" /></template>
                    Tải xuống
                  </GlassButton>
                </div>
              </div>
              <div v-if="!assignment.submissions.length" class="py-6 text-center text-sm text-muted">Chưa có lần nộp nào.</div>
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
            <p class="text-xs mt-1 text-muted">Hạn nộp: {{ assignment.deadlineDisplay }}</p>
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
                  <span v-for="fmt in cleanAllowedFormats" :key="fmt" class="inline-block rounded px-2 py-0.5 text-xs font-bold" style="background:var(--accent-primary-soft);color:var(--accent-primary)">{{ fmt }}</span>
                </div>
              </div>
              <div class="flex items-center justify-between text-sm">
                <span class="text-muted">Dung lượng tối đa</span>
                <span class="font-semibold text-heading">{{ assignment.rules.maxSizeMB }} MB</span>
              </div>
              <div class="flex items-center justify-between text-sm">
                <span class="text-muted">Số lần nộp</span>
                <span class="font-semibold text-heading">{{ assignment.rules.currentAttempt }}/{{ assignment.rules.maxAttempts }}</span>
              </div>
              <p class="text-xs text-muted leading-5 pt-3 border-t border-card">{{ assignment.rules.note }}</p>
            </div>
          </GlassPanel>

        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
.remove-file-button:hover {
  color: var(--color-danger-text);
}
</style>
