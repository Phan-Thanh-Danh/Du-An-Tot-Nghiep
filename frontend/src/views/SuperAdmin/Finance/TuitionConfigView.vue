<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import {
  AlertTriangle,
  Ban,
  Calculator,
  ChevronLeft,
  ChevronRight,
  Edit2,
  Filter,
  Loader2,
  Plus,
  RefreshCw,
  Save,
  Search,
  Wallet,
  X,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { usePopupStore } from '@/stores/popup'
import {
  createTuitionConfig,
  deactivateTuitionConfig,
  getTuitionConfig,
  getTuitionConfigOptions,
  getTuitionConfigs,
  updateTuitionConfig,
} from '@/services/financeTuitionConfigService'

const DEFAULT_CALCULATION_TYPE = 'co_dinh_theo_hoc_ky'

const authStore = useAuthStore()
const popup = usePopupStore()

const configs = ref([])
const loading = ref(false)
const optionsLoading = ref(false)
const saving = ref(false)
const deactivating = ref(false)
const loadError = ref('')

const options = ref({
  organizations: [],
  trainingPrograms: [],
  academicTerms: [],
  calculationTypes: [],
})

const filters = ref({
  keyword: '',
  maDonVi: '',
  maChuongTrinhDaoTao: '',
  maHocKy: '',
  conHoatDong: 'true',
  loaiCachTinhHocPhi: '',
})

const pagination = ref({
  pageNumber: 1,
  pageSize: 20,
  totalItems: 0,
  totalPages: 0,
})

const isFormOpen = ref(false)
const isEditMode = ref(false)
const selectedConfigId = ref(null)
const formErrors = ref({})
const deactivateTarget = ref(null)

function emptyForm() {
  return {
    maDonVi: '',
    maChuongTrinhDaoTao: '',
    maHocKy: '',
    namHocTrongChuongTrinh: 1,
    hocKyTrongNam: 1,
    soThuTuHocKy: 1,
    loaiCachTinhHocPhi: DEFAULT_CALCULATION_TYPE,
    soTienHocPhi: 0,
    tienHocLieu: 0,
    conHoatDong: true,
    ghiChu: '',
  }
}

const form = ref(emptyForm())

const canManage = computed(() => {
  return ['SuperAdmin', 'FinanceAdmin'].includes(authStore.role)
})

const previewTotal = computed(() => {
  return toNumber(form.value.soTienHocPhi) + toNumber(form.value.tienHocLieu)
})

const activeCount = computed(() => configs.value.filter((item) => rowActive(item)).length)
const inactiveCount = computed(() => configs.value.length - activeCount.value)

watch(
  form,
  () => {
    if (isFormOpen.value) validateForm()
  },
  { deep: true },
)

function getValue(source, ...keys) {
  for (const key of keys) {
    if (source && source[key] !== undefined && source[key] !== null) {
      return source[key]
    }
  }

  return ''
}

function toNumber(value) {
  const numberValue = Number(value)
  return Number.isFinite(numberValue) ? numberValue : 0
}

function toInteger(value) {
  const numberValue = Number(value)
  return Number.isFinite(numberValue) ? Math.trunc(numberValue) : 0
}

function normalizeOption(item) {
  return {
    id: Number(getValue(item, 'id', 'Id')),
    label: String(getValue(item, 'label', 'Label')),
    code: getValue(item, 'code', 'Code') || '',
    parentId: getValue(item, 'parentId', 'ParentId') || null,
  }
}

function normalizeCalculationType(item) {
  return {
    value: String(getValue(item, 'value', 'Value')),
    label: String(getValue(item, 'label', 'Label')),
  }
}

function normalizeOptions(payload) {
  const organizations = getValue(payload, 'organizations', 'Organizations') || []
  const trainingPrograms = getValue(payload, 'trainingPrograms', 'TrainingPrograms') || []
  const academicTerms = getValue(payload, 'academicTerms', 'AcademicTerms') || []
  const calculationTypes = getValue(payload, 'calculationTypes', 'CalculationTypes') || []

  return {
    organizations: organizations.map(normalizeOption),
    trainingPrograms: trainingPrograms.map(normalizeOption),
    academicTerms: academicTerms.map(normalizeOption),
    calculationTypes: calculationTypes.map(normalizeCalculationType),
  }
}

function optionText(option) {
  return option.code ? `${option.code} - ${option.label}` : option.label
}

function calculationTypeLabel(value) {
  const type = options.value.calculationTypes.find((item) => item.value === value)
  if (type) return type.label

  const labels = {
    co_dinh_theo_hoc_ky: 'Cố định theo học kỳ',
    theo_tin_chi: 'Theo tín chỉ',
    theo_mon_hoc: 'Theo môn học',
  }

  return labels[value] || value || 'Chưa xác định'
}

function rowId(row) {
  return getValue(row, 'id', 'Id', 'maCauHinhHocPhi', 'MaCauHinhHocPhi')
}

function rowActive(row) {
  return Boolean(getValue(row, 'conHoatDong', 'ConHoatDong'))
}

function rowEditable(row) {
  const value = getValue(row, 'coDuocSua', 'CoDuocSua')
  return value === '' ? true : Boolean(value)
}

function rowEditReason(row) {
  return getValue(row, 'lyDoKhongDuocSua', 'LyDoKhongDuocSua') || ''
}

function rowField(row, camelKey, pascalKey, defaultText = '') {
  const value = getValue(row, camelKey, pascalKey)
  return value === '' ? defaultText : value
}

function formatMoney(value) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0,
  }).format(toNumber(value))
}

function buildListParams() {
  return {
    keyword: filters.value.keyword.trim(),
    maDonVi: filters.value.maDonVi,
    maChuongTrinhDaoTao: filters.value.maChuongTrinhDaoTao,
    maHocKy: filters.value.maHocKy,
    conHoatDong: filters.value.conHoatDong,
    loaiCachTinhHocPhi: filters.value.loaiCachTinhHocPhi,
    pageNumber: pagination.value.pageNumber,
    pageSize: pagination.value.pageSize,
  }
}

async function loadOptions() {
  optionsLoading.value = true

  try {
    const data = await getTuitionConfigOptions()
    options.value = normalizeOptions(data)
  } catch (error) {
    popup.error('Không tải được dữ liệu chọn', error?.message || 'Vui lòng thử lại sau.')
  } finally {
    optionsLoading.value = false
  }
}

async function loadConfigs() {
  loading.value = true
  loadError.value = ''

  try {
    const data = await getTuitionConfigs(buildListParams())
    configs.value = getValue(data, 'items', 'Items') || []
    pagination.value.totalItems = Number(getValue(data, 'totalItems', 'TotalItems') || 0)
    pagination.value.totalPages = Number(getValue(data, 'totalPages', 'TotalPages') || 0)
  } catch (error) {
    loadError.value = error?.message || 'Không tải được danh sách cấu hình học phí.'
    configs.value = []
    pagination.value.totalItems = 0
    pagination.value.totalPages = 0
    popup.error('Không tải được danh sách', loadError.value)
  } finally {
    loading.value = false
  }
}

function applyFilters() {
  pagination.value.pageNumber = 1
  loadConfigs()
}

function resetFilters() {
  filters.value = {
    keyword: '',
    maDonVi: '',
    maChuongTrinhDaoTao: '',
    maHocKy: '',
    conHoatDong: 'true',
    loaiCachTinhHocPhi: '',
  }
  applyFilters()
}

function goToPage(pageNumber) {
  if (pageNumber < 1 || pageNumber > pagination.value.totalPages || loading.value) return
  pagination.value.pageNumber = pageNumber
  loadConfigs()
}

function changePageSize() {
  pagination.value.pageNumber = 1
  loadConfigs()
}

function validateForm() {
  const errors = {}
  const currentForm = form.value

  if (!currentForm.maDonVi) errors.maDonVi = 'Vui lòng chọn cơ sở.'
  if (!currentForm.maChuongTrinhDaoTao) {
    errors.maChuongTrinhDaoTao = 'Vui lòng chọn chương trình đào tạo.'
  }
  if (!currentForm.maHocKy) errors.maHocKy = 'Vui lòng chọn học kỳ.'
  if (!currentForm.loaiCachTinhHocPhi) {
    errors.loaiCachTinhHocPhi = 'Vui lòng chọn cách tính học phí.'
  }

  if (toInteger(currentForm.namHocTrongChuongTrinh) < 1) {
    errors.namHocTrongChuongTrinh = 'Năm học trong chương trình phải lớn hơn hoặc bằng 1.'
  }
  if (![1, 2, 3].includes(toInteger(currentForm.hocKyTrongNam))) {
    errors.hocKyTrongNam = 'Học kỳ trong năm chỉ được là 1, 2 hoặc 3.'
  }
  if (toInteger(currentForm.soThuTuHocKy) < 1) {
    errors.soThuTuHocKy = 'Số thứ tự học kỳ phải lớn hơn hoặc bằng 1.'
  }
  if (toNumber(currentForm.soTienHocPhi) < 0) {
    errors.soTienHocPhi = 'Số tiền học phí không được âm.'
  }
  if (toNumber(currentForm.tienHocLieu) < 0) {
    errors.tienHocLieu = 'Tiền học liệu không được âm.'
  }

  formErrors.value = errors
  return Object.keys(errors).length === 0
}

function makePayload() {
  return {
    maDonVi: toInteger(form.value.maDonVi),
    maChuongTrinhDaoTao: toInteger(form.value.maChuongTrinhDaoTao),
    maHocKy: toInteger(form.value.maHocKy),
    namHocTrongChuongTrinh: toInteger(form.value.namHocTrongChuongTrinh),
    hocKyTrongNam: toInteger(form.value.hocKyTrongNam),
    soThuTuHocKy: toInteger(form.value.soThuTuHocKy),
    loaiCachTinhHocPhi: form.value.loaiCachTinhHocPhi,
    soTienHocPhi: toNumber(form.value.soTienHocPhi),
    tienHocLieu: toNumber(form.value.tienHocLieu),
    conHoatDong: Boolean(form.value.conHoatDong),
    ghiChu: form.value.ghiChu?.trim() || null,
  }
}

function openCreateForm() {
  if (!canManage.value) {
    popup.warning('Chỉ được xem', 'Vai trò hiện tại không được tạo cấu hình học phí.')
    return
  }

  selectedConfigId.value = null
  isEditMode.value = false
  form.value = emptyForm()
  formErrors.value = {}
  isFormOpen.value = true
}

async function openEditForm(row) {
  if (!canManage.value) {
    popup.warning('Chỉ được xem', 'Vai trò hiện tại không được chỉnh sửa cấu hình học phí.')
    return
  }

  if (!rowEditable(row)) {
    popup.warning('Không thể sửa cấu hình', rowEditReason(row) || 'Học kỳ này không còn cho phép chỉnh sửa.')
    return
  }

  selectedConfigId.value = rowId(row)
  isEditMode.value = true
  formErrors.value = {}
  isFormOpen.value = true

  try {
    const detail = await getTuitionConfig(selectedConfigId.value)
    populateForm(detail)
  } catch (error) {
    isFormOpen.value = false
    popup.error('Không tải được chi tiết', error?.message || 'Vui lòng thử lại.')
  }
}

function populateForm(config) {
  const activeValue = getValue(config, 'conHoatDong', 'ConHoatDong')
  form.value = {
    maDonVi: String(getValue(config, 'maDonVi', 'MaDonVi')),
    maChuongTrinhDaoTao: String(getValue(config, 'maChuongTrinhDaoTao', 'MaChuongTrinhDaoTao')),
    maHocKy: String(getValue(config, 'maHocKy', 'MaHocKy')),
    namHocTrongChuongTrinh: toInteger(getValue(config, 'namHocTrongChuongTrinh', 'NamHocTrongChuongTrinh')),
    hocKyTrongNam: toInteger(getValue(config, 'hocKyTrongNam', 'HocKyTrongNam')),
    soThuTuHocKy: toInteger(getValue(config, 'soThuTuHocKy', 'SoThuTuHocKy')),
    loaiCachTinhHocPhi: String(getValue(config, 'loaiCachTinhHocPhi', 'LoaiCachTinhHocPhi') || DEFAULT_CALCULATION_TYPE),
    soTienHocPhi: toNumber(getValue(config, 'soTienHocPhi', 'SoTienHocPhi')),
    tienHocLieu: toNumber(getValue(config, 'tienHocLieu', 'TienHocLieu')),
    conHoatDong: activeValue === '' ? true : Boolean(activeValue),
    ghiChu: String(getValue(config, 'ghiChu', 'GhiChu') || ''),
  }
}

function closeForm() {
  if (saving.value) return
  isFormOpen.value = false
}

async function submitForm() {
  if (!canManage.value || !validateForm()) return

  saving.value = true

  try {
    const payload = makePayload()
    if (isEditMode.value) {
      await updateTuitionConfig(selectedConfigId.value, payload)
      popup.success('Đã cập nhật cấu hình', 'Tổng tiền dự kiến được backend tính lại từ học phí và học liệu.')
    } else {
      await createTuitionConfig(payload)
      popup.success('Đã tạo cấu hình', 'Cấu hình học phí chương trình đã sẵn sàng cho kỳ học.')
    }

    isFormOpen.value = false
    await loadConfigs()
  } catch (error) {
    popup.error('Không lưu được cấu hình', error?.message || 'Vui lòng kiểm tra dữ liệu và thử lại.')
  } finally {
    saving.value = false
  }
}

function askDeactivate(row) {
  if (!canManage.value) {
    popup.warning('Chỉ được xem', 'Vai trò hiện tại không được vô hiệu hóa cấu hình học phí.')
    return
  }

  deactivateTarget.value = row
}

async function confirmDeactivate() {
  if (!deactivateTarget.value) return

  deactivating.value = true

  try {
    await deactivateTuitionConfig(rowId(deactivateTarget.value))
    popup.success('Đã vô hiệu hóa cấu hình', 'Dữ liệu được giữ lại để tra cứu và audit.')
    deactivateTarget.value = null
    await loadConfigs()
  } catch (error) {
    popup.error('Không vô hiệu hóa được', error?.message || 'Vui lòng thử lại.')
  } finally {
    deactivating.value = false
  }
}

onMounted(async () => {
  await loadOptions()
  await loadConfigs()
})
</script>

<template>
  <div class="tuition-config-page space-y-4">
    <section class="summary-grid">
      <div class="summary-item">
        <Wallet :size="18" />
        <div>
          <p class="summary-label">Tổng cấu hình</p>
          <p class="summary-value">{{ pagination.totalItems }}</p>
        </div>
      </div>
      <div class="summary-item">
        <Filter :size="18" />
        <div>
          <p class="summary-label">Đang hiển thị</p>
          <p class="summary-value">{{ configs.length }}</p>
        </div>
      </div>
      <div class="summary-item">
        <Calculator :size="18" />
        <div>
          <p class="summary-label">Active trên trang</p>
          <p class="summary-value">{{ activeCount }}</p>
        </div>
      </div>
      <div class="summary-item">
        <Ban :size="18" />
        <div>
          <p class="summary-label">Ngưng trên trang</p>
          <p class="summary-value">{{ inactiveCount }}</p>
        </div>
      </div>
    </section>

    <section class="panel p-4">
      <div class="filter-grid">
        <label class="field">
          <span>Tìm kiếm</span>
          <span class="search-control">
            <Search :size="16" />
            <input
              v-model="filters.keyword"
              class="control input-with-icon"
              type="search"
              placeholder="Chương trình, học kỳ, ghi chú"
              @keyup.enter="applyFilters"
            >
          </span>
        </label>

        <label class="field">
          <span>Cơ sở</span>
          <select v-model="filters.maDonVi" class="control" :disabled="optionsLoading">
            <option value="">Tất cả cơ sở</option>
            <option v-for="item in options.organizations" :key="item.id" :value="item.id">
              {{ optionText(item) }}
            </option>
          </select>
        </label>

        <label class="field">
          <span>Chương trình</span>
          <select v-model="filters.maChuongTrinhDaoTao" class="control" :disabled="optionsLoading">
            <option value="">Tất cả chương trình</option>
            <option v-for="item in options.trainingPrograms" :key="item.id" :value="item.id">
              {{ optionText(item) }}
            </option>
          </select>
        </label>

        <label class="field">
          <span>Học kỳ</span>
          <select v-model="filters.maHocKy" class="control" :disabled="optionsLoading">
            <option value="">Tất cả học kỳ</option>
            <option v-for="item in options.academicTerms" :key="item.id" :value="item.id">
              {{ optionText(item) }}
            </option>
          </select>
        </label>

        <label class="field">
          <span>Trạng thái</span>
          <select v-model="filters.conHoatDong" class="control">
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đang hoạt động</option>
            <option value="false">Ngưng hoạt động</option>
          </select>
        </label>

        <label class="field">
          <span>Cách tính</span>
          <select v-model="filters.loaiCachTinhHocPhi" class="control" :disabled="optionsLoading">
            <option value="">Tất cả cách tính</option>
            <option v-for="item in options.calculationTypes" :key="item.value" :value="item.value">
              {{ item.label }}
            </option>
          </select>
        </label>
      </div>

      <div class="toolbar-actions">
        <button class="secondary-btn" type="button" @click="resetFilters">
          <X :size="16" />
          Xóa lọc
        </button>
        <button class="secondary-btn" type="button" :disabled="loading" @click="loadConfigs">
          <RefreshCw :size="16" :class="{ spin: loading }" />
          Tải lại
        </button>
        <button class="primary-btn" type="button" @click="applyFilters">
          <Filter :size="16" />
          Lọc
        </button>
        <button v-if="canManage" class="primary-btn" type="button" @click="openCreateForm">
          <Plus :size="16" />
          Tạo cấu hình
        </button>
      </div>
    </section>

    <section class="panel table-panel">
      <div v-if="loadError" class="state-line error-line">
        <AlertTriangle :size="18" />
        <span>{{ loadError }}</span>
      </div>

      <div class="table-scroll">
        <table>
          <thead>
            <tr>
              <th>Cơ sở</th>
              <th>Chương trình đào tạo</th>
              <th>Học kỳ</th>
              <th>Năm CT</th>
              <th>HK/năm</th>
              <th>Thứ tự</th>
              <th>Cách tính</th>
              <th class="money-col">Học phí</th>
              <th class="money-col">Học liệu</th>
              <th class="money-col">Tổng dự kiến</th>
              <th>Trạng thái</th>
              <th class="action-col">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td colspan="12">
                <div class="state-line">
                  <Loader2 :size="18" class="spin" />
                  <span>Đang tải danh sách cấu hình học phí...</span>
                </div>
              </td>
            </tr>

            <tr v-else-if="configs.length === 0">
              <td colspan="12">
                <div class="empty-state">
                  <Wallet :size="30" />
                  <p>Chưa có cấu hình học phí phù hợp bộ lọc.</p>
                </div>
              </td>
            </tr>

            <tr v-for="item in configs" v-else :key="rowId(item)">
              <td>
                <div class="cell-strong">{{ rowField(item, 'tenDonVi', 'TenDonVi') }}</div>
                <div class="cell-muted">#{{ rowField(item, 'maDonVi', 'MaDonVi') }}</div>
              </td>
              <td>
                <div class="cell-strong">{{ rowField(item, 'tenChuongTrinhDaoTao', 'TenChuongTrinhDaoTao', rowField(item, 'tenChuongTrinh', 'TenChuongTrinh')) }}</div>
                <div class="cell-muted">{{ rowField(item, 'maCodeChuongTrinh', 'MaCodeChuongTrinh') }}</div>
              </td>
              <td>
                <div class="cell-strong">{{ rowField(item, 'tenHocKy', 'TenHocKy') }}</div>
                <div class="cell-muted">{{ rowField(item, 'maCodeHocKy', 'MaCodeHocKy') }}</div>
              </td>
              <td>{{ rowField(item, 'namHocTrongChuongTrinh', 'NamHocTrongChuongTrinh') }}</td>
              <td>{{ rowField(item, 'hocKyTrongNam', 'HocKyTrongNam') }}</td>
              <td>{{ rowField(item, 'soThuTuHocKy', 'SoThuTuHocKy') }}</td>
              <td>
                <span class="type-pill">
                  {{ calculationTypeLabel(rowField(item, 'loaiCachTinhHocPhi', 'LoaiCachTinhHocPhi')) }}
                </span>
              </td>
              <td class="money-col">{{ formatMoney(rowField(item, 'soTienHocPhi', 'SoTienHocPhi')) }}</td>
              <td class="money-col">{{ formatMoney(rowField(item, 'tienHocLieu', 'TienHocLieu')) }}</td>
              <td class="money-col total-money">{{ formatMoney(rowField(item, 'tongTienDuKien', 'TongTienDuKien')) }}</td>
              <td>
                <span class="status-badge" :class="rowActive(item) ? 'status-active' : 'status-inactive'">
                  {{ rowActive(item) ? 'Đang hoạt động' : 'Ngưng hoạt động' }}
                </span>
              </td>
              <td class="action-col">
                <button
                  class="icon-btn"
                  type="button"
                  title="Chỉnh sửa cấu hình"
                  :disabled="!canManage || !rowEditable(item)"
                  @click="openEditForm(item)"
                >
                  <Edit2 :size="16" />
                </button>
                <button
                  class="icon-btn danger"
                  type="button"
                  title="Vô hiệu hóa cấu hình"
                  :disabled="!canManage || !rowActive(item)"
                  @click="askDeactivate(item)"
                >
                  <Ban :size="16" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="pagination-bar">
        <div class="page-size">
          <span>Hiển thị</span>
          <select v-model.number="pagination.pageSize" class="compact-control" @change="changePageSize">
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
          </select>
          <span>dòng</span>
        </div>

        <div class="pager">
          <button
            class="icon-btn"
            type="button"
            :disabled="pagination.pageNumber <= 1 || loading"
            @click="goToPage(pagination.pageNumber - 1)"
          >
            <ChevronLeft :size="16" />
          </button>
          <span>Trang {{ pagination.pageNumber }} / {{ pagination.totalPages || 1 }}</span>
          <button
            class="icon-btn"
            type="button"
            :disabled="pagination.pageNumber >= pagination.totalPages || loading"
            @click="goToPage(pagination.pageNumber + 1)"
          >
            <ChevronRight :size="16" />
          </button>
        </div>
      </div>
    </section>

    <Teleport to="body">
      <div v-if="isFormOpen" class="modal-backdrop">
        <section class="modal-panel">
          <header class="modal-header">
            <div>
              <p class="modal-eyebrow">{{ isEditMode ? 'Cập nhật' : 'Tạo mới' }}</p>
              <h2>{{ isEditMode ? 'Cấu hình học phí chương trình' : 'Cấu hình học phí theo kỳ' }}</h2>
            </div>
            <button class="icon-btn" type="button" title="Đóng" @click="closeForm">
              <X :size="18" />
            </button>
          </header>

          <div class="modal-body">
            <div class="form-grid">
              <label class="field">
                <span>Cơ sở</span>
                <select v-model="form.maDonVi" class="control">
                  <option value="">Chọn cơ sở</option>
                  <option v-for="item in options.organizations" :key="item.id" :value="String(item.id)">
                    {{ optionText(item) }}
                  </option>
                </select>
                <small v-if="formErrors.maDonVi">{{ formErrors.maDonVi }}</small>
              </label>

              <label class="field">
                <span>Chương trình đào tạo</span>
                <select v-model="form.maChuongTrinhDaoTao" class="control">
                  <option value="">Chọn chương trình</option>
                  <option v-for="item in options.trainingPrograms" :key="item.id" :value="String(item.id)">
                    {{ optionText(item) }}
                  </option>
                </select>
                <small v-if="formErrors.maChuongTrinhDaoTao">{{ formErrors.maChuongTrinhDaoTao }}</small>
              </label>

              <label class="field">
                <span>Học kỳ</span>
                <select v-model="form.maHocKy" class="control">
                  <option value="">Chọn học kỳ</option>
                  <option v-for="item in options.academicTerms" :key="item.id" :value="String(item.id)">
                    {{ optionText(item) }}
                  </option>
                </select>
                <small v-if="formErrors.maHocKy">{{ formErrors.maHocKy }}</small>
              </label>

              <label class="field">
                <span>Cách tính học phí</span>
                <select v-model="form.loaiCachTinhHocPhi" class="control">
                  <option v-for="item in options.calculationTypes" :key="item.value" :value="item.value">
                    {{ item.label }}
                  </option>
                </select>
                <small v-if="formErrors.loaiCachTinhHocPhi">{{ formErrors.loaiCachTinhHocPhi }}</small>
              </label>

              <label class="field">
                <span>Năm học trong chương trình</span>
                <input v-model.number="form.namHocTrongChuongTrinh" class="control" type="number" min="1">
                <small v-if="formErrors.namHocTrongChuongTrinh">{{ formErrors.namHocTrongChuongTrinh }}</small>
              </label>

              <label class="field">
                <span>Học kỳ trong năm</span>
                <select v-model.number="form.hocKyTrongNam" class="control">
                  <option :value="1">1</option>
                  <option :value="2">2</option>
                  <option :value="3">3</option>
                </select>
                <small v-if="formErrors.hocKyTrongNam">{{ formErrors.hocKyTrongNam }}</small>
              </label>

              <label class="field">
                <span>Số thứ tự học kỳ</span>
                <input v-model.number="form.soThuTuHocKy" class="control" type="number" min="1">
                <small v-if="formErrors.soThuTuHocKy">{{ formErrors.soThuTuHocKy }}</small>
              </label>

              <label class="field">
                <span>Trạng thái</span>
                <select v-model="form.conHoatDong" class="control">
                  <option :value="true">Đang hoạt động</option>
                  <option :value="false">Ngưng hoạt động</option>
                </select>
              </label>

              <label class="field">
                <span>Số tiền học phí</span>
                <input v-model.number="form.soTienHocPhi" class="control" type="number" min="0" step="1000">
                <small v-if="formErrors.soTienHocPhi">{{ formErrors.soTienHocPhi }}</small>
              </label>

              <label class="field">
                <span>Tiền học liệu</span>
                <input v-model.number="form.tienHocLieu" class="control" type="number" min="0" step="1000">
                <small v-if="formErrors.tienHocLieu">{{ formErrors.tienHocLieu }}</small>
              </label>
            </div>

            <div class="total-preview">
              <Calculator :size="18" />
              <span>Tổng tiền dự kiến</span>
              <strong>{{ formatMoney(previewTotal) }}</strong>
            </div>

            <label class="field">
              <span>Ghi chú</span>
              <textarea v-model="form.ghiChu" class="control textarea-control" rows="3" placeholder="Ghi chú nội bộ cho cấu hình học phí" />
            </label>
          </div>

          <footer class="modal-footer">
            <button class="secondary-btn" type="button" :disabled="saving" @click="closeForm">
              Hủy
            </button>
            <button class="primary-btn" type="button" :disabled="saving || Object.keys(formErrors).length > 0" @click="submitForm">
              <Loader2 v-if="saving" :size="16" class="spin" />
              <Save v-else :size="16" />
              Lưu cấu hình
            </button>
          </footer>
        </section>
      </div>

      <div v-if="deactivateTarget" class="modal-backdrop">
        <section class="confirm-panel">
          <div class="confirm-icon">
            <AlertTriangle :size="22" />
          </div>
          <h2>Vô hiệu hóa cấu hình học phí?</h2>
          <p>
            Cấu hình của
            <strong>{{ rowField(deactivateTarget, 'tenChuongTrinhDaoTao', 'TenChuongTrinhDaoTao', rowField(deactivateTarget, 'tenChuongTrinh', 'TenChuongTrinh')) }}</strong>
            trong
            <strong>{{ rowField(deactivateTarget, 'tenHocKy', 'TenHocKy') }}</strong>
            sẽ chuyển sang trạng thái ngưng hoạt động.
          </p>
          <div class="confirm-actions">
            <button class="secondary-btn" type="button" :disabled="deactivating" @click="deactivateTarget = null">
              Hủy
            </button>
            <button class="danger-btn" type="button" :disabled="deactivating" @click="confirmDeactivate">
              <Loader2 v-if="deactivating" :size="16" class="spin" />
              <Ban v-else :size="16" />
              Vô hiệu hóa
            </button>
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.tuition-config-page {
  color: var(--text-body);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.75rem;
}

.summary-item,
.panel {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 8px;
  box-shadow: var(--lg-shadow-sm);
  backdrop-filter: blur(var(--glass-blur));
}

.summary-item {
  min-height: 4.75rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 1rem;
  color: var(--text-heading);
}

.summary-item svg {
  color: var(--text-link);
}

.summary-label {
  margin: 0;
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 700;
}

.summary-value {
  margin: 0.125rem 0 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  font-weight: 800;
  line-height: 1;
}

.filter-grid,
.form-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.875rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  min-width: 0;
}

.field > span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 800;
}

.field small {
  color: var(--color-danger-text);
  font-size: 0.72rem;
  font-weight: 700;
}

.control,
.compact-control {
  min-height: 2.5rem;
  width: 100%;
  border: 1px solid var(--border-input);
  border-radius: 8px;
  background: var(--surface-input);
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 600;
  outline: none;
  transition: border-color 0.16s ease, box-shadow 0.16s ease, background 0.16s ease;
}

.control {
  padding: 0 0.75rem;
}

.compact-control {
  min-height: 2rem;
  padding: 0 0.5rem;
}

.control:focus,
.compact-control:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--focus-ring);
  background: var(--surface-input-focus);
}

.control:disabled,
.compact-control:disabled {
  color: var(--text-disabled);
  cursor: not-allowed;
}

.textarea-control {
  min-height: 5.5rem;
  padding: 0.65rem 0.75rem;
  resize: vertical;
}

.search-control {
  position: relative;
  display: block;
}

.search-control svg {
  position: absolute;
  left: 0.75rem;
  top: 50%;
  color: var(--text-placeholder);
  transform: translateY(-50%);
  pointer-events: none;
}

.input-with-icon {
  padding-left: 2.25rem;
}

.toolbar-actions,
.pagination-bar,
.modal-footer,
.confirm-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 0.625rem;
}

.toolbar-actions {
  flex-wrap: wrap;
  margin-top: 1rem;
}

.primary-btn,
.secondary-btn,
.danger-btn,
.icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.45rem;
  border-radius: 8px;
  font-size: 0.84rem;
  font-weight: 800;
  transition: transform 0.16s ease, border-color 0.16s ease, background 0.16s ease;
}

.primary-btn,
.secondary-btn,
.danger-btn {
  min-height: 2.35rem;
  padding: 0 0.85rem;
}

.primary-btn {
  border: 1px solid var(--border-focus);
  background: var(--text-link);
  color: var(--text-inverse);
}

.secondary-btn {
  border: 1px solid var(--border-default);
  background: var(--surface-elevated);
  color: var(--text-heading);
}

.danger-btn {
  border: 1px solid var(--color-danger-text);
  background: var(--color-danger-bg);
  color: var(--color-danger-text);
}

.icon-btn {
  width: 2.1rem;
  height: 2.1rem;
  border: 1px solid var(--border-default);
  background: var(--surface-elevated);
  color: var(--text-heading);
}

.icon-btn.danger {
  color: var(--color-danger-text);
}

.primary-btn:hover,
.secondary-btn:hover,
.danger-btn:hover,
.icon-btn:hover {
  transform: translateY(-1px);
}

.primary-btn:disabled,
.secondary-btn:disabled,
.danger-btn:disabled,
.icon-btn:disabled {
  color: var(--text-disabled);
  cursor: not-allowed;
  opacity: 0.62;
  transform: none;
}

.table-panel {
  overflow: hidden;
}

.table-scroll {
  overflow-x: auto;
}

table {
  width: 100%;
  min-width: 1180px;
  border-collapse: separate;
  border-spacing: 0;
}

th {
  background: var(--surface-table-header);
  border-bottom: 1px solid var(--border-table);
  color: var(--text-label);
  font-size: 0.68rem;
  font-weight: 900;
  letter-spacing: 0;
  padding: 0.8rem 0.75rem;
  text-align: left;
  text-transform: uppercase;
  white-space: nowrap;
}

td {
  border-bottom: 1px solid var(--border-table);
  color: var(--text-body);
  font-size: 0.83rem;
  font-weight: 600;
  padding: 0.85rem 0.75rem;
  vertical-align: middle;
}

tbody tr:hover td {
  background: var(--surface-table-row-hover);
}

.money-col {
  text-align: right;
  white-space: nowrap;
}

.total-money {
  color: var(--text-heading);
  font-weight: 900;
}

.action-col {
  text-align: right;
  white-space: nowrap;
}

.cell-strong {
  color: var(--text-heading);
  font-weight: 850;
}

.cell-muted {
  color: var(--text-muted);
  font-size: 0.75rem;
  margin-top: 0.15rem;
}

.type-pill,
.status-badge {
  display: inline-flex;
  align-items: center;
  min-height: 1.7rem;
  border-radius: 8px;
  border: 1px solid var(--border-default);
  padding: 0 0.55rem;
  font-size: 0.75rem;
  font-weight: 850;
  white-space: nowrap;
}

.type-pill {
  background: var(--surface-solid);
  color: var(--text-heading);
}

.status-active {
  border-color: var(--color-success-text);
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.status-inactive {
  border-color: var(--border-default);
  background: var(--surface-solid);
  color: var(--text-muted);
}

.state-line,
.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.6rem;
  min-height: 7rem;
  color: var(--text-label);
  font-weight: 700;
}

.error-line {
  min-height: 3rem;
  justify-content: flex-start;
  padding: 0 1rem;
  color: var(--color-danger-text);
}

.empty-state {
  flex-direction: column;
}

.empty-state p {
  margin: 0;
}

.pagination-bar {
  border-top: 1px solid var(--border-table);
  padding: 0.75rem 1rem;
  justify-content: space-between;
}

.page-size,
.pager {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--text-label);
  font-size: 0.82rem;
  font-weight: 800;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  background: color-mix(in srgb, var(--surface-page) 76%, transparent);
  backdrop-filter: blur(10px);
}

.modal-panel,
.confirm-panel {
  width: min(100%, 880px);
  max-height: calc(100vh - 2rem);
  overflow: auto;
  border: 1px solid var(--border-card);
  border-radius: 8px;
  background: var(--surface-modal);
  box-shadow: var(--lg-shadow-lg);
}

.confirm-panel {
  width: min(100%, 460px);
  padding: 1.25rem;
  text-align: center;
}

.modal-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  border-bottom: 1px solid var(--border-default);
  padding: 1rem 1.25rem;
}

.modal-header h2,
.confirm-panel h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.05rem;
  font-weight: 900;
}

.modal-eyebrow {
  margin: 0 0 0.2rem;
  color: var(--text-link);
  font-size: 0.72rem;
  font-weight: 900;
  text-transform: uppercase;
}

.modal-body {
  display: grid;
  gap: 1rem;
  padding: 1.25rem;
}

.modal-footer {
  border-top: 1px solid var(--border-default);
  padding: 1rem 1.25rem;
}

.total-preview {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  border: 1px solid var(--border-default);
  border-radius: 8px;
  background: var(--surface-solid);
  color: var(--text-label);
  padding: 0.85rem 1rem;
  font-weight: 800;
}

.total-preview strong {
  margin-left: auto;
  color: var(--text-heading);
  font-size: 1rem;
}

.confirm-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.7rem;
  height: 2.7rem;
  border-radius: 8px;
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
}

.confirm-panel p {
  color: var(--text-body);
  font-size: 0.9rem;
  line-height: 1.6;
}

.confirm-panel strong {
  color: var(--text-heading);
}

.confirm-actions {
  justify-content: center;
  margin-top: 1rem;
}

.spin {
  animation: spin 0.9s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 1180px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filter-grid,
  .form-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .summary-grid,
  .filter-grid,
  .form-grid {
    grid-template-columns: 1fr;
  }

  .toolbar-actions,
  .pagination-bar {
    align-items: stretch;
    flex-direction: column;
  }

  .primary-btn,
  .secondary-btn,
  .danger-btn {
    width: 100%;
  }
}
</style>
