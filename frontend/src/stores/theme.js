import { computed, ref, watch } from 'vue'
import { defineStore } from 'pinia'

const THEME_STORAGE_KEY = 'lms_theme_mode'
const DARK_CLASS = 'dark'

function getPreferredMode() {
  if (typeof window === 'undefined') return 'light'

  const storedMode = localStorage.getItem(THEME_STORAGE_KEY)
  if (storedMode === 'light' || storedMode === 'dark') return storedMode

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

function applyMode(nextMode) {
  if (typeof document === 'undefined') return

  const isDark = nextMode === 'dark'
  document.documentElement.classList.toggle(DARK_CLASS, isDark)
  document.documentElement.style.colorScheme = isDark ? 'dark' : 'light'
}

export const useThemeStore = defineStore('theme', () => {
  const mode = ref(getPreferredMode())
  const isDark = computed(() => mode.value === 'dark')

  applyMode(mode.value)

  watch(mode, (nextMode) => {
    applyMode(nextMode)

    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(THEME_STORAGE_KEY, nextMode)
    }
  })

  function toggleMode() {
    mode.value = isDark.value ? 'light' : 'dark'
  }

  return {
    mode,
    isDark,
    toggleMode,
  }
})
