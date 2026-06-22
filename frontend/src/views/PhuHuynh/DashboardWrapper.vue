<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import Dashboard from './Dashboard.vue'
import MobileDashboard from './MobileDashboard.vue'

const isMobile = ref(false)

function checkScreen() {
  isMobile.value = window.innerWidth < 768
}

onMounted(() => {
  checkScreen()
  window.addEventListener('resize', checkScreen)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkScreen)
})
</script>

<template>
  <component :is="isMobile ? MobileDashboard : Dashboard" />
</template>
