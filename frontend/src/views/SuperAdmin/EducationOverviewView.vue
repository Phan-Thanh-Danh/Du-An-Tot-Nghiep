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

      <div v-else-if="loading && !stats" class="mt-8 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="n in 4" :key="n" class="rounded-xl border border-card surface-card p-4">
          <div class="h-3 w-20 skeleton-shimmer rounded mb-3"></div>
          <div class="h-6 w-16 skeleton-shimmer rounded"></div>
        </div>
      </div>

      <div v-else-if="!stats" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 dark:bg-slate-800">
          <BarChart3 class="h-6 w-6 text-slate-500" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Không có dữ liệu</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Chưa có số liệu tổng quan đào tạo để hiển thị.</p>
      </div>

      <div v-else class="mt-6 space-y-6">
        <!-- Dashboard Stats Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Sinh viên đang học</p>
            <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalStudents || 0 }}</div>
          </div>
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Giảng viên giảng dạy</p>
            <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalTeachers || 0 }}</div>
          </div>
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Lớp học mở</p>
            <div class="mt-2 text-2xl font-bold text-heading">{{ stats.totalClasses || 0 }}</div>
          </div>
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Khóa học (Môn)</p>
            <div class="mt-2 text-2xl font-bold text-heading">{{ stats.activeCourses || 0 }} / {{ stats.totalSubjects || 0 }}</div>
          </div>
          
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Tỷ lệ qua môn trung bình</p>
            <div class="mt-2 text-2xl font-bold text-green-600 dark:text-green-400">{{ stats.passRate || 0 }}%</div>
          </div>
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Điểm trung bình (GPA)</p>
            <div class="mt-2 text-2xl font-bold text-blue-600 dark:text-blue-400">{{ stats.avgGpa || 0 }}</div>
          </div>
          <div class="rounded-xl border border-card surface-card p-4">
            <p class="text-xs font-bold text-muted uppercase">Sinh viên nguy cơ (Cảnh báo)</p>
            <div class="mt-2 text-2xl font-bold text-red-600 dark:text-red-400">{{ stats.atRiskCount || 0 }}</div>
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Phân bố điểm -->
          <div class="rounded-xl border border-card surface-card p-5">
            <h3 class="text-sm font-bold text-heading mb-4">Phân bố điểm số (Toàn trường)</h3>
            <div class="space-y-4" v-if="stats.gradeDistribution?.length">
              <div v-for="item in stats.gradeDistribution" :key="item.grade" class="flex items-center gap-3">
                <div class="w-24 text-xs font-semibold text-muted truncate">{{ item.grade }}</div>
                <div class="flex-1 h-3 bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
                  <div class="h-full rounded-full" 
                       :class="{
                         'bg-green-500': item.grade.includes('A') || item.grade.includes('B'),
                         'bg-yellow-500': item.grade.includes('C'),
                         'bg-orange-500': item.grade.includes('D'),
                         'bg-red-500': item.grade.includes('F')
                       }"
                       :style="{ width: `${item.percent}%` }">
                  </div>
                </div>
                <div class="w-12 text-right text-xs font-bold text-heading">{{ item.percent }}%</div>
              </div>
            </div>
            <div v-else class="text-xs text-muted text-center py-4">Chưa có dữ liệu phân bố điểm</div>
          </div>

          <!-- Top môn học nguy cơ -->
          <div class="rounded-xl border border-card surface-card p-5">
            <h3 class="text-sm font-bold text-heading mb-4">Top môn học có tỉ lệ rớt cao</h3>
            <div class="space-y-4" v-if="stats.topSubjects?.length">
              <div v-for="(item, index) in stats.topSubjects.slice(0, 5)" :key="index" class="flex items-center gap-3">
                <div class="w-32 text-xs font-semibold text-muted truncate" :title="item.subjectName">{{ item.subjectName }}</div>
                <div class="flex-1 h-3 bg-slate-200 dark:bg-slate-700 rounded-full overflow-hidden">
                  <div class="h-full bg-red-500 rounded-full" :style="{ width: `${item.failRate}%` }"></div>
                </div>
                <div class="w-16 text-right text-xs font-bold text-red-600 dark:text-red-400">{{ item.failRate }}% rớt</div>
              </div>
            </div>
            <div v-else class="text-xs text-muted text-center py-4">Chưa có dữ liệu môn học</div>
          </div>
        </div>

        <!-- Xu hướng điểm GPA -->
        <div class="rounded-xl border border-card surface-card p-5">
          <h3 class="text-sm font-bold text-heading mb-6">Xu hướng điểm trung bình (GPA)</h3>
          <div class="flex items-end gap-2 md:gap-6 h-48" v-if="stats.semesterTrend?.length">
            <div v-for="item in stats.semesterTrend" :key="item.semester" class="flex-1 flex flex-col items-center gap-2 group">
              <div class="text-xs font-bold text-blue-600 dark:text-blue-400 opacity-0 group-hover:opacity-100 transition-opacity">{{ item.avgGpa }}</div>
              <div class="w-full max-w-[48px] bg-blue-100 dark:bg-blue-900/40 rounded-t-lg relative group-hover:bg-blue-200 dark:group-hover:bg-blue-800/60 transition-colors flex items-end justify-center"
                   :style="{ height: `${(item.avgGpa / 10) * 100}%`, minHeight: '4px' }">
                <div class="w-full bg-blue-500 rounded-t-lg" :style="{ height: '100%' }"></div>
              </div>
              <div class="text-[10px] sm:text-xs font-semibold text-muted truncate max-w-full px-1 text-center" :title="item.semester">{{ item.semester || 'N/A' }}</div>
            </div>
          </div>
          <div v-else class="text-xs text-muted text-center py-10 border border-dashed border-card rounded-lg">Chưa có dữ liệu xu hướng</div>
        </div>
      </div>
    </div>
  </div>
</template>
