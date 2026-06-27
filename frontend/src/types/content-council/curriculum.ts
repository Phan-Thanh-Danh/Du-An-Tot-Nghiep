import { ContentBlockType, ContentStatus } from './common'

export interface EditorContentBlock {
  id: number
  lessonId: number
  type: ContentBlockType
  title: string
  order: number
  status: ContentStatus
  html?: string
  description?: string
  fileName?: string
  fileUrl?: string
  fileSize?: number
  fileType?: string
  videoUrl?: string
  durationSeconds?: number
  quizId?: number
  quizTitle?: string
  quizQuestionCount?: number
  quizDurationMinutes?: number
  quizTotalScore?: number
  quizCompletionRule?: 'pass' | 'submit'
  // properties for slide_html
  NoiDungJson?: string
  NoiDungHtml?: string
}

export interface EditorLesson {
  id: number
  chapterId: number
  title: string
  order: number
  type: string
  status: ContentStatus | 'empty'
  contents: EditorContentBlock[]
}

export interface EditorChapter {
  id: number
  subjectId: number
  title: string
  order: number
  hidden: boolean
  lessons: EditorLesson[]
}
