<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import {
  ArrowRight,
  BookOpenCheck,
  CalendarCheck2,
  CheckCircle2,
  Eye,
  EyeOff,
  GraduationCap,
  HelpCircle,
  Lock,
  Mail,
  ShieldCheck,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import LmsButton from '@/components/LmsButton.vue'
import LmsAlert from '@/components/LmsAlert.vue'
import LmsCard from '@/components/LmsCard.vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const showPassword = ref(false)
const rememberLogin = ref(authStore.rememberLogin)
const formError = ref('')

const form = reactive({
  email: '',
  password: '',
})

const fieldErrors = reactive({
  email: '',
  password: '',
})

const canSubmit = computed(() => !authStore.loading)

function clearFieldError(field) {
  fieldErrors[field] = ''
  formError.value = ''
}

function validateForm() {
  fieldErrors.email = ''
  fieldErrors.password = ''
  formError.value = ''

  if (!form.email.trim()) {
    fieldErrors.email = 'Vui lòng nhập email hoặc tên đăng nhập.'
  }

  if (!form.password) {
    fieldErrors.password = 'Vui lòng nhập mật khẩu.'
  }

  if (fieldErrors.email || fieldErrors.password) {
    formError.value = 'Vui lòng kiểm tra lại thông tin đăng nhập.'
    return false
  }

  return true
}

async function submitLogin() {
  if (authStore.loading || !validateForm()) return

  try {
    await authStore.login(
      {
        email: form.email.trim(),
        password: form.password,
      },
      {
        remember: rememberLogin.value,
      },
    )
  } catch {
    formError.value = authStore.error || 'Đăng nhập không thành công. Vui lòng thử lại.'
    return
  }

  // Ưu tiên chuyển hướng theo vai trò để tránh lỗi 404 nếu redirect cũ không phù hợp
  let targetPath = typeof route.query.redirect === 'string' ? route.query.redirect : '/'

  // Nếu đang ở trang chủ hoặc trang không thuộc quyền hạn, ép buộc về đúng Dashboard
  if (
    targetPath === '/' ||
    targetPath.startsWith('/student') ||
    targetPath.startsWith('/teacher') ||
    targetPath.startsWith('/staff')
  ) {
    if (authStore.hasRole('Teacher')) targetPath = '/teacher/dashboard'
    else if (authStore.hasRole('AcademicStaff')) targetPath = '/staff/dashboard'
    else targetPath = '/student/dashboard'
  }

  router.replace(targetPath)
}
</script>

<template>
  <main class="lg-login-bg relative min-h-screen overflow-hidden font-sans text-slate-950">
    <!-- Decorative Blobs -->
    <div class="pointer-events-none absolute inset-0">
      <div class="lg-blob lg-blob-cyan absolute -left-24 top-[-120px] lg-float" />
      <div class="lg-blob lg-blob-violet absolute right-[-120px] top-20 lg-float-slow" />
      <div class="lg-blob lg-blob-blue absolute bottom-[-140px] left-1/3 lg-float" style="animation-delay: -6s" />
    </div>

    <!-- Main Grid Layout -->
    <div class="relative grid min-h-screen items-center px-4 py-8 sm:px-6 lg:grid-cols-[1.02fr_0.98fr] lg:px-10 xl:px-16">
      <!-- Left Panel: Branding & Features (Desktop Only) -->
      <section class="lg-glass-dark hidden min-h-[720px] flex-col justify-between overflow-hidden rounded-[2rem] p-9 lg:flex">
        <div class="pointer-events-none absolute inset-y-8 left-10 right-[52%] overflow-hidden rounded-[2rem]">
          <div class="absolute -right-24 top-16 h-64 w-64 rounded-full bg-cyan-400/20 blur-3xl" />
          <div class="absolute bottom-10 left-8 h-80 w-80 rounded-full bg-indigo-500/25 blur-3xl" />
        </div>

        <!-- Brand -->
        <div class="relative flex items-center gap-3">
          <div class="flex h-12 w-12 items-center justify-center rounded-[18px] bg-white text-blue-900 shadow-lg shadow-cyan-950/20">
            <GraduationCap :size="25" :stroke-width="2.2" />
          </div>
          <div>
            <p class="text-base font-bold leading-tight">EduLMS</p>
            <p class="text-xs leading-tight text-slate-300">Academic Management System</p>
          </div>
        </div>

        <!-- Value Proposition -->
        <div class="relative max-w-2xl">
          <div class="mb-5 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/10 px-3 py-1.5 text-xs font-semibold text-cyan-100 backdrop-blur-md">
            <ShieldCheck :size="14" />
            JWT Authentication · Role-based Access
          </div>
          <h1 class="max-w-xl text-[40px] font-extrabold leading-[1.1] tracking-[-0.03em] text-white">
            Học tập, điểm danh và học vụ trong một trải nghiệm tập trung.
          </h1>
          <p class="mt-5 max-w-lg text-base leading-7 text-slate-300">
            Giao diện đăng nhập dành cho hệ thống LMS tốt nghiệp: sạch, tin cậy và tối ưu cho thao tác hằng ngày của sinh viên.
          </p>

          <!-- Feature Cards -->
          <div class="mt-9 grid max-w-xl grid-cols-3 gap-3">
            <div class="lg-glass-soft rounded-[20px] p-4">
              <BookOpenCheck :size="20" class="text-cyan-200" />
              <p class="mt-5 text-2xl font-bold">Course</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Theo dõi khóa học</p>
            </div>
            <div class="lg-glass-soft rounded-[20px] p-4">
              <CalendarCheck2 :size="20" class="text-blue-200" />
              <p class="mt-5 text-2xl font-bold">Attend</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Lịch học, điểm danh</p>
            </div>
            <div class="lg-glass-soft rounded-[20px] p-4">
              <CheckCircle2 :size="20" class="text-violet-200" />
              <p class="mt-5 text-2xl font-bold">Grade</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Kết quả học tập</p>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <p class="relative text-xs text-slate-400">
          Premium academic interface · Vue 3 · ASP.NET Core · SQL Server
        </p>
      </section>

      <!-- Right Panel: Login Form -->
      <section class="flex min-h-[calc(100vh-64px)] items-center justify-center py-6 lg:min-h-screen">
        <div class="login-card w-full max-w-[460px]">
          <!-- Mobile Brand -->
          <div class="mb-6 flex items-center justify-between lg:hidden">
            <div class="flex items-center gap-3">
              <div class="flex h-11 w-11 items-center justify-center rounded-[18px] bg-blue-900 text-white shadow-lg shadow-blue-900/20">
                <GraduationCap :size="23" :stroke-width="2.2" />
              </div>
              <div>
                <p class="text-base font-bold leading-tight text-slate-950">EduLMS</p>
                <p class="text-xs leading-tight text-slate-500">Academic Management System</p>
              </div>
            </div>
          </div>

          <!-- Login Card -->
          <LmsCard variant="glass" padding="1.5rem" class="sm:p-8">
            <!-- Header -->
            <div class="mb-8">
              <div class="mb-5 hidden h-[52px] w-[52px] items-center justify-center rounded-[20px] bg-blue-50 text-blue-900 lg:flex">
                <GraduationCap :size="27" :stroke-width="2.2" />
              </div>
              <p class="text-sm font-semibold text-teal-700">Cổng học tập trực tuyến</p>
              <h2 class="mt-2 text-[34px] font-extrabold leading-[1.12] tracking-[-0.03em]">
                Đăng nhập LMS
              </h2>
              <p class="mt-3 text-sm leading-6 text-slate-500">
                Truy cập hệ thống học tập và quản lý đào tạo
              </p>
            </div>

            <!-- Login Form -->
            <form class="space-y-5" novalidate @submit.prevent="submitLogin">
              <!-- Email Input -->
              <div>
                <label for="login-email" class="mb-2 block text-sm font-semibold text-slate-700">
                  Email hoặc tên đăng nhập
                </label>
                <div
                  :class="[
                    'flex items-center gap-3 rounded-lg border bg-white px-3.5 py-3 transition duration-200',
                    fieldErrors.email
                      ? 'border-red-300 ring-4 ring-red-100'
                      : 'border-slate-200 focus-within:border-blue-500 focus-within:ring-4 focus-within:ring-blue-100',
                  ]"
                >
                  <Mail :size="18" class="shrink-0 text-slate-400" />
                  <input
                    id="login-email"
                    v-model="form.email"
                    type="text"
                    autocomplete="username"
                    class="w-full bg-transparent text-[15px] text-slate-950 outline-none placeholder:text-slate-400"
                    placeholder="student@example.com"
                    aria-describedby="login-email-error"
                    :aria-invalid="Boolean(fieldErrors.email)"
                    @input="clearFieldError('email')"
                  />
                </div>
                <p v-if="fieldErrors.email" id="login-email-error" class="mt-1.5 text-sm text-red-600 font-medium">
                  {{ fieldErrors.email }}
                </p>
              </div>

              <!-- Password Input -->
              <div>
                <label for="login-password" class="mb-2 block text-sm font-semibold text-slate-700">
                  Mật khẩu
                </label>
                <div
                  :class="[
                    'flex items-center gap-3 rounded-lg border bg-white px-3.5 py-3 transition duration-200',
                    fieldErrors.password
                      ? 'border-red-300 ring-4 ring-red-100'
                      : 'border-slate-200 focus-within:border-blue-500 focus-within:ring-4 focus-within:ring-blue-100',
                  ]"
                >
                  <Lock :size="18" class="shrink-0 text-slate-400" />
                  <input
                    id="login-password"
                    v-model="form.password"
                    :type="showPassword ? 'text' : 'password'"
                    autocomplete="current-password"
                    class="w-full bg-transparent text-[15px] text-slate-950 outline-none placeholder:text-slate-400"
                    placeholder="Nhập mật khẩu"
                    aria-describedby="login-password-error"
                    :aria-invalid="Boolean(fieldErrors.password)"
                    @input="clearFieldError('password')"
                  />
                  <button
                    type="button"
                    class="lg-btn-ghost rounded-full p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-700 lg-focus-ring"
                    :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                    @click="showPassword = !showPassword"
                  >
                    <EyeOff v-if="showPassword" :size="18" />
                    <Eye v-else :size="18" />
                  </button>
                </div>
                <p v-if="fieldErrors.password" id="login-password-error" class="mt-1.5 text-sm text-red-600 font-medium">
                  {{ fieldErrors.password }}
                </p>
              </div>

              <!-- Remember & Forgot Password -->
              <div class="flex flex-wrap items-center justify-between gap-3 pt-2">
                <label class="inline-flex cursor-pointer items-center gap-2 text-sm font-medium text-slate-600 hover:text-slate-700">
                  <input
                    v-model="rememberLogin"
                    type="checkbox"
                    class="h-4 w-4 rounded border-slate-300 text-blue-900 focus:ring-blue-500 accent-blue-900"
                  />
                  Ghi nhớ đăng nhập
                </label>
                <RouterLink
                  to="/about"
                  class="inline-flex items-center gap-1 text-sm font-semibold text-blue-900 transition hover:text-violet-600 lg-focus-ring rounded px-1"
                >
                  <HelpCircle :size="15" />
                  Quên mật khẩu?
                </RouterLink>
              </div>

              <!-- Error Alert -->
              <LmsAlert v-if="formError || authStore.error" type="error" closeable>
                {{ formError || authStore.error }}
              </LmsAlert>

              <!-- Submit Button -->
              <LmsButton
                variant="primary"
                size="lg"
                type="submit"
                :loading="authStore.loading"
                :disabled="!canSubmit"
                class="w-full flex justify-center gap-2"
              >
                <span>{{ authStore.loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}</span>
                <ArrowRight v-if="!authStore.loading" :size="18" class="transition" />
              </LmsButton>
            </form>

            <!-- Security Notice -->
            <div class="lg-alert lg-alert-info mt-6 text-sm">
              <p class="font-medium">Lưu ý bảo mật</p>
              <p class="mt-1 text-xs">Tài khoản demo do giảng viên hoặc quản trị viên cấp. Không nhập mật khẩu thật trên máy không tin cậy.</p>
            </div>
          </LmsCard>

          <!-- Footer Text -->
          <p class="mt-5 text-center text-xs text-slate-500">
            LMS tốt nghiệp · giao diện Liquid Glass · không dùng mock login
          </p>
        </div>
      </section>
    </div>
  </main>
</template>

<style scoped>
.login-card {
  animation: lg-enter var(--lg-duration-enter) cubic-bezier(0.16, 1, 0.3, 1) both;
}

@media (prefers-reduced-motion: reduce) {
  .login-card {
    animation: none;
  }
}
</style>
