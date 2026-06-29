<script setup>
import { ref, computed, reactive, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Search,
  Plus,
  MapPin,
  Users,
  Monitor,
  Hammer,
  MoreVertical,
  History,
  X,
  SlidersHorizontal,
  Tv,
  Pencil,
  Trash2,
  AlertCircle,
  Calendar,
  Clock,
  Lightbulb,
  DoorOpen
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { roomApi } from '@/services/roomApi'
import { buildingApi } from '@/services/buildingApi'
import { floorApi } from '@/services/floorApi'

const loading = ref(true)
const error = ref(null)
const rooms = ref([])
const buildings = ref([])
const floors = ref([])

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const filterType    = ref('all')
const filterStatus  = ref('all')
const filterBuildingId = ref('all')
const filterFloorId = ref('all')
const showFilterPanel = ref(false)

const ROOM_TYPES = ['ly_thuyet', 'thuc_hanh', 'phong_thi_nghiem', 'lab', 'hoi_truong', 'truc_tuyen', 'khac']
const STATUSES = ['hoat_dong', 'bao_tri', 'ngung_hoat_dong']

const ROOM_TYPE_LABEL = {
  ly_thuyet: 'Lý thuyết',
  thuc_hanh: 'Thực hành',
  phong_thi_nghiem: 'Phòng thí nghiệm',
  lab: 'Lab',
  hoi_truong: 'Hội trường',
  truc_tuyen: 'Trực tuyến',
  khac: 'Khác'
}

async function fetchBuildings() {
  try {
    const res = await buildingApi.list({ PageSize: 200 })
    buildings.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch {
    buildings.value = []
  }
}

async function fetchFloors(buildingId) {
  try {
    const params = buildingId && buildingId !== 'all' ? { MaToaNha: buildingId } : { PageSize: 500 }
    const res = await floorApi.list(params)
    floors.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch {
    floors.value = []
  }
}

watch(filterBuildingId, (val) => {
  filterFloorId.value = 'all'
  fetchFloors(val)
})

const floorsForBuilding = computed(() => {
  return filterBuildingId.value === 'all'
    ? floors.value
    : floors.value.filter(f => f.maToaNha === Number(filterBuildingId.value))
})

onMounted(async () => {
  const route = useRoute()
  await fetchBuildings()
  if (route.query.buildingId) {
    filterBuildingId.value = String(route.query.buildingId)
    await fetchFloors(route.query.buildingId)
    if (route.query.floorId) filterFloorId.value = String(route.query.floorId)
  } else {
    await fetchFloors()
  }
  await fetchRooms()
  handleConflictSuggestion()
})

async function fetchRooms() {
  loading.value = true
  error.value = null
  try {
    const params = {}
    if (filterBuildingId.value !== 'all') params.BuildingId = filterBuildingId.value
    if (filterFloorId.value !== 'all') params.FloorId = filterFloorId.value
    const res = await roomApi.list({ ...params, PageSize: 500 })
    rooms.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  } catch (e) {
    error.value = e.message || 'Không thể tải danh sách phòng'
  } finally {
    loading.value = false
  }
}

const filteredRooms = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return rooms.value.filter(r => {
    const matchSearch = !q || r.tenPhong?.toLowerCase().includes(q) || r.maCodePhong?.toLowerCase().includes(q)
    const matchType   = filterType.value   === 'all' || r.loaiPhong === filterType.value
    const matchStatus = filterStatus.value === 'all' || r.trangThaiPhong === filterStatus.value
    return matchSearch && matchType && matchStatus
  })
})

const activeFilterCount = computed(() => {
  let count = 0
  if (filterType.value        !== 'all') count++
  if (filterStatus.value      !== 'all') count++
  if (filterBuildingId.value  !== 'all') count++
  if (filterFloorId.value     !== 'all') count++
  return count
})

function clearFilters() {
  filterType.value   = 'all'
  filterStatus.value = 'all'
  filterBuildingId.value = 'all'
  filterFloorId.value = 'all'
  searchQuery.value = ''
  fetchRooms()
}

// ── Status helpers ───────────────────────────────────────────
const STATUS_MAP = {
  hoat_dong:      { label: 'Đang hoạt động', dot: 'bg-(--lg-success)', badge: 'lg-badge lg-badge-success', pulse: true },
  bao_tri:        { label: 'Bảo trì',        dot: 'bg-(--lg-warning)', badge: 'lg-badge lg-badge-warning', pulse: false },
  ngung_hoat_dong: { label: 'Ngừng hoạt động',dot: 'bg-(--lg-danger)',  badge: 'lg-badge border-(--color-danger-bg) bg-(--color-danger-bg) text-(--color-danger-text)', pulse: false }
}
const getStatusInfo = s => STATUS_MAP[s] || STATUS_MAP.ngung_hoat_dong

const TYPE_ICON_MAP = { thuc_hanh: Monitor, hoi_truong: Tv, phong_thi_nghiem: Monitor, lab: Monitor }
const getRoomIcon = t => TYPE_ICON_MAP[t] || DoorOpen

// ── Add Room Modal ───────────────────────────────────────────
const showAddModal  = ref(false)
const isSubmitting  = ref(false)
const addErrors     = reactive({})

const defaultForm = () => ({
  maDonVi: 1,
  maToaNha: null,
  maTang: null,
  maCodePhong: '',
  tenPhong: '',
  sucChua: '',
  loaiPhong: 'ly_thuyet',
  trangThaiPhong: 'hoat_dong',
  ghiChu: ''
})

const newRoom = reactive(defaultForm())

function openAddModal() {
  Object.assign(newRoom, defaultForm())
  Object.keys(addErrors).forEach(k => delete addErrors[k])
  showAddModal.value = true
}

function closeAddModal() {
  showAddModal.value = false
}

function validateRoom() {
  Object.keys(addErrors).forEach(k => delete addErrors[k])
  if (!newRoom.maCodePhong.trim()) addErrors.maCodePhong = 'Mã phòng không được để trống'
  if (!newRoom.tenPhong.trim()) addErrors.tenPhong = 'Tên phòng không được để trống'
  if (!newRoom.sucChua || isNaN(newRoom.sucChua) || +newRoom.sucChua <= 0)
    addErrors.sucChua = 'Sức chứa phải là số dương'
  return Object.keys(addErrors).length === 0
}

async function submitAddRoom() {
  if (!validateRoom()) return
  isSubmitting.value = true
  try {
    await roomApi.create({
      maDonVi: newRoom.maDonVi,
      maToaNha: Number(newRoom.maToaNha),
      maTang: Number(newRoom.maTang),
      maCodePhong: newRoom.maCodePhong,
      tenPhong: newRoom.tenPhong,
      sucChua: Number(newRoom.sucChua),
      loaiPhong: newRoom.loaiPhong,
      ghiChu: newRoom.ghiChu || null
    })
    closeAddModal()
    await fetchRooms()
  } catch (e) {
    addErrors._api = e.message || 'Lỗi khi tạo phòng'
  } finally {
    isSubmitting.value = false
  }
}

// ── Edit Room Modal ──────────────────────────────────────────
const showEditModal = ref(false)
const editingRoom = ref(null)
const editErrors = reactive({})

const addFloorOptions = computed(() => {
  if (!newRoom.maToaNha) return []
  return floors.value.filter(f => f.maToaNha === Number(newRoom.maToaNha))
})

const editFloorOptions = computed(() => {
  if (!editingRoom.value?.maToaNha) return []
  return floors.value.filter(f => f.maToaNha === Number(editingRoom.value.maToaNha))
})

function openEditModal(room) {
  editingRoom.value = {
    ...room,
    maToaNha: room.maToaNha,
    maTang: room.maTang
  }
  Object.keys(editErrors).forEach(k => delete editErrors[k])
  showEditModal.value = true
}

function closeEditModal() {
  showEditModal.value = false
  editingRoom.value = null
}

function validateEdit() {
  Object.keys(editErrors).forEach(k => delete editErrors[k])
  if (!editingRoom.value.tenPhong.trim()) editErrors.tenPhong = 'Tên phòng không được để trống'
  if (!editingRoom.value.sucChua || isNaN(editingRoom.value.sucChua) || +editingRoom.value.sucChua <= 0)
    editErrors.sucChua = 'Sức chứa phải là số dương'
  return Object.keys(editErrors).length === 0
}

async function submitEditRoom() {
  if (!validateEdit()) return
  isSubmitting.value = true
  try {
    await roomApi.update(editingRoom.value.maPhong, {
      maDonVi: editingRoom.value.maDonVi,
      maToaNha: Number(editingRoom.value.maToaNha),
      maTang: Number(editingRoom.value.maTang),
      maCodePhong: editingRoom.value.maCodePhong,
      tenPhong: editingRoom.value.tenPhong,
      sucChua: Number(editingRoom.value.sucChua),
      loaiPhong: editingRoom.value.loaiPhong,
      trangThaiPhong: editingRoom.value.trangThaiPhong,
      ghiChu: editingRoom.value.ghiChu || null
    })
    closeEditModal()
    await fetchRooms()
  } catch (e) {
    editErrors._api = e.message || 'Lỗi khi cập nhật phòng'
  } finally {
    isSubmitting.value = false
  }
}

// ── Delete confirmation ──────────────────────────────────────
const confirmDelete = ref(null)

function requestDelete(room) {
  confirmDelete.value = room
}

async function executeDelete() {
  if (!confirmDelete.value) return
  try {
    await roomApi.delete(confirmDelete.value.maPhong)
    confirmDelete.value = null
    await fetchRooms()
  } catch (e) {
    error.value = e.message || 'Không thể xóa phòng'
    confirmDelete.value = null
  }
}

// ── Mark maintenance ─────────────────────────────────────────
async function markMaintenance(room) {
  try {
    await roomApi.update(room.maPhong, {
      maDonVi: room.maDonVi,
      maToaNha: room.maToaNha,
      maTang: room.maTang,
      maCodePhong: room.maCodePhong,
      tenPhong: room.tenPhong,
      sucChua: room.sucChua,
      loaiPhong: room.loaiPhong,
      trangThaiPhong: 'bao_tri',
      ghiChu: room.ghiChu
    })
    await fetchRooms()
  } catch {
    // silent
  } finally {
    menuOpenId.value = null
  }
}

// ── Usage History Modal ──────────────────────────────────────
const showHistoryModal = ref(false)
const historyRoom = ref(null)

function openHistoryModal(room) {
  historyRoom.value = room
  showHistoryModal.value = true
}

function closeHistoryModal() {
  showHistoryModal.value = false
  historyRoom.value = null
}

// ── Context menu ─────────────────────────────────────────────
const menuOpenId = ref(null)
function toggleMenu(id) { menuOpenId.value = menuOpenId.value === id ? null : id }
function closeMenu()    { menuOpenId.value = null }

// ── Conflict Suggestion Handling ─────────────────────────────
const highlightedRoomId = ref('')
const suggestedInfo = ref(null)
const route = useRoute()
const router = useRouter()

function findRoomById(id) {
  const nid = Number(id)
  return rooms.value.find(r => r.maPhong === nid || r.maCodePhong === id)
}

function handleConflictSuggestion() {
  if (route.query.action === 'change-room' && route.query.roomId) {
    if (route.query.autoApply === 'true' && route.query.suggestedRoom) {
      const oldRoom = findRoomById(route.query.roomId)
      if (oldRoom) {
        oldRoom.tenPhong = route.query.suggestedRoom
      }
      router.replace('/staff/conflicts')
      return
    }
    highlightedRoomId.value = route.query.roomId
    const oldRoom = findRoomById(route.query.roomId)
    if (oldRoom) {
      suggestedInfo.value = {
        oldRoomId: oldRoom.maPhong,
        oldRoomName: oldRoom.tenPhong,
        suggestedRoom: route.query.suggestedRoom || ''
      }
    }
  }
}

function applySuggestedRoom() {
  if (suggestedInfo.value) {
    const oldRoom = findRoomById(String(suggestedInfo.value.oldRoomId))
    if (oldRoom) {
      oldRoom.tenPhong = suggestedInfo.value.suggestedRoom
      console.log(`Đã áp dụng đổi phòng thành công! Phòng ${suggestedInfo.value.oldRoomId} đã đổi tên thành ${suggestedInfo.value.suggestedRoom}.`)
    }
    clearSuggestion()
  }
}

function clearSuggestion() {
  suggestedInfo.value = null
  highlightedRoomId.value = ''
}

// ── Helper for building/floor names ─────────────────────────
function getBuildingName(id) {
  const b = buildings.value.find(x => x.maToaNha === Number(id))
  return b ? b.tenToaNha : ''
}

function getFloorName(id) {
  const f = floors.value.find(x => x.maTang === Number(id))
  return f ? f.tenTang : ''
}

function applyFilterAndReload() {
  fetchRooms()
}
</script>

<template>
  <PageContainer
    title="Quản lý phòng học"
    subtitle="Theo dõi sức chứa, thiết bị và tình trạng sử dụng của các phòng học."
  >
    <template #actions>
      <button
        id="btn-add-room"
        class="lg-button-primary px-5 py-2.5 text-sm font-bold flex items-center gap-2"
        @click="openAddModal"
      >
        <Plus :size="18" /> Thêm phòng mới
      </button>
    </template>

    <div class="space-y-4" @click="closeMenu">

      <!-- Banner gợi ý giải quyết xung đột phòng học -->
      <div v-if="suggestedInfo" class="lg-glass-strong p-4 rounded-2xl border border-(--border-input-focus) bg-(--color-info-bg)/25 flex items-center justify-between gap-4 transition-all">
        <div class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-(--surface-input) flex items-center justify-center text-(--lg-primary) border border-default">
            <Lightbulb :size="20" />
          </div>
          <div>
            <h4 class="text-sm font-bold text-heading">Đề xuất đổi phòng học</h4>
            <p class="text-xs text-label mt-0.5">
              Phòng <strong class="text-heading">{{ suggestedInfo.oldRoomName }}</strong> đang bị trùng lịch dạy. Đề xuất đổi sang phòng <strong class="text-heading">{{ suggestedInfo.suggestedRoom }}</strong>.
            </p>
          </div>
        </div>
        <div class="flex items-center gap-2">
          <button @click="applySuggestedRoom" class="lg-button-primary px-4 py-2 text-xs font-bold shadow-md shadow-(--lg-primary)/10">
            Áp dụng đổi
          </button>
          <button @click="clearSuggestion" class="p-2 hover:bg-(--surface-input) rounded-lg text-placeholder transition-colors">
            <X :size="16" />
          </button>
        </div>
      </div>

      <!-- ── Stats Row ─────────────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div v-for="stat in [
          { label: 'Tổng phòng',      value: rooms.length,                                                   color: 'text-(--color-info-text)',    bg: 'bg-(--color-info-bg)',    border: 'border-(--color-info-bg)' },
          { label: 'Đang hoạt động',  value: rooms.filter(r=>r.trangThaiPhong==='hoat_dong').length,          color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)', border: 'border-(--color-success-bg)' },
          { label: 'Đang bảo trì',    value: rooms.filter(r=>r.trangThaiPhong==='bao_tri').length,            color: 'text-(--color-warning-text)', bg: 'bg-(--color-warning-bg)', border: 'border-(--color-warning-bg)' },
          { label: 'Ngừng hoạt động', value: rooms.filter(r=>r.trangThaiPhong==='ngung_hoat_dong').length,    color: 'text-(--color-danger-text)',  bg: 'bg-(--color-danger-bg)',  border: 'border-(--color-danger-bg)' },
        ]" :key="stat.label"
          :class="['rounded-2xl p-4 border border-default', stat.bg, stat.border]"
        >
          <p class="text-[11px] font-semibold uppercase tracking-widest text-placeholder">{{ stat.label }}</p>
          <p :class="['text-2xl font-semibold mt-1', stat.color]">{{ stat.value }}</p>
        </div>
      </div>

      <!-- ── Search & Filter Bar ───────────────────────── -->
      <div class="lg-glass-strong p-4 rounded-2xl space-y-3">
        <div class="flex flex-wrap items-center gap-3">
          <!-- Search -->
          <div class="flex-1 min-w-[240px] relative">
            <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
            <input
              id="input-search-rooms"
              v-model="searchQuery"
              type="text"
              placeholder="Tìm tên phòng hoặc mã phòng..."
              class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium transition-all"
            />
            <button
              v-if="searchQuery"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <!-- Quick selects -->
          <select
            id="select-filter-type"
            v-model="filterType"
            class="lg-input px-3 py-2.5 text-sm font-bold"
          >
            <option value="all">Tất cả loại</option>
            <option v-for="t in ROOM_TYPES" :key="t" :value="t">{{ ROOM_TYPE_LABEL[t] || t }}</option>
          </select>

          <select
            id="select-filter-building"
            v-model="filterBuildingId"
            class="lg-input px-3 py-2.5 text-sm font-bold min-w-[140px]"
            @change="applyFilterAndReload"
          >
            <option value="all">Tất cả tòa nhà</option>
            <option v-for="b in buildings" :key="b.maToaNha" :value="String(b.maToaNha)">
              {{ b.tenToaNha }}
            </option>
          </select>

          <select
            id="select-filter-floor"
            v-model="filterFloorId"
            class="lg-input px-3 py-2.5 text-sm font-bold min-w-[120px]"
            @change="applyFilterAndReload"
          >
            <option value="all">Tất cả lầu</option>
            <option v-for="f in floorsForBuilding" :key="f.maTang" :value="String(f.maTang)">
              {{ f.tenTang }}
            </option>
          </select>

          <!-- Advanced filter toggle -->
          <button
            id="btn-toggle-filter"
            :class="[
              'flex items-center gap-2 px-4 py-2.5 text-sm font-bold rounded-xl transition-all',
              showFilterPanel
                ? 'lg-button-primary'
                : 'lg-button-secondary text-body'
            ]"
            @click.stop="showFilterPanel = !showFilterPanel"
          >
            <SlidersHorizontal :size="16" />
            Bộ lọc
            <span
              v-if="activeFilterCount > 0"
              class="inline-flex items-center justify-center h-4 w-4 rounded-full bg-(--lg-success) text-white text-[10px] font-semibold"
            >{{ activeFilterCount }}</span>
          </button>

          <button
            v-if="activeFilterCount > 0 || searchQuery"
            class="text-xs font-bold text-placeholder hover:text-(--lg-danger) transition-colors"
            @click="clearFilters()"
          >Xóa bộ lọc</button>
        </div>

        <!-- Advanced filter panel -->
        <Transition name="slide-down">
          <div v-if="showFilterPanel" class="pt-3 border-t border-default flex flex-wrap gap-4">
            <div>
              <p class="text-[10px] font-semibold uppercase tracking-widest text-placeholder mb-2">Trạng thái</p>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="s in ['all', ...STATUSES]" :key="s"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    filterStatus === s
                      ? 'lg-button-primary'
                      : 'lg-button-secondary text-body'
                  ]"
                  @click="filterStatus = s"
                >
                  {{ s === 'all' ? 'Tất cả' : getStatusInfo(s).label }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- ── Loading / Error / Result ─────────────────────────── -->
      <div v-if="loading" class="flex items-center justify-center py-16">
        <div class="h-8 w-8 border-2 border-(--lg-primary) border-t-transparent rounded-full animate-spin"></div>
        <span class="ml-3 text-sm text-label">Đang tải...</span>
      </div>

      <div v-else-if="error" class="lg-glass-strong p-6 rounded-2xl text-center">
        <p class="text-sm text-(--lg-danger) font-semibold">{{ error }}</p>
        <button class="mt-3 text-sm font-bold text-(--lg-primary) hover:underline" @click="fetchRooms">Thử lại</button>
      </div>

      <div v-else class="flex items-center justify-between px-1">
        <p class="text-sm text-label font-semibold">
          Hiển thị <span class="font-semibold text-heading">{{ filteredRooms.length }}</span> / {{ rooms.length }} phòng
        </p>
      </div>

      <!-- ── Rooms Grid ─────────────────────────────────── -->
      <TransitionGroup name="room-list" tag="div" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <div
          v-for="room in filteredRooms"
          :key="room.maPhong"
          :class="[
            'lg-card surface-card group p-4 transition-all hover:-translate-y-0.5 hover:shadow-sm cursor-pointer relative',
            highlightedRoomId === String(room.maPhong) ? '!border-(--lg-primary) ring-2 ring-(--lg-primary)/20 shadow-md' : 'border-default'
          ]"
        >
          <!-- Top glow accent -->
          <div class="absolute top-0 left-0 right-0 h-0.5 bg-(--border-input-focus) opacity-0 group-hover:opacity-100 transition-opacity rounded-t-2xl"></div>

          <!-- Status dot + context menu -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center gap-2">
              <span :class="['h-2 w-2 rounded-full', getStatusInfo(room.trangThaiPhong).dot, getStatusInfo(room.trangThaiPhong).pulse ? 'animate-pulse' : '']"></span>
              <span class="text-[10px] font-semibold uppercase tracking-widest text-placeholder">{{ getStatusInfo(room.trangThaiPhong).label }}</span>
            </div>
            <div class="relative">
              <button
                class="p-1.5 hover:bg-(--surface-input-focus) rounded-lg text-placeholder transition-all"
                @click.stop="toggleMenu(room.maPhong)"
              >
                <MoreVertical :size="16" />
              </button>
              <Transition name="fade-up">
                <div
                  v-if="menuOpenId === room.maPhong"
                  class="absolute right-0 top-8 z-[100] surface-solid border border-default rounded-2xl shadow-sm w-44 py-1 overflow-hidden"
                  @click.stop
                >
                  <button
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-(--surface-input) transition-colors"
                    @click="openEditModal(room); menuOpenId = null"
                  >
                    <Pencil :size="14" class="text-(--lg-success)" /> Chỉnh sửa
                  </button>
                  <button
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-(--surface-input) transition-colors"
                    @click="openHistoryModal(room); menuOpenId = null"
                  >
                    <History :size="14" class="text-(--lg-info)" /> Lịch sử dùng
                  </button>
                  <button
                    v-if="room.trangThaiPhong !== 'bao_tri'"
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-(--surface-input) transition-colors"
                    @click="markMaintenance(room)"
                  >
                    <Hammer :size="14" class="text-(--lg-warning)" /> Đánh dấu bảo trì
                  </button>
                  <div class="border-t border-default my-1"></div>
                  <button
                    class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-(--lg-danger) hover:bg-(--color-danger-bg) transition-colors"
                    @click="requestDelete(room); menuOpenId = null"
                  >
                    <Trash2 :size="14" /> Xóa phòng
                  </button>
                </div>
              </Transition>
            </div>
          </div>

          <!-- Room icon + name -->
          <div class="flex items-center gap-3">
            <div class="h-12 w-12 rounded-2xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-bg) shrink-0">
              <component :is="getRoomIcon(room.loaiPhong)" :size="22" />
            </div>
            <div>
              <h3 class="text-base font-semibold text-heading leading-tight group-hover:text-(--lg-primary) transition-colors">{{ room.tenPhong }}</h3>
              <p class="text-[10px] font-semibold text-label uppercase tracking-widest flex items-center gap-1 mt-0.5">
                <MapPin :size="10" /> {{ getBuildingName(room.maToaNha) }} · {{ getFloorName(room.maTang) }}
              </p>
            </div>
          </div>

          <!-- Stats -->
          <div class="mt-4 grid grid-cols-2 gap-3">
            <div class="surface-input rounded-xl p-3 border border-default">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Sức chứa</p>
              <div class="flex items-center gap-1.5 mt-1">
                <Users :size="13" class="text-(--lg-primary)" />
                <span class="text-sm font-semibold text-heading">{{ room.sucChua }} SV</span>
              </div>
            </div>
            <div class="surface-input rounded-xl p-3 border border-default">
              <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Loại phòng</p>
              <p class="text-sm font-semibold text-heading mt-1">{{ ROOM_TYPE_LABEL[room.loaiPhong] || room.loaiPhong }}</p>
            </div>
          </div>

          <!-- Location info -->
          <div class="mt-4 surface-input rounded-xl p-3 border border-default">
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest mb-1">Vị trí</p>
            <p class="text-xs font-semibold text-heading">{{ getBuildingName(room.maToaNha) }} - {{ getFloorName(room.maTang) }}</p>
          </div>

          <!-- Footer -->
          <div class="mt-4 pt-4 border-t border-default flex items-center justify-between">
            <span :class="['px-2.5 py-1 text-[10px]', getStatusInfo(room.trangThaiPhong).badge]">
              {{ getStatusInfo(room.trangThaiPhong).label }}
            </span>
            <p class="text-[10px] font-bold text-placeholder uppercase tracking-wide">{{ room.maCodePhong }}</p>
          </div>
        </div>

        <!-- Empty state -->
        <div
          v-if="filteredRooms.length === 0"
          key="empty"
          class="col-span-full flex flex-col items-center justify-center py-20 text-center"
        >
          <div class="h-16 w-16 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <Search :size="28" class="text-placeholder" />
          </div>
          <p class="text-base font-semibold text-heading">Không tìm thấy phòng nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc.</p>
          <button class="mt-4 text-sm font-bold text-(--lg-primary) hover:underline" @click="clearFilters()">Xóa tất cả bộ lọc</button>
        </div>
      </TransitionGroup>
    </div>
  </PageContainer>

  <!-- ════════════════════════════════════════════════════════
       Add Room Modal
  ════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showAddModal"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeAddModal"
      >
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <!-- Modal Panel -->
        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <!-- Header gradient -->
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">Thêm phòng mới</h2>
                <p class="text-sm text-label mt-0.5">Điền thông tin phòng học mới</p>
              </div>
              <button
                class="h-9 w-9 rounded-2xl surface-input hover:bg-(--surface-input-focus) flex items-center justify-center text-label transition-all"
                @click="closeAddModal"
              >
                <X :size="18" />
              </button>
            </div>
          </div>

          <!-- Form body -->
          <div class="p-6 space-y-5">
            <p v-if="addErrors._api" class="text-sm text-(--lg-danger) font-semibold">{{ addErrors._api }}</p>

            <!-- Mã phòng -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Mã phòng <span class="text-(--lg-danger)">*</span>
              </label>
              <input v-model="newRoom.maCodePhong" type="text" placeholder="VD: PH001"
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', addErrors.maCodePhong ? 'border-(--lg-danger) bg-(--color-danger-bg)' : '']" />
              <p v-if="addErrors.maCodePhong" class="mt-1 text-xs text-(--lg-danger) font-semibold">{{ addErrors.maCodePhong }}</p>
            </div>

            <!-- Tên phòng -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Tên phòng <span class="text-(--lg-danger)">*</span>
              </label>
              <input v-model="newRoom.tenPhong" type="text" placeholder="VD: Phòng 305, Lab 3..."
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', addErrors.tenPhong ? 'border-(--lg-danger) bg-(--color-danger-bg)' : '']" />
              <p v-if="addErrors.tenPhong" class="mt-1 text-xs text-(--lg-danger) font-semibold">{{ addErrors.tenPhong }}</p>
            </div>

            <!-- Tòa nhà + Lầu -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Tòa nhà</label>
                <select v-model="newRoom.maToaNha" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option :value="null" disabled>Chọn tòa nhà</option>
                  <option v-for="b in buildings" :key="b.maToaNha" :value="b.maToaNha">{{ b.tenToaNha }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Lầu</label>
                <select v-model="newRoom.maTang" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option :value="null" disabled>Chọn lầu</option>
                  <option v-for="f in addFloorOptions" :key="f.maTang" :value="f.maTang">{{ f.tenTang }}</option>
                </select>
              </div>
            </div>

            <!-- Sức chứa + Loại phòng -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                  Sức chứa (SV) <span class="text-(--lg-danger)">*</span>
                </label>
                <input v-model="newRoom.sucChua" type="number" min="1" placeholder="VD: 40"
                  :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', addErrors.sucChua ? 'border-(--lg-danger) bg-(--color-danger-bg)' : '']" />
                <p v-if="addErrors.sucChua" class="mt-1 text-xs text-(--lg-danger) font-semibold">{{ addErrors.sucChua }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Loại phòng</label>
                <select v-model="newRoom.loaiPhong" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option v-for="t in ROOM_TYPES" :key="t" :value="t">{{ ROOM_TYPE_LABEL[t] || t }}</option>
                </select>
              </div>
            </div>

            <!-- Trạng thái -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-2">Trạng thái</label>
              <div class="flex gap-2 flex-wrap">
                <button v-for="s in STATUSES" :key="s"
                  :class="['flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all',
                    newRoom.trangThaiPhong === s ? 'lg-button-primary' : 'lg-button-secondary text-body']"
                  @click="newRoom.trangThaiPhong = s">
                  <span v-if="newRoom.trangThaiPhong !== s" :class="['h-2 w-2 rounded-full', getStatusInfo(s).dot]"></span>
                  {{ getStatusInfo(s).label }}
                </button>
              </div>
            </div>

            <!-- Ghi chú -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Ghi chú</label>
              <textarea v-model="newRoom.ghiChu" placeholder="(tùy chọn)" rows="2" class="w-full lg-input px-4 py-2.5 text-sm font-medium resize-none"></textarea>
            </div>
          </div>

          <!-- Footer actions -->
          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3 mt-4">
            <button class="lg-button-secondary px-5 py-2.5" @click="closeAddModal">Hủy</button>
            <button id="btn-submit-add-room"
              :class="['lg-button-primary px-6 py-2.5', isSubmitting ? 'opacity-70 cursor-not-allowed' : '']"
              :disabled="isSubmitting" @click="submitAddRoom">
              <span v-if="isSubmitting" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Plus v-else :size="16" />
              {{ isSubmitting ? 'Đang lưu...' : 'Thêm phòng' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
  <!-- ════════════════════════════════════════════════════════════
       Edit Room Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showEditModal && editingRoom"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeEditModal"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">Chỉnh sửa phòng</h2>
                <p class="text-sm text-label mt-0.5">Cập nhật thông tin phòng học</p>
              </div>
              <button
                class="h-9 w-9 rounded-2xl surface-input hover:bg-(--surface-input-focus) flex items-center justify-center text-label transition-all"
                @click="closeEditModal"
              >
                <X :size="18" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <p v-if="editErrors._api" class="text-sm text-(--lg-danger) font-semibold">{{ editErrors._api }}</p>

            <!-- Mã phòng -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Mã phòng</label>
              <input :value="editingRoom.maCodePhong" type="text" disabled class="w-full lg-input px-4 py-2.5 text-sm font-medium opacity-60" />
            </div>

            <!-- Tên phòng -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                Tên phòng <span class="text-(--lg-danger)">*</span>
              </label>
              <input v-model="editingRoom.tenPhong" type="text" placeholder="VD: Phòng 305, Lab 3..."
                :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', editErrors.tenPhong ? 'border-(--lg-danger) bg-(--color-danger-bg)' : '']" />
              <p v-if="editErrors.tenPhong" class="mt-1 text-xs text-(--lg-danger) font-semibold">{{ editErrors.tenPhong }}</p>
            </div>

            <!-- Tòa nhà + Lầu -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Tòa nhà</label>
                <select v-model="editingRoom.maToaNha" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option v-for="b in buildings" :key="b.maToaNha" :value="b.maToaNha">{{ b.tenToaNha }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Lầu</label>
                <select v-model="editingRoom.maTang" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option v-for="f in editFloorOptions" :key="f.maTang" :value="f.maTang">{{ f.tenTang }}</option>
                </select>
              </div>
            </div>

            <!-- Sức chứa + Loại phòng -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">
                  Sức chứa (SV) <span class="text-(--lg-danger)">*</span>
                </label>
                <input v-model="editingRoom.sucChua" type="number" min="1" placeholder="VD: 40"
                  :class="['w-full lg-input px-4 py-2.5 text-sm font-medium', editErrors.sucChua ? 'border-(--lg-danger) bg-(--color-danger-bg)' : '']" />
                <p v-if="editErrors.sucChua" class="mt-1 text-xs text-(--lg-danger) font-semibold">{{ editErrors.sucChua }}</p>
              </div>
              <div>
                <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Loại phòng</label>
                <select v-model="editingRoom.loaiPhong" class="w-full lg-input px-4 py-2.5 text-sm font-bold">
                  <option v-for="t in ROOM_TYPES" :key="t" :value="t">{{ ROOM_TYPE_LABEL[t] || t }}</option>
                </select>
              </div>
            </div>

            <!-- Trạng thái -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-2">Trạng thái</label>
              <div class="flex gap-2 flex-wrap">
                <button v-for="s in STATUSES" :key="s"
                  :class="['flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all',
                    editingRoom.trangThaiPhong === s ? 'lg-button-primary' : 'lg-button-secondary text-body']"
                  @click="editingRoom.trangThaiPhong = s">
                  <span v-if="editingRoom.trangThaiPhong !== s" :class="['h-2 w-2 rounded-full', getStatusInfo(s).dot]"></span>
                  {{ getStatusInfo(s).label }}
                </button>
              </div>
            </div>

            <!-- Ghi chú -->
            <div>
              <label class="block text-xs font-semibold text-label uppercase tracking-widest mb-1.5">Ghi chú</label>
              <textarea v-model="editingRoom.ghiChu" placeholder="(tùy chọn)" rows="2" class="w-full lg-input px-4 py-2.5 text-sm font-medium resize-none"></textarea>
            </div>
          </div>

          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3 mt-4">
            <button class="lg-button-secondary px-5 py-2.5" @click="closeEditModal">Hủy</button>
            <button
              class="lg-button-primary px-6 py-2.5 flex items-center gap-2"
              @click="submitEditRoom"
            >
              <Pencil :size="16" /> Cập nhật
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Delete Confirmation Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="confirmDelete"
        class="fixed inset-0 z-[200] flex items-center justify-center p-4"
        @click.self="confirmDelete = null"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-sm surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="p-6">
            <div class="h-12 w-12 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center mb-4 mx-auto">
              <AlertCircle :size="24" class="text-(--lg-danger)" />
            </div>
            <h3 class="text-lg font-semibold text-heading text-center">Xóa phòng học</h3>
            <p class="text-sm text-label text-center mt-2">
              Bạn có chắc muốn xóa
              <span class="font-semibold text-heading">{{ confirmDelete.tenPhong }}</span>
              ({{ confirmDelete.maCodePhong }})? Phòng sẽ được chuyển sang trạng thái ngừng hoạt động.
            </p>
          </div>
          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3">
            <button class="lg-button-secondary px-5 py-2.5" @click="confirmDelete = null">Hủy</button>
            <button
              class="flex items-center gap-2 px-5 py-2.5 text-sm font-bold rounded-xl bg-(--lg-danger) text-white hover:opacity-90 transition-all"
              @click="executeDelete"
            >
              <Trash2 :size="16" /> Xác nhận xóa
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Usage History Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showHistoryModal && historyRoom"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeHistoryModal"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <div class="modal-header p-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-semibold text-heading">Lịch sử sử dụng</h2>
                <p class="text-sm text-label mt-0.5">{{ historyRoom.tenPhong }} ({{ historyRoom.maCodePhong }})</p>
              </div>
              <button
                class="h-9 w-9 rounded-2xl surface-input hover:bg-(--surface-input-focus) flex items-center justify-center text-label transition-all"
                @click="closeHistoryModal"
              >
                <X :size="18" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-4">
            <div class="flex items-center gap-3 p-4 rounded-xl surface-input border border-default">
              <div class="flex-1 grid grid-cols-3 gap-3 text-center">
                <div>
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Tổng lượt dùng</p>
                  <p class="text-lg font-semibold text-heading mt-1">24</p>
                </div>
                <div>
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Giờ sử dụng</p>
                  <p class="text-lg font-semibold text-heading mt-1">168h</p>
                </div>
                <div>
                  <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Tỉ lệ SD</p>
                  <p class="text-lg font-semibold text-heading mt-1">78%</p>
                </div>
              </div>
            </div>

            <div v-for="item in [
              { date: '08/06/2026', time: '07:00 - 09:30', subject: 'Lập trình Web', teacher: 'TS. Nguyễn Văn A', status: 'completed' },
              { date: '08/06/2026', time: '09:45 - 12:15', subject: 'Cơ sở dữ liệu', teacher: 'ThS. Trần Thị B', status: 'completed' },
              { date: '07/06/2026', time: '13:00 - 15:30', subject: 'CTDL & GT', teacher: 'TS. Lê Văn C', status: 'completed' },
              { date: '07/06/2026', time: '15:45 - 18:15', subject: 'Mạng máy tính', teacher: 'ThS. Phạm Thị D', status: 'cancelled' },
              { date: '06/06/2026', time: '07:00 - 09:30', subject: 'Lập trình Web', teacher: 'TS. Nguyễn Văn A', status: 'completed' },
            ]" :key="item.date + item.time" class="flex items-start gap-3 p-3 rounded-xl surface-input border border-default hover:bg-(--surface-input-focus) transition-colors">
              <div class="h-9 w-9 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) shrink-0">
                <Calendar :size="16" />
              </div>
              <div class="flex-1 min-w-0">
                <div class="flex items-center justify-between">
                  <p class="text-sm font-semibold text-heading">{{ item.subject }}</p>
                  <span
                    :class="[
                      'text-[9px] font-bold uppercase px-2 py-0.5 rounded-lg',
                      item.status === 'completed'
                        ? 'bg-(--color-success-bg) text-(--color-success-text)'
                        : 'bg-(--color-danger-bg) text-(--color-danger-text)'
                    ]"
                  >{{ item.status === 'completed' ? 'Đã học' : 'Đã hủy' }}</span>
                </div>
                <p class="text-xs text-label mt-0.5 flex items-center gap-2">
                  <Clock :size="11" /> {{ item.date }} · {{ item.time }}
                </p>
                <p class="text-xs text-label">{{ item.teacher }}</p>
              </div>
            </div>

            <div v-if="false" class="text-center py-10">
              <p class="text-sm text-label">Chưa có lịch sử sử dụng cho phòng này.</p>
            </div>
          </div>

          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end">
            <button class="lg-button-secondary px-5 py-2.5" @click="closeHistoryModal">Đóng</button>
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

/* Slide-down filter panel */
.slide-down-enter-active,
.slide-down-leave-active {
  transition: all 0.25s ease;
}
.slide-down-enter-from,
.slide-down-leave-to {
  opacity: 0;
  transform: translateY(-8px);
}

/* Room list transition */
.room-list-enter-active,
.room-list-leave-active {
  transition: all 0.3s ease;
}
.room-list-enter-from,
.room-list-leave-to {
  opacity: 0;
  transform: scale(0.95);
}

/* Context menu fade-up */
.fade-up-enter-active,
.fade-up-leave-active {
  transition: all 0.15s ease;
}
.fade-up-enter-from,
.fade-up-leave-to {
  opacity: 0;
  transform: translateY(-6px) scale(0.97);
}

/* Modal transition */
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
