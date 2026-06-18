<script setup>
/**
 * Layout_SinhVien.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho toàn bộ giao diện Học sinh (Student).
 * Bao gồm: Sidebar (3 trạng thái), Topbar, Content area.
 *
 * Tech stack: Vue 3 + Composition API + TailwindCSS v4
 * Icons: lucide-vue-next
 * ─────────────────────────────────────────────────────────
 */
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from './AppTopbar.vue'
import PageContainer from './PageContainer.vue'
import AiAssistant from '@/components/ui/AiAssistant.vue'
import AnnouncementBanner from '@/components/ui/AnnouncementBanner.vue'

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
const route = useRoute()

// Map route → page title (dùng cho content area)
const pageTitleMap = {
  '/student/dashboard': { title: 'Dashboard', subtitle: 'Chào mừng bạn trở lại! Đây là tổng quan học tập hôm nay.' },
  '/student/courses': { title: 'Khóa học', subtitle: 'Danh sách các môn học đang theo học' },
  '/student/curriculum': { title: 'Khung chương trình', subtitle: 'Theo dõi lộ trình học tập theo kỳ và block' },
  '/student/assignments': { title: 'Bài tập', subtitle: 'Danh sách bài tập và hạn nộp' },
  '/student/exams': { title: 'Thi / Kiểm tra', subtitle: 'Lịch thi và kết quả kiểm tra' },
  '/student/grades': { title: 'Bảng điểm', subtitle: 'Kết quả học tập theo từng học kỳ' },
  '/student/schedule': { title: 'Thời khóa biểu', subtitle: 'Lịch học theo tuần / tháng' },
  '/student/attendance': { title: 'Điểm danh', subtitle: 'Tình trạng điểm danh các môn học' },
  '/student/registrations': { title: 'Đăng ký môn', subtitle: 'Đăng ký môn học theo kỳ' },
  '/student/tuition': { title: 'Học phí & Thanh toán', subtitle: 'Thông tin học phí và lịch sử giao dịch' },
  '/student/support-tickets': { title: 'Hỗ trợ & Ticket', subtitle: 'Gửi yêu cầu hỗ trợ và theo dõi xử lý' },
  '/student/requests': { title: 'Đơn từ', subtitle: 'Quản lý các đơn từ và yêu cầu' },
  '/student/profile': { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin cá nhân và cài đặt tài khoản' },
  '/student/notifications': { title: 'Trung tâm thông báo', subtitle: 'Theo dõi các nhắc nhở, lịch thi, deadline và học phí' },
}

const currentPageMeta = computed(() => {
  // Tìm exact match trước
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  // Tìm prefix match (cho dynamic routes)
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Trang học sinh', subtitle: '' }
})
</script>

<template>
  <!--
    ═══════════════════════════════════════════════════════
    APP SHELL: Layout_SinhVien
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
    <!-- Desktop sidebar (luôn visible, collapsed hoặc expanded) -->
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
            <router-view v-slot="{ Component, route }">
              <Transition
                enter-active-class="transition-all duration-300 ease-out will-change-transform will-change-opacity"
                enter-from-class="opacity-0 translate-y-3"
                enter-to-class="opacity-100 translate-y-0"
                leave-active-class="transition-opacity duration-75 ease-in"
                leave-from-class="opacity-100"
                leave-to-class="opacity-0"
                mode="out-in"
              >
                <component :is="Component" :key="route.path" />
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

/* Global: áp dụng font Inter cho toàn bộ layout học sinh */
.font-sans {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  font-size: 14px;
}

/* Smooth scroll */
html {
  scroll-behavior: smooth;
}

/* Custom scrollbar global (nhỏ và thanh lịch) */
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
