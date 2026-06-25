<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { 
  AlertTriangle, 
  CheckCircle, 
  Search, 
  RefreshCw, 
  Calendar, 
  User, 
  Building,
  ArrowRight,
  ShieldAlert,
  Info,
  Lightbulb,
  Clock,
  X,
  MapPin,
  BookOpen,
  Users
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const router = useRouter()

// ── Form State ──────────────────────────────────────────────
const isChecking = ref(false)
const checkResult = ref(null)
const activeDropdown = ref(null)
const showDetail = ref(false)
const detailData = ref(null)

const teacherSchedule = {
  name: 'Nguyễn Văn A',
  day: 'Thứ 2',
  slots: [
    { start: '07:00', end: '08:30', subject: 'Lập trình Java', class: 'SE1601', room: 'P.302', conflict: false },
    { start: '08:45', end: '10:15', subject: 'Lập trình Java', class: 'SE1601', room: 'P.302', conflict: true },
    { start: '10:30', end: '12:00', subject: 'Cấu trúc dữ liệu', class: 'SE1602', room: 'Lab 1', conflict: false },
    { start: '13:00', end: '14:30', subject: 'Hệ quản trị CSDL', class: 'SE1604', room: 'Lab 304', conflict: false },
  ]
}

const roomSchedule = {
  name: 'Lab 2',
  day: 'Thứ 2',
  slots: [
    { start: '07:00', end: '08:30', subject: 'Đồ họa 2D', class: 'IT201', teacher: 'ThS. Trần Văn B', conflict: false },
    { start: '08:45', end: '10:15', subject: 'Đồ họa 2D', class: 'IT201', teacher: 'ThS. Trần Văn B', conflict: false },
    { start: '10:30', end: '12:00', subject: 'Mạng máy tính', class: 'IT202', teacher: 'TS. Lê Thị C', conflict: true },
    { start: '13:00', end: '14:30', subject: 'An toàn thông tin', class: 'AT21', teacher: 'ThS. Phạm Văn D', conflict: false },
  ]
}

const classSchedule = {
  name: 'SE1601',
  day: 'Thứ 2',
  slots: [
    { start: '07:00', end: '08:30', subject: 'Lập trình Java', room: 'P.302', teacher: 'Nguyễn Văn A', conflict: false },
    { start: '08:45', end: '10:15', subject: 'Lập trình Java', room: 'P.302', teacher: 'Nguyễn Văn A', conflict: true },
    { start: '10:30', end: '12:00', subject: 'Cấu trúc dữ liệu', room: 'Lab 1', teacher: 'Trần Thị B', conflict: false },
    { start: '13:00', end: '14:30', subject: 'Hệ quản trị CSDL', room: 'Lab 304', teacher: 'Lê Văn C', conflict: false },
  ]
}

const form = ref({
  teacherId: '',
  roomId: '',
  classId: '',
  day: 'Thứ 2',
  startTime: '07:30',
  endTime: '09:30'
})

function performCheck() {
  isChecking.value = true
  activeDropdown.value = null
  // Mock check logic
  setTimeout(() => {
    isChecking.value = false
    checkResult.value = {
      hasConflict: true,
      conflicts: [
        { 
          type: 'teacher', 
          message: 'Giảng viên Nguyễn Văn A đã có lịch dạy lớp SE1601 tại P.302 cùng thời điểm.', 
          severity: 'high',
          suggestion: 'Gợi ý: Đổi sang giảng viên ThS. Trần Thị Lan (đang trống lịch)',
          actionRoute: {
            name: 'staff-assignments',
            query: {
              action: 'change-teacher',
              assignmentId: 'PC001',
              suggestedTeacher: 'ThS. Trần Thị Lan',
              autoApply: 'true'
            }
          }
        },
        { 
          type: 'room', 
          message: 'Phòng Lab 2 đang được sử dụng bởi lớp IT202.', 
          severity: 'medium',
          suggestion: 'Gợi ý: Đổi sang phòng Lab 3 (cùng sức chứa, đang trống)',
          actionRoute: {
            name: 'staff-rooms',
            query: {
              action: 'change-room',
              roomId: '3',
              suggestedRoom: 'Lab 3',
              autoApply: 'true'
            }
          }
        }
      ]
    }
  }, 1000)
}

function toggleDropdown(idx) {
  if (activeDropdown.value === idx) {
    activeDropdown.value = null
  } else {
    activeDropdown.value = idx
  }
}

function closeDropdown() {
  activeDropdown.value = null
}

function onDocumentClick(e) {
  if (activeDropdown.value === null) return
  const wrapper = e.target.closest('[data-dropdown-wrapper]')
  if (!wrapper) {
    activeDropdown.value = null
  }
}

onMounted(() => {
  document.addEventListener('click', onDocumentClick)
})

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick)
})

function resolveConflict(conflict, action) {
  activeDropdown.value = null
  if (action === 'change' && conflict.actionRoute) {
    removeConflict(conflict)
    router.push(conflict.actionRoute)
    return
  }
  if (action === 'view') {
    activeDropdown.value = null
    if (conflict.type === 'teacher') {
      detailData.value = { type: 'teacher', ...teacherSchedule }
    } else if (conflict.type === 'class') {
      detailData.value = { type: 'class', ...classSchedule }
    } else {
      detailData.value = { type: 'room', ...roomSchedule }
    }
    showDetail.value = true
    return
  }
  if (action === 'ignore') {
    removeConflict(conflict)
  }
}

function removeConflict(conflict) {
  const idx = checkResult.value.conflicts.indexOf(conflict)
  if (idx !== -1) {
    checkResult.value.conflicts.splice(idx, 1)
    if (checkResult.value.conflicts.length === 0) {
      checkResult.value.hasConflict = false
    }
  }
}

function navigateToSuggestion(conflict) {
  if (conflict.actionRoute) {
    removeConflict(conflict)
    router.push(conflict.actionRoute)
  }
}
</script>

<template>
  <PageContainer 
    title="Kiểm tra xung đột" 
    subtitle="Công cụ kiểm tra trùng lịch giảng viên, phòng học và lớp học trước khi xuất bản TKB."
  >
    <div class="max-w-4xl mx-auto space-y-8">
      
      <!-- ── Input Form ── -->
      <div class="lg-glass-soft p-5 rounded-2xl border-default shadow-sm">
        <h3 class="text-lg font-semibold text-heading mb-4 flex items-center gap-2">
          <Search :size="20" class="text-[var(--lg-primary)]" /> THÔNG TIN KIỂM TRA
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-1.5">
            <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Giảng viên</label>
            <div class="relative">
              <User :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="form.teacherId" type="text" placeholder="Nhập tên hoặc mã GV..." class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Phòng học</label>
            <div class="relative">
              <Building :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="form.roomId" type="text" placeholder="Nhập tên phòng..." class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium">
            </div>
          </div>

          <div class="space-y-1.5">
            <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Ngày trong tuần</label>
            <select v-model="form.day" class="w-full lg-input px-4 py-2.5 text-sm font-medium appearance-none">
              <option>Thứ 2</option><option>Thứ 3</option><option>Thứ 4</option>
              <option>Thứ 5</option><option>Thứ 6</option><option>Thứ 7</option>
            </select>
          </div>

          <div class="grid grid-cols-2 gap-4">
             <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Bắt đầu</label>
               <input v-model="form.startTime" type="time" class="w-full lg-input px-4 py-2.5 text-sm font-medium">
             </div>
             <div class="space-y-1.5">
               <label class="text-[10px] font-semibold text-placeholder uppercase tracking-widest ml-1">Kết thúc</label>
               <input v-model="form.endTime" type="time" class="w-full lg-input px-4 py-2.5 text-sm font-medium">
             </div>
          </div>
        </div>

        <div class="mt-8 flex items-center justify-end gap-4 border-t border-default pt-6">
          <button class="px-4 py-2.5 text-sm font-bold text-label hover:text-heading transition-colors">Xóa form</button>
          <button 
            @click="performCheck"
            :disabled="isChecking"
            class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-[var(--lg-primary)]/20 disabled:opacity-50"
          >
            <RefreshCw v-if="isChecking" :size="18" class="animate-spin" />
            <span v-else>Kiểm tra xung đột</span>
          </button>
        </div>
      </div>

      <!-- ── Results ── -->
      <Transition
        enter-active-class="transition-all duration-300 ease-out"
        enter-from-class="opacity-0 translate-y-4"
        enter-to-class="opacity-100 translate-y-0"
      >
        <div v-if="checkResult" class="space-y-4">
          <div :class="['p-4 rounded-2xl border flex items-start gap-4 shadow-sm', checkResult.hasConflict ? 'bg-[var(--color-danger-bg)] border-[var(--color-danger-bg)]' : 'bg-[var(--color-success-bg)] border-[var(--color-success-bg)]']">
            <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center shrink-0', checkResult.hasConflict ? 'bg-[var(--lg-danger)] text-white shadow-lg shadow-[var(--lg-danger)]/20' : 'bg-[var(--lg-success)] text-white shadow-lg shadow-[var(--lg-success)]/20']">
              <ShieldAlert v-if="checkResult.hasConflict" :size="24" />
              <CheckCircle v-else :size="24" />
            </div>
            <div class="flex-1">
              <h4 :class="['text-lg font-semibold', checkResult.hasConflict ? 'text-[var(--lg-danger)]' : 'text-[var(--lg-success)]']">
                {{ checkResult.hasConflict ? 'Phát hiện xung đột!' : 'Hợp lệ' }}
              </h4>
              <p :class="['text-sm font-medium mt-1', checkResult.hasConflict ? 'text-[var(--color-danger-text)]' : 'text-[var(--color-success-text)]']">
                {{ checkResult.hasConflict ? `Có ${checkResult.conflicts.length} xung đột được phát hiện cho các tiêu chí này.` : 'Không phát hiện bất kỳ xung đột nào. Bạn có thể xếp lịch này.' }}
              </p>
            </div>
          </div>

          <!-- Conflict List -->
          <div v-if="checkResult.hasConflict" class="grid grid-cols-1 gap-3">
              <div v-for="(conflict, idx) in checkResult.conflicts" :key="idx" class="lg-glass-strong p-4 rounded-2xl border-default flex items-start gap-4 group hover:border-[var(--lg-danger)]/20 transition-colors">
                 <div class="h-10 w-10 rounded-xl bg-[var(--surface-input)] flex items-center justify-center text-[var(--lg-danger)] border-default shrink-0">
                   <AlertTriangle :size="20" />
                 </div>
                 <div class="flex-1 pt-0.5">
                   <p class="text-sm font-bold text-heading">{{ conflict.message }}</p>
                   <button v-if="conflict.suggestion" @click="navigateToSuggestion(conflict)" class="inline-flex items-center gap-1.5 mt-2 bg-[var(--color-info-bg)]/50 border border-[var(--color-info-text)]/20 text-[var(--color-info-text)] text-xs font-semibold px-2.5 py-1.5 rounded-lg cursor-pointer hover:bg-[var(--color-info-bg)] hover:shadow-sm transition-all group/suggest">
                     <Lightbulb :size="14" /> {{ conflict.suggestion }}
                     <ArrowRight :size="12" class="opacity-0 group-hover/suggest:opacity-100 transition-opacity" />
                   </button>
                 </div>
                 <div data-dropdown-wrapper class="relative shrink-0">
                    <button @click="toggleDropdown(idx)" class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--color-danger-text)] rounded-lg text-placeholder transition-all relative z-10">
                       <ArrowRight :size="18" :class="['transition-transform duration-200', activeDropdown === idx ? 'rotate-90 text-[var(--lg-danger)]' : '']" />
                    </button>
                    
                    <!-- Dropdown Menu -->
                    <Transition
                      enter-active-class="transition ease-out duration-100"
                      enter-from-class="transform opacity-0 scale-95"
                      enter-to-class="transform opacity-100 scale-100"
                      leave-active-class="transition ease-in duration-75"
                      leave-from-class="transform opacity-100 scale-100"
                      leave-to-class="transform opacity-0 scale-95"
                    >
                      <div
                        v-if="activeDropdown === idx"
                        tabindex="-1"
                        @keydown.escape="closeDropdown"
                        class="absolute right-0 top-full mt-2 w-56 bg-[var(--surface-card)] border border-[var(--border-default)] rounded-xl shadow-xl z-[60] overflow-hidden"
                      >
                        <div class="p-1.5 flex flex-col gap-0.5">
                          <button @click="resolveConflict(conflict, 'view')" class="w-full text-left px-3 py-2 text-sm font-medium text-heading hover:bg-[var(--surface-input)] rounded-lg transition-colors">
                            Xem chi tiết lịch
                          </button>
                          <button @click="resolveConflict(conflict, 'change')" class="w-full text-left px-3 py-2 text-sm font-medium text-heading hover:bg-[var(--surface-input)] rounded-lg transition-colors">
                            {{ conflict.type === 'teacher' ? 'Đổi giảng viên khác' : conflict.type === 'class' ? 'Đổi lớp học khác' : 'Đổi phòng học khác' }}
                          </button>
                          <div class="h-px bg-[var(--border-default)] my-1"></div>
                          <button @click="resolveConflict(conflict, 'ignore')" class="w-full text-left px-3 py-2 text-sm font-medium text-[var(--lg-danger)] hover:bg-[var(--color-danger-bg)] rounded-lg transition-colors">
                            Bỏ qua cảnh báo
                          </button>
                        </div>
                      </div>
                    </Transition>
                 </div>
              </div>
          </div>
        </div>
      </Transition>

      <!-- ── Detail Modal ── -->
      <Teleport to="body">
        <Transition name="modal">
          <div v-if="showDetail && detailData" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showDetail = false">
            <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
            <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
              <div class="modal-header p-5">
                <div class="flex items-center justify-between">
                  <div>
                    <h2 class="text-xl font-semibold text-heading flex items-center gap-2.5">
                      <Calendar :size="20" class="text-[var(--lg-primary)]" />
                      Lịch {{ detailData.type === 'teacher' ? 'giảng viên' : detailData.type === 'class' ? 'lớp học' : 'phòng' }}
                    </h2>
                    <p class="text-sm text-label mt-1 flex items-center gap-2">
                      <User v-if="detailData.type === 'teacher'" :size="14" />
                      <Users v-else-if="detailData.type === 'class'" :size="14" />
                      <MapPin v-else :size="14" />
                      <strong class="text-heading">{{ detailData.name }}</strong>
                      <span class="text-placeholder">•</span>
                      {{ detailData.day }}
                    </p>
                  </div>
                  <button @click="showDetail = false" class="h-9 w-9 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all">
                    <X :size="18" />
                  </button>
                </div>
              </div>
              <div class="p-5 space-y-2">
                <div class="grid grid-cols-[80px_1fr_60px] gap-2 text-[10px] font-semibold text-placeholder uppercase tracking-widest px-3 pb-2 border-b border-default">
                  <span>Giờ</span>
                  <span>Môn học / Lớp</span>
                  <span class="text-right">{{ detailData.type === 'teacher' ? 'Phòng' : detailData.type === 'class' ? 'Phòng' : 'GV' }}</span>
                </div>
                <div v-for="(slot, i) in detailData.slots" :key="i"
                  :class="[
                    'grid grid-cols-[80px_1fr_60px] gap-2 items-center px-3 py-2.5 rounded-xl transition-colors',
                    slot.conflict ? 'bg-[var(--color-danger-bg)] border border-[var(--color-danger-text)]/20' : 'hover:bg-[var(--surface-input)]'
                  ]"
                >
                  <div class="flex items-center gap-1.5 text-xs font-bold text-label">
                    <Clock :size="12" class="text-placeholder" />
                    {{ slot.start }} - {{ slot.end }}
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading leading-tight">{{ slot.subject }}</p>
                    <p class="text-[10px] font-medium text-label mt-0.5">{{ detailData.type === 'teacher' ? slot.class : detailData.type === 'class' ? slot.teacher : slot.class + ' • ' + slot.subject }}</p>
                  </div>
                  <div class="text-right">
                    <span class="text-xs font-semibold text-label">{{ detailData.type === 'teacher' || detailData.type === 'class' ? slot.room : (slot.teacher || '') }}</span>
                    <div v-if="slot.conflict" class="text-[9px] font-bold text-[var(--lg-danger)] uppercase tracking-wider mt-0.5">Xung đột</div>
                  </div>
                </div>
              </div>
              <div class="px-5 pb-5 pt-2 border-t border-default flex items-center justify-end">
                <button @click="showDetail = false" class="lg-button-secondary px-5 py-2.5 text-sm font-bold">Đóng</button>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>

      <!-- ── Information Info ── -->
      <div class="lg-glass-strong p-4 rounded-2xl bg-[var(--color-info-bg)]/50 border-[var(--color-info-bg)]">
        <div class="flex gap-3">
          <Info :size="20" class="text-[var(--color-info-text)] shrink-0 mt-0.5" />
          <div>
            <h5 class="text-sm font-semibold text-[var(--color-info-text)]">Mẹo kiểm tra nhanh</h5>
            <p class="text-xs text-[var(--color-info-text)] mt-1 leading-relaxed">
              Bạn có thể kiểm tra một tiêu chí duy nhất (ví dụ: chỉ phòng) bằng cách để trống các trường khác. Hệ thống sẽ tự động quét toàn bộ cơ sở dữ liệu đã công bố và các bản nháp đang chờ duyệt.
            </p>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>
</template>

<style scoped>
.modal-header {
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
}
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative.w-full,
.modal-leave-to .relative.w-full {
  transform: scale(0.9) translateY(20px);
}
</style>
