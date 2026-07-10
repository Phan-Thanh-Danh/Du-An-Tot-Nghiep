<script setup>
import { onMounted, ref } from 'vue'
import { AlertCircle, CalendarRange, Loader2, RefreshCw } from 'lucide-vue-next'
import { examApi } from '@/services/examApi'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'

const loading = ref(false)
const error = ref('')
const examPeriods = ref([])

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await examApi.getExamPeriods()
    examPeriods.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được cấu hình kỳ thi'
    examPeriods.value = []
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
          <h2 class="text-lg font-bold text-heading">Cấu hình kỳ thi</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">Quản lý danh sách các kỳ thi tập trung theo học kỳ.</p>
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

      <SkeletonTable v-else-if="loading && examPeriods.length === 0" :rows="5" :columns="4" class="mt-8" />

      <div v-else-if="examPeriods.length === 0" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-indigo-50 dark:bg-indigo-900/20">
          <CalendarRange class="h-6 w-6 text-indigo-600 dark:text-indigo-400" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Chưa có kỳ thi nào</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Hệ thống chưa có kỳ thi nào được tạo.</p>
      </div>

      <div v-else class="mt-6 overflow-hidden rounded-xl border border-card shadow-sm">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
            <tr>
              <th scope="col" class="px-4 py-3 border-b border-card">Tên kỳ thi</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Học kỳ</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Ngày tạo</th>
              <th scope="col" class="px-4 py-3 border-b border-card text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-for="kyThi in examPeriods" :key="kyThi.maKyThi" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
              <td class="px-4 py-3 font-medium text-heading">{{ kyThi.tenKyThi }}</td>
              <td class="px-4 py-3 text-muted">
                {{ kyThi.hocKy?.tenHocKy || `Học kỳ ${kyThi.maHocKy}` }}
              </td>
              <td class="px-4 py-3 text-muted">{{ new Date(kyThi.ngayTao).toLocaleDateString('vi-VN') }}</td>
              <td class="px-4 py-3 text-right">
                <span :class="[
                  'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold',
                  kyThi.trangThai === 'xuat_ban' || kyThi.trangThai === 'active' ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' :
                  kyThi.trangThai === 'dong' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' :
                  'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-400'
                ]">
                  {{ kyThi.trangThai }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
