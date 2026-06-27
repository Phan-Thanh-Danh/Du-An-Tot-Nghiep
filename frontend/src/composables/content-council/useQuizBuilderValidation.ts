import { QuizBuilderQuestion, ContentCouncilQuiz } from '@/types/content-council/quiz'

export interface QuizBuilderValidationResult {
  canSave: boolean
  canPublish: boolean
  errors: string[]
  warnings: string[]
  metrics: {
    questionCount: number
    multipleChoiceCount: number
    essayCount: number
    currentScore: number
    configuredScore: number
    multipleChoicePercentage: number
    essayPercentage: number
  }
}

export function useQuizBuilderValidation() {
  
  const validate = (quiz: ContentCouncilQuiz, questions: QuizBuilderQuestion[]): QuizBuilderValidationResult => {
    const errors: string[] = []
    const warnings: string[] = []
    
    // Metrics calculation
    const questionCount = questions.length
    const multipleChoiceCount = questions.filter(q => q.questionType === 'multiple_choice').length
    const essayCount = questions.filter(q => q.questionType === 'essay').length
    
    // JS math precision issue fix
    const currentScore = Number(questions.reduce((sum, q) => sum + (q.score || 0), 0).toFixed(2))
    const configuredScore = quiz.totalScore || 0
    
    let multipleChoicePercentage = 0
    let essayPercentage = 0
    
    if (questionCount > 0) {
      multipleChoicePercentage = Math.round((multipleChoiceCount / questionCount) * 100)
      essayPercentage = 100 - multipleChoicePercentage
    }

    // Checking Conditions
    
    // 1. Minimum requirements
    if (questionCount === 0) {
      errors.push('Quiz chưa có câu hỏi nào.')
    }
    
    // 2. Score mapping
    if (currentScore !== configuredScore && questionCount > 0) {
      const diff = Math.abs(currentScore - configuredScore)
      if (currentScore < configuredScore) {
        errors.push(`Tổng điểm hiện tại (${currentScore}) đang thiếu ${diff.toFixed(2)} điểm so với cấu hình.`)
      } else {
        errors.push(`Tổng điểm hiện tại (${currentScore}) đang vượt quá ${diff.toFixed(2)} điểm so với cấu hình.`)
      }
    }
    
    // 3. Question Count matching
    if (questionCount !== quiz.questionCount && questionCount > 0) {
      warnings.push(`Số lượng câu hỏi thực tế (${questionCount}) khác với cấu hình (${quiz.questionCount}).`)
    }

    // 4. Duplicate questions
    const uniqueIds = new Set(questions.map(q => q.questionId))
    if (uniqueIds.size !== questionCount) {
      errors.push('Tồn tại câu hỏi bị trùng lặp trong Quiz.')
    }
    
    // 5. Inactive questions
    const hasInactive = questions.some(q => q.status === 'inactive')
    if (hasInactive) {
      errors.push('Quiz chứa câu hỏi đã bị vô hiệu hóa trong Ngân hàng câu hỏi.')
    }

    // 6. Subject mismatch
    const hasWrongSubject = questions.some(q => q.subjectId !== quiz.subjectId)
    if (hasWrongSubject) {
      errors.push('Quiz chứa câu hỏi không thuộc môn học hiện tại.')
    }

    // 7. Invalid scores or order
    const hasInvalidScore = questions.some(q => !q.score || q.score <= 0)
    if (hasInvalidScore) {
      errors.push('Có câu hỏi chưa được thiết lập điểm hợp lệ (>0).')
    }

    // 8. Format matching
    if (quiz.format === 'multiple_choice' && essayCount > 0) {
      errors.push('Hình thức Quiz là Trắc nghiệm nhưng đang chứa câu hỏi Tự luận.')
    } else if (quiz.format === 'essay' && multipleChoiceCount > 0) {
      errors.push('Hình thức Quiz là Tự luận nhưng đang chứa câu hỏi Trắc nghiệm.')
    } else if (quiz.format === 'mixed') {
      if (multipleChoiceCount === 0 || essayCount === 0) {
        errors.push('Hình thức Quiz là Hỗn hợp nhưng thiếu câu hỏi Trắc nghiệm hoặc Tự luận.')
      } else {
        // Warning if ratio is slightly off, Error if completely wrong
        const configMcRatio = quiz.multipleChoiceQuestionCount && quiz.questionCount 
          ? Math.round((quiz.multipleChoiceQuestionCount / quiz.questionCount) * 100) 
          : 70
        
        const diff = Math.abs(multipleChoicePercentage - configMcRatio)
        if (diff > 15) {
          warnings.push(`Tỷ lệ câu hỏi Trắc nghiệm thực tế (${multipleChoicePercentage}%) chênh lệch nhiều so với cấu hình (${configMcRatio}%).`)
        }
      }
    }

    const canPublish = errors.length === 0
    const canSave = !hasWrongSubject && !hasInvalidScore && uniqueIds.size === questionCount

    return {
      canSave,
      canPublish,
      errors,
      warnings,
      metrics: {
        questionCount,
        multipleChoiceCount,
        essayCount,
        currentScore,
        configuredScore,
        multipleChoicePercentage,
        essayPercentage
      }
    }
  }

  return {
    validate
  }
}
