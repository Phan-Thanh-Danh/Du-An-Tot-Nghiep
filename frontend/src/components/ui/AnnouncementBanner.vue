<script setup>
import { useAnnouncementsStore } from '@/stores/announcements'
import * as LucideIcons from 'lucide-vue-next'

const annStore = useAnnouncementsStore()

const iconMap = {
  info: 'Info',
  warning: 'AlertTriangle',
  success: 'CheckCircle2',
  error: 'XCircle',
}

const colorMap = {
  info: 'border-blue-200/50 dark:border-blue-500/20 bg-blue-50/80 dark:bg-blue-600/15 text-blue-700 dark:text-blue-300',
  warning: 'border-amber-200/50 dark:border-amber-500/20 bg-amber-50/80 dark:bg-amber-600/15 text-amber-700 dark:text-amber-300',
  success: 'border-emerald-200/50 dark:border-emerald-500/20 bg-emerald-50/80 dark:bg-emerald-600/15 text-emerald-700 dark:text-emerald-300',
  error: 'border-red-200/50 dark:border-red-500/20 bg-red-50/80 dark:bg-red-600/15 text-red-700 dark:text-red-300',
}
</script>

<template>
  <div class="space-y-0.5 px-2 pt-0.5">
    <div
      v-for="ann in annStore.visibleAnnouncements"
      :key="ann.id"
      :class="['flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-[10px] font-semibold backdrop-blur-sm transition-all', colorMap[ann.type] || colorMap.info]"
    >
      <component :is="LucideIcons[iconMap[ann.type] || 'Info']" :size="12" class="flex-shrink-0" />
      <span class="flex-1">{{ ann.message }}</span>
      <button
        v-if="ann.dismissable !== false"
        class="flex h-4 w-4 items-center justify-center rounded-full hover:bg-white/50 dark:hover:bg-white/10 transition-colors opacity-60 hover:opacity-100"
        @click="annStore.dismiss(ann.id)"
        aria-label="Đóng thông báo"
      >
        <LucideIcons.X :size="10" />
      </button>
    </div>
  </div>
</template>
