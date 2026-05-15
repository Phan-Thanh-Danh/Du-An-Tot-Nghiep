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
    validator: (value) => ['default', 'strong', 'soft', 'solid'].includes(value),
  },
  density: {
    type: String,
    default: 'normal',
    validator: (value) => ['compact', 'normal', 'spacious'].includes(value),
  },
  strong: Boolean,
  soft: Boolean,
  interactive: Boolean,
  padding: {
    type: String,
    default: '',
  },
})

const surfaceClass = computed(() => {
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
      'lg-card relative overflow-hidden',
      surfaceClass,
      densityClass,
      interactive ? 'lg-card-hover' : '',
    ]"
  >
    <div v-if="$slots.header" class="mb-5 border-b border-white/45 pb-4">
      <slot name="header" />
    </div>

    <slot />

    <div v-if="$slots.footer" class="mt-5 border-t border-white/45 pt-4">
      <slot name="footer" />
    </div>
  </component>
</template>
