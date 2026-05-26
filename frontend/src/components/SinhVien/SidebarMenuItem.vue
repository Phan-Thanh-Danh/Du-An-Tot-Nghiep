<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'

const props = defineProps({
  item: { type: Object, required: true },
  collapsed: { type: Boolean, default: false },
  depth: { type: Number, default: 0 }, // 0 = top-level child, 1 = sub-item
})

const route = useRoute()
const router = useRouter()

// Lấy icon component từ lucide-vue-next theo tên chuỗi
const IconComponent = computed(() => {
  return LucideIcons[props.item.icon] || LucideIcons.Circle
})

const isActive = computed(() => {
  if (!props.item.route) return false
  return route.path === props.item.route || route.path.startsWith(props.item.route + '/')
})

function preloadRoute() {
  if (!props.item.route) return
  const match = router.resolve(props.item.route)
  if (match && match.matched.length > 0) {
    match.matched.forEach(m => {
      Object.values(m.components).forEach(comp => {
        // Trigger the dynamic import function if it's a lazy loaded component
        if (typeof comp === 'function') {
          comp()
        }
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
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
      depth === 0
        ? 'text-slate-600 dark:text-slate-400 hover:text-slate-950 dark:hover:text-slate-100'
        : 'pl-4 text-[13px] text-slate-500 dark:text-slate-500 hover:text-slate-800 dark:hover:text-slate-300',
      isActive
        ? depth === 0
          ? 'lg-sidebar-item-active font-semibold'
          : 'border-white/60 dark:border-white/10 bg-white/70 dark:bg-slate-700/60 text-[var(--sidebar-accent-dark)] font-semibold shadow-sm'
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
      :size="depth === 0 ? 18 : 15"
      :stroke-width="isActive ? 2.5 : 1.8"
      :class="[
        'flex-shrink-0 transition-colors duration-300',
        isActive
          ? depth === 0 ? 'text-white' : 'text-[var(--sidebar-accent)]'
          : 'text-slate-400 dark:text-slate-500 group-hover:text-[var(--sidebar-accent)]',
      ]"
    />

    <!-- Label -->
    <span v-if="!collapsed" class="flex-1 truncate leading-tight transition-colors duration-300">
      {{ item.label }}
    </span>

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
      'lg-sidebar-item group relative flex w-full items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-[var(--sidebar-focus-ring)]',
      depth === 0
        ? 'text-slate-600 dark:text-slate-400 hover:text-slate-950 dark:hover:text-slate-100'
        : 'pl-4 text-[13px] text-slate-500 dark:text-slate-500 hover:text-slate-800 dark:hover:text-slate-300',
    ]"
  >
    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 18 : 15"
      stroke-width="1.8"
      class="flex-shrink-0 text-slate-400 dark:text-slate-500 group-hover:text-[var(--sidebar-accent)]"
    />

    <!-- Label -->
    <span v-if="!collapsed" class="flex-1 truncate leading-tight">
      {{ item.label }}
    </span>
  </button>
</template>
