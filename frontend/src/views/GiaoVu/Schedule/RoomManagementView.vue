<script setup>
import { ref, computed, reactive } from 'vue'
import {
  Search,
  Plus,
  MapPin,
  Users,
  Monitor,
  Hammer,
  MoreVertical,
  History,
  Building,
  X,
  SlidersHorizontal,
  Tv,
  Pencil,
  Trash2,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const rooms = ref([
  { id: 'PH001', name: 'Phòng 302', campus: 'Cơ sở chính', floor: '3', capacity: 45, type: 'Lý thuyết', devices: ['Projector', 'Điều hòa'], status: 'active' },
  { id: 'PH002', name: 'Lab 2', campus: 'Cơ sở chính', floor: '2', capacity: 30, type: 'Thực hành', devices: ['Máy tính', 'Server', 'Projector'], status: 'active' },
  { id: 'PH003', name: 'Phòng 105', campus: 'Cơ sở phụ', floor: '1', capacity: 60, type: 'Hội trường', devices: ['Âm thanh', 'Projector'], status: 'maintenance' },
  { id: 'PH004', name: 'Phòng 401', campus: 'Cơ sở chính', floor: '4', capacity: 40, type: 'Lý thuyết', devices: ['Bảng trắng', 'Điều hòa'], status: 'active' },
  { id: 'PH005', name: 'Studio 1', campus: 'Cơ sở chính', floor: '1', capacity: 15, type: 'Chuyên dụng', devices: ['Camera', 'Màn xanh'], status: 'inactive' },
  { id: 'PH006', name: 'Phòng 210', campus: 'Cơ sở phụ', floor: '2', capacity: 50, type: 'Lý thuyết', devices: ['Projector', 'Wifi', 'Điều hòa'], status: 'active' },
])

// ── Search & Filter ──────────────────────────────────────────
const searchQuery = ref('')
const filterType    = ref('all')
const filterStatus  = ref('all')
const filterCampus  = ref('all')
const filterFloor   = ref('all')
const showFilterPanel = ref(false)

const TYPES   = ['Lý thuyết', 'Thực hành', 'Hội trường', 'Chuyên dụng']
const STATUSES = ['active', 'maintenance', 'inactive']
const CAMPUSES = ['Cơ sở chính', 'Cơ sở phụ']
const FLOORS   = ['1', '2', '3', '4', '5']

const filteredRooms = computed(() => {
  return rooms.value.filter(r => {
    const q = searchQuery.value.toLowerCase().trim()
    const matchSearch = !q || r.name.toLowerCase().includes(q) || r.id.toLowerCase().includes(q)
    const matchType   = filterType.value   === 'all' || r.type   === filterType.value
    const matchStatus = filterStatus.value === 'all' || r.status === filterStatus.value
    const matchCampus = filterCampus.value === 'all' || r.campus === filterCampus.value
    const matchFloor  = filterFloor.value  === 'all' || r.floor  === filterFloor.value
    return matchSearch && matchType && matchStatus && matchCampus && matchFloor
  })
})

const activeFilterCount = computed(() => {
  let count = 0
  if (filterType.value   !== 'all') count++
  if (filterStatus.value !== 'all') count++
  if (filterCampus.value !== 'all') count++
  if (filterFloor.value  !== 'all') count++
  return count
})

function clearFilters() {
  filterType.value   = 'all'
  filterStatus.value = 'all'
  filterCampus.value = 'all'
  filterFloor.value  = 'all'
}

// ── Status helpers ───────────────────────────────────────────
const STATUS_MAP = {
  active:      { label: 'Đang hoạt động', dot: 'bg-[var(--lg-success)]', badge: 'lg-badge lg-badge-success', pulse: true },
  maintenance: { label: 'Bảo trì',        dot: 'bg-[var(--lg-warning)]', badge: 'lg-badge lg-badge-warning', pulse: false },
  inactive:    { label: 'Ngừng hoạt động',dot: 'bg-[var(--lg-danger)]',  badge: 'lg-badge border-[var(--color-danger-bg)] bg-[var(--color-danger-bg)] text-[var(--color-danger-text)]', pulse: false },
}
const getStatusInfo = s => STATUS_MAP[s] || STATUS_MAP.inactive

const TYPE_ICON_MAP = { 'Thực hành': Monitor, 'Hội trường': Tv }
const getRoomIcon = t => TYPE_ICON_MAP[t] || Building

// ── Add Room Modal ───────────────────────────────────────────
const showAddModal  = ref(false)
const isSubmitting  = ref(false)
const addErrors     = reactive({})

const deviceOptions = ['Projector', 'Màn hình', 'Điều hòa', 'Wifi', 'Máy tính', 'Server', 'Âm thanh', 'Camera', 'Màn xanh', 'Bảng trắng']

const defaultForm = () => ({
  name:     '',
  campus:   'Cơ sở chính',
  floor:    '1',
  capacity: '',
  type:     'Lý thuyết',
  devices:  [],
  status:   'active',
})

const newRoom = reactive(defaultForm())

function toggleDevice(d) {
  const idx = newRoom.devices.indexOf(d)
  if (idx === -1) newRoom.devices.push(d)
  else newRoom.devices.splice(idx, 1)
}

function openAddModal() {
  Object.assign(newRoom, defaultForm())
  newRoom.devices = []
  Object.keys(addErrors).forEach(k => delete addErrors[k])
  showAddModal.value = true
}

function closeAddModal() {
  showAddModal.value = false
}

function validateRoom() {
  Object.keys(addErrors).forEach(k => delete addErrors[k])
  if (!newRoom.name.trim()) addErrors.name = 'Tên phòng không được để trống'
  if (!newRoom.capacity || isNaN(newRoom.capacity) || +newRoom.capacity <= 0)
    addErrors.capacity = 'Sức chứa phải là số dương'
  return Object.keys(addErrors).length === 0
}

async function submitAddRoom() {
  if (!validateRoom()) return
  isSubmitting.value = true
  // Simulate API delay
  await new Promise(r => setTimeout(r, 700))
  const id = 'PH' + String(rooms.value.length + 1).padStart(3, '0')
  rooms.value.push({ id, ...newRoom, capacity: +newRoom.capacity, devices: [...newRoom.devices] })
  isSubmitting.value = false
  closeAddModal()
}

// ── Context menu ─────────────────────────────────────────────
const menuOpenId = ref(null)
function toggleMenu(id) { menuOpenId.value = menuOpenId.value === id ? null : id }
function closeMenu()    { menuOpenId.value = null }
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

      <!-- ── Stats Row ─────────────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div v-for="stat in [
          { label: 'Tổng phòng',      value: rooms.length,                                          color: 'text-[var(--color-info-text)]',    bg: 'bg-[var(--color-info-bg)]',    border: 'border-[var(--color-info-bg)]' },
          { label: 'Đang hoạt động',  value: rooms.filter(r=>r.status==='active').length,            color: 'text-[var(--color-success-text)]', bg: 'bg-[var(--color-success-bg)]', border: 'border-[var(--color-success-bg)]' },
          { label: 'Đang bảo trì',    value: rooms.filter(r=>r.status==='maintenance').length,       color: 'text-[var(--color-warning-text)]', bg: 'bg-[var(--color-warning-bg)]', border: 'border-[var(--color-warning-bg)]' },
          { label: 'Ngừng hoạt động', value: rooms.filter(r=>r.status==='inactive').length,          color: 'text-[var(--color-danger-text)]',  bg: 'bg-[var(--color-danger-bg)]',  border: 'border-[var(--color-danger-bg)]' },
        ]" :key="stat.label"
          :class="['rounded-2xl p-4 border border-default', stat.bg, stat.border]"
        >
          <p class="text-[11px] font-black uppercase tracking-widest text-placeholder">{{ stat.label }}</p>
          <p :class="['text-2xl font-black mt-1', stat.color]">{{ stat.value }}</p>
        </div>
      </div>

      <!-- ── Search & Filter Bar ───────────────────────── -->
      <div class="lg-glass-strong p-4 rounded-[24px] space-y-3">
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
            <option v-for="t in TYPES" :key="t" :value="t">{{ t }}</option>
          </select>

          <select
            id="select-filter-campus"
            v-model="filterCampus"
            class="lg-input px-3 py-2.5 text-sm font-bold"
          >
            <option value="all">Tất cả cơ sở</option>
            <option v-for="c in CAMPUSES" :key="c" :value="c">{{ c }}</option>
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
              class="inline-flex items-center justify-center h-4 w-4 rounded-full bg-[var(--lg-success)] text-white text-[10px] font-black"
            >{{ activeFilterCount }}</span>
          </button>

          <button
            v-if="activeFilterCount > 0 || searchQuery"
            class="text-xs font-bold text-placeholder hover:text-[var(--lg-danger)] transition-colors"
            @click="clearFilters(); searchQuery = ''"
          >Xóa bộ lọc</button>
        </div>

        <!-- Advanced filter panel -->
        <Transition name="slide-down">
          <div v-if="showFilterPanel" class="pt-3 border-t border-default flex flex-wrap gap-4">
            <div>
              <p class="text-[10px] font-black uppercase tracking-widest text-placeholder mb-2">Trạng thái</p>
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
            <div>
              <p class="text-[10px] font-black uppercase tracking-widest text-placeholder mb-2">Lầu</p>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="f in ['all', ...FLOORS]" :key="f"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    filterFloor === f
                      ? 'lg-button-primary'
                      : 'lg-button-secondary text-body'
                  ]"
                  @click="filterFloor = f"
                >
                  {{ f === 'all' ? 'Tất cả lầu' : 'Lầu ' + f }}
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </div>

      <!-- ── Result count ──────────────────────────────── -->
      <div class="flex items-center justify-between px-1">
        <p class="text-sm text-label font-semibold">
          Hiển thị <span class="font-black text-heading">{{ filteredRooms.length }}</span> / {{ rooms.length }} phòng
        </p>
      </div>

      <!-- ── Rooms Grid ─────────────────────────────────── -->
      <TransitionGroup name="room-list" tag="div" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <div
          v-for="room in filteredRooms"
          :key="room.id"
          class="lg-card lg-glass group p-5 transition-all hover:-translate-y-1 hover:shadow-xl cursor-pointer"
        >
          <!-- Top glow accent -->
          <div class="absolute top-0 left-0 right-0 h-0.5 bg-gradient-to-r from-[var(--lg-cyan)] to-[var(--lg-primary)] opacity-0 group-hover:opacity-100 transition-opacity rounded-t-[28px]"></div>

          <!-- Status dot + context menu -->
          <div class="flex items-center justify-between mb-4">
            <div class="flex items-center gap-2">
              <span :class="['h-2 w-2 rounded-full', getStatusInfo(room.status).dot, getStatusInfo(room.status).pulse ? 'animate-pulse' : '']"></span>
              <span class="text-[10px] font-black uppercase tracking-widest text-placeholder">{{ getStatusInfo(room.status).label }}</span>
            </div>
            <div class="relative">
              <button
                class="p-1.5 hover:bg-[var(--surface-input-focus)] rounded-lg text-placeholder transition-all"
                @click.stop="toggleMenu(room.id)"
              >
                <MoreVertical :size="16" />
              </button>
              <Transition name="fade-up">
                <div
                  v-if="menuOpenId === room.id"
                  class="absolute right-0 top-8 z-[100] surface-solid border border-default rounded-2xl shadow-xl w-44 py-1 overflow-hidden"
                  @click.stop
                >
                  <button class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-[var(--surface-input)] transition-colors">
                    <Pencil :size="14" class="text-[var(--lg-success)]" /> Chỉnh sửa
                  </button>
                  <button class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-[var(--surface-input)] transition-colors">
                    <History :size="14" class="text-[var(--lg-info)]" /> Lịch sử dùng
                  </button>
                  <button class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-body hover:bg-[var(--surface-input)] transition-colors">
                    <Hammer :size="14" class="text-[var(--lg-warning)]" /> Đánh dấu bảo trì
                  </button>
                  <div class="border-t border-default my-1"></div>
                  <button class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-[var(--lg-danger)] hover:bg-[var(--color-danger-bg)] transition-colors">
                    <Trash2 :size="14" /> Xóa phòng
                  </button>
                </div>
              </Transition>
            </div>
          </div>

          <!-- Room icon + name -->
          <div class="flex items-center gap-3">
            <div class="h-12 w-12 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] border border-[var(--color-info-bg)] shrink-0">
              <component :is="getRoomIcon(room.type)" :size="22" />
            </div>
            <div>
              <h3 class="text-base font-black text-heading leading-tight group-hover:text-[var(--lg-primary)] transition-colors">{{ room.name }}</h3>
              <p class="text-[10px] font-bold text-label uppercase tracking-widest flex items-center gap-1 mt-0.5">
                <MapPin :size="10" /> {{ room.campus }} · Lầu {{ room.floor }}
              </p>
            </div>
          </div>

          <!-- Stats -->
          <div class="mt-4 grid grid-cols-2 gap-3">
            <div class="surface-input rounded-xl p-3 border border-default">
              <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Sức chứa</p>
              <div class="flex items-center gap-1.5 mt-1">
                <Users :size="13" class="text-[var(--lg-primary)]" />
                <span class="text-sm font-black text-heading">{{ room.capacity }} SV</span>
              </div>
            </div>
            <div class="surface-input rounded-xl p-3 border border-default">
              <p class="text-[9px] font-black text-placeholder uppercase tracking-widest">Loại phòng</p>
              <p class="text-sm font-black text-heading mt-1">{{ room.type }}</p>
            </div>
          </div>

          <!-- Devices -->
          <div class="mt-4">
            <p class="text-[9px] font-black text-placeholder uppercase tracking-widest mb-2">Thiết bị</p>
            <div class="flex flex-wrap gap-1.5">
              <span
                v-for="device in room.devices"
                :key="device"
                class="px-2 py-0.5 rounded-lg surface-solid border border-default text-[10px] font-bold text-label shadow-sm"
              >{{ device }}</span>
              <span v-if="room.devices.length === 0" class="text-[10px] text-placeholder italic">Chưa có thiết bị</span>
            </div>
          </div>

          <!-- Footer -->
          <div class="mt-4 pt-4 border-t border-default flex items-center justify-between">
            <span :class="['px-2.5 py-1 text-[10px]', getStatusInfo(room.status).badge]">
              {{ getStatusInfo(room.status).label }}
            </span>
            <p class="text-[10px] font-bold text-placeholder uppercase tracking-wide">{{ room.id }}</p>
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
          <p class="text-base font-black text-heading">Không tìm thấy phòng nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa hoặc điều chỉnh bộ lọc.</p>
          <button class="mt-4 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="clearFilters(); searchQuery = ''">Xóa tất cả bộ lọc</button>
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
        <div class="relative w-full max-w-lg surface-modal rounded-[28px] shadow-2xl overflow-hidden max-h-[90vh] overflow-y-auto border border-default">
          <!-- Header gradient -->
          <div class="bg-gradient-to-r from-[var(--lg-cyan)] to-[var(--lg-primary)] p-6 pb-5">
            <div class="flex items-center justify-between">
              <div>
                <h2 class="text-xl font-black text-white">Thêm phòng mới</h2>
                <p class="text-sm text-white/80 mt-0.5">Điền thông tin phòng học mới</p>
              </div>
              <button
                class="h-9 w-9 rounded-2xl bg-white/20 hover:bg-white/30 flex items-center justify-center text-white transition-all"
                @click="closeAddModal"
              >
                <X :size="18" />
              </button>
            </div>
          </div>

          <!-- Form body -->
          <div class="p-6 space-y-5">

            <!-- Tên phòng -->
            <div>
              <label class="block text-xs font-black text-label uppercase tracking-widest mb-1.5" for="modal-room-name">
                Tên phòng <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input
                id="modal-room-name"
                v-model="newRoom.name"
                type="text"
                placeholder="VD: Phòng 305, Lab 3..."
                :class="[
                  'w-full lg-input px-4 py-2.5 text-sm font-medium transition-all',
                  addErrors.name
                    ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]'
                    : ''
                ]"
              />
              <p v-if="addErrors.name" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ addErrors.name }}</p>
            </div>

            <!-- Cơ sở + Lầu -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-black text-label uppercase tracking-widest mb-1.5" for="modal-campus">Cơ sở</label>
                <select
                  id="modal-campus"
                  v-model="newRoom.campus"
                  class="w-full lg-input px-4 py-2.5 text-sm font-bold"
                >
                  <option v-for="c in CAMPUSES" :key="c" :value="c">{{ c }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-black text-label uppercase tracking-widest mb-1.5" for="modal-floor">Lầu</label>
                <select
                  id="modal-floor"
                  v-model="newRoom.floor"
                  class="w-full lg-input px-4 py-2.5 text-sm font-bold"
                >
                  <option v-for="f in FLOORS" :key="f" :value="f">Lầu {{ f }}</option>
                </select>
              </div>
            </div>

            <!-- Sức chứa + Loại phòng -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-black text-label uppercase tracking-widest mb-1.5" for="modal-capacity">
                  Sức chứa (SV) <span class="text-[var(--lg-danger)]">*</span>
                </label>
                <input
                  id="modal-capacity"
                  v-model="newRoom.capacity"
                  type="number"
                  min="1"
                  placeholder="VD: 40"
                  :class="[
                    'w-full lg-input px-4 py-2.5 text-sm font-medium transition-all',
                    addErrors.capacity
                      ? 'border-[var(--lg-danger)] bg-[var(--color-danger-bg)]'
                      : ''
                  ]"
                />
                <p v-if="addErrors.capacity" class="mt-1 text-xs text-[var(--lg-danger)] font-semibold">{{ addErrors.capacity }}</p>
              </div>
              <div>
                <label class="block text-xs font-black text-label uppercase tracking-widest mb-1.5" for="modal-type">Loại phòng</label>
                <select
                  id="modal-type"
                  v-model="newRoom.type"
                  class="w-full lg-input px-4 py-2.5 text-sm font-bold"
                >
                  <option v-for="t in TYPES" :key="t" :value="t">{{ t }}</option>
                </select>
              </div>
            </div>

            <!-- Trạng thái -->
            <div>
              <label class="block text-xs font-black text-label uppercase tracking-widest mb-2">Trạng thái</label>
              <div class="flex gap-2 flex-wrap">
                <button
                  v-for="s in STATUSES"
                  :key="s"
                  :class="[
                    'flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold transition-all',
                    newRoom.status === s
                      ? 'lg-button-primary'
                      : 'lg-button-secondary text-body'
                  ]"
                  @click="newRoom.status = s"
                >
                  <span v-if="newRoom.status !== s" :class="['h-2 w-2 rounded-full', getStatusInfo(s).dot]"></span>
                  {{ getStatusInfo(s).label }}
                </button>
              </div>
            </div>

            <!-- Thiết bị -->
            <div>
              <label class="block text-xs font-black text-label uppercase tracking-widest mb-2">Thiết bị có sẵn</label>
              <div class="flex flex-wrap gap-2">
                <button
                  v-for="d in deviceOptions"
                  :key="d"
                  :class="[
                    'px-3 py-1.5 rounded-xl text-xs font-bold transition-all',
                    newRoom.devices.includes(d)
                      ? 'lg-button-primary'
                      : 'lg-button-secondary text-body'
                  ]"
                  @click="toggleDevice(d)"
                >{{ d }}</button>
              </div>
            </div>
          </div>

          <!-- Footer actions -->
          <div class="px-6 pb-6 pt-2 border-t border-default flex items-center justify-end gap-3 mt-4">
            <button
              class="lg-button-secondary px-5 py-2.5"
              @click="closeAddModal"
            >Hủy</button>
            <button
              id="btn-submit-add-room"
              :class="['lg-button-primary px-6 py-2.5', isSubmitting ? 'opacity-70 cursor-not-allowed' : '']"
              :disabled="isSubmitting"
              @click="submitAddRoom"
            >
              <span v-if="isSubmitting" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Plus v-else :size="16" />
              {{ isSubmitting ? 'Đang lưu...' : 'Thêm phòng' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
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
