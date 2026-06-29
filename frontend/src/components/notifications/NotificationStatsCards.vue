<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { Send, Eye, Users } from 'lucide-vue-next'

defineProps({
  stats: {
    type: Object,
    required: true
  },
  loading: Boolean
})
</script>

<template>
  <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
    <GlassPanel padding="compact" class="flex items-center gap-4">
      <div class="w-12 h-12 rounded-full bg-[var(--lg-primary)]/10 flex items-center justify-center">
        <Users class="w-6 h-6 text-[var(--lg-primary)]" />
      </div>
      <div>
        <div class="text-sm text-[var(--text-muted)]">Tổng người nhận</div>
        <div v-if="loading" class="h-7 w-16 bg-[var(--surface-hover)] rounded animate-pulse mt-1"></div>
        <div v-else class="text-2xl font-bold text-[var(--text-heading)]">{{ stats.tongNguoiNhan || 0 }}</div>
      </div>
    </GlassPanel>

    <GlassPanel padding="compact" class="flex items-center gap-4">
      <div class="w-12 h-12 rounded-full bg-[var(--color-success-bg)] flex items-center justify-center">
        <Send class="w-6 h-6 text-[var(--color-success-text)]" />
      </div>
      <div>
        <div class="text-sm text-[var(--text-muted)]">Đã gửi thành công</div>
        <div v-if="loading" class="h-7 w-16 bg-[var(--surface-hover)] rounded animate-pulse mt-1"></div>
        <div v-else class="text-2xl font-bold text-[var(--text-heading)]">{{ stats.daGuiThanhCong || 0 }}</div>
      </div>
    </GlassPanel>

    <GlassPanel padding="compact" class="flex items-center gap-4">
      <div class="w-12 h-12 rounded-full bg-[var(--color-info-bg)] flex items-center justify-center">
        <Eye class="w-6 h-6 text-[var(--color-info-text)]" />
      </div>
      <div>
        <div class="text-sm text-[var(--text-muted)]">Đã đọc</div>
        <div v-if="loading" class="h-7 w-16 bg-[var(--surface-hover)] rounded animate-pulse mt-1"></div>
        <div v-else class="text-2xl font-bold text-[var(--text-heading)]">
          {{ stats.daDoc || 0 }}
          <span class="text-sm font-normal text-[var(--text-muted)] ml-1" v-if="stats.tongNguoiNhan">
            ({{ Math.round((stats.daDoc / stats.tongNguoiNhan) * 100) }}%)
          </span>
        </div>
      </div>
    </GlassPanel>
  </div>
</template>
