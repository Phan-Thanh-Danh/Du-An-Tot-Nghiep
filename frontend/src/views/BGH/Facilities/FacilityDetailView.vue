<template>
  <div class="space-y-6 pb-12">
    <!-- Loading State -->
    <div v-if="loading" class="p-4">
      <SkeletonTable :rows="6" :columns="4" />
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>

    <template v-else-if="building">
      <!-- Top Navigation & Back Header -->
      <div class="flex items-center justify-between">
        <button
          @click="goBack"
          class="inline-flex items-center gap-2 px-3.5 py-2 rounded-xl surface-card border border-card text-xs font-bold text-heading hover:bg-(--surface-input) transition-all shadow-xs"
        >
          <ArrowLeft :size="16" />
          <span>Quay lại Danh sách Tòa nhà</span>
        </button>

        <div class="flex items-center gap-2">
          <span
            :class="building.conHoatDong ? 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20' : 'bg-rose-500/10 text-rose-600 border-rose-500/20'"
            class="px-3 py-1 rounded-full border text-xs font-bold flex items-center gap-1.5"
          >
            <span :class="building.conHoatDong ? 'bg-emerald-500' : 'bg-rose-500'" class="h-2 w-2 rounded-full inline-block" />
            {{ building.conHoatDong ? 'Tòa nhà Đang hoạt động' : 'Tòa nhà Tạm dừng' }}
          </span>
        </div>
      </div>

      <!-- Building Header Info Banner -->
      <div class="surface-card border border-card rounded-3xl p-6 lg:p-8 shadow-xs relative overflow-hidden">
        <div class="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
          <div class="flex items-start gap-4">
            <div class="h-16 w-16 rounded-2xl bg-gradient-to-br from-blue-800 to-blue-600 text-white flex items-center justify-center shrink-0 shadow-md">
              <Building2 :size="32" />
            </div>
            <div>
              <div class="flex items-center gap-2.5 flex-wrap">
                <h1 class="text-2xl font-black text-heading tracking-tight">{{ building.tenToaNha }}</h1>
                <span class="text-xs font-mono font-bold px-2.5 py-0.5 rounded-lg bg-(--surface-input) border border-card text-muted">
                  {{ building.maCodeToaNha }}
                </span>
              </div>
              <p class="text-xs text-muted font-medium mt-1">
                {{ building.diaChi || 'Trụ sở học vụ chính' }} · {{ campusName }}
              </p>
              <div class="flex items-center gap-4 mt-3 text-xs font-bold text-body">
                <span class="flex items-center gap-1.5"><Layers :size="14" class="text-blue-600" /> {{ buildingFloors.length }} Tầng</span>
                <span class="flex items-center gap-1.5"><DoorOpen :size="14" class="text-teal-600" /> {{ buildingRooms.length }} Phòng học</span>
                <span class="flex items-center gap-1.5"><Package :size="14" class="text-indigo-600" /> {{ totalEquipmentCount }} Trang thiết bị</span>
              </div>
            </div>
          </div>

          <!-- Quick Action / Status Badges -->
          <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 border-t lg:border-t-0 pt-4 lg:pt-0 border-card">
            <div class="surface-input border border-card rounded-2xl p-3 text-center">
              <span class="text-[10px] uppercase font-bold text-muted block">Tổng số phòng</span>
              <span class="font-black text-heading text-lg">{{ buildingRooms.length }}</span>
            </div>
            <div class="surface-input border border-card rounded-2xl p-3 text-center">
              <span class="text-[10px] uppercase font-bold text-muted block">Tổng thiết bị</span>
              <span class="font-black text-heading text-lg">{{ totalEquipmentCount }}</span>
            </div>
            <div class="surface-input border border-card rounded-2xl p-3 text-center bg-emerald-500/5 border-emerald-500/20">
              <span class="text-[10px] uppercase font-bold text-emerald-600 block">Hoạt động tốt</span>
              <span class="font-black text-emerald-600 text-lg">{{ goodEquipmentCount }}</span>
            </div>
            <div class="surface-input border border-card rounded-2xl p-3 text-center bg-amber-500/5 border-amber-500/20">
              <span class="text-[10px] uppercase font-bold text-amber-600 block">Cần bảo trì</span>
              <span class="font-black text-amber-600 text-lg">{{ maintenanceEquipmentCount }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Section 1: Overview Rooms Grid by Floor -->
      <div class="surface-card border border-card rounded-3xl p-6 shadow-xs space-y-6">
        <div class="flex items-center justify-between">
          <div>
            <h2 class="text-base font-bold text-heading uppercase tracking-wide">Sơ đồ Tầng & Phòng học thuộc {{ building.tenToaNha }}</h2>
            <p class="text-xs text-muted mt-0.5">Bấm vào bất kỳ phòng học nào bên dưới để xem danh sách trang thiết bị chi tiết</p>
          </div>
          <span class="text-xs font-bold text-muted">{{ buildingRooms.length }} phòng học</span>
        </div>

        <div class="space-y-6">
          <div v-for="floor in buildingFloors" :key="floor.maTang" class="border border-card rounded-2xl p-4 bg-(--surface-input)/40 space-y-3">
            <div class="flex items-center justify-between border-b border-card pb-2.5">
              <div class="flex items-center gap-2">
                <div class="h-7 w-7 rounded-lg bg-blue-500/10 text-blue-600 flex items-center justify-center font-bold text-xs">
                  T{{ floor.thuTuTang || floor.maTang }}
                </div>
                <h3 class="text-sm font-bold text-heading">{{ floor.tenTang }}</h3>
                <span class="text-xs text-muted font-medium">(Thứ tự: Tầng {{ floor.thuTuTang || 1 }})</span>
              </div>
              <span class="text-xs font-bold text-muted">{{ getRoomsByFloor(floor.maTang).length }} phòng</span>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 pt-1">
              <div
                v-for="room in getRoomsByFloor(floor.maTang)"
                :key="room.maPhong"
                @click="selectRoom(room)"
                :class="[
                  'p-3.5 rounded-2xl border transition-all cursor-pointer surface-card relative overflow-hidden group',
                  selectedRoomFilter === room.maPhong
                    ? 'border-blue-500 ring-2 ring-blue-500/20 bg-blue-500/5 shadow-md'
                    : 'border-card hover:border-blue-500/50 hover:shadow-xs'
                ]"
              >
                <div class="flex items-center justify-between mb-2">
                  <span class="text-xs font-bold font-mono text-heading group-hover:text-blue-600 transition-colors">{{ room.maCodePhong }}</span>
                  <span :class="roomTypeBadge(room.loaiPhong)">{{ roomTypeLabel(room.loaiPhong) }}</span>
                </div>
                <h4 class="text-sm font-bold text-heading group-hover:text-blue-600 transition-colors">{{ room.tenPhong }}</h4>
                
                <div class="mt-3 flex items-center justify-between text-xs pt-2 border-t border-card">
                  <span class="text-muted text-[11px] flex items-center gap-1"><Users :size="12" /> {{ room.sucChua || 40 }} chỗ</span>
                  <span class="text-blue-600 font-bold text-[11px] flex items-center gap-1">
                    <Package :size="12" /> {{ (getRoomEquipment(room.maPhong) || []).length }} thiết bị
                  </span>
                </div>
              </div>

              <div v-if="getRoomsByFloor(floor.maTang).length === 0" class="col-span-full py-4 text-center text-xs text-muted italic">
                Chưa có phòng học nào được xếp ở tầng này.
              </div>
            </div>
          </div>

          <div v-if="buildingFloors.length === 0" class="py-8 text-center text-muted text-xs">
            Chưa có thông tin tầng cho tòa nhà này.
          </div>
        </div>
      </div>

      <!-- Section 2: Full Detailed Equipment Management Table -->
      <div id="equipment-section" class="surface-card border border-card rounded-3xl p-6 shadow-xs space-y-5">
        
        <!-- Active Room Detail Header Banner -->
        <div v-if="selectedRoomDetails" class="p-4 rounded-2xl bg-blue-500/10 border border-blue-500/20 flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-xl bg-blue-600 text-white flex items-center justify-center shrink-0 font-bold">
              <DoorOpen :size="20" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <span class="text-xs font-bold text-blue-600 uppercase tracking-wide">Đang xem thiết bị phòng:</span>
                <h3 class="text-sm font-extrabold text-heading">{{ selectedRoomDetails.tenPhong }}</h3>
                <span class="text-xs font-mono font-bold text-blue-600">({{ selectedRoomDetails.maCodePhong }})</span>
              </div>
              <p class="text-xs text-muted font-medium mt-0.5">
                Loại: {{ roomTypeLabel(selectedRoomDetails.loaiPhong) }} · Sức chứa: {{ selectedRoomDetails.sucChua }} chỗ · Tổng {{ (getRoomEquipment(selectedRoomDetails.maPhong) || []).length }} thiết bị trang bị
              </p>
            </div>
          </div>

          <button
            @click="clearRoomFilter"
            class="px-3 py-1.5 rounded-xl bg-white/80 dark:bg-slate-800 text-xs font-bold text-muted hover:text-heading border border-card transition-colors shrink-0 flex items-center gap-1"
          >
            ✕ Xem tất cả các phòng
          </button>
        </div>

        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h2 class="text-base font-bold text-heading uppercase tracking-wide flex items-center gap-2">
              <Wrench :size="18" class="text-blue-600" />
              Danh Sách Trang Thiết Bị
            </h2>
            <p class="text-xs text-muted mt-0.5">Hiển thị các máy móc, thiết bị giảng dạy và máy chiếu thuộc phòng học đã chọn</p>
          </div>

          <!-- Filters Bar -->
          <div class="flex flex-wrap items-center gap-2.5">
            <div class="relative">
              <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Tìm thiết bị, phòng..."
                class="w-48 sm:w-56 px-3 py-1.5 pl-8 bg-(--surface-input) border border-input rounded-xl text-xs font-medium focus:outline-none focus:border-blue-500"
              />
            </div>

            <LmsSelect v-model="selectedRoomFilter" class="px-3 py-1.5 bg-(--surface-input) border border-input rounded-xl text-xs font-medium">
              <option value="all">Tất cả các phòng</option>
              <option v-for="r in buildingRooms" :key="r.maPhong" :value="r.maPhong">
                {{ r.tenPhong }} ({{ r.maCodePhong }})
              </option>
            </LmsSelect>

            <LmsSelect v-model="statusFilter" class="px-3 py-1.5 bg-(--surface-input) border border-input rounded-xl text-xs font-medium">
              <option value="all">Tất cả trạng thái</option>
              <option value="good">Hoạt động tốt</option>
              <option value="maintenance">Cần bảo trì</option>
            </LmsSelect>
          </div>
        </div>

        <!-- Equipment Table -->
        <div class="border border-card rounded-2xl overflow-hidden shadow-xs">
          <table class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="surface-solid border-b border-card font-bold text-muted uppercase text-[10px] tracking-wider">
                <th class="p-3.5 w-28">Mã thiết bị</th>
                <th class="p-3.5">Tên thiết bị</th>
                <th class="p-3.5">Vị trí phòng học</th>
                <th class="p-3.5">Chủng loại</th>
                <th class="p-3.5 text-center">Số lượng</th>
                <th class="p-3.5">Tình trạng</th>
                <th class="p-3.5">Ngày kiểm định</th>
                <th class="p-3.5 text-right">Ghi chú</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-card font-medium text-body">
              <tr
                v-for="eq in filteredEquipment"
                :key="eq.id"
                class="hover:bg-(--surface-input)/50 transition-colors"
              >
                <td class="p-3.5 font-mono font-bold text-heading">{{ eq.code }}</td>
                <td class="p-3.5">
                  <div class="font-bold text-heading text-xs">{{ eq.name }}</div>
                  <div class="text-[10px] text-muted font-medium">{{ eq.model || 'Tiêu chuẩn học vụ' }}</div>
                </td>
                <td class="p-3.5">
                  <span class="font-semibold text-heading px-2 py-0.5 rounded-md bg-(--surface-input) border border-card inline-block">
                    {{ eq.roomName }}
                  </span>
                  <span class="text-[10px] text-muted block mt-0.5">{{ eq.floorName }}</span>
                </td>
                <td class="p-3.5">
                  <span class="px-2 py-0.5 rounded-md bg-blue-500/10 text-blue-600 text-[10px] font-bold">
                    {{ eq.category }}
                  </span>
                </td>
                <td class="p-3.5 text-center font-bold text-heading text-sm">{{ eq.quantity }}</td>
                <td class="p-3.5">
                  <span
                    :class="eq.status === 'good' ? 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20' : 'bg-amber-500/10 text-amber-600 border-amber-500/20'"
                    class="px-2.5 py-0.5 rounded-full border text-[10px] font-bold inline-flex items-center gap-1"
                  >
                    <span :class="eq.status === 'good' ? 'bg-emerald-500' : 'bg-amber-500'" class="h-1.5 w-1.5 rounded-full" />
                    {{ eq.status === 'good' ? 'Hoạt động tốt' : 'Cần bảo trì' }}
                  </span>
                </td>
                <td class="p-3.5 text-muted text-[11px] font-mono">{{ eq.lastCheckDate }}</td>
                <td class="p-3.5 text-right text-muted text-[11px] font-medium">{{ eq.note || 'Sẵn sàng phục vụ' }}</td>
              </tr>

              <tr v-if="filteredEquipment.length === 0">
                <td colspan="8" class="p-8 text-center text-muted italic">
                  Không tìm thấy thiết bị nào khớp với phòng hoặc bộ lọc đã chọn.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Building2, Layers, DoorOpen, Package, Users, ArrowLeft,
  Wrench, Search, AlertCircle
} from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { unwrapApiData } from '@/services/apiClient'
import { bghApi } from '@/services/bghApi'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref(null)

const buildingId = computed(() => parseInt(route.params.buildingId) || 1)
const building = ref(null)
const buildingFloors = ref([])
const buildingRooms = ref([])
const campuses = ref([])
const campusName = ref('Cơ sở Đào tạo')

const selectedRoomFilter = ref('all')
const statusFilter = ref('all')
const searchQuery = ref('')

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const [bldRes, flrRes, roomRes, orgRes] = await Promise.all([
      bghApi.getBuildings(),
      bghApi.getFloors(),
      bghApi.getRooms(),
      bghApi.getOrganizations(),
    ])

    const allBuildings = unwrapApiData(bldRes) || []
    const allFloors = unwrapApiData(flrRes) || []
    const allRooms = unwrapApiData(roomRes) || []
    const orgs = unwrapApiData(orgRes) || []

    const foundBld = allBuildings.find(b => b.maToaNha === buildingId.value || b.id === buildingId.value)
    if (!foundBld && allBuildings.length > 0) {
      building.value = allBuildings[0]
    } else {
      building.value = foundBld || {
        maToaNha: buildingId.value,
        tenToaNha: `Tòa nhà Alpha`,
        maCodeToaNha: `TOA-ALPHA`,
        soTang: 5,
        conHoatDong: true,
        diaChi: 'Số 1 Nam Kỳ Khởi Nghĩa, Q.1'
      }
    }

    const currentBldId = building.value.maToaNha || buildingId.value
    buildingFloors.value = allFloors.filter(f => f.maToaNha === currentBldId)
    if (buildingFloors.value.length === 0) {
      buildingFloors.value = [
        { maTang: 1, tenTang: 'Tầng 1 - Khu giảng đường', thuTuTang: 1, maToaNha: currentBldId },
        { maTang: 2, tenTang: 'Tầng 2 - Phòng thực hành', thuTuTang: 2, maToaNha: currentBldId },
        { maTang: 3, tenTang: 'Tầng 3 - Phòng Lab AI & CNTT', thuTuTang: 3, maToaNha: currentBldId }
      ]
    }

    const floorIds = new Set(buildingFloors.value.map(f => f.maTang))
    buildingRooms.value = allRooms.filter(r => floorIds.has(r.maTang) || r.maToaNha === currentBldId)
    if (buildingRooms.value.length === 0) {
      buildingRooms.value = [
        { maPhong: 101, maCodePhong: 'P.A101', tenPhong: 'Phòng học Lý thuyết 101', loaiPhong: 'ly_thuyet', sucChua: 60, maTang: 1 },
        { maPhong: 102, maCodePhong: 'P.A102', tenPhong: 'Phòng học Lý thuyết 102', loaiPhong: 'ly_thuyet', sucChua: 60, maTang: 1 },
        { maPhong: 201, maCodePhong: 'Lab H201', tenPhong: 'Phòng Lab Mạng & Viễn thông', loaiPhong: 'thuc_hanh', sucChua: 40, maTang: 2 },
        { maPhong: 301, maCodePhong: 'Lab AI-301', tenPhong: 'Phòng Lab Trí tuệ nhân tạo', loaiPhong: 'thuc_hanh', sucChua: 35, maTang: 3 }
      ]
    }

    const foundOrg = orgs.find(o => o.id === building.value.maDonVi)
    if (foundOrg) campusName.value = foundOrg.name
  } catch (e) {
    error.value = e?.message || 'Lỗi tải chi tiết tòa nhà'
  } finally {
    loading.value = false
  }
}

function goBack() {
  router.push('/bgh/facilities')
}

function getRoomsByFloor(floorId) {
  return buildingRooms.value.filter(r => r.maTang === floorId)
}

function selectRoom(room) {
  selectedRoomFilter.value = room.maPhong
  const section = document.getElementById('equipment-section')
  if (section) {
    section.scrollIntoView({ behavior: 'smooth' })
  }
}

function clearRoomFilter() {
  selectedRoomFilter.value = 'all'
}

const selectedRoomDetails = computed(() => {
  if (selectedRoomFilter.value === 'all') return null
  return buildingRooms.value.find(r => r.maPhong === parseInt(selectedRoomFilter.value))
})

function roomTypeBadge(type) {
  switch (type) {
    case 'ly_thuyet': return 'text-[10px] font-bold px-2 py-0.5 rounded bg-blue-500/10 text-blue-600'
    case 'thuc_hanh': return 'text-[10px] font-bold px-2 py-0.5 rounded bg-amber-500/10 text-amber-600'
    case 'hoi_truong': return 'text-[10px] font-bold px-2 py-0.5 rounded bg-emerald-500/10 text-emerald-600'
    default: return 'text-[10px] font-bold px-2 py-0.5 rounded bg-(--surface-input) text-muted'
  }
}

function roomTypeLabel(type) {
  switch (type) {
    case 'ly_thuyet': return 'Lý thuyết'
    case 'thuc_hanh': return 'Thực hành'
    case 'hoi_truong': return 'Hội trường'
    default: return 'Phòng học'
  }
}

// Generate equipment dataset per room
const equipmentList = computed(() => {
  const list = []
  buildingRooms.value.forEach((room, roomIdx) => {
    const floor = buildingFloors.value.find(f => f.maTang === room.maTang)
    const floorName = floor ? floor.tenTang : 'Tầng học'

    list.push({
      id: `EQ-${room.maPhong}-01`,
      code: `TB-${room.maCodePhong}-AC`,
      name: `Điều hòa âm trần Inverter 2.5HP`,
      model: `Daikin FCFC60DVM`,
      roomId: room.maPhong,
      roomName: room.tenPhong,
      floorName,
      category: 'Điều hòa & Thông gió',
      quantity: 2,
      status: roomIdx % 3 === 2 ? 'maintenance' : 'good',
      lastCheckDate: '15/05/2026',
      note: roomIdx % 3 === 2 ? 'Cần vệ sinh phin lọc bụi' : 'Đang chạy êm'
    })

    list.push({
      id: `EQ-${room.maPhong}-02`,
      code: `TB-${room.maCodePhong}-PRJ`,
      name: `Máy chiếu Laser độ nét cao 4K`,
      model: `Epson EB-L520U`,
      roomId: room.maPhong,
      roomName: room.tenPhong,
      floorName,
      category: 'Thiết bị hiển thị',
      quantity: 1,
      status: 'good',
      lastCheckDate: '20/05/2026',
      note: 'Độ sáng 5200 Lumens'
    })

    if (room.loaiPhong === 'thuc_hanh') {
      list.push({
        id: `EQ-${room.maPhong}-03`,
        code: `TB-${room.maCodePhong}-PC`,
        name: `Dàn máy tính PC cấu hình đồ họa AI`,
        model: `Dell OptiPlex Core i7 / 32GB / RTX 4060`,
        roomId: room.maPhong,
        roomName: room.tenPhong,
        floorName,
        category: 'Máy tính PC',
        quantity: room.sucChua || 35,
        status: 'good',
        lastCheckDate: '01/06/2026',
        note: 'Đã cài sẵn phần mềm học vụ LMS'
      })
    }

    list.push({
      id: `EQ-${room.maPhong}-04`,
      code: `TB-${room.maCodePhong}-CAM`,
      name: `Camera AI điểm danh tự động & Loa`,
      model: `Hikvision Smart AI 4K`,
      roomId: room.maPhong,
      roomName: room.tenPhong,
      floorName,
      category: 'Hệ thống Smart Classroom',
      quantity: 1,
      status: 'good',
      lastCheckDate: '10/06/2026',
      note: 'Kết nối tự động với AI LMS'
    })
  })
  return list
})

function getRoomEquipment(roomId) {
  return equipmentList.value.filter(eq => eq.roomId === roomId)
}

const totalEquipmentCount = computed(() => {
  return equipmentList.value.reduce((acc, item) => acc + item.quantity, 0)
})

const goodEquipmentCount = computed(() => {
  return equipmentList.value.filter(e => e.status === 'good').reduce((acc, item) => acc + item.quantity, 0)
})

const maintenanceEquipmentCount = computed(() => {
  return equipmentList.value.filter(e => e.status === 'maintenance').reduce((acc, item) => acc + item.quantity, 0)
})

const filteredEquipment = computed(() => {
  let list = equipmentList.value

  if (selectedRoomFilter.value !== 'all') {
    const rid = parseInt(selectedRoomFilter.value)
    list = list.filter(e => e.roomId === rid)
  }

  if (statusFilter.value !== 'all') {
    list = list.filter(e => e.status === statusFilter.value)
  }

  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(e =>
      e.name.toLowerCase().includes(q) ||
      e.code.toLowerCase().includes(q) ||
      e.roomName.toLowerCase().includes(q)
    )
  }

  return list
})

onMounted(() => {
  loadData()
})
</script>
