<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertTriangle, CheckCircle2, Filter, Building2, CalendarDays, User, Search, ChevronDown, Loader2, X } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'
import { apiRequest } from '@/services/apiClient'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)

const popup = usePopupStore()
const searchQuery = ref('')
const severityFilter = ref('all')
const showFilterDetail = ref(false)

const mockConflicts = [
  { id: 'CF-001', type: 'room', severity: 'critical', dept1: 'Khoa CNTT', course1: 'Lập trình Web (SE1601)', dept2: 'Khoa Kinh tế', course2: 'Kế toán (KT120)', room: 'A1.01', slot: 'Thứ 2, Ca 1 (07:30-09:30)', date: '15/06/2026', status: 'unresolved' },
  { id: 'CF-002', type: 'teacher', severity: 'critical', dept1: 'Khoa CNTT', course1: 'Cấu trúc dữ liệu (SE1602)', dept2: 'Khoa CNTT', course2: 'Java (SE1603)', room: '—', teacher: 'Nguyễn Văn A', slot: 'Thứ 3, Ca 2 (09:45-11:45)', date: '16/06/2026', status: 'unresolved' },
  { id: 'CF-003', type: 'room', severity: 'warning', dept1: 'Khoa Ngoại ngữ', course1: 'Tiếng Anh 3 (EN101)', dept2: 'Khoa Thiết kế', course2: 'Đồ họa (GD201)', room: 'B2.03', slot: 'Thứ 5, Ca 3 (13:00-15:00)', date: '17/06/2026', status: 'resolved' },
]

const conflicts = ref(mockConflicts)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    if (!ENABLE_MOCK_API) {
      const res = await apiRequest('/api/thoi-khoa-bieu/check-xung-dot')
      conflicts.value = res?.data ?? res?.Data ?? mockConflicts
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu xung đột'
    conflicts.value = mockConflicts
  } finally {
    loading.value = false
  }
}

const filteredConflicts = computed(() => {
  let list = conflicts.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.id.toLowerCase().includes(q) || c.dept1.toLowerCase().includes(q) || c.dept2.toLowerCase().includes(q) || c.course1.toLowerCase().includes(q) || c.course2.toLowerCase().includes(q))
  }
  if (severityFilter.value !== 'all') {
    list = list.filter(c => c.severity === severityFilter.value)
  }
  return list
})

const unresolvedCount = computed(() => conflicts.value.filter(c => c.status === 'unresolved').length)

function resolveConflict(item) {
  const idx = conflicts.value.findIndex(c => c.id === item.id)
  if (idx !== -1) {
    conflicts.value[idx] = { ...conflicts.value[idx], status: 'resolved' }
  }
  popup.success('Đã xử lý', `Xung đột "${item.id}" đã được đánh dấu đã xử lý.`)
}

onMounted(() => { loadData() })
</script>

<template>
  <PageContainer title="Giám sát Xung đột" subtitle="Theo dõi tổng thể các lỗi tài nguyên giảng dạy và không gian.">
    <template #actions>
      <div class="relative">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input v-model="searchQuery" type="text" placeholder="Tìm khoa, môn học..." class="w-56 surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
      </div>
      <div class="text-xs font-bold" :class="unresolvedCount > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)'">
        {{ unresolvedCount }} chưa xử lý
      </div>
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
      <div class="flex items-center gap-1.5 rounded-lg border border-(--color-danger-text)/20 bg-(--color-danger-bg) px-3 py-1.5 text-(--color-danger-text)">
        <AlertTriangle :size="14" />
        <span class="text-[10px] font-semibold uppercase tracking-widest">{{ conflicts.filter(c => c.severity === 'critical' && c.status === 'unresolved').length }} Critical</span>
      </div>
      <div class="flex items-center gap-1.5 rounded-lg border border-(--color-warning-text)/20 bg-(--color-warning-bg) px-3 py-1.5 text-(--color-warning-text)">
        <AlertTriangle :size="14" />
        <span class="text-[10px] font-semibold uppercase tracking-widest">{{ conflicts.filter(c => c.severity === 'warning' && c.status === 'unresolved').length }} Warning</span>
      </div>
      <button @click="showFilterDetail = !showFilterDetail" class="lg-button-secondary px-3 py-1.5 text-[10px] font-bold flex items-center gap-1">
        <Filter :size="14" /> Lọc <ChevronDown :size="10" :class="showFilterDetail ? 'rotate-180' : ''" class="transition-transform" />
      </button>
    </div>

    <Transition name="fade-slide">
      <div v-if="showFilterDetail" class="surface-card border border-card rounded-2xl p-4 mb-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Mức độ</label>
            <select v-model="severityFilter" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
              <option value="all">Tất cả</option>
              <option value="critical">Critical</option>
              <option value="warning">Warning</option>
            </select>
          </div>
        </div>
        <div class="flex justify-end mt-4">
          <button @click="severityFilter = 'all'; showFilterDetail = false" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại</button>
        </div>
      </div>
    </Transition>

    <div v-if="filteredConflicts.length > 0" class="space-y-3">
      <div v-for="cf in filteredConflicts" :key="cf.id" class="surface-card border rounded-2xl p-4 transition-all"
        :class="cf.status === 'resolved' ? 'border-(--color-success-text)/20 opacity-70' : cf.severity === 'critical' ? 'border-(--color-danger-text)/20' : 'border-(--color-warning-text)/20'">
        <div class="flex items-start justify-between gap-4">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-2">
              <div :class="['h-8 w-8 rounded-xl flex items-center justify-center shrink-0', cf.type === 'room' ? 'bg-(--color-warning-bg) text-(--color-warning-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)']">
                <Building2 :size="16" />
              </div>
              <div>
                <div class="flex items-center gap-2">
                  <span class="text-xs font-mono font-bold text-muted">{{ cf.id }}</span>
                  <GlassBadge :variant="cf.severity === 'critical' ? 'danger' : 'warning'" size="sm">{{ cf.severity === 'critical' ? 'Nghiêm trọng' : 'Cảnh báo' }}</GlassBadge>
                  <GlassBadge v-if="cf.status === 'resolved'" variant="success" size="sm">Đã xử lý</GlassBadge>
                </div>
                <p class="text-sm font-bold text-heading mt-0.5">{{ cf.type === 'room' ? 'Trùng phòng' : 'Trùng giảng viên' }}</p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mt-3">
              <div class="surface-solid rounded-xl p-2.5 border border-default">
                <p class="text-[10px] font-bold text-muted uppercase tracking-widest mb-1">{{ cf.dept1 }}</p>
                <p class="text-xs font-semibold text-heading">{{ cf.course1 }}</p>
              </div>
              <div class="surface-solid rounded-xl p-2.5 border border-default">
                <p class="text-[10px] font-bold text-muted uppercase tracking-widest mb-1">{{ cf.dept2 }}</p>
                <p class="text-xs font-semibold text-heading">{{ cf.course2 }}</p>
              </div>
            </div>
          </div>

          <div class="shrink-0 text-right">
            <div class="text-xs font-bold text-label flex items-center gap-1 justify-end mb-1">
              <CalendarDays :size="12" /> {{ cf.date }}
            </div>
            <p class="text-[10px] text-muted">{{ cf.slot }}</p>
            <p v-if="cf.room !== '—'" class="text-[10px] font-bold text-muted mt-1">Phòng: {{ cf.room }}</p>
            <p v-if="cf.teacher" class="text-[10px] font-bold text-muted mt-1">
              <User :size="10" class="inline" /> {{ cf.teacher }}
            </p>
            <button v-if="cf.status === 'unresolved'" @click="resolveConflict(cf)" class="mt-3 lg-button-primary px-4 py-1.5 text-[10px] font-bold rounded-lg">Đã xử lý</button>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="surface-card border border-card rounded-2xl flex flex-col items-center justify-center p-12 text-center">
      <CheckCircle2 :size="48" class="text-(--color-success-text)/50 mb-4" />
      <h3 class="text-lg font-bold text-heading">Hệ thống hoạt động ổn định</h3>
      <p class="mt-2 text-sm text-muted max-w-md">Chưa phát hiện xung đột lịch nào ở cấp toàn trường. Các giáo vụ khoa đã xử lý tốt ở cấp đơn vị.</p>
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
