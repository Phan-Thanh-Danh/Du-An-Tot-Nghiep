<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  CreditCard,
  ChevronDown,
  DollarSign,
  QrCode,
  Smartphone,
  Wallet,
  ChevronLeft,
  X,
  Send,
  CheckCircle,
  HelpCircle,
  Copy
} from 'lucide-vue-next'
import { childrenData, getActiveChild, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

// Chế độ nộp: 'all' (Toàn bộ số dư) hoặc 'custom' (Theo đợt)
const paymentMode = ref('all')
const customAmountInput = ref(0)

const amountToPay = computed(() => {
  if (paymentMode.value === 'all') {
    return currentChild.value.balanceTuition
  }
  return Number(customAmountInput.value) || 0
})

// Phương thức thanh toán: 'vietqr', 'napas', 'ewallet'
const paymentMethod = ref('vietqr')

// State modal QR Code
const isQrModalOpen = ref(false)
const transactionCode = ref('')

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  // Reset nợ
  customAmountInput.value = currentChild.value.balanceTuition
  router.replace({ query: { studentId: id } })
}

// Định dạng tiền tệ VND
function formatCurrency(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount)
}

// Nội dung chuyển khoản chuyển mã
const qrTransferDescription = computed(() => {
  const code = currentChild.value.studentId
  const cleanedName = currentChild.value.name
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'D')
    .toUpperCase()
    .replace(/\s+/g, '')
  return `LMS PAY HP ${code} ${cleanedName}`
})

// Mở modal thanh toán
function handlePayment() {
  if (amountToPay.value <= 0) {
    popupStore.warning('Số tiền không hợp lệ', 'Vui lòng chọn số tiền lớn hơn 0 để thanh toán.')
    return
  }
  if (amountToPay.value > currentChild.value.balanceTuition) {
    popupStore.warning('Vượt quá công nợ', `Số tiền cần thanh toán không được vượt quá số dư nợ hiện tại: ${formatCurrency(currentChild.value.balanceTuition)}.`)
    return
  }

  // Tạo mã giao dịch ngẫu nhiên
  transactionCode.value = 'TX' + Math.floor(100000 + Math.random() * 900000)
  isQrModalOpen.value = true
}

// Giả lập thanh toán thành công, cập nhật số dư thực tế
function simulatePaymentSuccess() {
  const paid = amountToPay.value
  const child = childrenData.find(c => c.id === activeChildId.value)
  if (child) {
    // Cập nhật công nợ học sinh
    child.balanceTuition -= paid
    child.paidTuition += paid

    if (child.balanceTuition <= 0) {
      child.balanceTuition = 0
      child.isOverdue = false
      // Cập nhật tất cả các khoản phí thành Đã nộp
      child.feeItems.forEach(item => {
        item.status = 'Đã nộp'
      })
    } else {
      // Cập nhật các khoản phí một cách tương đối
      let remainingToClear = paid
      child.feeItems.forEach(item => {
        if (item.status === 'Chưa nộp' && remainingToClear >= item.amount) {
          item.status = 'Đã nộp'
          remainingToClear -= item.amount
        }
      })
    }

    // Thêm vào Lịch sử giao dịch
    const methodText = paymentMethod.value === 'vietqr' ? 'Chuyển khoản VietQR' :
                       paymentMethod.value === 'napas' ? 'Thẻ ngân hàng Napas' : 'Ví điện tử Momo'
    
    child.transactions.unshift({
      code: transactionCode.value,
      date: new Date().toLocaleDateString('vi-VN'),
      amount: paid,
      method: methodText,
      status: 'Thành công'
    })

    // Thêm vào hóa đơn điện tử
    const newInvoiceId = 'INV-2026-' + Math.floor(100 + Math.random() * 900)
    child.invoices.unshift({
      id: newInvoiceId,
      transactionCode: transactionCode.value,
      date: new Date().toLocaleDateString('vi-VN'),
      amount: paid,
      status: 'Đã phát hành'
    })
  }

  isQrModalOpen.value = false
  popupStore.success(
    'Thanh toán thành công',
    `Hệ thống đã nhận được số tiền ${formatCurrency(paid)} đóng cho học sinh ${currentChild.value.name}. Công nợ đã được khấu trừ.`
  )
  
  // Chuyển hướng về trang công nợ
  router.push({ path: '/parent/finance/tuition', query: { studentId: activeChildId.value } })
}

function copyToClipboard(text) {
  navigator.clipboard.writeText(text)
  popupStore.info('Đã sao chép', 'Đã sao chép nội dung vào bộ nhớ tạm.')
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

    <div v-if="currentChild.balanceTuition === 0" class="lg-card-glass p-8 text-center flex flex-col items-center justify-center gap-3">
      <CheckCircle :size="48" class="text-emerald-500" />
      <h3 class="text-sm font-bold text-heading">Không có công nợ cần thanh toán</h3>
      <p class="text-xs text-body max-w-md">
        Học sinh <strong>{{ currentChild.name }}</strong> đã hoàn thành 100% học phí kì này. Xin chân thành cảm ơn phụ huynh!
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
                {{ formatCurrency(currentChild.balanceTuition) }}
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
                  <QrCode :size="16" />
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
              <span class="text-heading">{{ currentChild.name }}</span>
            </div>
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Mã số học sinh:</span>
              <span class="text-heading">{{ currentChild.studentId }}</span>
            </div>
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Lớp hành chính:</span>
              <span class="text-heading">{{ currentChild.class }}</span>
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
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white w-full py-2.5 rounded-xl font-bold text-xs flex items-center justify-center gap-1.5 transition shadow-lg mt-4"
          >
            <Send :size="13" /> Tiến hành thanh toán
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

    <!-- ── MODAL QR CODE THANH TOÁN (VIETQR DYNAMIC) ── -->
    <div v-if="isQrModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Overlay -->
      <div @click="isQrModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm" />

      <!-- Modal Content -->
      <div class="lg-modal w-full max-w-md relative z-10 flex flex-col rounded-2xl shadow-xl overflow-hidden">
        
        <!-- Header -->
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h2 class="text-sm font-bold text-heading flex items-center gap-2">
            <QrCode :size="16" class="text-orange-600" />
            Quét mã VietQR chuyển khoản
          </h2>
          <button @click="isQrModalOpen = false" class="text-muted hover:text-orange-600">
            <X :size="16" />
          </button>
        </div>

        <!-- QR Body -->
        <div class="p-4 flex flex-col items-center text-center space-y-4">
          
          <!-- Dynamic QR code wrapper -->
          <div class="p-3 bg-white rounded-2xl border border-slate-200 shadow-sm flex flex-col items-center">
            <!-- Header VietQR logo simulation -->
            <div class="flex items-center justify-between w-full max-w-[180px] pb-2 border-b border-slate-100 mb-2">
              <span class="text-[9px] font-extrabold text-blue-700">Viet<span class="text-orange-600">QR</span></span>
              <span class="text-[7px] font-bold text-slate-400">TPBank</span>
            </div>

            <!-- Fake QR Code SVG representation to look premium and authentic -->
            <svg viewBox="0 0 100 100" class="h-44 w-44">
              <!-- Grid corners -->
              <rect x="5" y="5" width="20" height="20" fill="none" stroke="#000000" stroke-width="4" />
              <rect x="9" y="9" width="12" height="12" fill="#000000" />
              
              <rect x="75" y="5" width="20" height="20" fill="none" stroke="#000000" stroke-width="4" />
              <rect x="79" y="9" width="12" height="12" fill="#000000" />

              <rect x="5" y="75" width="20" height="20" fill="none" stroke="#000000" stroke-width="4" />
              <rect x="9" y="79" width="12" height="12" fill="#000000" />

              <!-- Scattered mini dots in center -->
              <rect x="40" y="10" width="8" height="8" fill="#ea580c" />
              <rect x="55" y="20" width="10" height="4" fill="#000000" />
              <rect x="40" y="35" width="12" height="6" fill="#000000" />
              <rect x="65" y="35" width="6" height="15" fill="#ea580c" />
              <rect x="15" y="45" width="20" height="4" fill="#000000" />
              <rect x="45" y="50" width="15" height="15" fill="#000000" />
              <rect x="70" y="60" width="10" height="10" fill="#000000" />
              <rect x="30" y="70" width="12" height="8" fill="#ea580c" />
              <rect x="80" y="75" width="5" height="5" fill="#000000" />
              
              <!-- Small center logo -->
              <circle cx="50" cy="50" r="10" fill="#ffffff" />
              <text x="50" y="53" text-anchor="middle" font-size="7" font-weight="bold" fill="#ea580c">LMS</text>
            </svg>
            
            <p class="text-[9px] text-slate-500 font-medium mt-2">Mã QR tạo động chỉ dùng 1 lần</p>
          </div>

          <!-- Transfer details table -->
          <div class="w-full text-xs space-y-2.5 text-left bg-slate-50 dark:bg-slate-900/40 p-4 rounded-xl border border-card">
            
            <div class="flex items-center justify-between">
              <span class="text-muted">Ngân hàng thụ hưởng:</span>
              <span class="font-bold text-heading">TPBank (Ngân hàng Tiên Phong)</span>
            </div>

            <div class="flex items-center justify-between">
              <span class="text-muted">Số tài khoản:</span>
              <div class="flex items-center gap-1 font-bold text-heading">
                <span>190204590201</span>
                <button @click="copyToClipboard('190204590201')" class="text-orange-600 hover:text-orange-700">
                  <Copy :size="12" />
                </button>
              </div>
            </div>

            <div class="flex items-center justify-between">
              <span class="text-muted">Chủ tài khoản:</span>
              <span class="font-bold text-heading">TRUONG DAI HOC LMS ACADEMIC</span>
            </div>

            <div class="flex items-center justify-between">
              <span class="text-muted">Số tiền chuyển khoản:</span>
              <span class="font-extrabold text-orange-600 text-sm">{{ formatCurrency(amountToPay) }}</span>
            </div>

            <div class="flex items-center justify-between border-t border-card pt-2 mt-2">
              <span class="text-muted font-bold">Nội dung chuyển khoản:</span>
              <div class="flex items-center gap-1 font-extrabold text-heading">
                <span>{{ qrTransferDescription }}</span>
                <button @click="copyToClipboard(qrTransferDescription)" class="text-orange-600 hover:text-orange-700">
                  <Copy :size="12" />
                </button>
              </div>
            </div>

          </div>

          <!-- Simulation Button -->
          <div class="w-full pt-3 border-t border-card flex flex-col gap-2">
            <button
              @click="simulatePaymentSuccess"
              class="lg-button-primary bg-emerald-600 hover:bg-emerald-700 text-white w-full py-2 rounded-xl text-xs font-bold flex items-center justify-center gap-1 transition"
            >
              <CheckCircle :size="13" /> Giả lập chuyển khoản thành công
            </button>
            <p class="text-[9px] text-muted">
              * Nhấn nút giả lập phía trên để hoàn tất giao dịch tự động của phụ huynh.
            </p>
          </div>

        </div>

      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
