<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  CreditCard,
  ChevronDown,
  ChevronLeft,
  CheckCircle,
  AlertTriangle,
  HelpCircle,
  Copy,
  ExternalLink,
  Loader2,
  QrCode,
  X
} from 'lucide-vue-next'
import FormSkeleton from '@/components/common/skeleton/FormSkeleton.vue'
import { parentApi } from '@/services/parentApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || null)
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref('')
const submitting = ref(false)

const children = ref([])
const tuitionData = ref(null)

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0] || null
})

function isInvoicePaid(status) {
  if (!status) return false
  const s = String(status).toLowerCase()
  return s === 'da_thanh_toan' || s === 'đã nộp' || s === 'da_nop' || s === 'paid'
}

const invoices = computed(() => tuitionData.value?.invoices || [])
const unpaidInvoices = computed(() => invoices.value.filter(inv => !isInvoicePaid(inv.status)))

const selectedInvoiceId = ref(null)

const selectedInvoice = computed(() => {
  if (!selectedInvoiceId.value && unpaidInvoices.value.length > 0) {
    return unpaidInvoices.value[0]
  }
  return unpaidInvoices.value.find(inv => inv.id === selectedInvoiceId.value) || unpaidInvoices.value[0] || null
})

const totalDue = computed(() => tuitionData.value?.totalDue || 0)

const activePayment = ref(null)
let pollTimer = null

const payOsQrImage = computed(() => {
  if (!activePayment.value) return ''
  const p = activePayment.value
  if (p.qrUrl) return p.qrUrl
  const payload = p.qrPayload || p.checkoutUrl || ''
  if (!payload) return ''
  return `https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=${encodeURIComponent(payload)}`
})

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const childrenRes = await parentApi.getChildren()
    children.value = childrenRes?.data || []
    const validChild = children.value.find(child => child.id === activeChildId.value) || children.value[0]
    if (!validChild) {
      tuitionData.value = null
      return
    }
    activeChildId.value = validChild.id
    localStorage.setItem('parent_active_student_id', validChild.id)
    const tuitionRes = await parentApi.getChildTuition(validChild.id)
    tuitionData.value = tuitionRes?.data || null

    if (unpaidInvoices.value.length > 0) {
      const qId = Number(route.query.invoiceId)
      const found = unpaidInvoices.value.find(i => i.id === qId)
      selectedInvoiceId.value = found ? found.id : unpaidInvoices.value[0].id
    }
  } catch (err) {
    error.value = err.message || 'Không thể tải dữ liệu.'
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

function selectChild(id) {
  cancelActivePayment()
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

function formatDate(dateStr) {
  if (!dateStr) return '—'
  const d = new Date(dateStr)
  if (isNaN(d.getTime())) return dateStr
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  return `${day}/${month}/${year}`
}

async function handlePayment() {
  if (!selectedInvoice.value) {
    popupStore.warning('Chưa chọn hóa đơn', 'Không tìm thấy hóa đơn cần thanh toán.')
    return
  }

  submitting.value = true
  try {
    if (!activeChildId.value) {
      throw new Error('Chưa chọn học sinh để thanh toán.')
    }
    const res = await parentApi.createTuitionPayment(activeChildId.value, selectedInvoice.value.id)
    const paymentData = res?.data || res

    if (!paymentData || (!paymentData.maGiaoDich && !paymentData.MaGiaoDich)) {
      throw new Error(paymentData?.message || 'Không thể tạo mã QR thanh toán PayOS.')
    }

    activePayment.value = {
      maGiaoDich: paymentData.maGiaoDich || paymentData.MaGiaoDich,
      maHoaDon: paymentData.maHoaDon || paymentData.MaHoaDon,
      amount: paymentData.amount || paymentData.Amount,
      maThamChieuNoiBo: paymentData.maThamChieuNoiBo || paymentData.MaThamChieuNoiBo,
      noiDungChuyenKhoan: paymentData.noiDungChuyenKhoan || paymentData.NoiDungChuyenKhoan,
      qrUrl: paymentData.qrUrl || paymentData.QrUrl,
      checkoutUrl: paymentData.checkoutUrl || paymentData.CheckoutUrl,
      qrPayload: paymentData.qrPayload || paymentData.QrPayload,
      trangThai: paymentData.trangThai || paymentData.TrangThai || 'cho_thanh_toan'
    }

    popupStore.info('Tạo mã QR thành công', 'Vui lòng quét mã QR bên dưới để hoàn tất thanh toán.')
    startPolling()
  } catch (err) {
    popupStore.error('Thanh toán thất bại', err.message || 'Có lỗi xảy ra khi tạo giao dịch PayOS.')
  } finally {
    submitting.value = false
  }
}

function startPolling() {
  stopPolling()
  pollTimer = setInterval(checkPaymentStatus, 3000)
}

function stopPolling() {
  if (pollTimer) {
    clearInterval(pollTimer)
    pollTimer = null
  }
}

async function checkPaymentStatus() {
  if (!activePayment.value || !activeChildId.value) return
  try {
    const res = await parentApi.getTuitionPayment(activeChildId.value, activePayment.value.maGiaoDich)
    const data = res?.data || res
    const status = data?.trangThai || data?.TrangThai
    if (status) {
      activePayment.value.trangThai = status
      if (status === 'thanh_cong') {
        stopPolling()
        popupStore.success(
          'Thanh toán thành công!',
          `Hệ thống đã nhận được tiền và gạch nợ cho hóa đơn #${activePayment.value.maHoaDon}.`
        )
        setTimeout(() => {
          router.push({ path: '/parent/finance/transactions', query: { studentId: activeChildId.value } })
        }, 1500)
      } else if (['that_bai', 'da_huy', 'het_han'].includes(status)) {
        stopPolling()
        popupStore.error('Giao dịch không thành công', `Trạng thái: ${status === 'het_han' ? 'Hết hạn' : 'Đã hủy'}.`)
      }
    }
  } catch (err) {
    console.error('Lỗi kiểm tra trạng thái thanh toán PayOS:', err)
  }
}

function cancelActivePayment() {
  stopPolling()
  activePayment.value = null
}

function copyContent(text) {
  if (!text) return
  navigator.clipboard.writeText(text)
  popupStore.success('Đã sao chép', `Nội dung: ${text}`)
}

function goBack() {
  stopPolling()
  router.push('/parent/finance/tuition')
}

onUnmounted(() => {
  stopPolling()
})
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
            Thanh toán học phí PayOS
          </h2>
          <p class="text-xs text-body">Cổng thanh toán trực tuyến tự động qua PayOS (VietQR / Chuyển khoản)</p>
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
    <div v-if="loading" class="p-4">
      <FormSkeleton :fields="6" />
    </div>

    <!-- ── ERROR ── -->
    <div v-else-if="error" class="lg-card-glass p-8 text-center">
      <p class="text-sm font-bold text-heading mb-1">Đã xảy ra lỗi</p>
      <p class="text-xs text-muted">{{ error }}</p>
      <button @click="loadData" class="mt-4 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Thử lại
      </button>
    </div>

    <!-- ── KHÔNG CÓ CÔNG NỢ ── -->
    <div v-else-if="totalDue === 0 || unpaidInvoices.length === 0" class="lg-card-glass p-8 text-center flex flex-col items-center justify-center gap-3">
      <CheckCircle :size="48" class="text-emerald-500" />
      <h3 class="text-sm font-bold text-heading">Không có hóa đơn cần thanh toán</h3>
      <p class="text-xs text-body max-w-md">
        Học sinh <strong>{{ currentChild?.name }}</strong> đã hoàn thành 100% học phí kì này. Xin chân thành cảm ơn phụ huynh!
      </p>
      <button @click="router.push('/parent/finance/tuition')" class="mt-2 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Xem chi tiết công nợ
      </button>
    </div>

    <!-- ── HIỂN THỊ MÃ QR PAYOS ĐÃ KHỞI TẠO (POLLING DANG DIỄN RA) ── -->
    <div v-else-if="activePayment" class="lg-card-glass p-6 space-y-6">
      <div class="flex items-center justify-between pb-3 border-b border-card">
        <div class="flex items-center gap-2">
          <QrCode :size="20" class="text-orange-600" />
          <h3 class="text-sm font-bold text-heading uppercase tracking-wide">
            Mã QR Thanh Toán PayOS (Tự động xác nhận)
          </h3>
        </div>
        <button
          @click="cancelActivePayment"
          class="flex items-center gap-1 text-xs font-bold text-muted hover:text-red-500 transition px-2 py-1 rounded-lg border border-card"
        >
          <X :size="14" />
          Hủy / Chọn hóa đơn khác
        </button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 items-center">
        <!-- Cột QR Code -->
        <div class="flex flex-col items-center justify-center p-4 rounded-2xl surface-input border border-card text-center space-y-3">
          <img
            :src="payOsQrImage"
            alt="Mã QR thanh toán VietQR / PayOS"
            class="w-60 h-60 rounded-xl border border-card shadow-md p-2 bg-white object-contain"
          />
          <p class="text-[11px] text-muted font-medium">Quét mã bằng ứng dụng ngân hàng hoặc ví điện tử</p>
          <a
            v-if="activePayment.checkoutUrl"
            :href="activePayment.checkoutUrl"
            target="_blank"
            class="inline-flex items-center gap-1.5 px-4 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-xl text-xs font-bold transition shadow-sm"
          >
            Mở trang thanh toán PayOS <ExternalLink :size="13" />
          </a>
        </div>

        <!-- Cột Thông tin chuyển khoản & Polling Status -->
        <div class="space-y-4">
          <div class="p-4 rounded-xl border border-orange-200 dark:border-orange-950/20 bg-orange-50/20 dark:bg-orange-950/10 space-y-3 text-xs">
            <div class="flex justify-between items-center">
              <span class="text-muted font-medium">Hóa đơn:</span>
              <span class="font-bold text-heading">Hóa đơn #{{ activePayment.maHoaDon }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-muted font-medium">Số tiền thanh toán:</span>
              <span class="text-base font-extrabold text-orange-600">{{ formatCurrency(activePayment.amount) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-muted font-medium">Mã tham chiếu:</span>
              <span class="font-mono text-heading font-bold">{{ activePayment.maThamChieuNoiBo }}</span>
            </div>
            <div class="pt-2 border-t border-card">
              <div class="flex justify-between items-center mb-1.5">
                <span class="text-muted font-bold">Nội dung chuyển khoản (bắt buộc):</span>
                <button
                  @click="copyContent(activePayment.noiDungChuyenKhoan)"
                  class="inline-flex items-center gap-1 text-[11px] font-bold text-orange-600 hover:text-orange-700 hover:underline transition"
                >
                  <Copy :size="12" /> Sao chép
                </button>
              </div>
              <p class="font-mono text-xs font-extrabold text-heading p-2.5 rounded-lg surface-input border border-card text-center select-all tracking-wider">
                {{ activePayment.noiDungChuyenKhoan }}
              </p>
            </div>
          </div>

          <!-- Trạng thái Polling -->
          <div class="p-3.5 rounded-xl border border-card surface-input flex items-center gap-3">
            <Loader2 :size="18" class="animate-spin text-orange-600 flex-shrink-0" />
            <div class="text-xs">
              <p class="font-bold text-heading">Đang chờ thanh toán...</p>
              <p class="text-[11px] text-muted">Hệ thống tự động kiểm tra trạng thái mỗi 3 giây.</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── FORM CHỌN HÓA ĐƠN VÀ TẠO MÃ THANH TOÁN ── -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Cột trái: Chọn hóa đơn & Provider (2/3 width) -->
      <div class="lg:col-span-2 space-y-6">
        
        <!-- Chọn hóa đơn cần thanh toán -->
        <div class="lg-card-glass p-5 space-y-4">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card">
            1. Chọn hóa đơn học phí cần thanh toán
          </h3>

          <div class="space-y-2.5">
            <label
              v-for="inv in unpaidInvoices"
              :key="inv.id"
              class="p-4 rounded-xl border cursor-pointer flex items-center justify-between transition"
              :class="selectedInvoiceId === inv.id ? 'border-orange-500 bg-orange-50/10' : 'border-card hover:bg-slate-50/40'"
            >
              <div class="flex items-center gap-3">
                <input
                  type="radio"
                  :value="inv.id"
                  v-model="selectedInvoiceId"
                  class="text-orange-600 focus:ring-orange-500"
                />
                <div>
                  <span class="text-xs font-bold text-heading block">Hóa đơn #{{ inv.id }}</span>
                  <span class="text-[11px] text-muted block mt-0.5">Hạn đóng: {{ formatDate(inv.dueDate) }}</span>
                </div>
              </div>
              <span class="text-sm font-extrabold text-orange-600">
                {{ formatCurrency(inv.amount) }}
              </span>
            </label>
          </div>
        </div>

        <!-- Phương thức thanh toán (Cố định PayOS) -->
        <div class="lg-card-glass p-5 space-y-4">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card">
            2. Phương thức thanh toán
          </h3>

          <div class="space-y-3">
            <!-- PayOS -->
            <label class="p-3.5 rounded-xl border border-orange-500 bg-orange-50/10 cursor-pointer flex items-center justify-between">
              <div class="flex items-center gap-3">
                <span class="p-2 bg-orange-500 text-white rounded-lg">
                  <CreditCard :size="16" />
                </span>
                <div>
                  <span class="text-xs font-bold text-heading block">Cổng thanh toán tự động PayOS (Khuyên dùng)</span>
                  <span class="text-[10px] text-muted font-normal block mt-0.5">Tạo mã QR thanh toán động, gạch nợ tự động ngay lập tức</span>
                </div>
              </div>
              <input type="radio" checked disabled class="text-orange-600" />
            </label>

            <!-- VietQR / Khác (Tạm ngưng) -->
            <div class="p-3.5 rounded-xl border border-card opacity-50 flex items-center justify-between">
              <div class="flex items-center gap-3">
                <span class="p-2 bg-slate-100 dark:bg-slate-800 text-muted rounded-lg">
                  <QrCode :size="16" />
                </span>
                <div>
                  <span class="text-xs font-bold text-heading block">VietQR Tĩnh / Phương thức khác</span>
                  <span class="text-[10px] text-muted font-normal block mt-0.5">Tạm ngưng — vui lòng dùng cổng PayOS tự động ở trên</span>
                </div>
              </div>
              <span class="text-[10px] font-bold text-muted bg-slate-200 dark:bg-slate-800 px-2 py-0.5 rounded">Đang bảo trì</span>
            </div>
          </div>
        </div>

      </div>

      <!-- Cột phải: Summary & Button Thanh toán (1/3 width) -->
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
              <span class="text-muted">Hóa đơn chọn:</span>
              <span class="text-heading font-bold">#{{ selectedInvoice?.id || '—' }}</span>
            </div>
            <div class="flex justify-between font-semibold">
              <span class="text-muted">Cổng thanh toán:</span>
              <span class="text-orange-600 font-bold">PayOS</span>
            </div>
            
            <div class="border-t border-card my-3"></div>
            
            <div class="flex justify-between items-baseline font-semibold pt-1">
              <span class="text-muted">Số tiền thanh toán:</span>
              <span class="text-base font-extrabold text-orange-600">
                {{ formatCurrency(selectedInvoice?.amount) }}
              </span>
            </div>
          </div>

          <button
            @click="handlePayment"
            :disabled="submitting || !selectedInvoice"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white w-full py-2.5 rounded-xl font-bold text-xs flex items-center justify-center gap-1.5 transition shadow-lg mt-4 disabled:opacity-60"
          >
            <Loader2 v-if="submitting" :size="14" class="animate-spin" />
            <CreditCard v-else :size="14" />
            {{ submitting ? 'Đang tạo mã QR...' : 'Tạo mã QR thanh toán PayOS' }}
          </button>
        </div>

        <!-- Hướng dẫn an toàn -->
        <div class="lg-card-glass p-5 space-y-3">
          <h3 class="text-xs font-bold text-heading uppercase tracking-wide pb-2 border-b border-card flex items-center gap-1.5">
            <HelpCircle :size="15" class="text-orange-600" />
            Hướng dẫn an toàn
          </h3>
          <div class="text-[10px] text-body leading-relaxed space-y-2">
            <p>
              1. Quét mã QR chuyển khoản chính thức do PayOS cung cấp.
            </p>
            <p>
              2. Giữ nguyên <strong>Nội dung chuyển khoản</strong> khi gửi tiền để hệ thống gạch nợ tự động.
            </p>
          </div>
        </div>
      </div>

    </div>

  </div>
</template>

<style scoped>
</style>
