<script setup>
/**
 * Layout_BGH.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho giao diện Hiệu trưởng / Ban giám hiệu (BGH).
 */
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from '../SinhVien/AppTopbar.vue'
import PageContainer from '../SinhVien/PageContainer.vue'
import AiAssistant from '@/components/ui/AiAssistant.vue'
import AnnouncementBanner from '@/components/ui/AnnouncementBanner.vue'

// ── Sidebar state ──────────────────────────────────────────
const sidebarCollapsed = ref(false)
const mobileSidebarOpen = ref(false)

const isSmallScreen = ref(false)

function checkScreen() {
  isSmallScreen.value = window.innerWidth < 1024
  if (isSmallScreen.value) {
    sidebarCollapsed.value = true
  }
}

onMounted(() => {
  checkScreen()
  window.addEventListener('resize', checkScreen)
})

onUnmounted(() => {
  window.removeEventListener('resize', checkScreen)
})

function toggleSidebar() {
  if (isSmallScreen.value) {
    mobileSidebarOpen.value = !mobileSidebarOpen.value
  } else {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }
}

function closeMobileSidebar() {
  mobileSidebarOpen.value = false
}

const route = useRoute()

const currentPageMeta = computed(() => {
  const meta = route.meta
  if (meta?.title) return { title: meta.title, subtitle: meta.subtitle || '' }
  return { title: 'Ban giám hiệu', subtitle: 'Hệ thống quản lý LMS' }
})

// ── Auto-close mobile sidebar on route change ──────────────
watch(() => route.path, () => {
  closeMobileSidebar()
})

// ── Body scroll lock when mobile drawer is open ────────────
watch(mobileSidebarOpen, (open) => {
  document.body.style.overflow = open ? 'hidden' : ''
})
</script>

<template>
  <div
    class="lg-app-bg relative flex h-screen w-full overflow-hidden font-sans"
    :style="{
      '--sidebar-accent': '#1e40af',
      '--active-start': '#1e3a8a',
      '--active-mid': '#1e40af',
      '--active-end': '#2563eb',
    }"
  >
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary" />
      <div class="lg-shell-orb lg-shell-orb-secondary" />
    </div>

    <!-- MOBILE OVERLAY -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-200"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="mobileSidebarOpen"
        class="lg-mobile-scrim fixed inset-0 z-40 lg:hidden"
        @click="closeMobileSidebar"
      />
    </Transition>

    <!-- SIDEBAR -->
    <div class="relative z-20 hidden h-full flex-shrink-0 lg:flex">
      <AppSidebar
        :collapsed="sidebarCollapsed"
        @toggle="toggleSidebar"
      />
    </div>

    <!-- Mobile sidebar -->
    <Transition
      enter-active-class="transition-transform duration-300 ease-out"
      enter-from-class="-translate-x-full"
      enter-to-class="translate-x-0"
      leave-active-class="transition-transform duration-200 ease-in"
      leave-from-class="translate-x-0"
      leave-to-class="-translate-x-full"
    >
      <div
        v-if="mobileSidebarOpen"
        class="fixed inset-y-0 left-0 z-50 flex lg:hidden"
      >
        <AppSidebar
          :collapsed="false"
          @toggle="closeMobileSidebar"
        />
      </div>
    </Transition>

    <!-- MAIN AREA -->
    <div class="relative z-10 flex min-w-0 flex-1 flex-col overflow-hidden pt-16">
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <AnnouncementBanner />

      <main class="flex-1 overflow-y-auto">
        <div class="lg-shell-content mx-auto">
          <PageContainer
            :title="currentPageMeta.title"
            :subtitle="currentPageMeta.subtitle"
          >
            <Suspense :timeout="0">
              <template #default>
                <router-view v-slot="{ Component, route }">
                  <Transition
                    enter-active-class="transition-all duration-200 ease-out will-change-transform will-change-opacity"
                    enter-from-class="opacity-0 translate-y-2"
                    enter-to-class="opacity-100 translate-y-0"
                    leave-active-class="transition-opacity duration-75 ease-in"
                    leave-from-class="opacity-100"
                    leave-to-class="opacity-0"
                    mode="out-in"
                  >
                    <component :is="Component" :key="route.path" />
                  </Transition>
                </router-view>
              </template>
              <template #fallback>
                <div class="flex flex-col items-center justify-center py-20">
                  <div class="h-8 w-8 animate-spin rounded-full border-2 border-[var(--border-default)] border-t-[var(--accent-primary)]" />
                  <p class="mt-4 text-sm font-medium text-body">Đang tải dữ liệu...</p>
                </div>
              </template>
            </Suspense>
          </PageContainer>
        </div>
      </main>
    </div>
  </div>
  <AiAssistant />
</template>

<style>
/* Font and scrollbar are globally managed or in other layouts, but keeping them here for safety */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.font-sans {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

html {
  scroll-behavior: smooth;
}

::-webkit-scrollbar {
  width: 5px;
  height: 5px;
}
::-webkit-scrollbar-track {
  background: transparent;
}
::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 999px;
}
::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
