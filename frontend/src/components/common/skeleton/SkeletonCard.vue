<template>
  <div class="skeleton-card" aria-label="Đang tải dữ liệu">
    <SkeletonBlock width="42%" height="0.875rem" />
    <SkeletonBlock width="70%" height="1.5rem" />
    <SkeletonBlock width="100%" height="0.75rem" />
    <SkeletonBlock width="86%" height="0.75rem" />
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'
</script>

<style scoped>
.skeleton-card {
  display: grid;
  gap: 0.75rem;
  min-height: 8rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg, 0.75rem);
  background: var(--surface-card);
  padding: 1rem;
}
</style>
