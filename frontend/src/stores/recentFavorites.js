import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { useAuthStore } from './auth'

const MAX_RECENT = 8

function storageKey(role) {
  const r = (role || 'unknown').toLowerCase().replace(/\s+/g, '_')
  return `lms_recent_favorites_${r}`
}

function loadPersisted(role) {
  try {
    const raw = localStorage.getItem(storageKey(role))
    const data = raw ? JSON.parse(raw) : { recent: [], favorites: [] }
    return {
      recent: (data.recent || []).filter(p => p && p.path && p.label),
      favorites: (data.favorites || []).filter(p => p && p.path && p.label),
    }
  } catch {
    return { recent: [], favorites: [] }
  }
}

function persist(role, data) {
  localStorage.setItem(storageKey(role), JSON.stringify(data))
}

localStorage.removeItem('lms_recent_favorites')

export const useRecentFavoritesStore = defineStore('recentFavorites', () => {
  const authStore = useAuthStore()
  const currentRole = computed(() => authStore.role || 'unknown')

  const saved = loadPersisted(currentRole.value)
  const recentPages = ref(saved.recent)
  const favoritePages = ref(saved.favorites)

  const favoritePaths = computed(() => new Set(favoritePages.value.map(p => p.path)))

  watch(currentRole, (newRole) => {
    const data = loadPersisted(newRole)
    recentPages.value = data.recent
    favoritePages.value = data.favorites
  })

  function visitPage(page) {
    recentPages.value = recentPages.value.filter(p => p.path !== page.path)
    recentPages.value.unshift({ ...page, visitedAt: Date.now() })
    if (recentPages.value.length > MAX_RECENT) {
      recentPages.value = recentPages.value.slice(0, MAX_RECENT)
    }
    persist(currentRole.value, { recent: recentPages.value, favorites: favoritePages.value })
  }

  function toggleFavorite(path, pageData) {
    const idx = favoritePages.value.findIndex(p => p.path === path)
    if (idx >= 0) {
      favoritePages.value.splice(idx, 1)
    } else {
      const existing = recentPages.value.find(p => p.path === path)
      if (existing) {
        favoritePages.value.push({ ...existing })
      } else if (pageData) {
        favoritePages.value.push({ ...pageData })
      }
    }
    persist(currentRole.value, { recent: recentPages.value, favorites: favoritePages.value })
  }

  function isFavorite(path) {
    return favoritePaths.value.has(path)
  }

  function clearAll() {
    recentPages.value = []
    favoritePages.value = []
    persist(currentRole.value, { recent: [], favorites: [] })
  }

  return { recentPages, favoritePages, visitPage, toggleFavorite, isFavorite, clearAll }
})