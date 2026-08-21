import { defineStore } from 'pinia'
import { ref } from 'vue'
import { contentCouncilApi } from '@/services/contentCouncilApi'
import type { QuestionBankItem, QuestionType, SelectionType, QuestionChoice } from '@/types/content-council/questionBank'
import type { DifficultyLevel } from '@/types/content-council/common'

const typeBeToFe: Record<string, QuestionType> = {
  trac_nghiem: 'multiple_choice',
  tu_luan: 'essay',
}
const typeFeToBe: Record<string, string> = {
  multiple_choice: 'trac_nghiem',
  essay: 'tu_luan',
}

const selBeToFe: Record<string, SelectionType> = {
  chon_mot: 'single',
  chon_nhieu: 'multiple',
}
const selFeToBe: Record<string, string> = {
  single: 'chon_mot',
  multiple: 'chon_nhieu',
}

const diffBeToFe: Record<string, DifficultyLevel> = {
  de: 'easy',
  trung_binh: 'medium',
  kho: 'hard',
}
const diffFeToBe: Record<string, string> = {
  easy: 'de',
  medium: 'trung_binh',
  hard: 'kho',
}

export function mapBeToFeQuestion(dto: any): QuestionBankItem {
  if (!dto) return {} as any

  const id = dto.maCauHoi ?? dto.MaCauHoi ?? dto.id ?? 0
  const loai = dto.loaiCauHoi ?? dto.LoaiCauHoi ?? dto.type ?? 'trac_nghiem'
  const kieu = dto.kieuLuaChon ?? dto.KieuLuaChon ?? dto.selectionType
  const doKho = dto.doKho ?? dto.DoKho ?? dto.difficulty ?? 'de'
  const subCode = dto.maCodeMonHoc ?? dto.MaCodeMonHoc ?? dto.subjectCode ?? ''
  const subName = dto.tenMonHoc ?? dto.TenMonHoc ?? dto.subjectName ?? ''

  let rawContent = dto.noiDung ?? dto.NoiDung ?? dto.content ?? ''
  if (rawContent === 'undefined' || rawContent === 'null') rawContent = ''

  return {
    id,
    code: dto.maCauHoiCode ?? dto.MaCauHoiCode ?? (subCode ? `Q-${subCode}-${id}` : `Q-${id}`),
    subjectId: dto.maMonHoc ?? dto.MaMonHoc ?? dto.subjectId ?? 0,
    subjectCode: subCode || subName,
    subjectName: subName || subCode,
    type: typeBeToFe[loai] || (loai as QuestionType) || 'multiple_choice',
    selectionType: kieu ? (selBeToFe[kieu] || (kieu as SelectionType)) : (loai === 'tu_luan' ? undefined : 'single'),
    content: rawContent,
    choices: Array.isArray(dto.luaChon) ? dto.luaChon : (Array.isArray(dto.LuaChon) ? dto.LuaChon : (Array.isArray(dto.choices) ? dto.choices : [])),
    correctAnswerIds: Array.isArray(dto.dapAnDung) ? dto.dapAnDung : (Array.isArray(dto.DapAnDung) ? dto.DapAnDung : (Array.isArray(dto.correctAnswerIds) ? dto.correctAnswerIds : [])),
    answerExplanation: dto.giaiThichDapAn ?? dto.GiaiThichDapAn ?? dto.answerExplanation ?? '',
    sampleAnswer: dto.dapAnMau ?? dto.DapAnMau ?? dto.sampleAnswer ?? '',
    difficulty: diffBeToFe[doKho] || (doKho as DifficultyLevel) || 'medium',
    status: (dto.conHoatDong === true || dto.ConHoatDong === true || dto.status === 'active') ? 'active' : 'inactive',
    usageCount: dto.soLanSuDung ?? dto.SoLanSuDung ?? dto.usageCount ?? 0,
    createdAt: dto.ngayTao ?? dto.NgayTao ?? dto.createdAt ?? new Date().toISOString(),
    updatedAt: dto.ngayCapNhat ?? dto.NgayCapNhat ?? dto.updatedAt ?? new Date().toISOString(),
  }
}

export function mapFeToBePayload(q: Partial<QuestionBankItem>) {
  const loai = q.type ? (typeFeToBe[q.type] || q.type) : 'trac_nghiem'
  const kieu = q.selectionType ? (selFeToBe[q.selectionType] || q.selectionType) : undefined
  const doKho = q.difficulty ? (diffFeToBe[q.difficulty] || q.difficulty) : 'trung_binh'

  return {
    maMonHoc: Number(q.subjectId || 0),
    loaiCauHoi: loai,
    noiDung: (q.content || '').trim(),
    kieuLuaChon: loai === 'tu_luan' ? null : (kieu || 'chon_mot'),
    luaChon: loai === 'tu_luan' ? null : (q.choices || []).map((c: QuestionChoice) => ({ id: String(c.id), content: String(c.content) })),
    dapAnDung: loai === 'tu_luan' ? null : (q.correctAnswerIds || []).map((a: string) => String(a)),
    giaiThichDapAn: q.answerExplanation || null,
    doKho: doKho,
  }
}

export const useQuestionStore = defineStore('contentCouncilQuestion', () => {
  const questions = ref<QuestionBankItem[]>([])
  const initialized = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function init() {
    if (initialized.value) return
    loading.value = true
    error.value = null
    try {
      const res = await contentCouncilApi.getQuestions({ pageSize: 100 })
      const rawData = res?.data?.items ?? res?.items ?? res?.data ?? (Array.isArray(res) ? res : [])
      questions.value = Array.isArray(rawData) ? rawData.map(mapBeToFeQuestion) : []
      initialized.value = true
    } catch (e: any) {
      error.value = e?.message || 'Không thể tải câu hỏi'
    } finally {
      loading.value = false
    }
  }

  function reset() {
    initialized.value = false
    questions.value = []
    error.value = null
    init()
  }

  function getQuestionsBySubject(subjectId: number) {
    return questions.value.filter(q => q.subjectId === subjectId)
  }

  function getQuestionById(id: number) {
    return questions.value.find(q => q.id === id)
  }

  async function addQuestion(q: Partial<QuestionBankItem>): Promise<QuestionBankItem> {
    loading.value = true
    error.value = null
    try {
      const payload = mapFeToBePayload(q)
      const res = await contentCouncilApi.createQuestion(payload)
      const rawData = res?.data ?? res?.Data ?? res
      const newQuestion = mapBeToFeQuestion(rawData)
      questions.value.unshift(newQuestion)
      return newQuestion
    } catch (e: any) {
      error.value = e?.message || 'Không thể thêm câu hỏi'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function updateQuestion(id: number, payload: Partial<QuestionBankItem>): Promise<QuestionBankItem> {
    loading.value = true
    error.value = null
    try {
      const bePayload = mapFeToBePayload(payload)
      const res = await contentCouncilApi.updateQuestion(id, bePayload)
      const rawData = res?.data ?? res?.Data ?? res
      const updatedQuestion = mapBeToFeQuestion(rawData)
      const idx = questions.value.findIndex(q => q.id === id)
      if (idx !== -1) {
        questions.value[idx] = updatedQuestion
      }
      return updatedQuestion
    } catch (e: any) {
      error.value = e?.message || 'Không thể cập nhật câu hỏi'
      throw e
    } finally {
      loading.value = false
    }
  }

  async function deleteQuestion(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx === -1) return
    if (questions.value[idx].usageCount > 0) {
      throw new Error('Không thể xóa câu hỏi đang được sử dụng.')
    }
    loading.value = true
    error.value = null
    try {
      await contentCouncilApi.deleteQuestion(id)
      questions.value.splice(idx, 1)
    } catch (e: any) {
      error.value = e?.message || 'Không thể xóa câu hỏi'
      throw e
    } finally {
      loading.value = false
    }
  }

  function adjustUsageCount(id: number, delta: number) {
    const q = questions.value.find(q => q.id === id)
    if (q) {
      q.usageCount = Math.max(0, q.usageCount + delta)
    }
  }

  async function toggleStatus(id: number) {
    const idx = questions.value.findIndex(q => q.id === id)
    if (idx === -1) return
    const currentStatus = questions.value[idx].status
    const newStatus = currentStatus === 'active' ? 'inactive' : 'active'
    try {
      if (newStatus === 'active') {
        await contentCouncilApi.activateQuestion(id)
      } else {
        await contentCouncilApi.deactivateQuestion(id)
      }
      questions.value[idx].status = newStatus
    } catch (e: any) {
      error.value = e?.message || 'Không thể đổi trạng thái câu hỏi'
      throw e
    }
  }

  return {
    questions,
    initialized,
    loading,
    error,
    init,
    reset,
    getQuestionsBySubject,
    getQuestionById,
    addQuestion,
    updateQuestion,
    deleteQuestion,
    adjustUsageCount,
    toggleStatus,
  }
})

