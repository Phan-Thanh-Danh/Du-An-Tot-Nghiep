<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  ShieldAlert, Search, User, Building, Users, CheckCircle2, Wrench, X, AlertTriangle, Lightbulb, Loader2
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import { staffApi } from '@/services/staffApi'
import { usePopupStore } from '@/stores/popup'

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'

const popupStore = usePopupStore()
const loading = ref(true)
const apiError = ref('')

const DEMO_CONFLICTS = [
  { id: 1, loai: 'giang_vien', mucDo: 'critical', doiTuong: 'TS. Nguyễn Văn A', thoiGian: 'Thứ 2 · Ca 1-3', moTa: 'Giảng viên bị xếp lịch dạy 2 lớp cùng giờ: SE1601 và SE1602.', trangThaiXuLy: 'chua_xu_ly', deXuat: 'Chuyển lớp SE1602 sang ThS. Trần Thị B hoặc đổi ca cho SE1601.' },
  { id: 2, loai: 'phong_hoc', mucDo: 'major', doiTuong: 'P.302', thoiGian: 'Thứ 3 · Ca 1-2', moTa: 'Phòng đã có lịch học Java (SE1601) nhưng lại được xếp thêm CTDL (SE1602).', trangThaiXuLy: 'chua_xu_ly', deXuat: 'Chuyển CTDL sang P.305 (cùng tầng, sức chứa tương đương).' },
  { id: 3, loai: 'lop_hoc', mucDo: 'major', doiTuong: 'SE1603', thoiGian: 'Thứ 4 · Ca 4-5', moTa: 'Lớp SE1603 đã có lịch học CSDL nhưng tiếp tục được thêm lịch học Web trong cùng ca.', trangThaiXuLy: 'dang_xu_ly', deXuat: 'Dời lịch Web sang Thứ 5 · Ca 1-3 (phòng trống).' },
  { id: 4, loai: 'giang_vien', mucDo: 'minor', doiTuong: 'ThS. Trần Thị Lan', thoiGian: 'Thứ 5 · Ca 3-4', moTa: 'Giảng viên có lịch họp khoa trùng với lịch dạy.', trangThaiXuLy: 'da_xu_ly', deXuat: 'Đã điều chỉnh lịch dạy sang Thứ 6. Không còn xung đột.' },
]

const conflicts = ref([])
const selected = ref(null)
const confirmAction = ref({ isOpen: false, title: '', message: '', label: '', variant: 'primary', run: null })
const searchQuery = ref('')
const filterLoai = ref('')
const filterMucDo = ref('')

const isChecking = ref(false)

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.checkConflicts()
    conflicts.value = res?.items ?? res ?? []
  } catch (err) {
    if (ENABLE_MOCK_API) {
      conflicts.value = DEMO_CONFLICTS
    } else {
      apiError.value = err?.message || 'Không thể tải danh sách xung đột.'
    }
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })

// ── Computed ───────────────────────────────────────────────────
const stats = computed(() => ({
  total: conflicts.value.length,
  giangVien: conflicts.value.filter(c => c.loai === 'giang_vien').length,
  phongHoc: conflicts.value.filter(c => c.loai === 'phong_hoc').length,
  lopHoc: conflicts.value.filter(c => c.loai === 'lop_hoc').length,
  chuaXuLy: conflicts.value.filter(c => c.trangThaiXuLy === 'chua_xu_ly').length
}))

const filtered = computed(() => {
  let list = conflicts.value
  if (filterLoai.value) list = list.filter(c => c.loai === filterLoai.value)
  if (filterMucDo.value) list = list.filter(c => c.mucDo === filterMucDo.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.doiTuong.toLowerCase().includes(q) || c.moTa.toLowerCase().includes(q))
  }
  return list
})

const loaiLabel = l => ({ giang_vien: 'Giảng viên', phong_hoc: 'Phòng học', lop_hoc: 'Lớp học' }[l] || l)
const mucDoLabel = m => ({ critical: 'Nghiêm trọng', major: 'Trung bình', minor: 'Nhẹ' }[m] || m)
const mucDoVariant = m => ({ critical: 'danger', major: 'warning', minor: 'info' }[m] || 'neutral')
const xuLyLabel = s => ({ chua_xu_ly: 'Chưa xử lý', dang_xu_ly: 'Đang xử lý', da_xu_ly: 'Đã xử lý' }[s] || s)
const xuLyVariant = s => ({ chua_xu_ly: 'danger', dang_xu_ly: 'warning', da_xu_ly: 'success' }[s] || 'neutral')
const loaiIcon = l => ({ giang_vien: User, phong_hoc: Building, lop_hoc: Users }[l] || ShieldAlert)

function performCheck() {
  isChecking.value = true
  setTimeout(() => {
    isChecking.value = false
  }, 700)
}

function applyFix(c) {
  confirmAction.value = {
    isOpen: true,
    title: 'Áp dụng đề xuất?',
    message: c.deXuat,
    label: 'Áp dụng',
    variant: 'primary',
    run: () => {
      const idx = conflicts.value.findIndex(x => x.id === c.id)
      if (idx !== -1) conflicts.value.splice(idx, 1)
      if (selected.value?.id === c.id) selected.value = null
      confirmAction.value.isOpen = false
    }
  }
}

function runConfirm() {
  if (confirmAction.value.run) confirmAction.value.run()
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-(--text-muted)" :size="28" />
      <p class="text-sm text-(--text-muted)">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-(--border-card) rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-(--text-heading)">Không thể tải dữ liệu</p>
      <p class="text-xs text-(--text-muted)">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <!-- Header -->
      <div class="flex items-start justify-between flex-wrap gap-4">
        <div>
          <div class="flex items-center gap-2">
            <ShieldAlert class="text-amber-500" :size="24" />
            <h1 class="text-xl font-bold text-(--text-heading)">Kiểm tra xung đột</h1>
          </div>
          <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Phát hiện và xử lý xung đột giảng viên, lớp học, phòng học trước khi xuất bản.</p>
        </div>
        <div class="flex gap-2">
          <GlassButton variant="primary" @click="performCheck" :disabled="isChecking">
            <Wrench v-if="!isChecking" :size="15" class="mr-1" />
            <span v-if="isChecking" class="w-3 h-3 rounded-full border-2 border-white border-t-transparent animate-spin mr-2"></span>
            {{ isChecking ? 'Đang kiểm tra...' : 'Kiểm tra toàn hệ thống' }}
          </GlassButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4 h-24">
        <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Tổng xung đột</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.total }}</p>
          </div>
          <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--color-danger-bg) text-(--color-danger-text)">
            <ShieldAlert :size="20" />
          </div>
        </div>
        <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Giảng viên</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.giangVien }}</p>
          </div>
          <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--accent-primary-soft) text-(--lg-primary)">
            <User :size="20" />
          </div>
        </div>
        <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Phòng học</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.phongHoc }}</p>
          </div>
          <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--color-success-bg) text-(--color-success-text)">
            <Building :size="20" />
          </div>
        </div>
        <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
          <div>
            <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Lớp học</p>
            <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.lopHoc }}</p>
          </div>
          <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--accent-violet-soft) text-(--accent-violet)">
            <Users :size="20" />
          </div>
        </div>
      </div>

      <!-- Main Content -->
      <div class="flex gap-4 flex-1 min-h-0 flex-col lg:flex-row">

        <!-- List -->
        <div class="flex-1 surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col min-w-0 overflow-hidden">
          <div class="p-3 border-b border-(--border-default) flex items-center justify-between bg-(--surface-input)">
            <div class="flex gap-2">
              <select v-model="filterLoai" class="bg-(--surface-card) border border-(--border-card) rounded-lg px-2 py-1.5 text-xs text-(--text-body) outline-none focus:border-(--lg-primary)">
                <option value="">Tất cả loại</option>
                <option value="giang_vien">Giảng viên</option>
                <option value="phong_hoc">Phòng học</option>
                <option value="lop_hoc">Lớp học</option>
              </select>
              <select v-model="filterMucDo" class="bg-(--surface-card) border border-(--border-card) rounded-lg px-2 py-1.5 text-xs text-(--text-body) outline-none focus:border-(--lg-primary)">
                <option value="">Mọi mức độ</option>
                <option value="critical">Nghiêm trọng</option>
                <option value="major">Trung bình</option>
              </select>
            </div>
            <div class="relative">
              <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="14" />
              <input v-model="searchQuery" type="text" placeholder="Tìm xung đột..." class="pl-8 pr-3 h-8 bg-(--surface-card) border border-(--border-input) rounded-lg text-xs text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus) w-48" />
            </div>
          </div>

          <div class="flex-1 overflow-auto p-4 space-y-3 bg-transparent">
            <div v-for="c in filtered" :key="c.id"
                 @click="selected = c"
                 class="surface-card border rounded-xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden"
                 :class="selected?.id === c.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary)' : 'border-(--border-card)'">

                 <div class="absolute left-0 top-0 bottom-0 w-1"
                      :class="c.mucDo === 'critical' ? 'bg-red-500' : c.mucDo === 'major' ? 'bg-amber-500' : 'bg-blue-500'"></div>

                 <div class="pl-2 flex justify-between items-start">
                    <div>
                      <div class="flex items-center gap-2 mb-1">
                        <component :is="loaiIcon(c.loai)" :size="14" class="text-(--text-muted)" />
                        <span class="text-xs font-mono font-bold text-(--text-heading)">{{ loaiLabel(c.loai) }}</span>
                        <GlassBadge :variant="mucDoVariant(c.mucDo)" size="xs">{{ mucDoLabel(c.mucDo) }}</GlassBadge>
                      </div>
                      <h3 class="font-bold text-(--text-heading) text-base mt-1">{{ c.doiTuong }}</h3>
                      <p class="text-sm text-(--color-danger-text) font-medium mt-1">{{ c.moTa }}</p>
                    </div>

                    <div class="text-right flex flex-col items-end gap-2">
                      <GlassBadge :variant="xuLyVariant(c.trangThaiXuLy)" size="xs">{{ xuLyLabel(c.trangThaiXuLy) }}</GlassBadge>
                      <p class="text-xs font-medium text-(--text-muted) bg-(--surface-input) px-2 py-1 rounded">{{ c.thoiGian }}</p>
                    </div>
                 </div>
            </div>

            <div v-if="filtered.length === 0" class="flex flex-col items-center justify-center p-8 text-(--text-muted)">
              <CheckCircle2 :size="48" class="opacity-20 mb-3" />
              <p>Không tìm thấy xung đột nào.</p>
            </div>
          </div>
        </div>

        <!-- Detail Panel -->
        <div v-if="selected" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
          <div class="surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
            <div class="p-4 border-b border-(--border-default) flex justify-between items-center bg-(--surface-input)">
              <h3 class="font-bold text-(--text-heading)">Gợi ý xử lý</h3>
              <button class="text-(--text-muted) hover:text-(--text-heading)" @click="selected = null"><X :size="16" /></button>
            </div>

            <div class="p-4 flex-1 overflow-auto space-y-5">
              <div>
                <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Phân tích xung đột</p>
                <div class="space-y-2 text-sm text-(--text-body)">
                  <div class="flex justify-between"><span class="text-(--text-muted)">Đối tượng:</span> <span class="font-medium text-right">{{ selected.doiTuong }}</span></div>
                  <div class="flex justify-between"><span class="text-(--text-muted)">Mức độ:</span>
                    <GlassBadge :variant="mucDoVariant(selected.mucDo)" size="xs">{{ mucDoLabel(selected.mucDo) }}</GlassBadge>
                  </div>
                  <div class="flex justify-between"><span class="text-(--text-muted)">Thời gian:</span> <span class="font-medium">{{ selected.thoiGian }}</span></div>
                </div>
              </div>

              <div class="p-3 bg-(--color-danger-bg) border border-(--color-danger-border) rounded-xl">
                <p class="text-xs font-bold text-(--color-danger-text) flex items-center gap-1 mb-1"><AlertTriangle :size="14"/> Mô tả lỗi</p>
                <p class="text-sm text-(--color-danger-text) opacity-90">{{ selected.moTa }}</p>
              </div>

              <div v-if="selected.deXuat" class="p-3 bg-(--color-success-bg) border border-(--color-success-border) rounded-xl">
                <p class="text-xs font-bold text-(--color-success-text) flex items-center gap-1 mb-1"><Lightbulb :size="14"/> Đề xuất từ hệ thống</p>
                <p class="text-sm text-(--color-success-text) opacity-90 font-medium">{{ selected.deXuat }}</p>
              </div>

              <div v-if="!selected.deXuat" class="p-3 bg-(--surface-input) rounded-xl text-center">
                <p class="text-sm text-(--text-muted)">Hệ thống không có đề xuất tự động. Cần xử lý thủ công.</p>
              </div>
            </div>

            <div class="p-4 border-t border-(--border-default) space-y-2">
              <GlassButton v-if="selected.deXuat" variant="primary" class="w-full justify-center" @click="applyFix(selected)">Áp dụng gợi ý</GlassButton>
              <GlassButton variant="secondary" class="w-full justify-center text-link">Sửa lịch thủ công</GlassButton>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div v-else class="w-full lg:w-80 shrink-0 hidden lg:flex flex-col items-center justify-center p-6 text-center border-2 border-dashed border-(--border-card) rounded-2xl">
          <div class="w-12 h-12 rounded-full bg-(--surface-input) flex items-center justify-center text-(--text-muted) mb-3">
            <Wrench :size="24" />
          </div>
          <p class="text-sm font-medium text-(--text-heading)">Chọn một xung đột</p>
          <p class="text-xs text-(--text-muted) mt-1">Chọn một dòng bên trái để xem nguyên nhân và gợi ý xử lý tự động.</p>
        </div>
      </div>
    </template>
  </div>

  <ConfirmActionDialog
    :is-open="confirmAction.isOpen"
    :title="confirmAction.title"
    :message="confirmAction.message"
    :confirm-text="confirmAction.label"
    @confirm="runConfirm"
    @cancel="confirmAction.isOpen = false"
  />
</template>
