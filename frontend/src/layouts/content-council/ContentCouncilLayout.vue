<script setup>
import { ref } from 'vue'
import ContentCouncilSidebar from '@/components/content-council/layout/ContentCouncilSidebar.vue'
import ContentCouncilHeader from '@/components/content-council/layout/ContentCouncilHeader.vue'

const isSidebarCollapsed = ref(false)
const isMobileDrawerOpen = ref(false)
</script>

<template>
  <div class="lg-app-bg relative flex h-screen w-full overflow-hidden font-sans text-slate-900">
    
    <!-- Mobile Overlay -->
    <Transition
      enter-active-class="transition-opacity duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-200"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div 
        v-if="isMobileDrawerOpen" 
        class="fixed inset-0 bg-slate-900/50 z-40 md:hidden"
        @click="isMobileDrawerOpen = false"
      ></div>
    </Transition>

    <!-- Sidebar Desktop -->
    <div class="hidden md:flex flex-shrink-0 h-full relative z-20">
      <ContentCouncilSidebar 
        v-model:isCollapsed="isSidebarCollapsed" 
        v-model:isMobileDrawerOpen="isMobileDrawerOpen" 
      />
    </div>

    <!-- Sidebar Mobile -->
    <Transition
      enter-active-class="transition-transform duration-300 ease-out"
      enter-from-class="-translate-x-full"
      enter-to-class="translate-x-0"
      leave-active-class="transition-transform duration-200 ease-in"
      leave-from-class="translate-x-0"
      leave-to-class="-translate-x-full"
    >
      <div
        v-if="isMobileDrawerOpen"
        class="fixed inset-y-0 left-0 z-50 flex md:hidden"
      >
        <ContentCouncilSidebar 
          v-model:isCollapsed="isSidebarCollapsed" 
          v-model:isMobileDrawerOpen="isMobileDrawerOpen" 
        />
      </div>
    </Transition>

    <!-- Main Content Area -->
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden relative z-10 pt-16">
      <!-- Header -->
      <ContentCouncilHeader 
        v-model:isMobileDrawerOpen="isMobileDrawerOpen" 
      />

      <!-- Page Content -->
      <main class="flex-1 overflow-y-auto lg-shell-content mx-auto w-full">
        <div class="mx-auto max-w-7xl px-4 md:px-6">
          <router-view v-slot="{ Component }">
            <transition name="fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </div>
      </main>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
