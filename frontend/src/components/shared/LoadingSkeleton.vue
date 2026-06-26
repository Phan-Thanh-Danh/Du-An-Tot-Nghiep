<script setup lang="ts">
defineProps<{
  variant?: 'stats' | 'toolbar' | 'card' | 'grid'
  count?: number
}>()
</script>

<template>
  <div v-if="variant === 'stats'" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4" aria-hidden="true">
    <div v-for="i in 4" :key="'stat-' + i" class="h-24 rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 overflow-hidden">
      <div class="p-5 space-y-3">
        <div class="h-3 w-24 rounded-full bg-slate-100 dark:bg-slate-700 animate-pulse" />
        <div class="h-8 w-16 rounded-lg bg-slate-100 dark:bg-slate-700 animate-pulse" />
      </div>
    </div>
  </div>

  <div v-else-if="variant === 'toolbar'" class="h-10 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 animate-pulse" aria-hidden="true" />

  <div v-else-if="variant === 'card'" class="rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 overflow-hidden" aria-hidden="true">
    <div class="h-32 bg-slate-100 dark:bg-slate-700 animate-pulse" />
    <div class="p-4 space-y-3">
      <div class="h-4 w-3/4 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
      <div class="h-3 w-1/3 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
      <div class="h-3 w-full rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
      <div class="h-3 w-2/3 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
      <div class="border-t border-slate-100 dark:border-slate-700" />
      <div class="grid grid-cols-4 gap-2">
        <div v-for="j in 4" :key="'stat-' + i + '-' + j" class="flex flex-col items-center gap-1">
          <div class="h-3 w-3 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
          <div class="h-3 w-6 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
          <div class="h-2 w-8 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
        </div>
      </div>
      <div class="flex justify-center"><div class="h-5 w-28 rounded-full bg-slate-100 dark:bg-slate-700 animate-pulse" /></div>
      <div class="border-t border-slate-100 dark:border-slate-700" />
      <div class="flex items-center justify-between">
        <div class="space-y-1">
          <div class="h-2 w-20 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
          <div class="h-2 w-24 rounded bg-slate-100 dark:bg-slate-700 animate-pulse" />
        </div>
        <div class="h-7 w-20 rounded-lg bg-slate-100 dark:bg-slate-700 animate-pulse" />
      </div>
    </div>
  </div>

  <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-5" aria-hidden="true">
    <div v-for="i in (count ?? 6)" :key="'card-' + i">
      <LoadingSkeleton variant="card" />
    </div>
  </div>
</template>
