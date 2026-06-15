<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import {
  createTuitionPayment,
  getStudentTuitionInvoices,
  getStudentTuitionTransactions,
  getTuitionPaymentStatus,
} from '@/services/tuitionService'
import {
  CreditCard, Wallet, Receipt, DollarSign,
  AlertCircle, CheckCircle2, XCircle, Clock,
  Sparkles, Download, ArrowRight, ShieldCheck,
  Building2, RefreshCw
} from 'lucide-vue-next'

const popupStore = usePopupStore()

const statusConfig = {
  Unpaid: { label: 'Chưa thanh toán', cls: 'badge-red', icon: AlertCircle },
  Partial: { label: 'Thanh toán một phần', cls: 'badge-amber', icon: Clock },
  Paid: { label: 'Đã thanh toán', cls: 'badge-green', icon: CheckCircle2 },
  Overdue: { label: 'Quá hạn', cls: 'badge-slate', icon: XCircle },
  Cancelled: { label: 'Đã hủy', cls: 'badge-slate', icon: XCircle },
  Processing: { label: 'Đang xử lý', cls: 'badge-blue', icon: Clock },
  Failed: { label: 'Thất bại', cls: 'badge-red', icon: XCircle },
  Success: { label: 'Thành công', cls: 'badge-green', icon: CheckCircle2 }
}

const backendStatusMap = {
  chua_thanh_toan: 'Unpaid',
  thanh_toan_mot_phan: 'Partial',
  da_thanh_toan: 'Paid',
  qua_han: 'Overdue',
  da_huy: 'Cancelled',
  cho_thanh_toan: 'Processing',
  dang_xu_ly: 'Processing',
  cho_xu_ly_thu_cong: 'Processing',
  thanh_cong: 'Success',
  that_bai: 'Failed',
  sai_so_tien: 'Failed',
  het_han: 'Overdue',
}

const rawInvoices = ref([])
const rawTransactions = ref([])
const isLoadingData = ref(false)
const loadError = ref('')
const activeTab = ref('invoices') // 'invoices' or 'history'
const modalOpen = ref(false)
useBodyScrollLock(modalOpen)
const selectedInvoice = ref(null)
const isProcessing = ref(false)
const paymentResult = ref(null)
const paymentConfirmed = ref(false)
const paymentPollTimer = ref(null)
let paymentPollAttempts = 0

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(Number(val || 0))
const formatNumber = (val) => new Intl.NumberFormat('vi-VN').format(Number(val || 0))
const formatDate = (date) => {
  const parsed = parseDate(date)
  return parsed ? new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(parsed) : 'Chưa cập nhật'
}
const formatDateTime = (date) => {
  const parsed = parseDate(date)
  return parsed ? new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(parsed) : 'Chưa cập nhật'
}

const invoices = computed(() => rawInvoices.value.map(mapInvoice))
const transactions = computed(() => rawTransactions.value.map(mapTransaction))
const paymentQrImageUrl = computed(() => {
  const provider = String(read(paymentResult.value, 'provider', 'Provider') || '').toLowerCase()
  if (provider === 'payos' && paymentResult.value?.qrPayload) {
    return `https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(paymentResult.value.qrPayload)}`
  }

  if (paymentResult.value?.qrUrl) return paymentResult.value.qrUrl
  if (paymentResult.value?.qrPayload) {
    return `https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(paymentResult.value.qrPayload)}`
  }

  return ''
})
const metrics = computed(() => {
  const totals = rawInvoices.value.reduce((acc, invoice) => {
    acc.soTienPhaiDong += toNumber(read(invoice, 'soTienPhaiDong', 'SoTienPhaiDong'))
    acc.giamTru += toNumber(read(invoice, 'giamTru', 'GiamTru'))
    acc.daThanhToan += toNumber(read(invoice, 'daThanhToan', 'DaThanhToan'))
    acc.conPhaiDong += toNumber(read(invoice, 'conPhaiDong', 'ConPhaiDong'))
    return acc
  }, { soTienPhaiDong: 0, giamTru: 0, daThanhToan: 0, conPhaiDong: 0 })

  return [
    { label: 'Tổng công nợ', value: formatNumber(totals.soTienPhaiDong), unit: 'đ', icon: Receipt, tone: 'slate', hint: 'Từ hóa đơn học phí' },
    { label: 'Giảm trừ', value: formatNumber(totals.giamTru), unit: 'đ', icon: Sparkles, tone: 'violet', hint: 'Học bổng/miễn giảm' },
    { label: 'Đã thanh toán', value: formatNumber(totals.daThanhToan), unit: 'đ', icon: CheckCircle2, tone: 'green', hint: 'Đã được xác nhận' },
    { label: 'Dư nợ còn lại', value: formatNumber(totals.conPhaiDong), unit: 'đ', icon: Wallet, tone: 'amber', hint: 'Cần thanh toán' },
  ]
})

onMounted(() => {
  loadTuitionData()
})

onBeforeUnmount(() => {
  stopPaymentPolling()
})

const openPaymentModal = (invoice) => {
  if (invoice.status === 'Processing') return // Prevent double payment
  if (invoice.conPhaiDong <= 0) {
    popupStore.info('Hóa đơn đã đủ', 'Hóa đơn này không còn số tiền cần thanh toán.')
    return
  }

  selectedInvoice.value = invoice
  paymentResult.value = null
  paymentConfirmed.value = false
  stopPaymentPolling()
  modalOpen.value = true
  createPayOsQrPayment()
}

const closePaymentModal = () => {
  if (isProcessing.value) return
  resetPaymentModal()
}

const resetPaymentModal = () => {
  stopPaymentPolling()
  modalOpen.value = false
  selectedInvoice.value = null
  paymentResult.value = null
  paymentConfirmed.value = false
}

const createPayOsQrPayment = async () => {
  if (!selectedInvoice.value) return

  isProcessing.value = true

  try {
    const result = await createTuitionPayment(selectedInvoice.value.maHoaDon, 'payos')
    paymentResult.value = result

    if (!result?.qrUrl && !result?.qrPayload && !result?.checkoutUrl) {
      throw new Error('PayOS không trả về dữ liệu thanh toán.')
    }

    popupStore.success('Đã tạo mã QR PayOS', 'Vui lòng chuyển khoản đúng số tiền và nội dung.')
    startPaymentPolling(read(result, 'maGiaoDich', 'MaGiaoDich'))
    await loadTransactions()
  } catch (error) {
    popupStore.error('Không tạo được thanh toán', error?.message || 'Vui lòng thử lại sau.')
  } finally {
    isProcessing.value = false
  }
}

const downloadPDF = (id) => {
  popupStore.info('Đang tải', `Đang tải hóa đơn ${id}...`)
}

async function loadTuitionData() {
  isLoadingData.value = true
  loadError.value = ''

  try {
    await Promise.all([loadInvoices(), loadTransactions()])
  } catch (error) {
    loadError.value = error?.message || 'Không thể tải dữ liệu học phí.'
    popupStore.error('Không tải được học phí', loadError.value)
  } finally {
    isLoadingData.value = false
  }
}

async function loadInvoices() {
  rawInvoices.value = await getStudentTuitionInvoices()
}

async function loadTransactions() {
  rawTransactions.value = await getStudentTuitionTransactions()
}

function startPaymentPolling(transactionId) {
  stopPaymentPolling()
  paymentPollAttempts = 0

  if (!transactionId) return

  schedulePaymentPoll(transactionId, 2500)
}

function schedulePaymentPoll(transactionId, delay = 3000) {
  paymentPollTimer.value = window.setTimeout(() => {
    pollPaymentStatus(transactionId)
  }, delay)
}

function stopPaymentPolling() {
  if (paymentPollTimer.value) {
    window.clearTimeout(paymentPollTimer.value)
    paymentPollTimer.value = null
  }
}

async function pollPaymentStatus(transactionId) {
  if (!modalOpen.value || paymentConfirmed.value) return

  paymentPollAttempts += 1

  try {
    const status = await getTuitionPaymentStatus(transactionId)
    paymentResult.value = {
      ...(paymentResult.value || {}),
      ...(status || {}),
    }

    const mappedStatus = mapStatus(read(status, 'trangThai', 'TrangThai'))

    if (mappedStatus === 'Success') {
      await handleConfirmedPayment()
      return
    }

    if (['Failed', 'Overdue', 'Cancelled'].includes(mappedStatus)) {
      stopPaymentPolling()
      await loadTuitionData()
      popupStore.error('Thanh toán chưa thành công', 'Giao dịch không hoàn tất. Vui lòng thử lại hoặc liên hệ kế toán.')
      return
    }
  } catch {
    // Giữ modal mở và thử lại; PayOS/webhook local có thể trễ vài giây.
  }

  if (paymentPollAttempts < 60) {
    schedulePaymentPoll(transactionId)
  }
}

async function handleConfirmedPayment() {
  stopPaymentPolling()
  paymentConfirmed.value = true
  popupStore.success('Đã nhận thanh toán', 'Hóa đơn học phí đã được backend xác nhận.')
  await loadTuitionData()

  window.setTimeout(() => {
    if (paymentConfirmed.value) {
      resetPaymentModal()
    }
  }, 1500)
}

function mapInvoice(invoice) {
  const soTien = toNumber(read(invoice, 'soTien', 'SoTien'))
  const giamTru = toNumber(read(invoice, 'giamTru', 'GiamTru'))
  const daThanhToan = toNumber(read(invoice, 'daThanhToan', 'DaThanhToan'))
  const soTienPhaiDong = toNumber(read(invoice, 'soTienPhaiDong', 'SoTienPhaiDong'))
  const conPhaiDong = toNumber(read(invoice, 'conPhaiDong', 'ConPhaiDong'))
  const rawStatus = read(invoice, 'trangThai', 'TrangThai')

  const items = [
    { name: 'Học phí học kỳ', amount: soTien },
  ]

  if (giamTru > 0) items.push({ name: 'Giảm trừ', amount: -giamTru })
  if (daThanhToan > 0) items.push({ name: 'Đã thanh toán', amount: -daThanhToan })
  items.push({ name: 'Còn phải đóng', amount: conPhaiDong })

  return {
    maHoaDon: read(invoice, 'maHoaDon', 'MaHoaDon'),
    id: read(invoice, 'maHoaDonCode', 'MaHoaDonCode'),
    semester: read(invoice, 'hocKy', 'HocKy'),
    total: conPhaiDong,
    soTienPhaiDong,
    conPhaiDong,
    dueDate: read(invoice, 'hanThanhToan', 'HanThanhToan'),
    status: mapStatus(rawStatus),
    rawStatus,
    items,
  }
}

function mapTransaction(transaction) {
  const provider = String(read(transaction, 'nhaCungCapThanhToan', 'NhaCungCapThanhToan') || '').toLowerCase()

  return {
    id: read(transaction, 'maGiaoDich', 'MaGiaoDich'),
    txId: read(transaction, 'maThamChieuNoiBo', 'MaThamChieuNoiBo') || `GD-${read(transaction, 'maGiaoDich', 'MaGiaoDich')}`,
    date: read(transaction, 'ngayTao', 'NgayTao'),
    amount: toNumber(read(transaction, 'soTien', 'SoTien')),
    method: providerLabel(provider),
    methodIcon: provider === 'payos' ? CreditCard : Building2,
    status: mapStatus(read(transaction, 'trangThai', 'TrangThai')),
  }
}

function mapStatus(status) {
  return backendStatusMap[String(status || '').trim().toLowerCase()] || 'Processing'
}

function getStatusConfig(status) {
  return statusConfig[status] || statusConfig.Processing
}

function providerLabel(provider) {
  if (provider === 'payos') return 'PayOS'
  if (provider === 'vietqr') return 'VietQR'
  return provider || 'Khác'
}

function read(source, camelKey, pascalKey) {
  return source?.[camelKey] ?? source?.[pascalKey]
}

function toNumber(value) {
  const number = Number(value || 0)
  return Number.isFinite(number) ? number : 0
}

function parseDate(value) {
  if (!value) return null
  const date = value instanceof Date
    ? value
    : new Date(String(value).length === 10 ? `${value}T00:00:00` : value)

  return Number.isNaN(date.getTime()) ? null : date
}
</script>

<template>
  <div class="tuition-page">
    <!-- Header -->
    <div class="page-header">
      <div>
        <div class="eyebrow"><CreditCard :size="15"/>Tài chính sinh viên</div>
        <h1 class="page-title">Học phí & Thanh toán</h1>
        <p class="page-sub">Quản lý hóa đơn, công nợ và thực hiện thanh toán trực tuyến an toàn.</p>
      </div>
      <div class="header-actions">
        <button class="btn-outline" :disabled="isLoadingData" @click="loadTuitionData">
          <RefreshCw :size="15" :class="isLoadingData ? 'animate-spin' : ''" />
          Tải lại
        </button>
        <router-link to="/student/requests" class="btn-outline">
          Yêu cầu hoàn phí / Bảo lưu
        </router-link>
      </div>
    </div>

    <!-- AI Banner -->
    <div class="ai-banner banner-violet">
      <div class="banner-icon"><Sparkles :size="24" /></div>
      <div class="banner-content">
        <h3>Công nợ học phí theo học kỳ</h3>
        <p>Hóa đơn, giảm trừ và số tiền còn phải đóng được lấy trực tiếp từ hệ thống tài chính. Trạng thái thanh toán chỉ được cập nhật sau khi backend xác nhận giao dịch.</p>
      </div>
    </div>

    <!-- Metrics -->
    <div class="metrics-grid">
      <div v-for="m in metrics" :key="m.label" class="metric-card" :class="`metric-${m.tone}`">
        <div class="metric-icon-wrap"><component :is="m.icon" :size="20"/></div>
        <div class="metric-body">
          <div class="metric-val">{{ m.value }}<span class="metric-unit">{{ m.unit }}</span></div>
          <div class="metric-lbl">{{ m.label }}</div>
          <div class="metric-hint">{{ m.hint }}</div>
        </div>
      </div>
    </div>

    <!-- Tab Navigation -->
    <div class="tab-nav">
      <button :class="['tab-btn', activeTab === 'invoices' && 'active']" @click="activeTab = 'invoices'">
        <Receipt :size="16"/> Hóa đơn cần đóng
      </button>
      <button :class="['tab-btn', activeTab === 'history' && 'active']" @click="activeTab = 'history'">
        <Clock :size="16"/> Lịch sử giao dịch
      </button>
    </div>

    <!-- Tab Content: Invoices -->
    <div v-if="activeTab === 'invoices'" class="content-section">
      <div v-if="isLoadingData" class="state-box">
        <Clock :size="18" class="animate-spin" />
        <span>Đang tải hóa đơn học phí...</span>
      </div>
      <div v-else-if="loadError" class="state-box state-error">
        <AlertCircle :size="18" />
        <span>{{ loadError }}</span>
      </div>
      <div v-else-if="invoices.length === 0" class="state-box">
        <Receipt :size="18" />
        <span>Chưa có hóa đơn học phí.</span>
      </div>

      <template v-else>
      <div v-for="inv in invoices" :key="inv.id" class="invoice-card" :class="`card-${inv.status}`">
        <div class="invoice-header">
          <div>
            <div class="flex items-center gap-2 mb-1">
              <span class="invoice-id">{{ inv.id }}</span>
              <span class="status-badge" :class="getStatusConfig(inv.status).cls">
                <component :is="getStatusConfig(inv.status).icon" :size="12" />
                {{ getStatusConfig(inv.status).label }}
              </span>
            </div>
            <h3 class="invoice-semester">{{ inv.semester }}</h3>
          </div>
          <div class="invoice-amount-block">
            <span class="amount-lbl">Còn phải thanh toán:</span>
            <span class="amount-val">{{ formatCurrency(inv.total) }}</span>
          </div>
        </div>

        <div class="invoice-body">
          <table class="items-table">
            <tbody>
              <tr v-for="(item, idx) in inv.items" :key="idx">
                <td>{{ item.name }}</td>
                <td class="text-right" :class="item.amount < 0 ? 'amount-discount font-semibold' : ''">
                  {{ formatCurrency(item.amount) }}
                </td>
              </tr>
            </tbody>
          </table>
          <div class="due-date-row">
            <Clock :size="14" />
            Hạn thanh toán: <strong>{{ formatDate(inv.dueDate) }}</strong>
            <span v-if="inv.status === 'Unpaid'" class="due-warning ml-2">(Sắp đến hạn)</span>
          </div>
        </div>

        <div class="invoice-footer">
          <button v-if="inv.status === 'Paid'" class="btn-secondary" @click="downloadPDF(inv.id)">
            <Download :size="15"/> Tải PDF Hóa đơn
          </button>
          
          <div class="flex-1"></div>
          
          <button v-if="['Unpaid', 'Partial', 'Failed'].includes(inv.status)" class="btn-primary" @click="openPaymentModal(inv)">
            <DollarSign :size="15"/> Thanh toán ngay
          </button>
          <button v-else-if="inv.status === 'Processing'" class="btn-secondary" disabled>
            <Clock :size="15" class="animate-spin" /> Đang xử lý giao dịch...
          </button>
        </div>
      </div>
      </template>
    </div>

    <!-- Tab Content: Transaction History -->
    <div v-else class="content-section">
      <div v-if="isLoadingData" class="state-box">
        <Clock :size="18" class="animate-spin" />
        <span>Đang tải lịch sử giao dịch...</span>
      </div>
      <div v-else-if="transactions.length === 0" class="state-box">
        <Clock :size="18" />
        <span>Chưa có giao dịch học phí.</span>
      </div>

      <div v-else class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Mã giao dịch</th>
              <th>Thời gian</th>
              <th>Phương thức</th>
              <th>Số tiền</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="tx in transactions" :key="tx.id">
              <td class="font-semibold transaction-id">{{ tx.txId }}</td>
              <td>{{ formatDateTime(tx.date) }}</td>
              <td>
                <div class="flex items-center gap-1.5">
                  <component :is="tx.methodIcon" :size="14" class="method-icon" />
                  {{ tx.method }}
                </div>
              </td>
              <td class="font-semibold">{{ formatCurrency(tx.amount) }}</td>
              <td>
                <span class="status-badge" :class="getStatusConfig(tx.status).cls">
                  <component :is="getStatusConfig(tx.status).icon" :size="12" />
                  {{ getStatusConfig(tx.status).label }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Payment Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalOpen" class="modal-overlay" @click.self="closePaymentModal">
          <div class="modal-content">
            <div class="modal-header">
              <h3>Thanh toán hóa đơn</h3>
              <button class="close-btn-sm" @click="closePaymentModal" :disabled="isProcessing"><XCircle :size="20"/></button>
            </div>
            
            <div class="modal-body">
              <div class="summary-box">
                <div class="flex justify-between text-sm mb-1">
                  <span class="modal-muted">Mã hóa đơn:</span>
                  <span class="font-semibold">{{ selectedInvoice?.id }}</span>
                </div>
                <div class="flex justify-between text-sm mb-3">
                  <span class="modal-muted">Nội dung:</span>
                  <span>{{ selectedInvoice?.semester }}</span>
                </div>
                <div class="modal-total-row flex justify-between items-center pt-3">
                  <span class="font-semibold">Cần thanh toán:</span>
                  <span class="modal-total">{{ selectedInvoice ? formatCurrency(selectedInvoice.total) : '0' }}</span>
                </div>
              </div>

              <div class="payos-flow-note">
                <CreditCard :size="18" />
                <div>
                  <strong>Thanh toán tự động qua PayOS</strong>
                  <p>PayOS tạo mã VietQR cho hóa đơn này. LMS sẽ tự đồng bộ trạng thái khi ngân hàng xác nhận giao dịch.</p>
                </div>
              </div>

              <div v-if="isProcessing && !paymentResult" class="payment-loading">
                <Clock :size="18" class="animate-spin" />
                <span>Đang tạo mã QR thanh toán...</span>
              </div>

              <div v-if="paymentQrImageUrl" class="qr-result" :class="{ confirmed: paymentConfirmed }">
                <div class="qr-image-wrap">
                  <img :src="paymentQrImageUrl" alt="QR thanh toán học phí qua PayOS" class="qr-image" />
                  <div v-if="paymentConfirmed" class="qr-confirmed-overlay">
                    <CheckCircle2 :size="34" />
                  </div>
                </div>
                <div class="qr-detail">
                  <span>Số tiền</span>
                  <strong>{{ formatCurrency(paymentResult.amount) }}</strong>
                </div>
                <div class="qr-detail">
                  <span>Nội dung chuyển khoản</span>
                  <strong>{{ paymentResult.noiDungChuyenKhoan }}</strong>
                </div>
                <p v-if="paymentConfirmed" class="qr-success-note">Thanh toán đã được xác nhận. Modal sẽ tự đóng sau vài giây.</p>
                <p v-else>Sau khi chuyển khoản, LMS sẽ kiểm tra PayOS và cập nhật hóa đơn khi giao dịch thành công.</p>
              </div>

              <div v-else-if="paymentResult?.checkoutUrl" class="checkout-fallback">
                <AlertCircle :size="18" />
                <span>PayOS chưa trả dữ liệu QR trực tiếp. Bạn có thể mở trang thanh toán PayOS để tiếp tục.</span>
                <a class="btn-primary" :href="paymentResult.checkoutUrl" target="_blank" rel="noopener">
                  Mở PayOS <ArrowRight :size="15" />
                </a>
              </div>

              <div class="security-badge">
                <ShieldCheck :size="16" />
                <span>Giao dịch được xác thực bởi PayOS và chữ ký số HMAC.</span>
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn-secondary" @click="closePaymentModal" :disabled="isProcessing">
                Đóng
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.tuition-page {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  width: 100%;
  color: var(--text-heading);
}

.page-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; flex-wrap: wrap; }
.header-actions { display: flex; align-items: center; gap: .5rem; flex-wrap: wrap; justify-content: flex-end; }
.eyebrow { display: inline-flex; align-items: center; gap: .35rem; width: fit-content; border: 1px solid var(--border-card); border-radius: 999px; background: var(--surface-input); color: var(--text-link); padding: .25rem .6rem; font-size: .7rem; font-weight: 850; text-transform: uppercase; }
.page-title { color: var(--text-heading); font-size: 1.35rem; font-weight: 900; margin: .45rem 0 .2rem; line-height: 1.15; }
.page-sub { font-size: .82rem; color: var(--text-body); margin: 0; }

/* AI Banner */
.ai-banner { display: flex; align-items: flex-start; gap: .75rem; padding: .85rem; border-radius: 16px; }
.banner-violet { background: var(--accent-violet-soft); border: 1px solid color-mix(in srgb, var(--accent-violet) 18%, transparent); color: var(--accent-violet); }
.banner-content h3 { font-size: .92rem; font-weight: 850; margin: 0 0 .2rem; }
.banner-content p { font-size: .8rem; margin: 0; line-height: 1.45; }

/* Metrics */
.metrics-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(13rem, 1fr)); gap: .75rem; }
.metric-card { display: flex; align-items: center; gap: .75rem; border: 1px solid var(--border-card); border-radius: 16px; background: var(--surface-card); padding: .8rem; box-shadow: var(--lg-shadow-sm); }
.metric-icon-wrap { width: 2.25rem; height: 2.25rem; border-radius: 12px; display: flex; align-items: center; justify-content: center; background: var(--surface-input); color: var(--text-link); }
.metric-slate { box-shadow: inset 3px 0 0 var(--text-placeholder), var(--lg-shadow-sm); }
.metric-violet { box-shadow: inset 3px 0 0 var(--accent-violet), var(--lg-shadow-sm); }
.metric-green { box-shadow: inset 3px 0 0 var(--color-success-text), var(--lg-shadow-sm); }
.metric-amber { box-shadow: inset 3px 0 0 var(--color-warning-text), var(--lg-shadow-sm); }
.metric-val { color: var(--text-heading); font-size: 1.05rem; font-weight: 900; line-height: 1; }
.metric-unit { font-size: .75rem; font-weight: 700; color: var(--text-placeholder); margin-left: 4px; }
.metric-lbl { font-size: .72rem; font-weight: 800; color: var(--text-label); margin-top: .25rem; }
.metric-hint { font-size: .68rem; color: var(--text-placeholder); margin-top: .1rem; }

/* Tabs */
.tab-nav { display: flex; gap: .35rem; border-bottom: 1px solid var(--border-card); padding-bottom: .25rem; }
.tab-btn { display: inline-flex; align-items: center; gap: .45rem; min-height: 2.25rem; padding: 0 .85rem; background: transparent; border: none; font-size: .82rem; font-weight: 850; color: var(--text-placeholder); cursor: pointer; border-radius: 10px; transition: all .2s; }
.tab-btn:hover { color: var(--text-heading); background: var(--surface-input); }
.tab-btn.active { color: var(--text-link); background: var(--accent-primary-soft); }

.content-section { display: flex; flex-direction: column; gap: .85rem; }
.state-box { min-height: 8rem; display: flex; align-items: center; justify-content: center; gap: .5rem; border: 1px dashed var(--border-card); border-radius: 14px; background: var(--surface-input); color: var(--text-label); font-size: .85rem; font-weight: 750; }
.state-error { color: var(--color-danger-text); background: var(--color-danger-bg); }

/* Invoices */
.invoice-card { background: var(--surface-card); border: 1px solid var(--border-card); border-radius: 18px; overflow: hidden; box-shadow: var(--lg-shadow-sm); transition: transform .2s, border-color .2s; }
.invoice-card:hover { transform: translateY(-2px); }
.card-Unpaid { border-color: color-mix(in srgb, var(--color-danger-text) 24%, var(--border-card)); }
.card-Paid { border-color: color-mix(in srgb, var(--color-success-text) 24%, var(--border-card)); }
.card-Processing { border-color: color-mix(in srgb, var(--text-link) 24%, var(--border-card)); }
.card-Cancelled { opacity: .78; }

.invoice-header { display: flex; justify-content: space-between; align-items: flex-start; padding: .9rem; border-bottom: 1px solid var(--border-card); flex-wrap: wrap; gap: 1rem; }
.invoice-id { font-size: .75rem; font-weight: 850; color: var(--text-label); background: var(--surface-input); padding: .2rem .5rem; border-radius: 6px; }
.invoice-semester { color: var(--text-heading); font-size: 1rem; font-weight: 900; margin: .45rem 0 0; }
.invoice-amount-block { text-align: right; }
.amount-lbl { display: block; font-size: .72rem; font-weight: 750; color: var(--text-placeholder); }
.amount-val { display: block; font-size: 1.25rem; font-weight: 900; color: var(--text-link); }

.invoice-body { padding: .9rem; }
.items-table { width: 100%; font-size: .875rem; border-collapse: collapse; margin-bottom: 1rem; }
.items-table td { padding: .45rem 0; border-bottom: 1px dashed var(--border-card); color: var(--text-label); }
.amount-discount { color: var(--color-success-text); }
.due-date-row { display: flex; align-items: center; gap: .4rem; font-size: .8125rem; color: var(--text-label); background: var(--surface-input); padding: .5rem .75rem; border-radius: 8px; border: 1px solid var(--border-card); }
.due-warning { color: var(--color-danger-text); }

.invoice-footer { padding: .75rem .9rem; border-top: 1px solid var(--border-card); display: flex; gap: .75rem; background: var(--surface-input); }

/* Table */
.table-container { background: var(--surface-card); border: 1px solid var(--border-card); border-radius: 18px; overflow-x: auto; box-shadow: var(--lg-shadow-sm); }
.data-table { width: 100%; border-collapse: collapse; font-size: .875rem; }
.data-table th { text-align: left; padding: .75rem; background: var(--surface-input); font-weight: 850; color: var(--text-label); border-bottom: 1px solid var(--border-card); }
.data-table td { padding: .75rem; border-bottom: 1px solid var(--border-card); color: var(--text-body); }
.transaction-id { color: var(--text-link); }
.method-icon { color: var(--text-placeholder); }

/* Status Badges */
.status-badge { display: inline-flex; align-items: center; gap: .3rem; font-size: .65rem; font-weight: 700; padding: .15rem .5rem; border-radius: 99px; text-transform: uppercase; }
.badge-green { background: var(--color-success-bg); color: var(--color-success-text); }
.badge-red { background: var(--color-danger-bg); color: var(--color-danger-text); }
.badge-amber { background: var(--color-warning-bg); color: var(--color-warning-text); }
.badge-blue { background: var(--color-info-bg); color: var(--color-info-text); }
.badge-slate { background: var(--surface-input); color: var(--text-placeholder); }

/* Buttons */
.btn-primary, .btn-secondary, .btn-outline { display: inline-flex; align-items: center; justify-content: center; gap: .4rem; padding: .6rem 1.2rem; border-radius: 10px; font-size: .8125rem; font-weight: 700; cursor: pointer; border: none; transition: all .15s; outline: none; text-decoration: none; }
.btn-primary { background: var(--accent-primary); color: var(--text-inverse); box-shadow: var(--lg-shadow-sm); }
.btn-primary:hover:not(:disabled) { transform: translateY(-1px); }
.btn-primary:disabled { opacity: .6; cursor: not-allowed; }
.btn-secondary { background: var(--surface-input); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-secondary:hover:not(:disabled) { border-color: var(--border-input-focus); color: var(--text-link); }
.btn-secondary:disabled { opacity: .6; cursor: not-allowed; }
.btn-outline { background: var(--surface-input); color: var(--text-label); border: 1px solid var(--border-input); }
.btn-outline:hover { color: var(--text-link); border-color: var(--border-input-focus); }

/* Modal */
.modal-overlay { position: fixed; inset: 0; z-index: 9998; background: color-mix(in srgb, var(--lg-bg-mid) 58%, transparent); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 1rem; }
.modal-content { position: relative; z-index: 9999; background: var(--surface-modal); width: 100%; max-width: 500px; border-radius: 22px; box-shadow: var(--lg-shadow-lg); overflow: hidden; border: 1px solid var(--border-card); }
.modal-header { padding: 1rem; border-bottom: 1px solid var(--border-card); display: flex; justify-content: space-between; align-items: center; }
.modal-header h3 { margin: 0; font-size: 1rem; font-weight: 900; color: var(--text-heading); }
.close-btn-sm { background: transparent; border: none; color: var(--text-placeholder); cursor: pointer; display: flex; transition: color .15s; }
.close-btn-sm:hover:not(:disabled) { color: var(--color-danger-text); }
.modal-body { padding: 1rem; display: flex; flex-direction: column; gap: 1rem; font-size: .875rem; color: var(--text-label); }

.summary-box { background: var(--accent-primary-soft); border: 1px dashed color-mix(in srgb, var(--accent-primary) 32%, transparent); padding: 1rem; border-radius: 12px; }
.modal-muted { color: var(--text-placeholder); }
.modal-total-row { border-top: 1px solid var(--border-card); color: var(--text-label); }
.modal-total { color: var(--text-link); font-size: 1.2rem; font-weight: 900; }

.payos-flow-note { display: flex; align-items: flex-start; gap: .75rem; padding: .85rem; border: 1px solid var(--border-card); border-radius: 12px; background: var(--surface-input); color: var(--text-label); }
.payos-flow-note svg { color: var(--text-link); flex: none; margin-top: .1rem; }
.payos-flow-note strong { display: block; color: var(--text-heading); font-size: .85rem; }
.payos-flow-note p { margin: .2rem 0 0; color: var(--text-body); font-size: .78rem; line-height: 1.45; }
.payment-loading { display: flex; align-items: center; justify-content: center; gap: .5rem; min-height: 10rem; border: 1px dashed var(--border-card); border-radius: 14px; background: var(--surface-input); color: var(--text-label); font-size: .85rem; font-weight: 750; }

.qr-result { display: flex; flex-direction: column; align-items: center; gap: .75rem; padding: .85rem; border: 1px solid var(--border-card); border-radius: 14px; background: var(--surface-input); text-align: center; }
.qr-result.confirmed { border-color: color-mix(in srgb, var(--color-success-text) 36%, var(--border-card)); background: var(--color-success-bg); }
.qr-image-wrap { position: relative; width: min(14rem, 100%); aspect-ratio: 1; }
.qr-image { width: min(14rem, 100%); aspect-ratio: 1; object-fit: contain; border-radius: 12px; border: 1px solid var(--border-card); background: var(--surface-card); }
.qr-confirmed-overlay { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; border-radius: 12px; background: color-mix(in srgb, var(--surface-card) 72%, transparent); color: var(--color-success-text); }
.qr-detail { width: 100%; display: flex; justify-content: space-between; gap: .75rem; color: var(--text-label); font-size: .8rem; text-align: left; }
.qr-detail strong { color: var(--text-heading); overflow-wrap: anywhere; text-align: right; }
.qr-result p { margin: 0; color: var(--text-body); font-size: .78rem; line-height: 1.45; }
.qr-success-note { color: var(--color-success-text) !important; font-weight: 800; }
.checkout-fallback { display: flex; flex-direction: column; align-items: center; gap: .75rem; padding: .85rem; border: 1px solid var(--border-card); border-radius: 14px; background: var(--surface-input); text-align: center; color: var(--text-label); font-size: .82rem; }
.checkout-fallback svg { color: var(--color-warning-text); }

.security-badge { display: flex; align-items: center; gap: .5rem; font-size: .75rem; color: var(--color-success-text); background: var(--color-success-bg); padding: .5rem; border-radius: 8px; justify-content: center; }

.modal-footer { padding: 1rem; border-top: 1px solid var(--border-card); display: flex; justify-content: flex-end; gap: .75rem; background: var(--surface-input); }

.modal-enter-active, .modal-leave-active { transition: all .3s cubic-bezier(0.16,1,.3,1); }
.modal-enter-from, .modal-leave-to { opacity: 0; transform: scale(0.95); }

@media (max-width: 640px) {
  .invoice-header { flex-direction: column; align-items: flex-start; }
  .invoice-amount-block { text-align: left; }
  .invoice-footer { flex-direction: column; }
  .btn-primary, .btn-secondary { width: 100%; }
}
</style>
