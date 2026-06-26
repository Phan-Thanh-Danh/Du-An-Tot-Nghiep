export type QuestionType = 'multiple_choice' | 'essay'
export type SelectionType = 'single' | 'multiple'
export type QuestionDifficulty = 'easy' | 'medium' | 'hard'
export type QuestionStatus = 'active' | 'inactive'

export interface QuestionChoice {
  id: string
  content: string
}

export interface QuestionBankItem {
  id: number
  code: string
  subjectId: number
  subjectCode: string
  subjectName: string
  type: QuestionType
  selectionType?: SelectionType
  content: string
  choices?: QuestionChoice[]
  correctAnswerIds?: string[]
  answerExplanation?: string
  sampleAnswer?: string
  difficulty: QuestionDifficulty
  status: QuestionStatus
  usageCount: number
  createdAt: string
  updatedAt: string
}
