<template>
  <div class="form-skeleton space-y-4" aria-label="Đang tải biểu mẫu">
    <div v-for="i in fields" :key="i" class="space-y-2">
      <!-- Label -->
      <SkeletonBlock width="25%" height="1rem" />
      <!-- Input -->
      <SkeletonBlock width="100%" height="2.5rem" borderRadius="var(--radius-lg, 0.75rem)" />
    </div>
  </div>
</template>

<script setup>
import SkeletonBlock from './SkeletonBlock.vue'

defineProps({
  fields: { type: Number, default: 4 }
})
</script>
