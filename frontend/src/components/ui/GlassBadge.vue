<script setup>
defineProps({
  as: {
    type: String,
    default: 'span',
  },
  variant: {
    type: String,
    default: 'neutral',
    validator: (value) =>
      ['default', 'neutral', 'primary', 'success', 'warning', 'danger', 'info', 'violet'].includes(
        value,
      ),
  },
  size: {
    type: String,
    default: 'sm',
    validator: (value) => ['sm', 'md'].includes(value),
  },
})

const classes = {
  default: 'lg-badge',
  neutral: 'lg-badge',
  primary: 'lg-badge lg-badge-primary',
  success: 'lg-badge lg-badge-success',
  warning: 'lg-badge lg-badge-warning',
  danger: 'lg-badge lg-badge-danger',
  info: 'lg-badge lg-badge-info',
  violet: 'lg-badge lg-badge-violet',
}

const sizes = {
  sm: 'glass-badge-sm',
  md: 'glass-badge-md',
}
</script>

<template>
  <component
    :is="as"
    :class="[
      classes[variant],
      sizes[size],
    ]"
  >
    <slot />
  </component>
</template>

<style scoped>
.glass-badge-sm {
  min-height: 1.375rem;
  padding: 0 0.5rem;
  font-size: 0.6875rem;
}

.glass-badge-md {
  min-height: 1.5rem;
  padding: 0 0.625rem;
  font-size: 0.75rem;
}
</style>
