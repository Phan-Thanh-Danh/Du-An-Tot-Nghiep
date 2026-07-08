<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  CreditCard,
  ChevronDown,
  Smartphone,
  ChevronLeft,
  Send,
  CheckCircle,
  HelpCircle
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref('')
const submitting = ref(false)

const children = ref([])
const tuitionData = ref(null)

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0] || null
})

const totalDue = computed(() => tuitionData.value?.totalDue || 0)

const paymentMode = ref('all')
const customAmountInput = ref(0)

const amountToPay = computed(() => {
  if (paymentMode.value === 'all') {
    return totalDue.value
  }
  return Number(customAmountInput.value) || 0
})

const paymentMethod = ref('vietqr')

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const [childrenRes, tuitionRes] = await Promise.all([
      parentApi.getChildren(),
      parentApi.getChildTuition(activeChildId.value)
    ])
    children.value = childrenRes?.data || []
    tuitionData.value = tuitionRes?.data || null
  } catch (err) {
    error.value = err.message || 'Không thể tải dữ liệu.'
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

function selectChild(id) {
  activeChildId.value = id
  localStorage.setItem('parent_active_student_id', id)
  dropdownOpen.value = false
  router.replace({ query: { studentId: id } })
  loadData()
}

function formatCurrency(amount) {
  if (amount == null) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

async function handlePayment() {
  if (amountToPay.value <= 0) {
    popupStore.warning('Số tiền không hợp lệ', 'Vui lòng chọn số tiền lớn hơn 0 để thanh toán.')
    return
  }
  if (amountToPay.value > totalDue.value) {
    popupStore.warning('Vượt quá công nợ', `Số tiền cần thanh toán không được vượt quá số dư nợ hiện tại: ${formatCurrency(totalDue.value)}.`)
    return
  }

  submitting.value = true
  try {
    const res = await parentApi.makePayment({
      childId: activeChildId.value,
      amount: amountToPay.value,
      paymentMethod: paymentMethod.value
    })
    if (res?.success !== false) {
      popupStore.success(
        'Thanh toán thành công',
        `Hệ thống đã ghi nhận yêu cầu thanh toán số tiền ${formatCurrency(amountToPay.value)}.`
      )
      router.push({ path: '/parent/finance/transactions', query: { studentId: activeChildId.value } })
    } else {
      popupStore.error('Thanh toán thất bại', res?.message || 'Không thể xử lý thanh toán.')
    }
  } catch (err) {
    popupStore.error('Thanh toán thất bại', err.message || 'Có lỗi xảy ra khi thanh toán.')
  } finally {
    submitting.value = false
  }
}

function goBack() {
  router.push('/parent/finance/tuition')
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
            <CreditCard :size="20" class="text-orange-600" />
            Thanh toán học phí trực tuyến
          </h2>
          <p class="text-xs text-body">Đóng học phí tiện lợi qua cổng thanh toán bảo mật VietQR, ví điện tử</p>
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
              {{ currentChild?.name?.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild?.name }}</span>
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
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-(--lg-shadow-md)"
          >
            <button
              v-for="child in children"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── LOADING ── -->
    <div v-if="loading" class="lg-card-glass p-8 text-center">
      <div class="animate-spin h-8 w-8 border-2 border-orange-600 border-t-transparent rounded-full mx-auto mb-3"></div>
      <p class="text-xs text-muted font-semibold">Đang tải dữ liệu...</p>
    </div>

    <!-- ── ERROR ── -->
    <div v-else-if="error" class="lg-card-glass p-8 text-center">
      <p class="text-sm font-bold text-heading mb-1">Đã xảy ra lỗi</p>
      <p class="text-xs text-muted">{{ error }}</p>
      <button @click="loadData" class="mt-4 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Thử lại
      </button>
    </div>

    <div v-else-if="totalDue === 0" class="lg-card-glass p-8 text-center flex flex-col items-center justify-center gap-3">
      <CheckCircle :size="48" class="text-emerald-500" />
      <h3 class="text-sm font-bold text-heading">Không có công nợ cần thanh toán</h3>
      <p class="text-xs text-body max-w-md">
        Học sinh <strong>{{ currentChild?.name }}</strong> đã hoàn thành 100% học phí kì này. Xin chân thành cảm ơn phụ huynh!
      </p>
      <button @click="router.push('/parent/finance/tuition')" class="mt-2 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Xem chi tiết công nợ
      </button>
    </div>

    <!-- ── FORM THANH TOÁN CHÍNH ── -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Cột trái: Form nhập số tiền và lựa chọn đợt đóng (2/3 width) -->
      <div class="lg:col-span-2 space-y-6">
        
        <!-- Nhập khoản phí đóng -->
        <div class="lg-card-glass p-5 space-y-4">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card">
            1. Chọn khoản phí thanh toán
          </h3>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <!-- Đóng toàn bộ -->
            <label
              class="p-4 rounded-xl border cursor-pointer flex flex-col justify-between transition"
              :class="paymentMode === 'all' ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-heading">Đóng toàn bộ công nợ</span>
                <input type="radio" value="all" v-model="paymentMode" class="text-orange-600 focus:ring-orange-500" />
              </div>
              <span class="text-lg font-extrabold text-orange-600 mt-3 block">
                {{ formatCurrency(totalDue) }}
              </span>
            </label>

            <!-- Đóng theo đợt -->
            <label
              class="p-4 rounded-xl border cursor-pointer flex flex-col justify-between transition"
              :class="paymentMode === 'custom' ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-heading">Đóng theo đợt tự chọn</span>
                <input type="radio" value="custom" v-model="paymentMode" class="text-orange-600 focus:ring-orange-500" />
              </div>
              
              <!-- Input số tiền nếu chọn custom -->
              <div class="mt-2 relative">
                <input
                  v-model="customAmountInput"
                  type="number"
                  :disabled="paymentMode !== 'custom'"
                  class="surface-input border-card w-full pl-8 pr-3 py-1.5 text-xs rounded-lg border focus:outline-none focus:ring-2 focus:ring-orange-500/20 disabled:opacity-40"
                  placeholder="Nhập số tiền đóng..."
                />
                <span class="absolute left-3 top-2 text-[10px] text-muted font-bold">đ</span>
              </div>
            </label>
          </div>
        </div>

        <!-- Chọn phương thức thanh toán -->
        <div class="lg-card-glass p-5 space-y-4">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card">
            2. Chọn phương thức thanh toán
          </h3>

          <div class="space-y-3">
            <!-- VietQR -->
            <label
              class="p-3.5 rounded-xl border cursor-pointer flex items-center justify-between transition"
              :class="paymentMethod === 'vietqr' ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center gap-3">
                <span class="p-2 bg-orange-50 dark:bg-orange-950/20 text-orange-600 rounded-lg">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="w-4 h-4"><rect x="3" y="3" width="18" height="18" rx="2" /><path d="M7 7h3v3H7zM14 7h3v3h-3zM7 14h3v3H7zM14 14h3v3h-3z" /></svg>
                </span>
                <div>
                  <span class="text-xs font-bold text-heading block">Chuyển khoản nhanh VietQR (Khuyên dùng)</span>
                  <span class="text-[10px] text-muted font-normal block mt-0.5">Quét mã QR động chuyển khoản tức thì không cần nhập số tài khoản</span>
                </div>
              </div>
              <input type="radio" value="vietqr" v-model="paymentMethod" class="text-orange-600 focus:ring-orange-500" />
            </label>

            <!-- Thẻ ngân hàng Napas -->
            <label
              class="p-3.5 rounded-xl border cursor-pointer flex items-center justify-between transition"
              :class="paymentMethod === 'napas' ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center gap-3">
                <span class="p-2 bg-blue-50 dark:bg-blue-950/20 text-blue-600 rounded-lg">
                  <CreditCard :size="16" />
                </span>
                <div>
                  <span class="text-xs font-bold text-heading block">Thẻ nội địa Napas / Thẻ quốc tế</span>
                  <span class="text-[10px] text-muted font-normal block mt-0.5">Hỗ trợ các thẻ ATM nội địa Napas, thẻ Visa/MasterCard</span>
                </div>
              </div>
              <input type="radio" value="napas" v-model="paymentMethod" class="text-orange-600 focus:ring-orange-500" />
            </label>

            <!-- Ví điện tử -->
            <label
              class="p-3.5 rounded-xl border cursor-pointer flex items-center justify-between transition"
              :class="paymentMethod === 'ewallet' ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center gap-3">
                <span class="p-2 bg-pink-50 dark:bg-pink-950/20 text-pink-600 rounded-lg">
                  <Smartphone :size="16" />
                </span>
                <div>
                  <span class="text-xs font-bold text-heading block">Ví điện tử Momo / ZaloPay</span>
                  <span class="text-[10px] text-muted font-normal block mt-0.5">Thanh toán qua ví điện tử liên kết trên điện thoại</span>
                </div>
              </div>
              <input type="radio" value="ewallet" v-model="paymentMethod" class="text-orange-600 focus:ring-orange-500" />
            </label>
          </div>
        </div>

      </div>

      <!-- Cột phải: Summary đóng tiền & Button Thanh toán (1/3 width) -->
      <div class="space-y-6">
        <div class="lg-card-glass p-5 space-y-4">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card">
            Tóm tắt giao dịch
          </h3>

          <div class="space-y-2 text-xs">
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Học sinh:</span>
              <span class="text-heading">{{ currentChild?.name }}</span>
            </div>
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Mã số học sinh:</span>
              <span class="text-heading">{{ currentChild?.studentId }}</span>
            </div>
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Lớp hành chính:</span>
              <span class="text-heading">{{ currentChild?.class }}</span>
            </div>
            
            <div class="border-t border-card my-3"></div>
            
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Phương thức:</span>
              <span class="text-heading font-bold">
                {{ paymentMethod === 'vietqr' ? 'Quét VietQR' : paymentMethod === 'napas' ? 'Thẻ ngân hàng' : 'Ví Momo' }}
              </span>
            </div>
            
            <div class="flex justify-between items-baseline font-semibold pt-1">
              <span class="text-muted">Số tiền thanh toán:</span>
              <span class="text-base font-extrabold text-orange-600">{{ formatCurrency(amountToPay) }}</span>
            </div>
          </div>

          <button
            @click="handlePayment"
            :disabled="submitting"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white w-full py-2.5 rounded-xl font-bold text-xs flex items-center justify-center gap-1.5 transition shadow-lg mt-4 disabled:opacity-60"
          >
            <Send :size="13" :class="submitting ? 'animate-spin' : ''" /> {{ submitting ? 'Đang xử lý...' : 'Tiến hành thanh toán' }}
          </button>
        </div>

        <!-- Hướng dẫn bảo mật -->
        <div class="lg-card-glass p-5 space-y-3">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card flex items-center gap-1.5">
            <HelpCircle :size="15" class="text-orange-600" />
            Hướng dẫn an toàn
          </h3>
          <div class="text-[10px] text-body leading-relaxed space-y-2">
            <p>
              1. Vui lòng quét mã QR chuyển khoản chính thức do hệ thống cung cấp, không gửi trực tiếp đến số tài khoản cá nhân.
            </p>
            <p>
              2. Nội dung chuyển khoản cần ghi chính xác theo mã hiển thị trên màn hình để hệ thống tự động gạch nợ thành công trong vòng 3 phút.
            </p>
          </div>
        </div>
      </div>

    </div>

  </div>
</template>

<style scoped>
</style>
