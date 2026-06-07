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
    validator: (value) =>
      ['default', 'strong', 'soft', 'flat', 'surface', 'solid', 'readable'].includes(value),
  },
  density: {
    type: String,
    default: 'default',
    validator: (value) =>
      ['none', 'compact', 'default', 'normal', 'comfortable', 'spacious'].includes(value),
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
  if (props.variant === 'flat' || props.variant === 'surface') return 'glass-panel-flat'
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
      default: 'lg-density-normal',
      normal: 'lg-density-normal',
      comfortable: 'lg-density-spacious',
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
    <div v-if="$slots.header" class="glass-panel-header">
      <slot name="header" />
    </div>

    <slot />

    <div v-if="$slots.footer" class="glass-panel-footer">
      <slot name="footer" />
    </div>
  </component>
</template>

<style scoped>
.glass-panel-flat {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  box-shadow: none;
}

.glass-panel-header {
  margin-bottom: 0.75rem;
  padding-bottom: 0.625rem;
  border-bottom: 1px solid var(--border-card);
}

.glass-panel-footer {
  margin-top: 0.75rem;
  padding-top: 0.625rem;
  border-top: 1px solid var(--border-card);
}
</style>
