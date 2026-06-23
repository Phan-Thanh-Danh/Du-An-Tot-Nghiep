<script setup>
import { ref } from 'vue'
import {
  User,
  Mail,
  Phone,
  MapPin,
  Calendar,
  CreditCard,
  Building,
  ShieldCheck,
  Edit,
  Camera
} from 'lucide-vue-next'
import { parentProfile } from '@/components/PhuHuynh/data/parentData.js'
import { mockParentUser } from '@/components/PhuHuynh/data/menuData.js'

const isEditing = ref(false)

function handleEdit() {
  isEditing.value = true
  // Mock edit action
  setTimeout(() => {
    isEditing.value = false
    alert('Tính năng cập nhật thông tin đang được hoàn thiện.')
  }, 500)
}
</script>

<template>
  <div class="space-y-6 pb-6">
    <!-- Header Banner & Avatar -->
    <div class="relative rounded-t-[32px] rounded-b-[24px] bg-gradient-to-br from-orange-600 via-orange-500 to-amber-500 p-6 sm:p-8 overflow-hidden shadow-[var(--lg-shadow-md)]">
      <!-- Background pattern -->
      <div class="absolute inset-0 opacity-10 bg-[url('data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cg fill=\'none\' fill-rule=\'evenodd\'%3E%3Cg fill=\'%23ffffff\' fill-opacity=\'1\'%3E%3Cpath d=\'M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E')]"></div>
      
      <div class="relative z-10 flex flex-col sm:flex-row items-center sm:items-end gap-6 text-center sm:text-left">
        <div class="relative group">
          <div class="h-28 w-28 rounded-full bg-white flex items-center justify-center shadow-lg border-4 border-white/30 text-4xl font-extrabold text-orange-600 overflow-hidden">
            <img v-if="mockParentUser.avatar" :src="mockParentUser.avatar" class="h-full w-full object-cover" />
            <span v-else>{{ mockParentUser.initials }}</span>
          </div>
          <button class="absolute bottom-0 right-0 p-2 bg-white text-orange-600 rounded-full shadow-md hover:bg-orange-50 transition-colors">
            <Camera :size="16" stroke-width="2.5" />
          </button>
        </div>
        <div class="flex-1 text-white pb-2">
          <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight mb-1">{{ parentProfile.name }}</h1>
          <p class="text-orange-100 font-medium text-sm sm:text-base flex items-center justify-center sm:justify-start gap-2">
            <ShieldCheck :size="18" /> {{ mockParentUser.relation }}
          </p>
        </div>
        <div class="pb-2">
          <button @click="handleEdit" class="inline-flex items-center gap-2 bg-white/20 hover:bg-white/30 text-white border border-white/30 backdrop-blur-md px-5 py-2.5 rounded-xl text-sm font-bold transition-all active:scale-95 shadow-sm">
            <Edit :size="16" /> Chỉnh sửa hồ sơ
          </button>
        </div>
      </div>
    </div>

    <!-- Info Sections -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      
      <!-- Thông tin cá nhân -->
      <div class="lg-glass p-6 rounded-[24px]">
        <div class="flex items-center gap-3 border-b border-card pb-4 mb-5">
          <div class="h-10 w-10 rounded-xl bg-orange-100 dark:bg-orange-950/30 text-orange-600 flex items-center justify-center shadow-sm">
            <User :size="20" stroke-width="2.5" />
          </div>
          <h2 class="text-lg font-bold text-heading">Thông tin cá nhân</h2>
        </div>
        <div class="space-y-4">
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <User :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Họ và tên</p>
              <p class="text-sm font-bold text-heading">{{ parentProfile.name }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <Calendar :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Ngày sinh</p>
              <p class="text-sm font-bold text-heading">{{ parentProfile.dob }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <CreditCard :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Số CCCD / CMND</p>
              <p class="text-sm font-bold text-heading">{{ parentProfile.cccd }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <Building :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Cơ sở đăng ký</p>
              <p class="text-sm font-bold text-heading">{{ mockParentUser.campus }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Thông tin liên hệ -->
      <div class="lg-glass p-6 rounded-[24px]">
        <div class="flex items-center gap-3 border-b border-card pb-4 mb-5">
          <div class="h-10 w-10 rounded-xl bg-orange-100 dark:bg-orange-950/30 text-orange-600 flex items-center justify-center shadow-sm">
            <Phone :size="20" stroke-width="2.5" />
          </div>
          <h2 class="text-lg font-bold text-heading">Thông tin liên hệ</h2>
        </div>
        <div class="space-y-4">
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <Phone :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Số điện thoại</p>
              <p class="text-sm font-bold text-heading">{{ parentProfile.phone }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <Mail :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Địa chỉ Email</p>
              <p class="text-sm font-bold text-heading">{{ parentProfile.email }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <MapPin :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Địa chỉ thường trú</p>
              <p class="text-sm font-bold text-heading leading-snug">{{ parentProfile.permanentAddress }}</p>
            </div>
          </div>
          <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-[var(--surface-input)] transition-colors">
            <MapPin :size="18" class="text-muted shrink-0 mt-0.5" />
            <div>
              <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Nơi ở hiện tại</p>
              <p class="text-sm font-bold text-heading leading-snug">{{ parentProfile.temporaryAddress }}</p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>
