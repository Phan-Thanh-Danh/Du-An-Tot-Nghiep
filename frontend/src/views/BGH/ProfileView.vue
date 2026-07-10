<template>
  <div class="space-y-6 pb-10">
    <div v-if="loading" class="p-4">
      <SkeletonTable :rows="6" :columns="2" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <template v-else>
    <div class="surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
      <div class="p-6 lg:p-8">
        <div class="flex flex-col lg:flex-row items-center lg:items-start gap-6">
          <div class="relative flex-shrink-0">
            <div class="h-20 w-20 rounded-2xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-2xl font-bold text-white shadow-lg">{{ profile.initials }}</div>
            <div class="absolute -bottom-1 -right-1 h-5 w-5 rounded-full border-2 border-(--surface-card) bg-(--color-success-text)" />
          </div>
          <div class="flex-1 text-center lg:text-left">
            <h1 class="text-2xl font-bold text-heading">{{ profile.hoTen }}</h1>
            <p class="text-sm text-muted mt-1">{{ profile.email }}</p>
            <div class="flex flex-wrap justify-center lg:justify-start gap-2 mt-3">
              <span class="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-(--color-info-bg) text-(--color-info-text) text-xs font-bold">
                <ShieldCheck :size="14" /> Ban Giám Hiệu
              </span>
              <span class="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-(--color-success-bg) text-(--color-success-text) text-xs font-bold">
                <CheckCircle2 :size="14" /> Đang hoạt động
              </span>
            </div>
          </div>
          <div class="flex gap-2">
            <button v-if="!editing" @click="startEdit" class="flex items-center gap-1.5 px-4 py-2 bg-(--lg-primary) text-white text-sm font-bold rounded-xl hover:brightness-110 transition-all shadow-sm">
              <Edit2 :size="16" /> Chỉnh sửa
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <div class="lg:col-span-2 space-y-6">
        <div class="surface-card border border-card rounded-2xl p-6 shadow-sm">
          <h2 class="text-lg font-bold text-heading mb-4">Thông tin cá nhân</h2>
          <div class="space-y-4">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div v-for="field in profileFields" :key="field.key" class="space-y-1">
                <label class="block text-xs font-bold text-muted">{{ field.label }}</label>
                <div v-if="!editing" class="text-sm font-semibold text-heading px-3 py-2 bg-(--surface-input) rounded-lg">
                  {{ field.value || '—' }}
                </div>
                <input v-if="editing && field.editable" v-model="editForm[field.key]" type="text" :placeholder="field.label" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)" />
                <div v-else-if="editing" class="text-sm font-semibold text-heading px-3 py-2 bg-(--surface-input) rounded-lg">
                  {{ field.value || '—' }}
                </div>
              </div>
            </div>
          </div>
          <div v-if="editing" class="flex items-center gap-3 mt-6 pt-4">
            <button @click="cancelEdit" class="px-4 py-2 border border-input rounded-lg text-sm font-bold text-body hover:bg-(--surface-input) transition-colors">Hủy</button>
            <button @click="saveProfile" class="px-4 py-2 bg-(--lg-primary) text-white text-sm font-bold rounded-lg hover:brightness-110 transition-all">Lưu thay đổi</button>
          </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-6 shadow-sm">
          <h2 class="text-lg font-bold text-heading mb-4">Đổi mật khẩu</h2>
          <div class="space-y-4 max-w-md">
            <div class="space-y-1">
              <label class="block text-xs font-bold text-muted">Mật khẩu hiện tại</label>
              <div class="relative">
                <input v-model="passwordForm.oldPassword" :type="showOld ? 'text' : 'password'" placeholder="Nhập mật khẩu hiện tại" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm pr-10 focus:outline-none focus:border-(--lg-primary)" />
                <button @click="showOld = !showOld" class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading">
                  <Eye v-if="!showOld" :size="16" /><EyeOff v-else :size="16" />
                </button>
              </div>
            </div>
            <div class="space-y-1">
              <label class="block text-xs font-bold text-muted">Mật khẩu mới</label>
              <div class="relative">
                <input v-model="passwordForm.newPassword" :type="showNew ? 'text' : 'password'" placeholder="Tối thiểu 8 ký tự" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm pr-10 focus:outline-none focus:border-(--lg-primary)" />
                <button @click="showNew = !showNew" class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading">
                  <Eye v-if="!showNew" :size="16" /><EyeOff v-else :size="16" />
                </button>
              </div>
            </div>
            <div class="space-y-1">
              <label class="block text-xs font-bold text-muted">Xác nhận mật khẩu mới</label>
              <div class="relative">
                <input v-model="passwordForm.confirmPassword" :type="showConfirm ? 'text' : 'password'" placeholder="Nhập lại mật khẩu mới" class="w-full px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm pr-10 focus:outline-none focus:border-(--lg-primary)" />
                <button @click="showConfirm = !showConfirm" class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading">
                  <Eye v-if="!showConfirm" :size="16" /><EyeOff v-else :size="16" />
                </button>
              </div>
            </div>
            <div v-if="passwordError" class="text-xs text-(--color-danger-text) bg-(--color-danger-bg) p-3 rounded-lg">{{ passwordError }}</div>
            <div v-if="passwordSuccess" class="text-xs text-(--color-success-text) bg-(--color-success-bg) p-3 rounded-lg">{{ passwordSuccess }}</div>
            <button @click="changePassword" class="px-4 py-2 bg-(--surface-input) border border-input text-heading text-sm font-bold rounded-lg hover:bg-(--surface-input-hover) transition-colors">Cập nhật mật khẩu</button>
          </div>
        </div>
      </div>

      <div class="space-y-6">
        <div class="surface-card border border-card rounded-2xl p-6 shadow-sm">
          <h2 class="text-lg font-bold text-heading mb-4">Hoạt động gần đây</h2>
          <div class="space-y-3">
            <div v-for="log in recentActivity" :key="log.time" class="flex gap-3">
              <div class="flex flex-col items-center">
                <div class="h-2 w-2 rounded-full bg-(--lg-primary) mt-2" />
                <div class="flex-1 w-px bg-default" />
              </div>
              <div class="flex-1 pb-3">
                <p class="text-xs font-semibold text-heading">{{ log.action }}</p>
                <p class="text-[10px] text-muted">{{ log.time }}</p>
              </div>
            </div>
            <div class="flex gap-3">
              <div class="h-2 w-2 rounded-full bg-(--lg-primary) mt-2" />
              <div class="flex-1">
                <p class="text-xs font-bold text-heading">Đã tạo tài khoản</p>
                <p class="text-[10px] text-muted">{{ profile.ngayTao }}</p>
              </div>
            </div>
          </div>
        </div>

        <div class="surface-card border border-card rounded-2xl p-6 shadow-sm">
          <h2 class="text-lg font-bold text-heading mb-3">Phiên đăng nhập</h2>
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Monitor :size="16" class="text-muted" />
                <span class="text-sm text-heading font-medium">Trình duyệt này</span>
              </div>
              <span class="text-[10px] font-bold text-(--color-success-text) bg-(--color-success-bg) px-2 py-0.5 rounded-full">Hiện tại</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </template>
  </div>
</template>

<script setup>
import { computed, ref, reactive, onMounted } from 'vue'
import {
  Edit2, ShieldCheck, CheckCircle2, Eye, EyeOff,
  Monitor, AlertCircle
} from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import { usePopupStore } from '@/stores/popup'
import { useAuthStore } from '@/stores/auth'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const popup = usePopupStore()
const auth = useAuthStore()
const loading = ref(false)
const error = ref(null)

const profile = reactive({
  maNguoiDung: null,
  hoTen: '',
  email: '',
  soDienThoai: '',
  vaiTroChinh: '',
  maDonVi: null,
  tenDonVi: '',
  trangThai: '',
  ngayTao: '',
  initials: '',
})

const profileFields = computed(() => [
  { key: 'hoTen', label: 'Họ và tên', value: profile.hoTen, editable: true },
  { key: 'email', label: 'Email', value: profile.email, editable: true },
  { key: 'soDienThoai', label: 'Số điện thoại', value: profile.soDienThoai, editable: true },
  { key: 'tenDonVi', label: 'Đơn vị', value: profile.tenDonVi, editable: false },
])

const editing = ref(false)
const editForm = reactive({ hoTen: '', email: '', soDienThoai: '' })

function startEdit() {
  editForm.hoTen = profile.hoTen
  editForm.email = profile.email
  editForm.soDienThoai = profile.soDienThoai
  editing.value = true
}

function cancelEdit() { editing.value = false }
async function saveProfile() {
  try {
    const response = await apiRequest('/api/account/profile', {
      method: 'PUT',
      body: JSON.stringify({
        hoTen: editForm.hoTen,
        email: editForm.email,
        soDienThoai: editForm.soDienThoai,
      }),
    })
    applyProfile(unwrapApiData(response))
    editing.value = false
    popup.success('Thành công', 'Thông tin cá nhân đã được cập nhật.')
  } catch (e) {
    popup.error('Lỗi', e.message || 'Không thể cập nhật hồ sơ.')
  }
}

const showOld = ref(false)
const showNew = ref(false)
const showConfirm = ref(false)
const passwordForm = reactive({ oldPassword: '', newPassword: '', confirmPassword: '' })
const passwordError = ref('')
const passwordSuccess = ref('')

async function changePassword() {
  passwordError.value = ''
  passwordSuccess.value = ''
  if (!passwordForm.oldPassword || !passwordForm.newPassword || !passwordForm.confirmPassword) {
    passwordError.value = 'Vui lòng điền đầy đủ các trường.'
    return
  }
  if (passwordForm.newPassword.length < 8) {
    passwordError.value = 'Mật khẩu mới phải có tối thiểu 8 ký tự.'
    return
  }
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    passwordError.value = 'Mật khẩu mới và xác nhận không khớp.'
    return
  }
  try {
    await apiRequest('/api/account/change-password', {
      method: 'PUT',
      body: JSON.stringify({
        currentPassword: passwordForm.oldPassword,
        newPassword: passwordForm.newPassword,
      }),
    })
    passwordSuccess.value = 'Đổi mật khẩu thành công!'
    passwordForm.oldPassword = ''
    passwordForm.newPassword = ''
    passwordForm.confirmPassword = ''
  } catch (e) {
    passwordError.value = e.message || 'Không thể đổi mật khẩu.'
  }
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const response = await apiRequest('/api/account/me')
    applyProfile(unwrapApiData(response))
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

function applyProfile(source = {}) {
  profile.maNguoiDung = source.maNguoiDung ?? source.id ?? auth.user?.id ?? null
  profile.hoTen = source.hoTen ?? source.fullName ?? auth.displayName ?? ''
  profile.email = source.email ?? auth.user?.email ?? ''
  profile.soDienThoai = source.soDienThoai ?? source.phone ?? ''
  profile.vaiTroChinh = source.vaiTroChinh ?? source.role ?? auth.user?.role ?? ''
  profile.maDonVi = source.maDonVi ?? null
  profile.tenDonVi = source.tenDonVi ?? source.donVi ?? ''
  profile.trangThai = source.trangThai ?? 'hoat_dong'
  profile.ngayTao = source.ngayTao ? new Date(source.ngayTao).toLocaleDateString('vi-VN') : ''
  profile.initials = auth.initials || (profile.hoTen || profile.email || 'BGH').slice(0, 2).toUpperCase()
}

const recentActivity = []
</script>
