<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Key,
  ChevronDown,
  ChevronLeft,
  CheckCircle2,
  XCircle,
  AlertCircle,
  HelpCircle,
  Lock
} from 'lucide-vue-next'
import { childrenData, getActiveChild, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'

const route = useRoute()
const router = useRouter()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
}

// Lấy danh sách quyền truy cập
const accessRights = computed(() => {
  return currentChild.value.accessRights || []
})

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div class="flex items-center gap-2">
        <button
          @click="goBack"
          class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
          title="Quay lại"
        >
          <ChevronLeft :size="18" />
        </button>
        <div>
          <h2 class="text-lg font-bold text-heading flex items-center gap-2">
            <Key :size="20" class="text-orange-600" />
            Quyền truy cập được cấp
          </h2>
          <p class="text-xs text-body">Minh bạch các quyền hạn xem dữ liệu do học sinh phê duyệt qua liên kết ParentLink</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-1.8 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild.name }}</span>
          </div>
          <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
        </button>

        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 translate-y-2 scale-95"
          enter-to-class="opacity-100 translate-y-0 scale-100"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0 scale-100"
          leave-to-class="opacity-0 translate-y-2 scale-95"
        >
          <div
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-[var(--lg-shadow-md)]"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-[var(--surface-card-hover)]"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── THÔNG TIN CHÍNH VÀ BANNER QUY TẮC ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Bảng quyền truy cập (2 cols) -->
      <div class="lg-col-span-2 lg-card-glass p-5 space-y-4">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-3 border-b border-card">
          Danh sách quyền của học sinh: <span class="text-orange-600 font-extrabold">{{ currentChild.name }}</span>
        </h3>

        <div class="divide-y divide-card">
          <div
            v-for="right in accessRights"
            :key="right.code"
            class="py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-3 first:pt-1 last:pb-1"
          >
            <div class="space-y-1 max-w-md">
              <h4 class="text-xs font-bold text-heading leading-snug">{{ right.name }}</h4>
              <p class="text-[11px] text-body leading-relaxed">{{ right.desc }}</p>
            </div>

            <!-- Badge trạng thái -->
            <div class="flex-shrink-0 flex items-center">
              <span
                v-if="right.active"
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-bold bg-emerald-50 text-emerald-700 dark:bg-emerald-950/20 dark:text-emerald-400 border border-emerald-200 dark:border-emerald-900/30"
              >
                <CheckCircle2 :size="12" /> Đang hoạt động
              </span>
              <span
                v-else
                class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-bold bg-red-50 text-red-700 dark:bg-red-950/20 dark:text-red-400 border border-red-200 dark:border-red-900/30"
              >
                <XCircle :size="12" /> Đã bị thu hồi
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Banner giải thích nghiệp vụ (1 col) -->
      <div class="space-y-4">
        
        <!-- Box giải thích nghiệp vụ -->
        <div class="lg-card-glass p-5 border border-orange-200 dark:border-orange-950/20 bg-orange-50/5 dark:bg-orange-950/5 space-y-4">
          <h4 class="text-xs font-bold text-heading flex items-center gap-1.5 uppercase tracking-wide">
            <Lock :size="15" class="text-orange-600" />
            Quy định phân quyền
          </h4>

          <div class="text-[11px] text-body space-y-3 leading-relaxed">
            <p>
              Tất cả các quyền truy cập thông tin hiển thị tại trang này đều được **phê duyệt trực tiếp từ tài khoản của học sinh** (qua chức năng thiết lập liên kết ParentLink).
            </p>
            <p>
              <strong class="text-orange-600 dark:text-orange-400">Lưu ý quan trọng:</strong> Phụ huynh không có quyền tự kích hoạt hoặc vô hiệu hóa các tính năng xem dữ liệu này. 
            </p>
            <p>
              Nếu muốn thay đổi hoặc bổ sung quyền (ví dụ: mở khóa quyền xem chi tiết bài làm kiểm tra), phụ huynh vui lòng trao đổi trực tiếp với con để cập nhật thiết lập trên trang cá nhân của học sinh.
            </p>
          </div>

          <div class="flex items-start gap-2 p-3 bg-slate-50 dark:bg-slate-900/40 border border-card rounded-xl">
            <AlertCircle :size="14" class="text-orange-600 flex-shrink-0 mt-0.5" />
            <span class="text-[10px] text-muted leading-relaxed font-semibold">
              Hệ thống bảo vệ quyền riêng tư cá nhân và tính độc lập học tập của người học theo quy định quy chế LMS.
            </span>
          </div>
        </div>

        <!-- Box trợ giúp khi có sự cố -->
        <div class="lg-card-glass p-5 space-y-3">
          <h4 class="text-xs font-bold text-heading flex items-center gap-1.5 uppercase tracking-wide">
            <HelpCircle :size="15" class="text-orange-600" />
            Bạn cần trợ giúp?
          </h4>
          <p class="text-[11px] text-body leading-relaxed">
            Nếu có sự sai lệch thông tin liên kết hoặc cần hỗ trợ cấu hình kỹ thuật từ trường học, phụ huynh có thể liên hệ bộ phận Khảo thí & Công nghệ thông tin theo hotline:
          </p>
          <div class="p-3 bg-slate-50 dark:bg-slate-900/40 rounded-xl border border-card text-center">
            <p class="text-xs font-extrabold text-heading">1900 1234 (Nhánh 2)</p>
            <p class="text-[9px] text-muted mt-0.5">Thời gian làm việc: 08:00 - 17:00 (T2 - T7)</p>
          </div>
        </div>

      </div>

    </div>
  </div>
</template>

<style scoped>
</style>
