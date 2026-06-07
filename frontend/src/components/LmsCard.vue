<script setup>
/**
 * LmsCard.vue
 * Reusable compact academic card component
 * Variants: glass (default), solid, glass-dark, glass-soft, flat
 */
import { computed } from 'vue'

const props = defineProps({
  variant: {
    type: String,
    default: 'glass',
    validator: (v) => ['glass', 'solid', 'glass-dark', 'glass-soft', 'flat'].includes(v),
  },
  interactive: Boolean,
  density: {
    type: String,
    default: 'default',
    validator: (v) => ['compact', 'default', 'comfortable'].includes(v),
  },
  padding: {
    type: String,
    default: '',
  },
})

const variantClasses = {
  glass: 'lg-glass',
  solid: 'lg-solid-soft',
  'glass-dark': 'lg-glass-dark',
  'glass-soft': 'lg-glass-soft',
  flat: 'lms-card-flat',
}

const densityPadding = {
  compact: 'var(--card-padding-sm)',
  default: 'var(--card-padding-md)',
  comfortable: 'var(--card-padding-lg)',
}

const cardPadding = computed(() => props.padding || densityPadding[props.density])
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
    :style="{ padding: cardPadding }"
  >
    <slot />
  </div>
</template>

<style scoped>
.lms-card-flat {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  box-shadow: none;
}
</style>
