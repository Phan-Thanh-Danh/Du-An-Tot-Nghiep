<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import {
  Star,
  MessageSquareHeart,
  BookOpen,
  Users,
  ShieldCheck,
  Calendar,
  X,
  RefreshCw,
  Award,
  Clock,
  Sparkles,
  ChevronRight
} from 'lucide-vue-next'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import ProgressBar from '@/components/ui/ProgressBar.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { teacherApi } from '@/services/teacherApi'

const loading = ref(true)
const isRefreshing = ref(false)
const error = ref('')

const report = ref({
  averageScore: 5.0,
  totalEvaluations: 0,
  totalReviews: 0,
  subjects: [],
  reviews: []
})

// Modal Popup State khi click vào từng mục tiêu chí
const isModalOpen = ref(false)
const selectedSubject = ref(null)
const selectedCriterion = ref(null)

let pollInterval = null

const fetchEvaluations = async (isInitial = false) => {
  if (isInitial) loading.value = true
  else isRefreshing.value = true
  error.value = ''
  try {
    const data = await teacherApi.getEvaluations()
    if (data) {
      report.value = data

      // Cập nhật lại dữ liệu trong popup nếu đang mở modal
      if (isModalOpen.value && selectedSubject.value && selectedCriterion.value) {
        const foundSub = data.subjects?.find(s => s.subjectCode === selectedSubject.value.subjectCode)
        if (foundSub) {
          selectedSubject.value = foundSub
          const foundCrit = foundSub.criteria?.find(c => c.questionId === selectedCriterion.value.questionId)
          if (foundCrit) {
            selectedCriterion.value = foundCrit
          }
        }
      }
    }
  } catch (err) {
    if (isInitial) {
      error.value = err?.message || 'Không thể tải dữ liệu đánh giá.'
    }
  } finally {
    loading.value = false
    isRefreshing.value = false
  }
}

// Mở popup chi tiết khi click vào một mục
const openCriterionModal = (subject, criterion) => {
  selectedSubject.value = subject
  selectedCriterion.value = criterion
  isModalOpen.value = true
}

// Đóng modal khi click vào chỗ trống bên ngoài hoặc bấm nút
const closeModal = () => {
  isModalOpen.value = false
  selectedSubject.value = null
  selectedCriterion.value = null
}

// Lắng nghe phím ESC để đóng modal
const handleKeydown = (e) => {
  if (e.key === 'Escape' && isModalOpen.value) {
    closeModal()
  }
}

onMounted(() => {
  fetchEvaluations(true)
  window.addEventListener('keydown', handleKeydown)
  // Tự động đồng bộ mỗi 3 giây để cập nhật tức thì khi sinh viên gửi đánh giá
  pollInterval = setInterval(() => {
    fetchEvaluations(false)
  }, 3000)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
  if (pollInterval) {
    clearInterval(pollInterval)
  }
})
</script>

<template>
  <div class="lg-page-enter mx-auto max-w-7xl space-y-6 pb-12">
    <!-- Header -->
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div>
        <div class="flex items-center gap-2 text-xs font-semibold uppercase tracking-wider text-(--text-muted)">
          <MessageSquareHeart :size="15" class="text-amber-500" />
          <span>Đảm bảo chất lượng & Ý kiến sinh viên</span>
        </div>
        <h1 class="mt-1 text-2xl font-bold text-(--text-heading)">Đánh giá & Phản hồi Theo Môn Học</h1>
        <p class="text-sm text-(--text-muted)">Xem điểm số trung bình và click trực tiếp vào từng mục để xem bảng chi tiết đánh giá.</p>
      </div>
      <div class="flex items-center gap-3">
        <span class="inline-flex items-center gap-1.5 text-xs text-(--text-muted) px-2.5 py-1 rounded-full surface-card border border-default">
          <span class="relative flex h-2 w-2">
            <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
            <span class="relative inline-flex rounded-full h-2 w-2 bg-emerald-500"></span>
          </span>
          <span>Tự động cập nhật trực tiếp</span>
        </span>
        <GlassBadge variant="info" class="flex items-center gap-1 py-1.5 px-3">
          <ShieldCheck :size="14" />
          <span>Khảo sát Ẩn danh 100%</span>
        </GlassBadge>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="space-y-4">
      <LoadingSkeleton variant="card" class="h-32" />
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <LoadingSkeleton variant="card" class="h-64" />
        <LoadingSkeleton variant="card" class="h-64" />
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="rounded-xl border border-red-200/50 bg-red-50/20 p-6 text-center text-red-600">
      <p>{{ error }}</p>
    </div>

    <!-- Main Content -->
    <div v-else class="space-y-6">
      <!-- Quick Overall Summary Bar -->
      <GlassPanel variant="strong" class="p-5 flex flex-wrap items-center justify-between gap-4 border-default">
        <div class="flex items-center gap-4">
          <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-500/15 text-amber-500 border border-amber-500/30 shrink-0">
            <Star :size="30" class="fill-amber-500" />
          </div>
          <div>
            <div class="flex items-baseline gap-2">
              <span class="text-2xl font-extrabold text-(--text-heading)">{{ report.averageScore }}</span>
              <span class="text-sm font-semibold text-(--text-muted)">/ 5.0 sao trung bình</span>
            </div>
            <p class="text-xs text-(--text-muted) mt-0.5">Tổng cộng {{ report.totalEvaluations }} lượt đánh giá trên tất cả các môn</p>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <div class="surface-card rounded-xl border border-default px-4 py-2 text-center">
            <p class="text-[11px] font-semibold text-(--text-muted) uppercase">Số môn phụ trách</p>
            <p class="text-lg font-bold text-(--text-heading)">{{ report.subjects?.length || 0 }}</p>
          </div>
          <div class="surface-card rounded-xl border border-default px-4 py-2 text-center">
            <p class="text-[11px] font-semibold text-(--text-muted) uppercase">Góp ý sinh viên</p>
            <p class="text-lg font-bold text-(--text-heading)">{{ report.totalReviews }}</p>
          </div>
        </div>
      </GlassPanel>

      <!-- DANH SÁCH TỪNG MÔN HỌC & CÁC MỤC ĐÁNH GIÁ -->
      <div v-if="report.subjects && report.subjects.length > 0" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div
          v-for="sub in report.subjects"
          :key="sub.subjectCode"
          class="surface-card rounded-2xl border border-default overflow-hidden shadow-sm hover:border-(--border-focus) transition-all duration-200"
        >
          <!-- Subject Card Header -->
          <div class="p-5 bg-slate-50/50 dark:bg-slate-900/30 border-b border-default flex items-center justify-between gap-4">
            <div class="flex items-center gap-3 min-w-0">
              <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-blue-500/10 text-blue-600 dark:text-blue-400 border border-blue-500/20 shrink-0">
                <BookOpen :size="22" />
              </div>
              <div class="min-w-0">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold uppercase tracking-wider text-blue-600 dark:text-blue-400 bg-blue-500/10 px-2 py-0.5 rounded">
                    {{ sub.subjectCode }}
                  </span>
                  <span v-if="sub.className" class="text-xs text-(--text-muted) truncate">
                    • Lớp: {{ sub.className }}
                  </span>
                </div>
                <h3 class="text-base font-bold text-(--text-heading) truncate mt-0.5" :title="sub.subjectName">
                  {{ sub.subjectName }}
                </h3>
              </div>
            </div>

            <!-- Subject Average Score -->
            <div class="flex flex-col items-end shrink-0">
              <div class="flex items-center gap-1.5">
                <Star :size="18" class="fill-amber-400 text-amber-400" />
                <span class="text-xl font-black text-(--text-heading)">{{ sub.averageScore }}</span>
                <span class="text-xs text-(--text-muted)">/ 5.0</span>
              </div>
              <span class="text-[11px] text-(--text-muted) mt-0.5">
                {{ sub.totalEvaluations }} lượt đánh giá
              </span>
            </div>
          </div>

          <!-- Subject Criteria List (Click vào từng mục để xem bảng chi tiết) -->
          <div class="p-4 space-y-2.5">
            <p class="text-xs font-semibold text-(--text-muted) px-1 mb-2">
              Danh sách các mục đánh giá (Nhấn vào mục để xem bảng chi tiết):
            </p>

            <div
              v-for="crit in sub.criteria"
              :key="crit.questionId"
              class="group relative rounded-xl border border-default surface-input p-3.5 hover:border-(--text-link) hover:bg-blue-50/20 dark:hover:bg-blue-900/10 transition-all cursor-pointer"
              @click="openCriterionModal(sub, crit)"
            >
              <div class="flex items-center justify-between gap-3">
                <div class="min-w-0 flex-1">
                  <p class="text-sm font-semibold text-(--text-heading) group-hover:text-(--text-link) transition-colors">
                    {{ crit.questionText }}
                  </p>
                  <div class="flex items-center gap-2 mt-1 text-xs text-(--text-muted)">
                    <span>{{ crit.responseCount }} phản hồi</span>
                    <span>•</span>
                    <span class="text-emerald-600 dark:text-emerald-400 font-medium">Click xem bảng chi tiết</span>
                  </div>
                </div>

                <div class="flex items-center gap-2 shrink-0">
                  <div class="flex items-center gap-1 bg-amber-500/10 border border-amber-500/20 px-2.5 py-1 rounded-lg">
                    <Star :size="14" class="fill-amber-400 text-amber-400" />
                    <span class="text-sm font-bold text-amber-600 dark:text-amber-400">{{ crit.averageScore }}</span>
                  </div>
                  <ChevronRight :size="16" class="text-(--text-muted) group-hover:text-(--text-link) group-hover:translate-x-0.5 transition-all" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="p-8 text-center text-(--text-muted)">
        <EmptyState
          title="Chưa có dữ liệu môn học"
          description="Hiện tại chưa ghi nhận môn học hoặc đánh giá nào."
        />
      </div>
    </div>

    <!-- POPUP MODAL: BẢNG CHI TIẾT ĐÁNH GIÁ (Click vào chỗ trống bên ngoài là tự thoát) -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div
          v-if="isModalOpen"
          class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
          @click.self="closeModal"
        >
          <!-- Modal Card Content -->
          <div
            class="surface-card w-full max-w-3xl rounded-2xl border border-default shadow-2xl overflow-hidden flex flex-col max-h-[85vh] animate-in fade-in zoom-in-95 duration-200"
          >
            <!-- Modal Header -->
            <div class="p-5 border-b border-default flex items-start justify-between gap-4 bg-slate-50/70 dark:bg-slate-900/50">
              <div>
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold uppercase tracking-wider text-blue-600 dark:text-blue-400 bg-blue-500/10 px-2 py-0.5 rounded">
                    {{ selectedSubject?.subjectCode }} - {{ selectedSubject?.subjectName }}
                  </span>
                  <span class="text-xs text-(--text-muted)">
                    {{ selectedSubject?.className }}
                  </span>
                </div>
                <h3 class="text-lg font-bold text-(--text-heading) mt-1">
                  {{ selectedCriterion?.questionText }}
                </h3>
              </div>

              <!-- Close Button -->
              <button
                class="rounded-lg p-2 text-(--text-muted) hover:text-(--text-heading) hover:bg-slate-200/50 dark:hover:bg-slate-800 transition"
                title="Đóng (Click bên ngoài hoặc ESC để thoát)"
                @click="closeModal"
              >
                <X :size="20" />
              </button>
            </div>

            <!-- Score Summary Bar inside Modal -->
            <div class="px-5 py-3 bg-amber-500/10 border-b border-amber-500/20 flex items-center justify-between">
              <div class="flex items-center gap-2">
                <Star :size="18" class="fill-amber-400 text-amber-400" />
                <span class="text-sm font-semibold text-(--text-heading)">Điểm trung bình mục này:</span>
                <span class="text-base font-extrabold text-amber-600 dark:text-amber-400">
                  {{ selectedCriterion?.averageScore }} / 5.0
                </span>
              </div>
              <span class="text-xs text-(--text-muted)">
                Tổng cộng: {{ selectedCriterion?.details?.length || 0 }} lượt đánh giá
              </span>
            </div>

            <!-- Modal Body: Table of Evaluations -->
            <div class="p-5 overflow-y-auto space-y-4 flex-1">
              <div v-if="selectedCriterion?.details && selectedCriterion.details.length > 0" class="overflow-x-auto">
                <table class="w-full text-left text-sm border-collapse">
                  <thead>
                    <tr class="border-b border-default text-xs font-bold uppercase text-(--text-muted)">
                      <th class="pb-3 px-2 w-12 text-center">STT</th>
                      <th class="pb-3 px-3 w-32">Số sao</th>
                      <th class="pb-3 px-3">Nội dung góp ý / Nhận xét</th>
                      <th class="pb-3 px-3 w-36 text-right">Thời gian</th>
                    </tr>
                  </thead>
                  <tbody class="divide-y divide-default/50">
                    <tr
                      v-for="(item, idx) in selectedCriterion.details"
                      :key="item.id || idx"
                      class="hover:bg-slate-50/50 dark:hover:bg-slate-900/20 transition-colors"
                    >
                      <td class="py-3 px-2 text-center font-bold text-(--text-muted) text-xs">
                        #{{ idx + 1 }}
                      </td>
                      <td class="py-3 px-3">
                        <div class="flex items-center gap-1.5">
                          <div class="flex items-center gap-0.5">
                            <Star
                              v-for="s in 5"
                              :key="s"
                              :size="13"
                              :class="s <= item.score ? 'fill-amber-400 text-amber-400' : 'text-slate-300 dark:text-slate-600'"
                            />
                          </div>
                          <span class="text-xs font-bold text-(--text-heading)">{{ item.score }}/5</span>
                        </div>
                      </td>
                      <td class="py-3 px-3">
                        <p class="text-sm text-(--text-body) italic">
                          "{{ item.feedback }}"
                        </p>
                      </td>
                      <td class="py-3 px-3 text-right text-xs text-(--text-muted) whitespace-nowrap">
                        <div class="flex items-center justify-end gap-1">
                          <Clock :size="12" />
                          <span>{{ item.createdAt }}</span>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div v-else class="py-10 text-center text-(--text-muted)">
                <EmptyState
                  title="Chưa có đánh giá nào"
                  description="Mục này hiện tại chưa có phản hồi nào từ sinh viên."
                />
              </div>
            </div>

            <!-- Modal Footer -->
            <div class="p-4 border-t border-default bg-slate-50/50 dark:bg-slate-900/30 flex items-center justify-between">
              <span class="text-xs text-(--text-muted)">
                💡 Mẹo: Nhấn phím <kbd class="px-1.5 py-0.5 text-[11px] bg-slate-200 dark:bg-slate-800 rounded font-mono">ESC</kbd> hoặc click vào chỗ trống bên ngoài để đóng
              </span>
              <button
                class="px-4 py-2 text-xs font-semibold rounded-lg bg-(--text-link) text-white hover:opacity-90 transition"
                @click="closeModal"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>
