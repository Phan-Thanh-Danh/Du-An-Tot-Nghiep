<template>
  <div class="space-y-6 pb-10">
    <!-- Loading State -->
    <div v-if="loading" class="p-4">
      <SkeletonTable :rows="6" :columns="4" />
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
      <!-- Filter & Action Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 class="sr-only text-xl font-bold text-heading">Cơ sở vật chất</h2>
          <p class="text-xs text-muted">Quản lý tòa nhà, tầng, phòng học và trang thiết bị học vụ trên toàn hệ thống</p>
        </div>
        <div class="flex gap-2">
          <LmsSelect v-model="campusFilter" class="px-3 py-2 bg-(--surface-input) border border-input rounded-xl text-xs font-bold text-body focus:outline-none focus:border-(--lg-primary)">
            <option value="all">Tất cả cơ sở</option>
            <option v-for="c in campuses" :key="c.maDonVi" :value="c.maDonVi">{{ c.tenDonVi }}</option>
          </LmsSelect>
        </div>
      </div>

      <!-- Buildings Grid Cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5">
        <div
          v-for="building in filteredBuildings"
          :key="building.maToaNha"
          @click="openBuildingDetail(building.maToaNha)"
          class="surface-card border border-card hover:border-blue-500/50 rounded-3xl p-6 transition-all hover:shadow-md cursor-pointer group flex flex-col justify-between relative overflow-hidden"
        >
          <div>
            <div class="flex items-center justify-between mb-4">
              <div class="h-12 w-12 rounded-2xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white shadow-xs group-hover:scale-105 transition-transform">
                <Building2 :size="24" />
              </div>
              <span
                :class="building.conHoatDong ? 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20' : 'bg-rose-500/10 text-rose-600 border-rose-500/20'"
                class="px-3 py-1 rounded-full border text-[11px] font-bold flex items-center gap-1.5"
              >
                <span :class="building.conHoatDong ? 'bg-emerald-500' : 'bg-rose-500'" class="h-1.5 w-1.5 rounded-full inline-block" />
                {{ building.conHoatDong ? 'Đang hoạt động' : 'Tạm ngừng' }}
              </span>
            </div>

            <div class="flex items-center gap-2 mb-1">
              <span class="text-xs font-mono font-bold text-muted px-2 py-0.5 rounded-md bg-(--surface-input) border border-card">
                {{ building.maCodeToaNha }}
              </span>
            </div>

            <h3 class="text-lg font-extrabold text-heading group-hover:text-blue-600 transition-colors">
              {{ building.tenToaNha }}
            </h3>
            <p class="text-xs text-muted font-medium mt-1">
              {{ building.diaChi || 'Trụ sở giảng dạy' }}
            </p>
          </div>

          <div class="mt-6 pt-4 border-t border-card space-y-3">
            <div class="flex items-center justify-between text-xs font-bold text-body">
              <span class="flex items-center gap-1.5 text-muted"><Layers :size="14" class="text-blue-600" /> Cấu trúc:</span>
              <span class="text-heading">{{ building.soTang || getFloors(building.maToaNha).length }} Tầng</span>
            </div>
            <div class="flex items-center justify-between text-xs font-bold text-body">
              <span class="flex items-center gap-1.5 text-muted"><DoorOpen :size="14" class="text-teal-600" /> Quy mô phòng:</span>
              <span class="text-heading">{{ getRoomsCount(building.maToaNha) }} Phòng học</span>
            </div>
            
            <div class="pt-2 flex items-center justify-between text-xs font-bold text-blue-600 group-hover:translate-x-1 transition-transform">
              <span class="flex items-center gap-1"><Package :size="14" /> Xem lớp & thiết bị chi tiết</span>
              <ChevronRight :size="16" />
            </div>
          </div>
        </div>
      </div>

      <div v-if="filteredBuildings.length === 0" class="text-center py-16 surface-card border border-card rounded-3xl text-muted">
        <Building2 :size="48" class="mx-auto mb-3 opacity-40" />
        <p class="font-bold text-heading text-sm">Không tìm thấy tòa nhà nào</p>
        <p class="text-xs mt-1">Vui lòng thử chọn cơ sở khác.</p>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Building2, ChevronRight, AlertCircle, Layers, DoorOpen, Package } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { unwrapApiData } from '@/services/apiClient'
import { bghApi } from '@/services/bghApi'

const router = useRouter()
const loading = ref(false)
const error = ref(null)

const campusFilter = ref('all')

const buildings = ref([])
const floors = ref([])
const rooms = ref([])
const campuses = ref([])

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
  if (!campusFilter.value || campusFilter.value === 'all') return buildings.value
  return buildings.value.filter(b => b.maDonVi === parseInt(campusFilter.value))
})

function getFloors(buildingId) {
  return floors.value.filter(f => f.maToaNha === buildingId)
}

function getRoomsCount(buildingId) {
  const floorIds = new Set(getFloors(buildingId).map(f => f.maTang))
  return rooms.value.filter(r => floorIds.has(r.maTang) || r.maToaNha === buildingId).length || 4
}

function openBuildingDetail(buildingId) {
  router.push(`/bgh/facilities/${buildingId}`)
}

onMounted(() => { loadData() })
</script>
