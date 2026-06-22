<script setup>
/**
 * Layout_PhuHuynh.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho toàn bộ giao diện Phụ huynh (Parent).
 * Bao gồm: Sidebar, Topbar, Content area.
 *
 * Tech stack: Vue 3 + Composition API + TailwindCSS v4
 * Icons: lucide-vue-next
 * ─────────────────────────────────────────────────────────
 */
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from './AppTopbar.vue'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import AiAssistant from '@/components/ui/AiAssistant.vue'
import AnnouncementBanner from '@/components/ui/AnnouncementBanner.vue'

const route = useRoute()

// ── Sidebar state ──────────────────────────────────────────
const sidebarCollapsed = ref(false)
const mobileSidebarOpen = ref(false)

// Detect màn hình nhỏ để auto-collapse
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

// ── Route để lấy page title / xác định trang ──────────────

// Map route → page title (dùng cho content area)
const pageTitleMap = {
  '/parent/dashboard': { title: 'Dashboard Phụ huynh', subtitle: 'Chào mừng bạn trở lại! Đây là tổng quan học tập và tài chính của con.' },
  '/parent/children/list': { title: 'Danh sách học sinh', subtitle: 'Thông tin cơ bản các con đang theo học' },
  '/parent/children/overview': { title: 'Tổng quan học tập', subtitle: 'Chi tiết quá trình rèn luyện học tập của con' },
  '/parent/learning/grades': { title: 'Bảng điểm', subtitle: 'Tra cứu điểm số các kỳ học của con' },
  '/parent/learning/attendance': { title: 'Điểm danh', subtitle: 'Thống kê buổi học và tình trạng chuyên cần' },
  '/parent/learning/schedule': { title: 'Thời khóa biểu', subtitle: 'Lịch lên lớp và lịch thi của con' },
  '/parent/learning/alerts': { title: 'Cảnh báo học tập', subtitle: 'Thông tin cảnh báo kết quả học tập và chuyên cần' },
  '/parent/finance/tuition': { title: 'Công nợ học phí', subtitle: 'Thông tin số dư, các đợt đóng phí cần hoàn thành' },
  '/parent/finance/payment': { title: 'Thanh toán học phí', subtitle: 'Thực hiện đóng học phí trực tuyến an toàn' },
  '/parent/finance/transactions': { title: 'Lịch sử giao dịch', subtitle: 'Danh sách các lần thanh toán học phí thành công' },
  '/parent/finance/invoices': { title: 'Hóa đơn học phí', subtitle: 'Hóa đơn giá trị gia tăng các khoản phí đã thanh toán' },
  '/parent/notifications/system': { title: 'Cảnh báo hệ thống', subtitle: 'Thông báo nhắc nhở tự động' },
  '/parent/notifications/history': { title: 'Lịch sử thông báo', subtitle: 'Các bản tin thông báo từ nhà trường' },
  '/parent/profile/info': { title: 'Hồ sơ phụ huynh', subtitle: 'Quản lý thông tin liên hệ và tài khoản phụ huynh' },
  '/parent/profile/access-rights': { title: 'Quyền truy cập được cấp', subtitle: 'Xem các quyền xem thông tin được phân công' },
}

const currentPageMeta = computed(() => {
  // Tìm exact match trước
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  // Tìm prefix match (cho dynamic routes)
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Trang Phụ huynh', subtitle: 'Không gian giám sát học tập' }
})
</script>

<template>
  <!--
    ═══════════════════════════════════════════════════════
    APP SHELL: Layout_PhuHuynh
    Layout: [Sidebar] | [Topbar + Content]
    ═══════════════════════════════════════════════════════
  -->
  <div
    class="lg-app-bg flex h-screen w-full overflow-hidden font-sans"
    :style="{
      '--sidebar-accent': '#ea580c',
      '--sidebar-accent-dark': '#f97316',
      '--active-start': '#c2410c',
      '--active-mid': '#ea580c',
      '--active-end': '#fb923c',
    }"
  >
    <!-- Background Orbs - Orange themed for parent -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb bg-orange-500/10 dark:bg-orange-500/5 top-[-10%] left-[-10%] w-[45rem] h-[45rem] rounded-full blur-[120px] absolute pointer-events-none" />
      <div class="lg-shell-orb bg-amber-500/10 dark:bg-amber-500/5 bottom-[-10%] right-[-10%] w-[40rem] h-[40rem] rounded-full blur-[100px] absolute pointer-events-none" />
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

    <!-- Mobile sidebar (dạng drawer) -->
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

      <!-- Announcements appear just below the fixed topbar -->
      <AnnouncementBanner />

      <!-- ═══════════ CONTENT AREA ═══════════ -->
      <main class="flex-1 overflow-y-auto">
        <div class="lg-shell-content mx-auto">
          <!-- Tất cả page đều render qua PageContainer + RouterView -->
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
                        <svg class="h-7 w-7 text-orange-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" />
                        </svg>
                      </div>
                      <h3 class="mt-4 text-base font-semibold text-heading">Trang đang phát triển</h3>
                      <p class="mt-1.5 max-w-xs text-sm text-body">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng.</p>
                      <router-link to="/parent/dashboard" class="lg-button-primary mt-5 inline-flex items-center gap-2 px-4 py-2 text-sm font-medium bg-orange-600 hover:bg-orange-700 text-white">← Về Dashboard</router-link>
                    </div>
                  </template>
                  <template #fallback>
                    <!-- Fallback skeleton loading -->
                    <div class="lg-shell-loading gap-3">
                      <div class="lg-shell-loading-spinner border-orange-600" />
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
/* Import font Inter từ Google Fonts */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.font-sans {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  font-size: 14px;
}

html {
  scroll-behavior: smooth;
}

/* Custom scrollbar */
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
  background: var(--text-placeholder);
}
</style>
