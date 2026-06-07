<script setup>
import { ref, computed } from 'vue'
import { 
  Search, Award, Clock, Download, Filter, 
  ExternalLink, ChevronRight, User, TrendingUp, CheckCircle2, AlertTriangle, Calendar,
  X, CheckCircle, XCircle, MinusCircle, FileText
} from 'lucide-vue-next'

const examResults = ref([
  { id: 1, studentId: 'SV16001', name: 'Nguyễn Văn A', score: 9.5, timeSpent: '45 min', date: '25/05/2026', status: 'Excellent' },
  { id: 2, studentId: 'SV16002', name: 'Trần Thị B', score: 7.2, timeSpent: '82 min', date: '25/05/2026', status: 'Good' },
  { id: 3, studentId: 'SV16003', name: 'Lê Hoàng C', score: 8.8, timeSpent: '60 min', date: '25/05/2026', status: 'Very Good' },
  { id: 4, studentId: 'SV16004', name: 'Phạm Minh D', score: 4.5, timeSpent: '89 min', date: '25/05/2026', status: 'Failed' },
  { id: 5, studentId: 'SV16005', name: 'Hoàng Vũ E', score: 6.0, timeSpent: '75 min', date: '25/05/2026', status: 'Average' },
])

const isDrawerOpen = ref(false)
const selectedResult = ref(null)

const mockQuestions = ref([
  { id: 1, text: 'Câu 1: Khái niệm về RESTful API là gì?', isCorrect: true, userAns: 'A', correctAns: 'A' },
  { id: 2, text: 'Câu 2: Phân biệt POST và PUT trong HTTP?', isCorrect: false, userAns: 'B', correctAns: 'C' },
  { id: 3, text: 'Câu 3: Status code 404 có ý nghĩa gì?', isCorrect: true, userAns: 'D', correctAns: 'D' },
  { id: 4, text: 'Câu 4: JWT (JSON Web Token) cấu tạo gồm mấy phần?', isCorrect: true, userAns: 'C', correctAns: 'C' },
  { id: 5, text: 'Câu 5: Mục đích của hàm hash password (BCrypt) là gì?', isCorrect: false, userAns: 'A', correctAns: 'B' },
])

const openDrawer = (result) => {
  selectedResult.value = result
  isDrawerOpen.value = true
}

const closeDrawer = () => {
  isDrawerOpen.value = false
  setTimeout(() => {
    selectedResult.value = null
  }, 300)
}

const averageScore = computed(() => {
  const total = examResults.value.reduce((acc, curr) => acc + curr.score, 0)
  return (total / examResults.value.length).toFixed(1)
})

const passRate = computed(() => {
  const passed = examResults.value.filter(res => res.score >= 5).length
  return ((passed / examResults.value.length) * 100).toFixed(0)
})

const highestScore = computed(() => {
  return Math.max(...examResults.value.map(res => res.score)).toFixed(1)
})
</script>

<template>
  <div class="space-y-6 pb-10 text-body">
    <!-- ── Header ── -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 surface-card border border-card rounded-2xl p-5 shadow-sm relative overflow-hidden">
      <!-- Decorative background -->
      
      
      <div class="relative z-10 flex items-center gap-5">
        <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] border border-[var(--color-info-text)]/20 shadow-sm">
           <Award :size="32" />
        </div>
        <div>
          <h1 class="text-xl md:text-xl font-black text-heading tracking-tight">Kết quả bài thi</h1>
          <p class="text-sm font-medium text-muted mt-1">Danh sách điểm số và thống kê kết quả của kỳ thi vừa qua.</p>
        </div>
      </div>
      <div class="relative z-10 flex gap-3">
         <button class="flex items-center gap-2 rounded-2xl surface-input px-5 py-3 border border-input shadow-sm hover:text-link transition-colors font-bold text-sm text-label">
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
                <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] border border-[var(--color-info-text)]/20">
                   <TrendingUp :size="20" />
                </div>
                <span class="text-[10px] font-black uppercase tracking-widest text-muted bg-[var(--surface-input)] px-2 py-1 rounded-lg">Trung bình</span>
             </div>
             <p class="text-xl font-black text-heading">{{ averageScore }}</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-3">
                <div class="h-10 w-10 rounded-xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] border border-[var(--color-success-text)]/20">
                   <CheckCircle2 :size="20" />
                </div>
                <span class="text-[10px] font-black uppercase tracking-widest text-muted bg-[var(--surface-input)] px-2 py-1 rounded-lg">Tỷ lệ Đạt</span>
             </div>
             <p class="text-xl font-black text-heading">{{ passRate }}%</p>
          </div>
          <div class="rounded-2xl surface-card border border-card p-4 shadow-sm">
             <div class="flex items-center justify-between mb-3">
                <div class="h-10 w-10 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)] border border-[var(--color-warning-text)]/20">
                   <Award :size="20" />
                </div>
                <span class="text-[10px] font-black uppercase tracking-widest text-muted bg-[var(--surface-input)] px-2 py-1 rounded-lg">Cao nhất</span>
             </div>
             <p class="text-xl font-black text-heading">{{ highestScore }}</p>
          </div>
       </div>

       <!-- Filters -->
       <div class="flex-1 rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
          <p class="text-sm font-bold text-heading mb-4 flex items-center gap-2"><Filter :size="16" class="text-link" /> Bộ lọc dữ liệu</p>
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="relative flex-1">
              <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
              <input type="text" placeholder="Tìm sinh viên bằng tên hoặc MSSV..." class="w-full rounded-xl border border-input surface-input pl-11 pr-4 py-3 text-sm font-medium outline-none focus:border-[var(--border-input-focus)] transition-colors" />
            </div>
            <div class="relative w-full sm:w-48 shrink-0">
               <select class="w-full rounded-xl border border-input surface-input px-4 py-3 text-sm font-bold text-label outline-none focus:border-[var(--border-input-focus)] transition-colors appearance-none cursor-pointer">
                  <option>Tất cả kỳ thi</option>
                  <option>Thi giữa kỳ</option>
                  <option>Thi cuối kỳ</option>
               </select>
               <ChevronRight :size="16" class="absolute right-4 top-1/2 -translate-y-1/2 text-placeholder rotate-90 pointer-events-none" />
            </div>
          </div>
       </div>
    </div>

    <!-- Results Table -->
    <div class="rounded-2xl border border-card surface-card shadow-sm overflow-hidden animate-fade-in-up">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid border-b border-default">
              <th class="px-5 py-4 text-[11px] font-black uppercase tracking-widest text-muted">Sinh viên</th>
              <th class="px-4 py-4 text-[11px] font-black uppercase tracking-widest text-muted">Điểm số</th>
              <th class="px-4 py-4 text-[11px] font-black uppercase tracking-widest text-muted">Thời gian làm bài</th>
              <th class="px-4 py-4 text-[11px] font-black uppercase tracking-widest text-muted">Ngày thi</th>
              <th class="px-5 py-4 text-[11px] font-black uppercase tracking-widest text-muted text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="res in examResults" :key="res.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-5 py-4">
                <div class="flex items-center gap-4">
                  <div class="h-10 w-10 rounded-2xl surface-solid border border-default flex items-center justify-center text-muted font-black text-sm group-hover:bg-[var(--color-info-bg)] group-hover:text-[var(--color-info-text)] group-hover:border-[var(--color-info-text)]/20 transition-colors shadow-sm">
                    {{ res.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-heading group-hover:text-link transition-colors">{{ res.name }}</p>
                    <p class="text-[10px] font-black text-muted uppercase tracking-widest mt-0.5">{{ res.studentId }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                   <div :class="['h-10 w-10 rounded-xl flex items-center justify-center border', 
                                res.score >= 8 ? 'bg-[var(--color-success-bg)] border-[var(--color-success-text)]/20 text-[var(--color-success-text)]' :
                                res.score >= 5 ? 'bg-[var(--color-info-bg)] border-[var(--color-info-text)]/20 text-[var(--color-info-text)]' :
                                'bg-[var(--color-danger-bg)] border-[var(--color-danger-text)]/20 text-[var(--color-danger-text)]']">
                      <Award :size="18" />
                   </div>
                   <div class="flex flex-col">
                      <span :class="['text-xl font-black', 
                                    res.score >= 8 ? 'text-[var(--color-success-text)]' :
                                    res.score >= 5 ? 'text-[var(--color-info-text)]' :
                                    'text-[var(--color-danger-text)]']">{{ res.score.toFixed(1) }}</span>
                   </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2 text-sm font-bold text-label surface-solid px-3 py-1.5 rounded-xl border border-default w-max">
                   <Clock :size="14" class="text-link" />
                   {{ res.timeSpent }}
                </div>
              </td>
              <td class="px-4 py-4">
                 <div class="flex items-center gap-2 text-sm font-bold text-label">
                   <Calendar :size="14" class="text-muted" />
                   {{ res.date }}
                 </div>
              </td>
              <td class="px-5 py-4 text-right">
                <button @click="openDrawer(res)" class="inline-flex items-center justify-center h-10 px-4 rounded-xl border border-input surface-input text-[11px] font-black uppercase tracking-widest text-muted hover:text-link hover:border-[var(--border-input-focus)] transition-colors shadow-sm">
                   Chi tiết <ChevronRight :size="14" class="ml-1" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Footer -->
      <div class="surface-solid px-5 py-4 border-t border-default flex items-center justify-between">
         <div class="flex items-center gap-2 text-xs font-bold text-muted uppercase tracking-widest">
            <User :size="14" class="text-muted" /> Hiển thị {{ examResults.length }} kết quả
         </div>
         <div class="flex items-center gap-1">
            <div class="h-1 w-8 bg-[var(--lg-primary)] rounded-full"></div>
            <div class="h-1 w-2 bg-[var(--color-info-bg)] rounded-full"></div>
            <div class="h-1 w-2 bg-[var(--color-info-bg)] rounded-full"></div>
         </div>
      </div>
    </div>
  </div>

  <!-- Slide-over Drawer for Exam Details -->
  <Teleport to="body">
    <!-- Backdrop -->
    <div 
      v-if="isDrawerOpen" 
      class="fixed inset-0 z-[9998] bg-[var(--surface-backdrop)] backdrop-blur-sm transition-opacity"
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
            <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] text-[var(--color-info-text)] flex items-center justify-center font-black text-lg shadow-sm border border-[var(--color-info-text)]/20">
              {{ selectedResult.name.split(' ').pop()[0] }}
            </div>
            <div>
              <h2 class="text-lg font-black text-heading">{{ selectedResult.name }}</h2>
              <p class="text-[11px] font-black text-muted uppercase tracking-widest mt-0.5">{{ selectedResult.studentId }}</p>
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
                <p class="text-[10px] font-black text-muted uppercase tracking-widest mb-1">Điểm tổng kết</p>
                <div class="flex items-baseline gap-2">
                   <span :class="['text-3xl font-black tracking-tighter',
                        selectedResult.score >= 8 ? 'text-[var(--color-success-text)]' :
                        selectedResult.score >= 5 ? 'text-[var(--color-info-text)]' : 'text-[var(--color-danger-text)]']">
                      {{ selectedResult.score.toFixed(1) }}
                   </span>
                   <span class="text-sm font-bold text-muted">/ 10</span>
                </div>
             </div>
             <div :class="['px-4 py-2 rounded-xl text-xs font-black uppercase tracking-widest border', 
                  selectedResult.score >= 8 ? 'bg-[var(--color-success-bg)] text-[var(--color-success-text)] border-[var(--color-success-text)]/20' :
                  selectedResult.score >= 5 ? 'bg-[var(--color-info-bg)] text-[var(--color-info-text)] border-[var(--color-info-text)]/20' : 'bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] border-[var(--color-danger-text)]/20']">
                {{ selectedResult.score >= 5 ? 'Đạt' : 'Không đạt' }}
             </div>
          </div>

          <!-- Quick Stats -->
          <div class="grid grid-cols-2 gap-4">
             <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
                <div class="flex items-center gap-2 mb-2">
                   <Clock :size="16" class="text-link" />
                   <span class="text-[10px] font-black text-muted uppercase tracking-widest">Thời gian làm bài</span>
                </div>
                <p class="text-lg font-black text-heading">{{ selectedResult.timeSpent }}</p>
             </div>
             <div class="rounded-2xl surface-card border border-card p-4 shadow-sm flex flex-col justify-center">
                <div class="flex items-center gap-2 mb-2">
                   <FileText :size="16" class="text-link" />
                   <span class="text-[10px] font-black text-muted uppercase tracking-widest">Số câu đúng</span>
                </div>
                <p class="text-lg font-black text-heading">{{ Math.round((selectedResult.score / 10) * mockQuestions.length) }} / {{ mockQuestions.length }}</p>
             </div>
          </div>

          <!-- Questions List -->
          <div>
             <h3 class="text-sm font-black text-heading uppercase tracking-wider mb-4 flex items-center gap-2">
                <CheckCircle2 :size="16" class="text-[var(--color-success-text)]" /> Chi tiết bài làm
             </h3>
             <div class="space-y-3">
                <div v-for="q in mockQuestions" :key="q.id" class="rounded-2xl surface-card border border-card p-4 shadow-sm hover:border-[var(--border-input-focus)] transition-colors">
                   <div class="flex items-start gap-3">
                      <div class="mt-0.5 shrink-0">
                         <CheckCircle v-if="q.isCorrect" :size="18" class="text-[var(--color-success-text)]" />
                         <XCircle v-else :size="18" class="text-[var(--color-danger-text)]" />
                      </div>
                      <div>
                         <p class="text-sm font-bold text-label leading-snug">{{ q.text }}</p>
                         <div class="flex items-center gap-4 mt-3">
                            <span class="text-[11px] font-black text-muted uppercase tracking-widest">Trả lời:
                               <span :class="q.isCorrect ? 'text-[var(--color-success-text)]' : 'text-[var(--color-danger-text)]'">{{ q.userAns }}</span>
                            </span>
                            <span v-if="!q.isCorrect" class="text-[11px] font-black text-muted uppercase tracking-widest">Đáp án:
                               <span class="text-[var(--color-success-text)]">{{ q.correctAns }}</span>
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
