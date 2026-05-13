<script setup>
defineProps({
  lines: {
    type: Number,
    default: 3,
  },
})
</script>

<template>
  <div class="space-y-3" aria-hidden="true">
    <div
      v-for="line in lines"
      :key="line"
      class="lg-skeleton h-4"
      :class="line === lines ? 'w-2/3' : 'w-full'"
    />
  </div>
</template>
