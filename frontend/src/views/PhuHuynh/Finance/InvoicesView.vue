<script setup>
import { ref, computed } from 'vue'
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
import { childrenData, getActiveChild, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'
import { usePopupStore } from '@/stores/popup'

const route = useRoute()
const router = useRouter()
const popupStore = usePopupStore()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

// State Modal xem hóa đơn chi tiết
const isInvoiceModalOpen = ref(false)
const selectedInvoice = ref(null)

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

function openInvoice(invoice) {
  selectedInvoice.value = invoice
  isInvoiceModalOpen.value = true
}

function printInvoice() {
  window.print()
}

function downloadInvoicePDF(invoiceId) {
  popupStore.success('Tải xuống thành công', `Hóa đơn điện tử ${invoiceId} đã được xuất thành công dưới dạng PDF.`)
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

    <!-- ── DANH SÁCH HÓA ĐƠN ── -->
    <div class="lg-card-glass p-5 space-y-4 print:hidden">
      <div class="flex items-center justify-between pb-3 border-b border-card">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
          Hóa đơn học phí đã phát hành
        </h3>
        <span class="text-[10px] text-muted font-semibold">Tự động xuất sau khi giao dịch thành công</span>
      </div>

      <div v-if="currentChild.invoices.length === 0" class="text-center py-12 text-muted text-xs">
        Học sinh hiện tại chưa có hóa đơn điện tử nào được phát hành.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse min-w-[600px]">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-3 px-3">Mã hóa đơn</th>
              <th class="py-3 px-3">Mã giao dịch gốc</th>
              <th class="py-3 px-3">Ngày phát hành</th>
              <th class="py-3 px-3 text-right">Tổng tiền</th>
              <th class="py-3 px-3 text-center">Trạng thái</th>
              <th class="py-3 px-3 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr
              v-for="inv in currentChild.invoices"
              :key="inv.id"
              class="hover:bg-[var(--surface-table-row-hover)] transition"
            >
              <td class="py-3 px-3 font-semibold text-heading">{{ inv.id }}</td>
              <td class="py-3 px-3 text-muted">{{ inv.transactionCode }}</td>
              <td class="py-3 px-3 text-body">{{ inv.date }}</td>
              <td class="py-3 px-3 text-right font-extrabold text-heading">{{ formatCurrency(inv.amount) }}</td>
              <td class="py-3 px-3 text-center">
                <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[9px] font-bold bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400">
                  <ShieldCheck :size="11" />
                  {{ inv.status }}
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
          <p class="text-[10px] text-slate-500">Ngày xuất: {{ selectedInvoice.date }}</p>
        </div>
      </div>

      <!-- Purchaser info -->
      <div class="space-y-1 border-b border-slate-300 pb-3">
        <p><strong>Đơn vị mua hàng:</strong> Phạm Thị Mẹ Học Sinh (Phụ huynh học sinh)</p>
        <p><strong>Học sinh thụ hưởng:</strong> {{ currentChild.name }} (Mã số: {{ currentChild.studentId }})</p>
        <p><strong>Lớp học phần:</strong> {{ currentChild.class }} - Chuyên ngành: {{ currentChild.major }}</p>
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
            <td class="p-2 border-r border-b border-slate-300">Học phí đợt đóng kỳ 1 (Liên kết mã {{ selectedInvoice.transactionCode }})</td>
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
          
          <!-- Mock Electronic Signature Badge -->
          <div class="border-2 border-red-500 text-red-500 rounded p-1 mx-auto max-w-[150px] font-bold text-[9px] mt-4 transform rotate-2">
            <p>ĐÃ KÝ ĐIỆN TỬ</p>
            <p>LMS ACADEMIC SYSTEM</p>
            <p>{{ selectedInvoice.date }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- ── MODAL XEM CHI TIẾT HÓA ĐƠN ĐIỆN TỬ ── -->
    <div v-if="isInvoiceModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 print:hidden">
      <!-- Overlay -->
      <div @click="isInvoiceModalOpen = false" class="absolute inset-0 bg-slate-900/40 dark:bg-slate-950/60 backdrop-blur-sm" />

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
                <p class="text-[10px] text-muted">Địa chỉ: Khu Công nghệ cao Hòa Lạc, Thạch Thất, Hà Nội</p>
              </div>
              <div class="text-right">
                <h3 class="text-xs font-extrabold text-heading">HÓA ĐƠN GIÁ TRỊ GIA TĂNG</h3>
                <p class="text-[10px] text-muted">Mã hóa đơn: <strong>{{ selectedInvoice.id }}</strong></p>
                <p class="text-[10px] text-muted">Ngày xuất: {{ selectedInvoice.date }}</p>
              </div>
            </div>

            <!-- Buyer Details -->
            <div class="space-y-1 surface-input p-3 rounded-lg border border-card">
              <p><strong>Người thanh toán:</strong> Phạm Thị Mẹ Học Sinh (Phụ huynh)</p>
              <p><strong>Sinh viên thụ hưởng:</strong> {{ currentChild.name }} (Mã số: {{ currentChild.studentId }})</p>
              <p><strong>Lớp học phần:</strong> {{ currentChild.class }} - Ngành: {{ currentChild.major }}</p>
              <p><strong>Chứng từ liên kết:</strong> Giao dịch số {{ selectedInvoice.transactionCode }}</p>
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
                  <td class="p-2 border-r border-card">1</td>
                  <td class="p-2 border-r border-card">Học phí đợt đóng kỳ 1 (Bổ sung quỹ học tập LMS)</td>
                  <td class="p-2 text-right font-semibold">{{ formatCurrency(selectedInvoice.amount) }}</td>
                </tr>
                <tr class="font-bold border-t border-card">
                  <td colspan="2" class="p-2 border-r border-card text-right">Thuế giá trị gia tăng (VAT 0%):</td>
                  <td class="p-2 text-right">0đ</td>
                </tr>
                <tr class="font-extrabold text-orange-600 bg-orange-50/10 border-t-2 border-card">
                  <td colspan="2" class="p-2 border-r border-card text-right">TỔNG THANH TOÁN THỰC TẾ:</td>
                  <td class="p-2 text-right">{{ formatCurrency(selectedInvoice.amount) }}</td>
                </tr>
              </tbody>
            </table>

            <!-- Stamp Signature simulation -->
            <div class="flex justify-between items-center pt-4">
              <span class="text-[9px] text-muted italic font-medium flex items-center gap-1">
                <CheckCircle2 :size="12" class="text-emerald-500" />
                Hóa đơn gốc được mã hóa lưu trữ an toàn trên Blockchain LMS
              </span>

              <!-- Stamp visual -->
              <div class="border border-red-500 text-red-500 rounded p-1 text-[8px] font-bold text-center w-32 transform rotate-1">
                <p>ĐÃ KÝ ĐIỆN TỬ</p>
                <p>LMS UNIVERSITY</p>
                <p>{{ selectedInvoice.date }}</p>
              </div>
            </div>

          </div>

        </div>

        <!-- Footer actions inside modal -->
        <div class="flex justify-end gap-2 pt-3 border-t border-card mt-3">
          <button
            @click="isInvoiceModalOpen = false"
            class="px-4 py-2 border border-card text-xs font-semibold rounded-xl text-label hover:bg-[var(--surface-card-hover)] transition"
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
  }
}
</style>
