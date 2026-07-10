<script setup>
import { onMounted, ref } from 'vue'
import { AlertCircle, BarChart3, Loader2, RefreshCw } from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'

const loading = ref(false)
const error = ref('')
const stats = ref(null)

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const response = await bghApi.getAcademicOverview()
    stats.value = response?.data || response?.Data || response
  } catch (err) {
    error.value = err?.message || 'Không tải được báo cáo tổng quan đào tạo'
    stats.value = null
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
          <h2 class="text-lg font-bold text-heading">Tổng quan đào tạo</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">Báo cáo phân tích chất lượng đào tạo toàn trường từ BGH.</p>
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

      <div v-else-if="loading && !stats" class="mt-8 flex flex-col items-center justify-center py-10">
        <Loader2 class="h-8 w-8 animate-spin text-muted" />
        <p class="mt-4 text-sm font-medium text-muted">Đang tải báo cáo đào tạo...</p>
      </div>

      <div v-else-if="!stats" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 dark:bg-slate-800">
          <BarChart3 class="h-6 w-6 text-slate-500" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Không có dữ liệu</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Chưa có số liệu tổng quan đào tạo để hiển thị.</p>
      </div>

      <div v-else class="mt-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <!-- Dashboard Stats Cards -->
        <div class="rounded-xl border border-card bg-slate-50 dark:bg-slate-800/50 p-4">
          <p class="text-xs font-bold text-muted uppercase">Sinh viên đang học</p>
          <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalActiveStudents || 0 }}</div>
        </div>
        <div class="rounded-xl border border-card bg-slate-50 dark:bg-slate-800/50 p-4">
          <p class="text-xs font-bold text-muted uppercase">Lớp học mở</p>
          <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalActiveClasses || 0 }}</div>
        </div>
        <div class="rounded-xl border border-card bg-slate-50 dark:bg-slate-800/50 p-4">
          <p class="text-xs font-bold text-muted uppercase">Tỷ lệ qua môn trung bình</p>
          <div class="mt-2 text-2xl font-bold text-green-600 dark:text-green-400">{{ stats.averagePassRate || 0 }}%</div>
        </div>
        <div class="rounded-xl border border-card bg-slate-50 dark:bg-slate-800/50 p-4">
          <p class="text-xs font-bold text-muted uppercase">Giảng viên tham gia giảng dạy</p>
          <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalActiveTeachers || 0 }}</div>
        </div>
        <div class="rounded-xl border border-card bg-slate-50 dark:bg-slate-800/50 p-4">
          <p class="text-xs font-bold text-muted uppercase">Sinh viên nguy cơ (Cảnh báo)</p>
          <div class="mt-2 text-2xl font-bold text-red-600 dark:text-red-400">{{ stats.totalAtRiskStudents || 0 }}</div>
        </div>
      </div>
    </div>
  </div>
</template>
