<script setup>
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  AlertCircle,
  AlertTriangle,
  ArrowRight,
  Bell,
  Box,
  Calendar,
  ChevronDown,
  ClipboardList,
  Eye,
  EyeOff,
  GraduationCap,
  LoaderCircle,
  Lock,
  ShieldCheck,
  User,
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

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
const capsLockOn = ref(false)
const demoExpanded = ref(false)

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
      targetPath.startsWith('/bgh') ||
      targetPath.startsWith('/parent') ||
      targetPath.startsWith('/super-admin')
    ) {
      if (authStore.hasRole('SuperAdmin')) targetPath = '/super-admin/dashboard'
      else if (authStore.hasRole('Principal')) targetPath = '/bgh/dashboard'
      else if (authStore.hasRole('Teacher')) targetPath = '/teacher/dashboard'
      else if (authStore.hasRole('AcademicStaff')) targetPath = '/staff/dashboard'
      else if (authStore.hasRole('Parent')) targetPath = '/parent/dashboard'
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

function quickLogin(demoEmail) {
  form.email = demoEmail
  form.password = 'mock_password_no_need'
  submitLogin()
}

function checkCapsLock(e) {
  capsLockOn.value = e.getModifierState('CapsLock')
}

function toggleDemo() {
  demoExpanded.value = !demoExpanded.value
}
</script>

<template>
  <main class="liquid-bg min-h-screen flex items-center justify-center p-4 md:p-10 font-['Inter'] text-[#191c1e] antialiased overflow-x-hidden relative">
    <div class="fixed top-[-10%] left-[-10%] w-[40%] h-[40%] rounded-full bg-[#dce1ff] blur-[120px] opacity-30 pointer-events-none"></div>
    <div class="fixed bottom-[-10%] right-[-10%] w-[40%] h-[40%] rounded-full bg-[#00236f]/20 blur-[120px] opacity-30 pointer-events-none"></div>

    <main class="w-full max-w-[1140px] relative z-10 glass-panel rounded-[28px] overflow-hidden shadow-2xl shadow-[#00236f]/5 flex flex-col lg:flex-row min-h-[600px] h-[calc(100vh-80px)] max-h-[768px]">
      <!-- Left Panel -->
      <div class="hidden lg:flex flex-col w-1/2 p-8 bg-gradient-to-br from-[#00236f]/5 to-transparent relative border-r border-[#00236f]/5">
        <div class="absolute inset-0 bg-[radial-gradient(#1e3a8a_1px,transparent_1px)] [background-size:16px_16px] pointer-events-none opacity-[0.05] pattern-mask"></div>

        <div class="relative z-10 space-y-2">
          <div class="flex items-center gap-2 text-[#00236f]">
            <GraduationCap class="w-8 h-8" aria-hidden="true" />
            <div class="flex flex-col">
              <span class="font-bold text-[24px] leading-tight">EduLMS</span>
              <span class="text-[13px] font-medium text-[#444651]">Cơ sở Đồng Nai</span>
            </div>
          </div>
          <div class="inline-block px-3 py-1 bg-[#00236f]/10 rounded-full font-semibold text-[13px] text-[#00236f] mt-4">
            Hệ thống quản lý học tập nội bộ
          </div>
        </div>

        <div class="relative z-10 mt-8 mb-4 space-y-0 flex flex-col">
          <div class="space-y-[10px]">
            <h1 class="text-[#00236f] tracking-tight leading-tight font-bold" style="font-size: 38px; line-height: 1.1;">
              Không gian học tập<br />của bạn
            </h1>
            <p class="text-lg text-[#444651] max-w-md" style="font-size: 18px; line-height: 28px; font-weight: 400;">
              Nền tảng tập trung kết nối sinh viên và giảng viên, cung cấp trải nghiệm học tập hiện đại, liền mạch và hiệu quả.
            </p>
          </div>

          <div class="grid grid-cols-3 gap-4 mt-6">
            <div class="bg-white/60 backdrop-blur-md rounded-xl p-4 border border-white/50 shadow-sm flex flex-col items-start gap-1">
              <div class="w-[36px] h-[36px] rounded-lg bg-[#00236f]/10 flex items-center justify-center text-[#00236f] mb-1">
                <Calendar class="w-[18px] h-[18px]" aria-hidden="true" />
              </div>
              <span class="font-semibold text-[13px] text-[#191c1e]">Lịch học</span>
              <span class="text-[12px] text-[#444651] font-medium leading-[1.45]">Theo dõi thời khóa biểu và phòng học</span>
            </div>
            <div class="bg-white/60 backdrop-blur-md rounded-xl p-4 border border-white/50 shadow-sm flex flex-col items-start gap-1">
              <div class="w-[36px] h-[36px] rounded-lg bg-[#00236f]/10 flex items-center justify-center text-[#00236f] mb-1">
                <ClipboardList class="w-[18px] h-[18px]" aria-hidden="true" />
              </div>
              <span class="font-semibold text-[13px] text-[#191c1e]">Bài tập</span>
              <span class="text-[12px] text-[#444651] font-medium leading-[1.45]">Quản lý hạn nộp và tiến độ</span>
            </div>
            <div class="bg-white/60 backdrop-blur-md rounded-xl p-4 border border-white/50 shadow-sm flex flex-col items-start gap-1">
              <div class="w-[36px] h-[36px] rounded-lg bg-[#00236f]/10 flex items-center justify-center text-[#00236f] mb-1">
                <Bell class="w-[18px] h-[18px]" aria-hidden="true" />
              </div>
              <span class="font-semibold text-[13px] text-[#191c1e]">Thông báo</span>
              <span class="text-[12px] text-[#444651] font-medium leading-[1.45]">Nhận cập nhật từ nhà trường</span>
            </div>
          </div>
        </div>

        <div class="relative z-10 flex items-center gap-2 mt-auto text-[#191c1e]">
          <ShieldCheck class="w-5 h-5" aria-hidden="true" />
          <span class="text-[13px] font-medium">Học tập an toàn và liền mạch</span>
        </div>
      </div>

      <!-- Right Panel: Login Form -->
      <div class="w-full lg:w-1/2 p-8 md:p-12 flex flex-col justify-center bg-white/40 overflow-y-auto">
        <!-- Mobile Brand -->
        <div class="flex lg:hidden items-center gap-2 text-[#00236f] mb-8">
          <GraduationCap class="w-8 h-8" aria-hidden="true" />
          <div class="flex flex-col">
            <span class="text-[24px] font-semibold leading-tight tracking-[-0.01em]">EduLMS</span>
            <span class="text-[12px] font-medium leading-4 tracking-[0.04em] text-[#585f67]">Cơ sở Đồng Nai</span>
          </div>
        </div>

        <div class="max-w-sm w-full mx-auto space-y-8 my-auto">
          <div class="space-y-2 text-center lg:text-left">
            <h2 class="text-[24px] font-semibold leading-8 tracking-[-0.01em] text-[#191c1e]">Chào mừng trở lại</h2>
            <p class="text-[14px] leading-5 text-[#444651]">Vui lòng đăng nhập để tiếp tục</p>
          </div>

          <form class="space-y-4" novalidate @submit.prevent="submitLogin">
            <!-- Alert -->
            <div
              v-if="errorMessage"
              class="p-3 rounded-lg bg-[#ffdad6] text-[#93000a] text-[14px] leading-5 flex items-center gap-2"
              role="alert"
            >
              <AlertCircle class="w-5 h-5 flex-shrink-0" aria-hidden="true" />
              <span>{{ errorMessage }}</span>
            </div>

            <!-- Username -->
            <div class="space-y-1">
              <label class="block text-[14px] font-semibold text-[#444651]" for="username">Tên đăng nhập / Email</label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#585f67] flex items-center">
                  <User class="w-5 h-5" aria-hidden="true" />
                </span>
                <input
                  id="username"
                  v-model="form.email"
                  name="username"
                  type="text"
                  autocomplete="username"
                  required
                  :disabled="authStore.loading"
                  :aria-invalid="Boolean(fieldErrors.email)"
                  :aria-describedby="fieldErrors.email ? 'email-error' : undefined"
                  class="w-full pl-10 pr-4 py-3 rounded-lg glass-input font-['Inter'] text-[15px] leading-6 text-[#191c1e] placeholder:text-[#c5c5d3] disabled:cursor-not-allowed"
                  placeholder="Email trường hoặc mã tài khoản"
                  @input="clearFieldError('email')"
                />
              </div>
              <p
                v-if="fieldErrors.email"
                id="email-error"
                role="alert"
                class="text-[12px] font-semibold text-[#ba1a1a]"
              >
                {{ fieldErrors.email }}
              </p>
            </div>

            <!-- Password -->
            <div class="space-y-1">
              <label class="block text-[14px] font-semibold text-[#444651]" for="password">Mật khẩu</label>
              <div class="relative">
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#585f67] flex items-center">
                  <Lock class="w-5 h-5" aria-hidden="true" />
                </span>
                <input
                  id="password"
                  v-model="form.password"
                  name="password"
                  :type="showPassword ? 'text' : 'password'"
                  autocomplete="current-password"
                  required
                  :disabled="authStore.loading"
                  :aria-invalid="Boolean(fieldErrors.password)"
                  :aria-describedby="fieldErrors.password ? 'password-error' : undefined"
                  class="w-full pl-10 pr-10 py-3 rounded-lg glass-input font-['Inter'] text-[15px] leading-6 text-[#191c1e] placeholder:text-[#c5c5d3] disabled:cursor-not-allowed"
                  placeholder="••••••••"
                  @input="clearFieldError('password')"
                  @keyup="checkCapsLock"
                  @keydown="checkCapsLock"
                />
                <button
                  type="button"
                  :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
                  :disabled="authStore.loading"
                  class="absolute right-3 top-1/2 -translate-y-1/2 text-[#585f67] hover:text-[#00236f] transition-colors flex items-center disabled:cursor-not-allowed disabled:opacity-50"
                  @click="showPassword = !showPassword"
                >
                  <EyeOff v-if="showPassword" class="w-5 h-5" aria-hidden="true" />
                  <Eye v-else class="w-5 h-5" aria-hidden="true" />
                </button>
              </div>
              <p
                v-if="fieldErrors.password"
                id="password-error"
                role="alert"
                class="text-[12px] font-semibold text-[#ba1a1a]"
              >
                {{ fieldErrors.password }}
              </p>
              <div
                v-if="capsLockOn"
                class="text-[12px] text-[#ba1a1a] flex items-center gap-1 mt-1"
              >
                <AlertTriangle class="w-4 h-4" aria-hidden="true" />
                <span>Caps Lock đang bật</span>
              </div>
            </div>

            <!-- Remember & Support -->
            <div class="flex flex-col gap-2">
              <label class="flex items-center gap-2 cursor-pointer w-fit min-h-[44px]">
                <input
                  v-model="rememberLogin"
                  type="checkbox"
                  :disabled="authStore.loading"
                  class="rounded text-[#00236f] focus:ring-[#00236f]/50 w-4 h-4 border-[#c5c5d3]"
                />
                <span class="text-[14px] text-[#444651] select-none">Ghi nhớ đăng nhập</span>
              </label>
              <p class="text-[13px] text-[#191c1e] font-semibold">
                Gặp sự cố đăng nhập? Liên hệ phòng đào tạo.
              </p>
            </div>

            <!-- Submit -->
            <button
              type="submit"
              :disabled="!canSubmit"
              :aria-busy="authStore.loading ? 'true' : 'false'"
              class="login-submit-btn w-full mt-4 py-3 px-4 rounded-lg font-semibold text-[14px] leading-4 tracking-[0.02em] flex justify-center items-center gap-2 transition-all duration-300 text-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              <LoaderCircle v-if="authStore.loading" class="w-5 h-5 animate-spin" aria-hidden="true" />
              <span v-else>Đăng nhập</span>
              <ArrowRight v-if="!authStore.loading" class="w-5 h-5 group-hover/btn:translate-x-1 transition-transform" aria-hidden="true" />
            </button>
          </form>

          <!-- Divider -->
          <div class="relative flex py-4 items-center">
            <div class="flex-grow border-t border-[#c5c5d3]/30"></div>
            <span class="flex-shrink-0 mx-4 text-[12px] font-medium leading-4 tracking-[0.04em] text-[#757682]">hoặc</span>
            <div class="flex-grow border-t border-[#c5c5d3]/30"></div>
          </div>

          <!-- Demo Accordion -->
          <div class="border border-[#c5c5d3]/30 rounded-lg overflow-hidden bg-white/30 backdrop-blur-sm">
            <button
              type="button"
              :aria-expanded="demoExpanded"
              aria-controls="demo-accounts-panel"
              class="w-full flex items-center justify-between p-4 text-[14px] font-semibold leading-4 tracking-[0.02em] text-[#191c1e] hover:bg-white/50 transition-colors"
              @click="toggleDemo"
            >
              <div class="flex items-center gap-2">
                <Box class="w-5 h-5 text-[#00236f]" aria-hidden="true" />
                Tài khoản demo
              </div>
              <ChevronDown
                class="w-5 h-5 transition-transform duration-300"
                :class="demoExpanded && 'rotate-180'"
                aria-hidden="true"
              />
            </button>
            <div
              id="demo-accounts-panel"
              v-show="demoExpanded"
              class="px-4 pb-4"
            >
               <div class="grid grid-cols-2 md:grid-cols-4 gap-2 pt-2">
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('sv_it@demo')"
                >
                  <span class="font-bold">Sinh viên IT</span>
                  <span class="text-[10px] opacity-80">sv_it@demo</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('sv_kt@demo')"
                >
                  <span class="font-bold">Sinh viên Kế toán</span>
                  <span class="text-[10px] opacity-80">sv_kt@demo</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('sv_qtkd@demo')"
                >
                  <span class="font-bold">Sinh viên QTKD</span>
                  <span class="text-[10px] opacity-80">sv_qtkd@demo</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('gv_kythuat@demo')"
                >
                  <span class="font-bold">Giảng viên</span>
                  <span class="text-[10px] opacity-80">gv_kythuat@demo</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('staff')"
                >
                  <span class="font-bold">Giáo vụ</span>
                  <span class="text-[10px] opacity-80">staff</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('bgh')"
                >
                  <span class="font-bold">Hiệu trưởng</span>
                  <span class="text-[10px] opacity-80">bgh</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('parent')"
                >
                  <span class="font-bold">Phụ huynh</span>
                  <span class="text-[10px] opacity-80">parent</span>
                </button>
                <button
                  type="button"
                  class="py-2 px-3 bg-[#dce3ec]/50 hover:bg-[#dce3ec] text-[#5e656d] rounded-md text-[12px] font-medium leading-4 tracking-[0.04em] transition-colors text-left flex flex-col"
                  @click="quickLogin('admin@demo')"
                >
                  <span class="font-bold">Quản trị viên</span>
                  <span class="text-[10px] opacity-80">admin@demo</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="mt-8 text-center pb-4 lg:pb-0">
          <p class="text-[13px] text-[#191c1e] font-semibold flex items-center justify-center gap-1">
            <ShieldCheck class="w-4 h-4 text-[#00236f]" aria-hidden="true" />
            Đăng nhập an toàn: Chỉ sử dụng tài khoản được nhà trường cấp.
          </p>
        </div>
      </div>
    </main>
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

.pattern-mask {
  -webkit-mask-image: linear-gradient(to bottom right, rgba(0,0,0,0.75), transparent 78%);
  mask-image: linear-gradient(to bottom right, rgba(0,0,0,0.75), transparent 78%);
}

.glass-input {
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(30, 58, 138, 0.1);
  transition: all 0.3s ease;
}

.glass-input:focus {
  background: rgba(255, 255, 255, 0.8);
  border-color: #1e3a8a;
  outline: none;
  box-shadow: 0 0 0 2px rgba(30, 58, 138, 0.1);
}

.glass-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.login-submit-btn {
  background: linear-gradient(90deg, #1d4ed8 0%, #2563eb 52%, #0891b2 100%);
  box-shadow:
    0 18px 40px rgba(37, 99, 235, 0.28),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
}

.login-submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 22px 48px rgba(37, 99, 235, 0.34),
    inset 0 1px 0 rgba(255, 255, 255, 0.34);
}

.login-submit-btn:active:not(:disabled) {
  transform: scale(0.98);
}
</style>
