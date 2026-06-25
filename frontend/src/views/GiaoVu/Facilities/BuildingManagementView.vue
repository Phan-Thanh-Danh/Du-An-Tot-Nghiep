<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Search, Plus, X, Building, ChevronDown, ChevronRight, MapPin, Layers,
  Pencil, Trash2, AlertCircle, DoorOpen, Users, Monitor, Tv,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { buildingApi } from '@/services/buildingApi'
import { floorApi } from '@/services/floorApi'
import { roomApi } from '@/services/roomApi'

const loading = ref(true)
const error = ref(null)
const buildings = ref([])
const searchQuery = ref('')
const filterStatus = ref('all')

const STATUS_OPTIONS = [
  { value: 'all', label: 'Tất cả trạng thái' },
  { value: 'active', label: 'Đang hoạt động' },
  { value: 'inactive', label: 'Ngừng hoạt động' },
]

const expandedBuildingId = ref(null)
const expandedFloorId = ref(null)

const floorCache = reactive({})
const roomCache = reactive({})
const floorLoading = reactive({})
const roomLoading = reactive({})

onMounted(async () => {
  await fetchBuildings()
})

async function fetchBuildings() {
  loading.value = true
  error.value = null
  try {
    const res = await buildingApi.list({ PageSize: 200 })
    buildings.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch (e) {
    error.value = e.message || 'Không thể tải danh sách tòa nhà'
  } finally {
    loading.value = false
  }
}

const filteredBuildings = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return buildings.value.filter(b => {
    const matchSearch = !q || b.tenToaNha?.toLowerCase().includes(q) || b.maCodeToaNha?.toLowerCase().includes(q)
    const matchStatus = filterStatus.value === 'all'
      || (filterStatus.value === 'active' && b.conHoatDong !== false)
      || (filterStatus.value === 'inactive' && b.conHoatDong === false)
    return matchSearch && matchStatus
  })
})

const stats = computed(() => ({
  total: buildings.value.length,
  active: buildings.value.filter(b => b.conHoatDong !== false).length,
  inactive: buildings.value.filter(b => b.conHoatDong === false).length,
}))

function toggleBuilding(id) {
  expandedBuildingId.value = expandedBuildingId.value === id ? null : id
  expandedFloorId.value = null
  if (expandedBuildingId.value && !floorCache[id]) {
    fetchFloors(id)
  }
}

async function fetchFloors(buildingId) {
  floorLoading[buildingId] = true
  try {
    const res = await floorApi.getByBuilding(buildingId)
    floorCache[buildingId] = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch {
    floorCache[buildingId] = []
  } finally {
    floorLoading[buildingId] = false
  }
}

function toggleFloor(buildingId, floorId) {
  const key = `${buildingId}-${floorId}`
  expandedFloorId.value = expandedFloorId.value === key ? null : key
  if (expandedFloorId.value === key && !roomCache[key]) {
    fetchRooms(floorId)
  }
}

async function fetchRooms(floorId) {
  const key = `floor-${floorId}`
  roomLoading[key] = true
  try {
    const res = await roomApi.getByFloor(floorId)
    roomCache[key] = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch {
    roomCache[key] = []
  } finally {
    roomLoading[key] = false
  }
}

function getFloors(buildingId) {
  return floorCache[buildingId] || []
}

function getRooms(floorId) {
  return roomCache[`floor-${floorId}`] || []
}

// ── Building CRUD ──
const showBuildingModal = ref(false)
const editingBuilding = ref(null)
const buildingForm = reactive({
  maDonVi: 1,
  maCodeToaNha: '',
  tenToaNha: '',
  diaChi: '',
  soTang: null,
})
const buildingErrors = reactive({})
const isSavingBuilding = ref(false)

function openAddBuilding() {
  editingBuilding.value = null
  Object.assign(buildingForm, { maDonVi: 1, maCodeToaNha: '', tenToaNha: '', diaChi: '', soTang: null })
  Object.keys(buildingErrors).forEach(k => delete buildingErrors[k])
  showBuildingModal.value = true
}

function openEditBuilding(b) {
  editingBuilding.value = b
  Object.assign(buildingForm, {
    maDonVi: b.maDonVi || 1,
    maCodeToaNha: b.maCodeToaNha || '',
    tenToaNha: b.tenToaNha || '',
    diaChi: b.diaChi || '',
    soTang: b.soTang ?? null,
  })
  Object.keys(buildingErrors).forEach(k => delete buildingErrors[k])
  showBuildingModal.value = true
}

function validateBuilding() {
  Object.keys(buildingErrors).forEach(k => delete buildingErrors[k])
  if (!buildingForm.maCodeToaNha.trim()) buildingErrors.maCodeToaNha = 'Mã tòa nhà không được để trống'
  if (!buildingForm.tenToaNha.trim()) buildingErrors.tenToaNha = 'Tên tòa nhà không được để trống'
  return Object.keys(buildingErrors).length === 0
}

async function submitBuilding() {
  if (!validateBuilding()) return
  isSavingBuilding.value = true
  try {
    if (editingBuilding.value) {
      await buildingApi.update(editingBuilding.value.maToaNha, {
        ...buildingForm,
        conHoatDong: editingBuilding.value.conHoatDong,
      })
    } else {
      await buildingApi.create(buildingForm)
    }
    showBuildingModal.value = false
    await fetchBuildings()
  } catch (e) {
    buildingErrors._api = e.message || 'Lỗi khi lưu tòa nhà'
  } finally {
    isSavingBuilding.value = false
  }
}

const confirmDeleteBuilding = ref(null)

function requestDeleteBuilding(b) {
  confirmDeleteBuilding.value = b
}

async function executeDeleteBuilding() {
  if (!confirmDeleteBuilding.value) return
  try {
    await buildingApi.delete(confirmDeleteBuilding.value.maToaNha)
    confirmDeleteBuilding.value = null
    await fetchBuildings()
  } catch (e) {
    error.value = e.message || 'Không thể xóa tòa nhà'
    confirmDeleteBuilding.value = null
  }
}

// ── Floor CRUD (inline within building) ──
const showFloorModal = ref(false)
const editingFloor = ref(null)
const floorBuildingId = ref(null)
const floorForm = reactive({
  maToaNha: null,
  tenTang: '',
  thuTuTang: null,
  moTa: '',
})
const floorErrors = reactive({})
const isSavingFloor = ref(false)

function openAddFloor(buildingId) {
  floorBuildingId.value = buildingId
  editingFloor.value = null
  Object.assign(floorForm, {
    maToaNha: buildingId,
    tenTang: '',
    thuTuTang: null,
    moTa: '',
  })
  Object.keys(floorErrors).forEach(k => delete floorErrors[k])
  showFloorModal.value = true
}

function openEditFloor(f) {
  floorBuildingId.value = f.maToaNha
  editingFloor.value = f
  Object.assign(floorForm, {
    maToaNha: f.maToaNha,
    tenTang: f.tenTang || '',
    thuTuTang: f.thuTuTang ?? null,
    moTa: f.moTa || '',
  })
  Object.keys(floorErrors).forEach(k => delete floorErrors[k])
  showFloorModal.value = true
}

function validateFloor() {
  Object.keys(floorErrors).forEach(k => delete floorErrors[k])
  if (!floorForm.tenTang.trim()) floorErrors.tenTang = 'Tên lầu không được để trống'
  if (floorForm.thuTuTang === null || floorForm.thuTuTang === '' || isNaN(floorForm.thuTuTang))
    floorErrors.thuTuTang = 'Thứ tự phải là số'
  return Object.keys(floorErrors).length === 0
}

async function submitFloor() {
  if (!validateFloor()) return
  isSavingFloor.value = true
  try {
    if (editingFloor.value) {
      await floorApi.update(editingFloor.value.maTang, {
        ...floorForm,
        conHoatDong: editingFloor.value.conHoatDong,
      })
    } else {
      await floorApi.create(floorForm)
    }
    showFloorModal.value = false
    await fetchFloors(floorBuildingId.value)
  } catch (e) {
    floorErrors._api = e.message || 'Lỗi khi lưu lầu'
  } finally {
    isSavingFloor.value = false
  }
}

const confirmDeleteFloor = ref(null)

function requestDeleteFloor(f) {
  confirmDeleteFloor.value = f
}

async function executeDeleteFloor() {
  if (!confirmDeleteFloor.value) return
  try {
    await floorApi.delete(confirmDeleteFloor.value.maTang)
    confirmDeleteFloor.value = null
    await fetchFloors(expandedBuildingId.value)
  } catch (e) {
    error.value = e.message || 'Không thể xóa lầu'
    confirmDeleteFloor.value = null
  }
}

// ── Room helpers ──
const ROOM_TYPE_LABEL = {
  ly_thuyet: 'Lý thuyết',
  thuc_hanh: 'Thực hành',
  phong_thi_nghiem: 'Phòng thí nghiệm',
  lab: 'Lab',
  hoi_truong: 'Hội trường',
  truc_tuyen: 'Trực tuyến',
  khac: 'Khác',
}

const ROOM_STATUS_MAP = {
  hoat_dong: { label: 'Đang hoạt động', dot: 'bg-[var(--lg-success)]', pulse: true },
  bao_tri: { label: 'Bảo trì', dot: 'bg-[var(--lg-warning)]', pulse: false },
  ngung_hoat_dong: { label: 'Ngừng hoạt động', dot: 'bg-[var(--lg-danger)]', pulse: false },
}

function getRoomStatusInfo(s) {
  return ROOM_STATUS_MAP[s] || ROOM_STATUS_MAP.ngung_hoat_dong
}

const TYPE_ICON_MAP = { thuc_hanh: Monitor, hoi_truong: Tv, phong_thi_nghiem: Monitor, lab: Monitor }
function getRoomTypeIcon(t) {
  return TYPE_ICON_MAP[t] || DoorOpen
}

const router = useRouter()

function navigateToRooms(buildingId, floorId) {
  router.push({ path: '/staff/rooms', query: { buildingId, floorId } })
}
</script>

<template>
  <PageContainer
    title="Quản lý tòa nhà"
    subtitle="Quản lý danh sách tòa nhà, lầu và phòng học trong cơ sở."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="openAddBuilding">
        <Plus :size="18" /> Thêm tòa nhà
      </button>
    </template>

    <div class="space-y-4">
      <!-- Stats -->
      <div class="grid grid-cols-3 gap-3">
        <div v-for="s in [
          { label: 'Tổng tòa nhà', value: stats.total, color: 'text-[var(--color-info-text)]', bg: 'bg-[var(--color-info-bg)]' },
          { label: 'Đang hoạt động', value: stats.active, color: 'text-[var(--color-success-text)]', bg: 'bg-[var(--color-success-bg)]' },
          { label: 'Ngừng hoạt động', value: stats.inactive, color: 'text-[var(--color-danger-text)]', bg: 'bg-[var(--color-danger-bg)]' },
        ]" :key="s.label"
          :class="['rounded-2xl p-4 border border-default', s.bg]"
        >
          <p class="text-[11px] font-semibold uppercase tracking-widest text-placeholder">{{ s.label }}</p>
          <p :class="['text-2xl font-semibold mt-1', s.color]">{{ s.value }}</p>
        </div>
      </div>

      <!-- Search & Filter -->
      <div class="lg-glass-strong p-4 rounded-2xl flex flex-wrap items-center gap-3">
        <div class="flex-1 min-w-[240px] relative">
          <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
          <input v-model="searchQuery" type="text" placeholder="Tìm tên hoặc mã tòa nhà..."
            class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium transition-all" />
          <button v-if="searchQuery" class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
            @click="searchQuery = ''"><X :size="14" /></button>
        </div>
        <select v-model="filterStatus" class="lg-input px-3 py-2.5 text-sm font-bold">
          <option v-for="o in STATUS_OPTIONS" :key="o.value" :value="o.value">{{ o.label }}</option>
        </select>
      </div>

      <!-- Loading / Error / Empty -->
      <div v-if="loading" class="flex items-center justify-center py-16">
        <div class="h-8 w-8 border-2 border-[var(--lg-primary)] border-t-transparent rounded-full animate-spin"></div>
        <span class="ml-3 text-sm text-label">Đang tải...</span>
      </div>

      <div v-else-if="error" class="lg-glass-strong p-6 rounded-2xl text-center">
        <p class="text-sm text-[var(--lg-danger)] font-semibold">{{ error }}</p>
        <button class="mt-3 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="fetchBuildings">Thử lại</button>
      </div>

      <!-- Building Tree -->
      <div v-else-if="filteredBuildings.length === 0" class="flex flex-col items-center justify-center py-20 text-center">
        <div class="h-16 w-16 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
          <Building :size="28" class="text-placeholder" />
        </div>
        <p class="text-base font-semibold text-heading">Không tìm thấy tòa nhà</p>
        <p class="text-sm text-label mt-1">Thử thay đổi từ khóa hoặc bộ lọc.</p>
      </div>

      <div v-else class="space-y-3">
        <div v-for="b in filteredBuildings" :key="b.maToaNha"
          class="lg-card surface-card border border-default rounded-2xl overflow-hidden transition-all">

          <!-- Building Header Row -->
          <div
            class="flex items-center gap-3 p-4 cursor-pointer hover:bg-[var(--surface-input)] transition-colors select-none"
            @click="toggleBuilding(b.maToaNha)">
            <button class="h-8 w-8 rounded-xl surface-input flex items-center justify-center text-label shrink-0">
              <component :is="expandedBuildingId === b.maToaNha ? ChevronDown : ChevronRight" :size="18" />
            </button>
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
              <Building :size="20" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <h3 class="text-base font-semibold text-heading">{{ b.tenToaNha }}</h3>
                <span class="text-[11px] px-2 py-0.5 rounded-lg font-bold uppercase tracking-wide"
                  :class="b.conHoatDong !== false
                    ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                    : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]'">
                  {{ b.conHoatDong !== false ? 'Hoạt động' : 'Ngừng' }}
                </span>
              </div>
              <p class="text-xs text-label mt-0.5 flex items-center gap-2">
                <span class="font-mono text-[10px] uppercase">{{ b.maCodeToaNha }}</span>
                <template v-if="b.diaChi"><span>·</span> <MapPin :size="10" /> {{ b.diaChi }}</template>
                <span v-if="b.soTang">· <Layers :size="10" /> {{ b.soTang }} tầng</span>
              </p>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <button class="p-2 hover:bg-[var(--surface-input-focus)] rounded-xl text-label transition-colors"
                @click.stop="openEditBuilding(b)">
                <Pencil :size="15" />
              </button>
              <button class="p-2 hover:bg-[var(--color-danger-bg)] rounded-xl text-[var(--lg-danger)]/70 hover:text-[var(--lg-danger)] transition-colors"
                @click.stop="requestDeleteBuilding(b)">
                <Trash2 :size="15" />
              </button>
            </div>
          </div>

          <!-- Floors Section (expandable) -->
          <Transition name="slide-down">
            <div v-if="expandedBuildingId === b.maToaNha" class="border-t border-default">
              <div class="p-4 bg-[var(--surface-input)]/30">
                <div class="flex items-center justify-between mb-3">
                  <p class="text-xs font-semibold uppercase tracking-widest text-placeholder">
                    Danh sách lầu
                    <span v-if="getFloors(b.maToaNha).length" class="ml-1">({{ getFloors(b.maToaNha).length }})</span>
                  </p>
                  <button class="flex items-center gap-1.5 text-xs font-bold text-[var(--lg-primary)] hover:underline"
                    @click="openAddFloor(b.maToaNha)">
                    <Plus :size="14" /> Thêm lầu
                  </button>
                </div>

                <div v-if="floorLoading[b.maToaNha]" class="flex items-center gap-2 py-4 text-sm text-label">
                  <div class="h-4 w-4 border-2 border-[var(--lg-primary)] border-t-transparent rounded-full animate-spin"></div>
                  Đang tải lầu...
                </div>

                <div v-else-if="getFloors(b.maToaNha).length === 0" class="text-center py-6 text-sm text-placeholder">
                  Chưa có lầu nào. <button class="text-[var(--lg-primary)] font-semibold hover:underline"
                    @click="openAddFloor(b.maToaNha)">Thêm lầu đầu tiên</button>
                </div>

                <div v-else class="space-y-2">
                  <div v-for="f in getFloors(b.maToaNha)" :key="f.maTang"
                    class="rounded-xl border border-default surface-card overflow-hidden">

                    <!-- Floor Header -->
                    <div class="flex items-center gap-3 p-3 cursor-pointer hover:bg-[var(--surface-input)] transition-colors select-none"
                      @click="toggleFloor(b.maToaNha, f.maTang)">
                      <button class="h-7 w-7 rounded-lg surface-input flex items-center justify-center text-label shrink-0">
                        <component :is="expandedFloorId === `${b.maToaNha}-${f.maTang}` ? ChevronDown : ChevronRight" :size="14" />
                      </button>
                      <Layers :size="16" class="text-[var(--lg-primary)] shrink-0" />
                      <span class="text-sm font-semibold text-heading flex-1">{{ f.tenTang }}</span>
                      <span class="text-xs text-placeholder">Thứ tự {{ f.thuTuTang }}</span>
                      <span class="text-[10px] px-2 py-0.5 rounded font-bold"
                        :class="f.conHoatDong !== false
                          ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                          : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]'">
                        {{ f.conHoatDong !== false ? 'Hoạt động' : 'Ngừng' }}
                      </span>
                      <button class="p-1.5 hover:bg-[var(--surface-input-focus)] rounded-lg text-label transition-colors"
                        @click.stop="openEditFloor(f)">
                        <Pencil :size="13" />
                      </button>
                      <button class="p-1.5 hover:bg-[var(--color-danger-bg)] rounded-lg text-[var(--lg-danger)]/70 hover:text-[var(--lg-danger)] transition-colors"
                        @click.stop="requestDeleteFloor(f)">
                        <Trash2 :size="13" />
                      </button>
                    </div>

                    <!-- Rooms Section (expandable) -->
                    <Transition name="slide-down">
                      <div v-if="expandedFloorId === `${b.maToaNha}-${f.maTang}`"
                        class="border-t border-default p-3 bg-[var(--surface-input)]/50">
                        <div v-if="roomLoading[`floor-${f.maTang}`]" class="flex items-center gap-2 py-3 text-xs text-label">
                          <div class="h-3 w-3 border-2 border-[var(--lg-primary)] border-t-transparent rounded-full animate-spin"></div>
                          Đang tải phòng...
                        </div>

                        <div v-else-if="getRooms(f.maTang).length === 0"
                          class="text-center py-6 text-sm text-placeholder">
                          Chưa có phòng học trên lầu này.
                          <button class="text-[var(--lg-primary)] font-semibold hover:underline ml-1"
                            @click="navigateToRooms(b.maToaNha, f.maTang)">
                            Thêm phòng
                          </button>
                        </div>

                        <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                          <div v-for="r in getRooms(f.maTang)" :key="r.maPhong"
                            class="flex items-center gap-3 p-3 rounded-xl surface-card border border-default hover:shadow-sm transition-all">
                            <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
                              <component :is="getRoomTypeIcon(r.loaiPhong)" :size="16" />
                            </div>
                            <div class="flex-1 min-w-0">
                              <div class="flex items-center gap-1.5">
                                <span class="text-sm font-semibold text-heading">{{ r.tenPhong }}</span>
                                <span :class="['h-1.5 w-1.5 rounded-full', getRoomStatusInfo(r.trangThaiPhong).dot,
                                  getRoomStatusInfo(r.trangThaiPhong).pulse ? 'animate-pulse' : '']"></span>
                              </div>
                              <p class="text-[10px] text-label mt-0.5 flex items-center gap-1.5">
                                <Users :size="10" /> {{ r.sucChua }} SV
                                <span class="text-placeholder">·</span>
                                {{ ROOM_TYPE_LABEL[r.loaiPhong] || r.loaiPhong }}
                                <span class="text-placeholder">·</span>
                                <span class="font-mono">{{ r.maCodePhong }}</span>
                              </p>
                            </div>
                          </div>
                        </div>
                      </div>
                    </Transition>
                  </div>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </PageContainer>

  <!-- ════════════════════════════════════════
       Building Add/Edit Modal
  ════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showBuildingModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showBuildingModal = false">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">{{ editingBuilding ? 'Chỉnh sửa tòa nhà' : 'Thêm tòa nhà mới' }}</h2>
                <p class="text-sm text-label mt-0.5">{{ editingBuilding ? 'Cập nhật thông tin tòa nhà' : 'Điền thông tin tòa nhà mới' }}</p>
              </div>
              <button class="h-9 w-9 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="showBuildingModal = false"><X :size="18" /></button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <p v-if="buildingErrors._api" class="text-sm text-[var(--lg-danger)] font-semibold">{{ buildingErrors._api }}</p>

            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Mã tòa nhà <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input v-model="buildingForm.maCodeToaNha" type="text" placeholder="VD: A, B, CS1..."
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', buildingErrors.maCodeToaNha ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]' : '']" />
              <p v-if="buildingErrors.maCodeToaNha" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ buildingErrors.maCodeToaNha }}</p>
            </div>

            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Tên tòa nhà <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input v-model="buildingForm.tenToaNha" type="text" placeholder="VD: Tòa nhà A, Cơ sở 1..."
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', buildingErrors.tenToaNha ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]' : '']" />
              <p v-if="buildingErrors.tenToaNha" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ buildingErrors.tenToaNha }}</p>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Địa chỉ</label>
                <input v-model="buildingForm.diaChi" type="text" placeholder="(tùy chọn)" class="w-full lg-input px-4 py-2.5 text-sm font-medium" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Số tầng</label>
                <input v-model.number="buildingForm.soTang" type="number" min="1" placeholder="VD: 5" class="w-full lg-input px-4 py-2.5 text-sm font-medium" />
              </div>
            </div>
          </div>

          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3">
            <button class="lg-button-secondary px-5 py-2.5" @click="showBuildingModal = false">Hủy</button>
            <button :class="['lg-button-primary px-6 py-2.5', isSavingBuilding ? 'opacity-70 cursor-not-allowed' : '']"
              :disabled="isSavingBuilding" @click="submitBuilding">
              <span v-if="isSavingBuilding" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin inline-block mr-2"></span>
              {{ isSavingBuilding ? 'Đang lưu...' : editingBuilding ? 'Cập nhật' : 'Thêm tòa nhà' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════
       Floor Add/Edit Modal
  ════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="showFloorModal" class="fixed inset-0 z-[100] flex items-center justify-center p-4" @click.self="showFloorModal = false">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">{{ editingFloor ? 'Chỉnh sửa lầu' : 'Thêm lầu mới' }}</h2>
                <p class="text-sm text-label mt-0.5">{{ editingFloor ? 'Cập nhật thông tin lầu' : 'Thêm lầu vào tòa nhà' }}</p>
              </div>
              <button class="h-9 w-9 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-label transition-all"
                @click="showFloorModal = false"><X :size="18" /></button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <p v-if="floorErrors._api" class="text-sm text-[var(--lg-danger)] font-semibold">{{ floorErrors._api }}</p>

            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Tên lầu <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input v-model="floorForm.tenTang" type="text" placeholder="VD: Tầng 1, Tầng trệt..."
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', floorErrors.tenTang ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]' : '']" />
              <p v-if="floorErrors.tenTang" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ floorErrors.tenTang }}</p>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                  Thứ tự <span class="text-[var(--lg-danger)]">*</span>
                </label>
                <input v-model.number="floorForm.thuTuTang" type="number" min="1" placeholder="VD: 1"
                  :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', floorErrors.thuTuTang ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]' : '']" />
                <p v-if="floorErrors.thuTuTang" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ floorErrors.thuTuTang }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Mô tả</label>
                <input v-model="floorForm.moTa" type="text" placeholder="(tùy chọn)" class="w-full lg-input px-4 py-2.5 text-sm font-medium" />
              </div>
            </div>
          </div>

          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3">
            <button class="lg-button-secondary px-5 py-2.5" @click="showFloorModal = false">Hủy</button>
            <button :class="['lg-button-primary px-6 py-2.5', isSavingFloor ? 'opacity-70 cursor-not-allowed' : '']"
              :disabled="isSavingFloor" @click="submitFloor">
              <span v-if="isSavingFloor" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin inline-block mr-2"></span>
              {{ isSavingFloor ? 'Đang lưu...' : editingFloor ? 'Cập nhật' : 'Thêm lầu' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════
       Delete Confirmation Modal (Building)
  ════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="confirmDeleteBuilding" class="fixed inset-0 z-[200] flex items-center justify-center p-4" @click.self="confirmDeleteBuilding = null">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-sm surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="p-6">
            <div class="h-12 w-12 rounded-2xl bg-[var(--color-danger-bg)] flex items-center justify-center mb-4 mx-auto">
              <AlertCircle :size="24" class="text-[var(--lg-danger)]" />
            </div>
            <h3 class="text-lg font-semibold text-heading text-center">Xóa tòa nhà</h3>
            <p class="text-sm text-label text-center mt-2">
              Bạn có chắc muốn xóa
              <span class="font-semibold text-heading">{{ confirmDeleteBuilding.tenToaNha }}</span>
              ({{ confirmDeleteBuilding.maCodeToaNha }})? Tòa nhà sẽ được chuyển sang trạng thái ngừng hoạt động.
            </p>
          </div>
          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3">
            <button class="lg-button-secondary px-5 py-2.5" @click="confirmDeleteBuilding = null">Hủy</button>
            <button class="flex items-center gap-2 px-5 py-2.5 text-sm font-bold rounded-xl bg-[var(--lg-danger)] text-white hover:opacity-90 transition-all"
              @click="executeDeleteBuilding">
              <Trash2 :size="16" /> Xác nhận xóa
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════
       Delete Confirmation Modal (Floor)
  ════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="confirmDeleteFloor" class="fixed inset-0 z-[200] flex items-center justify-center p-4" @click.self="confirmDeleteFloor = null">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-sm surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="p-6">
            <div class="h-12 w-12 rounded-2xl bg-[var(--color-danger-bg)] flex items-center justify-center mb-4 mx-auto">
              <AlertCircle :size="24" class="text-[var(--lg-danger)]" />
            </div>
            <h3 class="text-lg font-semibold text-heading text-center">Xóa lầu</h3>
            <p class="text-sm text-label text-center mt-2">
              Bạn có chắc muốn xóa
              <span class="font-semibold text-heading">{{ confirmDeleteFloor.tenTang }}</span>?
              Lầu sẽ được chuyển sang trạng thái ngừng hoạt động.
            </p>
          </div>
          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3">
            <button class="lg-button-secondary px-5 py-2.5" @click="confirmDeleteFloor = null">Hủy</button>
            <button class="flex items-center gap-2 px-5 py-2.5 text-sm font-bold rounded-xl bg-[var(--lg-danger)] text-white hover:opacity-90 transition-all"
              @click="executeDeleteFloor">
              <Trash2 :size="16" /> Xác nhận xóa
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-header {
  border-bottom: 1px solid var(--border-card);
  background: var(--surface-input);
}

.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.25s ease;
}
.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative.w-full,
.modal-leave-to .relative.w-full {
  transform: scale(0.9) translateY(20px);
}
</style>
