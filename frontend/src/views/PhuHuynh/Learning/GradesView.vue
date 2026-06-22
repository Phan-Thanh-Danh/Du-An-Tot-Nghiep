<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Award,
  ChevronDown,
  Printer,
  TrendingUp,
  AlertCircle,
  FileText,
  CheckCircle,
  XCircle,
  ChevronLeft
} from 'lucide-vue-next'
import { childrenData, setActiveChildId } from '@/components/PhuHuynh/data/parentData.js'

const route = useRoute()
const router = useRouter()

// Lấy studentId từ query URL hoặc local storage, mặc định là 1
const activeChildId = ref(Number(route.query.studentId) || Number(localStorage.getItem('parent_active_student_id')) || 1)
const dropdownOpen = ref(false)

const currentChild = computed(() => {
  return childrenData.find(c => c.id === activeChildId.value) || childrenData[0]
})

// Bộ lọc
const selectedSemester = ref('')
const selectedSubjectCode = ref('all')

// Cập nhật Học kỳ mặc định khi đổi học sinh
onMounted(() => {
  if (currentChild.value.semesters.length > 0) {
    selectedSemester.value = currentChild.value.semesters[0]
  }
})

function selectChild(id) {
  activeChildId.value = id
  setActiveChildId(id)
  dropdownOpen.value = false
  // Cập nhật học kỳ mặc định của học sinh mới chọn
  if (currentChild.value.semesters.length > 0) {
    selectedSemester.value = currentChild.value.semesters[0]
  }
  selectedSubjectCode.value = 'all'
  router.replace({ query: { studentId: id } })
}

// Bảng điểm sau khi lọc
const filteredGrades = computed(() => {
  const allGrades = currentChild.value.grades[selectedSemester.value] || []
  // Chỉ hiển thị điểm đã được công bố (isPublished = true) theo Rule của đề bài
  const publishedGrades = allGrades.filter(g => g.isPublished)
  
  if (selectedSubjectCode.value === 'all') {
    return publishedGrades
  }
  return publishedGrades.filter(g => g.code === selectedSubjectCode.value)
})

// Đếm số môn chưa công bố để hiển thị thông báo tế nhị
const unpublishedCount = computed(() => {
  const allGrades = currentChild.value.grades[selectedSemester.value] || []
  return allGrades.filter(g => !g.isPublished).length
})

// Danh sách môn học trong học kỳ phục vụ bộ lọc môn học
const subjectsInSemester = computed(() => {
  const allGrades = currentChild.value.grades[selectedSemester.value] || []
  return allGrades.filter(g => g.isPublished).map(g => ({ code: g.code, name: g.name }))
})

// Biểu đồ xu hướng điểm số bằng SVG
const trendPoints = computed(() => {
  const trend = currentChild.value.gpaTrend || []
  if (trend.length === 0) return []
  const width = 450
  const height = 150
  const padding = 30
  
  const minGpa = 4.0 // Giới hạn dưới của đồ thị
  const maxGpa = 10.0 // Giới hạn trên của đồ thị

  return trend.map((point, index) => {
    const x = padding + (index * (width - 2 * padding)) / (trend.length - 1)
    const y = height - padding - ((point.gpa - minGpa) * (height - 2 * padding)) / (maxGpa - minGpa)
    return {
      x,
      y,
      gpa: point.gpa,
      month: point.month
    }
  })
})

const trendPath = computed(() => {
  const points = trendPoints.value
  if (points.length === 0) return ''
  return points.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`).join(' ')
})

const trendAreaPath = computed(() => {
  const points = trendPoints.value
  if (points.length === 0) return ''
  // const width = 450
  const height = 150
  const padding = 30
  const startX = points[0].x
  const endX = points[points.length - 1].x
  const bottomY = height - padding

  return `${trendPath.value} L ${endX} ${bottomY} L ${startX} ${bottomY} Z`
})

// Hàm in PDF (sử dụng in trình duyệt)
function exportPDF() {
  window.print()
}

function goBack() {
  router.push('/parent/dashboard')
}
</script>

<template>
  <div class="space-y-6 print-container" id="grades-view-page">
    
    <!-- ── THANH TIÊU ĐỀ & CHỌN HỌC SINH ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 print:hidden">
      <div class="flex items-center gap-2">
        <button
          @click="goBack"
          class="lg-icon-button flex h-8 w-8 text-muted hover:text-orange-600 border border-card surface-card rounded-lg"
          title="Quay lại"
        >
          <ChevronLeft :size="18" />
        </button>
        <div>
          <h2 class="text-lg font-bold text-heading flex items-center gap-2">
            <Award :size="20" class="text-orange-600" />
            Bảng điểm học tập
          </h2>
          <p class="text-xs text-body">Theo dõi chi tiết điểm chuyên cần, quiz, kiểm tra giữa kỳ và cuối kỳ của con</p>
        </div>
      </div>

      <!-- Chọn học sinh nhanh -->
      <div class="relative min-w-[220px]">
        <button
          type="button"
          class="surface-input border-card flex w-full items-center justify-between gap-2.5 rounded-xl border px-3.5 py-2 text-xs font-semibold text-heading shadow-sm transition-all focus:outline-none"
          @click="dropdownOpen = !dropdownOpen"
        >
          <div class="flex items-center gap-2">
            <div class="h-5 w-5 flex items-center justify-center rounded-full bg-orange-600 text-[9px] font-bold text-white">
              {{ currentChild.name.split(' ').pop().charAt(0) }}
            </div>
            <span>{{ currentChild.name }}</span>
          </div>
          <ChevronDown :size="14" class="text-muted transition-transform" :class="dropdownOpen ? 'rotate-180' : ''" />
        </button>

        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 translate-y-2 scale-95"
          enter-to-class="opacity-100 translate-y-0 scale-100"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0 scale-100"
          leave-to-class="opacity-0 translate-y-2 scale-95"
        >
          <div
            v-if="dropdownOpen"
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-[var(--lg-shadow-md)]"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-[var(--surface-card-hover)]"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- ── THÔNG TIN HỌC SINH (Hiển thị khi In) ── -->
    <div class="hidden print:block border-b border-slate-300 pb-4 mb-6">
      <div class="text-center space-y-1">
        <h1 class="text-xl font-bold text-slate-900">EduLMS - BÁO CÁO KẾT QUẢ HỌC TẬP</h1>
        <p class="text-xs text-slate-600">Ngày in: {{ new Date().toLocaleDateString('vi-VN') }}</p>
      </div>
      <div class="grid grid-cols-2 gap-4 mt-4 text-xs text-slate-800">
        <div>
          <p><strong>Học sinh:</strong> {{ currentChild.name }}</p>
          <p><strong>Mã số học sinh:</strong> {{ currentChild.studentId }}</p>
        </div>
        <div>
          <p><strong>Lớp học phần:</strong> {{ currentChild.class }}</p>
          <p><strong>Ngành học:</strong> {{ currentChild.major }}</p>
        </div>
      </div>
    </div>

    <!-- ── DỮ LIỆU TỔNG QUAN & XU HƯỚNG ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Bộ lọc (1/3 width) -->
      <div class="lg-card-glass p-5 flex flex-col justify-between space-y-4 print:hidden">
        <div class="space-y-4">
          <h3 class="text-xs font-bold text-heading flex items-center gap-1.5 uppercase tracking-wide">
            <FileText :size="15" class="text-orange-600" />
            Bộ lọc tra cứu
          </h3>
          
          <!-- Lọc Học kỳ -->
          <div class="space-y-1.5">
            <label class="text-[11px] font-bold text-label block">Chọn học kỳ</label>
            <select
              v-model="selectedSemester"
              class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
            >
              <option v-for="sem in currentChild.semesters" :key="sem" :value="sem">
                {{ sem }}
              </option>
            </select>
          </div>

          <!-- Lọc Môn học -->
          <div class="space-y-1.5">
            <label class="text-[11px] font-bold text-label block">Chọn môn học</label>
            <select
              v-model="selectedSubjectCode"
              class="surface-input border-card w-full px-3 py-2 text-xs rounded-xl border focus:outline-none focus:ring-2 focus:ring-orange-500/20"
            >
              <option value="all">Tất cả môn học</option>
              <option v-for="sub in subjectsInSemester" :key="sub.code" :value="sub.code">
                [{{ sub.code }}] {{ sub.name }}
              </option>
            </select>
          </div>
        </div>

        <div class="pt-4 border-t border-card flex flex-col gap-2">
          <!-- Hành động Xuất PDF -->
          <button
            @click="exportPDF"
            class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white w-full py-2 rounded-xl flex items-center justify-center gap-2 font-bold text-xs"
          >
            <Printer :size="14" />
            Xuất bảng điểm (PDF)
          </button>
          <span class="text-[9px] text-muted text-center leading-relaxed">
            * Nhấn nút để mở cửa sổ in/lưu PDF của trình duyệt
          </span>
        </div>
      </div>

      <!-- Biểu đồ xu hướng điểm số (2/3 width) -->
      <div class="lg-card-glass p-5 lg:col-span-2 space-y-4 print:hidden">
        <div class="flex items-center justify-between">
          <h3 class="text-xs font-bold text-heading flex items-center gap-1.5 uppercase tracking-wide">
            <TrendingUp :size="15" class="text-orange-600" />
            Biểu đồ xu hướng điểm số (GPA)
          </h3>
          <span class="text-[10px] font-bold text-emerald-600 bg-emerald-50 dark:bg-emerald-950/20 px-2 py-0.5 rounded">
            GPA hiện tại: {{ currentChild.gpa }}
          </span>
        </div>

        <!-- SVG Line Chart -->
        <div class="w-full overflow-hidden flex justify-center py-2">
          <svg viewBox="0 0 450 150" class="w-full max-w-[480px] h-[130px] overflow-visible">
            <!-- Grids -->
            <line x1="30" y1="30" x2="420" y2="30" stroke="var(--border-default)" stroke-dasharray="3,3" stroke-width="0.5" />
            <line x1="30" y1="65" x2="420" y2="65" stroke="var(--border-default)" stroke-dasharray="3,3" stroke-width="0.5" />
            <line x1="30" y1="100" x2="420" y2="100" stroke="var(--border-default)" stroke-dasharray="3,3" stroke-width="0.5" />
            <line x1="30" y1="120" x2="420" y2="120" stroke="var(--border-default)" stroke-width="1" />

            <!-- Y Axis Labels -->
            <text x="12" y="34" class="fill-muted text-[8px] font-bold">10.0</text>
            <text x="12" y="69" class="fill-muted text-[8px] font-bold">7.0</text>
            <text x="12" y="104" class="fill-muted text-[8px] font-bold">4.0</text>

            <!-- Filled Path (Area below line) -->
            <path
              v-if="trendPoints.length > 0"
              :d="trendAreaPath"
              fill="url(#grad)"
              opacity="0.15"
            />

            <!-- Line Path -->
            <path
              v-if="trendPoints.length > 0"
              :d="trendPath"
              fill="none"
              stroke="#ea580c"
              stroke-width="2.5"
              stroke-linecap="round"
              stroke-linejoin="round"
            />

            <!-- Data Dots -->
            <g v-for="(p, i) in trendPoints" :key="i">
              <circle
                :cx="p.x"
                :cy="p.y"
                r="4.5"
                fill="#ffffff"
                stroke="#ea580c"
                stroke-width="2"
              />
              <text
                :x="p.x"
                :y="p.y - 8"
                text-anchor="middle"
                class="fill-heading text-[8px] font-extrabold"
              >
                {{ p.gpa }}
              </text>
              <text
                :x="p.x"
                :y="136"
                text-anchor="middle"
                class="fill-muted text-[8px] font-semibold"
              >
                {{ p.month }}
              </text>
            </g>

            <defs>
              <linearGradient id="grad" x1="0%" y1="0%" x2="0%" y2="100%">
                <stop offset="0%" stop-color="#ea580c" />
                <stop offset="100%" stop-color="#ea580c" stop-opacity="0" />
              </linearGradient>
            </defs>
          </svg>
        </div>
      </div>
    </div>

    <!-- ── THÔNG BÁO QUY TẮC PHÊ DUYỆT ĐIỂM (RULE BANNER) ── -->
    <div class="p-4 rounded-xl border border-orange-200 dark:border-orange-950/20 bg-orange-50/40 dark:bg-orange-950/5 flex gap-3 print:hidden">
      <AlertCircle :size="18" class="text-orange-600 flex-shrink-0 mt-0.5" />
      <div class="text-xs text-body space-y-1">
        <p class="font-bold text-heading">Quy tắc công bố điểm:</p>
        <p class="text-body leading-relaxed">
          Hệ thống chỉ hiển thị điểm số của các môn học mà giảng viên đã phê duyệt công bố chính thức. Điểm số của các bài làm đang trong quá trình chấm hoặc chưa được phê duyệt công bố sẽ tạm thời ẩn khỏi giao diện của phụ huynh để đảm bảo tính chuẩn xác.
        </p>
        <p v-if="unpublishedCount > 0" class="text-orange-700 dark:text-orange-400 font-semibold mt-1">
          💡 Hiện tại trong học kỳ này có <strong>{{ unpublishedCount }} môn học</strong> chưa được công bố chính thức (ví dụ: {{ currentChild.grades[selectedSemester]?.find(g => !g.isPublished)?.name }}).
        </p>
      </div>
    </div>

    <!-- ── CHI TIẾT BẢNG ĐIỂM ── -->
    <div class="lg-card-glass p-5 space-y-4">
      <div class="flex items-center justify-between pb-3 border-b border-card">
        <h3 class="text-xs font-bold text-heading uppercase tracking-wide">
          Bảng điểm chi tiết: {{ selectedSemester }}
        </h3>
        <span class="text-[10px] text-muted hidden print:block">
          Học sinh: {{ currentChild.name }} ({{ currentChild.studentId }})
        </span>
      </div>

      <div v-if="filteredGrades.length === 0" class="text-center py-12 text-muted text-xs">
        Không tìm thấy môn học nào phù hợp với bộ lọc hoặc chưa có môn học nào được công bố điểm.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse min-w-[700px]">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-3 px-3">Mã môn</th>
              <th class="py-3 px-3">Tên môn học</th>
              <th class="py-3 px-3 text-center">Chuyên cần (10%)</th>
              <th class="py-3 px-3 text-center">Quiz/Lab (20%)</th>
              <th class="py-3 px-3 text-center">Giữa kỳ (30%)</th>
              <th class="py-3 px-3 text-center">Cuối kỳ (40%)</th>
              <th class="py-3 px-3 text-center font-extrabold">Điểm TB</th>
              <th class="py-3 px-3 text-right">Trạng thái</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr
              v-for="grade in filteredGrades"
              :key="grade.code"
              class="hover:bg-[var(--surface-table-row-hover)] transition"
            >
              <td class="py-3 px-3 font-semibold text-orange-600 dark:text-orange-400">{{ grade.code }}</td>
              <td class="py-3 px-3 font-medium text-heading">{{ grade.name }}</td>
              <td class="py-3 px-3 text-center font-semibold text-body">{{ grade.attendance !== null ? grade.attendance : '-' }}</td>
              <td class="py-3 px-3 text-center font-semibold text-body">{{ grade.quiz !== null ? grade.quiz : '-' }}</td>
              <td class="py-3 px-3 text-center font-semibold text-body">{{ grade.midterm !== null ? grade.midterm : '-' }}</td>
              <td class="py-3 px-3 text-center font-semibold text-body">{{ grade.final !== null ? grade.final : '-' }}</td>
              <td class="py-3 px-3 text-center font-extrabold text-heading bg-[var(--surface-table-row-hover)] text-sm">
                {{ grade.average !== null ? grade.average : '-' }}
              </td>
              <td class="py-3 px-3 text-right">
                <span
                  class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-bold"
                  :class="
                    grade.status === 'Passed' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/30 dark:text-emerald-400' :
                    grade.status === 'Failed' ? 'bg-red-100 text-red-700 dark:bg-red-950/30 dark:text-red-400' :
                    'bg-[var(--surface-input)] text-label border border-[var(--border-card)]'
                  "
                >
                  <component :is="grade.status === 'Passed' ? CheckCircle : XCircle" :size="11" />
                  {{ grade.status === 'Passed' ? 'Đạt (Passed)' : grade.status === 'Failed' ? 'Học lại (Failed)' : 'Đang học' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
@media print {
  /* Giữ thiết kế in sạch sẽ */
  .print-container {
    background: white !important;
    color: black !important;
  }
  .print\:hidden {
    display: none !important;
  }
  #grades-view-page {
    margin: 0;
    padding: 20px;
    background: white !important;
  }
  th, td {
    border-bottom: 1px solid #cbd5e1 !important;
    color: #0f172a !important;
  }
}
</style>
