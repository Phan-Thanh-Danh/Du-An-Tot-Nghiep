<script setup>
/**
 * ProfileView.vue - Super Admin
 * Giao diện quản lý thông tin cá nhân, bảo mật, đổi mật khẩu và nhật ký đăng nhập dành riêng cho Super Admin.
 * Tuân thủ thiết kế liquid-glass.css, tối ưu trải nghiệm và ràng buộc nghiệp vụ.
 */
import { ref } from 'vue'
import {
  User,
  ShieldCheck,
  History,
  Lock,
  Key,
  Save,
  CheckCircle,
  AlertTriangle,
  Globe,
  Cpu,
  Eye,
  EyeOff
} from 'lucide-vue-next'

// --- Mock Data của Super Admin ---
const adminProfile = ref({
  fullName: 'Super Admin',
  email: 'admin@edu.vn',
  phone: '0901234567',
  role: 'SuperAdmin',
  scope: 'Toàn hệ thống (All Campuses)',
  status: 'Active',
  avatar: ''
})

// --- Mock Nhật ký đăng nhập ---
const loginLogs = ref([
  { id: 1, time: '2026-06-24 18:30:22', ip: '14.162.245.10', location: 'Hà Nội (VN)', device: 'Chrome 125, Windows 11', status: 'Success' },
  { id: 2, time: '2026-06-23 10:15:00', ip: '171.244.10.89', location: 'TP. Hồ Chí Minh (VN)', device: 'Firefox 126, Windows 11', status: 'Success' },
  { id: 3, time: '2026-06-22 08:45:12', ip: '103.21.140.22', location: 'Singapore (SG)', device: 'Chrome 125, MacOS 14.5', status: 'Success' },
  { id: 4, time: '2026-06-21 16:20:00', ip: '198.51.100.4', location: 'California (US)', device: 'Python-requests/2.31', status: 'Failed' },
  { id: 5, time: '2026-06-20 09:22:15', ip: '115.79.136.21', location: 'Vũng Tàu (VN)', device: 'Chrome Mobile 125, Android 13', status: 'Success' }
])

// --- State Điều khiển ---
const activeTab = ref('profile') // 'profile', 'security', 'logs'
const tabs = [
  { id: 'profile', label: 'Thông tin cá nhân', icon: User },
  { id: 'security', label: 'Bảo mật & Đổi mật khẩu', icon: ShieldCheck },
  { id: 'logs', label: 'Nhật ký đăng nhập', icon: History }
]

// --- Form Edit thông tin ---
const editForm = ref({
  fullName: adminProfile.value.fullName,
  email: adminProfile.value.email,
  phone: adminProfile.value.phone
})

// --- Form Đổi mật khẩu ---
const isPasswordModalOpen = ref(false)
const oldPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const showOldPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)

// --- State 2FA ---
const is2FaEnabled = ref(true)

// --- Toast Thông báo ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success') // 'success' | 'error' | 'info'

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => {
    showToast.value = false
  }, 4000)
}

// --- Xử lý Cập nhật thông tin ---
const updateProfile = () => {
  if (!editForm.value.fullName.trim()) {
    triggerToast('Họ và tên không được để trống!', 'error')
    return
  }
  if (!editForm.value.email.trim()) {
    triggerToast('Email không được để trống!', 'error')
    return
  }

  adminProfile.value.fullName = editForm.value.fullName
  adminProfile.value.email = editForm.value.email
  adminProfile.value.phone = editForm.value.phone

  // Ghi nhật ký audit giả lập
  const nowStr = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  loginLogs.value.unshift({
    id: Date.now(),
    time: nowStr,
    ip: '14.162.245.10',
    location: 'Hà Nội (VN)',
    device: 'Chrome 125, Windows 11',
    status: 'Profile_Updated'
  })

  triggerToast('Đã cập nhật thông tin hồ sơ thành công! (Ghi Log Audit)', 'success')
}

// --- Xử lý Đổi mật khẩu ---
const openPasswordModal = () => {
  oldPassword.value = ''
  newPassword.value = ''
  confirmPassword.value = ''
  showOldPassword.value = false
  showNewPassword.value = false
  showConfirmPassword.value = false
  isPasswordModalOpen.value = true
}

const confirmChangePassword = () => {
  if (!oldPassword.value) {
    triggerToast('Vui lòng nhập mật khẩu hiện tại!', 'error')
    return
  }
  if (newPassword.value.length < 8) {
    triggerToast('Mật khẩu mới phải có ít nhất 8 ký tự!', 'error')
    return
  }
  if (newPassword.value !== confirmPassword.value) {
    triggerToast('Mật khẩu xác nhận không trùng khớp!', 'error')
    return
  }

  // Ghi nhật ký audit giả lập
  const nowStr = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  loginLogs.value.unshift({
    id: Date.now(),
    time: nowStr,
    ip: '14.162.245.10',
    location: 'Hà Nội (VN)',
    device: 'Chrome 125, Windows 11',
    status: 'Password_Changed'
  })

  isPasswordModalOpen.value = false
  triggerToast('Đổi mật khẩu tài khoản thành công! (Ghi Log Audit)', 'success')
}

// --- Toggle 2FA ---
const toggle2Fa = () => {
  is2FaEnabled.value = !is2FaEnabled.value
  const statusStr = is2FaEnabled.value ? 'Kích hoạt' : 'Hủy kích hoạt'
  
  const nowStr = new Date().toLocaleString('vi-VN').replace(/\//g, '-')
  loginLogs.value.unshift({
    id: Date.now(),
    time: nowStr,
    ip: '14.162.245.10',
    location: 'Hà Nội (VN)',
    device: 'Chrome 125, Windows 11',
    status: is2FaEnabled.value ? '2FA_Enabled' : '2FA_Disabled'
  })

  triggerToast(`Đã ${statusStr.toLowerCase()} xác thực 2 lớp (2FA)!`, 'info')
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <!-- Orbs trang trí mờ ảo -->
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast Thông báo -->
    <div 
      v-if="showToast" 
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <ShieldCheck v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header Trang -->
      <div class="mb-6">
        <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
          <User class="w-8 h-8 text-primary" />
          Hồ Sơ Cá Nhân Admin
        </h1>
        <p class="text-sm text-muted mt-1">
          Quản lý thông tin cá nhân định danh, cấu hình bảo mật, đổi mật khẩu và nhật ký truy cập hệ thống của chính tài khoản quản trị.
        </p>
      </div>

      <!-- Layout: Settings Style (Sidebar + Content) -->
      <div class="flex flex-col lg:flex-row gap-6 items-start">
        
        <!-- Cột trái: Profile Summary Card & Tabs -->
        <div class="w-full lg:w-80 flex-shrink-0 space-y-4">
          <!-- Profile Card -->
          <div class="lg-glass-soft lg-card lg-density-normal text-center py-6 px-4">
            <div class="relative w-24 h-24 mx-auto mb-4">
              <div class="w-full h-full rounded-full bg-gradient-to-br from-violet-600 to-indigo-600 text-white flex items-center justify-center text-4xl font-black shadow-lg border border-white/20">
                {{ adminProfile.fullName.charAt(0) }}
              </div>
              <div class="absolute bottom-1 right-1 w-5 h-5 bg-emerald-500 rounded-full border-2 border-slate-900" title="Trạng thái: Hoạt động"></div>
            </div>
            
            <h3 class="text-lg font-bold text-heading">{{ adminProfile.fullName }}</h3>
            <p class="text-xs text-muted mb-3 font-mono">{{ adminProfile.email }}</p>
            
            <div class="flex flex-wrap items-center justify-center gap-2">
              <span class="px-2.5 py-0.5 rounded-full border text-[10px] font-extrabold bg-violet-500/10 text-violet-500 border-violet-300">
                Super Admin
              </span>
              <span class="px-2.5 py-0.5 rounded-full border text-[10px] font-extrabold bg-emerald-500/10 text-emerald-500 border-emerald-300">
                {{ adminProfile.status }}
              </span>
            </div>
          </div>

          <!-- Navigation Tab Sidebar -->
          <div class="lg-glass-soft lg-card p-2 space-y-1">
            <button 
              v-for="tab in tabs" 
              :key="tab.id"
              @click="activeTab = tab.id"
              class="w-full text-left px-4 py-2.5 rounded-xl text-sm font-semibold flex items-center gap-3 transition-all duration-200"
              :class="activeTab === tab.id 
                ? 'bg-violet-600 text-white shadow-md shadow-violet-500/20' 
                : 'text-label hover:bg-surface-table-row-hover hover:text-heading'"
            >
              <component :is="tab.icon" class="w-4 h-4" />
              {{ tab.label }}
            </button>
          </div>
        </div>

        <!-- Cột phải: Content Pane -->
        <div class="flex-1 min-w-0 w-full lg-glass-soft lg-card lg-density-normal p-6 min-h-[500px]">
          
          <!-- TAB 1: THÔNG TIN CÁ NHÂN -->
          <div v-show="activeTab === 'profile'" class="space-y-6">
            <div class="pb-3 border-b border-default">
              <h2 class="text-lg font-extrabold text-heading">Thông tin định danh</h2>
              <p class="text-xs text-muted">Cập nhật thông tin định danh cơ bản của tài khoản Super Admin.</p>
            </div>

            <!-- Profile Form -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Họ và tên -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Họ và tên *</label>
                <input 
                  type="text" 
                  v-model="editForm.fullName" 
                  class="w-full px-3 lg-control text-sm"
                  placeholder="Nhập họ và tên..."
                />
              </div>

              <!-- Email -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Email *</label>
                <input 
                  type="email" 
                  v-model="editForm.email" 
                  class="w-full px-3 lg-control text-sm"
                  placeholder="Nhập địa chỉ email..."
                />
              </div>

              <!-- Số điện thoại -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Số điện thoại</label>
                <input 
                  type="text" 
                  v-model="editForm.phone" 
                  class="w-full px-3 lg-control text-sm"
                  placeholder="Nhập số điện thoại..."
                />
              </div>

              <!-- Trạng thái -->
              <div>
                <label class="block text-xs font-bold text-label mb-1.5 uppercase">Trạng thái hệ thống</label>
                <div class="px-3 py-2 bg-surface-solid border border-default rounded-xl text-xs font-semibold text-heading">
                  {{ adminProfile.status }} (Đang hoạt động bình thường)
                </div>
              </div>
            </div>

            <div class="divider border-b border-default my-6"></div>

            <!-- Quyền hạn & Phạm vi (Readonly + Warning) -->
            <div class="space-y-4">
              <h3 class="text-sm font-extrabold text-heading">Quyền hạn & Phạm vi kiểm soát</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Vai trò -->
                <div>
                  <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Vai trò quản trị</label>
                  <div class="px-3 py-2 bg-surface-solid border border-default border-dashed rounded-xl text-xs font-mono font-bold text-heading">
                    {{ adminProfile.role }}
                  </div>
                </div>

                <!-- Phạm vi -->
                <div>
                  <label class="block text-xs font-bold text-muted mb-1.5 uppercase">Phạm vi quản lý dữ liệu</label>
                  <div class="px-3 py-2 bg-surface-solid border border-default border-dashed rounded-xl text-xs font-mono font-bold text-heading">
                    {{ adminProfile.scope }}
                  </div>
                </div>
              </div>

              <!-- Warning block: Ràng buộc không tự hạ quyền -->
              <div class="p-4 rounded-xl border bg-rose-500/10 border-rose-300 text-rose-700 dark:text-rose-400 flex items-start gap-3">
                <AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5 text-rose-500" />
                <div>
                  <h4 class="font-extrabold text-xs uppercase">RÀNG BUỘC BẢO VỆ QUYỀN HẠN TỐI CAO</h4>
                  <p class="text-xs mt-1 leading-relaxed">
                    Vai trò <code class="font-mono bg-rose-500/20 px-1 rounded text-heading text-[10px]">SuperAdmin</code> và phạm vi được gán cứng cho tài khoản chính danh. Hệ thống nghiêm cấm Super Admin tự hạ quyền, sửa vai trò hoặc thu hẹp phạm vi của chính mình để đề phòng lỗi rớt quyền quản trị tối cao (Lockout Protection).
                  </p>
                </div>
              </div>

              <!-- Thông báo Audit Log -->
              <div class="text-[10px] text-muted leading-relaxed">
                * Lưu ý bảo mật: Mọi thay đổi trong hồ sơ định danh cá nhân của tài khoản Super Admin sẽ được ghi lại dấu vết (Audit Log) theo thời gian thực để phục vụ kiểm toán bảo mật hệ thống.
              </div>
            </div>

            <!-- Form Actions -->
            <div class="flex justify-end pt-2">
              <button 
                @click="updateProfile"
                class="lg-btn-primary text-xs px-4 py-2 flex items-center gap-1.5"
              >
                <Save class="w-4 h-4" />
                Lưu Thay Đổi
              </button>
            </div>
          </div>

          <!-- TAB 2: BẢO MẬT & ĐỔI MẬT KHẨU -->
          <div v-show="activeTab === 'security'" class="space-y-6">
            <div class="pb-3 border-b border-default">
              <h2 class="text-lg font-extrabold text-heading">Bảo mật tài khoản</h2>
              <p class="text-xs text-muted">Bảo vệ quyền truy cập tài khoản thông qua mật khẩu và cơ chế xác thực đa yếu tố.</p>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <!-- Thay đổi mật khẩu Card -->
              <div class="p-4 rounded-xl bg-surface-card border border-default flex flex-col justify-between space-y-4">
                <div class="space-y-1.5">
                  <div class="w-10 h-10 rounded-lg bg-violet-500/10 flex items-center justify-center text-violet-500">
                    <Key class="w-5 h-5" />
                  </div>
                  <h3 class="font-bold text-sm text-heading">Đổi mật khẩu tài khoản</h3>
                  <p class="text-xs text-muted leading-relaxed">Thay đổi mật khẩu định kỳ hoặc khi nghi ngờ mật khẩu cũ bị lộ để bảo vệ tài khoản.</p>
                </div>
                <div>
                  <button 
                    @click="openPasswordModal"
                    class="lg-btn-secondary text-xs px-3.5 py-1.5 w-full flex items-center justify-center gap-1.5"
                  >
                    <Lock class="w-3.5 h-3.5" />
                    Đổi mật khẩu
                  </button>
                </div>
              </div>

              <!-- Xác thực 2 lớp 2FA Card -->
              <div class="p-4 rounded-xl bg-surface-card border border-default flex flex-col justify-between space-y-4">
                <div class="space-y-1.5">
                  <div class="w-10 h-10 rounded-lg bg-emerald-500/10 flex items-center justify-center text-emerald-500">
                    <ShieldCheck class="w-5 h-5" />
                  </div>
                  <div class="flex items-center gap-2">
                    <h3 class="font-bold text-sm text-heading">Xác thực 2 lớp (2FA)</h3>
                    <span 
                      class="text-[9px] px-1.5 py-0.2 rounded border font-extrabold"
                      :class="is2FaEnabled ? 'bg-emerald-500/10 text-emerald-500 border-emerald-300' : 'bg-rose-500/10 text-rose-500 border-rose-300'"
                    >
                      {{ is2FaEnabled ? 'Đã bật' : 'Chưa bật' }}
                    </span>
                  </div>
                  <p class="text-xs text-muted leading-relaxed">Bảo vệ tài khoản bằng cách yêu cầu mã xác nhận từ ứng dụng Authenticator mỗi lần đăng nhập.</p>
                </div>
                <div>
                  <button 
                    @click="toggle2Fa"
                    class="text-xs px-3.5 py-1.5 w-full flex items-center justify-center gap-1.5 rounded-lg border transition-all duration-200"
                    :class="is2FaEnabled 
                      ? 'bg-rose-500/10 text-rose-500 border-rose-300 hover:bg-rose-500/20' 
                      : 'lg-btn-primary'"
                  >
                    <ShieldCheck class="w-3.5 h-3.5" />
                    {{ is2FaEnabled ? 'Hủy kích hoạt 2FA' : 'Kích hoạt 2FA' }}
                  </button>
                </div>
              </div>
            </div>

            <!-- Cảnh báo An ninh cho Admin -->
            <div class="p-4 rounded-xl border bg-violet-500/5 border-violet-500/10 text-xs leading-relaxed space-y-2">
              <span class="font-bold text-violet-600 dark:text-violet-400 flex items-center gap-1">
                <ShieldCheck class="w-4 h-4" /> Nguyên tắc an toàn thông tin của Super Admin:
              </span>
              <p class="text-[11px] text-muted">
                1. Mật khẩu bắt buộc sử dụng tối thiểu 8 ký tự, bao gồm cả chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
                <br/>
                2. Nên thay đổi mật khẩu định kỳ 90 ngày một lần.
                <br/>
                3. Bắt buộc duy trì kích hoạt 2FA đối với tài khoản nắm giữ quyền tối cao để tránh các đợt tấn công dò mật khẩu (Brute Force) hoặc chiếm session token.
              </p>
            </div>
          </div>

          <!-- TAB 3: NHẬT KÝ ĐĂNG NHẬP -->
          <div v-show="activeTab === 'logs'" class="space-y-6">
            <div class="pb-3 border-b border-default flex justify-between items-end">
              <div>
                <h2 class="text-lg font-extrabold text-heading">Nhật ký truy cập hệ thống</h2>
                <p class="text-xs text-muted">Theo dõi trực tiếp lịch sử đăng nhập và các hành động nhạy cảm của chính tài khoản này.</p>
              </div>
              <span class="text-[10px] text-muted font-semibold bg-surface-solid border border-default px-2 py-0.5 rounded">
                Gần nhất
              </span>
            </div>

            <!-- Logs List Table -->
            <div class="lg-table-shell overflow-x-auto w-full max-w-full">
              <table class="min-w-[700px] w-full divide-y divide-default text-sm">
                <thead>
                  <tr class="surface-table-header">
                    <th class="px-4 py-2.5 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Thời gian</th>
                    <th class="px-4 py-2.5 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Địa chỉ IP</th>
                    <th class="px-4 py-2.5 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Vị trí địa lý</th>
                    <th class="px-4 py-2.5 text-left text-xs font-bold text-label uppercase whitespace-nowrap">Thiết bị & Trình duyệt</th>
                    <th class="px-4 py-2.5 text-center text-xs font-bold text-label uppercase whitespace-nowrap">Trạng thái</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-default">
                  <tr 
                    v-for="log in loginLogs" 
                    :key="log.id" 
                    class="transition-colors hover:bg-surface-table-row-hover"
                  >
                    <!-- Thời gian -->
                    <td class="px-4 py-3 whitespace-nowrap text-xs font-semibold text-muted">
                      {{ log.time }}
                    </td>

                    <!-- IP -->
                    <td class="px-4 py-3 whitespace-nowrap font-mono text-xs text-heading font-semibold">
                      {{ log.ip }}
                    </td>

                    <!-- Vị trí -->
                    <td class="px-4 py-3 whitespace-nowrap text-xs text-heading">
                      <div class="flex items-center gap-1">
                        <Globe class="w-3.5 h-3.5 text-slate-400" />
                        <span>{{ log.location }}</span>
                      </div>
                    </td>

                    <!-- Thiết bị -->
                    <td class="px-4 py-3 text-xs text-muted truncate max-w-[130px]" :title="log.device">
                      <div class="flex items-center gap-1">
                        <Cpu class="w-3.5 h-3.5 text-slate-400" />
                        <span>{{ log.device }}</span>
                      </div>
                    </td>

                    <!-- Trạng thái/Hành động -->
                    <td class="px-4 py-3 whitespace-nowrap text-center text-xs">
                      <span 
                        v-if="log.status === 'Success'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-emerald-500/10 text-emerald-500 border-emerald-300"
                      >
                        Thành công
                      </span>
                      <span 
                        v-else-if="log.status === 'Failed'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-rose-500/10 text-rose-500 border-rose-300"
                      >
                        Thất bại
                      </span>
                      <span 
                        v-else-if="log.status === 'Profile_Updated'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-sky-500/10 text-sky-500 border-sky-300"
                      >
                        Sửa hồ sơ
                      </span>
                      <span 
                        v-else-if="log.status === 'Password_Changed'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-amber-500/10 text-amber-500 border-amber-300"
                      >
                        Đổi mật khẩu
                      </span>
                      <span 
                        v-else-if="log.status === '2FA_Enabled'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-violet-500/10 text-violet-500 border-violet-300"
                      >
                        Bật 2FA
                      </span>
                      <span 
                        v-else-if="log.status === '2FA_Disabled'"
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-slate-500/10 text-slate-500 border-slate-300"
                      >
                        Tắt 2FA
                      </span>
                      <span 
                        v-else
                        class="px-2 py-0.5 rounded-full border text-[10px] font-extrabold bg-violet-500/10 text-violet-500 border-violet-300"
                      >
                        {{ log.status }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Footer audit message -->
            <p class="text-[10px] text-muted">
              * Nhật ký kiểm toán đăng nhập được hệ thống tự động khóa và lưu trữ không thể sửa đổi (Immutable Logs). Trong trường hợp nghi ngờ tài khoản bị rò rỉ phiên hoạt động, vui lòng thay đổi mật khẩu ngay lập tức.
            </p>
          </div>

        </div>

      </div>
    </div>

    <!-- CHANGE PASSWORD MODAL (Cửa sổ Đổi mật khẩu) -->
    <div 
      v-if="isPasswordModalOpen" 
      class="fixed inset-0 z-[105] flex items-center justify-center bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md bg-surface-modal lg-glass-strong border border-default rounded-2xl shadow-2xl p-6 flex flex-col animate-in scale-in duration-300">
        <!-- Modal Header -->
        <div class="flex items-center justify-between pb-3 border-b border-default mb-4">
          <h3 class="text-md font-extrabold text-heading flex items-center gap-2">
            <Key class="w-5 h-5 text-violet-500" />
            Thay Đổi Mật Khẩu
          </h3>
          <button 
            @click="isPasswordModalOpen = false" 
            class="p-1.5 hover:bg-surface-table-row-hover rounded-lg text-muted hover:text-heading transition-colors"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <!-- Modal Body -->
        <div class="space-y-4">
          <!-- Mật khẩu hiện tại -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Mật khẩu hiện tại *</label>
            <div class="relative">
              <input 
                :type="showOldPassword ? 'text' : 'password'" 
                v-model="oldPassword" 
                class="w-full pl-3 pr-10 lg-control text-sm"
                placeholder="Nhập mật khẩu hiện tại..."
              />
              <button 
                @click="showOldPassword = !showOldPassword" 
                class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading"
              >
                <Eye v-if="!showOldPassword" class="w-4 h-4" />
                <EyeOff v-else class="w-4 h-4" />
              </button>
            </div>
          </div>

          <!-- Mật khẩu mới -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Mật khẩu mới *</label>
            <div class="relative">
              <input 
                :type="showNewPassword ? 'text' : 'password'" 
                v-model="newPassword" 
                class="w-full pl-3 pr-10 lg-control text-sm"
                placeholder="Nhập mật khẩu mới..."
              />
              <button 
                @click="showNewPassword = !showNewPassword" 
                class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading"
              >
                <Eye v-if="!showNewPassword" class="w-4 h-4" />
                <EyeOff v-else class="w-4 h-4" />
              </button>
            </div>
            <p class="text-[10px] text-muted mt-1">Yêu cầu tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và số.</p>
          </div>

          <!-- Xác nhận mật khẩu mới -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Xác nhận mật khẩu mới *</label>
            <div class="relative">
              <input 
                :type="showConfirmPassword ? 'text' : 'password'" 
                v-model="confirmPassword" 
                class="w-full pl-3 pr-10 lg-control text-sm"
                placeholder="Nhập lại mật khẩu mới..."
              />
              <button 
                @click="showConfirmPassword = !showConfirmPassword" 
                class="absolute right-3 top-1/2 -translate-y-1/2 text-muted hover:text-heading"
              >
                <Eye v-if="!showConfirmPassword" class="w-4 h-4" />
                <EyeOff v-else class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="flex justify-end gap-2 pt-4 border-t border-default mt-5">
          <button 
            @click="isPasswordModalOpen = false" 
            class="lg-btn-secondary text-xs px-4 py-2"
          >
            Hủy Bỏ
          </button>
          <button 
            @click="confirmChangePassword"
            class="lg-btn-primary text-xs px-4 py-2 flex items-center gap-1.5"
          >
            <Key class="w-4 h-4" />
            Cập Nhật Mật Khẩu
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Reset css component helper nếu cần */
.lg-shell-content {
  max-width: 1200px;
  padding: 0 1rem;
}
</style>
