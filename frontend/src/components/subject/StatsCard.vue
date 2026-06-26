<script setup lang="ts">
import type { FunctionalComponent, HTMLAttributes, VNodeProps } from 'vue'

defineProps<{
  label: string
  value: number
  icon: FunctionalComponent<HTMLAttributes & VNodeProps, {}>
  variant?: 'default' | 'success' | 'warning' | 'danger'
  index?: number
}>()

const variantStyles: Record<string, { bg: string; iconBg: string }> = {
  default: { bg: 'bg-blue-50 dark:bg-blue-500/10', iconBg: 'text-blue-600 dark:text-blue-400' },
  success: { bg: 'bg-emerald-50 dark:bg-emerald-500/10', iconBg: 'text-emerald-600 dark:text-emerald-400' },
  warning: { bg: 'bg-amber-50 dark:bg-amber-500/10', iconBg: 'text-amber-600 dark:text-amber-400' },
  danger: { bg: 'bg-red-50 dark:bg-red-500/10', iconBg: 'text-red-600 dark:text-red-400' },
}
</script>

<template>
  <div
    class="relative overflow-hidden rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 p-5 transition-all duration-200 hover:shadow-md"
    :style="{ animationDelay: (index ?? 0) * 80 + 'ms' }"
  >
    <div class="flex items-start justify-between">
      <div class="space-y-1.5">
        <p class="text-[13px] font-medium text-slate-500 dark:text-slate-400">
          {{ label }}
        </p>
        <p class="text-3xl font-bold tracking-tight text-slate-900 dark:text-white tabular-nums">
          {{ value }}
        </p>
      </div>
      <div
        :class="[
          'flex h-11 w-11 items-center justify-center rounded-xl transition-colors duration-200',
          variantStyles[variant || 'default'].bg,
          variantStyles[variant || 'default'].iconBg,
        ]"
      >
        <component :is="icon" :size="20" aria-hidden="true" />
      </div>
    </div>
  </div>
</template>
