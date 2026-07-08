<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Search, Award, Clock, Download, Filter, 
  ChevronRight, User, TrendingUp, CheckCircle2, AlertCircle, Calendar,
  X, CheckCircle, XCircle, FileText, Loader2
} from 'lucide-vue-next'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(false)
const error = ref('')
const examResults = ref([])
const isDrawerOpen = ref(false)
const selectedResult = ref(null)

const examDetailQuestions = ref([])

async function loadResults() {
  loading.value = true
  error.value = ''
  try {
    const data = await teacherApi.getExamResults()
    examResults.value = Array.isArray(data) ? data : (data?.items ?? data?.data ?? [])
  } catch (e) {
    error.value = e?.message || 'Không thể tải kết quả thi.'
    examResults.value = []
  } finally {
    loading.value = false
  }
}

const openDrawer = async (result) => {
  selectedResult.value = result
  isDrawerOpen.value = true
  try {
    const detailData = await teacherApi.getExamDetail(result.examId || result.maCaThi)
    examDetailQuestions.value = detailData?.questions || detailData?.cauHoi || []
  } catch {
    examDetailQuestions.value = []
  }
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  setTimeout(() => {
    selectedResult.value = null
    examDetailQuestions.value = []
  }, 300)
}

const averageScore = computed(() => {
  if (!examResults.value.length) return '0.0'
  const total = examResults.value.reduce((acc, curr) => acc + (curr.score || curr.diem || 0), 0)
  return (total / examResults.value.length).toFixed(1)
})

const passRate = computed(() => {
  if (!examResults.value.length) return '0'
  const passed = examResults.value.filter(res => (res.score || res.diem || 0) >= 5).length
  return ((passed / examResults.value.length) * 100).toFixed(0)
})

const highestScore = computed(() => {
  if (!examResults.value.length) return '0.0'
  return Math.max(...examResults.value.map(res => res.score || res.diem || 0)).toFixed(1)
})

onMounted(() => { loadResults() })
</script>

<template>
  <div class="space-y-6 pb-10 text-body">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 surface-card border border-card rounded-2xl p-5 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20 shadow-sm">
           <Award :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-semibold text-heading tracking-tight">Kết quả bài thi</h1>
          <p class="text-sm font-medium text-muted mt-1">Danh sách điểm số và thống kê kết quả của kỳ thi vừa qua.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <button class="flex items-center gap-2 rounded-2xl surface-input px-5 py-3 border border-input shadow-sm hover:text-link transition-colors font-semibold text-sm text-label">
            <Download :size="18" /> Xuất kết quả
         </button>
      </div>
    </div>

    <!-- Quick Stats & Filters -->
    <div class="flex flex-col xl:flex-row gap-4">
       <!-- Stats -->
       <div class="grid grid-cols-1 md:grid-cols-3 xl:w-1/2 gap-4">
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-3">
                <div class="h-10 w-10 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) border border-(--color-info-text)/20">
                   <TrendingUp :size="20" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Trung bình</span>
             </div>
             <p class="text-xl font-semibold text-heading">{{ averageScore }}</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-3">
                <div class="h-10 w-10 rounded-xl bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text) border border-(--color-success-text)/20">
                   <CheckCircle2 :size="20" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Tỷ lệ Đạt</span>
             </div>
             <p class="text-xl font-semibold text-heading">{{ passRate }}%</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-3">
                <div class="h-10 w-10 rounded-xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text) border border-(--color-warning-text)/20">
                   <Award :size="20" />
                </div>
                <span class="text-[10px] font-semibold uppercase tracking-widest text-muted bg-(--surface-input) px-2 py-1 rounded-lg">Cao nhất</span>
             </div>
             <p class="text-xl font-semibold text-heading">{{ highestScore }}</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
          <p class="text-sm font-semibold text-heading mb-4 flex items-center gap-2"><Filter :size="16" class="text-link" /> Bộ lọc dữ liệu</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm sinh viên bằng tên hoặc MSSV..." class="w-full rounded-xl border border-input surface-input pl-11 pr-4 py-3 text-sm font-medium outline-none focus:border-(--border-input-focus) transition-colors" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
               <select class="w-full rounded-xl border border-input surface-input px-4 py-3 text-sm font-medium text-label outline-none focus:border-(--border-input-focus) transition-colors appearance-none cursor-pointer">
                  <option>Tất cả kỳ thi</option>
                  <option>Thi giữa kỳ</option>
                  <option>Thi cuối kỳ</option>
               </select>
               <ChevronRight :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder rotate-90 pointer-events-none" />
            </div>
          </div>
       </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex flex-col items-center justify-center py-20">
      <Loader2 :size="32" class="animate-spin text-muted mb-4" />
      <p class="text-sm font-semibold text-muted">Đang tải kết quả thi...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex flex-col items-center justify-center py-20">
      <AlertCircle :size="48" class="text-rose-400 mb-4" />
      <p class="text-sm font-semibold text-heading mb-2">Có lỗi xảy ra</p>
      <p class="text-sm text-muted mb-4">{{ error }}</p>
      <button class="btn-primary" @click="loadResults">Thử lại</button>
    </div>

    <!-- Empty -->
    <div v-else-if="!examResults.length" class="flex flex-col items-center justify-center py-20">
      <Award :size="48" class="text-placeholder mb-4" />
      <p class="text-sm font-medium text-muted">Chưa có kết quả thi nào.</p>
    </div>

    <!-- Results Table -->
    <div v-else class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid border-b border-default">
              <th class="px-5 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Sinh viên</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Điểm số</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Thời gian làm bài</th>
              <th class="px-4 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted">Ngày thi</th>
              <th class="px-5 py-4 text-[11px] font-semibold uppercase tracking-widest text-muted text-right">Thao tác</th>
            </tr>
          </thead>
            <tbody class="divide-y divide-default">
              <tr v-for="res in examResults" :key="res.id || res.maKetQua" class="group hover:bg-(--surface-input) transition-colors">
                <td class="px-5 py-4">
                  <div class="flex items-center gap-4">
                    <div class="h-10 w-10 rounded-2xl surface-solid border border-default flex items-center justify-center text-muted font-semibold text-sm group-hover:bg-(--color-info-bg) group-hover:text-(--color-info-text) group-hover:border-(--color-info-text)/20 transition-colors shadow-sm">
                      {{ (res.hoTen || res.name || '').split(' ').pop()[0] || '?' }}
                    </div>
                    <div>
                      <p class="text-sm font-semibold text-heading group-hover:text-link transition-colors">{{ res.hoTen || res.name }}</p>
                      <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mt-0.5">{{ res.maSinhVien || res.studentId }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-4 py-4">
                  <div class="flex items-center gap-3">
                     <div :class="['h-10 w-10 rounded-xl flex items-center justify-center border', 
                                  (res.diem || res.score) >= 8 ? 'bg-(--color-success-bg) border-(--color-success-text)/20 text-(--color-success-text)' :
                                  (res.diem || res.score) >= 5 ? 'bg-(--color-info-bg) border-(--color-info-text)/20 text-(--color-info-text)' :
                                  'bg-(--color-danger-bg) border-(--color-danger-text)/20 text-(--color-danger-text)']">
                        <Award :size="18" />
                     </div>
                     <div class="flex flex-col">
                         <span :class="['text-xl font-semibold',
                                      (res.diem || res.score) >= 8 ? 'text-(--color-success-text)' :
                                      (res.diem || res.score) >= 5 ? 'text-(--color-info-text)' :
                                      'text-(--color-danger-text)']">{{ (res.diem || res.score || 0).toFixed(1) }}</span>
                     </div>
                  </div>
                </td>
                <td class="px-4 py-4">
                  <div class="flex items-center gap-2 text-sm font-semibold text-label surface-solid px-3 py-1.5 rounded-xl border border-default w-max">
                     <Clock :size="14" class="text-link" />
                     {{ res.thoiGianLam || res.timeSpent || '--' }}
                  </div>
                </td>
                <td class="px-4 py-4">
                   <div class="flex items-center gap-2 text-sm font-semibold text-label">
                     <Calendar :size="14" class="text-muted" />
                     {{ res.ngayThi || res.date || '--' }}
                   </div>
                </td>
              <td class="px-5 py-4 text-right">
                <button @click="openDrawer(res)" class="inline-flex items-center justify-center h-10 px-4 rounded-xl border border-input surface-input text-[11px] font-semibold tracking-wider text-muted hover:text-link hover:border-(--border-input-focus) transition-colors shadow-sm">
                   Chi tiết <ChevronRight :size="14" class="ml-1" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="surface-solid px-5 py-4 border-t border-default flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-semibold text-muted tracking-wider">
            <User :size="14" class="text-muted" /> Hiển thị {{ examResults.length }} kết quả
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-(--lg-primary) rounded-full"></div>
            <div class="h-1 w-2 bg-(--color-info-bg) rounded-full"></div>
            <div class="h-1 w-2 bg-(--color-info-bg) rounded-full"></div>
         </div>
      </div>
    </div>
  </div>

  <!-- Slide-over Drawer for Exam Details -->
  <Teleport to="body">
    <!-- Backdrop -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[9998] bg-(--surface-backdrop) backdrop-blur-sm transition-opacity"
      @click="closeDrawer"
    ></div>

    <!-- Drawer Panel -->
    <div 
      class="fixed inset-y-0 right-0 z-[9999] w-full max-w-md surface-modal shadow-sm border-l border-card transition-transform duration-300 ease-in-out flex flex-col"
      :class="isDrawerOpen ? 'translate-x-0' : 'translate-x-full'"
    >
      <template v-if="selectedResult">
        <!-- Drawer Header -->
        <div class="flex items-center justify-between p-4 border-b border-default surface-solid">
          <div class="flex items-center gap-4">
            <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-semibold text-lg shadow-sm border border-(--color-info-text)/20">
              {{ (selectedResult.hoTen || selectedResult.name || '').split(' ').pop()[0] || '?' }}
            </div>
            <div>
              <h2 class="text-lg font-semibold text-heading">{{ selectedResult.hoTen || selectedResult.name }}</h2>
              <p class="text-[11px] font-semibold text-muted uppercase tracking-widest mt-0.5">{{ selectedResult.maSinhVien || selectedResult.studentId }}</p>
            </div>
          </div>
          <button @click="closeDrawer" class="h-10 w-10 rounded-xl flex items-center justify-center surface-input border border-input text-muted hover:text-heading transition-colors shadow-sm">
            <X :size="20" />
          </button>
        </div>

        <!-- Drawer Content -->
        <div class="flex-1 overflow-y-auto p-4 space-y-4 surface-solid scrollbar-hide">
          
           <!-- Score Card -->
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex items-center justify-between">
             <div>
                <p class="text-[10px] font-semibold text-muted uppercase tracking-widest mb-1">Điểm tổng kết</p>
                <div class="flex items-baseline gap-2">
                   <span :class="['text-3xl font-semibold tracking-tighter',
                        (selectedResult.diem || selectedResult.score) >= 8 ? 'text-(--color-success-text)' :
                        (selectedResult.diem || selectedResult.score) >= 5 ? 'text-(--color-info-text)' : 'text-(--color-danger-text)']">
                      {{ (selectedResult.diem || selectedResult.score || 0).toFixed(1) }}
                   </span>
                   <span class="text-sm font-medium text-muted">/ 10</span>
                </div>
             </div>
              <div :class="['px-4 py-2 rounded-xl text-xs font-semibold uppercase tracking-widest border',
                  (selectedResult.diem || selectedResult.score) >= 8 ? 'bg-(--color-success-bg) text-(--color-success-text) border-(--color-success-text)/20' :
                  (selectedResult.diem || selectedResult.score) >= 5 ? 'bg-(--color-info-bg) text-(--color-info-text) border-(--color-info-text)/20' : 'bg-(--color-danger-bg) text-(--color-danger-text) border-(--color-danger-text)/20']">
                {{ (selectedResult.diem || selectedResult.score) >= 5 ? 'Đạt' : 'Không đạt' }}
             </div>
          </div>

          <!-- Quick Stats -->
          <div class="grid grid-cols-2 gap-4">
             <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
                <div class="flex items-center gap-2 mb-2">
                   <Clock :size="16" class="text-link" />
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-widest">Thời gian làm bài</span>
                </div>
                 <p class="text-lg font-semibold text-heading">{{ selectedResult.thoiGianLam || selectedResult.timeSpent || '--' }}</p>
             </div>
             <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
                <div class="flex items-center gap-2 mb-2">
                   <FileText :size="16" class="text-link" />
                   <span class="text-[10px] font-semibold text-muted uppercase tracking-widest">Số câu đúng</span>
                </div>
                 <p class="text-lg font-semibold text-heading">{{ examDetailQuestions.length ? Math.round(((selectedResult.diem || selectedResult.score || 0) / 10) * examDetailQuestions.length) + ' / ' + examDetailQuestions.length : '--' }}</p>
             </div>
          </div>

          <!-- Questions List -->
          <div>
             <h3 class="text-sm font-semibold text-heading tracking-wide mb-4 flex items-center gap-2">
                <CheckCircle2 :size="16" class="text-(--color-success-text)" /> Chi tiết bài làm
             </h3>
             <div class="space-y-3">
                 <div v-for="(q, idx) in examDetailQuestions" :key="q.id || idx" class="rounded-2xl surface-card border border-card p-4 shadow-sm hover:border-(--border-input-focus) transition-colors">
                    <div class="flex items-start gap-3">
                       <div class="mt-0.5 shrink-0">
                          <CheckCircle v-if="q.dungSai || q.isCorrect" :size="18" class="text-(--color-success-text)" />
                          <XCircle v-else :size="18" class="text-(--color-danger-text)" />
                       </div>
                       <div>
                          <p class="text-sm font-semibold text-label leading-snug">{{ q.text || q.noiDung || 'Câu hỏi ' + (idx + 1) }}</p>
                          <div class="flex items-center gap-4 mt-3">
                             <span class="text-[11px] font-semibold text-muted uppercase tracking-widest">Trả lời:
                                <span :class="q.dungSai || q.isCorrect ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'">{{ q.traLoi || q.userAns || '--' }}</span>
                             </span>
                             <span v-if="!(q.dungSai || q.isCorrect)" class="text-[11px] font-semibold text-muted uppercase tracking-widest">Đáp án:
                                <span class="text-(--color-success-text)">{{ q.dapAn || q.correctAns || '--' }}</span>
                             </span>
                          </div>
                       </div>
                    </div>
                </div>
             </div>
          </div>

        </div>
      </template>
    </div>
  </Teleport>
</template>

<style scoped>
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(15px); }
  to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in-up {
  animation: fade-in-up 0.3s ease-out forwards;
}

.scrollbar-hide::-webkit-scrollbar {
    display: none;
}
.scrollbar-hide {
    -ms-overflow-style: none;
    scrollbar-width: none;
}
</style>
