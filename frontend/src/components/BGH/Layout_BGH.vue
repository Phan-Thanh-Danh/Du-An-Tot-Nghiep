<script setup>
/**
 * Layout_BGH.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho giao diện Hiệu trưởng / Ban giám hiệu (BGH).
 */
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from '../SinhVien/AppTopbar.vue'
import PageContainer from '../SinhVien/PageContainer.vue'
import { GraduationCap } from 'lucide-vue-next'

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

const pageTitleMap = {
  '/bgh/dashboard': { title: 'Dashboard chiến lược', subtitle: 'Tổng quan hệ thống đào tạo, chất lượng và thống kê' },
  '/bgh/schedule/pending': { title: 'TKB chờ duyệt', subtitle: 'Phê duyệt thời khóa biểu trước khi công bố' },
  '/bgh/academic/overview': { title: 'Tổng quan học tập', subtitle: 'Báo cáo điểm số, sinh viên và cảnh báo học vụ' },
  '/bgh/evaluations/overview': { title: 'Đánh giá giảng viên', subtitle: 'Theo dõi phản hồi và chất lượng giảng dạy' },
  '/bgh/strategic/dashboard': { title: 'Báo cáo chiến lược', subtitle: 'Báo cáo chuyên sâu theo học kỳ và cơ sở' },
}

const currentPageMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Ban giám hiệu', subtitle: 'Hệ thống quản lý LMS' }
})
</script>

<template>
  <div class="flex h-screen w-full overflow-hidden lg-app-bg font-sans">
    
    <!-- Mảng trang trí Liquid Glass Background (Blobs) for BGH (Indigo/Purple focused) -->
    <div class="pointer-events-none absolute inset-0 z-0 overflow-hidden">
      <div class="lg-blob lg-blob-violet" style="top: -10%; left: -5%; animation: lg-float 18s infinite;"></div>
      <div class="lg-blob lg-blob-blue" style="top: 20%; right: -10%; animation: lg-float-slow 24s infinite;"></div>
      <div class="lg-blob bg-indigo-500/10 blur-[80px]" style="height: 24rem; width: 24rem; bottom: -15%; left: 30%; border-radius: 50%; animation: lg-float 20s infinite reverse; position: absolute;"></div>
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
        class="fixed inset-0 z-40 bg-slate-900/40 backdrop-blur-sm lg:hidden"
        @click="closeMobileSidebar"
      />
    </Transition>

    <!-- SIDEBAR -->
    <div class="hidden lg:flex flex-shrink-0 h-full relative z-10">
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
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden relative z-0">
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <main class="flex-1 overflow-y-auto">
        <div class="mx-auto max-w-[1440px] px-4 sm:px-6 py-6">
          <PageContainer
            :title="currentPageMeta.title"
            :subtitle="currentPageMeta.subtitle"
          >
            <router-view v-slot="{ Component }">
              <Transition
                enter-active-class="transition-all duration-200 ease-out"
                enter-from-class="opacity-0 translate-y-2"
                enter-to-class="opacity-100 translate-y-0"
                mode="out-in"
              >
                <component :is="Component" v-if="Component" />
                <div v-else class="flex flex-col items-center justify-center py-24 text-center">
                  <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-indigo-50 border border-indigo-100">
                     <GraduationCap class="h-8 w-8 text-indigo-500" />
                  </div>
                  <h3 class="mt-4 text-base font-semibold text-slate-700">Trang đang phát triển</h3>
                  <p class="mt-1.5 text-sm text-slate-400 max-w-xs">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng bởi bộ phận kỹ thuật.</p>
                  <router-link to="/bgh/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700 transition-colors">← Về Dashboard</router-link>
                </div>
              </Transition>
            </router-view>
          </PageContainer>
        </div>
      </main>
    </div>
  </div>
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
