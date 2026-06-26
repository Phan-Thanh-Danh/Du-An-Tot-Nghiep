<script setup>
import { computed, nextTick, reactive, ref } from 'vue'
import {
  AlertCircle,
  AlertTriangle,
  ArrowRight,
  Box,
  Eye,
  EyeOff,
  LoaderCircle,
  Lock,
  User,
} from 'lucide-vue-next'
import { SHOW_DEMO_ACCOUNTS } from '@/data/authPortals'

const props = defineProps({
  portal: { type: Object, default: null },
  loading: { type: Boolean, default: false },
  serverError: { type: String, default: '' },
})

const emit = defineEmits(['submit'])

const form = reactive({
  email: '',
  password: '',
})

const rememberLogin = ref(false)
const showPassword = ref(false)
const capsLockOn = ref(false)

const fieldErrors = reactive({
  email: '',
  password: '',
})

const canSubmit = computed(() => form.email.trim() && form.password && !props.loading)
const hasDemo = computed(() => SHOW_DEMO_ACCOUNTS && props.portal?.demoUsername)

function clearFieldError(field) {
  fieldErrors[field] = ''
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

function submitLogin() {
  if (props.loading || !validateForm()) return

  emit('submit', {
    email: form.email.trim(),
    password: form.password,
    remember: rememberLogin.value,
  })
}

function quickLogin(demoEmail) {
  form.email = demoEmail
  form.password = 'mock_password_no_need'
  submitLogin()
}

function checkCapsLock(e) {
  capsLockOn.value = e.getModifierState('CapsLock')
}

defineExpose({
  resetForm() {
    form.email = ''
    form.password = ''
    fieldErrors.email = ''
    fieldErrors.password = ''
    capsLockOn.value = false
    showPassword.value = false
  },
  focusUsername() {
    nextTick(() => {
      document.getElementById('login-username')?.focus()
    })
  },
})
</script>

<template>
  <form class="space-y-4" novalidate @submit.prevent="submitLogin">
    <div
      v-if="serverError"
      class="p-3 rounded-lg bg-[#ffdad6] text-[#93000a] text-[14px] leading-5 flex items-center gap-2"
      role="alert"
    >
      <AlertCircle class="w-5 h-5 flex-shrink-0" aria-hidden="true" />
      <span>{{ serverError }}</span>
    </div>

    <div class="space-y-1">
      <label class="block text-[14px] font-semibold text-[#444651]" for="login-username">Tên đăng nhập / Email</label>
      <div class="relative">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#585f67] flex items-center">
          <User class="w-5 h-5" aria-hidden="true" />
        </span>
        <input
          id="login-username"
          v-model="form.email"
          name="username"
          type="text"
          autocomplete="username"
          required
          :disabled="loading"
          :aria-invalid="Boolean(fieldErrors.email)"
          :aria-describedby="fieldErrors.email ? 'login-email-error' : undefined"
          class="w-full pl-10 pr-4 py-3 rounded-lg glass-input font-['Inter'] text-[15px] leading-6 text-[#191c1e] placeholder:text-[#c5c5d3] disabled:cursor-not-allowed"
          placeholder="Email trường hoặc mã tài khoản"
          @input="clearFieldError('email')"
        />
      </div>
      <p
        v-if="fieldErrors.email"
        id="login-email-error"
        role="alert"
        class="text-[12px] font-semibold text-[#ba1a1a]"
      >
        {{ fieldErrors.email }}
      </p>
    </div>

    <div class="space-y-1">
      <label class="block text-[14px] font-semibold text-[#444651]" for="login-password">Mật khẩu</label>
      <div class="relative">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#585f67] flex items-center">
          <Lock class="w-5 h-5" aria-hidden="true" />
        </span>
        <input
          id="login-password"
          v-model="form.password"
          name="password"
          :type="showPassword ? 'text' : 'password'"
          autocomplete="current-password"
          required
          :disabled="loading"
          :aria-invalid="Boolean(fieldErrors.password)"
          :aria-describedby="fieldErrors.password ? 'login-password-error' : undefined"
          class="w-full pl-10 pr-10 py-3 rounded-lg glass-input font-['Inter'] text-[15px] leading-6 text-[#191c1e] placeholder:text-[#c5c5d3] disabled:cursor-not-allowed"
          placeholder="••••••••"
          @input="clearFieldError('password')"
          @keyup="checkCapsLock"
          @keydown="checkCapsLock"
        />
        <button
          type="button"
          :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
          :disabled="loading"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-[#585f67] hover:text-[#00236f] transition-colors flex items-center disabled:cursor-not-allowed disabled:opacity-50"
          @click="showPassword = !showPassword"
        >
          <EyeOff v-if="showPassword" class="w-5 h-5" aria-hidden="true" />
          <Eye v-else class="w-5 h-5" aria-hidden="true" />
        </button>
      </div>
      <p
        v-if="fieldErrors.password"
        id="login-password-error"
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

    <div class="flex flex-col gap-2">
      <label class="flex items-center gap-2 cursor-pointer w-fit min-h-[44px]">
        <input
          v-model="rememberLogin"
          type="checkbox"
          :disabled="loading"
          class="rounded text-[#00236f] focus:ring-[#00236f]/50 w-4 h-4 border-[#c5c5d3]"
        />
        <span class="text-[14px] text-[#444651] select-none">Ghi nhớ đăng nhập</span>
      </label>
      <p class="text-[13px] text-[#191c1e] font-semibold">
        Gặp sự cố đăng nhập? Liên hệ phòng đào tạo.
      </p>
    </div>

    <button
      type="submit"
      :disabled="!canSubmit"
      :aria-busy="loading ? 'true' : 'false'"
      class="login-submit-btn w-full mt-4 py-3 px-4 rounded-lg font-semibold text-[14px] leading-4 tracking-[0.02em] flex justify-center items-center gap-2 transition-all duration-300 text-white disabled:cursor-not-allowed disabled:opacity-60"
    >
      <LoaderCircle v-if="loading" class="w-5 h-5 animate-spin" aria-hidden="true" />
      <span v-else>Đăng nhập</span>
      <ArrowRight v-if="!loading" class="w-5 h-5 group-hover/btn:translate-x-1 transition-transform" aria-hidden="true" />
    </button>

    <div v-if="hasDemo" class="relative flex py-4 items-center">
      <div class="flex-grow border-t border-[#c5c5d3]/30"></div>
      <span class="flex-shrink-0 mx-4 text-[12px] font-medium leading-4 tracking-[0.04em] text-[#757682]">hoặc</span>
      <div class="flex-grow border-t border-[#c5c5d3]/30"></div>
    </div>

    <button
      v-if="hasDemo"
      type="button"
      class="w-full py-3 px-4 rounded-lg border border-dashed border-[#00236f]/20 text-[13px] font-semibold text-[#00236f] hover:bg-[#00236f]/5 transition-colors flex items-center justify-center gap-2"
      @click="quickLogin(portal.demoUsername)"
    >
      <Box class="w-4 h-4" aria-hidden="true" />
      <span>Đăng nhập với tài khoản demo {{ portal.shortLabel }}</span>
    </button>
  </form>
</template>

<style scoped>
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
