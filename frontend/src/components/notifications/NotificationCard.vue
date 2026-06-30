<script setup>
import { computed } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import NotificationPriorityBadge from './NotificationPriorityBadge.vue'
import { Check, Clock } from 'lucide-vue-next'
import dayjs from 'dayjs'
import relativeTime from 'dayjs/plugin/relativeTime'

dayjs.extend(relativeTime)

const props = defineProps({
  notification: {
    type: Object,
    required: true,
  },
  isSelected: Boolean,
})

const emit = defineEmits(['click', 'mark-read'])

const formattedTime = computed(() => dayjs(props.notification.ngayGui).fromNow())

const panelVariant = computed(() => {
  if (props.isSelected) return 'strong'
  return props.notification.daDoc ? 'soft' : 'strong'
})
</script>

<template>
  <GlassPanel 
    :variant="panelVariant" 
    interactive 
    :class="[
      'cursor-pointer transition-all duration-200 border-l-4',
      notification.daDoc ? 'border-l-transparent opacity-80' : 'border-l-(--lg-primary) lg-glow',
      isSelected ? 'ring-2 ring-(--lg-primary)' : ''
    ]"
    padding="compact"
    @click="emit('click', notification)"
  >
    <div class="flex items-start justify-between gap-3">
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-1">
          <NotificationPriorityBadge v-if="notification.doUuTien && notification.doUuTien !== 'BINH_THUONG'" :priority="notification.doUuTien" />
          <span class="text-xs text-(--text-muted) flex items-center gap-1">
            <Clock class="w-3 h-3" />
            {{ formattedTime }}
          </span>
        </div>
        
        <h4 :class="['text-sm font-semibold truncate', notification.daDoc ? 'text-(--text-body)' : 'text-(--text-heading)']">
          {{ notification.tieuDe }}
        </h4>
        
        <p class="text-xs text-(--text-muted) line-clamp-1 mt-1">
          {{ notification.tomTat || notification.noiDung?.replace(/<[^>]+>/g, '') }}
        </p>
      </div>

      <div class="flex flex-col items-end gap-2 shrink-0">
        <div 
          v-if="!notification.daDoc" 
          class="w-2 h-2 rounded-full bg-(--lg-primary)"
          title="Chưa đọc"
        ></div>
        <button 
          v-if="!notification.daDoc"
          @click.stop="emit('mark-read', notification)"
          class="p-1 rounded hover:bg-(--surface-hover) text-(--text-muted) hover:text-(--lg-primary) transition-colors"
          title="Đánh dấu đã đọc"
        >
          <Check class="w-4 h-4" />
        </button>
      </div>
    </div>
  </GlassPanel>
</template>
