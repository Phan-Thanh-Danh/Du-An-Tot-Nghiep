<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import RewardStatusBadge from './RewardStatusBadge.vue'

defineProps({
  campaigns: { type: Array, default: () => [] }
})
const emit = defineEmits(['select'])
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Mã đợt</th>
            <th class="px-4 py-3">Tên đợt / Học kỳ</th>
            <th class="px-4 py-3">Loại khen thưởng</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th class="px-4 py-3 text-right">Tổng UV</th>
            <th class="px-4 py-3 text-center">Hành động</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-card)]">
          <tr v-if="campaigns.length === 0">
            <td colspan="6" class="px-4 py-8 text-center text-[var(--text-muted)]">Chưa có đợt khen thưởng nào.</td>
          </tr>
          <tr v-for="c in campaigns" :key="c.id" class="hover:bg-[var(--surface-hover)] transition-colors">
            <td class="px-4 py-3 font-medium text-[var(--text-heading)]">{{ c.maDot }}</td>
            <td class="px-4 py-3">
              <div class="text-[var(--text-heading)]">{{ c.tenDot }}</div>
              <div class="text-xs text-[var(--text-muted)]">{{ c.hocKy }}</div>
            </td>
            <td class="px-4 py-3 text-[var(--text-body)]">{{ c.loaiDot }}</td>
            <td class="px-4 py-3">
              <RewardStatusBadge :status="c.trangThai" />
            </td>
            <td class="px-4 py-3 text-right font-medium text-[var(--text-heading)]">{{ c.tongUngVien }}</td>
            <td class="px-4 py-3 text-center">
              <GlassButton variant="secondary" size="sm" @click="emit('select', c)">Chi tiết</GlassButton>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
