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

const ENABLE_MOCK_API = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCK_API === 'true'
const loading = ref(false)
const error = ref(null)

const router = useRouter()
const semesterFilter = ref('Spring 2026')
const campusFilter = ref('')
const semesters = ['Spring 2026', 'Fall 2025', 'Summer 2025', 'Spring 2025']

const mockStats = [
  { id: 1, icon: ThumbsUp, value: '4.35 / 5', label: 'Điểm đánh giá trung bình', bg: 'bg-(--color-success-bg)', iconColor: 'text-(--color-success-text)' },
  { id: 2, icon: Users, value: '1,245', label: 'Số lượt khảo sát trong kỳ', bg: 'bg-(--color-info-bg)', iconColor: 'text-(--color-info-text)' },
  { id: 3, icon: Award, value: '12', label: 'GV đạt điểm ≥ 4.5', bg: 'bg-(--color-warning-bg)', iconColor: 'text-(--color-warning-text)' },
  { id: 4, icon: MessageSquare, value: '89%', label: 'Tỷ lệ SV tham gia khảo sát', bg: 'bg-(--color-info-bg)', iconColor: 'text-(--color-info-text)' },
]

const stats = ref(mockStats)

const mockTeacherRankings = [
  { id: 1, hoTen: 'TS. Nguyễn Khắc Anh', initials: 'NA', maCodeGv: 'GV001', khoa: 'Khoa CNTT', diemTb: 4.9, soLuot: 145, chatLuong: 4.9, phuongPhap: 4.8, dungGio: 5.0, xuHuong: 0.12 },
  { id: 2, hoTen: 'ThS. Trần Thị Bích', initials: 'TB', maCodeGv: 'GV015', khoa: 'Khoa Kinh Tế', diemTb: 4.8, soLuot: 210, chatLuong: 4.8, phuongPhap: 4.7, dungGio: 4.9, xuHuong: 0.08 },
  { id: 3, hoTen: 'ThS. Lê Văn Cường', initials: 'LC', maCodeGv: 'GV008', khoa: 'Khoa CNTT', diemTb: 4.7, soLuot: 98, chatLuong: 4.7, phuongPhap: 4.8, dungGio: 4.6, xuHuong: 0.15 },
  { id: 4, hoTen: 'TS. Phạm Thị Dung', initials: 'PD', maCodeGv: 'GV022', khoa: 'Khoa Thiết Kế', diemTb: 4.6, soLuot: 76, chatLuong: 4.5, phuongPhap: 4.7, dungGio: 4.6, xuHuong: -0.05 },
  { id: 5, hoTen: 'ThS. Hoàng Minh Đức', initials: 'HĐ', maCodeGv: 'GV005', khoa: 'Khoa CNTT', diemTb: 4.5, soLuot: 132, chatLuong: 4.4, phuongPhap: 4.6, dungGio: 4.5, xuHuong: 0.20 },
  { id: 6, hoTen: 'ThS. Nguyễn Thị Hoa', initials: 'NH', maCodeGv: 'GV018', khoa: 'Khoa Kinh Tế', diemTb: 4.4, soLuot: 88, chatLuong: 4.3, phuongPhap: 4.5, dungGio: 4.4, xuHuong: 0.03 },
  { id: 7, hoTen: 'TS. Võ Văn Hùng', initials: 'VH', maCodeGv: 'GV011', khoa: 'Khoa CNTT', diemTb: 4.3, soLuot: 115, chatLuong: 4.2, phuongPhap: 4.4, dungGio: 4.3, xuHuong: -0.10 },
  { id: 8, hoTen: 'ThS. Đặng Thị Kim', initials: 'ĐK', maCodeGv: 'GV025', khoa: 'Khoa Thiết Kế', diemTb: 4.2, soLuot: 65, chatLuong: 4.1, phuongPhap: 4.3, dungGio: 4.2, xuHuong: 0.07 },
  { id: 9, hoTen: 'ThS. Bùi Quang Linh', initials: 'BL', maCodeGv: 'GV030', khoa: 'Khoa Kinh Tế', diemTb: 3.9, soLuot: 92, chatLuong: 3.8, phuongPhap: 4.0, dungGio: 3.9, xuHuong: -0.08 },
  { id: 10, hoTen: 'ThS. Ngô Thị Mai', initials: 'NM', maCodeGv: 'GV033', khoa: 'Khoa CNTT', diemTb: 3.7, soLuot: 78, chatLuong: 3.6, phuongPhap: 3.8, dungGio: 3.7, xuHuong: -0.15 },
]

const teacherRankings = ref(mockTeacherRankings)

const mockDepartmentStats = [
  { ten: 'Khoa Công nghệ Thông tin', diemTb: 4.48, soGv: 38, soLuotKhaoSat: 568, xuHuong: 0.12 },
  { ten: 'Khoa Kinh Tế', diemTb: 4.27, soGv: 28, soLuotKhaoSat: 390, xuHuong: 0.03 },
  { ten: 'Khoa Thiết Kế', diemTb: 4.35, soGv: 15, soLuotKhaoSat: 141, xuHuong: 0.02 },
]

const deptStats = ref(mockDepartmentStats)

const mockComments = [
  { id: 1, giangVien: 'TS. Nguyễn Khắc Anh', initials: 'NA', monHoc: 'Lập trình Java', ngay: '10/06/2026', noiDung: 'Thầy giảng rất dễ hiểu, nhiều ví dụ thực tế. Em học được rất nhiều từ môn này!', rating: 5 },
  { id: 2, giangVien: 'ThS. Trần Thị Bích', initials: 'TB', monHoc: 'Kế toán tài chính', ngay: '08/06/2026', noiDung: 'Cô nhiệt tình, giải đáp mọi thắc mắc của sinh viên. Giáo trình rất hay.', rating: 5 },
  { id: 3, giangVien: 'ThS. Hoàng Minh Đức', initials: 'HĐ', monHoc: 'Cấu trúc dữ liệu', ngay: '05/06/2026', noiDung: 'Môn khó nhưng thầy giảng rất tận tâm, bài tập về nhà giúp hiểu sâu hơn.', rating: 4 },
  { id: 4, giangVien: 'ThS. Bùi Quang Linh', initials: 'BL', monHoc: 'Kinh tế vi mô', ngay: '02/06/2026', noiDung: 'Giảng viên có kiến thức nhưng phương pháp giảng dạy chưa thực sự cuốn hút.', rating: 3 },
]

const comments = ref(mockComments)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    if (!ENABLE_MOCK_API) {
      const res = await bghApi.getEvaluations()
      const data = unwrapApiData(res)
      if (data) {
        if (data.teacherRankings) teacherRankings.value = data.teacherRankings
        if (data.stats) stats.value = data.stats
        if (data.comments) comments.value = data.comments
        if (data.departmentStats) deptStats.value = data.departmentStats
      }
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
