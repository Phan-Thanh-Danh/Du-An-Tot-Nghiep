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
  '/student/assignments': { title: 'Bài tập', subtitle: 'Danh sách bài tập và hạn nộp' },
  '/student/exams': { title: 'Thi / Kiểm tra', subtitle: 'Lịch thi và kết quả kiểm tra' },
  '/student/grades': { title: 'Bảng điểm', subtitle: 'Kết quả học tập theo từng học kỳ' },
  '/student/schedule': { title: 'Thời khóa biểu', subtitle: 'Lịch học theo tuần / tháng' },
  '/student/attendance': { title: 'Điểm danh', subtitle: 'Tình trạng điểm danh các môn học' },
  '/student/registrations': { title: 'Đăng ký môn', subtitle: 'Đăng ký môn học theo kỳ' },
  '/student/tuition': { title: 'Học phí & Thanh toán', subtitle: 'Thông tin học phí và lịch sử giao dịch' },
  '/student/support-tickets': { title: 'Hỗ trợ & Ticket', subtitle: 'Gửi yêu cầu hỗ trợ và theo dõi xử lý' },
  '/student/requests': { title: 'Đơn từ', subtitle: 'Quản lý các đơn từ và yêu cầu' },
  '/student/evaluations': { title: 'Đánh giá giảng viên', subtitle: 'Đánh giá chất lượng giảng dạy' },
  '/student/profile': { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin cá nhân và cài đặt tài khoản' },
  '/student/parent-links': { title: 'Liên kết phụ huynh', subtitle: 'Quản lý quyền truy cập của phụ huynh' },
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
  <div class="flex h-screen w-full overflow-hidden bg-[#F8FAFC] font-sans">

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
    <!-- Desktop sidebar (luôn visible, collapsed hoặc expanded) -->
    <div class="hidden lg:flex flex-shrink-0 h-full">
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
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden">

      <!-- Topbar -->
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <!-- ═══════════ CONTENT AREA ═══════════ -->
      <main class="flex-1 overflow-y-auto">
        <div class="mx-auto max-w-[1440px] px-4 sm:px-6 py-6">

          <!-- Tất cả page đều render qua PageContainer + RouterView -->
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
                <!-- Empty state -->
                <div v-else class="flex flex-col items-center justify-center py-24 text-center">
                  <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-50">
                    <svg class="h-8 w-8 text-blue-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z" />
                    </svg>
                  </div>
                  <h3 class="mt-4 text-base font-semibold text-slate-700">Trang đang phát triển</h3>
                  <p class="mt-1.5 text-sm text-slate-400 max-w-xs">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng.</p>
                  <router-link to="/student/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors">← Về Dashboard</router-link>
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
/* Import font Inter từ Google Fonts */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

/* Global: áp dụng font Inter cho toàn bộ layout học sinh */
.font-sans {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
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
  background: #cbd5e1;
  border-radius: 999px;
}
::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
