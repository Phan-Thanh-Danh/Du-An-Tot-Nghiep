<script setup>
import { ref, computed, onMounted } from 'vue'
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
import SkeletonCard from '@/components/common/skeleton/SkeletonCard.vue'
import { parentApi } from '@/services/parentApi'

const route = useRoute()
const router = useRouter()

const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || null)
const dropdownOpen = ref(false)
const loading = ref(true)
const error = ref('')

const children = ref([])
const tuitionData = ref(null)

const currentChild = computed(() => {
  return children.value.find(c => c.id === activeChildId.value) || children.value[0] || null
})

const invoices = computed(() => tuitionData.value?.invoices || [])
const totalDue = computed(() => tuitionData.value?.totalDue || 0)

const totalTuition = computed(() => {
  return invoices.value.reduce((sum, inv) => sum + (inv.amount || 0), 0)
})

const paidTuition = computed(() => Math.max(0, totalTuition.value - totalDue.value))

const isOverdue = computed(() => {
  return invoices.value.some(inv => {
    if (inv.status === 'Đã nộp' || !inv.dueDate) return false
    const parts = String(inv.dueDate).split('/')
    if (parts.length !== 3) return false
    const due = new Date(+parts[2], +parts[1] - 1, +parts[0])
    return due < new Date()
  })
})

const deadlineTuition = computed(() => {
  const unpaid = invoices.value.filter(inv => inv.status !== 'Đã nộp' && inv.dueDate)
  if (unpaid.length === 0) return ''
  const dates = unpaid.map(inv => {
    const parts = String(inv.dueDate).split('/')
    return parts.length === 3 ? new Date(+parts[2], +parts[1] - 1, +parts[0]) : new Date(0)
  })
  const latest = new Date(Math.max(...dates))
  const d = latest.getDate().toString().padStart(2, '0')
  const m = (latest.getMonth() + 1).toString().padStart(2, '0')
  const y = latest.getFullYear()
  return `${d}/${m}/${y}`
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
  } catch (err) {
    error.value = err.message || 'Không thể tải dữ liệu học phí.'
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

function goToPayment() {
  if (activeChildId.value) {
    router.push({ path: '/parent/finance/payment', query: { studentId: activeChildId.value } })
  }
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
    <div v-if="loading" class="grid grid-cols-1 gap-4">
      <SkeletonCard v-for="n in 3" :key="n" />
    </div>

    <!-- ── ERROR ── -->
    <div v-else-if="error" class="lg-card-glass p-8 text-center">
      <AlertTriangle :size="36" class="text-red-500 mx-auto mb-3" />
      <p class="text-sm font-bold text-heading mb-1">Đã xảy ra lỗi</p>
      <p class="text-xs text-muted">{{ error }}</p>
      <button @click="loadData" class="mt-4 px-4 py-2 border border-card rounded-xl text-xs font-bold text-label hover:text-orange-600 transition">
        Thử lại
      </button>
    </div>

    <!-- ── EMPTY ── -->
    <div v-else-if="!tuitionData" class="lg-card-glass p-8 text-center">
      <Wallet :size="36" class="text-muted mx-auto mb-3" />
      <p class="text-sm font-bold text-heading mb-1">Chưa có dữ liệu học phí</p>
      <p class="text-xs text-muted">Thông tin học phí của học sinh chưa được cập nhật.</p>
    </div>

    <template v-else>
      <!-- ── CẢNH BÁO QUÁ HẠN HỌC PHÍ (RED ALERT BANNER) ── -->
      <div
        v-if="isOverdue"
        class="p-4 rounded-xl border border-red-200 dark:border-red-950/20 bg-red-50/50 dark:bg-red-950/10 flex gap-3 animate-pulse"
      >
        <AlertTriangle :size="20" class="text-red-500 flex-shrink-0 mt-0.5" />
        <div class="text-xs text-body space-y-1">
          <p class="font-extrabold text-red-600 dark:text-red-400">CẢNH BÁO: QUÁ HẠN THANH TOÁN HỌC PHÍ</p>
          <p class="text-slate-600 dark:text-slate-400 leading-relaxed font-semibold">
            Thời hạn nộp tiền học kỳ của con đã kết thúc vào ngày <strong>{{ deadlineTuition }}</strong>. Nhà trường kính đề nghị phụ huynh hoàn tất số dư nợ còn lại (<strong>{{ formatCurrency(totalDue) }}</strong>) sớm nhất để tránh ảnh hưởng đến kết quả thi học kỳ và hoạt động đăng ký môn học tiếp theo của học sinh.
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
            <span class="text-lg font-extrabold text-heading">{{ formatCurrency(totalTuition) }}</span>
            <span class="block text-[9px] text-muted font-bold mt-1">Tổng các hóa đơn học phí</span>
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
            <span class="text-lg font-extrabold text-emerald-600 dark:text-emerald-400">{{ formatCurrency(paidTuition) }}</span>
            <span class="block text-[9px] text-emerald-600 font-bold mt-1">Khớp lệnh giao dịch thực tế</span>
          </div>
        </div>

        <div class="lg-card-glass p-4 flex flex-col justify-between" :class="totalDue > 0 ? 'border-orange-200 dark:border-orange-950/20' : ''">
          <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
            <span class="text-[10px] text-muted font-bold uppercase">Công nợ còn lại (Nợ)</span>
            <span class="p-1.5 bg-orange-50 dark:bg-orange-950/20 text-orange-600 rounded-lg">
              <AlertTriangle :size="13" />
            </span>
          </div>
          <div>
            <span class="text-lg font-extrabold text-orange-600 dark:text-orange-400">{{ formatCurrency(totalDue) }}</span>
            <span class="block text-[9px] text-orange-600 font-bold mt-1" :class="isOverdue ? 'text-red-500 font-extrabold' : ''">
              {{ totalDue > 0 ? 'Chưa hoàn thành' : 'Đã hoàn thành học phí' }}
            </span>
          </div>
        </div>

        <div class="lg-card-glass p-4 flex flex-col justify-between" :class="isOverdue ? 'border-red-200 dark:border-red-950/20' : ''">
          <div class="flex items-center justify-between pb-2 border-b border-card mb-2">
            <span class="text-[10px] text-muted font-bold uppercase">Hạn chót thanh toán</span>
            <span class="p-1.5 rounded-lg" :class="isOverdue ? 'bg-red-50 dark:bg-red-950/20 text-red-600' : 'bg-slate-100 dark:bg-slate-800 text-muted'">
              <Clock :size="13" />
            </span>
          </div>
          <div>
            <span class="text-base font-extrabold text-heading" :class="isOverdue ? 'text-red-500' : ''">
              {{ deadlineTuition }}
            </span>
            <span class="block text-[9px] text-muted font-bold mt-1.5" :class="isOverdue ? 'text-red-500 font-extrabold' : ''">
              {{ isOverdue ? 'Đã quá hạn đóng phí!' : 'Trong thời hạn cho phép' }}
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
            v-if="totalDue > 0"
            @click="goToPayment"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white px-4 py-2 rounded-xl text-xs font-bold flex items-center gap-1.5 transition shadow-md self-start sm:self-center"
          >
            Thanh toán trực tuyến
            <ArrowRight :size="13" />
          </button>
        </div>

        <div v-if="invoices.length === 0" class="text-center py-8 text-muted text-xs">
          Không có hóa đơn nào.
        </div>

        <div v-else>
          <!-- MOBILE: Danh sách thẻ (hiện trên điện thoại) -->
          <div class="sm:hidden space-y-3">
          <div
            v-for="(inv, idx) in invoices"
            :key="'m-'+idx"
            class="flex items-center justify-between p-3 rounded-xl border border-card hover:bg-(--surface-card-hover) transition"
          >
            <div class="flex-1 min-w-0 pr-3">
              <p class="text-xs font-semibold text-heading leading-snug">{{ inv.id || 'Hóa đơn' }}</p>
              <p class="text-xs font-extrabold text-body mt-0.5">{{ formatCurrency(inv.amount) }}</p>
            </div>
            <span
              class="flex-shrink-0 inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold"
              :class="inv.status === 'Đã nộp' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' : 'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400'"
            >
              <component :is="inv.status === 'Đã nộp' ? CheckCircle : AlertTriangle" :size="11" />
              {{ inv.status }}
            </span>
          </div>
        </div>

        <!-- DESKTOP: Bảng dữ liệu truyền thống (ẩn trên điện thoại) -->
          <div class="hidden sm:block overflow-x-auto">
          <table class="w-full text-xs text-left border-collapse min-w-[600px]">
            <thead>
              <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
                <th class="py-3 px-3">Mã hóa đơn</th>
                <th class="py-3 px-3 text-right">Số tiền</th>
                <th class="py-3 px-3 text-right">Hạn thanh toán</th>
                <th class="py-3 px-3 text-right">Trạng thái</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-(--border-card)">
              <tr
                v-for="(inv, idx) in invoices"
                :key="idx"
                class="hover:bg-(--surface-table-row-hover) transition"
              >
                <td class="py-3 px-3 font-semibold text-heading">{{ inv.id || 'Hóa đơn' }}</td>
                <td class="py-3 px-3 text-right font-extrabold text-body">{{ formatCurrency(inv.amount) }}</td>
                <td class="py-3 px-3 text-right text-muted">{{ inv.dueDate || '—' }}</td>
                <td class="py-3 px-3 text-right">
                  <span
                    class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold"
                    :class="
                      inv.status === 'Đã nộp' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' :
                      'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400'
                    "
                  >
                    <component :is="inv.status === 'Đã nộp' ? CheckCircle : AlertTriangle" :size="11" />
                    {{ inv.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<style scoped>
</style>
