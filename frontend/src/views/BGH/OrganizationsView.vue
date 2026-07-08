<template>
  <div class="h-[calc(100vh-8rem)] flex gap-6">
    <!-- Left Panel: Tree View -->
    <div class="w-1/3 min-w-[300px] flex flex-col surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div class="p-4 flex items-center justify-between bg-(--surface-card)">
        <div>
          <h2 class="text-lg font-bold text-heading">Cơ Cấu Tổ Chức</h2>
          <p class="text-xs text-muted mt-0.5">Danh sách các khoa, phòng ban</p>
        </div>
        <button
          v-if="canEdit"
          @click="handleCreateRoot"
          class="flex items-center gap-1.5 px-3 py-1.5 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors"
        >
          <Plus :size="14" />
          <span>Thêm gốc</span>
        </button>
      </div>

      <div class="flex-1 overflow-y-auto p-4 space-y-1">
        <div v-if="loadingTree" class="flex justify-center p-8">
          <Loader2 class="animate-spin text-muted" :size="24" />
        </div>
        <div v-else-if="errorTree" class="flex flex-col items-center gap-3 p-8 text-center">
          <AlertCircle :size="24" class="text-(--color-danger-text)" />
          <p class="text-sm text-(--color-danger-text) font-medium">{{ errorTree }}</p>
          <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
        </div>
        <div v-else-if="flattenedTree.length === 0" class="text-center text-sm text-muted p-8">
          Chưa có dữ liệu cơ cấu tổ chức.
        </div>
        <template v-else>
          <div
            v-for="node in flattenedTree"
            :key="node.id"
            v-show="isNodeVisible(node)"
            @click="selectNode(node)"
            class="group flex items-center justify-between px-2 py-2 rounded-lg cursor-pointer transition-colors"
            :class="[
              selectedNode?.id === node.id 
                ? 'bg-(--lg-primary)/10 text-(--lg-primary)' 
                : 'hover:bg-(--surface-input) text-body'
            ]"
            :style="{ paddingLeft: `${node._level * 1.5 + 0.5}rem` }"
          >
            <div class="flex items-center gap-2 overflow-hidden">
              <button 
                v-if="node.children && node.children.length > 0"
                @click.stop="toggleExpand(node.id)"
                class="w-5 h-5 flex items-center justify-center rounded hover:bg-black/5 dark:hover:surface-card/5 text-muted"
              >
                <ChevronDown v-if="expandedNodes.has(node.id)" :size="14" />
                <ChevronRight v-else :size="14" />
              </button>
              <div v-else class="w-5 h-5"></div>
              
              <div class="flex-1 min-w-0 flex items-center gap-2">
                <component :is="getIconForLevel(node.organizationLevel)" :size="16" class="shrink-0 text-muted" />
                <span class="text-sm font-semibold truncate">{{ node.name }}</span>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>

    <!-- Right Panel: Details/Form -->
    <div class="flex-1 flex flex-col surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div class="p-4 bg-(--surface-card)">
        <h2 class="text-lg font-bold text-heading">
          {{ formMode === 'view' ? 'Chi tiết đơn vị' : (formMode === 'create' ? 'Thêm đơn vị mới' : 'Chỉnh sửa đơn vị') }}
        </h2>
      </div>

      <div class="flex-1 overflow-y-auto p-6">
        <div v-if="!selectedNode && formMode === 'view'" class="h-full flex flex-col items-center justify-center text-muted">
          <Network :size="48" stroke-width="1" class="mb-4 opacity-50" />
          <p class="text-sm">Chọn một đơn vị bên trái để xem chi tiết</p>
        </div>

        <form v-else @submit.prevent="saveOrganization" class="max-w-xl space-y-5">
          <!-- Parent Organization -->
          <div class="space-y-1.5">
            <label class="block text-xs font-bold text-heading">Đơn vị trực thuộc (Cấp trên)</label>
            <div v-if="formMode === 'view'" class="text-sm px-3 py-2 bg-(--surface-input) rounded-lg text-body">
              {{ getParentName(formData.parentId) || 'Không có (Cấp cao nhất)' }}
            </div>
            <select 
              v-else 
              v-model="formData.parentId"
              class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)"
            >
              <option :value="null">-- Không có (Cấp cao nhất) --</option>
              <option v-for="org in flatOrganizationsList" :key="org.id" :value="org.id" :disabled="org.id === formData.id">
                {{ org.name }}
              </option>
            </select>
          </div>

          <!-- Name -->
          <div class="space-y-1.5">
            <label class="block text-xs font-bold text-heading">Tên đơn vị <span v-if="formMode !== 'view'" class="text-(--color-danger-text)">*</span></label>
            <div v-if="formMode === 'view'" class="text-sm px-3 py-2 bg-(--surface-input) rounded-lg text-body font-semibold">
              {{ formData.name }}
            </div>
            <input 
              v-else 
              v-model="formData.name" 
              type="text" 
              required
              placeholder="Nhập tên đơn vị..."
              class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)"
            />
          </div>

          <!-- Level -->
          <div class="space-y-1.5">
            <label class="block text-xs font-bold text-heading">Cấp đơn vị <span v-if="formMode !== 'view'" class="text-(--color-danger-text)">*</span></label>
            <div v-if="formMode === 'view'" class="text-sm px-3 py-2 bg-(--surface-input) rounded-lg text-body">
              <span class="inline-flex items-center gap-1.5 px-2 py-0.5 rounded-md bg-(--color-info-bg) text-(--color-info-text) font-bold text-xs">
                <component :is="getIconForLevel(formData.organizationLevel)" :size="12" />
                {{ formData.organizationLevel }}
              </span>
            </div>
            <select 
              v-else 
              v-model="formData.organizationLevel"
              required
              class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)"
            >
              <option value="" disabled>-- Chọn cấp đơn vị --</option>
              <option v-for="lvl in organizationLevels" :key="lvl" :value="lvl">{{ lvl }}</option>
            </select>
          </div>

          <!-- Status / Dates (View Only) -->
          <div v-if="formMode === 'view' && selectedNode" class="pt-4 space-y-3">
            <div class="flex justify-between text-sm">
              <span class="text-muted font-medium">Trạng thái:</span>
              <span :class="selectedNode.isActive ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="font-bold">
                {{ selectedNode.isActive ? 'Đang hoạt động' : 'Đã vô hiệu hóa' }}
              </span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-muted font-medium">Ngày tạo:</span>
              <span class="text-body">{{ formatDate(selectedNode.createdAt) }}</span>
            </div>
            <div v-if="selectedNode.updatedAt" class="flex justify-between text-sm">
              <span class="text-muted font-medium">Cập nhật lần cuối:</span>
              <span class="text-body">{{ formatDate(selectedNode.updatedAt) }}</span>
            </div>
          </div>
          
          <!-- Error Alert -->
          <div v-if="apiError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start mt-4">
            <AlertCircle :size="16" class="shrink-0 mt-0.5" />
            <span>{{ apiError }}</span>
          </div>

        </form>
      </div>

      <!-- Footer Actions -->
      <div v-if="selectedNode || formMode !== 'view'" class="p-4 bg-(--surface-card) flex items-center justify-between">
        
        <!-- Left actions (Delete) -->
        <div>
          <button 
            v-if="formMode === 'view' && canEdit && selectedNode" 
            @click="confirmDelete(selectedNode)"
            class="flex items-center gap-1.5 px-3 py-2 text-(--color-danger-text) hover:bg-(--color-danger-bg) rounded-lg text-xs font-bold transition-colors"
          >
            <Trash2 :size="16" />
            <span>Xóa đơn vị</span>
          </button>
        </div>

        <!-- Right actions -->
        <div class="flex items-center gap-2">
          <!-- View Mode Actions -->
          <template v-if="formMode === 'view' && canEdit">
            <button 
              @click="handleCreateChild"
              class="flex items-center gap-1.5 px-4 py-2 border border-input bg-(--surface-input) hover:bg-(--surface-input-hover) text-body text-xs font-bold rounded-lg transition-colors"
            >
              <Plus :size="16" />
              <span>Thêm đơn vị con</span>
            </button>
            <button 
              @click="handleEdit"
              class="flex items-center gap-1.5 px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors"
            >
              <Edit2 :size="16" />
              <span>Chỉnh sửa</span>
            </button>
          </template>

          <!-- Edit/Create Mode Actions -->
          <template v-if="formMode !== 'view'">
            <button 
              type="button"
              @click="cancelEdit"
              class="flex items-center gap-1.5 px-4 py-2 border border-input bg-transparent hover:bg-(--surface-input) text-body text-xs font-bold rounded-lg transition-colors"
            >
              <X :size="16" />
              <span>Hủy bỏ</span>
            </button>
            <button 
              @click="saveOrganization"
              :disabled="saving"
              class="flex items-center gap-1.5 px-4 py-2 bg-(--color-success-bg) border border-(--color-success-text)/20 text-(--color-success-text) text-xs font-bold rounded-lg hover:bg-(--color-success-text) hover:text-white transition-colors disabled:opacity-50"
            >
              <Loader2 v-if="saving" :size="16" class="animate-spin" />
              <Check v-else :size="16" />
              <span>{{ saving ? 'Đang lưu...' : 'Lưu lại' }}</span>
            </button>
          </template>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Network, Building2, Plus, Edit2, Trash2, Check, X, 
  ChevronRight, ChevronDown, Loader2, AlertCircle, Library, Users 
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { bghApi } from '@/services/bghApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const authStore = useAuthStore()
const canEdit = computed(() => authStore.hasRole('SuperAdmin'))

const treeData = ref([])
const flatOrganizationsList = ref([])
const loadingTree = ref(false)
const errorTree = ref(null)
const saving = ref(false)
const apiError = ref('')

const expandedNodes = ref(new Set())
const selectedNode = ref(null)

const formMode = ref('view')
const formData = ref({
  id: null,
  parentId: null,
  name: '',
  organizationLevel: ''
})

const organizationLevels = ['Cơ sở', 'Khoa', 'Bộ môn', 'Phòng ban', 'Trung tâm']

const getIconForLevel = (level) => {
  switch (level) {
    case 'Cơ sở': return Building2
    case 'Khoa': return Library
    case 'Phòng ban': return Users
    default: return Network
  }
}

const formatDate = (dateStr) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleString('vi-VN')
}

const getParentName = (parentId) => {
  if (!parentId) return null
  const parent = flatOrganizationsList.value.find(org => org.id === parentId)
  return parent ? parent.name : null
}

const loadData = async () => {
  loadingTree.value = true
  errorTree.value = null
  try {
    const [treeRes, flatRes] = await Promise.all([
      bghApi.getOrganizationsTree(),
      bghApi.getOrganizations()
    ])
    treeData.value = unwrapApiData(treeRes) || []
    flatOrganizationsList.value = (unwrapApiData(flatRes) || []).map(o => ({ id: o.id, name: o.name, parentId: o.parentId }))
  } catch (e) {
    errorTree.value = e?.message || 'Lỗi tải dữ liệu cơ cấu tổ chức'
  } finally {
    loadingTree.value = false
  }
}

const flattenTreeData = (nodes, level = 0, parentPath = []) => {
  let result = []
  for (const node of nodes) {
    const currentPath = [...parentPath, node.id]
    result.push({ 
      ...node, 
      _level: level, 
      _parentPath: parentPath 
    })
    if (level === 0) {
      expandedNodes.value.add(node.id)
    }
    if (node.children && node.children.length > 0) {
      result = result.concat(flattenTreeData(node.children, level + 1, currentPath))
    }
  }
  return result
}

const flattenedTree = computed(() => flattenTreeData(treeData.value))

const isNodeVisible = (node) => {
  for (const parentId of node._parentPath) {
    if (!expandedNodes.value.has(parentId)) {
      return false
    }
  }
  return true
}

const toggleExpand = (nodeId) => {
  if (expandedNodes.value.has(nodeId)) {
    expandedNodes.value.delete(nodeId)
  } else {
    expandedNodes.value.add(nodeId)
  }
}

const selectNode = (node) => {
  selectedNode.value = node
  formMode.value = 'view'
  apiError.value = ''
  formData.value = {
    id: node.id,
    parentId: node.parentId,
    name: node.name,
    organizationLevel: node.organizationLevel
  }
}

const handleCreateRoot = () => {
  selectedNode.value = null
  formMode.value = 'create'
  formData.value = { id: null, parentId: null, name: '', organizationLevel: 'Cơ sở' }
  apiError.value = ''
}

const handleCreateChild = () => {
  if (!selectedNode.value) return
  expandedNodes.value.add(selectedNode.value.id)
  formMode.value = 'create'
  formData.value = { 
    id: null, 
    parentId: selectedNode.value.id, 
    name: '', 
    organizationLevel: '' 
  }
  apiError.value = ''
}

const handleEdit = () => {
  formMode.value = 'edit'
  apiError.value = ''
}

const cancelEdit = () => {
  formMode.value = 'view'
  apiError.value = ''
  if (selectedNode.value) {
    formData.value = {
      id: selectedNode.value.id,
      parentId: selectedNode.value.parentId,
      name: selectedNode.value.name,
      organizationLevel: selectedNode.value.organizationLevel
    }
  }
}

const saveOrganization = async () => {
  if (!formData.value.name || !formData.value.organizationLevel) {
    apiError.value = 'Vui lòng điền đầy đủ tên và cấp đơn vị.'
    return
  }
  saving.value = true
  apiError.value = ''

  try {
    if (formMode.value === 'create') {
      await apiRequest('/api/organizations', {
        method: 'POST',
        body: JSON.stringify({
          parentId: formData.value.parentId,
          name: formData.value.name,
          organizationLevel: formData.value.organizationLevel,
        })
      })
    } else {
      await apiRequest(`/api/organizations/${formData.value.id}`, {
        method: 'PUT',
        body: JSON.stringify({
          parentId: formData.value.parentId,
          name: formData.value.name,
          organizationLevel: formData.value.organizationLevel,
        })
      })
    }
    formMode.value = 'view'
    await loadData()
    if (formData.value.id) {
      const flat = flattenTreeData(treeData.value)
      const found = flat.find(f => f.id === formData.value.id)
      if (found) selectNode(found)
    } else {
      selectedNode.value = null
      formData.value = { id: null, parentId: null, name: '', organizationLevel: '' }
    }
  } catch (e) {
    apiError.value = e?.message || 'Lỗi lưu dữ liệu'
  } finally {
    saving.value = false
  }
}

const confirmDelete = async (node) => {
  saving.value = true
  apiError.value = ''

  try {
    await apiRequest(`/api/organizations/${node.id}`, { method: 'DELETE' })
    await loadData()
    selectedNode.value = null
    formData.value = { id: null, parentId: null, name: '', organizationLevel: '' }
  } catch (e) {
    apiError.value = e?.message || 'Lỗi xóa đơn vị'
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadData()
})
</script>
