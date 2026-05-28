<script setup>
/**
 * Layout_GiangVien.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho giao diện Giảng viên (Teacher).
 */
import { ref, computed, onMounted, onUnmounted } from 'vue'
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

const pageTitleMap = {
  '/teacher/dashboard': { title: 'Tổng quan giảng dạy', subtitle: 'Chào mừng TS. Khoa! Đây là báo cáo công việc giảng dạy của bạn.' },
  '/teacher/classes': { title: 'Lớp học của tôi', subtitle: 'Quản lý các lớp học đang phụ trách' },
  '/teacher/assignments': { title: 'Quản lý bài tập', subtitle: 'Tạo và quản lý bài tập cho sinh viên' },
  '/teacher/grading': { title: 'Chấm bài', subtitle: 'Danh sách bài nộp cần chấm điểm' },
  '/teacher/schedule': { title: 'Thời khóa biểu dạy', subtitle: 'Lịch giảng dạy chi tiết theo tuần' },
  '/teacher/attendance': { title: 'Điểm danh', subtitle: 'Ghi nhận điểm danh sinh viên trong lớp' },
  '/teacher/profile': { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin giảng viên và cài đặt' },
}

const currentPageMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Trang giảng viên', subtitle: '' }
})
</script>

<template>
  <!--
    ═══════════════════════════════════════════════════════
    APP SHELL: Layout_GiangVien
    Layout: [Sidebar] | [Topbar + Content]
    ═══════════════════════════════════════════════════════
  -->
  <div class="lg-app-bg flex h-screen w-full overflow-hidden font-sans">
    <div class="pointer-events-none absolute inset-0 -z-0 overflow-hidden">
      <div class="lg-orb -left-28 top-16 h-80 w-80 bg-cyan-300/30 will-change-transform" />
      <div class="lg-orb right-[-8rem] top-[-5rem] h-96 w-96 bg-violet-300/24 [animation-delay:-6s] will-change-transform" />
      <div class="lg-orb bottom-[-9rem] left-1/3 h-96 w-96 bg-blue-300/20 [animation-delay:-12s] will-change-transform" />
    </div>

    <!-- ═══════════ MOBILE OVERLAY ═══════════ -->
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

    <!-- ═══════════ SIDEBAR ═══════════ -->
    <!-- Desktop sidebar -->
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

    <!-- ═══════════ MAIN AREA (Topbar + Content) ═══════════ -->
    <div class="relative z-10 flex min-w-0 flex-1 flex-col overflow-hidden pt-[72px]">
      <!-- Topbar -->
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <AnnouncementBanner />

      <!-- ═══════════ CONTENT AREA ═══════════ -->
      <main class="flex-1 overflow-y-auto">
        <div class="mx-auto w-full max-w-[1500px] px-3 py-4 sm:px-4">
          <PageContainer
            :title="currentPageMeta.title"
            :subtitle="currentPageMeta.subtitle"
          >
            <router-view v-slot="{ Component }">
              <Transition
                enter-active-class="transition-all duration-300 ease-out will-change-transform will-change-opacity"
                enter-from-class="opacity-0 translate-y-3"
                enter-to-class="opacity-100 translate-y-0"
                leave-active-class="transition-opacity duration-75 ease-in"
                leave-from-class="opacity-100"
                leave-to-class="opacity-0"
                mode="out-in"
              >
                <Suspense timeout="0">
                  <template #default>
                    <component :is="Component" v-if="Component" />
                    <!-- Empty state -->
                    <div v-else class="flex flex-col items-center justify-center py-24 text-center">
                      <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-50">
                        <GraduationCap class="h-8 w-8 text-blue-300" />
                      </div>
                      <h3 class="mt-4 text-base font-semibold text-slate-700">Trang đang phát triển</h3>
                      <p class="mt-1.5 text-sm text-slate-400 max-w-xs">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng.</p>
                      <router-link to="/teacher/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors">← Về Dashboard</router-link>
                    </div>
                  </template>
                  <template #fallback>
                    <!-- Fallback skeleton loading -->
                    <div class="flex h-[60vh] w-full flex-col items-center justify-center space-y-6">
                      <div class="relative flex items-center justify-center">
                        <div class="absolute h-16 w-16 animate-ping rounded-full bg-blue-400/20"></div>
                        <div class="h-12 w-12 animate-spin rounded-full border-4 border-slate-200 border-t-blue-600 shadow-sm"></div>
                      </div>
                      <div class="flex flex-col items-center space-y-2">
                        <p class="text-sm font-semibold tracking-wide text-slate-600">Đang nạp dữ liệu...</p>
                        <div class="flex space-x-1">
                          <div class="h-2 w-2 animate-bounce rounded-full bg-blue-500" style="animation-delay: -0.3s"></div>
                          <div class="h-2 w-2 animate-bounce rounded-full bg-blue-500" style="animation-delay: -0.15s"></div>
                          <div class="h-2 w-2 animate-bounce rounded-full bg-blue-500"></div>
                        </div>
                      </div>
                    </div>
                  </template>
                </Suspense>
              </Transition>
            </router-view>
          </PageContainer>
        </div>
      </main>
    </div>
  </div>
  <AiAssistant />
</template>

<style>
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
