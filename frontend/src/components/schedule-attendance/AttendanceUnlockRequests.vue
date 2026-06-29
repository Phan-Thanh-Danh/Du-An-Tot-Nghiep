<script setup>
import { ref, onMounted } from 'vue'
import { scheduleAttendanceMockService } from '@/mocks/scheduleAttendanceMockService'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ScheduleMockBanner from './ScheduleMockBanner.vue'
import dayjs from 'dayjs'

const requests = ref([])
const loading = ref(false)

const loadData = async () => {
  loading.value = true
  try {
    const res = await scheduleAttendanceMockService.getAttendanceUnlockRequests()
    requests.value = res.items || []
  } finally {
    loading.value = false
  }
}
onMounted(() => loadData())

const processRequest = async (id, approve) => {
  if(approve) await scheduleAttendanceMockService.approveUnlockRequest(id)
  else await scheduleAttendanceMockService.rejectUnlockRequest(id) // stub
  loadData()
}
</script>

<template>
  <div class="p-6 h-full bg-[var(--surface-page)] overflow-y-auto custom-scrollbar flex flex-col">
    <ScheduleMockBanner />
    <h1 class="text-2xl font-bold text-[var(--text-heading)] mb-6">Yêu cầu mở khóa điểm danh</h1>
    
    <GlassPanel padding="none" class="flex-1 overflow-hidden flex flex-col">
      <div v-if="loading" class="p-6 animate-pulse bg-[var(--surface-card)] h-64"></div>
      <div v-else-if="requests.length === 0" class="p-12 text-center text-[var(--text-muted)]">
        Không có yêu cầu nào.
      </div>
      <div v-else class="overflow-y-auto custom-scrollbar flex-1">
        <table class="w-full text-sm text-left">
          <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs sticky top-0 z-10">
            <tr>
              <th class="px-4 py-3">Mã YC</th>
              <th class="px-4 py-3">Giáo viên</th>
              <th class="px-4 py-3">Mã buổi</th>
              <th class="px-4 py-3">Lý do</th>
              <th class="px-4 py-3">Ngày gửi</th>
              <th class="px-4 py-3">Trạng thái</th>
              <th class="px-4 py-3 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr v-for="r in requests" :key="r.id" class="hover:bg-[var(--surface-hover)]">
              <td class="px-4 py-3 font-medium">{{ r.maYeuCau }}</td>
              <td class="px-4 py-3">{{ r.giaoVien }}</td>
              <td class="px-4 py-3">{{ r.maBuoiHoc }}</td>
              <td class="px-4 py-3 truncate max-w-xs" :title="r.lyDo">{{ r.lyDo }}</td>
              <td class="px-4 py-3">{{ dayjs(r.ngayGui).format('DD/MM/YYYY HH:mm') }}</td>
              <td class="px-4 py-3">
                <GlassBadge v-if="r.trangThai==='cho_duyet'" variant="warning">Chờ duyệt</GlassBadge>
                <GlassBadge v-else-if="r.trangThai==='da_duyet'" variant="success">Đã duyệt</GlassBadge>
                <GlassBadge v-else variant="danger">Từ chối</GlassBadge>
              </td>
              <td class="px-4 py-3 text-right">
                <div class="flex justify-end gap-2" v-if="r.trangThai==='cho_duyet'">
                  <GlassButton variant="success" size="xs" @click="processRequest(r.id, true)">Duyệt</GlassButton>
                  <GlassButton variant="danger" size="xs" @click="processRequest(r.id, false)">Từ chối</GlassButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </GlassPanel>
  </div>
</template>
