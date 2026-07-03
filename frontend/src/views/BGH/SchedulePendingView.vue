<script setup>
import { ref, computed, onMounted } from 'vue'
import { Clock, Search, AlertTriangle, CheckCircle2, RotateCcw, AlertCircle, Loader2 } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { bghPendingSchedules } from '@/mocks/scheduleAttendanceMockData'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)

const popupStore = usePopupStore()

const mockSchedules = bghPendingSchedules.map(s => ({ ...s }))
const schedules = ref(mockSchedules)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    if (!ENABLE_MOCK_API) {
      const res = await bghApi.getPendingSchedules()
      schedules.value = unwrapApiData(res) || mockSchedules
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu lịch chờ duyệt'
    schedules.value = mockSchedules
  } finally {
    loading.value = false
  }
}

const searchQuery = ref('')
const filterHocKy = ref('')
const filterTrangThai = ref('')
const selectedItem = ref(null)
const confirmAction = ref(null)
const rejectReason = ref('')

const summaryCards = computed(() => [
  { label: 'Chờ duyệt', value: schedules.value.filter(s => s.trangThai === 'cho_duyet').length, border: 'border-amber-500' },
  { label: 'Có xung đột', value: schedules.value.filter(s => s.xungDot > 0).length, border: 'border-red-500' },
  { label: 'Đã trả về', value: schedules.value.filter(s => s.trangThai === 'tra_ve').length, border: 'border-blue-500' },
  { label: 'Đã duyệt hôm nay', value: 0, border: 'border-emerald-500' },
])

const filteredSchedules = computed(() => {
  let list = schedules.value
  if (filterHocKy.value) list = list.filter(s => s.tenHocKy === filterHocKy.value)
  if (filterTrangThai.value) list = list.filter(s => s.trangThai === filterTrangThai.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.maTkb.toLowerCase().includes(q) || s.tenDonVi.toLowerCase().includes(q) || s.tenHocKy.toLowerCase().includes(q))
  }
  return list
})

const trangThaiLabel = (tt) => {
  const map = { cho_duyet: 'Chờ duyệt', da_duyet: 'Đã duyệt', tra_ve: 'Đã trả về', da_huy: 'Đã hủy' }
  return map[tt] || tt
}
const trangThaiVariant = (tt) => {
  const map = { cho_duyet: 'warning', da_duyet: 'success', tra_ve: 'info', da_huy: 'neutral' }
  return map[tt] || 'neutral'
}

function approve(item) {
  confirmAction.value = {
    title: 'Duyệt thời khóa biểu?',
    message: `Duyệt lịch ${item.maTkb} (${item.tenDonVi} · ${item.tenHocKy})?${item.xungDot > 0 ? ` Lưu ý: còn ${item.xungDot} xung đột.` : ''}`,
    label: 'Duyệt',
    variant: 'primary',
    run: () => {
      item.trangThai = 'da_duyet'
      confirmAction.value = null
      if (selectedItem.value?.id === item.id) selectedItem.value = { ...item }
      popupStore.success('Đã duyệt', `Lịch ${item.maTkb} đã được phê duyệt.`)
    }
  }
}

function returnForEdit(item) {
  if (!rejectReason.value.trim()) {
    popupStore.warning('Thiếu thông tin', 'Vui lòng nhập lý do trả về.')
    return
  }
  confirmAction.value = {
    title: 'Trả về để chỉnh sửa?',
    message: `Lịch ${item.maTkb} sẽ được trả về Phòng Đào tạo để chỉnh sửa. Lý do: "${rejectReason.value}"`,
    label: 'Trả về',
    variant: 'danger',
    run: () => {
      item.trangThai = 'tra_ve'
      confirmAction.value = null
      rejectReason.value = ''
      if (selectedItem.value?.id === item.id) selectedItem.value = { ...item }
      popupStore.success('Đã trả về', `Lịch ${item.maTkb} đã được trả về để chỉnh sửa.`)
    }
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="bgh-pending max-w-7xl mx-auto space-y-5">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin" />
        <p class="text-sm font-medium">Đang tải dữ liệu...</p>
      </div>
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <Clock class="text-amber-500" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Duyệt Thời khóa biểu</h1>
      </div>
      <p class="text-(--text-body)">Phê duyệt hoặc trả về thời khóa biểu trước khi công bố chính thức cho sinh viên và giảng viên.</p>
    </GlassPanel>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel v-for="c in summaryCards" :key="c.label" variant="flat" density="compact" class="flex flex-col justify-center min-h-[88px]" :class="`border-l-4 ${c.border}`">
        <p class="text-sm font-medium text-(--text-muted) mb-1">{{ c.label }}</p>
        <strong class="text-2xl text-(--text-heading)">{{ c.value }}</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 items-center">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded-lg border border-(--border-input) flex-1 min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
          <Search :size="16" class="text-(--text-muted) shrink-0" />
          <input v-model="searchQuery" type="text" placeholder="Tìm mã, đơn vị, học kỳ..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filterHocKy" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả học kỳ</option>
          <option>Spring 2026</option><option>Summer 2026</option><option>Fall 2025</option>
        </select>
        <select v-model="filterTrangThai" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả trạng thái</option>
          <option value="cho_duyet">Chờ duyệt</option>
          <option value="tra_ve">Đã trả về</option>
          <option value="da_duyet">Đã duyệt</option>
        </select>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[450px]">
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto">
          <TableShell v-if="filteredSchedules.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Mã lịch</th>
                  <th>Đơn vị</th>
                  <th>Học kỳ</th>
                  <th>Quy mô</th>
                  <th>Xung đột</th>
                  <th>Trạng thái</th>
                  <th class="text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="s in filteredSchedules" :key="s.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedItem?.id === s.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedItem = s; rejectReason = ''"
                >
                  <td class="font-mono text-sm text-(--text-muted) whitespace-nowrap">{{ s.maTkb }}</td>
                  <td class="text-sm font-medium max-w-[160px] line-clamp-1" :title="s.tenDonVi">{{ s.tenDonVi }}</td>
                  <td class="text-sm whitespace-nowrap">{{ s.tenHocKy }}</td>
                  <td class="text-sm">
                    <div class="flex flex-col gap-0.5">
                      <span class="text-xs text-(--text-muted)">{{ s.soLop }} lớp · {{ s.soGiaoVien }} GV</span>
                      <span class="text-xs text-(--text-muted)">{{ s.tongSoTiet }} tiết/tuần</span>
                    </div>
                  </td>
                  <td>
                    <GlassBadge v-if="s.xungDot > 0" variant="danger" size="sm">{{ s.xungDot }} xung đột</GlassBadge>
                    <GlassBadge v-else variant="success" size="sm">Sạch</GlassBadge>
                  </td>
                  <td>
                    <GlassBadge :variant="trangThaiVariant(s.trangThai)" size="sm">{{ trangThaiLabel(s.trangThai) }}</GlassBadge>
                  </td>
                  <td class="text-right">
                    <div class="flex justify-end gap-1" v-if="s.trangThai === 'cho_duyet'">
                      <GlassButton variant="ghost" size="xs" class="text-emerald-600" @click.stop="approve(s)">Duyệt</GlassButton>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không có lịch chờ duyệt" description="Tất cả lịch học đã được xử lý hoặc không có lịch nào phù hợp bộ lọc." />
          </div>
        </div>

        <!-- Detail panel -->
        <div class="lg:col-span-1 bg-(--surface-card) flex flex-col">
          <div v-if="!selectedItem" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một lịch từ danh sách để xem chi tiết và thực hiện phê duyệt
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center justify-between mb-3">
                <h3 class="font-bold text-lg text-(--text-heading)">{{ selectedItem.maTkb }}</h3>
                <GlassBadge :variant="trangThaiVariant(selectedItem.trangThai)">{{ trangThaiLabel(selectedItem.trangThai) }}</GlassBadge>
              </div>
              <p class="text-sm font-medium text-(--text-body)">{{ selectedItem.tenDonVi }}</p>
              <p class="text-xs text-(--text-muted) mt-0.5">{{ selectedItem.tenHocKy }} · Nộp: {{ selectedItem.ngayTao }}</p>
            </div>

            <div class="p-5 flex-1 overflow-y-auto space-y-4">
              <div class="grid grid-cols-3 gap-3">
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-lg font-bold text-(--text-heading)">{{ selectedItem.soLop }}</div>
                  <div class="text-xs text-(--text-muted)">Lớp</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-lg font-bold text-(--text-heading)">{{ selectedItem.soGiaoVien }}</div>
                  <div class="text-xs text-(--text-muted)">Giảng viên</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-lg font-bold text-(--text-heading)">{{ selectedItem.tongSoTiet }}</div>
                  <div class="text-xs text-(--text-muted)">Tiết/tuần</div>
                </div>
              </div>

              <div>
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Tình trạng xung đột</div>
                <div v-if="selectedItem.xungDot === 0" class="flex items-center gap-2 text-emerald-600 dark:text-emerald-400 text-sm">
                  <CheckCircle2 :size="16" /> Không phát hiện xung đột
                </div>
                <div v-else>
                  <div class="flex items-center gap-2 text-red-500 text-sm mb-2">
                    <AlertTriangle :size="16" /> {{ selectedItem.xungDot }} xung đột phát hiện
                  </div>
                  <div v-for="(conf, i) in selectedItem.conflicts" :key="i" class="bg-(--color-danger-bg) text-(--color-danger-text) text-xs p-2 rounded-lg mb-1 flex items-start gap-1.5">
                    <AlertTriangle :size="12" class="mt-0.5 shrink-0" /> {{ conf }}
                  </div>
                </div>
              </div>

              <div v-if="selectedItem.trangThai === 'cho_duyet'">
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Lý do trả về (nếu từ chối)</div>
                <textarea v-model="rejectReason" rows="3" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) resize-none" placeholder="Nhập lý do trả về để chỉnh sửa..."></textarea>
              </div>
            </div>

            <div class="p-4 border-t border-(--border-default) bg-(--surface-modal) flex flex-col gap-2" v-if="selectedItem.trangThai === 'cho_duyet'">
              <GlassButton variant="primary" class="w-full h-10 justify-center" @click="approve(selectedItem)">
                <CheckCircle2 :size="16" class="mr-1" /> Phê duyệt lịch học
              </GlassButton>
              <GlassButton variant="secondary" class="w-full h-10 justify-center !text-red-500 !border-red-400 hover:!bg-red-500/10" @click="returnForEdit(selectedItem)">
                <RotateCcw :size="16" class="mr-1" /> Trả về chỉnh sửa
              </GlassButton>
            </div>
            <div class="p-4 border-t border-(--border-default) bg-(--surface-modal)" v-else>
              <GlassBadge :variant="trangThaiVariant(selectedItem.trangThai)" class="w-full justify-center py-2">
                {{ trangThaiLabel(selectedItem.trangThai) }}
              </GlassBadge>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
    </template>
  </div>
</template>
