<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Users, Shield, KeyRound, Loader2 } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { accountApi } from '@/services/accountApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const loading = ref(true); const error = ref(null); const rows = ref([])
const roleOptions = ref([]); const organizationOptions = ref([]); const classOptions = ref([])
const searchQuery = ref(''); const filterVaiTro = ref(''); const filterKichHoat = ref('')
const groupByMode = ref('role') // 'role' | 'class' (for students) | 'none'

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ hoTen: '', email: '', soDienThoai: '', matKhau: '', vaiTro: null, donVi: null, maLopHanhChinh: null, kichHoat: true })
const formErrors = ref({})

const showResetModal = ref(false); const resetTarget = ref(null); const resetPassword = ref(''); const resetting = ref(false)
const resetError = ref('')

const vaiTroBadge = {
  GiangVien: { label: 'Giảng viên', variant: 'primary' },
  AcademicStaff: { label: 'Giáo vụ', variant: 'info' },
  SinhVien: { label: 'Sinh viên', variant: 'success' },
  Principal: { label: 'BGH', variant: 'violet' },
  SuperAdmin: { label: 'Super Admin', variant: 'danger' },
  PhuHuynh: { label: 'Phụ huynh', variant: 'warning' },
  Admin: { label: 'Admin', variant: 'danger' },
  CampusAdmin: { label: 'Quản trị cơ sở', variant: 'warning' },
  Chairman: { label: 'Chủ tịch', variant: 'violet' },
}

const vaiTroOptions = computed(() => {
  const codes = roleOptions.value.map(r => r.uiCode).filter(Boolean)
  return [...new Set(codes)]
})

const dbCodeToUi = {
  giao_vien: 'GiangVien', nhan_vien: 'AcademicStaff', hoc_sinh: 'SinhVien',
  hieu_truong: 'Principal', sieu_quan_tri: 'SuperAdmin', phu_huynh: 'PhuHuynh',
  quan_tri: 'Admin', quan_tri_co_so: 'CampusAdmin', chu_tich: 'Chairman',
}

const isStudentRole = computed(() => {
  const role = roleOptions.value.find(r => r.maVaiTro === Number(formData.value.vaiTro))
  return role?.maCodeVaiTro === 'hoc_sinh'
})

onMounted(async () => {
  await Promise.all([loadMeta(), fetchData()])
})

async function loadMeta() {
  try {
    const [roles, orgs] = await Promise.all([accountApi.getRoles(), accountApi.getOrganizations()])
    roleOptions.value = roles.map(r => ({ ...r, uiCode: dbCodeToUi[r.maCodeVaiTro] || r.maCodeVaiTro }))
    organizationOptions.value = orgs.filter(o => o.isActive !== false)
  } catch (e) {
    popupStore.error('Lỗi', e.message || 'Không tải được danh mục vai trò/đơn vị.')
  }
}

async function loadClasses() {
  const donVi = formData.value.donVi
  try {
    const classes = donVi ? await accountApi.getClasses({ MaKhoa: Number(donVi) }) : await accountApi.getClasses()
    classOptions.value = classes
  } catch {
    classOptions.value = []
  }
}

async function fetchData() {
  loading.value = true; error.value = null
  try {
    const data = await accountApi.list({ VaiTro: filterVaiTro.value || undefined, KichHoat: filterKichHoat.value || undefined, Search: searchQuery.value || undefined })
    rows.value = Array.isArray(data) ? data : data?.items || data?.data || []
  } catch (e) { error.value = e.message || 'Không thể tải danh sách tài khoản' }
  finally { loading.value = false }
}

const filteredRows = computed(() => {
  let list = rows.value
  if (searchQuery.value) { const q = searchQuery.value.toLowerCase(); list = list.filter(r => r.hoTen?.toLowerCase().includes(q) || r.tenDangNhap?.toLowerCase().includes(q) || r.email?.toLowerCase().includes(q)) }
  if (filterVaiTro.value) list = list.filter(r => r.vaiTro === filterVaiTro.value)
  if (filterKichHoat.value !== '') { const a = filterKichHoat.value === 'true'; list = list.filter(r => r.kichHoat === a) }
  return list
})

const groupedAccounts = computed(() => {
  const filtered = filteredRows.value
  if (groupByMode.value === 'none') {
    return { ungrouped: filtered }
  }

  if (groupByMode.value === 'role') {
    const groups = {}
    filtered.forEach(account => {
      const role = account.vaiTro || 'Unknown'
      if (!groups[role]) groups[role] = []
      groups[role].push(account)
    })
    const roleOrder = ['SuperAdmin', 'Principal', 'AcademicStaff', 'GiangVien', 'SinhVien', 'PhuHuynh']
    const sorted = {}
    roleOrder.forEach(role => {
      if (groups[role]) sorted[role] = groups[role]
    })
    Object.keys(groups).forEach(role => {
      if (!sorted[role]) sorted[role] = groups[role]
    })
    return sorted
  }

  if (groupByMode.value === 'class' && filterVaiTro.value === 'SinhVien') {
    const groups = {}
    filtered.forEach(account => {
      const cls = account.lopHanhChinh || 'Chưa xác định'
      if (!groups[cls]) groups[cls] = []
      groups[cls].push(account)
    })
    return groups
  }

  return { ungrouped: filtered }
})

const summaryCards = computed(() => [
  { label: 'Tổng tài khoản', value: rows.value.length, color: 'text-(--lg-primary)' },
  { label: 'Đang hoạt động', value: rows.value.filter(r => r.kichHoat).length, color: 'text-(--color-success-text)' },
  { label: 'Đã khóa', value: rows.value.filter(r => !r.kichHoat).length, color: 'text-(--color-danger-text)' },
  { label: 'Giảng viên', value: rows.value.filter(r => r.vaiTro === 'GiangVien').length, color: 'text-(--color-info-text)' },
  { label: 'Sinh viên', value: rows.value.filter(r => r.vaiTro === 'SinhVien').length, color: 'text-(--color-success-text)' },
])

const iconMap = { 'Tổng tài khoản': Users, 'Đang hoạt động': Users, 'Đã khóa': Users, 'Giảng viên': Shield, 'Sinh viên': Users }
const bgMap = { 'Tổng tài khoản': 'bg-(--color-info-bg)', 'Đang hoạt động': 'bg-(--color-success-bg)', 'Đã khóa': 'bg-(--color-danger-bg)', 'Giảng viên': 'bg-(--color-info-bg)', 'Sinh viên': 'bg-(--color-success-bg)' }

function clearFilters() { searchQuery.value = ''; filterVaiTro.value = ''; filterKichHoat.value = '' }

const defaults = () => ({ hoTen: '', email: '', soDienThoai: '', matKhau: '', vaiTro: null, donVi: null, maLopHanhChinh: null, kichHoat: true })
function resetForm() { formData.value = defaults(); formErrors.value = {}; classOptions.value = [] }
function openCreate() {
  resetForm()
  formMode.value = 'create'; editingId.value = null; showFormModal.value = true
  const firstRole = roleOptions.value.find(r => r.uiCode === 'SinhVien') || roleOptions.value[0]
  if (firstRole) formData.value.vaiTro = firstRole.maVaiTro
  const firstOrg = organizationOptions.value[0]
  if (firstOrg) formData.value.donVi = firstOrg.maDonVi
  if (formData.value.vaiTro) loadClasses()
}
function openEdit(r) {
  formMode.value = 'edit'; editingId.value = r.maTaiKhoan
  formData.value = {
    hoTen: r.hoTen, email: r.email, soDienThoai: r.soDienThoai || '', matKhau: '',
    vaiTro: r.maVaiTro ?? null, donVi: r.maDonVi ?? null, maLopHanhChinh: r.maLopHanhChinh ?? null,
    kichHoat: r.kichHoat,
  }
  formErrors.value = {}; showFormModal.value = true
  loadClasses()
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.hoTen.trim()) e.hoTen = 'Họ tên không được để trống'
  if (!formData.value.email.trim()) e.email = 'Email không được để trống'
  else if (!formData.value.email.includes('@')) e.email = 'Email phải chứa @'
  if (!formData.value.vaiTro) e.vaiTro = 'Vui lòng chọn vai trò'
  if (!formData.value.donVi) e.donVi = 'Vui lòng chọn đơn vị'
  if (formMode.value === 'create') {
    if (!formData.value.matKhau) e.matKhau = 'Mật khẩu không được để trống'
    else if (formData.value.matKhau.length < 8) e.matKhau = 'Mật khẩu phải có tối thiểu 8 ký tự'
  }
  if (isStudentRole.value && !formData.value.maLopHanhChinh) e.maLopHanhChinh = 'Sinh viên phải được gán lớp hành chính'
  formErrors.value = e; return Object.keys(e).length === 0
}

async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    if (formMode.value === 'edit') {
      await accountApi.update(editingId.value, formData.value)
    } else {
      await accountApi.create(formData.value)
    }
    popupStore.success(formMode.value === 'edit' ? 'Cập nhật tài khoản' : 'Tạo tài khoản', 'Lưu thành công.')
    closeForm(); await fetchData()
  } catch (e) {
    formErrors.value._api = e.message || 'Lỗi khi lưu tài khoản'
    popupStore.error('Lỗi', e.message || 'Không thể lưu tài khoản.')
  }
  finally { submitting.value = false }
}

async function toggleActive(r) {
  try {
    await accountApi.toggleActive({ maTaiKhoan: r.maTaiKhoan, kichHoat: r.kichHoat })
    await fetchData()
    popupStore.success('Trạng thái tài khoản', r.kichHoat ? 'Đã khóa tài khoản.' : 'Đã mở khóa tài khoản.')
  }
  catch (e) { popupStore.error('Lỗi', e.message || 'Không thể thay đổi trạng thái.') }
}

function openResetPwd(r) {
  resetTarget.value = r; resetPassword.value = ''; resetError.value = ''
  showResetModal.value = true
}
function closeResetPwd() { showResetModal.value = false; resetTarget.value = null; resetPassword.value = ''; resetError.value = '' }

async function submitResetPwd() {
  if (!resetTarget.value) return
  if (!resetPassword.value) { resetError.value = 'Mật khẩu mới không được để trống.'; return }
  if (resetPassword.value.length < 8) { resetError.value = 'Mật khẩu mới phải có tối thiểu 8 ký tự.'; return }
  resetting.value = true
  try {
    await accountApi.resetPassword(resetTarget.value.maTaiKhoan, resetPassword.value)
    popupStore.success('Đặt lại mật khẩu', 'Đặt lại mật khẩu thành công.')
    closeResetPwd()
  } catch (e) {
    resetError.value = e.message || 'Không thể đặt lại mật khẩu.'
  } finally { resetting.value = false }
}

function handleVaiTroChange() {
  if (!isStudentRole.value) formData.value.maLopHanhChinh = null
  loadClasses()
}
function handleDonViChange() {
  formData.value.maLopHanhChinh = null
  loadClasses()
}
</script>

<template>
  <div class="space-y-4">

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
      <div v-for="c in summaryCards" :key="c.label" class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">{{ c.label }}</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ c.value }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl flex items-center justify-center" :class="bgMap[c.label]">
            <component :is="iconMap[c.label]" :size="20" :class="c.color" />
          </div>
        </div>
      </div>
    </div>

    <!-- Main Card -->
    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="p-4 border-b border-default bg-(--surface-input)">
        <div class="flex flex-wrap items-center gap-3">
          <div class="relative flex-1 min-w-[200px]">
            <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
            <input v-model="searchQuery" type="text" placeholder="Tìm tên, tài khoản, email..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" @keydown.enter="fetchData" />
          </div>
          <select v-model="filterVaiTro" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả vai trò</option>
            <option v-for="vt in vaiTroOptions" :key="vt" :value="vt">{{ (vaiTroBadge[vt] || {}).label || vt }}</option>
          </select>
          <select v-model="filterKichHoat" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đang hoạt động</option>
            <option value="false">Đã khóa</option>
          </select>
          <select v-model="groupByMode" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="none">Không sắp xếp</option>
            <option value="role">Sắp xếp theo vai trò</option>
            <option v-if="filterVaiTro === 'SinhVien'" value="class">Sắp xếp theo lớp</option>
          </select>
          <button v-if="filterVaiTro || filterKichHoat !== '' || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm tài khoản</GlassButton>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="6" /></div>
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchData">Thử lại</GlassButton>
        </div>
      </div>
      <div v-else-if="filteredRows.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy tài khoản nào" description="Thử thay đổi từ khóa hoặc bỏ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm tài khoản</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <!-- Grouped view -->
        <template v-if="Object.keys(groupedAccounts).length > 1 || groupByMode !== 'none'">
          <template v-for="(groupList, groupName) in groupedAccounts" :key="groupName">
            <div v-if="groupList.length > 0">
              <!-- Group header -->
              <div class="bg-(--surface-input) border-b border-default px-3 py-3 font-semibold text-sm text-heading sticky top-0 z-10">
                <span v-if="groupByMode === 'role'">
                  {{ (vaiTroBadge[groupName] || {}).label || groupName }} ({{ groupList.length }})
                </span>
                <span v-else>
                  {{ groupName }} ({{ groupList.length }})
                </span>
              </div>
              <!-- Group rows -->
              <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
                <thead class="bg-(--surface-input) border-b border-default text-muted hidden">
                  <tr>
                    <th class="px-3 py-4 font-semibold">Tên đăng nhập</th>
                    <th class="px-3 py-4 font-semibold">Họ tên</th>
                    <th class="px-3 py-4 font-semibold">Email</th>
                    <th class="px-3 py-4 font-semibold">Vai trò</th>
                    <th class="px-3 py-4 font-semibold">Đơn vị</th>
                    <th class="px-3 py-4 font-semibold text-center">Trạng thái</th>
                    <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-default">
                  <tr v-for="r in groupList" :key="r.maTaiKhoan" class="hover:bg-(--surface-hover) transition-colors">
                    <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">{{ r.tenDangNhap }}</td>
                    <td class="px-3 py-3.5 font-bold text-heading max-w-[200px] truncate">{{ r.hoTen }}</td>
                    <td class="px-3 py-3.5 text-body">{{ r.email }}</td>
                    <td class="px-3 py-3.5">
                      <GlassBadge :variant="(vaiTroBadge[r.vaiTro] || vaiTroBadge.SinhVien).variant" size="sm">
                        <Shield :size="10" class="mr-0.5" /> {{ (vaiTroBadge[r.vaiTro] || vaiTroBadge.SinhVien).label }}
                      </GlassBadge>
                    </td>
                    <td class="px-3 py-3.5 text-body">{{ r.donVi || r.lopHanhChinh || '—' }}</td>
                    <td class="px-3 py-3.5 text-center">
                      <GlassBadge :variant="r.kichHoat ? 'success' : 'danger'" size="sm">
                        {{ r.kichHoat ? 'Hoạt động' : 'Đã khóa' }}
                      </GlassBadge>
                    </td>
                    <td class="px-3 py-3.5">
                      <div class="flex items-center justify-center gap-1">
                        <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Sửa" @click.stop="openEdit(r)">
                          <Pencil :size="15" />
                        </button>
                        <button class="h-8 w-8 rounded-lg hover:bg-(--color-warning-bg) flex items-center justify-center text-muted hover:text-(--color-warning-text) transition-colors" :title="r.kichHoat ? 'Khóa' : 'Kích hoạt'" @click.stop="toggleActive(r)">
                          <component :is="r.kichHoat ? X : Users" :size="15" />
                        </button>
                        <button class="h-8 w-8 rounded-lg hover:bg-(--color-info-bg) flex items-center justify-center text-muted hover:text-(--color-info-text) transition-colors" title="Đặt lại mật khẩu" @click.stop="openResetPwd(r)">
                          <KeyRound :size="14" />
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </template>
        </template>
        <!-- Ungrouped table view -->
        <template v-else>
          <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
            <thead class="bg-(--surface-input) border-b border-default text-muted">
              <tr>
                <th class="px-3 py-4 font-semibold">Tên đăng nhập</th>
                <th class="px-3 py-4 font-semibold">Họ tên</th>
                <th class="px-3 py-4 font-semibold">Email</th>
                <th class="px-3 py-4 font-semibold">Vai trò</th>
                <th class="px-3 py-4 font-semibold">Đơn vị</th>
                <th class="px-3 py-4 font-semibold text-center">Trạng thái</th>
                <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="r in filteredRows" :key="r.maTaiKhoan" class="hover:bg-(--surface-hover) transition-colors">
                <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">{{ r.tenDangNhap }}</td>
                <td class="px-3 py-3.5 font-bold text-heading max-w-[200px] truncate">{{ r.hoTen }}</td>
                <td class="px-3 py-3.5 text-body">{{ r.email }}</td>
                <td class="px-3 py-3.5">
                  <GlassBadge :variant="(vaiTroBadge[r.vaiTro] || vaiTroBadge.SinhVien).variant" size="sm">
                    <Shield :size="10" class="mr-0.5" /> {{ (vaiTroBadge[r.vaiTro] || vaiTroBadge.SinhVien).label }}
                  </GlassBadge>
                </td>
                <td class="px-3 py-3.5 text-body">{{ r.donVi || r.lopHanhChinh || '—' }}</td>
                <td class="px-3 py-3.5 text-center">
                  <GlassBadge :variant="r.kichHoat ? 'success' : 'danger'" size="sm">
                    {{ r.kichHoat ? 'Hoạt động' : 'Đã khóa' }}
                  </GlassBadge>
                </td>
                <td class="px-3 py-3.5">
                  <div class="flex items-center justify-center gap-1">
                    <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Sửa" @click.stop="openEdit(r)">
                      <Pencil :size="15" />
                    </button>
                    <button class="h-8 w-8 rounded-lg hover:bg-(--color-warning-bg) flex items-center justify-center text-muted hover:text-(--color-warning-text) transition-colors" :title="r.kichHoat ? 'Khóa' : 'Kích hoạt'" @click.stop="toggleActive(r)">
                      <component :is="r.kichHoat ? X : Users" :size="15" />
                    </button>
                    <button class="h-8 w-8 rounded-lg hover:bg-(--color-info-bg) flex items-center justify-center text-muted hover:text-(--color-info-text) transition-colors" title="Đặt lại mật khẩu" @click.stop="openResetPwd(r)">
                      <KeyRound :size="14" />
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </template>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm tài khoản' : 'Chỉnh sửa tài khoản' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Họ tên <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.hoTen" type="text" placeholder="VD: TS. Nguyễn Văn An" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.hoTen ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.hoTen" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.hoTen }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Email <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.email" type="email" placeholder="VD: an.nv@lms.edu.vn" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.email ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.email" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.email }}</p>
              </div>
              <div v-if="formMode === 'create'">
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mật khẩu <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.matKhau" type="password" placeholder="Tối thiểu 8 ký tự" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.matKhau ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.matKhau" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.matKhau }}</p>
                <p class="mt-1 text-xs text-muted">Tên đăng nhập được tạo tự động từ email.</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Vai trò <span class="text-(--color-danger-text)">*</span></label>
                  <select v-model="formData.vaiTro" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.vaiTro ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" @change="handleVaiTroChange">
                    <option :value="null" disabled>Chọn vai trò</option>
                    <option v-for="r in roleOptions" :key="r.maVaiTro" :value="r.maVaiTro">{{ r.tenVaiTro }}</option>
                  </select>
                  <p v-if="formErrors.vaiTro" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.vaiTro }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Đơn vị <span class="text-(--color-danger-text)">*</span></label>
                  <select v-model="formData.donVi" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.donVi ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" @change="handleDonViChange">
                    <option :value="null" disabled>Chọn đơn vị</option>
                    <option v-for="o in organizationOptions" :key="o.maDonVi" :value="o.maDonVi">{{ o.tenDonVi }}</option>
                  </select>
                  <p v-if="formErrors.donVi" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.donVi }}</p>
                </div>
              </div>
              <div v-if="isStudentRole">
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Lớp hành chính <span class="text-(--color-danger-text)">*</span></label>
                <select v-model="formData.maLopHanhChinh" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maLopHanhChinh ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''">
                  <option :value="null" disabled>Chọn lớp</option>
                  <option v-for="c in classOptions" :key="c.maLop" :value="c.maLop">{{ c.tenLop }}</option>
                </select>
                <p v-if="formErrors.maLopHanhChinh" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maLopHanhChinh }}</p>
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm tài khoản' : 'Lưu thay đổi' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <!-- Reset password modal -->
    <Teleport to="body">
      <ConfirmActionDialog
        :model-value="showResetModal"
        title="Đặt lại mật khẩu"
        :message="`Nhập mật khẩu mới cho tài khoản ${resetTarget?.tenDangNhap || ''}.`"
        confirm-label="Đặt lại"
        :loading="resetting"
        @update:model-value="showResetModal = $event"
        @cancel="closeResetPwd"
        @confirm="submitResetPwd"
      >
        <div class="mt-2">
          <input v-model="resetPassword" type="password" placeholder="Mật khẩu mới (tối thiểu 8 ký tự)" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" @keydown.enter="submitResetPwd" />
          <p v-if="resetError" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ resetError }}</p>
        </div>
      </ConfirmActionDialog>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
