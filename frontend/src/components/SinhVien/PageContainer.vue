<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
})
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="sr-only">
      <h1 v-if="title">{{ title }}</h1>
      <p v-if="subtitle">{{ subtitle }}</p>
    </div>

    <div v-if="$slots.actions" class="mb-4 flex justify-end">
      <div class="lg-glass-strong flex items-center gap-1.5 rounded-full border border-white/60 px-2.5 py-1.5 shadow-sm">
        <slot name="actions" />
      </div>
    </div>

    <!-- Main content -->
    <div class="relative z-10">
      <slot />
    </div>
  </div>
</template>
