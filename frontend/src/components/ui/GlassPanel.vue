<script setup>
import { computed } from 'vue'

const props = defineProps({
  as: {
    type: String,
    default: 'section',
  },
  strong: Boolean,
  soft: Boolean,
  interactive: Boolean,
  padding: {
    type: String,
    default: 'p-5 sm:p-6',
  },
})

const surfaceClass = computed(() => {
  if (props.strong) return 'lg-glass-strong'
  if (props.soft) return 'lg-glass-soft'
  return 'lg-glass'
})
</script>

<template>
  <component
    :is="as"
    :class="[
      'lg-card relative overflow-hidden',
      surfaceClass,
      padding,
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
