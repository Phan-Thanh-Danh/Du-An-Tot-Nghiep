<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  User,
  Mail,
  Phone,
  MapPin,
  Calendar,
  Building,
  ShieldCheck,
  Edit,
  Camera,
  Loader2,
  AlertTriangle
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { useAuthStore } from '@/stores/auth'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'

const authStore = useAuthStore()
const router = useRouter()

const loading = ref(true)
const error = ref('')
const profile = ref({
  id: null,
  name: '',
  email: '',
  phone: '',
  campus: '',
  role: '',
  createdAt: null,
  lastLogin: null,
  status: '',
})

const initials = computed(() => {
  const name = profile.value.name || authStore.displayName || 'Phụ huynh'
  return name.split(' ').filter(Boolean).slice(-2).map(w => w[0]).join('').toUpperCase()
})

const displayName = computed(() => profile.value.name || authStore.displayName || 'Phụ huynh')
const displayEmail = computed(() => profile.value.email || authStore.user?.email || '')
const displayPhone = computed(() => profile.value.phone || '')
const displayCampus = computed(() => profile.value.campus || '')

function formatDate(dateStr) {
  if (!dateStr) return 'Chưa cập nhật'
  const d = new Date(dateStr)
  if (isNaN(d.getTime())) return 'Chưa cập nhật'
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  const hh = String(d.getHours()).padStart(2, '0')
  const mm = String(d.getMinutes()).padStart(2, '0')
  return `${day}/${month}/${year} ${hh}:${mm}`
}

function handleEdit() {
  alert('Tính năng cập nhật thông tin đang được hoàn thiện.')
}

async function loadProfile() {
  loading.value = true
  error.value = ''
  try {
    const res = await parentApi.getProfile()
    const data = res?.data
    if (data) {
      profile.value = {
        id: data.id,
        name: data.name || '',
        email: data.email || '',
        phone: data.phone || '',
        campus: data.campus || '',
        role: data.role || '',
        createdAt: data.createdAt,
        lastLogin: data.lastLogin,
        status: data.status || '',
      }
    }
  } catch (err) {
    error.value = err.message || 'Không thể tải thông tin hồ sơ.'
  } finally {
    loading.value = false
  }
}

onMounted(loadProfile)
</script>

<template>
  <div class="space-y-6 pb-6">

    <!-- ── LOADING ── -->
    <div v-if="loading" class="p-4">
      <SkeletonTable :rows="4" :columns="2" />
    </div>

    <!-- ── ERROR ── -->
    <div v-else-if="error" class="lg-card-glass p-8 text-center">
      <AlertTriangle :size="28" class="text-orange-500 mx-auto mb-3" />
      <p class="text-sm font-bold text-heading mb-1">Không thể tải hồ sơ</p>
      <p class="text-xs text-muted">{{ error }}</p>
      <button @click="loadProfile" class="mt-4 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Thử lại
      </button>
    </div>

    <template v-else>
      <!-- Header Banner & Avatar -->
      <div class="relative rounded-t-[32px] rounded-b-[24px] bg-gradient-to-br from-orange-600 via-orange-500 to-amber-500 p-6 sm:p-8 overflow-hidden shadow-(--lg-shadow-md)">
        <!-- Background pattern -->
        <div class="absolute inset-0 opacity-10 bg-[url('data:image/svg+xml,%3Csvg width=\'60\' height=\'60\' viewBox=\'0 0 60 60\' xmlns=\'http://www.w3.org/2000/svg\'%3E%3Cg fill=\'none\' fill-rule=\'evenodd\'%3E%3Cg fill=\'%23ffffff\' fill-opacity=\'1\'%3E%3Cpath d=\'M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z\'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E')]"></div>
        
        <div class="relative z-10 flex flex-col sm:flex-row items-center sm:items-end gap-6 text-center sm:text-left">
          <div class="relative group">
            <div class="h-28 w-28 rounded-full bg-white flex items-center justify-center shadow-lg border-4 border-white/30 text-4xl font-extrabold text-orange-600 overflow-hidden">
              <span>{{ initials }}</span>
            </div>
            <button class="absolute bottom-0 right-0 p-2 bg-white text-orange-600 rounded-full shadow-md hover:bg-orange-50 transition-colors">
              <Camera :size="16" stroke-width="2.5" />
            </button>
          </div>
          <div class="flex-1 text-white pb-2">
            <h1 class="text-2xl sm:text-3xl font-extrabold tracking-tight mb-1">{{ displayName }}</h1>
            <p class="text-orange-100 font-medium text-sm sm:text-base flex items-center justify-center sm:justify-start gap-2">
              <ShieldCheck :size="18" /> Phụ huynh học sinh
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
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <User :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Họ và tên</p>
                <p class="text-sm font-bold text-heading">{{ displayName }}</p>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <Calendar :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Ngày tạo tài khoản</p>
                <p class="text-sm font-bold text-heading">{{ formatDate(profile.createdAt) }}</p>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <Calendar :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Đăng nhập lần cuối</p>
                <p class="text-sm font-bold text-heading">{{ formatDate(profile.lastLogin) }}</p>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <Building :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Cơ sở đăng ký</p>
                <p class="text-sm font-bold text-heading">{{ displayCampus || 'Chưa cập nhật' }}</p>
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
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <Phone :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Số điện thoại</p>
                <p class="text-sm font-bold text-heading">{{ displayPhone || 'Chưa cập nhật' }}</p>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <Mail :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Địa chỉ Email</p>
                <p class="text-sm font-bold text-heading">{{ displayEmail || 'Chưa cập nhật' }}</p>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <MapPin :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Trạng thái tài khoản</p>
                <span v-if="profile.status === 'hoat_dong'" class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400">
                  <span class="h-1.5 w-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                  Đang hoạt động
                </span>
                <span v-else class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold bg-red-100 text-red-600 dark:bg-red-950/30 dark:text-red-400">
                  <span class="h-1.5 w-1.5 rounded-full bg-red-500"></span>
                  {{ profile.status || 'Không xác định' }}
                </span>
              </div>
            </div>
            <div class="flex items-start gap-4 p-3 rounded-xl hover:bg-(--surface-input) transition-colors">
              <ShieldCheck :size="18" class="text-muted shrink-0 mt-0.5" />
              <div>
                <p class="text-xs font-semibold text-muted uppercase tracking-wider mb-0.5">Vai trò trong hệ thống</p>
                <p class="text-sm font-bold text-heading">{{ profile.role === 'phu_huynh' ? 'Phụ huynh học sinh' : (profile.role || 'Phụ huynh') }}</p>
              </div>
            </div>
          </div>
        </div>

      </div>
    </template>
  </div>
</template>
