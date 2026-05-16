<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'

const props = defineProps({
  item: { type: Object, required: true },
  collapsed: { type: Boolean, default: false },
  depth: { type: Number, default: 0 }, // 0 = top-level child, 1 = sub-item
})

const route = useRoute()

// Lấy icon component từ lucide-vue-next theo tên chuỗi
const IconComponent = computed(() => {
  return LucideIcons[props.item.icon] || LucideIcons.Circle
})

const isActive = computed(() => {
  if (!props.item.route) return false
  return route.path === props.item.route || route.path.startsWith(props.item.route + '/')
})
</script>

<template>
  <router-link
    v-if="item.route"
    :to="item.route"
    :title="collapsed ? item.label : ''"
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-blue-500/20',
      depth === 0
        ? 'text-slate-600 hover:text-slate-950'
        : 'pl-4 text-[13px] text-slate-500 hover:text-slate-800',
      isActive
        ? depth === 0
          ? 'lg-sidebar-item-active font-semibold'
          : 'border-white/60 bg-white/70 text-blue-800 font-semibold shadow-sm'
        : '',
    ]"
  >
    <!-- Active indicator line -->
    <span
      v-if="isActive && depth === 0"
      class="absolute left-0 top-1/2 -translate-y-1/2 h-6 w-1 rounded-r-full bg-blue-600"
    />

    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 18 : 15"
      :stroke-width="isActive ? 2.5 : 1.8"
      :class="[
        'flex-shrink-0 transition-colors',
        isActive 
          ? depth === 0 ? 'text-white' : 'text-blue-600'
          : 'text-slate-400 group-hover:text-blue-600',
      ]"
    />

    <!-- Label -->
    <span v-if="!collapsed" class="flex-1 truncate leading-tight">
      {{ item.label }}
    </span>

    <!-- Active dot for depth > 0 -->
    <div
      v-if="isActive && depth > 0 && !collapsed"
      class="h-1.5 w-1.5 rounded-full bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.5)]"
    />
  </router-link>

  <button
    v-else
    :title="collapsed ? item.label : ''"
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-blue-500/20',
      depth === 0
        ? 'text-slate-600 hover:text-slate-950'
        : 'pl-4 text-[13px] text-slate-500 hover:text-slate-800',
    ]"
  >
    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 18 : 15"
      stroke-width="1.8"
      class="flex-shrink-0 text-slate-400 group-hover:text-blue-600"
    />

    <!-- Label -->
    <span v-if="!collapsed" class="flex-1 truncate leading-tight">
      {{ item.label }}
    </span>
  </button>
</template>
