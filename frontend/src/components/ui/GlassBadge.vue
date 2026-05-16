<script setup>
defineProps({
  variant: {
    type: String,
    default: 'default',
    validator: (value) =>
      ['default', 'primary', 'success', 'warning', 'danger', 'info', 'violet'].includes(value),
  },
  size: {
    type: String,
    default: 'sm',
    validator: (value) => ['sm', 'md'].includes(value),
  },
})

const classes = {
  default: 'lg-badge',
  primary: 'lg-badge lg-badge-primary',
  success: 'lg-badge lg-badge-success',
  warning: 'lg-badge lg-badge-warning',
  danger: 'lg-badge lg-badge-danger',
  info: 'lg-badge lg-badge-info',
  violet: 'lg-badge lg-badge-violet',
}

const sizes = {
  sm: 'px-2.5 py-1 text-[11px]',
  md: 'px-3 py-1.5 text-xs',
}
</script>

<template>
  <span
    :class="[
      classes[variant],
      sizes[size],
    ]"
  >
    <slot />
  </span>
</template>
