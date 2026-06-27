import { ContentCouncilQuiz, QuizStatus, QuizExamType, QuizFormat } from '@/types/content-council/quiz'
import { mockSemesters } from './semesters.mock'

const mockSubjects = [
  { id: 1, code: 'WEB201', name: 'Lập trình Web cơ bản' },
  { id: 2, code: 'COM101', name: 'Nhập môn CNTT' },
  { id: 3, code: 'PRO192', name: 'Lập trình Java' },
  { id: 4, code: 'DBI202', name: 'Hệ quản trị CSDL' }
]

const titles = [
  'Quiz chương 1 - Tổng quan',
  'Quiz HTML và CSS cơ bản',
  'Kiểm tra giữa kỳ',
  'Kiểm tra cuối kỳ',
  'Quiz kiểm thử hộp đen',
  'Bài kiểm tra thường xuyên số 1',
  'Bài kiểm tra thường xuyên số 2',
  'Quiz chương 2 - Phân tích',
  'Quiz nâng cao',
  'Đề thi thử'
]

const examTypes: QuizExamType[] = ['lesson_quiz', 'chapter_quiz', 'midterm', 'final', 'regular_test']
const formats: QuizFormat[] = ['multiple_choice', 'essay', 'mixed']
const statuses: QuizStatus[] = ['draft', 'published', 'open', 'closed']

const getRandomElement = (arr: any[]) => arr[Math.floor(Math.random() * arr.length)]

const quizzes: ContentCouncilQuiz[] = []
let idCounter = 1

for (let i = 1; i <= 30; i++) {
  const subject = mockSubjects[i % mockSubjects.length]
  const semester = mockSemesters[i % mockSemesters.length]
  const format = getRandomElement(formats) as QuizFormat
  const status = getRandomElement(statuses) as QuizStatus
  const examType = getRandomElement(examTypes) as QuizExamType
  const title = `${getRandomElement(titles)} ${subject.name}`
  
  let qCount = 0
  let mcCount = 0
  let eCount = 0
  
  if (format === 'multiple_choice') {
    qCount = Math.floor(Math.random() * 30) + 10 // 10-40
    mcCount = qCount
  } else if (format === 'essay') {
    qCount = Math.floor(Math.random() * 3) + 1 // 1-3
    eCount = qCount
  } else {
    mcCount = Math.floor(Math.random() * 20) + 10
    eCount = Math.floor(Math.random() * 2) + 1
    qCount = mcCount + eCount
  }

  // Quiz chưa có câu hỏi
  if (i === 5 || i === 15) {
    qCount = 0
    mcCount = 0
    eCount = 0
  }

  const duration = qCount === 0 ? 0 : Math.floor(Math.random() * 4) * 15 + 15 // 15, 30, 45, 60
  
  const created = new Date(Date.now() - Math.random() * 10000000000)
  const updated = new Date(created.getTime() + Math.random() * 10000000)
  
  const openAt = status === 'open' || status === 'closed' ? new Date(updated.getTime() + 10000).toISOString() : undefined
  const closeAt = status === 'closed' ? new Date(updated.getTime() + 86400000).toISOString() : undefined

  quizzes.push({
    id: idCounter++,
    code: `QZ-${subject.code}-${i.toString().padStart(3, '0')}`,
    title: title,
    subjectId: subject.id,
    subjectCode: subject.code,
    subjectName: subject.name,
    semesterId: semester.id,
    semesterName: semester.name,
    status: status,
    examType: examType,
    format: format,
    durationMinutes: duration,
    questionCount: qCount,
    multipleChoiceQuestionCount: mcCount,
    essayQuestionCount: eCount,
    totalScore: 10,
    passingScore: 5,
    passMethod: 'score',
    unlimitedAttempts: i % 3 === 0,
    maximumAttempts: i % 3 === 0 ? undefined : 2,
    finalScoreMethod: 'highest',
    shuffleQuestions: true,
    shuffleAnswers: true,
    showResultAfterSubmit: true,
    showCorrectAnswerAfterSubmit: status !== 'open',
    usageCount: i % 4 === 0 ? Math.floor(Math.random() * 5) + 1 : 0,
    createdAt: created.toISOString(),
    updatedAt: updated.toISOString(),
    openAt: openAt,
    closeAt: closeAt
  })
}

// Thêm một số case cố định để test empty state hoặc báo lỗi
quizzes.push({
  id: idCounter++,
  code: 'QZ-DRAFT-001',
  title: 'Bản nháp mới tạo - Chưa có câu hỏi',
  subjectId: 1,
  subjectCode: 'WEB201',
  subjectName: 'Lập trình Web cơ bản',
  status: 'draft',
  examType: 'lesson_quiz',
  format: 'multiple_choice',
  durationMinutes: 0,
  questionCount: 0,
  multipleChoiceQuestionCount: 0,
  essayQuestionCount: 0,
  totalScore: 10,
  passMethod: 'score',
  unlimitedAttempts: true,
  finalScoreMethod: 'highest',
  shuffleQuestions: false,
  shuffleAnswers: false,
  showResultAfterSubmit: false,
  showCorrectAnswerAfterSubmit: false,
  usageCount: 0,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString()
})

export const mockQuizzes = quizzes
