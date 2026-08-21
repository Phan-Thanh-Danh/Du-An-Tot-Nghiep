<script setup>
import { ref, computed, nextTick, onMounted } from 'vue'
import {
  CheckCircle2, XCircle, Eye, X, CalendarDays, AlertCircle, Loader2, Printer, User, MapPin, Building2, Clock, Users
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const popup = usePopupStore()

const loading = ref(false)
const error = ref(null)
const schedules = ref([])
const searchQuery = ref('')
const selectedItem = ref(null)
const detailPanel = ref(null)
const confirmDialog = ref({ isOpen: false, action: null, message: '', item: null })
const showScheduleModal = ref(false)

const daysOfWeek = ['Thứ 2', 'Thứ 3', 'Thứ 4', 'Thứ 5', 'Thứ 6', 'Thứ 7', 'Chủ Nhật']
const shifts = computed(() => {
  const unique = new Map()
  schedules.value.forEach(item => {
    if (!item.shiftId) return
    unique.set(Number(item.shiftId), {
      id: Number(item.shiftId),
      name: item.shift,
      time: [item.shiftStart?.slice(0, 5), item.shiftEnd?.slice(0, 5)].filter(Boolean).join(' - '),
    })
  })
  return [...unique.values()].sort((a, b) => a.id - b.id)
})

const filteredSchedules = computed(() => {
  let list = schedules.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.department?.toLowerCase().includes(q) || s.id?.toLowerCase().includes(q) || s.submitter?.toLowerCase().includes(q))
  }
  return list
})

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getPendingSchedules()
    const rawList = unwrapApiData(res) || []
    schedules.value = rawList.map((item) => ({
      ...item,
      status: item.status || 'pending',
      classes: Number(item.classes ?? item.metrics?.classes ?? 0),
      teachers: Number(item.teachers ?? item.metrics?.teachers ?? 0),
      hours: Number(item.hours ?? item.metrics?.hours ?? 0),
    })).filter(item => item.status === 'pending')
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu TKB chờ duyệt'
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })

function openConfirm(action, item) {
  selectedItem.value = item
  const msg = action === 'approve' ? `Phê duyệt thời khóa biểu "${item.id}" - ${item.department}?` : `Trả về "${item.id}" - ${item.department} yêu cầu chỉnh sửa?`
  confirmDialog.value = { isOpen: true, action, message: msg, item }
}

function formatHours(value) {
  const hours = Number(value)
  if (!Number.isFinite(hours)) return '0'
  return Number.isInteger(hours) ? String(hours) : hours.toFixed(1)
}

async function selectSchedule(item) {
  selectedItem.value = item
  await nextTick()
  detailPanel.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

async function handleConfirm() {
  const { item, action } = confirmDialog.value
  if (item && action) {
    const idx = schedules.value.findIndex(s => s.id === item.id)
    if (idx !== -1) {
      try {
        if (action === 'approve') {
          await bghApi.approveSchedule(item.id)
          popup.success('Đã duyệt', `Thời khóa biểu "${item.id}" đã được phê duyệt thành công.`)
        } else {
          await bghApi.rejectSchedule(item.id)
          popup.info('Đã trả về', `Thời khóa biểu "${item.id}" đã được trả về giáo vụ.`)
        }

        schedules.value.splice(idx, 1)
        if (selectedItem.value?.id === item.id) selectedItem.value = null
      } catch (e) {
        popup.error('Lỗi xử lý', e?.message || 'Không thể lưu trạng thái thời khóa biểu.')
      }
    }
  }
  confirmDialog.value = { isOpen: false, action: null, message: '', item: null }
}

function openScheduleModal(item) {
  selectedItem.value = item
  showScheduleModal.value = true
}

function getGridItem(dayIdx, shiftId) {
  if (!selectedItem.value) return null
  const dayOfWeek = dayIdx + 2
  if (Number(selectedItem.value.thuTrongTuan) !== dayOfWeek || Number(selectedItem.value.shiftId) !== Number(shiftId)) return null
  return {
    subject: selectedItem.value.subject || 'Chưa có tên môn học',
    code: selectedItem.value.classCode || selectedItem.value.id,
    teacher: selectedItem.value.submitter || 'Chưa phân công',
    room: selectedItem.value.room || 'Chưa xếp phòng',
    color: 'bg-blue-500/10 border-blue-500/30 text-blue-700'
  }
}
</script>

<template>
  <div class="flex flex-1 min-h-0 gap-4 flex-col lg:flex-row">
    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin" />
        <p class="text-sm font-medium">Đang tải dữ liệu...</p>
      </div>
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div class="flex-1 surface-card border border-card rounded-2xl p-4 flex flex-col gap-3 min-w-0 overflow-y-auto">
      <div v-for="item in filteredSchedules" :key="item.id"
           @click="selectSchedule(item)"
           class="surface-card border rounded-2xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden group flex flex-col lg:flex-row lg:items-center gap-4"
           :class="selectedItem?.id === item.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary) bg-(--lg-primary)/5' : item.status === 'approved' ? 'border-(--color-success-text)/30' : item.status === 'rejected' ? 'border-(--color-danger-text)/30' : 'border-card'">

        <div :class="['absolute left-0 top-0 bottom-0 w-1', item.status === 'pending' ? 'bg-amber-500' : item.status === 'approved' ? 'bg-(--color-success-text)' : 'bg-(--color-danger-text)']"></div>

        <div class="flex-1 pl-2">
          <div class="flex items-center gap-2 mb-1">
            <span class="text-xs font-mono font-bold text-muted">{{ item.id }}</span>
            <GlassBadge v-if="item.status === 'pending'" variant="warning" size="sm">Chờ duyệt</GlassBadge>
            <GlassBadge v-else-if="item.status === 'approved'" variant="success" size="sm">Đã duyệt</GlassBadge>
            <GlassBadge v-else variant="danger" size="sm">Đã trả về</GlassBadge>
          </div>
          <h3 class="font-bold text-heading text-base">{{ item.department }}</h3>
          <p class="text-sm text-body font-medium">{{ item.term }} — {{ item.type }}</p>
        </div>

        <div class="flex items-center gap-4 py-2 lg:py-0 border-y lg:border-y-0 lg:border-l border-default lg:px-4 shrink-0">
           <div class="text-center">
             <p class="text-[10px] uppercase text-muted font-bold">Lớp HP</p>
             <p class="font-bold text-heading text-sm">{{ item.classes }}</p>
           </div>
           <div class="text-center">
             <p class="text-[10px] uppercase text-muted font-bold">GV</p>
             <p class="font-bold text-heading text-sm">{{ item.teachers }}</p>
           </div>
           <div class="text-center">
             <p class="text-[10px] uppercase text-muted font-bold">Tổng giờ</p>
             <p class="font-bold text-heading text-sm">{{ formatHours(item.hours) }} Giờ</p>
           </div>
        </div>

        <div v-if="item.status === 'pending'" class="flex flex-wrap lg:flex-nowrap gap-2 shrink-0 lg:pl-2" @click.stop>
           <GlassButton variant="primary" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('approve', item)"><CheckCircle2 :size="14" class="mr-1"/>Duyệt</GlassButton>
           <GlassButton variant="danger" size="sm" class="flex-1 lg:flex-none justify-center" @click="openConfirm('reject', item)"><XCircle :size="14" class="mr-1"/>Trả về</GlassButton>
        </div>
      </div>

      <div v-if="filteredSchedules.length === 0" class="flex-1 flex flex-col items-center justify-center text-muted p-8">
        <CheckCircle2 :size="48" class="text-emerald-500/50 mb-4" />
        <p class="font-bold text-heading">Không có dữ liệu chờ duyệt</p>
      </div>
    </div>

    <!-- Side Details Panel -->
    <div v-if="selectedItem" ref="detailPanel" class="w-full lg:w-80 shrink-0 flex flex-col gap-3 lg:sticky lg:top-4 lg:self-start">
      <div class="surface-card border border-card rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
        <div class="p-4 flex justify-between items-center bg-(--surface-input)">
          <h3 class="font-bold text-heading">Chi tiết TKB</h3>
          <button class="text-muted hover:text-heading" @click="selectedItem = null"><X :size="16" /></button>
        </div>

        <div class="p-4 flex-1 overflow-auto space-y-5">
          <div>
            <p class="text-xs text-muted uppercase tracking-wider font-bold mb-2">Thông tin chung</p>
            <div class="space-y-2 text-xs text-body">
              <div class="flex justify-between"><span class="text-muted">Mã duyệt:</span> <span class="font-mono font-bold text-heading">{{ selectedItem.id }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Học kỳ:</span> <span class="font-semibold text-heading">{{ selectedItem.term }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Đơn vị:</span> <span class="font-semibold text-heading">{{ selectedItem.department }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Người nộp:</span> <span class="font-semibold text-heading">{{ selectedItem.submitter }}</span></div>
              <div class="flex justify-between"><span class="text-muted">Ngày nộp:</span> <span class="font-semibold text-heading">{{ selectedItem.created }}</span></div>
              <div class="flex justify-between items-center pt-1"><span class="text-muted">Trạng thái:</span>
                <GlassBadge v-if="selectedItem.status === 'pending'" variant="warning" size="sm">Chờ duyệt</GlassBadge>
                <GlassBadge v-else-if="selectedItem.status === 'approved'" variant="success" size="sm">Đã duyệt</GlassBadge>
                <GlassBadge v-else variant="danger" size="sm">Đã trả về</GlassBadge>
              </div>
            </div>
          </div>

          <div :class="selectedItem.conflicts > 0 ? 'bg-(--color-danger-bg) border-(--color-danger-text)/30 shadow-sm' : 'bg-(--color-success-bg) border-(--color-success-text)/30 shadow-sm'" class="p-3.5 rounded-xl border transition-all">
            <div class="flex items-center justify-between mb-1">
              <p :class="selectedItem.conflicts > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)'" class="text-xs font-extrabold uppercase tracking-wider">
                Kiểm tra xung đột hệ thống
              </p>
              <GlassBadge v-if="selectedItem.conflicts > 0" variant="danger" size="sm">CẢNH BÁO</GlassBadge>
              <GlassBadge v-else variant="success" size="sm">HỢP LỆ</GlassBadge>
            </div>
            <div :class="selectedItem.conflicts > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)'" class="flex items-start gap-2 text-xs font-bold mt-1.5">
              <CheckCircle2 v-if="selectedItem.conflicts === 0" :size="16" class="shrink-0 mt-0.5" />
              <AlertCircle v-else :size="16" class="shrink-0 mt-0.5" />
              <div>
                <span>{{ selectedItem.conflicts === 0 ? 'Hoàn toàn hợp lệ (0 xung đột phòng học / giảng viên)' : `Phát hiện ${selectedItem.conflicts} xung đột xếp lịch!` }}</span>
                <p v-if="selectedItem.conflicts > 0" class="text-[11px] text-body font-medium mt-1 leading-relaxed">Vui lòng kiểm tra lại phòng học trùng ca hoặc giảng viên trùng giờ trước khi bấm Duyệt.</p>
              </div>
            </div>
          </div>

          <div class="space-y-2 text-xs">
            <p class="text-xs font-bold text-heading mb-1 uppercase tracking-wider">Quy mô đào tạo</p>
            <div class="flex justify-between py-1 border-b border-default"><span class="text-muted">Lớp học phần:</span> <span class="font-bold text-heading">{{ selectedItem.classes }} Lớp</span></div>
            <div class="flex justify-between py-1 border-b border-default"><span class="text-muted">Giảng viên:</span> <span class="font-bold text-heading">{{ selectedItem.teachers }} GV</span></div>
            <div class="flex justify-between py-1"><span class="text-muted">Tổng giờ giảng dạy:</span> <span class="font-bold text-heading">{{ formatHours(selectedItem.hours) }} Giờ</span></div>
          </div>

          <GlassButton variant="primary" class="w-full justify-center text-xs font-bold py-2.5" @click="openScheduleModal(selectedItem)">
            <Eye :size="14" class="mr-1.5"/> Xem dữ liệu chi tiết
          </GlassButton>
        </div>
      </div>
    </div>

    <div v-else class="w-full lg:w-80 shrink-0 surface-card border border-card rounded-2xl flex flex-col items-center justify-center p-8 text-center text-muted">
      <CalendarDays :size="40" class="text-placeholder mb-3" />
      <p class="text-sm font-semibold text-heading">Chọn một TKB</p>
      <p class="text-xs mt-1">Click vào một thời khóa biểu để xem chi tiết</p>
    </div>

    <!-- FULL WEEKLY SCHEDULE MODAL VIEW -->
    <div v-if="showScheduleModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="surface-card border border-card rounded-3xl w-full max-w-5xl max-h-[90vh] flex flex-col shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">

        <!-- Modal Header -->
        <div class="p-5 border-b border-default flex items-center justify-between bg-surface-card">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-2xl bg-blue-500/10 text-blue-600 flex items-center justify-center border border-blue-500/20">
              <CalendarDays :size="20" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h2 class="text-lg font-bold text-heading">{{ selectedItem?.department }}</h2>
                <span class="text-xs font-mono font-bold text-muted bg-surface-input px-2 py-0.5 rounded-md">{{ selectedItem?.id }}</span>
              </div>
              <p class="text-xs text-muted font-medium mt-0.5">{{ selectedItem?.term }} · {{ selectedItem?.submitter }}</p>
            </div>
          </div>
          <div class="flex items-center gap-2">
            <button @click="showScheduleModal = false" class="p-2 text-muted hover:text-heading hover:bg-surface-input rounded-xl transition-colors">
              <X :size="20" />
            </button>
          </div>
        </div>

        <!-- Schedule Grid Content -->
        <div class="p-6 overflow-y-auto flex-1 space-y-4">
          <div class="grid grid-cols-4 gap-3 p-4 rounded-2xl surface-input border border-card text-xs">
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Số lớp HP</span>
              <span class="font-bold text-heading text-sm">{{ selectedItem?.classes }} Lớp</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Tổng thời lượng</span>
              <span class="font-bold text-heading text-sm">{{ formatHours(selectedItem?.hours) }} Giờ</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Người nộp TKB</span>
              <span class="font-bold text-heading text-sm">{{ selectedItem?.submitter }}</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Trạng thái</span>
              <GlassBadge v-if="selectedItem?.status === 'pending'" variant="warning" size="sm">Chờ BGH duyệt</GlassBadge>
              <GlassBadge v-else-if="selectedItem?.status === 'approved'" variant="success" size="sm">Đã phê duyệt</GlassBadge>
              <GlassBadge v-else variant="danger" size="sm">Đã trả về</GlassBadge>
            </div>
          </div>

          <!-- Weekly Schedule Table Grid -->
          <div class="border border-card rounded-2xl overflow-hidden shadow-sm">
            <table class="w-full border-collapse text-left text-xs">
              <thead>
                <tr class="surface-solid border-b border-card">
                  <th class="p-3 font-bold text-muted text-center w-28 border-r border-card">Ca học / Thứ</th>
                  <th v-for="day in daysOfWeek" :key="day" class="p-3 font-bold text-heading text-center border-r border-card last:border-r-0">
                    {{ day }}
                  </th>
                </tr>
              </thead>
              <tbody class="divide-y divide-card">
                <tr v-for="shift in shifts" :key="shift.id" class="hover:bg-surface-input/50 transition-colors">
                  <td class="p-3 border-r border-card font-semibold surface-solid">
                    <p class="font-bold text-heading text-xs">{{ shift.name }}</p>
                    <p class="text-[10px] text-muted">{{ shift.time }}</p>
                  </td>
                  <td v-for="(day, dayIdx) in daysOfWeek" :key="dayIdx" class="p-2 border-r border-card last:border-r-0 align-top h-24">
                    <div v-if="getGridItem(dayIdx, shift.id)"
                         :class="['p-2 rounded-xl border text-[11px] font-semibold h-full flex flex-col justify-between shadow-xs', getGridItem(dayIdx, shift.id).color]">
                      <div>
                        <p class="font-bold leading-tight">{{ getGridItem(dayIdx, shift.id).subject }}</p>
                        <p class="text-[10px] opacity-80 mt-0.5 font-mono">{{ getGridItem(dayIdx, shift.id).code }}</p>
                      </div>
                      <div class="mt-2 text-[10px] opacity-90 flex flex-col gap-0.5 border-t stroke-current/20 pt-1">
                        <span class="flex items-center gap-1"><User :size="10" /> {{ getGridItem(dayIdx, shift.id).teacher }}</span>
                        <span class="flex items-center gap-1 font-bold"><MapPin :size="10" /> {{ getGridItem(dayIdx, shift.id).room }}</span>
                      </div>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="p-4 border-t border-default flex justify-end gap-2 surface-card">
          <GlassButton v-if="selectedItem?.status === 'pending'" variant="primary" size="sm" @click="showScheduleModal = false; openConfirm('approve', selectedItem)">
            <CheckCircle2 :size="14" class="mr-1" /> Duyệt ngay
          </GlassButton>
          <GlassButton variant="secondary" size="sm" @click="showScheduleModal = false">
            Đóng cửa sổ
          </GlassButton>
        </div>

      </div>
    </div>

    <ConfirmActionDialog
      v-model="confirmDialog.isOpen"
      :title="confirmDialog.action === 'approve' ? 'Xác nhận phê duyệt' : 'Xác nhận trả về'"
      :message="confirmDialog.message"
      :variant="confirmDialog.action === 'approve' ? 'primary' : 'danger'"
      :confirmLabel="'Đồng ý'"
      @confirm="handleConfirm"
    />
    </template>
  </div>
</template>
