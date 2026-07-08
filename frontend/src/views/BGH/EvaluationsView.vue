<template>
  <div class="space-y-4">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin" />
        <p class="text-sm font-medium">Đang tải dữ liệu...</p>
      </div>
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
    <div class="flex items-center gap-3">
      <select v-model="semesterFilter" class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
        <option v-for="s in semesters" :key="s" :value="s">{{ s }}</option>
      </select>
      <select v-model="campusFilter" class="surface-input border border-input rounded-xl px-3 py-2 text-xs font-bold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
        <option value="">Tất cả cơ sở</option>
        <option>FPT Polytechnic Hồ Chí Minh</option>
        <option>FPT Polytechnic Đà Nẵng</option>
      </select>
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="stat in stats" :key="stat.id" class="surface-card border border-card rounded-2xl p-4 group hover:border-(--border-input-focus) transition-all">
        <div class="flex items-center justify-between">
          <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center transition-transform group-hover:scale-110 shadow-sm border border-default', stat.bg, stat.iconColor]">
            <component :is="stat.icon" :size="22" />
          </div>
        </div>
        <p class="mt-3 text-2xl font-bold text-heading">{{ stat.value }}</p>
        <p class="text-xs text-muted mt-0.5">{{ stat.label }}</p>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl overflow-hidden">
      <div class="p-4 flex items-center justify-between">
        <h2 class="text-base font-bold text-heading">Xếp hạng giảng viên</h2>
        <span class="text-xs text-muted">{{ filteredRankings.length }} giảng viên</span>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="bg-(--surface-card)">
            <tr>
              <th class="px-4 py-3 font-bold text-heading w-12">#</th>
              <th class="px-4 py-3 font-bold text-heading">Giảng viên</th>
              <th class="px-4 py-3 font-bold text-heading">Khoa</th>
              <th class="px-4 py-3 font-bold text-heading">Điểm TB</th>
              <th class="px-4 py-3 font-bold text-heading">Số lượt</th>
              <th class="px-4 py-3 font-bold text-heading">Chất lượng</th>
              <th class="px-4 py-3 font-bold text-heading">Phương pháp</th>
              <th class="px-4 py-3 font-bold text-heading">Đúng giờ</th>
              <th class="px-4 py-3 font-bold text-heading">Xu hướng</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(gv, i) in filteredRankings" :key="gv.id"
                class="hover:bg-(--surface-input)/50 transition-colors cursor-pointer"
                @click="viewTeacherDetail(gv)">
              <td class="px-4 py-3">
                <span :class="i < 3 ? 'text-indigo-500 font-bold' : 'text-muted'" class="text-sm">{{ i + 1 }}</span>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-2">
                  <div class="h-8 w-8 rounded-xl bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white text-xs font-bold">{{ gv.initials }}</div>
                  <div>
                    <p class="font-bold text-heading">{{ gv.hoTen }}</p>
                    <p class="text-[10px] text-muted">{{ gv.maCodeGv }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-3 text-xs">{{ gv.khoa }}</td>
              <td class="px-4 py-3">
                <span class="font-bold text-heading">{{ gv.diemTb }}</span>
                <div class="flex items-center gap-0.5 mt-0.5">
                  <Star v-for="s in 5" :key="s" :size="10" :class="s <= Math.round(gv.diemTb) ? 'text-amber-400' : 'text-default'" :fill="s <= Math.round(gv.diemTb) ? 'currentColor' : 'none'" />
                </div>
              </td>
              <td class="px-4 py-3 text-muted">{{ gv.soLuot }}</td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-1.5">
                  <div class="w-16 h-1.5 rounded-full bg-default overflow-hidden">
                    <div class="h-full rounded-full" :class="gv.chatLuong >= 4.5 ? 'bg-(--color-success-text)' : gv.chatLuong >= 4.0 ? 'bg-(--color-info-text)' : gv.chatLuong >= 3.5 ? 'bg-(--color-warning-text)' : 'bg-(--color-danger-text)'" :style="{ width: (gv.chatLuong / 5 * 100) + '%' }" />
                  </div>
                  <span class="text-xs font-bold text-heading">{{ gv.chatLuong.toFixed(1) }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-1.5">
                  <div class="w-16 h-1.5 rounded-full bg-default overflow-hidden">
                    <div class="h-full rounded-full" :class="gv.phuongPhap >= 4.5 ? 'bg-(--color-success-text)' : gv.phuongPhap >= 4.0 ? 'bg-(--color-info-text)' : 'bg-(--color-warning-text)'" :style="{ width: (gv.phuongPhap / 5 * 100) + '%' }" />
                  </div>
                  <span class="text-xs font-bold text-heading">{{ gv.phuongPhap.toFixed(1) }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-1.5">
                  <div class="w-16 h-1.5 rounded-full bg-default overflow-hidden">
                    <div class="h-full rounded-full bg-(--color-success-text)" :style="{ width: (gv.dungGio / 5 * 100) + '%' }" />
                  </div>
                  <span class="text-xs font-bold text-heading">{{ gv.dungGio.toFixed(1) }}</span>
                </div>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center gap-1 text-xs font-bold" :class="gv.xuHuong >= 0 ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'">
                  <component :is="gv.xuHuong >= 0 ? TrendingUp : TrendingDown" :size="14" />
                  {{ gv.xuHuong >= 0 ? '+' : '' }}{{ gv.xuHuong.toFixed(2) }}
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl p-5">
      <h2 class="text-base font-bold text-heading mb-4">Phân tích đánh giá theo khoa</h2>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div v-for="dept in departmentStats" :key="dept.ten" class="p-4 rounded-2xl border border-default">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-bold text-heading">{{ dept.ten }}</h3>
            <span class="text-xs font-bold text-link">{{ dept.soGv }} GV</span>
          </div>
          <div class="flex items-center gap-2">
            <div class="flex-1 h-2 rounded-full bg-default overflow-hidden">
              <div class="h-full rounded-full bg-gradient-to-r from-blue-800 to-blue-600" :style="{ width: (dept.diemTb / 5 * 100) + '%' }" />
            </div>
            <span class="text-sm font-bold text-heading shrink-0">{{ dept.diemTb.toFixed(1) }}</span>
          </div>
          <div class="mt-3 flex items-center justify-between text-[10px]">
            <span class="text-muted">SL khảo sát: {{ dept.soLuotKhaoSat }}</span>
            <span :class="dept.xuHuong >= 0 ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="font-bold">
              {{ dept.xuHuong >= 0 ? '↑' : '↓' }} {{ Math.abs(dept.xuHuong).toFixed(1) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl p-5">
      <h2 class="text-base font-bold text-heading mb-3">Bình luận nổi bật từ sinh viên</h2>
      <div>
        <div v-for="comment in comments" :key="comment.id" class="py-3 first:pt-0 last:pb-0">
          <div class="flex items-start gap-3">
            <div class="h-8 w-8 rounded-full bg-gradient-to-br from-blue-800 to-blue-600 flex items-center justify-center text-white text-xs font-bold shrink-0">{{ comment.initials }}</div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <span class="text-sm font-bold text-heading">{{ comment.giangVien }}</span>
                <span class="text-[10px] text-muted">{{ comment.monHoc }} · {{ comment.ngay }}</span>
              </div>
              <p class="text-xs text-body mt-1 italic">"{{ comment.noiDung }}"</p>
              <div class="flex items-center gap-0.5 mt-1">
                <Star v-for="s in 5" :key="s" :size="10" :class="s <= comment.rating ? 'text-amber-400' : 'text-default'" :fill="s <= comment.rating ? 'currentColor' : 'none'" />
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
import { Star, TrendingUp, TrendingDown, ThumbsUp, MessageSquare, Users, Award, AlertCircle, Loader2 } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const router = useRouter()
const semesterFilter = ref('Spring 2026')
const campusFilter = ref('')
const semesters = ['Spring 2026', 'Fall 2025', 'Summer 2025', 'Spring 2025']

const stats = ref([])
const teacherRankings = ref([])
const deptStats = ref([])
const comments = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getEvaluations()
    const data = unwrapApiData(res)
    if (data) {
      if (data.teacherRankings) teacherRankings.value = data.teacherRankings
      if (data.stats) stats.value = data.stats
      if (data.comments) comments.value = data.comments
      if (data.departmentStats) deptStats.value = data.departmentStats
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu đánh giá'
  } finally {
    loading.value = false
  }
}

const filteredRankings = computed(() => {
  if (!campusFilter.value) return teacherRankings.value
  return teacherRankings.value
})

function viewTeacherDetail(gv) {
  router.push(`/bgh/evaluations/detail/${gv.maCodeGv}`)
}

onMounted(() => { loadData() })
</script>
