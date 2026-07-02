<script setup>
import { ref, computed } from 'vue'
import { Activity, Filter, ArrowRight, Search, CheckCircle2, XCircle, Clock, ChevronDown, X } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()
const searchQuery = ref('')
const statusFilter = ref('all')
const showFilterDetail = ref(false)

const changes = ref([
  { id: 'CH-001', subject: 'Lập trình Web (SE104.N11)', type: 'makeup', teacher: 'Lê Văn C', oldSlot: 'Thứ 3, Ca 1 - A1.01', newSlot: 'Thứ 7, Ca 2 - B2.03', reason: 'Bận họp Khoa', date: '15/06/2026', updated: '10 phút trước', status: 'pending' },
  { id: 'CH-002', subject: 'Cơ sở dữ liệu (SE201.N12)', type: 'swap', teacher: 'Trần Thị H', oldSlot: 'Thứ 5, Ca 2 - A2.05', newSlot: 'Thứ 4, Ca 3 - A2.05', reason: 'Điều chỉnh lịch phòng', date: '16/06/2026', updated: '30 phút trước', status: 'pending' },
  { id: 'CH-003', subject: 'Tiếng Anh 3 (EN101.N02)', type: 'cancel', teacher: 'Nguyễn Văn K', oldSlot: 'Thứ 2, Ca 1 - B1.01', newSlot: '—', reason: 'Lịch nghỉ bù', date: '12/06/2026', updated: '1 giờ trước', status: 'approved' },
  { id: 'CH-004', subject: 'Toán rời rạc (MA101.N05)', type: 'makeup', teacher: 'Phạm Minh D', oldSlot: 'Thứ 6, Ca 3 - C1.02', newSlot: 'CN, Ca 1 - C1.02', reason: 'Bận công tác', date: '14/06/2026', updated: '2 giờ trước', status: 'approved' },
  { id: 'CH-005', subject: 'Kinh tế vi mô (EC201.N03)', type: 'swap', teacher: 'Hoàng Thị L', oldSlot: 'Thứ 4, Ca 2 - A3.04', newSlot: 'Thứ 5, Ca 2 - B1.03', reason: 'Trùng lịch GV', date: '17/06/2026', updated: '3 giờ trước', status: 'rejected' },
])

const filteredChanges = computed(() => {
  let list = changes.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.subject.toLowerCase().includes(q) || c.teacher.toLowerCase().includes(q) || c.id.toLowerCase().includes(q))
  }
  if (statusFilter.value !== 'all') {
    list = list.filter(c => c.status === statusFilter.value)
  }
  return list
})

const pendingCount = computed(() => changes.value.filter(c => c.status === 'pending').length)

const changeTypeBadge = (type) => {
  if (type === 'makeup') return { variant: 'info', label: 'Dạy bù' }
  if (type === 'swap') return { variant: 'warning', label: 'Đổi lịch' }
  return { variant: 'danger', label: 'Hủy' }
}

function approveChange(item) {
  const idx = changes.value.findIndex(c => c.id === item.id)
  if (idx !== -1) {
    changes.value[idx] = { ...changes.value[idx], status: 'approved' }
    popup.success('Đã duyệt', `Thay đổi "${item.id}" — ${item.subject} đã được phê duyệt.`)
  }
}

function rejectChange(item) {
  const idx = changes.value.findIndex(c => c.id === item.id)
  if (idx !== -1) {
    changes.value[idx] = { ...changes.value[idx], status: 'rejected' }
    popup.info('Đã từ chối', `Thay đổi "${item.id}" — ${item.subject} đã bị từ chối.`)
  }
}
</script>

<template>
  <PageContainer title="Thay đổi & Dạy bù" subtitle="Giám sát các biến động bất thường so với thời khóa biểu gốc.">
    <template #actions>
      <div class="relative">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input v-model="searchQuery" type="text" placeholder="Tìm môn, giảng viên..." class="w-56 surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
      </div>
      <div class="text-xs font-bold text-muted">{{ pendingCount }} chờ duyệt</div>
    </template>

    <div class="flex flex-wrap items-center gap-3 mb-4">
      <div class="flex gap-1 rounded-lg border border-default p-0.5 bg-(--surface-input)">
        <button @click="statusFilter = 'all'" class="px-3 py-1.5 text-[10px] font-semibold rounded-lg transition-colors" :class="statusFilter === 'all' ? 'bg-(--surface-card) text-heading shadow-sm' : 'text-muted hover:text-heading'">Tất cả</button>
        <button @click="statusFilter = 'pending'" class="px-3 py-1.5 text-[10px] font-semibold rounded-lg transition-colors" :class="statusFilter === 'pending' ? 'bg-amber-50 text-amber-700 shadow-sm' : 'text-muted hover:text-heading'">Chờ duyệt</button>
        <button @click="statusFilter = 'approved'" class="px-3 py-1.5 text-[10px] font-semibold rounded-lg transition-colors" :class="statusFilter === 'approved' ? 'bg-emerald-50 text-emerald-700 shadow-sm' : 'text-muted hover:text-heading'">Đã duyệt</button>
        <button @click="statusFilter = 'rejected'" class="px-3 py-1.5 text-[10px] font-semibold rounded-lg transition-colors" :class="statusFilter === 'rejected' ? 'bg-red-50 text-red-700 shadow-sm' : 'text-muted hover:text-heading'">Từ chối</button>
      </div>
      <button @click="showFilterDetail = !showFilterDetail" class="lg-button-secondary px-3 py-1.5 text-[10px] font-bold flex items-center gap-1">
        <Filter :size="14" /> Lọc <ChevronDown :size="10" :class="showFilterDetail ? 'rotate-180' : ''" class="transition-transform" />
      </button>
    </div>

    <Transition name="fade-slide">
      <div v-if="showFilterDetail" class="surface-card border border-card rounded-2xl p-4 mb-4">
        <div class="flex justify-end">
          <button @click="statusFilter = 'all'; showFilterDetail = false" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại</button>
        </div>
      </div>
    </Transition>

    <div class="space-y-3">
      <div v-for="item in filteredChanges" :key="item.id" class="surface-card border rounded-2xl p-4 transition-all group"
        :class="item.status === 'pending' ? 'border-amber-200/60 dark:border-amber-800/30' : item.status === 'approved' ? 'border-(--color-success-text)/20' : 'border-(--color-danger-text)/20'">
        <div class="flex items-start justify-between gap-4">
          <div class="flex items-start gap-4 flex-1 min-w-0">
            <div :class="['h-10 w-10 rounded-xl flex items-center justify-center shrink-0 border', item.type === 'makeup' ? 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20' : item.type === 'swap' ? 'bg-(--color-warning-bg) text-(--color-warning-text) border-(--color-warning-text)/20' : 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20']">
              <Activity :size="20" />
            </div>
            <div class="min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <h3 class="font-bold text-heading text-sm">{{ item.subject }}</h3>
                <GlassBadge :variant="changeTypeBadge(item.type).variant" size="xs">{{ changeTypeBadge(item.type).label }}</GlassBadge>
                <GlassBadge v-if="item.status === 'pending'" variant="warning" size="xs">Chờ duyệt</GlassBadge>
                <GlassBadge v-else-if="item.status === 'approved'" variant="success" size="xs">Đã duyệt</GlassBadge>
                <GlassBadge v-else variant="danger" size="xs">Từ chối</GlassBadge>
              </div>
              <div class="mt-2 space-y-1">
                <div class="flex items-center gap-2 text-sm text-body">
                  <span class="line-through text-muted">{{ item.oldSlot }}</span>
                  <ArrowRight :size="14" class="text-muted shrink-0" />
                  <span class="font-bold text-heading">{{ item.newSlot }}</span>
                </div>
                <div class="flex items-center gap-3 text-xs text-muted">
                  <span>GV: <strong class="text-label">{{ item.teacher }}</strong></span>
                  <span>Lý do: {{ item.reason }}</span>
                </div>
              </div>
            </div>
          </div>
          <div class="shrink-0 text-right flex flex-col items-end gap-2">
            <p class="text-[10px] text-muted flex items-center gap-1">
              <Clock :size="10" /> {{ item.updated }}
            </p>
            <div v-if="item.status === 'pending'" class="flex gap-1">
              <button @click="approveChange(item)" class="p-1.5 hover:bg-(--color-success-bg) hover:text-(--color-success-text) rounded-lg text-muted transition-all" title="Duyệt">
                <CheckCircle2 :size="18" />
              </button>
              <button @click="rejectChange(item)" class="p-1.5 hover:bg-(--color-danger-bg) hover:text-(--color-danger-text) rounded-lg text-muted transition-all" title="Từ chối">
                <XCircle :size="18" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="filteredChanges.length === 0" class="py-12 text-center surface-card border border-card rounded-2xl">
      <Activity :size="36" class="text-placeholder mx-auto mb-3" />
      <p class="text-xs font-semibold text-muted">Không có thay đổi nào phù hợp</p>
    </div>
  </PageContainer>
</template>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: all 0.25s ease-out;
}
.fade-slide-enter-from,
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}
</style>
