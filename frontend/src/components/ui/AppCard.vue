<script setup>
import { computed } from 'vue';

const props = defineProps({
  variant: {
    type: String,
    default: 'glass',
    validator: (v) => ['glass', 'solid', 'soft', 'strong'].includes(v)
  },
  density: {
    type: String,
    default: 'normal',
    validator: (v) => ['compact', 'normal', 'spacious'].includes(v)
  },
  interactive: {
    type: Boolean,
    default: false
  }
});

const cardClasses = computed(() => {
  const variantClasses = {
    glass: 'lg-glass lg-card',
    solid: 'lg-solid lg-card',
    soft: 'lg-glass-soft lg-card',
    strong: 'lg-glass-strong lg-card'
  };
  
  const densityClasses = {
    compact: 'lg-density-compact',
    normal: 'lg-density-normal',
    spacious: 'lg-density-spacious'
  };

  return [
    variantClasses[props.variant],
    densityClasses[props.density],
    props.interactive ? 'lg-card-hover cursor-pointer' : ''
  ].join(' ');
});
</script>

<template>
  <div :class="cardClasses">
    <div v-if="$slots.header" class="mb-4 border-b border-slate-200/50 pb-3 dark:border-slate-700/50">
      <slot name="header"></slot>
    </div>
    <div class="flex-1">
      <slot></slot>
    </div>
    <div v-if="$slots.footer" class="mt-4 pt-3 border-t border-slate-200/50 dark:border-slate-700/50">
      <slot name="footer"></slot>
    </div>
  </div>
</template>
