<script setup>
import { ref, computed } from 'vue'
import {
  CheckCircle2, XCircle, Eye, X, Search, CalendarDays
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()

const schedules = ref([
  { id: 'PD-241', term: 'Học kỳ 1 - 2026', type: 'Chính quy', department: 'Khoa CNTT', created: '10/05/2026', submitter: 'Nguyễn Văn A (Giáo vụ)', status: 'pending', metrics: { classes: 120, teachers: 45, hours: 360 } },
  { id: 'PD-242', term: 'Học kỳ 1 - 2026', type: 'Chính quy', department: 'Khoa Kinh tế & QT', created: '11/05/2026', submitter: 'Trần Thị B (Giáo vụ)', status: 'pending', metrics: { classes: 80, teachers: 28, hours: 215 } },
  { id: 'PD-243', term: 'Học kỳ 2 - 2026', type: 'Chất lượng cao', department: 'Khoa Ngôn ngữ Anh', created: '12/05/2026', submitter: 'Lê Văn C (Giáo vụ)', status: 'pending', metrics: { classes: 65, teachers: 22, hours: 180 } },
])

const searchQuery = ref('')
const selectedItem = ref(null)
const confirmDialog = ref({ isOpen: false, action: null, message: '', item: null })

const filteredSchedules = computed(() => {
  let list = schedules.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.department.toLowerCase().includes(q) || s.id.toLowerCase().includes(q) || s.submitter.toLowerCase().includes(q))
  }
  return list
})

const pendingCount = computed(() => schedules.value.filter(s => s.status === 'pending').length)

function openConfirm(action, item) {
  selectedItem.value = item
  const msg = action === 'approve' ? `Phê duyệt thời khóa biểu "${item.id}" - ${item.department}?` : `Trả về "${item.id}" - ${item.department} yêu cầu chỉnh sửa?`
  confirmDialog.value = { isOpen: true, action, message: msg, item }
}

function handleConfirm() {
  const { item, action } = confirmDialog.value
  if (item && action) {
    const idx = schedules.value.findIndex(s => s.id === item.id)
    if (idx !== -1) {
      if (action === 'approve') {
        schedules.value[idx] = { ...schedules.value[idx], status: 'approved' }
        popup.success('Đã duyệt', `Thời khóa biểu "${item.id}" đã được phê duyệt.`)
      } else {
        schedules.value[idx] = { ...schedules.value[idx], status: 'rejected' }
        popup.info('Đã trả về', `Thời khóa biểu "${item.id}" đã được trả về giáo vụ.`)
      }
    }
  }
  confirmDialog.value = { isOpen: false, action: null, message: '', item: null }
  selectedItem.value = null
}

function restoreItem(item) {
  const idx = schedules.value.findIndex(s => s.id === item.id)
  if (idx !== -1) {
    schedules.value[idx] = { ...schedules.value[idx], status: 'pending' }
  }
}
</script>

<template>
  <div class="flex flex-1 min-h-0 gap-4 flex-col lg:flex-row">
    <div class="flex-1 surface-card border border-card rounded-2xl p-4 flex flex-col gap-3 min-w-0 overflow-y-auto">
      <div v-for="item in filteredSchedules" :key="item.id"
           @click="selectedItem = item"
           class="surface-card border rounded-2xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden group flex flex-col lg:flex-row lg:items-center gap-4"
           :class="selectedItem?.id === item.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary) bg-(--lg-primary)/5' : item.status === 'approved' ? 'border-(--color-success-text)/30' : item.status === 'rejected' ? 'border-(--color-danger-text)/30' : 'border-card'">

        <div :class="['absolute left-0 top-0 bottom-0 w-1', item.status === 'pending' ? 'bg-amber-500' : item.status === 'approved' ? 'bg-(--color-success-text)' : 'bg-(--color-danger-text)']"></div>

        <div class="flex-1 pl-2">
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-mono font-bold text-muted">{{ item.id }}</span>
            <GlassBadge v-if="item.status === 'pending'" variant="warning" size="xs">Chờ duyệt</GlassBadge>
            <GlassBadge v-else-if="item.status === 'approved'" variant="success" size="xs">Đã duyệt</GlassBadge>
            <GlassBadge v-else variant="danger" size="xs">Đã trả về</GlassBadge>
          </div>
          <h3 class="font-bold text-heading text-base">{{ item.department }}</h3>
          <p class="text-sm text-body font-medium">{{ item.term }} — {{ item.type }}</p>
        </div>

        <div class="flex items-center gap-4 py-2 lg:py-0 border-y lg:border-y-0 lg:border-l border-default lg:px-4 shrink-0">
           <div class="text-center">
              <p class="text-[10px] uppercase text-muted font-bold">Lớp</p>
              <p class="font-bold text-heading">{{ item.metrics.classes }}</p>
           </div>
           <div class="text-center">
              <p class="text-[10px] uppercase text-muted font-bold">GV</p>
              <p class="font-bold text-heading">{{ item.metrics.teachers }}</p>
           </div>
           <div class="text-center">
              <p class="text-[10px] uppercase text-muted font-bold">Giờ</p>
              <p class="font-bold text-heading">{{ item.metrics.hours }}</p>
           </div>
        </div>

        <div v-if="item.status === 'pending'" class="flex flex-wrap lg:flex-nowrap gap-2 shrink-0 lg:pl-2" @click.stop>
           <GlassButton variant="primary" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('approve', item)"><CheckCircle2 :size="14" class="mr-1"/>Duyệt</GlassButton>
           <GlassButton variant="danger" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('reject', item)"><XCircle :size="14" class="mr-1"/>Trả về</GlassButton>
        </div>
        <div v-else class="flex gap-2 shrink-0 lg:pl-2" @click.stop>
           <GlassButton variant="secondary" size="sm" @click="restoreItem(item)">Khôi phục</GlassButton>
        </div>
      </div>

      <div v-if="filteredSchedules.length === 0" class="flex-1 flex flex-col items-center justify-center text-muted p-8">
        <CheckCircle2 :size="48" class="text-emerald-500/50 mb-4" />
        <p class="font-bold text-heading">Không có dữ liệu chờ duyệt</p>
      </div>
    </div>

    <div v-if="selectedItem" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
      <div class="surface-card border border-card rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
        <div class="p-4 flex justify-between items-center bg-(--surface-input)">
          <h3 class="font-bold text-heading">Chi tiết TKB</h3>
          <button class="text-muted hover:text-heading" @click="selectedItem = null"><X :size="16" /></button>
        </div>

        <div class="p-4 flex-1 overflow-auto space-y-5">
          <div>
            <p class="text-xs text-muted uppercase tracking-wider font-bold mb-1">Thông tin chung</p>
            <div class="space-y-2 text-sm text-body">
              <div class="flex justify-between"><span class="text-muted">Mã duyệt:</span> <span class="font-medium text-right">{{ selectedItem.id }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Học kỳ:</span> <span class="font-medium text-right">{{ selectedItem.term }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Đơn vị:</span> <span class="font-medium text-right">{{ selectedItem.department }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Người nộp:</span> <span class="font-medium text-right">{{ selectedItem.submitter }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Ngày nộp:</span> <span class="font-medium text-right">{{ selectedItem.created }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Trạng thái:</span>
                <GlassBadge v-if="selectedItem.status === 'pending'" variant="warning" size="xs">Chờ duyệt</GlassBadge>
                <GlassBadge v-else-if="selectedItem.status === 'approved'" variant="success" size="xs">Đã duyệt</GlassBadge>
                <GlassBadge v-else variant="danger" size="xs">Đã trả về</GlassBadge>
              </div>
            </div>
          </div>

          <div class="p-3 bg-(--surface-input) rounded-xl border border-default">
            <p class="text-xs font-bold text-heading mb-2">Kiểm tra xung đột</p>
            <div class="flex items-center gap-2 text-emerald-600 dark:text-emerald-400 text-sm font-medium">
              <CheckCircle2 :size="16" /> Hoàn toàn hợp lệ (0 xung đột)
            </div>
          </div>

          <div class="space-y-2 text-sm">
            <p class="text-xs font-bold text-heading mb-1">Quy mô</p>
            <div class="flex justify-between"><span class="text-muted">Lớp học phần:</span> <span class="font-medium">{{ selectedItem.metrics.classes }}</span></div>
            <div class="flex justify-between"><span class="text-muted">Giảng viên:</span> <span class="font-medium">{{ selectedItem.metrics.teachers }}</span></div>
            <div class="flex justify-between"><span class="text-muted">Tổng giờ:</span> <span class="font-medium">{{ selectedItem.metrics.hours }}h</span></div>
          </div>

          <GlassButton variant="secondary" class="w-full justify-center text-link"><Eye :size="14" class="mr-1"/> Xem dữ liệu chi tiết</GlassButton>
        </div>
      </div>
    </div>

    <div v-else class="w-full lg:w-80 shrink-0 surface-card border border-card rounded-2xl flex flex-col items-center justify-center p-8 text-center text-muted">
      <CalendarDays :size="40" class="text-placeholder mb-3" />
      <p class="text-sm font-semibold text-heading">Chọn một TKB</p>
      <p class="text-xs mt-1">Click vào một thời khóa biểu để xem chi tiết</p>
    </div>

    <ConfirmActionDialog
      :is-open="confirmDialog.isOpen"
      :title="confirmDialog.action === 'approve' ? 'Xác nhận phê duyệt' : 'Xác nhận trả về'"
      :message="confirmDialog.message"
      :variant="confirmDialog.action === 'approve' ? 'primary' : 'danger'"
      confirm-text="Đồng ý"
      @confirm="handleConfirm"
      @cancel="confirmDialog.isOpen = false"
    />
  </div>
</template>
