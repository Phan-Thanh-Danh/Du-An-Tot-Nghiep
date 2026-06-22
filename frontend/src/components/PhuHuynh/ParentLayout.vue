<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import LayoutPhuHuynh from './Layout_PhuHuynh.vue'
import LayoutPhuHuynhMobile from './Layout_PhuHuynh_Mobile.vue'

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
  <component :is="isMobile ? LayoutPhuHuynhMobile : LayoutPhuHuynh" />
</template>
