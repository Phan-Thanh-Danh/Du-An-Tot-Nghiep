<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import * as LucideIcons from 'lucide-vue-next'
import SidebarMenuItem from './SidebarMenuItem.vue'

const props = defineProps({
  group: { type: Object, required: true },
  collapsed: { type: Boolean, default: false },
})

const route = useRoute()

const GroupIcon = computed(() => LucideIcons[props.group.icon] || LucideIcons.Circle)
const hasChildren = computed(() => props.group.children && props.group.children.length > 0)
const isDirectRoute = computed(() => !hasChildren.value && !!props.group.route)

const isGroupActive = computed(() => {
  if (isDirectRoute.value) return route.path === props.group.route || route.path.startsWith(props.group.route + '/')
  return props.group.children?.some(
    (c) => route.path === c.route || route.path.startsWith(c.route + '/'),
  )
})

const isOpen = ref(isGroupActive.value)
const flyoutVisible = ref(false)
const buttonRef = ref(null)
const flyoutStyle = ref({ display: 'none' })

function updatePosition() {
  if (buttonRef.value) {
    const rect = buttonRef.value.getBoundingClientRect()
    const flyoutHeight = Math.min(360, 70 + (props.group.children?.length || 0) * 42)
    const viewportPadding = 12
    const maxTop = window.innerHeight - flyoutHeight - viewportPadding
    const top = Math.max(viewportPadding, Math.min(rect.top, maxTop))
    flyoutStyle.value = {
      position: 'fixed',
      top: `${top}px`,
      left: `${rect.right + 10}px`,
      display: flyoutVisible.value ? 'block' : 'none',
    }
  }
}

let hoverTimeout = null;

function showFlyout() {
  if (props.collapsed && hasChildren.value) {
    if (hoverTimeout) clearTimeout(hoverTimeout)
    flyoutVisible.value = true
    updatePosition()
  }
}

function hideFlyout() {
  if (props.collapsed) {
    hoverTimeout = setTimeout(() => {
      flyoutVisible.value = false
    }, 10)
  }
}

function handleToggle() {
  if (!props.collapsed) {
    if (hasChildren.value) {
      isOpen.value = !isOpen.value
    }
  } else {
    if (hasChildren.value) {
      flyoutVisible.value = !flyoutVisible.value
      updatePosition()
    }
  }
}

function handleClickOutside(event) {
  if (flyoutVisible.value) {
    const flyoutEl = document.getElementById(`flyout-${props.group.id}`)
    if (flyoutEl && !flyoutEl.contains(event.target) && !buttonRef.value?.contains(event.target)) {
      flyoutVisible.value = false
      updatePosition()
    }
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  window.addEventListener('resize', updatePosition)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  window.removeEventListener('resize', updatePosition)
})

watch(() => route.path, () => {
  flyoutVisible.value = false
  updatePosition()
})

watch(() => props.collapsed, () => {
  flyoutVisible.value = false
  updatePosition()
})
</script>

<template>
  <div class="relative w-full">
    <!-- TRƯỜNG HỢP: DASHBOARD -->
    <SidebarMenuItem
      v-if="isDirectRoute"
      :item="group"
      :collapsed="collapsed"
      :depth="0"
    />

    <!-- TRƯỜNG HỢP: CÓ DANH MỤC CON -->
    <template v-else>
      <button
        ref="buttonRef"
        type="button"
        class="lg-sidebar-item group flex w-full items-center gap-3 rounded-xl px-3 py-2 transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-blue-500/20"
        :class="[
          isGroupActive
            ? 'lg-sidebar-item-active font-semibold shadow-sm'
            : 'text-slate-600 dark:text-slate-400 hover:text-blue-700 dark:hover:text-blue-300 hover:bg-blue-50/50 dark:hover:bg-blue-600/10',
          collapsed ? 'justify-center' : '',
        ]"
        :aria-expanded="hasChildren ? isOpen || (collapsed && flyoutVisible) : undefined"
        :aria-controls="hasChildren ? `sidebar-group-${group.id}` : undefined"
        @click.stop="handleToggle"
        @mouseenter="showFlyout"
        @mouseleave="hideFlyout"
      >
        <component
          :is="GroupIcon"
          :size="18"
          :stroke-width="isGroupActive ? 2.5 : 1.8"
          :class="[
            'flex-shrink-0 transition-colors',
            isGroupActive ? 'text-white' : 'text-slate-400 dark:text-slate-500 group-hover:text-blue-600 dark:group-hover:text-blue-400',
          ]"
        />

        <span v-if="!collapsed" class="flex-1 text-left text-[13px] font-bold leading-tight">
          {{ group.label }}
        </span>

        <LucideIcons.ChevronDown
          v-if="!collapsed"
          :size="14"
          :class="['flex-shrink-0 transition-transform duration-300 text-slate-400 dark:text-slate-500', isOpen ? 'rotate-180' : '']"
        />
      </button>

      <!-- ACCORDION KHI MỞ: Dùng grid để animate height từ 0 -> auto mượt mà -->
      <div
        v-if="!collapsed"
        :id="`sidebar-group-${group.id}`"
        class="grid transition-all duration-300 ease-in-out overflow-hidden"
        :class="isOpen ? 'grid-rows-[1fr] opacity-100 mt-1 visible' : 'grid-rows-[0fr] opacity-0 mt-0 invisible'"
      >
        <div class="min-h-0 ml-4 space-y-1 border-l border-slate-200 dark:border-white/10 pl-3">
          <SidebarMenuItem
            v-for="child in group.children"
            :key="child.id"
            :item="child"
            :collapsed="false"
            :depth="1"
          />
        </div>
      </div>

      <!-- FLYOUT KHI THU GỌN -->
      <Teleport to="body">
        <div
          v-if="collapsed && flyoutVisible"
          :id="`flyout-${group.id}`"
          :style="flyoutStyle"
          class="fixed z-[90]"
          role="menu"
          @mouseenter="showFlyout"
          @mouseleave="hideFlyout"
        >
          <div class="lg-glass-strong min-w-[220px] rounded-2xl border border-white/60 dark:border-white/10 p-1.5 shadow-[0_24px_70px_rgba(15,23,42,0.22)] dark:shadow-[0_24px_70px_rgba(2,6,23,0.4)] backdrop-blur-2xl">
            <div class="mb-1.5 px-3 py-2.5 border-b border-slate-100/50 dark:border-white/10 bg-white/40 dark:bg-slate-700/30 rounded-t-xl">
              <div class="flex items-center gap-2">
                <div class="h-6 w-6 flex items-center justify-center rounded-lg bg-blue-100 dark:bg-blue-600/25 text-blue-700 dark:text-blue-300">
                  <component :is="GroupIcon" :size="14" />
                </div>
                <p class="text-[13px] font-bold text-slate-900 dark:text-slate-100">{{ group.label }}</p>
              </div>
            </div>

            <div class="space-y-0.5">
              <SidebarMenuItem
                v-for="child in group.children"
                :key="child.id"
                :item="child"
                :collapsed="false"
                :depth="0"
                @click="hideFlyout"
              />
            </div>
          </div>
        </div>
      </Teleport>
    </template>
  </div>
</template>
