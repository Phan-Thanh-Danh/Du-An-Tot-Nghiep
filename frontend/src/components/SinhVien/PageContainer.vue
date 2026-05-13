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
      class="lg-page-enter lg-glass-strong mb-6 flex flex-col gap-3 rounded-[28px] px-5 py-4 sm:flex-row sm:items-center sm:justify-between"
    >
      <div>
        <h1 class="text-xl font-bold leading-tight text-slate-950">{{ title }}</h1>
        <p v-if="subtitle" class="mt-1 text-sm text-slate-600">{{ subtitle }}</p>
      </div>
      <div v-if="$slots.actions" class="flex items-center gap-2 flex-shrink-0">
        <slot name="actions" />
      </div>
    </div>

    <!-- Main content -->
    <slot />
  </div>
</template>
