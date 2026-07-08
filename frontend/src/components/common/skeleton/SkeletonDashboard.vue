<template>
  <div class="skeleton-dashboard" aria-label="Đang tải dashboard">
    <SkeletonCard v-for="card in cards" :key="card" />
    <SkeletonChart class="wide" />
    <SkeletonTable class="wide" :rows="rows" :columns="columns" />
  </div>
</template>

<script setup>
import SkeletonCard from './SkeletonCard.vue'
import SkeletonChart from './SkeletonChart.vue'
import SkeletonTable from './SkeletonTable.vue'

defineProps({
  cards: { type: Number, default: 4 },
  rows: { type: Number, default: 5 },
  columns: { type: Number, default: 5 },
})
</script>

<style scoped>
.skeleton-dashboard {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.wide {
  grid-column: span 2;
}

@media (max-width: 1024px) {
  .skeleton-dashboard {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 680px) {
  .skeleton-dashboard {
    grid-template-columns: 1fr;
  }

  .wide {
    grid-column: span 1;
  }
}
</style>
