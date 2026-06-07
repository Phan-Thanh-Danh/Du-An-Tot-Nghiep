<script setup>
import { computed } from 'vue'
import { LoaderCircle } from 'lucide-vue-next'

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (value) =>
      ['primary', 'secondary', 'ghost', 'subtle', 'danger', 'success'].includes(value),
  },
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['sm', 'md', 'lg'].includes(value),
  },
  loading: Boolean,
  disabled: Boolean,
  block: Boolean,
  glow: Boolean,
  type: {
    type: String,
    default: 'button',
  },
})

const emit = defineEmits(['click'])

const variantClass = computed(() => ({
  primary: 'lg-button-primary font-semibold',
  secondary: 'lg-button-secondary font-semibold',
  ghost: 'lg-button-ghost font-semibold',
  subtle: 'lg-button-subtle font-semibold',
  danger: 'lg-button-danger font-semibold',
  success: 'lg-button-success font-semibold',
}[props.variant]))

const sizeClass = computed(() => ({
  sm: 'glass-button-sm',
  md: 'glass-button-md',
  lg: 'glass-button-lg',
}[props.size]))

const loaderSize = computed(() => (props.size === 'sm' ? 14 : 16))
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :aria-busy="loading ? 'true' : undefined"
    :class="[
      'items-center justify-center gap-2 transition-all duration-200 ease-out active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60',
      'glass-button',
      block ? 'flex w-full' : 'inline-flex',
      variantClass,
      sizeClass,
      glow ? 'lg-glow' : '',
    ]"
    @click="emit('click', $event)"
  >
    <LoaderCircle v-if="loading" :size="loaderSize" class="animate-spin" aria-hidden="true" />
    <slot name="leading" />
    <span>
      <slot />
    </span>
    <slot name="trailing" />
  </button>
</template>

<style scoped>
.glass-button {
  min-width: max-content;
  line-height: 1;
}

.glass-button-sm {
  min-height: var(--control-height-sm);
  padding: 0 0.75rem;
  border-radius: var(--radius-md);
  font-size: 0.78125rem;
}

.glass-button-md {
  min-height: var(--control-height-md);
  padding: 0 0.875rem;
  border-radius: var(--radius-md);
  font-size: 0.84375rem;
}

.glass-button-lg {
  min-height: var(--control-height-lg);
  padding: 0 1rem;
  border-radius: var(--radius-lg);
  font-size: 0.875rem;
}
</style>
