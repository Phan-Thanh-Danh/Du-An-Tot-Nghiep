<script setup>
/**
 * LmsButton.vue
 * Reusable Liquid Glass button component
 * Variants: primary, secondary, ghost
 * Sizes: sm, md, lg
 * States: default, loading, disabled
 */
import { computed } from 'vue'
import { LoaderCircle } from 'lucide-vue-next'

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'secondary', 'ghost'].includes(v),
  },
  size: {
    type: String,
    default: 'md',
    validator: (v) => ['sm', 'md', 'lg'].includes(v),
  },
  loading: Boolean,
  disabled: Boolean,
  type: {
    type: String,
    default: 'button',
  },
  asLink: Boolean,
  href: String,
  target: String,
})

const emit = defineEmits(['click'])

const variantClasses = {
  primary: 'lg-button-primary',
  secondary: 'lg-button-secondary',
  ghost: 'lg-btn-ghost',
}

const sizeClasses = {
  sm: 'px-3 py-1.5 text-xs',
  md: 'px-5 py-2.5 text-sm',
  lg: 'px-6 py-3 text-base',
}

const buttonClass = computed(() => [
  'lg-btn',
  variantClasses[props.variant],
  sizeClasses[props.size],
  {
    'opacity-60 cursor-not-allowed': props.disabled,
  },
])

const Component = computed(() => (props.asLink ? 'a' : 'button'))
</script>

<template>
  <component
    :is="Component"
    :class="buttonClass"
    :type="!asLink ? type : undefined"
    :href="asLink ? href : undefined"
    :target="asLink ? target : undefined"
    :disabled="disabled || loading"
    @click="emit('click')"
  >
    <LoaderCircle v-if="loading" :size="18" class="mr-2 animate-spin" />
    <slot />
  </component>
</template>

<style scoped>
/* Component-specific styles can go here if needed */
</style>
