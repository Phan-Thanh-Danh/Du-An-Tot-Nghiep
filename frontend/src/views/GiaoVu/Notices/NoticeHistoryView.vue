<script setup>
import { ref } from 'vue'
import { Search } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import TableShell from '@/components/ui/TableShell.vue'

const history = ref([
  { id: 'N-01', title: 'Thông báo nghỉ học môn Java', sentAt: '2026-06-29 08:30', recipientCount: 30, status: 'da_gui' },
  { id: 'N-02', title: 'Đổi phòng thi cuối kỳ', sentAt: '2026-06-28 14:15', recipientCount: 120, status: 'da_gui' },
])
</script>

<template>
  <div class="notice-history max-w-7xl mx-auto space-y-6">
    <GlassPanel variant="flat" density="compact">
      <h1 class="text-2xl font-bold text-(--text-heading)">Lịch sử thông báo giáo vụ</h1>
      <p class="text-(--text-body)">Xem lại các thông báo đã gửi cho sinh viên, giảng viên.</p>
    </GlassPanel>

    <GlassPanel variant="flat" class="p-0 overflow-hidden">
      <div class="p-4 border-b border-(--border-default)">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) max-w-md">
          <Search :size="16" />
          <input type="text" placeholder="Tìm theo tiêu đề..." class="bg-transparent border-none outline-none w-full" />
        </label>
      </div>

      <TableShell>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tiêu đề</th>
              <th>Thời gian gửi</th>
              <th>Số người nhận</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in history" :key="item.id">
              <td>{{ item.id }}</td>
              <td class="font-medium">{{ item.title }}</td>
              <td>{{ item.sentAt }}</td>
              <td>{{ item.recipientCount }}</td>
              <td><GlassBadge variant="success" size="sm">Đã gửi</GlassBadge></td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </GlassPanel>
  </div>
</template>
