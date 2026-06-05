<script setup>
import { computed } from 'vue'

const props = defineProps({
  as: {
    type: String,
    default: 'section',
  },
  variant: {
    type: String,
    default: 'default',
    validator: (value) => ['default', 'strong', 'soft', 'solid', 'readable'].includes(value),
  },
  density: {
    type: String,
    default: 'normal',
    validator: (value) => ['none', 'compact', 'normal', 'spacious'].includes(value),
  },
  strong: Boolean,
  soft: Boolean,
  interactive: Boolean,
  glow: Boolean,
  floating: Boolean,
  padding: {
    type: String,
    default: '',
  },
  clip: {
    type: Boolean,
    default: true,
  },
})

const surfaceClass = computed(() => {
  if (props.glow) return 'lg-glass-card-hover lg-glow'
  if (props.floating) return 'lg-glass-card-hover lg-float-card'
  if (props.strong) return 'lg-glass-strong'
  if (props.soft) return 'lg-glass-soft'
  if (props.variant === 'strong') return 'lg-glass-strong'
  if (props.variant === 'soft') return 'lg-glass-soft'
  if (props.variant === 'solid') return 'lg-solid-soft'
  if (props.variant === 'readable') return 'lg-readable'
  return 'lg-glass'
})

const densityClass = computed(() => {
  if (props.padding) return props.padding

  return (
    {
      none: 'p-0',
      compact: 'lg-density-compact',
      normal: 'lg-density-normal',
      spacious: 'lg-density-spacious',
    }[props.density] || 'lg-density-normal'
  )
})
</script>

<template>
  <component
    :is="as"
    :class="[
      'lg-card relative',
      surfaceClass,
      densityClass,
      clip ? 'overflow-hidden' : 'overflow-visible',
      interactive ? 'lg-card-hover' : '',
    ]"
  >
    <div v-if="$slots.header" class="mb-4 border-b border-card pb-3">
      <slot name="header" />
    </div>

    <slot />

    <div v-if="$slots.footer" class="mt-4 border-t border-card pt-3">
      <slot name="footer" />
    </div>
  </component>
</template>
