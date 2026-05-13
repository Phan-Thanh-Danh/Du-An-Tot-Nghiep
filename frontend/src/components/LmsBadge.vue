<script setup>
/**
 * LmsBadge.vue
 * Reusable Liquid Glass badge component
 * Variants: primary, success, warning, error, info
 */
defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'success', 'warning', 'error', 'info'].includes(v),
  },
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v),
  },
})

const variantClasses = {
  primary: 'lg-badge-primary',
  success: 'lg-badge-success',
  warning: 'lg-badge-warning',
  error: 'lg-badge-error',
  info: 'lg-badge-info',
}

const sizeClasses = {
  sm: 'px-2 py-1 text-xs',
  md: 'px-3 py-1.5 text-sm',
  lg: 'px-4 py-2 text-base',
}
</script>

<template>
  <span :class="['lg-badge', variantClasses[variant], sizeClasses[size]]">
    <slot />
  </span>
</template>

<style scoped>
/* Component-specific styles can go here if needed */
</style>
