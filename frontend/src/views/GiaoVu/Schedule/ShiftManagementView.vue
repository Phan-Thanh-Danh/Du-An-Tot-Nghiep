<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Clock, Sun, Moon, Sunset, Loader2 } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { shiftApi } from '@/services/shiftApi'

const loading = ref(true); const error = ref(null); const shifts = ref([])
const searchQuery = ref(''); const filterBuoi = ref('all'); const filterStatus = ref('all')

const BUOI_OPTIONS = [
  { value: 'all', label: 'Tất cả buổi' },
  { value: 'Sáng', label: 'Sáng' },
  { value: 'Chiều', label: 'Chiều' },
  { value: 'Tối', label: 'Tối' },
]
const BUOI_ICON = { Sáng: Sun, Chiều: Sunset, Tối: Moon }
const BUOI_COLOR = {
  Sáng: 'text-(--color-info-text) bg-(--color-info-bg)',
  Chiều: 'text-(--color-warning-text) bg-(--color-warning-bg)',
  Tối: 'text-(--color-success-text) bg-(--color-success-bg)',
}

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ tenCa: '', buoi: 'Sáng', gioBatDau: '', gioKetThuc: '', thuTu: null })
const formErrors = ref({})
const confirmDelete = ref(null)

onMounted(fetchShifts)
async function fetchShifts() {
  loading.value = true; error.value = null
  try { const res = await shiftApi.list({ PageSize: 200 }); shifts.value = Array.isArray(res) ? res : (res?.items || res?.data || []) }
  catch (e) { error.value = e.message || 'Không thể tải danh sách ca học' }
  finally { loading.value = false }
}

const filteredShifts = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return shifts.value.filter(s => {
    const ms = !q || s.tenCa?.toLowerCase().includes(q)
    const mb = filterBuoi.value === 'all' || s.buoi === filterBuoi.value
    const mst = filterStatus.value === 'all' || (filterStatus.value === 'active' && s.conHoatDong !== false) || (filterStatus.value === 'inactive' && s.conHoatDong === false)
    return ms && mb && mst
  })
})

const stats = computed(() => ({
  total: shifts.value.length,
  active: shifts.value.filter(s => s.conHoatDong !== false).length,
  inactive: shifts.value.filter(s => s.conHoatDong === false).length,
}))

function clearFilters() { searchQuery.value = ''; filterBuoi.value = 'all'; filterStatus.value = 'all' }

// ── Form ──
const defaults = () => ({ tenCa: '', buoi: 'Sáng', gioBatDau: '', gioKetThuc: '', thuTu: null })
function resetForm() { formData.value = defaults(); formErrors.value = {} }

function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(s) {
  formMode.value = 'edit'; editingId.value = s.maCaHoc
  formData.value = { tenCa: s.tenCa, buoi: s.buoi, gioBatDau: s.gioBatDau, gioKetThuc: s.gioKetThuc, thuTu: s.thuTu }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.tenCa.trim()) e.tenCa = 'Tên ca không được để trống'
  if (!formData.value.gioBatDau.trim()) e.gioBatDau = 'Giờ bắt đầu không được để trống'
  if (!formData.value.gioKetThuc.trim()) e.gioKetThuc = 'Giờ kết thúc không được để trống'
  if (formData.value.thuTu === null || formData.value.thuTu === '' || Number(formData.value.thuTu) < 1) e.thuTu = 'Thứ tự phải >= 1'
  formErrors.value = e; return Object.keys(e).length === 0
}

async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    const payload = { tenCa: formData.value.tenCa.trim(), buoi: formData.value.buoi, gioBatDau: formData.value.gioBatDau, gioKetThuc: formData.value.gioKetThuc, thuTu: Number(formData.value.thuTu) }
    if (formMode.value === 'edit') await shiftApi.update(editingId.value, payload)
    else await shiftApi.create(payload)
    closeForm(); await fetchShifts()
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu ca học' }
  finally { submitting.value = false }
}

// ── Actions ──
async function toggleActive(s) {
  try { await shiftApi.toggleActive(s.maCaHoc); await fetchShifts() }
  catch { /* ignore */ }
}
function requestDelete(s) { confirmDelete.value = s }
async function executeDelete() {
  if (!confirmDelete.value) return
  try { await shiftApi.toggleActive(confirmDelete.value.maCaHoc); confirmDelete.value = null; await fetchShifts() }
  catch { confirmDelete.value = null }
}
</script>

<template>
  <div class="space-y-4">

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng ca học</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.total }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <Clock :size="20" class="text-(--color-info-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Đang hoạt động</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.active }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) flex items-center justify-center">
            <Clock :size="20" class="text-(--color-success-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Ngừng hoạt động</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.inactive }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center">
            <Clock :size="20" class="text-(--color-danger-text)" />
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
            <input v-model="searchQuery" type="text" placeholder="Tìm tên ca học..."
              class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
          <select v-model="filterBuoi" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option v-for="o in BUOI_OPTIONS" :key="o.value" :value="o.value">{{ o.label }}</option>
          </select>
          <select v-model="filterStatus" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="all">Tất cả trạng thái</option>
            <option value="active">Đang hoạt động</option>
            <option value="inactive">Ngừng hoạt động</option>
          </select>
          <button v-if="filterBuoi !== 'all' || filterStatus !== 'all' || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate">
              <Plus :size="15" class="mr-1" /> Thêm ca học
            </GlassButton>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="6" /></div>
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchShifts">Thử lại</GlassButton>
        </div>
      </div>
      <div v-else-if="filteredShifts.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy ca học nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm ca học</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold text-center w-16">Thứ tự</th>
              <th class="px-3 py-4 font-semibold">Tên ca</th>
              <th class="px-3 py-4 font-semibold w-24">Buổi</th>
              <th class="px-3 py-4 font-semibold w-32">Giờ BĐ</th>
              <th class="px-3 py-4 font-semibold w-32">Giờ KT</th>
              <th class="px-3 py-4 font-semibold w-28">Trạng thái</th>
              <th class="px-3 py-4 font-semibold text-center w-32">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="s in filteredShifts" :key="s.maCaHoc" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 text-center"><span class="font-bold text-heading">#{{ s.thuTu }}</span></td>
              <td class="px-3 py-3.5 font-bold text-heading">{{ s.tenCa }}</td>
              <td class="px-3 py-3.5">
                <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[11px] font-bold" :class="BUOI_COLOR[s.buoi] || ''">
                  <component :is="BUOI_ICON[s.buoi] || Sun" :size="12" /> {{ s.buoi }}
                </span>
              </td>
              <td class="px-3 py-3.5 font-mono text-body">{{ s.gioBatDau }}</td>
              <td class="px-3 py-3.5 font-mono text-body">{{ s.gioKetThuc }}</td>
              <td class="px-3 py-3.5">
                <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold"
                  :class="s.conHoatDong ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)'">
                  <span class="h-1.5 w-1.5 rounded-full" :class="s.conHoatDong ? 'bg-(--color-success-text)' : 'bg-(--color-danger-text)'"></span>
                  {{ s.conHoatDong ? 'Hoạt động' : 'Ngừng' }}
                </span>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" :title="s.conHoatDong ? 'Vô hiệu hóa' : 'Kích hoạt'" @click.stop="toggleActive(s)">
                    <component :is="s.conHoatDong ? X : Clock" :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Chỉnh sửa" @click.stop="openEdit(s)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Ngừng hoạt động" @click.stop="requestDelete(s)">
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
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm ca học' : 'Chỉnh sửa ca học' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tên ca <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.tenCa" type="text" placeholder="VD: Ca 1" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.tenCa ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.tenCa" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.tenCa }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Buổi <span class="text-(--color-danger-text)">*</span></label>
                  <select v-model="formData.buoi" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option value="Sáng">Sáng</option>
                    <option value="Chiều">Chiều</option>
                    <option value="Tối">Tối</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ tự <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model.number="formData.thuTu" type="number" min="1" placeholder="VD: 1" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.thuTu ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.thuTu" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.thuTu }}</p>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Giờ BĐ <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model="formData.gioBatDau" type="time" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.gioBatDau ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.gioBatDau" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.gioBatDau }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Giờ KT <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model="formData.gioKetThuc" type="time" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.gioKetThuc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.gioKetThuc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.gioKetThuc }}</p>
                </div>
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm ca học' : 'Cập nhật' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <ConfirmActionDialog v-if="confirmDelete" :show="true" title="Ngừng hoạt động ca học"
      :message="`Bạn có chắc muốn ngừng hoạt động ca &quot;${confirmDelete.tenCa}&quot;?`"
      confirm-label="Xác nhận" variant="danger" @confirm="executeDelete" @cancel="confirmDelete = null" />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
