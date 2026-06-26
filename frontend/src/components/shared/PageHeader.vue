<script setup lang="ts">
import { ChevronRight, House, RefreshCw, Plus } from 'lucide-vue-next'

defineProps<{
  title: string
  description?: string
  isLoading?: boolean
}>()

const emit = defineEmits<{
  refresh: []
  add: []
}>()
</script>

<template>
  <div class="space-y-3">
    <nav class="flex items-center gap-1.5 text-xs font-medium text-slate-400 dark:text-slate-500" aria-label="Breadcrumb">
      <House :size="14" class="text-slate-400 dark:text-slate-500" aria-hidden="true" />
      <span>Quản lý nội dung</span>
      <ChevronRight :size="12" aria-hidden="true" />
      <span class="text-slate-700 dark:text-slate-300" aria-current="page">
        {{ title }}
      </span>
    </nav>

    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
      <div class="space-y-1">
        <h1 class="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">
          {{ title }}
        </h1>
        <p v-if="description" class="text-sm text-slate-500 dark:text-slate-400">
          {{ description }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          type="button"
          :disabled="isLoading"
          :aria-label="isLoading ? 'Đang tải...' : 'Làm mới dữ liệu'"
          class="inline-flex items-center justify-center gap-1.5 rounded-xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 px-3.5 py-2 text-sm font-medium text-slate-600 dark:text-slate-300 shadow-sm transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 hover:border-slate-300 dark:hover:border-slate-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
          @click="emit('refresh')"
        >
          <RefreshCw :size="16" :class="isLoading ? 'animate-spin' : ''" aria-hidden="true" />
          <span class="hidden sm:inline">Làm mới</span>
        </button>
        <button
          type="button"
          :aria-label="'Thêm môn học'"
          class="inline-flex items-center justify-center gap-1.5 rounded-xl bg-blue-600 dark:bg-blue-500 px-4 py-2 text-sm font-semibold text-white shadow-sm transition-all duration-200 hover:bg-blue-700 dark:hover:bg-blue-600 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
          @click="emit('add')"
        >
          <Plus :size="16" aria-hidden="true" />
          <span>Thêm môn học</span>
        </button>
      </div>
    </div>
  </div>
</template>
