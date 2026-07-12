<script setup>
import { ref, computed, onMounted } from 'vue'
import { Plus, X, Pencil, Loader2, BookOpen, Search } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { creditMappingApi } from '@/services/creditMappingApi'

const loading = ref(true); const error = ref(null); const items = ref([])
const searchQuery = ref('')

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ soTinChi: '', soBlockHoc: '', soBuoiMoiTuan: '', soCaMoiBuoi: '' })
const formErrors = ref({})
const confirmDelete = ref(null)

onMounted(fetchItems)
async function fetchItems() {
  loading.value = true; error.value = null
  try { const res = await creditMappingApi.list(); items.value = Array.isArray(res) ? res : (res?.items || res?.data || []) }
  catch (e) { error.value = e.message || 'Không thể tải danh sách cấu hình' }
  finally { loading.value = false }
}

const stats = computed(() => ({
  total: items.value.length
}))

const filteredItems = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return items.value.filter(s => {
    if (!q) return true
    return s.soTinChi.toString().includes(q)
  })
})

// ── Form ──
const defaults = () => ({ soTinChi: '', soBlockHoc: '', soBuoiMoiTuan: '', soCaMoiBuoi: '' })
function resetForm() { formData.value = defaults(); formErrors.value = {} }

function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(s) {
  formMode.value = 'edit'; editingId.value = s.maQuyDoi
  formData.value = { soTinChi: s.soTinChi, soBlockHoc: s.soBlockHoc, soBuoiMoiTuan: s.soBuoiMoiTuan, soCaMoiBuoi: s.soCaMoiBuoi }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (formData.value.soTinChi === '' || Number(formData.value.soTinChi) < 1 || Number(formData.value.soTinChi) > 20) e.soTinChi = 'Số tín chỉ từ 1-20'
  if (formData.value.soBlockHoc === '' || Number(formData.value.soBlockHoc) < 1 || Number(formData.value.soBlockHoc) > 5) e.soBlockHoc = 'Số block từ 1-5'
  if (formData.value.soBuoiMoiTuan === '' || Number(formData.value.soBuoiMoiTuan) < 1 || Number(formData.value.soBuoiMoiTuan) > 10) e.soBuoiMoiTuan = 'Số buổi từ 1-10'
  if (formData.value.soCaMoiBuoi === '' || Number(formData.value.soCaMoiBuoi) < 1 || Number(formData.value.soCaMoiBuoi) > 5) e.soCaMoiBuoi = 'Số ca từ 1-5'
  
  formErrors.value = e; return Object.keys(e).length === 0
}

async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    const payload = { 
        soTinChi: Number(formData.value.soTinChi), 
        soBlockHoc: Number(formData.value.soBlockHoc), 
        soBuoiMoiTuan: Number(formData.value.soBuoiMoiTuan), 
        soCaMoiBuoi: Number(formData.value.soCaMoiBuoi) 
    }
    if (formMode.value === 'edit') await creditMappingApi.update(editingId.value, payload)
    else await creditMappingApi.create(payload)
    closeForm(); await fetchItems()
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu cấu hình' }
  finally { submitting.value = false }
}

// ── Actions ──
function requestDelete(s) { confirmDelete.value = s }
async function executeDelete() {
  if (!confirmDelete.value) return
  try { await creditMappingApi.delete(confirmDelete.value.maQuyDoi); confirmDelete.value = null; await fetchItems() }
  catch (e) { alert(e.message || 'Không thể xóa cấu hình này'); confirmDelete.value = null }
}
</script>

<template>
  <div class="space-y-4">
    <!-- Header/Stats -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng cấu hình</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.total }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <BookOpen :size="20" class="text-(--color-info-text)" />
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
            <input v-model="searchQuery" type="text" placeholder="Tìm kiếm theo số tín chỉ..."
              class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
          <button v-if="searchQuery" @click="searchQuery = ''"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate">
              <Plus :size="15" class="mr-1" /> Thêm cấu hình
            </GlassButton>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="5" /></div>
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchItems">Thử lại</GlassButton>
        </div>
      </div>
      <div v-else-if="filteredItems.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy cấu hình" description="Vui lòng thử tìm kiếm khác hoặc thêm mới.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm cấu hình</GlassButton>
        </EmptyState>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold text-center w-24">Số Tín chỉ</th>
              <th class="px-3 py-4 font-semibold text-center w-24">Số Block</th>
              <th class="px-3 py-4 font-semibold text-center w-28">Buổi / Tuần</th>
              <th class="px-3 py-4 font-semibold text-center w-28">Ca / Buổi</th>
              <th class="px-3 py-4 font-semibold text-center w-32">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="s in filteredItems" :key="s.maQuyDoi" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 text-center font-bold text-(--lg-primary)">{{ s.soTinChi }} tín chỉ</td>
              <td class="px-3 py-3.5 text-center font-bold text-heading">{{ s.soBlockHoc }} Block</td>
              <td class="px-3 py-3.5 text-center text-body">{{ s.soBuoiMoiTuan }} buổi</td>
              <td class="px-3 py-3.5 text-center text-body">{{ s.soCaMoiBuoi }} ca</td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Chỉnh sửa" @click.stop="openEdit(s)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Xóa" @click.stop="requestDelete(s)">
                    <X :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm cấu hình quy đổi' : 'Chỉnh sửa cấu hình' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số Tín chỉ <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model.number="formData.soTinChi" type="number" min="1" max="20" placeholder="VD: 3" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.soTinChi ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.soTinChi" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.soTinChi }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số Block Học <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model.number="formData.soBlockHoc" type="number" min="1" max="5" placeholder="VD: 1" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.soBlockHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.soBlockHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.soBlockHoc }}</p>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số Buổi / Tuần <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model.number="formData.soBuoiMoiTuan" type="number" min="1" max="10" placeholder="VD: 3" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.soBuoiMoiTuan ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.soBuoiMoiTuan" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.soBuoiMoiTuan }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số Ca / Buổi <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model.number="formData.soCaMoiBuoi" type="number" min="1" max="5" placeholder="VD: 1" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.soCaMoiBuoi ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.soCaMoiBuoi" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.soCaMoiBuoi }}</p>
                </div>
              </div>

            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm cấu hình' : 'Cập nhật' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <ConfirmActionDialog v-if="confirmDelete" :show="true" title="Xóa cấu hình"
      :message="`Bạn có chắc muốn xóa cấu hình cho ${confirmDelete.soTinChi} tín chỉ?`"
      confirm-label="Xác nhận" variant="danger" @confirm="executeDelete" @cancel="confirmDelete = null" />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
