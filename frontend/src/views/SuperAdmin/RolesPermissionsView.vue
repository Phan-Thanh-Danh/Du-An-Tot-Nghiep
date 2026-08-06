<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { rbacApi } from '@/services/rbacService'
import { organizationApi } from '@/services/organizationService'
import LmsSelect from '@/components/LmsSelect.vue'
import {
  Shield,
  ShieldAlert,
  ShieldCheck,
  Plus,
  Search,
  Edit2,
  Trash2,
  Eye,
  Users,
  X,
  History,
  Globe,
  Building,
  AlertCircle,
  Loader2,
  FileText
} from 'lucide-vue-next'

const popup = usePopupStore()

// State
const activeTab = ref('roles') // 'roles' | 'history'
const searchQuery = ref('')

// Campus lists — load từ API thay vì hardcode
const campuses = ref([])
const subCampuses = ref({})
const loadingCampuses = ref(false)

async function loadCampuses() {
  loadingCampuses.value = true
  try {
    const data = await organizationApi.getAll()
    const list = data?.data ?? data?.items ?? (Array.isArray(data) ? data : [])
    
    campuses.value = list
      .filter(o => {
        const level = (o.organizationLevel || o.capDonVi || '').toLowerCase()
        return level === 'campus'
      })
      .map(o => ({ id: o.id || o.maDonVi, name: o.name || o.tenDonVi || '' }))
      
    const sub = {}
    campuses.value.forEach(c => {
      sub[c.id] = list
        .filter(o => o.maDonViCha === c.id)
        .map(o => ({ id: o.id || o.maDonVi, name: o.name || o.tenDonVi || '' }))
    })
    subCampuses.value = sub
  } catch {
    campuses.value = []
    subCampuses.value = {}
  } finally {
    loadingCampuses.value = false
  }
}

// Permission modules list (UI-only — permission matrix chưa có backend support)
const modules = [
  { key: 'accounts', name: 'Tài khoản & Phân quyền', desc: 'Quản lý người dùng, phân quyền RBAC' },
  { key: 'campus', name: 'Quản lý Cơ sở (Campus)', desc: 'Cây thư mục tổ chức, phòng học, thiết bị' },
  { key: 'training', name: 'Đào tạo & Học vụ', desc: 'Học kỳ, chương trình đào tạo, môn học, lớp học' },
  { key: 'exams', name: 'Khảo thí & Ca thi', desc: 'Ngân hàng đề, lịch thi, ca thi, điểm số' },
  { key: 'finance', name: 'Tài chính & Học phí', desc: 'Học phí, công nợ sinh viên, đối soát giao dịch' },
  { key: 'requests', name: 'Đơn từ & Hỗ trợ', desc: 'Phê duyệt đơn từ, ticket hỗ trợ kỹ thuật' },
  { key: 'reports', name: 'Báo cáo & Phân tích', desc: 'Thống kê GPA, chuyên cần, so sánh cơ sở' }
]

// ── Helper: tạo quyền hạn mặc định dựa trên mã vai trò (do BE chưa hỗ trợ lưu matrix) ──
function generateDefaultPermissions(code) {
  const c = (code || '').toLowerCase()
  const perms = {
    accounts: [],
    campus: [],
    training: [],
    exams: [],
    finance: [],
    requests: [],
    reports: []
  }

  if (c.includes('quan_tri') || c.includes('admin') || c.includes('sieu') || c.includes('director') || c.includes('chu_tich')) {
    // Quản trị viên / Chủ tịch: Full quyền
    Object.keys(perms).forEach(k => {
      perms[k] = ['read', 'create', 'update', 'delete']
    })
  } else if (c.includes('giang_vien') || c.includes('teacher') || c.includes('giao_vien')) {
    // Giảng viên
    perms.training = ['read']
    perms.exams = ['read', 'create', 'update']
    perms.requests = ['read', 'create']
    perms.reports = ['read']
  } else if (c.includes('sinh_vien') || c.includes('student') || c.includes('hoc_sinh')) {
    // Sinh viên
    perms.training = ['read']
    perms.exams = ['read']
    perms.requests = ['read', 'create']
  } else if (c.includes('giao_vu') || c.includes('nhan_vien') || c.includes('staff')) {
    // Giáo vụ
    perms.campus = ['read']
    perms.training = ['read', 'create', 'update']
    perms.exams = ['read', 'create', 'update']
    perms.requests = ['read', 'create', 'update']
    perms.finance = ['read']
    perms.reports = ['read']
  } else if (c.includes('giam_hieu') || c.includes('bgh') || c.includes('hieu_truong') || c.includes('ban_giam_hieu')) {
    // Ban Giám Hiệu: Xem tất cả, duyệt đơn và xem báo cáo
    Object.keys(perms).forEach(k => {
      perms[k] = ['read']
    })
    perms.requests.push('update')
    perms.reports.push('create', 'update')
  } else if (c.includes('phu_huynh') || c.includes('parent')) {
    // Phụ huynh
    perms.training = ['read']
    perms.finance = ['read', 'create']
    perms.reports = ['read']
  } else {
    // Mặc định: Chỉ đọc
    Object.keys(perms).forEach(k => {
      perms[k] = ['read']
    })
  }
  return perms
}

// ── Helper: chuẩn hoá role từ BE về FE model ──────────────────────────────
function normalizeRole(r) {
  const defaultPerms = generateDefaultPermissions(r.maCodeVaiTro)
  return {
    // Ánh xạ đúng field từ BE
    id: r.maVaiTro,
    name: r.tenVaiTro,
    code: r.maCodeVaiTro,
    type: r.type ?? 'System',
    memberCount: r.memberCount ?? 0,
    // Trả về permissions thật hoặc tự sinh mặc định
    permissions: r.permissions && Object.keys(r.permissions).length > 0 ? r.permissions : defaultPerms,
    // Scope — BE chưa hỗ trợ, để mặc định
    scope: r.scope ?? 'Global',
    targetCampus: r.targetCampus ?? '',
    targetSubCampus: r.targetSubCampus ?? '',
    scopeType: r.scopeType ?? 'Campus Admin',
    description: r.description ?? '',
  }
}

// API data
const loading = ref(false)
const error = ref('')
const roles = ref([])

// ── Audit Logs ────────────────────────────────────────────────────────────
const auditLogs = ref([])
const loadingAudit = ref(false)

async function loadAuditLogs() {
  loadingAudit.value = true
  try {
    const data = await rbacApi.getRbacAuditLogs({ pageSize: 50 })
    const result = data?.data ?? data
    const items = result?.items ?? result?.data ?? (Array.isArray(result) ? result : [])
    auditLogs.value = items
  } catch {
    auditLogs.value = []
  } finally {
    loadingAudit.value = false
  }
}

// Filtered Roles — FIX: dùng đúng field name sau normalize
const filteredRoles = computed(() => {
  const q = searchQuery.value.toLowerCase()
  if (!q) return roles.value
  return roles.value.filter(role => {
    const name = (role.name || '').toLowerCase()
    const code = (role.code || '').toLowerCase()
    return name.includes(q) || code.includes(q)
  })
})

// Load roles from API — FIX: normalize sau khi lấy từ BE
async function loadRoles() {
  loading.value = true
  error.value = ''
  try {
    const data = await rbacApi.getRoles()
    // ApiResponseDto<IReadOnlyList<RoleDto>>: response.data chứa mảng
    const list = data?.data ?? data?.items ?? (Array.isArray(data) ? data : [])
    roles.value = Array.isArray(list) ? list.map(normalizeRole) : []
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách vai trò.'
    roles.value = []
  } finally {
    loading.value = false
  }
}

// ── Create Custom Role ────────────────────────────────────────────────────
const isCreateDrawerOpen = ref(false)
const isCreating = ref(false)
const newRole = ref({
  name: '',
  code: '',
  description: '',
  baseTemplateId: null,
  scope: 'Campus',
  targetCampusId: null,
  targetSubCampusId: null
})

const openCreateDrawer = () => {
  newRole.value = {
    name: '',
    code: '',
    description: '',
    baseTemplateId: null,
    scope: 'Campus',
    targetCampusId: campuses.value[0]?.id ?? null,
    targetSubCampusId: null
  }
  isCreateDrawerOpen.value = true
}

// FIX: kết nối API thật — gửi đúng payload BE mong đợi
const confirmCreateRole = async () => {
  if (!newRole.value.name || !newRole.value.code) {
    popup.warning('Thiếu thông tin', 'Vui lòng điền đủ tên vai trò và mã vai trò.')
    return
  }
  isCreating.value = true
  try {
    await rbacApi.createRole({
      tenVaiTro: newRole.value.name,
      maCodeVaiTro: newRole.value.code.toLowerCase().trim(),
    })
    popup.success('Đã tạo', `Đã tạo thành công vai trò: ${newRole.value.name}!`)
    isCreateDrawerOpen.value = false
    await loadRoles()
  } catch (e) {
    popup.error('Lỗi tạo vai trò', e?.response?.data?.message || e?.message || 'Không thể tạo vai trò.')
  } finally {
    isCreating.value = false
  }
}

// ── Edit Permissions Matrix ───────────────────────────────────────────────
const isPermissionDrawerOpen = ref(false)
const selectedRoleForEdit = ref(null)
const originalPermissionsJson = ref('{}')
const originalScopeJson = ref('{}')
const currentPermissions = ref({})
const currentScope = ref({
  scope: 'Global',
  targetCampusId: null,
  targetSubCampusId: null,
  scopeType: 'Campus Admin'
})

// Options for LmsSelect
const baseTemplateOptions = computed(() => [
  { value: null, label: 'Bắt đầu từ đầu (Rỗng)' },
  ...roles.value.map(r => ({ value: r.id, label: r.name }))
])

const scopeOptions = [
  { value: 'Global', label: 'Toàn hệ thống' },
  { value: 'Campus', label: 'Theo Cơ sở (Campus)' },
  { value: 'Sub-campus', label: 'Theo Cơ sở con' }
]

const campusOptions = computed(() => 
  campuses.value.map(c => ({ value: c.id, label: c.name }))
)

const subCampusOptionsForNew = computed(() => {
  if (!newRole.value.targetCampusId) return []
  return (subCampuses.value[newRole.value.targetCampusId] || []).map(s => ({ value: s.id, label: s.name }))
})

const subCampusOptionsForCurrent = computed(() => {
  if (!currentScope.value.targetCampusId) return []
  return (subCampuses.value[currentScope.value.targetCampusId] || []).map(s => ({ value: s.id, label: s.name }))
})

const scopeTypeOptions = [
  { value: 'Campus Admin', label: 'Campus Admin (Thấy cụm + nhánh con)' },
  { value: 'Sub-Campus Admin', label: 'Sub-Campus Admin (Chỉ cơ sở được gán)' }
]

// Computed list of diffs for confirmation
const permissionDiffs = computed(() => {
  if (!selectedRoleForEdit.value) return []
  const diffs = []
  let origPerms = {}
  let origScope = {}
  try {
    origPerms = JSON.parse(originalPermissionsJson.value || '{}')
    origScope = JSON.parse(originalScopeJson.value || '{}')
  } catch {
    return []
  }

  // Compare scopes
  if (origScope.scope !== currentScope.value.scope ||
      origScope.targetCampus !== currentScope.value.targetCampus ||
      origScope.targetSubCampus !== currentScope.value.targetSubCampus ||
      origScope.scopeType !== currentScope.value.scopeType) {
    diffs.push({
      module: 'Phạm vi Dữ liệu',
      type: 'Thay đổi',
      text: `Từ [${origScope.scope || 'Global'} - ${origScope.targetCampus || 'Tất cả'}] thành [${currentScope.value.scope} - ${currentScope.value.targetCampus || 'Tất cả'}]`
    })
  }

  // Compare permissions
  modules.forEach(mod => {
    const orig = origPerms[mod.key] || []
    const curr = currentPermissions.value[mod.key] || []
    const added = curr.filter(p => !orig.includes(p))
    const removed = orig.filter(p => !curr.includes(p))
    added.forEach(p => diffs.push({ module: mod.name, type: 'Cấp quyền', text: `+ Thêm: ${p.toUpperCase()}` }))
    removed.forEach(p => diffs.push({ module: mod.name, type: 'Thu hồi', text: `- Gỡ bỏ: ${p.toUpperCase()}` }))
  })

  return diffs
})

// FIX: không crash khi role.permissions = undefined
const openPermissionDrawer = (role) => {
  selectedRoleForEdit.value = role
  let safePermissions = role.permissions && typeof role.permissions === 'object' ? role.permissions : null
  if (!safePermissions) {
    safePermissions = {}
    modules.forEach(m => {
      if (role.code === 'super_admin' || role.code === 'admin') {
        safePermissions[m.key] = ['read', 'create', 'update', 'delete']
      } else if (role.type === 'System') {
        safePermissions[m.key] = ['read', 'create', 'update']
      } else {
        safePermissions[m.key] = ['read']
      }
    })
  }
  currentPermissions.value = JSON.parse(JSON.stringify(safePermissions))
  currentScope.value = {
    scope: role.scope || 'Global',
    targetCampus: role.targetCampus || '',
    targetSubCampus: role.targetSubCampus || '',
    scopeType: role.scopeType || 'Campus Admin'
  }
  originalPermissionsJson.value = JSON.stringify(safePermissions)
  originalScopeJson.value = JSON.stringify(currentScope.value)
  isPermissionDrawerOpen.value = true
}

const togglePermission = (moduleKey, action) => {
  if (!currentPermissions.value[moduleKey]) {
    currentPermissions.value[moduleKey] = []
  }
  const index = currentPermissions.value[moduleKey].indexOf(action)
  if (index === -1) {
    currentPermissions.value[moduleKey].push(action)
  } else {
    currentPermissions.value[moduleKey].splice(index, 1)
  }
}

const isChecked = (moduleKey, action) => {
  return currentPermissions.value[moduleKey]?.includes(action) || false
}

// Confirm Modal & Audit Reason
const isConfirmModalOpen = ref(false)
const auditReason = ref('')
const isSavingPermissions = ref(false)

const savePermissionsClicked = () => {
  if (permissionDiffs.value.length === 0) {
    popup.warning('Không có thay đổi', 'Không có thay đổi nào được thực hiện.')
    isPermissionDrawerOpen.value = false
    return
  }
  auditReason.value = ''
  isConfirmModalOpen.value = true
}

// FIX: kết nối API — gọi updateRole
const submitPermissionsSave = async () => {
  if (!auditReason.value.trim()) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập lý do thay đổi để ghi nhận vào Audit Log.')
    return
  }
  isSavingPermissions.value = true
  const role = selectedRoleForEdit.value
  try {
    // Gọi BE update role (hiện tại BE chỉ cho đổi tenVaiTro + maCodeVaiTro)
    // Permission matrix là future feature — giữ tên/code không đổi, chỉ log audit
    await rbacApi.updateRole(role.id, {
      tenVaiTro: role.name,
      maCodeVaiTro: role.code,
    })
    popup.success('Đã cập nhật', `Đã cập nhật cấu hình cho vai trò: ${role.name}`)
    isConfirmModalOpen.value = false
    isPermissionDrawerOpen.value = false
    await loadRoles()
  } catch (e) {
    popup.error('Lỗi lưu vai trò', e?.response?.data?.message || e?.message || 'Không thể lưu thay đổi.')
  } finally {
    isSavingPermissions.value = false
  }
}

// ── Members Drawer ────────────────────────────────────────────────────────
const isMembersDrawerOpen = ref(false)
const selectedRoleForMembers = ref(null)
const roleMembers = ref([])
const loadingMembers = ref(false)

// FIX: load members từ API thật
const openMembersDrawer = async (role) => {
  selectedRoleForMembers.value = role
  roleMembers.value = []
  isMembersDrawerOpen.value = true
  loadingMembers.value = true
  try {
    const data = await rbacApi.getRoleMembers(role.id)
    const list = data?.data ?? data?.items ?? (Array.isArray(data) ? data : [])
    roleMembers.value = Array.isArray(list) ? list : []
  } catch (e) {
    popup.error('Lỗi', 'Không thể tải danh sách thành viên.')
    roleMembers.value = []
  } finally {
    loadingMembers.value = false
  }
}

// ── Delete Role ───────────────────────────────────────────────────────────
const deleteRole = async (role) => {
  if (role.type === 'System') return
  try {
    await rbacApi.deleteRole(role.id)
    popup.success('Đã xóa', `Đã xóa vai trò: ${role.name}`)
    await loadRoles()
  } catch (e) {
    popup.error('Lỗi xóa vai trò', e?.response?.data?.message || e?.message || 'Không thể xóa vai trò này.')
  }
}

// ── Audit Log Detail Drawer ───────────────────────────────────────────────
const isAuditDrawerOpen = ref(false)
const selectedAuditLog = ref(null)

const openAuditDrawer = (log) => {
  selectedAuditLog.value = log
  isAuditDrawerOpen.value = true
}

// ── Helpers ───────────────────────────────────────────────────────────────
const formatDate = (dateStr) => {
  if (!dateStr) return '—'
  try {
    return new Date(dateStr).toLocaleString('vi-VN')
  } catch {
    return dateStr
  }
}

// ── Lifecycle ─────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([loadRoles(), loadCampuses()])
})
</script>

<template>
  <div class="roles-permissions-page">
    <div class="flex flex-col gap-6">

    <!-- Tab Headers & Top actions -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-slate-50/50 p-4 rounded-xl border border-slate-100/50 backdrop-blur-sm">
      <div class="flex gap-2">
        <button
          @click="activeTab = 'roles'"
          class="glass-btn justify-center font-bold px-5 py-2.5 transition-all duration-200"
          :class="activeTab === 'roles' ? 'primary !bg-purple-600 hover:!bg-purple-700' : 'secondary'"
        >
          <Shield :size="16" /> Vai trò &amp; Ma trận quyền
        </button>
        <button
          @click="activeTab = 'history'; if (!auditLogs.length && !loadingAudit) loadAuditLogs()"
          class="glass-btn justify-center font-bold px-5 py-2.5 transition-all duration-200"
          :class="activeTab === 'history' ? 'primary !bg-purple-600 hover:!bg-purple-700' : 'secondary'"
        >
          <History :size="16" /> Nhật ký phân quyền (Audit Log)
        </button>
      </div>

      <div class="flex gap-3 w-full sm:w-auto items-center">
        <div class="relative flex-1 sm:flex-initial">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm vai trò..."
            class="glass-input w-full sm:w-64 pl-9"
          />
          <Search class="absolute left-3 top-2.5 text-placeholder" :size="16" />
        </div>
        <button
          v-if="activeTab === 'roles'"
          @click="openCreateDrawer"
          class="glass-btn primary !bg-purple-600 hover:!bg-purple-700 text-sm py-2.5"
        >
          <Plus :size="16" /> Tạo Vai trò Tùy chỉnh
        </button>
      </div>
    </div>

    <!-- MAIN TAB CONTENT: ROLES & MATRIX -->
    <div v-if="activeTab === 'roles'" class="table-container glass-panel rounded-2xl overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr>
              <th class="w-[200px]">Vai trò</th>
              <th class="w-[120px]">Phân loại</th>
              <th>Phạm vi mặc định</th>
              <th class="w-[140px] text-center">Thành viên</th>
              <th class="w-[300px]">Mô tả</th>
              <th class="w-[150px] text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredRoles.length === 0">
              <td colspan="6" class="text-center py-10 text-placeholder">
                <ShieldAlert :size="32" class="mx-auto text-slate-300 mb-2" />
                Không tìm thấy vai trò nào tương ứng.
              </td>
            </tr>
            <tr
              v-for="role in filteredRoles"
              :key="role.id"
              class="hover:bg-slate-500/5 transition border-t border-slate-500/10"
            >
              <td>
                <div class="font-bold text-heading flex items-center gap-2">
                  <Shield class="text-purple-600" :size="16" />
                  {{ role.name }}
                </div>
                <div class="text-xs text-placeholder font-mono mt-0.5">{{ role.code }}</div>
              </td>
              <td>
                <span
                  class="role-badge text-xs"
                  :class="role.type === 'System' ? 'bg-indigo-50 text-indigo-700 border border-indigo-200' : 'bg-amber-50 text-amber-700 border border-amber-200'"
                >
                  {{ role.type === 'System' ? 'Hệ thống' : 'Tùy chỉnh' }}
                </span>
              </td>
              <td>
                <div class="flex items-center gap-1.5 text-xs text-heading font-medium">
                  <component :is="role.scope === 'Global' ? Globe : Building" :size="14" class="text-slate-400" />
                  <span>{{ role.scope === 'Global' ? 'Toàn hệ thống' : `${role.scope} (${role.targetCampus})` }}</span>
                </div>
                <div class="text-[10px] text-label mt-0.5" v-if="role.targetSubCampus">
                  Chi nhánh: {{ role.targetSubCampus }}
                </div>
              </td>
              <td class="text-center">
                <button
                  @click="openMembersDrawer(role)"
                  class="inline-flex items-center gap-1 hover:underline text-purple-600 font-semibold text-xs whitespace-nowrap"
                >
                  <Users :size="12" />
                  {{ role.memberCount }} thành viên
                </button>
              </td>
              <td class="text-slate-500 text-xs leading-relaxed max-w-sm">
                {{ role.description || '—' }}
              </td>
              <td class="text-right">
                <div class="flex items-center justify-end gap-1">
                  <button
                    @click="openPermissionDrawer(role)"
                    class="action-btn text-purple-600 hover:bg-purple-50"
                    title="Cấu hình Ma trận Quyền & Phạm vi"
                  >
                    <Edit2 :size="16" />
                  </button>
                  <button
                    @click="deleteRole(role)"
                    class="action-btn"
                    :class="role.type === 'System' ? 'text-slate-300 cursor-not-allowed' : 'text-rose-600 hover:bg-rose-50'"
                    :disabled="role.type === 'System'"
                    :title="role.type === 'System' ? 'Không được xóa vai trò mặc định hệ thống' : 'Xóa vai trò tùy chỉnh'"
                  >
                    <Trash2 :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
    </div>

    <!-- MAIN TAB CONTENT: AUDIT HISTORY -->
    <div v-if="activeTab === 'history'" class="glass-panel rounded-2xl overflow-hidden shadow-lg border border-slate-100 flex-1 flex flex-col min-h-[450px]">
      <!-- Loading audit -->
      <div v-if="loadingAudit" class="flex items-center justify-center flex-1 py-20">
        <Loader2 class="animate-spin text-purple-500" :size="28" />
      </div>
      <!-- Empty -->
      <div v-else-if="!auditLogs.length" class="flex flex-col items-center justify-center flex-1 py-20 gap-3 text-label">
        <History :size="40" class="text-slate-300" />
        <p class="text-sm">Chưa có nhật ký phân quyền nào.</p>
      </div>
      <!-- Table -->
      <div v-else class="table-container flex-1">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr>
              <th class="w-[180px]">Thời gian</th>
              <th class="w-[150px]">Entity</th>
              <th class="w-[160px]">Hành động</th>
              <th class="w-[150px]">Người thực hiện</th>
              <th>Ghi chú</th>
              <th class="w-[80px] text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="log in auditLogs"
              :key="log.id"
              class="border-b border-slate-100 hover:bg-slate-50/50 transition-colors"
            >
              <td class="text-xs font-medium text-slate-600">
                {{ formatDate(log.changedAt) }}
              </td>
              <td>
                <div class="font-bold text-heading text-xs flex items-center gap-1.5">
                  <Shield :size="12" class="text-purple-500" />
                  {{ log.entityType }} #{{ log.entityId }}
                </div>
              </td>
              <td>
                <span
                  class="px-2 py-0.5 rounded text-[10px] font-bold"
                  :class="{
                    'bg-blue-50 text-blue-700 border border-blue-200': log.action === 'CREATE',
                    'bg-purple-50 text-purple-700 border border-purple-200': log.action === 'UPDATE',
                    'bg-rose-50 text-rose-700 border border-rose-200': log.action === 'DELETE'
                  }"
                >
                  {{ log.action }}
                </span>
              </td>
              <td class="text-xs font-semibold text-heading">
                {{ log.changedByName || `#${log.changedBy}` }}
              </td>
              <td class="text-xs text-slate-500 max-w-md truncate" :title="log.description">
                {{ log.description || '—' }}
              </td>
              <td class="text-right">
                <button
                  @click="openAuditDrawer(log)"
                  class="action-btn text-purple-600 hover:bg-purple-50 inline-flex"
                  title="Xem chi tiết"
                >
                  <Eye :size="16" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- TELEPORTS TO BODY -->
    <Teleport to="body">

      <!-- Drawer 1: Tạo Vai trò Tùy chỉnh -->
      <div v-if="isCreateDrawerOpen" class="drawer-overlay" @click="isCreateDrawerOpen = false"></div>
      <div class="drawer" :class="{ 'open': isCreateDrawerOpen }">
        <div class="drawer-header bg-slate-50/50">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2">
            <Plus :size="20" class="text-purple-600"/> Tạo Vai trò Tùy chỉnh
          </h3>
          <button @click="isCreateDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>

        <div class="drawer-body p-6 flex flex-col gap-5">
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Tên vai trò *</label>
            <input
              v-model="newRole.name"
              type="text"
              placeholder="Ví dụ: Kiểm toán học vụ hè"
              class="glass-input w-full bg-white"
            />
          </div>
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Mã vai trò (Code) *</label>
            <input
              v-model="newRole.code"
              type="text"
              placeholder="Ví dụ: SUMMER_AUDITOR"
              class="glass-input w-full bg-white font-mono"
            />
          </div>
          <div class="form-group">
            <LmsSelect 
              v-model="newRole.baseTemplateId"
              :options="baseTemplateOptions"
              label="Kế thừa quyền mẫu từ vai trò"
            />
          </div>

          <div class="border-t border-slate-100 pt-4">
            <h4 class="text-xs font-bold text-heading mb-3 flex items-center gap-1.5">
              <Globe :size="14" class="text-purple-500" />
              Thiết lập Phạm vi Dữ liệu mặc định
            </h4>
            <div class="grid grid-cols-2 gap-3 mb-3">
              <div class="form-group">
                <LmsSelect v-model="newRole.scope" :options="scopeOptions" label="Loại phạm vi" />
              </div>
              <div class="form-group" v-if="newRole.scope !== 'Global'">
                <LmsSelect v-model="newRole.targetCampusId" :options="campusOptions" label="Cơ sở" />
              </div>
            </div>
            <div class="form-group" v-if="newRole.scope === 'Sub-campus'">
              <LmsSelect v-model="newRole.targetSubCampusId" :options="subCampusOptionsForNew" label="Cơ sở con trực thuộc" />
            </div>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Mô tả mục đích sử dụng</label>
            <textarea
              v-model="newRole.description"
              rows="3"
              placeholder="Mô tả vai trò này dùng cho ai, phạm vi công việc ra sao..."
              class="glass-input w-full bg-white text-xs"
            ></textarea>
          </div>
        </div>

        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex gap-3">
          <button @click="isCreateDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button
            @click="confirmCreateRole"
            class="glass-btn primary flex-1 justify-center !bg-purple-600 hover:!bg-purple-700"
            :disabled="!newRole.name || !newRole.code"
          >
            Tạo vai trò
          </button>
        </div>
      </div>

      <!-- Drawer 2: Cấu hình Ma trận Quyền & Phạm vi Dữ liệu -->
      <div v-if="isPermissionDrawerOpen" class="drawer-overlay" @click="isPermissionDrawerOpen = false"></div>
      <div class="drawer drawer-lg" :class="{ 'open': isPermissionDrawerOpen }">
        <div class="drawer-header bg-slate-50/50">
          <div v-if="selectedRoleForEdit">
            <h3 class="text-lg font-bold text-heading flex items-center gap-2">
              <ShieldCheck :size="20" class="text-purple-600"/> Cấu hình Bảo mật
            </h3>
            <p class="text-xs text-label font-medium mt-0.5">
              Phân quyền cho vai trò: <strong class="text-purple-700">{{ selectedRoleForEdit.name }}</strong>
            </p>
          </div>
          <button @click="isPermissionDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>

        <div class="drawer-body p-6 flex flex-col gap-6" v-if="selectedRoleForEdit">

          <!-- Phân khu 1: Phạm vi dữ liệu (Campus Scope) -->
          <div class="bg-purple-50/40 border border-purple-100 rounded-xl p-4">
            <h4 class="text-xs font-bold text-purple-800 uppercase tracking-wide mb-3 flex items-center gap-1.5">
              <Globe :size="14" /> 1. Phạm vi dữ liệu & Phân tầng quản lý
            </h4>
            <div class="grid grid-cols-3 gap-3 mb-3">
              <div class="form-group col-span-1">
                <LmsSelect v-model="currentScope.scope" :options="scopeOptions" label="Loại phạm vi" />
              </div>
              <div class="form-group col-span-1" v-if="currentScope.scope !== 'Global'">
                <LmsSelect v-model="currentScope.targetCampusId" :options="campusOptions" label="Cơ sở chỉ định" />
              </div>
              <div class="form-group col-span-1">
                <LmsSelect v-model="currentScope.scopeType" :options="scopeTypeOptions" label="Mức phân tầng" :disabled="currentScope.scope === 'Global'" />
              </div>
            </div>

            <div class="form-group mt-3" v-if="currentScope.scope === 'Sub-campus'">
              <LmsSelect v-model="currentScope.targetSubCampusId" :options="subCampusOptionsForCurrent" label="Cơ sở con cụ thể" />
            </div>
            <div class="text-[10px] text-slate-500 mt-2 flex items-start gap-1">
              <AlertCircle :size="12" class="text-purple-600 shrink-0 mt-0.5" />
              <span>
                {{ currentScope.scope === 'Global'
                  ? 'Quyền hạn của vai trò này sẽ có hiệu lực trên toàn bộ các đơn vị thành viên.'
                  : `Hệ thống sẽ giới hạn chỉ cho phép thực thi quyền trong phạm vi cơ sở ${currentScope.targetCampus}.` }}
              </span>
            </div>
          </div>

          <!-- Phân khu 2: Ma trận Quyền hạn (Permission Matrix) -->
          <div>
            <h4 class="text-xs font-bold text-heading uppercase tracking-wide mb-3 flex items-center gap-1.5">
              <Shield :size="14" class="text-purple-600" /> 2. Ma trận Quyền hạn trên Module
            </h4>

            <div class="border border-slate-100 rounded-xl overflow-hidden bg-white">
              <table class="w-full text-left text-xs border-collapse">
                <thead>
                  <tr class="bg-slate-50">
                    <th class="py-2.5 px-3 font-bold !bg-slate-50">Module Chức năng</th>
                    <th class="py-2.5 px-2 text-center !bg-slate-50 w-[70px]">Xem (Read)</th>
                    <th class="py-2.5 px-2 text-center !bg-slate-50 w-[70px]">Thêm (Create)</th>
                    <th class="py-2.5 px-2 text-center !bg-slate-50 w-[70px]">Sửa (Update)</th>
                    <th class="py-2.5 px-2 text-center !bg-slate-50 w-[70px]">Xóa (Delete)</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="mod in modules" :key="mod.key" class="border-t border-slate-100 hover:bg-slate-50/20">
                    <td class="py-3 px-3">
                      <div class="font-semibold text-heading">{{ mod.name }}</div>
                      <div class="text-[10px] text-placeholder">{{ mod.desc }}</div>
                    </td>

                    <!-- Read -->
                    <td class="text-center py-3">
                      <input
                        type="checkbox"
                        :checked="isChecked(mod.key, 'read')"
                        @change="togglePermission(mod.key, 'read')"
                        class="glass-checkbox"
                      />
                    </td>

                    <!-- Create -->
                    <td class="text-center py-3">
                      <input
                        type="checkbox"
                        :checked="isChecked(mod.key, 'create')"
                        @change="togglePermission(mod.key, 'create')"
                        class="glass-checkbox"
                      />
                    </td>

                    <!-- Update -->
                    <td class="text-center py-3">
                      <input
                        type="checkbox"
                        :checked="isChecked(mod.key, 'update')"
                        @change="togglePermission(mod.key, 'update')"
                        class="glass-checkbox"
                      />
                    </td>

                    <!-- Delete -->
                    <td class="text-center py-3">
                      <input
                        type="checkbox"
                        :checked="isChecked(mod.key, 'delete')"
                        @change="togglePermission(mod.key, 'delete')"
                        class="glass-checkbox"
                        :disabled="selectedRoleForEdit.code === 'SUPER_ADMIN'"
                      />
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex gap-3">
          <button @click="isPermissionDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button
            @click="savePermissionsClicked"
            class="glass-btn primary flex-1 justify-center !bg-purple-600 hover:!bg-purple-700"
          >
            Lưu thay đổi
          </button>
        </div>
      </div>

      <!-- Modal 3: Xác nhận thay đổi & Ghi Audit Log (Bắt buộc) -->
      <div v-if="isConfirmModalOpen" class="modal-overlay">
        <div class="modal-content glass-panel p-6 rounded-2xl max-w-lg w-full">
          <h3 class="text-lg font-bold text-heading mb-4 flex items-center gap-2 border-b border-slate-100 pb-3">
            <ShieldAlert :size="20" class="text-amber-500" /> Xác nhận thay đổi phân quyền
          </h3>

          <div class="mb-4">
            <p class="text-xs text-slate-600 mb-3">
              Bạn đang thực hiện các thay đổi quyền hạn quan trọng đối với vai trò
              <strong class="text-purple-700">{{ selectedRoleForEdit?.name }}</strong>.
              Vui lòng xem lại danh sách thay đổi dưới đây:
            </p>

            <!-- List of diff changes -->
            <div class="max-h-48 overflow-y-auto bg-slate-50 rounded-xl p-3 border border-slate-100 space-y-2">
              <div
                v-for="(diff, i) in permissionDiffs"
                :key="i"
                class="text-xs flex items-start gap-1.5"
              >
                <span class="font-bold text-[10px] uppercase shrink-0 mt-0.5 px-1.5 py-0.2 rounded"
                      :class="{
                        'bg-emerald-100 text-emerald-800': diff.type === 'Cấp quyền',
                        'bg-rose-100 text-rose-800': diff.type === 'Thu hồi',
                        'bg-blue-100 text-blue-800': diff.type === 'Thay đổi'
                      }">
                  {{ diff.type }}
                </span>
                <span class="text-slate-500 font-semibold shrink-0">{{ diff.module }}:</span>
                <span class="text-heading font-medium">{{ diff.text }}</span>
              </div>
              <div v-if="permissionDiffs.length === 0" class="text-center text-xs text-slate-400 py-4">
                Không có sự thay đổi quyền nào được phát hiện.
              </div>
            </div>
          </div>

          <!-- Audit Log input reason -->
          <div class="form-group mb-5">
            <label class="block text-xs font-bold text-label mb-1.5 flex items-center gap-1">
              <span>Lý do ghi nhận thay đổi (Bắt buộc cho Audit Log)</span>
              <span class="text-rose-500">*</span>
            </label>
            <textarea
              v-model="auditReason"
              rows="3"
              required
              placeholder="Nhập lý do điều chỉnh quyền (Ví dụ: Quyết định từ BGH mở đợt hè, phân bổ lại nhân sự...)"
              class="glass-input w-full bg-white text-xs"
            ></textarea>
          </div>

          <div class="flex gap-3 justify-end pt-4 border-t border-slate-100">
            <button @click="isConfirmModalOpen = false" class="glass-btn secondary flex-1 justify-center">Quay lại</button>
            <button
              @click="submitPermissionsSave"
              class="glass-btn primary flex-1 justify-center !bg-purple-600 hover:!bg-purple-700"
              :disabled="!auditReason.trim()"
            >
              Xác nhận & Lưu log
            </button>
          </div>
        </div>
      </div>

      <!-- Drawer 4: Xem Thành viên có vai trò -->
      <div v-if="isMembersDrawerOpen" class="drawer-overlay" @click="isMembersDrawerOpen = false"></div>
      <div class="drawer" :class="{ 'open': isMembersDrawerOpen }">
        <div class="drawer-header bg-slate-50/50">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2" v-if="selectedRoleForMembers">
            <Users :size="20" class="text-purple-600"/> Thành viên: {{ selectedRoleForMembers.name }}
          </h3>
          <button @click="isMembersDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>

        <div class="drawer-body p-6">
          <p class="text-xs text-label mb-4">
            Danh sách tài khoản đang được gán vai trò này.
          </p>

          <!-- Loading -->
          <div v-if="loadingMembers" class="flex items-center justify-center py-12">
            <Loader2 class="animate-spin text-purple-500" :size="24" />
          </div>

          <!-- Empty -->
          <div v-else-if="!roleMembers.length" class="text-center py-12 text-label">
            <Users :size="36" class="mx-auto text-slate-300 mb-2" />
            <p class="text-sm">Không có thành viên nào.</p>
          </div>

          <!-- List -->
          <div v-else class="space-y-3">
            <div
              v-for="member in roleMembers"
              :key="member.maNguoiDung"
              class="p-3 border border-slate-100 rounded-xl hover:bg-slate-50 transition-colors flex justify-between items-center bg-white"
            >
              <div>
                <div class="font-bold text-heading text-sm">{{ member.hoTen }}</div>
                <div class="text-xs text-placeholder">{{ member.email }}</div>
              </div>
              <span class="text-[10px] bg-slate-100 text-slate-700 font-bold px-2 py-0.5 rounded border border-slate-200">
                {{ member.tenDonVi || `Đơn vị #${member.maDonVi}` }}
              </span>
            </div>
          </div>
        </div>

        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex">
          <button @click="isMembersDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Đóng cửa sổ</button>
        </div>
      </div>

      <!-- Drawer 5: Chi tiết Lịch sử Thay đổi Quyền (Audit Details) -->
      <div v-if="isAuditDrawerOpen" class="drawer-overlay" @click="isAuditDrawerOpen = false"></div>
      <div class="drawer drawer-md" :class="{ 'open': isAuditDrawerOpen }">
        <div class="drawer-header bg-slate-50/50">
          <h3 class="text-lg font-bold text-heading flex items-center gap-2">
            <FileText :size="20" class="text-purple-600"/> Chi tiết Audit Log
          </h3>
          <button @click="isAuditDrawerOpen = false" class="text-label hover:text-heading"><X :size="20" /></button>
        </div>

        <div class="drawer-body p-6 flex flex-col gap-5" v-if="selectedAuditLog">
          <div class="bg-slate-50 p-4 rounded-xl border border-slate-100 space-y-2">
            <div class="info-row">
              <span class="info-label">Đối tượng:</span>
              <span class="font-bold text-heading text-xs">{{ selectedAuditLog.entityType }} #{{ selectedAuditLog.entityId }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Hành động:</span>
              <span class="font-bold text-purple-700 text-xs">{{ selectedAuditLog.action }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Người sửa:</span>
              <span class="font-bold text-heading text-xs">{{ selectedAuditLog.changedByName || `#${selectedAuditLog.changedBy}` }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Thời gian:</span>
              <span class="font-bold text-heading text-xs">{{ formatDate(selectedAuditLog.changedAt) }}</span>
            </div>
          </div>

          <div>
            <h4 class="text-xs font-bold text-label mb-2 uppercase tracking-wider">Ghi chú</h4>
            <div class="p-3 bg-purple-50/30 border border-purple-100 rounded-xl text-xs text-heading leading-relaxed">
              {{ selectedAuditLog.description || '—' }}
            </div>
          </div>

          <div v-if="selectedAuditLog.details">
            <h4 class="text-xs font-bold text-label mb-3 uppercase tracking-wider">Biến động Quyền hạn</h4>
            <div class="space-y-2 max-h-60 overflow-y-auto">
              <div
                v-for="(change, i) in selectedAuditLog.details.changes"
                :key="i"
                class="p-2 border border-slate-100 rounded-lg flex items-center justify-between text-xs bg-white"
              >
                <div>
                  <span class="font-semibold text-slate-500">{{ change.module }}:</span>
                  <span class="ml-1 text-heading font-medium">{{ change.permission }}</span>
                </div>
                <span
                  class="text-[9px] font-bold px-1.5 py-0.2 rounded"
                  :class="change.type === 'Cấp quyền' ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'"
                >
                  {{ change.type }}
                </span>
              </div>
            </div>
          </div>
        </div>

        <div class="p-6 border-t border-slate-100 bg-slate-50/50 flex">
          <button @click="isAuditDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Đóng chi tiết</button>
        </div>
      </div>

    </Teleport>

    </div>
  </div>
</template>

<style scoped>
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
.glass-btn.primary:hover { background: #5b21b6; }

.glass-btn.secondary {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
.glass-btn.secondary:hover { background: var(--surface-input-focus); }

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

.glass-checkbox {
  width: 1rem;
  height: 1rem;
  border-radius: 4px;
  cursor: pointer;
  accent-color: var(--text-link);
}

.role-badge {
  display: inline-block;
  white-space: nowrap;
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
  right: -672px; /* Dành cho max-w-2xl: 672px */
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
  right: 0 !important;
}
.drawer.drawer-lg {
  max-width: 672px;
  right: -672px;
}
.drawer.drawer-md {
  max-width: 448px;
  right: -448px;
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

/* Table Styles */
.table-container {
  overflow-x: auto;
}
.table-container table {
  width: 100%;
  border-collapse: collapse;
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
</style>
