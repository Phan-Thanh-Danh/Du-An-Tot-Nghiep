<script setup>
import { onMounted, ref } from 'vue'
import { AlertCircle, Loader2, RefreshCw, Server, Settings2 } from 'lucide-vue-next'
import { superAdminApi } from '@/services/superAdminApi'

const loading = ref(false)
const error = ref('')
const modules = ref([])

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await superAdminApi.getSystemModules()
    modules.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được dữ liệu module hệ thống'
    modules.value = []
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
          <h2 class="text-lg font-bold text-heading">Module hệ thống</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">Quản lý trạng thái các module chức năng trên toàn hệ thống.</p>
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

      <div v-else-if="loading && modules.length === 0" class="mt-8 flex flex-col items-center justify-center py-10">
        <Loader2 class="h-8 w-8 animate-spin text-muted" />
        <p class="mt-4 text-sm font-medium text-muted">Đang tải danh sách module...</p>
      </div>

      <div v-else-if="modules.length === 0" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-blue-50 dark:bg-blue-900/20">
          <Server class="h-6 w-6 text-blue-600 dark:text-blue-400" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Không có dữ liệu</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Chưa có module nào được cấu hình trong hệ thống.</p>
      </div>

      <div v-else class="mt-6 overflow-hidden rounded-xl border border-card shadow-sm">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
            <tr>
              <th scope="col" class="px-4 py-3 border-b border-card">Module</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Mô tả</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Danh mục</th>
              <th scope="col" class="px-4 py-3 border-b border-card text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-for="mod in modules" :key="mod.id" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                  <Settings2 class="h-4 w-4 text-slate-400" />
                  <span class="font-bold text-heading">{{ mod.name }}</span>
                </div>
                <div class="mt-1 font-mono text-[10px] text-muted">{{ mod.id }}</div>
              </td>
              <td class="px-4 py-3 text-muted max-w-xs truncate" :title="mod.description">{{ mod.description || '-' }}</td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center rounded-full bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-600 dark:bg-slate-800 dark:text-slate-400">
                  {{ mod.category || 'General' }}
                </span>
              </td>
              <td class="px-4 py-3 text-right">
                <span :class="[
                  'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold',
                  mod.status === 'Enabled' || mod.status === 'Hoạt động' ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' :
                  mod.status === 'Partial' ? 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400' :
                  'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-400'
                ]">
                  {{ mod.status }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
