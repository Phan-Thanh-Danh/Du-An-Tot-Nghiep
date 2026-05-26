<script setup>
import { ref, computed } from 'vue'
import {
  Plus,
  Search,
  Filter,
  Calendar as CalendarIcon,
  MoreVertical,
  ChevronLeft,
  ChevronRight,
  Download,
  Upload,
  CheckCircle2,
  AlertCircle,
  Clock,
  X
} from 'lucide-vue-next'
import * as XLSX from 'xlsx'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Filters & State ──────────────────────────────────────────
const semester = ref('Spring 2026')
const campus = ref('Cơ sở chính')
const viewMode = ref('Week') // Day, Week, Month
const showCreateModal = ref(false)

// ── Form data for new schedule ──────────────────────────────
const newScheduleForm = ref({
  subject: '',
  class: '',
  teacher: '',
  room: '',
  day: 'Thứ 2',
  startTime: '07:30',
  endTime: '08:30',
  status: 'draft'
})

// ── Lecturers ───────────────────────────────────────────────
const lecturers = [
  { id: 1, name: 'Tất cả giảng viên' },
  { id: 2, name: 'TS. Nguyễn Văn A' },
  { id: 3, name: 'ThS. Trần Thị B' },
  { id: 4, name: 'TS. Lê Văn C' },
  { id: 5, name: 'ThS. Phạm Minh Tuấn' },
]
const selectedLecturer = ref(lecturers[0])

const filteredSchedules = computed(() => {
  if (selectedLecturer.value.id === 1) return schedules.value
  return schedules.value.filter(s => s.teacher === selectedLecturer.value.name)
})

// ── Mock Time Slots ─────────────────────────────────────────
const timeSlots = [
  '07:30', '08:30', '09:30', '10:30', '11:30',
  '12:30', '13:30', '14:30', '15:30', '16:30', '17:30'
]

const days = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'CN']

// ── Mock Schedule Data ──────────────────────────────────────
const schedules = ref([
  {
    id: 1,
    subject: 'Lập trình Java',
    class: 'SE1601',
    teacher: 'Nguyễn Văn A',
    room: 'P.302',
    day: 'Thứ 2',
    startTime: '07:30',
    endTime: '10:30',
    status: 'published',
    color: 'blue'
  },
  {
    id: 2,
    subject: 'Cấu trúc dữ liệu',
    class: 'SE1602',
    teacher: 'Trần Thị B',
    room: 'P.105',
    day: 'Thứ 3',
    startTime: '13:30',
    endTime: '15:30',
    status: 'draft',
    color: 'indigo'
  },
  {
    id: 3,
    subject: 'Web Frontend',
    class: 'SE1603',
    teacher: 'Lê Văn C',
    room: 'Lab 2',
    day: 'Thứ 4',
    startTime: '08:30',
    endTime: '11:30',
    status: 'pending',
    color: 'orange'
  }
])

const getStatusClass = (status) => {
  switch (status) {
    case 'published': return 'lg-badge-success'
    case 'pending': return 'lg-badge-warning'
    case 'draft': return 'lg-badge-violet'
    default: return 'lg-badge-primary'
  }
}

function getSchedule(day, time) {
  return filteredSchedules.value.find(s => s.day === day && s.startTime === time)
}

// ── Export to Excel ──────────────────────────────────────────
function exportToExcel() {
  const dataToExport = filteredSchedules.value.map(schedule => ({
    'Lớp': schedule.class,
    'Môn học': schedule.subject,
    'Giảng viên': schedule.teacher,
    'Phòng': schedule.room,
    'Thứ': schedule.day,
    'Giờ bắt đầu': schedule.startTime,
    'Giờ kết thúc': schedule.endTime,
    'Trạng thái': schedule.status
  }))

  const worksheet = XLSX.utils.json_to_sheet(dataToExport)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Thời khóa biểu')

  // Adjust column width
  const columnWidths = [12, 20, 20, 12, 8, 12, 12, 12]
  worksheet['!cols'] = columnWidths.map(width => ({ wch: width }))

  // Export with filename includes date
  const now = new Date()
  const filename = `TKB_${semester.value}_${now.toISOString().split('T')[0]}.xlsx`
  XLSX.writeFile(workbook, filename)
}

// ── Create new schedule ──────────────────────────────────────
function handleCreateSchedule() {
  if (!newScheduleForm.value.subject || !newScheduleForm.value.class || !newScheduleForm.value.teacher) {
    alert('Vui lòng điền đầy đủ thông tin')
    return
  }

  const newSchedule = {
    id: Math.max(...schedules.value.map(s => s.id), 0) + 1,
    ...newScheduleForm.value,
    color: 'indigo'
  }

  schedules.value.push(newSchedule)

  // Reset form
  newScheduleForm.value = {
    subject: '',
    class: '',
    teacher: '',
    room: '',
    day: 'Thứ 2',
    startTime: '07:30',
    endTime: '08:30',
    status: 'draft'
  }

  showCreateModal.value = false
  alert('Tạo lịch mới thành công!')
}
</script>

<template>
  <PageContainer
    title="Quản lý thời khóa biểu"
    subtitle="Xếp lịch học, kéo thả và kiểm tra xung đột cho học kỳ hiện tại."
  >
    <template #actions>
      <div class="flex items-center gap-3">
        <button @click="exportToExcel" class="lg-button-secondary px-4 py-2 text-sm font-bold hover:bg-slate-100 transition-colors">
          <Download :size="16" /> Xuất Excel
        </button>
        <button @click="showCreateModal = true" class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 hover:shadow-xl transition-all">
          <Plus :size="18" /> Tạo lịch mới
        </button>
      </div>
    </template>

    <div class="space-y-6">
      <!-- ── Toolbar ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex items-center justify-between gap-4">
        <div class="flex items-center gap-4">
          <div class="flex items-center bg-white/50 rounded-xl p-1 border border-slate-100">
            <button
              v-for="mode in ['Day', 'Week', 'Month']"
              :key="mode"
              @click="viewMode = mode"
              :class="[
                'px-4 py-1.5 text-xs font-bold rounded-lg transition-all',
                viewMode === mode ? 'bg-white text-blue-700 shadow-sm' : 'text-slate-500 hover:text-slate-700'
              ]"
            >
              {{ mode }}
            </button>
          </div>

          <div class="h-6 w-px bg-slate-200"></div>

          <div class="flex items-center gap-2">
            <button class="p-2 hover:bg-white rounded-lg text-slate-500 transition-colors">
              <ChevronLeft :size="20" />
            </button>
            <span class="text-sm font-bold text-slate-700">12/05 - 18/05, 2026</span>
            <button class="p-2 hover:bg-white rounded-lg text-slate-500 transition-colors">
              <ChevronRight :size="20" />
            </button>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <select v-model="semester" class="bg-white border border-slate-100 rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-2 focus:ring-blue-500/20">
            <option>Spring 2026</option>
            <option>Fall 2025</option>
          </select>
          <select v-model="campus" class="bg-white border border-slate-100 rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-2 focus:ring-blue-500/20">
            <option>Cơ sở chính</option>
            <option>Cơ sở phụ</option>
          </select>
          <select v-model="selectedLecturer" class="bg-white border border-slate-100 rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-2 focus:ring-blue-500/20">
            <option v-for="l in lecturers" :key="l.id" :value="l">{{ l.name }}</option>
          </select>
          <button class="lg-icon-button bg-white border border-slate-100 p-2 text-slate-500">
            <Filter :size="18" />
          </button>
        </div>
      </div>

      <!-- ── Calendar Grid ── -->
      <div class="lg-glass-strong rounded-[32px] overflow-hidden border border-slate-100 shadow-sm">
        <div class="overflow-x-auto">
          <table class="w-full border-collapse">
            <thead>
              <tr class="bg-slate-50/50">
                <th class="p-4 border-b border-r border-slate-100 w-20 text-[10px] font-black text-slate-400 uppercase tracking-widest">Thời gian</th>
                <th v-for="day in days" :key="day" class="p-4 border-b border-r border-slate-100 min-w-[160px]">
                  <div class="text-center">
                    <p class="text-[11px] font-black text-slate-400 uppercase tracking-widest">{{ day }}</p>
                    <p class="text-lg font-bold text-slate-800">1{{ days.indexOf(day) + 2 }}</p>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="time in timeSlots" :key="time" class="group">
                <td class="p-4 border-b border-r border-slate-100 text-center">
                  <span class="text-xs font-bold text-slate-500">{{ time }}</span>
                </td>
                <td v-for="day in days" :key="day" class="border-b border-r border-slate-100 p-2 relative min-h-[100px] hover:bg-blue-50/30 transition-colors cursor-pointer">
                  <!-- Event Card -->
                  <div
                    v-if="getSchedule(day, time)"
                    :class="[
                      'schedule-card p-3 rounded-2xl border transition-all hover:scale-[1.02] cursor-grab active:cursor-grabbing',
                      `schedule-card-${getSchedule(day, time).status}`
                    ]"
                  >
                    <div class="flex items-start justify-between">
                      <p class="schedule-card-class text-[10px] font-bold uppercase tracking-tighter">{{ getSchedule(day, time).class }}</p>
                      <MoreVertical :size="14" class="schedule-card-more cursor-pointer" />
                    </div>
                    <p class="schedule-card-subject text-sm font-black mt-1 leading-tight">{{ getSchedule(day, time).subject }}</p>
                    <div class="mt-3 flex items-center justify-between gap-2">
                       <div class="flex flex-col gap-0.5">
                          <span class="schedule-card-meta text-[10px] font-bold">{{ getSchedule(day, time).teacher }}</span>
                          <span class="schedule-card-meta text-[10px] font-bold">{{ getSchedule(day, time).room }}</span>
                       </div>
                       <span :class="['px-2 py-0.5 rounded-full text-[9px] font-black uppercase tracking-widest border', getStatusClass(getSchedule(day, time).status)]">
                         {{ getSchedule(day, time).status }}
                       </span>
                    </div>
                  </div>

                  <!-- Empty state hint on hover -->
                  <div class="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                    <Plus :size="16" class="text-blue-300" />
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- ── Legend & Status ── -->
      <div class="flex flex-wrap items-center justify-between gap-6 px-4">
        <div class="flex items-center gap-6">
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-green-500"></span>
            <span class="text-xs font-bold text-slate-600">Đã công bố</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-amber-500"></span>
            <span class="text-xs font-bold text-slate-600">Chờ duyệt</span>
          </div>
          <div class="flex items-center gap-2">
            <span class="h-3 w-3 rounded-full bg-slate-400"></span>
            <span class="text-xs font-bold text-slate-600">Bản nháp</span>
          </div>
        </div>

        <div class="flex items-center gap-4 text-xs font-bold text-slate-500">
          <div class="flex items-center gap-2">
            <AlertCircle :size="14" class="text-amber-500" />
            <span>2 xung đột cần xử lý</span>
          </div>
          <div class="h-4 w-px bg-slate-200"></div>
          <div class="flex items-center gap-2">
            <CheckCircle2 :size="14" class="text-green-500" />
            <span>Dữ liệu đã được lưu</span>
          </div>
        </div>
      </div>
    </div>
  </PageContainer>

  <!-- ── Modal Create New Schedule ── -->
  <div v-if="showCreateModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
    <div class="bg-white rounded-[24px] shadow-2xl max-w-md w-full p-6 border border-slate-100">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-bold text-slate-900">Tạo lịch mới</h2>
        <button @click="showCreateModal = false" class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 hover:text-slate-600 transition-colors">
          <X :size="20" />
        </button>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleCreateSchedule" class="space-y-4">
        <!-- Môn học -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Môn học</label>
          <input
            v-model="newScheduleForm.subject"
            type="text"
            placeholder="Nhập tên môn học"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Lớp -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Lớp</label>
          <input
            v-model="newScheduleForm.class"
            type="text"
            placeholder="Nhập mã lớp (vd: SE1601)"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Giảng viên -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Giảng viên</label>
          <select
            v-model="newScheduleForm.teacher"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option value="">-- Chọn giảng viên --</option>
            <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name" v-show="lecturer.id !== 1">
              {{ lecturer.name }}
            </option>
          </select>
        </div>

        <!-- Phòng -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Phòng</label>
          <input
            v-model="newScheduleForm.room"
            type="text"
            placeholder="Nhập mã phòng (vd: P.302)"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Thứ -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Thứ</label>
          <select
            v-model="newScheduleForm.day"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option v-for="day in days" :key="day" :value="day">{{ day }}</option>
          </select>
        </div>

        <!-- Giờ bắt đầu -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Giờ bắt đầu</label>
          <select
            v-model="newScheduleForm.startTime"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option v-for="time in timeSlots" :key="time" :value="time">{{ time }}</option>
          </select>
        </div>

        <!-- Giờ kết thúc -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Giờ kết thúc</label>
          <select
            v-model="newScheduleForm.endTime"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option v-for="time in timeSlots" :key="time" :value="time" :disabled="time <= newScheduleForm.startTime">{{ time }}</option>
          </select>
        </div>

        <!-- Trạng thái -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Trạng thái</label>
          <select
            v-model="newScheduleForm.status"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option value="draft">Bản nháp</option>
            <option value="pending">Chờ duyệt</option>
            <option value="published">Đã công bố</option>
          </select>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-slate-200">
          <button
            type="button"
            @click="showCreateModal = false"
            class="flex-1 px-4 py-2.5 rounded-lg border border-slate-200 text-slate-700 font-bold hover:bg-slate-50 transition-colors"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="flex-1 px-4 py-2.5 rounded-lg bg-blue-600 text-white font-bold hover:bg-blue-700 transition-colors shadow-lg shadow-blue-500/20"
          >
            Tạo lịch
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
/* Custom scrollbar for calendar */
.overflow-x-auto::-webkit-scrollbar {
  height: 6px;
}
.overflow-x-auto::-webkit-scrollbar-track {
  background: transparent;
}
.overflow-x-auto::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 99px;
}
</style>

<style>
/* Schedule Card Custom Styles */
.schedule-card {
  background: #ffffff !important;
  border: 1px solid #e2e8f0 !important;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.05) !important;
  backdrop-filter: none !important;
  -webkit-backdrop-filter: none !important;
}

.schedule-card .schedule-card-class {
  color: #64748b !important;
  opacity: 1 !important;
}

.schedule-card .schedule-card-meta {
  color: #475569 !important;
  opacity: 1 !important;
}

.schedule-card .schedule-card-more {
  color: #94a3b8 !important;
  opacity: 1 !important;
}

.schedule-card-published .schedule-card-subject {
  color: #0f172a !important;
}

.schedule-card-pending .schedule-card-subject {
  color: #d97706 !important;
}

.schedule-card-draft .schedule-card-subject {
  color: #7c3aed !important;
}

/* Dark Mode Overrides */
.dark .schedule-card {
  background: rgba(15, 23, 42, 0.52) !important;
  border: 1px solid rgba(255, 255, 255, 0.14) !important;
  box-shadow: 0 10px 30px rgba(2, 6, 23, 0.32) !important;
  backdrop-filter: blur(20px) saturate(170%) !important;
  -webkit-backdrop-filter: blur(20px) saturate(170%) !important;
}

.dark .schedule-card .schedule-card-class {
  color: #94a3b8 !important;
}

.dark .schedule-card .schedule-card-meta {
  color: #cbd5e1 !important;
}

.dark .schedule-card .schedule-card-more {
  color: rgba(255, 255, 255, 0.4) !important;
}

.dark .schedule-card-published {
  border-top: 4px solid rgba(37, 99, 235, 0.55) !important;
}

.dark .schedule-card-published .schedule-card-subject {
  color: #93c5fd !important;
}

.dark .schedule-card-pending {
  border-top: 4px solid rgba(245, 158, 11, 0.55) !important;
}

.dark .schedule-card-pending .schedule-card-subject {
  color: #fcd34d !important;
}

.dark .schedule-card-draft {
  border-top: 4px solid rgba(124, 58, 237, 0.55) !important;
}

.dark .schedule-card-draft .schedule-card-subject {
  color: #d8b4fe !important;
}
</style>
