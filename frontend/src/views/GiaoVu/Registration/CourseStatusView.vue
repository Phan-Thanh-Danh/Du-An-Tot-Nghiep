<script setup>
import { ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { 
  AlertCircle, 
  CheckCircle2, 
  XCircle, 
  MessageSquare, 
  RefreshCw, 
  MoreVertical,
  Mail,
  Users,
  X,
  Loader2,
  Eye,
  RotateCcw
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const popupStore = usePopupStore()

const pendingClasses = ref([
  { id: 'LHP003', subject: 'Lập trình Web', enrolled: 12, minEnroll: 15, teacher: 'Lê Văn C', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
  { id: 'LHP008', subject: 'Kỹ năng mềm', enrolled: 8, minEnroll: 20, teacher: 'Trần Thị H', status: 'pending_cancel', reason: 'Không đủ sĩ số tối thiểu' },
])

const cancelledClasses = ref([
  { id: 'LHP012', subject: 'Triết học Mác-Lênin', enrolled: 5, minEnroll: 20, teacher: 'Nguyễn Văn K', status: 'cancelled', date: '12/05/2026' },
])

const contextTarget = ref(null)

function toggleContextMenu(cls) {
  contextTarget.value = contextTarget.value?.id === cls.id ? null : cls
}

function closeContextMenu() {
  contextTarget.value = null
}

const showCancelModal = ref(false)
const cancelTarget = ref(null)
const cancelReason = ref('')
const cancelling = ref(false)

function openCancelModal(cls) {
  cancelTarget.value = cls
  cancelReason.value = ''
  cancelling.value = false
  showCancelModal.value = true
}

function confirmCancel() {
  if (!cancelTarget.value) return
  cancelling.value = true
  setTimeout(() => {
      const idx = pendingClasses.value.findIndex(c => c.id === cancelTarget.value.id)
      if (idx !== -1) {
        const removed = pendingClasses.value[idx]
        pendingClasses.value.splice(idx, 1)
        cancelledClasses.value.unshift({
          ...removed,
          status: 'cancelled',
          date: new Date().toLocaleDateString('vi-VN'),
        })
      }
    cancelling.value = false
    showCancelModal.value = false
    popupStore.success('Đã hủy lớp', `Lớp ${cancelTarget.value.id} - ${cancelTarget.value.subject} đã bị hủy.`)
    cancelTarget.value = null
  }, 600)
}

function closeCancelModal() {
  showCancelModal.value = false
  cancelTarget.value = null
  cancelReason.value = ''
}

const showReopenModal = ref(false)
const reopenTarget = ref(null)
const reopening = ref(false)
const reopenSource = ref('pending')

function openReopenModal(cls, source) {
  reopenTarget.value = cls
  reopenSource.value = source
  reopening.value = false
  showReopenModal.value = true
}

function confirmReopen() {
  if (!reopenTarget.value) return
  reopening.value = true
  setTimeout(() => {
    if (reopenSource.value === 'pending') {
      const idx = pendingClasses.value.findIndex(c => c.id === reopenTarget.value.id)
      if (idx !== -1) {
        pendingClasses.value.splice(idx, 1)
      }
    } else {
      const idx = cancelledClasses.value.findIndex(c => c.id === reopenTarget.value.id)
      if (idx !== -1) {
        cancelledClasses.value.splice(idx, 1)
      }
    }
    reopening.value = false
    showReopenModal.value = false
    popupStore.success('Đã mở lại lớp', `Lớp ${reopenTarget.value.id} - ${reopenTarget.value.subject} đã được mở lại.`)
    reopenTarget.value = null
  }, 600)
}

function closeReopenModal() {
  showReopenModal.value = false
  reopenTarget.value = null
}

const showDetailModal = ref(false)
const detailTarget = ref(null)

function openDetailModal(cls) {
  detailTarget.value = cls
  showDetailModal.value = true
  closeContextMenu()
}

function closeDetailModal() {
  showDetailModal.value = false
  detailTarget.value = null
}
</script>

<template>
  <PageContainer 
    title="Hủy / Mở lại lớp" 
    subtitle="Xử lý các lớp học phần không đủ sĩ số tối thiểu hoặc cần đóng/mở lại theo nhu cầu."
  >
    <div class="space-y-8" @click="closeContextMenu">
      
      <section class="space-y-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-semibold text-heading flex items-center gap-2">
            <AlertCircle :size="22" class="text-(--lg-danger)" /> LỚP CHỜ HỦY (DƯỚI MIN ENROLL)
          </h3>
          <span class="px-2 py-0.5 rounded-lg bg-(--color-danger-bg) text-(--lg-danger) text-[10px] font-semibold uppercase tracking-widest">{{ pendingClasses.length }} Lớp</span>
        </div>

        <div class="grid grid-cols-1 gap-4">
          <div v-for="cls in pendingClasses" :key="cls.id" class="lg-card-glass p-4 flex flex-col md:flex-row md:items-center justify-between gap-4 group hover:border-(--color-danger-bg)/50 transition-all">
            <div class="flex items-center gap-4">
              <div class="h-10 w-10 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center text-(--lg-danger) border border-(--color-danger-bg)/50 shrink-0">
                <Users :size="28" />
              </div>
              <div>
                <h4 class="text-base font-semibold text-heading">{{ cls.subject }}</h4>
                <div class="mt-1 flex items-center gap-3">
                  <span class="text-[10px] font-semibold text-link uppercase">{{ cls.id }}</span>
                  <span class="h-1 w-1 rounded-full bg-(--border-default)"></span>
                  <span class="text-xs font-bold text-label">{{ cls.teacher }}</span>
                  <span class="h-1 w-1 rounded-full bg-(--border-default)"></span>
                  <span class="text-[10px] font-medium text-(--lg-warning)">{{ cls.reason }}</span>
                </div>
              </div>
            </div>

            <div class="flex flex-wrap items-center gap-4">
              <div class="px-4 border-x border-default">
                <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Sĩ số hiện tại</p>
                <p class="text-lg font-semibold text-(--lg-danger)">{{ cls.enrolled }} <span class="text-placeholder font-medium">/ {{ cls.minEnroll }}</span></p>
              </div>

              <div class="flex items-center gap-2">
                <button class="lg-button-secondary px-4 py-2 text-xs font-bold text-(--lg-success) hover:bg-(--color-success-bg)" @click.stop="openReopenModal(cls, 'pending')">
                  <CheckCircle2 :size="16" /> Mở lại lớp
                </button>
                <button class="px-5 py-2.5 text-xs font-bold text-white bg-(--lg-danger) hover:opacity-90 rounded-2xl transition-all flex items-center gap-2" @click.stop="openCancelModal(cls)">
                  <XCircle :size="16" /> Xác nhận hủy
                </button>
                <div class="relative">
                  <button class="p-2 hover:bg-(--surface-solid) rounded-lg text-placeholder transition-all" @click.stop="toggleContextMenu(cls)">
                    <MoreVertical :size="16" />
                  </button>
                  <Transition
                    enter-active-class="transition-all duration-150 ease-out"
                    enter-from-class="opacity-0 scale-95"
                    enter-to-class="opacity-100 scale-100"
                    leave-active-class="transition-all duration-100 ease-in"
                    leave-from-class="opacity-100 scale-100"
                    leave-to-class="opacity-0 scale-95"
                  >
                    <div v-if="contextTarget?.id === cls.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-sm" @click.stop>
                      <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-success-bg) hover:text-(--lg-success) transition-all" @click="openReopenModal(cls, 'pending'); closeContextMenu()">
                        <CheckCircle2 :size="14" /> Mở lại lớp
                      </button>
                      <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-danger-bg) hover:text-(--lg-danger) transition-all" @click="openCancelModal(cls); closeContextMenu()">
                        <XCircle :size="14" /> Xác nhận hủy
                      </button>
                      <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-info-bg) hover:text-link transition-all" @click="openDetailModal(cls)">
                        <Eye :size="14" /> Xem chi tiết
                      </button>
                    </div>
                  </Transition>
                </div>
              </div>
            </div>
          </div>

          <div v-if="pendingClasses.length === 0" class="lg-card-glass p-8 flex flex-col items-center text-center">
            <div class="h-14 w-14 rounded-2xl bg-(--color-success-bg) flex items-center justify-center mb-3">
              <CheckCircle2 :size="28" class="text-(--lg-success)" />
            </div>
            <p class="text-sm font-semibold text-heading">Không có lớp chờ hủy</p>
            <p class="text-xs font-medium text-placeholder mt-1">Tất cả lớp đều đạt sĩ số tối thiểu.</p>
          </div>
        </div>
      </section>

      <section class="space-y-4 pt-4">
        <div class="flex items-center justify-between px-2">
          <h3 class="text-lg font-semibold text-label flex items-center gap-2">
            <RefreshCw :size="20" /> LỊCH SỬ LỚP ĐÃ HỦY
          </h3>
        </div>

        <div class="lg-table-shell overflow-hidden rounded-2xl">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="surface-solid">
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Lớp học phần</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Sĩ số lúc hủy</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Ngày hủy</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thông báo</th>
                <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="cls in cancelledClasses" :key="cls.id" class="group hover:bg-(--surface-input) transition-colors">
                <td class="px-4 py-4">
                   <p class="text-sm font-semibold text-heading">{{ cls.subject }}</p>
                   <p class="text-[10px] font-bold text-placeholder mt-1">{{ cls.id }} · {{ cls.teacher }}</p>
                </td>
                <td class="px-4 py-4">
                   <span class="text-sm font-bold text-label">{{ cls.enrolled }} SV</span>
                </td>
                <td class="px-4 py-4">
                   <span class="text-xs font-medium text-label">{{ cls.date }}</span>
                </td>
                <td class="px-4 py-4">
                   <div class="flex items-center gap-1.5 text-(--lg-success)">
                      <Mail :size="14" /> <span class="text-[10px] font-semibold uppercase tracking-widest">Đã gửi SV</span>
                   </div>
                </td>
                <td class="px-4 py-4 relative">
                   <div class="flex items-center gap-1">
                     <button class="p-2 hover:bg-(--color-info-bg) hover:text-link rounded-lg text-placeholder transition-all" title="Xem chi tiết" @click.stop="openDetailModal(cls)">
                       <Eye :size="16" />
                     </button>
                     <button class="p-2 hover:bg-(--color-success-bg) hover:text-(--lg-success) rounded-lg text-placeholder transition-all" title="Mở lại lớp" @click.stop="openReopenModal(cls, 'cancelled')">
                       <RotateCcw :size="16" />
                     </button>
                     <div class="relative">
                       <button class="p-2 hover:bg-(--surface-solid) rounded-lg text-placeholder transition-all" @click.stop="toggleContextMenu(cls)">
                         <MoreVertical :size="16" />
                       </button>
                       <Transition
                         enter-active-class="transition-all duration-150 ease-out"
                         enter-from-class="opacity-0 scale-95"
                         enter-to-class="opacity-100 scale-100"
                         leave-active-class="transition-all duration-100 ease-in"
                         leave-from-class="opacity-100 scale-100"
                         leave-to-class="opacity-0 scale-95"
                       >
                         <div v-if="contextTarget?.id === cls.id" class="absolute right-0 top-full mt-1 z-50 w-48 lg-glass-strong rounded-xl p-1 shadow-sm" @click.stop>
                           <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-success-bg) hover:text-(--lg-success) transition-all" @click="openReopenModal(cls, 'cancelled'); closeContextMenu()">
                             <RotateCcw :size="14" /> Mở lại lớp
                           </button>
                             <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-info-bg) hover:text-link transition-all" @click="openDetailModal(cls)">
                              <Eye :size="14" /> Xem chi tiết
                            </button>
                         </div>
                       </Transition>
                     </div>
                   </div>
                </td>
              </tr>
            </tbody>
          </table>
          <div v-if="cancelledClasses.length === 0" class="flex flex-col items-center justify-center py-12 text-center">
            <p class="text-xs font-medium text-placeholder">Chưa có lớp nào bị hủy.</p>
          </div>
        </div>
      </section>

      <div class="lg-card-glass p-4 border border-(--color-danger-bg)/50">
        <div class="flex gap-4">
          <div class="h-10 w-10 rounded-xl surface-card flex items-center justify-center text-(--lg-danger) shadow-sm border border-(--color-danger-bg)/50 shrink-0">
             <MessageSquare :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-semibold text-heading">Lưu ý khi hủy lớp</h4>
            <p class="text-xs text-label mt-2 leading-relaxed">
              Khi xác nhận hủy lớp, hệ thống sẽ tự động hoàn trả tín chỉ cho sinh viên, giải phóng phòng học và gửi thông báo qua Email/App. Đối với các sinh viên đã thanh toán học phí cho môn này, hệ thống sẽ tự động tạo <strong>Credit Note</strong> (phiếu khấu trừ) cho đợt đóng tiền tiếp theo.
            </p>
          </div>
        </div>
      </div>

    </div>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showCancelModal && cancelTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeCancelModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-danger-bg) text-(--lg-danger) flex items-center justify-center">
                <XCircle :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Xác nhận hủy lớp</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeCancelModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-4">
            <div class="h-10 w-10 rounded-full bg-(--color-danger-bg) text-(--color-danger-text) text-xs font-semibold flex items-center justify-center flex-shrink-0 border border-default">
              {{ cancelTarget.id.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-semibold text-heading">{{ cancelTarget.subject }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[10px] font-semibold text-link bg-(--color-info-bg) px-1.5 py-0.5 rounded">{{ cancelTarget.id }}</span>
                <span class="text-[11px] font-bold text-placeholder">{{ cancelTarget.teacher }}</span>
              </div>
            </div>
          </div>
          <div class="bg-(--color-danger-bg) border border-(--lg-danger)/30 rounded-2xl p-3 mb-4">
            <p class="text-[11px] font-bold text-(--lg-danger)">
              Lớp <strong>{{ cancelTarget.subject }}</strong> ({{ cancelTarget.id }}) sẽ bị hủy vĩnh viễn. 
              Sinh viên đã đăng ký ({{ cancelTarget.enrolled }} SV) sẽ được hoàn trả tín chỉ và nhận thông báo.
            </p>
          </div>
          <div>
            <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Lý do hủy (không bắt buộc)</label>
            <textarea v-model="cancelReason" rows="2" placeholder="Nhập lý do hủy lớp..." class="w-full lg-input px-4 py-2.5 text-sm resize-none"></textarea>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeCancelModal">Quay lại</button>
            <button class="px-5 py-2.5 text-sm font-bold text-white bg-(--lg-danger) hover:opacity-90 rounded-2xl transition-all flex items-center gap-2" @click="confirmCancel" :disabled="cancelling">
              <Loader2 v-if="cancelling" :size="16" class="animate-spin" />
              <XCircle v-else :size="16" />
              {{ cancelling ? 'Đang xử lý...' : 'Xác nhận hủy' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showReopenModal && reopenTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeReopenModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-success-bg) text-(--lg-success) flex items-center justify-center">
                <RotateCcw :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Mở lại lớp</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeReopenModal">
              <X :size="18" />
            </button>
          </div>
          <div class="surface-solid p-4 rounded-2xl flex items-center gap-3 mb-4">
            <div class="h-10 w-10 rounded-full bg-(--color-success-bg) text-(--color-success-text) text-xs font-semibold flex items-center justify-center flex-shrink-0 border border-default">
              {{ reopenTarget.id.slice(-2) }}
            </div>
            <div>
              <p class="text-sm font-semibold text-heading">{{ reopenTarget.subject }}</p>
              <div class="flex items-center gap-2 mt-0.5">
                <span class="text-[10px] font-semibold text-link bg-(--color-info-bg) px-1.5 py-0.5 rounded">{{ reopenTarget.id }}</span>
                <span class="text-[11px] font-bold text-placeholder">{{ reopenTarget.teacher }}</span>
              </div>
            </div>
          </div>
          <div class="bg-(--color-success-bg) border border-(--lg-success)/30 rounded-2xl p-3 mb-4">
            <p class="text-[11px] font-bold text-(--lg-success)">
              Xác nhận mở lại lớp <strong>{{ reopenTarget.subject }}</strong> ({{ reopenTarget.id }}). 
              Lớp sẽ được khôi phục và cho phép sinh viên đăng ký trở lại.
            </p>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeReopenModal">Quay lại</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2 bg-(--lg-success) hover:opacity-90" @click="confirmReopen" :disabled="reopening">
              <Loader2 v-if="reopening" :size="16" class="animate-spin" />
              <CheckCircle2 v-else :size="16" />
              {{ reopening ? 'Đang xử lý...' : 'Xác nhận mở lại' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showDetailModal && detailTarget" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDetailModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-2xl surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-6">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-xl bg-(--color-info-bg) text-link flex items-center justify-center">
                <Eye :size="20" />
              </div>
              <div>
                <h3 class="text-base font-semibold text-heading">{{ detailTarget.subject }}</h3>
                <div class="flex items-center gap-2 mt-0.5">
                  <span class="text-[10px] font-semibold text-link bg-(--color-info-bg) px-1.5 py-0.5 rounded">{{ detailTarget.id }}</span>
                  <span class="text-[11px] font-bold text-placeholder">{{ detailTarget.teacher }}</span>
                </div>
              </div>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeDetailModal">
              <X :size="18" />
            </button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <div class="surface-solid rounded-2xl p-4 text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest mb-1">Trạng thái</p>
              <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest border inline-block',
                detailTarget.status === 'cancelled' ? 'surface-solid text-placeholder border-default' : 'lg-badge-danger']">
                {{ detailTarget.status === 'cancelled' ? 'Đã hủy' : 'Chờ hủy' }}
              </span>
            </div>
            <div class="surface-solid rounded-2xl p-4 text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest mb-1">Sĩ số</p>
              <p class="text-xl font-semibold text-heading">{{ detailTarget.enrolled }}</p>
              <p class="text-[10px] font-semibold text-label">SV đã đăng ký</p>
            </div>
            <div class="surface-solid rounded-2xl p-4 text-center">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest mb-1">Min Enroll</p>
              <p class="text-xl font-semibold text-heading">{{ detailTarget.minEnroll }}</p>
              <p class="text-[10px] font-semibold text-label">Sĩ số tối thiểu</p>
            </div>
          </div>

          <div class="border-t border-default pt-5 space-y-4">
            <div v-if="detailTarget.status === 'cancelled'">
              <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-3">Thông tin hủy</h4>
              <div class="surface-solid rounded-2xl p-4 flex items-center gap-4">
                <div class="h-10 w-10 rounded-xl bg-(--color-danger-bg) text-(--lg-danger) flex items-center justify-center">
                  <XCircle :size="20" />
                </div>
                <div>
                  <p class="text-sm font-bold text-heading">Ngày hủy: {{ detailTarget.date }}</p>
                  <p class="text-xs text-label mt-0.5">Sĩ số lúc hủy: {{ detailTarget.enrolled }} SV</p>
                </div>
              </div>
            </div>
            <div v-else>
              <h4 class="text-xs font-semibold text-label uppercase tracking-widest mb-3">Lý do chờ hủy</h4>
              <div class="surface-solid rounded-2xl p-4 flex items-center gap-4">
                <div class="h-10 w-10 rounded-xl bg-(--color-warning-bg) text-(--lg-warning) flex items-center justify-center">
                  <AlertCircle :size="20" />
                </div>
                <div>
                  <p class="text-sm font-bold text-heading">{{ detailTarget.reason }}</p>
                  <p class="text-xs text-label mt-0.5">Lớp chỉ đạt {{ detailTarget.enrolled }}/{{ detailTarget.minEnroll }} sĩ số tối thiểu</p>
                </div>
              </div>
            </div>

            <div class="surface-solid rounded-2xl p-4 space-y-3">
              <h4 class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Thông tin lớp</h4>
              <div class="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span class="text-[10px] font-bold text-placeholder uppercase tracking-widest block mb-1">Mã lớp</span>
                  <span class="font-bold text-heading">{{ detailTarget.id }}</span>
                </div>
                <div>
                  <span class="text-[10px] font-bold text-placeholder uppercase tracking-widest block mb-1">Giảng viên</span>
                  <span class="font-bold text-heading">{{ detailTarget.teacher }}</span>
                </div>
              </div>
            </div>

            <div class="h-2 w-full surface-solid rounded-full overflow-hidden">
              <div class="h-full rounded-full transition-all"
                :class="detailTarget.enrolled >= detailTarget.minEnroll ? 'bg-(--lg-success)' : 'bg-(--lg-danger)'"
                :style="{ width: Math.min((detailTarget.enrolled / Math.max(detailTarget.minEnroll, 1)) * 100, 100) + '%' }"
              />
            </div>
            <div class="flex justify-between text-[10px] font-bold text-placeholder">
              <span>0</span>
              <span>{{ detailTarget.enrolled }} / {{ detailTarget.minEnroll }} SV</span>
              <span>{{ detailTarget.minEnroll }}</span>
            </div>
          </div>

          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDetailModal">Đóng</button>
            <button v-if="detailTarget.status !== 'cancelled'" class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2 bg-(--lg-success) hover:opacity-90" @click="closeDetailModal(); openReopenModal(detailTarget, 'pending')">
              <CheckCircle2 :size="16" /> Mở lại lớp
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
