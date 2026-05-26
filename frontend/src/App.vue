<script setup>
import { RouterView } from 'vue-router'
import { useThemeStore } from '@/stores/theme'
import PopupNotification from '@/components/ui/PopupNotification.vue'

useThemeStore()
</script>

<template>
  <RouterView />
  <PopupNotification />
</template>
