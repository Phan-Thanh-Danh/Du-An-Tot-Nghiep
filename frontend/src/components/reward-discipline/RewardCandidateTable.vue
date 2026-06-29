<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import RewardStatusBadge from './RewardStatusBadge.vue'

defineProps({
  candidates: { type: Array, default: () => [] }
})
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Sinh viên</th>
            <th class="px-4 py-3">Lớp</th>
            <th class="px-4 py-3 text-right">ĐTB</th>
            <th class="px-4 py-3 text-right">ĐRL</th>
            <th class="px-4 py-3">Xếp hạng</th>
            <th class="px-4 py-3">Trạng thái</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-card)]">
          <tr v-if="candidates.length === 0">
            <td colspan="6" class="px-4 py-8 text-center text-[var(--text-muted)]">Không có ứng viên nào.</td>
          </tr>
          <tr v-for="c in candidates" :key="c.id" class="hover:bg-[var(--surface-hover)] transition-colors">
            <td class="px-4 py-3">
              <div class="font-medium text-[var(--text-heading)]">{{ c.hoTen }}</div>
              <div class="text-xs text-[var(--text-muted)]">{{ c.maSinhVien }}</div>
            </td>
            <td class="px-4 py-3 text-[var(--text-body)]">{{ c.lop }}</td>
            <td class="px-4 py-3 text-right text-[var(--text-body)]">{{ c.diemTrungBinh }}</td>
            <td class="px-4 py-3 text-right text-[var(--text-body)]">{{ c.diemRenLuyen }}</td>
            <td class="px-4 py-3 font-medium text-amber-600">{{ c.xepHang }}</td>
            <td class="px-4 py-3">
              <RewardStatusBadge :status="c.trangThai" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
