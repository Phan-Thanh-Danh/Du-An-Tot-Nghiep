<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import RewardStatusBadge from './RewardStatusBadge.vue'
import { Medal, Calendar, Award } from 'lucide-vue-next'
import dayjs from 'dayjs'

defineProps({
  reward: { type: Object, required: true }
})
</script>

<template>
  <GlassPanel padding="normal" class="h-full flex flex-col hover:border-amber-500/30 transition-colors">
    <div class="flex justify-between items-start mb-3">
      <div class="p-2 bg-amber-500/10 rounded-lg text-amber-600">
        <Medal class="w-5 h-5" />
      </div>
      <RewardStatusBadge :status="reward.trangThai" />
    </div>
    
    <h3 class="font-semibold text-(--text-heading) mb-1 line-clamp-2">{{ reward.tieuDe }}</h3>
    <p class="text-xs text-(--text-muted) mb-3 flex items-center gap-1">
      <Award class="w-3 h-3" /> {{ reward.loaiKhenThuong }}
    </p>

    <div class="mt-auto space-y-2 text-sm text-(--text-body)">
      <div class="flex justify-between">
        <span class="text-(--text-muted)">Học kỳ:</span>
        <span class="font-medium">{{ reward.hocKy }}</span>
      </div>
      <div class="flex justify-between">
        <span class="text-(--text-muted)">Xếp hạng:</span>
        <span class="font-medium text-amber-600">{{ reward.xepHang || 'N/A' }}</span>
      </div>
      <div class="flex justify-between" v-if="reward.ngayCap">
        <span class="text-(--text-muted)">Ngày cấp:</span>
        <span class="font-medium flex items-center gap-1">
          <Calendar class="w-3 h-3" /> {{ dayjs(reward.ngayCap).format('DD/MM/YYYY') }}
        </span>
      </div>
    </div>

    <div class="mt-4 pt-4 border-t border-(--border-card) grid grid-cols-2 gap-2">
      <GlassButton variant="secondary" size="sm" class="w-full">Chi tiết</GlassButton>
      <GlassButton v-if="reward.certificateStatus === 'generated'" variant="primary" size="sm" class="w-full">Xem bằng khen</GlassButton>
    </div>
  </GlassPanel>
</template>
