<script setup>
/**
 * ProfileView.vue - Super Admin
 * Trang quản lý thông tin hồ sơ cá nhân và đổi mật khẩu của Super Admin.
 * Được thiết kế đồng bộ với hệ thống Liquid Glass UI.
 */
import { computed, onMounted, reactive, ref } from 'vue'
import { AlertCircle, RefreshCw, Save, UserCog, ShieldCheck, KeyRound, ArrowRight } from 'lucide-vue-next'
import { apiRequest } from '@/services/apiClient'

const loading = ref(false)
const saving = ref(false)
const changingPassword = ref(false)
const error = ref('')
const success = ref('')

const profile = reactive({
  id: '',
  hoTen: '',
  email: '',
  soDienThoai: '',
  vaiTroChinh: '',
  trangThai: '',
})

const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const hasProfile = computed(() => Boolean(profile.id || profile.email))

function applyProfile(data = {}) {
  profile.id = data.id || data.Id || ''
  profile.hoTen = data.hoTen || data.HoTen || ''
  profile.email = data.email || data.Email || ''
  profile.soDienThoai = data.soDienThoai || data.SoDienThoai || ''
  profile.vaiTroChinh = data.vaiTroChinh || data.VaiTroChinh || ''
  profile.trangThai = data.trangThai || data.TrangThai || ''
}

async function loadProfile() {
  loading.value = true
  error.value = ''
  success.value = ''

  try {
    const data = await apiRequest('/api/account/me')
    applyProfile(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được hồ sơ tài khoản.'
  } finally {
    loading.value = false
  }
}

async function saveProfile() {
  saving.value = true
  error.value = ''
  success.value = ''

  try {
    const data = await apiRequest('/api/account/profile', {
      method: 'PUT',
      body: JSON.stringify({
        email: profile.email,
        hoTen: profile.hoTen,
        soDienThoai: profile.soDienThoai,
      }),
    })
    applyProfile(data)
    success.value = 'Đã cập nhật thông tin hồ sơ cá nhân thành công.'
  } catch (err) {
    error.value = err?.message || 'Không cập nhật được hồ sơ.'
  } finally {
    saving.value = false
  }
}

async function changePassword() {
  changingPassword.value = true
  error.value = ''
  success.value = ''

  try {
    await apiRequest('/api/account/change-password', {
      method: 'PUT',
      body: JSON.stringify(passwordForm),
    })
    passwordForm.currentPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
    success.value = 'Đã đổi mật khẩu tài khoản thành công.'
  } catch (err) {
    error.value = err?.message || 'Không đổi được mật khẩu.'
  } finally {
    changingPassword.value = false
  }
}

onMounted(loadProfile)
</script>

<template>
  <section class="space-y-6">
    <!-- Header -->
    <header class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <p class="text-xs font-bold text-violet-600 dark:text-violet-400 uppercase tracking-widest">SuperAdmin Portal</p>
        <h1 class="text-2xl font-extrabold text-heading">Hồ sơ tài khoản cá nhân</h1>
        <p class="mt-1 text-xs text-muted">Quản lý thông tin định danh và bảo mật tài khoản hệ thống của bạn.</p>
      </div>

      <button
        type="button"
        class="lg-btn-secondary px-3.5 py-2 text-xs font-bold"
        :disabled="loading"
        @click="loadProfile"
      >
        <RefreshCw class="h-3.5 w-3.5" :class="{ 'animate-spin': loading }" />
        Tải lại dữ liệu
      </button>
    </header>

    <!-- Alerts -->
    <div v-if="error" class="lg-alert lg-alert-error flex items-start gap-2.5">
      <AlertCircle class="mt-0.5 h-4.5 w-4.5 flex-shrink-0" />
      <span>{{ error }}</span>
    </div>

    <div v-if="success" class="lg-alert lg-alert-success flex items-start gap-2.5">
      <ShieldCheck class="mt-0.5 h-4.5 w-4.5 flex-shrink-0" />
      <span>{{ success }}</span>
    </div>

    <!-- Main Grid -->
    <div class="grid gap-6 lg:grid-cols-[1fr_400px]">
      
      <!-- Edit Profile form -->
      <form class="lg-glass-soft lg-density-spacious rounded-2xl border border-default flex flex-col gap-4" @submit.prevent="saveProfile">
        <div class="flex items-center gap-3">
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-violet-500/10 text-violet-600 dark:text-violet-400">
            <UserCog class="h-5.5 w-5.5" />
          </div>
          <div>
            <h2 class="text-base font-extrabold text-heading">Thông tin cá nhân</h2>
            <p class="text-[10px] font-mono text-muted uppercase">GET /api/account/me · PUT /api/account/profile</p>
          </div>
        </div>

        <hr class="border-default/50" />

        <div v-if="loading" class="flex flex-col gap-4">
          <div v-for="i in 3" :key="i" class="space-y-1.5 animate-pulse">
            <div class="h-3.5 w-24 bg-slate-200 dark:bg-slate-700 rounded" />
            <div class="h-9 w-full bg-slate-200 dark:bg-slate-700 rounded" />
          </div>
        </div>

        <div v-else-if="!hasProfile" class="flex flex-col items-center justify-center py-10 text-center gap-2">
          <AlertCircle class="h-10 w-10 text-slate-300 dark:text-slate-600" />
          <p class="text-sm font-semibold text-label">Không tìm thấy thông tin</p>
          <p class="text-xs text-muted">API chưa trả về dữ liệu hồ sơ cá nhân.</p>
        </div>

        <div v-else class="space-y-4">
          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Họ và tên</span>
            <input v-model="profile.hoTen" class="lg-input px-3.5 py-2 font-semibold text-body" required />
          </label>
          
          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Địa chỉ Email</span>
            <input v-model="profile.email" type="email" class="lg-input px-3.5 py-2 font-semibold text-body" required />
          </label>

          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Số điện thoại liên hệ</span>
            <input v-model="profile.soDienThoai" class="lg-input px-3.5 py-2 font-semibold text-body" />
          </label>

          <div class="grid grid-cols-2 gap-4 rounded-xl border border-default bg-[var(--surface-app)] p-4 text-xs">
            <div>
              <span class="text-muted font-bold block uppercase text-[10px] mb-1">Vai trò hệ thống</span>
              <strong class="text-heading text-sm">{{ profile.vaiTroChinh || 'Chưa phân quyền' }}</strong>
            </div>
            <div>
              <span class="text-muted font-bold block uppercase text-[10px] mb-1">Trạng thái hoạt động</span>
              <strong class="text-heading text-sm">{{ profile.trangThai || 'N/A' }}</strong>
            </div>
          </div>

          <button
            type="submit"
            class="lg-btn-primary px-5 py-2 text-xs font-bold"
            :disabled="saving"
          >
            <Save class="h-4 w-4" />
            Lưu thay đổi hồ sơ
          </button>
        </div>
      </form>

      <!-- Change password form -->
      <form class="lg-glass-soft lg-density-spacious rounded-2xl border border-default flex flex-col gap-4" @submit.prevent="changePassword">
        <div class="flex items-center gap-3">
          <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400">
            <KeyRound class="h-5.5 w-5.5" />
          </div>
          <div>
            <h2 class="text-base font-extrabold text-heading">Đổi mật khẩu bảo mật</h2>
            <p class="text-[10px] font-mono text-muted uppercase">PUT /api/account/change-password</p>
          </div>
        </div>

        <hr class="border-default/50" />

        <div class="space-y-4">
          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Mật khẩu hiện tại</span>
            <input v-model="passwordForm.currentPassword" type="password" class="lg-input px-3.5 py-2 text-body" required />
          </label>
          
          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Mật khẩu mới</span>
            <input v-model="passwordForm.newPassword" type="password" class="lg-input px-3.5 py-2 text-body" required minlength="8" />
          </label>

          <label class="grid gap-1.5 text-xs font-bold text-label uppercase">
            <span>Xác nhận mật khẩu mới</span>
            <input v-model="passwordForm.confirmPassword" type="password" class="lg-input px-3.5 py-2 text-body" required minlength="8" />
          </label>

          <button
            type="submit"
            class="lg-btn-primary px-5 py-2 text-xs font-bold"
            :disabled="changingPassword"
          >
            <Save class="h-4 w-4" />
            Cập nhật mật khẩu mới
          </button>
        </div>
      </form>
    </div>

    <!-- Quick link to login logs -->
    <div class="lg-glass-soft rounded-2xl border border-default p-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div class="space-y-1">
        <h4 class="text-sm font-bold text-heading">Kiểm tra lịch sử đăng nhập bảo mật</h4>
        <p class="text-xs text-muted">Lịch sử và cảnh báo rủi ro đăng nhập AI của bạn được cập nhật trực tiếp tại Trung tâm nhật ký.</p>
      </div>
      <router-link
        to="/super-admin/login-history"
        class="lg-btn-secondary px-4 py-2 text-xs font-bold flex items-center gap-1 w-fit"
      >
        Xem nhật ký đăng nhập
        <ArrowRight class="h-3.5 w-3.5" />
      </router-link>
    </div>
  </section>
</template>
