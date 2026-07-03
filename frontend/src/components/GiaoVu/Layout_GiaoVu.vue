<script setup>
/**
 * Layout_GiaoVu.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho giao diện Giáo vụ (Academic Staff).
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
  '/staff/dashboard': { title: 'Tổng quan công việc', subtitle: 'Chào mừng trở lại! Đây là các đầu việc giáo vụ cần xử lý hôm nay.' },
  '/staff/schedule': { title: 'Quản lý Thời khóa biểu', subtitle: 'Sắp xếp lịch học và lịch thi' },
  '/staff/assignments': { title: 'Phân công giảng viên', subtitle: 'Gán giảng viên phụ trách cho các lớp học phần' },
  '/staff/buildings': { title: 'Quản lý tòa nhà', subtitle: 'Quản lý tòa nhà, lầu và phòng học' },
  '/staff/floors': { title: 'Quản lý lầu', subtitle: 'Danh sách lầu và phòng học trực thuộc' },
  '/staff/rooms': { title: 'Quản lý phòng học', subtitle: 'Theo dõi sức chứa và thiết bị phòng' },
  '/staff/conflicts': { title: 'Kiểm tra xung đột', subtitle: 'Phát hiện trùng lịch giảng viên và phòng học' },
  '/staff/schedule/pending': { title: 'Lịch chờ duyệt', subtitle: 'Danh sách TKB đang chờ Ban giám hiệu phê duyệt' },
  '/staff/schedule/published': { title: 'Lịch đã công bố', subtitle: 'Thời khóa biểu chính thức đang áp dụng' },
  '/staff/registrations': { title: 'Đợt đăng ký môn học', subtitle: 'Quản lý các đợt đăng ký và cấu hình tín chỉ' },

  '/staff/capacity': { title: 'Điều chỉnh sức chứa', subtitle: 'Thay đổi giới hạn số lượng sinh viên cho lớp' },
  '/staff/course-status': { title: 'Hủy / Mở lớp', subtitle: 'Xử lý các lớp không đủ sĩ số tối thiểu' },
  '/staff/courses': { title: 'Danh sách khóa học', subtitle: 'Quản lý và phân phối môn học cho giảng viên và lớp hành chính' },
  '/staff/requests': { title: 'Xử lý đơn từ', subtitle: 'Phê duyệt hoặc từ chối các yêu cầu từ sinh viên/giảng viên' },
  '/staff/notifications': { title: 'Thông báo', subtitle: 'Quản lý thông báo học vụ' },
  '/staff/notices/send': { title: 'Gửi thông báo', subtitle: 'Gửi thông báo học vụ đến sinh viên và giảng viên' },
  '/staff/profile': { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin giáo vụ và cài đặt' },
}

const currentPageMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key)) return val
  }
  return { title: 'Hệ thống Giáo vụ', subtitle: '' }
})
</script>

<template>
  <div
    class="lg-app-bg relative flex h-screen w-full overflow-hidden font-sans"
    :style="{
      '--sidebar-accent': '#0d9488',
      '--active-start': '#0f766e',
      '--active-mid': '#0d9488',
      '--active-end': '#14b8a6',
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
    <div class="hidden lg:flex flex-shrink-0 h-full">
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
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden relative z-10 pt-16">
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <AnnouncementBanner />

      <main class="flex-1 overflow-y-auto">
        <div class="lg-shell-content mx-auto flex flex-col">
          <PageContainer
            :title="currentPageMeta.title"
            :subtitle="currentPageMeta.subtitle"
          >
            <router-view />
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
  background: var(--text-placeholder);
}
</style>
