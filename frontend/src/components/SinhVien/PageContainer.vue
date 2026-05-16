<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
})
</script>

<template>
  <div class="flex flex-col min-h-full">
    <div class="mb-4 flex items-center justify-between">
      <div class="sr-only">
        <h1 v-if="title">{{ title }}</h1>
        <p v-if="subtitle">{{ subtitle }}</p>
      </div>

      <div v-if="$slots.actions" class="ml-auto">
        <div class="lg-glass-strong flex items-center gap-1.5 rounded-full px-2.5 py-1.5 shadow-sm border border-white/60">
          <slot name="actions" />
        </div>
      </div>
    </div>

    <!-- Main content -->
    <div class="relative z-10">
      <slot />
    </div>
  </div>
</template>
