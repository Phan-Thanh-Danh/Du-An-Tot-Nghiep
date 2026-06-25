<script setup>
import { ref, computed, reactive, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  Search, Plus, X, Layers, ChevronDown, ChevronRight, Building,
  Pencil, Trash2, AlertCircle, DoorOpen, Users, Monitor, Tv,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { floorApi } from '@/services/floorApi'
import { buildingApi } from '@/services/buildingApi'
import { roomApi } from '@/services/roomApi'

const loading = ref(true)
const error = ref(null)
const floors = ref([])
const buildings = ref([])
const searchQuery = ref('')
const filterBuildingId = ref('all')
const filterStatus = ref('all')
const expandedFloorId = ref(null)

const roomCache = reactive({})
const roomLoading = reactive({})

const BUILDING_STATUS = [
  { value: 'all', label: 'Tất cả trạng thái' },
  { value: 'active', label: 'Đang hoạt động' },
  { value: 'inactive', label: 'Ngừng hoạt động' },
]

onMounted(async () => {
  await Promise.all([fetchBuildings(), fetchFloors()])
})

async function fetchBuildings() {
  try {
    const res = await buildingApi.list({ PageSize: 200 })
    buildings.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch {
    buildings.value = []
  }
}

async function fetchFloors() {
  loading.value = true
  error.value = null
  try {
    const params = {}
    if (filterBuildingId.value !== 'all') params.MaToaNha = filterBuildingId.value
    const res = await floorApi.list({ ...params, PageSize: 200 })
    floors.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch (e) {
    error.value = e.message || 'Không thể tải danh sách lầu'
  } finally {
    loading.value = false
  }
}

function applyFilter() {
  fetchFloors()
}

const filteredFloors = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return floors.value.filter(f => {
    const matchSearch = !q || f.tenTang?.toLowerCase().includes(q)
    const matchStatus = filterStatus.value === 'all'
      || (filterStatus.value === 'active' && f.conHoatDong !== false)
      || (filterStatus.value === 'inactive' && f.conHoatDong === false)
    return matchSearch && matchStatus
  })
})

function getBuildingName(buildingId) {
  const b = buildings.value.find(x => x.maToaNha === buildingId)
  return b ? b.tenToaNha : ''
}

function getBuildingCode(buildingId) {
  const b = buildings.value.find(x => x.maToaNha === buildingId)
  return b ? b.maCodeToaNha : ''
}

function toggleFloor(floorId) {
  expandedFloorId.value = expandedFloorId.value === floorId ? null : floorId
  if (expandedFloorId.value && !roomCache[`floor-${floorId}`]) {
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
  hoat_dong: { label: 'Đang hoạt động', dot: 'bg-[var(--lg-success)]' },
  bao_tri: { label: 'Bảo trì', dot: 'bg-[var(--lg-warning)]' },
  ngung_hoat_dong: { label: 'Ngừng hoạt động', dot: 'bg-[var(--lg-danger)]' },
}

function getRoomStatusInfo(s) {
  return ROOM_STATUS_MAP[s] || ROOM_STATUS_MAP.ngung_hoat_dong
}

const TYPE_ICON_MAP = { thuc_hanh: Monitor, hoi_truong: Tv, phong_thi_nghiem: Monitor, lab: Monitor }
function getRoomTypeIcon(t) {
  return TYPE_ICON_MAP[t] || DoorOpen
}

// ── Floor CRUD ──
const showFloorModal = ref(false)
const editingFloor = ref(null)
const floorForm = reactive({
  maToaNha: null,
  tenTang: '',
  thuTuTang: null,
  moTa: '',
})
const floorErrors = reactive({})
const isSavingFloor = ref(false)

function openAddFloor() {
  editingFloor.value = null
  Object.assign(floorForm, {
    maToaNha: filterBuildingId.value !== 'all' ? Number(filterBuildingId.value) : null,
    tenTang: '',
    thuTuTang: null,
    moTa: '',
  })
  Object.keys(floorErrors).forEach(k => delete floorErrors[k])
  showFloorModal.value = true
}

function openEditFloor(f) {
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
  if (!floorForm.maToaNha) floorErrors.maToaNha = 'Vui lòng chọn tòa nhà'
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
    await fetchFloors()
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
    await fetchFloors()
  } catch (e) {
    error.value = e.message || 'Không thể xóa lầu'
    confirmDeleteFloor.value = null
  }
}

const router = useRouter()

function navigateToRooms(floorId) {
  router.push({ path: '/staff/rooms', query: { floorId } })
}
</script>

<template>
  <PageContainer
    title="Quản lý lầu"
    subtitle="Danh sách lầu trong các tòa nhà và phòng học trực thuộc."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2" @click="openAddFloor">
        <Plus :size="18" /> Thêm lầu
      </button>
    </template>

    <div class="space-y-4">
      <!-- Search & Filter -->
      <div class="lg-glass-strong p-4 rounded-2xl flex flex-wrap items-center gap-3">
        <div class="flex-1 min-w-[240px] relative">
          <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
          <input v-model="searchQuery" type="text" placeholder="Tìm tên lầu..."
            class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium" />
          <button v-if="searchQuery" class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
            @click="searchQuery = ''"><X :size="14" /></button>
        </div>
        <select v-model="filterBuildingId" @change="applyFilter" class="lg-input px-3 py-2.5 text-sm font-bold min-w-[160px]">
          <option value="all">Tất cả tòa nhà</option>
          <option v-for="b in buildings" :key="b.maToaNha" :value="b.maToaNha">
            {{ b.tenToaNha }} ({{ b.maCodeToaNha }})
          </option>
        </select>
        <select v-model="filterStatus" class="lg-input px-3 py-2.5 text-sm font-bold">
          <option v-for="o in BUILDING_STATUS" :key="o.value" :value="o.value">{{ o.label }}</option>
        </select>
      </div>

      <!-- Loading / Error -->
      <div v-if="loading" class="flex items-center justify-center py-16">
        <div class="h-8 w-8 border-2 border-[var(--lg-primary)] border-t-transparent rounded-full animate-spin"></div>
        <span class="ml-3 text-sm text-label">Đang tải...</span>
      </div>

      <div v-else-if="error" class="lg-glass-strong p-6 rounded-2xl text-center">
        <p class="text-sm text-[var(--lg-danger)] font-semibold">{{ error }}</p>
        <button class="mt-3 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="fetchFloors">Thử lại</button>
      </div>

      <div v-else-if="filteredFloors.length === 0" class="flex flex-col items-center justify-center py-20 text-center">
        <div class="h-16 w-16 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
          <Layers :size="28" class="text-placeholder" />
        </div>
        <p class="text-base font-semibold text-heading">Không tìm thấy lầu</p>
        <p class="text-sm text-label mt-1">Thử thay đổi tòa nhà hoặc từ khóa tìm kiếm.</p>
      </div>

      <!-- Floor List -->
      <div v-else class="space-y-2">
        <div v-for="f in filteredFloors" :key="f.maTang"
          class="lg-card surface-card border border-default rounded-2xl overflow-hidden transition-all">

          <!-- Floor Header -->
          <div class="flex items-center gap-3 p-4 cursor-pointer hover:bg-[var(--surface-input)] transition-colors select-none"
            @click="toggleFloor(f.maTang)">
            <button class="h-8 w-8 rounded-xl surface-input flex items-center justify-center text-label shrink-0">
              <component :is="expandedFloorId === f.maTang ? ChevronDown : ChevronRight" :size="18" />
            </button>
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
              <Layers :size="20" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <h3 class="text-base font-semibold text-heading">{{ f.tenTang }}</h3>
                <span v-if="f.thuTuTang" class="text-xs text-placeholder">· Thứ tự {{ f.thuTuTang }}</span>
                <span class="text-[10px] px-2 py-0.5 rounded font-bold"
                  :class="f.conHoatDong !== false
                    ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
                    : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]'">
                  {{ f.conHoatDong !== false ? 'Hoạt động' : 'Ngừng' }}
                </span>
              </div>
              <p class="text-xs text-label mt-0.5 flex items-center gap-1.5">
                <Building :size="11" />
                {{ getBuildingName(f.maToaNha) }}
                <span class="font-mono text-[10px] text-placeholder">({{ getBuildingCode(f.maToaNha) }})</span>
                <span v-if="f.moTa" class="text-placeholder">· {{ f.moTa }}</span>
              </p>
            </div>
            <div class="flex items-center gap-2 shrink-0">
              <button class="p-2 hover:bg-[var(--surface-input-focus)] rounded-xl text-label transition-colors"
                @click.stop="openEditFloor(f)">
                <Pencil :size="15" />
              </button>
              <button class="p-2 hover:bg-[var(--color-danger-bg)] rounded-xl text-[var(--lg-danger)]/70 hover:text-[var(--lg-danger)] transition-colors"
                @click.stop="requestDeleteFloor(f)">
                <Trash2 :size="15" />
              </button>
            </div>
          </div>

          <!-- Rooms Section -->
          <Transition name="slide-down">
            <div v-if="expandedFloorId === f.maTang" class="border-t border-default p-4 bg-[var(--surface-input)]/30">
              <div v-if="roomLoading[`floor-${f.maTang}`]" class="flex items-center gap-2 py-4 text-sm text-label">
                <div class="h-4 w-4 border-2 border-[var(--lg-primary)] border-t-transparent rounded-full animate-spin"></div>
                Đang tải phòng...
              </div>

              <div v-else-if="!roomCache[`floor-${f.maTang}`] || roomCache[`floor-${f.maTang}`].length === 0"
                class="text-center py-8 text-sm text-placeholder">
                Chưa có phòng học trên lầu này.
                <button class="text-[var(--lg-primary)] font-semibold hover:underline ml-1"
                  @click="navigateToRooms(f.maTang)">
                  Thêm phòng
                </button>
              </div>

              <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                <div v-for="r in roomCache[`floor-${f.maTang}`]" :key="r.maPhong"
                  class="flex items-center gap-3 p-3 rounded-xl surface-card border border-default hover:shadow-sm transition-all">
                  <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
                    <component :is="getRoomTypeIcon(r.loaiPhong)" :size="18" />
                  </div>
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-1.5">
                      <span class="text-sm font-semibold text-heading">{{ r.tenPhong }}</span>
                      <span :class="['h-1.5 w-1.5 rounded-full', getRoomStatusInfo(r.trangThaiPhong).dot]"></span>
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
  </PageContainer>

  <!-- Floor Add/Edit Modal -->
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
                Tòa nhà <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <select v-model="floorForm.maToaNha"
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', floorErrors.maToaNha ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]' : '']">
                <option :value="null" disabled>Chọn tòa nhà</option>
                <option v-for="b in buildings" :key="b.maToaNha" :value="b.maToaNha">
                  {{ b.tenToaNha }} ({{ b.maCodeToaNha }})
                </option>
              </select>
              <p v-if="floorErrors.maToaNha" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ floorErrors.maToaNha }}</p>
            </div>

            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Tên lầu <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input v-model="floorForm.tenTang" type="text" placeholder="VD: Tầng 1"
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

  <!-- Delete Confirmation Modal -->
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
