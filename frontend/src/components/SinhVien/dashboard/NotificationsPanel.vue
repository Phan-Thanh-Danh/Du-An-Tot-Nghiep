<script setup>
import { Bell, Dot } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

defineProps({
  notifications: {
    type: Array,
    default: () => [],
  },
})
</script>

<template>
  <GlassPanel density="none" class="rounded-[28px]">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 dark:border-white/10 px-4 py-3.5">
      <div>
        <h2 class="text-base font-bold text-slate-950 dark:text-slate-100">Thông báo</h2>
        <p class="text-xs font-medium text-slate-500 dark:text-slate-400">Học tập & Hoạt động</p>
      </div>
      <Bell :size="18" class="text-blue-700 dark:text-blue-400" />
    </div>

    <div class="space-y-1.5 p-4">
      <button
        v-for="notification in notifications"
        :key="notification.id"
        class="lg-list-item group flex min-h-[60px] w-full items-center gap-3.5 p-3 text-left shadow-sm border border-white/50 dark:border-white/10"
      >
        <span
          :class="[
          'h-2 w-2 flex-shrink-0 rounded-full transition-shadow duration-300',
            notification.unread ? 'bg-blue-600 dark:bg-blue-500 shadow-[0_0_8px_rgba(37,99,235,0.5)] dark:shadow-[0_0_8px_rgba(59,130,246,0.3)]' : 'bg-slate-300 dark:bg-slate-600 opacity-60',
          ]"
        />
        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <p :class="['truncate text-[13px] leading-tight', notification.unread ? 'font-bold text-slate-950 dark:text-slate-100' : 'font-semibold text-slate-500 dark:text-slate-400']">{{ notification.title }}</p>
            <Dot v-if="notification.unread" :size="16" class="flex-shrink-0 text-blue-600 dark:text-blue-400 -mt-0.5" />
          </div>
          <div class="mt-1 flex items-center gap-2.5">
            <GlassBadge :variant="notification.variant" size="sm">{{ notification.type }}</GlassBadge>
            <span class="text-xs font-medium text-slate-400 dark:text-slate-500">{{ notification.time }}</span>
          </div>
        </div>
      </button>
    </div>
    <div class="border-t border-white/45 dark:border-white/10 bg-white/30 dark:bg-slate-700/20 px-5 py-3">
      <router-link to="/student/notifications" class="inline-flex items-center gap-1.5 text-xs font-semibold text-blue-600 dark:text-blue-400 transition-colors hover:text-blue-800 dark:hover:text-blue-300">
        Xem tất cả
      </router-link>
    </div>
  </GlassPanel>
</template>
