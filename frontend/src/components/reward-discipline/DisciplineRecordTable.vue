<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import DisciplineStatusBadge from './DisciplineStatusBadge.vue'
import dayjs from 'dayjs'

defineProps({
  records: { type: Array, default: () => [] }
})
const emit = defineEmits(['select'])
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-(--surface-hover) text-(--text-muted) uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Hồ sơ</th>
            <th class="px-4 py-3">Sinh viên</th>
            <th class="px-4 py-3">Vi phạm</th>
            <th class="px-4 py-3">Mức độ / Hình thức</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th class="px-4 py-3 text-right">Ngày VP</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-(--border-card)">
          <tr v-if="records.length === 0">
            <td colspan="6" class="px-4 py-8 text-center text-(--text-muted)">Chưa có hồ sơ kỷ luật nào.</td>
          </tr>
          <tr v-for="r in records" :key="r.id" class="hover:bg-(--surface-hover) transition-colors cursor-pointer" @click="emit('select', r)">
            <td class="px-4 py-3 font-medium text-(--text-heading)">{{ r.maHoSo }}</td>
            <td class="px-4 py-3">
              <div class="text-(--text-heading)">{{ r.hoTen }}</div>
              <div class="text-xs text-(--text-muted)">{{ r.maSinhVien }} ({{ r.lop }})</div>
            </td>
            <td class="px-4 py-3 text-(--text-body) line-clamp-2" :title="r.tieuDe">{{ r.tieuDe }}</td>
            <td class="px-4 py-3">
              <div class="font-medium text-(--color-danger-text)">{{ r.mucDoKyLuat }}</div>
              <div class="text-xs text-(--text-muted)">{{ r.hinhThucXuLy }}</div>
            </td>
            <td class="px-4 py-3">
              <DisciplineStatusBadge :status="r.trangThai" />
            </td>
            <td class="px-4 py-3 text-right text-(--text-muted)">
              {{ dayjs(r.ngayViPham).format('DD/MM/YYYY') }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
