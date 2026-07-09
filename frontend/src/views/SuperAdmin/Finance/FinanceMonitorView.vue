<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { AlertCircle, Loader2, RefreshCw, Wallet } from 'lucide-vue-next'
import { getTuitionConfigs } from '@/services/financeTuitionConfigService'

const route = useRoute()
const loading = ref(false)
const error = ref('')
const configs = ref([])

const mode = computed(() => route.meta.financeMode || 'finance')
const title = computed(() => route.meta.title || 'Tài chính')

const modeNote = computed(() => {
  switch (mode.value) {
    case 'student-debts':
      return 'Endpoint chuyên biệt cho công nợ sinh viên chưa có trong contract; màn này chỉ đọc cấu hình học phí thật để tránh placeholder.'
    case 'payments':
      return 'Endpoint theo dõi thanh toán admin chưa có trong contract; luồng thanh toán thật hiện nằm ở student tuition và PayOS webhook.'
    case 'refunds':
      return 'Endpoint hoàn phí/bảo lưu chưa có trong contract; màn này không dựng dữ liệu giả.'
    default:
      return 'Màn tài chính đọc dữ liệu API thật.'
  }
})

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await getTuitionConfigs({ pageNumber: 1, pageSize: 20 })
    configs.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được dữ liệu tài chính'
    configs.value = []
  } finally {
    loading.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
      <div class="flex items-start justify-between gap-4">
        <div>
          <h2 class="text-lg font-bold text-heading">{{ title }}</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">{{ modeNote }}</p>
        </div>
        <button
          class="inline-flex items-center gap-2 rounded-xl border border-default surface-card px-3 py-2 text-xs font-bold text-heading hover:bg-(--surface-input)"
          @click="loadData"
        >
          <RefreshCw :size="14" />
          Tải lại
        </button>
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center py-16 text-muted">
      <Loader2 class="mr-2 animate-spin" :size="20" />
      Đang tải dữ liệu tài chính...
    </div>

    <div v-else-if="error" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <AlertCircle class="text-(--color-danger-text)" :size="28" />
      <p class="text-sm font-semibold text-(--color-danger-text)">{{ error }}</p>
    </div>

    <div v-else-if="configs.length === 0" class="flex flex-col items-center gap-3 rounded-2xl border border-card surface-card py-16 text-center">
      <Wallet class="text-muted" :size="32" />
      <p class="text-sm font-semibold text-heading">Chưa có dữ liệu cấu hình học phí.</p>
      <p class="max-w-md text-xs text-muted">API trả rỗng nên màn hình hiển thị empty state thật, không dựng dữ liệu mẫu.</p>
    </div>

    <div v-else class="overflow-hidden rounded-2xl border border-card surface-card shadow-sm">
      <table class="w-full text-left text-sm">
        <thead class="bg-(--surface-input)">
          <tr>
            <th class="px-4 py-3 font-bold text-heading">Chương trình</th>
            <th class="px-4 py-3 font-bold text-heading">Học kỳ</th>
            <th class="px-4 py-3 font-bold text-heading">Học phí</th>
            <th class="px-4 py-3 font-bold text-heading">Học liệu</th>
            <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in configs" :key="item.id || item.maCauHinhHocPhi || item.maChuongTrinhDaoTao" class="border-t border-default">
            <td class="px-4 py-3 font-semibold text-heading">{{ item.tenChuongTrinhDaoTao || item.tenChuongTrinh || '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.tenHocKy || item.maHocKy || '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.soTienHocPhi ?? '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.tienHocLieu ?? '-' }}</td>
            <td class="px-4 py-3 text-muted">{{ item.conHoatDong === false ? 'Ngừng' : 'Hoạt động' }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
