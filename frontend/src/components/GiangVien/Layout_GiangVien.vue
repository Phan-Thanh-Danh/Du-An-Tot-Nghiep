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
  <div
    class="lg-app-bg flex h-screen w-full overflow-hidden font-sans"
    :style="{
      '--sidebar-accent': '#2563eb',
      '--active-start': '#1d4ed8',
      '--active-mid': '#2563eb',
      '--active-end': '#0891b2',
    }"
  >
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary" />
      <div class="lg-shell-orb lg-shell-orb-secondary" />
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
        class="lg-mobile-scrim fixed inset-0 z-40 lg:hidden"
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
    <div class="relative z-10 flex min-w-0 flex-1 flex-col overflow-hidden pt-16">
      <!-- Topbar -->
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <AnnouncementBanner />

      <!-- ═══════════ CONTENT AREA ═══════════ -->
      <main class="flex-1 overflow-y-auto">
        <div class="lg-shell-content mx-auto">
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
                    <div v-else class="lg-shell-empty">
                      <div class="surface-input border-card flex h-12 w-12 items-center justify-center rounded-[var(--radius-lg)] border">
                        <GraduationCap class="h-7 w-7 text-link" />
                      </div>
                      <h3 class="mt-4 text-base font-semibold text-heading">Trang đang phát triển</h3>
                      <p class="mt-1.5 max-w-xs text-sm text-body">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng.</p>
                      <router-link to="/teacher/dashboard" class="lg-button-primary mt-5 inline-flex items-center gap-2 px-4 py-2 text-sm font-medium">← Về Dashboard</router-link>
                    </div>
                  </template>
                  <template #fallback>
                    <!-- Fallback skeleton loading -->
                    <div class="lg-shell-loading gap-3">
                      <div class="lg-shell-loading-spinner" />
                      <p class="text-sm font-semibold text-label">Đang nạp dữ liệu...</p>
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
  background: var(--border-default);
  border-radius: 999px;
}
::-webkit-scrollbar-thumb:hover {
  background: var(--text-muted);
}
</style>
