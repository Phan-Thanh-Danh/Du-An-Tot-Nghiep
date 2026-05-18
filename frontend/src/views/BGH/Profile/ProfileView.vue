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
  Camera,
  Award,
  Users,
  BarChart3,
  Globe
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { mockBGH } from '@/components/BGH/data/menuData.js'

// ── Mock Data ────────────────────────────────────────────────
const profile = ref({
  ...mockBGH,
  phone: '0901.234.567',
  joinDate: '01/01/2020',
  bio: 'Tận tâm vì sự nghiệp giáo dục và phát triển thế hệ trẻ. Cam kết xây dựng môi trường học tập hiện đại, minh bạch và chất lượng cao.',
})

const stats = [
  { label: 'Sinh viên quản lý', value: '12,450', icon: Users, color: 'text-blue-600', bg: 'bg-blue-100' },
  { label: 'Giảng viên', value: '458', icon: Award, color: 'text-purple-600', bg: 'bg-purple-100' },
  { label: 'GPA Trung bình', value: '3.24', icon: BarChart3, color: 'text-emerald-600', bg: 'bg-emerald-100' },
  { label: 'Cơ sở', value: '3', icon: Globe, color: 'text-amber-600', bg: 'bg-amber-100' },
]

const loginHistory = ref([
  { id: 1, device: 'Chrome / Windows 11', ip: '1.55.xxx.xxx', time: '16/05/2026 20:15', status: 'current' },
  { id: 2, device: 'Safari / MacBook Pro', ip: '1.55.xxx.xxx', time: '15/05/2026 09:45', status: 'previous' },
  { id: 3, device: 'LMS Mobile App / iOS', ip: '27.72.xxx.xxx', time: '14/05/2026 18:30', status: 'previous' },
])
</script>

<template>
  <PageContainer 
    title="Hồ sơ Hiệu trưởng" 
    subtitle="Quản lý thông tin lãnh đạo, bảo mật tài khoản và giám sát hoạt động hệ thống."
  >
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Executive Profile Card ── -->
      <div class="space-y-6">
        <div class="lg-card-glass p-10 text-center relative overflow-hidden group">
           <!-- Premium Decor Blobs: Tối ưu giảm blur -->
           <div class="absolute -top-10 -left-10 h-40 w-40 bg-indigo-500/10 rounded-full blur-[40px]"></div>
           <div class="absolute -bottom-10 -right-10 h-40 w-40 bg-purple-500/10 rounded-full blur-[40px]"></div>
           
           <div class="relative inline-block mb-8">
              <div class="h-40 w-40 rounded-[48px] bg-gradient-to-br from-indigo-600 via-blue-500 to-purple-600 p-1.5 shadow-[0_20px_50px_rgba(79,70,229,0.3)] group-hover:scale-105 transition-all duration-500">
                 <div class="h-full w-full rounded-[44px] bg-white flex items-center justify-center overflow-hidden relative">
                    <User v-if="!profile.avatar" :size="80" class="text-slate-200" />
                    <img v-else :src="profile.avatar" class="h-full w-full object-cover" />
                    
                    <button class="absolute inset-0 bg-slate-900/60 opacity-0 group-hover:opacity-100 flex flex-col items-center justify-center text-white transition-opacity duration-300">
                       <Camera :size="28" class="mb-2" />
                       <span class="text-[10px] font-black uppercase tracking-widest">Thay đổi ảnh</span>
                    </button>
                 </div>
              </div>
              <!-- Role Badge Overlay -->
              <div class="absolute -bottom-2 -right-2 bg-white p-1 rounded-2xl shadow-lg">
                 <div class="bg-indigo-600 text-white px-3 py-1 rounded-xl text-[10px] font-black uppercase tracking-tighter">Principal</div>
              </div>
           </div>

           <h3 class="text-2xl font-black text-slate-800 leading-tight mb-2">{{ profile.name }}</h3>
           <div class="inline-flex items-center gap-2 bg-indigo-50 px-3 py-1.5 rounded-full mb-6">
              <ShieldCheck :size="14" class="text-indigo-600" />
              <p class="text-[10px] font-black text-indigo-600 uppercase tracking-[0.1em]">{{ profile.staffId }}</p>
           </div>

           <p class="text-sm text-slate-500 leading-relaxed font-medium mb-8 italic">
              "{{ profile.bio }}"
           </p>

           <div class="space-y-4">
              <div class="flex items-center gap-4 p-4 bg-white/60 rounded-3xl border border-white/60 shadow-sm">
                 <div class="h-10 w-10 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600">
                    <MapPin :size="20" />
                 </div>
                 <div class="text-left">
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Văn phòng</p>
                    <p class="text-xs font-bold text-slate-700">{{ profile.campus }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-4 p-4 bg-white/60 rounded-3xl border border-white/60 shadow-sm">
                 <div class="h-10 w-10 rounded-2xl bg-purple-50 flex items-center justify-center text-purple-600">
                    <ShieldCheck :size="20" />
                 </div>
                 <div class="text-left">
                    <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest">Đơn vị quản lý</p>
                    <p class="text-xs font-bold text-slate-700">{{ profile.department }}</p>
                 </div>
              </div>
           </div>

           <button class="w-full mt-10 lg-button-primary py-4 text-xs font-black tracking-widest uppercase flex items-center justify-center gap-3 shadow-[0_15px_35px_rgba(37,99,235,0.25)]">
              <Edit3 :size="20" /> Cập nhật thông tin lãnh đạo
           </button>
        </div>

        <!-- Quick Stats Widget -->
        <div class="lg-card-glass p-6">
           <h4 class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-6 flex items-center gap-2">
              <BarChart3 :size="16" /> Chỉ số tổng quan
           </h4>
           <div class="grid grid-cols-2 gap-4">
              <div v-for="stat in stats" :key="stat.label" class="p-4 lg-card-premium text-center">
                 <div :class="['h-10 w-10 rounded-2xl mx-auto mb-3 flex items-center justify-center', stat.bg, stat.color]">
                    <component :is="stat.icon" :size="20" />
                 </div>
                 <p class="text-[10px] font-bold text-slate-400 uppercase tracking-tighter">{{ stat.label }}</p>
                 <p class="text-lg font-black text-slate-800">{{ stat.value }}</p>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Detailed Info & Control Center ── -->
      <div class="xl:col-span-2 space-y-8">
        
        <!-- Leadership Details & Security -->
        <div class="lg-card-glass overflow-hidden">
           <div class="p-8 border-b border-slate-100/50 bg-gradient-to-r from-white to-indigo-50/30 flex items-center justify-between">
              <div>
                 <h4 class="text-sm font-black text-slate-800 uppercase tracking-wide">Trung tâm điều khiển & Bảo mật</h4>
                 <p class="text-xs text-slate-500 font-medium mt-1">Thông tin liên lạc chính thức và thiết lập bảo mật cao cấp.</p>
              </div>
              <div class="h-12 w-12 rounded-2xl bg-amber-100 flex items-center justify-center text-amber-600 shadow-sm shadow-amber-200/50">
                 <ShieldAlert :size="24" />
              </div>
           </div>

           <div class="p-8 space-y-10">
              <!-- Contact Grid -->
              <div class="grid grid-cols-1 md:grid-cols-2 gap-10">
                 <div class="space-y-8">
                    <div class="flex items-center gap-5 group cursor-pointer">
                       <div class="h-14 w-14 rounded-3xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-100 group-hover:text-blue-600 transition-all duration-300 shadow-sm border border-slate-100">
                          <Mail :size="24" />
                       </div>
                       <div>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Email công vụ</p>
                          <p class="text-base font-bold text-slate-700 underline underline-offset-4 decoration-blue-200 hover:decoration-blue-500 transition-all">{{ profile.email }}</p>
                       </div>
                    </div>
                    <div class="flex items-center gap-5 group cursor-pointer">
                       <div class="h-14 w-14 rounded-3xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-emerald-100 group-hover:text-emerald-600 transition-all duration-300 shadow-sm border border-slate-100">
                          <Phone :size="24" />
                       </div>
                       <div>
                          <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-1">Đường dây nóng</p>
                          <p class="text-base font-bold text-slate-700">{{ profile.phone }}</p>
                       </div>
                    </div>
                 </div>

                 <!-- Password Card -->
                 <div class="p-8 bg-gradient-to-br from-indigo-600 to-blue-700 rounded-[40px] text-white shadow-xl shadow-indigo-200/50 relative overflow-hidden group">
                    <div class="absolute -right-4 -bottom-4 opacity-10 group-hover:scale-110 transition-transform duration-700">
                       <KeyRound :size="120" />
                    </div>
                    <div class="flex items-start justify-between mb-6">
                       <div class="h-12 w-12 rounded-2xl bg-white/30 flex items-center justify-center">
                          <KeyRound :size="24" />
                       </div>
                       <button class="text-[10px] font-black uppercase tracking-widest bg-white text-indigo-600 px-4 py-2 rounded-xl shadow-lg hover:scale-105 transition-transform active:scale-95">Đổi mật khẩu</button>
                    </div>
                    <h5 class="text-lg font-black mb-2">Bảo mật tài khoản</h5>
                    <p class="text-xs text-indigo-100/80 leading-relaxed font-medium">Tài khoản của bạn được bảo vệ bởi lớp mã hóa 256-bit. Lần đổi mật khẩu cuối cùng: 3 tháng trước.</p>
                 </div>
              </div>

              <!-- Login History -->
              <div>
                 <div class="flex items-center justify-between mb-8">
                    <h4 class="text-xs font-black text-slate-400 uppercase tracking-[0.2em] flex items-center gap-2">
                       <Clock :size="16" /> Hoạt động đăng nhập
                    </h4>
                    <button class="text-[10px] font-black text-blue-600 uppercase">Xem tất cả</button>
                 </div>
                 
                 <div class="space-y-4">
                    <div v-for="log in loginHistory" :key="log.id" class="flex items-center justify-between p-5 bg-white/50 rounded-3xl border border-slate-100 group hover:border-indigo-300 hover:shadow-md transition-all duration-300">
                       <div class="flex items-center gap-5">
                          <div :class="['h-12 w-12 rounded-2xl flex items-center justify-center shadow-sm', log.status === 'current' ? 'bg-emerald-100 text-emerald-600' : 'bg-slate-50 text-slate-400']">
                             <ShieldCheck v-if="log.status === 'current'" :size="20" />
                             <ChevronRight v-else :size="20" />
                          </div>
                          <div>
                             <p class="text-sm font-black text-slate-800">{{ log.device }}</p>
                             <p class="text-xs font-bold text-slate-400 mt-0.5 flex items-center gap-2">
                                <span>{{ log.ip }}</span>
                                <span class="h-1 w-1 rounded-full bg-slate-300"></span>
                                <span>{{ log.time }}</span>
                             </p>
                          </div>
                       </div>
                       <div v-if="log.status === 'current'" class="flex items-center gap-2 bg-emerald-50 text-emerald-600 px-3 py-1.5 rounded-xl border border-emerald-100">
                          <div class="h-2 w-2 rounded-full bg-emerald-500 animate-pulse"></div>
                          <span class="text-[10px] font-black uppercase tracking-widest">Đang hoạt động</span>
                       </div>
                    </div>
                 </div>
              </div>
           </div>
        </div>

        <!-- Footer Actions -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
           <div class="lg-card-premium p-6 flex items-center gap-4 cursor-pointer group">
              <div class="h-14 w-14 rounded-3xl bg-blue-50 flex items-center justify-center text-blue-600 group-hover:scale-110 transition-transform">
                 <Globe :size="24" />
              </div>
              <div class="flex-1">
                 <h4 class="text-sm font-black text-slate-800">Cài đặt ngôn ngữ</h4>
                 <p class="text-[10px] text-slate-500 font-bold uppercase tracking-widest mt-0.5">Tiếng Việt (Mặc định)</p>
              </div>
              <ChevronRight :size="18" class="text-slate-300" />
           </div>
           
           <div class="lg-card-premium p-8 border-rose-100 bg-gradient-to-br from-white to-rose-50/10 flex items-center justify-between group">
              <div class="flex items-center gap-5">
                 <div class="h-14 w-14 rounded-3xl bg-rose-100 flex items-center justify-center text-rose-600 shadow-sm shadow-rose-200/50 group-hover:rotate-12 transition-transform">
                    <LogOut :size="24" />
                 </div>
                 <div>
                    <h4 class="text-base font-black text-rose-900">Đăng xuất</h4>
                    <p class="text-xs text-rose-700/70 font-medium">Kết thúc phiên làm việc</p>
                 </div>
              </div>
              <button class="h-12 w-12 rounded-2xl bg-rose-600 text-white flex items-center justify-center shadow-lg shadow-rose-500/20 hover:bg-rose-700 hover:scale-105 active:scale-95 transition-all">
                 <LogOut :size="20" />
              </button>
           </div>
        </div>

      </div>

    </div>
  </PageContainer>
</template>

<style scoped>
.lg-card-glass {
  /* Tối ưu hóa cực độ: Loại bỏ hoàn toàn backdrop-filter để scroll mượt 100% */
  background: rgba(255, 255, 255, 0.96);
  border-radius: 40px;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.03);
}

.lg-card-premium {
  /* Không dùng hiệu ứng làm mờ cho các thành phần nhỏ */
  background: #ffffff;
  border-radius: 32px;
  border: 1px solid rgba(226, 232, 240, 0.7);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.01);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.lg-card-premium:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.04);
}
</style>
