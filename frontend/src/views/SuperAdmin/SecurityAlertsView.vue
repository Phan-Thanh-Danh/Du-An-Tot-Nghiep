<script setup>
import { onMounted, ref } from 'vue'
import { AlertCircle, Loader2, RefreshCw, ShieldAlert } from 'lucide-vue-next'
import { superAdminApi } from '@/services/superAdminApi'

const loading = ref(false)
const error = ref('')
const alerts = ref([])

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await superAdminApi.getSecurityAlerts()
    alerts.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được dữ liệu cảnh báo bảo mật'
    alerts.value = []
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
          <h2 class="text-lg font-bold text-heading">Cảnh báo bảo mật</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">Màn hình xem các cảnh báo bảo mật từ API thật.</p>
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

      <div v-else-if="loading && alerts.length === 0" class="mt-8 flex flex-col items-center justify-center py-10">
        <Loader2 class="h-8 w-8 animate-spin text-muted" />
        <p class="mt-4 text-sm font-medium text-muted">Đang tải cảnh báo bảo mật...</p>
      </div>

      <div v-else-if="alerts.length === 0" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-green-50 dark:bg-green-900/20">
          <ShieldAlert class="h-6 w-6 text-green-600 dark:text-green-400" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Hệ thống an toàn</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Không có cảnh báo bảo mật nào được ghi nhận.</p>
      </div>

      <div v-else class="mt-6 overflow-hidden rounded-xl border border-card shadow-sm">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
            <tr>
              <th scope="col" class="px-4 py-3 border-b border-card">ID</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Ngày tạo</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Mức độ rủi ro</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Địa chỉ IP</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-for="alert in alerts" :key="alert.maCanhBao" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
              <td class="px-4 py-3 font-medium text-heading">#{{ alert.maCanhBao }}</td>
              <td class="px-4 py-3 text-muted">{{ new Date(alert.ngayTao).toLocaleString('vi-VN') }}</td>
              <td class="px-4 py-3">
                <span :class="[
                  'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold',
                  alert.diemRuiRo > 80 ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400' :
                  alert.diemRuiRo > 50 ? 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400' :
                  'bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400'
                ]">
                  {{ alert.diemRuiRo }}
                </span>
              </td>
              <td class="px-4 py-3 text-muted font-mono text-xs">{{ alert.diaChiIp || '-' }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-xs font-bold text-slate-700 dark:bg-slate-800 dark:text-slate-300">
                  {{ alert.trangThai }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
