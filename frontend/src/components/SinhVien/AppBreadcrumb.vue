<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { ChevronRight, Home } from 'lucide-vue-next'
import { sinhVienMenuGroups } from './data/menuData.js'

const route = useRoute()

// Xây breadcrumb tự động từ route path
const breadcrumbs = computed(() => {
  const crumbs = [{ label: 'Trang chủ', route: '/student/dashboard', icon: Home }]
  const path = route.path

  // Tìm trong menu groups
  for (const group of sinhVienMenuGroups) {
    // Direct route (Dashboard)
    if (!group.children?.length && group.route) {
      if (path === group.route || path.startsWith(group.route + '/')) {
        if (group.route !== '/student/dashboard') {
          crumbs.push({ label: group.label })
        }
        return crumbs
      }
      continue
    }

    // Group với children
    for (const child of group.children || []) {
      if (path === child.route || path.startsWith(child.route + '/')) {
        crumbs.push({ label: group.label })
        crumbs.push({ label: child.label, route: child.route })

        // Nếu có dynamic segment phía sau
        if (path !== child.route) {
          const extra = path.replace(child.route, '').replace(/^\//, '')
          if (extra) crumbs.push({ label: 'Chi tiết' })
        }
        return crumbs
      }
    }
  }

  // Fallback: parse path
  const segments = path.split('/').filter(Boolean).slice(1)
  segments.forEach((seg) => {
    if (!seg.match(/^\d+$/) && seg !== 'student') {
      crumbs.push({ label: seg.charAt(0).toUpperCase() + seg.slice(1).replace(/-/g, ' ') })
    }
  })
  return crumbs
})
</script>

<template>
  <nav aria-label="Breadcrumb" class="flex items-center gap-1 text-sm">
    <template v-for="(crumb, index) in breadcrumbs" :key="index">
      <!-- Separator -->
      <ChevronRight
        v-if="index > 0"
        :size="14"
        class="text-slate-300 flex-shrink-0"
      />

      <!-- Crumb item -->
      <component
        :is="crumb.route && index < breadcrumbs.length - 1 ? 'router-link' : 'span'"
        :to="crumb.route"
        :class="[
          'flex items-center gap-1 transition-colors leading-none',
          index === breadcrumbs.length - 1
            ? 'text-slate-700 font-semibold'
            : 'text-slate-400 hover:text-blue-600 cursor-pointer',
          index === 0 ? 'text-slate-400' : '',
        ]"
      >
        <component :is="crumb.icon" v-if="crumb.icon" :size="13" />
        {{ crumb.label }}
      </component>
    </template>
  </nav>
</template>
