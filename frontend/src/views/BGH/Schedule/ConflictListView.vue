<script setup>
import { ref, computed } from 'vue'
import { ShieldAlert, Search } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import TableShell from '@/components/ui/TableShell.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { scheduleConflictRows } from '@/mocks/scheduleAttendanceMockData'

const conflicts = ref(scheduleConflictRows.map(c => ({ ...c })))
const selectedConflict = ref(null)
const searchQuery = ref('')
const filterLoai = ref('')
const filterMucDo = ref('')

const summaryCards = computed(() => [
  { label: 'Tổng xung đột', value: conflicts.value.length, border: 'border-(--border-default)' },
  { label: 'Trùng giảng viên', value: conflicts.value.filter(c => c.loai === 'giang_vien').length, border: 'border-red-500' },
  { label: 'Trùng lớp học', value: conflicts.value.filter(c => c.loai === 'lop_hoc').length, border: 'border-amber-500' },
  { label: 'Trùng phòng học', value: conflicts.value.filter(c => c.loai === 'phong_hoc').length, border: 'border-blue-500' },
])

const filteredConflicts = computed(() => {
  let list = conflicts.value
  if (filterLoai.value) list = list.filter(c => c.loai === filterLoai.value)
  if (filterMucDo.value) list = list.filter(c => c.mucDo === filterMucDo.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.doiTuong.toLowerCase().includes(q) || c.moTa.toLowerCase().includes(q))
  }
  return list
})

const loaiLabel = (l) => ({ giang_vien: 'Giảng viên', phong_hoc: 'Phòng học', lop_hoc: 'Lớp học' }[l] || l)
const mucDoLabel = (m) => ({ critical: 'Nghiêm trọng', major: 'Trung bình', minor: 'Nhẹ' }[m] || m)
const mucDoVariant = (m) => ({ critical: 'danger', major: 'warning', minor: 'info' }[m] || 'neutral')
const xuLyLabel = (s) => ({ chua_xu_ly: 'Chưa xử lý', dang_xu_ly: 'Đang xử lý', da_xu_ly: 'Đã xử lý' }[s] || s)
const xuLyVariant = (s) => ({ chua_xu_ly: 'danger', dang_xu_ly: 'warning', da_xu_ly: 'success' }[s] || 'neutral')
</script>

<template>
  <div class="bgh-conflicts max-w-7xl mx-auto space-y-5">
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <ShieldAlert class="text-red-500" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Giám sát Xung đột Lịch học</h1>
      </div>
      <p class="text-(--text-body)">Theo dõi và giám sát các xung đột tài nguyên (giảng viên, lớp, phòng) trên toàn hệ thống.</p>
    </GlassPanel>

    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <GlassPanel v-for="c in summaryCards" :key="c.label" variant="flat" density="compact" class="flex flex-col justify-center min-h-[88px]" :class="`border-l-4 ${c.border}`">
        <p class="text-sm font-medium text-(--text-muted) mb-1">{{ c.label }}</p>
        <strong class="text-2xl text-(--text-heading)">{{ c.value }}</strong>
      </GlassPanel>
    </div>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 items-center">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 h-10 rounded-lg border border-(--border-input) flex-1 min-w-[200px] focus-within:ring-2 focus-within:ring-(--border-focus) transition-shadow">
          <Search :size="16" class="text-(--text-muted) shrink-0" />
          <input v-model="searchQuery" type="text" placeholder="Tìm đối tượng, mô tả..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filterLoai" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả loại</option>
          <option value="giang_vien">Giảng viên</option>
          <option value="lop_hoc">Lớp học</option>
          <option value="phong_hoc">Phòng học</option>
        </select>
        <select v-model="filterMucDo" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả mức độ</option>
          <option value="critical">Nghiêm trọng</option>
          <option value="major">Trung bình</option>
          <option value="minor">Nhẹ</option>
        </select>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[400px]">
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto">
          <TableShell v-if="filteredConflicts.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Loại</th>
                  <th>Đối tượng</th>
                  <th>Lịch hiện tại</th>
                  <th>Mức độ</th>
                  <th>Tiết ảnh hưởng</th>
                  <th>Trạng thái xử lý</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="c in filteredConflicts" :key="c.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedConflict?.id === c.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedConflict = c"
                >
                  <td><GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ loaiLabel(c.loai) }}</GlassBadge></td>
                  <td class="text-sm font-medium max-w-[160px] truncate" :title="c.doiTuong">{{ c.doiTuong }}</td>
                  <td class="text-xs text-(--text-muted) max-w-[180px] truncate" :title="c.lichHienTai">{{ c.lichHienTai }}</td>
                  <td><GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ mucDoLabel(c.mucDo) }}</GlassBadge></td>
                  <td class="text-sm font-medium">{{ c.soTietAnhHuong }}</td>
                  <td><GlassBadge :variant="xuLyVariant(c.trangThaiXuLy)" size="sm">{{ xuLyLabel(c.trangThaiXuLy) }}</GlassBadge></td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không có xung đột" description="Không tìm thấy xung đột phù hợp bộ lọc hiện tại." />
          </div>
        </div>

        <div class="lg:col-span-1 bg-(--surface-card) flex flex-col">
          <div v-if="!selectedConflict" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một xung đột để xem chi tiết tác động
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center gap-2 mb-3 flex-wrap">
                <GlassBadge :variant="mucDoVariant(selectedConflict.mucDo)">{{ loaiLabel(selectedConflict.loai) }}</GlassBadge>
                <GlassBadge :variant="mucDoVariant(selectedConflict.mucDo)" size="sm">{{ mucDoLabel(selectedConflict.mucDo) }}</GlassBadge>
              </div>
              <h3 class="font-bold text-base text-(--text-heading)">{{ selectedConflict.doiTuong }}</h3>
              <p class="text-sm text-(--text-muted) mt-0.5 font-mono">{{ selectedConflict.id }}</p>
            </div>
            <div class="p-5 space-y-4 flex-1 overflow-y-auto">
              <div>
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Mô tả</div>
                <p class="text-sm text-(--text-body) leading-relaxed">{{ selectedConflict.moTa }}</p>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-xl font-bold text-(--text-heading)">{{ selectedConflict.soTietAnhHuong }}</div>
                  <div class="text-xs text-(--text-muted)">Tiết bị ảnh hưởng</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <GlassBadge :variant="xuLyVariant(selectedConflict.trangThaiXuLy)" class="justify-center">{{ xuLyLabel(selectedConflict.trangThaiXuLy) }}</GlassBadge>
                  <div class="text-xs text-(--text-muted) mt-1">Trạng thái xử lý</div>
                </div>
              </div>
              <div class="bg-(--color-warning-bg) border border-amber-300/40 rounded-lg p-3">
                <div class="text-xs font-semibold text-(--color-warning-text) uppercase tracking-wide mb-1">Đề xuất xử lý</div>
                <p class="text-sm text-(--text-body)">{{ selectedConflict.deXuat }}</p>
              </div>
              <div class="bg-(--surface-modal) border border-(--border-default) rounded-lg p-3">
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-1">Lưu ý cho BGH</div>
                <p class="text-xs text-(--text-muted)">BGH giám sát và nhắc Phòng Đào tạo xử lý. Không sửa trực tiếp TKB từ màn này.</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>
  </div>
</template>
