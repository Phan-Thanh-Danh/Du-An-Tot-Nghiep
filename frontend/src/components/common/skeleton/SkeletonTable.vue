<template>
  <div class="skeleton-table" aria-label="Đang tải bảng dữ liệu">
    <div v-for="row in rows" :key="row" class="skeleton-row">
      <SkeletonBlock v-for="col in columns" :key="col" height="0.875rem" />
    </div>
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'

defineProps({
  rows: { type: Number, default: 6 },
  columns: { type: Number, default: 5 },
})
</script>

<style scoped>
.skeleton-table {
  display: grid;
  overflow: hidden;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg, 0.75rem);
  background: var(--surface-card);
}

.skeleton-row {
  display: grid;
  grid-template-columns: repeat(v-bind(columns), minmax(0, 1fr));
  gap: 1rem;
  padding: 0.875rem 1rem;
  border-bottom: 1px solid var(--border-table, var(--border-card));
}

.skeleton-row:last-child {
  border-bottom: 0;
}
</style>
