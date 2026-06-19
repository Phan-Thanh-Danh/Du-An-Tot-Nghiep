<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  UserCheck,
  ChevronDown,
  Calendar,
  FileText,
  Clock,
  CheckCircle,
  XCircle,
  AlertTriangle,
  ChevronLeft,
  Send,
  Upload,
  X
} from 'lucide-vue-next'
import { childrenData, getActiveChild, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)
const filterStatus = ref('Tất cả')

// State của modal xin phép nghỉ học
const isModalOpen = ref(false)
const formSubject = ref('')
const formDate = ref('')
const formShift = ref('Ca 1 (07:30 - 09:30)')
const formReason = ref('')
const formFile = ref(null)
const fileName = ref('')

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  filterStatus.value = 'Tất cả'
  // reset form
  formSubject.value = currentChild.value.grades['Học kỳ 1 - 2025-2026']?.[0]?.name || ''
  router.replace({ query: { studentId: id } })
}

// Bảng điểm danh sau khi lọc
const filteredAttendance = computed(() => {
  const history = currentChild.value.attendanceHistory || []
  if (filterStatus.value === 'Tất cả') return history
  return history.filter(item => item.status === filterStatus.value)
})

// Môn học để chọn nghỉ phép
const activeSubjects = computed(() => {
  const grades = currentChild.value.grades['Học kỳ 1 - 2025-2026'] || []
  return grades.map(g => g.name)
})

// SVG Donut Chart variables
const radius = 50
const circumference = 2 * Math.PI * radius // ~314.16
const strokeDashoffset = computed(() => {
  const rate = currentChild.value.attendanceRate || 0
  return circumference - (rate / 100) * circumference
})

function openLeaveModal() {
  if (activeSubjects.value.length > 0) {
    formSubject.value = activeSubjects.value[0]
  }
  formDate.value = new Date().toISOString().substring(0, 10)
  formReason.value = ''
  formFile.value = null
  fileName.value = ''
  isModalOpen.value = true
}

function handleFileChange(event) {
  const file = event.target.files[0]
  if (file) {
    formFile.value = file
    fileName.value = file.name
  }
}

function submitLeaveRequest() {
  if (!formSubject.value || !formDate.value || !formReason.value.trim()) {
    popupStore.error('Thiếu thông tin', 'Vui lòng điền đầy đủ các thông tin bắt buộc.')
    return
  }

  // Giả lập gửi lên hệ thống
  isModalOpen.value = false
  popupStore.success(
    'Gửi đơn xin phép thành công',
    `Đơn xin nghỉ học môn "${formSubject.value}" ngày ${formDate.value} đã được chuyển tới Giảng viên bộ môn phê duyệt.`
  )
}

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div class="flex items-center gap-2">
        <button
          @click="goBack"
          class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
          title="Quay lại"
        >
          <ChevronLeft :size="18" />
        </button>
        <div>
          <h2 class="text-lg font-bold text-heading flex items-center gap-2">
            <UserCheck :size="20" class="text-orange-600" />
            Tình hình điểm danh
          </h2>
          <p class="text-xs text-body">Giám sát tần suất lên lớp, các buổi đi muộn hoặc vắng mặt của con em</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild.name }}</span>
          </div>
          <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
        </button>

        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 translate-y-2 scale-95"
          enter-to-class="opacity-100 translate-y-0 scale-100"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0 scale-100"
          leave-to-class="opacity-0 translate-y-2 scale-95"
        >
          <div
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-[var(--lg-shadow-md)]"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-[var(--surface-card-hover)]"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── THỐNG KÊ CHUYÊN CẦN TỔNG QUAN ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Biểu đồ tròn & Đơn nghỉ phép (1/3 width) -->
      <div class="lg-card-glass p-5 flex flex-col items-center justify-between gap-5 text-center">
        <div class="w-full">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide text-left mb-3">
            Tỷ lệ chuyên cần
          </h3>
          
          <!-- SVG Donut Chart -->
          <div class="relative flex items-center justify-center h-36 w-full">
            <svg class="h-32 w-32 transform -rotate-90">
              <!-- Background circle -->
              <circle
                cx="64"
                cy="64"
                :r="radius"
                fill="transparent"
                stroke="var(--border-default)"
                stroke-width="10"
              />
              <!-- Progress circle -->
              <circle
                cx="64"
                cy="64"
                :r="radius"
                fill="transparent"
                stroke="#ea580c"
                stroke-width="10"
                stroke-linecap="round"
                :stroke-dasharray="circumference"
                :stroke-dashoffset="strokeDashoffset"
                class="transition-all duration-700 ease-out"
              />
            </svg>
            <div class="absolute flex flex-col items-center justify-center">
              <span class="text-2xl font-extrabold text-heading">{{ currentChild.attendanceRate }}%</span>
              <span class="text-[9px] text-muted font-bold uppercase tracking-wider">Lên lớp</span>
            </div>
          </div>
        </div>

        <div class="w-full pt-4 border-t border-card">
          <button
            @click="openLeaveModal"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white w-full py-2 rounded-xl flex items-center justify-center gap-2 font-bold text-xs"
          >
            <Calendar :size="14" />
            Xin nghỉ học trực tuyến
          </button>
          <p class="text-[9px] text-muted mt-2 leading-relaxed">
            * Đơn xin nghỉ phép trực tuyến của phụ huynh sẽ được gửi trực tiếp đến Giảng viên bộ môn duyệt.
          </p>
        </div>
      </div>

      <!-- Khối KPI thống kê số buổi (2/3 width) -->
      <div class="lg-card-glass p-5 lg:col-span-2 flex flex-col justify-between space-y-4">
        <div>
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide mb-3">
            Tóm tắt số buổi học
          </h3>
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
            
            <div class="p-4 surface-card border border-card rounded-2xl flex flex-col justify-between items-center text-center">
              <span class="p-2 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-600 rounded-xl mb-2">
                <CheckCircle :size="18" />
              </span>
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Có mặt</span>
                <span class="text-lg font-extrabold text-heading mt-1 block">{{ currentChild.presentCount }} buổi</span>
              </div>
            </div>

            <div class="p-4 surface-card border border-card rounded-2xl flex flex-col justify-between items-center text-center">
              <span class="p-2 bg-blue-50 dark:bg-blue-950/20 text-blue-600 rounded-xl mb-2">
                <Clock :size="18" />
              </span>
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Đi muộn</span>
                <span class="text-lg font-extrabold text-heading mt-1 block">{{ currentChild.lateCount }} buổi</span>
              </div>
            </div>

            <div class="p-4 surface-card border border-card rounded-2xl flex flex-col justify-between items-center text-center">
              <span class="p-2 bg-orange-50 dark:bg-orange-950/20 text-orange-600 rounded-xl mb-2">
                <FileText :size="18" />
              </span>
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Vắng có phép</span>
                <span class="text-lg font-extrabold text-heading mt-1 block">{{ currentChild.excusedAbsences }} buổi</span>
              </div>
            </div>

            <div class="p-4 surface-card border border-card rounded-2xl flex flex-col justify-between items-center text-center">
              <span class="p-2 bg-red-50 dark:bg-red-950/20 text-red-600 rounded-xl mb-2">
                <XCircle :size="18" />
              </span>
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Vắng không phép</span>
                <span class="text-lg font-extrabold text-red-500 mt-1 block">{{ currentChild.unexcusedAbsences }} buổi</span>
              </div>
            </div>

          </div>
        </div>

        <div class="p-3 bg-amber-50/40 dark:bg-amber-950/5 border border-amber-200 dark:border-amber-950/20 rounded-xl flex items-start gap-2.5">
          <AlertTriangle :size="16" class="text-amber-600 mt-0.5 flex-shrink-0" />
          <p class="text-[11px] text-body leading-relaxed">
            <strong>Quy định:</strong> Sinh viên không được phép vắng mặt vượt quá <strong>20%</strong> tổng số buổi học của mỗi môn. Việc vắng không phép quá quy định sẽ trực tiếp dẫn đến bị **Đình chỉ thi (Học lại)** môn học đó.
          </p>
        </div>
      </div>
    </div>

    <!-- ── LỊCH SỬ ĐIỂM DANH CHI TIẾT ── -->
    <div class="lg-card-glass p-5 space-y-4">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 pb-3 border-b border-card">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
          Nhật ký điểm danh chi tiết
        </h3>
        
        <!-- Bộ lọc nhanh trạng thái -->
        <div class="flex flex-wrap gap-1.5">
          <button
            v-for="status in ['Tất cả', 'Có mặt', 'Đi muộn', 'Vắng có phép', 'Vắng không phép']"
            :key="status"
            @click="filterStatus = status"
            class="px-2.5 py-1 text-[10px] rounded-lg font-semibold border transition"
            :class="filterStatus === status ? 'bg-orange-600 border-orange-600 text-white' : 'border-card text-label hover:text-orange-600'"
          >
            {{ status }}
          </button>
        </div>
      </div>

      <div v-if="filteredAttendance.length === 0" class="text-center py-12 text-muted text-xs">
        Không tìm thấy dữ liệu điểm danh phù hợp với bộ lọc trạng thái.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse min-w-[650px]">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-3 px-3">Ngày học</th>
              <th class="py-3 px-3">Môn học</th>
              <th class="py-3 px-3">Ca học</th>
              <th class="py-3 px-3">Trạng thái</th>
              <th class="py-3 px-3 text-right">Ghi chú của Giảng viên</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr
              v-for="(item, idx) in filteredAttendance"
              :key="idx"
              class="hover:bg-[var(--surface-table-row-hover)] transition"
            >
              <td class="py-3 px-3 font-semibold text-heading">{{ item.date }}</td>
              <td class="py-3 px-3 font-medium text-body">{{ item.subject }}</td>
              <td class="py-3 px-3 text-muted">{{ item.shift }}</td>
              <td class="py-3 px-3">
                <span
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold"
                  :class="
                    item.status === 'Có mặt' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' :
                    item.status === 'Đi muộn' ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/30 dark:text-blue-400' :
                    item.status === 'Vắng có phép' ? 'bg-amber-100 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400' :
                    'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400'
                  "
                >
                  {{ item.status }}
                </span>
              </td>
              <td class="py-3 px-3 text-right text-muted italic font-medium">
                {{ item.note || '-' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ── MODAL XIN NGHỈ PHÉP TRỰC TUYẾN ── -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Overlay -->
      <div @click="isModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm" />
      
      <!-- Modal Content -->
      <div class="lg-modal w-full max-w-lg relative z-10 flex flex-col rounded-2xl shadow-xl overflow-hidden">
        
        <!-- Header -->
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h2 class="text-sm font-bold text-heading flex items-center gap-2">
            <Calendar :size="16" class="text-orange-600" />
            Đơn xin nghỉ học trực tuyến
          </h2>
          <button @click="isModalOpen = false" class="text-muted hover:text-orange-600">
            <X :size="16" />
          </button>
        </div>

        <!-- Body -->
        <form @submit.prevent="submitLeaveRequest" class="space-y-4 mt-4">
          <!-- Tên con -->
          <div class="space-y-1">
            <label class="text-[11px] font-bold text-label block">Học sinh đăng ký</label>
            <input
              type="text"
              :value="currentChild.name + ' (' + currentChild.studentId + ')'"
              disabled
              class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none opacity-60"
            />
          </div>

          <!-- Môn học -->
          <div class="space-y-1">
            <label class="text-[11px] font-bold text-label block">Môn học xin nghỉ *</label>
            <select
              v-model="formSubject"
              required
              class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
            >
              <option v-for="sub in activeSubjects" :key="sub" :value="sub">
                {{ sub }}
              </option>
            </select>
          </div>

          <!-- Ngày & Ca -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-label block">Ngày xin nghỉ *</label>
              <input
                v-model="formDate"
                type="date"
                required
                class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
              />
            </div>
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-label block">Ca học xin nghỉ *</label>
              <select
                v-model="formShift"
                required
                class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
              >
                <option value="Ca 1 (07:30 - 09:30)">Ca 1 (07:30 - 09:30)</option>
                <option value="Ca 2 (09:30 - 11:30)">Ca 2 (09:30 - 11:30)</option>
                <option value="Ca 3 (12:30 - 14:30)">Ca 3 (12:30 - 14:30)</option>
                <option value="Ca 4 (14:45 - 16:45)">Ca 4 (14:45 - 16:45)</option>
              </select>
            </div>
          </div>

          <!-- Lý do nghỉ học -->
          <div class="space-y-1">
            <label class="text-[11px] font-bold text-label block">Lý do xin nghỉ phép *</label>
            <textarea
              v-model="formReason"
              required
              rows="3"
              placeholder="Vui lòng nhập lý do cụ thể (ví dụ: bị sốt xuất huyết nhập viện, gia đình có việc hiếu hỉ...)"
              class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20 resize-none"
            ></textarea>
          </div>

          <!-- Đính kèm file minh chứng -->
          <div class="space-y-1">
            <label class="text-[11px] font-bold text-label block">Tệp minh chứng đính kèm (nếu có)</label>
            <div class="flex items-center gap-2">
              <label class="px-3 py-2 surface-input border-card rounded-xl text-xs font-semibold text-label cursor-pointer hover:bg-orange-50 dark:hover:bg-orange-950/20 flex items-center gap-1.5 transition border">
                <Upload :size="13" /> Chọn tệp tin
                <input type="file" @change="handleFileChange" class="hidden" accept=".jpg,.jpeg,.png,.pdf,.doc,.docx" />
              </label>
              <span class="text-xs text-muted truncate max-w-xs">
                {{ fileName || 'Chưa chọn tệp tin nào (chấp nhận PDF, hình ảnh, word)' }}
              </span>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-end gap-2 pt-3 border-t border-card mt-4">
            <button
              type="button"
              @click="isModalOpen = false"
              class="px-4 py-2 border border-card text-xs font-semibold rounded-xl text-label hover:bg-slate-50 dark:hover:bg-slate-900 transition"
            >
              Hủy bỏ
            </button>
            <button
              type="submit"
              class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl flex items-center gap-1.5 font-bold text-xs"
            >
              <Send :size="13" /> Gửi đơn xin phép
            </button>
          </div>
        </form>

      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
