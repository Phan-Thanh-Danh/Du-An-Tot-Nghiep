<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowRight,
  Eye,
  EyeOff,
  GraduationCap,
  Lock,
  Mail,
  MessageCircleQuestion,
  ShieldCheck,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import GlassButton from '@/components/ui/GlassButton.vue'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const form = reactive({
  email: '',
  password: '',
})

const rememberLogin = ref(authStore.rememberLogin)
const showPassword = ref(false)
const errorMessage = ref('')
const fieldErrors = reactive({
  email: '',
  password: '',
})

const canSubmit = computed(() => form.email.trim() && form.password && !authStore.loading)

function clearFieldError(field) {
  fieldErrors[field] = ''
  errorMessage.value = ''
}

function validateForm() {
  fieldErrors.email = ''
  fieldErrors.password = ''

  if (!form.email.trim()) {
    fieldErrors.email = 'Vui lòng nhập email hoặc tên đăng nhập.'
  }

  if (!form.password) {
    fieldErrors.password = 'Vui lòng nhập mật khẩu.'
  }

  return !fieldErrors.email && !fieldErrors.password
}

async function submitLogin() {
  errorMessage.value = ''

  if (authStore.loading || !validateForm()) {
    return
  }

  try {
    await authStore.login(
      {
        email: form.email.trim(),
        password: form.password,
      },
      { remember: rememberLogin.value },
    )

    let targetPath = typeof route.query.redirect === 'string' ? route.query.redirect : '/'

    if (
      targetPath === '/' ||
      targetPath.startsWith('/student') ||
      targetPath.startsWith('/teacher') ||
      targetPath.startsWith('/staff') ||
      targetPath.startsWith('/bgh')
    ) {
      if (authStore.hasRole('Principal')) targetPath = '/bgh/dashboard'
      else if (authStore.hasRole('Teacher')) targetPath = '/teacher/dashboard'
      else if (authStore.hasRole('AcademicStaff')) targetPath = '/staff/dashboard'
      else targetPath = '/student/dashboard'
    }

    router.replace(targetPath)
  } catch (error) {
    errorMessage.value =
      error?.response?.data?.message ||
      error?.message ||
      authStore.error ||
      'Không thể đăng nhập. Vui lòng kiểm tra lại thông tin tài khoản.'
  }
}

function quickLogin(roleName) {
  form.email = roleName
  form.password = 'mock_password_no_need'
  submitLogin()
}
</script>

<template>
  <main class="auth-liquid-page relative min-h-screen overflow-hidden font-sans text-slate-950">
    <div class="pointer-events-none absolute inset-0 overflow-hidden" aria-hidden="true">
      <div class="auth-orb auth-orb-cyan" />
      <div class="auth-orb auth-orb-violet" />
      <div class="auth-orb auth-orb-blue" />
    </div>

    <section class="relative z-10 flex min-h-screen items-center justify-center px-4 py-8 sm:px-6">
      <div class="login-card-glass w-full max-w-[440px] p-4 sm:p-8">
        <div class="relative z-[1]">
          <div class="mb-7 text-center">
            <div
              class="login-card-icon mx-auto mb-4 grid h-10 w-10 place-items-center rounded-2xl text-blue-700"
            >
              <GraduationCap class="h-6 w-6" aria-hidden="true" />
            </div>
            <h1 class="text-xl font-bold leading-tight text-slate-950">Đăng nhập LMS</h1>
            <p class="mt-3 text-sm leading-6 text-slate-600">
              Truy cập hệ thống học tập và học vụ của bạn.
            </p>
          </div>

          <form class="space-y-4" novalidate @submit.prevent="submitLogin">
            <div class="space-y-2">
              <label for="login-identity" class="block text-sm font-semibold text-slate-700">
                Email hoặc tên đăng nhập
              </label>
              <div
                :class="[
                  'login-input-shell flex h-12 items-center gap-3 rounded-[18px] border bg-white/75 px-3.5 shadow-sm backdrop-blur-xl transition duration-200',
                  fieldErrors.email
                    ? 'border-red-300 bg-red-50/80 ring-4 ring-red-500/10'
                    : 'border-white/70 focus-within:border-blue-400 focus-within:bg-white/90 focus-within:ring-4 focus-within:ring-blue-500/15',
                ]"
              >
                <Mail class="h-4 w-4 flex-none text-slate-400" aria-hidden="true" />
                <input
                  id="login-identity"
                  v-model="form.email"
                  name="username"
                  type="text"
                  autocomplete="username"
                  required
                  :disabled="authStore.loading"
                  :aria-invalid="Boolean(fieldErrors.email)"
                  :aria-describedby="fieldErrors.email ? 'login-identity-error' : undefined"
                  class="min-w-0 flex-1 bg-transparent text-[15px] text-slate-900 outline-none placeholder:text-slate-400 disabled:cursor-not-allowed"
                  placeholder="Nhập tài khoản học vụ"
                  @input="clearFieldError('email')"
                />
              </div>
              <p
                v-if="fieldErrors.email"
                id="login-identity-error"
                role="alert"
                class="text-sm font-semibold text-red-600"
              >
                {{ fieldErrors.email }}
              </p>
            </div>

            <div class="space-y-2">
              <label for="login-password" class="block text-sm font-semibold text-slate-700">
                Mật khẩu
              </label>
              <div
                :class="[
                  'login-input-shell flex h-12 items-center gap-3 rounded-[18px] border bg-white/75 px-3.5 shadow-sm backdrop-blur-xl transition duration-200',
                  fieldErrors.password
                    ? 'border-red-300 bg-red-50/80 ring-4 ring-red-500/10'
                    : 'border-white/70 focus-within:border-blue-400 focus-within:bg-white/90 focus-within:ring-4 focus-within:ring-blue-500/15',
                ]"
              >
                <Lock class="h-4 w-4 flex-none text-slate-400" aria-hidden="true" />
                <input
                  id="login-password"
                  v-model="form.password"
                  name="password"
                  :type="showPassword ? 'text' : 'password'"
                  autocomplete="current-password"
                  required
                  :disabled="authStore.loading"
                  :aria-invalid="Boolean(fieldErrors.password)"
                  :aria-describedby="fieldErrors.password ? 'login-password-error' : undefined"
                  class="min-w-0 flex-1 bg-transparent text-[15px] text-slate-900 outline-none placeholder:text-slate-400 disabled:cursor-not-allowed"
                  placeholder="Nhập mật khẩu"
                  @input="clearFieldError('password')"
                />
                <button
                  type="button"
                  class="rounded-xl p-1.5 text-slate-500 transition hover:bg-white/80 hover:text-blue-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-blue-500 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                  :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                  :disabled="authStore.loading"
                  @click="showPassword = !showPassword"
                >
                  <EyeOff v-if="showPassword" class="h-4 w-4" aria-hidden="true" />
                  <Eye v-else class="h-4 w-4" aria-hidden="true" />
                </button>
              </div>
              <p
                v-if="fieldErrors.password"
                id="login-password-error"
                role="alert"
                class="text-sm font-semibold text-red-600"
              >
                {{ fieldErrors.password }}
              </p>
            </div>

            <div class="flex items-center justify-between gap-3 text-sm">
              <label class="flex items-center gap-2 font-medium text-slate-600">
                <input
                  v-model="rememberLogin"
                  type="checkbox"
                  class="h-4 w-4 rounded border-slate-300 text-blue-700 focus:ring-blue-500"
                  :disabled="authStore.loading"
                />
                Ghi nhớ đăng nhập
              </label>

              <span class="inline-flex items-center gap-1.5 font-semibold text-blue-700">
                <MessageCircleQuestion class="h-4 w-4" aria-hidden="true" />
                Liên hệ học vụ
              </span>
            </div>

            <div
              v-if="errorMessage"
              id="login-error"
              class="rounded-2xl border border-red-200 bg-red-50/90 px-4 py-3 text-sm font-medium text-red-700"
              role="alert"
            >
              {{ errorMessage }}
            </div>

            <GlassButton
              type="submit"
              size="lg"
              variant="primary"
              block
              :disabled="!canSubmit"
              :loading="authStore.loading"
              :aria-busy="authStore.loading ? 'true' : 'false'"
              class="login-submit-button"
            >
              <span>Đăng nhập</span>
              <template #trailing>
                <ArrowRight class="h-4 w-4" aria-hidden="true" />
              </template>
            </GlassButton>
          </form>

          <!-- Panel Đăng nhập nhanh (Quick Login) -->
          <div class="relative my-4 flex items-center justify-center">
            <div class="absolute inset-0 flex items-center" aria-hidden="true">
              <div class="w-full border-t border-slate-200/50"></div>
            </div>
            <span class="relative bg-white/60 px-3 text-[11px] font-bold uppercase tracking-wider text-slate-500 backdrop-blur-md rounded-full py-0.5">
              Đăng nhập nhanh
            </span>
          </div>

          <div class="grid grid-cols-2 gap-2 mb-4">
            <button
              type="button"
              class="quick-login-btn flex flex-col items-center justify-center p-2.5 rounded-2xl border border-white/60 bg-white/40 hover:bg-white/70 active:scale-95 transition text-center shadow-sm"
              @click="quickLogin('student')"
            >
              <GraduationCap class="h-5 w-5 text-blue-700 mb-1" />
              <span class="text-xs font-bold text-slate-800">Sinh viên</span>
              <span class="text-[9px] text-slate-500">1-Click Login</span>
            </button>
            <button
              type="button"
              class="quick-login-btn flex flex-col items-center justify-center p-2.5 rounded-2xl border border-white/60 bg-white/40 hover:bg-white/70 active:scale-95 transition text-center shadow-sm"
              @click="quickLogin('teacher')"
            >
              <svg class="h-5 w-5 text-emerald-700 mb-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.58 0 8 0m-8 0c1.66 0 3.308.23 4.898.674M12 6.253c1.21-.77 2.898-1.253 4.898-1.253 1.753 0 3.308.477 4.5 1.253v13C20.168 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
              </svg>
              <span class="text-xs font-bold text-slate-800">Giảng viên</span>
              <span class="text-[9px] text-slate-500">1-Click Login</span>
            </button>
            <button
              type="button"
              class="quick-login-btn flex flex-col items-center justify-center p-2.5 rounded-2xl border border-white/60 bg-white/40 hover:bg-white/70 active:scale-95 transition text-center shadow-sm"
              @click="quickLogin('staff')"
            >
              <svg class="h-5 w-5 text-teal-700 mb-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
              </svg>
              <span class="text-xs font-bold text-slate-800">Giáo vụ</span>
              <span class="text-[9px] text-slate-500">1-Click Login</span>
            </button>
            <button
              type="button"
              class="quick-login-btn flex flex-col items-center justify-center p-2.5 rounded-2xl border border-white/60 bg-white/40 hover:bg-white/70 active:scale-95 transition text-center shadow-sm"
              @click="quickLogin('admin')"
            >
              <svg class="h-5 w-5 text-indigo-700 mb-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
              </svg>
              <span class="text-xs font-bold text-slate-800">Super Admin</span>
              <span class="text-[9px] text-slate-500">1-Click Login</span>
            </button>
          </div>

          <div class="login-security-note flex gap-3 rounded-2xl px-4 py-3 text-sm">
            <ShieldCheck class="mt-0.5 h-4 w-4 flex-none text-blue-700" aria-hidden="true" />
            <p class="leading-5 text-slate-600">
              Chỉ đăng nhập trên thiết bị tin cậy. Không chia sẻ tài khoản học vụ.
            </p>
          </div>
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
.auth-liquid-page {
  min-height: 100vh;
  background:
    radial-gradient(circle at 12% 10%, rgba(8, 145, 178, 0.22), transparent 32%),
    radial-gradient(circle at 88% 12%, rgba(124, 58, 237, 0.2), transparent 34%),
    radial-gradient(circle at 50% 100%, rgba(37, 99, 235, 0.14), transparent 36%),
    linear-gradient(135deg, #f8fafc 0%, #eff6ff 44%, #f5f3ff 100%);
}

.auth-orb {
  position: absolute;
  border-radius: 9999px;
  filter: blur(64px);
  opacity: 0.62;
  animation: auth-orb-float 18s ease-in-out infinite alternate;
}

.auth-orb-cyan {
  top: -120px;
  left: -6rem;
  width: 20rem;
  height: 20rem;
  background: rgba(103, 232, 249, 0.3);
}

.auth-orb-violet {
  top: 5rem;
  right: -7rem;
  width: 24rem;
  height: 24rem;
  background: rgba(196, 181, 253, 0.25);
  animation-delay: -6s;
}

.auth-orb-blue {
  bottom: -140px;
  left: 33%;
  width: 20rem;
  height: 20rem;
  background: rgba(147, 197, 253, 0.2);
  animation-delay: -10s;
}

.login-card-glass {
  position: relative;
  overflow: hidden;
  border-radius: 32px;
  background:
    radial-gradient(
      circle at top left,
      rgba(255, 255, 255, 0.94),
      rgba(255, 255, 255, 0.72) 42%,
      rgba(255, 255, 255, 0.58)
    ),
    linear-gradient(135deg, rgba(255, 255, 255, 0.86), rgba(255, 255, 255, 0.64));
  border: 1px solid rgba(255, 255, 255, 0.76);
  box-shadow:
    0 30px 90px rgba(15, 23, 42, 0.16),
    inset 0 1px 0 rgba(255, 255, 255, 0.9),
    inset 0 -1px 0 rgba(255, 255, 255, 0.28);
  backdrop-filter: blur(32px) saturate(180%);
  -webkit-backdrop-filter: blur(32px) saturate(180%);
  animation: login-card-enter 480ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

.login-card-glass::before {
  content: '';
  position: absolute;
  inset: 1px;
  border-radius: inherit;
  pointer-events: none;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.62),
    rgba(255, 255, 255, 0.14) 40%,
    transparent 68%
  );
  opacity: 0.55;
}

.login-card-icon,
.login-input-shell,
.login-security-note {
  border: 1px solid rgba(255, 255, 255, 0.7);
  box-shadow:
    0 10px 24px rgba(15, 23, 42, 0.06),
    inset 0 1px 0 rgba(255, 255, 255, 0.78);
  backdrop-filter: blur(16px) saturate(165%);
  -webkit-backdrop-filter: blur(16px) saturate(165%);
}

.login-card-icon {
  background: rgba(239, 246, 255, 0.8);
}

.login-security-note {
  background: rgba(239, 246, 255, 0.7);
  border-color: rgba(191, 219, 254, 0.7);
}

.login-input-shell:focus-within {
  box-shadow:
    0 0 0 4px rgba(37, 99, 235, 0.15),
    0 12px 28px rgba(37, 99, 235, 0.1),
    inset 0 1px 0 rgba(255, 255, 255, 0.82);
}

.login-submit-button {
  min-height: 3.125rem;
  border-radius: 18px;
  background: linear-gradient(90deg, #1d4ed8 0%, #2563eb 52%, #0891b2 100%);
  color: #ffffff;
  font-weight: 700;
  box-shadow:
    0 18px 40px rgba(37, 99, 235, 0.28),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
}

.login-submit-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 22px 48px rgba(37, 99, 235, 0.34),
    inset 0 1px 0 rgba(255, 255, 255, 0.34);
}

.login-submit-button:active:not(:disabled) {
  transform: scale(0.98);
}

@keyframes login-card-enter {
  from {
    opacity: 0;
    transform: translateY(16px) scale(0.985);
    filter: blur(5px);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
    filter: blur(0);
  }
}

@keyframes auth-orb-float {
  0% {
    transform: translate3d(0, 0, 0) scale(1);
  }
  50% {
    transform: translate3d(18px, -14px, 0) scale(1.04);
  }
  100% {
    transform: translate3d(-10px, 10px, 0) scale(0.98);
  }
}

@supports not ((backdrop-filter: blur(1px)) or (-webkit-backdrop-filter: blur(1px))) {
  .login-card-glass,
  .login-input-shell,
  .login-security-note {
    background: rgba(255, 255, 255, 0.94);
    backdrop-filter: none;
    -webkit-backdrop-filter: none;
  }
}

@media (prefers-reduced-motion: reduce) {
  .login-card-glass,
  .auth-orb,
  .login-submit-button {
    animation: none !important;
    transition: none !important;
  }
}
</style>
