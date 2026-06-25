<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  Search,
  Filter,
  ShieldAlert,
  KeyRound,
  Lock,
  Unlock,
  Edit2,
  Eye,
  UserPlus,
  History,
  CheckCircle2,
  AlertCircle,
  Mail,
  Smartphone,
  MapPin,
  Shield,
  Building2,
  X,
  FileSpreadsheet
} from 'lucide-vue-next'

// Filters
const searchQuery = ref('')
const selectedRole = ref('Tất cả')
const selectedCampus = ref('Tất cả')
const selectedStatus = ref('Tất cả')
const selectedUsers = ref([])

const roles = ['Tất cả', 'Giảng viên', 'Sinh viên', 'Giáo vụ', 'BGH', 'Admin']
const campuses = ['Tất cả', 'Hà Nội', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ', 'Toàn hệ thống']
const statuses = ['Tất cả', 'Active', 'Locked', 'First_login']

// Mock Data
const mockUsers = ref([
  { id: 1, name: 'Nguyễn Văn A', email: 'nguyenvana@example.com', phone: '0901234567', role: 'Sinh viên', campus: 'Hà Nội', status: 'Active', lastLogin: '2026-06-04 08:30:00', createdAt: '2026-01-15' },
  { id: 2, name: 'Trần Thị B', email: 'tranthib@example.com', phone: '0912345678', role: 'Giảng viên', campus: 'TP.HCM', status: 'Active', lastLogin: '2026-06-03 15:45:00', createdAt: '2025-08-20' },
  { id: 3, name: 'Lê Văn C', email: 'levanc@example.com', phone: '0923456789', role: 'Giáo vụ', campus: 'Đà Nẵng', status: 'Locked', lastLogin: '2026-05-20 09:15:00', createdAt: '2024-11-10', lockReason: 'Vi phạm chính sách bảo mật' },
  { id: 4, name: 'Phạm Thị D', email: 'phamthid@example.com', phone: '0934567890', role: 'BGH', campus: 'Hà Nội', status: 'Active', lastLogin: '2026-06-04 10:00:00', createdAt: '2024-05-05' },
  { id: 5, name: 'Hoàng Văn E', email: 'hoangvane@example.com', phone: '0945678901', role: 'Sinh viên', campus: 'Cần Thơ', status: 'First_login', lastLogin: null, createdAt: '2026-06-01' },
  { id: 6, name: 'Ngô Thị F', email: 'ngothif@example.com', phone: '0956789012', role: 'Admin', campus: 'Toàn hệ thống', status: 'Active', lastLogin: '2026-06-04 11:20:00', createdAt: '2023-12-01' },
  { id: 7, name: 'Đỗ Văn G', email: 'dovang@example.com', phone: '0967890123', role: 'Sinh viên', campus: 'Hà Nội', status: 'Active', lastLogin: '2026-06-01 07:15:00', createdAt: '2025-09-05' },
])

const filteredUsers = computed(() => {
  return mockUsers.value.filter(u => {
    const matchSearch = u.name.toLowerCase().includes(searchQuery.value.toLowerCase()) || u.email.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchRole = selectedRole.value === 'Tất cả' || u.role === selectedRole.value
    const matchCampus = selectedCampus.value === 'Tất cả' || u.campus === selectedCampus.value
    const matchStatus = selectedStatus.value === 'Tất cả' || u.status === selectedStatus.value
    return matchSearch && matchRole && matchCampus && matchStatus
  })
})

const getStatusBadge = (status) => {
  if (status === 'Active') return { class: 'bg-emerald-100 text-emerald-700 border-emerald-200', label: 'Đang hoạt động', icon: CheckCircle2 }
  if (status === 'Locked') return { class: 'bg-rose-100 text-rose-700 border-rose-200', label: 'Bị khóa', icon: Lock }
  if (status === 'First_login') return { class: 'bg-amber-100 text-amber-700 border-amber-200', label: 'Đăng nhập lần đầu', icon: ShieldAlert }
  return { class: 'bg-slate-100 text-slate-700 border-slate-200', label: 'Không xác định', icon: AlertCircle }
}

const getRoleClass = (role) => {
  if (role === 'Admin') return 'bg-indigo-100 text-indigo-700'
  if (role === 'BGH') return 'bg-purple-100 text-purple-700'
  if (role === 'Giáo vụ') return 'bg-blue-100 text-blue-700'
  if (role === 'Giảng viên') return 'bg-teal-100 text-teal-700'
  return 'bg-slate-100 text-slate-700'
}

// Modals & Drawer State
const isDrawerOpen = ref(false)
const currentUser = ref(null)

const isResetModalOpen = ref(false)
const resetPasswordUser = ref(null)

const isLockModalOpen = ref(false)
const lockActionUser = ref(null)
const lockReason = ref('')
const lockType = ref('temporary')

const isCreateDrawerOpen = ref(false)
const createImportMode = ref('create')
const newUserForm = ref({ name: '', email: '', password: '', role: 'Sinh viên', campus: 'Hà Nội' })

const isEditDrawerOpen = ref(false)
const editUserForm = ref(null)

// Actions
const openEditDrawer = (user) => {
  editUserForm.value = { ...user }
  isEditDrawerOpen.value = true
}

const confirmEditUser = () => {
  if (!editUserForm.value.name || !editUserForm.value.email) {
    alert('Vui lòng điền đủ Họ tên và Email.')
    return
  }
  const index = mockUsers.value.findIndex(u => u.id === editUserForm.value.id)
  if (index !== -1) {
    mockUsers.value[index] = { ...mockUsers.value[index], ...editUserForm.value }
  }
  alert(`Đã cập nhật thông tin cho ${editUserForm.value.email}!`)
  isEditDrawerOpen.value = false
  isDrawerOpen.value = false
}

const openCreateImportDrawer = () => {
  createImportMode.value = 'create'
  newUserForm.value = { name: '', email: '', password: '', role: 'Sinh viên', campus: 'Hà Nội' }
  isCreateDrawerOpen.value = true
}

const confirmCreateImport = () => {
  if (createImportMode.value === 'create') {
    if (!newUserForm.value.name || !newUserForm.value.email || !newUserForm.value.password) {
      alert('Vui lòng điền đầy đủ thông tin.')
      return
    }
    alert(`Đã tạo tài khoản cho ${newUserForm.value.email} thành công!`)
  } else {
    alert('Đã import danh sách tài khoản thành công!')
  }
  isCreateDrawerOpen.value = false
}

const openDrawer = (user) => {
  currentUser.value = user
  isDrawerOpen.value = true
}

const openResetModal = (user) => {
  resetPasswordUser.value = user
  isResetModalOpen.value = true
}

const confirmResetPassword = () => {
  alert(`Đã gửi email cấp lại mật khẩu cho tài khoản: ${resetPasswordUser.value.email}`)
  isResetModalOpen.value = false
}

const openLockModal = (user) => {
  lockActionUser.value = user
  lockReason.value = user.status === 'Locked' ? '' : ''
  lockType.value = 'temporary'
  isLockModalOpen.value = true
}

const confirmLockAction = () => {
  if (lockActionUser.value.status === 'Locked') {
    lockActionUser.value.status = 'Active'
    lockActionUser.value.lockReason = null
    alert(`Đã mở khóa tài khoản ${lockActionUser.value.email}. Đã ghi Log.`)
  } else {
    if (!lockReason.value.trim()) {
      alert('Super Admin bắt buộc phải nhập lý do khóa tài khoản!')
      return
    }
    lockActionUser.value.status = 'Locked'
    lockActionUser.value.lockReason = lockReason.value
    alert(`Đã khóa tài khoản ${lockActionUser.value.email}. Đã ghi Log.`)
  }
  isLockModalOpen.value = false
}

const toggleSelectAll = (e) => {
  if (e.target.checked) {
    selectedUsers.value = filteredUsers.value.map(u => u.id)
  } else {
    selectedUsers.value = []
  }
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return 'Chưa đăng nhập'
  return new Date(dateStr).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' })
}

const formatDate = (dateStr) => {
  if (!dateStr) return '--'
  return new Date(dateStr).toLocaleDateString('vi-VN')
}

const route = useRoute()
onMounted(() => {
  if (route.query.action === 'import') {
    createImportMode.value = 'import'
    isCreateDrawerOpen.value = true
  }
})
</script>

<template>
  <div class="users-management-page">
    <!-- Header -->
    <header class="page-header mb-6">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-heading">Danh sách người dùng</h1>
          <p class="text-sm text-label mt-1">Toàn quyền quản lý tài khoản, phân quyền và giám sát truy cập hệ thống đa cơ sở.</p>
        </div>
        <div class="flex items-center gap-3">
          <router-link to="/super-admin/login-history" class="glass-btn secondary shadow-sm">
            <History :size="16" /> Lịch sử đăng nhập
          </router-link>
          <button @click="openCreateImportDrawer" class="glass-btn primary shadow-sm">
            <UserPlus :size="16" /> Tạo / Import tài khoản
          </button>
        </div>
      </div>
    </header>

    <!-- Controls (Search & Filters) -->
    <div class="controls-panel glass-panel mb-6 p-4 rounded-2xl flex flex-col lg:flex-row gap-4 items-center justify-between">
      <div class="search-box relative w-full lg:w-80">
        <Search :size="18" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input 
          v-model="searchQuery" 
          type="text" 
          placeholder="Tìm kiếm theo Tên, Email..." 
          class="glass-input w-full pl-10"
        />
      </div>
      
      <div class="filters flex flex-wrap items-center gap-3 w-full lg:w-auto">
        <div class="filter-group">
          <Filter :size="14" class="text-label" />
          <select v-model="selectedRole" class="glass-select">
            <option v-for="r in roles" :key="r" :value="r">{{ r === 'Tất cả' ? 'Tất cả vai trò' : r }}</option>
          </select>
        </div>
        <div class="filter-group">
          <Building2 :size="14" class="text-label" />
          <select v-model="selectedCampus" class="glass-select">
            <option v-for="c in campuses" :key="c" :value="c">{{ c === 'Tất cả' ? 'Tất cả cơ sở' : c }}</option>
          </select>
        </div>
        <div class="filter-group">
          <Shield :size="14" class="text-label" />
          <select v-model="selectedStatus" class="glass-select">
            <option v-for="s in statuses" :key="s" :value="s">{{ s === 'Tất cả' ? 'Tất cả trạng thái' : (s === 'First_login' ? 'Đăng nhập lần đầu' : s) }}</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Bulk Actions (hiển thị khi có item được chọn) -->
    <transition name="fade">
      <div v-if="selectedUsers.length > 0" class="bulk-actions mb-4 p-3 rounded-xl bg-blue-50 border border-blue-100 flex items-center justify-between">
        <span class="text-sm font-semibold text-blue-800">Đã chọn {{ selectedUsers.length }} tài khoản</span>
        <div class="flex gap-2">
          <button class="glass-btn secondary text-xs py-1.5 px-3"><Mail :size="14" /> Gửi thông báo</button>
          <button class="glass-btn danger text-xs py-1.5 px-3"><Lock :size="14" /> Khóa hàng loạt</button>
        </div>
      </div>
    </transition>

    <!-- Data Table -->
    <div class="table-container glass-panel rounded-2xl overflow-hidden">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr>
            <th class="w-12 text-center">
              <input type="checkbox" class="glass-checkbox" :checked="selectedUsers.length === filteredUsers.length && filteredUsers.length > 0" @change="toggleSelectAll" />
            </th>
            <th>Thông tin định danh</th>
            <th>Vai trò & Cơ sở</th>
            <th>Trạng thái</th>
            <th>Dữ liệu truy cập</th>
            <th class="text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="filteredUsers.length === 0">
            <td colspan="6" class="text-center py-10 text-placeholder">Không tìm thấy người dùng nào phù hợp.</td>
          </tr>
          <tr v-for="user in filteredUsers" :key="user.id" class="hover:bg-slate-50/50 transition border-t border-slate-100/50">
            <td class="text-center">
              <input type="checkbox" class="glass-checkbox" :value="user.id" v-model="selectedUsers" />
            </td>
            <td>
              <div class="flex items-center gap-3">
                <div class="avatar bg-gradient-to-br from-blue-500 to-indigo-600 text-white flex items-center justify-center rounded-full font-bold w-10 h-10 shadow-sm shrink-0">
                  {{ user.name.charAt(0) }}
                </div>
                <div>
                  <div class="font-bold text-heading text-sm">{{ user.name }}</div>
                  <div class="text-xs text-label flex items-center gap-1 mt-0.5"><Mail :size="10" /> {{ user.email }}</div>
                  <div class="text-[11px] text-placeholder flex items-center gap-1"><Smartphone :size="10" /> {{ user.phone }}</div>
                </div>
              </div>
            </td>
            <td>
              <div class="flex flex-col items-start gap-1">
                <span class="role-badge" :class="getRoleClass(user.role)">{{ user.role }}</span>
                <span class="text-xs text-label flex items-center gap-1"><MapPin :size="10" class="text-placeholder" /> {{ user.campus }}</span>
              </div>
            </td>
            <td>
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full border text-xs font-semibold" :class="getStatusBadge(user.status).class">
                <component :is="getStatusBadge(user.status).icon" :size="12" />
                {{ getStatusBadge(user.status).label }}
              </div>
            </td>
            <td>
              <div class="text-xs">
                <div class="text-heading font-medium mb-1" title="Lần đăng nhập cuối">
                  <History :size="10" class="inline text-placeholder mr-1" />{{ formatDateTime(user.lastLogin) }}
                </div>
                <div class="text-label" title="Ngày tạo">
                  Tạo: {{ formatDate(user.createdAt) }}
                </div>
              </div>
            </td>
            <td class="text-right">
              <div class="flex items-center justify-end gap-2">
                <button @click="openDrawer(user)" class="action-btn text-blue-600 hover:bg-blue-50" title="Xem chi tiết">
                  <Eye :size="16" />
                </button>
                <button @click="openEditDrawer(user)" class="action-btn text-teal-600 hover:bg-teal-50" title="Chỉnh sửa & Gán quyền">
                  <Edit2 :size="16" />
                </button>
                <button @click="openResetModal(user)" class="action-btn text-amber-600 hover:bg-amber-50" title="Reset Mật khẩu">
                  <KeyRound :size="16" />
                </button>
                <button @click="openLockModal(user)" class="action-btn hover:bg-rose-50" :class="user.status === 'Locked' ? 'text-emerald-600' : 'text-rose-600'" :title="user.status === 'Locked' ? 'Mở khóa tài khoản' : 'Khóa tài khoản'">
                  <component :is="user.status === 'Locked' ? Unlock : Lock" :size="16" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Dịch chuyển (Teleport) tất cả Modal/Drawer ra ngoài thẻ body để không bị đè bởi Topbar/Sidebar -->
    <Teleport to="body">
      <!-- Drawer Chi tiết User -->
      <div v-if="isDrawerOpen" class="drawer-overlay" @click="isDrawerOpen = false"></div>
    <div class="drawer" :class="{ 'open': isDrawerOpen }">
      <div class="drawer-header">
        <h3 class="font-bold text-heading text-lg">Hồ sơ Người dùng</h3>
        <button @click="isDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
      </div>
      <div class="drawer-body p-6" v-if="currentUser">
        <div class="text-center mb-6">
          <div class="mx-auto bg-gradient-to-br from-blue-500 to-indigo-600 text-white flex items-center justify-center rounded-full font-bold w-20 h-20 shadow-md text-2xl mb-3">
            {{ currentUser.name.charAt(0) }}
          </div>
          <h2 class="text-xl font-bold text-heading">{{ currentUser.name }}</h2>
          <p class="text-sm text-label">{{ currentUser.email }}</p>
          <div class="mt-2 inline-flex items-center gap-1.5 px-3 py-1 rounded-full border text-xs font-semibold" :class="getStatusBadge(currentUser.status).class">
            <component :is="getStatusBadge(currentUser.status).icon" :size="12" />
            {{ getStatusBadge(currentUser.status).label }}
          </div>
        </div>

        <div class="info-section">
          <h4 class="font-semibold text-heading mb-3 flex items-center gap-2"><Shield :size="16" /> Phân quyền & Công tác</h4>
          <div class="info-row"><span class="info-label">Vai trò:</span> <span class="font-semibold text-heading">{{ currentUser.role }}</span></div>
          <div class="info-row"><span class="info-label">Cơ sở (Campus):</span> <span>{{ currentUser.campus }}</span></div>
        </div>

        <div class="info-section mt-6">
          <h4 class="font-semibold text-heading mb-3 flex items-center gap-2"><History :size="16" /> Lịch sử truy cập</h4>
          <div class="info-row"><span class="info-label">Lần đăng nhập cuối:</span> <span>{{ formatDateTime(currentUser.lastLogin) }}</span></div>
          <div class="info-row"><span class="info-label">Ngày tạo tài khoản:</span> <span>{{ formatDate(currentUser.createdAt) }}</span></div>
        </div>

        <div v-if="currentUser.status === 'Locked'" class="mt-6 p-4 bg-rose-50 border border-rose-200 rounded-xl">
          <h4 class="font-bold text-rose-800 text-sm mb-1 flex items-center gap-1"><Lock :size="14"/> Lý do khóa tài khoản</h4>
          <p class="text-rose-700 text-sm">{{ currentUser.lockReason || 'Không có lý do rõ ràng.' }}</p>
        </div>
        
        <div class="mt-8 flex gap-3">
          <button @click="openEditDrawer(currentUser)" class="glass-btn primary flex-1 justify-center"><Edit2 :size="16" /> Cập nhật</button>
          <button v-if="currentUser.status !== 'Locked'" @click="openLockModal(currentUser)" class="glass-btn danger flex-1 justify-center"><Lock :size="16" /> Khóa TK</button>
          <button v-else @click="openLockModal(currentUser)" class="glass-btn flex-1 justify-center !text-emerald-700 !border-emerald-200 !bg-emerald-50"><Unlock :size="16" /> Mở khóa</button>
        </div>
      </div>
    </div>

    <!-- Modal Reset Mật khẩu -->
    <div v-if="isResetModalOpen" class="modal-overlay">
      <div class="modal-content glass-panel p-6 rounded-2xl max-w-sm w-full">
        <div class="flex items-center justify-center w-12 h-12 rounded-full bg-amber-100 text-amber-600 mb-4 mx-auto">
          <KeyRound :size="24" />
        </div>
        <h3 class="text-lg font-bold text-center text-heading mb-2">Reset Mật Khẩu</h3>
        <p class="text-sm text-center text-label mb-6">Bạn có chắc chắn muốn cấp lại mật khẩu cho tài khoản <strong>{{ resetPasswordUser?.email }}</strong>?</p>
        <div class="flex gap-3">
          <button @click="isResetModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button @click="confirmResetPassword" class="glass-btn primary flex-1 justify-center">Xác nhận</button>
        </div>
      </div>
    </div>

    <!-- Modal Khóa / Mở Khóa Tài Khoản -->
    <div v-if="isLockModalOpen" class="modal-overlay">
      <div class="modal-content glass-panel p-6 rounded-2xl max-w-md w-full">
        <div class="flex items-center gap-3 mb-5 border-b border-slate-100 pb-4">
          <div class="flex items-center justify-center w-12 h-12 rounded-full" :class="lockActionUser?.status === 'Locked' ? 'bg-emerald-100 text-emerald-600' : 'bg-rose-100 text-rose-600'">
            <component :is="lockActionUser?.status === 'Locked' ? Unlock : Lock" :size="24" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-heading">{{ lockActionUser?.status === 'Locked' ? 'Mở Khóa Tài Khoản' : 'Khóa Tài Khoản' }}</h3>
            <p class="text-xs font-semibold text-blue-600 mt-0.5">{{ lockActionUser?.email }}</p>
          </div>
        </div>

        <template v-if="lockActionUser?.status !== 'Locked'">
          <div class="mb-4">
            <div class="bg-rose-50 border border-rose-200 rounded-lg p-3 mb-4">
              <p class="text-xs text-rose-700 font-medium leading-relaxed">Tài khoản này sẽ bị đưa vào trạng thái Locked (Soft delete). Người dùng sẽ không thể truy cập bất kỳ module nào của hệ thống.</p>
            </div>
            <div class="form-group mb-4">
              <label class="block text-xs font-bold text-label mb-1">Thời hạn khóa</label>
              <select v-model="lockType" class="glass-select w-full bg-white">
                <option value="temporary">Khóa tạm thời (Tự động mở sau thời hạn)</option>
                <option value="permanent">Khóa vĩnh viễn (Banned)</option>
              </select>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-rose-600 mb-1">Lý do khóa (Bắt buộc ghi Audit Log) *</label>
              <textarea v-model="lockReason" rows="3" class="glass-input w-full bg-white" placeholder="VD: Vi phạm chính sách bảo mật, nghỉ việc..."></textarea>
            </div>
          </div>
        </template>
        <template v-else>
          <div class="bg-emerald-50 border border-emerald-200 rounded-lg p-4 mb-6">
            <p class="text-sm text-emerald-800 font-semibold mb-1">Khôi phục trạng thái hoạt động</p>
            <p class="text-xs text-emerald-700">Người dùng sẽ có thể sử dụng hệ thống bình thường. Quyền hạn (Role) cũ vẫn được giữ nguyên.</p>
          </div>
        </template>

        <div class="flex gap-3 justify-end mt-2 pt-4 border-t border-slate-100">
          <button @click="isLockModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy bỏ thao tác</button>
          <button @click="confirmLockAction" class="glass-btn flex-1 justify-center" :class="lockActionUser?.status === 'Locked' ? 'primary' : 'danger'">
            <component :is="lockActionUser?.status === 'Locked' ? Unlock : Lock" :size="16" />
            {{ lockActionUser?.status === 'Locked' ? 'Xác nhận Mở Khóa' : 'Xác nhận Khóa (Ghi Log)' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Drawer Tạo / Import Tài khoản -->
    <div v-if="isCreateDrawerOpen" class="drawer-overlay" @click="isCreateDrawerOpen = false"></div>
    <div class="drawer" :class="{ 'open': isCreateDrawerOpen }">
      <div class="drawer-header bg-slate-50/50">
        <h3 class="text-lg font-bold text-heading flex items-center gap-2">
          <UserPlus :size="20" class="text-blue-600"/> Quản lý Tài khoản Mới
        </h3>
        <button @click="isCreateDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
      </div>
      
      <div class="drawer-body p-6">
        <div class="flex border-b border-slate-200 mb-6">
          <button @click="createImportMode = 'create'" class="flex-1 pb-3 font-semibold text-sm transition-colors border-b-2 outline-none focus:outline-none" :class="createImportMode === 'create' ? 'text-blue-600 border-blue-600' : 'text-slate-400 border-transparent'">Tạo thủ công</button>
          <button @click="createImportMode = 'import'" class="flex-1 pb-3 font-semibold text-sm transition-colors border-b-2 outline-none focus:outline-none" :class="createImportMode === 'import' ? 'text-blue-600 border-blue-600' : 'text-slate-400 border-transparent'">Import từ Excel</button>
        </div>

        <template v-if="createImportMode === 'create'">
          <div class="space-y-4 mb-6">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Họ và tên *</label>
              <input v-model="newUserForm.name" type="text" class="glass-input w-full bg-white" placeholder="Nhập họ và tên..." />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Email *</label>
              <input v-model="newUserForm.email" type="email" class="glass-input w-full bg-white" placeholder="email@fpt.edu.vn" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Mật khẩu *</label>
              <div class="relative">
                <input v-model="newUserForm.password" type="text" class="glass-input w-full bg-white" placeholder="Nhập mật khẩu..." />
                <KeyRound :size="16" class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder" />
              </div>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Vai trò</label>
                <select v-model="newUserForm.role" class="glass-select w-full bg-white">
                  <option v-for="r in roles.filter(r => r !== 'Tất cả')" :key="r" :value="r">{{ r }}</option>
                </select>
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Cơ sở trực thuộc</label>
                <select v-model="newUserForm.campus" class="glass-select w-full bg-white">
                  <option v-for="c in campuses.filter(c => c !== 'Tất cả')" :key="c" :value="c">{{ c }}</option>
                </select>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <div class="border-2 border-dashed border-blue-200 bg-blue-50/50 rounded-xl p-8 text-center mb-6 flex flex-col items-center justify-center">
            <div class="w-12 h-12 bg-white rounded-full flex items-center justify-center shadow-sm mb-3">
              <FileSpreadsheet :size="24" class="text-blue-500" />
            </div>
            <h4 class="font-bold text-sm text-heading mb-1">Kéo thả file Excel vào đây</h4>
            <p class="text-xs text-label mb-4">Hỗ trợ các định dạng .xlsx, .csv (Tối đa 5MB)</p>
            <button class="glass-btn secondary text-xs">Chọn file từ máy tính</button>
          </div>
          <div class="bg-amber-50 border border-amber-200 rounded-lg p-3">
            <p class="text-xs text-amber-800 font-semibold flex items-center gap-1"><AlertCircle :size="14"/> Hướng dẫn Import:</p>
            <ul class="text-[10px] text-amber-700 mt-1 pl-4 list-disc">
              <li>Mật khẩu mặc định sẽ được khởi tạo tự động và gửi qua email cho từng người (Hoặc dùng cột MatKhau nếu có).</li>
              <li>Kiểm tra kỹ đúng định dạng cột: HoTen, Email, SoDienThoai, VaiTro, CoSo.</li>
            </ul>
          </div>
        </template>

        <div class="flex gap-3 mt-6 border-t border-slate-100 pt-4">
          <button @click="isCreateDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button @click="confirmCreateImport" class="glass-btn primary flex-1 justify-center" :disabled="createImportMode === 'create' && (!newUserForm.name || !newUserForm.email || !newUserForm.password)" :class="{'opacity-50 cursor-not-allowed': createImportMode === 'create' && (!newUserForm.name || !newUserForm.email || !newUserForm.password)}">
            <component :is="createImportMode === 'create' ? UserPlus : FileSpreadsheet" :size="16" />
            {{ createImportMode === 'create' ? 'Tạo tài khoản' : 'Bắt đầu Import' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Drawer Chỉnh sửa thông tin tài khoản -->
    <div v-if="isEditDrawerOpen" class="drawer-overlay" @click="isEditDrawerOpen = false"></div>
    <div class="drawer" :class="{ 'open': isEditDrawerOpen }">
      <div class="drawer-header bg-slate-50/50">
        <h3 class="text-lg font-bold text-heading flex items-center gap-2">
          <Edit2 :size="20" class="text-teal-600"/> Chỉnh sửa Thông tin
        </h3>
        <button @click="isEditDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
      </div>
      
      <div class="drawer-body p-6" v-if="editUserForm">
        <div class="space-y-4 mb-6">
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Họ và tên *</label>
            <input v-model="editUserForm.name" type="text" class="glass-input w-full bg-white" />
          </div>
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Email *</label>
            <input v-model="editUserForm.email" type="email" class="glass-input w-full bg-white" />
          </div>
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1">Số điện thoại</label>
            <input v-model="editUserForm.phone" type="text" class="glass-input w-full bg-white" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Vai trò</label>
              <select v-model="editUserForm.role" class="glass-select w-full bg-white">
                <option v-for="r in roles.filter(r => r !== 'Tất cả')" :key="r" :value="r">{{ r }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Cơ sở trực thuộc</label>
              <select v-model="editUserForm.campus" class="glass-select w-full bg-white">
                <option v-for="c in campuses.filter(c => c !== 'Tất cả')" :key="c" :value="c">{{ c }}</option>
              </select>
            </div>
          </div>
        </div>

        <div class="flex gap-3 justify-end mt-6 border-t border-slate-100 pt-4">
          <button @click="isEditDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button @click="confirmEditUser" class="glass-btn primary flex-1 justify-center !bg-teal-600 hover:!bg-teal-700 !border-teal-600" :disabled="!editUserForm?.name || !editUserForm?.email">
            <Edit2 :size="16" /> Lưu thay đổi
          </button>
        </div>
      </div>
    </div>
    </Teleport>
  </div>
</template>

<style scoped>
.page-header {
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--border-default);
}

.glass-panel {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(12px);
}

.glass-btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 10px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.glass-btn.primary {
  background: var(--text-link);
  color: white;
}
.glass-btn.primary:hover { background: #1d4ed8; }

.glass-btn.secondary {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
.glass-btn.secondary:hover { background: var(--surface-input-focus); }

.glass-btn.danger {
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
  border-color: rgba(220, 38, 38, 0.2);
}
.glass-btn.danger:hover { background: rgba(220, 38, 38, 0.2); }

.glass-input, .glass-select {
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  padding: 0.5rem 0.75rem;
  border-radius: 10px;
  color: var(--text-heading);
  font-size: 0.8rem;
  outline: none;
  transition: all 0.2s;
}

.glass-input:focus, .glass-select:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
  background: var(--surface-input-focus);
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: var(--surface-input);
  padding: 0.25rem;
  padding-left: 0.75rem;
  border-radius: 12px;
  border: 1px solid var(--border-input);
}

.filter-group .glass-select {
  border: none;
  background: transparent;
  padding: 0.25rem 0.5rem;
  box-shadow: none;
}

.glass-checkbox {
  width: 1rem;
  height: 1rem;
  border-radius: 4px;
  cursor: pointer;
  accent-color: var(--text-link);
}

.role-badge {
  font-size: 0.65rem;
  font-weight: 800;
  padding: 0.15rem 0.5rem;
  border-radius: 6px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.action-btn {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transition: all 0.2s;
}

/* Table Styles */
.table-container {
  overflow-x: auto;
}
th {
  padding: 1rem;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--text-label);
  background: var(--surface-input);
  border-bottom: 1px solid var(--border-default);
}
td {
  padding: 1rem;
  vertical-align: middle;
}

/* Drawer */
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  backdrop-filter: blur(2px);
  z-index: 9990;
}
.drawer {
  position: fixed;
  top: 0;
  right: -400px;
  width: 100%;
  max-width: 400px;
  height: 100vh;
  background: var(--surface-solid);
  box-shadow: -10px 0 30px rgba(0,0,0,0.1);
  z-index: 9999;
  transition: right 0.3s ease;
  display: flex;
  flex-direction: column;
}
.drawer.open {
  right: 0;
}
.drawer-header {
  padding: 1.25rem 1.5rem;
  border-bottom: 1px solid var(--border-default);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.drawer-body {
  flex: 1;
  overflow-y: auto;
}
.info-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px dashed var(--border-default);
  font-size: 0.85rem;
}
.info-label {
  color: var(--text-label);
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  backdrop-filter: blur(4px);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}

/* Transitions */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
