<script setup>
import { computed } from 'vue'
import { Moon, Sun } from 'lucide-vue-next'
import { useThemeStore } from '@/stores/theme'

const themeStore = useThemeStore()

const label = computed(() =>
  themeStore.isDark ? 'Chuyển sang giao diện sáng' : 'Chuyển sang giao diện tối',
)
</script>

<template>
  <button
    type="button"
    class="lg-theme-toggle"
    :aria-label="label"
    :aria-pressed="themeStore.isDark"
    @click="themeStore.toggleMode"
  >
    <span class="lg-theme-toggle__track" aria-hidden="true">
      <span class="lg-theme-toggle__thumb">
        <Sun v-if="themeStore.isDark" :size="10" stroke-width="2.2" />
        <Moon v-else :size="10" stroke-width="2.2" />
      </span>
    </span>
    <span class="hidden text-xs font-bold sm:inline">
      {{ themeStore.isDark ? 'Dark' : 'Light' }}
    </span>
  </button>
</template>
