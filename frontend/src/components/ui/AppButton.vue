<script setup>
import { computed } from 'vue';

const props = defineProps({
  variant: {
    type: String,
    default: 'primary',
    validator: (value) => ['primary', 'secondary', 'danger', 'success', 'ghost', 'subtle'].includes(value),
  },
  size: {
    type: String,
    default: 'md',
    validator: (value) => ['sm', 'md', 'lg'].includes(value),
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  loading: {
    type: Boolean,
    default: false,
  },
  type: {
    type: String,
    default: 'button',
  },
});

const buttonClasses = computed(() => {
  const baseClasses = 'inline-flex items-center justify-center gap-2 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed font-medium';
  
  const sizeClasses = {
    sm: 'text-xs px-3 py-1.5 min-h-[32px] rounded-md',
    md: 'text-sm px-4 py-2 min-h-[40px] rounded-lg',
    lg: 'text-base px-5 py-2.5 min-h-[48px] rounded-xl',
  };

  // Maps to liquid-glass.css utilities
  const variantClasses = {
    primary: 'lg-btn-primary',
    secondary: 'lg-btn-secondary',
    danger: 'lg-btn-danger',
    success: 'lg-btn-success',
    ghost: 'lg-btn-ghost',
    subtle: 'lg-btn-subtle'
  };

  return [baseClasses, sizeClasses[props.size], variantClasses[props.variant]].join(' ');
});
</script>

<template>
  <button :type="type" :class="buttonClasses" :disabled="disabled || loading">
    <svg v-if="loading" class="animate-spin -ml-1 mr-2 h-4 w-4 text-current" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
    <slot></slot>
  </button>
</template>
