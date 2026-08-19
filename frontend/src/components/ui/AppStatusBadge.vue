<script setup>
import { computed } from 'vue';

const props = defineProps({
  status: {
    type: String,
    required: true,
    validator: (v) => ['success', 'warning', 'danger', 'info', 'neutral', 'primary', 'violet'].includes(v)
  },
  icon: {
    type: Boolean,
    default: false
  }
});

const badgeClasses = computed(() => {
  const base = 'lg-badge';
  const colorMap = {
    success: 'lg-badge-success',
    warning: 'lg-badge-warning',
    danger: 'lg-badge-danger',
    info: 'lg-badge-info',
    primary: 'lg-badge-primary',
    violet: 'lg-badge-violet',
    neutral: 'bg-slate-100 text-slate-600 border-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700'
  };
  
  return `${base} ${colorMap[props.status]}`;
});
</script>

<template>
  <span :class="badgeClasses">
    <slot name="icon" v-if="icon">
      <!-- Default dot icon if icon is requested but slot not provided -->
      <span class="w-1.5 h-1.5 rounded-full bg-current"></span>
    </slot>
    <slot></slot>
  </span>
</template>
