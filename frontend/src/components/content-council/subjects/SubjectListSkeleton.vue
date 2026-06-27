<template>
  <div class="bg-white rounded-xl border border-slate-200 p-5 flex flex-col h-[280px] animate-pulse">
    <!-- Header -->
    <div class="flex items-start justify-between mb-4">
      <div class="h-4 w-16 bg-slate-200 rounded"></div>
      <div class="h-5 w-24 bg-slate-200 rounded-full"></div>
    </div>

    <!-- Title and Description -->
    <div class="h-6 w-3/4 bg-slate-200 rounded mb-2"></div>
    <div class="h-4 w-full bg-slate-100 rounded mb-1"></div>
    <div class="h-4 w-5/6 bg-slate-100 rounded mb-6"></div>

    <!-- Stats Grid -->
    <div class="grid grid-cols-2 gap-x-2 gap-y-4 mb-6 mt-auto">
      <div class="flex flex-col gap-1">
        <div class="h-6 w-10 bg-slate-200 rounded"></div>
        <div class="h-3 w-14 bg-slate-100 rounded"></div>
      </div>
      <div class="flex flex-col gap-1">
        <div class="h-6 w-10 bg-slate-200 rounded"></div>
        <div class="h-3 w-14 bg-slate-100 rounded"></div>
      </div>
      <div class="flex flex-col gap-1">
        <div class="h-6 w-10 bg-slate-200 rounded"></div>
        <div class="h-3 w-14 bg-slate-100 rounded"></div>
      </div>
      <div class="flex flex-col gap-1">
        <div class="h-6 w-10 bg-slate-200 rounded"></div>
        <div class="h-3 w-14 bg-slate-100 rounded"></div>
      </div>
    </div>

    <!-- Footer -->
    <div class="flex items-center justify-between mt-auto pt-4 border-t border-slate-100">
      <div class="h-3 w-32 bg-slate-200 rounded"></div>
      <div class="h-4 w-20 bg-slate-200 rounded"></div>
    </div>
  </div>
</template>
