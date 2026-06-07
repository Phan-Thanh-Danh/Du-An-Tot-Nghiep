<script setup>
/**
 * LmsButton.vue
 * Reusable compact academic button component
 * Variants: primary, secondary, ghost, subtle, danger, success
 * Sizes: sm, md, lg
 * States: default, loading, disabled
 */
import { computed } from 'vue'
import { LoaderCircle } from 'lucide-vue-next'

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (v) => ['primary', 'secondary', 'ghost', 'subtle', 'danger', 'success'].includes(v),
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
  subtle: 'lg-button-subtle',
  danger: 'lg-button-danger',
  success: 'lg-button-success',
}

const sizeClasses = {
  sm: 'lms-button-sm',
  md: 'lms-button-md',
  lg: 'lms-button-lg',
}

const buttonClass = computed(() => [
  'lg-btn lms-button',
  variantClasses[props.variant],
  sizeClasses[props.size],
  {
    'cursor-not-allowed opacity-60': props.disabled,
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
    <LoaderCircle v-if="loading" class="lms-button-icon animate-spin" />
    <slot />
  </component>
</template>

<style scoped>
.lms-button {
  min-width: max-content;
  font-weight: 650;
  line-height: 1;
}

.lms-button-sm {
  min-height: var(--control-height-sm);
  padding: 0 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.78125rem;
}

.lms-button-md {
  min-height: var(--control-height-md);
  padding: 0 0.875rem;
  border-radius: var(--radius-md);
  font-size: 0.84375rem;
}

.lms-button-lg {
  min-height: var(--control-height-lg);
  padding: 0 1rem;
  border-radius: var(--radius-lg);
  font-size: 0.875rem;
}

.lms-button-icon {
  width: 1rem;
  height: 1rem;
}

.lms-button-sm .lms-button-icon {
  width: 0.875rem;
  height: 0.875rem;
}
</style>
