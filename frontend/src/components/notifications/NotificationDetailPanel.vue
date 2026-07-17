<script setup>
import { computed } from 'vue'
import { Calendar, User, ExternalLink } from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import SafeHtmlRenderer from '@/components/common/SafeHtmlRenderer.vue'
import { formatDate } from '@/utils/dateFormat'
import { getStatusMeta } from '@/utils/statusLabels'

const props = defineProps({
  notification: {
    type: Object,
    required: true
  }
})
</script>

<template>
  <div class="notification-detail flex flex-col h-full space-y-6">
    <div class="detail-header space-y-4">
      <div class="flex flex-wrap gap-2">
        <GlassBadge v-if="notification.priority === 'KHAN_CAP'" variant="danger">Khẩn cấp</GlassBadge>
        <GlassBadge v-if="notification.category" :variant="getStatusMeta('notifCategory', notification.category).variant">
          {{ getStatusMeta('notifCategory', notification.category).label }}
        </GlassBadge>
      </div>

      <h2 class="text-2xl font-semibold text-(--text-heading) leading-tight">
        {{ notification.title }}
      </h2>

      <div class="meta-row flex flex-wrap gap-4 text-sm text-(--text-muted) pb-4 border-b border-(--border-default)">
        <span class="flex items-center gap-1.5">
          <User :size="16" />
          {{ notification.sender || 'Hệ thống LMS' }}
        </span>
        <span class="flex items-center gap-1.5">
          <Calendar :size="16" />
          {{ formatDate(notification.createdAt, 'DD/MM/YYYY HH:mm') }}
        </span>
      </div>
    </div>

    <div class="detail-body flex-1 text-(--text-body) text-base leading-relaxed space-y-4">
      <!-- Safe rendering of Rich Text notification body -->
      <SafeHtmlRenderer :html="notification.body || notification.excerpt" />
    </div>

    <div v-if="notification.relatedPath" class="detail-footer pt-6 mt-auto border-t border-(--border-default)">
      <GlassButton variant="primary" :to="notification.relatedPath">
        <template #leading><ExternalLink :size="16" /></template>
        Đi đến chức năng liên quan
      </GlassButton>
    </div>
  </div>
</template>
