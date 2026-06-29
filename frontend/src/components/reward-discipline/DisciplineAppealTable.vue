<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import DisciplineStatusBadge from './DisciplineStatusBadge.vue'
import dayjs from 'dayjs'

defineProps({
  appeals: { type: Array, default: () => [] }
})
const emit = defineEmits(['select'])
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Sinh viên</th>
            <th class="px-4 py-3">Mã hồ sơ KL</th>
            <th class="px-4 py-3">Lý do khiếu nại</th>
            <th class="px-4 py-3">Ngày gửi</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th class="px-4 py-3 text-center">Hành động</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-card)]">
          <tr v-if="appeals.length === 0">
            <td colspan="6" class="px-4 py-8 text-center text-[var(--text-muted)]">Chưa có khiếu nại nào.</td>
          </tr>
          <tr v-for="a in appeals" :key="a.id" class="hover:bg-[var(--surface-hover)] transition-colors">
            <td class="px-4 py-3">
              <div class="font-medium text-[var(--text-heading)]">{{ a.hoTen }}</div>
              <div class="text-xs text-[var(--text-muted)]">{{ a.maSinhVien }}</div>
            </td>
            <td class="px-4 py-3 text-[var(--text-body)]">{{ a.maHoSo }}</td>
            <td class="px-4 py-3 text-[var(--text-body)] line-clamp-1">{{ a.lyDo }}</td>
            <td class="px-4 py-3 text-[var(--text-muted)]">{{ dayjs(a.ngayGui).format('DD/MM/YYYY') }}</td>
            <td class="px-4 py-3">
              <DisciplineStatusBadge :status="a.trangThai" />
            </td>
            <td class="px-4 py-3 text-center">
              <GlassButton variant="secondary" size="sm" @click="emit('select', a)">Chi tiết</GlassButton>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
