<script setup lang="ts">
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'

const props = defineProps<{
  currentPage: number
  totalPages: number
  totalItems: number
  pageSize: number
  range: { start: number; end: number }
}>()

const emit = defineEmits<{
  'update:page': [value: number]
  'update:pageSize': [value: number]
}>()

const pageSizeOptions = [10, 20, 50, 100]

const visiblePages = computed(() => {
  const pages: number[] = []
  const total = props.totalPages
  const current = props.currentPage

  if (total <= 5) {
    for (let i = 1; i <= total; i++) pages.push(i)
  } else {
    pages.push(1)
    let start = Math.max(2, current - 1)
    let end = Math.min(total - 1, current + 1)
    if (current <= 2) { start = 2; end = 4 }
    if (current >= total - 1) { start = total - 3; end = total - 1 }
    if (start > 2) pages.push(-1)
    for (let i = start; i <= end; i++) pages.push(i)
    if (end < total - 1) pages.push(-2)
    pages.push(total)
  }
  return pages
})

function canGoBack() { return props.currentPage > 1 }
function canGoForward() { return props.currentPage < props.totalPages }
</script>

<template>
  <div class="flex flex-col sm:flex-row items-center justify-between gap-4 pt-2">
    <p class="text-sm text-slate-500 dark:text-slate-400">
      <span class="font-medium text-slate-700 dark:text-slate-300">{{ range.start }}</span>
      -
      <span class="font-medium text-slate-700 dark:text-slate-300">{{ range.end }}</span>
      trên tổng số
      <span class="font-medium text-slate-700 dark:text-slate-300">{{ totalItems }}</span>
    </p>

    <div class="flex items-center gap-3">
      <div class="flex items-center gap-1.5 text-sm text-slate-500 dark:text-slate-400">
        <label for="page-size-select" class="sr-only">Số mục mỗi trang</label>
        <span class="hidden sm:inline">Hiển thị</span>
        <select
          id="page-size-select"
          :value="pageSize"
          class="rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 px-2 py-1.5 text-sm text-slate-700 dark:text-slate-300 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          @change="emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))"
        >
          <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
        </select>
        <span class="hidden sm:inline">/ trang</span>
      </div>

      <nav class="flex items-center gap-1" aria-label="Phân trang">
        <button
          type="button"
          :disabled="!canGoBack()"
          :aria-label="'Trang trước'"
          class="inline-flex items-center justify-center h-9 w-9 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 disabled:opacity-40 disabled:cursor-not-allowed focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
          @click="emit('update:page', currentPage - 1)"
        >
          <ChevronLeft :size="16" aria-hidden="true" />
        </button>

        <template v-for="(page, idx) in visiblePages" :key="idx">
          <span v-if="page < 0" class="px-1 text-slate-400 dark:text-slate-500 select-none">&hellip;</span>
          <button
            v-else
            type="button"
            :aria-label="'Trang ' + page"
            :aria-current="page === currentPage ? 'page' : undefined"
            class="inline-flex items-center justify-center h-9 min-w-[2.25rem] rounded-lg text-sm font-medium transition-all duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
            :class="page === currentPage
              ? 'bg-blue-600 dark:bg-blue-500 text-white shadow-sm'
              : 'border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 hover:bg-slate-50 dark:hover:bg-slate-700'"
            @click="emit('update:page', page)"
          >
            {{ page }}
          </button>
        </template>

        <button
          type="button"
          :disabled="!canGoForward()"
          :aria-label="'Trang sau'"
          class="inline-flex items-center justify-center h-9 w-9 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 text-slate-600 dark:text-slate-300 transition-all duration-200 hover:bg-slate-50 dark:hover:bg-slate-700 disabled:opacity-40 disabled:cursor-not-allowed focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500"
          @click="emit('update:page', currentPage + 1)"
        >
          <ChevronRight :size="16" aria-hidden="true" />
        </button>
      </nav>
    </div>
  </div>
</template>
