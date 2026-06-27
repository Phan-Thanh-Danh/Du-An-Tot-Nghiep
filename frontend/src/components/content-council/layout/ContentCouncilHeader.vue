<script setup>
import { useAuthStore } from '@/stores/auth'
import { Menu, Bell, User } from 'lucide-vue-next'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'
import Breadcrumbs from '@/components/ui/Breadcrumbs.vue'
import { ref } from 'vue'

const props = defineProps({
  isMobileDrawerOpen: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:isMobileDrawerOpen'])
const authStore = useAuthStore()

const toggleDrawer = () => {
  emit('update:isMobileDrawerOpen', !props.isMobileDrawerOpen)
}

const userMenuOpen = ref(false)
const toggleUserMenu = () => {
  userMenuOpen.value = !userMenuOpen.value
}
</script>

<template>
  <header class="lg-topbar absolute top-0 left-0 right-0 z-50 mx-2 mt-2 flex h-12 flex-shrink-0 items-center gap-2 overflow-visible rounded-[var(--radius-xl)] px-3 sm:mx-3 sm:mt-2 sm:gap-2.5">
    
    <!-- Mobile toggle -->
    <button
      class="lg-icon-button flex h-8 w-8 text-muted hover:text-link md:hidden"
      aria-label="Mở menu"
      @click="toggleDrawer"
    >
      <Menu :size="20" />
    </button>

    <!-- Page title + Breadcrumbs -->
    <div class="hidden min-w-0 flex-1 sm:block">
      <Breadcrumbs class="mb-0.5" />
      <div class="flex min-w-0 items-baseline gap-2">
        <h1 class="truncate text-base font-bold leading-tight text-heading">Nội dung môn học</h1>
        <span class="hidden text-xs font-medium text-muted lg:inline">Biên soạn chương và bài học</span>
      </div>
    </div>
    <div class="sm:hidden flex-1 font-medium text-heading text-sm">Nội dung môn học</div>

    <ThemeToggle />

    <!-- Notification -->
    <div class="relative">
      <button
        class="lg-icon-button surface-input relative h-8 w-8 border border-card text-label shadow-sm focus:outline-none hover:text-link"
        aria-label="Thông báo"
      >
        <Bell :size="15" />
        <span class="absolute -right-0.5 -top-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-[var(--color-danger-text)] text-[8px] font-bold text-inverse ring-2 ring-[var(--surface-topbar)]">1</span>
      </button>
    </div>

    <!-- Profile menu -->
    <div class="relative">
      <button
        class="surface-input flex items-center gap-2 rounded-[var(--radius-md)] border border-card p-1 transition-all duration-200 focus:outline-none ring-offset-2 focus:ring-2 focus:ring-[var(--focus-ring)] hover:bg-[var(--surface-input-focus)]"
        aria-label="Mở hồ sơ"
        @click="toggleUserMenu"
      >
        <div class="app-topbar-avatar flex h-8 w-8 items-center justify-center overflow-hidden rounded-full text-[10px] font-bold text-inverse shadow-sm ring-1 ring-[var(--border-card)]">
          <span v-if="authStore.initials" class="font-medium text-sm">{{ authStore.initials }}</span>
          <User v-else class="w-4 h-4" />
        </div>
        <div class="hidden pr-1.5 text-left sm:block">
          <p class="text-[12px] font-semibold leading-tight text-heading">{{ authStore.displayName || 'Hội đồng Nội dung Demo' }}</p>
          <p class="text-[10px] font-medium capitalize text-muted">Hội đồng nội dung</p>
        </div>
      </button>
    </div>
  </header>
</template>

<style scoped>
.app-topbar-avatar {
  background: linear-gradient(
    135deg,
    var(--active-start, var(--accent-primary)),
    var(--active-mid, var(--accent-primary)) 55%,
    var(--active-end, var(--accent-cyan))
  );
}
</style>
