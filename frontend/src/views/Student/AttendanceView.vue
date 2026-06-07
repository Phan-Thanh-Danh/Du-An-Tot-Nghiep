<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import {
  UserCheck, AlertTriangle, UserX, Clock,
  FileText, UploadCloud, X, CheckCircle2,
  Filter, Sparkles, AlertCircle,
  Info, Eye, FileSignature
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'

const popupStore = usePopupStore()

const subjects = ['Tất cả', 'Cấu trúc dữ liệu & Giải thuật', 'Toán rời rạc', 'Lập trình Web']
const filters = {
  status: ['Tất cả', 'Present', 'Absent', 'Late', 'Excused', 'Unconfirmed']
}

const mockAttendanceData = [
  { id: 1, date: new Date(2026, 4, 15, 7, 30), subject: 'Cấu trúc dữ liệu & Giải thuật', teacher: 'TS. Nguyễn Minh Khoa', status: 'Present', notes: 'Đúng giờ' },
  { id: 2, date: new Date(2026, 4, 17, 13, 30), subject: 'Toán rời rạc', teacher: 'ThS. Trần Thu Hà', status: 'Absent', notes: 'Không phép' },
  { id: 3, date: new Date(2026, 4, 20, 8, 0), subject: 'Lập trình Web', teacher: 'KS. Lê Văn Tâm', status: 'Late', notes: 'Đi trễ 15 phút' },
  { id: 4, date: new Date(2026, 4, 22, 7, 30), subject: 'Cấu trúc dữ liệu & Giải thuật', teacher: 'TS. Nguyễn Minh Khoa', status: 'Excused', notes: 'Có đơn xin phép bệnh' },
  { id: 5, date: new Date(2026, 4, 24, 13, 30), subject: 'Toán rời rạc', teacher: 'ThS. Trần Thu Hà', status: 'Unconfirmed', notes: 'Giảng viên chưa chốt' }
]

const quotas = [
  { subject: 'Cấu trúc dữ liệu & Giải thuật', absent: 2, max: 6 },
  { subject: 'Toán rời rạc', absent: 4, max: 5 },
  { subject: 'Lập trình Web', absent: 0, max: 7 }
]

const statusVariant = {
  Present: 'success',
  Absent: 'danger',
  Late: 'warning',
  Excused: 'primary',
  Unconfirmed: 'neutral'
}

const statusConfig = {
  Present: { label: 'Có mặt', icon: CheckCircle2 },
  Absent: { label: 'Vắng mặt', icon: UserX },
  Late: { label: 'Đi muộn', icon: Clock },
  Excused: { label: 'Có phép', icon: FileText },
  Unconfirmed: { label: 'Chưa xác nhận', icon: Info }
}

const metrics = [
  { label: 'Tỷ lệ có mặt', value: '85', unit: '%', icon: UserCheck, tone: 'success', hint: 'Trên mức an toàn' },
  { label: 'Tổng vắng', value: '2', unit: 'buổi', icon: UserX, tone: 'danger', hint: '1 không phép, 1 có phép' },
  { label: 'Đi muộn', value: '1', unit: 'lần', icon: Clock, tone: 'warning', hint: 'Cần chú ý đi học đúng giờ' },
  { label: 'Chưa xác nhận', value: '1', unit: 'buổi', icon: AlertCircle, tone: 'info', hint: 'Đang chờ giảng viên cập nhật' },
]

const selectedSubject = ref('Tất cả')
const selectedStatus = ref('Tất cả')
const drawerOpen = ref(false)
const modalOpen = ref(false)
const anyOverlayOpen = computed(() => drawerOpen.value || modalOpen.value)
useBodyScrollLock(anyOverlayOpen)
const selectedSession = ref(null)

const excuseForm = ref({
  reason: '',
  file: null
})

const filteredData = computed(() => {
  return mockAttendanceData.filter(item => {
    const matchSubject = selectedSubject.value === 'Tất cả' || item.subject === selectedSubject.value
    const matchStatus = selectedStatus.value === 'Tất cả' || item.status === selectedStatus.value
    return matchSubject && matchStatus
  })
})

const getAiRisk = computed(() => {
  const highRisk = quotas.find(q => (q.absent / q.max) >= 0.8)
  if (highRisk) {
    return {
      risk: 'high',
      title: 'Cảnh báo rủi ro cao từ AI',
      message: `Môn ${highRisk.subject} đã vắng ${highRisk.absent}/${highRisk.max} buổi. Nếu vắng thêm ${highRisk.max - highRisk.absent} buổi nữa sẽ bị cấm thi. Hãy chú ý đi học đầy đủ!`
    }
  }
  const mediumRisk = quotas.find(q => (q.absent / q.max) >= 0.6)
  if (mediumRisk) {
    return {
      risk: 'medium',
      title: 'Dự đoán AI: Cần lưu ý',
      message: `Môn ${mediumRisk.subject} đã vắng ${mediumRisk.absent}/${mediumRisk.max} buổi. Bạn đang ở ngưỡng 60% quỹ vắng.`
    }
  }
  return {
    risk: 'low',
    title: 'Dự đoán AI: An toàn',
    message: 'Thói quen đi học của bạn đang rất tốt, không có nguy cơ vi phạm quỹ vắng ở các môn học hiện tại.'
  }
})

const fmtDateTime = (date) => {
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  }).format(date)
}

const getQuotaStyle = (absent, max) => {
  const percent = absent / max
  if (percent >= 0.8) return { background: 'var(--color-danger-text)' }
  if (percent >= 0.6) return { background: 'var(--color-warning-text)' }
  return { background: 'var(--accent-primary)' }
}

const getRiskStyle = (risk) => {
  if (risk === 'high') return { background: 'var(--color-danger-bg)', borderColor: 'var(--color-danger-text)', color: 'var(--color-danger-text)' }
  if (risk === 'medium') return { background: 'var(--color-warning-bg)', borderColor: 'var(--color-warning-text)', color: 'var(--color-warning-text)' }
  return { background: 'var(--color-info-bg)', borderColor: 'var(--color-info-text)', color: 'var(--color-info-text)' }
}

const getMetricIconStyle = (tone) => {
  const map = {
    success: { bg: 'var(--color-success-bg)', color: 'var(--color-success-text)' },
    danger: { bg: 'var(--color-danger-bg)', color: 'var(--color-danger-text)' },
    warning: { bg: 'var(--color-warning-bg)', color: 'var(--color-warning-text)' },
    info: { bg: 'var(--color-info-bg)', color: 'var(--color-info-text)' },
  }
  return map[tone] || map.info
}

const openDrawer = (session) => {
  selectedSession.value = session
  drawerOpen.value = true
}

const openExcuseModal = (session) => {
  selectedSession.value = session
  excuseForm.value.reason = ''
  excuseForm.value.file = null
  drawerOpen.value = false
  modalOpen.value = true
}

const closeDrawer = () => drawerOpen.value = false
const closeModal = () => modalOpen.value = false

const handleFileChange = (e) => {
  excuseForm.value.file = e.target.files[0]
}

const submitExcuse = () => {
  if (!excuseForm.value.reason) return
  popupStore.success('Đã gửi giải trình', `Giải trình cho môn ${selectedSession.value.subject} đã được gửi.`)
  modalOpen.value = false
}
</script>

<template>
  <div class="lg-page-enter space-y-4 pb-6">

    <!-- Header -->
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <div class="flex items-center gap-1.5 text-xs font-bold text-link uppercase tracking-wider mb-1">
          <UserCheck :size="14" /> Quản lý học vụ
        </div>
        <h1 class="text-xl font-extrabold text-heading tracking-tight">Điểm danh</h1>
        <p class="text-sm text-muted mt-0.5">Theo dõi lịch sử chuyên cần, nộp đơn giải trình và nhận cảnh báo từ AI.</p>
      </div>
    </div>

    <!-- AI Risk Banner -->
    <div class="flex items-start gap-3 rounded-xl border p-3.5" :style="getRiskStyle(getAiRisk.risk)">
      <Sparkles v-if="getAiRisk.risk === 'low'" :size="20" class="shrink-0 mt-0.5" />
      <AlertTriangle v-else :size="20" class="shrink-0 mt-0.5" />
      <div>
        <h3 class="text-sm font-bold" :style="{ color: 'inherit' }">{{ getAiRisk.title }}</h3>
        <p class="text-xs mt-0.5" :style="{ color: 'inherit', opacity: 0.85 }">{{ getAiRisk.message }}</p>
      </div>
    </div>

    <!-- Quota Bars -->
    <div class="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
      <div v-for="q in quotas" :key="q.subject" class="surface-card border-card rounded-xl p-3.5">
        <div class="flex items-center justify-between text-xs font-semibold mb-2">
          <span class="text-body">{{ q.subject }}</span>
          <span class="text-muted">{{ q.absent }}/{{ q.max }} vắng</span>
        </div>
        <div class="h-1.5 overflow-hidden rounded-full bg-surface-input">
          <div class="h-full rounded-full transition-all" :style="{ ...getQuotaStyle(q.absent, q.max), width: `${Math.min((q.absent / q.max) * 100, 100)}%` }"></div>
        </div>
      </div>
    </div>

    <!-- Metrics Grid -->
    <div class="grid gap-3 grid-cols-2 xl:grid-cols-4">
      <div v-for="m in metrics" :key="m.label" class="flex items-center gap-3 surface-card border-card rounded-xl p-3.5">
        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg" :style="getMetricIconStyle(m.tone)">
          <component :is="m.icon" :size="18" />
        </div>
        <div>
          <div class="text-lg font-extrabold text-heading leading-none">{{ m.value }}<span class="text-xs font-semibold text-muted ml-1">{{ m.unit }}</span></div>
          <div class="text-xs font-semibold text-label mt-1">{{ m.label }}</div>
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="flex items-center gap-2 surface-card border-card rounded-lg p-2">
      <Filter :size="15" class="text-muted ml-1 shrink-0" />
      <select v-model="selectedSubject" class="flex-1 min-w-0 rounded-md border border-input bg-surface-input px-2.5 py-1.5 text-xs text-heading outline-none focus:border-link">
        <option v-for="s in subjects" :key="s" :value="s">{{ s }}</option>
      </select>
      <select v-model="selectedStatus" class="flex-1 min-w-0 rounded-md border border-input bg-surface-input px-2.5 py-1.5 text-xs text-heading outline-none focus:border-link">
        <option v-for="st in filters.status" :key="st" :value="st">{{ statusConfig[st]?.label || st }}</option>
      </select>
    </div>

    <!-- Table -->
    <TableShell density="compact">
      <table class="w-full border-collapse">
        <thead>
          <tr class="border-b border-card">
            <th class="text-left py-2.5 px-3 text-xs font-bold text-label">Thời gian</th>
            <th class="text-left py-2.5 px-3 text-xs font-bold text-label">Môn học</th>
            <th class="text-left py-2.5 px-3 text-xs font-bold text-label">Giảng viên</th>
            <th class="text-left py-2.5 px-3 text-xs font-bold text-label">Trạng thái</th>
            <th class="text-right py-2.5 px-3 text-xs font-bold text-label">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredData" :key="item.id" class="border-b border-card transition-colors hover:bg-surface-table-row-hover">
            <td class="py-2.5 px-3 text-sm font-medium text-heading">{{ fmtDateTime(item.date) }}</td>
            <td class="py-2.5 px-3 text-sm text-body">{{ item.subject }}</td>
            <td class="py-2.5 px-3 text-sm text-muted">{{ item.teacher }}</td>
            <td class="py-2.5 px-3">
              <GlassBadge :variant="statusVariant[item.status] || 'neutral'" size="sm">
                <template #default>
                  <component :is="statusConfig[item.status]?.icon" :size="11" />
                  {{ statusConfig[item.status]?.label }}
                </template>
              </GlassBadge>
            </td>
            <td class="py-2.5 px-3 text-right">
              <GlassButton variant="ghost" size="sm" @click="openDrawer(item)">
                <template #leading><Eye :size="14" /></template>
                Chi tiết
              </GlassButton>
            </td>
          </tr>
          <tr v-if="filteredData.length === 0">
            <td colspan="5" class="py-10 text-center text-sm text-muted italic">Không tìm thấy dữ liệu điểm danh phù hợp.</td>
          </tr>
        </tbody>
      </table>
    </TableShell>

    <!-- Drawer -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="drawerOpen" class="attendance-overlay fixed inset-0 z-50 flex justify-end backdrop-blur-sm" @click.self="closeDrawer">
          <Transition name="slide-right">
            <div v-if="selectedSession" class="attendance-drawer flex h-full w-full max-w-md flex-col bg-surface-modal border-l border-card">
              <div class="flex items-start justify-between gap-4 border-b border-card p-4">
                <div>
                  <GlassBadge :variant="statusVariant[selectedSession.status] || 'neutral'" size="md">
                    <component :is="statusConfig[selectedSession.status]?.icon" :size="13" />
                    {{ statusConfig[selectedSession.status]?.label }}
                  </GlassBadge>
                  <h2 class="text-base font-extrabold text-heading mt-2">{{ selectedSession.subject }}</h2>
                </div>
                <button class="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg border border-card bg-surface-input text-muted" @click="closeDrawer"><X :size="18"/></button>
              </div>
              <div class="flex-1 space-y-4 overflow-y-auto p-4">
                <div class="flex items-center gap-3 text-sm text-body">
                  <Clock :size="16" class="text-muted shrink-0" />
                  <span>{{ fmtDateTime(selectedSession.date) }}</span>
                </div>
                <div class="flex items-center gap-3 text-sm text-body">
                  <UserCheck :size="16" class="text-muted shrink-0" />
                  <span>{{ selectedSession.teacher }}</span>
                </div>
                <div class="rounded-lg border border-card bg-surface-input p-3.5">
                  <h4 class="text-xs font-semibold text-muted mb-1.5">Ghi chú từ hệ thống/giảng viên:</h4>
                  <p class="text-sm text-body">{{ selectedSession.notes || 'Không có ghi chú.' }}</p>
                </div>
                <div v-if="['Absent', 'Late', 'Unconfirmed'].includes(selectedSession.status)" class="rounded-lg border border-card p-4 text-center" style="background:var(--accent-primary-soft);border-color:color-mix(in srgb, var(--accent-primary) 30%, transparent)">
                  <FileSignature :size="22" style="color:var(--accent-primary)" class="mx-auto mb-2" />
                  <h4 class="text-sm font-bold" style="color:var(--accent-primary)">Cần giải trình?</h4>
                  <p class="text-xs mt-1" style="color:var(--accent-primary);opacity:0.8">Nếu bạn có lý do chính đáng, hãy nộp minh chứng để được xem xét chuyển trạng thái thành "Có phép".</p>
                  <GlassButton variant="primary" size="sm" class="mt-3 w-full justify-center" @click="openExcuseModal(selectedSession)">
                    Nộp đơn giải trình
                  </GlassButton>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>

    <!-- Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalOpen" class="attendance-overlay modal fixed inset-0 z-50 flex items-center justify-center backdrop-blur-sm p-4" @click.self="closeModal">
          <div class="attendance-modal relative w-full max-w-md overflow-hidden rounded-2xl border border-card bg-surface-modal">
            <div class="flex items-center justify-between border-b border-card px-4 py-3.5">
              <h3 class="text-sm font-bold text-heading">Giải trình vắng mặt</h3>
              <button class="flex h-7 w-7 items-center justify-center rounded-lg border border-card bg-surface-input text-muted" @click="closeModal"><X :size="16"/></button>
            </div>
            <div class="space-y-4 p-4">
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Môn học</label>
                <input type="text" :value="selectedSession?.subject" disabled class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-xs text-muted" />
              </div>
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Lý do giải trình <span style="color:var(--color-danger-text)">*</span></label>
                <textarea v-model="excuseForm.reason" rows="4" class="w-full rounded-lg border border-input bg-surface-input px-3 py-2 text-sm text-heading outline-none placeholder:text-placeholder focus:border-link" placeholder="Nhập chi tiết lý do vắng mặt..."></textarea>
              </div>
              <div>
                <label class="mb-1 block text-xs font-semibold text-label">Minh chứng đính kèm (Hình ảnh, PDF)</label>
                <label class="flex cursor-pointer flex-col items-center gap-2 rounded-lg border-2 border-dashed border-input bg-surface-input px-4 py-5 text-center transition-colors hover:bg-surface-card-hover">
                  <input type="file" class="hidden" @change="handleFileChange" accept="image/*,.pdf" />
                  <UploadCloud :size="22" class="text-placeholder" />
                  <span v-if="!excuseForm.file" class="text-xs text-muted">Nhấn để chọn file tải lên</span>
                  <span v-else class="text-xs font-semibold text-link">{{ excuseForm.file.name }}</span>
                </label>
              </div>
            </div>
            <div class="flex items-center justify-end gap-2 border-t border-card bg-surface-input px-4 py-3">
              <GlassButton variant="secondary" size="sm" @click="closeModal">Hủy</GlassButton>
              <GlassButton variant="primary" size="sm" :disabled="!excuseForm.reason" @click="submitExcuse">Gửi yêu cầu</GlassButton>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<style scoped>
.attendance-overlay {
  background: color-mix(in srgb, var(--lg-bg-mid) 42%, transparent);
}

.attendance-overlay.modal {
  background: color-mix(in srgb, var(--lg-bg-mid) 54%, transparent);
}

.attendance-drawer {
  box-shadow: var(--lg-shadow-lg);
}

.attendance-modal {
  box-shadow: var(--lg-shadow-lg);
}

/* Transitions */
.fade-enter-active, .fade-leave-active { transition: opacity .25s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-right-enter-active, .slide-right-leave-active { transition: transform .3s cubic-bezier(0.16,1,.3,1); }
.slide-right-enter-from, .slide-right-leave-to { transform: translateX(100%); }
.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }
</style>
