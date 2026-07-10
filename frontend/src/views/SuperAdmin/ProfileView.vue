<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { AlertCircle, RefreshCw, Save, UserCog } from 'lucide-vue-next'
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
    success.value = 'Đã cập nhật hồ sơ.'
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
    success.value = 'Đã đổi mật khẩu.'
  } catch (err) {
    error.value = err?.message || 'Không đổi được mật khẩu.'
  } finally {
    changingPassword.value = false
  }
}

onMounted(loadProfile)
</script>

<template>
  <section class="space-y-4">
    <header class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <p class="text-sm font-semibold text-label">SuperAdmin</p>
        <h1 class="text-2xl font-bold text-heading">Hồ sơ tài khoản</h1>
        <p class="mt-1 text-sm text-body">Dữ liệu lấy từ Account API của người đang đăng nhập.</p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-lg border border-card px-3 py-2 text-sm font-semibold text-heading transition hover:bg-[var(--surface-card-hover)] disabled:cursor-wait disabled:opacity-60"
        :disabled="loading"
        @click="loadProfile"
      >
        <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" />
        Tải lại
      </button>
    </header>

    <div v-if="error" class="rounded-lg border border-[var(--color-danger-text)] bg-[var(--color-danger-bg)] p-4 text-sm text-[var(--color-danger-text)]">
      <div class="flex items-start gap-2">
        <AlertCircle class="mt-0.5 h-4 w-4" />
        <span>{{ error }}</span>
      </div>
    </div>
    <div v-if="success" class="rounded-lg border border-[var(--color-success-text)] bg-[var(--color-success-bg)] p-4 text-sm text-[var(--color-success-text)]">
      {{ success }}
    </div>

    <div class="grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(320px,420px)]">
      <form class="rounded-lg border border-card surface-card p-5" @submit.prevent="saveProfile">
        <div class="mb-4 flex items-center gap-3">
          <UserCog class="h-5 w-5 text-link" />
          <div>
            <h2 class="text-lg font-semibold text-heading">Thông tin cá nhân</h2>
            <p class="text-sm text-body">GET /api/account/me, PUT /api/account/profile</p>
          </div>
        </div>

        <div v-if="loading" class="text-sm text-body">Đang tải hồ sơ...</div>
        <div v-else-if="!hasProfile" class="text-sm text-body">API chưa trả về hồ sơ.</div>
        <div v-else class="grid gap-4">
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Họ tên</span>
            <input v-model="profile.hoTen" class="rounded-lg border border-card surface-input px-3 py-2 text-body" />
          </label>
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Email</span>
            <input v-model="profile.email" type="email" class="rounded-lg border border-card surface-input px-3 py-2 text-body" />
          </label>
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Số điện thoại</span>
            <input v-model="profile.soDienThoai" class="rounded-lg border border-card surface-input px-3 py-2 text-body" />
          </label>
          <div class="grid gap-2 rounded-lg border border-card bg-[var(--surface-muted)] p-3 text-sm text-body">
            <p><span class="font-semibold text-heading">Vai trò:</span> {{ profile.vaiTroChinh || 'Chưa có dữ liệu' }}</p>
            <p><span class="font-semibold text-heading">Trạng thái:</span> {{ profile.trangThai || 'Chưa có dữ liệu' }}</p>
          </div>
          <button
            type="submit"
            class="inline-flex w-fit items-center gap-2 rounded-lg bg-[var(--lg-primary)] px-4 py-2 text-sm font-semibold text-white disabled:cursor-wait disabled:opacity-60"
            :disabled="saving"
          >
            <Save class="h-4 w-4" />
            Lưu hồ sơ
          </button>
        </div>
      </form>

      <form class="rounded-lg border border-card surface-card p-5" @submit.prevent="changePassword">
        <h2 class="text-lg font-semibold text-heading">Đổi mật khẩu</h2>
        <p class="mb-4 text-sm text-body">PUT /api/account/change-password</p>
        <div class="grid gap-4">
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Mật khẩu hiện tại</span>
            <input v-model="passwordForm.currentPassword" type="password" class="rounded-lg border border-card surface-input px-3 py-2 text-body" required />
          </label>
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Mật khẩu mới</span>
            <input v-model="passwordForm.newPassword" type="password" class="rounded-lg border border-card surface-input px-3 py-2 text-body" required minlength="8" />
          </label>
          <label class="grid gap-1 text-sm">
            <span class="font-medium text-label">Xác nhận mật khẩu</span>
            <input v-model="passwordForm.confirmPassword" type="password" class="rounded-lg border border-card surface-input px-3 py-2 text-body" required minlength="8" />
          </label>
          <button
            type="submit"
            class="inline-flex w-fit items-center gap-2 rounded-lg bg-[var(--lg-primary)] px-4 py-2 text-sm font-semibold text-white disabled:cursor-wait disabled:opacity-60"
            :disabled="changingPassword"
          >
            <Save class="h-4 w-4" />
            Đổi mật khẩu
          </button>
        </div>
      </form>
    </div>

    <div class="rounded-lg border border-card surface-card p-4 text-sm text-body">
      Lịch sử đăng nhập không còn dùng dữ liệu cục bộ. Cần endpoint backend riêng để bật phần này vào full action/API claim.
    </div>
  </section>
</template>
