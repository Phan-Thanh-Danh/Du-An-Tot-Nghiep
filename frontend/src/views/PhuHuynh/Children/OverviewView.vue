<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  LineChart,
  AlertTriangle,
  ClipboardList,
  ChevronLeft,
  ChevronDown,
  User,
  BookOpen,
  CalendarCheck
} from 'lucide-vue-next'
import { parentApi } from '@/services/parentApi'
import { getStoredActiveChildId, setActiveChildId } from '@/components/PhuHuynh/data/parentState.js'
import SkeletonDashboard from '@/components/common/skeleton/SkeletonDashboard.vue'

const route = useRoute()
const router = useRouter()

const childrenData = ref([])
const activeChildId = ref(Number(route.query.studentId) || getStoredActiveChildId())
const dropdownOpen = ref(false)
const loading = ref(false)
const error = ref('')

const currentChild = computed(() => {
  return childrenData.value.find(c => c.id === activeChildId.value) || childrenData.value[0] || {
    id: null,
    name: 'Chưa có học sinh liên kết',
    studentId: '',
    class: '',
    major: '',
    gpa: 0,
    activeCourses: 0,
    attendanceRate: 0,
    absences: 0,
    excusedAbsences: 0,
    unexcusedAbsences: 0,
    warnings: [],
    coursesList: [],
    recentSubmissions: [],
  }
})

function selectChild(id) {
  activeChildId.value = id
  dropdownOpen.value = false
  setActiveChildId(id)
  router.replace({ query: { studentId: id } })
  loadChildDetail(id)
}

function goBack() {
  router.push('/parent/children/list')
}

function getInitial(name = '') {
  return name.split(' ').filter(Boolean).pop()?.charAt(0).toUpperCase() || '-'
}

function mapAlerts(data) {
  return (data?.alerts || []).map((a, idx) => ({
    id: idx + 1,
    type: a.severity || 'info',
    subject: a.type || 'Hệ thống',
    reason: a.message || '',
    date: '',
  }))
}

async function loadChildDetail(childId) {
  if (!childId) return
  const [detailRes, gradesRes, alertsRes] = await Promise.allSettled([
    parentApi.getChildDetail(childId),
    parentApi.getChildGrades(childId),
    parentApi.getChildAlerts(childId),
  ])
  const detail = detailRes.status === 'fulfilled' ? detailRes.value?.data : {}
  const grades = gradesRes.status === 'fulfilled' ? gradesRes.value?.data || [] : []
  const alerts = alertsRes.status === 'fulfilled' ? mapAlerts(alertsRes.value?.data) : []
  const index = childrenData.value.findIndex(child => child.id === childId)
  const existing = childrenData.value[index] || {}
  const next = {
    ...existing,
    name: detail?.name || existing.name || '',
    studentId: detail?.email || existing.studentId || `ID ${childId}`,
    class: detail?.className || existing.class || '',
    major: '',
    gpa: detail?.gpa || 0,
    activeCourses: detail?.enrolledCourses || 0,
    attendanceRate: 0,
    absences: 0,
    excusedAbsences: 0,
    unexcusedAbsences: 0,
    warnings: alerts,
    coursesList: grades.map((grade, idx) => ({
      id: idx + 1,
      code: grade.code || '',
      name: grade.subject || '',
      progress: 0,
      gpa: grade.total || 0,
      status: grade.semester || '',
    })),
    recentSubmissions: [],
  }
  if (index >= 0) childrenData.value[index] = next
}

async function loadChildren() {
  loading.value = true
  error.value = ''
  try {
    const res = await parentApi.getChildren()
    childrenData.value = (res?.data || []).map(child => ({
      id: child.id,
      name: child.name || '',
      studentId: child.email || `ID ${child.id}`,
      class: child.className || '',
      major: '',
      gpa: 0,
      activeCourses: 0,
      attendanceRate: 0,
      absences: 0,
      excusedAbsences: 0,
      unexcusedAbsences: 0,
      warnings: [],
      coursesList: [],
      recentSubmissions: [],
    }))
    const firstChild = childrenData.value.find(child => child.id === activeChildId.value) || childrenData.value[0]
    if (firstChild) {
      activeChildId.value = firstChild.id
      setActiveChildId(firstChild.id)
      await loadChildDetail(firstChild.id)
    }
  } catch (e) {
    error.value = e?.message || 'Không thể tải tổng quan học sinh.'
  } finally {
    loading.value = false
  }
}

onMounted(loadChildren)
</script>

<template>
  <div class="space-y-6">
    <!-- ── THANH TÁC VỤ ĐẦU TRANG ── -->
    <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
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
            <LineChart :size="20" class="text-orange-600" />
            Tổng quan học tập
          </h2>
          <p class="text-xs text-body">Tóm tắt kết quả kiểm tra, chuyên cần và tiến trình học tập của con em</p>
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
              {{ getInitial(currentChild.name) }}
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
            class="surface-dropdown absolute right-0 top-[calc(100%+0.5rem)] z-50 w-full rounded-xl border border-card p-1 shadow-(--lg-shadow-md)"
          >
            <button
              v-for="child in childrenData"
              :key="child.id"
              type="button"
              class="flex w-full items-center justify-between rounded-lg px-2.5 py-1.5 text-left text-xs font-medium text-label transition hover:bg-(--surface-card-hover)"
              @click="selectChild(child.id)"
            >
              <span>{{ child.name }} ({{ child.class }})</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="p-4">
      <SkeletonDashboard :cards="4" :rows="5" />
    </div>

    <template v-else>
    <!-- ── THÔNG TIN HỌC SINH ── -->
    <div class="lg-card-glass p-5 flex flex-col md:flex-row md:items-center justify-between gap-6">
      <div class="flex items-center gap-4">
        <div class="h-14 w-14 flex items-center justify-center rounded-2xl bg-orange-100 dark:bg-orange-950/30 text-orange-600">
          <User :size="30" />
        </div>
        <div>
          <h3 class="text-base font-bold text-heading">{{ currentChild.name }}</h3>
          <p class="text-xs text-body mt-0.5">Mã số học sinh: <strong class="text-heading">{{ currentChild.studentId }}</strong> | Lớp học phần: <strong class="text-heading">{{ currentChild.class }}</strong></p>
          <p class="text-xs text-muted mt-0.5">Chuyên ngành đào tạo: {{ currentChild.major }}</p>
        </div>
      </div>

      <div class="flex flex-wrap gap-4 border-t md:border-t-0 md:border-l border-card pt-4 md:pt-0 md:pl-6">
        <div class="px-4 py-1.5 surface-card border border-card rounded-xl text-center min-w-[90px]">
          <span class="block text-[10px] text-muted font-semibold">Tích lũy GPA</span>
          <span class="text-base font-extrabold text-orange-600 mt-0.5 block">{{ currentChild.gpa }}</span>
        </div>
        <div class="px-4 py-1.5 surface-card border border-card rounded-xl text-center min-w-[90px]">
          <span class="block text-[10px] text-muted font-semibold">Số môn đang học</span>
          <span class="text-base font-extrabold text-heading mt-0.5 block">{{ currentChild.activeCourses }} môn</span>
        </div>
        <div class="px-4 py-1.5 surface-card border border-card rounded-xl text-center min-w-[90px]">
          <span class="block text-[10px] text-muted font-semibold">Chuyên cần</span>
          <span class="text-base font-extrabold text-heading mt-0.5 block">{{ currentChild.attendanceRate }}%</span>
        </div>
      </div>
    </div>

    <!-- ── DANH SÁCH BÀI TẬP VÀ ĐIỂM THI GẦN NHẤT ── -->
    <div class="lg-card-glass p-5">
      <h4 class="text-sm font-bold text-heading flex items-center gap-2 border-b border-card pb-3 mb-4">
        <ClipboardList :size="16" class="text-orange-600" />
        Kết quả bài tập & bài thi gần đây
      </h4>

      <div class="overflow-x-auto">
        <table class="w-full text-xs text-left border-collapse">
          <thead>
            <tr class="border-b border-card text-muted uppercase font-bold text-[10px]">
              <th class="py-2.5 px-3">Tên bài nộp</th>
              <th class="py-2.5 px-3">Môn học</th>
              <th class="py-2.5 px-3">Ngày nộp</th>
              <th class="py-2.5 px-3">Trạng thái</th>
              <th class="py-2.5 px-3 text-right">Điểm số</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-card)">
            <tr v-if="currentChild.recentSubmissions.length === 0">
              <td colspan="5" class="py-8 text-center text-muted">Chưa có dữ liệu bài nộp gần đây từ hệ thống.</td>
            </tr>
            <tr
              v-for="sub in currentChild.recentSubmissions"
              :key="sub.id"
              class="hover:bg-(--surface-card-hover) transition"
            >
              <td class="py-3 px-3 font-semibold text-heading">{{ sub.name }}</td>
              <td class="py-3 px-3 text-body">{{ sub.subject }}</td>
              <td class="py-3 px-3 text-muted">{{ sub.date }}</td>
              <td class="py-3 px-3">
                <span
                  class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase"
                  :class="
                    sub.status === 'Graded' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/20 dark:text-emerald-400' :
                    sub.status === 'Submitted' ? 'bg-orange-100 text-orange-700 dark:bg-orange-950/20 dark:text-orange-400 animate-pulse' :
                    'bg-red-100 text-red-700 dark:bg-red-950/20 dark:text-red-400'
                  "
                >
                  {{ sub.status === 'Graded' ? 'Đã chấm' : sub.status === 'Submitted' ? 'Chờ chấm' : 'Chưa nộp' }}
                </span>
              </td>
              <td class="py-3 px-3 text-right font-extrabold text-heading" :class="sub.score === 'Chưa nộp' ? 'text-red-500' : ''">
                {{ sub.score }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ── CHI TIẾT KẾT QUẢ & CẢNH BÁO ── -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Cột tiến độ môn học (2/3 width) -->
      <div class="lg-card-glass p-5 lg:col-span-2 space-y-4">
        <h4 class="text-sm font-bold text-heading flex items-center gap-2 border-b border-card pb-3">
          <BookOpen :size="16" class="text-orange-600" />
          Tiến trình môn học học kỳ hiện tại
        </h4>

        <div class="space-y-4">
          <div
            v-for="course in currentChild.coursesList"
            :key="course.id"
            class="p-4 rounded-xl border border-card hover:bg-(--surface-card-hover) transition"
          >
            <div class="flex items-center justify-between gap-2 mb-2">
              <div>
                <span class="text-[10px] font-bold text-orange-600 bg-orange-50 dark:bg-orange-950/20 px-2 py-0.5 rounded mr-2">{{ course.code }}</span>
                <span class="text-xs font-bold text-heading">{{ course.name }}</span>
              </div>
              <span class="text-xs font-bold text-heading">Điểm TB: {{ course.gpa }}</span>
            </div>

            <!-- Thanh tiến trình học tập -->
            <div>
              <div class="flex justify-between items-center text-[10px] text-muted mb-1 font-semibold">
                <span>Tiến độ chương trình học</span>
                <span>{{ course.progress }}%</span>
              </div>
              <div class="w-full bg-slate-100 dark:bg-slate-800 rounded-full h-2">
                <div
                  class="bg-orange-600 h-2 rounded-full transition-all duration-500"
                  :style="{ width: `${course.progress}%` }"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Cột chuyên cần & Cảnh báo học tập (1/3 width) -->
      <div class="space-y-6">
        
        <!-- Cảnh báo học tập -->
        <div class="lg-card-glass p-5">
          <h4 class="text-sm font-bold text-heading flex items-center gap-2 border-b border-card pb-3 mb-4">
            <AlertTriangle :size="16" class="text-red-500" />
            Cảnh báo rèn luyện học tập
          </h4>

          <div v-if="currentChild.warnings.length === 0" class="text-center py-6 text-muted text-xs">
            Hiện không có cảnh báo nào dành cho học sinh này.
          </div>
          <div v-else class="space-y-3">
            <div
              v-for="warn in currentChild.warnings"
              :key="warn.id"
              class="p-2.5 rounded-lg border text-xs"
              :class="warn.type === 'danger' ? 'border-red-200 bg-red-50/50 dark:border-red-950/20 dark:bg-red-950/5 text-red-700 dark:text-red-400' : 'border-amber-200 bg-amber-50/50 dark:border-amber-950/20 dark:bg-amber-950/5 text-amber-700 dark:text-amber-400'"
            >
              <div class="flex items-center justify-between mb-1 font-bold">
                <span>{{ warn.subject }}</span>
                <span class="text-[10px] text-muted font-normal">{{ warn.date }}</span>
              </div>
              <p class="leading-relaxed">{{ warn.reason }}</p>
            </div>
          </div>
        </div>

        <!-- Chuyên cần chi tiết -->
        <div class="lg-card-glass p-5">
          <h4 class="text-sm font-bold text-heading flex items-center gap-2 border-b border-card pb-3 mb-4">
            <CalendarCheck :size="16" class="text-orange-600" />
            Chi tiết chuyên cần
          </h4>

          <div class="space-y-3">
            <div class="flex justify-between items-center text-xs">
              <span class="text-body font-semibold">Tổng số buổi vắng:</span>
              <span class="font-extrabold text-orange-600">{{ currentChild.absences }} buổi</span>
            </div>
            <div class="border-t border-card my-2"></div>
            <div class="grid grid-cols-2 gap-2 text-center text-xs">
              <div class="p-2 surface-card border border-card rounded-lg">
                <span class="text-[10px] text-muted block">Vắng có phép</span>
                <span class="font-bold text-heading text-sm">{{ currentChild.excusedAbsences }}</span>
              </div>
              <div class="p-2 surface-card border border-card rounded-lg">
                <span class="text-[10px] text-muted block">Vắng không phép</span>
                <span class="font-bold text-red-500 text-sm">{{ currentChild.unexcusedAbsences }}</span>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- ── LƯU Ý THU HỒI QUYỀN ── -->
    <div class="text-[10px] text-center text-muted">
      * Lưu ý: Kết quả học tập và thông báo trên được trích xuất từ dữ liệu của nhà trường. Học sinh có thể tùy biến hoặc thu hồi quyền giám sát bất kỳ lúc nào theo quy định bảo mật.
    </div>
    </template>
  </div>
</template>

<style scoped>
.text-heading {
  color: var(--text-heading);
}
.text-body {
  color: var(--text-body);
}
.text-muted {
  color: var(--text-muted);
}
.border-card {
  border-color: var(--border-card);
}
</style>
