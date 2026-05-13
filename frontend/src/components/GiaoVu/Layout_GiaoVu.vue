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
  '/staff/assignments': { title: 'Phân công giảng viên', subtitle: 'Gán giảng viên phụ trách cho các lớp học phần' },
  '/staff/rooms': { title: 'Quản lý phòng học', subtitle: 'Theo dõi sức chứa và thiết bị phòng' },
  '/staff/conflicts': { title: 'Kiểm tra xung đột', subtitle: 'Phát hiện trùng lịch giảng viên và phòng học' },
  '/staff/schedule/pending': { title: 'Lịch chờ duyệt', subtitle: 'Danh sách TKB đang chờ Ban giám hiệu phê duyệt' },
  '/staff/schedule/published': { title: 'Lịch đã công bố', subtitle: 'Thời khóa biểu chính thức đang áp dụng' },
  '/staff/registrations': { title: 'Đợt đăng ký môn học', subtitle: 'Quản lý các đợt đăng ký và cấu hình tín chỉ' },
  '/staff/sections': { title: 'Lớp học phần', subtitle: 'Theo dõi sĩ số và trạng thái các lớp mở trong kỳ' },
  '/staff/registration-list': { title: 'Danh sách đăng ký', subtitle: 'Chi tiết sinh viên đăng ký môn học' },
  '/staff/waitlist': { title: 'Hàng chờ (Waitlist)', subtitle: 'Quản lý sinh viên trong danh sách chờ' },
  '/staff/capacity': { title: 'Điều chỉnh sức chứa', subtitle: 'Thay đổi giới hạn số lượng sinh viên cho lớp' },
  '/staff/course-status': { title: 'Hủy / Mở lớp', subtitle: 'Xử lý các lớp không đủ sĩ số tối thiểu' },
  '/staff/requests': { title: 'Xử lý đơn từ', subtitle: 'Phê duyệt hoặc từ chối các yêu cầu từ sinh viên/giảng viên' },
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
  <div class="flex h-screen w-full overflow-hidden lg-app-bg font-sans relative">
    
    <!-- Mảng trang trí Liquid Glass Background (Blobs) -->
    <div class="pointer-events-none absolute inset-0 z-0 overflow-hidden">
      <div class="lg-blob lg-blob-cyan" style="top: -10%; left: -5%; animation: lg-float 18s infinite;"></div>
      <div class="lg-blob lg-blob-violet" style="top: 20%; right: -10%; animation: lg-float-slow 24s infinite;"></div>
      <div class="lg-blob lg-blob-blue" style="bottom: -15%; left: 30%; animation: lg-float 20s infinite reverse;"></div>
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
    <div class="flex flex-1 flex-col min-w-0 overflow-hidden relative z-10">
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <main class="flex-1 overflow-y-auto">
        <div class="mx-auto max-w-[1440px] px-4 sm:px-6 py-6 min-h-full flex flex-col">
          <router-view v-slot="{ Component }">
            <Transition
              enter-active-class="transition-all duration-200 ease-out"
              enter-from-class="opacity-0 translate-y-2"
              enter-to-class="opacity-100 translate-y-0"
              mode="out-in"
            >
              <component :is="Component" :key="route.fullPath" v-if="Component" />
              <div v-else class="flex-1 flex flex-col items-center justify-center py-24 text-center">
                <div class="flex h-20 w-20 items-center justify-center rounded-3xl bg-teal-50 border border-teal-100 shadow-sm">
                   <ShieldCheck class="h-10 w-10 text-teal-500" />
                </div>
                <h3 class="mt-6 text-xl font-black text-slate-800">Trang đang phát triển</h3>
                <p class="mt-2 text-sm font-medium text-slate-400 max-w-sm">Trang này hiện đang được xây dựng hoặc đường dẫn không tồn tại. Vui lòng quay lại sau.</p>
                <router-link to="/staff/dashboard" class="mt-8 inline-flex items-center gap-2 rounded-2xl bg-teal-600 px-8 py-3 text-sm font-bold text-white hover:bg-teal-700 shadow-lg shadow-teal-500/20 transition-all active:scale-95">← Về Dashboard</router-link>
              </div>
            </Transition>
          </router-view>
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
