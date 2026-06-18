<template>
  <div class="space-y-4 pb-10">
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Cơ sở vật chất</h2>
        <p class="text-xs text-muted mt-1">Quản lý tòa nhà, tầng và phòng học trên toàn hệ thống</p>
      </div>
      <div class="flex gap-2">
        <select v-model="campusFilter" class="px-3 py-2 bg-[var(--surface-input)] border border-input rounded-lg text-sm text-body focus:outline-none focus:border-[var(--lg-primary)]">
          <option value="">Tất cả cơ sở</option>
          <option v-for="c in campuses" :key="c.maDonVi" :value="c.maDonVi">{{ c.tenDonVi }}</option>
        </select>
      </div>
    </div>

    <div v-for="building in filteredBuildings" :key="building.maToaNha" class="surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div @click="toggleBuilding(building.maToaNha)" class="px-5 py-4 flex items-center justify-between cursor-pointer hover:bg-[var(--surface-input)]/30 transition-colors">
        <div class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white"><Building2 :size="20" /></div>
          <div>
            <h3 class="text-base font-bold text-heading">{{ building.tenToaNha }}</h3>
            <p class="text-xs text-muted">{{ building.maCodeToaNha }} · {{ building.diaChi || building.tenDonVi }} · {{ building.soTang }} tầng</p>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <span :class="building.conHoatDong ? 'text-[var(--color-success-text)]' : 'text-[var(--color-danger-text)]'" class="text-xs font-bold flex items-center gap-1">
            <span :class="building.conHoatDong ? 'bg-[var(--color-success-text)]' : 'bg-[var(--color-danger-text)]'" class="h-1.5 w-1.5 rounded-full inline-block" />
            {{ building.conHoatDong ? 'Đang hoạt động' : 'Ngừng' }}
          </span>
          <ChevronDown v-if="expandedBuilding === building.maToaNha" :size="18" class="text-muted transition-transform" />
          <ChevronRight v-else :size="18" class="text-muted transition-transform" />
        </div>
      </div>

      <Transition enter-active-class="transition-all duration-300" enter-from-class="max-h-0 opacity-0" enter-to-class="max-h-[2000px] opacity-100" leave-active-class="transition-all duration-200" leave-from-class="max-h-[2000px] opacity-100" leave-to-class="max-h-0 opacity-0">
        <div v-if="expandedBuilding === building.maToaNha" class="border-t border-default overflow-hidden">
          <div v-for="floor in getFloors(building.maToaNha)" :key="floor.maTang" class="border-b border-default last:border-b-0">
            <div @click="toggleFloor(floor.maTang)" class="px-5 py-3 flex items-center justify-between cursor-pointer hover:bg-[var(--surface-input)]/20 transition-colors ml-4">
              <div class="flex items-center gap-2">
                <ChevronDown v-if="expandedFloor === floor.maTang" :size="14" class="text-muted" />
                <ChevronRight v-else :size="14" class="text-muted" />
                <span class="text-sm font-semibold text-heading">{{ floor.tenTang }}</span>
                <span class="text-xs text-muted">(Tầng {{ floor.thuTuTang }})</span>
                <span v-if="!floor.conHoatDong" class="text-[10px] text-[var(--color-danger-text)] bg-[var(--color-danger-bg)] px-1.5 py-0.5 rounded">Ngừng</span>
              </div>
              <span class="text-xs text-muted">{{ getRooms(floor.maTang).length }} phòng</span>
            </div>

            <Transition enter-active-class="transition-all duration-200" enter-from-class="max-h-0 opacity-0" enter-to-class="max-h-[500px] opacity-100" leave-active-class="transition-all duration-200" leave-from-class="max-h-[500px] opacity-100" leave-to-class="max-h-0 opacity-0">
              <div v-if="expandedFloor === floor.maTang" class="overflow-hidden">
                <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3 p-4 pt-0 ml-8">
                  <div v-for="room in getRooms(floor.maTang)" :key="room.maPhong" class="p-3 rounded-xl border border-default hover:border-[var(--border-input-focus)] hover:shadow-sm transition-all bg-[var(--surface-card)]">
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
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Building2, ChevronDown, ChevronRight, Users } from 'lucide-vue-next'

const campusFilter = ref('')
const expandedBuilding = ref(null)
const expandedFloor = ref(null)

function toggleBuilding(id) {
  expandedBuilding.value = expandedBuilding.value === id ? null : id
  expandedFloor.value = null
}

function toggleFloor(id) {
  expandedFloor.value = expandedFloor.value === id ? null : id
}

const campuses = [
  { maDonVi: 1, tenDonVi: 'FPT Polytechnic Hồ Chí Minh' },
  { maDonVi: 2, tenDonVi: 'FPT Polytechnic Đà Nẵng' },
  { maDonVi: 3, tenDonVi: 'FPT Polytechnic Cần Thơ' },
]

const buildings = [
  { maToaNha: 1, maDonVi: 1, maCodeToaNha: 'PTO', tenToaNha: 'PTO Building', diaChi: 'Quận 12, TP.HCM', soTang: 5, tenDonVi: 'FPT Polytechnic Hồ Chí Minh', conHoatDong: true },
  { maToaNha: 2, maDonVi: 1, maCodeToaNha: 'FBT', tenToaNha: 'FBT Tower', diaChi: 'Quận 12, TP.HCM', soTang: 3, tenDonVi: 'FPT Polytechnic Hồ Chí Minh', conHoatDong: true },
  { maToaNha: 3, maDonVi: 2, maCodeToaNha: 'DNG', tenToaNha: 'Đà Nẵng Campus', diaChi: 'Quận Hải Châu, Đà Nẵng', soTang: 4, tenDonVi: 'FPT Polytechnic Đà Nẵng', conHoatDong: true },
  { maToaNha: 4, maDonVi: 3, maCodeToaNha: 'CTO', tenToaNha: 'Cần Thơ Campus', diaChi: 'Quận Ninh Kiều, Cần Thơ', soTang: 3, tenDonVi: 'FPT Polytechnic Cần Thơ', conHoatDong: false },
]

const floors = [
  { maTang: 1, maToaNha: 1, tenTang: 'Tầng 1', thuTuTang: 1, conHoatDong: true },
  { maTang: 2, maToaNha: 1, tenTang: 'Tầng 2', thuTuTang: 2, conHoatDong: true },
  { maTang: 3, maToaNha: 1, tenTang: 'Tầng 3', thuTuTang: 3, conHoatDong: true },
  { maTang: 4, maToaNha: 1, tenTang: 'Tầng 4', thuTuTang: 4, conHoatDong: true },
  { maTang: 5, maToaNha: 1, tenTang: 'Tầng 5', thuTuTang: 5, conHoatDong: false },
  { maTang: 6, maToaNha: 2, tenTang: 'Tầng Trệt', thuTuTang: 1, conHoatDong: true },
  { maTang: 7, maToaNha: 2, tenTang: 'Tầng 1', thuTuTang: 2, conHoatDong: true },
  { maTang: 8, maToaNha: 2, tenTang: 'Tầng 2', thuTuTang: 3, conHoatDong: true },
  { maTang: 9, maToaNha: 3, tenTang: 'Tầng 1', thuTuTang: 1, conHoatDong: true },
  { maTang: 10, maToaNha: 3, tenTang: 'Tầng 2', thuTuTang: 2, conHoatDong: true },
  { maTang: 11, maToaNha: 3, tenTang: 'Tầng 3', thuTuTang: 3, conHoatDong: true },
  { maTang: 12, maToaNha: 4, tenTang: 'Tầng 1', thuTuTang: 1, conHoatDong: false },
]

const rooms = [
  { maPhong: 1, maDonVi: 1, maToaNha: 1, maTang: 1, maCodePhong: 'PTO.101', tenPhong: 'Phòng 101', sucChua: 40, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 2, maDonVi: 1, maToaNha: 1, maTang: 1, maCodePhong: 'PTO.102', tenPhong: 'Phòng 102', sucChua: 35, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 3, maDonVi: 1, maToaNha: 1, maTang: 1, maCodePhong: 'PTO.103', tenPhong: 'Phòng Lab 103', sucChua: 25, loaiPhong: 'thuc_hanh', loaiPhongLabel: 'Thực hành', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 4, maDonVi: 1, maToaNha: 1, maTang: 2, maCodePhong: 'PTO.201', tenPhong: 'Phòng 201', sucChua: 40, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 5, maDonVi: 1, maToaNha: 1, maTang: 2, maCodePhong: 'PTO.202', tenPhong: 'Phòng 202', sucChua: 30, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'bao_tri' },
  { maPhong: 6, maDonVi: 1, maToaNha: 1, maTang: 2, maCodePhong: 'PTO.203', tenPhong: 'Phòng Lab 203', sucChua: 25, loaiPhong: 'thuc_hanh', loaiPhongLabel: 'Thực hành', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 7, maDonVi: 1, maToaNha: 1, maTang: 3, maCodePhong: 'PTO.301', tenPhong: 'Phòng 301', sucChua: 50, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 8, maDonVi: 1, maToaNha: 1, maTang: 3, maCodePhong: 'PTO.302', tenPhong: 'Phòng 302', sucChua: 45, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 9, maDonVi: 1, maToaNha: 1, maTang: 4, maCodePhong: 'PTO.401', tenPhong: 'Phòng 401', sucChua: 80, loaiPhong: 'hoi_truong', loaiPhongLabel: 'Hội trường', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 10, maDonVi: 1, maToaNha: 2, maTang: 6, maCodePhong: 'FBT.G01', tenPhong: 'Phòng G01', sucChua: 30, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 11, maDonVi: 1, maToaNha: 2, maTang: 7, maCodePhong: 'FBT.101', tenPhong: 'Phòng 101', sucChua: 35, loaiPhong: 'thuc_hanh', loaiPhongLabel: 'Thực hành', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 12, maDonVi: 2, maToaNha: 3, maTang: 9, maCodePhong: 'DNG.101', tenPhong: 'Phòng 101', sucChua: 40, loaiPhong: 'ly_thuyet', loaiPhongLabel: 'Lý thuyết', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 13, maDonVi: 2, maToaNha: 3, maTang: 10, maCodePhong: 'DNG.201', tenPhong: 'Phòng 201', sucChua: 35, loaiPhong: 'thuc_hanh', loaiPhongLabel: 'Thực hành', trangThaiPhong: 'dang_su_dung' },
  { maPhong: 14, maDonVi: 2, maToaNha: 3, maTang: 11, maCodePhong: 'DNG.301', tenPhong: 'Phòng 301', sucChua: 80, loaiPhong: 'hoi_truong', loaiPhongLabel: 'Hội trường', trangThaiPhong: 'ngung_hoat_dong' },
]

const filteredBuildings = computed(() => {
  if (!campusFilter.value) return buildings
  return buildings.filter(b => b.maDonVi === parseInt(campusFilter.value))
})

function getFloors(buildingId) {
  return floors.filter(f => f.maToaNha === buildingId)
}

function getRooms(floorId) {
  return rooms.filter(r => r.maTang === floorId)
}

function roomTypeBadge(type) {
  switch (type) {
    case 'ly_thuyet': return 'text-[10px] px-1.5 py-0.5 rounded bg-[var(--color-info-bg)] text-[var(--color-info-text)] font-bold'
    case 'thuc_hanh': return 'text-[10px] px-1.5 py-0.5 rounded bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] font-bold'
    case 'hoi_truong': return 'text-[10px] px-1.5 py-0.5 rounded bg-[var(--color-success-bg)] text-[var(--color-success-text)] font-bold'
    default: return 'text-[10px] px-1.5 py-0.5 rounded bg-[var(--surface-input)] text-muted'
  }
}

function roomStatusBadge(status) {
  switch (status) {
    case 'dang_su_dung': return 'text-[10px] font-bold text-[var(--color-success-text)]'
    case 'bao_tri': return 'text-[10px] font-bold text-[var(--color-warning-text)]'
    case 'ngung_hoat_dong': return 'text-[10px] font-bold text-[var(--color-danger-text)]'
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
</script>
