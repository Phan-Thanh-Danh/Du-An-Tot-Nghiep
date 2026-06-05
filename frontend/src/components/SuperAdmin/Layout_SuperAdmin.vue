<script setup>
/**
 * Layout_SuperAdmin.vue
 * ─────────────────────────────────────────────────────────
 * App Shell chính cho toàn bộ giao diện Super Admin.
 * Bao gồm: Sidebar (3 trạng thái), Topbar, Content area.
 *
 * Kế thừa toàn bộ kiến trúc từ Layout_SinhVien.vue.
 * Phân biệt bằng: màu Violet/Purple, badge "SA", menu hệ thống.
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

// ── Route để lấy page title ──────────────────────────────
const route = useRoute()

const pageTitleMap = {
  // 1. Dashboard và Tổng quan
  '/super-admin/dashboard': { title: 'Dashboard Tổng Hệ Thống', subtitle: 'Tổng quan nhanh tình trạng toàn hệ thống' },
  '/super-admin/profile': { title: 'Hồ sơ cá nhân Admin', subtitle: 'Quản lý thông tin cá nhân và lịch sử đăng nhập' },

  // 2. Quản lý Cơ sở (Organization Hierarchy)
  '/super-admin/organizations': { title: 'Quản lý cây tổ chức', subtitle: 'Xem và quản lý cấu trúc Root → Campus → Sub-campus' },
  '/super-admin/organizations/form': { title: 'Thiết lập cơ sở', subtitle: 'Tạo hoặc chỉnh sửa chi tiết thông tin cơ sở' },
  '/super-admin/organizations/lock': { title: 'Khóa/Mở cơ sở', subtitle: 'Tạm ngưng hoạt động của cơ sở mà không xóa dữ liệu' },
  '/super-admin/organizations/admin-roles': { title: 'Phân quyền Admin cơ sở', subtitle: 'Gán vai trò và xác định phạm vi dữ liệu quản lý' },

  // 3. Tài khoản và Phân quyền (RBAC)
  '/super-admin/users': { title: 'Danh sách người dùng', subtitle: 'Quản lý tài khoản mọi vai trò trong hệ thống' },
  '/super-admin/users/import': { title: 'Tạo/Import tài khoản', subtitle: 'Thêm tài khoản thủ công hoặc từ file Excel' },
  '/super-admin/users/lock': { title: 'Khóa/Mở tài khoản', subtitle: 'Xử lý khóa/mở khóa tài khoản kèm lý do' },
  '/super-admin/roles-permissions': { title: 'Vai trò & Quyền hạn', subtitle: 'Thiết lập ma trận quyền hạn theo module và cơ sở' },
  '/super-admin/login-history': { title: 'Lịch sử đăng nhập', subtitle: 'Giám sát phiên đăng nhập và bảo mật' },

  // 4. Quản lý Đào tạo và Học vụ
  '/super-admin/training/semesters': { title: 'Cấu hình học kỳ', subtitle: 'Quản lý trạng thái đóng/mở/khóa học kỳ' },
  '/super-admin/training/programs': { title: 'Cấu trúc chương trình', subtitle: 'Quản lý chương trình đào tạo, môn học và điều kiện tiên quyết' },
  '/super-admin/training/subjects': { title: 'Quản lý môn học', subtitle: 'Cấu hình danh mục môn học và trọng số điểm' },
  '/super-admin/training/courses': { title: 'Quản lý khóa học', subtitle: 'Quản trị nội dung, gán giảng viên và tiến độ' },
  '/super-admin/training/exam-periods': { title: 'Mở/Đóng giai đoạn thi', subtitle: 'Điều phối ca thi và công bố kết quả bài làm' },
  '/super-admin/operations/schedules': { title: 'Thời khóa biểu', subtitle: 'Can thiệp, gán dạy thay hoặc tạo lịch bù' },
  '/super-admin/operations/schedules/approval': { title: 'Duyệt/Publish TKB', subtitle: 'Phê duyệt lịch học trước khi thông báo rộng rãi' },
  '/super-admin/operations/attendance-policy': { title: 'Quỹ vắng & Chuyên cần', subtitle: 'Cấu hình ngưỡng vắng và duyệt mở khóa điểm danh' },
  '/super-admin/operations/registration-periods': { title: 'Mở/Đóng đăng ký môn', subtitle: 'Quản lý các đợt đăng ký môn và chốt danh sách lớp' },
  '/super-admin/operations/pass-fail-rules': { title: 'Điều kiện Pass/Fail', subtitle: 'Thiết lập quy tắc đạt/rớt môn dựa trên điểm và chuyên cần' },

  // 5. Tài chính và Học phí
  '/super-admin/finance/tuition-config': { title: 'Cấu hình học phí', subtitle: 'Thiết lập mức thu theo kỳ, môn học hoặc ngành học' },
  '/super-admin/finance/student-debts': { title: 'Công nợ sinh viên', subtitle: 'Theo dõi hóa đơn, nợ quá hạn và gửi nhắc nợ' },
  '/super-admin/finance/payments': { title: 'Theo dõi thanh toán', subtitle: 'Giám sát giao dịch qua cổng thanh toán và đối soát' },
  '/super-admin/finance/refunds': { title: 'Hoàn phí/Bảo lưu', subtitle: 'Xử lý hoàn trả học phí và bảo lưu' },

  // 6. Hỗ trợ, Đơn từ và Đánh giá
  '/super-admin/support/tickets': { title: 'Ticket hỗ trợ', subtitle: 'Tiếp nhận và xử lý yêu cầu hỗ trợ kỹ thuật/học vụ' },
  '/super-admin/support/faq': { title: 'Quản lý FAQ', subtitle: 'Xây dựng bộ câu hỏi thường gặp trợ giúp tự động' },
  '/super-admin/approvals/requests': { title: 'Đơn cần duyệt', subtitle: 'Giám sát và xử lý đơn từ hành chính trực tuyến' },
  '/super-admin/approvals/history': { title: 'Lịch sử duyệt đơn', subtitle: 'Tra cứu lộ trình xử lý đơn đã kết thúc' },
  '/super-admin/evaluations/config': { title: 'Cấu hình đánh giá GV', subtitle: 'Mở đợt khảo sát và tạo câu hỏi khảo sát giảng viên' },
  '/super-admin/evaluations/results': { title: 'Kết quả đánh giá GV', subtitle: 'Xem báo cáo ẩn danh và phân tích cảm xúc sinh viên' },
  '/super-admin/awards': { title: 'Khen thưởng', subtitle: 'Cấp bằng khen số và quản lý thành tích' },
  '/super-admin/discipline': { title: 'Kỷ luật', subtitle: 'Quản lý hồ sơ và mức độ vi phạm kỷ luật' },

  // 7. Báo cáo và Phân tích (Analytics)
  '/super-admin/reports/education-overview': { title: 'Tổng quan đào tạo', subtitle: 'Báo cáo chất lượng đào tạo chung toàn hệ thống' },
  '/super-admin/reports/learning': { title: 'Báo cáo học tập', subtitle: 'Phân tích GPA, tỷ lệ pass/fail và dự đoán rủi ro rớt môn' },
  '/super-admin/reports/attendance': { title: 'Báo cáo chuyên cần', subtitle: 'Tổng hợp tỷ lệ đi học và các buổi học thiếu chuyên cần' },
  '/super-admin/reports/campus-comparison': { title: 'So sánh cơ sở', subtitle: 'Bảng dashboard so sánh hiệu năng các campus' },
  '/super-admin/reports/export': { title: 'Export dữ liệu', subtitle: 'Xuất file báo cáo phục vụ họp định kỳ' },

  // 8. Trung tâm Thông báo (Notification Hub)
  '/super-admin/notifications/templates': { title: 'Template thông báo', subtitle: 'Quản lý mẫu email/push/SMS' },
  '/super-admin/notifications/send': { title: 'Gửi thông báo toàn hệ thống', subtitle: 'Soạn và gửi thông báo theo campus/lớp/ngành' },
  '/super-admin/notifications/history': { title: 'Lịch sử thông báo', subtitle: 'Xem log gửi và trạng thái nhận của người dùng' },

  // 9. Quản trị Hệ thống, Audit và Bảo mật
  '/super-admin/audit/logs': { title: 'Audit Log', subtitle: 'Theo dõi thay đổi các trường dữ liệu nhạy cảm' },
  '/super-admin/security/alerts': { title: 'Security Alert', subtitle: 'Báo cáo đăng nhập lạ và cảnh báo nguy cơ bảo mật' },
  '/super-admin/system/modules': { title: 'Bật/Tắt module', subtitle: 'Quản lý trạng thái hoạt động của các module' },
  '/super-admin/system/ai-automation': { title: 'AI & Automation', subtitle: 'Quản lý AI, Job tự động và API keys liên kết' },
}

const currentPageMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return { title: 'Quản trị hệ thống', subtitle: 'Không gian làm việc của Super Admin' }
})
</script>

<template>
  <!--
    ═══════════════════════════════════════════════════════
    APP SHELL: Layout_SuperAdmin
    Layout: [Sidebar] | [Topbar + Content]
    Màu: Violet/Purple (#7c3aed) cho Super Admin
    ═══════════════════════════════════════════════════════
  -->
  <div class="lg-app-bg flex h-screen w-full overflow-hidden font-sans">
    <!-- Ambient background orbs - violet/purple theme -->
    <div class="pointer-events-none absolute inset-0 -z-0 overflow-hidden">
      <div class="lg-orb -left-28 top-16 h-80 w-80 bg-violet-300/25 will-change-transform" />
      <div class="lg-orb right-[-8rem] top-[-5rem] h-96 w-96 bg-purple-300/20 [animation-delay:-6s] will-change-transform" />
      <div class="lg-orb bottom-[-9rem] left-1/3 h-96 w-96 bg-indigo-300/18 [animation-delay:-12s] will-change-transform" />
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
    <div class="relative z-10 flex min-w-0 flex-1 flex-col overflow-hidden pt-[72px]">

      <!-- Topbar -->
      <AppTopbar @toggle-sidebar="toggleSidebar" />

      <!-- ═══════════ CONTENT AREA ═══════════ -->
      <main class="flex-1 overflow-y-auto">
        <div class="mx-auto w-full max-w-[1500px] px-3 py-4 sm:px-4">

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
                    <div v-else class="flex flex-col items-center justify-center py-24 text-center">
                      <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-violet-50 dark:bg-violet-600/20">
                        <svg class="h-8 w-8 text-violet-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
                        </svg>
                      </div>
                      <h3 class="mt-4 text-base font-semibold text-slate-700 dark:text-slate-300">Trang đang phát triển</h3>
                      <p class="mt-1.5 text-sm text-slate-400 max-w-xs">Trang <strong>{{ currentPageMeta.title }}</strong> đang được xây dựng.</p>
                      <router-link to="/super-admin/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-700 transition-colors">← Về Dashboard</router-link>
                    </div>
                  </template>
                  <template #fallback>
                    <!-- Fallback skeleton loading - violet theme -->
                    <div class="flex h-[60vh] w-full flex-col items-center justify-center space-y-6">
                      <div class="relative flex items-center justify-center">
                        <div class="absolute h-16 w-16 animate-ping rounded-full bg-violet-400/20"></div>
                        <div class="h-12 w-12 animate-spin rounded-full border-4 border-slate-200 dark:border-slate-700 border-t-violet-600 shadow-sm"></div>
                      </div>
                      <div class="flex flex-col items-center space-y-2">
                        <p class="text-sm font-semibold tracking-wide text-slate-600 dark:text-slate-400">Đang nạp dữ liệu...</p>
                        <div class="flex space-x-1">
                          <div class="h-2 w-2 animate-bounce rounded-full bg-violet-500" style="animation-delay: -0.3s"></div>
                          <div class="h-2 w-2 animate-bounce rounded-full bg-violet-500" style="animation-delay: -0.15s"></div>
                          <div class="h-2 w-2 animate-bounce rounded-full bg-violet-500"></div>
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
/* Import font Inter từ Google Fonts */
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

/* Global: áp dụng font Inter cho toàn bộ layout Super Admin */
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
