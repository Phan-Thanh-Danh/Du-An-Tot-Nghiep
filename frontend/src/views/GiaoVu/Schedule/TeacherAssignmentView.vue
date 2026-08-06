<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Trash2, Users, UserCheck, UserMinus, UserPlus, MapPin, Clock } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { assignmentApi } from '@/services/assignmentApi'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()
const loading = ref(true); const error = ref(null); const rows = ref([])
const searchQuery = ref(''); const filterTrangThai = ref(''); const filterDonVi = ref('')

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
const formData = ref({ maKhoaHoc: null, thuTrongTuan: 1, maCaHoc: null, maPhong: null, ngayBatDau: '', ngayKetThuc: '', trangThai: 'nhap' })
const formErrors = ref({})

const showAssignModal = ref(false); const assignItem = ref(null); const assignTeacherId = ref(null); const teachers = ref([]); const assigning = ref(false)

const showDeleteModal = ref(false); const itemToDelete = ref(null); const deleting = ref(false)

const donViOptions = ref([])
const courseOptions = ref([]); const caHocOptions = ref([]); const roomOptions = ref([])

const currentPage = ref(1); const pageSize = 10

const thuOptions = [1, 2, 3, 4, 5, 6, 7]

onMounted(async () => {
  await fetchData()
  await Promise.allSettled([
    assignmentApi.getDonViOptions().then(list => { donViOptions.value = list }),
    assignmentApi.getCourses().then(list => { courseOptions.value = list }),
    assignmentApi.getCaHocs().then(list => { caHocOptions.value = list }),
    assignmentApi.getRooms().then(list => { roomOptions.value = list }),
  ])
})

async function fetchData() {
  loading.value = true; error.value = null
  try {
    const data = await assignmentApi.list({ TrangThai: filterTrangThai.value || undefined })
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
  if (filterDonVi.value) list = list.filter(r => r.donVi === filterDonVi.value)
  return list
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredRows.value.length / pageSize)))
const paginatedRows = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredRows.value.slice(start, start + pageSize)
})

const summaryCards = computed(() => [
  { label: 'Tổng buổi dạy', value: rows.value.length, color: 'text-(--lg-primary)', icon: Users },
  { label: 'Đã phân công', value: rows.value.filter(r => r.trangThai === 'da_xuat_ban').length, color: 'text-(--color-success-text)', icon: UserCheck },
  { label: 'Bản nháp', value: rows.value.filter(r => r.trangThai === 'nhap').length, color: 'text-(--color-warning-text)', icon: Clock },
  { label: 'Đã hủy', value: rows.value.filter(r => r.trangThai === 'da_huy').length, color: 'text-(--color-danger-text)', icon: Trash2 },
])

const bgMap = { 'Tổng buổi dạy': 'bg-(--color-info-bg)', 'Đã phân công': 'bg-(--color-success-bg)', 'Bản nháp': 'bg-(--color-warning-bg)', 'Đã hủy': 'bg-(--color-danger-bg)' }

const badgeVariantMap = { nhap: 'warning', da_xuat_ban: 'success', da_huy: 'danger' }
const trangThaiLabelMap = { nhap: 'Bản nháp', da_xuat_ban: 'Đã phân công', da_huy: 'Đã hủy' }

function clearFilters() { searchQuery.value = ''; filterTrangThai.value = ''; filterDonVi.value = ''; currentPage.value = 1 }

// ── Form ──
const defaults = () => ({ maKhoaHoc: null, thuTrongTuan: 1, maCaHoc: null, maPhong: null, ngayBatDau: '', ngayKetThuc: '', trangThai: 'nhap' })
function resetForm() { formData.value = defaults(); formErrors.value = {} }
function openCreate() {
  resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true
}
function openEdit(r) {
  formMode.value = 'edit'; editingId.value = r.maPhanCong
  formData.value = {
    maKhoaHoc: r.maKhoaHoc || null,
    thuTrongTuan: r.thuTrongTuan || 1,
    maCaHoc: r.maCaHoc || null,
    maPhong: r.maPhong || null,
    ngayBatDau: r.ngayBatDau || '',
    ngayKetThuc: r.ngayKetThuc || '',
    trangThai: r.trangThai === 'da_huy' ? 'da_huy' : r.trangThai || 'nhap',
  }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.maKhoaHoc) e.maKhoaHoc = 'Vui lòng chọn khóa học'
  if (!formData.value.maCaHoc) e.maCaHoc = 'Vui lòng chọn ca học'
  if (!formData.value.maPhong) e.maPhong = 'Vui lòng chọn phòng'
  formErrors.value = e; return Object.keys(e).length === 0
}
async function submitForm() {
  if (!validate()) return; submitting.value = true
  const payload = {
    MaKhoaHoc: formData.value.maKhoaHoc,
    ThuTrongTuan: formData.value.thuTrongTuan,
    MaCaHoc: formData.value.maCaHoc,
    MaPhong: formData.value.maPhong,
    NgayBatDau: formData.value.ngayBatDau || null,
    NgayKetThuc: formData.value.ngayKetThuc || null,
  }
  try {
    if (formMode.value === 'edit') {
      payload.TrangThai = formData.value.trangThai || 'nhap'
      await assignmentApi.update(editingId.value, payload)
    } else {
      await assignmentApi.create(payload)
    }
    closeForm(); await fetchData(); popupStore.success('Thành công', formMode.value === 'edit' ? 'Đã cập nhật phân công' : 'Đã thêm phân công mới')
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu' }
  finally { submitting.value = false }
}

// ── Assign Teacher ──
async function openAssign(r) {
  assignItem.value = r; assignTeacherId.value = r.maGiangVien || null
  teachers.value = []
  try {
    const data = await assignmentApi.getTeachers({
      maMonHoc: r.maMonHoc,
      maHocKy: r.maHocKy,
      maLop: r.maLop,
    })
    teachers.value = Array.isArray(data) ? data : data?.items || data?.data || []
  } catch (e) { popupStore.info('Chưa có gợi ý', e.message || 'Không thể tải danh sách giảng viên') }
  showAssignModal.value = true
}
function closeAssign() { showAssignModal.value = false; assignItem.value = null; assignTeacherId.value = null }
async function confirmAssign() {
  if (!assignTeacherId.value) return; assigning.value = true
  try {
    await assignmentApi.assignTeacher(assignItem.value.maPhanCong, assignTeacherId.value, {
      maKhoaHoc: assignItem.value.maKhoaHoc,
    })
    closeAssign(); await fetchData(); popupStore.success('Thành công', 'Đã phân công giảng viên')
  } catch (e) { popupStore.error('Lỗi', e.message || 'Không thể phân công') }
  finally { assigning.value = false }
}

// ── Cancel (Hủy lịch) ──
function requestDelete(r) {
  itemToDelete.value = r; showDeleteModal.value = true
}
async function confirmDelete() {
  deleting.value = true
  try {
    await assignmentApi.remove(itemToDelete.value.maPhanCong)
    showDeleteModal.value = false; itemToDelete.value = null; await fetchData(); popupStore.success('Thành công', 'Đã hủy buổi dạy')
  } catch (e) { popupStore.error('Lỗi', e.message || 'Không thể hủy') }
  finally { deleting.value = false }
}

function selectedCourseLabel(maKhoaHoc) {
  const c = courseOptions.value.find(x => x.maKhoaHoc === maKhoaHoc)
  return c ? `${c.tenLop} — ${c.monHoc}` : ''
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
            <option value="nhap">Bản nháp</option>
            <option value="da_xuat_ban">Đã phân công</option>
            <option value="da_huy">Đã hủy</option>
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
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate"><Plus :size="15" class="mr-1" /> Lịch dạy mới</GlassButton>
          </div>
        </div>
      </div>

      <div v-if="loading" class="p-6"><SkeletonTable :rows="6" :columns="6" /></div>
      <div v-else-if="error" class="p-6">
        <div class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
          <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
          <p class="text-xs text-muted">{{ error }}</p>
          <GlassButton variant="primary" class="px-4 py-2 text-xs font-bold rounded-xl mt-2" @click="fetchData">Thử lại</GlassButton>
        </div>
      </div>
      <div v-else-if="filteredRows.length === 0" class="p-6">
        <EmptyState title="Không có lịch dạy nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Lịch dạy mới</GlassButton>
        </EmptyState>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold">Mã TKB</th>
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
                <div class="flex items-center gap-2" :class="r.maGiangVien ? 'text-heading' : 'text-red-500'">
                  <UserCheck v-if="r.maGiangVien" :size="15" class="text-emerald-500 shrink-0" />
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
                <GlassBadge :variant="badgeVariantMap[r.trangThai] || 'neutral'" size="sm">
                  {{ trangThaiLabelMap[r.trangThai] || r.trangThai }}
                </GlassBadge>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button v-if="r.trangThai !== 'da_huy'" class="h-8 w-8 rounded-lg hover:bg-(--color-success-bg) flex items-center justify-center text-muted hover:text-(--color-success-text) transition-colors" title="Phân công giảng viên" @click.stop="openAssign(r)">
                    <UserPlus :size="15" />
                  </button>
                  <button v-if="r.trangThai !== 'da_huy'" class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Sửa" @click.stop="openEdit(r)">
                    <Pencil :size="15" />
                  </button>
                  <button v-if="r.trangThai !== 'da_huy'" class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Hủy" @click.stop="requestDelete(r)">
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
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Lịch dạy mới' : 'Sửa lịch dạy' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Khóa học <span class="text-(--color-danger-text)">*</span></label>
                <select v-model.number="formData.maKhoaHoc" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maKhoaHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''">
                  <option :value="null" disabled>Chọn khóa học</option>
                  <option v-for="c in courseOptions" :key="c.maKhoaHoc" :value="c.maKhoaHoc">{{ c.tenLop }} — {{ c.monHoc }}</option>
                </select>
                <p v-if="formErrors.maKhoaHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maKhoaHoc }}</p>
                <p v-if="formData.maKhoaHoc && selectedCourseLabel(formData.maKhoaHoc)" class="mt-1 text-xs text-muted">GV: {{ courseOptions.find(c => c.maKhoaHoc === formData.maKhoaHoc)?.giangVien }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ <span class="text-(--color-danger-text)">*</span></label>
                  <select v-model.number="formData.thuTrongTuan" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                    <option v-for="t in thuOptions" :key="t" :value="t">Thứ {{ t }}</option>
                  </select>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ca học <span class="text-(--color-danger-text)">*</span></label>
                  <select v-model.number="formData.maCaHoc" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maCaHoc ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''">
                    <option :value="null" disabled>Chọn ca</option>
                    <option v-for="c in caHocOptions" :key="c.maCaHoc" :value="c.maCaHoc">{{ c.tenCa }} ({{ c.gioBatDau || '' }}–{{ c.gioKetThuc || '' }})</option>
                  </select>
                  <p v-if="formErrors.maCaHoc" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maCaHoc }}</p>
                </div>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Phòng <span class="text-(--color-danger-text)">*</span></label>
                <select v-model.number="formData.maPhong" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" :class="formErrors.maPhong ? 'border-(--color-danger-text) bg-(--color-danger-bg)' : ''">
                  <option :value="null" disabled>Chọn phòng</option>
                  <option v-for="p in roomOptions" :key="p.maPhong" :value="p.maPhong">{{ p.maCodePhong }} — {{ p.tenPhong }}</option>
                </select>
                <p v-if="formErrors.maPhong" class="mt-1 text-xs text-(--color-danger-text) font-semibold">{{ formErrors.maPhong }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày bắt đầu</label>
                  <input v-model="formData.ngayBatDau" type="date" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày kết thúc</label>
                  <input v-model="formData.ngayKetThuc" type="date" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
                </div>
              </div>
              <div v-if="formMode === 'edit'">
                <label class="block text-xs font-semibold text-(--text-muted) mb-1">Trạng thái</label>
                <select v-model="formData.trangThai" class="w-full h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
                  <option value="nhap">Bản nháp</option>
                  <option value="da_xuat_ban">Đã phân công</option>
                </select>
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
                <p class="text-xs text-muted">Lịch: {{ assignItem?.lichDay }} · Phòng: {{ assignItem?.phong }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-(--text-muted) mb-2">Chọn giảng viên</label>
                <div class="space-y-2 max-h-[240px] overflow-y-auto">
                  <label v-for="t in teachers" :key="t.maGiangVien" class="flex items-center gap-3 p-3 rounded-xl border border-card surface-card cursor-pointer hover:bg-(--surface-hover) transition-colors"
                    :class="assignTeacherId === t.maGiangVien ? 'ring-2 ring-(--lg-primary)' : ''">
                    <input type="radio" :value="t.maGiangVien" v-model="assignTeacherId" class="accent-(--lg-primary)" />
                    <div class="flex-1 min-w-0">
                      <p class="text-sm font-bold text-heading truncate">{{ t.hoTen }}</p>
                      <p class="text-xs text-muted">{{ t.chuyenNganh || '—' }}</p>
                    </div>
                    <span v-if="!t.isEligible" class="text-[10px] font-bold text-(--color-danger-text) shrink-0">Không đủ điều kiện</span>
                  </label>
                  <p v-if="teachers.length === 0" class="text-xs text-muted text-center py-4">Không có giảng viên phù hợp.</p>
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

    <!-- Cancel Dialog -->
    <ConfirmActionDialog
      v-model="showDeleteModal"
      title="Hủy buổi dạy"
      :message="`Bạn có chắc muốn hủy buổi dạy ${itemToDelete?.tenLop} - ${itemToDelete?.monHoc} (${itemToDelete?.lichDay})?`"
      confirmLabel="Hủy buổi dạy"
      :loading="deleting"
      @confirm="confirmDelete"
    />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>