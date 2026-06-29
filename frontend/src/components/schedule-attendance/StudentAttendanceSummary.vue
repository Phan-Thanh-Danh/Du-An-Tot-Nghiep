<script setup>
import { ref, onMounted } from 'vue'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import AttendanceStatusBadge from './AttendanceStatusBadge.vue'
import dayjs from 'dayjs'

const history = ref([])
const summary = ref({})
const loading = ref(false)

const fetchAttendance = async () => {
  loading.value = true
  try {
    summary.value = await scheduleAttendanceMockService.getStudentAttendanceSummary()
    const histRes = await scheduleAttendanceMockService.getStudentAttendanceHistory()
    history.value = histRes.items || []
  } finally {
    loading.value = false
  }
}
onMounted(() => fetchAttendance())
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar">
    <ScheduleMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Báo cáo Điểm danh</h1>
    
    <div v-if="loading" class="animate-pulse bg-[var(--surface-card)] h-24 rounded-xl mb-6"></div>
    <div v-else class="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--lg-primary)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Chuyên cần</div>
        <div class="text-2xl font-bold text-[var(--lg-primary)]">{{ summary.tyLe }}%</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-success-border)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Có mặt</div>
        <div class="text-2xl font-bold text-[var(--color-success-text)]">{{ summary.coMat }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-warning-border)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Đi muộn</div>
        <div class="text-2xl font-bold text-[var(--color-warning-text)]">{{ summary.diMuon }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-blue-500">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Có phép</div>
        <div class="text-2xl font-bold text-blue-600">{{ summary.coPhep }}</div>
      </GlassPanel>
      <GlassPanel padding="normal" class="text-center border-b-4 border-[var(--color-danger-border)]">
        <div class="text-xs text-[var(--text-muted)] uppercase mb-1">Vắng mặt</div>
        <div class="text-2xl font-bold text-[var(--color-danger-text)]">{{ summary.vang }}</div>
      </GlassPanel>
    </div>

    <h2 class="text-lg font-bold text-[var(--text-heading)] mb-4">Lịch sử điểm danh gần đây</h2>
    <GlassPanel padding="none" class="overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm text-left">
          <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs">
            <tr>
              <th class="px-4 py-3">Môn học</th>
              <th class="px-4 py-3">Ngày học</th>
              <th class="px-4 py-3">Giờ điểm danh</th>
              <th class="px-4 py-3">Trạng thái</th>
              <th class="px-4 py-3">Ghi chú</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr v-for="h in history" :key="h.maBuoiHoc" class="hover:bg-[var(--surface-hover)]">
              <td class="px-4 py-3 font-medium text-[var(--text-heading)]">{{ h.tenMon }}</td>
              <td class="px-4 py-3 text-[var(--text-body)]">{{ dayjs(h.ngayHoc).format('DD/MM/YYYY') }}</td>
              <td class="px-4 py-3 text-[var(--text-muted)]">{{ h.thoiGianDiemDanh ? dayjs(h.thoiGianDiemDanh).format('HH:mm') : '--' }}</td>
              <td class="px-4 py-3"><AttendanceStatusBadge :status="h.trangThai" /></td>
              <td class="px-4 py-3 text-[var(--text-muted)]">{{ h.ghiChu || '--' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </GlassPanel>
  </div>
</template>
