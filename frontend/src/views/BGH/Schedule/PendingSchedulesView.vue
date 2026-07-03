<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  CheckCircle2, XCircle, Eye, Search, Filter, AlertTriangle, Clock, User, Building2,
  ChevronDown, Loader2, X
} from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)

const popup = usePopupStore()
const router = useRouter()

const mockPendingSets = [
  { id: 'TKB-001', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa CNTT', classes: 86, slots: 420, conflicts: 0, sender: 'Phạm Minh D', date: '12/05/2026', status: 'pending_approval' },
  { id: 'TKB-002', semester: 'Spring 2026', campus: 'Cơ sở 2', dept: 'Khoa Kinh tế', classes: 42, slots: 215, conflicts: 3, sender: 'Nguyễn Bích L', date: '11/05/2026', status: 'pending_approval' },
  { id: 'TKB-003', semester: 'Spring 2026', campus: 'Cơ sở chính', dept: 'Khoa Ngoại ngữ', classes: 65, slots: 310, conflicts: 1, sender: 'Trần Văn K', date: '13/05/2026', status: 'pending_approval' },
  { id: 'TKB-004', semester: 'Fall 2025', campus: 'Cơ sở chính', dept: 'Khoa Thiết kế', classes: 38, slots: 195, conflicts: 0, sender: 'Lê Hoàng A', date: '10/05/2026', status: 'approved' },
]

const pendingSets = ref(mockPendingSets)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    if (!ENABLE_MOCK_API) {
      const res = await bghApi.getPendingSchedules()
      pendingSets.value = unwrapApiData(res) || mockPendingSets
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu TKB chờ duyệt'
    pendingSets.value = mockPendingSets
  } finally {
    loading.value = false
  }
}

const searchQuery = ref('')
const semesterFilter = ref('all')
const showAdvancedFilter = ref(false)
const conflictFilter = ref('all')

const filteredSets = computed(() => {
  let list = pendingSets.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.dept.toLowerCase().includes(q) || s.id.toLowerCase().includes(q) || s.sender.toLowerCase().includes(q) || s.semester.toLowerCase().includes(q))
  }
  if (semesterFilter.value !== 'all') {
    list = list.filter(s => s.semester === semesterFilter.value)
  }
  if (conflictFilter.value === 'has') {
    list = list.filter(s => s.conflicts > 0)
  } else if (conflictFilter.value === 'none') {
    list = list.filter(s => s.conflicts === 0)
  }
  return list
})

function approveSet(item) {
  const idx = pendingSets.value.findIndex(s => s.id === item.id)
  if (idx !== -1) {
    pendingSets.value[idx] = { ...pendingSets.value[idx], status: 'approved' }
    popup.success('Đã phê duyệt', `Bộ TKB "${item.id}" — ${item.dept} đã được duyệt.`)
  }
}

function rejectSet(item) {
  const idx = pendingSets.value.findIndex(s => s.id === item.id)
  if (idx !== -1) {
    pendingSets.value[idx] = { ...pendingSets.value[idx], status: 'rejected' }
    popup.info('Đã từ chối', `Bộ TKB "${item.id}" — ${item.dept} đã bị từ chối.`)
  }
}

function viewDetail(item) {
  popup.info(`Chi tiết TKB: ${item.id}`, `${item.dept}\nHọc kỳ: ${item.semester}\nCơ sở: ${item.campus}\nLớp: ${item.classes}\nLịch: ${item.slots}\nXung đột: ${item.conflicts}\nNgười gửi: ${item.sender}`)
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="space-y-4">
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
    <div class="space-y-4">
      
      <div class="surface-card border border-card rounded-2xl p-4 flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-3 flex-1">
           <div class="relative max-w-sm w-full">
              <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input v-model="searchQuery" type="text" placeholder="Tìm theo khoa, mã duyệt..." class="w-full surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
           </div>
           <select v-model="semesterFilter" class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
              <option value="all">Tất cả kỳ</option>
              <option value="Spring 2026">Spring 2026</option>
              <option value="Fall 2025">Fall 2025</option>
           </select>
        </div>
        <button @click="showAdvancedFilter = !showAdvancedFilter" class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
           <Filter :size="18" /> Lọc nâng cao <ChevronDown :size="14" :class="showAdvancedFilter ? 'rotate-180' : ''" class="transition-transform" />
        </button>
      </div>

      <Transition name="fade-slide">
        <div v-if="showAdvancedFilter" class="surface-card border border-card rounded-2xl p-4">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div>
              <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Xung đột</label>
              <select v-model="conflictFilter" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
                <option value="all">Tất cả</option>
                <option value="has">Có xung đột</option>
                <option value="none">Không xung đột</option>
              </select>
            </div>
          </div>
          <div class="flex justify-end gap-2 mt-4">
            <button @click="conflictFilter = 'all'; semesterFilter = 'all'; showAdvancedFilter = false" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại</button>
          </div>
        </div>
      </Transition>

      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Khoa / Bộ phận</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Học kỳ & CS</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Quy mô</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Xung đột</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest">Người gửi</th>
              <th class="px-4 py-3 text-[10px] font-semibold text-muted uppercase tracking-widest text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="set in filteredSets" :key="set.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20">
                    <Building2 :size="18" />
                  </div>
                  <p class="text-sm font-semibold text-heading leading-tight">{{ set.dept }}</p>
                </div>
              </td>
              <td class="px-4 py-3">
                <div>
                  <p class="text-xs font-semibold text-label leading-tight">{{ set.semester }}</p>
                  <p class="text-[10px] font-bold text-muted mt-0.5">{{ set.campus }}</p>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex flex-col gap-1">
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-tighter">Lớp: {{ set.classes }}</span>
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-tighter">Lịch: {{ set.slots }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div v-if="set.conflicts === 0" class="inline-flex items-center gap-1.5 rounded-lg border border-(--color-success-text)/20 bg-(--color-success-bg) px-2 py-1 text-(--color-success-text)">
                   <CheckCircle2 :size="14" />
                   <span class="text-[10px] font-semibold uppercase tracking-widest">Sẵn sàng</span>
                </div>
                <div v-else class="inline-flex items-center gap-1.5 rounded-lg border border-(--color-danger-text)/20 bg-(--color-danger-bg) px-2 py-1 text-(--color-danger-text)">
                   <AlertTriangle :size="14" />
                   <span class="text-[10px] font-semibold uppercase tracking-widest">{{ set.conflicts }} lỗi</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2 text-xs font-bold text-label">
                   <User :size="14" /> {{ set.sender }}
                </div>
                <p class="text-[10px] font-bold text-muted mt-1">{{ set.date }}</p>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-1">
                  <button @click="viewDetail(set)" class="p-2 hover:bg-(--color-info-bg) hover:text-(--color-info-text) rounded-lg text-muted transition-all" title="Xem chi tiết">
                    <Eye :size="18" />
                  </button>
                  <button v-if="set.status === 'pending_approval'" @click="approveSet(set)" class="p-2 hover:bg-(--color-success-bg) hover:text-(--color-success-text) rounded-lg text-muted transition-all" title="Phê duyệt">
                    <CheckCircle2 :size="18" />
                  </button>
                  <button v-if="set.status === 'pending_approval'" @click="rejectSet(set)" class="p-2 hover:bg-(--color-danger-bg) hover:text-(--color-danger-text) rounded-lg text-muted transition-all" title="Từ chối">
                    <XCircle :size="18" />
                  </button>
                  <span v-if="set.status === 'approved'" class="text-[10px] font-semibold text-(--color-success-text) uppercase tracking-widest">Đã duyệt</span>
                  <span v-if="set.status === 'rejected'" class="text-[10px] font-semibold text-(--color-danger-text) uppercase tracking-widest">Đã từ chối</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="filteredSets.length === 0" class="py-12 text-center">
          <CheckCircle2 :size="36" class="text-placeholder mx-auto mb-3" />
          <p class="text-xs font-semibold text-muted">Không tìm thấy dữ liệu phù hợp</p>
          <button @click="searchQuery = ''; semesterFilter = 'all'" class="mt-2 text-[11px] font-bold text-link underline-offset-2 hover:underline">Xoá bộ lọc</button>
        </div>
      </div>

      <div class="surface-card border border-(--color-info-text)/20 bg-(--color-info-bg) rounded-2xl p-4">
         <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-info-text) shrink-0 border border-(--color-info-text)/20">
               <Clock :size="20" />
            </div>
            <div>
               <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Chính sách phê duyệt TKB</h4>
               <p class="text-xs text-(--color-info-text) mt-1 leading-relaxed">
                 Hệ thống chỉ cho phép <strong>Duyệt & Publish</strong> bộ TKB khi số lượng xung đột nghiêm trọng (trùng phòng, trùng giảng viên) bằng 0. Nếu còn xung đột, BGH vui lòng gửi yêu cầu Giáo vụ chỉnh sửa.
               </p>
            </div>
         </div>
      </div>

  </div>
    </template>
  </div>
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
