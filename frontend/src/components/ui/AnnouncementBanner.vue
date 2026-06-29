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
  info: 'border-[color-mix(in srgb,var(--color-info-text) 20%,transparent)] bg-(--color-info-bg) text-(--color-info-text)',
  warning: 'border-[color-mix(in srgb,var(--color-warning-text) 20%,transparent)] bg-(--color-warning-bg) text-(--color-warning-text)',
  success: 'border-[color-mix(in srgb,var(--color-success-text) 20%,transparent)] bg-(--color-success-bg) text-(--color-success-text)',
  error: 'border-[color-mix(in srgb,var(--color-danger-text) 20%,transparent)] bg-(--color-danger-bg) text-(--color-danger-text)',
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
