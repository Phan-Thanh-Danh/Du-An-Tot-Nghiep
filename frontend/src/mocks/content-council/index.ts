import { ContentCouncilSubject, ContentCouncilSubjectDetail } from '@/types/content-council/subject'
import { QuestionBankItem } from '@/types/content-council/questionBank'
import { ContentCouncilQuiz, QuizBuilderQuestion } from '@/types/content-council/quiz'

import { mockSubjects } from './subjects.mock'
import { mockSubjectDetails } from './subjectDetails.mock'
import { initialMockQuestions } from './questions.mock'
import { mockQuizzes } from './quizzes.mock'
import { contentCouncilQuizQuestions } from './quizQuestions.mock'

function deepClone<T>(obj: T): T {
  return JSON.parse(JSON.stringify(obj))
}

export function initializeSubjectMockData() {
  return {
    subjects: deepClone<ContentCouncilSubject[]>(mockSubjects),
    subjectDetails: deepClone<Record<number, ContentCouncilSubjectDetail>>(
      mockSubjectDetails.reduce((acc, detail) => {
        acc[detail.id] = detail
        return acc
      }, {} as Record<number, ContentCouncilSubjectDetail>)
    )
  }
}

export function initializeQuestionMockData() {
  return deepClone<QuestionBankItem[]>(initialMockQuestions)
}

export function initializeQuizMockData() {
  return {
    quizzes: deepClone<ContentCouncilQuiz[]>(mockQuizzes),
    quizQuestions: deepClone<Record<number, QuizBuilderQuestion[]>>(contentCouncilQuizQuestions)
  }
}
