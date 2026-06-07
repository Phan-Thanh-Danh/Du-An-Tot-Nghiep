<script setup>
import { ref, computed } from 'vue'
import {
  AlertCircle,
  Award,
  BookOpen,
  Calendar,
  Check,
  CheckCircle2,
  ChevronRight,
  Clock,
  FileCode,
  FileText,
  Plus,
  Search,
  Send,
  SlidersHorizontal,
  Trash2,
  Upload,
  Users,
  X,
} from 'lucide-vue-next'

import EmptyState from '@/components/ui/EmptyState.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const assignments = ref([
  { id: 1, name: 'Assignment 1: HTML/CSS Basic', className: 'SE1601', deadline: '20/05/2026 23:59', status: 'Active', submissionsCount: 42, totalStudents: 45, maxScore: 10, type: 'Assignment' },
  { id: 2, name: 'Assignment 2: JavaScript DOM', className: 'SE1601', deadline: '28/05/2026 23:59', status: 'Active', submissionsCount: 12, totalStudents: 45, maxScore: 10, type: 'Assignment' },
  { id: 3, name: 'Lab 1: UI Design with Figma', className: 'SS1402', deadline: '15/05/2026 23:59', status: 'Completed', submissionsCount: 38, totalStudents: 38, maxScore: 10, type: 'Lab' },
])

const classesList = ref([
  { code: 'SE1601', name: 'Lớp SE1601 - Java', subject: 'Lập trình Java', students: 45 },
  { code: 'SS1402', name: 'Lớp SS1402 - Web', subject: 'Lập trình Web', students: 38 },
  { code: 'SA1709', name: 'Lớp SA1709 - DB', subject: 'Cơ sở dữ liệu', students: 42 }
])

// Filter & Search states
const searchQuery = ref('')
const selectedClassFilter = ref('')

const filteredAssignments = computed(() => {
  return assignments.value.filter(asm => {
    const matchesSearch = asm.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesClass = !selectedClassFilter.value || asm.className === selectedClassFilter.value
    return matchesSearch && matchesClass
  })
})

// Stats computation
const stats = computed(() => {
  const activeCount = assignments.value.filter(a => a.status === 'Active').length
  const draftCount = assignments.value.filter(a => a.status === 'Draft').length
  const totalSubmissions = assignments.value.reduce((acc, curr) => acc + curr.submissionsCount, 0)

  return {
    active: activeCount + draftCount,
    submissions: totalSubmissions,
    pendingGrades: 24
  }
})

const summaryStats = computed(() => [
  { label: 'Tổng bài tập', value: assignments.value.length, tone: 'primary' },
  { label: 'Đang mở', value: stats.value.active, tone: 'success' },
  { label: 'Chờ chấm', value: stats.value.pendingGrades, tone: 'warning' },
  {
    label: 'Đã đóng',
    value: assignments.value.filter(a => a.status === 'Completed').length,
    tone: 'neutral',
  },
])

// Modal & creation states
const showCreateModal = ref(false)
const dragActive = ref(false)
const isSubmitting = ref(false)

const initialForm = {
  name: '',
  classId: '',
  type: 'Assignment',
  deadlineDate: '',
  deadlineTime: '23:59',
  maxScore: 10,
  description: '',
  status: 'Active',
  files: []
}
const form = ref({ ...initialForm })
const errors = ref({})

// File selection
const fileInputRef = ref(null)

// Toast notification state
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

function openCreateModal() {
  form.value = {
    name: '',
    classId: '',
    type: 'Assignment',
    deadlineDate: '',
    deadlineTime: '23:59',
    maxScore: 10,
    description: '',
    status: 'Active',
    files: []
  }
  errors.value = {}
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
}

// Drag & drop handlers
function handleDragEnter(e) {
  e.preventDefault()
  dragActive.value = true
}
function handleDragLeave(e) {
  e.preventDefault()
  dragActive.value = false
}
function handleDragOver(e) {
  e.preventDefault()
}
function handleDrop(e) {
  e.preventDefault()
  dragActive.value = false
  if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
    addFiles(e.dataTransfer.files)
  }
}

function triggerFileInput() {
  fileInputRef.value?.click()
}

function handleFileSelect(e) {
  if (e.target.files && e.target.files.length > 0) {
    addFiles(e.target.files)
  }
}

function addFiles(fileList) {
  for (let i = 0; i < fileList.length; i++) {
    const file = fileList[i]
    if (form.value.files.some(f => f.name === file.name)) continue

    const newFile = ref({
      name: file.name,
      size: (file.size / (1024 * 1024)).toFixed(2) + ' MB',
      progress: 0,
      status: 'uploading'
    })
    form.value.files.push(newFile.value)

    let prog = 0
    const interval = setInterval(() => {
      prog += Math.floor(Math.random() * 25) + 15
      if (prog >= 100) {
        newFile.value.progress = 100
        newFile.value.status = 'done'
        clearInterval(interval)
      } else {
        newFile.value.progress = prog
      }
    }, 200)
  }
}

function removeFile(index) {
  form.value.files.splice(index, 1)
}

function validateForm() {
  errors.value = {}
  let isValid = true

  if (!form.value.name.trim()) {
    errors.value.name = 'Vui lòng nhập tên bài tập'
    isValid = false
  }

  if (!form.value.classId) {
    errors.value.classId = 'Vui lòng chọn lớp học học phần'
    isValid = false
  }

  if (!form.value.deadlineDate) {
    errors.value.deadlineDate = 'Vui lòng chọn ngày hạn nộp'
    isValid = false
  }

  if (!form.value.maxScore || form.value.maxScore <= 0 || form.value.maxScore > 100) {
    errors.value.maxScore = 'Thang điểm phải từ 1 đến 100'
    isValid = false
  }

  return isValid
}

function submitAssignment() {
  if (!validateForm()) {
    triggerToast('Vui lòng hoàn thiện tất cả thông tin hợp lệ!', 'error')
    return
  }

  isSubmitting.value = true

  setTimeout(() => {
    const selectedClass = classesList.value.find(c => c.code === form.value.classId)
    const dateParts = form.value.deadlineDate.split('-')
    const formattedDate = `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`

    const newAsm = {
      id: assignments.value.length + 1,
      name: form.value.name,
      className: form.value.classId,
      deadline: `${formattedDate} ${form.value.deadlineTime}`,
      submissionsCount: 0,
      totalStudents: selectedClass ? selectedClass.students : 45,
      status: form.value.status,
      maxScore: form.value.maxScore,
      type: form.value.type
    }

    assignments.value.unshift(newAsm)
    isSubmitting.value = false
    showCreateModal.value = false

    triggerToast(`Đã tạo thành công bài tập "${form.value.name}"!`, 'success')
  }, 900)
}

function assignmentStatusVariant(status) {
  if (status === 'Active') return 'success'
  if (status === 'Draft') return 'neutral'
  return 'info'
}

function assignmentStatusLabel(status) {
  if (status === 'Active') return 'Đang mở'
  if (status === 'Draft') return 'Nháp'
  return 'Hoàn thành'
}

function submissionPercent(assignment) {
  return Math.round((assignment.submissionsCount / assignment.totalStudents) * 100)
}
</script>

<template>
  <div class="assignments-page lg-page-enter">
    <Transition name="toast-slide">
      <div v-if="toast.show" :class="['toast', toast.type === 'success' ? 'success' : 'danger']">
        <CheckCircle2 v-if="toast.type === 'success'" :size="18" />
        <AlertCircle v-else :size="18" />
        <p>{{ toast.message }}</p>
      </div>
    </Transition>

    <GlassPanel variant="flat" density="compact" class="page-header">
      <div class="header-copy">
        <div class="eyebrow">
          <ClipboardList :size="15" />
          Quản lý bài tập
        </div>
        <div>
          <h1>Bài tập</h1>
          <p>Phát hành bài tập, theo dõi bài nộp và trạng thái chấm điểm theo lớp học phần.</p>
        </div>
      </div>

      <div class="header-actions">
        <div class="summary-strip">
          <div v-for="item in summaryStats" :key="item.label" :class="['summary-pill', item.tone]">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </div>
        <GlassButton variant="primary" size="md" @click="openCreateModal">
          <template #leading>
            <Plus :size="17" />
          </template>
          Tạo bài tập
        </GlassButton>
      </div>
    </GlassPanel>

    <GlassPanel variant="flat" density="compact" class="list-panel">
      <div class="list-toolbar">
        <div>
          <h2>Danh sách bài tập</h2>
          <p>{{ filteredAssignments.length }} bài tập phù hợp bộ lọc hiện tại.</p>
        </div>

        <div class="filters">
          <label class="search-field">
            <Search :size="16" />
            <input v-model="searchQuery" type="text" placeholder="Tìm tên bài tập..." />
          </label>

          <label class="select-field">
            <select v-model="selectedClassFilter">
              <option value="">Tất cả lớp</option>
              <option v-for="cls in classesList" :key="cls.code" :value="cls.code">
                {{ cls.code }} - {{ cls.subject }}
              </option>
            </select>
            <SlidersHorizontal :size="15" />
          </label>
        </div>
      </div>

      <TableShell v-if="filteredAssignments.length" density="compact">
        <table>
          <thead>
            <tr>
              <th>Bài tập</th>
              <th>Lớp</th>
              <th>Deadline</th>
              <th>Bài nộp</th>
              <th>Chờ chấm</th>
              <th>Trạng thái</th>
              <th class="text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="asm in filteredAssignments" :key="asm.id">
              <td>
                <div class="assignment-cell">
                  <span class="assignment-icon">
                    <FileText :size="18" />
                  </span>
                  <span>
                    <strong>{{ asm.name }}</strong>
                    <small>{{ asm.type || 'Assignment' }} · {{ asm.maxScore }} điểm</small>
                  </span>
                </div>
              </td>
              <td>
                <GlassBadge variant="primary">{{ asm.className }}</GlassBadge>
              </td>
              <td>
                <span class="deadline-cell">
                  <Clock :size="14" />
                  {{ asm.deadline }}
                </span>
              </td>
              <td>
                <div class="submission-cell">
                  <span>{{ asm.submissionsCount }}/{{ asm.totalStudents }}</span>
                  <div class="progress-track" aria-hidden="true">
                    <span :style="{ width: `${submissionPercent(asm)}%` }" />
                  </div>
                </div>
              </td>
              <td>
                <span class="pending-count">
                  {{ Math.max(asm.submissionsCount - Math.floor(asm.submissionsCount * 0.45), 0) }}
                </span>
              </td>
              <td>
                <GlassBadge :variant="assignmentStatusVariant(asm.status)">
                  <Check v-if="asm.status === 'Completed'" :size="11" />
                  {{ assignmentStatusLabel(asm.status) }}
                </GlassBadge>
              </td>
              <td>
                <div class="row-actions">
                  <GlassButton variant="ghost" size="sm">Xem bài nộp</GlassButton>
                  <router-link to="/teacher/grading" class="grade-link">
                    Chấm bài
                    <ChevronRight :size="14" />
                  </router-link>
                  <GlassButton variant="ghost" size="sm">Sửa</GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>

      <EmptyState
        v-else
        title="Không tìm thấy bài tập"
        description="Vui lòng thử lại với từ khóa hoặc bộ lọc lớp khác."
      >
        <GlassButton variant="secondary" size="sm" @click="openCreateModal">
          <template #leading>
            <Plus :size="15" />
          </template>
          Tạo bài tập mới
        </GlassButton>
      </EmptyState>
    </GlassPanel>

    <div v-if="showCreateModal" class="modal-root">
      <button type="button" class="modal-backdrop" aria-label="Đóng modal" @click="closeCreateModal" />

      <GlassPanel variant="flat" density="none" class="assignment-modal">
        <div class="modal-header">
          <div class="modal-title">
            <span class="modal-icon">
              <Plus :size="22" />
            </span>
            <span>
              <h3>Tạo bài tập mới</h3>
              <p>Thiết lập nội dung, deadline, thang điểm và tài liệu đính kèm.</p>
            </span>
          </div>
          <button type="button" class="icon-button" @click="closeCreateModal">
            <X :size="18" />
          </button>
        </div>

        <div class="modal-body">
          <section class="form-section">
            <h4>1. Thông tin chung</h4>

            <label class="field">
              <span>Tiêu đề bài tập <b>*</b></span>
              <input
                v-model="form.name"
                type="text"
                placeholder="Ví dụ: Assignment 3: Xây dựng giao diện Single Page..."
                :class="{ invalid: errors.name }"
              />
              <small v-if="errors.name" class="error-text">
                <AlertCircle :size="12" />
                {{ errors.name }}
              </small>
            </label>

            <div class="form-grid">
              <label class="field">
                <span>Lớp học phần <b>*</b></span>
                <select v-model="form.classId" :class="{ invalid: errors.classId }">
                  <option value="" disabled selected>Chọn lớp...</option>
                  <option v-for="cls in classesList" :key="cls.code" :value="cls.code">
                    {{ cls.code }} - {{ cls.subject }}
                  </option>
                </select>
                <small v-if="errors.classId" class="error-text">
                  <AlertCircle :size="12" />
                  {{ errors.classId }}
                </small>
              </label>

              <label class="field">
                <span>Thang điểm tối đa <b>*</b></span>
                <span class="field-with-icon">
                  <input
                    v-model.number="form.maxScore"
                    type="number"
                    min="1"
                    max="100"
                    placeholder="10"
                    :class="{ invalid: errors.maxScore }"
                  />
                  <Award :size="14" />
                </span>
                <small v-if="errors.maxScore" class="error-text">
                  <AlertCircle :size="12" />
                  {{ errors.maxScore }}
                </small>
              </label>
            </div>

            <div class="field">
              <span>Loại bài tập</span>
              <div class="type-grid">
                <button
                  v-for="t in ['Assignment', 'Lab', 'Homework']"
                  :key="t"
                  type="button"
                  :class="['type-option', form.type === t ? 'active' : '']"
                  @click="form.type = t"
                >
                  <FileText v-if="t === 'Assignment'" :size="17" />
                  <BookOpen v-else-if="t === 'Lab'" :size="17" />
                  <Send v-else :size="17" />
                  {{ t }}
                </button>
              </div>
            </div>

            <div class="form-grid">
              <label class="field">
                <span>Ngày hạn nộp <b>*</b></span>
                <span class="field-with-icon">
                  <input
                    v-model="form.deadlineDate"
                    type="date"
                    :class="{ invalid: errors.deadlineDate }"
                  />
                  <Calendar :size="14" />
                </span>
              </label>

              <label class="field">
                <span>Giờ hạn nộp</span>
                <span class="field-with-icon">
                  <input v-model="form.deadlineTime" type="time" />
                  <Clock :size="14" />
                </span>
              </label>
            </div>

            <small v-if="errors.deadlineDate" class="error-text inline-error">
              <AlertCircle :size="12" />
              {{ errors.deadlineDate }}
            </small>

            <div class="status-toggle">
              <div>
                <p>Kích hoạt bài tập ngay</p>
                <span>Sinh viên có thể nhìn thấy và nộp bài ngay sau khi tạo.</span>
              </div>
              <button
                type="button"
                :class="['switch', form.status === 'Active' ? 'active' : '']"
                @click="form.status = form.status === 'Active' ? 'Draft' : 'Active'"
              >
                <span />
              </button>
            </div>
          </section>

          <section class="form-section">
            <h4>2. Hướng dẫn & tài liệu</h4>

            <label class="field">
              <span>Mô tả & hướng dẫn chi tiết</span>
              <textarea
                v-model="form.description"
                rows="6"
                placeholder="Nhập yêu cầu đề bài, tiêu chí đánh giá, các bước thực hiện chi tiết cho sinh viên..."
              />
            </label>

            <div class="field">
              <span>Tài liệu đính kèm</span>
              <div
                :class="['upload-zone', dragActive ? 'active' : '']"
                @dragenter="handleDragEnter"
                @dragleave="handleDragLeave"
                @dragover="handleDragOver"
                @drop="handleDrop"
                @click="triggerFileInput"
              >
                <input
                  ref="fileInputRef"
                  type="file"
                  multiple
                  class="hidden"
                  @change="handleFileSelect"
                />
                <span class="upload-icon">
                  <Upload :size="20" />
                </span>
                <p>Nhấp để tải lên hoặc kéo thả file vào đây</p>
                <small>PDF, ZIP, RAR, DOCX · tối đa 15MB</small>
              </div>
            </div>

            <div v-if="form.files.length > 0" class="file-list">
              <div v-for="(file, index) in form.files" :key="index" class="file-row">
                <span class="file-icon">
                  <FileCode :size="17" />
                </span>
                <div class="file-copy">
                  <strong>{{ file.name }}</strong>
                  <div class="file-progress">
                    <span>{{ file.size }}</span>
                    <div v-if="file.status === 'uploading'" class="progress-track" aria-hidden="true">
                      <span :style="{ width: file.progress + '%' }" />
                    </div>
                    <em v-if="file.status === 'uploading'">{{ file.progress }}%</em>
                    <em v-else>Đã tải lên</em>
                  </div>
                </div>
                <button type="button" class="remove-file" @click.stop="removeFile(index)">
                  <Trash2 :size="14" />
                </button>
              </div>
            </div>
          </section>
        </div>

        <div class="modal-footer">
          <GlassButton variant="secondary" size="md" @click="closeCreateModal">Hủy</GlassButton>
          <GlassButton variant="primary" size="md" :disabled="isSubmitting" @click="submitAssignment">
            <span v-if="isSubmitting" class="spinner" />
            <span v-else>Tạo bài tập</span>
          </GlassButton>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.assignments-page {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.toast {
  position: fixed;
  top: 1rem;
  right: 1.5rem;
  z-index: 100;
  display: flex;
  align-items: center;
  gap: 0.65rem;
  max-width: min(24rem, calc(100vw - 2rem));
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  padding: 0.8rem 1rem;
  color: var(--text-heading);
  background: var(--surface-modal);
  box-shadow: var(--lg-shadow-md);
}

.toast.success {
  color: var(--color-success-text);
  background: var(--color-success-bg);
}

.toast.danger {
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
}

.toast p {
  margin: 0;
  font-size: 0.84rem;
  font-weight: 800;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-copy {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  min-width: 0;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.header-copy h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  line-height: 1.15;
  font-weight: 900;
}

.header-copy p,
.list-toolbar p,
.summary-pill span,
.deadline-cell,
.assignment-cell small,
.status-toggle span,
.upload-zone small,
.file-progress,
.form-section h4 {
  color: var(--text-muted);
}

.header-copy p,
.list-toolbar p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-actions {
  display: flex;
  align-items: flex-start;
  justify-content: flex-end;
  gap: 0.75rem;
}

.summary-strip {
  display: flex;
  gap: 0.45rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.summary-pill {
  display: grid;
  gap: 0.05rem;
  min-width: 5.8rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.warning {
  background: var(--color-warning-bg);
}

.summary-pill.primary {
  background: var(--accent-primary-soft);
}

.list-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.list-toolbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.list-toolbar h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.filters {
  display: flex;
  gap: 0.55rem;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.search-field,
.select-field,
.field input,
.field select,
.field textarea {
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-heading);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.search-field,
.select-field {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  gap: 0.45rem;
  padding: 0 0.7rem;
}

.search-field {
  width: min(18rem, 100%);
}

.select-field {
  width: min(15rem, 100%);
}

.search-field svg,
.select-field svg,
.field-with-icon svg {
  color: var(--text-placeholder);
  flex: none;
}

.search-field input,
.select-field select {
  min-width: 0;
  width: 100%;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.8rem;
  font-weight: 750;
}

.select-field select {
  appearance: none;
  cursor: pointer;
}

.search-field:focus-within,
.select-field:focus-within,
.field:focus-within input,
.field:focus-within select,
.field:focus-within textarea {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.assignment-cell,
.deadline-cell,
.submission-cell,
.row-actions,
.modal-title,
.field-with-icon,
.file-row,
.file-progress {
  display: flex;
  align-items: center;
}

.assignment-cell {
  gap: 0.65rem;
  min-width: 16rem;
}

.assignment-icon,
.modal-icon,
.upload-icon,
.file-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex: none;
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
}

.assignment-icon {
  width: 2.1rem;
  height: 2.1rem;
  border-radius: var(--radius-md);
}

.assignment-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
  line-height: 1.35;
}

.assignment-cell small {
  display: block;
  margin-top: 0.15rem;
  font-size: 0.7rem;
  font-weight: 800;
  text-transform: uppercase;
}

.deadline-cell {
  gap: 0.4rem;
  min-width: 9.5rem;
  font-size: 0.8rem;
  font-weight: 750;
}

.submission-cell {
  min-width: 8rem;
  gap: 0.55rem;
}

.submission-cell > span,
.pending-count {
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 900;
}

.progress-track {
  width: 4.5rem;
  height: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-card);
  overflow: hidden;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
}

.row-actions {
  justify-content: flex-end;
  gap: 0.35rem;
  min-width: 15rem;
}

.grade-link {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
  min-height: var(--control-height-sm);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--accent-primary-soft);
  color: var(--text-link);
  padding: 0 0.65rem;
  font-size: 0.78rem;
  font-weight: 850;
  text-decoration: none;
  white-space: nowrap;
}

.grade-link:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
}

.modal-root {
  position: fixed;
  inset: 0;
  z-index: 80;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

.modal-backdrop {
  position: absolute;
  inset: 0;
  border: 0;
  background: var(--surface-modal);
  cursor: pointer;
}

.assignment-modal {
  position: relative;
  z-index: 1;
  width: min(62rem, 100%);
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  animation: modal-in 0.22s ease-out both;
}

.modal-header,
.modal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem;
  background: var(--surface-card);
}

.modal-header {
  border-bottom: 1px solid var(--border-card);
}

.modal-footer {
  border-top: 1px solid var(--border-card);
  justify-content: flex-end;
}

.modal-title {
  gap: 0.75rem;
  min-width: 0;
}

.modal-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
}

.modal-title h3 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
}

.modal-title p {
  margin: 0.2rem 0 0;
  color: var(--text-muted);
  font-size: 0.78rem;
}

.icon-button,
.remove-file {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  transition:
    color 0.2s ease,
    border-color 0.2s ease,
    background 0.2s ease;
}

.icon-button {
  width: 2.25rem;
  height: 2.25rem;
}

.icon-button:hover,
.remove-file:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  color: var(--text-link);
}

.modal-body {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  gap: 1rem;
  padding: 1rem;
  overflow-y: auto;
  background: var(--surface-input);
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  min-width: 0;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-card);
  padding: 0.9rem;
}

.form-section h4 {
  margin: 0;
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.55rem;
  font-size: 0.72rem;
  font-weight: 900;
  text-transform: uppercase;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.field > span:first-child {
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 850;
}

.field b {
  color: var(--color-danger-text);
}

.field input,
.field select,
.field textarea {
  width: 100%;
  min-height: 2.55rem;
  padding: 0 0.75rem;
  outline: none;
  font-size: 0.84rem;
  font-weight: 700;
}

.field textarea {
  min-height: 8rem;
  padding: 0.75rem;
  resize: none;
  line-height: 1.55;
}

.field input.invalid,
.field select.invalid {
  border-color: var(--color-danger-text);
  background: var(--color-danger-bg);
}

.field-with-icon {
  position: relative;
}

.field-with-icon svg {
  position: absolute;
  right: 0.75rem;
}

.field-with-icon input {
  padding-right: 2.2rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}

.type-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.5rem;
}

.type-option {
  display: inline-flex;
  min-height: 2.6rem;
  align-items: center;
  justify-content: center;
  gap: 0.4rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-label);
  font-size: 0.78rem;
  font-weight: 850;
}

.type-option.active,
.type-option:hover {
  border-color: var(--border-input-focus);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.error-text {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--color-danger-text);
  font-size: 0.72rem;
  font-weight: 800;
}

.inline-error {
  margin-top: -0.35rem;
}

.status-toggle {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.75rem;
}

.status-toggle p {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.84rem;
  font-weight: 850;
}

.status-toggle span {
  font-size: 0.74rem;
  font-weight: 650;
}

.switch {
  position: relative;
  width: 2.6rem;
  height: 1.45rem;
  flex: none;
  border: 1px solid var(--border-input);
  border-radius: 999px;
  background: var(--surface-card);
  padding: 0.12rem;
}

.switch span {
  display: block;
  width: 1.1rem;
  height: 1.1rem;
  border-radius: 999px;
  background: var(--text-placeholder);
  transition: transform 0.2s ease, background 0.2s ease;
}

.switch.active {
  background: var(--accent-primary-soft);
  border-color: var(--border-input-focus);
}

.switch.active span {
  transform: translateX(1.15rem);
  background: var(--text-link);
}

.upload-zone {
  display: flex;
  min-height: 9rem;
  cursor: pointer;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 0.35rem;
  border: 1px dashed var(--border-input);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 1rem;
  text-align: center;
  transition:
    border-color 0.2s ease,
    background 0.2s ease;
}

.upload-zone.active,
.upload-zone:hover {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
}

.upload-icon {
  width: 2.3rem;
  height: 2.3rem;
  border-radius: var(--radius-md);
}

.upload-zone p {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 850;
}

.upload-zone small {
  font-size: 0.72rem;
  font-weight: 650;
}

.file-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.file-row {
  gap: 0.65rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.65rem;
}

.file-icon {
  width: 2rem;
  height: 2rem;
  border-radius: var(--radius-md);
}

.file-copy {
  flex: 1;
  min-width: 0;
}

.file-copy strong {
  display: block;
  overflow: hidden;
  color: var(--text-heading);
  font-size: 0.78rem;
  font-weight: 850;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-progress {
  gap: 0.4rem;
  margin-top: 0.25rem;
  font-size: 0.68rem;
  font-weight: 750;
}

.file-progress .progress-track {
  flex: 1;
  min-width: 3rem;
}

.file-progress em {
  color: var(--text-link);
  font-style: normal;
}

.remove-file {
  width: 2rem;
  height: 2rem;
}

.spinner {
  display: inline-block;
  width: 1rem;
  height: 1rem;
  border: 2px solid var(--text-inverse);
  border-top-color: transparent;
  border-radius: 999px;
  animation: spin 0.8s linear infinite;
}

.hidden {
  display: none;
}

.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.25s ease;
}

.toast-slide-enter-from {
  transform: translateY(-0.75rem);
  opacity: 0;
}

.toast-slide-leave-to {
  transform: translateY(0.75rem);
  opacity: 0;
}

@keyframes modal-in {
  from {
    opacity: 0;
    transform: translateY(0.75rem) scale(0.98);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1024px) {
  .page-header,
  .header-actions,
  .list-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip,
  .filters {
    justify-content: flex-start;
  }

  .modal-body {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .summary-strip,
  .filters,
  .form-grid,
  .type-grid {
    grid-template-columns: 1fr;
  }

  .summary-strip,
  .filters {
    display: grid;
  }

  .summary-pill,
  .search-field,
  .select-field {
    width: 100%;
  }

  .row-actions {
    justify-content: flex-start;
  }

  .modal-root {
    align-items: stretch;
    padding: 0.5rem;
  }

  .assignment-modal {
    max-height: calc(100vh - 1rem);
  }
}
</style>
