<script setup>
import { ref, computed, onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { rbacApi } from '@/services/rbacService'
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
  FileText
} from 'lucide-vue-next'

const ENABLE_MOCK_API =
  import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const popup = usePopupStore()

// State
const activeTab = ref('roles') // 'roles' | 'history'
const searchQuery = ref('')

// Campus & Sub-campus lists for Scope configuration
const campuses = ['Hà Nội', 'TP.HCM', 'Đà Nẵng', 'Cần Thơ']
const subCampuses = {
  'Hà Nội': ['Cơ sở 1 (Cầu Giấy)', 'Cơ sở 2 (Mỹ Đình)'],
  'TP.HCM': ['Cơ sở Quận 9', 'Cơ sở Quận 12'],
  'Đà Nẵng': ['Cơ sở Hải Châu', 'Cơ sở Liên Chiểu'],
  'Cần Thơ': ['Cơ sở Ninh Kiều']
}

// Mock modules list
const modules = [
  { key: 'accounts', name: 'Tài khoản & Phân quyền', desc: 'Quản lý người dùng, phân quyền RBAC' },
  { key: 'campus', name: 'Quản lý Cơ sở (Campus)', desc: 'Cây thư mục tổ chức, phòng học, thiết bị' },
  { key: 'training', name: 'Đào tạo & Học vụ', desc: 'Học kỳ, chương trình đào tạo, môn học, lớp học' },
  { key: 'exams', name: 'Khảo thí & Ca thi', desc: 'Ngân hàng đề, lịch thi, ca thi, điểm số' },
  { key: 'finance', name: 'Tài chính & Học phí', desc: 'Học phí, công nợ sinh viên, đối soát giao dịch' },
  { key: 'requests', name: 'Đơn từ & Hỗ trợ', desc: 'Phê duyệt đơn từ, ticket hỗ trợ kỹ thuật' },
  { key: 'reports', name: 'Báo cáo & Phân tích', desc: 'Thống kê GPA, chuyên cần, so sánh cơ sở' }
]

// Mock Roles Data (fallback)
const mockRoles = [
  {
    id: 1,
    name: 'Super Admin',
    code: 'SUPER_ADMIN',
    type: 'System',
    scope: 'Global',
    targetCampus: '',
    targetSubCampus: '',
    scopeType: 'Global Admin',
    memberCount: 2,
    description: 'Quyền tối cao trên toàn hệ thống, không bị giới hạn cơ sở hay quyền hạn.',
    permissions: {
      'accounts': ['read', 'create', 'update', 'delete'],
      'campus': ['read', 'create', 'update', 'delete'],
      'training': ['read', 'create', 'update', 'delete'],
      'exams': ['read', 'create', 'update', 'delete'],
      'finance': ['read', 'create', 'update', 'delete'],
      'requests': ['read', 'create', 'update', 'delete'],
      'reports': ['read', 'create', 'update', 'delete']
    }
  },
  {
    id: 2,
    name: 'Giảng viên',
    code: 'TEACHER',
    type: 'System',
    scope: 'Campus',
    targetCampus: 'Hà Nội',
    targetSubCampus: '',
    scopeType: 'Sub-Campus Admin',
    memberCount: 85,
    description: 'Vai trò dạy học và chấm điểm tại cơ sở được phân công trực tiếp.',
    permissions: {
      'accounts': ['read'],
      'campus': ['read'],
      'training': ['read', 'update'],
      'exams': ['read', 'create', 'update'],
      'finance': [],
      'requests': ['read', 'create'],
      'reports': ['read']
    }
  },
  {
    id: 3,
    name: 'Sinh viên',
    code: 'STUDENT',
    type: 'System',
    scope: 'Sub-campus',
    targetCampus: 'Hà Nội',
    targetSubCampus: 'Cơ sở 1 (Cầu Giấy)',
    scopeType: 'Sub-Campus Admin',
    memberCount: 1420,
    description: 'Học viên tham gia học tập, đăng ký môn và thanh toán học phí cá nhân.',
    permissions: {
      'accounts': ['read'],
      'campus': ['read'],
      'training': ['read'],
      'exams': ['read'],
      'finance': ['read', 'create'],
      'requests': ['read', 'create'],
      'reports': []
    }
  },
  {
    id: 4,
    name: 'Giáo vụ',
    code: 'ACADEMIC_STAFF',
    type: 'System',
    scope: 'Campus',
    targetCampus: 'Toàn bộ',
    targetSubCampus: '',
    scopeType: 'Campus Admin',
    memberCount: 12,
    description: 'Quản lý học vụ, điều phối thời khóa biểu và danh sách lớp học.',
    permissions: {
      'accounts': ['read'],
      'campus': ['read', 'create', 'update'],
      'training': ['read', 'create', 'update', 'delete'],
      'exams': ['read', 'create', 'update'],
      'finance': ['read'],
      'requests': ['read', 'create', 'update'],
      'reports': ['read', 'create']
    }
  },
  {
    id: 5,
    name: 'Admin Cơ sở Hà Nội',
    code: 'HN_CAMPUS_ADMIN',
    type: 'Custom',
    scope: 'Campus',
    targetCampus: 'Hà Nội',
    targetSubCampus: '',
    scopeType: 'Campus Admin',
    memberCount: 3,
    description: 'Quản trị viên phụ trách toàn bộ chi nhánh và cơ sở thành viên thuộc khu vực Hà Nội.',
    permissions: {
      'accounts': ['read', 'create', 'update'],
      'campus': ['read', 'create', 'update'],
      'training': ['read', 'create', 'update'],
      'exams': ['read', 'create', 'update'],
      'finance': ['read'],
      'requests': ['read', 'create', 'update'],
      'reports': ['read']
    }
  }
]

// Mock Audit Logs Data (fallback)
const mockAuditLogs = [
  {
    id: 1,
    roleName: 'Admin Cơ sở Hà Nội',
    action: 'Cập nhật Quyền hạn & Phạm vi',
    operator: 'Super Admin A',
    time: '2026-06-04 10:20:15',
    reason: 'Mở rộng quyền cập nhật học vụ để kịp tiến độ tuyển sinh đợt 2.',
    details: {
      scopeBefore: 'Sub-campus (Cơ sở 1)',
      scopeAfter: 'Campus (Hà Nội)',
      changes: [
        { module: 'Đào tạo & Học vụ', type: 'Cấp quyền', permission: 'Sửa (Update)' },
        { module: 'Tài chính & Học phí', type: 'Cấp quyền', permission: 'Xem (Read)' }
      ]
    }
  },
  {
    id: 2,
    roleName: 'Khảo thí Tùy biến',
    action: 'Tạo Vai trò mới',
    operator: 'Super Admin A',
    time: '2026-06-03 14:05:22',
    reason: 'Tạo vai trò riêng biệt cho bộ phận khảo thí học kỳ hè.',
    details: null
  }
]

// API data
const loading = ref(false)
const error = ref('')
const roles = ref([])
const auditLogs = ref([])

// Filtered Roles
const filteredRoles = computed(() => {
  return roles.value.filter(role => {
    return role.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
           role.code.toLowerCase().includes(searchQuery.value.toLowerCase())
  })
})

// Load roles from API (with mock fallback)
async function loadRoles() {
  loading.value = true
  error.value = ''
  try {
    const data = await rbacApi.getRoles()
    roles.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch (e) {
    if (ENABLE_MOCK_API) {
      roles.value = JSON.parse(JSON.stringify(mockRoles))
      auditLogs.value = JSON.parse(JSON.stringify(mockAuditLogs))
      return
    }
    error.value = e?.message || 'Không thể tải danh sách vai trò.'
    roles.value = []
  } finally {
    loading.value = false
  }
}

// Create Custom Role State
const isCreateDrawerOpen = ref(false)
const newRole = ref({
  name: '',
  code: '',
  description: '',
  baseTemplateId: null,
  scope: 'Campus',
  targetCampus: 'Hà Nội',
  targetSubCampus: ''
})

// Edit Permissions Matrix State
const isPermissionDrawerOpen = ref(false)
const selectedRoleForEdit = ref(null)
const originalPermissionsJson = ref('')
const originalScopeJson = ref('')
const currentPermissions = ref({})
const currentScope = ref({
  scope: 'Campus',
  targetCampus: 'Hà Nội',
  targetSubCampus: '',
  scopeType: 'Campus Admin'
})

// Confirm Modal & Audit Reason
const isConfirmModalOpen = ref(false)
const auditReason = ref('')

// Members Drawer State
const isMembersDrawerOpen = ref(false)
const selectedRoleForMembers = ref(null)
const mockMembersList = [
  { id: 101, name: 'Lê Hoàng Long', email: 'longlh@lms.edu.vn', campus: 'Hà Nội' },
  { id: 102, name: 'Nguyễn Thị Minh', email: 'minhnt@lms.edu.vn', campus: 'Hà Nội' },
  { id: 103, name: 'Trần Minh Quân', email: 'quantm@lms.edu.vn', campus: 'TP.HCM' }
]

// Audit Drawer State
const isAuditDrawerOpen = ref(false)
const selectedAuditLog = ref(null)

// Computed list of diffs for confirmation
const permissionDiffs = computed(() => {
  if (!selectedRoleForEdit.value) return []
  const diffs = []
  const origPerms = JSON.parse(originalPermissionsJson.value)
  const origScope = JSON.parse(originalScopeJson.value)

  // Compare scopes
  if (origScope.scope !== currentScope.value.scope ||
      origScope.targetCampus !== currentScope.value.targetCampus ||
      origScope.targetSubCampus !== currentScope.value.targetSubCampus ||
      origScope.scopeType !== currentScope.value.scopeType) {
    diffs.push({
      module: 'Phạm vi Dữ liệu',
      type: 'Thay đổi',
      text: `Từ [${origScope.scope} - ${origScope.targetCampus || 'Tất cả'} (${origScope.scopeType})] thành [${currentScope.value.scope} - ${currentScope.value.targetCampus || 'Tất cả'} (${currentScope.value.scopeType})]`
    })
  }

  // Compare permissions
  modules.forEach(mod => {
    const orig = origPerms[mod.key] || []
    const curr = currentPermissions.value[mod.key] || []

    const added = curr.filter(p => !orig.includes(p))
    const removed = orig.filter(p => !curr.includes(p))

    added.forEach(p => {
      diffs.push({
        module: mod.name,
        type: 'Cấp quyền',
        text: `+ Thêm hành động: ${p.toUpperCase()}`
      })
    })

    removed.forEach(p => {
      diffs.push({
        module: mod.name,
        type: 'Thu hồi',
        text: `- Gỡ bỏ hành động: ${p.toUpperCase()}`
      })
    })
  })

  return diffs
})

// Handlers
const openCreateDrawer = () => {
  newRole.value = {
    name: '',
    code: '',
    description: '',
    baseTemplateId: null,
    scope: 'Campus',
    targetCampus: 'Hà Nội',
    targetSubCampus: ''
  }
  isCreateDrawerOpen.value = true
}

const confirmCreateRole = () => {
  if (!newRole.value.name || !newRole.value.code) {
    popup.warning('Thiếu thông tin', 'Vui lòng điền đủ tên vai trò và mã vai trò.')
    return
  }

  // Find template if any
  let templatePermissions = {
    'accounts': ['read'],
    'campus': ['read'],
    'training': ['read'],
    'exams': [],
    'finance': [],
    'requests': [],
    'reports': []
  }

  if (newRole.value.baseTemplateId) {
    const template = roles.value.find(r => r.id === newRole.value.baseTemplateId)
    if (template) {
      templatePermissions = JSON.parse(JSON.stringify(template.permissions))
    }
  }

  const roleObj = {
    id: roles.value.length + 1,
    name: newRole.value.name,
    code: newRole.value.code.toUpperCase(),
    type: 'Custom',
    scope: newRole.value.scope,
    targetCampus: newRole.value.scope === 'Global' ? '' : newRole.value.targetCampus,
    targetSubCampus: newRole.value.scope === 'Sub-campus' ? newRole.value.targetSubCampus : '',
    scopeType: newRole.value.scope === 'Campus' ? 'Campus Admin' : (newRole.value.scope === 'Sub-campus' ? 'Sub-Campus Admin' : 'Global Admin'),
    memberCount: 0,
    description: newRole.value.description || 'Vai trò tùy chỉnh được tạo bởi Super Admin.',
    permissions: templatePermissions
  }

  roles.value.push(roleObj)

  // Write log
  auditLogs.value.unshift({
    id: auditLogs.value.length + 1,
    roleName: roleObj.name,
    action: 'Tạo Vai trò mới',
    operator: 'Super Admin A',
    time: new Date().toLocaleString(),
    reason: `Khởi tạo vai trò tùy chỉnh mới: ${roleObj.name}.`,
    details: null
  })

  popup.success('Đã tạo', `Đã tạo thành công vai trò tùy chỉnh: ${roleObj.name}!`)
  isCreateDrawerOpen.value = false
}

const openPermissionDrawer = (role) => {
  selectedRoleForEdit.value = role
  currentPermissions.value = JSON.parse(JSON.stringify(role.permissions))
  currentScope.value = {
    scope: role.scope,
    targetCampus: role.targetCampus || 'Hà Nội',
    targetSubCampus: role.targetSubCampus || '',
    scopeType: role.scopeType || 'Campus Admin'
  }

  originalPermissionsJson.value = JSON.stringify(role.permissions)
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

const savePermissionsClicked = () => {
  if (permissionDiffs.value.length === 0) {
    popup.warning('Không có thay đổi', 'Không có thay đổi nào được thực hiện.')
    isPermissionDrawerOpen.value = false
    return
  }
  auditReason.value = ''
  isConfirmModalOpen.value = true
}

const submitPermissionsSave = () => {
  if (!auditReason.value.trim()) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập lý do thay đổi để ghi nhận vào Audit Log.')
    return
  }

  const role = selectedRoleForEdit.value
  const index = roles.value.findIndex(r => r.id === role.id)

  if (index !== -1) {
    roles.value[index].permissions = JSON.parse(JSON.stringify(currentPermissions.value))
    roles.value[index].scope = currentScope.value.scope
    roles.value[index].targetCampus = currentScope.value.scope === 'Global' ? '' : currentScope.value.targetCampus
    roles.value[index].targetSubCampus = currentScope.value.scope === 'Sub-campus' ? currentScope.value.targetSubCampus : ''
    roles.value[index].scopeType = currentScope.value.scopeType
  }

  // Add Audit Log
  auditLogs.value.unshift({
    id: auditLogs.value.length + 1,
    roleName: role.name,
    action: 'Cập nhật Quyền hạn & Phạm vi',
    operator: 'Super Admin A',
    time: new Date().toLocaleString(),
    reason: auditReason.value,
    details: {
      scopeBefore: JSON.parse(originalScopeJson.value).scope,
      scopeAfter: currentScope.value.scope,
      changes: permissionDiffs.value.map(d => ({
        module: d.module,
        type: d.type,
        permission: d.text
      }))
    }
  })

  popup.success('Đã cập nhật', `Đã cập nhật cấu hình bảo mật cho vai trò: ${role.name}`)
  isConfirmModalOpen.value = false
  isPermissionDrawerOpen.value = false
}

const openMembersDrawer = (role) => {
  selectedRoleForMembers.value = role
  isMembersDrawerOpen.value = true
}

const deleteRole = (role) => {
  if (role.type === 'System') return
  roles.value = roles.value.filter(r => r.id !== role.id)

  // Add Audit Log
  auditLogs.value.unshift({
    id: auditLogs.value.length + 1,
    roleName: role.name,
    action: 'Xóa Vai trò',
    operator: 'Super Admin A',
    time: new Date().toLocaleString(),
    reason: `Xóa vai trò tùy chỉnh ${role.name} khỏi hệ thống.`,
    details: null
  })

  popup.success('Đã xóa', `Đã xóa vai trò: ${role.name}`)
}

const openAuditDrawer = (log) => {
  selectedAuditLog.value = log
  isAuditDrawerOpen.value = true
}

onMounted(() => {
  loadRoles()
})
</script>

<template>
  <div class="roles-permissions-page h-full flex flex-col gap-6">

    <!-- Tab Headers & Top actions -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 bg-slate-50/50 p-4 rounded-xl border border-slate-100/50 backdrop-blur-sm">
      <div class="flex gap-2">
        <button
          @click="activeTab = 'roles'"
          class="glass-btn justify-center font-bold px-5 py-2.5 transition-all duration-200"
          :class="activeTab === 'roles' ? 'primary !bg-purple-600 hover:!bg-purple-700' : 'secondary'"
        >
          <Shield :size="16" /> Vai trò & Ma trận quyền
        </button>
        <button
          @click="activeTab = 'history'"
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
    <div v-if="activeTab === 'roles'" class="glass-panel rounded-2xl overflow-hidden shadow-lg border border-slate-100 flex-1 flex flex-col min-h-[450px]">
      <div class="table-container flex-1">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr>
              <th class="w-[200px]">Vai trò</th>
              <th class="w-[120px]">Phân loại</th>
              <th class="w-[200px]">Phạm vi mặc định</th>
              <th class="w-[120px] text-center">Thành viên</th>
              <th>Mô tả</th>
              <th class="w-[150px] text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="role in filteredRoles"
              :key="role.id"
              class="border-b border-slate-100 hover:bg-slate-50/50 transition-colors"
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
                  class="inline-flex items-center gap-1 hover:underline text-purple-600 font-semibold text-xs"
                >
                  <Users :size="12" />
                  {{ role.memberCount }} thành viên
                </button>
              </td>
              <td class="text-slate-500 text-xs leading-relaxed max-w-sm">
                {{ role.description }}
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
            <tr v-if="filteredRoles.length === 0">
              <td colspan="6" class="text-center py-16 text-slate-400">
                <ShieldAlert :size="32" class="mx-auto text-slate-300 mb-2" />
                Không tìm thấy vai trò nào tương ứng.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- MAIN TAB CONTENT: AUDIT HISTORY -->
    <div v-if="activeTab === 'history'" class="glass-panel rounded-2xl overflow-hidden shadow-lg border border-slate-100 flex-1 flex flex-col min-h-[450px]">
      <div class="table-container flex-1">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr>
              <th class="w-[180px]">Thời gian</th>
              <th class="w-[180px]">Vai trò chịu tác động</th>
              <th class="w-[200px]">Hành động</th>
              <th class="w-[150px]">Người thực hiện</th>
              <th>Lý do điều chỉnh (Audit Log)</th>
              <th class="w-[100px] text-right">Chi tiết</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="log in mockAuditLogs"
              :key="log.id"
              class="border-b border-slate-100 hover:bg-slate-50/50 transition-colors"
            >
              <td class="text-xs font-medium text-slate-600">
                {{ log.time }}
              </td>
              <td>
                <div class="font-bold text-heading text-xs flex items-center gap-1.5">
                  <Shield :size="12" class="text-purple-500" />
                  {{ log.roleName }}
                </div>
              </td>
              <td>
                <span
                  class="px-2 py-0.5 rounded text-[10px] font-bold"
                  :class="{
                    'bg-blue-50 text-blue-700 border border-blue-200': log.action.includes('Tạo'),
                    'bg-purple-50 text-purple-700 border border-purple-200': log.action.includes('Cập nhật'),
                    'bg-rose-50 text-rose-700 border border-rose-200': log.action.includes('Xóa')
                  }"
                >
                  {{ log.action }}
                </span>
              </td>
              <td class="text-xs font-semibold text-heading">
                {{ log.operator }}
              </td>
              <td class="text-xs text-slate-500 max-w-md truncate" :title="log.reason">
                {{ log.reason }}
              </td>
              <td class="text-right">
                <button
                  v-if="log.details"
                  @click="openAuditDrawer(log)"
                  class="action-btn text-purple-600 hover:bg-purple-50 inline-flex"
                  title="Xem chi tiết lịch sử đổi quyền"
                >
                  <Eye :size="16" />
                </button>
                <span v-else class="text-xs text-slate-300 px-2">-</span>
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
            <label class="block text-xs font-bold text-label mb-1.5">Kế thừa quyền mẫu từ vai trò</label>
            <select v-model="newRole.baseTemplateId" class="glass-select w-full bg-white">
              <option :value="null">Bắt đầu từ đầu (Rỗng)</option>
              <option v-for="r in mockRoles" :key="r.id" :value="r.id">{{ r.name }}</option>
            </select>
          </div>

          <div class="border-t border-slate-100 pt-4">
            <h4 class="text-xs font-bold text-heading mb-3 flex items-center gap-1.5">
              <Globe :size="14" class="text-purple-500" />
              Thiết lập Phạm vi Dữ liệu mặc định
            </h4>
            <div class="grid grid-cols-2 gap-3 mb-3">
              <div class="form-group">
                <label class="block text-[10px] font-bold text-label mb-1">Loại phạm vi</label>
                <select v-model="newRole.scope" class="glass-select w-full bg-white text-xs">
                  <option value="Global">Toàn hệ thống</option>
                  <option value="Campus">Theo Cơ sở (Campus)</option>
                  <option value="Sub-campus">Theo Cơ sở con</option>
                </select>
              </div>
              <div class="form-group" v-if="newRole.scope !== 'Global'">
                <label class="block text-[10px] font-bold text-label mb-1">Cơ sở</label>
                <select v-model="newRole.targetCampus" class="glass-select w-full bg-white text-xs">
                  <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
                </select>
              </div>
            </div>
            <div class="form-group" v-if="newRole.scope === 'Sub-campus'">
              <label class="block text-[10px] font-bold text-label mb-1">Cơ sở con trực thuộc</label>
              <select v-model="newRole.targetSubCampus" class="glass-select w-full bg-white text-xs">
                <option v-for="sub in subCampuses[newRole.targetCampus] || []" :key="sub" :value="sub">
                  {{ sub }}
                </option>
              </select>
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
                <label class="block text-[10px] font-bold text-label mb-1">Loại phạm vi</label>
                <select v-model="currentScope.scope" class="glass-select w-full bg-white text-xs">
                  <option value="Global">Toàn hệ thống</option>
                  <option value="Campus">Theo Cơ sở (Campus)</option>
                  <option value="Sub-campus">Cơ sở con (Sub-campus)</option>
                </select>
              </div>
              <div class="form-group col-span-1" v-if="currentScope.scope !== 'Global'">
                <label class="block text-[10px] font-bold text-label mb-1">Cơ sở chỉ định</label>
                <select v-model="currentScope.targetCampus" class="glass-select w-full bg-white text-xs">
                  <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
                </select>
              </div>
              <div class="form-group col-span-1">
                <label class="block text-[10px] font-bold text-label mb-1">Mức phân tầng</label>
                <select v-model="currentScope.scopeType" class="glass-select w-full bg-white text-xs" :disabled="currentScope.scope === 'Global'">
                  <option value="Campus Admin">Campus Admin (Thấy cụm + nhánh con)</option>
                  <option value="Sub-Campus Admin">Sub-Campus Admin (Chỉ cơ sở được gán)</option>
                </select>
              </div>
            </div>

            <div class="form-group mt-3" v-if="currentScope.scope === 'Sub-campus'">
              <label class="block text-[10px] font-bold text-label mb-1">Cơ sở con cụ thể</label>
              <select v-model="currentScope.targetSubCampus" class="glass-select w-full bg-white text-xs">
                <option v-for="sub in subCampuses[currentScope.targetCampus] || []" :key="sub" :value="sub">
                  {{ sub }}
                </option>
              </select>
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
            Dưới đây là danh sách những tài khoản đang trực tiếp chịu ảnh hưởng của các cấu hình bảo mật thuộc vai trò này.
          </p>

          <div class="space-y-3">
            <div
              v-for="member in mockMembersList"
              :key="member.id"
              class="p-3 border border-slate-100 rounded-xl hover:bg-slate-50 transition-colors flex justify-between items-center bg-white"
            >
              <div>
                <div class="font-bold text-heading text-sm">{{ member.name }}</div>
                <div class="text-xs text-placeholder">{{ member.email }}</div>
              </div>
              <span class="text-[10px] bg-slate-100 text-slate-700 font-bold px-2 py-0.5 rounded border border-slate-200">
                {{ member.campus }}
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
              <span class="font-bold text-heading text-xs">{{ selectedAuditLog.roleName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Hành động:</span>
              <span class="font-bold text-purple-700 text-xs">{{ selectedAuditLog.action }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Người sửa:</span>
              <span class="font-bold text-heading text-xs">{{ selectedAuditLog.operator }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Thời gian:</span>
              <span class="font-bold text-heading text-xs">{{ selectedAuditLog.time }}</span>
            </div>
          </div>

          <div>
            <h4 class="text-xs font-bold text-label mb-2 uppercase tracking-wider">Lý do điều chỉnh (Mục đích)</h4>
            <div class="p-3 bg-purple-50/30 border border-purple-100 rounded-xl text-xs text-heading leading-relaxed">
              {{ selectedAuditLog.reason }}
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
