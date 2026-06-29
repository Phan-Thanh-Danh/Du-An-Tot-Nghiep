<script setup>
import { ref, computed } from 'vue'
import {
  Settings2, Plus, ArrowRight, CheckCircle2, Clock, Shield, Zap, FileJson,
  MoreVertical, Layers, Trash2, Pen, X, Save, Loader2, AlertCircle,
  Search, Copy, ToggleLeft, ToggleRight, History, Bell, Eye, ArrowUp, ArrowDown, Edit2, GitBranch
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole(['SuperAdmin', 'Admin']))
const popupStore = usePopupStore()

const roleOptions = ['Giáo vụ khoa', 'Trưởng phòng Giáo vụ', 'Ban Giám hiệu', 'Phòng Đào tạo', 'Hệ thống']
const roleColors = {
  'Giáo vụ khoa': 'text-link',
  'Trưởng phòng Giáo vụ': 'text-body',
  'Ban Giám hiệu': 'text-(--lg-warning)',
  'Phòng Đào tạo': 'text-(--lg-info)',
  'Hệ thống': 'text-(--lg-success)'
}

let nextWfId = 5, nextStepId = 10, nextBranchId = 20

function makeDefaultSteps(n) {
  const names = ['Tiếp nhận & Kiểm tra', 'Phê duyệt chính', 'Thực thi tự động']
  const roles = ['Giáo vụ khoa', 'Trưởng phòng Giáo vụ', 'Hệ thống']
  const slas = ['24h', '48h', 'Tự động']
  return Array.from({ length: n }, (_, i) => ({
    id: `s${nextStepId++}`,
    name: names[i] || `Bước ${i + 1}`,
    role: roles[i] || roleOptions[0],
    sla: slas[i] || '24h',
    branches: i < n - 1
      ? [
          { id: `b${nextBranchId++}`, label: 'Hợp lệ', nextStepId: null, condition: 'Hồ sơ đầy đủ và hợp lệ' },
          { id: `b${nextBranchId++}`, label: 'Không hợp lệ', nextStepId: null, condition: 'Hồ sơ thiếu thông tin' },
        ]
      : []
  }))
}

const workflows = ref([
  {
    id: 'WF-01', name: 'Nghỉ học / Bảo lưu', sla: '72h', status: 'active',
    version: 'v2.1.0', steps: makeDefaultSteps(3)
  },
  {
    id: 'WF-02', name: 'Chuyển lớp học phần', sla: '48h', status: 'active',
    version: 'v1.3.0', steps: makeDefaultSteps(2)
  },
  {
    id: 'WF-03', name: 'Cấp giấy xác nhận', sla: '24h', status: 'active',
    version: 'v1.0.0', steps: makeDefaultSteps(1)
  },
  {
    id: 'WF-04', name: 'Thi lại / Hoãn thi', sla: '48h', status: 'draft',
    version: 'v0.5.0', steps: makeDefaultSteps(2)
  },
])

const activeWorkflow = ref(workflows.value[1])

// Fix default steps branches to reference correct step IDs
function initWorkflowStepRefs() {
  workflows.value.forEach(wf => {
    wf.steps.forEach((step, i) => {
      step.branches.forEach(b => {
        if (b.label === 'Hợp lệ' || b.label === 'Đồng ý') {
          const next = wf.steps[i + 1]
          b.nextStepId = next ? next.id : null
        }
      })
    })
  })
}
initWorkflowStepRefs()

// ── Workflow List Features ─────────────────────────────────
const searchQuery = ref('')
const filterStatus = ref('all')

const filteredWorkflows = computed(() => {
  let result = workflows.value
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(w => w.name.toLowerCase().includes(q))
  }
  if (filterStatus.value !== 'all') {
    result = result.filter(w => w.status === filterStatus.value)
  }
  return result
})

function duplicateWorkflow(wf) {
  const newWf = {
    ...JSON.parse(JSON.stringify(wf)),
    id: `WF-${String(nextWfId++).padStart(2, '0')}`,
    name: `${wf.name} (bản sao)`,
    status: 'draft',
    version: 'v1.0.0',
  }
  workflows.value.push(newWf)
  popupStore.success('Sao chép', `Đã sao chép workflow "${wf.name}".`)
}

function togglePublish(wf) {
  wf.status = wf.status === 'active' ? 'draft' : 'active'
  popupStore.success(wf.status === 'active' ? 'Xuất bản' : 'Gỡ xuất bản',
    `Workflow "${wf.name}" đã chuyển sang trạng thái ${wf.status === 'active' ? 'Đang áp dụng' : 'Bản nháp'}.`)
}

// ── Add Workflow ───────────────────────────────────────────
const showAddModal = ref(false)
const addForm = ref({ name: '', sla: '24h', steps: 2 })
const adding = ref(false)

function openAddModal() {
  addForm.value = { name: '', sla: '24h', steps: 2 }
  showAddModal.value = true
}

function submitAdd() {
  if (!addForm.value.name.trim() || addForm.value.steps < 1) return
  adding.value = true
  setTimeout(() => {
    workflows.value.unshift({
      id: `WF-${String(nextWfId++).padStart(2, '0')}`,
      name: addForm.value.name,
      sla: addForm.value.sla,
      status: 'draft',
      version: 'v1.0.0',
      steps: makeDefaultSteps(addForm.value.steps),
    })
    adding.value = false
    showAddModal.value = false
    popupStore.success('Thêm workflow', `Đã tạo workflow "${addForm.value.name}".`)
  }, 400)
}

function closeAddModal() { showAddModal.value = false }

// ── Edit Workflow ──────────────────────────────────────────
const showEditModal = ref(false)
const editForm = ref({ name: '', sla: '24h' })
const saving = ref(false)

function openEditModal() {
  editForm.value = { name: activeWorkflow.value.name, sla: activeWorkflow.value.sla }
  showEditModal.value = true
}

function submitEdit() {
  if (!editForm.value.name.trim()) return
  saving.value = true
  setTimeout(() => {
    Object.assign(activeWorkflow.value, editForm.value)
    saving.value = false
    showEditModal.value = false
    popupStore.success('Cập nhật', `Đã cập nhật "${activeWorkflow.value.name}".`)
  }, 400)
}

function closeEditModal() { showEditModal.value = false }

// ── Delete Workflow ────────────────────────────────────────
const showDeleteModal = ref(false)

function openDeleteModal() { showDeleteModal.value = true }

function confirmDelete() {
  const idx = workflows.value.findIndex(w => w.id === activeWorkflow.value.id)
  if (idx !== -1) {
    workflows.value.splice(idx, 1)
    activeWorkflow.value = workflows.value[0] || null
  }
  showDeleteModal.value = false
  popupStore.success('Xóa workflow', `Đã xóa workflow.`)
}

function closeDeleteModal() { showDeleteModal.value = false }

// ── Step CRUD ──────────────────────────────────────────────
const showStepModal = ref(false)
const stepForm = ref({ name: '', role: roleOptions[0], sla: '24h' })
const editingStepId = ref(null)
const savingStep = ref(false)

function openAddStep() {
  editingStepId.value = null
  stepForm.value = { name: '', role: roleOptions[0], sla: '24h' }
  showStepModal.value = true
}

function openEditStep(step) {
  editingStepId.value = step.id
  stepForm.value = { name: step.name, role: step.role, sla: step.sla }
  showStepModal.value = true
}

function submitStep() {
  if (!stepForm.value.name.trim()) return
  savingStep.value = true
  setTimeout(() => {
    const steps = activeWorkflow.value.steps
    if (editingStepId.value) {
      const st = steps.find(s => s.id === editingStepId.value)
      if (st) Object.assign(st, stepForm.value)
    } else {
      const newStep = {
        id: `s${nextStepId++}`,
        name: stepForm.value.name,
        role: stepForm.value.role,
        sla: stepForm.value.sla,
        branches: [
          { id: `b${nextBranchId++}`, label: 'Hợp lệ', nextStepId: null, condition: '' },
          { id: `b${nextBranchId++}`, label: 'Không hợp lệ', nextStepId: null, condition: '' },
        ]
      }
      steps.push(newStep)
    }
    savingStep.value = false
    showStepModal.value = false
    popupStore.success(editingStepId.value ? 'Cập nhật bước' : 'Thêm bước',
      `${editingStepId.value ? 'Đã cập nhật' : 'Đã thêm'} bước "${stepForm.value.name}".`)
  }, 300)
}

function closeStepModal() { showStepModal.value = false }

function deleteStep(stepId) {
  const steps = activeWorkflow.value.steps
  const idx = steps.findIndex(s => s.id === stepId)
  if (idx === -1) return
  const stepName = steps[idx].name
  if (steps.length <= 1) {
    popupStore.error('Lỗi', 'Workflow phải có ít nhất 1 bước.')
    return
  }
  steps.splice(idx, 1)
  steps.forEach(s => {
    s.branches.forEach(b => {
      if (b.nextStepId === stepId) b.nextStepId = null
    })
  })
  popupStore.success('Xóa bước', `Đã xóa bước "${stepName}".`)
}

function moveStep(stepId, dir) {
  const steps = activeWorkflow.value.steps
  const idx = steps.findIndex(s => s.id === stepId)
  if (idx === -1) return
  const newIdx = idx + dir
  if (newIdx < 0 || newIdx >= steps.length) return
  const stepsCopy = [...steps]
  const [removed] = stepsCopy.splice(idx, 1)
  stepsCopy.splice(newIdx, 0, removed)
  activeWorkflow.value.steps = stepsCopy
}

// ── Branch CRUD ────────────────────────────────────────────
const showBranchModal = ref(false)
const branchForm = ref({ label: '', condition: '', nextStepId: null })
const editingBranch = ref(null)
const branchParentStepId = ref(null)
const savingBranch = ref(false)

const branchLabelPresets = ['Hợp lệ', 'Không hợp lệ', 'Đồng ý', 'Yêu cầu sửa', 'Từ chối', 'Cần bổ sung', 'Chờ xét duyệt']

function openAddBranch(stepId) {
  branchParentStepId.value = stepId
  editingBranch.value = null
  branchForm.value = { label: '', condition: '', nextStepId: null }
  showBranchModal.value = true
}

function openEditBranch(stepId, branch) {
  branchParentStepId.value = stepId
  editingBranch.value = { stepId, branchId: branch.id }
  branchForm.value = { label: branch.label, condition: branch.condition || '', nextStepId: branch.nextStepId }
  showBranchModal.value = true
}

function submitBranch() {
  if (!branchForm.value.label.trim()) return
  savingBranch.value = true
  setTimeout(() => {
    const step = activeWorkflow.value.steps.find(s => s.id === branchParentStepId.value)
    if (!step) return
    if (editingBranch.value) {
      const b = step.branches.find(br => br.id === editingBranch.value.branchId)
      if (b) Object.assign(b, branchForm.value)
    } else {
      step.branches.push({
        id: `b${nextBranchId++}`,
        label: branchForm.value.label,
        condition: branchForm.value.condition,
        nextStepId: branchForm.value.nextStepId,
      })
    }
    savingBranch.value = false
    showBranchModal.value = false
    popupStore.success(editingBranch.value ? 'Cập nhật nhánh' : 'Thêm nhánh',
      `${editingBranch.value ? 'Đã cập nhật' : 'Đã thêm'} nhánh "${branchForm.value.label}".`)
  }, 300)
}

function closeBranchModal() { showBranchModal.value = false }

function deleteBranch(stepId, branchId) {
  const step = activeWorkflow.value.steps.find(s => s.id === stepId)
  if (!step) return
  const idx = step.branches.findIndex(b => b.id === branchId)
  if (idx === -1) return
  step.branches.splice(idx, 1)
  popupStore.success('Xóa nhánh', `Đã xóa nhánh.`)
}

function getBranchTargetLabel(nextStepId, steps) {
  if (!nextStepId) return 'Kết thúc - Từ chối'
  const target = steps.find(s => s.id === nextStepId)
  if (!target) return 'Kết thúc - Từ chối'
  const idx = steps.findIndex(s => s.id === nextStepId)
  return `Bước ${idx + 1}: ${target.name}`
}

function getBranchColor(label) {
  const l = label.toLowerCase()
  if (l.includes('hợp lệ') || l.includes('đồng ý') || l.includes('duyệt')) return 'success'
  if (l.includes('sửa') || l.includes('bổ sung') || l.includes('chờ')) return 'warning'
  return 'danger'
}

function getBranchIcon(label) {
  const l = label.toLowerCase()
  if (l.includes('hợp lệ') || l.includes('đồng ý') || l.includes('duyệt')) return '✓'
  if (l.includes('sửa') || l.includes('bổ sung') || l.includes('chờ')) return '↻'
  return '✗'
}

const branchColorClasses = {
  success: { bg: 'bg-(--color-success-bg)', text: 'text-(--color-success-text)', border: 'border-(--color-success-text)/30', dot: 'bg-(--lg-success)' },
  danger: { bg: 'bg-(--color-danger-bg)', text: 'text-(--lg-danger)', border: 'border-(--lg-danger)/30', dot: 'bg-(--lg-danger)' },
  warning: { bg: 'bg-(--color-warning-bg)', text: 'text-(--color-warning-text)', border: 'border-(--color-warning-text)/30', dot: 'bg-(--lg-warning)' },
}

// ── Version History ────────────────────────────────────────
const showVersionModal = ref(false)
const versionHistory = [
  { version: 'v2.1.0', date: '15/06/2026', by: 'Nguyễn Văn A', changes: 'Thêm nhánh "Yêu cầu sửa", cập nhật SLA bước 2' },
  { version: 'v2.0.0', date: '01/04/2026', by: 'Trần Thị B', changes: 'Tái cấu trúc workflow, thêm bước tự động' },
  { version: 'v1.0.0', date: '10/12/2025', by: 'Lê Văn C', changes: 'Phiên bản đầu tiên' },
]

function openVersionModal() { showVersionModal.value = true }
function closeVersionModal() { showVersionModal.value = false }

// ── Notification Config ────────────────────────────────────
const showNotifModal = ref(false)
const notifStepId = ref(null)
const notifForm = ref({ email: false, sms: false, emailTemplate: '', smsTemplate: '' })

function openNotifModal(stepId) {
  notifStepId.value = stepId
  const step = activeWorkflow.value.steps.find(s => s.id === stepId)
  notifForm.value = {
    email: step?.notif?.email ?? true,
    sms: step?.notif?.sms ?? false,
    emailTemplate: step?.notif?.emailTemplate ?? 'Đơn từ #{{ma_don}} đã đến bước "{{ten_buoc}}". Vui lòng xử lý.',
    smsTemplate: step?.notif?.smsTemplate ?? '',
  }
  showNotifModal.value = true
}

function submitNotif() {
  const step = activeWorkflow.value.steps.find(s => s.id === notifStepId.value)
  if (!step) return
  if (!step.notif) step.notif = {}
  Object.assign(step.notif, notifForm.value)
  showNotifModal.value = false
  popupStore.success('Cập nhật thông báo', `Đã lưu cấu hình thông báo cho bước "${step.name}".`)
}

function closeNotifModal() { showNotifModal.value = false }

// ── Context Menu ─────────────────────────────────────────
const showFloatingMenu = ref(false)
function toggleFloatingMenu() { showFloatingMenu.value = !showFloatingMenu.value }

const getStatusBadge = (s) => s === 'active' ? 'lg-badge-success' : 'surface-solid text-placeholder border-default'
const getStatusLabel = (s) => s === 'active' ? 'Đang áp dụng' : 'Bản nháp'

function getStepNumber(stepId) {
  const idx = activeWorkflow.value?.steps.findIndex(s => s.id === stepId)
  return idx !== -1 ? idx + 1 : '?'
}
</script>

<template>
  <PageContainer
    title="Cấu hình quy trình duyệt"
    subtitle="Danh sách các quy trình duyệt đơn từ trong hệ thống."
  >
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8" @click="showFloatingMenu = false">

      <!-- ═══ Left: Workflow List ═══ -->
      <div class="space-y-4">
        <div class="flex items-center justify-between mb-2">
          <h4 class="text-xs font-semibold text-label uppercase tracking-widest">Loại đơn & Workflow</h4>
          <button v-if="canEdit" class="text-[11px] font-semibold text-link flex items-center gap-1 hover:underline" @click="openAddModal">
            <Plus :size="12" /> Thêm mới
          </button>
        </div>

        <!-- Search -->
        <div class="relative">
          <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
          <input v-model="searchQuery" type="text" placeholder="Tìm workflow..."
            class="w-full lg-input pl-8 pr-3 py-2 text-xs font-medium" />
        </div>

        <!-- Filter tabs -->
        <div class="flex gap-1 p-1 surface-solid rounded-xl">
          <button v-for="opt in [{ label: 'Tất cả', val: 'all' }, { label: 'Đang áp dụng', val: 'active' }, { label: 'Bản nháp', val: 'draft' }]"
            :key="opt.val"
            @click="filterStatus = opt.val"
            :class="[
              'flex-1 px-2 py-1.5 text-[10px] font-bold rounded-lg transition-all',
              filterStatus === opt.val ? 'bg-(--surface-card) text-heading shadow-sm' : 'text-placeholder hover:text-label'
            ]"
          >{{ opt.label }}</button>
        </div>

        <div v-for="wf in filteredWorkflows" :key="wf.id"
          @click="activeWorkflow = wf"
          :class="[
            'p-4 rounded-3xl border transition-all cursor-pointer group',
            activeWorkflow?.id === wf.id ? 'lg-glass-strong border-default' : 'lg-card-glass hover:border-(--border-input-focus)'
          ]"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-heading truncate">{{ wf.name }}</p>
              <div class="flex items-center gap-3 mt-2">
                <span class="text-[10px] font-semibold text-label uppercase tracking-tighter flex items-center gap-1">
                  <Layers :size="12" /> {{ wf.steps.length }} bước
                </span>
                <span class="text-[10px] font-semibold text-label uppercase tracking-tighter flex items-center gap-1">
                  <Clock :size="12" /> SLA {{ wf.sla }}
                </span>
              </div>
            </div>
            <div class="flex items-center gap-1 shrink-0 ml-2">
              <button v-if="canEdit" @click.stop="duplicateWorkflow(wf)" class="p-1.5 rounded-lg opacity-0 group-hover:opacity-100 hover:bg-(--surface-input) text-placeholder hover:text-link transition-all" title="Sao chép">
                <Copy :size="13" />
              </button>
              <button v-if="canEdit" @click.stop="togglePublish(wf)" class="p-1.5 rounded-lg opacity-0 group-hover:opacity-100 hover:bg-(--surface-input) transition-all" :class="wf.status === 'active' ? 'text-(--lg-success)' : 'text-placeholder'" :title="wf.status === 'active' ? 'Gỡ xuất bản' : 'Xuất bản'">
                <ToggleRight v-if="wf.status === 'active'" :size="13" />
                <ToggleLeft v-else :size="13" />
              </button>
            </div>
          </div>
        </div>

        <div v-if="filteredWorkflows.length === 0" class="text-center py-10">
          <p class="text-xs text-placeholder">Không tìm thấy workflow nào.</p>
        </div>
      </div>

      <!-- ═══ Right: Workflow Builder ═══ -->
      <div class="lg:col-span-2 space-y-4">

        <!-- Header Info -->
        <div class="surface-card border border-card rounded-2xl p-5 flex items-center justify-between">
          <div class="flex items-center gap-5">
            <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body border-default">
              <Settings2 :size="28" />
            </div>
            <div>
              <div class="flex items-center gap-3">
                <h3 class="text-xl font-semibold text-heading">{{ activeWorkflow?.name }}</h3>
                <span v-if="activeWorkflow" :class="['px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest border rounded-full', getStatusBadge(activeWorkflow.status)]">{{ getStatusLabel(activeWorkflow.status) }}</span>
              </div>
              <p class="text-xs font-bold text-label mt-1 uppercase tracking-tighter flex items-center gap-3">
                <span>{{ activeWorkflow?.id }}</span>
                <button @click="openVersionModal" class="text-link hover:underline inline-flex items-center gap-1">
                  <History :size="12" /> {{ activeWorkflow?.version }}
                </button>
              </p>
            </div>
          </div>
          <div v-if="canEdit" class="flex items-center gap-2">
            <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2" @click="openEditModal">
              <Pen :size="16" /> Sửa
            </button>
            <button class="lg-button-secondary px-3 py-2.5 text-sm font-bold text-(--lg-danger) hover:bg-(--color-danger-bg)" @click="openDeleteModal" title="Xóa workflow">
              <Trash2 :size="16" />
            </button>
          </div>
        </div>

        <!-- Builder Visualizer -->
        <div class="surface-card border border-card rounded-2xl p-8 relative overflow-visible">
          <div class="flex flex-col items-center">
            <template v-for="(step, idx) in activeWorkflow?.steps || []" :key="step.id">
              <div class="relative flex items-start w-full max-w-3xl">
                <!-- Left: step number + line -->
                <div class="flex flex-col items-center mr-4">
                  <div :class="[
                    'h-9 w-9 rounded-full flex items-center justify-center text-xs font-bold shadow-sm shrink-0 border z-10',
                    step.branches.length === 0
                      ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/30'
                      : 'surface-solid text-heading border-default'
                  ]">
                    <CheckCircle2 v-if="step.branches.length === 0" :size="18" />
                    <span v-else>{{ idx + 1 }}</span>
                  </div>
                  <div v-if="idx < activeWorkflow.steps.length - 1" class="w-0.5 flex-1 min-h-[40px] bg-(--border-default) mt-2"></div>
                </div>

                <!-- Right: step card + branches -->
                <div class="flex-1 min-w-0">
                  <div class="p-4 rounded-2xl border transition-all surface-card border-card hover:border-(--border-input-focus) shadow-sm">
                    <div class="flex items-center justify-between mb-3">
                      <h5 class="text-sm font-semibold text-heading uppercase tracking-wide">{{ step.name }}</h5>
                      <div v-if="canEdit" class="flex items-center gap-1 shrink-0">
                        <button @click="moveStep(step.id, -1)" :disabled="idx === 0" class="p-1 rounded-lg hover:bg-(--surface-input) text-placeholder hover:text-heading disabled:opacity-30 transition-all" title="Lên trên">
                          <ArrowUp :size="14" />
                        </button>
                        <button @click="moveStep(step.id, 1)" :disabled="idx >= activeWorkflow.steps.length - 1" class="p-1 rounded-lg hover:bg-(--surface-input) text-placeholder hover:text-heading disabled:opacity-30 transition-all" title="Xuống dưới">
                          <ArrowDown :size="14" />
                        </button>
                        <button @click="openEditStep(step)" class="p-1 rounded-lg hover:bg-(--color-info-bg) text-placeholder hover:text-link transition-all" title="Sửa bước">
                          <Edit2 :size="14" />
                        </button>
                        <button @click="deleteStep(step.id)" class="p-1 rounded-lg hover:bg-(--color-danger-bg) text-placeholder hover:text-(--lg-danger) transition-all" title="Xóa bước">
                          <Trash2 :size="14" />
                        </button>
                      </div>
                    </div>
                    <div class="flex items-center justify-between">
                      <div class="flex items-center gap-2 text-[11px] font-bold text-label">
                        <Shield :size="14" :class="roleColors[step.role] || 'text-label'" />
                        <select v-if="canEdit" v-model="step.role" @change.stop
                          class="bg-transparent border-none text-[11px] font-bold text-label cursor-pointer focus:outline-none">
                          <option v-for="r in roleOptions" :key="r" :value="r">{{ r }}</option>
                        </select>
                        <span v-else>{{ step.role }}</span>
                      </div>
                      <div class="flex items-center gap-2">
                        <button @click="openEditStep(step)" class="text-[10px] font-semibold text-(--color-warning-text) uppercase tracking-widest bg-(--color-warning-bg) px-2 py-1 rounded-lg hover:opacity-80 transition-all">
                          SLA: {{ step.sla }}
                        </button>
                        <button @click="openNotifModal(step.id)" class="p-1.5 rounded-lg hover:bg-(--surface-input) text-placeholder hover:text-(--color-info-text) transition-all" title="Cấu hình thông báo">
                          <Bell :size="13" />
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Branches -->
                  <div v-if="step.branches.length" class="mt-3 ml-2">
                    <div class="flex items-center justify-between mb-2">
                      <span class="text-[9px] font-bold text-placeholder uppercase tracking-widest flex items-center gap-1">
                        <GitBranch :size="11" /> Nhánh rẽ
                      </span>
                      <button v-if="canEdit" @click="openAddBranch(step.id)" class="text-[10px] font-semibold text-link hover:underline flex items-center gap-0.5">
                        <Plus :size="10" /> Thêm nhánh
                      </button>
                    </div>
                    <div class="space-y-1.5">
                      <div v-for="branch in step.branches" :key="branch.id" class="flex items-center gap-2 group/branch">
                        <div class="flex items-center shrink-0">
                          <div :class="['h-0.5 w-5', branchColorClasses[getBranchColor(branch.label)].bg]"></div>
                          <div :class="['h-2 w-2 rounded-full', branchColorClasses[getBranchColor(branch.label)].dot]"></div>
                        </div>
                        <div v-if="canEdit" class="flex items-center gap-1 text-[10px]">
                          <input v-model="branch.label" @change.stop
                            class="w-24 px-1.5 py-0.5 rounded border border-default bg-transparent text-[10px] font-bold focus:outline-none focus:border-(--border-input-focus)"
                            :class="branchColorClasses[getBranchColor(branch.label)].text" />
                        </div>
                        <div v-else :class="[
                          'inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold border',
                          branchColorClasses[getBranchColor(branch.label)].bg, branchColorClasses[getBranchColor(branch.label)].text, branchColorClasses[getBranchColor(branch.label)].border
                        ]">
                          <span>{{ getBranchIcon(branch.label) }}</span>
                          <span>{{ branch.label }}</span>
                        </div>
                        <div v-if="branch.condition" class="text-[9px] text-placeholder italic truncate max-w-[120px]" :title="branch.condition">
                          "{{ branch.condition }}"
                        </div>
                        <div class="flex items-center gap-1">
                          <ArrowRight :size="10" :class="branchColorClasses[getBranchColor(branch.label)].text" />
                          <span :class="['text-[9px] font-semibold', branchColorClasses[getBranchColor(branch.label)].text]">
                            {{ getBranchTargetLabel(branch.nextStepId, activeWorkflow.steps) }}
                          </span>
                        </div>
                        <div v-if="canEdit" class="flex items-center gap-0.5 opacity-0 group-hover/branch:opacity-100 transition-all">
                          <button @click="openEditBranch(step.id, branch)" class="p-0.5 rounded hover:bg-(--surface-input) text-placeholder hover:text-link" title="Sửa nhánh">
                            <Edit2 :size="11" />
                          </button>
                          <button @click="deleteBranch(step.id, branch.id)" class="p-0.5 rounded hover:bg-(--color-danger-bg) text-placeholder hover:text-(--lg-danger)" title="Xóa nhánh">
                            <X :size="11" />
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Auto-exec summary for end step -->
                  <div v-if="step.branches.length === 0" class="mt-3 ml-2 flex items-center gap-3">
                    <div class="flex items-center gap-2 text-[10px] font-bold text-(--color-success-text)">
                      <FileJson :size="14" />
                      <span>Thực thi tự động</span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Connector -->
              <div v-if="idx < activeWorkflow.steps.length - 1" class="relative flex items-center justify-center h-8 my-1">
                <div class="h-full w-0.5 bg-(--border-default) absolute left-[26px]"></div>
              </div>
            </template>
          </div>

          <!-- Add Step Button -->
          <div v-if="canEdit" class="flex justify-center mt-6">
            <button @click="openAddStep" class="inline-flex items-center gap-1.5 text-xs font-bold text-link hover:underline">
              <Plus :size="14" /> Thêm bước mới
            </button>
          </div>

          <!-- Floating Tools -->
          <div class="absolute bottom-6 right-6 flex flex-col gap-3">
            <div class="relative">
              <button v-if="canEdit" class="h-10 w-10 lg-card-glass rounded-2xl border-default text-placeholder hover:text-link shadow-sm flex items-center justify-center transition-all" @click="openAddModal" title="Thêm workflow mới">
                <Plus :size="24" />
              </button>
            </div>
            <div class="relative" @click.stop>
              <button class="h-10 w-10 surface-solid rounded-2xl text-heading shadow-sm flex items-center justify-center hover:bg-(--surface-input) transition-all" @click="toggleFloatingMenu">
                <MoreVertical :size="24" />
              </button>
              <Transition
                enter-active-class="transition-all duration-150 ease-out"
                enter-from-class="opacity-0 scale-95"
                enter-to-class="opacity-100 scale-100"
                leave-active-class="transition-all duration-100 ease-in"
                leave-from-class="opacity-100 scale-100"
                leave-to-class="opacity-0 scale-95"
              >
                <div v-if="showFloatingMenu" class="absolute right-0 bottom-full mb-2 z-50 w-44 lg-glass-strong rounded-xl p-1 shadow-sm" @click.stop>
                  <button v-if="canEdit" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-info-bg) hover:text-link transition-all" @click="openEditModal(); showFloatingMenu = false">
                    <Pen :size="14" /> Chỉnh sửa
                  </button>
                  <button v-if="canEdit" class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-(--color-danger-bg) hover:text-(--lg-danger) transition-all" @click="openDeleteModal(); showFloatingMenu = false">
                    <Trash2 :size="14" /> Xóa
                  </button>
                </div>
              </Transition>
            </div>
          </div>
        </div>

        <!-- Workflow Rules Summary -->
        <div class="surface-card border border-card rounded-2xl p-4">
          <div class="flex items-start gap-4">
            <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body shrink-0">
              <Shield :size="20" />
            </div>
            <div class="flex-1">
              <h4 class="text-sm font-semibold text-heading uppercase tracking-wide">Quy tắc bảo mật Workflow</h4>
              <p class="text-xs text-body mt-1 leading-relaxed">
                Các workflow đã phát sinh đơn từ thực tế sẽ <strong>không được phép xóa</strong>. Bạn chỉ có thể đóng (archive) hoặc tạo phiên bản mới để thay đổi cấu trình mà không ảnh hưởng đến dữ liệu lịch sử.
              </p>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- ═══ ADD WORKFLOW MODAL ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showAddModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeAddModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><Plus :size="18" /></div>
              <h3 class="text-base font-semibold text-heading">Thêm workflow mới</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeAddModal"><X :size="18" /></button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tên workflow</label>
              <input v-model="addForm.name" type="text" placeholder="VD: Xử lý đơn nghỉ học" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">SLA tổng</label>
              <select v-model="addForm.sla" class="w-full lg-input px-4 py-2.5 text-sm">
                <option value="24h">24h</option><option value="48h">48h</option><option value="72h">72h</option><option value="1 tuần">1 tuần</option>
              </select>
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Số bước khởi tạo</label>
              <input v-model.number="addForm.steps" type="number" min="1" max="10" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeAddModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="submitAdd" :disabled="!addForm.name.trim() || adding" :class="{ 'opacity-50 cursor-not-allowed': !addForm.name.trim() || adding }">
              <Loader2 v-if="adding" :size="16" class="animate-spin" />
              <Save v-else :size="16" /> Tạo mới
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ EDIT WORKFLOW MODAL ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showEditModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeEditModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><Pen :size="18" /></div>
              <h3 class="text-base font-semibold text-heading">Chỉnh sửa workflow</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeEditModal"><X :size="18" /></button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tên workflow</label>
              <input v-model="editForm.name" type="text" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">SLA tổng</label>
              <select v-model="editForm.sla" class="w-full lg-input px-4 py-2.5 text-sm">
                <option value="24h">24h</option><option value="48h">48h</option><option value="72h">72h</option><option value="1 tuần">1 tuần</option>
              </select>
            </div>
            <div class="p-3 surface-solid rounded-2xl">
              <p class="text-[10px] font-bold text-placeholder">{{ activeWorkflow?.steps.length }} bước xử lý</p>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeEditModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="submitEdit" :disabled="!editForm.name.trim() || saving" :class="{ 'opacity-50 cursor-not-allowed': !editForm.name.trim() || saving }">
              <Loader2 v-if="saving" :size="16" class="animate-spin" />
              <Save v-else :size="16" /> Lưu thay đổi
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ DELETE WORKFLOW MODAL ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showDeleteModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDeleteModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-danger-bg) text-(--lg-danger) flex items-center justify-center"><AlertCircle :size="18" /></div>
              <h3 class="text-base font-semibold text-heading">Xóa workflow</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeDeleteModal"><X :size="18" /></button>
          </div>
          <div class="bg-(--color-danger-bg) border border-(--lg-danger)/30 rounded-2xl p-4 mb-4">
            <p class="text-[13px] font-bold text-(--lg-danger)">Bạn có chắc chắn muốn xóa workflow <strong>{{ activeWorkflow?.name }}</strong>?</p>
            <p class="text-[11px] font-medium text-(--lg-danger)/70 mt-2">Thao tác này không thể hoàn tác.</p>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDeleteModal">Quay lại</button>
            <button class="px-5 py-2.5 text-sm font-bold text-white bg-(--lg-danger) hover:opacity-90 rounded-2xl transition-all flex items-center gap-2" @click="confirmDelete"><Trash2 :size="16" /> Xác nhận xóa</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ STEP MODAL (ADD/EDIT) ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showStepModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeStepModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><Layers :size="18" /></div>
              <h3 class="text-base font-semibold text-heading">{{ editingStepId ? 'Sửa bước' : 'Thêm bước mới' }}</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeStepModal"><X :size="18" /></button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tên bước</label>
              <input v-model="stepForm.name" type="text" placeholder="VD: Kiểm tra hồ sơ" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Vai trò xử lý</label>
              <select v-model="stepForm.role" class="w-full lg-input px-4 py-2.5 text-sm">
                <option v-for="r in roleOptions" :key="r" :value="r">{{ r }}</option>
              </select>
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">SLA (thời hạn xử lý)</label>
              <select v-model="stepForm.sla" class="w-full lg-input px-4 py-2.5 text-sm">
                <option value="Tự động">Tự động</option>
                <option value="24h">24h</option>
                <option value="48h">48h</option>
                <option value="72h">72h</option>
                <option value="1 tuần">1 tuần</option>
              </select>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeStepModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="submitStep" :disabled="!stepForm.name.trim() || savingStep" :class="{ 'opacity-50 cursor-not-allowed': !stepForm.name.trim() || savingStep }">
              <Loader2 v-if="savingStep" :size="16" class="animate-spin" />
              <Save v-else :size="16" /> {{ editingStepId ? 'Cập nhật' : 'Thêm' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ BRANCH MODAL (ADD/EDIT) ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showBranchModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeBranchModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><GitBranch :size="18" /></div>
              <h3 class="text-base font-semibold text-heading">{{ editingBranch ? 'Sửa nhánh rẽ' : 'Thêm nhánh rẽ' }}</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeBranchModal"><X :size="18" /></button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Nhãn nhánh</label>
              <input v-model="branchForm.label" type="text" placeholder="VD: Hợp lệ" list="branch-presets"
                class="w-full lg-input px-4 py-2.5 text-sm" />
              <datalist id="branch-presets">
                <option v-for="p in branchLabelPresets" :key="p" :value="p" />
              </datalist>
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Điều kiện (condition rule)</label>
              <input v-model="branchForm.condition" type="text" placeholder="VD: Hồ sơ đầy đủ và hợp lệ"
                class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Chuyển đến bước</label>
              <select v-model="branchForm.nextStepId" class="w-full lg-input px-4 py-2.5 text-sm">
                <option :value="null">Kết thúc - Từ chối</option>
                <option v-for="(s, i) in activeWorkflow?.steps || []" :key="s.id" :value="s.id">
                  Bước {{ i + 1 }}: {{ s.name }}
                </option>
              </select>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeBranchModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="submitBranch" :disabled="!branchForm.label.trim() || savingBranch" :class="{ 'opacity-50 cursor-not-allowed': !branchForm.label.trim() || savingBranch }">
              <Loader2 v-if="savingBranch" :size="16" class="animate-spin" />
              <Save v-else :size="16" /> {{ editingBranch ? 'Cập nhật' : 'Thêm' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ VERSION HISTORY MODAL ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showVersionModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeVersionModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-lg surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><History :size="18" /></div>
              <div>
                <h3 class="text-base font-semibold text-heading">Lịch sử phiên bản</h3>
                <p class="text-[10px] text-label">{{ activeWorkflow?.name }}</p>
              </div>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeVersionModal"><X :size="18" /></button>
          </div>
          <div class="space-y-3 max-h-[300px] overflow-y-auto">
            <div v-for="(v, i) in versionHistory" :key="v.version"
              :class="['flex gap-4 p-4 rounded-2xl border', i === 0 ? 'border-(--border-input-focus) bg-(--surface-input)' : 'border-default']">
              <div class="h-9 w-9 rounded-xl surface-solid flex items-center justify-center text-[10px] font-bold text-heading shrink-0 border border-default">
                {{ v.version }}
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center justify-between">
                  <p class="text-sm font-semibold text-heading">{{ v.version }}</p>
                  <span class="text-[10px] text-placeholder">{{ v.date }}</span>
                </div>
                <p class="text-[11px] text-label mt-0.5">Bởi <strong>{{ v.by }}</strong></p>
                <p class="text-[11px] text-body mt-2 leading-relaxed">{{ v.changes }}</p>
              </div>
            </div>
          </div>
          <div class="flex items-center justify-end mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeVersionModal">Đóng</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ═══ NOTIFICATION CONFIG MODAL ═══ -->
    <Transition enter-active-class="transition-all duration-200 ease-out" enter-from-class="opacity-0" enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in" leave-from-class="opacity-100" leave-to-class="opacity-0">
      <div v-if="showNotifModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeNotifModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-lg surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-(--color-info-bg) text-link flex items-center justify-center"><Bell :size="18" /></div>
              <div>
                <h3 class="text-base font-semibold text-heading">Cấu hình thông báo</h3>
                <p class="text-[10px] text-label">Bước {{ getStepNumber(notifStepId) }}: {{ activeWorkflow?.steps.find(s => s.id === notifStepId)?.name }}</p>
              </div>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-(--surface-solid) text-placeholder transition-all" @click="closeNotifModal"><X :size="18" /></button>
          </div>
          <div class="space-y-5">
            <!-- Email -->
            <div class="p-4 rounded-2xl surface-solid border border-default">
              <label class="flex items-center justify-between cursor-pointer">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-link">
                    <Bell :size="16" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading">Email</p>
                    <p class="text-[10px] text-placeholder">Gửi email cho người xử lý khi đến bước này</p>
                  </div>
                </div>
                <div @click.stop>
                  <label class="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" v-model="notifForm.email" class="sr-only peer" />
                    <div class="w-9 h-5 rounded-full peer peer-checked:bg-(--lg-primary) bg-(--surface-input) border border-default after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:after:translate-x-full"></div>
                  </label>
                </div>
              </label>
              <div v-if="notifForm.email" class="mt-3">
                <textarea v-model="notifForm.emailTemplate" rows="3"
                  class="w-full lg-input px-3 py-2 text-[11px] font-mono"
                  placeholder="Template email..."></textarea>
              </div>
            </div>

            <!-- SMS -->
            <div class="p-4 rounded-2xl surface-solid border border-default">
              <label class="flex items-center justify-between cursor-pointer">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text)">
                    <Bell :size="16" />
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-heading">SMS</p>
                    <p class="text-[10px] text-placeholder">Gửi SMS cho người xử lý</p>
                  </div>
                </div>
                <div @click.stop>
                  <label class="relative inline-flex items-center cursor-pointer">
                    <input type="checkbox" v-model="notifForm.sms" class="sr-only peer" />
                    <div class="w-9 h-5 rounded-full peer peer-checked:bg-(--lg-primary) bg-(--surface-input) border border-default after:content-[''] after:absolute after:top-0.5 after:left-0.5 after:bg-white after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:after:translate-x-full"></div>
                  </label>
                </div>
              </label>
              <div v-if="notifForm.sms" class="mt-3">
                <textarea v-model="notifForm.smsTemplate" rows="2"
                  class="w-full lg-input px-3 py-2 text-[11px] font-mono"
                  placeholder="Template SMS..."></textarea>
              </div>
            </div>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeNotifModal">Hủy</button>
            <button class="lg-button-primary px-5 py-2.5 text-sm font-bold" @click="submitNotif">
              <Save :size="16" /> Lưu cấu hình
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
