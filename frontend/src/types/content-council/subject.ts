import { SubjectStatus } from './common'
import { EditorChapter } from './curriculum'

export interface ContentCouncilSubject {
  id: number
  code: string
  name: string
  thumbnailUrl: string
  status: SubjectStatus
  chapterCount: number
  lessonCount: number
  contentCount: number
  quizCount: number
  updatedAt: string
}

export interface SubjectContentSettings {
  id: number
  subjectId: number
  subjectCode: string
  subjectName: string
  thumbnailUrl: string
  status: SubjectStatus
  description: string
  allowStudentPreview: boolean
  completionRule: 'all_lessons' | 'pass_all_quizzes' | 'view_all_videos'
  requireSequentialLearning: boolean
  passingScoreScale: 10 | 100
  minimumPassingScore: number
}

export interface ContentCouncilSubjectDetail extends ContentCouncilSubject {
  description: string
  instructorCount: number
  studentCount: number
  publishedContentRatio: number
  createdAt: string
  isPublished: boolean
  chapters?: EditorChapter[]
}
