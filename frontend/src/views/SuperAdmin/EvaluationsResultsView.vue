<script setup>
import { onMounted, ref } from 'vue'
import { AlertCircle, FileText, Loader2, RefreshCw } from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'

const loading = ref(false)
const error = ref('')
const evaluations = ref([])

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await bghApi.getEvaluations()
    evaluations.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được kết quả đánh giá'
    evaluations.value = []
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
          <h2 class="text-lg font-bold text-heading">Kết quả đánh giá</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">Tổng hợp báo cáo điểm đánh giá giáo viên từ Ban Giám Hiệu.</p>
        </div>
        <button
          class="inline-flex items-center gap-2 rounded-xl border border-default surface-card px-3 py-2 text-xs font-bold text-heading hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
          @click="loadData"
          :disabled="loading"
        >
          <Loader2 v-if="loading" class="h-4 w-4 animate-spin text-muted" />
          <RefreshCw v-else class="h-4 w-4 text-muted" />
          Tải lại
        </button>
      </div>

      <div v-if="error" class="mt-4 rounded-xl border border-red-200 bg-red-50 p-4 dark:border-red-900/50 dark:bg-red-900/20">
        <div class="flex items-start gap-3">
          <AlertCircle class="h-5 w-5 text-red-600 dark:text-red-400 mt-0.5" />
          <div>
            <h3 class="text-sm font-bold text-red-800 dark:text-red-200">Lỗi lấy dữ liệu</h3>
            <p class="mt-1 text-sm text-red-600 dark:text-red-300">{{ error }}</p>
          </div>
        </div>
      </div>

      <div v-else-if="loading && evaluations.length === 0" class="mt-8 flex flex-col items-center justify-center py-10">
        <Loader2 class="h-8 w-8 animate-spin text-muted" />
        <p class="mt-4 text-sm font-medium text-muted">Đang tải kết quả đánh giá...</p>
      </div>

      <div v-else-if="evaluations.length === 0" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 dark:bg-slate-800">
          <FileText class="h-6 w-6 text-slate-500" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Không có dữ liệu</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Chưa có kết quả đánh giá nào.</p>
      </div>

      <div v-else class="mt-6 overflow-hidden rounded-xl border border-card shadow-sm">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
            <tr>
              <th scope="col" class="px-4 py-3 border-b border-card">Mã HSYL</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Giáo viên</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Học kỳ</th>
              <th scope="col" class="px-4 py-3 border-b border-card text-right">Điểm</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-for="ev in evaluations" :key="ev.maDanhGia || ev.id" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
              <td class="px-4 py-3 font-medium text-heading font-mono text-xs">{{ ev.maDanhGia || ev.id }}</td>
              <td class="px-4 py-3">
                <div class="font-bold text-heading">{{ ev.tenGiaoVien || ev.giaoVienName }}</div>
              </td>
              <td class="px-4 py-3 text-muted">{{ ev.tenHocKy || ev.hocKy }}</td>
              <td class="px-4 py-3 text-right">
                <span class="inline-flex items-center rounded-full bg-blue-100 px-2 py-0.5 text-xs font-bold text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">
                  {{ ev.diemTong || ev.diem || '-' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
