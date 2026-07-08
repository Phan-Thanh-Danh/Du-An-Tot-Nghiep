<script setup>
import { ref, computed, onMounted } from 'vue'
import { Activity, Filter, ArrowRight, Search, CheckCircle2, XCircle, Clock, ChevronDown, Loader2, X } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const popup = usePopupStore()
const searchQuery = ref('')
const statusFilter = ref('all')
const showFilterDetail = ref(false)

const changes = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getScheduleChanges()
    changes.value = unwrapApiData(res) || []
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu thay đổi lịch'
  } finally {
    loading.value = false
  }
}

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
  }
  popup.info('Đã từ chối', `Thay đổi "${item.id}" — ${item.subject} đã bị từ chối.`)
}

onMounted(() => { loadData() })
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
                <GlassBadge :variant="changeTypeBadge(item.type).variant" size="sm">{{ changeTypeBadge(item.type).label }}</GlassBadge>
                <GlassBadge v-if="item.status === 'pending'" variant="warning" size="sm">Chờ duyệt</GlassBadge>
                <GlassBadge v-else-if="item.status === 'approved'" variant="success" size="sm">Đã duyệt</GlassBadge>
                <GlassBadge v-else variant="danger" size="sm">Từ chối</GlassBadge>
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
    </template>
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
