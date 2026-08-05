<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  Building2,
  MapPin,
  Mail,
  Phone,
  Users,
  BookOpen,
  Monitor,
  Lock,
  Unlock,
  Edit2,
  Plus,
  Shield,
  ChevronRight,
  ChevronDown,
  AlertTriangle,
  CheckCircle2,
  Save,
  X,
  FileEdit,
  AlertCircle,
  Info,
  Trash2,
  UserCog,
  UserCheck,
  Network,
  GitBranch,
  ZoomIn,
  ZoomOut,
  RotateCcw,
  MousePointer
} from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import { organizationApi } from '@/services/organizationService'
import OrgChartNode from '@/components/SuperAdmin/OrgChartNode.vue'

const popup = usePopupStore()

// Data & View Mode
const loading = ref(false)
const error = ref('')
const organizationTree = ref([])
const activeViewMode = ref('tree') // 'tree' or 'chart'

// Zoom State & Handlers for Family Tree Org Chart
const zoomScale = ref(1)
const minZoom = 0.3
const maxZoom = 2.5
const isRightMouseDown = ref(false)

const handleCanvasMouseDown = (e) => {
  if (e.button === 2) {
    isRightMouseDown.value = true
  }
}

const handleCanvasMouseUp = (e) => {
  if (e.button === 2) {
    isRightMouseDown.value = false
  }
}

const handleCanvasContextMenu = (e) => {
  // Prevent default context menu when right-clicking canvas to allow smooth right-click + wheel zoom
  e.preventDefault()
}

const handleCanvasWheel = (e) => {
  // Trigger zoom when holding right mouse button OR holding Ctrl key
  if (e.buttons === 2 || isRightMouseDown.value || e.ctrlKey) {
    e.preventDefault()
    const zoomFactor = e.deltaY < 0 ? 0.1 : -0.1
    const newScale = Math.min(maxZoom, Math.max(minZoom, zoomScale.value + zoomFactor))
    zoomScale.value = Number(newScale.toFixed(2))
  }
}

const zoomIn = () => {
  zoomScale.value = Math.min(maxZoom, Number((zoomScale.value + 0.15).toFixed(2)))
}

const zoomOut = () => {
  zoomScale.value = Math.max(minZoom, Number((zoomScale.value - 0.15).toFixed(2)))
}

const resetZoom = () => {
  zoomScale.value = 1
}



// Mapping helper to convert Backend organization DTOs to Frontend model structure
function mapTreeNodes(nodes) {
  if (!Array.isArray(nodes)) return []
  return nodes.map(node => {
    const id = node.id || node.Id || 0
    const parentId = node.parentId !== undefined ? node.parentId : (node.ParentId !== undefined ? node.ParentId : null)
    const name = node.name || node.Name || ''
    
    // Map OrganizationLevel to type
    let type = 'Campus'
    const level = node.organizationLevel || node.OrganizationLevel || ''
    if (level.toLowerCase() === 'root') type = 'Root'
    else if (level.toLowerCase() === 'subcampus' || level.toLowerCase() === 'sub-campus') type = 'Sub-campus'

    // Map IsActive to status
    const isActive = node.isActive !== undefined ? node.isActive : (node.IsActive !== undefined ? node.IsActive : true)
    const status = isActive ? 'Active' : 'Locked'

    const code = node.code || node.Code || (name ? name.split(' ').map(w => w.charAt(0)).join('').toUpperCase() + '_' + id : 'ORG_' + id)

    const rawMetrics = node.metrics || node.Metrics
    const metrics = {
      users: rawMetrics?.users !== undefined ? rawMetrics.users : (rawMetrics?.Users !== undefined ? rawMetrics.Users : 0),
      classes: rawMetrics?.classes !== undefined ? rawMetrics.classes : (rawMetrics?.Classes !== undefined ? rawMetrics.Classes : 0),
      rooms: rawMetrics?.rooms !== undefined ? rawMetrics.rooms : (rawMetrics?.Rooms !== undefined ? rawMetrics.Rooms : 0)
    }

    const admins = node.admins || node.Admins || []


    return {
      id: id,
      parentId: parentId,
      name: name,
      code: code,
      type: type,
      status: status,
      expanded: node.expanded !== undefined ? node.expanded : true,
      address: node.address || node.Address || 'Khu CNC Hòa Lạc, Thạch Thất, Hà Nội',
      email: node.email || node.Email || `${code.toLowerCase()}@fpt.edu.vn`,
      phone: node.phone || node.Phone || '024 7300 1866',
      metrics: metrics,
      admins: admins,
      children: mapTreeNodes(node.children || node.Children)
    }
  })
}

async function loadOrganizations() {
  loading.value = true
  error.value = ''
  try {
    const data = await organizationApi.getTree()
    const rawTree = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
    organizationTree.value = mapTreeNodes(rawTree)
  } catch (e) {
    error.value = e?.response?.data?.message || e?.message || 'Không thể tải cây tổ chức.'
    organizationTree.value = []
  } finally {
    loading.value = false
  }
}


function findOrgInTree(nodes, id) {
  for (const node of nodes) {
    if (node.id === id) return node
    if (node.children?.length) {
      const found = findOrgInTree(node.children, id)
      if (found) return found
    }
  }
  return null
}

// Flat list for Parent Selector
const existingOrgs = computed(() => {
  const list = []
  const traverse = (orgs) => {
    orgs.forEach(o => {
      list.push({ id: o.id, code: o.code, name: o.name, type: o.type })
      if (o.children && o.children.length > 0) traverse(o.children)
    })
  }
  traverse(organizationTree.value)
  return list
})

const existingCodes = computed(() => existingOrgs.value.map(o => o.code.toUpperCase()))

const selectedOrg = ref(null)
const isEditing = ref(false)
const isCreating = ref(false)

const form = ref({
  id: null,
  parentId: null,
  code: '',
  name: '',
  type: 'Campus',
  status: 'Active',
  address: '',
  email: '',
  phone: ''
})

// Validation State for Form
const errors = ref({
  code: '',
  name: '',
  parent: ''
})

watch(form, (newVal) => {
  if (!isEditing.value && !isCreating.value) return
  errors.value = { code: '', name: '', parent: '' }

  // Check Code
  if (!newVal.code) {
    errors.value.code = 'Mã cơ sở không được để trống.'
  } else if (existingCodes.value.includes(newVal.code.toUpperCase()) && (!isEditing.value || (isEditing.value && newVal.code.toUpperCase() !== selectedOrg.value?.code.toUpperCase()))) {
    errors.value.code = 'Mã cơ sở này đã tồn tại trên hệ thống (Unique Code).'
  }

  // Check Name
  if (!newVal.name) {
    errors.value.name = 'Tên cơ sở không được để trống.'
  }

  // Check Parent logic
  if (newVal.type === 'Campus' && newVal.parentId !== 1) {
    errors.value.parent = 'Cơ sở cấp Campus bắt buộc phải trực thuộc Root.'
  } else if (newVal.type === 'Sub-campus') {
    const parent = existingOrgs.value.find(o => o.id === newVal.parentId)
    if (!parent) {
      errors.value.parent = 'Vui lòng chọn cơ sở cha hợp lệ.'
    } else if (parent.type !== 'Campus') {
      errors.value.parent = 'Sub-campus bắt buộc phải trực thuộc một Campus.'
    }
  }
}, { deep: true })

const isFormValid = computed(() => {
  return !errors.value.code && !errors.value.name && !errors.value.parent
})

// Modals
const isLockModalOpen = ref(false)
const lockReason = ref('')

const isAssignRoleModalOpen = ref(false)
const roleForm = ref({ name: '', role: 'Campus Admin' })

// Handlers
const selectOrg = (org) => {
  selectedOrg.value = org
  isEditing.value = false
  isCreating.value = false
}

const toggleExpand = (org) => {
  org.expanded = !org.expanded
}

const startCreate = (parentOrg) => {
  isCreating.value = true
  isEditing.value = false
  form.value = {
    id: null,
    parentId: parentOrg ? parentOrg.id : 1,
    code: '',
    name: '',
    type: parentOrg ? (parentOrg.type === 'Root' ? 'Campus' : 'Sub-campus') : 'Campus',
    status: 'Active',
    address: '',
    email: '',
    phone: ''
  }
}

const startEdit = () => {
  isEditing.value = true
  isCreating.value = false
  form.value = { ...selectedOrg.value, parentId: 1 }
}

const isCancelModalOpen = ref(false)

const cancelForm = () => {
  isCancelModalOpen.value = true
}

const confirmCancel = () => {
  isEditing.value = false
  isCreating.value = false
  isCancelModalOpen.value = false
}

const submitForm = async () => {
  if (!isFormValid.value) {
    popup.warning('Lỗi', 'Vui lòng sửa các lỗi trong Bảng kiểm tra trước khi lưu.')
    return
  }

  const levelMapped = form.value.type === 'Sub-campus' ? 'SubCampus' : form.value.type
  const isActiveMapped = form.value.status === 'Active'
  const parentIdMapped = form.value.parentId === 1 ? null : form.value.parentId

  const payload = {
    parentId: parentIdMapped,
    name: form.value.name,
    organizationLevel: levelMapped,
    isActive: isActiveMapped
  }

  try {
    if (isEditing.value) {
      await organizationApi.update(form.value.id, payload)
    } else {
      await organizationApi.create(payload)
    }
    popup.success('Thành công', `Đã ${isEditing.value ? 'cập nhật' : 'tạo mới'} cơ sở thành công! Đã ghi Audit Log.`)
    isEditing.value = false
    isCreating.value = false
    await loadOrganizations()
    if (organizationTree.value.length > 0) {
      // Auto re-select active or first
      selectedOrg.value = selectedOrg.value ? findOrgInTree(organizationTree.value, selectedOrg.value.id) : organizationTree.value[0]
    }
  } catch (e) {
    popup.error('Lỗi', e?.response?.data?.message || e?.message || 'Không thể lưu cơ sở.')
  }
}

const submitDraft = async () => {
  const levelMapped = form.value.type === 'Sub-campus' ? 'SubCampus' : form.value.type
  const parentIdMapped = form.value.parentId === 1 ? null : form.value.parentId

  const payload = {
    parentId: parentIdMapped,
    name: form.value.name,
    organizationLevel: levelMapped,
    isActive: false // Draft is Inactive
  }

  try {
    if (isEditing.value) {
      await organizationApi.update(form.value.id, payload)
    } else {
      await organizationApi.create(payload)
    }
    popup.success('Thành công', 'Đã lưu bản nháp (Draft). Các thay đổi chưa kích hoạt chính thức.')
    isEditing.value = false
    isCreating.value = false
    await loadOrganizations()
  } catch (e) {
    popup.error('Lỗi', e?.response?.data?.message || e?.message || 'Không thể lưu bản nháp.')
  }
}

const openLockModal = () => {
  lockReason.value = ''
  isLockModalOpen.value = true
}

const confirmLockAction = async () => {
  const isCurrentlyLocked = selectedOrg.value?.status === 'Locked'
  const levelMapped = selectedOrg.value.type === 'Sub-campus' ? 'SubCampus' : selectedOrg.value.type
  const parentIdMapped = selectedOrg.value.parentId === 1 ? null : selectedOrg.value.parentId

  const payload = {
    parentId: parentIdMapped,
    name: selectedOrg.value.name,
    organizationLevel: levelMapped,
    isActive: isCurrentlyLocked // If currently locked, set active to true (unlock). Otherwise false.
  }

  try {
    await organizationApi.update(selectedOrg.value.id, payload)
    if (isCurrentlyLocked) {
      selectedOrg.value.status = 'Active'
      selectedOrg.value.lockReason = null
      popup.success('Thành công', `Đã mở khóa cơ sở ${selectedOrg.value.name}.`)
    } else {
      selectedOrg.value.status = 'Locked'
      selectedOrg.value.lockReason = lockReason.value || 'Khóa bởi quản trị viên'
      popup.success('Thành công', `Đã khóa cơ sở ${selectedOrg.value.name}. Hệ thống chặn tạo mới dữ liệu tại cơ sở này.`)
    }
    isLockModalOpen.value = false
    await loadOrganizations()
  } catch (e) {
    popup.error('Lỗi', e?.response?.data?.message || e?.message || 'Không thể thực hiện thao tác khóa/mở khóa.')
  }
}


const openAssignModal = () => {
  roleForm.value = { name: '', role: selectedOrg.value?.type === 'Campus' ? 'Campus Admin' : 'Sub-Campus Admin' }
  isAssignRoleModalOpen.value = true
}

const confirmAssignRole = () => {
  if (!roleForm.value.name) return
  selectedOrg.value.admins.push({ ...roleForm.value })
  popup.success('Thành công', `Đã gán quyền ${roleForm.value.role} cho ${roleForm.value.name} tại cơ sở ${selectedOrg.value.name}.`)
  isAssignRoleModalOpen.value = false
}

const revokeAdmin = (idx) => {
  popup.success('Thành công', `Đã thu hồi quyền Admin của ${selectedOrg.value.admins[idx].name}. (Đã ghi Log)`)
  selectedOrg.value.admins.splice(idx, 1)
}

// Helpers
const getStatusBadge = (status) => {
  if (status === 'Active') return { class: 'bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-500/30', label: 'Hoạt động' }
  if (status === 'Locked') return { class: 'bg-rose-500/15 text-rose-700 dark:text-rose-300 border border-rose-300 dark:border-rose-500/30', label: 'Bị khóa' }
  if (status === 'Draft') return { class: 'bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-300 dark:border-amber-500/30', label: 'Bản nháp' }
  return { class: 'bg-slate-500/15 text-slate-700 dark:text-slate-300 border border-slate-300 dark:border-slate-500/30', label: status }
}
const getTypeIcon = (type) => {
  if (type === 'Root') return Shield
  if (type === 'Campus') return Building2
  return MapPin
}

const systemAdmins = ref([
  { id: 1, name: 'Nguyễn Văn Admin', email: 'admin.hn@fpt.edu.vn', role: 'Campus Admin' },
  { id: 2, name: 'Lê HCM Admin', email: 'admin.hcm@fpt.edu.vn', role: 'Campus Admin' },
  { id: 3, name: 'Trần Detech', email: 'detech.hn@fpt.edu.vn', role: 'Sub-Campus Admin' },
  { id: 4, name: 'Phạm Thị Giáo Vụ', email: 'staff.hn@fpt.edu.vn', role: 'AcademicStaff' },
  { id: 5, name: 'Hoàng Văn Hiệu Trưởng', email: 'principal.hn@fpt.edu.vn', role: 'Principal' },
  { id: 6, name: 'Nguyễn Thị Admin 2', email: 'admin2.hn@fpt.edu.vn', role: 'Campus Admin' },
])

const route = useRoute()
onMounted(async () => {
  await loadOrganizations()
  if (organizationTree.value.length > 0 && !selectedOrg.value) {
    selectedOrg.value = organizationTree.value[0]
  }
  if (route.query.action === 'create') {
    startCreate(null)
  }
})
</script>

<template>
  <div v-if="loading" class="p-4">
    <ListSkeleton :rows="6" />
  </div>
  <div v-else-if="error" class="glass-panel rounded-2xl p-12 flex flex-col items-center justify-center mb-6">
    <AlertCircle :size="40" class="text-rose-400 mb-3" />
    <p class="text-rose-600 font-semibold mb-2">{{ error }}</p>
    <button @click="loadOrganizations" class="glass-btn primary text-xs">Thử lại</button>
  </div>
  <div v-else class="org-hierarchy-page pb-10">
    <header class="page-header mb-6">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-bold text-heading">Quản lý Cấu trúc Đa cơ sở (Hierarchy)</h1>
          <p class="text-sm text-label mt-1">Giao diện điều hành trung tâm: Thiết lập, điều phối, khóa và cấp quyền cho tất cả Campus/Sub-campus.</p>
        </div>
        <div class="flex flex-wrap items-center gap-3">
          <!-- View Mode Switcher -->
          <div class="flex items-center gap-1 rounded-xl border border-default surface-card p-1 shadow-sm">
            <button
              type="button"
              class="inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-bold transition-all"
              :class="activeViewMode === 'tree' ? 'bg-blue-600 text-white shadow-sm' : 'text-muted hover:text-heading'"
              @click="activeViewMode = 'tree'"
            >
              <Network class="h-3.5 w-3.5" />
              Chế độ Cây
            </button>

            <button
              type="button"
              class="inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-xs font-bold transition-all"
              :class="activeViewMode === 'chart' ? 'bg-purple-600 text-white shadow-sm' : 'text-muted hover:text-heading'"
              @click="activeViewMode = 'chart'"
            >
              <GitBranch class="h-3.5 w-3.5" />
              Sơ đồ Gia phả
            </button>
          </div>

          <button v-if="!isCreating && !isEditing" @click="startCreate(null)" class="glass-btn primary shadow-sm">
            <Plus :size="16" /> Tạo cơ sở mới
          </button>
          
          <template v-if="isCreating || isEditing">
            <button @click="cancelForm" class="glass-btn secondary shadow-sm"><X :size="16" /> Hủy</button>
            <button @click="submitDraft" class="glass-btn shadow-sm !bg-amber-100 !text-amber-700 !border-amber-200 hover:!bg-amber-200">
              <FileEdit :size="16" /> Lưu nháp (Draft)
            </button>
            <button @click="submitForm" class="glass-btn primary shadow-sm" :disabled="!isFormValid" :class="{'opacity-50 cursor-not-allowed': !isFormValid}">
              <Save :size="16" /> {{ isEditing ? 'Cập nhật' : 'Tạo mới' }}
            </button>
          </template>
        </div>
      </div>
    </header>

    <!-- ================= 1. TREE VIEW MODE ================= -->
    <div v-if="activeViewMode === 'tree'" class="flex flex-col lg:flex-row gap-6">

      <!-- Cây tổ chức (Tree View) -->
      <div class="lg:w-1/3 flex flex-col gap-4">
        <div class="glass-panel p-4 rounded-2xl h-full flex flex-col">
          <h3 class="font-bold text-heading mb-4 flex items-center gap-2 border-b border-default pb-2"><Building2 :size="18" class="text-blue-600 dark:text-blue-400" /> Sơ đồ Tổ chức (Tree)</h3>
          
          <div class="tree-container flex-1">
            <ul class="tree-list">
              <li v-for="root in organizationTree" :key="root.id" class="tree-node">
                <div class="tree-item" :class="{ 'active': selectedOrg?.id === root.id }" @click="selectOrg(root)">
                  <button @click.stop="toggleExpand(root)" class="expand-btn">
                    <component :is="root.expanded ? ChevronDown : ChevronRight" :size="14" />
                  </button>
                  <Shield :size="14" class="text-purple-600 dark:text-purple-400 shrink-0" />
                  <span class="font-bold text-sm truncate">{{ root.name }}</span>
                  <span class="status-dot ml-auto shrink-0" :class="root.status === 'Locked' ? 'bg-rose-500' : 'bg-emerald-500'"></span>
                </div>
                
                <ul v-if="root.expanded && root.children" class="tree-list child-list">
                  <li v-if="root.children.length === 0" class="py-1.5 px-3 text-xs text-placeholder italic">
                    Chưa có cơ sở trực thuộc
                  </li>
                  <li v-for="campus in root.children" :key="campus.id" class="tree-node">
                    <div class="tree-item" :class="{ 'active': selectedOrg?.id === campus.id }" @click="selectOrg(campus)">
                      <button @click.stop="toggleExpand(campus)" class="expand-btn" :class="{ 'invisible': !campus.children || !campus.children.length }">
                        <component :is="campus.expanded ? ChevronDown : ChevronRight" :size="14" />
                      </button>
                      <Building2 :size="14" class="text-blue-600 dark:text-blue-400 shrink-0" />
                      <span class="font-semibold text-sm truncate">{{ campus.name }}</span>
                      <span class="status-dot ml-auto shrink-0" :class="campus.status === 'Locked' ? 'bg-rose-500' : 'bg-emerald-500'"></span>
                    </div>

                    <ul v-if="campus.expanded && campus.children" class="tree-list child-list">
                      <li v-if="campus.children.length === 0" class="py-1.5 px-3 text-xs text-placeholder italic">
                        Chưa có cơ sở con trực thuộc
                      </li>
                      <li v-for="sub in campus.children" :key="sub.id" class="tree-node">
                        <div class="tree-item" :class="{ 'active': selectedOrg?.id === sub.id }" @click="selectOrg(sub)">
                          <span class="w-5 shrink-0"></span> <!-- spacing -->
                          <MapPin :size="14" class="text-teal-600 dark:text-teal-400 shrink-0" />
                          <span class="text-sm truncate">{{ sub.name }}</span>
                          <span class="status-dot ml-auto shrink-0" :class="sub.status === 'Locked' ? 'bg-rose-500' : 'bg-emerald-500'"></span>
                        </div>
                      </li>
                    </ul>
                  </li>
                </ul>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Right Panel: View Detail OR Form -->
      <div class="lg:w-2/3">
        
        <!-- ================= VIEW MODE ================= -->
        <div v-if="!isEditing && !isCreating && selectedOrg" class="glass-panel rounded-2xl overflow-hidden shadow-sm border border-default">
          <!-- Header detail -->
          <div class="p-6 border-b border-default lg-glass-soft flex justify-between items-start">
            <div class="flex gap-4 items-start">
              <div class="w-14 h-14 rounded-2xl flex items-center justify-center text-white shadow-md"
                   :class="selectedOrg.type === 'Root' ? 'bg-purple-600' : (selectedOrg.type === 'Campus' ? 'bg-blue-600' : 'bg-teal-600')">
                <component :is="getTypeIcon(selectedOrg.type)" :size="28" />
              </div>
              <div>
                <div class="flex items-center gap-2 mb-1">
                  <span class="text-xs font-bold uppercase tracking-wide" :class="selectedOrg.type === 'Root' ? 'text-purple-600 dark:text-purple-400' : (selectedOrg.type === 'Campus' ? 'text-blue-600 dark:text-blue-400' : 'text-teal-600 dark:text-teal-400')">
                    {{ selectedOrg.type }}
                  </span>
                  <span class="px-2.5 py-0.5 rounded-full text-[10px] font-extrabold" :class="getStatusBadge(selectedOrg.status).class">
                    {{ getStatusBadge(selectedOrg.status).label }}
                  </span>
                </div>
                <h2 class="text-2xl font-bold text-heading">{{ selectedOrg.name }}</h2>
                <p class="text-sm font-mono text-muted mt-0.5">Mã cơ sở: {{ selectedOrg.code }}</p>
              </div>
            </div>
            
            <div class="flex items-center gap-2">
              <button @click="startEdit" class="glass-btn secondary"><Edit2 :size="14" /> Cập nhật</button>
              <button @click="openLockModal" class="glass-btn" :class="selectedOrg.status === 'Locked' ? 'primary' : 'danger'">
                <component :is="selectedOrg.status === 'Locked' ? Unlock : Lock" :size="14" />
                {{ selectedOrg.status === 'Locked' ? 'Mở khóa' : 'Khóa cơ sở' }}
              </button>
            </div>
          </div>

          <!-- Lock Notice -->
          <div v-if="selectedOrg.status === 'Locked'" class="mx-6 mt-6 p-4 rounded-xl bg-rose-500/10 border border-rose-300 dark:border-rose-500/30 flex items-start gap-3 text-rose-700 dark:text-rose-300">
            <AlertTriangle :size="20" class="shrink-0 mt-0.5 text-rose-500" />
            <div>
              <h4 class="font-bold text-sm">Cơ sở đang bị khóa</h4>
              <p class="text-xs mt-1">Lý do: <strong>{{ selectedOrg.lockReason }}</strong></p>
              <p class="text-xs font-semibold mt-1">Hệ thống đang chặn tạo mới Người dùng, Lớp học và Phòng học tại cơ sở này để bảo vệ dữ liệu.</p>
            </div>
          </div>

          <!-- Metrics Cards -->
          <div class="p-6 grid grid-cols-3 gap-4">
            <div class="stat-card">
              <Users :size="20" class="text-blue-500 mb-2" />
              <span class="stat-label">Tổng người dùng</span>
              <span class="stat-value">{{ selectedOrg.metrics?.users.toLocaleString() || 0 }}</span>
            </div>
            <div class="stat-card">
              <BookOpen :size="20" class="text-teal-500 mb-2" />
              <span class="stat-label">Lớp học (Active)</span>
              <span class="stat-value">{{ selectedOrg.metrics?.classes.toLocaleString() || 0 }}</span>
            </div>
            <div class="stat-card">
              <Monitor :size="20" class="text-amber-500 mb-2" />
              <span class="stat-label">Phòng học (Rooms)</span>
              <span class="stat-value">{{ selectedOrg.metrics?.rooms.toLocaleString() || 0 }}</span>
            </div>
          </div>

          <!-- Tabs Info / Admin -->
          <div class="px-6 pb-6">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Contact -->
              <div class="info-box border border-default rounded-xl p-4 lg-glass-soft">
                <h4 class="font-bold text-heading text-sm mb-4 border-b border-default pb-2">Thông tin liên lạc</h4>
                <div class="space-y-3 text-sm">
                  <div class="flex gap-3"><MapPin :size="16" class="text-placeholder shrink-0 mt-0.5" /> <span class="text-body">{{ selectedOrg.address || 'Chưa cập nhật' }}</span></div>
                  <div class="flex gap-3"><Mail :size="16" class="text-placeholder shrink-0 mt-0.5" /> <span class="text-body">{{ selectedOrg.email || 'Chưa cập nhật' }}</span></div>
                  <div class="flex gap-3"><Phone :size="16" class="text-placeholder shrink-0 mt-0.5" /> <span class="text-body">{{ selectedOrg.phone || 'Chưa cập nhật' }}</span></div>
                </div>
              </div>

              <!-- Admins -->
              <div class="info-box border border-default rounded-xl p-4 lg-glass-soft">
                <div class="flex items-center justify-between border-b border-default pb-2 mb-4">
                  <h4 class="font-bold text-heading text-sm">Phân quyền (Admin Scope)</h4>
                  <button @click="openAssignModal" class="text-xs text-blue-600 dark:text-blue-400 font-bold hover:underline flex items-center gap-1"><Plus :size="12"/> Gán quyền</button>
                </div>
                <div v-if="!selectedOrg.admins || selectedOrg.admins.length === 0" class="text-sm text-placeholder italic">Chưa có Admin nào được gán.</div>
                <div v-else class="space-y-2">
                  <div v-for="(admin, idx) in selectedOrg.admins" :key="idx" class="group flex items-center justify-between surface-card p-2 rounded-lg border border-default shadow-sm hover:border-slate-400 transition-all">
                    <div class="flex items-center gap-2">
                      <div class="w-8 h-8 rounded-full bg-slate-200 dark:bg-slate-700 flex items-center justify-center text-xs font-bold text-heading">{{ admin.name.charAt(0) }}</div>
                      <div>
                        <span class="block text-sm font-semibold text-heading leading-tight">{{ admin.name }}</span>
                        <span class="block text-[10px] font-bold uppercase text-blue-600 dark:text-blue-400 mt-0.5">{{ admin.role }}</span>
                      </div>
                    </div>
                    <button @click="revokeAdmin(idx)" class="text-rose-500 hover:text-rose-700 opacity-0 group-hover:opacity-100 transition-opacity p-1.5 bg-rose-500/10 rounded-md" title="Thu hồi quyền">
                      <Trash2 :size="16" />
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>


        <!-- ================= FORM MODE ================= -->
        <div v-else-if="isEditing || isCreating" class="glass-panel p-6 rounded-2xl shadow-md border-2 border-blue-100/50">
          <div class="flex items-center gap-3 border-b border-slate-100 pb-4 mb-6">
            <div class="w-10 h-10 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center">
              <FileEdit :size="20" />
            </div>
            <div>
              <h2 class="text-xl font-bold text-heading">{{ isCreating ? 'Tạo mới Tổ chức / Cơ sở' : 'Cập nhật Thông tin Cơ sở' }}</h2>
              <p class="text-[11px] text-label uppercase tracking-widest font-semibold mt-0.5">Real-time Validation Enabled</p>
            </div>
          </div>
          
          <div class="grid grid-cols-1 md:grid-cols-5 gap-6">
            <!-- Left Form Content -->
            <div class="md:col-span-3 space-y-5">
              <div class="grid grid-cols-2 gap-4">
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Mã cơ sở (Unique) <span class="text-rose-500">*</span></label>
                  <input v-model="form.code" type="text" class="glass-input w-full uppercase" placeholder="VD: FPT_HN" :class="{'border-rose-300 bg-rose-50': errors.code}" />
                </div>
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Tên cơ sở <span class="text-rose-500">*</span></label>
                  <input v-model="form.name" type="text" class="glass-input w-full" placeholder="FPT University Hà Nội" :class="{'border-rose-300 bg-rose-50': errors.name}" />
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4">
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Loại cơ sở</label>
                  <select v-model="form.type" class="glass-select w-full" :disabled="form.type === 'Root'">
                    <option value="Campus">Campus (Cơ sở chính)</option>
                    <option value="Sub-campus">Sub-campus (Chi nhánh con)</option>
                  </select>
                </div>
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Trực thuộc (Parent) <span class="text-rose-500">*</span></label>
                  <select v-model="form.parentId" class="glass-select w-full" :class="{'border-rose-300 bg-rose-50': errors.parent}">
                    <option v-for="org in existingOrgs" :key="org.id" :value="org.id">
                      {{ org.type === 'Root' ? '🌟' : (org.type === 'Campus' ? '🏢' : '📍') }} {{ org.name }}
                    </option>
                  </select>
                </div>
              </div>

              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Trạng thái (Status)</label>
                <select v-model="form.status" class="glass-select w-full">
                  <option value="Active">Hoạt động (Active)</option>
                  <option value="Inactive">Tạm ngưng (Inactive)</option>
                  <option value="Draft">Bản nháp (Draft)</option>
                </select>
              </div>

              <div class="border-t border-slate-100 pt-5 space-y-4">
                <h4 class="text-sm font-bold text-heading">Thông tin liên hệ</h4>
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Địa chỉ</label>
                  <input v-model="form.address" type="text" class="glass-input w-full" placeholder="Địa chỉ đầy đủ..." />
                </div>
                <div class="grid grid-cols-2 gap-4">
                  <div class="form-group">
                    <label class="block text-xs font-bold text-label mb-1">Email</label>
                    <input v-model="form.email" type="email" class="glass-input w-full" placeholder="email@fpt.edu.vn" />
                  </div>
                  <div class="form-group">
                    <label class="block text-xs font-bold text-label mb-1">Điện thoại</label>
                    <input v-model="form.phone" type="text" class="glass-input w-full" placeholder="024..." />
                  </div>
                </div>
              </div>
            </div>

            <!-- Right Validation Panel -->
            <div class="md:col-span-2">
              <div class="rounded-xl overflow-hidden border-2 h-full flex flex-col" :class="isFormValid ? 'border-emerald-200' : 'border-rose-200'">
                <div class="p-3 flex items-center gap-2 border-b border-slate-100" :class="isFormValid ? 'bg-emerald-50' : 'bg-rose-50'">
                  <component :is="isFormValid ? CheckCircle2 : AlertCircle" :size="18" :class="isFormValid ? 'text-emerald-600' : 'text-rose-600'" />
                  <div>
                    <h3 class="font-bold text-xs" :class="isFormValid ? 'text-emerald-800' : 'text-rose-800'">Kiểm tra dữ liệu (Validation)</h3>
                  </div>
                </div>
                
                <div class="p-4 space-y-4 flex-1 bg-slate-50/50">
                  <div>
                    <div class="flex items-start gap-2 text-sm mb-1">
                      <component :is="!errors.code ? CheckCircle2 : X" :size="16" class="shrink-0 mt-0.5" :class="!errors.code ? 'text-emerald-500' : 'text-rose-500'" />
                      <span class="font-semibold" :class="!errors.code ? 'text-slate-700' : 'text-rose-600'">Mã cơ sở (Unique)</span>
                    </div>
                    <p v-if="errors.code" class="text-[11px] text-rose-500 pl-6 leading-tight">{{ errors.code }}</p>
                  </div>
                  
                  <div>
                    <div class="flex items-start gap-2 text-sm mb-1">
                      <component :is="!errors.name ? CheckCircle2 : X" :size="16" class="shrink-0 mt-0.5" :class="!errors.name ? 'text-emerald-500' : 'text-rose-500'" />
                      <span class="font-semibold" :class="!errors.name ? 'text-slate-700' : 'text-rose-600'">Tên cơ sở</span>
                    </div>
                    <p v-if="errors.name" class="text-[11px] text-rose-500 pl-6 leading-tight">{{ errors.name }}</p>
                  </div>
                  
                  <div>
                    <div class="flex items-start gap-2 text-sm mb-1">
                      <component :is="!errors.parent ? CheckCircle2 : X" :size="16" class="shrink-0 mt-0.5" :class="!errors.parent ? 'text-emerald-500' : 'text-rose-500'" />
                      <span class="font-semibold" :class="!errors.parent ? 'text-slate-700' : 'text-rose-600'">Cấu trúc cây (Parent check)</span>
                    </div>
                    <p v-if="errors.parent" class="text-[11px] text-rose-500 pl-6 leading-tight">{{ errors.parent }}</p>
                  </div>
                </div>

                <div class="p-4 bg-blue-50/50 border-t border-blue-100">
                  <h4 class="font-bold text-blue-800 text-[11px] mb-2 flex items-center gap-1"><Info :size="12" /> Lưu ý hệ thống</h4>
                  <ul class="text-[10px] text-blue-900/80 space-y-1.5 list-disc pl-3">
                    <li>Tránh vòng lặp: Một Sub-campus không thể nằm dưới một Sub-campus khác.</li>
                    <li>Mọi thay đổi từ giao diện này được ghi vào Audit Log tự động.</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- ================= 2. ORG CHART FAMILY TREE VIEW MODE ================= -->
    <div v-if="activeViewMode === 'chart'" class="org-family-tree-view space-y-6">
      <div class="lg-glass-soft rounded-2xl border border-default p-6 relative overflow-hidden">
        <!-- Chart Controls Header & Zoom Toolbar -->
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6 border-b border-default pb-4">
          <div>
            <h3 class="text-lg font-extrabold text-heading flex items-center gap-2">
              <GitBranch class="h-5 w-5 text-purple-600 dark:text-purple-400" />
              Sơ đồ Phân cấp Cơ sở Dạng Gia phả (Org Chart)
            </h3>
            <p class="text-xs text-muted mt-0.5 flex items-center gap-1.5">
              <MousePointer class="h-3.5 w-3.5 text-blue-500 shrink-0" />
              <span>Mẹo: <strong>Giữ Chuột phải + Lăn con trỏ chuột</strong> (hoặc Ctrl + Lăn chuột) để Phóng to / Thu nhỏ.</span>
            </p>
          </div>

          <!-- Zoom Toolbar Controls -->
          <div class="flex items-center gap-2">
            <div class="flex items-center gap-1 rounded-xl border border-default surface-card p-1 shadow-sm text-xs font-bold">
              <button
                type="button"
                @click="zoomOut"
                class="p-1.5 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 text-heading transition-colors"
                title="Thu nhỏ (-15%)"
              >
                <ZoomOut class="h-4 w-4" />
              </button>

              <span class="px-2 font-mono text-heading min-w-[50px] text-center select-none">
                {{ Math.round(zoomScale * 100) }}%
              </span>

              <button
                type="button"
                @click="zoomIn"
                class="p-1.5 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 text-heading transition-colors"
                title="Phóng to (+15%)"
              >
                <ZoomIn class="h-4 w-4" />
              </button>

              <button
                type="button"
                @click="resetZoom"
                class="p-1.5 rounded-lg hover:bg-slate-200 dark:hover:bg-slate-700 text-muted hover:text-heading transition-colors ml-1 border-l border-default pl-2"
                title="Đặt lại zoom (100%)"
              >
                <RotateCcw class="h-3.5 w-3.5" />
              </button>
            </div>

            <div v-if="selectedOrg" class="flex items-center gap-2 ml-2 pl-2 border-l border-default">
              <button @click="startEdit" class="glass-btn secondary text-xs"><Edit2 :size="14" /> Cập nhật</button>
            </div>
          </div>
        </div>

        <!-- Org Chart Tree Canvas Container with Zoom Scale -->
        <div
          class="org-chart-canvas-wrapper py-8 px-4 flex justify-center min-w-full overflow-auto relative cursor-grab active:cursor-grabbing select-none min-h-[420px]"
          @mousedown="handleCanvasMouseDown"
          @mouseup="handleCanvasMouseUp"
          @mouseleave="isRightMouseDown = false"
          @contextmenu="handleCanvasContextMenu"
          @wheel="handleCanvasWheel"
        >
          <div
            class="org-chart-canvas transition-transform duration-100 ease-out origin-top flex justify-center"
            :style="{ transform: `scale(${zoomScale})` }"
          >
            <div v-for="rootNode in organizationTree" :key="rootNode.id" class="inline-block">
              <OrgChartNode
                :node="rootNode"
                :selected-id="selectedOrg?.id"
                :level="0"
                @select-org="selectOrg"
              />
            </div>
          </div>
        </div>
      </div>


      <!-- Selected Node Quick Info & Actions Bar -->
      <div v-if="selectedOrg" class="lg-glass-soft rounded-2xl border border-default p-5 transition-all">
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div class="flex items-center gap-3.5">
            <div
              class="w-12 h-12 rounded-xl flex items-center justify-center text-white shadow-md font-bold shrink-0"
              :class="selectedOrg.type === 'Root' ? 'bg-purple-600' : (selectedOrg.type === 'Campus' ? 'bg-blue-600' : 'bg-teal-600')"
            >
              <component :is="getTypeIcon(selectedOrg.type)" class="h-6 w-6" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h3 class="text-base font-extrabold text-heading">{{ selectedOrg.name }}</h3>
                <span class="px-2.5 py-0.5 rounded-full text-[10px] font-extrabold" :class="getStatusBadge(selectedOrg.status).class">
                  {{ getStatusBadge(selectedOrg.status).label }}
                </span>
              </div>
              <p class="text-xs text-muted font-mono mt-0.5">
                Mã: <strong class="text-heading">{{ selectedOrg.code }}</strong> · {{ selectedOrg.address }}
              </p>
            </div>
          </div>

          <div class="flex items-center gap-4">
            <div class="grid grid-cols-2 gap-4 text-xs border-r border-default pr-4">
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Người dùng</span>
                <strong class="text-heading text-sm">{{ selectedOrg.metrics?.users || 0 }}</strong>
              </div>
              <div>
                <span class="block text-[10px] text-muted font-bold uppercase">Lớp học</span>
                <strong class="text-heading text-sm">{{ selectedOrg.metrics?.classes || 0 }}</strong>
              </div>
            </div>

            <div class="flex items-center gap-2">
              <button @click="startEdit" class="glass-btn secondary text-xs"><Edit2 :size="14" /> Cập nhật</button>
              <button @click="openLockModal" class="glass-btn text-xs" :class="selectedOrg.status === 'Locked' ? 'primary' : 'danger'">
                <component :is="selectedOrg.status === 'Locked' ? Unlock : Lock" :size="14" />
                {{ selectedOrg.status === 'Locked' ? 'Mở khóa' : 'Khóa cơ sở' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-if="isLockModalOpen" class="modal-overlay">
      <div class="modal-content glass-panel p-6 rounded-2xl max-w-md w-full">
        <div class="flex items-center gap-3 mb-4">
          <div class="flex items-center justify-center w-10 h-10 rounded-full" :class="selectedOrg?.status === 'Locked' ? 'bg-emerald-100 text-emerald-600' : 'bg-rose-100 text-rose-600'">
            <component :is="selectedOrg?.status === 'Locked' ? Unlock : Lock" :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-heading">{{ selectedOrg?.status === 'Locked' ? 'Mở khóa Cơ sở' : 'Khóa hoạt động Cơ sở' }}</h3>
            <p class="text-xs text-label">{{ selectedOrg?.name }}</p>
          </div>
        </div>

        <template v-if="selectedOrg?.status !== 'Locked'">
          <div class="mb-4">
            <p class="text-sm text-body mb-3">Khi khóa cơ sở, toàn bộ chức năng tạo mới (Người dùng, Lớp học, Lịch thi) thuộc phạm vi cơ sở này sẽ bị chặn. Dữ liệu cũ vẫn được giữ nguyên (Không xóa vật lý).</p>
            <div class="form-group">
              <label class="block text-xs font-bold text-rose-600 mb-1">Lý do khóa (Bắt buộc ghi Log) *</label>
              <textarea v-model="lockReason" rows="3" class="glass-input w-full" placeholder="Nhập lý do chi tiết để lưu Audit Log..."></textarea>
            </div>
          </div>
        </template>
        <template v-else>
          <p class="text-sm text-body mb-6">Xác nhận mở khóa hoạt động cho cơ sở này. Hệ thống sẽ cho phép tạo dữ liệu nghiệp vụ trở lại.</p>
        </template>

        <div class="flex gap-3 justify-end mt-2">
          <button @click="isLockModalOpen = false" class="glass-btn secondary">Hủy</button>
          <button @click="confirmLockAction" class="glass-btn" :class="selectedOrg?.status === 'Locked' ? 'primary' : 'danger'">
            {{ selectedOrg?.status === 'Locked' ? 'Xác nhận Mở Khóa' : 'Xác nhận Khóa (Ghi Log)' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Gán Quyền Admin -->
    <div v-if="isAssignRoleModalOpen" class="modal-overlay">
      <div class="modal-content glass-panel p-6 rounded-2xl max-w-lg w-full">
        <h3 class="text-lg font-bold text-heading mb-2 flex items-center gap-2"><UserCog :size="20" class="text-blue-600"/> Phân quyền Admin Cơ sở</h3>
        <p class="text-sm text-body mb-5">Chỉ định quyền quản trị chuyên biệt cho một tài khoản tại cơ sở: <strong class="text-heading">{{ selectedOrg?.name }}</strong></p>
        
        <div class="form-group mb-5">
          <label class="block text-xs font-bold text-label mb-1">Chọn tài khoản Admin từ hệ thống</label>
          <div class="relative">
            <select 
              v-model="roleForm.name" 
              class="glass-select w-full bg-white text-xs" 
              style="padding-left: 2.5rem;"
            >
              <option value="" disabled>-- Chọn tài khoản Admin --</option>
              <option v-for="admin in systemAdmins" :key="admin.email" :value="admin.name">
                {{ admin.name }} ({{ admin.email }})
              </option>
            </select>
            <UserCheck 
              :size="16" 
              class="absolute left-3 text-placeholder pointer-events-none" 
              style="top: 50%; transform: translateY(-50%);" 
            />
          </div>
        </div>
        
        <div class="form-group mb-6">
          <label class="block text-xs font-bold text-label mb-2">Phạm vi Quyền hạn (Scope Level)</label>
          <div class="grid grid-cols-2 gap-3">
            <!-- Campus Admin Option -->
            <label class="flex flex-col gap-2 p-3 border-2 rounded-xl cursor-pointer transition-all" :class="roleForm.role === 'Campus Admin' ? 'border-blue-500 bg-blue-50' : 'border-slate-200 bg-white hover:border-blue-200'">
              <div class="flex items-center gap-2">
                <input type="radio" v-model="roleForm.role" value="Campus Admin" name="admin_role" class="hidden" />
                <div class="w-4 h-4 rounded-full border-2 flex items-center justify-center" :class="roleForm.role === 'Campus Admin' ? 'border-blue-600' : 'border-slate-300'">
                  <div v-if="roleForm.role === 'Campus Admin'" class="w-2 h-2 bg-blue-600 rounded-full"></div>
                </div>
                <span class="font-bold text-sm text-heading">Campus Admin</span>
              </div>
              <p class="text-[11px] text-slate-500 leading-tight">Toàn quyền quản lý cơ sở này VÀ tất cả các Sub-campus trực thuộc nó.</p>
            </label>

            <!-- Sub-Campus Admin Option -->
            <label class="flex flex-col gap-2 p-3 border-2 rounded-xl cursor-pointer transition-all" :class="roleForm.role === 'Sub-Campus Admin' ? 'border-teal-500 bg-teal-50' : 'border-slate-200 bg-white hover:border-teal-200'">
              <div class="flex items-center gap-2">
                <input type="radio" v-model="roleForm.role" value="Sub-Campus Admin" name="admin_role" class="hidden" />
                <div class="w-4 h-4 rounded-full border-2 flex items-center justify-center" :class="roleForm.role === 'Sub-Campus Admin' ? 'border-teal-600' : 'border-slate-300'">
                  <div v-if="roleForm.role === 'Sub-Campus Admin'" class="w-2 h-2 bg-teal-600 rounded-full"></div>
                </div>
                <span class="font-bold text-sm text-heading">Sub-Campus Admin</span>
              </div>
              <p class="text-[11px] text-slate-500 leading-tight">Giới hạn quyền quản lý: CHỈ được thao tác dữ liệu thuộc chính cơ sở này.</p>
            </label>
          </div>
        </div>

        <div class="flex gap-3 pt-4 border-t border-slate-100">
          <button @click="isAssignRoleModalOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy thao tác</button>
          <button @click="confirmAssignRole" class="glass-btn primary flex-1 justify-center" :disabled="!roleForm.name" :class="{'opacity-50 cursor-not-allowed': !roleForm.name}">Gán quyền (Ghi Log)</button>
        </div>
      </div>
    </div>

    <!-- Modal Xác Nhận Hủy -->
    <div v-if="isCancelModalOpen" class="modal-overlay">
      <div class="modal-content glass-panel p-6 rounded-2xl max-w-sm w-full">
        <div class="flex items-center justify-center w-12 h-12 rounded-full bg-rose-100 text-rose-600 mb-4 mx-auto">
          <AlertTriangle :size="24" />
        </div>
        <h3 class="text-lg font-bold text-center text-heading mb-2">Xác nhận hủy</h3>
        <p class="text-sm text-center text-label mb-6">Bạn có chắc chắn muốn hủy? Các thay đổi chưa lưu sẽ bị mất.</p>
        <div class="flex gap-3">
          <button @click="isCancelModalOpen = false" class="glass-btn secondary flex-1 justify-center">Quay lại</button>
          <button @click="confirmCancel" class="glass-btn danger flex-1 justify-center">Đồng ý hủy</button>
        </div>
      </div>
    </div>
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
.glass-btn.primary { background: var(--text-link); color: white; }
.glass-btn.primary:hover { background: #1d4ed8; }
.glass-btn.secondary { background: var(--surface-input); border-color: var(--border-input); color: var(--text-heading); }
.glass-btn.secondary:hover { background: var(--surface-input-focus); }
.glass-btn.danger { background: var(--color-danger-bg); color: var(--color-danger-text); border-color: rgba(220, 38, 38, 0.2); }

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

/* Tree View */
.tree-container {
  overflow-x: auto;
}
.tree-list {
  list-style: none;
  padding: 0;
  margin: 0;
}
.child-list {
  padding-left: 1rem;
  border-left: 1px dashed var(--border-strong);
  margin-left: 0.75rem;
}
.tree-node {
  margin: 0.25rem 0;
}
.tree-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  color: var(--text-body);
}
.tree-item:hover {
  background: var(--surface-input);
}
.tree-item.active {
  background: var(--surface-input-focus);
  color: var(--text-link);
}
.expand-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border-radius: 4px;
  background: transparent;
  color: var(--text-placeholder);
  border: none;
  cursor: pointer;
}
.expand-btn:hover { background: var(--border-default); color: var(--text-heading); }
.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Metrics Cards */
.stat-card {
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  border-radius: 16px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
}
.stat-label {
  font-size: 0.7rem;
  font-weight: 700;
  color: var(--text-label);
  text-transform: uppercase;
}
.stat-value {
  font-size: 1.5rem;
  font-weight: 900;
  color: var(--text-heading);
  margin-top: 0.25rem;
}

/* Modal */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  backdrop-filter: blur(4px);
  z-index: 60;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
</style>
