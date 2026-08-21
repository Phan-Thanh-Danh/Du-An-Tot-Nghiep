import { defineStore } from 'pinia'
import { ref } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { ContentCouncilQuiz, QuizBuilderQuestion } from '@/types/content-council/quiz'
import { useQuestionStore } from './questionStore'


function normalizeQuiz(raw: any): ContentCouncilQuiz {
  if (!raw) return {} as ContentCouncilQuiz
  const id = raw.maDeKiemTra ?? raw.MaDeKiemTra ?? raw.id
  const loaiBe = raw.loaiDeThi ?? raw.LoaiDeThi
  const format: 'multiple_choice' | 'essay' | 'mixed' =
    loaiBe === 'trac_nghiem' ? 'multiple_choice'
    : loaiBe === 'tu_luan' ? 'essay'
    : loaiBe === 'ket_hop' ? 'mixed'
    : 'multiple_choice'

  const cfg = raw.cauHinh ?? raw.CauHinh ?? {}
  const cachTinhDat = cfg.cachTinhDat ?? cfg.CachTinhDat ?? 'theo_diem'
  const cachTinhDiemCuoi = cfg.cachTinhDiemCuoi ?? cfg.CachTinhDiemCuoi ?? 'lay_diem_cao_nhat'

  return {
    id,
    code: raw.code ?? `QZ-${id}`,
    title: raw.tieuDe ?? raw.TieuDe ?? raw.title ?? '',
    description: raw.moTa ?? raw.MoTa ?? '',
    subjectId: raw.maMonHoc ?? raw.MaMonHoc ?? raw.subjectId ?? 0,
    subjectCode: raw.maCodeMonHoc ?? raw.MaCodeMonHoc ?? raw.subjectCode ?? '',
    subjectName: raw.tenMonHoc ?? raw.TenMonHoc ?? raw.subjectName ?? '',
    status: raw.trangThai ?? raw.TrangThai ?? raw.status ?? 'draft',
    examType: 'lesson_quiz',
    format,
    durationMinutes: raw.thoiGianPhut ?? raw.ThoiGianPhut ?? 15,
    multipleChoicePercentage: raw.tyLeTracNghiem ?? raw.TyLeTracNghiem ?? 100,
    essayPercentage: raw.tyLeTuLuan ?? raw.TyLeTuLuan ?? 0,
    questionCount: raw.soCauHoi ?? raw.SoCauHoi ?? raw.tongSoCauHoi ?? 0,
    multipleChoiceQuestionCount: raw.soCauTracNghiem ?? raw.SoCauTracNghiem ?? 0,
    essayQuestionCount: raw.soCauTuLuan ?? raw.SoCauTuLuan ?? 0,
    totalScore: cfg.tongDiem ?? cfg.TongDiem ?? raw.tongDiem ?? raw.TongDiem ?? 10,
    passingScore: cfg.diemDat ?? cfg.DiemDat ?? null,
    minimumCorrectAnswers: cfg.soCauDungToiThieu ?? cfg.SoCauDungToiThieu ?? null,
    passMethod: cachTinhDat === 'theo_so_cau_dung' ? 'correct_answer_count' : 'score',
    unlimitedAttempts: cfg.khongGioiHanSoLan ?? cfg.KhongGioiHanSoLan ?? true,
    maximumAttempts: cfg.soLanLamToiDa ?? cfg.SoLanLamToiDa ?? null,
    finalScoreMethod: cachTinhDiemCuoi === 'lay_lan_cuoi' ? 'last' : (cachTinhDiemCuoi === 'lay_trung_binh' ? 'average' : 'highest'),
    shuffleQuestions: cfg.xaoTronCauHoi ?? cfg.XaoTronCauHoi ?? false,
    shuffleAnswers: cfg.xaoTronDapAn ?? cfg.XaoTronDapAn ?? false,
    showResultAfterSubmit: cfg.hienKetQuaSauKhiNop ?? cfg.HienKetQuaSauKhiNop ?? true,
    showCorrectAnswerAfterSubmit: cfg.hienDapAnDungSauKhiNop ?? cfg.HienDapAnDungSauKhiNop ?? false,
    showExplanationAfterSubmit: false,
    openAt: cfg.moLuc ?? cfg.MoLuc ?? null,
    closeAt: cfg.dongLuc ?? cfg.DongLuc ?? null,
    usageCount: 0,
    trangThaiDuyet: raw.trangThaiDuyet ?? raw.TrangThaiDuyet ?? 'nhap',
    createdAt: raw.ngayTao ?? raw.NgayTao ?? new Date().toISOString(),
    updatedAt: raw.ngayCapNhat ?? raw.NgayCapNhat ?? new Date().toISOString(),
  }
}

export const useQuizStore = defineStore('contentCouncilQuiz', () => {
  const quizzes = ref<ContentCouncilQuiz[]>([])
  const quizQuestions = ref<Record<number, QuizBuilderQuestion[]>>({})
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const questionStore = useQuestionStore()

  async function init(force = false) {
    if (!force && initialized.value) return
    loading.value = true
    error.value = null
    try {
      const res = await contentCouncilApi.getQuizzes()
      const rawData = res?.data?.items ?? res?.data?.Items ?? res?.items ?? res?.Items ?? (Array.isArray(res?.data) ? res.data : (Array.isArray(res) ? res : []))
      if (Array.isArray(rawData)) {
        quizzes.value = rawData.map(normalizeQuiz)
      } else {
        quizzes.value = []
      }
      initialized.value = true
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải bài kiểm tra'
    } finally {
      loading.value = false
    }
  }

  function reset() {
    initialized.value = false
    quizzes.value = []
    quizQuestions.value = {}
    error.value = null
    init()
  }

  function getQuizById(id: number) {
    return quizzes.value.find(q => q.id === id)
  }

  function getQuestionsForQuiz(quizId: number) {
    return quizQuestions.value[quizId] || []
  }

  async function addQuiz(q: ContentCouncilQuiz) {
    try {
      await contentCouncilApi.createQuiz({
        title: q.title,
        subjectId: q.subjectId,
        examType: q.examType,
        totalScore: q.totalScore,
        duration: q.duration,
      })
      quizzes.value.unshift(q)
      quizQuestions.value[q.id] = []
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm bài kiểm tra'
    }
  }

  async function updateQuiz(id: number, payload: Partial<ContentCouncilQuiz>) {
    try {
      await contentCouncilApi.updateQuiz(id, payload)
      const idx = quizzes.value.findIndex(q => q.id === id)
      if (idx !== -1) {
        Object.assign(quizzes.value[idx], payload)
        quizzes.value[idx].updatedAt = new Date().toISOString()
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật bài kiểm tra'
    }
  }

  async function deleteQuiz(id: number) {
    const idx = quizzes.value.findIndex(q => q.id === id)
    if (idx === -1) return
    try {
      await contentCouncilApi.deleteQuiz(id)
      const questions = quizQuestions.value[id] || []
      questions.forEach(q => questionStore.adjustUsageCount(q.questionId, -1))
      quizzes.value.splice(idx, 1)
      delete quizQuestions.value[id]
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa bài kiểm tra'
    }
  }

  async function updateQuizQuestions(quizId: number, questions: QuizBuilderQuestion[]) {
    const oldQ = quizQuestions.value[quizId] || []
    const oldIds = new Set(oldQ.map(q => q.questionId))
    const newIds = new Set(questions.map(q => q.questionId))

    newIds.forEach(id => {
      if (!oldIds.has(id)) questionStore.adjustUsageCount(id, 1)
    })
    oldIds.forEach(id => {
      if (!newIds.has(id)) questionStore.adjustUsageCount(id, -1)
    })

    try {
      await contentCouncilApi.assignQuestions(quizId, {
        questionIds: questions.map(q => q.questionId),
      })
      quizQuestions.value[quizId] = questions
      const quiz = getQuizById(quizId)
      if (quiz) {
        quiz.questionCount = questions.length
        quiz.totalScore = questions.reduce((sum, q) => sum + q.score, 0)
        quiz.multipleChoiceQuestionCount = questions.filter(q => q.questionType === 'multiple_choice').length
        quiz.essayQuestionCount = questions.filter(q => q.questionType === 'essay').length
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật câu hỏi cho bài kiểm tra'
    }
  }

  async function validateQuizAction(id: number) {
    try {
      await contentCouncilApi.validateQuiz(id)
      const q = getQuizById(id)
      if (q) q.trangThaiDuyet = 'da_xac_thuc'
    } catch (e: any) {
      error.value = e?.message || 'Không thể xác thực đề kiểm tra'
      throw e
    }
  }

  async function publishQuizAction(id: number) {
    try {
      const q = getQuizById(id)
      if (q && q.trangThaiDuyet !== 'da_xac_thuc') {
        try {
          await contentCouncilApi.validateQuiz(id)
          q.trangThaiDuyet = 'da_xac_thuc'
        } catch (vErr) {
          console.warn('Validate quiz step note:', vErr)
        }
      }
      await contentCouncilApi.publishQuiz(id)
      if (q) {
        q.status = 'published'
        q.trangThaiDuyet = 'da_xac_thuc'
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể xuất bản'
      throw e
    }
  }

  async function unpublishQuizAction(id: number) {
    try {
      await contentCouncilApi.unpublishQuiz(id)
      const q = getQuizById(id)
      if (q) {
        q.status = 'draft'
        q.trangThaiDuyet = 'nhap'
      }
    } catch (e: any) {
      error.value = e?.message || 'Không thể hủy xuất bản'
      throw e
    }
  }

  return {
    quizzes,
    quizQuestions,
    initialized,
    loading,
    error,
    init,
    reset,
    getQuizById,
    getQuestionsForQuiz,
    addQuiz,
    updateQuiz,
    deleteQuiz,
    updateQuizQuestions,
    validateQuizAction,
    publishQuizAction,
    unpublishQuizAction,
  }
})
