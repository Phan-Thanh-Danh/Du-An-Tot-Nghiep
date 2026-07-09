<template>
  <div class="space-y-4 pb-10">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin" />
        <p class="text-sm font-medium">Đang tải dữ liệu...</p>
      </div>
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Cơ sở vật chất</h2>
        <p class="text-xs text-muted mt-1">Quản lý tòa nhà, tầng và phòng học trên toàn hệ thống</p>
      </div>
      <div class="flex gap-2">
        <select v-model="campusFilter" class="px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả cơ sở</option>
          <option v-for="c in campuses" :key="c.maDonVi" :value="c.maDonVi">{{ c.tenDonVi }}</option>
        </select>
      </div>
    </div>

    <div v-for="building in filteredBuildings" :key="building.maToaNha" class="surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div @click="toggleBuilding(building.maToaNha)" class="px-5 py-4 flex items-center justify-between cursor-pointer hover:bg-(--surface-input)/30 transition-colors">
        <div class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white"><Building2 :size="20" /></div>
          <div>
            <h3 class="text-base font-bold text-heading">{{ building.tenToaNha }}</h3>
            <p class="text-xs text-muted">{{ building.maCodeToaNha }} · {{ building.diaChi || building.tenDonVi }} · {{ building.soTang }} tầng</p>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <span :class="building.conHoatDong ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="text-xs font-bold flex items-center gap-1">
            <span :class="building.conHoatDong ? 'bg-(--color-success-text)' : 'bg-(--color-danger-text)'" class="h-1.5 w-1.5 rounded-full inline-block" />
            {{ building.conHoatDong ? 'Đang hoạt động' : 'Ngừng' }}
          </span>
          <ChevronDown v-if="expandedBuilding === building.maToaNha" :size="18" class="text-muted transition-transform" />
          <ChevronRight v-else :size="18" class="text-muted transition-transform" />
        </div>
      </div>

      <Transition enter-active-class="transition-all duration-300" enter-from-class="max-h-0 opacity-0" enter-to-class="max-h-[2000px] opacity-100" leave-active-class="transition-all duration-200" leave-from-class="max-h-[2000px] opacity-100" leave-to-class="max-h-0 opacity-0">
        <div v-if="expandedBuilding === building.maToaNha" class="overflow-hidden">
          <div v-for="floor in getFloors(building.maToaNha)" :key="floor.maTang" class="last:border-b-0">
            <div @click="toggleFloor(floor.maTang)" class="px-5 py-3 flex items-center justify-between cursor-pointer hover:bg-(--surface-input)/20 transition-colors ml-4">
              <div class="flex items-center gap-2">
                <ChevronDown v-if="expandedFloor === floor.maTang" :size="14" class="text-muted" />
                <ChevronRight v-else :size="14" class="text-muted" />
                <span class="text-sm font-semibold text-heading">{{ floor.tenTang }}</span>
                <span class="text-xs text-muted">(Tầng {{ floor.thuTuTang }})</span>
                <span v-if="!floor.conHoatDong" class="text-[10px] text-(--color-danger-text) bg-(--color-danger-bg) px-1.5 py-0.5 rounded">Ngừng</span>
              </div>
              <span class="text-xs text-muted">{{ getRooms(floor.maTang).length }} phòng</span>
            </div>

            <Transition enter-active-class="transition-all duration-200" enter-from-class="max-h-0 opacity-0" enter-to-class="max-h-[500px] opacity-100" leave-active-class="transition-all duration-200" leave-from-class="max-h-[500px] opacity-100" leave-to-class="max-h-0 opacity-0">
              <div v-if="expandedFloor === floor.maTang" class="overflow-hidden">
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 p-4 pt-0 ml-8">
                  <div v-for="room in getRooms(floor.maTang)" :key="room.maPhong" class="p-3 rounded-xl border border-default hover:border-(--border-input-focus) hover:shadow-sm transition-all bg-(--surface-card)">
                    <div class="flex items-center justify-between mb-2">
                      <span class="text-xs font-bold text-heading">{{ room.maCodePhong }}</span>
                      <span :class="roomTypeBadge(room.loaiPhong)">{{ room.loaiPhongLabel }}</span>
                    </div>
                    <p class="text-sm font-semibold text-heading">{{ room.tenPhong }}</p>
                    <div class="mt-2 flex items-center justify-between text-[10px]">
                      <span class="text-muted flex items-center gap-1"><Users :size="12" /> {{ room.sucChua }} chỗ</span>
                      <span :class="roomStatusBadge(room.trangThaiPhong)">{{ roomStatusLabel(room.trangThaiPhong) }}</span>
                    </div>
                  </div>
                  <div v-if="getRooms(floor.maTang).length === 0" class="col-span-full text-center py-4 text-muted text-xs">
                    Chưa có phòng học nào trên tầng này.
                  </div>
                </div>
              </div>
            </Transition>
          </div>
        </div>
      </Transition>
    </div>

    <div v-if="filteredBuildings.length === 0" class="text-center py-12 text-muted">
      <Building2 :size="40" class="mx-auto mb-3 opacity-50" />
      <p>Không có tòa nhà nào.</p>
    </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Building2, MapPin, Monitor, Users, Wifi, Coffee, Dumbbell, Car, ChevronDown, ChevronRight, Search, Eye, Layers, AlertCircle, Loader2 } from 'lucide-vue-next'
import { usePopupStore } from '@/stores/popup'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
import { bghApi } from '@/services/bghApi'

const loading = ref(false)
const error = ref(null)

const popup = usePopupStore()
const searchQuery = ref('')
const typeFilter = ref('all')
const statusFilter = ref('all')
const campusFilter = ref('all')
const expandedBuildings = ref(new Set())
const showFilterDetail = ref(false)

const buildings = ref([])
const floors = ref([])
const rooms = ref([])
const campuses = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const [bldRes, flrRes, roomRes, orgRes] = await Promise.all([
      apiRequest('/api/bgh/master-data/buildings'),
      apiRequest('/api/bgh/master-data/floors'),
      apiRequest('/api/bgh/master-data/rooms'),
      bghApi.getOrganizations(),
    ])
    buildings.value = unwrapApiData(bldRes) || []
    floors.value = unwrapApiData(flrRes) || []
    rooms.value = unwrapApiData(roomRes) || []
    campuses.value = (unwrapApiData(orgRes) || []).map(o => ({ maDonVi: o.id, tenDonVi: o.name }))
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu cơ sở vật chất'
  } finally {
    loading.value = false
  }
}

const filteredBuildings = computed(() => {
  if (!campusFilter.value) return buildings.value
  return buildings.value.filter(b => b.maDonVi === parseInt(campusFilter.value))
})

function getFloors(buildingId) {
  return floors.value.filter(f => f.maToaNha === buildingId)
}

function getRooms(floorId) {
  return rooms.value.filter(r => r.maTang === floorId)
}

function roomTypeBadge(type) {
  switch (type) {
    case 'ly_thuyet': return 'text-[10px] px-1.5 py-0.5 rounded bg-(--color-info-bg) text-(--color-info-text) font-bold'
    case 'thuc_hanh': return 'text-[10px] px-1.5 py-0.5 rounded bg-(--color-warning-bg) text-(--color-warning-text) font-bold'
    case 'hoi_truong': return 'text-[10px] px-1.5 py-0.5 rounded bg-(--color-success-bg) text-(--color-success-text) font-bold'
    default: return 'text-[10px] px-1.5 py-0.5 rounded bg-(--surface-input) text-muted'
  }
}

function roomStatusBadge(status) {
  switch (status) {
    case 'dang_su_dung': return 'text-[10px] font-bold text-(--color-success-text)'
    case 'bao_tri': return 'text-[10px] font-bold text-(--color-warning-text)'
    case 'ngung_hoat_dong': return 'text-[10px] font-bold text-(--color-danger-text)'
    default: return 'text-[10px] font-bold text-muted'
  }
}

function roomStatusLabel(status) {
  switch (status) {
    case 'dang_su_dung': return 'Đang sử dụng'
    case 'bao_tri': return 'Bảo trì'
    case 'ngung_hoat_dong': return 'Ngừng hoạt động'
    default: return status
  }
}

onMounted(() => { loadData() })
</script>
