import { ref } from 'vue'
import { teacherApi } from '@/services/teacherApi'

export function useProctoringSession() {
  const loading = ref(false)
  const error = ref('')
  const currentSession = ref(null)
  const currentStudents = ref([])

  const loadSessionData = async (sessionId) => {
    loading.value = true
    error.value = ''
    try {
      const examsData = await teacherApi.getExams()
      const rawSessions = Array.isArray(examsData) ? examsData : (examsData?.data?.items ?? examsData?.data ?? examsData?.items ?? [])
      currentSession.value = rawSessions.find(s => s.id === Number(sessionId)) || null

      if (!currentSession.value) {
         error.value = 'Ca thi không tồn tại hoặc không được phân công cho bạn.'
         return
      }

      const studentsData = await teacherApi.getExamStudents(sessionId)
      currentStudents.value = Array.isArray(studentsData) ? studentsData : (studentsData?.data?.items ?? studentsData?.data ?? studentsData?.items ?? [])
    } catch (e) {
      error.value = e?.message || 'Lỗi khi tải thông tin ca thi.'
    } finally {
      loading.value = false
    }
  }

  const formatSessionTime = (session) => {
    if (!session) return ''
    const start = new Date(session.startTime)
    const end = new Date(session.endTime)
    return `${start.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })} - ${end.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}`
  }

  const formatViolationTime = (value) => {
    if (!value) return '--:--:--'
    return new Date(value).toLocaleTimeString('vi-VN', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    })
  }

  const violationLabelMap = {
    TAB_SWITCH: 'Rời tab',
    FULLSCREEN_EXIT: 'Thoát toàn màn hình',
    CLIPBOARD_ATTEMPT: 'Copy/Paste',
    CONTEXT_MENU: 'Chuột phải',
    DEVTOOLS_OPENED: 'Developer Tools',
    FORBIDDEN_EXTENSION_RUNTIME: 'Extension bị cấm',
    KEYBOARD_SHORTCUT_ATTEMPT: 'Phím tắt bị cấm',
    SCREEN_STREAM_STOPPED: 'Mất stream màn hình',
  }

  const violationLabel = (type) => {
    return violationLabelMap[type] || type || 'Cảnh báo'
  }

  const streamLabel = (status) => {
    if (status === 'streaming') return 'Đang truyền'
    if (status === 'lost') return 'Mất tín hiệu'
    if (status === 'reconnecting') return 'Đang kết nối lại'
    if (status === 'stopped') return 'Đã dừng'
    return 'Chưa kết nối'
  }

  const examStatusLabel = (status) => {
    if (status === 'in_progress') return 'Đang làm'
    if (status === 'submitted') return 'Đã nộp bài'
    if (status === 'suspended') return 'Bị đình chỉ'
    return 'Chưa bắt đầu'
  }

  const attendanceLabel = (status) => {
    if (status === 'present') return 'Có mặt'
    if (status === 'exempted') return 'Miễn thi'
    return 'Vắng mặt'
  }

  const attendanceBadgeVariant = (status) => {
    if (status === 'present') return 'success'
    if (status === 'exempted') return 'neutral'
    return 'warning'
  }

  const preflightLabel = (status) => {
    if (status === 'pass') return 'Đạt'
    if (status === 'risk') return 'Rủi ro'
    return 'Chưa kiểm tra'
  }

  return {
    loading,
    error,
    currentSession,
    currentStudents,
    loadSessionData,
    formatSessionTime,
    formatViolationTime,
    violationLabelMap,
    violationLabel,
    streamLabel,
    examStatusLabel,
    attendanceLabel,
    attendanceBadgeVariant,
    preflightLabel,
  }
}
