<script setup>
/**
 * SystemModulesView.vue - Super Admin
 * Giao diện quản lý trạng thái hoạt động của các phân hệ chức năng (Modules) toàn hệ thống.
 * Hỗ trợ Toggle bật tắt, Campus Override, Phase management, Confirm Modal và lưu vết audit log.
 */
import { ref, computed , onMounted} from 'vue'
import { apiRequest } from '@/services/apiClient'
import {
  Settings,
  Power,
  Search,
  Filter,
  RotateCcw,
  CheckCircle,
  AlertTriangle,
  X,
  Building,
  Layers,
  Info,
  Clock,
  User
} from 'lucide-vue-next'

// --- Mock Data cho System Modules ---
const modulesMock = ref([])

// --- State Bộ lọc ---
const searchQuery = ref('')
const selectedStatus = ref('all') // 'all', 'Enabled', 'Disabled', 'Partial'
const selectedPhase = ref('all') // 'all', '1', '2', '3'
const selectedCampus = ref('all') // 'all', 'HN', 'HCM', 'DN'

const filteredModules = computed(() => {
  return modulesMock.value.filter(mod => {
    // Lọc theo tên hoặc mã code
    const matchSearch = !searchQuery.value || 
      mod.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      mod.code.toLowerCase().includes(searchQuery.value.toLowerCase())

    // Lọc theo trạng thái
    const matchStatus = selectedStatus.value === 'all' || mod.status === selectedStatus.value

    // Lọc theo giai đoạn Phase
    const matchPhase = selectedPhase.value === 'all' || mod.phase.toString() === selectedPhase.value

    // Lọc theo Campus hoạt động (tìm xem campus đó có được Enabled không)
    let matchCampus = true
    if (selectedCampus.value !== 'all') {
      matchCampus = mod.campuses[selectedCampus.value] === true
    }

    return matchSearch && matchStatus && matchPhase && matchCampus
  })
})

const resetFilters = () => {
  searchQuery.value = ''
  selectedStatus.value = 'all'
  selectedPhase.value = 'all'
  selectedCampus.value = 'all'
}

// --- KPI Metrics ---
const totalModulesCount = computed(() => modulesMock.value.length)
const enabledCount = computed(() => modulesMock.value.filter(m => m.status === 'Enabled').length)
const partialCount = computed(() => modulesMock.value.filter(m => m.status === 'Partial').length)
const disabledCount = computed(() => modulesMock.value.filter(m => m.status === 'Disabled').length)

// --- State Confirm Change Status Modal ---
const isConfirmModalOpen = ref(false)
const pendingModule = ref(null)
const targetStatus = ref('')
const hasNotifiedUsers = ref(false)
const changeReason = ref('')

const openConfirmModal = (mod, target) => {
  if (mod.isCore && target === 'Disabled') {
    triggerToast(`Không thể tắt phân hệ cốt lõi: ${mod.name}!`, 'error')
    return
  }
  pendingModule.value = mod
  targetStatus.value = target
  hasNotifiedUsers.value = false
  changeReason.value = ''
  isConfirmModalOpen.value = true
}

// --- State Campus Override Modal ---
const isCampusModalOpen = ref(false)
const activeModule = ref(null)
const tempCampuses = ref({ HN: false, HCM: false, DN: false })
const campusOverrideReason = ref('')

const openCampusModal = (mod) => {
  activeModule.value = mod
  tempCampuses.value = { ...mod.campuses }
  campusOverrideReason.value = ''
  isCampusModalOpen.value = true
}

// --- Toast States ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success') // 'success' | 'error' | 'info'

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 4000)
}

// --- Xử lý Đổi trạng thái Module chính ---
const handleConfirmChangeStatus = () => {
  if (!pendingModule.value) return
  if (!changeReason.value.trim()) {
    triggerToast('Vui lòng nhập lý do thay đổi trạng thái module!', 'error')
    return
  }

  const mod = pendingModule.value
  const target = targetStatus.value

  mod.status = target
  // Đồng bộ trạng thái cho tất cả các Campus
  const boolVal = target === 'Enabled'
  Object.keys(mod.campuses).forEach(key => {
    mod.campuses[key] = boolVal
  })

  // Cập nhật người dùng & thời gian
  const timeString = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  mod.updatedBy = 'super_admin@fpt.edu.vn'
  mod.updatedAt = timeString

  isConfirmModalOpen.value = false
  triggerToast(`Đã chuyển trạng thái module ${mod.code} sang ${target} thành công!`, 'success')
}

// --- Xử lý Cấu hình Override Campus ---
const handleSaveCampusOverride = () => {
  if (!activeModule.value) return
  if (!campusOverrideReason.value.trim()) {
    triggerToast('Vui lòng nhập lý do điều chỉnh cấu hình cơ sở!', 'error')
    return
  }

  const mod = activeModule.value
  mod.campuses = { ...tempCampuses.value }

  // Tính toán lại trạng thái chung tổng quát
  const vals = Object.values(mod.campuses)
  const trueCount = vals.filter(v => v === true).length

  if (trueCount === vals.length) {
    mod.status = 'Enabled'
  } else if (trueCount === 0) {
    mod.status = 'Disabled'
  } else {
    mod.status = 'Partial'
  }

  // Cập nhật người dùng & thời gian
  const timeString = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  mod.updatedBy = 'super_admin@fpt.edu.vn'
  mod.updatedAt = timeString

  isCampusModalOpen.value = false
  triggerToast(`Đã lưu cấu hình cơ sở cho module ${mod.code} thành công!`, 'success')
}

onMounted(async () => {
  try {
    const res = await apiRequest('/api/super-admin/system/modules')
    if (Array.isArray(res)) {
      modulesMock.value = res
    }
  } catch (error) {
    console.error('Failed to load data for modulesMock', error)
  }
})

</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs trang trí 3D mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
          <Settings class="w-8 h-8 text-primary" />
          Bật / Tắt Phân Hệ Chức Năng (System Modules)
        </h1>
        <p class="text-sm text-muted mt-1">
          Quản lý trạng thái hoạt động của các phân hệ chức năng trên toàn hệ thống hoặc ghi đè cấu hình hoạt động độc lập cho từng cơ sở (Campus Override).
        </p>
      </div>

      <!-- KPI Dashboard Mini -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <!-- Tổng số module -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Layers class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Tổng số phân hệ</div>
            <div class="text-xl font-bold mt-0.5 text-heading">{{ totalModulesCount }} modules</div>
          </div>
        </div>

        <!-- Đã bật (Enabled) -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <Power class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Đang bật (Enabled)</div>
            <div class="text-xl font-bold mt-0.5 text-emerald-500 font-extrabold">{{ enabledCount }} modules</div>
          </div>
        </div>

        <!-- Bật một phần (Partial) -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Building class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Bật một phần (Partial)</div>
            <div class="text-xl font-bold mt-0.5 text-amber-500 font-extrabold">{{ partialCount }} modules</div>
          </div>
        </div>

        <!-- Đã tắt (Disabled) -->
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500">
            <Power class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted uppercase">Đang tắt (Disabled)</div>
            <div class="text-xl font-bold mt-0.5 text-rose-500 font-extrabold">{{ disabledCount }} modules</div>
          </div>
        </div>
      </div>

      <!-- Công cụ Tìm kiếm & Bộ Lọc Nâng Cao -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Tìm kiếm & Lọc phân hệ hệ thống</h3>
          </div>
          <button 
            @click="resetFilters" 
            class="text-xs text-link font-bold flex items-center gap-1 hover:underline"
          >
            <RotateCcw class="w-3.5 h-3.5" />
            Reset bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <!-- Tìm kiếm theo Tên/Mã -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Tên hoặc Mã Module</label>
            <div class="relative">
              <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
              <input 
                type="text" 
                v-model="searchQuery" 
                placeholder="Nhập M1, Đăng ký môn..."
                class="w-full pl-9 pr-3 lg-control text-sm"
              />
            </div>
          </div>

          <!-- Lọc Giai đoạn Phase -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Giai đoạn triển khai (Phase)</label>
            <select v-model="selectedPhase" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả giai đoạn</option>
              <option value="1">Phase 1 - Core Features</option>
              <option value="2">Phase 2 - Value Add</option>
              <option value="3">Phase 3 - Advanced AI</option>
            </select>
          </div>

          <!-- Lọc trạng thái -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Trạng thái chung</label>
            <select v-model="selectedStatus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="Enabled">Đã bật (Enabled)</option>
              <option value="Disabled">Đã tắt (Disabled)</option>
              <option value="Partial">Bật một phần (Partial)</option>
            </select>
          </div>

          <!-- Lọc theo Campus hoạt động -->
          <div>
            <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Hoạt động tại cơ sở</label>
            <select v-model="selectedCampus" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả cơ sở</option>
              <option value="HN">Campus Hà Nội (HN)</option>
              <option value="HCM">Campus TP. Hồ Chí Minh (HCM)</option>
              <option value="DN">Campus Đà Nẵng (DN)</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Bảng Modules Table -->
      <div class="lg-table-shell overflow-x-auto w-full max-w-full mb-8">
        <table class="min-w-[1000px] w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap w-24">Mã Module</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Tên & Mô tả phân hệ</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap w-28">Giai đoạn</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap">Bản đồ cơ sở (Campus Override)</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap w-36">Trạng thái chung</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Cập nhật cuối</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase whitespace-nowrap min-w-[220px] w-[220px]">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredModules.length === 0">
              <td colspan="7" class="px-4 py-12 text-center text-muted">
                <div class="flex flex-col items-center gap-2">
                  <Layers class="w-8 h-8 text-muted" />
                  <span>Không tìm thấy phân hệ nào phù hợp.</span>
                </div>
              </td>
            </tr>

            <tr v-for="mod in filteredModules" :key="mod.id" class="transition-colors hover:bg-surface-table-row-hover">
              <!-- Mã Module -->
              <td class="px-4 py-4 whitespace-nowrap font-mono font-bold text-primary text-xs">
                {{ mod.code }}
              </td>

              <!-- Tên & Mô tả -->
              <td class="px-4 py-4 max-w-sm">
                <div class="font-extrabold text-heading text-xs flex items-center gap-1.5">
                  {{ mod.name }}
                  <span v-if="mod.isCore" class="text-[9px] bg-sky-500/10 text-sky-600 dark:text-sky-400 px-1.5 py-0.2 rounded border border-sky-300 font-extrabold whitespace-nowrap">
                    Core
                  </span>
                </div>
                <div class="text-[10px] text-muted leading-relaxed mt-1" :title="mod.description">
                  {{ mod.description }}
                </div>
              </td>

              <!-- Giai đoạn (Phase) -->
              <td class="px-4 py-4 text-center whitespace-nowrap">
                <span 
                  class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold whitespace-nowrap"
                  :class="{
                    'bg-sky-500/10 text-sky-600 dark:text-sky-400 border-sky-300': mod.phase === 1,
                    'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-300': mod.phase === 2,
                    'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-300': mod.phase === 3
                  }"
                >
                  Phase {{ mod.phase }}
                </span>
              </td>

              <!-- Bản đồ cơ sở -->
              <td class="px-4 py-4 text-center">
                <div class="flex items-center justify-center gap-1.5 text-[9px] font-extrabold">
                  <span 
                    v-for="(val, campus) in mod.campuses" 
                    :key="campus"
                    class="px-1.5 py-0.5 rounded border"
                    :class="val ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-300' : 'bg-rose-500/10 text-rose-500 border-rose-300'"
                    :title="val ? `Đã bật tại Campus ${campus}` : `Đã tắt tại Campus ${campus}`"
                  >
                    {{ campus }}
                  </span>
                </div>
              </td>

              <!-- Trạng thái chung -->
              <td class="px-4 py-4 text-center whitespace-nowrap">
                <span 
                  class="px-2.5 py-0.5 rounded-full border text-[10px] font-extrabold tracking-wide whitespace-nowrap"
                  :class="{
                    'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-300': mod.status === 'Enabled',
                    'bg-rose-500/10 text-rose-500 border-rose-300': mod.status === 'Disabled',
                    'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-300': mod.status === 'Partial'
                  }"
                >
                  <span v-if="mod.status === 'Enabled'">Đã bật</span>
                  <span v-else-if="mod.status === 'Disabled'">Đã tắt</span>
                  <span v-else>Bật một phần</span>
                </span>
              </td>

              <!-- Cập nhật cuối -->
              <td class="px-4 py-4 whitespace-nowrap">
                <div class="flex items-center gap-1.5 text-xs text-heading font-semibold">
                  <User class="w-3.5 h-3.5 text-muted flex-shrink-0" />
                  <span class="truncate max-w-[120px]" :title="mod.updatedBy">{{ mod.updatedBy }}</span>
                </div>
                <div class="text-[10px] text-muted flex items-center gap-1 mt-0.5">
                  <Clock class="w-3 h-3 text-slate-400 flex-shrink-0" />
                  {{ mod.updatedAt }}
                </div>
              </td>

              <!-- Thao tác -->
              <td class="px-4 py-4 text-center whitespace-nowrap min-w-[220px] w-[220px]">
                <div class="flex items-center justify-center gap-2">
                  <!-- Nút Toggle gạt nhanh -->
                  <button
                    @click="openConfirmModal(mod, mod.status === 'Enabled' ? 'Disabled' : 'Enabled')"
                    class="lg-icon-button p-1.5 rounded-lg border border-default transition-all flex items-center justify-center gap-1"
                    :class="mod.status === 'Enabled' ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 hover:bg-emerald-500/20' : 'bg-rose-500/10 text-rose-500 hover:bg-rose-500/20'"
                    :title="mod.status === 'Enabled' ? 'Click để tắt phân hệ' : 'Click để bật phân hệ'"
                    :disabled="mod.isCore"
                  >
                    <Power class="w-3.5 h-3.5" />
                    <span class="text-[10px] font-bold">{{ mod.status === 'Enabled' ? 'Bật' : 'Tắt' }}</span>
                  </button>

                  <!-- Nút Cấu hình override -->
                  <button
                    @click="openCampusModal(mod)"
                    class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"
                    title="Cấu hình ghi đè trạng thái cho từng Campus"
                  >
                    <Building class="w-3.5 h-3.5" />
                    Campus
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Confirm Change Status Modal (Modal xác nhận đổi trạng thái) -->
    <div 
      v-if="isConfirmModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <!-- Close button -->
        <button 
          @click="isConfirmModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
          <div 
            class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0"
            :class="targetStatus === 'Enabled' ? 'bg-emerald-500/10 text-emerald-500' : 'bg-rose-500/10 text-rose-500'"
          >
            <Power class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Xác Nhận Thay Đổi Trạng Thái</h3>
            <p class="text-xs text-muted mt-0.5">Xác nhận chuyển trạng thái hoạt động của phân hệ chức năng.</p>
          </div>
        </div>

        <!-- Form nội dung -->
        <div class="space-y-4 mb-5">
          <!-- Cảnh báo an toàn dữ liệu -->
          <div class="p-3.5 rounded-lg bg-amber-500/5 border border-amber-500/15 text-xs flex items-start gap-2.5 text-amber-700 dark:text-amber-400">
            <Info class="w-4.5 h-4.5 flex-shrink-0 mt-0.5" />
            <div class="leading-relaxed">
              <strong>Quy tắc nghiệp vụ bảo toàn dữ liệu</strong>: Việc vô hiệu hóa phân hệ chỉ ẩn tính năng trên giao diện và chặn API tương ứng. Hệ thống <strong>tuyệt đối không xóa dữ liệu</strong> lịch sử đã có của phân hệ này trong database.
            </div>
          </div>

          <!-- Chi tiết thay đổi -->
          <div class="text-xs space-y-1 p-3 rounded-lg bg-surface-card border border-default">
            <div><span class="text-muted">Phân hệ:</span> <strong class="text-heading font-extrabold">{{ pendingModule?.name }} ({{ pendingModule?.code }})</strong></div>
            <div><span class="text-muted">Trạng thái mới:</span> 
              <span 
                class="font-extrabold text-[10px] px-2 py-0.5 rounded border ml-1.5"
                :class="targetStatus === 'Enabled' ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-300' : 'bg-rose-500/10 text-rose-500 border-rose-300'"
              >
                {{ targetStatus === 'Enabled' ? 'KÍCH HOẠT (ENABLED)' : 'TẠM TẮT (DISABLED)' }}
              </span>
            </div>
          </div>

          <!-- Checkbox xác nhận thông báo người dùng -->
          <div class="flex items-start gap-2">
            <input 
              type="checkbox" 
              id="notify-check" 
              v-model="hasNotifiedUsers" 
              class="w-4.5 h-4.5 text-primary border-default rounded focus:ring-primary mt-0.5"
            />
            <label for="notify-check" class="text-xs text-slate-700 dark:text-slate-300 leading-normal font-semibold cursor-pointer">
              Xác nhận đã gửi thông báo bảo trì/điều chỉnh hệ thống trước cho các đối tượng người dùng bị ảnh hưởng (Sinh viên, Giảng viên).
            </label>
          </div>

          <!-- Lý do thay đổi -->
          <div>
            <label class="block text-xs font-bold text-label mb-2 uppercase">Lý do điều chỉnh (Bắt buộc)</label>
            <textarea
              v-model="changeReason"
              rows="3"
              placeholder="Nhập lý do chi tiết điều chỉnh trạng thái module để lưu vết audit log..."
              class="w-full px-3 py-2 lg-control text-xs leading-relaxed"
            ></textarea>
            <span v-if="!changeReason.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Bắt buộc nhập lý do thay đổi</span>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="isConfirmModalOpen = false"
            class="lg-btn-secondary px-4 py-2 text-sm font-bold"
          >
            Hủy
          </button>
          <button
            @click="handleConfirmChangeStatus"
            :disabled="!changeReason.trim() || !hasNotifiedUsers"
            class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Power class="w-4 h-4" />
            Đồng ý thay đổi
          </button>
        </div>
      </div>
    </div>

    <!-- Campus Override Configuration Modal (Cấu hình ghi đè cơ sở) -->
    <div 
      v-if="isCampusModalOpen" 
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <!-- Close button -->
        <button 
          @click="isCampusModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1"
        >
          <X class="w-4.5 h-4.5" />
        </button>

        <!-- Header -->
        <div class="flex items-start gap-3.5 mb-4 border-b border-default pb-3">
          <div class="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
            <Building class="w-5.5 h-5.5" />
          </div>
          <div>
            <h3 class="text-base font-extrabold text-heading">Cấu Hình Ghi Đè Cơ Sở</h3>
            <p class="text-xs text-muted mt-0.5">Bật / Tắt hoạt động của phân hệ tại các campus thành viên độc lập.</p>
          </div>
        </div>

        <!-- Form cấu hình các campus -->
        <div class="space-y-4 mb-5">
          <div class="p-3.5 rounded-lg bg-violet-500/5 border border-violet-500/10 text-xs flex items-start gap-2.5 text-violet-700 dark:text-violet-400">
            <Info class="w-4.5 h-4.5 flex-shrink-0 mt-0.5" />
            <div class="leading-relaxed">
              Bạn có thể thiết lập bật tại campus này nhưng tắt tại campus khác. Trạng thái chung sẽ tự động chuyển thành **Bật một phần (Partial)**.
            </div>
          </div>

          <div class="text-xs font-semibold text-heading mb-2 uppercase tracking-wide">
            Module: {{ activeModule?.name }} ({{ activeModule?.code }})
          </div>

          <!-- Danh sách Campus gạt bật/tắt -->
          <div class="space-y-3 p-4 rounded-xl bg-surface-card border border-default">
            <!-- Campus Hà Nội -->
            <div class="flex items-center justify-between">
              <span class="text-xs font-bold text-heading">Campus Hà Nội (HN)</span>
              <button 
                @click="tempCampuses.HN = !tempCampuses.HN"
                class="w-11 h-6 rounded-full p-1 transition-colors duration-200 focus:outline-none"
                :class="tempCampuses.HN ? 'bg-emerald-500' : 'bg-slate-400 dark:bg-slate-600'"
              >
                <div 
                  class="bg-white w-4 h-4 rounded-full shadow-md transform transition-transform duration-200"
                  :class="tempCampuses.HN ? 'translate-x-5' : 'translate-x-0'"
                ></div>
              </button>
            </div>

            <!-- Campus Hồ Chí Minh -->
            <div class="flex items-center justify-between border-t border-default/50 pt-3">
              <span class="text-xs font-bold text-heading">Campus TP. Hồ Chí Minh (HCM)</span>
              <button 
                @click="tempCampuses.HCM = !tempCampuses.HCM"
                class="w-11 h-6 rounded-full p-1 transition-colors duration-200 focus:outline-none"
                :class="tempCampuses.HCM ? 'bg-emerald-500' : 'bg-slate-400 dark:bg-slate-600'"
              >
                <div 
                  class="bg-white w-4 h-4 rounded-full shadow-md transform transition-transform duration-200"
                  :class="tempCampuses.HCM ? 'translate-x-5' : 'translate-x-0'"
                ></div>
              </button>
            </div>

            <!-- Campus Đà Nẵng -->
            <div class="flex items-center justify-between border-t border-default/50 pt-3">
              <span class="text-xs font-bold text-heading">Campus Đà Nẵng (DN)</span>
              <button 
                @click="tempCampuses.DN = !tempCampuses.DN"
                class="w-11 h-6 rounded-full p-1 transition-colors duration-200 focus:outline-none"
                :class="tempCampuses.DN ? 'bg-emerald-500' : 'bg-slate-400 dark:bg-slate-600'"
              >
                <div 
                  class="bg-white w-4 h-4 rounded-full shadow-md transform transition-transform duration-200"
                  :class="tempCampuses.DN ? 'translate-x-5' : 'translate-x-0'"
                ></div>
              </button>
            </div>
          </div>

          <!-- Lý do bắt buộc -->
          <div>
            <label class="block text-xs font-bold text-label mb-2 uppercase">Lý do điều chỉnh (Bắt buộc)</label>
            <textarea
              v-model="campusOverrideReason"
              rows="2.5"
              placeholder="Nhập lý do chi tiết điều chỉnh cấu hình cơ sở để phục vụ kiểm toán hệ thống..."
              class="w-full px-3 py-2 lg-control text-xs leading-relaxed"
            ></textarea>
            <span v-if="!campusOverrideReason.trim()" class="text-[10px] text-rose-500 font-semibold mt-1 block">Bắt buộc nhập lý do thay đổi</span>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="flex items-center justify-end gap-2.5">
          <button
            @click="isCampusModalOpen = false"
            class="lg-btn-secondary px-4 py-2 text-sm font-bold"
          >
            Hủy
          </button>
          <button
            @click="handleSaveCampusOverride"
            :disabled="!campusOverrideReason.trim()"
            class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5 disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <CheckCircle class="w-4 h-4" />
            Lưu cấu hình
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
