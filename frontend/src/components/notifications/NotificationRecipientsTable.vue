<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  recipients: { type: Array, default: () => [] },
  loading: Boolean
})
</script>

<template>
  <GlassPanel padding="none" class="overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-sm text-left">
        <thead class="bg-[var(--surface-hover)] text-[var(--text-muted)] uppercase text-xs">
          <tr>
            <th class="px-4 py-3">Người nhận</th>
            <th class="px-4 py-3">Mã NV/SV</th>
            <th class="px-4 py-3">Trạng thái</th>
            <th class="px-4 py-3">Thời gian đọc</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-[var(--border-card)]">
          <template v-if="loading">
            <tr v-for="i in 3" :key="i">
              <td class="px-4 py-3" colspan="4">
                <div class="h-4 bg-[var(--surface-hover)] rounded animate-pulse w-full"></div>
              </td>
            </tr>
          </template>
          <tr v-else-if="recipients.length === 0">
            <td class="px-4 py-6 text-center text-[var(--text-muted)]" colspan="4">
              Không có dữ liệu
            </td>
          </tr>
          <tr v-else v-for="r in recipients" :key="r.id" class="hover:bg-[var(--surface-hover)]">
            <td class="px-4 py-3">
              <div class="font-medium text-[var(--text-body)]">{{ r.hoTenNguoiNhan }}</div>
            </td>
            <td class="px-4 py-3">{{ r.maNguoiNhan }}</td>
            <td class="px-4 py-3">
              <span v-if="r.trangThaiThucThi === 'THANH_CONG'" class="text-[var(--color-success-text)] flex items-center gap-1">
                Thành công
              </span>
              <span v-else class="text-[var(--color-danger-text)]">
                Lỗi
              </span>
            </td>
            <td class="px-4 py-3 text-[var(--text-muted)]">
              {{ r.thoiGianDoc || 'Chưa đọc' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </GlassPanel>
</template>
