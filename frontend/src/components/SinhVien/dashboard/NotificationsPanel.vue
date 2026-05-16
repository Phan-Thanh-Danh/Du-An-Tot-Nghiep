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
  <GlassPanel class="rounded-[28px] overflow-hidden" padding="p-0">
    <div class="flex items-center justify-between gap-3 border-b border-white/45 px-5 py-4">
      <div>
        <h2 class="text-base font-bold text-slate-950">Thông báo</h2>
        <p class="text-[11px] font-bold text-slate-500 uppercase tracking-wider">Học tập & Hoạt động</p>
      </div>
      <Bell :size="18" class="text-blue-700" />
    </div>

    <div class="space-y-1.5 p-4">
      <button
        v-for="notification in notifications"
        :key="notification.id"
        class="lg-list-item group flex min-h-[60px] w-full items-center gap-3.5 p-3 text-left shadow-sm border border-white/50"
      >
        <span
          :class="[
            'h-2 w-2 flex-shrink-0 rounded-full transition-shadow duration-300',
            notification.unread ? 'bg-blue-600 shadow-[0_0_8px_rgba(37,99,235,0.5)]' : 'bg-slate-300 opacity-60',
          ]"
        />
        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <p class="truncate text-[13px] font-bold text-slate-950 leading-tight">{{ notification.title }}</p>
            <Dot v-if="notification.unread" :size="16" class="flex-shrink-0 text-blue-600 -mt-0.5" />
          </div>
          <div class="mt-1 flex items-center gap-2.5">
            <GlassBadge :variant="notification.variant" size="sm">{{ notification.type }}</GlassBadge>
            <span class="text-[10px] font-bold uppercase tracking-wider text-slate-400">{{ notification.time }}</span>
          </div>
        </div>
      </button>
    </div>
  </GlassPanel>
</template>
