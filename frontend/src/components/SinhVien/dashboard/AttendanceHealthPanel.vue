<script setup>
import { computed } from 'vue'
import { AlertTriangle, Clock3, UserCheck } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'

const props = defineProps({
  attendance: {
    type: Object,
    required: true,
  },
})

const attendanceDegrees = computed(() =>
  Math.min(100, Math.max(0, props.attendance.rate)) * 3.6
)
</script>

<template>
  <GlassPanel density="none" class="rounded-[28px] lg-glass-card-hover">
    <div class="p-4 lg:p-5">
    <div class="flex items-start justify-between gap-3">
      <div>
        <h2 class="text-base font-bold text-slate-950">Chuyên cần & Cảnh báo</h2>
        <p class="text-xs font-medium text-slate-500">Tình trạng học tập</p>
      </div>
      <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-emerald-50 text-emerald-700 border border-emerald-100 shadow-sm">
        <UserCheck :size="18" />
      </div>
    </div>

    <div class="mt-5 flex flex-col gap-5 sm:flex-row sm:items-center">
      <div
        class="relative flex h-24 w-24 flex-shrink-0 items-center justify-center rounded-full p-2 shadow-inner"
        :style="{ background: `conic-gradient(#10b981 0deg, #06b6d4 ${attendanceDegrees}deg, #f1f5f9 ${attendanceDegrees}deg, #f1f5f9 360deg)` }"
      >
        <div class="flex h-full w-full items-center justify-center rounded-full bg-white/90 backdrop-blur-md shadow-sm">
          <div class="text-center">
            <p class="text-2xl font-bold tracking-tight text-slate-950 leading-none">{{ attendance.rate }}%</p>
            <p class="mt-1 text-xs font-medium text-slate-500">Tỉ lệ</p>
          </div>
        </div>
      </div>
      
      <div class="grid flex-1 grid-cols-2 gap-3">
        <div class="lg-readable rounded-[20px] p-3 shadow-sm border border-white/60">
          <p class="text-xs font-medium text-slate-500">Vắng mặt</p>
          <p class="mt-1 text-xl font-bold text-slate-950">{{ attendance.absent }} <span class="text-[11px] font-bold text-slate-400">buổi</span></p>
        </div>
        <div class="lg-readable rounded-[20px] p-3 shadow-sm border border-white/60">
          <p class="flex items-center gap-1.5 text-xs font-medium text-slate-500">
            <Clock3 :size="11" />
            Đi muộn
          </p>
          <p class="mt-1 text-xl font-bold text-slate-950">{{ attendance.late }} <span class="text-[11px] font-bold text-slate-400">buổi</span></p>
        </div>
      </div>
    </div>

    <div class="mt-5">
      <ProgressBar :value="attendance.rate" variant="green" class="h-2 shadow-inner" />
    </div>

    <div class="mt-4 flex gap-3 rounded-[20px] border border-amber-200/50 bg-amber-50/70 p-3 text-amber-900 shadow-sm backdrop-blur-md">
      <AlertTriangle :size="18" class="mt-0.5 flex-shrink-0 text-amber-600" />
      <p class="text-[13px] font-semibold leading-6">{{ attendance.warning }}</p>
    </div>
    </div>
  </GlassPanel>
</template>
