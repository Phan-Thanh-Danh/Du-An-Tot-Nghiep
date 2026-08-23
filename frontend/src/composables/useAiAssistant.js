import { ref } from 'vue'

const isOpen = ref(false)
const pendingPrompt = ref('')
const currentContext = ref({ courseId: null, lessonId: null })

/**
 * Composable quản lý trạng thái hiển thị và kích hoạt AI Assistant từ bất kỳ màn hình nào
 */
export function useAiAssistant() {
  /**
   * Mở AI Assistant kèm câu hỏi tự động điền và ngữ cảnh môn/bài học
   * @param {string} prompt - Câu hỏi cần gửi hoặc điền sẵn
   * @param {Object} [context] - Ngữ cảnh bổ sung { courseId, lessonId }
   */
  function openWithPrompt(prompt, context = {}) {
    pendingPrompt.value = prompt
    currentContext.value = { ...context }
    isOpen.value = true
  }

  function toggle() {
    isOpen.value = !isOpen.value
  }

  function open() {
    isOpen.value = true
  }

  function close() {
    isOpen.value = false
  }

  return {
    isOpen,
    pendingPrompt,
    currentContext,
    openWithPrompt,
    toggle,
    open,
    close,
  }
}
