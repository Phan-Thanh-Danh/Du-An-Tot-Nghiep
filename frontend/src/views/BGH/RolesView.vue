<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    
    <!-- ── Header & Actions ── -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Vai Trò & Phân Quyền</h2>
        <p class="text-xs text-muted mt-1">Quản lý danh mục các vai trò truy cập trong hệ thống</p>
      </div>
      <button 
        v-if="canEdit"
        @click="openCreateModal"
        class="flex items-center gap-2 px-4 py-2 bg-(--lg-primary) hover:bg-(--lg-primary-dark) text-white text-sm font-bold rounded-xl transition-all shadow-sm"
      >
        <Plus :size="18" />
        <span>Thêm vai trò</span>
      </button>
    </div>

    <!-- ── Filter Bar (Mô phỏng bộ lọc cơ bản) ── -->
    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-4 items-end">
      <!-- Keyword -->
      <div class="flex-1 min-w-[200px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Tìm kiếm vai trò</label>
        <div class="relative">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Tìm theo tên hoặc mã code..."
            class="w-full pl-9 pr-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)"
          />
        </div>
      </div>
    </div>

    <!-- ── Data Table ── -->
    <div class="flex-1 surface-card border border-card rounded-2xl shadow-sm flex flex-col overflow-hidden">
      <div class="flex-1 overflow-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="sticky top-0 bg-(--surface-card) border-b border-default z-10 backdrop-blur-[12px]">
            <tr>
              <th class="px-4 py-3 font-bold text-heading w-24">ID</th>
              <th class="px-4 py-3 font-bold text-heading">Mã Code</th>
              <th class="px-4 py-3 font-bold text-heading">Tên Vai Trò</th>
              <th v-if="canEdit" class="px-4 py-3 font-bold text-heading text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="loading" class="bg-transparent">
              <td colspan="4" class="py-12 text-center text-muted">
                <Loader2 class="animate-spin mx-auto mb-2" :size="24" />
                <p>Đang tải dữ liệu...</p>
              </td>
            </tr>
            <tr v-else-if="filteredRoles.length === 0" class="bg-transparent">
              <td colspan="4" class="py-12 text-center text-muted">
                <p>Không tìm thấy vai trò nào.</p>
              </td>
            </tr>
            <tr 
              v-else 
              v-for="role in filteredRoles" 
              :key="role.maVaiTro"
              class="hover:bg-(--surface-input)/50 transition-colors"
            >
              <td class="px-4 py-3 font-medium">{{ role.maVaiTro }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">
                  {{ role.maCodeVaiTro }}
                </span>
              </td>
              <td class="px-4 py-3 font-bold text-heading">{{ role.tenVaiTro }}</td>
              <td v-if="canEdit" class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEditModal(role)" class="p-1.5 text-muted hover:text-(--lg-primary) hover:bg-(--lg-primary)/10 rounded-lg transition-colors" title="Chỉnh sửa">
                    <Edit2 :size="16" />
                  </button>
                  <button @click="handleDelete(role)" class="p-1.5 text-muted hover:text-(--color-danger-text) hover:bg-(--color-danger-bg) rounded-lg transition-colors" title="Xóa vai trò">
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination (Tĩnh vì số lượng roles thường ít) -->
      <div class="p-4 border-t border-default bg-(--surface-card) flex items-center justify-between text-sm">
        <span class="text-muted">
          Tổng số: <span class="font-bold text-heading">{{ filteredRoles.length }}</span> vai trò
        </span>
      </div>
    </div>

    <!-- ── Modal: Role Form ── -->
    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-md surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-full">
        <div class="p-4 border-b border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">{{ modalMode === 'create' ? 'Thêm Vai Trò Mới' : 'Chỉnh Sửa Vai Trò' }}</h3>
          <button @click="closeModal" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted">
            <X :size="20" />
          </button>
        </div>
        
        <form @submit.prevent="submitForm" class="p-6 overflow-y-auto space-y-4">
          <div v-if="apiError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start">
            <AlertTriangle :size="16" class="shrink-0 mt-0.5" />
            <span>{{ apiError }}</span>
          </div>

          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Mã Code Vai Trò <span class="text-(--color-danger-text)">*</span></label>
            <input 
              v-model="formData.maCodeVaiTro" 
              type="text" 
              required 
              placeholder="VD: SuperAdmin, Student..."
              class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" 
            />
          </div>

          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Tên Vai Trò <span class="text-(--color-danger-text)">*</span></label>
            <input 
              v-model="formData.tenVaiTro" 
              type="text" 
              required 
              placeholder="VD: Quản trị viên, Sinh viên..."
              class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" 
            />
          </div>
        </form>

        <div class="p-4 border-t border-default bg-(--surface-card) flex justify-end gap-3">
          <button @click="closeModal" type="button" class="px-4 py-2 text-sm font-bold border border-input rounded-lg hover:bg-(--surface-input) transition-colors">
            Hủy
          </button>
          <button @click="submitForm" :disabled="saving" class="flex items-center justify-center gap-2 px-6 py-2 bg-(--lg-primary) text-white text-sm font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors disabled:opacity-50 min-w-[100px]">
            <Loader2 v-if="saving" :size="16" class="animate-spin" />
            <span v-else>Lưu lại</span>
          </button>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Search, Plus, Edit2, Trash2,
  Loader2, AlertTriangle, X 
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole(['SuperAdmin', 'Admin']))

let nextRoleId = 100

const mockRoles = [
  { maVaiTro: 1, maCodeVaiTro: 'sieu_quan_tri', tenVaiTro: 'Siêu quản trị' },
  { maVaiTro: 2, maCodeVaiTro: 'quan_tri', tenVaiTro: 'Quản trị hệ thống' },
  { maVaiTro: 3, maCodeVaiTro: 'quan_tri_co_so', tenVaiTro: 'Quản trị cơ sở' },
  { maVaiTro: 4, maCodeVaiTro: 'nhan_vien', tenVaiTro: 'Giáo vụ' },
  { maVaiTro: 5, maCodeVaiTro: 'hieu_truong', tenVaiTro: 'Ban Giám Hiệu' },
  { maVaiTro: 6, maCodeVaiTro: 'giao_vien', tenVaiTro: 'Giảng viên' },
  { maVaiTro: 7, maCodeVaiTro: 'hoc_sinh', tenVaiTro: 'Sinh viên' },
  { maVaiTro: 8, maCodeVaiTro: 'chu_tich', tenVaiTro: 'Chủ tịch hệ thống' },
  { maVaiTro: 9, maCodeVaiTro: 'admin_tai_chinh', tenVaiTro: 'Admin tài chính' },
  { maVaiTro: 10, maCodeVaiTro: 'ke_toan_co_so', tenVaiTro: 'Kế toán cơ sở' },
]

// --- Data State ---
const rolesList = ref([])
const loading = ref(false)
const searchQuery = ref('')

const filteredRoles = computed(() => {
  if (!searchQuery.value) return rolesList.value
  const query = searchQuery.value.toLowerCase()
  return rolesList.value.filter(r => 
    r.maCodeVaiTro.toLowerCase().includes(query) || 
    r.tenVaiTro.toLowerCase().includes(query)
  )
})

// --- Modal State ---
const showModal = ref(false)
const modalMode = ref('create')
const saving = ref(false)
const apiError = ref('')
const formData = ref({
  maVaiTro: null,
  maCodeVaiTro: '',
  tenVaiTro: ''
})

// --- Methods ---

const fetchRoles = () => {
  loading.value = true
  setTimeout(() => {
    rolesList.value = [...mockRoles]
    loading.value = false
  }, 200)
}

const openCreateModal = () => {
  modalMode.value = 'create'
  formData.value = { maVaiTro: null, maCodeVaiTro: '', tenVaiTro: '' }
  apiError.value = ''
  showModal.value = true
}

const openEditModal = (role) => {
  modalMode.value = 'edit'
  apiError.value = ''
  formData.value = { maVaiTro: role.maVaiTro, maCodeVaiTro: role.maCodeVaiTro, tenVaiTro: role.tenVaiTro }
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
}

const submitForm = () => {
  if (!formData.value.maCodeVaiTro || !formData.value.tenVaiTro) {
    apiError.value = 'Vui lòng điền đầy đủ các trường bắt buộc (*).'
    return
  }
  apiError.value = ''

  if (modalMode.value === 'create') {
    const newRole = { maVaiTro: nextRoleId++, maCodeVaiTro: formData.value.maCodeVaiTro, tenVaiTro: formData.value.tenVaiTro }
    mockRoles.push(newRole)
  } else {
    const role = mockRoles.find(r => r.maVaiTro === formData.value.maVaiTro)
    if (role) {
      role.maCodeVaiTro = formData.value.maCodeVaiTro
      role.tenVaiTro = formData.value.tenVaiTro
    }
  }

  closeModal()
  fetchRoles()
}

const handleDelete = (role) => {
    if (import.meta.env.VITE_MOCK === 'true' /* confirm(`Bạn có chắc chắn muốn xóa vai trò "${role.tenVaiTro}"? Hành động này có thể ảnh hưởng lớn đến hệ thống!`) */) return
  const idx = mockRoles.findIndex(r => r.maVaiTro === role.maVaiTro)
  if (idx !== -1) mockRoles.splice(idx, 1)
  fetchRoles()
}

onMounted(() => {
  fetchRoles()
})
</script>
