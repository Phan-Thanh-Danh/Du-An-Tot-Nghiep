<template>
  <div class="list-skeleton space-y-3" aria-label="Đang tải danh sách">
    <div v-for="i in items" :key="i" class="flex items-center gap-3 p-3 rounded-xl border border-card bg-(--surface-card)">
      <!-- Avatar / Icon -->
      <SkeletonBlock width="2.5rem" height="2.5rem" borderRadius="9999px" class="shrink-0" />
      
      <!-- Content -->
      <div class="flex-1 space-y-2">
        <SkeletonBlock width="40%" height="1rem" />
        <SkeletonBlock width="60%" height="0.875rem" />
      </div>
    </div>
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'

defineProps({
  items: { type: Number, default: 3 }
})
</script>
