<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
})
</script>

<template>
  <div class="flex flex-col min-h-full">
    <!-- Page header: title + actions slot -->
    <div
      v-if="title || $slots.actions"
      class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-6"
    >
      <div>
        <h1 class="text-xl font-bold text-slate-800 leading-tight">{{ title }}</h1>
        <p v-if="subtitle" class="text-sm text-slate-500 mt-0.5">{{ subtitle }}</p>
      </div>
      <div v-if="$slots.actions" class="flex items-center gap-2 flex-shrink-0">
        <slot name="actions" />
      </div>
    </div>

    <!-- Main content -->
    <slot />
  </div>
</template>
