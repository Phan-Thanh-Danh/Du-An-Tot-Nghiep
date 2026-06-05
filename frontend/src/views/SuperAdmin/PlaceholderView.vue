<script setup>
/**
 * PlaceholderView.vue - SuperAdmin
 * Hiển thị khi trang đang được phát triển.
 */
import { useRoute } from 'vue-router'
import { Construction } from 'lucide-vue-next'

const route = useRoute()
const title = route.meta?.title || 'Trang này'
</script>

<template>
  <div class="flex flex-col items-center justify-center py-24 text-center">
    <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-violet-50 dark:bg-violet-600/20 shadow-sm">
      <Construction :size="28" class="text-violet-400 dark:text-violet-500" />
    </div>
    <h3 class="mt-4 text-base font-semibold text-slate-700 dark:text-slate-300">Đang phát triển</h3>
    <p class="mt-1.5 text-sm text-slate-400 dark:text-slate-500 max-w-xs">
      Trang <strong class="text-slate-600 dark:text-slate-400">{{ title }}</strong> đang được xây dựng và sẽ ra mắt sớm.
    </p>
    <router-link
      to="/super-admin/dashboard"
      class="mt-6 inline-flex items-center gap-2 rounded-xl bg-violet-600 px-4 py-2 text-sm font-semibold text-white hover:bg-violet-700 transition-colors shadow-sm"
    >
      ← Về Dashboard
    </router-link>
  </div>
</template>
