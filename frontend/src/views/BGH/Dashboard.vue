<template>
  <div class="space-y-4 pb-10">
    
    <!-- Loading State -->
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="3" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <!-- ── Welcome Hero ── -->
    <div class="rounded-2xl surface-card border border-card p-4">
      <div class="flex flex-col md:flex-row items-center justify-between gap-4">
        <div class="max-w-xl text-center md:text-left">
          <h2 class="text-lg md:text-2xl font-semibold leading-tight tracking-tight text-heading">
            Tổng quan Ban giám hiệu
          </h2>
          <p class="mt-2 text-muted text-sm">
            {{ dashboardText }}
          </p>
          <div class="mt-4 flex flex-wrap justify-center md:justify-start gap-2">
            <router-link to="/bgh/schedule/pending" class="lg-button-primary rounded-lg px-3 py-2 text-xs font-bold transition-all active:scale-95">
              Duyệt TKB ngay
            </router-link>
            <router-link to="/bgh/academic/reports" class="lg-button-secondary rounded-lg px-3 py-2 text-xs font-bold transition-all">
              Báo cáo GPA
            </router-link>
          </div>
        </div>
        <div class="hidden lg:block">
          <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-(--color-info-bg)/70 border border-(--color-info-text)/20">
            <GraduationCap :size="22" class="text-(--color-info-text)/70" />
          </div>
        </div>
      </div>
    </div>

    <!-- ── Macro KPIs Grid ── -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="item in stats" :key="item.id" 
           class="group relative overflow-hidden rounded-2xl border border-card surface-card p-4 shadow-sm transition-all">
        <div class="flex items-center justify-between">
          <div :class="['flex h-10 w-10 items-center justify-center rounded-2xl transition-transform group-hover:scale-110', item.bgColor, item.iconColor]">
            <component :is="item.icon" :size="24" stroke-width="2.2" />
          </div>
          <div :class="['flex items-center gap-1 rounded-full px-2.5 py-1 text-[11px] font-bold', item.isNegative ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)']">
            {{ item.trend }}
            <ArrowUpRight v-if="!item.isNegative" :size="12" />
            <AlertCircle v-else :size="12" />
          </div>
        </div>
        <div class="mt-5">
          <p class="text-sm font-medium text-muted">{{ item.label }}</p>
          <p class="mt-1 text-xl font-semibold text-heading">{{ item.value }}</p>
        </div>
      </div>
    </div>

    <!-- ── Main Layout ── -->
    <div class="grid grid-cols-1 xl:grid-cols-3 gap-4">
      
      <!-- Left: Academic Performance & Teacher Evaluations -->
      <div class="xl:col-span-2 space-y-4">
        
        <!-- Teacher Evaluations Ranking -->
        <div v-if="topTeachers.length" class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden">
          <div class="flex items-center justify-between px-4 py-4">
            <div>
              <h2 class="text-lg font-bold text-heading">Ranking Giảng viên</h2>
              <p class="text-xs text-muted mt-0.5">Top giảng viên có điểm đánh giá cao nhất</p>
            </div>
            <router-link to="/bgh/evaluations/ranking" class="text-xs font-bold text-link hover:underline">Tất cả xếp hạng</router-link>
          </div>
          <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
             <div v-for="teacher in topTeachers" :key="teacher.id" 
                  class="group flex items-center gap-4 rounded-2xl border border-default p-4 transition-all hover:border-(--border-input-focus)">
               <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-bold shadow-sm">{{ teacher.initials }}</div>
               <div class="flex-1 min-w-0">
                 <h3 class="font-bold text-heading truncate">{{ teacher.name }}</h3>
                 <p class="text-xs text-muted">{{ teacher.department }}</p>
               </div>
               <div class="text-right">
                 <div class="flex items-center justify-end gap-1 text-sm font-semibold text-heading">
                    <Star class="w-3.5 h-3.5 text-(--color-warning-text)" fill="currentColor" /> {{ teacher.rating }}
                 </div>
                 <p class="text-[10px] text-muted">{{ teacher.reviews }} lượt</p>
               </div>
             </div>
          </div>
        </div>

        <!-- Trend Chart -->
        <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden p-4">
           <div class="flex items-center justify-between mb-4">
              <h2 class="text-base font-bold text-heading">Tỷ lệ Pass / Fail</h2>
              <select class="rounded-lg border border-input surface-input px-3 py-1.5 text-[10px] font-bold outline-none">
                 <option>Kỳ Spring 2026</option>
                 <option>Kỳ Fall 2025</option>
              </select>
           </div>
           <div class="flex items-end justify-between h-32 gap-4 pb-2">
              <div v-for="(h, i) in [45, 65, 80, 55, 90, 75, 88]" :key="i" class="flex-1 flex flex-col items-center gap-2">
                 <div class="w-full bg-(--lg-primary) rounded-t-xl transition-all hover:opacity-80" :style="{ height: h + '%' }" />
                 <span class="text-[10px] font-bold text-muted">{{ ['CNTT', 'KT', 'NN', 'DL', 'TK', 'YT', 'GD'][i] }}</span>
              </div>
           </div>
        </div>

      </div>

      <!-- Right: Pending Approvals & Risk Alerts -->
      <div class="space-y-4">
        
        <!-- Pending TKB Approvals -->
        <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
           <div class="mb-3 flex items-center justify-between">
              <h3 class="text-base font-bold text-heading">TKB Chờ Duyệt</h3>
              <span class="rounded-full bg-(--color-info-bg) px-2 py-0.5 text-[10px] font-bold text-(--color-info-text)">{{ pendingCount }}</span>
           </div>
           <div class="space-y-3">
             <div v-for="i in pendingSchedules" :key="i" 
                  class="p-3 rounded-xl border border-default surface-solid transition-all hover:bg-(--surface-input) cursor-pointer">
               <div class="flex justify-between items-start">
                  <p class="text-xs font-bold text-heading leading-tight">Bản thảo TKB #{{ i }}</p>
                  <span class="text-[9px] font-bold text-link">NEW</span>
               </div>
               <p class="mt-0.5 text-[10px] text-muted">Đang chờ phê duyệt</p>
               <button class="mt-2 w-full text-center text-[10px] font-bold text-link">Xem ngay →</button>
             </div>
           </div>
        </div>

        <!-- AI Risk Alerts -->
        <div class="rounded-2xl border border-(--color-danger-text)/20 bg-(--color-danger-bg) p-4 overflow-hidden relative">
          <div class="flex items-center gap-2">
             <h3 class="text-base font-bold text-heading">Cảnh báo rủi ro</h3>
          </div>
          <p class="text-xs text-body mt-1">AI phát hiện {{ riskCount }} sinh viên có rủi ro rớt môn cao do vắng học.</p>
          
          <div class="mt-4 space-y-3">
             <div v-for="sv in riskStudents" :key="sv.id" class="flex items-center justify-between border-b border-(--color-danger-text)/20 pb-2">
                <div>
                   <p class="text-xs font-bold text-heading">{{ sv.name }}</p>
                   <p class="text-[9px] text-muted">{{ sv.class }}</p>
                </div>
                <span class="text-[9px] font-bold bg-(--surface-card) text-(--color-danger-text) px-2 py-0.5 rounded-full">{{ sv.reason }}</span>
             </div>
          </div>
          <button class="mt-4 w-full text-center text-[10px] font-bold text-(--color-danger-text) hover:underline">Xem toàn bộ báo cáo rủi ro</button>
        </div>

        <!-- Strategy Announcements -->
        <div class="rounded-2xl border border-card surface-card shadow-sm p-4">
          <div class="mb-3 flex items-center justify-between">
            <h3 class="text-base font-bold text-heading">Thông báo</h3>
            <Bell :size="16" class="text-muted" />
          </div>
          <div class="space-y-3">
            <div class="flex gap-2">
              <div class="h-8 w-8 rounded-full bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) shrink-0">
                <ShieldCheck :size="14" />
              </div>
              <div>
                <p class="text-xs font-bold text-heading">Audit kết quả đào tạo 2025</p>
                <p class="text-[10px] text-muted mt-0.5">Phòng Thanh tra sẽ thực hiện kiểm tra vào tuần tới.</p>
              </div>
            </div>
          </div>
        </div>

      </div>

    </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'
import { 
  AlertCircle, BarChart2, PieChart, Star, AlertTriangle, GraduationCap, 
  TrendingUp, Clock, User, UserMinus, Sparkles, ArrowUpRight, Bell, ShieldCheck, Loader2
} from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)
const apiData = ref(null)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getDashboard()
    apiData.value = unwrapApiData(res)
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu tổng quan'
  } finally {
    loading.value = false
  }
}

const dashboardText = computed(() => {
  if (!apiData.value) return ''
  const d = apiData.value
  return `Tổng số ${d.totalStudents ?? 0} sinh viên, ${d.totalTeachers ?? 0} giáo viên, ${d.totalClasses ?? 0} lớp. Có ${d.pendingSchedules ?? 0} TKB đang chờ phê duyệt.`
})

const stats = computed(() => {
  if (!apiData.value) return []
  const d = apiData.value
  return [
    { id: 1, label: 'Tổng giáo viên', value: d.totalTeachers ?? '--', trend: 'Đang giảng dạy', isNegative: false, bgColor: 'bg-(--color-info-bg)', iconColor: 'text-(--color-info-text)', icon: User },
    { id: 2, label: 'Tổng sinh viên', value: d.totalStudents ?? '--', trend: 'Đang theo học', isNegative: false, bgColor: 'bg-(--color-success-bg)', iconColor: 'text-(--color-success-text)', icon: GraduationCap },
    { id: 3, label: 'Tổng lớp học', value: d.totalClasses ?? '--', trend: 'Đang hoạt động', isNegative: false, bgColor: 'bg-(--color-warning-bg)', iconColor: 'text-(--color-warning-text)', icon: BarChart2 },
    { id: 4, label: 'TKB chờ duyệt', value: d.pendingSchedules ?? 0, trend: d.pendingSchedules > 0 ? 'Cần xử lý' : 'Đã duyệt', isNegative: d.pendingSchedules > 0, bgColor: d.pendingSchedules > 0 ? 'bg-(--color-danger-bg)' : 'bg-(--color-success-bg)', iconColor: d.pendingSchedules > 0 ? 'text-(--color-danger-text)' : 'text-(--color-success-text)', icon: Clock },
  ]
})

const topTeachers = ref([])
const riskStudents = ref([])

const pendingSchedules = computed(() => apiData.value?.pendingSchedules ?? 0)
const pendingCount = computed(() => {
  const n = apiData.value?.pendingSchedules ?? 0
  return n > 0 ? `${n} Mới` : '0'
})
const riskCount = computed(() => apiData.value?.pendingRequests ?? 0)

onMounted(() => { loadData() })
</script>

<style scoped>
.transition-all {
  transition-duration: 300ms;
}
@keyframes pulse-soft {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(1.2); opacity: 0.3; }
}
.animate-pulse {
  animation: pulse-soft 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>
