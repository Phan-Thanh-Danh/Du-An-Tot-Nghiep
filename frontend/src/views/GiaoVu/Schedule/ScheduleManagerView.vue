<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import {
  Plus, Search, Filter, Download, ChevronLeft, ChevronRight,
  CheckCircle2, AlertCircle, X, Printer, Trash2, CalendarRange,
} from 'lucide-vue-next'
import * as XLSX from 'xlsx'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import MonthView from '@/components/Schedule/MonthView.vue'
import { useSchedule } from '@/composables/useSchedule'

const {
  monthLabel, nextMonth, prevMonth, goToToday,
  createEvent, updateEvent, deleteEvent,
  filterEventsByLecturer, filterEventsByQuery, exportToExcel,
  totalEvents, publishedEvents, pendingEvents, draftEvents,
} = useSchedule()

const popup = usePopupStore()

const semester = ref('Spring 2026')
const campus = ref('Cơ sở chính')
const showCreateModal = ref(false)
const showEditModal = ref(false)
const searchQuery = ref('')
const eventToEdit = ref(null)

useBodyScrollLock(showCreateModal)
useBodyScrollLock(showEditModal)

const newScheduleForm = ref({
  title: '', subject: '', teacher: '', room: '',
  day: 'Thứ 2', startTime: '07:30', endTime: '08:30', status: 'draft', type: 'class',
})

const editForm = ref({
  title: '', teacher: '', room: '', start: '', end: '', status: 'draft', type: 'class',
})

const lecturers = [
  { id: 1, name: 'Tất cả giảng viên' },
  { id: 2, name: 'TS. Nguyễn Văn A' },
  { id: 3, name: 'ThS. Trần Thị B' },
  { id: 4, name: 'TS. Lê Văn C' },
  { id: 5, name: 'ThS. Phạm Minh Tuấn' },
]
const selectedLecturer = ref(lecturers[0])

const filteredSchedules = computed(() => {
  const result = filterEventsByLecturer(selectedLecturer.value.name)
  return filterEventsByQuery(searchQuery.value, result)
})

const timeSlots = [
  '07:00', '07:30', '08:00', '08:30', '09:00', '09:30',
  '10:00', '10:30', '11:00', '11:30',
  '12:00', '12:30', '13:00', '13:30', '14:00', '14:30',
  '15:00', '15:30', '16:00', '16:30', '17:00', '17:30',
  '18:00', '18:30', '19:00', '19:30', '20:00', '20:30', '21:00',
]

// ── Create ──────────────────────────────────────────────
function resetForm() {
  newScheduleForm.value = {
    title: '', subject: '', teacher: '', room: '',
    day: 'Thứ 2', startTime: '07:30', endTime: '08:30', status: 'draft', type: 'class',
  }
}

function handleCreateSchedule() {
  if (!newScheduleForm.value.subject && !newScheduleForm.value.title) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập tên môn học.')
    return
  }
  const title = newScheduleForm.value.subject || newScheduleForm.value.title
  const now = new Date()
  const dateStr = now.toISOString().split('T')[0]

  createEvent({
    title,
    teacher: newScheduleForm.value.teacher,
    room: newScheduleForm.value.room,
    start: `${dateStr}T${newScheduleForm.value.startTime}:00`,
    end: `${dateStr}T${newScheduleForm.value.endTime}:00`,
    status: newScheduleForm.value.status,
    type: newScheduleForm.value.type,
  })

  resetForm()
  showCreateModal.value = false
  popup.success('Tạo lịch thành công', `Đã thêm "${title}" vào thời khóa biểu.`)
}

// ── Edit ────────────────────────────────────────────────
function openEditModal(event) {
  eventToEdit.value = event
  editForm.value = {
    id: event.id, title: event.title, teacher: event.teacher,
    room: event.room, start: event.start, end: event.end,
    status: event.status, type: event.type || 'class', color: event.color,
  }
  showEditModal.value = true
}

function handleEditSchedule() {
  if (!eventToEdit.value) return
  updateEvent(eventToEdit.value.id, {
    title: editForm.value.title,
    teacher: editForm.value.teacher,
    room: editForm.value.room,
    status: editForm.value.status,
    type: editForm.value.type,
  })
  showEditModal.value = false
  eventToEdit.value = null
  popup.success('Cập nhật thành công', 'Thông tin lịch đã được cập nhật.')
}

// ── Delete ──────────────────────────────────────────────
function handleDeleteEvent() {
  if (!eventToEdit.value) return
  deleteEvent(eventToEdit.value.id)
  showEditModal.value = false
  eventToEdit.value = null
  popup.success('Đã xóa lịch', 'Lịch đã được xóa khỏi thời khóa biểu.')
}

// ── Export ──────────────────────────────────────────────
function handleExportExcel() {
  const data = exportToExcel(filteredSchedules.value, semester.value)
  const worksheet = XLSX.utils.json_to_sheet(data)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Thời khóa biểu')
  const columnWidths = [30, 25, 12, 22, 22, 14]
  worksheet['!cols'] = columnWidths.map(w => ({ wch: w }))
  const now = new Date()
  const filename = `TKB_${semester.value.replace(/\s/g, '_')}_${now.toISOString().split('T')[0]}.xlsx`
  XLSX.writeFile(workbook, filename)
  popup.success('Xuất Excel thành công', `File "${filename}" đã được tải xuống.`)
}

function handleExportPDF() {
  const printWindow = window.open('', '_blank')
  if (!printWindow) {
    popup.warning('Trình duyệt chặn cửa sổ popup', 'Vui lòng cho phép popup để xuất PDF.')
    return
  }
  const eventsHtml = filteredSchedules.value.map(e => {
    const statusMap = { published: 'Đã công bố', pending: 'Chờ duyệt', draft: 'Bản nháp' }
    return `<tr>
      <td style="padding:8px;border:1px solid #ddd">${e.title}</td>
      <td style="padding:8px;border:1px solid #ddd">${e.teacher}</td>
      <td style="padding:8px;border:1px solid #ddd">${e.room}</td>
      <td style="padding:8px;border:1px solid #ddd">${e.start}</td>
      <td style="padding:8px;border:1px solid #ddd">${e.end}</td>
      <td style="padding:8px;border:1px solid #ddd">${statusMap[e.status] || e.status}</td>
    </tr>`
  }).join('')

  printWindow.document.write(`<!DOCTYPE html>
<html><head><title>Thời khóa biểu - ${semester.value}</title>
<style>body{font-family:'Segoe UI',sans-serif;padding:40px;color:#1e293b}h1{font-size:24px;margin-bottom:4px}h2{font-size:14px;color:#64748b;font-weight:normal;margin-bottom:24px}table{width:100%;border-collapse:collapse}th{background:#0f766e;color:white;padding:10px 8px;text-align:left;font-size:13px}td{font-size:12px;padding:8px;border:1px solid #ddd}.footer{margin-top:24px;font-size:11px;color:#94a3b8}</style></head>
<body><h1>Thời khóa biểu</h1><h2>Học kỳ: ${semester.value} | Cơ sở: ${campus.value}</h2>
<table><thead><tr><th>Môn học</th><th>Giảng viên</th><th>Phòng</th><th>Bắt đầu</th><th>Kết thúc</th><th>Trạng thái</th></tr></thead><tbody>${eventsHtml}</tbody></table>
<div class="footer">Xuất ngày: ${new Date().toLocaleDateString('vi-VN')} | Hệ thống LMS</div></body></html>`)
  printWindow.document.close()
  printWindow.focus()
  setTimeout(() => printWindow.print(), 500)
}
</script>

<template>
  <PageContainer
    title="Quản lý thời khóa biểu"
    subtitle="Xếp lịch học và quản lý thời khóa biểu cho học kỳ hiện tại."
  >
    <template #actions>
      <div class="flex items-center gap-2">
        <button @click="handleExportPDF"
          class="lg-button-secondary px-3 py-2 text-sm font-bold hover:bg-slate-100 dark:hover:bg-white/10 transition-colors">
          <Printer :size="16" /> PDF
        </button>
        <button @click="handleExportExcel"
          class="lg-button-secondary px-3 py-2 text-sm font-bold hover:bg-slate-100 dark:hover:bg-white/10 transition-colors">
          <Download :size="16" /> Excel
        </button>
        <button @click="showCreateModal = true"
          class="lg-button-primary px-4 py-2 text-sm font-bold shadow-lg shadow-teal-500/20 hover:shadow-xl transition-all">
          <Plus :size="18" /> Tạo lịch mới
        </button>
      </div>
    </template>

    <div class="space-y-3">
      <!-- Stats bar -->
      <div class="lg-glass-soft px-4 py-2.5 rounded-2xl flex items-center gap-4 flex-wrap border border-slate-200/70 dark:border-white/10">
        <div class="flex items-center gap-1.5 text-xs font-bold text-slate-500 dark:text-slate-400">
          <div class="w-2 h-2 rounded-full bg-teal-500" />
          <span>Đã công bố: <strong class="text-slate-700 dark:text-slate-300">{{ publishedEvents }}</strong></span>
        </div>
        <div class="flex items-center gap-1.5 text-xs font-bold text-slate-500 dark:text-slate-400">
          <div class="w-2 h-2 rounded-full bg-amber-500" />
          <span>Chờ duyệt: <strong class="text-slate-700 dark:text-slate-300">{{ pendingEvents }}</strong></span>
        </div>
        <div class="flex items-center gap-1.5 text-xs font-bold text-slate-500 dark:text-slate-400">
          <div class="w-2 h-2 rounded-full bg-slate-400" />
          <span>Bản nháp: <strong class="text-slate-700 dark:text-slate-300">{{ draftEvents }}</strong></span>
        </div>
        <div class="w-px h-4 bg-slate-200 dark:bg-white/10" />
        <div class="flex items-center gap-1.5 text-xs font-bold text-slate-500 dark:text-slate-400">
          <CalendarRange :size="13" />
          <span>Tổng số: <strong class="text-slate-700 dark:text-slate-300">{{ totalEvents }}</strong></span>
        </div>
      </div>

      <!-- Toolbar -->
      <div class="lg-glass-strong p-3 rounded-2xl flex items-center justify-between gap-3 flex-wrap border border-slate-200/70 dark:border-white/10">
        <!-- Month navigation -->
        <div class="flex items-center gap-2">
          <button @click="prevMonth"
            class="p-1.5 hover:bg-white/50 dark:hover:bg-white/10 rounded-lg text-slate-500 dark:text-slate-400 transition-colors">
            <ChevronLeft :size="18" />
          </button>
          <span class="text-sm font-bold text-slate-700 dark:text-slate-300 min-w-[170px] text-center select-none">
            {{ monthLabel }}
          </span>
          <button @click="nextMonth"
            class="p-1.5 hover:bg-white/50 dark:hover:bg-white/10 rounded-lg text-slate-500 dark:text-slate-400 transition-colors">
            <ChevronRight :size="18" />
          </button>
          <button @click="goToToday"
            class="ml-1 px-3 py-1.5 text-xs font-bold rounded-xl bg-white/60 dark:bg-white/10 text-slate-600 dark:text-slate-300 hover:bg-white dark:hover:bg-white/20 border border-slate-200 dark:border-white/10 transition-all">
            Hôm nay
          </button>
        </div>

        <!-- Filters -->
        <div class="flex items-center gap-2">
          <div class="relative">
            <Search :size="13" class="absolute left-2.5 top-1/2 -translate-y-1/2 text-slate-400 dark:text-slate-500" />
            <input v-model="searchQuery" type="text" placeholder="Tìm kiếm..."
              class="bg-white/70 dark:bg-white/5 border border-slate-200 dark:border-white/10 rounded-xl pl-8 pr-2.5 py-1.5 text-xs font-bold outline-none focus:ring-2 focus:ring-teal-500/20 dark:text-slate-200 w-32" />
          </div>
          <select v-model="semester"
            class="bg-white/70 dark:bg-white/5 border border-slate-200 dark:border-white/10 rounded-xl px-2.5 py-1.5 text-xs font-bold outline-none focus:ring-2 focus:ring-teal-500/20 dark:text-slate-200">
            <option>Spring 2026</option>
            <option>Fall 2025</option>
          </select>
          <select v-model="selectedLecturer"
            class="bg-white/70 dark:bg-white/5 border border-slate-200 dark:border-white/10 rounded-xl px-2.5 py-1.5 text-xs font-bold outline-none focus:ring-2 focus:ring-teal-500/20 dark:text-slate-200">
            <option v-for="l in lecturers" :key="l.id" :value="l">{{ l.name }}</option>
          </select>
          <button
            class="lg-icon-button bg-white/70 dark:bg-white/5 border border-slate-200 dark:border-white/10 p-1.5 text-slate-500 dark:text-slate-400">
            <Filter :size="16" />
          </button>
        </div>
      </div>

      <!-- Month Calendar -->
      <MonthView @edit="openEditModal" />

      <!-- Legend -->
      <div class="flex flex-wrap items-center justify-between gap-3 px-1">
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-1.5">
            <span class="h-2.5 w-2.5 rounded-full bg-green-500" />
            <span class="text-[11px] font-bold text-slate-500 dark:text-slate-400">Đã công bố</span>
          </div>
          <div class="flex items-center gap-1.5">
            <span class="h-2.5 w-2.5 rounded-full bg-amber-500" />
            <span class="text-[11px] font-bold text-slate-500 dark:text-slate-400">Chờ duyệt</span>
          </div>
          <div class="flex items-center gap-1.5">
            <span class="h-2.5 w-2.5 rounded-full bg-slate-400" />
            <span class="text-[11px] font-bold text-slate-500 dark:text-slate-400">Bản nháp</span>
          </div>
        </div>

        <div class="flex items-center gap-3 text-[11px] font-bold text-slate-400 dark:text-slate-500">
          <div class="flex items-center gap-1.5">
            <AlertCircle :size="13" class="text-amber-500" />
            <span>Kiểm tra xung đột</span>
          </div>
          <div class="h-3 w-px bg-slate-200 dark:bg-white/10" />
          <div class="flex items-center gap-1.5">
            <CheckCircle2 :size="13" class="text-green-500" />
            <span>Dữ liệu đã đồng bộ</span>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>

  <!-- Create Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showCreateModal"
        class="fixed inset-0 z-[9998] bg-black/40 backdrop-blur-sm flex items-center justify-center p-4"
        @click.self="showCreateModal = false">
        <div class="relative z-[9999] lg-glass-strong rounded-[24px] shadow-2xl max-w-md w-full p-5 border border-slate-200 dark:border-white/10">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-bold text-heading">Tạo lịch mới</h2>
            <button type="button" @click="showCreateModal = false"
              class="p-1.5 hover:bg-black/5 dark:hover:bg-white/10 rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
              <X :size="18" />
            </button>
          </div>
          <form @submit.prevent="handleCreateSchedule" class="space-y-3">
            <div class="grid grid-cols-2 gap-3">
              <div class="col-span-2">
                <label class="block text-xs font-bold text-label mb-1">Môn học <span class="text-red-500">*</span></label>
                <input v-model="newScheduleForm.subject" type="text" placeholder="Nhập tên môn học"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body" />
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Giảng viên</label>
                <select v-model="newScheduleForm.teacher"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option value="">-- Chọn --</option>
                  <option v-for="l in lecturers.filter(x => x.id !== 1)" :key="l.id" :value="l.name">{{ l.name }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Phòng</label>
                <input v-model="newScheduleForm.room" type="text" placeholder="VD: P.302"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body" />
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Loại</label>
                <select v-model="newScheduleForm.type"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option value="class">Lớp học</option>
                  <option value="exam">Thi</option>
                  <option value="meeting">Họp</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Giờ bắt đầu</label>
                <select v-model="newScheduleForm.startTime"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option v-for="t in timeSlots" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Giờ kết thúc</label>
                <select v-model="newScheduleForm.endTime"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option v-for="t in timeSlots" :key="t" :value="t" :disabled="t <= newScheduleForm.startTime">{{ t }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Trạng thái</label>
                <select v-model="newScheduleForm.status"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option value="draft">Bản nháp</option>
                  <option value="pending">Chờ duyệt</option>
                  <option value="published">Đã công bố</option>
                </select>
              </div>
            </div>
            <div class="flex gap-3 pt-3 mt-2 border-t border-slate-200 dark:border-white/10">
              <button type="button" @click="showCreateModal = false"
                class="flex-1 px-4 py-2 rounded-xl border border-slate-200 dark:border-white/10 text-label text-sm font-bold hover:bg-black/5 dark:hover:bg-white/5 transition-colors">Hủy</button>
              <button type="submit"
                class="flex-1 px-4 py-2 rounded-xl bg-teal-600 text-white text-sm font-bold hover:bg-teal-700 transition-colors shadow-lg shadow-teal-500/20">Tạo lịch</button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- Edit Modal -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showEditModal && eventToEdit"
        class="fixed inset-0 z-[9998] bg-black/40 backdrop-blur-sm flex items-center justify-center p-4"
        @click.self="showEditModal = false">
        <div class="relative z-[9999] lg-glass-strong rounded-[24px] shadow-2xl max-w-md w-full p-5 border border-slate-200 dark:border-white/10">
          <div class="flex items-center justify-between mb-4">
            <h2 class="text-lg font-bold text-heading">Chỉnh sửa lịch</h2>
            <button type="button" @click="showEditModal = false"
              class="p-1.5 hover:bg-black/5 dark:hover:bg-white/10 rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-slate-300 transition-colors">
              <X :size="18" />
            </button>
          </div>
          <form @submit.prevent="handleEditSchedule" class="space-y-3">
            <div class="grid grid-cols-2 gap-3">
              <div class="col-span-2">
                <label class="block text-xs font-bold text-label mb-1">Môn học</label>
                <input v-model="editForm.title" type="text"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body" />
              </div>
              <div class="col-span-2">
                <label class="block text-xs font-bold text-label mb-1">Giảng viên</label>
                <select v-model="editForm.teacher"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option v-for="l in lecturers.filter(x => x.id !== 1)" :key="l.id" :value="l.name">{{ l.name }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Phòng</label>
                <input v-model="editForm.room" type="text"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body" />
              </div>
              <div>
                <label class="block text-xs font-bold text-label mb-1">Loại</label>
                <select v-model="editForm.type"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option value="class">Lớp học</option>
                  <option value="exam">Thi</option>
                  <option value="meeting">Họp</option>
                </select>
              </div>
              <div class="col-span-2">
                <label class="block text-xs font-bold text-label mb-1">Trạng thái</label>
                <select v-model="editForm.status"
                  class="w-full px-3 py-2 text-sm rounded-xl border border-input bg-surface-input outline-none focus:ring-2 focus:ring-teal-500/20 focus:border-input-focus transition-colors text-body">
                  <option value="draft">Bản nháp</option>
                  <option value="pending">Chờ duyệt</option>
                  <option value="published">Đã công bố</option>
                </select>
              </div>
            </div>
            <div class="flex gap-2 pt-3 mt-2 border-t border-slate-200 dark:border-white/10">
              <button type="button" @click="handleDeleteEvent"
                class="px-3 py-2 rounded-xl border border-red-200 dark:border-red-500/20 text-red-600 dark:text-red-400 text-sm font-bold hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors">
                <Trash2 :size="14" /> Xóa
              </button>
              <button type="button" @click="showEditModal = false"
                class="flex-1 px-4 py-2 rounded-xl border border-slate-200 dark:border-white/10 text-label text-sm font-bold hover:bg-black/5 dark:hover:bg-white/5 transition-colors">Hủy</button>
              <button type="submit"
                class="flex-1 px-4 py-2 rounded-xl bg-teal-600 text-white text-sm font-bold hover:bg-teal-700 transition-colors shadow-lg shadow-teal-500/20">Cập nhật</button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
