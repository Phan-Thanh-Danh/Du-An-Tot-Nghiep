<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { cohortApi } from '@/services/cohortApi'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { Search, Plus, Edit2, CheckCircle2, XCircle, Loader2, RefreshCw, Unlock, Lock } from 'lucide-vue-next'

const popup = usePopupStore()

const cohorts = ref([])
const loading = ref(false)
const error = ref('')
const searchQuery = ref('')
const yearFilter = ref('')
const statusFilter = ref('all')
const pageIndex = ref(1)
const pageSize = ref(20)
const totalItems = ref(0)
const totalPages = ref(1)

const isFormOpen = ref(false)
const formMode = ref('create')
const editingId = ref(null)
const cohortForm = ref({
  maCodeKhoa: '',
  tenKhoa: '',
  namBatDau: new Date().getFullYear(),
  namKetThucDuKien: '',
  moTa: '',
  conHoatDong: true
})

const pendingAction = ref(null)

const statusOptions = [
  { value: 'all', label: 'Tất cả trạng thái' },
  { value: 'active', label: 'Đang hoạt động' },
  { value: 'inactive', label: 'Ngừng hoạt động' }
]

const filteredYears = computed(() => {
  const years = new Set(cohorts.value.map(c => c.namBatDau))
  return Array.from(years).sort((a, b) => b - a)
})

const statusIndicator = (cohort) => cohort.conHoatDong ? 'Đang hoạt động' : 'Ngừng hoạt động'

const formatYear = (value) => value ?? '—'

async function fetchCohorts() {
  loading.value = true
  error.value = ''

  try {
    const response = await cohortApi.list({
      pageIndex: pageIndex.value,
      pageSize: pageSize.value,
      keyword: searchQuery.value || undefined,
      namBatDau: yearFilter.value ? Number(yearFilter.value) : undefined,
      conHoatDong: statusFilter.value === 'all' ? undefined : statusFilter.value === 'active'
    })

    const page = cohortApi.unwrapPagedResult(response)
    cohorts.value = page.items
    totalItems.value = page.totalItems
    totalPages.value = page.pageIndex && page.pageSize ? Math.max(1, Math.ceil(page.totalItems / page.pageSize)) : 1
  } catch (e) {
    error.value = e?.message || 'Không thể tải danh sách khóa tuyển sinh.'
    cohorts.value = []
  } finally {
    loading.value = false
  }
}

watch([searchQuery, yearFilter, statusFilter, pageIndex, pageSize], () => {
  pageIndex.value = Math.max(1, pageIndex.value)
  fetchCohorts()
})

function openCreate() {
  formMode.value = 'create'
  editingId.value = null
  cohortForm.value = {
    maCodeKhoa: '',
    tenKhoa: '',
    namBatDau: new Date().getFullYear(),
    namKetThucDuKien: '',
    moTa: '',
    conHoatDong: true
  }
  isFormOpen.value = true
}

function openEdit(cohort) {
  formMode.value = 'edit'
  editingId.value = cohort.maKhoaTuyenSinh
  cohortForm.value = {
    maCodeKhoa: cohort.maCodeKhoa || cohort.MaCodeKhoa || '',
    tenKhoa: cohort.tenKhoa || cohort.TenKhoa || '',
    namBatDau: cohort.namBatDau || cohort.NamBatDau || new Date().getFullYear(),
    namKetThucDuKien: cohort.namKetThucDuKien ?? cohort.NamKetThucDuKien ?? '',
    moTa: cohort.moTa || cohort.MoTa || '',
    conHoatDong: cohort.conHoatDong !== false
  }
  isFormOpen.value = true
}

function closeForm() {
  isFormOpen.value = false
}

function validateForm() {
  if (!cohortForm.value.maCodeKhoa.trim()) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập mã khóa tuyển sinh.')
    return false
  }
  if (!cohortForm.value.tenKhoa.trim()) {
    popup.warning('Thiếu thông tin', 'Vui lòng nhập tên khóa tuyển sinh.')
    return false
  }
  if (!cohortForm.value.namBatDau || Number(cohortForm.value.namBatDau) < 2000) {
    popup.warning('Thiếu thông tin', 'Năm bắt đầu phải lớn hơn hoặc bằng 2000.')
    return false
  }
  if (cohortForm.value.namKetThucDuKien && Number(cohortForm.value.namKetThucDuKien) < Number(cohortForm.value.namBatDau)) {
    popup.warning('Lỗi dữ liệu', 'Năm kết thúc dự kiến phải lớn hơn hoặc bằng năm bắt đầu.')
    return false
  }
  return true
}

async function submitForm() {
  if (!validateForm()) return

  const payload = {
    maCodeKhoa: cohortForm.value.maCodeKhoa.trim(),
    tenKhoa: cohortForm.value.tenKhoa.trim(),
    namBatDau: Number(cohortForm.value.namBatDau),
    namKetThucDuKien: cohortForm.value.namKetThucDuKien ? Number(cohortForm.value.namKetThucDuKien) : undefined,
    moTa: cohortForm.value.moTa.trim() || undefined,
    conHoatDong: cohortForm.value.conHoatDong
  }

  try {
    if (formMode.value === 'edit' && editingId.value != null) {
      await cohortApi.update(editingId.value, payload)
      popup.success('Cập nhật thành công', 'Thông tin khóa tuyển sinh đã được lưu.')
    } else {
      await cohortApi.create(payload)
      popup.success('Tạo mới thành công', 'Khóa tuyển sinh mới đã được thêm.')
    }
    closeForm()
    await fetchCohorts()
  } catch (e) {
    popup.error('Lỗi', e?.message || 'Không thể lưu khóa tuyển sinh. Vui lòng thử lại.')
  }
}

function confirmDeactivate(cohort) {
  pendingAction.value = cohort
}

async function executeDeactivate() {
  if (!pendingAction.value) return

  try {
    await cohortApi.deactivate(pendingAction.value.maKhoaTuyenSinh)
    popup.success('Đã ngừng hoạt động', `Khóa tuyển sinh ${pendingAction.value.tenKhoa || pendingAction.value.TenKhoa} đã được ngừng.`)
    pendingAction.value = null
    await fetchCohorts()
  } catch (e) {
    popup.error('Lỗi', e?.message || 'Không thể ngừng hoạt động khóa tuyển sinh.')
  }
}

async function toggleActive(cohort) {
  try {
    if (cohort.conHoatDong) {
      await cohortApi.deactivate(cohort.maKhoaTuyenSinh)
      popup.success('Đã ngừng hoạt động', `Khóa tuyển sinh ${cohort.tenKhoa || cohort.TenKhoa} đã được ngừng.`)
    } else {
      await cohortApi.activate(cohort.maKhoaTuyenSinh)
      popup.success('Đã kích hoạt', `Khóa tuyển sinh ${cohort.tenKhoa || cohort.TenKhoa} đã được kích hoạt.`)
    }
    await fetchCohorts()
  } catch (e) {
    popup.error('Lỗi', e?.message || 'Không thể cập nhật trạng thái khóa tuyển sinh.')
  }
}

const pagedCohorts = computed(() => cohorts.value)

function resetFilters() {
  searchQuery.value = ''
  yearFilter.value = ''
  statusFilter.value = 'all'
}

onMounted(fetchCohorts)
</script>

<template>
  <div class="space-y-6 pb-10">
    <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 class="text-2xl font-bold text-heading">Quản lý khóa tuyển sinh</h1>
          <p class="text-sm text-label max-w-2xl mt-1">
            Tạo, sửa, kích hoạt hoặc ngừng hoạt động các khóa tuyển sinh. Dữ liệu này được sử dụng để gán chương trình đào tạo và phân theo hệ khóa.
          </p>
        </div>
        <button
          @click="openCreate"
          class="inline-flex items-center gap-2 rounded-xl bg-(--lg-primary) px-4 py-2 text-xs font-semibold text-white hover:bg-(--lg-primary-dark) transition-colors"
        >
          <Plus size="16" /> Tạo khóa tuyển sinh
        </button>
      </div>
    </div>

    <div class="grid gap-4 md:grid-cols-[1.6fr_0.8fr]">
      <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
        <div class="p-4 border-b border-default bg-(--surface-input)">
          <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
            <div class="flex flex-wrap gap-3 items-center">
              <div class="relative min-w-[240px]">
                <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" size="16" />
                <input
                  v-model="searchQuery"
                  @input="pageIndex = 1"
                  type="text"
                  placeholder="Tìm mã hoặc tên khóa tuyển sinh..."
                  class="h-11 w-full rounded-xl border border-(--border-input) bg-(--surface-card) pl-10 pr-4 text-sm outline-none focus:border-(--lg-primary) focus:ring-2 focus:ring-(--lg-primary)/20"
                />
              </div>
              <select
                v-model="yearFilter"
                @change="pageIndex = 1"
                class="h-11 rounded-xl border border-(--border-input) bg-(--surface-card) px-3 text-sm outline-none focus:border-(--lg-primary) focus:ring-2 focus:ring-(--lg-primary)/20"
              >
                <option value="">Tất cả năm bắt đầu</option>
                <option v-for="year in filteredYears" :key="year" :value="year">{{ year }}</option>
              </select>
              <select
                v-model="statusFilter"
                @change="pageIndex = 1"
                class="h-11 rounded-xl border border-(--border-input) bg-(--surface-card) px-3 text-sm outline-none focus:border-(--lg-primary) focus:ring-2 focus:ring-(--lg-primary)/20"
              >
                <option v-for="option in statusOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>
            </div>
            <button
              @click="resetFilters"
              class="h-11 rounded-xl border border-(--border-input) bg-(--surface-card) px-4 text-sm font-semibold text-heading hover:bg-(--surface-card)/90 transition"
            >
              Làm mới
            </button>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="min-w-full text-left text-sm text-body">
            <thead class="bg-(--surface-card)">
              <tr>
                <th class="px-4 py-3 font-semibold text-heading">Mã</th>
                <th class="px-4 py-3 font-semibold text-heading">Tên khóa tuyển sinh</th>
                <th class="px-4 py-3 font-semibold text-heading">Năm bắt đầu</th>
                <th class="px-4 py-3 font-semibold text-heading">Năm kết thúc dự kiến</th>
                <th class="px-4 py-3 font-semibold text-heading">Trạng thái</th>
                <th class="px-4 py-3 font-semibold text-heading">Hành động</th>
              </tr>
            </thead>
            <tbody>
              <tr v-if="loading">
                <td colspan="6" class="px-4 py-12 text-center text-muted">
                  <div class="inline-flex items-center gap-2">
                    <Loader2 size="18" class="animate-spin" /> Đang tải dữ liệu...
                  </div>
                </td>
              </tr>
              <tr v-else-if="error">
                <td colspan="6" class="px-4 py-12 text-center text-danger text-sm">
                  {{ error }}
                </td>
              </tr>
              <tr v-else-if="pagedCohorts.length === 0">
                <td colspan="6" class="px-4 py-12 text-center text-muted">
                  Không có khóa tuyển sinh phù hợp.
                </td>
              </tr>
              <tr v-else v-for="cohort in pagedCohorts" :key="cohort.maKhoaTuyenSinh" class="border-t border-default hover:bg-(--surface-input)/80 transition-colors">
                <td class="px-4 py-4 font-semibold text-heading">{{ cohort.maCodeKhoa || cohort.MaCodeKhoa }}</td>
                <td class="px-4 py-4">{{ cohort.tenKhoa || cohort.TenKhoa }}</td>
                <td class="px-4 py-4">{{ formatYear(cohort.namBatDau ?? cohort.NamBatDau) }}</td>
                <td class="px-4 py-4">{{ formatYear(cohort.namKetThucDuKien ?? cohort.NamKetThucDuKien) }}</td>
                <td class="px-4 py-4">
                  <span :class="cohort.conHoatDong ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'bg-(--color-danger-bg) text-(--color-danger-text)'" class="inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold">
                    {{ statusIndicator(cohort) }}
                  </span>
                </td>
                <td class="px-4 py-4 space-x-2">
                  <button
                    @click="openEdit(cohort)"
                    class="inline-flex items-center gap-2 rounded-xl border border-(--border-input) bg-(--surface-card) px-3 py-2 text-xs font-semibold text-heading hover:bg-(--surface-input) transition"
                  >
                    <Edit2 size="14" /> Sửa
                  </button>
                  <button
                    @click="toggleActive(cohort)"
                    class="inline-flex items-center gap-2 rounded-xl px-3 py-2 text-xs font-semibold transition"
                    :class="cohort.conHoatDong ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)'"
                  >
                    <component :is="cohort.conHoatDong ? Lock : Unlock" :size="14" />
                    {{ cohort.conHoatDong ? 'Ngừng' : 'Kích hoạt' }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="space-y-4">
          <div class="rounded-3xl bg-(--surface-card) p-4">
            <h2 class="text-lg font-semibold text-heading">Thống kê nhanh</h2>
            <div class="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div class="rounded-2xl border border-default bg-(--surface-input) p-4">
                <div class="text-xs uppercase tracking-[0.12em] text-label">Tổng số khóa</div>
                <div class="mt-2 text-3xl font-bold text-heading">{{ totalItems }}</div>
              </div>
              <div class="rounded-2xl border border-default bg-(--surface-input) p-4">
                <div class="text-xs uppercase tracking-[0.12em] text-label">Đang hoạt động</div>
                <div class="mt-2 text-3xl font-bold text-heading">{{ cohorts.filter(c => c.conHoatDong !== false).length }}</div>
              </div>
              <div class="rounded-2xl border border-default bg-(--surface-input) p-4">
                <div class="text-xs uppercase tracking-[0.12em] text-label">Ngừng hoạt động</div>
                <div class="mt-2 text-3xl font-bold text-heading">{{ cohorts.filter(c => c.conHoatDong === false).length }}</div>
              </div>
            </div>
          </div>
          <div class="rounded-3xl border border-default bg-(--surface-card) p-4">
            <h2 class="text-lg font-semibold text-heading">Hướng dẫn</h2>
            <ul class="space-y-2 text-sm text-body list-disc list-inside">
              <li>Khóa tuyển sinh dùng để phân loại chương trình đào tạo theo năm vào học.</li>
              <li>Chọn "Kích hoạt" để cho phép chương trình gán vào khóa tuyển sinh đó.</li>
              <li>Khóa tuyển sinh bị ngừng vẫn giữ lịch sử dữ liệu và có thể kích hoạt lại.</li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div v-if="isFormOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/30 backdrop-blur-sm"></div>
      <div class="relative w-full max-w-2xl rounded-3xl border border-default bg-(--surface-card) p-6 shadow-2xl">
        <div class="flex items-start justify-between gap-4">
          <div>
            <h2 class="text-xl font-bold text-heading">{{ formMode === 'create' ? 'Tạo mới khóa tuyển sinh' : 'Cập nhật khóa tuyển sinh' }}</h2>
            <p class="text-sm text-label mt-1">Nhập thông tin khóa tuyển sinh để sử dụng cho chương trình đào tạo.</p>
          </div>
          <button @click="closeForm" class="text-muted hover:text-heading">
            <XCircle size="22" />
          </button>
        </div>

        <div class="mt-6 grid gap-4 sm:grid-cols-2">
          <div>
            <label class="block text-xs font-semibold text-label mb-2">Mã khóa tuyển sinh<span class="text-rose-500">*</span></label>
            <input v-model="cohortForm.maCodeKhoa" type="text" class="glass-input w-full text-sm" placeholder="VD: K24" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-label mb-2">Tên khóa tuyển sinh<span class="text-rose-500">*</span></label>
            <input v-model="cohortForm.tenKhoa" type="text" class="glass-input w-full text-sm" placeholder="VD: Khóa 24 (2026-2029)" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-label mb-2">Năm bắt đầu<span class="text-rose-500">*</span></label>
            <input v-model.number="cohortForm.namBatDau" type="number" min="2000" class="glass-input w-full text-sm" />
          </div>
          <div>
            <label class="block text-xs font-semibold text-label mb-2">Năm kết thúc dự kiến</label>
            <input v-model.number="cohortForm.namKetThucDuKien" type="number" min="2000" class="glass-input w-full text-sm" />
          </div>
          <div class="sm:col-span-2">
            <label class="block text-xs font-semibold text-label mb-2">Mô tả</label>
            <textarea v-model="cohortForm.moTa" rows="3" class="glass-input w-full text-sm" placeholder="Ghi chú ngắn về khóa tuyển sinh"></textarea>
          </div>
          <div class="sm:col-span-2 flex items-center gap-3">
            <input id="cohort-active" type="checkbox" v-model="cohortForm.conHoatDong" class="h-4 w-4 rounded border-(--border-input) text-(--lg-primary) focus:ring-(--lg-primary)" />
            <label for="cohort-active" class="text-sm text-body">Kích hoạt khóa tuyển sinh ngay</label>
          </div>
        </div>

        <div class="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
          <button @click="closeForm" class="rounded-xl border border-(--border-input) bg-(--surface-card) px-5 py-3 text-sm font-semibold text-heading hover:bg-(--surface-input) transition">Hủy</button>
          <button @click="submitForm" class="rounded-xl bg-(--lg-primary) px-5 py-3 text-sm font-semibold text-white hover:bg-(--lg-primary-dark) transition">{{ formMode === 'create' ? 'Tạo mới' : 'Lưu thay đổi' }}</button>
        </div>
      </div>
    </div>

    <ConfirmActionDialog
      v-if="pendingAction"
      :modelValue="true"
      title="Ngừng hoạt động khóa tuyển sinh"
      :message="`Bạn có chắc muốn ngừng hoạt động khóa tuyển sinh ${pendingAction.value.tenKhoa || pendingAction.value.TenKhoa}?`"
      confirm-label="Xác nhận"
      variant="danger"
      @confirm="executeDeactivate"
      @cancel="pendingAction = null"
    />
  </div>
</template>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
