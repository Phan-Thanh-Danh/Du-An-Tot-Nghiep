<script setup lang="ts">
</script>

<template>
  <div class="h-screen flex flex-col bg-slate-50 animate-pulse">
    <!-- Header Skeleton -->
    <div class="h-[76px] bg-white border-b border-slate-200 px-6 flex items-center justify-between">
      <div>
        <div class="h-4 w-32 bg-slate-200 rounded mb-3"></div>
        <div class="h-6 w-64 bg-slate-200 rounded mb-2"></div>
        <div class="h-4 w-48 bg-slate-200 rounded"></div>
      </div>
      <div class="flex gap-3">
        <div class="h-10 w-32 bg-slate-200 rounded-lg"></div>
        <div class="h-10 w-32 bg-slate-200 rounded-lg"></div>
      </div>
    </div>

    <div class="flex-1 overflow-hidden flex flex-col md:flex-row max-w-[1600px] w-full mx-auto">
      <!-- Left Panel Skeleton -->
      <div class="flex-1 flex flex-col p-4 md:p-6 overflow-hidden">
        <div class="h-28 bg-slate-200 rounded-xl mb-6"></div>
        <div class="h-16 bg-slate-200 rounded-xl mb-4"></div>
        <div class="space-y-4">
          <div v-for="i in 3" :key="i" class="h-24 bg-slate-200 rounded-xl"></div>
        </div>
      </div>
      
      <!-- Right Panel Skeleton -->
      <div class="w-full md:w-[420px] lg:w-[480px] bg-white border-l border-slate-200 flex flex-col">
        <div class="h-[68px] bg-slate-100 border-b border-slate-200 px-5 py-4"></div>
        <div class="h-32 bg-slate-50 border-b border-slate-200 p-4"></div>
        <div class="flex-1 p-4 space-y-4 bg-white">
          <div v-for="i in 5" :key="'r'+i" class="h-20 bg-slate-100 rounded-lg"></div>
        </div>
      </div>
    </div>
  </div>
</template>
