import { DifficultyLevel } from './common'
import { QuestionChoice } from './questionBank'

export type QuizStatus = 'draft' | 'published' | 'open' | 'closed'
export type QuizExamType = 'lesson_quiz' | 'chapter_quiz' | 'midterm' | 'final' | 'regular_test'
export type QuizFormat = 'multiple_choice' | 'essay' | 'mixed'
export type QuizPassMethod = 'score' | 'correct_answer_count'
export type QuizFinalScoreMethod = 'highest' | 'latest' | 'average'

export interface ContentCouncilQuiz {
  id: number
  code: string
  title: string
  description?: string
  subjectId: number
  subjectCode: string
  subjectName: string
  semesterId?: number
  semesterName?: string
  status: QuizStatus
  examType: QuizExamType
  format: QuizFormat
  durationMinutes: number
  questionCount: number
  multipleChoiceQuestionCount: number
  essayQuestionCount: number
  totalScore: number
  passingScore?: number
  minimumCorrectAnswers?: number
  passMethod: QuizPassMethod
  unlimitedAttempts: boolean
  maximumAttempts?: number
  finalScoreMethod: QuizFinalScoreMethod
  shuffleQuestions: boolean
  shuffleAnswers: boolean
  showResultAfterSubmit: boolean
  showCorrectAnswerAfterSubmit: boolean
  showExplanationAfterSubmit: boolean
  openAt?: string
  closeAt?: string
  usageCount: number
  createdAt: string
  updatedAt: string
}

export interface QuizBuilderQuestion {
  questionId: number
  questionCode: string
  questionContent: string
  subjectId: number
  questionType: 'multiple_choice' | 'essay'
  selectionType?: 'single' | 'multiple'
  difficulty: DifficultyLevel
  score: number
  order: number
  status: 'active' | 'inactive'
  choices?: QuestionChoice[]
  correctAnswerIds?: string[]
  answerExplanation?: string
  sampleAnswer?: string
}
