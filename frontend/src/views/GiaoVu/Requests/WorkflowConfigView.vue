<script setup>
import { ref } from 'vue'
import { 
  Settings2, 
  Plus, 
  ArrowRight, 
  CheckCircle2, 
  Clock, 
  Shield, 
  Zap, 
  FileJson,
  MoreVertical,
  Layers,
  Edit2,
  Trash2,
  Pen,
  X,
  Save,
  Loader2,
  AlertCircle
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const roleOptions = ['Giáo vụ khoa', 'Trưởng phòng Giáo vụ', 'Ban Giám hiệu', 'Phòng Đào tạo', 'Hệ thống']
const roleColors = { 'Giáo vụ khoa': 'text-link', 'Trưởng phòng Giáo vụ': 'text-body', 'Ban Giám hiệu': 'text-[var(--lg-warning)]', 'Phòng Đào tạo': 'text-[var(--lg-info)]', 'Hệ thống': 'text-[var(--lg-success)]' }

// ── Mock Data ────────────────────────────────────────────────
const workflows = ref([
  { id: 'WF-01', name: 'Nghỉ học / Bảo lưu', steps: 3, sla: '72h', auto: false, status: 'active' },
  { id: 'WF-02', name: 'Chuyển lớp học phần', steps: 2, sla: '48h', auto: true, status: 'active' },
  { id: 'WF-03', name: 'Cấp giấy xác nhận', steps: 1, sla: '24h', auto: true, status: 'active' },
  { id: 'WF-04', name: 'Thi lại / Hoãn thi', steps: 2, sla: '48h', auto: false, status: 'draft' },
])

const activeWorkflow = ref(workflows.value[1])

let nextId = 5

// ── Add Workflow Modal ───────────────────────────────────────
const showAddModal = ref(false)
const addForm = ref({ name: '', sla: '24h', steps: 1 })
const adding = ref(false)

function openAddModal() {
  addForm.value = { name: '', sla: '24h', steps: 1 }
  showAddModal.value = true
}

function submitAdd() {
  if (!addForm.value.name.trim()) return
  adding.value = true
  setTimeout(() => {
    workflows.value.unshift({
      id: `WF-${String(nextId++).padStart(2, '0')}`,
      name: addForm.value.name,
      sla: addForm.value.sla,
      status: 'draft',
      steps: addForm.value.steps,
    })
    adding.value = false
    showAddModal.value = false
    popupStore.success('Thêm workflow', `Đã tạo workflow "${addForm.value.name}".`)
  }, 400)
}

function closeAddModal() {
  showAddModal.value = false
}

// ── Edit Workflow Modal ──────────────────────────────────────
const showEditModal = ref(false)
const editForm = ref({ name: '', sla: '24h' })
const saving = ref(false)

function openEditModal() {
  editForm.value = {
    name: activeWorkflow.value.name,
    sla: activeWorkflow.value.sla,
  }
  showEditModal.value = true
}

function submitEdit() {
  if (!editForm.value.name.trim()) return
  saving.value = true
  setTimeout(() => {
    activeWorkflow.value.name = editForm.value.name
    activeWorkflow.value.sla = editForm.value.sla
    saving.value = false
    showEditModal.value = false
    popupStore.success('Cập nhật workflow', `Đã cập nhật "${editForm.value.name}".`)
  }, 400)
}

function closeEditModal() {
  showEditModal.value = false
}

// ── Delete Workflow ──────────────────────────────────────────
const showDeleteModal = ref(false)

function openDeleteModal() {
  showDeleteModal.value = true
}

function confirmDelete() {
  const idx = workflows.value.findIndex(w => w.id === activeWorkflow.value.id)
  if (idx !== -1) {
    workflows.value.splice(idx, 1)
    activeWorkflow.value = workflows.value[0] || null
  }
  showDeleteModal.value = false
  popupStore.success('Xóa workflow', `Đã xóa workflow.`)
}

function closeDeleteModal() {
  showDeleteModal.value = false
}

// ── Context Menu ─────────────────────────────────────────────
const showFloatingMenu = ref(false)

function toggleFloatingMenu() {
  showFloatingMenu.value = !showFloatingMenu.value
}

const getStatusBadge = (status) => {
  return status === 'active' ? 'lg-badge-success' : 'surface-solid text-placeholder border-default'
}

const getStatusLabel = (status) => {
  return status === 'active' ? 'Đang áp dụng' : 'Bản nháp'
}
</script>

<template>
  <PageContainer 
    title="Cấu hình quy trình duyệt" 
    subtitle="Thiết lập các bước phê duyệt, vai trò và thời gian xử lý cho từng loại đơn từ."
  >
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8" @click="showFloatingMenu = false">
      
      <!-- ── Left: Workflow List ── -->
      <div class="space-y-4">
        <div class="flex items-center justify-between mb-2">
            <h4 class="text-xs font-semibold text-label uppercase tracking-widest">Loại đơn & Workflow</h4>
            <button class="text-[11px] font-semibold text-link uppercase tracking-widest flex items-center gap-1 hover:underline" @click="openAddModal">
              <Plus :size="12" /> Thêm mới
           </button>
        </div>
        
        <div 
          v-for="wf in workflows" 
          :key="wf.id"
          @click="activeWorkflow = wf"
          :class="[
            'p-5 rounded-3xl border transition-all cursor-pointer group',
            activeWorkflow.id === wf.id 
              ? 'lg-glass-strong border-default' 
              : 'lg-card-glass hover:border-[var(--border-input-focus)]'
          ]"
        >
           <div class="flex items-start justify-between">
              <div>
                  <p :class="['text-sm font-semibold leading-tight', activeWorkflow.id === wf.id ? 'text-heading' : 'text-heading']">
                    {{ wf.name }}
                 </p>
                 <div class="flex items-center gap-3 mt-2">
                     <span :class="['text-[10px] font-semibold uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-label' : 'text-placeholder']">
                        <Layers :size="12" /> {{ wf.steps }} bước
                     </span>
                     <span :class="['text-[10px] font-semibold uppercase tracking-tighter flex items-center gap-1', activeWorkflow.id === wf.id ? 'text-label' : 'text-placeholder']">
                       <Clock :size="12" /> SLA {{ wf.sla }}
                    </span>
                 </div>
              </div>
               <div :class="['h-8 w-8 rounded-xl flex items-center justify-center transition-colors', activeWorkflow.id === wf.id ? 'surface-solid text-heading' : 'surface-solid text-placeholder']">
                 <ArrowRight :size="16" />
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Workflow Builder ── -->
      <div class="lg:col-span-2 space-y-4">
        
        <!-- Header Info -->
        <div class="surface-card border border-card rounded-2xl p-5 flex items-center justify-between">
           <div class="flex items-center gap-5">
               <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-body border-default">
                 <Settings2 :size="28" />
              </div>
              <div>
                 <div class="flex items-center gap-3">
                     <h3 class="text-xl font-semibold text-heading">{{ activeWorkflow.name }}</h3>
                     <span v-if="activeWorkflow" :class="['px-2 py-0.5 text-[10px] font-semibold uppercase tracking-widest border rounded-full', getStatusBadge(activeWorkflow.status)]">{{ getStatusLabel(activeWorkflow.status) }}</span>
                 </div>
                  <p class="text-xs font-bold text-label mt-1 uppercase tracking-tighter">{{ activeWorkflow.id }} • Phiên bản v2.1.0</p>
              </div>
           </div>
           <div class="flex items-center gap-2">
             <button class="lg-button-secondary px-4 py-2.5 text-sm font-bold flex items-center gap-2" @click="openEditModal">
                <Pen :size="16" /> Sửa
             </button>
             <button class="lg-button-secondary px-3 py-2.5 text-sm font-bold text-[var(--lg-danger)] hover:bg-[var(--color-danger-bg)]" @click="openDeleteModal" title="Xóa workflow">
                <Trash2 :size="16" />
             </button>
           </div>
        </div>

        <!-- Builder Visualizer -->
         <div class="surface-card border border-card rounded-2xl p-8 relative overflow-visible">
           
           <div class="flex flex-col items-center gap-12">
              
               <!-- Step 1 -->
                <div class="flex items-center w-full max-w-sm">
                   <div class="h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-semibold shadow-sm shrink-0 border border-default">1</div>
                   <div class="flex-1 -ml-3 p-4 surface-card border border-card rounded-2xl shadow-sm group hover:border-[var(--border-input-focus)] transition-all">
                      <h5 class="text-sm font-semibold text-heading uppercase tracking-wide">Tiếp nhận & Kiểm tra</h5>
                     <div class="mt-4 flex items-center justify-between">
                         <div class="flex items-center gap-2 text-[11px] font-bold text-label">
                            <Shield :size="14" class="text-link" /> Vai trò: Giáo vụ khoa
                         </div>
                         <span class="text-[10px] font-semibold text-[var(--color-warning-text)] uppercase tracking-widest bg-[var(--color-warning-bg)] px-2 py-1 rounded-lg">SLA: 24h</span>
                     </div>
                  </div>
               </div>

               <div class="h-10 w-0.5 border-default flex items-center justify-center">
                  <div class="h-2 w-2 rounded-full bg-[var(--lg-primary)] animate-bounce"></div>
               </div>

                <!-- Step 2 -->
                 <div class="flex items-center w-full max-w-sm">
                   <div class="h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-semibold shadow-sm shrink-0 border border-default">2</div>
                   <div class="flex-1 -ml-3 p-4 surface-card rounded-2xl border border-card shadow-sm group">
                      <div class="flex items-center justify-between mb-4">
                         <h5 class="text-sm font-semibold text-heading uppercase tracking-wide">Phê duyệt chính</h5>
                         <Zap :size="18" class="text-[var(--color-warning-text)] fill-[var(--color-warning-text)]" title="Auto-Execute Available" />
                     </div>
                     <div class="flex items-center justify-between">
                         <div class="flex items-center gap-2 text-[11px] font-bold text-label">
                            <Shield :size="14" class="text-body" /> Trưởng phòng Giáo vụ
                         </div>
                         <span class="text-[10px] font-semibold text-[var(--color-warning-text)] uppercase tracking-widest bg-[var(--color-warning-bg)] px-2 py-1 rounded-lg">SLA: 48h</span>
                     </div>
                  </div>
               </div>

               <div class="h-10 w-0.5 border-default flex items-center justify-center">
                  <div class="h-2 w-2 rounded-full bg-[var(--lg-primary)]"></div>
              </div>

               <!-- Execution Step -->
                <div class="flex items-center w-full max-w-sm">
                   <div class="h-8 w-8 rounded-full surface-solid text-heading flex items-center justify-center text-xs font-semibold shadow-sm shrink-0 border border-default">
                     <CheckCircle2 :size="18" />
                  </div>
                   <div class="flex-1 -ml-3 p-5 surface-solid rounded-3xl border-2 border-dashed border-default">
                     <div class="flex items-center gap-3">
                        <div class="h-10 w-10 rounded-xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] shadow-sm">
                           <FileJson :size="20" />
                        </div>
                        <div>
                            <h5 class="text-xs font-semibold text-heading uppercase tracking-widest">Thực thi tự động</h5>
                            <p class="text-[10px] font-bold text-[var(--color-success-text)] mt-1 uppercase tracking-tighter">Update DB + Send PDF Email</p>
                        </div>
                     </div>
                  </div>
               </div>

           </div>

           <!-- Floating Tools -->
           <div class="absolute bottom-6 right-6 flex flex-col gap-3">
             <div class="relative">
               <button class="h-10 w-10 lg-card-glass rounded-2xl border-default text-placeholder hover:text-link shadow-sm flex items-center justify-center transition-all" @click="openAddModal" title="Thêm workflow mới">
                  <Plus :size="24" />
               </button>
             </div>
             <div class="relative" @click.stop>
               <button class="h-10 w-10 surface-solid rounded-2xl text-heading shadow-sm flex items-center justify-center hover:bg-[var(--surface-input)] transition-all" @click="toggleFloatingMenu">
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
                  <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-info-bg)] hover:text-link transition-all" @click="openEditModal(); showFloatingMenu = false">
                    <Pen :size="14" /> Chỉnh sửa
                  </button>
                  <button class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] font-semibold text-label hover:bg-[var(--color-danger-bg)] hover:text-[var(--lg-danger)] transition-all" @click="openDeleteModal(); showFloatingMenu = false">
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

    <!-- ═══════════════════════════════════════════════════════
         ADD WORKFLOW MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showAddModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeAddModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Plus :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Thêm workflow mới</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeAddModal">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tên workflow</label>
              <input v-model="addForm.name" type="text" placeholder="VD: Xử lý đơn nghỉ học" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">SLA tổng</label>
              <select v-model="addForm.sla" class="w-full lg-input px-4 py-2.5 text-sm appearance-none cursor-pointer">
                <option value="24h">24h</option>
                <option value="48h">48h</option>
                <option value="72h">72h</option>
                <option value="1 tuần">1 tuần</option>
              </select>
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Số bước</label>
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

    <!-- ═══════════════════════════════════════════════════════
         EDIT WORKFLOW MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showEditModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeEditModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-info-bg)] text-link flex items-center justify-center">
                <Pen :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Chỉnh sửa workflow</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeEditModal">
              <X :size="18" />
            </button>
          </div>
          <div class="space-y-4">
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Tên workflow</label>
              <input v-model="editForm.name" type="text" class="w-full lg-input px-4 py-2.5 text-sm" />
            </div>
            <div>
              <label class="text-[10px] font-semibold text-label uppercase tracking-widest mb-1.5 block">SLA tổng</label>
              <select v-model="editForm.sla" class="w-full lg-input px-4 py-2.5 text-sm appearance-none cursor-pointer">
                <option value="24h">24h</option>
                <option value="48h">48h</option>
                <option value="72h">72h</option>
                <option value="1 tuần">1 tuần</option>
              </select>
            </div>
            <div class="p-3 surface-solid rounded-2xl">
              <p class="text-[10px] font-bold text-placeholder">Hiện tại {{ activeWorkflow.steps }} bước xử lý</p>
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

    <!-- ═══════════════════════════════════════════════════════
         DELETE WORKFLOW MODAL
         ═══════════════════════════════════════════════════════ -->
    <Transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="showDeleteModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="closeDeleteModal">
        <div class="absolute inset-0 bg-black/30 backdrop-blur-sm" />
        <div class="relative w-full max-w-md surface-modal rounded-2xl p-6 shadow-sm border border-default">
          <div class="flex items-center justify-between mb-5">
            <div class="flex items-center gap-2">
              <div class="h-8 w-8 rounded-lg bg-[var(--color-danger-bg)] text-[var(--lg-danger)] flex items-center justify-center">
                <AlertCircle :size="18" />
              </div>
              <h3 class="text-base font-semibold text-heading">Xóa workflow</h3>
            </div>
            <button class="p-1.5 rounded-lg hover:bg-[var(--surface-solid)] text-placeholder transition-all" @click="closeDeleteModal">
              <X :size="18" />
            </button>
          </div>
          <div class="bg-[var(--color-danger-bg)] border border-[var(--lg-danger)]/30 rounded-2xl p-4 mb-4">
            <p class="text-[13px] font-bold text-[var(--lg-danger)]">
              Bạn có chắc chắn muốn xóa workflow <strong>{{ activeWorkflow?.name }}</strong>? 
            </p>
            <p class="text-[11px] font-medium text-[var(--lg-danger)]/70 mt-2">Các workflow đã phát sinh đơn từ thực tế sẽ không được phép xóa. Thao tác này không thể hoàn tác.</p>
          </div>
          <div class="flex items-center justify-end gap-3 mt-6 pt-4 border-t border-default">
            <button class="lg-button-secondary px-5 py-2.5 text-sm font-bold" @click="closeDeleteModal">Quay lại</button>
            <button class="px-5 py-2.5 text-sm font-bold text-white bg-[var(--lg-danger)] hover:opacity-90 rounded-2xl transition-all flex items-center gap-2" @click="confirmDelete">
              <Trash2 :size="16" /> Xác nhận xóa
            </button>
          </div>
        </div>
      </div>
    </Transition>

  </PageContainer>
</template>
