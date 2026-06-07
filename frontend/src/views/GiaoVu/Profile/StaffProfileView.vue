<script setup>
import { ref } from 'vue'
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
  Camera
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

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

const loginHistory = ref([
  { id: 1, device: 'Chrome / Windows 11', ip: '14.232.xxx.xxx', time: '13/05/2026 08:30', status: 'current' },
  { id: 2, device: 'Safari / iPhone 15', ip: '112.xxx.xxx.xxx', time: '12/05/2026 21:15', status: 'previous' },
  { id: 3, device: 'Chrome / Windows 11', ip: '14.232.xxx.xxx', time: '12/05/2026 09:00', status: 'previous' },
])
</script>

<template>
  <PageContainer 
    title="Hồ sơ cá nhân" 
    subtitle="Quản lý thông tin tài khoản, bảo mật và cài đặt cá nhân của giáo vụ."
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

            <h3 class="text-lg font-black text-heading leading-tight">{{ profile.name }}</h3>
            <p class="text-[10px] font-black text-link uppercase tracking-[0.2em] mt-1">{{ profile.code }}</p>

           <div class="mt-5 flex flex-col gap-3">
               <div class="flex items-center gap-2 p-3 surface-solid rounded-xl border-default">
                  <ShieldCheck :size="14" class="text-body" />
                  <div class="text-left">
                     <p class="text-[8px] font-black text-label uppercase tracking-widest">Chức danh</p>
                     <p class="text-[11px] font-bold text-heading">{{ profile.role }}</p>
                  </div>
               </div>
               <div class="flex items-center gap-2 p-3 surface-solid rounded-xl border-default">
                  <MapPin :size="14" class="text-body" />
                  <div class="text-left">
                     <p class="text-[8px] font-black text-label uppercase tracking-widest">Cơ sở làm việc</p>
                     <p class="text-[11px] font-bold text-heading">{{ profile.campus }}</p>
                 </div>
              </div>
           </div>

           <button class="w-full mt-5 lg-button-primary py-3 text-xs font-black flex items-center justify-center gap-2">
              <Edit3 :size="14" /> CHỈNH SỬA HỒ SƠ
           </button>
        </div>

        <!-- System Stats -->
        <div class="surface-card border border-card rounded-2xl p-3">
           <div class="grid grid-cols-2 gap-3">
               <div class="p-3 surface-solid rounded-xl border border-default shadow-sm">
                  <p class="text-[8px] font-black text-label uppercase tracking-widest">Ngày tham gia</p>
                  <p class="text-xs font-black text-heading mt-0.5">{{ profile.joinDate }}</p>
               </div>
               <div class="p-3 surface-solid rounded-xl border border-default shadow-sm">
                  <p class="text-[8px] font-black text-label uppercase tracking-widest">Trạng thái</p>
                  <p class="text-xs font-black text-[var(--color-success-text)] mt-0.5 flex items-center gap-1.5">
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
                <h4 class="text-xs font-black text-heading uppercase tracking-wide">Thông tin liên hệ & Bảo mật</h4>
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
                         <div>
                            <p class="text-[9px] font-black text-label uppercase tracking-widest mb-0.5">Email học vụ</p>
                            <p class="text-xs font-bold text-heading">{{ profile.email }}</p>
                         </div>
                      </div>
                      <div class="flex items-center gap-3">
                         <div class="h-8 w-8 rounded-lg surface-solid flex items-center justify-center text-placeholder">
                            <Phone :size="16" />
                         </div>
                         <div>
                            <p class="text-[9px] font-black text-label uppercase tracking-widest mb-0.5">Số điện thoại</p>
                            <p class="text-xs font-bold text-heading">{{ profile.phone }}</p>
                        </div>
                     </div>
                  </div>

                  <div class="p-3 surface-solid rounded-2xl border border-default">
                      <div class="flex items-start justify-between mb-3">
                         <div class="h-8 w-8 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-link shadow-sm">
                            <KeyRound :size="16" />
                         </div>
                         <button class="text-[9px] font-black text-link uppercase tracking-widest lg-button-ghost px-2 py-1 rounded-lg shadow-sm transition-all">Thay đổi</button>
                      </div>
                      <h5 class="text-xs font-black text-heading mb-0.5">Mật khẩu tài khoản</h5>
                      <p class="text-[10px] text-body leading-relaxed font-medium">Nên thay đổi mật khẩu định kỳ 6 tháng một lần.</p>
                  </div>
               </div>

               <!-- Login History -->
               <div>
                   <h4 class="text-[10px] font-black text-label uppercase tracking-widest mb-3 flex items-center gap-2">
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
                               <p class="text-[11px] font-black text-heading">{{ log.device }}</p>
                               <p class="text-[9px] font-bold text-label mt-0.5">IP: {{ log.ip }} • {{ log.time }}</p>
                            </div>
                         </div>
                         <span v-if="log.status === 'current'" class="text-[8px] font-black text-[var(--color-success-text)] uppercase tracking-widest bg-[var(--color-success-bg)] px-1.5 py-0.5 rounded-lg border border-[var(--color-success-text)]/20">Hiện tại</span>
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
                     <h4 class="text-xs font-black text-heading">Đăng xuất tài khoản</h4>
                     <p class="text-[10px] text-[var(--color-danger-text)] mt-0.5 font-medium">Kết thúc phiên làm việc hiện tại.</p>
                  </div>
               </div>
               <button class="px-3 py-2 lg-btn-danger rounded-xl text-[10px] font-black transition-all">ĐĂNG XUẤT</button>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
