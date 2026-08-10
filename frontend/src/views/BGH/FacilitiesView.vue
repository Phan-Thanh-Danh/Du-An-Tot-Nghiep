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
          <button @click="openAddBuildingModal" class="flex items-center gap-1.5 px-4 py-2 bg-(--lg-primary) hover:bg-(--lg-primary-dark) text-white text-xs font-bold rounded-xl transition-all shadow-sm">
            <Plus :size="16" />
            <span>Thêm Tòa nhà</span>
          </button>
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
              <div class="flex items-center gap-2">
                <span
                  :class="building.conHoatDong ? 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20' : 'bg-rose-500/10 text-rose-600 border-rose-500/20'"
                  class="px-3 py-1 rounded-full border text-[11px] font-bold flex items-center gap-1.5"
                >
                  <span :class="building.conHoatDong ? 'bg-emerald-500' : 'bg-rose-500'" class="h-1.5 w-1.5 rounded-full inline-block" />
                  {{ building.conHoatDong ? 'Đang hoạt động' : 'Tạm ngừng' }}
                </span>
                <button
                  @click="toggleSoftDeleteBuilding(building, $event)"
                  :title="building.conHoatDong ? 'Tạm dừng / Xóa mềm tòa nhà' : 'Khôi phục tòa nhà'"
                  class="p-1.5 rounded-xl border border-card hover:bg-rose-500/10 hover:text-rose-600 text-muted transition-colors"
                >
                  <Trash2 v-if="building.conHoatDong" :size="14" />
                  <RotateCcw v-else :size="14" class="text-emerald-600" />
                </button>
              </div>
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

    <!-- Modal Thêm Tòa nhà -->
    <div v-if="showBuildingModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div class="w-full max-w-md surface-card rounded-2xl shadow-2xl border border-default overflow-hidden flex flex-col">
        <div class="p-4 border-b border-default flex justify-between items-center bg-(--surface-card)">
          <h3 class="text-base font-bold text-heading flex items-center gap-2">
            <Building2 :size="20" class="text-blue-600" /> Thêm Tòa Nhà Mới
          </h3>
          <button @click="showBuildingModal = false" class="p-1 hover:bg-(--surface-input) rounded-lg text-muted"><X :size="20" /></button>
        </div>
        <form @submit.prevent="saveBuilding" class="p-6 space-y-4">
          <div v-if="buildingError" class="p-3 bg-(--color-danger-bg) text-(--color-danger-text) text-xs rounded-lg flex gap-2 items-start">
            <AlertCircle :size="16" class="shrink-0 mt-0.5" />
            <span>{{ buildingError }}</span>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Tên tòa nhà <span class="text-(--color-danger-text)">*</span></label>
            <input v-model="buildingForm.tenToaNha" type="text" required placeholder="Ví dụ: Tòa A" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
            <div v-if="nameDuplicateWarning" class="p-2.5 mt-1.5 bg-amber-500/10 border border-amber-500/30 text-amber-600 text-xs rounded-lg flex items-center gap-1.5 font-bold">
              <AlertTriangle :size="14" class="shrink-0" />
              <span>{{ nameDuplicateWarning }}</span>
            </div>
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Số tầng <span class="text-(--color-danger-text)">*</span></label>
            <input v-model.number="buildingForm.soTang" type="number" min="1" max="50" required placeholder="Ví dụ: 5" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
          </div>
          <div>
            <label class="block text-xs font-bold text-heading mb-1.5">Địa chỉ / Vị trí</label>
            <input v-model="buildingForm.diaChi" type="text" placeholder="Ví dụ: Khu học xá chính" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
          </div>
          <div class="pt-2 flex justify-end gap-3">
            <button type="button" @click="showBuildingModal = false" class="px-4 py-2 border border-input rounded-lg text-xs font-bold text-body hover:bg-(--surface-input) transition-colors">Hủy</button>
            <button type="submit" :disabled="savingBuilding || !!nameDuplicateWarning" class="px-5 py-2 bg-(--lg-primary) hover:bg-(--lg-primary-dark) text-white text-xs font-bold rounded-lg transition-colors disabled:opacity-50 flex items-center gap-1.5">
              <Loader2 v-if="savingBuilding" class="animate-spin" :size="14" />
              <span>{{ savingBuilding ? 'Đang lưu...' : 'Lưu Tòa Nhà' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Building2, ChevronRight, AlertCircle, AlertTriangle, Layers, DoorOpen, Package, Plus, X, Loader2, Trash2, RotateCcw } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { apiRequest, unwrapApiData } from '@/services/apiClient'
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
  return rooms.value.filter(r => floorIds.has(r.maTang) || r.maToaNha === buildingId).length
}

const showBuildingModal = ref(false)
const savingBuilding = ref(false)
const buildingError = ref('')
const buildingForm = ref({ tenToaNha: '', soTang: 5, diaChi: '' })

const nameDuplicateWarning = computed(() => {
  const name = buildingForm.value.tenToaNha?.trim().toLowerCase()
  if (!name) return ''
  const exists = buildings.value.some(b => b.tenToaNha?.trim().toLowerCase() === name)
  return exists ? 'Tòa nhà này đã tồn tại trong danh sách! Vui lòng chọn tên khác.' : ''
})

function openAddBuildingModal() {
  buildingForm.value = { tenToaNha: '', soTang: 5, diaChi: '' }
  buildingError.value = ''
  showBuildingModal.value = true
}

async function saveBuilding() {
  const name = buildingForm.value.tenToaNha?.trim()
  if (!name) {
    buildingError.value = 'Vui lòng nhập tên tòa nhà.'
    return
  }
  if (nameDuplicateWarning.value) {
    buildingError.value = nameDuplicateWarning.value
    return
  }
  savingBuilding.value = true
  buildingError.value = ''
  try {
    const code = 'TOA-' + name.replace(/[^a-zA-Z0-9]/g, '').toUpperCase()
    let selectedCampusId = campusFilter.value !== 'all' ? parseInt(campusFilter.value) : 0
    if (!selectedCampusId) {
      // Lấy đơn vị (campus) thực tế của user — campuses[0] có thể là org gốc không thuộc scope
      try {
        const profileRes = await apiRequest('/api/account/me')
        const profile = unwrapApiData(profileRes)
        selectedCampusId = profile?.maDonVi || 0
      } catch {
        selectedCampusId = 0
      }
    }
    if (!selectedCampusId) {
      buildingError.value = 'Không xác định được cơ sở đào tạo của tài khoản. Vui lòng chọn cơ sở trong bộ lọc.'
      return
    }
    const res = await apiRequest('/api/master-data/buildings', {
      method: 'POST',
      body: JSON.stringify({
        maDonVi: selectedCampusId,
        maCodeToaNha: code,
        tenToaNha: name,
        soTang: buildingForm.value.soTang,
        diaChi: buildingForm.value.diaChi || 'Khu học xá chính'
      })
    })
    
    const created = unwrapApiData(res)
    if (!created?.maToaNha) {
      buildingError.value = 'Không nhận được phản hồi hợp lệ từ máy chủ khi tạo tòa nhà.'
      return
    }
    buildings.value.unshift(created)
    bghApi.invalidate('/api/bgh/master-data/buildings')
    showBuildingModal.value = false
  } catch (e) {
    buildingError.value = e?.message || 'Lỗi lưu tòa nhà'
  } finally {
    savingBuilding.value = false
  }
}

async function toggleSoftDeleteBuilding(building, event) {
  event.stopPropagation()
  const isCurrentlyActive = building.conHoatDong
  const actionText = isCurrentlyActive ? 'tạm dừng (xóa mềm)' : 'khôi phục hoạt động'
  if (!confirm(`Bạn có chắc chắn muốn ${actionText} tòa nhà "${building.tenToaNha}"?`)) return
  try {
    if (building.maToaNha && typeof building.maToaNha === 'number' && building.maToaNha < 1000000000000) {
      if (isCurrentlyActive) {
        await apiRequest(`/api/master-data/buildings/${building.maToaNha}`, { method: 'DELETE' }).catch(() => null)
      } else {
        await apiRequest(`/api/master-data/buildings/${building.maToaNha}`, {
          method: 'PUT',
          body: JSON.stringify({
            maDonVi: building.maDonVi || 1,
            maCodeToaNha: building.maCodeToaNha,
            tenToaNha: building.tenToaNha,
            soTang: building.soTang,
            diaChi: building.diaChi,
            conHoatDong: true
          })
        }).catch(() => null)
      }
    }
    building.conHoatDong = !isCurrentlyActive
    bghApi.invalidate('/api/bgh/master-data/buildings')
  } catch (e) {
    alert(e?.message || 'Lỗi thay đổi trạng thái tòa nhà')
  }
}

function openBuildingDetail(buildingId) {
  router.push(`/bgh/facilities/${buildingId}`)
}

onMounted(() => { loadData() })
</script>
