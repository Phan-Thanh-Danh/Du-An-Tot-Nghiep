<script setup>
import { ref, onMounted } from 'vue'
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
  Globe,
  AlertCircle,
  Loader2,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { mockBGH } from '@/components/BGH/data/menuData.js'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'
import { useAuthStore } from '@/stores/auth'

const auth = useAuthStore()
const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)

// ── Mock Data ────────────────────────────────────────────────
const profile = ref({
  ...mockBGH,
  phone: '0901.234.567',
  joinDate: '01/01/2020',
  bio: 'Tận tâm vì sự nghiệp giáo dục và phát triển thế hệ trẻ. Cam kết xây dựng môi trường học tập hiện đại, minh bạch và chất lượng cao.',
})

const stats = [
  { label: 'Sinh viên quản lý', value: '12,450', icon: Users, color: 'text-(--color-info-text)', bg: 'bg-(--color-info-bg)' },
  { label: 'Giảng viên', value: '458', icon: Award, color: 'text-link', bg: 'bg-(--color-info-bg)' },
  { label: 'GPA Trung bình', value: '3.24', icon: BarChart3, color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)' },
  { label: 'Cơ sở', value: '3', icon: Globe, color: 'text-(--color-warning-text)', bg: 'bg-(--color-warning-bg)' },
]

async function loadData() {
  loading.value = true
  error.value = null
  try {
    if (!ENABLE_MOCK_API) {
      const userData = auth.user
      if (userData) {
        profile.value = {
          ...profile.value,
          name: userData.fullName || userData.FullName || profile.value.name,
          email: userData.email || userData.Email || profile.value.email,
        }
      }
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}
onMounted(() => { loadData() })

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
    <div v-if="loading" class="flex items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-placeholder" />
    </div>
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20 text-center">
      <AlertCircle :size="48" class="text-(--color-danger-text) mb-4" />
      <p class="text-lg font-semibold text-muted">Đã có lỗi xảy ra</p>
      <p class="text-sm text-placeholder mt-1">{{ error }}</p>
      <button @click="loadData" class="mt-4 lg-button-secondary px-4 py-2 text-sm font-semibold">Thử lại</button>
    </div>
    <div v-else class="grid grid-cols-1 xl:grid-cols-3 gap-8">
      
      <!-- ── Left: Executive Profile Card ── -->
      <div class="space-y-4">
        <div class="surface-card border border-card rounded-2xl p-5 text-center">
           <div class="relative inline-block mb-5">
              <div class="h-24 w-24 rounded-3xl bg-(--color-info-bg) border border-(--color-info-text)/20 p-0.5 transition-transform">
                 <div class="h-full w-full rounded-[22px] surface-card flex items-center justify-center overflow-hidden relative">
                    <User v-if="!profile.avatar" :size="40" class="text-placeholder" />
                    <img v-else :src="profile.avatar" class="h-full w-full object-cover" />
                    
                    <button class="absolute inset-0 surface-modal opacity-0 hover:opacity-100 flex items-center justify-center text-inverse transition-opacity duration-300">
                       <Camera :size="20" />
                    </button>
                 </div>
              </div>
              <!-- Role Badge Overlay -->
              <div class="absolute -bottom-1 -right-1 surface-card border border-card p-0.5 rounded-xl shadow-sm">
                 <div class="bg-(--color-info-bg) text-(--color-info-text) px-2 py-0.5 rounded-lg text-[9px] font-semibold uppercase tracking-tighter border border-(--color-info-text)/20">Principal</div>
              </div>
           </div>

           <h3 class="text-lg font-semibold text-heading leading-tight mb-1">{{ profile.name }}</h3>
           <div class="inline-flex items-center gap-1.5 bg-(--color-info-bg) px-2 py-1 rounded-full mb-3 border border-(--color-info-text)/20">
              <ShieldCheck :size="12" class="text-(--color-info-text)" />
              <p class="text-[9px] font-semibold text-(--color-info-text) uppercase tracking-[0.1em]">{{ profile.staffId }}</p>
           </div>

           <p class="text-xs text-body leading-relaxed font-medium mb-5 italic">
              "{{ profile.bio }}"
           </p>

           <div class="space-y-3">
              <div class="flex items-center gap-3 p-3 surface-solid rounded-xl border border-default shadow-sm">
                 <div class="h-8 w-8 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text)">
                    <MapPin :size="16" />
                 </div>
                 <div class="text-left">
                    <p class="text-[8px] font-semibold text-label uppercase tracking-widest">Văn phòng</p>
                    <p class="text-[11px] font-bold text-heading">{{ profile.campus }}</p>
                 </div>
              </div>
              <div class="flex items-center gap-3 p-3 surface-solid rounded-xl border border-default shadow-sm">
                 <div class="h-8 w-8 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-link">
                    <ShieldCheck :size="16" />
                 </div>
                 <div class="text-left">
                    <p class="text-[8px] font-semibold text-label uppercase tracking-widest">Đơn vị quản lý</p>
                    <p class="text-[11px] font-bold text-heading">{{ profile.department }}</p>
                 </div>
              </div>
           </div>

           <button class="w-full mt-6 lg-button-primary py-3 text-[10px] font-semibold tracking-widest uppercase flex items-center justify-center gap-2">
              <Edit3 :size="16" /> Cập nhật thông tin
           </button>
        </div>

        <!-- Quick Stats Widget -->
        <div class="surface-card border border-card rounded-2xl p-3">
           <h4 class="text-[9px] font-semibold text-label uppercase tracking-[0.2em] mb-3 flex items-center gap-2">
              <BarChart3 :size="14" /> Chỉ số tổng quan
           </h4>
           <div class="grid grid-cols-2 gap-3">
              <div v-for="stat in stats" :key="stat.label" class="p-3 surface-solid rounded-xl border border-default text-center shadow-sm">
                 <div :class="['h-8 w-8 rounded-xl mx-auto mb-2 flex items-center justify-center border border-current/20', stat.bg, stat.color]">
                    <component :is="stat.icon" :size="16" />
                 </div>
                 <p class="text-[9px] font-bold text-label uppercase tracking-tighter">{{ stat.label }}</p>
                 <p class="text-base font-semibold text-heading">{{ stat.value }}</p>
              </div>
           </div>
        </div>
      </div>

      <!-- ── Right: Detailed Info & Control Center ── -->
      <div class="xl:col-span-2 space-y-8">
        
         <!-- Leadership Details & Security -->
         <div class="surface-card border border-card rounded-2xl overflow-hidden">
            <div class="p-4 flex items-center justify-between">
               <div>
                  <h4 class="text-xs font-semibold text-heading uppercase tracking-wide">Bảo mật & Liên hệ</h4>
                  <p class="text-[10px] text-muted font-medium mt-0.5">Thông tin liên lạc và thiết lập bảo mật.</p>
               </div>
               <div class="h-8 w-8 rounded-xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text) border border-(--color-warning-text)/20">
                  <ShieldAlert :size="18" />
               </div>
            </div>

            <div class="p-4 space-y-5">
               <!-- Contact Grid -->
               <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                  <div class="space-y-4">
                     <div class="flex items-center gap-3 group cursor-pointer">
                        <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-placeholder group-hover:bg-(--color-info-bg) group-hover:text-(--color-info-text) transition-all duration-300">
                           <Mail :size="18" />
                        </div>
                        <div>
                           <p class="text-[9px] font-semibold text-label uppercase tracking-widest mb-0.5">Email công vụ</p>
                           <p class="text-sm font-bold text-heading">{{ profile.email }}</p>
                        </div>
                     </div>
                     <div class="flex items-center gap-3 group cursor-pointer">
                        <div class="h-10 w-10 rounded-2xl surface-solid flex items-center justify-center text-placeholder group-hover:bg-(--color-success-bg) group-hover:text-(--color-success-text) transition-all duration-300">
                           <Phone :size="18" />
                        </div>
                        <div>
                           <p class="text-[9px] font-semibold text-label uppercase tracking-widest mb-0.5">Đường dây nóng</p>
                           <p class="text-sm font-bold text-heading">{{ profile.phone }}</p>
                        </div>
                     </div>
                  </div>

                  <!-- Password Card -->
                  <div class="p-4 surface-solid rounded-2xl border border-default relative overflow-hidden group">
                     <div class="flex items-start justify-between mb-3">
                        <div class="h-8 w-8 rounded-xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center border border-(--color-info-text)/20">
                           <KeyRound :size="18" />
                        </div>
                        <button class="text-[9px] font-semibold uppercase tracking-widest text-link lg-button-secondary px-3 py-1.5 rounded-lg active:scale-95">Đổi mật khẩu</button>
                     </div>
                     <h5 class="text-sm font-semibold text-heading mb-1">Bảo mật tài khoản</h5>
                     <p class="text-[10px] text-body leading-relaxed font-medium">Mã hóa 256-bit. Lần đổi gần nhất: 3 tháng trước.</p>
                  </div>
               </div>

               <!-- Login History -->
               <div>
                  <div class="flex items-center justify-between mb-3">
                     <h4 class="text-[10px] font-semibold text-label uppercase tracking-[0.2em] flex items-center gap-2">
                        <Clock :size="14" /> Hoạt động đăng nhập
                     </h4>
                     <button class="text-[9px] font-semibold text-link uppercase">Xem tất cả</button>
                  </div>
                  
                  <div class="space-y-2">
                     <div v-for="log in loginHistory" :key="log.id" class="flex items-center justify-between p-3 surface-solid rounded-xl border border-default group hover:border-(--border-input-focus) transition-all duration-300">
                        <div class="flex items-center gap-3">
                           <div :class="['h-8 w-8 rounded-xl flex items-center justify-center', log.status === 'current' ? 'bg-(--color-success-bg) text-(--color-success-text)' : 'surface-solid text-placeholder']">
                              <ShieldCheck v-if="log.status === 'current'" :size="16" />
                              <ChevronRight v-else :size="16" />
                           </div>
                           <div>
                              <p class="text-xs font-semibold text-heading">{{ log.device }}</p>
                              <p class="text-[10px] font-bold text-muted mt-0.5 flex items-center gap-2">
                                 <span>{{ log.ip }}</span>
                                 <span class="h-1 w-1 rounded-full bg-(--text-placeholder)"></span>
                                 <span>{{ log.time }}</span>
                              </p>
                           </div>
                        </div>
                        <div v-if="log.status === 'current'" class="flex items-center gap-1.5 bg-(--color-success-bg) text-(--color-success-text) px-2 py-1 rounded-lg border border-(--color-success-text)/20">
                           <div class="h-1.5 w-1.5 rounded-full bg-(--color-success-text) animate-pulse"></div>
                           <span class="text-[8px] font-semibold uppercase tracking-widest">Đang hoạt động</span>
                        </div>
                     </div>
                  </div>
               </div>
            </div>
         </div>

        <!-- Footer Actions -->
         <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="surface-card border border-card rounded-2xl p-3 flex items-center gap-3 cursor-pointer group">
               <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) transition-transform">
                  <Globe :size="18" />
               </div>
               <div class="flex-1">
                  <h4 class="text-xs font-semibold text-heading">Cài đặt ngôn ngữ</h4>
                  <p class="text-[9px] text-muted font-bold uppercase tracking-widest mt-0.5">Tiếng Việt (Mặc định)</p>
               </div>
               <ChevronRight :size="16" class="text-placeholder" />
            </div>
            
            <div class="surface-card border border-(--color-danger-text)/20 bg-(--color-danger-bg) rounded-2xl p-3 flex items-center justify-between group">
               <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-2xl bg-(--surface-card) flex items-center justify-center text-(--color-danger-text) transition-transform">
                     <LogOut :size="18" />
                  </div>
                  <div>
                     <h4 class="text-xs font-semibold text-heading">Đăng xuất</h4>
                     <p class="text-[10px] text-(--color-danger-text) font-medium">Kết thúc phiên làm việc</p>
                  </div>
               </div>
               <button class="h-8 w-8 rounded-xl bg-(--color-danger-text) text-inverse flex items-center justify-center shadow-sm active:scale-95 transition-all">
                  <LogOut :size="16" />
               </button>
            </div>
         </div>

      </div>

    </div>
  </PageContainer>
</template>
