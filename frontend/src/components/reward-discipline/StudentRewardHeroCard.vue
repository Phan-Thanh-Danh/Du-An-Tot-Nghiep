<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { Trophy, Star } from 'lucide-vue-next'

defineProps({
  rewardsCount: { type: Number, default: 0 },
  topRank: { type: String, default: '' }
})
</script>

<template>
  <GlassPanel class="bg-gradient-to-br from-amber-500/10 to-amber-600/5 border border-amber-500/20 p-6 flex flex-col md:flex-row gap-6 items-center">
    <div class="p-4 bg-amber-500/10 rounded-full">
      <Trophy class="w-16 h-16 text-amber-500" />
    </div>
    <div class="text-center md:text-left flex-1">
      <h2 class="text-2xl font-bold text-[var(--text-heading)]">Thành tích của bạn</h2>
      <p class="text-[var(--text-muted)] mt-1">Chúc mừng! Sự nỗ lực của bạn đã được đền đáp xứng đáng.</p>
      
      <div class="flex flex-wrap gap-4 mt-4 justify-center md:justify-start">
        <div class="px-4 py-2 bg-[var(--surface-card)] rounded-lg border border-[var(--border-card)]">
          <div class="text-xs text-[var(--text-muted)]">Tổng số khen thưởng</div>
          <div class="text-xl font-semibold text-[var(--text-heading)]">{{ rewardsCount }}</div>
        </div>
        <div v-if="topRank" class="px-4 py-2 bg-[var(--surface-card)] rounded-lg border border-[var(--border-card)] flex items-center gap-2">
          <div>
            <div class="text-xs text-[var(--text-muted)]">Danh hiệu cao nhất</div>
            <div class="text-xl font-semibold text-amber-600 flex items-center gap-1">
              <Star class="w-4 h-4" /> {{ topRank }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </GlassPanel>
</template>
