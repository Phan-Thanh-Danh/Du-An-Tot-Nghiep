<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Book, Loader2 } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { subjectApi } from '@/services/subjectApi'

const loading = ref(true)
const error = ref(null)
const subjects = ref([])
const searchQuery = ref('')
const filterStatus = ref('all')

const showFormModal = ref(false)
const formMode = ref('create')
const editingId = ref(null)
const submitting = ref(false)
const confirmDelete = ref(null)

const formData = ref({ maCodeMonHoc: '', tenMonHoc: '', soTinChi: null })
const formErrors = ref({})

onMounted(fetchSubjects)

async function fetchSubjects() {
  loading.value = true
  error.value = null
  try {
    const res = await subjectApi.list({ PageSize: 500 })
    subjects.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch (e) {
    error.value = e.message || 'Không thể tải danh sách môn học'
  } finally {
    loading.value = false
  }
}

const filteredSubjects = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return subjects.value.filter(s => {
    const matchSearch = !q || s.tenMonHoc?.toLowerCase().includes(q) || s.maCodeMonHoc?.toLowerCase().includes(q)
    const matchStatus = filterStatus.value === 'all' || (filterStatus.value === 'active' && s.conHoatDong !== false) || (filterStatus.value === 'inactive' && s.conHoatDong === false)
    return matchSearch && matchStatus
  })
})

const stats = computed(() => ({
  total: subjects.value.length,
  active: subjects.value.filter(s => s.conHoatDong !== false).length,
  inactive: subjects.value.filter(s => s.conHoatDong === false).length,
}))

function clearFilters() { searchQuery.value = ''; filterStatus.value = 'all' }

function resetForm() { formData.value = { maCodeMonHoc: '', tenMonHoc: '', soTinChi: null }; formErrors.value = {} }

function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(s) { formMode.value = 'edit'; editingId.value = s.maMonHoc; formData.value = { maCodeMonHoc: s.maCodeMonHoc, tenMonHoc: s.tenMonHoc, soTinChi: s.soTinChi }; formErrors.value = {}; showFormModal.value = true }
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.maCodeMonHoc.trim()) e.maCodeMonHoc = 'Mã môn không được để trống'
  if (!formData.value.tenMonHoc.trim()) e.tenMonHoc = 'Tên môn không được để trống'
  if (formData.value.soTinChi === null || formData.value.soTinChi === '' || Number(formData.value.soTinChi) < 1) e.soTinChi = 'Số tín chỉ phải >= 1'
  formErrors.value = e
  return Object.keys(e).length === 0
}

async function submitForm() {
  if (!validate()) return
  submitting.value = true
  try {
    const payload = { maCodeMonHoc: formData.value.maCodeMonHoc.trim(), tenMonHoc: formData.value.tenMonHoc.trim(), soTinChi: Number(formData.value.soTinChi) }
    if (formMode.value === 'edit') await subjectApi.update(editingId.value, payload)
    else await subjectApi.create(payload)
    closeForm()
    await fetchSubjects()
  } catch (e) {
    formErrors.value._api = e.message || 'Lỗi khi lưu môn học'
  } finally {
    submitting.value = false
  }
}

async function toggleActive(s) {
  try {
    if (s.conHoatDong) await subjectApi.deactivate(s.maMonHoc); else await subjectApi.activate(s.maMonHoc)
    await fetchSubjects()
  } catch { /* ignore */ }
}

function requestDelete(s) { confirmDelete.value = s }
async function executeDelete() {
  if (!confirmDelete.value) return
  try {
    await subjectApi.deactivate(confirmDelete.value.maMonHoc)
    confirmDelete.value = null
    await fetchSubjects()
  } catch {
    confirmDelete.value = null
  }
}
</script>

<template>
  <div class="space-y-4">
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng môn học</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.total }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <Book :size="20" class="text-(--color-info-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Đang sử dụng</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.active }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) flex items-center justify-center">
            <Book :size="20" class="text-(--color-success-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Ngừng sử dụng</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.inactive }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center">
            <Book :size="20" class="text-(--color-danger-text)" />
          </div>
        </div>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="p-4 border-b border-default bg-(--surface-input)">
        <div class="flex flex-wrap items-center gap-3">
          <div class="relative flex-1 min-w-[200px]">
            <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
            <input v-model="searchQuery" type="text" placeholder="Tìm mã môn, tên môn..."
              class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
          <select v-model="filterStatus" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="all">Tất cả trạng thái</option>
            <option value="active">Đang sử dụng</option>
            <option value="inactive">Ngừng sử dụng</option>
          </select>
          <button v-if="filterStatus !== 'all' || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate">
              <Plus :size="15" class="mr-1" /> Thêm môn học
            </GlassButton>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="6" /></div>

      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchSubjects">Thử lại</GlassButton>
        </div>
      </div>

      <div v-else-if="filteredSubjects.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy môn học nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm môn học</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold">Mã môn</th>
              <th class="px-3 py-4 font-semibold">Tên môn học</th>
              <th class="px-3 py-4 font-semibold text-center">Số TC</th>
              <th class="px-3 py-4 font-semibold">Trạng thái</th>
              <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="s in filteredSubjects" :key="s.maMonHoc" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">{{ s.maCodeMonHoc }}</td>
              <td class="px-3 py-3.5 font-bold text-heading max-w-[300px] truncate">{{ s.tenMonHoc }}</td>
              <td class="px-3 py-3.5 text-center font-bold text-body">{{ s.soTinChi }}</td>
              <td class="px-3 py-3.5">
                <span class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-bold"
                  :class="s.conHoatDong ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)'">
                  <span class="h-1.5 w-1.5 rounded-full" :class="s.conHoatDong ? 'bg-(--color-success-text)' : 'bg-(--color-danger-text)'"></span>
                  {{ s.conHoatDong ? 'Đang sử dụng' : 'Ngừng sử dụng' }}
                </span>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Chỉnh sửa" @click.stop="openEdit(s)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" :title="s.conHoatDong ? 'Ngừng sử dụng' : 'Kích hoạt'" @click.stop="s.conHoatDong ? requestDelete(s) : toggleActive(s)">
                    <X :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm môn học' : 'Chỉnh sửa môn học' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã môn <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.maCodeMonHoc" type="text" placeholder="VD: CT101" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maCodeMonHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.maCodeMonHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maCodeMonHoc }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tên môn học <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.tenMonHoc" type="text" placeholder="VD: Lập trình Cơ bản" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.tenMonHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.tenMonHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.tenMonHoc }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số tín chỉ <span class="text-(--color-danger-text)">*</span></label>
                <input v-model.number="formData.soTinChi" type="number" min="1" max="20" placeholder="VD: 3" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.soTinChi ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.soTinChi" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.soTinChi }}</p>
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm môn học' : 'Cập nhật' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <ConfirmActionDialog v-if="confirmDelete" :show="true" title="Ngừng sử dụng môn học"
      :message="`Bạn có chắc muốn ngừng sử dụng môn &quot;${confirmDelete.tenMonHoc}&quot; (${confirmDelete.maCodeMonHoc})?`"
      confirm-label="Xác nhận" variant="danger" @confirm="executeDelete" @cancel="confirmDelete = null" />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
