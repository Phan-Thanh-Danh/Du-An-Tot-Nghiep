<script setup>
import { ref, computed } from 'vue'
import {
  ShieldAlert, Search, User, Building, Users, CheckCircle2, Wrench,
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import {
  caHocCatalog, thuTrongTuanOptions, scheduleConflictRows,
} from '@/mocks/scheduleAttendanceMockData'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

const conflicts = ref(scheduleConflictRows.map(c => ({ ...c })))
const selected = ref(null)
const confirmAction = ref(null)
const searchQuery = ref('')
const filterLoai = ref('')
const filterMucDo = ref('')

// check form
const form = ref({ hocKy: 'Spring 2026', thu: 2, caHocId: 'ca1', giaoVienId: '', lopId: '', phongId: '' })
const isChecking = ref(false)

// ── Computed ───────────────────────────────────────────────────
const stats = computed(() => ({
  total: conflicts.value.length,
  giangVien: conflicts.value.filter(c => c.loai === 'giang_vien').length,
  phongHoc: conflicts.value.filter(c => c.loai === 'phong_hoc').length,
  lopHoc: conflicts.value.filter(c => c.loai === 'lop_hoc').length,
  chuaXuLy: conflicts.value.filter(c => c.trangThaiXuLy === 'chua_xu_ly').length,
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
    popupStore.info('Kiểm tra xong', `Tìm thấy ${conflicts.value.length} xung đột trong lịch học hiện tại.`)
  }, 700)
}

function applyFix(c) {
  confirmAction.value = {
    title: 'Áp dụng đề xuất?',
    message: c.deXuat,
    label: 'Áp dụng',
    variant: 'primary',
    run: () => {
      const idx = conflicts.value.findIndex(x => x.id === c.id)
      if (idx !== -1) conflicts.value.splice(idx, 1)
      if (selected.value?.id === c.id) selected.value = null
      confirmAction.value = null
      popupStore.success('Đã xử lý', 'Xung đột đã được đánh dấu giải quyết.')
    }
  }
}
</script>

<template>
  <div class="conflict-view space-y-4 max-w-full">

    <!-- Header -->
    <div class="flex items-start gap-3 justify-between flex-wrap">
      <div>
        <div class="flex items-center gap-2">
          <ShieldAlert class="text-amber-500" :size="22" />
          <h1 class="text-xl font-bold text-(--text-heading)">Kiểm tra xung đột lịch học</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Phát hiện và xử lý xung đột giảng viên, lớp học, phòng học trước khi xuất bản.</p>
      </div>
    </div>

    <!-- Stat pills -->
    <div class="flex flex-wrap gap-2">
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
        <span class="font-bold text-xl text-(--text-heading)">{{ stats.total }}</span>
        <span class="text-(--text-muted)">Tổng xung đột</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-red-200 dark:border-red-800 bg-red-50/80 dark:bg-red-950/30 text-sm">
        <span class="font-bold text-xl text-red-600 dark:text-red-400">{{ stats.chuaXuLy }}</span>
        <span class="text-(--text-muted)">Chưa xử lý</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
        <User :size="14" class="text-(--text-muted)" />
        <span class="font-bold text-(--text-heading)">{{ stats.giangVien }}</span>
        <span class="text-(--text-muted)">GV</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
        <Building :size="14" class="text-(--text-muted)" />
        <span class="font-bold text-(--text-heading)">{{ stats.phongHoc }}</span>
        <span class="text-(--text-muted)">Phòng</span>
      </div>
      <div class="flex items-center gap-2 px-4 py-2 rounded-full border border-(--border-default) bg-(--surface-input) text-sm">
        <Users :size="14" class="text-(--text-muted)" />
        <span class="font-bold text-(--text-heading)">{{ stats.lopHoc }}</span>
        <span class="text-(--text-muted)">Lớp</span>
      </div>
    </div>

    <!-- Check form card -->
    <div class="surface-card border border-(--border-card) rounded-2xl p-4 shadow-sm">
      <h2 class="text-sm font-bold text-(--text-heading) mb-3">Kiểm tra lịch trước khi thêm mới</h2>
      <div class="flex flex-wrap gap-2 items-end">
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Học kỳ</label>
          <select v-model="form.hocKy" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option>Spring 2026</option><option>Summer 2026</option><option>Fall 2025</option>
          </select>
        </div>
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Thứ</label>
          <select v-model="form.thu" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="t in thuTrongTuanOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </div>
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Ca học</label>
          <select v-model="form.caHocId" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="ca in caHocCatalog" :key="ca.id" :value="ca.id">{{ ca.tenCa }} · {{ ca.gioBatDau }}</option>
          </select>
        </div>
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mã GV</label>
          <input v-model="form.giaoVienId" type="text" placeholder="VD: GV001" class="h-9 w-28 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mã lớp</label>
          <input v-model="form.lopId" type="text" placeholder="SE1601" class="h-9 w-28 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
        <div>
          <label class="block text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mã phòng</label>
          <input v-model="form.phongId" type="text" placeholder="P302" class="h-9 w-28 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
        <GlassButton variant="primary" class="h-9 mt-auto" :loading="isChecking" @click="performCheck">
          <Search :size="15" class="mr-1" />
          {{ isChecking ? 'Đang kiểm tra...' : 'Kiểm tra ngay' }}
        </GlassButton>
      </div>
    </div>

    <!-- Main split area -->
    <div class="flex gap-4 items-start">

      <!-- Left: conflict list -->
      <div class="flex-1 min-w-0 surface-card border border-(--border-card) rounded-2xl shadow-sm overflow-hidden">
        <!-- Toolbar -->
        <div class="p-3 border-b border-(--border-default) flex flex-wrap gap-2 items-center">
          <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-9 rounded-lg border border-(--border-input) flex-1 min-w-[180px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
            <Search :size="14" class="text-(--text-muted) shrink-0" />
            <input v-model="searchQuery" type="text" placeholder="Tìm đối tượng, mô tả..." class="bg-transparent border-none outline-none text-sm text-(--text-body) w-full" />
          </label>
          <select v-model="filterLoai" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option value="">Tất cả loại</option>
            <option value="giang_vien">Giảng viên</option>
            <option value="lop_hoc">Lớp học</option>
            <option value="phong_hoc">Phòng học</option>
          </select>
          <select v-model="filterMucDo" class="h-9 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option value="">Tất cả mức độ</option>
            <option value="critical">Nghiêm trọng</option>
            <option value="major">Trung bình</option>
            <option value="minor">Nhẹ</option>
          </select>
          <span class="text-xs text-(--text-muted) ml-auto">{{ filtered.length }} kết quả</span>
        </div>

        <!-- Conflict rows -->
        <div v-if="filtered.length > 0" class="divide-y divide-(--border-default)">
          <div
            v-for="c in filtered" :key="c.id"
            class="flex items-center gap-3 px-4 py-3 cursor-pointer transition-colors"
            :class="selected?.id === c.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
            @click="selected = c"
          >
            <!-- Type icon -->
            <div
              class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0"
              :class="{
                'bg-red-50 dark:bg-red-950/40 text-red-500': c.mucDo === 'critical',
                'bg-amber-50 dark:bg-amber-950/30 text-amber-500': c.mucDo === 'major',
                'bg-blue-50 dark:bg-blue-950/30 text-blue-500': c.mucDo === 'minor',
              }"
            >
              <component :is="loaiIcon(c.loai)" :size="16" />
            </div>

            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-sm font-semibold text-(--text-heading) truncate">{{ c.doiTuong }}</span>
                <GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ mucDoLabel(c.mucDo) }}</GlassBadge>
              </div>
              <p class="text-xs text-(--text-muted) truncate mt-0.5">{{ c.lichHienTai }}</p>
            </div>

            <!-- Status + action -->
            <div class="flex items-center gap-2 shrink-0">
              <GlassBadge :variant="xuLyVariant(c.trangThaiXuLy)" size="sm">{{ xuLyLabel(c.trangThaiXuLy) }}</GlassBadge>
              <GlassButton variant="ghost" size="xs" @click.stop="applyFix(c)">
                <Wrench :size="13" />
              </GlassButton>
            </div>
          </div>
        </div>

        <!-- Empty -->
        <div v-else class="p-12 text-center">
          <CheckCircle2 :size="40" class="mx-auto text-emerald-400 mb-3" />
          <p class="font-semibold text-(--text-heading)">Không có xung đột</p>
          <p class="text-sm text-(--text-muted) mt-1">Lịch học không có xung đột với bộ lọc hiện tại.</p>
        </div>
      </div>

      <!-- Right: detail panel -->
      <transition name="panel-slide">
        <div
          v-if="selected"
          class="w-72 shrink-0 surface-card border border-(--border-card) rounded-2xl shadow-lg overflow-hidden"
          style="position: sticky; top: 80px"
        >
          <!-- Header -->
          <div
            class="px-4 py-3 flex items-center gap-2 border-b border-(--border-default)"
            :class="{
              'bg-red-50/80 dark:bg-red-950/30': selected.mucDo === 'critical',
              'bg-amber-50/80 dark:bg-amber-950/30': selected.mucDo === 'major',
              'bg-blue-50/80 dark:bg-blue-950/30': selected.mucDo === 'minor',
            }"
          >
            <component :is="loaiIcon(selected.loai)" :size="16" :class="{
              'text-red-500': selected.mucDo === 'critical',
              'text-amber-500': selected.mucDo === 'major',
              'text-blue-500': selected.mucDo === 'minor',
            }" />
            <span class="text-xs font-bold uppercase tracking-wide text-(--text-heading) flex-1">{{ loaiLabel(selected.loai) }}</span>
            <GlassBadge :variant="mucDoVariant(selected.mucDo)" size="sm">{{ mucDoLabel(selected.mucDo) }}</GlassBadge>
          </div>

          <!-- Content -->
          <div class="p-4 space-y-3">
            <div>
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-0.5">Đối tượng</p>
              <p class="text-base font-bold text-(--text-heading)">{{ selected.doiTuong }}</p>
              <p class="text-xs font-mono text-(--text-muted)">{{ selected.id }}</p>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Mô tả</p>
              <p class="text-sm text-(--text-body) leading-relaxed">{{ selected.moTa }}</p>
            </div>

            <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default)">
              <p class="text-[10px] font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Lịch hiện tại</p>
              <p class="text-sm text-(--text-body)">{{ selected.lichHienTai }}</p>
            </div>

            <div class="grid grid-cols-2 gap-2">
              <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default) text-center">
                <p class="text-lg font-bold text-(--text-heading)">{{ selected.soTietAnhHuong }}</p>
                <p class="text-[10px] text-(--text-muted)">Tiết bị ảnh hưởng</p>
              </div>
              <div class="bg-(--surface-input) rounded-xl p-3 border border-(--border-default) text-center flex flex-col items-center justify-center">
                <GlassBadge :variant="xuLyVariant(selected.trangThaiXuLy)" class="justify-center text-xs">{{ xuLyLabel(selected.trangThaiXuLy) }}</GlassBadge>
                <p class="text-[10px] text-(--text-muted) mt-1">Trạng thái</p>
              </div>
            </div>

            <div class="bg-amber-50/80 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800 rounded-xl p-3">
              <p class="text-[10px] font-semibold text-amber-700 dark:text-amber-400 uppercase tracking-wide mb-1">Đề xuất xử lý</p>
              <p class="text-sm text-(--text-body)">{{ selected.deXuat }}</p>
            </div>
          </div>

          <div class="px-4 pb-4">
            <GlassButton variant="primary" class="w-full h-9 justify-center text-sm" @click="applyFix(selected)">
              <Wrench :size="14" class="mr-1" /> Áp dụng đề xuất
            </GlassButton>
          </div>
        </div>
      </transition>
    </div>

    <ConfirmActionDialog
      v-if="confirmAction"
      :show="true"
      :title="confirmAction.title"
      :message="confirmAction.message"
      :confirmLabel="confirmAction.label"
      :variant="confirmAction.variant"
      @confirm="confirmAction.run"
      @cancel="confirmAction = null"
    />
  </div>
</template>

<style scoped>
.panel-slide-enter-active, .panel-slide-leave-active { transition: opacity .2s ease, transform .2s ease; }
.panel-slide-enter-from, .panel-slide-leave-to { opacity: 0; transform: translateX(16px); }
</style>
