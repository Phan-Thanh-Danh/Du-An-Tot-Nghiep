<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Wallet,
  ChevronDown,
  AlertTriangle,
  CheckCircle,
  Clock,
  ArrowRight,
  ChevronLeft,
  DollarSign
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

// Định dạng tiền tệ VND
function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

function goToPayment() {
  router.push({ path: '/parent/finance/payment', query: { studentId: activeChildId.value } })
}

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
            <Wallet :size="20" class="text-orange-600" />
            Công nợ học phí
          </h2>
          <p class="text-xs text-body">Kiểm tra số dư học phí và hạn đóng tiền học của con</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
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

    <!-- ── CẢNH BÁO QUÁ HẠN HỌC PHÍ (RED ALERT BANNER) ── -->
    <div
      v-if="currentChild.isOverdue"
      class="p-4 rounded-xl border border-red-200 dark:border-red-950/20 bg-red-50/50 dark:bg-red-950/10 flex gap-3 animate-pulse"
    >
      <AlertTriangle :size="20" class="text-red-500 flex-shrink-0 mt-0.5" />
      <div class="text-xs text-body space-y-1">
        <p class="font-extrabold text-red-600 dark:text-red-400">CẢNH BÁO: QUÁ HẠN THANH TOÁN HỌC PHÍ</p>
        <p class="text-slate-600 dark:text-slate-400 leading-relaxed font-semibold">
          Thời hạn nộp tiền học kỳ của con đã kết thúc vào ngày <strong>{{ currentChild.deadlineTuition }}</strong>. Nhà trường kính đề nghị phụ huynh hoàn tất số dư nợ còn lại (<strong>{{ formatCurrency(currentChild.balanceTuition) }}</strong>) sớm nhất để tránh ảnh hưởng đến kết quả thi học kỳ và hoạt động đăng ký môn học tiếp theo của học sinh.
        </p>
      </div>
    </div>

    <!-- ── TÓM TẮT CÔNG NỢ KPI CARDS ── -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
      
      <div class="lg-card-glass p-4 flex flex-col justify-between">
        <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
          <span class="text-[10px] text-muted font-bold uppercase">Tổng số tiền cần nộp</span>
          <span class="p-1.5 surface-input border border-card text-label rounded-lg">
            <DollarSign :size="13" />
          </span>
        </div>
        <div>
          <span class="text-lg font-extrabold text-heading">{{ formatCurrency(currentChild.totalTuition) }}</span>
          <span class="block text-[9px] text-muted font-bold mt-1">Giai đoạn Học kỳ 1 - 2025-2026</span>
        </div>
      </div>

      <div class="lg-card-glass p-4 flex flex-col justify-between">
        <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
          <span class="text-[10px] text-muted font-bold uppercase">Số tiền đã đóng</span>
          <span class="p-1.5 bg-emerald-50 dark:bg-emerald-950/20 text-emerald-600 rounded-lg">
            <CheckCircle :size="13" />
          </span>
        </div>
        <div>
          <span class="text-lg font-extrabold text-emerald-600 dark:text-emerald-400">{{ formatCurrency(currentChild.paidTuition) }}</span>
          <span class="block text-[9px] text-emerald-600 font-bold mt-1">Khớp lệnh giao dịch thực tế</span>
        </div>
      </div>

      <div class="lg-card-glass p-4 flex flex-col justify-between" :class="currentChild.balanceTuition > 0 ? 'border-orange-200 dark:border-orange-950/20' : ''">
        <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
          <span class="text-[10px] text-muted font-bold uppercase">Công nợ còn lại (Nợ)</span>
          <span class="p-1.5 bg-orange-50 dark:bg-orange-950/20 text-orange-600 rounded-lg">
            <AlertTriangle :size="13" />
          </span>
        </div>
        <div>
          <span class="text-lg font-extrabold text-orange-600 dark:text-orange-400">{{ formatCurrency(currentChild.balanceTuition) }}</span>
          <span class="block text-[9px] text-orange-600 font-bold mt-1" :class="currentChild.isOverdue ? 'text-red-500 font-extrabold' : ''">
            {{ currentChild.balanceTuition > 0 ? 'Chưa hoàn thành' : 'Đã hoàn thành học phí' }}
          </span>
        </div>
      </div>

      <div class="lg-card-glass p-4 flex flex-col justify-between" :class="currentChild.isOverdue ? 'border-red-200 dark:border-red-950/20' : ''">
        <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
          <span class="text-[10px] text-muted font-bold uppercase">Hạn chót thanh toán</span>
          <span class="p-1.5 rounded-lg" :class="currentChild.isOverdue ? 'bg-red-50 dark:bg-red-950/20 text-red-600' : 'bg-slate-100 dark:bg-slate-800 text-muted'">
            <Clock :size="13" />
          </span>
        </div>
        <div>
          <span class="text-base font-extrabold text-heading" :class="currentChild.isOverdue ? 'text-red-500' : ''">
            {{ currentChild.deadlineTuition }}
          </span>
          <span class="block text-[9px] text-muted font-bold mt-1.5" :class="currentChild.isOverdue ? 'text-red-500 font-extrabold' : ''">
            {{ currentChild.isOverdue ? 'Đã quá hạn đóng phí!' : 'Trong thời hạn cho phép' }}
          </span>
        </div>
      </div>

    </div>

    <!-- ── CHI TIẾT CÁC KHOẢN PHÍ ── -->
    <div class="lg-card-glass p-5 space-y-4">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 pb-3 border-b border-card">
        <div>
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
            Chi tiết các khoản phí học tập
          </h3>
          <p class="text-[10px] text-muted font-semibold mt-0.5">Danh sách chi tiết hóa đơn học phần kì này</p>
        </div>
        
        <button
          v-if="currentChild.balanceTuition > 0"
          @click="goToPayment"
          class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl text-xs font-bold flex items-center gap-1.5 transition shadow-md self-start sm:self-center"
        >
          Thanh toán trực tuyến
          <ArrowRight :size="13" />
        </button>
      </div>

      <!-- MOBILE: Danh sách thẻ (hiện trên điện thoại) -->
      <div class="sm:hidden space-y-3">
        <div
          v-for="(item, idx) in currentChild.feeItems"
          :key="'m-'+idx"
          class="flex items-center justify-between p-3 rounded-xl border border-card hover:bg-[var(--surface-card-hover)] transition"
        >
          <div class="flex-1 min-w-0 pr-3">
            <p class="text-xs font-semibold text-heading leading-snug">{{ item.name }}</p>
            <p class="text-xs font-extrabold text-body mt-0.5">{{ formatCurrency(item.amount) }}</p>
          </div>
          <span
            class="flex-shrink-0 inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold"
            :class="item.status === 'Đã nộp' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' : 'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400'"
          >
            <component :is="item.status === 'Đã nộp' ? CheckCircle : AlertTriangle" :size="11" />
            {{ item.status }}
          </span>
        </div>
      </div>

      <!-- DESKTOP: Bảng dữ liệu truyền thống (ẩn trên điện thoại) -->
      <div class="hidden sm:block overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse min-w-[600px]">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-3 px-3">Tên khoản phí / Học phần</th>
              <th class="py-3 px-3 text-right">Mức phí cần nộp</th>
              <th class="py-3 px-3 text-right">Trạng thái đóng</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr
              v-for="(item, idx) in currentChild.feeItems"
              :key="idx"
              class="hover:bg-[var(--surface-table-row-hover)] transition"
            >
              <td class="py-3 px-3 font-semibold text-heading">{{ item.name }}</td>
              <td class="py-3 px-3 text-right font-extrabold text-body">{{ formatCurrency(item.amount) }}</td>
              <td class="py-3 px-3 text-right">
                <span
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold"
                  :class="
                    item.status === 'Đã nộp' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' :
                    'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400'
                  "
                >
                  <component :is="item.status === 'Đã nộp' ? CheckCircle : AlertTriangle" :size="11" />
                  {{ item.status }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
