<script setup>
/**
 * LmsCard.vue
 * Reusable Liquid Glass card component
 * Variants: glass (default), solid, glass-dark, glass-soft
 * Props: interactive, hover
 */

defineProps({
  variant: {
    type: String,
    default: 'glass',
    validator: (v) => ['glass', 'solid', 'glass-dark', 'glass-soft'].includes(v),
  },
  interactive: Boolean,
  padding: {
    type: String,
    default: '1.5rem',
  },
})

const variantClasses = {
  glass: 'lg-glass',
  solid: 'lg-solid-soft',
  'glass-dark': 'lg-glass-dark',
  'glass-soft': 'lg-glass-soft',
}
</script>

<template>
  <div
    :class="[
      'lg-card',
      variantClasses[variant],
      {
        'lg-card-hover': interactive,
        'cursor-pointer transition-all duration-200': interactive,
      },
    ]"
    :style="{ padding }"
  >
    <slot />
  </div>
</template>

<style scoped>
/* Component-specific styles can go here if needed */
</style>
