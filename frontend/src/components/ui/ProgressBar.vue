<script setup>
import { computed } from 'vue'

const props = defineProps({
  value: {
    type: Number,
    default: 0,
  },
  max: {
    type: Number,
    default: 100,
  },
  label: {
    type: String,
    default: '',
  },
  variant: {
    type: String,
    default: 'blue',
    validator: (value) => ['blue', 'cyan', 'teal', 'violet', 'amber', 'green', 'red'].includes(value),
  },
})

const percent = computed(() => {
  if (props.max <= 0) return 0
  return Math.min(100, Math.max(0, Math.round((props.value / props.max) * 100)))
})

const gradientClass = computed(
  () =>
    ({
      blue: 'from-blue-600 to-cyan-500',
      cyan: 'from-cyan-600 to-sky-400',
      teal: 'from-teal-600 to-emerald-400',
      violet: 'from-violet-600 to-indigo-500',
      amber: 'from-amber-500 to-orange-500',
      green: 'from-emerald-600 to-green-400',
      red: 'from-red-600 to-rose-500',
    })[props.variant],
)
</script>

<template>
  <div>
    <div v-if="label" class="mb-2 flex items-center justify-between gap-3 text-xs font-semibold text-slate-600">
      <span>{{ label }}</span>
      <span>{{ percent }}%</span>
    </div>
    <div
      class="lg-progress-track h-2.5 w-full"
      role="progressbar"
      :aria-label="label || 'Tiến độ'"
      :aria-valuenow="percent"
      aria-valuemin="0"
      aria-valuemax="100"
    >
      <div
        class="lg-progress-fill bg-gradient-to-r"
        :class="gradientClass"
        :style="{ width: `${percent}%` }"
      />
    </div>
  </div>
</template>
