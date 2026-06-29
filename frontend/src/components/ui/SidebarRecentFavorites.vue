<script setup>
import { useRouter } from 'vue-router'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'
import * as LucideIcons from 'lucide-vue-next'

defineProps({
  collapsed: { type: Boolean, default: false },
})

const router = useRouter()
const store = useRecentFavoritesStore()

function navigate(path) {
  if (path) router.push(path)
}

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Circle
}
</script>

<template>
  <div v-if="!collapsed && (store.favoritePages.length > 0 || store.recentPages.length > 0)" class="space-y-1.5 px-2 py-1.5">
    <!-- Favorites -->
    <div v-if="store.favoritePages.length > 0">
      <p class="px-2.5 pb-0.5 text-[9px] font-bold uppercase tracking-wider text-muted">Yêu thích</p>
      <div class="space-y-0.5">
        <div
          v-for="page in store.favoritePages"
          :key="page.path"
          class="group flex w-full cursor-pointer items-center gap-2 rounded-(--radius-md) px-2.5 py-1 text-left text-[11px] font-semibold text-label transition-all hover:bg-(--surface-card-hover) hover:text-heading"
          @click="navigate(page.path)"
        >
          <component :is="getIcon(page.icon)" :size="14" class="flex-shrink-0 text-muted" />
          <span class="flex-1 truncate">{{ page.label }}</span>
          <button
            class="flex h-5 w-5 items-center justify-center rounded-full opacity-0 transition-all hover:bg-(--color-warning-bg) hover:text-(--color-warning-text) group-hover:opacity-100"
            @click.stop="store.toggleFavorite(page.path)"
            title="Bỏ yêu thích"
          >
            <LucideIcons.Star :size="10" class="fill-amber-400 text-amber-400" />
          </button>
        </div>
      </div>
    </div>

    <!-- Recent -->
    <div v-if="store.recentPages.length > 0">
      <p class="px-2.5 pb-0.5 text-[9px] font-bold uppercase tracking-wider text-muted">Gần đây</p>
      <div class="space-y-0.5">
        <div
          v-for="page in store.recentPages.slice(0, 5)"
          :key="page.path"
          class="group flex w-full cursor-pointer items-center gap-2 rounded-(--radius-md) px-2.5 py-1 text-left text-[11px] font-semibold text-muted transition-all hover:bg-(--surface-card-hover) hover:text-label"
          @click="navigate(page.path)"
        >
          <component :is="getIcon(page.icon)" :size="14" class="flex-shrink-0 text-muted" />
          <span class="flex-1 truncate">{{ page.label }}</span>
          <button
            class="flex h-5 w-5 items-center justify-center rounded-full text-muted opacity-0 transition-all hover:bg-(--color-warning-bg) hover:text-(--color-warning-text) group-hover:opacity-100"
            @click.stop="store.toggleFavorite(page.path)"
            :title="store.isFavorite(page.path) ? 'Bỏ yêu thích' : 'Thêm yêu thích'"
          >
            <LucideIcons.Star :size="10" :class="store.isFavorite(page.path) ? 'fill-amber-400 text-amber-400' : ''" />
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
