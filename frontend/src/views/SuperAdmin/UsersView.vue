<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { usePopupStore } from '@/stores/popup'
import { adminUserApi } from '@/services/adminUserService'
import { organizationApi } from '@/services/organizationService'
import { classApi } from '@/services/classApi'
import LmsSelect from '@/components/LmsSelect.vue'
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
  FileSpreadsheet,
  ChevronLeft,
  ChevronRight,
  RefreshCw,
  Sparkles
} from 'lucide-vue-next'

const popup = usePopupStore()

// State & Filters
const searchQuery = ref('')
const selectedRole = ref('Tất cả')
const selectedCampus = ref('Tất cả')
const selectedStatus = ref('Tất cả')
const selectedUsers = ref([])

// Pagination
const pageIndex = ref(1)
const pageSize = ref(10)
const totalItems = ref(0)
const totalPages = ref(1)

// Dynamic Lists from Backend DB
const availableOrganizations = ref([])
const availableRoles = ref([])
const availableClasses = ref([])

const roleFilterOptions = computed(() => [
  { value: 'Tất cả', label: 'Tất cả vai trò' },
  ...availableRoles.value.map(r => ({ value: r.code, label: r.name }))
])

const campusFilterOptions = computed(() => [
  { value: 'Tất cả', label: 'Tất cả cơ sở' },
  ...availableOrganizations.value.map(c => ({ value: c.id, label: c.name }))
])

const statusFilterOptions = [
  { value: 'Tất cả', label: 'Tất cả trạng thái' },
  { value: 'active', label: 'Đang hoạt động' },
  { value: 'khoa', label: 'Bị khóa' },
  { value: 'chua_kich_hoat', label: 'Đăng nhập lần đầu' }
]

const pageSizeOptions = [
  { value: 10, label: '10 / trang' },
  { value: 25, label: '25 / trang' },
  { value: 50, label: '50 / trang' },
  { value: 100, label: '100 / trang' }
]

const formRoleOptions = computed(() => 
  availableRoles.value.map(r => ({ value: r.id, label: r.name }))
)

const formCampusOptions = computed(() => 
  availableOrganizations.value.map(c => ({ value: c.id, label: c.name }))
)

const formClassOptions = computed(() => {
  return availableClasses.value.map(c => ({ value: c.maLop, label: c.tenLop }))
})

const isNewUserStudent = computed(() => {
  const role = availableRoles.value.find(r => r.id === newUserForm.value.maVaiTro)
  return role && role.code === 'Student'
})

const isEditUserStudent = computed(() => {
  const role = availableRoles.value.find(r => r.id === editUserForm.value?.maVaiTro)
  return role && role.code === 'Student'
})

const loading = ref(false)
const error = ref('')

const users = ref([])

// Helper to flatten organization tree for selects
function flattenOrganizations(tree) {
  const result = []
  function traverse(nodes, depth = 0) {
    if (!nodes) return
    const list = Array.isArray(nodes) ? nodes : [nodes]
    for (const node of list) {
      if (!node) continue
      const id = node.id ?? node.Id ?? node.maDonVi
      const name = node.name ?? node.Name ?? node.tenDonVi ?? ''
      const prefix = depth > 0 ? '— '.repeat(depth) : ''
      if (id && name) {
        result.push({ id, name, displayName: `${prefix}${name}` })
      }
      const children = node.children ?? node.Children ?? node.cacDonViCon
      if (children && children.length > 0) {
        traverse(children, depth + 1)
      }
    }
  }
  traverse(tree)
  return result
}

async function loadOrganizations() {
  try {
    const data = await organizationApi.getTree()
    const rawTree = Array.isArray(data) ? data : (data?.items ?? data?.data ?? (data ? [data] : []))
    availableOrganizations.value = flattenOrganizations(rawTree)
    
    if (availableOrganizations.value.length === 0) {
      const allData = await organizationApi.getAll()
      const list = Array.isArray(allData) ? allData : (allData?.data ?? allData?.items ?? [])
      availableOrganizations.value = list.map(o => ({
        id: o.id || o.maDonVi,
        name: o.name || o.tenDonVi,
        displayName: o.name || o.tenDonVi
      }))
    }
  } catch (e) {
    console.error('Không thể nạp danh sách cơ sở:', e)
  }
}


async function loadRoles() {
  try {
    const res = await adminUserApi.getRoles()
    const list = res?.items ?? res?.data ?? res ?? []
    if (Array.isArray(list) && list.length > 0) {
      availableRoles.value = list.map(r => ({
        id: r.maVaiTro || r.id,
        code: r.maCodeVaiTro || r.code,
        name: r.tenVaiTro || r.name
      }))
    } else {
      availableRoles.value = [
        { id: 1, code: 'SuperAdmin', name: 'Super Admin' },
        { id: 2, code: 'Admin', name: 'Admin cơ sở' },
        { id: 3, code: 'CampusAdmin', name: 'Campus Admin' },
        { id: 4, code: 'BGH', name: 'Ban Giám Hiệu' },
        { id: 5, code: 'AcademicStaff', name: 'Giáo vụ' },
        { id: 6, code: 'Teacher', name: 'Giảng viên' },
        { id: 7, code: 'Student', name: 'Sinh viên' }
      ]
    }
  } catch (e) {
    availableRoles.value = [
      { id: 1, code: 'SuperAdmin', name: 'Super Admin' },
      { id: 2, code: 'Admin', name: 'Admin cơ sở' },
      { id: 3, code: 'CampusAdmin', name: 'Campus Admin' },
      { id: 4, code: 'BGH', name: 'Ban Giám Hiệu' },
      { id: 5, code: 'AcademicStaff', name: 'Giáo vụ' },
      { id: 6, code: 'Teacher', name: 'Giảng viên' },
      { id: 7, code: 'Student', name: 'Sinh viên' }
    ]
  }
}

async function loadClasses() {
  try {
    const res = await classApi.list()
    availableClasses.value = Array.isArray(res) ? res : (res?.items ?? [])
  } catch (e) {
    console.error('Không thể nạp danh sách lớp:', e)
  }
}

async function loadUsers() {
  loading.value = true
  error.value = ''
  try {
    const params = {
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
      keyword: searchQuery.value.trim() || undefined,
      role: selectedRole.value !== 'Tất cả' ? selectedRole.value : undefined,
      trangThai: selectedStatus.value !== 'Tất cả' ? selectedStatus.value : undefined,
      maDonVi: selectedCampus.value !== 'Tất cả' ? Number(selectedCampus.value) : undefined
    }

    const res = await adminUserApi.getUsers(params)
    const payload = res?.items ? res : (res?.data ?? res)
    const list = payload?.items ?? payload?.data ?? (Array.isArray(payload) ? payload : [])
    
    users.value = list.map(normalizeUser)
    totalItems.value = payload?.totalItems ?? payload?.totalCount ?? list.length
    totalPages.value = payload?.totalPages ?? Math.max(1, Math.ceil(totalItems.value / pageSize.value))
  } catch (e) {
    error.value = e?.response?.data?.message || e?.message || 'Không thể tải danh sách người dùng.'
    users.value = []
  } finally {
    loading.value = false
  }
}

// Watch filters & search to reload from API
watch([selectedRole, selectedCampus, selectedStatus, pageSize], () => {
  pageIndex.value = 1
  loadUsers()
})

let searchDebounceTimer = null
watch(searchQuery, () => {
  clearTimeout(searchDebounceTimer)
  searchDebounceTimer = setTimeout(() => {
    pageIndex.value = 1
    loadUsers()
  }, 400)
})

function normalizeUser(user) {
  const id = user.maNguoiDung || user.userId || user.id || 0
  const name = user.hoTen || user.name || user.fullName || user.email || ''
  const email = user.email || ''
  const phone = user.soDienThoai || user.phone || 'Chưa cập nhật'
  const role = user.tenVaiTro || user.role || user.vaiTroChinh || 'Người dùng'
  const campus = user.tenDonVi || user.campus || 'Chưa phân công'
  const maDonVi = user.maDonVi || user.organizationId || 0
  // Lấy các ID numeric từ backend (có sau khi bổ sung vào DTO)
  const maVaiTro = user.maVaiTro || 0
  const maLopHanhChinh = user.maLopHanhChinh ?? user.maLop ?? null
  const maCodeVaiTro = user.maCodeVaiTro || user.vaiTroChinh || ''
  const status = (user.trangThai || user.status || 'active').toLowerCase()

  return {
    ...user,
    id,
    name,
    email,
    phone,
    role,
    campus,
    maDonVi,
    maVaiTro,
    maLopHanhChinh,
    maCodeVaiTro,
    status,
    lastLogin: user.lanDangNhapCuoi || user.lastLogin || null,
    createdAt: user.ngayTao || user.createdAt || null
  }
}

const getStatusBadge = (status) => {
  const s = String(status || '').toLowerCase()
  if (s === 'active' || s === 'đang hoạt động') return { class: 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-500/30', label: 'Đang hoạt động', icon: CheckCircle2 }
  if (s === 'khoa' || s === 'locked' || s === 'bị khóa') return { class: 'bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-300 dark:border-rose-500/30', label: 'Bị khóa', icon: Lock }
  if (s === 'chua_kich_hoat' || s === 'first_login' || s === 'đăng nhập lần đầu') return { class: 'bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-300 dark:border-amber-500/30', label: 'Đăng nhập lần đầu', icon: ShieldAlert }
  return { class: 'bg-slate-500/15 text-slate-700 dark:text-slate-300 border border-slate-300 dark:border-slate-500/30', label: status || 'Không xác định', icon: AlertCircle }
}

const getRoleClass = (role) => {
  const r = String(role || '').toLowerCase()
  if (r.includes('admin') || r.includes('quản trị')) return 'bg-indigo-500/15 text-indigo-700 dark:text-indigo-300 border border-indigo-300 dark:border-indigo-500/30 font-bold'
  if (r.includes('bgh') || r.includes('giám hiệu')) return 'bg-purple-500/15 text-purple-700 dark:text-purple-300 border border-purple-300 dark:border-purple-500/30 font-bold'
  if (r.includes('giao_vu') || r.includes('giáo vụ') || r.includes('staff')) return 'bg-blue-500/15 text-blue-700 dark:text-blue-300 border border-blue-300 dark:border-blue-500/30 font-bold'
  if (r.includes('giang_vien') || r.includes('giảng viên') || r.includes('teacher')) return 'bg-teal-500/15 text-teal-700 dark:text-teal-300 border border-teal-300 dark:border-teal-500/30 font-bold'
  return 'bg-slate-500/15 text-slate-700 dark:text-slate-300 border border-slate-300 dark:border-slate-500/30 font-bold'
}

// Modals & Drawer State
const isDrawerOpen = ref(false)
const currentUser = ref(null)

const isResetModalOpen = ref(false)
const resetPasswordUser = ref(null)
const newPasswordInput = ref('')

const isLockModalOpen = ref(false)
const lockActionUser = ref(null)
const lockReason = ref('')

const isCreateDrawerOpen = ref(false)
const createImportMode = ref('create')
const newUserForm = ref({
  hoTen: '',
  email: '',
  soDienThoai: '',
  matKhau: '',
  maVaiTro: null,
  maDonVi: null
})

const isEditDrawerOpen = ref(false)
const editUserForm = ref({
  id: 0,
  hoTen: '',
  email: '',
  soDienThoai: '',
  maVaiTro: null,
  maDonVi: null,
  maLopHanhChinh: null
})

// Actions
const openEditDrawer = (user) => {
  // Match role theo maVaiTro numeric trước (chính xác nhất), fallback tên
  const matchedRole = availableRoles.value.find(r =>
    (user.maVaiTro > 0 && r.id === user.maVaiTro) ||
    r.code === user.maCodeVaiTro ||
    r.name === user.role ||
    r.code === user.role
  )
  // Match org theo maDonVi numeric
  const userOrgId = user.maDonVi || user.organizationId || 0
  const matchedOrg = userOrgId
    ? availableOrganizations.value.find(o => o.id === userOrgId)
    : availableOrganizations.value.find(o => o.name === user.campus)

  const maVaiTro = matchedRole?.id ?? availableRoles.value[0]?.id ?? null
  const maDonVi = matchedOrg?.id ?? (userOrgId > 0 ? userOrgId : null) ?? availableOrganizations.value[0]?.id ?? null
  // Giữ nguyên maLopHanhChinh của user – bắt buộc phải có cho Student
  const maLopHanhChinh = user.maLopHanhChinh ?? null

  console.debug('[openEditDrawer]', {
    user_role: user.role, user_maVaiTro: user.maVaiTro,
    user_maDonVi: user.maDonVi, user_maLopHanhChinh: user.maLopHanhChinh,
    matchedRole, matchedOrg, maVaiTro, maDonVi, maLopHanhChinh
  })

  editUserForm.value = {
    id: user.id,
    hoTen: user.name,
    email: user.email,
    soDienThoai: user.phone === 'Chưa cập nhật' ? '' : user.phone,
    maVaiTro,
    maDonVi,
    maLopHanhChinh
  }
  isEditDrawerOpen.value = true
}

const confirmEditUser = async () => {
  if (!editUserForm.value.hoTen.trim() || !editUserForm.value.email.trim()) {
    popup.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ Họ tên và Email.')
    return
  }

  const maVaiTro = Number(editUserForm.value.maVaiTro)
  const maDonVi = Number(editUserForm.value.maDonVi)

  if (!maVaiTro || maVaiTro < 1) {
    popup.warning('Thiếu thông tin', 'Vui lòng chọn Vai trò hợp lệ cho tài khoản.')
    return
  }
  if (!maDonVi || maDonVi < 1) {
    popup.warning('Thiếu thông tin', 'Vui lòng chọn Cơ sở / Đơn vị hợp lệ cho tài khoản.')
    return
  }

  const payload = {
    hoTen: editUserForm.value.hoTen.trim(),
    email: editUserForm.value.email.trim(),
    soDienThoai: editUserForm.value.soDienThoai?.trim() || null,
    maVaiTro,
    maDonVi,
    maLopHanhChinh: isEditUserStudent.value ? (editUserForm.value.maLopHanhChinh ?? null) : null
  }
  console.debug('[confirmEditUser] payload →', payload)

  try {
    await adminUserApi.update(editUserForm.value.id, payload)
    popup.success('Đã cập nhật', `Thông tin tài khoản ${editUserForm.value.email} đã được lưu thành công.`)
    isEditDrawerOpen.value = false
    isDrawerOpen.value = false
    await loadUsers()
  } catch (e) {
    const msg = e?.response?.data?.message
      || e?.response?.data?.title
      || e?.message
      || 'Không thể cập nhật tài khoản.'
    console.error('[confirmEditUser] error', e?.response?.data ?? e)
    popup.error('Lỗi cập nhật', msg)
  }
}

const openCreateImportDrawer = () => {
  createImportMode.value = 'create'
  newUserForm.value = {
    hoTen: '',
    email: '',
    soDienThoai: '',
    matKhau: 'Pass@123456',
    maVaiTro: availableRoles.value[0]?.id || 1,
    maDonVi: availableOrganizations.value[0]?.id || 1
  }
  isCreateDrawerOpen.value = true
}

const generateRandomPassword = () => {
  const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*'
  let result = 'Pass@'
  for (let i = 0; i < 6; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length))
  }
  return result
}

const confirmCreateImport = async () => {
  if (createImportMode.value === 'create') {
    if (!newUserForm.value.hoTen.trim() || !newUserForm.value.email.trim() || !newUserForm.value.matKhau.trim()) {
      popup.warning('Thiếu thông tin', 'Vui lòng điền đầy đủ Họ tên, Email và Mật khẩu.')
      return
    }
    try {
      await adminUserApi.create({
        hoTen: newUserForm.value.hoTen.trim(),
        email: newUserForm.value.email.trim(),
        soDienThoai: newUserForm.value.soDienThoai?.trim() || null,
        matKhau: newUserForm.value.matKhau.trim(),
        maVaiTro: Number(newUserForm.value.maVaiTro),
        maDonVi: Number(newUserForm.value.maDonVi),
        maLopHanhChinh: isNewUserStudent.value ? (newUserForm.value.maLopHanhChinh ?? null) : null
      })
      popup.success('Đã tạo tài khoản', `Tài khoản ${newUserForm.value.email} đã được khởi tạo thành công!`)
      isCreateDrawerOpen.value = false
      await loadUsers()
    } catch (e) {
      popup.error('Lỗi tạo tài khoản', e?.response?.data?.message || e?.message || 'Không thể tạo tài khoản.')
    }
  } else {
    popup.info('Import Excel', 'Chức năng Import từ Excel đang được tải mẫu.')
    isCreateDrawerOpen.value = false
  }
}

const openDrawer = (user) => {
  currentUser.value = user
  isDrawerOpen.value = true
}

const openResetModal = (user) => {
  resetPasswordUser.value = user
  newPasswordInput.value = generateRandomPassword()
  isResetModalOpen.value = true
}

const confirmResetPassword = async () => {
  if (!newPasswordInput.value.trim() || newPasswordInput.value.trim().length < 8) {
    popup.warning('Mật khẩu ngắn', 'Mật khẩu mới phải có tối thiểu 8 ký tự.')
    return
  }
  try {
    await adminUserApi.resetPassword(resetPasswordUser.value.id, {
      newPassword: newPasswordInput.value.trim()
    })
    popup.success('Đã reset mật khẩu', `Mật khẩu mới của ${resetPasswordUser.value.email} là: ${newPasswordInput.value.trim()}`)
    isResetModalOpen.value = false
  } catch (e) {
    popup.error('Lỗi reset mật khẩu', e?.response?.data?.message || e?.message || 'Không thể reset mật khẩu.')
  }
}

const openLockModal = (user) => {
  lockActionUser.value = user
  lockReason.value = ''
  isLockModalOpen.value = true
}

const confirmLockAction = async () => {
  try {
    const isLocked = lockActionUser.value.status === 'khoa' || lockActionUser.value.status === 'locked'
    if (isLocked) {
      await adminUserApi.unlock(lockActionUser.value.id)
      popup.success('Đã mở khóa', `Tài khoản ${lockActionUser.value.email} đã mở khóa hoạt động.`)
    } else {
      if (!lockReason.value.trim()) {
        popup.warning('Bắt buộc nhập lý do', 'Vui lòng ghi rõ lý do khóa tài khoản để lưu Audit Log.')
        return
      }
      await adminUserApi.lock(lockActionUser.value.id, lockReason.value.trim())
      popup.success('Đã khóa tài khoản', `Tài khoản ${lockActionUser.value.email} đã bị khóa và ghi Log thành công.`)
    }
    isLockModalOpen.value = false
    await loadUsers()
  } catch (e) {
    popup.error('Lỗi thao tác', e?.response?.data?.message || e?.message || 'Không thể thay đổi trạng thái tài khoản.')
  }
}

const toggleSelectAll = (e) => {
  if (e.target.checked) {
    selectedUsers.value = users.value.map(u => u.id)
  } else {
    selectedUsers.value = []
  }
}

const formatDateTime = (dateStr) => {
  if (!dateStr) return 'Chưa đăng nhập'
  try {
    return new Date(dateStr).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' })
  } catch {
    return dateStr
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return '--'
  try {
    return new Date(dateStr).toLocaleDateString('vi-VN')
  } catch {
    return dateStr
  }
}

const route = useRoute()
onMounted(async () => {
  await Promise.all([
    loadOrganizations(),
    loadRoles(),
    loadClasses()
  ])
  await loadUsers()
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
          <h1 class="text-2xl font-bold text-heading flex items-center gap-2">
            Danh sách người dùng
            <span v-if="totalItems > 0" class="text-xs px-2.5 py-0.5 rounded-full bg-blue-500/10 text-blue-600 dark:text-blue-400 font-semibold border border-blue-200 dark:border-blue-800">
              {{ totalItems }} tài khoản
            </span>
          </h1>
          <p class="text-sm text-label mt-1">Toàn quyền quản lý tài khoản, phân quyền và giám sát truy cập hệ thống toàn bộ cơ sở.</p>
        </div>
        <div class="flex items-center gap-3">
          <button @click="loadUsers" class="glass-btn secondary shadow-sm" title="Tải lại dữ liệu">
            <RefreshCw :size="16" :class="{ 'animate-spin': loading }" /> Tải lại
          </button>
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
    <div class="controls-panel glass-panel relative z-20 mb-6 p-4 rounded-2xl flex flex-col lg:flex-row gap-4 items-center justify-between">
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
        <div class="w-52">
          <LmsSelect v-model="selectedRole" :options="roleFilterOptions" placeholder="Vai trò" />
        </div>
        <div class="w-64">
          <LmsSelect v-model="selectedCampus" :options="campusFilterOptions" placeholder="Cơ sở" />
        </div>
        <div class="w-48">
          <LmsSelect v-model="selectedStatus" :options="statusFilterOptions" placeholder="Trạng thái" />
        </div>
      </div>
    </div>

    <!-- Bulk Actions (hiển thị khi có item được chọn) -->
    <transition name="fade">
      <div v-if="selectedUsers.length > 0" class="bulk-actions mb-4 p-3 rounded-xl bg-blue-500/10 border border-blue-200 dark:border-blue-800 flex items-center justify-between">
        <span class="text-sm font-semibold text-blue-700 dark:text-blue-300">Đã chọn {{ selectedUsers.length }} tài khoản</span>
        <div class="flex gap-2">
          <button class="glass-btn secondary text-xs py-1.5 px-3"><Mail :size="14" /> Gửi thông báo</button>
        </div>
      </div>
    </transition>

    <!-- Loading State -->
    <div v-if="loading" class="glass-panel rounded-2xl p-6 text-center text-label">
      <RefreshCw :size="32" class="animate-spin mx-auto mb-2 text-blue-500" />
      <p class="text-sm">Đang nạp danh sách tài khoản từ SQL Server database...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="glass-panel rounded-2xl p-12 flex flex-col items-center justify-center">
      <AlertCircle :size="40" class="text-rose-400 mb-3" />
      <p class="text-rose-600 dark:text-rose-400 font-semibold mb-2">{{ error }}</p>
      <button @click="loadUsers" class="glass-btn primary text-xs">Thử lại</button>
    </div>

    <!-- Data Table -->
    <div v-else class="table-container glass-panel rounded-2xl overflow-hidden">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr>
            <th class="w-12 text-center">
              <input type="checkbox" class="glass-checkbox" :checked="selectedUsers.length === users.length && users.length > 0" @change="toggleSelectAll" />
            </th>
            <th>Thông tin định danh</th>
            <th>Vai trò & Cơ sở</th>
            <th>Trạng thái</th>
            <th>Dữ liệu truy cập</th>
            <th class="text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="users.length === 0">
            <td colspan="6" class="text-center py-10 text-placeholder">Không tìm thấy người dùng nào phù hợp trong cơ sở dữ liệu.</td>
          </tr>
          <tr v-for="user in users" :key="user.id" class="hover:bg-slate-500/5 transition border-t border-slate-500/10">
            <td class="text-center">
              <input type="checkbox" class="glass-checkbox" :value="user.id" v-model="selectedUsers" />
            </td>
            <td>
              <div class="flex items-center gap-3">
                <div class="avatar bg-gradient-to-br from-blue-500 to-indigo-600 text-white flex items-center justify-center rounded-full font-bold w-10 h-10 shadow-sm shrink-0">
                  {{ user.name ? user.name.charAt(0).toUpperCase() : 'U' }}
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
              <div class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold" :class="getStatusBadge(user.status).class">
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
                <button @click="openDrawer(user)" class="action-btn text-blue-600 hover:bg-blue-500/10" title="Xem chi tiết">
                  <Eye :size="16" />
                </button>
                <button @click="openEditDrawer(user)" class="action-btn text-teal-600 hover:bg-teal-500/10" title="Chỉnh sửa & Gán quyền">
                  <Edit2 :size="16" />
                </button>
                <button @click="openResetModal(user)" class="action-btn text-amber-600 hover:bg-amber-500/10" title="Reset Mật khẩu">
                  <KeyRound :size="16" />
                </button>
                <button @click="openLockModal(user)" class="action-btn hover:bg-rose-500/10" :class="user.status === 'khoa' || user.status === 'locked' ? 'text-emerald-600' : 'text-rose-600'" :title="user.status === 'khoa' || user.status === 'locked' ? 'Mở khóa tài khoản' : 'Khóa tài khoản'">
                  <component :is="user.status === 'khoa' || user.status === 'locked' ? Unlock : Lock" :size="16" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination Bar -->
      <div class="pagination-bar p-4 border-t border-slate-500/10 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div class="text-xs text-label">
          Trang <strong>{{ pageIndex }}</strong> / <strong>{{ totalPages }}</strong> — Tổng số <strong>{{ totalItems }}</strong> tài khoản
        </div>
        <div class="flex items-center gap-3">
          <div class="flex items-center gap-2 text-xs text-label">
            <span>Hiển thị:</span>
            <div class="w-32">
              <LmsSelect v-model="pageSize" :options="pageSizeOptions" />
            </div>
          </div>
          <div class="flex gap-1">
            <button 
              @click="pageIndex > 1 && (pageIndex--, loadUsers())" 
              :disabled="pageIndex <= 1"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex <= 1 }"
            >
              <ChevronLeft :size="14" /> Trước
            </button>
            <button 
              @click="pageIndex < totalPages && (pageIndex++, loadUsers())" 
              :disabled="pageIndex >= totalPages"
              class="glass-btn secondary py-1 px-2.5 text-xs" 
              :class="{ 'opacity-50 cursor-not-allowed': pageIndex >= totalPages }"
            >
              Sau <ChevronRight :size="14" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Teleport Modals & Drawers -->
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
              {{ currentUser.name ? currentUser.name.charAt(0).toUpperCase() : 'U' }}
            </div>
            <h2 class="text-xl font-bold text-heading">{{ currentUser.name }}</h2>
            <p class="text-sm text-label">{{ currentUser.email }}</p>
            <div class="mt-2 inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold" :class="getStatusBadge(currentUser.status).class">
              <component :is="getStatusBadge(currentUser.status).icon" :size="12" />
              {{ getStatusBadge(currentUser.status).label }}
            </div>
          </div>

          <div class="info-section">
            <h4 class="font-semibold text-heading mb-3 flex items-center gap-2"><Shield :size="16" /> Phân quyền & Công tác</h4>
            <div class="info-row"><span class="info-label">Vai trò:</span> <span class="font-semibold text-heading">{{ currentUser.role }}</span></div>
            <div class="info-row"><span class="info-label">Cơ sở (Campus):</span> <span>{{ currentUser.campus }}</span></div>
            <div class="info-row"><span class="info-label">Số điện thoại:</span> <span>{{ currentUser.phone }}</span></div>
          </div>

          <div class="info-section mt-6">
            <h4 class="font-semibold text-heading mb-3 flex items-center gap-2"><History :size="16" /> Lịch sử truy cập</h4>
            <div class="info-row"><span class="info-label">Lần đăng nhập cuối:</span> <span>{{ formatDateTime(currentUser.lastLogin) }}</span></div>
            <div class="info-row"><span class="info-label">Ngày tạo tài khoản:</span> <span>{{ formatDate(currentUser.createdAt) }}</span></div>
          </div>

          <div class="mt-8 flex gap-3">
            <button @click="openEditDrawer(currentUser)" class="glass-btn primary flex-1 justify-center"><Edit2 :size="16" /> Cập nhật</button>
            <button v-if="currentUser.status !== 'khoa' && currentUser.status !== 'locked'" @click="openLockModal(currentUser)" class="glass-btn danger flex-1 justify-center"><Lock :size="16" /> Khóa TK</button>
            <button v-else @click="openLockModal(currentUser)" class="glass-btn flex-1 justify-center !text-emerald-500 !border-emerald-500/30 !bg-emerald-500/10"><Unlock :size="16" /> Mở khóa</button>
          </div>
        </div>
      </div>

      <!-- Modal Reset Mật khẩu Hiện Đại -->
      <div v-if="isResetModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-sm w-full">
          <div class="flex items-center justify-center w-12 h-12 rounded-full bg-amber-500/15 text-amber-500 mb-4 mx-auto">
            <KeyRound :size="24" />
          </div>
          <h3 class="text-lg font-bold text-center text-heading mb-1">Reset Mật Khẩu</h3>
          <p class="text-xs text-center text-label mb-4">Cấp lại mật khẩu cho tài khoản <strong>{{ resetPasswordUser?.email }}</strong></p>
          
          <div class="form-group mb-4">
            <label class="block text-xs font-bold text-label mb-1">Mật khẩu mới</label>
            <div class="relative">
              <input v-model="newPasswordInput" type="text" class="glass-input w-full pr-10" placeholder="Tối thiểu 8 ký tự..." />
              <button @click="newPasswordInput = generateRandomPassword()" class="absolute right-2 top-1/2 -translate-y-1/2 text-blue-500 hover:text-blue-600 p-1" title="Tạo mật khẩu ngẫu nhiên">
                <Sparkles :size="16" />
              </button>
            </div>
          </div>

          <div class="flex gap-3">
            <button @click="isResetModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button @click="confirmResetPassword" class="glass-btn primary flex-1 justify-center">Xác nhận Reset</button>
          </div>
        </div>
      </div>

      <!-- Modal Khóa / Mở Khóa Tài Khoản -->
      <div v-if="isLockModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-md w-full">
          <div class="flex items-center gap-3 mb-5 border-b border-slate-500/10 pb-4">
            <div class="flex items-center justify-center w-12 h-12 rounded-full" :class="lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? 'bg-emerald-500/15 text-emerald-500' : 'bg-rose-500/15 text-rose-500'">
              <component :is="lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? Unlock : Lock" :size="24" />
            </div>
            <div>
              <h3 class="text-lg font-bold text-heading">{{ lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? 'Mở Khóa Tài Khoản' : 'Khóa Tài Khoản' }}</h3>
              <p class="text-xs font-semibold text-blue-500 mt-0.5">{{ lockActionUser?.email }}</p>
            </div>
          </div>

          <template v-if="lockActionUser?.status !== 'khoa' && lockActionUser?.status !== 'locked'">
            <div class="mb-4">
              <div class="bg-rose-500/10 border border-rose-500/20 rounded-lg p-3 mb-4">
                <p class="text-xs text-rose-600 dark:text-rose-400 font-medium leading-relaxed">Tài khoản này sẽ bị khóa ngưng truy cập toàn bộ hệ thống. Thao tác này sẽ được ghi vào hệ thống Audit Log.</p>
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-rose-500 mb-1">Lý do khóa tài khoản (Ghi Audit Log) *</label>
                <textarea v-model="lockReason" rows="3" class="glass-input w-full" placeholder="VD: Vi phạm chính sách bảo mật, tạm ngừng công tác..."></textarea>
              </div>
            </div>
          </template>
          <template v-else>
            <div class="bg-emerald-500/10 border border-emerald-500/20 rounded-lg p-4 mb-6">
              <p class="text-sm text-emerald-600 dark:text-emerald-400 font-semibold mb-1">Khôi phục trạng thái hoạt động</p>
              <p class="text-xs text-emerald-600 dark:text-emerald-400">Tài khoản sẽ được mở khóa và đăng nhập bình thường vào hệ thống.</p>
            </div>
          </template>

          <div class="flex gap-3 justify-end mt-2 pt-4 border-t border-slate-500/10">
            <button @click="isLockModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy bỏ</button>
            <button @click="confirmLockAction" class="glass-btn flex-1 justify-center" :class="lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? 'primary' : 'danger'">
              <component :is="lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? Unlock : Lock" :size="16" />
              {{ lockActionUser?.status === 'khoa' || lockActionUser?.status === 'locked' ? 'Xác nhận Mở Khóa' : 'Xác nhận Khóa' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Drawer Tạo / Import Tài khoản -->
      <div v-if="isCreateDrawerOpen" class="drawer-overlay" @click="isCreateDrawerOpen = false"></div>
      <div class="drawer" :class="{ 'open': isCreateDrawerOpen }">
        <div class="drawer-header">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2">
            <UserPlus :size="20" class="text-blue-500"/> Quản lý Tài khoản Mới
          </h3>
          <button @click="isCreateDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>
        
        <div class="drawer-body p-6">
          <div class="flex border-b border-slate-500/10 mb-6">
            <button @click="createImportMode = 'create'" class="flex-1 pb-3 font-semibold text-sm transition-colors border-b-2 outline-none" :class="createImportMode === 'create' ? 'text-blue-500 border-blue-500' : 'text-label border-transparent'">Tạo thủ công</button>
            <button @click="createImportMode = 'import'" class="flex-1 pb-3 font-semibold text-sm transition-colors border-b-2 outline-none" :class="createImportMode === 'import' ? 'text-blue-500 border-blue-500' : 'text-label border-transparent'">Import từ Excel</button>
          </div>

          <template v-if="createImportMode === 'create'">
            <div class="space-y-4 mb-6">
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Họ và tên *</label>
                <input v-model="newUserForm.hoTen" type="text" class="glass-input w-full" placeholder="Nhập họ và tên..." />
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Email *</label>
                <input v-model="newUserForm.email" type="email" class="glass-input w-full" placeholder="email@fpt.edu.vn" />
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Số điện thoại</label>
                <input v-model="newUserForm.soDienThoai" type="text" class="glass-input w-full" placeholder="0901234567" />
              </div>
              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Mật khẩu ban đầu *</label>
                <div class="relative">
                  <input v-model="newUserForm.matKhau" type="text" class="glass-input w-full pr-10" placeholder="Pass@123456" />
                  <button @click="newUserForm.matKhau = generateRandomPassword()" class="absolute right-2 top-1/2 -translate-y-1/2 text-blue-500 hover:text-blue-600 p-1" title="Tạo mật khẩu ngẫu nhiên">
                    <Sparkles :size="16" />
                  </button>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-4">
                <LmsSelect v-model="newUserForm.maVaiTro" :options="formRoleOptions" label="Vai trò *" required />
                <LmsSelect v-model="newUserForm.maDonVi" :options="formCampusOptions" label="Cơ sở *" required />
              </div>
              <div class="form-group mt-4" v-if="isNewUserStudent">
                <LmsSelect v-model="newUserForm.maLopHanhChinh" :options="formClassOptions" label="Lớp hành chính *" required />
              </div>
            </div>
          </template>
          <template v-else>
            <div class="border-2 border-dashed border-blue-500/30 bg-blue-500/5 rounded-xl p-8 text-center mb-6 flex flex-col items-center justify-center">
              <div class="w-12 h-12 bg-blue-500/10 rounded-full flex items-center justify-center mb-3">
                <FileSpreadsheet :size="24" class="text-blue-500" />
              </div>
              <h4 class="font-bold text-sm text-heading mb-1">Kéo thả file Excel vào đây</h4>
              <p class="text-xs text-label mb-4">Hỗ trợ các định dạng .xlsx, .csv (Tối đa 5MB)</p>
              <button class="glass-btn secondary text-xs">Chọn file từ máy tính</button>
            </div>
          </template>

          <div class="flex gap-3 mt-6 border-t border-slate-500/10 pt-4">
            <button @click="isCreateDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button @click="confirmCreateImport" class="glass-btn primary flex-1 justify-center" :disabled="createImportMode === 'create' && (!newUserForm.hoTen || !newUserForm.email || !newUserForm.matKhau)">
              <component :is="createImportMode === 'create' ? UserPlus : FileSpreadsheet" :size="16" />
              {{ createImportMode === 'create' ? 'Tạo tài khoản' : 'Bắt đầu Import' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Drawer Chỉnh sửa thông tin tài khoản -->
      <div v-if="isEditDrawerOpen" class="drawer-overlay" @click="isEditDrawerOpen = false"></div>
      <div class="drawer" :class="{ 'open': isEditDrawerOpen }">
        <div class="drawer-header">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2">
            <Edit2 :size="20" class="text-teal-500"/> Chỉnh sửa Thông tin
          </h3>
          <button @click="isEditDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>
        
        <div class="drawer-body p-6" v-if="editUserForm">
          <div class="space-y-4 mb-6">
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Họ và tên *</label>
              <input v-model="editUserForm.hoTen" type="text" class="glass-input w-full" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Email *</label>
              <input v-model="editUserForm.email" type="email" class="glass-input w-full" />
            </div>
            <div class="form-group">
              <label class="block text-xs font-bold text-label mb-1">Số điện thoại</label>
              <input v-model="editUserForm.soDienThoai" type="text" class="glass-input w-full" />
            </div>
            <div class="grid grid-cols-2 gap-4">
              <LmsSelect v-model="editUserForm.maVaiTro" :options="formRoleOptions" label="Vai trò *" required />
              <LmsSelect v-model="editUserForm.maDonVi" :options="formCampusOptions" label="Cơ sở *" required />
            </div>
            <div class="form-group mt-4" v-if="isEditUserStudent">
              <LmsSelect v-model="editUserForm.maLopHanhChinh" :options="formClassOptions" label="Lớp hành chính *" required />
            </div>
          </div>

          <div class="flex gap-3 justify-end mt-6 border-t border-slate-500/10 pt-4">
            <button @click="isEditDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
            <button @click="confirmEditUser" class="glass-btn primary flex-1 justify-center !bg-teal-600 hover:!bg-teal-700 !border-teal-600" :disabled="!editUserForm?.hoTen || !editUserForm?.email">
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
  right: -520px;
  width: 100%;
  max-width: 520px;
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
