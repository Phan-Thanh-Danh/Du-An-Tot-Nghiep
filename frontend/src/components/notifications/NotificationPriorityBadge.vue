<script setup>
import { computed } from 'vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import { AlertCircle, AlertTriangle, Info, Bell } from 'lucide-vue-next'

const props = defineProps({
  priority: {
    type: String,
    required: true,
  },
})

const badgeConfig = computed(() => {
  switch (props.priority) {
    case 'KHAN_CAP':
      return { variant: 'danger', icon: AlertCircle, label: 'Khẩn cấp' }
    case 'QUAN_TRONG':
      return { variant: 'warning', icon: AlertTriangle, label: 'Quan trọng' }
    case 'THAP':
      return { variant: 'subtle', icon: Info, label: 'Ít quan trọng' }
    default:
      return { variant: 'primary', icon: Bell, label: 'Bình thường' }
  }
})
</script>

<template>
  <GlassBadge :variant="badgeConfig.variant" class="gap-1 px-2 py-0.5">
    <component :is="badgeConfig.icon" class="w-3 h-3" />
    {{ badgeConfig.label }}
  </GlassBadge>
</template>
