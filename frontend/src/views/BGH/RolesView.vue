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
          <thead class="sticky top-0 bg-(--surface-card) z-10 backdrop-blur-[12px]">
            <tr>
              <th class="px-4 py-3 font-bold text-heading w-24">ID</th>
              <th class="px-4 py-3 font-bold text-heading">Mã Code</th>
              <th class="px-4 py-3 font-bold text-heading">Tên Vai Trò</th>
              <th v-if="canEdit" class="px-4 py-3 font-bold text-heading text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading" class="bg-transparent">
              <td colspan="4" class="py-12 text-center text-muted">
                <Loader2 class="animate-spin mx-auto mb-2" :size="24" />
                <p>Đang tải dữ liệu...</p>
              </td>
            </tr>
            <tr v-else-if="error" class="bg-transparent">
              <td colspan="4" class="py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-3">
                  <AlertCircle :size="24" class="text-(--color-danger-text)" />
                  <p class="text-(--color-danger-text) font-medium">{{ error }}</p>
                  <button @click="fetchRoles()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
                </div>
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
      <div class="p-4 bg-(--surface-card) flex items-center justify-between text-sm">
        <span class="text-muted">
          Tổng số: <span class="font-bold text-heading">{{ filteredRoles.length }}</span> vai trò
        </span>
      </div>
    </div>

    <!-- ── Modal: Role Form ── -->
    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-md surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-full">
        <div class="p-4 flex justify-between items-center">
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

        <div class="p-4 bg-(--surface-card) flex justify-end gap-3">
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
  Loader2, AlertTriangle, AlertCircle, X 
} from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole(['SuperAdmin', 'Admin']))

// --- Data State ---
const rolesList = ref([])
const loading = ref(false)
const error = ref(null)
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

const fetchRoles = async () => {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getRoles()
    rolesList.value = unwrapApiData(res) || []
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu vai trò'
  } finally {
    loading.value = false
  }
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

const submitForm = async () => {
  if (!formData.value.maCodeVaiTro || !formData.value.tenVaiTro) {
    apiError.value = 'Vui lòng điền đầy đủ các trường bắt buộc (*).'
    return
  }
  apiError.value = ''
  saving.value = true

  try {
    if (modalMode.value === 'create') {
      await apiRequest('/api/admin/rbac/roles', {
        method: 'POST',
        body: JSON.stringify({ maCodeVaiTro: formData.value.maCodeVaiTro, tenVaiTro: formData.value.tenVaiTro })
      })
    } else {
      await apiRequest(`/api/admin/rbac/roles/${formData.value.maVaiTro}`, {
        method: 'PUT',
        body: JSON.stringify({ maCodeVaiTro: formData.value.maCodeVaiTro, tenVaiTro: formData.value.tenVaiTro })
      })
    }
    closeModal()
    await fetchRoles()
  } catch (e) {
    apiError.value = e?.message || 'Lỗi lưu dữ liệu'
  } finally {
    saving.value = false
  }
}

const handleDelete = async (role) => {
  saving.value = true
  try {
    await apiRequest(`/api/admin/rbac/roles/${role.maVaiTro}`, { method: 'DELETE' })
    await fetchRoles()
  } catch (e) {
    apiError.value = e?.message || 'Lỗi xóa vai trò'
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchRoles()
})
</script>
