<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Users, Loader2 } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { classApi } from '@/services/classApi'

const loading = ref(true); const error = ref(null); const rows = ref([])
const searchQuery = ref(''); const filterTrangThai = ref(''); const filterKhoa = ref('')

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ maCodeLop: '', tenLop: '', maKhoa: 1, maKhoaHoc: new Date().getFullYear(), siSoToiDa: 50, maGiaoVien: null, tenGiaoVien: '', namNhapHoc: String(new Date().getFullYear()), trangThai: 'moi', ghiChu: '' })
const formErrors = ref({})
const confirmDelete = ref(null)

const khoaOptions = [
  { ma: 1, ten: 'Công nghệ thông tin' }, { ma: 2, ten: 'Quản trị kinh doanh' },
  { ma: 3, ten: 'Ngôn ngữ Anh' }, { ma: 4, ten: 'Cơ khí' },
]
const statusOptions = [
  { value: 'moi', label: 'Mới' }, { value: 'dang_hoc', label: 'Đang học' },
  { value: 'da_tot_nghiep', label: 'Đã tốt nghiệp' }, { value: 'tam_dung', label: 'Tạm dừng' },
  { value: 'da_bi_huy', label: 'Đã hủy' },
]
const trangThaiBadge = {
  moi: { label: 'Mới', variant: 'info' },
  dang_hoc: { label: 'Đang học', variant: 'success' },
  da_tot_nghiep: { label: 'Đã tốt nghiệp', variant: 'violet' },
  tam_dung: { label: 'Tạm dừng', variant: 'warning' },
  da_bi_huy: { label: 'Đã hủy', variant: 'danger' },
}

onMounted(fetchData)
async function fetchData() {
  loading.value = true; error.value = null
  try {
    const data = await classApi.list({ TrangThai: filterTrangThai.value || undefined, MaKhoa: filterKhoa.value || undefined, Search: searchQuery.value || undefined })
    rows.value = Array.isArray(data) ? data : data?.items || data?.data || []
  } catch (e) { error.value = e.message || 'Không thể tải danh sách lớp' }
  finally { loading.value = false }
}

const filteredRows = computed(() => {
  let list = rows.value
  if (searchQuery.value) { const q = searchQuery.value.toLowerCase(); list = list.filter(r => r.maCodeLop?.toLowerCase().includes(q) || r.tenLop?.toLowerCase().includes(q) || r.tenGiaoVien?.toLowerCase().includes(q)) }
  if (filterTrangThai.value) list = list.filter(r => r.trangThai === filterTrangThai.value)
  if (filterKhoa.value) list = list.filter(r => r.maKhoa === Number(filterKhoa.value))
  return list
})

const summaryCards = computed(() => [
  { label: 'Tổng lớp', value: rows.value.length, color: 'text-(--lg-primary)', bg: 'bg-(--color-info-bg)' },
  { label: 'Đang học', value: rows.value.filter(r => r.trangThai === 'dang_hoc').length, color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)' },
  { label: 'Mới', value: rows.value.filter(r => r.trangThai === 'moi').length, color: 'text-blue-600', bg: 'bg-blue-50' },
  { label: 'Đã tốt nghiệp', value: rows.value.filter(r => r.trangThai === 'da_tot_nghiep').length, color: 'text-purple-600', bg: 'bg-purple-50' },
])

function clearFilters() { searchQuery.value = ''; filterTrangThai.value = ''; filterKhoa.value = '' }

// ── Form ──
const defaults = () => ({ maCodeLop: '', tenLop: '', maKhoa: 1, maKhoaHoc: new Date().getFullYear(), siSoToiDa: 50, maGiaoVien: null, tenGiaoVien: '', namNhapHoc: String(new Date().getFullYear()), trangThai: 'moi', ghiChu: '' })
function resetForm() { formData.value = defaults(); formErrors.value = {} }
function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(r) {
  formMode.value = 'edit'; editingId.value = r.maLop
  formData.value = { maCodeLop: r.maCodeLop, tenLop: r.tenLop, maKhoa: r.maKhoa, maKhoaHoc: r.maKhoaHoc, siSoToiDa: r.siSoToiDa, maGiaoVien: r.maGiaoVien, tenGiaoVien: r.tenGiaoVien || '', namNhapHoc: r.namNhapHoc, trangThai: r.trangThai, ghiChu: r.ghiChu || '' }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }
function validate() {
  const e = {}
  if (!formData.value.maCodeLop.trim()) e.maCodeLop = 'Mã lớp không được để trống'
  if (!formData.value.tenLop.trim()) e.tenLop = 'Tên lớp không được để trống'
  if (!formData.value.siSoToiDa || formData.value.siSoToiDa < 1) e.siSoToiDa = 'Sĩ số tối đa phải >= 1'
  formErrors.value = e; return Object.keys(e).length === 0
}
async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    if (formMode.value === 'edit') await classApi.update(editingId.value, formData.value)
    else await classApi.create(formData.value)
    closeForm(); await fetchData()
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu lớp' }
  finally { submitting.value = false }
}

// ── Actions ──
function requestDelete(r) { confirmDelete.value = r }
async function executeDelete() {
  if (!confirmDelete.value) return
  try { await classApi.delete(confirmDelete.value.maLop); confirmDelete.value = null; await fetchData() }
  catch { confirmDelete.value = null }
}
function getSiSoClass(r) { const p = r.siSo / (r.siSoToiDa || 1) * 100; return p >= 100 ? 'text-red-500 font-bold' : p >= 90 ? 'text-amber-500 font-bold' : 'text-(--text-body)' }
</script>

<template>
  <div class="space-y-4">

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="c in [
        { label: 'Tổng lớp', value: summaryCards[0].value, icon: Users, color: 'text-(--color-info-text)', bg: 'bg-(--color-info-bg)' },
        { label: 'Đang học', value: summaryCards[1].value, icon: Users, color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)' },
  { label: 'Mới', value: summaryCards[2].value, icon: Users, color: 'text-(--color-info-text)', bg: 'bg-(--color-info-bg)' },
  { label: 'Đã tốt nghiệp', value: summaryCards[3].value, icon: Users, color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)' },
      ]" :key="c.label" class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">{{ c.label }}</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ c.value }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl flex items-center justify-center" :class="c.bg">
            <component :is="c.icon" :size="20" :class="c.color" />
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
            <input v-model="searchQuery" type="text" placeholder="Tìm mã lớp, tên lớp, GVCN..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" @keydown.enter="fetchData" />
          </div>
          <select v-model="filterTrangThai" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả trạng thái</option>
            <option v-for="s in statusOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
          </select>
          <select v-model="filterKhoa" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả khoa</option>
            <option v-for="k in khoaOptions" :key="k.ma" :value="k.ma">{{ k.ten }}</option>
          </select>
          <button v-if="filterTrangThai || filterKhoa || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm lớp</GlassButton>
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
        <EmptyState title="Không tìm thấy lớp nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm lớp</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold">Mã lớp</th>
              <th class="px-3 py-4 font-semibold">Tên lớp</th>
              <th class="px-3 py-4 font-semibold">Khoa</th>
              <th class="px-3 py-4 font-semibold">Khóa</th>
              <th class="px-3 py-4 font-semibold text-center">Sĩ số</th>
              <th class="px-3 py-4 font-semibold">GVCN</th>
              <th class="px-3 py-4 font-semibold">Năm NH</th>
              <th class="px-3 py-4 font-semibold">Trạng thái</th>
              <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="r in filteredRows" :key="r.maLop" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">{{ r.maCodeLop }}</td>
              <td class="px-3 py-3.5 font-bold text-heading max-w-[200px] truncate">{{ r.tenLop }}</td>
              <td class="px-3 py-3.5 text-body">{{ r.tenKhoa }}</td>
              <td class="px-3 py-3.5 text-body">{{ r.khoaHoc }}</td>
              <td class="px-3 py-3.5 text-center"><span :class="getSiSoClass(r)">{{ r.siSo }}/{{ r.siSoToiDa }}</span></td>
              <td class="px-3 py-3.5 text-body">{{ r.tenGiaoVien || '—' }}</td>
              <td class="px-3 py-3.5 text-muted">{{ r.namNhapHoc }}</td>
              <td class="px-3 py-3.5">
                <GlassBadge :variant="(trangThaiBadge[r.trangThai] || trangThaiBadge.moi).variant" size="sm">
                  {{ (trangThaiBadge[r.trangThai] || trangThaiBadge.moi).label }}
                </GlassBadge>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Sửa" @click.stop="openEdit(r)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Xóa" @click.stop="requestDelete(r)">
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
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm lớp mới' : 'Chỉnh sửa lớp' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã lớp <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model="formData.maCodeLop" type="text" placeholder="VD: SE1601" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maCodeLop ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.maCodeLop" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maCodeLop }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Khóa</label>
                  <input v-model="formData.maKhoaHoc" type="number" min="2020" max="2030" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tên lớp <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.tenLop" type="text" placeholder="VD: Kỹ thuật phần mềm 1601" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.tenLop ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.tenLop" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.tenLop }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Khoa</label>
                  <select v-model="formData.maKhoa" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option v-for="k in khoaOptions" :key="k.ma" :value="k.ma">{{ k.ten }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Năm NH</label>
                  <input v-model="formData.namNhapHoc" type="text" placeholder="VD: 2024" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Sĩ số tối đa <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model="formData.siSoToiDa" type="number" min="1" max="200" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.siSoToiDa ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.siSoToiDa" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.siSoToiDa }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Trạng thái</label>
                  <select v-model="formData.trangThai" class="w-full h-9 px-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option v-for="s in statusOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
                  </select>
                </div>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">GVCN</label>
                <input v-model="formData.tenGiaoVien" type="text" placeholder="Tên GVCN" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ghi chú</label>
                <textarea v-model="formData.ghiChu" rows="2" placeholder="Ghi chú (nếu có)" class="w-full px-3 py-2 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) resize-none" />
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm lớp' : 'Lưu thay đổi' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <ConfirmActionDialog v-if="confirmDelete" :show="true" title="Xóa lớp?"
      :message="`Lớp &quot;${confirmDelete.maCodeLop} - ${confirmDelete.tenLop}&quot; sẽ bị xóa vĩnh viễn.`"
      confirm-label="Xóa" variant="danger" @confirm="executeDelete" @cancel="confirmDelete = null" />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
