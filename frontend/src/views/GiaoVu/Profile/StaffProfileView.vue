<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/services/apiClient'
import { 
  User, 
  Mail, 
  Phone, 
  MapPin, 
  ShieldCheck, 
  KeyRound, 
  Clock, 
  Edit3,
  LogOut,
  ChevronRight,
  ShieldAlert,
  Camera,
  Save,
  X,
  Lock,
  Eye,
  EyeOff,
  AlertCircle,
  CheckCircle2
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

const router = useRouter()
const authStore = useAuthStore()

// ── Mock Data ────────────────────────────────────────────────
const profile = ref({
  name: 'Phạm Minh Đức',
  code: 'GV-0452',
  role: 'Giáo vụ khoa CNTT',
  email: 'ducpm@university.edu.vn',
  phone: '0987.654.321',
  campus: 'Cơ sở chính (Hồ Chí Minh)',
  joinDate: '15/08/2021',
  avatar: null
})

const editForm = ref({ ...profile.value })
const editing = ref(false)

const loginHistory = ref([
  { id: 1, device: 'Chrome / Windows 11', ip: '14.232.xxx.xxx', time: '13/05/2026 08:30', status: 'current' },
  { id: 2, device: 'Safari / iPhone 15', ip: '112.xxx.xxx.xxx', time: '12/05/2026 21:15', status: 'previous' },
  { id: 3, device: 'Chrome / Windows 11', ip: '14.232.xxx.xxx', time: '12/05/2026 09:00', status: 'previous' },
])

// ── Change Password ──────────────────────────────────────────
const showChangePwd = ref(false)
const changingPwd = ref(false)
const changePwdError = ref('')
const changePwdSuccess = ref(false)
const showOldPwd = ref(false)
const showNewPwd = ref(false)
const showConfirmPwd = ref(false)
const changePwdForm = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: '',
})

function openChangePwd() {
  changePwdForm.value = { oldPassword: '', newPassword: '', confirmPassword: '' }
  changePwdError.value = ''
  changePwdSuccess.value = false
  showOldPwd.value = false
  showNewPwd.value = false
  showConfirmPwd.value = false
  showChangePwd.value = true
}

function closeChangePwd() {
  showChangePwd.value = false
}

async function handleChangePwd() {
  changePwdError.value = ''
  changePwdSuccess.value = false

  const { oldPassword, newPassword, confirmPassword } = changePwdForm.value

  if (!oldPassword || !newPassword || !confirmPassword) {
    changePwdError.value = 'Vui lòng nhập đầy đủ các trường.'
    return
  }

  if (newPassword.length < 8) {
    changePwdError.value = 'Mật khẩu mới phải có ít nhất 8 ký tự.'
    return
  }

  if (newPassword !== confirmPassword) {
    changePwdError.value = 'Mật khẩu xác nhận không khớp.'
    return
  }

  changingPwd.value = true
  try {
    await authApi.changePassword({
      oldPassword,
      newPassword,
      confirmPassword,
    })
    changePwdSuccess.value = true
    setTimeout(() => closeChangePwd(), 2000)
  } catch (err) {
    changePwdError.value = err.message || 'Đổi mật khẩu thất bại.'
  } finally {
    changingPwd.value = false
  }
}

function toggleEdit() {
  if (editing.value) {
    editForm.value = { ...profile.value }
  }
  editing.value = !editing.value
}

function saveProfile() {
  profile.value = { ...editForm.value }
  editing.value = false
}

function handleLogout() {
  authStore.logout()
  router.push('/login')
}
</script>

<template>
  <PageContainer 
    title="Hồ sơ cá nhân" 
    subtitle="Thông tin tài khoản và bảo mật"
  >
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">
      
      <!-- ── Left: Profile Card ── -->
      <div class="space-y-4">
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
           <div class="relative inline-block mb-4">
              <div class="h-24 w-24 rounded-3xl bg-[var(--color-info-bg)] border border-[var(--color-info-text)]/20 p-0.5 transition-transform">
                 <div class="h-full w-full rounded-[22px] surface-card flex items-center justify-center overflow-hidden relative">
                     <User v-if="!profile.avatar" :size="40" class="text-placeholder" />
                    <img v-else :src="profile.avatar" class="h-full w-full object-cover" />
                    
                    <button class="absolute inset-0 surface-modal opacity-0 group-hover:opacity-100 flex items-center justify-center text-inverse transition-opacity">
                       <Camera :size="18" />
                    </button>
                 </div>
              </div>
           </div>

            <h3 class="text-lg font-semibold text-heading leading-tight">{{ profile.name }}</h3>
            <p class="text-[10px] font-semibold text-link uppercase tracking-[0.2em] mt-1">{{ profile.code }}</p>

           <div class="mt-5 flex flex-col gap-3">
               <div class="flex items-center gap-2 p-3 surface-solid rounded-xl border-default">
                  <ShieldCheck :size="14" class="text-body" />
                  <div class="text-left">
                      <p class="text-[10px] font-semibold text-label">Chức danh</p>
                      <p class="text-[11px] font-bold text-heading">{{ profile.role }}</p>
                  </div>
               </div>
               <div class="flex items-center gap-2 p-3 surface-solid rounded-xl border-default">
                  <MapPin :size="14" class="text-body" />
                  <div class="text-left">
                      <p class="text-[10px] font-semibold text-label">Cơ sở làm việc</p>
                     <p class="text-[11px] font-bold text-heading">{{ profile.campus }}</p>
                 </div>
              </div>
           </div>

           <button @click="toggleEdit" class="w-full mt-5 lg-button-primary py-3 text-xs font-semibold flex items-center justify-center gap-2">
               <component :is="editing ? X : Edit3" :size="14" /> {{ editing ? 'Huỷ' : 'Chỉnh sửa hồ sơ' }}
            </button>
        </div>

        <!-- System Stats -->
        <div class="surface-card border border-card rounded-2xl p-3">
           <div class="grid grid-cols-2 gap-3">
               <div class="p-3 surface-solid rounded-xl border border-default shadow-sm">
                   <p class="text-[10px] font-semibold text-label">Ngày tham gia</p>
                  <p class="text-xs font-semibold text-heading mt-0.5">{{ profile.joinDate }}</p>
               </div>
               <div class="p-3 surface-solid rounded-xl border border-default shadow-sm">
                   <p class="text-[10px] font-semibold text-label">Trạng thái</p>
                  <p class="text-xs font-semibold text-[var(--color-success-text)] mt-0.5 flex items-center gap-1.5">
                     <span class="h-1.5 w-1.5 rounded-full bg-[var(--color-success-text)]"></span> Active
                 </p>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Detailed Info & Security ── -->
      <div class="xl:col-span-2 space-y-4">
        
         <!-- Contact & Security Section -->
         <div class="surface-card border border-card rounded-2xl overflow-hidden">
             <div class="p-4 border-b border-default flex items-center justify-between">
                <h4 class="text-xs font-semibold text-heading uppercase tracking-wide">Thông tin liên hệ & Bảo mật</h4>
                <ShieldAlert :size="16" class="text-[var(--color-warning-text)]" />
            </div>

            <div class="p-4 space-y-5">
               <!-- Grid Info -->
               <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                  <div class="space-y-3">
                      <div class="flex items-center gap-3">
                       <div class="h-8 w-8 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                             <Mail :size="16" />
                          </div>
                          <div class="flex-1">
                              <p class="text-[10px] font-semibold text-label">Email học vụ</p>
                             <input v-if="editing" v-model="editForm.email" type="email" class="w-full text-xs font-bold text-heading bg-transparent border-b border-link outline-none" />
                             <p v-else class="text-xs font-bold text-heading">{{ profile.email }}</p>
                          </div>
                       </div>
                       <div class="flex items-center gap-3">
                          <div class="h-8 w-8 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                             <Phone :size="16" />
                          </div>
                          <div class="flex-1">
                              <p class="text-[10px] font-semibold text-label">Số điện thoại</p>
                             <input v-if="editing" v-model="editForm.phone" type="text" class="w-full text-xs font-bold text-heading bg-transparent border-b border-link outline-none" />
                             <p v-else class="text-xs font-bold text-heading">{{ profile.phone }}</p>
                         </div>
                      </div>
                      <div v-if="editing" class="md:col-span-2">
                          <button @click="saveProfile" class="w-full lg-button-primary py-2 text-xs font-semibold flex items-center justify-center gap-2">
                             <Save :size="14" /> Lưu thay đổi
                         </button>
                      </div>
                  </div>

                  <div class="p-3 surface-solid rounded-2xl border border-default">
                      <div class="flex items-start justify-between mb-3">
                         <div class="h-8 w-8 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-link shadow-sm">
                            <KeyRound :size="16" />
                         </div>
                           <button @click="openChangePwd" class="text-[10px] font-semibold text-link lg-button-ghost px-2 py-1 rounded-lg transition-all">Thay đổi</button>
                      </div>
                      <h5 class="text-xs font-semibold text-heading mb-0.5">Mật khẩu tài khoản</h5>
                       <p class="text-[10px] text-body font-medium">Đặt mật khẩu mạnh, không dùng lại mật khẩu cũ.</p>
                  </div>
               </div>

               <!-- Login History -->
               <div>
                    <h4 class="text-[10px] font-semibold text-label mb-3 flex items-center gap-2">
                       <Clock :size="14" /> Lịch sử đăng nhập
                   </h4>
                   <div class="space-y-2">
                      <div v-for="log in loginHistory" :key="log.id" class="flex items-center justify-between p-3 surface-solid rounded-xl border border-default group hover:border-[var(--border-input-focus)] transition-all">
                         <div class="flex items-center gap-3">
                            <div :class="['h-6 w-6 rounded-full flex items-center justify-center', log.status === 'current' ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)]' : 'surface-solid text-placeholder']">
                               <ShieldCheck v-if="log.status === 'current'" :size="12" />
                               <ChevronRight v-else :size="12" />
                            </div>
                            <div>
                               <p class="text-[11px] font-semibold text-heading">{{ log.device }}</p>
                                <p class="text-[10px] text-label">IP: {{ log.ip }} • {{ log.time }}</p>
                            </div>
                         </div>
                          <span v-if="log.status === 'current'" class="text-[10px] font-semibold text-[var(--color-success-text)] bg-[var(--color-success-bg)] px-1.5 py-0.5 rounded-lg border border-[var(--color-success-text)]/20">Hiện tại</span>
                     </div>
                  </div>
               </div>
            </div>
         </div>

        <!-- Danger Zone -->
        <div class="surface-card border border-[var(--color-danger-text)]/20 rounded-2xl p-4 bg-[var(--color-danger-bg)]">
            <div class="flex items-center justify-between">
               <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-xl bg-[var(--surface-card)] flex items-center justify-center text-[var(--color-danger-text)] shadow-sm">
                     <LogOut :size="16" />
                  </div>
                  <div>
                     <h4 class="text-xs font-semibold text-heading">Đăng xuất tài khoản</h4>
                      <p class="text-[10px] text-[var(--color-danger-text)] mt-0.5 font-medium">Thoát khỏi hệ thống</p>
                  </div>
               </div>
                <button @click="handleLogout" class="px-3 py-2 lg-btn-danger rounded-xl text-[10px] font-semibold transition-all">Đăng xuất</button>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>

  <!-- ── Change Password Modal ── -->
  <Teleport to="body">
    <div v-if="showChangePwd" class="fixed inset-0 z-50 flex items-center justify-center p-4" @click.self="closeChangePwd">
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
      <div class="relative surface-card border border-card rounded-2xl shadow-2xl w-full max-w-md p-6 animate-in">
        <div class="flex items-center justify-between mb-5">
          <div class="flex items-center gap-2.5">
            <div class="h-9 w-9 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-link shadow-sm">
              <KeyRound :size="18" />
            </div>
            <div>
              <h3 class="text-sm font-bold text-heading">Đổi mật khẩu</h3>
              <p class="text-[10px] text-label font-semibold">Nhập mật khẩu hiện tại và mật khẩu mới</p>
            </div>
          </div>
          <button @click="closeChangePwd" class="h-7 w-7 rounded-lg surface-solid flex items-center justify-center text-placeholder hover:text-heading transition-colors">
            <X :size="15" />
          </button>
        </div>

        <div v-if="changePwdSuccess" class="flex flex-col items-center justify-center py-6 gap-3">
          <div class="h-14 w-14 rounded-full bg-[var(--color-success-bg)] flex items-center justify-center">
            <CheckCircle2 :size="28" class="text-[var(--color-success-text)]" />
          </div>
          <p class="text-sm font-bold text-heading">Đổi mật khẩu thành công!</p>
          <p class="text-[11px] text-body font-medium">Mật khẩu của bạn đã được cập nhật.</p>
        </div>

        <div v-else class="space-y-4">
          <!-- Old Password -->
          <div class="space-y-1.5">
            <label class="text-[10px] font-bold text-label uppercase tracking-widest">Mật khẩu hiện tại</label>
            <div class="flex items-center gap-2.5 h-10 px-3 rounded-xl border border-input surface-input text-body text-xs font-semibold focus-within:border-[var(--border-input-focus)] focus-within:shadow-[0_0_0_3px_var(--border-focus-ring)] transition-all">
              <Lock :size="15" class="text-placeholder shrink-0" />
              <input v-model="changePwdForm.oldPassword" :type="showOldPwd ? 'text' : 'password'" placeholder="Nhập mật khẩu hiện tại" class="w-full bg-transparent outline-none text-heading placeholder:text-placeholder" />
              <button @click="showOldPwd = !showOldPwd" class="text-placeholder hover:text-heading shrink-0">
                <component :is="showOldPwd ? EyeOff : Eye" :size="15" />
              </button>
            </div>
          </div>

          <!-- New Password -->
          <div class="space-y-1.5">
            <label class="text-[10px] font-bold text-label uppercase tracking-widest">Mật khẩu mới</label>
            <div class="flex items-center gap-2.5 h-10 px-3 rounded-xl border border-input surface-input text-body text-xs font-semibold focus-within:border-[var(--border-input-focus)] focus-within:shadow-[0_0_0_3px_var(--border-focus-ring)] transition-all">
              <Lock :size="15" class="text-placeholder shrink-0" />
              <input v-model="changePwdForm.newPassword" :type="showNewPwd ? 'text' : 'password'" placeholder="Nhập mật khẩu mới" class="w-full bg-transparent outline-none text-heading placeholder:text-placeholder" />
              <button @click="showNewPwd = !showNewPwd" class="text-placeholder hover:text-heading shrink-0">
                <component :is="showNewPwd ? EyeOff : Eye" :size="15" />
              </button>
            </div>
          </div>

          <!-- Confirm Password -->
          <div class="space-y-1.5">
            <label class="text-[10px] font-bold text-label uppercase tracking-widest">Xác nhận mật khẩu mới</label>
            <div class="flex items-center gap-2.5 h-10 px-3 rounded-xl border border-input surface-input text-body text-xs font-semibold focus-within:border-[var(--border-input-focus)] focus-within:shadow-[0_0_0_3px_var(--border-focus-ring)] transition-all">
              <Lock :size="15" class="text-placeholder shrink-0" />
              <input v-model="changePwdForm.confirmPassword" :type="showConfirmPwd ? 'text' : 'password'" placeholder="Nhập lại mật khẩu mới" class="w-full bg-transparent outline-none text-heading placeholder:text-placeholder" />
              <button @click="showConfirmPwd = !showConfirmPwd" class="text-placeholder hover:text-heading shrink-0">
                <component :is="showConfirmPwd ? EyeOff : Eye" :size="15" />
              </button>
            </div>
          </div>

          <!-- Error -->
          <div v-if="changePwdError" class="flex items-start gap-2 p-3 rounded-xl bg-[var(--color-danger-bg)] border border-[var(--color-danger-text)]/20">
            <AlertCircle :size="14" class="text-[var(--color-danger-text)] shrink-0 mt-0.5" />
            <p class="text-[11px] font-semibold text-[var(--color-danger-text)]">{{ changePwdError }}</p>
          </div>

          <!-- Password hint -->
          <div class="flex items-start gap-2 p-3 rounded-xl bg-[var(--color-warning-bg)] border border-[var(--color-warning-text)]/20">
            <AlertCircle :size="14" class="text-[var(--color-warning-text)] shrink-0 mt-0.5" />
            <p class="text-[10px] font-semibold text-[var(--color-warning-text)]">Mật khẩu mới phải có ít nhất 8 ký tự.</p>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-end gap-3 pt-2">
            <button @click="closeChangePwd" class="px-4 py-2 rounded-xl text-[10px] font-bold text-body surface-solid border border-default hover:text-heading transition-all">Huỷ</button>
            <button @click="handleChangePwd" :disabled="changingPwd" class="px-4 py-2 rounded-xl text-[10px] font-bold text-inverse bg-link hover:opacity-90 disabled:opacity-50 transition-all flex items-center gap-2">
              <span v-if="changingPwd" class="h-3 w-3 rounded-full border-2 border-inverse border-t-transparent animate-spin"></span>
              {{ changingPwd ? 'Đang xử lý...' : 'Cập nhật mật khẩu' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.animate-in {
  animation: fadeScaleIn 0.2s ease-out;
}
@keyframes fadeScaleIn {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
