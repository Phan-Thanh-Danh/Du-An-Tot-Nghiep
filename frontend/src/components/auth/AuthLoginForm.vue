<script setup>
import { computed, nextTick, reactive, ref } from 'vue'
import {
  AlertCircle,
  AlertTriangle,
  ArrowRight,
  Eye,
  EyeOff,
  LoaderCircle,
  Lock,
  User,
} from 'lucide-vue-next'

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

function checkCapsLock(e) {
  capsLockOn.value = e.getModifierState('CapsLock')
}

const fastLoginAccounts = {
  teacher: { email: 'teacher.cntt@lms.local', password: '123456', label: 'Tài khoản Giảng viên (Demo)' },
  student: { email: 'p12test_student011@lms.local', password: 'Test@123', label: 'Tài khoản Sinh viên (Demo)' },
  parent: { email: 'p15test_parent01@lms.local', password: 'Test@123', label: 'Tài khoản Phụ huynh (Demo)' },
  staff: { email: 'p12test_staff01@lms.local', password: 'Test@123', label: 'Tài khoản Giáo vụ (Demo)' },
  bgh: { email: 'p15test_bgh01@lms.local', password: 'Test@123', label: 'Tài khoản BGH (Demo)' },
  'content-council': { email: 'p15test_content01@lms.local', password: 'Test@123', label: 'Tài khoản HĐND (Demo)' },
  'super-admin': { email: 'admin@lms.local', password: '123456', label: 'Tài khoản Quản trị (Demo)' }
}

const fastAccount = computed(() => {
  if (!props.portal) return null
  return fastLoginAccounts[props.portal.slug] || null
})

function submitFastLogin() {
  if (fastAccount.value) {
    form.email = fastAccount.value.email
    form.password = fastAccount.value.password
    nextTick(() => submitLogin())
  }
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
      <label class="block text-[14px] font-semibold text-[#00236f]" for="login-username">Tên đăng nhập / Email</label>
      <div class="relative">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#00236f]/60 flex items-center">
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
          class="w-full pl-10 pr-4 py-3 rounded-lg glass-input text-[15px] leading-6 text-[#191c1e] placeholder:text-[#94a3b8] disabled:cursor-not-allowed"
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
      <label class="block text-[14px] font-semibold text-[#00236f]" for="login-password">Mật khẩu</label>
      <div class="relative">
        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[#00236f]/60 flex items-center">
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
          class="w-full pl-10 pr-10 py-3 rounded-lg glass-input text-[15px] leading-6 text-[#191c1e] placeholder:text-[#94a3b8] disabled:cursor-not-allowed"
          placeholder="••••••••"
          @input="clearFieldError('password')"
          @keyup="checkCapsLock"
          @keydown="checkCapsLock"
        />
        <button
          type="button"
          :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'"
          :disabled="loading"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-[#00236f]/60 hover:text-[#00236f] transition-colors flex items-center disabled:cursor-not-allowed disabled:opacity-50"
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
      <label class="flex items-center gap-2 cursor-pointer w-fit py-1">
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

    <!-- Nút đăng nhập nhanh cho môi trường Dev -->
    <div v-if="fastAccount" class="mt-4">
      <div class="relative flex items-center py-2">
        <div class="flex-grow border-t border-[#c5c5d3]"></div>
        <span class="flex-shrink-0 mx-4 text-[#585f67] text-[12px] font-medium">Truy cập nhanh (Dev Only)</span>
        <div class="flex-grow border-t border-[#c5c5d3]"></div>
      </div>
      <button
        type="button"
        @click="submitFastLogin"
        class="w-full mt-2 py-2 px-4 rounded-lg border border-[#c5c5d3] bg-white hover:bg-[#f7f9fb] text-[#00236f] text-[13px] font-semibold transition-colors flex justify-center items-center gap-2"
      >
        <User class="w-4 h-4" aria-hidden="true" />
        {{ fastAccount.label }}
      </button>
    </div>

  </form>
</template>

<style scoped>
.glass-input {
  background: rgba(255, 255, 255, 0.7);
  border: 1px solid rgba(0, 35, 111, 0.2);
  transition: all 0.3s ease;
}

.glass-input:focus {
  background: rgba(255, 255, 255, 1);
  border-color: #00236f;
  outline: none;
  box-shadow: 0 0 0 3px rgba(0, 35, 111, 0.1);
}

.glass-input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Remove default yellow autofill background in webkit browsers */
input:-webkit-autofill,
input:-webkit-autofill:hover,
input:-webkit-autofill:focus,
input:-webkit-autofill:active {
  -webkit-box-shadow: 0 0 0 30px rgba(255, 255, 255, 0.9) inset !important;
  -webkit-text-fill-color: #191c1e !important;
  transition: background-color 5000s ease-in-out 0s;
}

.login-submit-btn {
  background: linear-gradient(90deg, #00236f 0%, #1e3a8a 52%, #0891b2 100%);
  box-shadow:
    0 8px 24px rgba(0, 35, 111, 0.2),
    inset 0 1px 0 rgba(255, 255, 255, 0.2);
}

.login-submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow:
    0 12px 32px rgba(0, 35, 111, 0.3),
    inset 0 1px 0 rgba(255, 255, 255, 0.3);
}

.login-submit-btn:active:not(:disabled) {
  transform: scale(0.98);
}
</style>
