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

// Icon group header
const GroupIcon = computed(() => LucideIcons[props.group.icon] || LucideIcons.Circle)

// Nếu group có children => là nhóm expandable
const hasChildren = computed(() => props.group.children && props.group.children.length > 0)

// Nếu group không có children => là route trực tiếp (Dashboard)
const isDirectRoute = computed(() => !hasChildren.value && !!props.group.route)

// Active nếu bất kỳ child nào active, hoặc chính route đang active
const isGroupActive = computed(() => {
  if (isDirectRoute.value) return route.path === props.group.route || route.path.startsWith(props.group.route + '/')
  return props.group.children?.some(
    (c) => route.path === c.route || route.path.startsWith(c.route + '/'),
  )
})

// State mở/đóng submenu (auto-expand nếu có child đang active)
const isOpen = ref(isGroupActive.value)

// Flyout state khi collapsed
const flyoutVisible = ref(false)
const menuRef = ref(null)
const buttonRef = ref(null)
const flyoutStyle = ref({})

function toggleOpen() {
  if (!props.collapsed && hasChildren.value) {
    isOpen.value = !isOpen.value
  } else if (props.collapsed && hasChildren.value) {
    flyoutVisible.value = !flyoutVisible.value
    if (flyoutVisible.value) {
      updateFlyoutPosition()
    }
  }
}

function updateFlyoutPosition() {
  if (buttonRef.value) {
    const rect = buttonRef.value.getBoundingClientRect()
    flyoutStyle.value = {
      top: `${rect.top}px`,
      left: `${rect.right + 12}px` // cách 12px
    }
  }
}

function handleClickOutside(event) {
  // Check nếu click không nằm trong button và không nằm trong menu flyout
  if (props.collapsed && flyoutVisible.value) {
    const isClickInButton = buttonRef.value && buttonRef.value.contains(event.target)
    const isClickInMenu = menuRef.value && menuRef.value.contains(event.target)
    
    if (!isClickInButton && !isClickInMenu) {
      flyoutVisible.value = false
    }
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

watch(() => route.path, () => {
  flyoutVisible.value = false
})
</script>

<template>
  <div>
    <!-- ── Direct Route (Dashboard, no children) ── -->
    <SidebarMenuItem
      v-if="isDirectRoute"
      :item="group"
      :collapsed="collapsed"
      :depth="0"
    />

    <!-- ── Group header (có children) ── -->
    <template v-else>
      <button
        ref="buttonRef"
        class="lg-sidebar-item group flex w-full items-center gap-3 rounded-xl px-3 py-2.5 transition-all duration-200 focus:outline-none focus:ring-4 focus:ring-blue-500/20"
        :class="[
          isGroupActive
            ? 'lg-sidebar-item-active font-semibold'
            : 'text-slate-500 hover:text-slate-700',
          collapsed ? 'justify-center' : '',
        ]"
        :title="collapsed ? group.label : ''"
        @click="toggleOpen"
      >
        <!-- Group icon -->
        <component
          :is="GroupIcon"
          :size="18"
          :stroke-width="isGroupActive ? 2.5 : 1.8"
          :class="[
            'flex-shrink-0 transition-colors',
            isGroupActive ? 'text-white' : 'text-slate-400 group-hover:text-blue-600',
          ]"
        />

        <!-- Group label -->
        <span v-if="!collapsed" class="flex-1 text-left text-sm font-medium leading-tight">
          {{ group.label }}
        </span>

        <!-- Chevron -->
        <LucideIcons.ChevronDown
          v-if="!collapsed"
          :size="15"
          :class="[
            'flex-shrink-0 transition-transform duration-200 text-slate-400',
            isOpen ? 'rotate-180' : '',
          ]"
        />
      </button>

      <!-- ── Expanded submenu (khi sidebar mở rộng) ── -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 -translate-y-1"
        enter-to-class="opacity-100 translate-y-0"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0"
        leave-to-class="opacity-0 -translate-y-1"
      >
        <div v-if="!collapsed && isOpen" class="ml-4 mt-1 space-y-1 border-l border-white/45 pl-3">
          <SidebarMenuItem
            v-for="child in group.children"
            :key="child.id"
            :item="child"
            :collapsed="false"
            :depth="1"
          />
        </div>
      </Transition>

      <!-- ── Flyout submenu (khi sidebar collapsed) qua Teleport ── -->
      <Teleport to="body">
        <Transition
          enter-active-class="transition-all duration-150 ease-out"
          enter-from-class="opacity-0 translate-x-1"
          enter-to-class="opacity-100 translate-x-0"
          leave-active-class="transition-all duration-100 ease-in"
          leave-from-class="opacity-100 translate-x-0"
          leave-to-class="opacity-0 translate-x-1"
        >
          <div
            v-if="collapsed && flyoutVisible"
            ref="menuRef"
            :style="flyoutStyle"
            class="lg-glass-strong fixed z-[100] min-w-[220px] rounded-2xl p-2 shadow-2xl"
          >
            <!-- Group label trong flyout -->
            <div class="mb-1.5 px-3 py-1.5 text-xs font-semibold uppercase tracking-wider text-slate-400">
              {{ group.label }}
            </div>
            <SidebarMenuItem
              v-for="child in group.children"
              :key="child.id"
              :item="child"
              :collapsed="false"
              :depth="0"
              @click="flyoutVisible = false"
            />
          </div>
        </Transition>
      </Teleport>
    </template>
  </div>
</template>
