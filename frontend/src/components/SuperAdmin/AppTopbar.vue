<script setup>
/**
 * AppTopbar.vue - SuperAdmin
 * ─────────────────────────────────────────────────────────
 * Topbar dành cho Super Admin.
 * Tái sử dụng cùng kiến trúc với Topbar Sinh viên.
 * Màu accent: violet (#7c3aed).
 * ─────────────────────────────────────────────────────────
 */
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { Search, Bell, Menu, ShieldAlert } from 'lucide-vue-next'
import * as LucideIcons from 'lucide-vue-next'
import { useRoute, useRouter } from 'vue-router'
import { mockAdminUser, mockAdminNotifications } from './data/menuData.js'
import { useAuthStore } from '@/stores/auth'
import { useRecentFavoritesStore } from '@/stores/recentFavorites'
import ThemeToggle from '@/components/ui/ThemeToggle.vue'
import CommandPalette from '@/components/ui/CommandPalette.vue'
import Breadcrumbs from '@/components/ui/Breadcrumbs.vue'

const emit = defineEmits(['toggle-sidebar'])
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const recentStore = useRecentFavoritesStore()

const commandPaletteOpen = ref(false)

const profileMenuRef = ref(null)
const notifMenuRef = ref(null)

const notifOpen = ref(false)
const userMenuOpen = ref(false)
const unreadCount = computed(() => mockAdminNotifications.filter((n) => !n.read).length)

// Map route → page meta
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

const currentMeta = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [path, meta] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(`${path}/`)) return meta
  }
  return { title: 'Quản trị hệ thống', subtitle: 'Super Admin Portal' }
})

function toggleNotif() {
  notifOpen.value = !notifOpen.value
  userMenuOpen.value = false
}

function toggleUserMenu() {
  userMenuOpen.value = !userMenuOpen.value
  notifOpen.value = false
}

function closeAll() {
  notifOpen.value = false
  userMenuOpen.value = false
}

function handleClickOutside(event) {
  if (profileMenuRef.value && !profileMenuRef.value.contains(event.target)) {
    userMenuOpen.value = false
  }
  if (notifMenuRef.value && !notifMenuRef.value.contains(event.target)) {
    notifOpen.value = false
  }
}

function handleEscape(event) {
  if (event.key === 'Escape') closeAll()
}

function handleGlobalKeydown(e) {
  if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
    e.preventDefault()
    commandPaletteOpen.value = true
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
  document.addEventListener('keydown', handleEscape)
  document.addEventListener('keydown', handleGlobalKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
  document.removeEventListener('keydown', handleEscape)
  document.removeEventListener('keydown', handleGlobalKeydown)
})

watch(() => route.fullPath, (path) => {
  userMenuOpen.value = false
  notifOpen.value = false
  const title = currentMeta.value?.title
  if (title && path && !path.startsWith('/login')) {
    const icon = guessIcon(path)
    recentStore.visitPage({ path, label: title, icon })
  }
})

function guessIcon(path) {
  if (path.includes('dashboard')) return 'LayoutDashboard'
  if (path.includes('user')) return 'UserCog'
  if (path.includes('role')) return 'ShieldCheck'
  if (path.includes('session') || path.includes('login-history')) return 'History'
  if (path.includes('organization')) return 'Network'
  if (path.includes('training')) return 'GraduationCap'
  if (path.includes('operation') || path.includes('schedule')) return 'CalendarRange'
  if (path.includes('finance')) return 'Wallet'
  if (path.includes('support')) return 'LifeBuoy'
  if (path.includes('report') || path.includes('analytic')) return 'TrendingUp'
  if (path.includes('notification')) return 'Bell'
  if (path.includes('audit')) return 'ScrollText'
  if (path.includes('security')) return 'ShieldAlert'
  if (path.includes('system')) return 'Settings2'
  return 'Circle'
}

function notifColorClass(color) {
  const map = {
    red: 'bg-red-100 dark:bg-red-600/25 text-red-600 dark:text-red-300',
    green: 'bg-green-100 dark:bg-green-600/25 text-green-600 dark:text-green-300',
    blue: 'bg-blue-100 dark:bg-blue-600/25 text-blue-600 dark:text-blue-300',
    yellow: 'bg-yellow-100 dark:bg-yellow-600/25 text-yellow-600 dark:text-yellow-300',
  }
  return map[color] || 'bg-slate-100 dark:bg-slate-600/25 text-slate-600 dark:text-slate-300'
}

function getIcon(name) {
  return LucideIcons[name] || LucideIcons.Bell
}

const logout = () => {
  authStore.logout()
  closeAll()
  router.replace('/login')
}
</script>

<template>
  <header class="lg-topbar absolute top-0 left-0 right-0 z-50 mx-2 mt-2 flex h-14 flex-shrink-0 items-center gap-2 overflow-visible rounded-[22px] px-3 sm:mx-3 sm:mt-3 sm:gap-3 sm:px-4">
    <!-- Mobile toggle -->
    <button
      class="lg-icon-button flex p-2 text-muted hover:text-violet-700 dark:hover:text-violet-300 lg:hidden"
      aria-label="Mở menu"
      @click="emit('toggle-sidebar')"
    >
      <Menu :size="20" />
    </button>

    <!-- Page title + Breadcrumbs -->
    <div class="hidden min-w-0 flex-1 sm:block">
      <Breadcrumbs class="mb-0.5" />
      <div class="flex min-w-0 items-baseline gap-2">
        <h1 class="truncate text-base font-bold leading-tight text-heading">{{ currentMeta.title }}</h1>
        <span class="hidden text-xs font-medium text-muted lg:inline">{{ currentMeta.subtitle }}</span>
      </div>
    </div>

    <!-- Command palette trigger (Cmd+K) -->
    <button
      class="hidden md:inline-flex h-8 items-center gap-1.5 rounded-xl border border-card surface-input px-2.5 text-[10px] font-semibold text-label shadow-sm backdrop-blur-xl hover:bg-(--surface-input-focus) hover:text-heading transition-all focus:outline-none"
      aria-label="Mở command palette"
      @click="commandPaletteOpen = true"
    >
      <Search :size="14" />
      <span class="hidden sm:inline">Tìm kiếm</span>
    </button>

    <!-- Super Admin Badge -->
    <div class="hidden lg:flex items-center gap-1.5 rounded-full border border-violet-200/60 dark:border-violet-500/20 bg-violet-50/80 dark:bg-violet-600/15 px-3 py-1">
      <ShieldAlert :size="12" class="text-violet-600 dark:text-violet-400" />
      <span class="text-[10px] font-bold text-violet-700 dark:text-violet-300">Super Admin</span>
    </div>

    <ThemeToggle />

    <!-- Notification -->
    <div ref="notifMenuRef" class="relative">
      <button
        :class="[
          'lg-icon-button relative h-8 w-8 border border-card surface-input text-muted shadow-sm backdrop-blur-xl focus:outline-none',
          notifOpen ? 'bg-violet-50 dark:bg-violet-600/25 text-violet-700 dark:text-violet-300 shadow-md' : 'hover:text-violet-700 dark:hover:text-violet-300',
        ]"
        aria-label="Thông báo"
        aria-haspopup="menu"
        :aria-expanded="notifOpen"
        @click.stop="toggleNotif"
      >
        <Bell :size="15" />
        <span
          v-if="unreadCount > 0"
          class="absolute -right-0.5 -top-0.5 flex h-3.5 w-3.5 items-center justify-center rounded-full bg-red-500 text-[8px] font-bold text-white ring-2 ring-white dark:ring-slate-800"
        >
          {{ unreadCount }}
        </span>
      </button>

      <!-- Notification dropdown -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-2 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-2 scale-95"
      >
        <div
          v-if="notifOpen"
          class="absolute right-0 top-[calc(100%+0.75rem)] z-[80] w-80 origin-top-right overflow-hidden rounded-[24px] border border-card surface-dropdown p-1 shadow-[0_20px_50px_rgba(0,0,0,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <div class="flex items-center justify-between border-b border-default px-4 py-3">
            <h3 class="text-sm font-bold text-heading">Thông báo hệ thống</h3>
            <span v-if="unreadCount" class="rounded-full bg-violet-100 dark:bg-violet-600/25 px-2 py-0.5 text-[10px] font-bold text-violet-700 dark:text-violet-300">
              {{ unreadCount }} mới
            </span>
          </div>
          <div class="max-h-[320px] divide-y divide-(--border-card) overflow-y-auto" role="none">
            <div
              v-for="notif in mockAdminNotifications"
              :key="notif.id"
              class="flex cursor-pointer gap-3 px-4 py-3 transition-all hover:bg-(--surface-card-hover) active:scale-[0.98]"
              role="menuitem"
              tabindex="0"
              @click="() => { closeAll(); if(notif.link) router.push(notif.link); }"
            >
              <div :class="['flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full text-[10px]', notifColorClass(notif.color)]">
                <component :is="getIcon(notif.icon)" :size="14" />
              </div>
              <div class="min-w-0 flex-1">
                <p class="text-xs font-semibold leading-tight text-heading">{{ notif.title }}</p>
                <p class="mt-0.5 line-clamp-2 text-[11px] font-medium leading-snug text-body">{{ notif.description }}</p>
                <p class="mt-1.5 text-[10px] font-semibold text-muted">{{ notif.time }}</p>
              </div>
            </div>
          </div>
          <div class="border-t border-default px-4 py-2.5 text-center surface-card">
            <button class="text-[11px] font-bold text-violet-600 dark:text-violet-300 hover:text-violet-700 dark:hover:text-violet-200" @click="router.push('/super-admin/audit/logs'); closeAll()">Xem tất cả nhật ký</button>
          </div>
        </div>
      </Transition>
    </div>

    <!-- Profile menu -->
    <div ref="profileMenuRef" class="relative">
      <button
        :class="[
          'flex items-center gap-2 rounded-xl border border-card surface-input p-1 transition-all duration-200 focus:outline-none ring-offset-2 focus:ring-2 focus:ring-violet-500/20',
          userMenuOpen ? 'bg-(--surface-input-focus) shadow-md' : 'hover:bg-(--surface-input-focus)',
        ]"
        aria-haspopup="menu"
        :aria-expanded="userMenuOpen"
        aria-label="Mở hồ sơ"
        @click.stop="toggleUserMenu"
      >
        <div class="flex h-8 w-8 items-center justify-center rounded-full text-[10px] font-bold text-white shadow-sm ring-1 ring-(--border-card) overflow-hidden bg-gradient-to-br from-violet-600 to-purple-600">
          <img v-if="authStore.user?.avatar" :src="authStore.user.avatar" class="h-full w-full object-cover" />
          <span v-else>{{ authStore.initials || mockAdminUser.initials }}</span>
        </div>
        <div class="hidden pr-1.5 text-left sm:block">
          <p class="text-[12px] font-bold leading-tight text-heading">{{ authStore.displayName || mockAdminUser.name }}</p>
          <p class="text-[10px] font-medium text-violet-600 dark:text-violet-400">Super Admin</p>
        </div>
      </button>

      <!-- Profile dropdown -->
      <Transition
        enter-active-class="transition-all duration-200 ease-out"
        enter-from-class="opacity-0 translate-y-2 scale-95"
        enter-to-class="opacity-100 translate-y-0 scale-100"
        leave-active-class="transition-all duration-150 ease-in"
        leave-from-class="opacity-100 translate-y-0 scale-100"
        leave-to-class="opacity-0 translate-y-2 scale-95"
      >
        <div
          v-if="userMenuOpen"
          class="absolute right-0 top-[calc(100%+0.75rem)] z-[80] w-72 origin-top-right overflow-hidden rounded-[24px] border border-card surface-dropdown p-1.5 shadow-[0_20px_50px_rgba(0,0,0,0.15)] dark:shadow-[0_20px_50px_rgba(2,6,23,0.4)] backdrop-blur-2xl"
          role="menu"
          @click.stop
        >
          <div class="border-b border-default px-4 py-3.5 surface-card">
            <p class="text-[13px] font-bold text-heading">{{ authStore.displayName || mockAdminUser.name }}</p>
            <p class="mt-0.5 truncate text-[11px] font-medium text-muted">{{ authStore.user?.email || mockAdminUser.email }}</p>
            <span class="mt-2.5 inline-flex items-center gap-1.5 rounded-full bg-violet-50 dark:bg-violet-600/25 px-2.5 py-1 text-[10px] font-semibold text-violet-700 dark:text-violet-300 border border-violet-100 dark:border-violet-500/30 shadow-sm">
              <ShieldAlert :size="11" />
              Super Admin · {{ mockAdminUser.campus }}
            </span>
          </div>

          <div class="p-1 space-y-0.5">
            <router-link
              to="/super-admin/profile"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-label transition-all hover:bg-(--surface-card-hover) hover:text-violet-700 dark:hover:text-violet-300 hover:shadow-sm"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.UserCircle :size="16" class="text-muted" />
              Hồ sơ quản trị viên
            </router-link>
            <router-link
              to="/super-admin/system/ai-automation"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-label transition-all hover:bg-(--surface-card-hover) hover:text-violet-700 dark:hover:text-violet-300 hover:shadow-sm"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.Cpu :size="16" class="text-muted" />
              AI & Automation
            </router-link>
            <router-link
              to="/super-admin/audit/logs"
              class="flex items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-label transition-all hover:bg-(--surface-card-hover) hover:text-violet-700 dark:hover:text-violet-300 hover:shadow-sm"
              role="menuitem"
              @click="closeAll"
            >
              <LucideIcons.ScrollText :size="16" class="text-muted" />
              Audit Logs
            </router-link>
          </div>

          <div class="border-t border-default p-1">
            <button
              class="flex w-full items-center gap-2.5 rounded-xl px-3 py-2.5 text-[12px] font-bold text-red-600 dark:text-red-400 transition-all hover:bg-red-50 dark:hover:bg-red-600/20 hover:text-red-700 dark:hover:text-red-300"
              role="menuitem"
              @click="logout"
            >
              <LucideIcons.LogOut :size="16" />
              Đăng xuất
            </button>
          </div>
        </div>
      </Transition>
    </div>
  </header>

  <CommandPalette v-if="commandPaletteOpen" @close="commandPaletteOpen = false" />
</template>
