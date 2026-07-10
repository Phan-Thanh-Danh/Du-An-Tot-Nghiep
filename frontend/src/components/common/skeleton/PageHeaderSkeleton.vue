<template>
  <div class="page-header-skeleton mb-6 flex flex-col md:flex-row gap-4 items-start md:items-center justify-between" aria-label="Đang tải tiêu đề">
    <div class="space-y-2">
      <!-- Title -->
      <SkeletonBlock width="16rem" height="1.75rem" />
      <!-- Subtitle -->
      <SkeletonBlock width="10rem" height="1rem" />
    </div>
    <div class="flex items-center gap-3 w-full md:w-auto">
      <!-- Actions/Buttons -->
      <SkeletonBlock v-for="i in actions" :key="i" width="6rem" height="2.5rem" borderRadius="var(--radius-lg, 0.75rem)" class="flex-1 md:flex-none" />
    </div>
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'

defineProps({
  actions: { type: Number, default: 2 }
})
</script>
