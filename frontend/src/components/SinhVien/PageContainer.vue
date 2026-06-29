<script setup>
defineProps({
  title: { type: String, default: '' },
  subtitle: { type: String, default: '' },
})
</script>

<template>
  <div class="flex min-h-full flex-col">
    <div class="mb-3 sm:sr-only">
      <h1 v-if="title" class="text-xl font-bold leading-tight text-heading">{{ title }}</h1>
      <p v-if="subtitle" class="mt-1 text-sm leading-5 text-body">{{ subtitle }}</p>
    </div>

    <div v-if="$slots.actions" class="mb-3 flex justify-end">
      <div class="surface-card border-card flex items-center gap-1.5 rounded-(--radius-lg) border px-2 py-1.5 shadow-sm">
        <slot name="actions" />
      </div>
    </div>

    <!-- Main content -->
    <div class="relative z-10">
      <slot />
    </div>
  </div>
</template>
