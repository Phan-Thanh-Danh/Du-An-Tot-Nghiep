import { QuizBuilderQuestion } from '@/types/content-council/quiz'
import { initialMockQuestions } from './questions.mock'

export const contentCouncilQuizQuestions: Record<number, QuizBuilderQuestion[]> = {}

// Helper to convert QuestionBankItem to QuizBuilderQuestion
const convertToBuilderQuestion = (qId: number, order: number, score: number = 1): QuizBuilderQuestion | null => {
  const q = initialMockQuestions.find(x => x.id === qId)
  if (!q) return null
  
  return {
    questionId: q.id,
    questionCode: q.code,
    questionContent: q.content,
    subjectId: q.subjectId,
    questionType: q.type,
    selectionType: q.selectionType,
    difficulty: q.difficulty,
    score: score,
    order: order,
    status: q.status,
    choices: q.choices,
    correctAnswerIds: q.correctAnswerIds,
    answerExplanation: q.answerExplanation,
    sampleAnswer: q.sampleAnswer
  }
}

// 1. Quiz 1 (WEB201): has 10 questions, all multiple_choice, score = 1
const quiz1Questions: QuizBuilderQuestion[] = []
for (let i = 1; i <= 10; i++) {
  const bq = convertToBuilderQuestion(i, i, 1)
  if (bq) quiz1Questions.push(bq)
}
contentCouncilQuizQuestions[1] = quiz1Questions

// 2. Quiz 2 (COM101): has 5 questions, but score is messed up (e.g., total score is 10 but we only have 5 questions * 1 point = 5 points)
const quiz2Questions: QuizBuilderQuestion[] = []
for (let i = 11; i <= 15; i++) {
  const bq = convertToBuilderQuestion(i, i - 10, 1)
  if (bq) quiz2Questions.push(bq)
}
contentCouncilQuizQuestions[2] = quiz2Questions

// 3. Quiz 3 (PRO192): has 2 essay questions, score = 5 each
const quiz3Questions: QuizBuilderQuestion[] = []
let order3 = 1
for (let i = 19; i <= 20; i++) {
  const bq = convertToBuilderQuestion(i, order3++, 5)
  if (bq) quiz3Questions.push(bq)
}
contentCouncilQuizQuestions[3] = quiz3Questions

// 4. Quiz 4 (DBI202): mixed questions (1 multiple choice, 1 essay). 
// One of them is inactive to test validation.
const quiz4Questions: QuizBuilderQuestion[] = []
const qDBI1 = convertToBuilderQuestion(25, 1, 4) // id 25 is DBI202 multiple_choice (i=1 -> isMultiple=false -> wait, i=2 is multiple choice)
const qDBI2 = convertToBuilderQuestion(26, 2, 6)
if (qDBI1) quiz4Questions.push(qDBI1)
if (qDBI2) quiz4Questions.push(qDBI2)
contentCouncilQuizQuestions[4] = quiz4Questions

// Quiz 5 is explicitly set to have 0 questions in quiz mock (qCount = 0).
contentCouncilQuizQuestions[5] = []

// Quiz 31 (QZ-DRAFT-001) - newly created draft - empty
contentCouncilQuizQuestions[31] = []

// Fallback for others if needed, though they will just be initialized empty in composable.
