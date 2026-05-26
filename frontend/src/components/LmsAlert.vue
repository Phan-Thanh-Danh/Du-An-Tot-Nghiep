<script setup>
import { onMounted } from 'vue'
import { usePopupStore } from '@/stores/popup'

const props = defineProps({
  type: {
    type: String,
    default: 'info',
    validator: (v) => ['success', 'error', 'warning', 'info'].includes(v),
  },
  title: String,
  message: String,
  duration: { type: Number, default: 5000 },
})

const popupStore = usePopupStore()

onMounted(() => {
  popupStore.show({
    type: props.type,
    title: props.title,
    message: props.message,
    duration: props.duration,
  })
})
</script>

<template>
  <slot />
</template>
