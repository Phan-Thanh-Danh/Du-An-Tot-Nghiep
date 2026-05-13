<script setup>
defineProps({
  variant: {
    type: String,
    default: 'default',
    validator: (value) => ['default', 'success', 'warning', 'danger', 'info'].includes(value),
  },
})

const classes = {
  default: 'border-slate-200/70 bg-white/65 text-slate-700',
  success: 'border-emerald-200/80 bg-emerald-50/80 text-emerald-700',
  warning: 'border-amber-200/80 bg-amber-50/85 text-amber-700',
  danger: 'border-red-200/80 bg-red-50/85 text-red-700',
  info: 'border-sky-200/80 bg-sky-50/85 text-sky-700',
}
</script>

<template>
  <span
    :class="[
      'inline-flex items-center gap-1 rounded-full border px-2.5 py-1 text-xs font-bold backdrop-blur-md',
      classes[variant],
    ]"
  >
    <slot />
  </span>
</template>
