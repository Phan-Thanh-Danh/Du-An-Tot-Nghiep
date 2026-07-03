<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Trash2, Users, UserCheck, UserMinus, UserPlus, MapPin, Clock, Shield } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { assignmentApi } from '@/services/assignmentApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const loading = ref(true); const error = ref(null); const rows = ref([])
const searchQuery = ref(''); const filterTrangThai = ref(''); const filterDonVi = ref('')

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ tenLop: '', monHoc: '', maGiangVien: null, giangVien: 'Chưa phân công', soBuoi: 3, donVi: '', lichDay: '', phong: '', siSo: 0, trangThai: 'unassigned' })
const formErrors = ref({})

const showAssignModal = ref(false); const assignItem = ref(null); const assignTeacherId = ref(null); const teachers = ref([]); const assigning = ref(false)

const showDeleteModal = ref(false); const itemToDelete = ref(null); const deleting = ref(false)

const donViOptions = ref([])

const currentPage = ref(1); const pageSize = 10

onMounted(async () => {
  await fetchData()
  donViOptions.value = assignmentApi.getDonViOptions()
})

async function fetchData() {
  loading.value = true; error.value = null
  try {
    const data = await assignmentApi.list({ TrangThai: filterTrangThai.value || undefined, DonVi: filterDonVi.value || undefined })
    rows.value = Array.isArray(data) ? data : data?.items || data?.data || []
  } catch (e) { error.value = e.message || 'Không thể tải dữ liệu phân công' }
  finally { loading.value = false }
}

const filteredRows = computed(() => {
  let list = rows.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(r => r.monHoc?.toLowerCase().includes(q) || r.tenLop?.toLowerCase().includes(q) || r.giangVien?.toLowerCase().includes(q))
  }
  return list
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / pageSize)))
const paginatedRows = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredRows.value.slice(start, start + pageSize)
})

const summaryCards = computed(() => [
  { label: 'Tổng lớp HP', value: rows.value.length, color: 'text-(--lg-primary)', icon: Users },
  { label: 'Đã phân công', value: rows.value.filter(r => r.trangThai === 'assigned').length, color: 'text-(--color-success-text)', icon: UserCheck },
  { label: 'Chưa phân công', value: rows.value.filter(r => r.trangThai !== 'assigned').length, color: 'text-(--color-danger-text)', icon: UserMinus },
  { label: 'Giảng viên', value: new Set(rows.value.filter(r => r.maGiangVien).map(r => r.maGiangVien)).size, color: 'text-amber-600', icon: Users },
])

const bgMap = { 'Tổng lớp HP': 'bg-(--color-info-bg)', 'Đã phân công': 'bg-(--color-success-bg)', 'Chưa phân công': 'bg-(--color-danger-bg)', 'Giảng viên': 'bg-amber-50' }

function clearFilters() { searchQuery.value = ''; filterTrangThai.value = ''; filterDonVi.value = ''; currentPage.value = 1 }

// ── Form ──
const defaults = () => ({ tenLop: '', monHoc: '', maGiangVien: null, giangVien: 'Chưa phân công', soBuoi: 3, donVi: '', lichDay: '', phong: '', siSo: 0, trangThai: 'unassigned' })
function resetForm() { formData.value = defaults(); formErrors.value = {} }
function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(r) {
  formMode.value = 'edit'; editingId.value = r.maPhanCong
  formData.value = { tenLop: r.tenLop, monHoc: r.monHoc, maGiangVien: r.maGiangVien, giangVien: r.giangVien, soBuoi: r.soBuoi, donVi: r.donVi || '', lichDay: r.lichDay || '', phong: r.phong || '', siSo: r.siSo || 0, trangThai: r.trangThai }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.tenLop.trim()) e.tenLop = 'Tên lớp không được để trống'
  if (!formData.value.monHoc.trim()) e.monHoc = 'Môn học không được để trống'
  formErrors.value = e; return Object.keys(e).length === 0
}
async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    if (formMode.value === 'edit') await assignmentApi.update(editingId.value, formData.value)
    else await assignmentApi.create(formData.value)
    closeForm(); await fetchData(); popupStore.success('Thành công', formMode.value === 'edit' ? 'Đã cập nhật phân công' : 'Đã thêm phân công mới')
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu' }
  finally { submitting.value = false }
}

// ── Assign Teacher ──
async function openAssign(r) {
  assignItem.value = r; assignTeacherId.value = r.maGiangVien || null
  try {
    const data = await assignmentApi.getTeachers({ DonVi: r.donVi || undefined })
    teachers.value = Array.isArray(data) ? data : data?.items || data?.data || []
  } catch { teachers.value = [] }
  showAssignModal.value = true
}
function closeAssign() { showAssignModal.value = false; assignItem.value = null; assignTeacherId.value = null }
async function confirmAssign() {
  if (!assignTeacherId.value) return; assigning.value = true
  try {
    await assignmentApi.assignTeacher(assignItem.value.maPhanCong, assignTeacherId.value)
    closeAssign(); await fetchData(); popupStore.success('Thành công', 'Đã phân công giảng viên')
  } catch (e) { popupStore.error('Lỗi', e.message || 'Không thể phân công') }
  finally { assigning.value = false }
}

// ── Delete ──
function requestDelete(r) { itemToDelete.value = r; showDeleteModal.value = true }
async function confirmDelete() {
  deleting.value = true
  try {
    await assignmentApi.remove(itemToDelete.value.maPhanCong)
    showDeleteModal.value = false; itemToDelete.value = null; await fetchData(); popupStore.success('Thành công', 'Đã xóa phân công')
  } catch (e) { popupStore.error('Lỗi', e.message || 'Không thể xóa') }
  finally { deleting.value = false }
}
</script>

<template>
  <div class="space-y-4">

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="c in summaryCards" :key="c.label" class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">{{ c.label }}</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ c.value }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl flex items-center justify-center" :class="bgMap[c.label]">
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
            <input v-model="searchQuery" type="text" placeholder="Tìm môn, lớp, giảng viên..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" @keydown.enter="fetchData" />
          </div>
          <select v-model="filterTrangThai" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả trạng thái</option>
            <option value="assigned">Đã phân công</option>
            <option value="unassigned">Chưa phân công</option>
          </select>
          <select v-model="filterDonVi" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="">Tất cả đơn vị</option>
            <option v-for="dv in donViOptions" :key="dv" :value="dv">{{ dv }}</option>
          </select>
          <button v-if="filterTrangThai || filterDonVi || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate"><Plus :size="15" class="mr-1" /> Phân công mới</GlassButton>
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
        <EmptyState title="Không có phân công nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Phân công mới</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold">Mã PC</th>
              <th class="px-3 py-4 font-semibold">Lớp & Môn</th>
              <th class="px-3 py-4 font-semibold">Giảng viên</th>
              <th class="px-3 py-4 font-semibold">Lịch dạy</th>
              <th class="px-3 py-4 font-semibold">Đơn vị</th>
              <th class="px-3 py-4 font-semibold text-center">Trạng thái</th>
              <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="r in paginatedRows" :key="r.maPhanCong" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">PC{{ String(r.maPhanCong).padStart(3, '0') }}</td>
              <td class="px-3 py-3.5">
                <p class="font-bold text-heading">{{ r.tenLop }}</p>
                <p class="text-xs text-muted mt-0.5">{{ r.monHoc }}</p>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center gap-2" :class="r.trangThai !== 'assigned' ? 'text-red-500' : 'text-heading'">
                  <UserCheck v-if="r.trangThai === 'assigned'" :size="15" class="text-emerald-500 shrink-0" />
                  <UserMinus v-else :size="15" class="text-red-500 shrink-0" />
                  <span class="font-bold text-sm truncate max-w-[160px]">{{ r.giangVien }}</span>
                </div>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center gap-1.5 text-xs text-body">
                  <Clock :size="13" class="text-muted shrink-0" /> {{ r.lichDay || '—' }}
                </div>
                <div class="flex items-center gap-1.5 text-xs text-body mt-1">
                  <MapPin :size="13" class="text-muted shrink-0" /> {{ r.phong || '—' }}
                </div>
              </td>
              <td class="px-3 py-3.5 text-body">{{ r.donVi || '—' }}</td>
              <td class="px-3 py-3.5 text-center">
                <GlassBadge :variant="r.trangThai === 'assigned' ? 'success' : 'danger'" size="sm">
                  {{ r.trangThai === 'assigned' ? 'Đã phân công' : 'Chưa phân công' }}
                </GlassBadge>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button v-if="r.trangThai !== 'assigned'" class="h-8 w-8 rounded-lg hover:bg-(--color-success-bg) flex items-center justify-center text-muted hover:text-(--color-success-text) transition-colors" title="Phân công giảng viên" @click.stop="openAssign(r)">
                    <UserPlus :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Sửa" @click.stop="openEdit(r)">
                    <Pencil :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Xóa" @click.stop="requestDelete(r)">
                    <Trash2 :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="filteredRows.length > 0" class="border-t border-default px-4 py-3 flex items-center justify-between bg-(--surface-input)">
        <p class="text-xs text-muted">{{ filteredRows.length }} kết quả</p>
        <div class="flex items-center gap-1.5">
          <button :disabled="currentPage <= 1" class="h-7 px-2.5 rounded-lg text-xs font-bold border border-default bg-(--surface-card) text-muted disabled:opacity-30 hover:bg-(--surface-hover) transition-colors" @click="currentPage--">Trước</button>
          <span class="px-2 text-xs font-bold text-heading">{{ currentPage }} / {{ totalPages }}</span>
          <button :disabled="currentPage >= totalPages" class="h-7 px-2.5 rounded-lg text-xs font-bold border border-default bg-(--surface-card) text-muted disabled:opacity-30 hover:bg-(--surface-hover) transition-colors" @click="currentPage++">Sau</button>
        </div>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Phân công mới' : 'Sửa phân công' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Lớp <span class="text-(--color-danger-text)">*</span></label>
                  <input v-model="formData.tenLop" type="text" placeholder="VD: SE1601" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.tenLop ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                  <p v-if="formErrors.tenLop" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.tenLop }}</p>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Sĩ số</label>
                  <input v-model.number="formData.siSo" type="number" min="0" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Môn học <span class="text-(--color-danger-text)">*</span></label>
                <input v-model="formData.monHoc" type="text" placeholder="VD: Lập trình Java" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.monHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''" />
                <p v-if="formErrors.monHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.monHoc }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số buổi</label>
                  <input v-model.number="formData.soBuoi" type="number" min="1" max="30" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Đơn vị</label>
                  <input v-model="formData.donVi" type="text" placeholder="VD: CNTT" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Lịch dạy</label>
                <input v-model="formData.lichDay" type="text" placeholder="VD: T2 (Ca 1-2), T4 (Ca 3)" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phòng</label>
                <input v-model="formData.phong" type="text" placeholder="VD: Lab 102" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">{{ submitting ? 'Đang lưu...' : formMode === 'create' ? 'Thêm' : 'Lưu thay đổi' }}</GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <!-- Assign Teacher Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showAssignModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeAssign">
          <div class="w-full max-w-md lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">Phân công giảng viên</h3>
              <button @click="closeAssign" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 space-y-4">
              <div class="surface-card border border-card rounded-xl p-3 text-sm space-y-1">
                <p class="text-heading font-bold">{{ assignItem?.tenLop }} — {{ assignItem?.monHoc }}</p>
                <p class="text-xs text-muted">Sĩ số: {{ assignItem?.siSo }} · Phòng: {{ assignItem?.phong }} · {{ assignItem?.lichDay }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-2">Chọn giảng viên</label>
                <div class="space-y-2 max-h-[240px] overflow-y-auto">
                  <label v-for="t in teachers" :key="t.maGiangVien" class="flex items-center gap-3 p-3 rounded-xl border border-card surface-card cursor-pointer hover:bg-(--surface-hover) transition-colors"
                    :class="assignTeacherId === t.maGiangVien ? 'ring-2 ring-(--lg-primary)' : ''">
                    <input type="radio" :value="t.maGiangVien" v-model="assignTeacherId" class="accent-(--lg-primary)" />
                    <div class="flex-1 min-w-0">
                      <p class="text-sm font-bold text-heading truncate">{{ t.hoTen }}</p>
                      <p class="text-xs text-muted">{{ t.donVi }} · Đã dạy {{ t.soTietDaDay }}/{{ t.tietToiDa }} tiết</p>
                    </div>
                    <span v-if="t.soTietDaDay >= t.tietToiDa" class="text-[10px] font-bold text-(--color-danger-text) shrink-0">Quá tải</span>
                  </label>
                  <p v-if="teachers.length === 0" class="text-xs text-muted text-center py-4">Không có giảng viên nào.</p>
                </div>
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeAssign">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="!assignTeacherId || assigning" @click="confirmAssign">{{ assigning ? 'Đang phân công...' : 'Xác nhận' }}</GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <!-- Delete Dialog -->
    <ConfirmActionDialog
      v-model="showDeleteModal"
      title="Xóa phân công"
      :message="`Bạn có chắc muốn xóa phân công cho lớp ${itemToDelete?.tenLop} - ${itemToDelete?.monHoc}?`"
      confirmLabel="Xóa"
      :loading="deleting"
      @confirm="confirmDelete"
    />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
