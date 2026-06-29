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

let hoverTimeout = null

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
    const flyoutEl = document.getElementById(`flyout-admin-${props.group.id}`)
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
    <!-- TRƯỜNG HỢP: DIRECT ROUTE (Dashboard) -->
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
        class="lg-sidebar-item group flex w-full items-center gap-2.5 rounded-lg px-2.5 py-1.5 transition-all duration-200 focus:outline-none focus:ring-(--sidebar-focus-ring)"
        :class="[
          isGroupActive
            ? 'lg-sidebar-item-active font-semibold shadow-sm'
            : 'text-label hover:text-(--sidebar-accent) dark:hover:text-(--sidebar-accent-dark) hover:bg-(--accent-primary-soft)',
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
          :size="16"
          :stroke-width="isGroupActive ? 2.5 : 1.8"
          :class="[
            'flex-shrink-0 transition-colors',
            isGroupActive ? 'text-white' : 'text-muted group-hover:text-(--sidebar-accent) dark:group-hover:text-(--sidebar-accent-dark)',
          ]"
        />

        <span v-if="!collapsed" class="flex-1 min-w-0 truncate text-left text-[13px] font-bold leading-tight">
          {{ group.label }}
        </span>

        <LucideIcons.ChevronDown
          v-if="!collapsed"
          :size="14"
          :class="['flex-shrink-0 transition-transform duration-300 text-muted', isOpen ? 'rotate-180' : '']"
        />
      </button>

      <!-- ACCORDION KHI MỞ -->
      <div
        v-if="!collapsed"
        :id="`sidebar-group-${group.id}`"
        class="grid transition-all duration-300 ease-in-out overflow-hidden"
        :class="isOpen ? 'grid-rows-[1fr] opacity-100 mt-1 visible' : 'grid-rows-[0fr] opacity-0 mt-0 invisible'"
      >
        <div class="min-h-0 ml-3 w-[calc(100%-0.75rem)] space-y-0.5 border-l border-default pl-2">
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
          :id="`flyout-admin-${group.id}`"
          :style="flyoutStyle"
          class="fixed z-[90]"
          role="menu"
          @mouseenter="showFlyout"
          @mouseleave="hideFlyout"
        >
          <div class="lg-glass-strong min-w-[220px] rounded-2xl border border-card p-1.5 shadow-[0_24px_70px_rgba(15,23,42,0.22)] dark:shadow-[0_24px_70px_rgba(2,6,23,0.4)] backdrop-blur-2xl">
            <div class="mb-1 px-3 py-2 border-b border-default surface-card rounded-t-xl">
              <div class="flex items-center gap-2">
                <div class="h-5 w-5 flex items-center justify-center rounded-lg" :style="{ background: 'color-mix(in srgb, var(--sidebar-accent) 20%, transparent)', color: 'var(--sidebar-accent)' }">
                  <component :is="GroupIcon" :size="12" />
                </div>
                <p class="text-[12px] font-bold text-heading">{{ group.label }}</p>
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
