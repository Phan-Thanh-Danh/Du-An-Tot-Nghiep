<script setup>
import { ref, computed } from 'vue'
import { AlertTriangle, Search, CheckCircle, User, Building, Users, ShieldAlert } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import {
  caHocCatalog, thuTrongTuanOptions, scheduleConflictRows,
} from '@/mocks/scheduleAttendanceMockData'
import { usePopupStore } from '@/stores/popup'

const popupStore = usePopupStore()

// ── State ──────────────────────────────────────────────────────
const isChecking = ref(false)
const checkDone = ref(false)
const conflicts = ref([...scheduleConflictRows])
const selectedConflict = ref(null)
const confirmAction = ref(null)

const form = ref({
  hocKy: 'Spring 2026',
  thu: 2,
  caHocId: 'ca1',
  giaoVienId: '',
  lopId: '',
  phongId: '',
})

// ── Summary ────────────────────────────────────────────────────
const summaryCards = computed(() => [
  { label: 'Tổng xung đột', value: conflicts.value.length, border: 'border-(--border-default)', icon: ShieldAlert },
  { label: 'Trùng giảng viên', value: conflicts.value.filter(c => c.loai === 'giang_vien').length, border: 'border-red-500', icon: User },
  { label: 'Trùng lớp học', value: conflicts.value.filter(c => c.loai === 'lop_hoc').length, border: 'border-amber-500', icon: Users },
  { label: 'Trùng phòng học', value: conflicts.value.filter(c => c.loai === 'phong_hoc').length, border: 'border-blue-500', icon: Building },
])

const mucDoVariant = (mucDo) => {
  if (mucDo === 'critical') return 'danger'
  if (mucDo === 'major') return 'warning'
  return 'info'
}

const mucDoLabel = (mucDo) => {
  if (mucDo === 'critical') return 'Nghiêm trọng'
  if (mucDo === 'major') return 'Trung bình'
  return 'Nhẹ'
}

const loaiLabel = (loai) => {
  if (loai === 'giang_vien') return 'Giảng viên'
  if (loai === 'phong_hoc') return 'Phòng học'
  if (loai === 'lop_hoc') return 'Lớp học'
  return loai
}

// ── Check ──────────────────────────────────────────────────────
function performCheck() {
  isChecking.value = true
  setTimeout(() => {
    isChecking.value = false
    checkDone.value = true
    popupStore.info('Kết quả kiểm tra', `Tìm thấy ${conflicts.value.length} xung đột.`)
  }, 800)
}

// ── Resolve actions ────────────────────────────────────────────
function applyFix(conflict) {
  confirmAction.value = {
    title: 'Áp dụng đề xuất xử lý?',
    message: conflict.deXuat,
    label: 'Áp dụng',
    variant: 'primary',
    run: () => {
      const idx = conflicts.value.findIndex(c => c.id === conflict.id)
      if (idx !== -1) conflicts.value.splice(idx, 1)
      if (selectedConflict.value?.id === conflict.id) selectedConflict.value = null
      confirmAction.value = null
      popupStore.success('Đã xử lý', 'Xung đột đã được đánh dấu giải quyết.')
    }
  }
}
</script>

<template>
  <div class="conflict-check max-w-7xl mx-auto space-y-5">
    <!-- Header -->
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <AlertTriangle class="text-amber-500" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Kiểm tra xung đột lịch học</h1>
      </div>
      <p class="text-(--text-body)">Phát hiện và xử lý xung đột giảng viên, lớp học, phòng học trước khi xuất bản thời khóa biểu.</p>
    </GlassPanel>

    <!-- Summary cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel
        v-for="card in summaryCards" :key="card.label"
        variant="flat" density="compact"
        class="flex flex-col justify-center min-h-[88px]"
        :class="`border-l-4 ${card.border}`"
      >
        <p class="text-sm font-medium text-(--text-muted) mb-1">{{ card.label }}</p>
        <strong class="text-2xl text-(--text-heading)">{{ card.value }}</strong>
      </GlassPanel>
    </div>

    <!-- Check form -->
    <GlassPanel variant="flat" density="compact">
      <h2 class="text-base font-bold text-(--text-heading) mb-4">Kiểm tra lịch mới</h2>
      <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3 items-end">
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Học kỳ</label>
          <select v-model="form.hocKy" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option>Spring 2026</option><option>Summer 2026</option><option>Fall 2025</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ</label>
          <select v-model="form.thu" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="t in thuTrongTuanOptions" :key="t.value" :value="t.value">{{ t.label }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ca học</label>
          <select v-model="form.caHocId" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
            <option v-for="ca in caHocCatalog" :key="ca.id" :value="ca.id">{{ ca.tenCa }} · {{ ca.gioBatDau }}</option>
          </select>
        </div>
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã giảng viên</label>
          <input v-model="form.giaoVienId" type="text" placeholder="VD: GV001" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã lớp</label>
          <input v-model="form.lopId" type="text" placeholder="VD: SE1601" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
        <div>
          <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã phòng</label>
          <input v-model="form.phongId" type="text" placeholder="VD: P302" class="w-full h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus)" />
        </div>
      </div>
      <div class="mt-4">
        <GlassButton variant="primary" class="h-10" :loading="isChecking" @click="performCheck">
          <Search :size="16" class="mr-1.5" />
          {{ isChecking ? 'Đang kiểm tra...' : 'Kiểm tra xung đột' }}
        </GlassButton>
      </div>
    </GlassPanel>

    <!-- Results -->
    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default)">
        <h2 class="text-base font-bold text-(--text-heading)">Danh sách xung đột</h2>
        <p class="text-sm text-(--text-muted) mt-0.5">{{ conflicts.length > 0 ? `Phát hiện ${conflicts.length} xung đột cần xử lý.` : checkDone ? 'Không có xung đột.' : 'Chưa chạy kiểm tra.' }}</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[400px]">
        <!-- Conflict list -->
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto">
          <TableShell v-if="conflicts.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Loại xung đột</th>
                  <th>Đối tượng</th>
                  <th>Lịch hiện tại</th>
                  <th>Mức độ</th>
                  <th class="text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="c in conflicts" :key="c.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedConflict?.id === c.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedConflict = c"
                >
                  <td>
                    <GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ loaiLabel(c.loai) }}</GlassBadge>
                  </td>
                  <td class="text-sm font-medium max-w-[160px] truncate" :title="c.doiTuong">{{ c.doiTuong }}</td>
                  <td class="text-xs text-(--text-muted) max-w-[180px] truncate" :title="c.lichHienTai">{{ c.lichHienTai }}</td>
                  <td>
                    <GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ mucDoLabel(c.mucDo) }}</GlassBadge>
                  </td>
                  <td class="text-right">
                    <GlassButton variant="ghost" size="xs" @click.stop="applyFix(c)">Xử lý</GlassButton>
                  </td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState
              :title="checkDone ? 'Không có xung đột' : 'Chưa kiểm tra'"
              :description="checkDone ? 'Lịch học không có xung đột với dữ liệu hiện tại.' : 'Điền thông tin và nhấn Kiểm tra để xem kết quả.'"
            />
            <div v-if="checkDone" class="flex justify-center mt-4">
              <div class="flex items-center gap-2 text-emerald-600 dark:text-emerald-400">
                <CheckCircle :size="20" /> <span class="font-medium">Không có xung đột</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Detail panel -->
        <div class="lg:col-span-1 bg-(--surface-card)">
          <div v-if="!selectedConflict" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một xung đột bên trái để xem chi tiết và đề xuất xử lý
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center gap-2 mb-3">
                <GlassBadge :variant="mucDoVariant(selectedConflict.mucDo)">{{ loaiLabel(selectedConflict.loai) }}</GlassBadge>
                <GlassBadge :variant="mucDoVariant(selectedConflict.mucDo)" size="sm">{{ mucDoLabel(selectedConflict.mucDo) }}</GlassBadge>
              </div>
              <h3 class="font-bold text-base text-(--text-heading)">{{ selectedConflict.doiTuong }}</h3>
              <p class="text-sm text-(--text-muted) mt-1 font-mono">{{ selectedConflict.id }}</p>
            </div>

            <div class="p-5 flex-1 space-y-4 overflow-y-auto">
              <div>
                <div class="text-xs text-(--text-muted) font-semibold uppercase tracking-wide mb-2">Mô tả xung đột</div>
                <p class="text-sm text-(--text-body) leading-relaxed">{{ selectedConflict.moTa }}</p>
              </div>

              <div class="grid grid-cols-1 gap-3">
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default)">
                  <div class="text-xs text-(--text-muted) mb-1">Lịch hiện tại</div>
                  <div class="text-sm font-medium text-(--text-heading)">{{ selectedConflict.lichHienTai }}</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default)">
                  <div class="text-xs text-(--text-muted) mb-1">Số tiết bị ảnh hưởng</div>
                  <div class="text-sm font-bold text-(--text-heading)">{{ selectedConflict.soTietAnhHuong }} tiết</div>
                </div>
              </div>

              <div class="bg-(--color-warning-bg) border border-amber-300/40 rounded-lg p-3">
                <div class="text-xs font-semibold text-(--color-warning-text) uppercase tracking-wide mb-1">Đề xuất xử lý</div>
                <p class="text-sm text-(--text-body)">{{ selectedConflict.deXuat }}</p>
              </div>
            </div>

            <div class="p-4 border-t border-(--border-default) bg-(--surface-modal)">
              <GlassButton variant="primary" class="w-full h-10 justify-center" @click="applyFix(selectedConflict)">
                Áp dụng đề xuất xử lý
              </GlassButton>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>

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
