<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'
import { useRouter, useRoute, onBeforeRouteLeave } from 'vue-router'
import { QuizFormData, QuizFormMode } from '@/types/content-council/quizForm'
import { useQuizStore } from '@/stores/content-council/quizStore'
import { useQuizFormValidation } from '@/composables/content-council/useQuizFormValidation'
import { contentCouncilApi } from '@/services/contentCouncilApi'

// Components
import QuizFormHeader from '@/components/content-council/quizzes/form/QuizFormHeader.vue'
import QuizGeneralInformationSection from '@/components/content-council/quizzes/form/QuizGeneralInformationSection.vue'
import QuizStructureSection from '@/components/content-council/quizzes/form/QuizStructureSection.vue'
import QuizQuestionBankSection from '@/components/content-council/quizzes/form/QuizQuestionBankSection.vue'
import QuizPassingRulesSection from '@/components/content-council/quizzes/form/QuizPassingRulesSection.vue'
import QuizAttemptsSection from '@/components/content-council/quizzes/form/QuizAttemptsSection.vue'
import QuizScheduleSection from '@/components/content-council/quizzes/form/QuizScheduleSection.vue'
import QuizDisplayOptionsSection from '@/components/content-council/quizzes/form/QuizDisplayOptionsSection.vue'
import QuizFormSummaryCard from '@/components/content-council/quizzes/form/QuizFormSummaryCard.vue'
import QuizFormActionBar from '@/components/content-council/quizzes/form/QuizFormActionBar.vue'
import QuizFormSkeleton from '@/components/content-council/quizzes/form/QuizFormSkeleton.vue'
import QuizReadOnlyAlert from '@/components/content-council/quizzes/form/QuizReadOnlyAlert.vue'
import QuizUnsavedChangesDialog from '@/components/content-council/quizzes/form/QuizUnsavedChangesDialog.vue'
import { ChevronLeft } from 'lucide-vue-next'

const router = useRouter()
const route = useRoute()
const quizStore = useQuizStore()
quizStore.init()

const mode = computed<QuizFormMode>(() => route.params.quizId ? 'edit' : 'create')

// Initial Default Data
const getDefaultData = (): QuizFormData => ({
  subjectId: Number(route.query.subjectId) || null,
  semesterId: null,
  title: '',
  description: '',
  examType: 'lesson_quiz',
  format: 'multiple_choice',
  durationMinutes: 15,
  multipleChoicePercentage: 100,
  essayPercentage: 0,
  totalScore: 10,
  passMethod: 'score',
  passingScore: 5,
  minimumCorrectAnswers: null,
  unlimitedAttempts: false,
  maximumAttempts: 2,
  finalScoreMethod: 'highest',
  openAt: null,
  closeAt: null,
  shuffleQuestions: false,
  shuffleAnswers: false,
  showResultAfterSubmit: true,
  showCorrectAnswerAfterSubmit: false,
  showExplanationAfterSubmit: false,
  status: 'draft'
})

const formData = ref<QuizFormData>(getDefaultData())
const initialDataString = ref('')
const isLoading = ref(true)
const isSaving = ref(false)
const quizNotFound = ref(false)

// Selected question IDs from question bank
const selectedQuestionIds = ref<number[]>([])

const { 
  validationErrors, 
  sectionErrors, 
  validate, 
  resetValidation 
} = useQuizFormValidation(formData)

const isDirty = computed(() => {
  if (isLoading.value) return false
  return JSON.stringify(formData.value) !== initialDataString.value
})

const isReadOnly = computed(() => {
  if (mode.value === 'create') return false
  return formData.value.status === 'open' || formData.value.status === 'closed'
})

const hasQuestions = computed(() => {
  if (mode.value === 'create') return false
  const quiz = quizStore.getQuizById(Number(route.params.quizId))
  return quiz ? quiz.questionCount > 0 : false
})

// Load Data
onMounted(async () => {
  if (mode.value === 'edit') {
    const id = Number(route.params.quizId)
    try {
      const res = await contentCouncilApi.getQuizById(id)
      const quiz = normalizeQuizFromResponse(res?.data ?? res) || quizStore.getQuizById(id)
      if (!quiz) {
        quizNotFound.value = true
        isLoading.value = false
        return
      }

      formData.value = {
        id: quiz.id,
        code: quiz.code,
        subjectId: quiz.subjectId,
        semesterId: quiz.semesterId || null,
        title: quiz.title,
        description: quiz.description || '',
        examType: quiz.examType,
        format: quiz.format,
        durationMinutes: quiz.durationMinutes,
        multipleChoicePercentage: quiz.format === 'mixed' ? (quiz.multipleChoicePercentage ?? 70) : (quiz.format === 'essay' ? 0 : 100),
        essayPercentage: quiz.format === 'mixed' ? (quiz.essayPercentage ?? 30) : (quiz.format === 'essay' ? 100 : 0),
        totalScore: quiz.totalScore,
        passMethod: quiz.passMethod,
        passingScore: quiz.passingScore ?? null,
        minimumCorrectAnswers: quiz.minimumCorrectAnswers ?? null,
        unlimitedAttempts: quiz.unlimitedAttempts ?? true,
        maximumAttempts: quiz.maximumAttempts ?? null,
        finalScoreMethod: quiz.finalScoreMethod ?? 'highest',
        openAt: quiz.openAt || null,
        closeAt: quiz.closeAt || null,
        shuffleQuestions: quiz.shuffleQuestions ?? false,
        shuffleAnswers: quiz.shuffleAnswers ?? false,
        showResultAfterSubmit: quiz.showResultAfterSubmit ?? true,
        showCorrectAnswerAfterSubmit: quiz.showCorrectAnswerAfterSubmit ?? false,
        showExplanationAfterSubmit: false,
        status: quiz.status
      }

      // Load assigned questions for edit mode
      try {
        const qRes = await contentCouncilApi.getQuizQuestions(id)
        const qData = qRes?.data ?? qRes ?? []
        if (Array.isArray(qData) && qData.length > 0) {
          selectedQuestionIds.value = qData.map((q: any) => q.maCauHoi ?? q.MaCauHoi ?? q.questionId)
        }
      } catch (qErr) {
        console.error('Lỗi nạp danh sách câu hỏi của Quiz:', qErr)
      }
    } catch {
      quizNotFound.value = true
    }
  }

  initialDataString.value = JSON.stringify(formData.value)
  isLoading.value = false
})

// Leave Guard
const showLeaveDialog = ref(false)
let pendingRouteLocation: any = null

onBeforeRouteLeave((to, from, next) => {
  if (isDirty.value && !isSaving.value) {
    showLeaveDialog.value = true
    pendingRouteLocation = to
    next(false)
  } else {
    next()
  }
})

const confirmLeave = () => {
  showLeaveDialog.value = false
  isSaving.value = true // bypass guard
  if (pendingRouteLocation) {
    router.push(pendingRouteLocation)
  } else {
    router.push({ name: 'content-council-quizzes' })
  }
}

// Actions
const handleCancel = () => {
  if (isDirty.value) {
    showLeaveDialog.value = true
  } else {
    router.push({ name: 'content-council-quizzes' })
  }
}

const buildApiPayload = () => {
  const d = formData.value

  // Map FE format -> BE valid LoaiDeThi ('trac_nghiem' | 'tu_luan' | 'ket_hop')
  const loaiDeThi =
    d.format === 'multiple_choice' ? 'trac_nghiem'
    : d.format === 'essay' ? 'tu_luan'
    : d.format === 'mixed' ? 'ket_hop'
    : 'trac_nghiem'

  // Map FE passMethod -> BE CachTinhDat
  const cachTinhDat = d.passMethod === 'correct_answer_count' ? 'theo_so_cau_dung' : 'theo_diem'

  // Map FE finalScoreMethod -> BE CachTinhDiemCuoi
  const cachTinhDiemCuoi =
    d.finalScoreMethod === 'last' ? 'lay_lan_cuoi'
    : d.finalScoreMethod === 'average' ? 'lay_trung_binh'
    : 'lay_diem_cao_nhat'

  return {
    MaMonHoc: d.subjectId,
    MaHocKy: d.semesterId || null,
    TieuDe: d.title?.trim(),
    MoTa: d.description?.trim(),
    ThoiGianPhut: d.durationMinutes,
    LoaiDeThi: loaiDeThi,
    HinhThucThi: 'online_tu_do',
    TyLeTracNghiem: d.format === 'mixed' ? d.multipleChoicePercentage : (d.format === 'multiple_choice' ? 100 : 0),
    TyLeTuLuan: d.format === 'mixed' ? d.essayPercentage : (d.format === 'essay' ? 100 : 0),
    CauHinh: {
      MoTa: d.description?.trim(),
      TongDiem: d.totalScore,
      DiemDat: d.passMethod === 'score' ? (d.passingScore ?? 5) : 0,
      CachTinhDat: cachTinhDat,
      SoCauDungToiThieu: d.passMethod === 'correct_answer_count' ? (d.minimumCorrectAnswers ?? null) : null,
      KhongGioiHanSoLan: d.unlimitedAttempts,
      SoLanLamToiDa: d.unlimitedAttempts ? null : (d.maximumAttempts ?? null),
      CachTinhDiemCuoi: cachTinhDiemCuoi,
      MoLuc: d.openAt || null,
      DongLuc: d.closeAt || null,
      XaoTronCauHoi: d.shuffleQuestions,
      XaoTronDapAn: d.shuffleAnswers,
      HienKetQuaSauKhiNop: d.showResultAfterSubmit,
      HienDapAnDungSauKhiNop: d.showCorrectAnswerAfterSubmit,
      HienGiaiThichSauKhiNop: d.showExplanationAfterSubmit,
    }
  }
}

const normalizeQuizFromResponse = (raw: any): any => {
  if (!raw) return null
  const id = raw.MaDeKiemTra ?? raw.maDeKiemTra ?? raw.id
  if (!id) return null
  const cfg = raw.CauHinh ?? raw.cauHinh ?? {}
  
  const loaiBe = raw.LoaiDeThi ?? raw.loaiDeThi
  const format: 'multiple_choice' | 'essay' | 'mixed' =
    loaiBe === 'trac_nghiem' ? 'multiple_choice'
    : loaiBe === 'tu_luan' ? 'essay'
    : loaiBe === 'ket_hop' ? 'mixed'
    : 'multiple_choice'

  const rawStatus = raw.TrangThai ?? raw.trangThai ?? 'draft'
  const status: 'draft' | 'published' | 'open' | 'closed' =
    rawStatus === 'nhap' || rawStatus === 'draft' ? 'draft'
    : rawStatus === 'dang_mo' || rawStatus === 'published' || rawStatus === 'da_xuat_ban' || rawStatus === 'hoat_dong' ? 'published'
    : rawStatus === 'da_dong' || rawStatus === 'closed' ? 'closed'
    : rawStatus

  return {
    id,
    code: `QZ-${id}`,
    title: raw.TieuDe ?? raw.tieuDe ?? raw.title ?? '',
    description: raw.MoTa ?? raw.moTa ?? cfg.MoTa ?? cfg.moTa ?? '',
    subjectId: raw.MaMonHoc ?? raw.maMonHoc ?? raw.subjectId ?? 0,
    subjectCode: raw.MaCodeMonHoc ?? raw.maCodeMonHoc ?? '',
    subjectName: raw.TenMonHoc ?? raw.tenMonHoc ?? '',
    semesterId: raw.MaHocKy ?? raw.maHocKy ?? null,
    status,
    examType: 'lesson_quiz',
    format,
    durationMinutes: raw.ThoiGianPhut ?? raw.thoiGianPhut ?? 15,
    multipleChoicePercentage: raw.TyLeTracNghiem ?? raw.tyLeTracNghiem ?? 100,
    essayPercentage: raw.TyLeTuLuan ?? raw.tyLeTuLuan ?? 0,
    questionCount: raw.TongSoCauHoi ?? raw.tongSoCauHoi ?? raw.SoCauHoi ?? raw.soCauHoi ?? 0,
    multipleChoiceQuestionCount: raw.SoCauTracNghiem ?? raw.soCauTracNghiem ?? 0,
    essayQuestionCount: raw.SoCauTuLuan ?? raw.soCauTuLuan ?? 0,
    totalScore: cfg.TongDiem ?? cfg.tongDiem ?? 10,
    passingScore: cfg.DiemDat ?? cfg.diemDat ?? null,
    minimumCorrectAnswers: cfg.SoCauDungToiThieu ?? cfg.soCauDungToiThieu ?? null,
    passMethod: (cfg.CachTinhDat ?? cfg.cachTinhDat) === 'theo_so_cau_dung' ? 'correct_answer_count' : 'score',
    unlimitedAttempts: cfg.KhongGioiHanSoLan ?? cfg.khongGioiHanSoLan ?? false,
    maximumAttempts: cfg.SoLanLamToiDa ?? cfg.soLanLamToiDa ?? null,
    finalScoreMethod:
      (cfg.CachTinhDiemCuoi ?? cfg.cachTinhDiemCuoi) === 'lay_lan_cuoi' ? 'last'
      : (cfg.CachTinhDiemCuoi ?? cfg.cachTinhDiemCuoi) === 'lay_trung_binh' ? 'average'
      : 'highest',
    shuffleQuestions: cfg.XaoTronCauHoi ?? cfg.xaoTronCauHoi ?? false,
    shuffleAnswers: cfg.XaoTronDapAn ?? cfg.xaoTronDapAn ?? false,
    showResultAfterSubmit: cfg.HienKetQuaSauKhiNop ?? cfg.hienKetQuaSauKhiNop ?? true,
    showCorrectAnswerAfterSubmit: cfg.HienDapAnDungSauKhiNop ?? cfg.hienDapAnDungSauKhiNop ?? false,
    showExplanationAfterSubmit: cfg.HienGiaiThichSauKhiNop ?? cfg.hienGiaiThichSauKhiNop ?? false,
    openAt: cfg.MoLuc ?? cfg.moLuc ?? null,
    closeAt: cfg.DongLuc ?? cfg.dongLuc ?? null,
    usageCount: 0,
    trangThaiDuyet: raw.TrangThaiDuyet ?? raw.trangThaiDuyet ?? 'nhap',
    createdAt: raw.NgayTao ?? raw.ngayTao ?? new Date().toISOString(),
    updatedAt: raw.NgayCapNhat ?? raw.ngayCapNhat ?? new Date().toISOString(),
  }
}
function distributeScoreEvenly(totalScore: number, count: number): number[] {
  if (count <= 0) return []
  if (count === 1) return [totalScore]

  const totalCents = Math.round(totalScore * 100)
  const baseCents = Math.floor(totalCents / count)
  let remainderCents = totalCents - (baseCents * count)

  const scores: number[] = []
  for (let i = 0; i < count; i++) {
    let cents = baseCents
    if (remainderCents > 0) {
      cents += 1
      remainderCents -= 1
    }
    scores.push(cents / 100)
  }
  return scores
}

const saveDraft = async (): Promise<number | false> => {
  const { isValid } = validate()
  
  if (!isValid) {
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return false
  }

  isSaving.value = true
  
  try {
    const payload = buildApiPayload()
    let savedId: number
    
    if (mode.value === 'create') {
      // Call real create API
      const res = await contentCouncilApi.createQuiz(payload)
      const rawQuiz = res?.data ?? res
      const normalized = normalizeQuizFromResponse(rawQuiz)
      
      if (!normalized?.id) {
        throw new Error('Không nhận được ID quiz từ server')
      }
      
      savedId = normalized.id
      
      // Push into store with real data
      quizStore.quizzes.unshift(normalized)
      quizStore.quizQuestions[savedId] = []
      
      formData.value.id = savedId
      formData.value.code = normalized.code
    } else {
      // Call real update API
      const existingId = formData.value.id as number
      await contentCouncilApi.updateQuiz(existingId, payload)
      
      savedId = existingId
      
      // Refresh quiz in store from server
      const refreshRes = await contentCouncilApi.getQuizById(existingId)
      const refreshed = normalizeQuizFromResponse(refreshRes?.data ?? refreshRes)
      if (refreshed) {
        const idx = quizStore.quizzes.findIndex((q: any) => q.id === savedId)
        if (idx !== -1) {
          quizStore.quizzes[idx] = refreshed
        } else {
          quizStore.quizzes.unshift(refreshed)
        }
      }
    }
    
    initialDataString.value = JSON.stringify(formData.value)



    // Save selected question bank items if any are selected
    if (selectedQuestionIds.value && selectedQuestionIds.value.length > 0) {
      const total = formData.value.totalScore || 10
      const count = selectedQuestionIds.value.length
      const scores = distributeScoreEvenly(total, count)
      
      const items = selectedQuestionIds.value.map((qId, idx) => ({
        MaCauHoi: qId,
        maCauHoi: qId,
        DiemSo: scores[idx],
        diemSo: scores[idx],
        ThuTu: idx + 1,
        thuTu: idx + 1
      }))

      const questionsPayload = {
        Questions: items,
        questions: items
      }

      await contentCouncilApi.replaceQuestions(savedId, questionsPayload)
    }

    return savedId
  } catch (err: any) {
    console.error('Quiz save error:', err)
    // Show error toast
    const msg = document.createElement('div')
    msg.className = 'fixed bottom-24 right-6 bg-red-700 text-white px-4 py-3 rounded-lg shadow-lg z-50 text-sm'
    msg.textContent = `Lỗi lưu Quiz: ${err?.message || 'Vui lòng thử lại.'}`
    document.body.appendChild(msg)
    setTimeout(() => msg.remove(), 4000)
    return false
  } finally {
    isSaving.value = false
  }
}

const handleSaveDraft = async () => {
  const savedId = await saveDraft()
  if (savedId) {
    const msg = document.createElement('div')
    msg.className = 'fixed bottom-24 right-6 bg-slate-800 text-white px-4 py-3 rounded-lg shadow-lg z-50 text-sm animate-fade-in-up'
    msg.textContent = mode.value === 'create' ? 'Đã tạo Quiz bản nháp thành công!' : 'Đã cập nhật Quiz thành công!'
    document.body.appendChild(msg)
    setTimeout(() => msg.remove(), 3000)
    
    if (mode.value === 'create') {
      router.push({ name: 'content-council-quizzes' })
    }
  }
}

const handleSaveAndBuild = async () => {
  const savedId = await saveDraft()
  if (savedId) {
    router.push({ name: 'content-council-quiz-builder', params: { quizId: savedId } })
  }
}
</script>

<template>
  <div class="min-h-full pb-32">
    
    <!-- 404 Not Found -->
    <div v-if="quizNotFound" class="p-6 max-w-2xl mx-auto text-center py-20">
      <div class="bg-slate-100 w-20 h-20 rounded-full flex items-center justify-center mx-auto mb-4">
        <span class="text-3xl text-slate-400">?</span>
      </div>
      <h2 class="text-2xl font-bold text-slate-800 mb-2">Không tìm thấy Quiz</h2>
      <p class="text-slate-600 mb-6">Quiz bạn đang tìm không tồn tại hoặc đã bị xóa.</p>
      <button 
        @click="router.push({ name: 'content-council-quizzes' })"
        class="px-5 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition-colors inline-flex items-center gap-2"
      >
        <ChevronLeft class="w-4 h-4" />
        Quay lại danh sách Quiz
      </button>
    </div>

    <!-- Main Form Content -->
    <template v-else>
      <div class="p-6 max-w-[1200px] mx-auto w-full flex flex-col">
        
        <button 
          @click="handleCancel"
          class="flex items-center gap-1 text-sm font-medium text-slate-500 hover:text-slate-800 transition-colors w-max mb-4"
        >
          <ChevronLeft class="w-4 h-4" />
          Quiz / Đề kiểm tra
        </button>

        <!-- Skeleton -->
        <QuizFormSkeleton v-if="isLoading" />

        <!-- Form Layout -->
        <div v-else>
          <QuizFormHeader :mode="mode" />
          
          <QuizReadOnlyAlert :status="formData.status" />

          <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
            
            <!-- Left Column: Form Sections -->
            <div class="lg:col-span-8 space-y-6">
              <QuizGeneralInformationSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
                :has-questions="hasQuestions"
              />
              
              <QuizStructureSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizQuestionBankSection
                :subject-id="formData.subjectId"
                :format="formData.format"
                :is-read-only="isReadOnly"
                v-model:selectedQuestionIds="selectedQuestionIds"
              />
              
              <QuizPassingRulesSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
                :has-questions="hasQuestions"
              />
              
              <QuizAttemptsSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizScheduleSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
                :errors="validationErrors"
              />
              
              <QuizDisplayOptionsSection 
                v-model="formData" 
                :is-read-only="isReadOnly"
              />
            </div>

            <!-- Right Column: Summary Sticky -->
            <div class="lg:col-span-4">
              <QuizFormSummaryCard 
                :form-data="formData"
                :has-questions="hasQuestions"
              />
            </div>

          </div>
        </div>

      </div>

      <!-- Action Bar -->
      <QuizFormActionBar 
        v-if="!quizNotFound && !isLoading"
        :mode="mode"
        :is-dirty="isDirty"
        :is-saving="isSaving"
        :is-read-only="isReadOnly"
        @cancel="handleCancel"
        @save-draft="handleSaveDraft"
        @save-and-build="handleSaveAndBuild"
      />
    </template>

    <QuizUnsavedChangesDialog 
      :is-open="showLeaveDialog"
      @close="showLeaveDialog = false"
      @confirm="confirmLeave"
    />
  </div>
</template>

<style scoped>
.animate-fade-in-up {
  animation: fadeInUp 0.2s ease-out forwards;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
