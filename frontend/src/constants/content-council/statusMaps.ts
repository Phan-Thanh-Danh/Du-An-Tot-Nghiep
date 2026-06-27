import { ContentBlockType, ContentStatus, DifficultyLevel, SubjectStatus } from '@/types/content-council/common'
import { QuestionType, SelectionType, QuestionStatus } from '@/types/content-council/questionBank'
import { QuizStatus, QuizExamType, QuizFormat, QuizPassMethod, QuizFinalScoreMethod } from '@/types/content-council/quiz'

export const contentTypeUiMap: Record<ContentBlockType, { label: string, icon: string, color: string }> = {
  video: { label: 'Video', icon: 'Video', color: 'text-blue-600 bg-blue-50' },
  slide_html: { label: 'Slide', icon: 'Presentation', color: 'text-purple-600 bg-purple-50' },
  tai_lieu: { label: 'Tài liệu', icon: 'FileText', color: 'text-emerald-600 bg-emerald-50' },
  van_ban: { label: 'Văn bản', icon: 'AlignLeft', color: 'text-slate-600 bg-slate-50' },
  trac_nghiem: { label: 'Quiz', icon: 'CircleHelp', color: 'text-amber-600 bg-amber-50' }
}

export const subjectStatusUiMap: Record<SubjectStatus, { label: string, variant: string }> = {
  active: { label: 'Hoạt động', variant: 'success' },
  inactive: { label: 'Vô hiệu hóa', variant: 'danger' },
  archived: { label: 'Lưu trữ', variant: 'warning' }
}

export const contentStatusUiMap: Record<ContentStatus, { label: string, variant: string }> = {
  published: { label: 'Đã xuất bản', variant: 'success' },
  draft: { label: 'Bản nháp', variant: 'warning' },
  hidden: { label: 'Đang ẩn', variant: 'secondary' }
}

export const quizStatusUiMap: Record<QuizStatus, { label: string, variant: string }> = {
  published: { label: 'Đã xuất bản', variant: 'success' },
  draft: { label: 'Bản nháp', variant: 'warning' },
  open: { label: 'Đang mở', variant: 'info' },
  closed: { label: 'Đã đóng', variant: 'secondary' }
}

export const questionStatusUiMap: Record<QuestionStatus, { label: string, variant: string }> = {
  active: { label: 'Hoạt động', variant: 'success' },
  inactive: { label: 'Vô hiệu hóa', variant: 'danger' }
}

export const difficultyUiMap: Record<DifficultyLevel, { label: string, variant: string }> = {
  easy: { label: 'Dễ', variant: 'success' },
  medium: { label: 'Trung bình', variant: 'warning' },
  hard: { label: 'Khó', variant: 'danger' }
}

export const questionTypeUiMap: Record<QuestionType, string> = {
  multiple_choice: 'Trắc nghiệm',
  essay: 'Tự luận'
}

export const selectionTypeUiMap: Record<SelectionType, string> = {
  single: 'Chọn một đáp án',
  multiple: 'Chọn nhiều đáp án'
}

export const examTypeUiMap: Record<QuizExamType, string> = {
  lesson_quiz: 'Quiz bài học',
  chapter_quiz: 'Quiz cuối chương',
  midterm: 'Thi giữa kỳ',
  final: 'Thi cuối kỳ',
  regular_test: 'Kiểm tra định kỳ'
}

export const formatUiMap: Record<QuizFormat, string> = {
  multiple_choice: 'Trắc nghiệm',
  essay: 'Tự luận',
  mixed: 'Trắc nghiệm & Tự luận'
}

export const passMethodUiMap: Record<QuizPassMethod, string> = {
  score: 'Điểm số',
  correct_answer_count: 'Số câu đúng'
}

export const finalScoreMethodUiMap: Record<QuizFinalScoreMethod, string> = {
  highest: 'Điểm cao nhất',
  latest: 'Lần thi cuối',
  average: 'Điểm trung bình'
}
