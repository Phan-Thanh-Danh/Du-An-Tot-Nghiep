import { ref, Ref } from 'vue'
import { QuizFormData, QuizFormValidationResult } from '@/types/content-council/quizForm'

export function useQuizFormValidation(formData: Ref<QuizFormData>) {
  const isDirty = ref(false)
  const validationErrors = ref<Record<string, string>>({})
  const sectionErrors = ref<Record<string, string[]>>({})

  const validate = (): QuizFormValidationResult => {
    const data = formData.value
    const fErrors: Record<string, string> = {}
    const sErrors: Record<string, string[]> = {
      general: [],
      structure: [],
      passing: [],
      attempts: [],
      schedule: [],
      display: []
    }

    // 1. General
    if (!data.subjectId) {
      fErrors['subjectId'] = 'Vui lòng chọn môn học.'
      sErrors.general.push(fErrors['subjectId'])
    }
    if (!data.title || !data.title.trim()) {
      fErrors['title'] = 'Tiêu đề không được để trống.'
      sErrors.general.push(fErrors['title'])
    } else if (data.title.length > 255) {
      fErrors['title'] = 'Tiêu đề không được vượt quá 255 ký tự.'
      sErrors.general.push(fErrors['title'])
    }
    if (data.description && data.description.length > 1000) {
      fErrors['description'] = 'Mô tả không được vượt quá 1000 ký tự.'
      sErrors.general.push(fErrors['description'])
    }

    // 2. Structure
    if (data.durationMinutes < 1 || data.durationMinutes > 240) {
      fErrors['durationMinutes'] = 'Thời gian làm bài phải từ 1 đến 240 phút.'
      sErrors.structure.push(fErrors['durationMinutes'])
    }
    
    if (data.format === 'mixed') {
      if (data.multipleChoicePercentage <= 0 || data.essayPercentage <= 0) {
        fErrors['percentages'] = 'Tỷ lệ câu hỏi mỗi loại phải lớn hơn 0%.'
        sErrors.structure.push(fErrors['percentages'])
      }
      if (data.multipleChoicePercentage + data.essayPercentage !== 100) {
        fErrors['percentages'] = 'Tổng tỷ lệ trắc nghiệm và tự luận phải bằng 100%.'
        sErrors.structure.push(fErrors['percentages'])
      }
    }

    // 3. Passing Rules
    if (data.totalScore <= 0) {
      fErrors['totalScore'] = 'Tổng điểm phải lớn hơn 0.'
      sErrors.passing.push(fErrors['totalScore'])
    }

    if (data.passMethod === 'score') {
      if (data.passingScore === null || data.passingScore < 0) {
        fErrors['passingScore'] = 'Vui lòng nhập điểm đạt hợp lệ.'
        sErrors.passing.push(fErrors['passingScore'])
      } else if (data.passingScore > data.totalScore) {
        fErrors['passingScore'] = 'Điểm đạt không được lớn hơn tổng điểm.'
        sErrors.passing.push(fErrors['passingScore'])
      }
    } else if (data.passMethod === 'correct_answer_count') {
      if (data.minimumCorrectAnswers === null || data.minimumCorrectAnswers <= 0) {
        fErrors['minimumCorrectAnswers'] = 'Số câu đúng tối thiểu phải lớn hơn 0.'
        sErrors.passing.push(fErrors['minimumCorrectAnswers'])
      }
    }

    // 4. Attempts
    if (!data.unlimitedAttempts) {
      if (!data.maximumAttempts || data.maximumAttempts < 1 || data.maximumAttempts > 100) {
        fErrors['maximumAttempts'] = 'Số lượt làm tối đa phải từ 1 đến 100.'
        sErrors.attempts.push(fErrors['maximumAttempts'])
      }
    }

    // 5. Schedule
    if (data.openAt && data.closeAt) {
      const openDate = new Date(data.openAt)
      const closeDate = new Date(data.closeAt)
      if (closeDate <= openDate) {
        fErrors['closeAt'] = 'Thời gian đóng phải sau thời gian mở.'
        sErrors.schedule.push(fErrors['closeAt'])
      }
    }

    validationErrors.value = fErrors
    sectionErrors.value = sErrors

    const isValid = Object.keys(fErrors).length === 0

    return {
      isValid,
      fieldErrors: fErrors,
      sectionErrors: sErrors
    }
  }

  const resetValidation = () => {
    validationErrors.value = {}
    sectionErrors.value = {}
  }

  return {
    isDirty,
    validationErrors,
    sectionErrors,
    validate,
    resetValidation
  }
}
