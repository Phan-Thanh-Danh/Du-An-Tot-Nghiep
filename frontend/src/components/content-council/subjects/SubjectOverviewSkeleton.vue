<script setup>
</script>

<template>
  <div class="flex-1 flex flex-col animate-pulse">
    <!-- Header Skeleton -->
    <div class="surface-card rounded-t-xl border-x border-t border-card p-5 md:p-6 pb-0 flex flex-col md:flex-row gap-6 justify-between">
      <div class="flex-1 w-full">
        <div class="flex items-center gap-3 mb-4">
          <div class="h-5 w-20 bg-slate-200 rounded"></div>
          <div class="h-5 w-24 bg-slate-200 rounded-full"></div>
        </div>
        <div class="h-8 w-3/4 md:w-1/2 bg-slate-200 rounded mb-4"></div>
        <div class="h-4 w-full bg-slate-200 rounded mb-2"></div>
        <div class="h-4 w-5/6 bg-slate-200 rounded mb-6"></div>
      </div>
      <div class="flex items-center gap-3 shrink-0 mb-4 md:mb-0">
        <div class="h-10 w-32 bg-slate-200 rounded-lg"></div>
        <div class="h-10 w-40 bg-slate-200 rounded-lg"></div>
      </div>
    </div>

    <!-- Tabs Skeleton -->
    <div class="surface-card border-x border-b border-card rounded-b-xl mb-6 h-14 flex items-end px-6 space-x-8">
      <div class="h-4 w-20 bg-slate-200 rounded mb-4"></div>
      <div class="h-4 w-32 bg-slate-200 rounded mb-4"></div>
      <div class="h-4 w-16 bg-slate-200 rounded mb-4"></div>
    </div>

    <!-- Content Skeleton -->
    <div class="flex-1 flex flex-col gap-6 items-stretch">
      <!-- Top Card Skeleton -->
      <div class="w-full surface-card rounded-xl border border-card p-5">
        <div class="h-6 w-48 bg-slate-200 rounded mb-6"></div>
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
        </div>
      </div>

      <!-- Bottom Card Skeleton (Accordion) -->
      <div class="w-full surface-card rounded-xl border border-card p-5">
        <div class="flex justify-between items-center mb-6">
          <div class="h-6 w-48 bg-slate-200 rounded"></div>
          <div class="h-5 w-24 bg-slate-200 rounded"></div>
        </div>
        <div class="space-y-4">
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
          <div class="h-16 w-full lg-glass-soft rounded-lg border border-card"></div>
        </div>
      </div>
    </div>
  </div>
</template>
