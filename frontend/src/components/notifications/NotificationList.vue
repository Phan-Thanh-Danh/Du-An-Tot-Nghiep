<script setup>
import NotificationCard from './NotificationCard.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { BellRing, RefreshCw } from 'lucide-vue-next'

// eslint-disable-next-line no-unused-vars
const props = defineProps({
  notifications: {
    type: Array,
    default: () => [],
  },
  loading: Boolean,
  error: {
    type: String,
    default: null,
  },
  selectedId: {
    type: [Number, String],
    default: null,
  },
  hasMore: Boolean
})

const emit = defineEmits(['select', 'mark-read', 'load-more', 'retry'])
</script>

<template>
  <div class="h-full flex flex-col">
    <div v-if="error" class="p-4 m-2 rounded-lg bg-(--color-danger-bg) text-(--color-danger-text) text-sm flex items-center justify-between">
      <span>{{ error }}</span>
      <GlassButton variant="ghost" size="sm" @click="emit('retry')">
        <RefreshCw class="w-4 h-4" />
      </GlassButton>
    </div>

    <div v-if="loading && notifications.length === 0" class="p-2 space-y-2">
      <LoadingSkeleton v-for="i in 5" :key="i" class="h-24 w-full rounded-xl" />
    </div>

    <div v-else-if="notifications.length === 0" class="flex-1 flex items-center justify-center p-8">
      <EmptyState
        :icon="BellRing"
        title="Không có thông báo"
        description="Bạn đã xem hết tất cả thông báo hiện có."
      />
    </div>

    <div v-else class="flex-1 overflow-y-auto p-2 space-y-2 custom-scrollbar">
      <NotificationCard
        v-for="notif in notifications"
        :key="notif.id"
        :notification="notif"
        :isSelected="selectedId === notif.id"
        @click="emit('select', notif)"
        @mark-read="emit('mark-read', notif)"
      />

      <div v-if="hasMore" class="pt-4 pb-2 flex justify-center">
        <GlassButton variant="subtle" size="sm" :loading="loading" @click="emit('load-more')">
          Tải thêm
        </GlassButton>
      </div>
    </div>
  </div>
</template>
