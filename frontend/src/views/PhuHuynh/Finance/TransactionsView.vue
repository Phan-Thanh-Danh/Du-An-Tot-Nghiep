<script setup>
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  History,
  ChevronDown,
  CheckCircle,
  Clock,
  ArrowUpRight,
  ChevronLeft,
  Filter
} from 'lucide-vue-next'
import { childrenData, getActiveChild, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'

const route = useRoute()
const router = useRouter()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)
const filterStatus = ref('Tất cả')

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

// Danh sách giao dịch lọc theo trạng thái
const filteredTransactions = computed(() => {
  const list = currentChild.value.transactions || []
  if (filterStatus.value === 'Tất cả') return list
  return list.filter(t => t.status === filterStatus.value)
})

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
            <History :size="20" class="text-orange-600" />
            Lịch sử giao dịch đóng học phí
          </h2>
          <p class="text-xs text-body">Xem danh sách biên lai giao dịch thanh toán học phí đã thực hiện</p>
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

    <!-- ── DANH SÁCH GIAO DỊCH ── -->
    <div class="lg-card-glass p-5 space-y-4">
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 pb-3 border-b border-card">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
          Nhật ký đóng học phí của con
        </h3>
        
        <!-- Filter buttons -->
        <div class="flex items-center gap-1.5 flex-wrap">
          <Filter :size="12" class="text-muted mr-1" />
          <button
            v-for="status in ['Tất cả', 'Thành công', 'Đang xử lý']"
            :key="status"
            @click="filterStatus = status"
            class="px-2.5 py-1 text-[10px] rounded-lg font-semibold border transition"
            :class="filterStatus === status ? 'bg-orange-600 border-orange-600 text-white' : 'border-card text-label hover:text-orange-600'"
          >
            {{ status }}
          </button>
        </div>
      </div>

      <div v-if="filteredTransactions.length === 0" class="text-center py-12 text-muted text-xs">
        Không tìm thấy giao dịch nào phù hợp với bộ lọc.
      </div>
      <div v-else>
        <!-- MOBILE: Thẻ giao dịch (hiện trên điện thoại) -->
        <div class="sm:hidden space-y-3">
          <div
            v-for="trans in filteredTransactions"
            :key="'m-'+trans.code"
            class="p-3 rounded-xl border border-card space-y-2"
          >
            <div class="flex items-center justify-between">
              <span class="font-bold text-orange-600 dark:text-orange-400 text-xs flex items-center gap-1">
                <ArrowUpRight :size="12" /> {{ trans.code }}
              </span>
              <span
                class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold"
                :class="trans.status === 'Thành công' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' : 'bg-amber-100 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400'"
              >
                <component :is="trans.status === 'Thành công' ? CheckCircle : Clock" :size="10" />
                {{ trans.status }}
              </span>
            </div>
            <div class="flex items-end justify-between">
              <div>
                <p class="text-[10px] text-muted font-semibold">{{ trans.date }} · {{ trans.method }}</p>
              </div>
              <p class="text-sm font-extrabold text-heading">{{ formatCurrency(trans.amount) }}</p>
            </div>
          </div>
        </div>

        <!-- DESKTOP: Bảng truyền thống (ẩn trên điện thoại) -->
        <div class="hidden sm:block overflow-x-auto">
          <table class="w-full text-xs text-left border-collapse min-w-[600px]">
            <thead>
              <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
                <th class="py-3 px-3">Mã giao dịch</th>
                <th class="py-3 px-3">Ngày thanh toán</th>
                <th class="py-3 px-3 text-right">Số tiền nộp</th>
                <th class="py-3 px-3">Phương thức đóng</th>
                <th class="py-3 px-3 text-right">Trạng thái</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-[var(--border-card)]">
              <tr
                v-for="trans in filteredTransactions"
                :key="trans.code"
                class="hover:bg-[var(--surface-table-row-hover)] transition"
              >
                <td class="py-3 px-3 font-semibold text-orange-600 dark:text-orange-400">
                  <span class="flex items-center gap-1">
                    <ArrowUpRight :size="13" class="text-muted" />
                    {{ trans.code }}
                  </span>
                </td>
                <td class="py-3 px-3 text-body">{{ trans.date }}</td>
                <td class="py-3 px-3 text-right font-extrabold text-heading">{{ formatCurrency(trans.amount) }}</td>
                <td class="py-3 px-3 text-muted">{{ trans.method }}</td>
                <td class="py-3 px-3 text-right">
                  <span
                    class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold"
                    :class="
                      trans.status === 'Thành công' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' :
                      'bg-amber-100 text-amber-700 dark:bg-amber-950/30 dark:text-amber-400'
                    "
                  >
                    <component :is="trans.status === 'Thành công' ? CheckCircle : Clock" :size="11" />
                    {{ trans.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
