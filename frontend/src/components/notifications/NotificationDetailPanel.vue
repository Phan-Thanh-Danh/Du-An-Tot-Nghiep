<script setup>
import { computed } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import NotificationPriorityBadge from './NotificationPriorityBadge.vue'
import { Calendar, User, CheckCircle, EyeOff, Tag } from 'lucide-vue-next'
import dayjs from 'dayjs'

const props = defineProps({
  notification: {
    type: Object,
    default: null,
  }
})

const emit = defineEmits(['close', 'mark-read', 'hide'])

const formattedDate = computed(() => {
  if (!props.notification) return ''
  return dayjs(props.notification.ngayGui).format('DD/MM/YYYY HH:mm')
})
</script>

<template>
  <div class="h-full flex flex-col">
    <GlassPanel v-if="notification" variant="readable" class="flex-1 flex flex-col h-full overflow-hidden border-0 lg:border lg:border-[var(--border-card)]">
      <template #header>
        <div class="flex justify-between items-start mb-2">
          <h2 class="text-xl font-bold text-[var(--text-heading)] leading-tight">
            {{ notification.tieuDe }}
          </h2>
        </div>
        <div class="flex flex-wrap items-center gap-3 text-sm text-[var(--text-muted)] pb-2 border-b border-[var(--border-card)]">
          <span class="flex items-center gap-1">
            <User class="w-4 h-4" />
            {{ notification.nguoiGui || 'Hệ thống' }}
          </span>
          <span class="flex items-center gap-1">
            <Calendar class="w-4 h-4" />
            {{ formattedDate }}
          </span>
          <NotificationPriorityBadge v-if="notification.doUuTien" :priority="notification.doUuTien" />
          <span v-if="notification.loaiThongBao" class="flex items-center gap-1 px-2 py-0.5 rounded bg-[var(--surface-hover)]">
            <Tag class="w-3 h-3" />
            {{ notification.loaiThongBao }}
          </span>
        </div>
      </template>

      <div class="flex-1 overflow-y-auto py-4 custom-scrollbar">
        <!-- Content injected as HTML from backend editor -->
        <div class="prose prose-sm sm:prose-base dark:prose-invert max-w-none" v-html="notification.noiDung"></div>
        
        <!-- Action URL if present -->
        <div v-if="notification.actionUrl" class="mt-6">
          <GlassButton variant="primary" @click="window.open(notification.actionUrl, '_blank')">
            {{ notification.actionText || 'Xem chi tiết' }}
          </GlassButton>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-2 pt-2 border-t border-[var(--border-card)]">
          <GlassButton 
            variant="ghost" 
            size="sm" 
            @click="emit('hide', notification)"
          >
            <template #leading><EyeOff class="w-4 h-4" /></template>
            Ẩn thông báo
          </GlassButton>
          
          <GlassButton 
            v-if="!notification.daDoc"
            variant="success" 
            size="sm" 
            @click="emit('mark-read', notification)"
          >
            <template #leading><CheckCircle class="w-4 h-4" /></template>
            Đánh dấu đã đọc
          </GlassButton>
        </div>
      </template>
    </GlassPanel>

    <div v-else class="flex-1 flex flex-col items-center justify-center text-center p-8 h-full">
      <div class="w-16 h-16 rounded-full bg-[var(--surface-hover)] flex items-center justify-center mb-4">
        <Mail class="w-8 h-8 text-[var(--text-muted)]" />
      </div>
      <h3 class="text-lg font-medium text-[var(--text-heading)]">Chưa chọn thông báo</h3>
      <p class="text-[var(--text-muted)] mt-1">Chọn một thông báo từ danh sách để xem chi tiết.</p>
    </div>
  </div>
</template>
