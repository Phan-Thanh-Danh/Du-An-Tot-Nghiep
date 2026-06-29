<script setup>
import { ref, computed } from 'vue'
import { ArrowLeftRight, Search } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import TableShell from '@/components/ui/TableShell.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { bghScheduleChanges } from '@/mocks/scheduleAttendanceMockData'
import { formatDate } from '@/utils/dateFormat'

const changes = ref(bghScheduleChanges.map(c => ({ ...c })))
const selectedItem = ref(null)
const searchQuery = ref('')
const filterLoai = ref('')
const filterTrangThai = ref('')

const loaiLabel = (l) => ({
  day_thay: 'Dạy thay', doi_phong: 'Đổi phòng', doi_ca: 'Đổi ca', huy_buoi: 'Hủy buổi',
}[l] || l)

const loaiVariant = (l) => ({
  day_thay: 'info', doi_phong: 'warning', doi_ca: 'warning', huy_buoi: 'danger',
}[l] || 'neutral')

const ttLabel = (s) => ({ da_xac_nhan: 'Đã xác nhận', cho_xac_nhan: 'Chờ xác nhận' }[s] || s)
const ttVariant = (s) => ({ da_xac_nhan: 'success', cho_xac_nhan: 'warning' }[s] || 'neutral')

const summaryCards = computed(() => [
  { label: 'Tổng thay đổi', value: changes.value.length, border: 'border-(--border-default)' },
  { label: 'Đổi phòng', value: changes.value.filter(c => c.loaiThayDoi === 'doi_phong').length, border: 'border-amber-500' },
  { label: 'Dạy thay', value: changes.value.filter(c => c.loaiThayDoi === 'day_thay').length, border: 'border-blue-500' },
  { label: 'Hủy buổi', value: changes.value.filter(c => c.loaiThayDoi === 'huy_buoi').length, border: 'border-red-500' },
])

const filteredChanges = computed(() => {
  let list = changes.value
  if (filterLoai.value) list = list.filter(c => c.loaiThayDoi === filterLoai.value)
  if (filterTrangThai.value) list = list.filter(c => c.trangThai === filterTrangThai.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c =>
      c.lop.toLowerCase().includes(q) ||
      c.monHoc.toLowerCase().includes(q) ||
      c.giaoVien.toLowerCase().includes(q) ||
      c.id.toLowerCase().includes(q)
    )
  }
  return list
})
</script>

<template>
  <div class="bgh-changes max-w-7xl mx-auto space-y-5">
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <ArrowLeftRight class="text-(--lg-primary)" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Thay đổi lịch học phát sinh</h1>
      </div>
      <p class="text-(--text-body)">Theo dõi toàn bộ biến động lịch học sau khi Thời khóa biểu đã công bố: đổi phòng, đổi ca, dạy thay, hủy buổi.</p>
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
          <input v-model="searchQuery" type="text" placeholder="Tìm lớp, môn, GV, mã đơn..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filterLoai" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả loại</option>
          <option value="doi_phong">Đổi phòng</option>
          <option value="doi_ca">Đổi ca</option>
          <option value="day_thay">Dạy thay</option>
          <option value="huy_buoi">Hủy buổi</option>
        </select>
        <select v-model="filterTrangThai" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả trạng thái</option>
          <option value="da_xac_nhan">Đã xác nhận</option>
          <option value="cho_xac_nhan">Chờ xác nhận</option>
        </select>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[400px]">
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto">
          <TableShell v-if="filteredChanges.length > 0">
            <table>
              <thead>
                <tr>
                  <th class="whitespace-nowrap">Ngày học</th>
                  <th>Môn / Lớp</th>
                  <th>Loại thay đổi</th>
                  <th>Trước</th>
                  <th>Sau</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="c in filteredChanges" :key="c.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedItem?.id === c.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedItem = c"
                >
                  <td class="text-sm whitespace-nowrap">{{ formatDate(c.ngayHoc) }}</td>
                  <td>
                    <div class="text-sm font-medium line-clamp-1" :title="c.monHoc">{{ c.monHoc }}</div>
                    <div class="text-xs text-(--text-muted)">{{ c.lop }}</div>
                  </td>
                  <td><GlassBadge :variant="loaiVariant(c.loaiThayDoi)" size="sm">{{ loaiLabel(c.loaiThayDoi) }}</GlassBadge></td>
                  <td class="text-xs text-(--text-muted) max-w-[140px] line-clamp-1" :title="c.truoc">{{ c.truoc }}</td>
                  <td class="text-xs text-(--text-body) max-w-[140px] line-clamp-1" :title="c.sau">{{ c.sau }}</td>
                  <td><GlassBadge :variant="ttVariant(c.trangThai)" size="sm">{{ ttLabel(c.trangThai) }}</GlassBadge></td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không có thay đổi" description="Không tìm thấy thay đổi nào phù hợp bộ lọc." />
          </div>
        </div>

        <div class="lg:col-span-1 bg-(--surface-card) flex flex-col">
          <div v-if="!selectedItem" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một thay đổi để xem chi tiết và lịch sử
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center gap-2 mb-3">
                <GlassBadge :variant="loaiVariant(selectedItem.loaiThayDoi)">{{ loaiLabel(selectedItem.loaiThayDoi) }}</GlassBadge>
                <GlassBadge :variant="ttVariant(selectedItem.trangThai)" size="sm">{{ ttLabel(selectedItem.trangThai) }}</GlassBadge>
              </div>
              <h3 class="font-bold text-base text-(--text-heading)">{{ selectedItem.monHoc }}</h3>
              <p class="text-sm text-(--text-muted) mt-0.5">{{ selectedItem.lop }} · {{ formatDate(selectedItem.ngayHoc) }}</p>
              <p class="text-xs text-(--text-muted) font-mono mt-0.5">{{ selectedItem.id }}</p>
            </div>
            <div class="p-5 space-y-4 flex-1 overflow-y-auto">
              <div class="space-y-2">
                <div>
                  <div class="text-xs text-(--text-muted) mb-1">Giảng viên</div>
                  <div class="text-sm font-medium text-(--text-heading)">{{ selectedItem.giaoVien }}</div>
                </div>
                <div class="grid grid-cols-1 gap-2">
                  <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default)">
                    <div class="text-xs text-(--text-muted) mb-1">Trước thay đổi</div>
                    <div class="text-sm font-medium text-(--text-body)">{{ selectedItem.truoc }}</div>
                  </div>
                  <div class="bg-(--color-info-bg) p-3 rounded-lg border border-blue-300/40">
                    <div class="text-xs text-(--color-info-text) mb-1">Sau thay đổi</div>
                    <div class="text-sm font-medium text-(--text-body)">{{ selectedItem.sau }}</div>
                  </div>
                </div>
              </div>
              <div>
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Lý do thay đổi</div>
                <p class="text-sm text-(--text-body) leading-relaxed italic">"{{ selectedItem.lyDo }}"</p>
              </div>
              <div class="bg-(--surface-modal) border border-(--border-default) rounded-lg p-3">
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Lịch sử xử lý</div>
                <div class="flex items-start gap-2 text-xs">
                  <div class="w-1.5 h-1.5 rounded-full bg-emerald-500 mt-1 shrink-0"></div>
                  <div>
                    <div class="font-medium text-(--text-heading)">{{ selectedItem.nguoiCapNhat }}</div>
                    <div class="text-(--text-muted)">Cập nhật · {{ formatDate(selectedItem.ngayCapNhat) }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </GlassPanel>
  </div>
</template>
