<script setup>
import { computed } from 'vue'
import { AlertTriangle, Clock3, UserCheck } from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'

const props = defineProps({
  attendance: {
    type: Object,
    default: () => ({}),
  },
})

const rate = computed(() => props.attendance?.rate ?? props.attendance?.score ?? 100)
const absent = computed(() => props.attendance?.absent ?? 0)
const late = computed(() => props.attendance?.late ?? 0)
const warningMsg = computed(() => props.attendance?.warning || props.attendance?.advice || 'Bạn đang duy trì tỷ lệ chuyên cần rất tốt, hãy phát huy!')
const attendanceDegrees = computed(() => Math.min(100, Math.max(0, rate.value)) * 3.6)
</script>

<template>
  <GlassPanel variant="strong" density="none" class="rounded-2xl lg-glass-card-hover h-full flex flex-col justify-between">
    <div class="p-4 lg:p-5 flex-1 flex flex-col justify-between">
      <div class="flex items-start justify-between gap-3">
        <div>
          <h2 class="text-base font-semibold text-heading">Chuyên cần & Cảnh báo</h2>
          <p class="text-xs font-medium text-body">Tình trạng học tập</p>
        </div>
        <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-(--color-success-bg) text-(--color-success-text) border border-[color-mix(in srgb,var(--color-success-text) 20%,transparent)] shadow-sm">
          <UserCheck :size="18" />
        </div>
      </div>

      <div class="mt-5 flex flex-col gap-5 sm:flex-row sm:items-center">
        <div
          class="relative flex h-24 w-24 flex-shrink-0 items-center justify-center rounded-full p-2 shadow-inner"
          :style="{ background: `conic-gradient(var(--color-success-text) 0deg, var(--accent-cyan) ${attendanceDegrees}deg, var(--border-default) ${attendanceDegrees}deg, var(--border-default) 360deg)` }"
        >
          <div class="flex h-full w-full items-center justify-center rounded-full bg-(--surface-card) backdrop-blur-md shadow-sm">
            <div class="text-center">
              <p class="text-2xl font-semibold tracking-tight text-heading leading-none">{{ rate }}%</p>
              <p class="mt-1 text-xs font-medium text-body">Tỉ lệ</p>
            </div>
          </div>
        </div>
        
        <div class="grid flex-1 grid-cols-2 gap-3">
          <div class="lg-readable rounded-xl p-3 shadow-sm border border-card">
            <p class="text-xs font-medium text-body">Vắng mặt</p>
            <p class="mt-1 text-xl font-semibold text-heading">{{ absent }} <span class="text-[11px] font-semibold text-placeholder">buổi</span></p>
          </div>
          <div class="lg-readable rounded-xl p-3 shadow-sm border border-card">
            <p class="flex items-center gap-1.5 text-xs font-medium text-body">
              <Clock3 :size="11" />
              Đi muộn
            </p>
            <p class="mt-1 text-xl font-semibold text-heading">{{ late }} <span class="text-[11px] font-semibold text-placeholder">buổi</span></p>
          </div>
        </div>
      </div>

      <div class="mt-5">
        <ProgressBar :value="rate" variant="green" class="h-2 shadow-inner" />
      </div>

      <div class="mt-4 flex gap-3 rounded-xl border border-[color-mix(in srgb,var(--color-warning-text) 20%,transparent)] bg-(--color-warning-bg) p-3 text-(--color-warning-text) shadow-sm backdrop-blur-md">
        <AlertTriangle :size="18" class="mt-0.5 flex-shrink-0 text-(--color-warning-text)" />
        <p class="text-[13px] font-semibold leading-6">{{ warningMsg }}</p>
      </div>
    </div>
  </GlassPanel>
</template>
