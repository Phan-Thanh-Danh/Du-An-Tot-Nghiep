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
      'group relative flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-150',
      depth === 0
        ? 'text-slate-600 hover:bg-blue-50 hover:text-blue-700'
        : 'text-slate-500 hover:bg-slate-100 hover:text-slate-700 pl-4 text-[13px]',
      isActive
        ? depth === 0
          ? 'bg-blue-50 text-blue-700 font-semibold'
          : 'bg-blue-50 text-blue-700 font-semibold'
        : '',
    ]"
    @click="navigate"
  >
    <!-- Active indicator line -->
    <span
      v-if="isActive && depth === 0"
      class="absolute left-0 top-1/2 -translate-y-1/2 h-6 w-0.5 rounded-full bg-blue-600"
    />

    <!-- Icon -->
    <component
      :is="IconComponent"
      :size="depth === 0 ? 18 : 15"
      :stroke-width="isActive ? 2.5 : 1.8"
      :class="[
        'flex-shrink-0 transition-colors',
        isActive ? 'text-blue-600' : 'text-slate-400 group-hover:text-blue-500',
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
      class="absolute right-1 top-1 h-1.5 w-1.5 rounded-full bg-blue-600"
    />
  </button>
</template>
