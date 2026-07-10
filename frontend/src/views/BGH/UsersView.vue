<template>
  <div class="space-y-4 pb-10 h-[calc(100vh-8rem)] flex flex-col">
    <div v-if="loading" class="flex-1 p-4">
      <SkeletonTable :rows="8" :columns="6" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Quản lý Người Dùng</h2>
        <p class="text-xs text-muted mt-1">Danh sách tất cả tài khoản trong hệ thống</p>
      </div>
      <button v-if="canEdit" @click="openCreateModal" class="flex items-center gap-2 px-4 py-2 bg-(--lg-primary) hover:bg-(--lg-primary-dark) text-white text-sm font-bold rounded-xl transition-all shadow-sm">
        <Plus :size="18" /> <span>Thêm người dùng</span>
      </button>
    </div>

    <div class="surface-card border border-card rounded-2xl p-4 shadow-sm flex flex-wrap gap-4 items-end">
      <div class="flex-1 min-w-[200px]">
        <label class="block text-xs font-bold text-heading mb-1.5">Tìm kiếm</label>
        <div class="relative">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" :size="16" />
          <input v-model="keyword" @keyup.enter="handleFilter" type="text" placeholder="Tên, Email, SĐT..." class="w-full pl-9 pr-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
        </div>
      </div>
      <div class="w-full sm:w-48">
        <label class="block text-xs font-bold text-heading mb-1.5">Vai trò</label>
        <select v-model="roleFilter" @change="handleFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả vai trò</option>
          <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
        </select>
      </div>
      <div class="w-full sm:w-40">
        <label class="block text-xs font-bold text-heading mb-1.5">Trạng thái</label>
        <select v-model="statusFilter" @change="handleFilter" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả trạng thái</option>
          <option value="hoat_dong">Hoạt động</option>
          <option value="bi_khoa">Bị khóa</option>
        </select>
      </div>
      <button @click="handleFilter" class="px-4 py-2 bg-(--surface-input) border border-input hover:bg-(--surface-input-hover) text-heading text-sm font-bold rounded-lg transition-colors h-10">Lọc dữ liệu</button>
    </div>

    <div class="flex-1 surface-card border border-card rounded-2xl shadow-sm flex flex-col overflow-hidden">
      <div class="flex-1 overflow-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="sticky top-0 bg-(--surface-card) z-10 backdrop-blur-[12px]">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">Mã / ID</th>
              <th class="px-4 py-3 font-bold text-heading">Họ tên</th>
              <th class="px-4 py-3 font-bold text-heading">Email</th>
              <th class="px-4 py-3 font-bold text-heading">Vai trò</th>
              <th class="px-4 py-3 font-bold text-heading">Đơn vị</th>
              <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
              <th v-if="canEdit" class="px-4 py-3 font-bold text-heading text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredUsers.length === 0" class="bg-transparent">
              <td colspan="7" class="py-12 text-center text-muted"><p>Không tìm thấy người dùng nào.</p></td>
            </tr>
            <tr v-for="user in pagedUsers" :key="user.maNguoiDung" class="hover:bg-(--surface-input)/50 transition-colors">
              <td class="px-4 py-3 font-medium">{{ user.maNguoiDung }}</td>
              <td class="px-4 py-3 font-bold text-heading">{{ user.hoTen }}</td>
              <td class="px-4 py-3">{{ user.email }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ user.tenVaiTro }}</span>
              </td>
              <td class="px-4 py-3 text-xs">{{ user.tenDonVi || 'N/A' }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center gap-1 px-2 py-1 rounded-md text-[10px] font-bold uppercase tracking-wider" :class="user.trangThai === 'hoat_dong' ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)'">
                  <CheckCircle2 v-if="user.trangThai === 'hoat_dong'" :size="12" />
                  <Lock v-else :size="12" />
                  {{ user.trangThai === 'hoat_dong' ? 'Hoạt động' : 'Bị khóa' }}
                </span>
              </td>
              <td v-if="canEdit" class="px-4 py-3 text-right">
                <div class="flex items-center justify-end gap-2">
                  <button @click="openEditModal(user)" class="p-1.5 text-muted hover:text-(--lg-primary) hover:bg-(--lg-primary)/10 rounded-lg transition-colors" title="Chỉnh sửa"><Edit2 :size="16" /></button>
                  <button v-if="user.trangThai === 'hoat_dong'" @click="handleToggleLock(user)" class="p-1.5 text-muted hover:text-(--color-danger-text) hover:bg-(--color-danger-bg) rounded-lg transition-colors" title="Khóa tài khoản"><Lock :size="16" /></button>
                  <button v-else @click="handleToggleLock(user)" class="p-1.5 text-(--color-danger-text) hover:text-(--color-success-text) hover:bg-(--color-success-bg) rounded-lg transition-colors" title="Mở khóa tài khoản"><Unlock :size="16" /></button>
                  <button @click="handleResetPassword(user)" class="p-1.5 text-muted hover:text-(--color-warning-text) hover:bg-(--color-warning-bg) rounded-lg transition-colors" title="Đặt lại mật khẩu"><Key :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="p-4 bg-(--surface-card) flex items-center justify-between text-sm">
        <span class="text-muted">Hiển thị {{ pagedUsers.length }} / {{ filteredUsers.length }} người dùng</span>
        <div class="flex items-center gap-2">
          <button @click="prevPage" :disabled="currentPage === 1" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang trước</button>
          <span class="px-2 font-bold text-heading">Trang {{ currentPage }} / {{ totalPages }}</span>
          <button @click="nextPage" :disabled="currentPage >= totalPages" class="px-3 py-1.5 rounded-lg border border-default hover:bg-(--surface-input) disabled:opacity-50 disabled:cursor-not-allowed font-bold">Trang sau</button>
        </div>
      </div>
    </div>

    <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-lg surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col max-h-full">
        <div class="p-4 border-b border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-heading">{{ modalMode === 'create' ? 'Thêm Người Dùng Mới' : 'Chỉnh Sửa Người Dùng' }}</h3>
          <button @click="closeModal" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted"><X :size="20" /></button>
        </div>
        <form @submit.prevent="submitForm" class="p-6 overflow-y-auto space-y-4">
          <div v-if="apiError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start">
            <AlertTriangle :size="16" class="shrink-0 mt-0.5" /><span>{{ apiError }}</span>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Họ và tên <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.hoTen" type="text" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Email <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.email" type="email" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Số điện thoại</label>
            <input v-model="formData.soDienThoai" type="text" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div v-if="modalMode === 'create'">
            <label class="block text-xs font-bold text-heading mb-1.5">Mật khẩu <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="formData.matKhau" type="password" required minlength="8" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Vai trò <span class="text-(--color-danger-text)">*</span></label>
            <select v-model="formData.maCodeVaiTro" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none">
              <option value="" disabled>-- Chọn vai trò --</option>
              <option v-for="r in rolesList" :key="r.maCodeVaiTro" :value="r.maCodeVaiTro">{{ r.tenVaiTro }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Đơn vị <span class="text-(--color-danger-text)">*</span></label>
            <select v-model="formData.maDonVi" required class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm focus:border-(--lg-primary) outline-none">
              <option value="" disabled>-- Chọn đơn vị --</option>
              <option v-for="org in orgsList" :key="org.maDonVi" :value="org.maDonVi">{{ org.tenDonVi }} ({{ org.capDonVi }})</option>
            </select>
          </div>
        </form>
        <div class="p-4 border-t border-default bg-(--surface-card) flex justify-end gap-3">
          <button @click="closeModal" type="button" class="px-4 py-2 text-sm font-bold border border-input rounded-lg hover:bg-(--surface-input) transition-colors">Hủy</button>
          <button @click="submitForm" class="flex items-center justify-center gap-2 px-6 py-2 bg-(--lg-primary) text-white text-sm font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors min-w-[100px]">Lưu lại</button>
        </div>
      </div>
    </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, Edit2, Lock, Unlock, Key, CheckCircle2, AlertTriangle, AlertCircle, X } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import { bghApi } from '@/services/bghApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole(['SuperAdmin', 'Admin']))

const loading = ref(false)
const error = ref(null)

const keyword = ref('')
const roleFilter = ref('')
const statusFilter = ref('')
const currentPage = ref(1)
const pageSize = 15

const showModal = ref(false)
const modalMode = ref('create')
const apiError = ref('')
const saving = ref(false)
const formData = ref({ maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' })

const rolesList = ref([])
const orgsList = ref([])
const users = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const [userRes, roleRes, orgRes] = await Promise.all([
      bghApi.getUsers(),
      bghApi.getRoles(),
      bghApi.getOrganizations(),
    ])
    users.value = unwrapApiData(userRes) || []
    rolesList.value = (unwrapApiData(roleRes) || []).map(r => ({ maVaiTro: r.maVaiTro, maCodeVaiTro: r.maCodeVaiTro, tenVaiTro: r.tenVaiTro }))
    orgsList.value = (unwrapApiData(orgRes) || []).map(o => ({ maDonVi: o.id, tenDonVi: o.name, capDonVi: o.organizationLevel }))
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu người dùng'
  } finally {
    loading.value = false
  }
}

const filteredUsers = computed(() => {
  return users.value.filter(u => {
    if (keyword.value) {
      const kw = keyword.value.toLowerCase()
      if (!u.hoTen.toLowerCase().includes(kw) && !u.email.toLowerCase().includes(kw) && !(u.soDienThoai || '').includes(kw)) return false
    }
    if (roleFilter.value && u.vaiTroChinh !== roleFilter.value) return false
    if (statusFilter.value && u.trangThai !== statusFilter.value) return false
    return true
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredUsers.value.length / pageSize)))

const pagedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredUsers.value.slice(start, start + pageSize)
})

function handleFilter() { currentPage.value = 1 }
function prevPage() { if (currentPage.value > 1) currentPage.value-- }
function nextPage() { if (currentPage.value < totalPages.value) currentPage.value++ }

function openCreateModal() {
  if (!canEdit.value) return
  modalMode.value = 'create'
  formData.value = { maNguoiDung: null, hoTen: '', email: '', soDienThoai: '', matKhau: '', maCodeVaiTro: '', maDonVi: '' }
  apiError.value = ''
  showModal.value = true
}

function openEditModal(user) {
  if (!canEdit.value) return
  modalMode.value = 'edit'
  apiError.value = ''
  formData.value = {
    maNguoiDung: user.maNguoiDung,
    hoTen: user.hoTen,
    email: user.email,
    soDienThoai: user.soDienThoai || '',
    matKhau: '',
    maCodeVaiTro: user.vaiTroChinh,
    maDonVi: user.maDonVi
  }
  showModal.value = true
}

function closeModal() { showModal.value = false }

async function submitForm() {
  if (!canEdit.value) return
  if (!formData.value.hoTen || !formData.value.email || !formData.value.maCodeVaiTro || !formData.value.maDonVi) {
    apiError.value = 'Vui lòng điền đầy đủ các trường bắt buộc (*).'
    return
  }
  if (modalMode.value === 'create' && !formData.value.matKhau) {
    apiError.value = 'Vui lòng nhập mật khẩu.'
    return
  }
  apiError.value = ''
  saving.value = true
  try {
    if (modalMode.value === 'create') {
      await apiRequest('/api/admin/users', {
        method: 'POST',
        body: JSON.stringify({
          hoTen: formData.value.hoTen,
          email: formData.value.email,
          soDienThoai: formData.value.soDienThoai,
          matKhau: formData.value.matKhau,
          maCodeVaiTro: formData.value.maCodeVaiTro,
          maDonVi: parseInt(formData.value.maDonVi),
        })
      })
    } else {
      await apiRequest(`/api/admin/users/${formData.value.maNguoiDung}`, {
        method: 'PUT',
        body: JSON.stringify({
          hoTen: formData.value.hoTen,
          email: formData.value.email,
          soDienThoai: formData.value.soDienThoai,
          maCodeVaiTro: formData.value.maCodeVaiTro,
          maDonVi: parseInt(formData.value.maDonVi),
        })
      })
    }
    closeModal()
    await loadData()
  } catch (e) {
    apiError.value = e?.message || 'Lỗi lưu dữ liệu'
  } finally {
    saving.value = false
  }
}

async function handleToggleLock(user) {
  if (!canEdit.value) return
  const isLocking = user.trangThai === 'hoat_dong'
  try {
    await apiRequest(`/api/admin/users/${user.maNguoiDung}/${isLocking ? 'lock' : 'unlock'}`, { method: 'PATCH' })
    await loadData()
  } catch (e) {
    apiError.value = e?.message || 'Lỗi thực hiện thao tác'
  }
}

async function handleResetPassword(user) {
  if (!canEdit.value) return
  const newPassword = prompt(`Nhập mật khẩu mới cho ${user.email} (tối thiểu 8 ký tự):`)
  if (!newPassword || newPassword.length < 8) return
  try {
    await apiRequest(`/api/admin/users/${user.maNguoiDung}/reset-password`, {
      method: 'PATCH',
      body: JSON.stringify({ matKhauMoi: newPassword })
    })
  } catch (e) {
    apiError.value = e?.message || 'Lỗi đặt lại mật khẩu'
  }
}

onMounted(() => { loadData() })
</script>
