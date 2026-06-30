<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ApplicationStatusBadge from './ApplicationStatusBadge.vue'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'
dayjs.extend(relativeTime)

defineProps({
  applications: { type: Array, default: () => [] },
  loading: Boolean
})
const emit = defineEmits(['select'])
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-(--surface-hover) text-(--text-muted) uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Mã đơn</th>
            <th class="px-4 py-3">Sinh viên</th>
            <th class="px-4 py-3">Loại đơn / Tiêu đề</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th class="px-4 py-3">Người xử lý</th>
            <th class="px-4 py-3 text-right">Ngày tạo</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-(--border-card)">
          <template v-if="loading">
          <tr v-for="i in 5" :key="i">
            <td class="px-4 py-4" colspan="6">
              <div class="h-4 bg-(--surface-hover) rounded animate-pulse w-full"></div>
            </td>
          </tr>
          </template>
          <tr v-else-if="applications.length === 0">
            <td class="px-4 py-8 text-center text-(--text-muted)" colspan="6">
              Không có đơn từ nào trong hàng đợi.
            </td>
          </tr>
          <tr v-else v-for="app in applications" :key="app.id" class="hover:bg-(--surface-hover) cursor-pointer transition-colors" @click="emit('select', app)">
            <td class="px-4 py-3 font-medium text-(--text-heading)">{{ app.maDon }}</td>
            <td class="px-4 py-3">
              <div class="text-(--text-body)">{{ app.tenNguoiNop || 'N/A' }}</div>
              <div class="text-xs text-(--text-muted)">{{ app.maNguoiNop }}</div>
            </td>
            <td class="px-4 py-3">
              <div class="font-medium text-(--text-body) line-clamp-1">{{ app.tieuDe }}</div>
              <div class="text-xs text-(--text-muted) line-clamp-1">{{ app.tenLoaiDon }}</div>
            </td>
            <td class="px-4 py-3">
              <ApplicationStatusBadge :status="app.trangThai" />
            </td>
            <td class="px-4 py-3">
              <span v-if="app.tenNguoiXuLy" class="text-(--text-body)">{{ app.tenNguoiXuLy }}</span>
              <span v-else class="text-(--text-muted) italic">Chưa phân công</span>
            </td>
            <td class="px-4 py-3 text-right text-(--text-muted) whitespace-nowrap">
              {{ dayjs(app.ngayTao).fromNow() }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
