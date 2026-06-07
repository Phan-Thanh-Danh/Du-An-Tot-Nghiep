<script setup>
defineProps({
  lines: {
    type: Number,
    default: 3,
  },
})
</script>

<template>
  <div class="space-y-2.5" aria-hidden="true">
    <div
      v-for="line in lines"
      :key="line"
      class="lg-skeleton h-3.5"
      :class="line === lines ? 'w-2/3' : 'w-full'"
    />
  </div>
</template>
