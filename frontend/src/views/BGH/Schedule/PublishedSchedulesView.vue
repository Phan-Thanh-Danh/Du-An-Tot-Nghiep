<script setup>
import { ref, computed } from 'vue'
import { CheckCircle, Search } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import TableShell from '@/components/ui/TableShell.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { bghPublishedSchedules } from '@/mocks/scheduleAttendanceMockData'
import { formatDate } from '@/utils/dateFormat'

const schedules = ref(bghPublishedSchedules.map(s => ({ ...s })))
const selectedItem = ref(null)
const searchQuery = ref('')
const filterHocKy = ref('')

const summaryCards = computed(() => [
  { label: 'Đã công bố', value: schedules.value.length, border: 'border-emerald-500' },
  { label: 'Buổi học tuần này', value: schedules.value.reduce((a, b) => a + b.tongSoTiet, 0), border: 'border-blue-500' },
  { label: 'Thay đổi phát sinh', value: schedules.value.reduce((a, b) => a + b.thayDoiPhatSinh, 0), border: 'border-amber-500' },
  { label: 'Buổi bị hủy', value: schedules.value.reduce((a, b) => a + b.buoiHuy, 0), border: 'border-red-500' },
])

const filteredSchedules = computed(() => {
  let list = schedules.value
  if (filterHocKy.value) list = list.filter(s => s.tenHocKy === filterHocKy.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(s => s.maTkb.toLowerCase().includes(q) || s.tenDonVi.toLowerCase().includes(q) || s.tenHocKy.toLowerCase().includes(q))
  }
  return list
})
</script>

<template>
  <div class="bgh-published max-w-7xl mx-auto space-y-5">
    <GlassPanel variant="flat" density="compact">
      <div class="flex items-center gap-3 mb-1">
        <CheckCircle class="text-emerald-500" :size="24" />
        <h1 class="text-2xl font-bold text-(--text-heading)">Thời khóa biểu đã công bố</h1>
      </div>
      <p class="text-(--text-body)">Xem và theo dõi các bộ TKB đã được phê duyệt và công bố chính thức.</p>
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
          <input v-model="searchQuery" type="text" placeholder="Tìm mã, đơn vị, học kỳ..." class="bg-transparent border-none outline-none w-full text-sm text-(--text-body)" />
        </label>
        <select v-model="filterHocKy" class="h-10 px-3 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm outline-none focus:ring-2 focus:ring-(--border-focus) min-w-[140px]">
          <option value="">Tất cả học kỳ</option>
          <option>Spring 2026</option><option>Fall 2025</option>
        </select>
        <GlassButton variant="secondary" class="h-10 shrink-0">Xuất dữ liệu</GlassButton>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 min-h-[400px]">
        <div class="lg:col-span-2 border-r border-(--border-default) overflow-x-auto">
          <TableShell v-if="filteredSchedules.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Mã lịch</th>
                  <th>Đơn vị</th>
                  <th>Học kỳ</th>
                  <th>Quy mô</th>
                  <th>Ngày công bố</th>
                  <th>Thay đổi / Hủy</th>
                  <th>Trạng thái</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="s in filteredSchedules" :key="s.id"
                  class="cursor-pointer transition-colors"
                  :class="selectedItem?.id === s.id ? 'bg-(--surface-hover)' : 'hover:bg-(--surface-hover)'"
                  @click="selectedItem = s"
                >
                  <td class="font-mono text-sm text-(--text-muted) whitespace-nowrap">{{ s.maTkb }}</td>
                  <td class="text-sm font-medium max-w-[160px] line-clamp-1" :title="s.tenDonVi">{{ s.tenDonVi }}</td>
                  <td class="text-sm whitespace-nowrap">{{ s.tenHocKy }}</td>
                  <td class="text-sm">
                    <div class="text-xs text-(--text-muted)">{{ s.soLop }} lớp · {{ s.soGiaoVien }} GV</div>
                    <div class="text-xs text-(--text-muted)">{{ s.tongSoTiet }} tiết/tuần</div>
                  </td>
                  <td class="text-sm">{{ formatDate(s.ngayXuatBan) }}</td>
                  <td>
                    <div class="flex gap-1 flex-wrap">
                      <GlassBadge v-if="s.thayDoiPhatSinh > 0" variant="warning" size="sm">{{ s.thayDoiPhatSinh }} thay đổi</GlassBadge>
                      <GlassBadge v-if="s.buoiHuy > 0" variant="danger" size="sm">{{ s.buoiHuy }} hủy</GlassBadge>
                      <GlassBadge v-if="s.thayDoiPhatSinh === 0 && s.buoiHuy === 0" variant="success" size="sm">Ổn định</GlassBadge>
                    </div>
                  </td>
                  <td><GlassBadge variant="success" size="sm">Đã công bố</GlassBadge></td>
                </tr>
              </tbody>
            </table>
          </TableShell>
          <div v-else class="p-8">
            <EmptyState title="Không có lịch công bố" description="Chưa có lịch nào được công bố hoặc không khớp bộ lọc." />
          </div>
        </div>

        <div class="lg:col-span-1 bg-(--surface-card) flex flex-col">
          <div v-if="!selectedItem" class="h-full flex items-center justify-center p-6 text-center text-(--text-muted) text-sm">
            Chọn một lịch để xem chi tiết
          </div>
          <div v-else class="flex flex-col h-full">
            <div class="p-5 border-b border-(--border-default)">
              <div class="flex items-center gap-2 mb-3">
                <GlassBadge variant="success">Đã công bố</GlassBadge>
              </div>
              <h3 class="font-bold text-base text-(--text-heading)">{{ selectedItem.maTkb }}</h3>
              <p class="text-sm text-(--text-muted) mt-0.5">{{ selectedItem.tenDonVi }}</p>
              <p class="text-xs text-(--text-muted) mt-0.5">{{ selectedItem.tenHocKy }}</p>
            </div>
            <div class="p-5 space-y-4 flex-1 overflow-y-auto">
              <div class="grid grid-cols-2 gap-3">
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-xl font-bold text-(--text-heading)">{{ selectedItem.soLop }}</div>
                  <div class="text-xs text-(--text-muted)">Lớp học</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-xl font-bold text-(--text-heading)">{{ selectedItem.soGiaoVien }}</div>
                  <div class="text-xs text-(--text-muted)">Giảng viên</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-xl font-bold text-(--text-heading)">{{ selectedItem.tongSoTiet }}</div>
                  <div class="text-xs text-(--text-muted)">Tiết/tuần</div>
                </div>
                <div class="bg-(--surface-input) p-3 rounded-lg border border-(--border-default) text-center">
                  <div class="text-xl font-bold text-(--text-heading)">{{ formatDate(selectedItem.ngayXuatBan) }}</div>
                  <div class="text-xs text-(--text-muted)">Ngày công bố</div>
                </div>
              </div>

              <div>
                <div class="text-xs font-semibold text-(--text-muted) uppercase tracking-wide mb-2">Biến động sau công bố</div>
                <div class="space-y-2">
                  <div class="flex items-center justify-between bg-(--surface-input) p-3 rounded-lg border border-(--border-default)">
                    <span class="text-sm text-(--text-body)">Thay đổi phát sinh</span>
                    <GlassBadge :variant="selectedItem.thayDoiPhatSinh > 0 ? 'warning' : 'success'" size="sm">{{ selectedItem.thayDoiPhatSinh }}</GlassBadge>
                  </div>
                  <div class="flex items-center justify-between bg-(--surface-input) p-3 rounded-lg border border-(--border-default)">
                    <span class="text-sm text-(--text-body)">Buổi bị hủy</span>
                    <GlassBadge :variant="selectedItem.buoiHuy > 0 ? 'danger' : 'success'" size="sm">{{ selectedItem.buoiHuy }}</GlassBadge>
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
