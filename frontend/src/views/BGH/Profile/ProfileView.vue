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
      <div class="space-y-4">
        <div class="lg-card-glass p-6 text-center">
           <div class="relative inline-block mb-5">
              <div class="h-28 w-28 rounded-2xl bg-gradient-to-br from-indigo-600 via-blue-500 to-purple-600 p-1 shadow-lg group-hover:scale-105 transition-all duration-500">
                 <div class="h-full w-full rounded-[30px] bg-white flex items-center justify-center overflow-hidden relative">
                    <User v-if="!profile.avatar" :size="44" class="text-slate-200" />
                    <img v-else :src="profile.avatar" class="h-full w-full object-cover" />
                    
                    <button class="absolute inset-0 bg-slate-900/60 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white transition-opacity duration-300">
                       <Camera :size="20" />
                    </button>
                 </div>
              </div>
              <!-- Role Badge Overlay -->
              <div class="absolute -bottom-1 -right-1 bg-white p-0.5 rounded-xl shadow-lg">
                 <div class="bg-indigo-600 text-white px-2 py-0.5 rounded-lg text-[9px] font-black uppercase tracking-tighter">Principal</div>
              </div>
           </div>

           <h3 class="text-lg font-black text-slate-800 leading-tight mb-1">{{ profile.name }}</h3>
           <div class="inline-flex items-center gap-1.5 bg-indigo-50 px-2 py-1 rounded-full mb-3">
              <ShieldCheck :size="12" class="text-indigo-600" />
              <p class="text-[9px] font-black text-indigo-600 uppercase tracking-[0.1em]">{{ profile.staffId }}</p>
           </div>

           <p class="text-xs text-slate-500 leading-relaxed font-medium mb-5 italic">
              "{{ profile.bio }}"
           </p>

           <div class="space-y-3">
              <div class="flex items-center gap-3 p-3 bg-white/60 rounded-2xl border border-white/60 shadow-sm">
                 <div class="h-8 w-8 rounded-xl bg-blue-50 flex items-center justify-center text-blue-600">
                    <MapPin :size="16" />
                 </div>
                 <div class="text-left">
                    <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">Văn phòng</p>
                    <p class="text-[11px] font-bold text-slate-700">{{ profile.campus }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-3 p-3 bg-white/60 rounded-2xl border border-white/60 shadow-sm">
                 <div class="h-8 w-8 rounded-xl bg-purple-50 flex items-center justify-center text-purple-600">
                    <ShieldCheck :size="16" />
                 </div>
                 <div class="text-left">
                    <p class="text-[8px] font-black text-slate-400 uppercase tracking-widest">Đơn vị quản lý</p>
                    <p class="text-[11px] font-bold text-slate-700">{{ profile.department }}</p>
                 </div>
              </div>
           </div>

           <button class="w-full mt-6 lg-button-primary py-3 text-[10px] font-black tracking-widest uppercase flex items-center justify-center gap-2">
              <Edit3 :size="16" /> Cập nhật thông tin
           </button>
        </div>

        <!-- Quick Stats Widget -->
        <div class="lg-card-glass p-3">
           <h4 class="text-[9px] font-black text-slate-400 uppercase tracking-[0.2em] mb-3 flex items-center gap-2">
              <BarChart3 :size="14" /> Chỉ số tổng quan
           </h4>
           <div class="grid grid-cols-2 gap-3">
              <div v-for="stat in stats" :key="stat.label" class="p-3 lg-card-premium text-center">
                 <div :class="['h-8 w-8 rounded-xl mx-auto mb-2 flex items-center justify-center', stat.bg, stat.color]">
                    <component :is="stat.icon" :size="16" />
                 </div>
                 <p class="text-[9px] font-bold text-slate-400 uppercase tracking-tighter">{{ stat.label }}</p>
                 <p class="text-base font-black text-slate-800">{{ stat.value }}</p>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Detailed Info & Control Center ── -->
      <div class="xl:col-span-2 space-y-8">
        
         <!-- Leadership Details & Security -->
         <div class="lg-card-glass overflow-hidden">
            <div class="p-4 border-b border-slate-100/50 flex items-center justify-between">
               <div>
                  <h4 class="text-xs font-black text-slate-800 uppercase tracking-wide">Bảo mật & Liên hệ</h4>
                  <p class="text-[10px] text-slate-500 font-medium mt-0.5">Thông tin liên lạc và thiết lập bảo mật.</p>
               </div>
               <div class="h-8 w-8 rounded-xl bg-amber-100 flex items-center justify-center text-amber-600">
                  <ShieldAlert :size="18" />
               </div>
            </div>

            <div class="p-4 space-y-5">
               <!-- Contact Grid -->
               <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                  <div class="space-y-4">
                     <div class="flex items-center gap-3 group cursor-pointer">
                        <div class="h-10 w-10 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-blue-100 group-hover:text-blue-600 transition-all duration-300">
                           <Mail :size="18" />
                        </div>
                        <div>
                           <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Email công vụ</p>
                           <p class="text-sm font-bold text-slate-700">{{ profile.email }}</p>
                        </div>
                     </div>
                     <div class="flex items-center gap-3 group cursor-pointer">
                        <div class="h-10 w-10 rounded-2xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-emerald-100 group-hover:text-emerald-600 transition-all duration-300">
                           <Phone :size="18" />
                        </div>
                        <div>
                           <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-0.5">Đường dây nóng</p>
                           <p class="text-sm font-bold text-slate-700">{{ profile.phone }}</p>
                        </div>
                     </div>
                  </div>

                  <!-- Password Card -->
                  <div class="p-4 bg-gradient-to-br from-indigo-600 to-blue-700 rounded-3xl text-white relative overflow-hidden group">
                     <div class="absolute -right-4 -bottom-4 opacity-10">
                        <KeyRound :size="80" />
                     </div>
                     <div class="flex items-start justify-between mb-3">
                        <div class="h-8 w-8 rounded-xl bg-white/30 flex items-center justify-center">
                           <KeyRound :size="18" />
                        </div>
                        <button class="text-[9px] font-black uppercase tracking-widest bg-white text-indigo-600 px-3 py-1.5 rounded-lg shadow-lg hover:scale-105 transition-transform active:scale-95">Đổi mật khẩu</button>
                     </div>
                     <h5 class="text-sm font-black mb-1">Bảo mật tài khoản</h5>
                     <p class="text-[10px] text-indigo-100/80 leading-relaxed font-medium">Mã hóa 256-bit. Lần đổi gần nhất: 3 tháng trước.</p>
                  </div>
               </div>

               <!-- Login History -->
               <div>
                  <div class="flex items-center justify-between mb-3">
                     <h4 class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] flex items-center gap-2">
                        <Clock :size="14" /> Hoạt động đăng nhập
                     </h4>
                     <button class="text-[9px] font-black text-blue-600 uppercase">Xem tất cả</button>
                  </div>
                  
                  <div class="space-y-2">
                     <div v-for="log in loginHistory" :key="log.id" class="flex items-center justify-between p-3 bg-white/50 rounded-2xl border border-slate-100 group hover:border-indigo-300 hover:shadow-md transition-all duration-300">
                        <div class="flex items-center gap-3">
                           <div :class="['h-8 w-8 rounded-xl flex items-center justify-center', log.status === 'current' ? 'bg-emerald-100 text-emerald-600' : 'bg-slate-50 text-slate-400']">
                              <ShieldCheck v-if="log.status === 'current'" :size="16" />
                              <ChevronRight v-else :size="16" />
                           </div>
                           <div>
                              <p class="text-xs font-black text-slate-800">{{ log.device }}</p>
                              <p class="text-[10px] font-bold text-slate-400 mt-0.5 flex items-center gap-2">
                                 <span>{{ log.ip }}</span>
                                 <span class="h-1 w-1 rounded-full bg-slate-300"></span>
                                 <span>{{ log.time }}</span>
                              </p>
                           </div>
                        </div>
                        <div v-if="log.status === 'current'" class="flex items-center gap-1.5 bg-emerald-50 text-emerald-600 px-2 py-1 rounded-lg border border-emerald-100">
                           <div class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></div>
                           <span class="text-[8px] font-black uppercase tracking-widest">Đang hoạt động</span>
                        </div>
                     </div>
                  </div>
               </div>
            </div>
         </div>

        <!-- Footer Actions -->
         <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="lg-card-premium p-3 flex items-center gap-3 cursor-pointer group">
               <div class="h-10 w-10 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600 group-hover:scale-110 transition-transform">
                  <Globe :size="18" />
               </div>
               <div class="flex-1">
                  <h4 class="text-xs font-black text-slate-800">Cài đặt ngôn ngữ</h4>
                  <p class="text-[9px] text-slate-500 font-bold uppercase tracking-widest mt-0.5">Tiếng Việt (Mặc định)</p>
               </div>
               <ChevronRight :size="16" class="text-slate-300" />
            </div>
            
            <div class="lg-card-premium p-3 border-rose-100 flex items-center justify-between group">
               <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-2xl bg-rose-100 flex items-center justify-center text-rose-600 group-hover:rotate-12 transition-transform">
                     <LogOut :size="18" />
                  </div>
                  <div>
                     <h4 class="text-xs font-black text-rose-900">Đăng xuất</h4>
                     <p class="text-[10px] text-rose-700/70 font-medium">Kết thúc phiên làm việc</p>
                  </div>
               </div>
               <button class="h-8 w-8 rounded-xl bg-rose-600 text-white flex items-center justify-center shadow-lg hover:bg-rose-700 hover:scale-105 active:scale-95 transition-all">
                  <LogOut :size="16" />
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
