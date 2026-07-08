<template>
  <div class="skeleton-chart" aria-label="Đang tải biểu đồ">
    <SkeletonBlock width="34%" height="1rem" />
    <div class="chart-bars">
      <SkeletonBlock v-for="bar in bars" :key="bar" :height="`${40 + bar * 12}px`" border-radius="999px 999px 0 0" />
    </div>
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'

defineProps({
  bars: { type: Number, default: 8 },
})
</script>

<style scoped>
.skeleton-chart {
  display: grid;
  gap: 1rem;
  min-height: 16rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg, 0.75rem);
  background: var(--surface-card);
  padding: 1rem;
}

.chart-bars {
  display: grid;
  grid-template-columns: repeat(v-bind(bars), minmax(0, 1fr));
  align-items: end;
  gap: 0.75rem;
  min-height: 12rem;
}
</style>
