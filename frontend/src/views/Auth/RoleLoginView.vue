<script setup>
import { computed, nextTick, ref, onMounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ArrowLeft, GraduationCap, ShieldCheck } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { getPortalConfig } from '@/data/authPortals'
import { getHomeRouteByRole } from '@/utils/roleRoutes'
import { resolveSafeRoleRedirect } from '@/utils/authRedirect'
import AuthLoginForm from '@/components/auth/AuthLoginForm.vue'
import RoleLoginHero from '@/components/auth/RoleLoginHero.vue'
import RoleMismatchModal from '@/components/auth/RoleMismatchModal.vue'

/* ── Route enter animation (progressive enhancement) ── */
const pageEnterReady = ref(false)
const pageEnterEnabled = ref(false)
let enterRafId = 0

onMounted(() => {
  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches
  if (reduceMotion) {
    pageEnterReady.value = true
    return
  }
  pageEnterEnabled.value = true
  enterRafId = requestAnimationFrame(() => {
    pageEnterReady.value = true
  })
})

onBeforeUnmount(() => {
  cancelAnimationFrame(enterRafId)
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const loginFormRef = ref(null)

const portal = computed(() => {
  const slug = route.params.portal
  if (typeof slug !== 'string') return null
  return getPortalConfig(slug)
})

const portalValid = computed(() => portal.value !== null)
const portalLabel = computed(() => portal.value?.label || '')

const serverError = ref('')
const showMismatch = ref(false)
const actualRoleLabel = ref('')
const actualRoleDashboard = ref('')

function normalizeRole(role) {
  const normalized = String(role || '').trim().toLowerCase()
  const aliases = {
    lecturer: 'teacher',
    trainingdepartment: 'academicstaff',
    faculty: 'academicstaff',
    academicdepartment: 'academicstaff',
  }

  return aliases[normalized] || normalized
}

function roleMatches(actualRole, expectedRoles) {
  const roles = Array.isArray(expectedRoles) ? expectedRoles : [expectedRoles]
  return roles.some((item) => normalizeRole(actualRole) === normalizeRole(item))
}

function getRoleLabel(role) {
  const map = {
    Student: 'Sinh viên',
    Teacher: 'Giảng viên',
    AcademicStaff: 'Giáo vụ',
    TrainingDepartment: 'Phòng đào tạo',
    Faculty: 'Khoa',
    AcademicDepartment: 'Bộ môn',
    Principal: 'Ban giám hiệu',
    Parent: 'Phụ huynh',
    Admin: 'Quản trị viên',
    SuperAdmin: 'Quản trị viên',
    HoiDongQuanLyNoiDung: 'Hội đồng nội dung',
  }
  return map[role] || role
}

async function handleSubmit(credentials) {
  serverError.value = ''

  try {
    await authStore.login(
      { email: credentials.email, password: credentials.password },
      { remember: credentials.remember },
    )

    const actualRole = authStore.role
    if (!actualRole) {
      serverError.value = 'Không thể xác thực tài khoản.'
      return
    }

    const expectedRoles = portal.value?.allowedRoles || [portal.value?.backendRole || '']
    if (expectedRoles.length && !roleMatches(actualRole, expectedRoles)) {
      actualRoleLabel.value = getRoleLabel(actualRole)
      const dashboard = getHomeRouteByRole(actualRole)
      actualRoleDashboard.value = dashboard || '/'
      showMismatch.value = true
      return
    }

    navigateToDashboard(actualRole)
  } catch (error) {
    serverError.value =
      error?.response?.data?.message ||
      error?.message ||
      authStore.error ||
      'Không thể đăng nhập. Vui lòng kiểm tra lại thông tin tài khoản.'
  }
}

function navigateToDashboard(role) {
  const roleHomeRoute = getHomeRouteByRole(role)

  if (!roleHomeRoute) {
    serverError.value = 'Vai trò tài khoản chưa được hệ thống hỗ trợ.'
    authStore.logout()
    return
  }

  const redirectParam = typeof route.query.redirect === 'string' ? route.query.redirect : ''

  let targetPath = roleHomeRoute

  if (redirectParam) {
    const safeRedirect = resolveSafeRoleRedirect({
      router,
      redirectPath: redirectParam,
      actualRole: role,
    })
    if (safeRedirect) {
      targetPath = safeRedirect
    }
  }

  router.replace(targetPath)
}

function goToActualDashboard() {
  showMismatch.value = false
  if (actualRoleDashboard.value) {
    router.replace(actualRoleDashboard.value)
  }
}

async function useDifferentAccount() {
  showMismatch.value = false
  authStore.logout()
  await nextTick()
  loginFormRef.value?.resetForm()
  loginFormRef.value?.focusUsername()
}
</script>

<template>
  <main
    v-if="portalValid"
    class="liquid-bg min-h-screen flex items-center justify-center p-4 md:p-10 text-[#191c1e] antialiased overflow-x-hidden relative login-page"
    :class="{
      'login-enter-enabled': pageEnterEnabled,
      'login-enter-ready': pageEnterReady,
    }"
  >
    <div class="fixed top-[-10%] left-[-10%] w-[40%] h-[40%] rounded-full bg-[#dce1ff] blur-[120px] opacity-30 pointer-events-none"></div>
    <div class="fixed bottom-[-10%] right-[-10%] w-[40%] h-[40%] rounded-full bg-[#00236f]/20 blur-[120px] opacity-30 pointer-events-none"></div>

    <main class="w-full max-w-[1140px] relative z-10 glass-panel rounded-[28px] overflow-hidden shadow-2xl shadow-[#00236f]/5 flex flex-col lg:flex-row min-h-[600px] h-[calc(100vh-80px)] max-h-[768px]">
      <RoleLoginHero :portal="portal" />

      <div class="w-full lg:w-1/2 p-6 md:p-8 lg:p-8 flex flex-col bg-[#F9FAFC] overflow-y-auto">
        <div class="flex lg:hidden items-center gap-2 text-[#00236f] mb-8">
          <GraduationCap class="w-8 h-8" aria-hidden="true" />
          <div class="flex flex-col">
            <span class="text-[24px] font-semibold leading-tight tracking-[-0.01em]">EduLMS</span>
            <span class="text-[12px] font-medium leading-4 tracking-[0.04em] text-[#585f67]">Cơ sở Đồng Nai</span>
          </div>
        </div>

        <div class="max-w-sm w-full mx-auto space-y-6">
          <div class="mb-1">
            <router-link
              to="/"
              class="inline-flex items-center gap-1.5 text-[13px] font-medium text-[#585f67] hover:text-[#00236f] transition-colors"
            >
              <ArrowLeft class="w-4 h-4" aria-hidden="true" />
              <span>Quay lại chọn cổng truy cập</span>
            </router-link>
          </div>
          
          <div class="space-y-2 text-center lg:text-left">
            <h2 class="text-[24px] font-semibold leading-8 tracking-[-0.01em] text-[#00236f]">
              Chào mừng đến với {{ portal.label }}
            </h2>
            <p class="text-[14px] leading-5 text-[#444651]">
              {{ portal.audience }}
            </p>
          </div>

          <AuthLoginForm
            ref="loginFormRef"
            :portal="portal"
            :loading="authStore.loading"
            :server-error="serverError"
            @submit="handleSubmit"
          />
        </div>

        <div class="mt-8 text-center pb-4 lg:pb-0">
          <p class="text-[13px] text-[#191c1e] font-semibold flex items-center justify-center gap-1">
            <ShieldCheck class="w-4 h-4 text-[#00236f]" aria-hidden="true" />
            Đăng nhập an toàn: Chỉ sử dụng tài khoản được nhà trường cấp.
          </p>
        </div>
      </div>
    </main>

    <RoleMismatchModal
      :show="showMismatch"
      :expected-label="portalLabel"
      :actual-label="actualRoleLabel"
      :dashboard-path="actualRoleDashboard"
      @go-to-dashboard="goToActualDashboard"
      @use-different-account="useDifferentAccount"
    />
  </main>

  <main
    v-else
    class="liquid-bg min-h-screen flex items-center justify-center p-4 antialiased"
  >
    <div class="text-center space-y-4 max-w-sm">
      <p class="text-[15px] text-[#585f67]">Cổng truy cập không hợp lệ.</p>
      <router-link
        to="/"
        class="inline-flex items-center gap-1.5 text-[14px] font-semibold text-[#00236f] hover:underline"
      >
        <ArrowLeft class="w-4 h-4" aria-hidden="true" />
        <span>Về trang chọn cổng</span>
      </router-link>
    </div>
  </main>
</template>

<style scoped>
.liquid-bg {
  background:
    radial-gradient(circle at 0% 0%, #EFF6FF 0%, transparent 50%),
    radial-gradient(circle at 100% 100%, #dce1ff 0%, transparent 50%),
    #f7f9fb;
}

.glass-panel {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.4);
}

/* ═══ Route enter transition (D4.5) ═══ */
.login-page {
  opacity: 1;
  transform: none;
}

.login-enter-enabled:not(.login-enter-ready) {
  opacity: 0;
  transform: translateY(6px);
}

.login-enter-enabled.login-enter-ready {
  opacity: 1;
  transform: translateY(0);
  transition:
    opacity 240ms ease-out,
    transform 240ms cubic-bezier(0.16, 1, 0.3, 1);
}

@media (prefers-reduced-motion: reduce) {
  .login-page,
  .login-enter-enabled:not(.login-enter-ready),
  .login-enter-enabled.login-enter-ready {
    opacity: 1 !important;
    transform: none !important;
    transition: none !important;
  }
}
</style>
