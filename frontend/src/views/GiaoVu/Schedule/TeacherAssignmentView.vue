<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  Search, Filter, Plus, Calendar, UserCheck, UserMinus,
  Users, MapPin, Pencil, Trash2, ArrowLeftRight, Clock, Loader2, AlertCircle, BookOpen
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')

const DEMO_ASSIGNMENTS = [
  { id: 'PC001', subject: 'Lập trình Java', class: 'SE1601', teacher: 'TS. Nguyễn Minh Khoa', sessions: 3, status: 'assigned', dept: 'CNTT', schedule: 'T2 (Ca 1-2), T4 (Ca 3)', room: 'Lab 102', students: 30 },
  { id: 'PC002', subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'ThS. Trần Thị Lan', sessions: 2, status: 'assigned', dept: 'CNTT', schedule: 'T3 (Ca 4-5)', room: 'P.305', students: 35 },
  { id: 'PC003', subject: 'Lập trình Web', class: 'SE1603', teacher: 'Chưa phân công', sessions: 3, status: 'unassigned', dept: 'CNTT', schedule: 'T5 (Ca 1-3)', room: 'Lab 201', students: 28 },
  { id: 'PC004', subject: 'Hệ quản trị CSDL', class: 'SE1604', teacher: 'ThS. Lê Văn Dũng', sessions: 3, status: 'assigned', dept: 'CNTT', schedule: 'T6 (Ca 4-6)', room: 'Lab 304', students: 32 },
]

// ── State ────────────────────────────────────────────────
const searchQuery = ref('')
const assignments = ref([])
const showDeleteModal = ref(false)
const itemToDelete = ref(null)

// ── Filter State ────────────────────────────────────────────
const filters = ref({
  status: 'all', // all, assigned, unassigned
  department: 'all'
})

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getSchedules()
    assignments.value = res?.items ?? res ?? []
  } catch (err) {
    if (ENABLE_MOCK_API) {
      assignments.value = DEMO_ASSIGNMENTS
    } else {
      apiError.value = err?.message || 'Không thể tải danh sách.'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })

const filteredAssignments = computed(() => {
  let result = assignments.value
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(item =>
      item.subject.toLowerCase().includes(query) ||
      item.class.toLowerCase().includes(query) ||
      item.teacher.toLowerCase().includes(query)
    )
  }
  if (filters.value.status !== 'all') {
    result = result.filter(item => item.status === filters.value.status)
  }
  if (filters.value.department !== 'all') {
    result = result.filter(item => item.dept === filters.value.department)
  }
  return result
})

function handleDelete(item) {
  itemToDelete.value = item
  showDeleteModal.value = true
}

function confirmDelete() {
  assignments.value = assignments.value.filter(a => a.id !== itemToDelete.value.id)
  showDeleteModal.value = false
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div class="flex items-center gap-2">
           <Users class="text-(--lg-primary)" :size="24" />
           <div>
              <h1 class="text-xl font-bold text-(--text-heading)">Phân công giảng viên</h1>
              <p class="text-sm text-(--text-muted)">Gán giảng viên theo lớp, môn học, ca và tải giảng dạy.</p>
           </div>
        </div>
        <div class="flex gap-2 w-full sm:w-auto">
          <div class="relative flex-1 sm:w-64">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="16" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo môn, lớp, tên GV..."
              class="pl-9 pr-4 py-2 w-full bg-(--surface-input) border border-(--border-input) rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-(--lg-primary)"
            />
          </div>
          <GlassButton variant="secondary" title="Bộ lọc phân công">
            <Filter :size="16" />
          </GlassButton>
          <GlassButton variant="secondary" title="Lịch giảng dạy">
            <Calendar :size="16" />
          </GlassButton>
          <GlassButton variant="primary">
            <Plus :size="16" class="mr-1" />
            <span class="hidden sm:inline">Phân công mới</span>
          </GlassButton>
        </div>
      </div>

      <!-- Stats -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div class="surface-card p-4 rounded-2xl border border-(--border-card) flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase">Tổng lớp HP</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">120</p>
          </div>
          <div class="w-10 h-10 rounded-full bg-(--accent-primary-soft) text-(--lg-primary) flex items-center justify-center"><BookOpen :size="20"/></div>
        </div>
        <div class="surface-card p-4 rounded-2xl border border-(--border-card) flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase">Đã phân công</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">105</p>
          </div>
          <div class="w-10 h-10 rounded-full bg-(--color-success-bg) text-(--color-success-text) flex items-center justify-center"><UserCheck :size="20"/></div>
        </div>
        <div class="surface-card p-4 rounded-2xl border border-(--border-card) flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase">Chưa phân công</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">15</p>
          </div>
          <div class="w-10 h-10 rounded-full bg-(--color-danger-bg) text-(--color-danger-text) flex items-center justify-center"><UserMinus :size="20"/></div>
        </div>
        <div class="surface-card p-4 rounded-2xl border border-(--border-card) flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase">Giảng viên cơ hữu</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">45</p>
          </div>
          <div class="w-10 h-10 rounded-full bg-(--color-warning-bg) text-(--color-warning-text) flex items-center justify-center"><Users :size="20"/></div>
        </div>
      </div>

      <!-- Data Table -->
      <div class="surface-card border border-(--border-card) rounded-2xl overflow-hidden flex flex-col flex-1 min-h-0">
        <div class="overflow-x-auto flex-1">
          <table class="w-full text-left text-sm whitespace-nowrap">
            <thead class="bg-(--surface-input) border-b border-(--border-default) text-(--text-muted)">
              <tr>
                <th class="px-4 py-3 font-semibold w-24">Mã PC</th>
                <th class="px-4 py-3 font-semibold">Lớp & Môn</th>
                <th class="px-4 py-3 font-semibold">Giảng viên</th>
                <th class="px-4 py-3 font-semibold">Ca</th>
                <th class="px-4 py-3 font-semibold">Trạng thái</th>
                <th class="px-4 py-3 font-semibold text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-(--border-default)">
              <tr v-for="item in filteredAssignments" :key="item.id" class="hover:bg-(--surface-hover) transition-colors">
                <td class="px-4 py-3 font-mono text-xs font-medium text-(--text-muted)">{{ item.id }}</td>
                <td class="px-4 py-3">
                  <p class="font-semibold text-(--text-heading)">{{ item.class }}</p>
                  <p class="text-xs text-(--text-muted) mt-0.5">{{ item.subject }}</p>
                </td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-2" :class="item.status === 'unassigned' ? 'text-red-500' : 'text-(--text-heading)'">
                    <UserCheck v-if="item.status === 'assigned'" :size="16" class="text-emerald-500"/>
                    <UserMinus v-else :size="16" class="text-red-500"/>
                    <span class="font-medium">{{ item.teacher }}</span>
                  </div>
                </td>
                <td class="px-4 py-3">
                  <div class="flex items-center gap-1.5 text-xs text-(--text-body)">
                    <Clock :size="14" class="text-(--text-muted)"/> {{ item.schedule }}
                  </div>
                  <div class="flex items-center gap-1.5 text-xs text-(--text-body) mt-1">
                    <MapPin :size="14" class="text-(--text-muted)"/> {{ item.room }}
                  </div>
                </td>
                <td class="px-4 py-3">
                  <GlassBadge :variant="item.status === 'assigned' ? 'success' : 'danger'" size="xs">
                    {{ item.status === 'assigned' ? 'Đã phân công' : 'Chưa phân công' }}
                  </GlassBadge>
                </td>
                <td class="px-4 py-3 text-right">
                  <div class="flex justify-end gap-1">
                    <button class="p-2 rounded-lg hover:bg-(--surface-input-focus) text-(--text-muted) hover:text-(--text-heading) transition-colors" title="Đổi giảng viên" aria-label="Đổi giảng viên">
                      <ArrowLeftRight :size="16" />
                    </button>
                    <button class="p-2 rounded-lg hover:bg-(--surface-input-focus) text-(--text-muted) hover:text-(--text-heading) transition-colors" title="Sửa phân công" aria-label="Sửa phân công">
                      <Pencil :size="16" />
                    </button>
                    <button @click="handleDelete(item)" class="p-2 rounded-lg hover:bg-(--color-danger-bg) hover:text-(--color-danger-text) transition-colors" title="Xóa phân công" aria-label="Xóa phân công">
                      <Trash2 :size="16" />
                    </button>
                  </div>
                </td>
              </tr>
              <tr v-if="filteredAssignments.length === 0">
                <td colspan="6" class="px-4 py-12 text-center text-(--text-muted)">
                  <UserMinus class="mx-auto mb-3 opacity-20" :size="48" />
                  <p>Không tìm thấy phân công giảng viên nào.</p>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <!-- Pagination Placeholder -->
        <div class="border-t border-(--border-default) p-3 flex justify-between items-center bg-(--surface-input)">
           <p class="text-xs text-(--text-muted)">Hiển thị {{ filteredAssignments.length }} kết quả</p>
           <div class="flex gap-1">
              <GlassButton variant="secondary" size="xs">Trước</GlassButton>
              <GlassButton variant="primary" size="xs">1</GlassButton>
              <GlassButton variant="secondary" size="xs">Sau</GlassButton>
           </div>
        </div>
      </div>
    </template>
  </div>

  <ConfirmActionDialog
    :is-open="showDeleteModal"
    title="Xóa phân công"
    :message="`Bạn có chắc chắn muốn xóa phân công cho lớp ${itemToDelete?.class}? Lớp này sẽ trở về trạng thái chưa phân công.`"
    confirm-text="Xóa"
    @confirm="confirmDelete"
    @cancel="showDeleteModal = false"
  />
</template>
