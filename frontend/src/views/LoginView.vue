<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import {
  AlertCircle,
  ArrowRight,
  BookOpenCheck,
  CalendarCheck2,
  CheckCircle2,
  Eye,
  EyeOff,
  GraduationCap,
  HelpCircle,
  LoaderCircle,
  Lock,
  Mail,
  ShieldCheck,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

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

  const redirectPath = typeof route.query.redirect === 'string' ? route.query.redirect : '/student/dashboard'
  router.replace(redirectPath)
}
</script>

<template>
  <main class="relative min-h-screen overflow-hidden bg-[#F8FAFC] font-sans text-slate-950">
    <div class="pointer-events-none absolute inset-0">
      <div class="blob blob-navy absolute -left-24 top-[-120px] h-80 w-80 rounded-full bg-blue-300/35 blur-3xl" />
      <div class="blob blob-cyan absolute right-[-120px] top-20 h-96 w-96 rounded-full bg-cyan-200/45 blur-3xl" />
      <div class="blob blob-violet absolute bottom-[-140px] left-1/2 h-96 w-96 rounded-full bg-indigo-200/50 blur-3xl" />
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(30,58,138,0.10),transparent_34%),linear-gradient(135deg,#F8FAFC_0%,#EEF6FF_46%,#F7F4FF_100%)]" />
    </div>

    <div class="relative grid min-h-screen items-center px-4 py-8 sm:px-6 lg:grid-cols-[1.02fr_0.98fr] lg:px-10 xl:px-16">
      <section class="hidden min-h-[720px] flex-col justify-between overflow-hidden rounded-[28px] border border-white/40 bg-[#0F172A] p-9 text-white shadow-[0_28px_90px_rgba(15,23,42,0.30)] lg:flex">
        <div class="pointer-events-none absolute inset-y-8 left-10 right-[52%] overflow-hidden rounded-[28px]">
          <div class="absolute -right-24 top-16 h-64 w-64 rounded-full bg-cyan-400/20 blur-3xl" />
          <div class="absolute bottom-10 left-8 h-80 w-80 rounded-full bg-indigo-500/25 blur-3xl" />
        </div>

        <div class="relative flex items-center gap-3">
          <div class="flex h-12 w-12 items-center justify-center rounded-[18px] bg-white text-[#1E3A8A] shadow-lg shadow-cyan-950/20">
            <GraduationCap :size="25" :stroke-width="2.2" />
          </div>
          <div>
            <p class="text-base font-bold leading-tight">EduLMS</p>
            <p class="text-xs leading-tight text-slate-300">Academic Management System</p>
          </div>
        </div>

        <div class="relative max-w-2xl">
          <div class="mb-5 inline-flex items-center gap-2 rounded-full border border-white/10 bg-white/10 px-3 py-1.5 text-xs font-semibold text-cyan-100 backdrop-blur">
            <ShieldCheck :size="14" />
            JWT Authentication · Role-based Access
          </div>
          <h1 class="max-w-xl text-[40px] font-extrabold leading-[1.1] tracking-[-0.03em] text-white">
            Học tập, điểm danh và học vụ trong một trải nghiệm tập trung.
          </h1>
          <p class="mt-5 max-w-lg text-base leading-7 text-slate-300">
            Giao diện đăng nhập dành cho hệ thống LMS tốt nghiệp: sạch, tin cậy và tối ưu cho thao tác hằng ngày của sinh viên.
          </p>

          <div class="mt-9 grid max-w-xl grid-cols-3 gap-3">
            <div class="rounded-[20px] border border-white/10 bg-white/[0.08] p-4 backdrop-blur">
              <BookOpenCheck :size="20" class="text-cyan-200" />
              <p class="mt-5 text-2xl font-bold">Course</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Theo dõi khóa học</p>
            </div>
            <div class="rounded-[20px] border border-white/10 bg-white/[0.08] p-4 backdrop-blur">
              <CalendarCheck2 :size="20" class="text-blue-200" />
              <p class="mt-5 text-2xl font-bold">Attend</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Lịch học, điểm danh</p>
            </div>
            <div class="rounded-[20px] border border-white/10 bg-white/[0.08] p-4 backdrop-blur">
              <CheckCircle2 :size="20" class="text-violet-200" />
              <p class="mt-5 text-2xl font-bold">Grade</p>
              <p class="mt-1 text-xs leading-5 text-slate-300">Kết quả học tập</p>
            </div>
          </div>
        </div>

        <p class="relative text-xs text-slate-400">
          Premium academic interface · Vue 3 · ASP.NET Core · SQL Server
        </p>
      </section>

      <section class="flex min-h-[calc(100vh-64px)] items-center justify-center py-6 lg:min-h-screen">
        <div class="login-card w-full max-w-[460px]">
          <div class="mb-6 flex items-center justify-between lg:hidden">
            <div class="flex items-center gap-3">
              <div class="flex h-11 w-11 items-center justify-center rounded-[18px] bg-[#1E3A8A] text-white shadow-lg shadow-blue-900/20">
                <GraduationCap :size="23" :stroke-width="2.2" />
              </div>
              <div>
                <p class="text-base font-bold leading-tight text-slate-950">EduLMS</p>
                <p class="text-xs leading-tight text-slate-500">Academic Management System</p>
              </div>
            </div>
          </div>

          <div class="rounded-[28px] border border-white/65 bg-white/82 p-6 shadow-[0_24px_80px_rgba(15,23,42,0.14)] backdrop-blur-xl sm:p-8">
            <div class="mb-8">
              <div class="mb-5 hidden h-[52px] w-[52px] items-center justify-center rounded-[20px] bg-[#DBEAFE] text-[#1E3A8A] lg:flex">
                <GraduationCap :size="27" :stroke-width="2.2" />
              </div>
              <p class="text-sm font-semibold text-[#0F766E]">Cổng học tập trực tuyến</p>
              <h2 class="mt-2 text-[34px] font-extrabold leading-[1.12] tracking-[-0.03em] text-slate-950">
                Đăng nhập LMS
              </h2>
              <p class="mt-3 text-sm leading-6 text-slate-500">
                Truy cập hệ thống học tập và quản lý đào tạo
              </p>
            </div>

            <form class="space-y-5" novalidate @submit.prevent="submitLogin">
              <div>
                <label for="login-identifier" class="mb-2 block text-sm font-semibold text-slate-700">
                  Email hoặc tên đăng nhập
                </label>
                <div
                  :class="[
                    'flex items-center gap-3 rounded-[14px] border bg-white px-3.5 py-3 transition duration-200',
                    fieldErrors.email
                      ? 'border-red-300 ring-4 ring-red-100'
                      : 'border-slate-200 focus-within:border-[#1E3A8A] focus-within:ring-4 focus-within:ring-blue-100',
                  ]"
                >
                  <Mail :size="18" class="shrink-0 text-slate-400" />
                  <input
                    id="login-identifier"
                    v-model="form.email"
                    type="text"
                    autocomplete="username"
                    class="w-full bg-transparent text-[15px] text-slate-950 outline-none placeholder:text-slate-400"
                    placeholder="student@example.com"
                    aria-describedby="login-identifier-error"
                    :aria-invalid="Boolean(fieldErrors.email)"
                    @input="clearFieldError('email')"
                  />
                </div>
                <p
                  v-if="fieldErrors.email"
                  id="login-identifier-error"
                  class="mt-1.5 text-sm text-red-600"
                >
                  {{ fieldErrors.email }}
                </p>
              </div>

              <div>
                <label for="login-password" class="mb-2 block text-sm font-semibold text-slate-700">
                  Mật khẩu
                </label>
                <div
                  :class="[
                    'flex items-center gap-3 rounded-[14px] border bg-white px-3.5 py-3 transition duration-200',
                    fieldErrors.password
                      ? 'border-red-300 ring-4 ring-red-100'
                      : 'border-slate-200 focus-within:border-[#1E3A8A] focus-within:ring-4 focus-within:ring-blue-100',
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
                    class="rounded-full p-1.5 text-slate-400 transition hover:bg-slate-100 hover:text-slate-700 focus:outline-none focus:ring-4 focus:ring-blue-100"
                    :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                    @click="showPassword = !showPassword"
                  >
                    <EyeOff v-if="showPassword" :size="18" />
                    <Eye v-else :size="18" />
                  </button>
                </div>
                <p
                  v-if="fieldErrors.password"
                  id="login-password-error"
                  class="mt-1.5 text-sm text-red-600"
                >
                  {{ fieldErrors.password }}
                </p>
              </div>

              <div class="flex flex-wrap items-center justify-between gap-3">
                <label class="inline-flex cursor-pointer items-center gap-2 text-sm font-medium text-slate-600">
                  <input
                    v-model="rememberLogin"
                    type="checkbox"
                    class="h-4 w-4 rounded border-slate-300 text-[#1E3A8A] focus:ring-[#1E3A8A]"
                  />
                  Ghi nhớ đăng nhập
                </label>
                <RouterLink
                  to="/about"
                  class="inline-flex items-center gap-1 text-sm font-semibold text-[#1E3A8A] transition hover:text-[#7C3AED]"
                >
                  <HelpCircle :size="15" />
                  Quên mật khẩu?
                </RouterLink>
              </div>

              <div
                v-if="formError || authStore.error"
                role="alert"
                class="flex gap-3 rounded-[14px] border border-red-200 bg-red-50 px-4 py-3 text-sm leading-6 text-red-700"
              >
                <AlertCircle :size="18" class="mt-0.5 shrink-0" />
                <span>{{ formError || authStore.error }}</span>
              </div>

              <button
                type="submit"
                :disabled="!canSubmit"
                class="group flex w-full items-center justify-center gap-2 rounded-[14px] bg-[#1E3A8A] px-5 py-3.5 text-sm font-bold text-white shadow-lg shadow-blue-900/20 transition duration-200 hover:-translate-y-0.5 hover:bg-[#2347A5] hover:shadow-xl hover:shadow-blue-900/25 disabled:translate-y-0 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:shadow-none"
              >
                <LoaderCircle v-if="authStore.loading" :size="18" class="animate-spin" />
                <span>{{ authStore.loading ? 'Đang đăng nhập...' : 'Đăng nhập' }}</span>
                <ArrowRight
                  v-if="!authStore.loading"
                  :size="18"
                  class="transition group-hover:translate-x-0.5"
                />
              </button>
            </form>

            <div class="mt-6 rounded-[20px] border border-blue-100 bg-blue-50/70 px-4 py-3 text-sm leading-6 text-slate-600">
              Tài khoản demo do giảng viên hoặc quản trị viên cấp. Không nhập mật khẩu thật trên máy không tin cậy.
            </div>
          </div>

          <p class="mt-5 text-center text-xs text-slate-500">
            LMS tốt nghiệp · giao diện theo DESIGN.md · không dùng mock login
          </p>
        </div>
      </section>
    </div>
  </main>
</template>

<style scoped>
.login-card {
  animation: card-enter 520ms cubic-bezier(0.22, 1, 0.36, 1) both;
}

.blob {
  animation: blob-drift 14s ease-in-out infinite alternate;
}

.blob-cyan {
  animation-delay: -4s;
}

.blob-violet {
  animation-delay: -8s;
}

@keyframes card-enter {
  from {
    opacity: 0;
    transform: translateY(18px) scale(0.985);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes blob-drift {
  from {
    transform: translate3d(0, 0, 0) scale(1);
  }
  to {
    transform: translate3d(18px, -14px, 0) scale(1.06);
  }
}
</style>
