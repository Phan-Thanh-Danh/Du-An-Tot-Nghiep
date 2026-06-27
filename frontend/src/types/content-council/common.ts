export type ContentBlockType = 'video' | 'slide_html' | 'tai_lieu' | 'van_ban' | 'trac_nghiem'
export type SubjectStatus = 'active' | 'inactive' | 'archived'
export type ContentStatus = 'draft' | 'published' | 'hidden'
export type DifficultyLevel = 'easy' | 'medium' | 'hard'

export interface ApiErrorResponse {
  message: string
  code?: string
  details?: Record<string, string[]>
}
