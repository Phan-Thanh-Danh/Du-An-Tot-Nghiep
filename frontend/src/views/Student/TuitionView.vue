<script setup>
import { computed, onMounted, ref } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { usePopupStore } from '@/stores/popup'
import { useAuthStore } from '@/stores/auth'
import QRCode from 'qrcode'
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
const authStore = useAuthStore()

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
const paymentMethod = ref('payos')
const isProcessing = ref(false)
const paymentResult = ref(null)
const payosQrImage = ref('')
const paymentStatusTimer = ref(null)

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

const openPaymentModal = (invoice) => {
  if (invoice.status === 'Processing') return // Prevent double payment
  if (invoice.conPhaiDong <= 0) {
    popupStore.info('Hóa đơn đã đủ', 'Hóa đơn này không còn số tiền cần thanh toán.')
    return
  }

  selectedInvoice.value = invoice
  paymentMethod.value = 'payos'
  paymentResult.value = null
  payosQrImage.value = ''
  clearPaymentStatusPolling()
  modalOpen.value = true
}

const closePaymentModal = () => {
  if (isProcessing.value) return
  clearPaymentStatusPolling()
  modalOpen.value = false
  selectedInvoice.value = null
  paymentResult.value = null
  payosQrImage.value = ''
}

const confirmPayment = async () => {
  if (!selectedInvoice.value) return

  isProcessing.value = true
  paymentResult.value = null
  payosQrImage.value = ''

  try {
    const result = await createTuitionPayment(selectedInvoice.value.maHoaDon, paymentMethod.value)
    paymentResult.value = result

    if (paymentMethod.value === 'payos') {
      if (result?.qrPayload) {
        payosQrImage.value = await QRCode.toDataURL(result.qrPayload, {
          width: 280,
          margin: 1,
          errorCorrectionLevel: 'M',
        })
        startPaymentStatusPolling(result)
        return
      }

      if (!result?.checkoutUrl) {
        throw new Error('PayOS không trả về mã QR thanh toán.')
      }

      window.location.href = result.checkoutUrl
      return
    }

    if (paymentMethod.value === 'vietqr') {
      if (!result?.qrUrl) {
        throw new Error('Backend không trả về ảnh VietQR.')
      }

      popupStore.success('Đã tạo mã VietQR', 'Vui lòng chuyển khoản đúng số tiền và nội dung.')
      await loadTransactions()
    }
  } catch (error) {
    popupStore.error('Không tạo được thanh toán', error?.message || 'Vui lòng thử lại sau.')
  } finally {
    isProcessing.value = false
  }
}

function startPaymentStatusPolling(result) {
  clearPaymentStatusPolling()
  const check = async () => {
    try {
      const status = await getTuitionPaymentStatus(result.maGiaoDich)
      const raw = String(status?.trangThai || status?.TrangThai || '').toLowerCase()
      if (raw === 'thanh_cong' || raw === 'da_thanh_toan' || raw === 'thanh_toan') {
        clearPaymentStatusPolling()
        popupStore.success('Thanh toán thành công', 'Hóa đơn đã được xác nhận thanh toán.')
        modalOpen.value = false
        selectedInvoice.value = null
        paymentResult.value = null
        payosQrImage.value = ''
        loadTuitionData()
        return
      }
      paymentStatusTimer.value = setTimeout(check, 5000)
    } catch {
      paymentStatusTimer.value = setTimeout(check, 5000)
    }
  }
  paymentStatusTimer.value = setTimeout(check, 5000)
}

function clearPaymentStatusPolling() {
  if (paymentStatusTimer.value) {
    clearTimeout(paymentStatusTimer.value)
    paymentStatusTimer.value = null
  }
}

const downloadPDF = (id) => {
  const inv = invoices.value.find(i => i.id === id) || selectedInvoice.value
  if (!inv) return

  const studentName = authStore.user?.fullName || authStore.user?.FullName || authStore.displayName || 'Sinh viên'
  const studentCode = authStore.user?.username || authStore.user?.email || inv.id
  const campusName = authStore.user?.campusName || 'FPT Polytechnic'
  const formattedTotal = formatCurrency(inv.total)
  const formattedDate = formatDate(inv.dueDate)
  const isPaid = inv.status === 'Paid'
  const statusText = isPaid ? 'ĐÃ THANH TOÁN' : 'CHƯA THANH TOÁN'
  const statusColor = isPaid ? '#16a34a' : '#dc2626'

  const rows = inv.items
    .map((item, idx) => `
      <tr>
        <td style="text-align: center;">${idx + 1}</td>
        <td>${item.name}</td>
        <td class="text-right">${formatCurrency(item.amount)}</td>
      </tr>`)
    .join('')

  const scriptStart = '<scr' + 'ipt'
  const scriptEnd = '</scr' + 'ipt>'

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <title>HoaDon_${inv.id}_${studentName.replace(/\s+/g, '_')}</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: 'Times New Roman', Times, serif; font-size: 12pt; color: #000; background: #fff; padding: 15mm 12mm; line-height: 1.4; }
    .header { display: flex; justify-content: space-between; align-items: flex-start; border-bottom: 2px solid #000; padding-bottom: 12px; margin-bottom: 18px; }
    .logo-title { font-size: 15pt; font-weight: bold; text-transform: uppercase; color: #000; }
    .logo-sub { font-size: 10pt; color: #333; margin-top: 3px; font-style: italic; }
    .invoice-title { text-align: right; }
    .invoice-title h2 { margin: 0; color: #000; font-size: 15pt; text-transform: uppercase; font-weight: bold; }
    .invoice-title p { margin: 3px 0 0; color: #444; font-size: 10pt; }
    .info-box { border: 1px solid #000; background: #fdfdfd; padding: 10px 14px; margin-bottom: 18px; font-size: 11pt; }
    .info-row { display: flex; justify-content: space-between; margin-bottom: 6px; }
    .info-row:last-child { margin-bottom: 0; }
    .info-label { font-weight: bold; }
    table { width: 100%; border-collapse: collapse; font-size: 11pt; margin-bottom: 20px; }
    th, td { border: 1px solid #000; padding: 7px 10px; text-align: left; }
    th { background: #f1f5f9; font-weight: bold; text-align: center; text-transform: uppercase; font-size: 10pt; }
    .text-right { text-align: right; }
    .total-row { font-weight: bold; font-size: 12pt; background: #fafafa; }
    .stamp-container { display: flex; justify-content: space-between; align-items: flex-end; margin-top: 30px; }
    .stamp-box { border: 1.5px solid #ef4444; color: #ef4444; padding: 6px 10px; border-radius: 8px; font-weight: bold; text-align: center; transform: rotate(-1.5deg); background: rgba(254, 242, 242, 0.4); display: inline-block; width: 155px; word-break: break-all; line-height: 1.2; }
    .footer-note { color: #555; font-size: 9.5pt; font-style: italic; max-width: 320px; }
  </style>
</head>
<body>
  <div id="report-content">
    <div class="header">
      <div>
        <div class="logo-title">TRƯỜNG ĐẠI HỌC LMS ACADEMIC</div>
        <div class="logo-sub">Cơ sở: ${campusName}</div>
        <div class="logo-sub">Mã số thuế: 0102030405 | Hotline: 1900 1234</div>
      </div>
      <div class="invoice-title">
        <h2>HÓA ĐƠN ĐIỆN TỬ</h2>
        <p>Mã hóa đơn: <strong>${inv.id}</strong></p>
        <p>Hạn thanh toán: ${formattedDate}</p>
      </div>
    </div>

    <div class="info-box">
      <div class="info-row">
        <span class="info-label">Sinh viên thụ hưởng:</span>
        <span class="info-val"><strong>${studentName}</strong> (MSSV: ${studentCode})</span>
      </div>
      <div class="info-row">
        <span class="info-label">Nội dung:</span>
        <span class="info-val">${inv.semester}</span>
      </div>
      <div class="info-row">
        <span class="info-label">Trạng thái hóa đơn:</span>
        <span class="info-val" style="color: ${statusColor}; font-weight: bold;">${statusText}</span>
      </div>
    </div>

    <table>
      <thead>
        <tr>
          <th style="width: 50px; text-align: center;">STT</th>
          <th>Tên dịch vụ / Khoản thu</th>
          <th class="text-right" style="width: 150px;">Thành tiền (VND)</th>
        </tr>
      </thead>
      <tbody>
        ${rows}
        <tr>
          <td colspan="2" class="text-right" style="font-weight: bold;">Thuế giá trị gia tăng (VAT 0%):</td>
          <td class="text-right">0 ₫</td>
        </tr>
        <tr class="total-row">
          <td colspan="2" class="text-right">TỔNG CỘNG TIỀN THANH TOÁN:</td>
          <td class="text-right">${formattedTotal}</td>
        </tr>
      </tbody>
    </table>

    <div class="stamp-container">
      <div class="footer-note">
        * Chứng từ hóa đơn điện tử gốc được mã hóa và lưu trữ chính thức trên CSDL LMS System.
      </div>
      <div class="stamp-box">
        <div style="font-size: 10pt; font-weight: 800; text-transform: uppercase; color: #ef4444; margin-bottom: 2px;">ĐÃ KÝ ĐIỆN TỬ</div>
        <div style="font-size: 8.5pt; font-weight: 700; color: #ef4444; margin-bottom: 2px;">LMS UNIVERSITY</div>
        <div style="font-size: 7.5pt; color: #ef4444; word-break: break-all; font-weight: 600;">${new Date().toISOString()}</div>
      </div>
    </div>
  </div>

  <div id="loading-overlay" style="position:fixed;inset:0;background:rgba(255,255,255,0.95);display:flex;flex-direction:column;align-items:center;justify-content:center;z-index:9999;font-family:sans-serif;">
    <div style="width:48px;height:48px;border:5px solid #e2e8f0;border-top-color:#ea580c;border-radius:50%;animation:spin 0.8s linear infinite;margin-bottom:16px;"></div>
    <p style="color:#ea580c;font-weight:700;font-size:15px;margin:0;">Đang tạo file hóa đơn PDF...</p>
    <p style="color:#64748b;font-size:12px;margin:4px 0 0;">File PDF sẽ tự động tải về thiết bị của bạn</p>
  </div>
  <style>@keyframes spin { to { transform: rotate(360deg); } }</style>
  ${scriptStart} src="https://cdnjs.cloudflare.com/ajax/libs/html2pdf.js/0.10.1/html2pdf.bundle.min.js" crossorigin="anonymous">${scriptEnd}
  ${scriptStart}>
    window.onload = function() {
      var overlay = document.getElementById('loading-overlay');
      var content = document.getElementById('report-content');
      var filename = 'HoaDon_${inv.id}_${studentName.replace(/\s+/g, '_')}.pdf';
      var opt = {
        margin: [10, 10, 10, 10],
        filename: filename,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, logging: false },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
      };
      html2pdf().set(opt).from(content).save().then(function() {
        overlay.innerHTML = '<p style="color:#ea580c;font-weight:700;font-size:16px;">✓ Tải hóa đơn PDF thành công!</p><p style="color:#64748b;font-size:12px;">Cửa sổ này sẽ tự đóng...</p>';
        setTimeout(function() { window.close(); }, 1200);
      }).catch(function(err) {
        overlay.innerHTML = '<p style="color:#e11d48;font-weight:700;font-size:14px;">Lỗi tạo PDF: ' + err.message + '</p><button onclick="window.print()" style="margin-top:12px;padding:8px 20px;background:#ea580c;color:#fff;border:none;border-radius:8px;cursor:pointer;">In bằng trình duyệt</button>';
      });
    };
  ${scriptEnd}
</body>
</html>`

  const printWindow = window.open('', '_blank', 'width=900,height=700')
  if (printWindow) {
    printWindow.document.write(html)
    printWindow.document.close()
    popupStore.success('Đang tải hóa đơn PDF', `Hóa đơn ${inv.id} đang được kết xuất và tải về dưới dạng file PDF.`)
  } else {
    popupStore.warning('Cảnh báo Popup', 'Vui lòng cho phép mở cửa sổ bật lên (popup) để tải file PDF hóa đơn.')
  }
}

async function loadTuitionData() {
  isLoadingData.value = true
  loadError.value = ''
  try {
    const results = await Promise.allSettled([loadInvoices(), loadTransactions()])
    const errors = results.filter(r => r.status === 'rejected').map(r => r.reason)
    if (errors.length > 0) throw new Error(errors.map(e => e?.message || e).join('; '))
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

function setPaymentMethod(provider) {
  paymentMethod.value = provider
  paymentResult.value = null
  payosQrImage.value = ''
}

const showPayosQr = computed(() => Boolean(payosQrImage.value))

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

              <div class="payment-methods">
                <label class="font-semibold text-sm mb-2 block">Chọn phương thức thanh toán</label>
                <div class="method-options">
                  <label class="method-radio" :class="paymentMethod === 'payos' && 'selected'">
                    <input type="radio" :checked="paymentMethod === 'payos'" name="paymentMethod" @change="setPaymentMethod('payos')" />
                    <CreditCard :size="20" />
                    <div class="flex-1">
                      <div class="font-semibold">PayOS</div>
                      <div class="method-caption">Thanh toán tự động qua PayOS</div>
                    </div>
                  </label>
                  
                  <label class="method-radio" :class="paymentMethod === 'vietqr' && 'selected'">
                    <input type="radio" :checked="paymentMethod === 'vietqr'" name="paymentMethod" @change="setPaymentMethod('vietqr')" />
                    <Building2 :size="20" />
                    <div class="flex-1">
                      <div class="font-semibold">VietQR</div>
                      <div class="method-caption">Chuyển khoản VietQR</div>
                    </div>
                  </label>
                </div>
              </div>

              <div v-if="paymentResult?.qrUrl && paymentMethod === 'vietqr'" class="qr-result">
                <img :src="paymentResult.qrUrl" alt="VietQR thanh toán học phí" class="qr-image" />
                <div class="qr-detail">
                  <span>Số tiền</span>
                  <strong>{{ formatCurrency(paymentResult.amount) }}</strong>
                </div>
                <div class="qr-detail">
                  <span>Nội dung chuyển khoản</span>
                  <strong>{{ paymentResult.noiDungChuyenKhoan }}</strong>
                </div>
                <p>Sau khi chuyển khoản, kế toán sẽ đối soát và xác nhận thanh toán.</p>
              </div>

              <div v-if="payosQrImage && paymentMethod === 'payos'" class="qr-result">
                <img :src="payosQrImage" alt="QR PayOS thanh toán học phí" class="qr-image" />
                <div class="qr-detail">
                  <span>Số tiền</span>
                  <strong>{{ formatCurrency(selectedInvoice?.conPhaiDong) }}</strong>
                </div>
                <div class="qr-detail">
                  <span>Trạng thái</span>
                  <strong>Đang chờ quét mã</strong>
                </div>
                <p>Mở ứng dụng ngân hàng quét mã QR để thanh toán qua PayOS. Hệ thống tự xác nhận khi chuyển khoản thành công.</p>
                <a
                  v-if="paymentResult?.checkoutUrl"
                  :href="paymentResult.checkoutUrl"
                  target="_blank"
                  rel="noopener"
                  class="qr-fallback-link"
                >
                  Quét không được? Mở cổng thanh toán PayOS
                </a>
              </div>

              <div class="security-badge">
                <ShieldCheck :size="16" />
                <span>Giao dịch được mã hóa và bảo mật bởi chữ ký số HMAC.</span>
              </div>
            </div>

            <div class="modal-footer">
              <button class="btn-secondary" @click="closePaymentModal" :disabled="isProcessing">
                {{ showPayosQr || paymentResult?.qrUrl ? 'Đóng' : 'Hủy' }}
              </button>
              <button v-if="!showPayosQr && !(paymentResult?.qrUrl && paymentMethod === 'vietqr')" class="btn-primary" @click="confirmPayment" :disabled="isProcessing">
                <span v-if="isProcessing" class="flex items-center gap-2">
                  <Clock class="animate-spin" :size="16" /> Đang xử lý...
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

.method-options { display: flex; flex-direction: column; gap: .5rem; }
.method-radio { display: flex; align-items: center; gap: 1rem; padding: .85rem; border: 1px solid var(--border-input); border-radius: 12px; cursor: pointer; transition: all .2s; background: var(--surface-input); }
.method-radio:hover { border-color: var(--border-input-focus); }
.method-radio.selected { border-color: var(--border-input-focus); background: var(--accent-primary-soft); color: var(--text-link); box-shadow: var(--lg-shadow-sm); }
.method-radio input { display: none; }
.method-caption { color: var(--text-placeholder); font-size: .75rem; }

.qr-result { display: flex; flex-direction: column; align-items: center; gap: .75rem; padding: .85rem; border: 1px solid var(--border-card); border-radius: 14px; background: var(--surface-input); text-align: center; }
.qr-image { width: min(14rem, 100%); aspect-ratio: 1; object-fit: contain; border-radius: 12px; border: 1px solid var(--border-card); background: var(--surface-card); }
.qr-detail { width: 100%; display: flex; justify-content: space-between; gap: .75rem; color: var(--text-label); font-size: .8rem; text-align: left; }
.qr-detail strong { color: var(--text-heading); overflow-wrap: anywhere; text-align: right; }
.qr-result p { margin: 0; color: var(--text-body); font-size: .78rem; line-height: 1.45; }
.qr-fallback-link { font-size: .8rem; font-weight: 750; color: var(--text-link); text-decoration: underline; }

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
