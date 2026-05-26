<script setup>
import { ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { Lock, ArrowLeft, ShieldCheck, KeyRound, AlertCircle } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const router = useRouter()
const popupStore = usePopupStore()

const passwords = ref({
  current: '',
  new: '',
  confirm: ''
})

function handlePasswordChange() {
  popupStore.success('Đã đổi mật khẩu', 'Mật khẩu của bạn đã được cập nhật.')
  router.push('/teacher/profile')
}
</script>

<template>
  <div class="space-y-8 pb-10 text-slate-800 animate-fade-in max-w-5xl mx-auto">
    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white p-5 rounded-2xl border border-slate-100 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <router-link to="/teacher/profile" class="h-10 w-10 rounded-2xl bg-gradient-to-br from-slate-100 to-slate-50 flex items-center justify-center text-slate-500 shadow-sm border border-slate-200 hover:scale-105 hover:text-blue-600 hover:border-blue-200 transition-all duration-300">
           <ArrowLeft :size="28" stroke-width="2.5" />
        </router-link>
        <div>
          <h1 class="text-xl md:text-xl font-black text-slate-900 tracking-tight">Đổi mật khẩu</h1>
          <p class="text-sm font-medium text-slate-500 mt-1">Cập nhật mật khẩu để bảo vệ tài khoản của bạn.</p>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      
      <!-- Security Information -->
      <div class="lg:col-span-1 space-y-4">
         <div class="bg-white rounded-2xl p-5 border border-slate-100 shadow-sm relative overflow-hidden group">
            <div class="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-blue-50 to-transparent rounded-bl-[100px] -z-10 group-hover:scale-110 transition-transform duration-500"></div>
            <div class="h-10 w-10 rounded-2xl bg-blue-50 text-blue-600 flex items-center justify-center mb-4 shadow-inner border border-blue-100">
               <ShieldCheck :size="28" stroke-width="2" />
            </div>
            <h3 class="text-lg font-black text-slate-800 mb-4">Yêu cầu mật khẩu</h3>
            <ul class="space-y-4 text-sm font-medium text-slate-500">
               <li class="flex items-center gap-3">
                  <div class="w-2 h-2 rounded-full bg-cyan-400 shadow-[0_0_8px_rgba(34,211,238,0.6)]"></div> Ít nhất 8 ký tự
               </li>
               <li class="flex items-center gap-3">
                  <div class="w-2 h-2 rounded-full bg-blue-400 shadow-[0_0_8px_rgba(96,165,250,0.6)]"></div> Chứa ít nhất một chữ hoa
               </li>
               <li class="flex items-center gap-3">
                  <div class="w-2 h-2 rounded-full bg-indigo-400 shadow-[0_0_8px_rgba(129,140,248,0.6)]"></div> Chứa ít nhất một số
               </li>
               <li class="flex items-center gap-3">
                  <div class="w-2 h-2 rounded-full bg-violet-400 shadow-[0_0_8px_rgba(167,139,250,0.6)]"></div> Không trùng mật khẩu cũ
               </li>
            </ul>
         </div>
      </div>

      <!-- Change Password Form -->
      <div class="lg:col-span-2 bg-white rounded-2xl p-5 md:p-10 border border-slate-100 shadow-sm relative overflow-hidden">
         <div class="space-y-4 relative z-10">
            <div class="space-y-2">
               <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Mật khẩu hiện tại</label>
               <div class="relative">
                 <KeyRound :size="18" class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" />
                 <input type="password" v-model="passwords.current" placeholder="Nhập mật khẩu hiện tại" class="w-full rounded-[20px] border border-slate-200 bg-slate-50/50 pl-12 pr-5 py-4 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
               </div>
            </div>
            
            <hr class="border-slate-100 my-8" />
            
            <div class="space-y-2">
               <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Mật khẩu mới</label>
               <div class="relative">
                 <Lock :size="18" class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" />
                 <input type="password" v-model="passwords.new" placeholder="Nhập mật khẩu mới" class="w-full rounded-[20px] border border-slate-200 bg-slate-50/50 pl-12 pr-5 py-4 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
               </div>
            </div>
            
            <div class="space-y-2">
               <label class="text-[11px] font-black text-slate-500 uppercase tracking-widest pl-1">Xác nhận mật khẩu mới</label>
               <div class="relative">
                 <Lock :size="18" class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400" />
                 <input type="password" v-model="passwords.confirm" placeholder="Nhập lại mật khẩu mới" class="w-full rounded-[20px] border border-slate-200 bg-slate-50/50 pl-12 pr-5 py-4 text-sm font-medium text-slate-800 outline-none focus:border-blue-400 focus:bg-white focus:ring-4 focus:ring-blue-50 transition-all shadow-sm" />
               </div>
            </div>

            <div class="pt-8 flex flex-col sm:flex-row gap-4 items-center justify-between">
               <div class="flex items-center gap-2 text-amber-600 text-[10px] font-black uppercase tracking-widest bg-amber-50 px-4 py-2.5 rounded-xl border border-amber-100/50">
                  <AlertCircle :size="16" />
                  Bạn sẽ bị đăng xuất sau khi đổi
               </div>
               <div class="flex gap-3 w-full sm:w-auto">
                 <router-link to="/teacher/profile" class="flex-1 sm:flex-none text-center rounded-[20px] bg-white border border-slate-200 px-5 py-3.5 text-sm font-bold text-slate-500 hover:bg-slate-50 hover:text-slate-700 transition-colors shadow-sm">Hủy</router-link>
                 <button @click="handlePasswordChange" class="flex-1 sm:flex-none rounded-[20px] bg-gradient-to-br from-blue-500 to-cyan-500 px-5 py-3.5 text-sm font-bold text-white shadow-md shadow-blue-200 hover:shadow-lg hover:-translate-y-0.5 active:translate-y-0 transition-all">Cập nhật mật khẩu</button>
               </div>
            </div>
         </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in {
  animation: fade-in-up 0.4s ease-out forwards;
}
</style>
