<script setup>
import { ref, computed } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import {
  CreditCard, Wallet, Receipt, DollarSign,
  AlertCircle, CheckCircle2, XCircle, Clock,
  Sparkles, Download, ArrowRight, ShieldCheck,
  Building2, Smartphone, FileText
} from 'lucide-vue-next'

const popupStore = usePopupStore()

// Mock Data
const metrics = [
  { label: 'Tổng công nợ', value: '18,500,000', unit: 'đ', icon: Receipt, tone: 'slate', hint: 'Kỳ Spring 2026' },
  { label: 'Giảm trừ (Học bổng)', value: '3,700,000', unit: 'đ', icon: Sparkles, tone: 'violet', hint: 'Áp dụng tự động (20%)' },
  { label: 'Đã thanh toán', value: '5,000,000', unit: 'đ', icon: CheckCircle2, tone: 'green', hint: 'Thanh toán đợt 1' },
  { label: 'Dư nợ còn lại', value: '9,800,000', unit: 'đ', icon: Wallet, tone: 'amber', hint: 'Cần thanh toán' },
]

const mockInvoices = ref([
  { 
    id: 'INV-2026-SP-01', 
    semester: 'Kỳ Spring 2026 (Đợt 2)', 
    total: 9800000, 
    dueDate: new Date(2026, 5, 10), // June 10
    status: 'Unpaid',
    items: [
      { name: 'Học phí 15 tín chỉ', amount: 12000000 },
      { name: 'Phí bảo hiểm Y tế', amount: 800000 },
      { name: 'Trừ Học bổng GPA (20%)', amount: -3000000 }
    ]
  },
  { 
    id: 'INV-2026-SP-00', 
    semester: 'Kỳ Spring 2026 (Đợt 1)', 
    total: 5000000, 
    dueDate: new Date(2026, 1, 15),
    status: 'Paid',
    items: [
      { name: 'Học phí tạm ứng', amount: 5000000 }
    ]
  },
  { 
    id: 'INV-2025-FA-01', 
    semester: 'Kỳ Fall 2025', 
    total: 14500000, 
    dueDate: new Date(2025, 9, 20),
    status: 'Paid',
    items: [
      { name: 'Học phí 18 tín chỉ', amount: 14500000 }
    ]
  }
])

const mockTransactions = [
  { id: 1, txId: 'TX-VNP-993821', date: new Date(2026, 1, 12, 10, 30), amount: 5000000, method: 'VNPay', status: 'Success' },
  { id: 2, txId: 'TX-MOM-112233', date: new Date(2025, 9, 15, 14, 20), amount: 14500000, method: 'MoMo', status: 'Success' },
  { id: 3, txId: 'TX-BNK-884422', date: new Date(2025, 9, 15, 14, 15), amount: 14500000, method: 'Bank Transfer', status: 'Failed' }
]

const statusConfig = {
  Unpaid: { label: 'Chưa thanh toán', cls: 'badge-red', icon: AlertCircle },
  Partial: { label: 'Thanh toán một phần', cls: 'badge-amber', icon: Clock },
  Paid: { label: 'Đã thanh toán', cls: 'badge-green', icon: CheckCircle2 },
  Overdue: { label: 'Quá hạn', cls: 'badge-slate', icon: XCircle },
  Processing: { label: 'Đang xử lý', cls: 'badge-blue', icon: Clock },
  Failed: { label: 'Thất bại', cls: 'badge-red', icon: XCircle },
  Success: { label: 'Thành công', cls: 'badge-green', icon: CheckCircle2 }
}

const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val)
const formatDate = (date) => new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(date)
const formatDateTime = (date) => new Intl.DateTimeFormat('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }).format(date)

// State
const activeTab = ref('invoices') // 'invoices' or 'history'
const modalOpen = ref(false)
useBodyScrollLock(modalOpen)
const selectedInvoice = ref(null)
const paymentMethod = ref('vnpay')
const isProcessing = ref(false)

// Actions
const openPaymentModal = (invoice) => {
  if (invoice.status === 'Processing') return // Prevent double payment
  selectedInvoice.value = invoice
  paymentMethod.value = 'vnpay'
  modalOpen.value = true
}

const closePaymentModal = () => {
  if (isProcessing.value) return
  modalOpen.value = false
  selectedInvoice.value = null
}

const confirmPayment = () => {
  isProcessing.value = true
  
  // Set status to Processing locally to simulate lock
  const idx = mockInvoices.value.findIndex(inv => inv.id === selectedInvoice.value.id)
  if (idx !== -1) mockInvoices.value[idx].status = 'Processing'

  setTimeout(() => {
    isProcessing.value = false
    // Simulate redirection or success
    if (idx !== -1) mockInvoices.value[idx].status = 'Paid'
    closePaymentModal()
    popupStore.success('Thanh toán thành công', 'Giao dịch của bạn đã được ghi nhận.')
  }, 2000)
}

const downloadPDF = (id) => {
  popupStore.info('Đang tải', `Đang tải hóa đơn ${id}...`)
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
      <router-link to="/student/requests" class="btn-outline">
        Yêu cầu hoàn phí / Bảo lưu
      </router-link>
    </div>

    <!-- AI Banner -->
    <div class="ai-banner banner-violet">
      <div class="banner-icon"><Sparkles :size="24" /></div>
      <div class="banner-content">
        <h3>AI Phân tích: Giảm trừ học bổng tự động</h3>
        <p>Chúc mừng! Dựa trên kết quả GPA 3.8 từ kỳ trước, hệ thống đã tự động tính toán và áp dụng mức giảm trừ học bổng 20% (tương đương 3,700,000đ) vào hóa đơn kỳ này.</p>
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
      <div v-for="inv in mockInvoices" :key="inv.id" class="invoice-card" :class="`card-${inv.status}`">
        <div class="invoice-header">
          <div>
            <div class="flex items-center gap-2 mb-1">
              <span class="invoice-id">{{ inv.id }}</span>
              <span class="status-badge" :class="statusConfig[inv.status].cls">
                <component :is="statusConfig[inv.status].icon" :size="12" />
                {{ statusConfig[inv.status].label }}
              </span>
            </div>
            <h3 class="invoice-semester">{{ inv.semester }}</h3>
          </div>
          <div class="invoice-amount-block">
            <span class="amount-lbl">Tổng thanh toán:</span>
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
    </div>

    <!-- Tab Content: Transaction History -->
    <div v-else class="content-section">
      <div class="table-container">
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
            <tr v-for="tx in mockTransactions" :key="tx.id">
              <td class="font-bold transaction-id">{{ tx.txId }}</td>
              <td>{{ formatDateTime(tx.date) }}</td>
              <td>
                <div class="flex items-center gap-1.5">
                  <component :is="tx.method === 'VNPay' ? CreditCard : tx.method === 'MoMo' ? Smartphone : Building2" :size="14" class="method-icon" />
                  {{ tx.method }}
                </div>
              </td>
              <td class="font-semibold">{{ formatCurrency(tx.amount) }}</td>
              <td>
                <span class="status-badge" :class="statusConfig[tx.status].cls">
                  <component :is="statusConfig[tx.status].icon" :size="12" />
                  {{ statusConfig[tx.status].label }}
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
                  <span class="font-bold">{{ selectedInvoice?.id }}</span>
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

              <div class="payment-methods">
                <label class="font-semibold text-sm mb-2 block">Chọn phương thức thanh toán</label>
                <div class="method-options">
                  <label class="method-radio" :class="paymentMethod === 'vnpay' && 'selected'">
                    <input type="radio" v-model="paymentMethod" value="vnpay" name="paymentMethod" />
                    <CreditCard :size="20" />
                    <div class="flex-1">
                      <div class="font-bold">VNPay</div>
                      <div class="method-caption">Thẻ ATM / Thẻ tín dụng</div>
                    </div>
                  </label>
                  
                  <label class="method-radio" :class="paymentMethod === 'momo' && 'selected'">
                    <input type="radio" v-model="paymentMethod" value="momo" name="paymentMethod" />
                    <Smartphone :size="20" />
                    <div class="flex-1">
                      <div class="font-bold">Ví MoMo</div>
                      <div class="method-caption">Quét mã QR</div>
                    </div>
                  </label>

                  <label class="method-radio" :class="paymentMethod === 'bank' && 'selected'">
                    <input type="radio" v-model="paymentMethod" value="bank" name="paymentMethod" />
                    <Building2 :size="20" />
                    <div class="flex-1">
                      <div class="font-bold">Chuyển khoản Ngân hàng</div>
                      <div class="method-caption">Chuyển khoản theo số tài khoản</div>
                    </div>
                  </label>
                </div>
              </div>

              <div class="security-badge">
                <ShieldCheck :size="16" />
                <span>Giao dịch được mã hóa và bảo mật bởi chữ ký số HMAC.</span>
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn-secondary" @click="closePaymentModal" :disabled="isProcessing">Hủy</button>
              <button class="btn-primary" @click="confirmPayment" :disabled="isProcessing">
                <span v-if="isProcessing" class="flex items-center gap-2">
                  <Clock class="animate-spin" :size="16" /> Đang chuyển hướng...
                </span>
                <span v-else class="flex items-center gap-2">
                  Xác nhận Thanh toán <ArrowRight :size="16" />
                </span>
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

/* Invoices */
.invoice-card { background: var(--surface-card); border: 1px solid var(--border-card); border-radius: 18px; overflow: hidden; box-shadow: var(--lg-shadow-sm); transition: transform .2s, border-color .2s; }
.invoice-card:hover { transform: translateY(-2px); }
.card-Unpaid { border-color: color-mix(in srgb, var(--color-danger-text) 24%, var(--border-card)); }
.card-Paid { border-color: color-mix(in srgb, var(--color-success-text) 24%, var(--border-card)); }
.card-Processing { border-color: color-mix(in srgb, var(--text-link) 24%, var(--border-card)); }

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

.method-options { display: flex; flex-direction: column; gap: .5rem; }
.method-radio { display: flex; align-items: center; gap: 1rem; padding: .85rem; border: 1px solid var(--border-input); border-radius: 12px; cursor: pointer; transition: all .2s; background: var(--surface-input); }
.method-radio:hover { border-color: var(--border-input-focus); }
.method-radio.selected { border-color: var(--border-input-focus); background: var(--accent-primary-soft); color: var(--text-link); box-shadow: var(--lg-shadow-sm); }
.method-radio input { display: none; }
.method-caption { color: var(--text-placeholder); font-size: .75rem; }

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
