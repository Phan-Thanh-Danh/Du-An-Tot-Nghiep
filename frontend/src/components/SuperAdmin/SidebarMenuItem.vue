<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'

const props = defineProps({
  item: { type: Object, required: true },
  collapsed: { type: Boolean, default: false },
  depth: { type: Number, default: 0 },
})

const route = useRoute()
const router = useRouter()
const recentStore = useRecentFavoritesStore()

const IconComponent = computed(() => {
  return LucideIcons[props.item.icon] || LucideIcons.Circle
})

const isActive = computed(() => {
  if (!props.item.route) return false
  return route.path === props.item.route || route.path.startsWith(props.item.route + '/')
})

const isFavorite = computed(() => {
  return props.item.route ? recentStore.isFavorite(props.item.route) : false
})

function handleClick() {
  if (!props.item.route) return
  recentStore.visitPage({ path: props.item.route, label: props.item.label, icon: props.item.icon })
}

function toggleFav(event) {
  event.stopPropagation()
  event.preventDefault()
  if (!props.item.route) return
  recentStore.toggleFavorite(props.item.route, { path: props.item.route, label: props.item.label, icon: props.item.icon })
}

function preloadRoute() {
  if (!props.item.route) return
  const match = router.resolve(props.item.route)
  if (match && match.matched.length > 0) {
    match.matched.forEach(m => {
      Object.values(m.components).forEach(comp => {
        if (typeof comp === 'function') comp()
      })
    })
  }
}
</script>

<template>
  <router-link
    v-if="item.route"
    :to="item.route"
    :title="collapsed ? item.label : ''"
    :aria-current="isActive ? 'page' : undefined"
    role="menuitem"
    @mouseenter="preloadRoute"
    @focus="preloadRoute"
    @click="handleClick"
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-2.5 rounded-lg px-2.5 py-1.5 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
      depth === 0
        ? 'text-label hover:text-heading'
        : 'pl-3 text-[12px] text-muted hover:text-body',
      isActive
        ? depth === 0
          ? 'lg-sidebar-item-active font-semibold'
          : 'border-card surface-card text-[var(--sidebar-accent-dark)] font-semibold shadow-sm'
        : '',
    ]"
  >
    <!-- Active indicator line -->
    <span
      class="absolute left-0 top-1/2 -translate-y-1/2 h-6 w-1 rounded-r-full bg-[var(--sidebar-indicator)] transition-all duration-300 ease-out origin-center"
      :class="[isActive && depth === 0 ? 'opacity-100 scale-y-100' : 'opacity-0 scale-y-50']"
    />

    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 16 : 13"
      :stroke-width="isActive ? 2.5 : 1.8"
      :class="[
        'flex-shrink-0 transition-colors duration-300',
        isActive
          ? depth === 0 ? 'text-white' : 'text-[var(--sidebar-accent)]'
          : 'text-muted group-hover:text-[var(--sidebar-accent)]',
      ]"
    />

    <!-- Label -->
    <span v-if="!collapsed" class="flex-1 min-w-0 truncate leading-tight transition-colors duration-300">
      {{ item.label }}
    </span>

    <!-- Star (Favorite) button -->
    <button
      v-if="!collapsed && item.route"
      class="flex h-5 w-5 items-center justify-center rounded-full opacity-0 group-hover:opacity-100 hover:bg-amber-100 dark:hover:bg-amber-600/20 transition-all"
      :class="isFavorite ? 'opacity-100' : ''"
      @click="toggleFav"
      :title="isFavorite ? 'Bỏ yêu thích' : 'Thêm yêu thích'"
    >
      <LucideIcons.Star
        :size="10"
        :class="isFavorite ? 'fill-amber-400 text-amber-400' : 'text-muted'"
      />
    </button>

    <!-- Active dot for depth > 0 -->
    <div
      class="h-1.5 w-1.5 rounded-full bg-[var(--sidebar-indicator)] shadow-[0_0_8px_var(--sidebar-focus-ring)] transition-all duration-300 ease-out"
      :class="[isActive && depth > 0 && !collapsed ? 'opacity-100 scale-100' : 'opacity-0 scale-0']"
    />
  </router-link>

  <button
    v-else
    :title="collapsed ? item.label : ''"
    role="menuitem"
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-2.5 rounded-lg px-2.5 py-1.5 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
      depth === 0
        ? 'text-label hover:text-heading'
        : 'pl-3 text-[12px] text-muted hover:text-body',
    ]"
  >
    <component
      :is="IconComponent"
      :size="depth === 0 ? 16 : 13"
      stroke-width="1.8"
      class="flex-shrink-0 text-muted group-hover:text-[var(--sidebar-accent)]"
    />
    <span v-if="!collapsed" class="flex-1 min-w-0 truncate leading-tight">
      {{ item.label }}
    </span>
  </button>
</template>
