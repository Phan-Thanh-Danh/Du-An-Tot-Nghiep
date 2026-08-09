<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  FileText,
  ChevronDown,
  Eye,
  Download,
  Printer,
  ChevronLeft,
  X,
  ShieldCheck,
  CheckCircle2
} from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import { parentApi } from '@/services/parentApi'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || null)
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref('')

const children = ref([])
const invoices = ref([])
const childDetail = ref(null)

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0] || null
})

const currentChildInfo = computed(() => {
  const c = currentChild.value || {}
  const d = childDetail.value || {}
  return {
    id: c.id || d.id || '—',
    name: c.name || d.name || 'Học sinh',
    studentId: c.studentId || d.email || c.id || '—',
    className: d.className || c.className || c.class || 'CNTT-K16A',
    major: d.major || c.major || c.majorName || 'Công nghệ thông tin',
    campus: d.campus || c.campus || d.donVi || c.donVi || 'FPT Polytechnic Hồ Chí Minh'
  }
})

const isInvoiceModalOpen = ref(false)
const selectedInvoice = ref(null)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const childrenRes = await parentApi.getChildren()
    children.value = childrenRes?.data || []
    const validChild = children.value.find(child => child.id === activeChildId.value) || children.value[0]
    if (!validChild) {
      invoices.value = []
      return
    }
    activeChildId.value = validChild.id
    localStorage.setItem('parent_active_student_id', validChild.id)

    const [invRes, detailRes] = await Promise.all([
      parentApi.getChildInvoices(validChild.id).catch(() => ({ data: [] })),
      parentApi.getChildDetail(validChild.id).catch(() => ({ data: null }))
    ])

    invoices.value = invRes?.data || []
    childDetail.value = detailRes?.data || null
  } catch (err) {
    error.value = err.message || 'Không thể tải dữ liệu hóa đơn.'
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

function openInvoice(invoice) {
  selectedInvoice.value = invoice
  isInvoiceModalOpen.value = true
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

function formatStampDate(dateStr) {
  if (!dateStr) return '2026-07-30 01:56:41'
  const d = new Date(dateStr)
  if (isNaN(d.getTime())) return dateStr
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  const hh = String(d.getHours()).padStart(2, '0')
  const min = String(d.getMinutes()).padStart(2, '0')
  const ss = String(d.getSeconds()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd} ${hh}:${min}:${ss}`
}

function isInvoicePaid(status) {
  if (!status) return false
  const s = String(status).toLowerCase()
  return s === 'da_thanh_toan' || s === 'đã nộp' || s === 'da_nop' || s === 'paid'
}

function printInvoice() {
  if (selectedInvoice.value) {
    downloadInvoicePDF(selectedInvoice.value.id)
  } else {
    window.print()
  }
}

function downloadInvoicePDF(invoiceId) {
  const inv = invoices.value.find(i => i.id === invoiceId) || selectedInvoice.value
  if (!inv) return

  const info = currentChildInfo.value
  const formattedAmount = formatCurrency(inv.amount)
  const formattedDate = formatDate(inv.createdAt || inv.date || inv.dueDate)
  const isPaid = isInvoicePaid(inv.status)
  const statusText = isPaid ? 'ĐÃ THANH TOÁN' : 'CHƯA THANH TOÁN'
  const statusColor = isPaid ? '#16a34a' : '#dc2626'

  const scriptStart = '<script'
  const scriptEnd = '<' + '/script>'

  const html = `<!DOCTYPE html>
<html lang="vi">
<head>
  <meta charset="UTF-8">
  <title>HoaDon_HD${inv.id}_${info.name.replace(/\s+/g, '_')}</title>
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
    .info-val { font-weight: normal; }
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
        <div class="logo-sub">Cơ sở: ${info.campus}</div>
        <div class="logo-sub">Mã số thuế: 0102030405 | Hotline: 1900 1234</div>
      </div>
      <div class="invoice-title">
        <h2>HÓA ĐƠN ĐIỆN TỬ</h2>
        <p>Mã hóa đơn: <strong>HD#${inv.id}</strong></p>
        <p>Ngày phát hành: ${formattedDate}</p>
      </div>
    </div>

    <div class="info-box">
      <div class="info-row">
        <span class="info-label">Sinh viên thụ hưởng:</span>
        <span class="info-val"><strong>${info.name}</strong> (MSSV: ${info.studentId})</span>
      </div>
      <div class="info-row">
        <span class="info-label">Lớp hành chính:</span>
        <span class="info-val">${info.className}</span>
      </div>
      <div class="info-row">
        <span class="info-label">Chuyên ngành đào tạo:</span>
        <span class="info-val">${info.major}</span>
      </div>
      <div class="info-row">
        <span class="info-label">Mã giao dịch liên kết:</span>
        <span class="info-val">${inv.transactionCode || `GD-${inv.id}`}</span>
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
        <tr>
          <td style="text-align: center;">1</td>
          <td>Học phí đợt đóng kỳ 1 (Hệ thống quản lý đào tạo LMS)</td>
          <td class="text-right font-weight: bold;">${formattedAmount}</td>
        </tr>
        <tr>
          <td colspan="2" class="text-right" style="font-weight: bold;">Thuế giá trị gia tăng (VAT 0%):</td>
          <td class="text-right">0 ₫</td>
        </tr>
        <tr class="total-row">
          <td colspan="2" class="text-right">TỔNG CỘNG TIỀN THANH TOÁN:</td>
          <td class="text-right">${formattedAmount}</td>
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
        <div style="font-size: 7.5pt; color: #ef4444; word-break: break-all; font-weight: 600;">${inv.createdAt || inv.date || '2026-07-30T01:56:41.2166667'}</div>
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
      var filename = 'HoaDon_HD${inv.id}_${info.name.replace(/\s+/g, '_')}.pdf';
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
    popupStore.success('Đang tải hóa đơn PDF', `Hóa đơn HD#${inv.id} đang được kết xuất và tải về dưới dạng file PDF.`)
  } else {
    popupStore.warning('Cảnh báo Popup', 'Vui lòng cho phép mở cửa sổ bật lên (popup) để tải file PDF hóa đơn.')
  }
}

function goBack() {
  router.push('/parent/finance/tuition')
}
</script>

<template>
  <div class="space-y-6 print-container" id="invoices-view-page">
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 print:hidden">
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
            <FileText :size="20" class="text-orange-600" />
            Hóa đơn điện tử
          </h2>
          <p class="text-xs text-body">Tra cứu và tải xuống các hóa đơn tài chính chính thức đã phát hành</p>
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
      <SkeletonTable :rows="5" :columns="4" />
    </div>

    <!-- ── ERROR ── -->
    <div v-else-if="error" class="lg-card-glass p-8 text-center print:hidden">
      <p class="text-sm font-bold text-heading mb-1">Đã xảy ra lỗi</p>
      <p class="text-xs text-muted">{{ error }}</p>
      <button @click="loadData" class="mt-4 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Thử lại
      </button>
    </div>

    <!-- ── DANH SÁCH HÓA ĐƠN ── -->
    <div v-else class="lg-card-glass p-5 space-y-4 print:hidden">
      <div class="flex items-center justify-between pb-3 border-b border-card">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
          Hóa đơn học phí đã phát hành
        </h3>
        <span class="text-[10px] text-muted font-semibold">Tự động xuất sau khi giao dịch thành công</span>
      </div>

      <div v-if="invoices.length === 0" class="text-center py-12 text-muted text-xs">
        Học sinh hiện tại chưa có hóa đơn điện tử nào được phát hành.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse min-w-[750px]">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-3 px-3">Mã hóa đơn</th>
              <th class="py-3 px-3">Khoản thu / Kỳ học</th>
              <th class="py-3 px-3">Mã giao dịch</th>
              <th class="py-3 px-3">Ngày phát hành</th>
              <th class="py-3 px-3">Hạn thanh toán</th>
              <th class="py-3 px-3 text-right">Tổng tiền</th>
              <th class="py-3 px-3 text-center">Trạng thái</th>
              <th class="py-3 px-3 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-card)">
            <tr
              v-for="inv in invoices"
              :key="inv.id"
              class="hover:bg-(--surface-table-row-hover) transition"
            >
              <td class="py-3 px-3 font-bold text-heading font-mono">{{ inv.invoiceCode || ('HD#' + inv.id) }}</td>
              <td class="py-3 px-3 font-semibold text-heading">{{ inv.title || 'Học phí đợt đóng kỳ học' }}</td>
              <td class="py-3 px-3 text-muted font-mono text-[11px]">{{ inv.transactionCode ? ('GD-' + inv.transactionCode) : '—' }}</td>
              <td class="py-3 px-3 text-body">{{ formatDate(inv.createdAt || inv.date) }}</td>
              <td class="py-3 px-3 text-muted">{{ formatDate(inv.dueDate) }}</td>
              <td class="py-3 px-3 text-right font-extrabold text-heading">{{ formatCurrency(inv.amount) }}</td>
              <td class="py-3 px-3 text-center">
                <span
                  v-if="isInvoicePaid(inv.status)"
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-emerald-100 text-emerald-800 dark:bg-emerald-950/40 dark:text-emerald-300"
                >
                  <ShieldCheck :size="11" />
                  Đã thanh toán
                </span>
                <span
                  v-else
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-amber-100 text-amber-800 dark:bg-amber-950/40 dark:text-amber-300"
                >
                  Chưa thanh toán
                </span>
              </td>
              <td class="py-3 px-3 text-right">
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    @click="openInvoice(inv)"
                    class="p-1.5 border border-card rounded-lg hover:text-orange-600 hover:bg-orange-50 dark:hover:bg-orange-950/20 transition"
                    title="Xem chi tiết hóa đơn"
                  >
                    <Eye :size="13" />
                  </button>
                  <button
                    @click="downloadInvoicePDF(inv.id)"
                    class="p-1.5 border border-card rounded-lg hover:text-orange-600 hover:bg-orange-50 dark:hover:bg-orange-950/20 transition"
                    title="Tải hóa đơn PDF"
                  >
                    <Download :size="13" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ── HÓA ĐƠN ĐIỆN TỬ HIỂN THỊ KHI IN (PRINT CONTAINER) ── -->
    <div v-if="selectedInvoice" class="hidden print:block text-slate-800 bg-white p-8 border border-slate-300 max-w-[800px] mx-auto text-xs space-y-6">
      
      <!-- Invoice Title Header -->
      <div class="flex justify-between items-start border-b border-slate-300 pb-4">
        <div class="space-y-1">
          <h1 class="text-base font-extrabold text-slate-900">TRƯỜNG ĐẠI HỌC LMS ACADEMIC</h1>
          <p class="text-[10px] text-slate-500">Mã số thuế: 0102030405</p>
          <p class="text-[10px] text-slate-500">Địa chỉ: Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội</p>
        </div>
        <div class="text-right">
          <h2 class="text-sm font-extrabold text-slate-900">HÓA ĐƠN GIÁ TRỊ GIA TĂNG</h2>
          <p class="text-[10px] text-slate-500">Ký hiệu: 1C26LMS</p>
          <p class="text-[10px] text-slate-500">Mã hóa đơn: <strong class="text-slate-800">{{ selectedInvoice.id }}</strong></p>
          <p class="text-[10px] text-slate-500">Ngày xuất: {{ selectedInvoice.createdAt || selectedInvoice.date }}</p>
        </div>
      </div>

      <!-- Purchaser info -->
      <div class="space-y-1 border-b border-slate-300 pb-3">
        <p><strong>Đơn vị mua hàng:</strong> Phạm Thị Mẹ Học Sinh (Phụ huynh học sinh)</p>
        <p><strong>Học sinh thụ hưởng:</strong> {{ currentChild?.name }} (Mã số: {{ currentChild?.studentId }})</p>
        <p><strong>Lớp học phần:</strong> {{ currentChild?.class }} - Chuyên ngành: {{ currentChild?.major }}</p>
        <p><strong>Hình thức thanh toán:</strong> Chuyển khoản ngân hàng</p>
      </div>

      <!-- Fees details -->
      <table class="w-full text-left border-collapse border border-slate-300">
        <thead>
          <tr class="bg-slate-100 text-slate-950 font-bold border-b border-slate-300">
            <th class="p-2 border-r border-slate-300">STT</th>
            <th class="p-2 border-r border-slate-300">Tên dịch vụ / Khoản thu</th>
            <th class="p-2 border-r border-slate-300 text-right">Đơn giá (VND)</th>
            <th class="p-2 text-right">Thành tiền (VND)</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td class="p-2 border-r border-b border-slate-300">1</td>
            <td class="p-2 border-r border-b border-slate-300">Học phí đợt đóng kỳ 1 (Liên kết mã {{ selectedInvoice.transactionCode || selectedInvoice.id }})</td>
            <td class="p-2 border-r border-b border-slate-300 text-right">{{ formatCurrency(selectedInvoice.amount) }}</td>
            <td class="p-2 border-b border-slate-300 text-right font-bold">{{ formatCurrency(selectedInvoice.amount) }}</td>
          </tr>
          <!-- Summary rows -->
          <tr class="font-bold">
            <td colspan="3" class="p-2 border-r border-slate-300 text-right">Cộng tiền dịch vụ:</td>
            <td class="p-2 text-right">{{ formatCurrency(selectedInvoice.amount) }}</td>
          </tr>
          <tr class="font-bold">
            <td colspan="3" class="p-2 border-r border-slate-300 text-right">Thuế giá trị gia tăng (VAT):</td>
            <td class="p-2 text-right">0% (Miễn thuế học phí)</td>
          </tr>
          <tr class="font-bold text-slate-950 bg-slate-50">
            <td colspan="3" class="p-2 border-r border-slate-300 text-right">TỔNG CỘNG TIỀN THANH TOÁN:</td>
            <td class="p-2 text-right text-sm font-extrabold">{{ formatCurrency(selectedInvoice.amount) }}</td>
          </tr>
        </tbody>
      </table>

      <!-- Signatures -->
      <div class="grid grid-cols-2 text-center pt-8">
        <div>
          <p class="font-bold text-slate-700">Người mua hàng</p>
          <p class="text-[10px] text-slate-400 italic mt-1">(Ký, ghi rõ họ tên)</p>
        </div>
        <div class="space-y-1 relative">
          <p class="font-bold text-slate-700">Người bán hàng (Nhà trường)</p>
          <p class="text-[10px] text-slate-400 italic">(Ký, đóng dấu điện tử)</p>
          
          <!-- Electronic Signature Badge -->
          <div class="border-2 border-red-500 text-red-500 rounded p-1 mx-auto max-w-[150px] font-bold text-[9px] mt-4 transform rotate-2">
            <p>ĐÃ KÝ ĐIỆN TỬ</p>
            <p>LMS ACADEMIC SYSTEM</p>
            <p>{{ selectedInvoice.createdAt || selectedInvoice.date }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ── MODAL XEM CHI TIẾT HÓA ĐƠN ĐIỆN TỬ ── -->
    <div v-if="isInvoiceModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 print:hidden">
      <!-- Overlay -->
      <div @click="isInvoiceModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm"></div>

      <!-- Modal Content -->
      <div class="lg-modal w-full max-w-2xl relative z-10 flex flex-col rounded-2xl shadow-xl overflow-hidden max-h-[90vh]">
        
        <!-- Header -->
        <div class="flex items-center justify-between pb-3 border-b border-card">
          <h2 class="text-sm font-bold text-heading flex items-center gap-2">
            <FileText :size="16" class="text-orange-600" />
            Xem trước Hóa đơn điện tử
          </h2>
          <button @click="isInvoiceModalOpen = false" class="text-muted hover:text-orange-600">
            <X :size="16" />
          </button>
        </div>

        <!-- Invoice Body Scrollable -->
        <div class="flex-1 overflow-y-auto py-4 space-y-4 pr-1">
          
          <div class="p-6 surface-elevated rounded-xl border border-card text-[11px] text-body space-y-5">
            
            <!-- Title Header -->
            <div class="flex justify-between items-start border-b border-card pb-4">
              <div class="space-y-1">
                <h3 class="text-xs font-extrabold text-heading">TRƯỜNG ĐẠI HỌC LMS ACADEMIC</h3>
                <p class="text-[10px] text-muted">Mã số thuế: 0102030405</p>
                <p class="text-[10px] text-muted">Cơ sở: {{ currentChildInfo.campus }}</p>
              </div>
              <div class="text-right">
                <h3 class="text-xs font-extrabold text-heading">HÓA ĐƠN ĐIỆN TỬ</h3>
                <p class="text-[10px] text-muted">Mã hóa đơn: <strong>{{ selectedInvoice.invoiceCode || ('HD#' + selectedInvoice.id) }}</strong></p>
                <p class="text-[10px] text-muted">Ngày xuất: {{ formatDate(selectedInvoice.createdAt || selectedInvoice.date) }}</p>
              </div>
            </div>

            <!-- Buyer Details -->
            <div class="space-y-1.5 surface-input p-3 rounded-lg border border-card text-xs">
              <p><strong>Người thanh toán:</strong> Phụ huynh sinh viên</p>
              <p><strong>Sinh viên thụ hưởng:</strong> {{ currentChildInfo.name }} (MSSV: {{ currentChildInfo.studentId }})</p>
              <p><strong>Lớp hành chính:</strong> {{ currentChildInfo.className }}</p>
              <p><strong>Chuyên ngành đào tạo:</strong> {{ currentChildInfo.major }}</p>
              <p><strong>Mã giao dịch liên kết:</strong> {{ selectedInvoice.transactionCode ? ('GD-' + selectedInvoice.transactionCode) : ('GD-' + selectedInvoice.id) }}</p>
            </div>

            <!-- Table of Fees -->
            <table class="w-full text-left border-collapse border border-card">
              <thead>
                <tr class="surface-input font-bold border-b border-card">
                  <th class="p-2 border-r border-card">STT</th>
                  <th class="p-2 border-r border-card">Tên khoản thu</th>
                  <th class="p-2 text-right">Tổng phí (VND)</th>
                </tr>
              </thead>
              <tbody>
                <tr class="border-b border-card">
                  <td class="p-2 border-r border-card text-center">1</td>
                  <td class="p-2 border-r border-card">{{ selectedInvoice.title || 'Học phí đợt đóng kỳ 1 (Hệ thống quản lý đào tạo LMS)' }}</td>
                  <td class="p-2 text-right font-semibold">{{ formatCurrency(selectedInvoice.amount) }}</td>
                </tr>
                <tr class="font-bold border-t border-card">
                  <td colspan="2" class="p-2 border-r border-card text-right">Thuế giá trị gia tăng (VAT 0%):</td>
                  <td class="p-2 text-right">0 ₫</td>
                </tr>
                <tr class="font-extrabold text-orange-600 bg-orange-50/10 border-t-2 border-card">
                  <td colspan="2" class="p-2 border-r border-card text-right">TỔNG CỘNG TIỀN THANH TOÁN:</td>
                  <td class="p-2 text-right">{{ formatCurrency(selectedInvoice.amount) }}</td>
                </tr>
              </tbody>
            </table>

            <!-- Stamp Signature simulation -->
            <div class="flex justify-between items-end pt-4">
              <span class="text-[9.5px] text-muted italic font-medium flex items-center gap-1">
                <CheckCircle2 :size="12" class="text-emerald-500" />
                Chứng từ hóa đơn điện tử gốc được mã hóa và lưu trữ an toàn trên CSDL LMS System.
              </span>

              <!-- Stamp visual matching user's requested red seal image -->
              <div
                class="border-[1.5px] border-red-500 text-red-500 rounded-lg p-2 text-center w-38 font-bold tracking-tight transform -rotate-1 bg-red-50/40 dark:bg-red-950/20 shadow-xs leading-tight"
              >
                <p class="text-[10.5px] font-black uppercase text-red-500">ĐÃ KÝ ĐIỆN TỬ</p>
                <p class="text-[9px] font-bold text-red-500 mt-0.5">LMS UNIVERSITY</p>
                <p class="text-[7.5px] font-semibold text-red-500 mt-0.5 break-all opacity-95">{{ selectedInvoice.createdAt || selectedInvoice.date }}</p>
              </div>
            </div>

          </div>

        </div>

        <!-- Footer actions inside modal -->
        <div class="flex justify-end gap-2 pt-3 border-t border-card mt-3">
          <button
            @click="isInvoiceModalOpen = false"
            class="px-4 py-2 border border-card text-xs font-semibold rounded-xl text-label hover:bg-(--surface-card-hover) transition"
          >
            Đóng lại
          </button>
          <button
            @click="printInvoice"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl flex items-center gap-1.5 font-bold text-xs"
          >
            <Printer :size="13" /> In hóa đơn
          </button>
        </div>

      </div>
    </div>

  </div>
</template>

<style>
@media print {
  /* Hide all dashboard chrome while printing invoice */
  .print\:hidden,
  .surface-sidebar,
  .surface-topbar,
  .layout-sidebar,
  .layout-topbar,
  #grades-view-page,
  .fixed {
    display: none !important;
  }
  body, html {
    background: white !important;
    color: black !important;
  }
  .print-container {
    display: block !important;
    position: absolute;
    left: 0;
    top: 0;
    width: 100%;
    margin: 0;
    padding: 0;
    color: #000 !important;
  }
  .print-container * {
    color: #000 !important;
    font-weight: 600 !important;
  }
  .print-container strong,
  .print-container h1,
  .print-container h2,
  .print-container th {
    font-weight: 900 !important;
  }
  .print-container .border-slate-300 {
    border-color: #000 !important;
  }
}
</style>
