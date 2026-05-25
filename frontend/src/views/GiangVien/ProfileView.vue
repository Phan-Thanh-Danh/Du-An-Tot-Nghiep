<script setup>
import { ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { 
  User, Mail, Phone, Lock, Save, Camera, 
  ShieldCheck, MapPin, Briefcase, Calendar 
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const popupStore = usePopupStore()

const user = ref({
  name: 'TS. Nguyễn Minh Khoa',
  email: 'khoa.nm@lms.edu.vn',
  phone: '0901 234 567',
  department: 'Khoa Công nghệ thông tin',
  position: 'Giảng viên cấp cao',
  joinDate: '15/01/2020',
  address: 'Quận 7, TP. Hồ Chí Minh'
})

function handleUpdate() {
  popupStore.success('Đã cập nhật', 'Thông tin cá nhân đã được cập nhật thành công.')
}
</script>

<template>
  <div class="space-y-6 pb-10 text-slate-800">
    <!-- Header Hero -->
    <div class="relative h-48 rounded-[32px] bg-gradient-to-r from-blue-600 to-blue-600 overflow-hidden shadow-2xl">
       <div class="absolute inset-0 opacity-20">
          <div class="absolute top-0 right-0 w-64 h-64 bg-white rounded-full -translate-y-1/2 translate-x-1/2 blur-3xl"></div>
          <div class="absolute bottom-0 left-0 w-48 h-48 bg-blue-300 rounded-full translate-y-1/2 -translate-x-1/2 blur-3xl"></div>
       </div>
    </div>

    <!-- Profile Info Card -->
    <div class="relative -mt-20 px-8">
       <div class="lg-card-glass p-8 border-white/50 shadow-xl bg-white/80 backdrop-blur-2xl">
          <div class="flex flex-col md:flex-row gap-8">
             <!-- Avatar Area -->
             <div class="relative shrink-0">
                <div class="h-32 w-32 rounded-3xl bg-white p-1 shadow-xl border border-slate-100">
                   <div :class="['h-full w-full rounded-2xl bg-gradient-to-br flex items-center justify-center text-white font-bold text-4xl overflow-hidden', 
                      authStore.hasRole('Principal') ? 'from-blue-600 to-cyan-600' : 'from-blue-600 to-cyan-500']">
                      <img v-if="authStore.user?.avatar" :src="authStore.user.avatar" class="h-full w-full object-cover" />
                      <span v-else>{{ authStore.initials || 'TN' }}</span>
                   </div>
                </div>
                <button class="absolute -bottom-2 -right-2 h-10 w-10 rounded-full bg-slate-900 text-white flex items-center justify-center shadow-lg border-2 border-white hover:scale-110 transition-transform" title="Đổi ảnh đại diện">
                   <Camera :size="18" />
                </button>
             </div>

             <!-- Details -->
             <div class="flex-1 space-y-6">
                <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
                   <div>
                      <h1 class="text-3xl font-black text-slate-800 tracking-tight">{{ user.name }}</h1>
                      <div class="flex items-center gap-3 mt-1">
                         <span class="text-xs font-bold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-md uppercase tracking-wider">{{ user.position }}</span>
                         <span class="text-xs font-bold text-slate-400">• {{ user.department }}</span>
                      </div>
                   </div>
                    <div class="flex gap-2">
                       <router-link to="/teacher/change-password" class="lg-button-secondary py-2.5 px-5 text-xs font-bold flex items-center gap-2">
                          <Lock :size="16" /> Đổi mật khẩu
                       </router-link>
                       <button @click="handleUpdate" class="lg-button-primary py-2.5 px-6 text-xs font-bold flex items-center gap-2" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
                          <Save :size="16" /> Cập nhật hồ sơ
                       </button>
                    </div>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 pt-4 border-t border-slate-100">
                   <div class="flex items-center gap-3">
                      <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                         <Mail :size="20" />
                      </div>
                      <div>
                         <p class="text-[10px] font-black text-slate-400 uppercase">Email</p>
                         <p class="text-sm font-bold text-slate-700">{{ user.email }}</p>
                      </div>
                   </div>
                   <div class="flex items-center gap-3">
                      <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                         <Phone :size="20" />
                      </div>
                      <div>
                         <p class="text-[10px] font-black text-slate-400 uppercase">Số điện thoại</p>
                         <p class="text-sm font-bold text-slate-700">{{ user.phone }}</p>
                      </div>
                   </div>
                   <div class="flex items-center gap-3">
                      <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                         <Calendar :size="20" />
                      </div>
                      <div>
                         <p class="text-[10px] font-black text-slate-400 uppercase">Ngày tham gia</p>
                         <p class="text-sm font-bold text-slate-700">{{ user.joinDate }}</p>
                      </div>
                   </div>
                </div>
             </div>
          </div>
       </div>
    </div>

    <!-- Additional Info Sections -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 px-8">
       <div class="lg-card-glass p-8 border-slate-100">
          <h2 class="text-lg font-bold mb-6 flex items-center gap-2">
             <ShieldCheck :size="20" class="text-emerald-500" /> Thông tin xác thực
          </h2>
          <div class="space-y-4">
             <div class="flex justify-between p-4 rounded-2xl bg-slate-50">
                <span class="text-sm text-slate-500 font-medium">Trạng thái tài khoản</span>
                <span class="text-xs font-bold text-emerald-600 bg-emerald-50 px-2 py-1 rounded-lg border border-emerald-100">Đã xác minh</span>
             </div>
             <div class="flex justify-between p-4 rounded-2xl bg-slate-50">
                <span class="text-sm text-slate-500 font-medium">Lần đăng nhập cuối</span>
                <span class="text-xs font-bold text-slate-600">Hôm nay, 08:30 AM</span>
             </div>
          </div>
       </div>

       <div class="lg-card-glass p-8 border-slate-100">
          <h2 class="text-lg font-bold mb-6 flex items-center gap-2">
             <MapPin :size="20" class="text-blue-500" /> Địa chỉ công tác
          </h2>
          <div class="space-y-4">
             <div class="p-4 rounded-2xl bg-slate-50">
                <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Cơ sở chính</p>
                <p class="text-sm text-slate-700 font-bold">Cơ sở 1 - Đống Đa, Hà Nội</p>
             </div>
             <div class="p-4 rounded-2xl bg-slate-50">
                <p class="text-[10px] font-black text-slate-400 uppercase mb-1">Văn phòng</p>
                <p class="text-sm text-slate-700 font-bold">Tầng 4, Phòng Giảng viên 402</p>
             </div>
          </div>
       </div>
    </div>

  </div>
</template>
