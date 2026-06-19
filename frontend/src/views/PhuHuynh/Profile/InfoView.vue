<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import {
  User,
  ChevronLeft,
  Mail,
  Phone,
  MapPin,
  Lock,
  Edit2,
  Save,
  Eye,
  EyeOff,
  Calendar,
  CreditCard
} from 'lucide-vue-next'
import { parentProfile } from '@/components/PhuHuynh/data/parentData.js'
import { usePopupStore } from '@/stores/popup'

const router = useRouter()
const popupStore = usePopupStore()

// State chỉnh sửa thông tin hồ sơ
const isEditing = ref(false)
const profileForm = reactive({
  phone: parentProfile.phone,
  email: parentProfile.email,
  permanentAddress: parentProfile.permanentAddress,
  temporaryAddress: parentProfile.temporaryAddress
})

function saveProfile() {
  // Thực tế lưu vào data mock
  parentProfile.phone = profileForm.phone
  parentProfile.email = profileForm.email
  parentProfile.permanentAddress = profileForm.permanentAddress
  parentProfile.temporaryAddress = profileForm.temporaryAddress
  
  isEditing.value = false
  popupStore.success('Thành công', 'Đã cập nhật thông tin liên lạc của phụ huynh.')
}

function cancelEdit() {
  profileForm.phone = parentProfile.phone
  profileForm.email = parentProfile.email
  profileForm.permanentAddress = parentProfile.permanentAddress
  profileForm.temporaryAddress = parentProfile.temporaryAddress
  isEditing.value = false
}

// State đổi mật khẩu
const passForm = reactive({
  currentPass: '',
  newPass: '',
  confirmPass: ''
})
const showPass = reactive({
  current: false,
  new: false,
  confirm: false
})

const passwordErrors = reactive({
  currentPass: '',
  newPass: '',
  confirmPass: ''
})

function validatePasswordForm() {
  let isValid = true
  passwordErrors.currentPass = ''
  passwordErrors.newPass = ''
  passwordErrors.confirmPass = ''

  if (!passForm.currentPass) {
    passwordErrors.currentPass = 'Vui lòng nhập mật khẩu hiện tại.'
    isValid = false
  }
  if (!passForm.newPass) {
    passwordErrors.newPass = 'Vui lòng nhập mật khẩu mới.'
    isValid = false
  } else if (passForm.newPass.length < 6) {
    passwordErrors.newPass = 'Mật khẩu mới phải từ 6 ký tự trở lên.'
    isValid = false
  }
  if (!passForm.confirmPass) {
    passwordErrors.confirmPass = 'Vui lòng xác nhận mật khẩu mới.'
    isValid = false
  } else if (passForm.newPass !== passForm.confirmPass) {
    passwordErrors.confirmPass = 'Xác nhận mật khẩu mới không trùng khớp.'
    isValid = false
  }

  return isValid
}

function changePassword() {
  if (!validatePasswordForm()) {
    popupStore.error('Lỗi nhập liệu', 'Vui lòng kiểm tra lại biểu mẫu đổi mật khẩu.')
    return
  }

  // Giả lập đổi mật khẩu thành công
  popupStore.success('Đổi mật khẩu thành công', 'Mật khẩu tài khoản phụ huynh đã được cập nhật.')
  
  // Reset form
  passForm.currentPass = ''
  passForm.newPass = ''
  passForm.confirmPass = ''
}

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TIÊU ĐỀ ── -->
    <div class="flex items-center gap-2">
      <button
        @click="goBack"
        class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
        title="Quay lại"
      >
        <ChevronLeft :size="18" />
      </button>
      <div>
        <h2 class="text-lg font-bold text-heading flex items-center gap-2">
          <User :size="20" class="text-orange-600" />
          Hồ sơ phụ huynh
        </h2>
        <p class="text-xs text-body">Xem thông tin cá nhân và cập nhật mật khẩu, địa chỉ liên lạc</p>
      </div>
    </div>

    <!-- ── GRID CHÍNH ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Cột 1: Thông tin cố định (Read-only) -->
      <div class="lg-card-glass p-5 space-y-5 flex flex-col items-center text-center h-fit">
        <div class="h-20 w-20 flex items-center justify-center rounded-full bg-gradient-to-tr from-orange-600 to-amber-500 text-3xl font-extrabold text-white shadow-md">
          {{ parentProfile.name.split(' ').pop().charAt(0) }}
        </div>
        
        <div class="space-y-1 w-full">
          <h3 class="text-sm font-bold text-heading">{{ parentProfile.name }}</h3>
          <p class="text-[10px] text-muted font-bold tracking-wide uppercase">Tài khoản Phụ huynh (ParentLink)</p>
        </div>

        <div class="w-full border-t border-card pt-4 space-y-3.5 text-left text-xs">
          <div class="flex items-center justify-between">
            <span class="text-muted font-semibold flex items-center gap-1.5">
              <Calendar :size="13" />
              Ngày sinh
            </span>
            <span class="font-bold text-heading">{{ parentProfile.dob }}</span>
          </div>

          <div class="flex items-center justify-between">
            <span class="text-muted font-semibold flex items-center gap-1.5">
              <CreditCard :size="13" />
              Số CCCD
            </span>
            <span class="font-bold text-heading">{{ parentProfile.cccd }}</span>
          </div>

          <div class="flex items-center justify-between">
            <span class="text-muted font-semibold flex items-center gap-1.5">
              <Lock :size="13" />
              Vai trò hệ thống
            </span>
            <span class="font-bold text-orange-600">Phụ Huynh</span>
          </div>
        </div>
      </div>

      <!-- Cột 2: Thông tin liên lạc (Editable) -->
      <div class="lg-card-glass p-5 lg:col-span-2 space-y-5">
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide flex items-center gap-1.5">
            <Mail :size="15" class="text-orange-600" />
            Thông tin liên lạc
          </h3>
          
          <button
            v-if="!isEditing"
            @click="isEditing = true"
            class="text-xs font-bold text-orange-600 hover:text-orange-700 flex items-center gap-1 transition"
          >
            <Edit2 :size="12" /> Chỉnh sửa
          </button>
        </div>

        <form @submit.prevent="saveProfile" class="space-y-4">
          <!-- Số điện thoại -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Số điện thoại liên hệ khẩn cấp</label>
            <div class="relative">
              <input
                v-model="profileForm.phone"
                :disabled="!isEditing"
                type="text"
                class="surface-input border-card w-full pl-9 pr-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20 disabled:opacity-65"
              />
              <Phone :size="13" class="absolute left-3.5 top-3 text-muted" />
            </div>
          </div>

          <!-- Email nhận thông báo -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Email nhận thông báo kết quả</label>
            <div class="relative">
              <input
                v-model="profileForm.email"
                :disabled="!isEditing"
                type="email"
                class="surface-input border-card w-full pl-9 pr-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20 disabled:opacity-65"
              />
              <Mail :size="13" class="absolute left-3.5 top-3 text-muted" />
            </div>
          </div>

          <!-- Địa chỉ thường trú -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Địa chỉ thường trú</label>
            <div class="relative">
              <input
                v-model="profileForm.permanentAddress"
                :disabled="!isEditing"
                type="text"
                class="surface-input border-card w-full pl-9 pr-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20 disabled:opacity-65"
              />
              <MapPin :size="13" class="absolute left-3.5 top-3 text-muted" />
            </div>
          </div>

          <!-- Địa chỉ tạm trú -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Địa chỉ tạm trú / Nơi ở hiện tại</label>
            <div class="relative">
              <input
                v-model="profileForm.temporaryAddress"
                :disabled="!isEditing"
                type="text"
                class="surface-input border-card w-full pl-9 pr-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20 disabled:opacity-65"
              />
              <MapPin :size="13" class="absolute left-3.5 top-3 text-muted" />
            </div>
          </div>

          <!-- Actions button khi edit -->
          <div v-if="isEditing" class="pt-3 flex gap-3 justify-end border-t border-card">
            <button
              type="button"
              @click="cancelEdit"
              class="px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:bg-[var(--surface-card-hover)] transition"
            >
              Hủy bỏ
            </button>
            <button
              type="submit"
              class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl text-xs font-bold flex items-center gap-1 transition"
            >
              <Save :size="12" /> Lưu thay đổi
            </button>
          </div>
        </form>
      </div>

      <!-- Khung Đổi mật khẩu (Full width below) -->
      <div class="lg-card-glass p-5 lg:col-span-3 space-y-5">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card flex items-center gap-1.5">
          <Lock :size="15" class="text-orange-600" />
          Thay đổi mật khẩu tài khoản
        </h3>

        <form @submit.prevent="changePassword" class="space-y-4">
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Mật khẩu hiện tại</label>
            <div class="relative">
              <input
                v-model="passForm.currentPass"
                :type="showPass.current ? 'text' : 'password'"
                class="surface-input border-card w-full pl-3 pr-10 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
                placeholder="••••••••"
              />
              <button
                type="button"
                @click="showPass.current = !showPass.current"
                class="absolute right-3 top-2.5 text-muted hover:text-orange-600"
              >
                <Eye v-if="!showPass.current" :size="13" />
                <EyeOff v-else :size="13" />
              </button>
            </div>
            <p v-if="passwordErrors.currentPass" class="text-[10px] font-semibold text-red-600">{{ passwordErrors.currentPass }}</p>
          </div>

          <!-- Mật khẩu mới -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Mật khẩu mới (Tối thiểu 6 ký tự)</label>
            <div class="relative">
              <input
                v-model="passForm.newPass"
                :type="showPass.new ? 'text' : 'password'"
                class="surface-input border-card w-full pl-3 pr-10 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
                placeholder="••••••••"
              />
              <button
                type="button"
                @click="showPass.new = !showPass.new"
                class="absolute right-3 top-2.5 text-muted hover:text-orange-600"
              >
                <Eye v-if="!showPass.new" :size="13" />
                <EyeOff v-else :size="13" />
              </button>
            </div>
            <p v-if="passwordErrors.newPass" class="text-[10px] font-semibold text-red-600">{{ passwordErrors.newPass }}</p>
          </div>

          <!-- Xác nhận mật khẩu mới -->
          <div class="space-y-1">
            <label class="text-[10px] font-bold text-label uppercase tracking-wide">Xác nhận mật khẩu mới</label>
            <div class="relative">
              <input
                v-model="passForm.confirmPass"
                :type="showPass.confirm ? 'text' : 'password'"
                class="surface-input border-card w-full pl-3 pr-10 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
                placeholder="••••••••"
              />
              <button
                type="button"
                @click="showPass.confirm = !showPass.confirm"
                class="absolute right-3 top-2.5 text-muted hover:text-orange-600"
              >
                <Eye v-if="!showPass.confirm" :size="13" />
                <EyeOff v-else :size="13" />
              </button>
            </div>
            <p v-if="passwordErrors.confirmPass" class="text-[10px] font-semibold text-red-600">{{ passwordErrors.confirmPass }}</p>
          </div>

          </div><!-- /grid -->

          <!-- Nút đổi mật khẩu -->
          <div class="flex justify-end pt-2 border-t border-card">
            <button
              type="submit"
              class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-5 py-2 rounded-xl text-xs font-bold transition"
            >
              Cập nhật mật khẩu mới
            </button>
          </div>
        </form>
      </div>

    </div>
  </div>
</template>

<style scoped>
</style>
