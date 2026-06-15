<script setup>
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'
import { useAuthStore } from '@/stores/auth'
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
const isProcessing = ref(false)
const paymentResult = ref(null)
const paymentConfirmed = ref(false)
const paymentPollTimer = ref(null)
const downloadingInvoiceId = ref('')
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

const downloadPDF = async (invoice) => {
  if (!invoice || downloadingInvoiceId.value) return

  downloadingInvoiceId.value = invoice.id
  let renderHost = null

  try {
    renderHost = createInvoicePdfRenderHost(invoice)
    document.body.appendChild(renderHost)
    await waitForNextPaint()

    const invoicePage = renderHost.querySelector('.invoice-page')
    if (!invoicePage) {
      throw new Error('Không tìm thấy mẫu hóa đơn để xuất PDF.')
    }

    const [{ default: html2canvas }, { default: jsPDF }] = await Promise.all([
      import('html2canvas'),
      import('jspdf'),
    ])

    const filename = `${sanitizeFilename(invoice.id || 'hoa-don-hoc-phi')}.pdf`

    const canvas = await html2canvas(renderHost, {
      scale: 2,
      useCORS: true,
      backgroundColor: '#ffffff',
      logging: false,
    })

    const imgData = canvas.toDataURL('image/jpeg', 0.95)
    const pdf = new jsPDF('p', 'pt', 'a4')
    const pdfW = pdf.internal.pageSize.getWidth()
    const pdfH = pdf.internal.pageSize.getHeight()
    pdf.addImage(imgData, 'JPEG', 0, 0, pdfW, pdfH)
    pdf.save(filename)

    popupStore.success('Đã tải hóa đơn', `File ${filename} đã được tạo.`)
  } catch (error) {
    popupStore.error('Không tải được PDF', error?.message || 'Vui lòng thử lại sau.')
  } finally {
    renderHost?.remove()
    downloadingInvoiceId.value = ''
  }
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
    soTien,
    giamTru,
    daThanhToan,
    soTienPhaiDong,
    conPhaiDong,
    dueDate: read(invoice, 'hanThanhToan', 'HanThanhToan'),
    status: mapStatus(rawStatus),
    rawStatus,
    items,
  }
}

function createInvoicePdfRenderHost(invoice) {
  const parsedDocument = new DOMParser().parseFromString(buildInvoicePdfHtml(invoice), 'text/html')
  const invoicePage = parsedDocument.body.querySelector('.invoice-page')
  if (!invoicePage) {
    throw new Error('Không tìm thấy mẫu hóa đơn để xuất PDF.')
  }

  const styleText = Array.from(parsedDocument.head.querySelectorAll('style'))
    .map((style) => style.textContent || '')
    .join('\n')

  const uuid = `pdf-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`
  const renderHost = document.createElement('div')
  renderHost.className = uuid
  renderHost.setAttribute('aria-hidden', 'true')
  Object.assign(renderHost.style, {
    position: 'fixed',
    left: '0',
    top: '0',
    width: '794px',
    minHeight: '1123px',
    overflow: 'hidden',
    pointerEvents: 'none',
    zIndex: '-1',
    background: '#eef2f7',
  })

  const styleElement = document.createElement('style')
  styleElement.textContent = scopeInvoicePdfStyles(styleText, uuid)
  renderHost.appendChild(styleElement)
  renderHost.appendChild(document.importNode(invoicePage, true))

  return renderHost
}

function scopeInvoicePdfStyles(cssText, uuid) {
  return cssText
    .replace(/@page[^{]*\{[^}]*\}/g, '')
    .replace(/@media print\s*\{[\s\S]*?\}\s*$/g, '')
    .replace(/(^|})\s*([^@{}][^{]+)\{/g, (...groups) => {
      const closeBrace = groups[1]
      const selectorText = groups[2]
      const scopedSelectors = selectorText
        .split(',')
        .map((selector) => {
          const trimmed = selector.trim()
          if (!trimmed) return trimmed
          if (trimmed === 'body') return `.${uuid}`
          if (trimmed === '*') return `.${uuid} *`
          return `.${uuid} ${trimmed}`
        })
        .join(', ')

      return `${closeBrace}\n${scopedSelectors} {`
    })
}

function waitForNextPaint() {
  return new Promise((resolve) => {
    window.requestAnimationFrame(() => {
      window.requestAnimationFrame(() => {
        window.setTimeout(resolve, 200)
      })
    })
  })
}

function sanitizeFilename(value) {
  return String(value || 'hoa-don-hoc-phi')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-zA-Z0-9._-]+/g, '-')
    .replace(/^-+|-+$/g, '')
    || 'hoa-don-hoc-phi'
}

function buildInvoicePdfHtml(invoice) {
  const user = authStore.user || {}
  const statusLabel = getStatusConfig(invoice.status).label
  const exportedAt = new Date()
  const amountDueText = `${formatNumber(invoice.conPhaiDong)} đ`
  const studentCode = user.userId ? `SV${String(user.userId).padStart(3, '0')}` : 'Chưa cập nhật'
  const campus = user.campusId ? `Cơ sở ${user.campusId}` : 'Chưa cập nhật'
  const summaryText = invoice.conPhaiDong <= 0
    ? 'Hóa đơn đã được ghi nhận thanh toán trên hệ thống. Sinh viên không còn khoản tiền phải đóng cho học kỳ này.'
    : 'Hóa đơn còn số tiền phải thanh toán. Vui lòng thực hiện thanh toán đúng hạn để hệ thống ghi nhận công nợ.'

  return `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8" />
  <title>Hóa đơn học phí - ${escapeHtml(invoice.id)}</title>
  <style>
    @page { size: A4; margin: 0; }
    * { box-sizing: border-box; }
    body { margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background: #eef2f7; color: #0f172a; font-size: 14px; }
    .invoice-page { width: 794px; min-height: 1123px; margin: 0 auto; background: #ffffff; padding: 36px 42px; }
    .invoice-header { display: table; width: 100%; padding-bottom: 22px; border-bottom: 2px solid #e2e8f0; }
    .brand { display: table-cell; vertical-align: top; width: 60%; }
    .brand-logo { display: inline-block; width: 54px; height: 54px; border-radius: 14px; background: #2563eb; color: #ffffff; text-align: center; line-height: 54px; font-weight: 800; font-size: 18px; vertical-align: top; margin-right: 12px; }
    .brand-text { display: inline-block; vertical-align: top; }
    .brand-text h1 { margin: 0; font-size: 22px; color: #0f172a; text-transform: uppercase; }
    .brand-text p { margin: 6px 0 0; color: #64748b; line-height: 1.5; font-size: 13px; }
    .invoice-meta { display: table-cell; vertical-align: top; width: 40%; text-align: right; }
    .status-badge-pdf { display: inline-block; padding: 7px 14px; border-radius: 999px; background: #dcfce7; color: #15803d; font-weight: 700; font-size: 13px; margin-bottom: 10px; }
    .invoice-code { color: #334155; font-weight: 700; font-size: 13px; }
    .invoice-title { display: table; width: 100%; margin: 30px 0 24px; }
    .invoice-title-left { display: table-cell; vertical-align: bottom; width: 65%; }
    .invoice-title-left h2 { margin: 0; font-size: 28px; color: #111827; }
    .invoice-title-left p { margin: 8px 0 0; color: #475569; font-size: 15px; font-weight: 600; }
    .amount-due { display: table-cell; vertical-align: bottom; width: 35%; text-align: right; }
    .amount-due span { display: block; color: #64748b; font-size: 13px; margin-bottom: 6px; }
    .amount-due strong { font-size: 30px; color: #2563eb; font-weight: 900; }
    .info-grid { display: table; width: 100%; border-spacing: 14px 0; margin-left: -14px; margin-bottom: 28px; }
    .info-card { display: table-cell; width: 50%; border: 1px solid #e2e8f0; border-radius: 14px; padding: 16px; background: #f8fafc; vertical-align: top; }
    .info-card h3 { margin: 0 0 12px; font-size: 15px; color: #0f172a; }
    .info-row { display: table; width: 100%; padding: 7px 0; border-bottom: 1px dashed #dbe3ef; }
    .info-row:last-child { border-bottom: none; }
    .info-row span { display: table-cell; color: #64748b; }
    .info-row strong { display: table-cell; text-align: right; color: #111827; font-weight: 700; }
    .fee-section { margin-top: 28px; }
    .section-heading { display: table; width: 100%; margin-bottom: 10px; }
    .section-heading h3 { display: table-cell; margin: 0; font-size: 17px; color: #0f172a; font-weight: 800; }
    .section-heading span { display: table-cell; text-align: right; font-size: 12px; color: #64748b; vertical-align: middle; }
    .fee-card { border: 1px solid #dbe3ef; border-radius: 16px; overflow: hidden; background: #ffffff; }
    .fee-table { width: 100%; border-collapse: collapse; }
    .fee-table thead { background: #eef5ff; }
    .fee-table th { padding: 14px 16px; text-align: left; font-size: 12px; color: #1e3a8a; text-transform: uppercase; letter-spacing: 0.3px; border-bottom: 1px solid #d6e4f5; }
    .fee-table td { padding: 15px 16px; border-bottom: 1px solid #edf2f7; vertical-align: top; }
    .text-right { text-align: right; }
    .fee-name { font-size: 14px; font-weight: 700; color: #0f172a; margin-bottom: 4px; }
    .fee-desc { font-size: 12px; color: #64748b; }
    .amount { font-size: 14px; font-weight: 800; color: #111827; white-space: nowrap; }
    .minus { color: #0f766e; }
    .note { font-size: 13px; color: #475569; }
    .summary-area { display: table; width: 100%; background: #f8fafc; padding: 20px 22px; border-top: 1px solid #e2e8f0; }
    .summary-note { display: table-cell; width: 52%; vertical-align: middle; padding-right: 24px; }
    .summary-note-title { font-size: 16px; font-weight: 800; color: #0f172a; margin-bottom: 6px; }
    .summary-note-text { font-size: 13px; line-height: 1.6; color: #64748b; }
    .summary-total { display: table-cell; width: 48%; vertical-align: top; background: #ffffff; border: 1px solid #dbe3ef; border-radius: 14px; overflow: hidden; }
    .summary-line { display: table; width: 100%; border-bottom: 1px solid #edf2f7; }
    .summary-line span, .summary-line strong { display: table-cell; padding: 12px 16px; font-size: 13px; }
    .summary-line span { color: #475569; }
    .summary-line strong { text-align: right; color: #0f172a; font-weight: 800; white-space: nowrap; }
    .summary-line.final { background: #eff6ff; border-bottom: none; }
    .summary-line.final span { font-size: 15px; font-weight: 800; color: #0f172a; }
    .summary-line.final strong { font-size: 21px; font-weight: 900; color: #2563eb; }
    .payment-note { margin-top: 26px; padding: 16px 18px; border-radius: 14px; border: 1px solid #bfdbfe; background: #f8fbff; color: #334155; line-height: 1.6; font-size: 13px; }
    .payment-note strong { color: #1d4ed8; }
    .footer { margin-top: 42px; padding-top: 18px; border-top: 1px solid #e2e8f0; display: table; width: 100%; color: #64748b; font-size: 12px; line-height: 1.5; }
    .footer-left { display: table-cell; width: 60%; vertical-align: bottom; }
    .signature { display: table-cell; width: 40%; text-align: center; color: #334155; vertical-align: bottom; }
    .signature-space { height: 52px; }
    .signature-line { border-top: 1px solid #94a3b8; padding-top: 8px; font-weight: 700; }
    .watermark { margin-top: 24px; text-align: center; color: #94a3b8; font-size: 11px; }
    @media print { body { background: #ffffff; } .invoice-page { margin: 0; } }
  </style>
</head>
<body>
  <div class="invoice-page">
    <div class="invoice-header">
      <div class="brand">
        <div class="brand-logo">LMS</div>
        <div class="brand-text">
          <h1>Trung tâm đào tạo AET</h1>
          <p>Hệ thống quản lý học tập LMS<br />Email: support@aet.edu.vn | Hotline: 1900 0000</p>
        </div>
      </div>
      <div class="invoice-meta">
        <div class="status-badge-pdf">${escapeHtml(statusLabel)}</div>
        <div class="invoice-code">Mã hóa đơn: ${escapeHtml(invoice.id)}</div>
      </div>
    </div>
    <div class="invoice-title">
      <div class="invoice-title-left">
        <h2>Hóa đơn học phí</h2>
        <p>${escapeHtml(invoice.semester)}</p>
      </div>
      <div class="amount-due">
        <span>Còn phải thanh toán</span>
        <strong>${escapeHtml(amountDueText)}</strong>
      </div>
    </div>
    <div class="info-grid">
      <div class="info-card">
        <h3>Thông tin sinh viên</h3>
        ${infoRow('Mã sinh viên', studentCode)}
        ${infoRow('Họ và tên', user.fullName || authStore.displayName || 'Chưa cập nhật')}
        ${infoRow('Lớp', user.className || user.lop || 'Chưa cập nhật')}
        ${infoRow('Cơ sở', campus)}
      </div>
      <div class="info-card">
        <h3>Thông tin hóa đơn</h3>
        ${infoRow('Ngày tạo', formatDate(exportedAt))}
        ${infoRow('Hạn thanh toán', formatDate(invoice.dueDate))}
        ${infoRow('Phương thức', 'Chuyển khoản')}
        ${infoRow('Trạng thái', statusLabel)}
      </div>
    </div>
    <section class="fee-section">
      <div class="section-heading"><h3>Chi tiết khoản thu</h3><span>Đơn vị tiền tệ: VNĐ</span></div>
      <div class="fee-card">
        <table class="fee-table">
          <thead><tr><th style="width: 45%;">Nội dung</th><th style="width: 25%;" class="text-right">Số tiền</th><th style="width: 30%;" class="text-right">Ghi chú</th></tr></thead>
          <tbody>
            ${feeRow('Học phí học kỳ', 'Khoản học phí chính của học kỳ', invoice.soTien, 'Khoản phải thu')}
            ${feeRow('Giảm trừ', 'Học bổng hoặc chính sách miễn giảm', -invoice.giamTru, 'Học bổng / miễn giảm')}
            ${feeRow('Đã thanh toán', 'Số tiền sinh viên đã thanh toán', -invoice.daThanhToan, 'Chuyển khoản')}
          </tbody>
        </table>
        <div class="summary-area">
          <div class="summary-note"><div class="summary-note-title">Tổng kết thanh toán</div><div class="summary-note-text">${escapeHtml(summaryText)}</div></div>
          <div class="summary-total">
            ${summaryLine('Tổng học phí', invoice.soTien)}
            ${summaryLine('Tổng giảm trừ', -invoice.giamTru)}
            ${summaryLine('Đã thanh toán', -invoice.daThanhToan)}
            <div class="summary-line final"><span>Còn phải đóng</span><strong>${escapeHtml(amountDueText)}</strong></div>
          </div>
        </div>
      </div>
    </section>
    <div class="payment-note"><strong>Ghi chú:</strong> Hóa đơn này được tạo tự động từ hệ thống LMS. Trường hợp sinh viên đã thanh toán nhưng trạng thái chưa được cập nhật, vui lòng liên hệ phòng tài chính để được kiểm tra giao dịch.</div>
    <div class="footer">
      <div class="footer-left"><strong>Thông tin liên hệ</strong><br />Phòng tài chính - Trung tâm đào tạo AET<br />Địa chỉ: Biên Hòa, Đồng Nai<br />Thời gian xuất hóa đơn: ${escapeHtml(formatDateTime(exportedAt))}</div>
      <div class="signature">Người lập phiếu<div class="signature-space"></div><div class="signature-line">Hệ thống LMS</div></div>
    </div>
    <div class="watermark">Đây là hóa đơn điện tử được xuất từ hệ thống LMS. Không cần chữ ký tay.</div>
  </div>
</body>
</html>`
}

function infoRow(label, value) {
  return `<div class="info-row"><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong></div>`
}

function feeRow(name, desc, amount, note) {
  const isMinus = amount < 0
  return `<tr><td><div class="fee-name">${escapeHtml(name)}</div><div class="fee-desc">${escapeHtml(desc)}</div></td><td class="text-right amount ${isMinus ? 'minus' : ''}">${escapeHtml(formatSignedAmount(amount))}</td><td class="text-right note">${escapeHtml(note)}</td></tr>`
}

function summaryLine(label, amount) {
  return `<div class="summary-line"><span>${escapeHtml(label)}</span><strong>${escapeHtml(formatSignedAmount(amount))}</strong></div>`
}

function formatSignedAmount(amount) {
  const value = toNumber(amount)
  const prefix = value < 0 ? '-' : ''
  return `${prefix}${formatNumber(Math.abs(value))} đ`
}

function escapeHtml(value) {
  return String(value ?? '')
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
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
          <button
            v-if="inv.status !== 'Cancelled'"
            class="btn-secondary"
            :disabled="downloadingInvoiceId === inv.id"
            @click="downloadPDF(inv)"
          >
            <Download :size="15" :class="{ 'animate-pulse': downloadingInvoiceId === inv.id }"/>
            {{ downloadingInvoiceId === inv.id ? 'Đang tạo PDF...' : 'Tải PDF Hóa đơn' }}
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
