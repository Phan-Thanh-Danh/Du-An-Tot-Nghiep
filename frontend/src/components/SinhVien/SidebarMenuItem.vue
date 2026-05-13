<script setup>
import { computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'

const props = defineProps({
  item: { type: Object, required: true },
  collapsed: { type: Boolean, default: false },
  depth: { type: Number, default: 0 }, // 0 = top-level child, 1 = sub-item
})

const router = useRouter()
const route = useRoute()

// Lấy icon component từ lucide-vue-next theo tên chuỗi
const IconComponent = computed(() => {
  return LucideIcons[props.item.icon] || LucideIcons.Circle
})

const isActive = computed(() => {
  if (!props.item.route) return false
  return route.path === props.item.route || route.path.startsWith(props.item.route + '/')
})

function navigate() {
  if (props.item.route) {
    router.push(props.item.route)
  }
}
</script>

<template>
  <button
    :title="collapsed ? item.label : ''"
    :class="[
      'lg-sidebar-item group relative flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium focus:outline-none focus:ring-4 focus:ring-blue-500/20',
      depth === 0
        ? 'text-slate-600 hover:text-slate-950'
        : 'pl-4 text-[13px] text-slate-500 hover:text-slate-800',
      isActive
        ? depth === 0
          ? 'lg-sidebar-item-active font-semibold'
          : 'border-white/60 bg-white/70 text-blue-800 font-semibold shadow-sm'
        : '',
    ]"
    @click="navigate"
  >
    <!-- Active indicator line -->
    <span
      v-if="isActive && depth === 0"
      class="absolute left-1 top-1/2 h-6 w-1 -translate-y-1/2 rounded-full bg-white/80"
    />

    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 18 : 15"
      :stroke-width="isActive ? 2.5 : 1.8"
      :class="[
        'flex-shrink-0 transition-colors',
        isActive
          ? depth === 0
            ? 'text-white'
            : 'text-blue-700'
          : 'text-slate-400 group-hover:text-blue-600',
      ]"
    />

    <!-- Label (ẩn khi collapsed) -->
    <span
      v-if="!collapsed"
      class="flex-1 text-left leading-tight truncate"
    >
      {{ item.label }}
    </span>

    <!-- Active dot khi collapsed -->
    <span
      v-if="collapsed && isActive"
      class="absolute right-1 top-1 h-1.5 w-1.5 rounded-full bg-white"
    />
  </button>
</template>
