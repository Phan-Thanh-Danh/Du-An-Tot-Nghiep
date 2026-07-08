<template>
  <div class="skeleton-block" :style="{ width, height, borderRadius }" aria-hidden="true" />
</template>

<script setup>
defineProps({
  width: { type: String, default: '100%' },
  height: { type: String, default: '1rem' },
  borderRadius: { type: String, default: 'var(--radius-md, 0.5rem)' },
})
</script>

<style scoped>
.skeleton-block {
  display: block;
  overflow: hidden;
  background: linear-gradient(90deg, var(--surface-input), var(--surface-card-hover), var(--surface-input));
  background-size: 200% 100%;
  animation: skeleton-shimmer 1.35s ease-in-out infinite;
}

@keyframes skeleton-shimmer {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}
</style>
