<script setup lang="ts">
import { BookOpen } from 'lucide-vue-next'

defineProps<{
  title?: string
  description?: string
  actionLabel?: string
}>()

const emit = defineEmits<{
  action: []
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center py-16 px-4">
    <div class="relative mb-6">
      <div class="absolute inset-4 rounded-full bg-blue-100 dark:bg-blue-500/10 blur-xl opacity-60" aria-hidden="true" />
      <div class="relative flex h-20 w-20 items-center justify-center rounded-2xl border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800 shadow-sm">
        <BookOpen :size="32" class="text-slate-300 dark:text-slate-600" aria-hidden="true" />
      </div>
    </div>
    <h3 class="text-lg font-bold text-slate-900 dark:text-white">
      {{ title || 'Chưa có dữ liệu' }}
    </h3>
    <p class="mt-2 text-sm text-slate-500 dark:text-slate-400 max-w-sm text-center leading-relaxed">
      {{ description || 'Nội dung sẽ hiển thị khi có dữ liệu.' }}
    </p>
    <button
      v-if="actionLabel"
      type="button"
      :aria-label="actionLabel"
      class="mt-6 inline-flex items-center gap-2 rounded-xl bg-blue-600 dark:bg-blue-500 px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition-all duration-200 hover:bg-blue-700 dark:hover:bg-blue-600 active:scale-[0.97] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2"
      @click="emit('action')"
    >
      {{ actionLabel }}
    </button>
  </div>
</template>
