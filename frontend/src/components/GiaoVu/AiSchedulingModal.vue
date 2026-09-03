<script setup>
import { ref, computed, watch, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  Sparkles,
  Bot,
  X,
  Brain,
  CheckCircle2,
  AlertCircle,
  Clock,
  Calendar,
  Layers,
  Building2,
  ArrowRight,
  Loader2,
  TrendingUp,
  ShieldCheck,
  Zap,
  RotateCcw,
  ExternalLink
} from 'lucide-vue-next'
import { aiApi } from '@/services/aiApi.js'
import { scheduleApi } from '@/services/scheduleApi.js'
import { usePopupStore } from '@/stores/popup.js'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  campusId: { type: Number, default: 14 },
  termId: { type: Number, default: null },
  campusName: { type: String, default: 'Cơ sở TP.HCM' },
  termName: { type: String, default: 'Học kỳ 2 năm 2027' },
  availableTerms: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'draftGenerated'])
const router = useRouter()
const popupStore = usePopupStore()

// State
const selectedTermId = ref(props.termId)
const prompt = ref('')
const interpreting = ref(false)
const interpretation = ref(null)
const generating = ref(false)
const generationProgress = ref({
  theHeHienTai: 0,
  tongTheHe: 100,
  bestFitness: null,
  xepDuoc: null,
  khongXepDuoc: null,
  trangThai: 'pending'
})
let progressTimer = null

const generatedDraft = ref(null)
const explainingDraft = ref(false)
const draftExplanation = ref(null)
const errorMessage = ref('')
const readinessWarning = ref(null)

// Quick Prompt Chips
const promptChips = [
  {
    label: '🎒 Ưu tiên Sinh viên',
    desc: 'Hạn chế ca tối & tránh trống tiết',
    text: 'Xếp lịch học kỳ tới, ưu tiên sinh viên ít bị trống tiết và hạn chế ca tối sau 18h.'
  },
  {
    label: '👨‍🏫 Ưu tiên Giảng viên',
    desc: 'Theo nguyện vọng & tránh dồn tải',
    text: 'Xếp lịch ưu tiên nguyện vọng ca dạy của giảng viên, phân bổ tải đều trong tuần, tránh quá 3 ca mỗi ngày.'
  },
  {
    label: '⚖️ Cân bằng toàn diện',
    desc: 'Tối ưu giảng viên, sinh viên & phòng',
    text: 'Xếp lịch cân bằng toàn diện giữa sinh viên, giảng viên và tối ưu hóa công suất sử dụng phòng học.'
  }
]

function applyChip(chip) {
  prompt.value = chip.text
  interpretation.value = null
  errorMessage.value = ''
}

// Reset modal state
watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    selectedTermId.value = props.termId
    interpretation.value = null
    generating.value = false
    generatedDraft.value = null
    draftExplanation.value = null
    errorMessage.value = ''
    readinessWarning.value = null
  } else {
    stopProgressPolling()
  }
})

watch(() => props.termId, (newTerm) => {
  if (newTerm) selectedTermId.value = newTerm
})

watch(selectedTermId, () => {
  if (interpretation.value && prompt.value.trim()) {
    handleInterpret()
  }
})

// Step 1: AI Intent Interpretation
async function handleInterpret() {
  if (!prompt.value.trim()) return
  interpreting.value = true
  errorMessage.value = ''
  readinessWarning.value = null

  try {
    const res = await aiApi.interpretSchedulingIntent({
      message: prompt.value.trim(),
      campusId: props.campusId,
      semesterId: selectedTermId.value || props.termId
    })
    interpretation.value = res

    if (res?.canPrepareSchedule === false && res?.validationErrors?.length) {
      // Fetch readiness explanation
      const readRes = await aiApi.explainSchedulingReadiness({
        reasonCode: 'READINESS_BLOCKED',
        rawMessage: res.validationErrors.join('. '),
        campusId: props.campusId,
        semesterId: selectedTermId.value || props.termId
      })
      readinessWarning.value = readRes
    }
  } catch (err) {
    errorMessage.value = err?.message || 'Không thể phân tích yêu cầu xếp lịch bằng AI.'
  } finally {
    interpreting.value = false
  }
}

// Step 2: Trigger Generate (Borrowing the Genetic Algorithm Solver)
async function handleStartGeneration() {
  generating.value = true
  errorMessage.value = ''
  const clientDraftId = crypto.randomUUID()

  generationProgress.value = {
    draftId: clientDraftId,
    trangThai: 'pending',
    theHeHienTai: 0,
    tongTheHe: 100,
    bestFitness: null,
    xepDuoc: null,
    khongXepDuoc: null
  }

  startProgressPolling(clientDraftId)

  try {
    const profile = interpretation.value?.profile || 'balanced'
    const res = await scheduleApi.generateDraft({
      maHocKy: selectedTermId.value || interpretation.value?.semesterId || props.termId,
      maDonVi: props.campusId || interpretation.value?.campusId,
      profile: profile,
      tongTheHe: 100,
      kichThuocQuanThe: 50,
      tyLeCheo: 0.5,
      doTuoiThoToiDa: 10,
      clientDraftId
    })

    const draft = res?.data ?? res?.Data ?? res
    generatedDraft.value = draft
    emit('draftGenerated', draft)

    // Step 3: Call AI to explain the newly generated draft
    await fetchDraftExplanation(draft.draftId || draft.DraftId || clientDraftId)
  } catch (err) {
    errorMessage.value = err?.message || 'Có lỗi xảy ra trong quá trình xếp lịch bằng thuật toán.'
    generating.value = false
    stopProgressPolling()
  }
}

// Progress Polling
function startProgressPolling(draftId) {
  stopProgressPolling()
  progressTimer = setInterval(async () => {
    try {
      const p = await scheduleApi.getGenerationProgress(draftId)
      if (p) {
        generationProgress.value = { ...generationProgress.value, ...p }
      }
      if (p?.trangThai === 'hoan_tat' || p?.trangThai === 'draft') {
        stopProgressPolling()
        generating.value = false
      }
    } catch {
      // keep polling until completed or timeout
    }
  }, 600)
}

function stopProgressPolling() {
  if (progressTimer) {
    clearInterval(progressTimer)
    progressTimer = null
  }
}

// Step 3: Fetch Draft Facts & Explanation
async function fetchDraftExplanation(draftId) {
  explainingDraft.value = true
  try {
    const expRes = await aiApi.explainSchedulingDraft({
      draftId,
      campusId: props.campusId
    })
    draftExplanation.value = expRes
  } catch (err) {
    console.warn('Cannot fetch AI draft explanation:', err)
  } finally {
    explainingDraft.value = false
  }
}

// Navigation
function navigateToDrafts() {
  emit('close')
  router.push('/staff/schedule/pending-schedules')
}

function navigateToReadinessAction() {
  emit('close')
  if (readinessWarning.value?.actionRoute) {
    router.push(readinessWarning.value.actionRoute)
  }
}

const progressPercentage = computed(() => {
  const current = generationProgress.value.theHeHienTai || 0
  const total = generationProgress.value.tongTheHe || 100
  return Math.min(100, Math.round((current / total) * 100))
})

onUnmounted(() => {
  stopProgressPolling()
})
</script>

<template>
  <Teleport to="body">
    <transition name="modal-fade">
      <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-md">
        <div
          class="w-full max-w-2xl max-h-[90vh] flex flex-col rounded-3xl border border-(--border-card) bg-(--surface-card) text-(--text-body) shadow-2xl overflow-hidden transition-all"
        >
          <!-- Header -->
          <div class="px-6 py-4 border-b border-(--border-default) bg-(--surface-modal) flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="h-10 w-10 rounded-2xl bg-gradient-to-tr from-blue-600 via-indigo-600 to-cyan-500 flex items-center justify-center text-white shadow-md shadow-indigo-500/25">
                <Sparkles :size="20" class="animate-pulse" />
              </div>
              <div>
                <div class="flex items-center gap-2">
                  <h3 class="text-base font-bold text-heading">Trợ Lý Xếp Lịch Thông Minh (AI)</h3>
                  <span class="px-2 py-0.5 text-[10px] font-extrabold tracking-wider uppercase rounded-full bg-indigo-500/15 text-indigo-600 dark:text-indigo-400 border border-indigo-500/30">
                    Engine GA + AI
                  </span>
                </div>
                <div class="flex items-center gap-2 mt-0.5 text-xs text-muted">
                  <span class="flex items-center gap-1"><Building2 :size="12" /> {{ campusName }}</span>
                  <span>•</span>
                  <div class="flex items-center gap-1 font-semibold text-heading">
                    <Calendar :size="12" />
                    <select
                      v-if="availableTerms && availableTerms.length > 0"
                      v-model="selectedTermId"
                      class="bg-(--surface-input) font-bold text-xs text-heading border border-(--border-default) rounded-lg px-2 py-0.5 focus:outline-none focus:ring-1 focus:ring-indigo-500 cursor-pointer"
                    >
                      <option
                        v-for="t in availableTerms"
                        :key="t.value || t.maHocKy"
                        :value="t.value || t.maHocKy"
                        class="bg-(--surface-card) text-heading"
                      >
                        {{ t.label || t.tenHocKy }}
                      </option>
                    </select>
                    <span v-else>{{ termName }}</span>
                  </div>
                </div>
              </div>
            </div>
            <button
              @click="emit('close')"
              class="p-2 rounded-xl text-muted hover:text-heading hover:bg-(--surface-input) transition-colors cursor-pointer"
            >
              <X :size="20" />
            </button>
          </div>

          <!-- Body -->
          <div class="flex-1 overflow-y-auto p-6 space-y-5">
            <!-- Readiness Warning Alert (if any) -->
            <div
              v-if="readinessWarning"
              class="p-4 rounded-2xl bg-amber-500/10 border border-amber-500/30 text-amber-700 dark:text-amber-300 space-y-2"
            >
              <div class="flex items-center gap-2 font-bold text-sm">
                <AlertCircle :size="18" class="shrink-0" />
                <span>Điều kiện sẵn sàng chưa hoàn tất ({{ readinessWarning.reasonCode }})</span>
              </div>
              <p class="text-xs leading-relaxed opacity-90">
                {{ readinessWarning.humanExplanation }}
              </p>
              <div class="pt-1 flex items-center justify-between">
                <span class="text-xs font-semibold">{{ readinessWarning.recommendedAction }}</span>
                <GlassButton variant="secondary" size="sm" @click="navigateToReadinessAction">
                  <ExternalLink :size="13" class="mr-1" />
                  {{ readinessWarning.actionLabel || 'Khắc phục ngay' }}
                </GlassButton>
              </div>
            </div>

            <!-- Error Banner -->
            <div
              v-if="errorMessage"
              class="p-3.5 rounded-xl bg-red-500/10 border border-red-500/30 text-red-600 dark:text-red-400 text-xs flex items-center gap-2"
            >
              <AlertCircle :size="16" class="shrink-0" />
              <span>{{ errorMessage }}</span>
            </div>

            <!-- STAGE 1: Prompt Input (Active before generation or reset) -->
            <div v-if="!generating && !generatedDraft" class="space-y-4">
              <div>
                <label class="block text-xs font-bold text-heading uppercase tracking-wider mb-2">
                  1. Nhập yêu cầu xếp lịch bằng ngôn ngữ tự nhiên:
                </label>
                <div class="relative">
                  <textarea
                    v-model="prompt"
                    rows="3"
                    placeholder="Ví dụ: Xếp lịch học kỳ tới, ưu tiên sinh viên ít bị trống tiết và hạn chế ca tối sau 18h..."
                    class="w-full px-4 py-3 rounded-2xl border border-(--border-input) bg-(--surface-input) text-body text-xs focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-all resize-none shadow-2xs"
                  ></textarea>
                </div>
              </div>

              <!-- Quick Prompt Chips -->
              <div class="space-y-1.5">
                <span class="text-[11px] font-semibold text-muted">💡 Gợi ý kịch bản kiểm thử nhanh:</span>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-2">
                  <button
                    v-for="(chip, idx) in promptChips"
                    :key="idx"
                    type="button"
                    @click="applyChip(chip)"
                    class="p-2.5 rounded-xl border border-(--border-default) bg-(--surface-modal) hover:border-indigo-400 dark:hover:border-indigo-500 hover:bg-(--surface-input) text-left transition-all cursor-pointer group"
                  >
                    <div class="font-bold text-xs text-heading group-hover:text-indigo-600 dark:group-hover:text-indigo-400">{{ chip.label }}</div>
                    <div class="text-[10.5px] text-muted truncate mt-0.5">{{ chip.desc }}</div>
                  </button>
                </div>
              </div>

              <!-- Interpret Button -->
              <div class="pt-1">
                <button
                  type="button"
                  @click="handleInterpret"
                  :disabled="interpreting || !prompt.trim()"
                  class="w-full py-3 rounded-2xl bg-gradient-to-r from-blue-600 via-indigo-600 to-indigo-700 hover:from-blue-700 hover:to-indigo-800 text-white font-bold text-xs shadow-lg shadow-indigo-500/25 flex items-center justify-center gap-2 transition-all active:scale-[0.99] disabled:opacity-50 cursor-pointer"
                >
                  <Loader2 v-if="interpreting" :size="16" class="animate-spin" />
                  <Brain v-else :size="16" />
                  <span>{{ interpreting ? 'AI ĐANG PHÂN TÍCH YÊU CẦU...' : '⚡ PHÂN TÍCH & THIẾT LẬP BẰNG AI' }}</span>
                </button>
              </div>

              <!-- Interpretation Result Card (Confirmation Gate) -->
              <transition name="fade">
                <div
                  v-if="interpretation"
                  class="p-4 rounded-2xl border border-indigo-500/30 bg-indigo-500/5 dark:bg-indigo-500/10 space-y-3 shadow-xs"
                >
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                      <ShieldCheck :size="18" class="text-indigo-600 dark:text-indigo-400" />
                      <span class="font-bold text-xs text-heading">Kế Hoạch Xếp Lịch Do AI Đề Xuất</span>
                    </div>
                    <span class="px-2.5 py-0.5 rounded-full text-[10.5px] font-bold bg-indigo-600 text-white shadow-2xs">
                      Hồ sơ: {{ interpretation.profileDisplayName }}
                    </span>
                  </div>

                  <p class="text-xs text-body leading-relaxed">
                    {{ interpretation.summary }}
                  </p>

                  <div class="p-3 rounded-xl bg-(--surface-card) border border-(--border-default) space-y-1.5 text-xs">
                    <div class="font-semibold text-heading flex items-center gap-1.5 text-[11.5px]">
                      <CheckCircle2 :size="14" class="text-emerald-500" />
                      <span>Các ràng buộc tối ưu được ghi nhận:</span>
                    </div>
                    <ul class="space-y-1 pl-5 list-disc text-muted text-[11px]">
                      <li v-for="(pref, pIdx) in interpretation.requestedPreferences" :key="pIdx">
                        {{ pref }}
                      </li>
                      <li>Khối lượng: <strong class="text-heading">{{ interpretation.schedulableCourseCount }} khóa học</strong> sẵn sàng xếp phòng.</li>
                    </ul>
                  </div>

                  <!-- Confirmation Action -->
                  <div class="pt-2 flex items-center justify-between gap-3">
                    <span class="text-[11px] text-muted italic">
                      * Thuật toán Di truyền (GA) sẽ được kích hoạt để sinh bản nháp.
                    </span>
                    <button
                      type="button"
                      @click="handleStartGeneration"
                      :disabled="generating || interpretation.canPrepareSchedule === false"
                      class="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs shadow-md shadow-emerald-600/25 flex items-center gap-2 transition-all active:scale-95 disabled:opacity-50 cursor-pointer shrink-0"
                    >
                      <Zap :size="15" />
                      <span>XÁC NHẬN & BẮT ĐẦU XẾP LỊCH</span>
                    </button>
                  </div>
                </div>
              </transition>
            </div>

            <!-- STAGE 2: Progress Screen (Running GA Solver) -->
            <div v-else-if="generating" class="py-10 flex flex-col items-center justify-center text-center space-y-5">
              <div class="relative">
                <div class="h-20 w-20 rounded-3xl bg-gradient-to-tr from-blue-600 via-indigo-600 to-cyan-400 animate-spin flex items-center justify-center p-0.5 shadow-xl shadow-indigo-500/30">
                  <div class="h-full w-full bg-(--surface-card) rounded-[22px] flex items-center justify-center">
                    <Brain :size="32" class="text-indigo-600 dark:text-indigo-400 animate-pulse" />
                  </div>
                </div>
              </div>

              <div class="space-y-2 max-w-md w-full">
                <h4 class="text-base font-bold text-heading">Thuật toán GA đang tối ưu hóa lịch học...</h4>
                <p class="text-xs text-muted">
                  Đang chạy thế hệ <span class="font-bold text-heading">{{ generationProgress.theHeHienTai }}</span> / {{ generationProgress.tongTheHe }}
                </p>

                <!-- Progress Bar -->
                <div class="w-full bg-(--surface-input) rounded-full h-3 overflow-hidden border border-(--border-default)">
                  <div
                    class="bg-gradient-to-r from-blue-600 to-indigo-600 h-full rounded-full transition-all duration-300 shadow-sm"
                    :style="{ width: `${progressPercentage}%` }"
                  ></div>
                </div>

                <div class="flex items-center justify-between text-[11px] text-muted pt-1">
                  <span>Tiến độ: {{ progressPercentage }}%</span>
                  <span v-if="generationProgress.bestFitness">Độ phù hợp (Fitness): {{ generationProgress.bestFitness.toFixed(1) }}</span>
                </div>
              </div>
            </div>

            <!-- STAGE 3: Draft Explanation & Review (After GA completes) -->
            <div v-else-if="generatedDraft" class="space-y-4">
              <!-- Success Banner -->
              <div class="p-4 rounded-2xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-700 dark:text-emerald-300 flex items-center justify-between">
                <div class="flex items-center gap-2.5">
                  <div class="h-8 w-8 rounded-xl bg-emerald-500 text-white flex items-center justify-center shadow-xs">
                    <CheckCircle2 :size="18" />
                  </div>
                  <div>
                    <h4 class="text-sm font-bold">Thời khóa biểu đã được xếp thành công!</h4>
                    <p class="text-xs opacity-90">Bản nháp đã được lưu vào hệ thống để người dùng xem trước và duyệt.</p>
                  </div>
                </div>
                <span class="text-xs font-mono font-bold bg-(--surface-card) px-2.5 py-1 rounded-lg border border-emerald-500/20 text-heading">
                  #{{ (generatedDraft.draftId || generatedDraft.DraftId || '').substring(0, 8) }}
                </span>
              </div>

              <!-- Real Facts KPI Cards -->
              <div v-if="draftExplanation?.facts" class="grid grid-cols-2 sm:grid-cols-4 gap-2.5">
                <div class="p-3 rounded-xl bg-(--surface-modal) border border-(--border-default) text-center">
                  <div class="text-lg font-extrabold text-emerald-600 dark:text-emerald-400">
                    {{ draftExplanation.facts.assignedCourses }} / {{ draftExplanation.facts.totalCourses }}
                  </div>
                  <div class="text-[10.5px] text-muted mt-0.5">Khóa học đã xếp</div>
                </div>

                <div class="p-3 rounded-xl bg-(--surface-modal) border border-(--border-default) text-center">
                  <div class="text-lg font-extrabold text-blue-600 dark:text-blue-400">
                    {{ draftExplanation.facts.successRate }}%
                  </div>
                  <div class="text-[10.5px] text-muted mt-0.5">Tỷ lệ thành công</div>
                </div>

                <div class="p-3 rounded-xl bg-(--surface-modal) border border-(--border-default) text-center">
                  <div class="text-lg font-extrabold text-indigo-600 dark:text-indigo-400">
                    {{ draftExplanation.facts.eveningShiftsCount }}
                  </div>
                  <div class="text-[10.5px] text-muted mt-0.5">Buổi học ca tối</div>
                </div>

                <div class="p-3 rounded-xl bg-(--surface-modal) border border-(--border-default) text-center">
                  <div class="text-lg font-extrabold text-purple-600 dark:text-purple-400">
                    {{ draftExplanation.facts.hardConflictsCount }}
                  </div>
                  <div class="text-[10.5px] text-muted mt-0.5">Xung đột lịch</div>
                </div>
              </div>

              <!-- AI Strategic Review Box -->
              <div class="p-4 rounded-2xl bg-gradient-to-br from-indigo-500/5 via-blue-500/5 to-cyan-500/5 border border-indigo-500/20 space-y-2">
                <div class="flex items-center gap-2">
                  <Sparkles :size="16" class="text-indigo-600 dark:text-indigo-400" />
                  <h5 class="text-xs font-bold text-heading uppercase tracking-wider">AI Đánh Giá & Nhận Xét Bản Nháp:</h5>
                </div>
                <div v-if="explainingDraft" class="flex items-center gap-2 text-xs text-muted py-2">
                  <Loader2 :size="14" class="animate-spin text-indigo-600" />
                  <span>Đang tổng hợp phân tích từ số liệu thực tế...</span>
                </div>
                <p v-else class="text-xs text-body leading-relaxed whitespace-pre-line">
                  {{ draftExplanation?.aiExplanation }}
                </p>

                <!-- Highlights -->
                <div v-if="draftExplanation?.facts?.highlightNotes?.length" class="pt-2 border-t border-(--border-default) space-y-1">
                  <div v-for="(note, nIdx) in draftExplanation.facts.highlightNotes" :key="nIdx" class="text-[11px] text-muted flex items-start gap-1.5">
                    <span class="text-indigo-600 font-bold">•</span>
                    <span>{{ note }}</span>
                  </div>
                </div>
              </div>

              <!-- Action buttons -->
              <div class="pt-2 flex items-center justify-between gap-3">
                <GlassButton variant="ghost" size="sm" @click="generatedDraft = null">
                  <RotateCcw :size="14" class="mr-1" />
                  Thử prompt khác
                </GlassButton>
                <button
                  type="button"
                  @click="navigateToDrafts"
                  class="px-5 py-2.5 rounded-xl bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white font-bold text-xs shadow-md shadow-indigo-500/25 flex items-center gap-1.5 cursor-pointer"
                >
                  <span>XEM CHI TIẾT & DUYỆT BẢN NHÁP</span>
                  <ArrowRight :size="15" />
                </button>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="px-6 py-3 border-t border-(--border-default) bg-(--surface-modal) flex items-center justify-between text-xs text-muted">
            <span>Mô hình AI: Phân tích chuyên sâu (Local)</span>
            <button
              @click="emit('close')"
              class="px-3 py-1.5 rounded-lg hover:bg-(--surface-input) text-heading font-medium cursor-pointer"
            >
              Đóng
            </button>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
  transform: scale(0.97);
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
