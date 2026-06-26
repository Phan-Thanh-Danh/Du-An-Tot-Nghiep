export interface Subject {
  id: string
  code: string
  name: string
  description: string
  thumbnail: string
  status: 'active' | 'draft' | 'locked'
  chapterCount: number
  lessonCount: number
  contentCount: number
  quizCount: number
  updatedBy: string
  updatedAt: string
}

export type SubjectStatus = Subject['status']

export type SortKey = 'newest' | 'oldest' | 'nameAsc' | 'nameDesc'

export interface SubjectStats {
  total: number
  active: number
  draft: number
  locked: number
}
