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
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Profile Card ── -->
      <div class="space-y-6">
        <div class="lg-card-glass p-8 text-center relative overflow-hidden group">
           <!-- Decor Blob -->
           <div class="absolute -top-10 -right-10 h-32 w-32 bg-blue-500/5 rounded-full blur-3xl group-hover:bg-blue-500/10 transition-colors"></div>
           
           <div class="relative inline-block mb-6">
              <div class="h-32 w-32 rounded-[40px] bg-gradient-to-br from-blue-500 via-indigo-500 to-violet-600 p-1 shadow-xl shadow-blue-500/20 group-hover:scale-105 transition-transform">
                 <div class="h-full w-full rounded-[38px] bg-white flex items-center justify-center overflow-hidden relative">
                    <User v-if="!profile.avatar" :size="60" class="text-slate-200" />
                    <img v-else :src="profile.avatar" class="h-full w-full object-cover" />
                    
                    <button class="absolute inset-0 bg-slate-900/40 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white transition-opacity">
                       <Camera :size="24" />
                    </button>
                 </div>
              </div>
           </div>

           <h3 class="text-xl font-black text-slate-800 leading-tight">{{ profile.name }}</h3>
           <p class="text-[11px] font-black text-blue-600 uppercase tracking-[0.2em] mt-1.5">{{ profile.code }}</p>

           <div class="mt-8 flex flex-col gap-4">
              <div class="flex items-center gap-3 p-4 bg-slate-50/50 rounded-2xl border border-slate-100">
                 <ShieldCheck :size="18" class="text-indigo-500" />
                 <div class="text-left">
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Chức danh</p>
                    <p class="text-xs font-bold text-slate-700">{{ profile.role }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-3 p-4 bg-slate-50/50 rounded-2xl border border-slate-100">
                 <MapPin :size="18" class="text-rose-500" />
                 <div class="text-left">
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Cơ sở làm việc</p>
                    <p class="text-xs font-bold text-slate-700">{{ profile.campus }}</p>
                 </div>
              </div>
           </div>

           <button class="w-full mt-8 lg-button-primary py-3.5 text-sm font-black flex items-center justify-center gap-2">
              <Edit3 :size="18" /> CHỈNH SỬA HỒ SƠ
           </button>
        </div>

        <!-- System Stats -->
        <div class="lg-card-glass p-6">
           <div class="grid grid-cols-2 gap-4">
              <div class="p-4 bg-white rounded-2xl border border-slate-100 shadow-sm">
                 <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Ngày tham gia</p>
                 <p class="text-sm font-black text-slate-800 mt-1">{{ profile.joinDate }}</p>
              </div>
              <div class="p-4 bg-white rounded-2xl border border-slate-100 shadow-sm">
                 <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Trạng thái</p>
                 <p class="text-sm font-black text-emerald-600 mt-1 flex items-center gap-1.5">
                    <span class="h-1.5 w-1.5 rounded-full bg-emerald-500"></span> Active
                 </p>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Detailed Info & Security ── -->
      <div class="xl:col-span-2 space-y-8">
        
        <!-- Contact & Security Section -->
        <div class="lg-card-glass overflow-hidden">
           <div class="p-8 border-b border-slate-50 flex items-center justify-between">
              <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide">Thông tin liên hệ & Bảo mật</h4>
              <ShieldAlert :size="20" class="text-amber-500" />
           </div>

           <div class="p-8 space-y-8">
              <!-- Grid Info -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                 <div class="space-y-6">
                    <div class="flex items-center gap-4">
                       <div class="h-10 w-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-400">
                          <Mail :size="20" />
                       </div>
                       <div>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Email học vụ</p>
                          <p class="text-sm font-bold text-slate-700 underline underline-offset-4 decoration-blue-200">{{ profile.email }}</p>
                       </div>
                    </div>
                    <div class="flex items-center gap-4">
                       <div class="h-10 w-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-400">
                          <Phone :size="20" />
                       </div>
                       <div>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Số điện thoại</p>
                          <p class="text-sm font-bold text-slate-700">{{ profile.phone }}</p>
                       </div>
                    </div>
                 </div>

                 <div class="p-6 bg-blue-50/50 rounded-[32px] border border-blue-100">
                    <div class="flex items-start justify-between mb-4">
                       <div class="h-10 w-10 rounded-2xl bg-white flex items-center justify-center text-blue-600 shadow-sm">
                          <KeyRound :size="20" />
                       </div>
                       <button class="text-[10px] font-black text-blue-600 uppercase tracking-widest bg-white px-3 py-1.5 rounded-xl border border-blue-100 shadow-sm hover:bg-blue-600 hover:text-white transition-all">Thay đổi</button>
                    </div>
                    <h5 class="text-sm font-black text-blue-900 mb-1">Mật khẩu tài khoản</h5>
                    <p class="text-xs text-blue-700/70 leading-relaxed font-medium">Bạn nên thay đổi mật khẩu định kỳ 6 tháng một lần để đảm bảo an toàn hệ thống.</p>
                 </div>
              </div>

              <!-- Login History -->
              <div>
                 <h4 class="text-xs font-black text-slate-400 uppercase tracking-widest mb-6 flex items-center gap-2">
                    <Clock :size="16" /> Lịch sử đăng nhập gần nhất
                 </h4>
                 <div class="space-y-3">
                    <div v-for="log in loginHistory" :key="log.id" class="flex items-center justify-between p-4 bg-white rounded-2xl border border-slate-100 group hover:border-blue-200 transition-all">
                       <div class="flex items-center gap-4">
                          <div :class="['h-8 w-8 rounded-full flex items-center justify-center', log.status === 'current' ? 'bg-emerald-100 text-emerald-600' : 'bg-slate-100 text-slate-400']">
                             <ShieldCheck v-if="log.status === 'current'" :size="16" />
                             <ChevronRight v-else :size="16" />
                          </div>
                          <div>
                             <p class="text-xs font-black text-slate-800">{{ log.device }}</p>
                             <p class="text-[10px] font-bold text-slate-400 mt-0.5">IP: {{ log.ip }} • {{ log.time }}</p>
                          </div>
                       </div>
                       <span v-if="log.status === 'current'" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest bg-emerald-50 px-2 py-1 rounded-lg border border-emerald-100">Hiện tại</span>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Danger Zone -->
        <div class="lg-card-glass p-8 border-rose-100 bg-rose-50/5">
           <div class="flex items-center justify-between">
              <div class="flex items-center gap-4">
                 <div class="h-10 w-10 rounded-2xl bg-rose-100 flex items-center justify-center text-rose-600 shadow-sm">
                    <LogOut :size="20" />
                 </div>
                 <div>
                    <h4 class="text-sm font-black text-rose-900">Đăng xuất tài khoản</h4>
                    <p class="text-xs text-rose-700 mt-1 font-medium">Kết thúc phiên làm việc hiện tại trên thiết bị này.</p>
                 </div>
              </div>
              <button class="px-6 py-3 bg-rose-600 text-white rounded-2xl text-xs font-black shadow-lg shadow-rose-500/20 hover:bg-rose-700 transition-all">ĐĂNG XUẤT</button>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>
