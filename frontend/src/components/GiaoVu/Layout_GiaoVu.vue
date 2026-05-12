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
  '/staff/registrations': { title: 'Đăng ký môn học', subtitle: 'Quản lý các đợt đăng ký và lớp học phần' },
  '/staff/requests': { title: 'Xử lý đơn từ', subtitle: 'Phê duyệt hoặc từ chối các yêu cầu từ sinh viên/giảng viên' },
  '/staff/notices/send': { title: 'Gửi thông báo', subtitle: 'Gửi thông báo học vụ đến sinh viên và giảng viên' },
  '/staff/profile': { title: 'Hồ sơ cá nhân', subtitle: 'Thông tin giáo vụ và cài đặt' },
}

const currentPageMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Trang giáo vụ', subtitle: '' }
})
</script>

<template>
  <div class="flex h-screen w-full overflow-hidden bg-[#F8FAFC] font-sans">

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
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden">
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
                  <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-teal-50">
                     <ShieldCheck class="h-8 w-8 text-teal-400" />
                  </div>
                  <h3 class="mt-4 text-base font-semibold text-slate-700">Trang đang phát triển</h3>
                  <p class="mt-1.5 text-sm text-slate-400 max-w-xs">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng bởi bộ phận kỹ thuật.</p>
                  <router-link to="/staff/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-700 transition-colors">← Về Dashboard</router-link>
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
