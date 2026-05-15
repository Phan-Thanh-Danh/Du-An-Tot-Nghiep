<script setup>
import { computed } from 'vue'
import { LoaderCircle } from 'lucide-vue-next'

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (value) => ['primary', 'secondary', 'ghost', 'danger'].includes(value),
  },
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['sm', 'md', 'lg'].includes(value),
  },
  loading: Boolean,
  disabled: Boolean,
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
  danger: 'bg-red-600 text-white shadow-lg shadow-red-600/20 hover:bg-red-700 focus-visible:ring-4 focus-visible:ring-red-500/20',
}[props.variant]))

const sizeClass = computed(() => ({
  sm: 'min-h-9 px-3 py-1.5 text-xs',
  md: 'min-h-10 px-4 py-2.5 text-sm',
  lg: 'min-h-12 px-5 py-3 text-base',
}[props.size]))
</script>

<template>
  <button
    :type="type"
    :disabled="disabled || loading"
    :class="[
      'inline-flex items-center justify-center gap-2 rounded-[20px] transition-all duration-200 ease-out active:scale-[0.98] disabled:cursor-not-allowed disabled:opacity-60',
      variantClass,
      sizeClass,
    ]"
    @click="emit('click', $event)"
  >
    <LoaderCircle v-if="loading" :size="17" class="animate-spin" />
    <slot />
  </button>
</template>
